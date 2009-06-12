using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Ripper;
using Microsoft.Win32;
using Win32.Utils.Cd;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Drivers
{
  public class iMONLCDg : BaseDisplay, IDisplay
  {
    public static readonly string[,] _BrandTable = {
                                                     { "SOUNDGRAPH", "iMON" },
                                                     { "Antec", "VFD" }
                                                   };

    public static readonly int _BrandTableLength = 1;

    public static readonly byte[,] _Font8x5 = new byte[,]
                                                {
                                                  {0, 0, 0, 0, 0, 0}, {0, 100, 0x18, 4, 100, 0x18},
                                                  {0, 60, 0x40, 0x40, 0x20, 0x7c}, {0, 12, 0x30, 0x40, 0x30, 12},
                                                  {0, 60, 0x40, 0x30, 0x40, 60}, {0, 0, 0x3e, 0x1c, 8, 0},
                                                  {0, 4, 30, 0x1f, 30, 4}, {0, 0x10, 60, 0x7c, 60, 0x10},
                                                  {0, 0x20, 0x40, 0x3e, 1, 2}, {0, 0x22, 20, 8, 20, 0x22},
                                                  {0, 0, 0x38, 40, 0x38, 0}, {0, 0, 0x10, 0x38, 0x10, 0},
                                                  {0, 0, 0, 0x10, 0, 0}, {0, 8, 120, 8, 0, 0}, {0, 0, 0x15, 0x15, 10, 0}
                                                  , {0, 0x7f, 0x7f, 9, 9, 1},
                                                  {0, 0x10, 0x20, 0x7f, 1, 1}, {0, 4, 4, 0, 1, 0x1f},
                                                  {0, 0, 0x19, 0x15, 0x12, 0}, {0, 0x40, 0x60, 80, 0x48, 0x44},
                                                  {0, 6, 9, 9, 6, 0}, {0, 15, 2, 1, 1, 0}, {0, 0, 1, 0x1f, 1, 0},
                                                  {0, 0x44, 0x44, 0x4a, 0x4a, 0x51}, {0, 20, 0x74, 0x1c, 0x17, 20},
                                                  {0, 0x51, 0x4a, 0x4a, 0x44, 0x44}, {0, 0, 0, 4, 4, 4},
                                                  {0, 0, 0x7c, 0x54, 0x54, 0x44}, {0, 8, 8, 0x2a, 0x1c, 8},
                                                  {0, 0x7c, 0, 0x7c, 0x44, 0x7c}, {0, 4, 2, 0x7f, 2, 4},
                                                  {0, 0x10, 0x20, 0x7f, 0x20, 0x10},
                                                  {0, 0, 0, 0, 0, 0}, {0, 0, 0, 0x6f, 0, 0}, {0, 0, 7, 0, 7, 0},
                                                  {0, 20, 0x7f, 20, 0x7f, 20}, {0, 0, 7, 4, 30, 0},
                                                  {0, 0x23, 0x13, 8, 100, 0x62}, {0, 0x36, 0x49, 0x56, 0x20, 80},
                                                  {0, 0, 0, 7, 0, 0}, {0, 0, 0x1c, 0x22, 0x41, 0},
                                                  {0, 0, 0x41, 0x22, 0x1c, 0}, {0, 20, 8, 0x3e, 8, 20},
                                                  {0, 8, 8, 0x3e, 8, 8}, {0, 0, 80, 0x30, 0, 0}, {0, 8, 8, 8, 8, 8},
                                                  {0, 0, 0x60, 0x60, 0, 0}, {0, 0x20, 0x10, 8, 4, 2},
                                                  {0, 0x3e, 0x51, 0x49, 0x45, 0x3e}, {0, 0, 0x42, 0x7f, 0x40, 0},
                                                  {0, 0x42, 0x61, 0x51, 0x49, 70}, {0, 0x21, 0x41, 0x45, 0x4b, 0x31},
                                                  {0, 0x18, 20, 0x12, 0x7f, 0x10}, {0, 0x27, 0x45, 0x45, 0x45, 0x39},
                                                  {0, 60, 0x4a, 0x49, 0x49, 0x30}, {0, 1, 0x71, 9, 5, 3},
                                                  {0, 0x36, 0x49, 0x49, 0x49, 0x36}, {0, 6, 0x49, 0x49, 0x29, 30},
                                                  {0, 0, 0x36, 0x36, 0, 0}, {0, 0, 0x56, 0x36, 0, 0},
                                                  {0, 8, 20, 0x22, 0x41, 0}, {0, 20, 20, 20, 20, 20},
                                                  {0, 0, 0x41, 0x22, 20, 8}, {0, 2, 1, 0x51, 9, 6},
                                                  {0, 0x3e, 0x41, 0x5d, 0x49, 0x4e}, {0, 0x7e, 9, 9, 9, 0x7e},
                                                  {0, 0x7f, 0x49, 0x49, 0x49, 0x36}, {0, 0x3e, 0x41, 0x41, 0x41, 0x22},
                                                  {0, 0x7f, 0x41, 0x41, 0x41, 0x3e}, {0, 0x7f, 0x49, 0x49, 0x49, 0x41},
                                                  {0, 0x7f, 9, 9, 9, 1}, {0, 0x3e, 0x41, 0x49, 0x49, 0x7a},
                                                  {0, 0x7f, 8, 8, 8, 0x7f}, {0, 0, 0x41, 0x7f, 0x41, 0},
                                                  {0, 0x20, 0x40, 0x41, 0x3f, 1}, {0, 0x7f, 8, 20, 0x22, 0x41},
                                                  {0, 0x7f, 0x40, 0x40, 0x40, 0x40}, {0, 0x7f, 2, 12, 2, 0x7f},
                                                  {0, 0x7f, 4, 8, 0x10, 0x7f}, {0, 0x3e, 0x41, 0x41, 0x41, 0x3e},
                                                  {0, 0x7f, 9, 9, 9, 6}, {0, 0x3e, 0x41, 0x51, 0x21, 0x5e},
                                                  {0, 0x7f, 9, 0x19, 0x29, 70}, {0, 70, 0x49, 0x49, 0x49, 0x31},
                                                  {0, 1, 1, 0x7f, 1, 1}, {0, 0x3f, 0x40, 0x40, 0x40, 0x3f},
                                                  {0, 15, 0x30, 0x40, 0x30, 15}, {0, 0x3f, 0x40, 0x30, 0x40, 0x3f},
                                                  {0, 0x63, 20, 8, 20, 0x63}, {0, 7, 8, 0x70, 8, 7},
                                                  {0, 0x61, 0x51, 0x49, 0x45, 0x43}, {0, 0, 0x7f, 0x41, 0, 0},
                                                  {0, 2, 4, 8, 0x10, 0x20}, {0, 0, 0x41, 0x7f, 0, 0}, {0, 4, 2, 1, 2, 4}
                                                  , {0, 0x40, 0x40, 0x40, 0x40, 0x40},
                                                  {0, 0, 0, 3, 4, 0}, {0, 0x20, 0x54, 0x54, 0x54, 120},
                                                  {0, 0x7f, 0x48, 0x44, 0x44, 0x38}, {0, 0x38, 0x44, 0x44, 0x44, 0x20},
                                                  {0, 0x38, 0x44, 0x44, 0x48, 0x7f}, {0, 0x38, 0x54, 0x54, 0x54, 0x18},
                                                  {0, 8, 0x7e, 9, 1, 2}, {0, 12, 0x52, 0x52, 0x52, 0x3e},
                                                  {0, 0x7f, 8, 4, 4, 120}, {0, 0, 0x44, 0x7d, 0x40, 0},
                                                  {0, 0x20, 0x40, 0x44, 0x3d, 0}, {0, 0, 0x7f, 0x10, 40, 0x44},
                                                  {0, 0, 0x41, 0x7f, 0x40, 0}, {0, 0x7c, 4, 0x18, 4, 120},
                                                  {0, 0x7c, 8, 4, 4, 120}, {0, 0x38, 0x44, 0x44, 0x44, 0x38},
                                                  {0, 0x7c, 20, 20, 20, 8}, {0, 8, 20, 20, 0x18, 0x7c},
                                                  {0, 0x7c, 8, 4, 4, 8}, {0, 0x48, 0x54, 0x54, 0x54, 0x20},
                                                  {0, 4, 0x3f, 0x44, 0x40, 0x20}, {0, 60, 0x40, 0x40, 0x20, 0x7c},
                                                  {0, 0x1c, 0x20, 0x40, 0x20, 0x1c}, {0, 60, 0x40, 0x30, 0x40, 60},
                                                  {0, 0x44, 40, 0x10, 40, 0x44}, {0, 12, 80, 80, 80, 60},
                                                  {0, 0x44, 100, 0x54, 0x4c, 0x44}, {0, 0, 8, 0x36, 0x41, 0x41},
                                                  {0, 0, 0, 0x7f, 0, 0}, {0, 0x41, 0x41, 0x36, 8, 0}, {0, 4, 2, 4, 8, 4}
                                                  , {0, 0x7f, 0x6b, 0x6b, 0x6b, 0x7f},
                                                  {0, 0, 0x7c, 0x44, 0x7c, 0}, {0, 0, 8, 0x7c, 0, 0},
                                                  {0, 0, 100, 0x54, 0x48, 0}, {0, 0, 0x44, 0x54, 40, 0},
                                                  {0, 0, 0x1c, 0x10, 120, 0}, {0, 0, 0x5c, 0x54, 0x24, 0},
                                                  {0, 0, 120, 0x54, 0x74, 0}, {0, 0, 100, 20, 12, 0},
                                                  {0, 0, 0x7c, 0x54, 0x7c, 0}, {0, 0, 0x5c, 0x54, 60, 0},
                                                  {0, 120, 0x24, 0x26, 0x25, 120}, {0, 120, 0x25, 0x26, 0x24, 120},
                                                  {0, 0x70, 0x2a, 0x29, 0x2a, 0x70}, {0, 120, 0x25, 0x24, 0x25, 120},
                                                  {0, 0x20, 0x54, 0x56, 0x55, 120}, {0, 0x20, 0x55, 0x56, 0x54, 120},
                                                  {0, 0x20, 0x56, 0x55, 0x56, 120}, {0, 0x20, 0x55, 0x54, 0x55, 120},
                                                  {0, 0x7c, 0x54, 0x56, 0x55, 0x44}, {0, 0x7c, 0x55, 0x56, 0x54, 0x44},
                                                  {0, 0x7c, 0x56, 0x55, 0x56, 0x44}, {0, 0x7c, 0x55, 0x54, 0x55, 0x44},
                                                  {0, 0x38, 0x54, 0x56, 0x55, 0x18}, {0, 0x38, 0x55, 0x56, 0x54, 0x18},
                                                  {0, 0x38, 0x56, 0x55, 0x56, 0x18}, {0, 0x38, 0x55, 0x54, 0x55, 0x18},
                                                  {0, 0, 0x44, 0x7e, 0x45, 0}, {0, 0, 0x45, 0x7e, 0x44, 0},
                                                  {0, 0, 70, 0x7d, 70, 0}, {0, 0, 0x45, 0x7c, 0x45, 0},
                                                  {0, 0, 0x48, 0x7a, 0x41, 0}, {0, 0, 0x49, 0x7a, 0x40, 0},
                                                  {0, 0, 0x4a, 0x79, 0x42, 0}, {0, 0, 0x49, 120, 0x41, 0},
                                                  {0, 0x38, 0x44, 70, 0x45, 0x38}, {0, 0x38, 0x45, 70, 0x44, 0x38},
                                                  {0, 0x38, 70, 0x45, 70, 0x38}, {0, 0x38, 0x45, 0x44, 0x45, 0x38},
                                                  {0, 0x30, 0x48, 0x4a, 0x49, 0x30}, {0, 0x30, 0x49, 0x4a, 0x48, 0x30},
                                                  {0, 0x30, 0x4a, 0x49, 0x4a, 0x30}, {0, 0x30, 0x49, 0x48, 0x49, 0x30},
                                                  {0, 60, 0x40, 0x42, 0x41, 60}, {0, 60, 0x41, 0x42, 0x40, 60},
                                                  {0, 60, 0x42, 0x41, 0x42, 60}, {0, 60, 0x41, 0x40, 0x41, 60},
                                                  {0, 60, 0x40, 0x42, 0x21, 0x7c}, {0, 60, 0x41, 0x42, 0x20, 0x7c},
                                                  {0, 0x38, 0x42, 0x41, 0x22, 120}, {0, 60, 0x41, 0x40, 0x21, 0x7c},
                                                  {0, 0x4e, 0x51, 0x71, 0x11, 10}, {0, 0x58, 100, 100, 0x24, 0x10},
                                                  {0, 0x7c, 10, 0x11, 0x22, 0x7d}, {0, 120, 0x12, 9, 10, 0x71},
                                                  {0, 0, 0, 4, 2, 1}, {0, 1, 2, 4, 0, 0}, {0, 0, 2, 0, 2, 0},
                                                  {0, 0x30, 0x48, 0x45, 0x40, 0x20}, {0, 0, 0, 0x7b, 0, 0},
                                                  {0, 0x38, 0x44, 0x44, 0x38, 0x44}, {0, 0x40, 0x3e, 0x49, 0x49, 0x36},
                                                  {0, 8, 4, 8, 0x70, 12}, {0, 0x60, 80, 0x48, 80, 0x60},
                                                  {0, 0x30, 0x48, 0x45, 0x40, 0},
                                                  {0, 0x7c, 0x13, 0x12, 0x12, 0x7c}, {0, 0x7c, 0x12, 0x12, 0x13, 0x7c},
                                                  {0, 240, 0x2a, 0x29, 0x2a, 240}, {0, 240, 0x2a, 0x29, 0x2a, 0xf1},
                                                  {0, 0x7c, 0x13, 0x12, 0x13, 0x7c}, {0, 0x40, 60, 0x12, 0x12, 12},
                                                  {0, 0x7c, 1, 0x7f, 0x49, 0x41}, {0, 14, 0x11, 0xb1, 0xd1, 10},
                                                  {0, 0x7c, 0x55, 0x56, 0x54, 0}, {0, 0x7c, 0x54, 0x56, 0x55, 0},
                                                  {0, 0x7f, 0x49, 0x49, 0x49, 0}, {0, 0x7c, 0x55, 0x54, 0x55, 0},
                                                  {0, 0, 0x41, 0x7f, 0x48, 0}, {0, 0, 0x48, 0x7a, 0x49, 0},
                                                  {0, 0, 0x4a, 0x79, 0x4a, 0}, {0, 0, 0x45, 0x7c, 0x45, 0},
                                                  {0, 8, 0x7f, 0x49, 0x41, 0x3e}, {0, 120, 10, 0x11, 0x22, 0x79},
                                                  {0, 0x38, 0x45, 70, 0x44, 0x38}, {0, 0x38, 0x44, 70, 0x45, 0x38},
                                                  {0, 0x30, 0x4a, 0x49, 0x4a, 0x30}, {0, 0x30, 0x4a, 0x49, 0x41, 0x31},
                                                  {0, 0x38, 0x45, 0x44, 0x45, 0x38}, {0, 0, 20, 8, 20, 0},
                                                  {0, 0x3e, 0x51, 0x49, 0x44, 0x3e}, {0, 60, 0x41, 0x42, 0x40, 60},
                                                  {0, 60, 0x40, 0x42, 0x41, 60}, {0, 0x3f, 0x40, 0x40, 0x40, 0x3f},
                                                  {0, 60, 0x41, 0x40, 0x41, 60}, {0, 12, 0x10, 0x62, 0x11, 12},
                                                  {0, 0x7f, 0x22, 0x22, 0x22, 0x1c}, {0, 0x7e, 0x21, 0x2d, 0x2d, 0x12},
                                                  {0, 0x40, 0xa9, 170, 0xa8, 240}, {0, 0x40, 0xa8, 170, 0xa9, 240},
                                                  {0, 0x40, 170, 0xa9, 170, 240}, {0, 0x40, 170, 0xa9, 170, 0xf1},
                                                  {0, 0x20, 0x55, 0x54, 0x55, 120}, {0, 80, 0x55, 0x55, 0x54, 120},
                                                  {0, 0x40, 0x5e, 0x45, 0x5e, 0x40}, {0, 14, 0x91, 0xb1, 0x51, 8},
                                                  {0, 0x38, 0x55, 0x56, 0x54, 0x18}, {0, 0x38, 0x54, 0x56, 0x55, 0x18},
                                                  {0, 0x70, 170, 0xa9, 170, 0x30}, {0, 0x38, 0x55, 0x54, 0x55, 0x18},
                                                  {0, 0, 0x44, 0x7d, 0x42, 0}, {0, 0, 0x48, 0x7a, 0x41, 0},
                                                  {0, 0, 0x4a, 0x79, 0x42, 0}, {0, 0, 0x44, 0x7d, 0x40, 0},
                                                  {0, 0x10, 0x3e, 0x7e, 0x3e, 0x10}, {0, 0x55, 0x2a, 0x55, 0x2a, 0x55},
                                                  {0, 0x30, 0x49, 0x4a, 0x48, 0x30}, {0, 0x30, 0x48, 0x4a, 0x49, 0x30},
                                                  {0, 0x30, 0x4a, 0x49, 0x4a, 0x30}, {0, 0x38, 0x45, 0x44, 0x45, 0x38},
                                                  {0, 0x38, 0x45, 0x44, 0x45, 0x38}, {0, 60, 0x41, 0x40, 0x41, 60},
                                                  {0, 0x38, 0x44, 0x44, 0x44, 0x38}, {0, 60, 0x41, 0x42, 0x20, 0x7c},
                                                  {0, 60, 0x40, 0x42, 0x21, 0x7c}, {0, 0x38, 0x42, 0x41, 0x22, 0x7c},
                                                  {0, 60, 0x41, 0x40, 0x21, 0x7c}, {0, 12, 80, 0x52, 80, 60},
                                                  {0, 0x7c, 40, 40, 0x10, 0}, {0, 12, 0x51, 80, 0x51, 60}
                                                };

    private static readonly int[,] _iMON_FW_Display = new[,]
                                                        {
                                                          {8, 0, 2, 0, 0}, {0x36, 0, 9, 0, 3}, {0x39, 0, 10, 0, 0},
                                                          {0x3a, 0, 9, 0, 3}, {0x3b, 0, 0x11, 0, 3},
                                                          {0x49, 0, 0x12, 0, 0}, {0x4b, 0, 20, 0, 0},
                                                          {0x4c, 0, 0x12, 0, 0}, {0x3d, 0, 0x10, 0, 0},
                                                          {30, 0, 0x17, 0, 0}, {0x30, 50, 4, 0, 0}, {0x33, 60, 6, 0, 0},
                                                          {0x3e, 0x3f, 11, 0, 0}, {0x40, 0, 8, 0, 0},
                                                          {0x41, 0x47, 12, 0, 0}, {0x48, 0x4f, 13, 0, 0},
                                                          {0x70, 0x77, 7, 0, 0}, {120, 0x7f, 14, 0, 0},
                                                          {0x80, 0x83, 15, 0, 0}, {0x85, 0, 0x10, 0, 2},
                                                          {0x84, 0x8f, 0x10, 0, 0}, {0x90, 0x91, 0x13, 0x8888, 1},
                                                          {0x92, 0x97, 0x15, 0x8888, 1}, {0x98, 0x99, 0x18, 0x8888, 1},
                                                          {0x9a, 0x9b, 0x19, 0x8888, 1}, {0x9c, 0x9f, 0x16, 0x8888, 1},
                                                          {160, 0, 0x10, 0, 0}, {0xa1, 0, 0x16, 0x8888, 1},
                                                          {0x3600, 0x37ff, 0x1a, 0x8888, 0},
                                                          {0x3800, 0x39ff, 0x1b, 0x8888, 4}, {0x3412, 0, 0, 0, 2},
                                                          {0x3c40, 0, 0, 0, 2},
                                                          {0x3e00, 0x3eff, 0x1a, 0, 0},
                                                          {0x3f00, 0x3fff, 0x1b, 0x8888, 4}, {0, 0, 0, 0, 2}
                                                        };

    public static readonly byte[,] _InternalLargeIcons = new byte[,]
                                                           {
                                                             {
                                                               0xc0, 0x80, 0x80, 0xc0, 0xff, 0xc0, 0x80, 0x80, 0xc0,
                                                               0xff, 0xc7, 0x83, 0x93, 0x83, 0xc7, 0xff,
                                                               3, 1, 1, 3, 0xff, 3, 1, 1, 3, 0xff, 0xff, 0xff, 0xff,
                                                               0xff, 0xff, 0xff
                                                             }, {
                                                                  0xff, 0xfe, 0xfe, 0xbd, 0xdd, 0xed, 0xf5, 0xf9, 0xf9,
                                                                  0xf5, 0xed, 0xdd, 0xbd, 0xfe, 0xfe, 0xff,
                                                                  0xff, 1, 0xf9, 0xfd, 0xfd, 0xfd, 0xfd, 0xfd, 0xfd,
                                                                  0xfd, 0xfd, 0xfd, 0xfd, 0xf9, 1, 0xff
                                                                }, {
                                                                     0xff, 0x80, 0xaf, 0x8f, 0xaf, 0x8f, 0xaf, 0x8f,
                                                                     0xaf, 0x8f, 0xaf, 0x8f, 0xaf, 0x8f, 160, 0x8f,
                                                                     0xf5, 1, 0xf5, 0xf1, 0xf5, 0xf1, 0xf5, 0xf1, 0xf5,
                                                                     0xf1, 0xf5, 0xf1, 0xf5, 0xf1, 5, 1
                                                                   }, {
                                                                        0xff, 0xff, 0xff, 0xff, 0xff, 0x80, 0xc7, 0xc3,
                                                                        0xe3, 0xe3, 0xe3, 0xe3, 0xff, 0xff, 0xff, 0xff,
                                                                        0xff, 0xff, 0xf3, 0xe1, 0xe1, 3, 0xff, 0xff,
                                                                        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff
                                                                      }, {
                                                                           0xe3, 0xde, 0xbf, 0xbb, 0xbf, 0xde, 0xe1,
                                                                           0xe1, 0xde, 0xbf, 0xbb, 0xbf, 0xde, 0xe1,
                                                                           0xff, 0xff,
                                                                           0xff, 1, 0x7d, 0x7d, 0x7d, 0x7d, 0x7d, 0x7d,
                                                                           0x7d, 0x7d, 0x7d, 13, 0xcf, 0xb7, 0xb7, 0x87
                                                                         }, {
                                                                              0xf8, 0xf8, 0xf2, 0xf2, 0xf2, 230, 230,
                                                                              0xce, 0xce, 0xce, 0x9e, 0x9e, 0x3e, 0x3e,
                                                                              0x3e, 0xff,
                                                                              0, 30, 0x3e, 0x7e, 0x5e, 30, 0x3e, 0x7e,
                                                                              0x5e, 30, 0x3e, 0x7e, 0x5e, 30, 14, 0xff
                                                                            }, {
                                                                                 0xff, 0xff, 0xff, 0xff, 0xc0, 0x80,
                                                                                 0x80, 0xc0, 0xff, 0xc0, 0x80, 0x80,
                                                                                 0xc0, 0xff, 0xff, 0xff,
                                                                                 0xff, 0xff, 0xff, 0xff, 3, 1, 1, 3,
                                                                                 0xff, 3, 1, 1, 3, 0xff, 0xff, 0xff
                                                                               }, {
                                                                                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                                                                    0, 0, 0, 0,
                                                                                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                                                                    0, 0, 0, 0
                                                                                  }, {
                                                                                       0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                                                                       0, 0, 0, 0, 0,
                                                                                       0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                                                                       0, 0, 0, 0, 0
                                                                                     }, {
                                                                                          0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                                                                          0, 0, 0, 0, 0, 0,
                                                                                          0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                                                                          0, 0, 0, 0, 0, 0
                                                                                        }
                                                           };

    private static readonly object DWriteMutex = new object();
    private static readonly int[] Inserted_Media = new int[0x1b];

    private static readonly object ThreadMutex = new object();
    private static int _DisplayType = -1;
    private static bool _stopUpdateIconThread;
    private static bool _VFD_UseV3DLL;
    private static int _VfdReserved = 0x8888;
    private static int _VfdType = 0x18;
    private static DeviceVolumeMonitor DVM;
    private readonly string[] _lines = new string[2];
    private readonly SHA256Managed _sha256 = new SHA256Managed();


    private bool _Backlight;
    private ulong _BacklightLevel = 0L;
    private bool _BlankDisplayOnExit;
    private bool _Contrast = true;
    private ulong _ContrastLevel = 10L;
    private int _CurrentLargeIcon;
    private int _delay;
    private int _delayG;
    private bool _DelayStartup;
    private bool _displayTest;
    private bool _EnsureManagerStartup;
    private string _errorMessage = "";

    private string _ForceDisplay;
    private bool _ForceKeyBoardMode;
    private bool _ForceManagerReload;
    private bool _ForceManagerRestart;
    private int _gcols = 0x60;
    private int _grows = 0x10;
    private Thread _iconThread;
    private iMONDisplay _IMON;

    private bool _IsConfiguring;
    private bool _isDisabled;
    private bool _IsDisplayOff;
    private bool _IsHandlingPowerEvent;
    private byte[] _lastHash;
    private bool _MonitorPower;
    private bool _mpIsIdle;
    private bool _RestartFrontviewOnExit;
    private bool _LeaveFrontviewActive;
    private const int _tcols = 0x10;
    private int _trows = 2;
    private bool _USE_VFD_ICONS = false;

    private bool _UsingAntecManager;
    private bool _UsingSoundgraphManager;
    private AdvancedSettings AdvSettings = AdvancedSettings.Load();
    private byte[] bitmapData;
    private CustomFont CFont;
    private LargeIcon CustomLargeIcon;
    private DisplayOptions DisplayOptions;
    private DisplayControl DisplaySettings;
    private bool DoDebug;
    private bool DVMactive;
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
    private int LastProgLevel;
    private DateTime LastSettingsCheck = DateTime.Now;
    private int LastVolLevel;

    private SystemStatus MPStatus;
    private int progLevel;
    private int SendData_Error_Count;
    private DateTime SettingsLastModTime;
    private int volLevel;

    #region IDisplay Members

    public void CleanUp()
    {
      if (!_isDisabled)
      {
        Log.Info("(IDisplay) iMONLCDg.CleanUp(): called");
        AdvancedSettings.OnSettingsChanged -=
          AdvancedSettings_OnSettingsChanged;
        CloseLcd();
        Log.Info("(IDisplay) iMONLCDg.CleanUp(): completed");
      }
    }

    public void Configure()
    {
      Form form = new iMONLCDg_AdvancedSetupForm();
      form.ShowDialog();
      form.Dispose();
    }

    public void Dispose()
    {
      Log.Debug("iMONLCDg.Dispose(): called");
      //
      // If IRSS (Input Remote Server Suite by and-81) is installed
      // we need to restart it in order to re-register dll handler
      //
      bool irss_found = false;
      const string irss_srv = "InputService";
      const string irss_app = "IRServer";
      //
      // Service part
      //
      foreach (ServiceController ctrl in ServiceController.GetServices())
      {
        if (ctrl.ServiceName.ToLower() == irss_srv.ToLower() && ctrl.Status == ServiceControllerStatus.Running)
        {
          Log.Debug("iMONLCDg.Dispose(): Restarting \"" + irss_srv + "\" from IRSS");
          try
          {
            ctrl.Stop();
            ctrl.WaitForStatus(ServiceControllerStatus.Stopped);
            ctrl.Start();
            ctrl.WaitForStatus(ServiceControllerStatus.Running);
            irss_found = true;
            break;
          }
          catch (Exception ex)
          {
            Log.Error("iMONLCDg.Dispose(): Unable to restart \"" + irss_srv + "\" from IRSS: " + ex.Message);
          }
        }
      }
      //
      // Application part
      //
      if (!irss_found)
      {
        Process[] procs = Process.GetProcessesByName(irss_app);
        foreach (Process proc in procs)
        {
          Log.Debug("iMONLCDg.Dispose(): Restarting \"" + irss_app + "\" from IRSS");
          proc.Kill();
          proc.WaitForExit(2000);

          RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\IR Server Suite", false);

          if (key == null)
          {
            Log.Error("iMONLCDg.Dispose(): IRSS registry keys missing, aborting...");
            break;
          }
          string workdir = (string)key.GetValue("Install_Dir", string.Empty) + "\\Input Service";
          key.Close();

          Win32Functions.RedrawNotificationArea();

          try
          {
            Process proc2 = new Process
            {
              StartInfo =
              {
                FileName = workdir + "\\" + irss_app + ".exe",
                WorkingDirectory = workdir
              }
            };
            proc2.Start();
          }
          catch (Exception ex)
          {
            Log.Error("iMONLCDg.Dispose(): Unable to restart \"" + irss_app + "\" from IRSS: " + ex.Message);
          }
          break;
        }
      }
      Log.Debug("iMONLCDg.Dispose(): completed");
    }

    public void DrawImage(Bitmap bitmap)
    {
      if (!_isDisabled)
      {
        if (EQSettings._EqDataAvailable || _IsDisplayOff)
        {
          if (DoDebug)
          {
            Log.Info("iMONLCDg.DrawImage(): Suppressing display update!");
          }
        }
        else
        {
          if (DoDebug)
          {
            Log.Info("(IDisplay) iMONLCDg.DrawImage(): called");
          }
          if (bitmap == null)
          {
            Log.Debug("(IDisplay) iMONLCDg.DrawImage():  bitmap null");
          }
          else
          {
            BitmapData bitmapdata = bitmap.LockBits(new Rectangle(new Point(0, 0), bitmap.Size), ImageLockMode.ReadOnly,
                                                    bitmap.PixelFormat);
            try
            {
              if (bitmapData == null)
              {
                bitmapData = new byte[bitmapdata.Stride * _grows];
              }
              Marshal.Copy(bitmapdata.Scan0, bitmapData, 0, bitmapData.Length);
            }
            finally
            {
              bitmap.UnlockBits(bitmapdata);
            }
            byte[] buffer = _sha256.ComputeHash(bitmapData);
            if (ByteArray.AreEqual(buffer, _lastHash))
            {
              if (DoDebug)
              {
                Log.Info("(IDisplay) iMONLCDg.DrawImage():  bitmap not changed");
              }
            }
            else
            {
              UpdateAdvancedSettings();
              var pixelArray = new byte[0xc0];
              for (int i = 0; i < (_gcols - 1); i++)
              {
                pixelArray[i] = 0;
                pixelArray[i + 0x60] = 0;
                for (int j = 0; j < 8; j++)
                {
                  int index = (j * bitmapdata.Stride) + (i * 4);
                  if (
                    Color.FromArgb(bitmapData[index + 2], bitmapData[index + 1], bitmapData[index]).
                      GetBrightness() < 0.5f)
                  {
                    pixelArray[i] = (byte)(pixelArray[i] | ((byte)((1) << (7 - j))));
                  }
                }
                for (int k = 8; k < 0x10; k++)
                {
                  int num5 = (k * bitmapdata.Stride) + (i * 4);
                  if (
                    Color.FromArgb(bitmapData[num5 + 2], bitmapData[num5 + 1], bitmapData[num5]).
                      GetBrightness() < 0.5f)
                  {
                    pixelArray[i + 0x60] = (byte)(pixelArray[i + 0x60] | ((byte)((1) << (15 - k))));
                  }
                }
              }
              SendPixelArray(pixelArray);
              if (DoDebug)
              {
                Log.Info("(IDisplay) iMONLCDg.DrawImage(): Sending pixel array to iMON Handler");
              }
              _lastHash = buffer;
            }
          }
        }
      }
    }

    public void Initialize()
    {
      Log.Info("(IDisplay) iMONLCDg.Initialize(): called");
      if (_isDisabled)
      {
        Log.Info("(IDisplay) iMONLCDg.Initialize(): completed\n\n iMONLCDg DRIVER DISABLED\n\n");
      }
      else
      {
        OpenLcd();
        Clear();
        Log.Info("(IDisplay) iMONLCDg.Initialize(): completed");
      }
    }

    public void SetCustomCharacters(int[][] customCharacters)
    {
    }

    public void SetLine(int line, string message)
    {
      if (!_isDisabled)
      {
        try
        {
          if (DoDebug)
          {
            Log.Info("(IDisplay) iMONLCDg.SetLine(): called for Line {0} msg: '{1}'",
                     new object[] { line.ToString(), message });
          }
          if (_USE_VFD_ICONS & (_DisplayType == DisplayType.VFD))
          {
            _lines[line] = Add_VFD_Icons(line, message);
          }
          else
          {
            _lines[line] = message;
          }
          if (line == (_trows - 1))
          {
            DisplayLines();
          }
          if (DoDebug)
          {
            Log.Info("(IDisplay) iMONLCDg.SetLine(): completed");
          }
        }
        catch (Exception exception)
        {
          Log.Debug("(IDisplay) iMONLCDg.SetLine(): CAUGHT EXCEPTION {0}", new object[] { exception });
        }
      }
    }

    public void Setup(string port, int lines, int cols, int delay, int linesG, int colsG, int timeG, bool backLight,
                      int backlightLevel, bool contrast, int contrastLevel, bool blankOnExit)
    {
      Log.Info("(IDisplay) iMONLCDg.Setup(): called");
      MiniDisplayHelper.InitEQ(ref EQSettings);
      MiniDisplayHelper.InitDisplayControl(ref DisplaySettings);
      MiniDisplayHelper.InitDisplayOptions(ref DisplayOptions);
      _BlankDisplayOnExit = blankOnExit;
      _Backlight = false;
      _BacklightLevel = (ulong)backlightLevel;
      _Contrast = true;
      _ContrastLevel = ((ulong)contrastLevel) >> 2;
      InitializeDriver();
      if (_DelayStartup)
      {
        Log.Info("iMONLCDg.Setup(): Delaying device initialization by 10 seconds");
        Thread.Sleep(0x2710);
      }
      Check_iMON_Manager_Status();
      if (_IMON == null)
      {
        _IMON = new iMONDisplay();
      }
      int fWVersion = -1;
      int rEGVersion = -1;
      switch (_ForceDisplay)
      {
        case "LCD":
          _DisplayType = DisplayType.LCD;
          _VfdType = 0x18;
          _VfdReserved = 0x8888;
          Log.Info("(IDisplay) iMONLCDg.Setup(): Advanced options forces display type to LCD");
          break;
        case "LCD2":
          _DisplayType = DisplayType.LCD2;
          _VfdType = 0x1b;
          _VfdReserved = 0x8888;
          Log.Info("(IDisplay) iMONLCDg.Setup(): Advanced options forces display type to LCD2");
          break;
        case "VFD":
          _DisplayType = DisplayType.VFD;
          _VfdType = 0x10;
          _VfdType = 0x1a;
          _VfdReserved = 0;
          Log.Info("(IDisplay) iMONLCDg.Setup(): Advanced options forces display type to VFD");
          break;
        case "LCD3R":
          _DisplayType = DisplayType.ThreeRsystems;
          _VfdType = 9;
          _VfdReserved = 0;
          Log.Info("(IDisplay) iMONLCDg.Setup(): Advanced options forces display type to LCD3R");
          break;
        default:
          Log.Info("(IDisplay) iMONLCDg.Setup(): Autodetecting iMON Display device");
          try
          {
            Log.Info("(IDisplay) iMONLCDg.Setup(): attempting hardware information test");
            if (_IMON.RC_Available())
            {
              Log.Info("(IDisplay) iMONLCDg.Setup(): hardware information test - Opening SG_RC.dll");
              if (_IMON.iMONRC_Init(0x77, 0x83, 0x8888))
              {
                _IMON.iMONRC_ChangeRCSet(0x77);
                _IMON.iMONRC_ChangeRC6(1);
                long num4 = _IMON.iMONRC_CheckDriverVersion();
                int num5 = _IMON.iMONRC_GetFirmwareVer();
                int num6 = _IMON.iMONRC_GetHWType();
                int num7 = _IMON.iMONRC_GetLastRFMode();
                Log.Info(
                  "(IDisplay) iMONLCDg.Setup(): RC TEST returned DRVR: 0x{0}, FW: 0x{1} (HW: 0x{2}), RC_HW: 0x{3}, RF: 0x{4}",
                  new object[]
                    {
                      num4.ToString("x0000000000000000"), num5.ToString("x00000000"),
                      GetVFDTypeFromFirmware(num5).ToString("x00000000"), num6.ToString("x00000000"),
                      num7.ToString("x00000000")
                    });
                if (num5 > 0)
                {
                  fWVersion = num5;
                }
                Log.Info("(IDisplay) iMONLCDg.Setup(): Closing SG_RC.dll");
                _IMON.iMONRC_Uninit();
              }
              else
              {
                long num8 = _IMON.iMONRC_CheckDriverVersion();
                int num9 = _IMON.iMONRC_GetFirmwareVer();
                int num10 = _IMON.iMONRC_GetHWType();
                int num11 = _IMON.iMONRC_GetLastRFMode();
                Log.Info("iMONLCDg.Setup(): RC TEST returned DRVR: 0x{0}, FW: 0x{1} (HW: {2}), RC_HW: 0x{3}, RF: 0x{4}",
                         new object[]
                           {
                             num8.ToString("x0000000000000000"), num9.ToString("x00000000"),
                             GetVFDTypeFromFirmware(num9).ToString("x00000000"), num10.ToString("x00000000"),
                             num11.ToString("x00000000")
                           });
                if (num9 > 0)
                {
                  Log.Info("iMONLCDg.Setup(): Found valid display information");
                  fWVersion = num9;
                }
                Log.Info("iMONLCDg.Setup(): Closing SG_RC.dll");
              }
            }
            else
            {
              Log.Info("iMONLCDg.Setup(): Hardware AutoDetect not available");
            }
          }
          catch (Exception exception)
          {
            Log.Info("iMONLCDg.Setup(): RC TEST FAILED... SG_RC.dll not found. Exception: {0}",
                     new object[] { exception.ToString() });
          }
          try
          {
            int num3;
            for (int i = 0; i < _BrandTableLength; i++)
            {
              string curBrand = _BrandTable[i, 0];
              string curApp = _BrandTable[i, 1];
              Log.Info("iMONLCDg.Setup(): checking registry for " + curBrand + " entries");
              RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software" + curBrand + "\\" + curApp, false);
              if (key != null)
              {
                num3 = (int)key.GetValue("LastVFD", 0);
                if (num3 > 0)
                {
                  Log.Info("iMONLCDg.Setup(): " + curBrand + " registry entries found - HW: {0}",
                           new object[] { num3.ToString("x00") });
                  rEGVersion = num3;
                }
                else
                {
                  Log.Info("iMONLCDg.Setup(): " + curBrand + " \"LastVFD\" key not found");
                }
              }
              else
              {
                Log.Info("iMONLCDg.Setup(): " + curBrand + " registry entries NOT found");
              }
              Registry.CurrentUser.Close();
              if (rEGVersion >= 0)
              {
                break;
              }
            }
          }
          catch (Exception exception2)
          {
            Log.Info("iMONLCDg.Setup(): registry test caught exception {0}", new object[] { exception2.ToString() });
          }
          if (fWVersion > -1)
          {
            if (GetDisplayInfoFromFirmware(fWVersion))
            {
              Log.Info("iMONLCDg.Setup(): Hardware tests determined - iMON Type: {0}, Display Type: {1} Rsrvd: {2}",
                       new object[] { _VfdType.ToString("x00"), DisplayType.TypeName(_DisplayType), _VfdReserved.ToString("x00") });
            }
            else
            {
              Log.Info("iMONLCDg.Setup(): Hardware tests determined UNSUPPORTED display type!");
              _DisplayType = DisplayType.Unsupported;
            }
          }
          else if (rEGVersion > -1)
          {
            if (GetDisplayInfoFromRegistry(rEGVersion))
            {
              Log.Info("iMONLCDg.Setup(): Registry tests determined - iMON Type: {0}, Display Type: {1} Rsrvd: {2}",
                       new object[] { _VfdType.ToString("x00"), DisplayType.TypeName(_DisplayType), _VfdReserved.ToString("x00") });
            }
            else
            {
              Log.Info("iMONLCDg.Setup(): Registry tests determined UNSUPPORTED display type!");
              _DisplayType = DisplayType.Unsupported;
              _isDisabled = true;
            }
          }
          else
          {
            Log.Info("(IDisplay) iMONLCDg.Setup(): Display Type could not be determined");
            _DisplayType = DisplayType.Unsupported;
            _isDisabled = true;
          }
          if (_DisplayType == DisplayType.Unsupported)
          {
            _isDisabled = true;
            Log.Info("(IDisplay) iMONLCDg.Setup(): Display Type is NOT SUPPORTED - Plugin disabled");
          }
          break;
      }

      if (!_isDisabled)
      {
        try
        {
          Log.Info("(IDisplay) iMONLCDg.Setup(): Testing iMON Display device");
          if (_IMON.iMONVFD_IsInited())
          {
            Log.Info("(IDisplay) iMONLCDg.Setup(): iMON Display found");
            _IMON.iMONVFD_Uninit();
          }
          Log.Info("(IDisplay) iMONLCDg.Setup(): opening display type {0}",
                   new object[] { DisplayType.TypeName(_DisplayType) });
          if (!_IMON.iMONVFD_Init(_VfdType, _VfdReserved))
          {
            Log.Info("(IDisplay) iMONLCDg.Setup(): Open failed - No iMON device found");
            _isDisabled = true;
            _errorMessage = "iMONLCDg could not find an iMON LCD display";
          }
          else
          {
            Log.Info("(IDisplay) iMONLCDg.Setup(): iMON Display device found");
            _IMON.iMONVFD_Uninit();
          }
        }
        catch (Exception exception3)
        {
          _isDisabled = true;
          _errorMessage = exception3.Message;
          Log.Info("(IDisplay) iMONLCDg.Setup(): caught an exception.");
        }
      }
      string property = GUIPropertyManager.GetProperty("#currentmodule");
      Log.Info("(IDisplay) iMONLCDg.Setup(): current module = {0}", new object[] { property });
      if ((_DisplayType == DisplayType.LCD) || (_DisplayType == DisplayType.LCD2))
      {
        _grows = linesG;
        if (_grows > 0x10)
        {
          _grows = 0x10;
          Log.Info(
            "(IDisplay) iMONLCDg.Setup(): DISPLAY CONFIGURATION (GRAPHICS MODE) ERROR - Rows must be less then or equal to 16",
            new object[0]);
        }
        _gcols = colsG;
        if (_gcols > 0x60)
        {
          _gcols = 0x60;
          Log.Info(
            "(IDisplay) iMONLCDg.Setup(): DISPLAY CONFIGURATION (GRAPHICS MODE) ERROR - Columns must be less then or equal to 96",
            new object[0]);
        }
      }
      else if (_DisplayType == DisplayType.VFD)
      {
        _trows = lines;
        if (_trows > 2)
        {
          _trows = 2;
          Log.Info(
            "(IDisplay) iMONLCDg.Setup(): DISPLAY CONFIGURATION (TEXT MODE) ERROR - Rows must be less then or equal to 2",
            new object[0]);
        }
        else
        {
          Log.Info("(IDisplay) iMONLCDg.Setup(): _trows (Text Mode Rows) set to {0}", new object[] { _trows });
        }
        Log.Info("(IDisplay) iMONLCDg.Setup(): _tcols (Text Mode Columns) set to {0}", new object[] { _tcols });
        DisplayOptions.DiskMediaStatus = false;
        DisplayOptions.VolumeDisplay = false;
        DisplayOptions.ProgressDisplay = false;
        DisplayOptions.UseCustomFont = false;
        DisplayOptions.UseLargeIcons = false;
        DisplayOptions.UseCustomIcons = false;
        DisplayOptions.UseInvertedIcons = false;
      }
      else if (_DisplayType == DisplayType.ThreeRsystems)
      {
        _trows = lines;
        if (_trows > 1)
        {
          _trows = 1;
          Log.Info("(IDisplay) iMONLCDg.Setup(): DISPLAY CONFIGURATION (3Rsystems MODE) ERROR - Rows must be 1",
                   new object[0]);
        }
        _gcols = 12;
        Log.Info(
          "(IDisplay) iMONLCDg.Setup(): DISPLAY CONFIGURATION (3Rsystems MODE) ERROR - Columns must be less then or equal to 12",
          new object[0]);

        DisplayOptions.DiskMediaStatus = false;
        DisplayOptions.VolumeDisplay = false;
        DisplayOptions.ProgressDisplay = false;
        DisplayOptions.UseCustomFont = false;
        DisplayOptions.UseLargeIcons = false;
        DisplayOptions.UseCustomIcons = false;
        DisplayOptions.UseInvertedIcons = false;
      }
      _delay = delay;
      _delayG = timeG;
      _delay = Math.Max(_delay, _delayG);
      if ((_DisplayType == DisplayType.LCD) || (_DisplayType == DisplayType.LCD2))
      {
        _delay = Math.Min(2, _delay);
      }
      Log.Info("(IDisplay) iMONLCDg.Setup(): Completed");
    }

    public string Description
    {
      get { return "SoundGraph iMON USB VFD/LCD Plugin v20_05_2009 - build 2"; }
    }

    public string ErrorMessage
    {
      get { return _errorMessage; }
    }

    public bool IsDisabled
    {
      get
      {
        Log.Debug("iMONLCDg.IsDisabled: returning {0}", new object[] { _isDisabled });
        return _isDisabled;
      }
    }

    public string Name
    {
      get { return "iMONLCDg"; }
    }

    public bool SupportsGraphics
    {
      get
      {
        if (_isDisabled)
        {
          return true;
        }
        if (_IMON == null)
        {
          return true;
        }
        if (DoDebug)
        {
          Log.Info("(IDisplay) iMONLCDg.SupportsGraphics(): _displayType = {0}",
                   new object[] { DisplayType.TypeName(_DisplayType) });
        }
        if (!_IMON.iMONVFD_IsInited())
        {
          if (DoDebug)
          {
            Log.Info("(IDisplay) iMONLCDg.SupportsGraphics(): forcing true for configuration");
          }
          return true;
        }
        if ((_DisplayType == DisplayType.LCD) || (_DisplayType == DisplayType.LCD2))
        {
          if (DoDebug)
          {
            Log.Info("(IDisplay) iMONLCDg.SupportsGraphics(): returned true");
          }
          return true;
        }
        if (DoDebug)
        {
          Log.Info("(IDisplay) iMONLCDg.SupportsGraphics(): returned false");
        }
        return false;
      }
    }

    public bool SupportsText
    {
      get { return true; }
    }

    #endregion

    private void ActivateDVM()
    {
      try
      {
        if (!DVMactive)
        {
          DVM = new DeviceVolumeMonitor(GUIGraphicsContext.form.Handle);
          if (DVM != null)
          {
            DVM.OnVolumeInserted += VolumeInserted;
            DVM.OnVolumeRemoved += VolumeRemoved;
            DVM.AsynchronousEvents = true;
            DVM.Enabled = true;
            Log.Debug("iMONLCDg.ActivateDVM(): DVM Activated");
            DVMactive = true;
          }
        }
      }
      catch (Exception exception)
      {
        DVMactive = true;
        Log.Debug("iMONLCDg.ActivateDVM(): caught exception: {0}", new object[] { exception });
      }
    }

    private string Add_VFD_Icons(int _line, string message)
    {
      if (!_USE_VFD_ICONS)
      {
        return message;
      }
      if (_USE_VFD_ICONS)
      {
        return message;
      }
      if (!_DisplayType.Equals(DisplayType.VFD))
      {
        return message;
      }
      string str = message.Length < 0x10 ? message.PadRight(14, ' ') : message.Substring(0, 14);
      str = str + ' ';
      if (_line == 0)
      {
        if (MPStatus.MP_Is_Idle || !MPStatus.MediaPlayer_Active)
        {
          return (str + IMON_VFD_CHAR_BLOCK_FILLED);
        }
        if (MPStatus.MediaPlayer_Playing)
        {
          if (MPStatus.Media_Speed < 0)
          {
            return (str + IMON_VFD_CHAR_RPLAY);
          }
          return (str + IMON_VFD_CHAR_PLAY);
        }
        if (MPStatus.MediaPlayer_Paused)
        {
          str = str + IMON_VFD_CHAR_PAUSE;
        }
        return str;
      }
      if (MPStatus.Media_IsRecording)
      {
        return (str + IMON_VFD_CHAR_RECORD);
      }
      return (str + " ");
    }

    private void AdvancedSettings_OnSettingsChanged()
    {
      Log.Info("iMONLCDg.AdvancedSettings_OnSettingsChanged(): RELOADING SETTINGS");
      AdvancedSettings advSettings = AdvSettings;
      AdvancedSettings settings2 = AdvancedSettings.Load();
      bool flag = false;
      if (!advSettings.Equals(settings2))
      {
        flag = true;
      }
      if (flag)
      {
        CleanUp();
        Thread.Sleep(100);
      }
      LoadAdvancedSettings();
      if (flag)
      {
        Setup("", Settings.Instance.TextHeight, Settings.Instance.TextWidth, 0, Settings.Instance.GraphicHeight,
              Settings.Instance.GraphicWidth, 0, Settings.Instance.BackLightControl, Settings.Instance.Backlight,
              Settings.Instance.ContrastControl, Settings.Instance.Contrast, Settings.Instance.BlankOnExit);
        Initialize();
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
      if (MPStatus.MP_Is_Idle)
      {
        if (DoDebug)
        {
          Log.Info("iMONLCDg.DisplayLines(): _BlankDisplayWhenIdle = {0}, _BlankIdleTimeout = {1}",
                   new object[] { DisplaySettings.BlankDisplayWhenIdle, DisplaySettings._BlankIdleTimeout });
        }
        if (DisplaySettings.BlankDisplayWhenIdle)
        {
          if (!_mpIsIdle)
          {
            if (DoDebug)
            {
              Log.Info("iMONLCDg.DisplayLines(): MP going IDLE");
            }
            DisplaySettings._BlankIdleTime = DateTime.Now.Ticks;
          }
          if (!_IsDisplayOff &&
              ((DateTime.Now.Ticks - DisplaySettings._BlankIdleTime) > DisplaySettings._BlankIdleTimeout))
          {
            if (DoDebug)
            {
              Log.Info("iMONLCDg.DisplayLines(): Blanking display due to IDLE state");
            }
            DisplayOff();
          }
        }
        _mpIsIdle = true;
      }
      else
      {
        if (DisplaySettings.BlankDisplayWhenIdle & _mpIsIdle)
        {
          if (DoDebug)
          {
            Log.Info("iMONLCDg.DisplayLines(): MP no longer IDLE - restoring display");
          }
          DisplayOn();
        }
        _mpIsIdle = false;
      }
    }

    public void Check_iMON_Manager_Status()
    {
      Process[] processesByName;
      Process process;
      bool flag;
      bool flag2;

      for (int i = 0; i <= _BrandTableLength; i++)
      {
        string curBrand = _BrandTable[i, 0];
        string curApp = _BrandTable[i, 1];
        Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): Checking " + curBrand + " " + curApp + " Manager registry subkey.");
        RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\" + curBrand + "\\" + curApp, true);
        if (key != null)
        {
          Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): The " + curBrand + " " + curApp + " Manager registry subkey found.");
          flag2 = true;
          if (((int)key.GetValue("RCPlugin", -1)) != 1)
          {
            Log.Info("iMONLCDgCheck_iMON_Manager_Status(): \"RCPlugin\" configuration error.");
            Log.Info(
              "iMONLCDg.Check_iMON_Manager_Status(): The " + curBrand + " " + curApp + " Manager is not set correctly. The configuration has been corrected.",
              new object[0]);
            flag2 = false;
          }
          if (!_LeaveFrontviewActive)
          {
            if (((int)key.GetValue("RunFront", -1)) != 0)
            {
              Log.Info("iMONLCDgCheck_iMON_Manager_Status(): \"RunFront\" configuration error.");
              Log.Info(
                "iMONLCDg.Check_iMON_Manager_Status(): The " + curBrand + " " + curApp + " Manager is not set correctly. The configuration has been corrected.",
                new object[0]);
              flag2 = false;
            }
          }
          if (_ForceManagerReload)
          {
            Log.Info("iMONLCDgCheck_iMON_Manager_Status(): Forcing " + curBrand + " " + curApp + " Manager reload...");
            flag2 = false;
          }
          if (!flag2)
          {
            key.SetValue("RCPlugin", 1, RegistryValueKind.DWord);
            if (!_LeaveFrontviewActive)
            {
              key.SetValue("RunFront", 0, RegistryValueKind.DWord);
            }
            Registry.CurrentUser.Close();
            Thread.Sleep(100);
            processesByName = Process.GetProcessesByName(curApp);
            Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): Found {0} instances of " + curBrand + " " + curApp + " Manager",
                      new object[] { processesByName.Length });
            if (processesByName.Length > 0)
            {
              if (curBrand == _BrandTable[0, 0])
              {
                _UsingSoundgraphManager = true;
              }
              else
              {
                _UsingAntecManager = true;
              }
              Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): Stopping " + curBrand + " " + curApp + " Manager");
              processesByName[0].Kill();
              flag = false;
              while (!flag)
              {
                Thread.Sleep(100);
                Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): Waiting for " + curBrand + " " + curApp + " Manager to exit");
                processesByName[0].Dispose();
                processesByName = Process.GetProcessesByName(curApp);
                if (processesByName.Length == 0)
                {
                  flag = true;
                }
              }
              Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): " + curBrand + " " + curApp + " Manager Stopped");
              Win32Functions.RedrawNotificationArea();
              process = new Process
                          {
                            StartInfo =
                            {
                              WorkingDirectory = FindManagerPath(curBrand, curApp),
                              FileName = curApp + ".exe"
                            }
                          };
              Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): ReStarting " + curBrand + " " + curApp + " Manager");
              Process.Start(process.StartInfo);
            }
          }
          else
          {
            if (!_LeaveFrontviewActive)
            {
              Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): The " + curBrand + " " + curApp + " Manager registry entries are correct.",
                        new object[0]);
              key.SetValue("RunFront", 0, RegistryValueKind.DWord);
            }
            processesByName = Process.GetProcessesByName(curApp);
            Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): Found {0} instances of " + curBrand + " " + curApp + " Manager",
                      new object[] { processesByName.Length });
          }
          key.Close();
        }
        else
        {
          Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): The " + curBrand + " " + curApp + " Manager registry subkey NOT FOUND.",
                    new object[0]);
          Registry.CurrentUser.Close();
          processesByName = Process.GetProcessesByName(curApp);
          Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): state check: Found {0} instances of " + curBrand + " " + curApp + " Manager",
                    new object[] { processesByName.Length });
          if (processesByName.Length > 0)
          {
            Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): Inconsistant state: Forcing shutdown of " + curBrand + " " + curApp + " Manager",
                      new object[0]);
            processesByName[0].Kill();
            flag = false;
            while (!flag)
            {
              Thread.Sleep(100);
              Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): Waiting for " + curBrand + " " + curApp + " Manager to exit");
              processesByName[0].Dispose();
              processesByName = Process.GetProcessesByName(curApp);
              if (processesByName.Length == 0)
              {
                flag = true;
              }
            }
            Log.Info("iMONLCDg.Check_iMON_Manager_Status(): Inconsistant state: " + curBrand + " " + curApp + " Manager Stopped");
            Win32Functions.RedrawNotificationArea();
          }
        }
      }

      Log.Debug("iMONLCDg.Check_iMON_Manager_Status(): iMON/VFD Manager configuration check completed");
      if (_EnsureManagerStartup)
      {
        Log.Debug(
          "iMONLCDg.Check_iMON_Manager_Status(): Ensure Manager Start is selected.. ensuring that the manager is running",
          new object[0]);
        for (int i = 0; i <= _BrandTableLength; i++)
        {
          string curBrand = _BrandTable[i, 0];
          string curApp = _BrandTable[i, 1];
          Process[] processArray = Process.GetProcessesByName(curApp);
          string processName = string.Empty;
          if (processArray.Length == 0)
          {
            var process2 = new Process();
            string strPath = FindManagerPath(curBrand, curApp);
            if (String.IsNullOrEmpty(strPath))
            {
              Log.Info(
                "iMONLCDg.Check_iMON_Manager_Status(): ERROR: Unable to ensure " + curBrand + " " + curApp + " Manager is running. Installation not found.",
                new object[0]);
            }
            else
            {
              processName = _BrandTable[i, 1];
              process2.StartInfo.WorkingDirectory = strPath;
              process2.StartInfo.FileName = processName + ".exe";
              Log.Info("iMONLCDg.Check_iMON_Manager_Status(): Starting " + curBrand + " " + curApp + " Manager");

              Process.Start(process2.StartInfo);
              Thread.Sleep(0x3e8);
              int num2 = 0x1388;
              bool flag3 = false;
              while (!flag3 & (num2 > 0))
              {
                processArray = Process.GetProcessesByName(processName);
                if (processArray.Length > 0)
                {
                  if (processArray[0].Responding)
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
                _UsingAntecManager = false;
                _UsingSoundgraphManager = false;
                Log.Info("iMONLCDg.Check_iMON_Manager_Status(): Could not start iMON/VFD Manager process");
              }
              else
              {
                _UsingAntecManager = false;
                _UsingSoundgraphManager = false;
                if (curBrand == _BrandTable[0, 0])
                {
                  _UsingSoundgraphManager = true;
                }
                else
                {
                  _UsingAntecManager = true;
                }
                Log.Info("iMONLCDg.Check_iMON_Manager_Status(): Started " + curBrand + " " + curApp + " Manager");
              }
            }
          }
          else
          {
            _UsingAntecManager = false;
            _UsingSoundgraphManager = false;
            if (curBrand == _BrandTable[0, 0])
            {
              _UsingSoundgraphManager = true;
            }
            else
            {
              _UsingAntecManager = true;
            }
            Log.Info("iMONLCDg.Check_iMON_Manager_Status(): " + curBrand + " " + curApp + " Manager is running");
          }
        }
      }
    }

    public void Clear()
    {
      if (!_isDisabled)
      {
        Log.Debug("iMONLCDg.Clear(): called");
        for (int i = 0; i < 2; i++)
        {
          _lines[i] = new string(' ', Settings.Instance.TextWidth);
        }
        DisplayLines();
        Log.Debug("iMONLCDg.Clear(): completed");
      }
    }

    private void ClearDisplay()
    {
      Clear();
    }

    private void CloseLcd()
    {
      Log.Info("iMONLCDg.CloseLcd(): called");
      if (_IMON.iMONVFD_IsInited())
      {
        if ((_DisplayType == DisplayType.LCD) || (_DisplayType == DisplayType.LCD2))
        {
          if (!_displayTest)
          {
            while (_iconThread.IsAlive)
            {
              Log.Info("iMONLCDg.CloseLcd(): Stopping iMONLCDg.UpdateIcons() Thread");
              lock (ThreadMutex)
              {
                _stopUpdateIconThread = true;
              }
              _stopUpdateIconThread = true;
              Thread.Sleep(500);
            }
          }
          Log.Info("iMONLCDg.CloseLcd(): Preparing for shutdown");
          SendData(Command.SetIcons);
          SendData(Command.SetLines0);
          SendData(Command.SetLines1);
          SendData(Command.SetLines2);
          if (_BlankDisplayOnExit)
          {
            if (_DisplayType == DisplayType.LCD2)
            {
              Log.Info("iMONLCDg.CloseLcd(): sending display shutdown command to LCD2");
              SendData(-8646911284551352312L);
              SendData(-8502796096475496448L);
            }
            else
            {
              Log.Info("iMONLCDg.CloseLcd(): sending display shutdown command to LCD");
              SendData(Command.Shutdown);
            }
          }
          else
          {
            ulong num;
            DateTime now = DateTime.Now;
            if (_DisplayType == DisplayType.LCD2)
            {
              Log.Debug("iMONLCDg.CloseLcd(): sending clock enable command to LCD2");
              num = 9799832789158199296L;
            }
            else
            {
              Log.Debug("iMONLCDg.CloseLcd(): sending clock enable command to LCD");
              num = 0x5000000000000000L;
            }
            num += ((ulong)now.Second) << 0x30;
            num += ((ulong)now.Minute) << 40;
            num += ((ulong)now.Hour) << 0x20;
            num += ((ulong)now.Day) << 0x18;
            num += ((ulong)now.Month) << 0x10;
            num += (ulong)((now.Year & 15L) << 8);
            num += 0x80L;
            SendData(num);
          }
          SendData(Command.KeypadLightOff);
          if (DisplayOptions.UseCustomFont)
          {
            CFont.CloseFont();
          }
          if (DisplayOptions.UseLargeIcons & DisplayOptions.UseCustomIcons)
          {
            CustomLargeIcon.CloseIcons();
          }
        }
        else if (_DisplayType == DisplayType.VFD)
        {
          if (EQSettings.UseEqDisplay || DisplaySettings.BlankDisplayWithVideo)
          {
            while (_iconThread.IsAlive)
            {
              Log.Info("iMONLCDg.CloseLcd(): Stoping iMONLCDg.VFD_EQ_Update() Thread");
              lock (ThreadMutex)
              {
                _stopUpdateIconThread = true;
              }
              _stopUpdateIconThread = true;
              Thread.Sleep(500);
            }
          }
          if (_BlankDisplayOnExit)
          {
            Log.Info("iMONLCDg.CloseLcd(): Shutting down VFD display!!");
            SetText("", "");
          }
          else
          {
            Log.Info("iMONLCDg.CloseLcd(): Sending Shutdown message to VFD display!!");
            if ((DisplaySettings._Shutdown1 != string.Empty) || (DisplaySettings._Shutdown2 != string.Empty))
            {
              SetText(DisplaySettings._Shutdown1, DisplaySettings._Shutdown2);
            }
            else
            {
              SetText("   MediaPortal  ", "   not active   ");
              SetVFDClock();
            }
          }
        }
        else if (_DisplayType == DisplayType.ThreeRsystems)
        {
          if (EQSettings.UseEqDisplay || DisplaySettings.BlankDisplayWithVideo)
          {
            while (_iconThread.IsAlive)
            {
              Log.Info("iMONLCDg.CloseLcd(): Stoping iMONLCDg.VFD_EQ_Update() Thread");
              lock (ThreadMutex)
              {
                _stopUpdateIconThread = true;
              }
              _stopUpdateIconThread = true;
              Thread.Sleep(500);
            }
          }
          if (_BlankDisplayOnExit)
          {
            Log.Info("iMONLCDg.CloseLcd(): Sending Shutdown message to LCD3R display!!");
            if (DisplaySettings._Shutdown1 != string.Empty)
            {
              SendText3R(DisplaySettings._Shutdown1);
            }
            else
            {
              SendData(0x21c000000000000L);
              SendData(2L);
              SendText3R(" not active ");
            }
          }
          else
          {
            Log.Info("iMONLCDg.CloseLcd(): Shutting down LCD3R display (with clock)!!");
            SendData(0x21c010000000000L);
            SendData(2L);
          }
        }
        _IMON.iMONVFD_Uninit();
      }
      else
      {
        Log.Info("iMONLCDg.CloseLcd(): Display is not open!!");
      }
      if (_MonitorPower && !_IsHandlingPowerEvent)
      {
        Log.Info("iMONLCDg.CloseLcd(): Removing Power State Monitor callback from system event thread");
        SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
      }

      RestartFrontview();
      Log.Info("iMONLCDg.CloseLcd(): completed");
    }

    private static ulong ConvertPluginIconsToDriverIcons(ulong IconMask)
    {
      return (IconMask & (0xffffffffffL));
    }

    private void DisplayEQ()
    {
      if (!(EQSettings.UseEqDisplay & EQSettings._EqDataAvailable))
      {
        return;
      }
      if (EQSettings.RestrictEQ &
          ((DateTime.Now.Ticks - EQSettings._LastEQupdate.Ticks) < EQSettings._EqUpdateDelay))
      {
        return;
      }
      if (DoDebug)
      {
        Log.Info("\niMONLCDg.DisplayEQ(): Retrieved {0} samples of Equalizer data.",
                 new object[] { EQSettings.EqFftData.Length / 2 });
      }
      if ((EQSettings.UseStereoEq || EQSettings.UseVUmeter) || EQSettings.UseVUmeter2)
      {
        if (EQSettings.UseStereoEq)
        {
          EQSettings.Render_MaxValue = 100;
          EQSettings.Render_BANDS = 8;
          EQSettings.EqArray[0] = 0x63;
          if ((_DisplayType == DisplayType.LCD) || (_DisplayType == DisplayType.LCD2))
          {
            EQSettings.Render_MaxValue = (EQSettings._useEqMode == 2) ? 8 : 0x10;
            EQSettings.EqArray[0] = (byte)EQSettings._useEqMode;
          }
          else if (_DisplayType == DisplayType.ThreeRsystems)
          {
            EQSettings.Render_MaxValue = 6;
            EQSettings.EqArray[0] = 0;
          }
          MiniDisplayHelper.ProcessEqData(ref EQSettings);
          for (int i = 0; i < EQSettings.Render_BANDS; i++)
          {
            switch (EQSettings.EqArray[0])
            {
              case 2:
                {
                  var num2 = (byte)(EQSettings.EqArray[1 + i] & 15);
                  EQSettings.EqArray[1 + i] = (byte)((num2 << 4) | num2);
                  var num3 = (byte)(EQSettings.EqArray[9 + i] & 15);
                  EQSettings.EqArray[9 + i] = (byte)((num3 << 4) | num3);
                  break;
                }
            }
          }
          for (int j = 15; j > 7; j--)
          {
            EQSettings.EqArray[j + 1] = EQSettings.EqArray[j];
          }
          EQSettings.EqArray[8] = 0;
          EQSettings.EqArray[9] = 0;
        }
        else
        {
          EQSettings.Render_MaxValue = 80;
          EQSettings.Render_BANDS = 1;
          if ((_DisplayType == DisplayType.LCD) || (_DisplayType == DisplayType.LCD2))
          {
            EQSettings.Render_MaxValue = 0x60;
            if (EQSettings._useVUindicators)
            {
              EQSettings.Render_MaxValue = 0x60;
            }
          }
          else if (EQSettings._useVUindicators)
          {
            EQSettings.Render_MaxValue = 0x4b;
          }
          MiniDisplayHelper.ProcessEqData(ref EQSettings);
        }
      }
      else
      {
        EQSettings.Render_MaxValue = 100;
        EQSettings.Render_BANDS = 0x10;
        EQSettings.EqArray[0] = 0x63;
        if ((_DisplayType == DisplayType.LCD) || (_DisplayType == DisplayType.LCD2))
        {
          EQSettings.Render_MaxValue = (EQSettings._useEqMode == 2) ? 8 : 0x10;
          EQSettings.EqArray[0] = (byte)EQSettings._useEqMode;
        }
        else if (_DisplayType == DisplayType.ThreeRsystems)
        {
          EQSettings.Render_MaxValue = 6;
          EQSettings.EqArray[0] = 0;
        }
        MiniDisplayHelper.ProcessEqData(ref EQSettings);
        for (int k = 0; k < EQSettings.Render_BANDS; k++)
        {
          switch (EQSettings.EqArray[0])
          {
            case 2:
              {
                var num6 = (byte)(EQSettings.EqArray[1 + k] & 15);
                EQSettings.EqArray[1 + k] = (byte)((num6 << 4) | num6);
                break;
              }
          }
        }
      }
      if ((_DisplayType == DisplayType.LCD) || (_DisplayType == DisplayType.LCD2))
      {
        if (!EQSettings.UseVUmeter && !EQSettings.UseVUmeter2)
        {
          SetEQ(EQSettings.EqArray);
        }
        else
        {
          DrawVU(EQSettings.EqArray);
        }
      }
      else if (_DisplayType == DisplayType.ThreeRsystems)
      {
        for (int m = 0; m < 8; m++)
        {
          EQSettings.EqArray[1 + m] =
            (byte)((EQSettings.EqArray[1 + m] << 4) + EQSettings.EqArray[9 + m]);
        }
        ulong data = 0x901000000000000L;
        ulong num9 = 2L;
        data = data + EQSettings.EqArray[1] << 40;
        data = data + EQSettings.EqArray[2] << 0x20;
        data = data + EQSettings.EqArray[3] << 0x18;
        data = data + EQSettings.EqArray[4] << 0x10;
        data = data + EQSettings.EqArray[5] << 8;
        num9 = num9 + EQSettings.EqArray[6] << 40;
        num9 = num9 + EQSettings.EqArray[7] << 0x20;
        num9 = num9 + EQSettings.EqArray[8] << 0x18;
        SendData(0x200020000000000L);
        SendData(2L);
        SendData(0xd0f202020202000L);
        SendData(0x2020202020202002L);
        SendData(data);
        SendData(num9);
      }
      else
      {
        if (!EQSettings.UseVUmeter && !EQSettings.UseVUmeter2)
        {
          var destinationArray = new int[0x10];
          Array.Copy(EQSettings.EqArray, 1, destinationArray, 0, 0x10);
          lock (DWriteMutex)
          {
            _IMON.iMONVFD_SetEQ(destinationArray);
            goto Label_0613;
          }
        }
        DrawVU(EQSettings.EqArray);
      }
    Label_0613:
      EQSettings._LastEQupdate = DateTime.Now;
      if ((DateTime.Now.Ticks - EQSettings._EQ_FPS_time.Ticks) < 0x989680L)
      {
        EQSettings._EQ_Framecount++;
      }
      else
      {
        EQSettings._Max_EQ_FPS = Math.Max(EQSettings._Max_EQ_FPS, EQSettings._EQ_Framecount);
        EQSettings._EQ_Framecount = 0;
        EQSettings._EQ_FPS_time = DateTime.Now;
      }
    }

    private void DisplayLines()
    {
      UpdateAdvancedSettings();
      if (DoDebug)
      {
        Log.Info("iMONLCDg.DisplayLines(): Sending text to display type {0}",
                 new object[] { DisplayType.TypeName(_DisplayType) });
      }
      try
      {
        MiniDisplayHelper.GetSystemStatus(ref MPStatus);
        Check_Idle_State();
        if (EQSettings._EqDataAvailable || _IsDisplayOff)
        {
          if (DoDebug)
          {
            Log.Info("iMONLCDg.DisplayLines(): Suppressing display update!");
          }
        }
        else if ((_DisplayType == DisplayType.LCD) || (_DisplayType == DisplayType.LCD2))
        {
          if (DoDebug)
          {
            Log.Info("iMONLCDg.DisplayLines(): Calling SendText() to emulate VFD for {0}",
                     new object[] { DisplayType.TypeName(_DisplayType) });
          }
          SendText(_lines[0], _lines[1]);
        }
        else if (_DisplayType == DisplayType.VFD)
        {
          if (DoDebug)
          {
            Log.Info("iMONLCDg.DisplayLines(): Calling SetText()");
          }
          SetText(_lines[0], _lines[1]);
        }
        else if (_DisplayType == DisplayType.ThreeRsystems)
        {
          SendText3R(_lines[0]);
        }
      }
      catch (Exception exception)
      {
        Log.Debug("iMONLCDg.DisplayLines(): CAUGHT EXCEPTION {0}", new object[] { exception });
      }
    }

    private void DisplayOff()
    {
      if (!_IsDisplayOff)
      {
        if (DisplaySettings.EnableDisplayAction & DisplaySettings._DisplayControlAction)
        {
          if ((DateTime.Now.Ticks - DisplaySettings._DisplayControlLastAction) <
              DisplaySettings._DisplayControlTimeout)
          {
            if (DoDebug)
            {
              Log.Info("iMONLCDg.DisplayOff(): DisplayControlAction Timer = {0}.",
                       new object[] { DateTime.Now.Ticks - DisplaySettings._DisplayControlLastAction });
            }
            return;
          }
          if (DoDebug)
          {
            Log.Info("iMONLCDg.DisplayOff(): DisplayControlAction Timeout expired.");
          }
          DisplaySettings._DisplayControlAction = false;
          DisplaySettings._DisplayControlLastAction = 0L;
        }
        if (DoDebug)
        {
          Log.Info("iMONLCDg.DisplayOff(): called");
        }
        lock (DWriteMutex)
        {
          if ((_DisplayType == DisplayType.LCD) || (_DisplayType == DisplayType.LCD2))
          {
            if (DoDebug)
            {
              Log.Info("iMONLCDg.DisplayOff(): Sending Shutdown command to LCD");
            }
            if (_DisplayType == DisplayType.LCD2)
            {
              SendData(-8646911284551352312L);
            }
            else
            {
              SendData(Command.Shutdown);
            }
          }
          else
          {
            if (DoDebug)
            {
              Log.Info("iMONLCDg.DisplayOff(): Sending blank display to VFD");
            }
            _IMON.iMONVFD_SetText(new string(' ', 0x10), new string(' ', 0x10));
          }
          _IsDisplayOff = true;
        }
        if (DoDebug)
        {
          Log.Info("iMONLCDg.DisplayOff(): completed");
        }
      }
    }

    private void DisplayOn()
    {
      if (!_IsDisplayOff)
      {
        return;
      }
      if (DoDebug)
      {
        Log.Info("iMONLCDg.DisplayOn(): called");
      }
      if ((_DisplayType == DisplayType.LCD) || (_DisplayType == DisplayType.LCD2))
      {
        lock (DWriteMutex)
        {
          if (DoDebug)
          {
            Log.Info("iMONLCDg.DisplayOn(): Sending Display ON command to LCD");
          }
          _IsDisplayOff = false;
          if (_DisplayType == DisplayType.LCD2)
          {
            SendData(-8646911284551352256L);
            SendData(-8646911284551352256L);
            SendData(-8502796096475496448L);
            SendData(Command.SetContrast, _ContrastLevel);
            SendData(-8791026472627208192L);
            SendData(Command.SetIcons);
            SendData(Command.SetLines0);
            SendData(Command.SetLines1);
            SendData(Command.SetLines2);
            ClearDisplay();
            SendData(-8358680908399640433L);
          }
          else
          {
            SendData(Command.DisplayOn);
          }
          goto Label_0150;
        }
      }
      lock (DWriteMutex)
      {
        _IsDisplayOff = false;
      }
    Label_0150:
      if (DoDebug)
      {
        Log.Info("iMONLCDg.DisplayOn(): called");
      }
    }

    public void DoDisplayTest()
    {
      var mask = new BuiltinIconMask();
      if ((_DisplayType == DisplayType.LCD) || (_DisplayType == DisplayType.LCD2))
      {
        Log.Debug("(IDisplay) iMONLCDg.Setup() configure - do display test");
        _displayTest = true;
        OpenLcd();
        ClearDisplay();
        Thread.Sleep(500);
        SendText("iMONLCDg", "Display Test");
        Thread.Sleep(500);
        SendText("iMONLCDg", "All Icons");
        for (int i = 0; i < 2; i++)
        {
          SendData(Command.SetIcons, mask.ICON_ALL);
          Thread.Sleep(500);
          SendData(Command.SetIcons);
          Thread.Sleep(500);
        }
        var icon = new DiskIcon();
        icon.Reset();
        icon.On();
        SendText("iMONLCDg", "Disk On");
        Thread.Sleep(500);
        SendText("iMONLCDg", "Disk Spin CW");
        icon.RotateCW();
        for (int j = 0; j < 0x10; j++)
        {
          icon.Animate();
          SendData(Command.SetIcons, icon.Mask);
          Thread.Sleep(250);
        }
        SendText("iMONLCDg", "Disk Spin CCW");
        icon.RotateCCW();
        for (int k = 0; k < 0x10; k++)
        {
          icon.Animate();
          SendData(Command.SetIcons, icon.Mask);
          Thread.Sleep(250);
        }
        SendText("iMONLCDg", "Disk Flash");
        icon.RotateOff();
        icon.FlashOn();
        for (int m = 0; m < 0x10; m++)
        {
          icon.Animate();
          SendData(Command.SetIcons, icon.Mask);
          Thread.Sleep(250);
        }
        CloseLcd();
        _displayTest = false;
        Log.Debug("(IDisplay) iMONLCDg.Setup() configure - display test complete");
      }
    }

    private void DrawVU(byte[] EqDataArray)
    {
      if (DoDebug)
      {
        Log.Info("iMONLCDg.DrawVU(): Called");
      }
      if ((_DisplayType != DisplayType.LCD) && (_DisplayType != DisplayType.LCD2))
      {
        if (DoDebug)
        {
          Log.Info("iMONLCDg.DrawVU(): Drawing VU meter for VFD display");
        }
        string firstLine = "";
        string secondLine = "";
        char ch = IMON_CHAR_6_BARS;
        int num7 = 0x10;
        if (EQSettings._useVUindicators)
        {
          if (EQSettings.UseVUmeter)
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
          else if (EQSettings.UseVUmeter)
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
        if (EQSettings.UseVUmeter2 && EQSettings._useVUindicators)
        {
          secondLine = secondLine + "R";
        }
        if (DoDebug)
        {
          Log.Info("iMONLCDg.DrawVU(): Sending VU meter data to display: L = \"{0}\" - R = \"{1}\"",
                   new object[] { firstLine, secondLine });
        }
        _IMON.iMONVFD_SetText(firstLine, secondLine);
      }
      else
      {
        if (DoDebug)
        {
          Log.Info("iMONLCDg.DrawVU(): Drawing Graphical VU meter for LCD display");
        }
        int num = 0x60;
        int num2 = 0;
        var pixelArray = new byte[0xc0];
        if (EQSettings._useVUindicators)
        {
          num = 0x58;
          for (int m = 5; m >= 0; m--)
          {
            if ((m + num2) < 0x60)
            {
              pixelArray[num2 + m] = DisplayOptions.UseCustomFont
                                       ? BitReverse(CFont.PixelData(0x4c, m))
                                       : BitReverse(_Font8x5[0x4c, m]);
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
        if (EQSettings._useVUindicators)
        {
          for (int n = 5; n >= 0; n--)
          {
            if (DisplayOptions.UseCustomFont)
            {
              if (EQSettings.UseVUmeter)
              {
                pixelArray[num2 + n] = BitReverse(CFont.PixelData(0x52, n));
              }
              else
              {
                pixelArray[(num2 + 90) + n] = BitReverse(CFont.PixelData(0x52, n));
              }
            }
            else if (EQSettings.UseVUmeter)
            {
              pixelArray[num2 + n] = BitReverse(_Font8x5[0x52, n]);
            }
            else
            {
              pixelArray[(num2 + 90) + n] = BitReverse(_Font8x5[0x52, n]);
            }
          }
          if (EQSettings.UseVUmeter)
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
          if (EQSettings.UseVUmeter)
          {
            pixelArray[num2 + k] = 0x7e;
          }
          else
          {
            pixelArray[(num2 + (num - 1)) - k] = 0x7e;
          }
        }
        SendPixelArrayRaw(pixelArray);
      }
      if (DoDebug)
      {
        Log.Info("iMONLCDg.DrawVU(): completed");
      }
    }

    public string FindImonVFDdll()
    {
      RegistryKey key;
      string str;
      if (DoDebug)
      {
        Log.Info("iMONLCDg.FindImonVFDdll(): called.");
      }
      bool flag = false;
      bool flag2 = false;
      string str2 = string.Empty;
      string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
      string str4 = string.Empty;
      string str5 = string.Empty;
      if (Registry.CurrentUser.OpenSubKey(@"Software\Antec\VFD", false) != null)
      {
        if (DoDebug)
        {
          Log.Info("iMONLCDg.FindImonVFDdll(): found Antec registry keys.");
        }
        flag = true;
        Registry.CurrentUser.Close();
        key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\VFD.exe", false);
        if (key != null)
        {
          str4 = (string)key.GetValue("Path", string.Empty);
          if (str4 == string.Empty)
          {
            if (DoDebug)
            {
              Log.Info("iMONLCDg.FindImonVFDdll(): Antec file Path registry key not found. trying default path",
                       new object[0]);
            }
            str4 = folderPath + @"\Antec\VFD";
          }
          else if (DoDebug)
          {
            Log.Info("iMONLCDg.FindImonVFDdll(): found Antec file Path registry key.");
          }
        }
        else
        {
          if (DoDebug)
          {
            Log.Info("iMONLCDg.FindImonVFDdll(): Antec file Path registry key not found. trying default path",
                     new object[0]);
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
        if (DoDebug)
        {
          Log.Info("iMONLCDg.FindImonVFDdll(): found SoundGraph registry keys.");
        }
        flag2 = true;
        Registry.CurrentUser.Close();
        key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\iMON.exe", false);
        if (key != null)
        {
          str5 = (string)key.GetValue("Path", string.Empty);
          if (str5 == string.Empty)
          {
            if (DoDebug)
            {
              Log.Info("iMONLCDg.FindImonVFDdll(): SoundGraph file Path registry key not found. trying default path",
                       new object[0]);
            }
            str5 = folderPath + @"\SoundGraph\iMON";
          }
          else if (DoDebug)
          {
            Log.Info("iMONLCDg.FindImonVFDdll(): found SoundGraph file Path registry key.");
          }
        }
        else
        {
          if (DoDebug)
          {
            Log.Info("iMONLCDg.FindImonVFDdll(): SoundGraph file Path registry key not found. trying default path",
                     new object[0]);
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
          if (DoDebug)
          {
            Log.Info("iMONLCDg.FindImonVFDdll(): Selected Antec DLL.");
          }
          str2 = str;
        }
      }
      else if (!flag & flag2)
      {
        str = str5 + @"\sg_vfd.dll";
        if (File.Exists(str))
        {
          if (DoDebug)
          {
            Log.Info("iMONLCDg.FindImonVFDdll(): Selected SoundGraph DLL.");
          }
          str2 = str;
        }
      }
      else
      {
        str = str4 + @"\sg_vfd.dll";
        if (File.Exists(str))
        {
          if (DoDebug)
          {
            Log.Info("iMONLCDg.FindImonVFDdll(): Picked Antec DLL.");
          }
          str2 = str;
        }
        else
        {
          str = str5 + @"\sg_vfd.dll";
          if (File.Exists(str))
          {
            if (DoDebug)
            {
              Log.Info("iMONLCDg.FindImonVFDdll(): Picked Soundgraph DLL.");
            }
            str2 = str;
          }
        }
      }
      if (DoDebug)
      {
        Log.Info("iMONLCDg.FindImonVFDdll(): completed - selected file \"{0}\".", new object[] { str2 });
      }
      return str2;
    }

    public string FindManagerPath(string curBrand, string curApp)
    {
      if (DoDebug)
      {
        Log.Info("iMONLCDg.FindManagerPath(): called.");
      }
      string str = string.Empty;
      if (Registry.CurrentUser.OpenSubKey("Software\\" + curBrand + "\\" + curApp, false) != null)
      {
        Registry.CurrentUser.Close();
        RegistryKey key =
          Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + curApp + ".exe", false);
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
      if (DoDebug)
      {
        Log.Info("iMONLCDg.FindManagerPath(): selected path = \"{0}\".", new object[] { str });
      }
      return str;
    }

    public void ForceManagerRestart()
    {
      if (!_ForceManagerRestart)
      {
        if (DoDebug)
        {
          Log.Info("iMONLCDg.ForceManagerRestart(): Option not selected... restart not required.");
        }
      }
      else
      {
        for (int i = 0; i <= _BrandTableLength; i++)
        {
          string curBrand = _BrandTable[i, 0];
          string curApp = _BrandTable[i, 1];
          string str = FindManagerPath(curBrand, curApp);
          if (String.IsNullOrEmpty(str))
          {
            if (DoDebug)
            {
              Log.Info("iMONLCDg.ForceManagerRestart(): Manager installation not found... restart not possible.");
            }
          }
          else
          {
            Process process;
            Process[] processesByName = Process.GetProcessesByName(curApp);
            if (processesByName.Length > 0)
            {
              if (curBrand == _BrandTable[0, 0])
              {
                _UsingSoundgraphManager = true;
              }
              else
              {
                _UsingAntecManager = true;
              }
              if (DoDebug)
              {
                Log.Info("iMONLCDg.ForceManagerRestart(): Found " + curBrand + " " + curApp + " Manager process.");
              }
              processesByName[0].Kill();
              processesByName[0].WaitForExit(2000);
              Log.Debug("iMONLCDg.ForceManagerRestart(): " + curBrand + " " + curApp + " Manager Stopped");

              Win32Functions.RedrawNotificationArea();

              process = new Process
                                    {
                                      StartInfo =
                                        {
                                          WorkingDirectory = str,
                                          FileName = curApp + ".exe"
                                        }
                                    };
              Log.Debug("iMONLCDg.ForceManagerRestart(): ReStarting " + curBrand + " " + curApp + " Manager");
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
        Log.Info("iMONLCDg.GetDisplayInfoFromFirmware(): Searching for Firmware version = {0}",
                 new object[] { FWVersion.ToString("x00") });
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
            Log.Info(
              "iMONLCDg.GetDisplayInfoFromFirmware(): Found version match - FW: {0}, iMON Type: {1}, Reserved: {2}, Display Type: {3}",
              new object[]
                {
                  FWVersion.ToString("x00"), _VfdType.ToString("x00"), _VfdReserved.ToString("x00"),
                  DisplayType.TypeName(_DisplayType)
                });
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
            Log.Info(
              "iMONLCDg.GetDisplayInfoFromFirmware(): Found version match - FW: {0}, iMON Type: {1}, Reserved: {2}, Display Type: {3}",
              new object[]
                {
                  FWVersion.ToString("x00"), _VfdType.ToString("x00"), _VfdReserved.ToString("x00"),
                  DisplayType.TypeName(_DisplayType)
                });
            return true;
          }
        }
      }
      if (flag)
      {
        Log.Info("iMONLCDg.GetDisplayInfoFromFirmware(): version match NOT FOUND");
      }
      return false;
    }

    private static bool GetDisplayInfoFromRegistry(int REGVersion)
    {
      bool flag = Assembly.GetEntryAssembly().FullName.Contains("Configuration") | Settings.Instance.ExtensiveLogging;
      if (flag)
      {
        Log.Info("iMONLCDg.GetDisplayInfoFromRegistry(): searching for display type {0}",
                 new object[] { REGVersion.ToString("x00") });
      }
      for (int i = 0; (_iMON_FW_Display[i, 0] != 0) & (i < _iMON_FW_Display.Length); i++)
      {
        if (_iMON_FW_Display[i, 2] == REGVersion)
        {
          if (flag)
          {
            Log.Info("iMONLCDg.GetDisplayInfoFromRegistry(): Found display type match");
          }
          _VfdType = _iMON_FW_Display[i, 2];
          _VfdReserved = _iMON_FW_Display[i, 3];
          _DisplayType = _iMON_FW_Display[i, 4];
          Log.Info(
            "iMONLCDg.GetDisplayInfoFromRegistry(): Found display type match - iMON Type: {0}, Reserved: {1}, Display Type: {2}",
            new object[] { _VfdType.ToString("x00"), _VfdReserved.ToString("x00"), DisplayType.TypeName(_DisplayType) });
          return true;
        }
      }
      if (flag)
      {
        Log.Info("iMONLCDg.GetDisplayInfoFromRegistry(): display type match NOT FOUND");
      }
      return false;
    }

    private void GetEQ()
    {
      lock (DWriteMutex)
      {
        EQSettings._EqDataAvailable = MiniDisplayHelper.GetEQ(ref EQSettings);
        _iconThread.Priority = EQSettings._EqDataAvailable ? ThreadPriority.AboveNormal : ThreadPriority.BelowNormal;
      }
    }

    private static int GetVFDTypeFromFirmware(int FWVersion)
    {
      bool flag = Assembly.GetEntryAssembly().FullName.Contains("Configuration") | Settings.Instance.ExtensiveLogging;
      if (flag)
      {
        Log.Info("iMONLCDg.GetVFDTypeFromFirmware(): Searching for Firmware version {0}",
                 new object[] { FWVersion.ToString("x00") });
      }
      for (int i = 0; (_iMON_FW_Display[i, 0] != 0) & (i < _iMON_FW_Display.Length); i++)
      {
        if (_iMON_FW_Display[i, 1] == 0)
        {
          if (_iMON_FW_Display[i, 0] == FWVersion)
          {
            if (flag)
            {
              Log.Info("iMONLCDg.GetVFDTypeFromFirmware(): Found version match");
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
              Log.Info("iMONLCDg.GetVFDTypeFromFirmware(): Found version match");
            }
            return _iMON_FW_Display[i, 2];
          }
        }
      }
      if (flag)
      {
        Log.Info("iMONLCDg.GetVFDTypeFromFirmware(): version match NOT FOUND");
      }
      return -1;
    }

    private void InitializeDriver()
    {
      DoDebug = Assembly.GetEntryAssembly().FullName.Contains("Configuration") | Settings.Instance.ExtensiveLogging;
      _IsConfiguring = Assembly.GetEntryAssembly().FullName.Contains("Configuration");
      Log.Info("iMONLCDg.InitializeDriver(): started.");
      Log.Info("iMONLCDg.InitializeDriver(): iMONLCDg Driver - {0}", new object[] { Description });
      Log.Info("iMONLCDg.InitializeDriver(): Called by \"{0}\".", new object[] { Assembly.GetEntryAssembly().FullName });
      var info = new FileInfo(Assembly.GetExecutingAssembly().Location);
      if (DoDebug)
      {
        Log.Info("iMONLCDg.InitializeDriver(): Assembly creation time: {0} ( {1} UTC )",
                 new object[] { info.LastWriteTime, info.LastWriteTimeUtc.ToUniversalTime() });
      }
      if (DoDebug)
      {
        Log.Info("iMONLCDg.InitializeDriver(): Platform: {0}", new object[] { Environment.OSVersion.VersionString });
      }
      LoadAdvancedSettings();
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Idle Message: {0}",
               new object[] { Settings.Instance.IdleMessage });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Delay driver startup: {0}",
               new object[] { _DelayStartup.ToString() });
      Log.Info(
        "iMONLCDg.InitializeDriver(): Advanced options - Ensure Antec/iMON Manager is running before driver startup: {0}",
        new object[] { _EnsureManagerStartup.ToString() });
      Log.Info(
        "iMONLCDg.InitializeDriver(): Advanced options - Force Antec/iMON Manager Restart after driver startup: {0}",
        new object[] { _ForceManagerRestart.ToString() });
      Log.Info(
        "iMONLCDg.InitializeDriver(): Advanced options - Force Antec/iMON Manager Reload during driver startup: {0}",
        new object[] { _ForceManagerReload.ToString() });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Restart Antec/iMON Manager FrontView on exit: {0}",
               new object[] { _RestartFrontviewOnExit.ToString() });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Leave Antec/iMON Manager FrontView active: {0}",
               new object[] { _LeaveFrontviewActive.ToString() });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Force Manager to use KeyBoard mode for iMON PAD: {0}",
               new object[] { _ForceKeyBoardMode.ToString() });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Force Display Type: {0}",
               new object[] { _ForceDisplay });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Blank display on MediaPortal exit: {0}",
               new object[] { _BlankDisplayOnExit });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Control Brightness: {0}", new object[] { _Backlight });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Control Contrast: {0}", new object[] { _Contrast });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Force Graphic Text: {0}",
               new object[] { Settings.Instance.ForceGraphicText });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Use Disk Icon: {0}",
               new object[] { DisplayOptions.DiskIcon.ToString() });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Volume Bar: {10",
               new object[] { DisplayOptions.VolumeDisplay.ToString() });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Progress Bar: {0}",
               new object[] { DisplayOptions.ProgressDisplay.ToString() });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Use Disk Icon For Media Status: {0}",
               new object[] { DisplayOptions.DiskMediaStatus.ToString() });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Use Disk Icon For CD/DVD device status: {0}",
               new object[] { DisplayOptions.DiskMonitor.ToString() });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Custom Font: {0}",
               new object[] { DisplayOptions.UseCustomFont.ToString() });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Large Icons: {0}",
               new object[] { DisplayOptions.UseLargeIcons.ToString() });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Custom Large Icons: {0}",
               new object[] { DisplayOptions.UseCustomIcons.ToString() });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Equalizer Display: {0}",
               new object[] { EQSettings.UseEqDisplay });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options -   EQMode: {0}",
               new object[] { EQSettings._useEqMode });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options -   Normal Equalizer Display: {0}",
               new object[] { EQSettings.UseNormalEq });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options -   Stereo Equalizer Display: {0}",
               new object[] { EQSettings.UseStereoEq });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options -   VU Meter Display: {0}",
               new object[] { EQSettings.UseVUmeter });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options -   VU Meter Style 2 Display: {0}",
               new object[] { EQSettings.UseVUmeter2 });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options -     Use VU Channel indicators: {0}",
               new object[] { EQSettings._useVUindicators });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options -   Restrict EQ Update Rate: {0}",
               new object[] { EQSettings.RestrictEQ });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options -     Restricted EQ Update Rate: {0} updates per second",
               new object[] { EQSettings._EQ_Restrict_FPS });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options -   Delay EQ Startup: {0}",
               new object[] { EQSettings.DelayEQ });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options -     Delay EQ Startup Time: {0} seconds",
               new object[] { EQSettings._DelayEQTime });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options -   Smooth EQ Amplitude Decay: {0}",
               new object[] { EQSettings.SmoothEQ });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options -   Show Track Info with EQ display: {0}",
               new object[] { EQSettings.EQTitleDisplay });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options -     Show Track Info Interval: {0} seconds",
               new object[] { EQSettings._EQTitleDisplayTime });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options -     Show Track Info duration: {0} seconds",
               new object[] { EQSettings._EQTitleShowTime });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Blank display with video: {0}",
               new object[] { DisplaySettings.BlankDisplayWithVideo });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options -   Enable Display on Action: {0}",
               new object[] { DisplaySettings.EnableDisplayAction });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options -     Enable display for: {0} seconds",
               new object[] { DisplaySettings.DisplayActionTime });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Monitor PowerState Events: {0}",
               new object[] { _MonitorPower });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Blank display when idle: {0}",
               new object[] { DisplaySettings.BlankDisplayWhenIdle });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options -     blank display after: {0} seconds",
               new object[] { DisplaySettings._BlankIdleTimeout / 0xf4240L });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Shutdown Message - Line 1: {0}",
               new object[] { DisplaySettings._Shutdown1 });
      Log.Info("iMONLCDg.InitializeDriver(): Advanced options - Shutdown Message - Line 2: {0}",
               new object[] { DisplaySettings._Shutdown2 });
      Log.Info("iMONLCDg.InitializeDriver(): Setting - Audio using ASIO: {0}",
               new object[] { EQSettings._AudioUseASIO });
      Log.Info("iMONLCDg.InitializeDriver(): Setting - Audio using Mixer: {0}",
               new object[] { EQSettings._AudioIsMixing });
      if (!DisplayOptions.DiskMonitor & !DisplayOptions.DiskMediaStatus)
      {
        DisplayOptions.DiskIcon = false;
      }
      if (_ForceDisplay == "LCD")
      {
        _DisplayType = DisplayType.LCD;
        Log.Info("iMONLCDg.InitializeDriver(): Advanced options forces display type to LCD");
      }
      else if (_ForceDisplay == "LCD2")
      {
        _DisplayType = DisplayType.LCD2;
        Log.Info("iMONLCDg.InitializeDriver(): Advanced options forces display type to LCD2");
      }
      else if (_ForceDisplay == "VFD")
      {
        _DisplayType = DisplayType.VFD;
        Log.Info("iMONLCDg.InitializeDriver(): Advanced options forces display type to VFD");
      }
      else if (_ForceDisplay == "LCD3R")
      {
        _DisplayType = DisplayType.VFD;
        Log.Info("iMONLCDg.InitializeDriver(): Advanced options forces display type to LCD3R");
      }
      Log.Info("iMONLCDg.InitializeDriver(): Extensive logging: {0}", new object[] { DoDebug });
      Log.Info("iMONLCDg.InitializeDriver(): Use V3 DLL for VFD: {0}", new object[] { _VFD_UseV3DLL });
      Log.Info("iMONLCDg.InitializeDriver(): Display Type: {0}", new object[] { DisplayType.TypeName(_DisplayType) });
      if (((imonVFD_DLLFile = FindImonVFDdll()) == string.Empty) & !_VFD_UseV3DLL)
      {
        Log.Info("iMONLCDg.InitializeDriver(): Failed - installed sg_vfd.dll not found - driver disabled");
        _isDisabled = true;
      }
      else
      {
        _IMON = new iMONDisplay();
        if (!_IMON.Initialize(imonVFD_DLLFile))
        {
          Log.Info("iMONLCDg.InitializeDriver(): DLL linking Failed - driver disabled");
          _isDisabled = true;
        }
        else
        {
          _isDisabled = false;
          Log.Info("iMONLCDg.InitializeDriver(): completed.");
        }
      }
    }

    private static uint LengthToPixels(int Length)
    {
      var numArray = new uint[]
                       {
                         0, 0x80, 0xc0, 0xe0, 0xf0, 0xf8, 0xfc, 0xfe, 0xff, 0x80ff, 0xc0ff, 0xe0ff, 0xf0ff, 0xf8ff,
                         0xfcff, 0xfeff,
                         0xffff, 0x80ffff, 0xc0ffff, 0xe0ffff, 0xf0ffff, 0xf8ffff, 0xfcffff, 0xfeffff, 0xffffff,
                         0x80ffffff, 0xc0ffffff, 0xe0ffffff, 0xf0ffffff, 0xf8ffffff, 0xfcffffff, 0xfeffffff,
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
      AdvSettings = AdvancedSettings.Load();
      IdleMessage = (Settings.Instance.IdleMessage != string.Empty) ? Settings.Instance.IdleMessage : "MediaPortal";
      _DelayStartup = AdvSettings.DelayStartup;
      _EnsureManagerStartup = AdvSettings.EnsureManagerStartup;
      _ForceManagerRestart = AdvSettings.ForceManagerRestart;
      _ForceManagerReload = AdvSettings.ForceManagerReload;
      _RestartFrontviewOnExit = AdvSettings.RestartFrontviewOnExit;
      _LeaveFrontviewActive = AdvSettings.LeaveFrontviewActive;
      _ForceKeyBoardMode = AdvSettings.ForceKeyBoardMode;
      _ForceDisplay = AdvSettings.DisplayType;
      if (String.IsNullOrEmpty(_ForceDisplay))
      {
        _ForceDisplay = "AutoDetect";
      }
      DisplayOptions.VolumeDisplay = AdvSettings.VolumeDisplay;
      DisplayOptions.ProgressDisplay = AdvSettings.ProgressDisplay;
      DisplayOptions.DiskIcon = AdvSettings.DiskIcon;
      DisplayOptions.DiskMediaStatus = AdvSettings.DiskMediaStatus;
      DisplayOptions.DiskMonitor = AdvSettings.DeviceMonitor;
      DisplayOptions.UseCustomFont = AdvSettings.UseCustomFont;
      DisplayOptions.UseLargeIcons = AdvSettings.UseLargeIcons;
      DisplayOptions.UseCustomIcons = AdvSettings.UseCustomIcons;
      DisplayOptions.UseInvertedIcons = AdvSettings.UseInvertedIcons;
      EQSettings.UseEqDisplay = AdvSettings.EqDisplay;
      EQSettings.UseNormalEq = AdvSettings.NormalEQ;
      EQSettings.UseStereoEq = AdvSettings.StereoEQ;
      EQSettings.UseVUmeter = AdvSettings.VUmeter;
      EQSettings.UseVUmeter2 = AdvSettings.VUmeter2;
      EQSettings._useVUindicators = AdvSettings.VUindicators;
      EQSettings._useEqMode = AdvSettings.EqMode;
      EQSettings.RestrictEQ = AdvSettings.RestrictEQ;
      EQSettings._EQ_Restrict_FPS = AdvSettings.EqRate;
      EQSettings.DelayEQ = AdvSettings.DelayEQ;
      EQSettings._DelayEQTime = AdvSettings.DelayEqTime;
      EQSettings.SmoothEQ = AdvSettings.SmoothEQ;
      EQSettings.EQTitleDisplay = AdvSettings.EQTitleDisplay;
      EQSettings._EQTitleShowTime = AdvSettings.EQTitleShowTime;
      EQSettings._EQTitleDisplayTime = AdvSettings.EQTitleDisplayTime;
      EQSettings._EqUpdateDelay = (EQSettings._EQ_Restrict_FPS == 0)
                                    ? 0
                                    : ((0x989680 / EQSettings._EQ_Restrict_FPS) -
                                       (0xf4240 / EQSettings._EQ_Restrict_FPS));
      _VFD_UseV3DLL = AdvSettings.VFD_UseV3DLL;
      _MonitorPower = AdvSettings.MonitorPowerState;
      DisplaySettings.BlankDisplayWithVideo = AdvSettings.BlankDisplayWithVideo;
      DisplaySettings.EnableDisplayAction = AdvSettings.EnableDisplayAction;
      DisplaySettings.DisplayActionTime = AdvSettings.EnableDisplayActionTime;
      DisplaySettings.BlankDisplayWhenIdle = AdvSettings.BlankDisplayWhenIdle;
      DisplaySettings.BlankIdleDelay = AdvSettings.BlankIdleTime;
      DisplaySettings._BlankIdleTimeout = DisplaySettings.BlankIdleDelay * 0x989680;
      DisplaySettings._Shutdown1 = Settings.Instance.Shutdown1;
      DisplaySettings._Shutdown2 = Settings.Instance.Shutdown2;
      DisplaySettings._DisplayControlTimeout = DisplaySettings.DisplayActionTime * 0x989680;
      var info = new FileInfo(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg.xml"));
      SettingsLastModTime = info.LastWriteTime;
      LastSettingsCheck = DateTime.Now;
    }

    private void OnExternalAction(Action action)
    {
      if (DisplaySettings.EnableDisplayAction)
      {
        if (DoDebug)
        {
          Log.Info("iMONLCDg.OnExternalAction(): received action {0}", new object[] { action.wID.ToString() });
        }
        Action.ActionType wID = action.wID;
        if (wID <= Action.ActionType.ACTION_SHOW_OSD)
        {
          if ((wID != Action.ActionType.ACTION_SHOW_INFO) && (wID != Action.ActionType.ACTION_SHOW_OSD))
          {
            return;
          }
        }
        else if (((wID != Action.ActionType.ACTION_SHOW_MPLAYER_OSD) && (wID != Action.ActionType.ACTION_KEY_PRESSED)) &&
                 (wID != Action.ActionType.ACTION_MOUSE_CLICK))
        {
          return;
        }
        DisplaySettings._DisplayControlAction = true;
        DisplaySettings._DisplayControlLastAction = DateTime.Now.Ticks;
        if (DoDebug)
        {
          Log.Info("iMONLCDg.OnExternalAction(): received DisplayControlAction");
        }
        DisplayOn();
      }
    }

    private void OpenLcd()
    {
      if (!_isDisabled)
      {
        Log.Info("iMONLCDg.OpenLcd(): called");
        if (!_IMON.iMONVFD_IsInited())
        {
          Log.Info("iMONLCDg.OpenLcd(): opening display");
          Log.Info("iMONLCDg.OpenLcd(): opening display with iMONVFD_Init({0},{1})",
                   new object[] { _VfdType.ToString("x00"), _VfdReserved.ToString("x0000") });
          if (!_IMON.iMONVFD_Init(_VfdType, _VfdReserved))
          {
            Log.Info("iMONLCDg.OpenLcd(): Could not open display with Open({0},{1})",
                     new object[] { _VfdType.ToString("x00"), _VfdReserved.ToString("x0000") });
            _isDisabled = true;
            _errorMessage = "Could not open iMON display device";
          }
          else
          {
            Log.Info("iMONLCDg.OpenLcd(): display opened");
            if (!_displayTest & ((_DisplayType == DisplayType.LCD) || (_DisplayType == DisplayType.LCD2)))
            {
              if (DisplayOptions.UseCustomFont)
              {
                CFont = new CustomFont();
                CFont.InitializeCustomFont();
              }
              if (DisplayOptions.UseLargeIcons)
              {
                CustomLargeIcon = new LargeIcon();
                CustomLargeIcon.InitializeLargeIcons();
              }
              _iconThread = new Thread(UpdateIcons)
                              {
                                IsBackground = true,
                                Priority = ThreadPriority.BelowNormal,
                                Name = "UpdateIconThread"
                              };
              _iconThread.Start();
              if (_iconThread.IsAlive)
              {
                Log.Info("iMONLCDg.OpenLcd(): iMONLCDg.UpdateIcons() Thread Started");
              }
              else
              {
                Log.Info("iMONLCDg.OpenLcd(): iMONLCDg.UpdateIcons() FAILED TO START");
              }
            }
            else if (!_displayTest & (_DisplayType == DisplayType.VFD))
            {
              if (EQSettings.UseEqDisplay || DisplaySettings.BlankDisplayWithVideo)
              {
                _iconThread = new Thread(VFD_EQ_Update)
                                {
                                  IsBackground = true,
                                  Priority = ThreadPriority.BelowNormal,
                                  Name = "VFD_EQ_Update"
                                };
                _iconThread.Start();
                if (_iconThread.IsAlive)
                {
                  Log.Info("iMONLCDg.OpenLcd(): iMONLCDg.VFD_EQ_Update() Thread Started");
                }
                else
                {
                  Log.Info("iMONLCDg.OpenLcd(): iMONLCDg.VFD_EQ_Update() FAILED TO START");
                }
              }
            }
            else if ((!_displayTest & (_DisplayType == DisplayType.ThreeRsystems)) &&
                     (EQSettings.UseEqDisplay || DisplaySettings.BlankDisplayWithVideo))
            {
              _iconThread = new Thread(VFD_EQ_Update)
                              {
                                IsBackground = true,
                                Priority = ThreadPriority.BelowNormal,
                                Name = "VFD_EQ_Update"
                              };
              _iconThread.TrySetApartmentState(ApartmentState.MTA);
              _iconThread.Start();
              if (_iconThread.IsAlive)
              {
                Log.Info("iMONLCDg.OpenLcd(): iMONLCDg.VFD_EQ_Update() Thread Started");
              }
              else
              {
                Log.Info("iMONLCDg.OpenLcd(): iMONLCDg.VFD_EQ_Update() FAILED TO START");
              }
            }
          }
        }
        else
        {
          Log.Info("iMONLCDg.OpenLcd: Display already open");
        }
        if (_MonitorPower && !_IsHandlingPowerEvent)
        {
          Log.Info("iMONLCDg.OpenLcd(): Adding Power State Monitor callback to system event thread");
          SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
        }
        lock (DWriteMutex)
        {
          if ((_DisplayType == DisplayType.LCD) || (_DisplayType == DisplayType.LCD2))
          {
            if (_DisplayType == DisplayType.LCD2)
            {
              SendData(-8646911284551352256L);
              SendData(-8502796096475496448L);
              if (_Contrast)
              {
                Log.Info("iMONLCDg.OpenLcd(): Setting LCD2 contrast level to {0}", new object[] { _ContrastLevel });
                SendData(Command.SetContrast, _ContrastLevel);
              }
              SendData(-8791026472627208192L);
              SendData(Command.SetIcons);
              SendData(Command.SetLines0);
              SendData(Command.SetLines1);
              SendData(Command.SetLines2);
              ClearDisplay();
              SendData(-8358680908399640433L);
            }
            else
            {
              SendData(Command.DisplayOn);
              SendData(Command.ClearAlarm);
              if (_Contrast)
              {
                Log.Info("iMONLCDg.OpenLcd(): Setting LCD contrast level to {0}", new object[] { _ContrastLevel });
                SendData(Command.SetContrast, _ContrastLevel);
              }
              SendData(Command.KeypadLightOn);
            }
          }
          else if (_DisplayType == DisplayType.ThreeRsystems)
          {
            SendData(0x2020202020202000L);
            SendData(0x2020202020202002L);
            SendData(0x2020202020202004L);
            SendData(0x2020202020202006L);
            SendData(0x20202020ffffff08L);
            SendData(0x21c020000000000L);
            SendData(2L);
            SendData(0x200020000000000L);
            SendData(2L);
            SendData(0x21b010000000000L);
            SendData(2L);
          }
        }
        AdvancedSettings.OnSettingsChanged +=
          AdvancedSettings_OnSettingsChanged;
        ForceManagerRestart();
        Log.Info("iMONLCDg.OpenLcd(): completed");
      }
    }

    public void RestartFrontview()
    {
      if (_RestartFrontviewOnExit)
      {
        if (!_UsingAntecManager & !_UsingSoundgraphManager)
        {
          Log.Info("iMONLCDg.RestartFrontview(): Antec/Imon Manager is not running... restart not possible",
                   new object[0]);
        }
        else
        {
          Process[] processesByName;
          Process process;
          bool flag;
          int index = _UsingSoundgraphManager ? 0 : 1;
          string curBrand = _BrandTable[index, 0];
          string curApp = _BrandTable[index, 1];

          Log.Debug("iMONLCDg.RestartFrontview(): Resetting " + curBrand + " " + curApp + " Manager registry subkey.");
          RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\" + curBrand + "\\" + curApp, true);
          if (key != null)
          {
            Log.Debug("iMONLCDg.RestartFrontview(): Restarting " + curBrand + " " + curApp + " Manager with FrontView enabled.");
            key.SetValue("RunFront", 1, RegistryValueKind.DWord);
            Registry.CurrentUser.Close();
            processesByName = Process.GetProcessesByName(curApp);
            Log.Debug("iMONLCDg.RestartFrontview(): Found {0} instances of " + curBrand + " " + curApp + " Manager",
                      new object[] { processesByName.Length });
            if (processesByName.Length > 0)
            {
              Log.Info("iMONLCDg.RestartFrontview(): Stopping " + curBrand + " " + curApp + " Manager");
              processesByName[0].Kill();
              flag = false;
              while (!flag)
              {
                Thread.Sleep(100);
                Log.Debug("iMONLCDg.RestartFrontview(): Waiting for " + curBrand + " " + curApp + " Manager to exit");
                processesByName[0].Dispose();
                processesByName = Process.GetProcessesByName(curApp);
                if (processesByName.Length == 0)
                {
                  flag = true;
                }
              }
              Log.Info("iMONLCDg.RestartFrontview(): " + curBrand + " " + curApp + " Manager Stopped");
              Win32Functions.RedrawNotificationArea();
              process = new Process
                          {
                            StartInfo =
                            {
                              WorkingDirectory = FindManagerPath(curBrand, curApp),
                              FileName = curApp + ".exe"
                            }
                          };
              Log.Info("iMONLCDg.RestartFrontview(): ReStarting " + curBrand + " " + curApp + " Manager");
              Process.Start(process.StartInfo);
            }
            else
            {
              Log.Info("iMONLCDg.RestartFrontview(): " + curBrand + " " + curApp + " Manager is not running");
            }
          }
          else
          {
            Registry.CurrentUser.Close();
            Log.Info("iMONLCDg.RestartFrontview(): " + curBrand + " " + curApp + " Registry subkey NOT FOUND. Frontview restart not possible.",
                     new object[0]);
          }
        }
        Log.Info("iMONLCDg.RestartFrontview(): completed");
      }
    }

    private void RestoreDisplayFromVideoOrIdle()
    {
      if (DisplaySettings.BlankDisplayWithVideo)
      {
        if (DisplaySettings.BlankDisplayWhenIdle)
        {
          if (!MPStatus.MP_Is_Idle)
          {
            DisplayOn();
          }
        }
        else
        {
          DisplayOn();
        }
      }
    }

    private void SendData(Command command)
    {
      SendData((ulong)command);
    }

    private void SendData(long data)
    {
      SendData((ulong)data);
    }

    private void SendData(ulong data)
    {
      try
      {
        if (DoDebug)
        {
          Log.Info("iMONLCDg.SendData(): Sending {0} to display", new object[] { data.ToString("x0000000000000000") });
        }
        if (!_IMON.iMONLCD_SendData(ref data))
        {
          SendData_Error_Count++;
          if (SendData_Error_Count > 20)
          {
            _isDisabled = true;
            if (DoDebug)
            {
              Log.Info("iMONLCDg.SendData(): ERROR Sending {0} to display",
                       new object[] { data.ToString("x0000000000000000") });
            }
            if (DoDebug)
            {
              Log.Info("iMONLCDg.SendData(): ERROR LIMIT EXCEEDED - DISPLAY DISABLED");
            }
          }
          if (DoDebug)
          {
            Log.Info("iMONLCDg.SendData(): ERROR Sending {0} to display",
                     new object[] { data.ToString("x0000000000000000") });
          }
        }
        else
        {
          SendData_Error_Count = 0;
          Thread.Sleep(_delay);
        }
      }
      catch (Exception exception)
      {
        _isDisabled = true;
        _errorMessage = exception.Message;
        Log.Info("iMONLCDg.SendData(): caught exception '{0}'\nIs your SG_VFD.dll version 5.1 or higher??",
                 new object[0]);
      }
    }

    private void SendData(Command command, ulong optionBitmask)
    {
      SendData((ulong)(command) | optionBitmask);
    }

    private void SendPixelArray(byte[] PixelArray)
    {
      if (!_IsDisplayOff)
      {
        if (PixelArray.Length > 0xc0)
        {
          Log.Error("ERROR in iMONLCDg SendPixelArray");
        }
        if (DisplayOptions.UseLargeIcons ||
            (DisplayOptions.UseLargeIcons & DisplayOptions.UseCustomIcons))
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
          if ((DisplayOptions.UseLargeIcons & !DisplayOptions.UseCustomIcons) & DoDebug)
          {
            Log.Debug("iMONLCDg.SendText(): Inserting Large Icons");
          }
          if (DisplayOptions.UseCustomIcons & DoDebug)
          {
            Log.Debug("iMONLCDg.SendText(): Inserting Custom Large Icons");
          }
          if (DisplayOptions.UseInvertedIcons & DoDebug)
          {
            Log.Debug("iMONLCDg.SendText(): Using inverted Large Icon data");
          }
          for (int k = 0; k < 0x10; k++)
          {
            if (DisplayOptions.UseCustomIcons)
            {
              PixelArray[k] = CustomLargeIcon.PixelData(_CurrentLargeIcon, k);
              PixelArray[k + 0x60] = CustomLargeIcon.PixelData(_CurrentLargeIcon, k + 0x10);
            }
            else
            {
              PixelArray[k] = _InternalLargeIcons[_CurrentLargeIcon, k];
              PixelArray[k + 0x60] = _InternalLargeIcons[_CurrentLargeIcon, k + 0x10];
            }
            if (DisplayOptions.UseInvertedIcons)
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
              SendData(data);
            }
            num4++;
          }
        }
      }
    }

    private void SendPixelArrayRaw(byte[] PixelArray)
    {
      if (!_IsDisplayOff)
      {
        if (PixelArray.Length > 0xc0)
        {
          Log.Error("ERROR in iMONLCDg SendPixelArrayRaw");
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
              SendData(data);
            }
            num++;
          }
        }
      }
    }

    private void SendText(string Line1, string Line2)
    {
      if (DoDebug)
      {
        Log.Info("iMONLCDg.SendText(): Called");
      }
      if (DisplayOptions.UseCustomFont & DoDebug)
      {
        Log.Debug("iMONLCDg.SendText(): Using CustomFont");
      }
      int num = 0;
      var pixelArray = new byte[0xc0];
      for (int i = 0; i < Math.Min(0x10, Line1.Length); i++)
      {
        char charID = Line1[i];
        for (int k = 5; k >= 0; k--)
        {
          if ((k + num) < 0x60)
          {
            pixelArray[num + k] = DisplayOptions.UseCustomFont
                                    ? BitReverse(CFont.PixelData(charID, k))
                                    : BitReverse(_Font8x5[charID, k]);
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
            pixelArray[num + m] = DisplayOptions.UseCustomFont ? BitReverse(CFont.PixelData(ch2, m)) : BitReverse(_Font8x5[ch2, m]);
          }
        }
        num += 6;
      }
      SendPixelArray(pixelArray);
    }

    private void SendText3R(string Line1)
    {
      if (DoDebug)
      {
        Log.Info("iMONLCDg.SendText3R(): Called");
      }
      var buffer = new byte[] { 13, 15, 0x20, 0x20, 0x20, 0x20, 0x20, 0, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 2 };
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
      var data =
        (ulong)
        ((((((((buffer[0] << 0x38) + (buffer[1] << 0x30)) | (buffer[2] << 40)) | (buffer[3] << 0x20)) |
            (buffer[4] << 0x18)) | (buffer[5] << 0x10)) | (buffer[6] << 8)) | buffer[7]);
      var num4 =
        (ulong)
        ((((((((buffer[8] << 0x38) + (buffer[9] << 0x30)) | (buffer[10] << 40)) | (buffer[11] << 0x20)) |
            (buffer[12] << 0x18)) | (buffer[13] << 0x10)) | (buffer[14] << 8)) | buffer[15]);
      SendData(0x200020000000000L);
      SendData(2L);
      SendData(data);
      SendData(num4);
      if (DoDebug)
      {
        Log.Info("iMONLCDg.SendText3R(): Completed");
      }
    }

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int X, int Y);

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
            SendData(data);
          }
          num++;
        }
      }
    }

    private void SetLineLength(int TopLine, int BotLine, int TopProgress, int BotProgress)
    {
      SetLinePixels(LengthToPixels(TopLine), LengthToPixels(BotLine), LengthToPixels(TopProgress),
                    LengthToPixels(BotProgress));
    }

    private void SetLinePixels(ulong TopLine, ulong BotLine, ulong TopProgress, ulong BotProgress)
    {
      lock (DWriteMutex)
      {
        ulong optionBitmask = TopProgress << 0x20;
        optionBitmask += TopLine;
        optionBitmask &= 0xffffffffffffffL;
        SendData(Command.SetLines0, optionBitmask);
        optionBitmask = TopProgress >> 0x18;
        optionBitmask += BotProgress << 8;
        optionBitmask += BotLine << 40;
        optionBitmask &= 0xffffffffffffffL;
        SendData(Command.SetLines1, optionBitmask);
        optionBitmask = BotLine >> 0x10;
        SendData(Command.SetLines2, optionBitmask);
      }
    }

    private void SetText(string Line1, string Line2)
    {
      lock (DWriteMutex)
      {
        if (DoDebug)
        {
          Log.Info("iMONLCDg.SetText(): Sending text to display");
        }
        _IMON.iMONVFD_SetText(Line1, Line2);
        Thread.Sleep(_delay);
      }
    }

    private void SetVFDClock()
    {
      const string fileName = @"\\.\SGIMON";
      if (_BlankDisplayOnExit | (_DisplayType != DisplayType.VFD))
      {
        Log.Info("iMONLCDg.SetVFDClock(): Options specify diplay blank on exit - clock not set");
      }
      else if ((DisplaySettings._Shutdown1 != string.Empty) || (DisplaySettings._Shutdown2 != string.Empty))
      {
        Log.Info("iMONLCDg.SetVFDClock(): Custom Shutdown message defined - clock not set");
      }
      else
      {
        uint bytesReturned = 0;
        DateTime now = DateTime.Now;
        var buffer3 = new byte[8];
        buffer3[7] = 0x40;
        byte[] inBuffer = buffer3;
        var buffer4 = new byte[8];
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
        IntPtr zero = Win32Functions.CreateFile(fileName, 0xc0000000, 3, IntPtr.Zero, 3, 0, IntPtr.Zero);
        if ((((int)zero) == -1) | (((int)zero) == 0))
        {
          Log.Info("iMONLCDg.SetVFDClock(): Unable to open device - clock not set");
        }
        else
        {
          Log.Info("iMONLCDg.SetVFDClock(): setting the VFD clock");
          Win32Functions.DeviceIoControl(zero, 0x222018, inBuffer, 8, null, 0, ref bytesReturned, IntPtr.Zero);
          Win32Functions.DeviceIoControl(zero, 0x222018, buffer2, 8, null, 0, ref bytesReturned, IntPtr.Zero);
          Win32Functions.CloseHandle(zero);
        }
      }
    }

    private void ShowProgressBars()
    {
      progLevel = 0;
      volLevel = 0;
      if ((MPStatus.MediaPlayer_Playing || MiniDisplayHelper.IsCaptureCardViewing()) &
          DisplayOptions.VolumeDisplay)
      {
        try
        {
          if (!MPStatus.IsMuted)
          {
            volLevel = MPStatus.SystemVolumeLevel / 2048;
          }
        }
        catch (Exception exception)
        {
          if (DoDebug)
          {
            Log.Info("iMONLCDg.ShowProgressBars(): Audio Mixer NOT available! exception: {0}", new object[] { exception });
          }
        }
      }
      if (MPStatus.MediaPlayer_Playing & DisplayOptions.ProgressDisplay)
      {
        progLevel =
          ((int)(((((float)MPStatus.Media_CurrentPosition) / ((float)MPStatus.Media_Duration)) - 0.01) * 32.0)) +
          1;
      }
      if ((LastVolLevel != volLevel) || (LastProgLevel != progLevel))
      {
        if (DoDebug)
        {
          Log.Info("iMONLCDg.ShowProgressBars(): Sending vol: {0} prog: {1} cur: {2} dur: {3} to LCD.",
                   new object[]
                     {
                       volLevel.ToString(), progLevel.ToString(),
                       MPStatus.Media_CurrentPosition.ToString(), MPStatus.Media_Duration.ToString()
                     });
        }
        SetLineLength(volLevel, progLevel, volLevel, progLevel);
      }
      LastVolLevel = volLevel;
      LastProgLevel = progLevel;
    }

    private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
      Log.Debug("iMONLCDg.SystemEvents_PowerModeChanged: SystemPowerModeChanged event was raised.");
      switch (e.Mode)
      {
        case PowerModes.Resume:
          Log.Info(
            "iMONLCDg.SystemEvents_PowerModeChanged: Resume from Suspend or Hibernation detected, restarting display",
            new object[0]);
          _IsHandlingPowerEvent = true;
          OpenLcd();
          _IsHandlingPowerEvent = false;
          break;

        case PowerModes.StatusChange:
          break;

        case PowerModes.Suspend:
          Log.Info("iMONLCDg.SystemEvents_PowerModeChanged: Suspend or Hibernation detected, shutting down display",
                   new object[0]);
          _IsHandlingPowerEvent = true;
          CloseLcd();
          _IsHandlingPowerEvent = false;
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
      var document = new XmlDocument();
      document.Load(xmlPath);
      if (document.DocumentElement != null)
      {
        return Convert.ToInt32(document.DocumentElement.SelectSingleNode("/mappings").Attributes["version"].Value);
      }
      return -1;
    }

    private void UpdateAdvancedSettings()
    {
      if (DateTime.Now.Ticks >= LastSettingsCheck.AddMinutes(1.0).Ticks)
      {
        if (DoDebug)
        {
          Log.Info("iMONLCDg.UpdateAdvancedSettings(): called");
        }
        if (File.Exists(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg.xml")))
        {
          var info = new FileInfo(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg.xml"));
          if (info.LastWriteTime.Ticks > SettingsLastModTime.Ticks)
          {
            if (DoDebug)
            {
              Log.Info("iMONLCDg.UpdateAdvancedSettings(): updating advanced settings");
            }
            LoadAdvancedSettings();
          }
        }
        if (DoDebug)
        {
          Log.Info("iMONLCDg.UpdateAdvancedSettings(): completed");
        }
      }
    }

    private void UpdateIcons()
    {
      ulong optionBitmask = 0L;
      ulong num2;
      bool flag = false;
      var icon = new DiskIcon();
      Log.Debug("iMONLCDg.UpdateIcons(): Starting Icon Update Thread");
      var mask = new BuiltinIconMask();
      var drive = new CDDrive();
      if (DisplaySettings.BlankDisplayWithVideo & DisplaySettings.EnableDisplayAction)
      {
        GUIWindowManager.OnNewAction += OnExternalAction;
      }
      for (int i = 0; i < 0x1b; i++)
      {
        Inserted_Media[i] = 0;
      }
      if (DisplayOptions.DiskIcon & DisplayOptions.DiskMonitor)
      {
        char[] cDDriveLetters = CDDrive.GetCDDriveLetters();
        var arg = new object[] { cDDriveLetters.Length.ToString() };
        Log.Debug("iMONLCDg.UpdateIcons(): Found {0} CD/DVD Drives.", arg);
        for (int j = 0; j < cDDriveLetters.Length; j++)
        {
          if (drive.Open(cDDriveLetters[j]))
          {
            Log.Debug("iMONLCDg.UpdateIcons(): Checking media in Drive {0}.",
                      new object[] { cDDriveLetters[j].ToString() });
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
              Log.Debug("iMONLCDg.UpdateIcons(): Waiting for Drive {0} to refresh.",
                        new object[] { cDDriveLetters[j].ToString() });
              drive.Refresh();
              if (drive.GetNumAudioTracks() > 0)
              {
                Inserted_Media[cDDriveLetters[j] - 'A'] = 1;
                Log.Debug("iMONLCDg.UpdateIcons(): Found Audio CD in Drive {0}.",
                          new object[] { cDDriveLetters[j].ToString() });
              }
              else if (File.Exists(cDDriveLetters[j] + @"\VIDEO_TS"))
              {
                Inserted_Media[cDDriveLetters[j] - 'A'] = 2;
                Log.Debug("iMONLCDg.UpdateIcons(): Found DVD in Drive {0}.", new object[] { cDDriveLetters[j].ToString() });
              }
              else
              {
                Inserted_Media[cDDriveLetters[j] - 'A'] = 4;
                Log.Debug("iMONLCDg.UpdateIcons(): Unknown media found in Drive {0}.",
                          new object[] { cDDriveLetters[j].ToString() });
              }
            }
            else
            {
              Inserted_Media[cDDriveLetters[j] - 'A'] = 0;
              Log.Debug("iMONLCDg.UpdateIcons(): No media found in Drive {0}.",
                        new object[] { cDDriveLetters[j].ToString() });
            }
          }
          drive.Close();
        }
      }
      if (DisplayOptions.DiskIcon & DisplayOptions.DiskMonitor)
      {
        ActivateDVM();
      }
      icon.Reset();
      while (true)
      {
        do
        {
          lock (ThreadMutex)
          {
            if (DoDebug)
            {
              Log.Info("iMONLCDg.UpdateIcons(): Checking for Thread termination request");
            }
            if (_stopUpdateIconThread)
            {
              Log.Info("iMONLCDg.UpdateIcons(): Icon Update Thread terminating");
              _stopUpdateIconThread = false;
              if (DisplaySettings.BlankDisplayWithVideo & DisplaySettings.EnableDisplayAction)
              {
                GUIWindowManager.OnNewAction -= OnExternalAction;
              }
              if (DVM != null)
              {
                DVM.Dispose();
                DVM = null;
              }
              return;
            }
            if ((!DVMactive & DisplayOptions.DiskIcon) & DisplayOptions.DiskMonitor)
            {
              ActivateDVM();
            }
            num2 = optionBitmask;
            flag = !flag;
            int num7 = _CurrentLargeIcon;
            LastVolLevel = volLevel;
            LastProgLevel = progLevel;
            int num8 = 0;
            icon.Off();
            icon.Animate();
            if (DoDebug)
            {
              Log.Info("iMONLCDg.UpdateIcons(): Checking TV Card status: IsAnyCardRecording = {0}, IsViewing = {1}",
                       new object[]
                         {
                           MiniDisplayHelper.IsCaptureCardRecording().ToString(),
                           MiniDisplayHelper.IsCaptureCardViewing().ToString()
                         });
            }
            MiniDisplayHelper.GetSystemStatus(ref MPStatus);
            Check_Idle_State();
            if (DoDebug)
            {
              Log.Info("iMONLCDg.UpdateIcons(): System Status: Plugin Status = {0}, IsIdle = {1}",
                       new object[] { MPStatus.CurrentPluginStatus.ToString(), MPStatus.MP_Is_Idle });
            }
            optionBitmask = ConvertPluginIconsToDriverIcons(MPStatus.CurrentIconMask);
            if ((optionBitmask & (0x400000000L)) > 0L)
            {
              num8 = 5;
            }
            else if ((optionBitmask & (8L)) > 0L)
            {
              num8 = 1;
            }
            if (MiniDisplayHelper.IsCaptureCardViewing() && !MPStatus.Media_IsTimeshifting)
            {
              icon.On();
              icon.InvertOn();
              icon.RotateCW();
            }
            if (_mpIsIdle)
            {
              num8 = 0;
            }
            if (MPStatus.MediaPlayer_Playing)
            {
              icon.On();
              if ((MPStatus.CurrentIconMask & (0x10L)) > 0L)
              {
                icon.InvertOff();
              }
              else
              {
                icon.InvertOn();
              }
              if ((MPStatus.CurrentIconMask & (0x10000000000L)) > 0L)
              {
                icon.RotateCCW();
              }
              else
              {
                icon.RotateCW();
              }
              icon.FlashOff();
              if (((((((MPStatus.CurrentIconMask & (0x40L)) > 0L) |
                      ((MPStatus.CurrentIconMask & (8L)) > 0L)) |
                     (MPStatus.CurrentPluginStatus == Status.PlayingDVD)) |
                    (MPStatus.CurrentPluginStatus == Status.PlayingTV)) |
                   (MPStatus.CurrentPluginStatus == Status.PlayingVideo)) |
                  (MPStatus.CurrentPluginStatus == Status.Timeshifting))
              {
                if ((MPStatus.CurrentPluginStatus == Status.PlayingTV) |
                    ((MPStatus.CurrentIconMask & (8L)) > 0L))
                {
                  num8 = 1;
                }
                else
                {
                  num8 = 2;
                }
                if (DisplaySettings.BlankDisplayWithVideo)
                {
                  DisplayOff();
                }
              }
              else
              {
                num8 = 3;
              }
              GetEQ();
            }
            else if (MPStatus.MediaPlayer_Paused)
            {
              icon.On();
              lock (DWriteMutex)
              {
                EQSettings._EqDataAvailable = false;
                _iconThread.Priority = ThreadPriority.BelowNormal;
              }
              RestoreDisplayFromVideoOrIdle();
              icon.FlashOn();
              num8 = 6;
            }
            else
            {
              icon.Off();
              RestoreDisplayFromVideoOrIdle();
              lock (DWriteMutex)
              {
                EQSettings._EqDataAvailable = false;
                _iconThread.Priority = ThreadPriority.BelowNormal;
              }
            }
            if ((!MiniDisplayHelper.Player_Playing() & !MiniDisplayHelper.IsCaptureCardViewing()) ||
                (DisplayOptions.DiskIcon & !DisplayOptions.DiskMediaStatus))
            {
              int num9 = 0;
              if (DisplayOptions.DiskIcon)
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
            if (DisplayOptions.DiskIcon & DisplayOptions.DiskMediaStatus)
            {
              optionBitmask |= icon.Mask;
            }
            if (DoDebug)
            {
              Log.Info("iMONLCDg.UpdateIcons(): last = {0}, new = {1}, disk mask = {2}",
                       new object[]
                         {
                           num2.ToString("X0000000000000000"), optionBitmask.ToString("X0000000000000000"),
                           icon.Mask.ToString("X0000000000000000")
                         });
            }
            if (optionBitmask != num2)
            {
              lock (DWriteMutex)
              {
                SendData(Command.SetIcons, optionBitmask);
              }
            }
            DisplayEQ();
            if (DisplayOptions.VolumeDisplay || DisplayOptions.ProgressDisplay)
            {
              lock (DWriteMutex)
              {
                ShowProgressBars();
              }
            }
            if (num8 != num7)
            {
              _CurrentLargeIcon = num8;
            }
          }
        } while (EQSettings._EqDataAvailable && !MPStatus.MediaPlayer_Paused);
        Thread.Sleep(200);
      }
    }

    private void VFD_EQ_Update()
    {
      if (DisplaySettings.BlankDisplayWithVideo & DisplaySettings.EnableDisplayAction)
      {
        GUIWindowManager.OnNewAction += OnExternalAction;
      }
      while (true)
      {
        object obj2;
        Monitor.Enter(obj2 = ThreadMutex);
        try
        {
          if (DoDebug)
          {
            Log.Info("iMONLCDg.VFD_EQ_Update(): Checking for Thread termination request");
          }
          if (_stopUpdateIconThread)
          {
            if (DisplaySettings.BlankDisplayWithVideo & DisplaySettings.EnableDisplayAction)
            {
              GUIWindowManager.OnNewAction -= OnExternalAction;
            }
            Log.Info("iMONLCDg.VFD_EQ_Update(): VFD_EQ_Update Thread terminating");
            _stopUpdateIconThread = false;
            break;
          }
          MiniDisplayHelper.GetSystemStatus(ref MPStatus);
          if (((!MPStatus.MediaPlayer_Active | !MPStatus.MediaPlayer_Playing) &
               DisplaySettings.BlankDisplayWithVideo) &
              (DisplaySettings.BlankDisplayWhenIdle & !_mpIsIdle))
          {
            DisplayOn();
          }
          if (MPStatus.MediaPlayer_Playing)
          {
            if (EQSettings.UseEqDisplay && (MPStatus.Media_IsCD || MPStatus.Media_IsMusic || MPStatus.Media_IsRadio))
            {
              GetEQ();
              DisplayEQ();
            }
            if (DisplaySettings.BlankDisplayWithVideo &
                (((MPStatus.Media_IsDVD || MPStatus.Media_IsVideo) || MPStatus.Media_IsTV) ||
                 MPStatus.Media_IsTVRecording))
            {
              if (DoDebug)
              {
                Log.Info("iMONLCDg.VFD_EQ_Update(): Turning off display while playing video");
              }
              DisplayOff();
            }
          }
          else
          {
            RestoreDisplayFromVideoOrIdle();
            lock (DWriteMutex)
            {
              EQSettings._EqDataAvailable = false;
              _iconThread.Priority = ThreadPriority.BelowNormal;
            }
          }
        }
        catch (Exception exception)
        {
          Log.Info("iMONLCDg.VFD_EQ_Update(): CAUGHT EXCEPTION - EXITING! - {0}", new object[] { exception });
          break;
        }
        finally
        {
          Monitor.Exit(obj2);
        }
        if (!EQSettings._EqDataAvailable || MPStatus.MediaPlayer_Paused)
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
        Log.Info("iMONLCDg.UpdateDisplay.VolumeInserted(): volume inserted in drive {0}", new object[] { str });
      }
      var drive = new CDDrive();
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
          Log.Info("iMONLCDg.UpdateDisplay.VolumeInserted(): Audio CD inserted in drive {0}", new object[] { str });
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
            Log.Info("iMONLCDg.UpdateDisplay.VolumeInserted(): DVD inserted in drive {0}", new object[] { str });
          }
        }
        else
        {
          Inserted_Media[str[0] - 'A'] = 4;
          if (Settings.Instance.ExtensiveLogging)
          {
            Log.Info("iMONLCDg.UpdateDisplay.VolumeInserted(): Unknown Media inserted in drive {0}", new object[] { str });
          }
        }
      }
    }

    private static void VolumeRemoved(int bitMask)
    {
      string str = DVM.MaskToLogicalPaths(bitMask);
      if (Settings.Instance.ExtensiveLogging)
      {
        Log.Info("iMONLCDg.UpdateDisplay.VolumeRemoved(): volume removed from drive {0}", new object[] { str });
      }
      Inserted_Media[str[0] - 'A'] = 0;
    }

    #region Nested type: AdvancedSettings

    [Serializable]
    public class AdvancedSettings
    {
      #region Delegates

      public delegate void OnSettingsChangedHandler();

      #endregion

      private static readonly bool DoDebug = Settings.Instance.ExtensiveLogging;
      private static AdvancedSettings m_Instance;
      private int m_BlankIdleTime = 30;
      private int m_DelayEqTime = 10;
      private int m_EnableDisplayActionTime = 5;
      private int m_EqRate = 10;
      private int m_EQTitleDisplayTime = 10;
      private int m_EQTitleShowTime = 2;
      private bool m_NormalEQ = true;

      [XmlAttribute]
      public bool BlankDisplayWhenIdle { get; set; }

      [XmlAttribute]
      public bool BlankDisplayWithVideo { get; set; }

      [XmlAttribute]
      public int BlankIdleTime
      {
        get { return m_BlankIdleTime; }
        set { m_BlankIdleTime = value; }
      }

      [XmlAttribute]
      public bool DelayEQ { get; set; }

      [XmlAttribute]
      public int DelayEqTime
      {
        get { return m_DelayEqTime; }
        set { m_DelayEqTime = value; }
      }

      [XmlAttribute]
      public bool DelayStartup { get; set; }

      [XmlAttribute]
      public bool DeviceMonitor { get; set; }

      [XmlAttribute]
      public bool DiskIcon { get; set; }

      [XmlAttribute]
      public bool DiskMediaStatus { get; set; }

      [XmlAttribute]
      public string DisplayType { get; set; }

      [XmlAttribute]
      public bool EnableDisplayAction { get; set; }

      [XmlAttribute]
      public int EnableDisplayActionTime
      {
        get { return m_EnableDisplayActionTime; }
        set { m_EnableDisplayActionTime = value; }
      }

      [XmlAttribute]
      public bool EnsureManagerStartup { get; set; }

      [XmlAttribute]
      public bool EqDisplay { get; set; }

      [XmlAttribute]
      public int EqMode { get; set; }

      [XmlAttribute]
      public int EqRate
      {
        get { return m_EqRate; }
        set { m_EqRate = value; }
      }

      [XmlAttribute]
      public bool EQTitleDisplay { get; set; }

      [XmlAttribute]
      public int EQTitleDisplayTime
      {
        get { return m_EQTitleDisplayTime; }
        set { m_EQTitleDisplayTime = value; }
      }

      [XmlAttribute]
      public int EQTitleShowTime
      {
        get { return m_EQTitleShowTime; }
        set { m_EQTitleShowTime = value; }
      }

      [XmlAttribute]
      public bool ForceKeyBoardMode { get; set; }

      [XmlAttribute]
      public bool ForceManagerRestart { get; set; }

      [XmlAttribute]
      public bool ForceManagerReload { get; set; }

      public static AdvancedSettings Instance
      {
        get
        {
          if (m_Instance == null)
          {
            m_Instance = Load();
          }
          return m_Instance;
        }
        set { m_Instance = value; }
      }

      [XmlAttribute]
      public bool MonitorPowerState { get; set; }

      [XmlAttribute]
      public bool NormalEQ
      {
        get { return m_NormalEQ; }
        set { m_NormalEQ = value; }
      }

      [XmlAttribute]
      public bool ProgressDisplay { get; set; }

      [XmlAttribute]
      public bool RestartFrontviewOnExit { get; set; }

      [XmlAttribute]
      public bool LeaveFrontviewActive { get; set; }

      [XmlAttribute]
      public bool RestrictEQ { get; set; }

      [XmlAttribute]
      public bool SmoothEQ { get; set; }

      [XmlAttribute]
      public bool StereoEQ { get; set; }

      [XmlAttribute]
      public bool UseCustomFont { get; set; }

      [XmlAttribute]
      public bool UseCustomIcons { get; set; }

      [XmlAttribute]
      public bool UseInvertedIcons { get; set; }

      [XmlAttribute]
      public bool UseLargeIcons { get; set; }

      [XmlAttribute]
      public bool UseRC { get; set; }

      [XmlAttribute]
      public bool VFD_UseV3DLL { get; set; }

      [XmlAttribute]
      public bool VolumeDisplay { get; set; }

      [XmlAttribute]
      public bool VUindicators { get; set; }

      [XmlAttribute]
      public bool VUmeter { get; set; }

      [XmlAttribute]
      public bool VUmeter2 { get; set; }

      public static event OnSettingsChangedHandler OnSettingsChanged;

      private static void Default(AdvancedSettings _settings)
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
        _settings.LeaveFrontviewActive = false;
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
      }

      public static AdvancedSettings Load()
      {
        AdvancedSettings settings;
        Log.Info("iMONLCDg.AdvancedSettings.Load(): started");
        if (File.Exists(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg.xml")))
        {
          Log.Info("iMONLCDg.AdvancedSettings.Load(): Loading settings from XML file");
          var serializer = new XmlSerializer(typeof(AdvancedSettings));
          var xmlReader = new XmlTextReader(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg.xml"));
          settings = (AdvancedSettings)serializer.Deserialize(xmlReader);
          xmlReader.Close();
        }
        else
        {
          Log.Info("iMONLCDg.AdvancedSettings.Load(): Loading settings from defaults");
          settings = new AdvancedSettings();
          Default(settings);
          Log.Info("iMONLCDg.AdvancedSettings.Load(): Loaded settings from defaults");
        }
        Log.Info("iMONLCDg.AdvancedSettings.Load(): completed");
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

      public static void Save(AdvancedSettings ToSave)
      {
        if (DoDebug)
        {
          Log.Info("iMONLCDg.AdvancedSettings.Save(): Saving settings to XML file");
        }
        var serializer = new XmlSerializer(typeof(AdvancedSettings));
        var writer = new XmlTextWriter(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg.xml"),
                                       Encoding.UTF8) { Formatting = Formatting.Indented, Indentation = 2 };
        serializer.Serialize(writer, ToSave);
        writer.Close();
        if (DoDebug)
        {
          Log.Info("iMONLCDg.AdvancedSettings.Save(): completed");
        }
      }

      public static void SetDefaults()
      {
        Default(Instance);
      }
    }

    #endregion

    #region Nested type: BuiltinIconMask

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

    #endregion

    #region Nested type: Command

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

    #endregion

    #region Nested type: CustomFont

    private class CustomFont
    {
      private static byte[,] CstmFont;
      private readonly DataColumn CData0 = new DataColumn("CData0");
      private readonly DataColumn CData1 = new DataColumn("CData1");
      private readonly DataColumn CData2 = new DataColumn("CData2");
      private readonly DataColumn CData3 = new DataColumn("CData3");
      private readonly DataColumn CData4 = new DataColumn("CData4");
      private readonly DataColumn CData5 = new DataColumn("CData5");
      private readonly DataColumn CID = new DataColumn("CharID");
      private DisplayOptions CustomOptions = XMLUTILS.LoadDisplayOptionsSettings();
      private DataTable FontData = new DataTable("Character");

      public void CloseFont()
      {
        if (FontData != null)
        {
          FontData.Dispose();
        }
      }

      public void InitializeCustomFont()
      {
        if (CustomOptions.UseCustomFont)
        {
          if (FontData.Columns.Count == 0)
          {
            FontData.Rows.Clear();
            FontData.Columns.Clear();
            CstmFont = new byte[0x100, 6];
            CID.DataType = typeof(byte);
            FontData.Columns.Add(CID);
            CData0.DataType = typeof(byte);
            FontData.Columns.Add(CData0);
            CData1.DataType = typeof(byte);
            FontData.Columns.Add(CData1);
            CData2.DataType = typeof(byte);
            FontData.Columns.Add(CData2);
            CData3.DataType = typeof(byte);
            FontData.Columns.Add(CData3);
            CData4.DataType = typeof(byte);
            FontData.Columns.Add(CData4);
            CData5.DataType = typeof(byte);
            FontData.Columns.Add(CData5);
            FontData.Clear();
          }
          if (LoadCustomFontData())
          {
            Log.Debug("iMONLCDg.InitializeCustomFont(): Custom font data loaded");
          }
          else
          {
            SaveDefaultFontData();
            Log.Debug(
              "iMONLCDg.InitializeCustomFont(): Custom font file not found. Template file saved. loaded default file.",
              new object[0]);
          }
        }
      }

      private bool LoadCustomFontData()
      {
        Log.Debug("LoadCustomFontData(): called");
        if (File.Exists(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg_font.xml")))
        {
          FontData.Rows.Clear();
          var serializer = new XmlSerializer(typeof(DataTable));
          var xmlReader = new XmlTextReader(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg_font.xml"));
          Log.Debug("LoadCustomFontData(): DeSerializing data");
          FontData = (DataTable)serializer.Deserialize(xmlReader);
          Log.Debug("LoadCustomFontData(): Read data from file");
          xmlReader.Close();
          Log.Debug("LoadCustomFontData(): Converting font data");
          for (int j = 0; j < 0x100; j++)
          {
            DataRow row = FontData.Rows[j];
            CstmFont[j, 0] = (byte)row[1];
            CstmFont[j, 1] = (byte)row[2];
            CstmFont[j, 2] = (byte)row[3];
            CstmFont[j, 3] = (byte)row[4];
            CstmFont[j, 4] = (byte)row[5];
            CstmFont[j, 5] = (byte)row[6];
          }
          Log.Debug("LoadCustomFontData(): completed");
          return true;
        }
        Log.Debug("LoadCustomFontData(): Loading Custom Font from default Font");
        for (int i = 0; i < 0x100; i++)
        {
          for (int k = 0; k < 6; k++)
          {
            CstmFont[i, k] = _Font8x5[i, k];
          }
        }
        Log.Debug("LoadCustomFontData(): completed");
        return false;
      }

      public byte PixelData(int CharID, int CharIndex)
      {
        return CstmFont[CharID, CharIndex];
      }

      private void SaveDefaultFontData()
      {
        Log.Debug("SaveFontData(): called");
        Log.Debug("SaveFontData(): Converting font data");
        FontData.Rows.Clear();
        for (int i = 0; i < 0x100; i++)
        {
          DataRow row = FontData.NewRow();
          row[0] = i;
          row[1] = _Font8x5[i, 0];
          row[2] = _Font8x5[i, 1];
          row[3] = _Font8x5[i, 2];
          row[4] = _Font8x5[i, 3];
          row[5] = _Font8x5[i, 4];
          row[6] = _Font8x5[i, 5];
          FontData.Rows.Add(row);
        }
        var serializer = new XmlSerializer(typeof(DataTable));
        TextWriter textWriter = new StreamWriter(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg_font.xml"));
        Log.Debug("SaveFontData(): Serializing data");
        serializer.Serialize(textWriter, FontData);
        Log.Debug("SaveFontData(): Writing data to file");
        textWriter.Close();
        Log.Debug("SaveFontData(): completed");
      }
    }

    #endregion

    #region Nested type: DiskIcon

    public class DiskIcon
    {
      private readonly ulong[] _DiskMask = new ulong[]
                                             {
                                               0x80fe0000000000L, 0x80fd0000000000L, 0x80fb0000000000L,
                                               0x80f70000000000L
                                               , 0x80ef0000000000L, 0x80df0000000000L, 0x80bf0000000000L,
                                               0x807f0000000000L
                                             };

      private readonly ulong[] _DiskMaskInv = new ulong[]
                                                {
                                                  0x80010000000000L, 0x80020000000000L, 0x80040000000000L,
                                                  0x80080000000000L, 0x80100000000000L, 0x80200000000000L,
                                                  0x80400000000000L, 0x80800000000000L
                                                };

      private const ulong _diskSolidOffMask = 0L;
      private const ulong _diskSolidOnMask = 0x80ff0000000000L;
      private bool _diskFlash;
      private bool _diskInverted;

      private bool _diskOn;
      private bool _diskRotate;
      private bool _diskRotateClockwise = true;
      private int _diskSegment;
      private bool _diskSRWFlash = true;
      private int _flashState = 1;
      private DateTime _LastAnimate;

      public bool IsFlashing
      {
        get { return _diskFlash; }
      }

      public bool IsInverted
      {
        get { return _diskInverted; }
      }

      public bool IsOn
      {
        get { return _diskOn; }
      }

      public bool IsRotating
      {
        get { return _diskFlash; }
      }

      public ulong Mask
      {
        get
        {
          Log.Info("ON: {0}, flashing: {1}, FLASHSTATE : {2}, Rotate: {3}, Invert: {4}",
                   new object[] { _diskOn, _diskFlash, _flashState.ToString(), _diskRotate, _diskInverted });
          if (!_diskOn)
          {
            return _diskSolidOffMask;
          }
          if (!_diskRotate)
          {
            if (!_diskFlash)
            {
              return _diskSolidOnMask;
            }
            if (_flashState == 1)
            {
              return _diskSolidOnMask;
            }
            return _diskSolidOffMask;
          }
          if (!_diskFlash)
          {
            if (!_diskInverted)
            {
              return _DiskMask[_diskSegment];
            }
            return _DiskMaskInv[_diskSegment];
          }
          if (_flashState <= 0)
          {
            return _diskSolidOffMask;
          }
          if (!_diskInverted)
          {
            return _DiskMask[_diskSegment];
          }
          return _DiskMaskInv[_diskSegment];
        }
      }

      public void Animate()
      {
        if ((DateTime.Now.Ticks - _LastAnimate.Ticks) >= 0x7a120L)
        {
          if ((_diskRotate & !_diskFlash) || (_diskRotate & (_diskFlash & !_diskSRWFlash)))
          {
            if (_diskRotateClockwise)
            {
              _diskSegment++;
              if (_diskSegment > 7)
              {
                _diskSegment = 0;
              }
            }
            else
            {
              _diskSegment--;
              if (_diskSegment < 0)
              {
                _diskSegment = 7;
              }
            }
          }
          if (_diskFlash)
          {
            _flashState = _flashState == 1 ? 0 : 1;
          }
          _LastAnimate = DateTime.Now;
        }
      }

      public void FlashOff()
      {
        _diskFlash = false;
        _flashState = 1;
      }

      public void FlashOn()
      {
        _diskFlash = true;
      }

      public void InvertOff()
      {
        _diskInverted = false;
      }

      public void InvertOn()
      {
        _diskInverted = true;
      }

      public void Off()
      {
        _diskOn = false;
      }

      public void On()
      {
        if (Settings.Instance.ExtensiveLogging)
        {
          Log.Info("DISK ON CALLED");
        }
        _diskOn = true;
      }

      public void Reset()
      {
        _diskFlash = false;
        _diskRotate = false;
        _diskSegment = 0;
        _diskRotateClockwise = true;
        _diskOn = false;
        _flashState = 1;
        _diskInverted = false;
        _diskSRWFlash = true;
      }

      public void RotateCCW()
      {
        _diskRotateClockwise = false;
        _diskRotate = true;
      }

      public void RotateCW()
      {
        _diskRotateClockwise = true;
        _diskRotate = true;
      }

      public void RotateOff()
      {
        _diskRotateClockwise = false;
        _diskRotate = false;
      }

      public void SRWFlashOff()
      {
        _diskSRWFlash = false;
      }

      public void SRWFlashOn()
      {
        _diskSRWFlash = true;
      }
    }

    #endregion

    #region Nested type: DisplayType

    private class DisplayType
    {
      public static int LCD
      {
        get { return 1; }
      }

      public static int LCD2
      {
        get { return 4; }
      }

      public static int ThreeRsystems
      {
        get { return 3; }
      }

      public static int Unsupported
      {
        get { return 2; }
      }

      public static int VFD
      {
        get { return 0; }
      }

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
    }

    #endregion

    #region Nested type: LargeIcon

    private class LargeIcon
    {
      private static byte[,] CustomIcons;
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
      private readonly DataColumn LIID = new DataColumn("IconID");
      private DisplayOptions CustomOptions = XMLUTILS.LoadDisplayOptionsSettings();
      private DataTable LIconData = new DataTable("LargeIcons");

      public void CloseIcons()
      {
        if (LIconData != null)
        {
          LIconData.Dispose();
        }
      }

      public void InitializeLargeIcons()
      {
        if (CustomOptions.UseLargeIcons || CustomOptions.UseCustomIcons)
        {
          Log.Debug("iMONLCDg.InitializeLargeIcons(): Using Large Icons.");
          if (!CustomOptions.UseCustomIcons)
          {
            Log.Debug("iMONLCDg.InitializeLargeIcons(): Using Internal Large Icon Data.");
          }
          else
          {
            Log.Debug("iMONLCDg.InitializeLargeIcons(): Using Custom Large Icon Data.");
            if (LIconData.Columns.Count == 0)
            {
              LIconData.Rows.Clear();
              LIconData.Columns.Clear();
              CustomIcons = new byte[10, 0x20];
              LIID.DataType = typeof(byte);
              LIconData.Columns.Add(LIID);
              IData0.DataType = typeof(byte);
              LIconData.Columns.Add(IData0);
              IData1.DataType = typeof(byte);
              LIconData.Columns.Add(IData1);
              IData2.DataType = typeof(byte);
              LIconData.Columns.Add(IData2);
              IData3.DataType = typeof(byte);
              LIconData.Columns.Add(IData3);
              IData4.DataType = typeof(byte);
              LIconData.Columns.Add(IData4);
              IData5.DataType = typeof(byte);
              LIconData.Columns.Add(IData5);
              IData6.DataType = typeof(byte);
              LIconData.Columns.Add(IData6);
              IData7.DataType = typeof(byte);
              LIconData.Columns.Add(IData7);
              IData8.DataType = typeof(byte);
              LIconData.Columns.Add(IData8);
              IData9.DataType = typeof(byte);
              LIconData.Columns.Add(IData9);
              IData10.DataType = typeof(byte);
              LIconData.Columns.Add(IData10);
              IData11.DataType = typeof(byte);
              LIconData.Columns.Add(IData11);
              IData12.DataType = typeof(byte);
              LIconData.Columns.Add(IData12);
              IData13.DataType = typeof(byte);
              LIconData.Columns.Add(IData13);
              IData14.DataType = typeof(byte);
              LIconData.Columns.Add(IData14);
              IData15.DataType = typeof(byte);
              LIconData.Columns.Add(IData15);
              IData16.DataType = typeof(byte);
              LIconData.Columns.Add(IData16);
              IData17.DataType = typeof(byte);
              LIconData.Columns.Add(IData17);
              IData18.DataType = typeof(byte);
              LIconData.Columns.Add(IData18);
              IData19.DataType = typeof(byte);
              LIconData.Columns.Add(IData19);
              IData20.DataType = typeof(byte);
              LIconData.Columns.Add(IData20);
              IData21.DataType = typeof(byte);
              LIconData.Columns.Add(IData21);
              IData22.DataType = typeof(byte);
              LIconData.Columns.Add(IData22);
              IData23.DataType = typeof(byte);
              LIconData.Columns.Add(IData23);
              IData24.DataType = typeof(byte);
              LIconData.Columns.Add(IData24);
              IData25.DataType = typeof(byte);
              LIconData.Columns.Add(IData25);
              IData26.DataType = typeof(byte);
              LIconData.Columns.Add(IData26);
              IData27.DataType = typeof(byte);
              LIconData.Columns.Add(IData27);
              IData28.DataType = typeof(byte);
              LIconData.Columns.Add(IData28);
              IData29.DataType = typeof(byte);
              LIconData.Columns.Add(IData29);
              IData30.DataType = typeof(byte);
              LIconData.Columns.Add(IData30);
              IData31.DataType = typeof(byte);
              LIconData.Columns.Add(IData31);
              LIconData.Clear();
            }
            if (LoadLargeIconData())
            {
              Log.Debug("iMONLCDg.InitializeLargeIcons(): Custom Large Icon data loaded");
            }
            else
            {
              SaveDefaultLargeIconData();
              Log.Debug(
                "iMONLCDg.InitializeLargeIcons(): Custom Large Icon file not found. Template file saved. loaded default data.",
                new object[0]);
            }
          }
        }
      }

      private bool LoadLargeIconData()
      {
        Log.Debug("LoadLargeIconData(): called");
        if (File.Exists(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg_icons.xml")))
        {
          LIconData.Rows.Clear();
          var serializer = new XmlSerializer(typeof(DataTable));
          var xmlReader =
            new XmlTextReader(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg_icons.xml"));
          Log.Debug("LoadLargeIconData(): DeSerializing data");
          LIconData = (DataTable)serializer.Deserialize(xmlReader);
          Log.Debug("LoadLargeIconData(): Read data from file");
          xmlReader.Close();
          Log.Debug("LoadLargeIconData(): Converting icon data");
          for (int j = 0; j < 10; j++)
          {
            DataRow row = LIconData.Rows[j];
            for (int k = 1; k < 0x21; k++)
            {
              CustomIcons[j, k - 1] = (byte)row[k];
            }
          }
          Log.Debug("LoadLargeIconData(): completed");
          return true;
        }
        Log.Debug("LoadLargeIconData(): Loading Custom Large Icons from default Large Icons");
        for (int i = 0; i < 10; i++)
        {
          for (int m = 0; m < 0x20; m++)
          {
            CustomIcons[i, m] = _InternalLargeIcons[i, m];
          }
        }
        Log.Debug("LoadLargeIconData(): completed");
        return false;
      }

      public byte PixelData(int IconID, int ByteIndex)
      {
        return CustomIcons[IconID, ByteIndex];
      }

      private void SaveDefaultLargeIconData()
      {
        Log.Debug("SaveDefaultLargeIconData(): called");
        Log.Debug("SaveDefaultLargeIconData(): Converting icon data");
        LIconData.Rows.Clear();
        for (int i = 0; i < 10; i++)
        {
          DataRow row = LIconData.NewRow();
          row[0] = i;
          for (int j = 1; j < 0x21; j++)
          {
            row[j] = _InternalLargeIcons[i, j - 1];
          }
          LIconData.Rows.Add(row);
        }
        var serializer = new XmlSerializer(typeof(DataTable));
        TextWriter textWriter = new StreamWriter(Config.GetFile(Config.Dir.Config, "MiniDisplay_imonlcdg_icons.xml"));
        Log.Debug("SaveDefaultLargeIconData(): Serializing data");
        serializer.Serialize(textWriter, LIconData);
        Log.Debug("SaveDefaultLargeIconData(): Writing data to file");
        textWriter.Close();
        Log.Debug("SaveDefaultLargeIconData(): completed");
      }
    }

    #endregion

  }
}
