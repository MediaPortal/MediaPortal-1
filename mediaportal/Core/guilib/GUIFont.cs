using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using Microsoft.DirectX;using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// An implementation of the GUIFont class (renders text using DirectX textures).  This implementation generates the necessary textures for rendering the fonts in DirectX in the @skin\skinname\fonts directory.
	/// </summary>
	public class GUIFont
	{
		private string m_strFontName;
		private string m_strFileName;
		public const int MaxNumfontVertices = 100*6;
    
    private int       _StartCharacter=32;
    private int       _EndCharacter=255;

		// Font rendering flags
		[System.Flags]
			public enum RenderFlags
		{
			Centered = 0x0001,
			TwoSided = 0x0002, 
			Filtered = 0x0004,
      DontDiscard=0x0008
		}
		private System.Drawing.Font systemFont;

		
		int m_iFontHeight;

		private CustomVertex.TransformedColoredTextured[] fontVertices = null;

		private float[,] textureCoords = null;

		// Stateblocks for setting and restoring render states
		//private StateBlock savedStateBlock;
		//private StateBlock drawTextStateBlock;
		private int spacingPerChar=0;
		private Direct3D.Texture fontTexture;
		private int textureWidth; // Texture dimensions
		private int textureHeight;
		private float textureScale;
		private Direct3D.VertexBuffer vertexBuffer;
		//private RenderStates renderState;
		//private TextureState textureState0;
		//private TextureState textureState1;
		//private Sampler samplerState0;
    private FontStyle m_FontStyle=FontStyle.Regular;
    bool m_bSupportsAlphaBlend ;
//    StateBlock savedStateBlock;

    // Fill vertex buffer
    int iv = 0;
    int dwNumTriangles = 0;
		/// <summary>
		/// Constructor of the GUIFont class.
		/// </summary>
		/// <param name="strName">The name of the font used in the skin. (E.g., debug)</param>
		/// <param name="strFileName">The system name of the font (E.g., Arial)</param>
		/// <param name="iHeight">The height of the font.</param>
		public GUIFont(string strName, string strFileName, int iHeight)
		{
			m_strFontName=strName;
			m_strFileName=strFileName;
			m_iFontHeight=iHeight;
    }

		/// <summary>
		/// Constructor of the GUIFont class.
		/// </summary>
		/// <param name="strName">The name of the font used in the skin (E.g., debug).</param>
		/// <param name="strFileName">The system name of the font (E.g., Arial).</param>
		/// <param name="iHeight">The height of the font.</param>
		/// <param name="style">The style of the font (E.g., Bold)</param>
    public GUIFont(string strName, string strFileName, int iHeight, FontStyle style)
    {
      m_strFontName=strName;
      m_strFileName=strFileName;
      m_FontStyle=style;
      m_iFontHeight=iHeight;
    }

    public void SetRange(int start, int end)
    {
      _StartCharacter=start;
      _EndCharacter=end+1;
      if (_StartCharacter < 32) _StartCharacter=32;
    }

		/// <summary>
		/// Get/set the name of the font used in the skin (E.g., debug).
		/// </summary>
		public string FontName
		{
			get { return m_strFontName;}
			set { m_strFontName=value;}
		}

		/// <summary>
		/// Get/set the system name of the font (E.g., Arial).
		/// </summary>
		public string FileName
		{
			get { return m_strFileName;}
			set { m_strFileName=value;}
		}

		/// <summary>
		/// Get/set the height of the font.
		/// </summary>
		public int FontSize
		{
			get { return m_iFontHeight;}
			set {m_iFontHeight=value;}
		}

		/// <summary>
		/// Creates a system font.
		/// </summary>
		/// <param name="strFileName">The system font name (E.g., Arial).</param>
		/// <param name="style">The font style.</param>
		/// <param name="Size">The size.</param>
		public void Create(string strFileName, FontStyle style, int Size)
		{
      Dispose(null,null);
			m_strFileName = strFileName;
			m_iFontHeight = Size;
			systemFont = new System.Drawing.Font(m_strFileName, (float)m_iFontHeight, style);
		}

		/// <summary>
		/// Draws text with a maximum width.
		/// </summary>
		/// <param name="xpos">The X position.</param>
		/// <param name="ypos">The Y position.</param>
		/// <param name="color">The font color.</param>
		/// <param name="strLabel">The actual text.</param>
		/// <param name="fMaxWidth">The maximum width.</param>
		public void DrawTextWidth(float xpos,float ypos,long color,string strLabel, float fMaxWidth, GUIControl.Alignment alignment)
		{
			if (fMaxWidth<=0) return;
			if (xpos <=0) return;
			if (ypos <=0) return;
      if (strLabel==null) return;
      if (strLabel.Length==0) return;
			float fTextWidth=0,fTextHeight=0;
			GetTextExtent(strLabel,ref fTextWidth, ref fTextHeight);
			if (fTextWidth <=fMaxWidth)
			{
				DrawText( xpos, ypos, color, strLabel,alignment );	
				return;
			}
      while (fTextWidth >= fMaxWidth && strLabel.Length>1)
      {
        if (alignment==GUICheckMarkControl.Alignment.ALIGN_RIGHT)
          strLabel=strLabel.Substring(1);
        else
          strLabel=strLabel.Substring(0,strLabel.Length-1);
        GetTextExtent( strLabel, ref fTextWidth,ref fTextHeight);
      }
      GetTextExtent(strLabel,ref fTextWidth, ref fTextHeight);
      if (fTextWidth <=fMaxWidth)
      {
        DrawText( xpos, ypos, color, strLabel,alignment);	
      }
		}
    
		/// <summary>
		/// Draws aligned text.
		/// </summary>
		/// <param name="xpos">The X position.</param>
		/// <param name="ypos">The Y position.</param>
		/// <param name="color">The font color.</param>
		/// <param name="strLabel">The actual text.</param>
		/// <param name="alignment">The alignment of the text.</param>
		public void DrawText(float xpos, float ypos, long color, string strLabel, GUIControl.Alignment alignment)
		{
			if (strLabel==null) return;
			if (strLabel==String.Empty) return;
			if (xpos <=0) return;
			if (ypos <=0) return;
			int alpha=(int)((color>>24)&0xff);
			int red=(int)((color>>16)&0xff);
			int green=(int)((color>>8) &0xff);
			int blue=(int)(color&0xff);

			
			if (alignment==GUIControl.Alignment.ALIGN_LEFT)
			{
				DrawText(xpos, ypos, Color.FromArgb(alpha,red,green,blue), strLabel, RenderFlags.Filtered);
			}
			else if (alignment==GUIControl.Alignment.ALIGN_RIGHT)
			{
				float fW=0,fH=0;
				GetTextExtent(strLabel,ref fW, ref fH);
				DrawText(xpos-fW, ypos, Color.FromArgb(alpha,red,green,blue), strLabel, RenderFlags.Filtered);
      }
		}

		/// <summary>
		/// Draw shadowed text.
		/// </summary>
		/// <param name="fOriginX">The X position.</param>
		/// <param name="fOriginY">The Y position.</param>
		/// <param name="dwColor">The font color.</param>
		/// <param name="strText">The actual text.</param>
		/// <param name="alignment">The alignment of the text.</param>
		/// <param name="iShadowWidth">The width parameter of the shadow.</param>
		/// <param name="iShadowHeight">The height parameter of the shadow.</param>
		/// <param name="dwShadowColor">The shadow color.</param>
		public void DrawShadowText( float fOriginX, float fOriginY, long dwColor,
																string  strText, 
																GUIControl.Alignment alignment,
																int iShadowWidth, 
																int iShadowHeight,
																long dwShadowColor)
		{

			for (int x=-iShadowWidth; x < iShadowWidth; x++)
			{
				for (int y=-iShadowHeight; y < iShadowHeight; y++)
				{
					DrawText( (float)x+fOriginX, (float)y+fOriginY, dwShadowColor, strText,alignment);	
				}
			}
			DrawText( fOriginX, fOriginY, dwColor, strText,alignment);	
		}
	
    public void Present()
    {
      // Set the data for the vertex buffer
      if (dwNumTriangles > 0)
      {
				if (vertexBuffer==null) return;
				//if (savedStateBlock==null) return;
				if (GUIGraphicsContext.DX9Device==null) return;
				if (vertexBuffer.Disposed) return;
				//if (savedStateBlock.Disposed) return;
				if (GUIGraphicsContext.DX9Device.Disposed) return;
				if (fontVertices==null) return;
//        savedStateBlock.Apply();
        vertexBuffer.SetData(fontVertices, 0, LockFlags.Discard);
        GUIGraphicsContext.DX9Device.SetTexture(0, fontTexture);
        GUIGraphicsContext.DX9Device.VertexFormat = CustomVertex.TransformedColoredTextured.Format;
///        GUIGraphicsContext.DX9Device.PixelShader = null;
        GUIGraphicsContext.DX9Device.SetStreamSource(0, vertexBuffer, 0);

//        GUIGraphicsContext.DX9Device.SamplerState[0].MinFilter = TextureFilter.Linear;
//        GUIGraphicsContext.DX9Device.SamplerState[0].MagFilter = TextureFilter.Linear;
        GUIGraphicsContext.DX9Device.DrawPrimitives(PrimitiveType.TriangleList, 0, dwNumTriangles);
				GUIGraphicsContext.DX9Device.SetTexture(0, null);

			}
      dwNumTriangles=0;
      iv=0;
    }

    public bool GetFontCache(float xpos, float ypos, long color, string text, out VertexBuffer cachedVertexBuffer, out int triangles)
    {
      int alpha=(int)((color>>24)&0xff);
      int red=(int)((color>>16)&0xff);
      int green=(int)((color>>8) &0xff);
      int blue=(int)(color&0xff);
      return GetFontCache(xpos, ypos, Color.FromArgb(alpha,red,green,blue), text, out cachedVertexBuffer, out triangles);
    }
    public bool GetFontCache(float xpos, float ypos, Color color, string text, out VertexBuffer cachedVertexBuffer, out int triangles)
		{
			triangles=0;
			cachedVertexBuffer=null;

			if (text==null) return false;
			if (text==String.Empty) return false;
			if (xpos <=0) return false;
			if (ypos <=0) return false;
      if (GUIGraphicsContext.graphics!=null) 
      {
        return false;
      }
      VertexBuffer tmp=vertexBuffer;
      vertexBuffer = new VertexBuffer(typeof(CustomVertex.TransformedColoredTextured), MaxNumfontVertices,
				                                    GUIGraphicsContext.DX9Device, Usage.WriteOnly, 0, Pool.Managed);
      DrawText(xpos, ypos, color, text, RenderFlags.DontDiscard);
      vertexBuffer.SetData(fontVertices, 0, LockFlags.Discard);
      triangles=dwNumTriangles;     
      cachedVertexBuffer=vertexBuffer;

      Present();
      vertexBuffer=tmp;

      dwNumTriangles=0;
      iv=0;
      return true;
    }
    public void DrawFontCache(ref VertexBuffer cachedVertexBuffer, int triangles)
    {
      //return;
      // Set the data for the vertexbuffer
      if (cachedVertexBuffer!=null && triangles>0)
      {
        //savedStateBlock.Apply();
        GUIGraphicsContext.DX9Device.SetTexture(0, fontTexture);
        GUIGraphicsContext.DX9Device.VertexFormat = CustomVertex.TransformedColoredTextured.Format;
        GUIGraphicsContext.DX9Device.SetStreamSource(0, cachedVertexBuffer, 0);
        GUIGraphicsContext.DX9Device.DrawPrimitives(PrimitiveType.TriangleList, 0, triangles);
      }
    }
		/// <summary>
		/// Draw some text on the screen.
		/// </summary>
		/// <param name="xpos">The X position.</param>
		/// <param name="ypos">The Y position.</param>
		/// <param name="color">The font color.</param>
		/// <param name="text">The actual text.</param>
		/// <param name="flags">Font render flags.</param>
		protected void DrawText(float xpos, float ypos, Color color, string text, RenderFlags flags)
    {
			if (text==null) return;
			if (text==String.Empty) return;
			if (xpos <=0) return ;
			if (ypos <=0) return ;


      GUIGraphicsContext.Correct(ref xpos, ref ypos);
      if (GUIGraphicsContext.graphics!=null)
      {
        GUIGraphicsContext.graphics.TextRenderingHint =System.Drawing.Text.TextRenderingHint.AntiAlias;
        GUIGraphicsContext.graphics.SmoothingMode =System.Drawing.Drawing2D.SmoothingMode.HighQuality;//.AntiAlias;
        GUIGraphicsContext.graphics.DrawString(text,systemFont,new SolidBrush(color),xpos,ypos);
        return;
      }
			if (fontVertices==null) return;
			if (fontTexture==null) return;
			if (textureCoords==null) return;
			if (fontTexture.Disposed) return;
			//if (savedStateBlock==null) return;
			//if (savedStateBlock.Disposed) return;
			if (vertexBuffer==null) return;
			if (vertexBuffer.Disposed) return;
			if (GUIGraphicsContext.DX9Device==null) return;
			if (GUIGraphicsContext.DX9Device.Disposed) return;
			// Setup renderstate
			//savedStateBlock.Capture();
			//drawTextStateBlock.Apply();
			// Adjust for character spacing
			xpos -= spacingPerChar;
			xpos-=0.5f;
			float fStartX = xpos;
			ypos -=0.5f;

      int intColor = color.ToArgb();
      float yoff=(textureCoords[0,3]-textureCoords[0,1])*textureHeight;
      float fScaleX=textureWidth / textureScale;
      float fScaleY=textureHeight / textureScale;
			float fSpacing=2 * spacingPerChar;
			for (int i=0; i < text.Length;++i)
			{
        char c=text[i];
				if (c == '\n')
				{
					xpos = fStartX;
					ypos += yoff;
				}

				if (c < _StartCharacter || c >= _EndCharacter )
					continue;

        int index=c-_StartCharacter;
				float tx1 = textureCoords[index,0];
				float ty1 = textureCoords[index,1];
				float tx2 = textureCoords[index,2];
				float ty2 = textureCoords[index,3];

				float w = (tx2-tx1) * fScaleX;
				float h = (ty2-ty1) * fScaleY;

				if (xpos<0 || xpos+2 > GUIGraphicsContext.Width ||
					  ypos<0 || ypos+h > GUIGraphicsContext.Height+100)
				{
					c=' ';
				}

				if (c != ' ')
				{
					float xpos2=xpos+w;
					float ypos2=ypos+h;
          fontVertices[iv].X=xpos ;  fontVertices[iv].Y=ypos2 ; fontVertices[iv].Color=intColor;fontVertices[iv].Tu=tx1; fontVertices[iv].Tv=ty2;iv++;
          fontVertices[iv].X=xpos ;  fontVertices[iv].Y=ypos  ; fontVertices[iv].Color=intColor;fontVertices[iv].Tu=tx1; fontVertices[iv].Tv=ty1;iv++;
          fontVertices[iv].X=xpos2;  fontVertices[iv].Y=ypos2 ; fontVertices[iv].Color=intColor;fontVertices[iv].Tu=tx2; fontVertices[iv].Tv=ty2;iv++;
          fontVertices[iv].X=xpos2;  fontVertices[iv].Y=ypos  ; fontVertices[iv].Color=intColor;fontVertices[iv].Tu=tx2; fontVertices[iv].Tv=ty1;iv++;
          fontVertices[iv].X=xpos2;  fontVertices[iv].Y=ypos2 ; fontVertices[iv].Color=intColor;fontVertices[iv].Tu=tx2; fontVertices[iv].Tv=ty2;iv++;
          fontVertices[iv].X=xpos ;  fontVertices[iv].Y=ypos  ; fontVertices[iv].Color=intColor;fontVertices[iv].Tu=tx1; fontVertices[iv].Tv=ty1;iv++;

					dwNumTriangles += 2;

          if (flags!=RenderFlags.DontDiscard)
          {
            if (iv > (MaxNumfontVertices-12))
            {
              // Set the data for the vertexbuffer
              if (vertexBuffer!=null)
              {
                vertexBuffer.SetData(fontVertices, 0, LockFlags.Discard);
                //savedStateBlock.Apply();
                GUIGraphicsContext.DX9Device.SetTexture(0, fontTexture);
                GUIGraphicsContext.DX9Device.VertexFormat = CustomVertex.TransformedColoredTextured.Format;
                //GUIGraphicsContext.DX9Device.PixelShader = null;
                GUIGraphicsContext.DX9Device.SetStreamSource(0, vertexBuffer, 0);
                GUIGraphicsContext.DX9Device.DrawPrimitives(PrimitiveType.TriangleList, 0, dwNumTriangles);
								GUIGraphicsContext.DX9Device.SetTexture(0, null);
							}
              dwNumTriangles = 0;
              iv = 0;
            }
          }
				}

				xpos += w - fSpacing;
			}


      if (flags!=RenderFlags.DontDiscard)
        Present();
			// Restore the modified renderstates
			//savedStateBlock.Apply();
		}
    
		/// <summary>
		/// Measure the width of a string on the display.
		/// </summary>
		/// <param name="graphics">The graphics context.</param>
		/// <param name="text">The string that needs to be measured.</param>
		/// <param name="font">The font that needs to be used.</param>
		/// <returns>The width of the string.</returns>
		static public int MeasureDisplayStringWidth(Graphics graphics, string text,System.Drawing.Font font)
    {
      const int width = 32;

      System.Drawing.Bitmap   bitmap = new System.Drawing.Bitmap (width, 1, graphics);
      System.Drawing.SizeF    size   = graphics.MeasureString (text, font);
      System.Drawing.Graphics anagra = System.Drawing.Graphics.FromImage(bitmap);

      int measured_width = (int) size.Width;

      if (anagra != null)
      {
        anagra.Clear (Color.White);
        anagra.DrawString (text+"|", font, Brushes.Black,
          width - measured_width, -font.Height / 2);

        for (int i = width-1; i >= 0; i--)
        {
          measured_width--;
          if (bitmap.GetPixel (i, 0).R != 255)    // found a non-white pixel ?
            break;
        }
      }
      return measured_width;
    }

		/// <summary>
		/// Get the dimensions of a text string.
		/// </summary>
		/// <param name="text">The actual text.</param>
		/// <returns>The size of the rendered text.</returns>
		public void GetTextExtent(string text, ref float textwidth, ref float textheight)
		{
      textwidth  = 0.0f;
      textheight = 0.0f;

			if (null == text || text == String.Empty) return;

			float fRowWidth  = 0.0f;
			float fRowHeight = (textureCoords[0,3]-textureCoords[0,1])*textureHeight;
			textheight = fRowHeight;

			for (int i=0; i < text.Length;++i)
			{
        char c=text[i];
				if (c == '\n')
        {
          if (fRowWidth > textwidth)
            textwidth = fRowWidth;
					fRowWidth = 0.0f;
					textheight  += fRowHeight;
				}

				if (c < _StartCharacter || c >= _EndCharacter )
					continue;

				float tx1 = textureCoords[c-_StartCharacter,0];
				float tx2 = textureCoords[c-_StartCharacter,2];

				fRowWidth += (tx2-tx1)*textureWidth - 2*spacingPerChar;
      }

      if (fRowWidth > textwidth)
        textwidth = fRowWidth;
		}

		/// <summary>
		/// Cleanup any resources being used.
		/// </summary>
		public void Dispose(object sender, EventArgs e)
		{
			if (systemFont != null)
				systemFont.Dispose();

      if (fontTexture!=null)
        fontTexture.Dispose();

      if (vertexBuffer!=null)
        vertexBuffer.Dispose();
      vertexBuffer=null;
      fontTexture=null;
			systemFont = null;
      fontVertices=null;
		}

		/// <summary>
		/// Loads a font.
		/// </summary>
		/// <returns>True if loaded succesful.</returns>
		public bool Load()
		{
			Create(m_strFileName, m_FontStyle, m_iFontHeight);
			return true;
		}

		/// <summary>
		/// Initialize the device objects.
		/// </summary>
		public void InitializeDeviceObjects()
		{
      
      //textureState0 = GUIGraphicsContext.DX9Device.TextureState[0];
      //textureState1 = GUIGraphicsContext.DX9Device.TextureState[1];
      //samplerState0 = GUIGraphicsContext.DX9Device.SamplerState[0];
      //renderState = GUIGraphicsContext.DX9Device.RenderState;
      textureScale  = 1.0f; // Draw fonts into texture without scaling

      fontVertices = new CustomVertex.TransformedColoredTextured[MaxNumfontVertices];
			for (int i=0; i < fontVertices.Length; ++i) 
			{
				fontVertices[i].Rhw=1.0f;
				fontVertices[i].Z=0.0f;
			}

			// Create a directory to cache the font bitmaps
      string strCache=String.Format(@"{0}\fonts\",GUIGraphicsContext.Skin);
			try{
      System.IO.Directory.CreateDirectory(strCache);
			}
			catch(Exception){}
      strCache=String.Format(@"{0}\fonts\{1}_{2}.png",GUIGraphicsContext.Skin, m_strFontName, m_iFontHeight);
     
			// If the cached bitmap file exists load from file.
			if (System.IO.File.Exists(strCache))
      {
        bool bExists=true;
        using (Stream r = File.Open(strCache+".xml", FileMode.Open, FileAccess.Read))
        {
          // deserialize persons
          SoapFormatter c = new SoapFormatter();
          try
          {
            textureCoords = (float[,])c.Deserialize(r);
          }
          catch(Exception)
          {
            bExists=false;
          }
          int iLen=textureCoords.GetLength(0);
          if (iLen != 10+_EndCharacter-_StartCharacter)
          {
            bExists=false;
          }
          r.Close();
        }
        if (bExists)
        {
          bool SupportsCompressedTextures=Manager.CheckDeviceFormat(GUIGraphicsContext.DX9Device.DeviceCaps.AdapterOrdinal, 
                                                                    GUIGraphicsContext.DX9Device.DeviceCaps.DeviceType, 
                                                                    GUIGraphicsContext.DX9Device.DisplayMode.Format, 
                                                                    Usage.None, 
                                                                    ResourceType.Textures, 
                                                                    Format.Dxt3 );
          Format fmt=Format.Unknown;
          if (SupportsCompressedTextures) fmt=Format.Dxt3;
          spacingPerChar=(int)textureCoords[_EndCharacter-_StartCharacter,0];

          // load coords
          ImageInformation info = new ImageInformation();
          fontTexture=TextureLoader.FromFile(GUIGraphicsContext.DX9Device,
            strCache,
            0,0, //width/height
            1,//miplevels
            0,
            fmt,
            Pool.Managed,
            Filter.None,
            Filter.None,
            (int)0,
            ref info);

          textureHeight=info.Height;
          textureWidth=info.Width;
          RestoreDeviceObjects();
          Log.Write("  Loaded font:{0} height:{1} texture:{2}x{3} chars:[{4}-{5}] memleft:{6}",
              m_strFontName, m_iFontHeight,textureWidth,textureWidth, _StartCharacter,_EndCharacter,GUIGraphicsContext.DX9Device.AvailableTextureMemory.ToString());
          return;
        }
      }
      // If not generate it.
      textureCoords = new float[(10+_EndCharacter-_StartCharacter),4];

			// Create a bitmap on which to measure the alphabet
			Bitmap bmp = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			Graphics g = Graphics.FromImage(bmp);
			g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
			g.TextContrast = 0;
       
			// Establish the font and texture size
			textureScale  = 1.0f; // Draw fonts into texture without scaling

			// Calculate the dimensions for the smallest power-of-two texture which
			// can hold all the printable characters
			textureWidth = textureHeight = 256;
			for (;;)
			{
				try
				{
					// Measure the alphabet
					PaintAlphabet(g, true);
				}
				catch (System.InvalidOperationException)
				{
					// Scale up the texture size and try again
					textureWidth *= 2;
					textureHeight *= 2;
					continue;
				}

				break;
			}

			// If requested texture is too big, use a smaller texture and smaller font,
			// and scale up when rendering.
			Direct3D.Caps d3dCaps = GUIGraphicsContext.DX9Device.DeviceCaps;

			// If the needed texture is too large for the video card...
			if (textureWidth > d3dCaps.MaxTextureWidth)
			{
				// Scale the font size down to fit on the largest possible texture
				textureScale = (float)d3dCaps.MaxTextureWidth / (float)textureWidth;
				textureWidth = textureHeight = d3dCaps.MaxTextureWidth;

				for(;;)
				{
					// Create a new, smaller font
					m_iFontHeight = (int) Math.Floor(m_iFontHeight * textureScale);      
					systemFont = new System.Drawing.Font(systemFont.Name, m_iFontHeight, systemFont.Style);
                
					try
					{
						// Measure the alphabet
						PaintAlphabet(g, true);
					}
					catch (System.InvalidOperationException)
					{
						// If that still doesn't fit, scale down again and continue
						textureScale *= 0.9F;
						continue;
					}

					break;
				}
			}
      bmp.Dispose();

      Trace.WriteLine("font:"+ m_strFontName + " "+ m_strFileName+ " height:" + m_iFontHeight.ToString() +" "+textureWidth.ToString() + "x"+textureHeight.ToString());
			// Release the bitmap used for measuring and create one for drawing
      
      using (bmp = new Bitmap(textureWidth, textureHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
      {
        using (g = Graphics.FromImage(bmp))
        {
          g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
          g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
          g.TextContrast = 0;

          // Draw the alphabet
          PaintAlphabet(g, false);

          // Create a new texture for the font from the bitmap we just created
          try
          {
            fontTexture = Texture.FromBitmap(GUIGraphicsContext.DX9Device, bmp, 0, Pool.Managed);
            bmp.Save(strCache);
            textureCoords[_EndCharacter-_StartCharacter,0]=spacingPerChar;
            try
            {
              System.IO.File.Delete(strCache+".xml");
            }
            catch(Exception ){}
            using (Stream s = File.Open(strCache+".xml", FileMode.CreateNew, FileAccess.ReadWrite))
            {
              // serialize persons
              SoapFormatter b = new SoapFormatter();
              b.Serialize(s, (object)textureCoords);
              s.Close();
            }
          }
          catch(Exception ex)
          {
            string strLine=ex.Message;
          }
        }
      }
			RestoreDeviceObjects();
		}

		/// <summary>
		/// Restore the font after a device has been reset.
		/// </summary>
		public void RestoreDeviceObjects()
		{
			vertexBuffer = new VertexBuffer(typeof(CustomVertex.TransformedColoredTextured), MaxNumfontVertices,
				GUIGraphicsContext.DX9Device, Usage.WriteOnly, 0, Pool.Managed);

			Surface surf = GUIGraphicsContext.DX9Device.GetRenderTarget( 0 );
			m_bSupportsAlphaBlend = Manager.CheckDeviceFormat(GUIGraphicsContext.DX9Device.DeviceCaps.AdapterOrdinal, 
				GUIGraphicsContext.DX9Device.DeviceCaps.DeviceType, GUIGraphicsContext.DX9Device.DisplayMode.Format, 
				Usage.RenderTarget | Usage.QueryPostPixelShaderBlending, ResourceType.Surface, 
				surf.Description.Format );
/*
			// Create the state blocks for rendering text
			{
				GUIGraphicsContext.DX9Device.BeginStateBlock();
				GUIGraphicsContext.DX9Device.SetTexture(0, fontTexture);

        GUIGraphicsContext.DX9Device.RenderState.ZBufferEnable = false;

				if( m_bSupportsAlphaBlend )
				{
					GUIGraphicsContext.DX9Device.RenderState.AlphaBlendEnable = true;
					GUIGraphicsContext.DX9Device.RenderState.SourceBlend = Blend.SourceAlpha;
					GUIGraphicsContext.DX9Device.RenderState.DestinationBlend = Blend.InvSourceAlpha;
				}
				else
				{
					GUIGraphicsContext.DX9Device.RenderState.AlphaBlendEnable = false;
				}
				//GUIGraphicsContext.DX9Device.RenderState.AlphaTestEnable = true;
				//GUIGraphicsContext.DX9Device.RenderState.ReferenceAlpha = 0x08;
				//GUIGraphicsContext.DX9Device.RenderState.AlphaFunction = Compare.GreaterEqual;
				GUIGraphicsContext.DX9Device.RenderState.FillMode = FillMode.Solid;
				GUIGraphicsContext.DX9Device.RenderState.CullMode = Cull.CounterClockwise;
				GUIGraphicsContext.DX9Device.RenderState.StencilEnable = false;
				//GUIGraphicsContext.DX9Device.RenderState.Clipping = true;
				GUIGraphicsContext.DX9Device.ClipPlanes.DisableAll();
				GUIGraphicsContext.DX9Device.RenderState.VertexBlend = VertexBlend.Disable;
				GUIGraphicsContext.DX9Device.RenderState.IndexedVertexBlendEnable = false;
				GUIGraphicsContext.DX9Device.RenderState.FogEnable = false;
				//GUIGraphicsContext.DX9Device.RenderState.ColorWriteEnable = ColorWriteEnable.RedGreenBlueAlpha;
				GUIGraphicsContext.DX9Device.TextureState[0].ColorOperation = TextureOperation.Modulate;
				GUIGraphicsContext.DX9Device.TextureState[0].ColorArgument1 = TextureArgument.TextureColor;
				GUIGraphicsContext.DX9Device.TextureState[0].ColorArgument2 = TextureArgument.Diffuse;
				GUIGraphicsContext.DX9Device.TextureState[0].AlphaOperation = TextureOperation.Modulate;
				GUIGraphicsContext.DX9Device.TextureState[0].AlphaArgument1 = TextureArgument.TextureColor;
				GUIGraphicsContext.DX9Device.TextureState[0].AlphaArgument2 = TextureArgument.Diffuse;
				GUIGraphicsContext.DX9Device.TextureState[0].TextureCoordinateIndex = 0;
				GUIGraphicsContext.DX9Device.TextureState[0].TextureTransform = TextureTransform.Disable; // REVIEW
				GUIGraphicsContext.DX9Device.TextureState[1].ColorOperation = TextureOperation.Disable;
				GUIGraphicsContext.DX9Device.TextureState[1].AlphaOperation = TextureOperation.Disable;
				GUIGraphicsContext.DX9Device.SamplerState[0].MinFilter = TextureFilter.None;
				GUIGraphicsContext.DX9Device.SamplerState[0].MagFilter = TextureFilter.None;
				GUIGraphicsContext.DX9Device.SamplerState[0].MipFilter = TextureFilter.None;

				  savedStateBlock = GUIGraphicsContext.DX9Device.EndStateBlock();
			}*/

		}

		/// <summary>
		/// Attempt to draw the systemFont alphabet onto the provided texture
		/// graphics.
		/// </summary>
		/// <param name="g">Graphics object on which to draw and measure the letters</param>
		/// <param name="measureOnly">If set, the method will test to see if the alphabet will fit without actually drawing</param>
		public void PaintAlphabet(Graphics g, bool measureOnly)
		{
			string str;
			float x = 0;
			float y = 0;
			Point p = new Point(0, 0);
			Size size = new Size(0,0);
            
			// Calculate the spacing between characters based on line height
			size = g.MeasureString(" ", systemFont).ToSize();
			x = spacingPerChar = (int) Math.Ceiling(size.Height * 0.3);

			for (char c = (char)_StartCharacter; c < (char)_EndCharacter; c++)
			{
				str = c.ToString();
				// We need to do some things here to get the right sizes.  The default implemententation of MeasureString
				// will return a resolution independant size.  For our height, this is what we want.  However, for our width, we 
				// want a resolution dependant size.
				Size resSize = g.MeasureString(str, systemFont).ToSize();
				size.Height = resSize.Height + 1;

				// Now the Resolution independent width
				if (c != ' ') // We need the special case here because a space has a 0 width in GenericTypoGraphic stringformats
				{
					resSize = g.MeasureString(str, systemFont, p, StringFormat.GenericTypographic).ToSize();
					size.Width = resSize.Width;
				}
				else
					size.Width = resSize.Width;

				if ((x + size.Width + spacingPerChar) > textureWidth)
				{
					x = spacingPerChar;
					y += size.Height;
				}

				// Make sure we have room for the current character
				if ((y + size.Height) > textureHeight)
					throw new System.InvalidOperationException("Texture too small for alphabet");
                
				if (!measureOnly)
				{
					if (c != ' ') // We need the special case here because a space has a 0 width in GenericTypoGraphic stringformats
						g.DrawString(str, systemFont, Brushes.White, new Point((int)x, (int)y), StringFormat.GenericTypographic);
					else
						g.DrawString(str, systemFont, Brushes.White, new Point((int)x, (int)y));
					textureCoords[c-_StartCharacter,0] = ((float) (x + 0           - spacingPerChar)) / textureWidth;
					textureCoords[c-_StartCharacter,1] = ((float) (y + 0           + 0)) / textureHeight;
					textureCoords[c-_StartCharacter,2] = ((float) (x + size.Width  + spacingPerChar)) / textureWidth;
					textureCoords[c-_StartCharacter,3] = ((float) (y + size.Height + 0)) / textureHeight;
				}

				x += size.Width + (2 * spacingPerChar);
			}
		}
	}
}
