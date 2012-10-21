using System;
using System.IO;
using System.Linq;
using System.Text;
using Castle.Core.Logging;
using Castle.MicroKernel.Registration;
using Castle.Windsor;

namespace MediaPortal.Common.Utils
{
    public class LogManager : ILogManager
    {
        private readonly ILogger _logger = NullLogger.Instance;        

        public LogManager(ILogger logger)
        {
            _logger = logger;
        }

        public LogManager()
        {
            //in case no logger is reg. we just use the null logger.
        }

        public LogManager(Type type)
        {            
            ILoggerFactory loggerFactory = WindsorService.Resolve<ILoggerFactory>();
            _logger = loggerFactory.Create(type);        
        }
        
        private ILogger Instance
        {
            get
            {
                return _logger;
            }
        }

        public void Debug(string message)
        {
            Instance.Debug(message);
        }

        public void Debug(Func<string> messageFactory)
        {
            Instance.Debug(messageFactory);
        }

        public void Debug(string message, Exception exception)
        {
            Instance.Debug(message, exception);
        }

        public void DebugFormat(string format, params object[] args)
        {
            Instance.DebugFormat(format, args);
        }

        public void DebugFormat(Exception exception, string format, params object[] args)
        {
            Instance.DebugFormat(exception, format, args);
        }

        public void DebugFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            Instance.DebugFormat(formatProvider, format, args);
        }

        public void DebugFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            Instance.DebugFormat(exception, formatProvider, format, args);
        }

        public void Error(string message)
        {
            Instance.Error(message);
        }

        public void Error(Func<string> messageFactory)
        {
            Instance.Error(messageFactory);
        }

        public void Error(string message, Exception exception)
        {
            Instance.Error(message, exception);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            Instance.ErrorFormat(format, args);
        }

        public void ErrorFormat(Exception exception, string format, params object[] args)
        {
            Instance.ErrorFormat(exception, format, args);
        }

        public void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            Instance.ErrorFormat(formatProvider, format, args);
        }

        public void ErrorFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            Instance.ErrorFormat(exception, formatProvider, format, args);
        }

        public void Fatal(string message)
        {
            Instance.Fatal(message);
        }

        public void Fatal(Func<string> messageFactory)
        {
            Instance.Fatal(messageFactory);
        }

        public void Fatal(string message, Exception exception)
        {
            Instance.Fatal(message, exception);
        }

        public void FatalFormat(string format, params object[] args)
        {
            Instance.FatalFormat(format, args);
        }

        public void FatalFormat(Exception exception, string format, params object[] args)
        {
            Instance.FatalFormat(exception, format, args);
        }

        public void FatalFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            Instance.FatalFormat(formatProvider, format, args);
        }

        public void FatalFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            Instance.FatalFormat(exception, formatProvider, format, args);
        }

        public void Info(string message)
        {
            Instance.Info(message);
        }

        public void Info(Func<string> messageFactory)
        {
            Instance.Info(messageFactory);
        }

        public void Info(string message, Exception exception)
        {
            Instance.Info(message, exception);
        }

        public void InfoFormat(string format, params object[] args)
        {
            Instance.InfoFormat(format, args);
        }

        public void InfoFormat(Exception exception, string format, params object[] args)
        {
            Instance.InfoFormat(exception, format, args);
        }

        public void InfoFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            Instance.InfoFormat(formatProvider, format, args);
        }

        public void InfoFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            Instance.InfoFormat(exception, formatProvider, format, args);
        }

        public void Warn(string message)
        {
            Instance.Warn(message);
        }

        public void Warn(Func<string> messageFactory)
        {
            Instance.Warn(messageFactory);
        }

        public void Warn(string message, Exception exception)
        {
            Instance.Warn(message, exception);
        }

        public void WarnFormat(string format, params object[] args)
        {
            Instance.WarnFormat(format, args);
        }

        public void WarnFormat(Exception exception, string format, params object[] args)
        {
            Instance.WarnFormat(exception, format, args);
        }

        public void WarnFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            Instance.WarnFormat(formatProvider, format, args);
        }

        public void WarnFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            Instance.WarnFormat(exception, formatProvider, format, args);
        }

        public bool IsDebugEnabled { get { return Instance.IsDebugEnabled; } }
        public bool IsErrorEnabled { get { return Instance.IsDebugEnabled; } }
        public bool IsFatalEnabled { get { return Instance.IsFatalEnabled; } }
        public bool IsInfoEnabled { get { return Instance.IsInfoEnabled; } }
        public bool IsWarnEnabled { get { return Instance.IsWarnEnabled; } }
    }

    public interface ILogManager
    {
        void Debug(string message);
        void Debug(Func<string> messageFactory);
        void Debug(string message, Exception exception);
        void DebugFormat(string format, params object[] args);
        void DebugFormat(Exception exception, string format, params object[] args);
        void DebugFormat(IFormatProvider formatProvider, string format, params object[] args);
        void DebugFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args);
        void Error(string message);
        void Error(Func<string> messageFactory);
        void Error(string message, Exception exception);
        void ErrorFormat(string format, params object[] args);
        void ErrorFormat(Exception exception, string format, params object[] args);
        void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args);
        void ErrorFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args);
        void Fatal(string message);
        void Fatal(Func<string> messageFactory);
        void Fatal(string message, Exception exception);
        void FatalFormat(string format, params object[] args);
        void FatalFormat(Exception exception, string format, params object[] args);
        void FatalFormat(IFormatProvider formatProvider, string format, params object[] args);
        void FatalFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args);
        void Info(string message);
        void Info(Func<string> messageFactory);
        void Info(string message, Exception exception);
        void InfoFormat(string format, params object[] args);
        void InfoFormat(Exception exception, string format, params object[] args);
        void InfoFormat(IFormatProvider formatProvider, string format, params object[] args);
        void InfoFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args);
        void Warn(string message);
        void Warn(Func<string> messageFactory);
        void Warn(string message, Exception exception);
        void WarnFormat(string format, params object[] args);
        void WarnFormat(Exception exception, string format, params object[] args);
        void WarnFormat(IFormatProvider formatProvider, string format, params object[] args);
        void WarnFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args);
        bool IsDebugEnabled { get; }
        bool IsErrorEnabled { get; }
        bool IsFatalEnabled { get; }
        bool IsInfoEnabled { get; }
        bool IsWarnEnabled { get; }
    }
}
