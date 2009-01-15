using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

using MediaPortal.Util;

namespace ArtistThumbGenerator
{
  public partial class formMain : Form
  {
    string[] artistPaths;
    string[] thumbFiles;

    enum ScanAction
    {
      Lookup,
      Save,
    }

    public formMain()
    {
      InitializeComponent();
      textBoxThumbPath.Text = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Team MediaPortal\MediaPortal\Thumbs\Music\Folder";
      textBoxArtistRootFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
    }

    #region Utils

    public string GetLocalFolderThumb(string strFile)
    {
      if (strFile == null) return string.Empty;
      if (strFile.Length == 0) return string.Empty;

      string strPath, strFileName;
      Split(strFile, out strPath, out strFileName);
      string strFolderJpg = String.Format(@"{0}\{1}{2}", textBoxThumbPath.Text, EncryptLine(strPath), ".jpg");

      return strFolderJpg;
    }

    public string GetLocalFolderThumbForDir(string strDirPath)
    {
      if (string.IsNullOrEmpty(strDirPath))
        return string.Empty;

      string strFolderJpg = String.Format(@"{0}\{1}{2}", textBoxThumbPath.Text, EncryptLine(strDirPath), ".jpg");

      return strFolderJpg;
    }

    public string EncryptLine(string strLine)
    {
      if (string.IsNullOrEmpty(strLine))
        return string.Empty;

      if (String.Compare("unknown", strLine, true) == 0) return string.Empty;
      CRCTool crc = new CRCTool();
      crc.Init(CRCTool.CRCCode.CRC32);
      ulong dwcrc = crc.calc(strLine);
      string strRet = String.Format("{0}", dwcrc);
      return strRet;
    }

    public void Split(string strFileNameAndPath, out string strPath, out string strFileName)
    {
      strFileName = "";
      strPath = "";
      if (string.IsNullOrEmpty(strFileNameAndPath))
        return;

      try
      {
        strFileNameAndPath = strFileNameAndPath.Trim();
        if (strFileNameAndPath.Length == 0) return;
        int i = strFileNameAndPath.Length - 1;
        while (i >= 0)
        {
          char ch = strFileNameAndPath[i];
          if (ch == ':' || ch == '/' || ch == '\\') break;
          else i--;
        }
        if (i >= 0)
        {
          strPath = strFileNameAndPath.Substring(0, i).Trim();
          strFileName = strFileNameAndPath.Substring(i + 1).Trim();
        }
        else
        {
          strPath = "";
          strFileName = strFileNameAndPath;
        }
      }
      catch (Exception)
      {
        strPath = "";
        strFileName = strFileNameAndPath;
      }
    }

    #endregion

    private void buttonScan_Click(object sender, EventArgs e)
    {
      buttonScan.Enabled = false;
      MatchAllArtists(ScanAction.Lookup);
      buttonScan.Enabled = true;
    }

    private void buttonSave_Click(object sender, EventArgs e)
    {
      buttonSave.Enabled = false;
      MatchAllArtists(ScanAction.Save);
      buttonSave.Enabled = true;
    }

    private void MatchAllArtists(ScanAction aScanAction)
    {
      try
      {
        listBoxFolders.Items.Clear();
        artistPaths = Directory.GetDirectories(textBoxArtistRootFolder.Text, "*", System.IO.SearchOption.TopDirectoryOnly);
        thumbFiles = Directory.GetFiles(textBoxArtistThumbFolder.Text, "*.jpg", SearchOption.TopDirectoryOnly);
        Dictionary<string, string> allArtistThumbs = new Dictionary<string, string>(thumbFiles.Length);
        for (int i = 0; i < thumbFiles.Length; i++)
        {
          int dirPos = thumbFiles[i].LastIndexOf('\\');
          if (dirPos > 1)
          {
            string thumbPath = thumbFiles[i].Substring(dirPos + 1).ToLowerInvariant();
            allArtistThumbs[thumbPath] = thumbFiles[i];
          }
        }

        List<string> allThumbs = new List<string>(thumbFiles);
        foreach (string currentartistpath in artistPaths)
        {
          //string thumbName = GetLocalFolderThumbForDir(ArtistPath);
          //string displayString = string.Format("{0} : ({1} -> {2})", Path.GetFileName(thumbName), ArtistPath, thumbName);

          int dirPos = currentartistpath.LastIndexOf('\\');
          if (dirPos > 1)
          {
            string artistName = currentartistpath.Substring(dirPos + 1);
            string displayString = artistName;
            string highResPath = string.Format("{0}{1}", artistName, "L.jpg").ToLowerInvariant();
            string target = string.Format("{0}\\{1}.jpg", currentartistpath, artistName);
            if (allArtistThumbs.ContainsKey(highResPath))
            {
              displayString += string.Format(" <-- {0} --> {1}\\{2}.jpg", allArtistThumbs[highResPath], currentartistpath, artistName);

              if (aScanAction == ScanAction.Save)
              {
                try
                {
                  string source = allArtistThumbs[highResPath];
                  
                  if (!File.Exists(target))
                    File.Move(source, target);
                }
                catch (Exception exm)
                {
                  MessageBox.Show(string.Format("Could not move thumbnail - {0}", exm.Message));
                }
              }
            }
            else
            {
              displayString += " <-- (missing) --X";
            }
            if (aScanAction == ScanAction.Lookup)
            {
              if (!File.Exists(target))
                listBoxFolders.Items.Add(displayString);
            }

          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(string.Format("Doh! ({0})", ex.Message));
      }
    }

    private void buttonLookupDir_Click(object sender, EventArgs e)
    {
      folderBrowserDialog.SelectedPath = textBoxThumbPath.Text;
      if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
        textBoxThumbPath.Text = folderBrowserDialog.SelectedPath;
    }

    private void buttonLookupArtistRoot_Click(object sender, EventArgs e)
    {
      folderBrowserDialogArtist.SelectedPath = textBoxArtistRootFolder.Text;
      if (folderBrowserDialogArtist.ShowDialog() == DialogResult.OK)
        textBoxArtistRootFolder.Text = folderBrowserDialogArtist.SelectedPath;
    }

    private void buttonLookupArtistDir_Click(object sender, EventArgs e)
    {
      folderBrowserDialog.SelectedPath = textBoxArtistThumbFolder.Text;
      if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
        textBoxArtistThumbFolder.Text = folderBrowserDialog.SelectedPath;
    }

    private void buttonClose_Click(object sender, EventArgs e)
    {
      Application.Exit();
    }

    private void listBoxFolders_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      try
      {
        if (listBoxFolders.SelectedItem != null)
        {
          string selectedName = listBoxFolders.SelectedItem.ToString();
          string fileName = selectedName.Remove(selectedName.IndexOf(':') - 1);
          Clipboard.SetDataObject(fileName, true, 3, 100);
        }
      }
      catch (Exception) { }
    }

  }
}