using System;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// Summary description for GUISMSInputControl.
	/// </summary>
	public class GUISMSInputControl:GUIControl
  {    
    // How often (per second) the caret blinks
    const float fCARET_BLINK_RATE = 1.0f;
    // During the blink period, the amount the caret is visible. 0.5 equals
    // half the time, 0.75 equals 3/4ths of the time, etc.
    const float fCARET_ON_RATIO = 0.75f;

    GUIFont								          m_pFont=null;
    GUIFont								          m_pFont2=null;
    GUIFont								          m_pTextBoxFont=null;
    [XMLSkinElement("font")]			  protected string	  m_strFontName="font14";
    [XMLSkinElement("font2")]			  protected string	  m_strFontName2="font13";
    [XMLSkinElement("textcolor")]	  protected long  	  m_dwTextColor=0xFFFFFFFF;
    [XMLSkinElement("textcolor2")]	protected long      m_dwTextColor2=0xFFFFFFFF;

    [XMLSkinElement("textboxFont")]			protected string	  m_strTextBoxFontName="font13";
    [XMLSkinElement("textboxXpos")]	    protected int  	    m_dwTextBoxXpos=200;
    [XMLSkinElement("textboxYpos")]	    protected int       m_dwTextBoxYpos=300;
    [XMLSkinElement("textboxWidth")]	  protected int       m_dwTextBoxWidth=100;
    [XMLSkinElement("textboxHeight")]	  protected int       m_dwTextBoxHeight=30;
    [XMLSkinElement("textboxColor")]	  protected long      m_dwTextBoxColor=0xFFFFFFFF;
    [XMLSkinElement("textboxBgColor")]	protected long      m_dwTextBoxBgColor=0xFFFFFFFF;
    protected string m_strData="";
    protected int    m_iPos=0;
    DateTime      m_CaretTimer=DateTime.Now;

		public GUISMSInputControl(int dwParentID) : base(dwParentID)
		{
		}

    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction ();
      if (m_strFontName!="" && m_strFontName!="-")
        m_pFont=GUIFontManager.GetFont(m_strFontName);
      
      if (m_strFontName2!="" && m_strFontName2!="-")
        m_pFont2=GUIFontManager.GetFont(m_strFontName2);
      
      if (m_strTextBoxFontName!="" && m_strTextBoxFontName!="-")
        m_pTextBoxFont=GUIFontManager.GetFont(m_strTextBoxFontName);
    }

    public override void ScaleToScreenResolution()
    {
      base.ScaleToScreenResolution ();
      GUIGraphicsContext.ScaleHorizontal(ref m_dwTextBoxXpos);
      GUIGraphicsContext.ScaleVertical(ref m_dwTextBoxYpos);
      GUIGraphicsContext.ScaleHorizontal(ref m_dwTextBoxWidth);
      GUIGraphicsContext.ScaleVertical(ref m_dwTextBoxHeight);
    }
    
    public override void AllocResources()
    {
      base.AllocResources ();
      m_CaretTimer=DateTime.Now;
      m_strData="";
      m_iPos=0;
    }
    
    public override void FreeResources()
    {
      base.FreeResources ();
    }
    
    public override bool OnMessage(GUIMessage message)
    {
      return base.OnMessage (message);
    }
    
    public override void OnAction(Action action)
    {
      base.OnAction (action);
    }
    
    public override void Render()
    {
      DrawInput();
      DrawTextBox();
      DrawText();
    }

    void DrawInput()
    {
      int posY=m_dwPosY;
      int step=20;
      GUIGraphicsContext.ScaleVertical(ref step);
      m_pFont.DrawText (m_dwPosX,posY,m_dwTextColor ," 1     2       3",GUIControl.Alignment.ALIGN_LEFT); posY+=step;
      m_pFont2.DrawText(m_dwPosX,posY,m_dwTextColor2," _    abc    def",GUIControl.Alignment.ALIGN_LEFT); posY+=step;

      posY+=step;
      m_pFont.DrawText (m_dwPosX,posY,m_dwTextColor ," 4     5      6" ,GUIControl.Alignment.ALIGN_LEFT);posY+=step;
      m_pFont2.DrawText(m_dwPosX,posY,m_dwTextColor2,"ghi   jkl    mno",GUIControl.Alignment.ALIGN_LEFT);posY+=step;

      posY+=step;
      m_pFont.DrawText (m_dwPosX,posY,m_dwTextColor ," 7     8      9" ,GUIControl.Alignment.ALIGN_LEFT);posY+=step;
      m_pFont2.DrawText(m_dwPosX,posY,m_dwTextColor2,"pqrs tuv wxyz",GUIControl.Alignment.ALIGN_LEFT);posY+=step;
    }

    void DrawTextBox() 
    {
      long lColor=m_dwTextBoxBgColor;
      GUIGraphicsContext.DX9Device.SetTexture( 0, null );
      GUIGraphicsContext.DX9Device.TextureState[0].ColorOperation =Direct3D.TextureOperation.SelectArg1;
      GUIGraphicsContext.DX9Device.TextureState[0].ColorArgument1 =Direct3D.TextureArgument.TFactor;
      GUIGraphicsContext.DX9Device.VertexFormat=CustomVertex.TransformedColored.Format;
      GUIGraphicsContext.DX9Device.RenderState.AlphaBlendEnable=false;
      VertexBuffer vertexBuffer = new VertexBuffer(typeof(CustomVertex.TransformedColored),4, GUIGraphicsContext.DX9Device, 0, CustomVertex.TransformedColored.Format, Pool.Managed);

      int x1=m_dwTextBoxXpos, x2=x1+m_dwTextBoxWidth;
      int y1=m_dwTextBoxYpos,  y2=y1+m_dwTextBoxHeight;

      CustomVertex.TransformedColored[] verts = (CustomVertex.TransformedColored[])vertexBuffer.Lock(0,0);
      verts[0].X=x1-0.5f  ;verts[0].Y= y2-0.5f;verts[0].Z= 1.0f;verts[0].Rhw=1.0f;
      verts[1].X=x1-0.5f  ;verts[1].Y= y1-0.5f;verts[1].Z= 1.0f;verts[1].Rhw=1.0f;
      verts[2].X=x2- 0.5f;verts[2].Y= y2-0.5f;verts[2].Z= 1.0f;verts[2].Rhw=1.0f;
      verts[3].X=x2-0.5f ;verts[3].Y= y1-0.5f;verts[3].Z= 1.0f;verts[3].Rhw=1.0f;
      verts[0].Color=(int)lColor;
      verts[1].Color=(int)lColor;
      verts[2].Color=(int)lColor;
      verts[3].Color=(int)lColor;
      vertexBuffer.Unlock();
      GUIGraphicsContext.DX9Device.SetStreamSource( 0, vertexBuffer, 0);
      GUIGraphicsContext.DX9Device.RenderState.TextureFactor=(int)0xe0e0e0 ;
      GUIGraphicsContext.DX9Device.DrawPrimitives(PrimitiveType.TriangleStrip,0,2);
    }
    void DrawText()
    {
      m_pTextBoxFont.DrawText( m_dwTextBoxXpos, m_dwTextBoxYpos, m_dwTextBoxColor, m_strData, GUIControl.Alignment.ALIGN_LEFT );


      // Draw blinking caret using line primitives.
      TimeSpan ts=DateTime.Now-m_CaretTimer;
      if(  (ts.TotalSeconds % fCARET_BLINK_RATE ) < fCARET_ON_RATIO )
      {
        string strLine=m_strData.Substring(0,m_iPos );

        float fCaretWidth = 0.0f;
        float fCaretHeight=0.0f;
        m_pTextBoxFont.GetTextExtent( strLine, ref fCaretWidth, ref fCaretHeight );
        m_pTextBoxFont.DrawText( m_dwTextBoxXpos+(int)fCaretWidth, m_dwTextBoxYpos, 0xff202020, "|", GUIControl.Alignment.ALIGN_LEFT );
  
      }
    }


    /// <summary>
    /// Get/set the name of the font.
    /// </summary>
    public string FontName
    {
      get { return m_strFontName; }
      set 
      { 
        if (value==null) return;
        if (value==String.Empty) return;
        m_pFont=GUIFontManager.GetFont(value);
        m_strFontName=value;
      }
    }

    /// <summary>
    /// Get/set the name of the font.
    /// </summary>
    public string FontName2
    {
      get { return m_strFontName2; }
      set 
      { 
        if (value==null) return;
        if (value==String.Empty) return;
        m_pFont2=GUIFontManager.GetFont(value);
        m_strFontName2=value;
      }
    }

    /// <summary>
    /// Get/set the name of the font used in the textbox.
    /// </summary>
    public string TextBoxFontName
    {
      get { return m_strTextBoxFontName; }
      set 
      { 
        if (value==null) return;
        if (value==String.Empty) return;
        m_pTextBoxFont=GUIFontManager.GetFont(value);
        m_strTextBoxFontName=value;
      }
    }


    /// <summary>
    /// Get/set the textcolor of the text 0-9
    /// </summary>
    public long	TextColor
    { 
      get { return m_dwTextColor;}
      set 
      { 
          m_dwTextColor=value;
      }
    }
    /// <summary>
    /// Get/set the textcolor of the text a-z
    /// </summary>
    public long	TextColor2
    { 
      get { return m_dwTextColor2;}
      set 
      { 
          m_dwTextColor2=value;
      }
    }
    /// <summary>
    /// Get/set the textcolor of the textbox
    /// </summary>
    public long	TextBoxColor
    { 
      get { return m_dwTextBoxColor;}
      set 
      { 
          m_dwTextBoxColor=value;
      }
    }
    /// <summary>
    /// Get/set the backgroundcolor of the textbox
    /// </summary>
    public long	TextBoxBackGroundColor
    { 
      get { return m_dwTextBoxBgColor;}
      set 
      { 
        m_dwTextBoxBgColor=value;
      }
    }
    
    /// <summary>
    /// Get/set the x position of the textbox
    /// </summary>
    public int	TextBoxX
    { 
      get { return m_dwTextBoxXpos;}
      set 
      { 
        m_dwTextBoxXpos=value;
      }
    }
    /// <summary>
    /// Get/set the y position of the textbox
    /// </summary>
    public int	TextBoxY
    { 
      get { return m_dwTextBoxYpos;}
      set 
      { 
        m_dwTextBoxYpos=value;
      }
    }
    /// <summary>
    /// Get/set the Width of the textbox
    /// </summary>
    public int	TextBoxWidth
    { 
      get { return m_dwTextBoxWidth;}
      set 
      { 
        m_dwTextBoxWidth=value;
      }
    }
    /// <summary>
    /// Get/set the Height of the textbox
    /// </summary>
    public int	TextBoxHeight
    { 
      get { return m_dwTextBoxHeight;}
      set 
      { 
        m_dwTextBoxHeight=value;
      }
    }
	}
}
