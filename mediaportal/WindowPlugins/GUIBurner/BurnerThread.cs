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

#region Usings
using System;
using System.Collections;
using System.Threading;
using System.IO;
using System.Management;
using SQLite.NET;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using Core.Util;
using DShowNET;
using System.Runtime.InteropServices;

#endregion

namespace MediaPortal.GUI.GUIBurner
{
	/// <summary>
	/// Summary description for BurnerThread.
	/// </summary>
	public class BurnerThread
	{
		protected bool									converting = false;
		protected bool									deleteDvrSrc = false;
		protected bool									changeTVDatabase = false;
		protected bool									copyMpegPath = false;
		protected bool									deleteTVDatabase = false;
		protected string								mpegpath = "";
		protected int										rotCookie = 0;
		protected IGraphBuilder			  	graphBuilder =null;
		protected IStreamBufferSource 	bufferSource=null ;
		protected IFileSinkFilter				fileWriterFilter = null;			// DShow Filter: file writer
		protected IMediaControl					mediaControl=null;
		protected IBaseFilter						powerDvdMuxer =null;

		private ArrayList cFiles  = new ArrayList();
		private struct file 
		{
			public string name;
			public string path;
		}

		/// <summary>
		/// is converter thread running?
		/// </summary>
		public bool isConverting	
		{
			get{ return converting; }
			set{ converting = value; }
		}

		/// <summary>
		/// Delete DVR-MS after converting?
		/// </summary>
		public bool deleteDvrMsSrc
		{
			get{ return deleteDvrSrc; }
			set{ deleteDvrSrc = value; }
		}

		/// <summary>
		/// Update TV Database after converting?
		/// </summary>
		public bool changeDatabase
		{
			get{ return changeTVDatabase; }
			set{ changeTVDatabase = value; }
		}
	
		/// <summary>
		/// Delete TV Database after converting?
		/// </summary>
		public bool deleteDatabase
		{
			get{ return deleteTVDatabase; }
			set{ deleteTVDatabase = value; }
		}

		/// <summary>
		/// Copy MPeg after converting?
		/// </summary>
		public bool copyMpeg
		{
			get{ return copyMpegPath; }
			set{ copyMpegPath = value; }
		}

		/// <summary>
		/// MPeg Path?
		/// </summary>
		public string mpegPath
		{
			get{ return mpegpath; }
			set{ mpegpath = value; }
		}

		/// <summary>
		/// clear converter file list.
		/// </summary>
		public void ClearFiles()
		{
			cFiles.Clear();
		}

		/// <summary>
		/// add a file to converter file list.
		/// </summary>
		public void AddFiles(string name,string path)	
		{
			file fl = new file();
			fl.name=name;
			fl.path=path;
			cFiles.Add(fl);
		}

