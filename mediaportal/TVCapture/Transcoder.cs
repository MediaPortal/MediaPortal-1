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
		
		static Dvrms2Mpeg convertToMpg=null;
		static TranscodeToWMV	convertToWMV=null;
		static Transcoder()
		{
			convertToMpg = new Dvrms2Mpeg();
			convertToWMV = new TranscodeToWMV();
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
			quality= (Quality)QualityIndex;

			if (isMpeg)
			{
				if ( convertToMpg.Transcode(info,VideoFormat.Mpeg2,MediaPortal.Core.Transcoding.Quality.High) )
				{
					
					while (!convertToMpg.IsFinished() )
					{
						tinfo.percentDone=convertToMpg.Percentage();
						System.Threading.Thread.Sleep(100);
					}

					if (deleteOriginal)
					{
						DiskManagement.DeleteRecording(tinfo.recorded.FileName);
					}
					tinfo.status=Status.Completed;
				}
				else
				{
					tinfo.status=Status.Error;
				}
			}
			if (isWMV)
			{
				switch (quality)
				{
					case Quality.Portable:
						convertToWMV.CreateProfile("portable", 100*1000, new Size(0,0), 0, 0);
						break;
					case Quality.Low:
						convertToWMV.CreateProfile("low", 300*1000, new Size(0,0), 0, 0);
						break;
					case Quality.Medium:
						convertToWMV.CreateProfile("medium", 2048*1000, new Size(0,0), 0, 0);
						break;
					case Quality.High:
						convertToWMV.CreateProfile("high", 4096*1000, new Size(0,0), 0, 0);
						break;
					case Quality.Custom:
						convertToWMV.CreateProfile("custom", bitRate*1000, ScreenSize, bitRate, FPS);
						break;
				}
				if ( convertToMpg.Transcode(info,VideoFormat.Mpeg2,MediaPortal.Core.Transcoding.Quality.High) )
				{
					while (!convertToMpg.IsFinished() )
					{
						System.Threading.Thread.Sleep(100);
					}
					info.file=System.IO.Path.ChangeExtension(info.file,".mpg");
					if ( convertToWMV.Transcode(info,VideoFormat.Wmv,MediaPortal.Core.Transcoding.Quality.High) )
					{
						while (!convertToWMV.IsFinished() )
						{
							tinfo.percentDone=convertToWMV.Percentage();
							System.Threading.Thread.Sleep(100);
						}

						if (deleteOriginal)
						{
							DiskManagement.DeleteRecording(tinfo.recorded.FileName);//.dvr-ms
							DiskManagement.DeleteRecording(info.file);//.mpg
						}
						tinfo.status=Status.Completed;
					}
					else tinfo.status=Status.Error;
				}
				else
					tinfo.status=Status.Error;
			}
		
		}
	}
}
