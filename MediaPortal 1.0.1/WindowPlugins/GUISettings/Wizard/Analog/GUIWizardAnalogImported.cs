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
using System.Xml;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Settings.Wizard;
using MediaPortal.Radio.Database;
using MediaPortal.TV.Database;
using MediaPortal.TV.Recording;
using MediaPortal.Util;

namespace WindowPlugins.GUISettings.Wizard.Analog
{
  /// <summary>
  /// Summary description for GUIWizardAnalogImported.
  /// </summary>
  public class GUIWizardAnalogImported : GUIWindow
  {
    [SkinControl(26)] protected GUILabelControl lblChannelsFound = null;
    [SkinControl(27)] protected GUILabelControl lblStatus = null;
    [SkinControl(24)] protected GUIListControl listChannelsFound = null;
    [SkinControl(5)] protected GUIButtonControl btnNext = null;

    public GUIWizardAnalogImported()
    {
      GetID = (int) Window.WINDOW_WIZARD_ANALOG_IMPORTED;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\wizard_tvcard_analog_imported.xml");
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();
      string url = GUIPropertyManager.GetProperty("#WizardCityUrl");
      ImportAnalogChannels(url);
      UpdateList();
      lblStatus.Label = "Press Next to continue the setup";
      GUIPropertyManager.SetProperty("#Wizard.Analog.Done", "yes");
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == btnNext)
      {
        GUIWizardCardsDetected.ScanNextCardType();
        return;
      }
      base.OnClicked(controlId, control, actionType);
    }

    private void UpdateList()
    {
      listChannelsFound.Clear();
      ArrayList channels = new ArrayList();
      TVDatabase.GetChannels(ref channels);
      if (channels.Count == 0)
      {
        GUIListItem item = new GUIListItem();
        item.Label = "No channels found";
        item.IsFolder = false;
        listChannelsFound.Add(item);
        return;
      }
      int count = 1;
      foreach (TVChannel chan in channels)
      {
        GUIListItem item = new GUIListItem();
        item.Label = String.Format("{0}. {1}", count, chan.Name);
        item.IsFolder = false;
        string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, chan.Name);
        if (!File.Exists(strLogo))
        {
          strLogo = "defaultVideoBig.png";
        }
        item.ThumbnailImage = strLogo;
        item.IconImage = strLogo;
        item.IconImageBig = strLogo;
        listChannelsFound.Add(item);
        count++;
      }
      listChannelsFound.ScrollToEnd();
    }

    private void ImportAnalogChannels(string xmlFile)
    {
      XmlDocument doc = new XmlDocument();
      UriBuilder builder = new UriBuilder("http", "mediaportal.sourceforge.net", 80, "tvsetup/analog/" + xmlFile);
      doc.Load(builder.Uri.AbsoluteUri);
      XmlNodeList listTvChannels = doc.DocumentElement.SelectNodes("/mediaportal/tv/channel");
      foreach (XmlNode nodeChannel in listTvChannels)
      {
        XmlNode name = nodeChannel.Attributes.GetNamedItem("name");
        XmlNode number = nodeChannel.Attributes.GetNamedItem("number");
        XmlNode frequency = nodeChannel.Attributes.GetNamedItem("frequency");
        TVChannel chan = new TVChannel();
        chan.Name = name.Value;
        chan.Frequency = 0;
        try
        {
          chan.Number = Int32.Parse(number.Value);
        }
        catch (Exception)
        {
        }
        try
        {
          chan.Frequency = ConvertToTvFrequency(frequency.Value, ref chan);
        }
        catch (Exception)
        {
        }
        TVDatabase.AddChannel(chan);
        MapTvToOtherCards(chan);
      }
      XmlNodeList listRadioChannels = doc.DocumentElement.SelectNodes("/mediaportal/radio/channel");
      foreach (XmlNode nodeChannel in listRadioChannels)
      {
        XmlNode name = nodeChannel.Attributes.GetNamedItem("name");
        XmlNode frequency = nodeChannel.Attributes.GetNamedItem("frequency");
        RadioStation chan = new RadioStation();
        chan.Name = name.Value;
        chan.Frequency = ConvertToFrequency(frequency.Value);
        RadioDatabase.AddStation(ref chan);
        MapRadioToOtherCards(chan);
      }
    }

    private long ConvertToFrequency(string frequency)
    {
      if (frequency.Trim() == string.Empty)
      {
        return 0;
      }
      float testValue = 189.24f;
      string usage = testValue.ToString("f2");
      if (usage.IndexOf(".") >= 0)
      {
        frequency = frequency.Replace(",", ".");
      }
      if (usage.IndexOf(",") >= 0)
      {
        frequency = frequency.Replace(".", ",");
      }
      double freqValue = Convert.ToDouble(frequency);
      freqValue *= 1000000;
      return (long) (freqValue);
    }


    private long ConvertToTvFrequency(string frequency, ref TVChannel chan)
    {
      if (frequency.Trim() == string.Empty)
      {
        return 0;
      }
      chan.Number = TVDatabase.FindFreeTvChannelNumber(chan.Number);
      frequency = frequency.ToUpper();
      for (int i = 0; i < TVChannel.SpecialChannels.Length; ++i)
      {
        if (frequency.Equals(TVChannel.SpecialChannels[i].Name))
        {
          return TVChannel.SpecialChannels[i].Frequency;
        }
      }

      float testValue = 189.24f;
      string usage = testValue.ToString("f2");
      if (usage.IndexOf(".") >= 0)
      {
        frequency = frequency.Replace(",", ".");
      }
      if (usage.IndexOf(",") >= 0)
      {
        frequency = frequency.Replace(".", ",");
      }
      double freqValue = Convert.ToDouble(frequency);
      freqValue *= 1000000;
      return (long) (freqValue);
    }

    private void MapTvToOtherCards(TVChannel chan)
    {
      for (int i = 0; i < Recorder.Count; ++i)
      {
        TVCaptureDevice dev = Recorder.Get(i);
        if (dev.Network == NetworkType.Analog)
        {
          TVDatabase.MapChannelToCard(chan.ID, dev.ID);
        }
      }
    }

    private void MapRadioToOtherCards(RadioStation chan)
    {
      for (int i = 0; i < Recorder.Count; ++i)
      {
        TVCaptureDevice dev = Recorder.Get(i);

        if (dev.Network == NetworkType.Analog)
        {
          RadioDatabase.MapChannelToCard(chan.ID, dev.ID);
        }
      }
    }
  }
}