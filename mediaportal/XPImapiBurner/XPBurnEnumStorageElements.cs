using System;
using System.Collections;
using System.Runtime.InteropServices;
using XPBurn.COM;
using STATSTG=XPBurn.COM.STATSTG;

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
        ((XPBurnIStream) fStreamEnumerator.Current).Stat(elt, CONSTS.STATFLAG_DEFAULT);
        returned++;
      }

      while ((returned < celt) && (fStorageEnumerator.MoveNext()))
      {
        ((XPBurnIStorage) fStorageEnumerator.Current).Stat(elt, CONSTS.STATFLAG_DEFAULT);
        returned++;
      }

      if (pceltFetched != null)
      {
        *pceltFetched = (uint) returned;
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
        ((IDisposable) fStreamEnumerator).Dispose();
      }
      if (fStorageEnumerator != null)
      {
        ((IDisposable) fStorageEnumerator).Dispose();
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