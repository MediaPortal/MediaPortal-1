using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using MediaPortal.GUI.Library;

namespace MediaPortal.Configuration
{
  /// <summary>
  /// Summary description for HCWMappingForm.
  /// </summary>
  public class HCWMappingForm : System.Windows.Forms.Form
  {
    ArrayList remotesList    = new ArrayList();
    TreeNode	currentlySelectedNode;
    Array     windowsList    = Enum.GetValues(typeof(GUIWindow.Window));
    Array     actionList     = Enum.GetValues(typeof(Action.ActionType));
    string[]  fullScreenList = new string[] {"yes", "no"};
    string[]  playerList     = new string[] {"TV", "DVD", "MEDIA"};
    string[]  powerList      = new string[] {"EXIT", "REBOOT", "SHUTDOWN", "STANDBY", "HIBERNATE"};
    string[]  soundList      = new string[] {"back.wav", "click.wav", "cursor.wav"};
    string[]  keyList        = new string[] {"{BACKSPACE}", "{BREAK}", "{CAPSLOCK}", "{DELETE}", "{DOWN}", "{END}", "{ENTER}", "{ESC}",
                                              "{HELP}", "{HOME}", "{INSERT}", "{LEFT}", "{NUMLOCK}", "{PGDN}", "{PGUP}", "{PRTSC}",
                                              "{RIGHT}", "{SCROLLLOCK}", "{TAB}", "{UP}", "{F1}", "{F2}", "{F3}", "{F4}", "{F5}", "{F6}",
                                              "{F7}", "{F8}", "{F9}", "{F10}", "{F11}", "{F12}", "{F13}", "{F14}", "{F15}", "{F16}",
                                              "{ADD}", "{SUBTRACT}", "{MULTIPLY}", "{DIVIDE}"};

    /// <summary>
    /// Condition/action class
    /// </summary>
    class Mapping
    {
      string condition;
      string conProperty;
      int    layer;
      string command;
      string cmdProperty;
      string sound;

      public int    Layer       { get { return layer;       } }
      public string Condition   { get { return condition;   } }
      public string ConProperty { get { return conProperty; } }
      public string Command     { get { return command;     } }
      public string CmdProperty { get { return cmdProperty; } }
      public string Sound       { get { return sound;       } }

      public Mapping(int newLayer, string newCondition, string newConProperty, string newCommand, string newCmdProperty, string newSound)
      {
        layer       = newLayer;
        condition   = newCondition;
        conProperty = newConProperty;
        command     = newCommand;
        cmdProperty = newCmdProperty;
        sound       = newSound;
      }
    }


    /// <summary>
    /// Button/mapping class
    /// </summary>
    class RemoteButton
    {
      string    family;
      int       code;
      string    name;
      ArrayList mapping = new ArrayList();

      public string    Family  { get { return family;  } }
      public int       Code    { get { return code;    } }
      public string    Name    { get { return name;    } }
      public ArrayList Mapping { get { return mapping; } }

      public RemoteButton(string newFamily, int newCode, string newName, ArrayList newMapping)
      {
        family  = newFamily;
        code    = newCode;
        name    = newName;
        mapping = newMapping;
      }
    }


    class Remote
    {
      string name;
      ArrayList buttons = new ArrayList();

      public string    Name    { get { return name;    } }
      public ArrayList Buttons { get { return buttons; } }

      public Remote(string newName, ArrayList newButtons)
      {
        name    = newName;
        buttons = newButtons;
      }
    }


