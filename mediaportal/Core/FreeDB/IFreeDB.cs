using System;

namespace MediaPortal.Freedb
{
	/// <summary>
	/// Summary description for IFreeDB.
	/// </summary>
	public interface IFreeDB
	{
		bool connect();
    bool connect(FreeDBSite site);
    bool disconnect();
    //string getDiscID(string tracks, string[] offsets, string time);
    string getServerMessage();
    string[] getListOfGenres();
    string[] getHelp(string topic);
    string[] getLog();
    string[] getStatus();
    string[] getUsers();
    string[] getVersion();
    string[] getMessageOfTheDay();
    bool update();
    CDInfo[] getCDInfo();  // possible ones
    CDInfoDetail getCDInfoDetail(CDInfo info);
    bool sendCDInfoDetail(CDInfoDetail info);  // write it to the FreeDB db...
	}
}
