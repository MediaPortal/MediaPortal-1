#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.GUIBurner
{
  public class BurnDataDVD
  {
    #region enums

    private enum CopyState
    {
      FileCopy = 0,
      Finished = 1
    }

    private enum DVDBurnStates
    {
      Step1 = 0,
      Step2 = 1,
      Finished = 2
    }

    #endregion

    //class variables

    #region Class Variables

    private Process BurnerProcess; // Will run the external processes in another thread
    private CopyState _CurrentCopyState; // Current Convert State aka Step
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

    #endregion

    //events
    //in your UI class, listen in on these events to report back to user

    #region Events and Delegates

    public delegate void FileFinishedEventHandler(object sender, FileFinishedEventArgs e);

    public event FileFinishedEventHandler FileFinished;

    public event EventHandler AllFinished;

    public delegate void BurnDVDErrorEventHandler(object sender, BurnDVDErrorEventArgs e);

    public event BurnDVDErrorEventHandler BurnDVDError;

    public delegate void BurnDVDStatusUpdateEventHandler(object sender, BurnDVDStatusUpdateEventArgs e);

    public event BurnDVDStatusUpdateEventHandler BurnDVDStatusUpdate;

    #endregion

    //constructor

    #region Constructors

    ///<summary>BurnDataDVD Class Constructor.</summary>
    ///<return>None</return>
    ///<param name="FileNames">ArrayList of Filenames to include on the VidoeDVD</param>
    ///<param name="PathToTempFolder">Path to the folder to use for creating temporary files</param>
    ///<param name="PathToDvdBurnExe">Path to the executable used to write the ISO to the DVD</param>
    ///<param name="DebugMode">Debug Mode includes more logging and does not delete the temporary files created</param>
    ///<param name="RecorderDrive">The drive letter of the Recorder</param>
    ///<param name="DummyBurn">Do everything except the burn. Used for debugging</param>
    public BurnDataDVD(ArrayList FileNames, string PathToTempFolder, string PathtoDVDBurnExe, bool DebugMode,
                       string RecorderDrive, bool DummyBurn)
    {
      _InDebugMode = DebugMode;

      // Override burn setting if in debug mode
      if (DummyBurn)
      {
        _BurnTheDVD = false;
      }

      pBurnDataDVD(FileNames, PathToTempFolder, PathtoDVDBurnExe, RecorderDrive);
    }

    ///<summary>Private Initialization method called by the Constructors.</summary>
    private void pBurnDataDVD(ArrayList FileNames, string PathToTempFolder, string PathtoDVDBurnExe,
                              string RecorderDrive)
    {
      _FileNames = FileNames;
      _FileNameCount = 0;
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

      Directory.CreateDirectory(_TempFolderPath + "/DVD_Image");

      LogWrite("BurnDataDVDInit", "TempFolderPath: " + _TempFolderPath);
      LogWrite("BurnDataDVDInit", "Debug Mode: " + _InDebugMode.ToString());
    }

    #endregion

    #region Getters and Setters

    ///<summary>Called to Start the DataDVD File Conversion and Burning.</summary>
    ///<return>True if File Conversion and Burning has Started.</return>
    ///<return>False if File Conversion and Burning has not Started.</return>
    public bool Started
    {
      get { return _Started; }
    }

    #endregion

    ///<summary>Called to Start the DataDVD File Conversion and Burning.</summary>
    ///<return>True is Start successful.</return>
    ///<return>False if already Started.</return>
    public bool Start()
    {
      if (_Started == false)
      {
        ProvideStatusUpdate("Starting Data DVD Burning");
        _Started = true;

        NextFileCopy();
        return true;
      }
      else
      {
        return false;
      }
    }

    ///<summary>Called to do the File Copy for each file in the ArrayList of files to process
    /// and resets the Current Copy State to Step 1
    ///</summary>
    private void NextFileCopy()
    {
      _FileNameCount++;

      if (_FileNameCount <= _FileNames.Count)
      {
        //get next filename and reset the state
        _CurrentFileName = (string)_FileNames[_FileNameCount - 1];
        _CurrentCopyState = CopyState.FileCopy;

        ProvideStatusUpdate("Copying " + _CurrentFileName);

        //start again
        NextStep_FileCopy();
      }
      else
      {
        //all finished with conversions. So Start the burning steps.
        ProvideStatusUpdate("Starting Data DVD Burning Steps");
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
        ProvideStatusUpdate("Data DVD Burn Preperation for " + _FilesToBurn.Count.ToString() + " Data files.");
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

    ///<summary>Generate the DVD ISO File</summary>
    private void ISOFileCreation()
    {
      // Make the ISO of the DVD dir that contains the Data_TS and AUDIO_TS dirs
      // mkisofs -V "MyDVDName" -o mydvd.iso DirToMakeIsoOf

      try
      {
        _CurrentProcess = "DVD ISO Creation - mkisofs.exe";
        LogWrite("Entered ISOFileCreation", "");

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
        string args = "-V " + discName + " -l -allow-lowercase -o \"" + isofile + "\" \"" + imgFolder + "\"";
        BurnerProcess.StartInfo.Arguments = args;

        BurnerProcess.Exited += new EventHandler(BurnProcess_Exited);
        BurnerProcess.OutputDataReceived += new DataReceivedEventHandler(MakeISOOutputDataReceivedHandler);

        LogWrite("Starting ISOFileCreation", "Args: " + args);
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

        LogWrite("Entered WriteDVD", "BurnOption: " + _BurnTheDVD.ToString());

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

          LogWrite("Starting DVDBurn", "Args: " + args);
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

    ///<summary>Called for each Step in the DVD Creation after the File Copy(ing). </summary>
    private void NextStep_DVDCreation()
    {
      LogWrite("NextStep_DVDCreation", "CurrentState: " + _CurrentBurnState.ToString());
      switch (_CurrentBurnState)
      {
        case DVDBurnStates.Step1: // ISO File Creation
          ISOFileCreation();
          break;

        case DVDBurnStates.Step2: // Write DVD
          WriteDVD();
          break;

        case DVDBurnStates.Finished: // Finished
          //converting process completed, raise event
          _Started = false;

          CleanUp();

          ProvideStatusUpdate("Completed Data DVD Burning");

          if (AllFinished != null)
          {
            AllFinished(this, new EventArgs());
          }
          break;
      }
    }

    ///<summary>Called for each File Stepping through the File Conversion(s).</summary>
    private void NextStep_FileCopy()
    {
      LogWrite("NextStep_FileCopy",
               "CurrentState: " + _CurrentCopyState.ToString() + " CurrentFile: " + _CurrentFileName);

      switch (_CurrentCopyState)
      {
          #region Convert input file to a the DVD temp folder

        case CopyState.FileCopy:
          {
            string strFileName = Path.GetFileName(_CurrentFileName);
            ProvideStatusUpdate("Copying \"" + strFileName);


            string DestinationFilePath = Path.GetFileName(_CurrentFileName);
            DestinationFilePath = Path.Combine(_TempFolderPath + "/DVD_Image", DestinationFilePath);


            string SourceFilePath = _CurrentFileName;

            // Set the current filename to the new output file
            _CurrentFileName = DestinationFilePath; // Ready for next file?? 

            _CurrentProcess = "Data file copy";

            FileInfo fi = new FileInfo(_TempFolderPath + "/Copy.bat");
            StreamWriter sw = fi.CreateText();
            string CopyLine = "copy /Y \"" + SourceFilePath + "\" \"" + DestinationFilePath + "\" ";
            sw.WriteLine(CopyLine);
            sw.Close();

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
              BurnerProcess.StartInfo.FileName = fi.FullName;

              BurnerProcess.Exited += new EventHandler(FileCopyProcess_Exited);

              LogWrite("Starting File Copy", "Starting File Copy");

              BurnerProcess.Start();

/*              if (!BurnerProcess.HasExited)
              {
                //BurnerProcess.PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;
                BurnerProcess.BeginOutputReadLine();
              }
 */
            }
            catch (Exception ex)
            {
              Log.Error(ex.ToString());
            }
          }
          break;

          #endregion

          #region Finished File Copy Step   Start Next File

        case CopyState.Finished: // Finished one file Start Next

          ProvideStatusUpdate("Completed File Copy For: " + (string)_FileNames[_FileNameCount - 1]);

          if (FileFinished != null)
          {
            FileFinished(this, new FileFinishedEventArgs((string)_FileNames[_FileNameCount - 1], _CurrentFileName));
          }
          _FilesToBurn.Add(_CurrentFileName);
          NextFileCopy();

          break;

          #endregion
      }
    }

    #region Events etc.

    ///<summary>Called to provide status updates to any BurnDataDVDStatusUpdate event listeners</summary>
    private void ProvideStatusUpdate(string status)
    {
      if (_InDebugMode)
      {
        Log.Debug("ProvideStatusUpdate: {0}", status.ToString());
      }

      if (BurnDVDStatusUpdate != null)
      {
        BurnDVDStatusUpdateEventArgs be = new BurnDVDStatusUpdateEventArgs(status);
        //announce to anyone who is listening
        BurnDVDStatusUpdate(this, be);
      }
    }


    ///<summary>Called when each Conversion Process Step has completed
    ///to move to the next step.</summary>
    private void FileCopyProcess_Exited(object sender, EventArgs e)
    {
      //LogWrite("Convert Data Step Exited: ", _CurrentCopyState.ToString());
      ProvideStatusUpdate("Convert Process Exited: " + _CurrentProcess);

      //one process has finished, start next process
      _CurrentCopyState += 1;
      NextStep_FileCopy();
    }

    ///<summary>Called when each Burn Process Step has completed.
    ///Also announces the CompletedStep event to any listeners</summary>
    private void BurnProcess_Exited(object sender, EventArgs e)
    {
      LogWrite("Burn DVD Step Exited: Step: ", _CurrentBurnState.ToString());
      ProvideStatusUpdate("DVD Burn Process Exited: " + _CurrentProcess);

      //one process has finished, start next process
      _CurrentBurnState += 1;
      NextStep_DVDCreation();
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
      Log.Error("Processing Error: {0} - {1}", ErrorTitle, ErrorText);
      ProvideStatusUpdate("Processing Error: " + ErrorTitle + ": " + ErrorText);
      if (BurnDVDError != null)
      {
        BurnDVDErrorEventArgs be = new BurnDVDErrorEventArgs(ErrorTitle, ErrorText);
        BurnDVDError(this, be);
      }
    }

    #endregion

    #region LogWriting

    ///<summary>Called to Write to the MediaPortal.Log file</summary>
    ///<param name="EntryTitle">Log Entry Title Text.</param>
    ///<param name="EntryText">Log entry Main Text.</param>
    private void LogWrite(string EntryTitle, string EntryText)
    {
      Log.Info("My Burner Plugin->BurnDataDVD Class: {0} - {1}", EntryTitle, EntryText);
    }

    #endregion

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