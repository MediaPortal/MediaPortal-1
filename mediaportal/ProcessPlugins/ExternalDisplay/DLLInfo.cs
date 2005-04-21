using System.Runtime.InteropServices;

namespace ProcessPlugins.ExternalDisplay
{
  /// <summary>
  /// This structure is returned from the LCDHype drivers and contains important information about the display
  /// </summary>
  [StructLayout(LayoutKind.Sequential )]
  public struct DLLInfo
  {
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
    public char[] IDArray;             //Display description
    [MarshalAs(UnmanagedType.I1)]
    public bool SupportGfxLCD;         //does this driver support graphical LCDs?
    [MarshalAs(UnmanagedType.I1)]
    public bool SupportTxtLCD;         //does this driver support text
    [MarshalAs(UnmanagedType.I1)]
    public bool SupportLightSlider;    //does this driver support the light control slider
    [MarshalAs(UnmanagedType.I1)]
    public bool SupportContrastSlider; //does this driver support the contrast slider
    [MarshalAs(UnmanagedType.I1)]
    public bool SupportOutports;		   //does this driver support outports for controlling external circuits
    [MarshalAs(UnmanagedType.U1)]
    public byte CCharWidth;            //custom char width in pixels
    [MarshalAs(UnmanagedType.U1)]
    public byte CCharHeight;           //custom char height in pixels
    [MarshalAs(UnmanagedType.U1)]
    public byte FontPitch;             //fontpitch of LCD in pixels
  }
}
