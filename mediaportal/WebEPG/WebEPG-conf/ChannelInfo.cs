using System;
using System.Collections;

namespace WebEPG_conf
{
	/// <summary>
	/// Summary description for ChannelInfo.
	/// </summary>
	public class ChannelInfo
	{
		public string DisplayName;
		public string FullName;
		public string ChannelID;
		public string PrimaryGrabberID;
		public string PrimaryGrabberName;
		public bool Linked;
		public int linkStart;
		public int linkEnd;
		public Hashtable GrabberList;
	}
}
