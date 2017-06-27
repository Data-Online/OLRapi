using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using OLRapi;

namespace OLRapi.Controllers
{
    public class VerifyDataController : Controller
    {
        private OLR_dbEntities db = new OLR_dbEntities();

        // GET: VerifyData
        public async Task<ActionResult> Index()
        {
            var registrations = db.Registrations.Include(r => r.Contact).Include(r => r.Event);
            return View(await registrations.ToListAsync());
        }

        // GET: VerifyData/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Registration registration = await db.Registrations.FindAsync(id);
            if (registration == null)
            {
                return HttpNotFound();
            }
            return View(registration);
        }

        // GET: VerifyData/Create
        public ActionResult Create()
        {
            ViewBag.ContactId = new SelectList(db.Contacts, "ContactId", "Address1");
            ViewBag.EventId = new SelectList(db.Events, "EventId", "EventName");
            return View();
        }

        // POST: VerifyData/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "RegistrationId,EventId,ContactId,ValidationUid,RegistrationTypeId,PaymentDate,AdditionalDinnerTicket,SpecialRequirements,AdditionalDinnerName,DatePaid,InitialCreationDate")] Registration registration)
        {
            if (ModelState.IsValid)
            {
                db.Registrations.Add(registration);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.ContactId = new SelectList(db.Contacts, "ContactId", "Address1", registration.ContactId);
            ViewBag.EventId = new SelectList(db.Events, "EventId", "EventName", registration.EventId);
            return View(registration);
        }

        // GET: VerifyData/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Registration registration = await db.Registrations.FindAsync(id);
            if (registration == null)
            {
                return HttpNotFound();
            }
            ViewBag.ContactId = new SelectList(db.Contacts, "ContactId", "Address1", registration.ContactId);
            ViewBag.EventId = new SelectList(db.Events, "EventId", "EventName", registration.EventId);
            return View(registration);
        }

        // POST: VerifyData/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "RegistrationId,EventId,ContactId,ValidationUid,RegistrationTypeId,PaymentDate,AdditionalDinnerTicket,SpecialRequirements,AdditionalDinnerName,DatePaid,InitialCreationDate")] Registration registration)
        {
            if (ModelState.IsValid)
            {
                db.Entry(registration).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.ContactId = new SelectList(db.Contacts, "ContactId", "Address1", registration.ContactId);
            ViewBag.EventId = new SelectList(db.Events, "EventId", "EventName", registration.EventId);
            return View(registration);
        }

        // GET: VerifyData/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Registration registration = await db.Registrations.FindAsync(id);
            if (registration == null)
            {
                return HttpNotFound();
            }
            return View(registration);
        }

        // POST: VerifyData/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Registration registration = await db.Registrations.FindAsync(id);
            db.Registrations.Remove(registration);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
