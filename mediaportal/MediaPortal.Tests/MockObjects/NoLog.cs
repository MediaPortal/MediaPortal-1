using System;
using MediaPortal.Services;

namespace MediaPortal.Tests.MockObjects
{
 
    /// <summary>
    /// Dummy <see cref="ILog"/> service implementation that does absolutely nothing.
    /// </summary>
    public class NoLog : ILog   
    {
        public void BackupLogFiles()
        {
        }

        public void BackupLogFile(LogType logType)
        {
        }

        public void Write(Exception ex)
        {
        }

        /// <summary>
        /// Write a string to the logfile.
        /// </summary>
        /// <param name="format">The format of the string.</param>
        /// <param name="arg">An array containing the actual data of the string.</param>
        public void Write(string format, params object[] arg)
        {
        }

        public void Info(string format, params object[] arg)
        {
        }

        public void Info(LogType type, string format, params object[] arg)
        {
        }

        public void Warn(string format, params object[] arg)
        {
        }

        public void Warn(LogType type, string format, params object[] arg)
        {
        }

        public void Debug(string format, params object[] arg)
        {
        }

        public void Debug(LogType type, string format, params object[] arg)
        {
        }

        public void Error(string format, params object[] arg)
        {
        }

        public void Error(LogType type, string format, params object[] arg)
        {
        }

        public void Error(Exception ex)
        {
        }

        /// <summary>
        /// Write a string to the logfile.
        /// </summary>
        /// <param name="format">The format of the string.</param>
        /// <param name="arg">An array containing the actual data of the string.</param>
        public void WriteThreadId(string format, params object[] arg)
        {
        }

        public void WriteThreadId(LogType type, string format, params object[] arg)
        {
        }

        public void WriteFileThreadId(LogType type, bool isError, string format, params object[] arg)
        {
        }

        public void InfoThread(string format, params object[] arg)
        {
        }

        public void WarnThread(string format, params object[] arg)
        {
        }

        public void ErrorThread(string format, params object[] arg)
        {
        }

        public void SetConfigurationMode()
        {
        }

        public void WriteFile(LogType type, bool isError, string format, params object[] arg)
        {
        }

        public void WriteFile(LogType type, string format, params object[] arg)
        {
        }

        public void WriteFile(LogType type, Level logLevel, string format, params object[] arg)
        {
        }
    }
}
