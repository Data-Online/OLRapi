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
    
    public partial class FieldTripOption
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public FieldTripOption()
        {
            this.FieldTripChoices = new HashSet<FieldTripChoice>();
            this.FieldTripChoices1 = new HashSet<FieldTripChoice>();
            this.FieldTripChoices2 = new HashSet<FieldTripChoice>();
            this.FieldTripChoices3 = new HashSet<FieldTripChoice>();
        }
    
        public int FieldTripOptionId { get; set; }
        public Nullable<int> FieldTripId { get; set; }
        public Nullable<int> PlacesAvailable { get; set; }
        public string Description { get; set; }
        public Nullable<decimal> Cost { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FieldTripChoice> FieldTripChoices { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FieldTripChoice> FieldTripChoices1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FieldTripChoice> FieldTripChoices2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FieldTripChoice> FieldTripChoices3 { get; set; }
        public virtual FieldTrip FieldTrip { get; set; }
    }
}
