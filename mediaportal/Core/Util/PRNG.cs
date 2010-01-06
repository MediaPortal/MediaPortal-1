#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

#region Usings

using System;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;

#endregion

namespace MediaPortal.Util
{
  /// <summary>
  /// Pseudo Random Number Generator; a more random version of System.Random
  /// </summary>
  public class PseudoRandomNumberGenerator
  {
    #region Variables

    /// <summary>
    /// Random number generator from the System.Security.Cryptography namespace
    /// </summary>
    private RNGCryptoServiceProvider _prng;

    #endregion

    #region Constructor/Destructor

    public PseudoRandomNumberGenerator()
    {
      _prng = new RNGCryptoServiceProvider();
    }

    ~PseudoRandomNumberGenerator()
    {
      _prng = null;
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Returns a nonnegative random number.
    /// </summary>
    /// <returns>A 32-bit signed integer greater than or equal to zero</returns>
    public int Next()
    {
      byte[] rndBytes = new byte[4];
      _prng.GetBytes(rndBytes);
      return Convert.ToInt32(BitConverter.ToUInt32(rndBytes, 0) % 2147483648);
    }

    /// <summary>
    /// Returns a nonnegative random number less than the specified maximum.
    /// </summary>
    /// <param name="maxValue">The exclusive upper bound of the random number to be generated. maxValue must be greater than or equal to zero.</param>
    /// <returns>A 32-bit signed integer greater than or equal to zero, and less than maxValue; that is, the range of return values includes zero but not maxValue.</returns>
    public int Next(int maxValue)
    {
      return Next(0, maxValue);
    }

    /// <summary>
    /// Returns a random number within a specified range.
    /// </summary>
    /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
    /// <param name="maxValue">The exclusive upper bound of the random number returned. maxValue must be greater than or equal to minValue.</param>
    /// <returns>A 32-bit signed integer greater than or equal to minValue and less than maxValue; that is, the range of return values includes minValue but not maxValue. If minValue equals maxValue, minValue is returned.</returns>
    public int Next(int minValue, int maxValue)
    {
      return Convert.ToInt32((maxValue - minValue) * Sample() + minValue);
    }

    /// <summary>
    /// Fills the elements of a specified array of bytes with random numbers.
    /// </summary>
    /// <param name="buffer">array to fill with random numbers</param>
    public void NextBytes(byte[] buffer)
    {
      _prng.GetBytes(buffer);
    }

    /// <summary>
    /// Returns a random number between 0.0 and 1.0.
    /// </summary>
    /// <returns>A double-precision floating point number greater than or equal to 0.0, and less than 1.0.</returns>
    public double NextDouble()
    {
      return Sample();
    }

    /// <summary>
    /// Returns a random number between 0.0 and 1.0.
    /// </summary>
    /// <returns>A double-precision floating point number greater than or equal to 0.0, and less than 1.0.</returns>
    protected double Sample()
    {
      return Convert.ToDouble(Next() / 2147483648.0);
    }

    #endregion
  }
}