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
			public int        bitRate;
			public int        FPS;
			public int        Type;
			public Quality		quality;
			public bool				deleteOriginal;
			public Size				ScreenSize;
			public DateTime   StartTime;
			public bool				LowPriority=true;

			public TranscoderInfo(TVRecorded recording, int kbps, int fps, Size newSize,bool deleteWhenDone, int qualityIndex,DateTime dateTime, int outputType, bool priority)
			{
				recorded=recording;
				status=Status.Waiting;
				percentDone=0;
				bitRate=kbps;
				FPS=fps;
				ScreenSize=newSize;
				deleteOriginal=deleteWhenDone;
				quality=(Quality)qualityIndex;
				StartTime=dateTime;
				Type=outputType;
				LowPriority=priority;
			}
		}
		public enum Status
		{
			Waiting,
			Busy,
			Completed,
			Error
		}
		static ArrayList  queue = new ArrayList();
		static Thread		  WorkerThread =null;
		
		static Transcoder()
		{
		}

		static public void Transcode(TVRecorded rec,bool manual)
		{
			int		bitRate,FPS,Priority,QualityIndex,ScreenSizeIndex,Type,AutoHours;
			bool	deleteOriginal,AutoDeleteOriginal,AutoCompress;
			Size ScreenSize=new Size(0,0);
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
					bitRate=100;
					break;
				case 1:
					bitRate=256;
					break;
				case 2:
					bitRate=384;
					break;
				case 3:
					bitRate=768;
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

			lock(queue)
			{
				DateTime dtStart=DateTime.Now;
				bool deleteWhenDone=deleteOriginal;
				if (AutoCompress && !manual)
				{
					deleteWhenDone=AutoDeleteOriginal;
					dtStart = dtStart.AddHours(AutoHours);
				}
				TranscoderInfo info = new TranscoderInfo(rec,bitRate,FPS,ScreenSize,deleteWhenDone,QualityIndex,dtStart,Type, Priority==0);
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
					if (info.recorded.FileName==rec.FileName) return true;
				}
			}
			return false;
		}

		static void TranscodeWorkerThread()
		{

			while (GUIGraphicsContext.CurrentState != GUIGraphicsContext.State.STOPPING)
			{
				if (queue.Count==0) 
				{
					System.Threading.Thread.Sleep(100);
				}
				else
				{
					TranscoderInfo transcording =null;
					lock(queue)
					{
						for (int i=0; i < queue.Count;++i)
						{
							TranscoderInfo info = (TranscoderInfo)queue[i];
							if (DateTime.Now >=info.StartTime )
							{
								if (info.status==Status.Waiting)
								{
									transcording=info;
									break;
								}
							}
						}
					}

					if (transcording!=null && transcording.status==Status.Waiting)
					{
						DoTranscode(transcording);
					}
					else System.Threading.Thread.Sleep(1000);
				}
			}
		}
		static void DoTranscode(TranscoderInfo tinfo)
		{

			if (tinfo.LowPriority)
				System.Threading.Thread.CurrentThread.Priority=ThreadPriority.Lowest;
			else
				System.Threading.Thread.CurrentThread.Priority=ThreadPriority.Normal;

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


			bool isMpeg=(tinfo.Type==0);
			bool isWMV=(tinfo.Type==1);
			bool isXVID=(tinfo.Type==2);
			switch (tinfo.quality)
			{
				case Quality.High:
					tinfo.ScreenSize=new Size(0,0);//keep video resolution
					tinfo.FPS=0;//keep video FPS
					tinfo.bitRate=768;
				break;
					
				case Quality.Medium:
					tinfo.ScreenSize=new Size(0,0);//keep video resolution
					tinfo.FPS=0;//keep video FPS
					tinfo.bitRate=384;
					break;
					
				case Quality.Low:
					tinfo.ScreenSize=new Size(0,0);//keep video resolution
					tinfo.FPS=0;//keep video FPS
					tinfo.bitRate=256;
					break;
					
				case Quality.Portable:
					tinfo.ScreenSize=new Size(352,288);
					tinfo.FPS=15;
					tinfo.bitRate=100;
					break;
			}

			if (isWMV)
			{
				TranscodeToWMV WMVConverter = new TranscodeToWMV();
				WMVConverter.CreateProfile(tinfo.ScreenSize,tinfo.bitRate,tinfo.FPS);
				if (!WMVConverter.Transcode(info,VideoFormat.Wmv,tinfo.quality))
				{
					tinfo.status=Status.Error;
					return;
				}
				while (!WMVConverter.IsFinished()) 
				{
					if (GUIGraphicsContext.CurrentState==GUIGraphicsContext.State.STOPPING) return;
					tinfo.percentDone=WMVConverter.Percentage();
					System.Threading.Thread.Sleep(100);
				}
				if (tinfo.deleteOriginal)
				{
					DiskManagement.DeleteRecording(info.file);
					tinfo.recorded.FileName=System.IO.Path.ChangeExtension(info.file,".wmv");
					TVDatabase.SetRecordedFileName(tinfo.recorded);
				}
				tinfo.status=Status.Completed;
				return;
			}


			if (isXVID)
			{
				Dvrms2XVID xvidEncoder = new Dvrms2XVID();
				xvidEncoder.CreateProfile(tinfo.ScreenSize,tinfo.bitRate,tinfo.FPS);
				if (!xvidEncoder.Transcode(info,VideoFormat.Xvid,tinfo.quality))
				{
					tinfo.status=Status.Error;
					return;
				}
				while (!xvidEncoder.IsFinished()) 
				{
					if (GUIGraphicsContext.CurrentState==GUIGraphicsContext.State.STOPPING) return;
					tinfo.percentDone=xvidEncoder.Percentage();
					System.Threading.Thread.Sleep(100);
				}
				if (tinfo.deleteOriginal)
				{
					DiskManagement.DeleteRecording(info.file);
					tinfo.recorded.FileName=System.IO.Path.ChangeExtension(info.file,".avi");
					TVDatabase.SetRecordedFileName(tinfo.recorded);
				}
				tinfo.status=Status.Completed;
				return;
			}

			Dvrms2Mpeg mpgConverter = new Dvrms2Mpeg();
			if (!mpgConverter.Transcode(info,VideoFormat.Mpeg2,tinfo.quality))
			{
				tinfo.status=Status.Error;
				return;
			}
			while (!mpgConverter.IsFinished()) 
			{
				if (GUIGraphicsContext.CurrentState==GUIGraphicsContext.State.STOPPING) return;
				tinfo.percentDone=mpgConverter.Percentage();
				System.Threading.Thread.Sleep(100);
			}
			if (isMpeg)
			{
				if (tinfo.deleteOriginal)
				{
					DiskManagement.DeleteRecording(info.file);
					tinfo.recorded.FileName=System.IO.Path.ChangeExtension(info.file,".mpg");
					TVDatabase.SetRecordedFileName(tinfo.recorded);
				}
				tinfo.status=Status.Completed;
				return;
			}
/*
			if (isXVID)
			{
				info.file=System.IO.Path.ChangeExtension(info.file,".mpg");				
				string outputFile=System.IO.Path.ChangeExtension(info.file,".avi");
				string mencoderParams=String.Format("\"{0}\" -o \"{1}\" -oac mp3lame -ovc xvid  -xvidencopts autoaspect:bitrate={2} -demuxer 35",
																								info.file,outputFile,tinfo.bitRate);
				if (tinfo.FPS>0)
					mencoderParams+=String.Format(" -ofps {0}", tinfo.FPS);
				if (tinfo.ScreenSize.Width>0 && tinfo.ScreenSize.Height>0)
					mencoderParams+=String.Format(" -vf scale={0}:{1}", tinfo.ScreenSize.Width,tinfo.ScreenSize.Height);

				Log.Write("mencoder.exe {0}", mencoderParams);
				Utils.StartProcess(@"mencoder\mencoder.exe",mencoderParams,true,true);
				if (System.IO.File.Exists(outputFile))
				{
					if (tinfo.deleteOriginal)
					{
						tinfo.percentDone=0;
						DiskManagement.DeleteRecording(tinfo.recorded.FileName);
						tinfo.recorded.FileName=System.IO.Path.ChangeExtension(info.file,".avi");
						TVDatabase.SetRecordedFileName(tinfo.recorded);
					}
					tinfo.status=Status.Completed;
				}
				else
				{
					tinfo.status=Status.Error;
				}
				Utils.FileDelete(info.file);//delete the .mpg file
			}
			*/
		}
	}
}
