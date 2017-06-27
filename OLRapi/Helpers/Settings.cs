using System;
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
    }
}