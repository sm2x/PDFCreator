﻿using pdfforge.Obsidian;
using pdfforge.PDFCreator.UI.Interactions;
using pdfforge.PDFCreator.UI.Presentation.Helper.Translation;
using System;
using System.Windows.Input;
using Translatable;

namespace pdfforge.PDFCreator.UI.Presentation.ViewModelBases
{
    public abstract class AccountViewModelBase<TInteraction, TTranslatable>
        : OverlayViewModelBase<TInteraction, TTranslatable>
        where TTranslatable : ITranslatable, new()
        where TInteraction : AccountInteractionBase
    {
        public override string Title => Interaction.Title;

        public DelegateCommand SaveCommand { get; }

        public ICommand CancelCommand { get; }

        private bool _askForPasswordLater;

        public bool AskForPasswordLater
        {
            get { return _askForPasswordLater; }
            set
            {
                _askForPasswordLater = value;
                if (value)
                    SetPasswordAction("");
                SaveCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged(nameof(AskForPasswordLater));
            }
        }

        protected AccountViewModelBase(ITranslationUpdater translationUpdater) : base(translationUpdater)
        {
            SaveCommand = new DelegateCommand(o => SaveExecute(), o => SaveCanExecute());
            CancelCommand = new DelegateCommand(CancelExecute);
        }

        protected abstract void SaveExecute();

        protected abstract bool SaveCanExecute();

        private void CancelExecute(object obj)
        {
            Interaction.Success = false;
            FinishInteraction();
        }

        public Action<string> SetPasswordAction { get; set; }
    }
}
