using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using DirectShowLib;
namespace DShowNET
{

  [StructLayout(LayoutKind.Sequential), ComVisible(false)]
  public struct MPEG2VideoInfo		//  MPEG2VideoInfo
  {
    public VideoInfoHeader2 hdr;
    public UInt32 dwStartTimeCode;
    public UInt32 cbSequenceHeader;
    public UInt32 dwProfile;
    public UInt32 dwLevel;
    public UInt32 dwFlags;
    public UInt32 dwSequenceHeader;

  }
}
