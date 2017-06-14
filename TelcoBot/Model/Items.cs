/*  
 *  Items.cs
 *
 *  SoftSource Consulting, Inc.
 */

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using TelcoBot.Persistence;

namespace TelcoBot.Model
{
    /// <summary>
    /// Provides a top-level element for XML trees, for use in for XML serialization/deserialization.
    /// It has to be defined at the model level because it has attributes specifying the types it can be used to serialize.
    /// Fully-specified XmlElement attributes should be added to the Collection property for any additional model types that need deserialization.
    /// </summary>
    [XmlRoot("Items")]
    [Serializable]
    public class Items : ICollectionRoot
    {
        // The types are required on these attributes because we're deserializing into an objects collection, 
        //  so the normal type inference proccess won't work.
        [XmlElement("User", typeof(User))]
        [XmlElement("Bill", typeof(Bill))]
        [XmlElement("UserPaymentMethod", typeof(UserPaymentMethod))]
        [XmlElement("InternetServiceLevel", typeof(InternetServiceLevel))]
        public List<object> Collection { get; set; }
    }
}