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
    
    public partial class Workshop
    {
        public int WorkshopId { get; set; }
        public Nullable<int> RegistrationId { get; set; }
        public Nullable<int> AvailableWorkshopId { get; set; }
        public Nullable<bool> Attending { get; set; }
        public Nullable<bool> RecordDeleted { get; set; }
    
        public virtual AvailableWorkshop AvailableWorkshop { get; set; }
        public virtual Registration Registration { get; set; }
    }
}
