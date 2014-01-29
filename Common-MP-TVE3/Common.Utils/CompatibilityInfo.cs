#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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

using System;
using System.Reflection;
using MediaPortal.Common.Utils;

[assembly: SubsystemVersion("*", "1.1.6.27644")]

// MediaPortal subsystems' breaking versions
[assembly: SubsystemVersion("MP", "1.1.6.27644")]

[assembly: SubsystemVersion("MP.SkinEngine", "1.3.100.0")] // MP 1.4.0 Pre Release
[assembly: SubsystemVersion("MP.SkinEngine.Core", "1.1.6.27644")]
[assembly: SubsystemVersion("MP.SkinEngine.Controls", "1.3.100.0")]
[assembly: SubsystemVersion("MP.SkinEngine.Dialogs", "1.1.6.27644")]

[assembly: SubsystemVersion("MP.Input", "1.1.6.27644")]
[assembly: SubsystemVersion("MP.Input.Mapping", "1.1.6.27644")]
[assembly: SubsystemVersion("MP.Input.Keyboard", "1.1.6.27644")]
[assembly: SubsystemVersion("MP.Input.Mouse", "1.1.6.27644")]

[assembly: SubsystemVersion("MP.Players", "1.1.6.27644")]
[assembly: SubsystemVersion("MP.Players.DVD", "1.1.6.27644")]
[assembly: SubsystemVersion("MP.Players.Video", "1.1.6.27644")]
[assembly: SubsystemVersion("MP.Players.TV", "1.1.6.27644")]
[assembly: SubsystemVersion("MP.Players.Music", "1.3.100.0")]

[assembly: SubsystemVersion("MP.DB", "1.1.6.27644")]
[assembly: SubsystemVersion("MP.DB.Music", "1.5.100.0")]
[assembly: SubsystemVersion("MP.DB.Videos", "1.2.100.0")] // MP 1.3.0 Alpha 1
[assembly: SubsystemVersion("MP.DB.Pictures", "1.1.6.27644")]

[assembly: SubsystemVersion("MP.Filters", "1.1.6.27644")]
[assembly: SubsystemVersion("MP.Filters.TsWriter", "1.1.6.27644")]
[assembly: SubsystemVersion("MP.Filters.MPFileWriter", "1.1.6.27644")]
[assembly: SubsystemVersion("MP.Filters.StreamingServer", "1.1.6.27644")]
[assembly: SubsystemVersion("MP.Filters.TsReader", "1.1.6.27644")]
[assembly: SubsystemVersion("MP.Filters.DVBSubs", "1.1.6.27644")]
[assembly: SubsystemVersion("MP.Filters.AudioSwitcher", "1.1.6.27644")]
[assembly: SubsystemVersion("MP.Filters.IPTV", "1.1.6.27644")]

[assembly: SubsystemVersion("MP.Externals", "1.1.6.27644")]
[assembly: SubsystemVersion("MP.Externals.MediaInfo", "1.1.6.27644")]
[assembly: SubsystemVersion("MP.Externals.SQLite", "1.1.6.27644")]
[assembly: SubsystemVersion("MP.Externals.Gentle", "1.3.100.0")]  // MP 1.4.0 Pre Release
[assembly: SubsystemVersion("MP.Externals.Log4Net", "1.3.100.0")] // MP 1.4.0 Pre Release
[assembly: SubsystemVersion("MP.Externals.BASS", "1.3.100.0")]
[assembly: SubsystemVersion("MP.Externals.HTMLAgilityPack", "1.5.100.0")] // Added after 1.5 pre-release

[assembly: SubsystemVersion("MP.Config", "1.1.6.27644")]
[assembly: SubsystemVersion("MP.Config.DefaultSections", "1.1.6.27644")]

[assembly: SubsystemVersion("MP.Plugins", "1.1.6.27644")]             // reserved for plugin defined subsystems
[assembly: SubsystemVersion("MP.Plugins.Music", "1.6.100.0")]
[assembly: SubsystemVersion("MP.Plugins.Videos", "1.6.100.0")]
[assembly: SubsystemVersion("MP.Plugins.Pictures", "1.6.100.0")]
[assembly: SubsystemVersion("MP.Plugins.PowerScheduler", "1.1.6.27644")]
[assembly: SubsystemVersion("MP.Plugins.TV", "1.1.6.27644")]          // This refers to TVPlugin

//[assembly: SubsystemVersion("MP.Plugins.3rdParty", "1.1.6.27644")]  // reserved for 3rd party plugin defined subsystems


// TV Server subsystems' breaking versions

[assembly: SubsystemVersion("TVE", "1.1.6.27644")]

[assembly: SubsystemVersion("TVE.DB", "1.1.6.27644")]
[assembly: SubsystemVersion("TVE.Controller", "1.1.6.27644")]
[assembly: SubsystemVersion("TVE.Scheduler", "1.1.6.27644")]
[assembly: SubsystemVersion("TVE.Config", "1.1.6.27644")]
[assembly: SubsystemVersion("TVE.Config.Controls", "1.1.6.27644")]

[assembly: SubsystemVersion("TVE.Plugins", "1.1.6.27644")]
[assembly: SubsystemVersion("TVE.Plugins.PowerScheduler", "1.1.6.27644")]
[assembly: SubsystemVersion("TVE.Plugins.XmlTV", "1.1.6.27644")]
[assembly: SubsystemVersion("TVE.Plugins.WebEPG", "1.1.6.27644")]
[assembly: SubsystemVersion("TVE.Plugins.TvMoview", "1.1.6.27644")]
[assembly: SubsystemVersion("TVE.Plugins.ServerBlaster", "1.1.6.27644")]
