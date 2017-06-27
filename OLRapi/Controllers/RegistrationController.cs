using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using OLRapi;
using System.Web;
using SendGrid;
using SendGrid.Helpers.Mail;
using OLRapi.Helpers;

namespace OLRapi.Controllers
{
    public class RegistrationController : ApiController
    {
        private OLR_dbEntities db = new OLR_dbEntities();

        public class BaseRegistration
        {
            public string id { get; set; }
            public DateTime Date { get; set; }
            public string Email { get; set; }
        }

        [HttpPost]
        [Route("RegisterEmail/{eventid}")]
        public async Task<HttpResponseMessage> RegisterEmail(Guid eventid, [FromBody] string value)
        {
            if (Request.Properties.ContainsKey("MS_HttpContext"))
            {
                var ctx = Request.Properties["MS_HttpContext"] as HttpContextBase;
                if (ctx != null)
                {
                    // var zz = ctx.Request.UserHostAddress;
                    var sourceUrl = ctx.Request.UrlReferrer;
                }
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            if (value != null)
            {
                Event @event = await db.Events.Where(s => s.GID == eventid && s.Active == true).FirstOrDefaultAsync();

                if (@event == null)
                {
                    // Event not found or not active
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }

                // Okay the event is valid and active

                BaseRegistration baseRegistration = new BaseRegistration()
                {
                    Email = HttpUtility.HtmlEncode(value),
                    Date = DateTime.UtcNow
                };

                Contact @contact = await db.Contacts.Where(s => s.Email == baseRegistration.Email).FirstOrDefaultAsync();
                if (@contact != null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
                var registrationUid = Guid.NewGuid();

                IList<FieldTrip> fieldTrips = await db.FieldTrips.Where(s => s.EventId == @event.EventId).ToListAsync();
                IList<FieldTripChoice> fieldTripChoices = new List<FieldTripChoice>();
                foreach (var entry in fieldTrips)
                {
                    fieldTripChoices.Add(new FieldTripChoice { FieldTripId = entry.FieldTripId });
                }

                Registration registration = new Registration()
                {
                    ValidationUid = registrationUid,
                    Contact = new Contact() { Email = baseRegistration.Email },
                    // Field trip options need to be added here
                    EventId = @event.EventId,
                    FieldTripChoices = fieldTripChoices
                };

                db.Registrations.Add(registration);


                
                await db.SaveChangesAsync();

                var verify = db.Events.Select(s => s.Registrations.Where(w => w.EventId == @event.EventId && w.Contact.Email == baseRegistration.Email)).FirstOrDefaultAsync();
                
                if (verify != null)
                {
                    // var id = Guid.NewGuid();
                    // Send email
                    HttpStatusCode emailStatus = await SendEmail(baseRegistration.Email, @event.ContactEmail, registrationUid.ToString());

                    var response = new HttpResponseMessage(HttpStatusCode.Created)
                    {
                        Content = new StringContent(baseRegistration.Email)
                    };

                    
                    //response.Headers.Location =
                    //    new Uri(Url.Link("DefaultApi", new { action = "status", id = id }));
                    return response;
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
                //return CreatedAtRoute("DefaultApi", new { action = "status", id = id }, value);

                
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool EventExists(int id)
        {
            return db.Events.Count(e => e.EventId == id) > 0;
        }
        static async Task<HttpStatusCode> SendEmail(string eMail, string registrationEmail, string registrationUid)
        {
            // Base class from ISD core application

            var apiKey = Settings.GraphApiKey;
            var registrationUri = String.Format(Settings.RegistrationUrl, registrationUid);
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(registrationEmail, "Registrations");
            var subject = "Registration eMail";
            var to = new EmailAddress(eMail);
           

            var plainTextContent = "Thank you for your interest in this event";
            var htmlContent = "<strong>Thank you for your interest in this event</strong><br />"+
                                String.Format("Please click here : <a id='register' href={0}>{1}</a>",registrationUri, "register!");
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
            return response.StatusCode;
        }

    }
}