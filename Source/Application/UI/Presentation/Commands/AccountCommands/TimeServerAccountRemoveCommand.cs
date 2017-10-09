﻿using pdfforge.Obsidian.Trigger;
using pdfforge.PDFCreator.Conversion.Settings;
using pdfforge.PDFCreator.Core.Services.Macros;
using pdfforge.PDFCreator.UI.Interactions;
using pdfforge.PDFCreator.UI.Interactions.Enums;
using pdfforge.PDFCreator.UI.Presentation.Helper.Translation;
using pdfforge.PDFCreator.UI.Presentation.UserControls.Accounts.AccountViews;
using pdfforge.PDFCreator.UI.Presentation.UserControls.Profiles;
using pdfforge.PDFCreator.UI.Presentation.ViewModelBases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace pdfforge.PDFCreator.UI.Presentation.Commands
{
    public class TimeServerAccountRemoveCommand : TranslatableCommandBase<TimeServerTranslation>, IWaitableCommand
    {
        private readonly IInteractionRequest _interactionRequest;
        private readonly ICurrentSettingsProvider _currentSettingsProvider;
        private ObservableCollection<TimeServerAccount> TimeServerAccounts => _currentSettingsProvider.Settings?.ApplicationSettings?.Accounts?.TimeServerAccounts ?? new ObservableCollection<TimeServerAccount>();
        private readonly ObservableCollection<ConversionProfile> _profiles;
        private TimeServerAccount _currentAccount;
        private List<ConversionProfile> _usedInProfilesList;

        public TimeServerAccountRemoveCommand(IInteractionRequest interactionRequest, ICurrentSettingsProvider currentSettingsProvider,
            ITranslationUpdater translationUpdater)
           : base(translationUpdater)
        {
            _interactionRequest = interactionRequest;
            _currentSettingsProvider = currentSettingsProvider;
            _profiles = currentSettingsProvider?.Profiles;

            TimeServerAccounts.CollectionChanged += (sender, args) => RaiseCanExecuteChanged();
        }

        public override bool CanExecute(object parameter)
        {
            return TimeServerAccounts?.Count > 0;
        }

        public override void Execute(object parameter)
        {
            _currentAccount = parameter as TimeServerAccount;
            if (_currentAccount == null)
                return;

            _usedInProfilesList = _profiles.Where(p => p.PdfSettings.Signature.TimeServerAccountId.Equals(_currentAccount.AccountId)).ToList();

            var title = Translation.RemoveTimeServerAccount;

            var messageSb = new StringBuilder();
            messageSb.AppendLine(_currentAccount.AccountInfo);
            messageSb.AppendLine(Translation.SureYouWantToDeleteAccount);

            if (_usedInProfilesList.Count > 0)
            {
                messageSb.AppendLine();
                messageSb.AppendLine(Translation.GetAccountIsUsedInFollowingMessage(_usedInProfilesList.Count));
                messageSb.AppendLine();
                foreach (var profile in _usedInProfilesList)
                {
                    messageSb.AppendLine(profile.Name);
                }
                messageSb.AppendLine();
                messageSb.AppendLine(Translation.GetTimeServerGetsDisabledMessage(_usedInProfilesList.Count));
            }
            var message = messageSb.ToString();
            var icon = _usedInProfilesList.Count > 0 ? MessageIcon.Warning : MessageIcon.Question;
            var interaction = new MessageInteraction(message, title, MessageOptions.YesNo, icon);
            _interactionRequest.Raise(interaction, DeleteAccountCallback);
        }

        private void DeleteAccountCallback(MessageInteraction interaction)
        {
            if (interaction.Response != MessageResponse.Yes)
            {
                IsDone?.Invoke(this, new MacroCommandIsDoneEventArgs(ResponseStatus.Cancel));
                return;
            }

            if (TimeServerAccounts.Contains(_currentAccount))
                TimeServerAccounts.Remove(_currentAccount);

            foreach (var profile in _usedInProfilesList)
            {
                profile.PdfSettings.Signature.TimeServerAccountId = "";
                profile.PdfSettings.Signature.Enabled = false;
            }

            IsDone?.Invoke(this, new MacroCommandIsDoneEventArgs(ResponseStatus.Success));
        }

        public event EventHandler<MacroCommandIsDoneEventArgs> IsDone;
    }
}
