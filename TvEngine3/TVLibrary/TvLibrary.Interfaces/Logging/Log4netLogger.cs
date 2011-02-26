#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using log4net;
using TvLibrary.Interfaces;

namespace TvLibrary.Log
{
  public class Log4netLogger : ILogger
  {
    #region variables
    protected LogLevel _minLevel = LogLevel.All;
    protected ILog _logger = LogManager.GetLogger("Log");
    #endregion

    #region Constructors/Destructors
    public Log4netLogger(string ConfigName)
    {
      string logPath = PathManager.GetDataPath;
      string appPath = logPath;
      logPath = logPath + "\\log";

      XmlDocument xmlDoc = new XmlDocument();
      FileStream fs = new FileStream(Path.Combine(appPath, ConfigName), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
      xmlDoc.Load(fs);
      fs.Close();

      XmlNodeList nodeList = xmlDoc.SelectNodes("configuration/log4net/appender/file");
      foreach (XmlNode node in nodeList)
      {
        if (node.Attributes != null)
        {
          foreach (XmlAttribute attribute in node.Attributes)
          {
            if (attribute.Name.Equals("value"))
            {
              attribute.Value = Path.Combine(logPath, Path.GetFileName(attribute.Value));
              break;
            }
          }
        }
      }
      MemoryStream mStream = new MemoryStream();
      xmlDoc.Save(mStream);
      mStream.Seek(0, SeekOrigin.Begin);
      log4net.Config.XmlConfigurator.Configure(mStream);
    }
    #endregion

    #region Implementation of ILogger

    /// <summary>
    /// Gets or sets the log level.
    /// </summary>
    /// <value>A <see cref="LogLevel"/> value that indicates the minimum level messages must have to be 
    /// written to the logger.</value>
    public LogLevel Level
    {
      get { return _minLevel; }
      set { _minLevel = value; }
    }

    public void Epg(string format, params object[] args)
    {
      LogManager.GetLogger("Epg").InfoFormat(format, args);
    }

    /// <summary>
    /// Writes a debug message to the log.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Debug(string format, params object[] args)
    {
      if (_minLevel >= LogLevel.Debug)
      {
        _logger.DebugFormat(format, args);
      }
    }

    /// <summary>
    /// Writes a debug message to the log.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="ex">The <see cref="Exception"/> that caused the message.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Debug(string format, Exception ex, params object[] args)
    {
      if (_minLevel >= LogLevel.Debug)
      {
        _logger.Debug(string.Format(format, args), ex);
      }
    }

    /// <summary>
    /// Writes an informational message to the log.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Info(string format, params object[] args)
    {
      if (_minLevel >= LogLevel.Information)
      {
        _logger.InfoFormat(format, args);
      }
    }

    /// <summary>
    /// Writes an informational message to the log.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="ex">The <see cref="Exception"/> that caused the message.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Info(string format, Exception ex, params object[] args)
    {
      if (_minLevel >= LogLevel.Information)
      {
        _logger.Info(string.Format(format, args), ex);
      }
    }

    /// <summary>
    /// Writes a warning to the log.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Warn(string format, params object[] args)
    {
      if (_minLevel >= LogLevel.Warning)
      {
        _logger.WarnFormat(format, args);
      }
    }

    /// <summary>
    /// Writes a warning to the log, passing the original <see cref="Exception"/>.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="ex">The <see cref="Exception"/> that caused the message.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Warn(string format, Exception ex, params object[] args)
    {
      if (_minLevel >= LogLevel.Warning)
      {
        _logger.Warn(string.Format(format, args), ex);
      }
    }

    /// <summary>
    /// Writes an error message to the log.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Error(string format, params object[] args)
    {
      _logger.ErrorFormat(format, args);
    }

    /// <summary>
    /// Writes an error message to the log, passing the original <see cref="Exception"/>.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="ex">The <see cref="Exception"/> that caused the message.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Error(string format, Exception ex, params object[] args)
    {
       _logger.Error(string.Format(format, args), ex);
    }

    /// <summary>
    /// Writes an Error <see cref="Exception"/> to the log.
    /// </summary>
    /// <param name="ex">The <see cref="Exception"/> to write.</param>
    public void Error(Exception ex)
    {
      _logger.Error("", ex);
    }

    public void Write(Exception ex)
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendFormat("Exception   :{0}\n", ex);
      Error(SafeString(sb.ToString()));
    }

    public void Write(string format, params object[] arg)
    {
      Info(format, arg);
    }

    public void WriteThreadId(string format, params object[] arg)
    {
      Info(format, arg);
    }

    public void WriteFile(string format, params object[] arg)
    {
      Info(format, arg);
    }

    public void WriteFile(string format, Exception ex)
    {
      Error(format, ex);
    }

    /// <summary>
    /// Writes a critical error system message to the log.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Critical(string format, params object[] args)
    {
      if (_minLevel >= LogLevel.Critical)
      {
        _logger.FatalFormat(format, args);
      }
    }

    /// <summary>
    /// Writes a critical error system message to the log, passing the original <see cref="Exception"/>.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="ex">The <see cref="Exception"/> that caused the message.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Critical(string format, Exception ex, params object[] args)
    {
      if (_minLevel >= LogLevel.Critical)
      {
        _logger.Fatal(string.Format(format, args), ex);
      }
    }

    /// <summary>
    /// Writes an Critical error <see cref="Exception"/> to the log.
    /// </summary>
    /// <param name="ex">The <see cref="Exception"/> to write.</param>
    public void Critical(Exception ex)
    {
      if (_minLevel >= LogLevel.Critical)
      {
        _logger.Fatal("", ex);
      }
    }

    /// <summary>
    /// Replaces a password inside the string by stars
    /// </summary>
    /// <param name="Logtext">String to replace</param>
    /// <returns>String without password</returns>
    public String SafeString(String Logtext)
    {
      return new Regex(@"Password=[^;]*;", RegexOptions.IgnoreCase).Replace(Logtext, "Password=***;");
    }

    #endregion
  }
}
