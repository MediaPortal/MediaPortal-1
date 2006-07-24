// ConfigReader.cs: Interface to other app's .config file
// Copyright (C) 2005-2006  Michel Otte
// 
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
/*
 * Created by SharpDevelop.
 * User: Michel
 * Date: 25-9-2005
 * Time: 14:19
 * 
 */

using System;
using System.Xml;
using System.Reflection;
using System.Configuration;

namespace MPTestTool
{
	/// <summary>
	/// Interface to other app's .config file
	/// </summary>
	public class ConfigReader : AppSettingsReader
	{
		private bool configLoaded = false;
		private string _configFile;
		private XmlDocument config = new XmlDocument();
		
		public string configFile
		{
			get {
				return _configFile;
			}
			set {
				_configFile = value;
				loadConfig();
			}
		}
		public ConfigReader()
		{
		}
		public ConfigReader(string configFile)
		{
			_configFile = configFile;
			loadConfig();
		}
		private void loadConfig()
		{
			config.Load(_configFile);
			configLoaded = true;
		}
		public bool ConfigIsLoaded()
		{
			return configLoaded;
		}
		public string GetValue(string key)
		{
			return Convert.ToString(GetValue(key, typeof(string)));
		}
		public new object GetValue(string key, System.Type type)
		{
			if (!ConfigIsLoaded()) return String.Empty;
			XmlNode node;
			string value = String.Empty;
			string selectedNode = key.Substring(0, key.LastIndexOf("//"));
			try {
				node = config.SelectSingleNode(selectedNode);
				if (node != null)
				{
					XmlElement e = (XmlElement) 
						node.SelectSingleNode(key.Replace(selectedNode, ""));
					if (e != null)
					{
						value = e.GetAttribute("value");
					}
				}
				if (type == typeof(bool))
				{
					if ( (value.Equals("True")) || (value.Equals("False")) )
						return Convert.ToBoolean(value);
					else
						return false;
				}
        else if (type == typeof(int))
					return Convert.ToInt32(value);
				else if (type == typeof(double))
					return Convert.ToDouble(value);
				else if (type == typeof(DateTime))
					return Convert.ToDateTime(value);
				else
					return Convert.ToString(value);
			} catch {
				return String.Empty;
			}
		}
	}
}
