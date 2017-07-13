//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class Registration
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Registration()
        {
            this.FieldTripChoices = new HashSet<FieldTripChoice>();
            this.Workshops = new HashSet<Workshop>();
        }
    
        public int RegistrationId { get; set; }
        public Nullable<int> EventId { get; set; }
        public Nullable<int> ContactId { get; set; }
        public Nullable<System.Guid> ValidationUid { get; set; }
        public Nullable<int> RegistrationTypeId { get; set; }
        public Nullable<bool> AdditionalDinnerTicket { get; set; }
        public string SpecialRequirements { get; set; }
        public string AdditionalDinnerName { get; set; }
        public Nullable<System.DateTime> DatePaid { get; set; }
        public Nullable<System.DateTime> InitialCreationDate { get; set; }
        public Nullable<bool> RecordDeleted { get; set; }
    
        public virtual Contact Contact { get; set; }
        public virtual Event Event { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FieldTripChoice> FieldTripChoices { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Workshop> Workshops { get; set; }
    }
}
