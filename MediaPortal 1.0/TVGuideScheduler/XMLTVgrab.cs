#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
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

#endregion

using System;
using System.Text;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Threading;
using System.Collections;
using MediaPortal.GUI.Library;

namespace MediaPortal.TVGuideScheduler
{
	class XMLTVgrab
	{
		public static void BuildThreads(string[] daysToGrab,string grabber, string conf, string exe, string op, string args,bool runLowPrio)
		{
			//grab a single day per thread in multiple threads(much faster with tv_grab_uk_rt and others)
			int i =0;
			Thread[] grabThreads = new Thread[System.Convert.ToInt32(daysToGrab.Length)];
			//create a tread for each day defined in mediaprtal.xml
			foreach (string s in daysToGrab) 
			{	//start the threads
				RunMutliGrabber helper = new RunMutliGrabber( System.Convert.ToInt32(s),grabber,conf,exe,op,args );      
				if (runLowPrio)
					grabThreads[i].Priority=ThreadPriority.Lowest;
				grabThreads[i]= new Thread(new ThreadStart( helper.RunThread )) ;
                grabThreads[i].Name = string.Format("XMLTV Grabber {0}", s);
				grabThreads[i].Start();
				Thread.Sleep(10);
				i++;
			}
			//wait for the xmltv processes to finish
			foreach (Thread s in grabThreads) 
			{	
				s.Join();
			}

					
		}
		public static bool GrabberConfigure(string grabber, string path)
		{	//run grabber with the --configure option to set channels
			string xmltvpath = path + "\\xmltv.exe";
			string WorkingDir=xmltvpath.Substring(0, xmltvpath.Length - 9 );
			string outputPath=@""""+path + "\\"+grabber + @".conf""";
			string xmltvArgs = grabber + " --configure --config-file "+ outputPath;
			try
			{
				Process xmltvgrabber = new Process();
				xmltvgrabber.StartInfo.FileName = xmltvpath;
				xmltvgrabber.StartInfo.Arguments = xmltvArgs;
				xmltvgrabber.StartInfo.UseShellExecute = false;
				xmltvgrabber.StartInfo.WorkingDirectory = WorkingDir;
				xmltvgrabber.Start();
					
				int i = 0;
				while (!xmltvgrabber.HasExited)
				{
					xmltvgrabber.Refresh();
					i++;
					Thread.Sleep(100);
				}
				// Free resources associated with process.
				xmltvgrabber.Close();
				return true;
			}
			catch(Exception e)
			{
				Console.WriteLine("The following exception was raised: ");
				Console.WriteLine(e.Message);
				return false;
			}


		}


		public static bool RunGrabber(string grabber,string confFile,string path, string opFile,int days,int offset,string args, bool lowPrio)
		{	//start an xmltv process to grab multiple days
			string xmltvpath = path + "\\xmltv.exe";
			string WorkingDir=xmltvpath.Substring(0, xmltvpath.Length - 9 );
			string outputPath=@"""" + @opFile + @"\TvGuide.xml""";
			string xmltvArgs = grabber + " --config-file " + confFile + " --output "+outputPath + " --days " + days+ " " + args + " --offset " + offset;
      try
			{
				Process xmltvgrabber = new Process();
				xmltvgrabber.StartInfo.FileName = xmltvpath;
				xmltvgrabber.StartInfo.Arguments = xmltvArgs;
				xmltvgrabber.StartInfo.UseShellExecute = false;
				xmltvgrabber.StartInfo.WorkingDirectory = WorkingDir;
				xmltvgrabber.Start();
        if (lowPrio)        
					xmltvgrabber.PriorityClass = ProcessPriorityClass.BelowNormal;
					
				int i = 0;
				while (!xmltvgrabber.HasExited)
				{
					xmltvgrabber.Refresh();
					i++;
					Thread.Sleep(100);
				}
				// Free resources associated with process.
				xmltvgrabber.Close();
				return true;
			}
			catch(Exception e)
			{
				Console.WriteLine("The following exception was raised: ");
				Console.WriteLine(e.Message);
				return false;
			}
		}

    public static bool RunGrabber(string grabber,string path, string opFile,int days,int offset, bool lowPrio)
    //overload for nl_wolf grabber
    {	//start an xmltv process to grab multiple days
      string xmltvpath = path + "\\xmltv.exe";
      string WorkingDir=xmltvpath.Substring(0, xmltvpath.Length - 9 );
      string outputPath=@"""" + @opFile + @"\TvGuide.xml""";
      string xmltvArgs = grabber + " --output "+outputPath + " --days " + days + " --offset " + offset;
      try
      {
        Process xmltvgrabber = new Process();
        xmltvgrabber.StartInfo.FileName = xmltvpath;
        xmltvgrabber.StartInfo.Arguments = xmltvArgs;
        xmltvgrabber.StartInfo.UseShellExecute = false;
        xmltvgrabber.StartInfo.WorkingDirectory = WorkingDir;
        xmltvgrabber.Start();
				if (lowPrio)
					xmltvgrabber.PriorityClass = ProcessPriorityClass.BelowNormal;
					
        int i = 0;
        while (!xmltvgrabber.HasExited)
        {
          xmltvgrabber.Refresh();
          i++;
          Thread.Sleep(100);
        }
        // Free resources associated with process.
        xmltvgrabber.Close();
        return true;
      }
      catch(Exception e)
      {
        Console.WriteLine("The following exception was raised: ");
        Console.WriteLine(e.Message);
        return false;
      }
    }

		public class RunMutliGrabber
		{
			private int offset;
			private string grabber;
			private string confFile;
			private string path;
			private string opFile;
			private string args;

			public RunMutliGrabber(int offset,string grabber,string confFile,string path,string opFile,string args )
			{
				this.offset = offset;
				this.grabber = grabber;
				this.confFile = confFile;
				this.path = path;
				this.opFile = opFile;
				this.args = args;
			}

			
			public void RunThread()
			{	// thread to grab a single day
				string xmltvpath = path + "\\xmltv.exe";
				string outputPath=@"""" + @opFile + @"\TVguide" + Thread.CurrentThread.Name + @".xml""";
				string WorkingDir=xmltvpath.Substring(0, xmltvpath.Length - 10 );
				string xmltvargs = String.Format("{0} --config-file {1} --output {2} --days 1 {3} --offset {4}", grabber,confFile,outputPath,args,offset);
				try
				{
					Process grabThread = new Process();
					grabThread.StartInfo.FileName = xmltvpath;
					grabThread.StartInfo.Arguments = xmltvargs;
					grabThread.StartInfo.UseShellExecute = false;
					grabThread.StartInfo.WorkingDirectory = WorkingDir;
					grabThread.Start();
					grabThread.PriorityClass = ProcessPriorityClass.BelowNormal;
					int i = 0;
					while (!grabThread.HasExited)
					{
						//Discard cached information about the process.
						grabThread.Refresh();
						// Wait 2 seconds.
						i++;
						Thread.Sleep(2000);
					}

					// Close process 
					grabThread.Close();
				}
				catch(Exception e)
				{
					Console.WriteLine("The following exception was raised: ");
					Console.WriteLine(e.Message);
				}
			}
		}
	}
}