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

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using Mpe.Controls.Design;
using Mpe.Controls.Properties;

namespace Mpe.Controls
{
  /// <summary>
  /// This class implements the behaviour for a MediaPortal button object.  It contains
  /// all of the properties and events required for the screen and control editor
  /// to manipulate button objects.
  /// </summary>
  public class MpeSelectButton : MpeContainer
  {
    #region Variables

    protected bool active;
    protected MpeItemManager items;
    protected MpeLabel label;
    protected MpeImage leftImage;
    protected MpeImage rightImage;
    protected Point offset;
    protected Size textureSize;
    protected FileInfo buttonTextureFile;
    protected FileInfo buttonFocusTextureFile;
    protected FileInfo backTextureFile;
    protected FileInfo leftTextureFile;
    protected FileInfo leftFocusTextureFile;
    protected FileInfo rightTextureFile;
    protected FileInfo rightFocusTextureFile;

    #endregion

    #region Constructors

    public MpeSelectButton() : base()
    {
      MpeLog.Debug("MpeSelectButton()");
      Type = MpeControlType.SelectButton;
      layoutStyle = MpeLayoutStyle.HorizontalFlow;
      autoSize = false;
      active = false;
      offset = Point.Empty;
      textureSize = new Size(32, 32);
      items = new MpeItemManager();
      items.Type = MpeItemType.Text;
      items.TypeChanging += new MpeItemManager.TypeChangingHandler(OnItemTypeChanging);
      leftImage = new MpeImage();
      leftImage.Embedded = true;
      leftImage.AutoSize = false;
      leftImage.Size = textureSize;
      leftImage.Padding = new MpeControlPadding(8);
      rightImage = new MpeImage();
      rightImage.Embedded = true;
      rightImage.AutoSize = false;
      rightImage.Size = textureSize;
      rightImage.Padding = new MpeControlPadding(8);
      label = new MpeLabel();
      label.Embedded = true;
      Prepare();
    }

    public MpeSelectButton(MpeSelectButton sb) : base(sb)
    {
      MpeLog.Debug("MpeSelectButton(sb)");
      Type = MpeControlType.SelectButton;
      offset = sb.offset;
      leftImage = new MpeImage(sb.leftImage);
      rightImage = new MpeImage(sb.rightImage);
      label = new MpeLabel(sb.label);
      items = new MpeItemManager(sb.items);
      items.TypeChanging += new MpeItemManager.TypeChangingHandler(OnItemTypeChanging);
      textureSize = sb.textureSize;
      buttonTextureFile = sb.buttonTextureFile;
      buttonFocusTextureFile = sb.buttonFocusTextureFile;
      backTextureFile = sb.backTextureFile;
      leftTextureFile = sb.leftTextureFile;
      leftFocusTextureFile = sb.leftFocusTextureFile;
      rightTextureFile = sb.rightTextureFile;
      rightFocusTextureFile = sb.rightFocusTextureFile;
      Prepare();
    }

    #endregion

    #region Properties

    [Category("Textures")]
    [RefreshPropertiesAttribute(RefreshProperties.Repaint)]
    public MpeControlPadding TexturePadding
    {
      get { return leftImage.Padding; }
      set
      {
        if (value != null)
        {
          int w = Width;
          // Set left image padding
          leftImage.Padding = value;
          // Set right image padding to be mirror of left image
          rightImage.Padding.Set(value.Right, value.Left, value.Top, value.Bottom);
          // Resize Controls
          leftImage.Size =
            rightImage.Size =
            new Size(textureSize.Width + value.Left + value.Right, textureSize.Height + value.Top + value.Bottom);
          if (active)
          {
            label.Width = w - leftImage.Width - rightImage.Width;
          }
          Modified = true;
          Prepare();
          FirePropertyValueChanged("TexturePadding");
        }
      }
    }

    [Category("Textures")]
    [RefreshPropertiesAttribute(RefreshProperties.Repaint)]
    public Size TextureSize
    {
      get { return textureSize; }
      set
      {
        if (textureSize != value)
        {
          textureSize = value;
          int w = Width;
          leftImage.Size =
            rightImage.Size =
            new Size(value.Width + leftImage.Padding.Left + leftImage.Padding.Right,
                     value.Height + leftImage.Padding.Top + leftImage.Padding.Bottom);
          if (active)
          {
            label.Width = w - leftImage.Width - rightImage.Width;
          }
          Modified = true;
          Prepare();
          FirePropertyValueChanged("TextureSize");
        }
      }
    }

