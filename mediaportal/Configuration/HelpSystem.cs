using System;
using System.Windows.Forms;
using System.Xml;
using System.Net;
using System.IO;

using MediaPortal.GUI.Library;

namespace MediaPortal.Configuration
{
  public static class HelpSystem
  {
    static string helpReferencesFile = Config.GetFile(Config.Dir.Config, "HelpReferences.xml");
    private const string helpReferencesURL = @"http://install.team-mediaportal.com/HelpReferences_MediaPortal.xml";

    public static void ShowHelp(string sectionName)
    {
      if (!File.Exists(helpReferencesFile))
      {
        MessageBox.Show("No help reference found.\r\nPlease update your help references by pressing 'Update Help' on Project Section.");
        return;
      }

      XmlDocument doc = new XmlDocument();
      doc.Load(helpReferencesFile);

      XmlNode generalNode = doc.SelectSingleNode("/helpsystem/general");
      XmlNodeList sectionNodes = doc.SelectNodes("/helpsystem/sections/section");

      if (sectionNodes != null)
      {
        for (int i = 0; i < sectionNodes.Count; i++)
        {
          XmlNode sectionNode = sectionNodes[i];
          if (sectionNode.Attributes["name"].Value == sectionName)
          {
            System.Diagnostics.Process.Start(
              String.Format(@"{0}{1}",
                            generalNode.Attributes["baseurl"].Value,
                            sectionNode.Attributes["suburl"].Value));
            return;
          }
        }
      }

      Log.Error("No help reference found for section: {0}", sectionName);
      MessageBox.Show(String.Format("No help reference found for section:\r\n       {0}\r\n\r\nPlease update your help references by pressing 'Update Help' on Project Section.", sectionName));
    }

    public static void UpdateHelpReferences()
    {
      string helpReferencesTemp = Path.GetTempFileName();

      Application.DoEvents();
      try
      {
        if (File.Exists(helpReferencesTemp))
          File.Delete(helpReferencesTemp);

        Application.DoEvents();
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(helpReferencesURL);
        Application.DoEvents();

        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        {
          Application.DoEvents();
          using (Stream resStream = response.GetResponseStream())
          {
            using (TextReader tin = new StreamReader(resStream))
            {
              using (TextWriter tout = File.CreateText(helpReferencesTemp))
              {
                while (true)
                {
                  string line = tin.ReadLine();
                  if (line == null) break;
                  tout.WriteLine(line);
                }
              }
            }
          }
        }

        File.Delete(helpReferencesFile);
        File.Move(helpReferencesTemp, helpReferencesFile);

        MessageBox.Show("HelpReferences update succeeded.");
      }
      catch (Exception ex)
      {
        Log.Error("EXCEPTION in UpdateHelpReferences | {0}\r\n{1}", ex.Message, ex.Source);
        MessageBox.Show("HelpReferences update failed.");
      }
      Application.DoEvents();
    }
  }
}
