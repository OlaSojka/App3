using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace App3
{
   public class Question
    {
        [XmlElement(ElementName = "ID")]
        public int ID;

        [XmlElement(ElementName = "isConclusion")]
        public bool isConclusion;

        [XmlElement(ElementName = "yes")]
        public int yes;

        [XmlElement(ElementName = "no")]
        public int no;

        [XmlElement(ElementName = "content")]
        public string content;
    }

    public class Source
    {
        [XmlElement(ElementName = "linkDescription")]
        public string linkDescription;

        [XmlElement(ElementName = "link")]
        public string link;
    }

    [System.Xml.Serialization.XmlRoot("expertSystem")]
    public class ExpertSystem
    {
        [XmlElement(ElementName = "title")]
        public string title;

        [XmlElement(ElementName = "description")]
        public string description;

        [XmlArray("questions")]
        [XmlArrayItem("question")]
        public List<Question> questions = new List<Question>();

        [XmlArray("sources")]
        [XmlArrayItem("source")]
        public List<Source> sources = new List<Source>();

        public ExpertSystem()
        {
            questions = new List<Question>();
            sources = new List<Source>();
        }
    }
}