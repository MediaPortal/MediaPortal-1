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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using ICSharpCode.SharpZipLib.Zip;

namespace PostSetup
{
  /// <summary>
  /// MP3PartInstaller.
  /// Extend this control!! see MP3PartXMLTV or MP3PartFFDShow 
  /// </summary>
  public class MP3PartInstaller : UserControl
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private Container components = null;
    private Thread dt = null;
    private byte[] dataDownloaded = null;
    private Panel panDownload;
    private ProgressBar progressBar;
    private Label labProgressBytes;
    private Button btnMoreInfo;
    private CheckBox chkInstallThis;
    private ComboBox cbDownloadUrls;
    public Panel panPackage;
    private Label labInstallFrom;
    private bool cancelCalled = false;
    private MediaPortal.UserInterface.Controls.MPTextBox txtDescription;
    private string mpTargetDir;

    /// <summary>
    /// dont use this one.
    /// </summary>
    public MP3PartInstaller()
    {
      InitializeComponent();
    }
    /// <summary>
    /// No impl.
    /// </summary>
    /// <param name="mpTargetDir"></param>
    public virtual void Init(string mpTargetDir)
    {
      this.mpTargetDir = mpTargetDir;
      this.Visible = true;
      this.Dock = DockStyle.Top;

    }


    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.panDownload = new System.Windows.Forms.Panel();
      this.progressBar = new System.Windows.Forms.ProgressBar();
      this.labProgressBytes = new MediaPortal.UserInterface.Controls.MPLabel();
      this.labInstallFrom = new MediaPortal.UserInterface.Controls.MPLabel();
      this.btnMoreInfo = new MediaPortal.UserInterface.Controls.MPButton();
      this.chkInstallThis = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.cbDownloadUrls = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.panPackage = new System.Windows.Forms.Panel();
      this.txtDescription = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.panDownload.SuspendLayout();
      this.panPackage.SuspendLayout();
      this.SuspendLayout();
      // 
      // panDownload
      // 
      this.panDownload.Controls.Add(this.progressBar);
      this.panDownload.Controls.Add(this.labProgressBytes);
      this.panDownload.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panDownload.Location = new System.Drawing.Point(0, 192);
      this.panDownload.Name = "panDownload";
      this.panDownload.Size = new System.Drawing.Size(512, 40);
      this.panDownload.TabIndex = 5;
      // 
      // progressBar
      // 
      this.progressBar.Location = new System.Drawing.Point(16, 8);
      this.progressBar.Name = "progressBar";
      this.progressBar.Size = new System.Drawing.Size(360, 24);
      this.progressBar.TabIndex = 14;
      this.progressBar.Visible = false;
      // 
      // labProgressBytes
      // 
      this.labProgressBytes.Location = new System.Drawing.Point(384, 8);
      this.labProgressBytes.Name = "labProgressBytes";
      this.labProgressBytes.Size = new System.Drawing.Size(112, 24);
      this.labProgressBytes.TabIndex = 15;
      this.labProgressBytes.Text = "0 Kb.";
      this.labProgressBytes.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.labProgressBytes.Visible = false;
      // 
      // labInstallFrom
      // 
      this.labInstallFrom.Location = new System.Drawing.Point(16, 152);
      this.labInstallFrom.Name = "labInstallFrom";
      this.labInstallFrom.Size = new System.Drawing.Size(72, 24);
      this.labInstallFrom.TabIndex = 17;
      this.labInstallFrom.Text = "Install from:";
      this.labInstallFrom.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // btnMoreInfo
      // 
      this.btnMoreInfo.Location = new System.Drawing.Point(416, 16);
      this.btnMoreInfo.Name = "btnMoreInfo";
      this.btnMoreInfo.Size = new System.Drawing.Size(80, 24);
      this.btnMoreInfo.TabIndex = 16;
      this.btnMoreInfo.Text = "More Info.";
      this.btnMoreInfo.Click += new System.EventHandler(this.btnMoreInfo_Click);
      // 
      // chkInstallThis
      // 
      this.chkInstallThis.Location = new System.Drawing.Point(16, 16);
      this.chkInstallThis.Name = "chkInstallThis";
      this.chkInstallThis.Size = new System.Drawing.Size(384, 24);
      this.chkInstallThis.TabIndex = 15;
      this.chkInstallThis.CheckedChanged += new System.EventHandler(this.chkInstallThis_CheckedChanged);
      // 
      // cbDownloadUrls
      // 
      this.cbDownloadUrls.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbDownloadUrls.Enabled = false;
      this.cbDownloadUrls.ItemHeight = 13;
      this.cbDownloadUrls.Location = new System.Drawing.Point(88, 152);
      this.cbDownloadUrls.Name = "cbDownloadUrls";
      this.cbDownloadUrls.Size = new System.Drawing.Size(408, 21);
      this.cbDownloadUrls.TabIndex = 12;
      // 
      // panPackage
      // 
      this.panPackage.Controls.Add(this.txtDescription);
      this.panPackage.Controls.Add(this.labInstallFrom);
      this.panPackage.Controls.Add(this.btnMoreInfo);
      this.panPackage.Controls.Add(this.chkInstallThis);
      this.panPackage.Controls.Add(this.cbDownloadUrls);
      this.panPackage.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panPackage.Location = new System.Drawing.Point(0, 0);
      this.panPackage.Name = "panPackage";
      this.panPackage.Size = new System.Drawing.Size(512, 192);
      this.panPackage.TabIndex = 6;
      // 
      // txtDescription
      // 
      this.txtDescription.AcceptsReturn = true;
      this.txtDescription.AcceptsTab = true;
      this.txtDescription.BackColor = System.Drawing.SystemColors.Control;
      this.txtDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.txtDescription.Location = new System.Drawing.Point(16, 48);
      this.txtDescription.Multiline = true;
      this.txtDescription.Name = "txtDescription";
      this.txtDescription.ReadOnly = true;
      this.txtDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.txtDescription.Size = new System.Drawing.Size(480, 72);
      this.txtDescription.TabIndex = 18;
      this.txtDescription.Text = "txtDescription";
      // 
      // MP3PartInstaller
      // 
      this.Controls.Add(this.panPackage);
      this.Controls.Add(this.panDownload);
      this.Name = "MP3PartInstaller";
      this.Size = new System.Drawing.Size(512, 232);
      this.panDownload.ResumeLayout(false);
      this.panPackage.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    // Downloads a file, see callbacks: DownloadCompleteCallback,DownloadProgressCallback
    private void DownloadFile(string url)
    {
      try
      {
        this.labProgressBytes.Text = "Requesting..";
        this.progressBar.Minimum = 0;
        this.progressBar.Maximum = 0;
        this.progressBar.Value = 0;


        DownloadThread dl = new DownloadThread();
        dl.DownloadUrl = url;
        dl.CompleteCallback += new DownloadCompleteHandler(DownloadCompleteCallback);
        dl.ProgressCallback += new DownloadProgressHandler(DownloadProgressCallback);
        dl.ErrorCallback += new DownloadErrorHandler(DownloadErrorCallback);

        this.dt = new Thread(new ThreadStart(dl.Download));
        dt.IsBackground = true;
        dt.Name = "MP3Loader";
        dt.Start();

        while (dt.IsAlive)
        {
          System.Windows.Forms.Application.DoEvents();
          if (this.cancelCalled)
          {
            break;
          }
        }

      }
      catch (WebException e)
      {
        this.labProgressBytes.Text = "Error: " + e.ToString();
      }
      catch (Exception e)
      {
        this.labProgressBytes.Text = "Error: " + e.ToString();
      }

    }

