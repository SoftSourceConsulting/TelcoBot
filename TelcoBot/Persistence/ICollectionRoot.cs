/*  
 *  ICollectionRoot.cs
 *
 *  SoftSource Consulting, Inc.
 */

using System.Collections.Generic;

namespace TelcoBot.Persistence
{
    /// <summary>
    /// Contract for a class that can provides a top-level element for XML trees, for use in for XML serialization/deserialization.
    /// The implementing class will need these class-level attributes: [Serializable] and [XmlRoot("Items")]
    /// </summary>
    public interface ICollectionRoot
    {
        /// <summary>
        /// The implementing class will need fully specified XmlElement attributes on this property for each type 
        /// that it will be used to deserialize, e.g. [XmlElement("User", typeof(User))]
        /// </summary>
        List<object> Collection { get; set; }
    }

}