using System;
using System.Collections;
using XPBurn.COM;
using System.Runtime.InteropServices;
using System.IO;

namespace XPBurn
{	
  [ComVisible(true)]
	unsafe internal class XPBurnIStorage: IStorage
  {    
    #region Private Fields

    private XPBurn.COM.STATSTG fStatstg;

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
      fStatstg = new XPBurn.COM.STATSTG();

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
      XPBurnIStream stream = new XPBurnIStream(filename, streamName, System.IO.FileMode.Open);
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

    unsafe public int CreateStream(string p1, uint p2, uint p3, uint p4, void * * p5)
    {      
      return CONSTS.E_NOTIMPL;
    }

    unsafe public int OpenStream(string pwcsName, void* reserved1, uint grfMode, uint reserved2, out IStream stm)
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
          return CONSTS.STG_E_FILENOTFOUND;
      }
      else
        return CONSTS.STG_E_INVALIDPOINTER;
    }

    unsafe public int CreateStorage(string p1, uint p2, uint p3, uint p4, void * * p5)
    {      
      return CONSTS.E_NOTIMPL;
    }

    unsafe public int OpenStorage(string pwcsName, IStorage stgPriority, uint grfMode, char** snbExclude, uint reserved, out IStorage stg)
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
          return CONSTS.STG_E_FILENOTFOUND;
      }
      else
        return CONSTS.STG_E_INVALIDPOINTER;      
    }

    unsafe public int CopyTo(uint p1, Guid * p2, char * * p3, IStorage p4)
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

    unsafe public int EnumElements(uint p1, void* p2, uint p3, out IEnumSTATSTG enm)
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

    unsafe public int SetElementTimes(string p1, XPBurn.COM.FILETIME * p2, XPBurn.COM.FILETIME * p3, XPBurn.COM.FILETIME * p4)
    {     
      return CONSTS.E_NOTIMPL;
    }

    unsafe public int SetClass(Guid * p1)
    {      
      return CONSTS.E_NOTIMPL;
    }

    public int SetStateBits(uint p1, uint p2)
    {     
      return CONSTS.E_NOTIMPL;
    }

    unsafe public int Stat(XPBurn.COM.STATSTG* statstg, uint grfStatFlag)
    {
      if (statstg != null)
      {
        if (grfStatFlag != CONSTS.STATFLAG_NONAME)
        {
          string tempString = Marshal.PtrToStringUni(new IntPtr(fStatstg.pwcsName));
          statstg->pwcsName =(char*)Marshal.StringToCoTaskMemUni(tempString);
        }
        else
          statstg->pwcsName = null;

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
        return CONSTS.STG_E_INVALIDPOINTER;
    }

    #endregion
  }
}
