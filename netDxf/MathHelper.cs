﻿#region netDxf, Copyright(C) 2012 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2012 Daniel Carvajal (haplokuon@gmail.com)
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

namespace netDxf
{
    /// <summary>
    /// Utility math functions and constants.
    /// </summary>
    public class MathHelper
    {
        #region CoordinateSystem enum

        /// <summary>
        /// Defines the coordinate system reference.
        /// </summary>
        public enum CoordinateSystem
        {
            /// <summary>
            /// World coordinates.
            /// </summary>
            World,
            /// <summary>
            /// Object coordinates.
            /// </summary>
            Object
        }

        #endregion

        #region constants
        /// <summary>
        /// Represents the smallest number.
        /// </summary>
        public const double Epsilon = 0.000000000001;
        /// <summary>
        /// Defines the max number of decimals of an angle. Trigonometric functions are very prone to round off errors.
        /// </summary>
        public const int MaxAngleDecimals = 12;
        /// <summary>
        /// Constant to transform an angle between degrees and radians.
        /// </summary>
        public const double DegToRad = Math.PI / 180.0;

        /// <summary>
        /// Constant to transform an angle between degrees and radians.
        /// </summary>
        public const double RadToDeg = 180.0 / Math.PI;

        /// <summary>
        /// PI/2 (90 degrees)
        /// </summary>
        public const double HalfPI = Math.PI * 0.5;

        /// <summary>
        /// PI (180 degrees)
        /// </summary>
        public const double PI = Math.PI;

        /// <summary>
        /// 3*PI/2 (270 degrees)
        /// </summary>
        public const double ThreeHalfPI = 3 * Math.PI * 0.5;

        /// <summary>
        /// 2*PI (360 degrees)
        /// </summary>
        public const double TwoPI = 2 * Math.PI;

        #endregion

        #region static methods

        /// <summary>
        /// Checks if a number is close to one.
        /// </summary>
        /// <param name="number">Double precision number.</param>
        /// <param name="threshold">Tolerance.</param>
        /// <returns>True if its close to one or false in anyother case.</returns>
        public static bool IsOne(double number, double threshold = Epsilon)
        {
            return IsZero(number - 1, threshold);
        }

        /// <summary>
        /// Checks if a number is close to zero.
        /// </summary>
        /// <param name="number">Double precision number.</param>
        /// <param name="threshold">Tolerance.</param>
        /// <returns>True if its close to one or false in anyother case.</returns>
        public static bool IsZero(double number, double threshold = Epsilon)
        {
            return number >= -threshold && number <= threshold;
        }

        /// <summary>
        /// Checks if a number is equal to another.
        /// </summary>
        /// <param name="a">Double precision number.</param>
        /// <param name="b">Double precision number.</param>
        /// <param name="threshold">Tolerance.</param>
        /// <returns>True if its close to one or false in anyother case.</returns>
        public static bool IsEqual(double a, double b, double threshold = Epsilon)
        {
            return IsZero(a - b, threshold);
        }

        /// <summary>
        /// Transforms a point between coordinate systems.
        /// </summary>
        /// <param name="point">Point to transform.</param>
        /// <param name="rotation">Rotation angle in radians.</param>
        /// <param name="from">Point coordinate system.</param>
        /// <param name="to">Coordinate system of the transformed point.</param>
        /// <returns>Transormed point.</returns>
        public static Vector2 Transform(Vector2 point, double rotation, CoordinateSystem from, CoordinateSystem to)
        {
            // if the rotation is 0 no transformation is needed the transformation matrix is the identity
            if (IsZero(rotation)) return point;

            double sin = Math.Sin(rotation);
            double cos = Math.Cos(rotation);
            if (from == CoordinateSystem.World && to == CoordinateSystem.Object)
            {
                return new Vector2(point.X*cos + point.Y*sin, -point.X*sin + point.Y*cos);
            }
            if (from == CoordinateSystem.Object && to == CoordinateSystem.World)
            {
                return new Vector2(point.X * cos - point.Y * sin, point.X * sin + point.Y * cos);
            }
            return point;
        }

        /// <summary>
        /// Transforms a point between coordinate systems.
        /// </summary>
        /// <param name="point">Point to transform.</param>
        /// <param name="zAxis">Object normal vector.</param>
        /// <param name="from">Point coordinate system.</param>
        /// <param name="to">Coordinate system of the transformed point.</param>
        /// <returns>Transormed point.</returns>
        public static Vector3 Transform(Vector3 point, Vector3 zAxis, CoordinateSystem from, CoordinateSystem to)
        {
            // if the normal is (0,0,1) no transformation is needed the transformation matrix is the identity
            if(zAxis.Equals(Vector3.UnitZ)) return point;

            Matrix3 trans = ArbitraryAxis(zAxis);
            if (from == CoordinateSystem.World && to == CoordinateSystem.Object)
            {
                trans = trans.Traspose();
                return trans*point;
            }
            if (from == CoordinateSystem.Object && to == CoordinateSystem.World)
            {
                return trans*point;
            }
            return point;
        }

