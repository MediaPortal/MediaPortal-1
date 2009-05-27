#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Drawing;
using System.IO;
using System.Threading;
using System.Web.UI.WebControls;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Util;
using MediaPortal.Video.Database;
using Mpeg2SplitterPackage;
using TsCutterPackage;

namespace WindowPlugins.VideoEditor
{
  internal class VideoEditorPreview : GUIWindow
  {
    private const int windowID = 170602;

    #region GUIControls

    [SkinControl(24)] protected GUIButtonControl cutBtn = null;

    [SkinControl(25)] protected GUIButtonControl cancelBtn = null;

    [SkinControl(30)] protected GUIButtonControl endBtn = null;

    [SkinControl(29)] protected GUIButtonControl startBtn = null;

    [SkinControl(28)] protected GUIButtonControl addBtn = null;

    [SkinControl(31)] protected GUIButtonControl forwardBtn = null;

    [SkinControl(34)] protected GUIButtonControl backwardBtn = null;

    [SkinControl(33)] protected GUILabelControl currentPosLbl = null;

    [SkinControl(35)] protected GUILabelControl oldLenghtLbl = null;

    [SkinControl(36)] protected GUILabelControl newLenghtLbl = null;

    [SkinControl(37)] protected GUILabelControl startPosLbl = null;

    [SkinControl(38)] protected GUILabelControl endPosLbl = null;

    [SkinControl(99)] protected GUIVideoControl videoWindow = null;

    [SkinControl(100)] protected GUISliderControl positionSld = null;

    [SkinControl(101)] protected GUIListControl cutListCtrl = null;

    [SkinControl(102)] protected GUILabelControl progressLbl = null;

    [SkinControl(103)] protected GUIStatusbarControl progressBar = null;

    [SkinControl(104)] protected GUIProgressControl progressBar_ = null;

    [SkinControl(105)] protected GUILabelControl editCutPointsLbl = null;

    #endregion

    #region Own variables

    private double durationOld;
    private double durationNew;
    //double curPosition;
    private double startCut = 0;
    private double endCut = 0;
    private FileInfo inFilename;
    private FileInfo outFilename;
    private FileTypes cutType;
    private Thread cutThread;
    //IStreamBufferRecComp recCompcut = null;
    //System.Timers.Timer cutProgressTime; JoeDalton: unused
    //bool cutFinished = false; //JoeDalton: unused
    private List<TimeDomain> cutPointsList;
    private DvrMsModifier dvrMod;
    private bool goToStartPoint;
    private bool editCutPoint;
    private int editCutPointsIndex;
    private int lastIndexedCutPoint;

    private const int NR_OF_SPILTER_TIME_STAMPS = 40;
    private SPLITTER_TIME_STAMP[] tStamp = new SPLITTER_TIME_STAMP[NR_OF_SPILTER_TIME_STAMPS];
    // private int iCount = 0;

    #endregion

    #region constructor

    public VideoEditorPreview(string filepath)
    {
      try
      {
        if (filepath != string.Empty)
        {
          if (File.Exists(filepath))
          {
            inFilename = new FileInfo(filepath);
            GetFiletype();
          }
          else
          {
            throw new Exception();
          }
        }
        GetID = windowID;
      }
      catch (Exception ex)
      {
        Log.Error("VideoEditor: (DvrMpegCutPreview) " + ex.StackTrace);
      }
    }

    #endregion

    #region overrides

    public override bool Init()
    {
      try
      {
        //iCount = 0;
        return Load(GUIGraphicsContext.Skin + @"\VideoEditorCutScreen.xml");
      }
      catch (Exception ex)
      {
        MessageBox("Error loading skinfile: CutScreen.xml", "Skin Error");
        Log.Error(ex);
        return false;
      }
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
        GUIWindowManager.ActiveWindow = (int) Window.WINDOW_TV;
        if (videoWindow != null)
        {
          GUIGraphicsContext.VideoWindow =
            new Rectangle(videoWindow.XPosition, videoWindow.YPosition, videoWindow.Width, videoWindow.Height);
        }
        g_Player.FullScreen = false;
        g_Player.Play(inFilename.FullName);
        g_Player.Pause();
        durationOld = g_Player.Duration;
        oldLenghtLbl.Label = Utils.SecondsToHMSString((int) durationOld);
        newLenghtLbl.Label = Utils.SecondsToHMSString((int) durationNew);

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
        addBtn.IsEnabled = true;
        GetComSkipCutPoints();
      }
      catch (Exception ex)
      {
        Log.Error("VideoEditor: (OnPageLoad) " + ex.StackTrace);
      }
      //schneideListeLct.Add(new GUIListItem("Test"));
    }

