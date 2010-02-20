#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.IO;
using System.Runtime.InteropServices;
using XPBurn.COM;
using STATSTG = XPBurn.COM.STATSTG;

namespace XPBurn
{
  [ComVisible(true)]
  internal unsafe class XPBurnIStream : IStream
  {
    #region Private fields

    private FileStream fFileStream;
    private string fStreamName;
    private string fFilename;

    #endregion

    #region Constructors

    public XPBurnIStream(string filename, string streamName, FileMode fileMode)
    {
      fFileStream = File.Open(filename, fileMode);
      fStreamName = streamName;
      fFilename = filename;
    }

    #endregion

    #region IStream Members

    public int Read(void* pv, uint cb, uint* pcbRead)
    {
      if (pv != null)
      {
        try
        {
          byte[] buffer = new byte[cb];
          int numberRead = fFileStream.Read(buffer, 0, (int)cb);
          Marshal.Copy(buffer, 0, new IntPtr(pv), (int)cb);

          if (pcbRead != null)
          {
            *pcbRead = (uint)numberRead;
          }

          return CONSTS.S_OK;
        }
        catch (Exception)
        {
          return CONSTS.S_FALSE;
        }
      }
      else
      {
        return CONSTS.STG_E_INVALIDPOINTER;
      }
    }

    public unsafe int Write(void* pv, uint cb, uint* pcbWritten)
    {
      if (pv != null)
      {
        try
        {
          byte[] buffer = new byte[cb];
          Marshal.Copy(new IntPtr(pv), buffer, 0, (int)cb);
          fFileStream.Write(buffer, 0, (int)cb);

          if (pcbWritten != null)
          {
            *pcbWritten = cb;
          }

          return CONSTS.S_OK;
        }
        catch (Exception)
        {
          return CONSTS.S_FALSE;
        }
      }
      else
      {
        return CONSTS.STG_E_INVALIDPOINTER;
      }
    }

    public unsafe int Seek(long dlibMove, uint dwOrigin, ulong* libNewPosition)
    {
      SeekOrigin origin;
      long position;

      switch (dwOrigin)
      {
        case CONSTS.STREAM_SEEK_SET:
          origin = SeekOrigin.Begin;
          break;
        case CONSTS.STREAM_SEEK_CUR:
          origin = SeekOrigin.Current;
          break;
        case CONSTS.STREAM_SEEK_END:
          origin = SeekOrigin.End;
          break;
        default:
          return CONSTS.STG_E_INVALIDFUNCTION;
      }

      try
      {
        position = fFileStream.Seek(dlibMove, origin);
        if (libNewPosition != null)
        {
          *libNewPosition = (ulong)position;
        }

        return CONSTS.S_OK;
      }
      catch (Exception)
      {
        return CONSTS.S_FALSE;
      }
    }

    public int SetSize(ulong libNewSize)
    {
      try
      {
        fFileStream.SetLength((long)libNewSize);
        if (libNewSize == (ulong)fFileStream.Length)
        {
          return CONSTS.S_OK;
        }
        else
        {
          return CONSTS.E_FAIL;
        }
      }
      catch (Exception)
      {
        return CONSTS.E_UNEXPECTED;
      }
    }

    public unsafe int CopyTo(IStream stm, ulong cb, ulong* cbRead, ulong* cbWritten)
    {
      long count;
      int bytesRead;
      uint bytesWritten;
      int result;

      if (cbRead != null)
      {
        *cbRead = 0;
      }
      if (cbWritten != null)
      {
        *cbWritten = 0;
      }

      if (stm != null)
      {
        count = Math.Min((long)cb, fFileStream.Length - fFileStream.Position);

        if (count > 0)
        {
          byte[] buffer = new byte[count];
          bytesRead = fFileStream.Read(buffer, 0, (int)count);

          if (cbRead != null)
          {
            *cbRead = (ulong)bytesRead;
          }

          fixed (byte* bufferPtr = buffer)
          {
            result = stm.Write(bufferPtr, (uint)bytesRead, &bytesWritten);
          }
          if ((result == CONSTS.S_OK) && (cbWritten != null))
          {
            *cbWritten = bytesWritten;
          }

          return result;
        }
        else
        {
          return CONSTS.S_OK;
        }
      }
      else
      {
        return CONSTS.STG_E_INVALIDPOINTER;
      }
    }

    public int Commit(uint p1)
    {
      return CONSTS.S_OK;
    }

    public int Revert()
    {
      return CONSTS.S_OK;
    }

    public int LockRegion(ulong p1, ulong p2, uint p3)
    {
      return CONSTS.STG_E_INVALIDFUNCTION;
    }

    public int UnlockRegion(ulong p1, ulong p2, uint p3)
    {
      return CONSTS.STG_E_INVALIDFUNCTION;
    }

    public unsafe int Stat(STATSTG* statstg, uint grfStatFlag)
    {
      if (statstg != null)
      {
        if ((grfStatFlag == CONSTS.STATFLAG_DEFAULT) || (grfStatFlag == CONSTS.STATFLAG_NONAME))
        {
          try
          {
            for (int index = 0; index < sizeof (STATSTG); index++)
            {
              ((byte*)statstg)[index] = 0;
            }

            statstg->type = CONSTS.STGTY_STREAM;
            statstg->cbSize = (ulong)fFileStream.Length;

            long mtime = File.GetLastWriteTime(fFilename).ToFileTime();
            statstg->mtime.dwHighDateTime = (uint)(mtime >> 32);
            statstg->mtime.dwLowDateTime = (uint)(mtime & ((long)0x00000000FFFFFFFF));
            long ctime = File.GetCreationTime(fFilename).ToFileTime();
            statstg->ctime.dwHighDateTime = (uint)(ctime >> 32);
            statstg->ctime.dwLowDateTime = (uint)(ctime & ((long)0x00000000FFFFFFFF));
            long atime = File.GetLastAccessTime(fFilename).ToFileTime();
            statstg->atime.dwHighDateTime = (uint)(atime >> 32);
            statstg->atime.dwLowDateTime = (uint)(atime & ((long)0x00000000FFFFFFFF));

            if (grfStatFlag != CONSTS.STATFLAG_NONAME)
            {
              statstg->pwcsName = (char*)Marshal.StringToCoTaskMemUni(fStreamName);
            }

            return CONSTS.S_OK;
          }
          catch (Exception)
          {
            return CONSTS.E_UNEXPECTED;
          }
        }
        else
        {
          return CONSTS.STG_E_INVALIDFLAG;
        }
      }
      else
      {
        return CONSTS.STG_E_INVALIDPOINTER;
      }
    }

    public unsafe int Clone(void** p1)
    {
      return CONSTS.E_NOTIMPL;
    }

    #endregion
  }
}