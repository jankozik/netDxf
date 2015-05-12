﻿#region netDxf, Copyright(C) 2015 Daniel Carvajal, Licensed under LGPL.
// 
//                         netDxf library
//  Copyright (C) 2009-2015 Daniel Carvajal (haplokuon@gmail.com)
//  
//  This library is free software; you can redistribute it and/or
//  modify it under the terms of the GNU Lesser General Public
//  License as published by the Free Software Foundation; either
//  version 2.1 of the License, or (at your option) any later version.
//  
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
//  FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
//  COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
//  IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
//  CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Globalization;

namespace netDxf.Units
{
    /// <summary>
    /// Utility methods to format a decimal number to its different string representations.
    /// </summary>
    public sealed class LinearUnitFormat
    {
        #region public methods

        /// <summary>
        /// Converts a length value into its scientific string representation.
        /// </summary>
        /// <param name="length">The length value.</param>
        /// <param name="format">The unit style format.</param>
        /// <returns>A string that represents the length in scientific units.</returns>
        public static string ToScientific(double length, UnitStyleFormat format)
        {
            NumberFormatInfo numberFormat = new NumberFormatInfo
            {
                NumberDecimalSeparator = format.DecimalSeparator
            };

            return length.ToString(DecimalNumberFormat(format)+ "E+00", numberFormat);
        }

        /// <summary>
        /// Converts a length value into its decimal string representation.
        /// </summary>
        /// <param name="length">The length value.</param>
        /// <param name="format">The unit style format.</param>
        /// <returns>A string that represents the length in decimal units.</returns>
        public static string ToDecimal(double length, UnitStyleFormat format)
        {
            NumberFormatInfo numberFormat = new NumberFormatInfo
            {
                NumberDecimalSeparator = format.DecimalSeparator
            };

            return length.ToString(DecimalNumberFormat(format), numberFormat);
        }

        /// <summary>
        /// Converts a length value into its feet and fractional inches string representation.
        /// </summary>
        /// <param name="length">The length value.</param>
        /// <param name="format">The unit style format.</param>
        /// <returns>A string that represents the length in feet and fractional inches.</returns>
        /// <remarks>The Architectural format assume that each drawing unit represents one inch.</remarks>
        public static string ToArchitectural(double length, UnitStyleFormat format)
        {
            int feet = (int)(length / 12);
            double inchesDec = length - 12 * feet;
            int inches = (int)inchesDec;

            if (MathHelper.IsZero(inchesDec))
            {
                if (feet == 0)
                {
                    if(format.SupressZeroFeet)
                        return "0" + format.InchesSymbol;
                    if (format.SupressZeroInches)
                        return "0" + format.FeetSymbol;
                    return "0" + format.FeetSymbol + format.FeetInchesSeparator + "0" + format.InchesSymbol;
                }
                if(format.SupressZeroInches)
                    return string.Format("{0}" + format.FeetSymbol, feet);
                
                return string.Format("{0}" + format.FeetSymbol + format.FeetInchesSeparator + "0" + format.InchesSymbol, feet);
            }

            int numerator;
            int denominator;
            GetFraction(inchesDec, (short)Math.Pow(2, format.LinearDecimalPlaces), out numerator, out denominator);

            if (numerator == 0)
            {
                if (inches == 0)
                {
                    if (feet == 0)
                    {
                        if (format.SupressZeroFeet)
                            return "0" + format.InchesSymbol;
                        if (format.SupressZeroInches)
                            return "0" + format.FeetSymbol;
                        return "0" + format.FeetSymbol + format.FeetInchesSeparator + "0" + format.InchesSymbol;
                    }
                    if (format.SupressZeroInches)
                        return string.Format("{0}" + format.FeetSymbol, feet);

                    return string.Format("{0}" + format.FeetSymbol + format.FeetInchesSeparator + "0" + format.InchesSymbol, feet);
                }
                if (feet == 0)
                {
                    if (format.SupressZeroFeet)
                        return string.Format("{0}" + format.InchesSymbol, inches);
                    return string.Format("0" + format.FeetSymbol + format.FeetInchesSeparator + "{0}" + format.InchesSymbol, inches);
                }
                return string.Format("{0}" + format.FeetSymbol + format.FeetInchesSeparator + "{0}" + format.InchesSymbol, feet, inches);
            }

            string text = string.Empty;
            string feetStr = feet + format.FeetSymbol + format.FeetInchesSeparator;
            if (format.SupressZeroFeet && feet == 0) feetStr = string.Empty;
            switch (format.FractionType)
            {
                case FractionFormatType.Diagonal:
                    text = "\\A1;" + feetStr + inches + "{\\H1.0x;\\S" + numerator + "#" + denominator + ";}" + format.InchesSymbol;
                    break;
                case FractionFormatType.Horizontal:
                    text = "\\A1;" + feetStr + inches + "{\\H1.0x;\\S" + numerator + "/" + denominator + ";}" + format.InchesSymbol;
                    break;
                case FractionFormatType.NotStacked:
                    text = feetStr + inches + " " + numerator + "/" + denominator + format.InchesSymbol;
                    break;
            }
            return text;
        }

