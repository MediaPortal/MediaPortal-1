using System;
using System.IO;
using System.Reflection;

namespace MediaPortal.GUI.Library
{
	public class Log
	{
		private Log()
		{
		}

		static Log()
		{
			try
			{
				Uri uri = new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase);
				
				_logFilename = uri.LocalPath.Replace(".exe", ".log");
			}
			catch
			{
				_logFilename = "myblaster.log";
			}
		}

		public static void Delete()
		{
			lock(typeof(Log)) if(File.Exists(_logFilename)) File.Delete(_logFilename);
		}

		public static void Write(string strFormat, params object[] arg)
		{
			if(_logEnabled == false) return;

			lock(typeof(Log))
			{
				try
				{
					using(StreamWriter writer = new StreamWriter(_logFilename, true))
					{
						writer.BaseStream.Seek(0, SeekOrigin.End); // set the file pointer to the end of 
						writer.Write(DateTime.Now.ToShortDateString()+ " "+DateTime.Now.ToLongTimeString()+ " ");
						writer.WriteLine(strFormat,arg);
						writer.Close();
					}

					Console.WriteLine(string.Format(strFormat, arg));
				}
				catch(Exception)
				{
				}
			}
		}

		static public string Filename
		{ get { return _logFilename; } }

		static public bool Enabled
		{ get { return _logEnabled; } set { _logEnabled = value; } }

		static protected string		_logFilename;
		static protected bool		_logEnabled = true;
	}
}
