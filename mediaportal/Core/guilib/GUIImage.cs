using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

using System.Collections;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D=Microsoft.DirectX.Direct3D;


namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// A GUIControl for displaying Images.
	/// </summary>
	public class GUIImage :  GUIControl
	{
		[XMLSkinElement("colorkey")] private long                    m_dwColorKey=0;
		private VertexBuffer						m_vbBuffer=null;
		[XMLSkinElement("texture")] private string m_strFileName="";
		/// <summary>The width of the current texture.</summary>
		private int                     m_iTextureWidth=0;
		private int                     m_iTextureHeight=0;
		/// <summary>The width of the image containing the textures.</summary>
		private int                     m_iImageWidth=0;
		private int                     m_iImageHeight=0;
		private int                     m_iBitmap=0;
		private int                     m_dwItems=0;
		private int                     m_iCurrentLoop=0;
		private int										  m_iCurrentImage=0;
		[XMLSkinElement("keepaspectratio")] private bool    m_bKeepAspectRatio=false;
		private ArrayList								m_vecTextures = new ArrayList();

    //TODO GIF PALLETTE
    //private PaletteEntry						m_pPalette=null;
		/// <summary>The width of in which the texture will be rendered after scaling texture.</summary>
		private int                     m_iRenderWidth=0;
		private int                     m_iRenderHeight=0;
		private bool										m_bWasVisible=false;
    private System.Drawing.Image    m_image=null;
    private Rectangle               m_destRect;
    string                          m_strTextureFileName="";
    int                             g_nAnisotropy=0;
	  [XMLSkinElement("filtered")] bool	m_bFiltering=true;
    [XMLSkinElement("centered")] bool m_bCentered=false;
    string                          m_strTxt;
    DateTime                        m_AnimationTime=DateTime.MinValue;
    bool                            ContainsProperty=false;
    StateBlock                      savedStateBlock;
		public GUIImage (int dwParentID) : base(dwParentID)
		{
		}
		/// <summary>
		/// The constructor of the GUIImage class.
		/// </summary>
		/// <param name="dwParentID">The parent of this GUIImage control.</param>
		/// <param name="dwControlId">The ID of this GUIImage control.</param>
		/// <param name="dwPosX">The X position of this GUIImage control.</param>
		/// <param name="dwPosY">The Y position of this GUIImage control.</param>
		/// <param name="dwWidth">The width of this GUIImage control.</param>
		/// <param name="dwHeight">The height of this GUIImage control.</param>
		/// <param name="strTexture">The filename of the texture of this GUIImage control.</param>
		/// <param name="dwColorKey">The color that indicates transparancy.</param>
		public GUIImage(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight, string strTexture,long dwColorKey)
			:base(dwParentID, dwControlId,dwPosX, dwPosY, dwWidth, dwHeight)
		{		
			m_colDiffuse	= 0xFFFFFFFF;  
			m_strFileName=strTexture;
			m_iTextureWidth=0;
			m_iTextureHeight=0;
			m_dwColorKey=dwColorKey;
			m_iBitmap=0;
			
			m_iCurrentImage=0;
			m_bKeepAspectRatio=false;
			m_iCurrentLoop=0;
			m_iImageWidth = 0;
			m_iImageHeight = 0;
			FinalizeConstruction();
			
		}
		public override void ScaleToScreenResolution()
		{
			if (m_strFileName != "-" && m_strFileName != "")
			{
				if (m_dwWidth == 0 || m_dwHeight == 0)
				{		
					try
					{
						string strFileNameTemp="";
						if (!System.IO.File.Exists(m_strFileName))
						{
							if (m_strFileName[1] != ':')
								strFileNameTemp = GUIGraphicsContext.Skin + @"\media\" + m_strFileName;
						}
						using (Image img = Image.FromFile(strFileNameTemp))
						{
							if (0 == m_dwWidth)  m_dwWidth = img.Width;
							if (0 == m_dwHeight) m_dwHeight = img.Height;
						}
					}
					catch (Exception)
					{
					}
				}
			}
			base.ScaleToScreenResolution();
		}
		public override void FinalizeConstruction()
		{
			base.FinalizeConstruction ();
	
			m_dwItems=1;
			m_bWasVisible = IsVisible;
			
			m_iRenderWidth=m_dwWidth;
			m_iRenderHeight=m_dwHeight;
      if (m_strFileName.IndexOf("#")>=0) ContainsProperty=true;
		}
	
		/// <summary>
		/// Get/Set the TextureWidth
		/// </summary>
		public int TextureWidth
		{ 
			get { return m_iTextureWidth;}
			set { m_iTextureWidth=value;Update();}
		}

		/// <summary>
		/// Get/Set the TextureHeight
		/// </summary>
		public int TextureHeight
		{
			get { return m_iTextureHeight;}
			set { m_iTextureHeight=value;Update();}
		}

		/// <summary>
		/// Get the filename of the texture.
		/// </summary>
		public string FileName
		{
			get {return m_strFileName;}
		}
		
		/// <summary>
		/// Get the transparent color.
		/// </summary>
		public long	ColorKey 
		{
			get {return m_dwColorKey;}
		}

		/// <summary>
		/// Get/Set if the aspectratio of the texture needs to be preserved during rendering.
		/// </summary>
		public bool KeepAspectRatio
		{
			get { return m_bKeepAspectRatio;}
			set { m_bKeepAspectRatio=value;}
		}

		/// <summary>
		/// Get the width in which the control is rendered.
		/// </summary>
		public int RenderWidth
		{
			get { return m_iRenderWidth;}
		}

		/// <summary>
		/// Get the height in which the control is rendered.
		/// </summary>
		public int RenderHeight
		{
			get { return m_iRenderHeight;}
		}

		/// <summary>
		/// Returns if the control can have the focus.
		/// </summary>
		/// <returns>False</returns>
		public override bool CanFocus() 
		{
			return false;
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="iBitmap"></param>
		public void Select(int iBitmap)
		{
			// TODO Figure out what this is used for.
			m_iBitmap=iBitmap;
			Update();
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="iItems"></param>
		public void SetItems(int iItems)
		{
			// TODO The SetItems method does not seem to be used at all, do we need it?
			m_dwItems=iItems;
		}


		/// <summary>
		/// 
		/// </summary>
		protected void Process()
		{
			// If the number of textures that correspond to this control is lower than or equal to 1 do not change the texture.
			if (m_vecTextures.Count <= 1)
				return;
			
			// If the GUIImage has not been visible before start at the first texture in the m_vecTextures.
			if (!m_bWasVisible)
			{
				m_iCurrentLoop = 0;
				m_iCurrentImage = 0;
				m_bWasVisible = true;
				return;
			}
			
      if (m_iCurrentImage >= m_vecTextures.Count) 
        m_iCurrentImage =0;

      CachedTexture.Frame frame=(CachedTexture.Frame)m_vecTextures[m_iCurrentImage];
      string strFile=m_strFileName;
      if (ContainsProperty)
        strFile=GUIPropertyManager.Parse(m_strFileName);
			
      // Check the delay.
			int dwDelay    = frame.Duration;
      int iMaxLoops=0;

			// Default delay = 100;
			if (0==dwDelay) dwDelay=100;
			
      TimeSpan ts = DateTime.Now-m_AnimationTime;
      if (ts.TotalMilliseconds> dwDelay)
			{
        m_AnimationTime=DateTime.Now;

        // Reset the current image
				if (m_iCurrentImage+1 >= m_vecTextures.Count )
				{
					// Check if another loop is required
					if (iMaxLoops > 0)
					{
						// Go to the next loop
						if (m_iCurrentLoop+1 < iMaxLoops)
						{
							m_iCurrentLoop++;
							m_iCurrentImage=0;
						}
					}
					else
					{
						// 0 == loop forever
						m_iCurrentImage=0;
					}
				}
				// Switch to the next image.
				else
				{
					m_iCurrentImage++;
				}
			}
		}
		
		/// <summary>
		/// An action occured on the GUIImage. (Does nothing).
		/// </summary>
		/// <param name="action">The action.</param>
		public override void OnAction(Action action)
		{
		}

		/// <summary>
		/// A message was recieved by the GUIImage. (base class handles everything)
		/// </summary>
		/// <param name="message">The message.</param>
		/// <returns></returns>
		public override bool OnMessage(GUIMessage message)
		{
			return base.OnMessage(message);
		}

		/// <summary>
		/// PreAllocates the DirectX resources (E.g., preload the textures). 
		/// </summary>
		public override void PreAllocResources()
		{
		}

		/// <summary>
		/// Allocate the DirectX resources needed for rendering this GUIImage.
		/// </summary>
		public override void AllocResources()
		{
      CreateStateBlock();
      g_nAnisotropy=GUIGraphicsContext.DX9Device.DeviceCaps.MaxAnisotropy;
			if (m_strFileName=="-") return;

			m_iCurrentImage=0;
			m_iCurrentLoop=0;

      string strFile=m_strFileName;
      if (ContainsProperty)
        strFile=GUIPropertyManager.Parse(m_strFileName);

			int iImages = GUITextureManager.Load(strFile, m_dwColorKey,m_iRenderWidth,m_iTextureHeight);
			if (0==iImages) return;
			for (int i=0; i < iImages; i++)
			{
				CachedTexture.Frame frame;
				frame=GUITextureManager.GetTexture(strFile,i, out m_iTextureWidth,out m_iTextureHeight);//,m_pPalette);
				if (frame!=null) m_vecTextures.Add(frame);
			}

  
			
      // Create a vertex buffer for rendering the image
      m_vbBuffer = new VertexBuffer(typeof(CustomVertex.TransformedColoredTextured),
                                    4, GUIGraphicsContext.DX9Device, 
                                    Usage.WriteOnly, CustomVertex.TransformedColoredTextured.Format, 
                                    Pool.Managed);

			// Set state to render the image
      Update();
      CreateStateBlock();
		}

		/// <summary>
		/// Free the DirectX resources needed for rendering this GUIImage.
		/// </summary>
		public override void FreeResources()
		{
      lock (this)
      {
        m_strTextureFileName="";
        if (m_vbBuffer!=null)
        {
          if (!m_vbBuffer.Disposed) m_vbBuffer.Dispose();
          m_vbBuffer=null;
        }

        m_image=null;
        m_vecTextures.Clear();
        m_iCurrentImage=0;
        m_iCurrentLoop=0;
        m_iImageWidth=0;
        m_iImageHeight=0;
        m_iTextureWidth=0;
        m_iTextureHeight=0;
        if (savedStateBlock!=null) savedStateBlock.Dispose();
        savedStateBlock=null;
      }
		}

		/// <summary>
		/// Sets the state to render the image
		/// </summary>
    protected override void Update()
    {
      if (m_vbBuffer==null) return;
      if (m_vecTextures.Count==0) return;
      float x=(float)m_dwPosX;
      float y=(float)m_dwPosY;

      CachedTexture.Frame frame=(CachedTexture.Frame)m_vecTextures[m_iCurrentImage];
      Direct3D.Texture texture=frame.Image;
      if (texture==null)
      {
        return;
      }

      if (texture.Disposed)
      {
        FreeResources();
        return;
      }
      // Set the m_iImageWidth and m_iImageHeight based on the Direct3D surface for the texture.
      if (0==m_iImageWidth|| 0==m_iImageHeight)
      {
        Direct3D.SurfaceDescription desc;
        desc=texture.GetLevelDescription(0);
        m_iImageWidth = desc.Width;
        m_iImageHeight = desc.Height;
      }

      // Calculate the m_iTextureWidth and m_iTextureHeight based on the m_iImageWidth and m_iImageHeight
      if (0==m_iTextureWidth|| 0==m_iTextureHeight)
      {
        m_iTextureWidth  = (int)Math.Round( ((float)m_iImageWidth) / ((float)m_dwItems) );
        m_iTextureHeight = m_iImageHeight;

        if (m_iTextureHeight > (int)GUIGraphicsContext.Height )
          m_iTextureHeight = (int)GUIGraphicsContext.Height;

        if (m_iTextureWidth > (int)GUIGraphicsContext.Width )
          m_iTextureWidth = (int)GUIGraphicsContext.Width;
      }
			
      // If there are multiple items in the GUIImage the m_iTextureWidth is equal to the m_dwWidth
      if (m_dwWidth >0 && m_dwItems>1)
      {
        m_iTextureWidth=(int)m_dwWidth;
      }

      // Initialize the with of the control based on the texture width
      if (m_dwWidth==0) 
        m_dwWidth=m_iTextureWidth;
      // Initialize the height of the control based on the texture height
      if (m_dwHeight==0) 
        m_dwHeight=m_iTextureHeight;


      float nw =(float)m_dwWidth;
      float nh =(float)m_dwHeight;

      //TODO: Is this todo still needed? keepaspect ratio			
      if (m_bKeepAspectRatio && m_iTextureWidth!=0 && m_iTextureHeight!=0)
      {
        // TODO: remove or complete HDTV_1080i code
        //int iResolution=g_stSettings.m_ScreenResolution;
        float fSourceFrameRatio = ((float)m_iTextureWidth) / ((float)m_iTextureHeight);
        float fOutputFrameRatio = fSourceFrameRatio / GUIGraphicsContext.PixelRatio; 
        //if (iResolution == HDTV_1080i) fOutputFrameRatio *= 2;

        // maximize the thumbnails width
        float fNewWidth  = (float)m_dwWidth;
        float fNewHeight = fNewWidth/fOutputFrameRatio;

        // make sure the height is not larger than the maximum
        if (fNewHeight > m_dwHeight)
        {
          fNewHeight = (float)m_dwHeight;
          fNewWidth = fNewHeight*fOutputFrameRatio;
        }
        // this shouldnt happen, but just make sure that everything still fits onscreen
        if (fNewWidth > m_dwWidth || fNewHeight > m_dwHeight)
        {
          fNewWidth=(float)m_dwWidth;
          fNewHeight=(float)m_dwHeight;
        }
        nw=fNewWidth;
        nh=fNewHeight;
      }
			
      // 
      m_iRenderWidth=(int)Math.Round(nw);
      m_iRenderHeight=(int)Math.Round(nh);

      // reposition if calibration of the UI has been done
      if (CalibrationEnabled)
      {
        GUIGraphicsContext.Correct(ref x,ref y);
      }

      if (m_bCentered)
      {
        x += ((((float)m_dwWidth)-nw)/2.0f);
        y += ((((float)m_dwHeight)-nh)/2.0f); 
      }

      int iXOffset=(int)(m_iBitmap*m_dwWidth);
      float uoffs = ((float)(m_iBitmap * m_dwWidth)) / ((float)m_iImageWidth);
      float u = ((float)m_iTextureWidth)  / ((float)m_iImageWidth);
      float v = ((float)m_iTextureHeight) / ((float)m_iImageHeight);

      CustomVertex.TransformedColoredTextured[] verts = (CustomVertex.TransformedColoredTextured[])m_vbBuffer.Lock(0,0);
      verts[0].X= x- 0.5f; verts[0].Y=y+nh- 0.5f; verts[0].Z= 0.0f; verts[0].Rhw=1.0f ;
      verts[0].Color = (int)m_colDiffuse;
      verts[0].Tu = uoffs;
      verts[0].Tv = v;

      verts[1].X= x- 0.5f; verts[1].Y= y- 0.5f; verts[1].Z= 0.0f; verts[1].Rhw= 1.0f;
      verts[1].Color = (int)m_colDiffuse;
      verts[1].Tu = uoffs;
      verts[1].Tv = 0.0f;

      verts[2].X=  x+nw- 0.5f; verts[2].Y=y+nh- 0.5f;verts[1].Z=  0.0f; verts[2].Rhw= 1.0f;
      verts[2].Color = (int)m_colDiffuse;
      verts[2].Tu = uoffs+u;
      verts[2].Tv = v;

      verts[3].X= x+nw- 0.5f; verts[3].Y= y- 0.5f; verts[3].Z=0.0f; verts[3].Rhw=1.0f ;
      verts[3].Color = (int)m_colDiffuse;
      verts[3].Tu = uoffs+u;
      verts[3].Tv = 0.0f;
      m_vbBuffer.Unlock();
       
			// update the destination rectangle
      m_destRect=new Rectangle((int)x,(int)y,(int)nw,(int)nh);
		}

		/// <summary>
		/// Renders the GUIImage
		/// </summary>
		public override void Render()
    {
      lock (this)
      {
        // Do not render if not visible
        if (false==IsVisible)
        {
          m_bWasVisible = false;
          return;
        }

        if (m_strFileName==null) return;
        if (m_strFileName==String.Empty) return;

        if (ContainsProperty)
        {
          m_strTxt=GUIPropertyManager.Parse(m_strFileName);
          if (m_strTextureFileName != m_strTxt || 0==m_vecTextures.Count)
          {
            FreeResources();
            m_strTextureFileName =m_strTxt;
            if (m_strTxt.Length==0)
            {
              IsVisible=false;
              return;
            }
            IsVisible=true;
            AllocResources();
            Update();
          }
        }

        
  			
        // Do not render if there are not textures
        if (m_vecTextures==null) 
          return;
        if (0==m_vecTextures.Count)
          return ;
        // Do not render if there is no vertex buffer
        if (null==m_vbBuffer)
          return ;
  			
  			
        if (!GUIGraphicsContext.ShowBackground)
        {
          if (m_iRenderWidth==GUIGraphicsContext.Width && m_iRenderHeight==GUIGraphicsContext.Height)
          {
            if (GUIGraphicsContext.IsPlaying && GUIGraphicsContext.IsPlayingVideo)
            {
              return;
            }
          }
        }

        if (GUIGraphicsContext.graphics!=null)
        {
          // If the Image is not loaded, load the Image
          if (m_image==null)
          {
            string strFileName=m_strFileName;
            if (ContainsProperty)
              strFileName=GUIPropertyManager.Parse(m_strFileName);
            if (strFileName != "-")
            {
              if (!System.IO.File.Exists(strFileName))
              {
                if (strFileName[1]!=':')
                  strFileName=GUIGraphicsContext.Skin+@"\media\"+strFileName;
              }
              m_image= GUITextureManager.GetImage(strFileName);
            }
          }
          // Draw the image
          if (m_image!=null)
          {
            GUIGraphicsContext.graphics.CompositingQuality=System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            GUIGraphicsContext.graphics.CompositingMode=System.Drawing.Drawing2D.CompositingMode.SourceOver;
            GUIGraphicsContext.graphics.InterpolationMode=System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            GUIGraphicsContext.graphics.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            try
            {
              GUIGraphicsContext.graphics.DrawImage(m_image,m_destRect);
            }
            catch(Exception)
            {
            }
            return;        
          }
        }
        if (GUIGraphicsContext.DX9Device==null) return;
        if (GUIGraphicsContext.DX9Device.Disposed) return;

        if (m_vecTextures.Count != 1)
          Process();
        if (m_iCurrentImage< 0 || m_iCurrentImage >=m_vecTextures.Count) return;
        CachedTexture.Frame frame=(CachedTexture.Frame)m_vecTextures[m_iCurrentImage];
        if (frame==null) return;
        Direct3D.Texture texture=frame.Image;
        if (texture==null)
        {
          FreeResources();
          return;
        }
        if (texture.Disposed)
        {
          FreeResources();
          return;
        }
        
        // Render the image
        if (savedStateBlock!=null)
        {
          if (savedStateBlock.Disposed) savedStateBlock=null;
        }
        if (savedStateBlock==null)
        {
          CreateStateBlock();
        }
        if (savedStateBlock!=null)
        {
          savedStateBlock.Apply();
          GUIGraphicsContext.DX9Device.SetTexture( 0, texture);
          GUIGraphicsContext.DX9Device.SetStreamSource( 0, m_vbBuffer, 0);
          GUIGraphicsContext.DX9Device.VertexFormat = CustomVertex.TransformedColoredTextured.Format;
          GUIGraphicsContext.DX9Device.DrawPrimitives( PrimitiveType.TriangleStrip, 0, 2 );
        }
        // unset the texture and palette or the texture caching crashes because the runtime still has a reference
        //GUIGraphicsContext.DX9Device.SetTexture( 0, null);
      }
		}

		/// <summary>
		/// Set the filename of the texture and allocated the DirectX resources for this GUIImage.
		/// </summary>
		/// <param name="strFileName"></param>
		public void SetFileName(string strFileName)
		{
      if (m_strFileName==strFileName) return;
			m_strFileName=strFileName;
      if (m_strFileName.IndexOf("#")>=0) ContainsProperty=true;
      else ContainsProperty=false;
			FreeResources();
      AllocResources();
		}
    
		/// <summary>
		/// Gets the rectangle in which this GUIImage is rendered.
		/// </summary>
    public Rectangle rect
    {
      get { return m_destRect;}
    }

    /// <summary>
    /// Property to enable/disable filtering
    /// </summary>
		public bool Filtering
		{
			get { return m_bFiltering;}
			set {m_bFiltering=value;CreateStateBlock();}
    }

    /// <summary>
    /// Property which indicates if the image should be centered in the
    /// given (x,y)-(x+width,y+height) rectangles
    /// </summary>
    public bool Centered
    {
      get { return m_bCentered;}
      set {m_bCentered=value;}
    }

    public void Refresh()
    {
      Update();
    }
    public bool Allocated
    {
      get
      {
        if (FileName.Length==0) return false;
        if (FileName.Equals("-") ) return false;
        return true;
      }
    }
    void CreateStateBlock()
    {
      
      lock (this)
      {
        if (savedStateBlock!=null)
        {
          savedStateBlock.Dispose();
        }
        savedStateBlock=null;
        bool supportsAlphaBlend = Manager.CheckDeviceFormat(
          GUIGraphicsContext.DX9Device.DeviceCaps.AdapterOrdinal, 
          GUIGraphicsContext.DX9Device.DeviceCaps.DeviceType, 
          GUIGraphicsContext.DX9Device.DisplayMode.Format, 
          Usage.RenderTarget | Usage.QueryPostPixelShaderBlending, ResourceType.Textures, 
          Format.A8R8G8B8);
        bool supportsFiltering=Manager.CheckDeviceFormat(
          GUIGraphicsContext.DX9Device.DeviceCaps.AdapterOrdinal, 
          GUIGraphicsContext.DX9Device.DeviceCaps.DeviceType, 
          GUIGraphicsContext.DX9Device.DisplayMode.Format, 
          Usage.RenderTarget | Usage.QueryFilter, ResourceType.Textures, 
          Format.A8R8G8B8);

        GUIGraphicsContext.DX9Device.BeginStateBlock();
        
          
        


        GUIGraphicsContext.DX9Device.TextureState[0].ColorOperation =Direct3D.TextureOperation.Modulate;
        GUIGraphicsContext.DX9Device.TextureState[0].ColorArgument1 =Direct3D.TextureArgument.TextureColor;
        GUIGraphicsContext.DX9Device.TextureState[0].ColorArgument2 =Direct3D.TextureArgument.Diffuse;
  				
        GUIGraphicsContext.DX9Device.TextureState[0].AlphaOperation =Direct3D.TextureOperation.Modulate;
  				
        GUIGraphicsContext.DX9Device.TextureState[0].AlphaArgument1 =Direct3D.TextureArgument.TextureColor;
        GUIGraphicsContext.DX9Device.TextureState[0].AlphaArgument2 =Direct3D.TextureArgument.Diffuse;
        GUIGraphicsContext.DX9Device.TextureState[1].ColorOperation =Direct3D.TextureOperation.Disable;
        GUIGraphicsContext.DX9Device.TextureState[1].AlphaOperation =Direct3D.TextureOperation.Disable ;

        if (m_bFiltering)
        { 
          if (supportsFiltering)
          {
            GUIGraphicsContext.DX9Device.SamplerState[0].MinFilter=TextureFilter.Linear;
            GUIGraphicsContext.DX9Device.SamplerState[0].MagFilter=TextureFilter.Linear;
            GUIGraphicsContext.DX9Device.SamplerState[0].MipFilter=TextureFilter.Linear;
            GUIGraphicsContext.DX9Device.SamplerState[0].MaxAnisotropy=g_nAnisotropy;
    	      
            GUIGraphicsContext.DX9Device.SamplerState[1].MinFilter=TextureFilter.Linear;
            GUIGraphicsContext.DX9Device.SamplerState[1].MagFilter=TextureFilter.Linear;
            GUIGraphicsContext.DX9Device.SamplerState[1].MipFilter=TextureFilter.Linear;
            GUIGraphicsContext.DX9Device.SamplerState[1].MaxAnisotropy=g_nAnisotropy;
          }
          else
          {
            GUIGraphicsContext.DX9Device.SamplerState[0].MinFilter=TextureFilter.Point;
            GUIGraphicsContext.DX9Device.SamplerState[0].MagFilter=TextureFilter.Point;
            GUIGraphicsContext.DX9Device.SamplerState[0].MipFilter=TextureFilter.Point;
    	      
            GUIGraphicsContext.DX9Device.SamplerState[1].MinFilter=TextureFilter.Point;
            GUIGraphicsContext.DX9Device.SamplerState[1].MagFilter=TextureFilter.Point;
            GUIGraphicsContext.DX9Device.SamplerState[1].MipFilter=TextureFilter.Point;
          }
        }
        else
        {
          GUIGraphicsContext.DX9Device.SamplerState[0].MinFilter=TextureFilter.None;
          GUIGraphicsContext.DX9Device.SamplerState[0].MagFilter=TextureFilter.None;
          GUIGraphicsContext.DX9Device.SamplerState[0].MipFilter=TextureFilter.None;
          GUIGraphicsContext.DX9Device.SamplerState[1].MinFilter=TextureFilter.None;
          GUIGraphicsContext.DX9Device.SamplerState[1].MagFilter=TextureFilter.None;
          GUIGraphicsContext.DX9Device.SamplerState[1].MipFilter=TextureFilter.None;
        }
        GUIGraphicsContext.DX9Device.RenderState.ZBufferEnable=false;
        GUIGraphicsContext.DX9Device.RenderState.FogEnable=false;
        GUIGraphicsContext.DX9Device.RenderState.FogTableMode=Direct3D.FogMode.None;
        GUIGraphicsContext.DX9Device.RenderState.FillMode=Direct3D.FillMode.Solid;
        GUIGraphicsContext.DX9Device.RenderState.CullMode=Direct3D.Cull.CounterClockwise;
        if (supportsAlphaBlend)
        {
          GUIGraphicsContext.DX9Device.RenderState.AlphaBlendEnable=true;
          GUIGraphicsContext.DX9Device.RenderState.SourceBlend=Direct3D.Blend.SourceAlpha;
          GUIGraphicsContext.DX9Device.RenderState.DestinationBlend=Direct3D.Blend.InvSourceAlpha;
        }
        else
        {
          GUIGraphicsContext.DX9Device.RenderState.AlphaBlendEnable=false;
        }
        savedStateBlock = GUIGraphicsContext.DX9Device.EndStateBlock();
      }
    }
	}
}
