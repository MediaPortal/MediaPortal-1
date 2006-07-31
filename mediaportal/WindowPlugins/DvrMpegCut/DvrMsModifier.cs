using System;
using System.Collections.Generic;
using System.Text;
using DirectShowLib.SBE;
using DirectShowLib;
using MediaPortal.Utils.Services;
using System.Runtime.InteropServices;

namespace WindowPlugins.DvrMpegCut
{
  class DvrMsModifier
  {
    IStreamBufferRecComp recCompcut = null;
    ILog log;
    System.Timers.Timer progressTime;
    public delegate void Finished();
    public event Finished OnFinished;
    public delegate void Progress(int percentage);
    public event Progress OnProgress;
    int percent = 0;
    double newDuration = 0;
    System.IO.FileInfo inFilename;
    List<TimeDomain> cutPoints;

    public DvrMsModifier()
    {
      
      ServiceProvider services = GlobalServiceProvider.Instance;
      log = services.Get<ILog>();
      progressTime = new System.Timers.Timer(1000);
      progressTime.Elapsed += new System.Timers.ElapsedEventHandler(progressTime_Elapsed);
    }

    void progressTime_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      int progress = 0;
      if(recCompcut != null)
        recCompcut.GetCurrentLength(out progress);
      percent = System.Convert.ToInt32((progress * 100) / newDuration);
     // progressBar.Percentage = percent;
      //progressLbl.Label = percent.ToString();
      if (OnProgress != null)
        OnProgress(percent);
    }

    public void CutDvr(System.IO.FileInfo inFilename, List<TimeDomain> cutPoints)
    {
      this.inFilename = inFilename;
      this.cutPoints = cutPoints;
      CutDvr();
    }

    public void CutDvr()
    {
      try
      {
        recCompcut = (IStreamBufferRecComp)DShowNET.Helper.ClassId.CoCreateInstance(DShowNET.Helper.ClassId.RecComp);
        if (recCompcut != null)
        {
          System.IO.FileInfo outFilename;
					percent = 0;
          //CutProgressTime();
          progressTime.Start();
          string outPath = inFilename.FullName;
          //rename the source file ------------later this could be configurable to delete it
          //TODO behavior if the renamed sourcefile (_original) exists
          inFilename.MoveTo(inFilename.FullName.Replace(".dvr-ms", "_original.dvr-ms"));
          //to not to change the database the outputfile has the same name 
          outFilename = new System.IO.FileInfo(outPath);


          if (outFilename.Exists)
          {
            outFilename.Delete();
          }
          recCompcut.Initialize(outFilename.FullName, inFilename.FullName);
          for (int i = 0; i < cutPoints.Count; i++)
          {
            //string[] split = cutList[i].ToString().Split(new char[] { ':' });
            //startCut = cutPoints[i].StartTime;
            //endCut = cutPoints[i].EndTime;
            newDuration += cutPoints[i].Duration;     //<---- nicht sehr schön
            recCompcut.AppendEx(inFilename.FullName, (long)(cutPoints[i].StartTime * 10000000), (long)(cutPoints[i].EndTime * 10000000));
          } 
					progressTime.Stop();
          recCompcut.Close();
					percent = 100;

          if (OnFinished != null)
            OnFinished();
          //cutFinished = true;
          //progressLbl.Label = "100";
          //progressBar.Percentage = 100;
          //MessageBox(GUILocalizeStrings.Get(2083), GUILocalizeStrings.Get(2111)); //Dvrms:Finished to cut the video file , Finished !
          //progressBar.IsVisible = false;
          //progressLbl.IsVisible = false;

        }
      }
      catch (Exception ex)
      {
        log.Error(ex);
      }
      finally
      {
        Marshal.ReleaseComObject((object)recCompcut);
        recCompcut = null;
        cutPoints = null;
        percent = 0;
        newDuration = 0;
        //progressTime.Stop();
      }
    }

    public void JoinDvr(System.IO.FileInfo firstFile, System.IO.FileInfo secondFile)
    {
			try
			{
				recCompcut = (IStreamBufferRecComp)DShowNET.Helper.ClassId.CoCreateInstance(DShowNET.Helper.ClassId.RecComp);
				if (recCompcut != null)
				{
					System.IO.FileInfo outFilename;
					percent = 0;
					//progressTime.Start();
					string outPath = firstFile.FullName.Replace(".dvr-ms", "_joined.dvr-ms");
					//rename the source file ------------later this could be configurable to delete it
					//TODO behavior if the renamed sourcefile (_original) exists
					//inFilename.MoveTo(inFilename.FullName.Replace(".dvr-ms", "_original.dvr-ms"));
					//to not to change the database the outputfile has the same name 
					outFilename = new System.IO.FileInfo(outPath);


					if (outFilename.Exists)
					{
						outFilename.Delete();
					}
					recCompcut.Initialize(outFilename.FullName, firstFile.FullName);
					recCompcut.Append(firstFile.FullName);
					recCompcut.Append(secondFile.FullName);
					recCompcut.Close();
					percent = 100;
					if (OnFinished != null)
						OnFinished();
				}
			}
			catch (Exception ex)
			{
				log.Error(ex);
			}
			finally
			{
				Marshal.ReleaseComObject((object)recCompcut);
				recCompcut = null;
				cutPoints = null;
				percent = 0;
				newDuration = 0;
				//progressTime.Stop();
			}
    }

    public void JoinDvr(List<System.IO.FileInfo> fileList)
    {
			try
			{
				recCompcut = (IStreamBufferRecComp)DShowNET.Helper.ClassId.CoCreateInstance(DShowNET.Helper.ClassId.RecComp);
				if (recCompcut != null)
				{
					System.IO.FileInfo outFilename;
					percent = 0;
					//progressTime.Start();
					string outPath = fileList[0].FullName.Replace(".dvr-ms", "_joined.dvr-ms");
					//rename the source file ------------later this could be configurable to delete it
					//TODO behavior if the renamed sourcefile (_original) exists
					//inFilename.MoveTo(inFilename.FullName.Replace(".dvr-ms", "_original.dvr-ms"));
					//to not to change the database the outputfile has the same name 
					outFilename = new System.IO.FileInfo(outPath);


					if (outFilename.Exists)
					{
						outFilename.Delete();
					}
					recCompcut.Initialize(outFilename.FullName, fileList[0].FullName);
					foreach (System.IO.FileInfo file in fileList)
					{
						recCompcut.Append(file.FullName);
					}
					recCompcut.Close();
					percent = 100;
					if (OnFinished != null)
						OnFinished();
				}
			}
			catch (Exception ex)
			{
				log.Error(ex);
			}
			finally
			{
				Marshal.ReleaseComObject((object)recCompcut);
				recCompcut = null;
				cutPoints = null;
				percent = 0;
				newDuration = 0;
				//progressTime.Stop();
			}
    }


    public System.IO.FileInfo InFilename
    {
      get
      {
        return inFilename;
      }
      set
      {
        inFilename = value;
      }
    }

    public List<TimeDomain> CutPoints
    {
      get
      {
        return cutPoints;
      }
      set
      {
        cutPoints = value;
      }
    }
  }
}
