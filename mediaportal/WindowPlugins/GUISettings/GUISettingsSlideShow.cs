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

using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Configuration;

namespace MediaPortal.GUI.Settings
{
  /// <summary>
  /// 
  /// </summary>
  public class GUISettingsSlideshow : GUIWindow
  {
		[SkinControlAttribute(8)]		protected GUICheckMarkControl cmLoopSlideShows=null;
		[SkinControlAttribute(9)]		protected GUICheckMarkControl cmShuffleSlideShows=null;

    enum Controls
    {
      CONTROL_SPEED =2,
      CONTROL_TRANSITION=3,
      CONTROL_KENBURNS_SPEED=4,
      CONTROL_XFADE=5,
      CONTROL_KENBURNS=6,
      CONTROL_RANDOM=7      
    };

    int m_iSpeed=3;
    int m_iTransistion=20;
    int m_iKenBurnsSpeed=30;
    bool m_bXFade=false;
    bool m_bKenBurns=false;
    bool m_bRandom=false;

    public GUISettingsSlideshow()
    {
      GetID=(int)GUIWindow.Window.WINDOW_SETTINGS_SLIDESHOW;
    }

    public override bool Init()
    {
      return Load (GUIGraphicsContext.Skin+@"\SettingsSlideShow.xml");
    }
    public override void OnAction(Action action)
    {
      switch (action.wID)
      {
        case Action.ActionType.ACTION_PREVIOUS_MENU:
        {
          GUIWindowManager.ShowPreviousWindow();
          return;
        }
      }
      base.OnAction(action);
    }
    public override bool OnMessage(GUIMessage message)
    {
      switch ( message.Message )
      {

        case GUIMessage.MessageType.GUI_MSG_WINDOW_INIT:
        {
          base.OnMessage(message);
          LoadSettings();
          GUIControl.ClearControl(GetID,(int)Controls.CONTROL_SPEED);
          for (int i=1; i <=10;++i)
          {
            GUIControl.AddItemLabelControl(GetID,(int)Controls.CONTROL_SPEED,i.ToString());
          }

          GUIControl.ClearControl(GetID,(int)Controls.CONTROL_TRANSITION);
          for (int i=1; i <=50;++i)
          {
            GUIControl.AddItemLabelControl(GetID,(int)Controls.CONTROL_TRANSITION,i.ToString());
          }

          GUIControl.ClearControl(GetID,(int)Controls.CONTROL_KENBURNS_SPEED);
          for (int i=1; i <=50;++i)
          {
            GUIControl.AddItemLabelControl(GetID,(int)Controls.CONTROL_KENBURNS_SPEED,i.ToString());
          }

          GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_SPEED,m_iSpeed-1);
          GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_TRANSITION,m_iTransistion-1);
          GUIControl.SelectItemControl(GetID,(int)Controls.CONTROL_KENBURNS_SPEED,m_iKenBurnsSpeed-1);

          if (m_bXFade)
          {
            GUIControl.SelectControl(GetID, (int)Controls.CONTROL_XFADE);
          }
          if (m_bKenBurns)
					{
						GUIControl.SelectControl(GetID, (int)Controls.CONTROL_KENBURNS);
					}
          if (m_bRandom)
          {
            GUIControl.SelectControl(GetID, (int)Controls.CONTROL_RANDOM);
          }

          return true;
        }
        
        case GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT:
        {
          SaveSettings();
        }
        break;

        case GUIMessage.MessageType.GUI_MSG_CLICKED:
        {
          int iControl=message.SenderControlId;
          if (iControl==(int)Controls.CONTROL_SPEED)
          {
            string strLabel=message.Label;
            m_iSpeed=Int32.Parse(strLabel);
          }
          if (iControl==(int)Controls.CONTROL_TRANSITION)
          {
            string strLabel=message.Label;
            m_iTransistion=Int32.Parse(strLabel);
          }
          if (iControl==(int)Controls.CONTROL_KENBURNS_SPEED)
          {
            string strLabel=message.Label;
            m_iKenBurnsSpeed=Int32.Parse(strLabel);
          }
          if (iControl==(int)Controls.CONTROL_XFADE)
          {
            m_bXFade=true;
            m_bKenBurns=false;
            m_bRandom=false;
            UpdateButtons();
            return true;
          }
          if (iControl==(int)Controls.CONTROL_KENBURNS)
          {
            m_bXFade=false;
            m_bKenBurns=true;
            m_bRandom=false;
            UpdateButtons();
            return true;
          }
          if (iControl==(int)Controls.CONTROL_RANDOM)
          {
            m_bXFade=false;
            m_bKenBurns=false;
            m_bRandom=true;
            UpdateButtons();
            return true;
          }
        }
        break;

      }
      return base.OnMessage(message);
    }
    
    void UpdateButtons()
    {
      if (m_bRandom)
        GUIControl.SelectControl(GetID, (int)Controls.CONTROL_RANDOM);
      else
        GUIControl.DeSelectControl(GetID, (int)Controls.CONTROL_RANDOM);

      if (m_bXFade)
        GUIControl.SelectControl(GetID, (int)Controls.CONTROL_XFADE);
      else
        GUIControl.DeSelectControl(GetID, (int)Controls.CONTROL_XFADE);

      if (m_bKenBurns)
        GUIControl.SelectControl(GetID, (int)Controls.CONTROL_KENBURNS);
      else
        GUIControl.DeSelectControl(GetID, (int)Controls.CONTROL_KENBURNS);

    }

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (cmLoopSlideShows==control) OnLoopSlideShows();
			if (cmShuffleSlideShows==control) OnShuffleSlideShows();
			base.OnClicked (controlId, control, actionType);
		}
		void OnLoopSlideShows()
		{
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
			{
				xmlwriter.SetValueAsBool("pictures", "autoRepeat", cmLoopSlideShows.Selected);
			}
		}
		void OnShuffleSlideShows()
		{
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
			{
				xmlwriter.SetValueAsBool("pictures", "autoShuffle", cmShuffleSlideShows.Selected);
			}
		}

    #region Serialisation
    void LoadSettings()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        m_iSpeed=xmlreader.GetValueAsInt("pictures","speed",3);
        m_iTransistion=xmlreader.GetValueAsInt("pictures","transition",20);
        m_iKenBurnsSpeed=xmlreader.GetValueAsInt("pictures","kenburnsspeed",20);
        m_bKenBurns=xmlreader.GetValueAsBool("pictures","kenburns", false);
        m_bRandom=xmlreader.GetValueAsBool("pictures","random", false);
        m_bXFade = (!m_bRandom & !m_bKenBurns);


				cmShuffleSlideShows.Selected = xmlreader.GetValueAsBool("pictures", "autoShuffle", false);
				cmLoopSlideShows.Selected = xmlreader.GetValueAsBool("pictures", "autoRepeat", false);
      }      
    }

    void SaveSettings()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValue("pictures","speed",m_iSpeed.ToString());
        xmlwriter.SetValue("pictures","transition",m_iTransistion.ToString());
        xmlwriter.SetValue("pictures","kenburnsspeed",m_iKenBurnsSpeed.ToString());
        xmlwriter.SetValueAsBool("pictures","kenburns",m_bKenBurns);
        xmlwriter.SetValueAsBool("pictures","random",m_bRandom);
      }
    }
    #endregion

  }
}
