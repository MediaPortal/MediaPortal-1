using System;
using System.Runtime.InteropServices;

namespace MediaPortal.TV.Recording{
	/// <summary>
	/// Zusammenfassung für TSHelperTools.
	/// </summary>
	public class TSHelperTools
	{
		
		public struct TSHeader
		{
			public int	SyncByte;
			public bool TransportError;
			public bool PayloadUnitStart;
			public bool TransportPriority;
			public int	Pid;
			public int	TransportScrambling;
			public int	AdaptionFieldControl;
			public int	ContinuityCounter;
			public int	AdaptionField;

		}
		
		public TSHelperTools()
		{
			//
			// TODO: Fügen Sie hier die Konstruktorlogik hinzu
			//
		}

		public TSHeader GetHeader(IntPtr streamData)
		{
			byte[] data=new byte[5];
			Marshal.Copy(streamData,data,0,5);
			TSHeader header=new TSHeader();
			header.SyncByte=data[0]; // indicates header is not valid
			if(data[0]!=0x47)
				return header;// no ts-header, return
			header.SyncByte=data[0];
			header.TransportError=(data[1] & 0x80)>0?true:false;
			header.PayloadUnitStart=(data[1] & 0x40)>0?true:false;
			header.TransportPriority=(data[1] & 0x20)>0?true:false;
			header.Pid=((data[1] & 0x1F) <<8)+data[2];
			header.TransportScrambling=data[3] & 0xC0;
			header.AdaptionFieldControl=data[3] & 0x30;
			header.ContinuityCounter=data[3] & 0x0F;
			header.AdaptionField=data[4];

			return header;
		}
	}
}