        /// <summary>
        /// Converts a length value into its feet and decimal inches string representation.
        /// </summary>
        /// <param name="length">The length value.</param>
        /// <param name="format">The unit style format.</param>
        /// <returns>A string that represents the length in feet and decimal inches.</returns>
        /// <remarks>The Engineering format assume that each drawing unit represents one inch.</remarks>
        public static string ToEngineering(double length, UnitStyleFormat format)
        {
            NumberFormatInfo numberFormat = new NumberFormatInfo
            {
                NumberDecimalSeparator = format.DecimalSeparator
            };
            int feet = (int)(length / 12);
            double inches = length - 12 * feet;

            if (MathHelper.IsZero(inches))
            {
                if (feet == 0)
                {
                    if (format.SupressZeroFeet)
                        return "0" + format.InchesSymbol;
                    if (format.SupressZeroInches)
                        return "0" + format.FeetSymbol;
                    return "0" + format.FeetSymbol + format.FeetInchesSeparator + "0" + format.InchesSymbol;
                }
                if (format.SupressZeroInches)
                    return string.Format("{0}" + format.FeetSymbol, feet);

                return string.Format("{0}" + format.FeetSymbol + format.FeetInchesSeparator + "0" + format.InchesSymbol, feet);
            }

            string feetStr = feet + format.FeetSymbol + format.FeetInchesSeparator;
            if (format.SupressZeroFeet && feet == 0) feetStr = string.Empty;

            return string.Format(numberFormat, feetStr + inches.ToString(DecimalNumberFormat(format), numberFormat) + format.InchesSymbol);
        }

        /// <summary>
        /// Converts a length value into its fractional string representation.
        /// </summary>
        /// <param name="length">The length value.</param>
        /// <param name="format">The unit style format.</param>
        /// <returns>A string that represents the length in fractional units.</returns>
        public static string ToFractional(double length, UnitStyleFormat format)
        {
            int num = (int)length;
            int numerator;
            int denominator;
            GetFraction(length, (short)Math.Pow(2, format.LinearDecimalPlaces), out numerator, out denominator);
            if (numerator == 0)
                return string.Format("{0}", (int)length);

            string text = string.Empty;
            switch (format.FractionType)
            {
                case FractionFormatType.Diagonal:
                    text = "\\A1;" + num + "{\\H1.0x;\\S" + numerator + "#" + denominator + ";}";
                    break;
                case FractionFormatType.Horizontal:
                    text = "\\A1;" + num + "{\\H1.0x;\\S" + numerator + "/" + denominator + ";}";
                    break;
                case FractionFormatType.NotStacked:
                    text = num + " " + numerator + "/" + denominator;
                    break;
            }
            return text;
        }

        #endregion

        #region private methods

        private static string DecimalNumberFormat(UnitStyleFormat format)
        {
            char[] zeroes = new char[format.LinearDecimalPlaces + 2];
            if (format.SupressLinearLeadingZeros)
                zeroes[0] = '#';
            else
                zeroes[0] = '0';

            zeroes[1] = '.';

            for (int i = 2; i < zeroes.Length; i++)
            {
                if (format.SupressLinearTrailingZeros)
                    zeroes[i] = '#';
                else
                    zeroes[i] = '0';
            }
            return new string(zeroes);
        }

        private static void GetFraction(double number, int precision, out int numerator, out int denominator)
        {
            numerator = Convert.ToInt32(((number - (int)number) * precision));
            int commonFactor = GetGCD(numerator, precision);
            if (commonFactor <= 0) commonFactor = 1;
            numerator = numerator / commonFactor;
            denominator = precision / commonFactor;
        }

        private static int GetGCD(int number1, int number2)
        {
            int a = number1;
            int b = number2;
            while (b != 0)
            {
                int count = a % b;
                a = b;
                b = count;
            }
            return a;
        }
        #endregion
    }
}
