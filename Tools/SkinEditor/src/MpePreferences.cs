#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Mpe.Forms;

namespace Mpe
{
  public class MpePreferences
  {
    #region Variables

    // Internal
    private FileInfo file;
    private DirectoryInfo homeDir;
    // Preferences
    private string mediaPortalDir;
    private string language;
    private MpeLogLevel logLevel = MpeLogLevel.Info;
    // Window Preferences
    private Point windowPosition;
    private Size windowSize;
    private FormWindowState windowState;

    #endregion

    #region Constructors

    public MpePreferences(string filename)
    {
      homeDir = new DirectoryInfo(Directory.GetCurrentDirectory());
      file = new FileInfo(homeDir + "\\" + filename);
      windowState = FormWindowState.Maximized;
      language = "English";
    }

    #endregion

    #region Properties

    [Category("General")]
    [ReadOnly(true)]
    public DirectoryInfo HomeDir
    {
      get { return homeDir; }
    }

    [Browsable(false)]
    public DirectoryInfo HelpDir
    {
      get { return new DirectoryInfo(homeDir.FullName + "\\Help"); }
    }

    [Category("General")]
    [Editor(typeof(DirectoryEditor), typeof(UITypeEditor))]
    [Description("The folder or directory where MediaPortal is installed.")]
    public string MediaPortalDir
    {
      get { return mediaPortalDir; }
      set
      {
        try
        {
          DirectoryInfo dir = new DirectoryInfo(value);
          if (dir == null || dir.Exists == false)
          {
            throw new ArgumentException("Invalid MediaPortal directory.  The given directory does not exist.");
          }
          if (dir.GetDirectories("skin").Length != 1)
          {
            throw new ArgumentException("Invalid MediaPortal directiry. Missing skin folder.");
          }
          if (dir.GetDirectories("language").Length != 1)
          {
            throw new ArgumentException("Invalid MediaPortal directiry. Missing language folder.");
          }
          mediaPortalDir = dir.FullName;
          MpeLog.Info("Preferences.MediaPortalDir set to [" + mediaPortalDir + "]");
          return;
        }
        catch (Exception ee)
        {
          throw new ArgumentException(ee.Message);
        }
      }
    }

    [Category("Log")]
    [Description("Sets the threshold of the logger at application startup.")]
    public MpeLogLevel LogLevel
    {
      get { return logLevel; }
      set
      {
        if (logLevel != value)
        {
          logLevel = value;
        }
      }
    }

    [Browsable(false)]
    public DirectoryInfo[] MediaPortalSkins
    {
      get
      {
        try
        {
          DirectoryInfo d = new DirectoryInfo(mediaPortalDir);
          if (d != null && d.Exists)
          {
            return d.GetDirectories("skin\\*");
          }
        }
        catch (Exception e)
        {
          MpeLog.Debug(e);
        }
        return new DirectoryInfo[0];
      }
    }

    [Browsable(false)]
    public DirectoryInfo MediaPortalSkinDir
    {
      get { return new DirectoryInfo(mediaPortalDir + "\\skin"); }
    }

    [Browsable(false)]
    public Point WindowPosition
    {
      get { return windowPosition; }
      set { windowPosition = value; }
    }

    [Category("Language")]
    [ReadOnly(true)]
    public string DefaultLanguage
    {
      get { return language; }
      set { language = value; }
    }

    [Browsable(false)]
    public Size WindowSize
    {
      get { return windowSize; }
      set { windowSize = value; }
    }

    [Browsable(false)]
    public FormWindowState WindowState
    {
      get { return windowState; }
      set { windowState = value; }
    }

    #endregion

    #region Methods

    public void Load()
    {
      if (file == null || file.Exists == false)
      {
        Save();
        return;
      }
      try
      {
        XPathDocument doc = new XPathDocument(file.FullName);
        XPathNavigator nav = doc.CreateNavigator();
        XPathNodeIterator iterator = nav.Select("MediaPortalEditor/Preferences");
        if (iterator.MoveNext())
        {
          MediaPortalDir = GetXmlValue(iterator, "MediaPortalDir", MediaPortalDir);
          WindowPosition = GetXmlPoint(iterator, "WindowPosition", WindowPosition);
          WindowSize = GetXmlSize(iterator, "WindowSize", WindowSize);
          WindowState = GetXmlWindowState(iterator, "WindowState", WindowState);
          LogLevel = GetXmlLogLevel(iterator, "LogLevel", LogLevel);
        }
      }
      catch (Exception e)
      {
        MpeLog.Debug(e);
        throw new MpeParserException(e.Message);
      }
    }