    private System.Windows.Forms.TreeView treeMapping;
    private System.Windows.Forms.RadioButton radioButtonWindow;
    private System.Windows.Forms.RadioButton radioButtonFullscreen;
    private System.Windows.Forms.RadioButton radioButtonPlaying;
    private System.Windows.Forms.RadioButton radioButtonNoCondition;
    private System.Windows.Forms.ComboBox comboBoxCondProperty;
    private System.Windows.Forms.ComboBox comboBoxCmdProperty;
    private System.Windows.Forms.GroupBox groupBoxCondition;
    private System.Windows.Forms.RadioButton radioButtonAction;
    private System.Windows.Forms.RadioButton radioButtonKey;
    private System.Windows.Forms.RadioButton radioButtonActWindow;
    private System.Windows.Forms.RadioButton radioButtonToggle;
    private System.Windows.Forms.RadioButton radioButtonPower;
    private System.Windows.Forms.ComboBox comboBoxSound;
    private System.Windows.Forms.Label labelSound;
    private System.Windows.Forms.GroupBox groupBoxAction;
    private MediaPortal.UserInterface.Controls.MPGradientLabel headerLabel;
    private System.Windows.Forms.Button applyButton;
    private System.Windows.Forms.Button okButton;
    private System.Windows.Forms.Button cancelButton;
    private MediaPortal.UserInterface.Controls.MPBeveledLine beveledLine1;
    private System.Windows.Forms.GroupBox groupBoxRemoteName;
    private System.Windows.Forms.ComboBox comboBoxRemoteName;
    private System.Windows.Forms.NumericUpDown numericUpDownLayer;
    private System.Windows.Forms.Label labelLayer;
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public HCWMappingForm()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();
      LoadMapping("InputDeviceMappings\\defaults\\Hauppauge HCW.xml");
      Remote remote = (Remote)remotesList[0];
      comboBoxRemoteName.SelectedItem = remote.Name;
      PopulateTree(remote.Buttons);
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
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(HCWMappingForm));
      this.treeMapping = new System.Windows.Forms.TreeView();
      this.radioButtonWindow = new System.Windows.Forms.RadioButton();
      this.radioButtonFullscreen = new System.Windows.Forms.RadioButton();
      this.radioButtonPlaying = new System.Windows.Forms.RadioButton();
      this.radioButtonNoCondition = new System.Windows.Forms.RadioButton();
      this.comboBoxCondProperty = new System.Windows.Forms.ComboBox();
      this.comboBoxCmdProperty = new System.Windows.Forms.ComboBox();
      this.groupBoxCondition = new System.Windows.Forms.GroupBox();
      this.radioButtonAction = new System.Windows.Forms.RadioButton();
      this.radioButtonKey = new System.Windows.Forms.RadioButton();
      this.radioButtonActWindow = new System.Windows.Forms.RadioButton();
      this.radioButtonToggle = new System.Windows.Forms.RadioButton();
      this.radioButtonPower = new System.Windows.Forms.RadioButton();
      this.groupBoxAction = new System.Windows.Forms.GroupBox();
      this.labelSound = new System.Windows.Forms.Label();
      this.comboBoxSound = new System.Windows.Forms.ComboBox();
      this.headerLabel = new MediaPortal.UserInterface.Controls.MPGradientLabel();
      this.applyButton = new System.Windows.Forms.Button();
      this.okButton = new System.Windows.Forms.Button();
      this.cancelButton = new System.Windows.Forms.Button();
      this.beveledLine1 = new MediaPortal.UserInterface.Controls.MPBeveledLine();
      this.groupBoxRemoteName = new System.Windows.Forms.GroupBox();
      this.labelLayer = new System.Windows.Forms.Label();
      this.numericUpDownLayer = new System.Windows.Forms.NumericUpDown();
      this.comboBoxRemoteName = new System.Windows.Forms.ComboBox();
      this.groupBoxCondition.SuspendLayout();
      this.groupBoxAction.SuspendLayout();
      this.groupBoxRemoteName.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLayer)).BeginInit();
      this.SuspendLayout();
      // 
      // treeMapping
      // 
      this.treeMapping.FullRowSelect = true;
      this.treeMapping.HideSelection = false;
      this.treeMapping.ImageIndex = -1;
      this.treeMapping.Location = new System.Drawing.Point(64, 62);
      this.treeMapping.Name = "treeMapping";
      this.treeMapping.SelectedImageIndex = -1;
      this.treeMapping.Size = new System.Drawing.Size(312, 386);
      this.treeMapping.TabIndex = 1;
      this.treeMapping.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeMapping_AfterSelect);
      // 
      // radioButtonWindow
      // 
      this.radioButtonWindow.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButtonWindow.Location = new System.Drawing.Point(24, 24);
      this.radioButtonWindow.Name = "radioButtonWindow";
      this.radioButtonWindow.Size = new System.Drawing.Size(88, 16);
      this.radioButtonWindow.TabIndex = 9;
      this.radioButtonWindow.Text = "Window";
      this.radioButtonWindow.Click += new System.EventHandler(this.radioButtonWindow_Click);
      // 
      // radioButtonFullscreen
      // 
      this.radioButtonFullscreen.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButtonFullscreen.Location = new System.Drawing.Point(112, 24);
      this.radioButtonFullscreen.Name = "radioButtonFullscreen";
      this.radioButtonFullscreen.Size = new System.Drawing.Size(88, 16);
      this.radioButtonFullscreen.TabIndex = 10;
      this.radioButtonFullscreen.Text = "Fullscreen";
      this.radioButtonFullscreen.Click += new System.EventHandler(this.radioButtonFullscreen_Click);
      // 
      // radioButtonPlaying
      // 
      this.radioButtonPlaying.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButtonPlaying.Location = new System.Drawing.Point(24, 48);
      this.radioButtonPlaying.Name = "radioButtonPlaying";
      this.radioButtonPlaying.Size = new System.Drawing.Size(88, 16);
      this.radioButtonPlaying.TabIndex = 11;
      this.radioButtonPlaying.Text = "Playing";
      this.radioButtonPlaying.Click += new System.EventHandler(this.radioButtonPlaying_Click);
      // 
      // radioButtonNoCondition
      // 
      this.radioButtonNoCondition.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButtonNoCondition.Location = new System.Drawing.Point(112, 48);
      this.radioButtonNoCondition.Name = "radioButtonNoCondition";
      this.radioButtonNoCondition.Size = new System.Drawing.Size(88, 16);
      this.radioButtonNoCondition.TabIndex = 12;
      this.radioButtonNoCondition.Text = "No condition";
      this.radioButtonNoCondition.Click += new System.EventHandler(this.radioButtonNoCondition_Click);
      // 
      // comboBoxCondProperty
      // 
      this.comboBoxCondProperty.Location = new System.Drawing.Point(24, 72);
      this.comboBoxCondProperty.Name = "comboBoxCondProperty";
      this.comboBoxCondProperty.Size = new System.Drawing.Size(176, 21);
      this.comboBoxCondProperty.Sorted = true;
      this.comboBoxCondProperty.TabIndex = 13;
      // 
      // comboBoxCmdProperty
      // 
      this.comboBoxCmdProperty.Items.AddRange(new object[] {
                                                             "test"});
      this.comboBoxCmdProperty.Location = new System.Drawing.Point(24, 96);
      this.comboBoxCmdProperty.Name = "comboBoxCmdProperty";
      this.comboBoxCmdProperty.Size = new System.Drawing.Size(176, 21);
      this.comboBoxCmdProperty.Sorted = true;
      this.comboBoxCmdProperty.TabIndex = 14;
      // 
      // groupBoxCondition
      // 
      this.groupBoxCondition.Controls.Add(this.radioButtonWindow);
      this.groupBoxCondition.Controls.Add(this.radioButtonFullscreen);
      this.groupBoxCondition.Controls.Add(this.radioButtonPlaying);
      this.groupBoxCondition.Controls.Add(this.radioButtonNoCondition);
      this.groupBoxCondition.Controls.Add(this.comboBoxCondProperty);
      this.groupBoxCondition.Enabled = false;
      this.groupBoxCondition.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBoxCondition.Location = new System.Drawing.Point(400, 160);
      this.groupBoxCondition.Name = "groupBoxCondition";
      this.groupBoxCondition.Size = new System.Drawing.Size(224, 112);
      this.groupBoxCondition.TabIndex = 15;
      this.groupBoxCondition.TabStop = false;
      this.groupBoxCondition.Text = "Condition";
      // 
      // radioButtonAction
      // 
      this.radioButtonAction.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButtonAction.Location = new System.Drawing.Point(24, 24);
      this.radioButtonAction.Name = "radioButtonAction";
      this.radioButtonAction.Size = new System.Drawing.Size(88, 16);
      this.radioButtonAction.TabIndex = 14;
      this.radioButtonAction.Text = "Action";
      this.radioButtonAction.Click += new System.EventHandler(this.radioButtonAction_Click);
      // 
      // radioButtonKey
      // 
      this.radioButtonKey.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButtonKey.Location = new System.Drawing.Point(112, 24);
      this.radioButtonKey.Name = "radioButtonKey";
      this.radioButtonKey.Size = new System.Drawing.Size(88, 16);
      this.radioButtonKey.TabIndex = 16;
      this.radioButtonKey.Text = "Keystroke";
      this.radioButtonKey.Click += new System.EventHandler(this.radioButtonKey_Click);
      // 
      // radioButtonActWindow
      // 
      this.radioButtonActWindow.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButtonActWindow.Location = new System.Drawing.Point(24, 48);
      this.radioButtonActWindow.Name = "radioButtonActWindow";
      this.radioButtonActWindow.Size = new System.Drawing.Size(88, 16);
      this.radioButtonActWindow.TabIndex = 14;
      this.radioButtonActWindow.Text = "Window";
      this.radioButtonActWindow.Click += new System.EventHandler(this.radioButtonActWindow_Click);
      // 
      // radioButtonToggle
      // 
      this.radioButtonToggle.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButtonToggle.Location = new System.Drawing.Point(112, 48);
      this.radioButtonToggle.Name = "radioButtonToggle";
      this.radioButtonToggle.Size = new System.Drawing.Size(88, 16);
      this.radioButtonToggle.TabIndex = 17;
      this.radioButtonToggle.Text = "Toggle Layer";
      this.radioButtonToggle.Click += new System.EventHandler(this.radioButtonToggle_Click);
      // 
      // radioButtonPower
      // 
      this.radioButtonPower.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButtonPower.Location = new System.Drawing.Point(24, 72);
      this.radioButtonPower.Name = "radioButtonPower";
      this.radioButtonPower.Size = new System.Drawing.Size(112, 16);
      this.radioButtonPower.TabIndex = 18;
      this.radioButtonPower.Text = "Powerdown action";
      this.radioButtonPower.Click += new System.EventHandler(this.radioButtonPower_Click);
      // 
      // groupBoxAction
      // 
      this.groupBoxAction.Controls.Add(this.labelSound);
      this.groupBoxAction.Controls.Add(this.comboBoxSound);
      this.groupBoxAction.Controls.Add(this.radioButtonAction);
      this.groupBoxAction.Controls.Add(this.radioButtonKey);
      this.groupBoxAction.Controls.Add(this.radioButtonActWindow);
      this.groupBoxAction.Controls.Add(this.radioButtonToggle);
      this.groupBoxAction.Controls.Add(this.radioButtonPower);
      this.groupBoxAction.Controls.Add(this.comboBoxCmdProperty);
      this.groupBoxAction.Enabled = false;
      this.groupBoxAction.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBoxAction.Location = new System.Drawing.Point(400, 280);
      this.groupBoxAction.Name = "groupBoxAction";
      this.groupBoxAction.Size = new System.Drawing.Size(224, 168);
      this.groupBoxAction.TabIndex = 16;
      this.groupBoxAction.TabStop = false;
      this.groupBoxAction.Text = "Action";
      // 
      // labelSound
      // 
      this.labelSound.Location = new System.Drawing.Point(24, 128);
      this.labelSound.Name = "labelSound";
      this.labelSound.Size = new System.Drawing.Size(40, 16);
      this.labelSound.TabIndex = 20;
      this.labelSound.Text = "Sound:";
      // 
      // comboBoxSound
      // 
      this.comboBoxSound.Location = new System.Drawing.Point(72, 128);
      this.comboBoxSound.Name = "comboBoxSound";
      this.comboBoxSound.Size = new System.Drawing.Size(128, 21);
      this.comboBoxSound.Sorted = true;
      this.comboBoxSound.TabIndex = 19;
      // 
      // headerLabel
      // 
      this.headerLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.headerLabel.Caption = "Remote control mapping";
      this.headerLabel.FirstColor = System.Drawing.SystemColors.InactiveCaption;
      this.headerLabel.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.headerLabel.LastColor = System.Drawing.Color.WhiteSmoke;
      this.headerLabel.Location = new System.Drawing.Point(16, 16);
      this.headerLabel.Name = "headerLabel";
      this.headerLabel.PaddingLeft = 2;
      this.headerLabel.Size = new System.Drawing.Size(656, 24);
      this.headerLabel.TabIndex = 17;
      this.headerLabel.TextColor = System.Drawing.Color.WhiteSmoke;
      this.headerLabel.TextFont = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      // 
      // applyButton
      // 
      this.applyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.applyButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.applyButton.Location = new System.Drawing.Point(450, 485);
      this.applyButton.Name = "applyButton";
      this.applyButton.TabIndex = 20;
      this.applyButton.Text = "Apply";
      this.applyButton.Visible = false;
      // 
      // okButton
      // 
      this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.okButton.Location = new System.Drawing.Point(530, 485);
      this.okButton.Name = "okButton";
      this.okButton.TabIndex = 19;
      this.okButton.Text = "OK";
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.cancelButton.Location = new System.Drawing.Point(609, 485);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.TabIndex = 18;
      this.cancelButton.Text = "Cancel";
      // 
      // beveledLine1
      // 
      this.beveledLine1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.beveledLine1.Location = new System.Drawing.Point(8, 475);
      this.beveledLine1.Name = "beveledLine1";
      this.beveledLine1.Size = new System.Drawing.Size(676, 2);
      this.beveledLine1.TabIndex = 21;
      // 
      // groupBoxRemoteName
      // 
      this.groupBoxRemoteName.Controls.Add(this.labelLayer);
      this.groupBoxRemoteName.Controls.Add(this.numericUpDownLayer);
      this.groupBoxRemoteName.Controls.Add(this.comboBoxRemoteName);
      this.groupBoxRemoteName.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBoxRemoteName.Location = new System.Drawing.Point(400, 56);
      this.groupBoxRemoteName.Name = "groupBoxRemoteName";
      this.groupBoxRemoteName.Size = new System.Drawing.Size(224, 96);
      this.groupBoxRemoteName.TabIndex = 22;
      this.groupBoxRemoteName.TabStop = false;
      this.groupBoxRemoteName.Text = "Remote";
      // 
      // labelLayer
      // 
      this.labelLayer.Location = new System.Drawing.Point(24, 58);
      this.labelLayer.Name = "labelLayer";
      this.labelLayer.Size = new System.Drawing.Size(40, 16);
      this.labelLayer.TabIndex = 16;
      this.labelLayer.Text = "Layer:";
      // 
      // numericUpDownLayer
      // 
      this.numericUpDownLayer.Location = new System.Drawing.Point(72, 56);
      this.numericUpDownLayer.Minimum = new System.Decimal(new int[] {
                                                                       1,
                                                                       0,
                                                                       0,
                                                                       -2147483648});
      this.numericUpDownLayer.Name = "numericUpDownLayer";
      this.numericUpDownLayer.Size = new System.Drawing.Size(128, 20);
      this.numericUpDownLayer.TabIndex = 15;
      // 
      // comboBoxRemoteName
      // 
      this.comboBoxRemoteName.Location = new System.Drawing.Point(24, 24);
      this.comboBoxRemoteName.Name = "comboBoxRemoteName";
      this.comboBoxRemoteName.Size = new System.Drawing.Size(176, 21);
      this.comboBoxRemoteName.TabIndex = 14;
      this.comboBoxRemoteName.SelectedIndexChanged += new System.EventHandler(this.comboBoxRemoteName_SelectedIndexChanged);
      // 
      // HCWMappingForm
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.CancelButton = this.cancelButton;
      this.ClientSize = new System.Drawing.Size(692, 516);
      this.Controls.Add(this.groupBoxRemoteName);
      this.Controls.Add(this.beveledLine1);
      this.Controls.Add(this.applyButton);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.cancelButton);
      this.Controls.Add(this.headerLabel);
      this.Controls.Add(this.groupBoxCondition);
      this.Controls.Add(this.treeMapping);
      this.Controls.Add(this.groupBoxAction);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MinimumSize = new System.Drawing.Size(700, 550);
      this.Name = "HCWMappingForm";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Media Portal - Setup";
      this.groupBoxCondition.ResumeLayout(false);
      this.groupBoxAction.ResumeLayout(false);
      this.groupBoxRemoteName.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLayer)).EndInit();
      this.ResumeLayout(false);

    }
    #endregion


    void LoadMapping(string xmlFile)
    {
      XmlDocument doc = new XmlDocument();
      doc.Load(xmlFile);
      XmlNodeList listRemotes=doc.DocumentElement.SelectNodes("/mappings/remote");
      
      foreach (XmlNode nodeRemote in listRemotes)
      {
        comboBoxRemoteName.Items.Add(nodeRemote.Attributes["family"].Value);

        ArrayList buttonList = new ArrayList();

        XmlNodeList listButtons=nodeRemote.SelectNodes("button");
        foreach (XmlNode nodeButton in listButtons)
        {
          string name  = nodeButton.Attributes["name"].Value;

          int code = Convert.ToInt32(nodeButton.Attributes["code"].Value);
        
          ArrayList mapping = new ArrayList();
          XmlNodeList listActions = nodeButton.SelectNodes("action");
        
          foreach (XmlNode nodeAction in listActions)
          {
            string condition   = nodeAction.Attributes["condition"].Value.ToUpper();
            string conProperty = nodeAction.Attributes["conproperty"].Value.ToUpper();
            string command     = nodeAction.Attributes["command"].Value.ToUpper();
            string cmdProperty = nodeAction.Attributes["cmdproperty"].Value.ToUpper();
            string sound       = nodeAction.Attributes["sound"].Value;
            int    layer       = Convert.ToInt32(nodeAction.Attributes["layer"].Value);
            Mapping conditionMap = new Mapping(layer, condition, conProperty, command, cmdProperty, sound);
            mapping.Add(conditionMap);
          }
          RemoteButton remoteButton = new RemoteButton(nodeRemote.Attributes["family"].Value, code, name, mapping);
          buttonList.Add(remoteButton);
        }
        Remote remote = new Remote(nodeRemote.Attributes["family"].Value, buttonList);
        remotesList.Add(remote);
      }
    }



    void PopulateTree(ArrayList buttonList)
    {
      treeMapping.Nodes.Clear();
      foreach (RemoteButton remoteButton in buttonList)
      {
        int conditionCount = 0;

        TreeNode buttonNode   = new TreeNode(remoteButton.Name);
        TreeNode layer0Node   = new TreeNode("Layer 0");
        TreeNode layer1Node   = new TreeNode("Layer 1");
        TreeNode layerAllNode = new TreeNode("all Layers");
        
        treeMapping.Nodes.Add(buttonNode);

        foreach (Mapping conditionMap in remoteButton.Mapping)
        {
          conditionCount++;
          string conditionString = null;
          string commandString = null;
          string condition   = conditionMap.Condition;
          string conProperty = conditionMap.ConProperty;
          string command     = conditionMap.Command;
          string cmdProperty = conditionMap.CmdProperty;
          string sound       = conditionMap.Sound;
          int    layer       = conditionMap.Layer;

          #region Conditions

          switch (condition)
          {
            case "WINDOW":
              conditionString = Enum.GetName(typeof(GUIWindow.Window), Convert.ToInt32(conProperty));
              break;
            case "FULLSCREEN":
              if (conProperty == "TRUE")
                conditionString = "Fullscreen";
              else
                conditionString = "No fullscreen";
              break;
            case "PLAYER":
            switch (conProperty)
            {
              case "TV":
                conditionString = "TV playing";
                break;
              case "DVD":
                conditionString = "DVD playing";
                break;
              case "PLAYING":
                conditionString = "Media playing";
                break;
            }
              break;
            case "*":
              conditionString = "Wildcard";
              break;
          }

          #endregion
          #region Commands

          switch (command)
          {
            case "ACTION":
              commandString = Enum.GetName(typeof(Action.ActionType), Convert.ToInt32(cmdProperty));
              break;
            case "KEY":
              commandString = "Key \"" + cmdProperty + "\"";
              break;
            case "WINDOW":
              commandString = Enum.GetName(typeof(GUIWindow.Window), Convert.ToInt32(cmdProperty));
              break;
            case "TOGGLE":
              commandString = "Toggle Layer";
              break;
            case "POWER":
            switch (cmdProperty)
            {
              case "EXIT":
                commandString = "Exit Media Portal";
                break;
              case "REBOOT":
                commandString = "Reboot Windows";
                break;
              case "SHUTDOWN":
                commandString = "Shutdown Windows";
                break;
              case "STANDBY":
                commandString = "Suspend Windows (Standby)";
                break;
              case "HIBERNATE":
                commandString = "Hibernate Windows";
                break;
            }
              break;
          }

          #endregion

          TreeNode conditionNode = new TreeNode(conditionCount + ". " + conditionString);
          TreeNode commandNode = new TreeNode(commandString);
          conditionNode.Tag = conditionMap;
          conditionNode.Nodes.Add(commandNode);

          if ((command != "KEY") && (sound != ""))
            conditionNode.Nodes.Add("Sound: " + sound);
          if (layer == 0) layer0Node.Nodes.Add(conditionNode);
          if (layer == 1) layer1Node.Nodes.Add(conditionNode);
          if (layer == -1) layerAllNode.Nodes.Add(conditionNode);
        }
        if (layer0Node.Nodes.Count > 0) buttonNode.Nodes.Add(layer0Node);
        if (layer1Node.Nodes.Count > 0) buttonNode.Nodes.Add(layer1Node);
        if (layerAllNode.Nodes.Count > 0) buttonNode.Nodes.Add(layerAllNode);
      }
    }
    


    
    private void treeMapping_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
    {
      currentlySelectedNode = e.Node;

      if (currentlySelectedNode != null)
      {
        Mapping conditionMap = null;

        if (currentlySelectedNode.Tag != null)
          conditionMap = (Mapping)currentlySelectedNode.Tag;
        else if ((currentlySelectedNode.Parent != null) && (currentlySelectedNode.Parent.Tag != null))
          conditionMap = (Mapping)currentlySelectedNode.Parent.Tag;
        else
        {
          groupBoxCondition.Enabled = false;
          groupBoxAction.Enabled = false;
          return;
        }

        numericUpDownLayer.Value = conditionMap.Layer;

        switch (conditionMap.Condition)
        {
          case "WINDOW":
            radioButtonWindow.Checked = true;
            comboBoxCondProperty.Enabled = true;
            UpdateCombo(ref comboBoxCondProperty, windowsList, Enum.GetName(typeof(GUIWindow.Window), Convert.ToInt32(conditionMap.ConProperty)));
            break;
          case "FULLSCREEN":
            radioButtonFullscreen.Checked = true;
            comboBoxCondProperty.Enabled = true;
            if (Convert.ToBoolean(conditionMap.ConProperty))
              UpdateCombo(ref comboBoxCondProperty, fullScreenList, "yes");
            else
              UpdateCombo(ref comboBoxCondProperty, fullScreenList, "no");
            break;
          case "PLAYER":
            radioButtonPlaying.Checked = true;
            comboBoxCondProperty.Enabled = true;
            UpdateCombo(ref comboBoxCondProperty, playerList, conditionMap.ConProperty);
            break;
          case "*":
            radioButtonNoCondition.Checked = true;
            comboBoxCondProperty.Enabled = false;
            comboBoxCondProperty.DataSource = null;
            comboBoxCondProperty.Items.Clear();
            comboBoxCondProperty.Text = "";
            break;
        }

        switch (conditionMap.Command)
        {
          case "ACTION":
            radioButtonAction.Checked = true;
            comboBoxSound.Enabled = true;
            comboBoxCmdProperty.Enabled = true;
            UpdateCombo(ref comboBoxCmdProperty, actionList, Enum.GetName(typeof(Action.ActionType), Convert.ToInt32(conditionMap.CmdProperty)));
            break;
          case "KEY":
            radioButtonKey.Checked = true;
            comboBoxSound.Enabled = false;
            comboBoxCmdProperty.Enabled = true;
            UpdateCombo(ref comboBoxCmdProperty, keyList, conditionMap.CmdProperty);
            break;
          case "WINDOW":
            radioButtonActWindow.Checked = true;
            comboBoxSound.Enabled = true;
            comboBoxCmdProperty.Enabled = true;
            UpdateCombo(ref comboBoxCmdProperty, windowsList, Enum.GetName(typeof(GUIWindow.Window), Convert.ToInt32(conditionMap.CmdProperty)));
            break;
          case "TOGGLE":
            radioButtonToggle.Checked = true;
            comboBoxSound.Enabled = true;
            comboBoxCmdProperty.Enabled = false;
            comboBoxCmdProperty.DataSource = null;
            comboBoxCmdProperty.Items.Clear();
            comboBoxCmdProperty.Text = "";
            break;
          case "POWER":
            radioButtonPower.Checked = true;
            comboBoxSound.Enabled = true;
            comboBoxCmdProperty.Enabled = true;
            UpdateCombo(ref comboBoxCmdProperty, powerList, conditionMap.CmdProperty);
            break;
        }

        comboBoxSound.Text = conditionMap.Sound;
        UpdateCombo(ref comboBoxSound, soundList, conditionMap.Sound);

        groupBoxCondition.Enabled = true;
        groupBoxAction.Enabled = true;
      }
    }

    public void UpdateCombo(ref ComboBox comboBox, Array list, string hilight)
    {
      comboBox.DataSource = null;
      comboBox.Items.Clear();
      comboBox.DataSource = list;
      comboBox.Text = hilight;
      comboBox.SelectedItem = hilight;
    }

    private void radioButtonWindow_Click(object sender, System.EventArgs e)
    {
      if (radioButtonWindow.Checked)
      {
        comboBoxCondProperty.Enabled = true;
        Mapping conditionMap = (Mapping)currentlySelectedNode.Tag;
        UpdateCombo(ref comboBoxCondProperty, windowsList, Enum.GetName(typeof(GUIWindow.Window), Convert.ToInt32(conditionMap.ConProperty)));
      }
    }

    private void radioButtonFullscreen_Click(object sender, System.EventArgs e)
    {
      if (radioButtonFullscreen.Checked)
      {
        comboBoxCondProperty.Enabled = true;
        Mapping conditionMap = (Mapping)currentlySelectedNode.Tag;
        if (Convert.ToBoolean(conditionMap.ConProperty))
          UpdateCombo(ref comboBoxCondProperty, fullScreenList, "yes");
        else
          UpdateCombo(ref comboBoxCondProperty, fullScreenList, "no");
      }
    }

    private void radioButtonPlaying_Click(object sender, System.EventArgs e)
    {
      if (radioButtonPlaying.Checked)
      {
        comboBoxCondProperty.Enabled = true;
        Mapping conditionMap = (Mapping)currentlySelectedNode.Tag;
        UpdateCombo(ref comboBoxCondProperty, playerList, conditionMap.ConProperty);
      }
    }

    private void radioButtonNoCondition_Click(object sender, System.EventArgs e)
    {
      if (radioButtonNoCondition.Checked)
      {
        comboBoxCondProperty.Enabled = false;
        comboBoxCondProperty.Items.Clear();
        comboBoxCondProperty.Text = "";
      }
    }

    private void radioButtonAction_Click(object sender, System.EventArgs e)
    {
      comboBoxSound.Enabled = true;
      comboBoxCmdProperty.Enabled = true;
      Mapping conditionMap = (Mapping)currentlySelectedNode.Tag;
      UpdateCombo(ref comboBoxCmdProperty, actionList, Enum.GetName(typeof(Action.ActionType), Convert.ToInt32(conditionMap.CmdProperty)));
    }

    private void radioButtonKey_Click(object sender, System.EventArgs e)
    {
      comboBoxSound.Enabled = false;
      comboBoxCmdProperty.Enabled = true;
      Mapping conditionMap = (Mapping)currentlySelectedNode.Tag;
      UpdateCombo(ref comboBoxCmdProperty, keyList, conditionMap.CmdProperty);
    }

    private void radioButtonActWindow_Click(object sender, System.EventArgs e)
    {
      comboBoxSound.Enabled = true;
      comboBoxCmdProperty.Enabled = true;
      Mapping conditionMap = (Mapping)currentlySelectedNode.Tag;
      UpdateCombo(ref comboBoxCmdProperty, windowsList, Enum.GetName(typeof(GUIWindow.Window), Convert.ToInt32(conditionMap.CmdProperty)));
    }

    private void radioButtonToggle_Click(object sender, System.EventArgs e)
    {
      comboBoxSound.Enabled = true;
      comboBoxCmdProperty.Enabled = false;
      comboBoxCmdProperty.DataSource = null;
      comboBoxCmdProperty.Items.Clear();
      comboBoxCmdProperty.Text = "";
    }

    private void radioButtonPower_Click(object sender, System.EventArgs e)
    {
      comboBoxSound.Enabled = true;
      comboBoxCmdProperty.Enabled = true;
      Mapping conditionMap = (Mapping)currentlySelectedNode.Tag;
      UpdateCombo(ref comboBoxCmdProperty, powerList, conditionMap.CmdProperty);
    }

    private void comboBoxRemoteName_SelectedIndexChanged(object sender, System.EventArgs e)
    {
      foreach (Remote remote in remotesList)
      {
        if (remote.Name == comboBoxRemoteName.SelectedItem.ToString())
          PopulateTree(remote.Buttons);
      }
    }

    private void okButton_Click(object sender, System.EventArgs e)
    {
      MessageBox.Show(this, "This feature is work in progress.\nMappings are not saved yet.");
      this.Close();
    }
  }
}
