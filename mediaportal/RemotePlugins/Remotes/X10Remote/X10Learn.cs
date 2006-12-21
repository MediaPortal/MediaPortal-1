using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using System.Threading;
using MediaPortal.InputDevices;
using MediaPortal.Remotes.X10Remote;



namespace MediaPortal.Remotes.X10Remote
{
  public partial class X10Learn : Form
  {

    string m_sRemoteModel;
    bool m_bchangedsettings = false;

    public X10Learn(string Remotemodel)
    {
      m_sRemoteModel = Remotemodel;
      LoadMapping(Remotemodel + ".xml", false);
      Log.Info("X10Learn: Loaded Remote Mapping");
      InitializeComponent();
      InitializeListView(); 
    }

    //Initialize the listview

    private void InitializeListView()
    {
     // this.mpListView1.

    }



    //Button control
    private void mpOK_Click(object sender, EventArgs e)
    {
      if(m_bchangedsettings)
        SaveMapping(m_sRemoteModel + ".xml");

      this.Close();
    }

    private void mpApply_Click(object sender, EventArgs e)
    {
      if (m_bchangedsettings)
        SaveMapping(m_sRemoteModel + ".xml");

      this.Close();

    }

    private void mpCancel_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void ButtonStartLearn_Click(object sender, EventArgs e)
    {

    }

    private void ButtonEndLearn_Click(object sender, EventArgs e)
    {

    }

    private void ButtonSetChannel_Click(object sender, EventArgs e)
    {

    }


    //Input mapping functions

    void LoadMapping(string xmlFile, bool defaults)
    {
      try
      {
        
      //  XmlDocument doc = new XmlDocument();
      //  string path = "InputDeviceMappings\\defaults\\" + xmlFile;
      //  if (!defaults && File.Exists(Config.GetFile(Config.Dir.CustomInputDevice, xmlFile)))
      //    path = Config.GetFile(Config.Dir.CustomInputDevice, xmlFile);
      //  if (!File.Exists(path))
      //  {
      //    MessageBox.Show("Can't locate mapping file " + xmlFile + "\n\nMake sure it exists in /InputDeviceMappings/defaults", "Mapping file missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
      //    this.ShowInTaskbar = true;
      //    this.WindowState = FormWindowState.Minimized;
      //    return;
      //  }
      //  doc.Load(path);

      //  XmlNodeList listRemotes = doc.DocumentElement.SelectNodes("/mappings/remote");

      //  foreach (XmlNode nodeRemote in listRemotes)
      //  {
      //    TreeNode remoteNode = new TreeNode(nodeRemote.Attributes["family"].Value);
      //    remoteNode.Tag = new Data("REMOTE", null, nodeRemote.Attributes["family"].Value);
      //    XmlNodeList listButtons = nodeRemote.SelectNodes("button");
      //    foreach (XmlNode nodeButton in listButtons)
      //    {
      //      TreeNode buttonNode = new TreeNode((string)nodeButton.Attributes["name"].Value);
      //      buttonNode.Tag = new Data("BUTTON", nodeButton.Attributes["name"].Value, nodeButton.Attributes["code"].Value);
      //      remoteNode.Nodes.Add(buttonNode);

      //      TreeNode layer1Node = new TreeNode("Layer 1");
      //      TreeNode layer2Node = new TreeNode("Layer 2");
      //      TreeNode layerAllNode = new TreeNode("All Layers");
      //      layer1Node.Tag = new Data("LAYER", null, "1");
      //      layer2Node.Tag = new Data("LAYER", null, "2");
      //      layerAllNode.Tag = new Data("LAYER", null, "0");
      //      layer1Node.ForeColor = Color.DimGray;
      //      layer2Node.ForeColor = Color.DimGray;
      //      layerAllNode.ForeColor = Color.DimGray;

      //      XmlNodeList listActions = nodeButton.SelectNodes("action");

      //      foreach (XmlNode nodeAction in listActions)
      //      {
      //        string conditionString = string.Empty;
      //        string commandString = string.Empty;

      //        string condition = nodeAction.Attributes["condition"].Value.ToUpper();
      //        string conProperty = nodeAction.Attributes["conproperty"].Value.ToUpper();
      //        string command = nodeAction.Attributes["command"].Value.ToUpper();
      //        string cmdProperty = nodeAction.Attributes["cmdproperty"].Value.ToUpper();
      //        string sound = string.Empty;
      //        XmlAttribute soundAttribute = nodeAction.Attributes["sound"];
      //        if (soundAttribute != null)
      //          sound = soundAttribute.Value;
      //        bool gainFocus = false;
      //        XmlAttribute focusAttribute = nodeAction.Attributes["focus"];
      //        if (focusAttribute != null)
      //          gainFocus = Convert.ToBoolean(focusAttribute.Value);
      //        int layer = Convert.ToInt32(nodeAction.Attributes["layer"].Value);
      //      }
      //    }
      //  }
        
       
      
      //  changedSettings = false;
      }
      catch (Exception ex)
     {
        Log.Error(ex);
      //  File.Delete(Config.GetFile(Config.Dir.CustomInputDevice, xmlFile));
      //  LoadMapping(xmlFile, true);
      }
    }

