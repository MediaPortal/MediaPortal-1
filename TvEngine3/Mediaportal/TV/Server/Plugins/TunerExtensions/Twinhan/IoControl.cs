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
using System.Runtime.InteropServices;
using DirectShowLib;
using Mediaportal.TV.Server.Plugins.TunerExtension.Twinhan.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.Twinhan
{
  internal class IoControl
  {
    #region constants

    // GUID_THBDA_TUNER
    private static readonly Guid BDA_EXTENSION_PROPERTY_SET = new Guid(0xe5644cc4, 0x17a1, 0x4eed, 0xbd, 0x90, 0x74, 0xfd, 0xa1, 0xd6, 0x54, 0x23);
    // GUID_THBDA_CMD
    private static readonly Guid COMMAND_GUID = new Guid(0x255e0082, 0x2017, 0x4b03, 0x90, 0xf8, 0x85, 0x6a, 0x62, 0xcb, 0x3d, 0x67);

    private const int INSTANCE_SIZE = 32;   // The size of a property instance (KSP_NODE) parameter.
    private const int COMMAND_SIZE = 40;

    #endregion

    #region variables

    private IKsPropertySet _propertySet = null;

    #endregion

    public IoControl(IKsPropertySet propertySet)
    {
      _propertySet = propertySet;
    }

    public int Set(IoControlCode controlCode, IntPtr inBuffer, int inBufferSize)
    {
      int returnedByteCount;
      return Execute(controlCode, inBuffer, inBufferSize, IntPtr.Zero, 0, out returnedByteCount);
    }

    public int Get(IoControlCode controlCode, IntPtr outBuffer, int outBufferSize, out int returnedByteCount)
    {
      return Execute(controlCode, IntPtr.Zero, 0, outBuffer, outBufferSize, out returnedByteCount);
    }

    private int Execute(IoControlCode controlCode, IntPtr inBuffer, int inBufferSize, IntPtr outBuffer, int outBufferSize, out int returnedByteCount)
    {
      returnedByteCount = 0;
      int hr = (int)NativeMethods.HResult.E_FAIL;

      IntPtr instanceBuffer = Marshal.AllocCoTaskMem(INSTANCE_SIZE);
      IntPtr commandBuffer = Marshal.AllocCoTaskMem(COMMAND_SIZE);
      IntPtr returnedByteCountBuffer = Marshal.AllocCoTaskMem(sizeof(int));
      try
      {
        // Clear buffers. This is probably not actually needed, but better
        // to be safe than sorry!
        for (int i = 0; i < INSTANCE_SIZE; i++)
        {
          Marshal.WriteByte(instanceBuffer, i, 0);
        }
        Marshal.WriteInt32(returnedByteCountBuffer, 0, 0);

        // Fill the command buffer.
        Marshal.Copy(COMMAND_GUID.ToByteArray(), 0, commandBuffer, 16);
        Marshal.WriteInt32(commandBuffer, 16, (int)NativeMethods.CTL_CODE((NativeMethods.FileDevice)0xaa00, (uint)controlCode, NativeMethods.Method.METHOD_BUFFERED, NativeMethods.FileAccess.FILE_ANY_ACCESS));
        Marshal.WriteInt32(commandBuffer, 20, inBuffer.ToInt32());
        Marshal.WriteInt32(commandBuffer, 24, inBufferSize);
        Marshal.WriteInt32(commandBuffer, 28, outBuffer.ToInt32());
        Marshal.WriteInt32(commandBuffer, 32, outBufferSize);
        Marshal.WriteInt32(commandBuffer, 36, returnedByteCountBuffer.ToInt32());

        hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, 0, instanceBuffer, INSTANCE_SIZE, commandBuffer, COMMAND_SIZE);
        if (hr == (int)NativeMethods.HResult.S_OK)
        {
          returnedByteCount = Marshal.ReadInt32(returnedByteCountBuffer, 0);
        }
      }
      finally
      {
        Marshal.FreeCoTaskMem(instanceBuffer);
        Marshal.FreeCoTaskMem(commandBuffer);
        Marshal.FreeCoTaskMem(returnedByteCountBuffer);
      }
      return hr;
    }
  }
}