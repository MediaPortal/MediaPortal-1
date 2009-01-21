#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

namespace MediaPortal.Drawing
{
  public sealed class Pen : PenBase
  {
    #region Constructors

    public Pen()
    {
    }

    public Pen(Brush brush, double thickness)
    {
      _brush = brush;
      _thickness = thickness;
    }

    #endregion Constructors

    #region Properties

    public Brush Brush
    {
      get { return _brush; }
      set
      {
        if (!Equals(_brush, value))
        {
          _brush = value;
          RaiseChanged();
        }
      }
    }

    public PenLineCap DashCap
    {
      get { return _dashCap; }
      set
      {
        if (_dashCap != value)
        {
          _dashCap = value;
          RaiseChanged();
        }
      }
    }

    public DashStyle DashStyle
    {
      get { return _dashStyle; }
      set
      {
        if (!Equals(_dashStyle, value))
        {
          _dashStyle = value;
          RaiseChanged();
        }
      }
    }

    public PenLineCap EndLineCap
    {
      get { return _endLineCap; }
      set
      {
        if (_endLineCap != value)
        {
          _endLineCap = value;
          RaiseChanged();
        }
      }
    }

    public PenLineJoin LineJoin
    {
      get { return _lineJoin; }
      set
      {
        if (_lineJoin != value)
        {
          _lineJoin = value;
          RaiseChanged();
        }
      }
    }

    public double MiterLimit
    {
      get { return _miterLimit; }
      set
      {
        if (!Equals(_miterLimit, value))
        {
          _miterLimit = value;
          RaiseChanged();
        }
      }
    }

    public PenLineCap StartLineCap
    {
      get { return _startLineCap; }
      set
      {
        if (_startLineCap != value)
        {
          _startLineCap = value;
          RaiseChanged();
        }
      }
    }

    public double Thickness
    {
      get { return _thickness; }
      set
      {
        if (!Equals(_thickness, value))
        {
          _thickness = value;
          RaiseChanged();
        }
      }
    }

    #endregion Properties

    #region Fields

    private Brush _brush;
    private PenLineCap _dashCap;
    private DashStyle _dashStyle;
    private PenLineCap _endLineCap;
    private PenLineJoin _lineJoin;
    private double _miterLimit;
    private PenLineCap _startLineCap;
    private double _thickness;

    #endregion Fields
  }
}