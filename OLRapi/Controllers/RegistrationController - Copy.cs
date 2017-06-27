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

namespace OLRapi.Controllers
{
    public class RegistrationController : ApiController
    {
        private OLR_dbEntities db = new OLR_dbEntities();

        // GET: api/Registration
        public IQueryable<Event> GetEvents()
        {
            return db.Events;
        }

        // GET: api/Registration/5
        [ResponseType(typeof(Event))]
        public async Task<IHttpActionResult> GetEvent(int id)
        {
            Event @event = await db.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }

            return Ok(@event);
        }

        // PUT: api/Registration/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutEvent(int id, Event @event)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != @event.EventId)
            {
                return BadRequest();
            }

            db.Entry(@event).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }
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

                Registration registration = new Registration() { Contact = new Contact() { Email = baseRegistration.Email }, EventId = @event.EventId };

                db.Registrations.Add(registration);

                await db.SaveChangesAsync();

                var verify = db.Events.Select(s => s.Registrations.Where(w => w.EventId == @event.EventId && w.Contact.Email == baseRegistration.Email)).FirstOrDefaultAsync();

                if (verify != null)
                {
                    // var id = Guid.NewGuid();

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
        // POST: api/Registration
        [ResponseType(typeof(Event))]
        public async Task<IHttpActionResult> PostEvent(Event @event)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Events.Add(@event);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = @event.EventId }, @event);
        }

        // DELETE: api/Registration/5
        [ResponseType(typeof(Event))]
        public async Task<IHttpActionResult> DeleteEvent(int id)
        {
            Event @event = await db.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }

            db.Events.Remove(@event);
            await db.SaveChangesAsync();

            return Ok(@event);
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
    }
}