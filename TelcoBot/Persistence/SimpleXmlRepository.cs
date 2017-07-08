// 
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using TelcoBot.Model;

namespace TelcoBot.Persistence
{
    public class SimpleXmlRepository<TItem, TContainer> : ISimpleRepository<TItem>
        where TItem : IIdentified
        where TContainer : ICollectionRoot
    {
        private readonly string _defaultDataPath;
        private readonly string _dataFileRootName;
        private readonly Lazy<List<TItem>> _items;

        public SimpleXmlRepository(string defaultDataPath, string dataFileRootName)
        {
            _defaultDataPath = defaultDataPath;
            _dataFileRootName = dataFileRootName;
            _items = new Lazy<List<TItem>>(LoadData);
        }

        #region ISimpleRepository

        public int Count => _items.Value.Count;

        public IEnumerator<TItem> GetEnumerator() => _items.Value.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public TItem FindById(int id) => _items.Value.FirstOrDefault(b => b.Id == id);

        public void Save(TItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            // find the user with the same Id in the main collection
            int index = _items.Value.FindIndex(b => b.Id == item.Id);
            if (index == -1)
            {
                // if it's not in there, add it.
                _items.Value.Add(item);
            }
            else
            {
                // replace it with the incoming version.
                _items.Value.RemoveAt(index);
                _items.Value.Insert(index, item);
            }

            // serialize it to xml
            string xml = Serialize(_items.Value);

            // write it to disk
            File.WriteAllText(GetDataFilePath(), xml);
        }

        #endregion ISimpleRepository

        private List<TItem> LoadData()
        {
            string dataFilePath = GetDataFilePath();
            string xml = File.ReadAllText(dataFilePath);

            List<TItem> items = Deserialize(xml);

            return items;
        }

        private string GetDataFilePath()
        {
            string dataFilePath = _defaultDataPath + _dataFileRootName + ".xml";

            if (!File.Exists(dataFilePath))
                throw new InvalidOperationException("The data file could not be found.");

            return dataFilePath;
        }

        private List<TItem> Deserialize(string xml)
        {
            TContainer result;
            XmlSerializer serializer = new XmlSerializer(typeof(Items));
            using (StringReader reader = new StringReader(xml))
            {
                result = (TContainer)serializer.Deserialize(reader);
            }

            return result.Collection.Cast<TItem>().ToList();
        }

        private string Serialize(IEnumerable<TItem> items)
        {
            // put the collection into an items object
            Items container = new Items() { Collection = items.Cast<object>().ToList() };

            // serialize it to xml
            XmlSerializer serializer = new XmlSerializer(typeof(Items));
            StringWriter writer = new StringWriter();
            serializer.Serialize(writer, container);

            return writer.ToString();
        }
    }
}