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
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Xml;
using SQLite.NET;
using MediaPortal.Ripper;
using MediaPortal.Player;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using WindowPlugins.GUIPrograms;
using Programs.Utils;

namespace ProgramsDatabase
{
  /// <summary>
  /// Summary description for AppItem.
  /// </summary>
  public class AppItem
  {
    protected static SQLiteClient sqlDB = null;
    private ProgramDBComparer dbPc = new ProgramDBComparer();

    public delegate void FilelinkLaunchEventHandler(FilelinkItem curLink, bool mpGuiMode);

    public event FilelinkLaunchEventHandler OnLaunchFilelink = null;

    int appID;
    int fatherID;
    string title;
    string shortTitle;
    string filename;
    string arguments;
    ProcessWindowStyle windowStyle;
    string startupDir;
    bool useShellExecute;
    bool useQuotes;
    myProgSourceType sourceType;
    string sourceFile;
    string imageFile;
    string imageDirectories; // in one string for sqlite db field
    public string[] imageDirs; // imageDirectories splitted
    string fileDirectory;
    string validExtensions;
    bool importValidImagesOnly;
    int appPosition;
    bool enabled;
    bool enableGUIRefresh;
    int pincode;
    int contentID;
    string systemDefault;
    bool waitForExit;
    string preLaunch;
    string postLaunch;


    string launchErrorMsg;


    // two magic image-slideshow counters
    int thumbIndex = 0;
    int thumbFolderIndex = -1;

    string lastFilepath = ""; // cached path

    protected bool filesAreLoaded = false; // load on demand....
    protected Filelist fileList = null;

    protected bool linksAreLoaded = false;
    protected FilelinkList fileLinks = null;

    // event: read new file
    public delegate void RefreshInfoEventHandler(string curLine);

    public event RefreshInfoEventHandler OnRefreshInfo = null;

    protected void SendRefreshInfo(string Message)
    {
      if (OnRefreshInfo != null)
      {
        OnRefreshInfo(Message);
      }
    }

    protected int GetID = ProgramUtils.GetID;

    public AppItem(SQLiteClient initSqlDB)
    {
      // constructor: save SQLiteDB object 
      sqlDB = initSqlDB;
      // .. init member variables ...
      appID = -1;
      fatherID = -1;
      title = "";
      shortTitle = "";
      filename = "";
      arguments = "";
      windowStyle = ProcessWindowStyle.Normal;
      startupDir = "";
      useShellExecute = false;
      useQuotes = true;
      enabled = true;
      sourceType = myProgSourceType.UNKNOWN;
      sourceFile = "";
      imageFile = "";
      fileDirectory = "";
      imageDirectories = "";
      validExtensions = "";
      appPosition = 0;
      importValidImagesOnly = false;
      enableGUIRefresh = false;
      pincode = -1;
      contentID = 100;
      systemDefault = "";
      waitForExit = true;
      filesAreLoaded = false;
      preLaunch = "";
      postLaunch = "";
    }

    public SQLiteClient db
    {
      get { return sqlDB; }
    }

    public int CurrentSortIndex
    {
      get { return GetCurrentSortIndex(); }
      set { SetCurrentSortIndex(value); }
    }

    public bool CurrentSortIsAscending
    {
      get { return GetCurrentSortIsAscending(); }
      set { SetCurrentSortIsAscending(value); }
    }


    public FileItem PrevFile(FileItem curFile)
    {
      if (Files == null)
      {
        return null;
      }
      if (Files.Count == 0)
      {
        return null;
      }
      int index = this.Files.IndexOf(curFile);
      index = index - 1;
      if (index < 0)
        index = Files.Count - 1;
      return (FileItem) Files[index];
    }

    public FileItem NextFile(FileItem curFile)
    {
      if (Files == null)
      {
        return null;
      }
      if (Files.Count == 0)
      {
        return null;
      }
      int index = this.Files.IndexOf(curFile);
      index = index + 1;
      if (index > Files.Count - 1)
        index = 0;
      return (FileItem) Files[index];
    }

