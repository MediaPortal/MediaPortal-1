/*
 * Test cases
 * Copyright (C) 2004 Morten Mertner
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: TestConfig.cs 1234 2008-03-14 11:41:44Z mm $
 */

using Gentle.Common;
using NUnit.Framework;

namespace Gentle.Framework.Tests
{
	[TestFixture]
	public class TestConfigFileAccess
	{
		// if you edit this you must also edit the name in the log4net configuration (or the test case will fail)
		private const string DEFAULT_LOG_NAME = "Gentle.NET.log";

		private class Cfg
		{
			[Configuration( "Gentle.Framework/Options/Analyzer/Silent" )]
			public bool isAnalyzerSilent = true; // default false in config file

			[Configuration( "Gentle.Framework/DefaultProvider/@connectionString" )]
			public string cn = null;

			public Cfg()
			{
				Configurator.Configure( this );
			}
		}

		/// <summary>
		/// Test case for verifying access to the configuration file and required keys in it.
		/// </summary>
		[Test]
		public void AccessTest()
		{
			Cfg cfg = new Cfg();
			Assert.AreEqual( false, cfg.isAnalyzerSilent, "enum" );
			Assert.IsNotNull( cfg.cn, "connstr" );
		}
	}

	/*
	/// <summary>
	/// This class holds test cases for the functionality in the General subfolder, which 
	/// are basic utility and helper classes used throughout the framework.
	/// </summary>
	[TestFixture]
	public class TestGeneral
	{
		// if you edit this you must also edit the name in the config file (or the test case will fail)
		private const string DEFAULT_LOG_NAME = "Gentle.NET.log";

		/// <summary>
		/// Test case for verifying access to the configuration file and required keys in it.
		/// </summary>
		[Test]
		public void AccessTest()
		{
			Config cfg = Config.GetConfig();
			// ensure environment string
			Assert.IsNotNull( cfg.Environment, "No environment!" );
			Assert.IsTrue( cfg.Environment.Length > 0 );
			// ensure machine name
			Assert.IsNotNull( cfg.MachineName, "No machine name!" );
			Assert.IsTrue( cfg.MachineName.Length > 0 );
			// check for presence of core framework keys (provide default value to avoid
			// exceptions for keys not present)
			Assert.IsNotNull( cfg[ "Gentle.Framework/EngineName", null ], "No engine name" );
			Assert.IsNotNull( cfg[ "Gentle.Framework/ConnectionString", null ], "No connstring" );
			Assert.IsNotNull( cfg[ "Gentle.Framework/Messages/DeveloperError", null ], "No messages" );
		}

		/// <summary>
		/// Ensure that the application is creating a logging file and the logging 
		/// configuration settings are set up correctly for log4net.
		/// </summary>
		[Test]
		public void LoggingTest () 
		{
			Check.Log( Severity.Debug, "Test logging level {0}", Severity.Debug );
			Check.Log( Severity.Info, "Test logging level {0}", Severity.Info );
			Check.Log( Severity.Notice, "Test logging level {0}", Severity.Notice );
			Check.Log( Severity.Warning, "Test logging level {0}", Severity.Warning );
			Check.Log( Severity.Error, "Test logging level {0}", Severity.Error );
			Check.Log( Severity.Critical, "Test logging level {0}", Severity.Critical );
			Assertion.Assert( "Check for log file failed: " + DEFAULT_LOG_NAME, 
				File.Exists( Path.Combine(Environment.CurrentDirectory, DEFAULT_LOG_NAME) ) );
		}
	}
	*/
}