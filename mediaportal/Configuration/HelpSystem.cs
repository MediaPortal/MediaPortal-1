using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Net;
using System.IO;

namespace MediaPortal.Configuration
{
  public static class HelpSystem
  {
    static string helpReferencesFile = String.Format(@"{0}\HelpReferences.xml", Application.StartupPath);
    static string helpReferencesTemp = String.Format(@"{0}_temp", helpReferencesFile);
    static string helpReferencesBaseURL = @"http://install.team-mediaportal.com/HelpReferences_{0}.xml";

    public static void ShowHelp(string sectionName)
    {
      if (!System.IO.File.Exists(helpReferencesFile))
      {
        MessageBox.Show("File not found: {0}", helpReferencesFile);
        return;
      }

      XmlDocument doc = new XmlDocument();
      doc.Load(helpReferencesFile);

      XmlNode generalNode = doc.SelectSingleNode("/helpsystem/general");
      XmlNodeList sectionNodes = doc.SelectNodes("/helpsystem/sections/section");

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

      MessageBox.Show("No help reference found for section: {0}", sectionName);
    }

    public static void UpdateHelpReferences(string product)
    {
      string helpReferencesURL = String.Format(helpReferencesBaseURL, product);

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
            using (System.IO.TextReader tin = new StreamReader(resStream))
            {
              using (System.IO.TextWriter tout = System.IO.File.CreateText(helpReferencesTemp))
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

        System.IO.File.Delete(helpReferencesFile);
        System.IO.File.Move(helpReferencesTemp, helpReferencesFile);

        MessageBox.Show("HelpReferences update succeeded.");
      }
      catch (Exception)
      {
        MessageBox.Show("HelpReferences update failed.");
      }
      Application.DoEvents();
    }
  }
}
