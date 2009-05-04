#region Copyright (C) 2005-2009 Team MediaPortal

/*
 *  Copyright (C) 2005-2009 Team MediaPortal
 *  http://www.team-mediaportal.com
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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace DeployVersionSVN
{
  public class AssemblyUpdate
  {
    private readonly string _version;

    public AssemblyUpdate(string version)
    {
      _version = version;
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
        StreamReader read = new StreamReader(file.FullName, true);
        Encoding encoding = read.CurrentEncoding;
        string filetext = read.ReadToEnd();
        read.Close();

        string newtext = Regex.Replace(filetext, "(?<version>Version\\(\"[0-9]+.[0-9]+.[0-9]+.)(?<build>[0-9]+)",
                                       "${version}" + _version);

        if (filetext != newtext)
        {
          Console.WriteLine("Updating: " + file.FullName);
          TextWriter write = new StreamWriter(file.FullName, false, encoding);
          //TextWriter write = new StreamWriter(file.FullName);
          write.Write(newtext);
          write.Close();
        }
      }
    }
  }
}