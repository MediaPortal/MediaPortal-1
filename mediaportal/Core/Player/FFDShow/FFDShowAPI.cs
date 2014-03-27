#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Collections;
using Microsoft.Win32;
using System.IO;
using System.Runtime.Remoting.Messaging;
using FFDShow.Interfaces;
using MediaPortal.GUI.Library;
using DShowNET.Helper;

namespace FFDShow
{
  /// <summary>
  /// FFDShowAPI library class. Use this class to get/set FFDShow live settings
  /// </summary>
  public class FFDShowAPI : IDisposable
  {
    #region Guids

    public static readonly Guid FFDShowVideoGuid = new Guid("04FE9017-F873-410E-871E-AB91661A4EF7");
    public static readonly Guid FFDShowVideoDXVAGuid = new Guid("0B0EFF97-C750-462C-9488-B10E7D87F1A6");
    public static readonly Guid FFDShowVideoRawGuid = new Guid("0B390488-D80F-4A68-8408-48DC199F0E97");

    #endregion

    #region Constants

    /// <summary>
    /// Identifier of Window messages structure that can carry string for interprocess communication
    /// </summary>
    public const int WM_COPYDATA = 0x004A;

    /// <summary>
    /// List of commands understood by FFDShow remote API
    /// These are commands that concern integers transmission (get or set) or 
    /// single commands such as "Pause video"
    /// </summary>
    private enum FFD_WPRM : int
    {
      SET_PARAM_NAME = 0,
      SET_PARAM_VALUE_INT = 1,
      GET_PARAM_NAME = 2,
      GET_PARAM_VALUE_INT = 3,
      PAUSE_VIDEO = 4,
      RESUME_VIDEO = 5,
      GET_STATE = 6,
      GET_DURATION = 7,
      GET_CUR_TIME = 8,
      SET_PARAM_VALUE_STR = 9,
      //GET_PARAM_VALUE_STR = 13,
      SET_CURTIME = 13,
      SET_ADDTOROT = 14,
      FASTFORWARD = 15,
      FASTREWIND = 16,
      GETFASTFORWARDSPEED = 17,
      CAPTUREIMAGE = 18,
      SET_OSD_POSX = 19,
      SET_OSD_POSY = 20,
      //SET_STREAM = 21,
      //GET_CURRENT_AUDIO_STREAM = 22,
      //GET_CURRENT_SUBTITLE_STREAM = 23,
      GET_FRAMERATE = 24,
      SET_AUDIO_STREAM = 25,
      SET_SUBTITLE_STREAM = 26,
      SET_FFRW_NO_OSD = 27
    }

    /// <summary>
    /// List of commands understood by FFDShow remote API.
    /// These are commands that require strings transmissions
    /// </summary>
#pragma warning disable 1591
    public enum FFD_MSG
    {
      GET_PARAMSTR = 19,
      GET_CURRENT_SUBTITLES = 20,
      GET_PRESETLIST = 21,
      GET_SOURCEFILE = 22,
      GET_SUBTITLEFILESLIST = 23,
      GET_CHAPTERSLIST = 25,
      GET_AUDIOSTREAMSLIST = 300,
      GET_SUBTITLESTREAMSLIST = 301
    }
#pragma warning restore 1591

    //Copy data flags
    private const int FFDSM_SET_ACTIVE_PRESET_STR = 10;
    private const int FFDSM_SET_SHORTOSD_MSG = 18;
    private const int FFDSM_SET_OSD_MSG = 19;


    /// <summary>
    /// Playing state
    /// </summary>
    public enum PlayState : int
    {
      /// <summary>
      /// Stop state
      /// </summary>
      StopState = 0,
      /// <summary>
      /// Pause state
      /// </summary>
      PauseState = 1,
      /// <summary>
      /// Play state
      /// </summary>
      PlayState = 2,
      /// <summary>
      /// Fast forwarding or rewinding state
      /// </summary>
      FastForwardRewind = 3
    } ;

    /// <summary>
    /// Running object table registration
    /// </summary>
    public enum ROTRegistration : int
    {
      /// <summary>
      /// Register the DirectShow graph in the running object table (graph can be read and modified after that)
      /// Don't forget to unregister it.
      /// </summary>
      RegisterToRot = 1,
      /// <summary>
      /// Unregister the DirectShow graph from the running object table
      /// </summary>
      UnregisterToRot = 0
    } ;

    /// <summary>
    /// File name mode
    /// </summary>
    public enum FileNameMode : int
    {
      /// <summary>
      /// Full path (including drive and directories)
      /// </summary>
      FullPath,
      /// <summary>
      /// File name with extension but without the directory path
      /// </summary>
      FileName,
      /// <summary>
      /// File name without extension and without the directory path
      /// </summary>
      FileNameWithoutExtension,
      /// <summary>
      /// File name without extension and without the directory path and with suffixe only
      /// </summary>
      FileNameLanguage
    }

    private const int FALSE = 0;
    private const int TRUE = 1;

    public const int minRevision = 3640;

    #endregion Constants

    #region Structures

    /// <summary>
    /// FFDShow instance structure
    /// </summary>
    public struct FFDShowInstance
    {
      /// <summary>
      /// Unique identifier for this FFDShow instance
      /// </summary>
      public int handle;

      /// <summary>
      /// File name of the media being played by this instance (may be null)
      /// </summary>
      public string fileName;
    }

    #endregion Structures

    #region Variables

    private static string ffdshowRegKey = @"SOFTWARE\GNU\ffdshow";
    private static string ffdshowDXVARegKey = @"SOFTWARE\GNU\ffdshow_dxva";
    private static string ffdshowAudioRegKey = @"SOFTWARE\GNU\ffdshow_audio";

    private uint FFDShowAPIRemoteId = 32786;

    /// <summary>
    /// Unique identifier of the running instance of FFDShow
    /// </summary>
    protected int ffDShowInstanceHandle = 0;

    private int requestTimeout = 2000;
    private FFDShowReceiver receiver = null;
    private bool IsFFDShowActive = false;
    private static string strAppName = "ffdshow_remote_class";
    private string fileName = null;
    private FileNameMode fileNameMode = FileNameMode.FullPath;
    private int initFFDShowInstanceHandle = 0;
    private static bool ffrwNoOSD = false;
    private IffdshowDec ffdshowDec = null;
    private IffDecoder ffDecoder = null;
    private IffdshowBase ffdshowBase = null;
    private IffdshowDecVideo ffdshowDecVideo = null;
    private IAMStreamSelect streamSelect = null;

    public enum FFDShowAPIMode
    {
      DirectShowMode,
      InterProcessMode
    } ;

    private FFDShowAPIMode ffdshowAPIMode = FFDShowAPIMode.InterProcessMode;

    //private AutoResetEvent resetEvent = new AutoResetEvent(false);

    #endregion Variables

    #region Helper methods

    public static string getFileName(string fileName, FileNameMode mode)
    {
      FileInfo fileInfo = new FileInfo(fileName);
      switch (mode)
      {
        case FileNameMode.FullPath:
          return fileInfo.FullName;
        case FileNameMode.FileName:
          return fileInfo.Name;
        case FileNameMode.FileNameWithoutExtension:
          string formattedFileName = fileInfo.Name;
          if (formattedFileName.LastIndexOf('.') != -1)
            formattedFileName = formattedFileName.Substring(0, formattedFileName.LastIndexOf('.'));
          return formattedFileName;
        case FileNameMode.FileNameLanguage:
          string formattedFileNameLanguage = fileInfo.Name;
          //string[] words = formattedFileNameLanguage.Split(new char[] { '.', '-', '_' });
          string[] words = formattedFileNameLanguage.Split(new char[] {'.', '_'});
          if (formattedFileNameLanguage.LastIndexOf('.') != -1)
            formattedFileNameLanguage = words[words.Length - 2];
          //Log.Error("FFDSHOWAPI(getFileName): formattedFileNameLanguage \"{0}\"", formattedFileNameLanguage);
          return formattedFileNameLanguage;
      }
      return null;
    }

    #endregion

    #region WIN32 Class

#pragma warning disable 1591
    /// <summary>
    /// Win32 COM methods and constants
    /// </summary>
    public class Win32
    {
      // The CopyData Constant for SendMessage
      public const Int32 WM_COPYDATA = 0x004A;
      public const Int32 WM_KEYDOWN = 0x0100;
      public const Int32 WM_APPCOMMAND = 0x0319;
      public const Int32 WM_INPUT = 0x00FF;

      [StructLayout(LayoutKind.Sequential)]
      public struct COPYDATASTRUCT
      {
        internal UIntPtr dwData;
        internal uint cbData;
        internal IntPtr lpData;
      }

      // Import the SendMessage function for use with COPYDATASTRUCT
      [DllImport("User32.Dll")]
      public static extern IntPtr SendMessage(IntPtr hwnd, Int32 msg, Int32 hwndFrom, ref COPYDATASTRUCT cds);

      [DllImport("User32.Dll")]
      public static extern IntPtr SendMessage(IntPtr hwnd, Int32 msg, Int32 hwndFrom, IntPtr cds);

