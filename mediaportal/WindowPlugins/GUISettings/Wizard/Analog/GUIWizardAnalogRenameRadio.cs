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
using System.Collections;
using System.Xml;
using System.Threading;
using MediaPortal.Dialogs;
using MediaPortal.Radio.Database;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Recording;
using MediaPortal.Util;
using MediaPortal.GUI.Settings.Wizard;
using DShowNET;
namespace WindowPlugins.GUISettings.Wizard.Analog
{
	/// <summary>
	/// Summary description for GUIWizardAnalogRenameRadio.
	/// </summary>
	public class GUIWizardAnalogRenameRadio : GUIWindow
	{
		[SkinControlAttribute(24)]			protected GUIListControl  listChannelsFound=null;
		[SkinControlAttribute(5)]			  protected GUIButtonControl  btnNext=null;
		[SkinControlAttribute(25)]			protected GUIButtonControl  btnBack=null;
		TVCaptureDevice captureCard=null;

		public GUIWizardAnalogRenameRadio()
		{
			
			GetID=(int)GUIWindow.Window.WINDOW_WIZARD_ANALOG_RENAME_RADIO;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\wizard_tvcard_analog_rename.xml");
		}

		protected override void OnPageLoad()
		{
			base.OnPageLoad ();

		    UpdateList();
            if (listChannelsFound.Count == 0)
            {
                OnNextPage();		// no channels found skip renaming
            }
            else
            {
                int card = Int32.Parse(GUIPropertyManager.GetProperty("#WizardCard"));
                if (card >= 0 && card < Recorder.Count)
                {
                    captureCard = Recorder.Get(card);
                    RadioStation chan = (RadioStation)GUIWizardAnalogTuneRadio.RadioStationsFound[0];
                    captureCard.StartRadio(chan);
                }
            }
		}
		protected override void OnPageDestroy(int newWindowId)
		{
			if (captureCard!=null)
			{
				captureCard.DeleteGraph();
				captureCard=null;
			}
			base.OnPageDestroy (newWindowId);
		}

		void UpdateList()
		{
			int selectedItem=listChannelsFound.SelectedListItemIndex;
			listChannelsFound.Clear();
			foreach (RadioStation chan in GUIWizardAnalogTuneRadio.RadioStationsFound)
			{
				GUIListItem item = new GUIListItem();
				item.Label=chan.Name;
				item.IsFolder=false;
				string strLogo="DefaultMyradio.png";
				item.ThumbnailImage=strLogo;
				item.IconImage=strLogo;
				item.IconImageBig=strLogo;
				item.MusicTag=chan;
				item.OnItemSelected+=new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(item_OnItemSelected);
				listChannelsFound.Add(item);
			}
			while (selectedItem>0 && selectedItem>=GUIWizardAnalogTuneRadio.RadioStationsFound.Count)
				selectedItem--;
			listChannelsFound.SelectedListItemIndex=selectedItem;
			GUIListItem selitem=listChannelsFound.SelectedListItem;
			if (selitem!=null)
			{
				RadioStation ch = selitem.MusicTag as RadioStation;
				if (captureCard!=null)
				{
					captureCard.StartRadio(ch);
				}
			}

		}
		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (listChannelsFound==control)
			{
				OnShowContextMenu();
			}
			if (btnNext==control)
			{
				OnNextPage();
				return;
			}
			base.OnClicked (controlId, control, actionType);
		}
		void OnNextPage()
		{
			ArrayList listChannels =new ArrayList();
			RadioDatabase.GetStations(ref listChannels);
			foreach (RadioStation ch in GUIWizardAnalogTuneRadio.RadioStationsFound)
			{
				bool found=false;
				foreach (RadioStation listChan in listChannels)
				{
					if (String.Compare(listChan.Name,ch.Name,true)==0)
					{
						listChan.Frequency=ch.Frequency;
						RadioDatabase.UpdateStation(listChan);
						if (captureCard!=null)
							RadioDatabase.MapChannelToCard(listChan.ID,captureCard.ID);
						found=true;
					}
				}
				if (!found)
				{
					RadioStation newStation = new RadioStation();
					newStation.Name=ch.Name;
					newStation.Frequency=ch.Frequency;
					RadioDatabase.AddStation(ref newStation);
					if (captureCard!=null)
						RadioDatabase.MapChannelToCard(ch.ID,captureCard.ID);
				}
			}
			if (captureCard!=null)
			{
				MapRadioToOtherCards(captureCard.ID);
			}
			GUIPropertyManager.SetProperty("#Wizard.Analog.Done","yes");
			GUIWizardCardsDetected.ScanNextCardType();
		}

		protected override void OnShowContextMenu()
		{
			GUIListItem item = listChannelsFound.SelectedListItem;
			if (item==null) return;
			RadioStation chan = (RadioStation)item.MusicTag;
			GUIDialogMenu dlg=(GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
			if (dlg!=null)
			{
				dlg.Reset();
				dlg.SetHeading(GUILocalizeStrings.Get(924));//Menu
				dlg.AddLocalizedString(117);//delete
				dlg.AddLocalizedString(118);//rename
			}
			dlg.DoModal(GetID);
			switch (dlg.SelectedId)
			{
				case 117://delete
					foreach (RadioStation ch in GUIWizardAnalogTuneRadio.RadioStationsFound)
					{
						if (ch.Frequency==chan.Frequency)
						{
							GUIWizardAnalogTuneRadio.RadioStationsFound.Remove(ch);
							break;
						}
					}
					UpdateList();
				break;

				case 118://rename
					VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
					if (null == keyboard) return ;
					keyboard.Reset();
					keyboard.Text=chan.Name;
					keyboard.DoModal(GetID);
					if (keyboard.IsConfirmed)
					{
						chan.Name = keyboard.Text;
						foreach (RadioStation ch in GUIWizardAnalogTuneRadio.RadioStationsFound)
						{
							if (ch.Frequency==chan.Frequency)
							{
								ch.Name=chan.Name;
							}
						}
						UpdateList();
					}
					break;
			}
		}

		private void item_OnItemSelected(GUIListItem item, GUIControl parent)
		{
			RadioStation chan = (RadioStation)item.MusicTag;
			if (captureCard!=null)
			{
				captureCard.StartRadio(chan);

			}
		}
		void MapRadioToOtherCards(int id)
		{
			ArrayList radioChans = new ArrayList();
			MediaPortal.Radio.Database.RadioDatabase.GetStationsForCard(ref radioChans,id);
			for (int i=0; i < Recorder.Count;++i)
			{
				TVCaptureDevice dev = Recorder.Get(i);

				if (dev.Network==NetworkType.Analog && dev.ID != id)
				{
					foreach (MediaPortal.Radio.Database.RadioStation chan in radioChans)
					{
						MediaPortal.Radio.Database.RadioDatabase.MapChannelToCard(chan.ID,dev.ID);
					}
				}
			}
		}

	}
}
