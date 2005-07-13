using System;
using System.IO;
using System.Collections;
using System.Management; 
using System.Threading;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.TV.Database;
using MediaPortal.Video.Database;
using Toub.MediaCenter.Dvrms.Metadata;

namespace MediaPortal.TV.Recording
{
	/// <summary>
	/// Summary description for DiskManagement.
	/// </summary>
	public class DiskManagement
	{
		static bool					 importing=false;
		static DateTime      m_dtCheckDiskSpace=DateTime.Now;
		public DiskManagement()
		{
			Recorder.OnTvRecordingEnded +=new MediaPortal.TV.Recording.Recorder.OnTvRecordingHandler(Recorder_OnTvRecordingEnded);
		}
		#region dvr-ms importing
		static public void DeleteRecording(string recordingFilename)
		{
			Utils.FileDelete(recordingFilename);
			int pos=recordingFilename.LastIndexOf(@"\");
			if (pos<0) return;
			string path=recordingFilename.Substring(0,pos);
			string filename=recordingFilename.Substring(pos+1);
			pos=filename.LastIndexOf(".");
			if (pos>=0)
				filename=filename.Substring(0,pos);
			filename=filename.ToLower();
			string[] strFiles;
			try
			{
				strFiles=System.IO.Directory.GetFiles(path);
				foreach (string strFile in strFiles)
				{
					try
					{
						if (strFile.ToLower().IndexOf(filename)>=0)
						{
							if (strFile.ToLower().IndexOf(".sbe")>=0)
							{
								System.IO.File.Delete(strFile);
							}
						}
					}
					catch(Exception){}
				}
			}
			catch(Exception){}
		}
		static public void ImportDvrMsFiles()
		{
			//dont import during recording...
			if (Recorder.IsAnyCardRecording()) return;
			if (importing) return;
		  Thread WorkerThread = new Thread(new ThreadStart(ImportWorkerThreadFunction));
			WorkerThread.Start();
		}
		static void ImportWorkerThreadFunction()
		{
			System.Threading.Thread.CurrentThread.Priority=ThreadPriority.BelowNormal;
			importing=true;
			try
			{
				//dont import during recording...
				if (Recorder.IsAnyCardRecording()) return;
				ArrayList recordings = new ArrayList();
				TVDatabase.GetRecordedTV(ref recordings);
				for (int i=0; i < Recorder.Count;i++)
				{
					TVCaptureDevice dev = Recorder.Get(i);
					if (dev==null) continue;
					try
					{
						string[] files=System.IO.Directory.GetFiles(dev.RecordingPath,"*.dvr-ms");
						foreach (string file in files)
						{
							System.Threading.Thread.Sleep(100);
							bool add=true;
							foreach (TVRecorded rec in recordings)
							{
								if (Recorder.IsAnyCardRecording()) return;
								if (rec.FileName!=null)
								{
									if (rec.FileName.ToLower()==file.ToLower())
									{
										add=false;
										break;
									}
								}
							}
							if (add)
							{
								Log.WriteFile(Log.LogType.Recorder,"Recorder: import recording {0}", file);
								try
								{
									System.Threading.Thread.Sleep(100);
									using (DvrmsMetadataEditor editor = new DvrmsMetadataEditor(file))
									{
										IDictionary dict=editor.GetAttributes();
										if (dict !=null)
										{	
											TVRecorded newRec = new TVRecorded();
											newRec.FileName=file;
											foreach (MetadataItem item in dict.Values)
											{
												if (item==null) continue;
												if (item.Name==null) continue;
												//Log.WriteFile(Log.LogType.Recorder,"attribute:{0} value:{1}", item.Name,item.Value.ToString());
												try { if (item.Name.ToLower()=="channel") newRec.Channel=(string)item.Value.ToString();} 
												catch(Exception){}
												try{ if (item.Name.ToLower()=="title") newRec.Title=(string)item.Value.ToString();} 
												catch(Exception){}
												try{ if (item.Name.ToLower()=="programtitle") newRec.Title=(string)item.Value.ToString();} 
												catch(Exception){}
												try{ if (item.Name.ToLower()=="genre") newRec.Genre=(string)item.Value.ToString();} 
												catch(Exception){}
												try{ if (item.Name.ToLower()=="details") newRec.Description=(string)item.Value.ToString();} 
												catch(Exception){}
												try{ if (item.Name.ToLower()=="start") newRec.Start=(long)UInt64.Parse(item.Value.ToString());} 
												catch(Exception){}
												try{ if (item.Name.ToLower()=="end") newRec.End=(long)UInt64.Parse(item.Value.ToString());} 
												catch(Exception){}
											}
											if (newRec.Channel==null)
											{
												string name=Utils.GetFilename(file);
												string[] parts=name.Split('_');
												if (parts.Length>0)
													newRec.Channel=parts[0];
											}
											if (newRec.Channel!=null && newRec.Channel.Length>0)
											{
												int id=TVDatabase.AddRecordedTV(newRec);
												if (id < 0)
												{
													Log.WriteFile(Log.LogType.Recorder,"Recorder: import recording {0} failed");
												}
												recordings.Add(newRec);
											}
											else
											{
												Log.WriteFile(Log.LogType.Recorder,"Recorder: import recording {0} failed, unknown tv channel", file);
											}
										}
									}//using (DvrmsMetadataEditor editor = new DvrmsMetadataEditor(file))
								}
								catch(Exception ex)
								{
									Log.WriteFile(Log.LogType.Log,true,"Recorder:Unable to import {0} reason:{1} {2} {3}", file,ex.Message, ex.Source,ex.StackTrace);
								}
							}//if (add)
						}//foreach (string file in files)
					}
					catch(Exception ex)
					{
						Log.WriteFile(Log.LogType.Log,true,"Recorder:Exception while importing recordings reason:{0} {1}", ex.Message, ex.Source);
					}
				}//for (int i=0; i < Recorder.Count;++i)
			}
			catch(Exception)
			{
			}
			importing=false;
		} //static void ImportDvrMsFiles()
		#endregion


		#region diskmanagement
		/// <summary>
		/// this method deleted any timeshifting files in the specified folder
		/// </summary>
		/// <param name="strPath">folder name</param>
		static public void DeleteOldTimeShiftFiles(string strPath)
		{
			if (strPath==null) return;
			if (strPath==String.Empty) return;
			// Remove any trailing slashes
			strPath=Utils.RemoveTrailingSlash(strPath);

      
			// clean the TempDVR\ folder
			string strDir=String.Empty;
			string[] strFiles;
			try
			{
				strDir=String.Format(@"{0}\TempDVR",strPath);
				strFiles=System.IO.Directory.GetFiles(strDir,"*.tmp");
				foreach (string strFile in strFiles)
				{
					try
					{
						System.IO.File.Delete(strFile);
					}
					catch(Exception){}
				}
			}
			catch(Exception){}

			// clean the TempSBE\ folder
			try
			{      
				strDir=String.Format(@"{0}\TempSBE",strPath);
				strFiles=System.IO.Directory.GetFiles(strDir,"*.tmp");
				foreach (string strFile in strFiles)
				{
					try
					{
						System.IO.File.Delete(strFile);
					}
					catch(Exception){}
				}
			}
			catch(Exception){}

			// delete *.tv
			try
			{      
				strDir=String.Format(@"{0}",strPath);
				strFiles=System.IO.Directory.GetFiles(strDir,"*.tv");
				foreach (string strFile in strFiles)
				{
					try
					{
						System.IO.File.Delete(strFile);
					}
					catch(Exception){}
				}
			}
			catch(Exception){}
		}//static void DeleteOldTimeShiftFiles(string strPath)

		static public void CheckRecordingDiskSpace()
		{
			TimeSpan ts = DateTime.Now-m_dtCheckDiskSpace;
			if (ts.TotalMinutes<1) return;

			m_dtCheckDiskSpace=DateTime.Now;

			//first get all drives..
			ArrayList drives = new ArrayList();
			for (int i=0; i < Recorder.Count;++i)
			{
				TVCaptureDevice dev =Recorder.Get(i);
				if (dev.RecordingPath==null) continue;
				if (dev.RecordingPath.Length<2) continue;
				string drive=dev.RecordingPath.Substring(0,2);
				bool newDrive=true;
				foreach (string tmpDrive in drives)
				{
					if (drive.ToLower()==tmpDrive.ToLower())
					{
						newDrive=false;
					}
				}
				if (newDrive) drives.Add(drive);
			}

			// for each drive get all recordings
			ArrayList recordings = new ArrayList();
			foreach (string drive in drives)
			{
				recordings.Clear();

				long lMaxRecordingSize=0;
				long diskSize=0;
				try
				{
					string cmd=String.Format( "win32_logicaldisk.deviceid=\"{0}:\"", drive[0]);
					using (ManagementObject disk = new ManagementObject(cmd))
					{
						disk.Get();
						diskSize=Int64.Parse(disk["Size"].ToString());
					}
				}
				catch(Exception)
				{
					continue;
				}

				for (int i=0; i < Recorder.Count;++i)
				{
					TVCaptureDevice dev =Recorder.Get(i);
					dev.GetRecordings(drive,ref recordings);
					
					int percentage= dev.MaxSizeLimit;
					long lMaxSize= (long) ( ((float)diskSize) * ( ((float)percentage) / 100f ));
					if (lMaxSize > lMaxRecordingSize) 
						lMaxRecordingSize=lMaxSize;
				}//foreach (TVCaptureDevice dev in m_tvcards)

				long totalSize=0;
				foreach (RecordingFileInfo info in recordings)
				{
					totalSize +=  info.info.Length;
				}

				if (totalSize >= lMaxRecordingSize && lMaxRecordingSize >0) 
				{
					Log.WriteFile(Log.LogType.Recorder,"Recorder: exceeded diskspace limit for recordings on drive:{0}",drive);
					Log.WriteFile(Log.LogType.Recorder,"Recorder:   {0} recordings contain {1} while limit is {2}",
						recordings.Count, Utils.GetSize(totalSize), Utils.GetSize(lMaxRecordingSize) );

					// we exceeded the diskspace
					//delete oldest files...
					recordings.Sort();
					while (totalSize > lMaxRecordingSize && recordings.Count>0)
					{
						RecordingFileInfo fi = (RecordingFileInfo)recordings[0];
						Log.WriteFile(Log.LogType.Recorder,"Recorder: delete old recording:{0} size:{1} date:{2} {3}",
							fi.filename,
							Utils.GetSize(fi.info.Length),
							fi.info.CreationTime.ToShortDateString(), fi.info.CreationTime.ToShortTimeString());
						totalSize -= fi.info.Length;
						if (Utils.FileDelete(fi.filename))
						{
							DeleteRecording(fi.filename);
							VideoDatabase.DeleteMovie(fi.filename);
							VideoDatabase.DeleteMovieInfo(fi.filename);
						}
						recordings.RemoveAt(0);
					}//while (totalSize > m_lMaxRecordingSize && files.Count>0)
				}//if (totalSize >= lMaxRecordingSize && lMaxRecordingSize >0) 
			}//foreach (string drive in drives)
		}//static void CheckRecordingDiskSpace()
		
		#endregion

		#region episode disk management
		private void Recorder_OnTvRecordingEnded(string recordingFilename, TVRecording recording, TVProgram program)
		{
			if (recording.EpisodesToKeep == Int32.MaxValue) return;
			if (recording.RecType==TVRecording.RecordingType.Once) return;
			
			//check how many episodes we got
			ArrayList recordings = new ArrayList();
			TVDatabase.GetRecordings(ref recordings);
			int recordingsFound=0;
			DateTime oldestRecording=DateTime.MaxValue;
			string   oldestFileName=String.Empty;
			foreach (TVRecorded rec in recordings)
			{
				if (rec.Title.ToLower().Equals(recording.Title.ToLower())	)
				{
					recordingsFound++;
					if (rec.StartTime < oldestRecording)
					{
						oldestRecording=rec.StartTime;
						oldestFileName=rec.FileName;
					}
				}
			}
			if (recordingsFound <= recording.EpisodesToKeep) return;
			if (Utils.FileDelete(oldestFileName))
			{
				DeleteRecording(oldestFileName);
				VideoDatabase.DeleteMovie(oldestFileName);
				VideoDatabase.DeleteMovieInfo(oldestFileName);
			}
		}
		#endregion
	}
}