    [Category("Textures")]
    [Editor(typeof(MpeImageEditor), typeof(UITypeEditor))]
    [RefreshPropertiesAttribute(RefreshProperties.Repaint)]
    [Description("This property defines the image that will be used to render the button when it has focus.")]
    public FileInfo TextureBackground
    {
      get { return backTextureFile; }
      set
      {
        if (backTextureFile != value)
        {
          if (value == null || value.Exists == false)
          {
            backTextureFile = null;
          }
          else
          {
            backTextureFile = value;
          }
          Prepare();
          Modified = true;
          FirePropertyValueChanged("TextureBackground");
        }
      }
    }

    [Category("Textures")]
    [Editor(typeof(MpeImageEditor), typeof(UITypeEditor))]
    [RefreshPropertiesAttribute(RefreshProperties.Repaint)]
    [Description("This property defines the image that will be used to render the button when it has focus.")]
    public FileInfo TextureButton
    {
      get { return buttonTextureFile; }
      set
      {
        if (buttonTextureFile != value)
        {
          if (value == null || value.Exists == false)
          {
            buttonTextureFile = null;
          }
          else
          {
            buttonTextureFile = value;
          }
          Prepare();
          Modified = true;
          FirePropertyValueChanged("TextureButton");
        }
      }
    }

    [Category("Textures")]
    [Editor(typeof(MpeImageEditor), typeof(UITypeEditor))]
    [RefreshPropertiesAttribute(RefreshProperties.Repaint)]
    [Description("This property defines the image that will be used to render the button when it has focus.")]
    public FileInfo TextureButtonFocus
    {
      get { return buttonFocusTextureFile; }
      set
      {
        if (buttonFocusTextureFile != value)
        {
          if (value == null || value.Exists == false)
          {
            buttonFocusTextureFile = null;
          }
          else
          {
            buttonFocusTextureFile = value;
          }
          Prepare();
          Modified = true;
          FirePropertyValueChanged("TextureButtonFocus");
        }
      }
    }

    [Category("Textures")]
    [Editor(typeof(MpeImageEditor), typeof(UITypeEditor))]
    [RefreshPropertiesAttribute(RefreshProperties.Repaint)]
    [Description("This property defines the image that will be used to render the button when it has focus.")]
    public FileInfo TextureLeft
    {
      get { return leftTextureFile; }
      set
      {
        if (leftTextureFile != value)
        {
          if (value == null || value.Exists == false)
          {
            leftTextureFile = null;
          }
          else
          {
            leftTextureFile = value;
          }
          Prepare();
          Modified = true;
          FirePropertyValueChanged("TextureLeft");
        }
      }
    }

    [Category("Textures")]
    [Editor(typeof(MpeImageEditor), typeof(UITypeEditor))]
    [RefreshPropertiesAttribute(RefreshProperties.Repaint)]
    [Description("This property defines the image that will be used to render the button when it has focus.")]
    public FileInfo TextureLeftFocus
    {
      get { return leftFocusTextureFile; }
      set
      {
        if (leftFocusTextureFile != value)
        {
          if (value == null || value.Exists == false)
          {
            leftFocusTextureFile = null;
          }
          else
          {
            leftFocusTextureFile = value;
          }
          Prepare();
          Modified = true;
          FirePropertyValueChanged("TextureLeftFocus");
        }
      }
    }

    [Category("Textures")]
    [Editor(typeof(MpeImageEditor), typeof(UITypeEditor))]
    [RefreshPropertiesAttribute(RefreshProperties.Repaint)]
    [Description("This property defines the image that will be used to render the button when it has focus.")]
    public FileInfo TextureRight
    {
      get { return rightTextureFile; }
      set
      {
        if (rightTextureFile != value)
        {
          if (value == null || value.Exists == false)
          {
            rightTextureFile = null;
          }
          else
          {
            rightTextureFile = value;
          }
          Prepare();
          Modified = true;
          FirePropertyValueChanged("TextureRight");
        }
      }
    }

    [Category("Textures")]
    [Editor(typeof(MpeImageEditor), typeof(UITypeEditor))]
    [RefreshPropertiesAttribute(RefreshProperties.Repaint)]
    [Description("This property defines the image that will be used to render the button when it has focus.")]
    public FileInfo TextureRightFocus
    {
      get { return rightFocusTextureFile; }
      set
      {
        if (rightFocusTextureFile != value)
        {
          if (value == null || value.Exists == false)
          {
            rightFocusTextureFile = null;
          }
          else
          {
            rightFocusTextureFile = value;
          }
          Prepare();
          Modified = true;
          FirePropertyValueChanged("TextureRightFocus");
        }
      }
    }

