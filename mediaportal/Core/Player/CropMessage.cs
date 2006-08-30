using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.Player
{
  public class CropMessage
  {
    public enum Edge
    {
      Top,
      Bottom,
      Left,
      Right
    }
    
    private int _amount;
    private Edge _edge;

    public CropMessage(Edge edge, int amount)
    {
      _edge = edge;
      _amount = amount;
    }

    public int Amount
    {
      get { return _amount; }
      set { _amount = value; }
    }
    public Edge EdgeToCtop
    {
      get { return _edge; }
      set { _edge = value; }
    }
  }
}
