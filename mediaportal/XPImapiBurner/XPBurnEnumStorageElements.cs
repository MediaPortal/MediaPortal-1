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
using System.Runtime.InteropServices;
using XPBurn.COM;
using STATSTG = XPBurn.COM.STATSTG;

namespace XPBurn
{
  [ComVisible(true)]
  internal unsafe class XPBurnEnumStorageElements : IEnumSTATSTG
  {
    #region Private fields

    private XPBurnIStorage fStorage;
    // TODO: IEnumerator<XPBurnIStream>
    private IEnumerator fStreamEnumerator;
    // TODO: IEnumerator<XPBurnIStorage>
    private IEnumerator fStorageEnumerator;

    #endregion

    #region Constructors

    public XPBurnEnumStorageElements(XPBurnIStorage storage)
    {
      fStreamEnumerator = null;
      fStorageEnumerator = null;
      fStorage = storage;
    }

    #endregion

    #region IEnumSTATSTG Members

    public unsafe int Next(uint celt, STATSTG* elt, uint* pceltFetched)
    {
      int returned;

      if (celt > 1)
      {
        return CONSTS.S_FALSE;
      }

      returned = 0;

      if (fStreamEnumerator == null)
      {
        fStreamEnumerator = fStorage.fStreams.Values.GetEnumerator();
      }
      if (fStorageEnumerator == null)
      {
        fStorageEnumerator = fStorage.fSubStorages.Values.GetEnumerator();
      }

      while ((returned < celt) && (fStreamEnumerator.MoveNext()))
      {
        ((XPBurnIStream)fStreamEnumerator.Current).Stat(elt, CONSTS.STATFLAG_DEFAULT);
        returned++;
      }

      while ((returned < celt) && (fStorageEnumerator.MoveNext()))
      {
        ((XPBurnIStorage)fStorageEnumerator.Current).Stat(elt, CONSTS.STATFLAG_DEFAULT);
        returned++;
      }

      if (pceltFetched != null)
      {
        *pceltFetched = (uint)returned;
      }

      if (returned == celt)
      {
        return CONSTS.S_OK;
      }
      else
      {
        return CONSTS.S_FALSE;
      }
    }

    public int Skip(uint p1)
    {
      return CONSTS.E_NOTIMPL;
    }

    public int Reset()
    {
      if (fStreamEnumerator != null)
      {
        ((IDisposable)fStreamEnumerator).Dispose();
      }
      if (fStorageEnumerator != null)
      {
        ((IDisposable)fStorageEnumerator).Dispose();
      }

      fStreamEnumerator = fStorage.fStreams.Values.GetEnumerator();
      fStorageEnumerator = fStorage.fSubStorages.Values.GetEnumerator();

      return CONSTS.S_OK;
    }

    public unsafe int Clone(void** p1)
    {
      return CONSTS.E_NOTIMPL;
    }

    #endregion
  }
}