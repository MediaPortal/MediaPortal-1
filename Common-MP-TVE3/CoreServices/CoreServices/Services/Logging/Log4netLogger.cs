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
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using log4net;

namespace MediaPortal.CoreServices
{
  public class Log4netLogger : ILogger
  {
    #region const variables
    const string _defaultConfig = @"<configuration>
  <configSections>
    <section name=""log4net"" type=""log4net.Config.Log4NetConfigurationSectionHandler, log4net""/>
  </configSections>

  <log4net>
    <appender name=""DefaultLogAppender"" type=""log4net.Appender.RollingFileAppender"">
      <file value=""[Name].log"" />
      <appendToFile value=""true"" />
      <rollingStyle value=""Once"" />
      <maxSizeRollBackups value=""4"" />
      <maximumFileSize value=""1MB"" />
      <staticLogFileName value=""true"" />
      <layout type=""log4net.Layout.PatternLayout"">
        <conversionPattern value=""[%date] [%-9thread] [%-5level] - %message%newline"" />
      </layout>
    </appender>

    <appender name=""ErrorLogAppender"" type=""log4net.Appender.RollingFileAppender"">
      <file value=""[Name]-Error.log"" />
      <appendToFile value=""true"" />
      <rollingStyle value=""Once"" />
      <maxSizeRollBackups value=""4"" />
      <maximumFileSize value=""1MB"" />
      <staticLogFileName value=""true"" />
      <threshold value=""ERROR"" />
      <layout type=""log4net.Layout.PatternLayout"">
        <conversionPattern value=""[%date] [%-9thread] [%-5level] - %message%newline"" />
      </layout>
    </appender>

     <appender name=""ConsoleAppender"" type=""log4net.Appender.ConsoleAppender"">
      <layout type=""log4net.Layout.PatternLayout"">
        <conversionPattern value=""[%date] [%-9thread] [%-5level] - %message%newline"" />
      </layout>
    </appender>

    <root>
      <level value=""ALL"" />
      <appender-ref ref=""ConsoleAppender"" />
      <appender-ref ref=""ErrorLogAppender"" />
      <appender-ref ref=""DefaultLogAppender"" />
    </root>
  </log4net>

</configuration>";
    #endregion

    #region variables
    protected LogLevel _minLevel = LogLevel.All;
    protected ILog _logger;
    #endregion

