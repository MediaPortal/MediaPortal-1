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
using System.IO;
using System.Collections;
using System.ComponentModel;
using EnterpriseDT.Net.Ftp;
using MediaPortal.GUI.Library;
using System.Threading;

namespace MediaPortal.Util
{
  /// <summary>
  /// This (static) class will handle all ftp connections with remote shares
  /// it contains functions to connect to a remote ftp server and
  /// download a file
  /// </summary>
  public class FtpConnectionCache
  {
    /// <summary>
    /// Subclass handeling 1 ftp connection
    /// </summary>
    private class FtpConnection
    {
      public FTPClient Connection = null; //current FTP connection
      public string HostName = string.Empty; //host name
      public string LoginName = string.Empty; //loginname
      public string Password = string.Empty; //password
      public int Port = 21; //tcp/ip port of the server
      public bool Busy = false; //Flag indicating if we are busy downloading a file
      public bool PASV = false; //use passive mode
      public string RemoteFileName = string.Empty; //remote file we're downloading
      public string LocalFileName = string.Empty; //local file where download is stored
      public string OriginalRemoteFileName = string.Empty; //original remote filename
      public long BytesTransferred = 0; // bytes transferred
      public long BytesOffset = 0; // bytes offset when resuming an ftp download

      public FtpConnection()
      {
        // using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.MPSettings())
      }

      /// <summary>
      /// Function which will be called by the begininvoke() to 
      /// start an asynchronous download
      /// </summary>
      /// <param name="ftp"></param>
      private void StartDownLoad(object sender, DoWorkEventArgs e)
      {
        try
        {
          //Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
          Thread.CurrentThread.Name = "FtpConnection";
          BytesTransferred = 0;
          BytesOffset = 0;

          Connection.TransferNotifyInterval = 65535; //send notify after receiving 64KB

          Connection.CommandSent += new FTPMessageHandler(Connection_CommandSent);
          Connection.ReplyReceived += new FTPMessageHandler(Connection_ReplyReceived);
          Connection.TransferStartedEx += new TransferHandler(Connection_TransferStartedEx);
          Connection.TransferCompleteEx += new TransferHandler(Connection_TransferCompleteEx);
          Connection.BytesTransferred += new BytesTransferredHandler(OnBytesTransferred);
          Connection.TransferType = FTPTransferType.BINARY;

          Log.Info("FTPConnection: Start download: {0}->{1}", RemoteFileName, LocalFileName);
          if (System.IO.File.Exists(LocalFileName))
          {
            FileInfo info = new FileInfo(LocalFileName);
            BytesOffset = info.Length;
            Connection.Resume();
          }


          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_FILE_DOWNLOADING, 0, 0, 0, 0, 0, null);
          msg.Label = OriginalRemoteFileName;
          msg.Label2 = LocalFileName;
          msg.Param1 = (int)(BytesTransferred + BytesOffset);
          GUIGraphicsContext.SendMessage(msg);

          Connection.Get(LocalFileName, RemoteFileName);
          Connection.BytesTransferred -= new BytesTransferredHandler(OnBytesTransferred);
          Log.Info("FTPConnection: Download finished: {0}->{1}", RemoteFileName, LocalFileName);
        }
        catch (Exception ex)
        {
          Log.Warn("FTPConnection: Download of {0} stopped", LocalFileName);
          Log.Error(ex);
        }
        finally
        {
          Connection.TransferStartedEx -= new TransferHandler(Connection_TransferStartedEx);
          Connection.CommandSent -= new FTPMessageHandler(Connection_CommandSent);
          Connection.ReplyReceived -= new FTPMessageHandler(Connection_ReplyReceived);
          Connection.BytesTransferred -= new BytesTransferredHandler(OnBytesTransferred);
          if (Connection.Connected)
          {
            Connection.QuitImmediately();
          }
          BytesTransferred = 0;
          BytesOffset = 0;
          Busy = false;
        }
      }

      private void Connection_TransferStartedEx(object sender, TransferEventArgs e)
      {
        Log.Debug("ftp: Transfer started {0}->{1}", e.RemoteFilename, e.LocalFilePath);
      }

