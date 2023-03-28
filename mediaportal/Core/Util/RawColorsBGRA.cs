using SharpDX.Mathematics.Interop;

namespace MediaPortal.Util
{
  public static class RawColorsBGRA
  {
    public static readonly RawColorBGRA Zero = new RawColorBGRA(0, 0, 0, 0);
    public static readonly RawColorBGRA Black = new RawColorBGRA(0, 0, 0, 255);
    public static readonly RawColorBGRA White = new RawColorBGRA(255, 255, 255, 255);

    public static RawColorBGRA FromARGB(long lColor)
    {
      return new RawColorBGRA((byte)lColor, (byte)(lColor >> 8), (byte)(lColor >> 16), (byte)(lColor >> 32));
    }

    public static RawColorBGRA FromColor(System.Drawing.Color color)
    {
      return new RawColorBGRA(color.B, color.G, color.R, color.A);
    }
  }
}
