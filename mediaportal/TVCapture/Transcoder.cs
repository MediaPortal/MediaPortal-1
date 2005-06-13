using System;
using System.Drawing;
using System.Collections;
using System.Threading;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Database;
using MediaPortal.Core.Transcoding;
using MediaPortal.Util;
namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// Summary description for Transcoder.
	/// </summary>
	public class Transcoder
	{
		public class TranscoderInfo
		{
			public TVRecorded recorded;
			public Status     status;
			public int        percentDone;
			public TranscoderInfo(TVRecorded recording)
			{
				recorded=recording;
				status=Status.Waiting;
				percentDone=0;
			}
		}
		public enum Status
		{
			Waiting,
			Busy,
			Completed,
			Error
		}
		enum Quality
		{
			Portable=0,
			Low,
			Medium,
			High,
			Custom
		}
		static ArrayList  queue = new ArrayList();
		static Thread		  WorkerThread =null;
		
		static Transcoder()
		{
		}

		static public void Transcode(TVRecorded rec)
		{
			lock(queue)
			{
				TranscoderInfo info = new TranscoderInfo(rec);
				queue.Add(info);
			}

			if (WorkerThread==null)
			{
				WorkerThread = new Thread(new ThreadStart(TranscodeWorkerThread));
				WorkerThread.Start();
			}
		}

		static public void Clear()
		{
			lock(queue)
			{
				bool deleted=false;
				do
				{
					deleted=false;
					for (int i=0; i < queue.Count;++i)
					{
						TranscoderInfo info = (TranscoderInfo)queue[i];
						if (info.status==Status.Error || info.status==Status.Completed)
						{
							deleted=true;
							queue.RemoveAt(i);
							break;
						}
					}
				} while (deleted);
			}
		}
		static public ArrayList Queue
		{
			get
			{
				return queue;
			}
		}
		static public bool IsTranscoding(TVRecorded rec)
		{
			lock(queue)
			{
				foreach (TranscoderInfo info in queue)
				{
					if (info.status==Status.Error || info.status==Status.Completed) continue;
					if (info.recorded.FileName==info.recorded.FileName) return true;
				}
			}
			return false;
		}

		static void TranscodeWorkerThread()
		{
			System.Threading.Thread.CurrentThread.Priority=ThreadPriority.BelowNormal;

			while (GUIGraphicsContext.CurrentState != GUIGraphicsContext.State.STOPPING)
			{
				if (queue.Count==0) 
				{
					System.Threading.Thread.Sleep(100);
				}
				else
				{
					TranscoderInfo info =null;
					lock(queue)
					{
						for (int i=0; i < queue.Count;++i)
						{
							info = (TranscoderInfo)queue[i];
							if (info.status==Status.Waiting)
							{
								break;
							}
						}
					}
					if (info!=null && info.status==Status.Waiting)
					{
						DoTranscode(info);
					}

				}
			}
		}
		static void DoTranscode(TranscoderInfo tinfo)
		{
			
			tinfo.status=Status.Busy;
			TranscodeInfo info = new TranscodeInfo();
			info.Author="Mediaportal";
			info.Channel=tinfo.recorded.Channel;
			info.Description=tinfo.recorded.Description;
			info.Title=tinfo.recorded.Title;
			info.Start=tinfo.recorded.StartTime;
			info.End=tinfo.recorded.EndTime;
			TimeSpan ts=(tinfo.recorded.EndTime-tinfo.recorded.StartTime);
			info.Duration=(int)ts.TotalSeconds;
			info.file=tinfo.recorded.FileName;

			int		bitRate,FPS,Priority,QualityIndex,ScreenSizeIndex,Type,AutoHours;
			bool	deleteOriginal,AutoDeleteOriginal,AutoCompress;
			Size ScreenSize=new Size(0,0);
			Quality quality;
			using(MediaPortal.Profile.Xml   xmlreader=new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				bitRate  = xmlreader.GetValueAsInt("compression","bitrate",4);
				FPS		   = xmlreader.GetValueAsInt("compression","fps",1);
				Priority = xmlreader.GetValueAsInt("compression","priority",0);
				QualityIndex  = xmlreader.GetValueAsInt("compression","quality",3);
				ScreenSizeIndex= xmlreader.GetValueAsInt("compression","screensize",1);
				Type     = xmlreader.GetValueAsInt("compression","type",0);
				deleteOriginal= xmlreader.GetValueAsBool("compression","deleteoriginal",true);

				AutoHours= xmlreader.GetValueAsInt("autocompression","hour",4);
				AutoDeleteOriginal= xmlreader.GetValueAsBool("autocompression","deleteoriginal",true);
				AutoCompress= xmlreader.GetValueAsBool("autocompression","enabled",true);
			}
			switch (bitRate)
			{
				case 0:
					bitRate=300;
					break;
				case 1:
					bitRate=500;
					break;
				case 2:
					bitRate=1024;
					break;
				case 3:
					bitRate=2048;
					break;
				case 4:
					bitRate=4096;
					break;
				case 5:
					bitRate=8192;
					break;
			}
			switch (FPS)
			{
				case 0:
					FPS=15;
					break;
				case 1:
					FPS=25;
					break;
				case 2:
					FPS=30;
					break;
			}
			switch (ScreenSizeIndex)
			{
				case 0:
					ScreenSize=new Size(1024,768);
					break;
				case 1:
					ScreenSize=new Size(720,576);
					break;
				case 2:
					ScreenSize=new Size(704,480);
					break;
				case 3:
					ScreenSize=new Size(740,288);
					break;
				case 4:
					ScreenSize=new Size(740,240);
					break;
				case 5:
					ScreenSize=new Size(704,576);
					break;
				case 6:
					ScreenSize=new Size(640,480);
					break;
				case 7:
					ScreenSize=new Size(640,288);
					break;
				case 8:
					ScreenSize=new Size(640,240);
					break;
				case 9:
					ScreenSize=new Size(352,288);
					break;
				case 10:
					ScreenSize=new Size(352,240);
					break;
			}
			bool isMpeg=(Type==0);
			bool isWMV=(Type==1);
			bool isXVID=(Type==2);
			quality= (Quality)QualityIndex;


			Dvrms2Mpeg mpgConverter = new Dvrms2Mpeg();
			if (!mpgConverter.Transcode(info,VideoFormat.Mpeg2,MediaPortal.Core.Transcoding.Quality.High))
			{
				tinfo.status=Status.Error;
				return;
			}
			while (!mpgConverter.IsFinished()) 
			{
				System.Threading.Thread.Sleep(100);
			}
			if (isMpeg)
			{
				return;
			}

			info.file=System.IO.Path.ChangeExtension(info.file,".mpg");
			if (isXVID)
			{
				string outputFile=System.IO.Path.ChangeExtension(info.file,".avi");
				string mencoderParams=String.Format("\"{0}\" -o \"{1}\" -oac mp3lame -ovc xvid  -xvidencopts autoaspect:bitrate=1024 -demuxer 35",
																								info.file,outputFile);
				Log.Write("mencoder.exe {0}", mencoderParams);
				Utils.StartProcess(@"mencoder\mencoder.exe",mencoderParams,true,true);
				if (System.IO.File.Exists(outputFile))
				{
					if (deleteOriginal)
					{
						DiskManagement.DeleteRecording(tinfo.recorded.FileName);
						DiskManagement.DeleteRecording(info.file);
					}
					tinfo.status=Status.Completed;
				}
				else
				{
					tinfo.status=Status.Error;
				}
			}
		}
	}
}
