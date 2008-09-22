#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;

namespace ProcessPlugins.ExternalDisplay.Drivers
{
  /// <summary>Wrapper driver for all LCDHype drivers</summary>
  /// <remarks>This class uses the .NET Reflection.Emit API to dynamically create a type with static methods
  /// that map to the methods the passed LCDHype driver DLL exposes.  The class then uses Reflection to 
  /// invoke these methods.</remarks>
  /// <author>JoeDalton</author>
  public class LCDHypeWrapper : BaseDisplay, IDisplay
  {
    private const int MAX_RESPIXELS = 76800;

    private const MethodAttributes METHOD_ATTRIBUTES =
      MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.PinvokeImpl |
      MethodAttributes.HideBySig;

    private const BindingFlags BINDING_FLAGS = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public;
    private string dllFile;
    private static ModuleBuilder s_mb;
    private Type m_tDllReg;
    private DLLInfo info;
    private string name;
    private bool isDisabled = false;
    private string errorMessage = "";
    private Bitmap lastBitmap;
    private byte[] bytes = new byte[MAX_RESPIXELS];

    public LCDHypeWrapper(string dllFile)
    {
      try
      {
        this.dllFile = dllFile;
        string[] tmp = dllFile.Split('/', '.', '\\');
        name = tmp[tmp.Length - 2];
        //if (!ExternalDisplay.VerifyDriverLinxDriver())
        //    return;
        CreateLCDHypeWrapper();
        GetDllInfo();
      }
      catch (TargetInvocationException ex)
      {
        isDisabled = true;
        Exception innerException = ex.InnerException;
        if (innerException != null)
        {
          errorMessage = innerException.Message;
        }
        else
        {
          errorMessage = ex.Message;
        }
        Log.Error("ExternalDisplay:Error while loading driver {0}: {1}", dllFile, errorMessage);
      }
    }

    public bool IsDisabled
    {
      get { return isDisabled; }
    }

    public string ErrorMessage
    {
      get { return errorMessage; }
    }

    public void SetCustomCharacters(int[][] customCharacters)
    {
      for (int i = 0; i < customCharacters.GetLength(0); i++)
      {
        //object res =
        //    m_tDllReg.InvokeMember("LCD_GetCGRAMChar", BINDING_FLAGS, null, null, new object[] {(byte) i});
        //byte pos = (byte) res;
        CharacterData data = new CharacterData();
        data.Position = (byte) i;
        data.SetData(customCharacters[i]);
        m_tDllReg.InvokeMember("LCD_SetCGRAMChar", BINDING_FLAGS, null, null, new object[] {data});
      }
    }

