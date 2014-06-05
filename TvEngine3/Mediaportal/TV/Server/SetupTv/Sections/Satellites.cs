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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;
using Mediaportal.TV.Server.SetupTV.Dialogs;
using Mediaportal.TV.Server.SetupTV.Sections.Helpers;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.SetupTV.Sections
{
  public partial class Satellites : SectionSettings
  {        
    public Satellites(string name) : base(name)
    {
     
    }    

    public override void OnSectionActivated()
    {      
      base.OnSectionActivated();
      RefreshAll();
    }

    private IList<Satellite> _satellites;

    private void RefreshAll()
    {
      mpListViewSatellites.BeginUpdate();
      try
      {
        _satellites = LoadSatellites();

        mpListViewSatellites.Items.Clear();


        if (_satellites.Count == 0)
        {
          MessageBox.Show(this, "No satellite transponder files found. Try and click the update button!", "Missing satellites");
          return;
        }

        foreach (Satellite sat in _satellites)
        {
          ListViewItem item = CreateListViewItem(sat);
          mpListViewSatellites.Items.Add(item);
        }
      }
      finally
      {
        mpListViewSatellites.EndUpdate();
      }
    }

    /// <summary>
    /// Loads all known satellites from xml file
    /// </summary>
    /// <returns></returns>
    private List<Satellite> LoadSatellites()
    {
      var existingSatellites = ServiceAgents.Instance.CardServiceAgent.ListAllSatellites();

      List<Satellite> newSatellites = new List<Satellite>();
      XmlDocument doc = new XmlDocument();
      doc.Load(String.Format(@"{0}\TuningParameters\dvbs\satellites.xml", PathManager.GetDataPath));
      XmlNodeList nodes = doc.SelectNodes("/satellites/satellite");
      if (nodes != null)
      {
        foreach (XmlNode node in nodes)
        {
          var newSat = new Satellite
          {
            Name = node.Attributes.GetNamedItem("name").Value,
            TransponderListUrl = node.Attributes.GetNamedItem("url").Value
          };
          string name = Utils.FilterFileName(newSat.Name);
          newSat.LocalTransponderFile = String.Format(@"{0}\TuningParameters\dvbs\{1}.xml", PathManager.GetDataPath, name);
          newSatellites.Add(newSat);
        }
      }
      String[] files = System.IO.Directory.GetFiles(String.Format(@"{0}\TuningParameters\dvbs\", PathManager.GetDataPath),
                                                    "*.xml");
      foreach (String file in files)
      {
        if (Path.GetFileName(file).StartsWith("Manual_Scans"))
        {
          var newSat = new Satellite
          {
            Name = Path.GetFileNameWithoutExtension(file),
            TransponderListUrl = "",
            LocalTransponderFile = file
          };
          newSatellites.Add(newSat);
        }
      }

      for (int j = 0; j < newSatellites.Count; j++)
      {
        Satellite newSat = newSatellites[j];      
        foreach (Satellite existingSat in existingSatellites)
        {
          string name = "";
          for (int i = 0; i < newSat.Name.Length; ++i)
          {
            if (newSat.Name[i] >= (char) 32 && newSat.Name[i] < (char) 127)
            {
              name += newSat.Name[i];
            }
          }
          if (String.Compare(name, existingSat.Name, System.StringComparison.OrdinalIgnoreCase) == 0)
          {
            newSat = existingSat;
            //newSat.Satellite = existingSat;
            break;
          }
        }
        //if (newSat.Satellite == null)
        if (newSat.IdSatellite == 0)
        {
          string name = "";
          for (int i = 0; i < newSat.Name.Length; ++i)
          {
            if (newSat.Name[i] >= (char)32 && newSat.Name[i] < (char)127)
            {
              name += newSat.Name[i];
            }
          }

          newSat.Name = name;                    
          ServiceAgents.Instance.CardServiceAgent.SaveSatellite(newSat);
        }
      }
      return newSatellites;
    }

    private ListViewItem CreateListViewItem(Satellite sat)
    {
      var item = new ListViewItem(sat.Name) {Tag = sat};

      item.SubItems.Add(sat.LocalTransponderFile);
      item.SubItems.Add(sat.Position.ToString(CultureInfo.InvariantCulture));      
      item.SubItems.Add(sat.TransponderListUrl);
      return item;
    }

    private void btnAddSat_Click(object sender, EventArgs e)
    {
      var dlg = new FormSatellite {Satellite = null};
      if (dlg.ShowDialog(this) == DialogResult.OK)
      {
        mpListViewSatellites.BeginUpdate();
        try
        {
          RefreshAll();
        }
        finally
        {
          mpListViewSatellites.EndUpdate();
        }
      }
    }

    private void btnEditSat_Click(object sender, EventArgs e)
    {
      ListView.SelectedIndexCollection indexes = mpListViewSatellites.SelectedIndices;
      if (indexes.Count == 0)
      {
        return;
      }
      Satellite satellite = (Satellite)mpListViewSatellites.Items[indexes[0]].Tag;
      FormSatellite dlg = new FormSatellite { Satellite = satellite };
      if (dlg.ShowDialog(this) == DialogResult.OK)
      {
        RefreshAll();        
      }
    }

    private void btnUpdateList_Click(object sender, EventArgs e)
    {
      string itemLine = String.Format("Updating satellites...");
      listViewStatus.Items.Add(new ListViewItem(itemLine));
      Application.DoEvents();
      DownloadSatelliteXML();
      _satellites = LoadSatellites();
      foreach (Satellite sat in _satellites)
      {
        DownloadTransponder(sat);
      }
      itemLine = String.Format("Update finished");
      listViewStatus.Items.Add(new ListViewItem(itemLine));
     
    }

    /// <summary>
    /// Downloads new Satellite XML 
    /// </summary>
    /// <param name="context"></param>
    private void DownloadSatelliteXML()
    {
      var satellite = new Satellite();
      satellite.TransponderListUrl = "http://install.team-mediaportal.com/tvsetup/DVBS/satellites.xml";
      satellite.LocalTransponderFile = String.Format(@"{0}\TuningParameters\dvbs\satellites.xml", PathManager.GetDataPath);
      if (satellite.TransponderListUrl == null)
        return;
      if (satellite.TransponderListUrl.Length == 0)
        return;
      if (!satellite.TransponderListUrl.ToLowerInvariant().StartsWith("http://"))
        return;
      string itemLine = String.Format("Downloading satellite xml");
      ListViewItem item = listViewStatus.Items.Add(new ListViewItem(itemLine));
      item.EnsureVisible();
      Application.DoEvents();

      try
      {
        string[,] contextDownload = new string[2, 2];
        contextDownload[0, 0] = satellite.LocalTransponderFile;
        contextDownload[0, 1] = satellite.TransponderListUrl;
        contextDownload[1, 1] = satellite.TransponderListUrl.Replace(".ini", "-S2.ini");
        {
          string satUrl = contextDownload[1, 1];
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

            String newPath = String.Format(@"{0}\TuningParameters\dvbs\{1}.xml", PathManager.GetDataPath,
                         Path.GetFileNameWithoutExtension(satellite.LocalTransponderFile));

            if (File.Exists(newPath))
            {
              File.Delete(newPath);
            }

            using (FileStream stream = new FileStream(newPath, FileMode.Create, FileAccess.Write))
            {
              byte[] bytes = ReadFully(response.GetResponseStream());

              stream.Write(bytes, 0, bytes.Length);
            }

            #region Replace correct url in satellite.xml

            // Read the XML file and replace URL into content
            ReplaceInFile(newPath, "http://fastsatfinder.com/bin/mp", "http://install.team-mediaportal.com/tvsetup/DVBS");

            #endregion
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
        item.Text = itemLine + " done";
      }
      Application.DoEvents();
    }

    /// <summary>
    /// Replace string in file 
    /// </summary>
    /// <param name="context"></param>
    public static void ReplaceInFile(
                      string filePath, string searchText, string replaceText)
    {

      var content = string.Empty;
      using (StreamReader reader = new StreamReader(filePath))
      {
        content = reader.ReadToEnd();
        reader.Close();
      }

      content = Regex.Replace(content, searchText, replaceText);

      using (StreamWriter writer = new StreamWriter(filePath))
      {
        writer.Write(content);
        writer.Close();
      }
    }


    /// <summary>
    /// Save reading data in file 
    /// </summary>
    /// <param name="context"></param>
    public static byte[] ReadFully(Stream input)
    {
      byte[] buffer = new byte[16 * 1024];
      using (MemoryStream ms = new MemoryStream())
      {
        int read;
        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
        {
          ms.Write(buffer, 0, read);
        }
        return ms.ToArray();
      }
    }

    

    /// <summary>
    /// Downloads new transponderlist and merges both S and S2 into one XML 
    /// </summary>
    /// <param name="satellite"></param>
    private void DownloadTransponder(Satellite satellite)
    {
      if (satellite.TransponderListUrl == null)
        return;
      if (satellite.TransponderListUrl.Length == 0)
        return;
      if (!satellite.TransponderListUrl.ToLowerInvariant().StartsWith("http://"))
        return;
      string itemLine = String.Format("Downloading transponders for: {0}", satellite.Name);
      ListViewItem item = listViewStatus.Items.Add(new ListViewItem(itemLine));
      item.EnsureVisible();
      Application.DoEvents();

      var transponders = new List<Transponder>();
      try
      {
        string[,] contextDownload = new string[2, 2];
        contextDownload[0, 0] = satellite.LocalTransponderFile;
        //        contextDownload[1, 0] = context.FileName.Replace(".ini", "-S2.ini");
        contextDownload[0, 1] = satellite.TransponderListUrl;
        contextDownload[1, 1] = satellite.TransponderListUrl.Replace(".ini", "-S2.ini");

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
                                       Path.GetFileNameWithoutExtension(satellite.LocalTransponderFile));
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
  }
}