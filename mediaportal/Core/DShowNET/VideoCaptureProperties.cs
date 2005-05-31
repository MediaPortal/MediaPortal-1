using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices; 
using MediaPortal.GUI.Library;

namespace DShowNET
{
	/// <summary>
	/// This class implements methods for vendor specific actions on tv capture cards.
	/// Currently we support
	/// - Hauppauge PVR cards
	/// - FireDTV digital cards
	/// </summary>
	public class VideoCaptureProperties
	{
		IVac ivac ;
		Hauppauge hauppauge ;
		DigitalEverywhere digitalEverywhere;
		public VideoCaptureProperties(IBaseFilter filter)
		{
			ivac=new IVac(filter);
			hauppauge =new Hauppauge(filter);
			digitalEverywhere=new DigitalEverywhere(filter);
			if (hauppauge.IsHauppage) Log.Write("Hauppauge card properties supported");
			if (ivac.IsIVAC) Log.Write("IVAC card properties supported");
			if (digitalEverywhere.IsDigitalEverywhere) Log.Write("Digital Everywhere card properties supported");
		}


		public void SetVideoBitRate(int minKbps, int maxKbps,bool isVBR)
		{
			
			if (hauppauge.IsHauppage)
			{
				hauppauge.SetVideoBitRate(minKbps, maxKbps,isVBR);
				return;
			}
			if (ivac.IsIVAC)
			{
				ivac.SetVideoBitRate(minKbps, maxKbps,isVBR);
				return;
			}
		}
		public bool GetVideoBitRate(out int minKbps, out int maxKbps,out bool isVBR)
		{
			minKbps=maxKbps=-1;
			isVBR=false;
			if (hauppauge.IsHauppage)
			{
				hauppauge.GetVideoBitRate(out minKbps, out maxKbps,out isVBR);
				return true;
			}
			if (ivac.IsIVAC)
			{
				ivac.GetVideoBitRate(out minKbps, out maxKbps,out isVBR);
				return true;
			}
			return false;
		}

    public string VersionInfo
    {
      get
			{
				if (hauppauge.IsHauppage)
				{
					return ivac.VersionInfo;
				}
				if (ivac.IsIVAC)
				{
					return ivac.VersionInfo;
				}
        return String.Empty;
      }
    }
		

		public bool SendPMTToFireDTV(byte[] PMT, int pmtLength)
		{
			if (digitalEverywhere.IsDigitalEverywhere)
			{
				return digitalEverywhere.SendPMTToFireDTV(PMT,pmtLength);
			}
			return false;
		}
		
	}//public class VideoCaptureProperties
}//namespace DShowNET
