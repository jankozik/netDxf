#region netDxf, Copyright(C) 2013 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2013 Daniel Carvajal (haplokuon@gmail.com)
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
using System.Collections.Generic;
using netDxf.Tables;

namespace netDxf.Collections
{
    /// <summary>
    /// Represents a collection of text styles.
    /// </summary>
    public sealed class TextStyles :
        TableObjects<TextStyle>
    {

        #region constructor

        internal TextStyles(DxfDocument document)
            : base(document)
        {
        }

        internal TextStyles(DxfDocument document, Dictionary<string, TextStyle> list, Dictionary<string, List<DxfObject>> references)
            : base(document, list, references)
        {
        }

        #endregion

        #region override methods

        /// <summary>
        /// Adds a text style to the list.
        /// </summary>
        /// <param name="style"><see cref="TextStyle">TextStyle</see> to add to the list.</param>
        /// <returns>
        /// If a text style already exists with the same name as the instance that is being added the method returns the existing text style,
        /// if not it will return the new text style.
        /// </returns>
        public override TextStyle Add(TextStyle style)
        {
            TextStyle add;
            if (this.list.TryGetValue(style.Name, out add))
                return add;

            this.document.NumHandles = style.AsignHandle(this.document.NumHandles);
            this.list.Add(style.Name, style);
            this.references.Add(style.Name, new List<DxfObject>());

            return style;
        }

        /// <summary>
        /// Removes a text style.
        /// </summary>
        /// <param name="name"><see cref="TextStyle">TextStyle</see> name to remove from the document.</param>
        /// <returns>True is the text style has been successfully removed, or false otherwise.</returns>
        /// <remarks>Reserved text styles or any other referenced by objects cannot be removed.</remarks>
        public override bool Remove(string name)
        {
            TextStyle style = this[name];

            if (style == null)
                return false;

            if (style.IsReserved)
                return false;

            if (this.references[style.Name].Count != 0)
                return false;

            this.references.Remove(style.Name);
            return this.list.Remove(style.Name);

        }

        #endregion

    }
}