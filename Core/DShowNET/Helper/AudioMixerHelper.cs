#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Runtime.InteropServices;

namespace DShowNET.AudioMixer
{
  /// <summary>
  /// <para>
  /// Code copied from
  /// </para>
  /// <para>
  /// http://www.dotnetboards.com/viewtopic.php?t=5859&amp;highlight=mixer
  /// </para>
  /// <para>
  /// Special thanks to arkam.
  /// </para>
  /// <para>
  /// Modified a little bit. Fixed memory leaks (Marshal.AllocCoTaskMem without
  /// Marshal.FreeCoTaskMem).
  /// </para>
  /// </summary>
  public class AudioMixerHelper
  {
    public class AudioMixerException : Exception
    {
      public AudioMixerException(string message) :
        base(message)
      {
      }
    }

    /// <summary>
    /// Defined in Mmsystem.h
    /// </summary>
    public const int MMSYSERR_NOERROR = 0;

    /// <summary>
    /// Defined in Mmsystem.h
    /// </summary>
    private const int MIXERR_BASE = 1024;

    /// <summary>
    /// Defined in Mmsystem.h
    /// </summary>
    public const int MIXERR_INVALCONTROL = MIXERR_BASE + 1;

    private const int MMSYSERR_BASE = 0;

    /// <summary>
    /// Defined in Mmsystem.h
    /// </summary>
    public const int MMSYSERR_BADDEVICEID = MMSYSERR_BASE + 2; /* device ID out of range */

    /// <summary>
    /// Defined in Mmsystem.h
    /// </summary>
    public const int MMSYSERR_INVALFLAG = MMSYSERR_BASE + 10; /* invalid flag passed */

    /// <summary>
    /// Defined in Mmsystem.h
    /// </summary>
    public const int MMSYSERR_INVALHANDLE = MMSYSERR_BASE + 5; /* device handle is invalid */

    /// <summary>
    /// Defined in Mmsystem.h
    /// </summary>
    public const int MMSYSERR_INVALPARAM = MMSYSERR_BASE + 11; /* invalid parameter passed */

    /// <summary>
    /// Defined in Mmsystem.h
    /// </summary>
    public const int MMSYSERR_NODRIVER = MMSYSERR_BASE + 6; /* no device driver present */

    /// <summary>
    /// Defined in Mmsystem.h
    /// </summary>
    public const int MMSYSERR_ALLOCATED = MMSYSERR_BASE + 4; /* device already allocated */

    /// <summary>
    /// Defined in Mmsystem.h
    /// </summary>
    public const int MMSYSERR_NOMEM = MMSYSERR_BASE + 7; /* memory allocation error */

    /// <summary>
    /// Defined in Mmsystem.h
    /// </summary>
    public const int MAXPNAMELEN = 32;

    /// <summary>
    /// 
    /// </summary>
    public const int MIXER_LONG_NAME_CHARS = 64;

    /// <summary>
    /// 
    /// </summary>
    public const int MIXER_SHORT_NAME_CHARS = 16;

    /// <summary>
    /// 
    /// </summary>
    public const int MIXER_GETLINEINFOF_COMPONENTTYPE = 0x3;

    /// <summary>
    /// 
    /// </summary>
    public const int MIXER_GETCONTROLDETAILSF_VALUE = 0x0;

    /// <summary>
    /// 
    /// </summary>
    public const int MIXER_GETLINECONTROLSF_ONEBYTYPE = 0x2;

    /// <summary>
    /// 
    /// </summary>
    public const int MIXER_SETCONTROLDETAILSF_VALUE = 0x0;

    /// <summary>
    /// 
    /// </summary>
    public const int MIXERLINE_COMPONENTTYPE_DST_FIRST = 0x0;

    /// <summary>
    /// 
    /// </summary>
    public const int MIXERLINE_COMPONENTTYPE_SRC_FIRST = 0x1000;

    /// <summary>
    /// 
    /// </summary>
    public const int MIXERLINE_COMPONENTTYPE_DST_SPEAKERS =
      (MIXERLINE_COMPONENTTYPE_DST_FIRST + 4);

