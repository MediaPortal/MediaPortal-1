/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
 *	http://www.team-mediaportal.com
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
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Collections;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using DirectShowLib.SBE;
using DirectShowLib;
using DShowNET.Helper;
using System.Runtime.InteropServices;
using Mpeg2SplitterPackage;

namespace DvrMpegCutMP
{
	class DvrMpegCutPreview : GUIWindow
	{
        enum EMode
        {
            E_CUT,
            E_TRIM
        };
		const int windowID = 170602;

		#region GUIControls
		[SkinControlAttribute(24)]
		protected GUIButtonControl cutBtn = null;
		[SkinControlAttribute(25)]
		protected GUIButtonControl cancelBtn = null;
		[SkinControlAttribute(30)]
		protected GUIButtonControl endBtn = null;
		[SkinControlAttribute(29)]
		protected GUIButtonControl startBtn = null;
		[SkinControlAttribute(28)]
		protected GUIButtonControl addBtn = null;
		[SkinControlAttribute(31)]
		protected GUIButtonControl forwardBtn = null;
		[SkinControlAttribute(34)]
		protected GUIButtonControl backwardBtn = null;
		[SkinControlAttribute(33)]
		protected GUILabelControl currentPosLbl = null;
		[SkinControlAttribute(35)]
		protected GUILabelControl oldLenghtLbl = null;
		[SkinControlAttribute(36)]
		protected GUILabelControl newLenghtLbl = null;
		[SkinControlAttribute(37)]
		protected GUILabelControl startPosLbl = null;
		[SkinControlAttribute(38)]
		protected GUILabelControl endPosLbl = null;
		[SkinControlAttribute(99)]
		protected GUIVideoControl videoWindow = null;
		[SkinControlAttribute(100)]
		protected GUISliderControl positionSld = null;
		[SkinControlAttribute(101)]
		protected GUIListControl videoListLct = null;
		#endregion

		#region Own variables
		double durationOld;
		double durationNew;
		double curPosition;
		double startCut = 0;
		double endCut = 0;
		ArrayList cutList = new ArrayList();
		FileInfo inFilename;
		FileInfo outFilename;
		FileTypes cutType;
		Thread cutThread;
		IStreamBufferRecComp recCompcut = null;

        //EMode eMode = EMode.E_CUT;
        EMode eMode = EMode.E_TRIM;
		const int NR_OF_SPILTER_TIME_STAMPS = 40;
		SPLITTER_TIME_STAMP[] tStamp = new SPLITTER_TIME_STAMP[NR_OF_SPILTER_TIME_STAMPS];
		int iCount = 0;

		#endregion

		#region constructor
		public DvrMpegCutPreview(string filepath)
		{
			try
			{
				if (filepath != String.Empty)
				{
					inFilename = new FileInfo(filepath);
					GetFiletype();
				}
				GetID = windowID;
			}
			catch (Exception ex)
			{
				Log.WriteFile(Log.LogType.Error, "DvrMpegCut: (DvrMpegCutPreview) " + ex.StackTrace);
			}

		}
		#endregion
		
		#region overrides
		public override bool Init()
		{
			iCount = 0; 
			return Load(GUIGraphicsContext.Skin + @"\CutScreen.xml");
		}

		protected override void OnPageLoad()
		{
			try
			{
				startPosLbl.Label = "";
				endPosLbl.Label = "";
				durationNew = 0;
                GUIGraphicsContext.VMR9Allowed = true;
                GUIGraphicsContext.IsFullScreenVideo = false;
                GUIWindowManager.ActiveWindow = (int)GUIWindow.Window.WINDOW_TV;
                if (videoWindow != null)
                {
                    GUIGraphicsContext.VideoWindow = new System.Drawing.Rectangle(videoWindow.XPosition, videoWindow.YPosition, videoWindow.Width, videoWindow.Height);
                    //Log.Write("Test " +videoWindow.XPosition  + " " + videoWindow.Width + " " + videoWindow.Height);
                }
                g_Player.FullScreen = false;
                g_Player.Play(inFilename.FullName);
				//g_Player.Pause();
				durationOld = g_Player.Duration;
				oldLenghtLbl.Label = TimeCalc(durationOld);
				newLenghtLbl.Label = TimeCalc(durationNew);
				//postitionSld.Percentage = 100;
				//postitionSld.SpinType = GUISpinControl.SpinType.SPIN_CONTROL_TYPE_FLOAT;
				//postitionSld.SetFloatRange(0, (float)dauerAlt);
				//postitionSld.FloatInterval = (float)0.5;
				positionSld.Percentage = 0;
				//schneideListeLct.Width = 350;
				//schneideListeLct.Height = 200;
				//	}
				//	else
				//		System.Windows.Forms.MessageBox.Show("Datei nicht vorhanden");
			}
			catch (Exception ex)
			{
				Log.WriteFile(Log.LogType.Error, "DvrMpegCut: (OnPageLoad) " + ex.StackTrace);
			}
			//schneideListeLct.Add(new GUIListItem("Test"));


		}

