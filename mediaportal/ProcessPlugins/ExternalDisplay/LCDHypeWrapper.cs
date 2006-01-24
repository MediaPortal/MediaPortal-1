/* 
 *	Copyright (C) 2005 Team MediaPortal
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

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace ProcessPlugins.ExternalDisplay
{
  public class LCDHypeWrapper : IDisplay
  {
    private const MethodAttributes METHOD_ATTRIBUTES =
      MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig;
    private const BindingFlags BINDING_FLAGS = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public;
    private string dllFile;
    private static ModuleBuilder s_mb;
    private Type m_tDllReg;
    private DLLInfo info;
    private string name;
    private bool isDisabled = false;
    private string errorMessage = "";

    public LCDHypeWrapper(string dllFile)
    {
      try
      {
        this.dllFile = dllFile;
        string[] tmp = dllFile.Split('/', '.', '\\');
        name = tmp[tmp.Length - 2];
        //if (!ExternalDisplay.VerifyDriverLynxDriver())
        //    return;
        CreateLCDHypeWrapper();
        GetDllInfo();
      }
      catch (Exception ex)
      {
        isDisabled = true;
        if (ex is TargetInvocationException)
        {
          Exception innerException = ex.InnerException;
          if (innerException != null && innerException is DllNotFoundException)
          {
            errorMessage = "Driverlynx Port I/O Driver not installed";
            return;
          }
        }
        errorMessage = ex.Message;
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
          {}
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

    public void Initialize(string _port, int _lines, int _cols, int _time, int _linesG, int _colsG, int _timeG,
                           bool _backLight, int _contrast)
    {
      if (!SupportsGraphics)
      {
        _linesG = 0;
        _colsG = 0;
        _timeG = 0;
      }

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
        m_tDllReg.InvokeMember("LCD_SendToMemory", BINDING_FLAGS, null, null, new object[] {_text[i]});
      }
    }

    public void Clear()
    {
      m_tDllReg.InvokeMember("LCD_Init", BINDING_FLAGS, null, null, null);
    }

    #endregion

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
                                   CallingConventions.Standard, typeof(void),
                                   new Type[] {Type.GetType("ProcessPlugins.ExternalDisplay.DLLInfo&")},
                                   CallingConvention.StdCall, CharSet.Auto);
      meb.DefineParameter(1, ParameterAttributes.Out, "_info");
      // Apply preservesig metadata attribute so we can handle return HRESULT ourselves
      meb.SetImplementationFlags(MethodImplAttributes.PreserveSig | meb.GetMethodImplementationFlags());

      //LCD_IsReadyToReceive
      meb = tb.DefinePInvokeMethod("LCD_IsReadyToReceive", dllFile, METHOD_ATTRIBUTES,
                                   CallingConventions.Standard, typeof(bool), null, CallingConvention.StdCall,
                                   CharSet.Auto);
      // Apply preservesig metadata attribute so we can handle return HRESULT ourselves
      meb.SetImplementationFlags(MethodImplAttributes.PreserveSig | meb.GetMethodImplementationFlags());

      //LCD_Init
      meb = tb.DefinePInvokeMethod("LCD_Init", dllFile, METHOD_ATTRIBUTES,
                                   CallingConventions.Standard, typeof(void), null, CallingConvention.StdCall,
                                   CharSet.Auto);
      // Apply preservesig metadata attribute so we can handle return HRESULT ourselves
      meb.SetImplementationFlags(MethodImplAttributes.PreserveSig | meb.GetMethodImplementationFlags());

      //LCD_ConfigDialog
      meb = tb.DefinePInvokeMethod("LCD_ConfigDialog", dllFile, METHOD_ATTRIBUTES,
                                   CallingConventions.Standard, typeof(void), null, CallingConvention.StdCall,
                                   CharSet.Auto);
      // Apply preservesig metadata attribute so we can handle return HRESULT ourselves
      meb.SetImplementationFlags(MethodImplAttributes.PreserveSig | meb.GetMethodImplementationFlags());

      //LCD_CleanUp
      meb = tb.DefinePInvokeMethod("LCD_CleanUp", dllFile, METHOD_ATTRIBUTES,
                                   CallingConventions.Standard, typeof(void), null, CallingConvention.StdCall,
                                   CharSet.Auto);
      // Apply preservesig metadata attribute so we can handle return HRESULT ourselves
      meb.SetImplementationFlags(MethodImplAttributes.PreserveSig | meb.GetMethodImplementationFlags());

      //LCD_SendToMemory
      meb = tb.DefinePInvokeMethod("LCD_SendToMemory", dllFile, METHOD_ATTRIBUTES,
                                   CallingConventions.Standard, typeof(void), new Type[] {typeof(char)},
                                   CallingConvention.StdCall, CharSet.Auto);
      // Apply preservesig metadata attribute so we can handle return HRESULT ourselves
      meb.SetImplementationFlags(MethodImplAttributes.PreserveSig | meb.GetMethodImplementationFlags());

      //LCD_SetOutputAddress
      meb = tb.DefinePInvokeMethod("LCD_SetOutputAddress", dllFile, METHOD_ATTRIBUTES,
                                   CallingConventions.Standard, typeof(void), new Type[] {typeof(int), typeof(int)},
                                   CallingConvention.StdCall, CharSet.Auto);
      // Apply preservesig metadata attribute so we can handle return HRESULT ourselves
      meb.SetImplementationFlags(MethodImplAttributes.PreserveSig | meb.GetMethodImplementationFlags());

      //LCD_SetIOPropertys
      meb = tb.DefinePInvokeMethod("LCD_SetIOPropertys", dllFile, METHOD_ATTRIBUTES,
                                   CallingConventions.Standard, typeof(void),
                                   new Type[]
                                     {
                                       typeof(string), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int),
                                       typeof(int), typeof(bool), typeof(byte), typeof(bool), typeof(byte), typeof(int),
                                       typeof(bool), typeof(bool)
                                     }, CallingConvention.StdCall, CharSet.Ansi);
      // Apply preservesig metadata attribute so we can handle return HRESULT ourselves
      meb.SetImplementationFlags(MethodImplAttributes.PreserveSig | meb.GetMethodImplementationFlags());
      //            meb.DefineParameter(1,ParameterAttributes.HasFieldMarshal,"_port");

      // Create the type
      m_tDllReg = tb.CreateType();
    }


    public void Dispose()
    {
      Clear();
      Cleanup();
    }
  }
}