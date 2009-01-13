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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.GUIBurner
{
  public class BurnVideoDVD
  {
    #region enums

    private enum ConvertState
    {
      VideoConversion = 0,
      Finished = 1
    }

    private enum DVDBurnStates
    {
      Step1 = 0,
      Step2 = 1,
      Step3 = 2,
      Step4 = 3,
      Step5 = 4,
      Finished = 5
    }

    #endregion

    //class variables

    #region Class Variables

    private Process BurnerProcess; // Will run the external processes in another thread
    private ConvertState _CurrentConvertState; // Current Convert State aka Step
    private DVDBurnStates _CurrentBurnState; // Current Burn State aka Step
    private string _CurrentProcess = string.Empty; // Current Process Running
    private bool _Started = false; // Has the Processing Started
    private string _PathtoDvdBurnExe = string.Empty; // Path to the EXE files for Burning
    private string _CurrentFileName = string.Empty; // Current Filename being processed
    private ArrayList _FileNames; // ArrayList of files to process
    private int _FileNameCount = 0; // Track the file in the Array being processed
    private string _TempFolderPath = string.Empty; // Path to temp folder used
    private string _RecorderDrive = string.Empty; // CD/DVD Drive letter
    private bool _InDebugMode = false; // Debug option
    private bool _BurnTheDVD = true; // Burn the DVD
    private List<string> _FilesToBurn = new List<string>(); // Converted Files ready to Burn
    private string _TvFormat = string.Empty; // "PAL" or "NTSC"
    private string _AspectRatio = string.Empty; // "4/3" or "16/9"

    #endregion

    //events
    //in your UI class, listen in on these events to report back to user

    #region Events and Delegates

    public delegate void FileFinishedEventHandler(object sender, FileFinishedEventArgs e);

    public event FileFinishedEventHandler FileFinished;

    public event EventHandler AllFinished;

    //public event System.Diagnostics.DataReceivedEventHandler OutputReceived;

    public delegate void BurnDVDErrorEventHandler(object sender, BurnDVDErrorEventArgs e);

    public event BurnDVDErrorEventHandler BurnDVDError;

    public delegate void BurnDVDStatusUpdateEventHandler(object sender, BurnDVDStatusUpdateEventArgs e);

    public event BurnDVDStatusUpdateEventHandler BurnDVDStatusUpdate;

    #endregion

    //constructor

    #region Constructors

    ///<summary>BurnVideoDVD Class Constructor.</summary>
    ///<return>None</return>
    ///<param name="FileNames">ArrayList of Filenames to include on the VidoeDVD</param>
    ///<param name="PathToTempFolder">Path to the folder to use for creating temporary files</param>
    ///<param name="TVFormat">"NTSC" or "PAL" format for the VideoDVD</param>
    ///<param name="AspectRatio">Aspect ratio - either "4/3" or "16/9"</param>
    ///<param name="PathToDvdBurnExe">Path to the executable used to write the ISO to the DVD</param>
    ///<param name="DebugMode">Debug Mode includes more logging and does not delete the temporary files created</param>
    ///<param name="RecorderDrive">The drive letter of the Recorder</param>
    ///<param name="DummyBurn">Do everything except the burn. Used for debugging</param>
    public BurnVideoDVD(ArrayList FileNames, string PathToTempFolder, string TVFormat, string AspectRatio,
                        string PathtoDVDBurnExe, bool DebugMode, string RecorderDrive, bool DummyBurn)
    {
      _InDebugMode = DebugMode;

      // Override burn setting if in debug mode
      if (DummyBurn)
      {
        _BurnTheDVD = false;
      }

      pBurnVideoDVD(FileNames, PathToTempFolder, TVFormat, AspectRatio, PathtoDVDBurnExe, RecorderDrive);
    }

    ///<summary>Private Initialization method called by the Constructors.</summary>
    private void pBurnVideoDVD(ArrayList FileNames, string PathToTempFolder, string TVFormat, string AspectRatio,
                               string PathtoDVDBurnExe, string RecorderDrive)
    {
      _FileNames = FileNames;
      _FileNameCount = 0;
      _TvFormat = TVFormat;
      _AspectRatio = AspectRatio;
      _PathtoDvdBurnExe = PathtoDVDBurnExe;
      _Started = false;
      _TempFolderPath = PathToTempFolder;
      _RecorderDrive = RecorderDrive;

      if (_TempFolderPath.EndsWith(@"\\") || _TempFolderPath.EndsWith("//"))
      {
        _TempFolderPath = _TempFolderPath.Substring(0, (_TempFolderPath.Length - 2));
      }
      else if (_TempFolderPath.EndsWith(@"\") || _TempFolderPath.EndsWith("/"))
      {
        _TempFolderPath = _TempFolderPath.Substring(0, (_TempFolderPath.Length - 1));
      }

      // Make the DVD dir. Gets deleted straight away, but saves an exception
      Directory.CreateDirectory(_TempFolderPath);

      // Delete the temp DVD dir and any contents from any previous DVD creation.
      Directory.Delete(_TempFolderPath, true);

      // Make the DVD dir that we just deleted above
      Directory.CreateDirectory(_TempFolderPath);

      Log.Debug("BurnVideoDVDInit", "TempFolderPath: " + _TempFolderPath);
      Log.Debug("BurnVideoDVDInit", "Debug Mode: " + _InDebugMode.ToString());
    }

    #endregion

    #region Getters and Setters

    ///<summary>Called to Start the VideoDVD File Conversion and Burning.</summary>
    ///<return>True if File Conversion and Burning has Started.</return>
    ///<return>False if File Conversion and Burning has not Started.</return>
    public bool Started
    {
      get { return _Started; }
    }

    #endregion

    ///<summary>Called to Start the VideoDVD File Conversion and Burning.</summary>
    ///<return>True is Start successful.</return>
    ///<return>False if already Started.</return>
    public bool Start()
    {
      if (_Started == false)
      {
        ProvideStatusUpdate("Starting Video DVD File Conversion and Burning");
        _Started = true;

        NextFileNameConversion();
        return true;
      }
      else
      {
        return false;
      }
    }

    ///<summary>Called to do the File Conversion for each file in the ArrayList of files to process
    /// and resets the Current Convert State to Step 1
    ///</summary>
    private void NextFileNameConversion()
    {
      _FileNameCount++;

      if (_FileNameCount <= _FileNames.Count)
      {
        //get next filename and reset the state
        _CurrentFileName = (string) _FileNames[_FileNameCount - 1];
        _CurrentConvertState = ConvertState.VideoConversion;

        ProvideStatusUpdate("Processing " + _CurrentFileName);

        //start again
        NextStep_FileConversion();
      }
      else
      {
        //all finished with conversions. So Start the burning steps.
        ProvideStatusUpdate("Starting Video DVD Burning Steps");
        BurnPrep();
      }
    }

    ///<summary>Called to start the DVD Burning Prep for DVD Creation. 
    ///</summary>
    private void BurnPrep()
    {
      if (_FilesToBurn.Count > 0) // Make sure we have files to burn
      {
        // Reset the Burn State
        ProvideStatusUpdate("Video DVD Burn Preperation for " + _FilesToBurn.Count.ToString() + " Video files.");
        _CurrentBurnState = DVDBurnStates.Step1;
        NextStep_DVDCreation();
      }
      else
      {
        //All finished with Burning send the AllFinished event to all listeners
        CleanUp();
        if (AllFinished != null)
        {
          AllFinished(this, new EventArgs());
        }
      }
    }

    #region DVD Burn Steps

    ///<summary>Called to Generate the DVD Menu</summary>
    private void MenuGeneration()
    {
      try
      {
        _CurrentProcess = "Generating DVD Menu - menuGen.exe";
        Log.Debug("Entered MenuGeneration Process", "");

        ProvideStatusUpdate("Creating DVD Menus");

        StreamWriter SW_MenuGen;

        #region Generate the Menu File

        SW_MenuGen = File.CreateText(Path.Combine(_TempFolderPath, "menuGen.gen"));

        string strTemp = Path.GetDirectoryName(Application.ExecutablePath);
        strTemp = Path.Combine(strTemp, GUIGraphicsContext.Skin);
        strTemp = "Theme Folder =" + Path.Combine(strTemp, "media");

        SW_MenuGen.WriteLine(strTemp);
        SW_MenuGen.WriteLine(@"Work Folder =" + _TempFolderPath);
        SW_MenuGen.WriteLine(@"Graphics Magick =" + Config.GetFile(Config.Dir.BurnerSupport, "gm.exe"));
        SW_MenuGen.WriteLine(@"Mplex =" + Config.GetFile(Config.Dir.BurnerSupport, "mplex.exe"));
        SW_MenuGen.WriteLine(@"jpeg2yuv =" + Config.GetFile(Config.Dir.BurnerSupport, "png2yuv.exe"));
        SW_MenuGen.WriteLine(@"mpeg2enc =" + Config.GetFile(Config.Dir.BurnerSupport, "mpeg2enc.exe"));
        SW_MenuGen.WriteLine(@"spumux =" + Config.GetFile(Config.Dir.BurnerSupport, "spumux.exe"));
        SW_MenuGen.WriteLine(@"AC3 audio =" + Config.GetFile(Config.Dir.BurnerSupport, "Silence.ac3"));
        SW_MenuGen.WriteLine(@"Button Image =" + Config.GetFile(Config.Dir.BurnerSupport, "navButton.png"));
        SW_MenuGen.WriteLine(@"DVD Format (PAL or NTSC)=" + _TvFormat.ToUpper());
        if (_InDebugMode)
        {
          strTemp = "1";
        }
        else
        {
          strTemp = "0";
        }

        SW_MenuGen.WriteLine(@"Leave files for debugging (0 is false, 1 is true)=" + strTemp);

        int NumberOfFiles = _FilesToBurn.Count;

        for (int i = 0; i < NumberOfFiles; i++)
        {
          strTemp = "-------------------------Video " + i.ToString() + " -------------------------";
          SW_MenuGen.WriteLine(strTemp);

          strTemp = Path.GetFileName(_FilesToBurn[i]);
          string strVideoName = Path.GetFileNameWithoutExtension(strTemp);
          strTemp = "Video " + i.ToString() + @" Show Title= " + strVideoName;
          SW_MenuGen.WriteLine(strTemp);

          // This will come from the TV/DVD database when it gets integrated into the context menu
          //strTemp = "Video " + i.ToString() + @" Episode Title= Live Together, Die Alone";
          strTemp = "Video " + i.ToString() + @" Episode Title=";
          SW_MenuGen.WriteLine(strTemp);

          // This will come from the TV/DVD database when it gets integrated into the context menu
          //strTemp = "Video " + i.ToString() + @" Description= After discovering something odd just offshore, Jack and Sayid come up with a plan to 'confront'";
          strTemp = "Video " + i.ToString() + @" Description=";
          SW_MenuGen.WriteLine(strTemp);

          // Commented until I can work out how to take a thumbnail of the video
          // strTemp = "Video " + i.ToString() + @" Thumbnail=C:\temp\DVD\thumbnail.jpg";
          strTemp = "Video " + i.ToString() + @" Thumbnail=";
          SW_MenuGen.WriteLine(strTemp);
        }

        SW_MenuGen.Close();

        #endregion

        Log.Info("Finished MenuGeneration", "Copying MenuGen Executable");


        // Copy menugen to strTempFolder. 
        // Needs to be in same dir as the menuGen.gen file we just made above
        string SourceFile = Config.GetFile(Config.Dir.BurnerSupport, "menuGen.exe");
        string DestFile = Path.Combine(_TempFolderPath, "menuGen.exe");
        File.Copy(SourceFile, DestFile);

        Log.Debug("Finished MenuGen Executable Copy", "Starting MenuGen Execution");

        #region MenuGen execution

        // Create the DVD menu files
        BurnerProcess = new Process();
        BurnerProcess.EnableRaisingEvents = true;
        BurnerProcess.StartInfo.WorkingDirectory = _TempFolderPath;
        BurnerProcess.StartInfo.UseShellExecute = false;

        if (!_InDebugMode) // Show output if in Debug mode
        {
          BurnerProcess.StartInfo.RedirectStandardOutput = true;
          BurnerProcess.StartInfo.CreateNoWindow = true;
        }

        BurnerProcess.StartInfo.FileName = DestFile;
        BurnerProcess.StartInfo.Arguments = "";

        BurnerProcess.Exited += new EventHandler(BurnProcess_Exited);
        //        BurnerProcess.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(processOutputHandler);

        BurnerProcess.Start();

        if (!BurnerProcess.HasExited)
        {
          BurnerProcess.PriorityClass = ProcessPriorityClass.BelowNormal;
          // BurnerProcess.BeginOutputReadLine();
        }

        #endregion
      }
      catch (Exception ex)
      {
        Log.Error(ex.ToString());
      }
    }

    ///<summary>Generate the DVD Creation Configuration XML File</summary>
    private void ConfigXMLCreation()
    {
      try
      {
        _CurrentProcess = "Config.xml Writer";
        Log.Debug("Starting ConfigXMLCreation", "");

        ProvideStatusUpdate("Creating Config file for DVD Generation program");

        // Now we create the Config.xml file for DvdAuthor.exe
        StreamWriter SW_ConfigFile;

        SW_ConfigFile = File.CreateText(Path.Combine(_TempFolderPath, "Config.xml"));

        string strTemp;

        SW_ConfigFile.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        SW_ConfigFile.WriteLine("<dvdauthor>");
        SW_ConfigFile.WriteLine("  <vmgm>");
        SW_ConfigFile.WriteLine("    <menus>");
        SW_ConfigFile.WriteLine("    <video format=\"" + _TvFormat + "\" />");
        SW_ConfigFile.WriteLine("      <pgc>");

        int NumberOfFiles = _FilesToBurn.Count;

        for (int i = 0; i < NumberOfFiles; i++)
        {
          strTemp = "         <button> jump titleset " + (i + 1).ToString() + " menu; </button>";
          SW_ConfigFile.WriteLine(strTemp);
        }

        string mBkgdPath = Path.Combine(_TempFolderPath, "menuBackground.menu.mpg");
        SW_ConfigFile.WriteLine("         <vob file=\"" + mBkgdPath + "\" pause=\"5\"/>");
        SW_ConfigFile.WriteLine("      </pgc>");
        SW_ConfigFile.WriteLine("    </menus>");
        SW_ConfigFile.WriteLine("  </vmgm>");

        for (int i = 0; i < NumberOfFiles; i++)
        {
          string smBkgd = "subMenuBackground." + i.ToString() + ".menu.mpg";
          smBkgd = Path.Combine(_TempFolderPath, smBkgd);

          SW_ConfigFile.WriteLine("  <titleset>");
          SW_ConfigFile.WriteLine("    <menus>");
          SW_ConfigFile.WriteLine("    <video format=\"" + _TvFormat + "\" />");
          SW_ConfigFile.WriteLine("      <pgc>");
          SW_ConfigFile.WriteLine("        <button> jump title 1; </button>");
          SW_ConfigFile.WriteLine("        <button> jump vmgm menu; </button>");
          strTemp = "        <vob file=\"" + smBkgd + "\" pause=\"5\"/>";
          SW_ConfigFile.WriteLine(strTemp);
          SW_ConfigFile.WriteLine("      </pgc>");
          SW_ConfigFile.WriteLine("    </menus>");

          SW_ConfigFile.WriteLine("    <titles>");
          SW_ConfigFile.WriteLine("    <video format=\"" + _TvFormat + "\" />");
          SW_ConfigFile.WriteLine("      <pgc>");
          strTemp = "        <vob file=\"" + _FilesToBurn[i] +
                    "\" chapters=\"15:00,30:00,45:00,1:00:00,1:15:00,1:30:00,1:45:00,2:00:00,2:15:00,2:30:00,2:45:00,3:00:00\" />";
          SW_ConfigFile.WriteLine(strTemp);
          SW_ConfigFile.WriteLine("        <post>call vmgm menu;</post>");
          SW_ConfigFile.WriteLine("      </pgc>");
          SW_ConfigFile.WriteLine("    </titles>");
          SW_ConfigFile.WriteLine("  </titleset>");
        }

        SW_ConfigFile.WriteLine("</dvdauthor>");
        SW_ConfigFile.Close();

        // No Actual external app running to Exit so 
        // we just call BurnProcess_Exited
        EventArgs e = new EventArgs();
        BurnProcess_Exited(this, e);

        Log.Info("Finished Config XML Creation", "");
      }
      catch (Exception ex)
      {
        Log.Error(ex.ToString());
      }
    }

    ///<summary>Generate the DVD Image File</summary>
    private void DVDFilesCreation()
    {
      try
      {
        _CurrentProcess = "DVD Image Creation - dvdauthor.exe";
        Log.Debug("Entered DVDFilesCreation", "");

        ProvideStatusUpdate("Creating DVD filesystem");

        BurnerProcess = new Process();
        BurnerProcess.EnableRaisingEvents = true;
        BurnerProcess.StartInfo.WorkingDirectory = _TempFolderPath;
        BurnerProcess.StartInfo.UseShellExecute = false;
        if (!_InDebugMode) // Show output if in Debug mode
        {
          BurnerProcess.StartInfo.RedirectStandardOutput = true;
          BurnerProcess.StartInfo.CreateNoWindow = true;
        }

        string imgFolder = Path.Combine(_TempFolderPath, "DVD_Image");
        string cfgfile = Path.Combine(_TempFolderPath, "Config.xml");

        if (!Directory.Exists(imgFolder))
        {
          Directory.CreateDirectory(imgFolder);
        }

        BurnerProcess.StartInfo.FileName = Config.GetFile(Config.Dir.BurnerSupport, "dvdauthor.exe");
        string args = "-o \"" + imgFolder + "\" -x \"" + cfgfile + "\"";
        BurnerProcess.StartInfo.Arguments = args;

        BurnerProcess.Exited += new EventHandler(BurnProcess_Exited);
        //         BurnerProcess.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(processOutputHandler);

        Log.Info("Starting DVDFilesCreation Process", "Args: " + args);
        BurnerProcess.Start();

        if (!BurnerProcess.HasExited)
        {
          BurnerProcess.PriorityClass = ProcessPriorityClass.BelowNormal;
          //  BurnerProcess.BeginOutputReadLine();
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex.ToString());
      }
    }

    ///<summary>Generate the DVD ISO File</summary>
    private void ISOFileCreation()
    {
      // Make the ISO of the DVD dir that contains the VIDEO_TS and AUDIO_TS dirs
      // mkisofs -V "MyDVDName" -o mydvd.iso -dvd-video DirToMakeIsoOf

      try
      {
        _CurrentProcess = "DVD ISO Creation - mkisofs.exe";
        Log.Debug("Entered ISOFileCreation", "");

        ProvideStatusUpdate("Generating ISO image of DVD filesystem");

        BurnerProcess = new Process();
        BurnerProcess.EnableRaisingEvents = true;
        BurnerProcess.StartInfo.WorkingDirectory = Config.GetFolder(Config.Dir.BurnerSupport);
        BurnerProcess.StartInfo.UseShellExecute = false;

        if (!_InDebugMode) // Show output if in Debug mode
        {
          BurnerProcess.StartInfo.RedirectStandardOutput = true;
          BurnerProcess.StartInfo.CreateNoWindow = true;
        }

        string discName = string.Format("\"MP-DVD-{0}\"", DateTime.Now.ToShortDateString());
        string imgFolder = Path.Combine(_TempFolderPath, "DVD_Image");
        string isofile = Path.Combine(_TempFolderPath, "dvd.iso");

        BurnerProcess.StartInfo.FileName = Config.GetFile(Config.Dir.BurnerSupport, "mkisofs.exe");
        string args = "-V " + discName + " -o \"" + isofile + "\" -dvd-video \"" + imgFolder + "\"";
        BurnerProcess.StartInfo.Arguments = args;

        BurnerProcess.Exited += new EventHandler(BurnProcess_Exited);
        BurnerProcess.OutputDataReceived += new DataReceivedEventHandler(MakeISOOutputDataReceivedHandler);

        Log.Info("Starting ISOFileCreation", "Args: " + args);
        BurnerProcess.Start();

        if (!BurnerProcess.HasExited)
        {
          BurnerProcess.PriorityClass = ProcessPriorityClass.BelowNormal;
          BurnerProcess.BeginOutputReadLine();
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex.ToString());
      }
    }


    ///<summary>Burn the DVD to a Disc</summary>
    private void WriteDVD()
    {
      try
      {
        _CurrentProcess = "Burning the DVD - dvdburn.exe";

        Log.Debug("Entered WriteDVD", "BurnOption: " + _BurnTheDVD.ToString());

        if (_BurnTheDVD == true)
        {
          ProvideStatusUpdate("Burning ISO image to DVD");

          BurnerProcess = new Process();
          BurnerProcess.EnableRaisingEvents = true;
          BurnerProcess.StartInfo.WorkingDirectory = _TempFolderPath;
          BurnerProcess.StartInfo.UseShellExecute = false;

          if (!_InDebugMode) // Show output if in Debug mode
          {
            BurnerProcess.StartInfo.RedirectStandardOutput = true;
            BurnerProcess.StartInfo.CreateNoWindow = true;
          }

          //BurnerProcess.StartInfo.FileName = Path.Combine(_PathtoDvdBurnExe, "dvdburn.exe");
          BurnerProcess.StartInfo.FileName = Path.Combine(_PathtoDvdBurnExe, "dvdburn.exe");

          //string isofile = Path.Combine(_TempFolderPath, "dvd.iso");

          string args = _RecorderDrive + " " + Path.Combine(_TempFolderPath, "dvd.iso");
          ;

          BurnerProcess.StartInfo.Arguments = args;

          BurnerProcess.Exited += new EventHandler(BurnProcess_Exited);
          //            BurnerProcess.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(processOutputHandler);

          Log.Info("Starting DVDBurn", "Args: " + args);
          BurnerProcess.Start();

          if (!BurnerProcess.HasExited)
          {
            BurnerProcess.PriorityClass = ProcessPriorityClass.BelowNormal;
            // BurnerProcess.BeginOutputReadLine();
          }
        }
        else
        {
          _CurrentProcess = "DVD Burning is Disabled";
          BurnProcess_Exited(this, new EventArgs());
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex.ToString());
      }
    }

    #endregion

    ///<summary>Called for each Step in the DVD Creation after the File Conversion(s). </summary>
    private void NextStep_DVDCreation()
    {
      Log.Debug("NextStep_DVDCreation", "CurrentState: " + _CurrentBurnState.ToString());
      switch (_CurrentBurnState)
      {
        case DVDBurnStates.Step1: // Menu Generation
          MenuGeneration();
          break;

        case DVDBurnStates.Step2: // XML Config File Creation
          ConfigXMLCreation();
          break;

        case DVDBurnStates.Step3: // DVD Files Creation
          DVDFilesCreation();
          break;

        case DVDBurnStates.Step4: // ISO File Creation
          ISOFileCreation();
          break;

        case DVDBurnStates.Step5: // Write DVD
          WriteDVD();
          break;

        case DVDBurnStates.Finished: // Finished
          //converting process completed, raise event
          _Started = false;

          CleanUp();

          ProvideStatusUpdate("Completed Video DVD Burning");

          if (AllFinished != null)
          {
            AllFinished(this, new EventArgs());
          }
          break;
      }
    }

    ///<summary>Called for each File Stepping through the File Conversion(s).</summary>
    private void NextStep_FileConversion()
    {
      Log.Debug("NextStep_FileConversion",
                "CurrentState: " + _CurrentConvertState.ToString() + " CurrentFile: " + _CurrentFileName);
      switch (_CurrentConvertState)
      {
          #region Convert input file to a DVD formatted MPG file using Mencoder

        case ConvertState.VideoConversion:
          {
            string strFileName = Path.GetFileNameWithoutExtension(_CurrentFileName);
            ProvideStatusUpdate("Converting \"" + strFileName + "\" to DVD format");


            string DestinationFilePath = Path.GetFileNameWithoutExtension(_CurrentFileName);
            DestinationFilePath = DestinationFilePath + ".mpg";
            DestinationFilePath = Path.Combine(_TempFolderPath, DestinationFilePath);


            string SourceFilePath = _CurrentFileName;

            // Set the current filename to the new output file
            _CurrentFileName = DestinationFilePath; // Ready for next file?? 

            _CurrentProcess = "Video file conversion - mencoder.exe";

            try
            {
              BurnerProcess = new Process();
              BurnerProcess.EnableRaisingEvents = true;
                // Gets or sets whether the Exited event should be raised when the process terminates. 
              BurnerProcess.StartInfo.WorkingDirectory = Config.GetFolder(Config.Dir.BurnerSupport);
              BurnerProcess.StartInfo.UseShellExecute = false;

              if (!_InDebugMode) // Show output if in Debug mode
              {
                BurnerProcess.StartInfo.RedirectStandardOutput = true;
                BurnerProcess.StartInfo.CreateNoWindow = true;
              }

              BurnerProcess.StartInfo.FileName = Config.GetFile(Config.Dir.BurnerSupport, "mencoder.exe");

              string args = string.Empty;
              if (_TvFormat.ToUpper() == "PAL")
              {
                args =
                  "-oac lavc -ovc lavc -of mpeg -mpegopts format=dvd:tsaf -vf scale=720:576,harddup -srate 48000 -af lavcresample=48000 -lavcopts vcodec=mpeg2video:vrc_buf_size=1835:vrc_maxrate=9800:vbitrate=5000:keyint=15:acodec=ac3:abitrate=192:aspect=" +
                  _AspectRatio + " -ofps 25 -o \"" + DestinationFilePath + "\"  \"" + SourceFilePath + "\" ";
              }
              else
              {
                args =
                  "-oac lavc -ovc lavc -of mpeg -mpegopts format=dvd:tsaf -vf scale=720:480,harddup -srate 48000 -af lavcresample=48000 -lavcopts vcodec=mpeg2video:vrc_buf_size=1835:vrc_maxrate=9800:vbitrate=5000:keyint=18:acodec=ac3:abitrate=192:aspect=" +
                  _AspectRatio + " -ofps 30000/1001 -o \"" + DestinationFilePath + "\"  \"" + SourceFilePath + "\" ";
              }

              BurnerProcess.StartInfo.Arguments = args;

              BurnerProcess.Exited += new EventHandler(BurnerProcess_Exited);
              BurnerProcess.OutputDataReceived += new DataReceivedEventHandler(FileConversionOutputDataReceivedHandler);

              Log.Debug("Starting: " + _CurrentProcess, "Exe Arguments: " + args);

              BurnerProcess.Start();

              if (!BurnerProcess.HasExited)
              {
                //BurnerProcess.PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;
                BurnerProcess.BeginOutputReadLine();
              }
            }
            catch (Exception ex)
            {
              Log.Error(ex.ToString());
            }
          }
          break;

          #endregion

          #region Finished File Conversion Step   Start Next File

        case ConvertState.Finished: // Finished one file Start Next

          ProvideStatusUpdate("Completed File Conversion For: " + (string) _FileNames[_FileNameCount - 1]);

          if (FileFinished != null)
          {
            FileFinished(this, new FileFinishedEventArgs((string) _FileNames[_FileNameCount - 1], _CurrentFileName));
          }
          _FilesToBurn.Add(_CurrentFileName);
          NextFileNameConversion();

          break;

          #endregion
      }
    }

    #region Events etc.

    ///<summary>Called to provide status updates to any BurnVideoDVDStatusUpdate event listeners</summary>
    private void ProvideStatusUpdate(string status)
    {
      Log.Debug("ProvideStatusUpdate: ", status.ToString());

      if (BurnDVDStatusUpdate != null)
      {
        BurnDVDStatusUpdateEventArgs be = new BurnDVDStatusUpdateEventArgs(status);
        //announce to anyone who is listening
        BurnDVDStatusUpdate(this, be);
      }
    }


    ///<summary>Called when each Conversion Process Step has completed
    ///to move to the next step.</summary>
    private void BurnerProcess_Exited(object sender, EventArgs e)
    {
      //Log.Debug("Convert Video Step Exited: ", _CurrentConvertState.ToString());
      ProvideStatusUpdate("Convert Process Exited: " + _CurrentProcess);

      //one process has finished, start next process
      _CurrentConvertState += 1;
      NextStep_FileConversion();
    }

    ///<summary>Called when each Burn Process Step has completed.
    ///Also announces the CompletedStep event to any listeners</summary>
    private void BurnProcess_Exited(object sender, EventArgs e)
    {
      Log.Debug("Burn DVD Step Exited: Step: ", _CurrentBurnState.ToString());
      ProvideStatusUpdate("DVD Burn Process Exited: " + _CurrentProcess);

      //one process has finished, start next process
      _CurrentBurnState += 1;
      NextStep_DVDCreation();
    }


    private void FileConversionOutputDataReceivedHandler(object sender, DataReceivedEventArgs e)
    {
      string sout = "Redirected StandardOutput IsEmpty";

      if (!String.IsNullOrEmpty(e.Data))
      {
        sout = e.Data.ToString();
      }

      //Pos:  76.5s   1835f (99%) 34.31fps Trem:   0min  35mb  A-V:0.017 [3646:191]

      string Percentage = "";
      string TimeLeft = "";


      if (sout.StartsWith("Pos") == true)
      {
        Percentage = sout.Substring(sout.IndexOf('(') + 1, 2);
        TimeLeft = sout.Substring(sout.IndexOf("Trem:") + 5, 4);

        byte Temp = Convert.ToByte(Percentage);
        if (Temp%5 == 0)
        {
          ProvideStatusUpdate(Percentage + "% done. Time Left = " + TimeLeft.ToString() + " min");
        }
      }


//      if (OutputReceived != null)
//      {
//        OutputReceived(this, e);
//      }
    }

    private void MakeISOOutputDataReceivedHandler(object sender, DataReceivedEventArgs e)
    {
      //     Debugger.Launch();
      //   Debugger.Break();

/*      string sout = "Redirected StandardOutput IsEmpty";

      if (!String.IsNullOrEmpty(e.Data))
        sout = e.Data.ToString();

      //Pos:  76.5s   1835f (99%) 34.31fps Trem:   0min  35mb  A-V:0.017 [3646:191]

      string Percentage = "";
      string TimeLeft = "";


      if (sout.StartsWith("Pos") == true)
      {
        Percentage = sout.Substring(sout.IndexOf('(') + 1, 2);
        TimeLeft = sout.Substring(sout.IndexOf("Trem:") + 5, 4);

        byte Temp = Convert.ToByte(Percentage);
        if (Temp % 5 == 0)
          ProvideStatusUpdate(Percentage + "% done. Time Left = " + TimeLeft.ToString() + " min");
      }

      //      if (OutputReceived != null)
      //      {
      //        OutputReceived(this, e);
      //      }
 */
    }


    private void ProcessingError(string ErrorTitle, string ErrorText)
    {
      Log.Error("Processing Error: " + ErrorTitle, ErrorText);
      ProvideStatusUpdate("Processing Error: " + ErrorTitle + ": " + ErrorText);
      if (BurnDVDError != null)
      {
        BurnDVDErrorEventArgs be = new BurnDVDErrorEventArgs(ErrorTitle, ErrorText);
        BurnDVDError(this, be);
      }
    }

    #endregion

    //#region LogWriting
    /////<summary>Called to Write to the MediaPortal.Log file when in DebugMode.</summary>
    /////<param name="EntryTitle">Log Entry Title Text.</param>
    /////<param name="EntryText">Log entry Main Text.</param>
    //private void Log.Debug(string EntryTitle, string EntryText)
    //{
    //    Log.Info("My Burner Plugin->BurnVideoDVD Class: {0} - {1}", EntryTitle, EntryText);
    //}
    //#endregion

    ///<summary>Simple Cleanup that deletes the temp files and directory</summary>
    private void CleanUp()
    {
      ProvideStatusUpdate("Performing Cleanup of Temporary Files");
      if (!_InDebugMode)
      {
        // Delete the temp DVD dir and any contents from any previous DVD creation.
        Directory.Delete(_TempFolderPath, true);
      }
      else
      {
        ProvideStatusUpdate("Temporary Files Not Deleted: In Debug Mode");
      }
    }
  }
}