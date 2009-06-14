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
using TvControl;
using TvLibrary.Log;
using TvLibrary.Implementations.DVB;
using TvLibrary.Implementations;
using TvLibrary.Interfaces;
using System.Windows.Forms;


namespace SetupTv.Sections
{
  [CLSCompliant(false)]
  public partial class CI_Menu_Dialog : SetupTv.SectionSettings
  {
    CiMenuHandler ciMenuHandler;
    CiMenuState ciMenuState = CiMenuState.Closed;
    int ciMenuChoices = 0;
    int cardNumber = 0;
    bool InitSuccess = false;

    public CI_Menu_Dialog(int p_cardNumber) : this()
    {
      cardNumber = p_cardNumber;
    }
    public CI_Menu_Dialog()
    {
      InitializeComponent();

      InitSuccess = false;

      ciMenuHandler = new CiMenuHandler();
      ciMenuHandler.SetCaller(this);
      
      // attach local eventhandler to server event
      RemoteControl.RegisterCiMenuCallbacks(ciMenuHandler);
    }

    private void CI_Menu_Dialog_Load(object sender, EventArgs e)
    {
      SetButtonState();
    }

    private void InitMenu()
    {
      Title.Text = Subtitle.Text = BottomText.Text = CiRequest.Text = "";
      Choices.Items.Clear();
    }

    private bool IsCAReady()
    {
      if (InitSuccess) return true;
      // call only once
      if (RemoteControl.Instance.InitConditionalAccess(cardNumber))
      {
        if (!RemoteControl.Instance.CiMenuSupported(cardNumber))
        {
          MessageBox.Show("The selected card doesn't support CI menu or CAM is not ready yet\r\n(Inititialization of CAM may require >10 seconds)");
          return false;
        }
        InitSuccess = true;
      }
      return InitSuccess;
    }
    private void btnOk_Click(object sender, EventArgs e)
    {
      try
      {
        if (IsCAReady())
        {
          RemoteControl.Instance.SetCiMenuHandler(cardNumber, null);
          RemoteControl.Instance.EnterCiMenu(cardNumber);
        }        
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
    }

    private void btnCloseMenu_Click(object sender, EventArgs e)
    {
      try
      {
        if (ciMenuState == CiMenuState.NoChoices || ciMenuState == CiMenuState.Ready)
        {
          RemoteControl.Instance.SelectMenu(cardNumber, 0); // back
          ciMenuState = CiMenuState.Closed;
        }
        if (ciMenuState == CiMenuState.Request)
        {
          RemoteControl.Instance.SendMenuAnswer(cardNumber, true, null);
          ciMenuState = CiMenuState.Ready;
        }
        SetButtonState();
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
    }

    private void btnSendAnswer_Click(object sender, EventArgs e)
    {
      try
      {
        if (ciMenuState == CiMenuState.Ready && Choices.SelectedIndex != -1)
        {
          RemoteControl.Instance.SelectMenu(cardNumber, Convert.ToByte(Choices.SelectedIndex + 1));
        }
        if (ciMenuState == CiMenuState.Request)
        {
          RemoteControl.Instance.SendMenuAnswer(cardNumber, false, CiAnswer.Text);
          ciMenuState = CiMenuState.Ready;
        }
        SetButtonState();
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
    }

    /// <summary>
    /// Handles all CiMenu actions from callback
    /// </summary>
    /// <param name="Menu">complete CI menu object</param>
    public void CiMenuCallback(CiMenu Menu)
    {
      ciMenuState = Menu.State;

      switch (ciMenuState)
      {
        // choices available, so show them
        case TvLibrary.Interfaces.CiMenuState.Ready:
          //ciMenuState = CiMenuState.Opened;
          Title.Text = Menu.Title;
          Subtitle.Text = Menu.Subtitle;
          BottomText.Text = Menu.BottomText;
          ciMenuChoices = Menu.NumChoices;

          Choices.Items.Clear();

          // no choices then we are ready yet
          if (ciMenuChoices == 0)
          {
            ciMenuState = CiMenuState.NoChoices;
            SetButtonState();
          }
          else
          {
            foreach (CiMenuEntry entry in Menu.MenuEntries)
            {
              Choices.Items.Add(entry);
            }
          }
          break;

        // errors and menu options with no choices
        case TvLibrary.Interfaces.CiMenuState.Error:
        case TvLibrary.Interfaces.CiMenuState.NoChoices:
          Title.Text = Menu.Title;
          Subtitle.Text = Menu.Subtitle;
          BottomText.Text = Menu.BottomText;
          ciMenuChoices = Menu.NumChoices;
          break;

        // requests require users input so open keyboard
        case TvLibrary.Interfaces.CiMenuState.Request:
          ciMenuState = CiMenuState.Request;
          SetButtonState();
          CiRequest.Text = String.Format("{0} ({1} Zeichen)", Menu.RequestText, Menu.AnswerLength);
          CiAnswer.MaxLength = (int)Menu.AnswerLength;
          CiAnswer.Text = "";
          CiAnswer.Focus();
          break;
      }
      SetButtonState();
    }

    private void SetButtonState()
    {
      btnOk.Enabled = ciMenuState == CiMenuState.Closed;
      btnCloseMenu.Enabled = (ciMenuState == CiMenuState.Ready || ciMenuState == CiMenuState.Request || ciMenuState == CiMenuState.NoChoices);
      btnSendAnswer.Enabled = (ciMenuState == CiMenuState.Ready || ciMenuState == CiMenuState.Request);
      grpCIMenu.Enabled = ciMenuState != CiMenuState.Closed;
      CiRequest.Visible = ciMenuState == CiMenuState.Request;
      CiAnswer.Visible = ciMenuState == CiMenuState.Request;
      if (ciMenuState == CiMenuState.Closed) InitMenu();
    }

  }
}
