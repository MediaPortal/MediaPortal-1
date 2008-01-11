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
using System.Xml;
using System.Net;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.EPG.config;
using MediaPortal.Services;
using MediaPortal.Utils.Services;
using MediaPortal.Util;

namespace WebEPG_conf
{
  /// <summary>
  /// Summary description for Form1.
  /// </summary>
  public class fChannels : System.Windows.Forms.Form
  {
    private string startDirectory;
    private Form selection;
    //private TreeNode tChannels;
    //private TreeNode tGrabbers;
    private SortedList ChannelList;
    private SortedList CountryList;
    //private Hashtable hChannelInfo;
    //private Hashtable hGrabberInfo;
    private EventHandler handler;
    private EventHandler selectHandler;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox2;
    private MediaPortal.UserInterface.Controls.MPLabel l_cID;
    private MediaPortal.UserInterface.Controls.MPButton bAdd;
    private System.Windows.Forms.ListBox lbChannels;
    private MediaPortal.UserInterface.Controls.MPGroupBox gbChannelDetails;
    private MediaPortal.UserInterface.Controls.MPGroupBox gbGrabber;
    private MediaPortal.UserInterface.Controls.MPButton bSave;
    private MediaPortal.UserInterface.Controls.MPTextBox tbCount;
    private MediaPortal.UserInterface.Controls.MPLabel lCount;
    private MediaPortal.UserInterface.Controls.MPButton bRemove;
    private MediaPortal.UserInterface.Controls.MPLabel label4;
    private MediaPortal.UserInterface.Controls.MPButton bUpdate;
    private MediaPortal.UserInterface.Controls.MPTextBox tbChannelName;
    private System.Windows.Forms.OpenFileDialog importFile;
    private MediaPortal.UserInterface.Controls.MPTextBox tbChannelID;
    private MediaPortal.UserInterface.Controls.MPTextBox tbURL;
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPTextBox tbValid;
    private System.Windows.Forms.ListBox lbGrabbers;
    private MediaPortal.UserInterface.Controls.MPButton bLoad;
    private MediaPortal.UserInterface.Controls.MPButton bImport;
    private MediaPortal.UserInterface.Controls.MPCheckBox cbFilter;
    private MediaPortal.UserInterface.Controls.MPTextBox tbFilterRegex;
    private MediaPortal.UserInterface.Controls.MPGroupBox groupBox1;
    private MediaPortal.UserInterface.Controls.MPRadioButton rbRegex;
    private MediaPortal.UserInterface.Controls.MPRadioButton rbGrabber;
    private MediaPortal.UserInterface.Controls.MPTextBox tbFilterGrabber;
    // private System.ComponentModel.IContainer components;
    private ILog _log;

    public fChannels()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      selectHandler = new EventHandler(DoSelect);
      handler = new EventHandler(DoEvent);
      bUpdate.Click += handler;
      bSave.Click += handler;
      bAdd.Click += handler;
      bRemove.Click += handler;
      bImport.Click += handler;
      bLoad.Click += handler;
      cbFilter.CheckedChanged += handler;
      lbChannels.SelectedValueChanged += handler;

      //ServiceProvider services = GlobalServiceProvider.Instance;
      _log = GlobalServiceProvider.Get<ILog>();
      //services.Add<ILog>(_log);

      startDirectory = Environment.CurrentDirectory;

      startDirectory += "\\WebEPG";

      //_log.Info("WebEPG Config: Loading Channels");
      //hChannelInfo = new Hashtable();$

