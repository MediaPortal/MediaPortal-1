#region Copyright (C) 2005-2006 Team MediaPortal - CoolHammer, mPod, diehard2

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal - Author: CoolHammer, mPod, diehard2
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
using System.Runtime.InteropServices.ComTypes;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using X10;

namespace MediaPortal.InputDevices
{
  /// <summary>
  /// This class initializes the x10 remotes and implements a sink to catch commands. A delegate is provided
  /// so that external classes can be made aware of events - mainly useful for learning.
  /// </summary>
  public class X10Remote : _DIX10InterfaceEvents
  {
    #region Member Variables

    private X10Interface X10Inter = null;
    private IConnectionPointContainer icpc = null;
    private IConnectionPoint icp = null;
    private int cookie = 0;

    private InputHandler _inputHandler = null;
    private bool _controlEnabled = false;
    private bool _logVerbose = false;
    private bool _x10Medion = true;
    private bool _x10Ati = false;
    private bool _x10Firefly = false;
    private bool _x10UseChannelControl = false;
    private int _x10Channel = 0;
    public bool _remotefound = false;

    #endregion

    #region Callback

    //Sets up callback so that other forms can catch a key press
    public delegate void X10Event(int keypress);

    public event X10Event X10KeyPressed;

    #endregion

    #region Constructor

    public X10Remote()
    {
    }

    #endregion

    #region Init method

    public void Init()
    {
      using (Settings xmlreader = new MPSettings())
      {
        _controlEnabled = xmlreader.GetValueAsBool("remote", "X10", false);
        _x10Medion = xmlreader.GetValueAsBool("remote", "X10Medion", false);
        _x10Ati = xmlreader.GetValueAsBool("remote", "X10ATI", false);
        _x10Firefly = xmlreader.GetValueAsBool("remote", "X10Firefly", false);
        _logVerbose = xmlreader.GetValueAsBool("remote", "X10VerboseLog", false);
        _x10UseChannelControl = xmlreader.GetValueAsBool("remote", "X10UseChannelControl", false);
        _x10Channel = xmlreader.GetValueAsInt("remote", "X10Channel", 0);
      }

      //Setup the X10 Remote
      try
      {
        if (X10Inter == null)
        {
          try
          {
            X10Inter = new X10Interface();
          }
          catch (COMException)
          {
            Log.Info("X10 debug: Could not get interface");
            _remotefound = false;
            return;
          }
          _remotefound = true;

          //Bind the interface using a connection point

          icpc = (IConnectionPointContainer) X10Inter;
          Guid IID_InterfaceEvents = typeof (_DIX10InterfaceEvents).GUID;
          icpc.FindConnectionPoint(ref IID_InterfaceEvents, out icp);
          icp.Advise(this, out cookie);
        }
      }
      catch (COMException cex)
      {
        Log.Info("X10 Debug: Com error - " + cex.ToString());
      }

      if (_inputHandler == null)
      {
        if (_controlEnabled)
        {
          if (_x10Medion)
          {
            _inputHandler = new InputHandler("Medion X10");
          }
          else if (_x10Ati)
          {
            _inputHandler = new InputHandler("ATI X10");
          }
          else if (_x10Firefly)
          {
            _inputHandler = new InputHandler("Firefly X10");
          }
          else
          {
            _inputHandler = new InputHandler("Other X10");
          }
        }
        else
        {
          return;
        }

        if (!_inputHandler.IsLoaded)
        {
          _controlEnabled = false;
          Log.Info("X10: Error loading default mapping file - please reinstall MediaPortal");
          return;
        }

        if (_logVerbose)
        {
          if (_x10Medion)
          {
            Log.Info("X10Remote: Start Medion");
          }
          else if (_x10Ati)
          {
            Log.Info("X10Remote: Start ATI");
          }
          else if (_x10Firefly)
          {
            Log.Info("X10Remote: Start Firefly");
          }
          else
          {
            Log.Info("X10Remote: Start Other");
          }
        }
      }
    }

    #endregion

    #region _DIX10InterfaceEvents Members

    public void X10Command(string bszCommand, EX10Command eCommand, int lAddress, EX10Key EKeyState, int lSequence,
                           EX10Comm eCommandType, object varTimestamp)
    {
      if ((EKeyState == EX10Key.X10KEY_ON || EKeyState == EX10Key.X10KEY_REPEAT) && lSequence != 2)
      {
        if (_x10UseChannelControl && (lAddress != ((_x10Channel - 1)*16)))
        {
          return;
        }

        int keypress = (int) Enum.Parse(typeof (EX10Command), eCommand.ToString());
        if (X10KeyPressed != null)
        {
          X10KeyPressed(keypress);
        }
        if (_inputHandler != null)
        {
          _inputHandler.MapAction(keypress);
        }

        if (_logVerbose)
        {
          Log.Info("X10Remote: Command Start --------------------------------------------");
          Log.Info("X10Remote: bszCommand   = {0}", bszCommand.ToString());
          Log.Info("X10Remote: eCommand     = {0} - {1}", keypress, eCommand.ToString());
          Log.Info("X10Remote: eCommandType = {0}", eCommandType.ToString());
          Log.Info("X10Remote: eKeyState    = {0}", EKeyState.ToString());
          Log.Info("X10Remote: lAddress     = {0}", lAddress.ToString());
          Log.Info("X10Remote: lSequence    = {0}", lSequence.ToString());
          Log.Info("X10Remote: varTimestamp = {0}", varTimestamp.ToString());
          Log.Info("X10Remote: Command End ----------------------------------------------");
        }
      }
    }

    public void X10HelpEvent(int hwndDialog, int lHelpID)
    {
      throw new Exception("The method or operation is not implemented.");
    }

    #endregion
  }
}