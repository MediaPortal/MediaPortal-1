/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Xml;
using System.Net;
using DirectShowLib;


using TvDatabase;
using TvControl;
using TvLibrary;
using TvLibrary.Log;
using TvLibrary.Channels;
using TvLibrary.Interfaces;
using TvLibrary.Implementations;
using DirectShowLib.BDA;

namespace SetupTv.Sections
{
  public partial class CardDvbS : SectionSettings
  {
    #region private classes
    class SatteliteContext : IComparable<SatteliteContext>
    {
      public string SatteliteName;
      public string Url;
      public string FileName;
      public Satellite Satelite;
      public SatteliteContext()
      {
        Url = "";
        Satelite = null;
        FileName = "";
        SatteliteName = "";
      }
      public override string ToString()
      {
        return SatteliteName;
      }
      public int CompareTo(SatteliteContext other)
      {
        return SatteliteName.CompareTo(other.SatteliteName);
      }


      #region IComparable<SatteliteContext> Members

      int IComparable<SatteliteContext>.CompareTo(SatteliteContext other)
      {
        return SatteliteName.CompareTo(other.SatteliteName);
      }

      #endregion
    }

    class Transponder : IComparable<Transponder>
    {
      public int CarrierFrequency; // frequency
      public Polarisation Polarisation;  // polarisation 0=hori, 1=vert
      public int SymbolRate; // symbol rate
      public ModulationType Modulation = ModulationType.ModNotSet;
      public BinaryConvolutionCodeRate InnerFecRate = BinaryConvolutionCodeRate.RateNotSet;
      //public Pilot Pilot = Pilot.NotSet;
      //public Rolloff RollOff = Rolloff.NotSet;

      public int CompareTo(Transponder other)
      {
        if (Polarisation < other.Polarisation) return 1;
        if (Polarisation > other.Polarisation) return -1;
        if (CarrierFrequency > other.CarrierFrequency) return 1;
        if (CarrierFrequency < other.CarrierFrequency) return -1;
        if (SymbolRate > other.SymbolRate) return 1;
        if (SymbolRate < other.SymbolRate) return -1;
        return 0;
      }
      public override string ToString()
      {
        return String.Format("{0} {1} {2} {3} {4}", CarrierFrequency, SymbolRate, Polarisation, Modulation, InnerFecRate);
      }
    }
    #endregion

    #region variables
    int _cardNumber;
    List<Transponder> _transponders = new List<Transponder>();
    int _channelCount = 0;

    int _tvChannelsNew = 0;
    int _radioChannelsNew = 0;
    int _tvChannelsUpdated = 0;
    int _radioChannelsUpdated = 0;
    bool _isScanning = false;
    bool _stopScanning = false;
    bool _enableEvents = false;
    bool _ignoreCheckBoxCreateGroupsClickEvent = false;
    #endregion

    #region ctors
    public CardDvbS()
      : this("DVBC")
    {
    }
    public CardDvbS(string name)
      : base(name)
    {
    }

    public CardDvbS(string name, int cardNumber)
      : base(name)
    {
      _cardNumber = cardNumber;
      InitializeComponent();
      base.Text = name;
      Init();
    }
    #endregion

