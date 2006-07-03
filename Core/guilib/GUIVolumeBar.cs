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
		GUIImage _imageVolumeBar;
		int					_current=0;
		int					_maximum=10;
		int					_image1=0;
		int					_image2=1;
		Rectangle   _destinationRectangle = new Rectangle(0,0,0,0);
		Rectangle   _sourceRectangle  = new Rectangle(0,0,0,0);
		[XMLSkinElement("align")]			protected Alignment _alignment =Alignment.ALIGN_RIGHT;  
		[XMLSkinElement("texture")]			protected string	_textureName="";
		[XMLSkinElement("imageHeight")]	protected int _imageHeight=3;

		public GUIVolumeBar(int dwParentID) : base(dwParentID)
		{
      _imageVolumeBar = new GUIImage(dwParentID, 0, 0, 0, 0, 0, "", 0);
      _imageVolumeBar.ParentControl = this;
      DimColor = base.DimColor;
		}
		
		public GUIVolumeBar(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight)
			: base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
		{
      _imageVolumeBar = new GUIImage(dwParentID, dwControlId, dwPosX, dwPosY, 0, 0, _textureName, 0);
      _imageVolumeBar.ParentControl = this;
		}

		public override void Render(float timePassed)
		{
			if (!IsVisible) return;
			if (_imageVolumeBar.TextureWidth<=0) return;
			
			try
			{
				_sourceRectangle.Y      = _image1 * (_imageVolumeBar.TextureHeight/_imageHeight);
				_sourceRectangle.Width  = _imageVolumeBar.TextureWidth;
				_sourceRectangle.Height = _imageVolumeBar.TextureHeight/_imageHeight;

				switch(_alignment)
				{
					case Alignment.ALIGN_LEFT:
						_destinationRectangle.X = _positionX;
						break;
					case Alignment.ALIGN_CENTER:
						_destinationRectangle.X = _positionX - (((_maximum * _imageVolumeBar.TextureWidth) - _imageVolumeBar.TextureWidth) / 2);
						break;
					case Alignment.ALIGN_RIGHT:
						_destinationRectangle.X = _imageVolumeBar.TextureWidth + _positionX - (_maximum * _imageVolumeBar.TextureWidth);
						break;
				}

				_destinationRectangle.Y=_positionY;
				_destinationRectangle.Width=_imageVolumeBar.TextureWidth;
				_destinationRectangle.Height=_height;

				for (int index = 0; index < _current; ++index)
				{
					_imageVolumeBar.RenderRect(timePassed,_sourceRectangle,_destinationRectangle);

					_destinationRectangle.X+=_imageVolumeBar.TextureWidth;
				}

				if (_image2 != _image1)
					_sourceRectangle.Y = _image2 * (_imageVolumeBar.TextureHeight/_imageHeight);

				for (int index = _current + 1; index < _maximum; ++index)
				{
					_imageVolumeBar.RenderRect(timePassed,_sourceRectangle,_destinationRectangle);

					_destinationRectangle.X+=_imageVolumeBar.TextureWidth;
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
			_imageVolumeBar.AllocResources();
			_imageVolumeBar.SetFileName(_textureName);
		}
		public override void FreeResources()
		{
			base.FreeResources ();
			_imageVolumeBar.FreeResources();
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
			get { return _imageVolumeBar.TextureHeight;}
		}
		public int TextureWidth
		{
			get { return _imageVolumeBar.TextureWidth;}
		}

    public override int DimColor
    {
      get { return base.DimColor; }
      set
      {
        base.DimColor = value;
        if (_imageVolumeBar != null) _imageVolumeBar.DimColor = value;
      }
    }

	}
}
