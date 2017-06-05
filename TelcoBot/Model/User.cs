/*  
 *  User.cs
 *  Neil McKamey-Gonzalez
 *  Softsource Consulting, Inc.
 */

using System;
using System.Xml.Serialization;

namespace TelcoBot.Model
{
    [Serializable]
    [XmlRoot("User")]
    public class User : IIdentified
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("FirstName")]
        public string FirstName { get; set; }

        [XmlElement("LastName")]
        public string LastName { get; set; }

        [XmlElement("PIN")]
        public int PIN { get; set; }

        [XmlElement("BotId")]
        public string BotId { get; set; }

        [XmlElement("IdInChannel")]
        public string IdInChannel { get; set; }

        [XmlElement("InternetServiceLevelId")]
        public int InternetServiceLevelId { get; set; }

        public override string ToString()
        {
            return FirstName + " " + LastName;
        }
    }
}