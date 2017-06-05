/*  
 *  InternetServiceLevel.cs
 *  Neil McKamey-Gonzalez
 *  Softsource Consulting, Inc.
 */

using System;
using System.Xml.Serialization;

namespace TelcoBot.Model
{
    [Serializable]
    [XmlRoot("InternetServiceLevel")]

    public class InternetServiceLevel : IIdentified
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("Description")]
        public string Description { get; set; }

        [XmlElement("Price")]
        public decimal Price { get; set; }

        [XmlElement("Image")]
        public string Image { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}