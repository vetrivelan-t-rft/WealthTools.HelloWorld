using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace WealthTools.Library.BrokerManager.Models
{

    public class Broker
    {
        [XmlElement("contact_details", typeof(ContactDetails))]
        public ArrayList contactInfoList { get; set; } = new ArrayList();
        [XmlElement("address", typeof(Address))]
        public ArrayList addressInfoList { get; set; } = new ArrayList();

       

        [XmlAttribute("Suffix")] public string Suffix { get; set; }
       [XmlAttribute("clrg_country_name")]  public string Clrg_country_name { get; set; }
         [XmlAttribute("firm_country_name")] public string Firm_country_name { get; set; }
        [XmlAttribute("sub_firm_no")] public string Sub_Firm_No { get; set; }
        [XmlAttribute("primary_state")] public string Primary_state { get; set; }
        [XmlAttribute("affiliation")]        public string Affiliation { get; set; }
        [XmlAttribute("clearing_firm_name")]        public string Clearing_firm_name { get; set; }
        [XmlAttribute("firm_name")]        public string Firm_name { get; set; }
        [XmlAttribute("passwd")]        public string Passwd { get; set; }
         [XmlAttribute("user_name")]        public string User_name { get; set; }
        [XmlAttribute("title")]        public string Title { get; set; }
        [XmlAttribute("sub_firm_num")]        public string Sub_Firm_Num { get; set; }
        [XmlAttribute("INSTITUTION_ID")]        public string InstitutionId { get; set; }
        [XmlAttribute("SUB_ID")]        public string SUB_ID { get; set; }
        [XmlAttribute("name")]        public string Name { get; set; }
        [XmlAttribute("USERNAME")]        public string USERNAME { get; set; }
        [XmlAttribute("broker_id")]        public string BrokerId { get; set; }
        public string Email { get; set; }
        public string FirmPhone { get; set; }
    }

    public class Address 
    {
        public Address() { }

        [System.Runtime.Serialization.DataMemberAttribute]
        [XmlAttribute("AddressType")]
        public AddressType Address_Type { get; set; }
        [System.Runtime.Serialization.DataMemberAttribute]
        [XmlAttribute("AddressLine1")]
        public string AddressLine1 { get; set; }
        [System.Runtime.Serialization.DataMemberAttribute]
        [XmlAttribute("AddressLine2")]
        public string AddressLine2 { get; set; }
        [System.Runtime.Serialization.DataMemberAttribute]
        [XmlAttribute("City")]
        public string City { get; set; }
        [System.Runtime.Serialization.DataMemberAttribute]
        [XmlAttribute("State")]
        public string State { get; set; }
        [System.Runtime.Serialization.DataMemberAttribute]
        [XmlAttribute("ZipCode")]
        public string ZipCode { get; set; }
        [System.Runtime.Serialization.DataMemberAttribute]
        [XmlAttribute("Country")]
        public string Country { get; set; }

        //public object Clone();
        //public void MapData(IDataReader reader, AddressType addrType);
    }

   public class ContactDetails
   {
        [XmlAttribute("contact_type")]
        public string Contact_type { get; set; }
        [XmlAttribute("phone")]
        public string Phone { get; set; }
        [XmlAttribute("fax")]
        public string Fax { get; set; }
        [XmlAttribute("email")]
        public string Email { get; set; }

        
    }

}