        /// <summary>
        /// Transforms a point list between coordinate systems.
        /// </summary>
        /// <param name="points">Points to transform.</param>
        /// <param name="zAxis">Object normal vector.</param>
        /// <param name="from">Points coordinate system.</param>
        /// <param name="to">Coordinate system of the transformed points.</param>
        /// <returns>Transormed point list.</returns>
        public static IList<Vector3> Transform(IList<Vector3> points, Vector3 zAxis, CoordinateSystem from, CoordinateSystem to)
        {
            if (zAxis.Equals(Vector3.UnitZ)) return points;

            Matrix3 trans = ArbitraryAxis(zAxis);
            List<Vector3> transPoints;
            if (from == CoordinateSystem.World && to == CoordinateSystem.Object)
            {
                transPoints = new List<Vector3>();
                trans = trans.Traspose();
                foreach (Vector3 p in points)
                {
                    transPoints.Add(trans*p);
                }
                return transPoints;
            }
            if (from == CoordinateSystem.Object && to == CoordinateSystem.World)
            {
                transPoints = new List<Vector3>();
                foreach (Vector3 p in points)
                {
                    transPoints.Add(trans*p);
                }
                return transPoints;
            }
            return points;
        }

        /// <summary>
        /// Gets the rotation matrix from the normal vector (extrusion direction) of an entity.
        /// </summary>
        /// <param name="zAxis">Normal vector.</param>
        /// <returns>Rotation matriz.</returns>
        public static Matrix3 ArbitraryAxis(Vector3 zAxis)
        {
            zAxis.Normalize();
            Vector3 wY = Vector3.UnitY;
            Vector3 wZ = Vector3.UnitZ;
            Vector3 aX;

            if ((Math.Abs(zAxis.X) < 1/64.0) && (Math.Abs(zAxis.Y) < 1/64.0))
                aX = Vector3.CrossProduct(wY, zAxis);
            else
                aX = Vector3.CrossProduct(wZ, zAxis);

            aX.Normalize();

            Vector3 aY = Vector3.CrossProduct(zAxis, aX);
            aY.Normalize();

            return new Matrix3(aX.X, aY.X, zAxis.X, aX.Y, aY.Y, zAxis.Y, aX.Z, aY.Z, zAxis.Z);
        }

        /// <summary>
        /// Calculates the minimum distance between a point and a line.
        /// </summary>
        /// <param name="q">A point.</param>
        /// <param name="origin">Line origin point.</param>
        /// <param name="dir">Line direction.</param>
        /// <returns>The minimum distance between the point and the line.</returns>
        public static double PointLineDistance(Vector3 q, Vector3 origin, Vector3 dir)
        {
            double t = Vector3.DotProduct(dir, q - origin);
            Vector3 qPrime = origin + t * dir;
            Vector3 vec = q - qPrime;
            double distanceSquared = Vector3.DotProduct(vec, vec);
            return Math.Sqrt(distanceSquared);
        }

        public static void OffsetLine(Vector3 start, Vector3 end, Vector3 normal, double offset, out Vector3 newStart, out Vector3 newEnd)
        {
            Vector3 dir = end - start;
            dir.Normalize();
            Vector3 perp = Vector3.CrossProduct(normal, dir);
            newStart = start + perp * offset;
            newEnd = end + perp * offset;
        }

        public static void OffsetLine(Vector2 start, Vector2 end, double offset, out Vector2 newStart, out Vector2 newEnd)
        {
            Vector2 dir = end - start;
            dir.Normalize();
            Vector2 perp = Vector2.Perpendicular(dir);
            newStart = start + perp * offset;
            newEnd = end + perp * offset;
        }

        /// <summary>
        /// Calculates the intersection point of two lines.
        /// </summary>
        /// <param name="point0">First line origin point.</param>
        /// <param name="dir0">First line direction.</param>
        /// <param name="point1">Second line origin point.</param>
        /// <param name="dir1">Second line direction.</param>
        /// <param name="intersection">Intersection point of the two lines.</param>
        /// <returns>0 if there is an intersection point, 1 if the lines are parallel or 2 if the lines are the same.</returns>
        public static int FindIntersection(Vector2 point0, Vector2 dir0, Vector2 point1, Vector2 dir1, out Vector2 intersection)
        {
            // Use a relative error test to test for parallelism. This effectively
            // is a threshold on the angle between D0 and D1. The threshold parameter is defined in MathHelper.Epsilon available globally.
            Vector2 vect = point1 - point0;
            double cross = Vector2.CrossProduct(dir0, dir1);
            double sqrCross = cross * cross;
            double sqrLen0 = dir0.X * dir0.X + dir0.Y * dir0.Y;
            double sqrLen1 = dir1.X * dir1.X + dir1.Y * dir1.Y;
            if (sqrCross > Epsilon * sqrLen0 * sqrLen1) {
                // lines are not parallel
                double s = (vect.X * dir1.Y - vect.Y * dir1.X) / cross;
                intersection = point0 + s * dir0;
                return 0;
            }
            // lines are parallel
            double sqrLenE = vect.X * vect.X + vect.Y * vect.Y;
            cross = vect.X * dir0.Y - vect.Y * dir0.X;
            sqrCross = cross * cross;
            if (sqrCross > Epsilon * sqrLen0 * sqrLenE)
            {
                // lines are different
                intersection = Vector2.Zero;
                return 1;
            }
            // lines are the same
            intersection = Vector2.Zero;
            return 2;   
        }

        #endregion

    }
}