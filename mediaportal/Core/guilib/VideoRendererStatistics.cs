using System;
using DShowNET;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// Summary description for VideoRendererStatistics.
	/// </summary>
	public class VideoRendererStatistics
	{
		static int   framesDrawn=0,avgSyncOffset=0,avgDevSyncOffset=0,framesDropped=0,jitter=0;
		static float avgFrameRate=0f;

		static public float AverageFrameRate
		{
			get { return avgFrameRate;}
			set 
			{
				avgFrameRate=value;}
		}
		static public int AverageSyncOffset
		{
			get { return avgSyncOffset;}
			set 
			{
				avgSyncOffset=value;}
		}
		static public int AverageDeviationSyncOffset
		{
			get { return avgDevSyncOffset;}
			set 
			{
				avgDevSyncOffset=value;}
		}
		static public int FramesDrawn
		{
			get { return framesDrawn;}
			set 
			{
				framesDrawn=value;
			}
		}
		static public int FramesDropped
		{
			get { return framesDropped;}
			set 
			{
				framesDropped=value;
			}
		}
		static public int Jitter
		{
			get { return jitter;}
			set {jitter=value;}
		}


		static public void Update(IQualProp quality)
		{
			if (quality!=null)
			{
				int framesDrawn=0,avgFrameRate=0,avgSyncOffset=0,avgDevSyncOffset=0,framesDropped=0,jitter=0;
				quality.get_AvgFrameRate(out avgFrameRate);
				quality.get_AvgSyncOffset(out avgSyncOffset);
				quality.get_DevSyncOffset(out avgDevSyncOffset);
				quality.get_FramesDrawn(out framesDrawn);
				quality.get_FramesDroppedInRenderer(out framesDropped);
				quality.get_Jitter(out jitter);
				VideoRendererStatistics.AverageFrameRate = ((float)avgFrameRate)/100.0f;
				VideoRendererStatistics.AverageSyncOffset=avgSyncOffset;
				VideoRendererStatistics.AverageDeviationSyncOffset=avgDevSyncOffset;
				VideoRendererStatistics.FramesDrawn=framesDrawn;
				VideoRendererStatistics.FramesDropped=framesDropped;
				VideoRendererStatistics.Jitter=jitter;
			}
		}
	}
}
