#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DeployVersionSVN
{
  public class AssemblyUpdate
  {
    public enum UpdateMode
    {
      Version,
      Copyright
    }

    private readonly string _version;
    private readonly UpdateMode _updateMode;
    private List<string> _excludedFiles;

    public AssemblyUpdate(string version) : this(version, UpdateMode.Version)
    {
    }

    public AssemblyUpdate(string version, UpdateMode mode)
    {
      _version = version;
      _updateMode = mode;
      _excludedFiles = new List<string>();

      LoadExcludedFiles(Path.Combine(
        Application.StartupPath, "ExcludeFromUpdate.txt"));
    }

    public void UpdateAll(string directory)
    {
      DirectoryInfo dir = new DirectoryInfo(directory);

      SearchDirectory(dir);
    }

    private void SearchDirectory(DirectoryInfo directory)
    {
      foreach (DirectoryInfo dir in directory.GetDirectories())
      {
        if (dir.Name != ".svn" && dir.Name != "obj" && dir.Name != "bin")
        {
          SearchDirectory(dir);
          UpdateFile(dir);
        }
      }
    }

    private void UpdateFile(DirectoryInfo directory)
    {
      foreach (FileInfo file in directory.GetFiles("AssemblyInfo.cs"))
      {
        foreach (string excludedFile in _excludedFiles)
          if (file.FullName.Contains(excludedFile))
          {
            Console.WriteLine("   Exluded: " + file.FullName);
            goto exclude;
          }

        StreamReader read = new StreamReader(file.FullName, true);
        Encoding encoding = read.CurrentEncoding;
        string filetext = read.ReadToEnd();
        read.Close();

        string newtext;

        if (_updateMode == UpdateMode.Copyright)
          newtext = Regex.Replace(filetext, "^(.*AssemblyCopyright.*)$",
                                  "[assembly: AssemblyCopyright(\"Copyright © " + _version + "\")]",
                                  RegexOptions.Multiline);
        else
          newtext = Regex.Replace(filetext, "(?<version>Version\\(\"[0-9]+.[0-9]+.[0-9]+.)(?<build>[0-9]+)",
                                  "${version}" + _version);

        if (filetext != newtext)
        {
          Console.WriteLine("Updating: " + file.FullName);
          TextWriter write = new StreamWriter(file.FullName, false, encoding);
          //TextWriter write = new StreamWriter(file.FullName);
          write.Write(newtext);
          write.Close();
        }

        exclude:
        continue;
      }
    }

    private void LoadExcludedFiles(string filename)
    {
      if (!File.Exists(filename)) return;

      Console.WriteLine("Excluding file which matches the following expressions: ");
      foreach (string str in File.ReadAllLines(filename))
      {
        if (String.IsNullOrEmpty(str)) continue;

        Console.WriteLine("   " + str);
        _excludedFiles.Add(str);
      }
    }
  }
}