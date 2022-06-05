using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.Net.Mail;

namespace DBRegressionLibrary.Common
{

    /// <summary>
    /// Common utility functions
    /// </summary>
    public class Common
    {
       
        /// <summary>
        /// Checks and Deletes a given file
        /// </summary>
        /// <param name="FilePath"></param>
        public static void CheckAndDeleteFile(string FilePath)
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }


        /// <summary>
        /// email alerting
        /// </summary>
        /// <param name="Subject"></param>
        /// <param name="Body"></param>
        /// <param name="IsBodyHtml"></param>
        /// <param name="Recipients"></param>
        /// <param name="FileAttachmentPaths"></param>
        public static void SendEmail(string Subject, string Body,  bool IsBodyHtml,  string Recipients, List<string> FileAttachmentPaths)
        {

            MailMessage mailMessage = new MailMessage(AppConfiguration.FromEmailAddress, Recipients, Subject, Body);

            mailMessage.IsBodyHtml = true;

            if (FileAttachmentPaths != null)
            {
                foreach (var filePath in FileAttachmentPaths)
                {
                    if (File.Exists(filePath))
                        mailMessage.Attachments.Add(new Attachment(filePath));
                }
            }

            SmtpClient emailClient = new SmtpClient(AppConfiguration.SMTPHostName, AppConfiguration.SMTPPortNumber);
            emailClient.Send(mailMessage);

            //Dispose the mailmessage object otherwise when invoking this method through asp.net website from IIS will lock the file
            mailMessage.Dispose();

        }
    }
}
