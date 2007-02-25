using System;
using ProjectInfinity.Logging;

namespace ProjectInfinity.Tests.Mocks
{
    /// <summary>
    /// A dummy <see cref="ILogger"/> implementation that does absolutely nothing.
    /// </summary>
    internal class NoLogger : ILogger
    {
        public LogLevel Level
        {
            get { return LogLevel.None; }
            set { }
        }

        public void Info(string format, params object[] args)
        {}

        public void Warn(string format, params object[] args)
        {}

        public void Debug(string format, params object[] args)
        {}

        public void Error(string format, params object[] args)
        {}

        public void Error(string format, Exception ex, params object[] args)
        {}

        public void Error(Exception ex)
        {}

        public void Critical(string format, params object[] args)
        {}
    }
}