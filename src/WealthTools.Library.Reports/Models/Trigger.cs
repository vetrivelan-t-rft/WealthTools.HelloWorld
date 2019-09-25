using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace WealthTools.Library.Reports.Models
{
    public class Trigger
    {
        [XmlElement("text")]
        public string text;
        [XmlAttribute("priority")]
        public int priority;
        [XmlAttribute("font_size")]
        public int fontSize;
        [XmlAttribute("placement_id")]
        public int placementId;
        [XmlAttribute("id")]
        public int id;
        [XmlAttribute("type_id")]
        public int typeId;
        [XmlAttribute("text_id")]
        public int textId;
        [XmlAttribute("name")]
        public string _name;
        [XmlAttribute("descr")]
        public string _descr;

        public Trigger() { }
    }
}
