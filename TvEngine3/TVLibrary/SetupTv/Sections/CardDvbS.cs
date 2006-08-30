/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using DirectShowLib;

using IdeaBlade.Persistence;
using IdeaBlade.Rdb;
using IdeaBlade.Persistence.Rdb;
using IdeaBlade.Util;

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
    class Transponder : IComparable
    {
      public string SatName;
      public string FileName;
      public override string ToString()
      {
        return SatName;
      }
      public int CompareTo(object o)
      {
        Transponder k = (Transponder)o;
        return SatName.CompareTo(k.SatName);
      }

    }

    struct TPList
    {
      public int TPfreq; // frequency
      public Polarisation TPpol;  // polarisation 0=hori, 1=vert
      public int TPsymb; // symbol rate
    }

    int _cardNumber;
    TPList[] _transponders = new TPList[1000];
    int _channelCount = 0;

    int _tvChannelsNew = 0;
    int _radioChannelsNew = 0;
    int _tvChannelsUpdated = 0;
    int _radioChannelsUpdated = 0;

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

    Transponder LoadTransponderName(string fileName)
    {
      Transponder ts = new Transponder();
      ts.FileName = @"Tuningparameters\" + fileName;
      ts.SatName = fileName;

      string line;
      System.IO.TextReader tin = System.IO.File.OpenText(@"Tuningparameters\" + fileName);
      while (true)
      {
        line = tin.ReadLine();
        if (line == null) break;
        string search = line.ToLower();
        int pos = search.IndexOf("satname");
        if (pos >= 0)
        {
          pos = search.IndexOf("=");
          if (pos > 0)
          {
            ts.SatName = line.Substring(pos + 1);
            ts.SatName = ts.SatName.Trim();
            break;
          }
        }
      }
      tin.Close();

      return ts;
    }
    void LoadList(string fileName)
    {

      _channelCount = 0;
      string line;
      string[] tpdata;
      // load transponder list and start scan
      System.IO.TextReader tin = System.IO.File.OpenText(fileName);
      int _count = 0;
      do
      {
        line = null;
        line = tin.ReadLine();
        if (line != null)
          if (line.Length > 0)
          {
            if (line.StartsWith(";"))
              continue;
            tpdata = line.Split(new char[] { ',' });
            if (tpdata.Length != 3)
              tpdata = line.Split(new char[] { ';' });
            if (tpdata.Length == 3)
            {
              try
              {

                _transponders[_count].TPfreq = Int32.Parse(tpdata[0]) * 1000;
                switch (tpdata[1].ToLower())
                {
                  case "v":

                    _transponders[_count].TPpol = Polarisation.LinearV;
                    break;
                  case "h":

                    _transponders[_count].TPpol = Polarisation.LinearH;
                    break;
                  default:

                    _transponders[_count].TPpol = Polarisation.LinearH;
                    break;
                }
                _transponders[_count].TPsymb = Int32.Parse(tpdata[2]);
                _count += 1;
              }
              catch
              { }
            }
          }
      } while (!(line == null));
      tin.Close();
      _channelCount = _count;
    }

    void Init()
    {
      mpTransponder1.Items.Clear();
      mpTransponder2.Items.Clear();
      mpTransponder3.Items.Clear();
      mpTransponder4.Items.Clear();
      string[] files = System.IO.Directory.GetFiles(System.IO.Directory.GetCurrentDirectory() + @"\Tuningparameters", "*.tpl");
      Transponder[] transponders = new Transponder[files.Length];
      int trans = 0;
      foreach (string file in files)
      {
        string fileName = System.IO.Path.GetFileName(file);
        Transponder ts = LoadTransponderName(fileName);
        if (ts != null)
        {
          transponders[trans++] = ts;
        }
      }
      Array.Sort(transponders);
      foreach (Transponder ts in transponders)
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

      mpLNB1.Checked = true;
      mpLNB1.Enabled = false;
      mpDisEqc1.Items.Clear();
      mpDisEqc1.Items.Add(DisEqcType.None);
      mpDisEqc1.Items.Add(DisEqcType.SimpleA);
      mpDisEqc1.Items.Add(DisEqcType.SimpleB);
      mpDisEqc1.Items.Add(DisEqcType.Level1AA);
      mpDisEqc1.Items.Add(DisEqcType.Level1BA);
      mpDisEqc1.Items.Add(DisEqcType.Level1AB);
      mpDisEqc1.Items.Add(DisEqcType.Level1BB);
      mpDisEqc1.SelectedIndex = 0;

      mpDisEqc2.Items.Clear();
      mpDisEqc2.Items.Add(DisEqcType.None);
      mpDisEqc2.Items.Add(DisEqcType.SimpleA);
      mpDisEqc2.Items.Add(DisEqcType.SimpleB);
      mpDisEqc2.Items.Add(DisEqcType.Level1AA);
      mpDisEqc2.Items.Add(DisEqcType.Level1BA);
      mpDisEqc2.Items.Add(DisEqcType.Level1AB);
      mpDisEqc2.Items.Add(DisEqcType.Level1BB);
      mpDisEqc2.SelectedIndex = 0;

      mpDisEqc3.Items.Clear();
      mpDisEqc3.Items.Add(DisEqcType.None);
      mpDisEqc3.Items.Add(DisEqcType.SimpleA);
      mpDisEqc3.Items.Add(DisEqcType.SimpleB);
      mpDisEqc3.Items.Add(DisEqcType.Level1AA);
      mpDisEqc3.Items.Add(DisEqcType.Level1BA);
      mpDisEqc3.Items.Add(DisEqcType.Level1AB);
      mpDisEqc3.Items.Add(DisEqcType.Level1BB);
      mpDisEqc3.SelectedIndex = 0;

      mpDisEqc4.Items.Clear();
      mpDisEqc4.Items.Add(DisEqcType.None);
      mpDisEqc4.Items.Add(DisEqcType.SimpleA);
      mpDisEqc4.Items.Add(DisEqcType.SimpleB);
      mpDisEqc4.Items.Add(DisEqcType.Level1AA);
      mpDisEqc4.Items.Add(DisEqcType.Level1BA);
      mpDisEqc4.Items.Add(DisEqcType.Level1AB);
      mpDisEqc4.Items.Add(DisEqcType.Level1BB);
      mpDisEqc4.SelectedIndex = 0;

      TvBusinessLayer layer = new TvBusinessLayer();
      mpTransponder1.SelectedIndex = Int32.Parse(layer.GetSetting("dvbs" + _cardNumber.ToString() + "Transponder1", "0").Value);
      mpTransponder2.SelectedIndex = Int32.Parse(layer.GetSetting("dvbs" + _cardNumber.ToString() + "Transponder2", "0").Value);
      mpTransponder3.SelectedIndex = Int32.Parse(layer.GetSetting("dvbs" + _cardNumber.ToString() + "Transponder3", "0").Value);
      mpTransponder4.SelectedIndex = Int32.Parse(layer.GetSetting("dvbs" + _cardNumber.ToString() + "Transponder4", "0").Value);


      mpDisEqc1.SelectedIndex = Int32.Parse(layer.GetSetting("dvbs" + _cardNumber.ToString() + "DisEqc1", "0").Value);
      mpDisEqc2.SelectedIndex = Int32.Parse(layer.GetSetting("dvbs" + _cardNumber.ToString() + "DisEqc2", "0").Value);
      mpDisEqc3.SelectedIndex = Int32.Parse(layer.GetSetting("dvbs" + _cardNumber.ToString() + "DisEqc3", "0").Value);
      mpDisEqc4.SelectedIndex = Int32.Parse(layer.GetSetting("dvbs" + _cardNumber.ToString() + "DisEqc4", "0").Value);

      mpLNB2.Checked = (layer.GetSetting("dvbs" + _cardNumber.ToString() + "LNB2", "false").Value == "true");
      mpLNB3.Checked = (layer.GetSetting("dvbs" + _cardNumber.ToString() + "LNB3", "false").Value == "true");
      mpLNB4.Checked = (layer.GetSetting("dvbs" + _cardNumber.ToString() + "LNB4", "false").Value == "true");
      mpLNB2_CheckedChanged(null, null); ;
      mpLNB3_CheckedChanged(null, null); ;
      mpLNB4_CheckedChanged(null, null); ;
    }
    public override void OnSectionDeActivated()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      base.OnSectionDeActivated();
      layer.GetSetting("dvbs" + _cardNumber.ToString() + "Transponder1", "0").Value = mpTransponder1.SelectedIndex.ToString();
      layer.GetSetting("dvbs" + _cardNumber.ToString() + "Transponder2", "0").Value = mpTransponder2.SelectedIndex.ToString();
      layer.GetSetting("dvbs" + _cardNumber.ToString() + "Transponder3", "0").Value = mpTransponder3.SelectedIndex.ToString();
      layer.GetSetting("dvbs" + _cardNumber.ToString() + "Transponder4", "0").Value = mpTransponder4.SelectedIndex.ToString();

      layer.GetSetting("dvbs" + _cardNumber.ToString() + "DisEqc1", "0").Value = mpDisEqc1.SelectedIndex.ToString();
      layer.GetSetting("dvbs" + _cardNumber.ToString() + "DisEqc2", "0").Value = mpDisEqc2.SelectedIndex.ToString();
      layer.GetSetting("dvbs" + _cardNumber.ToString() + "DisEqc3", "0").Value = mpDisEqc3.SelectedIndex.ToString();
      layer.GetSetting("dvbs" + _cardNumber.ToString() + "DisEqc4", "0").Value = mpDisEqc4.SelectedIndex.ToString();

      layer.GetSetting("dvbs" + _cardNumber.ToString() + "LNB2", "false").Value = mpLNB2.Checked ? "true" : "false";
      layer.GetSetting("dvbs" + _cardNumber.ToString() + "LNB3", "false").Value = mpLNB3.Checked ? "true" : "false";
      layer.GetSetting("dvbs" + _cardNumber.ToString() + "LNB4", "false").Value = mpLNB4.Checked ? "true" : "false";
      DatabaseManager.Instance.SaveChanges();
    }

    void UpdateStatus(int LNB)
    {
      mpLabelTunerLocked.Text = "No";
      if (RemoteControl.Instance.TunerLocked(_cardNumber))
        mpLabelTunerLocked.Text = "Yes";
      progressBarLevel.Value = RemoteControl.Instance.SignalLevel(_cardNumber);
      progressBarQuality.Value = RemoteControl.Instance.SignalQuality(_cardNumber);

      DVBSChannel channel = RemoteControl.Instance.CurrentChannel(_cardNumber) as DVBSChannel;
      if (channel == null)
        mpLabelChannel.Text = "none";
      else
        mpLabelChannel.Text = String.Format("LNB:{0} Freq:{1} Pol:{2}",
            LNB, channel.Frequency, channel.Polarisation);
      
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      UpdateStatus(1);
      labelScan1.Text = "";
      labelScan2.Text = "";
    }



    private void mpButtonScanTv_Click(object sender, EventArgs e)
    {
      Thread scanThread = new Thread(new ThreadStart(DoScan));
      scanThread.Start();
    }
    void DoScan()
    {
      try
      {
        RemoteControl.Instance.EpgGrabberEnabled = false;
        mpButtonScanTv.Enabled = false;
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

        labelScan1.Text = "";
        labelScan2.Text = "";

        _tvChannelsNew = 0;
        _radioChannelsNew = 0;
        _tvChannelsUpdated = 0;
        _radioChannelsUpdated = 0;

        Scan(1, (DisEqcType)mpDisEqc1.SelectedIndex, (Transponder)mpTransponder1.SelectedItem);
        if (mpLNB2.Checked)
          Scan(2, (DisEqcType)mpDisEqc2.SelectedIndex, (Transponder)mpTransponder2.SelectedItem);

        if (mpLNB3.Checked)
          Scan(3, (DisEqcType)mpDisEqc3.SelectedIndex, (Transponder)mpTransponder3.SelectedItem);

        if (mpLNB4.Checked)
          Scan(4, (DisEqcType)mpDisEqc2.SelectedIndex, (Transponder)mpTransponder4.SelectedItem);

        mpButtonScanTv.Enabled = true;
        mpTransponder1.Enabled = true;
        mpTransponder2.Enabled = true;
        mpTransponder3.Enabled = true;
        mpTransponder4.Enabled = true;
        mpDisEqc1.Enabled = true;
        mpDisEqc2.Enabled = true;
        mpDisEqc3.Enabled = true;
        mpDisEqc4.Enabled = true;
        progressBar1.Value = 100;

        mpLNB2.Enabled = true;
        mpLNB3.Enabled = true;
        mpLNB4.Enabled = true;
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      finally
      {
        RemoteControl.Instance.EpgGrabberEnabled = true;
      }
    }

    void Scan(int LNB, DisEqcType disEqc, Transponder transponder)
    {
      LoadList(transponder.FileName);
      if (_channelCount == 0) return;

      
      TvBusinessLayer layer = new TvBusinessLayer();
      Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));

      for (int index = 0; index < _channelCount; ++index)
      {
        float percent = ((float)(index)) / _channelCount;
        percent *= 100f;
        if (percent > 100f) percent = 100f;
        progressBar1.Value = (int)percent;

        
        DVBSChannel tuneChannel = new DVBSChannel();
        tuneChannel.Frequency = _transponders[index].TPfreq;
        tuneChannel.Polarisation = _transponders[index].TPpol;
        tuneChannel.SymbolRate = _transponders[index].TPsymb;
        tuneChannel.DisEqc = disEqc;

        if (index == 0)
        {
          RemoteControl.Instance.Tune(_cardNumber, tuneChannel);
        }
        UpdateStatus(LNB);
        
        IChannel[] channels = RemoteControl.Instance.Scan(_cardNumber, tuneChannel);
        UpdateStatus(LNB);
        
        if (channels == null) continue;
        if (channels.Length == 0) continue;

        for (int i = 0; i < channels.Length; ++i)
        {
          DVBSChannel channel = (DVBSChannel)channels[i];
          bool exists = (layer.GetChannelByName(channel.Name) != null);

          Channel dbChannel = layer.AddChannel(channel.Name);
          dbChannel.IsTv = channel.IsTv;
          dbChannel.IsRadio = channel.IsRadio;
          if (dbChannel.IsRadio)
          {
            dbChannel.GrabEpg = false;
          }
          dbChannel.SortOrder = 10000;
          if (channel.LogicalChannelNumber >= 1)
          {
            dbChannel.SortOrder = channel.LogicalChannelNumber;
          }
          layer.AddTuningDetails(dbChannel, channel);
          if (channel.IsTv)
          {
            if (exists)
              _tvChannelsUpdated++;
            else
              _tvChannelsNew++;
          }
          if (channel.IsRadio)
          {
            if (exists)
              _radioChannelsUpdated++;
            else
              _radioChannelsNew++;
          }
          layer.MapChannelToCard(card, dbChannel);

          labelScan1.Text = String.Format("Tv channels New:{0} Updated:{1}", _tvChannelsNew, _tvChannelsUpdated);
          labelScan2.Text = String.Format("Radio channels New:{0} Updated:{1}", _radioChannelsNew, _radioChannelsUpdated);

          
        }
      }

      DatabaseManager.Instance.SaveChanges();

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



  }
}