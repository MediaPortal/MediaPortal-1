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
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Mpe.Controls.Properties;

namespace Mpe.Controls
{
  public class MpeScreen : MpeContainer
  {
    #region Variables

    private bool allowOverlay;
    private bool autohideTopbar;
    private int defaultControl;
    private MpeScreenType screenType;
    private MpeScreenSize screenSize;

    private static int DialogGroupId = 123456;

    #endregion

    #region Constructors

    public MpeScreen() : base()
    {
      MpeLog.Debug("MpeScreen()");
      Type = MpeControlType.Screen;
      MpeScreen = this;
      AllowDrop = true;
      backImage.Description = "Default Image - Window Background";
      backImage.Id = 1;
      backImage.MpeScreen = this;
      backImage.Embedded = true;
      screenSize = MpeScreenSize.PAL;
      defaultControl = 0;
      allowOverlay = true;
      screenType = MpeScreenType.Window;
      controlLock.Location = true;
      controlLock.Size = true;
    }

    public MpeScreen(MpeScreen window) : base(window)
    {
      MpeLog.Debug("MpeScreen(window)");
      AllowDrop = true;
      allowOverlay = window.allowOverlay;
      defaultControl = window.defaultControl;
      screenSize = window.screenSize;
    }

    #endregion

    #region Properties

    [Browsable(false)]
    protected MpeGroup DialogGroup
    {
      get
      {
        for (int i = 0; i < Controls.Count; i++)
        {
          if (Controls[i] is MpeGroup)
          {
            MpeGroup result = (MpeGroup) Controls[i];
            if (result.Id == DialogGroupId)
            {
              return result;
            }
          }
        }
        return null;
      }
    }

    [ReadOnly(true)]
    public override MpeLayoutStyle LayoutStyle
    {
      get { return base.LayoutStyle; }
      set { base.LayoutStyle = value; }
    }

    [Browsable(false)]
    public new Point Location
    {
      get { return base.Location; }
      set { base.Location = value; }
    }

    [Category("Layout")]
    [TypeConverterAttribute(typeof(MpeScreenSizeConverter))]
    public MpeScreenSize ScreenSize
    {
      get { return screenSize; }
      set
      {
        if (value != null)
        {
          screenSize = value;
          base.Size = value.Size;
          if (backImage != null)
          {
            backImage.Size = value.Size;
          }
          Modified = true;
          Invalidate(false);
          FirePropertyValueChanged("ScreenSize");
        }
      }
    }

    [Category("Control")]
    public MpeScreenType ScreenType
    {
      get { return screenType; }
    }

    [Category("Control")]
    public bool AllowOverlay
    {
      get { return allowOverlay; }
      set
      {
        if (allowOverlay != value)
        {
          allowOverlay = value;
          Modified = true;
          FirePropertyValueChanged("AllowOverlay");
        }
      }
    }
    
    [Category("Control")]
    [Description("Automatically hide the top bar")]
    public bool AutohideTopbar
    {
      get { return autohideTopbar; }
      set
      {
        if (autohideTopbar != value)
        {
          autohideTopbar = value;
          Modified = true;
          FirePropertyValueChanged("AutohideTopbar");
        }
      }
    }

    [Category("Control")]
    public int DefaultControl
    {
      get { return defaultControl; }
      set
      {
        if (defaultControl != value)
        {
          defaultControl = value;
          Modified = true;
          FirePropertyValueChanged("DefaultControl");
        }
      }
    }

    #endregion

    #region Properties - Hidden

    [Browsable(false)]
    public override MpeControlType Type
    {
      get { return base.Type; }
      set { base.Type = value; }
    }

    [Browsable(false)]
    public new Size Size
    {
      get { return base.Size; }
      set { base.Size = value; }
    }

    [Browsable(false)]
    public override Color DiffuseColor
    {
      get { return base.DiffuseColor; }
      set { base.DiffuseColor = value; }
    }

    [Browsable(false)]
    public override MpeControlAlignment Alignment
    {
      get { return base.Alignment; }
      set { base.Alignment = value; }
    }

    [Browsable(false)]
    public override MpeControlPadding Padding
    {
      get { return base.Padding; }
      set { base.Padding = value; }
    }

    [Browsable(false)]
    public override int Spacing
    {
      get { return base.Spacing; }
      set { base.Spacing = value; }
    }

    [Browsable(false)]
    public override bool Spring
    {
      get { return base.Spring; }
      set { base.Spring = value; }
    }

