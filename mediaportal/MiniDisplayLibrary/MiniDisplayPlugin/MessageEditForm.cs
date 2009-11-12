using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin
{
  public class MessageEditForm : MPConfigForm
  {
    private bool _CharacterDrawMode;
    private string[] AlignmentList = new string[] {"Left", "Centered", "Right"};
    private MPBeveledLine beveledLine1;
    private MPButton buttonCancel;
    private MPButton buttonDefault;
    private MPButton buttonDown;
    private MPButton buttonNew;
    private MPButton buttonOk;
    private MPButton buttonRemove;
    private MPButton buttonUp;
    private CheckBox cbR0B0;
    private CheckBox cbR0B1;
    private CheckBox cbR0B2;
    private CheckBox cbR0B3;
    private CheckBox cbR0B4;
    private CheckBox cbR0B5;
    private CheckBox cbR0B6;
    private CheckBox cbR0B7;
    private CheckBox cbR1B0;
    private CheckBox cbR1B1;
    private CheckBox cbR1B2;
    private CheckBox cbR1B3;
    private CheckBox cbR1B4;
    private CheckBox cbR1B5;
    private CheckBox cbR1B6;
    private CheckBox cbR1B7;
    private CheckBox cbR2B0;
    private CheckBox cbR2B1;
    private CheckBox cbR2B2;
    private CheckBox cbR2B3;
    private CheckBox cbR2B4;
    private CheckBox cbR2B5;
    private CheckBox cbR2B6;
    private CheckBox cbR2B7;
    private CheckBox cbR3B0;
    private CheckBox cbR3B1;
    private CheckBox cbR3B2;
    private CheckBox cbR3B3;
    private CheckBox cbR3B4;
    private CheckBox cbR3B5;
    private CheckBox cbR3B6;
    private CheckBox cbR3B7;
    private CheckBox cbR4B0;
    private CheckBox cbR4B1;
    private CheckBox cbR4B2;
    private CheckBox cbR4B3;
    private CheckBox cbR4B4;
    private CheckBox cbR4B5;
    private CheckBox cbR4B6;
    private CheckBox cbR4B7;
    private CheckBox cbR5B0;
    private CheckBox cbR5B1;
    private CheckBox cbR5B2;
    private CheckBox cbR5B3;
    private CheckBox cbR5B4;
    private CheckBox cbR5B5;
    private CheckBox cbR5B6;
    private CheckBox cbR5B7;
    private CheckBox cbR6B0;
    private CheckBox cbR6B1;
    private CheckBox cbR6B2;
    private CheckBox cbR6B3;
    private CheckBox cbR6B4;
    private CheckBox cbR6B5;
    private CheckBox cbR6B6;
    private CheckBox cbR6B7;
    private CheckBox cbR7B0;
    private CheckBox cbR7B1;
    private CheckBox cbR7B2;
    private CheckBox cbR7B3;
    private CheckBox cbR7B4;
    private CheckBox cbR7B5;
    private CheckBox cbR7B6;
    private CheckBox cbR7B7;
    private bool changedSettings;
    private TreeNode CharacterEditNode;
    private MPComboBox comboBoxAlignment;
    private MPComboBox ComboBoxCondType;
    private MPComboBox comboBoxMessageType;
    private MPComboBox comboBoxProcessType;
    private MPComboBox ComboBoxStatusProperty;
    private MPComboBox comboBoxWindowProperty;
    private Container components = null;
    private string[] ConditionList = new string[] {"And", "IsNull", "NotNull", "Or"};
    private GroupBox groupboxCharacterEdit;
    private MPGroupBox GroupBoxCondition;
    private MPGroupBox groupBoxLine;
    private GroupBox groupBoxMessageEdit;
    private MPGroupBox groupBoxMessageType;
    private MPGroupBox groupBoxProcess;
    private MPGroupBox groupBoxStatus;
    private MPGroupBox groupBoxTextProgressBar;
    private GroupBox groupboxTranslationEdit;
    private MPGroupBox groupBoxWindow;
    private MPGradientLabel headerLabel;
    private MPLabel label1;
    private MPLabel labelExpand;
    private MPLabel labelLine;
    private string[] MessageTypeList = new string[] {"Line", "Image"};
    private MPLabel mpLabel1;
    private MPLabel mpLabel10;
    private MPLabel mpLabel11;
    private MPLabel mpLabel12;
    private MPLabel mpLabel13;
    private MPLabel mpLabel2;
    private MPLabel mpLabel3;
    private MPLabel mpLabel4;
    private MPLabel mpLabel5;
    private MPLabel mpLabel6;
    private MPLabel mpLabel7;
    private MPLabel mpLabel8;
    private MPLabel mpLabel9;
    private MPTextBox mpTextBoxTargetProperty;
    private MPTextBox mpTextBoxValueProperty;
    private MPTextBox mpTPBEndChar;
    private MPTextBox mpTPBFillChar;
    private MPTextBox mpTPBlength;
    private MPTextBox mpTPBStartChar;
    private MPTextBox mpTPBValueChar;
    private Array nativeWindowsList = Enum.GetValues(typeof (GUIWindow.Window));

    private string[] ProcessList = new string[]
                                     {
                                       "FixedValue", "Parse", "Performance Counter", "Property", "Text",
                                       "TextProgressBar"
                                     };

    private string[] ProcessListImage = new string[] {"Parse", "Property", "Text", "X", "Y"};
    private ArrayList propertyList = new ArrayList();
    private TreeNode SettingsNode;

    private string[] StatusList = new string[]
                                    {
                                      "ALL STATES", "Action", "Idle", "PlayingDVD", "PlayingMusic", "PlayingRadio",
                                      "PlayingTV", "PlayingRecording", "PlayingVideo", "TimeShifting", "Dialog"
                                    };

    private MPTextBox textBoxCondValue;
    private MPTextBox textBoxProcessValue;
    private MPTextBox TextBoxTranslateFrom;
    private MPTextBox TextBoxTranslateTo;
    private TreeView treeMapping;
    private ArrayList windowsList = new ArrayList();
    private ArrayList windowsListFiltered = new ArrayList();

    public MessageEditForm()
    {
      this.InitializeComponent();
      foreach (GUIWindow.Window window in this.nativeWindowsList)
      {
        if (window.ToString().IndexOf("DIALOG") == -1)
        {
          switch (((int) Enum.Parse(typeof (GUIWindow.Window), window.ToString())))
          {
            case 100:
            case 0x65:
            case 5:
            case -1:
            case 0x1fc:
            case 0x266:
            case 0x2f6:
            case 0x3ea:
            case 0x3eb:
            case 0x2f9:
            case 0x7d0:
            case 0x7d1:
            case 0x7d2:
            case 0x7d3:
            case 0x7d8:
            case 0x7d9:
            case 0x7da:
            case 0x7db:
            case 0x7dc:
            case 0x7dd:
            case 0x7de:
            case 0x7df:
            case 0x7e0:
            case 0xb55:
            case 0xb56:
            case 0xbbb:
            case 0xbbd:
            case 0xbbe:
            case 0xbbf:
            case 0xbc1:
            case 0xbc3:
            case 0x1edc:
              goto Label_0343;
          }
          this.windowsListFiltered.Add(this.GetFriendlyName(window.ToString()));
        }
        Label_0343:
        this.windowsList.Add(this.GetFriendlyName(window.ToString()));
      }
      this.windowsList.Add("ALL WINDOWS");
      this.propertyList.Clear();
      this.propertyList.Add("#date");
      this.propertyList.Add("#time");
      this.propertyList.Add("#Day");
      this.propertyList.Add("#SDOW");
      this.propertyList.Add("#DOW");
      this.propertyList.Add("#Month");
      this.propertyList.Add("#SMOY");
      this.propertyList.Add("#MOY");
      this.propertyList.Add("#SY");
      this.propertyList.Add("#Year");
      this.propertyList.Add("#highlightedbutton");
      this.propertyList.Add("#itemcount");
      this.propertyList.Add("#selecteditem");
      this.propertyList.Add("#selecteditem2");
      this.propertyList.Add("#selectedthumb");
      this.propertyList.Add("#homedate");
      this.propertyList.Add("#title");
      this.propertyList.Add("#paused");
      this.propertyList.Add("#artist");
      this.propertyList.Add("#album");
      this.propertyList.Add("#track");
      this.propertyList.Add("#year");
      this.propertyList.Add("#comment");
      this.propertyList.Add("#director");
      this.propertyList.Add("#genre");
      this.propertyList.Add("#cast");
      this.propertyList.Add("#dvdlabel");
      this.propertyList.Add("#imdbnumber");
      this.propertyList.Add("#file");
      this.propertyList.Add("#plot");
      this.propertyList.Add("#plotoutline");
      this.propertyList.Add("#rating");
      this.propertyList.Add("#tagline");
      this.propertyList.Add("#votes");
      this.propertyList.Add("#credits");
      this.propertyList.Add("#mpaarating");
      this.propertyList.Add("#runtime");
      this.propertyList.Add("#iswatched");
      this.propertyList.Add("#thumb");
      this.propertyList.Add("#currentplaytime");
      this.propertyList.Add("#currentremaining");
      this.propertyList.Add("#shortcurrentremaining");
      this.propertyList.Add("#shortcurrentplaytime");
      this.propertyList.Add("#duration");
      this.propertyList.Add("#shortduration");
      this.propertyList.Add("#playlogo");
      this.propertyList.Add("#playspeed");
      this.propertyList.Add("#percentage");
      this.propertyList.Add("#currentmodule");
      this.propertyList.Add("#currentmoduleid");
      this.propertyList.Add("#currentmodulefullscreenstate");
      this.propertyList.Add("#channel");
      this.propertyList.Add("#TV.start");
      this.propertyList.Add("#TV.stop");
      this.propertyList.Add("#TV.current");
      this.propertyList.Add("#TV.Record.channel");
      this.propertyList.Add("#TV.Record.start");
      this.propertyList.Add("#TV.Record.stop");
      this.propertyList.Add("#TV.Record.genre");
      this.propertyList.Add("#TV.Record.title");
      this.propertyList.Add("#TV.Record.description");
      this.propertyList.Add("#TV.Record.thumb");
      this.propertyList.Add("#TV.Record.percent1");
      this.propertyList.Add("#TV.Record.percent2");
      this.propertyList.Add("#TV.Record.percent3");
      this.propertyList.Add("#TV.Record.current");
      this.propertyList.Add("#TV.Record.duration");
      this.propertyList.Add("#TV.View.channel");
      this.propertyList.Add("#TV.View.thumb");
      this.propertyList.Add("#TV.View.start");
      this.propertyList.Add("#TV.View.stop");
      this.propertyList.Add("#TV.View.remaining");
      this.propertyList.Add("#TV.View.genre");
      this.propertyList.Add("#TV.View.title");
      this.propertyList.Add("#TV.View.compositetitle");
      this.propertyList.Add("#TV.View.description");
      this.propertyList.Add("#TV.View.Percentage");
      this.propertyList.Add("#TV.Next.start");
      this.propertyList.Add("#TV.Next.stop");
      this.propertyList.Add("#TV.Next.genre");
      this.propertyList.Add("#TV.Next.title");
      this.propertyList.Add("#TV.Next.description");
      this.propertyList.Add("#TV.Guide.Day");
      this.propertyList.Add("#TV.Guide.thumb");
      this.propertyList.Add("#TV.Guide.Title");
      this.propertyList.Add("#TV.Guide.CompositeTitle");
      this.propertyList.Add("#TV.Guide.Time");
      this.propertyList.Add("#TV.Guide.Duration");
      this.propertyList.Add("#TV.Guide.TimeFromNow");
      this.propertyList.Add("#TV.Guide.Description");
      this.propertyList.Add("#TV.Guide.Genre");
      this.propertyList.Add("#TV.Guide.EpisodeName");
      this.propertyList.Add("#TV.Guide.SeriesNumber");
      this.propertyList.Add("#TV.Guide.EpisodeNumber");
      this.propertyList.Add("#TV.Guide.EpisodePart");
      this.propertyList.Add("#TV.Guide.EpisodeDetail");
      this.propertyList.Add("#TV.Guide.Date");
      this.propertyList.Add("#TV.Guide.StarRating");
      this.propertyList.Add("#TV.Guide.Classification");
      this.propertyList.Add("#TV.Guide.Group");
      this.propertyList.Add("#Radio.Guide.Day");
      this.propertyList.Add("#Radio.Guide.thumb");
      this.propertyList.Add("#Radio.Guide.Title");
      this.propertyList.Add("#Radio.Guide.Time");
      this.propertyList.Add("#Radio.Guide.Duration");
      this.propertyList.Add("#Radio.Guide.TimeFromNow");
      this.propertyList.Add("#Radio.Guide.Description");
      this.propertyList.Add("#Radio.Guide.Genre");
      this.propertyList.Add("#Radio.Guide.EpisodeName");
      this.propertyList.Add("#Radio.Guide.SeriesNumber");
      this.propertyList.Add("#Radio.Guide.EpisodeNumber");
      this.propertyList.Add("#Radio.Guide.EpisodePart");
      this.propertyList.Add("#Radio.Guide.EpisodeDetail");
      this.propertyList.Add("#Radio.Guide.Date");
      this.propertyList.Add("#Radio.Guide.StarRating");
      this.propertyList.Add("#Radio.Guide.Classification");
      this.propertyList.Add("#TV.RecordedTV.Title");
      this.propertyList.Add("#TV.RecordedTV.Time");
      this.propertyList.Add("#TV.RecordedTV.Description");
      this.propertyList.Add("#TV.RecordedTV.thumb");
      this.propertyList.Add("#TV.RecordedTV.Genre");
      this.propertyList.Add("#TV.Signal.Quality");
      this.propertyList.Add("#TV.Scheduled.Title");
      this.propertyList.Add("#TV.Scheduled.Time");
      this.propertyList.Add("#TV.Scheduled.Description");
      this.propertyList.Add("#TV.Scheduled.thumb");
      this.propertyList.Add("#TV.Scheduled.Genre");
      this.propertyList.Add("#TV.Scheduled.Channel");
      this.propertyList.Add("#TV.Search.Title");
      this.propertyList.Add("#TV.Search.Time");
      this.propertyList.Add("#TV.Search.Description");
      this.propertyList.Add("#TV.Search.thumb");
      this.propertyList.Add("#TV.Search.Genre");
      this.propertyList.Add("#view");
      this.propertyList.Add("#TV.Transcoding.Percentage");
      this.propertyList.Add("#TV.Transcoding.File");
      this.propertyList.Add("#TV.Transcoding.Title");
      this.propertyList.Add("#TV.Transcoding.Genre");
      this.propertyList.Add("#TV.Transcoding.Description");
      this.propertyList.Add("#TV.Transcoding.Channel");
      this.propertyList.Add("#Play.Current.Thumb");
      this.propertyList.Add("#Play.Current.File");
      this.propertyList.Add("#Play.Current.Title");
      this.propertyList.Add("#Play.Current.Genre");
      this.propertyList.Add("#Play.Current.Comment");
      this.propertyList.Add("#Play.Current.Artist");
      this.propertyList.Add("#Play.Current.Director");
      this.propertyList.Add("#Play.Current.Album");
      this.propertyList.Add("#Play.Current.Track");
      this.propertyList.Add("#Play.Current.Year");
      this.propertyList.Add("#Play.Current.Duration");
      this.propertyList.Add("#Play.Current.Plot");
      this.propertyList.Add("#Play.Current.PlotOutline");
      this.propertyList.Add("#Play.Current.Channel");
      this.propertyList.Add("#Play.Current.Cast");
      this.propertyList.Add("#Play.Current.DVDLabel");
      this.propertyList.Add("#Play.Current.IMDBNumber");
      this.propertyList.Add("#Play.Current.Rating");
      this.propertyList.Add("#Play.Current.TagLine");
      this.propertyList.Add("#Play.Current.Votes");
      this.propertyList.Add("#Play.Current.Credits");
      this.propertyList.Add("#Play.Current.Runtime");
      this.propertyList.Add("#Play.Current.MPAARating");
      this.propertyList.Add("#Play.Current.IsWatched");
      this.propertyList.Add("#Play.Current.ArtistThumb");
      this.propertyList.Add("#Play.Current.Lastfm.TrackTags");
      this.propertyList.Add("#Play.Current.Lastfm.SimilarArtists");
      this.propertyList.Add("#Play.Current.Lastfm.ArtistInfo");
      this.propertyList.Add("#Play.Current.Lastfm.CurrentStream");
      this.propertyList.Add("#Play.Next.Thumb");
      this.propertyList.Add("#Play.Next.File");
      this.propertyList.Add("#Play.Next.Title");
      this.propertyList.Add("#Play.Next.CompositeTitle");
      this.propertyList.Add("#Play.Next.Genre");
      this.propertyList.Add("#Play.Next.Comment");
      this.propertyList.Add("#Play.Next.Artist");
      this.propertyList.Add("#Play.Next.Director");
      this.propertyList.Add("#Play.Next.Album");
      this.propertyList.Add("#Play.Next.Track");
      this.propertyList.Add("#Play.Next.Year");
      this.propertyList.Add("#Play.Next.Duration");
      this.propertyList.Add("#Play.Next.Plot");
      this.propertyList.Add("#Play.Next.PlotOutline");
      this.propertyList.Add("#Play.Next.Channel");
      this.propertyList.Add("#Play.Next.Cast");
      this.propertyList.Add("#Play.Next.DVDLabel");
      this.propertyList.Add("#Play.Next.IMDBNumber");
      this.propertyList.Add("#Play.Next.Rating");
      this.propertyList.Add("#Play.Next.TagLine");
      this.propertyList.Add("#Play.Next.Votes");
      this.propertyList.Add("#Play.Next.Credits");
      this.propertyList.Add("#Play.Next.Runtime");
      this.propertyList.Add("#Play.Next.MPAARating");
      this.propertyList.Add("#Play.Next.IsWatched");
      this.propertyList.Add("#Lastfm.Rating.AlbumTrack1");
      this.propertyList.Add("#Lastfm.Rating.AlbumTrack2");
      this.propertyList.Add("#Lastfm.Rating.AlbumTrack3");
      this.propertyList.Add("#numberplace.time");
      this.propertyList.Add("#numberplace.name1");
      this.propertyList.Add("#numberplace.name2");
      this.propertyList.Add("#numberplace.name3");
      this.propertyList.Add("#numberplace.name4");
      this.propertyList.Add("#numberplace.name5");
      this.propertyList.Add("#numberplace.score1");
      this.propertyList.Add("#numberplace.score2");
      this.propertyList.Add("#numberplace.score3");
      this.propertyList.Add("#numberplace.score4");
      this.propertyList.Add("#numberplace.score5");
      this.propertyList.Add("#facadeview.viewmode");
      this.propertyList.Add("#cur2rentmodule");
      this.propertyList.Add("#currentsleeptime");
      this.propertyList.Add("#burner_title");
      this.propertyList.Add("#burner_perc");
      this.propertyList.Add("#burner_size");
      this.propertyList.Add("#burner_info");
      this.propertyList.Add("#convert_info");
      this.propertyList.Add("#curheader");
      this.propertyList.Add("#trackduration");
      this.propertyList.Add("#fps");
      this.propertyList.Add("#WizardCountry");
      this.propertyList.Add("#WizardCountryCode");
      this.propertyList.Add("#WizardCity");
      this.propertyList.Add("#WizardCityUrl");
      this.propertyList.Add("#Wizard.Analog.Done");
      this.propertyList.Add("#Wizard.ATSC.Done");
      this.propertyList.Add("#Wizard.DVBC.Done");
      this.propertyList.Add("#Wizard.DVBS.Done");
      this.propertyList.Add("#Wizard.DVBT.Done");
      this.propertyList.Add("#Wizard.EPG.Done");
      this.propertyList.Add("#Wizard.Remote.Done");
      this.propertyList.Add("#Wizard.General.Done");
      this.propertyList.Add("#InternetAccess");
      this.propertyList.Add("#tetris_score");
      this.propertyList.Add("#tetris_lines");
      this.propertyList.Add("#tetris_level");
      this.propertyList.Add("#tetris_highscore");
      this.propertyList.Add("#Actor.Name");
      this.propertyList.Add("#Actor.DateOfBirth");
      this.propertyList.Add("#Actor.PlaceOfBirth");
      this.propertyList.Add("#Actor.Biography");
      this.propertyList.Add("#Actor.Movies");
      this.LoadMessages(false);
    }

    private void buttonApply_Click(object sender, EventArgs e)
    {
      if (this.changedSettings)
      {
        this.SaveSettings();
      }
    }

    private void buttonDefault_Click(object sender, EventArgs e)
    {
      this.LoadMessages(true);
    }

    private void buttonDown_Click(object sender, EventArgs e)
    {
      bool isExpanded = false;
      TreeNode selectedNode = this.treeMapping.SelectedNode;
      isExpanded = selectedNode.IsExpanded;
      if ((((((Data) selectedNode.Tag).Type == "MESSAGE") || (((Data) selectedNode.Tag).Type == "LINE")) ||
           ((((Data) selectedNode.Tag).Type == "IMAGE") || (((Data) selectedNode.Tag).Type == "PROCESS"))) ||
          ((((Data) selectedNode.Tag).Type == "CONDITION") || (((Data) selectedNode.Tag).Type == "SUBCONDITION")))
      {
        if (selectedNode.Index < (selectedNode.Parent.Nodes.Count - 1))
        {
          int index = selectedNode.Index + 1;
          TreeNode node = (TreeNode) selectedNode.Clone();
          TreeNode parent = selectedNode.Parent;
          selectedNode.Remove();
          if (isExpanded)
          {
            node.Expand();
          }
          parent.Nodes.Insert(index, node);
          this.treeMapping.SelectedNode = node;
        }
        this.treeMapping_AfterSelect(this,
                                     new TreeViewEventArgs(this.treeMapping.SelectedNode, TreeViewAction.ByKeyboard));
        this.changedSettings = true;
      }
    }

    private void buttonNew_Click(object sender, EventArgs e)
    {
      TreeNode parent;
      TreeNode node3;
      Log.Info("buttonNew_Click(): ADDING NEW NODE");
      TreeNode selectedNode = this.treeMapping.SelectedNode;
      Data tag = (Data) selectedNode.Tag;
      TreeNode node = new TreeNode("Idle (NEW MESSAGE)");
      node.Tag = new Data("MESSAGE", "STATUS", "Idle");
      TreeNode node5 = new TreeNode("WINDOWS");
      node5.Tag = new Data("WINDOWLIST", "", "");
      TreeNode node6 = new TreeNode("All Windows");
      node6.Tag = new Data("WINDOW", "ALL", "");
      TreeNode node7 = new TreeNode("LINE Alignment = Centered");
      node7.Tag = new Data("LINE", "ALIGNMENT", "Centered");
      TreeNode node8 = new TreeNode("Parse");
      node8.Tag = new Data("PROCESS", "Parse", "");
      TreeNode node9 = new TreeNode("NotNull: ");
      node9.Tag = new Data("CONDITION", "NotNull", "");
      TreeNode node10 = new TreeNode("NotNull: ");
      node10.Tag = new Data("SUBCONDITION", "NotNull", "");
      TreeNode node11 = new TreeNode("Translation: \"x\" = \"x\"");
      node11.Tag = new Data("TRANSLATION", "x", "x");
      switch (tag.Type)
      {
        case "SECTION":
          switch (((string) tag.Parameter))
          {
            case "STATUSMESSAGES":
              {
                TreeNode node12 = new TreeNode("LINE Alignment = Centered");
                node12.Tag = new Data("LINE", "ALIGNMENT", "Centered");
                Log.Info("buttonNew_Click(): adding new MESSAGE node");
                node5.Nodes.Add(node6);
                node.Nodes.Add(node5);
                node.Nodes.Add(node7);
                node.Nodes.Add(node12);
                node.ExpandAll();
                selectedNode.Nodes.Add(node);
                this.treeMapping.SelectedNode = node;
                break;
              }
            case "CHARACTERTRANSLATIONS":
              Log.Info("buttonNew_Click(): adding new MESSAGE node");
              selectedNode.Nodes.Add(node11);
              this.treeMapping.SelectedNode = node11;
              break;

            case "CUSTOMCHARACTERS":
              parent = selectedNode;
              if (parent.Nodes.Count <= 7)
              {
                node3 = new TreeNode("Character #" + parent.Nodes.Count.ToString());
                node3.Tag = new Data("CHARACTER", "", parent.Nodes.Count);
                for (int i = 0; i < 8; i++)
                {
                  TreeNode node13 = new TreeNode("Byte #" + i.ToString());
                  node13.Tag = new Data("BYTE", i, (byte) 0);
                  node3.Nodes.Add(node13);
                }
                node3.ExpandAll();
                parent.Nodes.Add(node3);
                this.treeMapping.SelectedNode = node3;
              }
              break;
          }
          break;

        case "TRANSLATION":
          Log.Info("buttonNew_Click(): adding new TRANSLATION node");
          selectedNode.Parent.Nodes.Add(node11);
          this.treeMapping.SelectedNode = node11;
          break;

        case "BYTE":
        case "CHARACTER":
          parent = selectedNode.Parent;
          if (tag.Type == "BYTE")
          {
            parent = selectedNode.Parent.Parent;
          }
          if (parent.Nodes.Count <= 7)
          {
            node3 = new TreeNode("Character #" + parent.Nodes.Count.ToString());
            node3.Tag = new Data("CHARACTER", "", parent.Nodes.Count);
            for (int j = 0; j < 8; j++)
            {
              TreeNode node14 = new TreeNode("Byte #" + j.ToString());
              node14.Tag = new Data("BYTE", j, (byte) 0);
              node3.Nodes.Add(node14);
            }
            node3.ExpandAll();
            parent.Nodes.Add(node3);
            this.treeMapping.SelectedNode = node3;
          }
          break;

        case "MESSAGE":
          Log.Info("buttonNew_Click(): adding new LINE node");
          selectedNode.Nodes.Add(node7);
          this.treeMapping.SelectedNode = node6;
          break;

        case "WINDOWLIST":
          Log.Info("buttonNew_Click(): adding new WINDOW node");
          selectedNode.Nodes.Add(node6);
          this.treeMapping.SelectedNode = node6;
          break;

        case "WINDOW":
          Log.Info("buttonNew_Click(): adding new WINDOW node");
          selectedNode.Parent.Nodes.Add(node6);
          node6.Expand();
          this.treeMapping.SelectedNode = node6;
          break;

        case "LINE":
          Log.Info("buttonNew_Click(): adding new PROCESS node");
          selectedNode.Nodes.Add(node8);
          this.treeMapping.SelectedNode = node8;
          break;

        case "IMAGE":
          Log.Info("buttonNew_Click(): adding new PROCESS node");
          selectedNode.Nodes.Add(node8);
          this.treeMapping.SelectedNode = node8;
          break;

        case "PROCESS":
          Log.Info("buttonNew_Click(): adding new CONDITION node");
          selectedNode.Nodes.Add(node9);
          this.treeMapping.SelectedNode = node9;
          break;

        case "CONDITION":
          string str3;
          if (((str3 = (string) tag.Parameter) == null) || (!(str3 == "And") && !(str3 == "Or")))
          {
            Log.Info("buttonNew_Click(): adding new CONDITION node");
            selectedNode.Parent.Nodes.Add(node9);
            this.treeMapping.SelectedNode = node9;
            break;
          }
          Log.Info("buttonNew_Click(): adding new SUBCONDITION node");
          selectedNode.Nodes.Add(node10);
          this.treeMapping.SelectedNode = node10;
          break;

        case "SUBCONDITION":
          Log.Info("buttonNew_Click(): adding new SUBCONDITION node");
          selectedNode.Parent.Nodes.Add(node10);
          this.treeMapping.SelectedNode = node10;
          break;

        default:
          Log.Info("buttonNew_Click(): Unknown node \"{0}\"", new object[] {tag.Type});
          break;
      }
      this.changedSettings = true;
      this.treeMapping_AfterSelect(this, new TreeViewEventArgs(this.treeMapping.SelectedNode, TreeViewAction.ByKeyboard));
    }

    private void buttonOk_Click(object sender, EventArgs e)
    {
      if (this.changedSettings)
      {
        this.SaveSettings();
      }
      base.Close();
    }

    private void buttonRemove_Click(object sender, EventArgs e)
    {
      TreeNode selectedNode = this.treeMapping.SelectedNode;
      Data tag = (Data) selectedNode.Tag;
      if (((tag.Type == "COMMAND") || (tag.Type == "SOUND")) || (tag.Type == "CONDITION"))
      {
        selectedNode = this.getNode("CONDITION");
        tag = (Data) selectedNode.Tag;
      }
      if (
        MessageBox.Show(this, "Are you sure you want to remove this " + tag.Type.ToLower() + "?",
                        "Remove " + tag.Type.ToLower(), MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2) == DialogResult.Yes)
      {
        selectedNode.Remove();
        this.changedSettings = true;
      }
    }

    private void buttonUp_Click(object sender, EventArgs e)
    {
      bool isExpanded = false;
      TreeNode selectedNode = this.treeMapping.SelectedNode;
      isExpanded = selectedNode.IsExpanded;
      if ((((((Data) selectedNode.Tag).Type == "MESSAGE") || (((Data) selectedNode.Tag).Type == "LINE")) ||
           ((((Data) selectedNode.Tag).Type == "IMAGE") || (((Data) selectedNode.Tag).Type == "PROCESS"))) ||
          ((((Data) selectedNode.Tag).Type == "CONDITION") || (((Data) selectedNode.Tag).Type == "SUBCONDITION")))
      {
        if (selectedNode.Index > 0)
        {
          int index = selectedNode.Index - 1;
          TreeNode node = (TreeNode) selectedNode.Clone();
          TreeNode parent = selectedNode.Parent;
          if (((Data) selectedNode.Parent.Nodes[index].Tag).Type == "WINDOWLIST")
          {
            return;
          }
          selectedNode.Remove();
          if (isExpanded)
          {
            node.Expand();
          }
          parent.Nodes.Insert(index, node);
          this.treeMapping.SelectedNode = node;
        }
        this.treeMapping_AfterSelect(this,
                                     new TreeViewEventArgs(this.treeMapping.SelectedNode, TreeViewAction.ByKeyboard));
        this.changedSettings = true;
      }
    }

    private void CharBitmap_CheckedChanged(object sender, EventArgs e)
    {
      if (!this._CharacterDrawMode)
      {
        string name = ((CheckBox) sender).Name;
        int row = int.Parse(name.Substring(3, 1));
        int num2 = int.Parse(name.Substring(5, 1));
        if (((CheckBox) sender).CheckState == CheckState.Checked)
        {
          ((CheckBox) sender).CheckState = CheckState.Indeterminate;
        }
        Log.Info("CharBitmap_CheckedChanged(): bit changed - row = {0}, bit = {1}", new object[] {row, num2});
        byte newValue = 0;
        byte num4 = 0x80;
        for (int i = 7; i >= 0; i--)
        {
          if (this.GetCharacterPixel(row, i))
          {
            newValue = (byte) (newValue + num4);
          }
          num4 = (byte) (num4 >> 1);
        }
        if (this.CharacterEditNode != null)
        {
          this.CharacterEditNode.Nodes[row].Tag = new Data("BYTE", row, newValue);
          if (newValue < 0x10)
          {
            this.CharacterEditNode.Nodes[row].Text = "Byte " + row.ToString() + ": 0x0" + newValue.ToString("x00");
          }
          else
          {
            this.CharacterEditNode.Nodes[row].Text = "Byte " + row.ToString() + ": 0x" + newValue.ToString("x00");
          }
          this.changedSettings = true;
          Log.Info("CharBitmap_CheckedChanged(): new row value = {0}", new object[] {newValue});
        }
      }
    }

    private void CleanAbbreviation(ref string name, string abbreviation)
    {
      int index = name.ToUpper().IndexOf(abbreviation.ToUpper());
      if (index != -1)
      {
        name = name.Substring(0, index) + abbreviation + name.Substring(index + abbreviation.Length);
      }
    }

    private void CloseThread()
    {
      Thread.Sleep(200);
      base.Close();
    }

    private void comboBoxAlignment_SelectionChangeCommitted(object sender, EventArgs e)
    {
      TreeNode node = this.getNode("LINE");
      Data tag = (Data) node.Tag;
      Log.Info(
        "comboBoxAlignment_SelectionChangeCommitted(): DATA - Type = {0}, Parameter = {1}, Value = {2} - New Window = {3}",
        new object[] {tag.Type, tag.Parameter, tag.Value, this.comboBoxAlignment.SelectedItem.ToString()});
      node.Tag = new Data("LINE", "ALIGNMENT", this.comboBoxAlignment.SelectedItem);
      node.Text = "LINE Alignment = " + ((string) this.comboBoxAlignment.SelectedItem);
      Data data2 = (Data) node.Tag;
      Log.Info("comboBoxAlignment_SelectionChangeCommitted(): NEW DATA - Type = {0}, Parameter = {1}, Value = {2}",
               new object[] {data2.Type, data2.Parameter, data2.Value});
      this.changedSettings = true;
    }

    private void ComboBoxCondType_SelectionChangeCommitted(object sender, EventArgs e)
    {
      TreeNode node = this.getNode("CONDITION");
      Data tag = (Data) node.Tag;
      string parameter = (string) tag.Parameter;
      if (parameter != null)
      {
        if (!(parameter == "AND") && !(parameter == "OR"))
        {
          if ((parameter == "ISNULL") || (parameter == "NOTNULL"))
          {
            node.Tag = new Data("CONDITION", (string) this.ComboBoxCondType.SelectedItem, this.textBoxCondValue.Text);
            node.Text = (string) this.ComboBoxCondType.SelectedItem;
          }
        }
        else
        {
          node.Tag = new Data("CONDITION", (string) this.ComboBoxCondType.SelectedItem, string.Empty);
          node.Text = (string) this.comboBoxWindowProperty.SelectedItem;
          this.textBoxCondValue.Text = string.Empty;
        }
      }
      this.changedSettings = true;
    }

    private void comboBoxMessageType_SelectedIndexChanged(object sender, EventArgs e)
    {
      TreeNode selectedNode = this.treeMapping.SelectedNode;
      Data tag = (Data) selectedNode.Tag;
      Log.Info(
        "comboBoxMessageType_SelectionChangeCommitted(): EXISTING TYPE = {0}, Parameter = {1}, Value = {2} - New Window = {3}",
        new object[] {tag.Type});
      string str = this.comboBoxMessageType.SelectedItem.ToString().ToLower();
      if (str != null)
      {
        if (!(str == "line"))
        {
          if ((str == "image") && (tag.Type.ToLower() != "image"))
          {
            selectedNode.Tag = new Data("IMAGE", "", "");
            selectedNode.Text = "IMAGE";
          }
        }
        else if (tag.Type.ToLower() != "line")
        {
          selectedNode.Tag = new Data("LINE", "ALIGNMENT", this.comboBoxAlignment.SelectedItem);
          selectedNode.Text = "LINE Alignment = " + ((string) this.comboBoxAlignment.SelectedItem);
        }
      }
      Data data2 = (Data) selectedNode.Tag;
      Log.Info("comboBoxMessageType_SelectionChangeCommitted(): NEW DATA - Type = {0}, Parameter = {1}, Value = {2}",
               new object[] {data2.Type, data2.Parameter, data2.Value});
      this.changedSettings = true;
    }

    private void comboBoxProcessType_SelectionChangeCommitted(object sender, EventArgs e)
    {
      Data data2;
      string str;
      this.textBoxProcessValue.Enabled = true;
      this.textBoxProcessValue.Text = string.Empty;
      this.groupBoxTextProgressBar.Enabled = false;
      this.groupBoxTextProgressBar.Visible = false;
      TreeNode node = this.getNode("PROCESS");
      Data tag = (Data) node.Tag;
      if (((str = (string) tag.Parameter) != null) && ((str == "AND") || (str == "OR")))
      {
        Log.Info(
          "comboBoxProcessType_SelectionChangeCommitted():     DATA - Type = {0}, Parameter = {1}, Value = {2} - New Window = {3}",
          new object[] {tag.Type, tag.Parameter, tag.Value, this.ComboBoxStatusProperty.SelectedItem.ToString()});
        node.Tag = new Data("PROCESS", this.comboBoxProcessType.SelectedItem, string.Empty);
        node.Text = (string) this.comboBoxProcessType.SelectedItem;
        this.textBoxProcessValue.Text = string.Empty;
        data2 = (Data) node.Tag;
        Log.Info("comboBoxProcessType_SelectionChangeCommitted(): NEW DATA - Type = {0}, Parameter = {1}, Value = {2}",
                 new object[] {data2.Type, data2.Parameter, data2.Value});
      }
      else if (this.comboBoxProcessType.Text.Equals("TextProgressBar"))
      {
        Log.Info(
          "comboBoxProcessType_SelectionChangeCommitted(): TPB DATA - Type = {0}, Parameter = {1}, Value = {2} - New Window = {3}",
          new object[] {tag.Type, tag.Parameter, tag.Value, this.ComboBoxStatusProperty.SelectedItem.ToString()});
        this.groupBoxTextProgressBar.Enabled = true;
        this.groupBoxTextProgressBar.Visible = true;
        node.Tag = new Data("PROCESS", this.comboBoxProcessType.SelectedItem, "[|]|*|-|8|#currentplaytime|#duration");
        this.mpTPBStartChar.Text = "[";
        this.mpTPBEndChar.Text = "]";
        this.mpTPBValueChar.Text = "*";
        this.mpTPBFillChar.Text = "-";
        this.mpTPBlength.Text = "8";
        this.mpTextBoxValueProperty.Text = "#currentplaytime";
        this.mpTextBoxTargetProperty.Text = "#duration";
        node.Text = (string) this.comboBoxProcessType.SelectedItem;
        data2 = (Data) node.Tag;
        Log.Info("comboBoxProcessType_SelectionChangeCommitted(): NEW DATA - Type = {0}, Parameter = {1}, Value = {2}",
                 new object[] {data2.Type, data2.Parameter, data2.Value});
      }
      else
      {
        Log.Info(
          "comboBoxProcessType_SelectionChangeCommitted():     DATA - Type = {0}, Parameter = {1}, Value = {2} - New Window = {3}",
          new object[] {tag.Type, tag.Parameter, tag.Value, this.ComboBoxStatusProperty.SelectedItem.ToString()});
        node.Tag = new Data("PROCESS", this.comboBoxProcessType.SelectedItem, this.textBoxProcessValue.Text);
        node.Text = ((string) this.comboBoxProcessType.SelectedItem) + " - " + this.textBoxProcessValue.Text;
        data2 = (Data) node.Tag;
        Log.Info("comboBoxProcessType_SelectionChangeCommitted(): NEW DATA - Type = {0}, Parameter = {1}, Value = {2}",
                 new object[] {data2.Type, data2.Parameter, data2.Value});
      }
      this.changedSettings = true;
    }

    private void ComboBoxStatusProperty_SelectionChangeCommitted(object sender, EventArgs e)
    {
      TreeNode node = this.getNode("MESSAGE");
      Data tag = (Data) node.Tag;
      Log.Info(
        "ComboBoxStatusProperty_SelectionChangeCommitted(): DATA - Type = {0}, Parameter = {1}, Value = {2} - New Window = {3}",
        new object[] {tag.Type, tag.Parameter, tag.Value, this.ComboBoxStatusProperty.SelectedItem.ToString()});
      node.Tag = new Data("MESSAGE", "STATUS", this.ComboBoxStatusProperty.SelectedItem);
      node.Text = (string) this.ComboBoxStatusProperty.SelectedItem;
      Data data2 = (Data) node.Tag;
      Log.Info(
        "ComboBoxStatusProperty_SelectionChangeCommitted(): NEW DATA - Type = {0}, Parameter = {1}, Value = {2}",
        new object[] {data2.Type, data2.Parameter, data2.Value});
      this.changedSettings = true;
    }

    private void comboBoxWindowProperty_SelectionChangeCommitted(object sender, EventArgs e)
    {
      string str;
      TreeNode node = this.getNode("WINDOW");
      Data tag = (Data) node.Tag;
      Log.Info(
        "comboBoxWindowProperty_SelectionChangeCommitted(): DATA - Type = {0}, Parameter = {1}, Value = {2} - New Window = {3}",
        new object[] {tag.Type, tag.Parameter, tag.Value, this.comboBoxWindowProperty.SelectedItem.ToString()});
      if (((str = this.comboBoxWindowProperty.SelectedItem.ToString()) != null) && (str == "ALL WINDOWS"))
      {
        node.Tag = new Data("WINDOW", "ALL", "");
        node.Text = "ALL WINDOWS";
      }
      else
      {
        Log.Info(": finding window \"{0}\" = \"{1}\"",
                 new object[]
                   {
                     this.comboBoxWindowProperty.SelectedItem.ToString(),
                     this.GetWindowName(this.comboBoxWindowProperty.SelectedItem.ToString())
                   });
        int num =
          (int)
          Enum.Parse(typeof (GUIWindow.Window), this.GetWindowName(this.comboBoxWindowProperty.SelectedItem.ToString()));
        node.Tag = new Data("WINDOW", "ID",
                            (int)
                            Enum.Parse(typeof (GUIWindow.Window),
                                       this.GetWindowName(this.comboBoxWindowProperty.SelectedItem.ToString())));
        node.Text = num.ToString();
        Data data2 = (Data) node.Tag;
        Log.Info(
          "comboBoxWindowProperty_SelectionChangeCommitted(): NEW DATA - Type = {0}, Parameter = {1}, Value = {2} ({3})",
          new object[] {data2.Type, data2.Parameter, data2.Value, num});
      }
      this.changedSettings = true;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && (this.components != null))
      {
        this.components.Dispose();
      }
      base.Dispose(disposing);
    }

    private void DrawCustomCharacter(TreeNode CharBaseNode)
    {
      this._CharacterDrawMode = true;
      Log.Info("DrawCustomCharacter(): called");
      if (CharBaseNode.Nodes.Count != 8)
      {
        Log.Info("DrawCustomCharacter(): Invalid character data");
      }
      for (int i = 0; i < 8; i++)
      {
        TreeNode node = CharBaseNode.Nodes[i];
        Data tag = (Data) node.Tag;
        byte num2 = (byte) tag.Value;
        byte num3 = 0x80;
        for (int j = 7; j >= 0; j--)
        {
          if ((num2 & num3) > 0)
          {
            this.SetCharacterPixel(i, j, true);
          }
          else
          {
            this.SetCharacterPixel(i, j, false);
          }
          num3 = (byte) (num3 >> 1);
        }
      }
      Log.Info("DrawCustomCharacter(): completed");
      this._CharacterDrawMode = false;
    }

    private string GetActionName(string friendlyName)
    {
      string str = string.Empty;
      try
      {
        if (Enum.Parse(typeof (Action.ActionType), "ACTION_" + friendlyName.Replace(' ', '_').ToUpper()) != null)
        {
          str = "ACTION_" + friendlyName.Replace(' ', '_').ToUpper();
        }
      }
      catch (ArgumentException)
      {
        try
        {
          if (Enum.Parse(typeof (Action.ActionType), friendlyName.Replace(' ', '_').ToUpper()) != null)
          {
            str = friendlyName.Replace(' ', '_').ToUpper();
          }
          return str;
        }
        catch (ArgumentException)
        {
          return str;
        }
      }
      return str;
    }

    private bool GetCharacterPixel(int Row, int Column)
    {
      string key = "cbR" + Row.ToString().Trim() + "B" + Column.ToString().Trim();
      Control[] controlArray = this.groupboxCharacterEdit.Controls.Find(key, false);
      if (controlArray.Length > 0)
      {
        CheckBox box = (CheckBox) controlArray[0];
        if (box.CheckState == CheckState.Unchecked)
        {
          return false;
        }
        return true;
      }
      Log.Info("CONTROL \"{0}\" NOT FOUND", new object[] {key});
      return false;
    }

    private string GetFriendlyName(string name)
    {
      if ((name.IndexOf("ACTION") != -1) || (name.IndexOf("WINDOW") != -1))
      {
        name = name.Substring(7);
      }
      bool flag = true;
      string str = string.Empty;
      foreach (char ch in name)
      {
        if (ch == '_')
        {
          str = str + " ";
          flag = true;
        }
        else if (flag)
        {
          str = str + ch.ToString();
          flag = false;
        }
        else
        {
          str = str + ch.ToString().ToLower();
        }
      }
      this.CleanAbbreviation(ref str, "TV");
      this.CleanAbbreviation(ref str, "DVD");
      this.CleanAbbreviation(ref str, "UI");
      this.CleanAbbreviation(ref str, "Guide");
      this.CleanAbbreviation(ref str, "MSN");
      this.CleanAbbreviation(ref str, "OSD");
      this.CleanAbbreviation(ref str, "LCD");
      this.CleanAbbreviation(ref str, "EPG");
      this.CleanAbbreviation(ref str, "DVBC");
      this.CleanAbbreviation(ref str, "DVBS");
      this.CleanAbbreviation(ref str, "DVBT");
      return str;
    }

    private TreeNode getNode(string type)
    {
      TreeNode selectedNode = this.treeMapping.SelectedNode;
      Data tag = (Data) selectedNode.Tag;
      if (tag.Type == type)
      {
        return selectedNode;
      }
      string str = type;
      if (str != null)
      {
        if (!(str == "COMMAND"))
        {
          if (str == "SOUND")
          {
            if ((tag.Type == "COMMAND") || (tag.Type == "KEY"))
            {
              selectedNode = selectedNode.Parent;
              foreach (TreeNode node4 in selectedNode.Nodes)
              {
                tag = (Data) node4.Tag;
                if (tag.Type == type)
                {
                  return node4;
                }
              }
            }
            else if (tag.Type == "CONDITION")
            {
              foreach (TreeNode node5 in selectedNode.Nodes)
              {
                tag = (Data) node5.Tag;
                if (tag.Type == type)
                {
                  return node5;
                }
              }
            }
          }
          else if (str == "CONDITION")
          {
            if (((tag.Type == "SOUND") || (tag.Type == "COMMAND")) || (tag.Type == "KEY"))
            {
              return selectedNode.Parent;
            }
          }
          else if (str == "LAYER")
          {
            if (((tag.Type == "SOUND") || (tag.Type == "COMMAND")) || (tag.Type == "KEY"))
            {
              return selectedNode.Parent.Parent;
            }
            if (tag.Type == "CONDITION")
            {
              return selectedNode.Parent;
            }
          }
          else if (str == "BUTTON")
          {
            if (((tag.Type == "SOUND") || (tag.Type == "COMMAND")) || (tag.Type == "KEY"))
            {
              return selectedNode.Parent.Parent.Parent;
            }
            if (tag.Type == "CONDITION")
            {
              return selectedNode.Parent.Parent;
            }
            if (tag.Type == "LAYER")
            {
              return selectedNode.Parent;
            }
          }
          else if (str == "REMOTE")
          {
            if (((tag.Type == "SOUND") || (tag.Type == "COMMAND")) || (tag.Type == "KEY"))
            {
              return selectedNode.Parent.Parent.Parent.Parent;
            }
            if (tag.Type == "CONDITION")
            {
              return selectedNode.Parent.Parent.Parent;
            }
            if (tag.Type == "LAYER")
            {
              return selectedNode.Parent.Parent;
            }
            if (tag.Type == "BUTTON")
            {
              return selectedNode.Parent;
            }
          }
        }
        else if ((tag.Type == "SOUND") || (tag.Type == "KEY"))
        {
          selectedNode = selectedNode.Parent;
          foreach (TreeNode node2 in selectedNode.Nodes)
          {
            tag = (Data) node2.Tag;
            if (tag.Type == type)
            {
              return node2;
            }
          }
        }
        else if (tag.Type == "CONDITION")
        {
          foreach (TreeNode node3 in selectedNode.Nodes)
          {
            tag = (Data) node3.Tag;
            if (tag.Type == type)
            {
              return node3;
            }
          }
        }
      }
      return null;
    }

    private string GetWindowName(int WindowID)
    {
      return this.GetWindowName(this.GetFriendlyName((string) this.windowsList[WindowID]));
    }

    private string GetWindowName(string friendlyName)
    {
      return ("WINDOW_" + friendlyName.Replace(' ', '_').ToUpper());
    }

    private void InitializeComponent()
    {
      this.treeMapping = new System.Windows.Forms.TreeView();
      this.labelExpand = new MediaPortal.UserInterface.Controls.MPLabel();
      this.buttonDefault = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonRemove = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonDown = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonUp = new MediaPortal.UserInterface.Controls.MPButton();
      this.beveledLine1 = new MediaPortal.UserInterface.Controls.MPBeveledLine();
      this.buttonOk = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.headerLabel = new MediaPortal.UserInterface.Controls.MPGradientLabel();
      this.buttonNew = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBoxMessageEdit = new System.Windows.Forms.GroupBox();
      this.groupBoxProcess = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.groupBoxTextProgressBar = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpTPBlength = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.mpLabel12 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpTPBFillChar = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.mpLabel10 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpTPBValueChar = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.mpLabel11 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpTPBEndChar = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.mpLabel9 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpTPBStartChar = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.mpLabel8 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpTextBoxValueProperty = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.mpLabel6 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel7 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpTextBoxTargetProperty = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.GroupBoxCondition = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.mpLabel2 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.mpLabel3 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxCondValue = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.ComboBoxCondType = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxProcessValue = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.comboBoxProcessType = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.groupBoxMessageType = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.comboBoxMessageType = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.mpLabel13 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupBoxStatus = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.ComboBoxStatusProperty = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.groupBoxWindow = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.comboBoxWindowProperty = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.groupBoxLine = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.comboBoxAlignment = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.labelLine = new MediaPortal.UserInterface.Controls.MPLabel();
      this.groupboxCharacterEdit = new System.Windows.Forms.GroupBox();
      this.cbR7B0 = new System.Windows.Forms.CheckBox();
      this.cbR7B1 = new System.Windows.Forms.CheckBox();
      this.cbR7B2 = new System.Windows.Forms.CheckBox();
      this.cbR7B3 = new System.Windows.Forms.CheckBox();
      this.cbR7B4 = new System.Windows.Forms.CheckBox();
      this.cbR7B5 = new System.Windows.Forms.CheckBox();
      this.cbR7B6 = new System.Windows.Forms.CheckBox();
      this.cbR7B7 = new System.Windows.Forms.CheckBox();
      this.cbR6B0 = new System.Windows.Forms.CheckBox();
      this.cbR6B1 = new System.Windows.Forms.CheckBox();
      this.cbR6B2 = new System.Windows.Forms.CheckBox();
      this.cbR6B3 = new System.Windows.Forms.CheckBox();
      this.cbR6B4 = new System.Windows.Forms.CheckBox();
      this.cbR6B5 = new System.Windows.Forms.CheckBox();
      this.cbR6B6 = new System.Windows.Forms.CheckBox();
      this.cbR6B7 = new System.Windows.Forms.CheckBox();
      this.cbR5B0 = new System.Windows.Forms.CheckBox();
      this.cbR5B1 = new System.Windows.Forms.CheckBox();
      this.cbR5B2 = new System.Windows.Forms.CheckBox();
      this.cbR5B3 = new System.Windows.Forms.CheckBox();
      this.cbR5B4 = new System.Windows.Forms.CheckBox();
      this.cbR5B5 = new System.Windows.Forms.CheckBox();
      this.cbR5B6 = new System.Windows.Forms.CheckBox();
      this.cbR5B7 = new System.Windows.Forms.CheckBox();
      this.cbR4B0 = new System.Windows.Forms.CheckBox();
      this.cbR4B1 = new System.Windows.Forms.CheckBox();
      this.cbR4B2 = new System.Windows.Forms.CheckBox();
      this.cbR4B3 = new System.Windows.Forms.CheckBox();
      this.cbR4B4 = new System.Windows.Forms.CheckBox();
      this.cbR4B5 = new System.Windows.Forms.CheckBox();
      this.cbR4B6 = new System.Windows.Forms.CheckBox();
      this.cbR4B7 = new System.Windows.Forms.CheckBox();
      this.cbR3B0 = new System.Windows.Forms.CheckBox();
      this.cbR3B1 = new System.Windows.Forms.CheckBox();
      this.cbR3B2 = new System.Windows.Forms.CheckBox();
      this.cbR3B3 = new System.Windows.Forms.CheckBox();
      this.cbR3B4 = new System.Windows.Forms.CheckBox();
      this.cbR3B5 = new System.Windows.Forms.CheckBox();
      this.cbR3B6 = new System.Windows.Forms.CheckBox();
      this.cbR3B7 = new System.Windows.Forms.CheckBox();
      this.cbR2B0 = new System.Windows.Forms.CheckBox();
      this.cbR2B1 = new System.Windows.Forms.CheckBox();
      this.cbR2B2 = new System.Windows.Forms.CheckBox();
      this.cbR2B3 = new System.Windows.Forms.CheckBox();
      this.cbR2B4 = new System.Windows.Forms.CheckBox();
      this.cbR2B5 = new System.Windows.Forms.CheckBox();
      this.cbR2B6 = new System.Windows.Forms.CheckBox();
      this.cbR2B7 = new System.Windows.Forms.CheckBox();
      this.cbR1B0 = new System.Windows.Forms.CheckBox();
      this.cbR1B1 = new System.Windows.Forms.CheckBox();
      this.cbR1B2 = new System.Windows.Forms.CheckBox();
      this.cbR1B3 = new System.Windows.Forms.CheckBox();
      this.cbR1B4 = new System.Windows.Forms.CheckBox();
      this.cbR1B5 = new System.Windows.Forms.CheckBox();
      this.cbR1B6 = new System.Windows.Forms.CheckBox();
      this.cbR1B7 = new System.Windows.Forms.CheckBox();
      this.cbR0B0 = new System.Windows.Forms.CheckBox();
      this.cbR0B1 = new System.Windows.Forms.CheckBox();
      this.cbR0B2 = new System.Windows.Forms.CheckBox();
      this.cbR0B3 = new System.Windows.Forms.CheckBox();
      this.cbR0B4 = new System.Windows.Forms.CheckBox();
      this.cbR0B5 = new System.Windows.Forms.CheckBox();
      this.cbR0B6 = new System.Windows.Forms.CheckBox();
      this.cbR0B7 = new System.Windows.Forms.CheckBox();
      this.groupboxTranslationEdit = new System.Windows.Forms.GroupBox();
      this.mpLabel5 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.TextBoxTranslateTo = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.mpLabel4 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.TextBoxTranslateFrom = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.groupBoxMessageEdit.SuspendLayout();
      this.groupBoxProcess.SuspendLayout();
      this.groupBoxTextProgressBar.SuspendLayout();
      this.GroupBoxCondition.SuspendLayout();
      this.groupBoxMessageType.SuspendLayout();
      this.groupBoxStatus.SuspendLayout();
      this.groupBoxWindow.SuspendLayout();
      this.groupBoxLine.SuspendLayout();
      this.groupboxCharacterEdit.SuspendLayout();
      this.groupboxTranslationEdit.SuspendLayout();
      this.SuspendLayout();
      // 
      // treeMapping
      // 
      this.treeMapping.AllowDrop = true;
      this.treeMapping.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)));
      this.treeMapping.FullRowSelect = true;
      this.treeMapping.HideSelection = false;
      this.treeMapping.Location = new System.Drawing.Point(16, 56);
      this.treeMapping.Name = "treeMapping";
      this.treeMapping.Size = new System.Drawing.Size(312, 335);
      this.treeMapping.TabIndex = 1;
      this.treeMapping.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeMapping_AfterSelect);
      // 
      // labelExpand
      // 
      this.labelExpand.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.labelExpand.AutoSize = true;
      this.labelExpand.Location = new System.Drawing.Point(328, 374);
      this.labelExpand.Name = "labelExpand";
      this.labelExpand.Size = new System.Drawing.Size(13, 13);
      this.labelExpand.TabIndex = 29;
      this.labelExpand.Text = "+";
      this.labelExpand.Click += new System.EventHandler(this.labelExpand_Click);
      // 
      // buttonDefault
      // 
      this.buttonDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonDefault.Location = new System.Drawing.Point(345, 442);
      this.buttonDefault.Name = "buttonDefault";
      this.buttonDefault.Size = new System.Drawing.Size(75, 23);
      this.buttonDefault.TabIndex = 28;
      this.buttonDefault.Text = "Reset";
      this.buttonDefault.UseVisualStyleBackColor = true;
      this.buttonDefault.Click += new System.EventHandler(this.buttonDefault_Click);
      // 
      // buttonRemove
      // 
      this.buttonRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonRemove.Enabled = false;
      this.buttonRemove.Location = new System.Drawing.Point(272, 397);
      this.buttonRemove.Name = "buttonRemove";
      this.buttonRemove.Size = new System.Drawing.Size(56, 20);
      this.buttonRemove.TabIndex = 27;
      this.buttonRemove.Text = "Remove";
      this.buttonRemove.UseVisualStyleBackColor = true;
      this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
      // 
      // buttonDown
      // 
      this.buttonDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonDown.Enabled = false;
      this.buttonDown.Location = new System.Drawing.Point(97, 397);
      this.buttonDown.Name = "buttonDown";
      this.buttonDown.Size = new System.Drawing.Size(56, 20);
      this.buttonDown.TabIndex = 24;
      this.buttonDown.Text = "Down";
      this.buttonDown.UseVisualStyleBackColor = true;
      this.buttonDown.Click += new System.EventHandler(this.buttonDown_Click);
      // 
      // buttonUp
      // 
      this.buttonUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonUp.Enabled = false;
      this.buttonUp.Location = new System.Drawing.Point(16, 397);
      this.buttonUp.Name = "buttonUp";
      this.buttonUp.Size = new System.Drawing.Size(56, 20);
      this.buttonUp.TabIndex = 23;
      this.buttonUp.Text = "Up";
      this.buttonUp.UseVisualStyleBackColor = true;
      this.buttonUp.Click += new System.EventHandler(this.buttonUp_Click);
      // 
      // beveledLine1
      // 
      this.beveledLine1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.beveledLine1.Location = new System.Drawing.Point(8, 432);
      this.beveledLine1.Name = "beveledLine1";
      this.beveledLine1.Size = new System.Drawing.Size(572, 2);
      this.beveledLine1.TabIndex = 21;
      // 
      // buttonOk
      // 
      this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonOk.Location = new System.Drawing.Point(426, 442);
      this.buttonOk.Name = "buttonOk";
      this.buttonOk.Size = new System.Drawing.Size(75, 23);
      this.buttonOk.TabIndex = 19;
      this.buttonOk.Text = "OK";
      this.buttonOk.UseVisualStyleBackColor = true;
      this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
      // 
      // buttonCancel
      // 
      this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonCancel.Location = new System.Drawing.Point(505, 442);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(75, 23);
      this.buttonCancel.TabIndex = 18;
      this.buttonCancel.Text = "Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
      // 
      // headerLabel
      // 
      this.headerLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.headerLabel.Caption = "MiniDisplay.xml";
      this.headerLabel.FirstColor = System.Drawing.SystemColors.InactiveCaption;
      this.headerLabel.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.headerLabel.LastColor = System.Drawing.Color.WhiteSmoke;
      this.headerLabel.Location = new System.Drawing.Point(16, 16);
      this.headerLabel.Name = "headerLabel";
      this.headerLabel.PaddingLeft = 2;
      this.headerLabel.Size = new System.Drawing.Size(558, 24);
      this.headerLabel.TabIndex = 17;
      this.headerLabel.TextColor = System.Drawing.Color.WhiteSmoke;
      this.headerLabel.TextFont = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      // 
      // buttonNew
      // 
      this.buttonNew.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonNew.Enabled = false;
      this.buttonNew.Location = new System.Drawing.Point(189, 397);
      this.buttonNew.Name = "buttonNew";
      this.buttonNew.Size = new System.Drawing.Size(56, 20);
      this.buttonNew.TabIndex = 26;
      this.buttonNew.Text = "New";
      this.buttonNew.UseVisualStyleBackColor = true;
      this.buttonNew.Click += new System.EventHandler(this.buttonNew_Click);
      // 
      // groupBoxMessageEdit
      // 
      this.groupBoxMessageEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxMessageEdit.Controls.Add(this.groupBoxProcess);
      this.groupBoxMessageEdit.Controls.Add(this.groupBoxMessageType);
      this.groupBoxMessageEdit.Controls.Add(this.groupBoxStatus);
      this.groupBoxMessageEdit.Controls.Add(this.groupBoxWindow);
      this.groupBoxMessageEdit.Controls.Add(this.groupBoxLine);
      this.groupBoxMessageEdit.Location = new System.Drawing.Point(344, 46);
      this.groupBoxMessageEdit.Name = "groupBoxMessageEdit";
      this.groupBoxMessageEdit.Size = new System.Drawing.Size(239, 380);
      this.groupBoxMessageEdit.TabIndex = 31;
      this.groupBoxMessageEdit.TabStop = false;
      this.groupBoxMessageEdit.Text = "Message Editor";
      this.groupBoxMessageEdit.Visible = false;
      // 
      // groupBoxProcess
      // 
      this.groupBoxProcess.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxProcess.Controls.Add(this.groupBoxTextProgressBar);
      this.groupBoxProcess.Controls.Add(this.GroupBoxCondition);
      this.groupBoxProcess.Controls.Add(this.mpLabel1);
      this.groupBoxProcess.Controls.Add(this.label1);
      this.groupBoxProcess.Controls.Add(this.textBoxProcessValue);
      this.groupBoxProcess.Controls.Add(this.comboBoxProcessType);
      this.groupBoxProcess.Enabled = false;
      this.groupBoxProcess.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxProcess.Location = new System.Drawing.Point(7, 192);
      this.groupBoxProcess.Name = "groupBoxProcess";
      this.groupBoxProcess.Size = new System.Drawing.Size(223, 184);
      this.groupBoxProcess.TabIndex = 32;
      this.groupBoxProcess.TabStop = false;
      this.groupBoxProcess.Text = "Process";
      // 
      // groupBoxTextProgressBar
      // 
      this.groupBoxTextProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxTextProgressBar.Controls.Add(this.mpTPBlength);
      this.groupBoxTextProgressBar.Controls.Add(this.mpLabel12);
      this.groupBoxTextProgressBar.Controls.Add(this.mpTPBFillChar);
      this.groupBoxTextProgressBar.Controls.Add(this.mpLabel10);
      this.groupBoxTextProgressBar.Controls.Add(this.mpTPBValueChar);
      this.groupBoxTextProgressBar.Controls.Add(this.mpLabel11);
      this.groupBoxTextProgressBar.Controls.Add(this.mpTPBEndChar);
      this.groupBoxTextProgressBar.Controls.Add(this.mpLabel9);
      this.groupBoxTextProgressBar.Controls.Add(this.mpTPBStartChar);
      this.groupBoxTextProgressBar.Controls.Add(this.mpLabel8);
      this.groupBoxTextProgressBar.Controls.Add(this.mpTextBoxValueProperty);
      this.groupBoxTextProgressBar.Controls.Add(this.mpLabel6);
      this.groupBoxTextProgressBar.Controls.Add(this.mpLabel7);
      this.groupBoxTextProgressBar.Controls.Add(this.mpTextBoxTargetProperty);
      this.groupBoxTextProgressBar.Enabled = false;
      this.groupBoxTextProgressBar.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxTextProgressBar.Location = new System.Drawing.Point(11, 47);
      this.groupBoxTextProgressBar.Name = "groupBoxTextProgressBar";
      this.groupBoxTextProgressBar.Size = new System.Drawing.Size(201, 131);
      this.groupBoxTextProgressBar.TabIndex = 33;
      this.groupBoxTextProgressBar.TabStop = false;
      this.groupBoxTextProgressBar.Text = "TextProgressBar Parameters";
      this.groupBoxTextProgressBar.Visible = false;
      // 
      // mpTPBlength
      // 
      this.mpTPBlength.BorderColor = System.Drawing.Color.Empty;
      this.mpTPBlength.Location = new System.Drawing.Point(69, 62);
      this.mpTPBlength.MaxLength = 2;
      this.mpTPBlength.Name = "mpTPBlength";
      this.mpTPBlength.Size = new System.Drawing.Size(23, 20);
      this.mpTPBlength.TabIndex = 36;
      this.mpTPBlength.TextChanged += new System.EventHandler(this.TextProgressBar_LostFocus);
      // 
      // mpLabel12
      // 
      this.mpLabel12.AutoSize = true;
      this.mpLabel12.Location = new System.Drawing.Point(6, 65);
      this.mpLabel12.Name = "mpLabel12";
      this.mpLabel12.Size = new System.Drawing.Size(62, 13);
      this.mpLabel12.TabIndex = 35;
      this.mpLabel12.Text = "Bar Length:";
      // 
      // mpTPBFillChar
      // 
      this.mpTPBFillChar.BorderColor = System.Drawing.Color.Empty;
      this.mpTPBFillChar.Location = new System.Drawing.Point(167, 39);
      this.mpTPBFillChar.MaxLength = 1;
      this.mpTPBFillChar.Name = "mpTPBFillChar";
      this.mpTPBFillChar.Size = new System.Drawing.Size(23, 20);
      this.mpTPBFillChar.TabIndex = 34;
      this.mpTPBFillChar.LostFocus += new System.EventHandler(this.TextProgressBar_LostFocus);
      // 
      // mpLabel10
      // 
      this.mpLabel10.AutoSize = true;
      this.mpLabel10.Location = new System.Drawing.Point(104, 42);
      this.mpLabel10.Name = "mpLabel10";
      this.mpLabel10.Size = new System.Drawing.Size(47, 13);
      this.mpLabel10.TabIndex = 33;
      this.mpLabel10.Text = "Fill Char:";
      // 
      // mpTPBValueChar
      // 
      this.mpTPBValueChar.BorderColor = System.Drawing.Color.Empty;
      this.mpTPBValueChar.Location = new System.Drawing.Point(69, 39);
      this.mpTPBValueChar.MaxLength = 1;
      this.mpTPBValueChar.Name = "mpTPBValueChar";
      this.mpTPBValueChar.Size = new System.Drawing.Size(23, 20);
      this.mpTPBValueChar.TabIndex = 32;
      this.mpTPBValueChar.LostFocus += new System.EventHandler(this.TextProgressBar_LostFocus);
      // 
      // mpLabel11
      // 
      this.mpLabel11.AutoSize = true;
      this.mpLabel11.Location = new System.Drawing.Point(6, 42);
      this.mpLabel11.Name = "mpLabel11";
      this.mpLabel11.Size = new System.Drawing.Size(62, 13);
      this.mpLabel11.TabIndex = 31;
      this.mpLabel11.Text = "Value Char:";
      // 
      // mpTPBEndChar
      // 
      this.mpTPBEndChar.BorderColor = System.Drawing.Color.Empty;
      this.mpTPBEndChar.Location = new System.Drawing.Point(167, 17);
      this.mpTPBEndChar.MaxLength = 1;
      this.mpTPBEndChar.Name = "mpTPBEndChar";
      this.mpTPBEndChar.Size = new System.Drawing.Size(23, 20);
      this.mpTPBEndChar.TabIndex = 30;
      this.mpTPBEndChar.LostFocus += new System.EventHandler(this.TextProgressBar_LostFocus);
      // 
      // mpLabel9
      // 
      this.mpLabel9.AutoSize = true;
      this.mpLabel9.Location = new System.Drawing.Point(104, 20);
      this.mpLabel9.Name = "mpLabel9";
      this.mpLabel9.Size = new System.Drawing.Size(54, 13);
      this.mpLabel9.TabIndex = 29;
      this.mpLabel9.Text = "End Char:";
      // 
      // mpTPBStartChar
      // 
      this.mpTPBStartChar.BorderColor = System.Drawing.Color.Empty;
      this.mpTPBStartChar.Location = new System.Drawing.Point(69, 17);
      this.mpTPBStartChar.MaxLength = 1;
      this.mpTPBStartChar.Name = "mpTPBStartChar";
      this.mpTPBStartChar.Size = new System.Drawing.Size(23, 20);
      this.mpTPBStartChar.TabIndex = 28;
      this.mpTPBStartChar.LostFocus += new System.EventHandler(this.TextProgressBar_LostFocus);
      // 
      // mpLabel8
      // 
      this.mpLabel8.AutoSize = true;
      this.mpLabel8.Location = new System.Drawing.Point(6, 20);
      this.mpLabel8.Name = "mpLabel8";
      this.mpLabel8.Size = new System.Drawing.Size(57, 13);
      this.mpLabel8.TabIndex = 27;
      this.mpLabel8.Text = "Start Char:";
      // 
      // mpTextBoxValueProperty
      // 
      this.mpTextBoxValueProperty.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpTextBoxValueProperty.BorderColor = System.Drawing.Color.Empty;
      this.mpTextBoxValueProperty.Location = new System.Drawing.Point(92, 85);
      this.mpTextBoxValueProperty.MaxLength = 100;
      this.mpTextBoxValueProperty.Name = "mpTextBoxValueProperty";
      this.mpTextBoxValueProperty.Size = new System.Drawing.Size(97, 20);
      this.mpTextBoxValueProperty.TabIndex = 26;
      this.mpTextBoxValueProperty.LostFocus += new System.EventHandler(this.TextProgressBar_LostFocus);
      // 
      // mpLabel6
      // 
      this.mpLabel6.AutoSize = true;
      this.mpLabel6.Location = new System.Drawing.Point(6, 88);
      this.mpLabel6.Name = "mpLabel6";
      this.mpLabel6.Size = new System.Drawing.Size(79, 13);
      this.mpLabel6.TabIndex = 25;
      this.mpLabel6.Text = "Value Property:";
      // 
      // mpLabel7
      // 
      this.mpLabel7.AutoSize = true;
      this.mpLabel7.Location = new System.Drawing.Point(6, 112);
      this.mpLabel7.Name = "mpLabel7";
      this.mpLabel7.Size = new System.Drawing.Size(83, 13);
      this.mpLabel7.TabIndex = 23;
      this.mpLabel7.Text = "Target Property:";
      // 
      // mpTextBoxTargetProperty
      // 
      this.mpTextBoxTargetProperty.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.mpTextBoxTargetProperty.BorderColor = System.Drawing.Color.Empty;
      this.mpTextBoxTargetProperty.Location = new System.Drawing.Point(92, 108);
      this.mpTextBoxTargetProperty.MaxLength = 100;
      this.mpTextBoxTargetProperty.Name = "mpTextBoxTargetProperty";
      this.mpTextBoxTargetProperty.Size = new System.Drawing.Size(97, 20);
      this.mpTextBoxTargetProperty.TabIndex = 22;
      this.mpTextBoxTargetProperty.LostFocus += new System.EventHandler(this.TextProgressBar_LostFocus);
      // 
      // GroupBoxCondition
      // 
      this.GroupBoxCondition.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.GroupBoxCondition.Controls.Add(this.mpLabel2);
      this.GroupBoxCondition.Controls.Add(this.mpLabel3);
      this.GroupBoxCondition.Controls.Add(this.textBoxCondValue);
      this.GroupBoxCondition.Controls.Add(this.ComboBoxCondType);
      this.GroupBoxCondition.Enabled = false;
      this.GroupBoxCondition.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.GroupBoxCondition.Location = new System.Drawing.Point(11, 78);
      this.GroupBoxCondition.Name = "GroupBoxCondition";
      this.GroupBoxCondition.Size = new System.Drawing.Size(201, 80);
      this.GroupBoxCondition.TabIndex = 32;
      this.GroupBoxCondition.TabStop = false;
      this.GroupBoxCondition.Text = "Condition";
      // 
      // mpLabel2
      // 
      this.mpLabel2.AutoSize = true;
      this.mpLabel2.Location = new System.Drawing.Point(13, 23);
      this.mpLabel2.Name = "mpLabel2";
      this.mpLabel2.Size = new System.Drawing.Size(34, 13);
      this.mpLabel2.TabIndex = 25;
      this.mpLabel2.Text = "Type:";
      // 
      // mpLabel3
      // 
      this.mpLabel3.AutoSize = true;
      this.mpLabel3.Location = new System.Drawing.Point(13, 56);
      this.mpLabel3.Name = "mpLabel3";
      this.mpLabel3.Size = new System.Drawing.Size(37, 13);
      this.mpLabel3.TabIndex = 23;
      this.mpLabel3.Text = "Value:";
      // 
      // textBoxCondValue
      // 
      this.textBoxCondValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxCondValue.BorderColor = System.Drawing.Color.Empty;
      this.textBoxCondValue.Enabled = false;
      this.textBoxCondValue.Location = new System.Drawing.Point(56, 52);
      this.textBoxCondValue.MaxLength = 100;
      this.textBoxCondValue.Name = "textBoxCondValue";
      this.textBoxCondValue.Size = new System.Drawing.Size(133, 20);
      this.textBoxCondValue.TabIndex = 22;
      this.textBoxCondValue.LostFocus += new System.EventHandler(this.textBoxCondValue_LostFocus);
      // 
      // ComboBoxCondType
      // 
      this.ComboBoxCondType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.ComboBoxCondType.BorderColor = System.Drawing.Color.Empty;
      this.ComboBoxCondType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.ComboBoxCondType.ForeColor = System.Drawing.Color.DarkGreen;
      this.ComboBoxCondType.Location = new System.Drawing.Point(53, 20);
      this.ComboBoxCondType.Name = "ComboBoxCondType";
      this.ComboBoxCondType.Size = new System.Drawing.Size(136, 21);
      this.ComboBoxCondType.Sorted = true;
      this.ComboBoxCondType.TabIndex = 14;
      this.ComboBoxCondType.SelectionChangeCommitted += new System.EventHandler(this.ComboBoxCondType_SelectionChangeCommitted);
      // 
      // mpLabel1
      // 
      this.mpLabel1.AutoSize = true;
      this.mpLabel1.Location = new System.Drawing.Point(24, 23);
      this.mpLabel1.Name = "mpLabel1";
      this.mpLabel1.Size = new System.Drawing.Size(34, 13);
      this.mpLabel1.TabIndex = 25;
      this.mpLabel1.Text = "Type:";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(24, 56);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(37, 13);
      this.label1.TabIndex = 23;
      this.label1.Text = "Value:";
      // 
      // textBoxProcessValue
      // 
      this.textBoxProcessValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxProcessValue.BorderColor = System.Drawing.Color.Empty;
      this.textBoxProcessValue.Enabled = false;
      this.textBoxProcessValue.Location = new System.Drawing.Point(67, 52);
      this.textBoxProcessValue.MaxLength = 100;
      this.textBoxProcessValue.Name = "textBoxProcessValue";
      this.textBoxProcessValue.Size = new System.Drawing.Size(133, 20);
      this.textBoxProcessValue.TabIndex = 22;
      this.textBoxProcessValue.LostFocus += new System.EventHandler(this.textBoxProcessValue_LostFocus);
      // 
      // comboBoxProcessType
      // 
      this.comboBoxProcessType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxProcessType.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxProcessType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxProcessType.ForeColor = System.Drawing.Color.DarkGreen;
      this.comboBoxProcessType.Location = new System.Drawing.Point(64, 20);
      this.comboBoxProcessType.Name = "comboBoxProcessType";
      this.comboBoxProcessType.Size = new System.Drawing.Size(136, 21);
      this.comboBoxProcessType.Sorted = true;
      this.comboBoxProcessType.TabIndex = 14;
      this.comboBoxProcessType.SelectionChangeCommitted += new System.EventHandler(this.comboBoxProcessType_SelectionChangeCommitted);
      // 
      // groupBoxMessageType
      // 
      this.groupBoxMessageType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxMessageType.Controls.Add(this.comboBoxMessageType);
      this.groupBoxMessageType.Controls.Add(this.mpLabel13);
      this.groupBoxMessageType.Enabled = false;
      this.groupBoxMessageType.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxMessageType.Location = new System.Drawing.Point(7, 105);
      this.groupBoxMessageType.Name = "groupBoxMessageType";
      this.groupBoxMessageType.Size = new System.Drawing.Size(224, 40);
      this.groupBoxMessageType.TabIndex = 35;
      this.groupBoxMessageType.TabStop = false;
      this.groupBoxMessageType.Text = "Message ";
      // 
      // comboBoxMessageType
      // 
      this.comboBoxMessageType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxMessageType.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxMessageType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxMessageType.Location = new System.Drawing.Point(80, 13);
      this.comboBoxMessageType.Name = "comboBoxMessageType";
      this.comboBoxMessageType.Size = new System.Drawing.Size(121, 21);
      this.comboBoxMessageType.TabIndex = 25;
      this.comboBoxMessageType.SelectedIndexChanged += new System.EventHandler(this.comboBoxMessageType_SelectedIndexChanged);
      // 
      // mpLabel13
      // 
      this.mpLabel13.AutoSize = true;
      this.mpLabel13.Location = new System.Drawing.Point(24, 16);
      this.mpLabel13.Name = "mpLabel13";
      this.mpLabel13.Size = new System.Drawing.Size(34, 13);
      this.mpLabel13.TabIndex = 16;
      this.mpLabel13.Text = "Type:";
      // 
      // groupBoxStatus
      // 
      this.groupBoxStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxStatus.Controls.Add(this.ComboBoxStatusProperty);
      this.groupBoxStatus.Enabled = false;
      this.groupBoxStatus.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxStatus.Location = new System.Drawing.Point(7, 13);
      this.groupBoxStatus.Name = "groupBoxStatus";
      this.groupBoxStatus.Size = new System.Drawing.Size(224, 42);
      this.groupBoxStatus.TabIndex = 34;
      this.groupBoxStatus.TabStop = false;
      this.groupBoxStatus.Text = " Status ";
      // 
      // ComboBoxStatusProperty
      // 
      this.ComboBoxStatusProperty.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.ComboBoxStatusProperty.BorderColor = System.Drawing.Color.Empty;
      this.ComboBoxStatusProperty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.ComboBoxStatusProperty.Location = new System.Drawing.Point(24, 15);
      this.ComboBoxStatusProperty.Name = "ComboBoxStatusProperty";
      this.ComboBoxStatusProperty.Size = new System.Drawing.Size(176, 21);
      this.ComboBoxStatusProperty.TabIndex = 25;
      this.ComboBoxStatusProperty.SelectionChangeCommitted += new System.EventHandler(this.ComboBoxStatusProperty_SelectionChangeCommitted);
      // 
      // groupBoxWindow
      // 
      this.groupBoxWindow.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxWindow.Controls.Add(this.comboBoxWindowProperty);
      this.groupBoxWindow.Enabled = false;
      this.groupBoxWindow.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxWindow.Location = new System.Drawing.Point(7, 59);
      this.groupBoxWindow.Name = "groupBoxWindow";
      this.groupBoxWindow.Size = new System.Drawing.Size(224, 42);
      this.groupBoxWindow.TabIndex = 31;
      this.groupBoxWindow.TabStop = false;
      this.groupBoxWindow.Text = " Window ";
      // 
      // comboBoxWindowProperty
      // 
      this.comboBoxWindowProperty.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxWindowProperty.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxWindowProperty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxWindowProperty.ForeColor = System.Drawing.Color.Blue;
      this.comboBoxWindowProperty.Location = new System.Drawing.Point(24, 15);
      this.comboBoxWindowProperty.Name = "comboBoxWindowProperty";
      this.comboBoxWindowProperty.Size = new System.Drawing.Size(176, 21);
      this.comboBoxWindowProperty.Sorted = true;
      this.comboBoxWindowProperty.TabIndex = 13;
      this.comboBoxWindowProperty.SelectionChangeCommitted += new System.EventHandler(this.comboBoxWindowProperty_SelectionChangeCommitted);
      // 
      // groupBoxLine
      // 
      this.groupBoxLine.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxLine.Controls.Add(this.comboBoxAlignment);
      this.groupBoxLine.Controls.Add(this.labelLine);
      this.groupBoxLine.Enabled = false;
      this.groupBoxLine.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxLine.Location = new System.Drawing.Point(7, 149);
      this.groupBoxLine.Name = "groupBoxLine";
      this.groupBoxLine.Size = new System.Drawing.Size(224, 40);
      this.groupBoxLine.TabIndex = 33;
      this.groupBoxLine.TabStop = false;
      this.groupBoxLine.Text = "Line";
      // 
      // comboBoxAlignment
      // 
      this.comboBoxAlignment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.comboBoxAlignment.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxAlignment.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxAlignment.Location = new System.Drawing.Point(80, 13);
      this.comboBoxAlignment.Name = "comboBoxAlignment";
      this.comboBoxAlignment.Size = new System.Drawing.Size(121, 21);
      this.comboBoxAlignment.TabIndex = 25;
      this.comboBoxAlignment.SelectionChangeCommitted += new System.EventHandler(this.comboBoxAlignment_SelectionChangeCommitted);
      // 
      // labelLine
      // 
      this.labelLine.AutoSize = true;
      this.labelLine.Location = new System.Drawing.Point(24, 16);
      this.labelLine.Name = "labelLine";
      this.labelLine.Size = new System.Drawing.Size(56, 13);
      this.labelLine.TabIndex = 16;
      this.labelLine.Text = "Alignment:";
      // 
      // groupboxCharacterEdit
      // 
      this.groupboxCharacterEdit.Controls.Add(this.cbR7B0);
      this.groupboxCharacterEdit.Controls.Add(this.cbR7B1);
      this.groupboxCharacterEdit.Controls.Add(this.cbR7B2);
      this.groupboxCharacterEdit.Controls.Add(this.cbR7B3);
      this.groupboxCharacterEdit.Controls.Add(this.cbR7B4);
      this.groupboxCharacterEdit.Controls.Add(this.cbR7B5);
      this.groupboxCharacterEdit.Controls.Add(this.cbR7B6);
      this.groupboxCharacterEdit.Controls.Add(this.cbR7B7);
      this.groupboxCharacterEdit.Controls.Add(this.cbR6B0);
      this.groupboxCharacterEdit.Controls.Add(this.cbR6B1);
      this.groupboxCharacterEdit.Controls.Add(this.cbR6B2);
      this.groupboxCharacterEdit.Controls.Add(this.cbR6B3);
      this.groupboxCharacterEdit.Controls.Add(this.cbR6B4);
      this.groupboxCharacterEdit.Controls.Add(this.cbR6B5);
      this.groupboxCharacterEdit.Controls.Add(this.cbR6B6);
      this.groupboxCharacterEdit.Controls.Add(this.cbR6B7);
      this.groupboxCharacterEdit.Controls.Add(this.cbR5B0);
      this.groupboxCharacterEdit.Controls.Add(this.cbR5B1);
      this.groupboxCharacterEdit.Controls.Add(this.cbR5B2);
      this.groupboxCharacterEdit.Controls.Add(this.cbR5B3);
      this.groupboxCharacterEdit.Controls.Add(this.cbR5B4);
      this.groupboxCharacterEdit.Controls.Add(this.cbR5B5);
      this.groupboxCharacterEdit.Controls.Add(this.cbR5B6);
      this.groupboxCharacterEdit.Controls.Add(this.cbR5B7);
      this.groupboxCharacterEdit.Controls.Add(this.cbR4B0);
      this.groupboxCharacterEdit.Controls.Add(this.cbR4B1);
      this.groupboxCharacterEdit.Controls.Add(this.cbR4B2);
      this.groupboxCharacterEdit.Controls.Add(this.cbR4B3);
      this.groupboxCharacterEdit.Controls.Add(this.cbR4B4);
      this.groupboxCharacterEdit.Controls.Add(this.cbR4B5);
      this.groupboxCharacterEdit.Controls.Add(this.cbR4B6);
      this.groupboxCharacterEdit.Controls.Add(this.cbR4B7);
      this.groupboxCharacterEdit.Controls.Add(this.cbR3B0);
      this.groupboxCharacterEdit.Controls.Add(this.cbR3B1);
      this.groupboxCharacterEdit.Controls.Add(this.cbR3B2);
      this.groupboxCharacterEdit.Controls.Add(this.cbR3B3);
      this.groupboxCharacterEdit.Controls.Add(this.cbR3B4);
      this.groupboxCharacterEdit.Controls.Add(this.cbR3B5);
      this.groupboxCharacterEdit.Controls.Add(this.cbR3B6);
      this.groupboxCharacterEdit.Controls.Add(this.cbR3B7);
      this.groupboxCharacterEdit.Controls.Add(this.cbR2B0);
      this.groupboxCharacterEdit.Controls.Add(this.cbR2B1);
      this.groupboxCharacterEdit.Controls.Add(this.cbR2B2);
      this.groupboxCharacterEdit.Controls.Add(this.cbR2B3);
      this.groupboxCharacterEdit.Controls.Add(this.cbR2B4);
      this.groupboxCharacterEdit.Controls.Add(this.cbR2B5);
      this.groupboxCharacterEdit.Controls.Add(this.cbR2B6);
      this.groupboxCharacterEdit.Controls.Add(this.cbR2B7);
      this.groupboxCharacterEdit.Controls.Add(this.cbR1B0);
      this.groupboxCharacterEdit.Controls.Add(this.cbR1B1);
      this.groupboxCharacterEdit.Controls.Add(this.cbR1B2);
      this.groupboxCharacterEdit.Controls.Add(this.cbR1B3);
      this.groupboxCharacterEdit.Controls.Add(this.cbR1B4);
      this.groupboxCharacterEdit.Controls.Add(this.cbR1B5);
      this.groupboxCharacterEdit.Controls.Add(this.cbR1B6);
      this.groupboxCharacterEdit.Controls.Add(this.cbR1B7);
      this.groupboxCharacterEdit.Controls.Add(this.cbR0B0);
      this.groupboxCharacterEdit.Controls.Add(this.cbR0B1);
      this.groupboxCharacterEdit.Controls.Add(this.cbR0B2);
      this.groupboxCharacterEdit.Controls.Add(this.cbR0B3);
      this.groupboxCharacterEdit.Controls.Add(this.cbR0B4);
      this.groupboxCharacterEdit.Controls.Add(this.cbR0B5);
      this.groupboxCharacterEdit.Controls.Add(this.cbR0B6);
      this.groupboxCharacterEdit.Controls.Add(this.cbR0B7);
      this.groupboxCharacterEdit.Location = new System.Drawing.Point(344, 46);
      this.groupboxCharacterEdit.Name = "groupboxCharacterEdit";
      this.groupboxCharacterEdit.Size = new System.Drawing.Size(239, 380);
      this.groupboxCharacterEdit.TabIndex = 32;
      this.groupboxCharacterEdit.TabStop = false;
      this.groupboxCharacterEdit.Text = " Custom Character Editor ";
      this.groupboxCharacterEdit.Visible = false;
      // 
      // cbR7B0
      // 
      this.cbR7B0.AutoSize = true;
      this.cbR7B0.Location = new System.Drawing.Point(142, 112);
      this.cbR7B0.Name = "cbR7B0";
      this.cbR7B0.Size = new System.Drawing.Size(15, 14);
      this.cbR7B0.TabIndex = 63;
      this.cbR7B0.UseVisualStyleBackColor = true;
      this.cbR7B0.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR7B1
      // 
      this.cbR7B1.AutoSize = true;
      this.cbR7B1.Location = new System.Drawing.Point(130, 112);
      this.cbR7B1.Name = "cbR7B1";
      this.cbR7B1.Size = new System.Drawing.Size(15, 14);
      this.cbR7B1.TabIndex = 62;
      this.cbR7B1.UseVisualStyleBackColor = true;
      this.cbR7B1.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR7B2
      // 
      this.cbR7B2.AutoSize = true;
      this.cbR7B2.Location = new System.Drawing.Point(118, 112);
      this.cbR7B2.Name = "cbR7B2";
      this.cbR7B2.Size = new System.Drawing.Size(15, 14);
      this.cbR7B2.TabIndex = 61;
      this.cbR7B2.UseVisualStyleBackColor = true;
      this.cbR7B2.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR7B3
      // 
      this.cbR7B3.AutoSize = true;
      this.cbR7B3.Location = new System.Drawing.Point(106, 112);
      this.cbR7B3.Name = "cbR7B3";
      this.cbR7B3.Size = new System.Drawing.Size(15, 14);
      this.cbR7B3.TabIndex = 60;
      this.cbR7B3.UseVisualStyleBackColor = true;
      this.cbR7B3.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR7B4
      // 
      this.cbR7B4.AutoSize = true;
      this.cbR7B4.Location = new System.Drawing.Point(94, 112);
      this.cbR7B4.Name = "cbR7B4";
      this.cbR7B4.Size = new System.Drawing.Size(15, 14);
      this.cbR7B4.TabIndex = 59;
      this.cbR7B4.UseVisualStyleBackColor = true;
      this.cbR7B4.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR7B5
      // 
      this.cbR7B5.AutoSize = true;
      this.cbR7B5.Location = new System.Drawing.Point(82, 112);
      this.cbR7B5.Name = "cbR7B5";
      this.cbR7B5.Size = new System.Drawing.Size(15, 14);
      this.cbR7B5.TabIndex = 58;
      this.cbR7B5.UseVisualStyleBackColor = true;
      this.cbR7B5.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR7B6
      // 
      this.cbR7B6.AutoSize = true;
      this.cbR7B6.Location = new System.Drawing.Point(70, 112);
      this.cbR7B6.Name = "cbR7B6";
      this.cbR7B6.Size = new System.Drawing.Size(15, 14);
      this.cbR7B6.TabIndex = 57;
      this.cbR7B6.UseVisualStyleBackColor = true;
      this.cbR7B6.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR7B7
      // 
      this.cbR7B7.AutoSize = true;
      this.cbR7B7.Location = new System.Drawing.Point(58, 112);
      this.cbR7B7.Name = "cbR7B7";
      this.cbR7B7.Size = new System.Drawing.Size(15, 14);
      this.cbR7B7.TabIndex = 56;
      this.cbR7B7.UseVisualStyleBackColor = true;
      this.cbR7B7.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR6B0
      // 
      this.cbR6B0.AutoSize = true;
      this.cbR6B0.Location = new System.Drawing.Point(142, 100);
      this.cbR6B0.Name = "cbR6B0";
      this.cbR6B0.Size = new System.Drawing.Size(15, 14);
      this.cbR6B0.TabIndex = 55;
      this.cbR6B0.UseVisualStyleBackColor = true;
      this.cbR6B0.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR6B1
      // 
      this.cbR6B1.AutoSize = true;
      this.cbR6B1.Location = new System.Drawing.Point(130, 100);
      this.cbR6B1.Name = "cbR6B1";
      this.cbR6B1.Size = new System.Drawing.Size(15, 14);
      this.cbR6B1.TabIndex = 54;
      this.cbR6B1.UseVisualStyleBackColor = true;
      this.cbR6B1.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR6B2
      // 
      this.cbR6B2.AutoSize = true;
      this.cbR6B2.Location = new System.Drawing.Point(118, 100);
      this.cbR6B2.Name = "cbR6B2";
      this.cbR6B2.Size = new System.Drawing.Size(15, 14);
      this.cbR6B2.TabIndex = 53;
      this.cbR6B2.UseVisualStyleBackColor = true;
      this.cbR6B2.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR6B3
      // 
      this.cbR6B3.AutoSize = true;
      this.cbR6B3.Location = new System.Drawing.Point(106, 100);
      this.cbR6B3.Name = "cbR6B3";
      this.cbR6B3.Size = new System.Drawing.Size(15, 14);
      this.cbR6B3.TabIndex = 52;
      this.cbR6B3.UseVisualStyleBackColor = true;
      this.cbR6B3.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR6B4
      // 
      this.cbR6B4.AutoSize = true;
      this.cbR6B4.Location = new System.Drawing.Point(94, 100);
      this.cbR6B4.Name = "cbR6B4";
      this.cbR6B4.Size = new System.Drawing.Size(15, 14);
      this.cbR6B4.TabIndex = 51;
      this.cbR6B4.UseVisualStyleBackColor = true;
      this.cbR6B4.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR6B5
      // 
      this.cbR6B5.AutoSize = true;
      this.cbR6B5.Location = new System.Drawing.Point(82, 100);
      this.cbR6B5.Name = "cbR6B5";
      this.cbR6B5.Size = new System.Drawing.Size(15, 14);
      this.cbR6B5.TabIndex = 50;
      this.cbR6B5.UseVisualStyleBackColor = true;
      this.cbR6B5.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR6B6
      // 
      this.cbR6B6.AutoSize = true;
      this.cbR6B6.Location = new System.Drawing.Point(70, 100);
      this.cbR6B6.Name = "cbR6B6";
      this.cbR6B6.Size = new System.Drawing.Size(15, 14);
      this.cbR6B6.TabIndex = 49;
      this.cbR6B6.UseVisualStyleBackColor = true;
      this.cbR6B6.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR6B7
      // 
      this.cbR6B7.AutoSize = true;
      this.cbR6B7.Location = new System.Drawing.Point(58, 100);
      this.cbR6B7.Name = "cbR6B7";
      this.cbR6B7.Size = new System.Drawing.Size(15, 14);
      this.cbR6B7.TabIndex = 48;
      this.cbR6B7.UseVisualStyleBackColor = true;
      this.cbR6B7.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR5B0
      // 
      this.cbR5B0.AutoSize = true;
      this.cbR5B0.Location = new System.Drawing.Point(142, 88);
      this.cbR5B0.Name = "cbR5B0";
      this.cbR5B0.Size = new System.Drawing.Size(15, 14);
      this.cbR5B0.TabIndex = 47;
      this.cbR5B0.UseVisualStyleBackColor = true;
      this.cbR5B0.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR5B1
      // 
      this.cbR5B1.AutoSize = true;
      this.cbR5B1.Location = new System.Drawing.Point(130, 88);
      this.cbR5B1.Name = "cbR5B1";
      this.cbR5B1.Size = new System.Drawing.Size(15, 14);
      this.cbR5B1.TabIndex = 46;
      this.cbR5B1.UseVisualStyleBackColor = true;
      this.cbR5B1.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR5B2
      // 
      this.cbR5B2.AutoSize = true;
      this.cbR5B2.Location = new System.Drawing.Point(118, 88);
      this.cbR5B2.Name = "cbR5B2";
      this.cbR5B2.Size = new System.Drawing.Size(15, 14);
      this.cbR5B2.TabIndex = 45;
      this.cbR5B2.UseVisualStyleBackColor = true;
      this.cbR5B2.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR5B3
      // 
      this.cbR5B3.AutoSize = true;
      this.cbR5B3.Location = new System.Drawing.Point(106, 88);
      this.cbR5B3.Name = "cbR5B3";
      this.cbR5B3.Size = new System.Drawing.Size(15, 14);
      this.cbR5B3.TabIndex = 44;
      this.cbR5B3.UseVisualStyleBackColor = true;
      this.cbR5B3.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR5B4
      // 
      this.cbR5B4.AutoSize = true;
      this.cbR5B4.Location = new System.Drawing.Point(94, 88);
      this.cbR5B4.Name = "cbR5B4";
      this.cbR5B4.Size = new System.Drawing.Size(15, 14);
      this.cbR5B4.TabIndex = 43;
      this.cbR5B4.UseVisualStyleBackColor = true;
      this.cbR5B4.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR5B5
      // 
      this.cbR5B5.AutoSize = true;
      this.cbR5B5.Location = new System.Drawing.Point(82, 88);
      this.cbR5B5.Name = "cbR5B5";
      this.cbR5B5.Size = new System.Drawing.Size(15, 14);
      this.cbR5B5.TabIndex = 42;
      this.cbR5B5.UseVisualStyleBackColor = true;
      this.cbR5B5.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR5B6
      // 
      this.cbR5B6.AutoSize = true;
      this.cbR5B6.Location = new System.Drawing.Point(70, 88);
      this.cbR5B6.Name = "cbR5B6";
      this.cbR5B6.Size = new System.Drawing.Size(15, 14);
      this.cbR5B6.TabIndex = 41;
      this.cbR5B6.UseVisualStyleBackColor = true;
      this.cbR5B6.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR5B7
      // 
      this.cbR5B7.AutoSize = true;
      this.cbR5B7.Location = new System.Drawing.Point(58, 88);
      this.cbR5B7.Name = "cbR5B7";
      this.cbR5B7.Size = new System.Drawing.Size(15, 14);
      this.cbR5B7.TabIndex = 40;
      this.cbR5B7.UseVisualStyleBackColor = true;
      this.cbR5B7.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR4B0
      // 
      this.cbR4B0.AutoSize = true;
      this.cbR4B0.Location = new System.Drawing.Point(142, 76);
      this.cbR4B0.Name = "cbR4B0";
      this.cbR4B0.Size = new System.Drawing.Size(15, 14);
      this.cbR4B0.TabIndex = 39;
      this.cbR4B0.UseVisualStyleBackColor = true;
      this.cbR4B0.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR4B1
      // 
      this.cbR4B1.AutoSize = true;
      this.cbR4B1.Location = new System.Drawing.Point(130, 76);
      this.cbR4B1.Name = "cbR4B1";
      this.cbR4B1.Size = new System.Drawing.Size(15, 14);
      this.cbR4B1.TabIndex = 38;
      this.cbR4B1.UseVisualStyleBackColor = true;
      this.cbR4B1.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR4B2
      // 
      this.cbR4B2.AutoSize = true;
      this.cbR4B2.Location = new System.Drawing.Point(118, 76);
      this.cbR4B2.Name = "cbR4B2";
      this.cbR4B2.Size = new System.Drawing.Size(15, 14);
      this.cbR4B2.TabIndex = 37;
      this.cbR4B2.UseVisualStyleBackColor = true;
      this.cbR4B2.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR4B3
      // 
      this.cbR4B3.AutoSize = true;
      this.cbR4B3.Location = new System.Drawing.Point(106, 76);
      this.cbR4B3.Name = "cbR4B3";
      this.cbR4B3.Size = new System.Drawing.Size(15, 14);
      this.cbR4B3.TabIndex = 36;
      this.cbR4B3.UseVisualStyleBackColor = true;
      this.cbR4B3.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR4B4
      // 
      this.cbR4B4.AutoSize = true;
      this.cbR4B4.Location = new System.Drawing.Point(94, 76);
      this.cbR4B4.Name = "cbR4B4";
      this.cbR4B4.Size = new System.Drawing.Size(15, 14);
      this.cbR4B4.TabIndex = 35;
      this.cbR4B4.UseVisualStyleBackColor = true;
      this.cbR4B4.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR4B5
      // 
      this.cbR4B5.AutoSize = true;
      this.cbR4B5.Location = new System.Drawing.Point(82, 76);
      this.cbR4B5.Name = "cbR4B5";
      this.cbR4B5.Size = new System.Drawing.Size(15, 14);
      this.cbR4B5.TabIndex = 34;
      this.cbR4B5.UseVisualStyleBackColor = true;
      this.cbR4B5.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR4B6
      // 
      this.cbR4B6.AutoSize = true;
      this.cbR4B6.Location = new System.Drawing.Point(70, 76);
      this.cbR4B6.Name = "cbR4B6";
      this.cbR4B6.Size = new System.Drawing.Size(15, 14);
      this.cbR4B6.TabIndex = 33;
      this.cbR4B6.UseVisualStyleBackColor = true;
      this.cbR4B6.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR4B7
      // 
      this.cbR4B7.AutoSize = true;
      this.cbR4B7.Location = new System.Drawing.Point(58, 76);
      this.cbR4B7.Name = "cbR4B7";
      this.cbR4B7.Size = new System.Drawing.Size(15, 14);
      this.cbR4B7.TabIndex = 32;
      this.cbR4B7.UseVisualStyleBackColor = true;
      this.cbR4B7.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR3B0
      // 
      this.cbR3B0.AutoSize = true;
      this.cbR3B0.Location = new System.Drawing.Point(142, 64);
      this.cbR3B0.Name = "cbR3B0";
      this.cbR3B0.Size = new System.Drawing.Size(15, 14);
      this.cbR3B0.TabIndex = 31;
      this.cbR3B0.UseVisualStyleBackColor = true;
      this.cbR3B0.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR3B1
      // 
      this.cbR3B1.AutoSize = true;
      this.cbR3B1.Location = new System.Drawing.Point(130, 64);
      this.cbR3B1.Name = "cbR3B1";
      this.cbR3B1.Size = new System.Drawing.Size(15, 14);
      this.cbR3B1.TabIndex = 30;
      this.cbR3B1.UseVisualStyleBackColor = true;
      this.cbR3B1.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR3B2
      // 
      this.cbR3B2.AutoSize = true;
      this.cbR3B2.Location = new System.Drawing.Point(118, 64);
      this.cbR3B2.Name = "cbR3B2";
      this.cbR3B2.Size = new System.Drawing.Size(15, 14);
      this.cbR3B2.TabIndex = 29;
      this.cbR3B2.UseVisualStyleBackColor = true;
      this.cbR3B2.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR3B3
      // 
      this.cbR3B3.AutoSize = true;
      this.cbR3B3.Location = new System.Drawing.Point(106, 64);
      this.cbR3B3.Name = "cbR3B3";
      this.cbR3B3.Size = new System.Drawing.Size(15, 14);
      this.cbR3B3.TabIndex = 28;
      this.cbR3B3.UseVisualStyleBackColor = true;
      this.cbR3B3.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR3B4
      // 
      this.cbR3B4.AutoSize = true;
      this.cbR3B4.Location = new System.Drawing.Point(94, 64);
      this.cbR3B4.Name = "cbR3B4";
      this.cbR3B4.Size = new System.Drawing.Size(15, 14);
      this.cbR3B4.TabIndex = 27;
      this.cbR3B4.UseVisualStyleBackColor = true;
      this.cbR3B4.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR3B5
      // 
      this.cbR3B5.AutoSize = true;
      this.cbR3B5.Location = new System.Drawing.Point(82, 64);
      this.cbR3B5.Name = "cbR3B5";
      this.cbR3B5.Size = new System.Drawing.Size(15, 14);
      this.cbR3B5.TabIndex = 26;
      this.cbR3B5.UseVisualStyleBackColor = true;
      this.cbR3B5.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR3B6
      // 
      this.cbR3B6.AutoSize = true;
      this.cbR3B6.Location = new System.Drawing.Point(70, 64);
      this.cbR3B6.Name = "cbR3B6";
      this.cbR3B6.Size = new System.Drawing.Size(15, 14);
      this.cbR3B6.TabIndex = 25;
      this.cbR3B6.UseVisualStyleBackColor = true;
      this.cbR3B6.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR3B7
      // 
      this.cbR3B7.AutoSize = true;
      this.cbR3B7.Location = new System.Drawing.Point(58, 64);
      this.cbR3B7.Name = "cbR3B7";
      this.cbR3B7.Size = new System.Drawing.Size(15, 14);
      this.cbR3B7.TabIndex = 24;
      this.cbR3B7.UseVisualStyleBackColor = true;
      this.cbR3B7.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR2B0
      // 
      this.cbR2B0.AutoSize = true;
      this.cbR2B0.Location = new System.Drawing.Point(142, 52);
      this.cbR2B0.Name = "cbR2B0";
      this.cbR2B0.Size = new System.Drawing.Size(15, 14);
      this.cbR2B0.TabIndex = 23;
      this.cbR2B0.UseVisualStyleBackColor = true;
      this.cbR2B0.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR2B1
      // 
      this.cbR2B1.AutoSize = true;
      this.cbR2B1.Location = new System.Drawing.Point(130, 52);
      this.cbR2B1.Name = "cbR2B1";
      this.cbR2B1.Size = new System.Drawing.Size(15, 14);
      this.cbR2B1.TabIndex = 22;
      this.cbR2B1.UseVisualStyleBackColor = true;
      this.cbR2B1.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR2B2
      // 
      this.cbR2B2.AutoSize = true;
      this.cbR2B2.Location = new System.Drawing.Point(118, 52);
      this.cbR2B2.Name = "cbR2B2";
      this.cbR2B2.Size = new System.Drawing.Size(15, 14);
      this.cbR2B2.TabIndex = 21;
      this.cbR2B2.UseVisualStyleBackColor = true;
      this.cbR2B2.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR2B3
      // 
      this.cbR2B3.AutoSize = true;
      this.cbR2B3.Location = new System.Drawing.Point(106, 52);
      this.cbR2B3.Name = "cbR2B3";
      this.cbR2B3.Size = new System.Drawing.Size(15, 14);
      this.cbR2B3.TabIndex = 20;
      this.cbR2B3.UseVisualStyleBackColor = true;
      this.cbR2B3.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR2B4
      // 
      this.cbR2B4.AutoSize = true;
      this.cbR2B4.Location = new System.Drawing.Point(94, 52);
      this.cbR2B4.Name = "cbR2B4";
      this.cbR2B4.Size = new System.Drawing.Size(15, 14);
      this.cbR2B4.TabIndex = 19;
      this.cbR2B4.UseVisualStyleBackColor = true;
      this.cbR2B4.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR2B5
      // 
      this.cbR2B5.AutoSize = true;
      this.cbR2B5.Location = new System.Drawing.Point(82, 52);
      this.cbR2B5.Name = "cbR2B5";
      this.cbR2B5.Size = new System.Drawing.Size(15, 14);
      this.cbR2B5.TabIndex = 18;
      this.cbR2B5.UseVisualStyleBackColor = true;
      this.cbR2B5.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR2B6
      // 
      this.cbR2B6.AutoSize = true;
      this.cbR2B6.Location = new System.Drawing.Point(70, 52);
      this.cbR2B6.Name = "cbR2B6";
      this.cbR2B6.Size = new System.Drawing.Size(15, 14);
      this.cbR2B6.TabIndex = 17;
      this.cbR2B6.UseVisualStyleBackColor = true;
      this.cbR2B6.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR2B7
      // 
      this.cbR2B7.AutoSize = true;
      this.cbR2B7.Location = new System.Drawing.Point(58, 52);
      this.cbR2B7.Name = "cbR2B7";
      this.cbR2B7.Size = new System.Drawing.Size(15, 14);
      this.cbR2B7.TabIndex = 16;
      this.cbR2B7.UseVisualStyleBackColor = true;
      this.cbR2B7.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR1B0
      // 
      this.cbR1B0.AutoSize = true;
      this.cbR1B0.Location = new System.Drawing.Point(142, 40);
      this.cbR1B0.Name = "cbR1B0";
      this.cbR1B0.Size = new System.Drawing.Size(15, 14);
      this.cbR1B0.TabIndex = 15;
      this.cbR1B0.UseVisualStyleBackColor = true;
      this.cbR1B0.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR1B1
      // 
      this.cbR1B1.AutoSize = true;
      this.cbR1B1.Location = new System.Drawing.Point(130, 40);
      this.cbR1B1.Name = "cbR1B1";
      this.cbR1B1.Size = new System.Drawing.Size(15, 14);
      this.cbR1B1.TabIndex = 14;
      this.cbR1B1.UseVisualStyleBackColor = true;
      this.cbR1B1.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR1B2
      // 
      this.cbR1B2.AutoSize = true;
      this.cbR1B2.Location = new System.Drawing.Point(118, 40);
      this.cbR1B2.Name = "cbR1B2";
      this.cbR1B2.Size = new System.Drawing.Size(15, 14);
      this.cbR1B2.TabIndex = 13;
      this.cbR1B2.UseVisualStyleBackColor = true;
      this.cbR1B2.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR1B3
      // 
      this.cbR1B3.AutoSize = true;
      this.cbR1B3.Location = new System.Drawing.Point(106, 40);
      this.cbR1B3.Name = "cbR1B3";
      this.cbR1B3.Size = new System.Drawing.Size(15, 14);
      this.cbR1B3.TabIndex = 12;
      this.cbR1B3.UseVisualStyleBackColor = true;
      this.cbR1B3.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR1B4
      // 
      this.cbR1B4.AutoSize = true;
      this.cbR1B4.Location = new System.Drawing.Point(94, 40);
      this.cbR1B4.Name = "cbR1B4";
      this.cbR1B4.Size = new System.Drawing.Size(15, 14);
      this.cbR1B4.TabIndex = 11;
      this.cbR1B4.UseVisualStyleBackColor = true;
      this.cbR1B4.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR1B5
      // 
      this.cbR1B5.AutoSize = true;
      this.cbR1B5.Location = new System.Drawing.Point(82, 40);
      this.cbR1B5.Name = "cbR1B5";
      this.cbR1B5.Size = new System.Drawing.Size(15, 14);
      this.cbR1B5.TabIndex = 10;
      this.cbR1B5.UseVisualStyleBackColor = true;
      this.cbR1B5.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR1B6
      // 
      this.cbR1B6.AutoSize = true;
      this.cbR1B6.Location = new System.Drawing.Point(70, 40);
      this.cbR1B6.Name = "cbR1B6";
      this.cbR1B6.Size = new System.Drawing.Size(15, 14);
      this.cbR1B6.TabIndex = 9;
      this.cbR1B6.UseVisualStyleBackColor = true;
      this.cbR1B6.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR1B7
      // 
      this.cbR1B7.AutoSize = true;
      this.cbR1B7.Location = new System.Drawing.Point(58, 40);
      this.cbR1B7.Name = "cbR1B7";
      this.cbR1B7.Size = new System.Drawing.Size(15, 14);
      this.cbR1B7.TabIndex = 8;
      this.cbR1B7.UseVisualStyleBackColor = true;
      this.cbR1B7.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR0B0
      // 
      this.cbR0B0.AutoSize = true;
      this.cbR0B0.Location = new System.Drawing.Point(142, 28);
      this.cbR0B0.Name = "cbR0B0";
      this.cbR0B0.Size = new System.Drawing.Size(15, 14);
      this.cbR0B0.TabIndex = 7;
      this.cbR0B0.UseVisualStyleBackColor = true;
      this.cbR0B0.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR0B1
      // 
      this.cbR0B1.AutoSize = true;
      this.cbR0B1.Location = new System.Drawing.Point(130, 28);
      this.cbR0B1.Name = "cbR0B1";
      this.cbR0B1.Size = new System.Drawing.Size(15, 14);
      this.cbR0B1.TabIndex = 6;
      this.cbR0B1.UseVisualStyleBackColor = true;
      this.cbR0B1.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR0B2
      // 
      this.cbR0B2.AutoSize = true;
      this.cbR0B2.Location = new System.Drawing.Point(118, 28);
      this.cbR0B2.Name = "cbR0B2";
      this.cbR0B2.Size = new System.Drawing.Size(15, 14);
      this.cbR0B2.TabIndex = 5;
      this.cbR0B2.UseVisualStyleBackColor = true;
      this.cbR0B2.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR0B3
      // 
      this.cbR0B3.AutoSize = true;
      this.cbR0B3.Location = new System.Drawing.Point(106, 28);
      this.cbR0B3.Name = "cbR0B3";
      this.cbR0B3.Size = new System.Drawing.Size(15, 14);
      this.cbR0B3.TabIndex = 4;
      this.cbR0B3.UseVisualStyleBackColor = true;
      this.cbR0B3.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR0B4
      // 
      this.cbR0B4.AutoSize = true;
      this.cbR0B4.Location = new System.Drawing.Point(94, 28);
      this.cbR0B4.Name = "cbR0B4";
      this.cbR0B4.Size = new System.Drawing.Size(15, 14);
      this.cbR0B4.TabIndex = 3;
      this.cbR0B4.UseVisualStyleBackColor = true;
      this.cbR0B4.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR0B5
      // 
      this.cbR0B5.AutoSize = true;
      this.cbR0B5.Location = new System.Drawing.Point(82, 28);
      this.cbR0B5.Name = "cbR0B5";
      this.cbR0B5.Size = new System.Drawing.Size(15, 14);
      this.cbR0B5.TabIndex = 2;
      this.cbR0B5.UseVisualStyleBackColor = true;
      this.cbR0B5.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR0B6
      // 
      this.cbR0B6.AutoSize = true;
      this.cbR0B6.Location = new System.Drawing.Point(70, 28);
      this.cbR0B6.Name = "cbR0B6";
      this.cbR0B6.Size = new System.Drawing.Size(15, 14);
      this.cbR0B6.TabIndex = 1;
      this.cbR0B6.UseVisualStyleBackColor = true;
      this.cbR0B6.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // cbR0B7
      // 
      this.cbR0B7.AutoSize = true;
      this.cbR0B7.Location = new System.Drawing.Point(58, 28);
      this.cbR0B7.Name = "cbR0B7";
      this.cbR0B7.Size = new System.Drawing.Size(15, 14);
      this.cbR0B7.TabIndex = 0;
      this.cbR0B7.UseVisualStyleBackColor = true;
      this.cbR0B7.CheckedChanged += new System.EventHandler(this.CharBitmap_CheckedChanged);
      // 
      // groupboxTranslationEdit
      // 
      this.groupboxTranslationEdit.Controls.Add(this.mpLabel5);
      this.groupboxTranslationEdit.Controls.Add(this.TextBoxTranslateTo);
      this.groupboxTranslationEdit.Controls.Add(this.mpLabel4);
      this.groupboxTranslationEdit.Controls.Add(this.TextBoxTranslateFrom);
      this.groupboxTranslationEdit.Location = new System.Drawing.Point(344, 46);
      this.groupboxTranslationEdit.Name = "groupboxTranslationEdit";
      this.groupboxTranslationEdit.Size = new System.Drawing.Size(239, 380);
      this.groupboxTranslationEdit.TabIndex = 33;
      this.groupboxTranslationEdit.TabStop = false;
      this.groupboxTranslationEdit.Text = " Translation Editor ";
      this.groupboxTranslationEdit.Visible = false;
      // 
      // mpLabel5
      // 
      this.mpLabel5.AutoSize = true;
      this.mpLabel5.Location = new System.Drawing.Point(15, 56);
      this.mpLabel5.Name = "mpLabel5";
      this.mpLabel5.Size = new System.Drawing.Size(23, 13);
      this.mpLabel5.TabIndex = 27;
      this.mpLabel5.Text = "To:";
      // 
      // TextBoxTranslateTo
      // 
      this.TextBoxTranslateTo.BorderColor = System.Drawing.Color.Empty;
      this.TextBoxTranslateTo.Location = new System.Drawing.Point(58, 52);
      this.TextBoxTranslateTo.MaxLength = 100;
      this.TextBoxTranslateTo.Name = "TextBoxTranslateTo";
      this.TextBoxTranslateTo.Size = new System.Drawing.Size(133, 20);
      this.TextBoxTranslateTo.TabIndex = 26;
      this.TextBoxTranslateTo.LostFocus += new System.EventHandler(this.TextBoxTranslateTo_LostFocus);
      // 
      // mpLabel4
      // 
      this.mpLabel4.AutoSize = true;
      this.mpLabel4.Location = new System.Drawing.Point(15, 28);
      this.mpLabel4.Name = "mpLabel4";
      this.mpLabel4.Size = new System.Drawing.Size(33, 13);
      this.mpLabel4.TabIndex = 25;
      this.mpLabel4.Text = "From:";
      // 
      // TextBoxTranslateFrom
      // 
      this.TextBoxTranslateFrom.BorderColor = System.Drawing.Color.Empty;
      this.TextBoxTranslateFrom.Location = new System.Drawing.Point(58, 24);
      this.TextBoxTranslateFrom.MaxLength = 1;
      this.TextBoxTranslateFrom.Name = "TextBoxTranslateFrom";
      this.TextBoxTranslateFrom.Size = new System.Drawing.Size(133, 20);
      this.TextBoxTranslateFrom.TabIndex = 24;
      this.TextBoxTranslateFrom.LostFocus += new System.EventHandler(this.TextBoxTranslateFrom_LostFocus);
      // 
      // MessageEditForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScroll = true;
      this.ClientSize = new System.Drawing.Size(590, 475);
      this.Controls.Add(this.labelExpand);
      this.Controls.Add(this.treeMapping);
      this.Controls.Add(this.buttonDefault);
      this.Controls.Add(this.buttonRemove);
      this.Controls.Add(this.buttonNew);
      this.Controls.Add(this.buttonDown);
      this.Controls.Add(this.buttonUp);
      this.Controls.Add(this.beveledLine1);
      this.Controls.Add(this.buttonOk);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.headerLabel);
      this.Controls.Add(this.groupBoxMessageEdit);
      this.Controls.Add(this.groupboxTranslationEdit);
      this.Controls.Add(this.groupboxCharacterEdit);
      this.Name = "MessageEditForm";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "MiniDisplay - Setup - Configuration Editor";
      this.groupBoxMessageEdit.ResumeLayout(false);
      this.groupBoxProcess.ResumeLayout(false);
      this.groupBoxProcess.PerformLayout();
      this.groupBoxTextProgressBar.ResumeLayout(false);
      this.groupBoxTextProgressBar.PerformLayout();
      this.GroupBoxCondition.ResumeLayout(false);
      this.GroupBoxCondition.PerformLayout();
      this.groupBoxMessageType.ResumeLayout(false);
      this.groupBoxMessageType.PerformLayout();
      this.groupBoxStatus.ResumeLayout(false);
      this.groupBoxWindow.ResumeLayout(false);
      this.groupBoxLine.ResumeLayout(false);
      this.groupBoxLine.PerformLayout();
      this.groupboxCharacterEdit.ResumeLayout(false);
      this.groupboxCharacterEdit.PerformLayout();
      this.groupboxTranslationEdit.ResumeLayout(false);
      this.groupboxTranslationEdit.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    private void labelExpand_Click(object sender, EventArgs e)
    {
      if (this.treeMapping.SelectedNode == null)
      {
        this.treeMapping.Select();
      }
      this.treeMapping.SelectedNode.ExpandAll();
    }

    private void LoadMessages(bool defaults)
    {
      try
      {        
        string fileName = "MiniDisplay.xml";
        this.groupBoxStatus.Enabled = false;
        this.groupBoxLine.Enabled = false;
        this.groupBoxWindow.Enabled = false;
        this.groupBoxProcess.Enabled = false;
        this.GroupBoxCondition.Enabled = false;
        this.treeMapping.Nodes.Clear();
        XmlDocument document = new XmlDocument();
        string file = Config.GetFile(Config.Dir.Config, fileName);
        if (!File.Exists(file))
        {
          MessageBox.Show("Can't locate configuration file " + fileName + "\n\nMake sure it exists",
                          "Mapping file missing", MessageBoxButtons.OK, MessageBoxIcon.Hand);
          this.buttonUp.Enabled =
            this.buttonDown.Enabled =
            this.buttonNew.Enabled =
            this.buttonRemove.Enabled = this.buttonDefault.Enabled = false;
          base.ShowInTaskbar = true;
          base.WindowState = FormWindowState.Minimized;
          new Thread(new ThreadStart(this.CloseThread)).Start();
        }
        else
        {
          Log.Info("LOADING {0}", new object[] {fileName});
          document.Load(file);
          Log.Info("Extracting Settings");
          XmlNode node = document.DocumentElement.SelectSingleNode("//Settings");
          Log.Info("Extracting Messages");
          XmlNodeList list = node.SelectNodes("Message");
          Log.Info("Extracting TranslateFrom");
          XmlNodeList list2 = node.SelectNodes("TranslateFrom");
          Log.Info("Extracting TranslateTo");
          XmlNodeList list3 = node.SelectNodes("TranslateTo");
          Log.Info("Extracting CustomCharacters");
          XmlNodeList list4 = node.SelectNodes("CustomCharacters");
          this.SettingsNode = new TreeNode("Settings");
          this.SettingsNode.Tag = new Data("SECTION", "SETTINGS", "");
          TreeNode node2 = new TreeNode("Status Messages");
          node2.Tag = new Data("SECTION", "STATUSMESSAGES", "");
          TreeNode node3 = new TreeNode("Character Translations");
          node3.Tag = new Data("SECTION", "CHARACTERTRANSLATIONS", "");
          TreeNode node4 = new TreeNode("Custom Characters");
          node4.Tag = new Data("SECTION", "CUSTOMCHARACTERS", "");
          Log.Info("Enumerating Settings");
          for (int i = 0; i < node.Attributes.Count; i++)
          {
            TreeNode node5 = new TreeNode("Setting: " + node.Attributes[i].Name + " = " + node.Attributes[i].Value);
            node5.Tag = new Data("SETTING", node.Attributes[i].Name, node.Attributes[i].Value);
            this.SettingsNode.Nodes.Add(node5);
          }
          Log.Info("Enumerating Custom Characters");
          if (list4.Count > 0)
          {
            XmlNodeList list5 = list4[0].SelectNodes("CustomCharacter");
            int newValue = 0;
            foreach (XmlNode node6 in list5)
            {
              TreeNode node7 = new TreeNode("Character #" + newValue.ToString());
              node7.Tag = new Data("CHARACTER", "", newValue);
              node4.Nodes.Add(node7);
              XmlNodeList list6 = node6.SelectNodes("int");
              if (list6.Count == 8)
              {
                for (int j = 0; j < 8; j++)
                {
                  string str4;
                  byte num4 = byte.Parse(list6[j].InnerText);
                  if (num4 < 0x10)
                  {
                    str4 = "Byte " + j.ToString() + ": 0x0" + num4.ToString("x00");
                  }
                  else
                  {
                    str4 = "Byte " + j.ToString() + ": 0x" + num4.ToString("x00");
                  }
                  TreeNode node8 = new TreeNode(str4);
                  node8.Tag = new Data("BYTE", j, num4);
                  node7.Nodes.Add(node8);
                }
              }
              else
              {
                Log.Info("Ignoring invalid custom character data");
              }
              newValue++;
            }
          }
          else
          {
            Log.Info("No character translations");
          }
          Log.Info("Enumerating Character Translations");
          if ((list2.Count == 0) && (list3.Count == 0))
          {
            Log.Info("No character translations");
          }
          else
          {
            XmlNodeList list7 = list2[0].SelectNodes("string");
            XmlNodeList list8 = list3[0].SelectNodes("string");
            for (int k = 0; k < list7.Count; k++)
            {
              if (k < list8.Count)
              {
                XmlNode node9 = list7[k];
                XmlNode node10 = list8[k];
                string text = "Translation: \"" + node9.InnerText + "\" = \"" + node10.InnerText + "\"";
                Log.Info("  Adding : {0}", new object[] {text});
                TreeNode node11 = new TreeNode(text);
                node11.Tag = new Data("TRANSLATION", node9.InnerText, node10.InnerText);
                node3.Nodes.Add(node11);
              }
              else
              {
                Log.Info("Ignoring corrupt character translation - index = {0}", new object[] {k});
              }
            }
          }
          Log.Info("Enumerating Messages");
          foreach (XmlNode node12 in list)
          {
            string str6;
            if (node12.Attributes.Count > 0)
            {
              str6 = node12.Attributes["Status"].Value;
            }
            else
            {
              str6 = "ALL STATES";
            }
            Log.Info("Adding Message: state = \"{0}\"", new object[] {str6});
            TreeNode node13 = new TreeNode(str6);
            node13.Tag = new Data("MESSAGE", "STATUS", str6);
            node2.Nodes.Add(node13);
            TreeNode node14 = new TreeNode("WINDOWS");
            node14.Tag = new Data("WINDOWLIST", "", "");
            node13.Nodes.Add(node14);
            Log.Info("Enuerating Message windows");
            XmlNodeList list9 = node12.SelectNodes("Window");
            if (list9.Count > 0)
            {
              foreach (XmlNode node15 in list9)
              {
                int num6 = int.Parse(node15.InnerText);
                string innerText = node15.InnerText;
                if (Enum.IsDefined(typeof (GUIWindow.Window), num6))
                {
                  innerText = innerText + " (" + this.GetFriendlyName(Enum.GetName(typeof (GUIWindow.Window), num6)) +
                              ")";
                }
                else
                {
                  innerText = innerText + "(UNKNOWN!)";
                }
                Log.Info("  Adding Window: {0}", new object[] {innerText});
                TreeNode node16 = new TreeNode(innerText);
                node16.Tag = new Data("WINDOW", "ID", node15.InnerText);
                node14.Nodes.Add(node16);
              }
            }
            else
            {
              Log.Info("  Adding Window: {0}", new object[] {"All Windows"});
              TreeNode node17 = new TreeNode("All Windows");
              node17.Tag = new Data("WINDOW", "ALL", "");
              node14.Nodes.Add(node17);
            }
            Log.Info("  Enuerating Lines");
            XmlNodeList list10 = node12.SelectNodes("Line");
            foreach (XmlNode node18 in node12.ChildNodes)
            {
              string name = node18.Name;
              if (name == null)
              {
                goto Label_1497;
              }
              if (!(name == "Line"))
              {
                if (name == "Image")
                {
                  goto Label_0FD9;
                }
                goto Label_1497;
              }
              Log.Info("    Adding Line");
              TreeNode node19 = new TreeNode("LINE Alignment = " + node18.Attributes["Alignment"].Value);
              node19.Tag = new Data("LINE", "ALIGNMENT", node18.Attributes["Alignment"].Value);
              node13.Nodes.Add(node19);
              Log.Info("    Added Line");
              Log.Info("    Enumerating Line properties - child nodes = {0}({1})",
                       new object[] {node18.HasChildNodes, node18.ChildNodes.Count});
              if (node18.HasChildNodes)
              {
                foreach (XmlNode node20 in node18.ChildNodes)
                {
                  if (node20 == null)
                  {
                    continue;
                  }
                  Log.Info("      Adding property: {0}", new object[] {node20.LocalName});
                  TreeNode node21 =
                    new TreeNode(node20.LocalName +
                                 ((node20.Attributes.GetNamedItem("Value") == null)
                                    ? string.Empty
                                    : (": " + node20.Attributes["Value"].Value)));
                  if (!node20.LocalName.Equals("TextProgressBar"))
                  {
                    node21.Tag = new Data("PROCESS", node20.LocalName, node20.Attributes["Value"].Value);
                  }
                  else
                  {
                    string str8 =
                      string.Concat(new object[]
                                      {
                                        node20.Attributes["StartChar"].Value[0], "|",
                                        node20.Attributes["EndChar"].Value[0], "|",
                                        node20.Attributes["ValueChar"].Value[0], "|", node20.Attributes["FillChar"].Value
                                        , "|", node20.Attributes["Length"].Value, "|"
                                      });
                    if (node20.ChildNodes[0].LocalName.Equals("ValueProperty"))
                    {
                      str8 = str8 + node20.ChildNodes[0].Attributes["Value"].Value + "|" +
                             node20.ChildNodes[1].Attributes["Value"].Value;
                    }
                    else
                    {
                      str8 = str8 + node20.ChildNodes[1].Attributes["Value"].Value + "|" +
                             node20.ChildNodes[0].Attributes["Value"].Value;
                    }
                    node21.Tag = new Data("PROCESS", node20.LocalName, str8);
                  }
                  node19.Nodes.Add(node21);
                  Log.Info("      Added property: {0}", new object[] {node20.LocalName});
                  if (!node20.LocalName.Equals("TextProgressBar") && node20.HasChildNodes)
                  {
                    Log.Info("      Enumerating Line conditions");
                    foreach (XmlNode node22 in node20.ChildNodes)
                    {
                      TreeNode node23;
                      Log.Info("        Adding Condition: {0}", new object[] {node22.LocalName});
                      if (((name = node22.LocalName.ToLower()) != null) && ((name == "or") || (name == "and")))
                      {
                        Log.Info("          Adding and/or Condition: {0}", new object[] {node22.LocalName});
                        node23 = new TreeNode(node22.LocalName);
                        node23.Tag = new Data("CONDITION", node22.LocalName, "");
                        node21.Nodes.Add(node23);
                        Log.Info("          Added and/or Condition: {0}", new object[] {node22.LocalName});
                        if (node22.HasChildNodes)
                        {
                          foreach (XmlNode node24 in node22.ChildNodes)
                          {
                            Log.Info("          Adding SubCondition: {0}", new object[] {node24.LocalName});
                            TreeNode node25 = new TreeNode(node24.LocalName + ": " + node24.Attributes["Value"].Value);
                            node25.Tag = new Data("SUBCONDITION", node24.LocalName, node24.Attributes["Value"].Value);
                            node23.Nodes.Add(node25);
                            Log.Info("          Added SubCondition: {0}", new object[] {node24.LocalName});
                          }
                        }
                      }
                      else
                      {
                        Log.Info("          Adding other Condition: {0}", new object[] {node22.LocalName});
                        node23 = new TreeNode(node22.LocalName + ": " + node22.Attributes["Value"].Value);
                        node23.Tag = new Data("CONDITION", node22.LocalName, node22.Attributes["Value"].Value);
                        node21.Nodes.Add(node23);
                        Log.Info("          Added other Condition: {0}", new object[] {node22.LocalName});
                      }
                      Log.Info("        Added Condition: {0}", new object[] {node22.LocalName});
                    }
                    Log.Info("      Enumerated Line conditions");
                  }
                }
              }
              Log.Info("    Enumerated Line properties");
              continue;
              Label_0FD9:
              Log.Info("    Adding Image");
              TreeNode node26 = new TreeNode("IMAGE");
              node26.Tag = new Data("IMAGE", "", "");
              node13.Nodes.Add(node26);
              Log.Info("    Added Image");
              Log.Info("    Enumerating Image properties - child nodes = {0}({1})",
                       new object[] {node18.HasChildNodes, node18.ChildNodes.Count});
              if (node18.HasChildNodes)
              {
                foreach (XmlNode node27 in node18.ChildNodes)
                {
                  if (node27 != null)
                  {
                    Log.Info("      Adding Image property: {0}", new object[] {node27.LocalName});
                    TreeNode node28 =
                      new TreeNode(node27.LocalName +
                                   ((node27.Attributes.GetNamedItem("Value") == null)
                                      ? string.Empty
                                      : (": " + node27.Attributes["Value"].Value)));
                    node28.Tag = new Data("PROCESS", node27.LocalName, node27.Attributes["Value"].Value);
                    node26.Nodes.Add(node28);
                    Log.Info("      Added Image property: {0}", new object[] {node27.LocalName});
                    if (node27.HasChildNodes)
                    {
                      Log.Info("      Enumerating Image conditions");
                      foreach (XmlNode node29 in node27.ChildNodes)
                      {
                        TreeNode node30;
                        Log.Info("        Adding Image Condition: {0}", new object[] {node29.LocalName});
                        if (((name = node29.LocalName.ToLower()) != null) && ((name == "or") || (name == "and")))
                        {
                          Log.Info("          Adding and/or Image Condition: {0}", new object[] {node29.LocalName});
                          node30 = new TreeNode(node29.LocalName);
                          node30.Tag = new Data("CONDITION", node29.LocalName, "");
                          node28.Nodes.Add(node30);
                          Log.Info("          Added and/or Image Condition: {0}", new object[] {node29.LocalName});
                          if (node29.HasChildNodes)
                          {
                            foreach (XmlNode node31 in node29.ChildNodes)
                            {
                              Log.Info("          Adding Image SubCondition: {0}", new object[] {node31.LocalName});
                              TreeNode node32 = new TreeNode(node31.LocalName + ": " + node31.Attributes["Value"].Value);
                              node32.Tag = new Data("SUBCONDITION", node31.LocalName, node31.Attributes["Value"].Value);
                              node30.Nodes.Add(node32);
                              Log.Info("          Added Image SubCondition: {0}", new object[] {node31.LocalName});
                            }
                          }
                        }
                        else
                        {
                          Log.Info("          Adding other Image Condition: {0}", new object[] {node29.LocalName});
                          node30 = new TreeNode(node29.LocalName + ": " + node29.Attributes["Value"].Value);
                          node30.Tag = new Data("CONDITION", node29.LocalName, node29.Attributes["Value"].Value);
                          node28.Nodes.Add(node30);
                          Log.Info("          Added other Image Condition: {0}", new object[] {node29.LocalName});
                        }
                        Log.Info("        Added Image Condition: {0}", new object[] {node29.LocalName});
                      }
                      Log.Info("      Enumerated Image conditions");
                    }
                  }
                }
              }
              Log.Info("    Enumerated Image properties");
              continue;
              Label_1497:
              ;
              Log.Info("  Enumerating unknown tag \"{0}\"", new object[] {node18.Name});
            }
            Log.Info("  Enuerated Lines");
            Log.Info("    MESSAGE ADDED   ");
            Log.Info("");
          }
          Log.Info("    MESSAGE PROCESSING COMPLETE   ");
          Log.Info("");
          this.treeMapping.Nodes.Add(node2);
          this.treeMapping.Nodes.Add(node3);
          this.treeMapping.Nodes.Add(node4);
          this.changedSettings = false;
        }
      }
      catch (Exception exception)
      {
        Log.Error(exception);
      }
    }

    private bool SaveMapping_OLD(string xmlFile)
    {
      try
      {
        Directory.CreateDirectory(Config.GetFolder(Config.Dir.CustomInputDevice));
      }
      catch
      {
        Log.Info("MAP: Error accessing directory \"InputDeviceMappings\\custom\"");
      }
      XmlTextWriter writer = new XmlTextWriter(Config.GetFile(Config.Dir.CustomInputDevice, xmlFile), Encoding.UTF8);
      writer.Formatting = Formatting.Indented;
      writer.Indentation = 1;
      writer.IndentChar = '\t';
      writer.WriteStartDocument(true);
      writer.WriteStartElement("mappings");
      writer.WriteAttributeString("version", "3");
      if (this.treeMapping.Nodes.Count > 0)
      {
        foreach (TreeNode node in this.treeMapping.Nodes)
        {
          writer.WriteStartElement("remote");
          writer.WriteAttributeString("family", (string) ((Data) node.Tag).Value);
          if (node.Nodes.Count > 0)
          {
            foreach (TreeNode node2 in node.Nodes)
            {
              writer.WriteStartElement("button");
              writer.WriteAttributeString("name", (string) ((Data) node2.Tag).Parameter);
              writer.WriteAttributeString("code", (string) ((Data) node2.Tag).Value);
              if (node2.Nodes.Count > 0)
              {
                foreach (TreeNode node3 in node2.Nodes)
                {
                  foreach (TreeNode node4 in node3.Nodes)
                  {
                    string str4 = string.Empty;
                    string str5 = string.Empty;
                    string str6 = string.Empty;
                    string str7 = string.Empty;
                    string str8 = string.Empty;
                    bool focus = false;
                    foreach (TreeNode node5 in node4.Nodes)
                    {
                      string type = ((Data) node5.Tag).Type;
                      if (type != null)
                      {
                        if (!(type == "COMMAND"))
                        {
                          if (type == "SOUND")
                          {
                            goto Label_02C0;
                          }
                        }
                        else
                        {
                          str4 = (string) ((Data) node5.Tag).Parameter;
                          focus = ((Data) node5.Tag).Focus;
                          if (str4 != "KEY")
                          {
                            str5 = ((Data) node5.Tag).Value.ToString();
                          }
                          else
                          {
                            str4 = "ACTION";
                            Key key = (Key) ((Data) node5.Tag).Value;
                            str5 = "93";
                            str6 = key.KeyChar.ToString();
                            str7 = key.KeyCode.ToString();
                          }
                        }
                      }
                      continue;
                      Label_02C0:
                      str8 = (string) ((Data) node5.Tag).Value;
                    }
                    string parameter = (string) ((Data) node4.Tag).Parameter;
                    string str3 = ((Data) node4.Tag).Value.ToString();
                    string str = Convert.ToString(((Data) node3.Tag).Value);
                    writer.WriteStartElement("action");
                    writer.WriteAttributeString("layer", str);
                    writer.WriteAttributeString("condition", parameter);
                    writer.WriteAttributeString("conproperty", str3);
                    writer.WriteAttributeString("command", str4);
                    writer.WriteAttributeString("cmdproperty", str5);
                    if (str5 == Convert.ToInt32(Action.ActionType.ACTION_KEY_PRESSED).ToString())
                    {
                      if (str6 != string.Empty)
                      {
                        writer.WriteAttributeString("cmdkeychar", str6);
                      }
                      else
                      {
                        writer.WriteAttributeString("cmdkeychar", "0");
                      }
                      if (str7 != string.Empty)
                      {
                        writer.WriteAttributeString("cmdkeycode", str7);
                      }
                      else
                      {
                        writer.WriteAttributeString("cmdkeychar", "0");
                      }
                    }
                    if (str8 != string.Empty)
                    {
                      writer.WriteAttributeString("sound", str8);
                    }
                    if (focus)
                    {
                      writer.WriteAttributeString("focus", focus.ToString());
                    }
                    writer.WriteEndElement();
                  }
                }
              }
              writer.WriteEndElement();
            }
          }
          writer.WriteEndElement();
        }
      }
      writer.WriteEndElement();
      writer.WriteEndDocument();
      writer.Close();
      this.changedSettings = false;
      return true;
    }

    private bool SaveSettings()
    {
      string fileName = "MiniDisplay.xml";
      try
      {
        Directory.CreateDirectory(Config.GetFolder(Config.Dir.Config));
      }
      catch
      {
      }
      XmlTextWriter writer = new XmlTextWriter(Config.GetFile(Config.Dir.Config, fileName), Encoding.UTF8);
      writer.Formatting = Formatting.Indented;
      writer.Indentation = 2;
      writer.IndentChar = ' ';
      writer.WriteStartDocument(true);
      writer.WriteStartElement("Settings");
      for (int i = 0; i < this.SettingsNode.Nodes.Count; i++)
      {
        Data tag = (Data) this.SettingsNode.Nodes[i].Tag;
        writer.WriteAttributeString((string) tag.Parameter, (string) tag.Value);
      }
      foreach (TreeNode node in this.treeMapping.Nodes[0].Nodes)
      {
        Data data2 = (Data) node.Tag;
        Log.Info("processing DATA: Type = {0}, parameter = {1}, value = {2}",
                 new object[] {data2.Type, data2.Parameter, data2.Value});
        writer.WriteStartElement("Message");
        if (((string) data2.Value) != "ALL STATES")
        {
          writer.WriteAttributeString("Status", (string) data2.Value);
        }
        foreach (TreeNode node2 in node.Nodes)
        {
          Data data3 = (Data) node2.Tag;
          string type = data3.Type;
          if (type != null)
          {
            if (!(type == "WINDOWLIST"))
            {
              if (type == "LINE")
              {
                goto Label_028F;
              }
              if (type == "IMAGE")
              {
                goto Label_05EE;
              }
            }
            else
            {
              Log.Info("processing WINDOWLIST");
              foreach (TreeNode node3 in node2.Nodes)
              {
                Data data4 = (Data) node3.Tag;
                Log.Info("processing WINDOW: Type = {0}, parameter = {1}, value = {2}",
                         new object[] {data4.Type, data4.Parameter, data4.Value});
                if (((string) data4.Parameter) == "ID")
                {
                  writer.WriteElementString("Window", data4.Value.ToString());
                }
              }
            }
          }
          continue;
          Label_028F:
          writer.WriteStartElement("Line");
          writer.WriteAttributeString("Alignment", (string) data3.Value);
          Log.Info("processing LINE: Alignment = {0}", new object[] {data3.Type});
          foreach (TreeNode node4 in node2.Nodes)
          {
            Data data5 = (Data) node4.Tag;
            Log.Info("processing pData: Type = {0}, parameter = {1}, value = {2}",
                     new object[] {data5.Type, data5.Parameter, data5.Value});
            if (data5.Parameter.Equals("TextProgressBar"))
            {
              string[] strArray = ((string) data5.Value).Split(new char[] {'|'});
              writer.WriteStartElement((string) data5.Parameter);
              writer.WriteAttributeString("StartChar", strArray[0]);
              writer.WriteAttributeString("EndChar", strArray[1]);
              writer.WriteAttributeString("ValueChar", strArray[2]);
              writer.WriteAttributeString("FillChar", strArray[3]);
              writer.WriteAttributeString("Length", strArray[4]);
              writer.WriteStartElement("ValueProperty");
              writer.WriteAttributeString("Value", strArray[5]);
              writer.WriteEndElement();
              writer.WriteStartElement("TargetProperty");
              writer.WriteAttributeString("Value", strArray[6]);
              writer.WriteEndElement();
            }
            else
            {
              writer.WriteStartElement((string) data5.Parameter);
              writer.WriteAttributeString("Value", (string) data5.Value);
              foreach (TreeNode node5 in node4.Nodes)
              {
                string str4;
                Data data6 = (Data) node5.Tag;
                Log.Info("processing cData: Type = {0}, parameter = {1}, value = {2}",
                         new object[] {data6.Type, data6.Parameter, data6.Value});
                if (((str4 = (string) data6.Parameter) != null) && ((str4 == "And") || (str4 == "Or")))
                {
                  writer.WriteStartElement((string) data6.Parameter);
                  foreach (TreeNode node6 in node5.Nodes)
                  {
                    Data data7 = (Data) node6.Tag;
                    writer.WriteStartElement((string) data7.Parameter);
                    writer.WriteAttributeString("Value", (string) data7.Value);
                    writer.WriteEndElement();
                  }
                  writer.WriteEndElement();
                  continue;
                }
                writer.WriteStartElement((string) data6.Parameter);
                writer.WriteAttributeString("Value", (string) data6.Value);
                writer.WriteEndElement();
              }
            }
            writer.WriteEndElement();
          }
          writer.WriteEndElement();
          continue;
          Label_05EE:
          writer.WriteStartElement("Image");
          Log.Info("processing IMAGE");
          foreach (TreeNode node7 in node2.Nodes)
          {
            Data data8 = (Data) node7.Tag;
            Log.Info("processing pData: Type = {0}, parameter = {1}, value = {2}",
                     new object[] {data8.Type, data8.Parameter, data8.Value});
            writer.WriteStartElement((string) data8.Parameter);
            writer.WriteAttributeString("Value", (string) data8.Value);
            foreach (TreeNode node8 in node7.Nodes)
            {
              string str5;
              Data data9 = (Data) node8.Tag;
              Log.Info("processing cData: Type = {0}, parameter = {1}, value = {2}",
                       new object[] {data9.Type, data9.Parameter, data9.Value});
              if (((str5 = (string) data9.Parameter) != null) && ((str5 == "And") || (str5 == "Or")))
              {
                writer.WriteStartElement((string) data9.Parameter);
                foreach (TreeNode node9 in node8.Nodes)
                {
                  Data data10 = (Data) node9.Tag;
                  writer.WriteStartElement((string) data10.Parameter);
                  writer.WriteAttributeString("Value", (string) data10.Value);
                  writer.WriteEndElement();
                }
                writer.WriteEndElement();
                continue;
              }
              writer.WriteStartElement((string) data9.Parameter);
              writer.WriteAttributeString("Value", (string) data9.Value);
              writer.WriteEndElement();
            }
            writer.WriteEndElement();
          }
          writer.WriteEndElement();
        }
        writer.WriteEndElement();
      }
      Log.Info("PROCESSING MESSAGES COMPLETED");
      Log.Info("PROCESSING TRANSLATIONS");
      writer.WriteStartElement("TranslateFrom");
      foreach (TreeNode node10 in this.treeMapping.Nodes[1].Nodes)
      {
        Data data11 = (Data) node10.Tag;
        Log.Info("processing nodeFrom: Type = {0}, parameter = {1}, value = {2}",
                 new object[] {data11.Type, data11.Parameter, data11.Value});
        writer.WriteElementString("string", (string) data11.Parameter);
      }
      writer.WriteEndElement();
      writer.WriteStartElement("TranslateTo");
      foreach (TreeNode node11 in this.treeMapping.Nodes[1].Nodes)
      {
        Data data12 = (Data) node11.Tag;
        Log.Info("processing nodeTo: Type = {0}, parameter = {1}, value = {2}",
                 new object[] {data12.Type, data12.Parameter, data12.Value});
        writer.WriteElementString("string", (string) data12.Value);
      }
      writer.WriteEndElement();
      Log.Info("PROCESSING TRANSLATIONS COMPLETED");
      Log.Info("PROCESSING CHARACTERS");
      writer.WriteStartElement("CustomCharacters");
      foreach (TreeNode node12 in this.treeMapping.Nodes[2].Nodes)
      {
        writer.WriteStartElement("CustomCharacter");
        foreach (TreeNode node13 in node12.Nodes)
        {
          Data data13 = (Data) node13.Tag;
          Log.Info("processing CustomCharacter: Type = {0}, parameter = {1}, value = {2}",
                   new object[] {data13.Type, data13.Parameter, data13.Value});
          writer.WriteElementString("int", data13.Value.ToString());
        }
        writer.WriteEndElement();
      }
      writer.WriteEndElement();
      Log.Info("PROCESSING CHARACTERS COMPLETED");
      writer.WriteEndElement();
      writer.WriteEndDocument();
      writer.Close();      
      this.changedSettings = false;
      return true;
    }

    private void SetCharacterPixel(int Row, int Column, bool SetOn)
    {
      string key = "cbR" + Row.ToString().Trim() + "B" + Column.ToString().Trim();
      Control[] controlArray = this.groupboxCharacterEdit.Controls.Find(key, false);
      if (controlArray.Length > 0)
      {
        CheckBox box = (CheckBox) controlArray[0];
        if (SetOn)
        {
          box.CheckState = CheckState.Indeterminate;
        }
        else
        {
          box.CheckState = CheckState.Unchecked;
        }
      }
      else
      {
        Log.Info("CONTROL \"{0}\" NOT FOUND", new object[] {key});
      }
    }

    private void textBoxCondValue_LostFocus(object sender, EventArgs e)
    {
      TreeNode selectedNode = this.treeMapping.SelectedNode;
      Data tag = (Data) selectedNode.Tag;
      selectedNode.Tag = new Data(tag.Type, tag.Parameter, this.textBoxCondValue.Text);
      selectedNode.Text = tag.Parameter + ": " + this.textBoxCondValue.Text;
      this.changedSettings = true;
    }

    private void textBoxProcessValue_LostFocus(object sender, EventArgs e)
    {
      TreeNode selectedNode = this.treeMapping.SelectedNode;
      Data tag = (Data) selectedNode.Tag;
      selectedNode.Tag = new Data(tag.Type, tag.Parameter, this.textBoxProcessValue.Text);
      selectedNode.Text = tag.Parameter + ": " + this.textBoxProcessValue.Text;
      this.changedSettings = true;
    }

    private void TextBoxTranslateFrom_LostFocus(object sender, EventArgs e)
    {
      TreeNode selectedNode = this.treeMapping.SelectedNode;
      Data tag = (Data) selectedNode.Tag;
      if (tag.Type.Equals("TRANSLATION"))
      {
        selectedNode.Tag = new Data("TRANSLATION", this.TextBoxTranslateFrom.Text, tag.Value);
        selectedNode.Text =
          string.Concat(new object[] {"Translation: \"", this.TextBoxTranslateFrom.Text, "\" = \"", tag.Value, "\""});
        this.changedSettings = true;
      }
    }

    private void TextBoxTranslateTo_LostFocus(object sender, EventArgs e)
    {
      TreeNode selectedNode = this.treeMapping.SelectedNode;
      Data tag = (Data) selectedNode.Tag;
      if (tag.Type.Equals("TRANSLATION"))
      {
        selectedNode.Tag = new Data("TRANSLATION", tag.Parameter, this.TextBoxTranslateTo.Text);
        selectedNode.Text =
          string.Concat(new object[] {"Translation: \"", tag.Parameter, "\" = \"", this.TextBoxTranslateTo.Text, "\""});
        this.changedSettings = true;
      }
    }

    private void TextProgressBar_LostFocus(object sender, EventArgs e)
    {
      TreeNode selectedNode = this.treeMapping.SelectedNode;
      Data tag = (Data) selectedNode.Tag;
      string newValue = this.mpTPBStartChar.Text.Substring(0, 1) + "|" + this.mpTPBEndChar.Text.Substring(0, 1) + "|" +
                        this.mpTPBValueChar.Text.Substring(0, 1) + "|" + this.mpTPBFillChar.Text.Substring(0, 1) + "|" +
                        this.mpTPBlength.Text + "|" + this.mpTextBoxValueProperty.Text + "|" +
                        this.mpTextBoxTargetProperty.Text;
      selectedNode.Tag = new Data(tag.Type, tag.Parameter, newValue);
      this.changedSettings = true;
    }

    private void treeMapping_AfterSelect(object sender, TreeViewEventArgs e)
    {
      if (e.Action == TreeViewAction.Unknown)
      {
        return;
      }
      Log.Info("treeMapping_AfterSelect: PROCESSING Treemapping select click");
      TreeNode charBaseNode = e.Node;
      Log.Info("treeMapping_AfterSelect: FOUND selected node");
      if (charBaseNode.Tag == null)
      {
        Log.Info("treeMapping_AfterSelect: SELECTED NODE DOES NOT HAVE A DATA TAG");
        return;
      }
      Data tag = (Data) charBaseNode.Tag;
      Log.Info("treeMapping_AfterSelect: data - Type = {0}, Parameter = {1}, Value = {2}",
               new object[] {tag.Type, tag.Parameter, tag.Value});
      this.groupboxTranslationEdit.Visible = false;
      this.groupBoxMessageEdit.Visible = false;
      this.groupboxCharacterEdit.Visible = false;
      this.groupBoxTextProgressBar.Visible = false;
      this.groupBoxTextProgressBar.Enabled = false;
      this.CharacterEditNode = null;
      this.groupBoxStatus.Enabled = false;
      this.ComboBoxStatusProperty.Enabled = false;
      this.ComboBoxStatusProperty.SelectedItem = "";
      this.groupBoxWindow.Enabled = false;
      this.comboBoxWindowProperty.Enabled = false;
      this.comboBoxWindowProperty.Text = "";
      this.groupBoxMessageType.Enabled = false;
      this.comboBoxMessageType.Enabled = false;
      this.comboBoxMessageType.Text = "";
      this.groupBoxLine.Enabled = false;
      this.comboBoxAlignment.Enabled = false;
      this.comboBoxAlignment.Text = "";
      this.groupBoxProcess.Enabled = false;
      this.comboBoxProcessType.Enabled = false;
      this.comboBoxProcessType.Text = "";
      this.textBoxProcessValue.Enabled = false;
      this.textBoxProcessValue.Text = "";
      this.GroupBoxCondition.Enabled = false;
      this.ComboBoxCondType.Enabled = false;
      this.ComboBoxCondType.Text = "";
      this.textBoxCondValue.Enabled = false;
      this.textBoxCondValue.Text = "";
      this.buttonUp.Enabled = false;
      this.buttonDown.Enabled = false;
      this.buttonRemove.Enabled = false;
      this.buttonNew.Enabled = false;
      switch (tag.Type)
      {
        case "TRANSLATION":
          this.groupboxTranslationEdit.Visible = true;
          this.TextBoxTranslateFrom.Text = (string) tag.Parameter;
          this.TextBoxTranslateTo.Text = (string) tag.Value;
          this.buttonNew.Enabled = true;
          this.buttonRemove.Enabled = true;
          goto Label_0C73;

        case "CHARACTER":
          this.buttonNew.Enabled = true;
          this.buttonRemove.Enabled = true;
          this.groupboxCharacterEdit.Visible = true;
          this.CharacterEditNode = charBaseNode;
          this.DrawCustomCharacter(charBaseNode);
          charBaseNode.ExpandAll();
          goto Label_0C73;

        case "BYTE":
          if (charBaseNode.Parent.Parent.Nodes.Count < 9)
          {
            this.buttonNew.Enabled = true;
          }
          this.groupboxCharacterEdit.Visible = true;
          this.CharacterEditNode = charBaseNode.Parent;
          this.DrawCustomCharacter(charBaseNode.Parent);
          charBaseNode.Parent.ExpandAll();
          goto Label_0C73;

        case "SECTION":
          string str3;
          Log.Info("treeMapping_AfterSelect: Processing SECTION message.");
          if (((str3 = (string) tag.Parameter) != null) &&
              (((str3 == "STATUSMESSAGES") || (str3 == "CHARACTERTRANSLATIONS")) || (str3 == "CUSTOMCHARACTERS")))
          {
            this.buttonNew.Enabled = true;
          }
          goto Label_0C73;

        case "MESSAGE":
          Log.Info("treeMapping_AfterSelect: Processing {0} message.", new object[] {tag.Type});
          this.groupBoxStatus.Enabled = true;
          this.ComboBoxStatusProperty.Enabled = true;
          this.UpdateCombo(ref this.ComboBoxStatusProperty, this.StatusList, (string) tag.Value);
          this.buttonNew.Enabled = true;
          this.buttonRemove.Enabled = true;
          this.groupBoxMessageEdit.Visible = true;
          if (charBaseNode.Index > 0)
          {
            this.buttonUp.Enabled = true;
          }
          if (charBaseNode.Index < (charBaseNode.Parent.Nodes.Count - 1))
          {
            this.buttonDown.Enabled = true;
          }
          goto Label_0C73;

        case "WINDOWLIST":
          Log.Info("treeMapping_AfterSelect: Processing {0} message.", new object[] {tag.Type});
          this.buttonNew.Enabled = true;
          this.buttonRemove.Enabled = true;
          this.groupBoxMessageEdit.Visible = true;
          goto Label_0C73;

        case "WINDOW":
          Log.Info("treeMapping_AfterSelect: Processing WINDOW {0} message.", new object[] {tag.Parameter});
          this.groupBoxWindow.Enabled = true;
          this.comboBoxWindowProperty.Enabled = true;
          switch (((string) tag.Parameter))
          {
            case "ALL":
              this.UpdateCombo(ref this.comboBoxWindowProperty, this.windowsList, "ALL WINDOWS");
              break;

            case "ID":
              this.UpdateCombo(ref this.comboBoxWindowProperty, this.windowsList,
                               this.GetFriendlyName(Enum.GetName(typeof (GUIWindow.Window), Convert.ToInt32(tag.Value))));
              break;
          }
          break;

        case "LINE":
          Log.Info("treeMapping_AfterSelect: Processing LINE message. (index = {0}, count = {1})",
                   new object[] {charBaseNode.Index, charBaseNode.Parent.Nodes.Count});
          this.groupBoxMessageType.Enabled = true;
          this.comboBoxMessageType.Enabled = true;
          this.UpdateCombo(ref this.comboBoxMessageType, this.MessageTypeList, tag.Type);
          this.groupBoxLine.Enabled = true;
          this.comboBoxAlignment.Enabled = true;
          this.UpdateCombo(ref this.comboBoxAlignment, this.AlignmentList, (string) tag.Value);
          this.groupBoxMessageType.Enabled = true;
          this.comboBoxMessageType.Enabled = true;
          this.UpdateCombo(ref this.comboBoxMessageType, this.MessageTypeList, tag.Type);
          if ((charBaseNode.Index > 0) &&
              (((Data) charBaseNode.Parent.Nodes[charBaseNode.Index - 1].Tag).Type != "WINDOWLIST"))
          {
            Log.Info("treeMapping_AfterSelect: Processing LINE message. Enabling buttonUp");
            this.buttonUp.Enabled = true;
          }
          if (charBaseNode.Index < (charBaseNode.Parent.Nodes.Count - 1))
          {
            Log.Info("treeMapping_AfterSelect: Processing LINE message. enabling buttonDown");
            this.buttonDown.Enabled = true;
          }
          this.groupBoxMessageEdit.Visible = true;
          this.buttonNew.Enabled = true;
          this.buttonRemove.Enabled = true;
          goto Label_0C73;

        case "IMAGE":
          Log.Info("treeMapping_AfterSelect: Processing IMAGE message. (index = {0}, count = {1})",
                   new object[] {charBaseNode.Index, charBaseNode.Parent.Nodes.Count});
          this.groupBoxMessageType.Enabled = true;
          this.comboBoxMessageType.Enabled = true;
          this.UpdateCombo(ref this.comboBoxMessageType, this.MessageTypeList, tag.Type);
          this.UpdateCombo(ref this.comboBoxProcessType, this.ProcessListImage, (string) tag.Parameter);
          if ((charBaseNode.Index > 0) &&
              (((Data) charBaseNode.Parent.Nodes[charBaseNode.Index - 1].Tag).Type != "WINDOWLIST"))
          {
            Log.Info("treeMapping_AfterSelect: Processing IMAGE message. Enabling buttonUp");
            this.buttonUp.Enabled = true;
          }
          if (charBaseNode.Index < (charBaseNode.Parent.Nodes.Count - 1))
          {
            Log.Info("treeMapping_AfterSelect: Processing IMAGE message. enabling buttonDown");
            this.buttonDown.Enabled = true;
          }
          this.groupBoxMessageEdit.Visible = true;
          this.buttonNew.Enabled = true;
          this.buttonRemove.Enabled = true;
          goto Label_0C73;

        case "PROCESS":
          {
            Log.Info("treeMapping_AfterSelect: Processing PROCESS message.");
            this.groupBoxProcess.Enabled = true;
            this.comboBoxProcessType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBoxProcessType.Enabled = true;
            if (!tag.Parameter.Equals("TextProgressBar"))
            {
              this.textBoxProcessValue.Enabled = true;
              this.textBoxProcessValue.Text = (string) tag.Value;
            }
            else
            {
              Log.Info("treeMapping_AfterSelect: Processing TextProgressBar message.");
              string[] strArray = ((string) tag.Value).Split(new char[] {'|'});
              if (strArray.Length >= 7)
              {
                this.mpTPBStartChar.Text = strArray[0];
                this.mpTPBEndChar.Text = strArray[1];
                this.mpTPBValueChar.Text = strArray[2];
                this.mpTPBFillChar.Text = strArray[3];
                this.mpTPBlength.Text = strArray[4];
                this.mpTextBoxValueProperty.Text = strArray[5];
                this.mpTextBoxTargetProperty.Text = strArray[6];
              }
              else
              {
                this.mpTPBStartChar.Text = "[";
                this.mpTPBEndChar.Text = "]";
                this.mpTPBValueChar.Text = "*";
                this.mpTPBFillChar.Text = "-";
                this.mpTPBlength.Text = "8";
                this.mpTextBoxValueProperty.Text = "#currentplaytime";
                this.mpTextBoxTargetProperty.Text = "#duration";
              }
              this.groupBoxTextProgressBar.Visible = true;
              this.groupBoxTextProgressBar.Enabled = true;
              Log.Info("treeMapping_AfterSelect: Processed TextProgressBar message.");
            }
            Data data2 = (Data) charBaseNode.Parent.Tag;
            if (data2.Type.Equals("IMAGE"))
            {
              this.UpdateCombo(ref this.comboBoxProcessType, this.ProcessListImage, (string) tag.Parameter);
            }
            else
            {
              this.UpdateCombo(ref this.comboBoxProcessType, this.ProcessList, (string) tag.Parameter);
            }
            if (charBaseNode.Index > 0)
            {
              this.buttonUp.Enabled = true;
            }
            if (charBaseNode.Index < (charBaseNode.Parent.Nodes.Count - 1))
            {
              this.buttonDown.Enabled = true;
            }
            this.buttonNew.Enabled = true;
            this.buttonRemove.Enabled = true;
            this.groupBoxMessageEdit.Visible = true;
            goto Label_0C73;
          }
        case "CONDITION":
        case "SUBCONDITION":
          Log.Info("treeMapping_AfterSelect: Processing {0} message.", new object[] {tag.Parameter});
          this.groupBoxProcess.Enabled = true;
          this.comboBoxProcessType.Enabled = false;
          this.textBoxProcessValue.Enabled = false;
          this.textBoxCondValue.Enabled = true;
          this.GroupBoxCondition.Enabled = true;
          this.UpdateCombo(ref this.ComboBoxCondType, this.ConditionList, (string) tag.Parameter);
          this.textBoxCondValue.Text = (string) tag.Value;
          if (charBaseNode.Index > 0)
          {
            this.buttonUp.Enabled = true;
          }
          if (charBaseNode.Index < (charBaseNode.Parent.Nodes.Count - 1))
          {
            this.buttonDown.Enabled = true;
          }
          this.buttonNew.Enabled = true;
          this.buttonRemove.Enabled = true;
          this.groupBoxMessageEdit.Visible = true;
          goto Label_0C73;

        default:
          goto Label_0C73;
      }
      this.buttonNew.Enabled = true;
      this.buttonRemove.Enabled = true;
      this.groupBoxMessageEdit.Visible = true;
      Label_0C73:
      if ((tag.Type == "MESSAGE") || (tag.Type == "SECTION"))
      {
        return;
      }
      bool flag = false;
      TreeNode parent = charBaseNode.Parent;
      if (parent != null)
      {
        tag = (Data) parent.Tag;
        Log.Info("treeMapping_AfterSelect: First parent node: Type = {0}, Parameter = {1}, Value = {2}",
                 new object[] {tag.Type, tag.Parameter, tag.Value});
        while (!flag)
        {
          Log.Info("treeMapping_AfterSelect: Processing parent node: Type = {0}, Parameter = {1}, Value = {2}",
                   new object[] {tag.Type, tag.Parameter, tag.Value});
          switch (tag.Type)
          {
            case "SECTION":
              Log.Info("treeMapping_AfterSelect: Processing SECTION message for display.");
              flag = true;
              goto Label_1044;

            case "WINDOWLIST":
              Log.Info("treeMapping_AfterSelect: Processing {0} message for display.", new object[] {tag.Type});
              if (tag.Type == "SECTION")
              {
                flag = true;
              }
              goto Label_1044;

            case "MESSAGE":
              Log.Info("treeMapping_AfterSelect: Processing {0} message for display.", new object[] {tag.Type});
              this.UpdateCombo(ref this.ComboBoxStatusProperty, this.StatusList, (string) tag.Value);
              this.ComboBoxStatusProperty.Enabled = false;
              flag = true;
              goto Label_1044;

            case "WINDOW":
              Log.Info("treeMapping_AfterSelect: Processing WINDOW {0} message for display.",
                       new object[] {tag.Parameter});
              switch (((string) tag.Parameter))
              {
                case "ID":
                  goto Label_0EFB;
              }
              goto Label_0F32;

            case "LINE":
              Log.Info("treeMapping_AfterSelect: Processing LINE message for display.");
              this.UpdateCombo(ref this.comboBoxAlignment, this.AlignmentList, (string) tag.Value);
              this.comboBoxAlignment.Enabled = false;
              goto Label_1044;

            case "IMAGE":
              Log.Info("treeMapping_AfterSelect: Processing IMAGE message for display.");
              goto Label_1044;

            case "PROCESS":
              Log.Info("treeMapping_AfterSelect: Processing PROCESS message for display.");
              this.UpdateCombo(ref this.comboBoxProcessType, this.ProcessList, (string) tag.Parameter);
              this.textBoxProcessValue.Text = (string) tag.Value;
              this.comboBoxProcessType.Enabled = false;
              goto Label_1044;

            case "CONDITION":
            case "SUBCONDITION":
              Log.Info("treeMapping_AfterSelect: Processing {0} message for display.", new object[] {tag.Parameter});
              this.UpdateCombo(ref this.ComboBoxCondType, this.ConditionList, (string) tag.Parameter);
              this.textBoxCondValue.Text = (string) tag.Value;
              this.ComboBoxCondType.Enabled = false;
              goto Label_1044;

            default:
              goto Label_1044;
          }
          Label_0EFB:
          this.UpdateCombo(ref this.comboBoxWindowProperty, this.windowsList,
                           this.GetFriendlyName(Enum.GetName(typeof (GUIWindow.Window), Convert.ToInt32(tag.Value))));
          Label_0F32:
          this.comboBoxWindowProperty.Enabled = false;
          Label_1044:
          if (!flag)
          {
            parent = parent.Parent;
            tag = (Data) parent.Tag;
          }
        }
      }
    }

    private void UpdateCombo(ref MPComboBox comboBox, Array list, string hilight)
    {
      comboBox.Items.Clear();
      foreach (object obj2 in list)
      {
        comboBox.Items.Add(obj2.ToString());
      }
      comboBox.Text = hilight;
      comboBox.SelectedItem = hilight;
      comboBox.Enabled = true;
    }

    private void UpdateCombo(ref MPComboBox comboBox, ArrayList list, string hilight)
    {
      this.UpdateCombo(ref comboBox, list.ToArray(), hilight);
    }

    private class Data
    {
      private object dataValue;
      private bool focus;
      private object parameter;
      private string type;

      public Data(object newType, object newParameter, object newValue)
      {
        if (newValue == null)
        {
          newValue = string.Empty;
        }
        if (newParameter == null)
        {
          newParameter = string.Empty;
        }
        this.type = (string) newType;
        this.dataValue = newValue;
        this.parameter = newParameter;
      }

      public Data(object newType, object newParameter, object newValue, bool newFocus)
      {
        if (newValue == null)
        {
          newValue = string.Empty;
        }
        if (newParameter == null)
        {
          newParameter = string.Empty;
        }
        this.type = (string) newType;
        this.dataValue = newValue;
        this.parameter = newParameter;
        this.focus = newFocus;
      }

      public bool Focus
      {
        get { return this.focus; }
        set { this.focus = value; }
      }

      public object Parameter
      {
        get { return this.parameter; }
        set { this.parameter = value; }
      }

      public string Type
      {
        get { return this.type; }
      }

      public object Value
      {
        get { return this.dataValue; }
        set { this.dataValue = value; }
      }
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      this.Close();
    }
  }
}