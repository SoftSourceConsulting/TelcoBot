/*  
 *  Bill.cs
 *
 *  SoftSource Consulting, Inc.
 */

using System;
using System.Xml.Serialization;

namespace TelcoBot.Model
{
    [Serializable]
    [XmlRoot("Bill")]
    public class Bill : IIdentified
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("UserId")]
        public int UserId { get; set; }

        [XmlElement("Year")]
        public int Year { get; set; }

        [XmlElement("Month")]
        public int Month { get; set; }

        [XmlElement("Amount")]
        public decimal Amount { get; set; }

        [XmlElement("IsPaid")]
        public bool IsPaid { get; set; }

        public override string ToString()
        {
            return $"{Month}/{Year} bill";
        }
    }
}