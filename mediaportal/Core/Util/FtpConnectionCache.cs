using System;
using System.Collections;
using EnterpriseDT.Net.Ftp;

namespace Core.Util
{
	/// <summary>
	/// Summary description for FtpConnectionCache.
	/// </summary>

	public class FtpConnectionCache
	{
    class FtpConnection
    {
      public FTPClient Connection=null;
      public string    HostName=String.Empty;
      public string    LoginName=String.Empty;
      public string    Password=String.Empty;
      public int       Port=21;
      public bool      Busy=false;
      public string    remoteFile=String.Empty;
      public string    localFile=String.Empty;

      delegate void OnDownloadHandler(FtpConnection ftp);
      event     OnDownloadHandler OnDownLoad=null;
      public FtpConnection()
      {
      }
      
      void GetCallback(IAsyncResult ar)
      {

        OnDownLoad.EndInvoke(ar);
        OnDownLoad -=new OnDownloadHandler(StartDownLoad);
        Busy=false;
      }
      
      void StartDownLoad(FtpConnection ftp)
      {
        ftp.Connection.TransferComplete += new EventHandler(OnTransferComplete);
        ftp.Connection.TransferType=FTPTransferType.BINARY;

        ftp.Connection.Get(ftp.localFile,ftp.remoteFile);
      }

      public void Download(string remotefile, string localfile)
      {
        localFile=localfile;
        remoteFile=remotefile;
        
        OnDownLoad +=new OnDownloadHandler(StartDownLoad);
        AsyncCallback callback = new AsyncCallback(GetCallback);
        OnDownLoad.BeginInvoke(this,callback,this);

      }

    }

    static ArrayList ftpConnections=new ArrayList();
		
    static public bool InCache(string hostname, string login, string password, int port, out FTPClient ftpclient)
    {
      ftpclient=null;
      foreach (FtpConnection client in ftpConnections)
      {
        if (client.HostName==hostname && 
            client.LoginName==login && 
            client.Password==password && client.Port==port)
        {
          if (!client.Busy)
          {
            ftpclient= client.Connection;
            return true;
          }
        }
      }
      return false;
    }
    static public FTPClient MakeConnection(string hostname, string login, string password, int port)
    {
      try
      {
        FtpConnection newConnection = new FtpConnection();
        newConnection.HostName=hostname;
        newConnection.LoginName=login;
        newConnection.Password=password;
        newConnection.Port=port;
        newConnection.Busy=false;
        newConnection.Connection = new FTPClient(hostname,port);
        newConnection.Connection.Login(login,password);
        ftpConnections.Add(newConnection);
        return newConnection.Connection;
      }
      catch(Exception)
      {
        return null;
      }
    }

    static public void Remove(FTPClient ftpclient)
    {
      foreach (FtpConnection client in ftpConnections)
      {
        if (client.Connection==ftpclient)
        {
          ftpConnections.Remove(client);
          return;
        }
      }
    }

    static public bool Download(FTPClient ftpclient,string remotefile,string localfile)
    {
      foreach (FtpConnection client in ftpConnections)
      {
        if (client.Connection==ftpclient)
        {
          if (!client.Busy)
          {
            client.Busy=true;
            client.Download(remotefile,localfile);

            return true;
          }
        }
      }
      return false;
    }

    public static bool IsDownloading(string remotefile)
    {
      foreach (FtpConnection client in ftpConnections)
      {
        if (client.Busy && client.remoteFile==remotefile)
        {
          return true;
        }
      }
      return false;
    }

    private static void OnTransferComplete(object sender, EventArgs e)
    {
      FTPClient ftpclient=sender as FTPClient;
      foreach (FtpConnection client in ftpConnections)
      {
        if (client.Connection==ftpclient)
        {
          client.Connection.TransferComplete -= new EventHandler(OnTransferComplete);
          return;
        }
      }
    }

    

  }
}
