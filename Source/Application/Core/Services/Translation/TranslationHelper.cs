﻿using NGettext;
using pdfforge.DataStorage;
using pdfforge.DataStorage.Storage;
using pdfforge.PDFCreator.Conversion.Settings;
using pdfforge.PDFCreator.Conversion.Settings.GroupPolicies;
using pdfforge.PDFCreator.Core.SettingsManagement;
using pdfforge.PDFCreator.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Translatable;
using Translatable.NGettext;

namespace pdfforge.PDFCreator.Core.Services.Translation
{
    /// <summary>
    ///     TranslationUtil provides functionality that is used in conjunction with the DynamicTranslator classes.
    /// </summary>
    public class BaseTranslationHelper : ILanguageProvider
    {
        private readonly IAssemblyHelper _assemblyHelper;
        protected readonly TranslationFactory TranslationFactory;
        private ILanguageLoader _languageLoader;

        // ReSharper disable once MemberCanBeProtected.Global
        public BaseTranslationHelper(IAssemblyHelper assemblyHelper, TranslationFactory translationFactory)
        {
            _assemblyHelper = assemblyHelper;
            TranslationFactory = translationFactory;
        }

        public Language CurrentLanguage { get; private set; }

        private ILanguageLoader LanguageLoader
        {
            get
            {
                if (_languageLoader == null)
                    _languageLoader = BuildLanguageLoader();

                return _languageLoader;
            }
            set { _languageLoader = value; }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public List<string> PossibleLanguagePaths { get; set; } = new List<string> { "Languages", @"..\..\..\..\..\Languages" };

        public IEnumerable<Language> GetAvailableLanguages()
        {
            return LanguageLoader.GetAvailableLanguages().OrderBy(t => t.NativeName);
        }

        public bool HasTranslation(string language)
        {
            return LanguageLoader.GetTranslationFileIfExists(language) != null;
        }

        public Language FindBestLanguage(CultureInfo culture)
        {
            return LanguageLoader.FindBestLanguage(culture);
        }

        /// <summary>
        ///     Initialize the Translator for later use in the application
        /// </summary>
        /// <param name="languageName">Language to use for initialization</param>
        public void InitTranslator(string languageName)
        {
            LanguageLoader = BuildLanguageLoader();

            CurrentLanguage = FindBestLanguage(languageName);
            TranslationFactory.TranslationSource = BuildTranslationSource(CurrentLanguage);
        }

        public ITranslationSource BuildTranslationSource(Language language)
        {
            var catalogBuilder = new GettextCatalogBuilder(LanguageLoader.TranslationFolder);
            var catalog = (Catalog)catalogBuilder.GetCatalog("messages", language.Iso2);
            return new GettextTranslationSource(catalog);
        }

        public Language FindBestLanguage(string languageName)
        {
            if (string.IsNullOrEmpty(languageName))
                languageName = "en";

            try
            {
                var cultureInfo = new CultureInfo(languageName);
                return LanguageLoader.FindBestLanguage(cultureInfo);
            }
            catch (CultureNotFoundException)
            {
            }

            var language = LanguageLoader.GetAvailableLanguages().FirstOrDefault(x => x.Iso2 == languageName);
            if (language != null)
                return language;

            return LanguageLoader.FindBestLanguage(new CultureInfo("en"));
        }

        private Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private Data ReadEnglishTranslation()
        {
            using (var stream = GenerateStreamFromString(TranslationResources.English))
            {
                var data = Data.CreateDataStorage();
                var iniStorage = new IniStorage();
                iniStorage.SetData(data);
                iniStorage.ReadData(stream, clear: true);
                return data;
            }
        }

        /// <summary>
        ///     Initialize an empty translator (i.e. for tests)
        /// </summary>
        public void InitEmptyTranslator()
        {
            _languageLoader = BuildLanguageLoader();
        }

        private ILanguageLoader BuildLanguageLoader()
        {
            var appDir = _assemblyHelper.GetAssemblyDirectory();

            var translationPathCandidates = PossibleLanguagePaths.Select(path => Path.Combine(appDir, path)).ToArray();

            return new GettextLanguageLoader(translationPathCandidates);
        }
    }

