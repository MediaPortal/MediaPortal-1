using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Globalization;
using Microsoft.Win32;

using DirectX.Capture;
using MediaPortal.TV.Database;
using MediaPortal.WinControls;
namespace MediaPortal
{
	/// <summary>
	/// Summary description for SetupForm.
	/// </summary>
	public class SetupForm : System.Windows.Forms.Form
	{
    private System.Windows.Forms.TabPage tabPlayers;
    private System.Windows.Forms.GroupBox MoviePlayerBox;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TextBox movieFile;
    private System.Windows.Forms.Label Parameters;
    private System.Windows.Forms.TextBox movieParameters;
    private System.Windows.Forms.Button bntSelectMovieFile;
    private System.Windows.Forms.GroupBox audioGroupBox;
    private System.Windows.Forms.ListView listAudioShares;
    private System.Windows.Forms.ColumnHeader HdrAudioFolder;
    private System.Windows.Forms.ColumnHeader HdrAudioName;
    private System.Windows.Forms.TabPage tabAudioShares;
    private System.Windows.Forms.Button btnAddAudioShare;
    private System.Windows.Forms.Button btnDelAudioShare;

    private System.Windows.Forms.GroupBox VideoGroupBox;
    private System.Windows.Forms.ListView listVideoShares;
    private System.Windows.Forms.ColumnHeader HdrVideoName;
    private System.Windows.Forms.ColumnHeader HdrVideoFolder;
    private System.Windows.Forms.TabPage tabVideoShares;
    private System.Windows.Forms.Button btnAddVideoShare;
    private System.Windows.Forms.Button btnDelVideoShare;

    private System.Windows.Forms.GroupBox PictureGroupBox;
    private System.Windows.Forms.ListView listPictureShares;
    private System.Windows.Forms.ColumnHeader HdrPictureFolder;
    private System.Windows.Forms.ColumnHeader HdrPictureName;
    private System.Windows.Forms.TabPage tabPictureShares;
    private System.Windows.Forms.Button btnAddPictureShare;
    private System.Windows.Forms.Button btnDelPictureShare;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.NumericUpDown UpDownPictureDuration;
    private System.Windows.Forms.NumericUpDown UpDownPictureTransition;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.CheckBox chkMusicID3;
    private System.Windows.Forms.TabPage tabWeather;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.TextBox txtboxAudioFiles;
    private System.Windows.Forms.GroupBox groupBox3;
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.TextBox txtboxVideoFiles;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.TextBox txtBoxPictureFiles;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.ListView listViewWeather;
		private System.Windows.Forms.ColumnHeader WeatherHeader1;
		private System.Windows.Forms.ColumnHeader WeatherHeader2;
		private System.Windows.Forms.Button btnWeatherAddCity;
		private System.Windows.Forms.Button btnWeatherDel;
		private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.RadioButton radioCelsius;
		private System.Windows.Forms.RadioButton radioFarenHeit;
		private System.Windows.Forms.GroupBox groupBox6;
		private System.Windows.Forms.RadioButton radioWindSpeedMS;
		private System.Windows.Forms.RadioButton radioWindSpeedKH;
		private System.Windows.Forms.RadioButton radioWindSpeedMPH;
		private System.Windows.Forms.GroupBox groupBox7;
		private System.Windows.Forms.NumericUpDown cntrlweatherRefresh;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.TabPage tabGeneral;
		private System.Windows.Forms.CheckBox checkStartFullScreen;
		private System.Windows.Forms.ComboBox comboBoxLanguage;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.ComboBox comboBoxSkins;
    private System.Windows.Forms.Button btnEditMusicShare;
    private System.Windows.Forms.Button btnEditPictureShare;
    private System.Windows.Forms.Button btnEditMovieShare;
    private System.Windows.Forms.CheckBox chkBoxRepeatAudioPlaylist;
    private System.Windows.Forms.CheckBox chkBoxVideoRepeat;
    private System.Windows.Forms.CheckBox checkBoxMovieInternalPlayer;
    private System.Windows.Forms.GroupBox Skin;
    private System.Windows.Forms.GroupBox groupBox8;
    private System.Windows.Forms.CheckBox checkBoxShowSubtitles;
    private System.Windows.Forms.Label label14;
    private System.Windows.Forms.Label label16;
    private System.Windows.Forms.NumericUpDown numericUpDownSubShadow;
    private System.Windows.Forms.Button btnChooseSubFont;
    private System.Windows.Forms.TextBox txtBoxSubFont;
		private System.Windows.Forms.CheckBox checkBoxShufflePlaylists;
		private System.Windows.Forms.GroupBox groupBox9;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.TrackBar trackBarOSDTimeout;
		private System.Windows.Forms.Label labelOSDTimeout;
    private System.Windows.Forms.TabPage tabPageCapture;
    private System.Windows.Forms.GroupBox groupBox10;
    private System.Windows.Forms.Label label17;
    private System.Windows.Forms.ComboBox comboVideoDevice;
    private System.Windows.Forms.ComboBox comboAudioDevice;
    private System.Windows.Forms.Label label18;
    private System.Windows.Forms.Label label19;
    private System.Windows.Forms.ComboBox comboCompressorAudio;
    private System.Windows.Forms.GroupBox groupBox12;
    private System.Windows.Forms.Label label20;
    private System.Windows.Forms.ComboBox comboCompressorVideo;
    private ListViewEx listTVChannels;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private System.Windows.Forms.Button btnRecPath;
    private System.Windows.Forms.TextBox textBoxRecPath;
    private System.Windows.Forms.GroupBox groupBox11;
    private System.Windows.Forms.Label label22;
    private System.Windows.Forms.ComboBox comboBoxCaptureFormat;
    private System.Windows.Forms.GroupBox groupBox13;
    private System.Windows.Forms.RadioButton btnradioAntenna;
    private System.Windows.Forms.RadioButton btnradioCable;
    private System.Windows.Forms.CheckBox checkBoxAutoHideMouse;
    private System.Windows.Forms.Label label24;
    private System.Windows.Forms.NumericUpDown upDownCountry;
    private System.Windows.Forms.Label label25;
    private System.Windows.Forms.LinkLabel linkLabel1;
    private System.Windows.Forms.Label label26;
    private System.Windows.Forms.LinkLabel linkLabel2;
    private System.Windows.Forms.Label label27;
    private System.Windows.Forms.GroupBox groupBox14;
    private System.Windows.Forms.TabControl tabControl;
    private System.Windows.Forms.TabPage TabDVDPlayer;
    private System.Windows.Forms.GroupBox DVDPlayerBox;
    private System.Windows.Forms.CheckBox checkBoxInternalDVDPlayer;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox dvdFile;
    private System.Windows.Forms.Button dvdbtnSelect;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox dvdParams;
    private System.Windows.Forms.Label label28;
    private System.Windows.Forms.Label label29;
    private System.Windows.Forms.ComboBox comboBoxAudioLanguage;
    private System.Windows.Forms.ComboBox comboBoxSubtitleLanguage;
    private System.Windows.Forms.CheckBox checkBoxDVDSubtitles;
    private System.Windows.Forms.ColumnHeader columnHeader3;
    private System.Windows.Forms.Button btnTvChannelUp;
    private System.Windows.Forms.Button btnTvChannelDown;
		private System.Windows.Forms.GroupBox groupBox15;
		private System.Windows.Forms.Label label30;
		private System.Windows.Forms.ComboBox comboAudioPlayer;
		private System.Windows.Forms.TabPage tabAudioPlayer;
    private System.Windows.Forms.Button btnNewChannel;
    private System.Windows.Forms.Button btnDelChannel;
    private System.Windows.Forms.Button btnEditChannel;
    private System.Windows.Forms.TextBox textEditBox;
    private System.Windows.Forms.Label label23;
    private System.Windows.Forms.TabPage tabTVChannels;
    private System.Windows.Forms.GroupBox groupBox16;
    private System.Windows.Forms.ListView listPropertyPages;
    private System.Windows.Forms.ColumnHeader columnHeader4;
 
