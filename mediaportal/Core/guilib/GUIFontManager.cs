using System;
using System.Collections;
using System.Xml;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// The class responsible for keeping track of the used fonts.
	/// </summary>
	public class GUIFontManager
	{
		static protected ArrayList m_fonts = new ArrayList();

    // singleton. Dont allow any instance of this class
    private GUIFontManager()
    {
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
      Log.Write("Load fonts from {0}", strFilename);
			m_fonts.Clear();
			// Load the debug font
      GUIFont fontDebug = new GUIFont("debug","Arial",12);
      fontDebug.Load();
      m_fonts.Add(fontDebug);			
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
					if (nodeHeight!=null&&nodeName!=null &&  nodeFileName!=null)
					{
						string strName=nodeName.InnerText;
						string strFileName=nodeFileName.InnerText;
						int iHeight=Int32.Parse(nodeHeight.InnerText);
            
            // height is based on 720x576
            float fPercent =( (float)GUIGraphicsContext.Height) / 576.0f;
            fPercent*=iHeight;
            iHeight=(int)fPercent;
						GUIFont font = new GUIFont(strName,strFileName,iHeight);
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
						m_fonts.Add(font);
					}
				}
				return true;
			}
			catch(Exception ex)
			{
        Log.Write("exception loading fonts {0} err:{1} stack:{2}", strFilename, ex.Message,ex.StackTrace);
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
      if (iFont>=0 && iFont < m_fonts.Count) return (GUIFont) m_fonts[ iFont];
      return GetFont("debug");
    }

		/// <summary>
		/// Gets a GUIFont.
		/// </summary>
		/// <param name="strFontName">The name of the font</param>
		/// <returns>A GUIFont instance representing the strFontName or a default GUIFont if the strFontName does not exists.</returns>
		static public GUIFont GetFont( string strFontName)
		{
			for (int i=0; i < m_fonts.Count;++i)
			{
        GUIFont font=(GUIFont)m_fonts[i];
				if (font.FontName==strFontName) return font;
			}

      // just return a font
      return GetFont("debug");
		}

    static public void Present()
    {
      for (int i=0; i < m_fonts.Count;++i)
      {
        GUIFont font=(GUIFont)m_fonts[i];
        font.Present();
      }
    }
		/// <summary>
		/// Disposes all GUIFonts.
		/// </summary>
		static public void	Dispose()
		{
      foreach (GUIFont font in m_fonts)
      {
        font.Dispose(null,null);
      }
		}

		/// <summary>
		/// Initializes the device objects of the GUIFonts.
		/// </summary>
		static public void InitializeDeviceObjects()
		{
      Log.Write("fonts.InitializeDeviceObjects()");
			foreach (GUIFont font in m_fonts)
			{
				font.InitializeDeviceObjects();
			}
		}

		/// <summary>
		/// Restores the device objects of the GUIFonts.
		/// </summary>
		static public void RestoreDeviceObjects()
		{
      if (GUIGraphicsContext.CurrentState==GUIGraphicsContext.State.STOPPING) return;

			foreach (GUIFont font in m_fonts)
			{
				font.RestoreDeviceObjects();
			}
		}
	}
}
