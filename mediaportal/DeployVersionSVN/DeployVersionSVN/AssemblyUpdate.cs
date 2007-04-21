using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DeployVersionSVN
{
  public class AssemblyUpdate
  {
    string _version;

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
        TextReader read = new StreamReader(file.FullName);
        string filetext = read.ReadToEnd();
        read.Close();

        string newtext = Regex.Replace(filetext, "(?<version>Version\\(\"[0-9]+.[0-9]+.[0-9]+.)(?<build>[0-9]+)", "${version}" + _version);

        if (filetext != newtext)
        {
          Console.WriteLine("Updating: " + file.FullName);
          TextWriter write = new StreamWriter(file.FullName);
          write.Write(newtext);
          write.Close();
        }
      }
    }
  }
}
