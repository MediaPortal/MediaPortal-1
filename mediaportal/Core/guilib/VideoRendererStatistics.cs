/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
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
      try // QI for get_AvgFrameRate crashes in GUIDialogNotify.cs when Video is being played,
          // that's why we're working around it for now: NEEDS FURTHER INVESTIGATION!
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
      catch
      {
      }
		}
	}
}
