#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Text;
using System.Collections.Generic;
using System.Runtime.Remoting;

namespace TvLibrary.Interfaces
{
  /// <summary>
  /// Delegate defines the method call from the server to the client
  /// </summary>
  /// <param name="Menu">Pass a CiMenu object</param>
  public delegate void CiMenuCallback(CiMenu Menu);

  /// <summary>
  /// This class is used by client to provide delegates to the server that will
  /// fire events back through these delegates. Overriding OnServerEvent to capture
  /// the callback from the server
  /// </summary>
  public abstract class CiMenuCallbackSink : MarshalByRefObject
  {
    /// <summary>
    /// Called by the server to fire the call back to the client
    /// </summary>
    /// <param name="Menu">a CiMenu object</param>
    public void FireCiMenuCallback(CiMenu Menu)
    {
      //Console.WriteLine("Activating callback");
      CiMenuCallback(Menu);
    }

    /// <summary>
    /// Client overrides this method to receive the callback events from the server
    /// </summary>
    /// <param name="Menu">a CiMenu object</param>
    protected abstract void CiMenuCallback(CiMenu Menu);
  }

  /// <summary>
  /// CiMenuEntry class to store a single entry
  /// </summary>
  [Serializable]
  public class CiMenuEntry
  {
    private Int32 m_Index;
    private String m_Message;

    /// <summary>
    /// Index of menu entry
    /// </summary>
    public int Index
    {
      get { return m_Index; }
    }

    /// <summary>
    /// Message of menu entry
    /// </summary>
    public String Message
    {
      get { return m_Message; }
    }

    /// <summary>
    /// CTOR
    /// </summary>
    /// <param name="Index">Index of entry</param>
    /// <param name="Message">Message</param>
    public CiMenuEntry(Int32 Index, String Message)
    {
      m_Index = Index;
      m_Message = Message;
    }

    /// <summary>
    /// Formatted choice text
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return String.Format("{0}) {1}", m_Index, m_Message);
    }
  }

  /// <summary>
  /// CiMenu class contains all information of a menu
  /// derived from MarshalByRefObject for passing through remoting
  /// </summary>
  [Serializable]
  public class CiMenu : MarshalByRefObject
  {
    #region private vars

    private string _title;
    private string _subtitle;
    private string _bottomText;
    private int _numChoices;
    private CiMenuState _menuState;
    private List<CiMenuEntry> _ciMenuEntries = new List<CiMenuEntry>();

    private int _answerLength;
    private string _requestText;
    private bool _bPassword = false;

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
    /// <param name="Title">Title</param>
    /// <param name="Subtitle">Subtitle</param>
    /// <param name="BottomText">Bottomtext</param>
    /// <param name="State">Menu state</param>
    public CiMenu(String Title, String Subtitle, String BottomText, CiMenuState State)
    {
      _title = Title;
      _subtitle = Subtitle;
      _bottomText = BottomText;
      _menuState = State;

      // clear entries before filling later
      _ciMenuEntries.Clear();
    }

    /// <summary>
    /// Add an entry to ci menu object
    /// </summary>
    /// <param name="Index">index</param>
    /// <param name="Message">message</param>
    public void AddEntry(Int32 Index, String Message)
    {
      // add to list of entries
      _ciMenuEntries.Add(new CiMenuEntry(Index, Message));
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

    /// <summary>
    /// override to avoid timeout ?
    /// </summary>
    /// <returns></returns>
    public override object InitializeLifetimeService()
    {
      return null;
    }
  }
}