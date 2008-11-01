using System;
using System.IO;
using System.Xml;
using MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers;
using MediaPortal.GUI.Library;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin
{
  public class XMLUTILS
  {
    private static string xWidth = "262";

    private static XmlNode BuildMenuNode(XmlDocument doc, string nodeType, int ID, string nodeLabel, string Link, string nodeFont, string xPos, string yPos, string Width, string Height, string OnUp, string OnDown)
    {
      XmlNode node = doc.CreateNode(XmlNodeType.Element, "control", "");
      node.AppendChild(doc.CreateElement("description")).AppendChild(doc.CreateTextNode(nodeLabel.ToString()));
      node.AppendChild(doc.CreateElement("type")).AppendChild(doc.CreateTextNode(nodeType));
      node.AppendChild(doc.CreateElement("id")).AppendChild(doc.CreateTextNode(ID.ToString()));
      if (!xPos.Equals(""))
      {
        node.AppendChild(doc.CreateElement("posX")).AppendChild(doc.CreateTextNode(xPos.ToString()));
      }
      if (!yPos.Equals(""))
      {
        node.AppendChild(doc.CreateElement("posY")).AppendChild(doc.CreateTextNode(yPos.ToString()));
      }
      if (!Width.Equals(""))
      {
        node.AppendChild(doc.CreateElement("width")).AppendChild(doc.CreateTextNode(Width.ToString()));
      }
      else
      {
        node.AppendChild(doc.CreateElement("width")).AppendChild(doc.CreateTextNode("700"));
      }
      if (!Height.Equals(""))
      {
        node.AppendChild(doc.CreateElement("height")).AppendChild(doc.CreateTextNode(Height.ToString()));
      }
      node.AppendChild(doc.CreateElement("label")).AppendChild(doc.CreateTextNode(nodeLabel.ToString()));
      if (!nodeFont.Equals(""))
      {
        node.AppendChild(doc.CreateElement("font")).AppendChild(doc.CreateTextNode(nodeFont.ToString()));
      }
      if (!Link.Equals(""))
      {
        node.AppendChild(doc.CreateElement("hyperlink")).AppendChild(doc.CreateTextNode(Link.ToString()));
      }
      if (!OnUp.Equals(""))
      {
        node.AppendChild(doc.CreateElement("onup")).AppendChild(doc.CreateTextNode(OnUp.ToString()));
      }
      if (!OnDown.Equals(""))
      {
        node.AppendChild(doc.CreateElement("ondown")).AppendChild(doc.CreateTextNode(OnDown.ToString()));
      }
      return node;
    }

    private static XmlNodeList BuildMenuXML_BackLight(XmlDocument doc)
    {
      XmlDocumentFragment fragment = doc.CreateDocumentFragment();
      fragment.AppendChild(BuildMenuNode(doc, "selectbutton", 0x41, "Red", "", "", "", "", xWidth, "", "17", ""));
      fragment.AppendChild(BuildMenuNode(doc, "selectbutton", 0x42, "Green", "", "", "", "", xWidth, "", "", ""));
      fragment.AppendChild(BuildMenuNode(doc, "selectbutton", 0x43, "Blue", "", "", "", "", xWidth, "", "", ""));
      fragment.AppendChild(BuildMenuNode(doc, "togglebutton", 0x44, "Invert Display", "", "", "", "", xWidth, "", "", "99"));
      return fragment.SelectNodes("control");
    }

    private static XmlNodeList BuildMenuXML_DisplayControl(XmlDocument doc)
    {
      MiniDisplay.DisplayControl control = LoadDisplayControlSettings();
      XmlDocumentFragment fragment = doc.CreateDocumentFragment();
      fragment.AppendChild(BuildMenuNode(doc, "togglebutton", 40, "Blank Display with Video", "", "", "", "", xWidth, "", "17", ""));
      if (control.BlankDisplayWithVideo)
      {
        fragment.AppendChild(BuildMenuNode(doc, "togglebutton", 0x29, "Enable Display Actions", "", "", "", "", xWidth, "", "", ""));
        if (control.EnableDisplayAction)
        {
          fragment.AppendChild(BuildMenuNode(doc, "selectbutton", 0x2a, "Action Display Time", "", "", "", "", xWidth, "", "", ""));
        }
      }
      fragment.AppendChild(BuildMenuNode(doc, "togglebutton", 0x2b, "Blank Display on Idle", "", "", "", "", xWidth, "", "", ""));
      if (control.BlankDisplayWhenIdle)
      {
        fragment.AppendChild(BuildMenuNode(doc, "selectbutton", 0x2c, "Idle Delay", "", "", "", "", xWidth, "", "", "99"));
      }
      return fragment.SelectNodes("control");
    }

    private static XmlNodeList BuildMenuXML_DisplayOptions(XmlDocument doc)
    {
      XmlDocumentFragment fragment = doc.CreateDocumentFragment();
      MiniDisplay.DisplayOptions options = new MiniDisplay.DisplayOptions();
      string type = Settings.Instance.Type;
      if (type != null)
      {
        if (!(type == "MatrixGX"))
        {
          if (type == "iMONLCDg")
          {
            options = LoadDisplayOptionsSettings();
            fragment.AppendChild(BuildMenuNode(doc, "togglebutton", 30, "Volume Display", "", "", "", "", xWidth, "", "17", ""));
            fragment.AppendChild(BuildMenuNode(doc, "togglebutton", 0x1f, "Progress display", "", "", "", "", xWidth, "", "", ""));
            fragment.AppendChild(BuildMenuNode(doc, "togglebutton", 0x20, "Disk Icon", "", "", "", "", xWidth, "", "", ""));
            if (options.DiskIcon)
            {
              fragment.AppendChild(BuildMenuNode(doc, "togglebutton", 0x21, "Media Status", "", "", "", "", xWidth, "", "", ""));
              fragment.AppendChild(BuildMenuNode(doc, "togglebutton", 0x22, "CD/DVD Status", "", "", "", "", xWidth, "", "", ""));
            }
            fragment.AppendChild(BuildMenuNode(doc, "togglebutton", 0x23, "Use Custom Font", "", "", "", "", xWidth, "", "", ""));
            fragment.AppendChild(BuildMenuNode(doc, "togglebutton", 0x24, "Use Large Icons", "", "", "", "", xWidth, "", "", ""));
            if (options.UseLargeIcons)
            {
              fragment.AppendChild(BuildMenuNode(doc, "togglebutton", 0x25, "Use Custom Large Icons", "", "", "", "", xWidth, "", "", ""));
              fragment.AppendChild(BuildMenuNode(doc, "togglebutton", 0x26, "Invert Large Icons", "", "", "", "", xWidth, "", "", ""));
            }
            if (options.UseCustomFont)
            {
              fragment.AppendChild(BuildMenuNode(doc, "button", 0x27, "Font Editor", "", "", "", "", xWidth, "", "", ""));
            }
            if (options.UseLargeIcons)
            {
              fragment.AppendChild(BuildMenuNode(doc, "button", 40, "Icon Editor", "", "", "", "", xWidth, "", "", ""));
            }
          }
          else if ((type == "VLSYS_Mplay") || (type == "MatrixMX"))
          {
          }
        }
        else
        {
          options = LoadDisplayOptionsSettings();
          fragment.AppendChild(BuildMenuNode(doc, "togglebutton", 30, "Volume Display", "", "", "", "", xWidth, "", "17", ""));
          fragment.AppendChild(BuildMenuNode(doc, "togglebutton", 0x1f, "Progress display", "", "", "", "", xWidth, "", "", ""));
          fragment.AppendChild(BuildMenuNode(doc, "togglebutton", 0x20, "Disk Icon", "", "", "", "", xWidth, "", "", ""));
          if (options.DiskIcon)
          {
            fragment.AppendChild(BuildMenuNode(doc, "togglebutton", 0x21, "Media Status", "", "", "", "", xWidth, "", "", ""));
          }
        }
      }
      return fragment.SelectNodes("control");
    }

    private static XmlNodeList BuildMenuXML_Equalizer(XmlDocument doc)
    {
      MiniDisplay.EQControl control = LoadEqualizerSettings();
      XmlDocumentFragment fragment = doc.CreateDocumentFragment();
      fragment.AppendChild(BuildMenuNode(doc, "togglebutton", 20, "Use Equalizer", "", "", "", "", xWidth, "", "17", ""));
      if (control.UseEqDisplay)
      {
        fragment.AppendChild(BuildMenuNode(doc, "selectbutton", 0x15, "Equalizer Style", "", "", "", "", xWidth, "", "", ""));
        fragment.AppendChild(BuildMenuNode(doc, "togglebutton", 0x16, "Use Equalizer Smoothing", "", "", "", "", xWidth, "", "", ""));
        fragment.AppendChild(BuildMenuNode(doc, "togglebutton", 0x17, "Delay Start", "", "", "", "", xWidth, "", "", ""));
        if (control.DelayEQ)
        {
          fragment.AppendChild(BuildMenuNode(doc, "selectbutton", 0x18, "Delay Time", "", "", "", "", xWidth, "", "", ""));
        }
        fragment.AppendChild(BuildMenuNode(doc, "togglebutton", 0x19, "Show Title", "", "", "", "", xWidth, "", "", ""));
        if (control.EQTitleDisplay)
        {
          fragment.AppendChild(BuildMenuNode(doc, "selectbutton", 0x1a, "Title Time", "", "", "", "", xWidth, "", "", ""));
          fragment.AppendChild(BuildMenuNode(doc, "selectbutton", 0x1b, "Title Frequency", "", "", "", "", xWidth, "", "", ""));
        }
      }
      return fragment.SelectNodes("control");
    }

    private static XmlNodeList BuildMenuXML_KeyPad(XmlDocument doc)
    {
      MatrixMX.KeyPadControl control = LoadKeyPadSettings();
      XmlDocumentFragment fragment = doc.CreateDocumentFragment();
      string type = Settings.Instance.Type;
      if (type != null)
      {
        if (!(type == "CFontz") && !(type == "MatrixMX"))
        {
          if (((type == "VLSYS_Mplay") || (type == "MatrixGX")) || (type == "iMONLCDg"))
          {
          }
        }
        else
        {
          fragment.AppendChild(BuildMenuNode(doc, "togglebutton", 60, "Enable Keypad", "", "", "", "", xWidth, "", "17", ""));
          if (control.EnableKeyPad)
          {
            fragment.AppendChild(BuildMenuNode(doc, "togglebutton", 0x3d, "Custom Mapping", "", "", "", "", xWidth, "", "", ""));
            if (control.EnableCustom)
            {
              fragment.AppendChild(BuildMenuNode(doc, "button", 0x3e, "KeyPad Mapping", "", "", "", "", xWidth, "", "", "99"));
            }
          }
          goto Label_018B;
        }
      }
      fragment.AppendChild(BuildMenuNode(doc, "label", 50, "NO KEYPAD OPTIONS", "", "", "", "", xWidth, "", "17", ""));
    Label_018B:
      return fragment.SelectNodes("control");
    }

    private static XmlNodeList BuildMenuXML_MainMenu(XmlDocument doc)
    {
      XmlDocumentFragment fragment = doc.CreateDocumentFragment();
      switch (Settings.Instance.Type)
      {
        case "iMONLCDg":
          {
            int num2 = 0x4da8;
            fragment.AppendChild(BuildMenuNode(doc, "button", 3, "Display Options", num2.ToString(), "", "", "", xWidth, "", "17", ""));
            int num3 = 0x4da9;
            fragment.AppendChild(BuildMenuNode(doc, "button", 4, "Display Control Options", num3.ToString(), "", "", "", xWidth, "", "", ""));
            int num4 = 0x4da7;
            fragment.AppendChild(BuildMenuNode(doc, "button", 5, "Equalizer Options", num4.ToString(), "", "", "", xWidth, "", "", ""));
            fragment.AppendChild(BuildMenuNode(doc, "togglebutton", 6, "Monitor Power States", "", "", "", "", xWidth, "", "", ""));
            fragment.AppendChild(BuildMenuNode(doc, "selectbutton", 7, "Contrast", "", "", "", "", xWidth, "", "", ""));
            break;
          }
        case "VLSYS_Mplay":
          {
            int num5 = 0x4da9;
            fragment.AppendChild(BuildMenuNode(doc, "button", 3, "Display Control Options", num5.ToString(), "", "", "", xWidth, "", "17", ""));
            int num6 = 0x4da7;
            fragment.AppendChild(BuildMenuNode(doc, "button", 4, "Equalizer Options", num6.ToString(), "", "", "", xWidth, "", "", ""));
            int num7 = 0x4daa;
            fragment.AppendChild(BuildMenuNode(doc, "button", 5, "Remote Options", num7.ToString(), "", "", "", xWidth, "", "", ""));
            fragment.AppendChild(BuildMenuNode(doc, "selectbutton", 7, "Contrast", "", "", "", "", xWidth, "", "", ""));
            break;
          }
        case "MD8800":
          {
            int num8 = 0x4da9;
            fragment.AppendChild(BuildMenuNode(doc, "button", 3, "Display Control Options", num8.ToString(), "", "", "", xWidth, "", "17", ""));
            fragment.AppendChild(BuildMenuNode(doc, "selectbutton", 7, "Contrast", "", "", "", "", xWidth, "", "", ""));
            break;
          }
        case "CFontz":
          {
            int num9 = 0x4dab;
            fragment.AppendChild(BuildMenuNode(doc, "button", 3, "Keypad Options", num9.ToString(), "", "", xWidth, "", "", "17", ""));
            int num10 = 0x4da9;
            fragment.AppendChild(BuildMenuNode(doc, "button", 4, "Display Control Options", num10.ToString(), "", "", xWidth, "", "", "", ""));
            int num11 = 0x4da7;
            fragment.AppendChild(BuildMenuNode(doc, "button", 5, "Equalizer Options", num11.ToString(), "", "", "", xWidth, "", "", ""));
            fragment.AppendChild(BuildMenuNode(doc, "selectbutton", 7, "Contrast", "", "", "", "", xWidth, "", "", ""));
            break;
          }
        case "MatrixMX":
          {
            int num12 = 0x4dab;
            fragment.AppendChild(BuildMenuNode(doc, "button", 3, "Keypad Options", num12.ToString(), "", "", xWidth, "", "", "17", ""));
            int num13 = 0x4da9;
            fragment.AppendChild(BuildMenuNode(doc, "button", 4, "Display Control Options", num13.ToString(), "", "", xWidth, "", "", "", ""));
            int num14 = 0x4da7;
            fragment.AppendChild(BuildMenuNode(doc, "button", 5, "Equalizer Options", num14.ToString(), "", "", "", xWidth, "", "", ""));
            fragment.AppendChild(BuildMenuNode(doc, "selectbutton", 7, "Contrast", "", "", "", "", xWidth, "", "", ""));
            break;
          }
        case "MatrixGX":
          {
            int num15 = 0x4da8;
            fragment.AppendChild(BuildMenuNode(doc, "button", 3, "Display Options", num15.ToString(), "", "", "", xWidth, "", "17", ""));
            int num16 = 0x4da9;
            fragment.AppendChild(BuildMenuNode(doc, "button", 4, "Display Control Options", num16.ToString(), "", "", "", xWidth, "", "", ""));
            int num17 = 0x4da7;
            fragment.AppendChild(BuildMenuNode(doc, "button", 5, "Equalizer Options", num17.ToString(), "", "", "", xWidth, "", "", ""));
            int num18 = 0x4dac;
            fragment.AppendChild(BuildMenuNode(doc, "button", 6, "Backlight Options", num18.ToString(), "", "", "", xWidth, "", "", ""));
            fragment.AppendChild(BuildMenuNode(doc, "selectbutton", 7, "Contrast", "", "", "", "", xWidth, "", "", ""));
            break;
          }
        default:
          fragment.AppendChild(BuildMenuNode(doc, "label", 3, "Plugin is not configured.", "", "", "", "", xWidth, "", "17", "99"));
          fragment.AppendChild(BuildMenuNode(doc, "label", 4, "Please run configure.exe first!", "", "", "", "", xWidth, "", "17", "99"));
          break;
      }
      return fragment.SelectNodes("control");
    }

    private static XmlNodeList BuildMenuXML_Remote(XmlDocument doc)
    {
      VLSYS_Mplay.RemoteControl control = LoadRemoteSettings();
      XmlDocumentFragment fragment = doc.CreateDocumentFragment();
      string type = Settings.Instance.Type;
      if (type != null)
      {
        if (!(type == "VLSYS_Mplay"))
        {
          if (((type == "MatrixMX") || (type == "MatrixGX")) || (type == "iMONLCDg"))
          {
          }
        }
        else
        {
          fragment.AppendChild(BuildMenuNode(doc, "togglebutton", 50, "Disable Remote", "", "", "", "", xWidth, "", "17", ""));
          if (!control.DisableRemote)
          {
            fragment.AppendChild(BuildMenuNode(doc, "togglebutton", 0x33, "Disable Repeat", "", "", "", "", xWidth, "", "", ""));
            if (!control.DisableRepeat)
            {
              fragment.AppendChild(BuildMenuNode(doc, "selectbutton", 0x34, "Repeat Delay", "", "", "", "", xWidth, "", "", ""));
            }
            fragment.AppendChild(BuildMenuNode(doc, "button", 0x35, "Remote Mapping", "", "", "", "", xWidth, "", "", "99"));
          }
          goto Label_01BC;
        }
      }
      fragment.AppendChild(BuildMenuNode(doc, "label", 50, "NO REMOTE OPTIONS", "", "", "", "", xWidth, "", "17", ""));
    Label_01BC:
      return fragment.SelectNodes("control");
    }

    private static void CleanNodes(XmlDocument doc, ref XmlNodeList cNodes)
    {
      foreach (XmlNode node in cNodes)
      {
        if (node.SelectSingleNode("id") != null)
        {
          string innerText = node.SelectSingleNode("id").InnerText;
          if ((innerText.Equals("3") || innerText.Equals("4")) || innerText.Equals("5"))
          {
            node.ParentNode.RemoveChild(node);
          }
        }
      }
    }

    public static string Create_GUI_Menu(GUIMenu WhichMenu)
    {
      int num = 0x4da6;
      int num2 = 3;
      if ((GUIGraphicsContext.Skin.Contains("BlueTwo") | GUIGraphicsContext.Skin.Contains("PureVisionHD")) | GUIGraphicsContext.Skin.Contains("Xface"))
      {
        xWidth = "300";
      }
      else
      {
        xWidth = string.Empty;
      }
      string filename = GUIGraphicsContext.Skin + @"\settings.xml";
      string str2 = Get_GUI_Menu_Filename(WhichMenu);
      string str3 = string.Empty;
      string str4 = "MiniDisplay - " + Settings.Instance.Type;
      XmlDocument doc = new XmlDocument();
      doc.Load(filename);
      bool flag = true;
      XmlNode parentNode = doc.DocumentElement.GetElementsByTagName("layout").Item(0);
      if (parentNode != null)
      {
        parentNode = parentNode.ParentNode;
      }
      else
      {
        flag = false;
        parentNode = doc.DocumentElement.GetElementsByTagName("controls").Item(0);
        if (parentNode == null)
        {
          Log.Info("MiniDisplay.XMLUTILS.Create_GUI_Menu():  Skin Template not compatible! Can not create menu file \"{0}\".", new object[] { str2 });
          return string.Empty;
        }
      }
      XmlNodeList list = null;
      switch (WhichMenu)
      {
        case GUIMenu.MainMenu:
          num = 0x4da6;
          num2 = 3;
          str3 = "Settings Menu";
          list = BuildMenuXML_MainMenu(doc);
          break;

        case GUIMenu.DisplayOptions:
          num = 0x4da8;
          num2 = 30;
          str3 = "Display Options";
          list = BuildMenuXML_DisplayOptions(doc);
          break;

        case GUIMenu.DisplayControl:
          num = 0x4da9;
          num2 = 40;
          str3 = "Display Control Options";
          list = BuildMenuXML_DisplayControl(doc);
          break;

        case GUIMenu.Equalizer:
          num = 0x4da7;
          num2 = 20;
          str3 = "Equalizer Options";
          list = BuildMenuXML_Equalizer(doc);
          break;

        case GUIMenu.Remote:
          num = 0x4daa;
          num2 = 50;
          str3 = "Remote Control Options";
          list = BuildMenuXML_Remote(doc);
          break;

        case GUIMenu.BackLight:
          num = 0x4dac;
          num2 = 0x41;
          str3 = "BackLight Options";
          list = BuildMenuXML_BackLight(doc);
          break;

        case GUIMenu.KeyPad:
          num = 0x4dab;
          num2 = 60;
          str3 = "KeyPad Options";
          list = BuildMenuXML_KeyPad(doc);
          break;

        case GUIMenu.Fan:
          break;

        default:
          num = 0x4da6;
          num2 = 20;
          str3 = "UNKNOWN MENU";
          break;
      }
      doc.GetElementsByTagName("id").Item(0).InnerText = num.ToString();
      doc.GetElementsByTagName("defaultcontrol").Item(0).InnerText = num2.ToString();
      doc.GetElementsByTagName("define").Item(0).InnerText = "#header.label:" + str4 + "\n" + str3;
      XmlNodeList list2 = parentNode.SelectNodes("control");
      if (flag)
      {
        foreach (XmlNode node2 in list2)
        {
          if ((node2.SelectSingleNode("type") != null) && (node2.SelectSingleNode("type").InnerText == "button"))
          {
            node2.ParentNode.RemoveChild(node2);
          }
        }
        foreach (XmlNode node3 in list)
        {
          parentNode.AppendChild(node3);
        }
      }
      else
      {
        XmlNode newChild = doc.CreateNode(XmlNodeType.Element, "control", "");
        newChild.AppendChild(doc.CreateElement("description")).AppendChild(doc.CreateTextNode("GUI Controls"));
        newChild.AppendChild(doc.CreateElement("type")).AppendChild(doc.CreateTextNode("group"));
        newChild.AppendChild(doc.CreateElement("id")).AppendChild(doc.CreateTextNode("999"));
        newChild.AppendChild(doc.CreateElement("posX")).AppendChild(doc.CreateTextNode("280"));
        newChild.AppendChild(doc.CreateElement("posY")).AppendChild(doc.CreateTextNode("400"));
        newChild.AppendChild(doc.CreateElement("layout")).AppendChild(doc.CreateTextNode("StackLayout"));
        foreach (XmlNode node5 in list2)
        {
          if ((node5.SelectSingleNode("type") != null) && (node5.SelectSingleNode("type").InnerText == "button"))
          {
            node5.ParentNode.RemoveChild(node5);
          }
        }
        foreach (XmlNode node6 in list)
        {
          newChild.AppendChild(node6);
        }
        parentNode.AppendChild(newChild);
      }
      doc.Save(str2);
      return str2;
    }

    public static void Delete_GUI_Menu(GUIMenu WhichMenu)
    {
      string path = Get_GUI_Menu_Filename(WhichMenu);
      try
      {
        File.Delete(path);
        Log.Info("MiniDisplay.XMLUTILS.Delete_GUI_Menu(): Deleted \"{0}\"", new object[] { path });
      } catch
      {
        Log.Info("MiniDisplay.XMLUTILS.Delete_GUI_Menu(): COULD NOT DELETE \"{0}\"", new object[] { path });
      }
    }

    public static string Get_GUI_Menu_Filename(GUIMenu WhichMenu)
    {
      string str = string.Empty;
      switch (WhichMenu)
      {
        case GUIMenu.MainMenu:
          str = @"\MiniDisplay_GUI_Setup.xml";
          break;

        case GUIMenu.DisplayOptions:
          str = @"\MiniDisplay_GUI_DisplayOptions.xml";
          break;

        case GUIMenu.DisplayControl:
          str = @"\MiniDisplay_GUI_DisplayControl.xml";
          break;

        case GUIMenu.Equalizer:
          str = @"\MiniDisplay_GUI_Equalizer.xml";
          break;

        case GUIMenu.Remote:
          str = @"\MiniDisplay_GUI_Remote.xml";
          break;

        case GUIMenu.BackLight:
          str = @"\MiniDisplay_GUI_BackLight.xml";
          break;

        case GUIMenu.KeyPad:
          str = @"\MiniDisplay_GUI_Keypad.xml";
          break;

        case GUIMenu.Fan:
          str = @"\MiniDisplay_GUI_Fan.xml";
          break;

        default:
          str = @"\MiniDisplay_GUI_Setup.xml";
          break;
      }
      return (GUIGraphicsContext.Skin + str);
    }

    public static MatrixGX.MOGX_Control LoadBackLightSettings()
    {
      MatrixGX.MOGX_Control control = new MatrixGX.MOGX_Control();
      if (Settings.Instance.Type.Equals("MatrixGX"))
      {
        MatrixGX.AdvancedSettings settings = MatrixGX.AdvancedSettings.Load();
        control.BackLightRed = settings.BacklightRED;
        control.BackLightGreen = settings.BacklightGREEN;
        control.BackLightBlue = settings.BacklightBLUE;
        control.InvertDisplay = settings.UseInvertedDisplay;
      }
      return control;
    }

    public static MiniDisplay.DisplayControl LoadDisplayControlSettings()
    {
      MiniDisplay.DisplayControl control = new MiniDisplay.DisplayControl();
      string type = Settings.Instance.Type;
      if (type.Equals("iMONLCDg"))
      {
        iMONLCDg.AdvancedSettings settings = iMONLCDg.AdvancedSettings.Load();
        control.BlankDisplayWithVideo = settings.BlankDisplayWithVideo;
        control.EnableDisplayAction = settings.EnableDisplayAction;
        control.DisplayActionTime = settings.EnableDisplayActionTime;
        control.BlankDisplayWhenIdle = settings.BlankDisplayWhenIdle;
        control.BlankIdleDelay = settings.BlankIdleTime;
        return control;
      }
      if (type.Equals("MatrixGX"))
      {
        MatrixGX.AdvancedSettings settings2 = MatrixGX.AdvancedSettings.Load();
        control.BlankDisplayWithVideo = settings2.BlankDisplayWithVideo;
        control.EnableDisplayAction = settings2.EnableDisplayAction;
        control.DisplayActionTime = settings2.EnableDisplayActionTime;
        control.BlankDisplayWhenIdle = settings2.BlankDisplayWhenIdle;
        control.BlankIdleDelay = settings2.BlankIdleTime;
        return control;
      }
      if (type.Equals("MD8800"))
      {
        MD8800.AdvancedSettings settings3 = MD8800.AdvancedSettings.Load();
        control.BlankDisplayWithVideo = settings3.BlankDisplayWithVideo;
        control.EnableDisplayAction = settings3.EnableDisplayAction;
        control.DisplayActionTime = settings3.EnableDisplayActionTime;
        control.BlankDisplayWhenIdle = settings3.BlankDisplayWhenIdle;
        control.BlankIdleDelay = settings3.BlankIdleTime;
        return control;
      }
      if (type.Equals("CFontz"))
      {
        CFontz.AdvancedSettings settings4 = CFontz.AdvancedSettings.Load();
        control.BlankDisplayWithVideo = settings4.BlankDisplayWithVideo;
        control.EnableDisplayAction = settings4.EnableDisplayAction;
        control.DisplayActionTime = settings4.EnableDisplayActionTime;
        control.BlankDisplayWhenIdle = settings4.BlankDisplayWhenIdle;
        control.BlankIdleDelay = settings4.BlankIdleTime;
        return control;
      }
      if (type.Equals("MatrixMX"))
      {
        MatrixMX.AdvancedSettings settings5 = MatrixMX.AdvancedSettings.Load();
        control.BlankDisplayWithVideo = settings5.BlankDisplayWithVideo;
        control.EnableDisplayAction = settings5.EnableDisplayAction;
        control.DisplayActionTime = settings5.EnableDisplayActionTime;
        control.BlankDisplayWhenIdle = settings5.BlankDisplayWhenIdle;
        control.BlankIdleDelay = settings5.BlankIdleTime;
        return control;
      }
      if (type.Equals("VLSYS_Mplay"))
      {
        VLSYS_Mplay.AdvancedSettings settings6 = VLSYS_Mplay.AdvancedSettings.Load();
        control.BlankDisplayWithVideo = settings6.BlankDisplayWithVideo;
        control.EnableDisplayAction = settings6.EnableDisplayAction;
        control.DisplayActionTime = settings6.EnableDisplayActionTime;
        control.BlankDisplayWhenIdle = settings6.BlankDisplayWhenIdle;
        control.BlankIdleDelay = settings6.BlankIdleTime;
      }
      return control;
    }

    public static MiniDisplay.DisplayOptions LoadDisplayOptionsSettings()
    {
      MiniDisplay.DisplayOptions options = new MiniDisplay.DisplayOptions();
      string type = Settings.Instance.Type;
      if (type.Equals("iMONLCDg"))
      {
        iMONLCDg.AdvancedSettings settings = iMONLCDg.AdvancedSettings.Load();
        options.VolumeDisplay = settings.VolumeDisplay;
        options.ProgressDisplay = settings.ProgressDisplay;
        options.DiskIcon = settings.DiskIcon;
        options.DiskMediaStatus = settings.DiskMediaStatus;
        options.DiskMonitor = settings.DeviceMonitor;
        options.UseCustomFont = settings.UseCustomFont;
        options.UseLargeIcons = settings.UseLargeIcons;
        options.UseCustomIcons = settings.UseCustomIcons;
        options.UseInvertedIcons = settings.UseInvertedIcons;
        return options;
      }
      if (type.Equals("MatrixGX"))
      {
        MatrixGX.AdvancedSettings settings2 = MatrixGX.AdvancedSettings.Load();
        options.VolumeDisplay = settings2.VolumeDisplay;
        options.ProgressDisplay = settings2.ProgressDisplay;
        options.DiskIcon = settings2.UseIcons;
      }
      return options;
    }

    public static MiniDisplay.EQControl LoadEqualizerSettings()
    {
      MiniDisplay.EQControl control = new MiniDisplay.EQControl();
      string type = Settings.Instance.Type;
      if (type.Equals("iMONLCDg"))
      {
        iMONLCDg.AdvancedSettings settings = iMONLCDg.AdvancedSettings.Load();
        control.UseEqDisplay = settings.EqDisplay;
        control.UseNormalEq = settings.NormalEQ;
        control.UseStereoEq = settings.StereoEQ;
        control.UseVUmeter = settings.VUmeter;
        control.UseVUmeter2 = settings.VUmeter2;
        control.SmoothEQ = settings.SmoothEQ;
        control.DelayEQ = settings.DelayEQ;
        control._DelayEQTime = settings.DelayEqTime;
        control.EQTitleDisplay = settings.EQTitleDisplay;
        control._EQTitleShowTime = settings.EQTitleShowTime;
        control._EQTitleDisplayTime = settings.EQTitleDisplayTime;
        return control;
      }
      if (type.Equals("CFontz"))
      {
        CFontz.AdvancedSettings settings2 = CFontz.AdvancedSettings.Load();
        control.UseEqDisplay = settings2.EqDisplay;
        control.UseNormalEq = settings2.NormalEQ;
        control.UseStereoEq = settings2.StereoEQ;
        control.UseVUmeter = settings2.VUmeter;
        control.UseVUmeter2 = settings2.VUmeter2;
        control.SmoothEQ = settings2.SmoothEQ;
        control.DelayEQ = settings2.DelayEQ;
        control._DelayEQTime = settings2.DelayEqTime;
        control.EQTitleDisplay = settings2.EQTitleDisplay;
        control._EQTitleShowTime = settings2.EQTitleShowTime;
        control._EQTitleDisplayTime = settings2.EQTitleDisplayTime;
        return control;
      }
      if (type.Equals("MatrixMX"))
      {
        MatrixMX.AdvancedSettings settings3 = MatrixMX.AdvancedSettings.Load();
        control.UseEqDisplay = settings3.EqDisplay;
        control.UseNormalEq = settings3.NormalEQ;
        control.UseStereoEq = settings3.StereoEQ;
        control.UseVUmeter = settings3.VUmeter;
        control.UseVUmeter2 = settings3.VUmeter2;
        control.SmoothEQ = settings3.SmoothEQ;
        control.DelayEQ = settings3.DelayEQ;
        control._DelayEQTime = settings3.DelayEqTime;
        control.EQTitleDisplay = settings3.EQTitleDisplay;
        control._EQTitleShowTime = settings3.EQTitleShowTime;
        control._EQTitleDisplayTime = settings3.EQTitleDisplayTime;
        return control;
      }
      if (type.Equals("MatrixGX"))
      {
        MatrixGX.AdvancedSettings settings4 = MatrixGX.AdvancedSettings.Load();
        control.UseEqDisplay = settings4.EqDisplay;
        control.UseNormalEq = settings4.NormalEQ;
        control.UseStereoEq = settings4.StereoEQ;
        control.UseVUmeter = settings4.VUmeter;
        control.UseVUmeter2 = settings4.VUmeter2;
        control.SmoothEQ = settings4.SmoothEQ;
        control.DelayEQ = settings4.DelayEQ;
        control._DelayEQTime = settings4.DelayEqTime;
        control.EQTitleDisplay = settings4.EQTitleDisplay;
        control._EQTitleShowTime = settings4.EQTitleShowTime;
        control._EQTitleDisplayTime = settings4.EQTitleDisplayTime;
        return control;
      }
      if (type.Equals("VLSYS_Mplay"))
      {
        VLSYS_Mplay.AdvancedSettings settings5 = VLSYS_Mplay.AdvancedSettings.Load();
        control.UseEqDisplay = settings5.EqDisplay;
        control.UseNormalEq = settings5.NormalEQ;
        control.UseStereoEq = settings5.StereoEQ;
        control.UseVUmeter = settings5.VUmeter;
        control.UseVUmeter2 = settings5.VUmeter2;
        control.SmoothEQ = settings5.SmoothEQ;
        control.DelayEQ = settings5.DelayEQ;
        control._DelayEQTime = settings5.DelayEqTime;
        control.EQTitleDisplay = settings5.EQTitleDisplay;
        control._EQTitleShowTime = settings5.EQTitleShowTime;
        control._EQTitleDisplayTime = settings5.EQTitleDisplayTime;
      }
      return control;
    }

    public static MatrixMX.KeyPadControl LoadKeyPadSettings()
    {
      MatrixMX.KeyPadControl control = new MatrixMX.KeyPadControl();
      string type = Settings.Instance.Type;
      if (type.Equals("MatrixMX"))
      {
        MatrixMX.AdvancedSettings settings = MatrixMX.AdvancedSettings.Load();
        control.EnableKeyPad = settings.EnableKeypad;
        control.EnableCustom = settings.UseCustomKeypadMap;
        return control;
      }
      if (type.Equals("CFontz"))
      {
        CFontz.AdvancedSettings settings2 = CFontz.AdvancedSettings.Load();
        control.EnableKeyPad = settings2.EnableKeypad;
        control.EnableCustom = settings2.UseCustomKeypadMap;
      }
      return control;
    }

    public static VLSYS_Mplay.RemoteControl LoadRemoteSettings()
    {
      VLSYS_Mplay.RemoteControl control = new VLSYS_Mplay.RemoteControl();
      if (Settings.Instance.Type.Equals("VLSYS_Mplay"))
      {
        VLSYS_Mplay.AdvancedSettings settings = VLSYS_Mplay.AdvancedSettings.Load();
        control.DisableRemote = settings.DisableRemote;
        control.DisableRepeat = settings.DisableRepeat;
        control.RepeatDelay = settings.RepeatDelay;
      }
      return control;
    }

    public static void SaveBackLightSettings(MatrixGX.MOGX_Control BLSettings)
    {
      if (Settings.Instance.Type.Equals("MatrixGX"))
      {
        MatrixGX.AdvancedSettings toSave = MatrixGX.AdvancedSettings.Load();
        toSave.BacklightRED = BLSettings.BackLightRed;
        toSave.BacklightGREEN = BLSettings.BackLightGreen;
        toSave.BacklightBLUE = BLSettings.BackLightBlue;
        toSave.UseInvertedDisplay = BLSettings.InvertDisplay;
        MatrixGX.AdvancedSettings.Instance = toSave;
        MatrixGX.AdvancedSettings.Save(toSave);
        MatrixGX.AdvancedSettings.NotifyDriver();
      }
    }

    public static void SaveDisplayControlSettings(MiniDisplay.DisplayControl DisplayControl)
    {
      string type = Settings.Instance.Type;
      if (type.Equals("iMONLCDg"))
      {
        iMONLCDg.AdvancedSettings toSave = iMONLCDg.AdvancedSettings.Load();
        toSave.BlankDisplayWithVideo = DisplayControl.BlankDisplayWithVideo;
        toSave.EnableDisplayAction = DisplayControl.EnableDisplayAction;
        toSave.EnableDisplayActionTime = DisplayControl.DisplayActionTime;
        toSave.BlankDisplayWhenIdle = DisplayControl.BlankDisplayWhenIdle;
        toSave.BlankIdleTime = DisplayControl.BlankIdleDelay;
        iMONLCDg.AdvancedSettings.Instance = toSave;
        iMONLCDg.AdvancedSettings.Save(toSave);
        iMONLCDg.AdvancedSettings.NotifyDriver();
      }
      else if (type.Equals("CFontz"))
      {
        CFontz.AdvancedSettings settings2 = CFontz.AdvancedSettings.Load();
        settings2.BlankDisplayWithVideo = DisplayControl.BlankDisplayWithVideo;
        settings2.EnableDisplayAction = DisplayControl.EnableDisplayAction;
        settings2.EnableDisplayActionTime = DisplayControl.DisplayActionTime;
        settings2.BlankDisplayWhenIdle = DisplayControl.BlankDisplayWhenIdle;
        settings2.BlankIdleTime = DisplayControl.BlankIdleDelay;
        CFontz.AdvancedSettings.Instance = settings2;
        CFontz.AdvancedSettings.Save(settings2);
        CFontz.AdvancedSettings.NotifyDriver();
      }
      else if (type.Equals("MD8800"))
      {
        MD8800.AdvancedSettings settings3 = MD8800.AdvancedSettings.Load();
        settings3.BlankDisplayWithVideo = DisplayControl.BlankDisplayWithVideo;
        settings3.EnableDisplayAction = DisplayControl.EnableDisplayAction;
        settings3.EnableDisplayActionTime = DisplayControl.DisplayActionTime;
        settings3.BlankDisplayWhenIdle = DisplayControl.BlankDisplayWhenIdle;
        settings3.BlankIdleTime = DisplayControl.BlankIdleDelay;
        MD8800.AdvancedSettings.Instance = settings3;
        MD8800.AdvancedSettings.Save(settings3);
        MD8800.AdvancedSettings.NotifyDriver();
      }
      else if (type.Equals("MatrixMX"))
      {
        MatrixMX.AdvancedSettings settings4 = MatrixMX.AdvancedSettings.Load();
        settings4.BlankDisplayWithVideo = DisplayControl.BlankDisplayWithVideo;
        settings4.EnableDisplayAction = DisplayControl.EnableDisplayAction;
        settings4.EnableDisplayActionTime = DisplayControl.DisplayActionTime;
        settings4.BlankDisplayWhenIdle = DisplayControl.BlankDisplayWhenIdle;
        settings4.BlankIdleTime = DisplayControl.BlankIdleDelay;
        MatrixMX.AdvancedSettings.Instance = settings4;
        MatrixMX.AdvancedSettings.Save(settings4);
        MatrixMX.AdvancedSettings.NotifyDriver();
      }
      else if (type.Equals("MatrixGX"))
      {
        MatrixGX.AdvancedSettings settings5 = MatrixGX.AdvancedSettings.Load();
        settings5.BlankDisplayWithVideo = DisplayControl.BlankDisplayWithVideo;
        settings5.EnableDisplayAction = DisplayControl.EnableDisplayAction;
        settings5.EnableDisplayActionTime = DisplayControl.DisplayActionTime;
        settings5.BlankDisplayWhenIdle = DisplayControl.BlankDisplayWhenIdle;
        settings5.BlankIdleTime = DisplayControl.BlankIdleDelay;
        MatrixGX.AdvancedSettings.Instance = settings5;
        MatrixGX.AdvancedSettings.Save(settings5);
        MatrixGX.AdvancedSettings.NotifyDriver();
      }
      else if (type.Equals("VLSYS_Mplay"))
      {
        VLSYS_Mplay.AdvancedSettings settings6 = VLSYS_Mplay.AdvancedSettings.Load();
        settings6.BlankDisplayWithVideo = DisplayControl.BlankDisplayWithVideo;
        settings6.EnableDisplayAction = DisplayControl.EnableDisplayAction;
        settings6.EnableDisplayActionTime = DisplayControl.DisplayActionTime;
        settings6.BlankDisplayWhenIdle = DisplayControl.BlankDisplayWhenIdle;
        settings6.BlankIdleTime = DisplayControl.BlankIdleDelay;
        VLSYS_Mplay.AdvancedSettings.Instance = settings6;
        VLSYS_Mplay.AdvancedSettings.Save(settings6);
        VLSYS_Mplay.AdvancedSettings.NotifyDriver();
      }
    }

    public static void SaveDisplayOptionsSettings(MiniDisplay.DisplayOptions DisplayOptions)
    {
      string type = Settings.Instance.Type;
      if (type.Equals("iMONLCDg"))
      {
        iMONLCDg.AdvancedSettings toSave = iMONLCDg.AdvancedSettings.Load();
        toSave.VolumeDisplay = DisplayOptions.VolumeDisplay;
        toSave.ProgressDisplay = DisplayOptions.ProgressDisplay;
        toSave.DiskIcon = DisplayOptions.DiskIcon;
        toSave.DiskMediaStatus = DisplayOptions.DiskMediaStatus;
        toSave.DeviceMonitor = DisplayOptions.DiskMonitor;
        toSave.UseCustomFont = DisplayOptions.UseCustomFont;
        toSave.UseLargeIcons = DisplayOptions.UseLargeIcons;
        toSave.UseCustomIcons = DisplayOptions.UseCustomIcons;
        toSave.UseInvertedIcons = DisplayOptions.UseInvertedIcons;
        iMONLCDg.AdvancedSettings.Instance = toSave;
        iMONLCDg.AdvancedSettings.Save(toSave);
        iMONLCDg.AdvancedSettings.NotifyDriver();
      }
      else if (type.Equals("MatrixGX"))
      {
        MatrixGX.AdvancedSettings settings2 = MatrixGX.AdvancedSettings.Load();
        settings2.VolumeDisplay = DisplayOptions.VolumeDisplay;
        settings2.ProgressDisplay = DisplayOptions.ProgressDisplay;
        settings2.UseIcons = DisplayOptions.DiskIcon;
        settings2.UseDiskIconForAllMedia = DisplayOptions.DiskMediaStatus;
        MatrixGX.AdvancedSettings.Instance = settings2;
        MatrixGX.AdvancedSettings.Save(settings2);
        MatrixGX.AdvancedSettings.NotifyDriver();
      }
      else if (!type.Equals("MatrixMX"))
      {
        type.Equals("VLSYS_Mplay");
      }
    }

    public static void SaveEqualizerSettings(MiniDisplay.EQControl EQSettings)
    {
      string type = Settings.Instance.Type;
      if (type.Equals("iMONLCDg"))
      {
        iMONLCDg.AdvancedSettings toSave = iMONLCDg.AdvancedSettings.Load();
        toSave.EqDisplay = EQSettings.UseEqDisplay;
        toSave.NormalEQ = EQSettings.UseNormalEq;
        toSave.StereoEQ = EQSettings.UseStereoEq;
        toSave.VUmeter = EQSettings.UseVUmeter;
        toSave.VUmeter2 = EQSettings.UseVUmeter2;
        toSave.SmoothEQ = EQSettings.SmoothEQ;
        toSave.DelayEQ = EQSettings.DelayEQ;
        toSave.DelayEqTime = EQSettings._DelayEQTime;
        toSave.EQTitleDisplay = EQSettings.EQTitleDisplay;
        toSave.EQTitleShowTime = EQSettings._EQTitleShowTime;
        toSave.EQTitleDisplayTime = EQSettings._EQTitleDisplayTime;
        toSave.RestrictEQ = true;
        toSave.EqRate = 30;
        iMONLCDg.AdvancedSettings.Instance = toSave;
        iMONLCDg.AdvancedSettings.Save(toSave);
        iMONLCDg.AdvancedSettings.NotifyDriver();
      }
      else if (type.Equals("CFontz"))
      {
        CFontz.AdvancedSettings settings2 = CFontz.AdvancedSettings.Load();
        settings2.EqDisplay = EQSettings.UseEqDisplay;
        settings2.NormalEQ = EQSettings.UseNormalEq;
        settings2.StereoEQ = EQSettings.UseStereoEq;
        settings2.VUmeter = EQSettings.UseVUmeter;
        settings2.VUmeter2 = EQSettings.UseVUmeter2;
        settings2.SmoothEQ = EQSettings.SmoothEQ;
        settings2.DelayEQ = EQSettings.DelayEQ;
        settings2.DelayEqTime = EQSettings._DelayEQTime;
        settings2.EQTitleDisplay = EQSettings.EQTitleDisplay;
        settings2.EQTitleShowTime = EQSettings._EQTitleShowTime;
        settings2.EQTitleDisplayTime = EQSettings._EQTitleDisplayTime;
        CFontz.AdvancedSettings.Instance = settings2;
        CFontz.AdvancedSettings.Save(settings2);
        CFontz.AdvancedSettings.NotifyDriver();
      }
      else if (type.Equals("MatrixMX"))
      {
        MatrixMX.AdvancedSettings settings3 = MatrixMX.AdvancedSettings.Load();
        settings3.EqDisplay = EQSettings.UseEqDisplay;
        settings3.NormalEQ = EQSettings.UseNormalEq;
        settings3.StereoEQ = EQSettings.UseStereoEq;
        settings3.VUmeter = EQSettings.UseVUmeter;
        settings3.VUmeter2 = EQSettings.UseVUmeter2;
        settings3.SmoothEQ = EQSettings.SmoothEQ;
        settings3.DelayEQ = EQSettings.DelayEQ;
        settings3.DelayEqTime = EQSettings._DelayEQTime;
        settings3.EQTitleDisplay = EQSettings.EQTitleDisplay;
        settings3.EQTitleShowTime = EQSettings._EQTitleShowTime;
        settings3.EQTitleDisplayTime = EQSettings._EQTitleDisplayTime;
        MatrixMX.AdvancedSettings.Instance = settings3;
        MatrixMX.AdvancedSettings.Save(settings3);
        MatrixMX.AdvancedSettings.NotifyDriver();
      }
      else if (type.Equals("MatrixGX"))
      {
        MatrixGX.AdvancedSettings settings4 = MatrixGX.AdvancedSettings.Load();
        settings4.EqDisplay = EQSettings.UseEqDisplay;
        settings4.NormalEQ = EQSettings.UseNormalEq;
        settings4.StereoEQ = EQSettings.UseStereoEq;
        settings4.VUmeter = EQSettings.UseVUmeter;
        settings4.VUmeter2 = EQSettings.UseVUmeter2;
        settings4.SmoothEQ = EQSettings.SmoothEQ;
        settings4.DelayEQ = EQSettings.DelayEQ;
        settings4.DelayEqTime = EQSettings._DelayEQTime;
        settings4.EQTitleDisplay = EQSettings.EQTitleDisplay;
        settings4.EQTitleShowTime = EQSettings._EQTitleShowTime;
        settings4.EQTitleDisplayTime = EQSettings._EQTitleDisplayTime;
        MatrixGX.AdvancedSettings.Instance = settings4;
        MatrixGX.AdvancedSettings.Save();
        MatrixGX.AdvancedSettings.NotifyDriver();
      }
      else if (type.Equals("VLSYS_Mplay"))
      {
        VLSYS_Mplay.AdvancedSettings settings5 = VLSYS_Mplay.AdvancedSettings.Load();
        settings5.EqDisplay = EQSettings.UseEqDisplay;
        settings5.NormalEQ = EQSettings.UseNormalEq;
        settings5.StereoEQ = EQSettings.UseStereoEq;
        settings5.VUmeter = EQSettings.UseVUmeter;
        settings5.VUmeter2 = EQSettings.UseVUmeter2;
        settings5.SmoothEQ = EQSettings.SmoothEQ;
        settings5.DelayEQ = EQSettings.DelayEQ;
        settings5.DelayEqTime = EQSettings._DelayEQTime;
        settings5.EQTitleDisplay = EQSettings.EQTitleDisplay;
        settings5.EQTitleShowTime = EQSettings._EQTitleShowTime;
        settings5.EQTitleDisplayTime = EQSettings._EQTitleDisplayTime;
        VLSYS_Mplay.AdvancedSettings.Instance = settings5;
        VLSYS_Mplay.AdvancedSettings.Save(settings5);
        VLSYS_Mplay.AdvancedSettings.NotifyDriver();
      }
    }

    public static void SaveKeyPadSettings(MatrixMX.KeyPadControl KeyPadOptions)
    {
      string type = Settings.Instance.Type;
      if (type.Equals("MatrixMX"))
      {
        MatrixMX.AdvancedSettings toSave = MatrixMX.AdvancedSettings.Load();
        toSave.EnableKeypad = KeyPadOptions.EnableKeyPad;
        toSave.UseCustomKeypadMap = KeyPadOptions.EnableCustom;
        MatrixMX.AdvancedSettings.Instance = toSave;
        MatrixMX.AdvancedSettings.Save(toSave);
        MatrixMX.AdvancedSettings.NotifyDriver();
      }
      else if (type.Equals("CFontz"))
      {
        CFontz.AdvancedSettings settings2 = CFontz.AdvancedSettings.Load();
        settings2.EnableKeypad = KeyPadOptions.EnableKeyPad;
        settings2.UseCustomKeypadMap = KeyPadOptions.EnableCustom;
        CFontz.AdvancedSettings.Instance = settings2;
        CFontz.AdvancedSettings.Save(settings2);
        CFontz.AdvancedSettings.NotifyDriver();
      }
    }

    public static void SaveRemoteSettings(VLSYS_Mplay.RemoteControl RemoteOptions)
    {
      if (Settings.Instance.Type.Equals("VLSYS_Mplay"))
      {
        VLSYS_Mplay.AdvancedSettings toSave = VLSYS_Mplay.AdvancedSettings.Load();
        toSave.DisableRemote = RemoteOptions.DisableRemote;
        toSave.DisableRepeat = RemoteOptions.DisableRepeat;
        toSave.RepeatDelay = RemoteOptions.RepeatDelay;
        VLSYS_Mplay.AdvancedSettings.Instance = toSave;
        VLSYS_Mplay.AdvancedSettings.Save(toSave);
        VLSYS_Mplay.AdvancedSettings.NotifyDriver();
      }
    }

    private enum Controls
    {
      TemplateButton = 0x3e9,
      TemplateFontLabel = 0x3eb,
      TemplateHoverImage = 0x3e8,
      TemplatePanel = 0x3ea
    }

    public enum GUIMenu
    {
      BackLight = 7,
      DisplayControl = 4,
      DisplayOptions = 3,
      Equalizer = 5,
      Fan = 9,
      KeyPad = 8,
      MainMenu = 0,
      Remote = 6
    }
  }
}

