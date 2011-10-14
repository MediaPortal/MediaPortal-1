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

#region license
//Copyright (c) 2005-2010 Roger Lipscombe
//Permission is hereby granted, free of charge, to any person obtaining a 
//copy of this software and associated documentation files (the "Software"),
//to deal in the Software without restriction, including without limitation
//the rights to use, copy, modify, merge, publish, distribute, sublicense,
//and/or sell copies of the Software, and to permit persons to whom the 
//Software is furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included
//in all copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
//WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT
//OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
#endregion

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MpeCore.Classes.SectionPanel
{
   public partial class EtchedLine : UserControl
   {
      public EtchedLine()
      {
         // This call is required by the Windows.Forms Form Designer.
         InitializeComponent();

         // Avoid receiving the focus.
         SetStyle(ControlStyles.Selectable, false);
      }

      protected override void OnPaint(PaintEventArgs e)
      {
         base.OnPaint(e);

         Brush lightBrush = new SolidBrush(_lightColor);
         Brush darkBrush = new SolidBrush(_darkColor);
         Pen lightPen = new Pen(lightBrush, 1);
         Pen darkPen = new Pen(darkBrush, 1);

         switch (Edge)
         {
            case EtchEdge.Top:
               e.Graphics.DrawLine(darkPen, 0, 0, Width, 0);
               e.Graphics.DrawLine(lightPen, 0, 1, Width, 1);
               break;
            case EtchEdge.Bottom:
               e.Graphics.DrawLine(darkPen, 0, Height - 2, Width, Height - 2);
               e.Graphics.DrawLine(lightPen, 0, Height - 1, Width, Height - 1);
               break;
         }
      }

      protected override void OnResize(EventArgs e)
      {
         base.OnResize (e);

         Refresh();
      }

      Color _darkColor = SystemColors.ControlDark;

      [Category("Appearance")]
      Color DarkColor
      {
         get { return _darkColor; }

         set
         {
            _darkColor = value;
            Refresh();
         }
      }

      Color _lightColor = SystemColors.ControlLightLight;

      [Category("Appearance")]
      Color LightColor
      {
         get { return _lightColor; }

         set
         {
            _lightColor = value;
            Refresh();
         }
      }

      EtchEdge _edge = EtchEdge.Top;

      [Category("Appearance")]
      public EtchEdge Edge
      {
         get { return _edge; }
         set
         {
            _edge = value;
            Refresh();
         }
      }
   }

   public enum EtchEdge
   {
      Top, Bottom
   }
}
