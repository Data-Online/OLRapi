﻿using System;
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
using Humanizer;
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
        [Route("api/getCurrentCost/{userGuid}")]
        public HttpResponseMessage CurrentCosts(Guid userGuid, HttpRequestMessage request)
        {
            // Requires re-coding, results from SP now a table of items
            var result = db.sp_rpt_CalculateCosts2(userGuid, false).ToArray();
            return request.CreateResponse(HttpStatusCode.OK, result); //, result);
        }

        [HttpGet]
        [Route("api/sendRegistration/{userGuid}")]
        public async Task<HttpResponseMessage> SendRegistration(Guid userGuid, HttpRequestMessage request)
        {
            // Requires re-coding, results from SP now a table of items
            if (await SendRegistrationDetails(userGuid))
                return request.CreateResponse(HttpStatusCode.OK); //, result);
            else
                return request.CreateResponse(HttpStatusCode.BadRequest); //, result);

        }

        private async Task<bool> SendRegistrationDetails(Guid userGuid)
        {
            RegistrationViewModel result = await GetRegistrationData(userGuid);
            if (result != null)
            {
                string copyToEmail = "";
                Guid eventID = new Guid(Settings.EventUID);
                Event eventDetails = db.Events.Where(w => w.GID == eventID).FirstOrDefault();
                DateTime expiryDate = new DateTime();
                // Error checking?
                var _confirmation = db.Configurations.Where(s => s.EventId == eventDetails.EventId).FirstOrDefault();
                if (_confirmation.SendEmailCopies ?? false)
                {
                    copyToEmail = _confirmation.EmailCopiesAddress;
                }
                var zz = await SendConfirmationEmail(result, eventDetails, expiryDate, copyToEmail);
                return true;
            }
            else
            {
                return false;
            }
        }

        [HttpGet]
        [Route("api/getForeignKeyData/{adminAccess}")]
        public async Task<HttpResponseMessage> GetForeignKeyData(HttpRequestMessage request, bool adminAccess)
        {
            ForeignKeyViewModel result = new ForeignKeyViewModel();
            result.towns = await db.HomeTowns.Where(s => s.TownName != "NOT LISTED").OrderBy(e => e.TownName).Select(o => o.TownName).ToListAsync();
            result.towns.Insert(0, "NOT LISTED");
            result.photoClubs = await db.PhotoClubs.Select(o => o.Description).ToListAsync();
            result.photoHonours = await db.Honours.Select(o => o.Description).ToListAsync();

            if (!adminAccess)
            {
                result.registrationTypes = await db.RegistrationTypes.Where(w => w.ActiveOption == true).Select(o => o.RegistrationType1).ToListAsync();
            }
            else
            {
                result.registrationTypes = await db.RegistrationTypes.Select(o => o.RegistrationType1).ToListAsync();
            }

            return request.CreateResponse<ForeignKeyViewModel>(HttpStatusCode.OK, result);
        }

        private static string IntToLetters(int value)
        {
            string result = string.Empty;
            while (--value >= 0)
            {
                result = (char)('A' + value % 26) + result;
                value /= 26;
            }
            return result;
        }



        public async Task<RegistrationViewModel> GetRegistrationData(Guid userGuid)
        {
            // First apply any availability logic
            ApplyAvailailityLogic();

            decimal totalCost = db.sp_rpt_CalculateCosts2(userGuid, false).Select(s => s.Cost ?? 0.00M).ToArray().Sum();

            int linkValidDays = Int32.Parse(Settings.LinkValidDays);

            RegistrationViewModel result = new RegistrationViewModel();

            IQueryable<Registration> query = db.Registrations.Where(s => s.ValidationUid == userGuid);

            Contact currentUser = await query.Select(s => s.Contact).FirstOrDefaultAsync();
            if (currentUser == null)
            {
                // No valid registration on file
                return null;
            }


            bool billPaid = query.Select(s => s.DatePaid).FirstOrDefault() != null;

            string paymentRef = query.Select(s => s.PaymentRef).FirstOrDefault(); //CreatePaymentRef(currentUser);

            IEnumerable<FieldTripChoice> fieldTripChoicesForContact = await db.FieldTripChoices.Where(s => s.Registration.ValidationUid == userGuid).ToArrayAsync();

            List<FieldTripOptionsAndChoices> FieldTrips = new List<FieldTripOptionsAndChoices>();  // options and choices with description

            var fieldTripQuery = db.FieldTrips.Select(s => s.FieldTripId);

            foreach (var availableFieldTripId in fieldTripQuery)
            {
                FieldTripOptionsAndChoices fieldTripOptionsAndChoices = new FieldTripOptionsAndChoices();
                List<string> currentFieldOptions = await db.FieldTripOptions.Where(s => s.FieldTripId == availableFieldTripId).OrderBy(o => o.Description).Select(o => o.Description).ToListAsync();

                fieldTripOptionsAndChoices.options = currentFieldOptions;
                fieldTripOptionsAndChoices.choices = GetCurrentFieldtripChoices(fieldTripChoicesForContact, availableFieldTripId);
                fieldTripOptionsAndChoices.fieldTripDescription = db.FieldTrips.Where(s => s.FieldTripId == availableFieldTripId).Select(o => o.Description).FirstOrDefault();
                FieldTrips.Add(fieldTripOptionsAndChoices);
            }


            IEnumerable<Workshop> workshopChoicesForContact = await db.Workshops.Where(s => s.Registration.ValidationUid == userGuid).ToArrayAsync();
            List<WorkshopChoices> workshops = GetCurrentWorkshopChoices(workshopChoicesForContact);

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

            var testIfLinkActive = query.Select(o => o.InitialCreationDate ?? DateTime.Today).FirstOrDefault().AddDays(linkValidDays) > DateTime.Now;
            try
            {
                string DefaultRegType = db.RegistrationTypes.Where(s => s.Default == true).Select(o => o.RegistrationType1).DefaultIfEmpty("").First();

                result = new RegistrationViewModel()
                {
                    // userDetails = new UserDetails() { email = "test@test.com", firstName = "", lastName = "" },
                    fieldTrips = FieldTrips,
                    workshops = workshops,
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
                        photoClubs = currentUser.PhotoClubContactLinks.Select(s => s.PhotoClub.Description).ToList(),
                        mobileNumber = currentUser.MobileNumber
                    },
                    //userDetails = new UserDetails() { firstName = "Graeme", lastName = "Atkinson", homeTown = "Dunedin", email = "atkinsongraeme@hotmail.com" },
                    registrationDetails = new RegistrationDetails()
                    {
                        registrationType = query.Select(o => o.RegistrationType.RegistrationType1).FirstOrDefault() ?? DefaultRegType,
                        //canonWorkshop = query.Select(s => s.Workshops).FirstOrDefault().Select(o => o.Attending ?? false).FirstOrDefault(),
                        additionalDinnerTicket = query.Select(s => s.AdditionalDinnerTicket ?? false).FirstOrDefault(),
                        additionalDinnerName = query.Select(s => s.AdditionalDinnerName ?? "").FirstOrDefault(),
                        specialRequirements = query.Select(o => o.SpecialRequirements).FirstOrDefault(),
                        linkExpiryDate = query.Select(o => o.InitialCreationDate ?? DateTime.Today).FirstOrDefault().AddDays(linkValidDays),
                        linkExpired = !(query.Select(o => o.InitialCreationDate ?? DateTime.Today).FirstOrDefault().AddDays(linkValidDays) > DateTime.Now)
                        , totalCost = totalCost
                        , paymentRef = paymentRef
                        , billPaid = billPaid
                    },
                    // "Full convention including awards dinner" }
                    eventAccountNumber = await query.Select(s => s.Event.AccountNumber).FirstOrDefaultAsync()
                };
            }
            catch (Exception ex)
            { return null; }

            return result;
        }

        private void ApplyAvailailityLogic()
        {
            // Registration types
            var _registrationTypes = db.RegistrationTypes.ToArray();
            //var _entiesToCheck = db.Registrations.Where(w => w.RegistrationType.InactiveOnMax ?? false);
            var _entiesToCheck = db.RegistrationTypes.Where(w => w.InactiveOnMax ?? false).ToList();

            foreach (var _item in _entiesToCheck)
            {
                var _currentTotal = db.Registrations.Where(w => w.RegistrationType.RegistrationType1 == _item.RegistrationType1).Select(s => s.RegistrationId).ToList().Count();
                if (_item.MaximumNumber <= _currentTotal)
                {
                    _item.ActiveOption = false;
                }
            }
            try
            {
                db.SaveChanges();
            }
            catch { }

            var _entiesToCheck2 = db.RegistrationTypes.Where(w => w.ActivateOnInactiveType ?? false).ToList();

            foreach (var _item in _entiesToCheck2)
            {
                bool _inactiveTypeIsTriggered = _entiesToCheck.Where(w => w.ActiveOption == false & _item.InactiveType == w.RegistrationTypeId).Count() > 0;
                if (_inactiveTypeIsTriggered)
                {
                    _item.ActiveOption = true;
                }
            }
            try
            {
                db.SaveChanges();
            }
            catch { }
        }

        private List<WorkshopChoices> GetCurrentWorkshopChoices(IEnumerable<Workshop> workshopChoicesForContact)
        {
            var result = new List<WorkshopChoices>();
            
            foreach (var workshop in workshopChoicesForContact)
            {
                var choices = new WorkshopChoices();
                choices.selected = workshop.Attending ?? false;
                choices.workshopName = workshop.AvailableWorkshop.WorkshopDescription;
                result.Add(choices);
            }
            return result;
        }



        //[System.Web.Mvc.ValidateAntiForgeryToken]
        [HttpPost]
        [Route("api/saveRegistrationDetails/{userGuid}")]
        public async Task<HttpResponseMessage> SaveUserSettings(Guid userGuid, HttpRequestMessage request, [FromBody] RegistrationViewModel enteredRegisrationData)
        {
            // Read existing record from database
            // Map view model to database
            // Save updates



            Registration registration = new Registration();
            try
            {
                registration = await db.Registrations.Where(s => s.ValidationUid == userGuid).FirstOrDefaultAsync();

                registration.Contact.FirstName = enteredRegisrationData.userDetails.firstName;
                registration.Contact.LastName = enteredRegisrationData.userDetails.lastName;
                registration.Contact.HomeTown = (db.HomeTowns.Where(s => s.TownName == enteredRegisrationData.userDetails.homeTown).FirstOrDefault());
                registration.Contact.MobileNumber = enteredRegisrationData.userDetails.mobileNumber;

                registration.Contact.NZIPPMember = enteredRegisrationData.userDetails.NZIPPMember;
                registration.Contact.PSNZMember = enteredRegisrationData.userDetails.PSNZMember;
                registration.Contact.PSNZAppliedFor = enteredRegisrationData.userDetails.PSNZMemberAppliedFor;

                // Honors

                registration.RegistrationType = db.RegistrationTypes.Where(s => s.RegistrationType1 == enteredRegisrationData.registrationDetails.registrationType).FirstOrDefault();

                registration.AdditionalDinnerTicket = enteredRegisrationData.registrationDetails.additionalDinnerTicket;
                registration.AdditionalDinnerName = enteredRegisrationData.registrationDetails.additionalDinnerName;
                registration.SpecialRequirements = enteredRegisrationData.registrationDetails.specialRequirements;

                registration.PaymentRef = string.IsNullOrEmpty(enteredRegisrationData.registrationDetails.paymentRef) ? CreatePaymentRef(registration.Contact) : enteredRegisrationData.registrationDetails.paymentRef;
                //if (registrationDetails.registrationDetails.paymentRef == "")
                //    registration.PaymentRef = CreatePaymentRef(registration.Contact);
            }
            catch (Exception ex) { return request.CreateResponse(HttpStatusCode.BadRequest); }

            try
            {
                var _fieldTripsOnFile = registration.FieldTripChoices.ToArray();

                foreach (var _currentFieldTrips in _fieldTripsOnFile)
                {
                    var _description = _currentFieldTrips.FieldTrip.Description;

                    var _fieldTripsSelected = enteredRegisrationData.fieldTrips.Where(s => s.fieldTripDescription == _description).Select(o => o.choices).ToList();

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
            }
            catch (Exception ex) { return request.CreateResponse(HttpStatusCode.BadRequest); }

            try
            {
                string SQL = string.Format("delete from HonourContactLinks where ContactId = {0}", registration.Contact.ContactId.ToString());
                db.Database.ExecuteSqlCommand(SQL);

                foreach (var description in enteredRegisrationData.userDetails.photoHonours)
                {
                    var _id = db.Honours.Where(s => s.Description == description).Select(o => o.HonourId).FirstOrDefault();
                    var _honour = new Honour() { HonourId = _id };
                    db.Honours.Attach(_honour);
                    HonourContactLink link = new HonourContactLink() { Honour = _honour, RecordDeleted = false };
                    registration.Contact.HonourContactLinks.Add(link);
                }
            }
            catch (Exception ex) { return request.CreateResponse(HttpStatusCode.BadRequest); }

            try
            {
                string SQL = string.Format("delete from PhotoClubContactLinks where ContactId = {0}", registration.Contact.ContactId.ToString());
                db.Database.ExecuteSqlCommand(SQL);

                foreach (var description in enteredRegisrationData.userDetails.photoClubs)
                {
                    var _id = db.PhotoClubs.Where(s => s.Description == description).Select(o => o.PhotoClubId).FirstOrDefault();
                    var _photoClub = new PhotoClub() { PhotoClubId = _id };
                    db.PhotoClubs.Attach(_photoClub);
                    PhotoClubContactLink link = new PhotoClubContactLink() { PhotoClub = _photoClub, RecordDeleted = false };
                    registration.Contact.PhotoClubContactLinks.Add(link);
                }
            }
            catch (Exception ex) { return request.CreateResponse(HttpStatusCode.BadRequest); }


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

            //var zz = registration.Workshops.FirstOrDefault();
            //zz.Attending = enteredRegisrationData.registrationDetails.canonWorkshop;
            /// Workshops
            try
            {
                foreach (var _workshop in enteredRegisrationData.workshops)
                {
                    var wsID = db.AvailableWorkshops.Where(sw => sw.WorkshopDescription == _workshop.workshopName).Select(i => i.AvailableWorkshopId).FirstOrDefault();
                    registration.Workshops.Where(s => s.AvailableWorkshop.AvailableWorkshopId == wsID).FirstOrDefault().Attending = _workshop.selected;
                }
            }
            catch (Exception ex) { return request.CreateResponse(HttpStatusCode.BadRequest); }

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
            catch (Exception ex) { return request.CreateResponse(HttpStatusCode.BadRequest); }

            return request.CreateResponse(HttpStatusCode.OK);
            //else
            //    return request.CreateResponse(HttpStatusCode.BadRequest);
        }

        [HttpPost]
        [Route("RegisterEmail/{eventid}")]
        //public async Task<HttpResponseMessage> RegisterEmail(Guid eventid, [FromBody] string returnedEmail)
        public async Task<HttpResponseMessage> RegisterEmail(Guid eventid, [FromBody] Newtonsoft.Json.Linq.JObject data)
        {
            string sourceUriTxt = "";
            string returnedEmail = "";
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

            Newtonsoft.Json.Linq.JToken j = (Newtonsoft.Json.Linq.JToken)data.SelectToken("recaptcha");
            Type type = typeof(string);
            string recapchaResponse = (string)System.Convert.ChangeType(j.ToString(), type);

            ReCaptcha.RecapchaControl challengeTest = new ReCaptcha.RecapchaControl();
            if (!challengeTest.CheckRecapchaChallenge(recapchaResponse))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            j = (Newtonsoft.Json.Linq.JToken)data.SelectToken("email");
            returnedEmail = (string)System.Convert.ChangeType(j.ToString(), type);

            try
            {
                if (returnedEmail != null)
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
                        Email = HttpUtility.HtmlEncode(returnedEmail),
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
                        bool testMode = Settings.TestMode;
                        HttpStatusCode emailStatus = await SendEmail(baseRegistration.Email, @event.ContactEmail, registrationUid.ToString(), sourceUriTxt, testMode);

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

 

        static async Task<HttpStatusCode> SendEmail(string eMail, string registrationEmail, string registrationUid, string sourceUrlTxt, bool testMode)
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
            var subject = "Register for the 66th PSNZ National Convention, Dunedin";
            var to = new EmailAddress(eMail);


            var plainTextContent = "Thank you for your interest in the 66th PSNZ National Convention.";
            var htmlContent = "<strong>" + plainTextContent + "</strong><br />";
            htmlContent += String.Format("Please follow this link to complete the registration process: <a id='register' href={0}>{1}</a>", registrationUri, "register now.");
            htmlContent += String.Format("<br/>This link will expire in 24 hours.");
            htmlContent += String.Format("<br /><br />{0}<br />{1}<br />{2}", "Kind regards,", "2018 National Convention Organising Committee", "www.naturallydunedin.co.nz");
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            if (testMode)
            {
                return HttpStatusCode.OK;
            }
            else
            {
                var response = await client.SendEmailAsync(msg);
                return response.StatusCode;
            }
        }

        static async Task<HttpStatusCode> SendConfirmationEmail(RegistrationViewModel registrationDetails, Event eventDetails, DateTime expiryDate, string copyToEmail)
        {
            // Base class from ISD core application


            var apiKey = Settings.GraphApiKey;
            int payByDays = Settings.PayByDays;

            //    <add key = "RegistrationUrlApi" value="{0}/Home/RegisterMe?Registration={1}" />
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(eventDetails.ContactEmail, "Registrations");
            // var subject = String.Format("{0} : Confirmation", eventDetails.EventName);
            var subject = String.Format("Automated Acknowledgement of Registration for 66th PSNZ National Convention, Dunedin");
            var to = new EmailAddress(HttpUtility.HtmlEncode(registrationDetails.userDetails.email));


            var plainTextContent = eventDetails.EventName + "\n\nThank you for your registration\n\n";

            string htmlContent = FormatSummaryMessage(registrationDetails, payByDays);

            htmlContent += String.Format("<br /><br />");

            htmlContent = FormatUserDetails(registrationDetails, htmlContent);
            htmlContent += String.Format("<br />");
            htmlContent = FormatRegistrationDetails(registrationDetails, htmlContent);

            // htmlContent += String.Format("If you need to make changes, please contact <b>natcon2018registrar@gmail.com</b><br /><br />");
            htmlContent += String.Format("If you need to make changes, please contact <b><a href='mailto:{0}?Subject={1}%20{2}' target='_top'>{0}</a></b><br /><br />",
                "natcon2018registrar@gmail.com", "Registration%20change%20request", registrationDetails.registrationDetails.paymentRef);
            //<a href="mailto:natcon2018registrar@gmail.com?Subject=Registration%20enquiry" target="_top">natcon2018registrar@gmail.com</a>
            htmlContent += String.Format("Kind regards,<br />2018 National Convention Organising Committee<br />");
            htmlContent += String.Format("www.naturallydunedin.co.nz");

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);

            if  (copyToEmail != "")
            {
                to = new EmailAddress(HttpUtility.HtmlEncode(copyToEmail));
                subject += " **COPY**";
                msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                response = await client.SendEmailAsync(msg);
            }

            // Status code not handled at the moment
            return response.StatusCode;
        }

        private static string FormatUserDetails(RegistrationViewModel registrationDetails, string htmlContent)
        {
            //First Name: Melanie
            //Last Name: Middlemiss
            //Home Town: Dunedin
            //Mobile Number: 123456

            //PSNZ member

            //Photographic Honours: APSNZ, LPSNZ, FPSNZ

            //Clubs / Societies you belong to: Dunedin Photographic Society, Dunedin Camera Clubs / Societies

            htmlContent += String.Format("<b>First Name:</b> {0}<br /><b>Last Name: </b>{1}<br /><b>Home Town: </b>{2}<br /><b>Mobile Number: </b>{3}<br /><br /><b>PSNZ member? </b>{4}<br /><br />",
                registrationDetails.userDetails.firstName, registrationDetails.userDetails.lastName, registrationDetails.userDetails.homeTown, registrationDetails.userDetails.mobileNumber,
                registrationDetails.userDetails.PSNZMember);
            htmlContent += String.Format("<b>Photographic Honours: </b>");
            htmlContent += String.Format("{0}<br/>", CreateFKItemList(registrationDetails.userDetails.photoHonours));
            htmlContent += "<br/>";
            htmlContent += String.Format("<b>Clubs / Societies you belong to: </b>");
            htmlContent += String.Format("{0}<br/>", CreateFKItemList(registrationDetails.userDetails.photoClubs));

            return htmlContent;
        }

        private static string CreateFKItemList(List<string> itemList)
        {
            string result = "";
            string comma = "";
            foreach (var item in itemList)
            {
                result += String.Format("{0} {1}", comma, item);
                comma = ",";
            }
            return result;
        }

        private static string FormatRegistrationDetails(RegistrationViewModel registrationDetails, string htmlContent)
        {
            htmlContent += String.Format("<strong>Registration</strong> : {0}<br /><br />", registrationDetails.registrationDetails.registrationType);
            if (registrationDetails.registrationDetails.additionalDinnerTicket)
            {
                htmlContent += String.Format("<strong>Additional Diners Name</strong> : {0}<br /><br />", registrationDetails.registrationDetails.additionalDinnerName);
            }
            htmlContent += String.Format("<strong>Special Requirements</strong> : {0}<br /><br />", registrationDetails.registrationDetails.specialRequirements);

            htmlContent += String.Format("<strong>Field Trip Choices</strong><br />");
            foreach (var item in registrationDetails.fieldTrips)
            {
                htmlContent += String.Format("{0}:<br />", item.fieldTripDescription);
                int loopcount = 0;
                htmlContent += String.Format("<ul>");
                foreach (var choice in item.choices)
                {
                    loopcount++;
                    htmlContent += String.Format("<li>{0} choice: {1}</li>", UppercaseFirst(loopcount.ToOrdinalWords()), choice);
                }
                htmlContent += String.Format("</ul>");
            }

            htmlContent += String.Format("<strong>Workshops:</strong><br />");
            htmlContent += String.Format("<ul>");

            foreach (var _workshop in registrationDetails.workshops)
            {
                htmlContent += String.Format("<li>{0}: {1}<br/></li>", _workshop.workshopName, _workshop.selected ? "Yes" : "No");
            }
            htmlContent += String.Format("</ul>");
            htmlContent += String.Format("<br/><br/>");
            //           htmlContent += String.Format("<strong>Attend Canon Workshop ?</strong> {0}<br /><br />", registrationDetails.registrationDetails.canonWorkshop ? "Yes" : "No");
            return htmlContent;
        }

        private static string FormatSummaryMessage(RegistrationViewModel registrationDetails, int payByDays)
        {
            var htmlContent = String.Format("<b>66th PSNZ National Convention, 19-22nd April 2018, Dunedin</b><br /><br />");
            // htmlContent += String.Format("<strong>{0}</strong><br/><br />Thank you for your registration<br /><br />", eventDetails.EventName);
            htmlContent += String.Format("<br />Thank you for your registration. A summary of your registration is given below. ");
            htmlContent += String.Format("If any of these details are incorrect, please contact <b>{0}</b><br /><br />", "natcon2018registrar@gmail.com");
            //                registrationDetails.registrationDetails.linkExpiryDate.ToString("dd/MM/yyy"));
            htmlContent += String.Format("<p style='color: red;'>Payment is of {0} is required by {1} to confirm your registration.</p>", registrationDetails.registrationDetails.totalCost.ToString("C2"),
                registrationDetails.registrationDetails.linkExpiryDate.AddDays(payByDays).ToString("dd/MM/yyy"));
            htmlContent += String.Format("Any additional field trip costs will be invoiced separately when field trip allocations are confirmed. ");
            htmlContent += String.Format("Field trip confirmation will start from mid January 2018.<br /><br />");

            htmlContent += String.Format("<strong>Registration Cost</strong> : {0}<br />", registrationDetails.registrationDetails.totalCost.ToString("C2"));
            htmlContent += String.Format("<strong>Reference Code for Payment</strong> : {0} (please use this code in the reference field when paying)<br />", registrationDetails.registrationDetails.paymentRef);
            htmlContent += String.Format("<strong>Bank Account for direct credit</strong> : {0}<br />", registrationDetails.eventAccountNumber);
            return htmlContent;
        }

        // Private functions

        private static string UppercaseFirst(string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        private static int[] GetIntArray(int num)
        {
            List<int> listOfInts = new List<int>();
            while (num > 0)
            {
                listOfInts.Add(num % 10);
                num = num / 10;
            }
            listOfInts.Reverse();
            return listOfInts.ToArray();
        }

        private static string GetUniquePayCode(int id, int length)
        {
            double i = 10;
            int count = 1;
            while (i < Math.Pow(10, length))
            {
                if (id < i)
                    id += (int)i;
                i = Math.Pow(i, count);
                count++;
            }
            //if (id < 10)
            //    id += 10;

            var _array = GetIntArray(id);

            string result = "";
            foreach (var entry in _array)
            {
                result += IntToLetters(entry);
            }
            return result;
        }

        private string CreatePaymentRef(Contact currentUser)
        {
            return String.Format("{0}{1}", currentUser.FirstName[0], currentUser.LastName[0] + GetUniquePayCode(currentUser.ContactId, Settings.RegistrationCodeLength));
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

    }

 
}