      LoadConfig();
      UpdateList("", -1);
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        //				if(components != null)
        //				{
        //					components.Dispose();
        //				}
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.bAdd = new MediaPortal.UserInterface.Controls.MPButton();
      this.lbChannels = new System.Windows.Forms.ListBox();
      this.groupBox2 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.cbFilter = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.bImport = new MediaPortal.UserInterface.Controls.MPButton();
      this.bLoad = new MediaPortal.UserInterface.Controls.MPButton();
      this.lCount = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tbCount = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.bSave = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.tbFilterGrabber = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.rbGrabber = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.rbRegex = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.tbFilterRegex = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.gbChannelDetails = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.tbValid = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tbURL = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.bUpdate = new MediaPortal.UserInterface.Controls.MPButton();
      this.tbChannelID = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.l_cID = new MediaPortal.UserInterface.Controls.MPLabel();
      this.tbChannelName = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.bRemove = new MediaPortal.UserInterface.Controls.MPButton();
      this.gbGrabber = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.lbGrabbers = new System.Windows.Forms.ListBox();
      this.importFile = new System.Windows.Forms.OpenFileDialog();
      this.groupBox2.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.gbChannelDetails.SuspendLayout();
      this.gbGrabber.SuspendLayout();
      this.SuspendLayout();
      // 
      // bAdd
      // 
      this.bAdd.Location = new System.Drawing.Point(8, 400);
      this.bAdd.Name = "bAdd";
      this.bAdd.Size = new System.Drawing.Size(72, 24);
      this.bAdd.TabIndex = 12;
      this.bAdd.Text = "Add";
      // 
      // lbChannels
      // 
      this.lbChannels.Location = new System.Drawing.Point(16, 24);
      this.lbChannels.Name = "lbChannels";
      this.lbChannels.Size = new System.Drawing.Size(256, 264);
      this.lbChannels.TabIndex = 10;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.cbFilter);
      this.groupBox2.Controls.Add(this.bImport);
      this.groupBox2.Controls.Add(this.bLoad);
      this.groupBox2.Controls.Add(this.lCount);
      this.groupBox2.Controls.Add(this.tbCount);
      this.groupBox2.Controls.Add(this.bSave);
      this.groupBox2.Controls.Add(this.lbChannels);
      this.groupBox2.Controls.Add(this.groupBox1);
      this.groupBox2.Location = new System.Drawing.Point(16, 8);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(280, 432);
      this.groupBox2.TabIndex = 13;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Channels";
      // 
      // cbFilter
      // 
      this.cbFilter.Location = new System.Drawing.Point(24, 320);
      this.cbFilter.Name = "cbFilter";
      this.cbFilter.Size = new System.Drawing.Size(48, 14);
      this.cbFilter.TabIndex = 20;
      this.cbFilter.Text = "Filter";
      // 
      // bImport
      // 
      this.bImport.Location = new System.Drawing.Point(8, 400);
      this.bImport.Name = "bImport";
      this.bImport.Size = new System.Drawing.Size(72, 24);
      this.bImport.TabIndex = 19;
      this.bImport.Text = "Import";
      // 
      // bLoad
      // 
      this.bLoad.Location = new System.Drawing.Point(104, 400);
      this.bLoad.Name = "bLoad";
      this.bLoad.Size = new System.Drawing.Size(72, 24);
      this.bLoad.TabIndex = 18;
      this.bLoad.Text = "Reload all";
      // 
      // lCount
      // 
      this.lCount.Location = new System.Drawing.Point(104, 292);
      this.lCount.Name = "lCount";
      this.lCount.Size = new System.Drawing.Size(80, 16);
      this.lCount.TabIndex = 1;
      this.lCount.Text = "Channel Count";
      // 
      // tbCount
      // 
      this.tbCount.Location = new System.Drawing.Point(16, 288);
      this.tbCount.Name = "tbCount";
      this.tbCount.Size = new System.Drawing.Size(72, 20);
      this.tbCount.TabIndex = 0;
      this.tbCount.Text = "";
      // 
      // bSave
      // 
      this.bSave.Location = new System.Drawing.Point(200, 400);
      this.bSave.Name = "bSave";
      this.bSave.Size = new System.Drawing.Size(72, 24);
      this.bSave.TabIndex = 16;
      this.bSave.Text = "Save";
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.tbFilterGrabber);
      this.groupBox1.Controls.Add(this.rbGrabber);
      this.groupBox1.Controls.Add(this.rbRegex);
      this.groupBox1.Controls.Add(this.tbFilterRegex);
      this.groupBox1.Location = new System.Drawing.Point(16, 320);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(256, 72);
      this.groupBox1.TabIndex = 22;
      this.groupBox1.TabStop = false;
      // 
      // tbFilterGrabber
      // 
      this.tbFilterGrabber.Location = new System.Drawing.Point(72, 40);
      this.tbFilterGrabber.Name = "tbFilterGrabber";
      this.tbFilterGrabber.Size = new System.Drawing.Size(176, 20);
      this.tbFilterGrabber.TabIndex = 24;
      this.tbFilterGrabber.Text = "";
      // 
      // rbGrabber
      // 
      this.rbGrabber.Location = new System.Drawing.Point(8, 40);
      this.rbGrabber.Name = "rbGrabber";
      this.rbGrabber.Size = new System.Drawing.Size(64, 24);
      this.rbGrabber.TabIndex = 23;
      this.rbGrabber.Text = "Grabber";
      // 
      // rbRegex
      // 
      this.rbRegex.Checked = true;
      this.rbRegex.Location = new System.Drawing.Point(8, 16);
      this.rbRegex.Name = "rbRegex";
      this.rbRegex.Size = new System.Drawing.Size(56, 24);
      this.rbRegex.TabIndex = 22;
      this.rbRegex.TabStop = true;
      this.rbRegex.Text = "Regex";
      // 
      // tbFilterRegex
      // 
      this.tbFilterRegex.Location = new System.Drawing.Point(72, 16);
      this.tbFilterRegex.Name = "tbFilterRegex";
      this.tbFilterRegex.Size = new System.Drawing.Size(176, 20);
      this.tbFilterRegex.TabIndex = 21;
      this.tbFilterRegex.Text = "";
      // 
      // gbChannelDetails
      // 
      this.gbChannelDetails.Controls.Add(this.tbValid);
      this.gbChannelDetails.Controls.Add(this.label1);
      this.gbChannelDetails.Controls.Add(this.tbURL);
      this.gbChannelDetails.Controls.Add(this.bUpdate);
      this.gbChannelDetails.Controls.Add(this.tbChannelID);
      this.gbChannelDetails.Controls.Add(this.label4);
      this.gbChannelDetails.Controls.Add(this.l_cID);
      this.gbChannelDetails.Controls.Add(this.tbChannelName);
      this.gbChannelDetails.Controls.Add(this.bAdd);
      this.gbChannelDetails.Controls.Add(this.bRemove);
      this.gbChannelDetails.Controls.Add(this.gbGrabber);
      this.gbChannelDetails.Location = new System.Drawing.Point(304, 8);
      this.gbChannelDetails.Name = "gbChannelDetails";
      this.gbChannelDetails.Size = new System.Drawing.Size(312, 432);
      this.gbChannelDetails.TabIndex = 14;
      this.gbChannelDetails.TabStop = false;
      this.gbChannelDetails.Text = "Channel Details";
      // 
      // tbValid
      // 
      this.tbValid.Location = new System.Drawing.Point(256, 72);
      this.tbValid.Name = "tbValid";
      this.tbValid.ReadOnly = true;
      this.tbValid.Size = new System.Drawing.Size(24, 20);
      this.tbValid.TabIndex = 21;
      this.tbValid.Text = "";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 72);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(64, 23);
      this.label1.TabIndex = 20;
      this.label1.Text = "URL";
      // 
      // tbURL
      // 
      this.tbURL.Location = new System.Drawing.Point(88, 72);
      this.tbURL.Name = "tbURL";
      this.tbURL.ReadOnly = true;
      this.tbURL.Size = new System.Drawing.Size(160, 20);
      this.tbURL.TabIndex = 19;
      this.tbURL.Text = "";
      // 
      // bUpdate
      // 
      this.bUpdate.Location = new System.Drawing.Point(120, 400);
      this.bUpdate.Name = "bUpdate";
      this.bUpdate.Size = new System.Drawing.Size(72, 24);
      this.bUpdate.TabIndex = 18;
      this.bUpdate.Text = "Update";
      // 
      // tbChannelID
      // 
      this.tbChannelID.Location = new System.Drawing.Point(88, 24);
      this.tbChannelID.Name = "tbChannelID";
      this.tbChannelID.Size = new System.Drawing.Size(192, 20);
      this.tbChannelID.TabIndex = 13;
      this.tbChannelID.Text = "";
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(16, 24);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(80, 16);
      this.label4.TabIndex = 10;
      this.label4.Text = "ID";
      // 
      // l_cID
      // 
      this.l_cID.Location = new System.Drawing.Point(16, 48);
      this.l_cID.Name = "l_cID";
      this.l_cID.Size = new System.Drawing.Size(64, 23);
      this.l_cID.TabIndex = 8;
      this.l_cID.Text = "Name";
      // 
      // tbChannelName
      // 
      this.tbChannelName.Location = new System.Drawing.Point(88, 48);
      this.tbChannelName.Name = "tbChannelName";
      this.tbChannelName.Size = new System.Drawing.Size(192, 20);
      this.tbChannelName.TabIndex = 7;
      this.tbChannelName.Text = "";
      // 
      // bRemove
      // 
      this.bRemove.Location = new System.Drawing.Point(232, 400);
      this.bRemove.Name = "bRemove";
      this.bRemove.Size = new System.Drawing.Size(72, 24);
      this.bRemove.TabIndex = 17;
      this.bRemove.Text = "Remove";
      // 
      // gbGrabber
      // 
      this.gbGrabber.Controls.Add(this.lbGrabbers);
      this.gbGrabber.Location = new System.Drawing.Point(8, 128);
      this.gbGrabber.Name = "gbGrabber";
      this.gbGrabber.Size = new System.Drawing.Size(296, 264);
      this.gbGrabber.TabIndex = 15;
      this.gbGrabber.TabStop = false;
      this.gbGrabber.Text = "Grabber List";
      // 
      // lbGrabbers
      // 
      this.lbGrabbers.Location = new System.Drawing.Point(16, 24);
      this.lbGrabbers.Name = "lbGrabbers";
      this.lbGrabbers.Size = new System.Drawing.Size(264, 199);
      this.lbGrabbers.TabIndex = 0;
      // 
      // importFile
      // 
      this.importFile.FileName = "channels.xml";
      this.importFile.Filter = "Xml Files (*.xml)|*.xml";
      this.importFile.Title = "Import MP Channel File";
      // 
      // fChannels
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(624, 461);
      this.Controls.Add(this.gbChannelDetails);
      this.Controls.Add(this.groupBox2);
      this.MaximizeBox = false;
      this.Name = "fChannels";
      this.Text = "WebEPG Channel Config";
      this.groupBox2.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.gbChannelDetails.ResumeLayout(false);
      this.gbGrabber.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    #endregion


    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      Application.Run(new fChannels());
      //Application.Run(new fGrabber());
    }

    private void DoSelect(Object source, EventArgs e)
    {
      if (source == selection)
      {
        //				if(selection.Text == "Selection ")
        //				{
        //					this.Activate();
        //					string[] id = (string[]) selection.Tag;
        //					selection.Text = "Selection";
        //
        //					tbChannelName.Tag = id[0];
        //					ChannelInfo info = (ChannelInfo) hChannelInfo[id[0]];
        //					if(info != null)
        //					{
        //						tbChannelName.Text = info.FullName;
        //						//_log.Info("WebEPG Config: Selection: {0}", info.FullName);
        //
        //						UpdateCurrent();
        //					}
        //				}
      }
    }

    private void DoEvent(Object source, EventArgs e)
    {
      if (source == cbFilter)
        UpdateList("", -1);

      if (source == bImport)
      {
          _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: Button: Import");
        if (importFile.ShowDialog() != DialogResult.Cancel)
        {
            _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: Importing channels: {0}", importFile.FileName);
          MediaPortal.Webepg.Profile.Xml xmlreader = new MediaPortal.Webepg.Profile.Xml(importFile.FileName);
          int channelCount = xmlreader.GetValueAsInt("ChannelInfo", "TotalChannels", 0);

          for (int ich = 0; ich < channelCount; ich++)
          {
            ChannelInfo channel = new ChannelInfo();
            channel.ChannelID = xmlreader.GetValueAsString(ich.ToString(), "ChannelID", "");
            channel.FullName = xmlreader.GetValueAsString(ich.ToString(), "FullName", "");
            if (channel.FullName == "")
              channel.FullName = channel.ChannelID;
            if (channel.ChannelID != "")
            {
              ChannelInfo info = (ChannelInfo)ChannelList[channel.ChannelID];
              if (info == null)
              {
                switch (MessageBox.Show("Add name of channel: " + channel.ChannelID +
                  "\nImport Name: " + channel.FullName,
                  "Channels Import",
                  MessageBoxButtons.YesNoCancel,
                  MessageBoxIcon.Information,
                  MessageBoxDefaultButton.Button2))
                {
                  case DialogResult.Yes:
                    ChannelList.Add(channel.ChannelID, channel);
                    break;
                  case DialogResult.No:
                    break;
                  default:
                    UpdateList("", -1);
                    return;
                }
              }
              else
              {
                if (channel.FullName != info.FullName && channel.FullName != channel.ChannelID)
                {
                  switch (MessageBox.Show("Replace name of channel: " + info.ChannelID +
                    "\nCurrent Name: " + info.FullName + "\nImport Name: " + channel.FullName,
                    "Channels Import",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1))
                  {
                    case DialogResult.Yes:
                      info.FullName = channel.FullName;
                      ChannelList.Remove(info.ChannelID);
                      ChannelList.Add(info.ChannelID, info);
                      break;
                    case DialogResult.No:
                      break;
                    default:
                      UpdateList("", -1);
                      return;
                  }
                }
              }
            }
          }
          UpdateList("", -1);
        }
      }

      if (source == bSave)
      {
          _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: Button: Save");
        string confFile = startDirectory + "\\channels\\channels.xml";
        if (System.IO.File.Exists(confFile))
        {
          System.IO.File.Delete(confFile.Replace(".xml", ".bak"));
          System.IO.File.Move(confFile, confFile.Replace(".xml", ".bak"));
        }
        MediaPortal.Webepg.Profile.Xml xmlwriter = new MediaPortal.Webepg.Profile.Xml(confFile);

        ChannelInfo[] infoList = new ChannelInfo[ChannelList.Count];

        int index = 0;
        IDictionaryEnumerator Enumerator = ChannelList.GetEnumerator();
        while (Enumerator.MoveNext())
        {
          infoList[index] = (ChannelInfo)Enumerator.Value;
          if (infoList[index].ChannelID != null && infoList[index].FullName != null)
          {
            if (infoList[index].GrabberList != null)
            {
              IDictionaryEnumerator grabEnum = infoList[index].GrabberList.GetEnumerator();
              while (grabEnum.MoveNext())
              {
                GrabberInfo gInfo = (GrabberInfo)grabEnum.Value;
                SortedList chList = (SortedList)CountryList[gInfo.Country];
                if (chList[index] == null)
                {
                  chList.Add(index, "");
                  //CountryList.Remove(gInfo.Country);
                  //CountryList.Add(gInfo.Country, chList);
                }
              }
            }
            index++;
          }
        }

        xmlwriter.SetValue("ChannelInfo", "TotalChannels", index.ToString());

        for (int i = 0; i < index; i++)
        {
          xmlwriter.SetValue(i.ToString(), "ChannelID", infoList[i].ChannelID);
          xmlwriter.SetValue(i.ToString(), "FullName", infoList[i].FullName);
        }


        IDictionaryEnumerator countryEnum = CountryList.GetEnumerator();
        while (countryEnum.MoveNext())
        {
          SortedList chList = (SortedList)countryEnum.Value;
          xmlwriter.SetValue((string)countryEnum.Key, "TotalChannels", chList.Count);

          index = 0;
          IDictionaryEnumerator chEnum = chList.GetEnumerator();
          while (chEnum.MoveNext())
          {
            xmlwriter.SetValue((string)countryEnum.Key, index.ToString(), chEnum.Key);
            index++;
          }
        }
        xmlwriter.Save();
      }

      if (source == bUpdate)
      {
          _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: Button: Update");
        ReplaceCurrent();
      }

      if (source == bLoad)
      {
        cbFilter.Checked = false;
        string channel = "";
        if (lbChannels.SelectedIndex > -1)
        {
          ChannelInfo info = (ChannelInfo)ChannelList.GetByIndex(lbChannels.SelectedIndex);
          channel = info.ChannelID;
        }
        LoadConfig();
        UpdateList(channel, -1);
      }

      if (source == bRemove)
      {
          _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: Button: Remove");
        if (lbChannels.SelectedIndex != -1)
        {
          ChannelList.Remove(lbChannels.SelectedValue);
          UpdateList("", lbChannels.SelectedIndex);
        }
      }

      if (source == bAdd)
      {
          _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: Button: Add");
        ChannelInfo info = new ChannelInfo();
        //
        //				while(ChannelList[info.DisplayName] != null)
        //					info.DisplayName+="*";

        info.FullName = tbChannelName.Text;
        info.ChannelID = tbChannelID.Text;

        ChannelList.Add(info.ChannelID, info);
        UpdateList(info.ChannelID, -1);
      }

      if (source == lbChannels)
      {
        if (lbChannels.SelectedIndex > -1)
        {
          ChannelInfo info = (ChannelInfo)ChannelList[lbChannels.SelectedItem]; //.GetByIndex(lbChannels.SelectedIndex);
          tbChannelID.Text = info.ChannelID;
          tbChannelName.Text = info.FullName;
          //					tbImportName.Text = info.ImportName;
          int index;
          if ((index = info.ChannelID.IndexOf("@")) != -1)
            tbURL.Text = info.ChannelID.Substring(index + 1);
          else
            tbURL.Text = info.ChannelID;

          //					tbValid.Text = "";
          //					IPHostEntry ipEntry;
          //					try
          //					{
          //						ipEntry = Dns.GetHostByName (tbURL.Text);
          //					}
          //					catch(System.Net.Sockets.SocketException ex)
          //					{
          //						tbValid.Text = "*";
          //					}

          string[] list;
          if (info.GrabberList != null)
          {
            IDictionaryEnumerator Enumerator = info.GrabberList.GetEnumerator();

            list = new string[info.GrabberList.Count];
            int i = 0;

            while (Enumerator.MoveNext())
            {
              GrabberInfo grabber = (GrabberInfo)Enumerator.Value;
              list[i++] = grabber.GrabberID;
            }
            //tbCount.Text = ChannelList.Count.ToString();

          }
          else
          {
            list = new string[0];
          }
          lbGrabbers.DataSource = list;
        }
      }

      //			if(source==bImport)
      //			{
      //				//tChannels = new TreeNode("Channels");
      //				if(System.IO.Directory.Exists(startDirectory + "\\Channels"))
      //					GetTreeChannels(startDirectory + "\\Channels");
      //				UpdateList("", -1);
      //			}

      if (source == selection)
        selection = null;

    }

    private void UpdateCurrent()
    {
      if (lbChannels.SelectedIndex != -1)
      {
        ChannelInfo info = (ChannelInfo)ChannelList[lbChannels.SelectedValue];

        info.FullName = tbChannelName.Text;
        info.ChannelID = tbChannelID.Text;

        ChannelList.Remove(info.ChannelID);
        ChannelList.Add(info.ChannelID, info);
      }
    }

    private void ReplaceCurrent()
    {
      if (lbChannels.SelectedIndex != -1)
      {
        //				ChannelList.RemoveAt(lbChannels.SelectedIndex);

        ChannelInfo info = (ChannelInfo)ChannelList[lbChannels.SelectedValue];

        info.FullName = tbChannelName.Text;
        info.ChannelID = tbChannelID.Text;

        ChannelList.Remove(info.ChannelID);
        ChannelList.Add(info.ChannelID, info);

        UpdateList(info.ChannelID, -1);
      }
    }

    private void GetTreeGrabbers(string Location)
    {
      System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(Location);
      System.IO.DirectoryInfo[] dirList = dir.GetDirectories();
      if (dirList.Length > 0)
      {
        for (int i = 0; i < dirList.Length; i++)
        {
          //LOAD FOLDERS
          System.IO.DirectoryInfo g = dirList[i];
          //TreeNode MainNext = new TreeNode(g.Name); //
          GetTreeGrabbers(g.FullName);
          //Main.Nodes.Add(MainNext);
          //MainNext.Tag = (g.FullName); 
        }
      }
      else
      {
        GetGrabbers(Location);
      }

    }

    private void GetGrabbers(string Location)
    {
      System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(Location);
      _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: Directory: {0}", Location);
      GrabberInfo gInfo;
      foreach (System.IO.FileInfo file in dir.GetFiles("*.xml"))
      {
        gInfo = new GrabberInfo();
        XmlDocument xml = new XmlDocument();
        XmlNodeList channelList;
        try
        {
            _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: File: {0}", file.Name);
          xml.Load(file.FullName);
          channelList = xml.DocumentElement.SelectNodes("/profile/section/entry");

          XmlNode entryNode = xml.DocumentElement.SelectSingleNode("section[@name=\"Info\"]/entry[@name=\"GuideDays\"]");
          if (entryNode != null)
            gInfo.GrabDays = int.Parse(entryNode.InnerText);
          entryNode = xml.DocumentElement.SelectSingleNode("section[@name=\"Info\"]/entry[@name=\"SiteDescription\"]");
          if (entryNode != null)
            gInfo.SiteDesc = entryNode.InnerText;
          entryNode = xml.DocumentElement.SelectSingleNode("section[@name=\"Listing\"]/entry[@name=\"SubListingLink\"]");
          gInfo.Linked = false;
          if (entryNode != null)
            gInfo.Linked = true;
        }
        catch (System.Xml.XmlException) // ex) 
        {
            _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: File open failed - XML error");
          return;
        }

        string GrabberSite = file.Name.Replace(".xml", "");
        GrabberSite = GrabberSite.Replace("_", ".");

        gInfo.GrabberID = file.Directory.Name + "\\" + file.Name;
        gInfo.GrabberName = GrabberSite;
        gInfo.Country = file.Directory.Name;
        //hGrabberInfo.Add(gInfo.GrabberID, gInfo);

        if (CountryList[file.Directory.Name] == null)
          CountryList.Add(file.Directory.Name, new SortedList());

        //TreeNode gNode = new TreeNode(GrabberSite);
        //Main.Nodes.Add(gNode);
        //XmlNode cl=sectionList.Attributes.GetNamedItem("ChannelList");

        foreach (XmlNode nodeChannel in channelList)
        {
          if (nodeChannel.Attributes != null)
          {
            XmlNode id = nodeChannel.ParentNode.Attributes.Item(0);
            if (id.InnerXml == "ChannelList")
            {
              id = nodeChannel.Attributes.Item(0);
              //idList.Add(id.InnerXml);

              ChannelInfo info = (ChannelInfo)ChannelList[id.InnerXml];
              if (info != null) // && info.GrabberList[gInfo.GrabberID] != null)
              {
                //								TreeNode tNode = new TreeNode(info.FullName);
                //								string [] tag = new string[2];
                //								tag[0] = info.ChannelID;
                //								tag[1] = GrabberSite;
                //								tNode.Tag = tag;
                //								gNode.Nodes.Add(tNode);
                if (info.GrabberList == null)
                  info.GrabberList = new SortedList();
                if (info.GrabberList[gInfo.GrabberID] == null)
                  info.GrabberList.Add(gInfo.GrabberID, gInfo);
              }
              else
              {
                info = new ChannelInfo();
                info.ChannelID = id.InnerXml;
                info.FullName = info.ChannelID;
                info.GrabberList = new SortedList();
                info.GrabberList.Add(gInfo.GrabberID, gInfo);
                ChannelList.Add(info.ChannelID, info);
              }
            }
          }
        }
      }
    }

    private void GetTreeChannels(string Location) //ref TreeNode Main, string Location)
    {
      System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(Location);
      if (dir.Exists)
      {
        System.IO.DirectoryInfo[] dirList = dir.GetDirectories();
        if (dirList.Length > 0)
        {
          for (int i = 0; i < dirList.Length; i++)
          {
            //LOAD FOLDERS
            System.IO.DirectoryInfo g = dirList[i];
            //TreeNode MainNext = new TreeNode(g.Name); //
            GetTreeChannels(g.FullName); //ref MainNext, g.FullName);
            //Main.Nodes.Add(MainNext);
            //MainNext.Tag = (g.FullName); 
          }
        }
        else
        {
          GetChannels(Location); //ref Main, Location);
        }
      }
    }


    private void GetChannels(string Location) //ref TreeNode Main, string Location)
    {
      System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(Location);
      _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: Directory: {0}", Location);
      foreach (System.IO.FileInfo file in dir.GetFiles("*.xml"))
      {
          _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: File: {0}", file.Name);
        ChannelInfo info = GetChannelInfo(file.FullName);
        if (info != null)
        {
          if (ChannelList[info.ChannelID] == null)
            ChannelList.Add(info.ChannelID, info);
          else
          {
            ChannelList.Remove(info.ChannelID);
            ChannelList.Add(info.ChannelID, info);
          }

        }
      }
    }

    private ChannelInfo GetChannelInfo(string filename)
    {
      MediaPortal.Webepg.Profile.Xml xmlreader = new MediaPortal.Webepg.Profile.Xml(filename);
      ChannelInfo info = new ChannelInfo();

      info.FullName = xmlreader.GetValueAsString("ChannelInfo", "FullName", "");
      if (info.FullName == "")
      {
          _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: File error: FullName not found");
        return null;
      }
      info.ChannelID = xmlreader.GetValueAsString("ChannelInfo", "ChannelID", "");
      if (info.ChannelID == "")
      {
          _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: File error: ChannelID not found");
        return null;
      }
      int GrabberCount = xmlreader.GetValueAsInt("ChannelInfo", "Grabbers", 0);
      if (GrabberCount == 0)
      {
          _log.WriteFile(LogType.WebEPG, Level.Information, "WebEPG Config: File error: Grabbers not found");
        return null;
      }

      info.GrabberList = new SortedList();
      //			for(int i=0; i < GrabberCount; i++)
      //			{
      //				string GrabberNumb = "Grabber" + (i+1).ToString();
      //				string GrabberID = xmlreader.GetValueAsString("ChannelInfo", GrabberNumb, "");
      //				if(GrabberID == "")
      //				{
      //					_log.Info("WebEPG Config: File error: {0} not found", GrabberNumb);
      //					return null;
      //				}
      //				
      //				int start = GrabberID.IndexOf("\\") + 1;
      //				int end =  GrabberID.LastIndexOf(".");
      //							
      //				string GrabberSite = GrabberID.Substring(start, end-start);
      //				GrabberSite = GrabberSite.Replace("_", ".");
      //				info.GrabberList.Add(GrabberSite, GrabberID); 
      //			}

      return info;
    }

    private void UpdateList(string select, int index)
    {
      IDictionaryEnumerator Enumerator = ChannelList.GetEnumerator();

      string[] list = new string[ChannelList.Count];
      int i = 0;
      int selectedIndex = -1;

      while (Enumerator.MoveNext())
      {
        ChannelInfo channel = (ChannelInfo)Enumerator.Value;
        if (cbFilter.Checked)
        {
          if (rbRegex.Checked)
          {
            Match result = null;
            try
            {
              Regex searchRegex = new Regex(tbFilterRegex.Text);
              result = searchRegex.Match(channel.ChannelID);
            }
            catch (System.ArgumentException ex)
            {
                _log.WriteFile(LogType.WebEPG, Level.Error, "WebEPG Config: Regex error: {0}", ex.ToString());
            }
            if (result.Success)
            {
              if (channel.ChannelID == select)
                selectedIndex = i;
              list[i++] = channel.ChannelID;
            }
          }

          if (rbGrabber.Checked)
          {
            if (channel.GrabberList != null &&
              channel.GrabberList[tbFilterGrabber.Text] != null)
            {
              if (channel.ChannelID == select)
                selectedIndex = i;
              list[i++] = channel.ChannelID;
            }
          }
        }
        else
        {
          if (channel.ChannelID == select)
            selectedIndex = i;
          list[i++] = channel.ChannelID;
        }
      }
      tbCount.Text = i.ToString(); //ChannelList.Count.ToString();
      string[] datasource = new string[i];
      for (int c = 0; c < i; c++)
        datasource[c] = list[c];
      lbChannels.DataSource = datasource;
      if (selectedIndex > 0)
        lbChannels.SelectedIndex = selectedIndex;
      if (index > 0)
      {
        if (index >= ChannelList.Count)
          index = ChannelList.Count - 1;
        lbChannels.SelectedIndex = index;
      }
    }

    private void LoadConfig()
    {
      ChannelsList ConfigInfo = new ChannelsList(startDirectory);
      string[] countries = ConfigInfo.GetCountries();
      ChannelList = ConfigInfo.GetChannelsList();

      CountryList = new SortedList();
      for (int i = 0; i < countries.Length; i++)
        CountryList.Add(countries[i], new SortedList());

      //			ChannelList = new SortedList();
      //
      //			if(System.IO.File.Exists(startDirectory + "\\channels\\channels.xml"))
      //			{
      //				_log.Info("WebEPG Config: Loading Existing channels.xml");
      //				MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml(startDirectory + "\\channels\\channels.xml");
      //				int channelCount = xmlreader.GetValueAsInt("ChannelInfo", "TotalChannels", 0);	
      //
      //				for (int ich = 0; ich < channelCount; ich++)
      //				{
      //					ChannelInfo channel = new ChannelInfo();
      //					channel.ChannelID = xmlreader.GetValueAsString(ich.ToString(), "ChannelID", "");
      //					channel.FullName = xmlreader.GetValueAsString(ich.ToString(), "FullName", "");
      //					if(channel.FullName == "")
      //						channel.FullName = channel.ChannelID;
      //					if(channel.ChannelID != "")
      //						ChannelList.Add(channel.ChannelID, channel);
      //				}
      //			}
      //
      //			
      //			_log.Info("WebEPG Config: Loading Grabbers");
      //			hGrabberInfo = new Hashtable();
      //			CountryList = new SortedList();
      //			//tGrabbers = new TreeNode("Web Sites");
      //			if(System.IO.Directory.Exists(startDirectory + "\\Grabbers"))
      //				GetTreeGrabbers(startDirectory + "\\Grabbers");
      //			else
      //				_log.Error("WebEPG Config: Cannot find grabbers directory");

      //
      // TODO: Add any constructor code after InitializeComponent call
      //

      //ChannelList = new SortedList();

      //hChannelInfo.
    }
  }
}
