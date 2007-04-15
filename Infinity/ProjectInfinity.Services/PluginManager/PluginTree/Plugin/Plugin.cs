#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 *  Code modified from SharpDevelop AddIn code
 *  Thanks goes to: Mike Krüger
 */

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using ProjectInfinity;
using ProjectInfinity.Logging;

namespace ProjectInfinity.Plugins
{
	public sealed class Plugin
	{
		Properties    _properties = new Properties();
		List<PluginRuntime> _runtimes   = new List<PluginRuntime>();
    //List<string> bitmapResources = new List<string>();
    //List<string> stringResources = new List<string>();
		
		string        _fileName = null;
		PluginManifest  _manifest = new PluginManifest();
		Dictionary<string, ExtensionPath> _paths = new Dictionary<string, ExtensionPath>();
		//AddInAction _action = AddInAction.Disable;
		bool _enabled;
		
		//static bool hasShownErrorMessage = false;

		public object CreateObject(string className)
		{
			foreach (PluginRuntime runtime in _runtimes) {
				object o = runtime.CreateInstance(className);
				if (o != null) {
					return o;
				}
			}
      ServiceScope.Get<ILogger>().Error("Cannot create object: " + className);
      //if (hasShownErrorMessage) {
      //  ServiceScope.Get<ILogger>().Error("Cannot create object: " + className);
      //} else {
      //  hasShownErrorMessage = true;
      //  //MessageService.ShowError("Cannot create object: " + className + "\nFuture missing objects will not cause an error message.");
      //}
			return null;
		}
		
		public override string ToString()
		{
			return "[Plugin: " + Name + "]";
		}
		
		string customErrorMessage;
		
		/// <summary>
		/// Gets the message of a custom load error. Used only when AddInAction is set to CustomError.
		/// Settings this property to a non-null value causes Enabled to be set to false and
		/// Action to be set to AddInAction.CustomError.
		/// </summary>
		public string CustomErrorMessage {
			get {
				return customErrorMessage;
			}
			internal set {
				if (value != null) {
					Enabled = false;
          //Action = AddInAction.CustomError;
				}
				customErrorMessage = value;
			}
		}
		
    ///// <summary>
    ///// Action to execute when the application is restarted.
    ///// </summary>
    //public AddInAction Action {
    //  get {
    //    return action;
    //  }
    //  set {
    //    action = value;
    //  }
    //}
		
		public List<PluginRuntime> Runtimes {
			get {
				return _runtimes;
			}
		}
		
		public Version Version {
			get {
				return _manifest.PrimaryVersion;
			}
		}
		
		public string FileName {
			get {
				return _fileName;
			}
		}
		
		public string Name {
			get {
				return _properties["name"];
			}
		}
		
		public PluginManifest Manifest {
			get {
				return _manifest;
			}
		}
		
		public Dictionary<string, ExtensionPath> Paths {
			get {
				return _paths;
			}
		}
		
		public Properties Properties {
			get {
				return _properties;
			}
		}
		
    //public List<string> BitmapResources {
    //  get {
    //    return bitmapResources;
    //  }
    //  set {
    //    bitmapResources = value;
    //  }
    //}
		
    //public List<string> StringResources {
    //  get {
    //    return stringResources;
    //  }
    //  set {
    //    stringResources = value;
    //  }
    //}
		
		public bool Enabled {
			get {
				return _enabled;
			}
			internal set {
				_enabled = value;
        //this.Action = value ? AddInAction.Enable : AddInAction.Disable;
			}
		}
		
		internal Plugin()
		{
		}
		
		static void SetupPlugin(XmlReader reader, Plugin plugin, string hintPath)
		{
			while (reader.Read()) {
				if (reader.NodeType == XmlNodeType.Element && reader.IsStartElement()) {
					switch (reader.LocalName) {
						case "StringResources":
						case "BitmapResources":
							if (reader.AttributeCount != 1) {
								throw new PluginLoadException("BitmapResources requires ONE attribute.");
							}

              string filename = reader.GetAttribute("file"); // StringParser.Parse(reader.GetAttribute("file"));
							
              //if(reader.LocalName == "BitmapResources")
              //{
              //  addIn.BitmapResources.Add(filename);
              //}
              //else
              //{
              //  addIn.StringResources.Add(filename);
              //}
							break;
						case "Runtime":
							if (!reader.IsEmptyElement) {
								PluginRuntime.ReadSection(reader, plugin, hintPath);
							}
							break;
						case "Include":
							if (reader.AttributeCount != 1) {
								throw new PluginLoadException("Include requires ONE attribute.");
							}
							if (!reader.IsEmptyElement) {
								throw new PluginLoadException("Include nodes must be empty!");
							}
							if (hintPath == null) {
								throw new PluginLoadException("Cannot use include nodes when hintPath was not specified (e.g. when AddInManager reads a .addin file)!");
							}
							string fileName = Path.Combine(hintPath, reader.GetAttribute(0));
							XmlReaderSettings xrs = new XmlReaderSettings();
							xrs.ConformanceLevel = ConformanceLevel.Fragment;
							using (XmlReader includeReader = XmlTextReader.Create(fileName, xrs)) {
								SetupPlugin(includeReader, plugin, Path.GetDirectoryName(fileName));
							}
							break;
						case "Path":
							if (reader.AttributeCount != 1) {
								throw new PluginLoadException("Import node requires ONE attribute.");
							}
							string pathName = reader.GetAttribute(0);
							ExtensionPath extensionPath = plugin.GetExtensionPath(pathName);
							if (!reader.IsEmptyElement) {
								ExtensionPath.SetUp(extensionPath, reader, "Path");
							}
							break;
						case "Manifest":
							plugin.Manifest.ReadManifestSection(reader, hintPath);
							break;
						default:
							throw new PluginLoadException("Unknown root path node:" + reader.LocalName);
					}
				}
			}
		}
		
		public ExtensionPath GetExtensionPath(string pathName)
		{
			if (!_paths.ContainsKey(pathName)) {
				return _paths[pathName] = new ExtensionPath(pathName, this);
			}
			return _paths[pathName];
		}
		
		public static Plugin Load(TextReader textReader)
		{
			return Load(textReader, null);
		}
		
		public static Plugin Load(TextReader textReader, string hintPath)
		{
			Plugin plugin = new Plugin();
			using (XmlTextReader reader = new XmlTextReader(textReader)) {
				while (reader.Read()){
					if (reader.IsStartElement()) {
						switch (reader.LocalName) {
							case "Plugin":    // addin -> plugin
								plugin._properties = Properties.ReadFromAttributes(reader);
								SetupPlugin(reader, plugin, hintPath);
								break;
							default:
								throw new PluginLoadException("Unknown add-in file.");
						}
					}
				}
			}
			return plugin;
		}
		
		public static Plugin Load(string fileName)
		{
			try {
				using (TextReader textReader = File.OpenText(fileName)) {
					Plugin plugin = Load(textReader, Path.GetDirectoryName(fileName));
					plugin._fileName = fileName;
					return plugin;
				}
			} catch (Exception e) {
				throw new PluginLoadException("Can't load " + fileName, e);
			}
		}
	}
}
