﻿using pdfforge.Obsidian;
using pdfforge.PDFCreator.UI.Presentation.DesignTime.Helper;
using pdfforge.PDFCreator.UI.Presentation.Help;
using pdfforge.PDFCreator.UI.Presentation.Helper.Translation;
using pdfforge.PDFCreator.Utilities.UserGuide;
using System.Windows.Input;

namespace pdfforge.PDFCreator.UI.Presentation.UserControls.Profiles.Advanced.UserToken
{
    public class UserTokenUserControlViewModel : ProfileUserControlViewModel<UserTokenTranslation>
    {
        public UserTokenUserControlViewModel(ITranslationUpdater translationUpdater, ISelectedProfileProvider profile, IUserGuideLauncher userGuideLauncher) : base(translationUpdater, profile)
        {
            OpenUserGuideCommand = new DelegateCommand(o => userGuideLauncher?.ShowHelpTopic(HelpTopic.UserTokens));
        }

        public ICommand OpenUserGuideCommand { get; private set; }
    }

    public class DesignTimeUserTokenUserControlViewModel : UserTokenUserControlViewModel
    {
        public DesignTimeUserTokenUserControlViewModel() : base(new DesignTimeTranslationUpdater(), new DesignTimeCurrentSettingsProvider(), new UserGuideLauncher())
        {
        }
    }
}