		protected override void OnPageDestroy(int new_windowId)
		{
			g_Player.Release();
			inFilename = null;
			//vmrPlayer.Release();
			base.OnPageDestroy(new_windowId);
		}

		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (control == cutBtn)
			{
				Cut();
			}
			if (control == cancelBtn)
			{
				if (cutThread != null)
				{
					cutThread.Abort();
					MessageBox("Process was canceled", "Cancel");
				}
			}
			if (control == startBtn)
			{
				startCut = g_Player.CurrentPosition;
				startPosLbl.Label = TimeCalc(startCut);
			}
			if (control == endBtn)
			{
				endCut = g_Player.CurrentPosition;
				endPosLbl.Label = TimeCalc(endCut);
			}
			if (control == addBtn)
			{
				if (startCut < endCut)
				{
					cutList.Add((startCut.ToString() + ":" + endCut.ToString()));
					videoListLct.Add(new GUIListItem(TimeCalc(startCut) + " - " + TimeCalc(endCut)));
					durationNew += (endCut - startCut);
					newLenghtLbl.Label = TimeCalc(durationNew);
					startPosLbl.Label = "";
					endPosLbl.Label = "";
					if (iCount < NR_OF_SPILTER_TIME_STAMPS)
					{
						tStamp[iCount].s_sec = (int)startCut;
						tStamp[iCount].s_min = tStamp[iCount].s_sec / 60;
						tStamp[iCount].s_hour = tStamp[iCount].s_min / 60;
						tStamp[iCount].s_min = tStamp[iCount].s_min % 60;
						tStamp[iCount].s_sec = tStamp[iCount].s_sec % 60;

						tStamp[iCount].e_sec = (int)endCut;
						tStamp[iCount].e_min = tStamp[iCount].e_sec / 60;
						tStamp[iCount].e_hour = tStamp[iCount].e_min / 60;
						tStamp[iCount].e_min = tStamp[iCount].e_min % 60;
						tStamp[iCount].e_sec = tStamp[iCount].e_sec % 60;
						iCount++;
					}             
				}
			}
			if (control == forwardBtn)
			{

				g_Player.SeekAbsolute((double) (g_Player.CurrentPosition +  1.0)); //org 1.5
				//System.Windows.Forms.MessageBox.Show("Test");
			}
			if (control == backwardBtn)
			{

				g_Player.SeekAbsolute((double) (g_Player.CurrentPosition -  1.0)); //org 1.0
				//System.Windows.Forms.MessageBox.Show("Test");
			}
			if (control == positionSld)
			{
				//System.Windows.Forms.MessageBox.Show(postitionSld.Percentage);
				//g_Player.PauseGraph();
				//vmrPlayer.SeekAbsolute(postitionSld.FloatValue);
				//g_Player.SeekAbsolute(postitionSld.FloatValue);
				g_Player.SeekAbsolute((double)((durationOld / 100) * positionSld.Percentage));
				//g_Player.ContinueGraph();
			}
			//postitionSld.Percentage = (int)((100 / dauerAlt) * g_Player.CurrentPosition);
			string temp = TimeCalc(g_Player.CurrentPosition);
			//aktPosition;
			currentPosLbl.Label = temp;//aktPosition.ToString();
			base.OnClicked(controlId, control, actionType);
		}
	#endregion

		enum FileTypes
		{
			Unknown,
			Dvrms,
			Mpeg,
			
		}

		private void GetFiletype()
		{
			switch (inFilename.Extension)
			{
				case ".dvr-ms":
					cutType = FileTypes.Dvrms;
					break;
				case ".mpeg":
					cutType = FileTypes.Mpeg;
					break;
				case ".mpg":
					cutType = FileTypes.Mpeg;
					break;
				default:
					cutType = FileTypes.Unknown;
					break;
			}

		}
		
		public string CutFileName
		{
			get
			{
				return inFilename.FullName;
			}
			set
			{
				inFilename = new FileInfo(value);
				GetFiletype();
			}
		}

		private void MessageBox(string text, string title)
		{
			GUIDialogOK dlg = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
			dlg.SetHeading(title);
			dlg.SetLine(1, text);
			dlg.SetLine(2, String.Empty);
			dlg.SetLine(3, String.Empty);
			dlg.DoModal(GUIWindowManager.ActiveWindow);
		}