    /// <summary>
    /// 
    /// </summary>
    public const int MIXERLINE_COMPONENTTYPE_SRC_MICROPHONE =
      (MIXERLINE_COMPONENTTYPE_SRC_FIRST + 3);

    /// <summary>
    /// 
    /// </summary>
    public const int MIXERLINE_COMPONENTTYPE_SRC_LINE =
      (MIXERLINE_COMPONENTTYPE_SRC_FIRST + 2);

    /// <summary>
    /// 
    /// </summary>
    public const int MIXERCONTROL_CT_CLASS_FADER = 0x50000000;

    /// <summary>
    /// 
    /// </summary>
    public const int MIXERCONTROL_CT_UNITS_UNSIGNED = 0x30000;

    /// <summary>
    /// 
    /// </summary>
    public const int MIXERCONTROL_CONTROLTYPE_FADER =
      (MIXERCONTROL_CT_CLASS_FADER | MIXERCONTROL_CT_UNITS_UNSIGNED);

    /// <summary>
    /// 
    /// </summary>
    public const int MIXERCONTROL_CONTROLTYPE_VOLUME =
      (MIXERCONTROL_CONTROLTYPE_FADER + 1);

    [DllImport("winmm.dll", CharSet = CharSet.Ansi)]
    private static extern int mixerClose(int hmx);

    [DllImport("winmm.dll", CharSet = CharSet.Ansi)]
    private static extern int mixerGetControlDetailsA(int hmxobj, ref
                                                                    MIXERCONTROLDETAILS pmxcd, int fdwDetails);

    [DllImport("winmm.dll", CharSet = CharSet.Ansi)]
    private static extern int mixerGetDevCapsA(int uMxId, MIXERCAPS
                                                            pmxcaps, int cbmxcaps);

    [DllImport("winmm.dll", CharSet = CharSet.Ansi)]
    private static extern int mixerGetID(int hmxobj, int pumxID, int
                                                                   fdwId);

    [DllImport("winmm.dll", CharSet = CharSet.Ansi)]
    private static extern int mixerGetLineControlsA(int hmxobj, ref
                                                                  MIXERLINECONTROLS pmxlc, int fdwControls);

    [DllImport("winmm.dll", CharSet = CharSet.Ansi)]
    private static extern int mixerGetLineInfoA(int hmxobj, ref
                                                              MIXERLINE pmxl, int fdwInfo);

    [DllImport("winmm.dll", CharSet = CharSet.Ansi)]
    private static extern int mixerGetNumDevs();

    [DllImport("winmm.dll", CharSet = CharSet.Ansi)]
    private static extern int mixerMessage(int hmx, int uMsg, int
                                                                dwParam1, int dwParam2);

    [DllImport("winmm.dll", CharSet = CharSet.Ansi)]
    private static extern int mixerOpen(out int phmx, int uMxId,
                                        int dwCallback, int dwInstance, int fdwOpen);

    [DllImport("winmm.dll", CharSet = CharSet.Ansi)]
    private static extern int mixerSetControlDetails(int hmxobj, ref
                                                                   MIXERCONTROLDETAILS pmxcd, int fdwDetails);

