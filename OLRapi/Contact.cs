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
    
    public partial class Contact
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Contact()
        {
            this.Registrations = new HashSet<Registration>();
        }
    
        public int ContactId { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public Nullable<int> HometownId { get; set; }
        public string MobileNumber { get; set; }
        public Nullable<bool> PSNZMembershipCheck { get; set; }
        public Nullable<bool> PSNZMember { get; set; }
        public string Email { get; set; }
        public Nullable<bool> PSNZAppliedFor { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Nullable<bool> RecordDeleted { get; set; }
        public Nullable<bool> NZIPPMember { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Registration> Registrations { get; set; }
        public virtual HomeTown HomeTown { get; set; }
    }
}
