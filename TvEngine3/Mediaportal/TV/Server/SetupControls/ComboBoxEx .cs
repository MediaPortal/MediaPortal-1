#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System.Drawing;
using System.Windows.Forms;

namespace Mediaportal.TV.Server.SetupControls
{
  public class ComboBoxEx : ComboBox
  {
    private ImageList imageList;

    public ImageList ImageList
    {
      get { return imageList; }
      set { imageList = value; }
    }

    public ComboBoxEx()
    {
      DrawMode = DrawMode.OwnerDrawFixed;
    }

    protected override void OnDrawItem(DrawItemEventArgs ea)
    {
      ea.DrawBackground();
      ea.DrawFocusRectangle();

      Rectangle bounds = ea.Bounds;
      string text = Text;
      int imageIndex = -1;
      if (ea.Index >= 0 && ea.Index < Items.Count)
      {
        object item = Items[ea.Index];
        text = item.ToString();
        ComboBoxExItem itemEx = item as ComboBoxExItem;
        if (itemEx != null)
        {
          text = itemEx.Text;
          imageIndex = itemEx.ImageIndex;
        }
      }

      int leftBound = bounds.Left;
      if (imageIndex != -1)
      {
        imageList.Draw(ea.Graphics, bounds.Left, bounds.Top, imageIndex);
        leftBound += imageList.ImageSize.Width;
      }
      ea.Graphics.DrawString(text, ea.Font, new SolidBrush(ea.ForeColor), leftBound, bounds.Top);

      base.OnDrawItem(ea);
    }
  }

  public class ComboBoxExItem
  {
    private string _text;
    private readonly int _id;

    public string Text
    {
      get { return _text; }
      set { _text = value; }
    }

    private int _imageIndex;

    public int ImageIndex
    {
      get { return _imageIndex; }
      set { _imageIndex = value; }
    }

    public ComboBoxExItem()
      : this("") {}

    public ComboBoxExItem(string text)
      : this(text, -1, -1) {}

    public ComboBoxExItem(string text, int imageIndex, int id)
    {
      _id = id;
      _text = text;
      _imageIndex = imageIndex;
    }

    public int Id
    {
      get { return _id; }
    }

    public override string ToString()
    {
      return _text;
    }
  }
}