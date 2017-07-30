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
                return request.CreateResponse<RegistrationViewModel>(HttpStatusCode.OK, result);
            }
        }

        [HttpGet]
        [Route("api/getForeignKeyData")]
        public async Task<HttpResponseMessage> GetForeignKeyData(HttpRequestMessage request)
        {
            ForeignKeyViewModel result = new ForeignKeyViewModel();
            result.towns = await db.HomeTowns.Select(o => o.TownName).ToListAsync();
            result.photoClubs = await db.PhotoClubs.Select(o => o.Description).ToListAsync();
            result.photoHonours = await db.Honours.Select(o => o.Description).ToListAsync();
            result.registrationTypes = await db.RegistrationTypes.Select(o => o.RegistrationType1).ToListAsync();

            return request.CreateResponse<ForeignKeyViewModel>(HttpStatusCode.OK, result);
        }

        public async Task<RegistrationViewModel> GetRegistrationData(Guid userGuid)
        {
            RegistrationViewModel result = new RegistrationViewModel();

            IQueryable<Registration> query = db.Registrations.Where(s => s.ValidationUid == userGuid);

            Contact currentUser = await query.Select(s => s.Contact).FirstOrDefaultAsync();
            if (currentUser == null)
            {
                // No valid registration on file
                return null;
            }

            IEnumerable<FieldTripChoice> fieldTripChoicesForContact = await db.FieldTripChoices.Where(s => s.Registration.ValidationUid == userGuid).ToArrayAsync();

            List<FieldTripOptionsAndChoices> FieldTrips = new List<FieldTripOptionsAndChoices>();  // options and choices with description

            var fieldTripQuery = db.FieldTrips.Select(s => s.FieldTripId);

            foreach (var availableFieldTripId in fieldTripQuery)
            {
                FieldTripOptionsAndChoices fieldTripOptionsAndChoices = new FieldTripOptionsAndChoices();
                List<string> currentFieldOptions = await db.FieldTripOptions.Where(s => s.FieldTripId == availableFieldTripId).Select(o => o.Description).ToListAsync();

                fieldTripOptionsAndChoices.options = currentFieldOptions;
                fieldTripOptionsAndChoices.choices = GetCurrentFieldtripChoices(fieldTripChoicesForContact, availableFieldTripId);
                fieldTripOptionsAndChoices.fieldTripDescription = db.FieldTrips.Where(s => s.FieldTripId == availableFieldTripId).Select(o => o.Description).FirstOrDefault();
                FieldTrips.Add(fieldTripOptionsAndChoices);
            }

            //foreach (var availableFieldTripId in db.FieldTripOptions.Select(s => s.FieldTripId).Distinct())
            //{
            //    FieldTripOptionsAndChoices fieldTripOptions = new FieldTripOptionsAndChoices();
            //    List<string> currentFieldTripDetails = await db.FieldTripOptions.Where(s => s.FieldTripId == availableFieldTripId).Select(o => o.Description).ToListAsync();

            //    // Assign to string array
            //    fieldTripOptions.options = currentFieldTripDetails;
            //    fieldTripOptions.choices = GetCurrentFieldtripChoices(fieldTripChoicesForContact, availableFieldTripId ?? 0);
            //    //new List<string>() { "Monarch", "Street Art", "Tunnel Beach" };
            //    fieldTripOptions.fieldTripDescription = db.FieldTrips.Where(s => s.FieldTripId == availableFieldTripId).Select(o => o.Description).FirstOrDefault().ToString();

            //    FieldTrips.Add(fieldTripOptions);
            //}

            //  var userDetailsOnFile = db.Registrations.Where()
            try
            {
                result = new RegistrationViewModel()
                {
                    // userDetails = new UserDetails() { email = "test@test.com", firstName = "", lastName = "" },
                    fieldTrips = FieldTrips,
                    userDetails = new UserDetails()
                    {
                        firstName = currentUser.FirstName,
                        lastName = currentUser.LastName,
                        homeTown = (currentUser.HomeTown == null ? null : currentUser.HomeTown.TownName),
                        email = currentUser.Email,
                        NZIPPMember = currentUser.NZIPPMember ?? false,
                        PSNZMember = currentUser.PSNZMember ?? false,
                        PSNZMemberAppliedFor = currentUser.PSNZAppliedFor ?? false,
                        photoHonours = currentUser.HonourContactLinks.Select(s => s.Honour.Description).ToList(),
                        photoClubs = currentUser.PhotoClubContactLinks.Select(s => s.PhotoClub.Description).ToList()
                    },
                    //userDetails = new UserDetails() { firstName = "Graeme", lastName = "Atkinson", homeTown = "Dunedin", email = "atkinsongraeme@hotmail.com" },
                    registrationDetails = new RegistrationDetails()
                    {
                        registrationType = query.Select(o => o.RegistrationType.RegistrationType1).FirstOrDefault(),
                        canonWorkshop = query.Select(s => s.Workshops).FirstOrDefault().Select(o => o.Attending ?? false).FirstOrDefault(),
                        additionalDinnerTicket = query.Select(s => s.AdditionalDinnerTicket ?? false).FirstOrDefault(),
                        additionalDinnerName = query.Select(s => s.AdditionalDinnerName ?? "").FirstOrDefault(),
                        specialRequirements = query.Select(o => o.SpecialRequirements).FirstOrDefault()
                    }
                    // "Full convention including awards dinner" }
                };
            }
            catch (Exception ex)
            { return null; }

            return result;
        }


        private List<string> GetCurrentFieldtripChoices(IEnumerable<FieldTripChoice> fieldTripChoices, int fieldTripId)
        {
            List<string> _choices = new List<string>() { null };
            try
            {
                var currentChoiceList = fieldTripChoices.Where(s => s.FieldTripId == fieldTripId)
                    .Select(x =>
                new List<string> { (x.FieldTripOption == null ? null : x.FieldTripOption.Description),
                                        (x.FieldTripOption1 == null ? null : x.FieldTripOption1.Description),
                                        (x.FieldTripOption2 == null ? null : x.FieldTripOption2.Description) })
                                    .ToList();
                _choices = currentChoiceList[0];
            }
            catch (Exception ex) { }

            return _choices;
        }

        //[System.Web.Mvc.ValidateAntiForgeryToken]
        [HttpPost]
        [Route("api/saveRegistrationDetails/{userGuid}")]
        public async Task<HttpResponseMessage> SaveUserSettings(Guid userGuid, HttpRequestMessage request, [FromBody] RegistrationViewModel registrationDetails)
        {
            // Read existing record from database
            // Map view model to database
            // Save updates

            Registration registration = await db.Registrations.Where(s => s.ValidationUid == userGuid).FirstOrDefaultAsync();

            registration.Contact.FirstName = registrationDetails.userDetails.firstName;
            registration.Contact.LastName = registrationDetails.userDetails.lastName;
            registration.Contact.HomeTown = (db.HomeTowns.Where(s => s.TownName == registrationDetails.userDetails.homeTown).FirstOrDefault());

            registration.Contact.NZIPPMember = registrationDetails.userDetails.NZIPPMember;
            registration.Contact.PSNZMember = registrationDetails.userDetails.PSNZMember;
            registration.Contact.PSNZAppliedFor = registrationDetails.userDetails.PSNZMemberAppliedFor;

            // Honors

            registration.RegistrationType = db.RegistrationTypes.Where(s => s.RegistrationType1 == registrationDetails.registrationDetails.registrationType).FirstOrDefault();

            registration.AdditionalDinnerTicket = registrationDetails.registrationDetails.additionalDinnerTicket;
            registration.AdditionalDinnerName = registrationDetails.registrationDetails.additionalDinnerName;
            registration.SpecialRequirements = registrationDetails.registrationDetails.specialRequirements;


            var _fieldTripsOnFile = registration.FieldTripChoices.ToArray();

            foreach (var _currentFieldTrips in _fieldTripsOnFile)
            {
                var _description = _currentFieldTrips.FieldTrip.Description;

                var _fieldTripsSelected = registrationDetails.fieldTrips.Where(s => s.fieldTripDescription == _description).Select(o => o.choices).ToList();

                int _count = 0;
                foreach (var _choice in _fieldTripsSelected[0])
                {
                    var _id = db.FieldTripOptions.Where(s => s.Description == _choice).Select(o => o.FieldTripOptionId).FirstOrDefault();
                    if (_id > 0)
                    {
                        switch (_count)
                        {
                            case 0:
                                _currentFieldTrips.FieldTripOptionId = _id;
                                break;
                            case 1:
                                _currentFieldTrips.FieldTripOptionId2 = _id;
                                break;
                            case 2:
                                _currentFieldTrips.FieldTripOptionId3 = _id;
                                break;
                        }
                    }
                    _count++;
                }
            }

            try
            {
                string SQL = string.Format("delete from HonourContactLinks where ContactId = {0}", registration.Contact.ContactId.ToString());
                db.Database.ExecuteSqlCommand(SQL);

                foreach (var description in registrationDetails.userDetails.photoHonours)
                {
                    var _id = db.Honours.Where(s => s.Description == description).Select(o => o.HonourId).FirstOrDefault();
                    var _honour = new Honour() { HonourId = _id };
                    db.Honours.Attach(_honour);
                    HonourContactLink link = new HonourContactLink() { Honour = _honour, RecordDeleted = false };
                    registration.Contact.HonourContactLinks.Add(link);
                }
            }
            catch (Exception ex) { }

            try
            {
                string SQL = string.Format("delete from PhotoClubContactLinks where ContactId = {0}", registration.Contact.ContactId.ToString());
                db.Database.ExecuteSqlCommand(SQL);

                foreach (var description in registrationDetails.userDetails.photoClubs)
                {
                    var _id = db.PhotoClubs.Where(s => s.Description == description).Select(o => o.PhotoClubId).FirstOrDefault();
                    var _photoClub = new PhotoClub() { PhotoClubId = _id };
                    db.PhotoClubs.Attach(_photoClub);
                    PhotoClubContactLink link = new PhotoClubContactLink() { PhotoClub = _photoClub, RecordDeleted = false };
                    registration.Contact.PhotoClubContactLinks.Add(link);
                }
            }
            catch (Exception ex) { }


            //            try
            //            {
            //                using (var context = new OLR_dbEntities())
            //                {
            //                    var _contact = new Contact() { ContactId = 74 };
            //                    context.Contacts.Attach(_contact);

            ////                    var _honourList = context.HonourContactLinks.Where(o => o.Contact == _contact);
            //                    foreach (var item in _contact.HonourContactLinks)//.Where(at => at.ContactId == _contact.ContactId))
            //                    {
            //                        _contact.HonourContactLinks.Remove(item);
            //                    }

            //                    var _honour = new Honour() { HonourId = 86 };
            //                    context.Honours.Attach(_honour);

            //                    HonourContactLink link = new HonourContactLink() { Honour = _honour };
            //                    //context.HonourContactLinks.Attach(link);

            //                    _contact.HonourContactLinks.Add(link);

            //                    context.SaveChanges();

            //                }
            //            }
            //            catch (Exception ex) { }



            //foreach (var _honour in _honourSelected)
            //{
            //    //_honoursOnFile.Add(db.Honours.Where(o => o.Description == _honour).Select(s => s.HonourId));
            //    var _description = _honour;
            //    //var _honourSelected = registrationDetails.userDetails.photoHonours.ToList();
            //}
            //    //registrationDetails.userDetails.photoHonours.ToList();

            // Special case, we are only supporting a single "Canon Workshop" 
            // *****
            var zz = registration.Workshops.FirstOrDefault();
            zz.Attending = registrationDetails.registrationDetails.canonWorkshop;

            //var _workshop = registration.Workshops.Where(s => s.AvailableWorkshop.WorkshopDescription == )
            //registration.Workshops = db.Workshops.Where(s => s.AvailableWorkshop.WorkshopDescription == registrationDetails.registrationDetails.)
            //foreach (var _fieldTrip in registrationDetails.fie)
            //{
            //    //  _fieldTripsOnFile[0].FieldTripOptionId = db.FieldTripOptions.Where(s => s.Description == _fieldTrip.choices.).Select(o => o.FieldTripId).FirstOrDefault();
            //}

            //UserSettingsViewModel _data = _portalService.SaveUserData(userSettings, User.Identity.Name);
            //if (_portalService.SaveUserSettings(userSettings, User.Identity.Name))

            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception ex) { }

            return request.CreateResponse(HttpStatusCode.OK);
            //else
            //    return request.CreateResponse(HttpStatusCode.BadRequest);
        }

        [HttpPost]
        [Route("RegisterEmail/{eventid}")]
        public async Task<HttpResponseMessage> RegisterEmail(Guid eventid, [FromBody] string value)
        {
            string sourceUriTxt = "";

            if (Request.Properties.ContainsKey("MS_HttpContext"))
            {
                var ctx = Request.Properties["MS_HttpContext"] as HttpContextBase;
                if (ctx != null)
                {
                    // var zz = ctx.Request.UserHostAddress;
                    var sourceUrl = ctx.Request.UrlReferrer;
                    sourceUriTxt = sourceUrl.AbsoluteUri.ToString();
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
                    if (@contact != null && !Settings.AllowDuplicateeMail)
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

                    if (registration.RegistrationId > 0)
                    {
                        // PK value, so record has been saved okay
                        // var id = Guid.NewGuid();
                        // Send email
                        HttpStatusCode emailStatus = await SendEmail(baseRegistration.Email, @event.ContactEmail, registrationUid.ToString(), sourceUriTxt);

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

                    //var verify = db.Events.Select(s => s.Registrations.Where(w => w.EventId == @event.EventId && w.Contact.Email == baseRegistration.Email)).FirstOrDefaultAsync();

                    //if (verify != null)
                    //{
                    //    // var id = Guid.NewGuid();
                    //    // Send email
                    //    HttpStatusCode emailStatus = await SendEmail(baseRegistration.Email, @event.ContactEmail, registrationUid.ToString());

                    //    var response = new HttpResponseMessage(HttpStatusCode.Created)
                    //    {
                    //        Content = new StringContent(baseRegistration.Email)
                    //    };


                    //    //response.Headers.Location =
                    //    //    new Uri(Url.Link("DefaultApi", new { action = "status", id = id }));
                    //    return response;
                    //}
                    //else
                    //{
                    //    return Request.CreateResponse(HttpStatusCode.BadRequest);
                    //}
                    ////return CreatedAtRoute("DefaultApi", new { action = "status", id = id }, value);


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
        static async Task<HttpStatusCode> SendEmail(string eMail, string registrationEmail, string registrationUid, string sourceUrlTxt)
        {
            // Base class from ISD core application

            var apiKey = Settings.GraphApiKey;
            string registrationUri = "";
            if (Settings.UseApi)
            {
                registrationUri = String.Format(Settings.RegistrationUrlApi, sourceUrlTxt, registrationUid);
            }
            else
            {
                registrationUri = String.Format(Settings.RegistrationUrl, registrationUid);
            }

            //    <add key = "RegistrationUrlApi" value="{0}/Home/RegisterMe?Registration={1}" />
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