		public BurnerThread()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		/// <summary>
		/// start converting file list.
		/// </summary>
		public void TranscodeThread()
		{
			long gfl=0;
			long zfl=0;
			long wfl=0;
			int perc;
			bool test=false;
			byte eff=0;

			foreach(file f in cFiles) 
			{
				FileInfo fil = new FileInfo(f.path+"\\"+f.name);	
				gfl=gfl+fil.Length;						
			}

			foreach(file f in cFiles) 
			{
				converting=true;
				string outName=System.IO.Path.ChangeExtension(f.path+"\\"+f.name,".mpg");
				FileInfo fil = new FileInfo(outName);	
				if (fil.Exists)												// Output file exist
				{
					continue;
				}
				Log.Write("Convert File {0}",f.path+"\\"+f.name);
				test=Transcode(f.path+"\\"+f.name);		// Start Converting
				if (test==false)											// Converting breaks with error
				{
					continue;
				}

				long fl=0;
				long flO=0;
				wfl=wfl+zfl;
				while (IsTranscoding()) 
				{
					Thread.Sleep(1000);
					FileInfo fi = new FileInfo(outName);	// The following Code is a little Hack to detect
					fl=fi.Length;													// the end of Conversion
					zfl=fl;
					string text=f.name;
					string c1="/ Convert ";
					string c2="- Convert ";
					string c3="\\ Convert ";
					string c4="| Convert ";
					if (fl>flO) 
					{
						flO=fl;
					} 
					else if (fl==flO) 
					{
						Thread.Sleep(6000);									 // 6 Seconds no Change of File Size
						FileInfo fi2 = new FileInfo(outName);// converting ends
						fl=fi2.Length;												
						if (fl==flO) 
						{
							mediaControl.StopWhenReady();			 // stop convert task
							text=" "; c1=""; c2=""; c3=""; c4="";
						} 
						else 
						{
							flO=fl;
						}
					}

					if ((zfl+wfl)>0) 
						perc=Convert.ToInt16((zfl+wfl)/(gfl/100d)); 
					else 
						perc=0;
					GUIPropertyManager.SetProperty("#burner_perc",perc.ToString());

					switch (eff)
					{
						case 0: 
							GUIPropertyManager.SetProperty("#convert_info",c1+text);
							eff=1;
							break;
						case 1: 
							GUIPropertyManager.SetProperty("#convert_info",c2+text);
							eff=2;
							break;
						case 2: 
							GUIPropertyManager.SetProperty("#convert_info",c3+text);
							eff=3;
							break;
						case 3: 
							GUIPropertyManager.SetProperty("#convert_info",c4+text);
							eff=0;
							break;
					}
				}
				if (changeDatabase==true) //Update TV Database
				{
					FileInfo f1 = new FileInfo(f.path+"\\"+f.name);
					if (f1.Exists)				
					{
						string oName=System.IO.Path.ChangeExtension(f.path+"\\"+f.name,".mpg");
						UpdateTVDatabase(f.path+"\\"+f.name,oName,deleteDatabase);
					}
				}
				if (deleteDvrSrc==true) //Delete DVR-MS Source File
				{ 
					string oName=System.IO.Path.ChangeExtension(f.path+"\\"+f.name,".mpg");
					FileInfo f1 = new FileInfo(oName);	
					if (f1.Exists)					// Output file exist
					{
						FileInfo f2 = new FileInfo(f.path+"\\"+f.name);
						f2.Delete();
					}
				}
				if (copyMpeg==true) //Copy Mpeg File
				{
					string oName=System.IO.Path.ChangeExtension(f.path+"\\"+f.name,".mpg");
					string dName=System.IO.Path.ChangeExtension(copyMpegPath+"\\"+f.name,".mpg");
					FileInfo f1 = new FileInfo(oName);	
					if (f1.Exists)					// Output file exist
					{
						f1.MoveTo(dName);
					}
				}
			}
			converting=false;
			GUIPropertyManager.SetProperty("#convert_info"," ");
			GUIPropertyManager.SetProperty("#burner_perc","-5");
		}