      [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
      public static extern IntPtr SendMessageTimeout(
        IntPtr windowHandle,
        int Msg,
        IntPtr wParam,
        ref COPYDATASTRUCT cds,
        SendMessageTimeoutFlags flags,
        int timeout,
        out IntPtr result);

      [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
      public static extern IntPtr SendMessageTimeout(
        IntPtr windowHandle,
        [MarshalAs(UnmanagedType.U4)] int Msg,
        IntPtr wParam,
        IntPtr lParam,
        SendMessageTimeoutFlags flags,
        int timeout,
        out IntPtr result);

      public enum SendMessageTimeoutFlags
      {
        SMTO_NORMAL = 0x0000,
        SMTO_BLOCK = 0x0001,
        SMTO_ABORTIFHUNG = 0x0002,
        SMTO_NOTIMEOUTIFNOTHUNG = 0x0008
      }


      [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
      public static extern IntPtr PostMessage(IntPtr hwnd, Int32 msg, Int32 hwndFrom, ref COPYDATASTRUCT cds);

      // Use COM interop to call the Win32 API GetLocalInfo.
      [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
      public static extern int GetLocaleInfo(
        // The locale identifier.
        int Locale,
        // The information type.
        int LCType,
        // The buffer size.
        [In, MarshalAs(UnmanagedType.LPWStr)] string lpLCData, int cchData
        );


      // Import the GlobalSize function
      [DllImport("kernel32.dll")]
      public static extern Int32 GlobalSize(IntPtr hmem);

      public const int WM_SYSCOMMAND = 0x0112;
      public const int SC_CLOSE = 0xF060;

      [DllImport("User32.Dll")]
      public static extern int FindWindow(string lpClassName,
                                          string lpWindowName);

      [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
      public static extern IntPtr SendMessage(int hWnd, uint Msg,
                                              int wParam, int lParam);

      [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
      public static extern IntPtr PostMessage(int hWnd, uint Msg,
                                              int wParam, int lParam);

      public delegate bool CallBack(int hwnd, IntPtr lParam);

      [DllImport("User32.Dll")]
      public static extern int EnumWindows(CallBack x, IntPtr y);

      [DllImport("User32.Dll")]
      public static extern void GetWindowText(int h, StringBuilder s, int nMaxCount);

      [DllImport("User32.Dll")]
      public static extern void GetClassName(int h, StringBuilder s, int nMaxCount);

      [DllImport("User32.Dll")]
      public static extern int IsWindow(int hwnd);
    }
#pragma warning restore 1591

    #endregion WIN32 Class

    #region Base Properties

    /// <summary>
    /// Gets the FFDShow instance handle (number that identifies the FFDShow instance)
    /// </summary>
    public int FFDShowInstanceHandle
    {
      get { return ffDShowInstanceHandle; }
    }

    /// <summary>
    /// Gets or sets the FFDShow remote identifier
    /// </summary>
    public uint FFDShowAPIRemote
    {
      get { return FFDShowAPIRemoteId; }
      set { FFDShowAPIRemoteId = value; }
    }

    /// <summary>
    /// Gets or sets the FFDShow registry key. Used sometimes when ffdshow is not active (for presets)
    /// </summary>
    public static string FFDShowRegKey
    {
      get { return ffdshowRegKey; }
      set { ffdshowRegKey = value; }
    }

    /// <summary>
    /// Gets or sets the registry key of FFDShow audio
    /// Used to get or set the default audio preset
    /// </summary>
    public static string FFDShowAudioRegKey
    {
      get { return ffdshowAudioRegKey; }
      set { ffdshowAudioRegKey = value; }
    }

    private static int osdX = 0;
    private static int osdY = 10;
    private bool updateOSD = false;

    /// <summary>
    /// Horizontal OSD position
    /// </summary>
    public static int OSDX
    {
      get { return osdX; }
      set { osdX = value; }
    }

    /// <summary>
    /// Vertical OSD position
    /// </summary>
    public static int OSDY
    {
      get { return osdY; }
      set { osdY = value; }
    }

    /// <summary>
    /// Gets or sets the OSD display when doing FastForward/Rewind
    /// This is a static parameter that will be applied to all the running FFDShow instances
    /// </summary>
    public static bool FFRWNoOSD
    {
      get { return ffrwNoOSD; }
      set { ffrwNoOSD = value; }
    }

    #endregion Base Properties

    #region Presets properties

    /// <summary>
    /// Gets or sets the default video preset (does not apply to currently running instances)
    /// </summary>
    public static String DefaultVideoPreset
    {
      get
      {
        using (RegistryKey preferencesKey = Registry.CurrentUser.OpenSubKey(ffdshowRegKey))
        {
          if (preferencesKey != null)
          {
            return (string)preferencesKey.GetValue("activePreset");
          }
          else return null;
        }
      }

      set
      {
        string[] presetList = VideoPresets;
        // Check if we set an existing preset
        bool found = false;
        for (int i = 0; i < presetList.Length; i++)
        {
          if (presetList[i].Equals(value))
          {
            found = true;
            break;
          }
        }
        if (found)
          using (RegistryKey preferencesKey = Registry.CurrentUser.CreateSubKey(ffdshowRegKey))
          {
            if (preferencesKey != null)
            {
              preferencesKey.SetValue("activePreset", value);
            }
          }
      }
    }

    /// <summary>
    /// Gets or sets the default audio preset (does not apply to currently running instances)
    /// </summary>
    public static string DefaultAudioPreset
    {
      get
      {
        using (RegistryKey preferencesKey = Registry.CurrentUser.OpenSubKey(ffdshowAudioRegKey))
        {
          if (preferencesKey != null)
          {
            return (string)preferencesKey.GetValue("activePreset");
          }
          else return null;
        }
      }
      set
      {
        string[] presetList = AudioPresets;
        // Check if we set an existing preset
        bool found = false;
        for (int i = 0; i < presetList.Length; i++)
        {
          if (presetList[i].Equals(value))
          {
            found = true;
            break;
          }
        }
        if (!found) return;
        using (RegistryKey preferencesKey = Registry.CurrentUser.CreateSubKey(ffdshowAudioRegKey))
        {
          if (preferencesKey != null)
          {
            preferencesKey.SetValue("activePreset", value);
          }
        }
      }
    }


    /// <summary>
    /// Gets or sets the video preset for the current instance. Also sets the preset as default.
    /// </summary>
    public String ActivePreset
    {
      get
      {
        string tmpStr = getStringParam(FFDShowConstants.FFDShowDataId.IDFF_OSDcurPreset);
        if (tmpStr != null && !tmpStr.Equals(""))
        {
          return tmpStr;
        }
        else
        {
          return DefaultVideoPreset;
        }
      }
      set
      {
        if (IsFFDShowActive)
        {
          PlayState playState = getState();
          if (playState == PlayState.PlayState || playState == PlayState.FastForwardRewind)
            pauseVideo();
          Win32.COPYDATASTRUCT cd = new Win32.COPYDATASTRUCT();
          cd.dwData = new UIntPtr((uint)FFDSM_SET_ACTIVE_PRESET_STR);
#if UNICODE
                    cd.lpData = Marshal.StringToHGlobalUni(value);
#else
          cd.lpData = Marshal.StringToHGlobalAnsi(value);
#endif
          cd.cbData = (uint)Win32.GlobalSize(cd.lpData);
          if (receiver == null)
            receiver = new FFDShowReceiver(Thread.CurrentThread);
          receiver.ReceivedString = null;
          receiver.ReceivedType = 0;
          //receiver.ParentThread = Thread.CurrentThread;
          Win32.SendMessage(new IntPtr(ffDShowInstanceHandle), Win32.WM_COPYDATA, receiver.Handle.ToInt32(), ref cd);
          if (playState == PlayState.PlayState || playState == PlayState.FastForwardRewind)
            startVideo();
        }
        DefaultVideoPreset = value;
      }
    }


    /// <summary>
    /// Gets or sets the default audio preset (does not apply to currently running instances).
    /// Same behaviour as DefaultAudioPreset property
    /// </summary>
    public string ActiveAudioPreset
    {
      get { return DefaultAudioPreset; }
      set { DefaultAudioPreset = value; }
    }

    #endregion Presets properties

    #region Enabled Properties

    // Show/hide subtitles
    /// <summary>
    /// Enable or disable subtitles filter
    /// </summary>
    public bool DoShowSubtitles
    {
      get
      {
        int value = getIntParam(FFDShowConstants.FFDShowDataId.IDFF_isSubtitles);
        if (value == 1)
          return true;
        else return false;
      }
      set
      {
        if (value)
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_isSubtitles, 1);
        else
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_isSubtitles, 0);
      }
    }

    public bool subEmbeddedPriority
    {
      get
      {
        int value = getIntParam(FFDShowConstants.FFDShowDataId.IDFF_subEmbeddedPriority);
        if (value == 1)
          return true;
        else return false;
      }
      set
      {
        if (value)
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_subEmbeddedPriority, 1);
        else
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_subEmbeddedPriority, 0);
      }
    }

    public bool subSSA
    {
      get
      {
        int value = getIntParam(FFDShowConstants.FFDShowDataId.IDFF_subSSA);
        if (value == 1)
          return true;
        else return false;
      }
      set
      {
        if (value)
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_subSSA, 1);
        else
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_subSSA, 0);
      }
    }

