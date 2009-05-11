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

using System;
using System.Collections;
using System.IO;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Settings.Wizard;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using MediaPortal.Util;

namespace WindowPlugins.GUISettings.Wizard.Analog
{
  /// <summary>
  /// Summary description for GUIWizardAnalogRename.
  /// </summary>
  public class GUIWizardAnalogRename : GUIWindow
  {
    [SkinControl(24)] protected GUIListControl listChannelsFound = null;
    [SkinControl(5)] protected GUIButtonControl btnNext = null;
    [SkinControl(25)] protected GUIButtonControl btnBack = null;

    private TVCaptureDevice captureCard = null;

    protected ArrayList listTvChannels = new ArrayList();


    public GUIWizardAnalogRename()
    {
      GetID = (int) Window.WINDOW_WIZARD_ANALOG_RENAME;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\wizard_tvcard_analog_rename.xml");
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      int cardID = Int32.Parse(GUIPropertyManager.GetProperty("#WizardCard"));

      // load Analog channels from DB
      listTvChannels.Clear();
      captureCard = Recorder.Get(cardID);
      if ((captureCard != null) && (captureCard.Network == NetworkType.Analog))
      {
        TVDatabase.GetChannelsForCard(ref listTvChannels, cardID);
      }

      UpdateList();
      if (listChannelsFound.Count == 0)
      {
        OnNextPage(); // no channels found skip renaming
      }
      else
      {
        if (captureCard != null)
        {
          TVChannel chan = (TVChannel) listTvChannels[0];
          captureCard.StartViewing(chan.Name);
          captureCard.Tune(chan);
        }
      }
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      if (captureCard != null)
      {
        captureCard.DeleteGraph();
        captureCard = null;
      }
      base.OnPageDestroy(newWindowId);
    }

    private void UpdateList()
    {
      int selectedItem = listChannelsFound.SelectedListItemIndex;
      listChannelsFound.Clear();
      foreach (TVChannel chan in listTvChannels)
      {
        GUIListItem item = new GUIListItem();
        item.Label = chan.Name;
        item.IsFolder = false;
        string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, chan.Name);
        if (!File.Exists(strLogo))
        {
          strLogo = "defaultVideoBig.png";
        }
        item.ThumbnailImage = strLogo;
        item.IconImage = strLogo;
        item.IconImageBig = strLogo;
        item.MusicTag = chan;
        item.OnItemSelected += new GUIListItem.ItemSelectedHandler(item_OnItemSelected);
        listChannelsFound.Add(item);
      }
      while (selectedItem > 0 && selectedItem >= listTvChannels.Count)
      {
        selectedItem--;
      }
      listChannelsFound.SelectedListItemIndex = selectedItem;
      GUIListItem selitem = listChannelsFound.SelectedListItem;
      if (selitem != null)
      {
        TVChannel ch = selitem.MusicTag as TVChannel;
        if (captureCard != null)
        {
          captureCard.StartViewing(ch.Name);
          captureCard.Tune(ch);
        }
      }
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (listChannelsFound == control)
      {
        OnShowContextMenu();
      }
      if (btnNext == control)
      {
        OnNextPage();
        return;
      }
      base.OnClicked(controlId, control, actionType);
    }

    private void OnNextPage()
    {
      ArrayList listChannels = new ArrayList();
      TVDatabase.GetChannels(ref listChannels);
      foreach (TVChannel ch in listTvChannels)
      {
        bool found = false;
        foreach (TVChannel listChan in listChannels)
        {
          if (String.Compare(listChan.Name, ch.Name, true) == 0)
          {
            listChan.Number = ch.Number;
            TVDatabase.UpdateChannel(listChan, listChan.Sort);
            if (captureCard != null)
            {
              TVDatabase.MapChannelToCard(listChan.ID, captureCard.ID);
            }
            found = true;
          }
        }
        if (!found)
        {
          TVDatabase.AddChannel(ch);
          if (captureCard != null)
          {
            TVDatabase.MapChannelToCard(ch.ID, captureCard.ID);
          }
        }
      }
      if (captureCard != null)
      {
        MapTvToOtherCards(captureCard.ID);
      }
      if (captureCard.SupportsRadio)
      {
        GUIWindowManager.ActivateWindow((int) Window.WINDOW_WIZARD_ANALOG_SCAN_RADIO);
      }
      else
      {
        GUIPropertyManager.SetProperty("#Wizard.Analog.Done", "yes");
        GUIWizardCardsDetected.ScanNextCardType();
      }
    }

    protected override void OnShowContextMenu()
    {
      GUIListItem item = listChannelsFound.SelectedListItem;
      if (item == null)
      {
        return;
      }
      TVChannel chan = (TVChannel) item.MusicTag;
      GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
      {
        dlg.Reset();
        dlg.SetHeading(GUILocalizeStrings.Get(924)); //Menu
        dlg.AddLocalizedString(117); //delete
        dlg.AddLocalizedString(118); //rename
      }
      dlg.DoModal(GetID);
      switch (dlg.SelectedId)
      {
        case 117: //delete
          foreach (TVChannel ch in listTvChannels)
          {
            if (ch.Number == chan.Number)
            {
              listTvChannels.Remove(ch);
              break;
            }
          }
          UpdateList();
          break;

        case 118: //rename
          VirtualKeyboard keyboard = (VirtualKeyboard) GUIWindowManager.GetWindow((int) Window.WINDOW_VIRTUAL_KEYBOARD);
          if (null == keyboard)
          {
            return;
          }
          keyboard.Reset();
          keyboard.Text = chan.Name;
          keyboard.DoModal(GetID);
          if (keyboard.IsConfirmed)
          {
            chan.Name = keyboard.Text;
            foreach (TVChannel ch in listTvChannels)
            {
              if (ch.Number == chan.Number)
              {
                ch.Name = chan.Name;
              }
            }
            UpdateList();
          }
          break;
      }
    }

    private void item_OnItemSelected(GUIListItem item, GUIControl parent)
    {
      TVChannel chan = (TVChannel) item.MusicTag;
      if (captureCard != null)
      {
        captureCard.StartViewing(chan.Name);
        captureCard.Tune(chan);
      }
    }

    private void MapTvToOtherCards(int id)
    {
      ArrayList tvchannels = new ArrayList();
      TVDatabase.GetChannelsForCard(ref tvchannels, id);
      for (int i = 0; i < Recorder.Count; ++i)
      {
        TVCaptureDevice dev = Recorder.Get(i);
        if (dev.Network == NetworkType.Analog && dev.ID != id)
        {
          foreach (TVChannel chan in tvchannels)
          {
            TVDatabase.MapChannelToCard(chan.ID, dev.ID);
          }
        }
      }
    }
  }
}