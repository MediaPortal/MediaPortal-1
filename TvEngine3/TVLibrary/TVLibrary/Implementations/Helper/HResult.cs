#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace TvLibrary
{
  /// <summary>
  ///  This class handles HResult codes returned by com class
  /// </summary>
  /// <remarks>
  /// A HResult code is a 32 bit value layed out as follows:
  ///
  ///   3 3 2 2 2 2 2 2 2 2 2 2 1 1 1 1 1 1 1 1 1 1
  ///   1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0
  ///  +---+-+-+-----------------------+-------------------------------+
  ///  |Sev|C|R|     Facility          |               Code            |
  ///  +---+-+-+-----------------------+-------------------------------+
  ///
  ///  where
  ///
  ///      Sev - is the severity code
  ///
  ///          00 - Success
  ///          01 - Informational
  ///          10 - Warning
  ///          11 - Error
  ///
  ///      C - is the Customer code flag
  ///
  ///      R - is a reserved bit
  ///
  ///      Facility - is the facility code
  ///
  ///      Code - is the facility's status code
  /// </remarks>
  public class HResult
  {
    #region Enums
    public enum Serverity
    {
      Unknown = -1,
      Success = 0,
      Info = 1,
      Warning = 2,
      Error = 3
    }

    public enum Facility
    {
      Unknown = -1,
      Null = 0,
      RPC = 1,
      Dispatch = 2,
      Storage = 3,
      ITF = 4,
      Win32 = 7,
      Windows = 8,
      Security = 9,
      Control = 10,
      Cert = 11,
      Internet = 12,
      MediaServer = 13,
      MSMQ = 14,
      SetupAPI = 15,
      SCard = 16,
      ComPlus = 17,
      AAF = 18,
      ACS = 20,
      DPlay = 21,
      UMI = 22,
      SXS = 23,
      WindowsCE = 24,
      HTTP = 25,
      BackgroundCopy = 32,
      Configuration = 33,
      StateManagement = 34,
      MetaDirectory = 35,
      D3DX = 0x877
    }
    #endregion

    #region Variables
    private uint _hresult;
    private int _facilityCode;
    private Facility _facility = Facility.Unknown;
    private int _serverityCode;
    private Serverity _serverity = Serverity.Unknown;
    private int _code;
    #endregion

    #region Constructors/Destructors
    /// <summary>
    /// Initializes a new instance of the <see cref="HResult"/> class.
    /// </summary>
    /// <param name="hresult">The hresult code.</param>
    public HResult(int hresult)
    {
      Set(hresult);
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets the facility type.
    /// </summary>
    /// <value>The facility type.</value>
    public Facility FacilityType
    {
      get { return _facility; }
    }

    /// <summary>
    /// Gets the facility code.
    /// </summary>
    /// <value>The facility code.</value>
    public int FacilityCode
    {
      get { return _facilityCode; }
    }

    /// <summary>
    /// Gets the serverity level.
    /// </summary>
    /// <value>The serverity level.</value>
    public Serverity ServerityLevel
    {
      get { return _serverity; }
    }

    /// <summary>
    /// Gets the code.
    /// </summary>
    /// <value>The code.</value>
    public int Code
    {
      get { return _code; }
    }

    /// <summary>
    /// Gets the DX error string.
    /// </summary>
    /// <value>The DX error string.</value>
    private string DXErrorString
    {
      get { return GetDXErrorString((int)_hresult); }
    }

    /// <summary>
    /// Gets the DX error description.
    /// </summary>
    /// <value>The DX error description.</value>
    private string DXErrorDescription
    {
      get { return GetDXErrorDescription((int)_hresult); }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Sets the specified hresult.
    /// </summary>
    /// <param name="hresult">The hresult.</param>
    public void Set(int hresult)
    {
      _hresult = (uint)hresult;

      _serverityCode = (int)(_hresult >> 30);
      _serverity = (Serverity)_serverityCode;

      _facilityCode = (int)(_hresult >> 16);
      _facilityCode = _facilityCode & 0x0FFF;

      if (Enum.IsDefined(typeof(Facility), _facilityCode))
        _facility = (Facility)_facilityCode;
      else
        _facility = Facility.Unknown;

      _code = (int)_hresult & 0x0000FFFF;

    }

    /// <summary>
    /// Static method which gets the DX error string.
    /// </summary>
    /// <param name="hresult">The hresult.</param>
    /// <returns>the DX error string</returns>
    [DllImport("Dxerr9.dll", EntryPoint = "GetDXErrorString", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public static extern string GetDXErrorString(int hresult);

    /// <summary>
    /// Static method which gets the DX error description.
    /// </summary>
    /// <param name="hresult">The hresult.</param>
    /// <returns>the DX error description</returns>
    [DllImport("Dxerr9.dll", EntryPoint = "GetDXErrorDescription", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public static extern string GetDXErrorDescription(int hresult);

    /// <summary>
    /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </returns>
    public override string ToString()
    {
      if (_facility == Facility.Unknown)
        return String.Format("0x{0} - {1}:Unknown(0x{2}):0x{3}", _hresult.ToString("X").PadLeft(8, '0'), _serverity, _facilityCode.ToString("X").PadLeft(3, '0'), _code.ToString("X").PadLeft(4, '0'));
      else
        return String.Format("0x{0} - {1}:{2}:0x{3}", _hresult.ToString("X").PadLeft(8, '0'), _serverity, _facility, _code.ToString("X").PadLeft(4, '0'));
    }

    /// <summary>
    /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </returns>
    public string ToDXString()
    {
      if(_hresult == 0)
        return String.Format("No DX Error");
      else
        return String.Format("DX Error: {0} - Error: {1}, Description:{2}", this.ToString(), DXErrorString, DXErrorDescription);
    }

    /// <summary>
    /// Operator ==s the specified a.
    /// </summary>
    /// <param name="a">A hresult</param>
    /// <param name="b">an int</param>
    /// <returns>bool</returns>
    public static bool operator ==(HResult a, int b)
    {
      if (a._hresult == (uint)b)
        return true;
      else if (a == null)
        return false;
      else
        return false;

    }

    public static bool operator <(HResult a, int b)
    {
      if (a._hresult < b)
        return true;
      else 
        return false;
    }

    public static bool operator > (HResult a, int b)
    {
      if (a._hresult > b)
        return true;
      else
        return false;
    }
    public static bool operator <= (HResult a, int b)
    {
      if (a._hresult <= b)
        return true;
      else
        return false;
    }

    public static bool operator >= (HResult a, int b)
    {
      if (a._hresult >= b)
        return true;
      else
        return false;
    }



    /// <summary>
    /// Operator !=s the specified a.
    /// </summary>
    /// <param name="a">A hresult</param>
    /// <param name="b">an int</param>
    /// <returns>bool</returns>
    public static bool operator !=(HResult a, int b)
    {
      return !(a == b);
    }

    /// <summary>
    /// Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>.</param>
    /// <returns>
    /// true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
    /// </returns>
    public override bool Equals(Object obj)
    {
      // Check for null values and compare run-time types.
      if (obj == null || GetType() != obj.GetType())
        return false;
      return this._hresult == ((HResult)obj)._hresult;
    }

    /// <summary>
    /// Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
    /// </summary>
    /// <returns>
    /// A hash code for the current <see cref="T:System.Object"></see>.
    /// </returns>
    public override int GetHashCode()
    {
      return _hresult.GetHashCode();
    }
    #endregion
  }
}
