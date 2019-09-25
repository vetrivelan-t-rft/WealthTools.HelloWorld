using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace WealthTools.Library.Reports.Models
{
    [XmlRoot("PortAnalytics_Cover_Page_Report")]
    public class PortAnalytics_Cover_Page_Report
    {
        public PortAnalytics_Cover_Page_Report() { }
        [XmlElement("CoverHeader")]
        public CoverHeader CoverHeader { get; set; }
        [XmlElement("Preferences")]
        public Preferences Preferences { get; set; }
        [XmlElement("PreparedForData")]
        public PreparedForData PreparedForData { get; set; }
        [XmlElement("ContactInfo")]
        public ContactInfo ContactInfo { get; set; }
        [XmlElement("bank_disclosure")]
        public bank_disclosure bank_disclosure { get; set; } = new bank_disclosure();
        
        
        [XmlAttribute("WatermarkImg")]
        public string WatermarkImg { get; set; }
        [XmlAttribute("DisplayWatermark")]
        public bool DisplayWatermark { get; set; }       
       
        [XmlAttribute("FooterImg")]
        public string FooterImg { get; set; }
        [XmlAttribute("Copyright")]
        public string Copyright { get; set; }
        [XmlAttribute("img")]
        public string img { get; set; }
        [XmlAttribute("PrintType")]
        public string PrintType { get; set; }
        [XmlAttribute("PreparedFor")]
        public string PreparedFor { get; set; }
        [XmlAttribute("PreparedByEmail")]
        public string PreparedByEmail { get; set; }
        [XmlAttribute("PreparedByTitle")]
        public string PreparedByTitle { get; set; }
        [XmlAttribute("PreparedBy")]
        public string PreparedBy { get; set; }
        [XmlAttribute("ReportDate")]
        public string ReportDate { get; set; }
        [XmlAttribute("ReportName")]
        public string ReportName { get; set; }
        [XmlElement("cover_disclaimer")]
        public bank_disclosure cover_disclaimer { get; set; } = new bank_disclosure();
        [XmlAttribute("ProgramName")]
        public string ProgramName { get; set; }
    }

    public class PreparedForData
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("addr1")]
        public string addr1 { get; set; }
        [XmlAttribute("addr2")]
        public string addr2 { get; set; }
        [XmlAttribute("city")]
        public string city { get; set; }
        [XmlAttribute("zipcode")]
        public string zipcode { get; set; }
    }

    [XmlRoot("ContactInfo")]
    public class ContactInfo
    {
        [XmlElement("AdvisorInfo")]
        public AdvisorInfo AdvisorInfo { get; set; } = new AdvisorInfo();
        [XmlElement("ClearingFirmInfo")]
        public ClearingFirmInfo ClearingFirmInfo { get; set; } = new ClearingFirmInfo();
    }

    public class AdvisorInfo
    {
        [XmlAttribute("firm_name")]
        public string firm_name { get; set; }
        [XmlAttribute("addr1")]
        public string addr1 { get; set; }
        [XmlAttribute("addr2")]
        public string addr2 { get; set; }
        [XmlAttribute("city")]
        public string city { get; set; }
        [XmlAttribute("addr3")]
        public string addr3 { get; set; }
        [XmlAttribute("country")]
        public string country { get; set; }
        [XmlAttribute("phone")]
        public string phone { get; set; }
        [XmlAttribute("fax")]
        public string fax { get; set; }
        [XmlAttribute("email")]
        public string email { get; set; }
    }

    public class ClearingFirmInfo
    {
        [XmlAttribute("firm_name")]
        public string firm_name { get; set; }
        [XmlAttribute("addr1")]
        public string addr1 { get; set; }
        [XmlAttribute("addr2")]
        public string addr2 { get; set; }
        [XmlAttribute("city")]
        public string city { get; set; }
        [XmlAttribute("addr3")]
        public string addr3 { get; set; }
        [XmlAttribute("country")]
        public string country { get; set; }
        [XmlAttribute("phone")]
        public string phone { get; set; }
        [XmlAttribute("fax")]
        public string fax { get; set; }
        [XmlAttribute("email")]
        public string email { get; set; }
    }

    [XmlRoot("CoverHeader")]
    public class CoverHeader
    {
      

        [XmlAttribute("IDS_REPORT_COVER_PAGE")]
        public string IDS_REPORT_COVER_PAGE { get; set; }
        [XmlAttribute("IDS_PREPAREDFOR")]
        public string IDS_PREPAREDFOR { get; set; }
        [XmlAttribute("IDS_PREPAREDBY")]
        public string IDS_PREPAREDBY { get; set; }
        [XmlAttribute("IDS_PROFILE_PHONE")]
        public string IDS_PROFILE_PHONE { get; set; }
        [XmlAttribute("IDS_PREPAREDDATE")]
        public string IDS_PREPAREDDATE { get; set; }
    }

    public class bank_disclosure
    {
        public bank_disclosure() { }

        [XmlElement("trigger")]
        public Trigger trigger { get; set; } = new Trigger();
    }

    public class Preferences
    {
        public Preferences() { }

        [XmlElement("show_svg_logo_cvrpage")]
        public string show_svg_logo_cvrpage { get; set; }
        [XmlElement("Display_Footer_Logo")]
        public string Display_Footer_Logo { get; set; }
        [XmlElement("international_brokers_yn")]
        public string international_brokers_yn { get; set; }
        [XmlElement("report_header_bar_color")]
        public string report_header_bar_color { get; set; }
        [XmlElement("daterange_subheader_color")]
        public string Daterange_subHeader_color { get; set; }
    }
}
