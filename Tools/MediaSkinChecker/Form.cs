using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WindowsApplication3
{
  public partial class Form1 : Form
  {
    const string FileLog = @"C:\MediaFolderCheck.txt";

    public Form1()
    {
      InitializeComponent();

      File.Delete(FileLog);

#if DEBUG
      sourcePath.Text = @"C:\svnroot\mediaportal\trunk\mediaportal\MediaPortal.Base\skin\Blue3wide\Media";
      destinationPath.Text = @"C:\svnroot\mediaportal\trunk\mediaportal";
#endif

    }

    private void button1_Click(object sender, EventArgs e)
    {
      buttonStart.Enabled = false;
      Console.WriteLine(String.Format("Creating {0} tree", sourcePath.Text));
      List<FileInfo> srcFiles = new List<FileInfo>();
      srcFiles.AddRange(GetFilesRecursive(new DirectoryInfo(sourcePath.Text), "*.png"));
      srcFiles.AddRange(GetFilesRecursive(new DirectoryInfo(sourcePath.Text), "*.bmp"));

      Console.WriteLine(String.Format("Creating {0} tree", destinationPath.Text));
      List<FileInfo> dstFiles = new List<FileInfo>();
      dstFiles.AddRange(GetFilesRecursive(new DirectoryInfo(destinationPath.Text), "*.cs"));
      dstFiles.AddRange(GetFilesRecursive(new DirectoryInfo(destinationPath.Text), "*.xml"));

      foreach (FileInfo srcFile in srcFiles)
      {
        bool found = false;
        Console.WriteLine(String.Format("Looking for file {0}...", srcFile.FullName));
        foreach (FileInfo dstFile in dstFiles)
        {
          StreamReader testTxt = new StreamReader(dstFile.FullName);
          string allRead = testTxt.ReadToEnd(); //Reads the whole text file to the end
          testTxt.Close(); //Closes the text file after it is fully read.
          //Console.WriteLine(String.Format("    Inspecting file {0}...", dstFile.FullName));
          found = Regex.IsMatch(allRead, srcFile.Name.Split('.')[0], RegexOptions.IgnoreCase);
          if (found) break;
        }
        if (!found)
        {
          string msg = String.Format("WARNING: file {0}[{1}] is not used !!!", srcFile.Name.Split('.')[0], srcFile.Name.Split('.')[1]);
          Console.WriteLine(msg);
          StreamWriter SW = File.AppendText(FileLog);
          SW.WriteLine(msg);
          SW.Close();
        }
      }
      MessageBox.Show("Please review {0} for unused Media gfx list.", "Finished", MessageBoxButtons.OK, MessageBoxIcon.Information);
      buttonStart.Enabled = true;
    }

    public static IEnumerable<FileInfo> GetFilesRecursive(DirectoryInfo dirInfo)
    {
      return GetFilesRecursive(dirInfo, "*.*");
    }
    public static IEnumerable<FileInfo> GetFilesRecursive(DirectoryInfo dirInfo, string searchPattern)
    {
      foreach (DirectoryInfo di in dirInfo.GetDirectories())
        foreach (FileInfo fi in GetFilesRecursive(di, searchPattern))
          yield return fi;

      foreach (FileInfo fi in dirInfo.GetFiles(searchPattern))
        yield return fi;
    }

    private void buttonSource_Click(object sender, EventArgs e)
    {
      FolderBrowserDialog dir = new FolderBrowserDialog();
      dir.ShowDialog();
      sourcePath.Text = dir.SelectedPath;
    }

    private void buttonDest_Click(object sender, EventArgs e)
    {
      FolderBrowserDialog dir = new FolderBrowserDialog();
      dir.ShowDialog();
      destinationPath.Text = dir.SelectedPath;
    }

  }
}