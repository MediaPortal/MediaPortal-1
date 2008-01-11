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
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Xml;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Serialization;
using MediaPortal.Drawing;
using MediaPortal.Drawing.Layouts;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// Creates new GUIControls based on Skin Xml data, and handles
  /// the skin references.xml file which contains default properties for most controls.
  /// </summary>
  public class GUIControlFactory
  {

    #region Constructors

    private GUIControlFactory() // NON-CREATABLE
    {
    }

    static GUIControlFactory()
    {
    }

    #endregion Constructors

    #region Methods

    public static void LoadReferences(string referenceFile)
    {
      try
      {
        if (m_referenceNodesByControlType != null)
          return;
        Log.Info("  Loading references from {0}", referenceFile);
        m_referenceNodesByControlType = new Hashtable();
        _cachedStyleNodes = new Dictionary<string, XmlNode>();

        XmlDocument doc = new XmlDocument();

        doc.PreserveWhitespace = true;
        doc.Load(referenceFile);
        // Check the root element
        if (doc.DocumentElement == null || doc.DocumentElement.Name != "controls")
          return;

        ReadSkinSizeFromReferenceFile(doc);

        XmlNodeList list = doc.DocumentElement.SelectNodes("/controls/control");
        foreach (XmlNode controlNode in list)
        {
          if (GetControlType(controlNode) != null)
            m_referenceNodesByControlType[GetControlType(controlNode)] = controlNode;
        }

        // cache the styles
        foreach (XmlNode node in doc.DocumentElement.SelectNodes("/controls/style"))
        {
          XmlAttribute styleNameAttribute = node.Attributes["Name"];

          if (styleNameAttribute != null)
            _cachedStyleNodes[styleNameAttribute.Value] = node;
        }
      }
      catch (Exception ex)
      {
        Log.Info("exception loading references {0} err:{1} stack:{2}",
          referenceFile, ex.Message, ex.StackTrace);
      }
    }


    /// <summary>
    /// Deletes all reference nodes from memory (Use this to change skins in runtime).
    /// </summary>
    public static void ClearReferences()
    {
      m_referenceNodesByControlType = null;
      _cachedStyleNodes = null;
    }

    private static void ReadSkinSizeFromReferenceFile(XmlDocument doc)
    {
      GUIGraphicsContext.SkinSize = new System.Drawing.Size(720, 576);
      XmlNode nodeSkinWidth = doc.DocumentElement.SelectSingleNode("/controls/skin/width/text()");
      XmlNode nodeSkinHeight = doc.DocumentElement.SelectSingleNode("/controls/skin/height/text()");
      if (nodeSkinWidth != null && nodeSkinHeight != null)
      {
        try
        {
          int iWidth = Convert.ToInt16(nodeSkinWidth.Value);
          int iHeight = Convert.ToInt16(nodeSkinHeight.Value);
          Log.Info("  original skin size:{0}x{1}", iWidth, iHeight);
          GUIGraphicsContext.SkinSize = new System.Drawing.Size(iWidth, iHeight);
        }
        catch (FormatException) // Size values were invalid.
        {
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="guiControlType">The type of control you wish to update.</param>
    /// <returns>A hashtable which contains the MemberInfo objects for every
    /// updatable field, indexed by their corresponding Xml Element name. </returns>
    static Hashtable GetMembersToUpdate(Type guiControlType)
    {
      // Lazy Initializiation...
      if (m_reflectionCacheByControlType.ContainsKey(guiControlType))
        return (Hashtable)m_reflectionCacheByControlType[guiControlType];

      Hashtable membersTable = new Hashtable();

      MemberInfo[] allMembers = guiControlType.GetMembers(
        BindingFlags.Instance
        | BindingFlags.NonPublic
        | BindingFlags.FlattenHierarchy
        | BindingFlags.Public);

      foreach (MemberInfo member in allMembers)
      {
        if (member.IsDefined(typeof(XMLSkinElementAttribute), false))
        {
          XMLSkinElementAttribute atrb = (XMLSkinElementAttribute)
            member.GetCustomAttributes(typeof(XMLSkinElementAttribute), false)[0];

          membersTable[atrb.XmlElementName] = member;
        }
      }
      m_reflectionCacheByControlType[guiControlType] = membersTable;
      return membersTable;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="guiControlType">The type of control you wish to update.</param>
    /// <returns>A hashtable which contains the MemberInfo objects for every
    /// updatable field, indexed by their corresponding Xml Element name. </returns>
    static Hashtable GetAttributesToUpdate(Type guiControlType)
    {
      // Lazy Initializiation...
      if (m_reflectionCacheByControlTypeAttr.ContainsKey(guiControlType))
        return (Hashtable)m_reflectionCacheByControlTypeAttr[guiControlType];

      Hashtable membersTable = new Hashtable();

      MemberInfo[] allMembers = guiControlType.GetMembers(
        BindingFlags.Instance
        | BindingFlags.NonPublic
        | BindingFlags.FlattenHierarchy
        | BindingFlags.Public);

      foreach (MemberInfo member in allMembers)
      {
        if (member.IsDefined(typeof(XMLSkinAttribute), false))
        {
          XMLSkinAttribute atrb = (XMLSkinAttribute)
            member.GetCustomAttributes(typeof(XMLSkinAttribute), false)[0];

          membersTable[atrb] = member;
        }
      }
      m_reflectionCacheByControlTypeAttr[guiControlType] = membersTable;
      return membersTable;
    }

    private static object ConvertXmlStringToObject(string valueName, string valueText, Type type)
    {
      if (type == typeof(bool))
      {
        if (string.Compare(valueText, "off", true) == 0 ||
          string.Compare(valueText, "no", true) == 0 ||
          string.Compare(valueText, "disabled", true) == 0)
        {
          return false;
        }

        return true;
      }

      try
      {
        if (type == typeof(double))
        {
          double result = 0;

          if (double.TryParse(valueText, NumberStyles.Number, null, out result))
            return result;

          return 1;
        }

        if (type == typeof(int) || type == typeof(long))
        {
          if (string.Compare(valueName, "textcolor", true) == 0 ||
            string.Compare(valueName, "colorkey", true) == 0 ||
            string.Compare(valueName, "colordiffuse", true) == 0)
          {
            if (valueText.Length > 0)
            {
              bool isNamedColor = false;

              foreach (char ch in valueText)
              {
                if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f')
                  continue;

                isNamedColor = true;
                break;
              }

              if (isNamedColor)
              {
                int index = valueText.IndexOf(':');

                if (index != -1)
                {
                  Color color = ColorTranslator.FromHtml(valueText.Substring(0, index));
                  int alpha = 255;

                  if (index < valueText.Length)
                  {
                    if (valueText[index + 1] == '#')
                      alpha = int.Parse(valueText.Substring(index + 2), NumberStyles.HexNumber);
                    else
                      alpha = int.Parse(valueText.Substring(index + 1));
                  }

                  return Color.FromArgb(alpha, color).ToArgb();
                }

                return Color.FromName(valueText).ToArgb();
              }

              try
              {
                Color color = ColorTranslator.FromHtml('#' + valueText);

                return color.ToArgb();
              }
              catch
              {
                Log.Info("GUIControlFactory.ConvertXmlStringToObject: Invalid color format '#{0}' reverting to White", valueText);

                return Color.White.ToArgb();
              }
            }
          }
        }

        if (type == typeof(int))
        {
          if (valueText.CompareTo("-") == 0) return 0;
          if (valueText.CompareTo("") == 0) return 0;
          int res;
          if (int.TryParse(valueText, out res))
            return res;
          if (int.TryParse(valueText, NumberStyles.HexNumber, null, out res))
            return res;
        }
        if (type == typeof(long))
        {
          if (valueText.CompareTo("-") == 0) return 0;
          if (valueText.CompareTo("") == 0) return 0;
          long res;
          if (System.Int64.TryParse(valueText, NumberStyles.HexNumber, null, out res))
            return res;
          return 0;
        }
      }
      catch (Exception)
      {
        return 0;
      }

      if (type == typeof(ILayout))
        return ParseLayout(valueText);

      // much of the above could be changed to use the following, needs time for thorough testing though
      TypeConverter converter = TypeDescriptor.GetConverter(type);

      if (converter.CanConvertFrom(typeof(string)))
        return converter.ConvertFromString(valueText);

      return null;
    }

    public static GUIControl Create(int dwParentId, XmlNode pControlNode, IDictionary defines)
    {
      Type typeOfControlToCreate = GetControlType(pControlNode);
      if (typeOfControlToCreate == null)
        return null;

      object[] ctorParams = { dwParentId };
      GUIControl control = (GUIControl)
        Activator.CreateInstance(typeOfControlToCreate, ctorParams);

      try
      {
        //				if(control is ISupportInitialize)
        ((ISupportInitialize)control).BeginInit();

        XmlNode referenceNode =
          (XmlNode)m_referenceNodesByControlType[typeOfControlToCreate];

        if (referenceNode != null)
          UpdateControlWithXmlData(control, typeOfControlToCreate, referenceNode, defines);

        XmlAttribute styleAttribute = pControlNode.Attributes["Style"];

        if (styleAttribute != null)
        {
          XmlNode styleNode = _cachedStyleNodes[styleAttribute.Value];

          if (styleNode != null)
          {
            Log.Info("Styling");
            UpdateControlWithXmlData(control, typeOfControlToCreate, styleNode, defines);
          }
        }

        UpdateControlWithXmlData(control, typeOfControlToCreate, pControlNode, defines);

        control.ScaleToScreenResolution();
        AddSubitemsToControl(pControlNode, control);
        control.FinalizeConstruction();

        if (control is IAddChild)
        {
          foreach (XmlNode subControlNode in pControlNode.SelectNodes("control"))
            ((IAddChild)control).AddChild(Create(dwParentId, subControlNode, defines));
        }

        if (typeOfControlToCreate == typeof(GUIFacadeControl))
        {
          GUIFacadeControl facade = (GUIFacadeControl)control;
          XmlNodeList nodeList = pControlNode.SelectNodes("control");
          foreach (XmlNode subControlNode in nodeList)
          {
            GUIControl subControl = Create(dwParentId, subControlNode, defines);

            if (subControl is GUIPlayListItemListControl)
            {
              GUIPlayListItemListControl list = subControl as GUIPlayListItemListControl;
              facade.PlayListView = list;
            }

            else if (subControl is GUIListControl)
            {
              GUIListControl list = subControl as GUIListControl;
              if (list.SubType == "album")
                facade.AlbumListView = list;
              else
                facade.ListView = list;
            }
            if (subControl is GUIThumbnailPanel)
              facade.ThumbnailView = subControl as GUIThumbnailPanel;
            if (subControl is GUIFilmstripControl)
              facade.FilmstripView = subControl as GUIFilmstripControl;
            //UpdateControlWithXmlData(subControl, subControl.GetType(), subControlNode, defines);
          }
        }

        //				if(control is ISupportInitialize)
        ((ISupportInitialize)control).EndInit();
      }
      catch (Exception e)
      {
        Log.Info("GUIControlFactory.Create: {0}\r\n\r\n{1}\r\n\r\n", e.Message, e.StackTrace);
        Log.Info("Parent: {0} Id: {1}", dwParentId, control.GetID);
      }

      return control;
    }

    private static bool GetConditionalVisibility(XmlNode element, GUIControl control, ref int condition, ref bool allowHiddenFocus)
    {
      condition = GUIInfoManager.TranslateString(element.InnerText);
      return (condition != 0);
    }
    private static void UpdateControlWithXmlData(GUIControl control, Type controlType, XmlNode pControlNode, IDictionary defines)
    {
      List<int> vecInfo = new List<int>();
      Hashtable attributesThatCanBeUpdates = GetAttributesToUpdate(controlType);
      if (attributesThatCanBeUpdates != null)
      {
        IDictionaryEnumerator en = attributesThatCanBeUpdates.GetEnumerator();
        while (en.MoveNext())
        {
          XMLSkinAttribute xmlAttr = (XMLSkinAttribute)en.Key;
          MemberInfo correspondingMemberAttr = en.Value as MemberInfo;
          XmlNode elementNode = pControlNode.SelectSingleNode(xmlAttr.XmlElementName);
          if (elementNode != null)
          {
            XmlNode attribNode = elementNode.Attributes.GetNamedItem(xmlAttr.XmlAttributeName);
            if (attribNode != null)
            {
              if (correspondingMemberAttr != null)
              {

                string text = attribNode.Value;

                if (text.Length > 0 && text[0] == '#' && defines.Contains(text))
                  text = (string)defines[text];

                object newValue = null;

                if (correspondingMemberAttr.MemberType == MemberTypes.Field)
                  newValue = ConvertXmlStringToObject(xmlAttr.XmlAttributeName, text, ((FieldInfo)correspondingMemberAttr).FieldType);
                else if (correspondingMemberAttr.MemberType == MemberTypes.Property)
                  newValue = ConvertXmlStringToObject(xmlAttr.XmlAttributeName, text, ((PropertyInfo)correspondingMemberAttr).PropertyType);

                try
                {
                  if (correspondingMemberAttr.MemberType == MemberTypes.Field)
                    ((FieldInfo)correspondingMemberAttr).SetValue(control, newValue);
                  else if (correspondingMemberAttr.MemberType == MemberTypes.Property)
                    ((PropertyInfo)correspondingMemberAttr).SetValue(control, newValue, null);
                }
                catch (Exception e)
                {
                  Log.Info("Couldn't place {0}, which is {1} in {2}. Exception:{3}",
                    newValue, newValue.GetType(), correspondingMemberAttr, e);
                }
              }
              else
              {
                if (char.IsUpper(xmlAttr.XmlAttributeName[0]))
                {
                  PropertyInfo propertyInfo;

                  if (xmlAttr.XmlAttributeName.IndexOf('.') != -1)
                  {
                    propertyInfo = controlType.GetProperty(xmlAttr.XmlAttributeName.Split('.')[1]);
                  }
                  else
                  {
                    propertyInfo = controlType.GetProperty(xmlAttr.XmlAttributeName);
                  }

                  if (propertyInfo == null)
                  {
                    Log.Info("GUIControlFactory.UpdateControlWithXmlData: '{0}' does not contain a definition for '{1}'", controlType, xmlAttr.XmlAttributeName);
                    return;
                  }
                }
              }
            }
          }
        }
      }
      Hashtable membersThatCanBeUpdated = GetMembersToUpdate(controlType);
      List<VisualEffect> animations = new List<VisualEffect>();
      List<VisualEffect> thumbAnimations = new List<VisualEffect>();
      XmlNodeList childNodes = pControlNode.ChildNodes;
      foreach (XmlNode element in childNodes)
      {
        if (element.Name == "visible")
        {
          if (element.InnerText != null)
          {
            if (element.InnerText != "yes" && element.InnerText != "no")
            {
              if (element.InnerText.Length != 0)
              {
                int iVisibleCondition = 0;
                bool allowHiddenFocus = false;
                GetConditionalVisibility(element, control, ref iVisibleCondition, ref allowHiddenFocus);
                control.SetVisibleCondition(iVisibleCondition, allowHiddenFocus);
                continue;
              }
            }
          }
        }
        if (element.Name == "animation")
        {
          VisualEffect effect = new VisualEffect();
          if (effect.Create(element))
          {
            animations.Add(effect);
            continue;
          }
        }
        if (element.Name == "thumbAnimation")
        {
          VisualEffect effect = new VisualEffect();
          if (effect.Create(element))
          {
            thumbAnimations.Add(effect);
            continue;
          }
        }
        if (element.Name == "info")
        {
          List<string> infoList = new List<string>();
          if (GetMultipleString(element, "info", ref infoList))
          {
            vecInfo.Clear();
            for (int i = 0; i < infoList.Count; i++)
            {
              int infoId = GUIInfoManager.TranslateString(infoList[i]);
              if (infoId!=0)
                vecInfo.Add(infoId);
            }
          }
          control.Info=vecInfo;
        }
        MemberInfo correspondingMember = membersThatCanBeUpdated[element.Name] as MemberInfo;

        if (correspondingMember != null)
        {
          string text = element.InnerText;

          if (text.Length > 0 && text[0] == '#' && defines.Contains(text))
            text = (string)defines[text];

          object newValue = null;

          if (correspondingMember.MemberType == MemberTypes.Field)
            newValue = ConvertXmlStringToObject(element.Name, text, ((FieldInfo)correspondingMember).FieldType);
          else if (correspondingMember.MemberType == MemberTypes.Property)
            newValue = ConvertXmlStringToObject(element.Name, text, ((PropertyInfo)correspondingMember).PropertyType);

          try
          {
            if (correspondingMember.MemberType == MemberTypes.Field)
              ((FieldInfo)correspondingMember).SetValue(control, newValue);
            else if (correspondingMember.MemberType == MemberTypes.Property)
              ((PropertyInfo)correspondingMember).SetValue(control, newValue, null);
          }
          catch (Exception e)
          {
            Log.Info("Couldn't place {0}, which is {1} in {2}. Exception:{3}",
              newValue, newValue.GetType(), correspondingMember, e);
          }
        }
        else
        {
          if (char.IsUpper(element.Name[0]))
          {
            PropertyInfo propertyInfo;

            if (element.Name.IndexOf('.') != -1)
            {
              propertyInfo = controlType.GetProperty(element.Name.Split('.')[1]);
            }
            else
            {
              propertyInfo = controlType.GetProperty(element.Name);
            }

            if (propertyInfo == null)
            {
              Log.Info("GUIControlFactory.UpdateControlWithXmlData: '{0}' does not contain a definition for '{1}'", controlType, element.Name);
              return;
            }

            string xml = element.OuterXml;

            if (xml.IndexOf("Button.") != -1)
              xml = xml.Replace("Button.", "GUIControl.");
            else if (xml.IndexOf("Window.") != -1)
              xml = xml.Replace("Window.", "GUIWindow.");

            XamlParser.LoadXml(xml, XmlNodeType.Element, control);
          }
        }
      }
      if (animations.Count > 0)
        control.SetAnimations(animations);
      if (thumbAnimations.Count > 0)
        control.SetThumbAnimations(thumbAnimations);
    }

    private static void AddSubitemsToControl(XmlNode subItemsNode, GUIControl control)
    {
      XmlNodeList subNodes = subItemsNode.SelectNodes("subitems/subitem/text()");
      foreach (XmlNode subNode in subNodes)
      {
        string strSubItem = subNode.Value;
        if (Char.IsDigit(strSubItem[0]))
        {
          GUILocalizeStrings.LocalizeLabel(ref strSubItem);
        }
        control.AddSubItem(strSubItem);
      }
    }

    private static Type GetControlType(XmlNode controlNode)
    {
      XmlNode typeText = controlNode.SelectSingleNode("type/text()");
      if (typeText == null || typeText.Value == "")
        return null;
      string xmlTypeName = typeText.Value;
      switch (xmlTypeName)
      {
        case ("image"):
          return typeof(GUIImage);
        case ("imagelist"):
          return typeof(GUIImageList);
        case ("slider"):
          return typeof(GUISliderControl);
        case ("fadelabel"):
          return typeof(GUIFadeLabel);
        case ("label"):
          return typeof(GUILabelControl);
        case ("button"):
          return typeof(GUIButtonControl);
        case ("updownbutton"):
          return typeof(GUIUpDownButton);
        case ("button3part"):
          return typeof(GUIButton3PartControl);
        case ("statusbar"):
          return typeof(GUIStatusbarControl);
        case ("progress"):
          return typeof(GUIProgressControl);
        case ("tvprogress"):
          return typeof(GUITVProgressControl);
        case ("hscrollbar"):
          return typeof(GUIHorizontalScrollbar);
        case ("vscrollbar"):
          return typeof(GUIVerticalScrollbar);
        case ("textbox"):
          return typeof(GUITextControl);
        case ("textboxscrollup"):
          return typeof(GUITextScrollUpControl);
        case ("thumbnailpanel"):
          return typeof(GUIThumbnailPanel);
        case ("spincontrol"):
          return typeof(GUISpinControl);
        case ("checkmark"):
          return typeof(GUICheckMarkControl);
        case ("selectbutton"):
          return typeof(GUISelectButtonControl);
        case ("listcontrol"):
          return typeof(GUIListControl);
        case ("updownlistcontrol"):
          return typeof(GUIUpDownListControl);
        case ("checklistcontrol"):
          return typeof(GUICheckListControl);
        case ("togglebutton"):
          return typeof(GUIToggleButtonControl);
        case ("group"):
          return typeof(GUIGroup);
        case ("videowindow"):
          return typeof(GUIVideoControl);
        case ("facadeview"):
          return typeof(GUIFacadeControl);
        case ("filmstrip"):
          return typeof(GUIFilmstripControl);
        case ("smsinput"):
          return typeof(GUISMSInputControl);
        case ("sortbutton"):
          return typeof(GUISortButtonControl);
        case ("volumebar"):
          return typeof(GUIVolumeBar);
        case ("multiimage"):
          return typeof(GUIMultiImage);
        case ("animation"):
          return typeof(GUIAnimation);
        case ("playlistbutton"):
          return typeof(GUIPlayListButtonControl);
        case ("playlistcontrol"):
          return typeof(GUIPlayListItemListControl);
        case ("gridcontrol"):
          return typeof(GUIGridControl);
        case ("actiongroup"):
          return typeof(GUIActionGroup);
        case ("menu"):
          return typeof(GUIMenuControl);
        default:
          Type t = (Type)m_hashCustomControls[xmlTypeName];

          if (t == null)
          {
            Log.Info("ERROR: unknown control:<{0}>", xmlTypeName);
            return null;
          }

          return t;
      }
    }

    public static void RegisterControl(string strName, Type t)
    {
      m_hashCustomControls[strName] = t;
    }

    static object ParseLayout(string valueText)
    {
      int openingBracket = valueText.IndexOf('(');
      int[] valueParameters = null;
      string layoutClass = null;

      if (openingBracket != -1)
      {
        layoutClass = valueText.Substring(0, openingBracket);
        valueParameters = ParseParameters(valueText.Substring(openingBracket).Trim());
      }
      else
      {
        layoutClass = valueText;
        valueParameters = new int[0];
      }

      if (string.Compare(layoutClass, "GridLayout", true) == 0)
      {
        if (valueParameters.Length >= 5)
          return new GridLayout(valueParameters[0], valueParameters[1], valueParameters[2], valueParameters[3], (Orientation)valueParameters[4]);

        if (valueParameters.Length >= 4)
          return new GridLayout(valueParameters[0], valueParameters[1], valueParameters[2], valueParameters[3]);

        if (valueParameters.Length >= 2)
          return new GridLayout(valueParameters[0], valueParameters[1]);

        if (valueParameters.Length >= 1)
          return new GridLayout(valueParameters[0]);

        if (valueParameters.Length == 0)
          return new GridLayout();

        return null;
      }

      if (string.Compare(layoutClass, "StackLayout", true) == 0)
      {
        if (valueParameters.Length >= 2)
          return new StackLayout(valueParameters[0], (Orientation)valueParameters[1]);

        if (valueParameters.Length >= 1)
          return new StackLayout(valueParameters[0]);

        if (valueParameters.Length == 0)
          return new StackLayout();

        return null;
      }

      if (string.Compare(layoutClass, "RingLayout", true) == 0)
      {
        if (valueParameters.Length >= 2)
          return new RingLayout(valueParameters[0], valueParameters[1]);

        if (valueParameters.Length >= 1)
          return new RingLayout(valueParameters[0]);

        if (valueParameters.Length == 0)
          return new RingLayout();

        return null;
      }

      return null;
    }

    static int[] ParseParameters(string valueText)
    {
      if (!(valueText.StartsWith("(") && valueText.EndsWith(")")))
        return new int[0];

      valueText = valueText.Substring(1, valueText.Length - 2);

      try
      {
        ArrayList valuesTemp = new ArrayList();

        foreach (string token in valueText.Split(new char[] { ',', ' ' }))
        {
          if (token == string.Empty)
            continue;

          if (string.Compare(token, "Horizontal") == 0)
          {
            valuesTemp.Add((int)Orientation.Horizontal);
          }
          else if (string.Compare(token, "Vertical") == 0)
          {
            valuesTemp.Add((int)Orientation.Vertical);
          }
          else
          {
            valuesTemp.Add(int.Parse(token));
          }
        }

        int[] values = new int[valuesTemp.Count];

        Array.Copy(valuesTemp.ToArray(), values, values.Length);

        return values;
      }
      catch { }

      return new int[0];
    }
    static bool GetMultipleString(XmlNode rootNode, string tag, ref List<string> infoList)
    {
      infoList.Clear();
      if (rootNode.HasChildNodes)
      {
        foreach (XmlNode subNode in rootNode.ChildNodes)
        {
          infoList.Add(subNode.Value);
        }
      }
      else
      {
        infoList.Add(rootNode.Value);
      }
      return (infoList.Count>0);
    }
    #endregion Methods

    #region Fields

    /// <summary>
    /// Contains all of the reference nodes, indexed by control Type.
    /// </summary>
    static Hashtable m_referenceNodesByControlType = null;

    static Hashtable m_hashCustomControls = new Hashtable();

    /// <summary>
    /// A hashtable which contains the reflection results for every control.
    /// </summary>
    static Hashtable m_reflectionCacheByControlType = new Hashtable(20);
    static Hashtable m_reflectionCacheByControlTypeAttr = new Hashtable(20);

    // same as above but for caching style nodes
    static Dictionary<string, XmlNode> _cachedStyleNodes;

    #endregion Fields
  }
}