    /// <summary>
    /// Abort the installtion..
    /// </summary>
    public void Abort()
    {
      try
      {
        this.cancelCalled = true;
        labProgressBytes.Text = "Cancel!";
        this.dt.Abort();
        this.dt = null;
        System.Windows.Forms.Application.DoEvents();
        Thread.Sleep(200);
        progressBar.Visible = false;
        progressBar.Value = 0;
        labProgressBytes.Visible = false;
        ButtonAction = BUTTONACTION_INSTALL;
      }
      catch (Exception)
      {
        //Console.WriteLine(e);
      }
    }

    /// <summary>
    ///  Callback for Download to use.
    /// </summary>
    /// <param name="bytesSoFar"></param>
    /// <param name="totalBytes"></param>
    private void DownloadProgressCallback(int bytesSoFar, int totalBytes)
    {
      if (totalBytes != -1)
      {
        progressBar.Minimum = 0;
        progressBar.Maximum = totalBytes;
        progressBar.Value = bytesSoFar;
        labProgressBytes.Text = (bytesSoFar / 1024).ToString("#,##0") + " of " + (totalBytes / 1024).ToString("#,##0") + " Kb.";
      }
      else
      {
        progressBar.Visible = false;
        labProgressBytes.Text = (bytesSoFar / 1024).ToString("#,##0") + " Kb downloaded.";
      }
    }

