using System;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// Summary description for GUIVolumeBar.
	/// </summary>
	public class GUIVolumeBar : GUIControl
	{
		GUIImage imgBar;
		int					_percent=40;
		int					_image1=0;
		int					_image2=1;
		Rectangle   rectDest = new Rectangle(0,0,0,0);
		Rectangle   rectSrc  = new Rectangle(0,0,0,0);
		[XMLSkinElement("texture")]			protected string	_textureName="";
		[XMLSkinElement("imageHeight")]	protected int _imageHeight=3;

		public GUIVolumeBar(int dwParentID) : base(dwParentID)
		{
			imgBar   =new GUIImage(dwParentID, 0, 0, 0,0, 0, "",0);
		}
		
		public GUIVolumeBar(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight)
			: base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
		{
			imgBar   =new GUIImage(dwParentID, dwControlId, dwPosX, dwPosY,0, 0, _textureName,0);
		}
		public override void Render(float timePassed)
		{
			if (!IsVisible) return;
			if (imgBar.Width<=0) return;
			int w = (int)((((float)_percent) * ((float)m_dwWidth) )/100f);
			int x=0;
			while (x < w)
			{
				rectDest.X=m_dwPosX+x;
				rectDest.Y=m_dwPosY;
				rectDest.Width=imgBar.TextureWidth;
				rectDest.Height=m_dwHeight;

				rectSrc.X      = 0;
				rectSrc.Y      = _image1 * (imgBar.TextureHeight/_imageHeight);
				rectSrc.Width  = imgBar.TextureWidth;
				rectSrc.Height = imgBar.TextureHeight/_imageHeight;
				imgBar.RenderRect(timePassed,rectSrc,rectDest);

				x+=imgBar.TextureWidth;
			}
			while (x < m_dwWidth)
			{
				rectDest.X=m_dwPosX+x;
				rectDest.Y=m_dwPosY;
				rectDest.Width=imgBar.TextureWidth;
				rectDest.Height=m_dwHeight;

				rectSrc.X      = 0;
				rectSrc.Y      = _image2 * (imgBar.TextureHeight/_imageHeight);
				rectSrc.Width  = imgBar.TextureWidth;
				rectSrc.Height= imgBar.TextureHeight/_imageHeight;
				imgBar.RenderRect(timePassed,rectSrc,rectDest);

				x+=imgBar.TextureWidth;
			}

		}
		public override void AllocResources()
		{
			base.AllocResources ();
			imgBar.AllocResources();
			imgBar.SetFileName(_textureName);
		}
		public override void FreeResources()
		{
			base.FreeResources ();
			imgBar.FreeResources();
			imgBar=null;
		}

		public int Image1
		{
			get { return _image1;}
			set {  _image2=value;}
		}
		public int Image2
		{
			get { return _image2;}
			set {  _image2=value;}
		}
		public int ImageHeight
		{
			get { return _imageHeight;}
			set {  _imageHeight=value;}
		}
		public int Percent
		{
			get { return _percent;}
			set { 
				_percent=value;
				if (_percent<0) _percent=0;
				if (_percent>100) _percent=100;
			}
		}

		public int TextureHeight
		{
			get { return imgBar.TextureHeight;}
		}
		public int TextureWidth
		{
			get { return imgBar.TextureWidth;}
		}
	}
}