    #region helper methods
    void DownloadTransponder(SatteliteContext context)
    {
      if (context.Url == null) return;
      if (context.Url.Length == 0) return;
      if (!context.Url.ToLower().StartsWith("http://")) return;
      string itemLine = String.Format("Downloading transponders for:{0}", context.SatteliteName);
      ListViewItem item = listViewStatus.Items.Add(new ListViewItem(itemLine));
      item.EnsureVisible();
      Application.DoEvents();
      try
      {
        try
        {
          System.IO.File.Delete(context.FileName);
        }
        catch (Exception)
        {
        }
        item.Text = itemLine + " connecting...";
        Application.DoEvents();
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(context.Url);
        item.Text = itemLine + " downloading...";
        Application.DoEvents();
        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        {
          item.Text = itemLine + " saving...";
          Application.DoEvents();
          using (Stream resStream = response.GetResponseStream())
          {
            using (System.IO.TextReader tin = new StreamReader(resStream))
            {
              using (System.IO.TextWriter tout = System.IO.File.CreateText(context.FileName))
              {
                while (true)
                {
                  string line = tin.ReadLine();
                  if (line == null) break;
                  tout.WriteLine(line);
                }
              }
            }
          }
        }
        item.Text = itemLine + " done";
      }
      catch (Exception)
      {
        item.Text = itemLine + " failed";
      }
      Application.DoEvents();
    }
    void LoadTransponders(SatteliteContext context)
    {
      if (!System.IO.File.Exists(context.FileName))
      {
        DownloadTransponder(context);
      }
      _transponders.Clear();
      _channelCount = 0;
      string line;
      string[] tpdata;
      // load transponder list and start scan
      System.IO.TextReader tin = System.IO.File.OpenText(context.FileName);
      int _count = 0;
      do
      {
        line = null;
        line = tin.ReadLine();
        if (line != null)
        {
          line = line.Trim();
          if (line.Length > 0)
          {
            if (line.StartsWith(";"))
              continue;
            tpdata = line.Split(new char[] { ',' });
            if (tpdata.Length >= 3)
            {
              if (tpdata[0].IndexOf("=") >= 0)
              {
                tpdata[0] = tpdata[0].Substring(tpdata[0].IndexOf("=") + 1);
              }
              try
              {

                Transponder transponder = new Transponder();
                transponder.CarrierFrequency = Int32.Parse(tpdata[0]) * 1000;
                switch (tpdata[1].ToLower())
                {
                  case "v":
                    transponder.Polarisation = Polarisation.LinearV;
                    break;
                  case "h":
                    transponder.Polarisation = Polarisation.LinearH;
                    break;
                  case "r":
                    transponder.Polarisation = Polarisation.CircularR;
                    break;
                  case "l":
                    transponder.Polarisation = Polarisation.CircularL;
                    break;
                  default:
                    transponder.Polarisation = Polarisation.LinearH;
                    break;
                }
                transponder.SymbolRate = Int32.Parse(tpdata[2]);
                if (tpdata.Length >= 4)
                {
                  tpdata[3] = tpdata[3].ToLower();
                  if (tpdata[3] == "8psk") transponder.Modulation = ModulationType.Mod8psk; //not supported by BDA yet...
                  if (tpdata[3] == "qpsk") transponder.Modulation = ModulationType.ModQpsk; //not supported by BDA yet...
                  if (tpdata[3] == "16apsk") transponder.Modulation = ModulationType.Mod16Apsk; //not supported by BDA yet...
                  if (tpdata[3] == "32apsk") transponder.Modulation = ModulationType.Mod32Apsk; //not supported by BDA yet...

                  if (tpdata.Length >= 5)
                  {
                    tpdata[4] = tpdata[4].ToLower();
                    if (tpdata[4] == "1/2") transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate1_2;
                    if (tpdata[4] == "2/3") transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate2_3;
                    if (tpdata[4] == "3/4") transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate3_4;
                    if (tpdata[4] == "3/5") transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate3_5;
                    if (tpdata[4] == "4/5") transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate4_5;
                    if (tpdata[4] == "5/11") transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate5_11;
                    if (tpdata[4] == "5/6") transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate5_6;
                    if (tpdata[4] == "7/8") transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate7_8;
                    //if (tpdata[4] == "9/10") transponder.InnerFecRate = BinaryConvolutionCodeRate.RateNotDefined;
                    if (tpdata[4] == "1/4") transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate1_4; //DVB-S2 For Hauppauge (Not in the BDA Network Provider)
                    if (tpdata[4] == "1/3") transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate1_3; //DVB-S2 For Hauppauge (Not in the BDA Network Provider)
                    if (tpdata[4] == "2/5") transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate2_5; //DVB-S2 For Hauppauge (Not in the BDA Network Provider)
                    if (tpdata[4] == "6/7") transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate6_7; //DVB-S2 For Hauppauge (Not in the BDA Network Provider)
                    if (tpdata[4] == "8/9") transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate8_9; //DVB-S2 For Hauppauge (Not in the BDA Network Provider)
                    if (tpdata[4] == "9/10") transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate9_10; //DVB-S2 For Hauppauge (Not in the BDA Network Provider)
                    //Optional RateMax;
                  }
                }
                _transponders.Add(transponder);
                _count += 1;
              }
              catch
              { }
            }
          }
        }
      } while (!(line == null));
      tin.Close();
      _channelCount = _count;
      _transponders.Sort();
    }
    List<SatteliteContext> LoadSattelites()
    {
      List<SatteliteContext> satellites = new List<SatteliteContext>();
      XmlDocument doc = new XmlDocument();
      doc.Load(System.IO.Directory.GetCurrentDirectory() + @"\Tuningparameters\satellites.xml");
      XmlNodeList nodes = doc.SelectNodes("/satellites/satellite");
      foreach (XmlNode node in nodes)
      {
        SatteliteContext ts = new SatteliteContext();
        ts.SatteliteName = node.Attributes.GetNamedItem("name").Value;
        ts.Url = node.Attributes.GetNamedItem("url").Value;
        string name = Utils.FilterFileName(ts.SatteliteName);
        ts.FileName = System.IO.Directory.GetCurrentDirectory() + @"\Tuningparameters\" + name + ".ini";

        satellites.Add(ts);
      }

      /*
      string[] files = System.IO.Directory.GetFiles(System.IO.Directory.GetCurrentDirectory() + @"\Tuningparameters", "*.tpl");

      foreach (string file in files)
      {
        string fileName = System.IO.Path.GetFileName(file);
        SatteliteContext ts = LoadSatteliteName(fileName);
        if (ts != null)
        {
          satellites.Add(ts);
        }
      }*/

      //satellites.Sort();
      IList dbSats = Satellite.ListAll();
      foreach (SatteliteContext ts in satellites)
      {
        foreach (Satellite dbSat in dbSats)
        {
          string name = "";
          for (int i = 0; i < ts.SatteliteName.Length; ++i)
          {
            if (ts.SatteliteName[i] >= (char)32 && ts.SatteliteName[i] < (char)127)
              name += ts.SatteliteName[i];
          }
          if (String.Compare(name, dbSat.SatelliteName, true) == 0)
          {
            ts.Satelite = dbSat;
            break;
          }
        }
        if (ts.Satelite == null)
        {
          string name = "";
          for (int i = 0; i < ts.SatteliteName.Length; ++i)
          {
            if (ts.SatteliteName[i] >= (char)32 && ts.SatteliteName[i] < (char)127)
              name += ts.SatteliteName[i];
          }
          ts.Satelite = new Satellite(name, ts.FileName);
          ts.Satelite.Persist();
        }
      }
      return satellites;
    }
    #endregion

