using System.Runtime.InteropServices;

namespace MediaPortal.Player
{
  public enum eAudioDualMonoMode
  {
    STEREO = 0,
    LEFT_MONO = 1,
    RIGHT_MONO = 2,
    MIX_MONO = 3,
    UNSUPPORTED = 4
  } ;

  [ComVisible(true), ComImport,
   Guid("A575A6D8-6F52-4598-9507-6542EBB67677"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IMPAudioSwitcherFilter
  {
    [PreserveSig]
    int GetAudioDualMonoMode([Out] out uint mode);

    [PreserveSig]
    int SetAudioDualMonoMode([In] uint mode);
  }
}