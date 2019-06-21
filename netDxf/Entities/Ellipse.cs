#region netDxf library, Copyright (C) 2009-2019 Daniel Carvajal (haplokuon@gmail.com)

//                        netDxf library
// Copyright (C) 2009-2019 Daniel Carvajal (haplokuon@gmail.com)
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
using System.Diagnostics;
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents an ellipse <see cref="EntityObject">entity</see>.
    /// </summary>
    public class Ellipse :
        EntityObject
    {
        #region private classes

        public class CalculatePoints
        {
            public static void main()
            {
                // TODO Auto-generated method stub

                /*
                 * 
                dp(t) = sqrt( (r1*sin(t))^2 + (r2*cos(t))^2)
                circ = sum(dp(t), t=0..2*Pi step 0.0001)
            
                n = 20
            
                nextPoint = 0
                run = 0.0
                for t=0..2*Pi step 0.0001
                    if n*run/circ >= nextPoint then
                        set point (r1*cos(t), r2*sin(t))
                        nextPoint = nextPoint + 1
                    next
                    run = run + dp(t)
                next
             */


                double r1 = 20.0;
                double r2 = 10.0;

                double theta = 0.0;
                double twoPi = Math.PI*2.0;
                double deltaTheta = 0.0001;
                double numIntegrals = Math.Round(twoPi/deltaTheta);
                double circ=0.0;
                double dpt=0.0;

                /* integrate over the elipse to get the circumference */
                for( int i=0; i < numIntegrals; i++ ) {
                    theta += i*deltaTheta;
                    dpt = computeDpt( r1, r2, theta);
                    circ += dpt;
                }

                int n=20;
                int nextPoint = 0;
                double run = 0.0;
                theta = 0.0;

                for( int i=0; i < numIntegrals; i++ ) {
                    theta += deltaTheta;
                    double subIntegral = n*run/circ;
                    if( (int) subIntegral >= nextPoint ) {
                        double x = r1 * Math.Cos(theta);
                        double y = r2 * Math.Sin(theta);
                        nextPoint++;
                    }
                    run += computeDpt(r1, r2, theta);
                }
            }

            static double computeDpt( double r1, double r2, double theta )
            {
                double dp=0.0;

                double dpt_sin = Math.Pow(r1*Math.Sin(theta), 2.0);
                double dpt_cos = Math.Pow( r2*Math.Cos(theta), 2.0);
                dp = Math.Sqrt(dpt_sin + dpt_cos);

                return dp;
            }
        }

        private static class ConicThroughFivePoints
        {
            private static double[] CoefficientsLine(Vector2 p1, Vector2 p2)
            {
                // line: A*x + B*y + C = 0
                double[] coeficients = new double[3];
                coeficients[0] = p1.Y - p2.Y; //A
                coeficients[1] = p2.X - p1.X; //B
                coeficients[2] = p1.X * p2.Y - p2.X * p1.Y; //C
                return coeficients;
            }

            private static double[] CoefficientsProductLines(double[] l1, double[] l2)
            {
                // lines product: A*x2 + B*xy + C*y2 + D*x + E*y + F = 0
                double[] coeficients = new double[6];
                coeficients[0] = l1[0] * l2[0]; //A
                coeficients[1] = l1[0] * l2[1] + l1[1] * l2[0]; //B
                coeficients[2] = l1[1] * l2[1]; //C
                coeficients[3] = l1[0] * l2[2] + l1[2] * l2[0]; //D
                coeficients[4] = l1[1] * l2[2] + l1[2] * l2[1]; //E
                coeficients[5] = l1[2] * l2[2]; //F
                return coeficients;
            }

            private static double Lambda(double[] alpha_beta, double[] gamma_delta, Vector2 p)
            {
                // conic families: alpha_beta + lambda*gamma_delta = 0
                double a1 = alpha_beta[0] * p.X * p.X;
                double b1 = alpha_beta[1] * p.X * p.Y;
                double c1 = alpha_beta[2] * p.Y * p.Y;
                double d1 = alpha_beta[3] * p.X;
                double e1 = alpha_beta[4] * p.Y;
                double f1 = alpha_beta[5];

                double a2 = gamma_delta[0] * p.X * p.X;
                double b2 = gamma_delta[1] * p.X * p.Y;
                double c2 = gamma_delta[2] * p.Y * p.Y;
                double d2 = gamma_delta[3] * p.X;
                double e2 = gamma_delta[4] * p.Y;
                double f2 = gamma_delta[5];

                double sum1 = a1 + b1 + c1 + d1 + e1 + f1;
                double sum2 = a2 + b2 + c2 + d2 + e2 + f2;

                if (MathHelper.IsZero(sum2)) return double.NaN;

                return -sum1 / sum2;
            }

            private static double[] ConicCoefficients(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4, Vector2 point5)
            {
                double[] line_alpha = CoefficientsLine(point1, point2);
                double[] line_beta = CoefficientsLine(point3, point4);
                double[] line_gamma = CoefficientsLine(point1, point3);
                double[] line_delta = CoefficientsLine(point2, point4);

                double[] alpha_beta = CoefficientsProductLines(line_alpha, line_beta);
                double[] gamma_delta = CoefficientsProductLines(line_gamma, line_delta);

                double lambda = Lambda(alpha_beta, gamma_delta, point5);
                if (double.IsNaN(lambda)) return null; // conic coefficients cannot be found, duplicate points

                double[] coeficients = new double[6];
                coeficients[0] = alpha_beta[0] + lambda * gamma_delta[0];          
                coeficients[1] = alpha_beta[1] + lambda * gamma_delta[1];
                coeficients[2] = alpha_beta[2] + lambda * gamma_delta[2];
                coeficients[3] = alpha_beta[3] + lambda * gamma_delta[3];
                coeficients[4] = alpha_beta[4] + lambda * gamma_delta[4];
                coeficients[5] = alpha_beta[5] + lambda * gamma_delta[5];

                return coeficients;
            }

            public static bool EllipseProperties(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4, Vector2 point5, out Vector2 center, out double semiMajorAxis, out double semiMinorAxis, out double rotation)
            {         
                center = Vector2.NaN;
                semiMajorAxis = double.NaN;
                semiMinorAxis = double.NaN;
                rotation = double.NaN;

                double[] coeficients = ConicCoefficients(point1, point2, point3, point4, point5);
                if(coeficients == null) return false;

                double a = coeficients[0];
                double b = coeficients[1];
                double c = coeficients[2];
                double d = coeficients[3];
                double e = coeficients[4];
                double f = coeficients[5];

                double q = b * b - 4 * a * c;
                           
                if (q >= 0) return false; // not an ellipse

                center.X = (2 * c * d - b * e) / q;
                center.Y = (2 * a * e - b * d) / q;

                double m = Math.Sqrt((a - c) * (a - c) + b * b);
                double n = 2 * (a * e * e + c * d * d - b * d * e + q * f);
                double axis1 = -Math.Sqrt(n * (a + c + m)) / q;
                double axis2 = -Math.Sqrt(n * (a + c - m)) / q;

                if (MathHelper.IsZero(b))
                {
                    // ellipse parallel to world axis
                    if (MathHelper.IsEqual(a, c))
                        rotation = 0.0;
                    else
                        rotation = a < c ? 0.0 : MathHelper.HalfPI;
                }
                else
                {
                    rotation = Math.Atan((c - a - Math.Sqrt((a - c) * (a - c) + b * b)) / b);
                }

                if (axis1 >= axis2)
                {
                    semiMajorAxis = axis1;
                    semiMinorAxis = axis2;
                }
                else
                {
                    semiMajorAxis = axis2;
                    semiMinorAxis = axis1;
                    rotation += MathHelper.HalfPI;
                }
                return true;
            }
        }

        #endregion

        #region private fields

        private Vector3 center;
        private double majorAxis;
        private double minorAxis;
        private double rotation;
        private double startAngle;
        private double endAngle;
        private double thickness;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Ellipse</c> class.
        /// </summary>
        public Ellipse()
            : this(Vector3.Zero, 1.0, 0.5)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Ellipse</c> class.
        /// </summary>
        /// <param name="center">Ellipse <see cref="Vector2">center</see> in object coordinates.</param>
        /// <param name="majorAxis">Ellipse major axis.</param>
        /// <param name="minorAxis">Ellipse minor axis.</param>
        /// <remarks>The center Z coordinate represents the elevation of the arc along the normal.</remarks>
        public Ellipse(Vector2 center, double majorAxis, double minorAxis)
            : this(new Vector3(center.X, center.Y, 0.0), majorAxis, minorAxis)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Ellipse</c> class.
        /// </summary>
        /// <param name="center">Ellipse <see cref="Vector3">center</see> in object coordinates.</param>
        /// <param name="majorAxis">Ellipse major axis.</param>
        /// <param name="minorAxis">Ellipse minor axis.</param>
        /// <remarks>The center Z coordinate represents the elevation of the arc along the normal.</remarks>
        public Ellipse(Vector3 center, double majorAxis, double minorAxis)
            : base(EntityType.Ellipse, DxfObjectCode.Ellipse)
        {
            this.center = center;
            if (majorAxis <= 0)
                throw new ArgumentOutOfRangeException(nameof(majorAxis), majorAxis, "The major axis value must be greater than zero.");
            this.majorAxis = majorAxis;
            if (minorAxis <= 0)
                throw new ArgumentOutOfRangeException(nameof(minorAxis), minorAxis, "The minor axis value must be greater than zero.");
            this.minorAxis = minorAxis;
            this.startAngle = 0.0;
            this.endAngle = 0.0;
            this.rotation = 0.0;
            this.thickness = 0.0;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the ellipse <see cref="Vector3">center</see>.
        /// </summary>
        /// <remarks>The center Z coordinate represents the elevation of the arc along the normal.</remarks>
        public Vector3 Center
        {
            get { return this.center; }
            set { this.center = value; }
        }

        /// <summary>
        /// Gets or sets the ellipse mayor axis.
        /// </summary>
        public double MajorAxis
        {
            get { return this.majorAxis; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The major axis value must be greater than zero.");
                this.majorAxis = value;
            }
        }

        /// <summary>
        /// Gets or sets the ellipse minor axis.
        /// </summary>
        public double MinorAxis
        {
            get { return this.minorAxis; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The minor axis value must be greater than zero.");
                this.minorAxis = value;
            }
        }

        /// <summary>
        /// Gets or sets the ellipse local rotation in degrees along its normal.
        /// </summary>
        public double Rotation
        {
            get { return this.rotation; }
            set { this.rotation = MathHelper.NormalizeAngle(value); }
        }

        /// <summary>
        /// Gets or sets the ellipse start angle in degrees.
        /// </summary>
        /// <remarks>To get a full ellipse set the start angle equal to the end angle.</remarks>
        public double StartAngle
        {
            get { return this.startAngle; }
            set { this.startAngle = MathHelper.NormalizeAngle(value); }
        }

        /// <summary>
        /// Gets or sets the ellipse end angle in degrees.
        /// </summary>
        /// <remarks>To get a full ellipse set the end angle equal to the start angle.</remarks>
        public double EndAngle
        {
            get { return this.endAngle; }
            set { this.endAngle = MathHelper.NormalizeAngle(value); }
        }

        /// <summary>
        /// Gets or sets the ellipse thickness.
        /// </summary>
        public double Thickness
        {
            get { return this.thickness; }
            set { this.thickness = value; }
        }

        /// <summary>
        /// Checks if the actual instance is a full ellipse.
        /// </summary>
        /// <remarks>An ellipse is considered full when its start and end angles are equal.</remarks>
        public bool IsFullEllipse
        {
            get { return MathHelper.IsEqual(this.startAngle, this.endAngle); }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Calculate the local point on the ellipse for a given angle relative to the center.
        /// </summary>
        /// <param name="angle">Angle in degrees.</param>
        /// <returns>A local point on the ellipse for the given angle relative to the center.</returns>
        public Vector2 PolarCoordinateRelativeToCenter(double angle)
        {
            double a = this.MajorAxis*0.5;
            double b = this.MinorAxis*0.5;
            double radians = angle*MathHelper.DegToRad;

            double a1 = a*Math.Sin(radians);
            double b1 = b*Math.Cos(radians);

            double radius = (a*b)/Math.Sqrt(b1*b1 + a1*a1);

            // convert the radius back to cartesian coordinates
            return new Vector2(radius*Math.Cos(radians), radius*Math.Sin(radians));
        }

        /// <summary>
        /// Converts the ellipse in a list of vertexes.
        /// </summary>
        /// <param name="precision">Number of vertexes generated.</param>
        /// <returns>A list vertexes that represents the ellipse expressed in object coordinate system.</returns>
        public List<Vector2> PolygonalVertexes(int precision)
        {
            List<Vector2> points = new List<Vector2>();
            double beta = this.rotation * MathHelper.DegToRad;
            double sinbeta = Math.Sin(beta);
            double cosbeta = Math.Cos(beta);
            double start;
            double end;
            double steps;

            if (this.IsFullEllipse)
            {
                start = 0;
                end = MathHelper.TwoPI;
                steps = precision;
            }
            else
            {
                Vector2 startPoint = this.PolarCoordinateRelativeToCenter(this.startAngle);
                Vector2 endPoint = this.PolarCoordinateRelativeToCenter(this.endAngle);
                double a = 1 / (0.5 * this.majorAxis);
                double b = 1 / (0.5 * this.minorAxis);
                start = Math.Atan2(startPoint.Y * b, startPoint.X * a);
                end = Math.Atan2(endPoint.Y * b, endPoint.X * a);

                if (end < start) end += MathHelper.TwoPI;
                steps = precision - 1;
            }
           
            double delta = (end - start) / steps;

            for (int i = 0; i < precision; i++)
            {
                double angle = start + delta*i;
                double sinalpha = Math.Sin(angle);
                double cosalpha = Math.Cos(angle);

                double pointX = 0.5 * (this.majorAxis * cosalpha * cosbeta - this.minorAxis * sinalpha * sinbeta);
                double pointY = 0.5 * (this.majorAxis * cosalpha * sinbeta + this.minorAxis * sinalpha * cosbeta);

                points.Add(new Vector2(pointX, pointY));
            }

            return points;
        }

        /// <summary>
        /// Converts the ellipse in a Polyline.
        /// </summary>
        /// <param name="precision">Number of vertexes generated.</param>
        /// <returns>A new instance of <see cref="LwPolyline">LightWeightPolyline</see> that represents the ellipse.</returns>
        public LwPolyline ToPolyline(int precision)
        {
            IEnumerable<Vector2> vertexes = this.PolygonalVertexes(precision);
            Vector3 ocsCenter = MathHelper.Transform(this.center, this.Normal, CoordinateSystem.World, CoordinateSystem.Object);
            LwPolyline poly = new LwPolyline
            {
                Layer = (Layer) this.Layer.Clone(),
                Linetype = (Linetype) this.Linetype.Clone(),
                Color = (AciColor) this.Color.Clone(),
                Lineweight = this.Lineweight,
                Transparency = (Transparency) this.Transparency.Clone(),
                LinetypeScale = this.LinetypeScale,
                Normal = this.Normal,
                Elevation = ocsCenter.Z,
                Thickness = this.Thickness,
                IsClosed = this.IsFullEllipse
            };

            foreach (Vector2 v in vertexes)
            {
                poly.Vertexes.Add(new LwPolylineVertex(v.X + ocsCenter.X, v.Y + ocsCenter.Y));
            }
            return poly;
        }

        #endregion

        #region overrides

        /// <summary>
        /// Moves, scales, and/or rotates the current entity given a 3x3 transformation matrix and a translation vector.
        /// </summary>
        /// <param name="transformation">Transformation matrix.</param>
        /// <param name="translation">Translation vector.</param>
        /// <remarks>Matrix3 adopts the convention of using column vectors to represent a transformation matrix.</remarks>
        public override void TransformBy(Matrix3 transformation, Vector3 translation)
        {
            // NOTE: this is a generic implementation of the ellipse transformation,
            // for non rotated ellipses and/or uniform scaling the code can be simplified

            // rectangle that circumscribe the ellipse
            double semiMajorAxis = this.MajorAxis * 0.5;
            double semiMinorAxis = this.MinorAxis * 0.5;

            Vector2 p1 = new Vector2(-semiMajorAxis, semiMinorAxis);
            Vector2 p2 = new Vector2(semiMajorAxis, semiMinorAxis);
            Vector2 p3 = new Vector2(-semiMajorAxis, -semiMinorAxis);
            Vector2 p4 = new Vector2(semiMajorAxis, -semiMinorAxis);
            List<Vector2> ocsPoints = MathHelper.Transform(new[] {p1, p2, p3, p4}, this.Rotation * MathHelper.DegToRad, CoordinateSystem.Object, CoordinateSystem.World);

            Vector3 p1prime = new Vector3(ocsPoints[0].X, ocsPoints[0].Y, 0.0);
            Vector3 p2prime = new Vector3(ocsPoints[1].X, ocsPoints[1].Y, 0.0);
            Vector3 p3prime = new Vector3(ocsPoints[2].X, ocsPoints[2].Y, 0.0);
            Vector3 p4prime = new Vector3(ocsPoints[3].X, ocsPoints[3].Y, 0.0);
            List<Vector3> wcsPoints = MathHelper.Transform(new[] {p1prime, p2prime, p3prime, p4prime}, this.Normal, CoordinateSystem.Object, CoordinateSystem.World);
            for (int i = 0; i < wcsPoints.Count; i++)
            {
                wcsPoints[i] += this.Center;

                wcsPoints[i] = transformation * wcsPoints[i];
                wcsPoints[i] += translation;
            }

            Vector3 newNormal = transformation * this.Normal;
            if (Vector3.Equals(Vector3.Zero, newNormal)) newNormal = this.Normal;

            List<Vector3> rectPoints = MathHelper.Transform(wcsPoints, newNormal, CoordinateSystem.World, CoordinateSystem.Object);
            
            // corners of the transformed rectangle that circumscribe the new ellipse        
            Vector2 pointA = new Vector2(rectPoints[0].X, rectPoints[0].Y);
            Vector2 pointB = new Vector2(rectPoints[1].X, rectPoints[1].Y);
            Vector2 pointC = new Vector2(rectPoints[2].X, rectPoints[2].Y);
            Vector2 pointD = new Vector2(rectPoints[3].X, rectPoints[3].Y);

            // the new ellipse is tangent at the mid points
            Vector2 pointM = Vector2.MidPoint(pointA, pointB);
            Vector2 pointN = Vector2.MidPoint(pointC, pointD);
            Vector2 pointH = Vector2.MidPoint(pointA, pointC);
            Vector2 pointK = Vector2.MidPoint(pointB, pointD);

            // we need to find a fifth point
            Vector2 origin = Vector2.MidPoint(pointH, pointK);
            Vector2 pointX = Vector2.MidPoint(pointH, origin); // a point along the OH segment

            // intersection line AC and line parallel to BC through pointX
            Vector2 pointY = MathHelper.FindIntersection(pointA, pointC - pointA, pointX, pointC - pointB);
            if (Vector2.IsNaN(pointY))
            {
                Debug.Assert(false, "The transformation cannot be applied.");
                return;
            }

            // find the fifth point in the ellipse
            Vector2 pointZ = MathHelper.FindIntersection(pointM, pointX - pointM, pointN, pointY - pointN);
            if(Vector2.IsNaN(pointZ))
            {
                Debug.Assert(false, "The transformation cannot be applied.");
                return;
            }
            
            Vector3 oldNormal = this.Normal;
            double oldRotation = this.Rotation;

            Vector2 newCenter;
            double newSemiMajorAxis;
            double newSemiMinorAxis;
            double newRotation;

            if (ConicThroughFivePoints.EllipseProperties(pointM, pointN, pointH, pointK, pointZ, out newCenter, out newSemiMajorAxis, out newSemiMinorAxis, out newRotation))
            {
                double axis1 = 2 * newSemiMajorAxis;
                axis1 = MathHelper.IsZero(axis1) ? MathHelper.Epsilon : axis1;
                double axis2 = 2 * newSemiMinorAxis;
                axis2 = MathHelper.IsZero(axis2) ? MathHelper.Epsilon : axis2;

                this.Center = transformation * this.Center + translation;
                this.MajorAxis = axis1;
                this.MinorAxis = axis2;
                this.Rotation = newRotation * MathHelper.RadToDeg;
                this.Normal = newNormal;
            }
            else
            {
                Debug.Assert(false, "The transformation cannot be applied.");
                return;
            }

            if (this.IsFullEllipse) return;

            //if not full ellipse calculate start and end angles
            Vector2 start = this.PolarCoordinateRelativeToCenter(this.StartAngle);
            Vector2 end = this.PolarCoordinateRelativeToCenter(this.EndAngle);

            if (!MathHelper.IsZero(this.Rotation))
            {
                double beta = oldRotation * MathHelper.DegToRad;
                double sinbeta = Math.Sin(beta);
                double cosbeta = Math.Cos(beta);

                start = new Vector2(start.X * cosbeta - start.Y * sinbeta, start.X * sinbeta + start.Y * cosbeta);
                end = new Vector2(end.X * cosbeta - end.Y * sinbeta, end.X * sinbeta + end.Y * cosbeta);
            }

            Vector3 pStart = new Vector3(start.X, start.Y, 0.0);
            Vector3 pEnd = new Vector3(end.X, end.Y, 0.0);

            List<Vector3> wcsAnglePoints = MathHelper.Transform(new[] {pStart, pEnd}, oldNormal, CoordinateSystem.Object, CoordinateSystem.World);
            for (int i = 0; i < wcsAnglePoints.Count; i++)
            {
                wcsPoints[i] += this.Center;

                wcsAnglePoints[i] = transformation * wcsAnglePoints[i];
                wcsPoints[i] += translation;

            }
            List<Vector3> ocsAnglePoints = MathHelper.Transform(wcsAnglePoints, newNormal, CoordinateSystem.World, CoordinateSystem.Object);

            Vector2 newStart = new Vector2(ocsAnglePoints[0].X, ocsAnglePoints[0].Y);
            Vector2 newEnd = new Vector2(ocsAnglePoints[1].X, ocsAnglePoints[1].Y);

            double invert = Math.Sign(transformation.M11 * transformation.M22 * transformation.M33) < 0 ? 180.0 : 0.0;
            this.StartAngle = invert + Vector2.Angle(newStart) * MathHelper.RadToDeg - this.Rotation;
            this.EndAngle = invert + Vector2.Angle(newEnd) * MathHelper.RadToDeg - this.Rotation;
        }

        /// <summary>
        /// Creates a new Ellipse that is a copy of the current instance.
        /// </summary>
        /// <returns>A new Ellipse that is a copy of this instance.</returns>
        public override object Clone()
        {
            Ellipse entity = new Ellipse
            {
                //EntityObject properties
                Layer = (Layer) this.Layer.Clone(),
                Linetype = (Linetype) this.Linetype.Clone(),
                Color = (AciColor) this.Color.Clone(),
                Lineweight = this.Lineweight,
                Transparency = (Transparency) this.Transparency.Clone(),
                LinetypeScale = this.LinetypeScale,
                Normal = this.Normal,
                IsVisible = this.IsVisible,
                //Ellipse properties
                Center = this.center,
                MajorAxis = this.majorAxis,
                MinorAxis = this.minorAxis,
                Rotation = this.rotation,
                StartAngle = this.startAngle,
                EndAngle = this.endAngle,
                Thickness = this.thickness
            };

            foreach (XData data in this.XData.Values)
                entity.XData.Add((XData) data.Clone());

            return entity;
        }

        #endregion
    }
}