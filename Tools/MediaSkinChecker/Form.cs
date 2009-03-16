using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace MediaSkinChecker
{
  public partial class Form1 : Form
  {
    const string FileLog = @"C:\MediaFolderCheck.txt";
    const string FileBatch = @"C:\MediaFolderCheck.bat";
    const string SVNcmd = @"C:\Program Files\CollabNet Subversion Client\svn";

    public Form1()
    {
      InitializeComponent();
      File.Delete(FileLog);
      File.Delete(FileBatch);

#if DEBUG
      sourcePath.Text = @"C:\svnroot\mediaportal\trunk\mediaportal\MediaPortal.Base\skin\Blue3wide\Media";
      destinationPath.Text = @"C:\svnroot\mediaportal\trunk\mediaportal";
#endif

    }

    private void buttonUnused_Click(object sender, EventArgs e)
    {
      buttonUnused.Enabled = false;
      buttonMissing.Enabled = false;
      ToolStripText(String.Format("Creating {0} tree", sourcePath.Text));
      List<FileInfo> srcFiles = new List<FileInfo>();
      srcFiles.AddRange(GetFilesRecursive(new DirectoryInfo(sourcePath.Text), "*.png"));
      srcFiles.AddRange(GetFilesRecursive(new DirectoryInfo(sourcePath.Text), "*.bmp"));

      ToolStripText(String.Format("Creating {0} tree", destinationPath.Text));
      List<FileInfo> dstFiles = new List<FileInfo>();
      dstFiles.AddRange(GetFilesRecursive(new DirectoryInfo(destinationPath.Text), "*.cs"));
      dstFiles.AddRange(GetFilesRecursive(new DirectoryInfo(destinationPath.Text), "*.xml"));

      foreach (FileInfo srcFile in srcFiles)
      {
        bool found = false;
        ToolStripText(String.Format("Looking for referenced file {0}...", srcFile.Name));
        foreach (FileInfo dstFile in dstFiles)
        {
          StreamReader testTxt = new StreamReader(dstFile.FullName);
          string allRead = testTxt.ReadToEnd(); //Reads the whole text file to the end
          testTxt.Close();

          string CleanFileName = Path.GetFileNameWithoutExtension(srcFile.Name);
          //Exception for icon_numberplace_overlay_?
          CleanFileName = Regex.Replace(CleanFileName, "icon_numberplace_overlay_[1-9]", "icon_numberplace_overlay_", RegexOptions.IgnoreCase);
          //Exception for common.waiting.?
          CleanFileName = Regex.Replace(CleanFileName, "common.waiting.[1-8]", "common.waiting.", RegexOptions.IgnoreCase);

          found = allRead.ToLower().Contains(CleanFileName.ToLower());
          if (found) break;
        }
        if (!found)
        {
          string relativePath = "." + srcFile.FullName.Replace(sourcePath.Text, string.Empty);
          string msg1 = String.Format("WARNING: file {0} is not used !!!", relativePath);
          string msg2 = String.Format("\"{0}\" remove \"{1}\"", SVNcmd, relativePath);
          StreamWriter SW1 = File.AppendText(FileLog);
          SW1.WriteLine(msg1);
          SW1.Close();
          StreamWriter SW2 = File.AppendText(FileBatch);
          SW2.WriteLine(msg2);
          SW2.Close();
        }
      }
      MessageBox.Show(String.Format("Please review {0} for unused Media gfx list.", FileLog), "Finished", MessageBoxButtons.OK, MessageBoxIcon.Information);
      buttonUnused.Enabled = true;
      buttonMissing.Enabled = true;
    }

    private void buttonMissing_Click(object sender, EventArgs e)
    {
      buttonUnused.Enabled = false;
      buttonMissing.Enabled = false;
      ToolStripText(String.Format("Creating {0} tree", sourcePath.Text));
      List<FileInfo> srcFiles = new List<FileInfo>();
      srcFiles.AddRange(GetFilesRecursive(new DirectoryInfo(destinationPath.Text), "*.xml"));

      ToolStripText(String.Format("Creating {0} tree", destinationPath.Text));
      List<FileInfo> dstFiles = new List<FileInfo>();
      dstFiles.AddRange(GetFilesRecursive(new DirectoryInfo(sourcePath.Text), "*.bmp"));
      dstFiles.AddRange(GetFilesRecursive(new DirectoryInfo(sourcePath.Text), "*.png"));

      List<String> gfxfile = new List<String>();

      foreach (FileInfo srcFile in srcFiles)
      {

        if (!srcFile.FullName.ToLower().Contains("skin"))
        {
          continue;
        }
        ToolStripText(String.Format("Looking for references in file {0}...", srcFile.Name));

        XmlTextReader xmlReader = new XmlTextReader(srcFile.FullName);

        while (xmlReader.Read())
        {
          if (xmlReader.NodeType != XmlNodeType.Text)
          {
            continue;
          }
          string NodeValue = xmlReader.Value.ToLower(); //.Replace("animations\\", string.Empty);
          if (NodeValue.Contains("png") || NodeValue.Contains("bmp"))
          {
            if (NodeValue.Contains(";"))
            {
              gfxfile.AddRange(NodeValue.Split(';'));
            }
            else
            {
              if (NodeValue.Contains(":"))
              {
                NodeValue = NodeValue.Split(':')[1];
              }
              gfxfile.Add(NodeValue);
            }
          }
        }
      }
      gfxfile.Sort();
      List<string> gfxfile2 = removeDuplicates(gfxfile);

      string gfxmissing = string.Empty;
      foreach (string s in gfxfile2)
      {
        bool found = false;
        foreach (FileInfo dstFile in dstFiles)
        {
          gfxmissing = s.Trim();
          if (dstFile.FullName.ToLower().Contains(gfxmissing))
          {
            found = true;
            break;
          }
        }
        if (!found)
        {
          string msg1 = String.Format("WARNING: file {0} is missing !!!", gfxmissing);
          StreamWriter SW1 = File.AppendText(FileLog);
          SW1.WriteLine(msg1);
          SW1.Close();
        }
      }

      MessageBox.Show(String.Format("Please review {0} for missing Media gfx list.", FileLog), "Finished", MessageBoxButtons.OK, MessageBoxIcon.Information);
      buttonUnused.Enabled = true;
      buttonMissing.Enabled = true;
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

    public void ToolStripText(string status)
    {
      toolStripStatusLabel.ForeColor = System.Drawing.Color.Black;
      toolStripStatusLabel.Text = status;
      statusStrip.Refresh();
    }

    static List<string> removeDuplicates(IEnumerable<string> inputList)
    {
      Dictionary<string, int> uniqueStore = new Dictionary<string, int>();
      List<string> finalList = new List<string>();

      foreach (string currValue in inputList)
      {
        if (!uniqueStore.ContainsKey(currValue))
        {
          uniqueStore.Add(currValue, 0);
          finalList.Add(currValue);
        }
      }
      return finalList;
    }
  }
}