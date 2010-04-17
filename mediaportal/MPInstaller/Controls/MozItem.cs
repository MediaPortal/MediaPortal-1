#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Pabo.MozBar
{

  #region Enumerations

  public enum MozItemState
  {
    Normal = 0,
    Focus = 1,
    Selected = 2
  }

  public enum MozItemStyle
  {
    Text = 0,
    Picture = 1,
    TextAndPicture = 2,
    Divider = 3
  }

  public enum MozTextAlign
  {
    Bottom = 0,
    Right = 1,
    Top = 2,
    Left = 3
  }

  #endregion

  #region delegates

  public delegate void MozItemEventHandler(object sender, MozItemEventArgs e);

  public delegate void MozItemClickEventHandler(object sender, MozItemClickEventArgs e);

  public delegate void ImageChangedEventHandler(object sender, ImageChangedEventArgs e);

  #endregion

  /// <summary>
  /// Summary description for MozItem.
  /// </summary>
  [Designer(typeof (MozItemDesigner))]
  [ToolboxItem(true)]
  [DefaultEvent("Click")]
  [ToolboxBitmap(typeof (MozItem), "Pabo.MozBar.MozItem.bmp")]
  public class MozItem : Control
  {
    #region EventHandler

    internal event MozItemEventHandler ItemGotFocus;
    internal event MozItemEventHandler ItemLostFocus;
    internal event MozItemClickEventHandler ItemClick;
    internal event MozItemClickEventHandler ItemDoubleClick;

    [Browsable(true)]
    [Category("Property Changed")]
    [Description("Indicates that the ItemStyle has changed.")]
    public event EventHandler ItemStyleChanged;

    [Browsable(true)]
    [Category("Property Changed")]
    [Description("Indicates that an item image has changed.")]
    public event ImageChangedEventHandler ImageChanged;

    [Browsable(true)]
    [Category("Property Changed")]
    [Description("Indicates that TextAlign has changed.")]
    public event EventHandler TextAlignChanged;

    #endregion

    #region private class members

    private MozPane m_mozPane;
    private MouseButtons m_mouseButton;
    private Image image;

    private ImageCollection m_imageCollection;

    private MozTextAlign m_textAlign;

    private MozItemState m_state;
    private MozItemStyle m_itemStyle;

    private Container components = null;

    #endregion

    #region constructor

    public MozItem()
    {
      // This call is required by the Windows.Forms Form Designer.
      InitializeComponent();

      this.SetStyle(ControlStyles.DoubleBuffer, true);
      this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
      this.SetStyle(ControlStyles.UserPaint, true);
      this.SetStyle(ControlStyles.ResizeRedraw, true);
      this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);

      // TODO: Add any initialization after the InitializeComponent call

      m_imageCollection = new ImageCollection(this);

      image = null;

      m_state = MozItemState.Normal;

      m_itemStyle = MozItemStyle.TextAndPicture;
      m_textAlign = MozTextAlign.Bottom;
      DoLayout();
    }

    #endregion

    #region dispose

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }

        // Delete images...
        if (image != null)
        {
          image.Dispose();
        }
        image = null;
        m_imageCollection.Dispose();
      }
      base.Dispose(disposing);
    }

    #endregion

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {}

    #endregion

    #region Properties

    #region Colors

    private Color SelectedColor
    {
      get
      {
        if (m_mozPane != null)
        {
          return m_mozPane.ItemColors.SelectedBackground;
        }
        else
        {
          return Color.FromArgb(193, 210, 238);
        }
      }
    }

    private Color SelectedBorderColor
    {
      get
      {
        if (m_mozPane != null)
        {
          return m_mozPane.ItemColors.SelectedBorder;
        }
        else
        {
          return Color.FromArgb(49, 106, 197);
        }
      }
    }

    private Color SelectedText
    {
      get
      {
        if (m_mozPane != null)
        {
          return m_mozPane.ItemColors.SelectedText;
        }
        else
        {
          return Color.Black;
        }
      }
    }

    private Color FocusText
    {
      get
      {
        if (m_mozPane != null)
        {
          return m_mozPane.ItemColors.FocusText;
        }
        else
        {
          return Color.Black;
        }
      }
    }

    private Color FocusColor
    {
      get
      {
        if (m_mozPane != null)
        {
          return m_mozPane.ItemColors.FocusBackground;
        }
        else
        {
          return Color.FromArgb(224, 232, 246);
        }
      }
    }

    private Color FocusBorderColor
    {
      get
      {
        if (m_mozPane != null)
        {
          return m_mozPane.ItemColors.FocusBorder;
        }
        else
        {
          return Color.White;
        }
      }
    }

    private Color TextColor
    {
      get
      {
        if (m_mozPane != null)
        {
          return m_mozPane.ItemColors.Text;
        }
        else
        {
          return Color.Black;
        }
      }
    }

    private Color BackgroundColor
    {
      get
      {
        if (m_mozPane != null)
        {
          return m_mozPane.ItemColors.Background;
        }
        else
        {
          return Color.White;
        }
      }
    }

    private Color BorderColor
    {
      get
      {
        if (m_mozPane != null)
        {
          return m_mozPane.ItemColors.Border;
        }
        else
        {
          return Color.FromArgb(152, 180, 226);
        }
      }
    }

    private Color DividerColor
    {
      get
      {
        if (m_mozPane != null)
        {
          return m_mozPane.ItemColors.Divider;
        }
        else
        {
          return Color.FromArgb(127, 157, 185);
        }
      }
    }

    #endregion

    #region BorderStyles

    private ButtonBorderStyle SelectedBorderStyle
    {
      get
      {
        if (m_mozPane != null)
        {
          return m_mozPane.ItemBorderStyles.Selected;
        }
        else
        {
          return ButtonBorderStyle.Solid;
        }
      }
    }

    private ButtonBorderStyle NormalBorderStyle
    {
      get
      {
        if (m_mozPane != null)
        {
          return m_mozPane.ItemBorderStyles.Normal;
        }
        else
        {
          return ButtonBorderStyle.None;
        }
      }
    }

    private ButtonBorderStyle FocusBorderStyle
    {
      get
      {
        if (m_mozPane != null)
        {
          return m_mozPane.ItemBorderStyles.Focus;
        }
        else
        {
          return ButtonBorderStyle.Solid;
        }
      }
    }

    #endregion

    [Browsable(true)]
    [Category("Appearance")]
    [Description("Images for various states.")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ImageCollection Images
    {
      get { return m_imageCollection; }
      set
      {
        if ((value != null) && (value != m_imageCollection))
        {
          m_imageCollection = value;
        }
      }
    }

    [Browsable(true)]
    [Category("Appearance")]
    [Description("The alignment of the text that will be displayed.")]
    [DefaultValue(typeof (MozTextAlign), "Bottom")]
    public MozTextAlign TextAlign
    {
      get { return m_textAlign; }
      set
      {
        if (m_textAlign != value)
        {
          m_textAlign = value;
          DoLayout();
          if (this.MozPane != null)
          {
            this.MozPane.DoLayout();
          }
          if (this.TextAlignChanged != null)
          {
            this.TextAlignChanged(this, new EventArgs());
          }

          this.Invalidate();
        }
      }
    }

    public override string Text
    {
      get { return base.Text; }
      set
      {
        if (value != base.Text)
        {
          base.Text = value;
          this.DoLayout();
          this.Invalidate();
        }
      }
    }

    [Browsable(true)]
    [Category("Appearance")]
    [Description("The visual appearance of the item.")]
    [DefaultValue(typeof (MozItemStyle), "TextAndPicture")]
    public MozItemStyle ItemStyle
    {
      get { return m_itemStyle; }
      set
      {
        if (value != m_itemStyle)
        {
          m_itemStyle = value;
          DoLayout();
          if (this.MozPane != null)
          {
            this.MozPane.DoLayout();
          }
          if (ItemStyleChanged != null)
          {
            this.ItemStyleChanged(this, new EventArgs());
          }
          this.Invalidate();
        }
      }
    }

    // obsolete properties

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This property is not supported", true)]
    public override Color BackColor
    {
      get { return base.BackColor; }
      set { base.BackColor = value; }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This property is not supported", true)]
    public override Color ForeColor
    {
      get { return base.ForeColor; }
      set { base.ForeColor = value; }
    }


    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This property is not supported", true)]
    public override Image BackgroundImage
    {
      get { return base.BackgroundImage; }
      set { base.BackgroundImage = value; }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This property is not supported", true)]
    public override RightToLeft RightToLeft
    {
      get { return base.RightToLeft; }
      set { base.RightToLeft = value; }
    }

    protected internal MozPane MozPane
    {
      get { return this.m_mozPane; }

      set { this.m_mozPane = value; }
    }


    internal MozItemState state
    {
      get { return m_state; }
      set
      {
        m_state = value;
        Invalidate();
      }
    }

    #endregion

    #region methods

    public void DoLayout()
    {
      int imageHeight;
      int imageWidth;
      MozPaneStyle mode;

      ImageList list = GetImageList();

      if (list != null)
      {
        imageHeight = list.ImageSize.Height;
        imageWidth = list.ImageSize.Width;
      }
      else
      {
        imageHeight = 32;
        imageWidth = 32;
      }

      if (m_mozPane != null)
      {
        mode = m_mozPane.Style;
      }
      else
      {
        mode = MozPaneStyle.Vertical;
      }

      switch (mode)
      {
        case MozPaneStyle.Vertical:
          {
            if (m_mozPane != null)
            {
              if (!m_mozPane.IsVerticalScrollBarVisible())
              {
                this.Width = m_mozPane.Width - (2 * m_mozPane.Padding.Horizontal);
              }
              else
              {
                this.Width = m_mozPane.Width - (2 * m_mozPane.Padding.Horizontal) - 3 -
                             (SystemInformation.VerticalScrollBarWidth - 2);
              }
            }
            else
            {
              this.Width = 40;
            }

            switch (m_itemStyle)
            {
              case MozItemStyle.Divider:
                {
                  this.Height = 2 * 4;
                  break;
                }

              case MozItemStyle.Picture:
                {
                  this.Height = imageHeight + (2 * 4);
                  break;
                }
              case MozItemStyle.Text:
                {
                  this.Height = base.Font.Height + (2 * 4);
                  break;
                }
              case MozItemStyle.TextAndPicture:
                {
                  switch (m_textAlign)
                  {
                    case MozTextAlign.Bottom:
                    case MozTextAlign.Top:
                      {
                        this.Height = imageHeight + (3 * 4) + base.Font.Height;
                        break;
                      }
                    case MozTextAlign.Right:
                    case MozTextAlign.Left:
                      {
                        this.Height = imageHeight + (2 * 4);
                        break;
                      }
                  }
                  break;
                }
            }
            break;
          }
        case MozPaneStyle.Horizontal:
          {
            if (m_mozPane != null)
            {
              if (!m_mozPane.IsHorizontalScrollBarVisible())
              {
                this.Height = m_mozPane.Height - (2 * m_mozPane.Padding.Vertical);
              }
              else
              {
                this.Height = m_mozPane.Height - (2 * m_mozPane.Padding.Vertical) - 3 -
                              (SystemInformation.HorizontalScrollBarHeight - 2);
              }
            }

            else
            {
              this.Height = 40;
            }

            switch (m_itemStyle)
            {
              case MozItemStyle.Divider:
                {
                  this.Width = 2 * 4;
                  break;
                }
              case MozItemStyle.Picture:
                {
                  this.Width = imageWidth + (2 * 4);
                  break;
                }
              case MozItemStyle.Text:
                {
                  this.Width = (2 * 4) + (int)MeasureString(this.Text);
                  break;
                }
              case MozItemStyle.TextAndPicture:
                {
                  switch (m_textAlign)
                  {
                    case MozTextAlign.Bottom:
                    case MozTextAlign.Top:
                      {
                        int minWidth = 2 * 4 + imageWidth;
                        int stringWidth = (2 * 4) + (int)MeasureString(this.Text);
                        if (stringWidth > minWidth)
                        {
                          this.Width = stringWidth;
                        }
                        else
                        {
                          this.Width = minWidth;
                        }
                        break;
                      }
                    case MozTextAlign.Right:
                    case MozTextAlign.Left:
                      {
                        this.Width = (3 * 4) + (int)MeasureString(this.Text) + imageWidth;
                        break;
                      }
                  }
                  break;
                }
            }
            break;
          }
      }
    }

    private float MeasureString(string str)
    {
      SizeF f = new SizeF();
      Graphics g;
      g = this.CreateGraphics();
      f = g.MeasureString(str, this.Font);
      g.Dispose();
      g = null;

      return f.Width;
    }

    public ImageList GetImageList()
    {
      if (m_mozPane == null)
      {
        return null;
      }
      else
      {
        return m_mozPane.ImageList;
      }
    }

    public bool IsSelected()
    {
      if (m_state == MozItemState.Selected)
      {
        return true;
      }
      else
      {
        return false;
      }
    }

    public void SelectItem()
    {
      if (this.Enabled)
      {
        if (ItemClick != null)
        {
          this.ItemClick(this, new MozItemClickEventArgs(this, MouseButtons.Left));
        }
        Invalidate();
      }
    }

    #endregion

    #region Events

    protected override void OnEnabledChanged(EventArgs e)
    {
      base.OnEnabledChanged(e);
      // Enabled has changed, Invalidate..
      Invalidate();
    }

    protected override void OnFontChanged(EventArgs e)
    {
      base.OnFontChanged(e);
      DoLayout();
      Invalidate();
    }

    protected override void OnSizeChanged(EventArgs e)
    {
      base.OnSizeChanged(e);

      if (m_mozPane != null)
      {
        m_mozPane.DoLayout();
        m_mozPane.Invalidate();
      }
      DoLayout();
      Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
      if (ItemGotFocus != null)
      {
        this.ItemGotFocus(this, new MozItemEventArgs(this));
      }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
      if (ItemLostFocus != null)
      {
        this.ItemLostFocus(this, new MozItemEventArgs(this));
      }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
      m_mouseButton = e.Button;
      if (ItemClick != null)
      {
        this.ItemClick(this, new MozItemClickEventArgs(this, m_mouseButton));
      }
    }

    protected override void OnDoubleClick(EventArgs e)
    {
      if (ItemDoubleClick != null)
      {
        this.ItemDoubleClick(this, new MozItemClickEventArgs(this, m_mouseButton));
      }
    }

    protected override void OnClick(EventArgs e)
    {
      base.OnClick(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
      int imageHeight;
      int imageWidth;
      int paddingX;
      int paddingY;


      Pen dividerPen = new Pen(this.DividerColor, 0);
      Brush textBrush = new SolidBrush(this.TextColor);
      Brush disabledTextBrush = new SolidBrush(Color.Gray);
      Brush bgBrush = new SolidBrush(Color.Black);
      Color borderColor = Color.Black;
      ButtonBorderStyle btnBorderStyle = ButtonBorderStyle.None;

      // Check if a ImageList exist
      ImageList list = GetImageList();
      if (list != null)
      {
        // if so get Height and Width
        imageHeight = list.ImageSize.Height;
        imageWidth = list.ImageSize.Width;
      }
      else
      {
        // if not use default values
        imageHeight = 32;
        imageWidth = 32;
      }

      // Check if the item has belongs to a MozPane
      if (m_mozPane != null)
      {
        // If so get the padding
        paddingX = m_mozPane.Padding.Horizontal;
        paddingY = m_mozPane.Padding.Vertical;
      }
      else
      {
        // use some kind of padding if no daddy is found
        paddingX = 1;
        paddingY = 1;
      }

      Rectangle textRect = new Rectangle();
      Rectangle imageRect = new Rectangle(0, 0, imageWidth, imageHeight);
      Rectangle borderRect = new Rectangle();

      StringFormat f = new StringFormat();

      Point borderLocation = new Point();

      borderLocation.X = 0;
      borderLocation.Y = 0;

      borderRect.Location = borderLocation;
      borderRect.Width = this.Width;
      borderRect.Height = this.Height;

      // Draw background
      e.Graphics.FillRectangle(new SolidBrush(this.BackgroundColor), this.DisplayRectangle);

      // Use Normal image for disabled state
      if (!this.Enabled)
      {
        this.m_state = MozItemState.Normal;
      }
      // A divider should not be able to be selected or recieve focus
      if (this.ItemStyle == MozItemStyle.Divider)
      {
        this.m_state = MozItemState.Normal;
      }

      // Check state for item, to decide image
      switch (m_state)
      {
        case MozItemState.Focus:
          {
            textBrush = new SolidBrush(this.FocusText);
            bgBrush = new SolidBrush(this.FocusColor);
            borderColor = this.FocusBorderColor;
            btnBorderStyle = this.FocusBorderStyle;
            if (m_imageCollection.FocusImage != null)
            {
              image = m_imageCollection.FocusImage;
            }
            else
            {
              // if focusimage isnt set use Normal image
              image = m_imageCollection.NormalImage;
            }
            break;
          }
        case MozItemState.Selected:
          {
            textBrush = new SolidBrush(this.SelectedText);
            bgBrush = new SolidBrush(this.SelectedColor);
            borderColor = this.SelectedBorderColor;
            btnBorderStyle = this.SelectedBorderStyle;
            if (m_imageCollection.SelectedImage != null)
            {
              image = m_imageCollection.SelectedImage;
            }
            else
            {
              image = m_imageCollection.NormalImage;
            }
            break;
          }
        case MozItemState.Normal:
          {
            image = m_imageCollection.NormalImage;
            bgBrush = new SolidBrush(this.BackgroundColor);
            btnBorderStyle = this.NormalBorderStyle;
            borderColor = this.BorderColor;
            break;
          }
      }

      e.Graphics.FillRectangle(bgBrush, borderRect);
      ControlPaint.DrawBorder(e.Graphics, borderRect, borderColor, btnBorderStyle);

      // check for itemStyle
      switch (m_itemStyle)
      {
        case MozItemStyle.Divider:
          {
            float ptY;
            float ptX;

            if (m_mozPane != null)
            {
              // Check MozPane orientation
              if (m_mozPane.Style == MozPaneStyle.Vertical)
              {
                ptY = borderRect.Top + (borderRect.Height / 2);
                e.Graphics.DrawLine(dividerPen, borderRect.Left, ptY, borderRect.Right, ptY);
              }
              else
              {
                ptX = borderRect.Left + (borderRect.Width / 2);
                e.Graphics.DrawLine(dividerPen, ptX, borderRect.Top, ptX, borderRect.Bottom);
              }
            }
            else
            {
              ptY = borderRect.Top + (borderRect.Height / 2);
              e.Graphics.DrawLine(dividerPen, borderRect.Left, ptY, borderRect.Right, ptY);
            }

            break;
          }
        case MozItemStyle.Text:
          {
            f.Alignment = StringAlignment.Center;
            f.LineAlignment = StringAlignment.Center;
            textRect = borderRect;
            if (m_state == MozItemState.Selected)
            {
              textRect.X += 1;
              textRect.Y += 1;
            }
            if (this.Enabled)
            {
              e.Graphics.DrawString(this.Text, this.Font, textBrush, textRect, f);
            }
            else
            {
              e.Graphics.DrawString(this.Text, this.Font, disabledTextBrush, textRect, f);
            }
            break;
          }
        case MozItemStyle.Picture:
          {
            if (image != null)
            {
              // center image
              imageRect.X = ((borderRect.Width / 2) - (imageRect.Width / 2));
              imageRect.Y = ((borderRect.Height / 2) - (imageRect.Height / 2));
              if (m_state == MozItemState.Selected)
              {
                imageRect.X += 1;
                imageRect.Y += 1;
              }

              if (this.Enabled)
              {
                if (image != null)
                {
                  e.Graphics.DrawImage(image, imageRect);
                }
                else if (image != null)
                {
                  ControlPaint.DrawImageDisabled(e.Graphics, image, imageRect.X, imageRect.Y, this.BackgroundColor);
                }
              }
            }
            break;
          }
        case MozItemStyle.TextAndPicture:
          {
            f.LineAlignment = StringAlignment.Center;

            switch (m_textAlign)
            {
              case MozTextAlign.Bottom:
                {
                  f.Alignment = StringAlignment.Center;
                  textRect.Height = this.Font.Height + (2 * 4);
                  textRect.Y = borderRect.Bottom - textRect.Height;
                  textRect.X = borderRect.X;
                  textRect.Width = borderRect.Width;

                  imageRect.Y = borderRect.Top + 2;
                  imageRect.X = ((borderRect.Width / 2) - imageRect.Width / 2);
                  break;
                }
              case MozTextAlign.Top:
                {
                  f.Alignment = StringAlignment.Center;
                  textRect.Height = this.Font.Height + (2 * 4);
                  textRect.Y = borderRect.Top;
                  textRect.X = borderRect.X;
                  textRect.Width = borderRect.Width;

                  imageRect.Y = borderRect.Bottom - 2 - imageRect.Height;
                  imageRect.X = ((borderRect.Width / 2) - imageRect.Width / 2);
                  break;
                }
              case MozTextAlign.Right:
                {
                  f.Alignment = StringAlignment.Near;
                  textRect.Height = borderRect.Height - 2 * 4;
                  textRect.Y = borderRect.Top + 4;
                  textRect.X = borderRect.X + 4 + imageRect.Width + 4;
                  textRect.Width = borderRect.Width - 4 - imageRect.Width;

                  imageRect.X = 4;
                  imageRect.Y = ((borderRect.Height / 2) - (imageRect.Height / 2));
                  break;
                }
              case MozTextAlign.Left:
                {
                  f.Alignment = StringAlignment.Near;
                  textRect.Height = borderRect.Height - 2 * 4;
                  textRect.Y = borderRect.Top + 4;
                  textRect.X = borderRect.X + 4;
                  textRect.Width = borderRect.Width - 4 - imageRect.Width;

                  imageRect.X = borderRect.Right - 4 - imageRect.Width;
                  imageRect.Y = ((borderRect.Height / 2) - (imageRect.Height / 2));
                  break;
                }
            }

            // Check if enabled
            if (this.Enabled)
            {
              if (m_state == MozItemState.Selected)
              {
                imageRect.X += 1;
                imageRect.Y += 1;
                textRect.X += 1;
                textRect.Y += 1;
              }
              // draw image and text
              if (image != null)
              {
                e.Graphics.DrawImage(image, imageRect);
              }
              e.Graphics.DrawString(this.Text, this.Font, textBrush, textRect, f);
            }
            else
            {
              // Draw disabled image and text
              if (image != null)
              {
                ControlPaint.DrawImageDisabled(e.Graphics, image, imageRect.X, imageRect.Y, this.BackColor);
              }
              e.Graphics.DrawString(this.Text, this.Font, disabledTextBrush, textRect, f);
            }

            break;
          }
      }

      // tidy up
      dividerPen.Dispose();
      textBrush.Dispose();
      disabledTextBrush.Dispose();
      bgBrush.Dispose();
    }

    protected override void OnResize(EventArgs e)
    {
      if (m_mozPane != null)
      {
        m_mozPane.DoLayout();
        m_mozPane.Invalidate();
      }
      DoLayout();
      Invalidate();
    }

    protected override void OnMove(EventArgs e)
    {
      Invalidate();
    }

    #endregion

    #region ImageCollection

    [TypeConverter(typeof (ItemCollectionTypeConverter))]
    public class ImageCollection
    {
      private MozItem m_item;

      private int m_imageIndex;
      private int m_focusImageIndex;
      private int m_selectedImageIndex;

      // used for setting image without using an imagelist..
      private Image m_image;
      private Image m_focusImage;
      private Image m_selectedImage;

      public MozItem Item;


      public ImageCollection(MozItem item)
      {
        m_item = item;
        m_imageIndex = -1;
        m_focusImageIndex = -1;
        m_selectedImageIndex = -1;

        m_image = null;
        m_focusImage = null;
        m_selectedImage = null;
      }

      public void Dispose()
      {
        m_imageIndex = -1;
        m_focusImageIndex = -1;
        m_selectedImageIndex = -1;
        if (m_image != null)
        {
          m_image.Dispose();
        }
        if (m_focusImage != null)
        {
          m_focusImage.Dispose();
        }
        if (m_selectedImage != null)
        {
          m_selectedImage.Dispose();
        }
      }

      public ImageList GetImageList()
      {
        if (m_item == null)
        {
          return null;
        }
        else
        {
          return m_item.GetImageList();
        }
      }

      [Browsable(false)]
      public Image NormalImage
      {
        get
        {
          // If image already has been set (without imagelist) return it.
          if (m_image != null)
          {
            return m_image;
          }

          // Check that an ImageList exists and that index is valid
          if ((GetImageList() != null) && (m_imageIndex != -1))
          {
            // make sure we catch any exceptions that might occur here
            try
            {
              // return image from panels ImageList
              return GetImageList().Images[m_imageIndex];
            }
            catch (Exception)
            {
              // An exception occured , the imagelist (or image) might have been disposed
              // or removed in the designer , return null
              return null;
            }
          }
          else
          {
            return null;
          }
        }
        set
        {
          m_image = value;
          if (m_item.ImageChanged != null)
          {
            m_item.ImageChanged(this, new ImageChangedEventArgs(MozItemState.Normal));
          }
          m_item.Invalidate();
        }
      }

      [TypeConverter(typeof (ImageTypeConverter))]
      [Editor(typeof (ImageMapEditor), typeof (UITypeEditor))]
      [Description("Image for normal state.")]
      public int Normal
      {
        get { return m_imageIndex; }
        set
        {
          if (value != m_imageIndex)
          {
            m_imageIndex = value;
            if (m_item.ImageChanged != null)
            {
              m_item.ImageChanged(this, new ImageChangedEventArgs(MozItemState.Normal));
            }
            m_item.Invalidate();
          }
        }
      }

      [Browsable(false)]
      public Image FocusImage
      {
        get
        {
          if (m_focusImage != null)
          {
            return m_focusImage;
          }

          if ((GetImageList() != null) && (m_focusImageIndex != -1))
          {
            try
            {
              return GetImageList().Images[m_focusImageIndex];
            }
            catch (Exception)
            {
              return null;
            }
          }
          else
          {
            return null;
          }
        }
        set
        {
          m_focusImage = value;
          if (m_item.ImageChanged != null)
          {
            m_item.ImageChanged(this, new ImageChangedEventArgs(MozItemState.Focus));
          }
          m_item.Invalidate();
        }
      }

      [TypeConverter(typeof (ImageTypeConverter))]
      [Editor(typeof (ImageMapEditor), typeof (UITypeEditor))]
      [Description("Image for has focus state.")]
      public int Focus
      {
        get { return m_focusImageIndex; }
        set
        {
          if (value != m_focusImageIndex)
          {
            m_focusImageIndex = value;

            if (m_item.ImageChanged != null)
            {
              m_item.ImageChanged(this, new ImageChangedEventArgs(MozItemState.Focus));
            }
            m_item.Invalidate();
          }
        }
      }

      [Browsable(false)]
      public Image SelectedImage
      {
        get
        {
          if (m_selectedImage != null)
          {
            return m_selectedImage;
          }

          if ((GetImageList() != null) && (m_selectedImageIndex != -1))
          {
            try
            {
              return GetImageList().Images[m_selectedImageIndex];
            }
            catch (Exception)
            {
              return null;
            }
          }
          else
          {
            return null;
          }
        }
        set
        {
          m_selectedImage = value;
          if (m_item.ImageChanged != null)
          {
            m_item.ImageChanged(this, new ImageChangedEventArgs(MozItemState.Selected));
          }
          m_item.Invalidate();
        }
      }

      [TypeConverter(typeof (ImageTypeConverter))]
      [Editor(typeof (ImageMapEditor), typeof (UITypeEditor))]
      [Description("Image for selected state.")]
      public int Selected
      {
        get { return m_selectedImageIndex; }
        set
        {
          if (value != m_selectedImageIndex)
          {
            m_selectedImageIndex = value;

            if (m_item.ImageChanged != null)
            {
              m_item.ImageChanged(this, new ImageChangedEventArgs(MozItemState.Selected));
            }
            m_item.Invalidate();
          }
        }
      }
    }

    #endregion

    #region ImageTypeConverter

    public class ImageTypeConverter : TypeConverter
    {
      public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
                                       Type destinationType)
      {
        if (value.ToString() == "-1")
        {
          return "(none)";
        }
        else
        {
          return value.ToString();
        }
      }

      public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
      {
        if (destinationType == typeof (string))
        {
          return true;
        }
        return base.CanConvertTo(context, destinationType);
      }

      public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
      {
        if (sourceType == typeof (string))
        {
          return true;
        }
        return base.CanConvertFrom(context, sourceType);
      }

      public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
      {
        if (value.GetType() == typeof (string))
        {
          // none = -1 = no image
          if ((value.ToString().ToUpper().IndexOf("NONE") >= 0) || (value.ToString() == ""))
          {
            return -1;
          }
          return
            Convert.ToInt16(value.ToString());
        }
        return base.ConvertFrom(context, culture, value);
      }
    }

    #endregion
  }

  #region ItemCollectionTypeConverter

  public class ItemCollectionTypeConverter : ExpandableObjectConverter
  {
    // override stuff here

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
                                     Type destinationType)
    {
      return "";
    }
  }

  #endregion

  #region Designer

  // Designer

  public class MozItemDesigner : ControlDesigner
  {
    public MozItemDesigner() {}

    protected override void OnPaintAdornments(PaintEventArgs pe)
    {
      base.OnPaintAdornments(pe);
      Control item = base.Control;
      Rectangle rect = item.ClientRectangle;
      ControlPaint.DrawBorder(pe.Graphics, rect, Color.Black, ButtonBorderStyle.Dashed);
    }


    public override bool CanBeParentedTo(IDesigner parentDesigner)
    {
      if (parentDesigner.Component is MozPane)
      {
        return true;
      }
      else
      {
        return false;
      }
    }

    public override SelectionRules SelectionRules
    {
      get
      {
        SelectionRules selectionRules = base.SelectionRules;
        selectionRules = SelectionRules.Visible;
        return selectionRules;
      }
    }

    protected override void PreFilterProperties(IDictionary properties)
    {
      base.PreFilterProperties(properties);

      properties.Remove("BackgroundImage");
      properties.Remove("RightToLeft");
      properties.Remove("Imemode");
    }

    protected override void PreFilterEvents(IDictionary events)
    {
      base.PreFilterEvents(events);
      events.Remove("ForeColorChanged");
      events.Remove("BackColorChanged");
      events.Remove("BorderStyleChanged");
    }
  }

  #endregion

  #region MozItemEventArgs

  public class MozItemEventArgs : EventArgs
  {
    #region Class Data

    /// <summary>
    /// The MozItem that generated the event
    /// </summary>
    private MozItem m_mozItem;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the MozItemEventArgs class with default settings
    /// </summary>
    public MozItemEventArgs()
    {
      m_mozItem = null;
    }


    public MozItemEventArgs(MozItem mozItem)
    {
      this.m_mozItem = mozItem;
    }

    #endregion

    #region Properties

    public MozItem MozItem
    {
      get { return this.m_mozItem; }
    }

    #endregion
  }

  #endregion

  #region MozItemClickEventArgs

  public class MozItemClickEventArgs : EventArgs
  {
    #region Class Data

    /// <summary>
    /// The MozItem that generated the event
    /// </summary>
    private MozItem m_mozItem;

    private MouseButtons m_button;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the MozItemEventArgs class with default settings
    /// </summary>
    public MozItemClickEventArgs()
    {
      m_mozItem = null;
      m_button = MouseButtons.Left;
    }

    public MozItemClickEventArgs(MozItem mozItem, MouseButtons button)
    {
      this.m_mozItem = mozItem;
      this.m_button = button;
    }

    #endregion

    #region Properties

    public MozItem MozItem
    {
      get { return this.m_mozItem; }
    }

    public MouseButtons Button
    {
      get { return this.m_button; }
    }

    #endregion
  }

  #endregion

  #region ImageChangedEventArgs

  public class ImageChangedEventArgs : EventArgs
  {
    #region Class Data

    /// <summary>
    /// The image that has changed
    /// </summary>
    private MozItemState m_image;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the MozItemEventArgs class with default settings
    /// </summary>
    public ImageChangedEventArgs()
    {
      m_image = 0;
    }


    public ImageChangedEventArgs(MozItemState image)
    {
      this.m_image = image;
    }

    #endregion

    #region Properties

    public MozItemState Image
    {
      get { return this.m_image; }
    }

    #endregion
  }

  #endregion
}