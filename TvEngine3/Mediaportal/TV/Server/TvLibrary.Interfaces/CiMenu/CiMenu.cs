#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.CiMenu
{
  /// <summary>
  /// Delegate defines the method call from the server to the client
  /// </summary>
  /// <param name="menu">Pass a CiMenu object</param>
  public delegate void CiMenuCallback(CiMenu menu);

  /// <summary>
  /// CiMenu class contains all information of a menu
  /// derived from MarshalByRefObject for passing through remoting
  /// </summary>
  [Serializable]
  public class CiMenu 
  {
    #region private vars

    private readonly string _title;
    private readonly string _subtitle;
    private readonly string _bottomText;
    private int _numChoices;
    private CiMenuState _menuState;
    private readonly List<CiMenuEntry> _ciMenuEntries = new List<CiMenuEntry>(15);

    private int _answerLength;
    private string _requestText;
    private bool _bPassword;

    #endregion

    #region properties

    /// <summary>
    /// Title of menu
    /// </summary>
    public String Title
    {
      get { return _title; }
    }

    /// <summary>
    /// Subtitle of menu
    /// </summary>
    public String Subtitle
    {
      get { return _subtitle; }
    }

    /// <summary>
    /// Bottom text
    /// </summary>
    public String BottomText
    {
      get { return _bottomText; }
    }

    /// <summary>
    /// Number of choices
    /// </summary>
    public int NumChoices
    {
      get { return _numChoices; }
      set { _numChoices = value; }
    }

    /// <summary>
    /// Menu state
    /// </summary>
    public CiMenuState State
    {
      get { return _menuState; }
      set { _menuState = value; }
    }

    /// <summary>
    /// Menu entries
    /// </summary>
    public List<CiMenuEntry> MenuEntries
    {
      get { return _ciMenuEntries; }
    }

    /// <summary>
    /// RequestText 
    /// </summary>
    public String RequestText
    {
      get { return _requestText; }
    }

    /// <summary>
    /// AnswerLength 
    /// </summary>
    public int AnswerLength
    {
      get { return _answerLength; }
    }

    /// <summary>
    /// Request input as password?
    /// </summary>
    public bool Password
    {
      get { return _bPassword; }
    }

    #endregion

    /// <summary>
    /// CTOR
    /// </summary>
    /// <param name="title">Title</param>
    /// <param name="subtitle">Subtitle</param>
    /// <param name="bottomText">Bottomtext</param>
    /// <param name="state">Menu state</param>
    public CiMenu(String title, String subtitle, String bottomText, CiMenuState state)
    {
      _title = title;
      _subtitle = subtitle;
      _bottomText = bottomText;
      _menuState = state;

      // clear entries before filling later
      _ciMenuEntries.Clear();
    }

    /// <summary>
    /// Add an entry to ci menu object
    /// </summary>
    /// <param name="index">index</param>
    /// <param name="message">message</param>
    public void AddEntry(Int32 index, String message)
    {
      // add to list of entries
      _ciMenuEntries.Add(new CiMenuEntry(index, message));
    }

    /// <summary>
    /// Sets information from a CAM request 
    /// </summary>
    /// <param name="sRequest">Request messages</param>
    /// <param name="nAnswerLength">Expected max. answer length</param>
    /// <param name="bPassword">Show as password input</param>
    public void Request(string sRequest, int nAnswerLength, bool bPassword)
    {
      _requestText = sRequest;
      _answerLength = nAnswerLength;
      _bPassword = bPassword;
      _menuState = CiMenuState.Request;
    }
  }
}