      private void Connection_ReplyReceived(object sender, FTPMessageEventArgs e)
      {
        Log.Debug("ftp:Cmd  :{0}", e.Message);
      }

      private void Connection_CommandSent(object sender, FTPMessageEventArgs e)
      {
        Log.Debug("ftp:reply:{0}", e.Message);
      }


      /// <summary>
      /// Function to start a download
      /// </summary>
      /// <param name="orgremoteFile"></param>
      /// <param name="remotefile"></param>
      /// <param name="localfile"></param>
      public void Download(string orgremoteFile, string remotefile, string localfile)
      {
        string name = System.IO.Path.GetFileName(remotefile);
        string folder = orgremoteFile.Substring("remote:".Length);
        folder = folder.Substring(0, folder.Length - (name.Length + 1));
        string[] subitems = folder.Split(new char[] {'?'});
        if (subitems[4] == string.Empty) subitems[4] = "/";
        bool fileExists = false;
        string tmpDir = string.Empty;
        FTPFile[] files;
        try
        {
          tmpDir = subitems[4];
          Connection.ChDir(tmpDir);
          files = Connection.DirDetails(); //(tmpDir);
          for (int i = 0; i < files.Length; ++i)
          {
            FTPFile file = files[i];
            if (file.Dir) continue;
            if (String.Compare(file.Name, name, true) == 0)
            {
              fileExists = true;
              break;
            }
          }
          if (!fileExists)
          {
            if (Connection.Connected)
            {
              Connection.QuitImmediately();
            }
            Busy = false;
            return;
          }
        }
        catch (Exception)
        {
          return;
        }

        Log.Debug("FTPConnection: Download {0} from {1} to {2}", remotefile, tmpDir, localfile);
        LocalFileName = localfile;
        RemoteFileName = remotefile;
        OriginalRemoteFileName = orgremoteFile;
        BackgroundWorker worker = new BackgroundWorker();
        worker.DoWork += new DoWorkEventHandler(StartDownLoad);
        worker.RunWorkerAsync();
      }