    [ReadOnly(true)]
    public override bool AutoSize
    {
      get { return base.AutoSize; }
      set { base.AutoSize = value; }
    }

    [Browsable(false)]
    public override MpeControlLock Locked
    {
      get { return base.Locked; }
      set { base.Locked = value; }
    }

    [Browsable(false)]
    public override string Visible
    {
      get { return base.Visible; }
      set { base.Visible = value; }
    }

    #endregion

    #region Methods

    public override MpeControl Copy()
    {
      return new MpeScreen(this);
    }

    protected override void PrepareControl()
    {
      if (ScreenSize != null)
      {
        Size = ScreenSize.Size;
      }
    }

    public override void Load(XPathNodeIterator iterator, MpeParser parser)
    {
      MpeLog.Debug("MpeScreen.Load()");
      this.parser = parser;
      XPathNodeIterator i = null;
      if (iterator == null)
      {
        throw new MpeParserException("The given iterator is invalid.");
      }
      if (iterator.Current.Name == "controls")
      {
        Width = parser.GetInt(iterator, "skin/width", Width);
        Height = parser.GetInt(iterator, "skin/height", Height);
        ScreenSize = MpeScreenSize.FromResolution(Width, Height);
        i = iterator.Current.Select("control[type='image']");
        if (i.MoveNext())
        {
          backImage.Load(i, parser);
        }
        Id = 0;
      }
      else if (iterator.Current.Name == "window")
      {
        string stype = parser.GetString(iterator, "type", "");
        if (stype == MpeScreenType.Dialog.ToString().ToLower())
        {
          screenType = MpeScreenType.Dialog;
        }
        else if (stype == MpeScreenType.OnScreenDisplay.ToString().ToLower())
        {
          screenType = MpeScreenType.OnScreenDisplay;
        }
        else
        {
          screenType = MpeScreenType.Window;
        }

        Id = parser.GetInt(iterator, "id", Id);
        AllowOverlay = parser.GetBoolean(iterator, "allowoverlay", AllowOverlay);
        AutohideTopbar = parser.GetBoolean(iterator, "autohidetopbar", AutohideTopbar);
        DefaultControl = parser.GetInt(iterator, "defaultcontrol", DefaultControl);

        if (screenType == MpeScreenType.Dialog)
        {
          // Initialize the default screen
          MpeScreen defaultScreen = (MpeScreen) parser.GetControl(MpeControlType.Screen);
          if (defaultScreen == null)
          {
            throw new MpeParserException("Reference screen was never initialized and loaded");
          }
          TextureBack = defaultScreen.TextureBack;
          Size = defaultScreen.Size;
          AllowDrop = false;
          // First create the dialog group

          MpeGroup dialog = (MpeGroup) parser.CreateControl(MpeControlType.Group);
          dialog.Id = DialogGroupId;
          dialog.LayoutStyle = MpeLayoutStyle.Grid;
          dialog.Parser = Parser;
          Controls.Add(dialog);

          // Add all the controls
          i = iterator.Current.Select("controls/control");
          bool first = true;
          while (i.MoveNext())
          {
            string s = parser.GetString(i, "type", "");
            if (first && s == MpeControlType.Image.ToString())
            {
              first = false;
              dialog.TextureBackImage.Load(i, parser);
              dialog.Size = dialog.TextureBackImage.Size;
              dialog.Location = dialog.TextureBackImage.Location;
            }
            else
            {
              XPathNodeIterator typeIterator = i.Current.SelectChildren("type", "");
              if (typeIterator.MoveNext())
              {
                MpeControlType type = MpeControlType.Create(typeIterator.Current.Value);
                MpeControl c = parser.CreateControl(type);
                dialog.Controls.Add(c);
                c.Load(i, parser);
                c.BringToFront();
              }
            }
          }
        }
        else
        {
          AllowDrop = true;
          i = iterator.Current.Select("controls/control");
          bool first = true;
          while (i.MoveNext())
          {
            string s = parser.GetString(i, "type", "");
            if (first && s == MpeControlType.Image.ToString())
            {
              backImage.Load(i, parser);
            }
            else
            {
              XPathNodeIterator typeIterator = i.Current.SelectChildren("type", "");
              if (typeIterator.MoveNext())
              {
                MpeControlType type = MpeControlType.Create(typeIterator.Current.Value);
                MpeControl c = parser.CreateControl(type);
                Controls.Add(c);
                c.Load(i, parser);
                c.BringToFront();
              }
            }
            first = false;
          }
        }
      }
      Modified = false;
    }

