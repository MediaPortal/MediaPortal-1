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
      try
      {
        System.IO.Directory.CreateDirectory("log");
        System.IO.File.Delete(@"log\MediaPortal.log");
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
      lock (typeof(Log))
      {
        try
        {
          using (StreamWriter writer = new StreamWriter(@"log\MediaPortal.log",true))
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
		}
	}
}