      /// <summary>
      /// callback from the ftp library when some data has been transferred
      /// We just send a message to the current window so it can update its status
      /// </summary>
      /// <param name="ftpClient"></param>
      /// <param name="bytesTransferred"></param>
      private void OnBytesTransferred(object ftpClient, BytesTransferredEventArgs bytesTransferred)
      {
        try
        {
          BytesTransferred = bytesTransferred.ByteCount;

          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_FILE_DOWNLOADING, 0, 0, 0, 0, 0, null);
          msg.Label = OriginalRemoteFileName;
          msg.Label2 = LocalFileName;
          msg.Param1 = (int)(BytesTransferred + BytesOffset);
          GUIGraphicsContext.SendMessage(msg);
        }
        catch (Exception ex)
        {
          Log.Error(ex);
        }
      }
    }

    /// <summary>
    /// list containing all active ftp connections
    /// </summary>
    private static ArrayList ftpConnections = new ArrayList();

    static FtpConnectionCache() {}

    /// <summary>
    /// Checks if we have a idle connection to the remote ftp server
    /// </summary>
    /// <param name="hostname">hostname or ipadres of the ftp server</param>
    /// <param name="login">loginname</param>
    /// <param name="password">password</param>
    /// <param name="port">tcp/ip port</param>
    /// <param name="ftpclient">on return contains the idle ftp connection for this server
    /// or null if none is found</param>
    /// <returns>
    /// true: found an idle connection, this is returned in ftpclient
    /// false: no idle connections found. ftpclient =null
    /// </returns>
    public static bool InCache(string hostname, string login, string password, int port, bool active,
                               out FTPClient ftpclient)
    {
      ftpclient = null;
      foreach (FtpConnection client in ftpConnections)
      {
        if (client.HostName == hostname &&
            client.LoginName == login &&
            client.Password == password && client.Port == port)
        {
          if (!client.Busy)
          {
            ftpclient = client.Connection;
            return true;
          }
        }
      }
      return false;
    }

    /// <summary>
    /// Create a new ftp connection to the remote server
    /// </summary>
    /// <param name="hostname">hostname or ip adres of remote ftp server</param>
    /// <param name="login">loginname</param>
    /// <param name="password">password</param>
    /// <param name="port">tcpip port</param>
    /// <param name="active">active or PASV</param>
    /// <returns>
    /// instance of an FTPClient handling the ftp connection
    /// or null if no connection could be made
    /// </returns>
    public static FTPClient MakeConnection(string hostname, string login, string password, int port, bool active)
    {
      try
      {
        Log.Info("FTPConnection: Connect to ftp://{0}:{1} with user {2}", hostname, port, login);
        FtpConnection newConnection = new FtpConnection();
        newConnection.HostName = hostname;
        newConnection.LoginName = login;
        newConnection.Password = password;
        newConnection.Port = port;
        newConnection.Busy = false;
        newConnection.Connection = new FTPClient();
        newConnection.Connection.RemoteHost = hostname;
        newConnection.HostName = hostname;
        newConnection.Port = port;
        newConnection.Connection.Connect();
        newConnection.Connection.Login(login, password);
        newConnection.Connection.ConnectMode = FTPConnectMode.ACTIVE;
        ftpConnections.Add(newConnection);
//#if DEBUG
        newConnection.Connection.DebugResponses(true);
//#endif
        Log.Debug("FTPConnection: New connection successful");
        return newConnection.Connection;
      }
      catch (Exception ex)
      {
        Log.Error("FTPConnection: Unable to connect to ftp://{0}:{1} reason: {2}", hostname, port, ex.Message);
        return null;
      }
    }

    /// <summary>
    /// Remove an ftp client from the cache
    /// this can be used to remove ftp clients which are disconnected
    /// </summary>
    /// <param name="ftpclient">FTPClient</param>
    public static void Remove(FTPClient ftpclient)
    {
      foreach (FtpConnection client in ftpConnections)
      {
        if (client.Connection == ftpclient)
        {
          ftpConnections.Remove(client);
          return;
        }
      }
    }

    /// <summary>
    /// Start downloading a file from a remote server to local harddisk
    /// orgremoteFile is in format remote:hostname?port?login?password?folder
    /// while the remotefile only contains the remote path+filename
    /// </summary>
    /// <param name="ftpclient">FTP client to use</param>
    /// <param name="orgremoteFile">remote file including all details</param>
    /// <param name="remotefile">remote file including only path+filename</param>
    /// <param name="localfile">filename where download should be stored</param>
    /// <returns>
    /// true: download is started
    /// false: unable to download file
    /// </returns>
    public static bool Download(FTPClient ftpclient, string orgremoteFile, string remotefile, string localfile)
    {
      foreach (FtpConnection client in ftpConnections)
      {
        if (client.Connection == ftpclient)
        {
          if (!client.Busy)
          {
            client.Busy = true;
            client.Download(orgremoteFile, remotefile, localfile);

            return true;
          }
        }
      }
      return false;
    }

    /// <summary>
    /// Method which checks if the remotefile is being downloaded or not
    /// remote file is in format remote:hostname?port?login?password?folder
    /// </summary>
    /// <param name="remotefile">remote file</param>
    /// <returns>true: file is being downloaded
    /// false: file is not being downloaded</returns>
    public static bool IsDownloading(string remotefile)
    {
      foreach (FtpConnection client in ftpConnections)
      {
        if (client.Busy && client.OriginalRemoteFileName == remotefile)
        {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Callback from the FTP client. This is called when a file has been downloaded
    /// We're just sending a message to the current window so it can update its view
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void Connection_TransferCompleteEx(object sender, TransferEventArgs e)
    {
      Log.Info("FTPConnection: Transfer completed: {0}->{1}", e.RemoteFilename, e.LocalFilePath);
      try
      {
        FTPClient ftpclient = sender as FTPClient;
        foreach (FtpConnection client in ftpConnections)
        {
          if (client.Connection == ftpclient)
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_FILE_DOWNLOADED, 0, 0, 0, 0, 0, null);
            msg.Label = client.OriginalRemoteFileName;
            msg.Label2 = client.LocalFileName;
            GUIGraphicsContext.SendMessage(msg);
            return;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }
  }
}