    #region DVB-S scanning tab
    void Init()
    {
      _enableEvents = false;
      mpTransponder1.Items.Clear();
      mpTransponder2.Items.Clear();
      mpTransponder3.Items.Clear();
      mpTransponder4.Items.Clear();
      List<SatteliteContext> satellites = LoadSattelites();

      foreach (SatteliteContext ts in satellites)
      {
        mpTransponder1.Items.Add(ts);
        mpTransponder2.Items.Add(ts);
        mpTransponder3.Items.Add(ts);
        mpTransponder4.Items.Add(ts);
      }
      if (mpTransponder1.Items.Count > 0)
        mpTransponder1.SelectedIndex = 0;
      if (mpTransponder2.Items.Count > 0)
        mpTransponder2.SelectedIndex = 0;
      if (mpTransponder3.Items.Count > 0)
        mpTransponder3.SelectedIndex = 0;
      if (mpTransponder4.Items.Count > 0)
        mpTransponder4.SelectedIndex = 0;

      mpDisEqc1.Items.Clear();
      mpDisEqc1.Items.Add(DisEqcType.None);
      mpDisEqc1.Items.Add(DisEqcType.SimpleA);
      mpDisEqc1.Items.Add(DisEqcType.SimpleB);
      mpDisEqc1.Items.Add(DisEqcType.Level1AA);
      mpDisEqc1.Items.Add(DisEqcType.Level1AB);
      mpDisEqc1.Items.Add(DisEqcType.Level1BA);
      mpDisEqc1.Items.Add(DisEqcType.Level1BB);
      mpDisEqc1.SelectedIndex = 0;

      mpDisEqc2.Items.Clear();
      mpDisEqc2.Items.Add(DisEqcType.None);
      mpDisEqc2.Items.Add(DisEqcType.SimpleA);
      mpDisEqc2.Items.Add(DisEqcType.SimpleB);
      mpDisEqc2.Items.Add(DisEqcType.Level1AA);
      mpDisEqc2.Items.Add(DisEqcType.Level1AB);
      mpDisEqc2.Items.Add(DisEqcType.Level1BA);
      mpDisEqc2.Items.Add(DisEqcType.Level1BB);
      mpDisEqc2.SelectedIndex = 0;

      mpDisEqc3.Items.Clear();
      mpDisEqc3.Items.Add(DisEqcType.None);
      mpDisEqc3.Items.Add(DisEqcType.SimpleA);
      mpDisEqc3.Items.Add(DisEqcType.SimpleB);
      mpDisEqc3.Items.Add(DisEqcType.Level1AA);
      mpDisEqc3.Items.Add(DisEqcType.Level1AB);
      mpDisEqc3.Items.Add(DisEqcType.Level1BA);
      mpDisEqc3.Items.Add(DisEqcType.Level1BB);
      mpDisEqc3.SelectedIndex = 0;

      mpDisEqc4.Items.Clear();
      mpDisEqc4.Items.Add(DisEqcType.None);
      mpDisEqc4.Items.Add(DisEqcType.SimpleA);
      mpDisEqc4.Items.Add(DisEqcType.SimpleB);
      mpDisEqc4.Items.Add(DisEqcType.Level1AA);
      mpDisEqc4.Items.Add(DisEqcType.Level1AB);
      mpDisEqc4.Items.Add(DisEqcType.Level1BA);
      mpDisEqc4.Items.Add(DisEqcType.Level1BB);
      mpDisEqc4.SelectedIndex = 0;

      comboBoxRollOff.Items.Clear();
      comboBoxRollOff.Items.Add(Rolloff.NotDefined);
      comboBoxRollOff.Items.Add(Rolloff.NotSet);
      comboBoxRollOff.Items.Add(Rolloff.RollOff_20);
      comboBoxRollOff.Items.Add(Rolloff.RollOff_25);
      comboBoxRollOff.Items.Add(Rolloff.RollOff_35);
      comboBoxRollOff.Items.Add(Rolloff.RollOffMax);
      comboBoxRollOff.SelectedIndex = 4;

      TvBusinessLayer layer = new TvBusinessLayer();
      mpTransponder1.SelectedIndex = Int32.Parse(layer.GetSetting("dvbs" + _cardNumber.ToString() + "SatteliteContext1", "0").Value);
      mpTransponder2.SelectedIndex = Int32.Parse(layer.GetSetting("dvbs" + _cardNumber.ToString() + "SatteliteContext2", "0").Value);
      mpTransponder3.SelectedIndex = Int32.Parse(layer.GetSetting("dvbs" + _cardNumber.ToString() + "SatteliteContext3", "0").Value);
      mpTransponder4.SelectedIndex = Int32.Parse(layer.GetSetting("dvbs" + _cardNumber.ToString() + "SatteliteContext4", "0").Value);

      mpDisEqc1.SelectedIndex = Int32.Parse(layer.GetSetting("dvbs" + _cardNumber.ToString() + "DisEqc1", "0").Value);
      mpDisEqc2.SelectedIndex = Int32.Parse(layer.GetSetting("dvbs" + _cardNumber.ToString() + "DisEqc2", "0").Value);
      mpDisEqc3.SelectedIndex = Int32.Parse(layer.GetSetting("dvbs" + _cardNumber.ToString() + "DisEqc3", "0").Value);
      mpDisEqc4.SelectedIndex = Int32.Parse(layer.GetSetting("dvbs" + _cardNumber.ToString() + "DisEqc4", "0").Value);

      mpBand1.SelectedIndex = Int32.Parse(layer.GetSetting("dvbs" + _cardNumber.ToString() + "band1", "0").Value);
      mpBand2.SelectedIndex = Int32.Parse(layer.GetSetting("dvbs" + _cardNumber.ToString() + "band2", "0").Value);
      mpBand3.SelectedIndex = Int32.Parse(layer.GetSetting("dvbs" + _cardNumber.ToString() + "band3", "0").Value);
      mpBand4.SelectedIndex = Int32.Parse(layer.GetSetting("dvbs" + _cardNumber.ToString() + "band4", "0").Value);

      comboBoxRollOff.SelectedIndex = Int32.Parse(layer.GetSetting("dvbs" + _cardNumber.ToString() + "rollOff", "4").Value);

      mpLNB1.Checked = (layer.GetSetting("dvbs" + _cardNumber.ToString() + "LNB1", "false").Value == "true");
      mpLNB2.Checked = (layer.GetSetting("dvbs" + _cardNumber.ToString() + "LNB2", "false").Value == "true");
      mpLNB3.Checked = (layer.GetSetting("dvbs" + _cardNumber.ToString() + "LNB3", "false").Value == "true");
      mpLNB4.Checked = (layer.GetSetting("dvbs" + _cardNumber.ToString() + "LNB4", "false").Value == "true");
      mpLNB1_CheckedChanged(null, null); ;
      mpLNB2_CheckedChanged(null, null); ;
      mpLNB3_CheckedChanged(null, null); ;
      mpLNB4_CheckedChanged(null, null); ;

      checkBox2.Checked = (layer.GetSetting("lnbDefault", "true").Value != "true");
      textBoxLNBLo.Text = layer.GetSetting("LnbLowFrequency", "0").Value;
      textBoxLNBHi.Text = layer.GetSetting("LnbHighFrequency", "0").Value;
      textBoxLNBSwitch.Text = layer.GetSetting("LnbSwitchFrequency", "0").Value;

      checkBoxCreateGroups.Checked = (layer.GetSetting("dvbs" + _cardNumber.ToString() + "creategroups", "false").Value == "true");
      if (!checkBoxCreateGroups.Checked)
      {
        checkBoxCreateGroupsSat.Checked = (layer.GetSetting("dvbs" + _cardNumber.ToString() + "creategroupssat", "false").Value == "true");
      }

      checkEnableDVBS2.Checked = (layer.GetSetting("dvbs" + _cardNumber.ToString() + "enabledvbs2", "false").Value == "true");
      if (!checkEnableDVBS2.Checked)
      {
        checkEnableDVBS2.Checked = (layer.GetSetting("dvbs" + _cardNumber.ToString() + "enabledvbs2", "false").Value == "false");
      }
      
      checkBoxPilot.Checked = (layer.GetSetting("dvbs" + _cardNumber.ToString() + "pilot", "false").Value == "true");
      if (!checkBoxPilot.Checked)
      {
        checkBoxPilot.Checked = (layer.GetSetting("dvbs" + _cardNumber.ToString() + "pilot", "false").Value == "false");
      }

      Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
      _enableEvents = true;
      mpLNB1_CheckedChanged(null, null); ;
    }