    protected void LaunchGenericPlayer(string command, string filename)
    {
      // quick&dirty: remove placeholder out of the filename
      filename = filename.Replace("%PLAY%", "");
      filename = filename.Replace("%PLAYAUDIOSTREAM%", "");
      filename = filename.Replace("%PLAYVIDEOSTREAM%", "");
      // don't use quotes!
      filename = filename.Trim();
      filename = filename.TrimStart('\"');
      filename = filename.TrimEnd('\"');
      if (command == "%PLAY%")
      {
        g_Player.Play(filename);
      }
      else if (command == "%PLAYAUDIOSTREAM%")
      {
        g_Player.PlayAudioStream(filename);
      }
      else if (command == "%PLAYVIDEOSTREAM%")
      {
        g_Player.PlayVideoStream(filename);
      }
      else
      {
        Log.Write("error in myPrograms: AppItem.LaunchGenericPlayer: unknown command: {0}", command);
        return;
      }
      if (Utils.IsVideo(filename))
      {
        GUIGraphicsContext.IsFullScreenVideo = true;
        GUIWindowManager.ActivateWindow((int) GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
      }
      return;
    }

    public virtual void LaunchFile(FileItem curFile, bool mpGuiMode)
    {
      string curFilename = curFile.Filename;
      if (curFilename == "")
      {
        return;
      }

      // Launch File by item
      if (mpGuiMode)
      {
        curFile.UpdateLaunchInfo();
      }
      ProcessStartInfo procStart = new ProcessStartInfo();
      if (Filename != "")
      {
        // use the APPLICATION launcher and add current file information
        procStart.FileName = Filename; // filename of the application
        // set the arguments: one of the arguments is the fileitem-filename
        procStart.Arguments = " " + this.Arguments + " ";
        if (UseQuotes)
        {
          // avoid double quotes around the filename-argument.....
          curFilename = "\"" + (curFile.Filename.TrimStart('\"')).TrimEnd('\"') + "\"";
        }

        if (procStart.Arguments.IndexOf("%FILEnoPATHnoEXT%") >= 0)
        {
          // ex. kawaks:
          // winkawaks.exe alpham2
          // => filename without path and extension is necessary!
          string filenameNoPathNoExt = curFile.ExtractFileName();
          filenameNoPathNoExt = (filenameNoPathNoExt.TrimStart('\"')).TrimEnd('\"');
          filenameNoPathNoExt = Path.GetFileNameWithoutExtension(filenameNoPathNoExt);
          procStart.Arguments = procStart.Arguments.Replace("%FILEnoPATHnoEXT%", filenameNoPathNoExt);
        }
        else
        {
          // the fileitem-argument can be positioned anywhere in the argument string...
          if (procStart.Arguments.IndexOf("%FILE%") == -1)
          {
            // no placeholder found => default handling: add the fileitem as the last argument
            procStart.Arguments = procStart.Arguments + curFilename;
          }
          else
          {
            // placeholder found => replace the placeholder by the correct filename
            procStart.Arguments = procStart.Arguments.Replace("%FILE%", curFilename);
          }
        }
        procStart.WorkingDirectory = Startupdir;
        if (procStart.WorkingDirectory.IndexOf("%FILEDIR%") != -1)
        {
          procStart.WorkingDirectory = procStart.WorkingDirectory.Replace("%FILEDIR%", Path.GetDirectoryName(curFile.Filename));
        }
        procStart.UseShellExecute = UseShellExecute;
      }
      else
      {
        // application has no launch-file 
        // => try to make a correct launch using the current FILE object
        string guessedFilename = curFile.ExtractFileName();
        procStart.FileName = guessedFilename;
        procStart.Arguments = curFile.ExtractArguments();
        if (procStart.Arguments != "")
        {
          guessedFilename = procStart.Arguments;
        }
        procStart.WorkingDirectory = curFile.ExtractDirectory(guessedFilename);
        procStart.UseShellExecute = UseShellExecute;
      }
      procStart.WindowStyle = this.WindowStyle;


      bool useGenericPlayer = (procStart.FileName.ToUpper() == "%PLAY%") ||
        (procStart.FileName.ToUpper() == "%PLAYAUDIOSTREAM%") ||
        (procStart.FileName.ToUpper() == "%PLAYVIDEOSTREAM%");

      this.LaunchErrorMsg = "";
      try
      {
        DoPreLaunch();
        if (useGenericPlayer)
        {
          // use generic player
          if (mpGuiMode)
          {
            LaunchGenericPlayer(procStart.FileName, curFilename);
            return;
          }
          else
          {
            // generic player can only be used in MPGUI mode! 
            // => Apologize to the user :-)
            string problemString = "Sorry! The internal generic players cannot be used in Configuration. \nTry it in the MediaPortal application!";
            this.LaunchErrorMsg = problemString;
          }
        }
        else
        {
          if (mpGuiMode)
          {
            AutoPlay.StopListening();
            if (g_Player.Playing)
            {
              g_Player.Stop();
            }
          }
          Utils.StartProcess(procStart, WaitForExit);
          if (mpGuiMode)
          {
            GUIGraphicsContext.DX9Device.Reset(GUIGraphicsContext.DX9Device.PresentationParameters);
            AutoPlay.StartListening();
          }
        }
      }
      catch (Exception ex)
      {
        string ErrorString = String.Format("myPrograms: error launching program\n  filename: {0}\n  arguments: {1}\n  WorkingDirectory: {2}\n  stack: {3} {4} {5}",
                                           procStart.FileName,
                                           procStart.Arguments,
                                           procStart.WorkingDirectory,
                                           ex.Message,
                                           ex.Source,
                                           ex.StackTrace);
        Log.Write(ErrorString);
        this.LaunchErrorMsg = ErrorString;
      }
      finally
      {
        DoPostLaunch();
      }
    }

    protected void DoPreLaunch()
    {
      if (waitForExit && (preLaunch != ""))
      {
        LaunchCmd(preLaunch);
      }
    }

    protected void DoPostLaunch()
    {
      if (waitForExit && (preLaunch != ""))
      {
        LaunchCmd(postLaunch);
      }
    }

    protected void LaunchCmd(string commands)
    {
      string results = "";
      string errors = "";
      string[] script;
      string curLine;
      Process p = new Process();
      StreamWriter sw;
      StreamReader sr;
      StreamReader err;

      script = commands.Split('\r');
      if (script.Length > 0)
      {
        ProcessStartInfo psI = new ProcessStartInfo("cmd");
        psI.UseShellExecute = false;
        psI.RedirectStandardInput = true;
        psI.RedirectStandardOutput = true;
        psI.RedirectStandardError = true;
        psI.CreateNoWindow = true;
        p.StartInfo = psI;

        p.Start();
        sw = p.StandardInput;
        sr = p.StandardOutput;
        err = p.StandardError;

        sw.AutoFlush = true;

        for (int i = 0; i < script.Length; i++)
        {
          curLine = script[i].Trim();
          curLine = curLine.TrimStart('\n');
          if (curLine != "")
            sw.WriteLine(curLine);
        }
        sw.Close();

        results += sr.ReadToEnd();
        errors += err.ReadToEnd();

        if (errors.Trim() != "")
        {
          Log.Write("AppItem PrePost errors: {0}", errors);
        }
      }
      
    }

    public virtual void LaunchFile(GUIListItem item)
    {
      // Launch File by GUILISTITEM
      // => look for FileItem and launch it using the found object
      if (item.MusicTag == null)
      {
        return;
      }
      FileItem curFile = (FileItem) item.MusicTag;
      if (curFile == null)
      {
        return;
      }
      this.LaunchFile(curFile, true);
    }

    protected virtual void LaunchFilelink(FilelinkItem curLink, bool MPGUIMode)
    {
      this.OnLaunchFilelink(curLink, MPGUIMode);
    }

    public virtual string DefaultFilepath()
    {
      return ""; // override this if the appitem can have subfolders
    }

    public virtual int DisplayFiles(string filePath, GUIFacadeControl facadeView)
    {
      int totalItems = 0;
      if (filePath != lastFilepath)
      {
        Files.Load(AppID, filePath);
        Filelinks.Load(AppID, filePath);
      }
      totalItems = totalItems + DisplayArrayList(filePath, this.Files, facadeView);
      totalItems = totalItems + DisplayArrayList(filePath, this.Filelinks, facadeView);
      lastFilepath = filePath;
      return totalItems;
    }

    protected int DisplayArrayList(string filePath, ArrayList dbItems, GUIFacadeControl facadeView)
    {
      int totalItems = 0;
      //foreach (FileItem curFile in dbItems)
      foreach (object obj in dbItems)
      {
        totalItems = totalItems + 1;
        if (obj is FileItem)
        {
          FileItem curFile = obj as FileItem;
          GUIListItem gli = new GUIListItem(curFile.Title);
          gli.Label2 = curFile.Title2; 
          gli.MusicTag = curFile;
          gli.IsFolder = curFile.IsFolder;
          gli.OnRetrieveArt += new MediaPortal.GUI.Library.GUIListItem.RetrieveCoverArtHandler(OnRetrieveCoverArt);
          gli.OnItemSelected += new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(OnItemSelected);
          facadeView.Add(gli);
        }
        else if (obj is ProgramFilterItem)
        {
          ProgramFilterItem curFilter = obj as ProgramFilterItem;
          GUIListItem gli = new GUIListItem(curFilter.Title);
          gli.Label2 = curFilter.Title2; // some filters may have more than one text
          gli.MusicTag = curFilter;
          gli.IsFolder = true;
          facadeView.Add(gli);
        }
      }
      return totalItems;
    }


    void OnRetrieveCoverArt(GUIListItem gli)
    {
      if ((gli.MusicTag != null) && (gli.MusicTag is FileItem))
      {
        FileItem curFile = (FileItem) gli.MusicTag;
        if (curFile.Imagefile != "")
        {
          gli.ThumbnailImage = curFile.Imagefile;
          gli.IconImageBig = curFile.Imagefile;
          gli.IconImage = curFile.Imagefile;
        }
        else
        {
          gli.ThumbnailImage = GUIGraphicsContext.Skin + @"\media\DefaultFolderBig.png";
          gli.IconImageBig = GUIGraphicsContext.Skin + @"\media\DefaultFolderBig.png";
          gli.IconImage = GUIGraphicsContext.Skin + @"\media\DefaultFolderNF.png";
        }
      }
    }

    private void OnItemSelected(GUIListItem item, GUIControl parent)
    {
      GUIFilmstripControl filmstrip = parent as GUIFilmstripControl;
      if (filmstrip == null) return;
      if (item == null) return;
      if ((item.MusicTag != null) && (item.MusicTag is FileItem) && (!item.IsFolder))
      {
        filmstrip.InfoImageFileName = item.ThumbnailImage;
      }
      else
      {
        filmstrip.InfoImageFileName = "";
      }
    }


    public virtual void OnSort(GUIFacadeControl view, bool doSwitchState)
    {
/*
 *       if (!filesAreLoaded)
      {
        LoadFiles();
      }

      if (doSwitchState)
      {
        dbPc.updateState();
      }
      view.Sort(dbPc);
*/      
    }

    public virtual void OnSortToggle(GUIFacadeControl view)
    {
      dbPc.sortAscending = (!dbPc.sortAscending);
      view.Sort(dbPc);
    }

    public virtual int GetCurrentSortIndex()
    {
      return dbPc.currentSortMethodIndex;
    }

    public virtual void SetCurrentSortIndex(int newValue)
    {
      dbPc.currentSortMethodIndex = newValue;
    }

    public virtual string CurrentSortTitle()
    {
      return dbPc.currentSortMethodAsText;
    }

    public virtual bool GetCurrentSortIsAscending()
    {
      return dbPc.sortAscending;
    }

    public virtual void SetCurrentSortIsAscending(bool newValue)
    {
      dbPc.sortAscending = newValue;
    }

    public virtual bool RefreshButtonVisible()
    {
      return false; // otherwise, override this in child class
    }

    public virtual bool FileEditorAllowed()
    {
      return true; // otherwise, override this in child class
    }

    public virtual bool FileAddAllowed()
    {
      return true; // otherwise, override this in child class
    }

    public virtual bool FilesCanBeFavourites()
    {
      return true; // otherwise, override this in child class
    }

    public virtual bool FileBrowseAllowed()
    {
      // set this to true, if SUBDIRECTORIES are allowed
      // (example: possible for DIRECTORY-CACHE)
      return false; // otherwise, override this in child class
    }

    public virtual bool SubItemsAllowed()
    {
      return false;
    }

    public virtual bool ProfileLoadingAllowed()
    {
      return false;
    }

    public virtual void Refresh(bool mpGuiMode)
    {
      // descendant classes do that!
    }


    public virtual void OnInfo(GUIListItem item, ref bool isOverviewVisible)
    {
      GUIFileInfo fileInfoDialog = (GUIFileInfo) GUIWindowManager.GetWindow(ProgramUtils.ProgramInfoID);
      if (null != fileInfoDialog)
      {
        if (item.MusicTag == null)
        {
          return;
        }
        FileItem curFile = (FileItem) item.MusicTag;
        fileInfoDialog.App = this;
        fileInfoDialog.File = curFile;
        fileInfoDialog.IsOverviewVisible = isOverviewVisible;
        fileInfoDialog.DoModal(GetID);
        isOverviewVisible = fileInfoDialog.IsOverviewVisible;
        return;
      }
    }

    public int AppID
    {
      get { return appID; }
      set { appID = value; }
    }

    public int FatherID
    {
      get { return fatherID; }
      set { fatherID = value; }
    }

    public string Title
    {
      get { return title; }
      set { title = value; }
    }

    public string ShortTitle
    {
      get { return shortTitle; }
      set { shortTitle = value; }
    }

    public string Filename
    {
      get { return filename; }
      set { filename = value; }
    }

    public string Arguments
    {
      get { return arguments; }
      set { arguments = value; }
    }

    public bool UseQuotes
    {
      get { return useQuotes; }
      set { useQuotes = value; }
    }

    public bool UseShellExecute
    {
      get { return useShellExecute; }
      set { useShellExecute = value; }
    }

    public bool Enabled
    {
      get { return enabled; }
      set { enabled = value; }
    }

    public ProcessWindowStyle WindowStyle
    {
      get { return windowStyle; }
      set { windowStyle = value; }
    }

    public string Startupdir
    {
      get { return startupDir; }
      set { startupDir = value; }
    }

    public string FileDirectory
    {
      get { return fileDirectory; }
      set { fileDirectory = value; }
    }

    public string ImageDirectory
    {
      get { return imageDirectories; }
      set { SetImageDirectory(value); }
    }

    private void SetImageDirectory(string value)
    {
      imageDirectories = value;
      imageDirs = imageDirectories.Split('\r');
      for (int i = 0; i < imageDirs.Length; i++)
      {
        imageDirs[i] = imageDirs[i].Trim();
        // hack the \n away.... 
        imageDirs[i] = imageDirs[i].TrimStart('\n');
        // hack trailing backslashes away
        imageDirs[i] = imageDirs[i].TrimEnd('\\');
      }
    }

    public string Imagefile
    {
      get { return imageFile; }
      set { imageFile = value; }
    }

    public string Source
    {
      get { return sourceFile; }
      set { sourceFile = value; }
    }

    public myProgSourceType SourceType
    {
      get { return sourceType; }
      set { sourceType = value; }
    }

    public string ValidExtensions
    {
      get { return validExtensions; }
      set { validExtensions = value; }
    }

    public bool ImportValidImagesOnly
    {
      get { return importValidImagesOnly; }
      set { importValidImagesOnly = value; }
    }

    public int Position
    {
      get { return appPosition; }
      set { appPosition = value; }
    }

    public int ContentID
    {
      get { return contentID; }
      set { contentID = value; }
    }

    public string SystemDefault
    {
      get { return systemDefault; }
      set { systemDefault = value; }
    }

    public bool WaitForExit
    {
      get { return waitForExit; }
      set { waitForExit = value; }
    }


    public bool GUIRefreshPossible
    {
      get { return RefreshButtonVisible(); }
    }

    public bool EnableGUIRefresh
    {
      get { return enableGUIRefresh; }
      set { enableGUIRefresh = value; }
    }

    public int Pincode
    {
      get { return pincode; }
      set { pincode = value; }
    }

    public string LaunchErrorMsg
    {
      get { return launchErrorMsg; }
      set { launchErrorMsg = value; }
    }

    public string PreLaunch
    {
      get { return preLaunch;}
      set { preLaunch = value;}
    }

    public string PostLaunch
    {
      get { return postLaunch;}
      set { postLaunch = value;}
    }


    public Filelist Files
    {
      // load on demand....
      get
      {
        if (!filesAreLoaded)
        {
          LoadFiles();
        }
        return fileList;
      }
    }


    public FilelinkList Filelinks
    {
      // load on demand....
      get
      {
        if (!linksAreLoaded)
        {
          LoadFileLinks();
        }
        return fileLinks;
      }
    }


    private int GetNewAppID()
    {
      // get an unused SQL application KEY-number
      if (sqlDB != null)
      {
        // won't work in multiuser environment :)
        SQLiteResultSet results;
        int res = 0;
        results = sqlDB.Execute("SELECT MAX(APPID) FROM application");
        ArrayList arr = (ArrayList) results.Rows[0];
        if (arr[0] != null)
        {
          if ((string)arr[0] != "")
          {
            res = Int32.Parse((string) arr[0]);
          }
        }
        return res + 1;
      }
      else return -1;
    }

    private void Insert()
    {
      if (sqlDB != null)
      {
        try
        {
          if (ContentID <= 0)
          {
            ContentID = 100;
          }
          AppID = GetNewAppID(); // important to avoid subsequent inserts!
          string sql = String.Format("insert into application (appid, fatherID, title, shorttitle, filename, arguments, windowstyle, startupdir, useshellexecute, usequotes, source_type, source, imagefile, filedirectory, imagedirectory, validextensions, importvalidimagesonly, position, enabled, enableGUIRefresh, GUIRefreshPossible, pincode, contentID, systemDefault, WaitForExit, preLaunch, postLaunch) values('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}', '{17}', '{18}', '{19}', '{20}', '{21}', '{22}', '{23}', '{24}', '{25}', '{26}')",
                                        AppID, FatherID, ProgramUtils.Encode(Title), ProgramUtils.Encode(ShortTitle), ProgramUtils.Encode(Filename), ProgramUtils.Encode(Arguments),
                                        ProgramUtils.WindowStyleToStr(WindowStyle), ProgramUtils.Encode(Startupdir), ProgramUtils.BooleanToStr(UseShellExecute),
                                        ProgramUtils.BooleanToStr(UseQuotes), ProgramUtils.SourceTypeToStr(SourceType), ProgramUtils.Encode(Source), ProgramUtils.Encode(Imagefile),
                                        ProgramUtils.Encode(FileDirectory), ProgramUtils.Encode(ImageDirectory), ProgramUtils.Encode(ValidExtensions), ProgramUtils.BooleanToStr(importValidImagesOnly), Position,
                                        ProgramUtils.BooleanToStr(Enabled), ProgramUtils.BooleanToStr(EnableGUIRefresh), ProgramUtils.BooleanToStr(GUIRefreshPossible), Pincode,
                                        ContentID, ProgramUtils.Encode(SystemDefault), ProgramUtils.BooleanToStr(WaitForExit), ProgramUtils.Encode(PreLaunch), ProgramUtils.Encode(PostLaunch)
            );
          sqlDB.Execute(sql);
        }
        catch (SQLiteException ex)
        {
          Log.Write("programdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        }
      }
    }

    private void Update()
    {
      string sql = "";
      if ((AppID >= 0) && (sqlDB != null))
      {
        if (ContentID <= 0)
        {
          ContentID = 100;
        }
        try
        {
          sql = String.Format("update application set title = '{0}', shorttitle = '{1}', filename = '{2}', arguments = '{3}', windowstyle = '{4}', startupdir = '{5}', useshellexecute = '{6}', usequotes = '{7}', source_type = '{8}', source = '{9}', imagefile = '{10}',filedirectory = '{11}',imagedirectory = '{12}',validextensions = '{13}',importvalidimagesonly = '{14}',position = {15}, enabled = '{16}', fatherID = '{17}', enableGUIRefresh = '{18}', GUIRefreshPossible = '{19}', pincode = '{20}', contentID = '{21}', systemDefault = '{22}', WaitForExit = '{23}', preLaunch = '{24}', postLaunch = '{25}' where appID = {26}",
                                 ProgramUtils.Encode(Title), ProgramUtils.Encode(ShortTitle), ProgramUtils.Encode(Filename), ProgramUtils.Encode(Arguments),
                                 ProgramUtils.WindowStyleToStr(WindowStyle), ProgramUtils.Encode(Startupdir), ProgramUtils.BooleanToStr(UseShellExecute),
                                 ProgramUtils.BooleanToStr(UseQuotes), ProgramUtils.SourceTypeToStr(SourceType), ProgramUtils.Encode(Source), ProgramUtils.Encode(Imagefile),
                                 ProgramUtils.Encode(FileDirectory), ProgramUtils.Encode(ImageDirectory), ProgramUtils.Encode(ValidExtensions), ProgramUtils.BooleanToStr(importValidImagesOnly), Position,
                                 ProgramUtils.BooleanToStr(Enabled), FatherID, ProgramUtils.BooleanToStr(EnableGUIRefresh), ProgramUtils.BooleanToStr(GUIRefreshPossible),
                                 Pincode, ContentID, ProgramUtils.Encode(SystemDefault), ProgramUtils.BooleanToStr(WaitForExit), ProgramUtils.Encode(PreLaunch), ProgramUtils.Encode(PostLaunch),
                                 AppID);
          sqlDB.Execute(sql);
        }
        catch (SQLiteException ex)
        {
          Log.Write("programdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
          Log.Write("sql \n{0}", sql);
        }
      }
    }

    public void Delete()
    {
      if ((AppID >= 0) && (sqlDB != null))
      {
        try
        {
          DeleteFiles();
          DeleteFileLinks();
          sqlDB.Execute(String.Format("delete from application where appid = {0}", AppID));
        }
        catch (SQLiteException ex)
        {
          Log.Write("programdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        }
      }
    }

    public bool FirstImageDirectoryValid()
    {
      if (this.imageDirs.Length == 0)
      {
        return false;
      }
      else
      {
        string firstDirectory = imageDirs[0];
        if (firstDirectory == "")
        {
          return false;
        }
        else
        {
          return System.IO.Directory.Exists(firstDirectory);
        }
      }

    }


    protected void DeleteFiles()
    {
      if ((AppID >= 0) && (sqlDB != null))
      {
        try
        {
          sqlDB.Execute(String.Format("delete from file where appid = {0}", AppID));
        }
        catch (SQLiteException ex)
        {
          Log.Write("programdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        }
      }
    }

    protected void DeleteFileLinks()
    {
      if ((AppID >= 0) && (sqlDB != null))
      {
        try
        {
          sqlDB.Execute(String.Format("delete from filteritem where appid = {0} or grouperappid = {0}", AppID));
        }
        catch (SQLiteException ex)
        {
          Log.Write("programdatabase exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
        }
      }
    }


    public virtual void LoadFiles()
    {
      if (sqlDB != null)
      {
        // load Files and fill Files-arraylist here!
        if (fileList == null)
        {
          fileList = new Filelist(sqlDB);
        }
        else
        {
          fileList.Clear();
        }
        lastFilepath = "";
        fileList.Load(AppID, "");
        filesAreLoaded = true;
      }
    }

    protected virtual void LoadFileLinks()
    {
      if (sqlDB != null)
      {
        if (fileLinks == null)
        {
          fileLinks = new FilelinkList(sqlDB);
        }
        else
        {
          fileLinks.Clear();
        }
        lastFilepath = "";
        fileLinks.Load(AppID, "");
        linksAreLoaded = true;
      }
    }

    protected virtual void FixFileLinks()
    {
      // after a import the appitem has completely new
      // fileitems (new ids) and LINKS stored in filteritems
      // are out of sync... fix this here!

      // query with data to fix
      string sqlSelectDataToFix = String.Format("select fi.appid, fi.fileid as oldfileid, f.fileid as newfileid, fi.filename as filename from filteritem fi, file f where fi.appID = f.appid and fi.filename = f.filename and fi.appID = {0}", AppID);

      // update command to fix one single link
      string sqlFixOneLink = "update filteritem set fileID = {0}, tag = 0 where appID = {1} and filename = '{2}'";

      SQLiteResultSet rows2fix;


      try
      {
        // 1) initialize TAG
        sqlDB.Execute(String.Format("update filteritem set tag = 1234 where appid = {0}", AppID));

        // 2) fix all fileids of the newly imported files
        rows2fix = sqlDB.Execute(sqlSelectDataToFix);
        int newFileID;
        string filenameToFix;
        if (rows2fix.Rows.Count == 0) return;
        for (int row = 0; row < rows2fix.Rows.Count; row++)
        {
          newFileID = ProgramUtils.GetIntDef(rows2fix, row, "newfileid", -1);
          filenameToFix = ProgramUtils.Get(rows2fix, row, "filename");
          sqlDB.Execute(String.Format(sqlFixOneLink, newFileID, AppID, ProgramUtils.Encode(filenameToFix)));
        }

        // 3) delete untouched links ( they were not imported anymore )
        sqlDB.Execute(String.Format("delete from filteritem where appid = {0} and tag = 1234", AppID));

      }
      catch (SQLiteException ex)
      {
        Log.Write("programdatabase exception (AppItem.FixFileLinks) err:{0} stack:{1}", ex.Message, ex.StackTrace);
      }

    }


    public void Write()
    {
      if (appID == -1)
      {
        Insert();
      }
      else
      {
        Update();
      }
    }

    public virtual string CurrentFilePath()
    {
      return this.FileDirectory;
    }


    public void Assign(AppItem sourceApp)
    {
      this.Enabled = sourceApp.Enabled;
      this.AppID = sourceApp.AppID;
      this.FatherID = sourceApp.FatherID;
      this.Title = sourceApp.Title;
      this.ShortTitle = sourceApp.ShortTitle;
      this.Filename = sourceApp.Filename;
      this.Arguments = sourceApp.Arguments;
      this.WindowStyle = sourceApp.WindowStyle;
      this.Startupdir = sourceApp.Startupdir;
      this.UseShellExecute = sourceApp.UseShellExecute;
      this.UseQuotes = sourceApp.UseQuotes;
      this.SourceType = sourceApp.SourceType;
      this.Source = sourceApp.Source;
      this.Imagefile = sourceApp.Imagefile;
      this.FileDirectory = sourceApp.FileDirectory;
      this.ImageDirectory = sourceApp.ImageDirectory;
      this.ValidExtensions = sourceApp.ValidExtensions;
      this.ImportValidImagesOnly = sourceApp.ImportValidImagesOnly;
      this.Position = sourceApp.Position;
      this.EnableGUIRefresh = sourceApp.EnableGUIRefresh;
      this.Pincode = sourceApp.Pincode;
      this.WaitForExit = sourceApp.WaitForExit;
      this.PreLaunch = sourceApp.PreLaunch;
      this.PostLaunch = sourceApp.PostLaunch;
      this.SystemDefault = sourceApp.SystemDefault;
      this.ContentID = sourceApp.ContentID;
    }

    public bool CheckPincode()
    {
      bool res = true;
      if (this.Pincode > 0)
      {
        res = false;
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GET_PASSWORD, 0, 0, 0, 0, 0, 0);
        GUIWindowManager.SendMessage(msg);
        int enteredPincode = -1;
        try
        {
          enteredPincode = Int32.Parse(msg.Label);
        }
        catch (Exception)
        {
          res = false;
        }
        res = (enteredPincode == this.Pincode);
      }
      return res;
    }

    // imagedirectory stuff
    // get next imagedirectory that holds at least one image for a fileitem
    // * m_pFile:       the file we're looking images for
    private void GetNextThumbFolderIndex(FileItem fileItem)
    {
      if (fileItem == null) return;
      bool foundThumb = false;
      while (!foundThumb)
      {
        thumbFolderIndex++;
        if (thumbFolderIndex >= imageDirs.Length)
        {
          thumbFolderIndex = -1;
          foundThumb = true;
        }
        else
        {
          string candFolder = imageDirs[thumbFolderIndex];
          string candThumb = candFolder + "\\" + fileItem.ExtractImageFileNoPath();
          if (candThumb.ToLower() != fileItem.Imagefile.ToLower())
          {
            foundThumb = (System.IO.File.Exists(candThumb));
          }
          else
          {
            // skip the initial directory, in case it's reentered as a search directory!
            foundThumb = false;
          }
        }
      }
    }

    public virtual string GetCurThumb(GUIListItem item)
    {
      if (item.MusicTag == null)
      {
        return "";
      }
      if (item.MusicTag is FileItem)
      {
        FileItem curFile = item.MusicTag as FileItem;
        return GetCurThumb(curFile);
      }
      else if (item.MusicTag is AppItem)
      {
        AppItem curApp = item.MusicTag as AppItem;
        return curApp.Imagefile;
      }
      else
      {
        return "";
      }
    }


    public string GetCurThumb(FileItem fileItem)
    {
      string curThumb = "";
      if (thumbFolderIndex == -1)
      {
        curThumb = fileItem.Imagefile;
      }
      else
      {
        string curFolder = imageDirs[thumbFolderIndex];
        curThumb = curFolder + "\\" + fileItem.ExtractImageFileNoPath();
      }
      if (thumbIndex > 0)
      {
        // try to find another thumb....
        // use the myGames convention:
        // every thumb has the postfix "_1", "_2", etc with the same file extension
        string curExtension = fileItem.ExtractImageExtension();
        if (curThumb != "")
        {
          string cand = curThumb.Replace(curExtension, "_" + thumbIndex.ToString() + curExtension);
          if (System.IO.File.Exists(cand))
          {
            // found another thumb => override the filename!
            curThumb = cand;
          }
          else
          {
            thumbIndex = 0; // restart at the first thumb!
            GetNextThumbFolderIndex(fileItem);
          }
        }
      }
      return curThumb;
    }

    public void ResetThumbs()
    {
      thumbIndex = 0;
      thumbFolderIndex = -1;
    }

    public void NextThumb()
    {
      thumbIndex++;
    }


    public void LoadFromXmlProfile(XmlNode node)
    {
      XmlNode titleNode = node.SelectSingleNode("title");
      if (titleNode != null)
      {
        this.Title = titleNode.InnerText;
      }

      XmlNode launchingAppNode = node.SelectSingleNode("launchingApplication");
      if (launchingAppNode != null)
      {
        this.Filename = launchingAppNode.InnerText;
      }

      XmlNode useShellExecuteNode = node.SelectSingleNode("useShellExecute");
      if (useShellExecuteNode != null)
      {
        this.UseShellExecute = ProgramUtils.StrToBoolean(useShellExecuteNode.InnerText);
      }

      XmlNode argumentsNode = node.SelectSingleNode("arguments");
      if (argumentsNode != null)
      {
        this.Arguments = argumentsNode.InnerText;
      }

      XmlNode windowStyleNode = node.SelectSingleNode("windowStyle");
      if (windowStyleNode != null)
      {
        this.WindowStyle = ProgramUtils.StringToWindowStyle(windowStyleNode.InnerText);
      }

      XmlNode startupDirNode = node.SelectSingleNode("startupDir");
      if (startupDirNode != null)
      {
        this.Startupdir = startupDirNode.InnerText;
      }

      XmlNode useQuotesNode = node.SelectSingleNode("useQuotes");
      if (useQuotesNode != null)
      {
        this.UseQuotes = ProgramUtils.StrToBoolean(useQuotesNode.InnerText);
      }

      XmlNode fileExtensioneNode = node.SelectSingleNode("fileextensions");
      if (fileExtensioneNode != null)
      {
        this.ValidExtensions = fileExtensioneNode.InnerText;
      }
    }

  }

}