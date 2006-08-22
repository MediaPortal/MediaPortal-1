/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
 */

using System;
using System.Collections;
using MediaPortal.Utils.Services;

namespace MediaPortal.GUI.X10Plugin
{
	/// <summary>
	/// X10Plugin Appliance Configuration class. Load and save conf in Mediaportal xml config file.
	/// </summary>
	public class ApplianceConfiguration
	{
		public ArrayList m_X10Appliances = new ArrayList();
		public ArrayList m_locations = new ArrayList();
		public int m_CMDevice = (int)SendX10.CMDevices.CM11;
		public string m_CM1xHost = "localhost"; // must contain real servername (no domain i think)
		public int m_CM17COMPort = 1;

		public const string DEFAULT_LOCATION = "Default location";
    private IConfig _config;

		public ApplianceConfiguration()
		{
      ServiceProvider services = GlobalServiceProvider.Instance;
      _config = services.Get<IConfig>();
		}

		/// <summary>
		/// Load X10Plugin settings from MediaPortal.xml file
		/// </summary>
		public void LoadSettings()
		{
			m_locations.Clear();
			m_X10Appliances.Clear();
			using( MediaPortal.Profile.Settings   xmlreader=new MediaPortal.Profile.Settings(_config.Get(Config.Options.ConfigPath) + "MediaPortal.xml"))
			{
				int i = 0;
				while(true)
				{
					string strCodeTag = String.Format("Code{0}", i);
					string strDescriptionTag = String.Format("Description{0}", i);
					string strLocationTag = String.Format("Location{0}", i);

					i++;
					string strCode = xmlreader.GetValueAsString("X10Plugin", strCodeTag, "");
					string strDescription = xmlreader.GetValueAsString("X10Plugin", strDescriptionTag, "");
					string strLocation = xmlreader.GetValueAsString("X10Plugin", strLocationTag, DEFAULT_LOCATION);

					if (strCode.Length > 0 && strDescription.Length > 0)
					{
						X10Appliance sx10 = new X10Appliance();
						sx10.m_strCode = strCode;
						sx10.m_strDescription = strDescription;
						sx10.m_location = strLocation;
						m_X10Appliances.Add(sx10);

						if (! m_locations.Contains(strLocation))
							m_locations.Add(strLocation);
					}
					else break;
				}
				
				m_CM1xHost = xmlreader.GetValueAsString("X10Plugin", "CM1xHost", "localhost"); 
				m_CMDevice = xmlreader.GetValueAsInt("X10Plugin", "CMDevice", (int)SendX10.CMDevices.CM11);
				m_CM17COMPort = xmlreader.GetValueAsInt("X10Plugin", "CM17COMPort", 1);
				/*
				if(m_X10Appliances.Count < 1)
				{
					SmallUrl surl = new SmallUrl();
					surl.m_strLocation="MediaPortal";
					surl.m_strUrl="http://www.team-mediaportal.com";
					m_SmallUrls.Add(surl);
					surl = new SmallUrl();
					surl.m_strLocation="Microsoft";
					surl.m_strUrl="http://www.microsoft.com";
					m_SmallUrls.Add(surl);
				}*/

			}
		}

		/// <summary>
		/// Save config in Mediaportal.xml file
		/// </summary>
		public void SaveSettings()
		{
			using(MediaPortal.Profile.Settings   xmlwriter=new MediaPortal.Profile.Settings(_config.Get(Config.Options.ConfigPath) + "MediaPortal.xml"))
			{
				int i=0;
				foreach (X10Appliance sx10 in m_X10Appliances)
				{
					string strCodeTag = String.Format("Code{0}", i);
					string strDescriptionTag = String.Format("Description{0}", i);
					string strLocationTag = String.Format("Location{0}", i);

					xmlwriter.SetValue("X10Plugin", strCodeTag, sx10.m_strCode);
					xmlwriter.SetValue("X10Plugin", strDescriptionTag, sx10.m_strDescription);
					if ((sx10.m_location != null) && (sx10.m_location != ""))
						xmlwriter.SetValue("X10Plugin", strLocationTag, sx10.m_location);

					i++;
				}

				xmlwriter.SetValue("X10Plugin", "CM1xHost", m_CM1xHost);
				xmlwriter.SetValue("X10Plugin", "CMDevice", m_CMDevice);
				xmlwriter.SetValue("X10Plugin", "CM17COMPort", m_CM17COMPort);
			}
		}

	}

	class X10Appliance
	{
		public string m_strCode;
		public string m_strDescription;
		public string m_location;
	}
	
}
