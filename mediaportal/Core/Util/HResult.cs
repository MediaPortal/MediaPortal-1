#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.Runtime.InteropServices;

//  Values are 32 bit values layed out as follows:
//
//   3 3 2 2 2 2 2 2 2 2 2 2 1 1 1 1 1 1 1 1 1 1
//   1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0
//  +---+-+-+-----------------------+-------------------------------+
//  |Sev|C|R|     Facility          |               Code            |
//  +---+-+-+-----------------------+-------------------------------+
//
//  where
//
//      Sev - is the severity code
//
//          00 - Success
//          01 - Informational
//          10 - Warning
//          11 - Error
//
//      C - is the Customer code flag
//
//      R - is a reserved bit
//
//      Facility - is the facility code
//
//      Code - is the facility's status code

namespace MediaPortal.Util
{
  public class HRESULT
  {
    #region Enums
    public enum Serverity
    {
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
      MetaDirectory = 35
    }
    #endregion

    #region Variables
    private int _hresult;
    private int _facilityCode;
    private Facility _facility;
    private int _serverityCode;
    private Serverity _serverity;
    private int _code;
    #endregion

    #region Constructors/Destructors
    public HRESULT(int hresult)
    {
      _hresult = hresult;

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
    #endregion

    #region Properties
    public Facility FacilityType
    {
      get { return _facility; }
    }

    public int FacilityCode
    {
      get { return _facilityCode; }
    }

    public Serverity ServerityLevel
    {
      get { return _serverity; }
    }

    public int Code
    {
      get { return _code; }
    }
    public void Replace(int hresult)
    {
      _hresult = hresult;

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

    public string DXErrorString
    {
      get { return GetDXErrorString((int)_hresult); }
    }

    public string DXErrorDescription
    {
      get { return GetDXErrorDescription((int)_hresult); }
    }
    #endregion

    #region Public Methods
    [DllImport("Dxerr9.dll", EntryPoint = "GetDXErrorString", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public static extern string GetDXErrorString(int HRESULT);

    [DllImport("Dxerr9.dll", EntryPoint = "GetDXErrorDescription", ExactSpelling = false, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public static extern string GetDXErrorDescription(int HRESULT);

    public override string ToString()
    {
      if (_facility == Facility.Unknown)
        return String.Format("{0}:Unknown({1}):{2}", _serverity, _facilityCode, _code);
      else
        return String.Format("{0}:{1}:{2}", _serverity, _facility, _code);
    }

    public string ToDXString()
    {
      return String.Format("HRESULT: 0x{0:X} - Error: {1}, Description:{2}", _hresult, GetDXErrorString(_hresult), GetDXErrorDescription(_hresult));

    }

    public static bool operator ==(HRESULT a, int b)
    {
      if (a._hresult == b)
        return true;
      else if (a == null)
        return false;
      else
        return false;

    }

    public static bool operator !=(HRESULT a, int b)
    {
      return !(a == b);

    }
    #endregion
  }
}