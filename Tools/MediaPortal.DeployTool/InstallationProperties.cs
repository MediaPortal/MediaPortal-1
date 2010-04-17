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

using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace MediaPortal.DeployTool
{
  class InstallationProperties : NameValueCollection
  {
    readonly string _cfgfile = Path.Combine(Application.StartupPath, "settings.xml");

    #region Singleton implementation
    static readonly InstallationProperties _instance = new InstallationProperties();

    InstallationProperties()
    {

    }
    public static InstallationProperties Instance
    {
      get
      {
        return _instance;
      }
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
