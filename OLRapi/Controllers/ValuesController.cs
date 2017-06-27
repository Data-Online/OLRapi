using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace OLRapi.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        public class Registration
        {
            public string id { get; set; }
            public DateTime Date { get; set; }
            public string Email { get; set; }
        }

        // POST api/values
        public HttpResponseMessage Post([FromBody]string value)
        {
            if (value != null)
            {
                Registration registration = new Registration()
                {
                    Email = HttpUtility.HtmlEncode(value),
                    Date = DateTime.UtcNow
                };

                var id = Guid.NewGuid();
                //updates[id] = registration;

                var response = new HttpResponseMessage(HttpStatusCode.Created)
                {
                    Content = new StringContent(registration.Email)
                };
                response.Headers.Location =
                    new Uri(Url.Link("DefaultApi", new { action = "status", id = id }));
                return response;
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
