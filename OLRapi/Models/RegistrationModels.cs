using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OLRapi.Models
{
    public class BaseRegistration
    {
        public string id { get; set; }
        public DateTime Date { get; set; }
        public string Email { get; set; }
    }

    public class RegistrationViewModel
    {
        public UserDetails userDetails { get; set; }
        public FieldTripOptions fieldTrip1 { get; set; }

        public List<FieldTripOptions> fieldTrips { get; set; }
    }

    public class UserDetails
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string mobileNumber { get; set; }
        public bool PSNZMember { get; set; }
        public bool NZIPPMember { get; set; }
        public bool PSNZMemberAppliedFor { get; set; }

    }

    public class FieldTripOptions
    {
        public string fieldTripDescription { get; set; }
        public List<string> options { get; set; }
        public List<string> choices { get; set; }
        //public List<ListType> options { get; set; }
    }

    public class ListType
    {
        public int key { get; set; }
        public string value { get; set; }
    }

}