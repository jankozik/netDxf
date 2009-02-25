﻿#region netDxf, Copyright(C) 2009 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2009 Daniel Carvajal (haplokuon@gmail.com)
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

namespace netDxf.Entities
{
    /// <summary>
    /// Represents a polyline.
    /// </summary>
    public class Polyline :
        IPolyline
    {
        #region private fields

        private const string DXF_NAME = DxfEntityCode.Polyline;
        private const EntityType TYPE = EntityType.Polyline;
        private List<PolylineVertex> vertexes;
        private bool isClosed;
        private PolylineTypeFlags flags;
        private Layer layer;
        private AciColor color;
        private LineType lineType;
        private Vector3 normal;
        private float elevation;
        private float thickness;
        private readonly List<XData> xData;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the Polyline class.
        /// </summary>
        /// <param name="vertexes">Polyline vertex list.</param>
        /// <param name="isClosed">Sets if the polyline is closed</param>
        public Polyline(List<PolylineVertex> vertexes, bool isClosed)
        {
            this.vertexes = vertexes;
            this.isClosed = isClosed;
            this.layer = Layer.Default;
            this.color = AciColor.Bylayer;
            this.lineType = LineType.ByLayer;
            this.normal = Vector3.UnitZ;
            this.elevation = 0.0f;
            this.flags = isClosed ? PolylineTypeFlags.ClosedPolylineOrClosedPolygonMeshInM : PolylineTypeFlags.OpenPolyline;
            this.xData = new List<XData>();
        }

        /// <summary>
        /// Initializes a new instance of the Polyline class.
        /// </summary>
        /// <param name="vertexes">Polyline vertex list.</param>
        public Polyline(List<PolylineVertex> vertexes)
        {
            this.vertexes = vertexes;
            this.isClosed = false;
            this.layer = Layer.Default;
            this.color = AciColor.Bylayer;
            this.lineType = LineType.ByLayer;
            this.normal = Vector3.UnitZ;
            this.elevation = 0.0f;
            this.flags = PolylineTypeFlags.OpenPolyline;
            this.xData = new List<XData>();
        }

        /// <summary>
        /// Initializes a new instance of the Polyline class.
        /// </summary>
        public Polyline()
        {
            this.vertexes = new List<PolylineVertex>();
            this.isClosed = false;
            this.layer = Layer.Default;
            this.color = AciColor.Bylayer;
            this.lineType = LineType.ByLayer;
            this.normal = Vector3.UnitZ;
            this.elevation = 0.0f;
            this.flags = PolylineTypeFlags.OpenPolyline;
            this.xData = new List<XData>();
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the polyline vertexes.
        /// </summary>
        public List<PolylineVertex> Vertexes
        {
            get { return this.vertexes; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.vertexes = value;
            }
        }

        /// <summary>
        /// Gets or sets if the polyline is closed.
        /// </summary>
        public virtual bool IsClosed
        {
            get { return this.isClosed; }
            set
            {
                this.flags |= value ? PolylineTypeFlags.ClosedPolylineOrClosedPolygonMeshInM : PolylineTypeFlags.OpenPolyline;
                this.isClosed = value;
            }
        }

        public Vector3 Normal
        {
            get { return this.normal; }
            set
            {
                if (Vector3.Zero == value)
                    throw new ArgumentNullException("value", "The normal can not be the zero vector");
                value.Normalize();
                this.normal = value;
            }
        }

        public float Thickness
        {
            get { return this.thickness; }
            set { this.thickness = value; }
        }

        public float Elevation
        {
            get { return this.elevation; }
            set { this.elevation = value; }
        }

        #endregion

        #region IPolyline Members

        public PolylineTypeFlags Flags
        {
            get { return this.flags; }
        }

        #endregion

        #region IEntityObject Members

        /// <summary>
        /// Gets the dxf code that represents the entity.
        /// </summary>
        public string DxfName
        {
            get { return DXF_NAME; }
        }

        /// <summary>
        /// Gets the entity type.
        /// </summary>
        public EntityType Type
        {
            get { return TYPE; }
        }

        /// <summary>
        /// Gets or sets the entity color.
        /// </summary>
        public AciColor Color
        {
            get { return this.color; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.color = value;
            }
        }

        /// <summary>
        /// Gets or sets the entity layer.
        /// </summary>
        public Layer Layer
        {
            get { return this.layer; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.layer = value;
            }
        }

        /// <summary>
        /// Gets or sets the entity line type.
        /// </summary>
        public LineType LineType
        {
            get { return this.lineType; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.lineType = value;
            }
        }

        /// <summary>
        /// Gets or sets the entity extended data.
        /// </summary>
        public List<XData> XData
        {
            get { return this.xData; }
        }

        #endregion

        #region public methods

        public void SetConstantWidth(float width)
        {
            foreach (PolylineVertex v in this.vertexes)
            {
                v.BeginThickness = width;
                v.EndThickness = width;
            }
        }

        /// <summary>
        /// Obtains a list of vertexes that represent the polyline approximating the curve segments as necessary.
        /// </summary>
        /// <param name="bulgePrecision">Curve segments precision (a value of zero means that no approximation will be made).</param>
        /// <param name="weldThreshold">Tolerance to consider if two new generated vertexes are equal.</param>
        /// <param name="bulgeThreshold">Minimun distance from which approximate curved segments of the polyline.</param>
        /// <returns>The vertexes are expresed in world coordinate system.</returns>
        public List<Vector3> PoligonalVertexes(byte bulgePrecision, float weldThreshold, float bulgeThreshold)
        {
            List<Vector3> wcsVertexes = new List<Vector3>();
            Matrix3 rotation = MathHelper.ArbitraryAxis(this.normal);
            int index = 0;

            foreach (PolylineVertex vertex in this.Vertexes)
            {
                float bulge = vertex.Bulge;
                Vector2 p1;
                Vector2 p2;

                if (index == this.Vertexes.Count - 1)
                {
                    p1 = new Vector2(vertex.Location.X, vertex.Location.Y);
                    p2 = new Vector2(this.vertexes[0].Location.X, this.vertexes[0].Location.Y);
                }
                else
                {
                    p1 = new Vector2(vertex.Location.X, vertex.Location.Y);
                    p2 = new Vector2(this.vertexes[index + 1].Location.X, this.vertexes[index + 1].Location.Y);
                }

                if (!p1.Equals(p2, weldThreshold))
                {
                    if (bulge == 0 || bulgePrecision == 0)
                    {
                        wcsVertexes.Add(rotation*new Vector3(p1.X, p1.Y, this.elevation));
                    }
                    else
                    {
                        float c = Vector2.Distance(p1, p2);
                        if (c >= bulgeThreshold)
                        {
                            float s = (c/2)*Math.Abs(bulge);
                            float r = ((c/2)*(c/2) + s*s)/(2*s);
                            float theta = (float) (4*Math.Atan(Math.Abs(bulge)));
                            float gamma = (float) ((Math.PI - theta)/2);
                            float phi;

                            if (bulge > 0)
                            {
                                phi = Vector2.AngleBetween(Vector2.UnitX, p2 - p1) + gamma;
                            }
                            else
                            {
                                phi = Vector2.AngleBetween(Vector2.UnitX, p2 - p1) - gamma;
                            }

                            Vector2 center = new Vector2((float) (p1.X + r*Math.Cos(phi)), (float) (p1.Y + r*Math.Sin(phi)));
                            Vector2 a1 = p1 - center;
                            float angle = 4*((float) (Math.Atan(bulge)))/(bulgePrecision + 1);

                            wcsVertexes.Add(rotation*new Vector3(p1.X, p1.Y, this.elevation));
                            for (int i = 1; i <= bulgePrecision; i++)
                            {
                                Vector2 curvePoint = new Vector2();
                                Vector2 prevCurvePoint = new Vector2(this.vertexes[this.vertexes.Count - 1].Location.X, this.vertexes[this.vertexes.Count - 1].Location.Y);
                                curvePoint.X = center.X + (float) (Math.Cos(i*angle)*a1.X - Math.Sin(i*angle)*a1.Y);
                                curvePoint.Y = center.Y + (float) (Math.Sin(i*angle)*a1.X + Math.Cos(i*angle)*a1.Y);

                                if (!curvePoint.Equals(prevCurvePoint, weldThreshold) &&
                                    !curvePoint.Equals(p2, weldThreshold))
                                {
                                    wcsVertexes.Add(rotation*new Vector3(curvePoint.X, curvePoint.Y, this.elevation));
                                }
                            }
                        }
                        else
                        {
                            wcsVertexes.Add(rotation*new Vector3(p1.X, p1.Y, this.elevation));
                        }
                    }
                }
                index++;
            }

            return wcsVertexes;
        }

        #endregion

        #region overrides

        public override string ToString()
        {
            return TYPE.ToString();
        }

        #endregion
    }
}