		private bool Transcode(string file)
		{
			Type comtype = null;
			object comobj = null;
			try 
			{
				comtype = Type.GetTypeFromCLSID( Clsid.FilterGraph );
				comobj = Activator.CreateInstance( comtype );
				graphBuilder = (IGraphBuilder) comobj; comobj = null;
			
				DsROT.AddGraphToRot( graphBuilder, out rotCookie );		// graphBuilder capGraph
				Guid clsid = Clsid.StreamBufferSource;
				Guid riid = typeof(IStreamBufferSource).GUID;
				Object comObj = DsBugWO.CreateDsInstance( ref clsid, ref riid );
				bufferSource = (IStreamBufferSource) comObj; comObj = null;
		
				IBaseFilter filter = (IBaseFilter) bufferSource;
				graphBuilder.AddFilter(filter, "SBE SOURCE");
		
				IFileSourceFilter fileSource = (IFileSourceFilter) bufferSource;
				int hr = fileSource.Load(file, IntPtr.Zero);
				string monikerPowerDvdMuxer=@"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{BC650178-0DE4-47DF-AF50-BBD9C7AEF5A9}";
				powerDvdMuxer = Marshal.BindToMoniker( monikerPowerDvdMuxer ) as IBaseFilter;

				hr = graphBuilder.AddFilter( powerDvdMuxer, "Cyberlink MPEG Muxer" );
				string monikerFileWrite=@"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{3E8868CB-5FE8-402C-AA90-CB1AC6AE3240}";
				IBaseFilter fileWriterbase = Marshal.BindToMoniker( monikerFileWrite ) as IBaseFilter;
				
				fileWriterFilter = fileWriterbase as IFileSinkFilter;

				hr = graphBuilder.AddFilter( fileWriterbase , "FileWriter" );

				IPin pinOut0, pinOut1;
				IPin pinIn0, pinIn1;

				DsUtils.GetPin((IBaseFilter)bufferSource,PinDirection.Output,0,out pinOut0);
				DsUtils.GetPin((IBaseFilter)bufferSource,PinDirection.Output,1,out pinOut1);

				DsUtils.GetPin(powerDvdMuxer,PinDirection.Input,0,out pinIn0);
				DsUtils.GetPin(powerDvdMuxer,PinDirection.Input,1,out pinIn1);
				AMMediaType amAudio= new AMMediaType();
				amAudio.majorType = MediaType.Audio;
				amAudio.subType = MediaSubType.MPEG2_Audio;
				pinOut0.Connect(pinIn1,ref amAudio);

				AMMediaType amVideo= new AMMediaType();
				amVideo.majorType = MediaType.Video;
				amVideo.subType = MediaSubType.MPEG2_Video;
				pinOut1.Connect(pinIn0,ref amVideo);

				IPin pinOut, pinIn;
				hr=DsUtils.GetPin(powerDvdMuxer,PinDirection.Output,0,out pinOut);
				hr=DsUtils.GetPin(fileWriterbase,PinDirection.Input,0,out pinIn);

				AMMediaType mt = new AMMediaType(); 
				hr=pinOut.Connect(pinIn,ref mt);

				string outputFileName=System.IO.Path.ChangeExtension(file,".mpg");
				mt.majorType=MediaType.Stream;
				mt.subType=MediaSubType.MPEG2;

				hr=fileWriterFilter.SetFileName(outputFileName, ref mt);

				mediaControl= graphBuilder as IMediaControl;
				hr=mediaControl.Run();
			}

			catch(Exception ex)
			{
				Log.Write("DVR2MPG:Unable create graph", ex.Message);
				Cleanup();
				return false;
			}
			return true;
		}

		private bool IsFinished()
		{
			if (mediaControl==null) return true;
			FilterState state;

			mediaControl.GetState(200, out state);
			if (state==FilterState.Stopped)
			{
				Cleanup();
				return true;
			}
			return false;
		}

		private bool IsTranscoding()
		{
			if (IsFinished()) return false;
			return true;
		}

		private void Cleanup()
		{
			if( rotCookie != 0 )
				DsROT.RemoveGraphFromRot( ref rotCookie );

			if( mediaControl != null )
			{
				mediaControl.Stop();
				mediaControl = null;
			}

			if ( powerDvdMuxer != null )
				Marshal.ReleaseComObject( powerDvdMuxer );
			powerDvdMuxer=null;

			if ( fileWriterFilter != null )
				Marshal.ReleaseComObject( fileWriterFilter );
			fileWriterFilter=null;

			if ( bufferSource != null )
				Marshal.ReleaseComObject( bufferSource );
			bufferSource = null;

			DsUtils.RemoveFilters(graphBuilder);

			if( graphBuilder != null )
				Marshal.ReleaseComObject( graphBuilder ); graphBuilder = null;
		}

		private SQLiteClient m_db;
		private bool dbExists;

		private void UpdateTVDatabase(string fileName,string oName,bool delete) 
		{
			string rSQL;
			try 
			{
				// Open database
				try
				{
					System.IO.Directory.CreateDirectory("database");
				}
				catch(Exception){}
				dbExists = System.IO.File.Exists( @"database\TVDatabaseV21.db3" );
				m_db = new SQLiteClient(@"database\TVDatabaseV21.db3");
				if( dbExists )
				{
 					rSQL = String.Format("SELECT * FROM recorded WHERE strFileName LIKE '{0}'",fileName);
					m_db.Execute(rSQL);
					if (delete==false) 
					{
						rSQL = String.Format("update recorded set strFileName='{0}' where strFileName like '{1}'",oName,fileName);
						m_db.Execute(rSQL);
					} 
					else 
					{
						rSQL = String.Format("delete recorded where strFileName = '{0}'",fileName);
						m_db.Execute(rSQL);
					}
				}
			} 
			catch (SQLiteException ex)
			{
				Log.Write("TVdatabase exception err:{0} stack:{1}", ex.Message,ex.StackTrace);
			}
		}
	}
}
