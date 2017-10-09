﻿using pdfforge.PDFCreator.Conversion.Settings;
using pdfforge.PDFCreator.Utilities.Tokens;
using System;
using System.Collections.Generic;

namespace pdfforge.PDFCreator.Conversion.Jobs.Jobs
{
    /// <summary>
    ///     AbstractJob implements a few methods of the Job interface that can be shared among the Job types
    /// </summary>
    public class Job
    {
        public Job(JobInfo.JobInfo jobInfo, ConversionProfile profile, JobTranslations jobTranslations, Accounts accounts)
        {
            JobTranslations = jobTranslations;
            JobInfo = jobInfo;
            Profile = profile;
            Accounts = accounts;
        }

        /// <summary>
        ///     JobInfo that defines the current Job (source files, Metadata etc)
        /// </summary>
        public JobInfo.JobInfo JobInfo { get; }

        /// <summary>
        ///     The Profile settings that are used in the job
        /// </summary>
        public ConversionProfile Profile { get; set; }

        /// <summary>
        ///     All currently available accounts (Dropbox, FTP, )
        /// </summary>
        public Accounts Accounts { get; set; }

        public TokenReplacer TokenReplacer { get; set; } = new TokenReplacer();

        /// <summary>
        ///     Holds passwords for encryption etc.
        /// </summary>
        public JobPasswords Passwords { get; set; } = new JobPasswords();

        /// <summary>
        ///     Holds translations required during the job
        /// </summary>
        public JobTranslations JobTranslations { get; set; }

        /// <summary>
        ///     The number of copies requested for the print job
        /// </summary>
        public int NumberOfCopies { get; set; }

        /// <summary>
        ///     The number of pages in the print job including cover and attachment pages
        /// </summary>
        public int NumberOfPages { get; set; }

        /// <summary>
        ///     The Output files that have been generated by this job
        /// </summary>
        public IList<string> OutputFiles { get; set; } = new List<string>();

        /// <summary>
        ///     The template for the output files. This may contain a wildcard to create multiple files, i.e. a file per page. The
        ///     template is used to construct the final output filename.
        /// </summary>
        public string OutputFilenameTemplate { get; set; }

        /// <summary>
        ///     The folder in which the job can store temporary data
        /// </summary>
        public string JobTempFolder { get; set; }

        /// <summary>
        ///     The folder in which the job produces the output files
        /// </summary>
        public string JobTempOutputFolder { get; set; }

        /// <summary>
        ///     Temporary filename of the output file with extension
        /// </summary>
        public string JobTempFileName { get; set; } = "output";

        /// <summary>
        ///     Flag to skip the SaveFileDialog (Therefore an OutputFilename must be set)
        /// </summary>
        public bool SkipSaveFileDialog { get; set; }

        /// <summary>
        /// ShareLinks from upload actions like Dropbox
        /// </summary>
        public JobShareLinks ShareLinks { get; set; } = new JobShareLinks();

        /// <summary>
        ///     If true, the job has completed execution
        /// </summary>
        public bool Completed { get; set; }

        /// <summary>
        ///     A list of output files produced during the conversion
        /// </summary>
        public IList<string> TempOutputFiles { get; set; } = new List<string>();

        public event EventHandler<JobCompletedEventArgs> OnJobCompleted;

        public event EventHandler<JobProgressChangedEventArgs> OnJobProgressChanged;

        public event EventHandler<JobLoginFailedEventArgs> OnJobHasError;

        public void InitMetadataWithTemplates()
        {
            JobInfo.Metadata.Author = Profile.AuthorTemplate;
            JobInfo.Metadata.Title = Profile.TitleTemplate;
            JobInfo.Metadata.Subject = Profile.SubjectTemplate;
            JobInfo.Metadata.Keywords = Profile.KeywordTemplate;
        }

        public void ReplaceTokensInMetadata()
        {
            JobInfo.Metadata.Author = TokenReplacer.ReplaceTokens(JobInfo.Metadata.Author);
            JobInfo.Metadata.Title = TokenReplacer.ReplaceTokens(JobInfo.Metadata.Title);
            JobInfo.Metadata.Subject = TokenReplacer.ReplaceTokens(JobInfo.Metadata.Subject);
            JobInfo.Metadata.Keywords = TokenReplacer.ReplaceTokens(JobInfo.Metadata.Keywords);
        }

        public void CallJobCompleted()
        {
            Completed = true;

            OnJobCompleted?.Invoke(this, new JobCompletedEventArgs(this));
        }

        public void ReportProgress(int percentProgress)
        {
            OnJobProgressChanged?.Invoke(this, new JobProgressChangedEventArgs(this, percentProgress));
        }

        public void OnErrorDuringLogin(Action<string> continueAction, Action abortAction, string actionDisplayName)
        {
            if (OnJobHasError == null)
                abortAction();
            else
                OnJobHasError(this, new JobLoginFailedEventArgs(continueAction, abortAction, actionDisplayName));
        }
    }
}