    private void dvrMod_OnFinished()
    {
      progressBar.Percentage = 100;
      //MessageBox(GUILocalizeStrings.Get(2083), GUILocalizeStrings.Get(2111)); //Dvrms:Finished to cut the video file , 
      GUIDialogYesNo yesnoDialog = (GUIDialogYesNo) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_YES_NO);
      yesnoDialog.SetHeading(2111); // Finished !
      yesnoDialog.SetLine(1, 2082); // //Finished to cut the video file
      yesnoDialog.SetLine(2, 2083); // Would you like to delete the original file?
      yesnoDialog.DoModal(GetID);
      if (yesnoDialog.IsConfirmed)
      {
        File.Delete(inFilename.FullName);
      }
      progressBar.IsVisible = false;
      progressBar.Percentage = 0;
      progressLbl.IsVisible = false;
      progressLbl.Label = "0";
      cutListCtrl.Clear();
      cutPointsList.Clear();
      cutBtn.IsEnabled = false;
      addBtn.IsEnabled = false;
      int fileId = VideoDatabase.GetFileId(inFilename.FullName);
      VideoDatabase.SetMovieDuration(fileId, (int) durationNew);
    }

    private void dvrMod_OnProgress(int percentage)
    {
      progressBar.Percentage = percentage;
      progressLbl.Label = percentage.ToString();
    }

    private void tsFileCutter_OnInitFailed()
    {
      MessageBox(GUILocalizeStrings.Get(200051), GUILocalizeStrings.Get(2081)); // Invalid videofile, Cannot cut
    }

    protected override void OnPageDestroy(int new_windowId)
    {
      g_Player.Release();
      inFilename = null;
      base.OnPageDestroy(new_windowId);
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
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
          //cutProgressTime.Stop();  JoeDalton: unused
          MessageBox(GUILocalizeStrings.Get(2091), GUILocalizeStrings.Get(510));
          progressBar.IsVisible = false;
          progressLbl.IsVisible = false;
          cutBtn.IsEnabled = true;
        }
      }
      if (control == startBtn)
      {
        startCut = g_Player.CurrentPosition;
        startPosLbl.Label = Utils.SecondsToHMSString((int) startCut);
      }
      if (control == endBtn)
      {
        endCut = g_Player.CurrentPosition;
        endPosLbl.Label = Utils.SecondsToHMSString((int) endCut);
      }
      if (control == addBtn)
      {
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
            //_log.Info("Days::" + cutPointsList[0].StartTimeSp.Days.ToString() + "::Seconds::" + cutPointsList[0].StartTimeSp.Seconds.ToString(), null);
          }
          else
          {
            cutListCtrl.Add(
              new GUIListItem(Utils.SecondsToHMSString((int) startCut) + " - " + Utils.SecondsToHMSString((int) endCut)));
            durationNew += (endCut - startCut);
            newLenghtLbl.Label = Utils.SecondsToHMSString((int) durationNew);
            cutPointsList.Add(new TimeDomain(startCut, endCut));
          }

          startPosLbl.Label = "";
          endPosLbl.Label = "";
          cutBtn.IsEnabled = true;
        }
        //}
      }
      if (control == forwardBtn)
      {
        double newPos = g_Player.CurrentPosition + 1.3; //org 1.5
        if (newPos > g_Player.Duration - 0.5)
        {
          g_Player.SeekAbsolute(g_Player.Duration - 0.5);
        }
        else
        {
          g_Player.SeekAbsolute(newPos);
        }

        positionSld.Percentage = (int) ((100/durationOld)*(g_Player.CurrentPosition + 1.0));
        g_Player.Pause();
      }
      if (control == backwardBtn)
      {
        g_Player.SeekAbsolute(g_Player.CurrentPosition - 1.0); //org 1.0
        positionSld.Percentage = (int) ((100/durationOld)*g_Player.CurrentPosition);
        g_Player.Pause();
      }
      if (control == positionSld)
      {
        double newPos = (durationOld/100)*positionSld.Percentage;
        if (newPos >= g_Player.Duration - 0.5)
        {
          g_Player.SeekAbsolute(g_Player.Duration - 0.5);
        }
        else
        {
          g_Player.SeekAbsolute(newPos);
        }
        g_Player.Pause();
      }
      if (control == cutListCtrl)
      {
        if (cutListCtrl.SelectedListItem == null)
        {
          return;
        }
        if (lastIndexedCutPoint != cutListCtrl.SelectedListItemIndex)
        {
          goToStartPoint = true;
        }
        if (goToStartPoint)
        {
          g_Player.SeekAbsolute(cutPointsList[cutListCtrl.SelectedListItemIndex].StartTime);
          goToStartPoint = false;
          lastIndexedCutPoint = cutListCtrl.SelectedListItemIndex;
          positionSld.Percentage = (int) ((100/durationOld)*cutPointsList[cutListCtrl.SelectedListItemIndex].StartTime);
          g_Player.Pause();
        }
        else
        {
          g_Player.SeekAbsolute(cutPointsList[cutListCtrl.SelectedListItemIndex].EndTime);
          goToStartPoint = true;
          lastIndexedCutPoint = cutListCtrl.SelectedListItemIndex;
          positionSld.Percentage = (int) ((100/durationOld)*cutPointsList[cutListCtrl.SelectedListItemIndex].EndTime);
          g_Player.Pause();
        }
      }
      //positionSld.Percentage = (int)((100 / durationOld) * g_Player.CurrentPosition);
      base.OnClicked(controlId, control, actionType);
    }

    protected override void OnShowContextMenu()
    {
      if (cutListCtrl.SelectedListItem == null)
      {
        return;
      }

      if (editCutPoint)
      {
        GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
        if (dlg == null)
        {
          return;
        }

        dlg.Reset();
        dlg.SetHeading(498); // menu
        dlg.AddLocalizedString(510); //cancel
        dlg.DoModal(GetID);
        if (dlg.SelectedId == -1)
        {
          return;
        }
        else
        {
          editCutPoint = false;
          editCutPointsLbl.IsVisible = false;
        }
      }
      else
      {
        GUIDialogMenu dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
        if (dlg == null)
        {
          return;
        }
        dlg.Reset();
        dlg.SetHeading(498); // menu
        dlg.AddLocalizedString(117); //Delete
        dlg.AddLocalizedString(2076); //edit
        dlg.DoModal(GetID);
        if (dlg.SelectedId == -1)
        {
          return;
        }
        switch (dlg.SelectedId)
        {
          case 117:
            cutPointsList.RemoveAt(cutListCtrl.SelectedListItemIndex);
            ReloadCutList();
            break;
          case 2076:
            editCutPoint = true;
            //positionSld.Percentage = (int)((100 / durationOld) * cutPointsList[cutListCtrl.SelectedListItemIndex].StartTime);
            //g_Player.SeekAbsolute(cutPointsList[cutListCtrl.SelectedListItemIndex].StartTime);
            editCutPointsIndex = cutListCtrl.SelectedListItemIndex;
            editCutPointsLbl.IsVisible = true;
            //addBtn.Label = GUILocalizeStrings.Get(510);
            break;
        }
        //joinListCtrl.RemoveSubItem(joinListCtrl.SelectedListItemIndex);//joinListCtrl.SelectedListItem.);//SelectedLabelText);
        //System.Windows.Forms.MessageBox.Show(selected.Label + "::" + joinListCtrl.SelectedItem.ToString() + "::" + joinListCtrl.SelectedListItemIndex.ToString());
      }
      if (cutPointsList.Count == 0)
      {
        cutBtn.IsEnabled = false;
      }
    }

    private void ReloadCutList()
    {
      cutListCtrl.Clear();
      durationNew = 0;
      foreach (TimeDomain cutPoints in cutPointsList)
      {
        cutListCtrl.Add(
          new GUIListItem(Utils.SecondsToHMSString((int) cutPoints.StartTime) + " - " +
                          Utils.SecondsToHMSString((int) cutPoints.EndTime)));
        durationNew += (cutPoints.EndTime - cutPoints.StartTime);
      }
      newLenghtLbl.Label = Utils.SecondsToHMSString((int) durationNew);
    }

    #endregion

    private void GetComSkipCutPoints()
    {
      string comskipFilePath = inFilename.FullName.Replace(inFilename.Extension, ".txt");
      try
      {
        if (File.Exists(comskipFilePath))
        {
          GUIDialogYesNo yesnoDialog = (GUIDialogYesNo) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_YES_NO);
          if (yesnoDialog == null)
          {
            return;
          }

          yesnoDialog.SetHeading("Video Editor");
          yesnoDialog.SetLine(1, 2059); //Comskip file found, 
          yesnoDialog.SetLine(2, 2060); //would you like to use it?
          yesnoDialog.DoModal(GetID);
          if (yesnoDialog.IsConfirmed)
          {
            StreamReader comSkipFile = new StreamReader(comskipFilePath);
            string line = comSkipFile.ReadLine();
            string[] lineParts = line.Split(new char[] {'\t', ' '});
            if (lineParts.Length < 1)
            {
              MessageBox(GUILocalizeStrings.Get(2061), "Video Editor"); //The Comskip file is incompatible
              return;
            }
            int framesPerSec = 25;
            double curPos = 0;
            if (BaseCompareValidator.CanConvert(lineParts[lineParts.Length - 1], ValidationDataType.Integer))
            {
              framesPerSec = Convert.ToInt32(lineParts[lineParts.Length - 1])/100;
            }
            comSkipFile.ReadLine();
            while (!comSkipFile.EndOfStream)
            {
              line = comSkipFile.ReadLine();
              lineParts = line.Split(new char[] {'\t', ' '});
              int cutPoint1, cutPoint2;
              double startPoint = 0, endPoint = 0;
              if (BaseCompareValidator.CanConvert(lineParts[0], ValidationDataType.Integer))
              {
                cutPoint1 = Convert.ToInt32(lineParts[0].Trim());
                endPoint = cutPoint1/framesPerSec;
              }
              else
              {
                MessageBox(GUILocalizeStrings.Get(2061), "Video Editor"); //The Comskip file is incompatible
                return;
              }

              if (BaseCompareValidator.CanConvert(lineParts[1], ValidationDataType.Integer))
              {
                cutPoint2 = Convert.ToInt32(lineParts[1].Trim());
                startPoint = cutPoint2/framesPerSec;
              }
              else
              {
                MessageBox(GUILocalizeStrings.Get(2061), "Video Editor"); //The Comskip file is incompatible
                return;
              }
              cutListCtrl.Add(
                new GUIListItem(Utils.SecondsToHMSString((int) curPos) + " - " +
                                Utils.SecondsToHMSString((int) endPoint)));
              durationNew += (endPoint - curPos);
              newLenghtLbl.Label = Utils.SecondsToHMSString((int) durationNew);
              cutPointsList.Add(new TimeDomain(curPos, endPoint));
              curPos = startPoint;
            }
            cutListCtrl.Add(
              new GUIListItem(Utils.SecondsToHMSString((int) curPos) + " - " +
                              Utils.SecondsToHMSString((int) durationOld)));
            durationNew += (durationOld - curPos);
            newLenghtLbl.Label = Utils.SecondsToHMSString((int) durationNew);
            cutPointsList.Add(new TimeDomain(curPos, durationOld));
            cutBtn.IsEnabled = true;
          }
        }
      }
      catch
      {
        MessageBox(GUILocalizeStrings.Get(2061), "Video Editor"); //The Comskip file is incompatible
        return;
      }
    }

    private enum FileTypes
    {
      Unknown,
      Dvrms,
      Mpeg,
      Ts
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
        case ".ts":
          cutType = FileTypes.Ts;
          break;
        default:
          cutType = FileTypes.Unknown;
          break;
      }
    }

    public string CutFileName
    {
      get { return inFilename.FullName; }
      set
      {
        inFilename = new FileInfo(value);
        GetFiletype();
      }
    }

    private void MessageBox(string text, string title)
    {
      GUIDialogOK dlg = (GUIDialogOK) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_OK);
      dlg.SetHeading(title);
      dlg.SetLine(1, text);
      dlg.SetLine(2, string.Empty);
      dlg.SetLine(3, string.Empty);
      dlg.DoModal(GUIWindowManager.ActiveWindow);
    }

    #region cutmethods

    protected void Cut()
    {
      g_Player.Release();
      switch (cutType)
      {
        case FileTypes.Dvrms:
          progressBar.Percentage = 0;
          progressBar.IsVisible = true;
          progressLbl.IsVisible = true;
          cutBtn.IsEnabled = false;
          dvrMod.CutDvr(inFilename, cutPointsList);
          // cutThread.Start();
          // CutDvrms();
          break;
        case FileTypes.Mpeg:
          for (int iCount = 0; iCount < cutPointsList.Count; iCount++)
          {
            if (iCount < NR_OF_SPILTER_TIME_STAMPS)
            {
              tStamp[iCount].start = new DateTime(1900, 1, 1, 0, 0, 0, 0);
              tStamp[iCount].start = tStamp[iCount].start.AddSeconds((int) cutPointsList[iCount].StartTime);
              tStamp[iCount].end = new DateTime(1900, 1, 1, 0, 0, 0, 0);
              tStamp[iCount].end = tStamp[iCount].end.AddSeconds((int) cutPointsList[iCount].EndTime);
            }
          }
          cutThread = new Thread(new ThreadStart(CutMpeg));
          cutThread.Priority = ThreadPriority.BelowNormal;
          cutThread.Name = "MpegCutter";

          progressBar.Percentage = 0;
          progressBar.IsVisible = true;
          progressLbl.IsVisible = true;
          cutBtn.IsEnabled = false;
          cutThread.Start();
          break;
        case FileTypes.Ts:
          cutThread = new Thread(new ThreadStart(CutTs));
          cutThread.Priority = ThreadPriority.BelowNormal;
          cutThread.Name = "TsCutter";

          progressBar.Percentage = 0;
          progressBar.IsVisible = true;
          progressLbl.IsVisible = true;
          cutBtn.IsEnabled = false;
          cutThread.Start();
          break;
        default:
          MessageBox(GUILocalizeStrings.Get(2080), GUILocalizeStrings.Get(2081)); // Unsupported filetype, Cannot cut
          break;
      }
    }

    private void CutTs()
    {
      outFilename = new FileInfo(inFilename.FullName);
      int tmp = inFilename.FullName.LastIndexOf('.');
      string newInFilename = inFilename.FullName.Remove(tmp) + "_original" + inFilename.Extension;
      inFilename.MoveTo(newInFilename);

      // TsFileCutter expects the cutpoint as the intervalls to cut out, currently the intervalls show the parts to keep
      // so we have to "revert" the cutPointsList"
      List<TimeDomain> cutList = new List<TimeDomain>();
      if (cutPointsList.Count == 1)
      {
        if (cutPointsList[0].StartTime == 0)
        {
          cutList.Add(new TimeDomain(cutPointsList[0].EndTime, durationOld));
        }
        else
        {
          cutList.Add(new TimeDomain(0, cutPointsList[0].StartTime));
          cutList.Add(new TimeDomain(cutPointsList[0].EndTime, durationOld));
        }
      }
      else
      {
        if (cutPointsList[0].StartTime != 0)
        {
          cutList.Add(new TimeDomain(0, cutPointsList[0].StartTime));
        }
        for (int i = 1; i < cutPointsList.Count; i++)
        {
          cutList.Add(new TimeDomain(cutPointsList[i - 1].EndTime, cutPointsList[i].StartTime));
        }
        // Don't add the last cutpoint if the last endtime is the end of the file
        if ((int) cutPointsList[cutPointsList.Count - 1].EndTime != (int) durationOld)
        {
          cutList.Add(new TimeDomain(cutPointsList[cutPointsList.Count - 1].EndTime, durationOld));
        }
      }
      Log.Info("Cutpointslist:");
      foreach (TimeDomain td in cutPointsList)
      {
        Log.Info("  " + td.StartTimeSp.Minutes.ToString() + ":" + td.StartTimeSp.Seconds.ToString() + " - " +
                 td.EndTimeSp.Minutes.ToString() + ":" + td.EndTimeSp.Seconds.ToString());
      }
      Log.Info("\"negative\" cutpoints");
      foreach (TimeDomain td in cutList)
      {
        Log.Info("  " + td.StartTimeSp.Minutes.ToString() + ":" + td.StartTimeSp.Seconds.ToString() + " - " +
                 td.EndTimeSp.Minutes.ToString() + ":" + td.EndTimeSp.Seconds.ToString());
      }

      TsFileCutter cutter = new TsFileCutter();
      cutter.InitStreams(inFilename.FullName, outFilename.FullName, cutList);
      cutter.OnProgress += dvrMod_OnProgress;
      cutter.OnFinished += dvrMod_OnFinished;
      cutter.OnInitFailed += tsFileCutter_OnInitFailed;
      cutter.Cut();
      progressLbl.Label = "0";
      progressBar.Percentage = 0;
    }

    private void CutMpeg()
    {
      outFilename = new FileInfo(inFilename.FullName);
      int tmp = inFilename.FullName.LastIndexOf('.');
      string newInFilename = inFilename.FullName.Remove(tmp) + "_original" + inFilename.Extension;
      inFilename.MoveTo(newInFilename);
      Mpeg2Splitter cMpeg2Splitter = new Mpeg2Splitter();
      cMpeg2Splitter.OnProgress += new Mpeg2Splitter.Progress(dvrMod_OnProgress);
      cMpeg2Splitter.OnFinished += new Mpeg2Splitter.Finished(dvrMod_OnFinished);
      cMpeg2Splitter.Scene(inFilename.FullName, outFilename.FullName, ref tStamp, cutPointsList.Count);
      progressLbl.Label = "100";
      progressBar.Percentage = 100;
    }

    #endregion
  }
}