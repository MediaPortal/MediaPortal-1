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
using System;
using System.Runtime.InteropServices;

namespace TvLibrary.Interfaces
{
  /// <summary>
  /// State of ci menu
  /// </summary>
  public enum CiMenuState
  {
    Closed    = 0,
    Opened    = 1,
    Ready     = 2,
    Request   = 3,
    NoChoices = 4,
    Error     = 5
  }

  /////<summary>
  ///// The tswriter ci menu callback
  /////</summary>
  //[ComVisible(true), ComImport,
  // Guid("8B633992-AD34-4a34-9C3F-B8DD69B2295C"),
  //InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  ////InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
  //public interface ICiMenuCallbacks
  //{
  //  [PreserveSig]
  //  int OnCiMenu(
  //    [MarshalAs(UnmanagedType.LPStr)] string lpszTitle, 
  //    [MarshalAs(UnmanagedType.LPStr)] string lpszSubTitle, 
  //    [MarshalAs(UnmanagedType.LPStr)] string lpszBottom, 
  //    int nNumChoices);

  //  [PreserveSig]
  //  int OnCiMenuChoice(
  //    int nChoice,
  //    [MarshalAs(UnmanagedType.LPStr)] string lpszText);

  //  [PreserveSig]
  //  int OnCiCloseDisplay(
  //    int nDelay);

  //  [PreserveSig]
  //  int OnCiRequest (
  //    bool bBlind,
  //    uint nAnswerLength,
  //    [MarshalAs(UnmanagedType.LPStr)] string lpszText);
  //};

  /// <summary>
  /// Interface for all DVB cards to support CI menu
  /// </summary>
  public interface ICiMenuCallbacks
  {
    int OnCiMenu(
      string  lpszTitle,
      string  lpszSubTitle,
      string  lpszBottom,
      int     nNumChoices);

    int OnCiMenuChoice(
      int     nChoice,
      string  lpszText);

    int OnCiCloseDisplay(
      int     nDelay);

    int OnCiRequest(
      bool    bBlind,
      uint    nAnswerLength,
      string  lpszText);
  };
}
