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

using MediaPortal.GUI.Library;

namespace MediaPortal.Dialogs
{
  /// <summary>
  ///  This is a MediaPortal Class.
  /// </summary>
  public class GUIDialogProgress : GUIDialogWindow
  {
    #region Enums

    protected enum Controls
    {
      CancelButton = 10,
      ProgressBar = 20
    } ;

    #endregion

    #region Variables

    // Private Variables
    // Protected Variables
    protected bool _showWaitCursor = false;
    private int _percentage = 0;
    private bool _showProgressBar = false;
    private bool _canceled = false;
    private WaitCursor cursor = null;
    // Public Variables

    #endregion

    #region Constructors/Destructors

    public GUIDialogProgress()
    {
      GetID = (int) Window.WINDOW_DIALOG_PROGRESS;
    }

    #endregion

    #region Properties

    // Public Properties
    public int Percentage
    {
      get { return _percentage; }
      set
      {
        if (Percentage != value)
        {
          _percentage = value;
        }
        GUIProgressControl progress = (GUIProgressControl) GetControl((int) Controls.ProgressBar);
        if (progress != null)
        {
          progress.Percentage = _percentage;
        }
      }
    }

    public bool DisplayProgressBar
    {
      get { return _showProgressBar; }
      set
      {
        if (_showProgressBar != value)
        {
          _showProgressBar = value;
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_VISIBLE, GetID, 0, (int) Controls.ProgressBar,
                                          0, 0, null);
          if (!_showProgressBar)
          {
            msg.Message = GUIMessage.MessageType.GUI_MSG_HIDDEN;
          }
          OnMessage(msg);
        }
      }
    }

    public bool ShowWaitCursor
    {
      get { return _showWaitCursor; }
      set
      {
        if (ShowWaitCursor != value)
        {
          _showWaitCursor = value;
          if (_showWaitCursor)
          {
            cursor = new WaitCursor();
          }
          else
          {
            if (cursor != null)
            {
              cursor.Dispose();
              cursor = null;
            }
          }
        }
      }
    }

    public bool IsCanceled
    {
      get { return _canceled; }
    }

    #endregion

    #region Public Methods

    public void SetHeading(string HeadingLine)
    {
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, 1, 0, 0, null);
      msg.Label = HeadingLine;
      OnMessage(msg);
    }

    public void SetHeading(int LocalizeID)
    {
      //Reset();
      SetHeading(GUILocalizeStrings.Get(LocalizeID));
    }

    public void SetLine(int LineNr, string Line)
    {
      if (LineNr < 1)
      {
        return;
      }
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_LABEL_SET, GetID, 0, 1 + LineNr, 0, 0, null);
      msg.Label = Line;
      if ((msg.Label == string.Empty) || (msg.Label == ""))
      {
        msg.Label = "  ";
      }
      OnMessage(msg);
    }

    public void SetLine(int LineNr, int LocalizeID)
    {
      SetLine(LineNr, GUILocalizeStrings.Get(LocalizeID));
    }

    #endregion

    #region Old routines -> to be deleted

    // REFACTOR THIS ONE

    public void SetPercentage(int NewPercentage)
    {
      Percentage = NewPercentage;
    }

    public void Progress()
    {
    }

    public void StartModal(int ParentId)
    {
      _canceled = false;
      PageLoad(ParentId);
    }

    public void ShowProgressBar(bool DisplayBar)
    {
      DisplayProgressBar = DisplayBar;
    }

    public void ProgressKeys()
    {
    }

    public void DisableCancel(bool bOnOff)
    {
      if (bOnOff)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_DISABLED, GetID, 0, (int) Controls.CancelButton,
                                        0, 0, null);
        OnMessage(msg);
      }
      else
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ENABLED, GetID, 0, (int) Controls.CancelButton, 0,
                                        0, null);
        OnMessage(msg);
      }
    }

    public void Close()
    {
      PageDestroy();
    }

    //------------------------------------------------------

    #endregion

    #region Protected Methods

    #endregion

    #region Private Methods

    #endregion

    #region <Base class> Overloads

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\dialogProgress.xml");
    }

    public override void Reset()
    {
      base.Reset();
      Percentage = 0;
      DisplayProgressBar = true;
      ShowWaitCursor = false;
      SetHeading(string.Empty);
      SetLine(1, string.Empty);
      SetLine(2, string.Empty);
      SetLine(3, string.Empty);
      SetLine(4, string.Empty);
    }

    public override bool OnMessage(GUIMessage message)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
          {
            _canceled = false;
            return base.OnMessage(message);
          }


        case GUIMessage.MessageType.GUI_MSG_CLICKED:
          {
            if (message.SenderControlId == (int) Controls.CancelButton)
            {
              _canceled = true;
              PageDestroy();
              return true;
            }
          }
          break;
      }

      if ((_running) && (_parentWindow != null))
      {
        if (message.TargetWindowId == _parentWindow.GetID)
        {
          return _parentWindow.OnMessage(message);
        }
      }

      return base.OnMessage(message);
    }

    #endregion
  }
}