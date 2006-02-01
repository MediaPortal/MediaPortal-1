using System;
using System.Collections;
using System.Xml;
using System.Threading;
using MediaPortal.Dialogs;
using MediaPortal.TV.Database;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Recording;
using MediaPortal.Util;
using MediaPortal.GUI.Settings.Wizard;
using DShowNET;
namespace WindowPlugins.GUISettings.Wizard.Analog
{
  /// <summary>
  /// Summary description for GUIWizardAnalogRename.
  /// </summary>
  public class GUIWizardAnalogRename : GUIWindow
  {
    [SkinControlAttribute(24)]
    protected GUIListControl listChannelsFound = null;
    [SkinControlAttribute(5)]
    protected GUIButtonControl btnNext = null;
    [SkinControlAttribute(25)]
    protected GUIButtonControl btnBack = null;
    TVCaptureDevice captureCard = null;

    public GUIWizardAnalogRename()
    {

      GetID = (int)GUIWindow.Window.WINDOW_WIZARD_ANALOG_RENAME;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\wizard_tvcard_analog_rename.xml");
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();

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
          TVChannel chan = (TVChannel)GUIWizardAnalogTune.TVChannelsFound[0];
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

    void UpdateList()
    {
      int selectedItem = listChannelsFound.SelectedListItemIndex;
      listChannelsFound.Clear();
      foreach (TVChannel chan in GUIWizardAnalogTune.TVChannelsFound)
      {
        GUIListItem item = new GUIListItem();
        item.Label = chan.Name;
        item.IsFolder = false;
        string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, chan.Name);
        if (!System.IO.File.Exists(strLogo))
        {
          strLogo = "defaultVideoBig.png";
        }
        item.ThumbnailImage = strLogo;
        item.IconImage = strLogo;
        item.IconImageBig = strLogo;
        item.MusicTag = chan;
        item.OnItemSelected += new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(item_OnItemSelected);
        listChannelsFound.Add(item);
      }
      while (selectedItem > 0 && selectedItem >= GUIWizardAnalogTune.TVChannelsFound.Count)
        selectedItem--;
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
    protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
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
    void OnNextPage()
    {
      ArrayList listChannels = new ArrayList();
      TVDatabase.GetChannels(ref listChannels);
      foreach (TVChannel ch in GUIWizardAnalogTune.TVChannelsFound)
      {
        bool found = false;
        foreach (TVChannel listChan in listChannels)
        {
          if (String.Compare(listChan.Name, ch.Name, true) == 0)
          {
            listChan.Number = ch.Number;
            TVDatabase.UpdateChannel(listChan, listChan.Sort);
            if (captureCard != null)
              TVDatabase.MapChannelToCard(listChan.ID, captureCard.ID);
            found = true;
          }
        }
        if (!found)
        {
          TVDatabase.AddChannel(ch);
          if (captureCard != null)
            TVDatabase.MapChannelToCard(ch.ID, captureCard.ID);
        }
      }
      if (captureCard != null)
      {
        MapTvToOtherCards(captureCard.ID);
      }
      GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_WIZARD_ANALOG_SCAN_RADIO);
    }

    protected override void OnShowContextMenu()
    {
      GUIListItem item = listChannelsFound.SelectedListItem;
      if (item == null) return;
      TVChannel chan = (TVChannel)item.MusicTag;
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      if (dlg != null)
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
          foreach (TVChannel ch in GUIWizardAnalogTune.TVChannelsFound)
          {
            if (ch.Number == chan.Number)
            {
              GUIWizardAnalogTune.TVChannelsFound.Remove(ch);
              break;
            }
          }
          UpdateList();
          break;

        case 118://rename
          VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
          if (null == keyboard) return;
          keyboard.Reset();
          keyboard.Text = chan.Name;
          keyboard.DoModal(GetID);
          if (keyboard.IsConfirmed)
          {
            chan.Name = keyboard.Text;
            foreach (TVChannel ch in GUIWizardAnalogTune.TVChannelsFound)
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
      TVChannel chan = (TVChannel)item.MusicTag;
      if (captureCard != null)
      {
        captureCard.StartViewing(chan.Name);
        captureCard.Tune(chan);

      }
    }
    void MapTvToOtherCards(int id)
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
