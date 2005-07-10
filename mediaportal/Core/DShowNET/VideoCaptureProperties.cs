using System;
using System.Collections;
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
		Twinhan twinhan;
		IVac ivac ;
		Hauppauge hauppauge ;
		DigitalEverywhere digitalEverywhere;
		public VideoCaptureProperties(IBaseFilter filter)
		{	
			twinhan=new Twinhan(filter);
			ivac=new IVac(filter);
			hauppauge =new Hauppauge(filter);
			digitalEverywhere=new DigitalEverywhere(filter);
			/*
			if (hauppauge.IsHauppage) Log.Write("Hauppauge card properties supported");
			if (ivac.IsIVAC) Log.Write("IVAC card properties supported");
			if (digitalEverywhere.IsDigitalEverywhere) Log.Write("Digital Everywhere card properties supported");
			if (twinhan.IsTwinhan) Log.Write("Twinhan card properties supported");
			*/
		}

		public bool IsCAPMTNeeded
		{
			get { return twinhan.IsTwinhan; }
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
					return hauppauge.VersionInfo;
				}
				if (ivac.IsIVAC)
				{
					return ivac.VersionInfo;
				}
        return String.Empty;
      }
    }
		

		public bool SendPMT(int videoPid, int audioPid,byte[] PMT, int pmtLength)
		{
			if (digitalEverywhere.IsDigitalEverywhere)
			{
				return digitalEverywhere.SendPMTToFireDTV(PMT,pmtLength);
			}
			if (twinhan.IsTwinhan)
			{
				twinhan.SendPMT((uint)videoPid,(uint)audioPid,PMT,pmtLength);
				return true;
			}
			return false;
		}
		public bool SetPIDS(bool isDvbc, bool isDvbT, bool isDvbS, bool isAtsc, ArrayList pids)
		{
			if (digitalEverywhere.IsDigitalEverywhere)
			{
				return digitalEverywhere.SetPIDS(isDvbc, isDvbT, isDvbS, isAtsc, pids);
			}
			return false;
		}
		public bool IsCISupported()
		{
			if (digitalEverywhere.IsDigitalEverywhere) return true;
			if (twinhan.IsTwinhan) return true;
			return false;
		}
		
		public void SetTvFormat(AnalogVideoStandard standard)
		{
			if (ivac.IsIVAC)
			{
				ivac.SetTvFormat(standard);
			}
		}
	}//public class VideoCaptureProperties
}//namespace DShowNET
