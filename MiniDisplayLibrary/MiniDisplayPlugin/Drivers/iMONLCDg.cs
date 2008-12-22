using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Win32;
using Win32.Utils.Cd;
using MediaPortal.ProcessPlugins.MiniDisplayPlugin;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.InputDevices;
using MediaPortal.Ripper;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
  public class iMONLCDg : BaseDisplay, IDisplay, IDisposable
  {
    private bool _Backlight;
    private ulong _BacklightLevel = 0x7fL;
    private bool _BlankDisplayOnExit;
    private bool _Contrast = true;
    private ulong _ContrastLevel = 10L;
    private int _CurrentLargeIcon;
    private int _delay;
    private int _delayG;
    private bool _DelayStartup;
    private bool _displayTest;
    private static int _DisplayType = -1;
    private bool _EnsureManagerStartup;
    private string _errorMessage = "";
    public static readonly byte[,] _Font8x5 = new byte[,] { 
            { 0, 0, 0, 0, 0, 0 }, { 0, 100, 0x18, 4, 100, 0x18 }, { 0, 60, 0x40, 0x40, 0x20, 0x7c }, { 0, 12, 0x30, 0x40, 0x30, 12 }, { 0, 60, 0x40, 0x30, 0x40, 60 }, { 0, 0, 0x3e, 0x1c, 8, 0 }, { 0, 4, 30, 0x1f, 30, 4 }, { 0, 0x10, 60, 0x7c, 60, 0x10 }, { 0, 0x20, 0x40, 0x3e, 1, 2 }, { 0, 0x22, 20, 8, 20, 0x22 }, { 0, 0, 0x38, 40, 0x38, 0 }, { 0, 0, 0x10, 0x38, 0x10, 0 }, { 0, 0, 0, 0x10, 0, 0 }, { 0, 8, 120, 8, 0, 0 }, { 0, 0, 0x15, 0x15, 10, 0 }, { 0, 0x7f, 0x7f, 9, 9, 1 }, 
            { 0, 0x10, 0x20, 0x7f, 1, 1 }, { 0, 4, 4, 0, 1, 0x1f }, { 0, 0, 0x19, 0x15, 0x12, 0 }, { 0, 0x40, 0x60, 80, 0x48, 0x44 }, { 0, 6, 9, 9, 6, 0 }, { 0, 15, 2, 1, 1, 0 }, { 0, 0, 1, 0x1f, 1, 0 }, { 0, 0x44, 0x44, 0x4a, 0x4a, 0x51 }, { 0, 20, 0x74, 0x1c, 0x17, 20 }, { 0, 0x51, 0x4a, 0x4a, 0x44, 0x44 }, { 0, 0, 0, 4, 4, 4 }, { 0, 0, 0x7c, 0x54, 0x54, 0x44 }, { 0, 8, 8, 0x2a, 0x1c, 8 }, { 0, 0x7c, 0, 0x7c, 0x44, 0x7c }, { 0, 4, 2, 0x7f, 2, 4 }, { 0, 0x10, 0x20, 0x7f, 0x20, 0x10 }, 
            { 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0x6f, 0, 0 }, { 0, 0, 7, 0, 7, 0 }, { 0, 20, 0x7f, 20, 0x7f, 20 }, { 0, 0, 7, 4, 30, 0 }, { 0, 0x23, 0x13, 8, 100, 0x62 }, { 0, 0x36, 0x49, 0x56, 0x20, 80 }, { 0, 0, 0, 7, 0, 0 }, { 0, 0, 0x1c, 0x22, 0x41, 0 }, { 0, 0, 0x41, 0x22, 0x1c, 0 }, { 0, 20, 8, 0x3e, 8, 20 }, { 0, 8, 8, 0x3e, 8, 8 }, { 0, 0, 80, 0x30, 0, 0 }, { 0, 8, 8, 8, 8, 8 }, { 0, 0, 0x60, 0x60, 0, 0 }, { 0, 0x20, 0x10, 8, 4, 2 }, 
            { 0, 0x3e, 0x51, 0x49, 0x45, 0x3e }, { 0, 0, 0x42, 0x7f, 0x40, 0 }, { 0, 0x42, 0x61, 0x51, 0x49, 70 }, { 0, 0x21, 0x41, 0x45, 0x4b, 0x31 }, { 0, 0x18, 20, 0x12, 0x7f, 0x10 }, { 0, 0x27, 0x45, 0x45, 0x45, 0x39 }, { 0, 60, 0x4a, 0x49, 0x49, 0x30 }, { 0, 1, 0x71, 9, 5, 3 }, { 0, 0x36, 0x49, 0x49, 0x49, 0x36 }, { 0, 6, 0x49, 0x49, 0x29, 30 }, { 0, 0, 0x36, 0x36, 0, 0 }, { 0, 0, 0x56, 0x36, 0, 0 }, { 0, 8, 20, 0x22, 0x41, 0 }, { 0, 20, 20, 20, 20, 20 }, { 0, 0, 0x41, 0x22, 20, 8 }, { 0, 2, 1, 0x51, 9, 6 }, 
            { 0, 0x3e, 0x41, 0x5d, 0x49, 0x4e }, { 0, 0x7e, 9, 9, 9, 0x7e }, { 0, 0x7f, 0x49, 0x49, 0x49, 0x36 }, { 0, 0x3e, 0x41, 0x41, 0x41, 0x22 }, { 0, 0x7f, 0x41, 0x41, 0x41, 0x3e }, { 0, 0x7f, 0x49, 0x49, 0x49, 0x41 }, { 0, 0x7f, 9, 9, 9, 1 }, { 0, 0x3e, 0x41, 0x49, 0x49, 0x7a }, { 0, 0x7f, 8, 8, 8, 0x7f }, { 0, 0, 0x41, 0x7f, 0x41, 0 }, { 0, 0x20, 0x40, 0x41, 0x3f, 1 }, { 0, 0x7f, 8, 20, 0x22, 0x41 }, { 0, 0x7f, 0x40, 0x40, 0x40, 0x40 }, { 0, 0x7f, 2, 12, 2, 0x7f }, { 0, 0x7f, 4, 8, 0x10, 0x7f }, { 0, 0x3e, 0x41, 0x41, 0x41, 0x3e }, 
            { 0, 0x7f, 9, 9, 9, 6 }, { 0, 0x3e, 0x41, 0x51, 0x21, 0x5e }, { 0, 0x7f, 9, 0x19, 0x29, 70 }, { 0, 70, 0x49, 0x49, 0x49, 0x31 }, { 0, 1, 1, 0x7f, 1, 1 }, { 0, 0x3f, 0x40, 0x40, 0x40, 0x3f }, { 0, 15, 0x30, 0x40, 0x30, 15 }, { 0, 0x3f, 0x40, 0x30, 0x40, 0x3f }, { 0, 0x63, 20, 8, 20, 0x63 }, { 0, 7, 8, 0x70, 8, 7 }, { 0, 0x61, 0x51, 0x49, 0x45, 0x43 }, { 0, 0, 0x7f, 0x41, 0, 0 }, { 0, 2, 4, 8, 0x10, 0x20 }, { 0, 0, 0x41, 0x7f, 0, 0 }, { 0, 4, 2, 1, 2, 4 }, { 0, 0x40, 0x40, 0x40, 0x40, 0x40 }, 
            { 0, 0, 0, 3, 4, 0 }, { 0, 0x20, 0x54, 0x54, 0x54, 120 }, { 0, 0x7f, 0x48, 0x44, 0x44, 0x38 }, { 0, 0x38, 0x44, 0x44, 0x44, 0x20 }, { 0, 0x38, 0x44, 0x44, 0x48, 0x7f }, { 0, 0x38, 0x54, 0x54, 0x54, 0x18 }, { 0, 8, 0x7e, 9, 1, 2 }, { 0, 12, 0x52, 0x52, 0x52, 0x3e }, { 0, 0x7f, 8, 4, 4, 120 }, { 0, 0, 0x44, 0x7d, 0x40, 0 }, { 0, 0x20, 0x40, 0x44, 0x3d, 0 }, { 0, 0, 0x7f, 0x10, 40, 0x44 }, { 0, 0, 0x41, 0x7f, 0x40, 0 }, { 0, 0x7c, 4, 0x18, 4, 120 }, { 0, 0x7c, 8, 4, 4, 120 }, { 0, 0x38, 0x44, 0x44, 0x44, 0x38 }, 
            { 0, 0x7c, 20, 20, 20, 8 }, { 0, 8, 20, 20, 0x18, 0x7c }, { 0, 0x7c, 8, 4, 4, 8 }, { 0, 0x48, 0x54, 0x54, 0x54, 0x20 }, { 0, 4, 0x3f, 0x44, 0x40, 0x20 }, { 0, 60, 0x40, 0x40, 0x20, 0x7c }, { 0, 0x1c, 0x20, 0x40, 0x20, 0x1c }, { 0, 60, 0x40, 0x30, 0x40, 60 }, { 0, 0x44, 40, 0x10, 40, 0x44 }, { 0, 12, 80, 80, 80, 60 }, { 0, 0x44, 100, 0x54, 0x4c, 0x44 }, { 0, 0, 8, 0x36, 0x41, 0x41 }, { 0, 0, 0, 0x7f, 0, 0 }, { 0, 0x41, 0x41, 0x36, 8, 0 }, { 0, 4, 2, 4, 8, 4 }, { 0, 0x7f, 0x6b, 0x6b, 0x6b, 0x7f }, 
            { 0, 0, 0x7c, 0x44, 0x7c, 0 }, { 0, 0, 8, 0x7c, 0, 0 }, { 0, 0, 100, 0x54, 0x48, 0 }, { 0, 0, 0x44, 0x54, 40, 0 }, { 0, 0, 0x1c, 0x10, 120, 0 }, { 0, 0, 0x5c, 0x54, 0x24, 0 }, { 0, 0, 120, 0x54, 0x74, 0 }, { 0, 0, 100, 20, 12, 0 }, { 0, 0, 0x7c, 0x54, 0x7c, 0 }, { 0, 0, 0x5c, 0x54, 60, 0 }, { 0, 120, 0x24, 0x26, 0x25, 120 }, { 0, 120, 0x25, 0x26, 0x24, 120 }, { 0, 0x70, 0x2a, 0x29, 0x2a, 0x70 }, { 0, 120, 0x25, 0x24, 0x25, 120 }, { 0, 0x20, 0x54, 0x56, 0x55, 120 }, { 0, 0x20, 0x55, 0x56, 0x54, 120 }, 
            { 0, 0x20, 0x56, 0x55, 0x56, 120 }, { 0, 0x20, 0x55, 0x54, 0x55, 120 }, { 0, 0x7c, 0x54, 0x56, 0x55, 0x44 }, { 0, 0x7c, 0x55, 0x56, 0x54, 0x44 }, { 0, 0x7c, 0x56, 0x55, 0x56, 0x44 }, { 0, 0x7c, 0x55, 0x54, 0x55, 0x44 }, { 0, 0x38, 0x54, 0x56, 0x55, 0x18 }, { 0, 0x38, 0x55, 0x56, 0x54, 0x18 }, { 0, 0x38, 0x56, 0x55, 0x56, 0x18 }, { 0, 0x38, 0x55, 0x54, 0x55, 0x18 }, { 0, 0, 0x44, 0x7e, 0x45, 0 }, { 0, 0, 0x45, 0x7e, 0x44, 0 }, { 0, 0, 70, 0x7d, 70, 0 }, { 0, 0, 0x45, 0x7c, 0x45, 0 }, { 0, 0, 0x48, 0x7a, 0x41, 0 }, { 0, 0, 0x49, 0x7a, 0x40, 0 }, 
            { 0, 0, 0x4a, 0x79, 0x42, 0 }, { 0, 0, 0x49, 120, 0x41, 0 }, { 0, 0x38, 0x44, 70, 0x45, 0x38 }, { 0, 0x38, 0x45, 70, 0x44, 0x38 }, { 0, 0x38, 70, 0x45, 70, 0x38 }, { 0, 0x38, 0x45, 0x44, 0x45, 0x38 }, { 0, 0x30, 0x48, 0x4a, 0x49, 0x30 }, { 0, 0x30, 0x49, 0x4a, 0x48, 0x30 }, { 0, 0x30, 0x4a, 0x49, 0x4a, 0x30 }, { 0, 0x30, 0x49, 0x48, 0x49, 0x30 }, { 0, 60, 0x40, 0x42, 0x41, 60 }, { 0, 60, 0x41, 0x42, 0x40, 60 }, { 0, 60, 0x42, 0x41, 0x42, 60 }, { 0, 60, 0x41, 0x40, 0x41, 60 }, { 0, 60, 0x40, 0x42, 0x21, 0x7c }, { 0, 60, 0x41, 0x42, 0x20, 0x7c }, 
            { 0, 0x38, 0x42, 0x41, 0x22, 120 }, { 0, 60, 0x41, 0x40, 0x21, 0x7c }, { 0, 0x4e, 0x51, 0x71, 0x11, 10 }, { 0, 0x58, 100, 100, 0x24, 0x10 }, { 0, 0x7c, 10, 0x11, 0x22, 0x7d }, { 0, 120, 0x12, 9, 10, 0x71 }, { 0, 0, 0, 4, 2, 1 }, { 0, 1, 2, 4, 0, 0 }, { 0, 0, 2, 0, 2, 0 }, { 0, 0x30, 0x48, 0x45, 0x40, 0x20 }, { 0, 0, 0, 0x7b, 0, 0 }, { 0, 0x38, 0x44, 0x44, 0x38, 0x44 }, { 0, 0x40, 0x3e, 0x49, 0x49, 0x36 }, { 0, 8, 4, 8, 0x70, 12 }, { 0, 0x60, 80, 0x48, 80, 0x60 }, { 0, 0x30, 0x48, 0x45, 0x40, 0 }, 
            { 0, 0x7c, 0x13, 0x12, 0x12, 0x7c }, { 0, 0x7c, 0x12, 0x12, 0x13, 0x7c }, { 0, 240, 0x2a, 0x29, 0x2a, 240 }, { 0, 240, 0x2a, 0x29, 0x2a, 0xf1 }, { 0, 0x7c, 0x13, 0x12, 0x13, 0x7c }, { 0, 0x40, 60, 0x12, 0x12, 12 }, { 0, 0x7c, 1, 0x7f, 0x49, 0x41 }, { 0, 14, 0x11, 0xb1, 0xd1, 10 }, { 0, 0x7c, 0x55, 0x56, 0x54, 0 }, { 0, 0x7c, 0x54, 0x56, 0x55, 0 }, { 0, 0x7f, 0x49, 0x49, 0x49, 0 }, { 0, 0x7c, 0x55, 0x54, 0x55, 0 }, { 0, 0, 0x41, 0x7f, 0x48, 0 }, { 0, 0, 0x48, 0x7a, 0x49, 0 }, { 0, 0, 0x4a, 0x79, 0x4a, 0 }, { 0, 0, 0x45, 0x7c, 0x45, 0 }, 
            { 0, 8, 0x7f, 0x49, 0x41, 0x3e }, { 0, 120, 10, 0x11, 0x22, 0x79 }, { 0, 0x38, 0x45, 70, 0x44, 0x38 }, { 0, 0x38, 0x44, 70, 0x45, 0x38 }, { 0, 0x30, 0x4a, 0x49, 0x4a, 0x30 }, { 0, 0x30, 0x4a, 0x49, 0x41, 0x31 }, { 0, 0x38, 0x45, 0x44, 0x45, 0x38 }, { 0, 0, 20, 8, 20, 0 }, { 0, 0x3e, 0x51, 0x49, 0x44, 0x3e }, { 0, 60, 0x41, 0x42, 0x40, 60 }, { 0, 60, 0x40, 0x42, 0x41, 60 }, { 0, 0x3f, 0x40, 0x40, 0x40, 0x3f }, { 0, 60, 0x41, 0x40, 0x41, 60 }, { 0, 12, 0x10, 0x62, 0x11, 12 }, { 0, 0x7f, 0x22, 0x22, 0x22, 0x1c }, { 0, 0x7e, 0x21, 0x2d, 0x2d, 0x12 }, 
            { 0, 0x40, 0xa9, 170, 0xa8, 240 }, { 0, 0x40, 0xa8, 170, 0xa9, 240 }, { 0, 0x40, 170, 0xa9, 170, 240 }, { 0, 0x40, 170, 0xa9, 170, 0xf1 }, { 0, 0x20, 0x55, 0x54, 0x55, 120 }, { 0, 80, 0x55, 0x55, 0x54, 120 }, { 0, 0x40, 0x5e, 0x45, 0x5e, 0x40 }, { 0, 14, 0x91, 0xb1, 0x51, 8 }, { 0, 0x38, 0x55, 0x56, 0x54, 0x18 }, { 0, 0x38, 0x54, 0x56, 0x55, 0x18 }, { 0, 0x70, 170, 0xa9, 170, 0x30 }, { 0, 0x38, 0x55, 0x54, 0x55, 0x18 }, { 0, 0, 0x44, 0x7d, 0x42, 0 }, { 0, 0, 0x48, 0x7a, 0x41, 0 }, { 0, 0, 0x4a, 0x79, 0x42, 0 }, { 0, 0, 0x44, 0x7d, 0x40, 0 }, 
            { 0, 0x10, 0x3e, 0x7e, 0x3e, 0x10 }, { 0, 0x55, 0x2a, 0x55, 0x2a, 0x55 }, { 0, 0x30, 0x49, 0x4a, 0x48, 0x30 }, { 0, 0x30, 0x48, 0x4a, 0x49, 0x30 }, { 0, 0x30, 0x4a, 0x49, 0x4a, 0x30 }, { 0, 0x38, 0x45, 0x44, 0x45, 0x38 }, { 0, 0x38, 0x45, 0x44, 0x45, 0x38 }, { 0, 60, 0x41, 0x40, 0x41, 60 }, { 0, 0x38, 0x44, 0x44, 0x44, 0x38 }, { 0, 60, 0x41, 0x42, 0x20, 0x7c }, { 0, 60, 0x40, 0x42, 0x21, 0x7c }, { 0, 0x38, 0x42, 0x41, 0x22, 0x7c }, { 0, 60, 0x41, 0x40, 0x21, 0x7c }, { 0, 12, 80, 0x52, 80, 60 }, { 0, 0x7c, 40, 40, 0x10, 0 }, { 0, 12, 0x51, 80, 0x51, 60 }
         };
    private string _ForceDisplay;
    private bool _ForceKeyBoardMode;
    private bool _ForceManagerRestart;
    private bool _ForceManagerReload;
    private int _gcols = 0x60;
    private int _grows = 0x10;
    private Thread _iconThread;
    private iMONDisplay _IMON;
    private static readonly int[,] _iMON_FW_Display = new int[,] { 
            { 8, 0, 2, 0, 0 }, { 0x36, 0, 9, 0, 3 }, { 0x39, 0, 10, 0, 0 }, { 0x3a, 0, 9, 0, 3 }, { 0x3b, 0, 0x11, 0, 3 }, { 0x49, 0, 0x12, 0, 0 }, { 0x4b, 0, 20, 0, 0 }, { 0x4c, 0, 0x12, 0, 0 }, { 0x3d, 0, 0x10, 0, 0 }, { 30, 0, 0x17, 0, 0 }, { 0x30, 50, 4, 0, 0 }, { 0x33, 60, 6, 0, 0 }, { 0x3e, 0x3f, 11, 0, 0 }, { 0x40, 0, 8, 0, 0 }, { 0x41, 0x47, 12, 0, 0 }, { 0x48, 0x4f, 13, 0, 0 }, 
            { 0x70, 0x77, 7, 0, 0 }, { 120, 0x7f, 14, 0, 0 }, { 0x80, 0x83, 15, 0, 0 }, { 0x85, 0, 0x10, 0, 2 }, { 0x84, 0x8f, 0x10, 0, 0 }, { 0x90, 0x91, 0x13, 0x8888, 1 }, { 0x92, 0x97, 0x15, 0x8888, 1 }, { 0x98, 0x99, 0x18, 0x8888, 1 }, { 0x9a, 0x9b, 0x19, 0x8888, 1 }, { 0x9c, 0x9f, 0x16, 0x8888, 1 }, { 160, 0, 0x10, 0, 0 }, { 0xa1, 0, 0x16, 0x8888, 1 }, { 0x3600, 0x37ff, 0x1a, 0x8888, 0 }, { 0x3800, 0x39ff, 0x1b, 0x8888, 4 }, { 0x3412, 0, 0, 0, 2 }, { 0x3c40, 0, 0, 0, 2 }, 
            { 0x3e00, 0x3eff, 0x1a, 0, 0 }, { 0x3f00, 0x3fff, 0x1b, 0x8888, 4 }, { 0, 0, 0, 0, 2 }
         };
    private static readonly int[,] _iMON_FW_Display_OLD = new int[,] { 
            { 8, 0, 2, 0, 0 }, { 30, 0, 0x17, 0, 0 }, { 0x30, 0, 4, 0, 0 }, { 0x31, 0, 4, 0, 0 }, { 50, 0, 4, 0, 0 }, { 0x33, 0, 6, 0, 0 }, { 0x34, 0, 6, 0, 0 }, { 0x35, 0, 6, 0, 0 }, { 0x36, 0, 6, 0, 0 }, { 0x37, 0, 6, 0, 0 }, { 0x38, 0, 6, 0, 0 }, { 0x39, 0, 10, 0, 0 }, { 0x39, 0, 6, 0, 0 }, { 0x3a, 0, 9, 0, 2 }, { 0x3b, 0, 0x11, 0, 3 }, { 60, 0, 6, 0, 0 }, 
            { 0x3d, 0, 0x10, 0, 0 }, { 0x3e, 0, 11, 0, 0 }, { 0x3f, 0, 11, 0, 0 }, { 0x40, 0, 8, 0, 0 }, { 0x41, 0, 12, 0, 0 }, { 0x42, 0, 12, 0, 0 }, { 0x43, 0, 12, 0, 0 }, { 0x44, 0, 12, 0, 0 }, { 0x45, 0, 12, 0, 0 }, { 70, 0, 12, 0, 0 }, { 0x47, 0, 12, 0, 0 }, { 0x48, 0, 13, 0, 0 }, { 0x49, 0, 0x12, 0, 0 }, { 0x4a, 0, 13, 0, 0 }, { 0x4b, 0, 20, 0, 0 }, { 0x4c, 0, 13, 0, 0 }, 
            { 0x4d, 0, 13, 0, 0 }, { 0x4e, 0, 13, 0, 0 }, { 0x4f, 0, 13, 0, 0 }, { 0x70, 0, 7, 0, 0 }, { 0x71, 0, 7, 0, 0 }, { 0x72, 0, 7, 0, 0 }, { 0x73, 0, 7, 0, 0 }, { 0x74, 0, 7, 0, 0 }, { 0x75, 0, 7, 0, 0 }, { 0x76, 0, 7, 0, 0 }, { 0x77, 0, 7, 0, 0 }, { 120, 0, 14, 0, 0 }, { 0x79, 0, 14, 0, 0 }, { 0x7a, 0, 14, 0, 0 }, { 0x7b, 0, 14, 0, 0 }, { 0x7c, 0, 14, 0, 0 }, 
            { 0x7d, 0, 14, 0, 0 }, { 0x7e, 0, 14, 0, 0 }, { 0x7f, 0, 14, 0, 0 }, { 0x80, 0, 15, 0, 0 }, { 0x81, 0, 15, 0, 0 }, { 130, 0, 15, 0, 0 }, { 0x83, 0, 15, 0, 0 }, { 0x84, 0, 0x10, 0, 0 }, { 0x85, 0, 0x10, 0, 2 }, { 0x86, 0, 0x10, 0, 0 }, { 0x87, 0, 0x10, 0, 0 }, { 0x88, 0, 0x10, 0, 0 }, { 0x89, 0, 0x10, 0, 0 }, { 0x8a, 0, 0x10, 0, 0 }, { 0x8b, 0, 0x10, 0, 0 }, { 140, 0, 0x10, 0, 0 }, 
            { 0x8d, 0, 0x10, 0, 0 }, { 0x8e, 0, 0x10, 0, 0 }, { 0x8f, 0, 0x10, 0, 0 }, { 0x90, 0x91, 0x13, 0x8888, 1 }, { 0x92, 0x97, 0x15, 0x8888, 1 }, { 0x98, 0, 0x18, 0x8888, 1 }, { 0x99, 0, 0x18, 0x8888, 1 }, { 0x9a, 0, 0x19, 0x8888, 1 }, { 0x9b, 0, 0x19, 0x8888, 1 }, { 0x9c, 0, 0x16, 0x8888, 1 }, { 0x9d, 0, 0x16, 0x8888, 1 }, { 0x9e, 0, 0x16, 0x8888, 1 }, { 0x9f, 0, 0x16, 0x8888, 1 }, { 160, 0, 0x10, 0, 0 }, { 0xa1, 0, 0x16, 0x8888, 1 }, { 0x3600, 0x37ff, 0x1a, 0x8888, 0 }, 
            { 0x3801, 0, 0x1b, 0x8888, 4 }, { 0x3800, 0x39ff, 0x1b, 0x8888, 4 }, { 0x3f00, 0x3fff, 0x1b, 0x8888, 4 }, { 0, 0, 0, 0, 2 }
         };
    private InputHandler _inputHandler;
    public static readonly byte[,] _InternalLargeIcons = new byte[,] { { 
            0xc0, 0x80, 0x80, 0xc0, 0xff, 0xc0, 0x80, 0x80, 0xc0, 0xff, 0xc7, 0x83, 0x93, 0x83, 0xc7, 0xff, 
            3, 1, 1, 3, 0xff, 3, 1, 1, 3, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff
         }, { 
            0xff, 0xfe, 0xfe, 0xbd, 0xdd, 0xed, 0xf5, 0xf9, 0xf9, 0xf5, 0xed, 0xdd, 0xbd, 0xfe, 0xfe, 0xff, 
            0xff, 1, 0xf9, 0xfd, 0xfd, 0xfd, 0xfd, 0xfd, 0xfd, 0xfd, 0xfd, 0xfd, 0xfd, 0xf9, 1, 0xff
         }, { 
            0xff, 0x80, 0xaf, 0x8f, 0xaf, 0x8f, 0xaf, 0x8f, 0xaf, 0x8f, 0xaf, 0x8f, 0xaf, 0x8f, 160, 0x8f, 
            0xf5, 1, 0xf5, 0xf1, 0xf5, 0xf1, 0xf5, 0xf1, 0xf5, 0xf1, 0xf5, 0xf1, 0xf5, 0xf1, 5, 1
         }, { 
            0xff, 0xff, 0xff, 0xff, 0xff, 0x80, 0xc7, 0xc3, 0xe3, 0xe3, 0xe3, 0xe3, 0xff, 0xff, 0xff, 0xff, 
            0xff, 0xff, 0xf3, 0xe1, 0xe1, 3, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff
         }, { 
            0xe3, 0xde, 0xbf, 0xbb, 0xbf, 0xde, 0xe1, 0xe1, 0xde, 0xbf, 0xbb, 0xbf, 0xde, 0xe1, 0xff, 0xff, 
            0xff, 1, 0x7d, 0x7d, 0x7d, 0x7d, 0x7d, 0x7d, 0x7d, 0x7d, 0x7d, 13, 0xcf, 0xb7, 0xb7, 0x87
         }, { 
            0xf8, 0xf8, 0xf2, 0xf2, 0xf2, 230, 230, 0xce, 0xce, 0xce, 0x9e, 0x9e, 0x3e, 0x3e, 0x3e, 0xff, 
            0, 30, 0x3e, 0x7e, 0x5e, 30, 0x3e, 0x7e, 0x5e, 30, 0x3e, 0x7e, 0x5e, 30, 14, 0xff
         }, { 
            0xff, 0xff, 0xff, 0xff, 0xc0, 0x80, 0x80, 0xc0, 0xff, 0xc0, 0x80, 0x80, 0xc0, 0xff, 0xff, 0xff, 
            0xff, 0xff, 0xff, 0xff, 3, 1, 1, 3, 0xff, 3, 1, 1, 3, 0xff, 0xff, 0xff
         }, { 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
         }, { 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
         }, { 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
         } };
    private bool _IsConfiguring;
    private bool _isDisabled;
    private bool _IsDisplayOff;
    private bool _IsHandlingPowerEvent;
    private static bool _IsVistaOS = (Environment.OSVersion.Version.Major > 5);
    private byte[] _lastHash;
    private readonly string[] _lines = new string[2];
    private bool _MonitorPower;
    private bool _mpIsIdle;
    private Thread _RemoteThread;
    private bool _RestartFrontviewOnExit;
    private readonly SHA256Managed _sha256 = new SHA256Managed();
    private static bool _stopRemoteThread = false;
    private static bool _stopUpdateIconThread = false;
    private int _tcols = 0x10;
    private int _trows = 2;
    private bool _USE_VFD_ICONS = false;
    //private bool _UseRC;
    private bool _UsingAntecManager;
    private bool _UsingSoundgraphManager;
    private static bool _VFD_UseV3DLL;
    private static int _VfdReserved = 0x8888;
    private static int _VfdType = 0x18;
    private AdvancedSettings AdvSettings = AdvancedSettings.Load();
    private byte[] bitmapData;
    private CustomFont CFont;
    private readonly BitmapConverter converter = new BitmapConverter(true);
    private LargeIcon CustomLargeIcon;
    private DisplayOptions DisplayOptions;
    private DisplayControl DisplaySettings;
    private bool DoDebug;
    private static DeviceVolumeMonitor DVM;
    private bool DVMactive;
    private static object DWriteMutex = new object();
    private EQControl EQSettings;
    private string IdleMessage = string.Empty;
    //private char IMON_CHAR_1_BAR;
    //private char IMON_CHAR_2_BARS = '\x0001';
    //private char IMON_CHAR_3_BARS = '\x0002';
    //private char IMON_CHAR_4_BARS = '\x0003';
    //private char IMON_CHAR_5_BARS = '\x0004';
    private char IMON_CHAR_6_BARS = '\x0005';
    //private char IMON_CHAR_7_BARS = '\x0006';
    //private char IMON_CHAR_8_BARS = '\a';
    //private char IMON_VFD_CHAR_ARROW_DOWN = '\x0019';
    //private char IMON_VFD_CHAR_ARROW_LEFT = '\x001b';
    //private char IMON_VFD_CHAR_ARROW_RIGHT = '\x001a';
    //private char IMON_VFD_CHAR_ARROW_UP = '\x0018';
    //private char IMON_VFD_CHAR_BLOCK_EMPTY = '2';
    private char IMON_VFD_CHAR_BLOCK_FILLED = '\a';
    //private char IMON_VFD_CHAR_DBL_TRI_DOWN = '\x0015';
    //private char IMON_VFD_CHAR_DBL_TRI_UP = '\x0014';
    //private char IMON_VFD_CHAR_ENTER = '\x0017';
    //private char IMON_VFD_CHAR_HEART = '\x009d';
    //private char IMON_VFD_CHAR_HOUSE = '\x007f';
    private char IMON_VFD_CHAR_PAUSE = '\x00a0';
    private char IMON_VFD_CHAR_PLAY = '\x0010';
    private char IMON_VFD_CHAR_RECORD = '\x0016';
    private char IMON_VFD_CHAR_RPLAY = '\x0011';
    //private char IMON_VFD_CHAR_TRI_DOWN = '\x001f';
    //private char IMON_VFD_CHAR_TRI_UP = '\x001e';
    private string imonVFD_DLLFile;
    private static readonly int[] Inserted_Media = new int[0x1b];
    private int LastProgLevel;
    private DateTime LastSettingsCheck = DateTime.Now;
    private int LastVolLevel;
    private static int[] MCEKeyCodeToKeyCode = new int[] { 
            0, 0, 0, 0, 0x41, 0x42, 0x43, 0x44, 0x45, 70, 0x47, 0x48, 0x49, 0x4a, 0x4b, 0x4c, 
            0x4d, 0x4e, 0x4f, 80, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 90, 0x31, 50, 
            0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x30, 13, 0x1b, 8, 9, 0x20, 0x6d, 0xbb, 0xdb, 
            0xdd, 0xe2, 0, 0xba, 0xde, 0xc0, 0xbc, 190, 0xbf, 20, 0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 
            0x76, 0x77, 120, 0x79, 0x7a, 0x7b, 0, 0, 0, 0x2d, 0x24, 0x21, 0x2e, 0x23, 0x22, 0x27, 
            0x25, 40, 0x26, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 0xa5
         };
    private static string[] MCEKeyCodeToKeyString = new string[] { 
            "", "", "", "", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", 
            "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "1", "2", 
            "3", "4", "5", "6", "7", "8", "9", "0", "{ENTER}", "{ESC}", "{BS}", "{TAB}", " ", "-", "=", "{[}", 
            "{]}", @"\", "", ";", "'", "`", ",", ".", "?", "{CAPSLOCK}", "{F1}", "{F2}", "{F3}", "{F4}", "{F5}", "{F6}", 
            "{F7}", "{F8}", "{F9}", "{F10}", "{F11}", "{F12}", "", "", "", "{INSERT}", "{HOME}", "{PGUP}", "{DELETE}", "{END}", "{PGDN}", "{RIGHT}", 
            "{LEFT}", "{DOWN}", "{UP}", "", "", "", "", "", "", "", "", "", "", "", "", "", 
            "", "", "", "", "", ""
         };
    private static int[] MCEModifierToKeyModifier = new int[] { 0, 0x20000, 0x10000, 0, 0x40000, 0, 0, 0, 0x5b, 0, 0, 0, 0, 0, 0, 0 };
    private static string[] MCEModifierToModifierString = new string[] { "*", "^{*}", "+{*}", "^{+{*}}", "%{*}", "%{^{*}}", "%{+{*}}", "%{^{+{*}}}", "*", "^{*}", "+{*}", "+{^{*}}", "%{*}", "%{^{*}}", "%{+{*}}", "%{+{^{*}}}" };
    private SystemStatus MPStatus = new SystemStatus();
    private DateTime NullTime;
    private int progLevel;
    private static object RemoteMutex = new object();
    private RemoteControl RemoteSettings;
    private RemoteState RemoteStatus;
    private int SendData_Error_Count;
    private DateTime SettingsLastModTime;
    private static object ThreadMutex = new object();
    private int volLevel;

    private void ActivateDVM()
    {
      try
      {
        if (!this.DVMactive)
        {
          DVM = new DeviceVolumeMonitor(GUIGraphicsContext.form.Handle);
          if (DVM != null)
          {
            DVM.OnVolumeInserted += new DeviceVolumeAction(iMONLCDg.VolumeInserted);
            DVM.OnVolumeRemoved += new DeviceVolumeAction(iMONLCDg.VolumeRemoved);
            DVM.AsynchronousEvents = true;
            DVM.Enabled = true;
            MediaPortal.GUI.Library.Log.Debug("iMONLCDg.ActivateDVM(): DVM Activated", new object[0]);
            this.DVMactive = true;
          }
        }
      }
      catch (Exception exception)
      {
        this.DVMactive = true;
        MediaPortal.GUI.Library.Log.Debug("iMONLCDg.ActivateDVM(): caught exception: {0}", new object[] { exception });
      }
    }

    private string Add_VFD_Icons(int _line, string message)
    {
      if (!this._USE_VFD_ICONS)
      {
        return message;
      }
      if (this._USE_VFD_ICONS)
      {
        return message;
      }
      if (!_DisplayType.Equals(DisplayType.VFD))
      {
        return message;
      }
      string str = string.Empty;
      if (message.Length < 0x10)
      {
        str = message.PadRight(14, ' ');
      }
      else
      {
        str = message.Substring(0, 14);
      }
      str = str + ' ';
      if (_line == 0)
      {
        if (this.MPStatus.MP_Is_Idle || !this.MPStatus.MediaPlayer_Active)
        {
          return (str + this.IMON_VFD_CHAR_BLOCK_FILLED);
        }
        if (this.MPStatus.MediaPlayer_Playing)
        {
          if (this.MPStatus.Media_Speed < 0)
          {
            return (str + this.IMON_VFD_CHAR_RPLAY);
          }
          return (str + this.IMON_VFD_CHAR_PLAY);
        }
        if (this.MPStatus.MediaPlayer_Paused)
        {
          str = str + this.IMON_VFD_CHAR_PAUSE;
        }
        return str;
      }
      if (this.MPStatus.Media_IsRecording)
      {
        return (str + this.IMON_VFD_CHAR_RECORD);
      }
      return (str + " ");
    }

    private void AdvancedSettings_OnSettingsChanged()
    {
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.AdvancedSettings_OnSettingsChanged(): RELOADING SETTINGS", new object[0]);
      AdvancedSettings advSettings = this.AdvSettings;
      AdvancedSettings settings2 = AdvancedSettings.Load();
      bool flag = false;
      if (!advSettings.Equals(settings2))
      {
        flag = true;
      }
      if (flag)
      {
        this.CleanUp();
        Thread.Sleep(100);
      }
      this.LoadAdvancedSettings();
      if (flag)
      {
        this.Setup("", Settings.Instance.TextHeight, Settings.Instance.TextWidth, 0, Settings.Instance.GraphicHeight, Settings.Instance.GraphicWidth, 0, Settings.Instance.BackLightControl, Settings.Instance.Backlight, Settings.Instance.ContrastControl, Settings.Instance.Contrast, Settings.Instance.BlankOnExit);
        this.Initialize();
      }
    }

    private static byte BitReverse(byte inByte)
    {
      byte num = 0;
      for (byte i = 0x80; Convert.ToInt32(i) > 0; i = (byte)(i >> 1))
      {
        num = (byte)(num >> 1);
        if (((byte)(inByte & i)) != 0)
        {
          num = (byte)(num | 0x80);
        }
      }
      return num;
    }

    private void Check_Idle_State()
    {
      if (this.MPStatus.MP_Is_Idle)
      {
        if (this.DoDebug)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.DisplayLines(): _BlankDisplayWhenIdle = {0}, _BlankIdleTimeout = {1}", new object[] { this.DisplaySettings.BlankDisplayWhenIdle, this.DisplaySettings._BlankIdleTimeout });
        }
        if (this.DisplaySettings.BlankDisplayWhenIdle)
        {
          if (!this._mpIsIdle)
          {
            if (this.DoDebug)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.DisplayLines(): MP going IDLE", new object[0]);
            }
            this.DisplaySettings._BlankIdleTime = DateTime.Now.Ticks;
          }
          if (!this._IsDisplayOff && ((DateTime.Now.Ticks - this.DisplaySettings._BlankIdleTime) > this.DisplaySettings._BlankIdleTimeout))
          {
            if (this.DoDebug)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.DisplayLines(): Blanking display due to IDLE state", new object[0]);
            }
            this.DisplayOff();
          }
        }
        this._mpIsIdle = true;
      }
      else
      {
        if (this.DisplaySettings.BlankDisplayWhenIdle & this._mpIsIdle)
        {
          if (this.DoDebug)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.DisplayLines(): MP no longer IDLE - restoring display", new object[0]);
          }
          this.DisplayOn();
        }
        this._mpIsIdle = false;
      }
    }

    public void Check_iMON_Manager_Status()
    {
      Process[] processesByName;
      Process process;
      bool flag;
      bool flag2;
      MediaPortal.GUI.Library.Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): Checking iMON/VFD Manager configuration", new object[0]);
      string str = string.Empty;
      int num = -1;
      MediaPortal.GUI.Library.Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): Checking Antec VFD Manager registry subkey.", new object[0]);
      RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Antec\VFD", true);
      if (key != null)
      {
        MediaPortal.GUI.Library.Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): The Antec VFD Manager registry subkey found.", new object[0]);
        flag2 = true;
        str = (string)key.GetValue("CurRemote", string.Empty);
        num = (int)key.GetValue("MouseMode", -1);
        if ((str.Equals("iMON PAD") & (num != 0)) & this._ForceKeyBoardMode)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDgCheck_iMON_Manager_Status(): \"iMON PAD\" configuration error.", new object[0]);
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.Check_iMON_Manager_Status(): The Antec VFD Manager is not set correctly. The configuration has been corrected.", new object[0]);
          flag2 = false;
        }
        if (((int)key.GetValue("RCPlugin", -1)) != 1)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDgCheck_iMON_Manager_Status(): \"RCPlugin\" configuration error.", new object[0]);
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.Check_iMON_Manager_Status(): The Antec VFD Manager is not set correctly. The configuration has been corrected.", new object[0]);
          flag2 = false;
        }
        if (((int)key.GetValue("RunFront", -1)) != 0)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDgCheck_iMON_Manager_Status(): \"RunFront\" configuration error.", new object[0]);
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.Check_iMON_Manager_Status(): The Antec VFD Manager is not set correctly. The configuration has been corrected.", new object[0]);
          flag2 = false;
        }
        if (_ForceManagerReload)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDgCheck_iMON_Manager_Status(): Forcing Antec/iMON Manager reload...", new object[0]);
          flag2 = false;
        }
        if (!flag2)
        {
          key.SetValue("RCPlugin", 1, RegistryValueKind.DWord);
          key.SetValue("RunFront", 0, RegistryValueKind.DWord);
          if (str.Equals("iMON PAD") & (num != 0))
          {
            key.SetValue("MouseMode", 0, RegistryValueKind.DWord);
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.Check_iMON_Manager_Status(): Forcing iMON PAD remote setting to Keyboard mode.", new object[0]);
          }
          Registry.CurrentUser.Close();
          Thread.Sleep(100);
          processesByName = Process.GetProcessesByName("VFD");
          MediaPortal.GUI.Library.Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): Found {0} instances of Antec VFD Manager", new object[] { processesByName.Length });
          if (processesByName.Length > 0)
          {
            this._UsingAntecManager = true;
            MediaPortal.GUI.Library.Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): Stopping VFD Manager", new object[0]);
            processesByName[0].Kill();
            flag = false;
            while (!flag)
            {
              Thread.Sleep(100);
              MediaPortal.GUI.Library.Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): Waiting for VFD Manager to exit", new object[0]);
              processesByName[0].Dispose();
              processesByName = Process.GetProcessesByName("VFD");
              if (processesByName.Length == 0)
              {
                flag = true;
              }
            }
            MediaPortal.GUI.Library.Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): VFD Manager Stopped", new object[0]);
            MediaPortal.ProcessPlugins.MiniDisplayPlugin.Win32Functions.RedrawNotificationArea();
            process = new Process();
            process.StartInfo.WorkingDirectory = this.FindAntecManagerPath();
            process.StartInfo.FileName = "VFD.exe";
            MediaPortal.GUI.Library.Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): ReStarting VFD Manager", new object[0]);
            Process.Start(process.StartInfo);
          }
        }
        else
        {
          MediaPortal.GUI.Library.Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): The Antec VFD Manager registry entries are correct.", new object[0]);
          key.SetValue("RunFront", 0, RegistryValueKind.DWord);
          processesByName = Process.GetProcessesByName("VFD");
          MediaPortal.GUI.Library.Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): Found {0} instances of Antec VFD Manager", new object[] { processesByName.Length });
        }
        key.Close();
      }
      else
      {
        MediaPortal.GUI.Library.Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): The Antec VFD Manager registry subkey NOT FOUND.", new object[0]);
        Registry.CurrentUser.Close();
        processesByName = Process.GetProcessesByName("VFD");
        MediaPortal.GUI.Library.Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): state check: Found {0} instances of Antec VFD Manager", new object[] { processesByName.Length });
        if (processesByName.Length > 0)
        {
          MediaPortal.GUI.Library.Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): Inconsistant state: Forcing shutdown of Antec VFD Manager", new object[0]);
          processesByName[0].Kill();
          flag = false;
          while (!flag)
          {
            Thread.Sleep(100);
            MediaPortal.GUI.Library.Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): Waiting for VFD Manager to exit", new object[0]);
            processesByName[0].Dispose();
            processesByName = Process.GetProcessesByName("VFD");
            if (processesByName.Length == 0)
            {
              flag = true;
            }
          }
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.Check_iMON_Manager_Status(): Inconsistant state: Antec VFD Manager Stopped", new object[0]);
          MediaPortal.ProcessPlugins.MiniDisplayPlugin.Win32Functions.RedrawNotificationArea();
        }
      }
      MediaPortal.GUI.Library.Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): Checking SoundGraph iMON Manager registry subkey.", new object[0]);
      key = Registry.CurrentUser.OpenSubKey(@"Software\SOUNDGRAPH\iMON", true);
      if (key != null)
      {
        MediaPortal.GUI.Library.Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): The Soundgraph iMON Manager registry subkey found.", new object[0]);
        flag2 = true;
        str = (string)key.GetValue("CurRemote", string.Empty);
        num = (int)key.GetValue("MouseMode", -1);
        if ((str.Equals("iMON PAD") & (num != 0)) & this._ForceKeyBoardMode)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDgCheck_iMON_Manager_Status(): \"iMON PAD\" configuration error.", new object[0]);
          MediaPortal.GUI.Library.Log.Info("iMONLCDgCheck_iMON_Manager_Status(): The Soundgraph iMON Manager is not set correctly. The configuration has been corrected.", new object[0]);
          flag2 = false;
        }
        if (((int)key.GetValue("RCPlugin", -1)) != 1)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDgCheck_iMON_Manager_Status(): \"RCPlugin\" configuration error.", new object[0]);
          MediaPortal.GUI.Library.Log.Info("iMONLCDgCheck_iMON_Manager_Status(): The Soundgraph iMON Manager is not set correctly. The configuration has been corrected.", new object[0]);
          flag2 = false;
        }

        if (((int)key.GetValue("RunFront", -1)) != 0)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDgCheck_iMON_Manager_Status(): \"RunFront\" configuration error.", new object[0]);
          MediaPortal.GUI.Library.Log.Info("iMONLCDgCheck_iMON_Manager_Status(): The Soundgraph iMON Manager is not set correctly. The configuration has been corrected.", new object[0]);
          flag2 = false;
        }
        if (_ForceManagerReload)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDgCheck_iMON_Manager_Status(): Forcing Antec/iMON Manager reload...", new object[0]);
          flag2 = false;
        }
        if (!flag2)
        {
          key.SetValue("RCPlugin", 1, RegistryValueKind.DWord);
          key.SetValue("RunFront", 0, RegistryValueKind.DWord);
          if (str.Equals("iMON PAD") & (num != 0))
          {
            key.SetValue("MouseMode", 0, RegistryValueKind.DWord);
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.Check_iMON_Manager_Status(): Forcing iMON PAD remote setting to Keyboard mode.", new object[0]);
          }
          Registry.CurrentUser.Close();
          Thread.Sleep(100);
          processesByName = Process.GetProcessesByName("iMON");
          MediaPortal.GUI.Library.Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): Found {0} instances of SoundGraph iMON Manager", new object[] { processesByName.Length });
          if (processesByName.Length > 0)
          {
            this._UsingSoundgraphManager = true;
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.Check_iMON_Manager_Status(): Stopping iMON Manager", new object[0]);
            processesByName[0].Kill();
            flag = false;
            while (!flag)
            {
              Thread.Sleep(100);
              MediaPortal.GUI.Library.Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): Waiting for iMON Manager to exit", new object[0]);
              processesByName[0].Dispose();
              processesByName = Process.GetProcessesByName("iMON");
              if (processesByName.Length == 0)
              {
                flag = true;
              }
            }
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.Check_iMON_Manager_Status(): iMON Manager Stopped", new object[0]);
            MediaPortal.ProcessPlugins.MiniDisplayPlugin.Win32Functions.RedrawNotificationArea();
            process = new Process();
            process.StartInfo.WorkingDirectory = this.FindSoundGraphManagerPath();
            process.StartInfo.FileName = "iMON.exe";
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.Check_iMON_Manager_Status(): ReStarting iMON Manager", new object[0]);
            Process.Start(process.StartInfo);
          }
        }
        else
        {
          MediaPortal.GUI.Library.Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): The SoundGraph iMON Manager registry entries are correct.", new object[0]);
          key.SetValue("RunFront", 0, RegistryValueKind.DWord);
          processesByName = Process.GetProcessesByName("iMON");
          MediaPortal.GUI.Library.Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): Found {0} instances of SoundGraph iMON Manager", new object[] { processesByName.Length });
        }
        key.Close();
      }
      else
      {
        Registry.CurrentUser.Close();
        MediaPortal.GUI.Library.Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): SoundGraph Registry subkey NOT FOUND", new object[0]);
        processesByName = Process.GetProcessesByName("iMON");
        MediaPortal.GUI.Library.Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): STATE CHECK: Found {0} instances of SoundGraph iMON Manager", new object[] { processesByName.Length });
        if (processesByName.Length > 0)
        {
          MediaPortal.GUI.Library.Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): INCONSISTANT STATE: Forcing shutdown of SoundGraph iMON Manager", new object[0]);
          processesByName[0].Kill();
          flag = false;
          while (!flag)
          {
            Thread.Sleep(100);
            MediaPortal.GUI.Library.Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): Waiting for iMON Manager to exit", new object[0]);
            processesByName[0].Dispose();
            processesByName = Process.GetProcessesByName("iMON");
            if (processesByName.Length == 0)
            {
              flag = true;
            }
          }
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.Check_iMON_Manager_Status(): Inconsistant state: Soundgraph iMON Manager Stopped", new object[0]);
          MediaPortal.ProcessPlugins.MiniDisplayPlugin.Win32Functions.RedrawNotificationArea();
        }
      }
      MediaPortal.GUI.Library.Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): iMON/VFD Manager configuration check completed", new object[0]);
      if (this._EnsureManagerStartup)
      {
        MediaPortal.GUI.Library.Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): Ensure Manager Start is selected.. ensuring that the manager is running", new object[0]);
        Process[] processArray2 = Process.GetProcessesByName("VFD");
        Process[] processArray3 = Process.GetProcessesByName("iMON");
        string processName = string.Empty;
        if ((processArray2.Length == 0) & (processArray3.Length == 0))
        {
          Process process2 = new Process();
          string str3 = this.FindAntecManagerPath();
          string str4 = this.FindSoundGraphManagerPath();
          if ((str3 == string.Empty) & (str4 == string.Empty))
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.Check_iMON_Manager_Status(): ERROR: Unable to ensure Antec VFD or iMON Manager is running. Installation not found.", new object[0]);
          }
          else
          {
            if (str3 != string.Empty)
            {
              process2.StartInfo.WorkingDirectory = str3;
              process2.StartInfo.FileName = "VFD.exe";
              processName = "VFD";
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.Check_iMON_Manager_Status(): Starting Antec VFD Manager", new object[0]);
            }
            else if (str4 != string.Empty)
            {
              process2.StartInfo.WorkingDirectory = str4;
              process2.StartInfo.FileName = "iMON.exe";
              processName = "iMON";
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.Check_iMON_Manager_Status(): Starting iMON Manager", new object[0]);
            }
            Process.Start(process2.StartInfo);
            Thread.Sleep(0x3e8);
            int num2 = 0x1388;
            bool flag3 = false;
            while (!flag3 & (num2 > 0))
            {
              processArray2 = Process.GetProcessesByName(processName);
              if (processArray2.Length > 0)
              {
                if (processArray2[0].Responding)
                {
                  flag3 = true;
                }
                else
                {
                  Thread.Sleep(100);
                  num2 -= 100;
                }
              }
            }
            if (!flag3)
            {
              this._UsingAntecManager = false;
              this._UsingSoundgraphManager = false;
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.Check_iMON_Manager_Status(): Could not start Antec/iMON Manager process", new object[0]);
            }
            else if (processName.Equals("VFD"))
            {
              this._UsingAntecManager = true;
              this._UsingSoundgraphManager = false;
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.Check_iMON_Manager_Status(): Started Antec VFD Manager", new object[0]);
            }
            else
            {
              this._UsingAntecManager = false;
              this._UsingSoundgraphManager = true;
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.Check_iMON_Manager_Status(): Started iMON Manager", new object[0]);
            }
          }
        }
        else if (processArray2.Length > 0)
        {
          this._UsingAntecManager = true;
          this._UsingSoundgraphManager = false;
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.Check_iMON_Manager_Status(): Antec VFD Manager is running", new object[0]);
        }
        else if (processArray3.Length > 0)
        {
          this._UsingAntecManager = false;
          this._UsingSoundgraphManager = true;
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.Check_iMON_Manager_Status(): iMON Manager is running", new object[0]);
        }
      }
    }

    public void CleanUp()
    {
      if (!this._isDisabled)
      {
        MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.CleanUp(): called", new object[0]);
        AdvancedSettings.OnSettingsChanged -= new AdvancedSettings.OnSettingsChangedHandler(this.AdvancedSettings_OnSettingsChanged);
        this.CloseLcd();
        MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.CleanUp(): completed", new object[0]);
      }
    }

    public void Clear()
    {
      if (!this._isDisabled)
      {
        MediaPortal.GUI.Library.Log.Debug("iMONLCDg.Clear(): called", new object[0]);
        for (int i = 0; i < 2; i++)
        {
          this._lines[i] = new string(' ', Settings.Instance.TextWidth);
        }
        this.DisplayLines();
        MediaPortal.GUI.Library.Log.Debug("iMONLCDg.Clear(): completed", new object[0]);
      }
    }

    private void ClearDisplay()
    {
      this.Clear();
    }

    private void ClearPixels()
    {
      this.Clear();
    }

    private void CloseLcd()
    {
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.CloseLcd(): called", new object[0]);
      if (this._IMON.iMONVFD_IsInited())
      {
        if ((_DisplayType == DisplayType.LCD) || (_DisplayType == DisplayType.LCD2))
        {
          if (!this._displayTest)
          {
            while (this._iconThread.IsAlive)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.CloseLcd(): Stopping iMONLCDg.UpdateIcons() Thread", new object[0]);
              lock (ThreadMutex)
              {
                _stopUpdateIconThread = true;
              }
              _stopUpdateIconThread = true;
              Thread.Sleep(500);
            }
          }
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.CloseLcd(): Preparing for shutdown", new object[0]);
          this.SendData(Command.SetIcons);
          this.SendData(Command.SetLines0);
          this.SendData(Command.SetLines1);
          this.SendData(Command.SetLines2);
          if (this._BlankDisplayOnExit)
          {
            if (_DisplayType == DisplayType.LCD2)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.CloseLcd(): sending display shutdown command to LCD2", new object[0]);
              this.SendData(-8646911284551352312L);
              this.SendData(-8502796096475496448L);
            }
            else
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.CloseLcd(): sending display shutdown command to LCD", new object[0]);
              this.SendData(Command.Shutdown);
            }
          }
          else
          {
            ulong num;
            DateTime now = DateTime.Now;
            if (_DisplayType == DisplayType.LCD2)
            {
              MediaPortal.GUI.Library.Log.Debug("iMONLCDg.CloseLcd(): sending clock enable command to LCD2", new object[0]);
              num = 9799832789158199296L;
            }
            else
            {
              MediaPortal.GUI.Library.Log.Debug("iMONLCDg.CloseLcd(): sending clock enable command to LCD", new object[0]);
              num = 0x5000000000000000L;
            }
            num += ((ulong)now.Second) << 0x30;
            num += ((ulong)now.Minute) << 40;
            num += ((ulong)now.Hour) << 0x20;
            num += ((ulong)now.Day) << 0x18;
            num += ((ulong)now.Month) << 0x10;
            num += (ulong)((now.Year & 15L) << 8);
            num += (ulong)0x80L;
            this.SendData(num);
          }
          this.SendData(Command.KeypadLightOff);
          if (this.DisplayOptions.UseCustomFont)
          {
            this.CFont.CloseFont();
          }
          if (this.DisplayOptions.UseLargeIcons & this.DisplayOptions.UseCustomIcons)
          {
            this.CustomLargeIcon.CloseIcons();
          }
        }
        else if (_DisplayType == DisplayType.VFD)
        {
          if (this.EQSettings.UseEqDisplay || this.DisplaySettings.BlankDisplayWithVideo)
          {
            while (this._iconThread.IsAlive)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.CloseLcd(): Stoping iMONLCDg.VFD_EQ_Update() Thread", new object[0]);
              lock (ThreadMutex)
              {
                _stopUpdateIconThread = true;
              }
              _stopUpdateIconThread = true;
              Thread.Sleep(500);
            }
          }
          if (this._BlankDisplayOnExit)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.CloseLcd(): Shutting down VFD display!!", new object[0]);
            this.SetText("", "");
          }
          else
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.CloseLcd(): Sending Shutdown message to VFD display!!", new object[0]);
            if ((this.DisplaySettings._Shutdown1 != string.Empty) || (this.DisplaySettings._Shutdown2 != string.Empty))
            {
              this.SetText(this.DisplaySettings._Shutdown1, this.DisplaySettings._Shutdown2);
            }
            else
            {
              this.SetText("   MediaPortal  ", "   not active   ");
              this.SetVFDClock();
            }
          }
        }
        else if (_DisplayType == DisplayType.ThreeRsystems)
        {
          if (this.EQSettings.UseEqDisplay || this.DisplaySettings.BlankDisplayWithVideo)
          {
            while (this._iconThread.IsAlive)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.CloseLcd(): Stoping iMONLCDg.VFD_EQ_Update() Thread", new object[0]);
              lock (ThreadMutex)
              {
                _stopUpdateIconThread = true;
              }
              _stopUpdateIconThread = true;
              Thread.Sleep(500);
            }
          }
          if (this._BlankDisplayOnExit)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.CloseLcd(): Sending Shutdown message to LCD3R display!!", new object[0]);
            if (this.DisplaySettings._Shutdown1 != string.Empty)
            {
              this.SendText3R(this.DisplaySettings._Shutdown1);
            }
            else
            {
              this.SendData((long)0x21c000000000000L);
              this.SendData((long)2L);
              this.SendText3R(" not active ");
            }
          }
          else
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.CloseLcd(): Shutting down LCD3R display (with clock)!!", new object[0]);
            this.SendData((long)0x21c010000000000L);
            this.SendData((long)2L);
          }
        }
        this._IMON.iMONVFD_Uninit();
      }
      else
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.CloseLcd(): Display is not open!!", new object[0]);
      }
      if (this._MonitorPower && !this._IsHandlingPowerEvent)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.CloseLcd(): Removing Power State Monitor callback from system event thread", new object[0]);
        SystemEvents.PowerModeChanged -= new PowerModeChangedEventHandler(this.SystemEvents_PowerModeChanged);
      }
      this.Remote_Stop();
      this.RestartFrontview();
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.CloseLcd(): completed", new object[0]);
    }

    public void Configure()
    {
      Form form = new iMONLCDg_AdvancedSetupForm();
      form.ShowDialog();
      form.Dispose();
    }

    private ulong ConvertPluginIconsToDriverIcons(ulong IconMask)
    {
      return (IconMask & ((ulong)0xffffffffffL));
    }

    private void DisplayEQ()
    {
      if (!(this.EQSettings.UseEqDisplay & this.EQSettings._EqDataAvailable))
      {
        return;
      }
      if (this.EQSettings.RestrictEQ & ((DateTime.Now.Ticks - this.EQSettings._LastEQupdate.Ticks) < this.EQSettings._EqUpdateDelay))
      {
        return;
      }
      if (this.DoDebug)
      {
        MediaPortal.GUI.Library.Log.Info("\niMONLCDg.DisplayEQ(): Retrieved {0} samples of Equalizer data.", new object[] { this.EQSettings.EqFftData.Length / 2 });
      }
      if ((this.EQSettings.UseStereoEq || this.EQSettings.UseVUmeter) || this.EQSettings.UseVUmeter2)
      {
        if (this.EQSettings.UseStereoEq)
        {
          this.EQSettings.Render_MaxValue = 100;
          this.EQSettings.Render_BANDS = 8;
          this.EQSettings.EqArray[0] = 0x63;
          if ((_DisplayType == DisplayType.LCD) || (_DisplayType == DisplayType.LCD2))
          {
            this.EQSettings.Render_MaxValue = (this.EQSettings._useEqMode == 2) ? 8 : 0x10;
            this.EQSettings.EqArray[0] = (byte)this.EQSettings._useEqMode;
          }
          else if (_DisplayType == DisplayType.ThreeRsystems)
          {
            this.EQSettings.Render_MaxValue = 6;
            this.EQSettings.EqArray[0] = 0;
          }
          MiniDisplayHelper.ProcessEqData(ref this.EQSettings);
          for (int i = 0; i < this.EQSettings.Render_BANDS; i++)
          {
            switch (this.EQSettings.EqArray[0])
            {
              case 2:
                {
                  byte num2 = (byte)(this.EQSettings.EqArray[1 + i] & 15);
                  this.EQSettings.EqArray[1 + i] = (byte)((num2 << 4) | num2);
                  byte num3 = (byte)(this.EQSettings.EqArray[9 + i] & 15);
                  this.EQSettings.EqArray[9 + i] = (byte)((num3 << 4) | num3);
                  break;
                }
            }
          }
          for (int j = 15; j > 7; j--)
          {
            this.EQSettings.EqArray[j + 1] = this.EQSettings.EqArray[j];
          }
          this.EQSettings.EqArray[8] = 0;
          this.EQSettings.EqArray[9] = 0;
        }
        else
        {
          this.EQSettings.Render_MaxValue = 80;
          this.EQSettings.Render_BANDS = 1;
          if ((_DisplayType == DisplayType.LCD) || (_DisplayType == DisplayType.LCD2))
          {
            this.EQSettings.Render_MaxValue = 0x60;
            if (this.EQSettings._useVUindicators)
            {
              this.EQSettings.Render_MaxValue = 0x60;
            }
          }
          else if (this.EQSettings._useVUindicators)
          {
            this.EQSettings.Render_MaxValue = 0x4b;
          }
          MiniDisplayHelper.ProcessEqData(ref this.EQSettings);
        }
      }
      else
      {
        this.EQSettings.Render_MaxValue = 100;
        this.EQSettings.Render_BANDS = 0x10;
        this.EQSettings.EqArray[0] = 0x63;
        if ((_DisplayType == DisplayType.LCD) || (_DisplayType == DisplayType.LCD2))
        {
          this.EQSettings.Render_MaxValue = (this.EQSettings._useEqMode == 2) ? 8 : 0x10;
          this.EQSettings.EqArray[0] = (byte)this.EQSettings._useEqMode;
        }
        else if (_DisplayType == DisplayType.ThreeRsystems)
        {
          this.EQSettings.Render_MaxValue = 6;
          this.EQSettings.EqArray[0] = 0;
        }
        MiniDisplayHelper.ProcessEqData(ref this.EQSettings);
        for (int k = 0; k < this.EQSettings.Render_BANDS; k++)
        {
          switch (this.EQSettings.EqArray[0])
          {
            case 2:
              {
                byte num6 = (byte)(this.EQSettings.EqArray[1 + k] & 15);
                this.EQSettings.EqArray[1 + k] = (byte)((num6 << 4) | num6);
                break;
              }
          }
        }
      }
      if ((_DisplayType == DisplayType.LCD) || (_DisplayType == DisplayType.LCD2))
      {
        if (!this.EQSettings.UseVUmeter && !this.EQSettings.UseVUmeter2)
        {
          this.SetEQ(this.EQSettings.EqArray);
        }
        else
        {
          this.DrawVU(this.EQSettings.EqArray);
        }
      }
      else if (_DisplayType == DisplayType.ThreeRsystems)
      {
        for (int m = 0; m < 8; m++)
        {
          this.EQSettings.EqArray[1 + m] = (byte)((this.EQSettings.EqArray[1 + m] << 4) + this.EQSettings.EqArray[9 + m]);
        }
        ulong data = 0x901000000000000L;
        ulong num9 = 2L;
        data = data + this.EQSettings.EqArray[1] << 40;
        data = data + this.EQSettings.EqArray[2] << 0x20;
        data = data + this.EQSettings.EqArray[3] << 0x18;
        data = data + this.EQSettings.EqArray[4] << 0x10;
        data = data + this.EQSettings.EqArray[5] << 8;
        num9 = num9 + this.EQSettings.EqArray[6] << 40;
        num9 = num9 + this.EQSettings.EqArray[7] << 0x20;
        num9 = num9 + this.EQSettings.EqArray[8] << 0x18;
        this.SendData((long)0x200020000000000L);
        this.SendData((long)2L);
        this.SendData((long)0xd0f202020202000L);
        this.SendData((long)0x2020202020202002L);
        this.SendData(data);
        this.SendData(num9);
      }
      else
      {
        if (!this.EQSettings.UseVUmeter && !this.EQSettings.UseVUmeter2)
        {
          int[] destinationArray = new int[0x10];
          Array.Copy(this.EQSettings.EqArray, 1, destinationArray, 0, 0x10);
          lock (DWriteMutex)
          {
            this._IMON.iMONVFD_SetEQ(destinationArray);
            goto Label_0613;
          }
        }
        this.DrawVU(this.EQSettings.EqArray);
      }
    Label_0613:
      this.EQSettings._LastEQupdate = DateTime.Now;
      if ((DateTime.Now.Ticks - this.EQSettings._EQ_FPS_time.Ticks) < 0x989680L)
      {
        this.EQSettings._EQ_Framecount++;
      }
      else
      {
        this.EQSettings._Max_EQ_FPS = Math.Max(this.EQSettings._Max_EQ_FPS, this.EQSettings._EQ_Framecount);
        this.EQSettings._EQ_Framecount = 0;
        this.EQSettings._EQ_FPS_time = DateTime.Now;
      }
    }

    private void DisplayLines()
    {
      this.UpdateAdvancedSettings();
      if (this.DoDebug)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.DisplayLines(): Sending text to display type {0}", new object[] { DisplayType.TypeName(_DisplayType) });
      }
      try
      {
        MiniDisplayHelper.GetSystemStatus(ref this.MPStatus);
        this.Check_Idle_State();
        if (this.EQSettings._EqDataAvailable || this._IsDisplayOff)
        {
          if (this.DoDebug)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.DisplayLines(): Suppressing display update!", new object[0]);
          }
        }
        else if ((_DisplayType == DisplayType.LCD) || (_DisplayType == DisplayType.LCD2))
        {
          if (this.DoDebug)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.DisplayLines(): Calling SendText() to emulate VFD for {0}", new object[] { DisplayType.TypeName(_DisplayType) });
          }
          this.SendText(this._lines[0], this._lines[1]);
        }
        else if (_DisplayType == DisplayType.VFD)
        {
          if (this.DoDebug)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.DisplayLines(): Calling SetText()", new object[0]);
          }
          this.SetText(this._lines[0], this._lines[1]);
        }
        else if (_DisplayType == DisplayType.ThreeRsystems)
        {
          this.SendText3R(this._lines[0]);
        }
      }
      catch (Exception exception)
      {
        MediaPortal.GUI.Library.Log.Debug("iMONLCDg.DisplayLines(): CAUGHT EXCEPTION {0}", new object[] { exception });
      }
    }

    private void DisplayOff()
    {
      if (!this._IsDisplayOff)
      {
        if (this.DisplaySettings.EnableDisplayAction & this.DisplaySettings._DisplayControlAction)
        {
          if ((DateTime.Now.Ticks - this.DisplaySettings._DisplayControlLastAction) < this.DisplaySettings._DisplayControlTimeout)
          {
            if (this.DoDebug)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.DisplayOff(): DisplayControlAction Timer = {0}.", new object[] { DateTime.Now.Ticks - this.DisplaySettings._DisplayControlLastAction });
            }
            return;
          }
          if (this.DoDebug)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.DisplayOff(): DisplayControlAction Timeout expired.", new object[0]);
          }
          this.DisplaySettings._DisplayControlAction = false;
          this.DisplaySettings._DisplayControlLastAction = 0L;
        }
        if (this.DoDebug)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.DisplayOff(): called", new object[0]);
        }
        lock (DWriteMutex)
        {
          if ((_DisplayType == DisplayType.LCD) || (_DisplayType == DisplayType.LCD2))
          {
            if (this.DoDebug)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.DisplayOff(): Sending Shutdown command to LCD", new object[0]);
            }
            if (_DisplayType == DisplayType.LCD2)
            {
              this.SendData(-8646911284551352312L);
            }
            else
            {
              this.SendData(Command.Shutdown);
            }
          }
          else
          {
            if (this.DoDebug)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.DisplayOff(): Sending blank display to VFD", new object[0]);
            }
            this._IMON.iMONVFD_SetText(new string(' ', 0x10), new string(' ', 0x10));
          }
          this._IsDisplayOff = true;
        }
        if (this.DoDebug)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.DisplayOff(): completed", new object[0]);
        }
      }
    }

    private void DisplayOn()
    {
      if (!this._IsDisplayOff)
      {
        return;
      }
      if (this.DoDebug)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.DisplayOn(): called", new object[0]);
      }
      if ((_DisplayType == DisplayType.LCD) || (_DisplayType == DisplayType.LCD2))
      {
        lock (DWriteMutex)
        {
          if (this.DoDebug)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.DisplayOn(): Sending Display ON command to LCD", new object[0]);
          }
          this._IsDisplayOff = false;
          if (_DisplayType == DisplayType.LCD2)
          {
            this.SendData(-8646911284551352256L);
            this.SendData(-8646911284551352256L);
            this.SendData(-8502796096475496448L);
            this.SendData(Command.SetContrast, this._ContrastLevel);
            this.SendData(-8791026472627208192L);
            this.SendData(Command.SetIcons);
            this.SendData(Command.SetLines0);
            this.SendData(Command.SetLines1);
            this.SendData(Command.SetLines2);
            this.ClearDisplay();
            this.SendData(-8358680908399640433L);
          }
          else
          {
            this.SendData(Command.DisplayOn);
          }
          goto Label_0150;
        }
      }
      lock (DWriteMutex)
      {
        this._IsDisplayOff = false;
      }
    Label_0150:
      if (this.DoDebug)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.DisplayOn(): called", new object[0]);
      }
    }

    public void Dispose()
    {
      MediaPortal.GUI.Library.Log.Debug("iMONLCDg.Dispose(): called", new object[0]);
      //
      // If IRSS (Input Remote Server Suite by and-81) is installed
      // we need to restart the service to re-register dll handler
      //
      string irss_srv = "InputService";
      foreach (ServiceController ctrl in ServiceController.GetServices())
      {
        if (ctrl.ServiceName.ToLower() == irss_srv.ToLower())
        {
          MediaPortal.GUI.Library.Log.Debug("iMONLCDg.Dispose(): Restarting \"" + irss_srv + "\" from IRSS", new object[0]);
          ctrl.Stop();
          ctrl.WaitForStatus(ServiceControllerStatus.Stopped);
          ctrl.Start();
          ctrl.WaitForStatus(ServiceControllerStatus.Running);
        }
      }
      MediaPortal.GUI.Library.Log.Debug("iMONLCDg.Dispose(): completed", new object[0]);
    }

    public void DoDisplayTest()
    {
      BuiltinIconMask mask = new BuiltinIconMask();
      if ((_DisplayType == DisplayType.LCD) || (_DisplayType == DisplayType.LCD2))
      {
        MediaPortal.GUI.Library.Log.Debug("(IDisplay) iMONLCDg.Setup() configure - do display test", new object[0]);
        this._displayTest = true;
        this.OpenLcd();
        this.ClearDisplay();
        Thread.Sleep(500);
        this.SendText("iMONLCDg", "Display Test");
        Thread.Sleep(500);
        this.SendText("iMONLCDg", "All Icons");
        for (int i = 0; i < 2; i++)
        {
          this.SendData(Command.SetIcons, mask.ICON_ALL);
          Thread.Sleep(500);
          this.SendData(Command.SetIcons);
          Thread.Sleep(500);
        }
        DiskIcon icon = new DiskIcon();
        icon.Reset();
        icon.On();
        this.SendText("iMONLCDg", "Disk On");
        Thread.Sleep(500);
        this.SendText("iMONLCDg", "Disk Spin CW");
        icon.RotateCW();
        for (int j = 0; j < 0x10; j++)
        {
          icon.Animate();
          this.SendData(Command.SetIcons, icon.Mask);
          Thread.Sleep(250);
        }
        this.SendText("iMONLCDg", "Disk Spin CCW");
        icon.RotateCCW();
        for (int k = 0; k < 0x10; k++)
        {
          icon.Animate();
          this.SendData(Command.SetIcons, icon.Mask);
          Thread.Sleep(250);
        }
        this.SendText("iMONLCDg", "Disk Flash");
        icon.RotateOff();
        icon.FlashOn();
        for (int m = 0; m < 0x10; m++)
        {
          icon.Animate();
          this.SendData(Command.SetIcons, icon.Mask);
          Thread.Sleep(250);
        }
        this.CloseLcd();
        this._displayTest = false;
        MediaPortal.GUI.Library.Log.Debug("(IDisplay) iMONLCDg.Setup() configure - display test complete", new object[0]);
      }
    }

    public void DrawImage(Bitmap bitmap)
    {
      if (!this._isDisabled)
      {
        if (this.EQSettings._EqDataAvailable || this._IsDisplayOff)
        {
          if (this.DoDebug)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.DrawImage(): Suppressing display update!", new object[0]);
          }
        }
        else
        {
          if (this.DoDebug)
          {
            MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.DrawImage(): called", new object[0]);
          }
          if (bitmap == null)
          {
            MediaPortal.GUI.Library.Log.Debug("(IDisplay) iMONLCDg.DrawImage():  bitmap null", new object[0]);
          }
          else
          {
            BitmapData bitmapdata = bitmap.LockBits(new Rectangle(new Point(0, 0), bitmap.Size), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            try
            {
              if (this.bitmapData == null)
              {
                this.bitmapData = new byte[bitmapdata.Stride * this._grows];
              }
              Marshal.Copy(bitmapdata.Scan0, this.bitmapData, 0, this.bitmapData.Length);
            }
            finally
            {
              bitmap.UnlockBits(bitmapdata);
            }
            byte[] buffer = this._sha256.ComputeHash(this.bitmapData);
            if (ByteArray.AreEqual(buffer, this._lastHash))
            {
              if (this.DoDebug)
              {
                MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.DrawImage():  bitmap not changed", new object[0]);
              }
            }
            else
            {
              this.UpdateAdvancedSettings();
              byte[] pixelArray = new byte[0xc0];
              for (int i = 0; i < (this._gcols - 1); i++)
              {
                pixelArray[i] = 0;
                pixelArray[i + 0x60] = 0;
                for (int j = 0; j < 8; j++)
                {
                  int index = (j * bitmapdata.Stride) + (i * 4);
                  if (System.Drawing.Color.FromArgb(this.bitmapData[index + 2], this.bitmapData[index + 1], this.bitmapData[index]).GetBrightness() < 0.5f)
                  {
                    pixelArray[i] = (byte)(pixelArray[i] | ((byte)(((int)1) << (7 - j))));
                  }
                }
                for (int k = 8; k < 0x10; k++)
                {
                  int num5 = (k * bitmapdata.Stride) + (i * 4);
                  if (System.Drawing.Color.FromArgb(this.bitmapData[num5 + 2], this.bitmapData[num5 + 1], this.bitmapData[num5]).GetBrightness() < 0.5f)
                  {
                    pixelArray[i + 0x60] = (byte)(pixelArray[i + 0x60] | ((byte)(((int)1) << (15 - k))));
                  }
                }
              }
              this.SendPixelArray(pixelArray);
              if (this.DoDebug)
              {
                MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.DrawImage(): Sending pixel array to iMON Handler", new object[0]);
              }
              this._lastHash = buffer;
            }
          }
        }
      }
    }

    private void DrawVU(byte[] EqDataArray)
    {
      if (this.DoDebug)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.DrawVU(): Called", new object[0]);
      }
      if ((_DisplayType != DisplayType.LCD) && (_DisplayType != DisplayType.LCD2))
      {
        if (this.DoDebug)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.DrawVU(): Drawing VU meter for VFD display", new object[0]);
        }
        string firstLine = "";
        string secondLine = "";
        char ch = this.IMON_CHAR_6_BARS;
        int num7 = 0x10;
        if (this.EQSettings._useVUindicators)
        {
          if (this.EQSettings.UseVUmeter)
          {
            firstLine = "L";
            secondLine = "R";
          }
          else
          {
            firstLine = "L";
          }
          num7 = 15;
        }
        for (int i = 0; i < num7; i++)
        {
          if (EqDataArray[1] == 0)
          {
            firstLine = firstLine + ' ';
          }
          else if (((i + 1) * 5) < EqDataArray[1])
          {
            firstLine = firstLine + ch;
          }
          if (EqDataArray[2] == 0)
          {
            secondLine = secondLine + ' ';
          }
          else if (this.EQSettings.UseVUmeter)
          {
            if (((i + 1) * 5) < EqDataArray[2])
            {
              secondLine = secondLine + ch;
            }
          }
          else if (EqDataArray[2] > ((num7 - i) * 5))
          {
            secondLine = secondLine + ch;
          }
          else
          {
            secondLine = secondLine + ' ';
          }
        }
        if (this.EQSettings.UseVUmeter2 && this.EQSettings._useVUindicators)
        {
          secondLine = secondLine + "R";
        }
        if (this.DoDebug)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.DrawVU(): Sending VU meter data to display: L = \"{0}\" - R = \"{1}\"", new object[] { firstLine, secondLine });
        }
        this._IMON.iMONVFD_SetText(firstLine, secondLine);
      }
      else
      {
        if (this.DoDebug)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.DrawVU(): Drawing Graphical VU meter for LCD display", new object[0]);
        }
        int num = 0x60;
        int num2 = 0;
        byte[] pixelArray = new byte[0xc0];
        if (this.EQSettings._useVUindicators)
        {
          num = 0x58;
          for (int m = 5; m >= 0; m--)
          {
            if ((m + num2) < 0x60)
            {
              if (this.DisplayOptions.UseCustomFont)
              {
                pixelArray[num2 + m] = BitReverse(this.CFont.PixelData(0x4c, m));
              }
              else
              {
                pixelArray[num2 + m] = BitReverse(_Font8x5[0x4c, m]);
              }
            }
          }
          num2 += 8;
        }
        for (int j = 0; j < 0x60; j++)
        {
          if ((EqDataArray[1] - 1) <= j)
          {
            break;
          }
          pixelArray[num2 + j] = 0x7e;
        }
        num2 = 0x60;
        if (this.EQSettings._useVUindicators)
        {
          for (int n = 5; n >= 0; n--)
          {
            if (this.DisplayOptions.UseCustomFont)
            {
              if (this.EQSettings.UseVUmeter)
              {
                pixelArray[num2 + n] = BitReverse(this.CFont.PixelData(0x52, n));
              }
              else
              {
                pixelArray[(num2 + 90) + n] = BitReverse(this.CFont.PixelData(0x52, n));
              }
            }
            else if (this.EQSettings.UseVUmeter)
            {
              pixelArray[num2 + n] = BitReverse(_Font8x5[0x52, n]);
            }
            else
            {
              pixelArray[(num2 + 90) + n] = BitReverse(_Font8x5[0x52, n]);
            }
          }
          if (this.EQSettings.UseVUmeter)
          {
            num2 += 8;
          }
        }
        for (int k = 0; k < num; k++)
        {
          if ((EqDataArray[2] - 1) <= k)
          {
            break;
          }
          if (this.EQSettings.UseVUmeter)
          {
            pixelArray[num2 + k] = 0x7e;
          }
          else
          {
            pixelArray[(num2 + (num - 1)) - k] = 0x7e;
          }
        }
        this.SendPixelArrayRaw(pixelArray);
      }
      if (this.DoDebug)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.DrawVU(): completed", new object[0]);
      }
    }

    public string FindAntecManagerPath()
    {
      if (this.DoDebug)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.FindAntecManagerPath(): called.", new object[0]);
      }
      string str = string.Empty;
      if (Registry.CurrentUser.OpenSubKey(@"Software\Antec\VFD", false) != null)
      {
        Registry.CurrentUser.Close();
        RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\VFD.exe", false);
        if (key != null)
        {
          str = (string)key.GetValue("Path", string.Empty);
        }
        Registry.LocalMachine.Close();
      }
      else
      {
        Registry.CurrentUser.Close();
      }
      if (this.DoDebug)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.FindAntecManagerPath(): selected path = \"{0}\".", new object[] { str });
      }
      return str;
    }

    public string FindImonVFDdll()
    {
      RegistryKey key;
      string str;
      if (this.DoDebug)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.FindImonVFDdll(): called.", new object[0]);
      }
      bool flag = false;
      bool flag2 = false;
      string str2 = string.Empty;
      string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
      string str4 = string.Empty;
      string str5 = string.Empty;
      if (Registry.CurrentUser.OpenSubKey(@"Software\Antec\VFD", false) != null)
      {
        if (this.DoDebug)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.FindImonVFDdll(): found Antec registry keys.", new object[0]);
        }
        flag = true;
        Registry.CurrentUser.Close();
        key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\VFD.exe", false);
        if (key != null)
        {
          str4 = (string)key.GetValue("Path", string.Empty);
          if (str4 == string.Empty)
          {
            if (this.DoDebug)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.FindImonVFDdll(): Antec file Path registry key not found. trying default path", new object[0]);
            }
            str4 = folderPath + @"\Antec\VFD";
          }
          else if (this.DoDebug)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.FindImonVFDdll(): found Antec file Path registry key.", new object[0]);
          }
        }
        else
        {
          if (this.DoDebug)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.FindImonVFDdll(): Antec file Path registry key not found. trying default path", new object[0]);
          }
          str4 = folderPath + @"\Antec\VFD";
        }
        Registry.LocalMachine.Close();
      }
      else
      {
        Registry.CurrentUser.Close();
      }
      if (Registry.CurrentUser.OpenSubKey(@"Software\SOUNDGRAPH\iMON", false) != null)
      {
        if (this.DoDebug)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.FindImonVFDdll(): found SoundGraph registry keys.", new object[0]);
        }
        flag2 = true;
        Registry.CurrentUser.Close();
        key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\iMON.exe", false);
        if (key != null)
        {
          str5 = (string)key.GetValue("Path", string.Empty);
          if (str5 == string.Empty)
          {
            if (this.DoDebug)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.FindImonVFDdll(): SoundGraph file Path registry key not found. trying default path", new object[0]);
            }
            str5 = folderPath + @"\SoundGraph\iMON";
          }
          else if (this.DoDebug)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.FindImonVFDdll(): found SoundGraph file Path registry key.", new object[0]);
          }
        }
        else
        {
          if (this.DoDebug)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.FindImonVFDdll(): SoundGraph file Path registry key not found. trying default path", new object[0]);
          }
          str5 = folderPath + @"\Antec\VFD";
        }
        Registry.LocalMachine.Close();
      }
      else
      {
        Registry.CurrentUser.Close();
      }
      if (flag & !flag2)
      {
        str = str4 + @"\sg_vfd.dll";
        if (File.Exists(str))
        {
          if (this.DoDebug)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.FindImonVFDdll(): Selected Antec DLL.", new object[0]);
          }
          str2 = str;
        }
      }
      else if (!flag & flag2)
      {
        str = str5 + @"\sg_vfd.dll";
        if (File.Exists(str))
        {
          if (this.DoDebug)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.FindImonVFDdll(): Selected SoundGraph DLL.", new object[0]);
          }
          str2 = str;
        }
      }
      else
      {
        str = str4 + @"\sg_vfd.dll";
        if (File.Exists(str))
        {
          if (this.DoDebug)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.FindImonVFDdll(): Picked Antec DLL.", new object[0]);
          }
          str2 = str;
        }
        else
        {
          str = str5 + @"\sg_vfd.dll";
          if (File.Exists(str))
          {
            if (this.DoDebug)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.FindImonVFDdll(): Picked Soundgraph DLL.", new object[0]);
            }
            str2 = str;
          }
        }
      }
      if (this.DoDebug)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.FindImonVFDdll(): completed - selected file \"{0}\".", new object[] { str2 });
      }
      return str2;
    }

    public string FindSoundGraphManagerPath()
    {
      if (this.DoDebug)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.FindSoundGraphManagerPath(): called.", new object[0]);
      }
      string str = string.Empty;
      if (Registry.CurrentUser.OpenSubKey(@"Software\SOUNDGRAPH\iMON", false) != null)
      {
        Registry.CurrentUser.Close();
        RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\iMON.exe", false);
        if (key != null)
        {
          str = (string)key.GetValue("Path", string.Empty);
        }
        Registry.LocalMachine.Close();
      }
      else
      {
        Registry.CurrentUser.Close();
      }
      if (this.DoDebug)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.FindSoundGraphManagerPath(): selected path = \"{0}\".", new object[] { str });
      }
      return str;
    }

    private void FireRemoteEvent(byte KeyCode)
    {
      int btnCode = KeyCode;
      if (this.DoDebug)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.FireRemoteEvent(): called", new object[0]);
      }
      if (!this._inputHandler.MapAction(btnCode))
      {
        if (this.DoDebug)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.FireRemoteEvent(): No button mapping for remote button = {0}", new object[] { btnCode.ToString("x00") });
        }
      }
      else if (this.DoDebug)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.FireRemoteEvent(): fired event for remote button = {0}", new object[] { btnCode.ToString("x00") });
      }
      if (this.DoDebug)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.FireRemoteEvent(): completed", new object[0]);
      }
    }

    public void ForceManagerRestart()
    {
      string str = string.Empty;
      string str2 = string.Empty;
      if (!this._ForceManagerRestart)
      {
        if (this.DoDebug)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.ForceManagerRestart(): Option not selected... restart not required.", new object[0]);
        }
      }
      else
      {
        str = this.FindAntecManagerPath();
        str2 = this.FindSoundGraphManagerPath();
        if (str.Equals(string.Empty) & str2.Equals(string.Empty))
        {
          if (this.DoDebug)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.ForceManagerRestart(): Manager installation not found... restart not possible.", new object[0]);
          }
        }
        else
        {
          Process[] processesByName;
          Process process;
          bool flag;
          RegistryKey key;
          if (!str.Equals(string.Empty))
          {
            if (this._ForceKeyBoardMode)
            {
              key = Registry.CurrentUser.OpenSubKey(@"Software\ANTEC\VFD", true);
              if ((key != null) && key.GetValue("CurRemote").Equals("iMON PAD"))
              {
                MediaPortal.GUI.Library.Log.Debug("iMONLCDg.ForceManagerRestart(): Forcing iMON PAD mode setting to KeyBoard.", new object[0]);
                key.SetValue("MouseMode", 0, RegistryValueKind.DWord);
              }
              key.Close();
              Registry.CurrentUser.Close();
            }
            processesByName = Process.GetProcessesByName("VFD");
            if (processesByName.Length > 0)
            {
              this._UsingAntecManager = true;
              if (this.DoDebug)
              {
                MediaPortal.GUI.Library.Log.Info("iMONLCDg.ForceManagerRestart(): Found Antec Manager process.", new object[0]);
              }
              processesByName[0].Kill();
              flag = false;
              while (!flag)
              {
                Thread.Sleep(100);
                MediaPortal.GUI.Library.Log.Debug("iMONLCDg.ForceManagerRestart(): Waiting for VFD Manager to exit", new object[0]);
                processesByName[0].Dispose();
                processesByName = Process.GetProcessesByName("VFD");
                if (processesByName.Length == 0)
                {
                  flag = true;
                }
              }
              MediaPortal.GUI.Library.Log.Debug("iMONLCDg.ForceManagerRestart(): VFD Manager Stopped", new object[0]);
              MediaPortal.ProcessPlugins.MiniDisplayPlugin.Win32Functions.RedrawNotificationArea();
              process = new Process();
              process.StartInfo.WorkingDirectory = str;
              process.StartInfo.FileName = "VFD.exe";
              MediaPortal.GUI.Library.Log.Debug("iMONLCDg.ForceManagerRestart(): ReStarting VFD Manager", new object[0]);
              Process.Start(process.StartInfo);
              GUIGraphicsContext.form.Activate();
            }
          }
          if (!str2.Equals(string.Empty))
          {
            if (this._ForceKeyBoardMode)
            {
              key = Registry.CurrentUser.OpenSubKey(@"Software\SOUNDGRAPH\iMON", true);
              if ((key != null) && key.GetValue("CurRemote").Equals("iMON PAD"))
              {
                MediaPortal.GUI.Library.Log.Debug("iMONLCDg.ForceManagerRestart(): Forcing iMON PAD mode setting to KeyBoard.", new object[0]);
                key.SetValue("MouseMode", 0, RegistryValueKind.DWord);
              }
              key.Close();
              Registry.CurrentUser.Close();
            }
            processesByName = Process.GetProcessesByName("iMON");
            if (processesByName.Length > 0)
            {
              this._UsingSoundgraphManager = true;
              if (this.DoDebug)
              {
                MediaPortal.GUI.Library.Log.Info("iMONLCDg.ForceManagerRestart(): Found iMON Manager process.", new object[0]);
              }
              processesByName[0].Kill();
              flag = false;
              while (!flag)
              {
                Thread.Sleep(100);
                MediaPortal.GUI.Library.Log.Debug("iMONLCDg.ForceManagerRestart(): Waiting for iMON Manager to exit", new object[0]);
                processesByName[0].Dispose();
                processesByName = Process.GetProcessesByName("iMON");
                if (processesByName.Length == 0)
                {
                  flag = true;
                }
              }
              MediaPortal.GUI.Library.Log.Debug("iMONLCDg.ForceManagerRestart(): iMON Manager Stopped", new object[0]);
              MediaPortal.ProcessPlugins.MiniDisplayPlugin.Win32Functions.RedrawNotificationArea();
              process = new Process();
              process.StartInfo.WorkingDirectory = str2;
              process.StartInfo.FileName = "iMON.exe";
              MediaPortal.GUI.Library.Log.Debug("iMONLCDg.ForceManagerRestart(): ReStarting iMON Manager", new object[0]);
              Process.Start(process.StartInfo);
              GUIGraphicsContext.form.Activate();
            }
          }
        }
      }
    }

    private static bool GetDisplayInfoFromFirmware(int FWVersion)
    {
      bool flag = Assembly.GetEntryAssembly().FullName.Contains("Configuration") | Settings.Instance.ExtensiveLogging;
      if (flag)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.GetDisplayInfoFromFirmware(): Searching for Firmware version = {0}", new object[] { FWVersion.ToString("x00") });
      }
      for (int i = 0; (_iMON_FW_Display[i, 0] != 0) & (i < _iMON_FW_Display.Length); i++)
      {
        if (_iMON_FW_Display[i, 1] == 0)
        {
          if (_iMON_FW_Display[i, 0] == FWVersion)
          {
            _VfdType = _iMON_FW_Display[i, 2];
            _VfdReserved = _iMON_FW_Display[i, 3];
            _DisplayType = _iMON_FW_Display[i, 4];
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.GetDisplayInfoFromFirmware(): Found version match - FW: {0}, iMON Type: {1}, Reserved: {2}, Display Type: {3}", new object[] { FWVersion.ToString("x00"), _VfdType.ToString("x00"), _VfdReserved.ToString("x00"), DisplayType.TypeName(_DisplayType) });
            return true;
          }
        }
        else
        {
          int num2 = _iMON_FW_Display[i, 0];
          int num3 = _iMON_FW_Display[i, 1];
          if (((FWVersion == num2) || (FWVersion == num3)) || ((FWVersion > num2) && (FWVersion < num3)))
          {
            _VfdType = _iMON_FW_Display[i, 2];
            _VfdReserved = _iMON_FW_Display[i, 3];
            _DisplayType = _iMON_FW_Display[i, 4];
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.GetDisplayInfoFromFirmware(): Found version match - FW: {0}, iMON Type: {1}, Reserved: {2}, Display Type: {3}", new object[] { FWVersion.ToString("x00"), _VfdType.ToString("x00"), _VfdReserved.ToString("x00"), DisplayType.TypeName(_DisplayType) });
            return true;
          }
        }
      }
      if (flag)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.GetDisplayInfoFromFirmware(): version match NOT FOUND", new object[0]);
      }
      return false;
    }

    private static bool GetDisplayInfoFromRegistry(int REGVersion)
    {
      bool flag = Assembly.GetEntryAssembly().FullName.Contains("Configuration") | Settings.Instance.ExtensiveLogging;
      if (flag)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.GetDisplayInfoFromRegistry(): searching for display type {0}", new object[] { REGVersion.ToString("x00") });
      }
      for (int i = 0; (_iMON_FW_Display[i, 0] != 0) & (i < _iMON_FW_Display.Length); i++)
      {
        if (_iMON_FW_Display[i, 2] == REGVersion)
        {
          if (flag)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.GetDisplayInfoFromRegistry(): Found display type match", new object[0]);
          }
          _VfdType = _iMON_FW_Display[i, 2];
          _VfdReserved = _iMON_FW_Display[i, 3];
          _DisplayType = _iMON_FW_Display[i, 4];
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.GetDisplayInfoFromRegistry(): Found display type match - iMON Type: {0}, Reserved: {1}, Display Type: {2}", new object[] { _VfdType.ToString("x00"), _VfdReserved.ToString("x00"), DisplayType.TypeName(_DisplayType) });
          return true;
        }
      }
      if (flag)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.GetDisplayInfoFromRegistry(): display type match NOT FOUND", new object[0]);
      }
      return false;
    }

    private void GetEQ()
    {
      lock (DWriteMutex)
      {
        this.EQSettings._EqDataAvailable = MiniDisplayHelper.GetEQ(ref this.EQSettings);
        if (this.EQSettings._EqDataAvailable)
        {
          this._iconThread.Priority = ThreadPriority.AboveNormal;
        }
        else
        {
          this._iconThread.Priority = ThreadPriority.BelowNormal;
        }
      }
    }

    private static int GetVFDTypeFromFirmware(int FWVersion)
    {
      bool flag = Assembly.GetEntryAssembly().FullName.Contains("Configuration") | Settings.Instance.ExtensiveLogging;
      if (flag)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.GetVFDTypeFromFirmware(): Searching for Firmware version {0}", new object[] { FWVersion.ToString("x00") });
      }
      for (int i = 0; (_iMON_FW_Display[i, 0] != 0) & (i < _iMON_FW_Display.Length); i++)
      {
        if (_iMON_FW_Display[i, 1] == 0)
        {
          if (_iMON_FW_Display[i, 0] == FWVersion)
          {
            if (flag)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.GetVFDTypeFromFirmware(): Found version match", new object[0]);
            }
            return _iMON_FW_Display[i, 2];
          }
        }
        else
        {
          int num2 = _iMON_FW_Display[i, 0];
          int num3 = _iMON_FW_Display[i, 1];
          if (((FWVersion == num2) || (FWVersion == num3)) || ((FWVersion > num2) && (FWVersion < num3)))
          {
            if (flag)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.GetVFDTypeFromFirmware(): Found version match", new object[0]);
            }
            return _iMON_FW_Display[i, 2];
          }
        }
      }
      if (flag)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.GetVFDTypeFromFirmware(): version match NOT FOUND", new object[0]);
      }
      return -1;
    }

    public void Initialize()
    {
      MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.Initialize(): called", new object[0]);
      if (this._isDisabled)
      {
        MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.Initialize(): completed\n\n iMONLCDg DRIVER DISABLED\n\n", new object[0]);
      }
      else
      {
        this.OpenLcd();
        this.Clear();
        MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.Initialize(): completed", new object[0]);
      }
    }

    private void InitializeDriver()
    {
      this.DoDebug = Assembly.GetEntryAssembly().FullName.Contains("Configuration") | Settings.Instance.ExtensiveLogging;
      this._IsConfiguring = Assembly.GetEntryAssembly().FullName.Contains("Configuration");
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): started.", new object[0]);
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): iMONLCDg Driver - {0}", new object[] { this.Description });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Called by \"{0}\".", new object[] { Assembly.GetEntryAssembly().FullName });
      FileInfo info = new FileInfo(Assembly.GetExecutingAssembly().Location);
      if (this.DoDebug)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Assembly creation time: {0} ( {1} UTC )", new object[] { info.LastWriteTime, info.LastWriteTimeUtc.ToUniversalTime() });
      }
      if (this.DoDebug)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Platform: {0}", new object[] { Environment.OSVersion.VersionString });
      }
      this.LoadAdvancedSettings();
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Idle Message: {0}", new object[] { Settings.Instance.IdleMessage });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Delay driver startup: {0}", new object[] { this._DelayStartup.ToString() });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Ensure Antec/iMON Manager is running before driver startup: {0}", new object[] { this._EnsureManagerStartup.ToString() });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Force Antec/iMON Manager Restart after driver startup: {0}", new object[] { this._ForceManagerRestart.ToString() });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Force Antec/iMON Manager Reload during driver startup: {0}", new object[] { this._ForceManagerReload.ToString() });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Restart Antec/iMON Manager FrontView on exit: {0}", new object[] { this._RestartFrontviewOnExit.ToString() });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Force Manager to use KeyBoard mode for iMON PAD: {0}", new object[] { this._ForceKeyBoardMode.ToString() });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Force Display Type: {0}", new object[] { this._ForceDisplay });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Blank display on MediaPortal exit: {0}", new object[] { this._BlankDisplayOnExit });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Control Brightness: {0}", new object[] { this._Backlight });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Control Contrast: {0}", new object[] { this._Contrast });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Force Graphic Text: {0}", new object[] { Settings.Instance.ForceGraphicText });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Use Disk Icon: {0}", new object[] { this.DisplayOptions.DiskIcon.ToString() });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Volume Bar: {10", new object[] { this.DisplayOptions.VolumeDisplay.ToString() });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Progress Bar: {0}", new object[] { this.DisplayOptions.ProgressDisplay.ToString() });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Use Disk Icon For Media Status: {0}", new object[] { this.DisplayOptions.DiskMediaStatus.ToString() });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Use Disk Icon For CD/DVD device status: {0}", new object[] { this.DisplayOptions.DiskMonitor.ToString() });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Custom Font: {0}", new object[] { this.DisplayOptions.UseCustomFont.ToString() });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Large Icons: {0}", new object[] { this.DisplayOptions.UseLargeIcons.ToString() });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Custom Large Icons: {0}", new object[] { this.DisplayOptions.UseCustomIcons.ToString() });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Equalizer Display: {0}", new object[] { this.EQSettings.UseEqDisplay });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options -   EQMode: {0}", new object[] { this.EQSettings._useEqMode });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options -   Normal Equalizer Display: {0}", new object[] { this.EQSettings.UseNormalEq });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options -   Stereo Equalizer Display: {0}", new object[] { this.EQSettings.UseStereoEq });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options -   VU Meter Display: {0}", new object[] { this.EQSettings.UseVUmeter });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options -   VU Meter Style 2 Display: {0}", new object[] { this.EQSettings.UseVUmeter2 });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options -     Use VU Channel indicators: {0}", new object[] { this.EQSettings._useVUindicators });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options -   Restrict EQ Update Rate: {0}", new object[] { this.EQSettings.RestrictEQ });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options -     Restricted EQ Update Rate: {0} updates per second", new object[] { this.EQSettings._EQ_Restrict_FPS });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options -   Delay EQ Startup: {0}", new object[] { this.EQSettings.DelayEQ });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options -     Delay EQ Startup Time: {0} seconds", new object[] { this.EQSettings._DelayEQTime });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options -   Smooth EQ Amplitude Decay: {0}", new object[] { this.EQSettings.SmoothEQ });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options -   Show Track Info with EQ display: {0}", new object[] { this.EQSettings.EQTitleDisplay });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options -     Show Track Info Interval: {0} seconds", new object[] { this.EQSettings._EQTitleDisplayTime });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options -     Show Track Info duration: {0} seconds", new object[] { this.EQSettings._EQTitleShowTime });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Blank display with video: {0}", new object[] { this.DisplaySettings.BlankDisplayWithVideo });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options -   Enable Display on Action: {0}", new object[] { this.DisplaySettings.EnableDisplayAction });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options -     Enable display for: {0} seconds", new object[] { this.DisplaySettings.DisplayActionTime });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Monitor PowerState Events: {0}", new object[] { this._MonitorPower });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Blank display when idle: {0}", new object[] { this.DisplaySettings.BlankDisplayWhenIdle });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options -     blank display after: {0} seconds", new object[] { this.DisplaySettings._BlankIdleTimeout / 0xf4240L });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Shutdown Message - Line 1: {0}", new object[] { this.DisplaySettings._Shutdown1 });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Shutdown Message - Line 2: {0}", new object[] { this.DisplaySettings._Shutdown2 });
      if (this.RemoteSettings.EnableRemote)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Remote Type: {0}", new object[] { this.RemoteSettings.RemoteType });
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Disable Remote Key Repeat: {0}", new object[] { this.RemoteSettings.DisableRepeat });
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Remote Key Repeat Delay: {0}", new object[] { this.RemoteSettings.RepeatDelay });
      }
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Setting - Audio using ASIO: {0}", new object[] { this.EQSettings._AudioUseASIO });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Setting - Audio using Mixer: {0}", new object[] { this.EQSettings._AudioIsMixing });
      if (!this.DisplayOptions.DiskMonitor & !this.DisplayOptions.DiskMediaStatus)
      {
        this.DisplayOptions.DiskIcon = false;
      }
      if (this._ForceDisplay == "LCD")
      {
        _DisplayType = DisplayType.LCD;
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options forces display type to LCD", new object[0]);
      }
      else if (this._ForceDisplay == "LCD2")
      {
        _DisplayType = DisplayType.LCD2;
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options forces display type to LCD2", new object[0]);
      }
      else if (this._ForceDisplay == "VFD")
      {
        _DisplayType = DisplayType.VFD;
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options forces display type to VFD", new object[0]);
      }
      else if (this._ForceDisplay == "LCD3R")
      {
        _DisplayType = DisplayType.VFD;
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Advanced options forces display type to LCD3R", new object[0]);
      }
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Extensive logging: {0}", new object[] { this.DoDebug });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Use V3 DLL for VFD: {0}", new object[] { _VFD_UseV3DLL });
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Display Type: {0}", new object[] { DisplayType.TypeName(_DisplayType) });
      if (((this.imonVFD_DLLFile = this.FindImonVFDdll()) == string.Empty) & !_VFD_UseV3DLL)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): Failed - installed sg_vfd.dll not found - driver disabled", new object[0]);
        this._isDisabled = true;
      }
      else
      {
        this._IMON = new iMONDisplay();
        if (!this._IMON.Initialize(this.imonVFD_DLLFile))
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): DLL linking Failed - driver disabled", new object[0]);
          this._isDisabled = true;
        }
        else
        {
          this._isDisabled = false;
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.InitializeDriver(): completed.", new object[0]);
        }
      }
    }

    private void InitRemoteSettings(ref RemoteControl RCsettings)
    {
      RCsettings.EnableRemote = false;
      RCsettings.RemoteType = "MCE";
      RCsettings.DisableRepeat = false;
      RCsettings.RepeatDelay = 4;
    }

    private void InitRemoteState(ref RemoteState RemoteStatus)
    {
      RemoteStatus.KeyPressed = 0xff;
      RemoteStatus.KeyModifier = 0xff;
      RemoteStatus.LastKeyPressed = 0xff;
      RemoteStatus.LastKeyModifier = 0xff;
      RemoteStatus.LastButtonPressed = 0xff;
      RemoteStatus.LastButtonToggle = 0xff;
      RemoteStatus.LastButtonPressTimestamp = DateTime.Now;
      RemoteStatus.LastMouseKeyEvent.Last_L_Button = 0;
      RemoteStatus.LastMouseKeyEvent.Last_R_Button = 0;
      RemoteStatus.LastMouseKeyEvent.Last_X_Delta = 0;
      RemoteStatus.LastMouseKeyEvent.Last_Y_Delta = 0;
      RemoteStatus.LastMouseKeyEvent.Last_X_Size = 0;
      RemoteStatus.LastMouseKeyEvent.Last_Y_Size = 0;
    }

    private string KeyCodeToKeyString(int KeyPress, int KeyMod)
    {
      if ((KeyPress > MCEKeyCodeToKeyString.Length) | (KeyMod > MCEModifierToModifierString.Length))
      {
        return "";
      }
      return MCEModifierToModifierString[KeyMod].Replace("*", MCEKeyCodeToKeyString[KeyPress]);
    }

    private void KillManager()
    {
      bool flag = false;
      Process[] processesByName = Process.GetProcessesByName("VFD");
      if (processesByName.Length > 0)
      {
        this._UsingAntecManager = true;
        MediaPortal.GUI.Library.Log.Debug("iMONLCDg.KillManager(): Shutting down VFD Manager...", new object[0]);
        processesByName[0].Kill();
        flag = false;
        while (!flag)
        {
          Thread.Sleep(100);
          MediaPortal.GUI.Library.Log.Debug("iMONLCDg.KillManager(): Waiting for iMON Manager to exit", new object[0]);
          processesByName[0].Dispose();
          processesByName = Process.GetProcessesByName("iMON");
          if (processesByName.Length == 0)
          {
            flag = true;
          }
        }
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.KillManager(): Antec VFD Manager Stopped", new object[0]);
        MediaPortal.ProcessPlugins.MiniDisplayPlugin.Win32Functions.RedrawNotificationArea();
      }
      processesByName = Process.GetProcessesByName("iMON");
      if (processesByName.Length > 0)
      {
        this._UsingSoundgraphManager = true;
        MediaPortal.GUI.Library.Log.Debug("iMONLCDg.KillManager(): Shutting down iMON Manager...", new object[0]);
        processesByName[0].Kill();
        flag = false;
        while (!flag)
        {
          Thread.Sleep(100);
          MediaPortal.GUI.Library.Log.Debug("iMONLCDg.KillManager(): Waiting for iMON Manager to exit", new object[0]);
          processesByName[0].Dispose();
          processesByName = Process.GetProcessesByName("iMON");
          if (processesByName.Length == 0)
          {
            flag = true;
          }
        }
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.KillManager(): Soundgraph iMON Manager Stopped", new object[0]);
        MediaPortal.ProcessPlugins.MiniDisplayPlugin.Win32Functions.RedrawNotificationArea();
      }
    }

    private static uint LengthToPixels(int Length)
    {
      uint[] numArray = new uint[] { 
                0, 0x80, 0xc0, 0xe0, 0xf0, 0xf8, 0xfc, 0xfe, 0xff, 0x80ff, 0xc0ff, 0xe0ff, 0xf0ff, 0xf8ff, 0xfcff, 0xfeff, 
                0xffff, 0x80ffff, 0xc0ffff, 0xe0ffff, 0xf0ffff, 0xf8ffff, 0xfcffff, 0xfeffff, 0xffffff, 0x80ffffff, 0xc0ffffff, 0xe0ffffff, 0xf0ffffff, 0xf8ffffff, 0xfcffffff, 0xfeffffff, 
                uint.MaxValue
             };
      if (Math.Abs(Length) > 0x20)
      {
        return 0;
      }
      if (Length >= 0)
      {
        return numArray[Length];
      }
      return (numArray[0x20 + Length] ^ uint.MaxValue);
    }

    private void LoadAdvancedSettings()
    {
      this.AdvSettings = AdvancedSettings.Load();
      this.IdleMessage = (Settings.Instance.IdleMessage != string.Empty) ? Settings.Instance.IdleMessage : "MediaPortal";
      this._DelayStartup = this.AdvSettings.DelayStartup;
      this._EnsureManagerStartup = this.AdvSettings.EnsureManagerStartup;
      this._ForceManagerRestart = this.AdvSettings.ForceManagerRestart;
      this._ForceManagerReload = this.AdvSettings.ForceManagerReload;
      this._RestartFrontviewOnExit = this.AdvSettings.RestartFrontviewOnExit;
      this._ForceKeyBoardMode = this.AdvSettings.ForceKeyBoardMode;
      this.RemoteSettings.EnableRemote = false;
      this.RemoteSettings.RemoteType = this.AdvSettings.RemoteType;
      this.RemoteSettings.DisableRepeat = this.AdvSettings.DisableRepeat;
      this.RemoteSettings.RepeatDelay = this.AdvSettings.RepeatDelay * 0x19;
      this._ForceDisplay = this.AdvSettings.DisplayType;
      if (this._ForceDisplay == null || this._ForceDisplay.Equals(string.Empty))
      {
        this._ForceDisplay = "AutoDetect";
      }
      this.DisplayOptions.VolumeDisplay = this.AdvSettings.VolumeDisplay;
      this.DisplayOptions.ProgressDisplay = this.AdvSettings.ProgressDisplay;
      this.DisplayOptions.DiskIcon = this.AdvSettings.DiskIcon;
      this.DisplayOptions.DiskMediaStatus = this.AdvSettings.DiskMediaStatus;
      this.DisplayOptions.DiskMonitor = this.AdvSettings.DeviceMonitor;
      this.DisplayOptions.UseCustomFont = this.AdvSettings.UseCustomFont;
      this.DisplayOptions.UseLargeIcons = this.AdvSettings.UseLargeIcons;
      this.DisplayOptions.UseCustomIcons = this.AdvSettings.UseCustomIcons;
      this.DisplayOptions.UseInvertedIcons = this.AdvSettings.UseInvertedIcons;
      this.EQSettings.UseEqDisplay = this.AdvSettings.EqDisplay;
      this.EQSettings.UseNormalEq = this.AdvSettings.NormalEQ;
      this.EQSettings.UseStereoEq = this.AdvSettings.StereoEQ;
      this.EQSettings.UseVUmeter = this.AdvSettings.VUmeter;
      this.EQSettings.UseVUmeter2 = this.AdvSettings.VUmeter2;
      this.EQSettings._useVUindicators = this.AdvSettings.VUindicators;
      this.EQSettings._useEqMode = this.AdvSettings.EqMode;
      this.EQSettings.RestrictEQ = this.AdvSettings.RestrictEQ;
      this.EQSettings._EQ_Restrict_FPS = this.AdvSettings.EqRate;
      this.EQSettings.DelayEQ = this.AdvSettings.DelayEQ;
      this.EQSettings._DelayEQTime = this.AdvSettings.DelayEqTime;
      this.EQSettings.SmoothEQ = this.AdvSettings.SmoothEQ;
      this.EQSettings.EQTitleDisplay = this.AdvSettings.EQTitleDisplay;
      this.EQSettings._EQTitleShowTime = this.AdvSettings.EQTitleShowTime;
      this.EQSettings._EQTitleDisplayTime = this.AdvSettings.EQTitleDisplayTime;
      this.EQSettings._EqUpdateDelay = (this.EQSettings._EQ_Restrict_FPS == 0) ? 0 : ((0x989680 / this.EQSettings._EQ_Restrict_FPS) - (0xf4240 / this.EQSettings._EQ_Restrict_FPS));
      _VFD_UseV3DLL = this.AdvSettings.VFD_UseV3DLL;
      this._MonitorPower = this.AdvSettings.MonitorPowerState;
      this.DisplaySettings.BlankDisplayWithVideo = this.AdvSettings.BlankDisplayWithVideo;
      this.DisplaySettings.EnableDisplayAction = this.AdvSettings.EnableDisplayAction;
      this.DisplaySettings.DisplayActionTime = this.AdvSettings.EnableDisplayActionTime;
      this.DisplaySettings.BlankDisplayWhenIdle = this.AdvSettings.BlankDisplayWhenIdle;
      this.DisplaySettings.BlankIdleDelay = this.AdvSettings.BlankIdleTime;
      this.DisplaySettings._BlankIdleTimeout = this.DisplaySettings.BlankIdleDelay * 0x989680;
      this.DisplaySettings._Shutdown1 = Settings.Instance.Shutdown1;
      this.DisplaySettings._Shutdown2 = Settings.Instance.Shutdown2;
      this.DisplaySettings._DisplayControlTimeout = this.DisplaySettings.DisplayActionTime * 0x989680;
      FileInfo info = new FileInfo(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg.xml"));
      this.SettingsLastModTime = info.LastWriteTime;
      this.LastSettingsCheck = DateTime.Now;
    }

    private void OnExternalAction(Action action)
    {
      if (this.DisplaySettings.EnableDisplayAction)
      {
        if (this.DoDebug)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.OnExternalAction(): received action {0}", new object[] { action.wID.ToString() });
        }
        Action.ActionType wID = action.wID;
        if (wID <= Action.ActionType.ACTION_SHOW_OSD)
        {
          if ((wID != Action.ActionType.ACTION_SHOW_INFO) && (wID != Action.ActionType.ACTION_SHOW_OSD))
          {
            return;
          }
        }
        else if (((wID != Action.ActionType.ACTION_SHOW_MPLAYER_OSD) && (wID != Action.ActionType.ACTION_KEY_PRESSED)) && (wID != Action.ActionType.ACTION_MOUSE_CLICK))
        {
          return;
        }
        this.DisplaySettings._DisplayControlAction = true;
        this.DisplaySettings._DisplayControlLastAction = DateTime.Now.Ticks;
        if (this.DoDebug)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.OnExternalAction(): received DisplayControlAction", new object[0]);
        }
        this.DisplayOn();
      }
    }

    private void OpenLcd()
    {
      if (!this._isDisabled)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.OpenLcd(): called", new object[0]);
        if (!this._IMON.iMONVFD_IsInited())
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.OpenLcd(): opening display", new object[0]);
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.OpenLcd(): opening display with iMONVFD_Init({0},{1})", new object[] { _VfdType.ToString("x00"), _VfdReserved.ToString("x0000") });
          if (!this._IMON.iMONVFD_Init(_VfdType, _VfdReserved))
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.OpenLcd(): Could not open display with Open({0},{1})", new object[] { _VfdType.ToString("x00"), _VfdReserved.ToString("x0000") });
            this._isDisabled = true;
            this._errorMessage = "Could not open iMON display device";
          }
          else
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.OpenLcd(): display opened", new object[0]);
            if (!this._displayTest & ((_DisplayType == DisplayType.LCD) || (_DisplayType == DisplayType.LCD2)))
            {
              if (this.DisplayOptions.UseCustomFont)
              {
                this.CFont = new CustomFont();
                this.CFont.InitializeCustomFont();
              }
              if (this.DisplayOptions.UseLargeIcons)
              {
                this.CustomLargeIcon = new LargeIcon();
                this.CustomLargeIcon.InitializeLargeIcons();
              }
              this._iconThread = new Thread(new ThreadStart(this.UpdateIcons));
              this._iconThread.IsBackground = true;
              this._iconThread.Priority = ThreadPriority.BelowNormal;
              this._iconThread.Name = "UpdateIconThread";
              this._iconThread.Start();
              if (this._iconThread.IsAlive)
              {
                MediaPortal.GUI.Library.Log.Info("iMONLCDg.OpenLcd(): iMONLCDg.UpdateIcons() Thread Started", new object[0]);
              }
              else
              {
                MediaPortal.GUI.Library.Log.Info("iMONLCDg.OpenLcd(): iMONLCDg.UpdateIcons() FAILED TO START", new object[0]);
              }
            }
            else if (!this._displayTest & (_DisplayType == DisplayType.VFD))
            {
              if (this.EQSettings.UseEqDisplay || this.DisplaySettings.BlankDisplayWithVideo)
              {
                this._iconThread = new Thread(new ThreadStart(this.VFD_EQ_Update));
                this._iconThread.IsBackground = true;
                this._iconThread.Priority = ThreadPriority.BelowNormal;
                this._iconThread.Name = "VFD_EQ_Update";
                this._iconThread.Start();
                if (this._iconThread.IsAlive)
                {
                  MediaPortal.GUI.Library.Log.Info("iMONLCDg.OpenLcd(): iMONLCDg.VFD_EQ_Update() Thread Started", new object[0]);
                }
                else
                {
                  MediaPortal.GUI.Library.Log.Info("iMONLCDg.OpenLcd(): iMONLCDg.VFD_EQ_Update() FAILED TO START", new object[0]);
                }
              }
            }
            else if ((!this._displayTest & (_DisplayType == DisplayType.ThreeRsystems)) && (this.EQSettings.UseEqDisplay || this.DisplaySettings.BlankDisplayWithVideo))
            {
              this._iconThread = new Thread(new ThreadStart(this.VFD_EQ_Update));
              this._iconThread.IsBackground = true;
              this._iconThread.Priority = ThreadPriority.BelowNormal;
              this._iconThread.Name = "VFD_EQ_Update";
              this._iconThread.TrySetApartmentState(ApartmentState.MTA);
              this._iconThread.Start();
              if (this._iconThread.IsAlive)
              {
                MediaPortal.GUI.Library.Log.Info("iMONLCDg.OpenLcd(): iMONLCDg.VFD_EQ_Update() Thread Started", new object[0]);
              }
              else
              {
                MediaPortal.GUI.Library.Log.Info("iMONLCDg.OpenLcd(): iMONLCDg.VFD_EQ_Update() FAILED TO START", new object[0]);
              }
            }
          }
        }
        else
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.OpenLcd: Display already open", new object[0]);
        }
        if (this._MonitorPower && !this._IsHandlingPowerEvent)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.OpenLcd(): Adding Power State Monitor callback to system event thread", new object[0]);
          SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(this.SystemEvents_PowerModeChanged);
        }
        lock (DWriteMutex)
        {
          if ((_DisplayType == DisplayType.LCD) || (_DisplayType == DisplayType.LCD2))
          {
            if (_DisplayType == DisplayType.LCD2)
            {
              this.SendData(-8646911284551352256L);
              this.SendData(-8502796096475496448L);
              if (this._Contrast)
              {
                MediaPortal.GUI.Library.Log.Info("iMONLCDg.OpenLcd(): Setting LCD2 contrast level to {0}", new object[] { this._ContrastLevel });
                this.SendData(Command.SetContrast, this._ContrastLevel);
              }
              this.SendData(-8791026472627208192L);
              this.SendData(Command.SetIcons);
              this.SendData(Command.SetLines0);
              this.SendData(Command.SetLines1);
              this.SendData(Command.SetLines2);
              this.ClearDisplay();
              this.SendData(-8358680908399640433L);
            }
            else
            {
              this.SendData(Command.DisplayOn);
              this.SendData(Command.ClearAlarm);
              if (this._Contrast)
              {
                MediaPortal.GUI.Library.Log.Info("iMONLCDg.OpenLcd(): Setting LCD contrast level to {0}", new object[] { this._ContrastLevel });
                this.SendData(Command.SetContrast, this._ContrastLevel);
              }
              this.SendData(Command.KeypadLightOn);
            }
          }
          else if (_DisplayType == DisplayType.ThreeRsystems)
          {
            this.SendData((long)0x2020202020202000L);
            this.SendData((long)0x2020202020202002L);
            this.SendData((long)0x2020202020202004L);
            this.SendData((long)0x2020202020202006L);
            this.SendData((long)0x20202020ffffff08L);
            this.SendData((long)0x21c020000000000L);
            this.SendData((long)2L);
            this.SendData((long)0x200020000000000L);
            this.SendData((long)2L);
            this.SendData((long)0x21b010000000000L);
            this.SendData((long)2L);
          }
        }
        AdvancedSettings.OnSettingsChanged += new AdvancedSettings.OnSettingsChangedHandler(this.AdvancedSettings_OnSettingsChanged);
        this.ForceManagerRestart();
        this.Remote_Start();
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.OpenLcd(): completed", new object[0]);
      }
    }

    private void Remote_Process_MCE(byte[] RCBuffer, ref RemoteState RemoteStatus)
    {
      bool extensiveLogging = Settings.Instance.ExtensiveLogging;
      if (RCBuffer.Length == 8)
      {
        if ((RCBuffer[0] == 0x80) & (RCBuffer[7] == 0xae))
        {
          byte num2 = RCBuffer[2];
          byte keyCode = RCBuffer[3];
          if (num2 != RemoteStatus.LastButtonToggle)
          {
            this.FireRemoteEvent(keyCode);
            RemoteStatus.LastButtonPressTimestamp = DateTime.Now;
            if (extensiveLogging)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Run(): received Button press - code = {0}", new object[] { keyCode.ToString("X00") });
            }
          }
          else if ((DateTime.Now.Ticks > RemoteStatus.LastButtonPressTimestamp.AddMilliseconds((double)this.RemoteSettings.RepeatDelay).Ticks) && !this.RemoteSettings.DisableRepeat)
          {
            this.FireRemoteEvent(keyCode);
            RemoteStatus.LastButtonPressTimestamp = DateTime.Now;
            if (extensiveLogging)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Run(): REPEATING buttonpress - code = {0}", new object[] { keyCode.ToString("X00") });
            }
          }
          RemoteStatus.LastButtonPressed = keyCode;
          RemoteStatus.LastButtonToggle = num2;
        }
        else if ((RCBuffer[7] == 190) & (RCBuffer[3] < 0x66))
        {
          byte index = RCBuffer[2];
          byte keyMod = RCBuffer[3];
          int num5 = (((keyMod & 240) >> 4) | (keyMod & 15)) & 15;
          int num6 = MCEKeyCodeToKeyCode[index] | MCEModifierToKeyModifier[num5];
          if ((index != RemoteStatus.LastKeyPressed) | (keyMod != RemoteStatus.LastKeyModifier))
          {
            Keys.Return.ToString();
            SendKeys.SendWait(this.KeyCodeToKeyString(index, keyMod));
            if (extensiveLogging)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Run(): keyboard key press - code = {0} -> {1}", new object[] { index, num6.ToString("X00") });
            }
            RemoteStatus.LastKeyPressed = index;
            RemoteStatus.LastKeyModifier = keyMod;
            RemoteStatus.LastButtonPressTimestamp = DateTime.Now;
          }
          else if ((DateTime.Now.Ticks > RemoteStatus.LastButtonPressTimestamp.AddMilliseconds((double)this.RemoteSettings.RepeatDelay).Ticks) && !this.RemoteSettings.DisableRepeat)
          {
            SendKeys.SendWait(this.KeyCodeToKeyString(index, keyMod));
            RemoteStatus.LastButtonPressTimestamp = DateTime.Now;
            if (extensiveLogging)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Run(): REPEATING keyboard key press - code = {0}", new object[] { num6.ToString("X00") });
            }
          }
        }
        else if (RCBuffer[7] != 0xce)
        {
          if ((RemoteStatus.LastKeyPressed != 0xff) && extensiveLogging)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Run(): RELEASED key - code = {0}", new object[] { RemoteStatus.LastKeyPressed });
          }
          if ((RemoteStatus.LastButtonPressed != 0xff) && extensiveLogging)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Run(): RELEASED button - code = {0}", new object[] { RemoteStatus.LastButtonPressed });
          }
          RemoteStatus.LastKeyPressed = 0xff;
          RemoteStatus.LastKeyModifier = 0xff;
          RemoteStatus.LastButtonPressed = 0xff;
          RemoteStatus.LastButtonToggle = 0xff;
          RemoteStatus.LastButtonPressTimestamp = this.NullTime;
        }
      }
    }

    private void Remote_Process_PAD(byte[] RCBuffer, ref RemoteState RemoteStatus)
    {
      MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Process_PAD(): called", new object[0]);
      if ((RCBuffer[0] & 0xfc) == 40)
      {
        int num = 0;
        num += (RCBuffer[0] & 3) << 6;
        num += RCBuffer[1] & 0x30;
        num += (RCBuffer[1] & 6) << 1;
        num += (RCBuffer[2] & 0xc0) >> 6;
        if (num != RemoteStatus.KeyPressed)
        {
          this.FireRemoteEvent((byte)num);
          RemoteStatus.LastButtonPressTimestamp = DateTime.Now;
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Run(): received Button press - code = {0}", new object[] { num.ToString("X00") });
        }
        else if ((DateTime.Now.Ticks > RemoteStatus.LastButtonPressTimestamp.AddMilliseconds((double)this.RemoteSettings.RepeatDelay).Ticks) && !this.RemoteSettings.DisableRepeat)
        {
          this.FireRemoteEvent((byte)num);
          RemoteStatus.LastButtonPressTimestamp = DateTime.Now;
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Run(): REPEATING buttonpress - code = {0}", new object[] { num.ToString("X00") });
        }
      }
      else if ((RCBuffer[0] & 0xfc) == 0x68)
      {
        int num2 = ((RCBuffer[0] & 2) > 0) ? 1 : -1;
        int num3 = ((RCBuffer[0] & 1) > 0) ? 1 : -1;
        int dx = (RCBuffer[1] & 120) >> 3;
        int dy = (RCBuffer[2] & 120) >> 3;
        byte num1 = RCBuffer[1];
        byte num6 = RCBuffer[1];
        if (num2 > 0)
        {
          if ((Cursor.Position.X + dx) > Screen.PrimaryScreen.Bounds.Right)
          {
            SetCursorPos(Screen.PrimaryScreen.Bounds.Right, Cursor.Position.Y);
          }
          else
          {
            Cursor.Position.Offset(dx, 0);
          }
        }
        else if ((Cursor.Position.X - dx) < Screen.PrimaryScreen.Bounds.Left)
        {
          SetCursorPos(Screen.PrimaryScreen.Bounds.Left, Cursor.Position.Y);
        }
        else
        {
          Cursor.Position.Offset(-dx, 0);
        }
        if (num3 > 0)
        {
          if ((Cursor.Position.Y + dy) > Screen.PrimaryScreen.Bounds.Bottom)
          {
            SetCursorPos(Cursor.Position.X, Screen.PrimaryScreen.Bounds.Bottom);
          }
          else
          {
            Cursor.Position.Offset(0, dy);
          }
        }
        else if ((Cursor.Position.Y - num3) < Screen.PrimaryScreen.Bounds.Top)
        {
          SetCursorPos(Cursor.Position.X, Screen.PrimaryScreen.Bounds.Top);
        }
        else
        {
          Cursor.Position.Offset(0, -dy);
        }
      }
    }

    private void Remote_Run()
    {
      byte[] outBuffer = new byte[8];
      uint bytesReturned = 0;
      string str = string.Empty;
      int num2 = 0;
      this.NullTime = DateTime.Now;
      this.InitRemoteState(ref this.RemoteStatus);
      if (this.RemoteSettings.RemoteType == "MCE")
      {
        num2 = 0x77;
      }
      else
      {
        num2 = 0x73;
      }
      IntPtr zero = IntPtr.Zero;
      string fileName = @"\\.\SGIMON";
      if (!this._IMON.iMONRC_IsInited())
      {
        if (!this._IMON.iMONRC_Init(num2, 0x83, 0x8888))
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Run(): Unable to open RC hardware - thread not started", new object[0]);
          return;
        }
        this._IMON.iMONRC_ChangeRCSet(num2);
        this._IMON.iMONRC_Uninit();
      }
      zero = MediaPortal.ProcessPlugins.MiniDisplayPlugin.Win32Functions.CreateFile(fileName, 0xc0000000, 3, IntPtr.Zero, 3, 0, IntPtr.Zero);
      if ((((int)zero) == -1) | (((int)zero) == 0))
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Run(): Unable to open RC device - thread not started", new object[0]);
      }
      else
      {
        if (this.RemoteSettings.RemoteType == "MCE")
        {
          this.Remote_Set_RC6(zero, true);
        }
        else
        {
          this.Remote_Set_RC6(zero, false);
        }
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Run(): iMON RC Device opened", new object[0]);
        while (true)
        {
          lock (RemoteMutex)
          {
            if (this.DoDebug)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Run(): Checking for Remote Thread termination request", new object[0]);
            }
            if (_stopRemoteThread)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Run(): Remote Manager Thread terminating", new object[0]);
              _stopRemoteThread = false;
              MediaPortal.ProcessPlugins.MiniDisplayPlugin.Win32Functions.CloseHandle(zero);
              return;
            }
            MediaPortal.ProcessPlugins.MiniDisplayPlugin.Win32Functions.DeviceIoControl(zero, 0x222030, null, 0, outBuffer, 8, ref bytesReturned, IntPtr.Zero);
            if ((outBuffer[0] != 0xff) | (outBuffer[0] != 0))
            {
              str = string.Empty;
              for (int i = 0; i < 8; i++)
              {
                str = str + outBuffer[i].ToString("X00") + " ";
              }
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Run(): DeviceIoControl - Read = {0} : {1}", new object[] { bytesReturned, str });
            }
            if (outBuffer[6] == 1)
            {
              this.Remote_Process_PAD(outBuffer, ref this.RemoteStatus);
            }
            if (((outBuffer[7] == 0xae) | (outBuffer[7] == 0xae)) | (outBuffer[7] == 0xae))
            {
              this.Remote_Process_MCE(outBuffer, ref this.RemoteStatus);
            }
          }
          Thread.Sleep(0x19);
        }
      }
    }

    private void Remote_Run_OLD()
    {
      byte[] outBuffer = new byte[8];
      uint bytesReturned = 0;
      byte iChar = 0xff;
      byte iCode = 0xff;
      byte num4 = 0xff;
      byte num5 = 0xff;
      byte keyCode = 0xff;
      byte num7 = 0xff;
      byte num8 = 0xff;
      DateTime now = DateTime.Now;
      DateTime time1 = DateTime.Now;
      IntPtr zero = IntPtr.Zero;
      string fileName = @"\\.\SGIMON";
      if (!this._IMON.iMONRC_IsInited())
      {
        if (!this._IMON.iMONRC_Init(0x77, 0x83, 0x8888))
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Run(): Unable to open RC hardware - thread not started", new object[0]);
          return;
        }
        this._IMON.iMONRC_Uninit();
      }
      zero = MediaPortal.ProcessPlugins.MiniDisplayPlugin.Win32Functions.CreateFile(fileName, 0xc0000000, 3, IntPtr.Zero, 3, 0, IntPtr.Zero);
      if ((((int)zero) == -1) | (((int)zero) == 0))
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Run(): Unable to open RC device - thread not started", new object[0]);
      }
      else
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Run(): iMON RC Device opened", new object[0]);
        while (true)
        {
          lock (RemoteMutex)
          {
            if (this.DoDebug)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Run(): Checking for Remote Thread termination request", new object[0]);
            }
            if (_stopUpdateIconThread)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Run(): Remote Manager Thread terminating", new object[0]);
              _stopRemoteThread = false;
              MediaPortal.ProcessPlugins.MiniDisplayPlugin.Win32Functions.CloseHandle(zero);
              return;
            }
            MediaPortal.ProcessPlugins.MiniDisplayPlugin.Win32Functions.DeviceIoControl(zero, 0x222030, null, 0, outBuffer, 8, ref bytesReturned, IntPtr.Zero);
            if (outBuffer[0] == 0x80)
            {
              string str2 = string.Empty;
              for (int i = 0; i < 8; i++)
              {
                str2 = str2 + outBuffer[i].ToString("X00") + " ";
              }
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Run(): DeviceIoControl - Read = {0} : {1}", new object[] { bytesReturned, str2 });
              if (this.RemoteSettings.RemoteType == "MCE")
              {
                num7 = outBuffer[2];
                keyCode = outBuffer[3];
                if (num7 != num8)
                {
                  this.FireRemoteEvent(keyCode);
                  now = DateTime.Now;
                  MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Run(): received Button press - code = {0}", new object[] { keyCode });
                }
                else if ((DateTime.Now.Ticks > now.AddMilliseconds((double)this.RemoteSettings.RepeatDelay).Ticks) && !this.RemoteSettings.DisableRepeat)
                {
                  this.FireRemoteEvent(keyCode);
                  now = DateTime.Now;
                  MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Run(): REPEATING buttonpress - code = {0}", new object[] { keyCode });
                }
                num8 = num7;
              }
            }
            else if (outBuffer[7] == 190)
            {
              iChar = outBuffer[2];
              iCode = outBuffer[3];
              if ((iChar != num4) | (iCode != num5))
              {
                Key key = new Key(iChar, iCode);
                Action action = new Action(key, Action.ActionType.ACTION_KEY_PRESSED, 0f, 0f);
                GUIGraphicsContext.OnAction(action);
                num4 = iChar;
                num5 = iCode;
              }
            }
            else if (outBuffer[7] != 0xce)
            {
              if (num4 != 0xff)
              {
                MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Run(): RELEASED key - code = {0}", new object[] { iChar });
              }
              num4 = 0xff;
            }
          }
          Thread.Sleep(0x19);
        }
      }
    }

    private void Remote_Set_RC6(IntPtr dHandle, bool RC6on)
    {
      uint bytesReturned = 0;
      byte[] inBuffer = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0 };
      for (int i = 0; i < 11; i += 2)
      {
        if (i == 8)
        {
          byte num3;
          inBuffer[6] = (byte)(num3 = 0);
          inBuffer[4] = inBuffer[5] = num3;
          inBuffer[4] = RC6on ? ((byte)1) : ((byte)0);
        }
        else
        {
          byte num5;
          inBuffer[6] = (byte)(num5 = 0x20);
          inBuffer[4] = inBuffer[5] = num5;
        }
        inBuffer[7] = (byte)i;
        MediaPortal.ProcessPlugins.MiniDisplayPlugin.Win32Functions.DeviceIoControl(dHandle, 0x222018, inBuffer, 8, null, 0, ref bytesReturned, IntPtr.Zero);
      }
    }

    private void Remote_Start()
    {
      if (this.RemoteSettings.EnableRemote)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Start(): testing for RC hardware", new object[0]);
        int num = 0;
        if (this.RemoteSettings.RemoteType == "MCE")
        {
          num = 0x77;
        }
        else
        {
          num = 0x73;
        }
        if (this._IMON.RC_Available())
        {
          if (this._IMON.iMONRC_Init(num, 0x83, 0x8888))
          {
            this._IMON.iMONRC_ChangeRCSet(num);
            int num2 = this._IMON.iMONRC_GetHWType();
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Start(): RC TEST returned RC_HW: 0x{0}", new object[] { num2.ToString("x00000000") });
            if (num2 < 1)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Start(): No remote control hardware found.", new object[0]);
              return;
            }
            this._IMON.iMONRC_Uninit();
          }
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Start(): remote control hardware found.", new object[0]);
          bool flag2 = false;
          bool flag3 = false;
          if (this.RemoteSettings.EnableRemote)
          {
            try
            {
              if (this.TestXmlVersion(Config.GetFile(Config.Dir.CustomInputDefault, "iMon_Remote.xml")) < 3)
              {
                MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Start(): Deleting iMon_Remote mapping file with the wrong version stamp.", new object[0]);
                File.Delete(Config.GetFile(Config.Dir.CustomInputDefault, "iMon_Remote.xml"));
              }
              if (!File.Exists(Config.GetFile(Config.Dir.CustomInputDefault, "iMon_Remote.xml")))
              {
                MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Start(): Creating default iMON_Remote mapping file", new object[0]);
                if (!AdvancedSettings.CreateDefaultRemoteMapping())
                {
                  MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Start(): ERROR Creating default iMON_Remote mapping file", new object[0]);
                  flag2 = false;
                }
                else
                {
                  flag2 = true;
                }
              }
              else
              {
                flag2 = true;
              }
            }
            catch (Exception exception)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Start(): CAUGHT EXCEPTION while loading InputHander - {0}", new object[] { exception });
              flag2 = false;
              flag3 = false;
            }
            if (flag2)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg_Remote_Start(): Loading InputHandler", new object[0]);
              this._inputHandler = new InputHandler("iMon_Remote");
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Start(): InputHandler loaded = {0}", new object[] { this._inputHandler.IsLoaded });
              if (this._inputHandler.IsLoaded)
              {
                flag3 = true;
              }
              else
              {
                flag3 = false;
                MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Start(): error loading InputHandler - remote support disabled", new object[0]);
              }
            }
            else
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Start(): Remote support disabled - no remote mapping file", new object[0]);
              flag3 = false;
            }
            if (!flag3 || !this._inputHandler.IsLoaded)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Start(): Error loading remote mapping file - Remote support disabled", new object[0]);
              flag3 = false;
            }
          }
          else
          {
            flag3 = false;
          }
          if (!flag3)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Start(): Unable to load remote input mapper - remote control support disabled.", new object[0]);
          }
          else
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Start(): Starting iMONLCDg.Remote_Run() Thread", new object[0]);
            this._RemoteThread = new Thread(new ThreadStart(this.Remote_Run));
            this._RemoteThread.IsBackground = true;
            this._RemoteThread.Priority = ThreadPriority.Lowest;
            this._RemoteThread.Name = "iMON_Remote_Manager";
            this._RemoteThread.TrySetApartmentState(ApartmentState.MTA);
            this._RemoteThread.Start();
            if (this._RemoteThread.IsAlive)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Start(): iMONLCDg.Remote_Run() Thread Started", new object[0]);
            }
            else
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Start(): iMONLCDg.Remote_Run() FAILED TO START", new object[0]);
            }
          }
        }
        else
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Start(): No remote control hardware found.", new object[0]);
        }
      }
    }

    private void Remote_Stop()
    {
      if (this.RemoteSettings.EnableRemote)
      {
        while (this._RemoteThread.IsAlive)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Stop(): Stopping iMONLCDg.Remote_Run() Thread", new object[0]);
          lock (RemoteMutex)
          {
            _stopRemoteThread = true;
          }
          _stopRemoteThread = true;
          Thread.Sleep(500);
        }
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.Remote_Stop(): iMONLCDg.Remote_Run() Thread has stopped", new object[0]);
      }
    }

    public void RestartFrontview()
    {
      if (this._RestartFrontviewOnExit)
      {
        if (!this._UsingAntecManager & !this._UsingSoundgraphManager)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.RestartFrontview(): Antec/Imon Manager is not running... restart not possible", new object[0]);
        }
        else
        {
          Process[] processesByName;
          Process process;
          bool flag;
          RegistryKey key;
          if (this._UsingAntecManager)
          {
            MediaPortal.GUI.Library.Log.Debug("iMONLCDg.RestartFrontview(): Resetting Antec Manager registry subkey.", new object[0]);
            key = Registry.CurrentUser.OpenSubKey(@"Software\ANTEC\VFD", true);
            if (key != null)
            {
              MediaPortal.GUI.Library.Log.Debug("iMONLCDg.RestartFrontview(): Restarting Antec Manager with FrontView enabled.", new object[0]);
              key.SetValue("RunFront", 1, RegistryValueKind.DWord);
              if (this._ForceKeyBoardMode && key.GetValue("CurRemote").Equals("iMON PAD"))
              {
                MediaPortal.GUI.Library.Log.Debug("iMONLCDg.RestartFrontview(): Forcing iMON PAD mode setting to KeyBoard.", new object[0]);
                key.SetValue("MouseMode", 0, RegistryValueKind.DWord);
              }
              Registry.CurrentUser.Close();
              processesByName = Process.GetProcessesByName("VFD");
              MediaPortal.GUI.Library.Log.Debug("iMONLCDg.RestartFrontview(): Found {0} instances of Antec Manager", new object[] { processesByName.Length });
              if (processesByName.Length > 0)
              {
                MediaPortal.GUI.Library.Log.Info("iMONLCDg.RestartFrontview(): Stopping Antec Manager", new object[0]);
                processesByName[0].Kill();
                flag = false;
                while (!flag)
                {
                  Thread.Sleep(100);
                  MediaPortal.GUI.Library.Log.Debug("iMONLCDg.RestartFrontview(): Waiting for Antec Manager to exit", new object[0]);
                  processesByName[0].Dispose();
                  processesByName = Process.GetProcessesByName("VFD");
                  if (processesByName.Length == 0)
                  {
                    flag = true;
                  }
                }
                MediaPortal.GUI.Library.Log.Info("iMONLCDg.RestartFrontview(): Antec Manager Stopped", new object[0]);
                MediaPortal.ProcessPlugins.MiniDisplayPlugin.Win32Functions.RedrawNotificationArea();
                process = new Process();
                process.StartInfo.WorkingDirectory = this.FindAntecManagerPath();
                process.StartInfo.FileName = "VFD.exe";
                MediaPortal.GUI.Library.Log.Info("iMONLCDg.RestartFrontview(): ReStarting Antec Manager", new object[0]);
                Process.Start(process.StartInfo);
              }
              else
              {
                MediaPortal.GUI.Library.Log.Info("iMONLCDg.RestartFrontview(): Antec Manager is not running", new object[0]);
              }
            }
            else
            {
              Registry.CurrentUser.Close();
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.RestartFrontview(): Antec Registry subkey NOT FOUND. Frontview restart not possible.", new object[0]);
            }
          }
          else
          {
            MediaPortal.GUI.Library.Log.Debug("iMONLCDg.RestartFrontview(): Resetting SoundGraph iMON Manager registry subkey.", new object[0]);
            key = Registry.CurrentUser.OpenSubKey(@"Software\SOUNDGRAPH\iMON", true);
            if (key != null)
            {
              MediaPortal.GUI.Library.Log.Debug("iMONLCDg.RestartFrontview(): Restarting iMON Manager with FrontView enabled.", new object[0]);
              key.SetValue("RunFront", 1, RegistryValueKind.DWord);
              if (this._ForceKeyBoardMode && key.GetValue("CurRemote").Equals("iMON PAD"))
              {
                MediaPortal.GUI.Library.Log.Debug("iMONLCDg.RestartFrontview(): Forcing iMON PAD mode setting to KeyBoard.", new object[0]);
                key.SetValue("MouseMode", 0, RegistryValueKind.DWord);
              }
              Registry.CurrentUser.Close();
              Thread.Sleep(100);
              processesByName = Process.GetProcessesByName("iMON");
              MediaPortal.GUI.Library.Log.Debug("iMONLCDg.RestartFrontview(): Found {0} instances of SoundGraph iMON Manager", new object[] { processesByName.Length });
              if (processesByName.Length > 0)
              {
                MediaPortal.GUI.Library.Log.Info("iMONLCDg.RestartFrontview(): Stopping iMON Manager", new object[0]);
                processesByName[0].Kill();
                flag = false;
                while (!flag)
                {
                  Thread.Sleep(100);
                  MediaPortal.GUI.Library.Log.Debug("iMONLCDg.RestartFrontview(): Waiting for iMON Manager to exit", new object[0]);
                  processesByName[0].Dispose();
                  processesByName = Process.GetProcessesByName("iMON");
                  if (processesByName.Length == 0)
                  {
                    flag = true;
                  }
                }
                MediaPortal.GUI.Library.Log.Info("iMONLCDg.RestartFrontview(): iMON Manager Stopped", new object[0]);
                MediaPortal.ProcessPlugins.MiniDisplayPlugin.Win32Functions.RedrawNotificationArea();
                process = new Process();
                process.StartInfo.WorkingDirectory = this.FindSoundGraphManagerPath();
                process.StartInfo.FileName = "iMON.exe";
                MediaPortal.GUI.Library.Log.Info("iMONLCDg.RestartFrontview(): ReStarting iMON Manager", new object[0]);
                Process.Start(process.StartInfo);
              }
            }
            else
            {
              Registry.CurrentUser.Close();
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.RestartFrontview(): SoundGraph Registry subkey NOT FOUND. Frontview restart not possible.", new object[0]);
            }
          }
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.RestartFrontview(): completed", new object[0]);
        }
      }
    }

    private void RestoreDisplayFromVideoOrIdle()
    {
      if (this.DisplaySettings.BlankDisplayWithVideo)
      {
        if (this.DisplaySettings.BlankDisplayWhenIdle)
        {
          if (!this.MPStatus.MP_Is_Idle)
          {
            this.DisplayOn();
          }
        }
        else
        {
          this.DisplayOn();
        }
      }
    }

    private void SendData(Command command)
    {
      this.SendData((ulong)command);
    }

    private void SendData(long data)
    {
      this.SendData((ulong)data);
    }

    private void SendData(ulong data)
    {
      try
      {
        if (this.DoDebug)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.SendData(): Sending {0} to display", new object[] { data.ToString("x0000000000000000") });
        }
        if (!this._IMON.iMONLCD_SendData(ref data))
        {
          this.SendData_Error_Count++;
          if (this.SendData_Error_Count > 20)
          {
            this._isDisabled = true;
            if (this.DoDebug)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.SendData(): ERROR Sending {0} to display", new object[] { data.ToString("x0000000000000000") });
            }
            if (this.DoDebug)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.SendData(): ERROR LIMIT EXCEEDED - DISPLAY DISABLED", new object[0]);
            }
          }
          if (this.DoDebug)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.SendData(): ERROR Sending {0} to display", new object[] { data.ToString("x0000000000000000") });
          }
        }
        else
        {
          this.SendData_Error_Count = 0;
          Thread.Sleep(this._delay);
        }
      }
      catch (Exception exception)
      {
        this._isDisabled = true;
        this._errorMessage = exception.Message;
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.SendData(): caught exception '{0}'\nIs your SG_VFD.dll version 5.1 or higher??", new object[0]);
      }
    }

    private void SendData(Command command, ulong optionBitmask)
    {
      this.SendData((ulong)(command) | optionBitmask);
    }

    private void SendPixelArray(byte[] PixelArray)
    {
      if (!this._IsDisplayOff)
      {
        if (PixelArray.Length > 0xc0)
        {
          MediaPortal.GUI.Library.Log.Error("ERROR in iMONLCDg SendPixelArray", new object[0]);
        }
        if (this.DisplayOptions.UseLargeIcons || (this.DisplayOptions.UseLargeIcons & this.DisplayOptions.UseCustomIcons))
        {
          for (int i = 0x5f; i > 0x11; i--)
          {
            PixelArray[i] = PixelArray[i - 0x12];
            PixelArray[i + 0x60] = PixelArray[(i + 0x60) - 0x12];
          }
          for (int j = 0; j < 0x12; j++)
          {
            PixelArray[j] = 0;
            PixelArray[j + 0x60] = 0;
          }
          if ((this.DisplayOptions.UseLargeIcons & !this.DisplayOptions.UseCustomIcons) & this.DoDebug)
          {
            MediaPortal.GUI.Library.Log.Debug("iMONLCDg.SendText(): Inserting Large Icons", new object[0]);
          }
          if (this.DisplayOptions.UseCustomIcons & this.DoDebug)
          {
            MediaPortal.GUI.Library.Log.Debug("iMONLCDg.SendText(): Inserting Custom Large Icons", new object[0]);
          }
          if (this.DisplayOptions.UseInvertedIcons & this.DoDebug)
          {
            MediaPortal.GUI.Library.Log.Debug("iMONLCDg.SendText(): Using inverted Large Icon data", new object[0]);
          }
          for (int k = 0; k < 0x10; k++)
          {
            if (this.DisplayOptions.UseCustomIcons)
            {
              PixelArray[k] = this.CustomLargeIcon.PixelData(this._CurrentLargeIcon, k);
              PixelArray[k + 0x60] = this.CustomLargeIcon.PixelData(this._CurrentLargeIcon, k + 0x10);
            }
            else
            {
              PixelArray[k] = _InternalLargeIcons[this._CurrentLargeIcon, k];
              PixelArray[k + 0x60] = _InternalLargeIcons[this._CurrentLargeIcon, k + 0x10];
            }
            if (this.DisplayOptions.UseInvertedIcons)
            {
              PixelArray[k] = (byte)(PixelArray[k] ^ 0xff);
              PixelArray[k + 0x60] = (byte)(PixelArray[k + 0x60] ^ 0xff);
            }
          }
        }
        int num4 = 0x20;
        lock (DWriteMutex)
        {
          for (int m = 0; m <= 0xbd; m += 7)
          {
            long data = num4;
            for (int n = 6; n >= 0; n--)
            {
              data = data << 8;
              if ((m + n) < PixelArray.Length)
              {
                data += PixelArray[m + n];
              }
            }
            if (num4 <= 0x3b)
            {
              this.SendData(data);
            }
            num4++;
          }
        }
      }
    }

    private void SendPixelArrayRaw(byte[] PixelArray)
    {
      if (!this._IsDisplayOff)
      {
        if (PixelArray.Length > 0xc0)
        {
          MediaPortal.GUI.Library.Log.Error("ERROR in iMONLCDg SendPixelArrayRaw", new object[0]);
        }
        int num = 0x20;
        lock (DWriteMutex)
        {
          for (int i = 0; i <= 0xbd; i += 7)
          {
            long data = num;
            for (int j = 6; j >= 0; j--)
            {
              data = data << 8;
              if ((i + j) < PixelArray.Length)
              {
                data += PixelArray[i + j];
              }
            }
            if (num <= 0x3b)
            {
              this.SendData(data);
            }
            num++;
          }
        }
      }
    }

    private void SendText(string Line1, string Line2)
    {
      if (this.DoDebug)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.SendText(): Called", new object[0]);
      }
      if (this.DisplayOptions.UseCustomFont & this.DoDebug)
      {
        MediaPortal.GUI.Library.Log.Debug("iMONLCDg.SendText(): Using CustomFont", new object[0]);
      }
      int num = 0;
      byte[] pixelArray = new byte[0xc0];
      for (int i = 0; i < Math.Min(0x10, Line1.Length); i++)
      {
        char charID = Line1[i];
        for (int k = 5; k >= 0; k--)
        {
          if ((k + num) < 0x60)
          {
            if (this.DisplayOptions.UseCustomFont)
            {
              pixelArray[num + k] = BitReverse(this.CFont.PixelData(charID, k));
            }
            else
            {
              pixelArray[num + k] = BitReverse(_Font8x5[charID, k]);
            }
          }
        }
        num += 6;
      }
      num = 0x60;
      for (int j = 0; j < Math.Min(0x10, Line2.Length); j++)
      {
        char ch2 = Line2[j];
        for (int m = 5; m >= 0; m--)
        {
          if ((m + num) < 0xc0)
          {
            if (this.DisplayOptions.UseCustomFont)
            {
              pixelArray[num + m] = BitReverse(this.CFont.PixelData(ch2, m));
            }
            else
            {
              pixelArray[num + m] = BitReverse(_Font8x5[ch2, m]);
            }
          }
        }
        num += 6;
      }
      this.SendPixelArray(pixelArray);
    }

    private void SendText3R(string Line1)
    {
      if (this.DoDebug)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.SendText3R(): Called", new object[0]);
      }
      byte[] buffer = new byte[] { 13, 15, 0x20, 0x20, 0x20, 0x20, 0x20, 0, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 2 };
      Line1 = Line1 + "            ";
      Line1 = Line1.Substring(0, 12);
      for (int i = 0; i < 5; i++)
      {
        buffer[2 + i] = (byte)Line1[i];
      }
      for (int j = 5; j < 12; j++)
      {
        buffer[3 + j] = (byte)Line1[j];
      }
      ulong data = (ulong)((((((((buffer[0] << 0x38) + (buffer[1] << 0x30)) | (buffer[2] << 40)) | (buffer[3] << 0x20)) | (buffer[4] << 0x18)) | (buffer[5] << 0x10)) | (buffer[6] << 8)) | buffer[7]);
      ulong num4 = (ulong)((((((((buffer[8] << 0x38) + (buffer[9] << 0x30)) | (buffer[10] << 40)) | (buffer[11] << 0x20)) | (buffer[12] << 0x18)) | (buffer[13] << 0x10)) | (buffer[14] << 8)) | buffer[15]);
      this.SendData((long)0x200020000000000L);
      this.SendData((long)2L);
      this.SendData(data);
      this.SendData(num4);
      if (this.DoDebug)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.SendText3R(): Completed", new object[0]);
      }
    }

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int X, int Y);
    public void SetCustomCharacters(int[][] customCharacters)
    {
    }

    private void SetEQ(byte[] EqDataArray)
    {
      lock (DWriteMutex)
      {
        int num = 0x40;
        for (int i = 0; i <= 0x15; i += 7)
        {
          long data = num;
          for (int j = 6; j >= 0; j--)
          {
            data = data << 8;
            if ((i + j) < EqDataArray.Length)
            {
              data += EqDataArray[i + j];
            }
          }
          if (num <= 0x42)
          {
            this.SendData(data);
          }
          num++;
        }
      }
    }

    public void SetLine(int line, string message)
    {
      if (!this._isDisabled)
      {
        try
        {
          if (this.DoDebug)
          {
            MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.SetLine(): called for Line {0} msg: '{1}'", new object[] { line.ToString(), message });
          }
          if (this._USE_VFD_ICONS & (_DisplayType == DisplayType.VFD))
          {
            this._lines[line] = this.Add_VFD_Icons(line, message);
          }
          else
          {
            this._lines[line] = message;
          }
          if (line == (this._trows - 1))
          {
            this.DisplayLines();
          }
          if (this.DoDebug)
          {
            MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.SetLine(): completed", new object[0]);
          }
        }
        catch (Exception exception)
        {
          MediaPortal.GUI.Library.Log.Debug("(IDisplay) iMONLCDg.SetLine(): CAUGHT EXCEPTION {0}", new object[] { exception });
        }
      }
    }

    private void SetLineLength(int TopLine, int BotLine, int TopProgress, int BotProgress)
    {
      this.SetLinePixels(LengthToPixels(TopLine), LengthToPixels(BotLine), LengthToPixels(TopProgress), LengthToPixels(BotProgress));
    }

    private void SetLinePixels(ulong TopLine, ulong BotLine, ulong TopProgress, ulong BotProgress)
    {
      lock (DWriteMutex)
      {
        ulong optionBitmask = TopProgress << 0x20;
        optionBitmask += TopLine;
        optionBitmask &= (ulong)0xffffffffffffffL;
        this.SendData(Command.SetLines0, optionBitmask);
        optionBitmask = TopProgress >> 0x18;
        optionBitmask += BotProgress << 8;
        optionBitmask += BotLine << 40;
        optionBitmask &= (ulong)0xffffffffffffffL;
        this.SendData(Command.SetLines1, optionBitmask);
        optionBitmask = BotLine >> 0x10;
        this.SendData(Command.SetLines2, optionBitmask);
      }
    }

    private void SetText(string Line1, string Line2)
    {
      lock (DWriteMutex)
      {
        if (this.DoDebug)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.SetText(): Sending text to display", new object[0]);
        }
        this._IMON.iMONVFD_SetText(Line1, Line2);
        Thread.Sleep(this._delay);
      }
    }

    public void Setup(string port, int lines, int cols, int delay, int linesG, int colsG, int timeG, bool backLight, int backlightLevel, bool contrast, int contrastLevel, bool blankOnExit)
    {
      MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.Setup(): called", new object[0]);
      MiniDisplayHelper.InitEQ(ref this.EQSettings);
      MiniDisplayHelper.InitDisplayControl(ref this.DisplaySettings);
      MiniDisplayHelper.InitDisplayOptions(ref this.DisplayOptions);
      this.InitRemoteSettings(ref this.RemoteSettings);
      this._BlankDisplayOnExit = blankOnExit;
      this._Backlight = false;
      this._BacklightLevel = (ulong)backlightLevel;
      this._Contrast = true;
      this._ContrastLevel = ((ulong)contrastLevel) >> 2;
      this.InitializeDriver();
      if (this._DelayStartup)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.Setup(): Delaying device initialization by 10 seconds", new object[0]);
        Thread.Sleep(0x2710);
      }
      this.Check_iMON_Manager_Status();
      if (this._IMON == null)
      {
        this._IMON = new iMONDisplay();
      }
      int fWVersion = -1;
      int rEGVersion = -1;
      if ((this._ForceDisplay == "LCD") || (this._ForceDisplay == "LCD2"))
      {
        if (this._ForceDisplay == "LCD")
        {
          _DisplayType = DisplayType.LCD;
          _VfdType = 0x18;
          MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.Setup(): Advanced options forces display type to LCD", new object[0]);
        }
        else
        {
          _DisplayType = DisplayType.LCD2;
          _VfdType = 0x1b;
          MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.Setup(): Advanced options forces display type to LCD2", new object[0]);
        }
        _VfdReserved = 0x8888;
      }
      else if (this._ForceDisplay == "VFD")
      {
        _DisplayType = DisplayType.VFD;
        _VfdType = 0x10;
        _VfdType = 0x1a;
        _VfdReserved = 0;
        MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.Setup(): Advanced options forces display type to VFD", new object[0]);
      }
      else if (this._ForceDisplay == "LCD3R")
      {
        _DisplayType = DisplayType.ThreeRsystems;
        _VfdType = 9;
        _VfdReserved = 0;
        MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.Setup(): Advanced options forces display type to LCD3R", new object[0]);
      }
      else
      {
        MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.Setup(): Autodetecting iMON Display device", new object[0]);
        try
        {
          MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.Setup(): attempting hardware information test", new object[0]);
          if (this._IMON.RC_Available())
          {
            MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.Setup(): hardware information test - Opening SG_RC.dll", new object[0]);
            if (this._IMON.iMONRC_Init(0x77, 0x83, 0x8888))
            {
              this._IMON.iMONRC_ChangeRCSet(0x77);
              this._IMON.iMONRC_ChangeRC6(1);
              long num4 = this._IMON.iMONRC_CheckDriverVersion();
              int num5 = this._IMON.iMONRC_GetFirmwareVer();
              int num6 = this._IMON.iMONRC_GetHWType();
              int num7 = this._IMON.iMONRC_GetLastRFMode();
              MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.Setup(): RC TEST returned DRVR: 0x{0}, FW: 0x{1} (HW: 0x{2}), RC_HW: 0x{3}, RF: 0x{4}", new object[] { num4.ToString("x0000000000000000"), num5.ToString("x00000000"), GetVFDTypeFromFirmware(num5).ToString("x00000000"), num6.ToString("x00000000"), num7.ToString("x00000000") });
              if (num5 > 0)
              {
                fWVersion = num5;
              }
              MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.Setup(): Closing SG_RC.dll", new object[0]);
              this._IMON.iMONRC_Uninit();
            }
            else
            {
              long num8 = this._IMON.iMONRC_CheckDriverVersion();
              int num9 = this._IMON.iMONRC_GetFirmwareVer();
              int num10 = this._IMON.iMONRC_GetHWType();
              int num11 = this._IMON.iMONRC_GetLastRFMode();
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.Setup(): RC TEST returned DRVR: 0x{0}, FW: 0x{1} (HW: {2}), RC_HW: 0x{3}, RF: 0x{4}", new object[] { num8.ToString("x0000000000000000"), num9.ToString("x00000000"), GetVFDTypeFromFirmware(num9).ToString("x00000000"), num10.ToString("x00000000"), num11.ToString("x00000000") });
              if (num9 > 0)
              {
                MediaPortal.GUI.Library.Log.Info("iMONLCDg.Setup(): Found valid display information", new object[0]);
                fWVersion = num9;
              }
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.Setup(): Closing SG_RC.dll", new object[0]);
            }
          }
          else
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.Setup(): Hardware AutoDetect not available", new object[0]);
          }
        }
        catch (Exception exception)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.Setup(): RC TEST FAILED... SG_RC.dll not found. Exception: {0}", new object[] { exception.ToString() });
        }
        try
        {
          int num3;
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.Setup(): checking registry for ANTEC entries", new object[0]);
          RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Antec\VFD", false);
          if (key != null)
          {
            num3 = (int)key.GetValue("LastVFD", 0);
            if (num3 > 0)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.Setup(): ANTEC registry entries found - HW: {0}", new object[] { num3.ToString("x00") });
              rEGVersion = num3;
            }
            else
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.Setup(): ANTEC \"LastVFD\" key not found", new object[0]);
            }
          }
          else
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.Setup(): ANTEC registry entries NOT found", new object[0]);
          }
          Registry.CurrentUser.Close();
          if (rEGVersion < 0)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.Setup(): checking registry for SOUNDGRAPH entries", new object[0]);
            key = Registry.CurrentUser.OpenSubKey(@"Software\SOUNDGRAPH\iMON", false);
            if (key != null)
            {
              num3 = (int)key.GetValue("LastVFD", 0);
              if (num3 > 0)
              {
                MediaPortal.GUI.Library.Log.Info("iMONLCDg.Setup(): SOUNDGRAPH registry entries found - HW: {0}", new object[] { num3.ToString("x00") });
                rEGVersion = num3;
              }
              else
              {
                MediaPortal.GUI.Library.Log.Info("iMONLCDg.Setup(): SOUNDGRAPH \"LastVFD\" key not found", new object[0]);
              }
            }
            else
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.Setup(): SOUNDGRAPH registry entries NOT found", new object[0]);
            }
            Registry.CurrentUser.Close();
          }
        }
        catch (Exception exception2)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.Setup(): registry test caught exception {0}", new object[] { exception2.ToString() });
        }
        if (fWVersion > -1)
        {
          if (GetDisplayInfoFromFirmware(fWVersion))
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.Setup(): Hardware tests determined - iMON Type: {0}, Display Type: {1} Rsrvd: {2}", new object[] { _VfdType.ToString("x00"), DisplayType.TypeName(_DisplayType), _VfdReserved.ToString("x00") });
          }
          else
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.Setup(): Hardware tests determined UNSUPPORTED display type!", new object[0]);
            _DisplayType = DisplayType.Unsupported;
          }
        }
        else if (rEGVersion > -1)
        {
          if (GetDisplayInfoFromRegistry(rEGVersion))
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.Setup(): Registry tests determined - iMON Type: {0}, Display Type: {1} Rsrvd: {2}", new object[] { _VfdType.ToString("x00"), DisplayType.TypeName(_DisplayType), _VfdReserved.ToString("x00") });
          }
          else
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.Setup(): Registry tests determined UNSUPPORTED display type!", new object[0]);
            _DisplayType = DisplayType.Unsupported;
            this._isDisabled = true;
          }
        }
        else
        {
          MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.Setup(): Display Type could not be determined", new object[0]);
          _DisplayType = DisplayType.Unsupported;
          this._isDisabled = true;
        }
        if (_DisplayType == DisplayType.Unsupported)
        {
          this._isDisabled = true;
          MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.Setup(): Display Type is NOT SUPPORTED - Plugin disabled", new object[0]);
        }
      }
      if (!this._isDisabled)
      {
        try
        {
          MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.Setup(): Testing iMON Display device", new object[0]);
          if (this._IMON.iMONVFD_IsInited())
          {
            MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.Setup(): iMON Display found", new object[0]);
            this._IMON.iMONVFD_Uninit();
          }
          MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.Setup(): opening display type {0}", new object[] { DisplayType.TypeName(_DisplayType) });
          if (!this._IMON.iMONVFD_Init(_VfdType, _VfdReserved))
          {
            MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.Setup(): Open failed - No iMON device found", new object[0]);
            this._isDisabled = true;
            this._errorMessage = "iMONLCDg could not find an iMON LCD display";
          }
          else
          {
            MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.Setup(): iMON Display device found", new object[0]);
            this._IMON.iMONVFD_Uninit();
          }
        }
        catch (Exception exception3)
        {
          this._isDisabled = true;
          this._errorMessage = exception3.Message;
          MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.Setup(): caught an exception.", new object[0]);
        }
      }
      string property = GUIPropertyManager.GetProperty("#currentmodule");
      MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.Setup(): current module = {0}", new object[] { property });
      if ((_DisplayType == DisplayType.LCD) || (_DisplayType == DisplayType.LCD2))
      {
        this._grows = linesG;
        if (this._grows > 0x10)
        {
          this._grows = 0x10;
          MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.Setup(): DISPLAY CONFIGURATION (GRAPHICS MODE) ERROR - Rows must be less then or equal to 16", new object[0]);
        }
        this._gcols = colsG;
        if (this._gcols > 0x60)
        {
          this._gcols = 0x60;
          MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.Setup(): DISPLAY CONFIGURATION (GRAPHICS MODE) ERROR - Columns must be less then or equal to 96", new object[0]);
        }
      }
      else if (_DisplayType == DisplayType.VFD)
      {
        this._trows = lines;
        if (this._trows > 2)
        {
          this._trows = 2;
          MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.Setup(): DISPLAY CONFIGURATION (TEXT MODE) ERROR - Rows must be less then or equal to 2", new object[0]);
        }
        else
        {
          MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.Setup(): _trows (Text Mode Rows) set to {0}", new object[] { this._trows });
        }
        if (this._tcols > 0x10)
        {
          this._gcols = 0x10;
          MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.Setup(): DISPLAY CONFIGURATION (TEXT MODE) ERROR - Columns must be less then or equal to 16", new object[0]);
        }
        else
        {
          MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.Setup(): _tcols (Text Mode Columns) set to {0}", new object[] { this._tcols });
        }
        this.DisplayOptions.DiskMediaStatus = false;
        this.DisplayOptions.VolumeDisplay = false;
        this.DisplayOptions.ProgressDisplay = false;
        this.DisplayOptions.UseCustomFont = false;
        this.DisplayOptions.UseLargeIcons = false;
        this.DisplayOptions.UseCustomIcons = false;
        this.DisplayOptions.UseInvertedIcons = false;
      }
      else if (_DisplayType == DisplayType.ThreeRsystems)
      {
        this._trows = lines;
        if (this._trows > 1)
        {
          this._trows = 1;
          MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.Setup(): DISPLAY CONFIGURATION (3Rsystems MODE) ERROR - Rows must be 1", new object[0]);
        }
        if (this._tcols > 12)
        {
          this._gcols = 12;
          MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.Setup(): DISPLAY CONFIGURATION (3Rsystems MODE) ERROR - Columns must be less then or equal to 12", new object[0]);
        }
        this.DisplayOptions.DiskMediaStatus = false;
        this.DisplayOptions.VolumeDisplay = false;
        this.DisplayOptions.ProgressDisplay = false;
        this.DisplayOptions.UseCustomFont = false;
        this.DisplayOptions.UseLargeIcons = false;
        this.DisplayOptions.UseCustomIcons = false;
        this.DisplayOptions.UseInvertedIcons = false;
      }
      this._delay = delay;
      this._delayG = timeG;
      this._delay = Math.Max(this._delay, this._delayG);
      if ((_DisplayType == DisplayType.LCD) || (_DisplayType == DisplayType.LCD2))
      {
        this._delay = Math.Min(2, this._delay);
      }
      MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.Setup(): Completed", new object[0]);
    }

    private void SetVFDClock()
    {
      if (this._BlankDisplayOnExit | (_DisplayType != DisplayType.VFD))
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.SetVFDClock(): Options specify diplay blank on exit - clock not set", new object[0]);
      }
      else if ((this.DisplaySettings._Shutdown1 != string.Empty) || (this.DisplaySettings._Shutdown2 != string.Empty))
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.SetVFDClock(): Custom Shutdown message defined - clock not set", new object[0]);
      }
      else
      {
        IntPtr zero = IntPtr.Zero;
        string fileName = @"\\.\SGIMON";
        uint bytesReturned = 0;
        DateTime now = DateTime.Now;
        byte[] buffer3 = new byte[8];
        buffer3[7] = 0x40;
        byte[] inBuffer = buffer3;
        byte[] buffer4 = new byte[8];
        buffer4[2] = 1;
        buffer4[7] = 0x42;
        byte[] buffer2 = buffer4;
        inBuffer[0] = (byte)(now.Year & 15L);
        inBuffer[1] = 3;
        inBuffer[2] = (byte)now.Day;
        inBuffer[3] = (byte)now.Month;
        inBuffer[4] = (byte)now.Hour;
        inBuffer[5] = (byte)now.Minute;
        inBuffer[6] = (byte)now.Second;
        zero = MediaPortal.ProcessPlugins.MiniDisplayPlugin.Win32Functions.CreateFile(fileName, 0xc0000000, 3, IntPtr.Zero, 3, 0, IntPtr.Zero);
        if ((((int)zero) == -1) | (((int)zero) == 0))
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.SetVFDClock(): Unable to open device - clock not set", new object[0]);
        }
        else
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.SetVFDClock(): setting the VFD clock", new object[0]);
          MediaPortal.ProcessPlugins.MiniDisplayPlugin.Win32Functions.DeviceIoControl(zero, 0x222018, inBuffer, 8, null, 0, ref bytesReturned, IntPtr.Zero);
          MediaPortal.ProcessPlugins.MiniDisplayPlugin.Win32Functions.DeviceIoControl(zero, 0x222018, buffer2, 8, null, 0, ref bytesReturned, IntPtr.Zero);
          MediaPortal.ProcessPlugins.MiniDisplayPlugin.Win32Functions.CloseHandle(zero);
        }
      }
    }

    private void ShowProgressBars()
    {
      this.progLevel = 0;
      this.volLevel = 0;
      if ((this.MPStatus.MediaPlayer_Playing || MiniDisplayHelper.IsCaptureCardViewing()) & this.DisplayOptions.VolumeDisplay)
      {
        try
        {
          if (!this.MPStatus.IsMuted)
          {
            this.volLevel = this.MPStatus.SystemVolumeLevel / 2048;
          }
        }
        catch (Exception exception)
        {
          if (this.DoDebug)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.ShowProgressBars(): Audio Mixer NOT available! exception: {0}", new object[] { exception });
          }
        }
      }
      if (this.MPStatus.MediaPlayer_Playing & this.DisplayOptions.ProgressDisplay)
      {
        this.progLevel = ((int)(((((float)this.MPStatus.Media_CurrentPosition) / ((float)this.MPStatus.Media_Duration)) - 0.01) * 32.0)) + 1;
      }
      if ((this.LastVolLevel != this.volLevel) || (this.LastProgLevel != this.progLevel))
      {
        if (this.DoDebug)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.ShowProgressBars(): Sending vol: {0} prog: {1} cur: {2} dur: {3} to LCD.", new object[] { this.volLevel.ToString(), this.progLevel.ToString(), this.MPStatus.Media_CurrentPosition.ToString(), this.MPStatus.Media_Duration.ToString() });
        }
        this.SetLineLength(this.volLevel, this.progLevel, this.volLevel, this.progLevel);
      }
      this.LastVolLevel = this.volLevel;
      this.LastProgLevel = this.progLevel;
    }

    private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
      MediaPortal.GUI.Library.Log.Debug("iMONLCDg.SystemEvents_PowerModeChanged: SystemPowerModeChanged event was raised.", new object[0]);
      switch (e.Mode)
      {
        case PowerModes.Resume:
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.SystemEvents_PowerModeChanged: Resume from Suspend or Hibernation detected, restarting display", new object[0]);
          this._IsHandlingPowerEvent = true;
          this.OpenLcd();
          this._IsHandlingPowerEvent = false;
          break;

        case PowerModes.StatusChange:
          break;

        case PowerModes.Suspend:
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.SystemEvents_PowerModeChanged: Suspend or Hibernation detected, shutting down display", new object[0]);
          this._IsHandlingPowerEvent = true;
          this.CloseLcd();
          this._IsHandlingPowerEvent = false;
          return;

        default:
          return;
      }
    }

    public int TestXmlVersion(string xmlPath)
    {
      if (!File.Exists(xmlPath))
      {
        return 3;
      }
      XmlDocument document = new XmlDocument();
      document.Load(xmlPath);
      return Convert.ToInt32(document.DocumentElement.SelectSingleNode("/mappings").Attributes["version"].Value);
    }

    private void UpdateAdvancedSettings()
    {
      if (DateTime.Now.Ticks >= this.LastSettingsCheck.AddMinutes(1.0).Ticks)
      {
        if (this.DoDebug)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.UpdateAdvancedSettings(): called", new object[0]);
        }
        if (File.Exists(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg.xml")))
        {
          FileInfo info = new FileInfo(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg.xml"));
          if (info.LastWriteTime.Ticks > this.SettingsLastModTime.Ticks)
          {
            if (this.DoDebug)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.UpdateAdvancedSettings(): updating advanced settings", new object[0]);
            }
            this.LoadAdvancedSettings();
          }
        }
        if (this.DoDebug)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.UpdateAdvancedSettings(): completed", new object[0]);
        }
      }
    }

    private void UpdateIcons()
    {
      ulong optionBitmask = 0L;
      ulong num2 = 0L;
      bool flag = false;
      DiskIcon icon = new DiskIcon();
      MediaPortal.GUI.Library.Log.Debug("iMONLCDg.UpdateIcons(): Starting Icon Update Thread", new object[0]);
      BuiltinIconMask mask = new BuiltinIconMask();
      CDDrive drive = new CDDrive();
      if (this.DisplaySettings.BlankDisplayWithVideo & this.DisplaySettings.EnableDisplayAction)
      {
        GUIWindowManager.OnNewAction += new OnActionHandler(this.OnExternalAction);
      }
      for (int i = 0; i < 0x1b; i++)
      {
        Inserted_Media[i] = 0;
      }
      if (this.DisplayOptions.DiskIcon & this.DisplayOptions.DiskMonitor)
      {
        char[] cDDriveLetters = CDDrive.GetCDDriveLetters();
        object[] arg = new object[] { cDDriveLetters.Length.ToString() };
        MediaPortal.GUI.Library.Log.Debug("iMONLCDg.UpdateIcons(): Found {0} CD/DVD Drives.", arg);
        for (int j = 0; j < cDDriveLetters.Length; j++)
        {
          if (drive.Open(cDDriveLetters[j]))
          {
            MediaPortal.GUI.Library.Log.Debug("iMONLCDg.UpdateIcons(): Checking media in Drive {0}.", new object[] { cDDriveLetters[j].ToString() });
            bool flag2 = false;
            for (int k = 0; k < 10; k++)
            {
              if (drive.IsCDReady())
              {
                flag2 = true;
              }
              else
              {
                Thread.Sleep(50);
              }
            }
            if (flag2)
            {
              MediaPortal.GUI.Library.Log.Debug("iMONLCDg.UpdateIcons(): Waiting for Drive {0} to refresh.", new object[] { cDDriveLetters[j].ToString() });
              drive.Refresh();
              if (drive.GetNumAudioTracks() > 0)
              {
                Inserted_Media[cDDriveLetters[j] - 'A'] = 1;
                MediaPortal.GUI.Library.Log.Debug("iMONLCDg.UpdateIcons(): Found Audio CD in Drive {0}.", new object[] { cDDriveLetters[j].ToString() });
              }
              else if (File.Exists(cDDriveLetters[j] + @"\VIDEO_TS"))
              {
                Inserted_Media[cDDriveLetters[j] - 'A'] = 2;
                MediaPortal.GUI.Library.Log.Debug("iMONLCDg.UpdateIcons(): Found DVD in Drive {0}.", new object[] { cDDriveLetters[j].ToString() });
              }
              else
              {
                Inserted_Media[cDDriveLetters[j] - 'A'] = 4;
                MediaPortal.GUI.Library.Log.Debug("iMONLCDg.UpdateIcons(): Unknown media found in Drive {0}.", new object[] { cDDriveLetters[j].ToString() });
              }
            }
            else
            {
              Inserted_Media[cDDriveLetters[j] - 'A'] = 0;
              MediaPortal.GUI.Library.Log.Debug("iMONLCDg.UpdateIcons(): No media found in Drive {0}.", new object[] { cDDriveLetters[j].ToString() });
            }
          }
          drive.Close();
        }
      }
      if (this.DisplayOptions.DiskIcon & this.DisplayOptions.DiskMonitor)
      {
        this.ActivateDVM();
      }
      icon.Reset();
      while (true)
      {
        do
        {
          lock (ThreadMutex)
          {
            if (this.DoDebug)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.UpdateIcons(): Checking for Thread termination request", new object[0]);
            }
            if (_stopUpdateIconThread)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.UpdateIcons(): Icon Update Thread terminating", new object[0]);
              _stopUpdateIconThread = false;
              if (this.DisplaySettings.BlankDisplayWithVideo & this.DisplaySettings.EnableDisplayAction)
              {
                GUIWindowManager.OnNewAction -= new OnActionHandler(this.OnExternalAction);
              }
              if (DVM != null)
              {
                DVM.Dispose();
                DVM = null;
              }
              return;
            }
            if ((!this.DVMactive & this.DisplayOptions.DiskIcon) & this.DisplayOptions.DiskMonitor)
            {
              this.ActivateDVM();
            }
            num2 = optionBitmask;
            flag = !flag;
            int num7 = this._CurrentLargeIcon;
            this.LastVolLevel = this.volLevel;
            this.LastProgLevel = this.progLevel;
            int num8 = 0;
            optionBitmask = 0L;
            icon.Off();
            icon.Animate();
            if (this.DoDebug)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.UpdateIcons(): Checking TV Card status: IsAnyCardRecording = {0}, IsViewing = {1}", new object[] { MiniDisplayHelper.IsCaptureCardRecording().ToString(), MiniDisplayHelper.IsCaptureCardViewing().ToString() });
            }
            MiniDisplayHelper.GetSystemStatus(ref this.MPStatus);
            this.Check_Idle_State();
            if (this.DoDebug)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.UpdateIcons(): System Status: Plugin Status = {0}, IsIdle = {1}", new object[] { this.MPStatus.CurrentPluginStatus.ToString(), this.MPStatus.MP_Is_Idle });
            }
            optionBitmask = this.ConvertPluginIconsToDriverIcons(this.MPStatus.CurrentIconMask);
            if ((optionBitmask & ((ulong)0x400000000L)) > 0L)
            {
              num8 = 5;
            }
            else if ((optionBitmask & ((ulong)8L)) > 0L)
            {
              num8 = 1;
            }
            if (MiniDisplayHelper.IsCaptureCardViewing() && !this.MPStatus.Media_IsTimeshifting)
            {
              icon.On();
              icon.InvertOn();
              icon.RotateCW();
            }
            if (this._mpIsIdle)
            {
              num8 = 0;
            }
            if (this.MPStatus.MediaPlayer_Playing)
            {
              icon.On();
              if ((this.MPStatus.CurrentIconMask & ((ulong)0x10L)) > 0L)
              {
                icon.InvertOff();
              }
              else
              {
                icon.InvertOn();
              }
              if ((this.MPStatus.CurrentIconMask & ((ulong)0x10000000000L)) > 0L)
              {
                icon.RotateCCW();
              }
              else
              {
                icon.RotateCW();
              }
              icon.FlashOff();
              if (((((((this.MPStatus.CurrentIconMask & ((ulong)0x40L)) > 0L) | ((this.MPStatus.CurrentIconMask & ((ulong)8L)) > 0L)) | (this.MPStatus.CurrentPluginStatus == Status.PlayingDVD)) | (this.MPStatus.CurrentPluginStatus == Status.PlayingTV)) | (this.MPStatus.CurrentPluginStatus == Status.PlayingVideo)) | (this.MPStatus.CurrentPluginStatus == Status.Timeshifting))
              {
                if ((this.MPStatus.CurrentPluginStatus == Status.PlayingTV) | ((this.MPStatus.CurrentIconMask & ((ulong)8L)) > 0L))
                {
                  num8 = 1;
                }
                else
                {
                  num8 = 2;
                }
                if (this.DisplaySettings.BlankDisplayWithVideo)
                {
                  this.DisplayOff();
                }
              }
              else
              {
                num8 = 3;
              }
              this.GetEQ();
            }
            else if (this.MPStatus.MediaPlayer_Paused)
            {
              icon.On();
              lock (DWriteMutex)
              {
                this.EQSettings._EqDataAvailable = false;
                this._iconThread.Priority = ThreadPriority.BelowNormal;
              }
              this.RestoreDisplayFromVideoOrIdle();
              icon.FlashOn();
              num8 = 6;
            }
            else
            {
              icon.Off();
              this.RestoreDisplayFromVideoOrIdle();
              lock (DWriteMutex)
              {
                this.EQSettings._EqDataAvailable = false;
                this._iconThread.Priority = ThreadPriority.BelowNormal;
              }
            }
            if ((!MiniDisplayHelper.Player_Playing() & !MiniDisplayHelper.IsCaptureCardViewing()) || (this.DisplayOptions.DiskIcon & !this.DisplayOptions.DiskMediaStatus))
            {
              int num9 = 0;
              if (this.DisplayOptions.DiskIcon)
              {
                for (int m = 0; m < 0x1b; m++)
                {
                  num9 |= Inserted_Media[m];
                }
                switch (num9)
                {
                  case 1:
                    optionBitmask |= mask.ICON_CDIn;
                    goto Label_06B0;

                  case 2:
                    optionBitmask |= mask.ICON_DVDIn;
                    goto Label_06B0;
                }
                if (num9 > 0)
                {
                  optionBitmask |= mask.ICON_DiskOn;
                }
              }
            }
          Label_06B0:
            if (this.DisplayOptions.DiskIcon & this.DisplayOptions.DiskMediaStatus)
            {
              optionBitmask |= icon.Mask;
            }
            if (this.DoDebug)
            {
              MediaPortal.GUI.Library.Log.Info("iMONLCDg.UpdateIcons(): last = {0}, new = {1}, disk mask = {2}", new object[] { num2.ToString("X0000000000000000"), optionBitmask.ToString("X0000000000000000"), icon.Mask.ToString("X0000000000000000") });
            }
            if (optionBitmask != num2)
            {
              lock (DWriteMutex)
              {
                this.SendData(Command.SetIcons, optionBitmask);
              }
            }
            this.DisplayEQ();
            if (this.DisplayOptions.VolumeDisplay || this.DisplayOptions.ProgressDisplay)
            {
              lock (DWriteMutex)
              {
                this.ShowProgressBars();
              }
            }
            if (num8 != num7)
            {
              this._CurrentLargeIcon = num8;
            }
          }
        }
        while (this.EQSettings._EqDataAvailable && !this.MPStatus.MediaPlayer_Paused);
        Thread.Sleep(200);
      }
    }

    private void VFD_EQ_Update()
    {
      if (this.DisplaySettings.BlankDisplayWithVideo & this.DisplaySettings.EnableDisplayAction)
      {
        GUIWindowManager.OnNewAction += new OnActionHandler(this.OnExternalAction);
      }
      while (true)
      {
        object obj2;
        Monitor.Enter(obj2 = ThreadMutex);
        try
        {
          if (this.DoDebug)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.VFD_EQ_Update(): Checking for Thread termination request", new object[0]);
          }
          if (_stopUpdateIconThread)
          {
            if (this.DisplaySettings.BlankDisplayWithVideo & this.DisplaySettings.EnableDisplayAction)
            {
              GUIWindowManager.OnNewAction -= new OnActionHandler(this.OnExternalAction);
            }
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.VFD_EQ_Update(): VFD_EQ_Update Thread terminating", new object[0]);
            _stopUpdateIconThread = false;
            break;
          }
          MiniDisplayHelper.GetSystemStatus(ref this.MPStatus);
          if (((!this.MPStatus.MediaPlayer_Active | !this.MPStatus.MediaPlayer_Playing) & this.DisplaySettings.BlankDisplayWithVideo) & (this.DisplaySettings.BlankDisplayWhenIdle & !this._mpIsIdle))
          {
            this.DisplayOn();
          }
          if (this.MPStatus.MediaPlayer_Playing)
          {
            if (this.EQSettings.UseEqDisplay)
            {
              this.GetEQ();
              this.DisplayEQ();
            }
            if (this.DisplaySettings.BlankDisplayWithVideo & (((this.MPStatus.Media_IsDVD || this.MPStatus.Media_IsVideo) || this.MPStatus.Media_IsTV) || this.MPStatus.Media_IsTVRecording))
            {
              if (this.DoDebug)
              {
                MediaPortal.GUI.Library.Log.Info("iMONLCDg.VFD_EQ_Update(): Turning off display while playing video", new object[0]);
              }
              this.DisplayOff();
            }
          }
          else
          {
            this.RestoreDisplayFromVideoOrIdle();
            lock (DWriteMutex)
            {
              this.EQSettings._EqDataAvailable = false;
              this._iconThread.Priority = ThreadPriority.BelowNormal;
            }
          }
        }
        catch (Exception exception)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.VFD_EQ_Update(): CAUGHT EXCEPTION - EXITING! - {0}", new object[] { exception });
          break;
        }
        finally
        {
          Monitor.Exit(obj2);
        }
        if (!this.EQSettings._EqDataAvailable || this.MPStatus.MediaPlayer_Paused)
        {
          Thread.Sleep(250);
        }
      }
    }

    private static void VolumeInserted(int bitMask)
    {
      string str = DVM.MaskToLogicalPaths(bitMask);
      if (Settings.Instance.ExtensiveLogging)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.UpdateDisplay.VolumeInserted(): volume inserted in drive {0}", new object[] { str });
      }
      CDDrive drive = new CDDrive();
      if (drive.IsOpened)
      {
        drive.Close();
      }
      drive.Open(str[0]);
      while (!drive.IsCDReady())
      {
        Thread.Sleep(100);
      }
      drive.Refresh();
      if (drive.GetNumAudioTracks() > 0)
      {
        Inserted_Media[str[0] - 'A'] = 1;
        if (Settings.Instance.ExtensiveLogging)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.UpdateDisplay.VolumeInserted(): Audio CD inserted in drive {0}", new object[] { str });
        }
        drive.Close();
      }
      else
      {
        drive.Close();
        if (Directory.Exists(str + @"\VIDEO_TS"))
        {
          Inserted_Media[str[0] - 'A'] = 2;
          if (Settings.Instance.ExtensiveLogging)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.UpdateDisplay.VolumeInserted(): DVD inserted in drive {0}", new object[] { str });
          }
        }
        else
        {
          Inserted_Media[str[0] - 'A'] = 4;
          if (Settings.Instance.ExtensiveLogging)
          {
            MediaPortal.GUI.Library.Log.Info("iMONLCDg.UpdateDisplay.VolumeInserted(): Unknown Media inserted in drive {0}", new object[] { str });
          }
        }
      }
    }

    private static void VolumeRemoved(int bitMask)
    {
      string str = DVM.MaskToLogicalPaths(bitMask);
      if (Settings.Instance.ExtensiveLogging)
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.UpdateDisplay.VolumeRemoved(): volume removed from drive {0}", new object[] { str });
      }
      Inserted_Media[str[0] - 'A'] = 0;
    }

    public string Description
    {
      get
      {
        return "SoundGraph iMON USB VFD/LCD Plugin V12_01_2008";
      }
    }

    public string ErrorMessage
    {
      get
      {
        return this._errorMessage;
      }
    }

    public bool IsDisabled
    {
      get
      {
        MediaPortal.GUI.Library.Log.Debug("iMONLCDg.IsDisabled: returning {0}", new object[] { this._isDisabled });
        return this._isDisabled;
      }
    }

    public string Name
    {
      get
      {
        return "iMONLCDg";
      }
    }

    public bool SupportsGraphics
    {
      get
      {
        if (this._isDisabled)
        {
          return true;
        }
        if (this._IMON == null)
        {
          return true;
        }
        if (this.DoDebug)
        {
          MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.SupportsGraphics(): _displayType = {0}", new object[] { DisplayType.TypeName(_DisplayType) });
        }
        if (!this._IMON.iMONVFD_IsInited())
        {
          if (this.DoDebug)
          {
            MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.SupportsGraphics(): forcing true for configuration", new object[0]);
          }
          return true;
        }
        if ((_DisplayType == DisplayType.LCD) || (_DisplayType == DisplayType.LCD2))
        {
          if (this.DoDebug)
          {
            MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.SupportsGraphics(): returned true", new object[0]);
          }
          return true;
        }
        if (this.DoDebug)
        {
          MediaPortal.GUI.Library.Log.Info("(IDisplay) iMONLCDg.SupportsGraphics(): returned false", new object[0]);
        }
        return false;
      }
    }

    public bool SupportsText
    {
      get
      {
        return true;
      }
    }

    [Serializable]
    public class AdvancedSettings
    {
      private static bool DoDebug = Settings.Instance.ExtensiveLogging;
      private bool m_BlankDisplayWhenIdle;
      private bool m_BlankDisplayWithVideo;
      private int m_BlankIdleTime = 30;
      private bool m_DelayEQ;
      private int m_DelayEqTime = 10;
      private bool m_DelayStartup;
      private bool m_DeviceMonitor;
      private bool m_DisableRepeat;
      private bool m_DiskIcon;
      private bool m_DiskMediaStatus;
      private string m_DisplayType;
      private bool m_EnableDisplayAction;
      private int m_EnableDisplayActionTime = 5;
      private bool m_EnsureManagerStart;
      private bool m_EqDisplay;
      private int m_EqMode;
      private int m_EqRate = 10;
      private bool m_EQTitleDisplay;
      private int m_EQTitleDisplayTime = 10;
      private int m_EQTitleShowTime = 2;
      private bool m_ForceKeyBoardMode;
      private bool m_ForceManagerRestart;
      private bool m_ForceManagerReload;
      private static iMONLCDg.AdvancedSettings m_Instance;
      private bool m_MonitorPowerState;
      private bool m_NormalEQ = true;
      private bool m_ProgressDisplay;
      private string m_RemoteType = "MCE";
      private int m_RepeatDelay;
      private bool m_RestartFrontviewOnExit;
      private bool m_RestrictEQ;
      private bool m_SmoothEQ;
      private bool m_StereoEQ;
      private bool m_UseCustomFont;
      private bool m_UseCustomIcons;
      private bool m_UseInvertedIcons;
      private bool m_UseLargeIcons;
      private bool m_UseRC;
      private bool m_VFD_UseV3DLL;
      private bool m_VolumeDisplay;
      private bool m_VUindicators;
      private bool m_VUmeter;
      private bool m_VUmeter2;

      public static event OnSettingsChangedHandler OnSettingsChanged;

      public static bool CreateDefaultRemoteMapping()
      {
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.AdvancedSettings.CreateDefaultRemoteMapping(): called", new object[0]);
        bool flag = false;
        string str = "iMon_Remote";
        try
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.AdvancedSettings.CreateDefaultRemoteMapping(): remote mapping file does not exist - Creating default mapping file", new object[0]);
          XmlTextWriter writer = new XmlTextWriter(Config.GetFile(Config.Dir.CustomInputDefault, str + ".xml"), Encoding.UTF8);
          writer.Formatting = Formatting.Indented;
          writer.Indentation = 1;
          writer.IndentChar = '\t';
          writer.WriteStartDocument(true);
          writer.WriteStartElement("mappings");
          writer.WriteAttributeString("version", "3");
          writer.WriteStartElement("remote");
          writer.WriteAttributeString("family", str + "_MCE");
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Power TV");
          writer.WriteAttributeString("code", "101");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "POWER");
          writer.WriteAttributeString("cmdproperty", "EXIT");
          writer.WriteAttributeString("sound", "back.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Record");
          writer.WriteAttributeString("code", "23");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "501");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "113");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "89");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Stop");
          writer.WriteAttributeString("code", "25");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "13");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Pause");
          writer.WriteAttributeString("code", "24");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "105");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "12");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Rewind");
          writer.WriteAttributeString("code", "21");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "600");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "87");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "29");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "17");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Play");
          writer.WriteAttributeString("code", "22");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "105");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "68");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Forward");
          writer.WriteAttributeString("code", "20");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "600");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "86");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "28");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "16");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Replay");
          writer.WriteAttributeString("code", "27");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "PLAYER");
          writer.WriteAttributeString("conproperty", "DVD");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "92");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "600");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "6");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "29");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2005");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "92");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "15");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Skip");
          writer.WriteAttributeString("code", "26");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "PLAYER");
          writer.WriteAttributeString("conproperty", "DVD");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "91");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "600");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "5");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "28");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2005");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "91");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "14");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Back");
          writer.WriteAttributeString("code", "35");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "10");
          writer.WriteAttributeString("sound", "back.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "UP");
          writer.WriteAttributeString("code", "30");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "3");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "DOWN");
          writer.WriteAttributeString("code", "31");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "4");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "LEFT");
          writer.WriteAttributeString("code", "32");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "1");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "RIGHT");
          writer.WriteAttributeString("code", "33");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "2");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "OK");
          writer.WriteAttributeString("code", "34");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "11");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "47");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "10");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "10");
          writer.WriteAttributeString("sound", "back.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "7");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Info");
          writer.WriteAttributeString("code", "15");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "FULLSCREEN");
          writer.WriteAttributeString("conproperty", "true");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "24");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "106");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Volume +");
          writer.WriteAttributeString("code", "16");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "103");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "VOL-");
          writer.WriteAttributeString("code", "17");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "102");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Start");
          writer.WriteAttributeString("code", "13");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "115");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteAttributeString("focus", "True");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Channel Up");
          writer.WriteAttributeString("code", "18");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "7701");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "9979");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "7700");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "9979");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "31");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "95");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "602");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "95");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "5");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Channel Down");
          writer.WriteAttributeString("code", "19");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "7701");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "9980");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "7700");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "9980");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "30");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "94");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "602");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "94");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "6");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Mute");
          writer.WriteAttributeString("code", "14");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "9982");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Recorded TV");
          writer.WriteAttributeString("code", "72");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "WINDOW");
          writer.WriteAttributeString("cmdproperty", "603");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Guide");
          writer.WriteAttributeString("code", "38");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "600");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "10");
          writer.WriteAttributeString("sound", "back.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "WINDOW");
          writer.WriteAttributeString("cmdproperty", "600");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Live TV");
          writer.WriteAttributeString("code", "37");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "7701");
          writer.WriteAttributeString("command", "WINDOW");
          writer.WriteAttributeString("cmdproperty", "602");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "18");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "602");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "18");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "7700");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "18");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "WINDOW");
          writer.WriteAttributeString("cmdproperty", "1");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "DVD Menu");
          writer.WriteAttributeString("code", "36");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "PLAYER");
          writer.WriteAttributeString("conproperty", "DVD");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "90");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "WINDOW");
          writer.WriteAttributeString("cmdproperty", "3001");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "1");
          writer.WriteAttributeString("code", "1");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "37");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "93");
          writer.WriteAttributeString("cmdkeychar", "49");
          writer.WriteAttributeString("cmdkeycode", "0");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "2");
          writer.WriteAttributeString("code", "2");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "38");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "93");
          writer.WriteAttributeString("cmdkeychar", "50");
          writer.WriteAttributeString("cmdkeycode", "0");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "3");
          writer.WriteAttributeString("code", "3");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "39");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "93");
          writer.WriteAttributeString("cmdkeychar", "51");
          writer.WriteAttributeString("cmdkeycode", "0");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "4");
          writer.WriteAttributeString("code", "4");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "40");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "93");
          writer.WriteAttributeString("cmdkeychar", "52");
          writer.WriteAttributeString("cmdkeycode", "0");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "5");
          writer.WriteAttributeString("code", "5");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "41");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "93");
          writer.WriteAttributeString("cmdkeychar", "53");
          writer.WriteAttributeString("cmdkeycode", "0");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "6");
          writer.WriteAttributeString("code", "6");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "42");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "93");
          writer.WriteAttributeString("cmdkeychar", "54");
          writer.WriteAttributeString("cmdkeycode", "0");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "7");
          writer.WriteAttributeString("code", "7");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "43");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "93");
          writer.WriteAttributeString("cmdkeychar", "55");
          writer.WriteAttributeString("cmdkeycode", "0");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "8");
          writer.WriteAttributeString("code", "8");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "44");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "93");
          writer.WriteAttributeString("cmdkeychar", "56");
          writer.WriteAttributeString("cmdkeycode", "0");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "9");
          writer.WriteAttributeString("code", "9");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "45");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "93");
          writer.WriteAttributeString("cmdkeychar", "57");
          writer.WriteAttributeString("cmdkeycode", "0");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "0");
          writer.WriteAttributeString("code", "0");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "80");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "80");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "25");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "80");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "603");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "80");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "605");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "80");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "606");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "80");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "501");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "80");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "601");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "80");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "759");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "80");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "6");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "80");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "10");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "48");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "11");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "48");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "600");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "88");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "93");
          writer.WriteAttributeString("cmdkeychar", "48");
          writer.WriteAttributeString("cmdkeycode", "0");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "*");
          writer.WriteAttributeString("code", "29");
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "#");
          writer.WriteAttributeString("code", "28");
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Clear");
          writer.WriteAttributeString("code", "10");
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Enter");
          writer.WriteAttributeString("code", "11");
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Teletext");
          writer.WriteAttributeString("code", "90");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "602");
          writer.WriteAttributeString("command", "WINDOW");
          writer.WriteAttributeString("cmdproperty", "7701");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "1");
          writer.WriteAttributeString("command", "WINDOW");
          writer.WriteAttributeString("cmdproperty", "7700");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "7701");
          writer.WriteAttributeString("command", "WINDOW");
          writer.WriteAttributeString("cmdproperty", "7700");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "7700");
          writer.WriteAttributeString("command", "WINDOW");
          writer.WriteAttributeString("cmdproperty", "7701");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Red");
          writer.WriteAttributeString("code", "91");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "7701");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "9975");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "7700");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "9975");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "18");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Green");
          writer.WriteAttributeString("code", "92");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "7701");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "9976");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "7700");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "9976");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "26");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Yellow");
          writer.WriteAttributeString("code", "93");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "7701");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "9977");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "7700");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "9977");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "FULLSCREEN");
          writer.WriteAttributeString("conproperty", "true");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "119");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "11");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Blue");
          writer.WriteAttributeString("code", "94");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "7701");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "9978");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "7700");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "9978");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "511");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "9886");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "19");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "My TV");
          writer.WriteAttributeString("code", "70");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "7701");
          writer.WriteAttributeString("command", "WINDOW");
          writer.WriteAttributeString("cmdproperty", "602");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "18");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "602");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "18");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "7700");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "18");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "WINDOW");
          writer.WriteAttributeString("cmdproperty", "1");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "My Music");
          writer.WriteAttributeString("code", "71");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "WINDOW");
          writer.WriteAttributeString("cmdproperty", "501");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "My Pictures");
          writer.WriteAttributeString("code", "73");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "WINDOW");
          writer.WriteAttributeString("cmdproperty", "2");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "My Videos");
          writer.WriteAttributeString("code", "74");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "6");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "18");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2005");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "18");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "WINDOW");
          writer.WriteAttributeString("cmdproperty", "6");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "My Radio");
          writer.WriteAttributeString("code", "80");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "WINDOW");
          writer.WriteAttributeString("cmdproperty", "30");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Messenger");
          writer.WriteAttributeString("code", "105");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "WINDOW");
          writer.WriteAttributeString("cmdproperty", "32");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Aspect Ratio / Power PC");
          writer.WriteAttributeString("code", "12");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "19");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Print");
          writer.WriteAttributeString("code", "78");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "19");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("remote");
          writer.WriteAttributeString("family", str + "_PAD");
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "App Exit");
          writer.WriteAttributeString("code", "2");
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Power");
          writer.WriteAttributeString("code", "16");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "POWER");
          writer.WriteAttributeString("cmdproperty", "EXIT");
          writer.WriteAttributeString("sound", "back.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Record");
          writer.WriteAttributeString("code", "64");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "501");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "113");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "89");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Play");
          writer.WriteAttributeString("code", "128");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "105");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "68");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Eject");
          writer.WriteAttributeString("code", "114");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "105");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "68");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Rewind");
          writer.WriteAttributeString("code", "130");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "600");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "87");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "29");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "17");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Pause");
          writer.WriteAttributeString("code", "144");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "105");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "12");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Forward");
          writer.WriteAttributeString("code", "192");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "600");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "86");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "28");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "16");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Replay");
          writer.WriteAttributeString("code", "208");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "PLAYER");
          writer.WriteAttributeString("conproperty", "DVD");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "92");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "600");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "6");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "29");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2005");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "92");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "15");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Stop");
          writer.WriteAttributeString("code", "220");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "13");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Skip");
          writer.WriteAttributeString("code", "66");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "PLAYER");
          writer.WriteAttributeString("conproperty", "DVD");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "91");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "600");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "5");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "28");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2005");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "91");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "14");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Backspace");
          writer.WriteAttributeString("code", "32");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "10");
          writer.WriteAttributeString("sound", "back.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Mouse / Keyboard");
          writer.WriteAttributeString("code", "80");
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Select / Space");
          writer.WriteAttributeString("code", "148");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "11");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "47");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "10");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "10");
          writer.WriteAttributeString("sound", "back.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "7");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Windows Key");
          writer.WriteAttributeString("code", "192");
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Menu Key");
          writer.WriteAttributeString("code", "60");
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Enter");
          writer.WriteAttributeString("code", "34");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "11");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "47");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "10");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "10");
          writer.WriteAttributeString("sound", "back.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "7");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Esc");
          writer.WriteAttributeString("code", "252");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "10");
          writer.WriteAttributeString("sound", "back.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Eject2");
          writer.WriteAttributeString("code", "86");
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "App Launch");
          writer.WriteAttributeString("code", "124");
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Start Button");
          writer.WriteAttributeString("code", "178");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "115");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteAttributeString("focus", "True");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Task Switcher");
          writer.WriteAttributeString("code", "150");
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Mute");
          writer.WriteAttributeString("code", "218");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "9982");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Volume +");
          writer.WriteAttributeString("code", "38");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "103");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Channel Up");
          writer.WriteAttributeString("code", "22");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "7701");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "9979");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "7700");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "9979");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "31");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "95");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "602");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "95");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "5");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Timer");
          writer.WriteAttributeString("code", "198");
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Volume Down");
          writer.WriteAttributeString("code", "42");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "102");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Channel Down");
          writer.WriteAttributeString("code", "14");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "7701");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "9980");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "7700");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "9980");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "30");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "94");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "602");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "94");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "6");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "1");
          writer.WriteAttributeString("code", "58");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "37");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "93");
          writer.WriteAttributeString("cmdkeychar", "49");
          writer.WriteAttributeString("cmdkeycode", "0");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "2");
          writer.WriteAttributeString("code", "242");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "38");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "93");
          writer.WriteAttributeString("cmdkeychar", "50");
          writer.WriteAttributeString("cmdkeycode", "0");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "3");
          writer.WriteAttributeString("code", "50");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "39");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "93");
          writer.WriteAttributeString("cmdkeychar", "51");
          writer.WriteAttributeString("cmdkeycode", "0");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "4");
          writer.WriteAttributeString("code", "138");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "40");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "93");
          writer.WriteAttributeString("cmdkeychar", "52");
          writer.WriteAttributeString("cmdkeycode", "0");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "5");
          writer.WriteAttributeString("code", "90");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "41");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "93");
          writer.WriteAttributeString("cmdkeychar", "53");
          writer.WriteAttributeString("cmdkeycode", "0");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "6");
          writer.WriteAttributeString("code", "170");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "42");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "93");
          writer.WriteAttributeString("cmdkeychar", "54");
          writer.WriteAttributeString("cmdkeycode", "0");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "7");
          writer.WriteAttributeString("code", "214");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "43");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "93");
          writer.WriteAttributeString("cmdkeychar", "55");
          writer.WriteAttributeString("cmdkeycode", "0");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "8");
          writer.WriteAttributeString("code", "136");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "44");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "93");
          writer.WriteAttributeString("cmdkeychar", "56");
          writer.WriteAttributeString("cmdkeycode", "0");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "9");
          writer.WriteAttributeString("code", "160");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "45");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "93");
          writer.WriteAttributeString("cmdkeychar", "57");
          writer.WriteAttributeString("cmdkeycode", "0");
          writer.WriteAttributeString("sound", "cursor.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "*");
          writer.WriteAttributeString("code", "56");
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "0");
          writer.WriteAttributeString("code", "234");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2007");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "80");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "80");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "25");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "80");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "603");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "80");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "605");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "80");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "606");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "80");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "501");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "80");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "601");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "80");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "759");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "80");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "6");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "80");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "10");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "48");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "11");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "48");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "600");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "88");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "93");
          writer.WriteAttributeString("cmdkeychar", "48");
          writer.WriteAttributeString("cmdkeycode", "0");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "#");
          writer.WriteAttributeString("code", "96");
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "My Movie");
          writer.WriteAttributeString("code", "200");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "6");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "18");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "2005");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "18");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "WINDOW");
          writer.WriteAttributeString("cmdproperty", "6");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "My Music");
          writer.WriteAttributeString("code", "82");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "WINDOW");
          writer.WriteAttributeString("cmdproperty", "501");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "My Photo");
          writer.WriteAttributeString("code", "240");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "WINDOW");
          writer.WriteAttributeString("cmdproperty", "2");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "My TV");
          writer.WriteAttributeString("code", "40");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "7701");
          writer.WriteAttributeString("command", "WINDOW");
          writer.WriteAttributeString("cmdproperty", "602");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "18");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "602");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "18");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "WINDOW");
          writer.WriteAttributeString("conproperty", "7700");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "18");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "WINDOW");
          writer.WriteAttributeString("cmdproperty", "1");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Bookmark");
          writer.WriteAttributeString("code", "8");
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Thumbnail");
          writer.WriteAttributeString("code", "188");
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Aspect Ratio");
          writer.WriteAttributeString("code", "106");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "19");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Fullscreen");
          writer.WriteAttributeString("code", "166");
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "My DVD");
          writer.WriteAttributeString("code", "102");
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "PLAYER");
          writer.WriteAttributeString("conproperty", "DVD");
          writer.WriteAttributeString("command", "ACTION");
          writer.WriteAttributeString("cmdproperty", "90");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteStartElement("action");
          writer.WriteAttributeString("layer", "0");
          writer.WriteAttributeString("condition", "*");
          writer.WriteAttributeString("conproperty", "-1");
          writer.WriteAttributeString("command", "WINDOW");
          writer.WriteAttributeString("cmdproperty", "3001");
          writer.WriteAttributeString("sound", "click.wav");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Menu");
          writer.WriteAttributeString("code", "246");
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Caption");
          writer.WriteAttributeString("code", "74");
          writer.WriteEndElement();
          writer.WriteStartElement("button");
          writer.WriteAttributeString("name", "Language");
          writer.WriteAttributeString("code", "202");
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.WriteEndDocument();
          writer.Close();
          flag = true;
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.AdvancedSettings.CreateDefaultRemoteMapping: remote mapping file created", new object[0]);
        }
        catch
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.AdvancedSettings.CreateDefaultRemoteMapping: Error saving remote mapping to XML file", new object[0]);
          flag = false;
        }
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.AdvancedSettings.CreateDefaultRemoteMapping: completed", new object[0]);
        return flag;
      }

      private static void Default(iMONLCDg.AdvancedSettings _settings)
      {
        _settings.DelayStartup = false;
        _settings.DisplayType = null;
        _settings.DiskIcon = false;
        _settings.DiskMediaStatus = false;
        _settings.DeviceMonitor = false;
        _settings.ProgressDisplay = false;
        _settings.VolumeDisplay = false;
        _settings.UseCustomFont = false;
        _settings.UseLargeIcons = false;
        _settings.UseCustomIcons = false;
        _settings.UseInvertedIcons = false;
        _settings.EnsureManagerStartup = false;
        _settings.ForceManagerRestart = false;
        _settings.ForceManagerReload = false;
        _settings.RestartFrontviewOnExit = false;
        _settings.ForceKeyBoardMode = false;
        _settings.EqDisplay = false;
        _settings.NormalEQ = true;
        _settings.StereoEQ = false;
        _settings.VUmeter = false;
        _settings.VUmeter2 = false;
        _settings.VUindicators = false;
        _settings.EqMode = 0;
        _settings.RestrictEQ = false;
        _settings.EqRate = 10;
        _settings.DelayEQ = false;
        _settings.DelayEqTime = 10;
        _settings.SmoothEQ = false;
        _settings.BlankDisplayWithVideo = false;
        _settings.EnableDisplayAction = false;
        _settings.EnableDisplayActionTime = 5;
        _settings.VFD_UseV3DLL = false;
        _settings.MonitorPowerState = false;
        _settings.EQTitleDisplay = false;
        _settings.EQTitleDisplayTime = 10;
        _settings.EQTitleShowTime = 2;
        _settings.BlankDisplayWhenIdle = false;
        _settings.BlankIdleTime = 30;
        _settings.UseRC = false;
        _settings.RemoteType = "MCE";
        _settings.DisableRepeat = false;
        _settings.RepeatDelay = 4;
      }

      public static iMONLCDg.AdvancedSettings Load()
      {
        iMONLCDg.AdvancedSettings settings;
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.AdvancedSettings.Load(): started", new object[0]);
        if (File.Exists(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg.xml")))
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.AdvancedSettings.Load(): Loading settings from XML file", new object[0]);
          XmlSerializer serializer = new XmlSerializer(typeof(iMONLCDg.AdvancedSettings));
          XmlTextReader xmlReader = new XmlTextReader(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg.xml"));
          settings = (iMONLCDg.AdvancedSettings)serializer.Deserialize(xmlReader);
          xmlReader.Close();
        }
        else
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.AdvancedSettings.Load(): Loading settings from defaults", new object[0]);
          settings = new iMONLCDg.AdvancedSettings();
          Default(settings);
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.AdvancedSettings.Load(): Loaded settings from defaults", new object[0]);
        }
        MediaPortal.GUI.Library.Log.Info("iMONLCDg.AdvancedSettings.Load(): completed", new object[0]);
        return settings;
      }

      public static void NotifyDriver()
      {
        if (OnSettingsChanged != null)
        {
          OnSettingsChanged();
        }
      }

      public static void Save()
      {
        Save(Instance);
      }

      public static void Save(iMONLCDg.AdvancedSettings ToSave)
      {
        if (DoDebug)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.AdvancedSettings.Save(): Saving settings to XML file", new object[0]);
        }
        XmlSerializer serializer = new XmlSerializer(typeof(iMONLCDg.AdvancedSettings));
        XmlTextWriter writer = new XmlTextWriter(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg.xml"), Encoding.UTF8);
        writer.Formatting = Formatting.Indented;
        writer.Indentation = 2;
        serializer.Serialize((XmlWriter)writer, ToSave);
        writer.Close();
        if (DoDebug)
        {
          MediaPortal.GUI.Library.Log.Info("iMONLCDg.AdvancedSettings.Save(): completed", new object[0]);
        }
      }

      public static void SetDefaults()
      {
        Default(Instance);
      }

      [XmlAttribute]
      public bool BlankDisplayWhenIdle
      {
        get
        {
          return this.m_BlankDisplayWhenIdle;
        }
        set
        {
          this.m_BlankDisplayWhenIdle = value;
        }
      }

      [XmlAttribute]
      public bool BlankDisplayWithVideo
      {
        get
        {
          return this.m_BlankDisplayWithVideo;
        }
        set
        {
          this.m_BlankDisplayWithVideo = value;
        }
      }

      [XmlAttribute]
      public int BlankIdleTime
      {
        get
        {
          return this.m_BlankIdleTime;
        }
        set
        {
          this.m_BlankIdleTime = value;
        }
      }

      [XmlAttribute]
      public bool DelayEQ
      {
        get
        {
          return this.m_DelayEQ;
        }
        set
        {
          this.m_DelayEQ = value;
        }
      }

      [XmlAttribute]
      public int DelayEqTime
      {
        get
        {
          return this.m_DelayEqTime;
        }
        set
        {
          this.m_DelayEqTime = value;
        }
      }

      [XmlAttribute]
      public bool DelayStartup
      {
        get
        {
          return this.m_DelayStartup;
        }
        set
        {
          this.m_DelayStartup = value;
        }
      }

      [XmlAttribute]
      public bool DeviceMonitor
      {
        get
        {
          return this.m_DeviceMonitor;
        }
        set
        {
          this.m_DeviceMonitor = value;
        }
      }

      [XmlAttribute]
      public bool DisableRepeat
      {
        get
        {
          return this.m_DisableRepeat;
        }
        set
        {
          this.m_DisableRepeat = value;
        }
      }

      [XmlAttribute]
      public bool DiskIcon
      {
        get
        {
          return this.m_DiskIcon;
        }
        set
        {
          this.m_DiskIcon = value;
        }
      }

      [XmlAttribute]
      public bool DiskMediaStatus
      {
        get
        {
          return this.m_DiskMediaStatus;
        }
        set
        {
          this.m_DiskMediaStatus = value;
        }
      }

      [XmlAttribute]
      public string DisplayType
      {
        get
        {
          return this.m_DisplayType;
        }
        set
        {
          this.m_DisplayType = value;
        }
      }

      [XmlAttribute]
      public bool EnableDisplayAction
      {
        get
        {
          return this.m_EnableDisplayAction;
        }
        set
        {
          this.m_EnableDisplayAction = value;
        }
      }

      [XmlAttribute]
      public int EnableDisplayActionTime
      {
        get
        {
          return this.m_EnableDisplayActionTime;
        }
        set
        {
          this.m_EnableDisplayActionTime = value;
        }
      }

      [XmlAttribute]
      public bool EnsureManagerStartup
      {
        get
        {
          return this.m_EnsureManagerStart;
        }
        set
        {
          this.m_EnsureManagerStart = value;
        }
      }

      [XmlAttribute]
      public bool EqDisplay
      {
        get
        {
          return this.m_EqDisplay;
        }
        set
        {
          this.m_EqDisplay = value;
        }
      }

      [XmlAttribute]
      public int EqMode
      {
        get
        {
          return this.m_EqMode;
        }
        set
        {
          this.m_EqMode = value;
        }
      }

      [XmlAttribute]
      public int EqRate
      {
        get
        {
          return this.m_EqRate;
        }
        set
        {
          this.m_EqRate = value;
        }
      }

      [XmlAttribute]
      public bool EQTitleDisplay
      {
        get
        {
          return this.m_EQTitleDisplay;
        }
        set
        {
          this.m_EQTitleDisplay = value;
        }
      }

      [XmlAttribute]
      public int EQTitleDisplayTime
      {
        get
        {
          return this.m_EQTitleDisplayTime;
        }
        set
        {
          this.m_EQTitleDisplayTime = value;
        }
      }

      [XmlAttribute]
      public int EQTitleShowTime
      {
        get
        {
          return this.m_EQTitleShowTime;
        }
        set
        {
          this.m_EQTitleShowTime = value;
        }
      }

      [XmlAttribute]
      public bool ForceKeyBoardMode
      {
        get
        {
          return this.m_ForceKeyBoardMode;
        }
        set
        {
          this.m_ForceKeyBoardMode = value;
        }
      }

      [XmlAttribute]
      public bool ForceManagerRestart
      {
        get
        {
          return this.m_ForceManagerRestart;
        }
        set
        {
          this.m_ForceManagerRestart = value;
        }
      }

      [XmlAttribute]
      public bool ForceManagerReload
      {
        get
        {
          return this.m_ForceManagerReload;
        }
        set
        {
          this.m_ForceManagerReload = value;
        }
      }

      public static iMONLCDg.AdvancedSettings Instance
      {
        get
        {
          if (m_Instance == null)
          {
            m_Instance = Load();
          }
          return m_Instance;
        }
        set
        {
          m_Instance = value;
        }
      }

      [XmlAttribute]
      public bool MonitorPowerState
      {
        get
        {
          return this.m_MonitorPowerState;
        }
        set
        {
          this.m_MonitorPowerState = value;
        }
      }

      [XmlAttribute]
      public bool NormalEQ
      {
        get
        {
          return this.m_NormalEQ;
        }
        set
        {
          this.m_NormalEQ = value;
        }
      }

      [XmlAttribute]
      public bool ProgressDisplay
      {
        get
        {
          return this.m_ProgressDisplay;
        }
        set
        {
          this.m_ProgressDisplay = value;
        }
      }

      [XmlAttribute]
      public string RemoteType
      {
        get
        {
          return this.m_RemoteType;
        }
        set
        {
          this.m_RemoteType = value;
        }
      }

      [XmlAttribute]
      public int RepeatDelay
      {
        get
        {
          return this.m_RepeatDelay;
        }
        set
        {
          this.m_RepeatDelay = value;
        }
      }

      [XmlAttribute]
      public bool RestartFrontviewOnExit
      {
        get
        {
          return this.m_RestartFrontviewOnExit;
        }
        set
        {
          this.m_RestartFrontviewOnExit = value;
        }
      }

      [XmlAttribute]
      public bool RestrictEQ
      {
        get
        {
          return this.m_RestrictEQ;
        }
        set
        {
          this.m_RestrictEQ = value;
        }
      }

      [XmlAttribute]
      public bool SmoothEQ
      {
        get
        {
          return this.m_SmoothEQ;
        }
        set
        {
          this.m_SmoothEQ = value;
        }
      }

      [XmlAttribute]
      public bool StereoEQ
      {
        get
        {
          return this.m_StereoEQ;
        }
        set
        {
          this.m_StereoEQ = value;
        }
      }

      [XmlAttribute]
      public bool UseCustomFont
      {
        get
        {
          return this.m_UseCustomFont;
        }
        set
        {
          this.m_UseCustomFont = value;
        }
      }

      [XmlAttribute]
      public bool UseCustomIcons
      {
        get
        {
          return this.m_UseCustomIcons;
        }
        set
        {
          this.m_UseCustomIcons = value;
        }
      }

      [XmlAttribute]
      public bool UseInvertedIcons
      {
        get
        {
          return this.m_UseInvertedIcons;
        }
        set
        {
          this.m_UseInvertedIcons = value;
        }
      }

      [XmlAttribute]
      public bool UseLargeIcons
      {
        get
        {
          return this.m_UseLargeIcons;
        }
        set
        {
          this.m_UseLargeIcons = value;
        }
      }

      [XmlAttribute]
      public bool UseRC
      {
        get
        {
          return this.m_UseRC;
        }
        set
        {
          this.m_UseRC = value;
        }
      }

      [XmlAttribute]
      public bool VFD_UseV3DLL
      {
        get
        {
          return this.m_VFD_UseV3DLL;
        }
        set
        {
          this.m_VFD_UseV3DLL = value;
        }
      }

      [XmlAttribute]
      public bool VolumeDisplay
      {
        get
        {
          return this.m_VolumeDisplay;
        }
        set
        {
          this.m_VolumeDisplay = value;
        }
      }

      [XmlAttribute]
      public bool VUindicators
      {
        get
        {
          return this.m_VUindicators;
        }
        set
        {
          this.m_VUindicators = value;
        }
      }

      [XmlAttribute]
      public bool VUmeter
      {
        get
        {
          return this.m_VUmeter;
        }
        set
        {
          this.m_VUmeter = value;
        }
      }

      [XmlAttribute]
      public bool VUmeter2
      {
        get
        {
          return this.m_VUmeter2;
        }
        set
        {
          this.m_VUmeter2 = value;
        }
      }

      public delegate void OnSettingsChangedHandler();
    }

    public class BuiltinIconMask
    {
      public readonly ulong ICON_AC3 = 0x10000000L;
      public readonly ulong ICON_Alarm = 0x800000000L;
      public readonly ulong ICON_ALL = 0xffffffffffffffL;
      public readonly ulong ICON_CD_DVD = 0x10L;
      public readonly ulong ICON_CDIn = 0x806b0000000000L;
      public readonly ulong ICON_DiskOff = 0x7f7000ffffffffffL;
      public readonly ulong ICON_DiskOn = 0x80ff0000000000L;
      public readonly ulong ICON_DivX = 0x10000L;
      public readonly ulong ICON_DTS = 0x8000000L;
      public readonly ulong ICON_DVDIn = 0x80550000000000L;
      public readonly ulong ICON_FIT = 0x400000L;
      public readonly ulong ICON_HDTV = 0x100000L;
      public readonly ulong ICON_Movie = 0x40L;
      public readonly ulong ICON_MP3 = 0x2000000L;
      public readonly ulong ICON_MPG = 0x20000L;
      public readonly ulong ICON_MPG2 = 0x20000000L;
      public readonly ulong ICON_Music = 0x80L;
      public readonly ulong ICON_News = 2L;
      public readonly ulong ICON_OGG = 0x1000000L;
      public readonly ulong ICON_Photo = 0x20L;
      public readonly ulong ICON_Rec = 0x400000000L;
      public readonly ulong ICON_REP = 0x2000000000L;
      public readonly ulong ICON_SCR1 = 0x80000L;
      public readonly ulong ICON_SCR2 = 0x40000L;
      public readonly ulong ICON_SFL = 0x1000000000L;
      public readonly ulong ICON_SRC = 0x800000L;
      public readonly ulong ICON_Time = 0x100000000L;
      public readonly ulong ICON_TV = 8L;
      public readonly ulong ICON_TV_2 = 0x200000L;
      public readonly ulong ICON_Vol = 0x200000000L;
      public readonly ulong ICON_WAV = 0x4000000000L;
      public readonly ulong ICON_WebCast = 4L;
      public readonly ulong ICON_WMA = 0x4000000L;
      public readonly ulong ICON_WMA2 = 0x8000000000L;
      public readonly ulong ICON_WMV = 0x40000000L;
      public readonly ulong ICON_xVid = 0x80000000L;
      public readonly ulong SPKR_FC = 0x8000L;
      public readonly ulong SPKR_FL = 1L;
      public readonly ulong SPKR_FR = 0x4000L;
      public readonly ulong SPKR_LFE = 0x1000L;
      public readonly ulong SPKR_RL = 0x400L;
      public readonly ulong SPKR_RR = 0x100L;
      public readonly ulong SPKR_SL = 0x2000L;
      public readonly ulong SPKR_SPDIF = 0x200L;
      public readonly ulong SPKR_SR = 0x800L;
    }

    private enum Command : ulong
    {
      ClearAlarm = 0x5100000000000000L,
      DisplayControl = 0x5000000000000000L,
      DisplayOn = 0x5000000000000040L,
      KeypadLightOff = 0x400000000000000L,
      KeypadLightOn = 0x400000000000004L,
      LCD2_DisplayControl = 9799832789158199296L,
      LCD2_DisplayOn = 9799832789158199360L,
      LCD2_HIDMode = 9223372036854775808L,
      LCD2_Init86 = 9655717601082343424L,
      LCD2_Init88 = 9799832789158199360L,
      LCD2_Init8A = 9943947977234055168L,
      LCD2_Init8C = 10088063165309911183L,
      LCD2_Shutdown = 9799832789158199304L,
      SetContrast = 0x300000000000000L,
      SetIcons = 0x100000000000000L,
      SetLines0 = 0x1000000000000000L,
      SetLines1 = 0x1100000000000000L,
      SetLines2 = 0x1200000000000000L,
      Shutdown = 0x5000000000000008L,
      VFD2_DisplayOff = 9944230551722393856L,
      VFD2_ShowClock = 9943949076745683200L
    }

    private class CustomFont
    {
      private readonly DataColumn CData0 = new DataColumn("CData0");
      private readonly DataColumn CData1 = new DataColumn("CData1");
      private readonly DataColumn CData2 = new DataColumn("CData2");
      private readonly DataColumn CData3 = new DataColumn("CData3");
      private readonly DataColumn CData4 = new DataColumn("CData4");
      private readonly DataColumn CData5 = new DataColumn("CData5");
      private readonly DataColumn CID = new DataColumn("CharID");
      private static byte[,] CstmFont;
      private DisplayOptions CustomOptions = XMLUTILS.LoadDisplayOptionsSettings();
      private DataTable FontData = new DataTable("Character");

      public void CloseFont()
      {
        if (this.FontData != null)
        {
          this.FontData.Dispose();
        }
      }

      public void InitializeCustomFont()
      {
        if (this.CustomOptions.UseCustomFont)
        {
          if (this.FontData.Columns.Count == 0)
          {
            this.FontData.Rows.Clear();
            this.FontData.Columns.Clear();
            CstmFont = new byte[0x100, 6];
            this.CID.DataType = typeof(byte);
            this.FontData.Columns.Add(this.CID);
            this.CData0.DataType = typeof(byte);
            this.FontData.Columns.Add(this.CData0);
            this.CData1.DataType = typeof(byte);
            this.FontData.Columns.Add(this.CData1);
            this.CData2.DataType = typeof(byte);
            this.FontData.Columns.Add(this.CData2);
            this.CData3.DataType = typeof(byte);
            this.FontData.Columns.Add(this.CData3);
            this.CData4.DataType = typeof(byte);
            this.FontData.Columns.Add(this.CData4);
            this.CData5.DataType = typeof(byte);
            this.FontData.Columns.Add(this.CData5);
            this.FontData.Clear();
          }
          if (this.LoadCustomFontData())
          {
            MediaPortal.GUI.Library.Log.Debug("iMONLCDg.InitializeCustomFont(): Custom font data loaded", new object[0]);
          }
          else
          {
            this.SaveDefaultFontData();
            MediaPortal.GUI.Library.Log.Debug("iMONLCDg.InitializeCustomFont(): Custom font file not found. Template file saved. loaded default file.", new object[0]);
          }
        }
      }

      private bool LoadCustomFontData()
      {
        MediaPortal.GUI.Library.Log.Debug("LoadCustomFontData(): called", new object[0]);
        if (File.Exists(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg_font.xml")))
        {
          this.FontData.Rows.Clear();
          XmlSerializer serializer = new XmlSerializer(typeof(DataTable));
          XmlTextReader xmlReader = new XmlTextReader(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg_font.xml"));
          MediaPortal.GUI.Library.Log.Debug("LoadCustomFontData(): DeSerializing data", new object[0]);
          this.FontData = (DataTable)serializer.Deserialize(xmlReader);
          MediaPortal.GUI.Library.Log.Debug("LoadCustomFontData(): Read data from file", new object[0]);
          xmlReader.Close();
          MediaPortal.GUI.Library.Log.Debug("LoadCustomFontData(): Converting font data", new object[0]);
          for (int j = 0; j < 0x100; j++)
          {
            DataRow row = this.FontData.Rows[j];
            CstmFont[j, 0] = (byte)row[1];
            CstmFont[j, 1] = (byte)row[2];
            CstmFont[j, 2] = (byte)row[3];
            CstmFont[j, 3] = (byte)row[4];
            CstmFont[j, 4] = (byte)row[5];
            CstmFont[j, 5] = (byte)row[6];
          }
          MediaPortal.GUI.Library.Log.Debug("LoadCustomFontData(): completed", new object[0]);
          return true;
        }
        MediaPortal.GUI.Library.Log.Debug("LoadCustomFontData(): Loading Custom Font from default Font", new object[0]);
        for (int i = 0; i < 0x100; i++)
        {
          for (int k = 0; k < 6; k++)
          {
            CstmFont[i, k] = iMONLCDg._Font8x5[i, k];
          }
        }
        MediaPortal.GUI.Library.Log.Debug("LoadCustomFontData(): completed", new object[0]);
        return false;
      }

      public byte PixelData(int CharID, int CharIndex)
      {
        return CstmFont[CharID, CharIndex];
      }

      private void SaveDefaultFontData()
      {
        MediaPortal.GUI.Library.Log.Debug("SaveFontData(): called", new object[0]);
        MediaPortal.GUI.Library.Log.Debug("SaveFontData(): Converting font data", new object[0]);
        this.FontData.Rows.Clear();
        for (int i = 0; i < 0x100; i++)
        {
          DataRow row = this.FontData.NewRow();
          row[0] = i;
          row[1] = iMONLCDg._Font8x5[i, 0];
          row[2] = iMONLCDg._Font8x5[i, 1];
          row[3] = iMONLCDg._Font8x5[i, 2];
          row[4] = iMONLCDg._Font8x5[i, 3];
          row[5] = iMONLCDg._Font8x5[i, 4];
          row[6] = iMONLCDg._Font8x5[i, 5];
          this.FontData.Rows.Add(row);
        }
        XmlSerializer serializer = new XmlSerializer(typeof(DataTable));
        TextWriter textWriter = new StreamWriter(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg_font.xml"));
        MediaPortal.GUI.Library.Log.Debug("SaveFontData(): Serializing data", new object[0]);
        serializer.Serialize(textWriter, this.FontData);
        MediaPortal.GUI.Library.Log.Debug("SaveFontData(): Writing data to file", new object[0]);
        textWriter.Close();
        MediaPortal.GUI.Library.Log.Debug("SaveFontData(): completed", new object[0]);
      }
    }

    public class DiskIcon
    {
      private bool _diskFlash;
      private bool _diskInverted;
      private readonly ulong[] _DiskMask = new ulong[ ] { 0x80fe0000000000L, 0x80fd0000000000L, 0x80fb0000000000L, 0x80f70000000000L, 0x80ef0000000000L, 0x80df0000000000L, 0x80bf0000000000L, 0x807f0000000000L };
      private readonly ulong[] _DiskMaskInv = new ulong[ ] { 0x80010000000000L, 0x80020000000000L, 0x80040000000000L, 0x80080000000000L, 0x80100000000000L, 0x80200000000000L, 0x80400000000000L, 0x80800000000000L };
      private bool _diskOn;
      private bool _diskRotate;
      private bool _diskRotateClockwise = true;
      private int _diskSegment;
      private readonly ulong _diskSolidOffMask = 0;
      private readonly ulong _diskSolidOnMask = 0x80ff0000000000L;
      private bool _diskSRWFlash = true;
      private int _flashState = 1;
      private DateTime _LastAnimate;

      public void Animate()
      {
        if ((DateTime.Now.Ticks - this._LastAnimate.Ticks) >= 0x7a120L)
        {
          if ((this._diskRotate & !this._diskFlash) || (this._diskRotate & (this._diskFlash & !this._diskSRWFlash)))
          {
            if (this._diskRotateClockwise)
            {
              this._diskSegment++;
              if (this._diskSegment > 7)
              {
                this._diskSegment = 0;
              }
            }
            else
            {
              this._diskSegment--;
              if (this._diskSegment < 0)
              {
                this._diskSegment = 7;
              }
            }
          }
          if (this._diskFlash)
          {
            if (this._flashState == 1)
            {
              this._flashState = 0;
            }
            else
            {
              this._flashState = 1;
            }
          }
          this._LastAnimate = DateTime.Now;
        }
      }

      public void FlashOff()
      {
        this._diskFlash = false;
        this._flashState = 1;
      }

      public void FlashOn()
      {
        this._diskFlash = true;
      }

      public void InvertOff()
      {
        this._diskInverted = false;
      }

      public void InvertOn()
      {
        this._diskInverted = true;
      }

      public void Off()
      {
        this._diskOn = false;
      }

      public void On()
      {
        if (Settings.Instance.ExtensiveLogging)
        {
          MediaPortal.GUI.Library.Log.Info("DISK ON CALLED", new object[0]);
        }
        this._diskOn = true;
      }

      public void Reset()
      {
        this._diskFlash = false;
        this._diskRotate = false;
        this._diskSegment = 0;
        this._diskRotateClockwise = true;
        this._diskOn = false;
        this._flashState = 1;
        this._diskInverted = false;
        this._diskSRWFlash = true;
      }

      public void RotateCCW()
      {
        this._diskRotateClockwise = false;
        this._diskRotate = true;
      }

      public void RotateCW()
      {
        this._diskRotateClockwise = true;
        this._diskRotate = true;
      }

      public void RotateOff()
      {
        this._diskRotateClockwise = false;
        this._diskRotate = false;
      }

      public void SRWFlashOff()
      {
        this._diskSRWFlash = false;
      }

      public void SRWFlashOn()
      {
        this._diskSRWFlash = true;
      }

      public bool IsFlashing
      {
        get
        {
          return this._diskFlash;
        }
      }

      public bool IsInverted
      {
        get
        {
          return this._diskInverted;
        }
      }

      public bool IsOn
      {
        get
        {
          return this._diskOn;
        }
      }

      public bool IsRotating
      {
        get
        {
          return this._diskFlash;
        }
      }

      public ulong Mask
      {
        get
        {
          MediaPortal.GUI.Library.Log.Info("ON: {0}, flashing: {1}, FLASHSTATE : {2}, Rotate: {3}, Invert: {4}", new object[] { this._diskOn, this._diskFlash, this._flashState.ToString(), this._diskRotate, this._diskInverted });
          if (!this._diskOn)
          {
            return this._diskSolidOffMask;
          }
          if (!this._diskRotate)
          {
            if (!this._diskFlash)
            {
              return this._diskSolidOnMask;
            }
            if (this._flashState == 1)
            {
              return this._diskSolidOnMask;
            }
            return this._diskSolidOffMask;
          }
          if (!this._diskFlash)
          {
            if (!this._diskInverted)
            {
              return this._DiskMask[this._diskSegment];
            }
            return this._DiskMaskInv[this._diskSegment];
          }
          if (this._flashState <= 0)
          {
            return this._diskSolidOffMask;
          }
          if (!this._diskInverted)
          {
            return this._DiskMask[this._diskSegment];
          }
          return this._DiskMaskInv[this._diskSegment];
        }
      }
    }

    private class DisplayType
    {
      public static string TypeName(int DisplayType)
      {
        switch (DisplayType)
        {
          case -1:
            return "AutoDetect";

          case 0:
            return "VFD";

          case 1:
            return "LCD";

          case 3:
            return "3Rsystems";

          case 4:
            return "LCD2";
        }
        return "Unsupported";
      }

      public static int AutoDetect
      {
        get
        {
          return -1;
        }
      }

      public static int LCD
      {
        get
        {
          return 1;
        }
      }

      public static int LCD2
      {
        get
        {
          return 4;
        }
      }

      public static int ThreeRsystems
      {
        get
        {
          return 3;
        }
      }

      public static int Unsupported
      {
        get
        {
          return 2;
        }
      }

      public static int VFD
      {
        get
        {
          return 0;
        }
      }
    }

    private class LargeIcon
    {
      private static byte[,] CustomIcons;
      private DisplayOptions CustomOptions = XMLUTILS.LoadDisplayOptionsSettings();
      private readonly DataColumn IData0 = new DataColumn("IData0");
      private readonly DataColumn IData1 = new DataColumn("IData1");
      private readonly DataColumn IData10 = new DataColumn("IData10");
      private readonly DataColumn IData11 = new DataColumn("IData11");
      private readonly DataColumn IData12 = new DataColumn("IData12");
      private readonly DataColumn IData13 = new DataColumn("IData13");
      private readonly DataColumn IData14 = new DataColumn("IData14");
      private readonly DataColumn IData15 = new DataColumn("IData15");
      private readonly DataColumn IData16 = new DataColumn("IData16");
      private readonly DataColumn IData17 = new DataColumn("IData17");
      private readonly DataColumn IData18 = new DataColumn("IData18");
      private readonly DataColumn IData19 = new DataColumn("IData19");
      private readonly DataColumn IData2 = new DataColumn("IData2");
      private readonly DataColumn IData20 = new DataColumn("IData20");
      private readonly DataColumn IData21 = new DataColumn("IData21");
      private readonly DataColumn IData22 = new DataColumn("IData22");
      private readonly DataColumn IData23 = new DataColumn("IData23");
      private readonly DataColumn IData24 = new DataColumn("IData24");
      private readonly DataColumn IData25 = new DataColumn("IData25");
      private readonly DataColumn IData26 = new DataColumn("IData26");
      private readonly DataColumn IData27 = new DataColumn("IData27");
      private readonly DataColumn IData28 = new DataColumn("IData28");
      private readonly DataColumn IData29 = new DataColumn("IData29");
      private readonly DataColumn IData3 = new DataColumn("IData3");
      private readonly DataColumn IData30 = new DataColumn("IData30");
      private readonly DataColumn IData31 = new DataColumn("IData31");
      private readonly DataColumn IData4 = new DataColumn("IData4");
      private readonly DataColumn IData5 = new DataColumn("IData5");
      private readonly DataColumn IData6 = new DataColumn("IData6");
      private readonly DataColumn IData7 = new DataColumn("IData7");
      private readonly DataColumn IData8 = new DataColumn("IData8");
      private readonly DataColumn IData9 = new DataColumn("IData9");
      private DataTable LIconData = new DataTable("LargeIcons");
      private readonly DataColumn LIID = new DataColumn("IconID");

      public void CloseIcons()
      {
        if (this.LIconData != null)
        {
          this.LIconData.Dispose();
        }
      }

      public void InitializeLargeIcons()
      {
        if (this.CustomOptions.UseLargeIcons || this.CustomOptions.UseCustomIcons)
        {
          MediaPortal.GUI.Library.Log.Debug("iMONLCDg.InitializeLargeIcons(): Using Large Icons.", new object[0]);
          if (!this.CustomOptions.UseCustomIcons)
          {
            MediaPortal.GUI.Library.Log.Debug("iMONLCDg.InitializeLargeIcons(): Using Internal Large Icon Data.", new object[0]);
          }
          else
          {
            MediaPortal.GUI.Library.Log.Debug("iMONLCDg.InitializeLargeIcons(): Using Custom Large Icon Data.", new object[0]);
            if (this.LIconData.Columns.Count == 0)
            {
              this.LIconData.Rows.Clear();
              this.LIconData.Columns.Clear();
              CustomIcons = new byte[10, 0x20];
              this.LIID.DataType = typeof(byte);
              this.LIconData.Columns.Add(this.LIID);
              this.IData0.DataType = typeof(byte);
              this.LIconData.Columns.Add(this.IData0);
              this.IData1.DataType = typeof(byte);
              this.LIconData.Columns.Add(this.IData1);
              this.IData2.DataType = typeof(byte);
              this.LIconData.Columns.Add(this.IData2);
              this.IData3.DataType = typeof(byte);
              this.LIconData.Columns.Add(this.IData3);
              this.IData4.DataType = typeof(byte);
              this.LIconData.Columns.Add(this.IData4);
              this.IData5.DataType = typeof(byte);
              this.LIconData.Columns.Add(this.IData5);
              this.IData6.DataType = typeof(byte);
              this.LIconData.Columns.Add(this.IData6);
              this.IData7.DataType = typeof(byte);
              this.LIconData.Columns.Add(this.IData7);
              this.IData8.DataType = typeof(byte);
              this.LIconData.Columns.Add(this.IData8);
              this.IData9.DataType = typeof(byte);
              this.LIconData.Columns.Add(this.IData9);
              this.IData10.DataType = typeof(byte);
              this.LIconData.Columns.Add(this.IData10);
              this.IData11.DataType = typeof(byte);
              this.LIconData.Columns.Add(this.IData11);
              this.IData12.DataType = typeof(byte);
              this.LIconData.Columns.Add(this.IData12);
              this.IData13.DataType = typeof(byte);
              this.LIconData.Columns.Add(this.IData13);
              this.IData14.DataType = typeof(byte);
              this.LIconData.Columns.Add(this.IData14);
              this.IData15.DataType = typeof(byte);
              this.LIconData.Columns.Add(this.IData15);
              this.IData16.DataType = typeof(byte);
              this.LIconData.Columns.Add(this.IData16);
              this.IData17.DataType = typeof(byte);
              this.LIconData.Columns.Add(this.IData17);
              this.IData18.DataType = typeof(byte);
              this.LIconData.Columns.Add(this.IData18);
              this.IData19.DataType = typeof(byte);
              this.LIconData.Columns.Add(this.IData19);
              this.IData20.DataType = typeof(byte);
              this.LIconData.Columns.Add(this.IData20);
              this.IData21.DataType = typeof(byte);
              this.LIconData.Columns.Add(this.IData21);
              this.IData22.DataType = typeof(byte);
              this.LIconData.Columns.Add(this.IData22);
              this.IData23.DataType = typeof(byte);
              this.LIconData.Columns.Add(this.IData23);
              this.IData24.DataType = typeof(byte);
              this.LIconData.Columns.Add(this.IData24);
              this.IData25.DataType = typeof(byte);
              this.LIconData.Columns.Add(this.IData25);
              this.IData26.DataType = typeof(byte);
              this.LIconData.Columns.Add(this.IData26);
              this.IData27.DataType = typeof(byte);
              this.LIconData.Columns.Add(this.IData27);
              this.IData28.DataType = typeof(byte);
              this.LIconData.Columns.Add(this.IData28);
              this.IData29.DataType = typeof(byte);
              this.LIconData.Columns.Add(this.IData29);
              this.IData30.DataType = typeof(byte);
              this.LIconData.Columns.Add(this.IData30);
              this.IData31.DataType = typeof(byte);
              this.LIconData.Columns.Add(this.IData31);
              this.LIconData.Clear();
            }
            if (this.LoadLargeIconData())
            {
              MediaPortal.GUI.Library.Log.Debug("iMONLCDg.InitializeLargeIcons(): Custom Large Icon data loaded", new object[0]);
            }
            else
            {
              this.SaveDefaultLargeIconData();
              MediaPortal.GUI.Library.Log.Debug("iMONLCDg.InitializeLargeIcons(): Custom Large Icon file not found. Template file saved. loaded default data.", new object[0]);
            }
          }
        }
      }

      private bool LoadLargeIconData()
      {
        MediaPortal.GUI.Library.Log.Debug("LoadLargeIconData(): called", new object[0]);
        if (File.Exists(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg_icons.xml")))
        {
          this.LIconData.Rows.Clear();
          XmlSerializer serializer = new XmlSerializer(typeof(DataTable));
          XmlTextReader xmlReader = new XmlTextReader(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg_icons.xml"));
          MediaPortal.GUI.Library.Log.Debug("LoadLargeIconData(): DeSerializing data", new object[0]);
          this.LIconData = (DataTable)serializer.Deserialize(xmlReader);
          MediaPortal.GUI.Library.Log.Debug("LoadLargeIconData(): Read data from file", new object[0]);
          xmlReader.Close();
          MediaPortal.GUI.Library.Log.Debug("LoadLargeIconData(): Converting icon data", new object[0]);
          for (int j = 0; j < 10; j++)
          {
            DataRow row = this.LIconData.Rows[j];
            for (int k = 1; k < 0x21; k++)
            {
              CustomIcons[j, k - 1] = (byte)row[k];
            }
          }
          MediaPortal.GUI.Library.Log.Debug("LoadLargeIconData(): completed", new object[0]);
          return true;
        }
        MediaPortal.GUI.Library.Log.Debug("LoadLargeIconData(): Loading Custom Large Icons from default Large Icons", new object[0]);
        for (int i = 0; i < 10; i++)
        {
          for (int m = 0; m < 0x20; m++)
          {
            CustomIcons[i, m] = iMONLCDg._InternalLargeIcons[i, m];
          }
        }
        MediaPortal.GUI.Library.Log.Debug("LoadLargeIconData(): completed", new object[0]);
        return false;
      }

      public byte PixelData(int IconID, int ByteIndex)
      {
        return CustomIcons[IconID, ByteIndex];
      }

      private void SaveDefaultLargeIconData()
      {
        MediaPortal.GUI.Library.Log.Debug("SaveDefaultLargeIconData(): called", new object[0]);
        MediaPortal.GUI.Library.Log.Debug("SaveDefaultLargeIconData(): Converting icon data", new object[0]);
        this.LIconData.Rows.Clear();
        for (int i = 0; i < 10; i++)
        {
          DataRow row = this.LIconData.NewRow();
          row[0] = i;
          for (int j = 1; j < 0x21; j++)
          {
            row[j] = iMONLCDg._InternalLargeIcons[i, j - 1];
          }
          this.LIconData.Rows.Add(row);
        }
        XmlSerializer serializer = new XmlSerializer(typeof(DataTable));
        TextWriter textWriter = new StreamWriter(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg_icons.xml"));
        MediaPortal.GUI.Library.Log.Debug("SaveDefaultLargeIconData(): Serializing data", new object[0]);
        serializer.Serialize(textWriter, this.LIconData);
        MediaPortal.GUI.Library.Log.Debug("SaveDefaultLargeIconData(): Writing data to file", new object[0]);
        textWriter.Close();
        MediaPortal.GUI.Library.Log.Debug("SaveDefaultLargeIconData(): completed", new object[0]);
      }
    }

    private enum LargeIconType
    {
      IDLE,
      TV,
      MOVIE,
      MUSIC,
      VIDEO,
      RECORDING,
      PAUSED
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MouseKeyEvent
    {
      public int Last_X_Delta;
      public int Last_Y_Delta;
      public int Last_X_Size;
      public int Last_Y_Size;
      public int Last_L_Button;
      public int Last_R_Button;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RemoteControl
    {
      public string RemoteType;
      public bool EnableRemote;
      public bool DisableRepeat;
      public int RepeatDelay;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RemoteState
    {
      public byte KeyPressed;
      public byte KeyModifier;
      public byte LastKeyPressed;
      public byte LastKeyModifier;
      public byte LastButtonPressed;
      public byte LastButtonToggle;
      public DateTime LastButtonPressTimestamp;
      public iMONLCDg.MouseKeyEvent LastMouseKeyEvent;
    }
  }
}

