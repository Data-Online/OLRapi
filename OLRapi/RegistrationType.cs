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
    
    public partial class RegistrationType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public RegistrationType()
        {
            this.Registrations = new HashSet<Registration>();
            this.RegistrationTypes1 = new HashSet<RegistrationType>();
        }
    
        public int RegistrationTypeId { get; set; }
        public Nullable<int> EventId { get; set; }
        public string RegistrationType1 { get; set; }
        public Nullable<decimal> CostMember { get; set; }
        public Nullable<decimal> CostNonMember { get; set; }
        public Nullable<bool> Default { get; set; }
        public Nullable<bool> ActiveOption { get; set; }
        public Nullable<int> MaximumNumber { get; set; }
        public Nullable<bool> InactiveOnMax { get; set; }
        public Nullable<bool> ActivateOnInactiveType { get; set; }
        public Nullable<int> InactiveType { get; set; }
    
        public virtual Event Event { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Registration> Registrations { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RegistrationType> RegistrationTypes1 { get; set; }
        public virtual RegistrationType RegistrationType2 { get; set; }
    }
}
