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
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Xml;
using System.Net;
using TvDatabase;
using TvControl;
using TvLibrary;
using TvLibrary.Log;
using TvLibrary.Channels;
using TvLibrary.Interfaces;
using DirectShowLib.BDA;
using System.Xml.Serialization;
using MediaPortal.UserInterface.Controls;

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
      public String DisplayName
      {
        get
        {
          return System.IO.Path.GetFileNameWithoutExtension(FileName);
        }
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

    [Serializable]
    public class Transponder : IComparable<Transponder>
    {
      public int CarrierFrequency; // frequency
      public Polarisation Polarisation;  // polarisation 0=hori, 1=vert
      public int SymbolRate; // symbol rate
      public ModulationType Modulation = ModulationType.ModNotSet;
      public BinaryConvolutionCodeRate InnerFecRate = BinaryConvolutionCodeRate.RateNotSet;
      public Pilot Pilot = Pilot.NotSet;
      public RollOff Rolloff = RollOff.NotSet;

      public DVBSChannel toDVBSChannel
      {
        get 
        {
          DVBSChannel tuneChannel = new DVBSChannel();
          tuneChannel.Frequency = this.CarrierFrequency;
          tuneChannel.Polarisation = this.Polarisation;
          tuneChannel.SymbolRate = this.SymbolRate;
          tuneChannel.ModulationType = this.Modulation;
          tuneChannel.InnerFecRate = this.InnerFecRate;
          //Grab the Pilot & Roll-off settings
          tuneChannel.Pilot = this.Pilot;
          tuneChannel.Rolloff = this.Rolloff;
          return tuneChannel;
        }
      }

      public int CompareTo(Transponder other)
      {
        if (Polarisation < other.Polarisation)
          return 1;
        if (Polarisation > other.Polarisation)
          return -1;
        if (CarrierFrequency > other.CarrierFrequency)
          return 1;
        if (CarrierFrequency < other.CarrierFrequency)
          return -1;
        if (SymbolRate > other.SymbolRate)
          return 1;
        if (SymbolRate < other.SymbolRate)
          return -1;
        return 0;
      }
      public override string ToString()
      {
        return String.Format("{0} {1} {2} {3} {4}", CarrierFrequency, SymbolRate, Polarisation, Modulation, InnerFecRate);
      }
    }
    #endregion

    #region variables

    readonly int _cardNumber;
    List<Transponder> _transponders = new List<Transponder>();

    int _tvChannelsNew;
    int _radioChannelsNew;
    int _tvChannelsUpdated;
    int _radioChannelsUpdated;
    bool _enableEvents;
    bool _ignoreCheckBoxCreateGroupsClickEvent;
    User _user;

    CI_Menu_Dialog ciMenuDialog; // ci menu dialog object

    ScanState scanState; // scan state

    bool _isScanning
    {
      get
      {
        return scanState == ScanState.Scanning || scanState == ScanState.Cancel || scanState == ScanState.Updating;
      }
    }
    /// <summary>
    /// Returns active scan type
    /// </summary>
    private ScanTypes ActiveScanType
    {
      get
      {
        if (checkBoxAdvancedTuning.Checked == false)
        {
          return ScanTypes.Predefined;
        }
        if (scanPredefProvider.Checked == true)
        {
          return ScanTypes.Predefined;
        }
        if (scanSingleTransponder.Checked == true)
        {
          return ScanTypes.SingleTransponder;
        }
        if (scanNIT.Checked == true)
        {
          return ScanTypes.NIT;
        }
        return ScanTypes.Predefined;
      }
    }

    #endregion

    #region ctors
    public CardDvbS()
      : this("DVBS")
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
      //insert complete ci menu dialog to tab
      Card dbCard = Card.Retrieve(_cardNumber);
      if (dbCard.CAM == true)
      {
        ciMenuDialog = new CI_Menu_Dialog(_cardNumber);
        this.tabPageCIMenu.Controls.Add(ciMenuDialog);
      }
      else
      {
        this.tabPageCIMenu.Dispose();
      }
      base.Text = name;
      Init();
    }
    #endregion

    #region helper methods
    /// <summary>
    /// Downloads new transponderlist and merges both S and S2 into one XML 
    /// </summary>
    /// <param name="context"></param>
    void DownloadTransponder(SatteliteContext context)
    {
      if (context.Url == null)
        return;
      if (context.Url.Length == 0)
        return;
      if (!context.Url.ToLowerInvariant().StartsWith("http://"))
        return;
      string itemLine = String.Format("Downloading transponders for: {0}", context.SatteliteName);
      ListViewItem item = listViewStatus.Items.Add(new ListViewItem(itemLine));
      item.EnsureVisible();
      Application.DoEvents();

      List<Transponder> transponders = new List<Transponder>();
      try
      {
        string[,] contextDownload = new string[2, 2];
        contextDownload[0, 0] = context.FileName;
        //        contextDownload[1, 0] = context.FileName.Replace(".ini", "-S2.ini");
        contextDownload[0, 1] = context.Url;
        contextDownload[1, 1] = context.Url.Replace(".ini", "-S2.ini");

        for (int row = 0; row <= 1; row++)
        {
          string satUrl = contextDownload[row, 1];
          item.Text = itemLine + " connecting...";
          Application.DoEvents();
          HttpWebRequest request = (HttpWebRequest)WebRequest.Create(satUrl);
          request.ReadWriteTimeout = 30 * 1000; //thirty secs timeout
          request.Proxy.Credentials = CredentialCache.DefaultCredentials;

          item.Text = itemLine + " downloading...";
          Application.DoEvents();
          using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
          {
            item.Text = itemLine + " saving...";
            Application.DoEvents();
            using (Stream resStream = response.GetResponseStream())
            {
              String line;
              using (TextReader tin = new StreamReader(resStream))
              {
                #region Parse and fill transponder list
                do
                {
                  line = tin.ReadLine();
                  if (line != null)
                  {
                    line = line.Trim();
                    if (line.Length > 0)
                    {
                      if (line.StartsWith(";"))
                        continue;
                      if (line.Contains("S2"))
                        continue;
                      try
                      {
                        String[] tpdata = line.Split(new char[] { ',' });
                        if (tpdata.Length >= 3)
                        {
                          if (tpdata[0].IndexOf("=") >= 0)
                          {
                            tpdata[0] = tpdata[0].Substring(tpdata[0].IndexOf("=") + 1);
                          }
                          Transponder transponder = new Transponder();
                          transponder.CarrierFrequency = Int32.Parse(tpdata[0]) * 1000;
                          switch (tpdata[1].ToLowerInvariant())
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
                          for (int idx = 3; idx < tpdata.Length; ++idx)
                          {
                            string fieldValue = tpdata[idx].ToLowerInvariant();
                            if (fieldValue == "8psk")
                              transponder.Modulation = ModulationType.Mod8Psk;
                            if (fieldValue == "qpsk")
                              transponder.Modulation = ModulationType.ModQpsk;
                            if (fieldValue == "16apsk")
                              transponder.Modulation = ModulationType.Mod16Apsk;
                            if (fieldValue == "32apsk")
                              transponder.Modulation = ModulationType.Mod32Apsk;

                            if (fieldValue == "12")
                              transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate1_2;
                            if (fieldValue == "23")
                              transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate2_3;
                            if (fieldValue == "34")
                              transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate3_4;
                            if (fieldValue == "35")
                              transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate3_5;
                            if (fieldValue == "45")
                              transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate4_5;
                            if (fieldValue == "511")
                              transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate5_11;
                            if (fieldValue == "56")
                              transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate5_6;
                            if (fieldValue == "78")
                              transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate7_8;
                            if (fieldValue == "14")
                              transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate1_4;
                            if (fieldValue == "13")
                              transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate1_3;
                            if (fieldValue == "25")
                              transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate2_5;
                            if (fieldValue == "67")
                              transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate6_7;
                            if (fieldValue == "89")
                              transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate8_9;
                            if (fieldValue == "910")
                              transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate9_10;

                            if (fieldValue == "1/2")
                              transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate1_2;
                            if (fieldValue == "2/3")
                              transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate2_3;
                            if (fieldValue == "3/4")
                              transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate3_4;
                            if (fieldValue == "3/5")
                              transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate3_5;
                            if (fieldValue == "4/5")
                              transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate4_5;
                            if (fieldValue == "5/11")
                              transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate5_11;
                            if (fieldValue == "5/6")
                              transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate5_6;
                            if (fieldValue == "7/8")
                              transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate7_8;
                            if (fieldValue == "1/4")
                              transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate1_4;
                            if (fieldValue == "1/3")
                              transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate1_3;
                            if (fieldValue == "2/5")
                              transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate2_5;
                            if (fieldValue == "6/7")
                              transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate6_7;
                            if (fieldValue == "8/9")
                              transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate8_9;
                            if (fieldValue == "9/10")
                              transponder.InnerFecRate = BinaryConvolutionCodeRate.Rate9_10;

                            if (fieldValue == "off")
                              transponder.Pilot = Pilot.Off;
                            if (fieldValue == "on")
                              transponder.Pilot = Pilot.On;

                            if (fieldValue == "0.20")
                              transponder.Rolloff = RollOff.Twenty;
                            if (fieldValue == "0.25")
                              transponder.Rolloff = RollOff.TwentyFive;
                            if (fieldValue == "0.35")
                              transponder.Rolloff = RollOff.ThirtyFive;
                          }
                          transponders.Add(transponder);
                        }
                      }
                      catch { } // ignore parsing errors in single line (i.e. 19,2 Astra reading fails due split & parse)
                    }
                  }
                }
                while (line != null);
                #endregion
              }
            }
          }
        } // for
      }
      catch (WebException WebEx)
      {
        //HTTP Protocol error, like 404 file not found. Do Nothing
        if (WebEx.Status != WebExceptionStatus.ProtocolError)
        {
          throw new WebException(WebEx.Message);
        }
        item.Text = itemLine + " done";
      }
      catch (Exception)
      {
        item.Text = itemLine + " failed";
      }
      finally
      {
        String newPath = String.Format(@"{0}\TuningParameters\dvbs\{1}.xml", Log.GetPathName(), Path.GetFileNameWithoutExtension(context.FileName));
        if (File.Exists(newPath))
        {
          File.Delete(newPath);
        }
        if (transponders.Count > 0)
        {
          System.IO.TextWriter parFileXML = System.IO.File.CreateText(newPath);
          XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<Transponder>));
          xmlSerializer.Serialize(parFileXML, transponders);
          parFileXML.Close();
        }
        item.Text = itemLine + " done";
      }
      Application.DoEvents();
    }

    /// <summary>
    /// Loads new xml transponder list
    /// </summary>
    /// <param name="FileName"></param>
    void LoadTransponders(SatteliteContext context)
    {
      String fileName = context.FileName;
      if (!File.Exists(fileName))
      {
        DownloadTransponder(context);
      }

      // clear before refilling
      _transponders.Clear();
      try
      {
        XmlReader parFileXML = XmlReader.Create(fileName);
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<Transponder>));
        _transponders = (List<Transponder>)xmlSerializer.Deserialize(parFileXML);
        parFileXML.Close();
        _transponders.Sort();
      }
      catch (Exception ex)
      {
        Log.Error("Error loading tuningdetails: {0}", ex.ToString());
        MessageBox.Show("Transponder list could not be loaded, check error.log for details.");
      }
    }

    /// <summary>
    /// Loads all known satellites from xml file
    /// </summary>
    /// <returns></returns>
    static List<SatteliteContext> LoadSattelites()
    {
      List<SatteliteContext> satellites = new List<SatteliteContext>();
      XmlDocument doc = new XmlDocument();
      doc.Load(String.Format(@"{0}\TuningParameters\dvbs\satellites.xml", Log.GetPathName()));
      XmlNodeList nodes = doc.SelectNodes("/satellites/satellite");
      if (nodes != null)
      {
        foreach (XmlNode node in nodes)
        {
          SatteliteContext ts = new SatteliteContext();
          ts.SatteliteName = node.Attributes.GetNamedItem("name").Value;
          ts.Url = node.Attributes.GetNamedItem("url").Value;
          string name = Utils.FilterFileName(ts.SatteliteName);
          ts.FileName = String.Format(@"{0}\TuningParameters\dvbs\{1}.xml", Log.GetPathName(), name);
          satellites.Add(ts);
        }
      }
      String[] files = System.IO.Directory.GetFiles(String.Format(@"{0}\TuningParameters\dvbs\", Log.GetPathName()), "*.xml");
      foreach (String file in files)
      {
        if (Path.GetFileName(file).StartsWith("Manual_Scans"))
        {
          SatteliteContext ts = new SatteliteContext();
          ts.SatteliteName = Path.GetFileNameWithoutExtension(file);
          ts.Url = "";
          ts.FileName = file;
          satellites.Add(ts);
        }
      }
      IList<Satellite> dbSats = Satellite.ListAll();
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
      // set to same positions as progress
      mpGrpAdvancedTuning.Top = mpGrpScanProgress.Top;

      _enableEvents = false;
    
      TvBusinessLayer layer = new TvBusinessLayer();
      int idx = 0;

      mpComboBoxPolarisation.Items.Add(Polarisation.LinearH);
      mpComboBoxPolarisation.Items.Add(Polarisation.LinearV);
      mpComboBoxPolarisation.Items.Add(Polarisation.CircularL);
      mpComboBoxPolarisation.Items.Add(Polarisation.CircularR);
      mpComboBoxPolarisation.Items.Add(Polarisation.Max);
      mpComboBoxPolarisation.Items.Add(Polarisation.NotDefined);
      mpComboBoxPolarisation.Items.Add(Polarisation.NotSet);
      mpComboBoxPolarisation.SelectedIndex = 0;
      mpComboBoxMod.SelectedIndex = 0;
      //mpButtonSaveList.Enabled = (_transponders.Count != 0);


      //List<SimpleFileName> satellites = fileFilters.AllFiles;
      List<SatteliteContext> satellites = LoadSattelites();
      MPComboBox[] mpTrans = new MPComboBox[] { mpTransponder1, mpTransponder2, mpTransponder3, mpTransponder4 };
      MPComboBox[] mpDisEqc = new MPComboBox[] { mpDisEqc1, mpDisEqc2, mpDisEqc3, mpDisEqc4 };
      MPComboBox[] mpBands = new MPComboBox[] { mpBand1, mpBand2, mpBand3, mpBand4 };
      MPCheckBox[] mpLNBs = new MPCheckBox[] { mpLNB1, mpLNB2, mpLNB3, mpLNB4 };
      MPComboBox curBox;
      MPCheckBox curCheck;
      for(int ctlIndex= 0; ctlIndex<4; ctlIndex++)
      {
        idx=ctlIndex+1;
        curBox = mpTrans[ctlIndex];
        curBox.Items.Clear();
        foreach (SatteliteContext ts in satellites)
        {
          curBox.Items.Add(ts);
        }
        if (curBox.Items.Count > 0)
        {
          int selIdx = Int32.Parse(layer.GetSetting(String.Format("dvbs{0}SatteliteContext{1}", _cardNumber, idx), "0").Value);
          if (selIdx < curBox.Items.Count)
          {
            curBox.SelectedIndex = selIdx;
          }
        }

        curBox = mpDisEqc[ctlIndex];
        curBox.Items.Clear();
        curBox.Items.Add(DisEqcType.None);
        curBox.Items.Add(DisEqcType.SimpleA);
        curBox.Items.Add(DisEqcType.SimpleB);
        curBox.Items.Add(DisEqcType.Level1AA);
        curBox.Items.Add(DisEqcType.Level1AB);
        curBox.Items.Add(DisEqcType.Level1BA);
        curBox.Items.Add(DisEqcType.Level1BB);
        curBox.SelectedIndex = Int32.Parse(layer.GetSetting(String.Format("dvbs{0}DisEqc{1}", _cardNumber, idx), "0").Value);

        curBox = mpBands[ctlIndex];
        curBox.SelectedIndex = Int32.Parse(layer.GetSetting(String.Format("dvbs{0}band{1}", _cardNumber, idx), "0").Value);

        curCheck = mpLNBs[ctlIndex];
        curCheck.Checked = (layer.GetSetting(String.Format("dvbs{0}LNB{1}", _cardNumber, idx), "0").Value == "true");

      }

      mpLNB1_CheckedChanged(null, null);
      mpLNB2_CheckedChanged(null, null);
      mpLNB3_CheckedChanged(null, null);
      mpLNB4_CheckedChanged(null, null);

      chkOverrideLNB.Checked = (layer.GetSetting("lnbDefault", "true").Value != "true");
      textBoxLNBLo.Text = layer.GetSetting("LnbLowFrequency", "0").Value;
      textBoxLNBHi.Text = layer.GetSetting("LnbHighFrequency", "0").Value;
      textBoxLNBSwitch.Text = layer.GetSetting("LnbSwitchFrequency", "0").Value;
      chkOverrideLNB_CheckedChanged(null, null);

      checkBoxCreateGroups.Checked = (layer.GetSetting("dvbs" + _cardNumber + "creategroups", "false").Value == "true");
      if (!checkBoxCreateGroups.Checked)
      {
        checkBoxCreateGroupsSat.Checked = (layer.GetSetting("dvbs" + _cardNumber + "creategroupssat", "false").Value == "true");
      }

      checkEnableDVBS2.Checked = (layer.GetSetting("dvbs" + _cardNumber + "enabledvbs2", "false").Value == "true");
      if (!checkEnableDVBS2.Checked)
      {
        checkEnableDVBS2.Checked = (layer.GetSetting("dvbs" + _cardNumber + "enabledvbs2", "false").Value == "true");
      }

      _enableEvents = true;
      mpLNB1_CheckedChanged(null, null);

      checkBoxAdvancedTuning.Checked = false;
      //grpManualScan.Enabled = false;
      checkBoxAdvancedTuning.Enabled = true;

      scanState = ScanState.Initialized;
    }

    public override void OnSectionDeActivated()
    {
      timer1.Enabled = false;
      SaveSettings();
      base.OnSectionDeActivated();
      if (ciMenuDialog != null)
      {
        ciMenuDialog.OnSectionDeActivated();
      }
    }

    public override void SaveSettings()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("dvbs" + _cardNumber + "creategroups", "false");
      setting.Value = checkBoxCreateGroups.Checked ? "true" : "false";
      setting.Persist();

      setting = layer.GetSetting("dvbs" + _cardNumber + "creategroupssat", "false");
      setting.Value = checkBoxCreateGroupsSat.Checked ? "true" : "false";
      setting.Persist();

      setting = layer.GetSetting("dvbs" + _cardNumber + "SatteliteContext1", "0");
      setting.Value = mpTransponder1.SelectedIndex.ToString();
      setting.Persist();
      setting = layer.GetSetting("dvbs" + _cardNumber + "SatteliteContext2", "0");
      setting.Value = mpTransponder2.SelectedIndex.ToString();
      setting.Persist();
      setting = layer.GetSetting("dvbs" + _cardNumber + "SatteliteContext3", "0");
      setting.Value = mpTransponder3.SelectedIndex.ToString();
      setting.Persist();
      setting = layer.GetSetting("dvbs" + _cardNumber + "SatteliteContext4", "0");
      setting.Value = mpTransponder4.SelectedIndex.ToString();
      setting.Persist();

      setting = layer.GetSetting("dvbs" + _cardNumber + "DisEqc1", "0");
      setting.Value = mpDisEqc1.SelectedIndex.ToString();
      setting.Persist();
      setting = layer.GetSetting("dvbs" + _cardNumber + "DisEqc2", "0");
      setting.Value = mpDisEqc2.SelectedIndex.ToString();
      setting.Persist();
      setting = layer.GetSetting("dvbs" + _cardNumber + "DisEqc3", "0");
      setting.Value = mpDisEqc3.SelectedIndex.ToString();
      setting.Persist();
      setting = layer.GetSetting("dvbs" + _cardNumber + "DisEqc4", "0");
      setting.Value = mpDisEqc4.SelectedIndex.ToString();
      setting.Persist();

      setting = layer.GetSetting("dvbs" + _cardNumber + "band1", "0");
      setting.Value = mpBand1.SelectedIndex.ToString();
      setting.Persist();
      setting = layer.GetSetting("dvbs" + _cardNumber + "band2", "0");
      setting.Value = mpBand2.SelectedIndex.ToString();
      setting.Persist();
      setting = layer.GetSetting("dvbs" + _cardNumber + "band3", "0");
      setting.Value = mpBand3.SelectedIndex.ToString();
      setting.Persist();
      setting = layer.GetSetting("dvbs" + _cardNumber + "band4", "0");
      setting.Value = mpBand4.SelectedIndex.ToString();
      setting.Persist();

      setting = layer.GetSetting("dvbs" + _cardNumber + "LNB1", "false");
      setting.Value = mpLNB1.Checked ? "true" : "false";
      setting.Persist();

      setting = layer.GetSetting("dvbs" + _cardNumber + "LNB2", "false");
      setting.Value = mpLNB2.Checked ? "true" : "false";
      setting.Persist();
      setting = layer.GetSetting("dvbs" + _cardNumber + "LNB3", "false");
      setting.Value = mpLNB3.Checked ? "true" : "false";
      setting.Persist();
      setting = layer.GetSetting("dvbs" + _cardNumber + "LNB4", "false");
      setting.Value = mpLNB4.Checked ? "true" : "false";
      setting.Persist();

      setting = layer.GetSetting("dvbs" + _cardNumber + "enabledvbs2", "false");
      setting.Value = checkEnableDVBS2.Checked ? "true" : "false";
      setting.Persist();

      bool restart = false;
      setting = layer.GetSetting("lnbDefault", "true");
      if (setting.Value != (chkOverrideLNB.Checked ? "false" : "true"))
        restart = true;
      setting.Value = chkOverrideLNB.Checked ? "false" : "true";
      setting.Persist();

      setting = layer.GetSetting("LnbLowFrequency", "0");
      if (setting.Value != textBoxLNBLo.Text)
        restart = true;
      setting.Value = textBoxLNBLo.Text;
      setting.Persist();

      setting = layer.GetSetting("LnbHighFrequency", "0");
      if (setting.Value != textBoxLNBHi.Text)
        restart = true;
      setting.Value = textBoxLNBHi.Text;
      setting.Persist();

      setting = layer.GetSetting("LnbSwitchFrequency", "0");
      if (setting.Value != textBoxLNBSwitch.Text)
        restart = true;
      setting.Value = textBoxLNBSwitch.Text;
      setting.Persist();
      if (restart)
      {
        RemoteControl.Instance.ClearCache();
        RemoteControl.Instance.Restart();
      }
    }

    void UpdateStatus()
    {
      progressBarLevel.Value = Math.Min(100, RemoteControl.Instance.SignalLevel(_cardNumber));
      progressBarQuality.Value = Math.Min(100, RemoteControl.Instance.SignalQuality(_cardNumber));
      progressBarSatLevel.Value = Math.Min(100, RemoteControl.Instance.SignalLevel(_cardNumber));
      progressBarSatQuality.Value = Math.Min(100, RemoteControl.Instance.SignalQuality(_cardNumber));
      labelTunerLock.Text = RemoteControl.Instance.TunerLocked(_cardNumber) ? "Yes" : "No";
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      UpdateStatus();
      labelCurrentPosition.Text = "";
      tabControl1_SelectedIndexChanged(null, null);
      _user = new User();

      if (ciMenuDialog != null)
      {
        ciMenuDialog.OnSectionActivated();
      }
    }

        #region Scan handling
    private void InitScanProcess()
    {
      // once completed reset to new beginning
      switch (scanState)
      {
        case ScanState.Done:
          scanState = ScanState.Initialized;
          listViewStatus.Items.Clear();
          SetButtonState();
          return;

        case ScanState.Initialized:
          SaveSettings();
          TvBusinessLayer layer = new TvBusinessLayer();
          Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));
          if (card.Enabled == false)
          {
            MessageBox.Show(this, "Card is disabled, please enable the card before scanning");
            return;
          }
          if (!RemoteControl.Instance.CardPresent(card.IdCard))
          {
            MessageBox.Show(this, "Card is not found, please make sure card is present before scanning");
            return;
          }
          // Check if the card is locked for scanning.
          User user;
          if (RemoteControl.Instance.IsCardInUse(_cardNumber, out user))
          {
            MessageBox.Show(this, "Card is locked. Scanning not possible at the moment ! Perhaps you are scanning an other part of a hybrid card.");
            return;
          }

          SetButtonState();
          ShowActiveGroup(1); // force progess visible

          Thread scanThread = new Thread(DoScan);
          scanThread.Name = "DVB-S scan thread";
          scanThread.Start();
          break;

        case ScanState.Scanning:
          scanState = ScanState.Cancel;
          SetButtonState();
          break;

        case ScanState.Cancel:
          return;
      }
    }
    #endregion


    private void mpButtonScanTv_Click(object sender, EventArgs e)
    {
      InitScanProcess();
    }

    void DoScan()
    {
      try
      {
        scanState = ScanState.Scanning;
        SetButtonState();
        RemoteControl.Instance.EpgGrabberEnabled = false;
        SetAllControlState(false);

        listViewStatus.Items.Clear();
        _tvChannelsNew = 0;
        _radioChannelsNew = 0;
        _tvChannelsUpdated = 0;
        _radioChannelsUpdated = 0;

        if (mpLNB1.Checked)
          Scan(1, (BandType)mpBand1.SelectedIndex, (DisEqcType)mpDisEqc1.SelectedIndex, (SatteliteContext)mpTransponder1.SelectedItem);
        if (scanState == ScanState.Cancel)
          return;

        if (mpLNB2.Checked)
          Scan(2, (BandType)mpBand2.SelectedIndex, (DisEqcType)mpDisEqc2.SelectedIndex, (SatteliteContext)mpTransponder2.SelectedItem);
        if (scanState == ScanState.Cancel)
          return;

        if (mpLNB3.Checked)
          Scan(3, (BandType)mpBand3.SelectedIndex, (DisEqcType)mpDisEqc3.SelectedIndex, (SatteliteContext)mpTransponder3.SelectedItem);
        if (scanState == ScanState.Cancel)
          return;

        if (mpLNB4.Checked)
          Scan(4, (BandType)mpBand4.SelectedIndex, (DisEqcType)mpDisEqc4.SelectedIndex, (SatteliteContext)mpTransponder4.SelectedItem);

        listViewStatus.Items.Add(new ListViewItem(String.Format("Total radio channels new:{0} updated:{1}", _radioChannelsNew, _radioChannelsUpdated)));
        listViewStatus.Items.Add(new ListViewItem(String.Format("Total tv channels new:{0} updated:{1}", _tvChannelsNew, _tvChannelsUpdated)));
        ListViewItem item = listViewStatus.Items.Add(new ListViewItem("Scan done..."));
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
        progressBar1.Value = 100;
        SetAllControlState(true);
        scanState = ScanState.Done;
        SetButtonState();
      }
    }

    private void SetAllControlState(bool state)
    {
      mpTransponder1.Enabled = state;
      mpTransponder2.Enabled = state;
      mpTransponder3.Enabled = state;
      mpTransponder4.Enabled = state;
      mpDisEqc1.Enabled = state;
      mpDisEqc2.Enabled = state;
      mpDisEqc3.Enabled = state;
      mpDisEqc4.Enabled = state;
      mpBand1.Enabled = state;
      mpBand2.Enabled = state;
      mpBand3.Enabled = state;
      mpBand4.Enabled = state;
      mpLNB1.Enabled = state;
      mpLNB2.Enabled = state;
      mpLNB3.Enabled = state;
      mpLNB4.Enabled = state;
      checkEnableDVBS2.Enabled = state;
    }

    void Scan(int LNB, BandType bandType, DisEqcType disEqc, SatteliteContext context)
    {
      // all transponders to scan
      List<DVBSChannel> _channels = new List<DVBSChannel>();

      // get default sat position from DB
      TvBusinessLayer layer = new TvBusinessLayer();
      Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));

      int position = -1;
      Setting setting = layer.GetSetting("dvbs" + _cardNumber + "motorEnabled", "no");
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

      // what to scan
      switch (ActiveScanType)
      {
        case ScanTypes.Predefined:
          LoadTransponders(context);
          foreach (Transponder t in _transponders)
          {
            DVBSChannel curChannel = t.toDVBSChannel;
            curChannel.BandType = bandType;
            curChannel.SatelliteIndex = position;
            _channels.Add(curChannel);
          }
          break;

        // scan Network Information Table for transponder info
        case ScanTypes.NIT:
          _transponders.Clear();
          DVBSChannel tuneChannel = GetManualTuning();

          listViewStatus.Items.Clear();
          string line = String.Format("lnb:{0} {1}tp- {2} {3} {4}", LNB, 1 , tuneChannel.Frequency, tuneChannel.Polarisation, tuneChannel.SymbolRate);
          ListViewItem item = listViewStatus.Items.Add(new ListViewItem(line));
          item.EnsureVisible();

          IChannel[] channels = RemoteControl.Instance.ScanNIT(_cardNumber, tuneChannel);
          if (channels != null)
          {
            for (int i = 0; i < channels.Length; ++i)
            {
              DVBSChannel curChannel = (DVBSChannel)channels[i];
              curChannel.BandType = bandType;
              curChannel.SatelliteIndex = position;
              _channels.Add(curChannel);
              item = listViewStatus.Items.Add(new ListViewItem(curChannel.ToString()));
              item.EnsureVisible();
            }
          }

          ListViewItem lastItem = listViewStatus.Items.Add(new ListViewItem(String.Format("Scan done, found {0} transponders...", _channels.Count)));
          lastItem.EnsureVisible();
          break;

        // scan only single TP
        case ScanTypes.SingleTransponder:
          _channels.Add(GetManualTuning());
          break;
      }
      
      // no channels
      if (_channels.Count == 0)
        return;
      
      User user = new User();
      user.CardId = _cardNumber;
      int scanIndex = 0; // count of really scanned TPs (S2 skipped)
      for (int index = 0; index < _channels.Count; ++index)
      {
        if (scanState == ScanState.Cancel)
          return;

        DVBSChannel tuneChannel = _channels[index];
        float percent = ((float)(index)) / _channels.Count;
        percent *= 100f;
        if (percent > 100f)
          percent = 100f;
        progressBar1.Value = (int)percent;

        // if S2 transponder and not enabled skip it
        if (tuneChannel.Rolloff != RollOff.NotSet && !checkEnableDVBS2.Checked)
        {
          continue;
        }
        
        scanIndex++;

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

        if (scanIndex == 1) // first scanned
        {
          RemoteControl.Instance.Tune(ref user, tuneChannel, -1);
        }
        UpdateStatus();

        IChannel[] channels = RemoteControl.Instance.Scan(_cardNumber, tuneChannel);

        UpdateStatus();

        if (channels == null || channels.Length == 0)
        {
          if (RemoteControl.Instance.TunerLocked(_cardNumber) == false)
          {
            line = String.Format("lnb:{0} {1}tp- {2} {3} {4}:No signal", LNB, scanIndex, tuneChannel.Frequency, tuneChannel.Polarisation, tuneChannel.SymbolRate);
            item.Text = line;
            item.ForeColor = Color.Red;
            continue;
          }
          line = String.Format("lnb:{0} {1}tp- {2} {3} {4}:Nothing found", LNB, scanIndex, tuneChannel.Frequency, tuneChannel.Polarisation, tuneChannel.SymbolRate);
          item.Text = line;
          item.ForeColor = Color.Red;
          continue;
        }

        int newChannels = 0;
        int updatedChannels = 0;
        for (int i = 0; i < channels.Length; ++i)
        {
          Channel dbChannel;
          DVBSChannel channel = (DVBSChannel)channels[i];
          //TuningDetail currentDetail = layer.GetChannel(channel);
          TuningDetail currentDetail = layer.GetChannel(channel.Provider, channel.Name, channel.ServiceId);
          bool exists;
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
          dbChannel.Persist();


          if (dbChannel.IsTv)
          {
            layer.AddChannelToGroup(dbChannel, TvConstants.TvGroupNames.AllChannels);
            layer.AddChannelToGroup(dbChannel, TvConstants.TvGroupNames.DVBS);
            if (checkBoxCreateGroupsSat.Checked)
            {
              layer.AddChannelToGroup(dbChannel, context.Satelite.SatelliteName);
            }
            if (checkBoxCreateGroups.Checked)
            {
              layer.AddChannelToGroup(dbChannel, channel.Provider);
            }
          }
          if (dbChannel.IsRadio)
          {
            layer.AddChannelToRadioGroup(dbChannel, TvConstants.RadioGroupNames.AllChannels);
            layer.AddChannelToRadioGroup(dbChannel, TvConstants.RadioGroupNames.DVBS);
            if (checkBoxCreateGroupsSat.Checked)
            {
              layer.AddChannelToRadioGroup(dbChannel, context.Satelite.SatelliteName);
            }
            if (checkBoxCreateGroups.Checked)
            {
              layer.AddChannelToRadioGroup(dbChannel, channel.Provider);
            }
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
            TuningDetail td = layer.UpdateTuningDetails(dbChannel, channel, currentDetail);
            td.Persist();
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
          layer.MapChannelToCard(card, dbChannel, false);
          line = String.Format("lnb:{0} {1}tp- {2} {3} {4}:New:{5} Updated:{6}",
              LNB, 1 + index, tuneChannel.Frequency, tuneChannel.Polarisation, tuneChannel.SymbolRate, newChannels, updatedChannels);
          item.Text = line;
        }
      }
    }

    private void CardDvbS_Load(object sender, EventArgs e)
    {

    }

    #endregion

    #region DiSEqC Motor tab

    void SetupMotor()
    {
      _enableEvents = false;
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("dvbs" + _cardNumber + "motorEnabled", "no");
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

      setting = layer.GetSetting("dvbs" + _cardNumber + "motorStepSize", "10");
      int stepsize;
      if (Int32.TryParse(setting.Value, out stepsize))
        comboBoxStepSize.SelectedIndex = stepsize - 1;
      else
        comboBoxStepSize.SelectedIndex = 9;

      comboBoxSat.Items.Clear();

      setting = layer.GetSetting("dvbs" + _cardNumber + "selectedMotorSat", "0");
      int index;
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
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;
      //move motor west
      RemoteControl.Instance.DiSEqCDriveMotor(_cardNumber, DiSEqCDirection.West, (byte)(1 + comboBoxStepSize.SelectedIndex));
      comboBox1_SelectedIndexChanged(null, null);//tune..;
    }

    private void buttonSetWestLimit_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;
      //set motor west limit
      RemoteControl.Instance.DiSEqCSetWestLimit(_cardNumber);
    }

    private void tabPage2_Click(object sender, EventArgs e)
    {

    }

    private void button1_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      //goto selected sat
      if (comboBoxSat.SelectedIndex < 0)
        return;
      if (checkBox1.Checked == false)
        return;
      SatteliteContext sat = (SatteliteContext)comboBoxSat.Items[comboBoxSat.SelectedIndex];

      Card card = Card.Retrieve(_cardNumber);
      IList<DiSEqCMotor> motorSettings = card.ReferringDiSEqCMotor();
      foreach (DiSEqCMotor motor in motorSettings)
      {
        if (motor.IdSatellite == sat.Satelite.IdSatellite)
        {
          RemoteControl.Instance.DiSEqCGotoPosition(_cardNumber, (byte)motor.Position);
          MessageBox.Show("Satellite moving to position:" + motor.Position, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
          comboBox1_SelectedIndexChanged(null, null);
          return;
        }
      }
      MessageBox.Show("No position stored for this satellite", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
    }

    private void buttonStore_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;
      //store motor position..
      int index = -1;
      SatteliteContext sat = (SatteliteContext)comboBoxSat.SelectedItem;
      Card card = Card.Retrieve(_cardNumber);
      IList<DiSEqCMotor> motorSettings = card.ReferringDiSEqCMotor();
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
      MessageBox.Show("Satellite position stored to:" + index, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

    }

    private void buttonMoveEast_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;
      //move motor east
      RemoteControl.Instance.DiSEqCDriveMotor(_cardNumber, DiSEqCDirection.East, (byte)(1 + comboBoxStepSize.SelectedIndex));
      comboBox1_SelectedIndexChanged(null, null);//tune..
    }

    private void buttonSetEastLimit_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;
      //set motor east limit
      RemoteControl.Instance.DiSEqCSetEastLimit(_cardNumber);
    }

    private void comboBoxSat_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("dvbs" + _cardNumber + "selectedMotorSat", "0");
      setting.Value = comboBoxSat.SelectedIndex.ToString();
      setting.Persist();
      LoadMotorTransponder();
      comboBox1_SelectedIndexChanged(null, null);
    }

    private void checkBoxEnabled_CheckedChanged(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;
      TvBusinessLayer layer = new TvBusinessLayer();
      if (checkBoxEnabled.Checked)
      {
        RemoteControl.Instance.DiSEqCForceLimit(_cardNumber, true);
        Setting setting = layer.GetSetting("dvbs" + _cardNumber + "limitsEnabled", "yes");
        setting.Value = "yes";
        setting.Persist();
      }
      else
      {
        if (MessageBox.Show("Disabling the east/west limits could damage your dish!!! Are you sure?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
        {
          RemoteControl.Instance.DiSEqCForceLimit(_cardNumber, false);
          Setting setting = layer.GetSetting("dvbs" + _cardNumber + "limitsEnabled", "yes");
          setting.Value = "no";
          setting.Persist();
        }
        else
        {
          _enableEvents = false;
          checkBoxEnabled.Checked = true;
          RemoteControl.Instance.DiSEqCForceLimit(_cardNumber, true);
          Setting setting = layer.GetSetting("dvbs" + _cardNumber + "limitsEnabled", "yes");
          setting.Value = "yes";
          setting.Persist();
          _enableEvents = true;
        }
      }
    }

    void LoadMotorTransponder()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("dvbs" + _cardNumber + "limitsEnabled", "yes");
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
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;
      Transponder transponder = (Transponder)comboBox1.SelectedItem;
      DVBSChannel tuneChannel = new DVBSChannel();
      tuneChannel.Frequency = transponder.CarrierFrequency;
      tuneChannel.Polarisation = transponder.Polarisation;
      tuneChannel.SymbolRate = transponder.SymbolRate;
      tuneChannel.ModulationType = transponder.Modulation;
      tuneChannel.Pilot = transponder.Pilot;
      tuneChannel.Rolloff = transponder.Rolloff;
      tuneChannel.InnerFecRate = transponder.InnerFecRate;
      tuneChannel.BandType = BandType.Universal;
      tuneChannel.DisEqc = DisEqcType.None;
      if (mpBand1.SelectedIndex >= 0)
        tuneChannel.BandType = (BandType)mpBand1.SelectedIndex;
      if (mpDisEqc1.SelectedIndex >= 0)
        tuneChannel.DisEqc = (DisEqcType)mpDisEqc1.SelectedIndex;
      _user.CardId = _cardNumber;
      RemoteControl.Instance.StopCard(_user);
      _user.CardId = _cardNumber;
      RemoteControl.Instance.Tune(ref _user, tuneChannel, -1);
      progressBarLevel.Value = 1;
      progressBarQuality.Value = 1;
      progressBarSatLevel.Value = 1;
      progressBarSatQuality.Value = 1;
      labelTunerLock.Text = String.Empty;

    }

    private void buttonStop_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;
      RemoteControl.Instance.DiSEqCStopMotor(_cardNumber);
      comboBox1_SelectedIndexChanged(null, null);
    }

    private void buttonGotoStart_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;
      RemoteControl.Instance.DiSEqCGotoReferencePosition(_cardNumber);
      comboBox1_SelectedIndexChanged(null, null);

    }

    private void buttonUp_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;
      //move motor up
      RemoteControl.Instance.DiSEqCDriveMotor(_cardNumber, DiSEqCDirection.Up, (byte)(1 + comboBoxStepSize.SelectedIndex));
    }

    private void buttonDown_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;
      //move motor up
      RemoteControl.Instance.DiSEqCDriveMotor(_cardNumber, DiSEqCDirection.Down, (byte)(1 + comboBoxStepSize.SelectedIndex));
    }

    private void comboBoxStepSize_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting setting = layer.GetSetting("dvbs" + _cardNumber + "motorStepSize", "10");
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
      Setting setting = layer.GetSetting("dvbs" + _cardNumber + "motorEnabled", "no");
      setting.Value = checkBox1.Checked ? "yes" : "no";
      setting.Persist();
    }

    bool reentrant;
    DateTime _signalTimer = DateTime.MinValue;
    private void timer1_Tick(object sender, EventArgs e)
    {
      if (reentrant)
        return;
      try
      {
        reentrant = true;
        TimeSpan ts = DateTime.Now - _signalTimer;
        if (ts.TotalMilliseconds > 500)
        {
          if (checkBox1.Checked == false)
            return;

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
              offset = offset.Length != 0 ? String.Format("{0}, {1} steps up", offset, -stepsElevation) : String.Format("{0} steps up", -stepsElevation);
            }
            else if (stepsElevation > 0)
            {
              offset = offset.Length != 0 ? String.Format("{0}, {1} steps down", offset, stepsElevation) : String.Format("{0} steps down", stepsElevation);
            }
            labelCurrentPosition.Text = offset.Length > 0 ? String.Format("{0} of {1}", offset, satPosition) : satPosition;
          }
        }
        UpdateStatus();
      }
      finally
      {
        reentrant = false;
      }
    }

    private void buttonReset_Click(object sender, EventArgs e)
    {
      if (checkBox1.Checked == false)
        return;
      RemoteControl.Instance.DiSEqCReset(_cardNumber);
    }

    #endregion

    private void buttonUpdate_Click(object sender, EventArgs e)
    {
      scanState = ScanState.Updating;
      SetButtonState();
      ShowActiveGroup(1);
      listViewStatus.Items.Clear();
      string itemLine = String.Format("Updating satellites...");
      listViewStatus.Items.Add(new ListViewItem(itemLine));
      Application.DoEvents();
      List<SatteliteContext> sats = LoadSattelites();
      foreach (SatteliteContext sat in sats)
      {
        DownloadTransponder(sat);
      }
      itemLine = String.Format("Update finished");
      listViewStatus.Items.Add(new ListViewItem(itemLine));
      scanState = ScanState.Initialized;
      SetButtonState();
    }

    #region LNB selection tab
    private void chkOverrideLNB_CheckedChanged(object sender, EventArgs e)
    {
      textBoxLNBLo.Enabled = chkOverrideLNB.Checked;
      textBoxLNBHi.Enabled = chkOverrideLNB.Checked;
      textBoxLNBSwitch.Enabled = chkOverrideLNB.Checked;
    }

    private void mpLNB1_CheckedChanged(object sender, EventArgs e)
    {
      mpTransponder1.Visible = mpBand1.Visible = mpDisEqc1.Visible = mpLNB1.Checked;
    }

    private void mpLNB2_CheckedChanged(object sender, EventArgs e)
    {
      mpTransponder2.Visible = mpBand2.Visible = mpDisEqc2.Visible = mpLNB2.Checked;
    }

    private void mpLNB3_CheckedChanged(object sender, EventArgs e)
    {
      mpTransponder3.Visible = mpBand3.Visible = mpDisEqc3.Visible = mpLNB3.Checked;
    }

    private void mpLNB4_CheckedChanged(object sender, EventArgs e)
    {
      mpTransponder4.Visible = mpBand4.Visible = mpDisEqc4.Visible = mpLNB4.Checked;
    }

    private void mpBand1_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      if (chkOverrideLNB.Checked)
        return;
      int lof1, lof2, sw;
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
      if (_ignoreCheckBoxCreateGroupsClickEvent)
        return;
      _ignoreCheckBoxCreateGroupsClickEvent = true;
      if (checkBoxCreateGroups.Checked)
      {
        checkBoxCreateGroups.Checked = false;
      }
      _ignoreCheckBoxCreateGroupsClickEvent = false;
    }

    private void checkBoxCreateGroups_CheckedChanged(object sender, EventArgs e)
    {
      if (_ignoreCheckBoxCreateGroupsClickEvent)
        return;
      _ignoreCheckBoxCreateGroupsClickEvent = true;
      if (checkBoxCreateGroupsSat.Checked)
      {
        checkBoxCreateGroupsSat.Checked = false;
      }
      _ignoreCheckBoxCreateGroupsClickEvent = false;
    }

    private void mpButtonManualScan_Click(object sender, EventArgs e)
    {

    }

    #region GUI handling
    /// <summary>
    /// Sets correct button state 
    /// </summary>
    private void SetButtonState()
    {
      //mpComboBoxCountry.Enabled = !_isScanning && ActiveScanType == ScanTypes.Predefined;
      //mpComboBoxRegion.Enabled = !_isScanning && ActiveScanType == ScanTypes.Predefined;
      textBoxFreq.Enabled = ActiveScanType != ScanTypes.Predefined;
      textBoxSymbolRate.Enabled = ActiveScanType != ScanTypes.Predefined;
      mpComboBoxMod.Enabled = ActiveScanType != ScanTypes.Predefined;
      mpComboBoxPolarisation.Enabled = ActiveScanType != ScanTypes.Predefined;

      mpButtonScanTv.Enabled = true;

      int forceProgress = 0;
      switch (scanState)
      {
        default:
        case ScanState.Initialized:
          mpButtonScanTv.Text = "Scan for channels";
          break;

        case ScanState.Scanning:
          mpButtonScanTv.Text = "Cancel...";
          break;

        case ScanState.Cancel:
          mpButtonScanTv.Text = "Cancelling...";
          break;

        case ScanState.Done:
          mpButtonScanTv.Text = "New scan";
          forceProgress = 1; // leave window open
          break;
        
        case ScanState.Updating:
          mpButtonScanTv.Text = "Scan for channels";
          mpButtonScanTv.Enabled = false;
          break;
      }

      ShowActiveGroup(forceProgress);
    }

    /// <summary>
    /// Show either scan option or progress
    /// </summary>
    /// <param name="ForceShowProgress">1 to force progress visible</param>
    private void ShowActiveGroup(int ForceShowProgress)
    {
      if (ForceShowProgress == 1)
      {
        mpGrpAdvancedTuning.Visible = false;
        mpGrpScanProgress.Visible = true;
        checkBoxAdvancedTuning.Enabled = false;
        checkEnableDVBS2.Enabled = false;
        buttonUpdate.Enabled = false;
      }
      else
      {
        mpGrpAdvancedTuning.Visible = checkBoxAdvancedTuning.Checked && !_isScanning;
        mpGrpScanProgress.Visible = _isScanning;
        checkBoxAdvancedTuning.Enabled = !_isScanning;
        checkEnableDVBS2.Enabled = !_isScanning;
        buttonUpdate.Enabled = !_isScanning;
      }

      if (mpGrpAdvancedTuning.Visible)
      {
        mpGrpAdvancedTuning.BringToFront();
      }
      if (mpGrpScanProgress.Visible)
      {
        mpGrpScanProgress.BringToFront();
      }
      UpdateZOrder();
      Application.DoEvents();
      Thread.Sleep(100);
    }

    private void UpdateGUIControls(object sender, EventArgs e)
    {
      SetButtonState();
    }
    #endregion

    private void StartScanThread()
    {
      Thread scanThread = new Thread(DoScan);
      scanThread.Name = "DVB-S scan thread";
      scanThread.Start();
    }

    private static Transponder ToTransonder(IChannel channel)
    {
      DVBSChannel ch = (DVBSChannel)channel;
      Transponder t = new Transponder();
      t.CarrierFrequency = Convert.ToInt32(ch.Frequency);
      t.InnerFecRate = ch.InnerFecRate;
      t.Modulation = ch.ModulationType;
      t.Pilot = ch.Pilot;
      t.Rolloff = ch.Rolloff;
      t.SymbolRate = ch.SymbolRate;
      t.Polarisation = ch.Polarisation;
      return t;
    }

    private DVBSChannel GetManualTuning()
    {
      DVBSChannel tuneChannel = new DVBSChannel();
      tuneChannel.Frequency = Int32.Parse(textBoxFreq.Text);
      tuneChannel.ModulationType = (ModulationType)mpComboBoxMod.SelectedIndex;
      tuneChannel.SymbolRate = Int32.Parse(textBoxSymbolRate.Text);
      tuneChannel.Polarisation = (Polarisation)mpComboBoxPolarisation.SelectedItem;
      return tuneChannel;
    }

    private void mpButtonSaveList_Click(object sender, EventArgs e)
    {
      SaveManualScanList();
    }

    private String SaveManualScanList()
    {
      _transponders.Sort();
      String filePath = String.Format(@"{0}\TuningParameters\dvbs\Manual_Scans.{1}.xml", Log.GetPathName(), DateTime.Now.ToString("yyyy-MM-dd"));
      System.IO.TextWriter parFileXML = System.IO.File.CreateText(filePath);
      XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<Transponder>));
      xmlSerializer.Serialize(parFileXML, _transponders);
      parFileXML.Close();
      Init();
      return Path.GetFileNameWithoutExtension(filePath);
    }

    private void mpButtonScanSingleTP_Click(object sender, EventArgs e)
    {
      DVBSChannel tuneChannel = GetManualTuning();
      Transponder t = ToTransonder(tuneChannel);
      _transponders.Add(t);
      StartScanThread();
    }

    private void checkBoxAdvancedTuning_CheckedChanged(object sender, EventArgs e)
    {
      mpGrpAdvancedTuning.Visible = checkBoxAdvancedTuning.Checked;
    }
  }
}
