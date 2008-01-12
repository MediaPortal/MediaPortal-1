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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;

namespace SetupTv
{
  public class SHOUTcastStation
  {
    public string name;
    public string url;
    public int bitrate;
  }
  /// <summary>
  /// Summary description for SearchSHOUTcast.
  /// </summary>
  public class SearchSHOUTcast : System.Windows.Forms.Form
  {
    private MediaPortal.UserInterface.Controls.MPButton SearchButton;
    private MediaPortal.UserInterface.Controls.MPTextBox SearchText;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private MediaPortal.UserInterface.Controls.MPListView ResultsBox;
    private MediaPortal.UserInterface.Controls.MPButton AddButton;
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;
    private MediaPortal.UserInterface.Controls.MPLabel WaitLabel;
    private System.Windows.Forms.ColumnHeader columnHeader3;

    private SHOUTcastStation Selected_Radiostation = null;     //Our return station info

    public SearchSHOUTcast()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();
      //
      // TODO: Add any constructor code after InitializeComponent call
      //
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
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
      this.SearchButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.SearchText = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.ResultsBox = new MediaPortal.UserInterface.Controls.MPListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
      this.AddButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.WaitLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.SuspendLayout();
      // 
      // SearchButton
      // 
      this.SearchButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
      this.SearchButton.ForeColor = System.Drawing.Color.Black;
      this.SearchButton.Location = new System.Drawing.Point(8, 240);
      this.SearchButton.Name = "SearchButton";
      this.SearchButton.Size = new System.Drawing.Size(80, 23);
      this.SearchButton.TabIndex = 0;
      this.SearchButton.Text = "Search";
      this.SearchButton.Click += new System.EventHandler(this.SearchButton_Click);
      // 
      // SearchText
      // 
      this.SearchText.Location = new System.Drawing.Point(96, 240);
      this.SearchText.Name = "SearchText";
      this.SearchText.Size = new System.Drawing.Size(184, 20);
      this.SearchText.TabIndex = 3;
      this.SearchText.Text = "";
      // 
      // ResultsBox
      // 
      this.ResultsBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
      this.ResultsBox.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                 this.columnHeader1,
                                                                                 this.columnHeader2,
                                                                                 this.columnHeader3});
      this.ResultsBox.ForeColor = System.Drawing.Color.Black;
      this.ResultsBox.FullRowSelect = true;
      this.ResultsBox.HideSelection = false;
      this.ResultsBox.Location = new System.Drawing.Point(8, 0);
      this.ResultsBox.MultiSelect = false;
      this.ResultsBox.Name = "ResultsBox";
      this.ResultsBox.Size = new System.Drawing.Size(464, 231);
      this.ResultsBox.TabIndex = 4;
      this.ResultsBox.View = System.Windows.Forms.View.Details;
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Bitrate";
      this.columnHeader1.Width = 65;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Station name";
      this.columnHeader2.Width = 395;
      // 
      // columnHeader3
      // 
      this.columnHeader3.Text = "Station Number(Ignore)";
      this.columnHeader3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.columnHeader3.Width = 121;
      // 
      // AddButton
      // 
      this.AddButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
      this.AddButton.ForeColor = System.Drawing.Color.Black;
      this.AddButton.Location = new System.Drawing.Point(392, 240);
      this.AddButton.Name = "AddButton";
      this.AddButton.Size = new System.Drawing.Size(80, 23);
      this.AddButton.TabIndex = 5;
      this.AddButton.Text = "Add Station";
      this.AddButton.Click += new System.EventHandler(this.AddButton_Click);
      // 
      // WaitLabel
      // 
      this.WaitLabel.BackColor = System.Drawing.Color.White;
      this.WaitLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.WaitLabel.Enabled = false;
      this.WaitLabel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.WaitLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.WaitLabel.ForeColor = System.Drawing.Color.Black;
      this.WaitLabel.Location = new System.Drawing.Point(160, 104);
      this.WaitLabel.Name = "WaitLabel";
      this.WaitLabel.Size = new System.Drawing.Size(168, 32);
      this.WaitLabel.TabIndex = 6;
      this.WaitLabel.Text = "Please Wait";
      this.WaitLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.WaitLabel.UseMnemonic = false;
      this.WaitLabel.Visible = false;
      // 
      // SearchSHOUTcast
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(482, 264);
      this.Controls.Add(this.WaitLabel);
      this.Controls.Add(this.AddButton);
      this.Controls.Add(this.ResultsBox);
      this.Controls.Add(this.SearchText);
      this.Controls.Add(this.SearchButton);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "SearchSHOUTcast";
      this.ShowInTaskbar = false;
      this.Text = "Search SHOUTcast for your favorite radio stations";
      this.Load += new System.EventHandler(this.SearchSHOUTcast_Load);
      this.ResumeLayout(false);

    }
    #endregion

    public SHOUTcastStation Station
    {
      //Fill in station settings with search
      get
      {
        //returns selected radiostation if nothing then null
        if (Selected_Radiostation == null) return null;
        else return Selected_Radiostation;
      }
    }

    private ArrayList Search(string search)
    {
      //Create needed variables
      int count = 100,
        total = 0,
        counter = 1;										   // Used to keep track of file numbering
      int[] found = new int[count],
        end = new int[count];

      string[] SCstaindex = new string[count];                      //Index info
      string[] SCstanum = new string[count];					       // Second part holds file number on Shoutcast
      string[] SCstaname = new string[count];                       // Extracted station name
      string[] SCstabr = new string[count];						   //Bitrate info
      WebClient myWebClient = new WebClient();         			   // Create a new WebClient instance.	
      string HTMLdownload;										   // Hold HTML for parsing or misc
      byte[] HTMLbuffer;                      					   // HTML buffer
      MatchCollection mc;											   // Holds matched data
      // Create Regex objects and define the search criteria.
      Regex nterm = new Regex("Unfortunately, there weren't any SHOUTcast streams found containing the term");
      Regex ngenre = new Regex("Unfortunately, there weren't any SHOUTcast streams found under the genre");
      Regex snum = new Regex("rn=[0-9]*");
      Regex sname = new Regex("target=\"_scurl\" href=.*</a>");
      Regex namerepstr = new Regex("target.*\">");
      Regex sbr = new Regex("<font face=\"Arial, Helvetica\" size=\"2\" color=\"#FFFFFF\">[0-9]*</font>");
      Regex brrepstr = new Regex("<font face=\"Arial, Helvetica\" size=\"2\" color=\"#FFFFFF\">");

      //Connect to remote server and download HTML
      string address = "http://www.shoutcast.com/directory/?s=" + search + "&numresult=100&orderby=bitrate";
      HTMLbuffer = myWebClient.DownloadData(address);
      HTMLdownload = Encoding.ASCII.GetString(HTMLbuffer);

      //Check requested if genre or search exists
      if (search == "")
      {
        return null;
      }

      mc = nterm.Matches(HTMLdownload);
      if (mc.Count > 0)
      {
        return null;
      }
      mc = ngenre.Matches(HTMLdownload);
      if (mc.Count > 0)
      {
        return null;
      }

      try
      {

        //Build index numbers for stations
        int ind = 0;
        for (int i = counter; i < counter + count; i++)
        {
          if (i < 10)
          {

            SCstaindex[ind] = "0" + i.ToString();
          }
          else SCstaindex[ind] = i.ToString();
          ind++;
        }

        //Extract Station Number and store in SCstanum[]
        mc = snum.Matches(HTMLdownload);
        // Loop through the match collection to retrieve all 
        // matches and positions.
        total = mc.Count;
        for (int i = 0; i < mc.Count; i++)
        {
          // Add the match string to the string array.   
          SCstanum[i] = mc[i].Value.Replace("rn=", "");
          // Record the character position where the match was found.
          found[i] = mc[i].Index;
        }


        //Extract Station Name and store in SCstaname
        mc = sname.Matches(HTMLdownload);
        for (int i = 0; i < mc.Count; i++)
        {
          // Add the match string to the string array.   
          SCstaname[i] = mc[i].Value;
          SCstaname[i] = namerepstr.Replace(SCstaname[i], "");
          SCstaname[i] = SCstaname[i].Replace("</a>", "");
          // Record the character position where the match was found.
          found[i] = mc[i].Index;
        }


        //Extract bitrate and store in SCstabr
        mc = sbr.Matches(HTMLdownload);
        for (int i = 0; i < mc.Count; i++)
        {
          // Add the match string to the string array.   
          SCstabr[i] = mc[i].Value;
          SCstabr[i] = brrepstr.Replace(SCstabr[i], "");
          SCstabr[i] = SCstabr[i].Replace("</font>", "");
          // Record the character position where the match was found.
          found[i] = mc[i].Index;
        }

      }
      catch (IndexOutOfRangeException)
      {
        return null;
      }
      catch (WebException)
      {
        return null;
      }
      catch (Exception)
      {
        return null;
      }

      //Write stuff to array list
      ArrayList stations = new ArrayList();
      for (int i = 0; i < total; i++)
      {
        SHOUTcastStation station = new SHOUTcastStation();
        station.url = SCstanum[i];
        station.name = SCstaname[i];
        station.bitrate = Convert.ToInt32(SCstabr[i]);
        stations.Add(station);
      }
      return stations;
    }

    private void SearchButton_Click(object sender, System.EventArgs e)
    {
      //Check if the user has entered any text
      if (SearchText.Text == null || SearchText.Text == "") return;
      ArrayList Station_List = new ArrayList();
      //Show wait window
      WaitLabel.Focus();
      WaitLabel.Visible = true;
      WaitLabel.Enabled = true;
      Station_List = Search(SearchText.Text);
      WaitLabel.Enabled = false;
      WaitLabel.Visible = false;
      if (Station_List == null)
      {
        MessageBox.Show(this, "No stations found please search again.");
        return;
      }
      if (ResultsBox.Items != null) ResultsBox.Items.Clear();
      foreach (SHOUTcastStation station in Station_List)
      {
        SHOUTcastStation radiostation = new SHOUTcastStation();
        radiostation.name = station.name;
        radiostation.url = station.url;
        radiostation.bitrate = station.bitrate;
        ListViewItem listItem = new ListViewItem(new string[] {   radiostation.bitrate.ToString(),
																		  radiostation.name,
																		  radiostation.url
																	  });
        listItem.Tag = radiostation;
        ResultsBox.Items.Add(listItem);
      }
      ResultsBox.Focus();
    }

    private void AddButton_Click(object sender, System.EventArgs e)
    {
      foreach (ListViewItem listItem in ResultsBox.SelectedItems)
      {
        WaitLabel.Focus();
        WaitLabel.Visible = true;
        WaitLabel.Enabled = true;
        WebClient myWebClient = new WebClient();         			   // Create a new WebClient instance.	
        string file_loc = "http://www.shoutcast.com/sbin/shoutcast-playlist.pls?rn=";
        //Get station file number to get the file
        SHOUTcastStation radiostation = listItem.Tag as SHOUTcastStation;
        file_loc = file_loc + radiostation.url + "&file=filename.pls";
        //Download the file to extract url information
        myWebClient.DownloadFile(file_loc, "SCtemp.tmp");
        //Open file and check if we have met our daily limit if so quit
        StreamReader tr = File.OpenText("SCtemp.tmp");
        string parse_me = tr.ReadToEnd();
        tr.Close();
        File.Delete("SCtemp.tmp");
        if (parse_me.StartsWith("Too many requests.  Try again tomorrow."))
        {
          MessageBox.Show(this, "Too many requests, try again in a few minutes.");
          WaitLabel.Enabled = false;
          WaitLabel.Visible = false;
          Selected_Radiostation = null;
          return;
        }
        else
        {
          //Get our station info
          Regex url_loc = new Regex("http://[^\r\n>]*");
          MatchCollection mc;
          mc = url_loc.Matches(parse_me);
          if (mc.Count < 1)
          {
            MessageBox.Show(this, "Something weird happened try again.");
            WaitLabel.Enabled = false;
            WaitLabel.Visible = false;
            Selected_Radiostation = null;
            return;
          }
          //Get the first link of the collected streams
          radiostation.url = mc[0].Value.Replace("Title", "");
          Selected_Radiostation = radiostation;
          WaitLabel.Enabled = false;
          WaitLabel.Visible = false;
          this.Close();
          return;
        }

      }

    }

    private void SearchSHOUTcast_Load(object sender, System.EventArgs e)
    {
      //Check if we are connected to a network
      if (!SystemInformation.Network)
      {
        MessageBox.Show(this, "You need to be connected to a network to search for stations");
        this.Close();
        return;
      }
      //Create necessary registry temp keys to begin search
      bool reged = false;
      using (RegistryKey rs = Registry.ClassesRoot.OpenSubKey("CLSID\\"))
      {
        string[] names = rs.GetSubKeyNames();
        foreach (string s in names)
        {
          //Check for shoutcastsource.ax key is registered
          if (s.StartsWith("{68F540E9-766F-44D2-AB07-E26CC6D27A79}"))
          {
            reged = true;
            break;
          }
        }
      }
      if (!reged)
      {
        //Inform the user that they need to register the source to play streams
        MessageBox.Show(this, "The SHOUTcast source needs to be registered in your system \nto play shoutcast streams with Media Player.\nCheck this link for additional info:\nhttp://www.maisenbachers.de/dokuw/howto:myradio:createshortcuts");
        this.Close();
        return;
      }
    }
  }
}
