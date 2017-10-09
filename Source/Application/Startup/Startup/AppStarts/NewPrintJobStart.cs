﻿using NLog;
using pdfforge.PDFCreator.Conversion.Jobs.FolderProvider;
using pdfforge.PDFCreator.Conversion.Jobs.JobInfo;
using pdfforge.PDFCreator.Conversion.Settings;
using pdfforge.PDFCreator.Core.SettingsManagement;
using pdfforge.PDFCreator.Core.Workflow;
using pdfforge.PDFCreator.Utilities.IO;
using System;
using System.IO;

namespace pdfforge.PDFCreator.Core.Startup.AppStarts
{
    public class NewPrintJobStart : MaybePipedStart
    {
        private readonly IJobInfoManager _jobInfoManager;
        private readonly IJobInfoQueue _jobInfoQueue;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly ISettingsProvider _settingsProvider;
        private readonly ISpoolerProvider _spoolerProvider;

        public NewPrintJobStart(ISettingsProvider settingsProvider, IJobInfoQueue jobInfoQueue, ISpoolerProvider spoolerProvider, IMaybePipedApplicationStarter maybePipedApplicationStarter, IJobInfoManager jobInfoManager)
            : base(maybePipedApplicationStarter)
        {
            _settingsProvider = settingsProvider;
            _jobInfoQueue = jobInfoQueue;
            _spoolerProvider = spoolerProvider;
            _jobInfoManager = jobInfoManager;
        }

        public string NewJobInfoFile { get; internal set; }

        protected override string ComposePipeMessage()
        {
            EnsureJobFileIsInSpoolPath();

            return "NewJob|" + NewJobInfoFile;
        }

        protected override bool StartApplication()
        {
            if (string.IsNullOrEmpty(NewJobInfoFile) || !File.Exists(NewJobInfoFile))
            {
                _logger.Error("No file in InfoDataFile argument or file does not exist.");
                return false;
            }

            EnsureJobFileIsInSpoolPath();

            _logger.Debug("Adding new job");
            var jobInfo = _jobInfoManager.ReadFromInfFile(NewJobInfoFile);
            _jobInfoQueue.Add(jobInfo);

            return true;
        }

        private void EnsureJobFileIsInSpoolPath()
        {
            // Move to spool folder, if the correct spool folder could not be determined for some reason
            if (
                !Path.GetFullPath(NewJobInfoFile)
                    .StartsWith(_spoolerProvider.SpoolFolder, StringComparison.OrdinalIgnoreCase))
            {
                _logger.Debug(
                    "JobInfo file from printer is not in our spool folder ({0}) - we'll move it there",
                    _spoolerProvider.SpoolFolder);
                NewJobInfoFile = MoveSpoolFile(NewJobInfoFile, _spoolerProvider.SpoolFolder,
                    _settingsProvider.Settings.ApplicationSettings);
            }
        }

        private string MoveSpoolFile(string infFile, string spoolFolder, ApplicationSettings applicationSettings)
        {
            var ji = _jobInfoManager.ReadFromInfFile(infFile);

            var jobName = Path.GetFileNameWithoutExtension(infFile);
            var jobFolder = Path.Combine(spoolFolder, jobName);

            jobFolder = new UniqueDirectory(jobFolder).MakeUniqueDirectory();
            Directory.CreateDirectory(jobFolder);

            foreach (var sourceFile in ji.SourceFiles)
            {
                var targetFile = Path.Combine(jobFolder, Path.GetFileName(sourceFile.Filename));
                File.Move(sourceFile.Filename, targetFile);
                sourceFile.Filename = Path.GetFileName(sourceFile.Filename);
            }

            var newInfFile = Path.Combine(jobFolder, jobName + ".inf");

            _jobInfoManager.SaveToInfFile(ji, newInfFile);

            File.Delete(infFile);

            return newInfFile;
        }
    }
}
