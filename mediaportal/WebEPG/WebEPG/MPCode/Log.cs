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
			Error
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
			string fname=@"log\WebEPG.log";
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
			if (type != LogType.Log && type != LogType.Error)
				WriteFile(LogType.Log,strFormat,arg);
		}//static public void WriteFile(LogType type, string strFormat, params object[] arg)
	}
}
