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
using System.Collections.Generic;
using System.Text;
using MediaPortal.GUI.Library;

namespace DvrMpegCutMP
{
	public class DvrMpegCutMPSetup : ISetupForm
	{
		int windowID = 170601;

		public DvrMpegCutMPSetup()
		{ }

		#region ISetupForm Member

		public string Author()
		{
			return "kaybe and brutus, skin by Ralph";
		}

		public bool CanEnable()
		{
			return true;
		}

		public bool DefaultEnabled()
		{
			return true;
		}

		public string Description()
		{
			return "to cut mpeg and dvr-ms files";
		}

		public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
		{
			strButtonText = PluginName();
			strButtonImage = String.Empty;
			strButtonImageFocus = String.Empty;
			strPictureImage = String.Empty;
			return true;
		}

		public int GetWindowId()
		{
			return windowID;
		}

		public bool HasSetup()
		{
			return false;
		}

		public string PluginName()
		{
			return "Dvr-MpegCut";
		}

		public void ShowPlugin()
		{
			System.Windows.Forms.MessageBox.Show("Nothing to configure, just enable and start MP ;)");
		}

		#endregion
	}
}