    public void Save()
    {
      XmlTextWriter writer = null;
      try
      {
        XmlDocument doc = new XmlDocument();
        XmlNode root = doc.CreateElement("MediaPortalEditor");
        doc.AppendChild(root);
        XmlNode node = doc.CreateElement("Preferences");
        root.AppendChild(node);

        SetXmlValue(doc, node, "MediaPortalDir", MediaPortalDir);
        SetXmlValue(doc, node, "WindowPosition", (WindowPosition.X + "," + WindowPosition.Y));
        SetXmlValue(doc, node, "WindowSize", (WindowSize.Width + "," + WindowSize.Height));
        SetXmlValue(doc, node, "WindowState", WindowState.ToString());
        SetXmlValue(doc, node, "LogLevel", LogLevel.ToString());

        writer = new XmlTextWriter(file.FullName, Encoding.UTF8);
        writer.Formatting = Formatting.Indented;
        writer.Indentation = 3;
        doc.WriteTo(writer);
        writer.Close();
      }
      catch (Exception e)
      {
        MpeLog.Debug(e);
        MpeLog.Error(e);
        throw new MpeParserException(e.Message);
      }
    }

    private void SetXmlValue(XmlDocument doc, XmlNode node, string name, string value)
    {
      // First check to see if the element already exists, and if so, remove it
      if (value == null || value.Length == 0)
      {
        value = "-";
      }
      XmlNode n = node.SelectSingleNode(name);
      if (n != null)
      {
        n.RemoveAll();
        n.AppendChild(doc.CreateTextNode(value));
        return;
      }
      XmlElement e = doc.CreateElement(name);
      e.AppendChild(doc.CreateTextNode(value));
      node.AppendChild(e);
    }

    private string GetXmlValue(XPathNodeIterator iterator, string tagName, string defaultValue)
    {
      XPathNodeIterator i = iterator.Current.Select(tagName);
      if (i.MoveNext())
      {
        return (i.Current.Value != null && i.Current.Value.Equals("-") == false) ? i.Current.Value : defaultValue;
      }
      return defaultValue;
    }

    private FormWindowState GetXmlWindowState(XPathNodeIterator iterator, string tagName, FormWindowState defaultValue)
    {
      XPathNodeIterator i = iterator.Current.Select(tagName);
      if (i.MoveNext())
      {
        string s = i.Current.Value;
        if (s != null)
        {
          switch (s)
          {
            case "Normal":
              return FormWindowState.Normal;
            case "Maximized":
              return FormWindowState.Maximized;
            case "Minimized":
              return FormWindowState.Minimized;
          }
        }
      }
      return defaultValue;
    }

    private MpeLogLevel GetXmlLogLevel(XPathNodeIterator iterator, string tagName, MpeLogLevel defaultValue)
    {
      XPathNodeIterator i = iterator.Current.Select(tagName);
      if (i.MoveNext())
      {
        string s = i.Current.Value;
        if (s != null)
        {
          switch (s)
          {
            case "Debug":
              return MpeLogLevel.Debug;
            case "Info":
              return MpeLogLevel.Info;
            case "Warn":
              return MpeLogLevel.Warn;
            case "Error":
              return MpeLogLevel.Error;
          }
        }
      }
      return defaultValue;
    }

    private Point GetXmlPoint(XPathNodeIterator iterator, string tagName, Point defaultValue)
    {
      XPathNodeIterator i = iterator.Current.Select(tagName);
      if (i.MoveNext())
      {
        string s = i.Current.Value;
        if (s != null && s.Equals("-") == false)
        {
          string[] values = s.Split(',');
          if (values.Length == 2)
          {
            try
            {
              return new Point(int.Parse(values[0]), int.Parse(values[1]));
            }
            catch
            {
              //
            }
          }
        }
      }
      return defaultValue;
    }

    private Size GetXmlSize(XPathNodeIterator iterator, string tagName, Size defaultValue)
    {
      XPathNodeIterator i = iterator.Current.Select(tagName);
      if (i.MoveNext())
      {
        string s = i.Current.Value;
        if (s != null && s.Equals("-") == false)
        {
          string[] values = s.Split(',');
          if (values.Length == 2)
          {
            try
            {
              return new Size(int.Parse(values[0]), int.Parse(values[1]));
            }
            catch
            {
              //
            }
          }
        }
      }
      return defaultValue;
    }

    #endregion
  }
}