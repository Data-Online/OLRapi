﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OLRapi
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class OLR_dbEntities : DbContext
    {
        public OLR_dbEntities()
            : base("name=OLR_dbEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Contact> Contacts { get; set; }
        public virtual DbSet<Event> Events { get; set; }
        public virtual DbSet<Registration> Registrations { get; set; }
        public virtual DbSet<FieldTripChoice> FieldTripChoices { get; set; }
        public virtual DbSet<FieldTrip> FieldTrips { get; set; }
        public virtual DbSet<AvailableWorkshop> AvailableWorkshops { get; set; }
        public virtual DbSet<Workshop> Workshops { get; set; }
        public virtual DbSet<FieldTripOption> FieldTripOptions { get; set; }
        public virtual DbSet<HomeTown> HomeTowns { get; set; }
        public virtual DbSet<Honour> Honours { get; set; }
        public virtual DbSet<PhotoClub> PhotoClubs { get; set; }
        public virtual DbSet<RegistrationType> RegistrationTypes { get; set; }
        public virtual DbSet<HonourContactLink> HonourContactLinks { get; set; }
        public virtual DbSet<PhotoClubContactLink> PhotoClubContactLinks { get; set; }
        public virtual DbSet<Configuration> Configurations { get; set; }
    
        public virtual ObjectResult<Nullable<decimal>> sp_rpt_CalculateCosts(Nullable<System.Guid> guid)
        {
            var guidParameter = guid.HasValue ?
                new ObjectParameter("guid", guid) :
                new ObjectParameter("guid", typeof(System.Guid));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<decimal>>("sp_rpt_CalculateCosts", guidParameter);
        }
    
        public virtual ObjectResult<sp_rpt_CalculateCosts1_Result> sp_rpt_CalculateCosts1(Nullable<System.Guid> guid)
        {
            var guidParameter = guid.HasValue ?
                new ObjectParameter("guid", guid) :
                new ObjectParameter("guid", typeof(System.Guid));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_rpt_CalculateCosts1_Result>("sp_rpt_CalculateCosts1", guidParameter);
        }
    
        public virtual ObjectResult<sp_rpt_CalculateCosts2_Result> sp_rpt_CalculateCosts2(Nullable<System.Guid> guid, Nullable<bool> includeFieldTrips)
        {
            var guidParameter = guid.HasValue ?
                new ObjectParameter("guid", guid) :
                new ObjectParameter("guid", typeof(System.Guid));
    
            var includeFieldTripsParameter = includeFieldTrips.HasValue ?
                new ObjectParameter("includeFieldTrips", includeFieldTrips) :
                new ObjectParameter("includeFieldTrips", typeof(bool));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_rpt_CalculateCosts2_Result>("sp_rpt_CalculateCosts2", guidParameter, includeFieldTripsParameter);
        }
    }
}
