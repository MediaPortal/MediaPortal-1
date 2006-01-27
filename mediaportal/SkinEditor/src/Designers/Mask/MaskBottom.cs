using System.Drawing;
using System.Windows.Forms;

namespace Mpe.Designers.Mask
{
  /// <summary>
  /// Summary description for MaskBottom.
  /// </summary>
  public class MaskBottom : MaskComponent
  {
    public MaskBottom(MpeControlMask mask) : base(mask)
    {
      nodes = new Rectangle[3];
      for (int i = 0; i < nodes.Length; i++)
      {
        nodes[i] = new Rectangle(0, 0, mask.NodeSize, mask.NodeSize);
      }
    }

    public override void Initialize()
    {
      if (mask != null && mask.SelectedControl != null)
      {
        Left = mask.SelectedControl.Left - mask.NodeSize;
        Top = mask.SelectedControl.Top + mask.SelectedControl.Height;
        Width = mask.SelectedControl.Width + mask.NodeSize + mask.NodeSize;
        Height = mask.NodeSize;
        nodes[1].Location = new Point(Width/2 - mask.NodeSize/2, 0);
        nodes[2].Location = new Point(Width - mask.NodeSize, 0);
      }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
      if (mask.MoveDrag == false && mask.ResizeDrag == false)
      {
        if (nodes[0].Contains(e.X, e.Y) && mask.LocationLocked == false && mask.SizeLocked == false)
        {
          if (Cursor != Cursors.SizeNESW)
          {
            Cursor = Cursors.SizeNESW;
            return;
          }
        }
        else if (nodes[1].Contains(e.X, e.Y) && mask.SizeLocked == false)
        {
          if (Cursor != Cursors.SizeNS)
          {
            Cursor = Cursors.SizeNS;
            return;
          }
        }
        else if (nodes[2].Contains(e.X, e.Y) && mask.SizeLocked == false)
        {
          if (Cursor != Cursors.SizeNWSE)
          {
            Cursor = Cursors.SizeNWSE;
            return;
          }
        }
        else if (mask.LocationLocked == false)
        {
          if (Cursor != Cursors.SizeAll)
          {
            Cursor = Cursors.SizeAll;
            return;
          }
        }
        else
        {
          if (Cursor != Cursors.Default)
          {
            Cursor = Cursors.Default;
            return;
          }
        }
      }
      base.OnMouseMove(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
      if (Cursor == Cursors.Default)
      {
        return;
      }
      else if (Cursor == Cursors.SizeAll)
      {
        mask.MoveDrag = true;
      }
      else if (Cursor == Cursors.SizeNESW)
      {
        mask.ResizeDrag = true;
        mask.ResizeNodeIndex = 5;
      }
      else if (Cursor == Cursors.SizeNS)
      {
        mask.ResizeDrag = true;
        mask.ResizeNodeIndex = 6;
      }
      else if (Cursor == Cursors.SizeNWSE)
      {
        mask.ResizeDrag = true;
        mask.ResizeNodeIndex = 7;
      }
      base.OnMouseDown(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
      // Paint Bar
      e.Graphics.FillRectangle(mask.BarBrush, ClientRectangle);
      // Paint Nodes
      if (mask.SizeLocked)
      {
        for (int i = 0; i < nodes.Length; i++)
        {
          e.Graphics.FillRectangle(mask.DisabledNodeBrush, nodes[i]);
          e.Graphics.DrawRectangle(mask.NodePen, nodes[i].Left, nodes[i].Top, nodes[i].Width - 1, nodes[i].Height - 1);
        }
      }
      else if (mask.LocationLocked)
      {
        for (int i = 0; i < nodes.Length; i++)
        {
          if (i == 0)
          {
            e.Graphics.FillRectangle(mask.DisabledNodeBrush, nodes[i]);
          }
          else
          {
            e.Graphics.FillRectangle(mask.NodeBrush, nodes[i]);
          }
          e.Graphics.DrawRectangle(mask.NodePen, nodes[i].Left, nodes[i].Top, nodes[i].Width - 1, nodes[i].Height - 1);
        }
      }
      else
      {
        for (int i = 0; i < nodes.Length; i++)
        {
          e.Graphics.FillRectangle(mask.NodeBrush, nodes[i]);
          e.Graphics.DrawRectangle(mask.NodePen, nodes[i].Left, nodes[i].Top, nodes[i].Width - 1, nodes[i].Height - 1);
        }
      }
    }
  }
}