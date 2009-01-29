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
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Windows.Forms;

namespace MediaPortal.MPInstaller
{
  public partial class DownloadForm : MPInstallerForm
  {
    private string source = string.Empty;
    private string dest = string.Empty;
    private WebClient client = new WebClient();
    public int direction = 0;
    public string user = string.Empty;
    public string password = string.Empty;

    public DownloadForm(string s, string d)
    {
      InitializeComponent();
      label2.Text = "Resolving host ....";
      source = s;
      dest = d;
      client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressCallback);
      client.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadEnd);
      client.UploadProgressChanged += new UploadProgressChangedEventHandler(UploadProgressCallback);
      client.UploadFileCompleted += new UploadFileCompletedEventHandler(DownloadEnd);
      progressBar1.Minimum = 0;
      progressBar1.Maximum = 100;
      progressBar1.Value = 0;
    }

    private void download_form_Shown(object sender, EventArgs e)
    {
      this.Refresh();
      this.Update();
      if (!String.IsNullOrEmpty(source) && !String.IsNullOrEmpty(dest))
      {
        if (direction == 0)
        {
          string result = string.Empty;
          ;
          try
          {
            //login_form dlg = new login_form(user,password);
            //dlg.ShowDialog();
            //user = dlg.username;
            //password = dlg.password;
            //result = client.DownloadString(new System.Uri(MPinstalerStruct.DEFAULT_UPDATE_SITE + "/mp.php?option=login&user="+user+"&passwd="+password));
//                  if (result.Contains("Login was made !"))
            if (true)
            {
              if (Path.GetExtension(dest) == ".xml")
              {
                //MessageBox.Show(MPinstalerStruct.DEFAULT_UPDATE_SITE+"/mp_download.php?file=" + Path.GetFileName(source));
                client.DownloadFileAsync(
                  new Uri(MPinstallerStruct.DEFAULT_UPDATE_SITE + "/mp.php?option=getxml&user=" + user + "&passwd=" +
                          password), dest);
              }
              else
              {
                //source = source.Replace("&","|");
                //source = source.Replace("id=", "id1=");
                //source = source.Replace("http://mpi.team-mediaportal.com", "*");
                client.CachePolicy = new RequestCachePolicy();
                client.UseDefaultCredentials = true;
                client.DownloadFileAsync(
                  new Uri(MPinstallerStruct.DEFAULT_UPDATE_SITE + "/mp.php?option=down&user=" + user + "&passwd=" +
                          password + "&filename=" + Path.GetFileName(source)), dest);
              }
            }
            // TODO
            //else
            //{
            //  MessageBox.Show("Login error !");
            //  this.Close();
            //}
          }
          catch (WebException ex)
          {
            MessageBox.Show("Error ocured : " + ex.Message);
            this.Close();
          }
        }
        else
        {
          client.UploadFileAsync(new Uri(source), dest);
        }
        //client.DownloadFile(new System.Uri(source), dest);
      }
    }

    private void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
    {
      progressBar1.Value = e.ProgressPercentage;
      label2.Text = string.Format("{0} kb/{1} kb", e.BytesReceived/1024, e.TotalBytesToReceive/1024);
    }

    private void UploadProgressCallback(object sender, UploadProgressChangedEventArgs e)
    {
      progressBar1.Value = e.ProgressPercentage;
    }

    private void DownloadEnd(object sender, AsyncCompletedEventArgs e)
    {
      if (e.Error != null)
      {
        if (File.Exists(dest))
        {
          if (!client.IsBusy)
          {
            try
            {
              File.Delete(dest);
            }
            catch (Exception)
            {
            }
          }
        }
        MessageBox.Show(e.Error.Message + "\n" + e.Error.InnerException);
      }
      button1.Enabled = false;
      this.Close();
    }

    public static bool FtpDownload(string source, string dest, string user, string pwd)
    {
      FtpWebRequest reqFTP;
      try
      {
        //filePath = <<The full path where the file is to be created.>>, 
        //fileName = <<Name of the file to be created(Need not be the name of the file on FTP server).>>
        FileStream outputStream = new FileStream(dest, FileMode.Create);

        reqFTP = (FtpWebRequest) FtpWebRequest.Create(new Uri(source));
        reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
        reqFTP.UseBinary = true;
        reqFTP.Credentials = new NetworkCredential(user, pwd);
        FtpWebResponse response = (FtpWebResponse) reqFTP.GetResponse();
        Stream ftpStream = response.GetResponseStream();
        long cl = response.ContentLength;
        int bufferSize = 2048;
        int readCount;
        byte[] buffer = new byte[bufferSize];

        readCount = ftpStream.Read(buffer, 0, bufferSize);
        while (readCount > 0)
        {
          outputStream.Write(buffer, 0, readCount);
          readCount = ftpStream.Read(buffer, 0, bufferSize);
        }

        ftpStream.Close();
        outputStream.Close();
        response.Close();
        if (response.StatusCode == FtpStatusCode.ClosingData)
        {
          return true;
        }
        else
        {
          return false;
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
        return false;
      }
    }


    public static bool FtpUpload(string source, string dest, string user, string pwd)
    {
      if (!dest.EndsWith("/"))
      {
        dest += "/";
      }
      dest += Path.GetFileName(source);
      FileInfo fileInf = new FileInfo(source);
      //string uri = "ftp://" + ftpServerIP + "/" + fileInf.Name;
      FtpWebRequest reqFTP;

      // Create FtpWebRequest object from the Uri provided
      reqFTP = (FtpWebRequest) FtpWebRequest.Create(new Uri(dest));

      // Provide the WebPermission Credintials
      reqFTP.Credentials = new NetworkCredential(user, pwd);

      // By default KeepAlive is true, where the control connection is not closed
      // after a command is executed.
      reqFTP.KeepAlive = false;

      // Specify the command to be executed.
      reqFTP.Method = WebRequestMethods.Ftp.UploadFile;

      // Specify the data transfer type.
      reqFTP.UseBinary = true;

      // Notify the server about the size of the uploaded file
      reqFTP.ContentLength = fileInf.Length;

      // The buffer size is set to 2kb
      int buffLength = 2048;
      byte[] buff = new byte[buffLength];
      int contentLen;

      // Opens a file stream (System.IO.FileStream) to read the file to be uploaded
      FileStream fs = fileInf.OpenRead();

      try
      {
        // Stream to which the file to be upload is written
        Stream strm = reqFTP.GetRequestStream();

        // Read from the file stream 2kb at a time
        contentLen = fs.Read(buff, 0, buffLength);

        // Till Stream content ends
        while (contentLen != 0)
        {
          // Write Content from the file stream to the FTP Upload Stream
          strm.Write(buff, 0, contentLen);
          contentLen = fs.Read(buff, 0, buffLength);
        }

        // Close the file stream and the Request Stream
        strm.Close();
        fs.Close();
        return true;
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Upload Error");
        return false;
      }
    }

    public static bool FtpRenam(string source, string newFilename, string user, string pwd)
    {
      FtpWebRequest reqFTP;
      try
      {
        MessageBox.Show(newFilename);
        reqFTP = (FtpWebRequest) FtpWebRequest.Create(new Uri(source));
        reqFTP.Method = WebRequestMethods.Ftp.Rename;
        reqFTP.RenameTo = newFilename;
        reqFTP.UseBinary = true;
        reqFTP.Credentials = new NetworkCredential(user, pwd);
        FtpWebResponse response = (FtpWebResponse) reqFTP.GetResponse();
        Stream ftpStream = response.GetResponseStream();

        ftpStream.Close();
        response.Close();
        if (response.StatusCode == FtpStatusCode.ClosingData)
        {
          return true;
        }
        else
        {
          return false;
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
        return false;
      }
    }


    private void button1_Click(object sender, EventArgs e)
    {
      client.CancelAsync();
      this.Close();
    }
  }
}