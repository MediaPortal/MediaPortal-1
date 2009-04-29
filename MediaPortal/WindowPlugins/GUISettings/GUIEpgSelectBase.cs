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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.EPG.config;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.TV.Database;
using MediaPortal.Util;
using MediaPortal.WebEPG.Config.Grabber;

namespace WindowPlugins.GUISettings.Epg
{
  /// <summary>
  /// Summary description foGUIWizardEpgSelectr GUIEpgSelectBase.
  /// </summary>
  public class GUIEpgSelectBase : GUIWindow, IComparer<GUIListItem>
  {
    [SkinControl(24)] protected GUIListControl listGrabbers = null;
    [SkinControl(2)] protected GUILabelControl lblLine1 = null;
    [SkinControl(3)] protected GUILabelControl lblLine2 = null;
    [SkinControl(27)] protected GUIButtonControl btnManual = null;

    protected bool epgGrabberSelected = false;
    protected ChannelsList _channelList;
    protected List<ChannelGrabberInfo> _epgChannels;


    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      LoadGrabbers();
    }

    protected void LoadGrabbers()
    {
      lblLine1.Label = GUILocalizeStrings.Get(200000);
      lblLine2.Label = GUILocalizeStrings.Get(200001);
      epgGrabberSelected = false;
      listGrabbers.Clear();

      _channelList = new ChannelsList(@"webepg");

      string[] country = _channelList.GetCountries();

      string localCountry = GUIPropertyManager.GetProperty("#WizardCountryCode");

      for (int i = 0; i < country.Length; i++)
      {
        try
        {
          RegionInfo rInfo = new RegionInfo(country[i]);
          GUIListItem item = new GUIListItem();
          item.Label = rInfo.DisplayName;
          item.Path = country[i];
          if (localCountry == country[i])
          {
            OnChangeGrabber(item);
            return;
          }
          listGrabbers.Add(item);
        }
        catch (Exception)
        {
        }
      }

      listGrabbers.Sort(this);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == listGrabbers)
      {
        if (!epgGrabberSelected)
        {
          OnChangeGrabber(listGrabbers.SelectedListItem);
        }
        else
        {
          OnMap();
        }
      }
      if (control == btnManual)
      {
        OnManual();
      }
      base.OnClicked(controlId, control, actionType);
    }


    protected void OnChangeGrabber(GUIListItem item)
    {
      if (item == null)
      {
        return;
      }

      // Channel List
      _epgChannels = _channelList.GetChannelArrayList(item.Path);
      string country = GUIPropertyManager.GetProperty("#WizardCountryCode");

      ShowChannelMappingList(country);
      epgGrabberSelected = true;
    }

    protected void ShowChannelMappingList(string country)
    {
      EPGConfig config = new EPGConfig("webepg");
      config.Load();
      ArrayList list = config.GetAll();

      listGrabbers.Clear();
      epgGrabberSelected = true;
      ArrayList channels = new ArrayList();
      TVDatabase.GetChannels(ref channels);

      GUIListItem ch; // = new GUIListItem();
      //channels.Sort (this);
      foreach (TVChannel chan in channels)
      {
        ch = new GUIListItem();
        ch.Label = chan.Name;
        ch.Path = "";

        int chID = _channelList.FindChannel(chan.Name, country);
        if (chID != -1)
        {
          ChannelGrabberInfo chInfo = _epgChannels[chID];
          ch.Label2 = chInfo.FullName;
          ch.Path = chInfo.ChannelID;
          ch.ItemId = chID;
        }
        else
        {
          foreach (EPGConfigData data in list)
          {
            if (String.Compare(data.DisplayName, ch.Label, true) == 0)
            {
              for (int i = 0; i < _epgChannels.Count; ++i)
              {
                ChannelGrabberInfo info = (ChannelGrabberInfo) _epgChannels[i];
                if (info.ChannelID == data.ChannelID)
                {
                  ch.ItemId = i;
                  ch.Path = info.ChannelID;
                  ch.Label2 = info.FullName;
                  break;
                }
              }
              break;
            }
          }
        }

        listGrabbers.Add(ch);
      }
      listGrabbers.Sort(this);

      //setup and import epg...
      lblLine1.Label = GUILocalizeStrings.Get(200002);
      lblLine2.Label = GUILocalizeStrings.Get(200003);
    }


    protected void MapChannels()
    {
      if (epgGrabberSelected == false)
      {
        return;
      }

      EPGConfig config = new EPGConfig(@"webepg");


      config.MaxGrab = 7;

      for (int i = 0; i < listGrabbers.Count; ++i)
      {
        try
        {
          GUIListItem item = listGrabbers[i];
          if (item.Label.Length > 0 && item.Label2.Length > 0)
          {
            EPGConfigData data = new EPGConfigData();
            data.DisplayName = item.Label;
            data.ChannelID = item.Path;
            ChannelGrabberInfo selChannel = _epgChannels[item.ItemId];
            GrabberInfo gInfo = selChannel.GrabberList[0];
            data.PrimaryGrabberID = gInfo.GrabberID;
            config.Add(data);
          }
        }
        catch (Exception ex)
        {
          //MessageBox.Show("Your mapping is invalid! Error code: " + i );
          Log.Error("GUIWizard: Invalid mapping!", ex);
        }
      }
      config.Save();
    }

    protected void OnMap()
    {
      GUIListItem item = listGrabbers.SelectedListItem;
      if (item == null)
      {
        return;
      }

      GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
      dlg.Reset();
      dlg.SetHeading(GUILocalizeStrings.Get(924)); //Menu
      dlg.ShowQuickNumbers = false;

      dlg.Add("Delete");

      int selected = 0;
      int count = 0;
      foreach (ChannelGrabberInfo chan in _epgChannels)
      {
        dlg.Add(chan.FullName);
        if (chan.FullName == item.Label2)
        {
          selected = count;
        }
        count++;
      }

      dlg.SelectedLabel = selected;
      dlg.ShowQuickNumbers = false;
      dlg.DoModal(GetID);
      if (dlg.SelectedLabel < 0 || dlg.SelectedLabel >= _epgChannels.Count + 1)
      {
        return;
      }
      if (dlg.SelectedLabel == 0)
      {
        item.Label2 = "";
        item.Path = "";
      }
      else
      {
        ChannelGrabberInfo selChannel = (ChannelGrabberInfo) _epgChannels[dlg.SelectedLabel - 1];
        item.Label2 = selChannel.FullName;
        item.Path = selChannel.ChannelID;
        item.ItemId = dlg.SelectedLabel - 1;
      }
    }

    protected void ShowError(string line1, string line2)
    {
      GUIDialogOK pDlgOK = (GUIDialogOK) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_OK);
      pDlgOK.SetHeading(608);
      pDlgOK.SetLine(1, line1);
      pDlgOK.SetLine(2, line2);
      pDlgOK.DoModal(GetID);
      return;
    }


    protected void OnManual()
    {
      string _strTVGuideFile;
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        _strTVGuideFile = xmlreader.GetValueAsString("xmltv", "folder", "xmltv");
        _strTVGuideFile = Utils.RemoveTrailingSlash(_strTVGuideFile);
        _strTVGuideFile += @"\tvguide.xml";
      }
      if (!File.Exists(_strTVGuideFile))
      {
        ShowError("Unable to open tvguide.xml from", _strTVGuideFile);
        LoadGrabbers();
        return;
      }
      //load tvguide.xml
      XmlDocument xml = new XmlDocument();
      xml.Load(_strTVGuideFile);
      if (xml.DocumentElement == null)
      {
        ShowError("Unable to open tvguide.xml from", _strTVGuideFile);
        LoadGrabbers();
        return;
      }
      XmlNodeList channelList = xml.DocumentElement.SelectNodes("/tv/channel");
      if (channelList == null)
      {
        ShowError("Invalid xmltv file", "no tv channels found");
        LoadGrabbers();
        return;
      }
      if (channelList.Count == 0)
      {
        ShowError("Invalid xmltv file", "no tv channels found");
        LoadGrabbers();
        return;
      }
      foreach (XmlNode nodeChannel in channelList)
      {
        if (nodeChannel.Attributes != null)
        {
          XmlNode nodeId = nodeChannel.Attributes.GetNamedItem("id");
          XmlNode nodeName = nodeChannel.SelectSingleNode("display-name");
          if (nodeName == null)
          {
            nodeName = nodeChannel.SelectSingleNode("Display-Name");
          }
          if (nodeName != null && nodeName.InnerText != null)
          {
            GUIListItem ch = new GUIListItem();
            ch.Label = nodeName.InnerText;
            ch.Path = nodeId.InnerText;
            int idChannel;
            string strTvChannel;
            if (TVDatabase.GetEPGMapping(ch.Path, out idChannel, out strTvChannel))
            {
              ch.Label2 = strTvChannel;
              ch.ItemId = idChannel;
            }
            listGrabbers.Add(ch);
          }
        }
      }
    }

    #region IComparer Members

    public int Compare(GUIListItem item1, GUIListItem item2)
    {
      //			TVChannel ch1=(TVChannel)x;
      //			TVChannel ch2=(TVChannel)y;
      //			return String.Compare(ch1.Name,ch2.Name,true);
      return String.Compare(item1.Label, item2.Label, true);
    }

    #endregion
  }
}