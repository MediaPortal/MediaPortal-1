#region Copyright (C) 2005-2006 Team MediaPortal

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

#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Collections;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal;
using DirectShowLib.SBE;
using DirectShowLib;
using DShowNET.Helper;
using System.Runtime.InteropServices;
using Mpeg2SplitterPackage;

namespace WindowPlugins.DvrMpegCut
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
    protected GUIListControl cutListCtrl = null;
    [SkinControlAttribute(102)]
    protected GUILabelControl progressLbl = null;
    [SkinControlAttribute(103)]
    protected GUIStatusbarControl progressBar = null;
    [SkinControlAttribute(104)]
    protected GUIProgressControl progressBar_ = null;
		[SkinControlAttribute(105)]
		protected GUILabelControl editCutPointsLbl = null;
    #endregion

    #region Own variables
    double durationOld;
    double durationNew;
    //double curPosition;
    double startCut = 0;
    double endCut = 0;
    FileInfo inFilename;
    FileInfo outFilename;
    FileTypes cutType;
    Thread cutThread;
    IStreamBufferRecComp recCompcut = null;
    System.Timers.Timer cutProgressTime;
    bool cutFinished = false;
    List<TimeDomain> cutPointsList;
    DvrMsModifier dvrMod;
		bool goToStartPoint;
		bool editCutPoint;
		int editCutPointsIndex;
		int lastIndexedCutPoint;
   // int videoLength;

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
        Log.Error("DvrMpegCut: (DvrMpegCutPreview) " + ex.StackTrace);
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
          //Log.Info("Test " +videoWindow.XPosition  + " " + videoWindow.Width + " " + videoWindow.Height);
        }
        g_Player.FullScreen = false;
        g_Player.Play(inFilename.FullName);
        g_Player.Pause();
        durationOld = g_Player.Duration;
        oldLenghtLbl.Label = MediaPortal.Util.Utils.SecondsToHMSString((int)durationOld);
        newLenghtLbl.Label = MediaPortal.Util.Utils.SecondsToHMSString((int)durationNew);
        //postitionSld.Percentage = 100;
        //postitionSld.SpinType = GUISpinControl.SpinType.SPIN_CONTROL_TYPE_FLOAT;
        //positionSld.SetFloatRange(0, (float)durationOld);
       // positionSld.FloatInterval = (float)0.5;
       // positionSld.SpinType = GUISpinControl.SpinType.Float;
        positionSld.Percentage = 0;
        progressBar.Percentage = 0;
        progressBar.IsVisible = false;
        progressLbl.Label = "0";
        progressLbl.IsVisible = false;
        progressBar_.Percentage = 50;
        progressBar_.IsVisible = false;
				editCutPointsLbl.IsVisible = false;
        cutPointsList = new List<TimeDomain>();
        dvrMod = new DvrMsModifier();
        dvrMod.OnProgress += new DvrMsModifier.Progress(dvrMod_OnProgress);
        dvrMod.OnFinished += new DvrMsModifier.Finished(dvrMod_OnFinished);
				goToStartPoint = true;
				lastIndexedCutPoint = 0;
				cutBtn.IsEnabled = false;
				editCutPoint = false;
				editCutPointsIndex = 0;
      }
      catch (Exception ex)
      {
        Log.Error("DvrMpegCut: (OnPageLoad) " + ex.StackTrace);
      }
      //schneideListeLct.Add(new GUIListItem("Test"));
    }

    void dvrMod_OnFinished()
    {
      MessageBox(GUILocalizeStrings.Get(2083), GUILocalizeStrings.Get(2111)); //Dvrms:Finished to cut the video file , Finished !
      progressBar.IsVisible = false;
      progressBar.Percentage = 0;
      progressLbl.IsVisible = false;
      progressLbl.Label = "0";
			cutListCtrl.Clear();
			cutPointsList.Clear();
    }

    void dvrMod_OnProgress(int percentage)
    {
      progressBar.Percentage = percentage;
      progressLbl.Label = percentage.ToString();
    }

    protected override void OnPageDestroy(int new_windowId)
    {
      g_Player.Release();
      inFilename = null;
      //vmrPlayer.Release();
      base.OnPageDestroy(new_windowId);
    }

    public override void OnAction(Action action)
    {
      //code send by Davide
     /* switch (action.wID)
      {
        case Action.ActionType.ACTION_STOP:
          g_Player.Stop();
          break;
        case Action.ActionType.ACTION_PAUSE:
          g_Player.Pause();
          break;
        case Action.ActionType.ACTION_PLAY:
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
          break;
        case Action.ActionType.ACTION_REWIND:
          g_Player.SeekAbsolute((double)(g_Player.CurrentPosition - 5.0));
          break;
        case Action.ActionType.ACTION_STEP_BACK:
          g_Player.SeekAbsolute((double)(g_Player.CurrentPosition - 30.0));
          break;
        case Action.ActionType.ACTION_PREV_ITEM:
          g_Player.SeekAbsolute((double)(g_Player.CurrentPosition - 2.0));
          break;
        case Action.ActionType.ACTION_FORWARD:
          g_Player.SeekAbsolute((double)(g_Player.CurrentPosition + 5.0));
          break;
        case Action.ActionType.ACTION_STEP_FORWARD:
          g_Player.SeekAbsolute((double)(g_Player.CurrentPosition + 30.0));
          break;
        case Action.ActionType.ACTION_NEXT_ITEM:
          g_Player.SeekAbsolute((double)(g_Player.CurrentPosition + 2.0));
          break;
      }*/

       base.OnAction(action);
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
					cutProgressTime.Stop();
					MessageBox(GUILocalizeStrings.Get(2091), GUILocalizeStrings.Get(510));
					progressBar.IsVisible = false;
					progressLbl.IsVisible = false;
					cutBtn.IsEnabled = true;
				}
			}
			if (control == startBtn)
			{
				startCut = g_Player.CurrentPosition;
				startPosLbl.Label = MediaPortal.Util.Utils.SecondsToHMSString((int)startCut);
			}
			if (control == endBtn)
			{
				endCut = g_Player.CurrentPosition;
				endPosLbl.Label = MediaPortal.Util.Utils.SecondsToHMSString((int)endCut);
			}
			if (control == addBtn)
			{
				/*if (addBtn.Label == GUILocalizeStrings.Get(510)) //cancel
				{
					editCutPointsLbl.IsVisible = false;
					editCutPoint = false;
					addBtn.Label = GUILocalizeStrings.Get(2093);
				}
				else
				{*/
				if (startCut < endCut)
				{
					// cutList.Add((startCut.ToString() + ":" + endCut.ToString()));
					if (editCutPoint)
					{
						cutPointsList.RemoveAt(editCutPointsIndex);
						cutPointsList.Insert(editCutPointsIndex, new TimeDomain(startCut, endCut));
						ReloadCutList();
						editCutPointsLbl.IsVisible = false;
						editCutPoint = false;
					}
					else
					{
						cutListCtrl.Add(new GUIListItem(MediaPortal.Util.Utils.SecondsToHMSString((int)startCut) + " - " + MediaPortal.Util.Utils.SecondsToHMSString((int)endCut)));
						durationNew += (endCut - startCut);
						newLenghtLbl.Label = MediaPortal.Util.Utils.SecondsToHMSString((int)durationNew);
						cutPointsList.Add(new TimeDomain(startCut, endCut));
					}
					startPosLbl.Label = "";
					endPosLbl.Label = "";
					cutBtn.IsEnabled = true;
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
				//}
			}
			if (control == forwardBtn)
			{
				g_Player.SeekAbsolute((double)(g_Player.CurrentPosition + 1.3)); //org 1.5
				positionSld.Percentage = (int)((100 / durationOld) * (g_Player.CurrentPosition + 1.0)); 
			}
			if (control == backwardBtn)
			{
				g_Player.SeekAbsolute((double)(g_Player.CurrentPosition - 1.0)); //org 1.0
				positionSld.Percentage = (int)((100 / durationOld) * g_Player.CurrentPosition);
			}
			if (control == positionSld)
			{
				g_Player.SeekAbsolute((double)((durationOld / 100) * positionSld.Percentage));
			}
			if (control == cutListCtrl)
			{
				if (cutListCtrl.SelectedListItem == null)
					return;
				if (lastIndexedCutPoint != cutListCtrl.SelectedListItemIndex)
				{
					goToStartPoint = true;
				}
				if (goToStartPoint)
				{
					g_Player.SeekAbsolute(cutPointsList[cutListCtrl.SelectedListItemIndex].StartTime);
					goToStartPoint = false;
					lastIndexedCutPoint = cutListCtrl.SelectedListItemIndex;
					positionSld.Percentage = (int)((100 / durationOld) * cutPointsList[cutListCtrl.SelectedListItemIndex].StartTime);
				}
				else
				{
					g_Player.SeekAbsolute(cutPointsList[cutListCtrl.SelectedListItemIndex].EndTime);
					goToStartPoint = true;
					lastIndexedCutPoint = cutListCtrl.SelectedListItemIndex;
					positionSld.Percentage = (int)((100 / durationOld) * cutPointsList[cutListCtrl.SelectedListItemIndex].EndTime);
				}
			}
			//positionSld.Percentage = (int)((100 / durationOld) * g_Player.CurrentPosition);
			base.OnClicked(controlId, control, actionType);
		}

    protected override void OnShowContextMenu()
    {
			if (cutListCtrl.SelectedListItem == null) return;
			if (editCutPoint)
			{
				GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
				if (dlg == null) return;
				dlg.Reset();
				dlg.SetHeading(924); // menu
				dlg.AddLocalizedString(510);  //cancel
				dlg.DoModal(GetID);
				if (dlg.SelectedId == -1) return;
				else
				{
					editCutPoint = false;
					editCutPointsLbl.IsVisible = false;
				}
			}
			else
			{
				GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
				if (dlg == null) return;
				dlg.Reset();
				dlg.SetHeading(924); // menu
				dlg.AddLocalizedString(117);  //Delete
				dlg.AddLocalizedString(2076); //edit
				dlg.DoModal(GetID);
				if (dlg.SelectedId == -1) return;
				switch (dlg.SelectedId)
				{
					case 117:
						cutPointsList.RemoveAt(cutListCtrl.SelectedListItemIndex);
						ReloadCutList();
						break;
					case 2076:
						editCutPoint = true;
						positionSld.Percentage = (int)((100 / durationOld) * cutPointsList[cutListCtrl.SelectedListItemIndex].StartTime);
						g_Player.SeekAbsolute(cutPointsList[cutListCtrl.SelectedListItemIndex].StartTime);
						editCutPointsIndex = cutListCtrl.SelectedListItemIndex;
						editCutPointsLbl.IsVisible = true;
						//addBtn.Label = GUILocalizeStrings.Get(510);
						break;
				}
				//joinListCtrl.RemoveSubItem(joinListCtrl.SelectedListItemIndex);//joinListCtrl.SelectedListItem.);//SelectedLabelText);
				//System.Windows.Forms.MessageBox.Show(selected.Label + "::" + joinListCtrl.SelectedItem.ToString() + "::" + joinListCtrl.SelectedListItemIndex.ToString());
			}
			if (cutPointsList.Count == 0)
				cutBtn.IsEnabled = false;
    }

		private void ReloadCutList()
		{
			cutListCtrl.Clear();
			durationNew = 0;
			foreach (TimeDomain cutPoints in cutPointsList)
			{
				cutListCtrl.Add(new GUIListItem(MediaPortal.Util.Utils.SecondsToHMSString((int)cutPoints.StartTime) + " - " + MediaPortal.Util.Utils.SecondsToHMSString((int)cutPoints.EndTime)));
				durationNew += (cutPoints.EndTime - cutPoints.StartTime);
			}
			newLenghtLbl.Label = MediaPortal.Util.Utils.SecondsToHMSString((int)durationNew);
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
      g_Player.Release();
      switch (cutType)
      {
        case FileTypes.Dvrms:
          dvrMod.CutPoints = cutPointsList;
          dvrMod.InFilename = inFilename;
          cutThread = new Thread(new ThreadStart(dvrMod.CutDvr));//CutDvrms));
          //cutThread.SetApartmentState(ApartmentState.STA);
          //cutThread.IsBackground = true;
          cutThread.Priority = ThreadPriority.BelowNormal;
          progressBar.Percentage = 0;
          progressBar.IsVisible = true;
          progressLbl.IsVisible = true;
					cutBtn.IsEnabled = false;
          cutThread.Start();
         // CutDvrms();
          break;
        case FileTypes.Mpeg:
          cutThread = new Thread(new ThreadStart(CutMpeg));
          cutThread.Priority = ThreadPriority.BelowNormal;
          cutThread.Start();
					cutBtn.IsEnabled = false;
          //CutMpeg();
          break;
        default:
          MessageBox(GUILocalizeStrings.Get(2080), GUILocalizeStrings.Get(2081)); // Unsupported filetype, Cannot cut
          break;
      }

    }

    /*private void CutDvrms()
    {
      try
      {
        recCompcut = (IStreamBufferRecComp)DShowNET.Helper.ClassId.CoCreateInstance(DShowNET.Helper.ClassId.RecComp);
        if (recCompcut != null)
        {
          CutProgressTime();
          string outPath = inFilename.FullName;
          //rename the source file ------------later this could be configurable to delete it
          //TODO behavior if the renamed sourcefile (_original) exists
          inFilename.MoveTo(inFilename.FullName.Replace(".dvr-ms", "_original.dvr-ms"));
          //to not to change the database the outputfile has the same name 
          outFilename = new FileInfo(outPath);
         

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
          cutFinished = true;
          progressLbl.Label = "100";
          progressBar.Percentage = 100;
          MessageBox(GUILocalizeStrings.Get(2083), GUILocalizeStrings.Get(2111)); //Dvrms:Finished to cut the video file , Finished !
          progressBar.IsVisible = false;
          progressLbl.IsVisible = false;

        }
      }
      catch (Exception e)
      {
        Log.Error("DvrMpegCut: (CutDvrms) " + e.StackTrace);
        if (cutProgressTime != null)
        {
          cutProgressTime.Stop();
          progressBar.IsVisible = false;
          progressLbl.IsVisible = false;
        }
      }
    }*/

    private void CutMpeg()
    {
      outFilename = new FileInfo(inFilename.FullName);
      int tmp = inFilename.FullName.LastIndexOf('.');
      string newInFilename = inFilename.FullName.Remove(tmp) + "_original" + inFilename.Extension;
      inFilename.MoveTo(newInFilename);
      Mpeg2Splitter cMpeg2Splitter = new Mpeg2Splitter();
      //CutProgressTime();
      if (eMode == EMode.E_CUT)
      {
        cMpeg2Splitter.Cut(inFilename.FullName, outFilename.FullName, ref tStamp, iCount);
      }
      else
      {
        cMpeg2Splitter.Trim(inFilename.FullName, outFilename.FullName, ref tStamp[0]);
      }
      cutFinished = true;
      progressLbl.Label = "100";
      progressBar.Percentage = 100;
      MessageBox(GUILocalizeStrings.Get(2082), GUILocalizeStrings.Get(2111));
      progressBar.IsVisible = false;
      progressLbl.IsVisible = false;
			cutBtn.IsEnabled = true;
    }

  /*  private void CutProgressTime()
    {
      cutFinished = false;
      cutProgressTime = new System.Timers.Timer(1000);
      cutProgressTime.Elapsed += new System.Timers.ElapsedEventHandler(cutProgressTime_Elapsed);
      progressBar.Percentage = 0;
      progressBar.IsVisible = true;
      progressLbl.IsVisible = true;
      cutProgressTime.Start();
    }

    void cutProgressTime_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      if (!cutFinished)
      {
        int progress;
        recCompcut.GetCurrentLength(out progress);
        int percent = System.Convert.ToInt32((progress * 100) / durationNew);
        progressBar.Percentage = percent;
        progressLbl.Label = percent.ToString();
        
      }
      else
        cutProgressTime.Stop();
    }*/
    #endregion

    #region obsolete
    /// <summary>
    /// Converts the time in sec to hh:mm:ss format
    /// </summary>
    /// <remarks>now using: MediaPortal.Util.Utils.SecondsToHMSString()</remarks>
    /// <param name="timeSec">time in sec</param>
    /// <returns>time in hh:mm:ss</returns>
    private string TimeCalc(double timeSec)
    {
      int hr, min, sec;
      string hr_ = "", min_ = "", sec_ = "";
      //calc min
      min = (int)timeSec / 60;
      //only sec
      if (min < 1)
      {
        sec = (int)timeSec;
        hr_ = "00";
        min_ = "00";
        if (sec < 10)
          sec_ = "0" + Convert.ToInt32(sec).ToString();
        else
          sec_ = Convert.ToInt32(sec).ToString();
      }
      //less than one hour
      if (min >= 1 && min < 60)
      {
        sec = (int)timeSec % 60;
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
        sec = (int)timeSec % 60;
        hr = (int)min / 60;
        min = (int)min % 60;
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
    #endregion
  }
}
