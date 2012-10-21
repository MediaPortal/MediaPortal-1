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
using System.IO;
using System.Reflection;
using System.Xml;
using System.Windows.Forms;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Integration;
using log4net;

namespace TvLibrary.Integration.MP1
{
  public class Log4NetLogger:ILogger
  {
    /// <summary>
    /// Creates a new <see cref="Log4NetLogger"/> instance and initializes it with the given parameters.
    /// </summary>
    /// <param name="logPath">Path where the logfiles should be written to.</param>
    public Log4NetLogger(string logPath)
    {
      XmlDocument xmlDoc = new XmlDocument();
      using (Stream stream = new FileStream(Application.ExecutablePath + ".config", FileMode.Open, FileAccess.Read))
        xmlDoc.Load(stream);
      XmlNodeList nodeList = xmlDoc.SelectNodes("configuration/log4net/appender/file");
      foreach (XmlNode node in nodeList)
        if (node.Attributes != null)
          foreach (XmlAttribute attribute in node.Attributes)
            if (attribute.Name.Equals("value"))
            {
              attribute.Value = Path.Combine(logPath, Path.GetFileName(attribute.Value));
              break;
            }

      using (MemoryStream stream = new MemoryStream())
      {
        xmlDoc.Save(stream);
        stream.Seek(0, SeekOrigin.Begin);
        log4net.Config.XmlConfigurator.Configure(stream);
      }
    }

    protected ILog GetLogger
    {
      get { return LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType); }
    }
    /// <summary>
    /// Writes a debug message to the log.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Debug (string format, params object[] args)
    {
      GetLogger.DebugFormat(format, args);
    }

    /// <summary>
    /// Writes a debug message to the log.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="ex">The <see cref="Exception"/> that caused the message.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Debug (string format, Exception ex, params object[] args)
    {
      GetLogger.Debug(string.Format(format, args), ex);
    }

    /// <summary>
    /// Writes an informational message to the log.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Info (string format, params object[] args)
    {
      GetLogger.InfoFormat(format, args);
    }

    /// <summary>
    /// Writes an informational message to the log.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="ex">The <see cref="Exception"/> that caused the message.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Info (string format, Exception ex, params object[] args)
    {
      GetLogger.Info(string.Format(format, args), ex);
    }

    /// <summary>
    /// Writes a warning to the log.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Warn (string format, params object[] args)
    {
      GetLogger.WarnFormat(format, args);
    }

    /// <summary>
    /// Writes a warning to the log, passing the original <see cref="Exception"/>.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="ex">The <see cref="Exception"/> that caused the message.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Warn (string format, Exception ex, params object[] args)
    {
      GetLogger.Warn(string.Format(format, args), ex);
    }

    /// <summary>
    /// Writes an error message to the log.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Error (string format, params object[] args)
    {
      GetLogger.ErrorFormat(format, args);
    }

    /// <summary>
    /// Writes an error message to the log, passing the original <see cref="Exception"/>.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="ex">The <see cref="Exception"/> that caused the message.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Error (string format, Exception ex, params object[] args)
    {
      GetLogger.Error(string.Format(format, args), ex);
    }

    /// <summary>
    /// Writes an Error <see cref="Exception"/> to the log.
    /// </summary>
    /// <param name="ex">The <see cref="Exception"/> to write.</param>
    public void Error (Exception ex)
    {
      GetLogger.Error("", ex);
    }

    /// <summary>
    /// Writes a critical error system message to the log.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Critical (string format, params object[] args)
    {
      GetLogger.FatalFormat(format, args);
    }

    /// <summary>
    /// Writes a critical error system message to the log, passing the original <see cref="Exception"/>.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="ex">The <see cref="Exception"/> that caused the message.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Critical (string format, Exception ex, params object[] args)
    {
      GetLogger.Fatal(string.Format(format, args), ex);
    }

    /// <summary>
    /// Writes an Critical error <see cref="Exception"/> to the log.
    /// </summary>
    /// <param name="ex">The <see cref="Exception"/> to write.</param>
    public void Critical (Exception ex)
    {
      GetLogger.Fatal("", ex);
    }
  }
}