    /// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public SetupForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			LoadSettings();
      SetupCapture();
			listTVChannels.SubItemClicked += new ListViewEx.SubItemClickEventHandler(listTVChannels_SubItemClicked);
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
      this.tabControl = new System.Windows.Forms.TabControl();
      this.tabGeneral = new System.Windows.Forms.TabPage();
      this.label27 = new System.Windows.Forms.Label();
      this.linkLabel2 = new System.Windows.Forms.LinkLabel();
      this.label26 = new System.Windows.Forms.Label();
      this.linkLabel1 = new System.Windows.Forms.LinkLabel();
      this.label25 = new System.Windows.Forms.Label();
      this.checkBoxAutoHideMouse = new System.Windows.Forms.CheckBox();
      this.comboBoxSkins = new System.Windows.Forms.ComboBox();
      this.label13 = new System.Windows.Forms.Label();
      this.label12 = new System.Windows.Forms.Label();
      this.comboBoxLanguage = new System.Windows.Forms.ComboBox();
      this.checkStartFullScreen = new System.Windows.Forms.CheckBox();
      this.Skin = new System.Windows.Forms.GroupBox();
      this.groupBox14 = new System.Windows.Forms.GroupBox();
      this.tabAudioPlayer = new System.Windows.Forms.TabPage();
      this.groupBox15 = new System.Windows.Forms.GroupBox();
      this.label30 = new System.Windows.Forms.Label();
      this.comboAudioPlayer = new System.Windows.Forms.ComboBox();
      this.tabPlayers = new System.Windows.Forms.TabPage();
      this.groupBox9 = new System.Windows.Forms.GroupBox();
      this.labelOSDTimeout = new System.Windows.Forms.Label();
      this.label15 = new System.Windows.Forms.Label();
      this.trackBarOSDTimeout = new System.Windows.Forms.TrackBar();
      this.groupBox8 = new System.Windows.Forms.GroupBox();
      this.btnChooseSubFont = new System.Windows.Forms.Button();
      this.txtBoxSubFont = new System.Windows.Forms.TextBox();
      this.numericUpDownSubShadow = new System.Windows.Forms.NumericUpDown();
      this.label16 = new System.Windows.Forms.Label();
      this.label14 = new System.Windows.Forms.Label();
      this.checkBoxShowSubtitles = new System.Windows.Forms.CheckBox();
      this.MoviePlayerBox = new System.Windows.Forms.GroupBox();
      this.bntSelectMovieFile = new System.Windows.Forms.Button();
      this.movieParameters = new System.Windows.Forms.TextBox();
      this.Parameters = new System.Windows.Forms.Label();
      this.movieFile = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.checkBoxMovieInternalPlayer = new System.Windows.Forms.CheckBox();
      this.TabDVDPlayer = new System.Windows.Forms.TabPage();
      this.DVDPlayerBox = new System.Windows.Forms.GroupBox();
      this.checkBoxDVDSubtitles = new System.Windows.Forms.CheckBox();
      this.comboBoxSubtitleLanguage = new System.Windows.Forms.ComboBox();
      this.comboBoxAudioLanguage = new System.Windows.Forms.ComboBox();
      this.label29 = new System.Windows.Forms.Label();
      this.label28 = new System.Windows.Forms.Label();
      this.checkBoxInternalDVDPlayer = new System.Windows.Forms.CheckBox();
      this.label1 = new System.Windows.Forms.Label();
      this.dvdFile = new System.Windows.Forms.TextBox();
      this.dvdbtnSelect = new System.Windows.Forms.Button();
      this.label2 = new System.Windows.Forms.Label();
      this.dvdParams = new System.Windows.Forms.TextBox();
      this.tabAudioShares = new System.Windows.Forms.TabPage();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.checkBoxShufflePlaylists = new System.Windows.Forms.CheckBox();
      this.chkBoxRepeatAudioPlaylist = new System.Windows.Forms.CheckBox();
      this.txtboxAudioFiles = new System.Windows.Forms.TextBox();
      this.label8 = new System.Windows.Forms.Label();
      this.chkMusicID3 = new System.Windows.Forms.CheckBox();
      this.audioGroupBox = new System.Windows.Forms.GroupBox();
      this.btnEditMusicShare = new System.Windows.Forms.Button();
      this.btnDelAudioShare = new System.Windows.Forms.Button();
      this.btnAddAudioShare = new System.Windows.Forms.Button();
      this.listAudioShares = new System.Windows.Forms.ListView();
      this.HdrAudioName = new System.Windows.Forms.ColumnHeader();
      this.HdrAudioFolder = new System.Windows.Forms.ColumnHeader();
      this.tabVideoShares = new System.Windows.Forms.TabPage();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.chkBoxVideoRepeat = new System.Windows.Forms.CheckBox();
      this.txtboxVideoFiles = new System.Windows.Forms.TextBox();
      this.label9 = new System.Windows.Forms.Label();
      this.VideoGroupBox = new System.Windows.Forms.GroupBox();
      this.btnEditMovieShare = new System.Windows.Forms.Button();
      this.btnDelVideoShare = new System.Windows.Forms.Button();
      this.btnAddVideoShare = new System.Windows.Forms.Button();
      this.listVideoShares = new System.Windows.Forms.ListView();
      this.HdrVideoName = new System.Windows.Forms.ColumnHeader();
      this.HdrVideoFolder = new System.Windows.Forms.ColumnHeader();
      this.tabPictureShares = new System.Windows.Forms.TabPage();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.txtBoxPictureFiles = new System.Windows.Forms.TextBox();
      this.label10 = new System.Windows.Forms.Label();
      this.label7 = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      this.UpDownPictureTransition = new System.Windows.Forms.NumericUpDown();
      this.label5 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.UpDownPictureDuration = new System.Windows.Forms.NumericUpDown();
      this.PictureGroupBox = new System.Windows.Forms.GroupBox();
      this.btnEditPictureShare = new System.Windows.Forms.Button();
      this.btnDelPictureShare = new System.Windows.Forms.Button();
      this.btnAddPictureShare = new System.Windows.Forms.Button();
      this.listPictureShares = new System.Windows.Forms.ListView();
      this.HdrPictureName = new System.Windows.Forms.ColumnHeader();
      this.HdrPictureFolder = new System.Windows.Forms.ColumnHeader();
      this.tabWeather = new System.Windows.Forms.TabPage();
      this.groupBox7 = new System.Windows.Forms.GroupBox();
      this.label11 = new System.Windows.Forms.Label();
      this.cntrlweatherRefresh = new System.Windows.Forms.NumericUpDown();
      this.groupBox6 = new System.Windows.Forms.GroupBox();
      this.radioWindSpeedMPH = new System.Windows.Forms.RadioButton();
      this.radioWindSpeedKH = new System.Windows.Forms.RadioButton();
      this.radioWindSpeedMS = new System.Windows.Forms.RadioButton();
      this.groupBox5 = new System.Windows.Forms.GroupBox();
      this.radioFarenHeit = new System.Windows.Forms.RadioButton();
      this.radioCelsius = new System.Windows.Forms.RadioButton();
      this.groupBox4 = new System.Windows.Forms.GroupBox();
      this.btnWeatherDel = new System.Windows.Forms.Button();
      this.btnWeatherAddCity = new System.Windows.Forms.Button();
      this.listViewWeather = new System.Windows.Forms.ListView();
      this.WeatherHeader1 = new System.Windows.Forms.ColumnHeader();
      this.WeatherHeader2 = new System.Windows.Forms.ColumnHeader();
      this.tabPageCapture = new System.Windows.Forms.TabPage();
      this.listPropertyPages = new System.Windows.Forms.ListView();
      this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
      this.groupBox13 = new System.Windows.Forms.GroupBox();
      this.upDownCountry = new System.Windows.Forms.NumericUpDown();
      this.label24 = new System.Windows.Forms.Label();
      this.label23 = new System.Windows.Forms.Label();
      this.btnradioCable = new System.Windows.Forms.RadioButton();
      this.btnradioAntenna = new System.Windows.Forms.RadioButton();
      this.groupBox11 = new System.Windows.Forms.GroupBox();
      this.label22 = new System.Windows.Forms.Label();
      this.comboBoxCaptureFormat = new System.Windows.Forms.ComboBox();
      this.btnRecPath = new System.Windows.Forms.Button();
      this.textBoxRecPath = new System.Windows.Forms.TextBox();
      this.groupBox12 = new System.Windows.Forms.GroupBox();
      this.comboCompressorVideo = new System.Windows.Forms.ComboBox();
      this.label20 = new System.Windows.Forms.Label();
      this.label19 = new System.Windows.Forms.Label();
      this.comboCompressorAudio = new System.Windows.Forms.ComboBox();
      this.groupBox10 = new System.Windows.Forms.GroupBox();
      this.label18 = new System.Windows.Forms.Label();
      this.label17 = new System.Windows.Forms.Label();
      this.comboAudioDevice = new System.Windows.Forms.ComboBox();
      this.comboVideoDevice = new System.Windows.Forms.ComboBox();
      this.groupBox16 = new System.Windows.Forms.GroupBox();
      this.tabTVChannels = new System.Windows.Forms.TabPage();
      this.textEditBox = new System.Windows.Forms.TextBox();
      this.btnEditChannel = new System.Windows.Forms.Button();
      this.btnDelChannel = new System.Windows.Forms.Button();
      this.btnNewChannel = new System.Windows.Forms.Button();
      this.btnTvChannelDown = new System.Windows.Forms.Button();
      this.btnTvChannelUp = new System.Windows.Forms.Button();
      this.listTVChannels = new MediaPortal.WinControls.ListViewEx();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
      this.tabControl.SuspendLayout();
      this.tabGeneral.SuspendLayout();
      this.tabAudioPlayer.SuspendLayout();
      this.groupBox15.SuspendLayout();
      this.tabPlayers.SuspendLayout();
      this.groupBox9.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarOSDTimeout)).BeginInit();
      this.groupBox8.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSubShadow)).BeginInit();
      this.MoviePlayerBox.SuspendLayout();
      this.TabDVDPlayer.SuspendLayout();
      this.DVDPlayerBox.SuspendLayout();
      this.tabAudioShares.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.audioGroupBox.SuspendLayout();
      this.tabVideoShares.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.VideoGroupBox.SuspendLayout();
      this.tabPictureShares.SuspendLayout();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.UpDownPictureTransition)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.UpDownPictureDuration)).BeginInit();
      this.PictureGroupBox.SuspendLayout();
      this.tabWeather.SuspendLayout();
      this.groupBox7.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.cntrlweatherRefresh)).BeginInit();
      this.groupBox6.SuspendLayout();
      this.groupBox5.SuspendLayout();
      this.groupBox4.SuspendLayout();
      this.tabPageCapture.SuspendLayout();
      this.groupBox13.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.upDownCountry)).BeginInit();
      this.groupBox11.SuspendLayout();
      this.groupBox12.SuspendLayout();
      this.groupBox10.SuspendLayout();
      this.tabTVChannels.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabControl
      // 
      this.tabControl.Controls.Add(this.tabGeneral);
      this.tabControl.Controls.Add(this.tabAudioPlayer);
      this.tabControl.Controls.Add(this.tabPlayers);
      this.tabControl.Controls.Add(this.TabDVDPlayer);
      this.tabControl.Controls.Add(this.tabAudioShares);
      this.tabControl.Controls.Add(this.tabVideoShares);
      this.tabControl.Controls.Add(this.tabPictureShares);
      this.tabControl.Controls.Add(this.tabWeather);
      this.tabControl.Controls.Add(this.tabPageCapture);
      this.tabControl.Controls.Add(this.tabTVChannels);
      this.tabControl.Location = new System.Drawing.Point(0, 0);
      this.tabControl.Name = "tabControl";
      this.tabControl.SelectedIndex = 0;
      this.tabControl.Size = new System.Drawing.Size(624, 400);
      this.tabControl.TabIndex = 0;
      // 
      // tabGeneral
      // 
      this.tabGeneral.Controls.Add(this.label27);
      this.tabGeneral.Controls.Add(this.linkLabel2);
      this.tabGeneral.Controls.Add(this.label26);
      this.tabGeneral.Controls.Add(this.linkLabel1);
      this.tabGeneral.Controls.Add(this.label25);
      this.tabGeneral.Controls.Add(this.checkBoxAutoHideMouse);
      this.tabGeneral.Controls.Add(this.comboBoxSkins);
      this.tabGeneral.Controls.Add(this.label13);
      this.tabGeneral.Controls.Add(this.label12);
      this.tabGeneral.Controls.Add(this.comboBoxLanguage);
      this.tabGeneral.Controls.Add(this.checkStartFullScreen);
      this.tabGeneral.Controls.Add(this.Skin);
      this.tabGeneral.Controls.Add(this.groupBox14);
      this.tabGeneral.Location = new System.Drawing.Point(4, 22);
      this.tabGeneral.Name = "tabGeneral";
      this.tabGeneral.Size = new System.Drawing.Size(616, 374);
      this.tabGeneral.TabIndex = 5;
      this.tabGeneral.Text = "General";
      // 
      // label27
      // 
      this.label27.Location = new System.Drawing.Point(24, 248);
      this.label27.Name = "label27";
      this.label27.Size = new System.Drawing.Size(208, 16);
      this.label27.TabIndex = 12;
      this.label27.Text = "IRC: EFNet #MediaPortal";
      // 
      // linkLabel2
      // 
      this.linkLabel2.Location = new System.Drawing.Point(104, 216);
      this.linkLabel2.Name = "linkLabel2";
      this.linkLabel2.Size = new System.Drawing.Size(208, 16);
      this.linkLabel2.TabIndex = 11;
      this.linkLabel2.TabStop = true;
      this.linkLabel2.Text = "http://dott.lir.dk/mediaportal/forum";
      // 
      // label26
      // 
      this.label26.Location = new System.Drawing.Point(24, 216);
      this.label26.Name = "label26";
      this.label26.Size = new System.Drawing.Size(72, 16);
      this.label26.TabIndex = 10;
      this.label26.Text = "Forums:";
      // 
      // linkLabel1
      // 
      this.linkLabel1.Location = new System.Drawing.Point(104, 192);
      this.linkLabel1.Name = "linkLabel1";
      this.linkLabel1.Size = new System.Drawing.Size(224, 16);
      this.linkLabel1.TabIndex = 9;
      this.linkLabel1.TabStop = true;
      this.linkLabel1.Text = "http://sourceforge.net/projects/MediaPortal";
      // 
      // label25
      // 
      this.label25.Location = new System.Drawing.Point(24, 192);
      this.label25.Name = "label25";
      this.label25.Size = new System.Drawing.Size(72, 16);
      this.label25.TabIndex = 8;
      this.label25.Text = "Sourceforge:";
      // 
      // checkBoxAutoHideMouse
      // 
      this.checkBoxAutoHideMouse.Checked = true;
      this.checkBoxAutoHideMouse.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxAutoHideMouse.Location = new System.Drawing.Point(32, 48);
      this.checkBoxAutoHideMouse.Name = "checkBoxAutoHideMouse";
      this.checkBoxAutoHideMouse.Size = new System.Drawing.Size(112, 16);
      this.checkBoxAutoHideMouse.TabIndex = 7;
      this.checkBoxAutoHideMouse.Text = "Auto hide mouse";
      // 
      // comboBoxSkins
      // 
      this.comboBoxSkins.Location = new System.Drawing.Point(96, 104);
      this.comboBoxSkins.Name = "comboBoxSkins";
      this.comboBoxSkins.Size = new System.Drawing.Size(121, 21);
      this.comboBoxSkins.TabIndex = 4;
      // 
      // label13
      // 
      this.label13.Location = new System.Drawing.Point(32, 112);
      this.label13.Name = "label13";
      this.label13.Size = new System.Drawing.Size(64, 23);
      this.label13.TabIndex = 3;
      this.label13.Text = "Skin:";
      // 
      // label12
      // 
      this.label12.Location = new System.Drawing.Point(32, 80);
      this.label12.Name = "label12";
      this.label12.Size = new System.Drawing.Size(64, 23);
      this.label12.TabIndex = 2;
      this.label12.Text = "Language:";
      // 
      // comboBoxLanguage
      // 
      this.comboBoxLanguage.Location = new System.Drawing.Point(96, 72);
      this.comboBoxLanguage.Name = "comboBoxLanguage";
      this.comboBoxLanguage.Size = new System.Drawing.Size(121, 21);
      this.comboBoxLanguage.TabIndex = 1;
      // 
      // checkStartFullScreen
      // 
      this.checkStartFullScreen.Location = new System.Drawing.Point(32, 24);
      this.checkStartFullScreen.Name = "checkStartFullScreen";
      this.checkStartFullScreen.TabIndex = 0;
      this.checkStartFullScreen.Text = "Start fullscreen";
      // 
      // Skin
      // 
      this.Skin.Location = new System.Drawing.Point(16, 8);
      this.Skin.Name = "Skin";
      this.Skin.Size = new System.Drawing.Size(320, 144);
      this.Skin.TabIndex = 6;
      this.Skin.TabStop = false;
      this.Skin.Text = "Skin";
      // 
      // groupBox14
      // 
      this.groupBox14.Location = new System.Drawing.Point(16, 160);
      this.groupBox14.Name = "groupBox14";
      this.groupBox14.Size = new System.Drawing.Size(320, 120);
      this.groupBox14.TabIndex = 13;
      this.groupBox14.TabStop = false;
      this.groupBox14.Text = "Project info";
      // 
      // tabAudioPlayer
      // 
      this.tabAudioPlayer.Controls.Add(this.groupBox15);
      this.tabAudioPlayer.Location = new System.Drawing.Point(4, 22);
      this.tabAudioPlayer.Name = "tabAudioPlayer";
      this.tabAudioPlayer.Size = new System.Drawing.Size(616, 374);
      this.tabAudioPlayer.TabIndex = 8;
      this.tabAudioPlayer.Text = "Audio Player";
      // 
      // groupBox15
      // 
      this.groupBox15.Controls.Add(this.label30);
      this.groupBox15.Controls.Add(this.comboAudioPlayer);
      this.groupBox15.Location = new System.Drawing.Point(16, 16);
      this.groupBox15.Name = "groupBox15";
      this.groupBox15.Size = new System.Drawing.Size(496, 328);
      this.groupBox15.TabIndex = 0;
      this.groupBox15.TabStop = false;
      this.groupBox15.Text = "Audio player";
      // 
      // label30
      // 
      this.label30.Location = new System.Drawing.Point(16, 32);
      this.label30.Name = "label30";
      this.label30.Size = new System.Drawing.Size(40, 16);
      this.label30.TabIndex = 1;
      this.label30.Text = "player:";
      // 
      // comboAudioPlayer
      // 
      this.comboAudioPlayer.Items.AddRange(new object[] {
                                                          "Windows MediaPlayer 9",
                                                          "DirectShow"});
      this.comboAudioPlayer.Location = new System.Drawing.Point(64, 32);
      this.comboAudioPlayer.Name = "comboAudioPlayer";
      this.comboAudioPlayer.Size = new System.Drawing.Size(121, 21);
      this.comboAudioPlayer.TabIndex = 2;
      // 
      // tabPlayers
      // 
      this.tabPlayers.Controls.Add(this.groupBox9);
      this.tabPlayers.Controls.Add(this.groupBox8);
      this.tabPlayers.Controls.Add(this.MoviePlayerBox);
      this.tabPlayers.Location = new System.Drawing.Point(4, 22);
      this.tabPlayers.Name = "tabPlayers";
      this.tabPlayers.Size = new System.Drawing.Size(616, 374);
      this.tabPlayers.TabIndex = 3;
      this.tabPlayers.Text = "MoviePlayer";
      // 
      // groupBox9
      // 
      this.groupBox9.Controls.Add(this.labelOSDTimeout);
      this.groupBox9.Controls.Add(this.label15);
      this.groupBox9.Controls.Add(this.trackBarOSDTimeout);
      this.groupBox9.Location = new System.Drawing.Point(304, 128);
      this.groupBox9.Name = "groupBox9";
      this.groupBox9.Size = new System.Drawing.Size(200, 120);
      this.groupBox9.TabIndex = 8;
      this.groupBox9.TabStop = false;
      this.groupBox9.Text = "Onscreen display";
      // 
      // labelOSDTimeout
      // 
      this.labelOSDTimeout.Location = new System.Drawing.Point(80, 24);
      this.labelOSDTimeout.Name = "labelOSDTimeout";
      this.labelOSDTimeout.Size = new System.Drawing.Size(64, 16);
      this.labelOSDTimeout.TabIndex = 2;
      this.labelOSDTimeout.Text = "none";
      // 
      // label15
      // 
      this.label15.Location = new System.Drawing.Point(16, 24);
      this.label15.Name = "label15";
      this.label15.Size = new System.Drawing.Size(48, 16);
      this.label15.TabIndex = 1;
      this.label15.Text = "Timeout";
      // 
      // trackBarOSDTimeout
      // 
      this.trackBarOSDTimeout.Location = new System.Drawing.Point(24, 48);
      this.trackBarOSDTimeout.Name = "trackBarOSDTimeout";
      this.trackBarOSDTimeout.Size = new System.Drawing.Size(120, 42);
      this.trackBarOSDTimeout.TabIndex = 0;
      this.trackBarOSDTimeout.ValueChanged += new System.EventHandler(this.trackBarOSDTimeout_ValueChanged);
      // 
      // groupBox8
      // 
      this.groupBox8.Controls.Add(this.btnChooseSubFont);
      this.groupBox8.Controls.Add(this.txtBoxSubFont);
      this.groupBox8.Controls.Add(this.numericUpDownSubShadow);
      this.groupBox8.Controls.Add(this.label16);
      this.groupBox8.Controls.Add(this.label14);
      this.groupBox8.Controls.Add(this.checkBoxShowSubtitles);
      this.groupBox8.Location = new System.Drawing.Point(24, 128);
      this.groupBox8.Name = "groupBox8";
      this.groupBox8.Size = new System.Drawing.Size(264, 120);
      this.groupBox8.TabIndex = 7;
      this.groupBox8.TabStop = false;
      this.groupBox8.Text = "Subtitles";
      // 
      // btnChooseSubFont
      // 
      this.btnChooseSubFont.Location = new System.Drawing.Point(224, 56);
      this.btnChooseSubFont.Name = "btnChooseSubFont";
      this.btnChooseSubFont.Size = new System.Drawing.Size(24, 23);
      this.btnChooseSubFont.TabIndex = 10;
      this.btnChooseSubFont.Text = "...";
      this.btnChooseSubFont.Click += new System.EventHandler(this.btnChooseSubFont_Click);
      // 
      // txtBoxSubFont
      // 
      this.txtBoxSubFont.Enabled = false;
      this.txtBoxSubFont.Location = new System.Drawing.Point(104, 56);
      this.txtBoxSubFont.Name = "txtBoxSubFont";
      this.txtBoxSubFont.Size = new System.Drawing.Size(112, 20);
      this.txtBoxSubFont.TabIndex = 9;
      this.txtBoxSubFont.Text = "textBoxSubFont";
      // 
      // numericUpDownSubShadow
      // 
      this.numericUpDownSubShadow.Location = new System.Drawing.Point(104, 88);
      this.numericUpDownSubShadow.Name = "numericUpDownSubShadow";
      this.numericUpDownSubShadow.Size = new System.Drawing.Size(48, 20);
      this.numericUpDownSubShadow.TabIndex = 7;
      this.numericUpDownSubShadow.ValueChanged += new System.EventHandler(this.numericUpDownSubShadow_ValueChanged);
      // 
      // label16
      // 
      this.label16.Location = new System.Drawing.Point(24, 88);
      this.label16.Name = "label16";
      this.label16.Size = new System.Drawing.Size(72, 16);
      this.label16.TabIndex = 6;
      this.label16.Text = "Dropshadow";
      // 
      // label14
      // 
      this.label14.Location = new System.Drawing.Point(24, 56);
      this.label14.Name = "label14";
      this.label14.Size = new System.Drawing.Size(32, 16);
      this.label14.TabIndex = 1;
      this.label14.Text = "Font";
      // 
      // checkBoxShowSubtitles
      // 
      this.checkBoxShowSubtitles.Location = new System.Drawing.Point(24, 24);
      this.checkBoxShowSubtitles.Name = "checkBoxShowSubtitles";
      this.checkBoxShowSubtitles.Size = new System.Drawing.Size(56, 24);
      this.checkBoxShowSubtitles.TabIndex = 0;
      this.checkBoxShowSubtitles.Text = "Show";
      this.checkBoxShowSubtitles.CheckedChanged += new System.EventHandler(this.checkBoxShowSubtitles_CheckedChanged);
      // 
      // MoviePlayerBox
      // 
      this.MoviePlayerBox.Controls.Add(this.bntSelectMovieFile);
      this.MoviePlayerBox.Controls.Add(this.movieParameters);
      this.MoviePlayerBox.Controls.Add(this.Parameters);
      this.MoviePlayerBox.Controls.Add(this.movieFile);
      this.MoviePlayerBox.Controls.Add(this.label3);
      this.MoviePlayerBox.Controls.Add(this.checkBoxMovieInternalPlayer);
      this.MoviePlayerBox.Location = new System.Drawing.Point(24, 16);
      this.MoviePlayerBox.Name = "MoviePlayerBox";
      this.MoviePlayerBox.Size = new System.Drawing.Size(472, 104);
      this.MoviePlayerBox.TabIndex = 6;
      this.MoviePlayerBox.TabStop = false;
      this.MoviePlayerBox.Text = "Movie Player";
      // 
      // bntSelectMovieFile
      // 
      this.bntSelectMovieFile.Location = new System.Drawing.Point(416, 32);
      this.bntSelectMovieFile.Name = "bntSelectMovieFile";
      this.bntSelectMovieFile.Size = new System.Drawing.Size(24, 23);
      this.bntSelectMovieFile.TabIndex = 4;
      this.bntSelectMovieFile.Text = "...";
      this.bntSelectMovieFile.Click += new System.EventHandler(this.bntSelectMovieFile_Click);
      // 
      // movieParameters
      // 
      this.movieParameters.Location = new System.Drawing.Point(32, 72);
      this.movieParameters.Name = "movieParameters";
      this.movieParameters.Size = new System.Drawing.Size(152, 20);
      this.movieParameters.TabIndex = 3;
      this.movieParameters.Text = "";
      // 
      // Parameters
      // 
      this.Parameters.Location = new System.Drawing.Point(16, 56);
      this.Parameters.Name = "Parameters";
      this.Parameters.TabIndex = 2;
      this.Parameters.Text = "Parameters";
      // 
      // movieFile
      // 
      this.movieFile.Location = new System.Drawing.Point(32, 32);
      this.movieFile.Name = "movieFile";
      this.movieFile.Size = new System.Drawing.Size(376, 20);
      this.movieFile.TabIndex = 1;
      this.movieFile.Text = "";
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 16);
      this.label3.Name = "label3";
      this.label3.TabIndex = 0;
      this.label3.Text = "Filename";
      // 
      // checkBoxMovieInternalPlayer
      // 
      this.checkBoxMovieInternalPlayer.Location = new System.Drawing.Point(208, 72);
      this.checkBoxMovieInternalPlayer.Name = "checkBoxMovieInternalPlayer";
      this.checkBoxMovieInternalPlayer.Size = new System.Drawing.Size(128, 24);
      this.checkBoxMovieInternalPlayer.TabIndex = 7;
      this.checkBoxMovieInternalPlayer.Text = "Use Internal player";
      this.checkBoxMovieInternalPlayer.CheckedChanged += new System.EventHandler(this.checkBoxMovieInternalPlayer_CheckedChanged);
      // 
      // TabDVDPlayer
      // 
      this.TabDVDPlayer.Controls.Add(this.DVDPlayerBox);
      this.TabDVDPlayer.Location = new System.Drawing.Point(4, 22);
      this.TabDVDPlayer.Name = "TabDVDPlayer";
      this.TabDVDPlayer.Size = new System.Drawing.Size(616, 374);
      this.TabDVDPlayer.TabIndex = 7;
      this.TabDVDPlayer.Text = "DVDPlayer";
      // 
      // DVDPlayerBox
      // 
      this.DVDPlayerBox.Controls.Add(this.checkBoxDVDSubtitles);
      this.DVDPlayerBox.Controls.Add(this.comboBoxSubtitleLanguage);
      this.DVDPlayerBox.Controls.Add(this.comboBoxAudioLanguage);
      this.DVDPlayerBox.Controls.Add(this.label29);
      this.DVDPlayerBox.Controls.Add(this.label28);
      this.DVDPlayerBox.Controls.Add(this.checkBoxInternalDVDPlayer);
      this.DVDPlayerBox.Controls.Add(this.label1);
      this.DVDPlayerBox.Controls.Add(this.dvdFile);
      this.DVDPlayerBox.Controls.Add(this.dvdbtnSelect);
      this.DVDPlayerBox.Controls.Add(this.label2);
      this.DVDPlayerBox.Controls.Add(this.dvdParams);
      this.DVDPlayerBox.Location = new System.Drawing.Point(8, 16);
      this.DVDPlayerBox.Name = "DVDPlayerBox";
      this.DVDPlayerBox.Size = new System.Drawing.Size(592, 272);
      this.DVDPlayerBox.TabIndex = 6;
      this.DVDPlayerBox.TabStop = false;
      this.DVDPlayerBox.Text = "DVD Player";
      // 
      // checkBoxDVDSubtitles
      // 
      this.checkBoxDVDSubtitles.Location = new System.Drawing.Point(328, 184);
      this.checkBoxDVDSubtitles.Name = "checkBoxDVDSubtitles";
      this.checkBoxDVDSubtitles.Size = new System.Drawing.Size(104, 16);
      this.checkBoxDVDSubtitles.TabIndex = 10;
      this.checkBoxDVDSubtitles.Text = "Show Subtitles";
      this.checkBoxDVDSubtitles.CheckedChanged += new System.EventHandler(this.checkBoxDVDSubtitles_CheckedChanged);
      // 
      // comboBoxSubtitleLanguage
      // 
      this.comboBoxSubtitleLanguage.Location = new System.Drawing.Point(160, 184);
      this.comboBoxSubtitleLanguage.Name = "comboBoxSubtitleLanguage";
      this.comboBoxSubtitleLanguage.Size = new System.Drawing.Size(152, 21);
      this.comboBoxSubtitleLanguage.Sorted = true;
      this.comboBoxSubtitleLanguage.TabIndex = 9;
      // 
      // comboBoxAudioLanguage
      // 
      this.comboBoxAudioLanguage.Location = new System.Drawing.Point(160, 152);
      this.comboBoxAudioLanguage.Name = "comboBoxAudioLanguage";
      this.comboBoxAudioLanguage.Size = new System.Drawing.Size(152, 21);
      this.comboBoxAudioLanguage.Sorted = true;
      this.comboBoxAudioLanguage.TabIndex = 8;
      // 
      // label29
      // 
      this.label29.Location = new System.Drawing.Point(24, 184);
      this.label29.Name = "label29";
      this.label29.Size = new System.Drawing.Size(136, 16);
      this.label29.TabIndex = 7;
      this.label29.Text = "Default subtitle language:";
      // 
      // label28
      // 
      this.label28.Location = new System.Drawing.Point(24, 152);
      this.label28.Name = "label28";
      this.label28.Size = new System.Drawing.Size(128, 16);
      this.label28.TabIndex = 6;
      this.label28.Text = "Default Audio language:";
      // 
      // checkBoxInternalDVDPlayer
      // 
      this.checkBoxInternalDVDPlayer.Location = new System.Drawing.Point(16, 16);
      this.checkBoxInternalDVDPlayer.Name = "checkBoxInternalDVDPlayer";
      this.checkBoxInternalDVDPlayer.Size = new System.Drawing.Size(152, 16);
      this.checkBoxInternalDVDPlayer.TabIndex = 5;
      this.checkBoxInternalDVDPlayer.Text = "Use Internal DVD player";
      this.checkBoxInternalDVDPlayer.CheckedChanged += new System.EventHandler(this.checkBoxInternalDVDPlayer_CheckedChanged);
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(32, 40);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(100, 16);
      this.label1.TabIndex = 0;
      this.label1.Text = "Filename";
      // 
      // dvdFile
      // 
      this.dvdFile.Location = new System.Drawing.Point(48, 56);
      this.dvdFile.Name = "dvdFile";
      this.dvdFile.Size = new System.Drawing.Size(392, 20);
      this.dvdFile.TabIndex = 1;
      this.dvdFile.Text = "";
      // 
      // dvdbtnSelect
      // 
      this.dvdbtnSelect.Location = new System.Drawing.Point(448, 56);
      this.dvdbtnSelect.Name = "dvdbtnSelect";
      this.dvdbtnSelect.Size = new System.Drawing.Size(24, 23);
      this.dvdbtnSelect.TabIndex = 2;
      this.dvdbtnSelect.Text = "...";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(32, 80);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(72, 16);
      this.label2.TabIndex = 3;
      this.label2.Text = "Parameters";
      // 
      // dvdParams
      // 
      this.dvdParams.Location = new System.Drawing.Point(48, 96);
      this.dvdParams.Name = "dvdParams";
      this.dvdParams.Size = new System.Drawing.Size(160, 20);
      this.dvdParams.TabIndex = 4;
      this.dvdParams.Text = "";
      // 
      // tabAudioShares
      // 
      this.tabAudioShares.Controls.Add(this.groupBox2);
      this.tabAudioShares.Controls.Add(this.audioGroupBox);
      this.tabAudioShares.Location = new System.Drawing.Point(4, 22);
      this.tabAudioShares.Name = "tabAudioShares";
      this.tabAudioShares.Size = new System.Drawing.Size(616, 374);
      this.tabAudioShares.TabIndex = 0;
      this.tabAudioShares.Text = "Music";
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.checkBoxShufflePlaylists);
      this.groupBox2.Controls.Add(this.chkBoxRepeatAudioPlaylist);
      this.groupBox2.Controls.Add(this.txtboxAudioFiles);
      this.groupBox2.Controls.Add(this.label8);
      this.groupBox2.Controls.Add(this.chkMusicID3);
      this.groupBox2.Location = new System.Drawing.Point(24, 272);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(496, 88);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Music settings";
      // 
      // checkBoxShufflePlaylists
      // 
      this.checkBoxShufflePlaylists.Location = new System.Drawing.Point(32, 56);
      this.checkBoxShufflePlaylists.Name = "checkBoxShufflePlaylists";
      this.checkBoxShufflePlaylists.Size = new System.Drawing.Size(136, 24);
      this.checkBoxShufflePlaylists.TabIndex = 4;
      this.checkBoxShufflePlaylists.Text = "Auto shuffle playlist";
      // 
      // chkBoxRepeatAudioPlaylist
      // 
      this.chkBoxRepeatAudioPlaylist.Location = new System.Drawing.Point(32, 40);
      this.chkBoxRepeatAudioPlaylist.Name = "chkBoxRepeatAudioPlaylist";
      this.chkBoxRepeatAudioPlaylist.Size = new System.Drawing.Size(104, 16);
      this.chkBoxRepeatAudioPlaylist.TabIndex = 3;
      this.chkBoxRepeatAudioPlaylist.Text = "Repeat playlists";
      // 
      // txtboxAudioFiles
      // 
      this.txtboxAudioFiles.Location = new System.Drawing.Point(240, 40);
      this.txtboxAudioFiles.Name = "txtboxAudioFiles";
      this.txtboxAudioFiles.Size = new System.Drawing.Size(232, 20);
      this.txtboxAudioFiles.TabIndex = 2;
      this.txtboxAudioFiles.Text = "";
      // 
      // label8
      // 
      this.label8.Location = new System.Drawing.Point(176, 48);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(64, 16);
      this.label8.TabIndex = 1;
      this.label8.Text = "Audio files:";
      // 
      // chkMusicID3
      // 
      this.chkMusicID3.Location = new System.Drawing.Point(32, 16);
      this.chkMusicID3.Name = "chkMusicID3";
      this.chkMusicID3.TabIndex = 0;
      this.chkMusicID3.Text = "Show ID3 tags";
      // 
      // audioGroupBox
      // 
      this.audioGroupBox.Controls.Add(this.btnEditMusicShare);
      this.audioGroupBox.Controls.Add(this.btnDelAudioShare);
      this.audioGroupBox.Controls.Add(this.btnAddAudioShare);
      this.audioGroupBox.Controls.Add(this.listAudioShares);
      this.audioGroupBox.Location = new System.Drawing.Point(24, 16);
      this.audioGroupBox.Name = "audioGroupBox";
      this.audioGroupBox.Size = new System.Drawing.Size(496, 248);
      this.audioGroupBox.TabIndex = 0;
      this.audioGroupBox.TabStop = false;
      this.audioGroupBox.Text = "Music folders";
      // 
      // btnEditMusicShare
      // 
      this.btnEditMusicShare.Location = new System.Drawing.Point(136, 216);
      this.btnEditMusicShare.Name = "btnEditMusicShare";
      this.btnEditMusicShare.Size = new System.Drawing.Size(48, 23);
      this.btnEditMusicShare.TabIndex = 3;
      this.btnEditMusicShare.Text = "Edit";
      this.btnEditMusicShare.Click += new System.EventHandler(this.btnEditMusicShare_Click);
      // 
      // btnDelAudioShare
      // 
      this.btnDelAudioShare.Location = new System.Drawing.Point(72, 216);
      this.btnDelAudioShare.Name = "btnDelAudioShare";
      this.btnDelAudioShare.Size = new System.Drawing.Size(56, 23);
      this.btnDelAudioShare.TabIndex = 2;
      this.btnDelAudioShare.Text = "Delete";
      this.btnDelAudioShare.Click += new System.EventHandler(this.btnDelAudioShare_Click);
      // 
      // btnAddAudioShare
      // 
      this.btnAddAudioShare.Location = new System.Drawing.Point(8, 216);
      this.btnAddAudioShare.Name = "btnAddAudioShare";
      this.btnAddAudioShare.Size = new System.Drawing.Size(56, 23);
      this.btnAddAudioShare.TabIndex = 1;
      this.btnAddAudioShare.Text = "Add";
      this.btnAddAudioShare.Click += new System.EventHandler(this.btnAddAudioShare_Click);
      // 
      // listAudioShares
      // 
      this.listAudioShares.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                      this.HdrAudioName,
                                                                                      this.HdrAudioFolder});
      this.listAudioShares.FullRowSelect = true;
      this.listAudioShares.HideSelection = false;
      this.listAudioShares.Location = new System.Drawing.Point(8, 24);
      this.listAudioShares.MultiSelect = false;
      this.listAudioShares.Name = "listAudioShares";
      this.listAudioShares.Size = new System.Drawing.Size(448, 176);
      this.listAudioShares.TabIndex = 0;
      this.listAudioShares.View = System.Windows.Forms.View.Details;
      this.listAudioShares.DoubleClick += new System.EventHandler(this.listAudioShares_DoubleClick);
      // 
      // HdrAudioName
      // 
      this.HdrAudioName.Text = "Name";
      this.HdrAudioName.Width = 100;
      // 
      // HdrAudioFolder
      // 
      this.HdrAudioFolder.Text = "Folder";
      this.HdrAudioFolder.Width = 277;
      // 
      // tabVideoShares
      // 
      this.tabVideoShares.Controls.Add(this.groupBox3);
      this.tabVideoShares.Controls.Add(this.VideoGroupBox);
      this.tabVideoShares.Location = new System.Drawing.Point(4, 22);
      this.tabVideoShares.Name = "tabVideoShares";
      this.tabVideoShares.Size = new System.Drawing.Size(616, 374);
      this.tabVideoShares.TabIndex = 1;
      this.tabVideoShares.Text = "Movies";
      // 
      // groupBox3
      // 
      this.groupBox3.Controls.Add(this.chkBoxVideoRepeat);
      this.groupBox3.Controls.Add(this.txtboxVideoFiles);
      this.groupBox3.Controls.Add(this.label9);
      this.groupBox3.Location = new System.Drawing.Point(24, 272);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(496, 88);
      this.groupBox3.TabIndex = 1;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Movie Settings";
      // 
      // chkBoxVideoRepeat
      // 
      this.chkBoxVideoRepeat.Location = new System.Drawing.Point(16, 24);
      this.chkBoxVideoRepeat.Name = "chkBoxVideoRepeat";
      this.chkBoxVideoRepeat.TabIndex = 2;
      this.chkBoxVideoRepeat.Text = "Repeat Playlists";
      // 
      // txtboxVideoFiles
      // 
      this.txtboxVideoFiles.Location = new System.Drawing.Point(216, 24);
      this.txtboxVideoFiles.Name = "txtboxVideoFiles";
      this.txtboxVideoFiles.Size = new System.Drawing.Size(264, 20);
      this.txtboxVideoFiles.TabIndex = 1;
      this.txtboxVideoFiles.Text = "";
      // 
      // label9
      // 
      this.label9.Location = new System.Drawing.Point(144, 32);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(64, 23);
      this.label9.TabIndex = 0;
      this.label9.Text = "Movie files:";
      // 
      // VideoGroupBox
      // 
      this.VideoGroupBox.Controls.Add(this.btnEditMovieShare);
      this.VideoGroupBox.Controls.Add(this.btnDelVideoShare);
      this.VideoGroupBox.Controls.Add(this.btnAddVideoShare);
      this.VideoGroupBox.Controls.Add(this.listVideoShares);
      this.VideoGroupBox.Location = new System.Drawing.Point(24, 16);
      this.VideoGroupBox.Name = "VideoGroupBox";
      this.VideoGroupBox.Size = new System.Drawing.Size(496, 248);
      this.VideoGroupBox.TabIndex = 0;
      this.VideoGroupBox.TabStop = false;
      this.VideoGroupBox.Text = "Movie folders";
      // 
      // btnEditMovieShare
      // 
      this.btnEditMovieShare.Location = new System.Drawing.Point(136, 216);
      this.btnEditMovieShare.Name = "btnEditMovieShare";
      this.btnEditMovieShare.Size = new System.Drawing.Size(48, 23);
      this.btnEditMovieShare.TabIndex = 3;
      this.btnEditMovieShare.Text = "Edit";
      this.btnEditMovieShare.Click += new System.EventHandler(this.btnEditMovieShare_Click);
      // 
      // btnDelVideoShare
      // 
      this.btnDelVideoShare.Location = new System.Drawing.Point(72, 216);
      this.btnDelVideoShare.Name = "btnDelVideoShare";
      this.btnDelVideoShare.Size = new System.Drawing.Size(56, 23);
      this.btnDelVideoShare.TabIndex = 2;
      this.btnDelVideoShare.Text = "Delete";
      this.btnDelVideoShare.Click += new System.EventHandler(this.btnDelVideoShare_Click);
      // 
      // btnAddVideoShare
      // 
      this.btnAddVideoShare.Location = new System.Drawing.Point(8, 216);
      this.btnAddVideoShare.Name = "btnAddVideoShare";
      this.btnAddVideoShare.Size = new System.Drawing.Size(56, 23);
      this.btnAddVideoShare.TabIndex = 1;
      this.btnAddVideoShare.Text = "Add";
      this.btnAddVideoShare.Click += new System.EventHandler(this.btnAddVideoShare_Click);
      // 
      // listVideoShares
      // 
      this.listVideoShares.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                      this.HdrVideoName,
                                                                                      this.HdrVideoFolder});
      this.listVideoShares.FullRowSelect = true;
      this.listVideoShares.HideSelection = false;
      this.listVideoShares.Location = new System.Drawing.Point(8, 24);
      this.listVideoShares.MultiSelect = false;
      this.listVideoShares.Name = "listVideoShares";
      this.listVideoShares.Size = new System.Drawing.Size(448, 176);
      this.listVideoShares.TabIndex = 0;
      this.listVideoShares.View = System.Windows.Forms.View.Details;
      this.listVideoShares.DoubleClick += new System.EventHandler(this.listVideoShares_DoubleClick);
      // 
      // HdrVideoName
      // 
      this.HdrVideoName.Text = "Name";
      this.HdrVideoName.Width = 100;
      // 
      // HdrVideoFolder
      // 
      this.HdrVideoFolder.Text = "Folder";
      this.HdrVideoFolder.Width = 277;
      // 
      // tabPictureShares
      // 
      this.tabPictureShares.Controls.Add(this.groupBox1);
      this.tabPictureShares.Controls.Add(this.PictureGroupBox);
      this.tabPictureShares.Location = new System.Drawing.Point(4, 22);
      this.tabPictureShares.Name = "tabPictureShares";
      this.tabPictureShares.Size = new System.Drawing.Size(616, 374);
      this.tabPictureShares.TabIndex = 2;
      this.tabPictureShares.Text = "Pictures";
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.txtBoxPictureFiles);
      this.groupBox1.Controls.Add(this.label10);
      this.groupBox1.Controls.Add(this.label7);
      this.groupBox1.Controls.Add(this.label6);
      this.groupBox1.Controls.Add(this.UpDownPictureTransition);
      this.groupBox1.Controls.Add(this.label5);
      this.groupBox1.Controls.Add(this.label4);
      this.groupBox1.Controls.Add(this.UpDownPictureDuration);
      this.groupBox1.Location = new System.Drawing.Point(24, 272);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(496, 88);
      this.groupBox1.TabIndex = 2;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Slideshow";
      // 
      // txtBoxPictureFiles
      // 
      this.txtBoxPictureFiles.Location = new System.Drawing.Point(96, 56);
      this.txtBoxPictureFiles.Name = "txtBoxPictureFiles";
      this.txtBoxPictureFiles.Size = new System.Drawing.Size(376, 20);
      this.txtBoxPictureFiles.TabIndex = 8;
      this.txtBoxPictureFiles.Text = "";
      // 
      // label10
      // 
      this.label10.Location = new System.Drawing.Point(24, 56);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(72, 24);
      this.label10.TabIndex = 7;
      this.label10.Text = "Picture files";
      // 
      // label7
      // 
      this.label7.Location = new System.Drawing.Point(320, 24);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(48, 16);
      this.label7.TabIndex = 6;
      this.label7.Text = "Frames";
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(120, 24);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(32, 23);
      this.label6.TabIndex = 5;
      this.label6.Text = "sec.";
      // 
      // UpDownPictureTransition
      // 
      this.UpDownPictureTransition.Location = new System.Drawing.Point(256, 24);
      this.UpDownPictureTransition.Minimum = new System.Decimal(new int[] {
                                                                            1,
                                                                            0,
                                                                            0,
                                                                            0});
      this.UpDownPictureTransition.Name = "UpDownPictureTransition";
      this.UpDownPictureTransition.Size = new System.Drawing.Size(48, 20);
      this.UpDownPictureTransition.TabIndex = 4;
      this.UpDownPictureTransition.Value = new System.Decimal(new int[] {
                                                                          1,
                                                                          0,
                                                                          0,
                                                                          0});
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(192, 24);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(56, 23);
      this.label5.TabIndex = 3;
      this.label5.Text = "Transition";
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(24, 24);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(48, 23);
      this.label4.TabIndex = 2;
      this.label4.Text = "Duration";
      // 
      // UpDownPictureDuration
      // 
      this.UpDownPictureDuration.Location = new System.Drawing.Point(80, 24);
      this.UpDownPictureDuration.Maximum = new System.Decimal(new int[] {
                                                                          20,
                                                                          0,
                                                                          0,
                                                                          0});
      this.UpDownPictureDuration.Minimum = new System.Decimal(new int[] {
                                                                          1,
                                                                          0,
                                                                          0,
                                                                          0});
      this.UpDownPictureDuration.Name = "UpDownPictureDuration";
      this.UpDownPictureDuration.Size = new System.Drawing.Size(32, 20);
      this.UpDownPictureDuration.TabIndex = 1;
      this.UpDownPictureDuration.Value = new System.Decimal(new int[] {
                                                                        3,
                                                                        0,
                                                                        0,
                                                                        0});
      // 
      // PictureGroupBox
      // 
      this.PictureGroupBox.Controls.Add(this.btnEditPictureShare);
      this.PictureGroupBox.Controls.Add(this.btnDelPictureShare);
      this.PictureGroupBox.Controls.Add(this.btnAddPictureShare);
      this.PictureGroupBox.Controls.Add(this.listPictureShares);
      this.PictureGroupBox.Location = new System.Drawing.Point(24, 16);
      this.PictureGroupBox.Name = "PictureGroupBox";
      this.PictureGroupBox.Size = new System.Drawing.Size(496, 248);
      this.PictureGroupBox.TabIndex = 0;
      this.PictureGroupBox.TabStop = false;
      this.PictureGroupBox.Text = "Picture folders";
      // 
      // btnEditPictureShare
      // 
      this.btnEditPictureShare.Location = new System.Drawing.Point(136, 216);
      this.btnEditPictureShare.Name = "btnEditPictureShare";
      this.btnEditPictureShare.Size = new System.Drawing.Size(48, 23);
      this.btnEditPictureShare.TabIndex = 3;
      this.btnEditPictureShare.Text = "Edit";
      this.btnEditPictureShare.Click += new System.EventHandler(this.btnEditPictureShare_Click);
      // 
      // btnDelPictureShare
      // 
      this.btnDelPictureShare.Location = new System.Drawing.Point(72, 216);
      this.btnDelPictureShare.Name = "btnDelPictureShare";
      this.btnDelPictureShare.Size = new System.Drawing.Size(56, 23);
      this.btnDelPictureShare.TabIndex = 2;
      this.btnDelPictureShare.Text = "Delete";
      this.btnDelPictureShare.Click += new System.EventHandler(this.btnDelPictureShare_Click);
      // 
      // btnAddPictureShare
      // 
      this.btnAddPictureShare.Location = new System.Drawing.Point(8, 216);
      this.btnAddPictureShare.Name = "btnAddPictureShare";
      this.btnAddPictureShare.Size = new System.Drawing.Size(56, 23);
      this.btnAddPictureShare.TabIndex = 1;
      this.btnAddPictureShare.Text = "Add";
      this.btnAddPictureShare.Click += new System.EventHandler(this.btnAddPictureShare_Click);
      // 
      // listPictureShares
      // 
      this.listPictureShares.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                        this.HdrPictureName,
                                                                                        this.HdrPictureFolder});
      this.listPictureShares.FullRowSelect = true;
      this.listPictureShares.HideSelection = false;
      this.listPictureShares.Location = new System.Drawing.Point(0, 24);
      this.listPictureShares.MultiSelect = false;
      this.listPictureShares.Name = "listPictureShares";
      this.listPictureShares.Size = new System.Drawing.Size(448, 176);
      this.listPictureShares.TabIndex = 0;
      this.listPictureShares.View = System.Windows.Forms.View.Details;
      this.listPictureShares.DoubleClick += new System.EventHandler(this.listPictureShares_DoubleClick);
      // 
      // HdrPictureName
      // 
      this.HdrPictureName.Text = "Name";
      // 
      // HdrPictureFolder
      // 
      this.HdrPictureFolder.Text = "Folder";
      this.HdrPictureFolder.Width = 277;
      // 
      // tabWeather
      // 
      this.tabWeather.Controls.Add(this.groupBox7);
      this.tabWeather.Controls.Add(this.groupBox6);
      this.tabWeather.Controls.Add(this.groupBox5);
      this.tabWeather.Controls.Add(this.groupBox4);
      this.tabWeather.Location = new System.Drawing.Point(4, 22);
      this.tabWeather.Name = "tabWeather";
      this.tabWeather.Size = new System.Drawing.Size(616, 374);
      this.tabWeather.TabIndex = 4;
      this.tabWeather.Text = "Weather";
      // 
      // groupBox7
      // 
      this.groupBox7.Controls.Add(this.label11);
      this.groupBox7.Controls.Add(this.cntrlweatherRefresh);
      this.groupBox7.Location = new System.Drawing.Point(320, 240);
      this.groupBox7.Name = "groupBox7";
      this.groupBox7.Size = new System.Drawing.Size(248, 88);
      this.groupBox7.TabIndex = 3;
      this.groupBox7.TabStop = false;
      this.groupBox7.Text = "Refresh every";
      // 
      // label11
      // 
      this.label11.Location = new System.Drawing.Point(104, 32);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(48, 23);
      this.label11.TabIndex = 1;
      this.label11.Text = "Minutes";
      // 
      // cntrlweatherRefresh
      // 
      this.cntrlweatherRefresh.Increment = new System.Decimal(new int[] {
                                                                          10,
                                                                          0,
                                                                          0,
                                                                          0});
      this.cntrlweatherRefresh.Location = new System.Drawing.Point(40, 32);
      this.cntrlweatherRefresh.Maximum = new System.Decimal(new int[] {
                                                                        120,
                                                                        0,
                                                                        0,
                                                                        0});
      this.cntrlweatherRefresh.Minimum = new System.Decimal(new int[] {
                                                                        10,
                                                                        0,
                                                                        0,
                                                                        0});
      this.cntrlweatherRefresh.Name = "cntrlweatherRefresh";
      this.cntrlweatherRefresh.Size = new System.Drawing.Size(48, 20);
      this.cntrlweatherRefresh.TabIndex = 0;
      this.cntrlweatherRefresh.Value = new System.Decimal(new int[] {
                                                                      30,
                                                                      0,
                                                                      0,
                                                                      0});
      // 
      // groupBox6
      // 
      this.groupBox6.Controls.Add(this.radioWindSpeedMPH);
      this.groupBox6.Controls.Add(this.radioWindSpeedKH);
      this.groupBox6.Controls.Add(this.radioWindSpeedMS);
      this.groupBox6.Location = new System.Drawing.Point(160, 240);
      this.groupBox6.Name = "groupBox6";
      this.groupBox6.Size = new System.Drawing.Size(144, 88);
      this.groupBox6.TabIndex = 2;
      this.groupBox6.TabStop = false;
      this.groupBox6.Text = "Wind speed";
      // 
      // radioWindSpeedMPH
      // 
      this.radioWindSpeedMPH.Location = new System.Drawing.Point(16, 48);
      this.radioWindSpeedMPH.Name = "radioWindSpeedMPH";
      this.radioWindSpeedMPH.TabIndex = 2;
      this.radioWindSpeedMPH.Text = "mph";
      // 
      // radioWindSpeedKH
      // 
      this.radioWindSpeedKH.Location = new System.Drawing.Point(16, 32);
      this.radioWindSpeedKH.Name = "radioWindSpeedKH";
      this.radioWindSpeedKH.TabIndex = 1;
      this.radioWindSpeedKH.Text = "km/hour";
      // 
      // radioWindSpeedMS
      // 
      this.radioWindSpeedMS.Location = new System.Drawing.Point(16, 16);
      this.radioWindSpeedMS.Name = "radioWindSpeedMS";
      this.radioWindSpeedMS.TabIndex = 0;
      this.radioWindSpeedMS.Text = "m/s";
      // 
      // groupBox5
      // 
      this.groupBox5.Controls.Add(this.radioFarenHeit);
      this.groupBox5.Controls.Add(this.radioCelsius);
      this.groupBox5.Location = new System.Drawing.Point(16, 240);
      this.groupBox5.Name = "groupBox5";
      this.groupBox5.Size = new System.Drawing.Size(128, 88);
      this.groupBox5.TabIndex = 1;
      this.groupBox5.TabStop = false;
      this.groupBox5.Text = "Temperature";
      // 
      // radioFarenHeit
      // 
      this.radioFarenHeit.Location = new System.Drawing.Point(24, 40);
      this.radioFarenHeit.Name = "radioFarenHeit";
      this.radioFarenHeit.Size = new System.Drawing.Size(88, 32);
      this.radioFarenHeit.TabIndex = 1;
      this.radioFarenHeit.Text = "Fahrenheit";
      // 
      // radioCelsius
      // 
      this.radioCelsius.Location = new System.Drawing.Point(24, 24);
      this.radioCelsius.Name = "radioCelsius";
      this.radioCelsius.Size = new System.Drawing.Size(72, 24);
      this.radioCelsius.TabIndex = 0;
      this.radioCelsius.Text = "Celsius";
      // 
      // groupBox4
      // 
      this.groupBox4.Controls.Add(this.btnWeatherDel);
      this.groupBox4.Controls.Add(this.btnWeatherAddCity);
      this.groupBox4.Controls.Add(this.listViewWeather);
      this.groupBox4.Location = new System.Drawing.Point(16, 8);
      this.groupBox4.Name = "groupBox4";
      this.groupBox4.Size = new System.Drawing.Size(552, 224);
      this.groupBox4.TabIndex = 0;
      this.groupBox4.TabStop = false;
      this.groupBox4.Text = "Cities";
      // 
      // btnWeatherDel
      // 
      this.btnWeatherDel.Location = new System.Drawing.Point(80, 192);
      this.btnWeatherDel.Name = "btnWeatherDel";
      this.btnWeatherDel.Size = new System.Drawing.Size(48, 23);
      this.btnWeatherDel.TabIndex = 2;
      this.btnWeatherDel.Text = "Delete";
      this.btnWeatherDel.Click += new System.EventHandler(this.btnWeatherDel_Click);
      // 
      // btnWeatherAddCity
      // 
      this.btnWeatherAddCity.Location = new System.Drawing.Point(16, 192);
      this.btnWeatherAddCity.Name = "btnWeatherAddCity";
      this.btnWeatherAddCity.Size = new System.Drawing.Size(48, 23);
      this.btnWeatherAddCity.TabIndex = 1;
      this.btnWeatherAddCity.Text = "Add";
      this.btnWeatherAddCity.Click += new System.EventHandler(this.btnWeatherAddCity_Click);
      // 
      // listViewWeather
      // 
      this.listViewWeather.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                      this.WeatherHeader1,
                                                                                      this.WeatherHeader2});
      this.listViewWeather.Location = new System.Drawing.Point(16, 24);
      this.listViewWeather.Name = "listViewWeather";
      this.listViewWeather.Size = new System.Drawing.Size(504, 160);
      this.listViewWeather.TabIndex = 0;
      this.listViewWeather.View = System.Windows.Forms.View.Details;
      // 
      // WeatherHeader1
      // 
      this.WeatherHeader1.Text = "City";
      this.WeatherHeader1.Width = 252;
      // 
      // WeatherHeader2
      // 
      this.WeatherHeader2.Text = "shortcode";
      this.WeatherHeader2.Width = 121;
      // 
      // tabPageCapture
      // 
      this.tabPageCapture.Controls.Add(this.listPropertyPages);
      this.tabPageCapture.Controls.Add(this.groupBox13);
      this.tabPageCapture.Controls.Add(this.groupBox11);
      this.tabPageCapture.Controls.Add(this.btnRecPath);
      this.tabPageCapture.Controls.Add(this.textBoxRecPath);
      this.tabPageCapture.Controls.Add(this.groupBox12);
      this.tabPageCapture.Controls.Add(this.groupBox10);
      this.tabPageCapture.Controls.Add(this.groupBox16);
      this.tabPageCapture.Location = new System.Drawing.Point(4, 22);
      this.tabPageCapture.Name = "tabPageCapture";
      this.tabPageCapture.Size = new System.Drawing.Size(616, 374);
      this.tabPageCapture.TabIndex = 6;
      this.tabPageCapture.Text = "Video Capture";
      // 
      // listPropertyPages
      // 
      this.listPropertyPages.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                        this.columnHeader4});
      this.listPropertyPages.FullRowSelect = true;
      this.listPropertyPages.Location = new System.Drawing.Point(24, 256);
      this.listPropertyPages.MultiSelect = false;
      this.listPropertyPages.Name = "listPropertyPages";
      this.listPropertyPages.Size = new System.Drawing.Size(384, 97);
      this.listPropertyPages.TabIndex = 14;
      this.listPropertyPages.View = System.Windows.Forms.View.Details;
      this.listPropertyPages.DoubleClick += new System.EventHandler(this.listPropertyPages_DoubleClick);
      // 
      // columnHeader4
      // 
      this.columnHeader4.Text = "Property";
      this.columnHeader4.Width = 299;
      // 
      // groupBox13
      // 
      this.groupBox13.Controls.Add(this.upDownCountry);
      this.groupBox13.Controls.Add(this.label24);
      this.groupBox13.Controls.Add(this.label23);
      this.groupBox13.Controls.Add(this.btnradioCable);
      this.groupBox13.Controls.Add(this.btnradioAntenna);
      this.groupBox13.Location = new System.Drawing.Point(424, 88);
      this.groupBox13.Name = "groupBox13";
      this.groupBox13.Size = new System.Drawing.Size(176, 120);
      this.groupBox13.TabIndex = 12;
      this.groupBox13.TabStop = false;
      this.groupBox13.Text = "Tuner";
      // 
      // upDownCountry
      // 
      this.upDownCountry.Location = new System.Drawing.Point(64, 64);
      this.upDownCountry.Name = "upDownCountry";
      this.upDownCountry.Size = new System.Drawing.Size(56, 20);
      this.upDownCountry.TabIndex = 4;
      this.upDownCountry.Value = new System.Decimal(new int[] {
                                                                31,
                                                                0,
                                                                0,
                                                                0});
      // 
      // label24
      // 
      this.label24.Location = new System.Drawing.Point(8, 16);
      this.label24.Name = "label24";
      this.label24.Size = new System.Drawing.Size(40, 16);
      this.label24.TabIndex = 3;
      this.label24.Text = "Source:";
      // 
      // label23
      // 
      this.label23.Location = new System.Drawing.Point(8, 64);
      this.label23.Name = "label23";
      this.label23.Size = new System.Drawing.Size(48, 16);
      this.label23.TabIndex = 2;
      this.label23.Text = "Country:";
      // 
      // btnradioCable
      // 
      this.btnradioCable.Location = new System.Drawing.Point(56, 40);
      this.btnradioCable.Name = "btnradioCable";
      this.btnradioCable.Size = new System.Drawing.Size(72, 16);
      this.btnradioCable.TabIndex = 1;
      this.btnradioCable.Text = "Cable";
      // 
      // btnradioAntenna
      // 
      this.btnradioAntenna.Location = new System.Drawing.Point(56, 16);
      this.btnradioAntenna.Name = "btnradioAntenna";
      this.btnradioAntenna.Size = new System.Drawing.Size(72, 24);
      this.btnradioAntenna.TabIndex = 0;
      this.btnradioAntenna.Text = "Antenna";
      // 
      // groupBox11
      // 
      this.groupBox11.Controls.Add(this.label22);
      this.groupBox11.Controls.Add(this.comboBoxCaptureFormat);
      this.groupBox11.Location = new System.Drawing.Point(424, 8);
      this.groupBox11.Name = "groupBox11";
      this.groupBox11.Size = new System.Drawing.Size(176, 72);
      this.groupBox11.TabIndex = 11;
      this.groupBox11.TabStop = false;
      this.groupBox11.Text = "Capture format";
      // 
      // label22
      // 
      this.label22.Location = new System.Drawing.Point(16, 24);
      this.label22.Name = "label22";
      this.label22.Size = new System.Drawing.Size(40, 16);
      this.label22.TabIndex = 11;
      this.label22.Text = "Format:";
      // 
      // comboBoxCaptureFormat
      // 
      this.comboBoxCaptureFormat.Items.AddRange(new object[] {
                                                               ".avi",
                                                               ".asf",
                                                               ".mpg"});
      this.comboBoxCaptureFormat.Location = new System.Drawing.Point(64, 24);
      this.comboBoxCaptureFormat.Name = "comboBoxCaptureFormat";
      this.comboBoxCaptureFormat.Size = new System.Drawing.Size(88, 21);
      this.comboBoxCaptureFormat.TabIndex = 10;
      this.comboBoxCaptureFormat.Text = ".avi";
      // 
      // btnRecPath
      // 
      this.btnRecPath.Location = new System.Drawing.Point(368, 208);
      this.btnRecPath.Name = "btnRecPath";
      this.btnRecPath.Size = new System.Drawing.Size(24, 23);
      this.btnRecPath.TabIndex = 9;
      this.btnRecPath.Text = "...";
      this.btnRecPath.Click += new System.EventHandler(this.btnRecPath_Click);
      // 
      // textBoxRecPath
      // 
      this.textBoxRecPath.Location = new System.Drawing.Point(40, 208);
      this.textBoxRecPath.Name = "textBoxRecPath";
      this.textBoxRecPath.Size = new System.Drawing.Size(312, 20);
      this.textBoxRecPath.TabIndex = 8;
      this.textBoxRecPath.Text = "";
      // 
      // groupBox12
      // 
      this.groupBox12.Controls.Add(this.comboCompressorVideo);
      this.groupBox12.Controls.Add(this.label20);
      this.groupBox12.Controls.Add(this.label19);
      this.groupBox12.Controls.Add(this.comboCompressorAudio);
      this.groupBox12.Location = new System.Drawing.Point(24, 88);
      this.groupBox12.Name = "groupBox12";
      this.groupBox12.Size = new System.Drawing.Size(384, 80);
      this.groupBox12.TabIndex = 6;
      this.groupBox12.TabStop = false;
      this.groupBox12.Text = "Compressors";
      // 
      // comboCompressorVideo
      // 
      this.comboCompressorVideo.Location = new System.Drawing.Point(64, 16);
      this.comboCompressorVideo.Name = "comboCompressorVideo";
      this.comboCompressorVideo.Size = new System.Drawing.Size(240, 21);
      this.comboCompressorVideo.TabIndex = 6;
      this.comboCompressorVideo.SelectedIndexChanged += new System.EventHandler(this.comboCompressorVideo_SelectedIndexChanged);
      // 
      // label20
      // 
      this.label20.Location = new System.Drawing.Point(24, 16);
      this.label20.Name = "label20";
      this.label20.Size = new System.Drawing.Size(40, 16);
      this.label20.TabIndex = 5;
      this.label20.Text = "Video:";
      // 
      // label19
      // 
      this.label19.Location = new System.Drawing.Point(24, 48);
      this.label19.Name = "label19";
      this.label19.Size = new System.Drawing.Size(40, 16);
      this.label19.TabIndex = 4;
      this.label19.Text = "Audio:";
      // 
      // comboCompressorAudio
      // 
      this.comboCompressorAudio.Location = new System.Drawing.Point(64, 48);
      this.comboCompressorAudio.Name = "comboCompressorAudio";
      this.comboCompressorAudio.Size = new System.Drawing.Size(240, 21);
      this.comboCompressorAudio.TabIndex = 5;
      this.comboCompressorAudio.SelectedIndexChanged += new System.EventHandler(this.comboCompressorAudio_SelectedIndexChanged);
      // 
      // groupBox10
      // 
      this.groupBox10.Controls.Add(this.label18);
      this.groupBox10.Controls.Add(this.label17);
      this.groupBox10.Controls.Add(this.comboAudioDevice);
      this.groupBox10.Controls.Add(this.comboVideoDevice);
      this.groupBox10.Location = new System.Drawing.Point(24, 8);
      this.groupBox10.Name = "groupBox10";
      this.groupBox10.Size = new System.Drawing.Size(384, 80);
      this.groupBox10.TabIndex = 0;
      this.groupBox10.TabStop = false;
      this.groupBox10.Text = "Capture Devices";
      // 
      // label18
      // 
      this.label18.Location = new System.Drawing.Point(24, 48);
      this.label18.Name = "label18";
      this.label18.Size = new System.Drawing.Size(40, 16);
      this.label18.TabIndex = 3;
      this.label18.Text = "Audio:";
      // 
      // label17
      // 
      this.label17.Location = new System.Drawing.Point(24, 16);
      this.label17.Name = "label17";
      this.label17.Size = new System.Drawing.Size(40, 16);
      this.label17.TabIndex = 2;
      this.label17.Text = "Video:";
      // 
      // comboAudioDevice
      // 
      this.comboAudioDevice.Location = new System.Drawing.Point(64, 48);
      this.comboAudioDevice.Name = "comboAudioDevice";
      this.comboAudioDevice.Size = new System.Drawing.Size(240, 21);
      this.comboAudioDevice.TabIndex = 1;
      this.comboAudioDevice.SelectedIndexChanged += new System.EventHandler(this.comboAudioDevice_SelectedIndexChanged);
      // 
      // comboVideoDevice
      // 
      this.comboVideoDevice.Location = new System.Drawing.Point(64, 16);
      this.comboVideoDevice.Name = "comboVideoDevice";
      this.comboVideoDevice.Size = new System.Drawing.Size(240, 21);
      this.comboVideoDevice.TabIndex = 0;
      this.comboVideoDevice.SelectedIndexChanged += new System.EventHandler(this.comboVideoDevice_SelectedIndexChanged);
      // 
      // groupBox16
      // 
      this.groupBox16.Location = new System.Drawing.Point(24, 176);
      this.groupBox16.Name = "groupBox16";
      this.groupBox16.Size = new System.Drawing.Size(384, 72);
      this.groupBox16.TabIndex = 13;
      this.groupBox16.TabStop = false;
      this.groupBox16.Text = "Recording path";
      // 
      // tabTVChannels
      // 
      this.tabTVChannels.Controls.Add(this.textEditBox);
      this.tabTVChannels.Controls.Add(this.btnEditChannel);
      this.tabTVChannels.Controls.Add(this.btnDelChannel);
      this.tabTVChannels.Controls.Add(this.btnNewChannel);
      this.tabTVChannels.Controls.Add(this.btnTvChannelDown);
      this.tabTVChannels.Controls.Add(this.btnTvChannelUp);
      this.tabTVChannels.Controls.Add(this.listTVChannels);
      this.tabTVChannels.Location = new System.Drawing.Point(4, 22);
      this.tabTVChannels.Name = "tabTVChannels";
      this.tabTVChannels.Size = new System.Drawing.Size(616, 374);
      this.tabTVChannels.TabIndex = 9;
      this.tabTVChannels.Text = "TVChannels";
      // 
      // textEditBox
      // 
      this.textEditBox.Location = new System.Drawing.Point(504, 248);
      this.textEditBox.Name = "textEditBox";
      this.textEditBox.TabIndex = 18;
      this.textEditBox.Text = "textBox1";
      this.textEditBox.Visible = false;
      // 
      // btnEditChannel
      // 
      this.btnEditChannel.Location = new System.Drawing.Point(120, 240);
      this.btnEditChannel.Name = "btnEditChannel";
      this.btnEditChannel.Size = new System.Drawing.Size(48, 23);
      this.btnEditChannel.TabIndex = 17;
      this.btnEditChannel.Text = "Edit";
      this.btnEditChannel.Click += new System.EventHandler(this.btnEditChannel_Click);
      // 
      // btnDelChannel
      // 
      this.btnDelChannel.Location = new System.Drawing.Point(64, 240);
      this.btnDelChannel.Name = "btnDelChannel";
      this.btnDelChannel.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.btnDelChannel.Size = new System.Drawing.Size(48, 23);
      this.btnDelChannel.TabIndex = 16;
      this.btnDelChannel.Text = "Delete";
      this.btnDelChannel.Click += new System.EventHandler(this.btnDelChannel_Click);
      // 
      // btnNewChannel
      // 
      this.btnNewChannel.Location = new System.Drawing.Point(24, 240);
      this.btnNewChannel.Name = "btnNewChannel";
      this.btnNewChannel.Size = new System.Drawing.Size(32, 24);
      this.btnNewChannel.TabIndex = 15;
      this.btnNewChannel.Text = "Add";
      this.btnNewChannel.Click += new System.EventHandler(this.btnNewChannel_Click);
      // 
      // btnTvChannelDown
      // 
      this.btnTvChannelDown.Location = new System.Drawing.Point(416, 208);
      this.btnTvChannelDown.Name = "btnTvChannelDown";
      this.btnTvChannelDown.Size = new System.Drawing.Size(16, 23);
      this.btnTvChannelDown.TabIndex = 14;
      this.btnTvChannelDown.Text = "v";
      this.btnTvChannelDown.Click += new System.EventHandler(this.btnTvChannelDown_Click);
      // 
      // btnTvChannelUp
      // 
      this.btnTvChannelUp.Location = new System.Drawing.Point(416, 184);
      this.btnTvChannelUp.Name = "btnTvChannelUp";
      this.btnTvChannelUp.Size = new System.Drawing.Size(16, 24);
      this.btnTvChannelUp.TabIndex = 13;
      this.btnTvChannelUp.Text = "^";
      this.btnTvChannelUp.Click += new System.EventHandler(this.btnTvChannelUp_Click);
      // 
      // listTVChannels
      // 
      this.listTVChannels.AllowDrop = true;
      this.listTVChannels.AllowRowReorder = true;
      this.listTVChannels.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                     this.columnHeader1,
                                                                                     this.columnHeader2,
                                                                                     this.columnHeader3});
      this.listTVChannels.FullRowSelect = true;
      this.listTVChannels.HideSelection = false;
      this.listTVChannels.Location = new System.Drawing.Point(24, 16);
      this.listTVChannels.MultiSelect = false;
      this.listTVChannels.Name = "listTVChannels";
      this.listTVChannels.Size = new System.Drawing.Size(384, 216);
      this.listTVChannels.TabIndex = 0;
      this.listTVChannels.View = System.Windows.Forms.View.Details;
      this.listTVChannels.DoubleClick += new System.EventHandler(this.listTVChannels_DoubleClick);
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "TV Channel";
      this.columnHeader1.Width = 108;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Number";
      this.columnHeader2.Width = 118;
      // 
      // columnHeader3
      // 
      this.columnHeader3.Text = "Frequency";
      this.columnHeader3.Width = 75;
      // 
      // SetupForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(632, 405);
      this.Controls.Add(this.tabControl);
      this.Name = "SetupForm";
      this.Text = "Setup MediaPortal";
      this.Closing += new System.ComponentModel.CancelEventHandler(this.SetupForm_Closing);
      this.Load += new System.EventHandler(this.SetupForm_Load);
      this.tabControl.ResumeLayout(false);
      this.tabGeneral.ResumeLayout(false);
      this.tabAudioPlayer.ResumeLayout(false);
      this.groupBox15.ResumeLayout(false);
      this.tabPlayers.ResumeLayout(false);
      this.groupBox9.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.trackBarOSDTimeout)).EndInit();
      this.groupBox8.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSubShadow)).EndInit();
      this.MoviePlayerBox.ResumeLayout(false);
      this.TabDVDPlayer.ResumeLayout(false);
      this.DVDPlayerBox.ResumeLayout(false);
      this.tabAudioShares.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.audioGroupBox.ResumeLayout(false);
      this.tabVideoShares.ResumeLayout(false);
      this.groupBox3.ResumeLayout(false);
      this.VideoGroupBox.ResumeLayout(false);
      this.tabPictureShares.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.UpDownPictureTransition)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.UpDownPictureDuration)).EndInit();
      this.PictureGroupBox.ResumeLayout(false);
      this.tabWeather.ResumeLayout(false);
      this.groupBox7.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.cntrlweatherRefresh)).EndInit();
      this.groupBox6.ResumeLayout(false);
      this.groupBox5.ResumeLayout(false);
      this.groupBox4.ResumeLayout(false);
      this.tabPageCapture.ResumeLayout(false);
      this.groupBox13.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.upDownCountry)).EndInit();
      this.groupBox11.ResumeLayout(false);
      this.groupBox12.ResumeLayout(false);
      this.groupBox10.ResumeLayout(false);
      this.tabTVChannels.ResumeLayout(false);
      this.ResumeLayout(false);

    }
		#endregion


    private void dvdbtnSelect_Click(object sender, System.EventArgs e)
    {
      OpenFileDialog dlg=new OpenFileDialog();
      dlg.CheckFileExists=true;
      dlg.CheckPathExists=true;
      dlg.RestoreDirectory=true;
      dlg.Filter= "exe files (*.exe)|*.exe";
      dlg.FilterIndex=0;
      dlg.Title="Select DVD player";
      dlg.ShowDialog();
      if (dlg.FileName!="")
      {
        dvdFile.Text=dlg.FileName;
      }
    }

    private void bntSelectMovieFile_Click(object sender, System.EventArgs e)
    {
      OpenFileDialog dlg=new OpenFileDialog();
      dlg.CheckFileExists=true;
      dlg.CheckPathExists=true;
      dlg.RestoreDirectory=true;
      dlg.Filter= "exe files (*.exe)|*.exe";
      dlg.FilterIndex=0;
      dlg.Title="Select Movie Player";
      dlg.ShowDialog();
      if (dlg.FileName!="")
      {
        movieFile.Text=dlg.FileName;
      }
    }

		void LoadShares(AMS.Profile.Xml xmlreader)
		{
			listVideoShares.Items.Clear();
			listAudioShares.Items.Clear();
			listPictureShares.Items.Clear();
			ListViewItem newItem;
			string strPath;
			for (int i=0; i < 20; i++)
			{
				string strShareName=String.Format("sharename{0}",i);
				string strSharePath=String.Format("sharepath{0}",i);
				string strName=xmlreader.GetValueAsString("movies", strShareName,"");
				strPath=xmlreader.GetValueAsString("movies", strSharePath,"");
				if (strName.Length>0 && strPath.Length>0)
				{
					newItem=listVideoShares.Items.Add(strName);
					newItem.SubItems.Add(strPath);
				}

				strName=xmlreader.GetValueAsString("music", strShareName,"");
				strPath=xmlreader.GetValueAsString("music", strSharePath,"");
				if (strName.Length>0 && strPath.Length>0)
				{
					newItem=listAudioShares.Items.Add(strName);
					newItem.SubItems.Add(strPath);
				}

				strName=xmlreader.GetValueAsString("pictures", strShareName,"");
				strPath=xmlreader.GetValueAsString("pictures", strSharePath,"");
				if (strName.Length>0 && strPath.Length>0)
				{
					newItem=listPictureShares.Items.Add(strName);
					newItem.SubItems.Add(strPath);
				}
			}
			if (listVideoShares.Items.Count==0)
			{
				ListViewItem newItemVideo;

				strPath=String.Format(@"{0}\My Movies",Environment.GetFolderPath( Environment.SpecialFolder.Personal).ToString());
				System.IO.Directory.CreateDirectory(strPath);
				newItemVideo=listVideoShares.Items.Add("Movies");
				newItemVideo.SubItems.Add( strPath );
			}
			if (listAudioShares.Items.Count==0)
			{
				ListViewItem newItemAudio;
				strPath=Environment.GetFolderPath( Environment.SpecialFolder.MyMusic).ToString();
				if (strPath=="")
				{
					strPath=String.Format(@"{0}\My Music",Environment.GetFolderPath( Environment.SpecialFolder.Personal).ToString());
					System.IO.Directory.CreateDirectory(strPath);
				}
				newItemAudio=listAudioShares.Items.Add("Music");
				newItemAudio.SubItems.Add( strPath );
			}
			if (listPictureShares.Items.Count==0)
			{
				ListViewItem newItemPictures;
				strPath=Environment.GetFolderPath(Environment.SpecialFolder.MyPictures).ToString();
				if (strPath=="")
				{
					strPath=String.Format(@"{0}\My Pictures",Environment.GetFolderPath( Environment.SpecialFolder.Personal).ToString());
					System.IO.Directory.CreateDirectory(strPath);
				}
				newItemPictures=listPictureShares.Items.Add("Pictures");
				newItemPictures.SubItems.Add( strPath );
			}
		}

		void LoadSettings()
		{
			AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml");
			checkStartFullScreen.Checked=xmlreader.GetValueAsBool("general","startfullscreen",false);
      checkBoxAutoHideMouse.Checked=xmlreader.GetValueAsBool("general","autohidemouse",false);
      
			dvdFile.Text=xmlreader.GetValueAsString("dvdplayer","path",@"C:\program files\cyberlink\powerdvd\powerdvd.exe");
			dvdParams.Text=xmlreader.GetValueAsString("dvdplayer","arguments","");
      checkBoxInternalDVDPlayer.Checked=xmlreader.GetValueAsBool("dvdplayer","internal",true);
      
      string strDVDAudioLanguage=xmlreader.GetValueAsString("dvdplayer","audiolanguage","English");
      string strDVDSubLanguage=xmlreader.GetValueAsString("dvdplayer","subtitlelanguage","English");
      checkBoxDVDSubtitles.Checked=xmlreader.GetValueAsBool("dvdplayer","showsubtitles",true);
      AddLanguages(comboBoxAudioLanguage,strDVDAudioLanguage);
      AddLanguages(comboBoxSubtitleLanguage,strDVDSubLanguage);


			movieFile.Text=xmlreader.GetValueAsString("movieplayer","path",@"zplayer\zplayer.exe");
			movieParameters.Text=xmlreader.GetValueAsString("movieplayer","arguments", @"/PLAY /F /Q");
			checkBoxMovieInternalPlayer.Checked=xmlreader.GetValueAsBool("movieplayer","internal",true);
      checkBoxMovieInternalPlayer.Checked=true;
      

			string strAudioPlayer=xmlreader.GetValueAsString("audioplayer","player", "Windows Media Player 9");
			comboAudioPlayer.Items.Clear();
			comboAudioPlayer.Items.Add("Windows Media Player 9");
			comboAudioPlayer.Items.Add("DirectShow");
			comboAudioPlayer.Text=strAudioPlayer;


			trackBarOSDTimeout.Value=xmlreader.GetValueAsInt("movieplayer","osdtimeout",0);

			LoadShares(xmlreader);

			UpDownPictureDuration.Value=xmlreader.GetValueAsInt("pictures","speed",3);
			UpDownPictureTransition.Value=xmlreader.GetValueAsInt("pictures","transisition",20);
			
      chkBoxRepeatAudioPlaylist.Checked=xmlreader.GetValueAsBool("musicfiles","repeat",true);
      
			chkMusicID3.Checked=xmlreader.GetValueAsBool("musicfiles","showid3",true);

      checkBoxShufflePlaylists.Checked=xmlreader.GetValueAsBool("musicfiles","autoshuffle",true);
      chkBoxVideoRepeat.Checked=xmlreader.GetValueAsBool("movies","repeat",true);

      txtboxAudioFiles.Text=xmlreader.GetValueAsString("music","extensions",".mp3,.wma,.ogg,.flac,.wav");
			txtBoxPictureFiles.Text=xmlreader.GetValueAsString("pictures","extensions",".jpg,.jpeg,.gif,.bmp,.pcx,.png");
			txtboxVideoFiles.Text=xmlreader.GetValueAsString("movies","extensions",".avi,.mpg,.ogm,.mpeg,.mkv,.wmv,.ifo,.qt,.rm,.mov");

			LoadWeather(xmlreader);


			string strLanguage=xmlreader.GetValueAsString("skin","language","english");
			comboBoxLanguage.Items.Clear();
			int iItem=0;
			string[] Folders=System.IO.Directory.GetDirectories(@"language\","*.*");
			foreach (string strFolder in Folders)
			{
				string strFile=strFolder.Substring(@"language\".Length);
				if (strFile.ToLower()!="cvs")
				{
					comboBoxLanguage.Items.Add(strFile);
					if (strFile.ToLower()==strLanguage.ToLower())
					{
						comboBoxLanguage.SelectedIndex=iItem;
					}
					iItem++;
				}
			}
			

			string strSkin=xmlreader.GetValueAsString("skin","name","MediaCenter");
			comboBoxSkins.Items.Clear();
			iItem=0;
			Folders=System.IO.Directory.GetDirectories(@"skin\","*.*");
			foreach (string strFolder in Folders)
			{
				string strFile=strFolder.Substring(@"skin\".Length);
				if (strFile.ToLower()!="cvs")
				{
					comboBoxSkins.Items.Add(strFile);
					if (strFile.ToLower()==strSkin.ToLower())
					{
						comboBoxSkins.SelectedIndex=iItem;
					}
					iItem++;
				}
			}
      LoadSubtitles(xmlreader);
		}

    void LoadSubtitles(AMS.Profile.Xml xmlreader)
    {
      string strFontName=xmlreader.GetValueAsString("subtitles","fontface","Arial");
      string strColor=xmlreader.GetValueAsString("subtitles","color","ffffff");
      bool bBold   =xmlreader.GetValueAsBool("subtitles","bold",true);
      checkBoxShowSubtitles.Checked=xmlreader.GetValueAsBool("subtitles","enabled",true);
      int iFontSize=xmlreader.GetValueAsInt("subtitles","fontsize",18);
      numericUpDownSubShadow.Value=xmlreader.GetValueAsInt("subtitles","shadow",5);

      txtBoxSubFont.Text=String.Format("{0} {1}", strFontName,iFontSize);
      if (bBold) txtBoxSubFont.Text += ", Bold";

			if (strColor!=null)
			{
				try
				{
					int iColor=Int32.Parse(strColor,NumberStyles.HexNumber);
					txtBoxSubFont.BackColor=Color.FromArgb(iColor);
				}
				catch(Exception)
				{
				}
			}
    }

    void SaveSubtitles(AMS.Profile.Xml xmlWriter)
    {
      xmlWriter.SetValueAsBool("subtitles","enabled",checkBoxShowSubtitles.Checked );
      xmlWriter.SetValue("subtitles","shadow", numericUpDownSubShadow.Value.ToString());

    }

		void LoadWeather(AMS.Profile.Xml xmlreader)
		{
			
			listViewWeather.Items.Clear();
			ListViewItem newItem;
			for (int i=0; i < 20; i++)
			{
				string strCityTag=String.Format("city{0}",i);
				string strCodeTag=String.Format("code{0}",i);

				string strCity=xmlreader.GetValueAsString("weather",strCityTag,"");
				string strCode=xmlreader.GetValueAsString("weather",strCodeTag,"");
				if (strCity.Length>0 && strCode.Length>0)
				{
					newItem=listViewWeather.Items.Add(strCity);
					newItem.SubItems.Add(strCode);
				}
			}
			if (listViewWeather.Items.Count==0)
			{
				//add some default cities
				newItem=listViewWeather.Items.Add("London");
				newItem.SubItems.Add("UKXX0085");
				newItem=listViewWeather.Items.Add("Oslo");
				newItem.SubItems.Add("NOXX0029");
				newItem=listViewWeather.Items.Add("Madrid");
				newItem.SubItems.Add("SPXX0050");
			}
			string strTmp;
			radioCelsius.Checked=true;
			radioFarenHeit.Checked=false;
			strTmp=xmlreader.GetValueAsString("weather","temperature","C");
			if (strTmp=="F")
			{
				radioCelsius.Checked=false;
				radioFarenHeit.Checked=true;
			}
			radioWindSpeedKH.Checked=false;
			radioWindSpeedMPH.Checked=false;
			radioWindSpeedMS.Checked=false;
			strTmp=xmlreader.GetValueAsString("weather","speed","K");
			if (strTmp=="K") radioWindSpeedKH.Checked=true;
			if (strTmp=="M") radioWindSpeedMPH.Checked=true;
			if (strTmp=="S") radioWindSpeedMS.Checked=true;
			
			cntrlweatherRefresh.Value=xmlreader.GetValueAsInt("weather","refresh",30);
    }

		void SaveWeather(AMS.Profile.Xml xmlWriter)
		{
			ListViewItem newItem;
			for (int i=0; i < 20; i++)
			{
				string strCityTag=String.Format("city{0}",i);
				string strCodeTag=String.Format("code{0}",i);

				string strCity="";
				string strCode="";
				if (i < listViewWeather.Items.Count)
				{
					newItem = listViewWeather.Items[i];
					strCity=newItem.Text;
					strCode=newItem.SubItems[1].Text;
				}
				xmlWriter.SetValue("weather",strCityTag, strCity);
				xmlWriter.SetValue("weather",strCodeTag, strCode);
			}
			if (radioCelsius.Checked)
				xmlWriter.SetValue("weather","temperature","C");
			else
				xmlWriter.SetValue("weather","temperature","F");

			if (radioWindSpeedKH.Checked)
				xmlWriter.SetValue("weather","speed","K");
			else if (radioWindSpeedMPH.Checked)
				xmlWriter.SetValue("weather","speed","M");
			else 
				xmlWriter.SetValue("weather","speed","S");

			xmlWriter.SetValue("weather","refresh", cntrlweatherRefresh.Value.ToString());

		}
 
    void SaveShares(AMS.Profile.Xml xmlWriter, ListView view, string strTag)
    {
      for (int i=0; i < 20; ++i)
      {
        string strShareName=String.Format("sharename{0}",i);
        string strSharePath=String.Format("sharepath{0}",i);
        string strName="";
        string strPath="";
        if (i < view.Items.Count)
        {
          ListViewItem item=view.Items[i];
          strName=item.Text;
          strPath=item.SubItems[1].Text;
        }
        xmlWriter.SetValue(strTag,strShareName,strName);
        xmlWriter.SetValue(strTag,strSharePath,strPath);
      }
    }
    void SaveSettings()
    {
      AMS.Profile.Xml   xmlWriter=new AMS.Profile.Xml("MediaPortal.xml");
			
			xmlWriter.SetValueAsBool("general","startfullscreen",checkStartFullScreen.Checked);
      xmlWriter.SetValueAsBool("general","autohidemouse", checkBoxAutoHideMouse.Checked);
      
      xmlWriter.SetValue("dvdplayer","path",dvdFile.Text);
      xmlWriter.SetValue("dvdplayer","arguments",dvdParams.Text);
      xmlWriter.SetValueAsBool("dvdplayer","internal",checkBoxInternalDVDPlayer.Checked);
      
      xmlWriter.SetValue("dvdplayer","audiolanguage",(string)comboBoxAudioLanguage.SelectedItem);
      xmlWriter.SetValue("dvdplayer","subtitlelanguage",(string)comboBoxSubtitleLanguage.SelectedItem);

      xmlWriter.SetValueAsBool("dvdplayer","showsubtitles",checkBoxDVDSubtitles.Checked);
      xmlWriter.SetValue("movieplayer","path",movieFile.Text);
      xmlWriter.SetValue("movieplayer","arguments",movieParameters.Text);

      xmlWriter.SetValueAsBool("movieplayer","internal",checkBoxMovieInternalPlayer.Checked );
			xmlWriter.SetValue("movieplayer","osdtimeout",trackBarOSDTimeout.Value.ToString());

			xmlWriter.SetValue("audioplayer","player", (string)comboAudioPlayer.SelectedItem);
			


      SaveShares(xmlWriter, listAudioShares,"music");
      SaveShares(xmlWriter, listPictureShares,"pictures");
      SaveShares(xmlWriter, listVideoShares,"movies");
      
      xmlWriter.SetValue("pictures","speed",UpDownPictureDuration.Value.ToString());
      xmlWriter.SetValue("pictures","transisition",UpDownPictureTransition.Value.ToString());
      xmlWriter.SetValueAsBool("musicfiles","showid3",chkMusicID3.Checked);
      xmlWriter.SetValueAsBool("musicfiles","repeat",chkBoxRepeatAudioPlaylist.Checked);
      xmlWriter.SetValueAsBool("movies","repeat",chkBoxVideoRepeat.Checked);
      xmlWriter.SetValueAsBool("musicfiles","autoshuffle",checkBoxShufflePlaylists.Checked);
			
      xmlWriter.SetValue("music","extensions", txtboxAudioFiles.Text);
      xmlWriter.SetValue("movies","extensions", txtboxVideoFiles.Text);
			xmlWriter.SetValue("pictures","extensions",txtBoxPictureFiles.Text);

			SaveWeather(xmlWriter);

      SaveSubtitles(xmlWriter);
      SaveCapture(xmlWriter);
			
			xmlWriter.SetValue("skin","language",comboBoxLanguage.SelectedItem);
			xmlWriter.SetValue("skin","name",comboBoxSkins.SelectedItem);
			SaveFrequencies();
		}


    private void SetupForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      SaveSettings();
    }

    private void btnAddAudioShare_Click(object sender, System.EventArgs e)
    {
      AddShare(listAudioShares);
    }

    private void btnDelAudioShare_Click(object sender, System.EventArgs e)
    {
      DelShare(listAudioShares);
    }

    private void btnAddVideoShare_Click(object sender, System.EventArgs e)
    {
      AddShare(listVideoShares);    
    }

    private void btnDelVideoShare_Click(object sender, System.EventArgs e)
    {
      DelShare(listVideoShares);
    }

    private void btnAddPictureShare_Click(object sender, System.EventArgs e)
    {
      AddShare(listPictureShares);    
    }

    private void btnDelPictureShare_Click(object sender, System.EventArgs e)
    {
      DelShare(listPictureShares);
    }

    void DelShare(ListView view)
    {
      if (view.SelectedItems.Count==0) return;
      int iItem=view.SelectedIndices[0];
      DialogResult result=MessageBox.Show(this.Parent,"Are you sure to delete this share?", "Delete Share",MessageBoxButtons.YesNo);
      if (result==DialogResult.Yes)
      {
        view.Items.RemoveAt(iItem);
      }
    }

    void AddShare(ListView view)
    {
      formNewShare dlg=new formNewShare();
      dlg.ShowDialog(this.Parent);
      if (dlg.SelectedPath==null) return;

      ListViewItem newItem;
      newItem=view.Items.Add(dlg.ShareName);
      newItem.SubItems.Add(dlg.SelectedPath);

    }

		private void btnWeatherAddCity_Click(object sender, System.EventArgs e)
		{
			SearchCity form=new SearchCity();
			form.ShowDialog(this.Parent);
			if (form.City.Length>0 && form.ShortCode.Length>0)
			{
				ListViewItem newItem=listViewWeather.Items.Add(form.City);
				newItem.SubItems.Add(form.ShortCode);
			}
		}

		private void btnWeatherDel_Click(object sender, System.EventArgs e)
		{
			if (listViewWeather.SelectedItems.Count==0) return;
			int iItem=listViewWeather.SelectedIndices[0];
			DialogResult result=MessageBox.Show(this.Parent,"Are you sure to delete this city?", "Delete City",MessageBoxButtons.YesNo);
			if (result==DialogResult.Yes)
			{
				listViewWeather.Items.RemoveAt(iItem);
			}		
		}

    private void btnEditMusicShare_Click(object sender, System.EventArgs e)
    {
      EditShare(listAudioShares);
    }

    private void btnEditPictureShare_Click(object sender, System.EventArgs e)
    {
      EditShare(listPictureShares);
    }

    private void btnEditMovieShare_Click(object sender, System.EventArgs e)
    {
      EditShare(listVideoShares);
    }

    void EditShare(ListView view)
    {
      if (view.SelectedItems.Count==0) return;
			int iItem=view.SelectedIndices[0];
      ListViewItem item=view.Items[iItem];
      formNewShare share=new formNewShare();
      share.ShareName=item.SubItems[0].Text;
      share.SelectedPath=item.SubItems[1].Text;
      share.ShowDialog(this);
      if (share.SelectedPath==null) return;
      item.SubItems[0].Text=share.ShareName;
      item.SubItems[1].Text=share.SelectedPath;
    }

    private void listAudioShares_DoubleClick(object sender, System.EventArgs e)
    {
      EditShare(listAudioShares);
    }

    private void listPictureShares_DoubleClick(object sender, System.EventArgs e)
    {
      EditShare(listPictureShares);
    }

    private void listVideoShares_DoubleClick(object sender, System.EventArgs e)
    {
      EditShare(listVideoShares);
    }

    private void checkBoxMovieInternalPlayer_CheckedChanged(object sender, System.EventArgs e)
    {
      if (checkBoxMovieInternalPlayer.Checked)
      {
        movieFile.Enabled=false;
        movieParameters.Enabled=false;
				trackBarOSDTimeout.Enabled=true;
				checkBoxShowSubtitles.Enabled=true;
				txtBoxSubFont.Enabled=true;
				btnChooseSubFont.Enabled=true;
				numericUpDownSubShadow.Enabled=true;

      }
      else
      {
        movieFile.Enabled=true;
				movieParameters.Enabled=true;
				trackBarOSDTimeout.Enabled=false;
				checkBoxShowSubtitles.Enabled=false;
				txtBoxSubFont.Enabled=false;
				btnChooseSubFont.Enabled=false;
				numericUpDownSubShadow.Enabled=false;
			}
    }

    private void btnChooseSubFont_Click(object sender, System.EventArgs e)
    {

      AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml");
      FontDialog dlg = new FontDialog();
      string strFontName=xmlreader.GetValueAsString("subtitles","fontface","Arial");
      int iFontSize=xmlreader.GetValueAsInt("subtitles","fontsize",18);
      bool bBold=xmlreader.GetValueAsBool("subtitles","bold",true);
      int iDropShadow=xmlreader.GetValueAsInt("subtitles","shadow",5);

      FontStyle style=FontStyle.Regular;
      if (bBold)  style=FontStyle.Bold;
      dlg.Font = new Font(strFontName,iFontSize,style);
      
      dlg.Color = txtBoxSubFont.BackColor;
      dlg.ShowColor=true;
      dlg.ShowDialog();
      bBold=dlg.Font.Bold;
      strFontName=dlg.Font.Name;
      int iHeight=(int)dlg.Font.Size;
      txtBoxSubFont.Text=String.Format("{0} {1}", strFontName,iHeight);
      if (bBold) txtBoxSubFont.Text += ", Bold";
      txtBoxSubFont.BackColor=dlg.Color;
      
      xmlreader.SetValue("subtitles", "fontface", strFontName);
      xmlreader.SetValue("subtitles", "fontsize", iHeight.ToString());
      xmlreader.SetValueAsBool("subtitles", "bold", bBold);
      
      string strColor=String.Format("{0:X}", dlg.Color.ToArgb());
      xmlreader.SetValue("subtitles","color",strColor);
    }

    private void numericUpDownSubShadow_ValueChanged(object sender, System.EventArgs e)
    {
      
    }

    private void checkBoxShowSubtitles_CheckedChanged(object sender, System.EventArgs e)
    {
      if ( checkBoxShowSubtitles.Checked)
      {
        txtBoxSubFont.Enabled=true;
        btnChooseSubFont.Enabled=true;
        numericUpDownSubShadow.Enabled=true;
      }
      else
      {
        txtBoxSubFont.Enabled=false;
        btnChooseSubFont.Enabled=false;
        numericUpDownSubShadow.Enabled=false;
      }
    }

    private void SetupForm_Load(object sender, System.EventArgs e)
    {
      Cursor.Show();
    }

		private void trackBarOSDTimeout_ValueChanged(object sender, System.EventArgs e)
		{
			if (trackBarOSDTimeout.Value==0)
				labelOSDTimeout.Text="none";
			else
				labelOSDTimeout.Text=trackBarOSDTimeout.Value.ToString() + " sec.";
		}

		int GetInt(string strLine)
		{
			try
			{
				return System.Int32.Parse(strLine);
			}
			catch (Exception)
			{
				return 0;
			}
		}

    void SaveCapture(AMS.Profile.Xml xmlWriter)
    {
      xmlWriter.SetValue("capture","videodevice", (string)comboVideoDevice.SelectedItem);
      xmlWriter.SetValue("capture","audiodevice", (string)comboAudioDevice.SelectedItem);
      xmlWriter.SetValue("capture","audiocompressor",(string)comboCompressorAudio.SelectedItem);
      xmlWriter.SetValue("capture","videocompressor", (string)comboCompressorVideo.SelectedItem);
      xmlWriter.SetValue("capture","recordingpath",textBoxRecPath.Text);
      xmlWriter.SetValue("capture","format",comboBoxCaptureFormat.SelectedItem);

      if (btnradioAntenna.Checked)
        xmlWriter.SetValue("capture","tuner", "Antenna");
      else
        xmlWriter.SetValue("capture","tuner", "Cable");
      
      xmlWriter.SetValue("capture","country",upDownCountry.Value.ToString());

      
      for (int i=0; i < listTVChannels.Items.Count;++i)
      {
        try
        {
          string strChannel=listTVChannels.Items[i].Text;
          string strNumber=listTVChannels.Items[i].SubItems[1].Text;
          string strFreq=listTVChannels.Items[i].SubItems[2].Text;
          int iNumber=Int32.Parse(strNumber);
          if (iNumber <254)
          {
            TVDatabase.SetChannelNumber(strChannel,iNumber);
            TVDatabase.SetChannelFrequency(strChannel,strFreq);
            TVDatabase.SetChannelSort(strChannel,i);
          }
        }
        catch(Exception)
        {
        }
      }
    }

    void SetupCapture()
    {
      AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml");
      string strRecPath=xmlreader.GetValueAsString("capture","recordingpath","");
      string strVideoDevice=xmlreader.GetValueAsString("capture","videodevice","none");
      string strAudioDevice=xmlreader.GetValueAsString("capture","audiodevice","none");
      string strCompressorAudio=xmlreader.GetValueAsString("capture","audiocompressor","none");
      string strCompressorVideo=xmlreader.GetValueAsString("capture","videocompressor","none");
      string strCaptureFormat=xmlreader.GetValueAsString("capture","format",".avi");
      string strTunerType=xmlreader.GetValueAsString("capture","tuner","Antenna");
      upDownCountry.Value=xmlreader.GetValueAsInt("capture","country",31);
      if (strRecPath=="") 
      {
        strRecPath=String.Format(@"{0}\My Movies",Environment.GetFolderPath( Environment.SpecialFolder.Personal).ToString());
      }
      if (strTunerType=="Antenna") btnradioAntenna.Checked=true;
      else btnradioCable.Checked=true;

      for (int x=0; x < comboBoxCaptureFormat.Items.Count;++x)
      {
        if ( ((string)comboBoxCaptureFormat.Items[x]) ==strCaptureFormat)
        {
          comboBoxCaptureFormat.SelectedIndex=x;
          break;
        }
      }
      textBoxRecPath.Text=strRecPath;
			int index=0;

      Filters filters = new Filters();
      // video capture devices
      comboVideoDevice.Items.Clear();
      comboVideoDevice.Items.Add("none");
      int i=1;
      foreach (Filter filter in filters.VideoInputDevices)
      {
        comboVideoDevice.Items.Add(filter.Name);
        if (String.Compare(filter.Name,strVideoDevice,true)==0) index=i;
        ++i;
      }
      comboVideoDevice.SelectedIndex=index;

      // audio capture devices
      comboAudioDevice.Items.Clear();
      comboAudioDevice.Items.Add("none");
      i=1;
      index=0;
      foreach (Filter filter in filters.AudioInputDevices)
      {
        comboAudioDevice.Items.Add(filter.Name);
        if (String.Compare(filter.Name,strAudioDevice,true)==0) index=i;
        ++i;
      }
      comboAudioDevice.SelectedIndex=index;

      // audio compressors
      comboCompressorAudio.Items.Clear();
      comboCompressorAudio.Items.Add("none");
      i=1;
      index=0;
      foreach (Filter filter in filters.AudioCompressors)
      {
        comboCompressorAudio.Items.Add(filter.Name);
        if (String.Compare(filter.Name,strCompressorAudio,true)==0) index=i;
        ++i;
      }
      comboCompressorAudio.SelectedIndex=index;

      // Video compressors
      comboCompressorVideo.Items.Clear();
      comboCompressorVideo.Items.Add("none");
      i=1;
      index=0;
      foreach (Filter filter in filters.VideoCompressors)
      {
        comboCompressorVideo.Items.Add(filter.Name);
        if (String.Compare(filter.Name,strCompressorVideo,true)==0) index=i;
        ++i;
      }
      comboCompressorVideo.SelectedIndex=index;

      // setup tv channel list
      listTVChannels.Items.Clear();
      
      ListViewItem newItem;
      
      ArrayList channels = new ArrayList();
      TVDatabase.GetChannels(ref channels);
      foreach (TVChannel chan in channels)
      {
        newItem = listTVChannels.Items.Add(chan.Name);
        newItem.SubItems.Add(chan.Number.ToString());
        newItem.SubItems.Add(chan.Frequency.ToString());
      }
      SetupPropertyPageList();
    }

    private void listTVChannels_DoubleClick(object sender, System.EventArgs e)
    {
      if (listTVChannels.SelectedItems.Count==0) return;
      int iItem=listTVChannels.SelectedIndices[0];
      ListViewItem item=listTVChannels.Items[iItem];
      try
      {
        int iChannel=Int32.Parse(item.SubItems[1].Text);
        if (iChannel<254)
        {
          TVChannelForm dlg= new TVChannelForm();
          dlg.Channel = (string)item.SubItems[0].Text;
          dlg.Number=iChannel;
          dlg.Frequency = (string)item.SubItems[2].Text;
          dlg.ShowDialog();
          if (dlg.Number>=0)
          {
            item.SubItems[1].Text=String.Format("{0}",dlg.Number);
            item.SubItems[2].Text=String.Format("{0}",dlg.Frequency);
          }
        }
      }
      catch (Exception)
      {
      }
    
    }

    private void btnRecPath_Click(object sender, System.EventArgs e)
    {
      FolderBrowserDialog dlg=new FolderBrowserDialog();
      dlg.ShowNewFolderButton=true;
      dlg.ShowDialog(this);
      if (dlg.SelectedPath==null) return;
      textBoxRecPath.Text=dlg.SelectedPath;
    
    }

    private void checkBoxInternalDVDPlayer_CheckedChanged(object sender, System.EventArgs e)
    {
      if (checkBoxInternalDVDPlayer.Checked)
      {
        dvdFile.Enabled=false;
        dvdbtnSelect.Enabled=false;
        dvdParams.Enabled=false;
        comboBoxAudioLanguage.Enabled=true;
        comboBoxSubtitleLanguage.Enabled=true;
      }
      else
      {
        dvdFile.Enabled=true;
        dvdbtnSelect.Enabled=true;
        dvdParams.Enabled=true;
        comboBoxAudioLanguage.Enabled=false;
        comboBoxSubtitleLanguage.Enabled=false;
      }
    }

    private Capture setupgraph()
    {
      string strVideoDevice=(string )comboVideoDevice.SelectedItem;
      string strAudioDevice=(string )comboAudioDevice.SelectedItem;
      string strCompressorAudio=(string)comboCompressorAudio.SelectedItem;
      string strCompressorVideo=(string)comboCompressorVideo.SelectedItem;

      
      DirectX.Capture.Filter videoDevice=null;
      DirectX.Capture.Filter audioDevice=null;
      Filters filters=new Filters();
      // find video capture device
      foreach (Filter filter in filters.VideoInputDevices)
      {
        if (String.Compare(filter.Name,strVideoDevice)==0)
        {
          videoDevice=filter;
          break;
        }
      }
      // find audio capture device
      foreach (Filter filter in filters.AudioInputDevices)
      {
        if (String.Compare(filter.Name,strAudioDevice)==0)
        {
          audioDevice=filter;
          break;
        }
      }
      
    
      // create new capture!
      Capture capture=null;
      try
      {
        capture = new Capture(videoDevice,audioDevice);
      }
      catch(Exception)
      {
        return null;
      }

      try
      {
        // add audio compressor
        foreach (Filter filter in filters.AudioCompressors)
        {
          if (String.Compare(filter.Name,strCompressorAudio)==0)
          {
            capture.AudioCompressor=filter;
            break;
          }
        }
      }
      catch (Exception)
      {
      }

      try
      {
        //add video compressor
        foreach (Filter filter in filters.VideoCompressors)
        {
          if (String.Compare(filter.Name,strCompressorVideo)==0)
          {
            capture.VideoCompressor=filter;
            break;
          }
        }

      }
      catch (Exception)
      {
      }
      return capture;
    }




    void AddLanguages(ComboBox box ,string strDefault)
    {
      int iIndex=0;
      int iSelected=0;
      box.Items.Clear();
			foreach ( CultureInfo ci in CultureInfo.GetCultures( CultureTypes.NeutralCultures) )  
      {
        box.Items.Add(ci.EnglishName);
        if (String.Compare(ci.EnglishName,strDefault,true)==0) iSelected=iIndex;
        iIndex++;
      }
      box.SelectedItem=strDefault;
    }

    private void checkBoxDVDSubtitles_CheckedChanged(object sender, System.EventArgs e)
    {
      if (checkBoxDVDSubtitles.Checked)
      {
        comboBoxSubtitleLanguage.Enabled=true;
      }
      else
      {
        comboBoxSubtitleLanguage.Enabled=false;
      }
    }

    private void btnTvChannelUp_Click(object sender, System.EventArgs e)
    {
      if (listTVChannels.SelectedItems.Count==0) return;
      int iItem=listTVChannels.SelectedIndices[0];
      if (iItem<=0) return;
      ListViewItem item1=(ListViewItem)listTVChannels.Items[iItem].Clone();
      ListViewItem item2=(ListViewItem)listTVChannels.Items[iItem-1].Clone();
    
      listTVChannels.Items[iItem-1]=item1;
      listTVChannels.Items[iItem]=item2;
      listTVChannels.Focus();
    }

    private void btnTvChannelDown_Click(object sender, System.EventArgs e)
    {

      if (listTVChannels.SelectedItems.Count==0) return;
      int iItem=listTVChannels.SelectedIndices[0];
      if (iItem+1>=listTVChannels.Items.Count) return;
      ListViewItem item1=(ListViewItem)listTVChannels.Items[iItem].Clone();
      ListViewItem item2=(ListViewItem)listTVChannels.Items[iItem+1].Clone();
    
      listTVChannels.Items[iItem+1]=item1;
      listTVChannels.Items[iItem]=item2;  
      listTVChannels.Focus();

    }

		private void listTVChannels_SubItemClicked(object sender, ListViewEx.SubItemClickEventArgs e)
		{
			//lvItem = e.Item;
			//lvSubItem = e.SubItem;
			if (e.SubItem==0) return;
			Control[] Editors = new Control[] { null, textEditBox, textEditBox };
			listTVChannels.StartEditing(Editors[e.SubItem], e.Item, e.SubItem);
		}
		private void control_SelectedValueChanged(object sender, System.EventArgs e)
		{
			listTVChannels.EndEditing(true);
		}

		void SaveFrequencies()
		{
			try
			{
				System.IO.File.Delete("tuner.xml");
			}
			catch(Exception)
			{
			}
			RegistryKey hklm =Registry.LocalMachine;
			RegistryKey hklm2 =Registry.LocalMachine;
			hklm=hklm.CreateSubKey(@"Software\Microsoft\TV System Services\TVAutoTune\TS66-1");//cable
			hklm2=hklm2.CreateSubKey(@"Software\Microsoft\TV System Services\TVAutoTune\TS66-0");//broadcast
      

			for (int i=0; i < 200; ++i)
			{
				try
				{
					hklm.DeleteValue(i.ToString());
				}
				catch(Exception)
				{
				}
				try
				{
					hklm2.DeleteValue(i.ToString());
				}
				catch(Exception)
				{
				}
			}
			for (int i=0; i < 200; ++i)
			{
				string strTagChan=String.Format("channel{0}",i);
				if (i < listTVChannels.Items.Count)
				{
					string strChan = listTVChannels.Items[i].SubItems[1].Text;
					string strFreq = listTVChannels.Items[i].SubItems[2].Text;
					try
					{
						int iChan=Int32.Parse(strChan);
						if (iChan <254)
						{
							UInt32 dwFreq = UInt32.Parse(strFreq);
							string strKey=strChan;
							hklm.SetValue(strKey,(Int32)dwFreq);
							hklm2.SetValue(strKey,(Int32)dwFreq);
						}
					}
					catch (Exception)
					{
					}
				}
			}
			hklm.Close();
		}

    private void btnDelChannel_Click(object sender, System.EventArgs e)
    {
      if (listTVChannels.SelectedItems.Count==0) return;
      int iItem=listTVChannels.SelectedIndices[0];
      DialogResult result=MessageBox.Show(this.Parent,"Are you sure to delete this channel?", "Delete channel",MessageBoxButtons.YesNo);
      if (result==DialogResult.Yes)
      {
        string strChannel=listTVChannels.Items[iItem].Text;
        listTVChannels.Items.RemoveAt(iItem);
        
        TVDatabase.RemoveChannel(strChannel);
      }
    
    }

    private void btnNewChannel_Click(object sender, System.EventArgs e)
    {
      TVChannelFormNew dlg= new TVChannelFormNew();
      dlg.Channel = "New channel";
      dlg.Number=0;
      dlg.Frequency = "0";
      dlg.ShowDialog();
      if (dlg.Number>=0)
      {
        TVChannel chan= new TVChannel();
        chan.Number=dlg.Number;
        chan.Name=dlg.Channel;
        try
        {
          chan.Frequency=Int64.Parse(dlg.Frequency);
        }
        catch(Exception)
        {
        }
        
        TVDatabase.AddChannel(chan);

        ListViewItem newItem = listTVChannels.Items.Add(chan.Name);
        newItem.SubItems.Add(chan.Number.ToString());
        newItem.SubItems.Add(chan.Frequency.ToString());

      }    
    }

    private void btnEditChannel_Click(object sender, System.EventArgs e)
    {
      listTVChannels_DoubleClick(null,null);
    }

    private void SetupPropertyPageList()
    {
      listPropertyPages.Items.Clear();
      Capture cap=setupgraph();
      if (cap==null) return;
      foreach (PropertyPage page in cap.PropertyPages)
      {
        listPropertyPages.Items.Add( page.Name);
      }
      cap.Stop();
      cap.Dispose();
      cap=null;
    }
    private void listPropertyPages_DoubleClick(object sender, System.EventArgs e)
    {
      if (listPropertyPages.SelectedItems.Count==0) return;
      int iItem=listPropertyPages.SelectedIndices[0];
      int i=0;
      Capture cap=setupgraph();
      if (cap==null) return;
      foreach (PropertyPage page in cap.PropertyPages)
      {
        if (i==iItem)
        {
          page.Show(this);
          break;
        }
        i++;
      }
      cap.Stop();
      cap.Dispose();
      cap=null;
    }

    private void comboVideoDevice_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      SetupPropertyPageList();
    }

    private void comboAudioDevice_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      SetupPropertyPageList();
    }

    private void comboCompressorVideo_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      SetupPropertyPageList();
    }

    private void comboCompressorAudio_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      SetupPropertyPageList();
    }
	}
}
