﻿using NLog;
using pdfforge.PDFCreator.Conversion.ActionsInterface;
using pdfforge.PDFCreator.Conversion.Jobs;
using pdfforge.PDFCreator.Conversion.Jobs.Jobs;
using pdfforge.PDFCreator.Conversion.Settings;
using pdfforge.PDFCreator.Utilities;
using pdfforge.PDFCreator.Utilities.Ftp;
using pdfforge.PDFCreator.Utilities.IO;
using System;
using System.ComponentModel;
using System.IO;

namespace pdfforge.PDFCreator.Conversion.Actions.Actions
{
    public class FtpAction : RetypePasswordActionBase, ICheckable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IFtpConnectionFactory _ftpConnectionFactory;
        private readonly IPathUtil _pathUtil;

        protected override string PasswordText => "FTP";

        public FtpAction(IFtpConnectionFactory ftpConnectionFactory, IPathUtil pathUtil)
        {
            _ftpConnectionFactory = ftpConnectionFactory;
            _pathUtil = pathUtil;
        }

        public override ActionResult Check(ConversionProfile profile, Accounts accounts)
        {
            var actionResult = new ActionResult();
            if (!IsEnabled(profile))
                return actionResult;

            var ftpAccount = accounts.GetFtpAccount(profile);
            if (ftpAccount == null)
            {
                Logger.Error($"The specified FTP account with ID '{profile.Ftp.AccountId}' is not configured.");
                actionResult.Add(ErrorCode.Ftp_NoAccount);

                return actionResult;
            }

            if (string.IsNullOrEmpty(ftpAccount.Server))
            {
                Logger.Error("No FTP server specified.");
                actionResult.Add(ErrorCode.Ftp_NoServer);
            }

            if (string.IsNullOrEmpty(ftpAccount.UserName))
            {
                Logger.Error("No FTP username specified.");
                actionResult.Add(ErrorCode.Ftp_NoUser);
            }

            if (profile.AutoSave.Enabled && string.IsNullOrEmpty(ftpAccount.Password))
            {
                Logger.Error("Automatic saving without ftp password.");
                actionResult.Add(ErrorCode.Ftp_AutoSaveWithoutPassword);
            }

            return actionResult;
        }

        protected override ActionResult DoActionProcessing(Job job)
        {
            var ftpAccount = job.Accounts.GetFtpAccount(job.Profile);

            Logger.Debug("Creating ftp connection.\r\nServer: " + ftpAccount.Server + "\r\nUsername: " + ftpAccount.UserName);

            var ftpConnection = _ftpConnectionFactory.BuildConnection(ftpAccount.Server, ftpAccount.UserName, job.Passwords.FtpPassword);

            try
            {
                ftpConnection.Open();
                ftpConnection.Login();
            }
            catch (Win32Exception ex)
            {
                if (ex.NativeErrorCode.Equals(12007))
                {
                    Logger.Error("Can not connect to the internet for login to ftp. Win32Exception Message:\r\n" + ex.Message);
                    ftpConnection.Close();
                    return new ActionResult(ErrorCode.Ftp_ConnectionError);
                }
                if (ex.NativeErrorCode.Equals(12014))
                {
                    Logger.Error("Can not login to ftp because the password is incorrect. Win32Exception Message:\r\n" + ex.Message);
                    ftpConnection.Close();
                    return new ActionResult(ErrorCode.PasswordAction_Login_Error);
                }

                Logger.Error("Win32Exception while login to ftp server:\r\n" + ex.Message);
                ftpConnection.Close();
                return new ActionResult(ErrorCode.Ftp_LoginError);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception while login to ftp server:\r\n" + ex.Message);
                ftpConnection.Close();
                return new ActionResult(ErrorCode.Ftp_LoginError);
            }

            var fullDirectory = job.TokenReplacer.ReplaceTokens(job.Profile.Ftp.Directory).Trim();
            if (!IsValidPath(fullDirectory))
            {
                Logger.Warn("Directory contains invalid characters \"" + fullDirectory + "\"");
                fullDirectory = MakeValidPath(fullDirectory);
            }

            Logger.Debug("Directory on ftp server: " + fullDirectory);

            var directories = fullDirectory.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                foreach (var directory in directories)
                {
                    if (!ftpConnection.DirectoryExists(directory))
                    {
                        Logger.Debug("Create folder: " + directory);
                        ftpConnection.CreateDirectory(directory);
                    }
                    Logger.Debug("Move to: " + directory);
                    ftpConnection.SetCurrentDirectory(directory);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception while setting directory on ftp server\r\n:" + ex.Message);
                ftpConnection.Close();
                return new ActionResult(ErrorCode.Ftp_DirectoryError);
            }

            foreach (var file in job.OutputFiles)
            {
                var targetFile = Path.GetFileName(file);
                targetFile = MakeValidPath(targetFile);
                if (job.Profile.Ftp.EnsureUniqueFilenames)
                {
                    Logger.Debug("Make unique filename for " + targetFile);
                    try
                    {
                        var uf = new UniqueFilenameForFtp(targetFile, ftpConnection, _pathUtil);
                        targetFile = uf.CreateUniqueFileName();
                        Logger.Debug("-> The unique filename is \"" + targetFile + "\"");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Exception while generating unique filename\r\n:" + ex.Message);
                        ftpConnection.Close();
                        return new ActionResult(ErrorCode.Ftp_DirectoryReadError);
                    }
                }

                try
                {
                    ftpConnection.PutFile(file, targetFile);
                }
                catch (Exception ex)
                {
                    Logger.Error("Exception while uploading the file \"" + file + "\": \r\n" + ex.Message);
                    ftpConnection.Close();
                    return new ActionResult(ErrorCode.Ftp_UploadError);
                }
            }

            ftpConnection.Close();
            return new ActionResult();
        }

        private bool IsValidPath(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            return ValidName.IsValidFtpPath(path);
        }

        private string MakeValidPath(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            return ValidName.MakeValidFtpPath(path);
        }

        protected override void SetPassword(Job job, string password)
        {
            job.Passwords.FtpPassword = password;
        }

        public override bool IsEnabled(ConversionProfile profile)
        {
            return profile.Ftp.Enabled;
        }
    }
}
