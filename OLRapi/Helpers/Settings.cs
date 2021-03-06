﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace OLRapi.Helpers
{
    public static class Settings
    {
        public static string GraphApiKey  => ConfigurationManager.AppSettings["SendGridApi"];

        public static string RegistrationUrl => ConfigurationManager.AppSettings["RegistrationUrl"];
        public static string RegistrationUrlApi => ConfigurationManager.AppSettings["RegistrationUrlApi"];

        public static string EventUID => ConfigurationManager.AppSettings["EventUID"];

        public static bool AllowDuplicateeMail = ConfigurationManager.AppSettings["AllowDuplicateeMail"] == "true" ? true : false;
        public static bool UseApi = ConfigurationManager.AppSettings["UseApi"] == "true" ? true : false;

        public static string LinkValidDays => ConfigurationManager.AppSettings["LinkValidDays"];
        public static int RegistrationCodeLength => Convert.ToInt32(ConfigurationManager.AppSettings["RegistrationCodeLength"]);

        public static bool TestMode => Convert.ToBoolean(ConfigurationManager.AppSettings["TestMode"]);

        public static int PayByDays => Convert.ToInt32(ConfigurationManager.AppSettings["PayByDays"]);

        public static string ReCaptchaPublicKey => ConfigurationManager.AppSettings["ReCaptcha.PublicKey"];
        public static string ReCaptchaPrivateKey => ConfigurationManager.AppSettings["ReCaptcha.PrivateKey"];
    }
}