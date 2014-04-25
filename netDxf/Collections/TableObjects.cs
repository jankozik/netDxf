#region netDxf, Copyright(C) 2014 Daniel Carvajal, Licensed under LGPL.

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
using netDxf.Tables;

namespace netDxf.Collections
{

    /// <summary>
    /// Represents a list of table objects
    /// </summary>
    /// <typeparam name="T"><see cref="TableObject">TableObject</see>.</typeparam>
    public abstract class TableObjects<T> :
        IEnumerable<T> where T : TableObject
    {

        #region private fields

        protected readonly DxfDocument document;
        protected readonly Dictionary<string, T> list;
        protected readonly Dictionary<string, List<DxfObject>> references;

        #endregion

        #region constructor

        internal TableObjects(DxfDocument document)
        {
            this.document = document;
            this.list = new Dictionary<string, T>(StringComparer.InvariantCultureIgnoreCase);
            this.references = new Dictionary<string, List<DxfObject>>(StringComparer.InvariantCultureIgnoreCase);
        }

        internal TableObjects(DxfDocument document, Dictionary<string, T> list, Dictionary<string, List<DxfObject>> references)
        {
            this.document = document;
            this.list = list;
            this.references = references;
        }

        #endregion

        #region public properties
        
        /// <summary>
        /// Gets a table object from the list by name.
        /// </summary>
        /// <param name="name">Table object name.</param>
        /// <returns>The table object with the specified name.</returns>
        /// <remarks>Table object names are case insensitive.</remarks>
        public T this[string name]
        {
            get
            {
                T tableObject;
                return this.list.TryGetValue(name, out tableObject) ? tableObject : null;
            }
        }

        /// <summary>
        /// Gets the table object list.
        /// </summary>
        public ICollection<T> Values
        {
            get { return this.list.Values; }
        }

        /// <summary>
        /// Gets the ObjectTable names.
        /// </summary>
        public ICollection<string> Names
        {
            get { return this.list.Keys; }
        }

        /// <summary>
        /// Gets the number of table objects.
        /// </summary>
        public int Count
        {
            get { return this.list.Count; }
        }

        #endregion

        #region internal properties

        /// <summary>
        /// Gets the <see cref="DxfObject">dxf objects</see> referenced by a T.
        /// </summary>
        internal Dictionary<string, List<DxfObject>> References
        {
            get { return this.references; }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Gets the <see cref="DxfObject">dxf objects</see> referenced by a T.
        /// </summary>
        /// <returns>The list of DxfObjects that reference the specified table object.</returns>
        /// <remarks>If there is no table object with the specified name in the list the method will return null.</remarks>
        public List<DxfObject> GetReferences(string name)
        {
            if (!this.Contains(name))
                return null;
            List<DxfObject> objects = new List<DxfObject>();
            objects.AddRange(this.references[name]);
            return objects;
        }

        /// <summary>
        /// Gets the <see cref="DxfObject">dxf objects</see> referenced by a T.
        /// </summary>
        /// <returns>The list of DxfObjects that reference the specified table object.</returns>
        /// <remarks>If there is no specified table object in the list the method will return null.</remarks>
        public List<DxfObject> GetReferences(T tableObject)
        {
            if (!this.Contains(tableObject.Name))
                return null;
            List<DxfObject> objects = new List<DxfObject>();
            objects.AddRange(this.references[tableObject.Name]);
            return objects;
        }

        /// <summary>
        /// Checks if a table object already exists in the list. 
        /// </summary>
        /// <param name="name">Table objec name.</param>
        /// <returns>True is a table object exists with the specified name, false otherwise.</returns>
        public bool Contains(string name)
        {
            return this.list.ContainsKey(name);
        }

        /// <summary>
        /// Checks if a table object already exists in the list. 
        /// </summary>
        /// <param name="tableObject">Table object.</param>
        /// <returns>True is a table object exists, false otherwise.</returns>
        public bool Contains(T tableObject)
        {
            return this.list.ContainsValue(tableObject);
        }

        /// <summary>
        /// Gets the table object associated with the specified name.
        /// </summary>
        /// <param name="name"> The name of the table object to get.</param>
        /// <param name="tableObject">When this method returns, contains the table object associated with the specified name, if the key is found;
        /// otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns>True if the table contains an element with the specified name; otherwise, false.</returns>
        public bool TryGetValue(string name, out T tableObject)
        {
            return this.list.TryGetValue(name, out tableObject);
        }

        /// <summary>
        /// Adds a table object to the list.
        /// </summary>
        /// <param name="tableObject"><see cref="TableObject">Table object</see> to add to the list.</param>
        /// <returns>
        /// If a table object already exists with the same name as the instance that is being added the method returns the existing table object,
        /// if not it will return the new table object.
        /// </returns>
        public abstract T Add(T tableObject);

        /// <summary>
        /// Removes a table object.
        /// </summary>
        /// <param name="name">Table object name to remove from the document.</param>
        /// <returns>True is the table object has been successfully removed, or false otherwise.</returns>
        /// <remarks>Reserved table objects or any other referenced by objects cannot be removed.</remarks>
        public abstract bool Remove(string name);

        /// <summary>
        /// Removes a table object.
        /// </summary>
        /// <param name="tableObject"><see cref="TableObject">Table object</see> to remove from the document.</param>
        /// <returns>True is the table object has been successfully removed, or false otherwise.</returns>
        /// <remarks>Reserved table objects or any other referenced by objects cannot be removed.</remarks>
        public abstract bool Remove(T tableObject);

        /// <summary>
        /// Removes all table objects that are not reserved and have no references.
        /// </summary>
        public void Clear()
        {
            string[] names = new string[this.Count];
            this.list.Keys.CopyTo(names, 0);
            foreach (string o in names)
            {
                this.Remove(o);
            }
        }

	    #endregion

        #region implements IEnumerator<T>

        /// <summary>
        /// Returns an enumerator that iterates through the table object collection.
        /// </summary>
        /// <returns>An enumerator for the table object collection.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return this.list.Values.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the table object collection.
        /// </summary>
        /// <returns>An enumerator for the table object collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.list.Values.GetEnumerator();
        }

        #endregion

    }
}