    public interface ITranslationHelper : ILanguageProvider
    {
        /// <summary>
        ///     Temporarily sets a translation while storing the old translator for later use. Use RevertTemporaryTranslation to
        ///     revert to the initial translator.
        /// </summary>
        /// <param name="language">The language definition to use</param>
        /// <returns>true, if the translation was successfully loaded</returns>
        bool SetTemporaryTranslation(Language language);

        /// <summary>
        ///     Reverts a temporarily set translation to it's original. If no temporary translation has been set, nothing will be
        ///     reverted.
        /// </summary>
        void RevertTemporaryTranslation();

        /// <summary>
        ///     Translates a profile list by searching for predefined translations based on their GUID and apply the translated
        ///     name to them
        /// </summary>
        /// <param name="profiles">The profile list</param>
        void TranslateProfileList(IList<ConversionProfile> profiles);

        List<string> PossibleLanguagePaths { get; set; }

        /// <summary>
        ///     Initialize the Translator for later use in the application
        /// </summary>
        /// <param name="languageName">Language to use for initialization</param>
        void InitTranslator(string languageName);

        ITranslationSource BuildTranslationSource(Language language);

        /// <summary>
        ///     Initialize an empty translator (i.e. for tests)
        /// </summary>
        void InitEmptyTranslator();
    }

    public class TranslationHelper : BaseTranslationHelper, ITranslationHelper
    {
        private readonly IGpoSettings _gpoSettings;

        public TranslationHelper(ISettingsProvider settingsProvider, IAssemblyHelper assemblyHelper, TranslationFactory translationFactory, IGpoSettings gpoSettings) : base(assemblyHelper, translationFactory)
        {
            _gpoSettings = gpoSettings;
            settingsProvider.LanguageChanged += SettingsProviderOnLanguageChanged;
        }

        private void SettingsProviderOnLanguageChanged(object sender, LanguageChangedEventArgs eventArgs)
        {
            var settings = eventArgs.Settings;
            if (!string.IsNullOrEmpty(_gpoSettings?.Language))
            {
                InitTranslator(_gpoSettings.Language);
            }
            else
            {
                InitTranslator(settings.ApplicationSettings.Language);
            }
            TranslateProfileList(settings.ConversionProfiles);
        }

        /// <summary>
        ///     Temporarily sets a translation while storing the old translator for later use. Use RevertTemporaryTranslation to
        ///     revert to the initial translator.
        /// </summary>
        /// <param name="language">The language definition to use</param>
        /// <returns>true, if the translation was successfully loaded</returns>
        public bool SetTemporaryTranslation(Language language)
        {
            if (!HasTranslation(language.Iso2))
                return false;

            if (_tmpTranslationSource == null)
                _tmpTranslationSource = TranslationFactory.TranslationSource;

            TranslationFactory.TranslationSource = BuildTranslationSource(language);

            return true;
        }

        private ITranslationSource _tmpTranslationSource;

        /// <summary>
        ///     Reverts a temporarily set translation to it's original. If no temporary translation has been set, nothing will be
        ///     reverted.
        /// </summary>
        public void RevertTemporaryTranslation()
        {
            if (_tmpTranslationSource != null)
                TranslationFactory.TranslationSource = _tmpTranslationSource;

            _tmpTranslationSource = null;
        }

        /// <summary>
        ///     Translates a profile list by searching for predefined translations based on their GUID and apply the translated
        ///     name to them
        /// </summary>
        /// <param name="profiles">The profile list</param>
        public void TranslateProfileList(IList<ConversionProfile> profiles)
        {
            foreach (var p in profiles)
                try
                {
                    var translation = TranslationFactory.CreateTranslation<ProfileNameByGuidTranslation>().GetProfileGuidTranslation(p.Guid);
                    if (!string.IsNullOrEmpty(translation))
                        p.Name = translation;
                }
                catch (ArgumentException)
                {
                    //do nothing, profile must not be renamed
                }
        }
    }
}
