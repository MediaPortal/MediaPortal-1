#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Xml;
using System.Net;
using DirectShowLib.BDA;
using System.Xml.Serialization;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupTV.Sections.CIMenu;
using Mediaportal.TV.Server.SetupTV.Sections.Helpers;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.Entities.Factories;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces.Device;
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.Interfaces.Services;
using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class CardDvbS : SectionSettings
  {
 
    #region private classes

    private class SatelliteContext : IComparable<SatelliteContext>
    {
      public string SatelliteName;
      public string Url;
      public string FileName;
      public Satellite Satellite;

      public SatelliteContext()
      {
        Url = "";
        Satellite = null;
        FileName = "";
        SatelliteName = "";
      }

      public String DisplayName
      {
        get { return System.IO.Path.GetFileNameWithoutExtension(FileName); }
      }

      public override string ToString()
      {
        return SatelliteName;
      }

      public int CompareTo(SatelliteContext other)
      {
        return SatelliteName.CompareTo(other.SatelliteName);
      }

      #region IComparable<SatelliteContext> Members

      int IComparable<SatelliteContext>.CompareTo(SatelliteContext other)
      {
        return SatelliteName.CompareTo(other.SatelliteName);
      }

      #endregion
    }

    [Serializable]
    public class Transponder : IComparable<Transponder>
    {
      public int CarrierFrequency; // frequency
      public Polarisation Polarisation; // polarisation 0=hori, 1=vert
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
          tuneChannel.Frequency = CarrierFrequency;
          tuneChannel.Polarisation = Polarisation;
          tuneChannel.SymbolRate = SymbolRate;
          tuneChannel.ModulationType = Modulation;
          tuneChannel.InnerFecRate = InnerFecRate;
          //Grab the Pilot & Roll-off settings
          tuneChannel.Pilot = Pilot;
          tuneChannel.RollOff = Rolloff;
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

    private readonly int _cardNumber;
    private List<Transponder> _transponders = new List<Transponder>();

    private int _tvChannelsNew;
    private int _radioChannelsNew;
    private int _tvChannelsUpdated;
    private int _radioChannelsUpdated;
    private bool _enableEvents;
    private bool _ignoreCheckBoxCreateGroupsClickEvent;
    private IUser _user;

    private CI_Menu_Dialog ciMenuDialog; // ci menu dialog object

    private ScanState scanState; // scan state

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
      : this("DVBS") {}

    public CardDvbS(string name)
      : base(name) {}

    public CardDvbS(string name, int cardNumber)
      : base(name)
    {
      _cardNumber = cardNumber;

      InitializeComponent();
      //insert complete ci menu dialog to tab
      Card dbCard = ServiceAgents.Instance.CardServiceAgent.GetCard(_cardNumber, CardIncludeRelationEnum.None);
      if (dbCard.UseConditionalAccess == true)
      {
        ciMenuDialog = new CI_Menu_Dialog(_cardNumber);
        tabPageCIMenu.Controls.Add(ciMenuDialog);
      }
      else
      {
        tabPageCIMenu.Dispose();
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
    private void DownloadTransponder(SatelliteContext context)
    {
      if (context.Url == null)
        return;
      if (context.Url.Length == 0)
        return;
      if (!context.Url.ToLowerInvariant().StartsWith("http://"))
        return;
      string itemLine = String.Format("Downloading transponders for: {0}", context.SatelliteName);
      ListViewItem item = listViewStatus.Items.Add(new ListViewItem(itemLine));
      item.EnsureVisible();
      Application.DoEvents();

      List<Transponder> transponders = new List<Transponder>();
      try
      {
        string[,] contextDownload = new string[2,2];
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
                        String[] tpdata = line.Split(new char[] {','});
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
                      catch {} // ignore parsing errors in single line (i.e. 19,2 Astra reading fails due split & parse)
                    }
                  }
                } while (line != null);

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
        String newPath = String.Format(@"{0}\TuningParameters\dvbs\{1}.xml", PathManager.GetDataPath,
                                       Path.GetFileNameWithoutExtension(context.FileName));
        if (File.Exists(newPath))
        {
          File.Delete(newPath);
        }
        if (transponders.Count > 0)
        {
          System.IO.TextWriter parFileXML = System.IO.File.CreateText(newPath);
          XmlSerializer xmlSerializer = new XmlSerializer(typeof (List<Transponder>));
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
    private void LoadTransponders(SatelliteContext context)
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
        XmlSerializer xmlSerializer = new XmlSerializer(typeof (List<Transponder>));
        _transponders = (List<Transponder>)xmlSerializer.Deserialize(parFileXML);
        parFileXML.Close();
        _transponders.Sort();
      }
      catch (Exception ex)
      {
        this.LogError(ex, "Error loading tuningdetails");
        MessageBox.Show("Transponder list could not be loaded, check error.log for details.");
      }
    }

    /// <summary>
    /// Loads all known satellites from xml file
    /// </summary>
    /// <returns></returns>
    private static List<SatelliteContext> LoadSatellites()
    {
      List<SatelliteContext> satellites = new List<SatelliteContext>();
      XmlDocument doc = new XmlDocument();
      doc.Load(String.Format(@"{0}\TuningParameters\dvbs\satellites.xml", PathManager.GetDataPath));
      XmlNodeList nodes = doc.SelectNodes("/satellites/satellite");
      if (nodes != null)
      {
        foreach (XmlNode node in nodes)
        {
          SatelliteContext ts = new SatelliteContext();
          ts.SatelliteName = node.Attributes.GetNamedItem("name").Value;
          ts.Url = node.Attributes.GetNamedItem("url").Value;
          string name = Utils.FilterFileName(ts.SatelliteName);
          ts.FileName = String.Format(@"{0}\TuningParameters\dvbs\{1}.xml", PathManager.GetDataPath, name);
          satellites.Add(ts);
        }
      }
      String[] files = System.IO.Directory.GetFiles(String.Format(@"{0}\TuningParameters\dvbs\", PathManager.GetDataPath),
                                                    "*.xml");
      foreach (String file in files)
      {
        if (Path.GetFileName(file).StartsWith("Manual_Scans"))
        {
          SatelliteContext ts = new SatelliteContext();
          ts.SatelliteName = Path.GetFileNameWithoutExtension(file);
          ts.Url = "";
          ts.FileName = file;
          satellites.Add(ts);
        }
      }
      IList<Satellite> dbSats = ServiceAgents.Instance.CardServiceAgent.ListAllSatellites(); 
      foreach (SatelliteContext ts in satellites)
      {
        foreach (Satellite dbSat in dbSats)
        {
          string name = "";
          for (int i = 0; i < ts.SatelliteName.Length; ++i)
          {
            if (ts.SatelliteName[i] >= (char)32 && ts.SatelliteName[i] < (char)127)
              name += ts.SatelliteName[i];
          }
          if (String.Compare(name, dbSat.SatelliteName, true) == 0)
          {
            ts.Satellite = dbSat;
            break;
          }
        }
        if (ts.Satellite == null)
        {
          string name = "";
          for (int i = 0; i < ts.SatelliteName.Length; ++i)
          {
            if (ts.SatelliteName[i] >= (char)32 && ts.SatelliteName[i] < (char)127)
              name += ts.SatelliteName[i];
          }
          

          ts.Satellite = new Satellite { SatelliteName = name, TransponderFileName = ts.FileName };
          ts.FileName = ts.FileName;

          ServiceAgents.Instance.CardServiceAgent.SaveSatellite(ts.Satellite);
        }
      }
      return satellites;
    }

    #endregion

    #region DVB-S scanning tab

    private void Init()
    {
      // set to same positions as progress
      mpGrpAdvancedTuning.Top = mpGrpScanProgress.Top;

      _enableEvents = false;

      int idx = 0;

      mpComboBoxPolarisation.Items.AddRange(new object[]
                                              {
                                                "Not Set",
                                                "Not Defined",
                                                "Horizontal",
                                                "Vertical",
                                                "Circular Left",
                                                "Circular Right"
                                              });
      mpComboBoxPolarisation.SelectedIndex = 2;

      mpComboBoxMod.Items.AddRange(new object[]
                                     {
                                       "Not Set",
                                       "Not Defined",
                                       "16 QAM",
                                       "32 QAM",
                                       "64 QAM",
                                       "80 QAM",
                                       "96 QAM",
                                       "112 QAM",
                                       "128 QAM",
                                       "160 QAM",
                                       "192 QAM",
                                       "224 QAM",
                                       "256 QAM",
                                       "320 QAM",
                                       "384 QAM",
                                       "448 QAM",
                                       "512 QAM",
                                       "640 QAM",
                                       "768 QAM",
                                       "896 QAM",
                                       "1024 QAM",
                                       "QPSK",
                                       "BPSK",
                                       "OQPSK",
                                       "8 VSB",
                                       "16 VSB",
                                       "Analog Amplitude",
                                       "Analog Frequency",
                                       "8 PSK",
                                       "RF",
                                       "16 APSK",
                                       "32 APSK",
                                       "QPSK (DVB-S2)",
                                       "8 PSK (DVB-S2)",
                                       "DirectTV"
                                     });
      mpComboBoxMod.SelectedIndex = 0;

      mpComboBoxInnerFecRate.Items.AddRange(new object[]
                                              {
                                                "Not Set",
                                                "Not Defined",
                                                "1/2",
                                                "2/3",
                                                "3/4",
                                                "3/5",
                                                "4/5",
                                                "5/6",
                                                "5/11",
                                                "7/8",
                                                "1/4",
                                                "1/3",
                                                "2/5",
                                                "6/7",
                                                "8/9",
                                                "9/10"
                                              });
      mpComboBoxInnerFecRate.SelectedIndex = 0;

      mpComboBoxPilot.Items.AddRange(new object[]
                                       {
                                         "Not Set",
                                         "Not Defined",
                                         "Off",
                                         "On"
                                       });
      mpComboBoxPilot.SelectedIndex = 0;

      mpComboBoxRollOff.Items.AddRange(new object[]
                                         {
                                           "Not Set",
                                           "Not Defined",
                                           ".20 Roll Off",
                                           ".25 Roll Off",
                                           ".35 Roll Off"
                                         });
      mpComboBoxRollOff.SelectedIndex = 0;
      //mpButtonSaveList.Enabled = (_transponders.Count != 0);

      //List<SimpleFileName> satellites = fileFilters.AllFiles;
      List<SatelliteContext> satellites = LoadSatellites();
      IList<LnbType> tempLnbTypes = ServiceAgents.Instance.CardServiceAgent.ListAllLnbTypes();
      LnbType[] lnbTypes = new LnbType[tempLnbTypes.Count];
      tempLnbTypes.CopyTo(lnbTypes, 0);

      MPComboBox[] mpTrans = new MPComboBox[] {mpTransponder1, mpTransponder2, mpTransponder3, mpTransponder4};
      MPComboBox[] mpComboDiseqc = new MPComboBox[] {mpComboDiseqc1, mpComboDiseqc2, mpComboDiseqc3, mpComboDiseqc4};
      MPComboBox[] mpComboLnbType = new MPComboBox[] {mpComboLnbType1, mpComboLnbType2, mpComboLnbType3, mpComboLnbType4};
      MPCheckBox[] mpLNBs = new MPCheckBox[] {mpLNB1, mpLNB2, mpLNB3, mpLNB4};
      MPComboBox curBox;
      MPCheckBox curCheck;
      for (int ctlIndex = 0; ctlIndex < 4; ctlIndex++)
      {
        idx = ctlIndex + 1;
        curBox = mpTrans[ctlIndex];
        curBox.Items.Clear();
        foreach (SatelliteContext ts in satellites)
        {
          curBox.Items.Add(ts);
        }
        if (curBox.Items.Count > 0)
        {
          int selIdx =
            Int32.Parse(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue(String.Format("dvbs{0}SatteliteContext{1}", _cardNumber, idx), "0").Value);
          if (selIdx < curBox.Items.Count)
          {
            curBox.SelectedIndex = selIdx;
          }
        }

        curBox = mpComboDiseqc[ctlIndex];
        curBox.Items.Clear();
        curBox.Items.AddRange(new object[] {
            "None",
            // Simple DiSEqC (burst)
            "Simple A (tone burst)",
            "Simple B (data burst)",
            // DiSEqC 1.0
            "Port A (option A, position A)",
            "Port B (option A, position B)",
            "Port C (option B, position A)",
            "Port D (option B, position B)",
            // DiSEqC 1.1+
            "Port 1",
            "Port 2",
            "Port 3",
            "Port 4",
            "Port 5",
            "Port 6",
            "Port 7",
            "Port 8",
            "Port 9",
            "Port 10",
            "Port 11",
            "Port 12",
            "Port 13",
            "Port 14",
            "Port 15",
            "Port 16"});
        curBox.SelectedIndex =
          Int32.Parse(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue(String.Format("dvbs{0}DiSEqC{1}", _cardNumber, idx), "0").Value);

        curBox = mpComboLnbType[ctlIndex];
        curBox.Items.Clear();
        curBox.Items.AddRange((object[])lnbTypes);
        if (curBox.Items.Count > 0)
        {
          curBox.SelectedIndex =
            Int32.Parse(ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue(String.Format("dvbs{0}band{1}", _cardNumber, idx), "0").Value);
        }

        curCheck = mpLNBs[ctlIndex];        
        curCheck.Checked = (ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue(String.Format("dvbs{0}LNB{1}", _cardNumber, idx), "0").Value == "true");
      }

      mpLNB1_CheckedChanged(null, null);
      mpLNB2_CheckedChanged(null, null);
      mpLNB3_CheckedChanged(null, null);
      mpLNB4_CheckedChanged(null, null);

      checkBoxCreateGroups.Checked = (ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("dvbs" + _cardNumber + "creategroups", "false").Value == "true");
      checkBoxCreateGroupsSat.Checked = (ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("dvbs" + _cardNumber + "creategroupssat", "false").Value ==
                                         "true");
      checkBoxCreateSignalGroup.Checked =
        (ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("dvbs" + _cardNumber + "createsignalgroup", "false").Value == "true");

      checkBoxEnableDVBS2.Checked = (ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("dvbs" + _cardNumber + "enabledvbs2", "false").Value == "true");


      _enableEvents = true;
      mpLNB1_CheckedChanged(null, null);

      checkBoxAdvancedTuning.Checked = false;
      checkBoxAdvancedTuning.Enabled = true;

      checkBoxCreateSignalGroup.Text = "\"" + TvConstants.TvGroupNames.DVBS + "\"";

      scanState = ScanState.Initialized;
      SetControlStates();
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
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("dvbs" + _cardNumber + "creategroups", checkBoxCreateGroups.Checked ? "true" : "false");
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("dvbs" + _cardNumber + "creategroupssat", checkBoxCreateGroupsSat.Checked ? "true" : "false");
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("dvbs" + _cardNumber + "createsignalgroup", checkBoxCreateSignalGroup.Checked ? "true" : "false");
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("dvbs" + _cardNumber + "SatteliteContext1", mpTransponder1.SelectedIndex.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("dvbs" + _cardNumber + "SatteliteContext2", mpTransponder2.SelectedIndex.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("dvbs" + _cardNumber + "SatteliteContext3", mpTransponder3.SelectedIndex.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("dvbs" + _cardNumber + "SatteliteContext4", mpTransponder4.SelectedIndex.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("dvbs" + _cardNumber + "DisEqc1", mpComboDiseqc1.SelectedIndex.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("dvbs" + _cardNumber + "DisEqc2", mpComboDiseqc2.SelectedIndex.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("dvbs" + _cardNumber + "DisEqc3", mpComboDiseqc3.SelectedIndex.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("dvbs" + _cardNumber + "DisEqc4", mpComboDiseqc4.SelectedIndex.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("dvbs" + _cardNumber + "band1", mpComboLnbType1.SelectedIndex.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("dvbs" + _cardNumber + "band2", mpComboLnbType2.SelectedIndex.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("dvbs" + _cardNumber + "band3", mpComboLnbType3.SelectedIndex.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("dvbs" + _cardNumber + "band4", mpComboLnbType4.SelectedIndex.ToString());
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("dvbs" + _cardNumber + "LNB1", mpLNB1.Checked ? "true" : "false");
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("dvbs" + _cardNumber + "LNB2", mpLNB2.Checked ? "true" : "false");
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("dvbs" + _cardNumber + "LNB3", mpLNB3.Checked ? "true" : "false");
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("dvbs" + _cardNumber + "LNB4", mpLNB4.Checked ? "true" : "false");
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("dvbs" + _cardNumber + "enabledvbs2", checkBoxEnableDVBS2.Checked ? "true" : "false");

    
    }

    private void UpdateStatus()
    {
      progressBarLevel.Value = Math.Min(100, ServiceAgents.Instance.ControllerServiceAgent.SignalLevel(_cardNumber));
      progressBarQuality.Value = Math.Min(100, ServiceAgents.Instance.ControllerServiceAgent.SignalQuality(_cardNumber));
      progressBarSatLevel.Value = Math.Min(100, ServiceAgents.Instance.ControllerServiceAgent.SignalLevel(_cardNumber));
      progressBarSatQuality.Value = Math.Min(100, ServiceAgents.Instance.ControllerServiceAgent.SignalQuality(_cardNumber));
      labelTunerLock.Text = ServiceAgents.Instance.ControllerServiceAgent.TunerLocked(_cardNumber) ? "Yes" : "No";
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
          SetControlStates();
          return;

        case ScanState.Initialized:
         SaveSettings();
          
          Card card = ServiceAgents.Instance.CardServiceAgent.GetCardByDevicePath(ServiceAgents.Instance.ControllerServiceAgent.CardDevice(_cardNumber));
          if (card.Enabled == false)
          {
            MessageBox.Show(this, "Tuner is disabled. Please enable the tuner before scanning.");
            return;
          }
          if (!ServiceAgents.Instance.ControllerServiceAgent.IsCardPresent(card.IdCard))
          {
            MessageBox.Show(this, "Tuner is not found. Please make sure the tuner is present before scanning.");
            return;
          }
          // Check if the card is locked for scanning.
          IUser user;
          if (ServiceAgents.Instance.ControllerServiceAgent.IsCardInUse(_cardNumber, out user))
          {
            MessageBox.Show(this,
                            "Tuner is locked. Scanning is not possible at the moment. Perhaps you are using another part of a hybrid card?");
            return;
          }

          StartScanThread();
          break;

        case ScanState.Scanning:
          scanState = ScanState.Cancel;
          SetControlStates();
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

    private void DoScan()
    {
      MethodInvoker updateControls = new MethodInvoker(SetControlStates);
      try
      {
        scanState = ScanState.Scanning;
        Invoke(updateControls);
        ServiceAgents.Instance.ControllerServiceAgent.EpgGrabberEnabled = false;

        listViewStatus.Items.Clear();
        _tvChannelsNew = 0;
        _radioChannelsNew = 0;
        _tvChannelsUpdated = 0;
        _radioChannelsUpdated = 0;

        if (mpLNB1.Checked)
          Scan(1, (LnbType)mpComboLnbType1.SelectedItem, (DiseqcPort)mpComboDiseqc1.SelectedIndex,
               (SatelliteContext)mpTransponder1.SelectedItem);
        if (scanState == ScanState.Cancel)
          return;

        if (mpLNB2.Checked)
          Scan(2, (LnbType)mpComboLnbType2.SelectedItem, (DiseqcPort)mpComboDiseqc2.SelectedIndex,
               (SatelliteContext)mpTransponder2.SelectedItem);
        if (scanState == ScanState.Cancel)
          return;

        if (mpLNB3.Checked)
          Scan(3, (LnbType)mpComboLnbType3.SelectedItem, (DiseqcPort)mpComboDiseqc3.SelectedIndex,
               (SatelliteContext)mpTransponder3.SelectedItem);
        if (scanState == ScanState.Cancel)
          return;

        if (mpLNB4.Checked)
          Scan(4, (LnbType)mpComboLnbType4.SelectedItem, (DiseqcPort)mpComboDiseqc4.SelectedIndex,
               (SatelliteContext)mpTransponder4.SelectedItem);

        listViewStatus.Items.Add(
          new ListViewItem(String.Format("Total radio channels new:{0} updated:{1}", _radioChannelsNew,
                                         _radioChannelsUpdated)));
        listViewStatus.Items.Add(
          new ListViewItem(String.Format("Total tv channels new:{0} updated:{1}", _tvChannelsNew, _tvChannelsUpdated)));
        ListViewItem item = listViewStatus.Items.Add(new ListViewItem("Scan done..."));
        item.EnsureVisible();
      }
      catch (Exception ex)
      {
        this.LogError(ex);
      }
      finally
      {
        IUser user = new User();
        user.CardId = _cardNumber;
        ServiceAgents.Instance.ControllerServiceAgent.StopCard(user.CardId);
        ServiceAgents.Instance.ControllerServiceAgent.EpgGrabberEnabled = true;
        progressBar1.Value = 100;
        scanState = ScanState.Done;
        Invoke(updateControls);
      }
    }

    private void Scan(int lnb, LnbType lnbType, DiseqcPort diseqc, SatelliteContext context)
    {
      // all transponders to scan
      List<DVBSChannel> _channels = new List<DVBSChannel>();

      // get default sat position from DB

      Card card = ServiceAgents.Instance.CardServiceAgent.GetCardByDevicePath(ServiceAgents.Instance.ControllerServiceAgent.CardDevice(_cardNumber));

      int position = -1;
      Setting setting = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("dvbs" + _cardNumber + "motorEnabled", "no");
      if (setting.Value == "yes")
      {
        foreach (DisEqcMotor motor in card.DisEqcMotors)
        {
          if (motor.IdSatellite == context.Satellite.IdSatellite)
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
            _channels.Add(curChannel);
          }
          break;

          // scan Network Information Table for transponder info
        case ScanTypes.NIT:
          _transponders.Clear();
          DVBSChannel tuneChannel = GetManualTuning();
          tuneChannel.Diseqc = diseqc;
          tuneChannel.LnbType = lnbType;
          tuneChannel.SatelliteIndex = position;

          listViewStatus.Items.Clear();
          string line = String.Format("lnb:{0} {1}tp- {2} {3} {4}", lnb, 1, tuneChannel.Frequency,
                                      tuneChannel.Polarisation, tuneChannel.SymbolRate);
          ListViewItem item = listViewStatus.Items.Add(new ListViewItem(line));
          item.EnsureVisible();

          IChannel[] channels = ServiceAgents.Instance.ControllerServiceAgent.ScanNIT(_cardNumber, tuneChannel);
          if (channels != null)
          {
            for (int i = 0; i < channels.Length; ++i)
            {
              DVBSChannel curChannel = (DVBSChannel)channels[i];
              _channels.Add(curChannel);
              item = listViewStatus.Items.Add(new ListViewItem(curChannel.ToString()));
              item.EnsureVisible();
            }
          }

          ListViewItem lastItem =
            listViewStatus.Items.Add(
              new ListViewItem(String.Format("Scan done, found {0} transponders...", _channels.Count)));
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

      IUser user = new User();
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

        // If this is a DVB-S2 transponder and DVB-S2
        // scanning is not enabled then skip it. Note that
        // a roll-off of .35 is the default for standard
        // DVB-S.
        if ((tuneChannel.RollOff == RollOff.Twenty ||
             tuneChannel.RollOff == RollOff.TwentyFive ||
             tuneChannel.Pilot == Pilot.On) &&
            !checkBoxEnableDVBS2.Checked)
        {
          continue;
        }

        scanIndex++;

        tuneChannel.Diseqc = diseqc;
        tuneChannel.LnbType = lnbType;
        tuneChannel.SatelliteIndex = position;
        string line = String.Format("lnb:{0} {1}tp- {2} {3} {4}", lnb, 1 + index, tuneChannel.Frequency,
                                    tuneChannel.Polarisation, tuneChannel.SymbolRate);
        ListViewItem item = listViewStatus.Items.Add(new ListViewItem(line));
        item.EnsureVisible();

        if (scanIndex == 1) // first scanned
        {
          ServiceAgents.Instance.ControllerServiceAgent.Scan(user.Name, user.CardId, out user, tuneChannel, -1);
        }
        UpdateStatus();

        IChannel[] channels = ServiceAgents.Instance.ControllerServiceAgent.Scan(_cardNumber, tuneChannel);

        UpdateStatus();

        if (channels == null || channels.Length == 0)
        {
          if (ServiceAgents.Instance.ControllerServiceAgent.TunerLocked(_cardNumber) == false)
          {
            line = String.Format("lnb:{0} {1}tp- {2} {3} {4}:No signal", lnb, scanIndex, tuneChannel.Frequency,
                                 tuneChannel.Polarisation, tuneChannel.SymbolRate);
            item.Text = line;
            item.ForeColor = Color.Red;
            continue;
          }
          line = String.Format("lnb:{0} {1}tp- {2} {3} {4}:Nothing found", lnb, scanIndex, tuneChannel.Frequency,
                               tuneChannel.Polarisation, tuneChannel.SymbolRate);
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
          bool exists;
          TuningDetail currentDetail;
          //Check if we already have this tuningdetail. The user has the option to enable channel move detection...
          if (checkBoxEnableChannelMoveDetection.Checked)
          {
            //According to the DVB specs ONID + SID is unique, therefore we do not need to use the TSID to identify a service.
            //The DVB spec recommends that the SID should not change if a service moves. This theoretically allows us to
            //track channel movements.
            TuningDetailSearchEnum tuningDetailSearchEnum = TuningDetailSearchEnum.NetworkId;
            tuningDetailSearchEnum |= TuningDetailSearchEnum.ServiceId;
            currentDetail = ServiceAgents.Instance.ChannelServiceAgent.GetTuningDetailCustom(channel, tuningDetailSearchEnum);                             
          }
          else
          {
            //There are certain providers that do not maintain unique ONID + SID combinations.
            //In those cases, ONID + TSID + SID is generally unique. The consequence of using the TSID to identify
            //a service is that channel movement tracking won't work (each transponder/mux should have its own TSID).
            currentDetail = ServiceAgents.Instance.ChannelServiceAgent.GetTuningDetail(channel);
          }

          if (currentDetail == null)
          {
            //add new channel
            exists = false;
            dbChannel = ChannelFactory.CreateChannel(channel.Name);
            dbChannel.SortOrder = 10000;
            if (channel.LogicalChannelNumber >= 1)
            {
              dbChannel.SortOrder = channel.LogicalChannelNumber;
            }
            dbChannel.MediaType = (int)channel.MediaType;
            dbChannel = ServiceAgents.Instance.ChannelServiceAgent.SaveChannel(dbChannel);
            dbChannel.AcceptChanges();
          }
          else
          {
            exists = true;
            dbChannel = currentDetail.Channel;
          }

          if (dbChannel.MediaType == (int)MediaTypeEnum.TV)
          {
            ChannelGroup group = ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(TvConstants.TvGroupNames.AllChannels, MediaTypeEnum.TV);
            MappingHelper.AddChannelToGroup(ref dbChannel, @group);
            if (checkBoxCreateSignalGroup.Checked)
            {
              group = ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(TvConstants.TvGroupNames.DVBS, MediaTypeEnum.TV);
              MappingHelper.AddChannelToGroup(ref dbChannel, @group);
            }
            if (checkBoxCreateGroupsSat.Checked)
            {
              group = ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(context.Satellite.SatelliteName, MediaTypeEnum.TV);
              MappingHelper.AddChannelToGroup(ref dbChannel, @group);
            }
            if (checkBoxCreateGroups.Checked)
            {
              group = ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(channel.Provider, MediaTypeEnum.TV);
              MappingHelper.AddChannelToGroup(ref dbChannel, @group);
            }
          }
          if (dbChannel.MediaType == (int)MediaTypeEnum.Radio)
          {
            ChannelGroup group = ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(TvConstants.RadioGroupNames.AllChannels, MediaTypeEnum.Radio);
            MappingHelper.AddChannelToGroup(ref dbChannel, @group);
            if (checkBoxCreateSignalGroup.Checked)
            {
              group = ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(TvConstants.RadioGroupNames.DVBS, MediaTypeEnum.Radio);
              MappingHelper.AddChannelToGroup(ref dbChannel, @group);
            }
            if (checkBoxCreateGroupsSat.Checked)
            {
              group = ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(context.Satellite.SatelliteName, MediaTypeEnum.Radio);
              MappingHelper.AddChannelToGroup(ref dbChannel, @group);
            }
            if (checkBoxCreateGroups.Checked)
            {
              group = ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(channel.Provider, MediaTypeEnum.Radio);
              MappingHelper.AddChannelToGroup(ref dbChannel, @group);
            }
          }

          if (currentDetail == null)
          {
            channel.SatelliteIndex = position; // context.Satellite.IdSatellite;
            ServiceAgents.Instance.ChannelServiceAgent.AddTuningDetail(dbChannel.IdChannel, channel);
          }
          else
          {
            //update tuning details...
            channel.SatelliteIndex = position; // context.Satellite.IdSatellite;
            currentDetail.SatIndex = position; //context.Satellite.IdSatellite;
            ServiceAgents.Instance.ChannelServiceAgent.UpdateTuningDetail(dbChannel.IdChannel, currentDetail.IdTuning, channel);
          }
          if (channel.MediaType == MediaTypeEnum.TV)
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
          if (channel.MediaType == MediaTypeEnum.Radio)
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
          MappingHelper.AddChannelToCard(dbChannel, card, false);
          line = String.Format("lnb:{0} {1}tp- {2} {3} {4}:New:{5} Updated:{6}",
                               lnb, 1 + index, tuneChannel.Frequency, tuneChannel.Polarisation, tuneChannel.SymbolRate,
                               newChannels, updatedChannels);
          item.Text = line;
        }
      }
    }

    private void CardDvbS_Load(object sender, EventArgs e) {}

    #endregion

    #region DiSEqC Motor tab

    private void SetupMotor()
    {
      _enableEvents = false;

      Setting setting = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("dvbs" + _cardNumber + "motorEnabled", "no");
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

      setting = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("dvbs" + _cardNumber + "motorStepSize", "10");
      int stepsize;
      if (Int32.TryParse(setting.Value, out stepsize))
        comboBoxStepSize.SelectedIndex = stepsize - 1;
      else
        comboBoxStepSize.SelectedIndex = 9;

      comboBoxSat.Items.Clear();

      setting = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("dvbs" + _cardNumber + "selectedMotorSat", "0");
      int index;
      Int32.TryParse(setting.Value, out index);

      List<SatelliteContext> satellites = LoadSatellites();

      foreach (SatelliteContext sat in satellites)
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
      ServiceAgents.Instance.ControllerServiceAgent.DiSEqCDriveMotor(_cardNumber, DiseqcDirection.West,
                                              (byte)(1 + comboBoxStepSize.SelectedIndex));
      comboBox1_SelectedIndexChanged(null, null); //tune..;
    }

    private void buttonSetWestLimit_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;
      //set motor west limit
      ServiceAgents.Instance.ControllerServiceAgent.DiSEqCSetWestLimit(_cardNumber);
    }

    private void tabPage2_Click(object sender, EventArgs e) {}

    private void button1_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      //goto selected sat
      if (comboBoxSat.SelectedIndex < 0)
        return;
      if (checkBox1.Checked == false)
        return;
      SatelliteContext sat = (SatelliteContext)comboBoxSat.Items[comboBoxSat.SelectedIndex];

      Card card = ServiceAgents.Instance.CardServiceAgent.GetCard(_cardNumber);
      IList<DisEqcMotor> motorSettings = card.DisEqcMotors;
      foreach (DisEqcMotor motor in motorSettings)
      {
        if (motor.IdSatellite == sat.Satellite.IdSatellite)
        {
          ServiceAgents.Instance.ControllerServiceAgent.DiSEqCGotoPosition(_cardNumber, (byte)motor.Position);
          MessageBox.Show("Satellite moving to position:" + motor.Position, "Info", MessageBoxButtons.OK,
                          MessageBoxIcon.Information);
          comboBox1_SelectedIndexChanged(null, null);
          return;
        }
      }
      MessageBox.Show("No position stored for this satellite", "Warning", MessageBoxButtons.OK,
                      MessageBoxIcon.Exclamation);
    }

    private void buttonStore_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;
      //store motor position..
      int index = -1;
      SatelliteContext sat = (SatelliteContext)comboBoxSat.SelectedItem;
      Card card = ServiceAgents.Instance.CardServiceAgent.GetCard(_cardNumber);
      IList<DisEqcMotor> motorSettings = card.DisEqcMotors;
      foreach (DisEqcMotor motor in motorSettings)
      {
        if (motor.IdSatellite == sat.Satellite.IdSatellite)
        {
          index = motor.Position;
          break;
        }
      }
      if (index < 0)
      {
        index = motorSettings.Count + 1;
        DisEqcMotor motor = new DisEqcMotor();
        motor.IdCard = card.IdCard;
        motor.IdSatellite = sat.Satellite.IdSatellite;
        motor.Position = index;
        ServiceAgents.Instance.CardServiceAgent.SaveDisEqcMotor(motor);

      }
      ServiceAgents.Instance.ControllerServiceAgent.DiSEqCStorePosition(_cardNumber, (byte)(index));
      MessageBox.Show("Satellite position stored to:" + index, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void buttonMoveEast_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;
      //move motor east
      ServiceAgents.Instance.ControllerServiceAgent.DiSEqCDriveMotor(_cardNumber, DiseqcDirection.East,
                                              (byte)(1 + comboBoxStepSize.SelectedIndex));
      comboBox1_SelectedIndexChanged(null, null); //tune..
    }

    private void buttonSetEastLimit_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;
      //set motor east limit
      ServiceAgents.Instance.ControllerServiceAgent.DiSEqCSetEastLimit(_cardNumber);
    }

    private void comboBoxSat_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;

      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("dvbs" + _cardNumber + "selectedMotorSat", comboBoxSat.SelectedIndex.ToString());
      LoadMotorTransponder();
      comboBox1_SelectedIndexChanged(null, null);
    }


    private void checkBoxEnabled_CheckedChanged(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;
      if (checkBoxEnabled.Checked)
      {
        ServiceAgents.Instance.ControllerServiceAgent.DiSEqCForceLimit(_cardNumber, true);
        ServiceAgents.Instance.SettingServiceAgent.SaveSetting("dvbs" + _cardNumber + "limitsEnabled", "yes");
      }
      else
      {
        if (
          MessageBox.Show("Disabling the east/west limits could damage your dish!!! Are you sure?", "Warning",
                          MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
        {
          ServiceAgents.Instance.ControllerServiceAgent.DiSEqCForceLimit(_cardNumber, false);
          ServiceAgents.Instance.SettingServiceAgent.SaveSetting("dvbs" + _cardNumber + "limitsEnabled", "no");

        }
        else
        {
          _enableEvents = false;
          checkBoxEnabled.Checked = true;
          ServiceAgents.Instance.ControllerServiceAgent.DiSEqCForceLimit(_cardNumber, true);
          ServiceAgents.Instance.SettingServiceAgent.SaveSetting("dvbs" + _cardNumber + "limitsEnabled", "yes");
          _enableEvents = true;
        }
      }
    }
    private void LoadMotorTransponder()
    {
      Setting setting = ServiceAgents.Instance.SettingServiceAgent.GetSettingWithDefaultValue("dvbs" + _cardNumber + "limitsEnabled", "yes");
      if (setting.Value == "yes")
        checkBoxEnabled.Checked = true;
      if (setting.Value == "no")
        checkBoxEnabled.Checked = false;
      comboBox1.Items.Clear();
      SatelliteContext sat = (SatelliteContext)comboBoxSat.SelectedItem;
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
      tuneChannel.RollOff = transponder.Rolloff;
      tuneChannel.InnerFecRate = transponder.InnerFecRate;
      tuneChannel.Diseqc = DiseqcPort.None;
      if (mpComboLnbType1.SelectedIndex >= 0)
        tuneChannel.LnbType = (LnbType)mpComboLnbType1.SelectedItem;
      if (mpComboDiseqc1.SelectedIndex >= 0)
        tuneChannel.Diseqc = (DiseqcPort)mpComboDiseqc1.SelectedIndex;
      _user.CardId = _cardNumber;
      ServiceAgents.Instance.ControllerServiceAgent.StopCard(_user.CardId);
      _user.CardId = _cardNumber;
      ServiceAgents.Instance.ControllerServiceAgent.Tune(_user.Name, _user.CardId, out _user, tuneChannel, -1);
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
      ServiceAgents.Instance.ControllerServiceAgent.DiSEqCStopMotor(_cardNumber);
      comboBox1_SelectedIndexChanged(null, null);
    }

    private void buttonGotoStart_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;
      ServiceAgents.Instance.ControllerServiceAgent.DiSEqCGotoReferencePosition(_cardNumber);
      comboBox1_SelectedIndexChanged(null, null);
    }

    private void buttonUp_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;
      //move motor up
      ServiceAgents.Instance.ControllerServiceAgent.DiSEqCDriveMotor(_cardNumber, DiseqcDirection.Up,
                                              (byte)(1 + comboBoxStepSize.SelectedIndex));
    }

    private void buttonDown_Click(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;
      //move motor up
      ServiceAgents.Instance.ControllerServiceAgent.DiSEqCDriveMotor(_cardNumber, DiseqcDirection.Down,
                                              (byte)(1 + comboBoxStepSize.SelectedIndex));
    }

    private void comboBoxStepSize_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (_enableEvents == false)
        return;
      if (checkBox1.Checked == false)
        return;
      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("dvbs" + _cardNumber + "motorStepSize", String.Format("{0}", (1 + comboBoxStepSize.SelectedIndex)));            
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

      ServiceAgents.Instance.SettingServiceAgent.SaveSetting("dvbs" + _cardNumber + "motorEnabled", checkBox1.Checked ? "yes" : "no");      
    }

    private bool reentrant;
    private DateTime _signalTimer = DateTime.MinValue;

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

          ServiceAgents.Instance.ControllerServiceAgent.UpdateSignalSate(_cardNumber);
          _signalTimer = DateTime.Now;
          int satPos, stepsAzimuth, stepsElevation;
          ServiceAgents.Instance.ControllerServiceAgent.DiSEqCGetPosition(_cardNumber, out satPos, out stepsAzimuth, out stepsElevation);
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
              offset = offset.Length != 0
                         ? String.Format("{0}, {1} steps up", offset, -stepsElevation)
                         : String.Format("{0} steps up", -stepsElevation);
            }
            else if (stepsElevation > 0)
            {
              offset = offset.Length != 0
                         ? String.Format("{0}, {1} steps down", offset, stepsElevation)
                         : String.Format("{0} steps down", stepsElevation);
            }
            labelCurrentPosition.Text = offset.Length > 0
                                          ? String.Format("{0} of {1}", offset, satPosition)
                                          : satPosition;
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
      ServiceAgents.Instance.ControllerServiceAgent.DiSEqCReset(_cardNumber);
    }

    #endregion

    private void buttonUpdate_Click(object sender, EventArgs e)
    {
      scanState = ScanState.Updating;
      SetControlStates();
      listViewStatus.Items.Clear();
      string itemLine = String.Format("Updating satellites...");
      listViewStatus.Items.Add(new ListViewItem(itemLine));
      Application.DoEvents();
      List<SatelliteContext> sats = LoadSatellites();
      foreach (SatelliteContext sat in sats)
      {
        DownloadTransponder(sat);
      }
      itemLine = String.Format("Update finished");
      listViewStatus.Items.Add(new ListViewItem(itemLine));
      scanState = ScanState.Initialized;
      SetControlStates();
    }

    #region LNB selection tab

    private void mpLNB1_CheckedChanged(object sender, EventArgs e)
    {
      mpTransponder1.Visible = mpComboLnbType1.Visible = mpComboDiseqc1.Visible = mpLNB1.Checked;
    }

    private void mpLNB2_CheckedChanged(object sender, EventArgs e)
    {
      mpTransponder2.Visible = mpComboLnbType2.Visible = mpComboDiseqc2.Visible = mpLNB2.Checked;
    }

    private void mpLNB3_CheckedChanged(object sender, EventArgs e)
    {
      mpTransponder3.Visible = mpComboLnbType3.Visible = mpComboDiseqc3.Visible = mpLNB3.Checked;
    }

    private void mpLNB4_CheckedChanged(object sender, EventArgs e)
    {
      mpTransponder4.Visible = mpComboLnbType4.Visible = mpComboDiseqc4.Visible = mpLNB4.Checked;
    }

    private void checkEnableDVBS2_CheckedChanged(object sender, EventArgs e)
    {
      mpComboBoxPilot.Enabled = false;
      mpComboBoxRollOff.Enabled = false;
      if (checkBoxEnableDVBS2.Enabled && checkBoxEnableDVBS2.Checked && ActiveScanType != ScanTypes.Predefined)
      {
        mpComboBoxPilot.Enabled = true;
        mpComboBoxRollOff.Enabled = true;
      }
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

    private void mpButtonManualScan_Click(object sender, EventArgs e) {}

    #region GUI handling

    /// <summary>
    /// Sets correct visibility and enabled states for UI controls.
    /// </summary>
    private void SetControlStates()
    {
      int gotoView = 1;
      switch (scanState)
      {
        default:
        case ScanState.Initialized:
          if (mpButtonScanTv.Text != "Scan for channels" || !mpButtonScanTv.Enabled)
          {
            mpButtonScanTv.Enabled = true;
          }
          mpButtonScanTv.Text = "Scan for channels";
          gotoView = 0;
          break;

        case ScanState.Scanning:
          mpButtonScanTv.Text = "Cancel...";
          break;

        case ScanState.Cancel:
          mpButtonScanTv.Text = "Cancelling...";
          break;

        case ScanState.Done:
          mpButtonScanTv.Text = "New scan";
          break;

        case ScanState.Updating:
          mpButtonScanTv.Text = "Scan for channels";
          mpButtonScanTv.Enabled = false;
          break;
      }

      bool enableScanControls = scanState == ScanState.Initialized;
      Control[] scanControls = new Control[]
                                 {
                                   mpLNB1, mpLNB2, mpLNB3, mpLNB4,
                                   mpComboDiseqc1, mpComboDiseqc2, mpComboDiseqc3, mpComboDiseqc4,
                                   mpComboLnbType1, mpComboLnbType2, mpComboLnbType3, mpComboLnbType4,
                                   mpTransponder1, mpTransponder2, mpTransponder3, mpTransponder4,
                                   checkBoxCreateGroupsSat, checkBoxCreateGroups, checkBoxCreateSignalGroup,
                                   checkBoxEnableDVBS2, checkBoxEnableChannelMoveDetection, checkBoxAdvancedTuning,
                                   buttonUpdate
                                 };
      for (int ctlIndex = 0; ctlIndex < scanControls.Length; ctlIndex++)
      {
        scanControls[ctlIndex].Enabled = enableScanControls;
      }

      bool enableNonPredef = scanState == ScanState.Initialized && ActiveScanType != ScanTypes.Predefined;
      Control[] nonPredefControls = new Control[]
                                      {
                                        textBoxFreq, textBoxSymbolRate,
                                        mpComboBoxPolarisation, mpComboBoxMod,
                                        mpComboBoxInnerFecRate
                                      };
      for (int ctlIndex = 0; ctlIndex < nonPredefControls.Length; ctlIndex++)
      {
        nonPredefControls[ctlIndex].Enabled = enableNonPredef;
      }

      // Only give access to the DVB-S2 fields when DVB-S2 scanning
      // is enabled - helps prevent users who don't know what the
      // fields are for from doing something they shouldn't.
      mpComboBoxPilot.Enabled = false;
      mpComboBoxRollOff.Enabled = false;
      if (checkBoxEnableDVBS2.Enabled && checkBoxEnableDVBS2.Checked && ActiveScanType != ScanTypes.Predefined)
      {
        mpComboBoxPilot.Enabled = true;
        mpComboBoxRollOff.Enabled = true;
      }

      SwitchToView(gotoView);
    }

    /// <summary>
    /// Show either scan parameters or scan progress.
    /// </summary>
    /// <param name="view">0 for parameters, 1 for progress</param>
    private void SwitchToView(int view)
    {
      if (view == 1)
      {
        mpGrpAdvancedTuning.Visible = false;
        mpGrpScanProgress.Visible = true;
      }
      else
      {
        mpGrpAdvancedTuning.Visible = checkBoxAdvancedTuning.Checked;
        mpGrpScanProgress.Visible = false;
      }

      if (mpGrpScanProgress.Visible)
      {
        mpGrpAdvancedTuning.SendToBack();
        mpGrpScanProgress.BringToFront();
      }
      else
      {
        mpGrpScanProgress.SendToBack();
        mpGrpAdvancedTuning.BringToFront();
      }
      UpdateZOrder();
      Application.DoEvents();
      Thread.Sleep(100);
    }

    private void UpdateGUIControls(object sender, EventArgs e)
    {
      SetControlStates();
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
      t.Rolloff = ch.RollOff;
      t.SymbolRate = ch.SymbolRate;
      t.Polarisation = ch.Polarisation;
      return t;
    }

    private DVBSChannel GetManualTuning()
    {
      DVBSChannel tuneChannel = new DVBSChannel();
      tuneChannel.Frequency = Int32.Parse(textBoxFreq.Text);
      tuneChannel.SymbolRate = Int32.Parse(textBoxSymbolRate.Text);
      tuneChannel.Polarisation = (Polarisation)mpComboBoxPolarisation.SelectedIndex - 1;
      tuneChannel.ModulationType = (ModulationType)mpComboBoxMod.SelectedIndex - 1;
      tuneChannel.InnerFecRate = (BinaryConvolutionCodeRate)mpComboBoxInnerFecRate.SelectedIndex - 1;
      if (checkBoxEnableDVBS2.Checked)
      {
        tuneChannel.Pilot = (Pilot)mpComboBoxPilot.SelectedIndex - 1;
        tuneChannel.RollOff = (RollOff)mpComboBoxRollOff.SelectedIndex - 1;
      }
      else
      {
        tuneChannel.Pilot = Pilot.NotSet;
        tuneChannel.RollOff = RollOff.NotSet;
      }
      return tuneChannel;
    }

    private void mpButtonSaveList_Click(object sender, EventArgs e)
    {
      SaveManualScanList();
    }

    private String SaveManualScanList()
    {
      _transponders.Sort();
      String filePath = String.Format(@"{0}\TuningParameters\dvbs\Manual_Scans.{1}.xml", PathManager.GetDataPath,
                                      DateTime.Now.ToString("yyyy-MM-dd"));
      System.IO.TextWriter parFileXML = System.IO.File.CreateText(filePath);
      XmlSerializer xmlSerializer = new XmlSerializer(typeof (List<Transponder>));
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
      SetControlStates();
    }

    private void mpCombo_MouseHover(object sender, EventArgs e)
    {
      toolTip1.SetToolTip((Control)sender, ((MPComboBox)sender).SelectedItem.ToString());
    }
  }
}