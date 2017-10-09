﻿using pdfforge.PDFCreator.UI.Presentation.DesignTime.Helper;
using pdfforge.PDFCreator.UI.Presentation.UserControls.PrintJob;
using pdfforge.PDFCreator.Utilities;

namespace pdfforge.PDFCreator.UI.Presentation.DesignTime
{
    internal class DesignTimeSignaturePasswordsViewModel : SignaturePasswordViewModel
    {
        public DesignTimeSignaturePasswordsViewModel() : base(new DesignTimeTranslationUpdater(), new SignaturePasswordCheck())
        {
            CertificatePath = "X:\\CertificateFile.psx";
        }

        public override PrintJobPasswordButtonViewModel PrintJobPasswordButtonViewModel => new DesignTimePrintJobPasswordButtonViewModel();

        protected override void CancelAction()
        {
        }

        protected override void RemoveAction()
        {
        }

        protected override void OkAction()
        {
        }

        protected override void CloseAction()
        {
        }
    }
}
