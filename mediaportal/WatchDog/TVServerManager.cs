#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using WatchDogService.Interface;
using MediaPortal.Profile;

namespace WatchDog
{
  class TVServerManager
  {
    HttpChannel _httpChannel = new HttpChannel();
    WatchDogServiceInterface _remoteObject = null;
    string _url = string.Empty;

    public TVServerManager()
    {
      try
      {
        ChannelServices.RegisterChannel(_httpChannel, false);
      }
      catch (Exception ex)
      {
      }

      string hostName;
      try
      {
        using (Settings xmlreader = new MPSettings())
        {
          hostName = xmlreader.GetValueAsString("tvservice", "hostname", string.Empty);
        }

        if (string.IsNullOrEmpty(hostName))
        {
          MessageBox.Show("The host name is empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        }
      }
      catch (Exception)
      {
        MessageBox.Show("The host name is empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }
      _url = string.Format("http://{0}:9997/WatchDogServer", hostName);

    }

    public void TvServerRemoteStart()
    {
      string result = string.Empty;
      try
      {
        _remoteObject = (WatchDogServiceInterface)Activator.GetObject(typeof(WatchDogServiceInterface), _url);

        result = _remoteObject.StartTVService();
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      try
      {
        ChannelServices.UnregisterChannel(_httpChannel);
      }
      catch (Exception ex)
      {
      }
      MessageBox.Show(result, "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public void TvServerRemoteStop()
    {
      string result = string.Empty;
      try
      {
        _remoteObject = (WatchDogServiceInterface)Activator.GetObject(typeof(WatchDogServiceInterface), _url);

        result = _remoteObject.StopTVService();
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      try
      {
        ChannelServices.UnregisterChannel(_httpChannel);
      }
      catch (Exception ex)
      {
      }
      MessageBox.Show(result, "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public void TvServerRemoteLogRead(string ZipFile)
    {
      try
      {
        _remoteObject = (WatchDogServiceInterface)Activator.GetObject(typeof(WatchDogServiceInterface), _url);

        MemoryStream sr = new MemoryStream();

        sr = (MemoryStream)_remoteObject.ReadLog();

        using (FileStream writer = new FileStream(ZipFile, FileMode.CreateNew, FileAccess.Write))
        {
          byte[] buf = new byte[1024 * 1024];
          sr.Seek(0, SeekOrigin.Begin);
          int bytesRead = sr.Read(buf, 0, 1024 * 1024);
          while (bytesRead > 0)
          {
            writer.Write(buf, 0, bytesRead);
            bytesRead = sr.Read(buf, 0, 1024 * 1024);
          }
          writer.Flush();
          writer.Close();
          sr.Close();
          sr.Close();
          sr.Dispose();
        }

      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      try
      {
        ChannelServices.UnregisterChannel(_httpChannel);
      }
      catch (Exception ex)
      {
      }
    }

    public void RebootTvServer()
    {
      try
      {
        _remoteObject = (WatchDogServiceInterface)Activator.GetObject(typeof(WatchDogServiceInterface), _url);

        _remoteObject.Reboot();
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }
      MessageBox.Show("Done", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public void ShutdownTvServer()
    {
      try
      {
        _remoteObject = (WatchDogServiceInterface)Activator.GetObject(typeof(WatchDogServiceInterface), _url);

        _remoteObject.Shutdown();
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }
      MessageBox.Show("Done", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public void PowerOffTvServer()
    {
      try
      {
        _remoteObject = (WatchDogServiceInterface)Activator.GetObject(typeof(WatchDogServiceInterface), _url);

        _remoteObject.PowerOff();
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }
      MessageBox.Show("Done", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public void ClearTVserverLogs()
    {
      string result = string.Empty;
      try
      {
        _remoteObject = (WatchDogServiceInterface)Activator.GetObject(typeof(WatchDogServiceInterface), _url);

        result = _remoteObject.ClearTVserverLogs();
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      try
      {
        ChannelServices.UnregisterChannel(_httpChannel);
      }
      catch (Exception ex)
      {
      }
      MessageBox.Show(result, "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public void ClearWindowsEventLogs()
    {
      string result = string.Empty;
      try
      {
        _remoteObject = (WatchDogServiceInterface)Activator.GetObject(typeof(WatchDogServiceInterface), _url);

        result = _remoteObject.ClearWindowsEventLogs();
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      try
      {
        ChannelServices.UnregisterChannel(_httpChannel);
      }
      catch (Exception ex)
      {
      }
      MessageBox.Show(result, "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

  }
}
