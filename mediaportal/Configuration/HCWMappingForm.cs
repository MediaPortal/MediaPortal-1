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
    private System.Windows.Forms.TreeView treeMapping;
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
      PopulateTree("InputDeviceMappings\\defaults\\Hauppauge HCW.xml");
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
      this.treeMapping = new System.Windows.Forms.TreeView();
      this.SuspendLayout();
      // 
      // treeMapping
      // 
      this.treeMapping.ImageIndex = -1;
      this.treeMapping.Location = new System.Drawing.Point(32, 32);
      this.treeMapping.Name = "treeMapping";
      this.treeMapping.SelectedImageIndex = -1;
      this.treeMapping.Size = new System.Drawing.Size(368, 552);
      this.treeMapping.TabIndex = 1;
      // 
      // HCWMappingForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(432, 622);
      this.Controls.Add(this.treeMapping);
      this.Name = "HCWMappingForm";
      this.Text = "HCWMappingForm";
      this.ResumeLayout(false);

    }
		#endregion

    ArrayList remote;

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
    class RemoteMap
    {
      int       code;
      string    name;
      ArrayList mapping = new ArrayList();

      public int       Code    { get { return code;    } }
      public string    Name    { get { return name;    } }
      public ArrayList Mapping { get { return mapping; } }

      public RemoteMap(int newCode, string newName, ArrayList newMapping)
      {
        code    = newCode;
        name    = newName;
        mapping = newMapping;
      }
    }

    void PopulateTree(string xmlFile)
    {
      remote = new ArrayList();
      XmlDocument doc = new XmlDocument();
      doc.Load(xmlFile);
      XmlNodeList listButtons=doc.DocumentElement.SelectNodes("/mappings/button");
      foreach (XmlNode nodeButton in listButtons)
      {
        string name  = nodeButton.Attributes["name"].Value;
        int    value = Convert.ToInt32(nodeButton.Attributes["code"].Value);

        TreeNode buttonNode = new TreeNode(name);
        treeMapping.Nodes.Add(buttonNode);

        TreeNode layer0Node = new TreeNode("Layer 0");

        TreeNode layer1Node = new TreeNode("Layer 1");

        TreeNode layerAllNode = new TreeNode("all Layers");

        ArrayList mapping = new ArrayList();
        XmlNodeList listActions = nodeButton.SelectNodes("action");
        int conditionCount = 0;
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

          TreeNode conditionNode;

          string conditionString = null;

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
          conditionCount++;
          conditionNode = new TreeNode(conditionCount + ". " + conditionString);

          string commandString = null;

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

          TreeNode commandNode = new TreeNode(commandString);
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

        RemoteMap remoteMap = new RemoteMap(value, name, mapping);
        remote.Add(remoteMap);
      }
    }
	}
}
