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
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using XPBurn.COM;
using FILETIME = XPBurn.COM.FILETIME;
using STATSTG = XPBurn.COM.STATSTG;

namespace XPBurn
{
  [ComVisible(true)]
  internal unsafe class XPBurnIStorage : IStorage
  {
    #region Private Fields

    private STATSTG fStatstg;

    #endregion

    #region Public Fields

    // TODO: Should be Dictionary<string, XPBurnIStream>
    public Hashtable fStreams;
    // TODO: Shold be Dictionary<string, XPBurnIStorage>
    public Hashtable fSubStorages;

    #endregion

    #region Constructors

    public XPBurnIStorage(string name)
    {
      fStreams = new Hashtable();
      fSubStorages = new Hashtable();
      fStatstg = new STATSTG();

      fStatstg.pwcsName = (char*)Marshal.StringToCoTaskMemUni(name);
      fStatstg.type = CONSTS.STGTY_STORAGE;
    }

    #endregion

    #region Destructors

    ~XPBurnIStorage()
    {
      Marshal.FreeCoTaskMem(new IntPtr(fStatstg.pwcsName));
    }

    #endregion

    #region Public helper methods

    public XPBurnIStream CreateFileStream(string filename, string streamName)
    {
      XPBurnIStream stream = new XPBurnIStream(filename, streamName, FileMode.Open);
      fStreams.Add(streamName, stream);

      return stream;
    }

    public XPBurnIStorage CreateStorageDirectory(string storageName)
    {
      XPBurnIStorage storage = new XPBurnIStorage(storageName);
      fSubStorages.Add(storageName, storage);

      return storage;
    }

    #endregion

    #region IStorage Members

    public unsafe int CreateStream(string p1, uint p2, uint p3, uint p4, void** p5)
    {
      return CONSTS.E_NOTIMPL;
    }

    public unsafe int OpenStream(string pwcsName, void* reserved1, uint grfMode, uint reserved2, out IStream stm)
    {
      stm = null;

      if (pwcsName != null)
      {
        if (fStreams.Contains(pwcsName))
        {
          stm = (IStream)fStreams[pwcsName];

          return CONSTS.S_OK;
        }
        else
        {
          return CONSTS.STG_E_FILENOTFOUND;
        }
      }
      else
      {
        return CONSTS.STG_E_INVALIDPOINTER;
      }
    }

    public unsafe int CreateStorage(string p1, uint p2, uint p3, uint p4, void** p5)
    {
      return CONSTS.E_NOTIMPL;
    }

    public unsafe int OpenStorage(string pwcsName, IStorage stgPriority, uint grfMode, char** snbExclude, uint reserved,
                                  out IStorage stg)
    {
      stg = null;

      if (pwcsName != null)
      {
        if (fSubStorages.Contains(pwcsName))
        {
          stg = (IStorage)fSubStorages[pwcsName];

          return CONSTS.S_OK;
        }
        else
        {
          return CONSTS.STG_E_FILENOTFOUND;
        }
      }
      else
      {
        return CONSTS.STG_E_INVALIDPOINTER;
      }
    }

    public unsafe int CopyTo(uint p1, Guid* p2, char** p3, IStorage p4)
    {
      return CONSTS.E_NOTIMPL;
    }

    public int MoveElementTo(string p1, IStorage p2, string p3, uint p4)
    {
      return CONSTS.E_NOTIMPL;
    }

    public int Commit(uint p1)
    {
      return CONSTS.S_OK;
    }

    public int Revert()
    {
      return CONSTS.S_OK;
    }

    public unsafe int EnumElements(uint p1, void* p2, uint p3, out IEnumSTATSTG enm)
    {
      enm = new XPBurnEnumStorageElements(this);

      return CONSTS.S_OK;
    }

    public int DestroyElement(string p1)
    {
      return CONSTS.E_NOTIMPL;
    }

    public int RenameElement(string p1, string p2)
    {
      return CONSTS.E_NOTIMPL;
    }

    public unsafe int SetElementTimes(string p1, FILETIME* p2, FILETIME* p3, FILETIME* p4)
    {
      return CONSTS.E_NOTIMPL;
    }

    public unsafe int SetClass(Guid* p1)
    {
      return CONSTS.E_NOTIMPL;
    }

    public int SetStateBits(uint p1, uint p2)
    {
      return CONSTS.E_NOTIMPL;
    }

    public unsafe int Stat(STATSTG* statstg, uint grfStatFlag)
    {
      if (statstg != null)
      {
        if (grfStatFlag != CONSTS.STATFLAG_NONAME)
        {
          string tempString = Marshal.PtrToStringUni(new IntPtr(fStatstg.pwcsName));
          statstg->pwcsName = (char*)Marshal.StringToCoTaskMemUni(tempString);
        }
        else
        {
          statstg->pwcsName = null;
        }

        statstg->type = fStatstg.type;
        statstg->cbSize = fStatstg.cbSize;
        statstg->mtime = fStatstg.mtime;
        statstg->ctime = fStatstg.ctime;
        statstg->atime = fStatstg.atime;
        statstg->grfMode = fStatstg.grfMode;
        statstg->grfLocksSupported = fStatstg.grfLocksSupported;
        statstg->clsid = fStatstg.clsid;
        statstg->grfStateBits = fStatstg.grfStateBits;
        statstg->reserved = fStatstg.reserved;

        return CONSTS.S_OK;
      }
      else
      {
        return CONSTS.STG_E_INVALIDPOINTER;
      }
    }

    #endregion
  }
}