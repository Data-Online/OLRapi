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
using OLRapi.Models;
//using System.Web.Mvc;

namespace OLRapi.Controllers
{
    public class RegistrationController : ApiController
    {
        private OLR_dbEntities db = new OLR_dbEntities();
        

        [HttpGet]
        [Route("api/registration/{userGuid}")]
        public async Task<HttpResponseMessage> Registration(Guid userGuid, HttpRequestMessage request)
        {
            //Guid userGuid = new Guid("96BE1CE0-27F4-4A46-BDCD-EBAB16A1EA27zz");

            var result = await GetRegistrationData(userGuid);
            if (result == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            else
            {
                return request.CreateResponse<RegistrationViewModel>(HttpStatusCode.OK,result);
            }
        }

        public async Task<RegistrationViewModel> GetRegistrationData(Guid userGuid)
        {
            RegistrationViewModel result;

            var currentUser = await db.Registrations.Where(s => s.ValidationUid == userGuid).Select(s => s.Contact).FirstOrDefaultAsync();
            if (currentUser == null)
            {
                // No valid registration on file
                return null;
            }

            IEnumerable<FieldTripChoice> fieldTripChoices = await db.FieldTripChoices.Where(s => s.Registration.ValidationUid == userGuid).ToArrayAsync();

            List<FieldTripOptions> FieldTrips = new List<FieldTripOptions>();

            foreach (var itemId in db.FieldTripOptions.Select(s => s.FieldTripId).Distinct())
            {
                FieldTripOptions fieldTripOptions = new FieldTripOptions();
                List<string> currentFieldTripDetails = await db.FieldTripOptions.Where(s => s.FieldTripId == itemId).Select(o => o.Description).ToListAsync();
                
                List<string> _choices = new List<string>();
                try
                {
                    var zz = fieldTripChoices.Where(s => s.FieldTripId == itemId)
                        .Select(x =>
                    //new List<string> {  (x.FieldTripOption.Description == null ? "" : x.FieldTripOption.Description),
                    //                (x.FieldTripOption2.Description == null ? "" : x.FieldTripOption2.Description) }).ToList();
                    new List<string> { (x.FieldTripOption == null ? null : x.FieldTripOption.Description),
                                        (x.FieldTripOption1 == null ? null : x.FieldTripOption1.Description),
                                        (x.FieldTripOption3 == null ? null : x.FieldTripOption3.Description) })
                                        .ToList();
                    //foreach (var item in zz)
                    //{
                    //    string zz = db.
                    //    _options.Add(item.FieldTripOption ?? "").ToString());
                    //}
                    _choices = zz[0];
                }
                catch (Exception ex) { }


                // Assign to string array
                fieldTripOptions.options = currentFieldTripDetails;
                fieldTripOptions.choices = _choices;//(List<string>)fieldTripChoices.Where(s => s.FieldTripId == itemId).Select(x => x.FieldTripOption);
                                //new List<string>() { "Monarch", "Street Art", "Tunnel Beach" };
                fieldTripOptions.fieldTripDescription = "Saturday";

                FieldTrips.Add(fieldTripOptions);
            }

            result = new RegistrationViewModel()
            {
                userDetails = new UserDetails() { email = "test@test.com", firstName = "", lastName = "" },
                fieldTrips = FieldTrips
            };

            return result;
        }

        //[System.Web.Mvc.ValidateAntiForgeryToken]
        [HttpPost]
        [Route("api/saveRegistrationDetails")]
        public HttpResponseMessage SaveUserSettings(HttpRequestMessage request, [FromBody] RegistrationViewModel registrationDetails)
        {
            //UserSettingsViewModel _data = _portalService.SaveUserData(userSettings, User.Identity.Name);
            //if (_portalService.SaveUserSettings(userSettings, User.Identity.Name))
            return request.CreateResponse(HttpStatusCode.OK);
            //else
            //    return request.CreateResponse(HttpStatusCode.BadRequest);
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

            try
            {
                if (value != null)
                {
                    Event @event = new Event();

                    @event = await db.Events.Where(s => s.GID == eventid && s.Active == true).FirstOrDefaultAsync();


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
                        fieldTripChoices.Add(new FieldTripChoice { FieldTripId = entry.FieldTripId, RecordDeleted = false });
                    }


                    IList<AvailableWorkshop> availableWorkshops = await db.AvailableWorkshops.Where(s => s.EventId == @event.EventId).ToListAsync();
                    IList<Workshop> workshops = new List<Workshop>();

                    foreach (var entry in availableWorkshops)
                    {
                        workshops.Add(new Workshop { AvailableWorkshopId = entry.AvailableWorkshopId, RecordDeleted = false });
                    }

                    Registration registration = new Registration()
                    {
                        ValidationUid = registrationUid,
                        Contact = new Contact() { Email = baseRegistration.Email, RecordDeleted = false },
                        // Field trip options need to be added here
                        EventId = @event.EventId,
                        FieldTripChoices = fieldTripChoices,
                        Workshops = workshops,
                        InitialCreationDate = DateTime.Now,
                        RecordDeleted = false
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
            catch (Exception ex)
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
            var htmlContent = "<strong>Thank you for your interest in this event</strong><br />" +
                                String.Format("Please click here : <a id='register' href={0}>{1}</a>", registrationUri, "register!");
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
            return response.StatusCode;
        }

    }
}