    [Browsable(false)]
    public override FileInfo TextureBack
    {
      get { return base.TextureBack; }
      set { base.TextureBack = value; }
    }

    [Category("Control")]
    [DefaultValue(false)]
    public bool Active
    {
      get { return active; }
      set
      {
        if (active != value)
        {
          active = value;
          Prepare();
        }
      }
    }

    [Browsable(true)]
    public override bool Focused
    {
      get { return base.Focused; }
      set
      {
        base.Focused = value;
        Prepare();
      }
    }

    [Category("Items")]
    public MpeItemManager Items
    {
      get { return items; }
      set { items = value; }
    }

    [Category("Labels")]
    [Browsable(true)]
    [RefreshProperties(RefreshProperties.Repaint)]
    public MpeControlPadding LabelPadding
    {
      get { return new MpeControlPadding(offset); }
      set
      {
        offset = value.ToPoint();
        Prepare();
      }
    }

    [Category("Labels")]
    [ReadOnly(false)]
    public MpeControlAlignment LabelAlignment
    {
      get { return label.Alignment; }
      set { label.Alignment = value; }
    }

    [Category("Labels")]
    [Browsable(true)]
    [RefreshProperties(RefreshProperties.Repaint)]
    [Editor(typeof(MpeStringEditor), typeof(UITypeEditor))]
    public string LabelText
    {
      get { return label.Text; }
      set
      {
        label.Text = value;
        Modified = true;
        Prepare();
        FirePropertyValueChanged("LabelText");
      }
    }

    [Category("Labels")]
    public virtual Color LabelColor
    {
      get { return label.TextColor; }
      set
      {
        if (label.TextColor != value)
        {
          label.TextColor = value;
          Modified = true;
          FirePropertyValueChanged("LabelColor");
        }
      }
    }

    [Category("Labels")]
    public virtual Color LabelDisabledColor
    {
      get { return label.DisabledColor; }
      set
      {
        if (label.DisabledColor != value)
        {
          label.DisabledColor = value;
          Modified = true;
          FirePropertyValueChanged("LabelDisabledColor");
        }
      }
    }

    [TypeConverter(typeof(MpeFontConverter))]
    [Category("Labels")]
    [Description("The font that will be used to render the button label.")]
    [RefreshProperties(RefreshProperties.Repaint)]
    public MpeFont LabelFont
    {
      get { return label.Font; }
      set
      {
        label.Font = value;
        Modified = true;
        Invalidate(false);
        FirePropertyValueChanged("LabelFont");
      }
    }

    [Browsable(true)]
    public override int OnLeft
    {
      get { return base.OnLeft; }
      set { base.OnLeft = value; }
    }

    [Browsable(true)]
    public override int OnRight
    {
      get { return base.OnRight; }
      set { base.OnRight = value; }
    }

    [Browsable(true)]
    public override int OnUp
    {
      get { return base.OnUp; }
      set { base.OnUp = value; }
    }

    [Browsable(true)]
    public override int OnDown
    {
      get { return base.OnDown; }
      set { base.OnDown = value; }
    }

    #endregion

    #region Properties - Hidden

    [Browsable(false)]
    public override Size GridSize
    {
      get { return base.GridSize; }
      set { base.GridSize = value; }
    }

    [Browsable(false)]
    public override bool ShowBorder
    {
      get { return base.ShowBorder; }
      set { base.ShowBorder = value; }
    }

    [Browsable(false)]
    public override bool ShowGrid
    {
      get { return base.ShowGrid; }
      set { base.ShowGrid = value; }
    }

    [Browsable(false)]
    public override bool SnapToGrid
    {
      get { return base.SnapToGrid; }
      set { base.SnapToGrid = value; }
    }

    [Browsable(false)]
    public override bool Spring
    {
      get { return base.Spring; }
      set { base.Spring = value; }
    }

    [Browsable(false)]
    public override MpeLayoutStyle LayoutStyle
    {
      get { return base.LayoutStyle; }
      set { base.LayoutStyle = value; }
    }

    [Browsable(false)]
    public override bool AutoSize
    {
      get { return base.AutoSize; }
      set { base.AutoSize = value; }
    }

    [Browsable(false)]
    public override int Spacing
    {
      get { return base.Spacing; }
      set { base.Spacing = value; }
    }

    [Browsable(false)]
    public override MpeControlPadding Padding
    {
      get { return base.Padding; }
      set { base.Padding = value; }
    }

