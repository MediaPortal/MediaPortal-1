using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;

namespace MediaPortal.DeployTool
{
  public enum SnapshotType
  {
    MediaPortal,
    TvServer
  }
  public class SnapshotLookup
  {
    private static void ParseMediaPortalEntry(string line,out string downloadUrl,out string revision)
    {
      line = line.Remove(0, line.IndexOf("shot: ") + 6);
      line = line.Replace(' ', '-');
      line=line.Insert(line.IndexOf(DateTime.Now.Year.ToString()) + 4, "-");
      revision = line.Substring(line.IndexOf(':') + 1, 5);
      line = line.Substring(0, line.IndexOf("Revision") - 4);
      downloadUrl = "http://svn.team-mediaportal.com/MediaPortal_SVN-Snaps_based_on_V0.2.3.0_Release/MediaPortal-svn--" + line + "-Rev" + revision + ".exe";
    }
    private static void ParseTvServerEntry(string line, out string downloadUrl, out string revision)
    {
    
      line = line.Remove(0, line.IndexOf("shot: ") + 6);
      line = line.Replace(' ', '-');
      revision = line.Substring(line.IndexOf("Revision") + 9, 5);
      line = line.Substring(0, line.IndexOf("Revision") - 1);
      line = line.Insert(10, "-");
      downloadUrl = "http://tvengine3.team-mediaportal.com/tvengine3-" + line + "--Rev" + revision + ".zip";
    }
    public static bool GetSnapshotInfo(SnapshotType sType, out string downloadUrl, out string revision)
    {
      string url = "";
      downloadUrl = null;
      revision = null;
      switch (sType)
      {
        case SnapshotType.MediaPortal:
          url = "http://forum.team-mediaportal.com/mediaportal_nightly_builds_v0_2_3_0-f229.html";
          break;
        case SnapshotType.TvServer:
          url = "http://forum.team-mediaportal.com/tv_server_nightly_builds-f197.html";
          break;
      }
      HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
      request.CookieContainer = new CookieContainer();
      Cookie c1 = new Cookie("bblastvisit", "0", "/", ".team-mediaportal.com");
      request.CookieContainer.Add(c1);
      Cookie c2 = new Cookie("bblastactivity", "0", "/", ".team-mediaportal.com");
      request.CookieContainer.Add(c2);
      request.AllowAutoRedirect = false;
      WebResponse response = null;
      try
      {
        response = request.GetResponse();
      }
      catch (Exception)
      {
        return false;
      }
      Stream stream = response.GetResponseStream();
      StreamReader reader = new StreamReader(stream);
      string line;
      while ((line = reader.ReadLine()) != null)
      {
        if (line.Contains("Revision"))
        {
          switch (sType)
          {
            case SnapshotType.MediaPortal:
              ParseMediaPortalEntry(line, out downloadUrl, out revision);
              break;
            case SnapshotType.TvServer:
              ParseTvServerEntry(line, out downloadUrl, out revision);
              break;
          }
          break;
        }
      }
      reader.Close();
      stream.Close();
      response.Close();
      return (downloadUrl!=null && revision!=null);
    }
  }
}
