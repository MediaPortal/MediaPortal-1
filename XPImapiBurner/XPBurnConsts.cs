using System;

namespace XPBurn
{
	/// <summary>
	/// Summary description for XPBurnConsts.
	/// </summary>
	internal class CONSTS
	{
    public const int MEDIA_BLANK = 0x1;
    public const int MEDIA_RW = 0x2;
    public const int MEDIA_WRITABLE = 0x4;
    public const int MEDIA_UNUSABLE = 0x8;
    public const int E_NOTIMPL = unchecked((int)0x80004001);
    public const int S_OK = 0x00000000;
    public const int S_FALSE = 0x00000001;
    public const uint STGTY_STORAGE = 1;
    public const uint STATFLAG_NONAME = 1;
    public const int STG_E_INVALIDPOINTER = unchecked((int)0x80030009);
    public const int STG_E_FILENOTFOUND = unchecked((int)0x80030002);    
    public const int STG_E_INVALIDFUNCTION = unchecked((int)0x80030001);
    public const int STG_E_INVALIDFLAG = unchecked((int)0x800300FF);
    public const int STATFLAG_DEFAULT = 0;    
    public const int STGTY_STREAM = 2;
    public const int E_UNEXPECTED = unchecked((int)0x8000FFFF);
    public const int STREAM_SEEK_SET = 0;  
    public const int STREAM_SEEK_CUR = 1;
    public const int STREAM_SEEK_END = 2;
    public const int E_FAIL = unchecked((int)0x80004005);
	}
}
