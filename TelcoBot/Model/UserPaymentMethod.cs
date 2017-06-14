/*  
 *  UserPaymentMethod.cs
 *
 *  SoftSource Consulting, Inc.
 */

using System;
using System.Xml.Serialization;

namespace TelcoBot.Model
{
    [Serializable]
    [XmlRoot("UserPaymentMethod")]
    public class UserPaymentMethod : IIdentified
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("UserId")]
        public int UserId { get; set; }

        [XmlElement("Description")]
        public string Description { get; set; }

        [XmlElement("Type")]
        public string Type { get; set; }

        [XmlElement("Identifier")]
        public string Identifier { get; set; }

        [XmlElement("Image")]
        public string Image { get; set; }

        public override string ToString()
        {
            return Description;
        }
    }
}