using System;
using System.Collections.Generic;
using MediaPortal.Common.Utils;
using Mediaportal.TV.Common.IntegrationProviderInterfaces;

namespace Mediaportal.TV.Common.Logging
{
  public class LogProvider
  {
    #region static usage

    //private static LogProvider _logProvíder;

    private static readonly IDictionary<Type, LogProvider> _logProviderCache = new Dictionary<Type, LogProvider>();
    private static readonly object _LogProviderLock = new object();


    public static LogProvider Log(Type type)
    {
      LogProvider logger;
      lock (_LogProviderLock)
      {
        bool hasLogger = _logProviderCache.TryGetValue(type, out logger);
        if (!hasLogger)
        {
          logger = new LogProvider(type);
          _logProviderCache[type] = logger;
        }
      }
      return logger;
    }

    public static LogProvider Log()
    {
      return Log(typeof(LogProvider));
    }

    #endregion

    private readonly Type _type;
    private readonly  ILogger _logger;
    public LogProvider()
    {
      _type = GetType();
      _logger = GlobalServiceProvider.Instance.Get<IIntegrationProvider>().Logger;
    }

    public LogProvider(Type caller)
    {
      _type = caller;
      _logger = GlobalServiceProvider.Instance.Get<IIntegrationProvider>().Logger;
    }

    /// <summary>
    /// Writes a debug message to the log.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Debug(string format, params object[] args)
    {
      _logger.Info(_type, format, args);
    }

    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="ex">The <see cref="Exception"/> that caused the message.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Debug(string format, Exception ex, params object[] args)
    {
      _logger.Debug(_type, format, ex, args);
    }
    /// <summary>
    /// Writes an informational message to the log.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Info(string format, params object[] args)
    {
      _logger.Info(_type, format, args);
    }
    /// <summary>
    /// Writes an informational message to the log.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="ex">The <see cref="Exception"/> that caused the message.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Info(string format, Exception ex, params object[] args)
    {
      _logger.Info(_type, format, ex, args);
    }
    /// <summary>
    /// Writes a warning to the log.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Warn(string format, params object[] args)
    {
      _logger.Warn(_type, format, args);
    }
    /// <summary>
    /// Writes a warning to the log, passing the original <see cref="Exception"/>.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="ex">The <see cref="Exception"/> that caused the message.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Warn(string format, Exception ex, params object[] args)
    {
      _logger.Warn(_type, format, ex, args);
    }
    /// <summary>
    /// Writes an error message to the log.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Error(string format, params object[] args)
    {
     _logger.Error(_type, format, args); 
    }
    /// <summary>
    /// Writes an error message to the log, passing the original <see cref="Exception"/>.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="ex">The <see cref="Exception"/> that caused the message.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Error(string format, Exception ex, params object[] args)
    {
      _logger.Error(_type, format, ex, args); 
    }
    /// <summary>
    /// Writes an Error <see cref="Exception"/> to the log.
    /// </summary>
    /// <param name="ex">The <see cref="Exception"/> to write.</param>
    public void Error(Exception ex)
    {
      _logger.Error(_type, ex); 
    }
    /// <summary>
    /// Writes a critical error system message to the log.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Critical(string format, params object[] args)
    {
      _logger.Critical(_type, format, args);
    }
    /// <summary>
    /// Writes a critical error system message to the log, passing the original <see cref="Exception"/>.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="ex">The <see cref="Exception"/> that caused the message.</param>
    /// <param name="args">An array of objects to write using format.</param>
    public void Critical(string format, Exception ex, params object[] args)
    {
      _logger.Critical(_type, format, ex, args);
    }
    /// <summary>
    /// Writes an Critical error <see cref="Exception"/> to the log.
    /// </summary>
    /// <param name="ex">The <see cref="Exception"/> to write.</param>
    public void Critical(Exception ex)
    {
      _logger.Critical(_type, ex);
    }
  }
}
