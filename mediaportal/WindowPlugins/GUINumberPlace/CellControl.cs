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

using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.NumberPlace
{
  /// <summary>
  /// Summary description for CellControl.
  /// </summary>
  public class CellControl : GUIButtonControl
  {
    [XMLSkinElement("textcolor")] //protected long m_dwNonEditableTextColor=0x770000FF;
      protected static long m_dwCellIncorrectTextColor = 0xFFFFFFFF;

    protected static long m_dwTextColor = 0xFFFFFFFF;
    protected long m_dwDisabledColor = 0xFF000000;
    protected int m_iTextOffsetY = 2;

    private GUIImage m_imgFocus = null;
    private GUIImage m_imgNoFocus = null;
    private GUIImage _colorOverlay = null;
    private GUIImage[] m_imgOverlay = new GUIImage[9];
    private GUILabelControl m_label = null;

    private bool[] _isCandidate = new bool[9];
    private bool _displayColourOverlay = false;
    private bool _showCandidates = false;

    public int SolutionValue = 0;
    public bool editable = true;

    private int cellValue = 0;

    public int CellValue
    {
      get { return cellValue; }
      set
      {
        if (editable)
        {
          cellValue = value;
        }
      }
    }

    public static long M_dwCellIncorrectTextColor
    {
      get { return m_dwCellIncorrectTextColor; }
      set { m_dwCellIncorrectTextColor = value; }
    }

    public static long M_dwTextColor
    {
      get { return m_dwTextColor; }
    }

    public long M_dwDisabledColor
    {
      get { return m_dwDisabledColor; }

      set { m_dwDisabledColor = value; }
    }

    public bool ShowCandidates
    {
      set { _showCandidates = value; }
    }

    public bool Highlight
    {
      set { _displayColourOverlay = value; }
    }

    public CellControl(int dwParentID)
      : base(dwParentID)
    {
    }

    public CellControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight,
                       string strTextureFocus, string strTextureNoFocus)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight, strTextureFocus, strTextureNoFocus)
    {
    }

    public override void AllocResources()
    {
      base.AllocResources();

      m_imgFocus = new GUIImage(GetID, GetID*10, _positionX, _positionY, this.Width, this.Height, "icon_empty_focus.png",
                                0xFFFFFFFF);
      m_imgFocus.AllocResources();
      m_imgNoFocus = new GUIImage(GetID, GetID*100, _positionX, _positionY, this.Width, this.Height,
                                  "icon_empty_nofocus.png", 0xFFFFFFFF);
      m_imgNoFocus.AllocResources();
      for (int i = 0; i < 9; i++)
      {
        m_imgOverlay[i] = new GUIImage(GetID, GetID*1000 + i, _positionX, _positionY, this.Width, this.Height,
                                       string.Format("icon_numberplace_overlay_{0}.png", i + 1), 0xFFFFFFFF);
        m_imgOverlay[i].AllocResources();
      }
      m_label = new GUILabelControl(GetID, GetID*1000, _positionX, _positionY, this.Width, this.Height, "font18",
                                    string.Empty, 0xFFFFFFFF, Alignment.ALIGN_CENTER, false);
      _colorOverlay = new GUIImage(GetID, GetID*10, _positionX, _positionY, this.Width, this.Height,
                                   "icon_numberplace_colouroverlay.png", 0xFFFFFFFF);
      _colorOverlay.AllocResources();
    }

    public override void Render(float timePassed)
    {
      base.Render(timePassed);
      // Do not render if not visible.
      if (GUIGraphicsContext.EditMode == false)
      {
        if (!IsVisible)
        {
          return;
        }
      }

      // The GUIButtonControl has the focus

      //if (value < 0)
      //{

      if (Focus)
      {
        //render the focused image
        m_imgFocus.Render(timePassed);
      }
      else
      {
        //render the non-focused image
        m_imgNoFocus.Render(timePassed);
      }
      //}

      m_label.TextAlignment = Alignment.ALIGN_CENTER;
      m_label.Label = (CellValue > 0) ? CellValue.ToString() : "";

      if (CellValue > 0)
      {
        if (editable)
        {
          if (CellValue == SolutionValue)
          {
            m_label.TextColor = m_dwTextColor;
          }
          else
          {
            m_label.TextColor = m_dwCellIncorrectTextColor;
          }
        }
        else
        {
          m_label.TextColor = m_dwDisabledColor;
        }
      }

      m_label.SetPosition(XPosition, YPosition + m_iTextOffsetY);
      m_label.Render(timePassed);

      if (_showCandidates)
      {
        if (_displayColourOverlay && editable && (m_label.Label == string.Empty))
        {
          _colorOverlay.Render(timePassed);
        }

        if (m_label.Label == string.Empty)
        {
          for (int i = 0; i < 9; i++)
          {
            if (_isCandidate[i])
            {
              m_imgOverlay[i].Render(timePassed);
            }
          }
        }
      }
    }

    public void SetCandidate(int number)
    {
      if ((cellValue == 0) && editable)
      {
        _isCandidate[number - 1] = true;
      }
    }

    public void RemoveCandidate(int number)
    {
      _isCandidate[number - 1] = false;
    }

    public void ClearCandidates()
    {
      for (int i = 1; i <= 9; i++)
      {
        RemoveCandidate(i);
      }
    }

    public bool IsCandidate(int number)
    {
      return (_isCandidate[number - 1]);
    }

    public void HighlightCandidate(int number)
    {
      if (number == -1)
      {
        _displayColourOverlay = false;
      }
      else if (number == 0)
      {
        _displayColourOverlay = false;
      }
      else if (_isCandidate[number - 1])
      {
        _displayColourOverlay = true;
      }
      else
      {
        _displayColourOverlay = false;
      }
    }
  }
}