    /// <summary>
    ///  Callback for Download to use.
    /// </summary>
    /// <param name="dataDownloaded"></param>
    private void DownloadCompleteCallback(byte[] dataDownloaded)
    {
      if (!progressBar.Visible)
      {
        progressBar.Visible = true;
        progressBar.Minimum = 0;
        progressBar.Value = progressBar.Maximum = 1;
      }
      labProgressBytes.Text = "Download complete!";
      this.dataDownloaded = dataDownloaded;

    }

    /// <summary>
    ///  Callback for Download to use.
    /// </summary>
    /// <param name="e"></param>
    private void DownloadErrorCallback(Exception e)
    {
      labProgressBytes.Text = "Error Downloading!";
      progressBar.Value = progressBar.Minimum;
    }

    /// <summary>
    /// Unzips this.dataDownloaded byte[].
    /// NOTE: No tmp files are used to unzip.
    /// But the unziped files are written to disk.
    /// </summary>
    /// <param name="indata"></param>
    /// <param name="targetDirectory"></param>
    private void UnZipFile(byte[] indata, string targetDirectory, string excludeDir)
    {
      try
      {
        labProgressBytes.Text = "Unzipping...";

        if (!targetDirectory.EndsWith(@"\"))
          targetDirectory += @"\";

        //creates dir that dont exists.
        try
        {
          Directory.CreateDirectory(targetDirectory);
        }
        catch (Exception) { }
        ZipInputStream zis = new ZipInputStream(new MemoryStream(indata));
        ZipEntry theEntry;

        // Buffer size.
        byte[] data = new byte[2048];
        int nb = data.Length;

        progressBar.Value = 0;
        progressBar.Maximum = indata.Length;

        // for each ZipEntry in Zip Archive.
        while ((theEntry = zis.GetNextEntry()) != null)
        {
          // file to unzip from ziparchive. (path+file)
          string cfile = targetDirectory + theEntry.Name;

          // exlude some dirs.
          if (excludeDir != null && !excludeDir.Equals(""))
            cfile = cfile.Replace(@"\" + excludeDir, "");

          if (theEntry.IsDirectory)
          {
            // if its a directory, create it.
            try
            {
              Directory.CreateDirectory(cfile);
            }
            catch (Exception) { }
          }
          else
          {
            // if its a file, write it.
            FileStream fs = new FileStream(cfile, FileMode.Create);
            while ((nb = zis.Read(data, 0, data.Length)) > 0)
            {
              fs.Write(data, 0, nb);
              progressBar.Value = +nb;
            }
            fs.Close();

          }
        }
        zis.Close();
        progressBar.Value = progressBar.Maximum;
      }
      catch (Exception e)
      {
        throw e;
      }
    }


    private string title;

    /// <summary>
    /// Title of control (beside checkbox), (app to download and install)
    /// </summary>
    public string Title
    {
      get { return title; }
      set
      {
        title = value;
        this.chkInstallThis.Text = title;
      }
    }

    private string description;

    /// <summary>
    /// Description of control (under checkbox and title)
    /// </summary>
    public string Description
    {
      get { return description; }
      set
      {
        description = value;
        txtDescription.Lines = description.Split(new char[] { '\n' });
      }
    }


    private string unzipToPath;

    /// <summary>
    /// If this is set, It will unzip downloaded data to this directory
    /// </summary>
    public string UnZipToPath
    {
      get { return unzipToPath; }
      set { unzipToPath = value; }
    }

    private string unzipToExcludeDir;

    /// <summary>
    /// this is a fix to exlude directory in a zip archive, so tey will not been created. (ex. XMLTv zip file)
    /// </summary>
    public string UnzipToExcludeDir
    {
      get { return unzipToExcludeDir; }
      set { unzipToExcludeDir = value; }
    }

    private string saveAsFilename;

    /// <summary>
    /// If this it set, the downloaded file is written to disk.
    /// </summary>
    public string SaveAsFilename
    {
      get { return saveAsFilename; }
      set { saveAsFilename = value; }
    }


    private string saveToPath;

    /// <summary>
    /// Path where the SaveAs filename will been written.
    /// </summary>
    public string SaveToPath
    {
      get { return saveToPath; }
      set { saveToPath = value; }
    }


    private string execCommand;

    /// <summary>
    /// Exec Command after unzip and/or save.
    /// </summary>
    public string ExecCommand
    {
      get { return execCommand; }
      set { execCommand = value; }
    }


    private string execCmdArguments;

    /// <summary>
    /// Arguments to Command above.
    /// </summary>
    public string ExecCmdArguments
    {
      get { return execCmdArguments; }
      set { execCmdArguments = value; }
    }



    private bool doInstallation;
    public bool DoInstallation
    {
      get { return doInstallation; }
      set
      {
        doInstallation = value;
        this.chkInstallThis.Checked = value;
      }
    }


    public delegate void ButtonActionHandler(int buttonAction);
    public event ButtonActionHandler ButtonAction_Changed;
    public const int BUTTONACTION_NEXT = 0;
    public const int BUTTONACTION_INSTALL = 1;
    public const int BUTTONACTION_CANCEL = 2;
    public const int BUTTONACTION_CLOSE = 3;
    public const int BUTTONACTION_INSTALLED = 4;
    public const int BUTTONACTION_DONTINSTALL = 5;

    private int buttonAction;
    public int ButtonAction
    {
      get { return buttonAction; }
      set
      {
        buttonAction = value;
        //callback to app, to change button action!! (text)
        switch (buttonAction)
        {
          case BUTTONACTION_NEXT:
            break;
          case BUTTONACTION_INSTALL:
            panPackage.Enabled = true;
            panDownload.Enabled = false;
            break;
          case BUTTONACTION_CANCEL:
            panPackage.Enabled = false;
            panDownload.Enabled = true;
            break;
          case BUTTONACTION_CLOSE:
            panDownload.Enabled = false;
            panPackage.Enabled = false;
            break;
          case BUTTONACTION_INSTALLED:
            panDownload.Enabled = false;
            panPackage.Enabled = false;
            break;
          case BUTTONACTION_DONTINSTALL:
            panDownload.Enabled = false;
            panPackage.Enabled = true;
            this.chkInstallThis.Checked = false;
            break;
        }

        ButtonAction_Changed(buttonAction);

      }
    }

    private string moreInfoUrl;

    /// <summary>
    /// Property for the MoreInfo button, url that default browser will go to.
    /// </summary>
    public string MoreInfoUrl
    {
      get { return moreInfoUrl; }
      set { moreInfoUrl = value; }
    }


    private string[] downloadUrls;

    /// <summary>
    /// Urls to download package from. (shown in combobox)
    /// </summary>
    public string[] DownloadUrls
    {
      get { return downloadUrls; }
      set
      {
        downloadUrls = value;
        if (downloadUrls != null)
        {
          cbDownloadUrls.Items.Clear();
          cbDownloadUrls.Items.AddRange(downloadUrls);
          cbDownloadUrls.SelectedIndex = 0;
        }
      }
    }


    /// <summary>
    /// Install ("Run") this control.. 
    /// It will download, unzip, save and exec something.
    /// </summary>
    public void Install()
    {

      ButtonAction = BUTTONACTION_CANCEL;
      if (this.cbDownloadUrls.Text != null && !this.cbDownloadUrls.Text.Equals(""))
      {
        cancelCalled = false;
        panDownload.Enabled = true;

        progressBar.Visible = true;
        labProgressBytes.Visible = true;

        //download file, it will wait inside until its done or error.
        DownloadFile(this.cbDownloadUrls.Text);

        // is cancel pressed?!
        if (!cancelCalled)
        {
          try
          {
            // is it set? we will unzip
            if (UnZipToPath != null && !UnZipToPath.Equals(""))
            {
              labProgressBytes.Text = "Unzipping...";
              UnZipFile(this.dataDownloaded, UnZipToPath, UnzipToExcludeDir);
              labProgressBytes.Text = "Unzipping done!";
            }
          }
          catch (Exception ex)
          {
            Console.Write(ex);
            labProgressBytes.Text = "Unzip error!";
            MessageBox.Show("A Error occur when unzipping package.\nException: " + ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            ButtonAction = BUTTONACTION_DONTINSTALL;
            return;
          }

          try
          {

            // if set, we will write the (unziped data) or (not unziped data) to disk.
            if (SaveAsFilename != null && !SaveAsFilename.Equals("") && SaveToPath != null && !SaveToPath.Equals(""))
            {
              // save only to disk
              labProgressBytes.Text = "Saving...";
              SaveFile(this.dataDownloaded, SaveToPath, SaveAsFilename);
              labProgressBytes.Text = "Saving done!";
            }

          }
          catch (Exception ex)
          {
            Console.Write(ex);
            labProgressBytes.Text = "Save file error!";
            MessageBox.Show("A Error occur when saving file to disk.\nException: " + ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            ButtonAction = BUTTONACTION_DONTINSTALL;
            return;
          }

          try
          {
            //if set, we will execute a command with arguments..
            if (ExecCommand != null && !ExecCommand.Equals(""))
            {
              labProgressBytes.Text = "Running application...";
              Process proc = new Process();
              proc.EnableRaisingEvents = false;
              proc.StartInfo.FileName = ExecCommand;
              proc.StartInfo.Arguments = ExecCmdArguments;
              proc.Start();
              proc.WaitForExit(60000 * 3); // the exec got 3 min to finnish. (else error)

              if (proc.ExitCode == 0)
              {
                labProgressBytes.Text = "Running application done!";
                progressBar.Value = progressBar.Maximum = 1;
                Thread.Sleep(500);
                labProgressBytes.Text = "Done!";
                panDownload.Enabled = false;
                ButtonAction = BUTTONACTION_INSTALLED;
              }
              else
              {
                labProgressBytes.Text = "Exec error! (status!=0)";
                MessageBox.Show("A Error occur when running 3-part-package installation.\nThe installation did not return status 0, could be a problem.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ButtonAction = BUTTONACTION_DONTINSTALL;
              }
            }
          }
          catch (Exception ex)
          {
            Console.Write(ex);
            labProgressBytes.Text = "Exec error!";
            MessageBox.Show("A Error occur when 3-part-package installation.\nException: " + ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            ButtonAction = BUTTONACTION_DONTINSTALL;
            return;
          }

          labProgressBytes.Text = "Installation done!";
          ButtonAction = BUTTONACTION_INSTALLED;

        }
      }

    }

    /// <summary>
    /// Save byte to file on disk.
    /// </summary>
    /// <param name="indata">byte data to save</param>
    /// <param name="targetDirectory">directory where it should be written</param>
    /// <param name="filename">name of file to create</param>
    private void SaveFile(byte[] indata, string targetDirectory, string filename)
    {
      try
      {
        //creates directoris if they dont exists.
        try
        {
          Directory.CreateDirectory(targetDirectory);
        }
        catch (Exception) { }
        //write byte to disk.
        FileStream fs = new FileStream(@targetDirectory + "/" + filename, FileMode.CreateNew);
        for (int i = 0; i < indata.Length; i++)
          fs.WriteByte(indata[i]);
        fs.Close();

      }
      catch (Exception e)
      {
        if (!e.Message.EndsWith("already exists."))
          throw e;
      }
    }

    /// <summary>
    ///  Opens default browser and goes to MoreInfoUrl Property Url.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnMoreInfo_Click(object sender, EventArgs e)
    {
      try
      {
        Process proc = new Process();
        proc.EnableRaisingEvents = false;
        proc.StartInfo.FileName = MoreInfoUrl;
        proc.Start();
      }
      catch (Exception e1)
      {
        Console.Write(e1);
      }
    }

    protected bool RegistryKeyExists(string hklmRegKey)
    {
      try
      {
        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(hklmRegKey))
        {
          if (key == null)
            return false;
          else
            return true;
        }
      }
      catch (Exception)
      {
        return true;
      }
    }

    /// <summary>
    /// Event if someone checks the checkbox.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void chkInstallThis_CheckedChanged(object sender, EventArgs e)
    {
      if (((CheckBox)sender).Checked)
      {
        cbDownloadUrls.Enabled = true;
        CheckedToInstall();
      }
      else
      {
        cbDownloadUrls.Enabled = false;
        UnCheckedToInstall();
      }
    }

    public virtual void CheckedToInstall()
    {
      // do something?!	
    }
    public virtual void UnCheckedToInstall()
    {
      // do something?!
    }

    public virtual bool AlreadyExits()
    {
      // do something?!
      return false;
    }


    protected void CallControlPanelApplet(string applet)
    {
      // check this link to get all the other applets in controlpanel.
      // http://www.helpware.net/FAR/far_faq.htm#cpl
      // shell32.dll,Control_RunDLL CPLfilename,@AppletNo,AppletPage

      //CPL filename Applet No Description 
      // APPWIZ.CPL  0 Add/Remove Programs Properties 
      try
      {

        Process proc = new Process();
        proc.EnableRaisingEvents = false;
        proc.StartInfo.FileName = "Rundll32.exe";
        proc.StartInfo.Arguments = "shell32.dll,Control_RunDLL " + applet;

        proc.Start();
      }
      catch (Exception e1)
      {
        Console.Write(e1);
      }




    }


  }
}