    [Browsable(false)]
    public override MpeControlAlignment Alignment
    {
      get { return base.Alignment; }
      set { base.Alignment = value; }
    }

    #endregion

    #region Methods

    public override MpeControl Copy()
    {
      return new MpeSelectButton(this);
    }

    protected override void PrepareControl()
    {
      if (leftImage != null && rightImage != null && label != null)
      {
        int w = Width;
        Controls.Clear();
        if (active)
        {
          if (Spring)
          {
            Spring = false;
            label.Width -= (leftImage.Width + rightImage.Width);
          }
          label.Padding.Set(0, offset.Y);
          TextureBack = TextureBackground;
          if (focused)
          {
            leftImage.Texture = leftFocusTextureFile;
            rightImage.Texture = rightFocusTextureFile;
          }
          else
          {
            leftImage.Texture = leftTextureFile;
            rightImage.Texture = rightTextureFile;
          }
          Controls.Add(leftImage);
          Controls.Add(label);
          Controls.Add(rightImage);
        }
        else
        {
          label.Width = w;
          label.Padding.Set(offset);
          if (focused)
          {
            TextureBack = TextureButtonFocus;
          }
          else
          {
            TextureBack = TextureButton;
          }
          Controls.Add(label);
          Spring = true;
        }
        base.PrepareControl();
      }
    }

    public override void Load(XPathNodeIterator iterator, MpeParser parser)
    {
      MpeLog.Debug("MpeSelectButton.Load()");
      base.Load(iterator, parser);
      this.parser = parser;
      // Regular Button Properties
      label.Load(iterator, parser);
      label.AutoSize = false;
      label.Size = new Size(parser.GetInt(iterator, "width", Width), parser.GetInt(iterator, "height", Height));
      offset = new Point(parser.GetInt(iterator, "textXOff2", offset.X), parser.GetInt(iterator, "textYOff2", offset.Y));
      TextureButton = parser.GetImageFile(iterator, "textureNoFocus", TextureButton);
      TextureButtonFocus = parser.GetImageFile(iterator, "textureFocus", TextureButton);
      tags.Remove("align");
      tags.Remove("font");
      tags.Remove("textcolor");
      tags.Remove("disabledcolor");
      tags.Remove("label");
      tags.Remove("textXOff");
      tags.Remove("textYOff");
      tags.Remove("textureFocus");
      tags.Remove("textureNoFocus");
      // Select Button Properties
      int x = parser.GetInt(iterator, "textXOff", TexturePadding.Width);
      TextureSize =
        new Size(parser.GetInt(iterator, "textureWidth", textureSize.Width),
                 parser.GetInt(iterator, "textureHeight", textureSize.Height));
      TexturePadding = new MpeControlPadding(x/2, (Height - textureSize.Height)/2);
      TextureBackground = parser.GetImageFile(iterator, "texturebg", TextureBack);
      TextureLeft = parser.GetImageFile(iterator, "textureLeft", TextureLeft);
      TextureLeftFocus = parser.GetImageFile(iterator, "textureLeftFocus", TextureLeftFocus);
      TextureRight = parser.GetImageFile(iterator, "textureRight", TextureRight);
      TextureRightFocus = parser.GetImageFile(iterator, "textureRightFocus", TextureRightFocus);
      tags.Remove("texturebg");
      tags.Remove("textureLeft");
      tags.Remove("textureLeftFocus");
      tags.Remove("textureRight");
      tags.Remove("textureRightFocus");
      tags.Remove("textureWidth");
      tags.Remove("textureHeight");
      tags.Remove("textXOff2");
      tags.Remove("textYOff2");
      // Get SubItems
      items.Values.Clear();
      XPathNodeIterator i = iterator.Current.Select("subitems/subitem");
      while (i.MoveNext())
      {
        MpeItem item = new MpeItem();
        item.Value = i.Current.Value;
        items.Values.Add(item);
      }
      Prepare();
      Modified = false;
    }

