// ImageListPopup by John O'Byrne
// This class pops up window displaying the different images
// contained in an ImageList and allows to select one.
// History: 24/02/2003 : Initial Release
//          25/02/2003 : Added Keyboard support (arrows + space or enter to validate)
//                       Added Drag'n'Drop Support (disabled by default) - The selected Image
//                       and its Id are available to the drop target

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CustomUIControls
{

  #region Delegates

  public delegate void ImageListPanelEventHandler(object sender, ImageListPanelEventArgs ilpea);

  #endregion

  [ToolboxItem(false)]
  public class ImageListPanel : Control
  {
    #region Protected Member Variables

    protected Bitmap _Bitmap = null;
    protected ImageList _imageList = null;
    protected int _nBitmapWidth = 0;
    protected int _nBitmapHeight = 0;
    protected int _nItemWidth = 0;
    protected int _nItemHeight = 0;
    protected int _nRows = 0;
    protected int _nColumns = 0;
    protected int _nHSpace = 0;
    protected int _nVSpace = 0;
    protected int _nCoordX = -1;
    protected int _nCoordY = -1;
    protected bool _bIsMouseDown = false;

    protected int _defaultImage;

    #endregion

    #region Public Properties

    public Color BackgroundColor = Color.FromArgb(255, 255, 255);
    public Color BackgroundOverColor = Color.FromArgb(241, 238, 231);
    public Color HLinesColor = Color.FromArgb(222, 222, 222);
    public Color VLinesColor = Color.FromArgb(165, 182, 222);
    public Color BorderColor = Color.FromArgb(0, 16, 123);
    public bool EnableDragDrop = false;

    #endregion

    #region Events

    public event ImageListPanelEventHandler ItemClick = null;

    #endregion

    #region Constructor

    public ImageListPanel() {}

    #endregion

    #region Public Methods

    public bool Init(ImageList imageList, int nHSpace, int nVSpace, int nColumns, int defaultImage)
    {
      int nRows;

      Brush bgBrush = new SolidBrush(BackgroundColor);
      Pen vPen = new Pen(VLinesColor);
      Pen hPen = new Pen(HLinesColor);
      Pen borderPen = new Pen(BorderColor);

      _imageList = imageList;
      _nColumns = nColumns;

      _defaultImage = defaultImage;
      if (_defaultImage > _imageList.Images.Count)
      {
        _defaultImage = _imageList.Images.Count;
      }
      if (_defaultImage < 0)
      {
        _defaultImage = -1;
      }


      nRows = imageList.Images.Count / _nColumns;
      if (imageList.Images.Count % _nColumns > 0)
      {
        nRows++;
      }

      _nRows = nRows;
      _nHSpace = nHSpace;
      _nVSpace = nVSpace;
      _nItemWidth = _imageList.ImageSize.Width + nHSpace;
      _nItemHeight = _imageList.ImageSize.Height + nVSpace;
      _nBitmapWidth = _nColumns * _nItemWidth + 1;
      _nBitmapHeight = _nRows * _nItemHeight + 1;
      this.Width = _nBitmapWidth;
      this.Height = _nBitmapHeight;


      _Bitmap = new Bitmap(_nBitmapWidth, _nBitmapHeight);
      Graphics grfx = Graphics.FromImage(_Bitmap);
      grfx.FillRectangle(bgBrush, 0, 0, _nBitmapWidth, _nBitmapHeight);
      for (int i = 0; i < _nColumns; i++)
      {
        grfx.DrawLine(vPen, i * _nItemWidth, 0, i * _nItemWidth, _nBitmapHeight - 1);
      }
      for (int i = 0; i < _nRows; i++)
      {
        grfx.DrawLine(hPen, 0, i * _nItemHeight, _nBitmapWidth - 1, i * _nItemHeight);
      }

      grfx.DrawRectangle(borderPen, 0, 0, _nBitmapWidth - 1, _nBitmapHeight - 1);

      for (int i = 0; i < _nColumns; i++)
      {
        for (int j = 0; j < _nRows; j++)
        {
          if ((j * _nColumns + i) < imageList.Images.Count)
          {
            imageList.Draw(grfx,
                           i * _nItemWidth + _nHSpace / 2,
                           j * _nItemHeight + nVSpace / 2,
                           imageList.ImageSize.Width,
                           imageList.ImageSize.Height,
                           j * _nColumns + i);
          }
        }
      }

      /*	int a = (_defaultImage / _nColumns);  // rad
			int b = (_defaultImage % _nColumns); // kolumn;

			_nCoordX = b*(_nItemWidth+_nHSpace/2)-1;
			_nCoordY = a*(_nItemHeight+nVSpace/2)-1;
		*/

      // Clean up
      bgBrush.Dispose();
      vPen.Dispose();
      hPen.Dispose();
      borderPen.Dispose();

      Invalidate();
      return true;
    }

    public void Show(int x, int y)
    {
      this.Left = x;
      this.Top = y;
      base.Show();
    }

    #endregion

    #region Overrides

    protected override void OnMouseLeave(EventArgs ea)
    {
      // We repaint the popup if the mouse is no more over it
      base.OnMouseLeave(ea);
      _nCoordX = -1;
      _nCoordY = -1;
      Invalidate();
    }

    /*protected override void OnDeactivate(EventArgs ea)
		{
			// If the form loses focus, we hide it
			this.Hide();
		}*/

    protected override void OnKeyDown(KeyEventArgs kea)
    {
      if (_nCoordX == -1 || _nCoordY == -1)
      {
        _nCoordX = 0;
        _nCoordY = 0;
        Invalidate();
      }
      else
      {
        switch (kea.KeyCode)
        {
          case Keys.Down:
            if (_nCoordY < _nRows - 1)
            {
              _nCoordY++;
              Invalidate();
            }
            break;
          case Keys.Up:
            if (_nCoordY > 0)
            {
              _nCoordY--;
              Invalidate();
            }
            break;
          case Keys.Right:
            if (_nCoordX < _nColumns - 1)
            {
              _nCoordX++;
              Invalidate();
            }
            break;
          case Keys.Left:
            if (_nCoordX > 0)
            {
              _nCoordX--;
              Invalidate();
            }
            break;
          case Keys.Enter:
          case Keys.Space:
            // We fire the event only when the mouse is released
            int nImageId = _nCoordY * _nColumns + _nCoordX;
            if (ItemClick != null && nImageId >= 0 && nImageId < _imageList.Images.Count)
            {
              ItemClick(this, new ImageListPanelEventArgs(nImageId));
              _nCoordX = -1;
              _nCoordY = -1;
              Hide();
            }
            break;
          case Keys.Escape:
            _nCoordX = -1;
            _nCoordY = -1;
            Hide();
            break;
        }
      }
    }

    protected override void OnMouseMove(MouseEventArgs mea)
    {
      // Update the popup only if the image selection has changed
      if (ClientRectangle.Contains(new Point(mea.X, mea.Y)))
      {
        if (EnableDragDrop && _bIsMouseDown)
        {
          int nImage = _nCoordY * _nColumns + _nCoordX;
          if (nImage <= _imageList.Images.Count - 1)
          {
            DataObject data = new DataObject();
            data.SetData(DataFormats.Text, nImage.ToString());
            data.SetData(DataFormats.Bitmap, _imageList.Images[nImage]);
            try
            {
              DragDropEffects dde = DoDragDrop(data, DragDropEffects.Copy | DragDropEffects.Move);
            }
            catch {}
            _bIsMouseDown = false;
          }
        }

        if (((mea.X / _nItemWidth) != _nCoordX) || ((mea.Y / _nItemHeight) != _nCoordY))
        {
          _nCoordX = mea.X / _nItemWidth;
          _nCoordY = mea.Y / _nItemHeight;
          Invalidate();
        }
      }
      else
      {
        _nCoordX = -1;
        _nCoordY = -1;
        Invalidate();
      }
      base.OnMouseMove(mea);
    }

    protected override void OnMouseDown(MouseEventArgs mea)
    {
      base.OnMouseDown(mea);
      _bIsMouseDown = true;
      Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs mea)
    {
      base.OnMouseDown(mea);
      _bIsMouseDown = false;

      // We fire the event only when the mouse is released
      int nImageId = _nCoordY * _nColumns + _nCoordX;
      // check that imageID is a valid image
      if (ItemClick != null && nImageId >= 0 && nImageId < _imageList.Images.Count)
      {
        ItemClick(this, new ImageListPanelEventArgs(nImageId));
        Hide();
      }
    }


    protected override void OnPaintBackground(PaintEventArgs pea)
    {
      Graphics grfx = pea.Graphics;
      grfx.PageUnit = GraphicsUnit.Pixel;

      // Basic double buffering technique
      Bitmap offscreenBitmap = new Bitmap(_nBitmapWidth, _nBitmapHeight);
      Graphics offscreenGrfx = Graphics.FromImage(offscreenBitmap);
      // We blit the precalculated bitmap on the offscreen Graphics
      offscreenGrfx.DrawImage(_Bitmap, 0, 0);

      if (_nCoordX != -1 && _nCoordY != -1 && (_nCoordY * _nColumns + _nCoordX) < _imageList.Images.Count)
      {
        // We draw the selection rectangle
        offscreenGrfx.FillRectangle(new SolidBrush(BackgroundOverColor), _nCoordX * _nItemWidth + 1,
                                    _nCoordY * _nItemHeight + 1, _nItemWidth - 1, _nItemHeight - 1);
        if (_bIsMouseDown)
        {
          // Mouse Down aspect for the image
          _imageList.Draw(offscreenGrfx,
                          _nCoordX * _nItemWidth + _nHSpace / 2 + 1,
                          _nCoordY * _nItemHeight + _nVSpace / 2 + 1,
                          _imageList.ImageSize.Width,
                          _imageList.ImageSize.Height,
                          _nCoordY * _nColumns + _nCoordX);
        }
        else
        {
          // Normal aspect for the image
          _imageList.Draw(offscreenGrfx,
                          _nCoordX * _nItemWidth + _nHSpace / 2,
                          _nCoordY * _nItemHeight + _nVSpace / 2,
                          _imageList.ImageSize.Width,
                          _imageList.ImageSize.Height,
                          _nCoordY * _nColumns + _nCoordX);
        }
        // Border selection Rectangle
        offscreenGrfx.DrawRectangle(new Pen(BorderColor), _nCoordX * _nItemWidth, _nCoordY * _nItemHeight, _nItemWidth,
                                    _nItemHeight);
      }

      // We blit the offscreen image on the screen
      grfx.DrawImage(offscreenBitmap, 0, 0);

      // Clean up
      offscreenGrfx.Dispose();
    }

    #endregion
  }

  #region ImageListPanelEventArgs

  public class ImageListPanelEventArgs : EventArgs
  {
    public int SelectedItem;

    public ImageListPanelEventArgs(int selectedItem)
    {
      SelectedItem = selectedItem;
    }
  }

  #endregion
}