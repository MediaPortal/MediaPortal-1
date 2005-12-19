/* 
 *	Copyright (C) 2005 Team MediaPortal
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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.GUI.Library;

using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;


namespace MediaPortal.Configuration.Sections
{
	/// <summary>
	/// Summary description for FireDTVRemote.
	/// </summary>
	public class FireDTVRemote : MediaPortal.Configuration.SectionSettings
	{
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Splitter splitter1;
		private System.Data.DataSet dataSet1;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.Label labelAction;
		private System.Windows.Forms.Label labelKey;
		private System.Windows.Forms.Label labelKeyCode;
		private System.Windows.Forms.ComboBox comboBoxAction;
		private System.Windows.Forms.TextBox textBoxKey;
		private System.Windows.Forms.ComboBox comboBoxKeyCode;
		private System.Windows.Forms.ComboBox comboBoxGotoWindow;
		private System.Windows.Forms.Label label1;
		private System.Data.DataTable RemoteControl;
		private System.Data.DataColumn dataColumn1;
		private System.Data.DataColumn dataColumn2;
		private System.Data.DataColumn dataColumn3;
		private System.Data.DataTable RemoteControlKeys;
		private System.Data.DataColumn RemoteID;
		private System.Data.DataColumn KeyCode;
		private System.Data.DataColumn ActionType;
		private System.Data.DataColumn ActionName;
		private System.Data.DataColumn Key_Value;
		private System.Data.DataColumn Key_Code;
		private System.Data.DataColumn DestinationWindow;
		private System.Data.DataColumn cSharpCode;
		private System.Data.DataColumn keyDescription;
		private System.Windows.Forms.ComboBox comboBoxRemoteControl;
		private System.Windows.Forms.DataGrid dataGrid1;
		private System.Windows.Forms.DataGridTableStyle RemoteKeys;
		private System.Data.DataColumn dataColumn4;
		private System.Windows.Forms.DataGridTextBoxColumn dataGridKeyCode;
		private System.Windows.Forms.DataGridTextBoxColumn dataGridKeyDescription;
		private System.Windows.Forms.TextBox textBoxcSharp;
		private System.Windows.Forms.Label labelcSharp;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox textBoxCode;
		private System.Windows.Forms.TextBox textBoxDescription;
		private System.Windows.Forms.CheckBox checkBoxEnabled;
		private System.Windows.Forms.ContextMenu contextMenu1;
		private System.Windows.Forms.MenuItem menuAdd;
		private System.Windows.Forms.MenuItem menuDelete;
		private System.Windows.Forms.Button buttonLearn;
		private System.Windows.Forms.ComboBox comboBoxFireDTVDevice;
		private System.Windows.Forms.TextBox textBoxFireDTVKeyFile;
		private System.Windows.Forms.CheckBox checkBoxFireDTVEnabled;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.CheckBox checkBoxAdvanceMode;
		private System.Windows.Forms.PictureBox pictureBox1;
		#region RemoteControlKeys
		public enum FireDTVRemoteControlKeys
		{
			RemoteKey_Power				= 768,
			RemoteKey_Sleep				= 769,
			RemoteKey_Mute				= 789,
			RemoteKey_Record			= 791,
			RemoteKey_PreviousChapter	= 795,
			RemoteKey_StopEject			= 770,
			RemoteKey_NextChapter		= 798,
			RemoteKey_SubTitle			= 790,
			RemoteKey_Rewind			= 796,
			RemoteKey_PausePlay			= 797,
			RemoteKey_FastFoward		= 847,
			RemoteKey_List				= 848,
			RemoteKey_Favourites		= 849,
			RemoteKey_UpArrow			= 780,
			RemoteKey_Menu				= 850,
			RemoteKey_LeftArrow			= 776,
			RemoteKey_Select			= 771,
			RemoteKey_RightArrow		= 772,
			RemoteKey_EPG				= 851,
			RemoteKey_DownArrow			= 784,
			RemoteKey_Exit				= 852,
			RemoteKey_VolumeUp			= 799,
			RemoteKey_VolumeDown		= 843,
			RemoteKey_ChannelUp			= 832,
			RemoteKey_ChannelDown		= 844,
			RemoteKey_ChannelList		= 841,
			RemoteKey_Last				= 845,
			RemoteKey_Full				= 788,
			RemoteKey_Info				= 846,
			RemoteKey_1					= 773,
			RemoteKey_2					= 774,
			RemoteKey_3					= 775,
			RemoteKey_4					= 777,
			RemoteKey_5					= 778,
			RemoteKey_6					= 779,
			RemoteKey_7					= 781,
			RemoteKey_8					= 782,
			RemoteKey_9					= 783,
			RemoteKey_0					= 786,
			RemoteKey_Text				= 792,
			RemoteKey_Audio				= 793,
			RemoteKey_CI				= 842,
			RemoteKey_Display4_3		= 833,
			RemoteKey_Display16_9		= 787,
			RemoteKey_OnScreenDisplay	= 785,
			RemoteKey_TV				= 834,
			RemoteKey_DVD				= 835,
			RemoteKey_VCR				= 836,
			RemoteKey_AUX				= 837,
			RemoteKey_Red				= 794,
			RemoteKey_Green				= 838,
			RemoteKey_Yellow			= 839,
			RemoteKey_Blue				= 840
		}
		public enum FireDTVConditions
		{
			IsFullScreenVideo,
			IsPlaying,
			IsPlayingVideo,
			IsDVD,
			IsTV,
			IsTVRecording,
			IsTimeShifting
		}
		#endregion

		#region Private Variables and Methods
		private MadMouse.FireDTV.FireDTVControl FireDTV = new MadMouse.FireDTV.FireDTVControl(0);
		private void LoadDrivers()
		{
			try
			{
				FireDTV.OpenDrivers();
				comboBoxFireDTVDevice.DataSource	= FireDTV.SourceFilterCollection;
				comboBoxFireDTVDevice.DisplayMember = "DeviceFriendlyName";
				comboBoxFireDTVDevice.ValueMember	= "DisplayString";
			}
			catch(Exception)
			{
			}
		}
		#endregion
		#region LoadSettings
		public override void LoadSettings()
		{
			LoadDrivers();
			using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				comboBoxAction.DataSource = Enum.GetValues(typeof(Action.ActionType));;
				comboBoxAction.DisplayMember = "Name";

				comboBoxKeyCode.DataSource		= Enum.GetValues(typeof(System.Windows.Forms.Keys));
				comboBoxKeyCode.DisplayMember	= "Name";
				
				comboBoxGotoWindow.DataSource = Enum.GetValues(typeof(GUIWindow.Window));
				comboBoxGotoWindow.DisplayMember = "Name";

				checkBoxFireDTVEnabled.Checked	= xmlreader.GetValueAsBool("remote", "FireDTV", false);
				textBoxFireDTVKeyFile.Text		= xmlreader.GetValueAsString("remote", "FireDTVKeyFile", "FireDTVKeyMap.XML");
				comboBoxRemoteControl.Text		= xmlreader.GetValueAsString("remote", "FireDTVRemoteName", "FireDTV Remote Control"); 

				checkBoxAdvanceMode.Checked		= xmlreader.GetValueAsBool("remote", "FireDTVAdvanceMode", false);
				HandleAdvMode(this);

				string DeviceName = xmlreader.GetValueAsString("remote", "FireDTVDeviceName", string.Empty);
				if (DeviceName != string.Empty)
					comboBoxFireDTVDevice.SelectedValue = DeviceName; 
				
			}
			try
			{
				dataSet1.ReadXml(textBoxFireDTVKeyFile.Text);
			}
			catch (FileNotFoundException eNofile)
			{
				MessageBox.Show(textBoxFireDTVKeyFile.Text + " : " + eNofile.Message);
			}

		}
		#endregion
		#region SaveSettings
		public override void SaveSettings()
		{
			using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				xmlwriter.SetValueAsBool("remote", "FireDTV", checkBoxFireDTVEnabled.Checked);
				xmlwriter.SetValue("remote", "FireDTVKeyFile", textBoxFireDTVKeyFile.Text);
				xmlwriter.SetValue("remote", "FireDTVRemoteName", comboBoxRemoteControl.Text);
				xmlwriter.SetValue("remote", "FireDTVDeviceName", comboBoxFireDTVDevice.SelectedValue);
				xmlwriter.SetValueAsBool("remote", "FireDTVAdvanceMode", checkBoxAdvanceMode.Checked);
			}
			dataSet1.WriteXml(textBoxFireDTVKeyFile.Text);
			dataSet1.WriteXmlSchema(textBoxFireDTVKeyFile.Text + ".Schema");
		}
		#endregion
		#region Constructor and Destructor
		public FireDTVRemote() : this("FireDTV Remote")
		{
		}

		public FireDTVRemote(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

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
		#endregion
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(FireDTVRemote));
      this.panel1 = new System.Windows.Forms.Panel();
      this.dataGrid1 = new System.Windows.Forms.DataGrid();
      this.contextMenu1 = new System.Windows.Forms.ContextMenu();
      this.menuAdd = new System.Windows.Forms.MenuItem();
      this.menuDelete = new System.Windows.Forms.MenuItem();
      this.dataSet1 = new System.Data.DataSet();
      this.RemoteControl = new System.Data.DataTable();
      this.dataColumn1 = new System.Data.DataColumn();
      this.dataColumn2 = new System.Data.DataColumn();
      this.dataColumn3 = new System.Data.DataColumn();
      this.RemoteControlKeys = new System.Data.DataTable();
      this.RemoteID = new System.Data.DataColumn();
      this.KeyCode = new System.Data.DataColumn();
      this.ActionType = new System.Data.DataColumn();
      this.ActionName = new System.Data.DataColumn();
      this.Key_Value = new System.Data.DataColumn();
      this.Key_Code = new System.Data.DataColumn();
      this.DestinationWindow = new System.Data.DataColumn();
      this.cSharpCode = new System.Data.DataColumn();
      this.keyDescription = new System.Data.DataColumn();
      this.dataColumn4 = new System.Data.DataColumn();
      this.RemoteKeys = new System.Windows.Forms.DataGridTableStyle();
      this.dataGridKeyCode = new System.Windows.Forms.DataGridTextBoxColumn();
      this.dataGridKeyDescription = new System.Windows.Forms.DataGridTextBoxColumn();
      this.panel2 = new System.Windows.Forms.Panel();
      this.checkBoxAdvanceMode = new System.Windows.Forms.CheckBox();
      this.comboBoxFireDTVDevice = new System.Windows.Forms.ComboBox();
      this.comboBoxRemoteControl = new System.Windows.Forms.ComboBox();
      this.textBoxFireDTVKeyFile = new System.Windows.Forms.TextBox();
      this.checkBoxFireDTVEnabled = new System.Windows.Forms.CheckBox();
      this.splitter1 = new System.Windows.Forms.Splitter();
      this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
      this.panel3 = new System.Windows.Forms.Panel();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.checkBoxEnabled = new System.Windows.Forms.CheckBox();
      this.textBoxDescription = new System.Windows.Forms.TextBox();
      this.textBoxCode = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.labelcSharp = new System.Windows.Forms.Label();
      this.textBoxcSharp = new System.Windows.Forms.TextBox();
      this.buttonLearn = new System.Windows.Forms.Button();
      this.comboBoxGotoWindow = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.comboBoxKeyCode = new System.Windows.Forms.ComboBox();
      this.textBoxKey = new System.Windows.Forms.TextBox();
      this.labelKeyCode = new System.Windows.Forms.Label();
      this.labelKey = new System.Windows.Forms.Label();
      this.labelAction = new System.Windows.Forms.Label();
      this.comboBoxAction = new System.Windows.Forms.ComboBox();
      this.panel1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.dataSet1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.RemoteControl)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.RemoteControlKeys)).BeginInit();
      this.panel2.SuspendLayout();
      this.panel3.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.dataGrid1);
      this.panel1.Controls.Add(this.panel2);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
      this.panel1.Location = new System.Drawing.Point(0, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(216, 368);
      this.panel1.TabIndex = 0;
      // 
      // dataGrid1
      // 
      this.dataGrid1.AlternatingBackColor = System.Drawing.Color.Silver;
      this.dataGrid1.BackColor = System.Drawing.Color.White;
      this.dataGrid1.CaptionBackColor = System.Drawing.Color.Maroon;
      this.dataGrid1.CaptionFont = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.dataGrid1.CaptionForeColor = System.Drawing.Color.White;
      this.dataGrid1.CaptionText = "Remote Keys";
      this.dataGrid1.ContextMenu = this.contextMenu1;
      this.dataGrid1.DataMember = "RemoteControlKeys";
      this.dataGrid1.DataSource = this.dataSet1;
      this.dataGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dataGrid1.Font = new System.Drawing.Font("Tahoma", 8F);
      this.dataGrid1.ForeColor = System.Drawing.Color.Black;
      this.dataGrid1.GridLineColor = System.Drawing.Color.Silver;
      this.dataGrid1.HeaderBackColor = System.Drawing.Color.Silver;
      this.dataGrid1.HeaderFont = new System.Drawing.Font("Tahoma", 8F);
      this.dataGrid1.HeaderForeColor = System.Drawing.Color.Black;
      this.dataGrid1.LinkColor = System.Drawing.Color.Maroon;
      this.dataGrid1.Location = new System.Drawing.Point(0, 112);
      this.dataGrid1.Name = "dataGrid1";
      this.dataGrid1.ParentRowsBackColor = System.Drawing.Color.Silver;
      this.dataGrid1.ParentRowsForeColor = System.Drawing.Color.Black;
      this.dataGrid1.SelectionBackColor = System.Drawing.Color.Maroon;
      this.dataGrid1.SelectionForeColor = System.Drawing.Color.White;
      this.dataGrid1.Size = new System.Drawing.Size(216, 256);
      this.dataGrid1.TabIndex = 9;
      this.dataGrid1.TableStyles.AddRange(new System.Windows.Forms.DataGridTableStyle[] {
                                                                                          this.RemoteKeys});
      this.dataGrid1.Tag = "1";
      this.dataGrid1.CurrentCellChanged += new System.EventHandler(this.dataGrid1_CurrentCellChanged);
      // 
      // contextMenu1
      // 
      this.contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                 this.menuAdd,
                                                                                 this.menuDelete});
      // 
      // menuAdd
      // 
      this.menuAdd.Index = 0;
      this.menuAdd.Text = "Add New Key";
      // 
      // menuDelete
      // 
      this.menuDelete.Index = 1;
      this.menuDelete.Text = "Delete Key";
      this.menuDelete.Click += new System.EventHandler(this.menuDelete_Click);
      // 
      // dataSet1
      // 
      this.dataSet1.DataSetName = "NewDataSet";
      this.dataSet1.Locale = new System.Globalization.CultureInfo("en-GB");
      this.dataSet1.Relations.AddRange(new System.Data.DataRelation[] {
                                                                        new System.Data.DataRelation("Relation_RemoteID", "RemoteControl", "RemoteControlKeys", new string[] {
                                                                                                                                                                               "RemoteID"}, new string[] {
                                                                                                                                                                                                           "RemoteID"}, false)});
      this.dataSet1.Tables.AddRange(new System.Data.DataTable[] {
                                                                  this.RemoteControl,
                                                                  this.RemoteControlKeys});
      // 
      // RemoteControl
      // 
      this.RemoteControl.Columns.AddRange(new System.Data.DataColumn[] {
                                                                         this.dataColumn1,
                                                                         this.dataColumn2,
                                                                         this.dataColumn3});
      this.RemoteControl.Constraints.AddRange(new System.Data.Constraint[] {
                                                                             new System.Data.UniqueConstraint("Constraint1", new string[] {
                                                                                                                                            "RemoteID"}, false),
                                                                             new System.Data.UniqueConstraint("Constraint2", new string[] {
                                                                                                                                            "RemoteName"}, true)});
      this.RemoteControl.PrimaryKey = new System.Data.DataColumn[] {
                                                                     this.dataColumn2};
      this.RemoteControl.TableName = "RemoteControl";
      // 
      // dataColumn1
      // 
      this.dataColumn1.AllowDBNull = false;
      this.dataColumn1.AutoIncrement = true;
      this.dataColumn1.ColumnName = "RemoteID";
      this.dataColumn1.DataType = typeof(int);
      // 
      // dataColumn2
      // 
      this.dataColumn2.AllowDBNull = false;
      this.dataColumn2.ColumnName = "RemoteName";
      // 
      // dataColumn3
      // 
      this.dataColumn3.ColumnName = "RemoteActive";
      this.dataColumn3.DataType = typeof(bool);
      // 
      // RemoteControlKeys
      // 
      this.RemoteControlKeys.Columns.AddRange(new System.Data.DataColumn[] {
                                                                             this.RemoteID,
                                                                             this.KeyCode,
                                                                             this.ActionType,
                                                                             this.ActionName,
                                                                             this.Key_Value,
                                                                             this.Key_Code,
                                                                             this.DestinationWindow,
                                                                             this.cSharpCode,
                                                                             this.keyDescription,
                                                                             this.dataColumn4});
      this.RemoteControlKeys.Constraints.AddRange(new System.Data.Constraint[] {
                                                                                 new System.Data.UniqueConstraint("Constraint1", new string[] {
                                                                                                                                                "RemoteID",
                                                                                                                                                "KeyCode"}, true),
                                                                                 new System.Data.ForeignKeyConstraint("Relation1", "RemoteControl", new string[] {
                                                                                                                                                                   "RemoteID"}, new string[] {
                                                                                                                                                                                               "RemoteID"}, System.Data.AcceptRejectRule.None, System.Data.Rule.Cascade, System.Data.Rule.Cascade)});
      this.RemoteControlKeys.PrimaryKey = new System.Data.DataColumn[] {
                                                                         this.RemoteID,
                                                                         this.KeyCode};
      this.RemoteControlKeys.TableName = "RemoteControlKeys";
      // 
      // RemoteID
      // 
      this.RemoteID.AllowDBNull = false;
      this.RemoteID.ColumnName = "RemoteID";
      this.RemoteID.DataType = typeof(int);
      // 
      // KeyCode
      // 
      this.KeyCode.AllowDBNull = false;
      this.KeyCode.ColumnName = "KeyCode";
      this.KeyCode.DataType = typeof(int);
      // 
      // ActionType
      // 
      this.ActionType.ColumnName = "ActionType";
      // 
      // ActionName
      // 
      this.ActionName.ColumnName = "ActionName";
      // 
      // Key_Value
      // 
      this.Key_Value.ColumnName = "Key_Value";
      this.Key_Value.DataType = typeof(char);
      // 
      // Key_Code
      // 
      this.Key_Code.ColumnName = "Key_Code";
      // 
      // DestinationWindow
      // 
      this.DestinationWindow.ColumnName = "DestinationWindow";
      // 
      // cSharpCode
      // 
      this.cSharpCode.ColumnName = "cSharpCode";
      // 
      // keyDescription
      // 
      this.keyDescription.ColumnName = "keyDescription";
      // 
      // dataColumn4
      // 
      this.dataColumn4.ColumnName = "Enabled";
      this.dataColumn4.DataType = typeof(bool);
      // 
      // RemoteKeys
      // 
      this.RemoteKeys.DataGrid = this.dataGrid1;
      this.RemoteKeys.GridColumnStyles.AddRange(new System.Windows.Forms.DataGridColumnStyle[] {
                                                                                                 this.dataGridKeyCode,
                                                                                                 this.dataGridKeyDescription});
      this.RemoteKeys.HeaderForeColor = System.Drawing.SystemColors.ControlText;
      this.RemoteKeys.MappingName = "RemoteControlKeys";
      this.RemoteKeys.RowHeadersVisible = false;
      // 
      // dataGridKeyCode
      // 
      this.dataGridKeyCode.Format = "";
      this.dataGridKeyCode.FormatInfo = null;
      this.dataGridKeyCode.HeaderText = "Code";
      this.dataGridKeyCode.MappingName = "KeyCode";
      this.dataGridKeyCode.Width = 50;
      // 
      // dataGridKeyDescription
      // 
      this.dataGridKeyDescription.Format = "";
      this.dataGridKeyDescription.FormatInfo = null;
      this.dataGridKeyDescription.HeaderText = "Description";
      this.dataGridKeyDescription.MappingName = "keyDescription";
      this.dataGridKeyDescription.Width = 145;
      // 
      // panel2
      // 
      this.panel2.Controls.Add(this.checkBoxAdvanceMode);
      this.panel2.Controls.Add(this.comboBoxFireDTVDevice);
      this.panel2.Controls.Add(this.comboBoxRemoteControl);
      this.panel2.Controls.Add(this.textBoxFireDTVKeyFile);
      this.panel2.Controls.Add(this.checkBoxFireDTVEnabled);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel2.Location = new System.Drawing.Point(0, 0);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(216, 112);
      this.panel2.TabIndex = 2;
      // 
      // checkBoxAdvanceMode
      // 
      this.checkBoxAdvanceMode.Dock = System.Windows.Forms.DockStyle.Top;
      this.checkBoxAdvanceMode.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.checkBoxAdvanceMode.Location = new System.Drawing.Point(0, 86);
      this.checkBoxAdvanceMode.Name = "checkBoxAdvanceMode";
      this.checkBoxAdvanceMode.Size = new System.Drawing.Size(216, 19);
      this.checkBoxAdvanceMode.TabIndex = 10;
      this.checkBoxAdvanceMode.Text = "Enable Advance Mode (Beta)";
      this.checkBoxAdvanceMode.CheckedChanged += new System.EventHandler(this.checkBoxAdvanceMode_CheckedChanged);
      // 
      // comboBoxFireDTVDevice
      // 
      this.comboBoxFireDTVDevice.Dock = System.Windows.Forms.DockStyle.Top;
      this.comboBoxFireDTVDevice.Location = new System.Drawing.Point(0, 65);
      this.comboBoxFireDTVDevice.Name = "comboBoxFireDTVDevice";
      this.comboBoxFireDTVDevice.Size = new System.Drawing.Size(216, 21);
      this.comboBoxFireDTVDevice.TabIndex = 9;
      // 
      // comboBoxRemoteControl
      // 
      this.comboBoxRemoteControl.DataSource = this.RemoteControl;
      this.comboBoxRemoteControl.DisplayMember = "RemoteName";
      this.comboBoxRemoteControl.Dock = System.Windows.Forms.DockStyle.Top;
      this.comboBoxRemoteControl.Location = new System.Drawing.Point(0, 44);
      this.comboBoxRemoteControl.Name = "comboBoxRemoteControl";
      this.comboBoxRemoteControl.Size = new System.Drawing.Size(216, 21);
      this.comboBoxRemoteControl.TabIndex = 8;
      this.comboBoxRemoteControl.Tag = "1";
      this.comboBoxRemoteControl.ValueMember = "RemoteID";
      // 
      // textBoxFireDTVKeyFile
      // 
      this.textBoxFireDTVKeyFile.Dock = System.Windows.Forms.DockStyle.Top;
      this.textBoxFireDTVKeyFile.Location = new System.Drawing.Point(0, 24);
      this.textBoxFireDTVKeyFile.Name = "textBoxFireDTVKeyFile";
      this.textBoxFireDTVKeyFile.ReadOnly = true;
      this.textBoxFireDTVKeyFile.Size = new System.Drawing.Size(216, 20);
      this.textBoxFireDTVKeyFile.TabIndex = 7;
      this.textBoxFireDTVKeyFile.Tag = "1";
      this.textBoxFireDTVKeyFile.Text = "FireDTVKeyMap.XML";
      // 
      // checkBoxFireDTVEnabled
      // 
      this.checkBoxFireDTVEnabled.Dock = System.Windows.Forms.DockStyle.Top;
      this.checkBoxFireDTVEnabled.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.checkBoxFireDTVEnabled.Location = new System.Drawing.Point(0, 0);
      this.checkBoxFireDTVEnabled.Name = "checkBoxFireDTVEnabled";
      this.checkBoxFireDTVEnabled.Size = new System.Drawing.Size(216, 24);
      this.checkBoxFireDTVEnabled.TabIndex = 4;
      this.checkBoxFireDTVEnabled.Text = "Enable FireDTV Remote Control";
      // 
      // splitter1
      // 
      this.splitter1.Location = new System.Drawing.Point(216, 0);
      this.splitter1.Name = "splitter1";
      this.splitter1.Size = new System.Drawing.Size(3, 368);
      this.splitter1.TabIndex = 1;
      this.splitter1.TabStop = false;
      // 
      // openFileDialog1
      // 
      this.openFileDialog1.DefaultExt = "*.xml";
      this.openFileDialog1.Title = "FireDTV Key Mapping";
      // 
      // panel3
      // 
      this.panel3.Controls.Add(this.pictureBox1);
      this.panel3.Controls.Add(this.checkBoxEnabled);
      this.panel3.Controls.Add(this.textBoxDescription);
      this.panel3.Controls.Add(this.textBoxCode);
      this.panel3.Controls.Add(this.label3);
      this.panel3.Controls.Add(this.label2);
      this.panel3.Controls.Add(this.labelcSharp);
      this.panel3.Controls.Add(this.textBoxcSharp);
      this.panel3.Controls.Add(this.buttonLearn);
      this.panel3.Controls.Add(this.comboBoxGotoWindow);
      this.panel3.Controls.Add(this.label1);
      this.panel3.Controls.Add(this.comboBoxKeyCode);
      this.panel3.Controls.Add(this.textBoxKey);
      this.panel3.Controls.Add(this.labelKeyCode);
      this.panel3.Controls.Add(this.labelKey);
      this.panel3.Controls.Add(this.labelAction);
      this.panel3.Controls.Add(this.comboBoxAction);
      this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel3.Location = new System.Drawing.Point(219, 0);
      this.panel3.Name = "panel3";
      this.panel3.Size = new System.Drawing.Size(301, 368);
      this.panel3.TabIndex = 2;
      // 
      // pictureBox1
      // 
      this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
      this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
      this.pictureBox1.Location = new System.Drawing.Point(166, 160);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(68, 200);
      this.pictureBox1.TabIndex = 18;
      this.pictureBox1.TabStop = false;
      this.pictureBox1.Tag = "0";
      // 
      // checkBoxEnabled
      // 
      this.checkBoxEnabled.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.dataSet1, "RemoteControlKeys.Enabled"));
      this.checkBoxEnabled.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.checkBoxEnabled.Location = new System.Drawing.Point(86, 4);
      this.checkBoxEnabled.Name = "checkBoxEnabled";
      this.checkBoxEnabled.Size = new System.Drawing.Size(104, 20);
      this.checkBoxEnabled.TabIndex = 17;
      this.checkBoxEnabled.Tag = "1";
      this.checkBoxEnabled.Text = "Enabled";
      // 
      // textBoxDescription
      // 
      this.textBoxDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxDescription.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.dataSet1, "RemoteControlKeys.keyDescription"));
      this.textBoxDescription.Location = new System.Drawing.Point(62, 46);
      this.textBoxDescription.Name = "textBoxDescription";
      this.textBoxDescription.ReadOnly = true;
      this.textBoxDescription.Size = new System.Drawing.Size(182, 20);
      this.textBoxDescription.TabIndex = 16;
      this.textBoxDescription.Tag = "1";
      this.textBoxDescription.Text = "";
      // 
      // textBoxCode
      // 
      this.textBoxCode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxCode.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.dataSet1, "RemoteControlKeys.KeyCode"));
      this.textBoxCode.Location = new System.Drawing.Point(62, 26);
      this.textBoxCode.Name = "textBoxCode";
      this.textBoxCode.ReadOnly = true;
      this.textBoxCode.Size = new System.Drawing.Size(124, 20);
      this.textBoxCode.TabIndex = 15;
      this.textBoxCode.Tag = "1";
      this.textBoxCode.Text = "";
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(-10, 48);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(70, 16);
      this.label3.TabIndex = 14;
      this.label3.Tag = "1";
      this.label3.Text = "Description:";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(-10, 28);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(70, 16);
      this.label2.TabIndex = 13;
      this.label2.Tag = "1";
      this.label2.Text = "Code:";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // labelcSharp
      // 
      this.labelcSharp.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.labelcSharp.Location = new System.Drawing.Point(0, 156);
      this.labelcSharp.Name = "labelcSharp";
      this.labelcSharp.Size = new System.Drawing.Size(301, 14);
      this.labelcSharp.TabIndex = 12;
      this.labelcSharp.Text = "cSharp Code";
      this.labelcSharp.Visible = false;
      // 
      // textBoxcSharp
      // 
      this.textBoxcSharp.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.dataSet1, "RemoteControlKeys.cSharpCode"));
      this.textBoxcSharp.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.textBoxcSharp.Location = new System.Drawing.Point(0, 170);
      this.textBoxcSharp.Multiline = true;
      this.textBoxcSharp.Name = "textBoxcSharp";
      this.textBoxcSharp.Size = new System.Drawing.Size(301, 198);
      this.textBoxcSharp.TabIndex = 11;
      this.textBoxcSharp.Text = "";
      this.textBoxcSharp.Visible = false;
      // 
      // buttonLearn
      // 
      this.buttonLearn.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.buttonLearn.Location = new System.Drawing.Point(4, 2);
      this.buttonLearn.Name = "buttonLearn";
      this.buttonLearn.TabIndex = 10;
      this.buttonLearn.Tag = "0";
      this.buttonLearn.Text = "Learn";
      this.buttonLearn.Visible = false;
      this.buttonLearn.Click += new System.EventHandler(this.buttonLoad_Click);
      // 
      // comboBoxGotoWindow
      // 
      this.comboBoxGotoWindow.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxGotoWindow.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.dataSet1, "RemoteControlKeys.DestinationWindow"));
      this.comboBoxGotoWindow.Location = new System.Drawing.Point(62, 131);
      this.comboBoxGotoWindow.MaxDropDownItems = 20;
      this.comboBoxGotoWindow.Name = "comboBoxGotoWindow";
      this.comboBoxGotoWindow.Size = new System.Drawing.Size(182, 21);
      this.comboBoxGotoWindow.TabIndex = 7;
      this.comboBoxGotoWindow.Tag = "1";
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(-10, 132);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(70, 16);
      this.label1.TabIndex = 6;
      this.label1.Tag = "1";
      this.label1.Text = "Window:";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // comboBoxKeyCode
      // 
      this.comboBoxKeyCode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxKeyCode.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.dataSet1, "RemoteControlKeys.Key_Code"));
      this.comboBoxKeyCode.Location = new System.Drawing.Point(62, 108);
      this.comboBoxKeyCode.MaxDropDownItems = 20;
      this.comboBoxKeyCode.Name = "comboBoxKeyCode";
      this.comboBoxKeyCode.Size = new System.Drawing.Size(182, 21);
      this.comboBoxKeyCode.TabIndex = 5;
      this.comboBoxKeyCode.Tag = "1";
      // 
      // textBoxKey
      // 
      this.textBoxKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxKey.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.dataSet1, "RemoteControlKeys.Key_Value"));
      this.textBoxKey.Location = new System.Drawing.Point(62, 88);
      this.textBoxKey.MaxLength = 1;
      this.textBoxKey.Name = "textBoxKey";
      this.textBoxKey.Size = new System.Drawing.Size(20, 20);
      this.textBoxKey.TabIndex = 4;
      this.textBoxKey.Tag = "1";
      this.textBoxKey.Text = "";
      // 
      // labelKeyCode
      // 
      this.labelKeyCode.Location = new System.Drawing.Point(-10, 110);
      this.labelKeyCode.Name = "labelKeyCode";
      this.labelKeyCode.Size = new System.Drawing.Size(70, 16);
      this.labelKeyCode.TabIndex = 3;
      this.labelKeyCode.Tag = "1";
      this.labelKeyCode.Text = "Key Code:";
      this.labelKeyCode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.labelKeyCode.Click += new System.EventHandler(this.labelKeyCode_Click);
      // 
      // labelKey
      // 
      this.labelKey.Location = new System.Drawing.Point(-10, 90);
      this.labelKey.Name = "labelKey";
      this.labelKey.Size = new System.Drawing.Size(70, 16);
      this.labelKey.TabIndex = 2;
      this.labelKey.Tag = "1";
      this.labelKey.Text = "Key:";
      this.labelKey.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // labelAction
      // 
      this.labelAction.Location = new System.Drawing.Point(-10, 68);
      this.labelAction.Name = "labelAction";
      this.labelAction.Size = new System.Drawing.Size(70, 16);
      this.labelAction.TabIndex = 1;
      this.labelAction.Tag = "1";
      this.labelAction.Text = "Action:";
      this.labelAction.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // comboBoxAction
      // 
      this.comboBoxAction.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxAction.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.dataSet1, "RemoteControlKeys.ActionName"));
      this.comboBoxAction.Location = new System.Drawing.Point(62, 66);
      this.comboBoxAction.MaxDropDownItems = 20;
      this.comboBoxAction.Name = "comboBoxAction";
      this.comboBoxAction.Size = new System.Drawing.Size(182, 21);
      this.comboBoxAction.TabIndex = 0;
      this.comboBoxAction.Tag = "1";
      // 
      // FireDTVRemote
      // 
      this.Controls.Add(this.panel3);
      this.Controls.Add(this.splitter1);
      this.Controls.Add(this.panel1);
      this.Name = "FireDTVRemote";
      this.Size = new System.Drawing.Size(520, 368);
      this.panel1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.dataSet1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.RemoteControl)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.RemoteControlKeys)).EndInit();
      this.panel2.ResumeLayout(false);
      this.panel3.ResumeLayout(false);
      this.ResumeLayout(false);

    }
		#endregion

		private void openDialogButton_Click(object sender, System.EventArgs e)
		{
			openFileDialog1.InitialDirectory = System.Windows.Forms.Application.StartupPath;
			openFileDialog1.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*" ;
			openFileDialog1.FilterIndex = 1 ;
			openFileDialog1.RestoreDirectory = true ;

			if (openFileDialog1.ShowDialog() == DialogResult.OK)
				textBoxFireDTVKeyFile.Text = openFileDialog1.FileName;
		}

		private void comboBox1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			MessageBox.Show(comboBoxAction.SelectedValue.ToString());
		}

		private void labelKeyCode_Click(object sender, System.EventArgs e)
		{
		
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			
		}

		private void buttonLoad_Click(object sender, System.EventArgs e)
		{
			
		}

		private void dataGrid1_CurrentCellChanged(object sender, System.EventArgs e)
		{
		
		}

		private void menuDelete_Click(object sender, System.EventArgs e)
		{
			
		}

		private void HandleAdvMode(System.Windows.Forms.Control baseControl)
		{
			foreach(System.Windows.Forms.Control ctrl in baseControl.Controls)
			{
				if (ctrl != null)
				{
					if (ctrl.HasChildren)
						HandleAdvMode(ctrl);

					if ((ctrl.Tag != null) && (ctrl.Tag.ToString() == "1"))
						ctrl.Visible = checkBoxAdvanceMode.Checked;
				}
			}
		}
		private void checkBoxAdvanceMode_CheckedChanged(object sender, System.EventArgs e)
		{
			HandleAdvMode(this);		

		}
	}
}