		#region cutmethods
		protected void Cut()
		{
			switch (cutType)
			{
				case FileTypes.Dvrms:
					//cutThread = new Thread(new ThreadStart(CutDvrms));
					//cutThread.Start();
					CutDvrms();
					break;
				case FileTypes.Mpeg:
					CutMpeg();
					break;
				default:
					MessageBox("Unsupported filetype","Cannot cut");
					break;
			}
			
		}

		private void CutDvrms()
		{
			g_Player.Release();
			
			try
			{
				recCompcut = (IStreamBufferRecComp)DShowNET.Helper.ClassId.CoCreateInstance(DShowNET.Helper.ClassId.RecComp);

				if (recCompcut != null)
				{
					//to not to change the database the outputfile has the same name 
					outFilename = new FileInfo(inFilename.FullName);
					//rename the source file ------------later this could be configurable to delete it
					//TODO behavior if the renamed sourcefile (_original) exists
					inFilename.MoveTo(inFilename.FullName.Replace(".dvr-ms", "_original.dvr-ms"));
					
					if (outFilename.Exists)
					{
						outFilename.Delete();
					}
					recCompcut.Initialize(outFilename.FullName, inFilename.FullName);
					for (int i = 0; i < cutList.Count; i++)
					{
						string[] split = cutList[i].ToString().Split(new char[] { ':' });
						startCut = System.Convert.ToDouble(split[0]);
						endCut = System.Convert.ToDouble(split[1]);
						recCompcut.AppendEx(inFilename.FullName, (long)(startCut * 10000000), (long)(endCut * 10000000));
					}
					recCompcut.Close();
					Marshal.ReleaseComObject((object)recCompcut);
					MessageBox("Dvrms:Finished to cut the video file", "Finished");
					
				}
			}
			catch (Exception e)
			{
				Log.WriteFile(Log.LogType.Error,"DvrMpegCut: (CutDvrms) "+ e.StackTrace);
			}
		}

		private void CutMpeg()
		{
			g_Player.Release();
			outFilename = new FileInfo(inFilename.FullName);
			int tmp = inFilename.FullName.LastIndexOf('.');
			string newInFilename = inFilename.FullName.Remove(tmp)+"_original"+inFilename.Extension;
			inFilename.MoveTo(newInFilename);
			Mpeg2Splitter cMpeg2Splitter = new Mpeg2Splitter();
            if (eMode == EMode.E_CUT)
            {
                cMpeg2Splitter.Cut(inFilename.FullName, outFilename.FullName, ref tStamp, iCount);
            }
            else
            {
                cMpeg2Splitter.Trim(inFilename.FullName, outFilename.FullName, ref tStamp[0]);
            }
            MessageBox("Mpeg:Finished to cut the video file", "Finished");
		}
		#endregion

		/// <summary>
		/// Converts the time in sec to hh:mm:ss format
		/// </summary>
		/// <param name="timeSec">time in sec</param>
		/// <returns>time in hh:mm:ss</returns>
		private string TimeCalc(double timeSec)
		{
			double hr, min, sec;
			string hr_ = "", min_ = "", sec_ = "";
			//calc min
			min = timeSec / 60;
			//only sec
			if (min < 1)
			{
				sec = timeSec;
				hr_ = "00";
				min_ = "00";
				if (sec < 10)
					sec_ = "0" + Convert.ToInt32(sec).ToString();
				else
					sec_ = Convert.ToInt32(sec).ToString();
			}
			//lower than one hour
			if (min >= 1 && min < 60)
			{
				sec = timeSec % 60;
				hr_ = "00";
				if (min < 10)
					min_ = "0" + Convert.ToInt32(min).ToString();
				else
					min_ = Convert.ToInt32(min).ToString();
				if (sec < 10)
					sec_ = "0" + Convert.ToInt32(sec).ToString();
				else
					sec_ = Convert.ToInt32(sec).ToString();
			}
			//more than one hour
			if (min >= 60)
			{
				sec = timeSec % 60;
				hr = min / 60;
				min = min % 60;
				if (min < 10)
					min_ = "0" + Convert.ToInt32(min).ToString();
				else
					min_ = Convert.ToInt32(min).ToString();
				if (sec < 10)
					sec_ = "0" + Convert.ToInt32(sec).ToString();
				else
					sec_ = Convert.ToInt32(sec).ToString();
				hr_ = Convert.ToInt32(hr).ToString();
			}
			
			return hr_ + ":" + min_ + ":" + sec_;
		}
	}
}