    public override void Save(XmlDocument doc, XmlNode node, MpeParser parser, MpeControl reference)
    {
      if (reference == null)
      {
        // Update reference.xml file
        if (node.Name != "controls")
        {
          throw new MpeParserException("Invalid root node <" + node.Name + "> provided.");
        }
        // Update the skin node
        XmlNode skin = node.SelectSingleNode("skin");
        if (skin == null)
        {
          throw new MpeParserException(
            "Invalid reference.xml file. The <skin> element must be the first child in the document.");
        }
        skin.RemoveAll();
        parser.SetValue(doc, skin, "width", Width.ToString());
        parser.SetValue(doc, skin, "height", Height.ToString());
        // Update the image control node that defines the window background
        XmlNode image = skin.NextSibling;
        if (image == null || image.Name.Equals("control") == false)
        {
          throw new MpeParserException(
            "Invalid reference.xml file. A <control> element of type image must follow the <skin> element.");
        }
        XmlNode test = image.SelectSingleNode("type");
        if (test == null || test.InnerXml.Equals("image") == false)
        {
          throw new MpeParserException(
            "Invalid reference.xml file. A <control> element of type image must follow the <skin> element.");
        }
        image.RemoveAll();
        backImage.Save(doc, image, parser, null);
      }
      else
      {
        // Update screen.xml file
        if (node == null || node.Name.Equals("window") == false)
        {
          throw new MpeParserException("Invalid root node <" + node.Name + "> provided. Looking for a <window> element.");
        }
        node.RemoveAll();

        if (screenType != MpeScreenType.Window)
        {
          parser.SetValue(doc, node, "type", screenType.ToString().ToLower());
        }

        parser.SetValue(doc, node, "id", Id.ToString());
        parser.SetValue(doc, node, "defaultcontrol", DefaultControl.ToString());
        parser.SetValue(doc, node, "allowoverlay", AllowOverlay ? "yes" : "no");
        parser.SetValue(doc, node, "autohidetopbar", AutohideTopbar ? "yes" : "no");

        XmlElement controls = doc.CreateElement("controls");
        node.AppendChild(controls);

        if (ScreenType == MpeScreenType.Dialog)
        {
          MpeGroup dg = DialogGroup;

          //if (dg.TextureBack != null) {
          XmlElement image = doc.CreateElement("control");
          dg.TextureBackImage.Save(doc, image, parser, parser.GetControl(MpeControlType.Image));
          controls.AppendChild(image);
          //}

          for (int i = dg.Controls.Count - 1; i >= 0; i--)
          {
            if (dg.Controls[i] is MpeControl)
            {
              MpeControl control = (MpeControl) dg.Controls[i];
              try
              {
                XmlElement element = doc.CreateElement("control");
                MpeControl referenceControl = parser.GetControl(control.Type);
                control.Save(doc, element, parser, referenceControl);
                controls.AppendChild(element);
              }
              catch (Exception e)
              {
                MpeLog.Debug(e);
                MpeLog.Error(e);
                throw new MpeParserException(e.Message);
              }
            }
          }
        }
        else
        {
          if (TextureBack != null)
          {
            XmlElement image = doc.CreateElement("control");
            backImage.Save(doc, image, parser, parser.GetControl(MpeControlType.Image));
            controls.AppendChild(image);
          }

          for (int i = Controls.Count - 1; i >= 0; i--)
          {
            if (Controls[i] is MpeControl)
            {
              MpeControl control = (MpeControl) Controls[i];
              try
              {
                XmlElement element = doc.CreateElement("control");
                MpeControl referenceControl = parser.GetControl(control.Type);
                control.Save(doc, element, parser, referenceControl);
                controls.AppendChild(element);
              }
              catch (Exception e)
              {
                MpeLog.Debug(e);
                MpeLog.Error(e);
                throw new MpeParserException(e.Message);
              }
            }
          }
        }
      }
    }

    #endregion

    #region Event Handlers

    protected override void OnPaint(PaintEventArgs e)
    {
      /*if (backImage != null)
				e.Graphics.DrawImage(backImage.TextureImage,0,0,Width,Height);*/
      base.OnPaint(e);
    }

    protected override void OnLocationChanged(EventArgs e)
    {
      // Do nothing
    }

    #endregion
  }
}