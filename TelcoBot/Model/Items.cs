﻿// 
// Copyright (c) SoftSource Consulting, Inc. All rights reserved.
// Licensed under the MIT license.
// 
// https://github.com/SoftSourceConsulting/TelcoBot
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

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