    public override void OnSectionDeActivated()
    {
      timer1.Enabled = false;
      base.OnSectionDeActivated();
    }

    void SaveSettings()
    {
      Setting setting;
      TvBusinessLayer layer = new TvBusinessLayer();
      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "creategroups", "false");
      setting.Value = checkBoxCreateGroups.Checked ? "true" : "false";
      setting.Persist();

      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "creategroupssat", "false");
      setting.Value = checkBoxCreateGroupsSat.Checked ? "true" : "false";
      setting.Persist();

      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "SatteliteContext1", "0");
      setting.Value = mpTransponder1.SelectedIndex.ToString();
      setting.Persist();
      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "SatteliteContext2", "0");
      setting.Value = mpTransponder2.SelectedIndex.ToString();
      setting.Persist();
      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "SatteliteContext3", "0");
      setting.Value = mpTransponder3.SelectedIndex.ToString();
      setting.Persist();
      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "SatteliteContext4", "0");
      setting.Value = mpTransponder4.SelectedIndex.ToString();
      setting.Persist();

      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "DisEqc1", "0");
      setting.Value = mpDisEqc1.SelectedIndex.ToString();
      setting.Persist();
      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "DisEqc2", "0");
      setting.Value = mpDisEqc2.SelectedIndex.ToString();
      setting.Persist();
      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "DisEqc3", "0");
      setting.Value = mpDisEqc3.SelectedIndex.ToString();
      setting.Persist();
      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "DisEqc4", "0");
      setting.Value = mpDisEqc4.SelectedIndex.ToString();
      setting.Persist();

      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "band1", "0");
      setting.Value = mpBand1.SelectedIndex.ToString();
      setting.Persist();
      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "band2", "0");
      setting.Value = mpBand2.SelectedIndex.ToString();
      setting.Persist();
      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "band3", "0");
      setting.Value = mpBand3.SelectedIndex.ToString();
      setting.Persist();
      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "band4", "0");
      setting.Value = mpBand4.SelectedIndex.ToString();
      setting.Persist();

      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "LNB1", "false");
      setting.Value = mpLNB1.Checked ? "true" : "false";
      setting.Persist();

      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "LNB2", "false");
      setting.Value = mpLNB2.Checked ? "true" : "false";
      setting.Persist();
      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "LNB3", "false");
      setting.Value = mpLNB3.Checked ? "true" : "false";
      setting.Persist();
      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "LNB4", "false");
      setting.Value = mpLNB4.Checked ? "true" : "false";
      setting.Persist();

      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "enabledvbs2", "false");
      setting.Value = checkEnableDVBS2.Checked ? "true" : "false";
      setting.Persist();

      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "pilot", "false");
      setting.Value = checkBoxPilot.Checked ? "true" : "false";
      setting.Persist();

      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "rollOff", "4");
      setting.Value = comboBoxRollOff.SelectedIndex.ToString();
      setting.Persist();

      bool restart = false;
      setting = layer.GetSetting("lnbDefault", "true");
      if (setting.Value != (checkBox2.Checked ? "false" : "true")) restart = true;
      setting.Value = checkBox2.Checked ? "false" : "true";
      setting.Persist();

      setting = layer.GetSetting("LnbLowFrequency", "0");
      if (setting.Value != textBoxLNBLo.Text) restart = true;
      setting.Value = textBoxLNBLo.Text;
      setting.Persist();

      setting = layer.GetSetting("LnbHighFrequency", "0");
      if (setting.Value != textBoxLNBHi.Text) restart = true;
      setting.Value = textBoxLNBHi.Text;
      setting.Persist();

      setting = layer.GetSetting("LnbSwitchFrequency", "0");
      if (setting.Value != textBoxLNBSwitch.Text) restart = true;
      setting.Value = textBoxLNBSwitch.Text;
      setting.Persist();
      if (restart)
      {
        RemoteControl.Instance.ClearCache();
        RemoteControl.Instance.Restart();
      }
    }

    void UpdateStatus(int LNB)
    {
      progressBarLevel.Value = Math.Min(100, RemoteControl.Instance.SignalLevel(_cardNumber));
      progressBarQuality.Value = Math.Min(100, RemoteControl.Instance.SignalQuality(_cardNumber));
      progressBarSatLevel.Value = Math.Min(100, RemoteControl.Instance.SignalLevel(_cardNumber));
      progressBarSatQuality.Value = Math.Min(100, RemoteControl.Instance.SignalQuality(_cardNumber));
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      UpdateStatus(1);
      labelCurrentPosition.Text = "";
      tabControl1_SelectedIndexChanged(null, null);
    }

    private void mpButtonScanTv_Click(object sender, EventArgs e)
    {
      if (_isScanning == false)
      {
        SaveSettings();
        TvBusinessLayer layer = new TvBusinessLayer();
        Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
        if (card.Enabled == false)
        {
          MessageBox.Show(this, "Card is disabled, please enable the card before scanning");
          return;
        }
        Thread scanThread = new Thread(new ThreadStart(DoScan));
        scanThread.Start();
      }
      else
      {
        _stopScanning = true;
      }
    }

    void DoScan()
    {
      string buttonText = mpButtonScanTv.Text;
      try
      {
        _isScanning = true;
        _stopScanning = false;
        mpButtonScanTv.Text = "Cancel...";
        RemoteControl.Instance.EpgGrabberEnabled = false;
        mpTransponder1.Enabled = false;
        mpTransponder2.Enabled = false;
        mpTransponder3.Enabled = false;
        mpTransponder4.Enabled = false;
        mpDisEqc1.Enabled = false;
        mpDisEqc2.Enabled = false;
        mpDisEqc3.Enabled = false;
        mpDisEqc4.Enabled = false;
        mpLNB1.Enabled = false;
        mpLNB2.Enabled = false;
        mpLNB3.Enabled = false;
        mpLNB4.Enabled = false;
        mpBand1.Enabled = false;
        mpBand2.Enabled = false;
        mpBand3.Enabled = false;
        mpBand4.Enabled = false;
        checkEnableDVBS2.Enabled = false;
        checkBoxPilot.Enabled = false;
        comboBoxRollOff.Enabled = false;

        listViewStatus.Items.Clear();
        _tvChannelsNew = 0;
        _radioChannelsNew = 0;
        _tvChannelsUpdated = 0;
        _radioChannelsUpdated = 0;

        if (mpLNB1.Checked)
          Scan(1, (BandType)mpBand1.SelectedIndex, (DisEqcType)mpDisEqc1.SelectedIndex, (SatteliteContext)mpTransponder1.SelectedItem);
        if (_stopScanning) return;

        if (mpLNB2.Checked)
          Scan(2, (BandType)mpBand2.SelectedIndex, (DisEqcType)mpDisEqc2.SelectedIndex, (SatteliteContext)mpTransponder2.SelectedItem);
        if (_stopScanning) return;

        if (mpLNB3.Checked)
          Scan(3, (BandType)mpBand3.SelectedIndex, (DisEqcType)mpDisEqc3.SelectedIndex, (SatteliteContext)mpTransponder3.SelectedItem);
        if (_stopScanning) return;

        if (mpLNB4.Checked)
          Scan(4, (BandType)mpBand4.SelectedIndex, (DisEqcType)mpDisEqc4.SelectedIndex, (SatteliteContext)mpTransponder4.SelectedItem);

        ListViewItem item = listViewStatus.Items.Add(new ListViewItem(String.Format("Total radio channels new:{0} updated:{1}", _radioChannelsNew, _radioChannelsUpdated)));
        item = listViewStatus.Items.Add(new ListViewItem(String.Format("Total tv channels new:{0} updated:{1}", _tvChannelsNew, _tvChannelsUpdated)));
        item = listViewStatus.Items.Add(new ListViewItem("Scan done..."));
        item.EnsureVisible();
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      finally
      {
        User user = new User();
        user.CardId = _cardNumber;
        RemoteControl.Instance.StopCard(user);
        RemoteControl.Instance.EpgGrabberEnabled = true;
        mpTransponder1.Enabled = true;
        mpTransponder2.Enabled = true;
        mpTransponder3.Enabled = true;
        mpTransponder4.Enabled = true;
        mpDisEqc1.Enabled = true;
        mpDisEqc2.Enabled = true;
        mpDisEqc3.Enabled = true;
        mpDisEqc4.Enabled = true;
        mpBand1.Enabled = true;
        mpBand2.Enabled = true;
        mpBand3.Enabled = true;
        mpBand4.Enabled = true;
        progressBar1.Value = 100;
        mpLNB1.Enabled = true;
        mpLNB2.Enabled = true;
        mpLNB3.Enabled = true;
        mpLNB4.Enabled = true;
        mpButtonScanTv.Text = buttonText;
        checkBoxPilot.Enabled = true;
        comboBoxRollOff.Enabled = true;
        checkEnableDVBS2.Enabled = true;
        _isScanning = false;
      }
    }

    void Scan(int LNB, BandType bandType, DisEqcType disEqc, SatteliteContext context)
    {
      LoadTransponders(context);
      if (_channelCount == 0) return;

      TvBusinessLayer layer = new TvBusinessLayer();
      Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));

      int position = -1;
      Setting setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "motorEnabled", "no");
      if (setting.Value == "yes")
      {
        foreach (DiSEqCMotor motor in card.ReferringDiSEqCMotor())
        {
          if (motor.IdSatellite == context.Satelite.IdSatellite)
          {
            position = motor.Position;
            break;
          }
        }
      }

      User user = new User();
      user.CardId = _cardNumber;
      for (int index = 0; index < _channelCount; ++index)
      {
        if (_stopScanning) return;
        float percent = ((float)(index)) / _channelCount;
        percent *= 100f;
        if (percent > 100f) percent = 100f;
        progressBar1.Value = (int)percent;

        DVBSChannel tuneChannel = new DVBSChannel();
        tuneChannel.Frequency = _transponders[index].CarrierFrequency;
        tuneChannel.Polarisation = _transponders[index].Polarisation;
        tuneChannel.SymbolRate = _transponders[index].SymbolRate;
        tuneChannel.BandType = bandType;
        tuneChannel.SatelliteIndex = position;
        tuneChannel.ModulationType = _transponders[index].Modulation;
        tuneChannel.InnerFecRate = _transponders[index].InnerFecRate;
        //Grab the Pilot & Roll-off settings
        if (checkEnableDVBS2.Checked)
        {
          setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "pilot", "false");
          if (setting.Value == "true")
            tuneChannel.Pilot = Pilot.PilotOn;
          else
            tuneChannel.Pilot = Pilot.PilotOff;
          setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "rollOff", "4");
          if (setting.Value == "0")
            tuneChannel.RollOff = Rolloff.NotSet;
          if (setting.Value == "1")
            tuneChannel.RollOff = Rolloff.NotDefined;
          if (setting.Value == "2")
            tuneChannel.RollOff = Rolloff.RollOff_20;
          if (setting.Value == "3")
            tuneChannel.RollOff = Rolloff.RollOff_25;
          if (setting.Value == "4")
            tuneChannel.RollOff = Rolloff.RollOff_35;
          if (setting.Value == "5")
            tuneChannel.RollOff = Rolloff.RollOffMax;
        }
        if (!checkEnableDVBS2.Checked)
        {
          tuneChannel.Pilot = Pilot.NotSet;
          tuneChannel.RollOff = Rolloff.NotSet;
        }
        if (bandType == BandType.Circular)
        {
          if (tuneChannel.Polarisation == Polarisation.LinearH)
            tuneChannel.Polarisation = Polarisation.CircularL;
          else if (tuneChannel.Polarisation == Polarisation.LinearV)
            tuneChannel.Polarisation = Polarisation.CircularR;
        }
        tuneChannel.DisEqc = disEqc;
        string line = String.Format("lnb:{0} {1}tp- {2} {3} {4}", LNB, 1 + index, tuneChannel.Frequency, tuneChannel.Polarisation, tuneChannel.SymbolRate);
        ListViewItem item = listViewStatus.Items.Add(new ListViewItem(line));
        item.EnsureVisible();

        if (index == 0)
        {
          RemoteControl.Instance.Tune(ref user, tuneChannel, -1);
        }
        UpdateStatus(LNB);

        IChannel[] channels = RemoteControl.Instance.Scan(_cardNumber, tuneChannel);

        UpdateStatus(LNB);

        if (channels == null || channels.Length == 0)
        {
          if (RemoteControl.Instance.TunerLocked(_cardNumber) == false)
          {
            line = String.Format("lnb:{0} {1}tp- {2} {3} {4}:No signal", LNB, 1 + index, tuneChannel.Frequency, tuneChannel.Polarisation, tuneChannel.SymbolRate);
            item.Text = line;
            item.ForeColor = Color.Red;
            continue;
          }
          else
          {
            line = String.Format("lnb:{0} {1}tp- {2} {3} {4}:Nothing found", LNB, 1 + index, tuneChannel.Frequency, tuneChannel.Polarisation, tuneChannel.SymbolRate);
            item.Text = line;
            item.ForeColor = Color.Red;
            continue;
          }
        }

        int newChannels = 0;
        int updatedChannels = 0;
        bool exists;
        for (int i = 0; i < channels.Length; ++i)
        {
          Channel dbChannel;
          DVBSChannel channel = (DVBSChannel)channels[i];
          TuningDetail currentDetail = layer.GetChannel(channel);
          if (currentDetail == null)
          {
            //add new channel
            exists = false;
            dbChannel = layer.AddNewChannel(channel.Name);
            dbChannel.SortOrder = 10000;
            if (channel.LogicalChannelNumber >= 1)
            {
              dbChannel.SortOrder = channel.LogicalChannelNumber;
            }
          }
          else
          {
            exists = true;
            dbChannel = currentDetail.ReferencedChannel();
          }

          dbChannel.IsTv = channel.IsTv;
          dbChannel.IsRadio = channel.IsRadio;
          dbChannel.FreeToAir = channel.FreeToAir;
          if (dbChannel.IsRadio)
          {
            dbChannel.GrabEpg = false;
          }
          dbChannel.Persist();

          if (checkBoxCreateGroupsSat.Checked)
          {
            layer.AddChannelToGroup(dbChannel, context.Satelite.SatelliteName);
          }
          else if (checkBoxCreateGroups.Checked)
          {
            layer.AddChannelToGroup(dbChannel, channel.Provider);
          }
          if (currentDetail == null)
          {
            channel.SatelliteIndex = position;// context.Satelite.IdSatellite;
            layer.AddTuningDetails(dbChannel, channel);
          }
          else
          {
            //update tuning details...
            channel.SatelliteIndex = position;// context.Satelite.IdSatellite;
            currentDetail.SatIndex = position;//context.Satelite.IdSatellite;
            layer.UpdateTuningDetails(dbChannel, channel, currentDetail);
          }
          if (channel.IsTv)
          {
            if (exists)
            {
              _tvChannelsUpdated++;
              updatedChannels++;
            }
            else
            {
              _tvChannelsNew++;
              newChannels++;
            }
          }
          if (channel.IsRadio)
          {
            if (exists)
            {
              _radioChannelsUpdated++;
              updatedChannels++;
            }
            else
            {
              _radioChannelsNew++;
              newChannels++;
            }
          }
          layer.MapChannelToCard(card, dbChannel);
          line = String.Format("lnb:{0} {1}tp- {2} {3} {4}:New:{5} Updated:{6}",
              LNB, 1 + index, tuneChannel.Frequency, tuneChannel.Polarisation, tuneChannel.SymbolRate, newChannels, updatedChannels);
          item.Text = line;
        }
      }
      // DatabaseManager.Instance.SaveChanges();
    }

    private void CardDvbS_Load(object sender, EventArgs e)
    {

    }

    private void mpComboBoxCam_SelectedIndexChanged(object sender, EventArgs e)
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
      //card.CamType = mpComboBoxCam.SelectedIndex;
      card.Persist();
    }

    #endregion

    #region DiSEqC Motor tab

    void SetupMotor()
    {
      _enableEvents = false;
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "motorEnabled", "no");
      bool enabled = false;
      if (setting.Value == "yes")
      {
        enabled = true;
      }
      checkBox1.Checked = enabled;
      checkBox1_CheckedChanged(null, null);

      comboBoxStepSize.Items.Clear();
      for (int i = 1; i < 127; ++i)
        comboBoxStepSize.Items.Add(i.ToString());

      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "motorStepSize", "10");
      int stepsize = 10;
      if (Int32.TryParse(setting.Value, out stepsize))
        comboBoxStepSize.SelectedIndex = stepsize - 1;
      else
        comboBoxStepSize.SelectedIndex = 9;

      comboBoxSat.Items.Clear();

      setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "selectedMotorSat", "0");
      int index = 0;
      Int32.TryParse(setting.Value, out index);

      List<SatteliteContext> satellites = LoadSattelites();

      foreach (SatteliteContext sat in satellites)
      {
        comboBoxSat.Items.Add(sat);
      }
      if (index >= 0 && index < satellites.Count)
        comboBoxSat.SelectedIndex = index;
      else
        comboBoxSat.SelectedIndex = 0;
      LoadMotorTransponder();
      _enableEvents = true;
    }
    private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (tabControl1.SelectedIndex == 1)
      {
        SetupMotor();
        timer1.Enabled = true;
      }
      else
      {
        timer1.Enabled = false;
      }
    }

    private void buttonMoveWest_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false) return;
      if (checkBox1.Checked == false) return;
      //move motor west
      RemoteControl.Instance.DiSEqCDriveMotor(_cardNumber, DiSEqCDirection.West, (byte)(1 + comboBoxStepSize.SelectedIndex));
    }

    private void buttonSetWestLimit_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false) return;
      if (checkBox1.Checked == false) return;
      //set motor west limit
      RemoteControl.Instance.DiSEqCSetWestLimit(_cardNumber);
    }

    private void tabPage2_Click(object sender, EventArgs e)
    {

    }

    private void button1_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false) return;
      //goto selected sat
      if (comboBoxSat.SelectedIndex < 0) return;
      if (checkBox1.Checked == false) return;
      TvBusinessLayer layer = new TvBusinessLayer();
      SatteliteContext sat = (SatteliteContext)comboBoxSat.Items[comboBoxSat.SelectedIndex];

      Card card = Card.Retrieve(_cardNumber);
      IList motorSettings = card.ReferringDiSEqCMotor();
      foreach (DiSEqCMotor motor in motorSettings)
      {
        if (motor.IdSatellite == sat.Satelite.IdSatellite)
        {
          RemoteControl.Instance.DiSEqCGotoPosition(_cardNumber, (byte)motor.Position);
          MessageBox.Show("Satellite moving to position:" + motor.Position.ToString(), "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
          comboBox1_SelectedIndexChanged(null, null);
          return;
        }
      }
      MessageBox.Show("No position stored for this satellite", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
    }

    private void buttonStore_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false) return;
      if (checkBox1.Checked == false) return;
      //store motor position..
      int index = -1;
      SatteliteContext sat = (SatteliteContext)comboBoxSat.SelectedItem;
      TvBusinessLayer layer = new TvBusinessLayer();
      Card card = Card.Retrieve(_cardNumber);
      IList motorSettings = card.ReferringDiSEqCMotor();
      foreach (DiSEqCMotor motor in motorSettings)
      {
        if (motor.IdSatellite == sat.Satelite.IdSatellite)
        {
          index = motor.Position;
          break;
        }
      }
      if (index < 0)
      {
        index = motorSettings.Count + 1;
        DiSEqCMotor motor = new DiSEqCMotor(card.IdCard, sat.Satelite.IdSatellite, index);
        motor.Persist();
      }
      RemoteControl.Instance.DiSEqCStorePosition(_cardNumber, (byte)(index));
      MessageBox.Show("Satellite position stored to:" + index.ToString(), "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

    }

    private void buttonMoveEast_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false) return;
      if (checkBox1.Checked == false) return;
      //move motor east
      RemoteControl.Instance.DiSEqCDriveMotor(_cardNumber, DiSEqCDirection.East, (byte)(1 + comboBoxStepSize.SelectedIndex));
    }

    private void buttonSetEastLimit_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false) return;
      if (checkBox1.Checked == false) return;
      //set motor east limit
      RemoteControl.Instance.DiSEqCSetEastLimit(_cardNumber);
    }

    private void comboBoxSat_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (_enableEvents == false) return;
      if (checkBox1.Checked == false) return;
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "selectedMotorSat", "0");
      setting.Value = comboBoxSat.SelectedIndex.ToString();
      setting.Persist();
      LoadMotorTransponder();
      comboBox1_SelectedIndexChanged(null, null);
    }

    private void checkBoxEnabled_CheckedChanged(object sender, EventArgs e)
    {
      if (_enableEvents == false) return;
      if (checkBox1.Checked == false) return;
      TvBusinessLayer layer = new TvBusinessLayer();
      if (checkBoxEnabled.Checked)
      {
        RemoteControl.Instance.DiSEqCForceLimit(_cardNumber, true);
        Setting setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "limitsEnabled", "yes");
        setting.Value = "yes";
        setting.Persist();
      }
      else
      {
        if (MessageBox.Show("Disabling the east/west limits could damage your dish!!! Are you sure?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
        {
          RemoteControl.Instance.DiSEqCForceLimit(_cardNumber, false);
          Setting setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "limitsEnabled", "yes");
          setting.Value = "no";
          setting.Persist();
        }
        else
        {
          _enableEvents = false;
          checkBoxEnabled.Checked = true;
          RemoteControl.Instance.DiSEqCForceLimit(_cardNumber, true);
          Setting setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "limitsEnabled", "yes");
          setting.Value = "yes";
          setting.Persist();
          _enableEvents = true;
        }
      }
    }

    void LoadMotorTransponder()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "limitsEnabled", "yes");
      if (setting.Value == "yes")
        checkBoxEnabled.Checked = true;
      if (setting.Value == "no")
        checkBoxEnabled.Checked = false;
      comboBox1.Items.Clear();
      SatteliteContext sat = (SatteliteContext)comboBoxSat.SelectedItem;
      LoadTransponders(sat);
      _transponders.Sort();
      foreach (Transponder transponder in _transponders)
      {
        comboBox1.Items.Add(transponder);
      }
      if (comboBox1.Items.Count > 0)
        comboBox1.SelectedIndex = 0;
      bool eventsEnabled = _enableEvents;
      _enableEvents = true;
      comboBox1_SelectedIndexChanged(null, null);
      _enableEvents = eventsEnabled;
    }

    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (_enableEvents == false) return;
      if (checkBox1.Checked == false) return;
      Transponder transponder = (Transponder)comboBox1.SelectedItem;
      TvBusinessLayer layer = new TvBusinessLayer();
      DVBSChannel tuneChannel = new DVBSChannel();
      tuneChannel.Frequency = transponder.CarrierFrequency;
      tuneChannel.Polarisation = transponder.Polarisation;
      tuneChannel.SymbolRate = transponder.SymbolRate;
      tuneChannel.BandType = BandType.Universal;
      tuneChannel.DisEqc = DisEqcType.None;
      User user = new User();
      user.CardId = _cardNumber;
      RemoteControl.Instance.Tune(ref user, tuneChannel, -1);

    }

    private void buttonStop_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false) return;
      if (checkBox1.Checked == false) return;
      RemoteControl.Instance.DiSEqCStopMotor(_cardNumber);
      comboBox1_SelectedIndexChanged(null, null);
    }

    private void buttonGotoStart_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false) return;
      if (checkBox1.Checked == false) return;
      RemoteControl.Instance.DiSEqCGotoReferencePosition(_cardNumber);
      comboBox1_SelectedIndexChanged(null, null);

    }

    private void buttonUp_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false) return;
      if (checkBox1.Checked == false) return;
      //move motor up
      RemoteControl.Instance.DiSEqCDriveMotor(_cardNumber, DiSEqCDirection.Up, (byte)(1 + comboBoxStepSize.SelectedIndex));
    }

    private void buttonDown_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false) return;
      if (checkBox1.Checked == false) return;
      //move motor up
      RemoteControl.Instance.DiSEqCDriveMotor(_cardNumber, DiSEqCDirection.Down, (byte)(1 + comboBoxStepSize.SelectedIndex));
    }

    private void comboBoxStepSize_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (_enableEvents == false) return;
      if (checkBox1.Checked == false) return;
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "motorStepSize", "10");
      setting.Value = String.Format("{0}", (1 + comboBoxStepSize.SelectedIndex));
      setting.Persist();

    }

    private void checkBox1_CheckedChanged(object sender, EventArgs e)
    {
      comboBoxSat.Enabled = checkBox1.Checked;
      comboBox1.Enabled = checkBox1.Checked;
      buttonGoto.Enabled = checkBox1.Checked;
      comboBoxStepSize.Enabled = checkBox1.Checked;
      buttonUp.Enabled = checkBox1.Checked;
      buttonDown.Enabled = checkBox1.Checked;
      buttonMoveWest.Enabled = checkBox1.Checked;
      buttonMoveEast.Enabled = checkBox1.Checked;
      buttonStop.Enabled = checkBox1.Checked;
      checkBoxEnabled.Enabled = checkBox1.Checked;
      buttonGotoStart.Enabled = checkBox1.Checked;
      buttonStore.Enabled = checkBox1.Checked;
      buttonSetWestLimit.Enabled = checkBox1.Checked;
      buttonSetEastLimit.Enabled = checkBox1.Checked;
      buttonReset.Enabled = checkBox1.Checked;

      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("dvbs" + _cardNumber.ToString() + "motorEnabled", "no");
      if (checkBox1.Checked) setting.Value = "yes";
      else setting.Value = "no";
      setting.Persist();
    }

    bool reentrant = false;
    DateTime _signalTimer = DateTime.MinValue;
    private void timer1_Tick(object sender, EventArgs e)
    {
      if (reentrant) return;
      try
      {
        reentrant = true;
        TimeSpan ts = DateTime.Now - _signalTimer;
        if (ts.TotalMilliseconds > 500)
        {
          if (checkBox1.Checked == false) return;
          RemoteControl.Instance.UpdateSignalSate(_cardNumber);
          _signalTimer = DateTime.Now;
          int satPos, stepsAzimuth, stepsElevation;
          RemoteControl.Instance.DiSEqCGetPosition(_cardNumber, out satPos, out stepsAzimuth, out stepsElevation);
          if (satPos < 0)
            labelCurrentPosition.Text = "unknown";
          else
          {
            string offset = "";
            string satPosition = String.Format("Satellite postion:#{0}", satPos);
            if (stepsAzimuth < 0)
              offset = String.Format("{0} steps west", -stepsAzimuth);
            else if (stepsAzimuth > 0)
              offset = String.Format("{0} steps east", stepsAzimuth);
            if (stepsElevation < 0)
            {
              if (offset.Length != 0)
                offset = String.Format("{0}, {1} steps up", offset, -stepsElevation);
              else
                offset = String.Format("{0} steps up", -stepsElevation);
            }
            else if (stepsElevation > 0)
            {
              if (offset.Length != 0)
                offset = String.Format("{0}, {1} steps down", offset, stepsElevation);
              else
                offset = String.Format("{0} steps down", stepsElevation);
            }
            if (offset.Length > 0)
              labelCurrentPosition.Text = String.Format("{0} of {1}", offset, satPosition);
            else
              labelCurrentPosition.Text = satPosition;
          }
        }
        UpdateStatus(1);
      }
      finally
      {
        reentrant = false;
      }
    }

    private void buttonReset_Click(object sender, EventArgs e)
    {
      if (checkBox1.Checked == false) return;
      RemoteControl.Instance.DiSEqCReset(_cardNumber);
    }

    #endregion
    
    private void buttonUpdate_Click(object sender, EventArgs e)
    {
      listViewStatus.Items.Clear();
      string itemLine = String.Format("Updating satellites...");
      ListViewItem item = listViewStatus.Items.Add(new ListViewItem(itemLine));
      Application.DoEvents();
      List<SatteliteContext> sats = LoadSattelites();
      foreach (SatteliteContext sat in sats)
      {
        DownloadTransponder(sat);
      }
      itemLine = String.Format("Update finished");
      item = listViewStatus.Items.Add(new ListViewItem(itemLine));
    }

    #region LNB selection tab
    private void checkBox2_CheckedChanged(object sender, EventArgs e)
    {
      textBoxLNBLo.Enabled = checkBox2.Checked;
      textBoxLNBHi.Enabled = checkBox2.Checked;
      textBoxLNBSwitch.Enabled = checkBox2.Checked;
    }

    private void mpLNB1_CheckedChanged(object sender, EventArgs e)
    {
      mpTransponder1.Enabled = mpLNB1.Checked;
      mpDisEqc1.Enabled = mpLNB1.Checked;
    }

    private void mpLNB2_CheckedChanged(object sender, EventArgs e)
    {
      mpTransponder2.Enabled = mpLNB2.Checked;
      mpDisEqc2.Enabled = mpLNB2.Checked;
    }

    private void mpLNB3_CheckedChanged(object sender, EventArgs e)
    {
      mpTransponder3.Enabled = mpLNB3.Checked;
      mpDisEqc3.Enabled = mpLNB3.Checked;
    }

    private void mpLNB4_CheckedChanged(object sender, EventArgs e)
    {
      mpTransponder4.Enabled = mpLNB4.Checked;
      mpDisEqc4.Enabled = mpLNB4.Checked;
    }

    private void mpBand1_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (_enableEvents == false) return;
      if (checkBox2.Checked) return;
      int lof1 = 0, lof2 = 0, sw = 0;
      ScanParameters p = new ScanParameters();
      BandTypeConverter.GetDefaultLnbSetup(p, (BandType)mpBand1.SelectedIndex, out lof1, out lof2, out sw);
      textBoxLNBLo.Text = lof1.ToString();
      textBoxLNBHi.Text = lof2.ToString();
      textBoxLNBSwitch.Text = sw.ToString();
    }

    private void mpBand2_SelectedIndexChanged(object sender, EventArgs e)
    {
      mpBand1_SelectedIndexChanged(sender, e);
    }

    private void mpBand3_SelectedIndexChanged(object sender, EventArgs e)
    {
      mpBand1_SelectedIndexChanged(sender, e);
    }

    private void mpBand4_SelectedIndexChanged(object sender, EventArgs e)
    {
      mpBand1_SelectedIndexChanged(sender, e);
    }
    #endregion

    private void checkBoxCreateGroupsSat_CheckedChanged(object sender, EventArgs e)
    {
      if (_ignoreCheckBoxCreateGroupsClickEvent) return;
      _ignoreCheckBoxCreateGroupsClickEvent = true;
      if (checkBoxCreateGroups.Checked)
      {
        checkBoxCreateGroups.Checked = false;
      }
      _ignoreCheckBoxCreateGroupsClickEvent = false;
    }

    private void checkBoxCreateGroups_CheckedChanged(object sender, EventArgs e)
    {
      if (_ignoreCheckBoxCreateGroupsClickEvent) return;
      _ignoreCheckBoxCreateGroupsClickEvent = true;
      if (checkBoxCreateGroupsSat.Checked)
      {
        checkBoxCreateGroupsSat.Checked = false;
      }
      _ignoreCheckBoxCreateGroupsClickEvent = false;
    }

    private void checkEnableDVBS2_CheckedChanged(object sender, EventArgs e)
    {
      if (checkEnableDVBS2.Checked)
      {
        checkBoxPilot.Enabled = true;
        comboBoxRollOff.Enabled = true;
      }
      if (!checkEnableDVBS2.Checked)
      {
        checkBoxPilot.Enabled = false;
        comboBoxRollOff.Enabled = false;
      }
    }
  }
}
