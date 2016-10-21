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
using System.Runtime.InteropServices;
using DirectShowLib;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Dvb;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.DigitalDevices
{
  /// <summary>
  /// A wrapper class for Digital Devices CI slot properties and methods.
  /// </summary>
  internal class CiSlot : IDisposable
  {
    #region enums

    private enum CommonInterfaceProperty
    {
      DecryptProgram = 0,
      CamMenuTitle,
      CamCasIds,
      CiBitRate,
      CiUnlockFilter,
      CiTunerCount,
      CiMaxBitRate
    }

    private enum CamControlMethod
    {
      Reset = 0,
      EnterMenu,
      CloseMenu,
      GetMenu,
      MenuReply,    // Select a menu entry.
      CamAnswer,    // Send an answer to a CAM enquiry.
      SendCaPmt
    }

    /// <summary>
    /// Digital Devices CAM menu types.
    /// </summary>
    public enum MenuType
    {
      /// <summary>
      /// An unknown/unsupported/unrecognised menu type.
      /// </summary>
      Unknown,
      /// <summary>
      /// A menu (where choices can be made).
      /// </summary>
      Menu,
      /// <summary>
      /// A list (where entries are read-only).
      /// </summary>
      List,
      /// <summary>
      /// An enquiry (a request from the CAM).
      /// </summary>
      Enquiry
    }

    #endregion

    #region constants

    private static readonly Guid COMMON_INTERFACE_PROPERTY_SET = new Guid(0x0aa8a501, 0xa240, 0x11de, 0xb1, 0x30, 0x00, 0x00, 0x00, 0x00, 0x4d, 0x56);
    private static readonly Guid CAM_CONTROL_METHOD_SET = new Guid(0x0aa8a511, 0xa240, 0x11de, 0xb1, 0x30, 0x00, 0x00, 0x00, 0x00, 0x4d, 0x56);

    private const int BUFFER_SIZE = 2048;       // This is arbitrary - an estimate of the buffer size needed to hold the largest menu or answer. Note must be greater than Pmt.MAX_SIZE.
    private const int MENU_TITLE_LENGTH = 256;
    private const int MAX_CA_SYSTEM_COUNT = 64;
    private static readonly int KS_METHOD_SIZE = Marshal.SizeOf(typeof(KsMethod));  // 24

    #endregion

    #region variables

    private IntPtr _buffer = IntPtr.Zero;
    private object _lock = new object();
    private int _index = -1;
    private IKsPropertySet _propertySet = null;
    private IKsControl _control = null;

    #endregion

    public CiSlot(int index, IBaseFilter filter)
    {
      _index = index;
      _propertySet = filter as IKsPropertySet;
      _control = filter as IKsControl;
      _buffer = Marshal.AllocCoTaskMem(BUFFER_SIZE);
    }

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    ~CiSlot()
    {
      Dispose(false);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the slot is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (_buffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_buffer);
        _buffer = IntPtr.Zero;
      }
    }

    #endregion

    /// <summary>
    /// Get the CI slot's index.
    /// </summary>
    public int Index
    {
      get
      {
        return _index;
      }
    }

    /// <summary>
    /// Ask the CI to start decrypting a service.
    /// </summary>
    /// <param name="serviceId">The service identifier.</param>
    /// <returns>an HRESULT indicating whether the decrypt request was successfully passed</returns>
    public int DecryptService(uint serviceId)
    {
      lock (_lock)
      {
        Marshal.WriteInt32(_buffer, 0, (int)serviceId);
        return _propertySet.Set(COMMON_INTERFACE_PROPERTY_SET, (int)CommonInterfaceProperty.DecryptProgram,
          _buffer, 4,
          _buffer, 4
        );
      }
    }

    /// <summary>
    /// Read the menu title from the CAM in the CI.
    /// </summary>
    /// <param name="title">The CAM menu title.</param>
    /// <returns>an HRESULT indicating whether the CAM menu title was successfully retrieved</returns>
    public int GetCamMenuTitle(out string title)
    {
      title = string.Empty;

      lock (_lock)
      {
        for (int i = 0; i < MENU_TITLE_LENGTH; i++)
        {
          Marshal.WriteByte(_buffer, i, 0);
        }

        int returnedByteCount;
        int hr = _propertySet.Get(COMMON_INTERFACE_PROPERTY_SET, (int)CommonInterfaceProperty.CamMenuTitle,
          _buffer, MENU_TITLE_LENGTH,
          _buffer, MENU_TITLE_LENGTH,
          out returnedByteCount
        );
        if (hr == (int)NativeMethods.HResult.S_OK && returnedByteCount == MENU_TITLE_LENGTH)
        {
          title = DvbTextConverter.Convert(_buffer);
        }
        return hr;
      }
    }

    /// <summary>
    /// Read the supported CA system IDs from the CAM in the CI.
    /// </summary>
    /// <param name="casIds">The CA system IDs supported by the CAM.</param>
    /// <returns>an HRESULT indicating whether the CA system IDs were successfully retrieved</returns>
    public int GetCamCaSystemIds(out IList<ushort> casIds)
    {
      casIds = new List<ushort>(20);

      lock (_lock)
      {
        int bufferSize = MAX_CA_SYSTEM_COUNT * 2;
        for (int i = 0; i < bufferSize; i++)
        {
          Marshal.WriteByte(_buffer, i, 0);
        }

        int returnedByteCount;
        int hr = _propertySet.Get(COMMON_INTERFACE_PROPERTY_SET, (int)CommonInterfaceProperty.CamCasIds,
          _buffer, bufferSize,
          _buffer, bufferSize,
          out returnedByteCount
        );
        if (hr == (int)NativeMethods.HResult.S_OK)
        {
          int offset = 0;
          while (returnedByteCount - offset >= 2)
          {
            casIds.Add((ushort)Marshal.ReadInt16(_buffer, offset));
            offset += 2;
          }
        }
        return hr;
      }
    }

    /// <summary>
    /// Read the current bit rate of the stream passing through the CI.
    /// </summary>
    /// <param name="bitRate">The current bit rate of the stream passing through the CI.</param>
    /// <returns>an HRESULT indicating whether the bit rate was successfully retrieved</returns>
    public int GetCiBitRate(out int bitRate)
    {
      bitRate = 0;

      lock (_lock)
      {
        int returnedByteCount;
        int hr = _propertySet.Get(COMMON_INTERFACE_PROPERTY_SET, (int)CommonInterfaceProperty.CiBitRate,
          _buffer, 4,
          _buffer, 4,
          out returnedByteCount
        );
        if (hr == (int)NativeMethods.HResult.S_OK && returnedByteCount == 4)
        {
          bitRate = Marshal.ReadInt32(_buffer, 0);
        }
        return hr;
      }
    }

    /// <summary>
    /// Read the number of tuners linked to the CI.
    /// </summary>
    /// <param name="tunerCount">The number of tuners linked to the CI.</param>
    /// <returns>an HRESULT indicating whether the tuner count was successfully retrieved</returns>
    public int GetCiTunerCount(out int tunerCount)
    {
      tunerCount = 0;

      lock (_lock)
      {
        int returnedByteCount;
        int hr = _propertySet.Get(COMMON_INTERFACE_PROPERTY_SET, (int)CommonInterfaceProperty.CiTunerCount,
          _buffer, 4,
          _buffer, 4,
          out returnedByteCount
        );
        if (hr == (int)NativeMethods.HResult.S_OK && returnedByteCount == 4)
        {
          tunerCount = Marshal.ReadInt32(_buffer, 0);
        }
        return hr;
      }
    }

    /// <summary>
    /// Read the maximum stream bit rate that the CI supports.
    /// </summary>
    /// <param name="bitRate">The maximum bit rate that the CI supports.</param>
    /// <returns>an HRESULT indicating whether the bit rate was successfully retrieved</returns>
    public int GetCiMaxBitRate(out int bitRate)
    {
      bitRate = 0;

      lock (_lock)
      {
        int returnedByteCount;
        int hr = _propertySet.Get(COMMON_INTERFACE_PROPERTY_SET, (int)CommonInterfaceProperty.CiMaxBitRate,
          _buffer, 4,
          _buffer, 4,
          out returnedByteCount
        );
        if (hr == (int)NativeMethods.HResult.S_OK && returnedByteCount == 4)
        {
          bitRate = Marshal.ReadInt32(_buffer, 0);
        }
        return hr;
      }
    }

    /// <summary>
    /// Reset the CAM in the CI.
    /// </summary>
    /// <returns>an HRESULT indicating whether the CAM was successfully reset</returns>
    public int ResetCam()
    {
      lock (_lock)
      {
        KsMethod method = new KsMethod(CAM_CONTROL_METHOD_SET, (int)CamControlMethod.Reset, KsMethodFlag.Send);
        int returnedByteCount = 0;
        return _control.KsMethod(ref method, KS_METHOD_SIZE, IntPtr.Zero, 0, ref returnedByteCount);
      }
    }

    /// <summary>
    /// Send an enter menu request to the CAM in the CI.
    /// </summary>
    /// <returns>an HRESULT indicating whether the request was successfully sent</returns>
    public int EnterCamMenu()
    {
      lock (_lock)
      {
        KsMethod method = new KsMethod(CAM_CONTROL_METHOD_SET, (int)CamControlMethod.EnterMenu, KsMethodFlag.Send);
        int returnedByteCount = 0;
        return _control.KsMethod(ref method, KS_METHOD_SIZE, IntPtr.Zero, 0, ref returnedByteCount);
      }
    }

    /// <summary>
    /// Send a close menu request to the CAM in the CI.
    /// </summary>
    /// <returns>an HRESULT indicating whether the request was successfully sent</returns>
    public int CloseCamMenu()
    {
      lock (_lock)
      {
        KsMethod method = new KsMethod(CAM_CONTROL_METHOD_SET, (int)CamControlMethod.CloseMenu, KsMethodFlag.Send);
        int returnedByteCount = 0;
        return _control.KsMethod(ref method, KS_METHOD_SIZE, IntPtr.Zero, 0, ref returnedByteCount);
      }
    }

    /// <summary>
    /// Read the latest menu information from the CAM in the CI.
    /// </summary>
    /// <param name="id">The menu identifier. Used to determine whether the menu has been seen before.</param>
    /// <param name="type">The menu type.</param>
    /// <param name="strings">The menu strings (title, sub-title, entries and footer).</param>
    /// <param name="answerLength">For an enquiry menu: the expected answer length.</param>
    /// <returns>an HRESULT indicating whether the menu was successfully read</returns>
    public int GetCamMenu(out int id, out MenuType type, out IList<string> strings, out int answerLength)
    {
      id = 0;
      type = MenuType.Unknown;
      strings = new List<string>(15);
      answerLength = 0;

      lock (_lock)
      {
        for (int i = 0; i < BUFFER_SIZE; i++)
        {
          Marshal.WriteByte(_buffer, i, 0);
        }

        KsMethod method = new KsMethod(CAM_CONTROL_METHOD_SET, (int)CamControlMethod.GetMenu, KsMethodFlag.Send);
        int returnedByteCount = 0;
        int hr = _control.KsMethod(ref method, KS_METHOD_SIZE, _buffer, BUFFER_SIZE, ref returnedByteCount);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          return hr;
        }

        //Dump.DumpBinary(_buffer, returnedByteCount);

        id = Marshal.ReadInt32(_buffer, 0);
        int menuType = Marshal.ReadInt32(_buffer, 4);
        if (menuType == 1 || menuType == 2)
        {
          type = MenuType.Menu;
          if (menuType == 2)
          {
            type = MenuType.List;
          }
          int stringCount = Marshal.ReadInt32(_buffer, 8) + 3;   // + 3 for title, sub-title and footer
          int byteCount = Marshal.ReadInt32(_buffer, 12);

          IntPtr stringPtr = IntPtr.Add(_buffer, 16);
          int decodedByteCount;
          int totalDecodedByteCount = 0;
          for (int i = 0; i < stringCount && totalDecodedByteCount < byteCount; i++)
          {
            string s = DvbTextConverter.Convert(stringPtr, -1, 0, out decodedByteCount);
            if (decodedByteCount == 0)
            {
              this.LogWarn("Digital Devices: failed to decode menu/list string {0} of {1}", i + 1, stringCount);
              Dump.DumpBinary(_buffer, returnedByteCount);
              break;
            }
            totalDecodedByteCount += decodedByteCount;
            strings.Add(s);
            stringPtr = IntPtr.Add(stringPtr, decodedByteCount);
          }

          if (strings.Count < 3 || stringCount != strings.Count)
          {
            this.LogError("Digital Devices: actual menu entry count {0} does not match expected entry count {1}", strings.Count, stringCount);
            Dump.DumpBinary(_buffer, returnedByteCount);
            return (int)NativeMethods.HResult.E_FAIL;
          }
        }
        else if (menuType == 3 || menuType == 4)
        {
          type = MenuType.Enquiry;
          answerLength = Marshal.ReadInt32(_buffer, 8);
          int byteCount = Marshal.ReadInt32(_buffer, 12); // number of bytes in the prompt string, including NULL termination
          strings.Add(DvbTextConverter.Convert(IntPtr.Add(_buffer, 16)));
        }
        // We seem to get a type 0 menu for each actual list/menu. That might
        // be because the interface expects each list/menu has to be handled
        // (closed?) individually. We ignore the type 0 menus.
        else if (menuType != 0)
        {
          this.LogWarn("Digital Devices: menu type {0} not supported", menuType);
          Dump.DumpBinary(_buffer, returnedByteCount);
        }
        return hr;
      }
    }

    /// <summary>
    /// Send an entry selection request to the CAM in the CI.
    /// </summary>
    /// <param name="id">The identifier of the menu which the selection relates to.</param>
    /// <param name="choice">The index of the selected entry.</param>
    /// <returns>an HRESULT indicating whether the request was successfully sent</returns>
    public int SelectCamMenuEntry(int id, int choice)
    {
      lock (_lock)
      {
        Marshal.WriteInt32(_buffer, 0, id);
        Marshal.WriteInt32(_buffer, 4, choice);
        KsMethod method = new KsMethod(CAM_CONTROL_METHOD_SET, (int)CamControlMethod.MenuReply, KsMethodFlag.Send);
        int returnedByteCount = 0;
        return _control.KsMethod(ref method, KS_METHOD_SIZE, _buffer, 8, ref returnedByteCount);
      }
    }

    /// <summary>
    /// Send an enquiry answer request to the CAM in the CI.
    /// </summary>
    /// <param name="id">The identifier of the menu which the answer relates to.</param>
    /// <param name="answer">The answer.</param>
    /// <returns>an HRESULT indicating whether the request was successfully sent</returns>
    public int AnswerCamMenuEnquiry(int id, string answer)
    {
      lock (_lock)
      {
        Marshal.WriteInt32(_buffer, 0, id);
        int bufferSize = 12;
        if (answer != null && answer.Length > 0)
        {
          Marshal.WriteInt32(_buffer, 4, answer.Length);
          for (int i = 0; i < answer.Length; i++)
          {
            Marshal.WriteByte(_buffer, 8 + i, (byte)answer[i]);
          }
          Marshal.WriteByte(_buffer, 8 + answer.Length, 0); // NULL terminate
          bufferSize = 8 + answer.Length + 1;
        }
        else
        {
          Marshal.WriteInt32(_buffer, 4, 0);
          Marshal.WriteInt32(_buffer, 8, 0);
        }

        KsMethod method = new KsMethod(CAM_CONTROL_METHOD_SET, (int)CamControlMethod.CamAnswer, KsMethodFlag.Send);
        int returnedByteCount = 0;
        return _control.KsMethod(ref method, KS_METHOD_SIZE, _buffer, bufferSize, ref returnedByteCount);
      }
    }

    /// <summary>
    /// Send CA PMT to the CAM in the CI.
    /// </summary>
    /// <param name="caPmt">The CA PMT.</param>
    /// <returns>an HRESULT indicating whether the CA PMT was successfully sent</returns>
    public int SendCaPmt(byte[] caPmt)
    {
      lock (_lock)
      {
        Marshal.WriteInt32(_buffer, 0, caPmt.Length);
        Marshal.Copy(caPmt, 0, IntPtr.Add(_buffer, 4), caPmt.Length);

        KsMethod method = new KsMethod(CAM_CONTROL_METHOD_SET, (int)CamControlMethod.SendCaPmt, KsMethodFlag.Send);
        int returnedByteCount = 0;
        return _control.KsMethod(ref method, KS_METHOD_SIZE, _buffer, 4 + caPmt.Length, ref returnedByteCount);
      }
    }
  }
}