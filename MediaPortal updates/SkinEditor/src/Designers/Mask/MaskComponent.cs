using System.Drawing;
using System.Windows.Forms;

namespace Mpe.Designers.Mask
{
  /// <summary>
  /// MaskComponent
  /// </summary>
  public abstract class MaskComponent : Control
  {
    protected MpeControlMask mask;
    protected Rectangle[] nodes;

    public MaskComponent(MpeControlMask mask)
    {
      this.mask = mask;
      SetStyle(ControlStyles.SupportsTransparentBackColor, true);
      SetStyle(ControlStyles.DoubleBuffer, true);
      SetStyle(ControlStyles.AllPaintingInWmPaint, true);
      SetStyle(ControlStyles.UserPaint, true);
      SetStyle(ControlStyles.ResizeRedraw, true);
      BackColor = Color.Transparent;
    }

    public abstract void Initialize();
  }
}