#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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

//using System.IO;
//using MediaPortal.Utils.Services;
//using NUnit.Framework;
////Disabled until log is reworked

//namespace MediaPortal.Tests.Utils.Services
//{
//  [TestFixture]
//  [Category("Log")]
//  public class LogTest
//  {
//    [Test]
//    public void TestLogMessage()
//    {
//      StringWriter logString = new StringWriter();
//      using (NewLog log = new NewLog(logString, NewLog.Level.Information))
//      {
//        log.Info("test");
//      }
//      string message = logString.ToString().Substring(27);
//      Assert.AreEqual(" [Information][TestRunnerThread]: test\r\n", message,"LogMessage is not what we expect");
//    }

////            private string TEST_INFO = "Testing Info";
////            private string TEST_WARN = "Testing Warn";
////            private string TEST_ERROR = "Testing Error";
////            private string TEST_DEBUG = "Testing Debug";

////        [Test]
////        public void LogLevelError()
////        {
////                    StringWriter logString = new StringWriter();
////                    Log log = new Log(logString, Log.Level.Error);

////          log.Debug(TEST_DEBUG);
////          Assert.IsTrue( logString.ToString() == string.Empty );

////          log.Info(TEST_INFO);
////                    Assert.IsTrue( logString.ToString() == string.Empty );

////          log.Warn(TEST_WARN);
////                    Assert.IsTrue( logString.ToString() == string.Empty );

////          log.Error(TEST_ERROR);
////                    Assert.IsTrue(logString.ToString().IndexOf(TEST_ERROR) != -1);
////        }

////            [Test]
////            public void LogLevelWarn()
////            {
////                StringWriter logString = new StringWriter();
////                Log log = new Log(logString, Log.Level.Warning);

////                log.Debug(TEST_DEBUG);
////                Assert.IsTrue(logString.ToString() == string.Empty);

////                log.Info(TEST_INFO);
////                Assert.IsTrue(logString.ToString() == string.Empty);

////                log.Warn(TEST_WARN);
////                Assert.IsTrue(logString.ToString().IndexOf(TEST_WARN) != -1);
////                logString.Flush();

////                log.Error(TEST_ERROR);
////                Assert.IsTrue(logString.ToString().IndexOf(TEST_ERROR) != -1);
////                logString.Flush();
////            }

////            [Test]
////            public void LogLevelInfo()
////            {
////                StringWriter logString = new StringWriter();
////                Log log = new Log(logString, Log.Level.Information);

////                log.Debug(TEST_DEBUG);
////                Assert.IsTrue(logString.ToString() == string.Empty);

////                log.Info(TEST_INFO);
////                Assert.IsTrue(logString.ToString().IndexOf(TEST_INFO) != -1);
////                logString.Flush();

////                log.Warn(TEST_WARN);
////                Assert.IsTrue(logString.ToString().IndexOf(TEST_WARN) != -1);
////                logString.Flush();

////                log.Error(TEST_ERROR);
////                Assert.IsTrue(logString.ToString().IndexOf(TEST_ERROR) != -1);
////                logString.Flush();
////            }

////            [Test]
////            public void LogLevelDebug()
////            {
////                StringWriter logString = new StringWriter();
////                Log log = new Log(logString, Log.Level.Debug);

////                log.Debug(TEST_DEBUG);
////                Assert.IsTrue(logString.ToString().IndexOf(TEST_DEBUG) != -1);
////                logString.Flush();

////                log.Info(TEST_INFO);
////                Assert.IsTrue(logString.ToString().IndexOf(TEST_INFO) != -1);
////                logString.Flush();

////                log.Warn(TEST_WARN);
////                Assert.IsTrue(logString.ToString().IndexOf(TEST_WARN) != -1);
////                logString.Flush();

////                log.Error(TEST_ERROR);
////                Assert.IsTrue(logString.ToString().IndexOf(TEST_ERROR) != -1);
////                logString.Flush();
////            }
//  }
//}
