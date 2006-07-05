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
using System.Collections.Generic;
using System.Xml;
using System.Runtime.InteropServices;
using MediaPortal.Utils.Services;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// The class responsible for keeping track of the used fonts.
	/// </summary>
	public class GUIFontManager
	{
		[DllImport("fontEngine.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		unsafe private static extern void FontEnginePresentTextures();


		[DllImport("fontEngine.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		unsafe private static extern void FontEngineSetDevice(void* device);

    static protected List<GUIFont> _listFonts = new List<GUIFont>();
    static protected ILog _log;

		// singleton. Dont allow any instance of this class
		private GUIFontManager()
    {
		}

    static GUIFontManager()
    {
      ServiceProvider services = GlobalServiceProvider.Instance;
      _log = services.Get<ILog>();
    }

		static public int Count
		{
			get { return _listFonts.Count;}
		}
		/// <summary>
		/// Loads the fonts from a file.
		/// </summary>
		/// <param name="strFilename">The filename from where the fonts are loaded.</param>
		/// <returns>true if loaded else false</returns>
		static public bool LoadFonts( string strFilename)
		{
			// Clear current set of fonts
			Dispose();
			int counter=0;
			_log.Info("  Load fonts from {0}", strFilename);
			_listFonts.Clear();

			// Load the debug font
			GUIFont fontDebug = new GUIFont("debug","Arial",12);
			fontDebug.ID=counter++;
			fontDebug.Load();
			_listFonts.Add(fontDebug);			


			try
			{
				// Load the XML document
				XmlDocument doc = new XmlDocument();
				doc.Load(strFilename);
				// Check the root element
				if (doc.DocumentElement==null) return false;
				string strRoot=doc.DocumentElement.Name;
				if (strRoot!="fonts") return false;
				// Select the list of fonts
				XmlNodeList list=doc.DocumentElement.SelectNodes("/fonts/font");
				foreach (XmlNode node in list)
				{
					XmlNode nodeStart=node.SelectSingleNode("start");
					XmlNode nodeEnd  =node.SelectSingleNode("end");
					XmlNode nodeName = node.SelectSingleNode("name");
					XmlNode nodeFileName = node.SelectSingleNode("filename");
					XmlNode nodeHeight = node.SelectSingleNode("height");
					XmlNode nodeBold = node.SelectSingleNode("bold");
					XmlNode nodeItalics = node.SelectSingleNode("italic");
					if (nodeHeight!=null&&nodeName!=null &&  nodeFileName!=null)
					{
						bool   bold=false;
						bool   italic=false;
						if (nodeBold!=null && nodeBold.InnerText!=null && nodeBold.InnerText.Equals("yes") ) 
							bold=true;
						if (nodeItalics!=null && nodeItalics.InnerText!=null && nodeItalics.InnerText.Equals("yes") ) 
							italic=true;
						string strName=nodeName.InnerText;
						string strFileName=nodeFileName.InnerText;
						int iHeight=Int32.Parse(nodeHeight.InnerText);
            
						// height is based on 720x576
						float fPercent =( (float)GUIGraphicsContext.Height) / 576.0f;
						fPercent*=iHeight;
						iHeight=(int)fPercent;
						System.Drawing.FontStyle style = new System.Drawing.FontStyle();
						style=System.Drawing.FontStyle.Regular;
						if (bold)
							style|=System.Drawing.FontStyle.Bold;
						if (italic)
							style|=System.Drawing.FontStyle.Italic;
						GUIFont font = new GUIFont(strName,strFileName,iHeight,style);
						font.ID=counter++;
						if (nodeStart!=null && nodeStart.InnerText!="" && nodeEnd!=null&& nodeEnd.InnerText!="" )
						{
							int start=Int32.Parse(nodeStart.InnerText);
							int end=Int32.Parse(nodeEnd.InnerText);
							font.SetRange(start,end);
						}
						else
						{
							font.SetRange(0,GUIGraphicsContext.CharsInCharacterSet);
						}

						font.Load();
						_listFonts.Add(font);
					}
				}
				return true;
			}
			catch(Exception ex)
			{
				_log.Info("exception loading fonts {0} err:{1} stack:{2}", strFilename, ex.Message,ex.StackTrace);
			}

			return false;
		}
		
		/// <summary>
		/// Gets a GUIFont.
		/// </summary>
		/// <param name="iFont">The font number</param>
		/// <returns>A GUIFont instance representing the fontnumber or a default GUIFont if the number does not exists.</returns>
		static public GUIFont GetFont( int iFont)
		{
			if (iFont>=0 && iFont < _listFonts.Count) return  _listFonts[ iFont];
			return GetFont("debug");
		}

		/// <summary>
		/// Gets a GUIFont.
		/// </summary>
		/// <param name="strFontName">The name of the font</param>
		/// <returns>A GUIFont instance representing the strFontName or a default GUIFont if the strFontName does not exists.</returns>
		static public GUIFont GetFont( string strFontName)
		{
			for (int i=0; i < _listFonts.Count;++i)
			{
				GUIFont font=_listFonts[i];
				if (font.FontName==strFontName) return font;
			}

			// just return a font
			return GetFont("debug");
		}

		static public void Present()
		{

			FontEnginePresentTextures();
			for (int i=0; i < _listFonts.Count;++i)
			{
				GUIFont font=_listFonts[i];
				font.Present();
			}
		}
		/// <summary>
		/// Disposes all GUIFonts.
		/// </summary>
		static public void	Dispose()
		{
			_log.Info("  fonts.Dispose()");
			foreach (GUIFont font in _listFonts)
			{
				font.Dispose(null,null);
			}
		}

		/// <summary>
		/// Initializes the device objects of the GUIFonts.
		/// </summary>
		static public void InitializeDeviceObjects()
		{
			_log.Info("  fonts.InitializeDeviceObjects()");
      IntPtr upDevice = DShowNET.Helper.DirectShowUtil.GetUnmanagedDevice(GUIGraphicsContext.DX9Device);

      unsafe
      {
        FontEngineSetDevice(upDevice.ToPointer());
      }
			foreach (GUIFont font in _listFonts)
			{
				font.InitializeDeviceObjects();
			}
		}

		/// <summary>
		/// Restores the device objects of the GUIFonts.
		/// </summary>
		static public void RestoreDeviceObjects()
		{
			IntPtr upDevice = DShowNET.Helper.DirectShowUtil.GetUnmanagedDevice(GUIGraphicsContext.DX9Device);

			unsafe
			{
				FontEngineSetDevice(upDevice.ToPointer());
			}
			if (GUIGraphicsContext.CurrentState==GUIGraphicsContext.State.STOPPING) return;

			foreach (GUIFont font in _listFonts)
			{
				font.RestoreDeviceObjects();
			}

		}
	}
}
