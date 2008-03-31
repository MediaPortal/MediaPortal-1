#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace SetupControls
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

      ComboBoxExItem item;
      Size imageSize = imageList.ImageSize;
      Rectangle bounds = ea.Bounds;

      try
      {
        item = (ComboBoxExItem)Items[ea.Index];

        if (item.ImageIndex != -1)
        {
          imageList.Draw(ea.Graphics, bounds.Left, bounds.Top, item.ImageIndex);
          ea.Graphics.DrawString(item.Text, ea.Font, new SolidBrush(ea.ForeColor), bounds.Left + imageSize.Width, bounds.Top);
        }
        else
        {
          ea.Graphics.DrawString(item.Text, ea.Font, new SolidBrush(ea.ForeColor), bounds.Left, bounds.Top);
        }
      }
      catch
      {
        if (ea.Index != -1)
        {
          ea.Graphics.DrawString(Items[ea.Index].ToString(), ea.Font, new  SolidBrush(ea.ForeColor), bounds.Left, bounds.Top);
        }
        else
        {
          ea.Graphics.DrawString(Text, ea.Font, new  SolidBrush(ea.ForeColor), bounds.Left, bounds.Top);
        }
      }

      base.OnDrawItem(ea);
    }
  }

  public class ComboBoxExItem
  {
    private string _text;
    private int _id;
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
      : this("")
    {
    }

    public ComboBoxExItem(string text)
      : this(text, -1,-1)
    {
    }

    public ComboBoxExItem(string text, int imageIndex,int id)
    {
      _id = id;
      _text = text;
      _imageIndex = imageIndex;
    }

    public int Id
    {
      get
      {
        return _id;
      }
    }
    public override string ToString()
    {
      return _text;
    }
  }
}



