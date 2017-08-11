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
        public List<FieldTripOptionsAndChoices> fieldTrips { get; set; }
        public RegistrationDetails registrationDetails { get; set; }
    }

    public class UserDetails
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string homeTown { get; set; }
        public string mobileNumber { get; set; }
        public bool PSNZMember { get; set; }
        public bool NZIPPMember { get; set; }
        public bool PSNZMemberAppliedFor { get; set; }
        public List<string> photoHonours { get; set; }
        public List<string> photoClubs { get; set; }

    }

    public class RegistrationDetails
    {
        public string registrationType { get; set; }
        public bool additionalDinnerTicket { get; set; }
        public string additionalDinnerName { get; set; }

        public string specialRequirements { get; set; }
        public bool canonWorkshop { get; set; }

        public DateTime linkExpiryDate { get; set; }
        public decimal totalCost { get; set; }
        public string paymentRef { get; set; }
        public bool billPaid { get; set; }
    }

    public class FieldTripOptionsAndChoices
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

    internal class ForeignKeyViewModel
    {
        public List<string> towns { get; set; }
        public List<string> photoClubs { get; set; }
        public List<string> photoHonours { get; set; }
        public List<string> registrationTypes { get; set; }

    }
}