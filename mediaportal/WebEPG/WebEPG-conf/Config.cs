/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
using System.Xml;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.GUI.Library;

namespace WebEPG_conf
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class fChannels : System.Windows.Forms.Form
	{
		private string startDirectory;
		private Form selection;
		private TreeNode tChannels;
		private TreeNode tGrabbers;
		private SortedList CountryList;
		private SortedList ChannelList;
		private Hashtable hChannelInfo;
		private Hashtable hGrabberInfo;
		private EventHandler handler;
		private EventHandler selectHandler;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label Grabber;
		private System.Windows.Forms.Label Lang;
		private System.Windows.Forms.Label l_cID;
		private System.Windows.Forms.Button bAdd;
		private System.Windows.Forms.Button bImport;
		private System.Windows.Forms.ListBox lbChannels;
		private System.Windows.Forms.GroupBox gbChannelDetails;
		private System.Windows.Forms.TextBox tbGrabSite;
		private System.Windows.Forms.TextBox tbLanguage;
		private System.Windows.Forms.GroupBox gbGrabber;
		private System.Windows.Forms.Label lGrabDay;
		private System.Windows.Forms.Button bSave;
		private System.Windows.Forms.TextBox tbCount;
		private System.Windows.Forms.Label lCount;
		private System.Windows.Forms.Button bRemove;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox tbDisplayName;
		private System.Windows.Forms.Button bUpdate;
		private System.Windows.Forms.Button bChannelID;
		private System.Windows.Forms.Button bGrabber;
		private System.Windows.Forms.TextBox tbChannelName;
		private System.Windows.Forms.NumericUpDown nMaxGrab;
		private System.Windows.Forms.OpenFileDialog importFile;
		private System.Windows.Forms.Label lSiteDesc;
		private System.Windows.Forms.Label lGuideDays;
		private System.Windows.Forms.TextBox tbGrabDays;
		private System.Windows.Forms.TextBox tbSiteDescription;
		private System.Windows.Forms.Label lShour;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.NumericUpDown nStart;
		private System.Windows.Forms.CheckBox cbLinked;
		private System.Windows.Forms.GroupBox gbLinked;
		private System.Windows.Forms.NumericUpDown nEnd;
		private System.ComponentModel.IContainer components;

		public fChannels()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			selectHandler = new EventHandler(DoSelect);
			handler = new EventHandler(DoEvent);
			bImport.Click += handler;
			bUpdate.Click += handler;
			bSave.Click += handler;
			bAdd.Click += handler;
			bRemove.Click += handler;
			bGrabber.Click += handler;
			bChannelID.Click += handler;
			lbChannels.SelectedValueChanged += handler;
			tbDisplayName.TextChanged += handler;
			nStart.Click += handler;
			nEnd.Click += handler;
			cbLinked.CheckStateChanged += handler;


			startDirectory = Environment.CurrentDirectory;

			LoadConfig();

			UpdateList("", -1);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.bAdd = new System.Windows.Forms.Button();
			this.bImport = new System.Windows.Forms.Button();
			this.lbChannels = new System.Windows.Forms.ListBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.lCount = new System.Windows.Forms.Label();
			this.tbCount = new System.Windows.Forms.TextBox();
			this.bSave = new System.Windows.Forms.Button();
			this.lGrabDay = new System.Windows.Forms.Label();
			this.nMaxGrab = new System.Windows.Forms.NumericUpDown();
			this.gbChannelDetails = new System.Windows.Forms.GroupBox();
			this.bUpdate = new System.Windows.Forms.Button();
			this.tbDisplayName = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.bChannelID = new System.Windows.Forms.Button();
			this.l_cID = new System.Windows.Forms.Label();
			this.tbChannelName = new System.Windows.Forms.TextBox();
			this.Lang = new System.Windows.Forms.Label();
			this.tbLanguage = new System.Windows.Forms.TextBox();
			this.bRemove = new System.Windows.Forms.Button();
			this.gbGrabber = new System.Windows.Forms.GroupBox();
			this.lSiteDesc = new System.Windows.Forms.Label();
			this.lGuideDays = new System.Windows.Forms.Label();
			this.tbGrabDays = new System.Windows.Forms.TextBox();
			this.bGrabber = new System.Windows.Forms.Button();
			this.Grabber = new System.Windows.Forms.Label();
			this.tbGrabSite = new System.Windows.Forms.TextBox();
			this.importFile = new System.Windows.Forms.OpenFileDialog();
			this.tbSiteDescription = new System.Windows.Forms.TextBox();
			this.lShour = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.nStart = new System.Windows.Forms.NumericUpDown();
			this.nEnd = new System.Windows.Forms.NumericUpDown();
			this.cbLinked = new System.Windows.Forms.CheckBox();
			this.gbLinked = new System.Windows.Forms.GroupBox();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.nMaxGrab)).BeginInit();
			this.gbChannelDetails.SuspendLayout();
			this.gbGrabber.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.nStart)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nEnd)).BeginInit();
			this.gbLinked.SuspendLayout();
			this.SuspendLayout();
			// 
			// bAdd
			// 
			this.bAdd.Location = new System.Drawing.Point(8, 368);
			this.bAdd.Name = "bAdd";
			this.bAdd.Size = new System.Drawing.Size(72, 24);
			this.bAdd.TabIndex = 12;
			this.bAdd.Text = "Add";
			// 
			// bImport
			// 
			this.bImport.Location = new System.Drawing.Point(16, 368);
			this.bImport.Name = "bImport";
			this.bImport.Size = new System.Drawing.Size(72, 24);
			this.bImport.TabIndex = 11;
			this.bImport.Text = "Import";
			// 
			// lbChannels
			// 
			this.lbChannels.Location = new System.Drawing.Point(32, 32);
			this.lbChannels.MultiColumn = true;
			this.lbChannels.Name = "lbChannels";
			this.lbChannels.Size = new System.Drawing.Size(168, 277);
			this.lbChannels.TabIndex = 10;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.lCount);
			this.groupBox2.Controls.Add(this.tbCount);
			this.groupBox2.Controls.Add(this.bImport);
			this.groupBox2.Controls.Add(this.bSave);
			this.groupBox2.Controls.Add(this.lGrabDay);
			this.groupBox2.Controls.Add(this.nMaxGrab);
			this.groupBox2.Location = new System.Drawing.Point(16, 8);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(200, 400);
			this.groupBox2.TabIndex = 13;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "My Channels";
			this.groupBox2.Enter += new System.EventHandler(this.groupBox2_Enter);
			// 
			// lCount
			// 
			this.lCount.Location = new System.Drawing.Point(104, 312);
			this.lCount.Name = "lCount";
			this.lCount.Size = new System.Drawing.Size(80, 16);
			this.lCount.TabIndex = 1;
			this.lCount.Text = "Channel Count";
			// 
			// tbCount
			// 
			this.tbCount.Location = new System.Drawing.Point(16, 304);
			this.tbCount.Name = "tbCount";
			this.tbCount.Size = new System.Drawing.Size(72, 20);
			this.tbCount.TabIndex = 0;
			this.tbCount.Text = "";
			// 
			// bSave
			// 
			this.bSave.Location = new System.Drawing.Point(112, 368);
			this.bSave.Name = "bSave";
			this.bSave.Size = new System.Drawing.Size(72, 24);
			this.bSave.TabIndex = 16;
			this.bSave.Text = "Save";
			// 
			// lGrabDay
			// 
			this.lGrabDay.Location = new System.Drawing.Point(104, 336);
			this.lGrabDay.Name = "lGrabDay";
			this.lGrabDay.Size = new System.Drawing.Size(72, 16);
			this.lGrabDay.TabIndex = 9;
			this.lGrabDay.Text = "Guide Days";
			// 
			// nMaxGrab
			// 
			this.nMaxGrab.Location = new System.Drawing.Point(16, 328);
			this.nMaxGrab.Maximum = new System.Decimal(new int[] {
																	 14,
																	 0,
																	 0,
																	 0});
			this.nMaxGrab.Minimum = new System.Decimal(new int[] {
																	 1,
																	 0,
																	 0,
																	 0});
			this.nMaxGrab.Name = "nMaxGrab";
			this.nMaxGrab.Size = new System.Drawing.Size(72, 20);
			this.nMaxGrab.TabIndex = 13;
			this.nMaxGrab.Value = new System.Decimal(new int[] {
																   2,
																   0,
																   0,
																   0});
			// 
			// gbChannelDetails
			// 
			this.gbChannelDetails.Controls.Add(this.bUpdate);
			this.gbChannelDetails.Controls.Add(this.tbDisplayName);
			this.gbChannelDetails.Controls.Add(this.label4);
			this.gbChannelDetails.Controls.Add(this.bChannelID);
			this.gbChannelDetails.Controls.Add(this.l_cID);
			this.gbChannelDetails.Controls.Add(this.tbChannelName);
			this.gbChannelDetails.Controls.Add(this.Lang);
			this.gbChannelDetails.Controls.Add(this.tbLanguage);
			this.gbChannelDetails.Controls.Add(this.bAdd);
			this.gbChannelDetails.Controls.Add(this.bRemove);
			this.gbChannelDetails.Controls.Add(this.gbGrabber);
			this.gbChannelDetails.Location = new System.Drawing.Point(224, 8);
			this.gbChannelDetails.Name = "gbChannelDetails";
			this.gbChannelDetails.Size = new System.Drawing.Size(312, 400);
			this.gbChannelDetails.TabIndex = 14;
			this.gbChannelDetails.TabStop = false;
			this.gbChannelDetails.Text = "Channel Details";
			// 
			// bUpdate
			// 
			this.bUpdate.Location = new System.Drawing.Point(120, 368);
			this.bUpdate.Name = "bUpdate";
			this.bUpdate.Size = new System.Drawing.Size(72, 24);
			this.bUpdate.TabIndex = 18;
			this.bUpdate.Text = "Update";
			this.bUpdate.Visible = false;
			// 
			// tbDisplayName
			// 
			this.tbDisplayName.Location = new System.Drawing.Point(88, 24);
			this.tbDisplayName.Name = "tbDisplayName";
			this.tbDisplayName.Size = new System.Drawing.Size(176, 20);
			this.tbDisplayName.TabIndex = 13;
			this.tbDisplayName.Text = "";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(16, 24);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(80, 16);
			this.label4.TabIndex = 10;
			this.label4.Text = "Display Name";
			// 
			// bChannelID
			// 
			this.bChannelID.Location = new System.Drawing.Point(272, 48);
			this.bChannelID.Name = "bChannelID";
			this.bChannelID.Size = new System.Drawing.Size(22, 20);
			this.bChannelID.TabIndex = 9;
			this.bChannelID.Text = "...";
			// 
			// l_cID
			// 
			this.l_cID.Location = new System.Drawing.Point(16, 48);
			this.l_cID.Name = "l_cID";
			this.l_cID.Size = new System.Drawing.Size(64, 23);
			this.l_cID.TabIndex = 8;
			this.l_cID.Text = "Channel";
			// 
			// tbChannelName
			// 
			this.tbChannelName.Location = new System.Drawing.Point(88, 48);
			this.tbChannelName.Name = "tbChannelName";
			this.tbChannelName.ReadOnly = true;
			this.tbChannelName.Size = new System.Drawing.Size(176, 20);
			this.tbChannelName.TabIndex = 7;
			this.tbChannelName.Text = "";
			// 
			// Lang
			// 
			this.Lang.Location = new System.Drawing.Point(16, 72);
			this.Lang.Name = "Lang";
			this.Lang.Size = new System.Drawing.Size(56, 16);
			this.Lang.TabIndex = 5;
			this.Lang.Text = "Language";
			this.Lang.Visible = false;
			// 
			// tbLanguage
			// 
			this.tbLanguage.Location = new System.Drawing.Point(88, 72);
			this.tbLanguage.Name = "tbLanguage";
			this.tbLanguage.ReadOnly = true;
			this.tbLanguage.Size = new System.Drawing.Size(104, 20);
			this.tbLanguage.TabIndex = 4;
			this.tbLanguage.Text = "";
			this.tbLanguage.Visible = false;
			// 
			// bRemove
			// 
			this.bRemove.Location = new System.Drawing.Point(232, 368);
			this.bRemove.Name = "bRemove";
			this.bRemove.Size = new System.Drawing.Size(72, 24);
			this.bRemove.TabIndex = 17;
			this.bRemove.Text = "Remove";
			// 
			// gbGrabber
			// 
			this.gbGrabber.Controls.Add(this.gbLinked);
			this.gbGrabber.Controls.Add(this.tbSiteDescription);
			this.gbGrabber.Controls.Add(this.lSiteDesc);
			this.gbGrabber.Controls.Add(this.lGuideDays);
			this.gbGrabber.Controls.Add(this.tbGrabDays);
			this.gbGrabber.Controls.Add(this.bGrabber);
			this.gbGrabber.Controls.Add(this.Grabber);
			this.gbGrabber.Controls.Add(this.tbGrabSite);
			this.gbGrabber.Location = new System.Drawing.Point(8, 104);
			this.gbGrabber.Name = "gbGrabber";
			this.gbGrabber.Size = new System.Drawing.Size(296, 256);
			this.gbGrabber.TabIndex = 15;
			this.gbGrabber.TabStop = false;
			this.gbGrabber.Text = "Grabber Details";
			// 
			// lSiteDesc
			// 
			this.lSiteDesc.Location = new System.Drawing.Point(8, 48);
			this.lSiteDesc.Name = "lSiteDesc";
			this.lSiteDesc.Size = new System.Drawing.Size(64, 24);
			this.lSiteDesc.TabIndex = 11;
			this.lSiteDesc.Text = "Site Description";
			// 
			// lGuideDays
			// 
			this.lGuideDays.Location = new System.Drawing.Point(8, 136);
			this.lGuideDays.Name = "lGuideDays";
			this.lGuideDays.Size = new System.Drawing.Size(56, 30);
			this.lGuideDays.TabIndex = 8;
			this.lGuideDays.Text = "Guide Days";
			this.lGuideDays.Click += new System.EventHandler(this.label2_Click);
			// 
			// tbGrabDays
			// 
			this.tbGrabDays.Location = new System.Drawing.Point(80, 144);
			this.tbGrabDays.Name = "tbGrabDays";
			this.tbGrabDays.ReadOnly = true;
			this.tbGrabDays.Size = new System.Drawing.Size(104, 20);
			this.tbGrabDays.TabIndex = 7;
			this.tbGrabDays.Text = "";
			// 
			// bGrabber
			// 
			this.bGrabber.Location = new System.Drawing.Point(264, 24);
			this.bGrabber.Name = "bGrabber";
			this.bGrabber.Size = new System.Drawing.Size(22, 20);
			this.bGrabber.TabIndex = 6;
			this.bGrabber.Text = "...";
			// 
			// Grabber
			// 
			this.Grabber.Location = new System.Drawing.Point(8, 24);
			this.Grabber.Name = "Grabber";
			this.Grabber.Size = new System.Drawing.Size(56, 23);
			this.Grabber.TabIndex = 1;
			this.Grabber.Text = "Site";
			// 
			// tbGrabSite
			// 
			this.tbGrabSite.Location = new System.Drawing.Point(80, 24);
			this.tbGrabSite.Name = "tbGrabSite";
			this.tbGrabSite.ReadOnly = true;
			this.tbGrabSite.Size = new System.Drawing.Size(176, 20);
			this.tbGrabSite.TabIndex = 0;
			this.tbGrabSite.Text = "";
			// 
			// importFile
			// 
			this.importFile.FileName = "ChannelList.xml";
			this.importFile.Filter = "Xml Files (*.xml)|*.xml";
			this.importFile.Title = "Import MP Channel File";
			// 
			// tbSiteDescription
			// 
			this.tbSiteDescription.BackColor = System.Drawing.SystemColors.ScrollBar;
			this.tbSiteDescription.Location = new System.Drawing.Point(80, 48);
			this.tbSiteDescription.Multiline = true;
			this.tbSiteDescription.Name = "tbSiteDescription";
			this.tbSiteDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.tbSiteDescription.Size = new System.Drawing.Size(176, 88);
			this.tbSiteDescription.TabIndex = 12;
			this.tbSiteDescription.Text = "";
			// 
			// lShour
			// 
			this.lShour.Location = new System.Drawing.Point(120, 18);
			this.lShour.Name = "lShour";
			this.lShour.Size = new System.Drawing.Size(40, 16);
			this.lShour.TabIndex = 15;
			this.lShour.Text = "Start -";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(208, 18);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(64, 16);
			this.label2.TabIndex = 17;
			this.label2.Text = "End (Hour)";
			// 
			// nStart
			// 
			this.nStart.Location = new System.Drawing.Point(72, 16);
			this.nStart.Maximum = new System.Decimal(new int[] {
																   23,
																   0,
																   0,
																   0});
			this.nStart.Name = "nStart";
			this.nStart.Size = new System.Drawing.Size(40, 20);
			this.nStart.TabIndex = 18;
			this.nStart.Value = new System.Decimal(new int[] {
																 18,
																 0,
																 0,
																 0});
			// 
			// nEnd
			// 
			this.nEnd.Location = new System.Drawing.Point(168, 16);
			this.nEnd.Maximum = new System.Decimal(new int[] {
																 23,
																 0,
																 0,
																 0});
			this.nEnd.Name = "nEnd";
			this.nEnd.Size = new System.Drawing.Size(40, 20);
			this.nEnd.TabIndex = 19;
			this.nEnd.Value = new System.Decimal(new int[] {
															   23,
															   0,
															   0,
															   0});
			// 
			// cbLinked
			// 
			this.cbLinked.Location = new System.Drawing.Point(8, 16);
			this.cbLinked.Name = "cbLinked";
			this.cbLinked.Size = new System.Drawing.Size(72, 24);
			this.cbLinked.TabIndex = 20;
			this.cbLinked.Text = "Enable";
			// 
			// gbLinked
			// 
			this.gbLinked.Controls.Add(this.nEnd);
			this.gbLinked.Controls.Add(this.label2);
			this.gbLinked.Controls.Add(this.nStart);
			this.gbLinked.Controls.Add(this.cbLinked);
			this.gbLinked.Controls.Add(this.lShour);
			this.gbLinked.Location = new System.Drawing.Point(8, 176);
			this.gbLinked.Name = "gbLinked";
			this.gbLinked.Size = new System.Drawing.Size(280, 72);
			this.gbLinked.TabIndex = 21;
			this.gbLinked.TabStop = false;
			this.gbLinked.Text = "Linked Pages";
			this.gbLinked.Visible = false;
			// 
			// fChannels
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(552, 421);
			this.Controls.Add(this.gbChannelDetails);
			this.Controls.Add(this.lbChannels);
			this.Controls.Add(this.groupBox2);
			this.MaximizeBox = false;
			this.Name = "fChannels";
			this.Text = "WebEPG Config";
			this.groupBox2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.nMaxGrab)).EndInit();
			this.gbChannelDetails.ResumeLayout(false);
			this.gbGrabber.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.nStart)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nEnd)).EndInit();
			this.gbLinked.ResumeLayout(false);
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

		private void label2_Click(object sender, System.EventArgs e)
		{
		
		}

		private void groupBox2_Enter(object sender, System.EventArgs e)
		{
		
		}

		private void DoSelect(Object source, EventArgs e)
		{
			if(source==selection)
			{
				if(selection.Text == "Selection ")
				{
					this.Activate();
					string[] id = (string[]) selection.Tag;
					selection.Text = "Selection";

					tbChannelName.Tag = id[0];
					ChannelInfo info = (ChannelInfo) hChannelInfo[id[0]];
					if(info != null)
					{
						tbChannelName.Text = info.FullName;
						Log.WriteFile(Log.LogType.Log, false, "WebEPG Config: Selection: {0}", info.FullName);

						GrabberInfo gInfo = (GrabberInfo) info.GrabberList[id[1]];
						if(gInfo != null)
						{
							tbGrabSite.Text = gInfo.GrabberName;
							tbGrabSite.Tag = gInfo.GrabberID;
							tbGrabDays.Text = gInfo.GrabDays.ToString();
							tbSiteDescription.Text = gInfo.SiteDesc;
							gbLinked.Visible = gInfo.Linked;
							if(!gInfo.Linked)
								cbLinked.Checked = gInfo.Linked;

							UpdateCurrent();
						}
						else
							tbGrabSite.Text = "(Unknown)";
					}
				}
			}
		}

		private void DoEvent(Object source, EventArgs e)
		{
			if(source==cbLinked)
			{
				nStart.ReadOnly =! cbLinked.Checked;
				nEnd.ReadOnly =! cbLinked.Checked;
				UpdateCurrent();
			}

			if(source==nStart)
			{
				if(nStart.Value > nEnd.Value)
					nEnd.Value = nStart.Value;
				nEnd.Minimum = nStart.Value;
				UpdateCurrent();
			}

			if(source==nEnd)
			{
				UpdateCurrent();
			}

			if(source==tbDisplayName)
			{
				bUpdate.Visible=true;
			}

			if(source==bImport)
			{
				Log.WriteFile(Log.LogType.Log, false, "WebEPG Config: Button: Import");
				if (importFile.ShowDialog() != DialogResult.Cancel)
				{
					Log.WriteFile(Log.LogType.Log, false, "WebEPG Config: Importing MP ChannelList: {0}", importFile.FileName);
					MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml(importFile.FileName);
					int version = xmlreader.GetValueAsInt("MP channel export list", "version", 0);
					if(version == 1)
					{
						//ChannelList.Clear();

						//int channels = xmlreader.GetValueAsInt("Group 0", "TOTAL CHANNELS", 0);

						for(int i=0; i <= 999; i++)
						{
							string name = xmlreader.GetValueAsString(i.ToString(), "Name", "");
							if(name != "")
							{
								if(ChannelList[name] == null)
								{
									ChannelInfo channel = new ChannelInfo();
									channel.DisplayName = name;
									ChannelList.Add(name, channel);
								}
							}
							else
							{
								break;
							}
						}
						UpdateList("", -1);
					}
					else
					{

					}
				}	
			}

			if(source==bSave)
			{
				Log.WriteFile(Log.LogType.Log, false, "WebEPG Config: Button: Save");
				string confFile = startDirectory + "\\WebEPG.xml";
				if(System.IO.File.Exists(confFile))
				{
					System.IO.File.Delete(confFile.Replace(".xml",".bak"));
					System.IO.File.Move(confFile,confFile.Replace(".xml",".bak"));
				}
				MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml(confFile);

				xmlwriter.SetValue("General", "MaxDays", nMaxGrab.Value.ToString());

				ChannelInfo[] infoList = new ChannelInfo[ChannelList.Count];

				int index=0;
				IDictionaryEnumerator Enumerator = ChannelList.GetEnumerator();
				while (Enumerator.MoveNext())
				{
					infoList[index] = (ChannelInfo) Enumerator.Value;
					if(infoList[index].ChannelID != null && infoList[index].DisplayName != null && infoList[index].PrimaryGrabberID != null)
						index++;
				}


				xmlwriter.SetValue("ChannelMap", "Count", index.ToString());

				for(int i=0; i < index; i++)
				{
					xmlwriter.SetValue((i+1).ToString(), "ChannelID", infoList[i].ChannelID);
					xmlwriter.SetValue((i+1).ToString(), "DisplayName", infoList[i].DisplayName);
					xmlwriter.SetValue((i+1).ToString(), "Grabber1", infoList[i].PrimaryGrabberID);
					if(infoList[i].Linked)
					{
						xmlwriter.SetValueAsBool((i+1).ToString(), "Grabber1-Linked", infoList[i].Linked);
						xmlwriter.SetValue((i+1).ToString(), "Grabber1-Start", infoList[i].linkStart);
						xmlwriter.SetValue((i+1).ToString(), "Grabber1-End", infoList[i].linkEnd);
					}
				}
				xmlwriter.Save();
			}

			if(source==bUpdate)
			{
				Log.WriteFile(Log.LogType.Log, false, "WebEPG Config: Button: Update");
				ReplaceCurrent();
				bUpdate.Visible=false;
			}

			if(source==bRemove)
			{
				Log.WriteFile(Log.LogType.Log, false, "WebEPG Config: Button: Remove");
				if(lbChannels.SelectedIndex != -1)
				{
					ChannelList.RemoveAt(lbChannels.SelectedIndex);
					UpdateList("", lbChannels.SelectedIndex);
				}
			}

			if(source==bAdd)
			{
				Log.WriteFile(Log.LogType.Log, false, "WebEPG Config: Button: Add");
				ChannelInfo info = new ChannelInfo();
				info.DisplayName = tbDisplayName.Text;

				while(ChannelList[info.DisplayName] != null)
					info.DisplayName+="*";

				info.FullName = tbChannelName.Text;
				info.ChannelID = (string) tbChannelName.Tag;
				info.PrimaryGrabberName = tbGrabSite.Text;
				info.PrimaryGrabberID = (string) tbGrabSite.Tag;
				info.Linked = cbLinked.Checked;
				if(info.Linked)
				{
					info.linkStart = (int) nStart.Value;
					info.linkEnd = (int) nEnd.Value;
				}
				ChannelList.Add(info.DisplayName, info);
				UpdateList(info.DisplayName, -1);
			}

			if(source==lbChannels)
			{
				if(lbChannels.SelectedIndex > -1)
				{
					ChannelInfo info = (ChannelInfo) ChannelList.GetByIndex(lbChannels.SelectedIndex);
					tbDisplayName.Text = info.DisplayName;
					tbChannelName.Tag = info.ChannelID;
					tbChannelName.Text = info.FullName;


					tbGrabSite.Text = info.PrimaryGrabberName;
					tbGrabSite.Tag = info.PrimaryGrabberID;
					if(info.PrimaryGrabberID != null)
					{
						GrabberInfo gInfo = (GrabberInfo) hGrabberInfo[info.PrimaryGrabberID];
						if(gInfo != null)
						{
							tbGrabDays.Text = gInfo.GrabDays.ToString();
							tbSiteDescription.Text = gInfo.SiteDesc;
							gbLinked.Visible = gInfo.Linked;

							if(info.Linked)
							{
								cbLinked.Checked = info.Linked;
								nStart.ReadOnly = !cbLinked.Checked;
								nEnd.ReadOnly = !cbLinked.Checked;
								nStart.Value = info.linkStart;
								nEnd.Value = info.linkEnd;
							}
						}
					}
				}
			}

//			if(source==bFolder)
//			{
//				if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
//					tbMPfolder.Text = folderBrowserDialog.SelectedPath;
//			}

			if(source==selection)
				selection=null;

			if(source==bChannelID)
			{
				if(selection == null)
				{
					selection = new fSelection(tChannels, tGrabbers, true);
					selection.MinimizeBox = false;
					selection.Closed += handler;
					selection.TextChanged += selectHandler;
					selection.Show();
				}
				else
				{
					selection.BringToFront();
				}
			}

			if(source==bGrabber)
			{
				if(selection == null)
				{
					selection = new fSelection(tChannels, tGrabbers, false);
					selection.MinimizeBox = false;
					selection.Closed += handler;
					selection.TextChanged += selectHandler;
					selection.Show();
				}
				else
				{
					//selection.Closed
					//selection.Show();
					//selection.M

					selection.BringToFront();
				}
			}
		}

		private void UpdateCurrent()
		{
			if(lbChannels.SelectedIndex != -1)
			{
				ChannelInfo info = (ChannelInfo) ChannelList.GetByIndex(lbChannels.SelectedIndex);

				info.DisplayName = tbDisplayName.Text;
				info.FullName = tbChannelName.Text;
				info.ChannelID = (string) tbChannelName.Tag;
				info.PrimaryGrabberName = tbGrabSite.Text;
				info.PrimaryGrabberID = (string) tbGrabSite.Tag;
				info.Linked = cbLinked.Checked;
				if(info.Linked)
				{
					info.linkStart = (int) nStart.Value;
					info.linkEnd = (int) nEnd.Value;
				}

				ChannelList.SetByIndex(lbChannels.SelectedIndex, info);
			}
		}

		private void ReplaceCurrent()
		{
			if(lbChannels.SelectedIndex != -1)
			{
				ChannelList.RemoveAt(lbChannels.SelectedIndex);

				ChannelInfo info = new ChannelInfo();

				info.DisplayName = tbDisplayName.Text;
				info.FullName = tbChannelName.Text;
				info.ChannelID = (string) tbChannelName.Tag;
				info.PrimaryGrabberName = tbGrabSite.Text;
				info.PrimaryGrabberID = (string) tbGrabSite.Tag;
				info.Linked = cbLinked.Checked;
				if(info.Linked)
				{
					info.linkStart = (int) nStart.Value;
					info.linkEnd = (int) nEnd.Value;
				}
				ChannelList.Add(info.DisplayName, info);

				UpdateList(info.DisplayName, -1);
			}
		}

		private void GetTreeGrabbers(ref TreeNode Main, string Location)
		{
			System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(Location); 
			System.IO.DirectoryInfo[] dirList = dir.GetDirectories();
			if(dirList.Length > 0)
			{
				for(int i=0; i < dirList.Length; i++)
				{     
					//LOAD FOLDERS
					System.IO.DirectoryInfo g = dirList[i];
					TreeNode MainNext = new TreeNode(g.Name); //
					GetTreeGrabbers(ref MainNext, g.FullName);
					Main.Nodes.Add(MainNext);
					//MainNext.Tag = (g.FullName); 
				}
			}
			else
			{
				GetGrabbers(ref Main, Location);
			}

		}

		private void GetGrabbers(ref TreeNode Main, string Location)
		{
			System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(Location); 
			Log.WriteFile(Log.LogType.Log, false, "WebEPG Config: Directory: {0}", Location);
			GrabberInfo gInfo;
			foreach (System.IO.FileInfo file in dir.GetFiles("*.xml"))
			{
				gInfo = new GrabberInfo();
				XmlDocument xml=new XmlDocument();
				XmlNodeList channelList;
				try 
				{
					Log.WriteFile(Log.LogType.Log, false, "WebEPG Config: File: {0}", file.Name);
					xml.Load(file.FullName);
					channelList = xml.DocumentElement.SelectNodes("/profile/section/entry");
  				
					XmlNode entryNode = xml.DocumentElement.SelectSingleNode("section[@name=\"Info\"]/entry[@name=\"GuideDays\"]");
					if (entryNode!=null)
						gInfo.GrabDays = int.Parse(entryNode.InnerText);
					entryNode = xml.DocumentElement.SelectSingleNode("section[@name=\"Info\"]/entry[@name=\"SiteDescription\"]");
					if (entryNode!=null)
						gInfo.SiteDesc = entryNode.InnerText;
					entryNode = xml.DocumentElement.SelectSingleNode("section[@name=\"Listing\"]/entry[@name=\"SubListingLink\"]");
					gInfo.Linked = false;
					if (entryNode!=null)
						gInfo.Linked = true;
				} 
				catch(System.Xml.XmlException ex) 
				{
					Log.WriteFile(Log.LogType.Log, false, "WebEPG Config: File open failed - XML error");
					return;
				}
				
				string GrabberSite = file.Name.Replace(".xml", "");
				GrabberSite = GrabberSite.Replace("_", ".");

				gInfo.GrabberID=file.Directory.Name + "\\" + file.Name;
				gInfo.GrabberName = GrabberSite;
				gInfo.Country = file.Directory.Name;
				hGrabberInfo.Add(gInfo.GrabberID, gInfo);

				if(CountryList[file.Directory.Name] == null)
					CountryList.Add(file.Directory.Name, new SortedList());

				TreeNode gNode = new TreeNode(GrabberSite);
				Main.Nodes.Add(gNode);
				//XmlNode cl=sectionList.Attributes.GetNamedItem("ChannelList");

				foreach (XmlNode nodeChannel in channelList)
				{
					if (nodeChannel.Attributes!=null)
					{
						XmlNode id = nodeChannel.ParentNode.Attributes.Item(0);
						if(id.InnerXml == "ChannelList")
						{
							id = nodeChannel.Attributes.Item(0);
							//idList.Add(id.InnerXml);

							ChannelInfo info = (ChannelInfo) hChannelInfo[id.InnerXml];
							if(info != null) // && info.GrabberList[gInfo.GrabberID] != null)
							{
								TreeNode tNode = new TreeNode(info.FullName);
								string [] tag = new string[2];
								tag[0] = info.ChannelID;
								tag[1] = gInfo.GrabberID;
								tNode.Tag = tag;
								gNode.Nodes.Add(tNode);
								if(info.GrabberList == null)
									info.GrabberList = new SortedList();
								if(info.GrabberList[gInfo.GrabberID] == null)
									info.GrabberList.Add(gInfo.GrabberID, gInfo);
							}
							else
							{
								info = new ChannelInfo();
								info.ChannelID = id.InnerXml;
								info.FullName = info.ChannelID;
								info.GrabberList = new SortedList();
								info.GrabberList.Add(gInfo.GrabberID, gInfo);
								hChannelInfo.Add(info.ChannelID, info);

								TreeNode tNode = new TreeNode(info.FullName);
								string [] tag = new string[2];
								tag[0] = info.ChannelID;
								tag[1] = gInfo.GrabberID;
								tNode.Tag = tag;
								gNode.Nodes.Add(tNode);
							}
						}
					}
				}
			}
		}

		private void GetTreeChannels(string Location) //ref TreeNode Main, string Location)
		{
			System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(Location);
			if(dir.Exists) 
			{
				System.IO.DirectoryInfo[] dirList = dir.GetDirectories();
				if(dirList.Length > 0)
				{
					for(int i=0; i < dirList.Length; i++)
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
			Log.WriteFile(Log.LogType.Log, false, "WebEPG Config: Directory: {0}", Location);
			foreach (System.IO.FileInfo file in dir.GetFiles("*.xml")) 
			{
				Log.WriteFile(Log.LogType.Log, false, "WebEPG Config: File: {0}", file.Name);
				ChannelInfo info = GetChannelInfo(file.FullName);
				if(info != null)
				{
					//TreeNode cNode = new TreeNode(info.FullName);
					//Main.Nodes.Add(cNode);

					if(info.GrabberList.Count != 0)
					{
						
						IDictionaryEnumerator Enumerator = info.GrabberList.GetEnumerator();

						while (Enumerator.MoveNext())
						{
							//TreeNode tNode = new TreeNode((string) Enumerator.Key);
							//string [] tag = new string[2];
							//tag[0] = info.ChannelID;
							//tag[1] = (string) Enumerator.Key;
							//tNode.Tag = tag;
							//cNode.Nodes.Add(tNode);
							if(hChannelInfo[info.ChannelID] == null)
								hChannelInfo.Add(info.ChannelID, info);
						}
					}
				}
			}
		}

		private ChannelInfo GetChannelInfo(string filename)
		{
			MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml(filename);
			ChannelInfo info = new ChannelInfo();

			info.FullName = xmlreader.GetValueAsString("ChannelInfo", "FullName", "");
			if(info.FullName == "")
			{
				Log.WriteFile(Log.LogType.Log, false, "WebEPG Config: File error: FullName not found");
				return null;
			}
			info.ChannelID = xmlreader.GetValueAsString("ChannelInfo", "ChannelID", "");
			if(info.ChannelID == "")
			{
				Log.WriteFile(Log.LogType.Log, false, "WebEPG Config: File error: ChannelID not found");
				return null;
			}
			int GrabberCount = xmlreader.GetValueAsInt("ChannelInfo", "Grabbers", 0);
			if(GrabberCount == 0)
			{
				Log.WriteFile(Log.LogType.Log, false, "WebEPG Config: File error: Grabbers not found");
				return null;
			}

			info.GrabberList = new SortedList();
//			for(int i=0; i < GrabberCount; i++)
//			{
//				string GrabberNumb = "Grabber" + (i+1).ToString();
//				string GrabberID = xmlreader.GetValueAsString("ChannelInfo", GrabberNumb, "");
//				if(GrabberID == "")
//				{
//					Log.WriteFile(Log.LogType.Log, false, "WebEPG Config: File error: {0} not found", GrabberNumb);
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
			int i=0;
			int selectedIndex=-1;

			while (Enumerator.MoveNext())
			{
				ChannelInfo channel = (ChannelInfo) Enumerator.Value;
				if(channel.DisplayName == select)
					selectedIndex=i;
				list[i++] = channel.DisplayName;
			}
			tbCount.Text = ChannelList.Count.ToString();
			lbChannels.DataSource = list;
			if(selectedIndex > 0)
				lbChannels.SelectedIndex = selectedIndex;
			if(index > 0)
			{
				if(index >= ChannelList.Count)
					index = ChannelList.Count-1;
				lbChannels.SelectedIndex = index;
			}

			bUpdate.Visible=false;
		}

		private void LoadConfig()
		{
			Log.WriteFile(Log.LogType.Log, false, "WebEPG Config: Loading Channels");
			hChannelInfo = new Hashtable();

			if(System.IO.File.Exists(startDirectory + "\\channels\\channels.xml"))
			{
				Log.WriteFile(Log.LogType.Log, false, "WebEPG Config: Loading Existing channels.xml");
				MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml(startDirectory + "\\channels\\channels.xml");
				int channelCount = xmlreader.GetValueAsInt("ChannelInfo", "TotalChannels", 0);	

				for (int i = 0; i < channelCount; i++)
				{
					ChannelInfo channel = new ChannelInfo();
					channel.ChannelID = xmlreader.GetValueAsString(i.ToString(), "ChannelID", "");
					channel.FullName = xmlreader.GetValueAsString(i.ToString(), "FullName", "");
					hChannelInfo.Add(channel.ChannelID, channel);
				}
			}

			Log.WriteFile(Log.LogType.Log, false, "WebEPG Config: Loading Grabbers");
			hGrabberInfo = new Hashtable();
			CountryList = new SortedList();
			tGrabbers = new TreeNode("Web Sites");
			if(System.IO.Directory.Exists(startDirectory + "\\Grabbers"))
				GetTreeGrabbers(ref tGrabbers, startDirectory + "\\Grabbers");
			else
				Log.WriteFile(Log.LogType.Log, true, "WebEPG Config: Cannot find grabbers directory");


			IDictionaryEnumerator Enumerator = hChannelInfo.GetEnumerator();
			while (Enumerator.MoveNext())
			{
				ChannelInfo info = (ChannelInfo) Enumerator.Value;
				if(info.ChannelID != null && info.FullName != null)
				{
					if(info.GrabberList != null)
					{
						IDictionaryEnumerator grabEnum = info.GrabberList.GetEnumerator();
						while (grabEnum.MoveNext())
						{
							GrabberInfo gInfo = (GrabberInfo) grabEnum.Value;
							SortedList chList = (SortedList) CountryList[gInfo.Country];
							if(chList[info.ChannelID] == null)
							{
								chList.Add(info.ChannelID, gInfo.GrabberID);
								//CountryList.Remove(gInfo.Country);
								//CountryList.Add(gInfo.Country, chList);
							}
						}
					}
				}
			}

			tChannels = new TreeNode("Channels");
			IDictionaryEnumerator countryEnum = CountryList.GetEnumerator();
			while (countryEnum.MoveNext())
			{
				SortedList chList = (SortedList) countryEnum.Value;
				TreeNode cNode = new TreeNode();
				cNode.Text = (string) countryEnum.Key;

				IDictionaryEnumerator chEnum = chList.GetEnumerator();
				while (chEnum.MoveNext())
				{
					TreeNode chNode = new TreeNode();

					ChannelInfo info = (ChannelInfo) hChannelInfo[chEnum.Key];
					chNode.Text = info.FullName;
					string [] tag = new string[2];
					tag[0] = info.ChannelID;
					tag[1] = (string) chEnum.Value;
					chNode.Tag = tag;

					cNode.Nodes.Add(chNode);
				}

				tChannels.Nodes.Add(cNode);
			}

			//
			// TODO: Add any constructor code after InitializeComponent call
			//

			ChannelList = new SortedList();

			if(System.IO.File.Exists(startDirectory + "\\WebEPG.xml"))
			{
				Log.WriteFile(Log.LogType.Log, false, "WebEPG Config: Loading Existing WebEPG.xml");
				MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml(startDirectory + "\\WebEPG.xml");
				nMaxGrab.Value = xmlreader.GetValueAsInt("General", "MaxDays", 1);
				int channelCount = xmlreader.GetValueAsInt("ChannelMap", "Count", 0);	

				for (int i = 1; i <= channelCount; i++)
				{
					ChannelInfo channel = new ChannelInfo();
					channel.ChannelID = xmlreader.GetValueAsString(i.ToString(), "ChannelID", "");
					channel.DisplayName = xmlreader.GetValueAsString(i.ToString(), "DisplayName", "");

					if(ChannelList[channel.DisplayName] == null)
					{

						ChannelInfo info = (ChannelInfo) hChannelInfo[channel.ChannelID];
						channel.FullName = "(Unknown)";
						if(info != null)
							channel.FullName = info.FullName;

						string GrabberID = xmlreader.GetValueAsString(i.ToString(), "Grabber1", "");
						if(GrabberID != "")
						{
							int start = GrabberID.IndexOf("\\") + 1;
							int end =  GrabberID.LastIndexOf(".");
							
							string GrabberSite = GrabberID.Substring(start, end-start);
							GrabberSite = GrabberSite.Replace("_", ".");
							channel.PrimaryGrabberName = GrabberSite;
							channel.PrimaryGrabberID = GrabberID;
							channel.Linked = xmlreader.GetValueAsBool(i.ToString(), "Grabber1-Linked", false);
							if(channel.Linked)
							{
								channel.linkStart = xmlreader.GetValueAsInt(i.ToString(), "Grabber1-Start", 18);
								channel.linkEnd = xmlreader.GetValueAsInt(i.ToString(), "Grabber1-End", 23);
							}

							ChannelList.Add(channel.DisplayName, channel);
						}
					}
				}
			}
		}
	}
}