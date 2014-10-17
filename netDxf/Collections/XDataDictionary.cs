﻿#region netDxf, Copyright(C) 2014 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2014 Daniel Carvajal (haplokuon@gmail.com)
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 

#endregion

using System;
using System.Collections;
using System.Collections.Generic;

namespace netDxf.Collections
{
    /// <summary>
    /// Represents a dictionary of <see cref="XData">XData</see>.
    /// </summary>
    public class XDataDictionary :
        IDictionary<string, XData>
    {

        #region private fields

        private readonly Dictionary<string, XData> innerDictionary;

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of <c>XDataDictionary</c>.
        /// </summary>
        public XDataDictionary()
        {
            this.innerDictionary = new Dictionary<string, XData>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Initializes a new instance of <c>XDataDictionary</c> and has the specified items.
        /// </summary>
        /// <param name="items">The list of <see cref="XData">extended data</see> items initially stored.</param>
        public XDataDictionary(ICollection<XData> items)
        {
            this.innerDictionary = new Dictionary<string, XData>(items.Count, StringComparer.OrdinalIgnoreCase);
            this.AddRange(items);
        }

        /// <summary>
        /// Initializes a new instance of <c>XDataDictionary</c> and has the specified initial capacity.
        /// </summary>
        /// <param name="capacity">The number of items the collection can initially store.</param>
        public XDataDictionary(int capacity)
        {
            this.innerDictionary = new Dictionary<string, XData>(capacity, StringComparer.OrdinalIgnoreCase);
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the <see cref="XData">extended data</see> with the specified application registry name.
        /// </summary>
        /// <param name="appId">The application registry name to get or set.</param>
        /// <returns>The <see cref="XData">extended data</see> of the application registry.</returns>
        public XData this[string appId]
        {
            get { return this.innerDictionary[appId]; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                if (!value.ApplicationRegistry.Name.Equals(appId, StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException(String.Format("The extended data application registry name {0} must be equal to the specified appId {1}.", value.ApplicationRegistry.Name, appId));

                this.innerDictionary[appId] = value;
            }
        }

        /// <summary>
        /// Gets an ICollection containing the application registry names of the current dictionary.
        /// </summary>
        public ICollection<string> AppIds
        {
            get { return this.innerDictionary.Keys; }
        }

        /// <summary>
        /// Gets an ICollection containing the <see cref="XData">extended data</see> list of the current dictionary.
        /// </summary>
        public ICollection<XData> Values
        {
            get { return this.innerDictionary.Values; }
        }

        /// <summary>
        /// Gets the number of <see cref="XData">extended data</see> contained in the current dictionary.
        /// </summary>
        public int Count
        {
            get { return this.innerDictionary.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the actual dictionary is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Adds an <see cref="XData">extended data</see> to the current dictionary.
        /// </summary>
        /// <param name="item">The <see cref="XData">extended data</see> to add.</param>
        /// <returns>True is the <see cref="XData">extended data</see> has been correctly added to the dictionary; otherwise, false.</returns>
        /// <remarks>
        /// If the current dictionary already contains an appId equals to the extended data that is being added
        /// the <see cref="XDataRecord">XDataRecords</see> will be added to the existing one.
        /// </remarks>
        public bool Add(XData item)
        {
            if (item == null) throw new ArgumentNullException("item");
            XData xdata;
            if (this.innerDictionary.TryGetValue(item.ApplicationRegistry.Name, out xdata))
                xdata.XDataRecord.AddRange(item.XDataRecord);
            else
                this.innerDictionary.Add(item.ApplicationRegistry.Name, item);
            return true;
        }

        /// <summary>
        /// Adds a list of <see cref="XData">extended data</see> to the current dictionary.
        /// </summary>
        /// <param name="items">The list of <see cref="XData">extended data</see> to add.</param>
        public void AddRange(ICollection<XData> items)
        {
            foreach (XData data in items)
            {
                this.Add(data);
            }
        }

        /// <summary>
        /// Removes an <see cref="XData">extended data</see> with the specified application registry name from the current dictionary.
        /// </summary>
        /// <param name="appId">The application registry name of the <see cref="XData">extended data</see> to remove.</param>
        /// <returns>True if the <see cref="XData">extended data</see> is successfully removed; otherwise, false.</returns>
        public bool Remove(string appId)
        {
            if (!this.innerDictionary.ContainsKey(appId)) return false;
            this.innerDictionary.Remove(appId);
            return true;
        }

        /// <summary>
        /// Removes all <see cref="XData">extended data</see> from the current dictionary.
        /// </summary>
        public void Clear()
        {
            this.innerDictionary.Clear();
        }

        /// <summary>
        /// Determines whether current dictionary contains an <see cref="XData">extended data</see> with the specified application registry name.
        /// </summary>
        /// <param name="appId">The application registry name to locate in the current dictionary.</param>
        /// <returns>True if the current dictionary contains an <see cref="XData">extended data</see> with the application registry name; otherwise, false.</returns>
        public bool ContainsAppId(string appId)
        {
            return this.innerDictionary.ContainsKey(appId);
        }

        /// <summary>
        /// Determines whether current dictionary contains a specified <see cref="XData">extended data</see>.
        /// </summary>
        /// <param name="value">The <see cref="XData">extended data</see> to locate in the current dictionary.</param>
        /// <returns>True if the current dictionary contains the <see cref="XData">extended data</see>; otherwise, false.</returns>
        public bool ContainsValue(XData value)
        {
            return this.innerDictionary.ContainsValue(value);
        }

        /// <summary>
        /// Gets the <see cref="XData">extended data</see> associated with the specified application registry name.
        /// </summary>
        /// <param name="appId">The application registry name whose value to get.</param>
        /// <param name="value">When this method returns, the <see cref="XData">extended data</see> associated with the specified application registry name,
        /// if the application registry name is found; otherwise, null. This parameter is passed uninitialized.</param>
        /// <returns>True if the current dictionary contains an <see cref="XData">extended data</see> with the specified application registry name; otherwise, false.</returns>
        public bool TryGetValue(string appId, out XData value)
        {
            return this.innerDictionary.TryGetValue(appId, out value);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the dictionary.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the dictionary.</returns>
        public IEnumerator<KeyValuePair<string, XData>> GetEnumerator()
        {
            return this.innerDictionary.GetEnumerator();
        }

        #endregion

        #region private properties

        ICollection<string> IDictionary<string, XData>.Keys
        {
            get { return this.innerDictionary.Keys; }
        }

        #endregion

        #region private methods

        bool IDictionary<string, XData>.ContainsKey(string tag)
        {
            return this.innerDictionary.ContainsKey(tag);
        }

        void IDictionary<string, XData>.Add(string key, XData value)
        {
            this.Add(value);
        }

        void ICollection<KeyValuePair<string, XData>>.Add(KeyValuePair<string, XData> item)
        {
            this.Add(item.Value);
        }

        bool ICollection<KeyValuePair<string, XData>>.Remove(KeyValuePair<string, XData> item)
        {
            if (!ReferenceEquals(item.Value, this.innerDictionary[item.Key])) return false;
            return this.Remove(item.Key);
        }

        bool ICollection<KeyValuePair<string, XData>>.Contains(KeyValuePair<string, XData> item)
        {
            return ((IDictionary<string, XData>)this.innerDictionary).Contains(item);
        }

        void ICollection<KeyValuePair<string, XData>>.CopyTo(KeyValuePair<string, XData>[] array, int arrayIndex)
        {
            ((IDictionary<string, XData>)this.innerDictionary).CopyTo(array, arrayIndex);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}