    public void DrawImage(Bitmap bitmap)
    {
      if (!SupportsGraphics)
      {
        return;
      }
      if (bitmap == lastBitmap)
      {
        return;
      }
      //clear the array
      Array.Clear(bytes, 0, bytes.Length);
      //a rectangle of the size of the bitmap
      Rectangle l_Size = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
      //get the bitmap data
      BitmapData l_Data = bitmap.LockBits(l_Size, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
      int bpp = l_Data.Stride/l_Data.Width;
      //create a source buffer for the bitmap data (RGB 24 bits)
      byte[] l_DestBuffer = new byte[l_Data.Stride*l_Size.Height];
      //copy the data buffer to the destination buffer
      Marshal.Copy(l_Data.Scan0, l_DestBuffer, 0, l_DestBuffer.Length);
      bitmap.UnlockBits(l_Data);
      for (int j = 0; j < l_Size.Height; j++)
      {
        for (int i = 0; i < l_Size.Width; i++)
        {
          //test if current pixel must be enabled or not
          int pixel = i*bpp + j*l_Data.Stride;
          if (Color.FromArgb(l_DestBuffer[pixel + 2],
                             l_DestBuffer[pixel + 1],
                             l_DestBuffer[pixel]).GetBrightness() < 0.5f)
          {
            bytes[i + j*l_Size.Width] = 1;
          }
        }
      }
      m_tDllReg.InvokeMember("LCD_SendToGfxMemory", BINDING_FLAGS, null, null,
                             new object[] {bytes, 0, 0, l_Size.Width - 1, l_Size.Height - 1, false});
      lastBitmap = bitmap;
    }

    public void Configure()
    {
      try
      {
        m_tDllReg.InvokeMember("LCD_ConfigDialog", BINDING_FLAGS, null, null, null);
      }
      catch (TargetInvocationException ex)
      {
        if (ex.InnerException is EntryPointNotFoundException)
        {
          return;
        }
        throw;
      }
    }

    #region IDisplay Members

    public string Name
    {
      get { return name; }
    }

    private string m_Description = null;

    public string Description
    {
      get
      {
        if (m_Description == null)
        {
          if (info.IDArray == null || isDisabled)
          {
            return Name + " (disabled)";
          }
          int i = 0;
          for (; i < 256 && info.IDArray[i] != 0; i++)
          {
          }
          m_Description = new string(info.IDArray, 0, i);
        }
        return m_Description;
      }
    }

    public bool SupportsText
    {
      get { return info.SupportTxtLCD; }
    }

    public bool SupportsGraphics
    {
      get { return info.SupportGfxLCD; }
    }

    public void Setup(string _port, int _lines, int _cols, int _time, int _linesG, int _colsG, int _timeG,
                      bool _backLight, int _contrast)
    {
      //if (!SupportsGraphics)
      //{
      //  _linesG = 0;
      //  _colsG = 0;
      //  _timeG = 0;
      //}

      m_tDllReg.InvokeMember("LCD_SetIOPropertys", BINDING_FLAGS, null, null,
                             new object[]
                               {
                                 _port, _time, _timeG, _cols, _lines, _colsG, _linesG, _backLight, (byte) 127,
                                 info.SupportContrastSlider, (byte) _contrast, 0, false, false
                               });
    }

    public void Cleanup()
    {
      m_tDllReg.InvokeMember("LCD_CleanUp", BINDING_FLAGS, null, null, null);
    }

    /// <summary>
    /// Shows the given message on the indicated line.
    /// </summary>
    /// <param name="_line">The line to thow the message on.</param>
    /// <param name="_message">The message to show.</param>
    public void SetLine(int _line, string _message)
    {
      SetPosition(0, _line);
      SendText(_message);
    }

    protected void SetPosition(int x, int y)
    {
      m_tDllReg.InvokeMember("LCD_SetOutputAddress", BINDING_FLAGS, null, null, new object[] {x, y});
    }

    private void SendText(string _text)
    {
      for (int i = 0; i < _text.Length; i++)
      {
        byte c = (byte) _text[i];
        if (c < 32)
        {
          c = (byte) m_tDllReg.InvokeMember("LCD_GetCGRAMChar", BINDING_FLAGS, null, null, new object[] {c});
        }
        m_tDllReg.InvokeMember("LCD_SendToMemory", BINDING_FLAGS, null, null, new object[] {c});
      }
    }

    public void Initialize()
    {
      m_tDllReg.InvokeMember("LCD_Init", BINDING_FLAGS, null, null, null);
      lastBitmap = null;
    }

    public void CleanUp()
    {
      m_tDllReg.InvokeMember("LCD_CleanUp", BINDING_FLAGS, null, null, null);
    }

    #endregion

    /// <summary>
    /// Loads the driver information structure, <see cref="DLLInfo"/>
    /// </summary>
    /// <exception cref="TargetInvocationException">when the DLL_GetInfo call throws an exception.</exception>
    public void GetDllInfo()
    {
      object[] p = new object[1];
      m_tDllReg.InvokeMember("DLL_GetInfo", BINDING_FLAGS, null, null, p);
      info = (DLLInfo) p[0];
    }

    public bool IsReadyToReceive()
    {
      return (bool) m_tDllReg.InvokeMember("LCD_IsReadyToReceive", BINDING_FLAGS, null, null, null);
    }

    private void CreateLCDHypeWrapper()
    {
      if (s_mb == null)
      {
        // Create dynamic assembly    
        AssemblyName an = new AssemblyName();
        an.Name = "LCDHypeWrapper" + Guid.NewGuid().ToString("N");
        AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);

        // Add module to assembly
        s_mb = ab.DefineDynamicModule("LCDDriverModule");
      }

      // Add class to module
      TypeBuilder tb = s_mb.DefineType(name + Guid.NewGuid().ToString("N"));

      MethodBuilder meb;

      //DLL_GetInfo
      meb = tb.DefinePInvokeMethod("DLL_GetInfo", dllFile, METHOD_ATTRIBUTES,
                                   CallingConventions.Standard, typeof (void),
                                   new Type[]
                                     {
                                       Type.GetType(
                                         "ProcessPlugins.ExternalDisplay.Drivers.LCDHypeWrapper+DLLInfo&")
                                     },
                                   CallingConvention.StdCall, CharSet.Auto);
      meb.DefineParameter(1, ParameterAttributes.Out, "_info");
      // Apply preservesig metadata attribute so we can handle return HRESULT ourselves
      meb.SetImplementationFlags(MethodImplAttributes.PreserveSig | meb.GetMethodImplementationFlags());

      //LCD_IsReadyToReceive
      meb = tb.DefinePInvokeMethod("LCD_IsReadyToReceive", dllFile, METHOD_ATTRIBUTES,
                                   CallingConventions.Standard, typeof (bool), null, CallingConvention.StdCall,
                                   CharSet.Auto);
      // Apply preservesig metadata attribute so we can handle return HRESULT ourselves
      meb.SetImplementationFlags(MethodImplAttributes.PreserveSig | meb.GetMethodImplementationFlags());

      //LCD_Init
      meb = tb.DefinePInvokeMethod("LCD_Init", dllFile, METHOD_ATTRIBUTES,
                                   CallingConventions.Standard, typeof (void), null, CallingConvention.StdCall,
                                   CharSet.Auto);
      // Apply preservesig metadata attribute so we can handle return HRESULT ourselves
      meb.SetImplementationFlags(MethodImplAttributes.PreserveSig | meb.GetMethodImplementationFlags());

      //LCD_ConfigDialog
      meb = tb.DefinePInvokeMethod("LCD_ConfigDialog", dllFile, METHOD_ATTRIBUTES,
                                   CallingConventions.Standard, typeof (void), null, CallingConvention.StdCall,
                                   CharSet.Auto);
      // Apply preservesig metadata attribute so we can handle return HRESULT ourselves
      meb.SetImplementationFlags(MethodImplAttributes.PreserveSig | meb.GetMethodImplementationFlags());

      //LCD_CleanUp
      meb = tb.DefinePInvokeMethod("LCD_CleanUp", dllFile, METHOD_ATTRIBUTES,
                                   CallingConventions.Standard, typeof (void), null, CallingConvention.StdCall,
                                   CharSet.Auto);
      // Apply preservesig metadata attribute so we can handle return HRESULT ourselves
      meb.SetImplementationFlags(MethodImplAttributes.PreserveSig | meb.GetMethodImplementationFlags());

      //LCD_GetCGRAMChar
      meb = tb.DefinePInvokeMethod("LCD_GetCGRAMChar", dllFile, METHOD_ATTRIBUTES,
                                   CallingConventions.Standard, typeof (byte), new Type[] {typeof (byte)},
                                   CallingConvention.StdCall,
                                   CharSet.Auto);
      // Apply preservesig metadata attribute so we can handle return HRESULT ourselves
      meb.SetImplementationFlags(MethodImplAttributes.PreserveSig | meb.GetMethodImplementationFlags());

      //LCD_SetCGRAMChar
      meb = tb.DefinePInvokeMethod("LCD_SetCGRAMChar", dllFile, METHOD_ATTRIBUTES,
                                   CallingConventions.Standard, typeof (void), new Type[] {typeof (CharacterData)},
                                   CallingConvention.StdCall,
                                   CharSet.Auto);
      // Apply preservesig metadata attribute so we can handle return HRESULT ourselves
      meb.SetImplementationFlags(MethodImplAttributes.PreserveSig | meb.GetMethodImplementationFlags());

      //LCD_SendToMemory
      meb = tb.DefinePInvokeMethod("LCD_SendToMemory", dllFile, METHOD_ATTRIBUTES,
                                   CallingConventions.Standard, typeof (void), new Type[] {typeof (byte)},
                                   CallingConvention.StdCall, CharSet.Auto);
      // Apply preservesig metadata attribute so we can handle return HRESULT ourselves
      meb.SetImplementationFlags(MethodImplAttributes.PreserveSig | meb.GetMethodImplementationFlags());

      //LCD_SendToGfxMemory
      meb = tb.DefinePInvokeMethod("LCD_SendToGfxMemory", dllFile, METHOD_ATTRIBUTES,
                                   CallingConventions.Standard, typeof (void),
                                   new Type[]
                                     {
                                       typeof (byte[]), typeof (int), typeof (int), typeof (int), typeof (int),
                                       typeof (bool)
                                     }, CallingConvention.StdCall, CharSet.Auto);
      // Apply preservesig metadata attribute so we can handle return HRESULT ourselves
      meb.SetImplementationFlags(MethodImplAttributes.PreserveSig | meb.GetMethodImplementationFlags());

      //LCD_SetOutputAddress
      meb = tb.DefinePInvokeMethod("LCD_SetOutputAddress", dllFile, METHOD_ATTRIBUTES,
                                   CallingConventions.Standard, typeof (void),
                                   new Type[] {typeof (int), typeof (int)},
                                   CallingConvention.StdCall, CharSet.Auto);
      // Apply preservesig metadata attribute so we can handle return HRESULT ourselves
      meb.SetImplementationFlags(MethodImplAttributes.PreserveSig | meb.GetMethodImplementationFlags());

      //LCD_SetIOPropertys
      meb = tb.DefinePInvokeMethod("LCD_SetIOPropertys", dllFile, METHOD_ATTRIBUTES,
                                   CallingConventions.Standard, typeof (void),
                                   new Type[]
                                     {
                                       typeof (string), typeof (int), typeof (int), typeof (int), typeof (int),
                                       typeof (int),
                                       typeof (int), typeof (bool), typeof (byte), typeof (bool), typeof (byte),
                                       typeof (int),
                                       typeof (bool), typeof (bool)
                                     }, CallingConvention.StdCall, CharSet.Ansi);
      // Apply preservesig metadata attribute so we can handle return HRESULT ourselves
      meb.SetImplementationFlags(MethodImplAttributes.PreserveSig | meb.GetMethodImplementationFlags());
      //            meb.DefineParameter(1,ParameterAttributes.HasFieldMarshal,"_port");

      // Create the type
      m_tDllReg = tb.CreateType();
    }


