using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using log4net;

namespace MediaPortal.Common.Utils.Logger
{
  public class CommonLog4NetLogger: ICommonLogger
  {

    #region variables
    protected CommonLogLevel MinLevel = CommonLogLevel.All;
    protected Dictionary<CommonLogType, ILog> _loggers;
    #endregion

    #region Constructors/Destructors
    public CommonLog4NetLogger(string loggerName, string dataPath, string logPath)
    {
      var logFile = Path.Combine(dataPath, "Log4Net.config");
      if (!File.Exists(logFile)) 
        WriteDefaultConfig(logFile);

      var xmlDoc = new XmlDocument();
      var fs = new FileStream(Path.Combine(dataPath, "Log4Net.config"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
      xmlDoc.Load(fs);
      fs.Close();
      
      var nodeList = xmlDoc.SelectNodes("configuration/log4net/appender/file");
      if (nodeList != null)
        foreach (var node in nodeList.Cast<XmlNode>().Where(node => node.Attributes != null)) {
          if (node != null)
            if (node.Attributes != null)
              foreach (var attribute in node.Attributes.Cast<XmlAttribute>().Where(attribute => attribute.Name.Equals("value")))
              {
                if (attribute.Value != null)
                  attribute.Value = Path.Combine(logPath, Path.GetFileName(attribute.Value)).Replace("[Name]", loggerName);
                break;
              }
        }
      var mStream = new MemoryStream();
      xmlDoc.Save(mStream);
      mStream.Seek(0, SeekOrigin.Begin);
      log4net.Config.XmlConfigurator.Configure(mStream);

      _loggers = new Dictionary<CommonLogType, ILog>();
      foreach (CommonLogType logType in Enum.GetValues(typeof(CommonLogType)))
      {
        _loggers.Add(logType, LogManager.GetLogger(logType.ToString()));
      }
    }

    #endregion

    #region WriteDefaultConfig
    private void WriteDefaultConfig(string logFile)
    {
      TextWriter tw = new StreamWriter(logFile);
      tw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
      tw.WriteLine("<configuration>)");
      tw.WriteLine("  <configSections>");
      tw.WriteLine("    <section name=\"log4net\" type=\"log4net.Config.Log4NetConfigurationSectionHandler, log4net\"/>");
      tw.WriteLine("  </configSections>");
      tw.WriteLine(" ");
      tw.WriteLine("  <log4net>");
      tw.WriteLine("    <appender name=\"DefaultLogAppender\" type=\"log4net.Appender.RollingFileAppender\">");
      tw.WriteLine("      <file value=\"[Name].log\" />");
      tw.WriteLine("      <encoding type=\"System.Text.UTF8Encoding\" />");
      tw.WriteLine("      <appendToFile value=\"true\" />");
      tw.WriteLine("      <lockingModel type=\"log4net.Appender.FileAppender+MinimalLock\" />");
      tw.WriteLine("      <rollingStyle value=\"Size\" />");
      tw.WriteLine("      <maxSizeRollBackups value=\"5\" />");
      tw.WriteLine("      <maximumFileSize value=\"5MB\" />");
      tw.WriteLine("      <staticLogFileName value=\"true\" />");
      tw.WriteLine("      <PreserveLogFileNameExtension value=\"true\" />");
      tw.WriteLine("      <layout type=\"log4net.Layout.PatternLayout\">");
      tw.WriteLine("        <conversionPattern value=\"[%date] [%-7logger] [%-9thread] [%-5level] - %message%newline\" />");
      tw.WriteLine("      </layout>");
      tw.WriteLine("    </appender>");
      tw.WriteLine(" ");
      tw.WriteLine("    <appender name=\"ErrorLogAppender\" type=\"log4net.Appender.RollingFileAppender\">");
      tw.WriteLine("      <file value=\"[Name]-Error.log\" />");
      tw.WriteLine("      <encoding type=\"System.Text.UTF8Encoding\" />");
      tw.WriteLine("      <appendToFile value=\"true\" />");
      tw.WriteLine("      <lockingModel type=\"log4net.Appender.FileAppender+MinimalLock\" />");
      tw.WriteLine("      <rollingStyle value=\"Size\" />");
      tw.WriteLine("      <maxSizeRollBackups value=\"5\" />");
      tw.WriteLine("      <maximumFileSize value=\"5MB\" />");
      tw.WriteLine("      <staticLogFileName value=\"true\" />");
      tw.WriteLine("      <PreserveLogFileNameExtension value=\"true\" />");
      tw.WriteLine("      <layout type=\"log4net.Layout.PatternLayout\">");
      tw.WriteLine("        <conversionPattern value=\"[%date] [%-7logger] [%-9thread] [%-5level] - %message%newline\" />");
      tw.WriteLine("      </layout>");
      tw.WriteLine("    </appender>");
      tw.WriteLine(" ");
      tw.WriteLine("    <appender name=\"ErrorLossyFileAppender\" type=\"log4net.Appender.BufferingForwardingAppender\">");
      tw.WriteLine("      <bufferSize value=\"1\" />");
      tw.WriteLine("      <encoding type=\"System.Text.UTF8Encoding\" />");
      tw.WriteLine("      <lossy value=\"true\"/>");
      tw.WriteLine("      <evaluator type=\"log4net.Core.LevelEvaluator\">");
      tw.WriteLine("      <threshold value=\"ERROR\" />");
      tw.WriteLine("      </evaluator>");
      tw.WriteLine("      <lockingModel type=\"log4net.Appender.FileAppender+MinimalLock\" />");
      tw.WriteLine("      <appender-ref ref=\"ErrorLogAppender\" />");
      tw.WriteLine("    </appender>");
      tw.WriteLine(" ");
      tw.WriteLine("    <appender name=\"ConsoleAppender\" type=\"log4net.Appender.ConsoleAppender\">");
      tw.WriteLine("      <encoding type=\"System.Text.UTF8Encoding\" />");
      tw.WriteLine("      <layout type=\"log4net.Layout.PatternLayout\">");
      tw.WriteLine("      <lockingModel type=\"log4net.Appender.FileAppender+MinimalLock\" />");
      tw.WriteLine("        <conversionPattern value=\"[%date] [%-7logger] [%-9thread] [%-5level] - %message%newline\" />");
      tw.WriteLine("      </layout>");
      tw.WriteLine("    </appender>");
      tw.WriteLine(" ");
      tw.WriteLine("    <root>");
      tw.WriteLine("      <level value=\"ALL\" />");
      tw.WriteLine("      <appender-ref ref=\"ConsoleAppender\" />");
      tw.WriteLine("      <appender-ref ref=\"ErrorLossyFileAppender\" />");
      tw.WriteLine("      <appender-ref ref=\"DefaultLogAppender\" />");
      tw.WriteLine("    </root>");
      tw.WriteLine("  </log4net>");
      tw.WriteLine(" ");
      tw.WriteLine("</configuration>");
      tw.Flush();
      tw.Close();
    }

    #endregion

    #region Implementation of ICommonLogger

    /// <summary>
    /// Gets or sets the log level.
    /// </summary>
    /// <value>A <see cref="CommonLogLevel"/> value that indicates the minimum level messages must have to be 
    /// written to the logger.</value>
    public CommonLogLevel LogLevel
    {
      get { return MinLevel; }
      set { MinLevel = value; }
    }


    /// <summary>
    /// Writes a debug message to the Log  
    /// </summary>
    /// <param name="logType">Defines log4net logger</param>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Debug(CommonLogType logType, string format, params object[] args)
    {
      if (MinLevel >= CommonLogLevel.Debug)
      {
        _loggers[logType].Debug(FormatString(format, args));
      }
    }

    /// <summary>
    /// Writes an informational message to the Log
    /// </summary>
    /// <param name="logType">Defines log4net logger</param>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Info(CommonLogType logType, string format, params object[] args)
    {
      if (MinLevel >= CommonLogLevel.Information)
      {
        _loggers[logType].Info(FormatString(format, args));
      }
    }

    /// <summary>
    /// Writes a warning message to the Log
    /// </summary>
    /// <param name="logType">Defines log4net logger</param>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Warn(CommonLogType logType, string format, params object[] args)
    {
      if (MinLevel >= CommonLogLevel.Warning)
      {
        _loggers[logType].Warn(FormatString(format, args));
      }
    }

    /// <summary>
    /// Writes an error message to the Log
    /// </summary>
    /// <param name="logType">Defines log4net logger</param>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Error(CommonLogType logType, string format, params object[] args)
    {
      _loggers[logType].Error(FormatString(format, args));
    }

    /// <summary>
    /// Writes an Error <see cref="Exception"/> to the Log
    /// </summary>
    /// <param name="logType">Defines log4net logger</param>
    /// <param name="ex">The <see cref="Exception"/> to write.</param>
    public void Error(CommonLogType logType, Exception ex)
    {
      _loggers[logType].Error(FormatException(ex));
    }

    /// <summary>
    /// Writes an error <see cref="Exception"/> to the Log
    /// </summary>
    /// <param name="logType">Defines log4net logger</param>
    /// <param name="message">A message string.</param>
    /// <param name="ex">The <see cref="Exception"/> to write.</param>
    public void Error(CommonLogType logType, string message, Exception ex)
    {
      Error(logType, message);
      Error(logType, ex);
    }

    /// <summary>
    /// Writes a critical error system message to the Log
    /// </summary>
    /// <param name="logType">Defines log4net logger</param>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Critical(CommonLogType logType, string format, params object[] args)
    {
      if (MinLevel >= CommonLogLevel.Critical)
      {
        _loggers[logType].Fatal(FormatString(format, args));
      }
    }


    /// <summary>
    /// Writes an Critical error <see cref="Exception"/> to the  Log
    /// </summary>
    /// <param name="logType">Defines log4net logger</param>
    /// <param name="ex">The <see cref="Exception"/> to write.</param>
    public void Critical(CommonLogType logType, Exception ex)
    {
      if (MinLevel >= CommonLogLevel.Critical)
      {
        _loggers[logType].Fatal(FormatException(ex));
      }
    }

    /// <summary>
    /// Writes an critical <see cref="Exception"/> to the  Log
    /// </summary>
    /// <param name="logType">Defines log4net logger</param>
    /// <param name="message">A message string.</param>
    /// <param name="ex">The <see cref="Exception"/> to write.</param>
    public void Critical(CommonLogType logType, string message, Exception ex)
    {
      Critical(logType, message);
      Critical(logType, ex);
    }

    #endregion

    #region protected routines

    /// <summary>
    /// Writes an <see cref="Exception"/> instance.
    /// </summary>
    /// <param name="ex">The <see cref="Exception"/> to write.</param>
    protected string FormatException(Exception ex)
    {
      var sb = new StringBuilder();
      sb.AppendFormat("Exception: " + SafeString(ex.ToString()));
      sb.AppendFormat("  Message: " + SafeString(ex.Message));
      sb.AppendFormat("  Site   : " + ex.TargetSite);
      sb.AppendFormat("  Source : " + ex.Source);
      if (ex.InnerException != null)
      {
        sb.AppendFormat("  Inner Exception(s):");
        {
          var stack = new Stack<Exception>();
          stack.Push(ex);
          while (stack.Count > 0)
          {
            var except = stack.Pop();
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
      var result = format;
      if (args != null && args.Length > 0)
      {
        try 
        {
          result = string.Format(format, args);
        }
        catch {}
      }
      return result;
    }

    #endregion

  }
}
