using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Mpe.Controls.Properties;

namespace Mpe.Controls
{
  /// <summary>
  /// This class will be used by the PropertiesControl to display information about
  /// the selected textureImage.
  /// </summary>
  [DefaultProperty("Name")]
  public class MpeImageViewer : MpeImage
  {
    #region Variables

    private MpeZoomLevel zoom;

    #endregion

    #region Constructors

    public MpeImageViewer() : base()
    {
      MpeLog.Debug("MpeImageViewer()");
      ZoomLevel = MpeZoomLevel.x1;
      controlLock.Size = true;
    }

    #endregion

    #region Properties

    [Browsable(false)]
    public virtual int ZoomFactor
    {
      get { return (int) ZoomLevel; }
    }

    [Browsable(false)]
    public override bool AutoSize
    {
      get { return base.AutoSize; }
      set { base.AutoSize = false; }
    }

    [Browsable(false)]
    public override MpeControlLock Locked
    {
      get { return base.Locked; }
      set { base.Locked = value; }
    }

    [Category("Image")]
    [ReadOnly(true)]
    public Size ImageSize
    {
      get { return textureImage.Size; }
    }

    [Browsable(false)]
    public new Size Size
    {
      get { return base.Size; }
      set { base.Size = value; }
    }

    [Browsable(false)]
    public new Point Location
    {
      get { return base.Location; }
      set { base.Location = value; }
    }

    [Browsable(false)]
    public override int Id
    {
      get { return base.Id; }
      set { base.Id = value; }
    }

    [Browsable(false)]
    public override string Description
    {
      get { return base.Description; }
      set { base.Description = value; }
    }

    [ReadOnly(true)]
    [Category("Image")]
    public override FileInfo Texture
    {
      get { return base.Texture; }
      set
      {
        base.Texture = value;
        ZoomLevel = zoom;
      }
    }

    [CategoryAttribute("Image")]
    [DescriptionAttribute("The horizontal and vertical resolution in pixels per inch.")]
    [ReadOnlyAttribute(true)]
    public Size Resolution
    {
      get { return new Size((int) textureImage.HorizontalResolution, (int) textureImage.VerticalResolution); }
    }

    [CategoryAttribute("Image")]
    [DescriptionAttribute("The color depth used by this textureImage.")]
    [ReadOnlyAttribute(true)]
    public PixelFormat PixelFormat
    {
      get { return textureImage.PixelFormat; }
    }

    [CategoryAttribute("Designer")]
    [RefreshPropertiesAttribute(RefreshProperties.Repaint)]
    [DescriptionAttribute("Zoom in on the image to see fine details.")]
    [DefaultValueAttribute(true)]
    public MpeZoomLevel ZoomLevel
    {
      get { return zoom; }
      set
      {
        if (textureImage != null)
        {
          Size = new Size(ImageSize.Width*(int) value, ImageSize.Height*(int) value);
          Invalidate(false);
        }
        zoom = value;
      }
    }

    #endregion

    #region Hidden Properties

    [Browsable(false)]
    public override MpeTagCollection Tags
    {
      get { return base.Tags; }
      set { base.Tags = value; }
    }

    [Browsable(false)]
    public override MpeControlType Type
    {
      get { return base.Type; }
      set { base.Type = value; }
    }

    [Browsable(false)]
    public override int OnLeft
    {
      get { return base.OnLeft; }
      set { base.OnLeft = value; }
    }

    [Browsable(false)]
    public override int OnRight
    {
      get { return base.OnRight; }
      set { base.OnRight = value; }
    }

    [Browsable(false)]
    public override int OnUp
    {
      get { return base.OnUp; }
      set { base.OnUp = value; }
    }

    [Browsable(false)]
    public override int OnDown
    {
      get { return base.OnDown; }
      set { base.OnDown = value; }
    }

    [Browsable(false)]
    public override bool Visible
    {
      get { return base.Visible; }
      set { base.Visible = value; }
    }

    [Browsable(false)]
    public override Color DiffuseColor
    {
      get { return base.DiffuseColor; }
      set { base.DiffuseColor = value; }
    }

    [Browsable(false)]
    public override int ColorKey
    {
      get { return base.ColorKey; }
      set { base.ColorKey = value; }
    }

    [Browsable(false)]
    public override bool Centered
    {
      get { return base.Centered; }
      set { base.Centered = value; }
    }

    [Browsable(false)]
    public override bool Filtered
    {
      get { return base.Filtered; }
      set { base.Filtered = value; }
    }

    [Browsable(false)]
    public override bool KeepAspectRatio
    {
      get { return base.KeepAspectRatio; }
      set { base.KeepAspectRatio = value; }
    }

    [Browsable(false)]
    public override MpeControlPadding Padding
    {
      get { return base.Padding; }
      set { base.Padding = value; }
    }

    #endregion

    #region Event Handlers

    protected override void OnPaint(PaintEventArgs e)
    {
      if (textureImage == null)
      {
        e.Graphics.DrawRectangle(borderPen, 1, 1, Width - 4, Width - 4);
      }
      else
      {
        e.Graphics.DrawImage(textureImage, 0, 0, textureImage.Width*(int) ZoomLevel, textureImage.Height*(int) ZoomLevel);
      }
    }

    protected override void OnLockChanged(MpeControlLockType type, bool value)
    {
      controlLock.Size = true;
    }

    #endregion
  }
}