using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MPx86Proxy.Logging
{
    internal static class Log
    {
        private static FileStream _Fs;
        private static StreamWriter _Wr;

        private static System.Globalization.CultureInfo _CiEn = System.Globalization.CultureInfo.GetCultureInfo("en-US");

        public static LogLevelEnum LogLevel
        { get; private set; }

        public static string LogFile
        { get; private set; }

        static Log()
        {
            LogLevel = LogLevelEnum.Debug;
            LogFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +  @"\Team MediaPortal\MediaPortal\log\MPx86Proxy.log";
            _Fs = new FileStream(LogFile, FileMode.Create, FileAccess.Write);
            _Wr = new StreamWriter(_Fs);
        }

        public static void Error(string strMessage)
        {
            if (LogLevel <= LogLevelEnum.Error)
                append("Error", strMessage);
        }
        public static void Error(string strMessage, params object[] args)
        {
            if (LogLevel <= LogLevelEnum.Error)
                append("Error", string.Format(strMessage, args));
        }

        public static void Warning(string strMessage)
        {
            if (LogLevel <= LogLevelEnum.Warning)
                append("Warning", strMessage);
        }
        public static void Warning(string strMessage, params object[] args)
        {
            if (LogLevel <= LogLevelEnum.Warning)
                append("Warning", string.Format(strMessage, args));
        }

        public static void Info(string strMessage)
        {
            if (LogLevel <= LogLevelEnum.Info)
                append("Info", strMessage);
        }
        public static void Info(string strMessage, params object[] args)
        {
            if (LogLevel <= LogLevelEnum.Info)
                append("Info", string.Format(strMessage, args));
        }

        public static void Debug(string strMessage)
        {
            if (LogLevel <= LogLevelEnum.Debug)
                append("Debug", strMessage);
        }
        public static void Debug(string strMessage, params object[] args)
        {
            if (LogLevel <= LogLevelEnum.Debug)
                append("Debug", string.Format(strMessage, args));
        }

        public static void Trace(string strMessage)
        {
            if (LogLevel <= LogLevelEnum.Trace)
                append("Trace", strMessage);
        }
        public static void Trace(string strMessage, params object[] args)
        {
            if (LogLevel <= LogLevelEnum.Trace)
                append("Trace", string.Format(strMessage, args));
        }

        private static void append(string strLevel, string strMessage)
        {
            lock (_Wr)
            {
                _Wr.Write(DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss.fff ", _CiEn));
                _Wr.WriteLine("{0,7} {1}", strLevel, strMessage);
                _Wr.Flush();
            }
        }
    }

}
