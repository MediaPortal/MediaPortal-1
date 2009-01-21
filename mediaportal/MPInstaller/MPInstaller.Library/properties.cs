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
using System.ComponentModel;
using System.ComponentModel.Design;
using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Xml;

namespace MediaPortal.MPInstaller
{
  public class ProiectPropertiesClass
  {
    private string mpminversion;
    private string mpmaxversion;
    private string minextensionversion;
    private string maxextensionversion;
    private string forumurl;
    private string weburl;
    private DateTime creationdate;
    private bool singlegroupselect;
    private bool clarskincache;

    public ProiectPropertiesClass()
    {
      Clear();
    }
    [Description("Minimum version of MediaPortal")]
    public string MPMinVersion
    {
      set
      {
        mpminversion = value;
      }
      get
      {
        return mpminversion;
      }
    }
    [
    Description("Maximum version of MediaPortal"),
    ]
    public string MPMaxVersion
    {
      set
      {
        mpmaxversion = value;
      }
      get
      {
        return mpmaxversion;
      }
    }

    public string MinExtensionVersion
    {
      set
      {
        minextensionversion = value;
      }
      get
      {
        return minextensionversion;
      }
    }

    public string MaxExtensionVersion
    {
      set
      {
        maxextensionversion = value;
      }
      get
      {
        return maxextensionversion;
      }
    }

    public string ForumURL
    {
      set
      {
        forumurl = value;
      }
      get
      {
        return forumurl;
      }
    }

    public string WebURL
    {
      set
      {
        weburl = value;
      }
      get
      {
        return weburl;
      }
    }

    public DateTime CreationDate
    {
      set
      {
        creationdate = value;
      }
      get
      {
        return creationdate;
      }
    }
    [Description("Used only when a grouping is specified. If is True alove select only one group")]
    public bool SingleGroupSelect
    {
      set
      {
        singlegroupselect = value;
      }
      get
      {
        return singlegroupselect;
      }
    }

    [Description("If true clear the skin cache ")]
    public bool ClearSkinCache
    {
      get
      {
        return clarskincache;
      }
      set
      {
        clarskincache = value;
      }
    }
    public void Save(XmlWriter writer)
    {
      writer.WriteElementString("MPMaxVersion", MPMaxVersion);
      writer.WriteElementString("MPMinVersion", MPMinVersion);
      writer.WriteElementString("MinExtensionVersion", MinExtensionVersion);
      writer.WriteElementString("MaxExtensionVersion", MaxExtensionVersion);
      writer.WriteElementString("ForumURL", ForumURL);
      writer.WriteElementString("WebURL", WebURL);
      writer.WriteElementString("CreationDate", CreationDate.ToString("dd-MM-yy"));
      writer.WriteElementString("SingleGroupSelect", SingleGroupSelect.ToString());
      writer.WriteElementString("ClearSkinCache", ClearSkinCache.ToString());
    }

    public void Load(XmlNode basenode)
    {
      if (basenode != null)
      {
        XmlNode node;
        node = basenode.SelectSingleNode("MPMaxVersion");
        if (node != null && node.InnerText != null)
          MPMaxVersion = node.InnerText;
        node = basenode.SelectSingleNode("MPMinVersion");
        if (node != null && node.InnerText != null)
          MPMinVersion = node.InnerText;
        node = basenode.SelectSingleNode("MinExtensionVersion");
        if (node != null && node.InnerText != null)
          MinExtensionVersion = node.InnerText;
        node = basenode.SelectSingleNode("MaxExtensionVersion");
        if (node != null && node.InnerText != null)
          MaxExtensionVersion = node.InnerText;
        node = basenode.SelectSingleNode("ForumURL");
        if (node != null && node.InnerText != null)
          ForumURL = node.InnerText;
        node = basenode.SelectSingleNode("WebURL");
        if (node != null && node.InnerText != null)
          WebURL = node.InnerText;
        node = basenode.SelectSingleNode("CreationDate");
        if (node != null && node.InnerText != null)
          try
          {
            CreationDate = DateTime.ParseExact(node.InnerText, "dd-MM-yy", null);
          }
          catch (System.FormatException)
          {
            CreationDate = DateTime.Now;
          }
        node = basenode.SelectSingleNode("SingleGroupSelect");
        if (node != null && node.InnerText != null)
          if (node.InnerText == "True")
            SingleGroupSelect = true;
          else SingleGroupSelect = false;

        node = basenode.SelectSingleNode("ClearSkinCache");
        if (node != null && node.InnerText != null)
          if (node.InnerText == "True")
            ClearSkinCache = true;
          else ClearSkinCache = false;



      }

    }

    public void Clear()
    {
      MPMaxVersion = string.Empty;
      MPMinVersion = string.Empty;
      ForumURL = string.Empty;
      WebURL = string.Empty;
      CreationDate = DateTime.Today;
      SingleGroupSelect = false;
    }
  }

  public class FilePropertiesClass
  {
    private string outputfilename;
    private bool defaultfile;
    public FilePropertiesClass()
    {

    }

    public string OutputFileName
    {
      set
      {
        outputfilename = value;
      }
      get
      {
        return outputfilename;
      }
    }

    public bool  DefaultFile
    {
      set
      {
        defaultfile = value;
      }
      get
      {
        return defaultfile;
      }

    }
    
    public void Clear()
    {
      OutputFileName = string.Empty;
      DefaultFile = false;
    }
    
    override public string ToString()
    {
      string x_ret=string.Empty;;
      x_ret += "OutputFileName" + "=" + OutputFileName+"|";
      x_ret += "DefaultFile" + "=" + DefaultFile.ToString() + "|";
      return x_ret;
    }
    
    public FilePropertiesClass Parse(string fullstring)
    {
      this.Clear();
      string[] temparray = fullstring.Split('|');
      foreach(string s in temparray)
      {
        if (s.Contains("="))
        {
          switch (s.Substring(0, s.IndexOf('=')))
          {
            case "OutputFileName":
              OutputFileName = s.Substring(s.IndexOf('=') + 1);
              break;
            case "DefaultFile":
              DefaultFile = s.Substring(s.IndexOf('=') + 1).ToUpper()=="TRUE"?true :false;
              break;
          }
        }
      }
      return this;
    }
  }
}
