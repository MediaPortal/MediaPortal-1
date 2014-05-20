using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MediaPortal.Common.Utils.Logger
{
  public interface ICommonLogger
  {
    /// <summary>
    /// Gets or sets the log level.
    /// </summary>
    /// <value>A <see cref="CommonLogLevel"/> value that indicates the minimum level messages must have to be 
    /// written to the logger.</value>
    CommonLogLevel LogLevel { get; set; }

    /// <summary>
    /// Sets the log level for the log type.
    /// </summary>
    /// <param name="logType"></param>
    /// <param name="logLevel"></param>
    void SetLogLevel(CommonLogType logType, CommonLogLevel logLevel);

    /// <summary>
    /// Writes a debug message to the Log  
    /// </summary>
    /// <param name="logType">Defines log4net logger</param>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    void Debug(CommonLogType logType, string format, params object[] args);

    /// <summary>
    /// Writes an informational message to the Log
    /// </summary>
    /// <param name="logType">Defines log4net logger</param>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    void Info(CommonLogType logType, string format, params object[] args);

    /// <summary>
    /// Writes a warning message to the Log
    /// </summary>
    /// <param name="logType">Defines log4net logger</param>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    void Warn(CommonLogType logType, string format, params object[] args);

    /// <summary>
    /// Writes an error message to the Log
    /// </summary>
    /// <param name="logType">Defines log4net logger</param>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    void Error(CommonLogType logType, string format, params object[] args);

    /// <summary>
    /// Writes an Error <see cref="Exception"/> to the Log
    /// </summary>
    /// <param name="logType">Defines log4net logger</param>
    /// <param name="ex">The <see cref="Exception"/> to write.</param>
    void Error(CommonLogType logType, Exception ex);

    /// <summary>
    /// Writes an error <see cref="Exception"/> to the Log
    /// </summary>
    /// <param name="logType">Defines log4net logger</param>
    /// <param name="message">A message string.</param>
    /// <param name="ex">The <see cref="Exception"/> to write.</param>
    void Error(CommonLogType logType, string message, Exception ex);

    /// <summary>
    /// Writes a critical error system message to the Log
    /// </summary>
    /// <param name="logType">Defines log4net logger</param>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    void Critical(CommonLogType logType, string format, params object[] args);

    /// <summary>
    /// Writes an Critical error <see cref="Exception"/> to the  Log
    /// </summary>
    /// <param name="logType">Defines log4net logger</param>
    /// <param name="ex">The <see cref="Exception"/> to write.</param>
    void Critical(CommonLogType logType, Exception ex);

    /// <summary>
    /// Writes an critical <see cref="Exception"/> to the  Log
    /// </summary>
    /// <param name="logType">Defines log4net logger</param>
    /// <param name="message">A message string.</param>
    /// <param name="ex">The <see cref="Exception"/> to write.</param>
    void Critical(CommonLogType logType, string message, Exception ex);
  }
}
