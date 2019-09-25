using System.Collections.Generic;

namespace WealthTools.Library.Contacts.Models
{
   public class SearchResult
    {      
        public string HouseholdID {get; set;}       
        public string Type {get; set;}
        public string Name { get; set; }
        public List<Contact> Persons { get; set; } = new List<Contact>();    
        
    }

    public class Contact
    {
        public RelationShipType RelationShipType { get; set; }
        public string InvestorId { get; set; }       
        public string HouseholdId { get; set; }             
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string Suffix { get; set; }
        public string AddressLine1 { get; set; }        
        public string AddressLine2 { get; set; }        
        public string HomePhone { get; set; }
        public string Phone { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Zip { get; set; }       
        public string PersonalEmail { get; set; }
        public string Short_name { get; set; }
    }

   
 
}