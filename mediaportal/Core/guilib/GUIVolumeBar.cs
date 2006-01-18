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
		int					_current=0;
		int					_maximum=10;
		int					_image1=0;
		int					_image2=1;
		Rectangle   rectDest = new Rectangle(0,0,0,0);
		Rectangle   rectSrc  = new Rectangle(0,0,0,0);
		[XMLSkinElement("align")]			protected Alignment _alignment =Alignment.ALIGN_RIGHT;  
		[XMLSkinElement("texture")]			protected string	_textureName="";
		[XMLSkinElement("imageHeight")]	protected int _imageHeight=3;

		public GUIVolumeBar(int dwParentID) : base(dwParentID)
		{
      imgBar = new GUIImage(dwParentID, 0, 0, 0, 0, 0, "", 0);
      imgBar.ParentControl = this;
		}
		
		public GUIVolumeBar(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight)
			: base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
		{
      imgBar = new GUIImage(dwParentID, dwControlId, dwPosX, dwPosY, 0, 0, _textureName, 0);
      imgBar.ParentControl = this;
		}

		public override void Render(float timePassed)
		{
			if (!IsVisible) return;
			if (imgBar.TextureWidth<=0) return;
			
			try
			{
				rectSrc.Y      = _image1 * (imgBar.TextureHeight/_imageHeight);
				rectSrc.Width  = imgBar.TextureWidth;
				rectSrc.Height = imgBar.TextureHeight/_imageHeight;

				switch(_alignment)
				{
					case Alignment.ALIGN_LEFT:
						rectDest.X = _positionX;
						break;
					case Alignment.ALIGN_CENTER:
						rectDest.X = _positionX - (((_maximum * imgBar.TextureWidth) - imgBar.TextureWidth) / 2);
						break;
					case Alignment.ALIGN_RIGHT:
						rectDest.X = imgBar.TextureWidth + _positionX - (_maximum * imgBar.TextureWidth);
						break;
				}

				rectDest.Y=_positionY;
				rectDest.Width=imgBar.TextureWidth;
				rectDest.Height=_height;

				for (int index = 0; index < _current; ++index)
				{
					imgBar.RenderRect(timePassed,rectSrc,rectDest);

					rectDest.X+=imgBar.TextureWidth;
				}

				if (_image2 != _image1)
					rectSrc.Y = _image2 * (imgBar.TextureHeight/_imageHeight);

				for (int index = _current + 1; index < _maximum; ++index)
				{
					imgBar.RenderRect(timePassed,rectSrc,rectDest);

					rectDest.X+=imgBar.TextureWidth;
				}
			}
			catch(Exception e)
			{
				Log.Write(e.Message);
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
		}

		public int Image1
		{
			get { return _image1;}
			set {  _image1=value;}
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
		public int Current
		{
			get { return _current;}
			set { _current = Math.Max(0, Math.Min(value, _maximum)); }
		}

		public int Maximum
		{
			get { return _maximum;}
			set { _maximum=value; }
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
