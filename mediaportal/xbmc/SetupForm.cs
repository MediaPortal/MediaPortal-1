using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Globalization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using Microsoft.Win32;

using DirectX.Capture;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;
using MediaPortal.WinControls;
using MediaPortal.TV.Recording;
using MediaPortal.Util;

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
    private ListViewEx listAudioShares;
    private System.Windows.Forms.ColumnHeader HdrAudioFolder;
    private System.Windows.Forms.ColumnHeader HdrAudioName;
    private System.Windows.Forms.TabPage tabAudioShares;
    private System.Windows.Forms.Button btnAddAudioShare;
    private System.Windows.Forms.Button btnDelAudioShare;

    private System.Windows.Forms.GroupBox VideoGroupBox;
    private ListViewEx listVideoShares;
    private System.Windows.Forms.ColumnHeader HdrVideoName;
    private System.Windows.Forms.ColumnHeader HdrVideoFolder;
    private System.Windows.Forms.TabPage tabVideoShares;
    private System.Windows.Forms.Button btnAddVideoShare;
    private System.Windows.Forms.Button btnDelVideoShare;

    private System.Windows.Forms.GroupBox PictureGroupBox;
    private ListViewEx listPictureShares;
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
    private ListViewEx listTVChannels;
    private System.Windows.Forms.ColumnHeader columnHeader1;
    private System.Windows.Forms.ColumnHeader columnHeader2;
    private System.Windows.Forms.Button btnRecPath;
    private System.Windows.Forms.TextBox textBoxRecPath;
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
		private System.Windows.Forms.NumericUpDown UpDownPreRecording;
		private System.Windows.Forms.NumericUpDown UpDownPostRecording;
		private System.Windows.Forms.GroupBox groupBox17;
		private System.Windows.Forms.Label label21;
		private System.Windows.Forms.Label label31;
		private System.Windows.Forms.Label label32;
		private System.Windows.Forms.Label label33;
		private System.Windows.Forms.Label label34;
		private System.Windows.Forms.Label label35;
    bool  m_bAscending= true;
    int   iColumn=-1;
    private System.Windows.Forms.GroupBox groupBox18;
    private System.Windows.Forms.GroupBox groupBox19;
    private System.Windows.Forms.CheckBox xmltvTimeZoneCheck;
    private System.Windows.Forms.Label label36;
    private System.Windows.Forms.Label label37;
    private System.Windows.Forms.NumericUpDown timeZoneCorrection;
    private System.Windows.Forms.ColumnHeader HdrDefault;
    private System.Windows.Forms.ColumnHeader columnHeader5;
    private System.Windows.Forms.ColumnHeader columnHeader6;
    private System.Windows.Forms.GroupBox groupBox20;
    private System.Windows.Forms.TextBox textBoxXMLTVFolder;
    private System.Windows.Forms.Button btnXMLTVFolder;
    private System.Windows.Forms.Label label39;
    private System.Windows.Forms.ComboBox comboMovieAudioRenderer;
    private System.Windows.Forms.ToolTip toolTip1;
    private System.Windows.Forms.Button btnEditCaptureDevice;
    private System.Windows.Forms.Button btnDelCaptureDevice;
    private System.Windows.Forms.Button btnAddCaptureDevice;
    private ListViewEx listCaptureCards;
    private System.Windows.Forms.ColumnHeader columnHeader7;
    private System.Windows.Forms.ColumnHeader columnHeader8;
    private System.Windows.Forms.ColumnHeader columnHeader9;
    private System.ComponentModel.IContainer components;
    private System.Windows.Forms.Label label17;
    private System.Windows.Forms.TextBox textBoxPlayLists;
    private System.Windows.Forms.Button btnPlayListFolder;
    private System.Windows.Forms.Label label18;
    private System.Windows.Forms.TextBox textBoxPlayListFolderVideo;
    private System.Windows.Forms.Button btnPlayListVideo;
    private System.Windows.Forms.CheckBox checkBoxARDVD;
    private System.Windows.Forms.ComboBox comboDVDNavigator;
    private System.Windows.Forms.Label label40;
    private System.Windows.Forms.ComboBox comboDVDAudioRenderer;
    private System.Windows.Forms.Label label38;
    private System.Windows.Forms.CheckBox checkBoxDVDSubtitles;
    private System.Windows.Forms.ComboBox comboBoxSubtitleLanguage;
    private System.Windows.Forms.ComboBox comboBoxAudioLanguage;
    private System.Windows.Forms.Label label29;
    private System.Windows.Forms.Label label28;
    private System.Windows.Forms.CheckBox checkBoxInternalDVDPlayer;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox dvdFile;
    private System.Windows.Forms.Button dvdbtnSelect;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox dvdParams;
    private System.Windows.Forms.GroupBox DVDPlayerBox;
    private System.Windows.Forms.Label label19;
    private System.Windows.Forms.ComboBox comboBoxDVDARCorrectionMode;
    private System.Windows.Forms.GroupBox groupBox10;
    private System.Windows.Forms.GroupBox groupBox11;
    private System.Windows.Forms.GroupBox groupBox12;
    private System.Windows.Forms.Label label20;
    private System.Windows.Forms.ComboBox comboBoxDVDDisplayMode;
    private System.Windows.Forms.CheckBox checkBoxMouseSupport;
    private System.Windows.Forms.CheckBox checkBoxHideFileExtensions;
    private System.Windows.Forms.Button buttonAutoTune;
    private System.Windows.Forms.Button buttonAutoTune2;
    private System.Windows.Forms.PictureBox pictureBox1;
    private System.Windows.Forms.CheckBox checkBoxTVGuideColors;
    ArrayList m_tvcards = new ArrayList();

		public SetupForm()
		{
      Log.Write("------------enter setup--------");
      Recorder.Stop();
      InitializeComponent();

			LoadSettings();
      SetupCapture();
			listTVChannels.SubItemClicked += new ListViewEx.SubItemClickEventHandler(listTVChannels_SubItemClicked);
      listAudioShares.SubItemClicked += new ListViewEx.SubItemClickEventHandler(listAudioShares_SubItemClicked);
      listVideoShares.SubItemClicked += new ListViewEx.SubItemClickEventHandler(listVideoShares_SubItemClicked);
      listPictureShares.SubItemClicked += new ListViewEx.SubItemClickEventHandler(listPictureShares_SubItemClicked);
      listCaptureCards.SubItemClicked += new ListViewEx.SubItemClickEventHandler(listCaptureCards_SubItemClicked);

      
      //general
      toolTip1.SetToolTip(checkStartFullScreen ,"If you enable this then MediaPortal will start in fullscreen mode instead of windowed mode");
      toolTip1.SetToolTip(checkBoxAutoHideMouse,"If enabled then Mediaportal will automaticly hide the mouse pointer when its inactive for 3 seconds");
      toolTip1.SetToolTip(comboBoxLanguage     ,"Select which language file Mediaportal should use");
      toolTip1.SetToolTip(comboBoxSkins        ,"Select which skin Mediaportal should use");


      //movieplayer
      toolTip1.SetToolTip(movieFile                   ,"Select the filename of an external video player mediaportal should use for playing movies");
      toolTip1.SetToolTip(bntSelectMovieFile          ,"Select the filename of an external video player mediaportal should use for playing movies");
      toolTip1.SetToolTip(movieParameters             ,"Specify any extra parameters for the external video player");
      toolTip1.SetToolTip(checkBoxMovieInternalPlayer ,"Specify whether Mediaportal should use its build-in video player or an external video player");

      //movieplayer:subtitles
      toolTip1.SetToolTip(checkBoxShowSubtitles       ,"Turns on/off subtitles");
      toolTip1.SetToolTip(txtBoxSubFont               ,"Fontface, font size & color to use for displaying subtitles");
      toolTip1.SetToolTip(btnChooseSubFont            ,"Fontface, font size & color to use for displaying subtitles");
      toolTip1.SetToolTip(numericUpDownSubShadow      ,"Specify how much shadow a subtitle should have (in pixels)");

      //movieplayer:OSD
      toolTip1.SetToolTip(trackBarOSDTimeout          ,"Idle timeout in seconds before the OSD disappears");

      //movieplayer:audio renderer
      toolTip1.SetToolTip(comboMovieAudioRenderer     ,"Select audio renderer to be used for video playback. For DD/DTS you may have to try each to find out which gives the best result");


      //audioplayer:audio player
      toolTip1.SetToolTip(comboAudioPlayer            ,"Specify if mediaportal should use Windows Media Player 9 for audio playback or its default buildin audioplayer (Windows Mediaplayer 9 is recommended)");


      //DVDplayer
      toolTip1.SetToolTip(dvdFile                     ,"Select the filename of an external DVD player mediaportal should use for playing movies");
      toolTip1.SetToolTip(dvdbtnSelect                ,"Select the filename of an external DVD player mediaportal should use for playing movies");
      toolTip1.SetToolTip(dvdParams                   ,"Specify any extra parameters for the external DVD player");
      toolTip1.SetToolTip(checkBoxInternalDVDPlayer   ,"Specify whether Mediaportal should use its build-in DVD player or an external DVD player");

      //DVDplayer:audio/sub languages
      toolTip1.SetToolTip(comboBoxAudioLanguage       ,"Select default audio language for DVD playback");
      toolTip1.SetToolTip(comboBoxSubtitleLanguage    ,"Select default subtitle language for DVD playback");
      toolTip1.SetToolTip(checkBoxDVDSubtitles        ,"Turn on/off subtitles");

      //DVDplayer:audio renderer
      toolTip1.SetToolTip(comboDVDAudioRenderer       ,"Select audio renderer to be used for DVD playback. For DD/DTS you may have to try each to find out which gives the best result");
      toolTip1.SetToolTip(comboDVDNavigator           ,"Select which DVD navigator codec to use for DVD playback");
    
      //DVDplayer:AR mode
      toolTip1.SetToolTip(comboBoxDVDARCorrectionMode,"Sets the aspect ratio correction mode");
      toolTip1.SetToolTip(checkBoxARDVD              ,"Use pixel ratio correction for DVD or not");
      toolTip1.SetToolTip(comboBoxDVDDisplayMode     ,"Specify the preffered display mode");

      UpdateCaptureCardList();
      
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
      this.components = new System.ComponentModel.Container();
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(SetupForm));
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
      this.checkBoxHideFileExtensions = new System.Windows.Forms.CheckBox();
      this.checkBoxMouseSupport = new System.Windows.Forms.CheckBox();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.groupBox14 = new System.Windows.Forms.GroupBox();
      this.tabPlayers = new System.Windows.Forms.TabPage();
      this.comboMovieAudioRenderer = new System.Windows.Forms.ComboBox();
      this.label39 = new System.Windows.Forms.Label();
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
      this.tabAudioPlayer = new System.Windows.Forms.TabPage();
      this.groupBox15 = new System.Windows.Forms.GroupBox();
      this.label30 = new System.Windows.Forms.Label();
      this.comboAudioPlayer = new System.Windows.Forms.ComboBox();
      this.TabDVDPlayer = new System.Windows.Forms.TabPage();
      this.groupBox12 = new System.Windows.Forms.GroupBox();
      this.checkBoxDVDSubtitles = new System.Windows.Forms.CheckBox();
      this.comboBoxSubtitleLanguage = new System.Windows.Forms.ComboBox();
      this.comboBoxAudioLanguage = new System.Windows.Forms.ComboBox();
      this.label29 = new System.Windows.Forms.Label();
      this.label28 = new System.Windows.Forms.Label();
      this.groupBox11 = new System.Windows.Forms.GroupBox();
      this.comboDVDNavigator = new System.Windows.Forms.ComboBox();
      this.label40 = new System.Windows.Forms.Label();
      this.comboDVDAudioRenderer = new System.Windows.Forms.ComboBox();
      this.label38 = new System.Windows.Forms.Label();
      this.groupBox10 = new System.Windows.Forms.GroupBox();
      this.comboBoxDVDDisplayMode = new System.Windows.Forms.ComboBox();
      this.label20 = new System.Windows.Forms.Label();
      this.label19 = new System.Windows.Forms.Label();
      this.comboBoxDVDARCorrectionMode = new System.Windows.Forms.ComboBox();
      this.checkBoxARDVD = new System.Windows.Forms.CheckBox();
      this.DVDPlayerBox = new System.Windows.Forms.GroupBox();
      this.checkBoxInternalDVDPlayer = new System.Windows.Forms.CheckBox();
      this.label1 = new System.Windows.Forms.Label();
      this.dvdFile = new System.Windows.Forms.TextBox();
      this.dvdbtnSelect = new System.Windows.Forms.Button();
      this.label2 = new System.Windows.Forms.Label();
      this.dvdParams = new System.Windows.Forms.TextBox();
      this.tabAudioShares = new System.Windows.Forms.TabPage();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.btnPlayListFolder = new System.Windows.Forms.Button();
      this.textBoxPlayLists = new System.Windows.Forms.TextBox();
      this.label17 = new System.Windows.Forms.Label();
      this.checkBoxShufflePlaylists = new System.Windows.Forms.CheckBox();
      this.chkBoxRepeatAudioPlaylist = new System.Windows.Forms.CheckBox();
      this.txtboxAudioFiles = new System.Windows.Forms.TextBox();
      this.label8 = new System.Windows.Forms.Label();
      this.chkMusicID3 = new System.Windows.Forms.CheckBox();
      this.audioGroupBox = new System.Windows.Forms.GroupBox();
      this.btnEditMusicShare = new System.Windows.Forms.Button();
      this.btnDelAudioShare = new System.Windows.Forms.Button();
      this.btnAddAudioShare = new System.Windows.Forms.Button();
      this.listAudioShares = new MediaPortal.WinControls.ListViewEx();
      this.HdrAudioName = new System.Windows.Forms.ColumnHeader();
      this.HdrAudioFolder = new System.Windows.Forms.ColumnHeader();
      this.HdrDefault = new System.Windows.Forms.ColumnHeader();
      this.tabVideoShares = new System.Windows.Forms.TabPage();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.btnPlayListVideo = new System.Windows.Forms.Button();
      this.textBoxPlayListFolderVideo = new System.Windows.Forms.TextBox();
      this.label18 = new System.Windows.Forms.Label();
      this.chkBoxVideoRepeat = new System.Windows.Forms.CheckBox();
      this.txtboxVideoFiles = new System.Windows.Forms.TextBox();
      this.label9 = new System.Windows.Forms.Label();
      this.VideoGroupBox = new System.Windows.Forms.GroupBox();
      this.btnEditMovieShare = new System.Windows.Forms.Button();
      this.btnDelVideoShare = new System.Windows.Forms.Button();
      this.btnAddVideoShare = new System.Windows.Forms.Button();
      this.listVideoShares = new MediaPortal.WinControls.ListViewEx();
      this.HdrVideoName = new System.Windows.Forms.ColumnHeader();
      this.HdrVideoFolder = new System.Windows.Forms.ColumnHeader();
      this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
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
      this.listPictureShares = new MediaPortal.WinControls.ListViewEx();
      this.HdrPictureName = new System.Windows.Forms.ColumnHeader();
      this.HdrPictureFolder = new System.Windows.Forms.ColumnHeader();
      this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
      this.tabTVChannels = new System.Windows.Forms.TabPage();
      this.groupBox20 = new System.Windows.Forms.GroupBox();
      this.btnXMLTVFolder = new System.Windows.Forms.Button();
      this.textBoxXMLTVFolder = new System.Windows.Forms.TextBox();
      this.groupBox19 = new System.Windows.Forms.GroupBox();
      this.checkBoxTVGuideColors = new System.Windows.Forms.CheckBox();
      this.label37 = new System.Windows.Forms.Label();
      this.label36 = new System.Windows.Forms.Label();
      this.timeZoneCorrection = new System.Windows.Forms.NumericUpDown();
      this.xmltvTimeZoneCheck = new System.Windows.Forms.CheckBox();
      this.btnEditChannel = new System.Windows.Forms.Button();
      this.btnDelChannel = new System.Windows.Forms.Button();
      this.btnNewChannel = new System.Windows.Forms.Button();
      this.btnTvChannelDown = new System.Windows.Forms.Button();
      this.btnTvChannelUp = new System.Windows.Forms.Button();
      this.listTVChannels = new MediaPortal.WinControls.ListViewEx();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
      this.groupBox18 = new System.Windows.Forms.GroupBox();
      this.buttonAutoTune2 = new System.Windows.Forms.Button();
      this.textEditBox = new System.Windows.Forms.TextBox();
      this.tabPageCapture = new System.Windows.Forms.TabPage();
      this.btnEditCaptureDevice = new System.Windows.Forms.Button();
      this.btnDelCaptureDevice = new System.Windows.Forms.Button();
      this.btnAddCaptureDevice = new System.Windows.Forms.Button();
      this.listCaptureCards = new MediaPortal.WinControls.ListViewEx();
      this.columnHeader7 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader8 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader9 = new System.Windows.Forms.ColumnHeader();
      this.UpDownPostRecording = new System.Windows.Forms.NumericUpDown();
      this.groupBox13 = new System.Windows.Forms.GroupBox();
      this.buttonAutoTune = new System.Windows.Forms.Button();
      this.upDownCountry = new System.Windows.Forms.NumericUpDown();
      this.label24 = new System.Windows.Forms.Label();
      this.label23 = new System.Windows.Forms.Label();
      this.btnradioCable = new System.Windows.Forms.RadioButton();
      this.btnradioAntenna = new System.Windows.Forms.RadioButton();
      this.btnRecPath = new System.Windows.Forms.Button();
      this.textBoxRecPath = new System.Windows.Forms.TextBox();
      this.groupBox16 = new System.Windows.Forms.GroupBox();
      this.groupBox17 = new System.Windows.Forms.GroupBox();
      this.label35 = new System.Windows.Forms.Label();
      this.label34 = new System.Windows.Forms.Label();
      this.label33 = new System.Windows.Forms.Label();
      this.label32 = new System.Windows.Forms.Label();
      this.label31 = new System.Windows.Forms.Label();
      this.label21 = new System.Windows.Forms.Label();
      this.UpDownPreRecording = new System.Windows.Forms.NumericUpDown();
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
      this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
      this.tabControl.SuspendLayout();
      this.tabGeneral.SuspendLayout();
      this.Skin.SuspendLayout();
      this.tabPlayers.SuspendLayout();
      this.groupBox9.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarOSDTimeout)).BeginInit();
      this.groupBox8.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSubShadow)).BeginInit();
      this.MoviePlayerBox.SuspendLayout();
      this.tabAudioPlayer.SuspendLayout();
      this.groupBox15.SuspendLayout();
      this.TabDVDPlayer.SuspendLayout();
      this.groupBox12.SuspendLayout();
      this.groupBox11.SuspendLayout();
      this.groupBox10.SuspendLayout();
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
      this.tabTVChannels.SuspendLayout();
      this.groupBox20.SuspendLayout();
      this.groupBox19.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.timeZoneCorrection)).BeginInit();
      this.groupBox18.SuspendLayout();
      this.tabPageCapture.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.UpDownPostRecording)).BeginInit();
      this.groupBox13.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.upDownCountry)).BeginInit();
      this.groupBox17.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.UpDownPreRecording)).BeginInit();
      this.tabWeather.SuspendLayout();
      this.groupBox7.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.cntrlweatherRefresh)).BeginInit();
      this.groupBox6.SuspendLayout();
      this.groupBox5.SuspendLayout();
      this.groupBox4.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabControl
      // 
      this.tabControl.Controls.Add(this.tabGeneral);
      this.tabControl.Controls.Add(this.tabPlayers);
      this.tabControl.Controls.Add(this.tabAudioPlayer);
      this.tabControl.Controls.Add(this.TabDVDPlayer);
      this.tabControl.Controls.Add(this.tabAudioShares);
      this.tabControl.Controls.Add(this.tabVideoShares);
      this.tabControl.Controls.Add(this.tabPictureShares);
      this.tabControl.Controls.Add(this.tabTVChannels);
      this.tabControl.Controls.Add(this.tabPageCapture);
      this.tabControl.Controls.Add(this.tabWeather);
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
      this.label27.Location = new System.Drawing.Point(24, 296);
      this.label27.Name = "label27";
      this.label27.Size = new System.Drawing.Size(208, 16);
      this.label27.TabIndex = 12;
      this.label27.Text = "IRC: EFNet #MediaPortal";
      // 
      // linkLabel2
      // 
      this.linkLabel2.Location = new System.Drawing.Point(104, 264);
      this.linkLabel2.Name = "linkLabel2";
      this.linkLabel2.Size = new System.Drawing.Size(208, 16);
      this.linkLabel2.TabIndex = 7;
      this.linkLabel2.TabStop = true;
      this.linkLabel2.Text = "http://dott.lir.dk/mediaportal/forum";
      // 
      // label26
      // 
      this.label26.Location = new System.Drawing.Point(24, 264);
      this.label26.Name = "label26";
      this.label26.Size = new System.Drawing.Size(72, 16);
      this.label26.TabIndex = 10;
      this.label26.Text = "Forums:";
      // 
      // linkLabel1
      // 
      this.linkLabel1.Location = new System.Drawing.Point(104, 240);
      this.linkLabel1.Name = "linkLabel1";
      this.linkLabel1.Size = new System.Drawing.Size(224, 16);
      this.linkLabel1.TabIndex = 6;
      this.linkLabel1.TabStop = true;
      this.linkLabel1.Text = "http://mediaportal.sourceforge.net";
      // 
      // label25
      // 
      this.label25.Location = new System.Drawing.Point(24, 240);
      this.label25.Name = "label25";
      this.label25.Size = new System.Drawing.Size(72, 16);
      this.label25.TabIndex = 8;
      this.label25.Text = "Website";
      // 
      // checkBoxAutoHideMouse
      // 
      this.checkBoxAutoHideMouse.Checked = true;
      this.checkBoxAutoHideMouse.CheckState = System.Windows.Forms.CheckState.Checked;
      this.checkBoxAutoHideMouse.Location = new System.Drawing.Point(32, 48);
      this.checkBoxAutoHideMouse.Name = "checkBoxAutoHideMouse";
      this.checkBoxAutoHideMouse.Size = new System.Drawing.Size(112, 16);
      this.checkBoxAutoHideMouse.TabIndex = 1;
      this.checkBoxAutoHideMouse.Text = "Auto hide mouse";
      // 
      // comboBoxSkins
      // 
      this.comboBoxSkins.Location = new System.Drawing.Point(96, 152);
      this.comboBoxSkins.Name = "comboBoxSkins";
      this.comboBoxSkins.Size = new System.Drawing.Size(168, 21);
      this.comboBoxSkins.TabIndex = 3;
      // 
      // label13
      // 
      this.label13.Location = new System.Drawing.Point(32, 160);
      this.label13.Name = "label13";
      this.label13.Size = new System.Drawing.Size(48, 16);
      this.label13.TabIndex = 3;
      this.label13.Text = "Skin:";
      // 
      // label12
      // 
      this.label12.Location = new System.Drawing.Point(32, 128);
      this.label12.Name = "label12";
      this.label12.Size = new System.Drawing.Size(64, 16);
      this.label12.TabIndex = 2;
      this.label12.Text = "Language:";
      // 
      // comboBoxLanguage
      // 
      this.comboBoxLanguage.Location = new System.Drawing.Point(96, 120);
      this.comboBoxLanguage.Name = "comboBoxLanguage";
      this.comboBoxLanguage.Size = new System.Drawing.Size(168, 21);
      this.comboBoxLanguage.TabIndex = 2;
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
      this.Skin.Controls.Add(this.checkBoxHideFileExtensions);
      this.Skin.Controls.Add(this.checkBoxMouseSupport);
      this.Skin.Controls.Add(this.pictureBox1);
      this.Skin.Location = new System.Drawing.Point(16, 8);
      this.Skin.Name = "Skin";
      this.Skin.Size = new System.Drawing.Size(592, 184);
      this.Skin.TabIndex = 0;
      this.Skin.TabStop = false;
      this.Skin.Text = "Skin";
      // 
      // checkBoxHideFileExtensions
      // 
      this.checkBoxHideFileExtensions.Location = new System.Drawing.Point(16, 80);
      this.checkBoxHideFileExtensions.Name = "checkBoxHideFileExtensions";
      this.checkBoxHideFileExtensions.Size = new System.Drawing.Size(232, 16);
      this.checkBoxHideFileExtensions.TabIndex = 1;
      this.checkBoxHideFileExtensions.Text = "Hide file extensions";
      // 
      // checkBoxMouseSupport
      // 
      this.checkBoxMouseSupport.Location = new System.Drawing.Point(16, 56);
      this.checkBoxMouseSupport.Name = "checkBoxMouseSupport";
      this.checkBoxMouseSupport.Size = new System.Drawing.Size(248, 24);
      this.checkBoxMouseSupport.TabIndex = 0;
      this.checkBoxMouseSupport.Text = "Show special mouse controls like scrollbars";
      // 
      // pictureBox1
      // 
      this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
      this.pictureBox1.Location = new System.Drawing.Point(264, 16);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(320, 144);
      this.pictureBox1.TabIndex = 13;
      this.pictureBox1.TabStop = false;
      // 
      // groupBox14
      // 
      this.groupBox14.Location = new System.Drawing.Point(16, 216);
      this.groupBox14.Name = "groupBox14";
      this.groupBox14.Size = new System.Drawing.Size(320, 120);
      this.groupBox14.TabIndex = 5;
      this.groupBox14.TabStop = false;
      this.groupBox14.Text = "Project info";
      // 
      // tabPlayers
      // 
      this.tabPlayers.Controls.Add(this.comboMovieAudioRenderer);
      this.tabPlayers.Controls.Add(this.label39);
      this.tabPlayers.Controls.Add(this.groupBox9);
      this.tabPlayers.Controls.Add(this.groupBox8);
      this.tabPlayers.Controls.Add(this.MoviePlayerBox);
      this.tabPlayers.Location = new System.Drawing.Point(4, 22);
      this.tabPlayers.Name = "tabPlayers";
      this.tabPlayers.Size = new System.Drawing.Size(616, 374);
      this.tabPlayers.TabIndex = 3;
      this.tabPlayers.Text = "MoviePlayer";
      // 
      // comboMovieAudioRenderer
      // 
      this.comboMovieAudioRenderer.Location = new System.Drawing.Point(120, 264);
      this.comboMovieAudioRenderer.Name = "comboMovieAudioRenderer";
      this.comboMovieAudioRenderer.Size = new System.Drawing.Size(184, 21);
      this.comboMovieAudioRenderer.TabIndex = 3;
      // 
      // label39
      // 
      this.label39.Location = new System.Drawing.Point(24, 272);
      this.label39.Name = "label39";
      this.label39.Size = new System.Drawing.Size(100, 16);
      this.label39.TabIndex = 9;
      this.label39.Text = "Audio renderer:";
      // 
      // groupBox9
      // 
      this.groupBox9.Controls.Add(this.labelOSDTimeout);
      this.groupBox9.Controls.Add(this.label15);
      this.groupBox9.Controls.Add(this.trackBarOSDTimeout);
      this.groupBox9.Location = new System.Drawing.Point(304, 128);
      this.groupBox9.Name = "groupBox9";
      this.groupBox9.Size = new System.Drawing.Size(200, 120);
      this.groupBox9.TabIndex = 2;
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
      this.groupBox8.TabIndex = 1;
      this.groupBox8.TabStop = false;
      this.groupBox8.Text = "Subtitles";
      // 
      // btnChooseSubFont
      // 
      this.btnChooseSubFont.Location = new System.Drawing.Point(224, 56);
      this.btnChooseSubFont.Name = "btnChooseSubFont";
      this.btnChooseSubFont.Size = new System.Drawing.Size(24, 23);
      this.btnChooseSubFont.TabIndex = 2;
      this.btnChooseSubFont.Text = "...";
      this.btnChooseSubFont.Click += new System.EventHandler(this.btnChooseSubFont_Click);
      // 
      // txtBoxSubFont
      // 
      this.txtBoxSubFont.Enabled = false;
      this.txtBoxSubFont.Location = new System.Drawing.Point(104, 56);
      this.txtBoxSubFont.Name = "txtBoxSubFont";
      this.txtBoxSubFont.Size = new System.Drawing.Size(112, 20);
      this.txtBoxSubFont.TabIndex = 1;
      this.txtBoxSubFont.Text = "textBoxSubFont";
      // 
      // numericUpDownSubShadow
      // 
      this.numericUpDownSubShadow.Location = new System.Drawing.Point(104, 88);
      this.numericUpDownSubShadow.Name = "numericUpDownSubShadow";
      this.numericUpDownSubShadow.Size = new System.Drawing.Size(48, 20);
      this.numericUpDownSubShadow.TabIndex = 3;
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
      this.MoviePlayerBox.TabIndex = 0;
      this.MoviePlayerBox.TabStop = false;
      this.MoviePlayerBox.Text = "Movie Player";
      // 
      // bntSelectMovieFile
      // 
      this.bntSelectMovieFile.Location = new System.Drawing.Point(416, 32);
      this.bntSelectMovieFile.Name = "bntSelectMovieFile";
      this.bntSelectMovieFile.Size = new System.Drawing.Size(24, 23);
      this.bntSelectMovieFile.TabIndex = 1;
      this.bntSelectMovieFile.Text = "...";
      this.bntSelectMovieFile.Click += new System.EventHandler(this.bntSelectMovieFile_Click);
      // 
      // movieParameters
      // 
      this.movieParameters.Location = new System.Drawing.Point(32, 72);
      this.movieParameters.Name = "movieParameters";
      this.movieParameters.Size = new System.Drawing.Size(152, 20);
      this.movieParameters.TabIndex = 2;
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
      this.movieFile.TabIndex = 0;
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
      this.checkBoxMovieInternalPlayer.TabIndex = 3;
      this.checkBoxMovieInternalPlayer.Text = "Use Internal player";
      this.checkBoxMovieInternalPlayer.CheckedChanged += new System.EventHandler(this.checkBoxMovieInternalPlayer_CheckedChanged);
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
      this.comboAudioPlayer.Size = new System.Drawing.Size(208, 21);
      this.comboAudioPlayer.TabIndex = 2;
      // 
      // TabDVDPlayer
      // 
      this.TabDVDPlayer.Controls.Add(this.groupBox12);
      this.TabDVDPlayer.Controls.Add(this.groupBox11);
      this.TabDVDPlayer.Controls.Add(this.groupBox10);
      this.TabDVDPlayer.Controls.Add(this.DVDPlayerBox);
      this.TabDVDPlayer.Location = new System.Drawing.Point(4, 22);
      this.TabDVDPlayer.Name = "TabDVDPlayer";
      this.TabDVDPlayer.Size = new System.Drawing.Size(616, 374);
      this.TabDVDPlayer.TabIndex = 7;
      this.TabDVDPlayer.Text = "DVDPlayer";
      // 
      // groupBox12
      // 
      this.groupBox12.Controls.Add(this.checkBoxDVDSubtitles);
      this.groupBox12.Controls.Add(this.comboBoxSubtitleLanguage);
      this.groupBox12.Controls.Add(this.comboBoxAudioLanguage);
      this.groupBox12.Controls.Add(this.label29);
      this.groupBox12.Controls.Add(this.label28);
      this.groupBox12.Location = new System.Drawing.Point(8, 96);
      this.groupBox12.Name = "groupBox12";
      this.groupBox12.Size = new System.Drawing.Size(600, 88);
      this.groupBox12.TabIndex = 9;
      this.groupBox12.TabStop = false;
      this.groupBox12.Text = "Languages";
      // 
      // checkBoxDVDSubtitles
      // 
      this.checkBoxDVDSubtitles.Location = new System.Drawing.Point(320, 56);
      this.checkBoxDVDSubtitles.Name = "checkBoxDVDSubtitles";
      this.checkBoxDVDSubtitles.Size = new System.Drawing.Size(104, 16);
      this.checkBoxDVDSubtitles.TabIndex = 5;
      this.checkBoxDVDSubtitles.Text = "Show Subtitles";
      this.checkBoxDVDSubtitles.CheckedChanged += new System.EventHandler(this.checkBoxDVDSubtitles_CheckedChanged);
      // 
      // comboBoxSubtitleLanguage
      // 
      this.comboBoxSubtitleLanguage.ItemHeight = 13;
      this.comboBoxSubtitleLanguage.Location = new System.Drawing.Point(152, 56);
      this.comboBoxSubtitleLanguage.Name = "comboBoxSubtitleLanguage";
      this.comboBoxSubtitleLanguage.Size = new System.Drawing.Size(152, 21);
      this.comboBoxSubtitleLanguage.Sorted = true;
      this.comboBoxSubtitleLanguage.TabIndex = 4;
      // 
      // comboBoxAudioLanguage
      // 
      this.comboBoxAudioLanguage.ItemHeight = 13;
      this.comboBoxAudioLanguage.Location = new System.Drawing.Point(152, 24);
      this.comboBoxAudioLanguage.Name = "comboBoxAudioLanguage";
      this.comboBoxAudioLanguage.Size = new System.Drawing.Size(152, 21);
      this.comboBoxAudioLanguage.Sorted = true;
      this.comboBoxAudioLanguage.TabIndex = 3;
      // 
      // label29
      // 
      this.label29.Location = new System.Drawing.Point(16, 56);
      this.label29.Name = "label29";
      this.label29.Size = new System.Drawing.Size(136, 16);
      this.label29.TabIndex = 7;
      this.label29.Text = "Default subtitle language:";
      // 
      // label28
      // 
      this.label28.Location = new System.Drawing.Point(16, 24);
      this.label28.Name = "label28";
      this.label28.Size = new System.Drawing.Size(128, 16);
      this.label28.TabIndex = 6;
      this.label28.Text = "Default Audio language:";
      // 
      // groupBox11
      // 
      this.groupBox11.Controls.Add(this.comboDVDNavigator);
      this.groupBox11.Controls.Add(this.label40);
      this.groupBox11.Controls.Add(this.comboDVDAudioRenderer);
      this.groupBox11.Controls.Add(this.label38);
      this.groupBox11.Location = new System.Drawing.Point(8, 184);
      this.groupBox11.Name = "groupBox11";
      this.groupBox11.Size = new System.Drawing.Size(600, 72);
      this.groupBox11.TabIndex = 8;
      this.groupBox11.TabStop = false;
      this.groupBox11.Text = "DVD Codec";
      // 
      // comboDVDNavigator
      // 
      this.comboDVDNavigator.ItemHeight = 13;
      this.comboDVDNavigator.Location = new System.Drawing.Point(104, 40);
      this.comboDVDNavigator.Name = "comboDVDNavigator";
      this.comboDVDNavigator.Size = new System.Drawing.Size(184, 21);
      this.comboDVDNavigator.TabIndex = 7;
      // 
      // label40
      // 
      this.label40.Location = new System.Drawing.Point(8, 48);
      this.label40.Name = "label40";
      this.label40.Size = new System.Drawing.Size(88, 16);
      this.label40.TabIndex = 13;
      this.label40.Text = "DVD Navigator:";
      // 
      // comboDVDAudioRenderer
      // 
      this.comboDVDAudioRenderer.ItemHeight = 13;
      this.comboDVDAudioRenderer.Location = new System.Drawing.Point(104, 16);
      this.comboDVDAudioRenderer.Name = "comboDVDAudioRenderer";
      this.comboDVDAudioRenderer.Size = new System.Drawing.Size(152, 21);
      this.comboDVDAudioRenderer.TabIndex = 6;
      // 
      // label38
      // 
      this.label38.Location = new System.Drawing.Point(8, 24);
      this.label38.Name = "label38";
      this.label38.Size = new System.Drawing.Size(88, 16);
      this.label38.TabIndex = 11;
      this.label38.Text = "Audio renderer:";
      // 
      // groupBox10
      // 
      this.groupBox10.Controls.Add(this.comboBoxDVDDisplayMode);
      this.groupBox10.Controls.Add(this.label20);
      this.groupBox10.Controls.Add(this.label19);
      this.groupBox10.Controls.Add(this.comboBoxDVDARCorrectionMode);
      this.groupBox10.Controls.Add(this.checkBoxARDVD);
      this.groupBox10.Location = new System.Drawing.Point(8, 256);
      this.groupBox10.Name = "groupBox10";
      this.groupBox10.Size = new System.Drawing.Size(600, 112);
      this.groupBox10.TabIndex = 7;
      this.groupBox10.TabStop = false;
      this.groupBox10.Text = "Aspect Ratio";
      // 
      // comboBoxDVDDisplayMode
      // 
      this.comboBoxDVDDisplayMode.Items.AddRange(new object[] {
                                                                "Default",
                                                                "16:9",
                                                                "4:3 Pan Scan",
                                                                "4:3 Letterbox"});
      this.comboBoxDVDDisplayMode.Location = new System.Drawing.Point(136, 48);
      this.comboBoxDVDDisplayMode.Name = "comboBoxDVDDisplayMode";
      this.comboBoxDVDDisplayMode.Size = new System.Drawing.Size(121, 21);
      this.comboBoxDVDDisplayMode.TabIndex = 18;
      // 
      // label20
      // 
      this.label20.Location = new System.Drawing.Point(24, 56);
      this.label20.Name = "label20";
      this.label20.Size = new System.Drawing.Size(80, 16);
      this.label20.TabIndex = 17;
      this.label20.Text = "Display mode:";
      // 
      // label19
      // 
      this.label19.Location = new System.Drawing.Point(24, 16);
      this.label19.Name = "label19";
      this.label19.Size = new System.Drawing.Size(96, 24);
      this.label19.TabIndex = 16;
      this.label19.Text = "Aspect Ratio Correction mode:";
      // 
      // comboBoxDVDARCorrectionMode
      // 
      this.comboBoxDVDARCorrectionMode.Items.AddRange(new object[] {
                                                                     "Crop",
                                                                     "Letterbox",
                                                                     "Stretched",
                                                                     "Follow stream"});
      this.comboBoxDVDARCorrectionMode.Location = new System.Drawing.Point(136, 16);
      this.comboBoxDVDARCorrectionMode.Name = "comboBoxDVDARCorrectionMode";
      this.comboBoxDVDARCorrectionMode.Size = new System.Drawing.Size(121, 21);
      this.comboBoxDVDARCorrectionMode.TabIndex = 9;
      // 
      // checkBoxARDVD
      // 
      this.checkBoxARDVD.Location = new System.Drawing.Point(24, 80);
      this.checkBoxARDVD.Name = "checkBoxARDVD";
      this.checkBoxARDVD.Size = new System.Drawing.Size(224, 16);
      this.checkBoxARDVD.TabIndex = 8;
      this.checkBoxARDVD.Text = "Use PixelRatio correction for DVD\'s";
      // 
      // DVDPlayerBox
      // 
      this.DVDPlayerBox.Controls.Add(this.checkBoxInternalDVDPlayer);
      this.DVDPlayerBox.Controls.Add(this.label1);
      this.DVDPlayerBox.Controls.Add(this.dvdFile);
      this.DVDPlayerBox.Controls.Add(this.dvdbtnSelect);
      this.DVDPlayerBox.Controls.Add(this.label2);
      this.DVDPlayerBox.Controls.Add(this.dvdParams);
      this.DVDPlayerBox.Location = new System.Drawing.Point(8, 8);
      this.DVDPlayerBox.Name = "DVDPlayerBox";
      this.DVDPlayerBox.Size = new System.Drawing.Size(600, 88);
      this.DVDPlayerBox.TabIndex = 6;
      this.DVDPlayerBox.TabStop = false;
      this.DVDPlayerBox.Text = "DVD Player";
      // 
      // checkBoxInternalDVDPlayer
      // 
      this.checkBoxInternalDVDPlayer.Location = new System.Drawing.Point(16, 16);
      this.checkBoxInternalDVDPlayer.Name = "checkBoxInternalDVDPlayer";
      this.checkBoxInternalDVDPlayer.Size = new System.Drawing.Size(152, 16);
      this.checkBoxInternalDVDPlayer.TabIndex = 0;
      this.checkBoxInternalDVDPlayer.Text = "Use Internal DVD player";
      this.checkBoxInternalDVDPlayer.CheckedChanged += new System.EventHandler(this.checkBoxInternalDVDPlayer_CheckedChanged);
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(24, 40);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(56, 16);
      this.label1.TabIndex = 0;
      this.label1.Text = "Filename";
      // 
      // dvdFile
      // 
      this.dvdFile.Location = new System.Drawing.Point(88, 32);
      this.dvdFile.Name = "dvdFile";
      this.dvdFile.Size = new System.Drawing.Size(392, 20);
      this.dvdFile.TabIndex = 0;
      this.dvdFile.Text = "";
      // 
      // dvdbtnSelect
      // 
      this.dvdbtnSelect.Location = new System.Drawing.Point(488, 32);
      this.dvdbtnSelect.Name = "dvdbtnSelect";
      this.dvdbtnSelect.Size = new System.Drawing.Size(24, 23);
      this.dvdbtnSelect.TabIndex = 1;
      this.dvdbtnSelect.Text = "...";
      this.dvdbtnSelect.Click += new System.EventHandler(this.dvdbtnSelect_Click);
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(24, 64);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(64, 16);
      this.label2.TabIndex = 3;
      this.label2.Text = "Parameters";
      // 
      // dvdParams
      // 
      this.dvdParams.Location = new System.Drawing.Point(88, 56);
      this.dvdParams.Name = "dvdParams";
      this.dvdParams.Size = new System.Drawing.Size(160, 20);
      this.dvdParams.TabIndex = 2;
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
      this.groupBox2.Controls.Add(this.btnPlayListFolder);
      this.groupBox2.Controls.Add(this.textBoxPlayLists);
      this.groupBox2.Controls.Add(this.label17);
      this.groupBox2.Controls.Add(this.checkBoxShufflePlaylists);
      this.groupBox2.Controls.Add(this.chkBoxRepeatAudioPlaylist);
      this.groupBox2.Controls.Add(this.txtboxAudioFiles);
      this.groupBox2.Controls.Add(this.label8);
      this.groupBox2.Controls.Add(this.chkMusicID3);
      this.groupBox2.Location = new System.Drawing.Point(24, 256);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(576, 104);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Music settings";
      // 
      // btnPlayListFolder
      // 
      this.btnPlayListFolder.Location = new System.Drawing.Point(488, 72);
      this.btnPlayListFolder.Name = "btnPlayListFolder";
      this.btnPlayListFolder.Size = new System.Drawing.Size(24, 23);
      this.btnPlayListFolder.TabIndex = 2;
      this.btnPlayListFolder.Text = "...";
      this.btnPlayListFolder.Click += new System.EventHandler(this.btnPlayListFolder_Click);
      // 
      // textBoxPlayLists
      // 
      this.textBoxPlayLists.Location = new System.Drawing.Point(88, 72);
      this.textBoxPlayLists.Name = "textBoxPlayLists";
      this.textBoxPlayLists.Size = new System.Drawing.Size(392, 20);
      this.textBoxPlayLists.TabIndex = 1;
      this.textBoxPlayLists.Text = "";
      // 
      // label17
      // 
      this.label17.Location = new System.Drawing.Point(16, 80);
      this.label17.Name = "label17";
      this.label17.Size = new System.Drawing.Size(80, 16);
      this.label17.TabIndex = 4;
      this.label17.Text = "Playlist folder:";
      // 
      // checkBoxShufflePlaylists
      // 
      this.checkBoxShufflePlaylists.Location = new System.Drawing.Point(336, 24);
      this.checkBoxShufflePlaylists.Name = "checkBoxShufflePlaylists";
      this.checkBoxShufflePlaylists.Size = new System.Drawing.Size(128, 24);
      this.checkBoxShufflePlaylists.TabIndex = 4;
      this.checkBoxShufflePlaylists.Text = "Auto shuffle playlist";
      // 
      // chkBoxRepeatAudioPlaylist
      // 
      this.chkBoxRepeatAudioPlaylist.Location = new System.Drawing.Point(336, 8);
      this.chkBoxRepeatAudioPlaylist.Name = "chkBoxRepeatAudioPlaylist";
      this.chkBoxRepeatAudioPlaylist.Size = new System.Drawing.Size(104, 16);
      this.chkBoxRepeatAudioPlaylist.TabIndex = 3;
      this.chkBoxRepeatAudioPlaylist.Text = "Repeat playlists";
      // 
      // txtboxAudioFiles
      // 
      this.txtboxAudioFiles.Location = new System.Drawing.Point(88, 48);
      this.txtboxAudioFiles.Name = "txtboxAudioFiles";
      this.txtboxAudioFiles.Size = new System.Drawing.Size(232, 20);
      this.txtboxAudioFiles.TabIndex = 0;
      this.txtboxAudioFiles.Text = "";
      // 
      // label8
      // 
      this.label8.Location = new System.Drawing.Point(16, 56);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(64, 16);
      this.label8.TabIndex = 1;
      this.label8.Text = "Audio files:";
      // 
      // chkMusicID3
      // 
      this.chkMusicID3.Location = new System.Drawing.Point(336, 48);
      this.chkMusicID3.Name = "chkMusicID3";
      this.chkMusicID3.Size = new System.Drawing.Size(104, 16);
      this.chkMusicID3.TabIndex = 5;
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
      this.audioGroupBox.Size = new System.Drawing.Size(576, 240);
      this.audioGroupBox.TabIndex = 0;
      this.audioGroupBox.TabStop = false;
      this.audioGroupBox.Text = "Music folders";
      // 
      // btnEditMusicShare
      // 
      this.btnEditMusicShare.Location = new System.Drawing.Point(136, 208);
      this.btnEditMusicShare.Name = "btnEditMusicShare";
      this.btnEditMusicShare.Size = new System.Drawing.Size(48, 23);
      this.btnEditMusicShare.TabIndex = 3;
      this.btnEditMusicShare.Text = "Edit";
      this.btnEditMusicShare.Click += new System.EventHandler(this.btnEditMusicShare_Click);
      // 
      // btnDelAudioShare
      // 
      this.btnDelAudioShare.Location = new System.Drawing.Point(72, 208);
      this.btnDelAudioShare.Name = "btnDelAudioShare";
      this.btnDelAudioShare.Size = new System.Drawing.Size(56, 23);
      this.btnDelAudioShare.TabIndex = 2;
      this.btnDelAudioShare.Text = "Delete";
      this.btnDelAudioShare.Click += new System.EventHandler(this.btnDelAudioShare_Click);
      // 
      // btnAddAudioShare
      // 
      this.btnAddAudioShare.Location = new System.Drawing.Point(8, 208);
      this.btnAddAudioShare.Name = "btnAddAudioShare";
      this.btnAddAudioShare.Size = new System.Drawing.Size(56, 23);
      this.btnAddAudioShare.TabIndex = 1;
      this.btnAddAudioShare.Text = "Add";
      this.btnAddAudioShare.Click += new System.EventHandler(this.btnAddAudioShare_Click);
      // 
      // listAudioShares
      // 
      this.listAudioShares.AllowDrop = true;
      this.listAudioShares.AllowRowReorder = true;
      this.listAudioShares.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                      this.HdrAudioName,
                                                                                      this.HdrAudioFolder,
                                                                                      this.HdrDefault});
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
      this.HdrAudioFolder.Width = 254;
      // 
      // HdrDefault
      // 
      this.HdrDefault.Text = "Default";
      this.HdrDefault.Width = 89;
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
      this.groupBox3.Controls.Add(this.btnPlayListVideo);
      this.groupBox3.Controls.Add(this.textBoxPlayListFolderVideo);
      this.groupBox3.Controls.Add(this.label18);
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
      // btnPlayListVideo
      // 
      this.btnPlayListVideo.Location = new System.Drawing.Point(448, 56);
      this.btnPlayListVideo.Name = "btnPlayListVideo";
      this.btnPlayListVideo.Size = new System.Drawing.Size(24, 24);
      this.btnPlayListVideo.TabIndex = 4;
      this.btnPlayListVideo.Text = "...";
      this.btnPlayListVideo.Click += new System.EventHandler(this.btnPlayListVideo_Click);
      // 
      // textBoxPlayListFolderVideo
      // 
      this.textBoxPlayListFolderVideo.Location = new System.Drawing.Point(104, 56);
      this.textBoxPlayListFolderVideo.Name = "textBoxPlayListFolderVideo";
      this.textBoxPlayListFolderVideo.Size = new System.Drawing.Size(336, 20);
      this.textBoxPlayListFolderVideo.TabIndex = 3;
      this.textBoxPlayListFolderVideo.Text = "";
      // 
      // label18
      // 
      this.label18.Location = new System.Drawing.Point(16, 56);
      this.label18.Name = "label18";
      this.label18.Size = new System.Drawing.Size(80, 16);
      this.label18.TabIndex = 2;
      this.label18.Text = "Playlists folder:";
      // 
      // chkBoxVideoRepeat
      // 
      this.chkBoxVideoRepeat.Location = new System.Drawing.Point(16, 24);
      this.chkBoxVideoRepeat.Name = "chkBoxVideoRepeat";
      this.chkBoxVideoRepeat.TabIndex = 0;
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
      this.listVideoShares.AllowDrop = true;
      this.listVideoShares.AllowRowReorder = true;
      this.listVideoShares.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                      this.HdrVideoName,
                                                                                      this.HdrVideoFolder,
                                                                                      this.columnHeader5});
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
      this.HdrVideoFolder.Width = 284;
      // 
      // columnHeader5
      // 
      this.columnHeader5.Text = "Default";
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
      this.txtBoxPictureFiles.TabIndex = 2;
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
      this.UpDownPictureTransition.TabIndex = 1;
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
      this.label4.TabIndex = 1;
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
      this.UpDownPictureDuration.TabIndex = 0;
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
      this.listPictureShares.AllowDrop = true;
      this.listPictureShares.AllowRowReorder = true;
      this.listPictureShares.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                        this.HdrPictureName,
                                                                                        this.HdrPictureFolder,
                                                                                        this.columnHeader6});
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
      this.HdrPictureFolder.Width = 323;
      // 
      // columnHeader6
      // 
      this.columnHeader6.Text = "Default";
      // 
      // tabTVChannels
      // 
      this.tabTVChannels.Controls.Add(this.groupBox20);
      this.tabTVChannels.Controls.Add(this.groupBox19);
      this.tabTVChannels.Controls.Add(this.btnEditChannel);
      this.tabTVChannels.Controls.Add(this.btnDelChannel);
      this.tabTVChannels.Controls.Add(this.btnNewChannel);
      this.tabTVChannels.Controls.Add(this.btnTvChannelDown);
      this.tabTVChannels.Controls.Add(this.btnTvChannelUp);
      this.tabTVChannels.Controls.Add(this.listTVChannels);
      this.tabTVChannels.Controls.Add(this.groupBox18);
      this.tabTVChannels.Location = new System.Drawing.Point(4, 22);
      this.tabTVChannels.Name = "tabTVChannels";
      this.tabTVChannels.Size = new System.Drawing.Size(616, 374);
      this.tabTVChannels.TabIndex = 9;
      this.tabTVChannels.Text = "TVChannels";
      // 
      // groupBox20
      // 
      this.groupBox20.Controls.Add(this.btnXMLTVFolder);
      this.groupBox20.Controls.Add(this.textBoxXMLTVFolder);
      this.groupBox20.Location = new System.Drawing.Point(8, 304);
      this.groupBox20.Name = "groupBox20";
      this.groupBox20.Size = new System.Drawing.Size(432, 56);
      this.groupBox20.TabIndex = 21;
      this.groupBox20.TabStop = false;
      this.groupBox20.Text = "XMLTV Folder";
      // 
      // btnXMLTVFolder
      // 
      this.btnXMLTVFolder.Location = new System.Drawing.Point(376, 24);
      this.btnXMLTVFolder.Name = "btnXMLTVFolder";
      this.btnXMLTVFolder.Size = new System.Drawing.Size(32, 23);
      this.btnXMLTVFolder.TabIndex = 1;
      this.btnXMLTVFolder.Text = "...";
      this.btnXMLTVFolder.Click += new System.EventHandler(this.btnXMLTVFolder_Click);
      // 
      // textBoxXMLTVFolder
      // 
      this.textBoxXMLTVFolder.Location = new System.Drawing.Point(16, 24);
      this.textBoxXMLTVFolder.Name = "textBoxXMLTVFolder";
      this.textBoxXMLTVFolder.Size = new System.Drawing.Size(352, 20);
      this.textBoxXMLTVFolder.TabIndex = 0;
      this.textBoxXMLTVFolder.Text = "";
      // 
      // groupBox19
      // 
      this.groupBox19.Controls.Add(this.checkBoxTVGuideColors);
      this.groupBox19.Controls.Add(this.label37);
      this.groupBox19.Controls.Add(this.label36);
      this.groupBox19.Controls.Add(this.timeZoneCorrection);
      this.groupBox19.Controls.Add(this.xmltvTimeZoneCheck);
      this.groupBox19.Location = new System.Drawing.Point(448, 8);
      this.groupBox19.Name = "groupBox19";
      this.groupBox19.Size = new System.Drawing.Size(160, 352);
      this.groupBox19.TabIndex = 20;
      this.groupBox19.TabStop = false;
      this.groupBox19.Text = "TVGuide";
      // 
      // checkBoxTVGuideColors
      // 
      this.checkBoxTVGuideColors.Location = new System.Drawing.Point(16, 56);
      this.checkBoxTVGuideColors.Name = "checkBoxTVGuideColors";
      this.checkBoxTVGuideColors.Size = new System.Drawing.Size(120, 32);
      this.checkBoxTVGuideColors.TabIndex = 4;
      this.checkBoxTVGuideColors.Text = "Use colors in TVGuide";
      // 
      // label37
      // 
      this.label37.Location = new System.Drawing.Point(96, 112);
      this.label37.Name = "label37";
      this.label37.Size = new System.Drawing.Size(40, 16);
      this.label37.TabIndex = 3;
      this.label37.Text = "Hours";
      // 
      // label36
      // 
      this.label36.Location = new System.Drawing.Point(16, 96);
      this.label36.Name = "label36";
      this.label36.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.label36.Size = new System.Drawing.Size(128, 16);
      this.label36.TabIndex = 1;
      this.label36.Text = "Timezone compensation";
      // 
      // timeZoneCorrection
      // 
      this.timeZoneCorrection.Location = new System.Drawing.Point(32, 112);
      this.timeZoneCorrection.Maximum = new System.Decimal(new int[] {
                                                                       23,
                                                                       0,
                                                                       0,
                                                                       0});
      this.timeZoneCorrection.Minimum = new System.Decimal(new int[] {
                                                                       23,
                                                                       0,
                                                                       0,
                                                                       -2147483648});
      this.timeZoneCorrection.Name = "timeZoneCorrection";
      this.timeZoneCorrection.Size = new System.Drawing.Size(56, 20);
      this.timeZoneCorrection.TabIndex = 2;
      // 
      // xmltvTimeZoneCheck
      // 
      this.xmltvTimeZoneCheck.Location = new System.Drawing.Point(16, 24);
      this.xmltvTimeZoneCheck.Name = "xmltvTimeZoneCheck";
      this.xmltvTimeZoneCheck.Size = new System.Drawing.Size(120, 32);
      this.xmltvTimeZoneCheck.TabIndex = 0;
      this.xmltvTimeZoneCheck.Text = "Use Timezone info from XMLTV";
      // 
      // btnEditChannel
      // 
      this.btnEditChannel.Location = new System.Drawing.Point(120, 256);
      this.btnEditChannel.Name = "btnEditChannel";
      this.btnEditChannel.Size = new System.Drawing.Size(48, 23);
      this.btnEditChannel.TabIndex = 3;
      this.btnEditChannel.Text = "Edit";
      this.btnEditChannel.Click += new System.EventHandler(this.btnEditChannel_Click);
      // 
      // btnDelChannel
      // 
      this.btnDelChannel.Location = new System.Drawing.Point(64, 256);
      this.btnDelChannel.Name = "btnDelChannel";
      this.btnDelChannel.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.btnDelChannel.Size = new System.Drawing.Size(48, 23);
      this.btnDelChannel.TabIndex = 2;
      this.btnDelChannel.Text = "Delete";
      this.btnDelChannel.Click += new System.EventHandler(this.btnDelChannel_Click);
      // 
      // btnNewChannel
      // 
      this.btnNewChannel.Location = new System.Drawing.Point(24, 256);
      this.btnNewChannel.Name = "btnNewChannel";
      this.btnNewChannel.Size = new System.Drawing.Size(32, 24);
      this.btnNewChannel.TabIndex = 1;
      this.btnNewChannel.Text = "Add";
      this.btnNewChannel.Click += new System.EventHandler(this.btnNewChannel_Click);
      // 
      // btnTvChannelDown
      // 
      this.btnTvChannelDown.Location = new System.Drawing.Point(416, 224);
      this.btnTvChannelDown.Name = "btnTvChannelDown";
      this.btnTvChannelDown.Size = new System.Drawing.Size(16, 23);
      this.btnTvChannelDown.TabIndex = 5;
      this.btnTvChannelDown.Text = "v";
      this.btnTvChannelDown.Click += new System.EventHandler(this.btnTvChannelDown_Click);
      // 
      // btnTvChannelUp
      // 
      this.btnTvChannelUp.Location = new System.Drawing.Point(416, 200);
      this.btnTvChannelUp.Name = "btnTvChannelUp";
      this.btnTvChannelUp.Size = new System.Drawing.Size(16, 24);
      this.btnTvChannelUp.TabIndex = 4;
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
      this.listTVChannels.Location = new System.Drawing.Point(24, 32);
      this.listTVChannels.Name = "listTVChannels";
      this.listTVChannels.Size = new System.Drawing.Size(384, 216);
      this.listTVChannels.TabIndex = 0;
      this.listTVChannels.View = System.Windows.Forms.View.Details;
      this.listTVChannels.DoubleClick += new System.EventHandler(this.listTVChannels_DoubleClick);
      this.listTVChannels.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listTVChannels_ColumnClick);
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "TV Channel name";
      this.columnHeader1.Width = 108;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Channel";
      this.columnHeader2.Width = 118;
      // 
      // columnHeader3
      // 
      this.columnHeader3.Text = "Frequency (MHz)";
      this.columnHeader3.Width = 118;
      // 
      // groupBox18
      // 
      this.groupBox18.Controls.Add(this.buttonAutoTune2);
      this.groupBox18.Controls.Add(this.textEditBox);
      this.groupBox18.Location = new System.Drawing.Point(8, 8);
      this.groupBox18.Name = "groupBox18";
      this.groupBox18.Size = new System.Drawing.Size(432, 288);
      this.groupBox18.TabIndex = 19;
      this.groupBox18.TabStop = false;
      this.groupBox18.Text = "TV Channels";
      // 
      // buttonAutoTune2
      // 
      this.buttonAutoTune2.Location = new System.Drawing.Point(176, 248);
      this.buttonAutoTune2.Name = "buttonAutoTune2";
      this.buttonAutoTune2.TabIndex = 19;
      this.buttonAutoTune2.Text = "Auto Tune";
      this.buttonAutoTune2.Click += new System.EventHandler(this.buttonAutoTune2_Click);
      // 
      // textEditBox
      // 
      this.textEditBox.Location = new System.Drawing.Point(312, 248);
      this.textEditBox.Name = "textEditBox";
      this.textEditBox.TabIndex = 18;
      this.textEditBox.Text = "textBox1";
      this.textEditBox.Visible = false;
      // 
      // tabPageCapture
      // 
      this.tabPageCapture.Controls.Add(this.btnEditCaptureDevice);
      this.tabPageCapture.Controls.Add(this.btnDelCaptureDevice);
      this.tabPageCapture.Controls.Add(this.btnAddCaptureDevice);
      this.tabPageCapture.Controls.Add(this.listCaptureCards);
      this.tabPageCapture.Controls.Add(this.UpDownPostRecording);
      this.tabPageCapture.Controls.Add(this.groupBox13);
      this.tabPageCapture.Controls.Add(this.btnRecPath);
      this.tabPageCapture.Controls.Add(this.textBoxRecPath);
      this.tabPageCapture.Controls.Add(this.groupBox16);
      this.tabPageCapture.Controls.Add(this.groupBox17);
      this.tabPageCapture.Location = new System.Drawing.Point(4, 22);
      this.tabPageCapture.Name = "tabPageCapture";
      this.tabPageCapture.Size = new System.Drawing.Size(616, 374);
      this.tabPageCapture.TabIndex = 6;
      this.tabPageCapture.Text = "Video Capture";
      // 
      // btnEditCaptureDevice
      // 
      this.btnEditCaptureDevice.Location = new System.Drawing.Point(128, 232);
      this.btnEditCaptureDevice.Name = "btnEditCaptureDevice";
      this.btnEditCaptureDevice.Size = new System.Drawing.Size(40, 23);
      this.btnEditCaptureDevice.TabIndex = 3;
      this.btnEditCaptureDevice.Text = "Edit";
      this.btnEditCaptureDevice.Click += new System.EventHandler(this.btnEditCaptureDevice_Click);
      // 
      // btnDelCaptureDevice
      // 
      this.btnDelCaptureDevice.Location = new System.Drawing.Point(64, 232);
      this.btnDelCaptureDevice.Name = "btnDelCaptureDevice";
      this.btnDelCaptureDevice.Size = new System.Drawing.Size(56, 23);
      this.btnDelCaptureDevice.TabIndex = 2;
      this.btnDelCaptureDevice.Text = "Delete";
      this.btnDelCaptureDevice.Click += new System.EventHandler(this.btnDelCaptureDevice_Click);
      // 
      // btnAddCaptureDevice
      // 
      this.btnAddCaptureDevice.Location = new System.Drawing.Point(16, 232);
      this.btnAddCaptureDevice.Name = "btnAddCaptureDevice";
      this.btnAddCaptureDevice.Size = new System.Drawing.Size(40, 23);
      this.btnAddCaptureDevice.TabIndex = 1;
      this.btnAddCaptureDevice.Text = "Add";
      this.btnAddCaptureDevice.Click += new System.EventHandler(this.btnAddCaptureDevice_Click);
      // 
      // listCaptureCards
      // 
      this.listCaptureCards.AllowDrop = true;
      this.listCaptureCards.AllowRowReorder = true;
      this.listCaptureCards.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                       this.columnHeader7,
                                                                                       this.columnHeader8,
                                                                                       this.columnHeader9});
      this.listCaptureCards.FullRowSelect = true;
      this.listCaptureCards.Location = new System.Drawing.Point(16, 16);
      this.listCaptureCards.Name = "listCaptureCards";
      this.listCaptureCards.Size = new System.Drawing.Size(392, 208);
      this.listCaptureCards.TabIndex = 0;
      this.listCaptureCards.View = System.Windows.Forms.View.Details;
      this.listCaptureCards.DoubleClick += new System.EventHandler(this.listCaptureCards_DoubleClick);
      // 
      // columnHeader7
      // 
      this.columnHeader7.Text = "Capture Device";
      this.columnHeader7.Width = 174;
      // 
      // columnHeader8
      // 
      this.columnHeader8.Text = "use for TV";
      this.columnHeader8.Width = 96;
      // 
      // columnHeader9
      // 
      this.columnHeader9.Text = "use for Recording";
      this.columnHeader9.Width = 115;
      // 
      // UpDownPostRecording
      // 
      this.UpDownPostRecording.Location = new System.Drawing.Point(464, 312);
      this.UpDownPostRecording.Name = "UpDownPostRecording";
      this.UpDownPostRecording.Size = new System.Drawing.Size(48, 20);
      this.UpDownPostRecording.TabIndex = 9;
      // 
      // groupBox13
      // 
      this.groupBox13.Controls.Add(this.buttonAutoTune);
      this.groupBox13.Controls.Add(this.upDownCountry);
      this.groupBox13.Controls.Add(this.label24);
      this.groupBox13.Controls.Add(this.label23);
      this.groupBox13.Controls.Add(this.btnradioCable);
      this.groupBox13.Controls.Add(this.btnradioAntenna);
      this.groupBox13.Location = new System.Drawing.Point(424, 88);
      this.groupBox13.Name = "groupBox13";
      this.groupBox13.Size = new System.Drawing.Size(176, 120);
      this.groupBox13.TabIndex = 7;
      this.groupBox13.TabStop = false;
      this.groupBox13.Text = "Tuner";
      // 
      // buttonAutoTune
      // 
      this.buttonAutoTune.Location = new System.Drawing.Point(32, 88);
      this.buttonAutoTune.Name = "buttonAutoTune";
      this.buttonAutoTune.Size = new System.Drawing.Size(75, 24);
      this.buttonAutoTune.TabIndex = 4;
      this.buttonAutoTune.Text = "Auto Tune";
      this.buttonAutoTune.Click += new System.EventHandler(this.buttonAutoTune_Click);
      // 
      // upDownCountry
      // 
      this.upDownCountry.Location = new System.Drawing.Point(64, 56);
      this.upDownCountry.Maximum = new System.Decimal(new int[] {
                                                                  999,
                                                                  0,
                                                                  0,
                                                                  0});
      this.upDownCountry.Name = "upDownCountry";
      this.upDownCountry.Size = new System.Drawing.Size(56, 20);
      this.upDownCountry.TabIndex = 2;
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
      this.label23.Location = new System.Drawing.Point(8, 56);
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
      // btnRecPath
      // 
      this.btnRecPath.Location = new System.Drawing.Point(368, 320);
      this.btnRecPath.Name = "btnRecPath";
      this.btnRecPath.Size = new System.Drawing.Size(24, 23);
      this.btnRecPath.TabIndex = 6;
      this.btnRecPath.Text = "...";
      this.btnRecPath.Click += new System.EventHandler(this.btnRecPath_Click);
      // 
      // textBoxRecPath
      // 
      this.textBoxRecPath.Location = new System.Drawing.Point(40, 320);
      this.textBoxRecPath.Name = "textBoxRecPath";
      this.textBoxRecPath.Size = new System.Drawing.Size(312, 20);
      this.textBoxRecPath.TabIndex = 5;
      this.textBoxRecPath.Text = "";
      // 
      // groupBox16
      // 
      this.groupBox16.Location = new System.Drawing.Point(24, 288);
      this.groupBox16.Name = "groupBox16";
      this.groupBox16.Size = new System.Drawing.Size(384, 72);
      this.groupBox16.TabIndex = 4;
      this.groupBox16.TabStop = false;
      this.groupBox16.Text = "Recording path";
      // 
      // groupBox17
      // 
      this.groupBox17.Controls.Add(this.label35);
      this.groupBox17.Controls.Add(this.label34);
      this.groupBox17.Controls.Add(this.label33);
      this.groupBox17.Controls.Add(this.label32);
      this.groupBox17.Controls.Add(this.label31);
      this.groupBox17.Controls.Add(this.label21);
      this.groupBox17.Controls.Add(this.UpDownPreRecording);
      this.groupBox17.Location = new System.Drawing.Point(424, 216);
      this.groupBox17.Name = "groupBox17";
      this.groupBox17.Size = new System.Drawing.Size(176, 144);
      this.groupBox17.TabIndex = 8;
      this.groupBox17.TabStop = false;
      this.groupBox17.Text = "Pre/Post recording";
      // 
      // label35
      // 
      this.label35.Location = new System.Drawing.Point(40, 56);
      this.label35.Name = "label35";
      this.label35.Size = new System.Drawing.Size(112, 16);
      this.label35.TabIndex = 0;
      this.label35.Text = "before program starts";
      // 
      // label34
      // 
      this.label34.Location = new System.Drawing.Point(40, 120);
      this.label34.Name = "label34";
      this.label34.Size = new System.Drawing.Size(120, 16);
      this.label34.TabIndex = 1;
      this.label34.Text = "after program stopped";
      // 
      // label33
      // 
      this.label33.Location = new System.Drawing.Point(104, 96);
      this.label33.Name = "label33";
      this.label33.Size = new System.Drawing.Size(40, 16);
      this.label33.TabIndex = 18;
      this.label33.Text = "min.";
      // 
      // label32
      // 
      this.label32.Location = new System.Drawing.Point(16, 80);
      this.label32.Name = "label32";
      this.label32.Size = new System.Drawing.Size(128, 16);
      this.label32.TabIndex = 17;
      this.label32.Text = "Stop Recording:";
      // 
      // label31
      // 
      this.label31.Location = new System.Drawing.Point(96, 32);
      this.label31.Name = "label31";
      this.label31.Size = new System.Drawing.Size(24, 16);
      this.label31.TabIndex = 16;
      this.label31.Text = "min.";
      // 
      // label21
      // 
      this.label21.Location = new System.Drawing.Point(16, 16);
      this.label21.Name = "label21";
      this.label21.Size = new System.Drawing.Size(104, 16);
      this.label21.TabIndex = 0;
      this.label21.Text = "Start recording:";
      // 
      // UpDownPreRecording
      // 
      this.UpDownPreRecording.Location = new System.Drawing.Point(40, 32);
      this.UpDownPreRecording.Name = "UpDownPreRecording";
      this.UpDownPreRecording.Size = new System.Drawing.Size(48, 20);
      this.UpDownPreRecording.TabIndex = 0;
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
      this.Skin.ResumeLayout(false);
      this.tabPlayers.ResumeLayout(false);
      this.groupBox9.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.trackBarOSDTimeout)).EndInit();
      this.groupBox8.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSubShadow)).EndInit();
      this.MoviePlayerBox.ResumeLayout(false);
      this.tabAudioPlayer.ResumeLayout(false);
      this.groupBox15.ResumeLayout(false);
      this.TabDVDPlayer.ResumeLayout(false);
      this.groupBox12.ResumeLayout(false);
      this.groupBox11.ResumeLayout(false);
      this.groupBox10.ResumeLayout(false);
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
      this.tabTVChannels.ResumeLayout(false);
      this.groupBox20.ResumeLayout(false);
      this.groupBox19.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.timeZoneCorrection)).EndInit();
      this.groupBox18.ResumeLayout(false);
      this.tabPageCapture.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.UpDownPostRecording)).EndInit();
      this.groupBox13.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.upDownCountry)).EndInit();
      this.groupBox17.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.UpDownPreRecording)).EndInit();
      this.tabWeather.ResumeLayout(false);
      this.groupBox7.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.cntrlweatherRefresh)).EndInit();
      this.groupBox6.ResumeLayout(false);
      this.groupBox5.ResumeLayout(false);
      this.groupBox4.ResumeLayout(false);
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
        string strDefault=xmlreader.GetValueAsString("movies", "default","");
        string strShareName=String.Format("sharename{0}",i);
				string strSharePath=String.Format("sharepath{0}",i);
				string strName=xmlreader.GetValueAsString("movies", strShareName,"");
				strPath=xmlreader.GetValueAsString("movies", strSharePath,"");
				if (strName.Length>0 && strPath.Length>0)
				{
					newItem=listVideoShares.Items.Add(strName);
					newItem.SubItems.Add(strPath);
          if (strName==strDefault)
          {
            newItem.SubItems.Add("X");
          }
          else 
            newItem.SubItems.Add("");
				}

        
        strDefault=xmlreader.GetValueAsString("music", "default","");
				strName=xmlreader.GetValueAsString("music", strShareName,"");
				strPath=xmlreader.GetValueAsString("music", strSharePath,"");
				if (strName.Length>0 && strPath.Length>0)
				{
					newItem=listAudioShares.Items.Add(strName);
          newItem.SubItems.Add(strPath);
          if (strName==strDefault)
          {
            newItem.SubItems.Add("X");
          }
          else 
            newItem.SubItems.Add("");
				}

        strDefault=xmlreader.GetValueAsString("pictures", "default","");
        strName=xmlreader.GetValueAsString("pictures", strShareName,"");
				strPath=xmlreader.GetValueAsString("pictures", strSharePath,"");
				if (strName.Length>0 && strPath.Length>0)
				{
					newItem=listPictureShares.Items.Add(strName);
          newItem.SubItems.Add(strPath);
          if (strName==strDefault)
          {
            newItem.SubItems.Add("X");
          }
          else 
            newItem.SubItems.Add("");
				}
			}
			if (listVideoShares.Items.Count==0)
			{
				ListViewItem newItemVideo;

				strPath=String.Format(@"{0}\My Videos",Environment.GetFolderPath( Environment.SpecialFolder.Personal).ToString());
				System.IO.Directory.CreateDirectory(strPath);
        newItemVideo=listVideoShares.Items.Add("Movies");
        newItemVideo.SubItems.Add(strPath);
				newItemVideo.SubItems.Add( "");

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
        newItemAudio.SubItems.Add( "");
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
        newItemPictures.SubItems.Add( "");
			}
		}

		void LoadSettings()
		{
      using (AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        checkStartFullScreen.Checked=xmlreader.GetValueAsBool("general","startfullscreen",false);
        checkBoxAutoHideMouse.Checked=xmlreader.GetValueAsBool("general","autohidemouse",false);
        checkBoxMouseSupport.Checked=xmlreader.GetValueAsBool("general","mousesupport",true);
        checkBoxHideFileExtensions.Checked=xmlreader.GetValueAsBool("general","hideextensions",true);

        dvdFile.Text=xmlreader.GetValueAsString("dvdplayer","path",@"C:\program files\cyberlink\powerdvd\powerdvd.exe");
        dvdParams.Text=xmlreader.GetValueAsString("dvdplayer","arguments","");
        checkBoxInternalDVDPlayer.Checked=xmlreader.GetValueAsBool("dvdplayer","internal",true);
        checkBoxARDVD.Checked=xmlreader.GetValueAsBool("dvdplayer","pixelratiocorrection",false);
        string strDVDAudioRenderer=xmlreader.GetValueAsString("dvdplayer","audiorenderer","");
        string strARMode=xmlreader.GetValueAsString("dvdplayer","armode","Stretched");
        int iSel=0;
        comboBoxDVDARCorrectionMode.SelectedIndex=0;
        foreach (string strItem in comboBoxDVDARCorrectionMode.Items)
        {
          if (strItem==strARMode)
          {
            comboBoxDVDARCorrectionMode.SelectedIndex=iSel;
            break;
          }
          iSel++;
        }

        string strDisplayMode=xmlreader.GetValueAsString("dvdplayer","displaymode","");
        iSel=0;
        comboBoxDVDDisplayMode.SelectedIndex=0;
        foreach (string strItem in comboBoxDVDDisplayMode.Items)
        {
          if (strItem==strDisplayMode)
          {
            comboBoxDVDDisplayMode.SelectedIndex=iSel;
            break;
          }
          iSel++;
        }

        SetupAudioRenderer(comboDVDAudioRenderer,strDVDAudioRenderer);
        
        string strDVDAudioLanguage=xmlreader.GetValueAsString("dvdplayer","audiolanguage","English");
        string strDVDSubLanguage=xmlreader.GetValueAsString("dvdplayer","subtitlelanguage","English");
        checkBoxDVDSubtitles.Checked=xmlreader.GetValueAsBool("dvdplayer","showsubtitles",true);
        AddLanguages(comboBoxAudioLanguage,strDVDAudioLanguage);
        AddLanguages(comboBoxSubtitleLanguage,strDVDSubLanguage);

        string strDVDNavigator=xmlreader.GetValueAsString("dvdplayer","navigator","");
        SetupDVDNavigator(comboDVDNavigator,strDVDNavigator);

        movieFile.Text=xmlreader.GetValueAsString("movieplayer","path",@"zplayer\zplayer.exe");
        movieParameters.Text=xmlreader.GetValueAsString("movieplayer","arguments", @"%filename% /PLAY /F /Q");
        checkBoxMovieInternalPlayer.Checked=xmlreader.GetValueAsBool("movieplayer","internal",true);

        string strMovieAudioRenderer=xmlreader.GetValueAsString("movieplayer","audiorenderer","");
        SetupAudioRenderer(comboMovieAudioRenderer,strMovieAudioRenderer);
        

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
        string strDefault="";
        if (listAudioShares.Items.Count>0)
        {
          strDefault=listAudioShares.Items[0].SubItems[1].Text;
        }
 
        textBoxPlayLists.Text=xmlreader.GetValueAsString("music","playlists",strDefault);
        strDefault="";
        if (listVideoShares.Items.Count>0)
        {
          strDefault=listVideoShares.Items[0].SubItems[1].Text;
        }
        textBoxPlayListFolderVideo.Text=xmlreader.GetValueAsString("movies","playlists",strDefault);
        LoadWeather(xmlreader);


        textBoxXMLTVFolder.Text=xmlreader.GetValueAsString("xmltv","folder","xmltv");

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

        xmltvTimeZoneCheck.Checked=xmlreader.GetValueAsBool("xmltv", "usetimezone",true);
        timeZoneCorrection.Value=xmlreader.GetValueAsInt("xmltv", "timezonecorrection",0);

        checkBoxTVGuideColors.Checked=xmlreader.GetValueAsBool("xmltv", "colors",true);
      }
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
        else break;
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
          if (item.SubItems.Count>=3)
          {
            if (item.SubItems[2].Text=="X")
            {
              xmlWriter.SetValue(strTag, "default",strName);
            }
          }
        }
        xmlWriter.SetValue(strTag,strShareName,strName);
        xmlWriter.SetValue(strTag,strSharePath,strPath);
      }
    }
    void SaveSettings()
    {
      using (AMS.Profile.Xml   xmlWriter=new AMS.Profile.Xml("MediaPortal.xml"))
      {
  			
        xmlWriter.SetValueAsBool("general","startfullscreen",checkStartFullScreen.Checked);
        xmlWriter.SetValueAsBool("general","autohidemouse", checkBoxAutoHideMouse.Checked);
        xmlWriter.SetValueAsBool("general","mousesupport", checkBoxMouseSupport.Checked);
        xmlWriter.SetValueAsBool("general","hideextensions", checkBoxHideFileExtensions.Checked);
        

        xmlWriter.SetValue("dvdplayer","path",dvdFile.Text);
        xmlWriter.SetValue("dvdplayer","arguments",dvdParams.Text);
        xmlWriter.SetValueAsBool("dvdplayer","internal",checkBoxInternalDVDPlayer.Checked);
        xmlWriter.SetValueAsBool("dvdplayer","pixelratiocorrection",checkBoxARDVD.Checked);
        xmlWriter.SetValue("dvdplayer","audiolanguage",(string)comboBoxAudioLanguage.SelectedItem);
        xmlWriter.SetValue("dvdplayer","subtitlelanguage",(string)comboBoxSubtitleLanguage.SelectedItem);
        xmlWriter.SetValue("dvdplayer","audiorenderer",(string)comboDVDAudioRenderer.SelectedItem);
        xmlWriter.SetValue("dvdplayer","navigator",(string)comboDVDNavigator.SelectedItem);

        xmlWriter.SetValue("dvdplayer","armode",(string)comboBoxDVDARCorrectionMode.SelectedItem);
        xmlWriter.SetValue("dvdplayer","displaymode",(string)comboBoxDVDDisplayMode.SelectedItem);
        xmlWriter.SetValueAsBool("dvdplayer","showsubtitles",checkBoxDVDSubtitles.Checked);
        xmlWriter.SetValue("movieplayer","path",movieFile.Text);
        xmlWriter.SetValue("movieplayer","arguments",movieParameters.Text);

        
        xmlWriter.SetValue("movieplayer","audiorenderer",comboMovieAudioRenderer.SelectedItem);

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
  		  xmlWriter.SetValue("music" ,"playlists",textBoxPlayLists.Text);
        xmlWriter.SetValue("movies","playlists",textBoxPlayListFolderVideo.Text);
        
        xmlWriter.SetValue("music","extensions", txtboxAudioFiles.Text);
        xmlWriter.SetValue("movies","extensions", txtboxVideoFiles.Text);
        xmlWriter.SetValue("pictures","extensions",txtBoxPictureFiles.Text);

        SaveWeather(xmlWriter);

        SaveSubtitles(xmlWriter);
        SaveCapture(xmlWriter);
  			
        xmlWriter.SetValue("skin","language",comboBoxLanguage.SelectedItem);
        xmlWriter.SetValue("skin","name",comboBoxSkins.SelectedItem);
        SaveFrequencies();
          
        xmlWriter.SetValueAsBool("xmltv", "colors",checkBoxTVGuideColors.Checked);
        xmlWriter.SetValueAsBool("xmltv","usetimezone",xmltvTimeZoneCheck.Checked);
        xmlWriter.SetValue("xmltv","timezonecorrection",timeZoneCorrection.Value.ToString() );
        xmlWriter.SetValue("xmltv","folder",textBoxXMLTVFolder.Text);
      }
		}


    private void SetupForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      // sanity checks
      if (!System.IO.Directory.Exists(textBoxPlayListFolderVideo.Text))
      {
        MessageBox.Show(this.Parent,"video playlist folder does not exists", "Invalid configuration",MessageBoxButtons.OK);
        e.Cancel=true;
        return;
      }
      if (!System.IO.Directory.Exists(textBoxPlayLists.Text))
      {
        MessageBox.Show(this.Parent,"music playlist folder does not exists", "Invalid configuration",MessageBoxButtons.OK);
        e.Cancel=true;
        return;
      }
      if (!System.IO.Directory.Exists(textBoxRecPath.Text))
      {
        MessageBox.Show(this.Parent,"Movie recording folder does not exists", "Invalid configuration",MessageBoxButtons.OK);
        e.Cancel=true;
        return;
      }
      //check channel numbers!=0
      for (int i=0; i < listTVChannels.Items.Count;++i)
      {
        try
        {
          string strChannel=listTVChannels.Items[i].Text;
          string strNumber=listTVChannels.Items[i].SubItems[1].Text;
          string strFreq=listTVChannels.Items[i].SubItems[2].Text;
          int iNumber=GetInt(strNumber);
          if (iNumber <254)
          {
            int iChanNumber=GetInt(strNumber);
            if (iChanNumber==0)
            {
              MessageBox.Show(this.Parent,"Some tv channels have no channel number assigned", "Invalid configuration",MessageBoxButtons.OK);
              e.Cancel=true;
              return;
            }
          }
        }
        catch(Exception){}
      }

      SaveSettings();
      Recorder.Start();      
      Log.Write("------------leave setup--------");
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
      newItem.SubItems.Add("");

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
        comboMovieAudioRenderer.Enabled=true;

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
        comboMovieAudioRenderer.Enabled=false;
			}
    }

    private void btnChooseSubFont_Click(object sender, System.EventArgs e)
    {

      using (AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
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
      xmlWriter.SetValue("capture","recordingpath",textBoxRecPath.Text);
			xmlWriter.SetValue("capture","prerecord", UpDownPreRecording.Value.ToString());
			xmlWriter.SetValue("capture","postrecord", UpDownPostRecording.Value.ToString());

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
          int iNumber=GetInt(strNumber);
          if (iNumber <254)
          {
            long lFreq=0;
            double dTmp=2.50;
            string strTst=dTmp.ToString();
            if (strTst.IndexOf(".")>0)
              strFreq=strFreq.Replace("," , ".");
            else 
              strFreq=strFreq.Replace("." , ",");
            if (strFreq.IndexOf(".") >0  || strFreq.IndexOf(",") >0 )
            {
              double dFreq;
              dFreq=Convert.ToDouble(strFreq);
              dFreq*= (1000000d);
              lFreq=(long)dFreq;
            }
            else
            {
              try
              {
                lFreq=Int64.Parse(strFreq);
                if (lFreq<1000) lFreq *= 1000000L;
              }
              catch (Exception)
              {
              }
            }
            TVDatabase.SetChannelNumber(strChannel,iNumber);
            TVDatabase.SetChannelFrequency(strChannel,lFreq.ToString());
            TVDatabase.SetChannelSort(strChannel,i);
          }
        }
        catch(Exception)
        {
        }
      }

      try
      {
        Utils.FileDelete(@"capturecards.xml");
        using (Stream s = File.Open(@"capturecards.xml", FileMode.CreateNew, FileAccess.ReadWrite))
        {
          SoapFormatter b = new SoapFormatter();
          b.Serialize(s, m_tvcards);
          s.Close();
        }
      }
      catch(Exception)
      {
      }
    }


    void SetupCapture()
    {
      using ( AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        string strRecPath=xmlreader.GetValueAsString("capture","recordingpath","");
        string strTunerType=xmlreader.GetValueAsString("capture","tuner","Antenna");

        UpDownPreRecording.Value =xmlreader.GetValueAsInt("capture","prerecord", 5);
        UpDownPostRecording.Value=xmlreader.GetValueAsInt("capture","postrecord", 5);

        upDownCountry.Value=xmlreader.GetValueAsInt("capture","country",31);
        if (strRecPath=="") 
        {
          strRecPath=String.Format(@"{0}\My Videos",Environment.GetFolderPath( Environment.SpecialFolder.Personal).ToString());
        }
        if (strTunerType=="Antenna") btnradioAntenna.Checked=true;
        else btnradioCable.Checked=true;

        textBoxRecPath.Text=strRecPath;

        // setup tv channel list
        listTVChannels.Items.Clear();
        
        ListViewItem newItem;
        
        ArrayList channels = new ArrayList();
        TVDatabase.GetChannels(ref channels);
        foreach (TVChannel chan in channels)
        {
          newItem = listTVChannels.Items.Add(chan.Name);
          newItem.SubItems.Add(chan.Number.ToString());
          double dFreq=(double)chan.Frequency;
          if (chan.Number>=254) dFreq=0;
          dFreq /=1000000d;
          string strFreq=dFreq.ToString();
          newItem.SubItems.Add(strFreq);
        }
      }
      try
      {
        m_tvcards.Clear();
        using (Stream r = File.Open(@"capturecards.xml", FileMode.Open, FileAccess.Read))
        {
          SoapFormatter c = new SoapFormatter();
          m_tvcards = (ArrayList)c.Deserialize(r);
          r.Close();
        } 
      }
      catch(Exception)
      {
      }
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
        comboDVDAudioRenderer.Enabled=true;
      }
      else
      {
        dvdFile.Enabled=true;
        dvdbtnSelect.Enabled=true;
        dvdParams.Enabled=true;
        comboBoxAudioLanguage.Enabled=false;
        comboBoxSubtitleLanguage.Enabled=false;
        comboDVDAudioRenderer.Enabled=false;
      }
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
      ArrayList channels=new ArrayList();
      TVDatabase.GetChannels(ref channels); 
      int iChan=0;
			foreach (TVChannel chan in channels)
			{
				string strTagChan=String.Format("channel{0}",iChan);
				try
				{
					if (chan.Number <254)
					{
            if (chan.Frequency>0)
            {
              UInt32 dwFreq = (UInt32)chan.Frequency;
              string strKey=chan.Number.ToString();
              hklm.SetValue(strKey,(Int32)dwFreq);
              hklm2.SetValue(strKey,(Int32)dwFreq);
            }
					}
				}
				catch (Exception)
				{
				}
        ++iChan;
			}
			hklm.Close();
		}

    private void btnDelChannel_Click(object sender, System.EventArgs e)
    {
      if (listTVChannels.SelectedItems.Count==0) return;
			DialogResult result=MessageBox.Show(this.Parent,"Are you sure to delete this channel?", "Delete channel",MessageBoxButtons.YesNo);
			if (result==DialogResult.Yes)
			{
				while( listTVChannels.SelectedIndices.Count>0)
				{
					int iItem=listTVChannels.SelectedIndices[0];
					string strChannel=listTVChannels.Items[iItem].Text;
					listTVChannels.Items.RemoveAt(iItem);
					TVDatabase.RemoveChannel(strChannel);
				}
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

    private void listTVChannels_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
    {
      if (iColumn==e.Column)
        m_bAscending=!m_bAscending;
      iColumn=e.Column;
      listTVChannels.ListViewItemSorter = new ListViewItemComparer(e.Column,m_bAscending);
      listTVChannels.Sort();
      listTVChannels.ListViewItemSorter=null;


    }


    private void listAudioShares_SubItemClicked(object sender, ListViewEx.SubItemClickEventArgs e)
    {
      if (e.SubItem!=2) return;
      foreach (ListViewItem item in listAudioShares.Items)
      {
        item.SubItems[2].Text="";
      }
      e.Item.SubItems[2].Text="X";
    }
    private void listVideoShares_SubItemClicked(object sender, ListViewEx.SubItemClickEventArgs e)
    {
      if (e.SubItem!=2) return;
      foreach (ListViewItem item in listVideoShares.Items)
      {
        item.SubItems[2].Text="";
      }
      e.Item.SubItems[2].Text="X";
    }
    private void listPictureShares_SubItemClicked(object sender, ListViewEx.SubItemClickEventArgs e)
    {
      if (e.SubItem!=2) return;
      foreach (ListViewItem item in listPictureShares.Items)
      {
        item.SubItems[2].Text="";
      }
      e.Item.SubItems[2].Text="X";
    }

    private void btnXMLTVFolder_Click(object sender, System.EventArgs e)
    {
      FolderBrowserDialog dlg=new FolderBrowserDialog();
      dlg.ShowNewFolderButton=true;
      dlg.ShowDialog(this);
      if (dlg.SelectedPath==null) return;
      textBoxXMLTVFolder.Text=dlg.SelectedPath;
    }

    void SetupDVDNavigator(ComboBox box,string strDVDNavigator)
    {
      if (strDVDNavigator=="")
      {
        strDVDNavigator="DVD Navigator";
      }
      box.Items.Clear();
      Filters filters=new Filters();
      int iSelected=0;
      int i=0;
      foreach (Filter filter in filters.FileWriters) 
      {
        if ( String.Compare(filter.Name,"DVD Navigator",true)==0 ||
             String.Compare(filter.Name,"CyberLink DVD Navigator",true)==0)
        {
          box.Items.Add(filter.Name);
        
          if ( String.Compare(filter.Name,strDVDNavigator,true)==0)
          {
            iSelected=i;
          }
          ++i;
        }
      }
      box.SelectedIndex=iSelected;
    }

    void SetupAudioRenderer(ComboBox box,string strDVDAudioRenderer)
    {
      
      if (strDVDAudioRenderer=="")
      {
        strDVDAudioRenderer="Default DirectSound Device";
      }
      box.Items.Clear();
      Filters filters=new Filters();
      int iSelected=0;
      int i=0;
      foreach (Filter audioRenderer in filters.AudioRenderers) 
      {
        box.Items.Add(audioRenderer.Name);
        if ( String.Compare(audioRenderer.Name,strDVDAudioRenderer,true)==0)
        {
          iSelected=i;
        }
        ++i;
      }
      box.SelectedIndex=iSelected;
    }
    private void listCaptureCards_SubItemClicked(object sender, ListViewEx.SubItemClickEventArgs e)
    {
      if (listCaptureCards.SelectedItems.Count==0) return;
      int iItem=listCaptureCards.SelectedIndices[0];
      if (iItem < 0 || iItem >=m_tvcards.Count) return; 
      if (e.SubItem<=0) return;
      TVCaptureDevice dev = (TVCaptureDevice)m_tvcards[iItem];
      if (e.SubItem==1)
      {
        dev.UseForTV=!dev.UseForTV;
      }
      if (e.SubItem==2)
      {
        dev.UseForRecording=!dev.UseForRecording;
      }
      UpdateCaptureCardList();
    }

    private void UpdateCaptureCardList()
    {
      listCaptureCards.Items.Clear();
      foreach (TVCaptureDevice dev in m_tvcards)
      {
        ListViewItem item = listCaptureCards.Items.Add(dev.ToString());
        if (dev.UseForTV) 
          item.SubItems.Add("X");
        else
          item.SubItems.Add(" ");

        if (dev.UseForRecording) 
          item.SubItems.Add("X");
        else
          item.SubItems.Add(" ");
      }
    }
    private void btnAddCaptureDevice_Click(object sender, System.EventArgs e)
    {
      FormCapture dlg = new FormCapture();
      dlg.ShowDialog(this.Parent);
      if (dlg.VideoDevice!="")
      {
        TVCaptureDevice dev = new TVCaptureDevice();
        dev.UseForRecording=dlg.UseForRecording;
        dev.UseForTV=dlg.UseForTV;
        dev.AudioCompressor=dlg.AudioCompressor;
        dev.VideoCompressor=dlg.VideoCompressor;
        dev.AudioDevice=dlg.AudioDevice;
        dev.VideoDevice=dlg.VideoDevice;
        dev.CaptureFormat=dlg.CaptureFormat;
        dev.FrameRate=dlg.FrameRate;
        dev.FrameSize=dlg.FrameSize;
        m_tvcards.Add(dev);
        UpdateCaptureCardList();
      }
    }

    private void btnEditCaptureDevice_Click(object sender, System.EventArgs e)
    {
      FormCapture dlg = new FormCapture();
      if (listCaptureCards.SelectedItems.Count==0) return;
      int iItem=listCaptureCards.SelectedIndices[0];
      if (iItem<0 || iItem >= m_tvcards.Count) return;
      TVCaptureDevice dev=(TVCaptureDevice)m_tvcards[iItem];
      dlg.UseForRecording=dev.UseForRecording;
      dlg.UseForTV=dev.UseForTV;
      dlg.AudioCompressor=dev.AudioCompressor;
      dlg.VideoCompressor=dev.VideoCompressor;
      dlg.AudioDevice=dev.AudioDevice;
      dlg.VideoDevice=dev.VideoDevice;
      dlg.CaptureFormat=dev.CaptureFormat;
      dlg.FrameRate=dev.FrameRate;
      dlg.FrameSize=dev.FrameSize;

      dlg.ShowDialog(this.Parent);
      if (dlg.VideoDevice!="")
      {
        dev.UseForRecording=dlg.UseForRecording;
        dev.UseForTV=dlg.UseForTV;
        dev.AudioCompressor=dlg.AudioCompressor;
        dev.VideoCompressor=dlg.VideoCompressor;
        dev.AudioDevice=dlg.AudioDevice;
        dev.VideoDevice=dlg.VideoDevice;
        dev.CaptureFormat=dlg.CaptureFormat;
        dev.FrameRate=dlg.FrameRate;
        dev.FrameSize=dlg.FrameSize;

        UpdateCaptureCardList();
      }
    }

    private void btnDelCaptureDevice_Click(object sender, System.EventArgs e)
    {
      if (listCaptureCards.SelectedItems.Count==0) return;
      int iItem=listCaptureCards.SelectedIndices[0];
      DialogResult result=MessageBox.Show(this.Parent,"Are you sure to delete this device?", "Delete device",MessageBoxButtons.YesNo);
      if (result==DialogResult.Yes)
      {
        m_tvcards.RemoveAt(iItem);
        UpdateCaptureCardList();
      }
    }

    private void listCaptureCards_DoubleClick(object sender, System.EventArgs e)
    {
      btnEditCaptureDevice_Click(null,null);    
    }

    private void btnPlayListFolder_Click(object sender, System.EventArgs e)
    {
      FolderBrowserDialog dlg=new FolderBrowserDialog();
      dlg.ShowNewFolderButton=true;
      dlg.ShowDialog(this);
      if (dlg.SelectedPath==null) return;
      textBoxPlayLists.Text=dlg.SelectedPath;
    }

    private void btnPlayListVideo_Click(object sender, System.EventArgs e)
    {
      FolderBrowserDialog dlg=new FolderBrowserDialog();
      dlg.ShowNewFolderButton=true;
      dlg.ShowDialog(this);
      if (dlg.SelectedPath==null) return;
      textBoxPlayListFolderVideo.Text=dlg.SelectedPath;
    }

    private void buttonAutoTune_Click(object sender, System.EventArgs e)
    {
      SaveSettings();
      FormAutoTune autoTuneDlg = new FormAutoTune();
      autoTuneDlg.ShowDialog(this.Parent);
      LoadSettings();
      SetupCapture();
      UpdateCaptureCardList();
    }

    private void buttonAutoTune2_Click(object sender, System.EventArgs e)
    {
      buttonAutoTune_Click(sender,e);
    }
	}

  // Implements the manual sorting of items by columns.
  class ListViewItemComparer : IComparer
  {
    private int col;
    private bool m_bAscending;
    public ListViewItemComparer()
    {
      col = 0;
      m_bAscending=true;
    }
    public ListViewItemComparer(int column, bool bAscending)
    {
      col = column;
      m_bAscending=bAscending;
    }
    public int Compare(object x, object y)
    {
			
      try
      {
        if (m_bAscending)
        {
          int i1=Convert.ToInt32(((ListViewItem)x).SubItems[col].Text);
          int i2=Convert.ToInt32(((ListViewItem)y).SubItems[col].Text);
          if (i1>i2) return 1;
          if (i1<i2) return -1;
          return 0;
        }
        else
        {
          int i2=Convert.ToInt32(((ListViewItem)x).SubItems[col].Text);
          int i1=Convert.ToInt32(((ListViewItem)y).SubItems[col].Text);
          if (i1>i2) return 1;
          if (i1<i2) return -1;
          return 0;
        }
      }
      catch(Exception)
      {
      }
      return 0;
    }
  }

}