    bool SaveMapping(string xmlFile)
    {
     try
     {
       DirectoryInfo dir = Directory.CreateDirectory(Config.GetFolder(Config.Dir.CustomInputDevice));
     }
     catch
     {
       Log.Info("MAP: Error accessing directory \"InputDeviceMappings\\custom\"");
     }

      ////try

      //{
      //  XmlTextWriter writer = new XmlTextWriter(Config.GetFile(Config.Dir.CustomInputDevice, xmlFile), System.Text.Encoding.UTF8);
      //  writer.Formatting = Formatting.Indented;
      //  writer.Indentation = 1;
      //  writer.IndentChar = (char)9;
      //  writer.WriteStartDocument(true);
      //  writer.WriteStartElement("mappings"); // <mappings>
      //  writer.WriteAttributeString("version", "3");
      //  if (treeMapping.Nodes.Count > 0)
      //    foreach (TreeNode remoteNode in treeMapping.Nodes)
      //    {
      //      writer.WriteStartElement("remote"); // <remote>
      //      writer.WriteAttributeString("family", (string)((Data)remoteNode.Tag).Value);
      //      if (remoteNode.Nodes.Count > 0)
      //        foreach (TreeNode buttonNode in remoteNode.Nodes)
      //        {
      //          writer.WriteStartElement("button"); // <button>
      //          writer.WriteAttributeString("name", (string)((Data)buttonNode.Tag).Parameter);
      //          writer.WriteAttributeString("code", (string)((Data)buttonNode.Tag).Value);

      //          if (buttonNode.Nodes.Count > 0)
      //            foreach (TreeNode layerNode in buttonNode.Nodes)
      //            {
      //              foreach (TreeNode conditionNode in layerNode.Nodes)
      //              {
      //                string layer;
      //                string condition;
      //                string conProperty;
      //                string command = string.Empty;
      //                string cmdProperty = string.Empty;
      //                string cmdKeyChar = string.Empty;
      //                string cmdKeyCode = string.Empty;
      //                string sound = string.Empty;
      //                bool focus = false;
      //                foreach (TreeNode commandNode in conditionNode.Nodes)
      //                {
      //                  switch (((Data)commandNode.Tag).Type)
      //                  {
      //                    case "COMMAND":
      //                      {
      //                        command = (string)((Data)commandNode.Tag).Parameter;
      //                        focus = ((Data)commandNode.Tag).Focus;
      //                        if (command != "KEY")
      //                          cmdProperty = ((Data)commandNode.Tag).Value.ToString();
      //                        else
      //                        {
      //                          command = "ACTION";
      //                          Key key = (Key)((Data)commandNode.Tag).Value;
      //                          cmdProperty = "93";
      //                          cmdKeyChar = key.KeyChar.ToString();
      //                          cmdKeyCode = key.KeyCode.ToString();
      //                        }
      //                      }
      //                      break;
      //                    case "SOUND":
      //                      sound = (string)((Data)commandNode.Tag).Value;
      //                      break;
      //                  }
      //                }
      //                condition = (string)((Data)conditionNode.Tag).Parameter;
      //                conProperty = ((Data)conditionNode.Tag).Value.ToString();
      //                layer = Convert.ToString(((Data)layerNode.Tag).Value);
      //                writer.WriteStartElement("action"); // <action>
      //                writer.WriteAttributeString("layer", layer);
      //                writer.WriteAttributeString("condition", condition);
      //                writer.WriteAttributeString("conproperty", conProperty);
      //                writer.WriteAttributeString("command", command);
      //                writer.WriteAttributeString("cmdproperty", cmdProperty);
      //                if (cmdProperty == Convert.ToInt32(Action.ActionType.ACTION_KEY_PRESSED).ToString())
      //                {
      //                  if (cmdKeyChar != string.Empty)
      //                  {
      //                    writer.WriteAttributeString("cmdkeychar", cmdKeyChar);
      //                  }
      //                  else
      //                  {
      //                    writer.WriteAttributeString("cmdkeychar", "0");
      //                  }
      //                  if (cmdKeyCode != string.Empty)
      //                  {
      //                    writer.WriteAttributeString("cmdkeycode", cmdKeyCode);
      //                  }
      //                  else
      //                  {
      //                    writer.WriteAttributeString("cmdkeychar", "0");
      //                  }

      //                }
      //                if (sound != string.Empty)
      //                  writer.WriteAttributeString("sound", sound);
      //                if (focus)
      //                  writer.WriteAttributeString("focus", focus.ToString());
      //                writer.WriteEndElement(); // </action>
      //              }
      //            }
      //          writer.WriteEndElement(); // </button>
      //        }
      //      writer.WriteEndElement(); // </remote>
      //    }
      //  writer.WriteEndElement(); // </mapping>
      //  writer.WriteEndDocument();
      //  writer.Close();
      //  changedSettings = false;
      //  return true;
      //}
     return true;

    }

  }
}