    /// <summary>
    /// 
    /// </summary>
    public struct MIXERCAPS
    {
      public int wMid; // manufacturer id 
      public int wPid; // product id 
      public int vDriverVersion; // version of the driver 
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAXPNAMELEN)] public string szPname; // product name 
      public int fdwSupport; // misc. support bits 
      public int cDestinations; // count of destinations 
    }

    /// <summary>
    /// 
    /// </summary>
    public struct MIXERCONTROL
    {
      public int cbStruct; // size in Byte of MIXERCONTROL 
      public int dwControlID; // unique control id for mixer device 
      public int dwControlType; // MIXERCONTROL_CONTROLpublic enum _xxx 
      public int fdwControl; // MIXERCONTROL_CONTROLF_xxx 
      public int cMultipleItems; // if MIXERCONTROL_CONTROLF_MULTIPLE 

      [MarshalAs(UnmanagedType.ByValTStr,
        SizeConst = MIXER_SHORT_NAME_CHARS)] public string szShortName; // short name of 

      [MarshalAs(UnmanagedType.ByValTStr,
        SizeConst = MIXER_LONG_NAME_CHARS)] public string szName; // long name of 

      public int lMinimum; // Minimum value 
      public int lMaximum; // Maximum value 
      //[ MarshalAs( UnmanagedType.ByValArray, SizeConst=10 )] 
      [MarshalAs(UnmanagedType.U4, SizeConst = 10)] public int reserved; // replaced // reserved structure space 
    }

    /// <summary>
    /// 
    /// </summary>
    public struct MIXERCONTROLDETAILS
    {
      public int cbStruct; // size in Byte of MIXERCONTROLDETAILS 
      public int dwControlID; // control id to get/set details on 
      public int cChannels; // number of channels in paDetails array 
      public int item; // hwndOwner or cMultipleItems 
      public int cbDetails; // size of _one_ details_XX struct 
      public IntPtr paDetails; // pointer to array of details_XX structs 
    }

    /// <summary>
    /// 
    /// </summary>
    public struct MIXERCONTROLDETAILS_UNSIGNED
    {
      public int dwValue; // value of the control 
    }

    /// <summary>
    /// 
    /// </summary>
    public struct MIXERLINE
    {
      public int cbStruct; // size of MIXERLINE structure 
      public int dwDestination; // zero based destination index 
      public int dwSource; // zero based source index (if source) 
      public int dwLineID; // unique line id for mixer device 
      public int fdwLine; // state/information about line 
      public int dwUser; // driver specific information 
      public int dwComponentType; // component public enum line connects to 
      public int cChannels; // number of channels line supports 
      public int cConnections; // number of connections (possible) 
      public int cControls;

      [MarshalAs(UnmanagedType.ByValTStr,
        SizeConst = MIXER_SHORT_NAME_CHARS)] // number of controls at this line 
        public string szShortName;

      [MarshalAs(UnmanagedType.ByValTStr,
        SizeConst = MIXER_LONG_NAME_CHARS)] public string szName;

      public int dwType;
      public int dwDeviceID;
      public int wMid;
      public int wPid;
      public int vDriverVersion;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAXPNAMELEN)] public string szPname;
    }

    /// <summary>
    /// 
    /// </summary>
    public struct MIXERLINECONTROLS
    {
      public int cbStruct; // size in Byte of MIXERLINECONTROLS 
      public int dwLineID; // line id (from MIXERLINE.dwLineID) 
      // MIXER_GETLINECONTROLSF_ONEBYID or 
      public int dwControl; // MIXER_GETLINECONTROLSF_ONEBYpublic enum 
      public int cControls; // count of controls pmxctrl points to 
      public int cbmxctrl; // size in Byte of _one_ MIXERCONTROL 
      public IntPtr pamxctrl; // pointer to first MIXERCONTROL array 
    }

    private static void CheckErr(int errCode)
    {
      switch (errCode)
      {
        case MMSYSERR_NOERROR:
          break;
        case MIXERR_INVALCONTROL:
          throw new AudioMixerException("The control reference is invalid.");
        case MMSYSERR_BADDEVICEID:
          throw new AudioMixerException("The hmxobj parameter specifies an invalid device identifier.");
        case MMSYSERR_INVALFLAG:
          throw new AudioMixerException("One or more flags are invalid.");
        case MMSYSERR_INVALHANDLE:
          throw new AudioMixerException("The hmxobj parameter specifies an invalid handle.");
        case MMSYSERR_INVALPARAM:
          throw new AudioMixerException("One or more parameters are invalid.");
        case MMSYSERR_NODRIVER:
          throw new AudioMixerException("No mixer device is available for the object specified by hmxobj.");
        case MMSYSERR_ALLOCATED:
          throw new AudioMixerException(
            "The specified resource is already allocated by the maximum number of clients possible.");
        case MMSYSERR_NOMEM:
          throw new AudioMixerException("Unable to allocate resources.");
        default:
          throw new AudioMixerException("Audio Mixer unknown error.");
      }
    }

    private static void GetVolumeControl(int hmixer, int componentType,
                                         int ctrlType, out MIXERCONTROL mxc, out int vCurrentVol)
    {
      // This function attempts to obtain a mixer control. 
      // Returns True if successful. 
      MIXERLINECONTROLS mxlc = new MIXERLINECONTROLS();
      MIXERLINE mxl = new MIXERLINE();
      MIXERCONTROLDETAILS pmxcd = new MIXERCONTROLDETAILS();
      MIXERCONTROLDETAILS_UNSIGNED du = new
        MIXERCONTROLDETAILS_UNSIGNED();
      mxc = new MIXERCONTROL();

      vCurrentVol = -1;

      //mxl.szShortName = new string(' ', MIXER_SHORT_NAME_CHARS); 
      //mxl.szName = new string(' ', MIXER_LONG_NAME_CHARS); 
      mxl.cbStruct = Marshal.SizeOf(mxl);
      mxl.dwComponentType = componentType;

      // Obtain a line corresponding to the component public enum 
      CheckErr(mixerGetLineInfoA(hmixer, ref mxl,
                                 MIXER_GETLINEINFOF_COMPONENTTYPE));

      int sizeofMIXERCONTROL = 152;
      //Marshal.SizeOf(typeof(MIXERCONTROL)) 
      int ctrl = Marshal.SizeOf(typeof (MIXERCONTROL));
      mxlc.pamxctrl = Marshal.AllocCoTaskMem(sizeofMIXERCONTROL); //new MIXERCONTROL(); 
      mxlc.cbStruct = Marshal.SizeOf(mxlc);
      mxlc.dwLineID = mxl.dwLineID;
      mxlc.dwControl = ctrlType;
      mxlc.cControls = 1;
      mxlc.cbmxctrl = sizeofMIXERCONTROL;

      // Allocate a buffer for the control 
      mxc.cbStruct = sizeofMIXERCONTROL;

      // Get the control 
      try
      {
        CheckErr(mixerGetLineControlsA(hmixer, ref mxlc,
                                       MIXER_GETLINECONTROLSF_ONEBYTYPE));
        // Copy the control into the destination structure 
        mxc = (MIXERCONTROL) Marshal.PtrToStructure(mxlc.pamxctrl, typeof (MIXERCONTROL));
      }
      catch (Exception e)
      {
        throw e;
      }
      finally
      {
        Marshal.FreeCoTaskMem(mxlc.pamxctrl);
      }
      int sizeofMIXERCONTROLDETAILS =
        Marshal.SizeOf(typeof (MIXERCONTROLDETAILS));
      int sizeofMIXERCONTROLDETAILS_UNSIGNED =
        Marshal.SizeOf(typeof (MIXERCONTROLDETAILS_UNSIGNED));
      pmxcd.cbStruct = sizeofMIXERCONTROLDETAILS;
      pmxcd.dwControlID = mxc.dwControlID;
      pmxcd.paDetails =
        Marshal.AllocCoTaskMem(sizeofMIXERCONTROLDETAILS_UNSIGNED);
      pmxcd.cChannels = 1;
      pmxcd.item = 0;
      pmxcd.cbDetails = sizeofMIXERCONTROLDETAILS_UNSIGNED;

      try
      {
        CheckErr(mixerGetControlDetailsA(hmixer, ref pmxcd,
                                         MIXER_GETCONTROLDETAILSF_VALUE));
        du =
          (MIXERCONTROLDETAILS_UNSIGNED) Marshal.PtrToStructure(pmxcd.paDetails, typeof (MIXERCONTROLDETAILS_UNSIGNED));
      }
      catch (Exception e)
      {
        throw e;
      }
      finally
      {
        Marshal.FreeCoTaskMem(pmxcd.paDetails);
      }
      vCurrentVol = du.dwValue;
    }

    private static void SetVolumeControl(int hmixer, MIXERCONTROL mxc,
                                         int volume)
    {
      // This function sets the value for a volume control. 
      // Returns True if successful 
      MIXERCONTROLDETAILS mxcd = new MIXERCONTROLDETAILS();
      MIXERCONTROLDETAILS_UNSIGNED vol = new
        MIXERCONTROLDETAILS_UNSIGNED();

      mxcd.item = 0;
      mxcd.dwControlID = mxc.dwControlID;
      mxcd.cbStruct = Marshal.SizeOf(mxcd);
      mxcd.cbDetails = Marshal.SizeOf(vol);

      // Allocate a buffer for the control value buffer 
      mxcd.cChannels = 1;
      vol.dwValue = volume;

      // Copy the data into the control value buffer 
      //mxcd.paDetails = vol;
      //(MIXERCONTROL)Marshal.PtrToStructure(mxlc.pamxctrl,typeof(MIXERCONTROL)); 
      mxcd.paDetails = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof (MIXERCONTROLDETAILS_UNSIGNED)));
      Marshal.StructureToPtr(vol, mxcd.paDetails, false);

      // Set the control value 
      try
      {
        CheckErr(mixerSetControlDetails(hmixer, ref mxcd,
                                        MIXER_SETCONTROLDETAILSF_VALUE));
      }
      catch (Exception e)
      {
        throw e;
      }
      finally
      {
        Marshal.FreeCoTaskMem(mxcd.paDetails);
      }
    }

    /// <summary>
    /// Get the volume of sound card master channel.
    /// </summary>
    /// <returns>Volume from 0-65535</returns>
    /// <remarks>The value has been normalized to a number between 0 to 65535.
    /// </remarks>
    public static int GetVolume()
    {
      int mixer;
      MIXERCONTROL volCtrl = new MIXERCONTROL();
      int currentVol;
      CheckErr(mixerOpen(out mixer, 0, 0, 0, 0));
      try
      {
        int type = MIXERCONTROL_CONTROLTYPE_VOLUME;
        GetVolumeControl(mixer,
                         MIXERLINE_COMPONENTTYPE_DST_SPEAKERS, type, out volCtrl, out
                                                                                    currentVol);
      }
      catch (Exception ex)
      {
        throw ex;
      }
      finally
      {
        CheckErr(mixerClose(mixer));
      }
      int diff = volCtrl.lMaximum - volCtrl.lMinimum;
      if (diff > 0)
      {
        long normalized = 65535L*(long) (currentVol - volCtrl.lMinimum)/diff;
        currentVol = (int) normalized;
      }
      return currentVol;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="vVolume"></param>
    public static void SetVolume(int vVolume)
    {
      int mixer;
      MIXERCONTROL volCtrl = new MIXERCONTROL();
      int currentVol;
      CheckErr(mixerOpen(out mixer, 0, 0, 0, 0));
      try
      {
        int type = MIXERCONTROL_CONTROLTYPE_VOLUME;
        GetVolumeControl(mixer,
                         MIXERLINE_COMPONENTTYPE_DST_SPEAKERS, type, out volCtrl, out
                                                                                    currentVol);

        int diff = volCtrl.lMaximum - volCtrl.lMinimum;
        if (diff > 0)
        {
          long normalized = (long) vVolume*(long) diff/65535L;
          vVolume = (int) normalized + volCtrl.lMinimum;
        }

        if (vVolume > volCtrl.lMaximum)
        {
          vVolume = volCtrl.lMaximum;
        }
        if (vVolume < volCtrl.lMinimum)
        {
          vVolume = volCtrl.lMinimum;
        }
        SetVolumeControl(mixer, volCtrl, vVolume);
      }
      catch (Exception ex)
      {
        throw ex;
      }
      finally
      {
        CheckErr(mixerClose(mixer));
      }
    }
  }
}