    public override void Save(XmlDocument doc, XmlNode node, MpeParser parser, MpeControl reference)
    {
      base.Save(doc, node, parser, reference);
      MpeSelectButton button = null;
      if (reference != null && reference is MpeSelectButton)
      {
        button = (MpeSelectButton) reference;
      }
      // Regular Button Properties
      parser.SetValue(doc, node, "label", label.Text);
      if (button == null || button.label.Font.Name.Equals(label.Font.Name) == false)
      {
        parser.SetValue(doc, node, "font", label.Font.Name);
      }
      if (button == null || button.label.Alignment != label.Alignment)
      {
        parser.SetValue(doc, node, "align", label.Alignment.ToString().ToLower());
      }
      if (button == null || button.label.TextColor != label.TextColor)
      {
        parser.SetColor(doc, node, "textcolor", label.TextColor);
      }
      if (button == null || button.label.DisabledColor != label.DisabledColor)
      {
        parser.SetColor(doc, node, "disabledcolor", label.DisabledColor);
      }
      if (button == null || button.TextureButton == null || button.TextureButton.Equals(TextureButton) == false)
      {
        if (TextureButton == null)
        {
          parser.SetValue(doc, node, "textureNoFocus", "-");
        }
        else
        {
          parser.SetValue(doc, node, "textureNoFocus", TextureButton.Name);
        }
      }
      if (button == null || button.TextureButtonFocus == null ||
          button.TextureButtonFocus.Equals(TextureButtonFocus) == false)
      {
        if (TextureButtonFocus == null)
        {
          parser.SetValue(doc, node, "textureFocus", "-");
        }
        else
        {
          parser.SetValue(doc, node, "textureFocus", TextureButtonFocus.Name);
        }
      }
      if (button == null || button.LabelPadding.Left != LabelPadding.Left)
      {
        parser.SetInt(doc, node, "textXOff2", LabelPadding.Left);
      }
      if (button == null || button.LabelPadding.Top != LabelPadding.Top)
      {
        parser.SetInt(doc, node, "textYOff2", LabelPadding.Top);
      }
      // Select Button Specific Properties
      if (button == null || button.TexturePadding.Width != TexturePadding.Width)
      {
        parser.SetInt(doc, node, "textXOff", TexturePadding.Width);
      }
      if (button == null || button.LabelPadding.Top != LabelPadding.Top)
      {
        parser.SetInt(doc, node, "textYOff", LabelPadding.Top);
      }
      if (button == null || button.textureSize != textureSize)
      {
        parser.SetInt(doc, node, "textureWidth", textureSize.Width);
        parser.SetInt(doc, node, "textureHeight", textureSize.Height);
      }
      if (button == null || button.TextureBackground == null ||
          button.TextureBackground.Equals(TextureBackground) == false)
      {
        if (TextureBackground == null)
        {
          parser.SetValue(doc, node, "texturebg", "-");
        }
        else
        {
          parser.SetValue(doc, node, "texturebg", TextureBackground.Name);
        }
      }
      if (button == null || button.TextureLeft == null || button.TextureLeft.Equals(TextureLeft) == false)
      {
        if (TextureLeft == null)
        {
          parser.SetValue(doc, node, "textureLeft", "-");
        }
        else
        {
          parser.SetValue(doc, node, "textureLeft", TextureLeft.Name);
        }
      }
      if (button == null || button.TextureLeftFocus == null || button.TextureLeftFocus.Equals(TextureLeftFocus) == false
        )
      {
        if (TextureLeftFocus == null)
        {
          parser.SetValue(doc, node, "textureLeftFocus", "-");
        }
        else
        {
          parser.SetValue(doc, node, "textureLeftFocus", TextureLeftFocus.Name);
        }
      }
      if (button == null || button.TextureRight == null || button.TextureRight.Equals(TextureRight) == false)
      {
        if (TextureRight == null)
        {
          parser.SetValue(doc, node, "textureRight", "-");
        }
        else
        {
          parser.SetValue(doc, node, "textureRight", TextureRight.Name);
        }
      }
      if (button == null || button.TextureRightFocus == null ||
          button.TextureRightFocus.Equals(TextureRightFocus) == false)
      {
        if (TextureRightFocus == null)
        {
          parser.SetValue(doc, node, "textureRightFocus", "-");
        }
        else
        {
          parser.SetValue(doc, node, "textureRightFocus", TextureRightFocus.Name);
        }
      }
      XmlElement subitems = doc.CreateElement("subitems");
      for (int i = 0; items != null && items.Values != null && i < items.Values.Count; i++)
      {
        XmlElement subitem = doc.CreateElement("subitem");
        subitem.AppendChild(doc.CreateTextNode(items.Values[i].Value));
        subitems.AppendChild(subitem);
      }
      node.AppendChild(subitems);
      //parser.SetValue(doc, node, "type", parser.ControlTypeToXmlString(Type));
    }

    #endregion

    #region Event Handlers

    private void OnItemTypeChanging(MpeItemEventArgs e)
    {
      if (e.NewType != MpeItemType.Text)
      {
        MpeLog.Warn("MpeSelectButton items must be of type of Text");
        e.CancelTypeChange = true;
      }
    }

    #endregion
  }
}