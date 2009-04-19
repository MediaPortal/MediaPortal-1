using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace UninstallFilelist
{
  public class FileLister
  {
    DirectoryInfo _directory;
    List<string> _ignoredDirectories;
    List<string> _ignoredFiles;

    StringBuilder strBuilder = new StringBuilder();


    public string FileList
    {
      get { return strBuilder.ToString(); }
    }

    public FileLister(string directory, string exclude)
    {
      _directory = new DirectoryInfo(directory);
      _ignoredDirectories = new List<string>();
      _ignoredFiles = new List<string>();

      foreach (string str in exclude.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
      {
        if (str.StartsWith(";")) continue;

        //add ignored dirs
        if (str.StartsWith(@".\"))
        {
          _ignoredDirectories.Add(str.Substring(2));
          continue;
        }

        //add ignored files
        _ignoredFiles.Add(str);
      }
    }

    public void UpdateAll()
    {
      // parse sub dirs
      SearchDirectory(_directory);

      // parse root dir
      AddFiles(_directory);

      strBuilder.AppendLine(String.Format(
        "RMDir \"{0}\"",
        GetMediaPortalPath(_directory.FullName)
        ));
    }

    private void SearchDirectory(DirectoryInfo directory)
    {
      foreach (DirectoryInfo dir in directory.GetDirectories())
      {
        if (dir.Name != ".svn" && dir.Name != "obj" && dir.Name != "bin")
        {
          string tempString = GetMediaPortalPath(dir.FullName);
          if (_ignoredDirectories.Contains(tempString)) continue;

          SearchDirectory(dir);
          AddFiles(dir);

          strBuilder.AppendLine(String.Format("RMDir \"{0}\"", tempString));
        }
      }
    }

    private void AddFiles(DirectoryInfo directory)
    {
      foreach (FileInfo file in directory.GetFiles())
      {
        string tempString = GetMediaPortalPath(file.FullName);
        if (_ignoredFiles.Contains(tempString)) continue;

        //Delete /REBOOTOK "$MPdir.Base\AppStart.exe"
        //Delete /REBOOTOK "$MPdir.Base\AppStart.exe.config"
        strBuilder.AppendLine(String.Format("Delete /REBOOTOK \"{0}\"", tempString));
      }
    }

    private string GetMediaPortalPath(string path)
    {
      path = path.Replace(_directory.FullName, "$MPdir.Base");
      //Var MPdir.Base
      
      //Var MPdir.Config
      //Var MPdir.Plugins
      path = path.Replace(@"$MPdir.Base\plugins", "$MPdir.Plugins");
      //Var MPdir.Log
      //Var MPdir.CustomInputDevice
      //Var MPdir.CustomInputDefault
      path = path.Replace(@"$MPdir.Base\InputDeviceMappings\defaults", "$MPdir.CustomInputDefault");
      //Var MPdir.Skin
      path = path.Replace(@"$MPdir.Base\skin", "$MPdir.Skin");
      //Var MPdir.Language
      path = path.Replace(@"$MPdir.Base\language", "$MPdir.Language");
      //Var MPdir.Database
      //Var MPdir.Thumbs
      path = path.Replace(@"$MPdir.Base\thumbs", "$MPdir.Thumbs");
      //Var MPdir.Weather
      path = path.Replace(@"$MPdir.Base\weather", "$MPdir.Weather");
      //Var MPdir.Cache
      //Var MPdir.BurnerSupport

      return path;
    }
  }
}