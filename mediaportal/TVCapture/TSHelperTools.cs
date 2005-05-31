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
			public int	TableID;
			public int	SectionLen;
			public bool	IsMHWTable;
			public int	MHWIndicator;
			public byte[] Payload;
		}
		
		public TSHelperTools()
		{
			//
			// TODO: Fügen Sie hier die Konstruktorlogik hinzu
			//
		}

		public TSHeader GetHeader(IntPtr streamData)
		{
			TSHeader header=new TSHeader();
			header.SyncByte=Marshal.ReadByte(streamData,0); // indicates header is not valid
			if(header.SyncByte!=0x47)
				return header;// no ts-header, return
			header.SyncByte=Marshal.ReadByte(streamData,0);
			header.TransportError=(Marshal.ReadByte(streamData,1) & 0x80)>0?true:false;
			header.PayloadUnitStart=((Marshal.ReadByte(streamData,1)>>6) & 0x01)>0?true:false;
			header.TransportPriority=(Marshal.ReadByte(streamData,1) & 0x20)>0?true:false;
			header.Pid=((Marshal.ReadByte(streamData,1) & 0x1F) <<8)+Marshal.ReadByte(streamData,2);
			header.TransportScrambling=Marshal.ReadByte(streamData,3) & 0xC0;
			header.AdaptionFieldControl=(Marshal.ReadByte(streamData,3)>>4) & 0x3;
			header.ContinuityCounter=Marshal.ReadByte(streamData,3) & 0x0F;
			header.AdaptionField=Marshal.ReadByte(streamData,4);
			header.TableID=Marshal.ReadByte(streamData,5);
			header.SectionLen=((Marshal.ReadByte(streamData,6)-0x70)<<8)+Marshal.ReadByte(streamData,7);
			return header;
		}

	}
}
