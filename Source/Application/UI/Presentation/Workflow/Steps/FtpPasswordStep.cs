﻿using pdfforge.PDFCreator.Conversion.Jobs.Jobs;
using pdfforge.PDFCreator.UI.Presentation.UserControls.PrintJob;

namespace pdfforge.PDFCreator.UI.Presentation.Workflow.Steps
{
    public class FtpPasswordStep : WorkflowStepBase
    {
        public override string NavigationUri => nameof(FtpPasswordView);

        public override bool IsStepRequired(Job job)
        {
            if (!job.Profile.Ftp.Enabled)
                return false;

            return string.IsNullOrEmpty(job.Passwords.FtpPassword);
        }
    }
}
