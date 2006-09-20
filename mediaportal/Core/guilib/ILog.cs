using System;

namespace MediaPortal.GUI.Library
{
    public interface ILog
    {
        void BackupLogFiles();
        void BackupLogFile(Log.LogType logType);
        void Write(Exception ex);

        /// <summary>
        /// Write a string to the logfile.
        /// </summary>
        /// <param name="format">The format of the string.</param>
        /// <param name="arg">An array containing the actual data of the string.</param>
        void Write(string format, params object[] arg);

        void Info(string format, params object[] arg);
        void Info(Log.LogType type, string format, params object[] arg);
        void Warn(string format, params object[] arg);
        void Warn(Log.LogType type, string format, params object[] arg);
        void Debug(string format, params object[] arg);
        void Debug(Log.LogType type, string format, params object[] arg);
        void Error(string format, params object[] arg);
        void Error(Log.LogType type, string format, params object[] arg);
        void Error(Exception ex);

        /// <summary>
        /// Write a string to the logfile.
        /// </summary>
        /// <param name="format">The format of the string.</param>
        /// <param name="arg">An array containing the actual data of the string.</param>
        void WriteThreadId(string format, params object[] arg);

        void WriteThreadId(Log.LogType type, string format, params object[] arg);
        void WriteFileThreadId(Log.LogType type, bool isError, string format, params object[] arg);
        void InfoThread(string format, params object[] arg);
        void WarnThread(string format, params object[] arg);
        void ErrorThread(string format, params object[] arg);
        void SetConfigurationMode();
        void WriteFile(Log.LogType type, bool isError, string format, params object[] arg);
        void WriteFile(Log.LogType type, string format, params object[] arg);
        void WriteFile(Log.LogType type, Log.Level logLevel, string format, params object[] arg);
    }
}