    public void Dispose()
    {
      Initialize();
      Cleanup();
    }

    /// <summary>
    /// This structure is used to send custom character data to the LCDHype drivers
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct CharacterData
    {
      [MarshalAs(UnmanagedType.U1)] public byte Position;

      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)] public byte[,] Data;

      public void SetData(int[] data)
      {
        Data = new byte[8,8];
        for (int i = 0; i < 8; i++)
        {
          for (int j = 0; j < 8; j++)
          {
            Data[7 - i, j] = (data[j] & (int) Math.Pow(2, i)) > 0 ? (byte) 1 : (byte) 0;
          }
        }
      }
    }

    /// <summary>
    /// This structure is returned from the LCDHype drivers and contains important information about the display
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DLLInfo
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)] public char[] IDArray; //Display description

      [MarshalAs(UnmanagedType.I1)] public bool SupportGfxLCD; //does this driver support graphical LCDs?

      [MarshalAs(UnmanagedType.I1)] public bool SupportTxtLCD; //does this driver support text

      [MarshalAs(UnmanagedType.I1)] public bool SupportLightSlider; //does this driver support the light control slider

      [MarshalAs(UnmanagedType.I1)] public bool SupportContrastSlider; //does this driver support the contrast slider

      [MarshalAs(UnmanagedType.I1)] public bool SupportOutports;
                                                //does this driver support outports for controlling external circuits

      [MarshalAs(UnmanagedType.U1)] public byte CCharWidth; //custom char width in pixels

      [MarshalAs(UnmanagedType.U1)] public byte CCharHeight; //custom char height in pixels

      [MarshalAs(UnmanagedType.U1)] public byte FontPitch; //fontpitch of LCD in pixels
    }
  }
}