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
    
    public partial class AvailableWorkshop
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public AvailableWorkshop()
        {
            this.Workshops = new HashSet<Workshop>();
        }
    
        public int AvailableWorkshopId { get; set; }
        public Nullable<int> EventId { get; set; }
        public string WorkshopDescription { get; set; }
        public Nullable<int> PlacesAvailable { get; set; }
    
        public virtual Event Event { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Workshop> Workshops { get; set; }
    }
}
