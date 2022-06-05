using System;
using System.Collections.Generic;
using System.Text;

using System.Configuration;

namespace DBRegressionLibrary.Common
{

    /// <summary>
    /// Configurations for App.Config
    /// </summary>
    public class AppConfiguration
    {

        public static string SourceDBConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["SourceDBConnectionString"].ConnectionString;
            }
        }

        public static string TargetDBConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["TargetDBConnectionString"].ConnectionString;
            }
        }

        public static string TestConfigConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["TestConfigConnectionString"].ConnectionString;
            }
        }

        public static string RecErrorsCSVFilePath
        {
            get
            {
                return ConfigurationManager.AppSettings["RecErrorsCSVFilePath"];
            }

        }

        public static string FromEmailAddress
        {
            get
            {
                return ConfigurationManager.AppSettings["FromEmailAddress"];
            }

        }

        public static string ToEmailAddressForSupport
        {
            get
            {
                return ConfigurationManager.AppSettings["ToEmailAddressForSupport"];
            }

        }

        public static string SMTPHostName
        {
            get
            {
                return ConfigurationManager.AppSettings["SMTPHostName"];
            }

        }

        public static int SMTPPortNumber
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings["SMTPPortNumber"]);
            }

        }


    }
}
