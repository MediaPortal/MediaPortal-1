using System;
using System.Diagnostics;
using System.IO;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// An implementation of a log mechanism for the GUI library.
	/// </summary>
	public class Log
	{

		public enum LogType
		{
			Log,
			Capture,
			Recorder,
			Error,
			EPG
		}
		/// <summary>
		/// Private constructor of the GUIPropertyManager. Singleton. Do not allow any instance of this class.
		/// </summary>
    private Log()
    {
    }

		/// <summary>
		/// Deletes the logfile.
		/// </summary>
		static Log()
		{
			System.IO.Directory.CreateDirectory("log");
			Initialize(LogType.Capture);
			Initialize(LogType.Log);
			Initialize(LogType.Recorder);
			Initialize(LogType.Error);
			Initialize(LogType.EPG);
		}

		static void Initialize(LogType type)
		{
      try
      {
				string name=GetFileName(type);
				System.IO.File.Delete(name.Replace(".log",".bak"));
				System.IO.File.Move(name,name.Replace(".log",".bak"));
      }
      catch(Exception)
      {
      }
		}

		/// <summary>
		/// Write a string to the logfile.
		/// </summary>
		/// <param name="strFormat">The format of the string.</param>
		/// <param name="arg">An array containing the actual data of the string.</param>
		static public void Write(string strFormat, params object[] arg)
		{
      WriteFile(LogType.Log,strFormat,arg);
		}

		static string GetFileName(LogType type)
		{
			string fname=@"log\MediaPortal.log";
			switch (type)
			{
				case LogType.Capture:
					fname=@"log\capture.log";
					break;
				case LogType.Recorder:
					fname=@"log\recorder.log";
					break;
				case LogType.Error:
					fname=@"log\error.log";
					break;
				case LogType.EPG:
					fname=@"log\epg.log";
					break;
			}
			return fname;
		}

		static public void WriteFile(LogType type, bool isError, string strFormat, params object[] arg)
		{
			WriteFile(type, strFormat, arg);
			if (isError)
				WriteFile(LogType.Error, strFormat, arg);
		}

		static public void WriteFile(LogType type, string strFormat, params object[] arg)
		{
			lock (typeof(Log))
			{
				try
				{
					using (StreamWriter writer = new StreamWriter(GetFileName(type),true))
					{
						writer.BaseStream.Seek(0, SeekOrigin.End); // set the file pointer to the end of 
						writer.Write(DateTime.Now.ToShortDateString()+ " "+DateTime.Now.ToLongTimeString()+ " ");
						writer.WriteLine(strFormat,arg);
						writer.Close();
					}
					string strLine=String.Format(strFormat,arg);
					Debug.WriteLine(strLine);
				}
				catch(Exception)
				{
				}
			}

			//
			if (type != LogType.Log && type != LogType.Error&& type != LogType.EPG)
				WriteFile(LogType.Log,strFormat,arg);
		}//static public void WriteFile(LogType type, string strFormat, params object[] arg)
	}
}