    public bool subPGS
    {
      get
      {
        int value = getIntParam(FFDShowConstants.FFDShowDataId.IDFF_subPGS);
        if (value == 1)
          return true;
        else return false;
      }
      set
      {
        if (value)
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_subPGS, 1);
        else
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_subPGS, 0);
      }
    }

    public bool subFiles
    {
      get
      {
        int value = getIntParam(FFDShowConstants.FFDShowDataId.IDFF_subFiles);
        if (value == 1)
          return true;
        else return false;
      }
      set
      {
        if (value)
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_subFiles, 1);
        else
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_subFiles, 0);
      }
    }

    public bool subVobsub
    {
      get
      {
        int value = getIntParam(FFDShowConstants.FFDShowDataId.IDFF_subVobsub);
        if (value == 1)
          return true;
        else return false;
      }
      set
      {
        if (value)
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_subVobsub, 1);
        else
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_subVobsub, 0);
      }
    }

    public bool subText
    {
      get
      {
        int value = getIntParam(FFDShowConstants.FFDShowDataId.IDFF_subText);
        if (value == 1)
          return true;
        else return false;
      }
      set
      {
        if (value)
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_subText, 1);
        else
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_subText, 0);
      }
    }

    public bool subTextpin
    {
      get
      {
        int value = getIntParam(FFDShowConstants.FFDShowDataId.IDFF_subTextpin);
        if (value == 1)
          return true;
        else return false;
      }
      set
      {
        if (value)
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_subTextpin, 1);
        else
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_subTextpin, 0);
      }
    }

    /// <summary>
    /// Enable/disable crop and zoom
    /// </summary>
    public bool DoCropZoom
    {
      get { return (getIntParam(FFDShowConstants.FFDShowDataId.IDFF_isCropNzoom) == 1); }
      set
      {
        if (value)
        {
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_isCropNzoom, 1);
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_magnificationLocked, 0);
          //IDFF_cropNzoomMode => 2
        }
        else
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_isCropNzoom, 0);
      }
    }

    /// <summary>
    /// Enable/disable lock of cropping
    /// </summary>
    public bool isCropZoomLocked
    {
      get
      {
        int value = getIntParam(FFDShowConstants.FFDShowDataId.IDFF_magnificationLocked);
        if (value == 1)
          return true;
        else return false;
      }
      set
      {
        if (value)
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_magnificationLocked, 1);
        else
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_magnificationLocked, 0);
      }
    }

    /// <summary>
    /// Enable/disable picture properties
    /// </summary>
    public bool DoPictureProperties
    {
      get
      {
        int value = getIntParam(FFDShowConstants.FFDShowDataId.IDFF_isPictProp);
        if (value == 1)
          return true;
        else return false;
      }
      set
      {
        if (value)
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_isPictProp, 1);
        else
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_isPictProp, 0);
      }
    }

    /// <summary>
    /// Enable/disable crop and zoom
    /// </summary>
    public bool DoPostProcessing
    {
      get
      {
        int value = getIntParam(FFDShowConstants.FFDShowDataId.IDFF_isPostproc);
        if (value == 1)
          return true;
        else return false;
      }
      set
      {
        if (value)
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_isPostproc, 1);
        else
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_isPostproc, 0);
      }
    }

    /// <summary>
    /// Enable/disable crop and zoom
    /// </summary>
    public bool DoResize
    {
      get
      {
        int value = getIntParam(FFDShowConstants.FFDShowDataId.IDFF_isResize);
        if (value == 1)
          return true;
        else return false;
      }
      set
      {
        if (value)
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_isResize, 1);
        else
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_isResize, 0);
      }
    }

    /// <summary>
    /// Enable/disable noise reduction
    /// </summary>
    public bool DoNoiseReduction
    {
      get
      {
        int value = getIntParam(FFDShowConstants.FFDShowDataId.IDFF_isBlur);
        if (value == 1)
          return true;
        else return false;
      }
      set
      {
        if (value)
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_isBlur, 1);
        else
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_isBlur, 0);
      }
    }

    /// <summary>
    /// Enable/disable sharpen
    /// </summary>
    public bool DoSharpen
    {
      get
      {
        int value = getIntParam(FFDShowConstants.FFDShowDataId.IDFF_isSharpen);
        if (value == 1)
          return true;
        else return false;
      }
      set
      {
        if (value)
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_isSharpen, 1);
        else
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_isSharpen, 0);
      }
    }

    /// <summary>
    /// Get/Set deinterlace
    /// </summary>
    public bool DoDeinterlace
    {
      get
      {
        int value = getIntParam(FFDShowConstants.FFDShowDataId.IDFF_isDeinterlace);
        if (value == 1)
          return true;
        else return false;
      }
      set
      {
        if (value)
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_isDeinterlace, 1);
        else
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_isDeinterlace, 0);
      }
    }

    #endregion Enabled Properties

    #region Subtitles/Audio streams Properties

    /// <summary>
    /// Subtitles/audio stream structure
    /// </summary>
    public struct Stream
    {
      /// <summary>
      /// Name of the stream
      /// </summary>
      public string name;

      /// <summary>
      /// Language name of the stream
      /// </summary>
      public string languageName;

      /// <summary>
      /// True if the stream is active
      /// </summary>
      public bool enabled;

      /// <summary>
      /// True if this is an external file
      /// </summary>
      public bool isFile;

      /// <summary>
      /// Constructor of a stream structure
      /// </summary>
      /// <param name="name">Name of the stream</param>
      /// <param name="languageName">Language name of the stream</param>
      /// <param name="enabled">True if the stream is active</param>
      public Stream(string name, string languageName, bool enabled)
      {
        this.name = name;
        this.languageName = languageName;
        this.enabled = enabled;
        this.isFile = false;
      }

      /// <summary>
      /// Constructor of a stream structure with external file flag
      /// </summary>
      /// <param name="name">Name of the stream</param>
      /// <param name="languageName">Language name of the stream</param>
      /// <param name="enabled">True if the stream is active</param>
      /// <param name="isFile">True if this stream is an external file</param>
      public Stream(string name, string languageName, bool enabled, bool isFile)
      {
        this.name = name;
        this.languageName = languageName;
        this.enabled = enabled;
        this.isFile = isFile;
      }
    }

    public class Streams : SortedDictionary<int, Stream>
    {
      public enum StreamType
      {
        EmbeddedOnly,
        FilesOnly
      }

      public int Size(StreamType type)
      {
        int cnt = 0;
        foreach (KeyValuePair<int, Stream> streamPair in this)
        {
          if (type == StreamType.EmbeddedOnly && !streamPair.Value.isFile) cnt++;
          else if (type == StreamType.FilesOnly && streamPair.Value.isFile) cnt++;
        }
        return cnt;
      }
    }

    /// <summary>
    /// Gets the list of subtitle streams
    /// </summary>
    public Streams SubtitleStreams
    {
      get
      {
        subEmbeddedPriority = true;
        Streams subtitleStreams = new Streams();
        if (ffdshowAPIMode == FFDShowAPIMode.DirectShowMode && streamSelect != null)
        {
          int streamsNb = 0;
          streamSelect.Count(out streamsNb);
          if (streamsNb == 0) return subtitleStreams;
          for (int i = 0; i < streamsNb; i++)
          {
            AMMediaType mediaType;
            AMStreamSelectInfoFlags flag;
            int group, langId;
            string streamName;
            object pppunk, ppobject;
            streamSelect.Info(i, out mediaType, out flag, out langId,
                              out group, out streamName, out pppunk, out ppobject);
            if (group == 4 || group == 2 && streamName.LastIndexOf("No ") == -1)
            {
              if ((streamName == null || streamName.Equals("")) && subtitleStreams.Count == 0)
                streamName = "No subtitles";

              String langName = "";

              if (langId != 0)
              {
                int size = Win32.GetLocaleInfo(langId, 2, null, 0);
                String languageName = new String(' ', size);

                Win32.GetLocaleInfo(langId, 2, languageName, size);
                if (!languageName.Equals(new String(' ', size)))
                {
                  if (languageName.Contains("\0"))
                    langName = languageName.Substring(0, languageName.IndexOf("\0"));
                  else
                    langName = languageName;
                  int ipos = langName.IndexOf("(");
                  if (ipos > 0)
                  {
                    langName = langName.Substring(0, ipos);
                    langName = langName.Trim();
                  }
                }
              }
              else
              {
                langName = streamName;
                // If some splitter doesn't give LCID but Language is here need a little hack for parsing in VideoPlayerVRM7.
              }
              if (group == 4)
              {
                Stream stream = new Stream(
                  FFDShowAPI.getFileName(streamName, FFDShowAPI.FileNameMode.FileNameLanguage),
                  FFDShowAPI.getFileName(langName, FFDShowAPI.FileNameMode.FileNameLanguage),
                  (flag & AMStreamSelectInfoFlags.Enabled) == AMStreamSelectInfoFlags.Enabled ? true : false, true);
                subtitleStreams.Add(i, stream);
              }
              else if (group == 2)
              {
                Stream stream = new Stream(streamName, langName,
                                           (flag & AMStreamSelectInfoFlags.Enabled) == AMStreamSelectInfoFlags.Enabled
                                             ? true
                                             : false, false);
                subtitleStreams.Add(i, stream);
              }
            }
          }
          return subtitleStreams;
        }

        string listString = getCustomParam(FFD_MSG.GET_SUBTITLESTREAMSLIST, 0);
        parseStreamsString(listString, subtitleStreams);
        return subtitleStreams;
      }
    }

    /// <summary>
    /// Gets the list of internal audio streams
    /// </summary>
    public Streams AudioStreams
    {
      get
      {
        Streams audioStreams = new Streams();
        if (ffdshowAPIMode == FFDShowAPIMode.DirectShowMode && streamSelect != null)
        {
          int streamsNb = 0;
          streamSelect.Count(out streamsNb);
          if (streamsNb == 0) return audioStreams;
          for (int i = 0; i < streamsNb; i++)
          {
            AMMediaType mediaType;
            AMStreamSelectInfoFlags flag;
            int group, langId;
            string streamName;
            object pppunk, ppobject;
            streamSelect.Info(i, out mediaType, out flag, out langId,
                              out group, out streamName, out pppunk, out ppobject);
            if (group == 1)
            {
              String langName = "";
              //Log.Error("FFDSHOWAPI(AudioStreams): streamName {0},langName {2}, langId {2}", streamName, langName, langId);
              if (langId != 0)
              {
                int size = Win32.GetLocaleInfo(langId, 2, null, 0);
                String languageName = new String(' ', size);

                Win32.GetLocaleInfo(langId, 2, languageName, size);
                if (!languageName.Equals(new String(' ', size)))
                {
                  if (languageName.Contains("\0"))
                    langName = languageName.Substring(0, languageName.IndexOf("\0"));
                  else
                    langName = languageName;
                  int ipos = langName.IndexOf("(");
                  if (ipos > 0)
                  {
                    langName = langName.Substring(0, ipos);
                    langName = langName.Trim();
                  }
                }
              }
              //Log.Error("FFDSHOWAPI(AudioStreams): langName {0}", langName);

              Stream stream = new Stream(streamName, langName,
                                         (flag & AMStreamSelectInfoFlags.Enabled) == AMStreamSelectInfoFlags.Enabled
                                           ? true
                                           : false);
              audioStreams.Add(i, stream);
            }
          }
          return audioStreams;
        }

        string listString = getCustomParam(FFD_MSG.GET_AUDIOSTREAMSLIST, 0);
        parseStreamsString(listString, audioStreams);
        return audioStreams;
      }
    }

    /// <summary>
    /// Gets or sets the current audio stream
    /// </summary>
    public int AudioStream
    {
      get
      {
        //return SendMessage(FFD_WPRM.GET_CURRENT_AUDIO_STREAM, 0);
        Streams audioStreams = AudioStreams;
        foreach (KeyValuePair<int, Stream> audioStream in audioStreams)
        {
          if (audioStream.Value.enabled)
            return audioStream.Key;
        }
        return 0;
      }
      set
      {
        if (ffdshowAPIMode == FFDShowAPIMode.InterProcessMode)
          SendMessage(FFD_WPRM.SET_AUDIO_STREAM, value);
        else if (streamSelect != null)
          streamSelect.Enable(value, AMStreamSelectEnableFlags.Enable);
      }
    }

    private void parseStreamsString(string listString, Streams streamsList)
    {
      string[] list = null;
      if (listString != null && listString.Length > 0)
      {
        list = listString.Split(new string[] {"</enabled></stream><stream><id>"}, StringSplitOptions.None);
        ;
        if (list != null)
        {
          for (int i = 0; i < list.Length; i++)
          {
            if (i == 0)
              list[i] = list[i].Replace("<stream><id>", "");
            if (i == list.Length - 1)
              list[i] = list[i].Replace("</enabled></stream>", "");

            string[] subElement = list[i].Split(new string[] {"</id><name>"}, StringSplitOptions.None);
            if (subElement != null)
            {
              int streamId = int.Parse(subElement[0]);
              string[] subSubElement = subElement[1].Split(new string[] {"</name><language_name>"},
                                                           StringSplitOptions.None);
              string streamName = subSubElement[0];
              string[] subSubSubElement = subSubElement[1].Split(new string[] {"</language_name><enabled>"},
                                                                 StringSplitOptions.None);
              string streamLanguageName = subSubSubElement[0];
              string enabled = subSubSubElement[1];
              bool isEnabled = false;
              if (enabled.Equals("true"))
                isEnabled = true;


              if (streamLanguageName.IndexOf("(") > 0)
                streamLanguageName = streamLanguageName.Substring(0, streamLanguageName.IndexOf("(") - 1);
              streamsList[streamId] = new Stream(streamName, streamLanguageName, isEnabled);
            }
          }
        }
      }
    }


    /// <summary>
    /// Gets or sets the current internal subtitle stream
    /// Gets : the id of the stream is returned. Returns -1 if no stream is selected
    /// Sets : the id of the stream must be passed
    /// </summary>
    public int SubtitleStream
    {
      get
      {
        Streams subtitleStreams = SubtitleStreams;
        foreach (KeyValuePair<int, Stream> subtitleStream in subtitleStreams)
        {
          if (subtitleStream.Value.enabled)
            return subtitleStream.Key;
        }
        return -1;
      }
      set
      {
        if (ffdshowAPIMode == FFDShowAPIMode.InterProcessMode)
          SendMessage(FFD_WPRM.SET_SUBTITLE_STREAM, value);
        else if (streamSelect != null)
          streamSelect.Enable(value, AMStreamSelectEnableFlags.Enable);
      }
    }


    /// <summary>
    /// Set/get substitles delay (in ms)
    /// </summary>
    public int SubtitlesDelay
    {
      get { return getIntParam(FFDShowConstants.FFDShowDataId.IDFF_subDelay); }
      set { setIntParam(FFDShowConstants.FFDShowDataId.IDFF_subDelay, value); }
    }

    /// <summary>
    /// Set/get subtitles ratio speed (default : 1000/1000)
    /// </summary>
    public int[] SubtitlesSpeed
    {
      get
      {
        int speed1 = getIntParam(FFDShowConstants.FFDShowDataId.IDFF_subSpeed);
        int speed2 = getIntParam(FFDShowConstants.FFDShowDataId.IDFF_subSpeed2);
        int[] values = new int[2] {speed1, speed2};
        return values;
      }
      set
      {
        setIntParam(FFDShowConstants.FFDShowDataId.IDFF_subSpeed, value[0]);
        setIntParam(FFDShowConstants.FFDShowDataId.IDFF_subSpeed2, value[1]);
      }
    }

    /// <summary>
    /// Set/get the current external subtitles file
    /// </summary>
    public string CurrentSubtitleFile
    {
      get
      {
        if (ffdshowAPIMode == FFDShowAPIMode.InterProcessMode)
          return getCustomParam(FFD_MSG.GET_CURRENT_SUBTITLES, 0); //FFDSM_GET_CURRENT_SUBTITLES);
        else
        {
          if (getIntParam(FFDShowConstants.FFDShowDataId.IDFF_subShowEmbedded) > 0) return null;
          string customSubtitleFile = getStringParam(FFDShowConstants.FFDShowDataId.IDFF_subTempFilename);
          if (customSubtitleFile != null && !customSubtitleFile.Equals("")) return customSubtitleFile;
          string fileName = null;
          ffdshowDec.getCurrentSubtitlesFile(out fileName);
          if (fileName != null && !fileName.Equals("")) return fileName;
          return null;
        }
      }
      set
      {
        setStringParam(FFDShowConstants.FFDShowDataId.IDFF_subTempFilename, value);
        //setStringParam(FFDShowConstants.FFDShowDataId.IDFF_subFilename, value);
        //setIntParam(FFDShowConstants.FFDShowDataId.IDFF_subAutoFlnm, 0);
        setIntParam(FFDShowConstants.FFDShowDataId.IDFF_isSubtitles, 1);
        setIntParam(FFDShowConstants.FFDShowDataId.IDFF_subShowEmbedded, 0);
      }
    }

    /// <summary>
    /// Returns true if the subtitle filter is enabled, false otherwise
    /// </summary>
    public bool SubtitlesEnabled
    {
      get { return (getIntParam(FFDShowConstants.FFDShowDataId.IDFF_isSubtitles) == 1) ? true : false; }
      set { setIntParam(FFDShowConstants.FFDShowDataId.IDFF_isSubtitles, (value == true) ? 1 : 0); }
    }


    /// <summary>
    /// Retrieves the list of available subtitle files
    /// </summary>
    public string[] SubtitleFiles
    {
      get
      {
        if (ffdshowAPIMode == FFDShowAPIMode.InterProcessMode)
        {
          string[] list = null;
          string listString = getCustomParam(FFD_MSG.GET_SUBTITLEFILESLIST, 0); //FFDSM_GET_SUBTITLEFILES);
          if (listString != null)
          {
            list = listString.Split(';');
          }
          return list;
        }

        List<string> subtitleFiles = new List<string>();
        if (ffdshowAPIMode == FFDShowAPIMode.DirectShowMode && streamSelect != null)
        {
          int streamsNb = 0;
          streamSelect.Count(out streamsNb);
          if (streamsNb == 0) return subtitleFiles.ToArray();
          for (int i = 0; i < streamsNb; i++)
          {
            AMMediaType mediaType;
            AMStreamSelectInfoFlags flag;
            int group, langId;
            string streamName;
            object pppunk, ppobject;
            streamSelect.Info(i, out mediaType, out flag, out langId,
                              out group, out streamName, out pppunk, out ppobject);
            if (group == 4)
            {
              if ((streamName == null || streamName.Equals("")) && subtitleFiles.Count == 0)
                streamName = "No subtitle file";
              subtitleFiles.Add(streamName);
            }
          }
        }
        return subtitleFiles.ToArray();
      }
    }

    /// <summary>
    /// Horizontal position of the subtitles (percentage value : 0 to 100)
    /// </summary>
    public int SubtitleHorizontalPosition
    {
      get { return getIntParam(FFDShowConstants.FFDShowDataId.IDFF_subPosX); }
      set { setIntParam(FFDShowConstants.FFDShowDataId.IDFF_subPosX, value); }
    }

    /// <summary>
    /// Vertical position of the subtitles (percentage value : 0 to 100)
    /// </summary>
    public int SubtitleVerticalPosition
    {
      get { return getIntParam(FFDShowConstants.FFDShowDataId.IDFF_subPosY); }
      set { setIntParam(FFDShowConstants.FFDShowDataId.IDFF_subPosY, value); }
    }

    /// <summary>
    /// Set the font size of subtitles on the screen
    /// </summary>
    public int SubtitleFontSize
    {
      get
      {
        /*if (getIntParam(FFDShowConstants.FFDShowDataId.IDFF_fontAutosize) == 1)
            return getIntParam(FFDShowConstants.FFDShowDataId.IDFF_fontSizeA);
        else
            return getIntParam(FFDShowConstants.FFDShowDataId.IDFF_fontSizeP);*/
        return getIntParam(FFDShowConstants.FFDShowDataId.IDFF_fontXscale);
      }
      set
      {
        /*if (getIntParam(FFDShowConstants.FFDShowDataId.IDFF_fontAutosize) == 1)
            setIntParam(FFDShowConstants.FFDShowDataId.IDFF_fontSizeA, value);
        else
            setIntParam(FFDShowConstants.FFDShowDataId.IDFF_fontSizeP, value);*/
        setIntParam(FFDShowConstants.FFDShowDataId.IDFF_fontXscale, value);
        setIntParam(FFDShowConstants.FFDShowDataId.IDFF_fontYscale, value);
      }
    }

    #endregion Subtitles Properties

    #region Other Properties

    /// <summary>
    /// List of chapters. Slow to process : call it once
    /// </summary>
    public Dictionary<int, string> ChaptersList
    {
      get
      {
        Dictionary<int, string> chaptersList = new Dictionary<int, string>();
        string[] list = null;
        string listString = getCustomParam(FFD_MSG.GET_CHAPTERSLIST, 0);
        if (listString != null && listString.Length > 0)
        {
          list = listString.Split(new string[] {"</name></chapter><chapter><time>"}, StringSplitOptions.None);
          ;
          if (list != null)
          {
            for (int i = 0; i < list.Length; i++)
            {
              if (i == 0)
                list[i] = list[i].Replace("<chapter><time>", "");
              if (i == list.Length - 1)
                list[i] = list[i].Replace("</name></chapter>", "");
              string[] chapterElement = list[i].Split(new string[] {"</time><name>"}, StringSplitOptions.None);
              if (chapterElement != null)
              {
                int chapterTime = int.Parse(chapterElement[0]);
                string chapterName = chapterElement[1];
                chaptersList[chapterTime] = chapterName;
              }
            }
          }
        }
        return chaptersList;
      }
    }

    #endregion Other Properties

    #region Filters Properties

    /// <summary>
    ///  Set/get horizontal cropping
    /// </summary>
    public int CropHorizontal
    {
      get { return getIntParam(FFDShowConstants.FFDShowDataId.IDFF_magnificationX); }
      set { setIntParam(FFDShowConstants.FFDShowDataId.IDFF_magnificationX, value); }
    }

    /// <summary>
    /// Get/set horizontal cropping
    /// </summary>
    public int CropVertical
    {
      get { return getIntParam(FFDShowConstants.FFDShowDataId.IDFF_magnificationY); }
      set { setIntParam(FFDShowConstants.FFDShowDataId.IDFF_magnificationY, value); }
    }

    /// <summary>
    /// Get or set the vertical resize
    /// </summary>
    public int ResizeVertical
    {
      get { return getIntParam(FFDShowConstants.FFDShowDataId.IDFF_resizeDy); }
      set { setIntParam(FFDShowConstants.FFDShowDataId.IDFF_resizeDy, value); }
    }

    /// <summary>
    /// Get or set the vertical resize
    /// </summary>
    public bool ResizeModeFitToScreen
    {
      get { return getIntParam(FFDShowConstants.FFDShowDataId.IDFF_resizeMode) == 5; }
      set
      {
        if (value)
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_resizeMode, 5);
        else
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_resizeMode, 0);
      }
    }

    /// <summary>
    /// Get or set the vertical resize
    /// </summary>
    public bool ResizeModeFreeResize
    {
      get { return getIntParam(FFDShowConstants.FFDShowDataId.IDFF_resizeMode) == 0; }
      set
      {
        if (value)
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_resizeMode, 0);
        else
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_resizeMode, 5);
      }
    }

    /// <summary>
    /// Get or set the keep aspect ratio
    /// </summary>
    public bool ResizeKeepAspectRatio
    {
      get { return getIntParam(FFDShowConstants.FFDShowDataId.IDFF_isAspect) == 1; }
      set
      {
        if (value)
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_isAspect, 1);
        else
          setIntParam(FFDShowConstants.FFDShowDataId.IDFF_isAspect, 0);
      }
    }


    /// <summary>
    /// Get or set the horizontal resize
    /// </summary>
    public int ResizeHorizontal
    {
      get { return getIntParam(FFDShowConstants.FFDShowDataId.IDFF_resizeDx); }
      set { setIntParam(FFDShowConstants.FFDShowDataId.IDFF_resizeDx, value); }
    }

    /// <summary>
    ///  Set/get audio delay
    /// </summary>
    public int AudioDelay
    {
      get { return getIntParam(FFDShowConstants.FFDShowDataId.IDFF_videoDelay); }
      set { setIntParam(FFDShowConstants.FFDShowDataId.IDFF_videoDelay, value); }
    }

    #endregion Filters Properties

    #region Picture Properties

    private bool pictureEnabled = false;

    /// <summary>
    /// Gets or sets the picture gamma
    /// </summary>
    public int PictureGama
    {
      get { return getIntParam(FFDShowConstants.FFDShowDataId.IDFF_gammaCorrection); }
      set
      {
        if (!pictureEnabled)
        {
          DoPictureProperties = true;
          pictureEnabled = true;
        }
        setIntParam(FFDShowConstants.FFDShowDataId.IDFF_gammaCorrection, value);
      }
    }

    /// <summary>
    /// Gets or sets the picture hue
    /// </summary>
    public int PictureHue
    {
      get { return getIntParam(FFDShowConstants.FFDShowDataId.IDFF_hue); }
      set
      {
        if (!pictureEnabled)
        {
          DoPictureProperties = true;
          pictureEnabled = true;
        }
        setIntParam(FFDShowConstants.FFDShowDataId.IDFF_hue, value);
      }
    }

    /// <summary>
    /// Gets or sets the picture saturation
    /// </summary>
    public int PictureSaturation
    {
      get { return getIntParam(FFDShowConstants.FFDShowDataId.IDFF_saturation); }
      set
      {
        if (!pictureEnabled)
        {
          DoPictureProperties = true;
          pictureEnabled = true;
        }
        setIntParam(FFDShowConstants.FFDShowDataId.IDFF_saturation, value);
      }
    }

    /// <summary>
    /// Gets or sets the picture contrast
    /// </summary>
    public int PictureContrast
    {
      get { return getIntParam(FFDShowConstants.FFDShowDataId.IDFF_lumGain); }
      set
      {
        if (!pictureEnabled)
        {
          DoPictureProperties = true;
          pictureEnabled = true;
        }
        setIntParam(FFDShowConstants.FFDShowDataId.IDFF_lumGain, value);
      }
    }

    /// <summary>
    /// Gets or sets the picture brightness
    /// </summary>
    public int PictureBrightness
    {
      get { return getIntParam(FFDShowConstants.FFDShowDataId.IDFF_lumOffset); }
      set { setIntParam(FFDShowConstants.FFDShowDataId.IDFF_lumOffset, value); }
    }

    #endregion Picture Properties

    #region PostProcessing Properties

    /// <summary>
    /// Gets or sets the postprocessing intensity (deblocking strength)
    /// </summary>
    public int PostProcessingIntensity
    {
      get { return getIntParam(FFDShowConstants.FFDShowDataId.IDFF_deblockStrength); }
      set { setIntParam(FFDShowConstants.FFDShowDataId.IDFF_deblockStrength, value); }
    }

    #endregion PostProcessing Properties

    #region Constructors

    /// <summary>
    /// Constructor using a DirectShow graph filter. 
    /// Recommanded if you have access to DirectShow graph. 
    /// However it cannot be using with the ROT (running object table)
    /// </summary>
    /// <param name="filter">FFDShow filter object</param>
    public FFDShowAPI(object filter)
    {
      ffdshowDec = filter as IffdshowDec;
      ffDecoder = filter as IffDecoder;
      ffdshowDecVideo = filter as IffdshowDecVideo;
      streamSelect = filter as IAMStreamSelect;
      ffdshowBase = filter as IffdshowBase;
      if (ffdshowDec == null || ffDecoder == null || ffdshowDecVideo == null || ffdshowBase == null)
        ffdshowAPIMode = FFDShowAPIMode.InterProcessMode;
      else
        ffdshowAPIMode = FFDShowAPIMode.DirectShowMode;
    }


    /// <summary>
    /// Basic constructor using interprocess communication
    /// </summary>
    public FFDShowAPI()
    {
      initOSD();
    }

    /// <summary>
    /// Constructor with setting of the FFDShow window handle
    /// <param name="FFDShowAPIRemote">Remote API Identifier</param>
    /// </summary>
    public FFDShowAPI(uint FFDShowAPIRemote)
    {
      this.FFDShowAPIRemote = FFDShowAPIRemote;
      initOSD();
    }

    /// <summary>
    /// Constructor where the given file name is searched for between all running FFDShow instances
    /// </summary>
    /// <param name="fileName">Media file name to look FFDShow instance for</param>
    /// <param name="fileNameMode">Filename mode (full path,...)</param>
    public FFDShowAPI(string fileName, FileNameMode fileNameMode)
    {
      this.fileName = fileName;
      this.fileNameMode = fileNameMode;
      initOSD();
    }

    /// <summary>
    /// Constructor where the given file name is searched for between all running FFDShow instances
    /// </summary>
    /// <param name="fileName">Media file name to look FFDShow instance for</param>
    /// <param name="fileNameMode">Filename mode (full path,...)</param>
    /// <param name="FFDShowAPIRemote">Remote API Identifier</param>
    public FFDShowAPI(string fileName, FileNameMode fileNameMode, uint FFDShowAPIRemote)
    {
      this.FFDShowAPIRemote = FFDShowAPIRemote;
      this.fileName = fileName;
      this.fileNameMode = fileNameMode;
      initOSD();
    }

    /// <summary>
    /// Constructor where the given FFDShow instance handle is searched for between all running FFDShow instances
    /// </summary>
    /// <param name="FFDShowInstanceHandle">Handle of FFDShow window to look for</param>
    public FFDShowAPI(int FFDShowInstanceHandle)
    {
      this.initFFDShowInstanceHandle = FFDShowInstanceHandle;
      initOSD();
    }

    /// <summary>
    /// Constructor where the given FFDShow instance handle is searched for between all running FFDShow instances
    /// </summary>
    /// <param name="FFDShowInstanceHandle">Handle of FFDShow window to look for</param>
    /// <param name="FFDShowAPIRemote">Remote API Identifier</param>
    public FFDShowAPI(int FFDShowInstanceHandle, uint FFDShowAPIRemote)
    {
      this.initFFDShowInstanceHandle = FFDShowInstanceHandle;
      this.FFDShowAPIRemote = FFDShowAPIRemote;
      initOSD();
    }

    /// <summary>
    /// FFDShowAPI desctructor
    /// </summary>
    ~FFDShowAPI()
    {
      Dispose();
    }

    /// <summary>
    /// Cleaning
    /// </summary>
    public void Dispose()
    {
      if (ffdshowDec != null)
        DirectShowUtil.ReleaseComObject(ffdshowDec);
      else if (ffDecoder != null)
        DirectShowUtil.ReleaseComObject(ffDecoder);
      else if (ffdshowBase != null)
        DirectShowUtil.ReleaseComObject(ffdshowBase);
      else if (ffdshowDecVideo != null)
        DirectShowUtil.ReleaseComObject(ffdshowDecVideo);
      else if (streamSelect != null)
        DirectShowUtil.ReleaseComObject(streamSelect);
      ffdshowDec = null;
      ffDecoder = null;
      ffdshowBase = null;
      ffdshowDecVideo = null;
      streamSelect = null;
      System.GC.SuppressFinalize(this);
    }

    private void initOSD()
    {
      if (osdX != 0 || osdY != 0)
        updateOSD = true;
    }

    #endregion Constructors

    #region Loading

    /// <summary>
    /// Initialization method. Must be called after constructor.
    /// It will look for a running FFDShow instance basing on constructor parameters.
    /// </summary>
    /// <returns>True if FFDShow instance found</returns>
    private bool init()
    {
      if (ffdshowAPIMode == FFDShowAPIMode.DirectShowMode) return true;
      if (fileName == null && initFFDShowInstanceHandle == 0)
      {
        ffDShowInstanceHandle = Win32.FindWindow(strAppName, null);
        if (ffDShowInstanceHandle == 0)
          IsFFDShowActive = false;
        else
          IsFFDShowActive = true;
        return IsFFDShowActive;
      }
      else
      {
        if (initFFDShowInstanceHandle != 0)
        {
          ffDShowInstanceHandle = initFFDShowInstanceHandle;
          if (Win32.IsWindow(ffDShowInstanceHandle) == 1)
            IsFFDShowActive = true;
          else
            IsFFDShowActive = false;
          return IsFFDShowActive;
        }
        else if (ffDShowInstanceHandle != 0)
        {
          if (Win32.IsWindow(ffDShowInstanceHandle) == 1)
            IsFFDShowActive = true;
          else
            IsFFDShowActive = false;

          return IsFFDShowActive;
        }
        else
        {
          List<FFDShowInstance> list = getFFDShowInstances();
          for (int i = 0; i < list.Count; i++)
          {
            int localFFDShowInstanceHandle = list[i].handle;
            string FFDShowFileName = list[i].fileName;
            if (FFDShowFileName == null)
              continue;
            try
            {
              FileInfo fileInfo;
              switch (fileNameMode)
              {
                case FileNameMode.FullPath:
                  if (fileName.Equals(FFDShowFileName))
                  {
                    ffDShowInstanceHandle = localFFDShowInstanceHandle;
                    return (IsFFDShowActive = true);
                  }
                  break;
                case FileNameMode.FileName:
                  fileInfo = new FileInfo(FFDShowFileName);
                  if (fileName.Equals(fileInfo.Name))
                  {
                    ffDShowInstanceHandle = localFFDShowInstanceHandle;
                    return (IsFFDShowActive = true);
                  }
                  break;
                case FileNameMode.FileNameWithoutExtension:
                  fileInfo = new FileInfo(FFDShowFileName);
                  string formattedFileName = fileInfo.Name;
                  if (formattedFileName.LastIndexOf('.') != -1)
                    formattedFileName = formattedFileName.Substring(0, formattedFileName.LastIndexOf('.'));
                  if (fileName.Equals(formattedFileName))
                  {
                    ffDShowInstanceHandle = localFFDShowInstanceHandle;
                    return (IsFFDShowActive = true);
                  }
                  break;
              }
            }
            catch (ArgumentException) {}
          }
          return (IsFFDShowActive = false);
        }
      }
    }

    /// <summary>
    /// Returns the list of FFDShow instances running
    /// </summary>
    /// <returns>The list (handle and file name) of FFDShow instances running</returns>
    public static List<FFDShowInstance> getFFDShowInstances()
    {
      List<FFDShowInstance> list = new List<FFDShowInstance>();
      List<int> instancesArray = new List<int>();
      GCHandle gch = GCHandle.Alloc(instancesArray);
      Win32.EnumWindows(new Win32.CallBack(EnumWindowCallBack), (IntPtr)gch);
      using (FFDShowAPI ffDShowAPI = new FFDShowAPI())
      {
        for (int i = 0; i < instancesArray.Count; i++)
        {
          ffDShowAPI.ffDShowInstanceHandle = instancesArray[i];
          string FFDShowFileName = ffDShowAPI.getFileName();
          FFDShowInstance instance = new FFDShowInstance();
          instance.handle = ffDShowAPI.ffDShowInstanceHandle;
          instance.fileName = FFDShowFileName;
          list.Add(instance);
        }
      }
      gch.Free();
      return list;
    }

    private static bool EnumWindowCallBack(int hwnd, IntPtr lParam)
    {
      GCHandle gch = (GCHandle)lParam;
      List<int> instancesArray = (List<int>)gch.Target;
      StringBuilder sbc = new StringBuilder(256);
      Win32.GetClassName(hwnd, sbc, sbc.Capacity);
      //sb = new StringBuilder(1024);
      //Win32.GetWindowText((int)windowHandle, sb, sb.Capacity);
      if (sbc.Length > 0)
      {
        if (sbc.ToString().Equals(strAppName))
          instancesArray.Add(hwnd);
      }
      gch.Free();
      return true;
    }

    /// <summary>
    /// Look for an active FFDShow instance basing on constructor parameters
    /// </summary>
    /// <returns>True if any</returns>
    public bool checkFFDShowActive()
    {
      if (!init())
        IsFFDShowActive = false;
      else
        IsFFDShowActive = true;
      return IsFFDShowActive;
    }

    /// <summary>
    /// Check that the previously found FFDShow instance is still active
    /// </summary>
    /// <returns></returns>
    public bool checkFFDShowStillActive()
    {
      if (ffDShowInstanceHandle == 0)
        return IsFFDShowActive = false;
      IsFFDShowActive = (Win32.IsWindow(ffDShowInstanceHandle) == 1);
      return IsFFDShowActive;
    }

    #endregion Loading

    #region Commands

    /// <summary>
    /// Stop video
    /// </summary>
    public void stopVideo()
    {
      PostMessage(FFD_WPRM.PAUSE_VIDEO, 0);
    }

    /// <summary>
    /// Start video
    /// </summary>
    public void startVideo()
    {
      PostMessage(FFD_WPRM.RESUME_VIDEO, 0);
    }

    /// <summary>
    /// Pause video
    /// </summary>
    public void pauseVideo()
    {
      PostMessage(FFD_WPRM.PAUSE_VIDEO, 0);
    }

    /// <summary>
    /// Fast forward
    /// </summary>
    /// <param name="seconds">Step in seconds</param>
    public void FastForward(int seconds)
    {
      if (ffrwNoOSD)
        SendMessage(FFD_WPRM.SET_FFRW_NO_OSD, 1);
      int res = 0;
      if (seconds >= 0)
        res = SendMessage(FFD_WPRM.FASTFORWARD, seconds);
      else
        res = SendMessage(FFD_WPRM.FASTREWIND, -seconds);
    }

    /// <summary>
    /// Rewind
    /// </summary>
    /// <param name="seconds">Step in seconds</param>
    public void FastRewind(int seconds)
    {
      if (ffrwNoOSD)
        SendMessage(FFD_WPRM.SET_FFRW_NO_OSD, 1);
      SendMessage(FFD_WPRM.FASTREWIND, seconds);
    }

    /// <summary>
    /// Stop FastForward or Rewind if active
    /// </summary>
    public void StopFastForward()
    {
      SendMessage(FFD_WPRM.FASTFORWARD, 0);
    }

    /// <summary>
    /// Retrieves the step of FastForward/Rewind (negative if rewind)
    /// </summary>
    /// <returns>Step in seconds</returns>
    public int getFastForwardSpeed()
    {
      return SendMessage(FFD_WPRM.GETFASTFORWARDSPEED, 0);
    }


    /// <summary>
    /// Capture still image of the video being played.
    /// This method used the current capture parameters.
    /// The captureJPGPicture method should be called first
    /// </summary>
    /// <returns>1 if successfull</returns>
    public int captureImage()
    {
      return SendMessage(FFD_WPRM.CAPTUREIMAGE, 0);
    }

    /// <summary>
    /// Sets the position in the timeline of the media being played
    /// </summary>
    /// <param name="time">Time to set in seconds</param>
    public void setCurrentTime(int time)
    {
      int result = SendMessage(FFD_WPRM.SET_CURTIME, time);
    }

    /// <summary>
    /// Enable or disable OSD (On Screen Display)
    /// </summary>
    public void toggleOSD()
    {
      int value = SendMessage(FFD_WPRM.GET_PARAM_VALUE_INT, (int)FFDShowConstants.FFDShowDataId.IDFF_isOSD);
      if (value == 0)
        value = 1;
      else
        value = 0;
      int result = SendMessage(FFD_WPRM.SET_PARAM_NAME, (int)FFDShowConstants.FFDShowDataId.IDFF_isOSD);
      result = PostMessage(FFD_WPRM.SET_PARAM_VALUE_INT, value);
    }


    /// <summary>
    /// Retrieve play state
    /// </summary>
    /// <returns>Play state</returns>
    public PlayState getState()
    {
      return (PlayState)SendMessage(FFD_WPRM.GET_STATE, 0);
    }

    /// <summary>
    /// Retrieve duration of the media being played
    /// </summary>
    /// <returns>Duration in seconds</returns>
    public int getDuration()
    {
      return SendMessage(FFD_WPRM.GET_DURATION, 0);
    }

    /// <summary>
    /// Retrieve the current position in the timeline of the media being played
    /// </summary>
    /// <returns>Current position in seconds</returns>
    public int getCurrentTime()
    {
      return SendMessage(FFD_WPRM.GET_CUR_TIME, 0);
    }

    /// <summary>
    /// Retrieve the frame rate
    /// </summary>
    /// <returns>Retrieve the frame rate (float with decimals eventually)</returns>
    public float getFrameRate()
    {
      int fps1000 = SendMessage(FFD_WPRM.GET_FRAMERATE, 0);
      return (float)fps1000 / 1000;
    }


    /// <summary>
    /// Retrieve the file name being played
    /// </summary>
    /// <returns>File name</returns>
    public string getFileName()
    {
      //TODO in directshow mode
      if (ffdshowAPIMode == FFDShowAPIMode.InterProcessMode)
        return getCustomParam(FFD_MSG.GET_SOURCEFILE, 0); //FFDSM_GET_FILENAME);
      else return "NOT IMPLEMENTED";
    }


    /// <summary>
    /// Retrieve the number of embedded subtitles
    /// </summary>
    /// <returns>Returns 0 if no embedded</returns>
    public int getEmbeddedSubtitles()
    {
      return getIntParam(FFDShowConstants.FFDShowDataId.IDFF_subShowEmbedded);
    }


    /// <summary>
    /// Request a (un)registration to FFDShow into the Running Object Table.
    /// It lets retrieve the DirectShow graph
    /// </summary>
    /// <param name="registration">Registration command</param>
    /// <returns>Result of the registration</returns>
    public int setROTRegistration(ROTRegistration registration)
    {
      return SendMessage(FFD_WPRM.SET_ADDTOROT, (int)registration);
    }

    /// <summary>
    /// Display a short OSD (On Screen Display) message
    /// This message will be displayed a few seconds and will disappear automatically
    /// </summary>
    /// <param name="message">Message to be displayed</param>
    public void displayShortOSDMessage(string message)
    {
      if (updateOSD)
      {
        setOSDPosition(osdX, osdY);
        updateOSD = false;
      }
      Win32.COPYDATASTRUCT cd = new Win32.COPYDATASTRUCT();
      cd.dwData = new UIntPtr((uint)FFDSM_SET_SHORTOSD_MSG);
#if UNICODE
            cd.lpData = Marshal.StringToHGlobalUni(message);
#else
      cd.lpData = Marshal.StringToHGlobalAnsi(message);
#endif
      cd.cbData = (uint)Win32.GlobalSize(cd.lpData);
      Win32.SendMessage(new IntPtr(ffDShowInstanceHandle), Win32.WM_COPYDATA, 0, ref cd);
    }

    /// <summary>
    /// Display an OSD (On Screen Display) message
    /// This message will be displayed according to font and position settings inside FFDShow OSD section
    /// This message remains displayed until the same method is called with an empty string ""
    /// </summary>
    /// <param name="message">Message to be displayed. Empty string to erase it</param>
    public void displayOSDMessage(string message)
    {
      if (updateOSD)
      {
        setOSDPosition(osdX, osdY);
        updateOSD = false;
      }
      Win32.COPYDATASTRUCT cd = new Win32.COPYDATASTRUCT();
      cd.dwData = new UIntPtr((uint)FFDSM_SET_OSD_MSG);
#if UNICODE
            cd.lpData = Marshal.StringToHGlobalUni(message);
#else
      cd.lpData = Marshal.StringToHGlobalAnsi(message);
#endif
      cd.cbData = (uint)Win32.GlobalSize(cd.lpData);
      Win32.SendMessage(new IntPtr(ffDShowInstanceHandle), Win32.WM_COPYDATA, 0, ref cd);
    }

    /// <summary>
    /// Sets the position of the OSD messages
    /// </summary>
    /// <param name="x">Horizontal position</param>
    /// <param name="y">Vertical position</param>
    public void setOSDPosition(int x, int y)
    {
      SendMessage(FFD_WPRM.SET_OSD_POSX, x);
      SendMessage(FFD_WPRM.SET_OSD_POSY, y);
    }

    #endregion Commands

    #region Presets commands

    /// <summary>
    /// List of FFDShow audio presets
    /// </summary>
    public static string[] AudioPresets
    {
      get
      {
        using (RegistryKey preferencesKey = Registry.CurrentUser.OpenSubKey(ffdshowAudioRegKey))
        {
          if (preferencesKey != null)
          {
            return preferencesKey.GetSubKeyNames();
          }
          else return null;
        }
      }
    }

    /// <summary>
    /// List of FFDShow video presets
    /// </summary>
    public static string[] VideoPresets
    {
      get
      {
        using (RegistryKey preferencesKey = Registry.CurrentUser.OpenSubKey(ffdshowRegKey))
        {
          if (preferencesKey != null)
          {
            return preferencesKey.GetSubKeyNames();
          }
          else return null;
        }
      }
    }

    /// <summary>
    /// List of FFDShow video presets DXVA
    /// </summary>
    public static string[] VideoPresetsDXVA
    {
      get
      {
        using (RegistryKey preferencesKey = Registry.CurrentUser.OpenSubKey(ffdshowDXVARegKey))
        {
          if (preferencesKey != null)
          {
            return preferencesKey.GetSubKeyNames();
          }
          else return null;
        }
      }
    }

    /// <summary>
    /// Returns the list of FFDShow video presets
    /// </summary>
    /// <returns>Presets list</returns>
    public string[] getPresetList()
    {
      if (IsFFDShowActive)
      {
        string[] presetList = null;
        string presetListString = getCustomParam(FFD_MSG.GET_PRESETLIST, 0); //FFDSM_GET_PRESETLIST);
        if (presetListString != null)
        {
          presetList = presetListString.Split(';');
        }
        return presetList;
      }
      else
      {
        return VideoPresets;
      }
    }

    /// <summary>
    /// Returns the list of FFDShow audio presets
    /// </summary>
    /// <returns></returns>
    public string[] getAudioPresetList()
    {
      return AudioPresets;
    }

    #endregion Presets commands

    #region Picture grab commands

    /// <summary>
    /// Capture current frame to JPG file
    /// </summary>
    /// <param name="Prefix">Prefix of the file (frame number will be concatenated)</param>
    /// <param name="Path">Path where to store the picture file</param>
    public void captureJPGPicture(string Prefix, string Path)
    {
      PlayState playState = getState();
      if (playState != PlayState.PauseState && playState != PlayState.PlayState)
        return;
      setIntParam(FFDShowConstants.FFDShowDataId.IDFF_grabFormat, 0);
      setIntParam(FFDShowConstants.FFDShowDataId.IDFF_grabMode, 1);
      setIntParam(FFDShowConstants.FFDShowDataId.IDFF_grabDigits, 0);
      setStringParam(FFDShowConstants.FFDShowDataId.IDFF_grabPrefix, Prefix);
      setStringParam(FFDShowConstants.FFDShowDataId.IDFF_grabPath, Path);
      captureImage();
      if (playState == PlayState.PauseState)
      {
        startVideo();
        Thread.Sleep(600);
        pauseVideo();
      }
      else
        Thread.Sleep(600);
    }

    #endregion

    #region Base commands

    public static int FFDShowRevision
    {
      get
      {
        try
        {
          using (RegistryKey ffdshowGlobalKey = Registry.LocalMachine.OpenSubKey(FFDShowRegKey))
          {
            return (int)ffdshowGlobalKey.GetValue("revision", 0);
          }
        }
        catch (Exception) {}
        return 0;
      }
    }

    /// <summary>
    /// Retrieve a parameter from FFDShow. The requested parameter must match to an integer type
    /// </summary>
    /// <param name="param">Parameter to retrieve</param>
    /// <returns>Value of the parameter</returns>
    public int getIntParam(FFDShowConstants.FFDShowDataId param)
    {
      if (ffdshowAPIMode == FFDShowAPIMode.InterProcessMode)
        return SendMessage(FFD_WPRM.GET_PARAM_VALUE_INT, (int)param);
      else if (ffdshowBase != null)
      {
        int val = 0;
        ffdshowBase.getParam((uint)param, out val);
        return val;
      }
      else return 0;
    }

    /// <summary>
    /// Set the value of a parameter to FFDShow. The requested parameter must match to an integer type
    /// </summary>
    /// <param name="param">Parameter to set</param>
    /// <param name="value">Value to set</param>
    public void setIntParam(FFDShowConstants.FFDShowDataId param, int value)
    {
      if (ffdshowAPIMode == FFDShowAPIMode.InterProcessMode)
      {
        SendMessage(FFD_WPRM.SET_PARAM_NAME, (int)param);
        SendMessage(FFD_WPRM.SET_PARAM_VALUE_INT, value);
      }
      else if (ffdshowBase != null)
        ffdshowBase.putParam((uint)param, value);
    }

    /// <summary>
    /// Retrieve a parameter from FFDShow. The requested parameter must match to a string type
    /// </summary>
    /// <param name="type">Type of parameter to retrieve.</param>
    /// <param name="param">Empty if type is different from FFD_MSG.GETPARAMSTR, otherwise the identifier of the string parameter to retrieve</param>
    /// <returns></returns>
    public string getCustomParam(FFD_MSG type, FFDShowConstants.FFDShowDataId param)
    {
      if (receiver == null)
        receiver = new FFDShowReceiver(Thread.CurrentThread);
      receiver.ReceivedString = null;
      receiver.ReceivedType = 0;
      IntPtr ret = new IntPtr(0);
      Win32.SendMessageTimeout(new IntPtr(ffDShowInstanceHandle), (int)type, receiver.Handle, new IntPtr((int)param),
                               Win32.SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, requestTimeout, out ret);

      if (ret.ToInt32() != TRUE)
        return null;

      /*Debug.WriteLine("Sleep " + param + "/" + type);
      Debug.Flush();*/
      if (receiver.ReceivedType == 0)
      {
        try
        {
          Thread.Sleep(requestTimeout);
          /*Debug.WriteLine("Timeout " + param + "/" + type);
          Debug.Flush();*/
        }
        catch (ThreadInterruptedException)
        {
          /*Debug.WriteLine("Interrupt " + param + "/" + type); Debug.Flush();*/
        }
        ;
      }

      // Check that the received string corresponds to the paramId we requested
      if ((param != 0 && receiver.ReceivedType == (int)param) || receiver.ReceivedType == (int)type)
        return receiver.ReceivedString;
      else return null;
    }

    /// <summary>
    /// Retrieve a string parameter from FFDShow.
    /// Same behaviour as getCustomParam(FFD_MSG.GETPARAMSTR, param)
    /// </summary>
    /// <param name="param">Parameter to retrieve</param>
    /// <returns>String value of the parameter</returns>
    public string getStringParam(FFDShowConstants.FFDShowDataId param)
    {
      if (ffdshowAPIMode == FFDShowAPIMode.InterProcessMode)
        return getCustomParam(FFD_MSG.GET_PARAMSTR, param);
      else if (ffdshowBase != null)
      {
        /*string result;
        ffdshowBase.getParamStr3((uint)param, out result);
        return result;*/
        //return ffdshowBase.getParamStr2((uint)param);
        IntPtr result = new IntPtr();
        ffdshowBase.getParamStr3((uint)param, out result);
        return Marshal.PtrToStringAuto(result);
      }
      else return null;
    }

    /// <summary>
    /// Set a string parameter to FFDShow
    /// </summary>
    /// <param name="param">Identifier of the parameter</param>
    /// <param name="value"></param>
    /// <returns></returns>
    public int setStringParam(FFDShowConstants.FFDShowDataId param, string value)
    {
      if (ffdshowAPIMode == FFDShowAPIMode.DirectShowMode && ffdshowBase != null)
      {
        return ffdshowBase.putParamStr((uint)param, value);
      }
      int result = SendMessage(FFD_WPRM.SET_PARAM_NAME, (int)param);
      //IntPtr WindowHandle = new IntPtr(this.FFDShowAPIRemote);

      Win32.COPYDATASTRUCT cd = new Win32.COPYDATASTRUCT();
      cd.dwData = new UIntPtr((uint)FFD_WPRM.SET_PARAM_VALUE_STR);
#if UNICODE
            cd.lpData = Marshal.StringToHGlobalUni(value);
#else
      cd.lpData = Marshal.StringToHGlobalAnsi(value);
#endif
      cd.cbData = (uint)Win32.GlobalSize(cd.lpData);
      IntPtr returnedValue = new IntPtr(0);
#if x64
            returnedValue = Win32.SendMessage(new IntPtr(ffDShowInstanceHandle), (int)Win32.WM_COPYDATA, 0, ref cd);
#else
      Win32.SendMessageTimeout(new IntPtr(ffDShowInstanceHandle), (int)Win32.WM_COPYDATA, receiver.Handle, ref cd,
                               Win32.SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, (int)requestTimeout, out returnedValue);
#endif
      Marshal.FreeHGlobal(cd.lpData);
      return returnedValue.ToInt32();
    }

    private int SendMessage(FFD_WPRM wParam, int lParam)
    {
      return Win32.SendMessage(ffDShowInstanceHandle, FFDShowAPIRemote, (int)wParam, lParam).ToInt32();
    }

    private int PostMessage(FFD_WPRM wParam, int lParam)
    {
      return Win32.PostMessage(ffDShowInstanceHandle, FFDShowAPIRemote, (int)wParam, lParam).ToInt32();
    }

    #endregion Base commands
  }
}