    #region Constructors/Destructors
    public Log4netLogger(string LoggerName, string DataPath)
    {
      string logPath = DataPath + "\\log";
      string logFile = Path.Combine(DataPath, "Log4Net.config");

      XmlDocument xmlDoc = new XmlDocument();
      if (File.Exists(logFile))
      {
        FileStream fs = new FileStream(Path.Combine(DataPath, "Log4Net.config"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        xmlDoc.Load(fs);
        fs.Close();
      }
      else
      {
        xmlDoc.LoadXml(_defaultConfig);
      }

      XmlNodeList nodeList = xmlDoc.SelectNodes("configuration/log4net/appender/file");
      foreach (XmlNode node in nodeList)
      {
        if (node.Attributes != null)
        {
          foreach (XmlAttribute attribute in node.Attributes)
          {
            if (attribute.Name.Equals("value"))
            {
              attribute.Value = Path.Combine(logPath, Path.GetFileName(attribute.Value)).Replace("[Name]", LoggerName);
              break;
            }
          }
        }
      }
      MemoryStream mStream = new MemoryStream();
      xmlDoc.Save(mStream);
      mStream.Seek(0, SeekOrigin.Begin);
      log4net.Config.XmlConfigurator.Configure(mStream);
      _logger = LogManager.GetLogger(LoggerName);
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
      LogManager.GetLogger("Epg").Info(FormatString(format, args));
    }

    /// <summary>
    /// Writes a debug message to the Log  
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Debug(string format, params object[] args)
    {
      if (_minLevel >= LogLevel.Debug)
      {
        _logger.Debug(FormatString(format, args));
      }
    }

    /// <summary>
    /// Writes an informational message to the Log
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Info(string format, params object[] args)
    {
      if (_minLevel >= LogLevel.Information)
      {
        _logger.Info(FormatString(format, args));
      }
    }

    /// <summary>
    /// Writes a warning message to the Log
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Warn(string format, params object[] args)
    {
      if (_minLevel >= LogLevel.Warning)
      {
        _logger.Warn(FormatString(format, args));
      }
    }

    /// <summary>
    /// Writes an error message to the Log
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Error(string format, params object[] args)
    {
      _logger.Error(FormatString(format, args));
    }

    /// <summary>
    /// Writes an Error <see cref="Exception"/> to the Log
    /// </summary>
    /// <param name="ex">The <see cref="Exception"/> to write.</param>
    public void Error(Exception ex)
    {
      _logger.Error(FormatException(ex));
    }

    /// <summary>
    /// Writes an error <see cref="Exception"/> to the Log
    /// </summary>
    /// <param name="message">A message string.</param>
    /// <param name="ex">The <see cref="Exception"/> to write.</param>
    public void Error(string message, Exception ex)
    {
      Error(message);
      Error(ex);
    }

    /// <summary>
    /// Writes a critical error system message to the Log
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Critical(string format, params object[] args)
    {
      if (_minLevel >= LogLevel.Critical)
      {
        _logger.Fatal(FormatString(format, args));
      }
    }


    /// <summary>
    /// Writes an Critical error <see cref="Exception"/> to the  Log
    /// </summary>
    /// <param name="ex">The <see cref="Exception"/> to write.</param>
    public void Critical(Exception ex)
    {
      if (_minLevel >= LogLevel.Critical)
      {
        _logger.Fatal(FormatException(ex));
      }
    }

    /// <summary>
    /// Writes an critical <see cref="Exception"/> to the  Log
    /// </summary>
    /// <param name="message">A message string.</param>
    /// <param name="ex">The <see cref="Exception"/> to write.</param>
    public void Critical(string message, Exception ex)
    {
      Critical(message);
      Critical(ex);
    }

    #endregion

    #region protected routines

    /// <summary>
    /// Writes an <see cref="Exception"/> instance.
    /// </summary>
    /// <param name="ex">The <see cref="Exception"/> to write.</param>
    protected string FormatException(Exception ex)
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendFormat("Exception: " + SafeString(ex.ToString()));
      sb.AppendFormat("  Message: " + SafeString(ex.Message));
      sb.AppendFormat("  Site   : " + ex.TargetSite);
      sb.AppendFormat("  Source : " + ex.Source);
      if (ex.InnerException != null)
      {
        sb.AppendFormat("  Inner Exception(s):");
        if (ex.InnerException != null)
        {
          Stack<Exception> stack = new Stack<Exception>();
          stack.Push(ex);
          while (stack.Count > 0)
          {
            Exception except = stack.Pop();
            sb.AppendFormat("  -> " + except.Message);
            if (except.InnerException != null)
            {
              stack.Push(except.InnerException);
            }
          }
        }
      }
      sb.AppendFormat("  Stack Trace:");
      sb.AppendFormat("  " + ex.StackTrace);
      return sb.ToString();
    }

    /// <summary>
    /// Replaces a password inside the string by stars
    /// </summary>
    /// <param name="logtext">String to replace</param>
    /// <returns>String without password</returns>
    protected string SafeString(String logtext)
    {
      return new Regex(@"Password=[^;]*;", RegexOptions.IgnoreCase).Replace(logtext, "Password=***;");
    }

    /// <summary>
    /// Check for valid args and return formated string
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    /// <returns>formated string</returns>
    protected string FormatString(string format, params object[] args)
    {
      string result = format;
      if (args != null && args.Length > 0)
      {
        try 
        {
          result = string.Format(format, args);
        }
        catch (Exception) {}
      }
      return result;
    }

    #endregion
  }
}
