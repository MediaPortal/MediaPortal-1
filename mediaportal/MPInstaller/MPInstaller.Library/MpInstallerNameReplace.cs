using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace MediaPortal.MPInstaller
{
  public class MpInstallerNameReplace
  {
    private Dictionary<string, string> _groupSubstitution;

    public Dictionary<string, string> GroupSubstitutions
    {
      get { return _groupSubstitution; }
      set { _groupSubstitution = value; }
    }

    private Dictionary<string, string> _nameCleanups;

    public Dictionary<string, string> NameCleanups
    {
      get { return _nameCleanups; }
      set { _nameCleanups = value; }
    }

    public MpInstallerNameReplace()
    {
      GroupSubstitutions = new Dictionary<string, string>();
      NameCleanups = new Dictionary<string, string>();
    }

    public void Load(string filename)
    {
      XmlDocument doc = new XmlDocument();
      if (File.Exists(filename))
      {
        try
        {
          doc.Load(filename);
          XmlNode ver = doc.DocumentElement.SelectSingleNode("/MPinstallerCleenup");
          XmlNodeList NameCleanupsList = ver.SelectNodes("GroupSubstitution/Substitution");
          foreach (XmlNode nodeSubstitution in NameCleanupsList)
          {
            GroupSubstitutions.Add(nodeSubstitution.Attributes["key"].Value, nodeSubstitution.Attributes["value"].Value);
          }
          NameCleanupsList = ver.SelectNodes("NameCleanups/Substitution");
          foreach (XmlNode nodeNameCleanups in NameCleanupsList)
          {
            NameCleanups.Add(nodeNameCleanups.Attributes["key"].Value, nodeNameCleanups.Attributes["value"].Value);
          }
        }
        catch (Exception ex)
        {
          MessageBox.Show(ex.Message);
        }
      }
    }
  }
}