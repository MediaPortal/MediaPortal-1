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

using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace MediaPortal.DeployTool
{
  internal class InstallationProperties : NameValueCollection
  {
    private readonly string _cfgfile = Path.Combine(Application.StartupPath, "settings.xml");

    #region Singleton implementation

    private static readonly InstallationProperties _instance = new InstallationProperties();

    private InstallationProperties() {}

    public static InstallationProperties Instance
    {
      get { return _instance; }
    }

    #endregion

    public void Save()
    {
      XmlTextWriter txtWriter = new XmlTextWriter(_cfgfile, Encoding.ASCII)
                                  {
                                    Formatting = Formatting.Indented
                                  };
      txtWriter.WriteStartDocument();
      txtWriter.WriteStartElement("DeployTool");
      foreach (string key in _instance.Keys)
      {
        txtWriter.WriteElementString(key, _instance.GetValues(key)[0]);
      }
      txtWriter.WriteEndElement();
      txtWriter.WriteEndDocument();
      txtWriter.Close();
    }

    public void Load()
    {
      string e = null;
      XmlTextReader txtReader = new XmlTextReader(_cfgfile);

      while (txtReader.Read())
      {
        txtReader.MoveToElement();
        switch (txtReader.NodeType)
        {
          case XmlNodeType.Element:
            e = txtReader.Name;
            break;
          case XmlNodeType.Text:
            Instance.Set(e, txtReader.Value);
            break;
        }
      }
    }
  }
}