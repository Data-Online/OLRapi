using System;
using System.Web.Http;
//using System.Web.Mvc;

namespace OLRapi.Controllers
{
    public partial class RegistrationController : ApiController
    {
        public class BaseRegistration
        {
            public string id { get; set; }
            public DateTime Date { get; set; }
            public string Email { get; set; }
        }

    }
}