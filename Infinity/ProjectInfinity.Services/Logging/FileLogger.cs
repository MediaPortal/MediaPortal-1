using System;
using System.IO;
using System.Threading;

namespace ProjectInfinity.Logging
{
  /// <summary>
  /// An <see cref="ILogger"/> implementation that writes messages to a text file.
  /// </summary>
  /// <remarks>If the text file exists it will be truncated.</remarks>
  public class FileLogger : ILogger
  {
    private LogLevel level; //holds the treshold for the log level.
    private string fileName; //holds the file to write to.
    private static object syncObject = new object();

    /// <summary>
    /// Creates a new <see cref="FileLogger"/> instance and initializes it with the given filename and <see cref="LogLevel"/>.
    /// </summary>
    /// <param name="fileName">The full path of the file to write the messages to.</param>
    /// <param name="level">The minimum level messages must have to be written to the file.</param>
    public FileLogger(string fileName, LogLevel level)
    {
      this.fileName = fileName;
      this.level = level;
      if (level > LogLevel.None)
      {
        using (new StreamWriter(fileName, false))
        {
        }
      }
    }

    /// <summary>
    /// Gets or sets the log level.
    /// </summary>
    /// <value>A <see cref="LogLevel"/> value that indicates the minimum level messages must have to be 
    /// written to the file.</value>
    public LogLevel Level
    {
      get { return level; }
      set { level = value; }
    }

    /// <summary>
    /// Writes an informational message to the log.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Info(string format, params object[] args)
    {
      Write(string.Format(format, args), LogLevel.Information);
    }

    /// <summary>
    /// Writes a warning to the log.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Warn(string format, params object[] args)
    {
      Write(string.Format(format, args), LogLevel.Warning);
    }

    /// <summary>
    /// Writes a debug message to the log.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Debug(string format, params object[] args)
    {
      Write(string.Format(format, args), LogLevel.Debug);
    }

    /// <summary>
    /// Writes an error message to the log.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Error(string format, params object[] args)
    {
      Write(string.Format(format, args), LogLevel.Error);
    }

    /// <summary>
    /// Writes an error message to the log, passing the original <see cref="Exception"/>.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="ex">The <see cref="Exception"/> that caused the message.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Error(string format, Exception ex, params object[] args)
    {
      Write(string.Format(format, args), LogLevel.Error);
      Error(ex);
    }

    /// <summary>
    /// Writes an <see cref="Exception"/> to the log.
    /// </summary>
    /// <param name="ex">The <see cref="Exception"/> to write.</param>
    public void Error(Exception ex)
    {
      if (level >= LogLevel.Error)
      {
        WriteException(ex);
      }
    }

    /// <summary>
    /// Writes a critical system message to the log.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Critical(string format, params object[] args)
    {
      Write(string.Format(format, args), LogLevel.Critical);
    }

    /// <summary>
    /// Does the actual writing of the message to the file.
    /// </summary>
    /// <param name="message"></param>
    private void Write(string message)
    {
      Monitor.Enter(syncObject);
      try
      {
        using (StreamWriter writer = new StreamWriter(fileName, true))
        {
          writer.WriteLine(message);
        }
      }
      finally
      {
        Monitor.Exit(syncObject);
      }
    }

    /// <summary>
    /// Does the actual writing of the message to the file.
    /// </summary>
    /// <param name="message">The message to write</param>
    /// <param name="messageLevel">The <see cref="LogLevel"/> of the message to write</param>
    private void Write(string message, LogLevel messageLevel)
    {
      if (messageLevel > level)
      {
        return;
      }
      Monitor.Enter(syncObject);
      try
      {
        using (StreamWriter writer = new StreamWriter(fileName, true))
        {
          string thread = Thread.CurrentThread.Name;
          if (thread == null)
          {
            thread = Thread.CurrentThread.ManagedThreadId.ToString();
          }
          writer.WriteLine("{0:yyyy-MM-dd HH:mm:ss.ffffff} [{1}][{2}]: {3}", DateTime.Now, messageLevel, thread, message);
        }
      }
      finally
      {
        Monitor.Exit(syncObject);
      }
    }

    /// <summary>
    /// Writes an <see cref="Exception"/> instance to the file.
    /// </summary>
    /// <param name="ex">The <see cref="Exception"/> to write.</param>
    private void WriteException(Exception ex)
    {
      Write("Exception: " + ex);
      Write("  Message: " + ex.Message);
      Write("  Site   : " + ex.TargetSite);
      Write("  Source : " + ex.Source);
      if (ex.InnerException != null)
      {
        Write("Inner Exception(s):");
        WriteInnerException(ex.InnerException);
      }
      Write("Stack Trace:");
      Write(ex.StackTrace);
    }

    /// <summary>
    /// Writes any existing inner exceptions to the file.
    /// </summary>
    /// <param name="exception"></param>
    private void WriteInnerException(Exception exception)
    {
      if (exception == null)
      {
        throw new ArgumentNullException("exception");
      }
      Write(exception.Message);
      if (exception.InnerException != null)
      {
        WriteInnerException(exception);
      }
    }
  }
}