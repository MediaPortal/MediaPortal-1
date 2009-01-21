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

using System;
using System.Drawing;
using System.Threading;

// Credit and thanks to edx (felix.arends@gmx.de) for his Managed C++ version, a fair amount of copy and
// paste was involved.  Felix's version can be found at:
//
// http://www.codeproject.com/managedcpp/Tetris3D.asp
//
// Thanks also to the http://www.colinfahey.com/2003jan_tetris/tetris_standard_specifications.htm for
// what has to be the most detailed Tetris spec you'll find on the web.

namespace Tetris
{
  public enum State
  {
    Running,
    Paused,
    Stopped,
  }

  public interface IHostTetris
  {
    void OnRenderBlock(float timePassed, float x, float y, Color color, int nHint);
    void OnRenderSound(string strFilePath);
  }

  /// <summary>
  /// Written by Smirnoff (lee@netpoint-it.co.uk)
  /// </summary>
  public class Game
  {
    public Game(IHostTetris pHost)
    {
      m_pHost = pHost;
    }

    public enum Sound
    {
      Knock,
      Block,
      Level,
      Line,
    }

    #region Properties

    public State State
    {
      get { return m_nState; }
      set { m_nState = value; }
    }

    public static int Width
    {
      get { return 10; }
    }

    public static int Height
    {
      get { return 20; }
    }

    public int Score
    {
      set { m_nScore = value; }
      get { return m_nScore; }
    }

    public int Level
    {
      set { m_nLevel = value; }
      get { return m_nLevel + 1; }
    }

    public int Levels
    {
      get { return m_nLevelMax + 1; }
    }

    public int Lines
    {
      get { return m_nLines; }
    }

    public static float FallingSpeed
    {
      get { return 0.03f; }
    }

    public int Timeout
    {
      get { return _Timeouts[m_nLevel]; }
    }

    public int[,] Block
    {
      get { return m_nBlock; }
    }

    internal Block CurrentBlock
    {
      get { return m_blockCurrent; }
    }

    public int NextBlock
    {
      get { return m_blockNext != null ? m_blockNext.ColorIndex : -1; }
    }

    #endregion Properties

    #region Rendering

    public void Render(float timePassed)
    {
      if (m_pHost != null)
      {
        RenderBoard(timePassed);

        if (m_nState == State.Running || m_nState == State.Paused)
        {
          RenderBlock(timePassed, m_blockCurrent, 1);
        }

        RenderBlock(timePassed, m_blockNext, 2);
      }
    }

    private void RenderBoard(float timePassed)
    {
      for (int y = 0; y < Height; y++)
      {
        bool bEmptyRow = true;

        for (int x = 0; x < Width; x++)
        {
          if (m_nBlock[x, y] != 0)
          {
            bEmptyRow = false;

            if (y == m_nFallingRow)
            {
              // move next row?
              if ((Environment.TickCount - m_nFallingTime)*FallingSpeed > 1)
              {
                m_nFallingRow++;
                m_nFallingTime = Environment.TickCount;
              }
              else
              {
                m_pHost.OnRenderBlock(timePassed, x,
                                      Height - (y - ((Environment.TickCount - m_nFallingTime)*FallingSpeed)),
                                      _Colors[m_nBlock[x, y]], 0);
              }
            }

            if (y < m_nFallingRow)
            {
              m_pHost.OnRenderBlock(timePassed, x, Height - y, _Colors[m_nBlock[x, y]], 0);
            }
            else if (y > m_nFallingRow)
            {
              m_pHost.OnRenderBlock(timePassed, x, Height - y, _Colors[m_nBlock[x, y]], 0);
            }
          }
        }

        if (bEmptyRow)
        {
          m_nFallingRow = Height;
        }
      }
    }

    private void RenderBlock(float timePassed, Block block, int nHint)
    {
      if (block != null)
      {
        float[] x = new float[4];
        float[] y = new float[4];

        // get coordinates
        block.ToCoordinates(ref x, ref y);

        if (Environment.TickCount - m_nLastBlockTick < 600 && nHint == 0)
        {
          nHint = 3;
        }

        for (int nBlock = 0; nBlock < 4; nBlock++)
        {
          m_pHost.OnRenderBlock(timePassed, x[nBlock], Height - y[nBlock], block.Color, nHint);
        }
      }
    }

    #endregion

    private static int nLastTimerCall = Environment.TickCount;
    private static int last = Environment.TickCount;
    private int m_LinePlays = 0;

    public void Tick()
    {
      if (m_nState == State.Running && (Environment.TickCount - nLastTimerCall) > this.Timeout)
      {
        nLastTimerCall = Environment.TickCount;

        if (m_blockCurrent.Freefall() == false)
        {
          RenderSound(Sound.Knock);

          m_blockCurrent.Drop(this);
          m_blockCurrent.Clone(m_blockNext);

          if (m_blockNext.Create() == false)
          {
            m_nState = State.Stopped;
          }

          // settle block to game field and spawn next block
          m_blockCurrent.Y -= 5;
        }
      }

      int diff = Environment.TickCount - last;

      // another bing sound to play?
      if (m_LinePlays > 0)
      {
        if (diff > 150)
        {
          if (m_pHost != null)
          {
            m_pHost.OnRenderSound("MyTetris.Line.wav");
          }

          last = Environment.TickCount;
          m_LinePlays--;
        }
      }
    }

    private void RenderSound(Sound sound)
    {
      switch (sound)
      {
        case Sound.Knock:
          if (m_pHost != null)
          {
            m_pHost.OnRenderSound("MyTetris.Knock.wav");
          }
          break;
        case Sound.Level:
          if (m_pHost != null)
          {
            m_pHost.OnRenderSound("MyTetris.Level.wav");
          }
          break;
        case Sound.Block:
          if (m_pHost != null)
          {
            m_pHost.OnRenderSound("MyTetris.Block.wav");
          }
          break;
        case Sound.Line:
          m_LinePlays++;
          break;
      }
    }

    private void OnCompleteLine(int y)
    {
      m_nFallingRow = y;
      m_nFallingTime = Environment.TickCount;

      RenderSound(Sound.Line);

      m_nLines++;

      // play sound, if new level!
      if ((m_nLines - 1)/m_nLevelMax != m_nLevel && m_nLevel != m_nLevelMax)
      {
        RenderSound(Sound.Level);
      }

      m_nLevel = (m_nLines - 1)/m_nLevelMax;

      if (m_nLevel > m_nLevelMax)
      {
        // not more than 9 levels
        m_nLevel = m_nLevelMax;
      }

      for (; y < Height - 1; y++)
      {
        for (int x = 0; x < Width; x++)
        {
          m_nBlock[x, y] = m_nBlock[x, y + 1];
        }
      }

      for (int x = 0; x < Width; x++)
      {
        // upper row empty
        m_nBlock[x, Height - 1] = 0;
      }
    }

    public void CheckLines()
    {
      int x = 0;
      int y = 0;
      int nRows = 0;

      for (y = 0; y < Height; y++)
      {
        for (x = 0; x < Width; x++)
        {
          if (m_nBlock[x, y] == 0)
          {
            break;
          }
        }

        if (x == Width)
        {
          OnCompleteLine(y);
          y--;
          nRows++;
        }
      }

      // four at a time!
      if (nRows == 4)
      {
        // cause flashing effect
        m_nLastBlockTick = Environment.TickCount;

        // play sound
        RenderSound(Sound.Block);
      }
    }

    #region Public methods

    public void Start()
    {
      m_blockCurrent = new Block(this);
      m_blockCurrent.Create();

      m_blockNext = new Block(this);
      m_blockNext.Create();

      while (m_blockNext.ColorIndex == m_blockCurrent.ColorIndex)
      {
        // same block, keep trying until our current and next blocks are
        // different - saw too many instances where the two blocks at the
        // start of a game are the same.
        Thread.Sleep(100);
        m_blockNext.Create();
      }

      // shift the current block into the playing area
      m_blockCurrent.Y -= 5;

      m_nScore = 0;
      m_nLines = 0;
      m_nLevel = 0;

      // clear the board
      for (int x = 0; x < Width; x++)
      {
        for (int y = 0; y < Height; y++)
        {
          m_nBlock[x, y] = 0;
        }
      }

      m_nState = State.Running;
    }

    public enum Move
    {
      Rotate,
      Left,
      Right,
      Up,
      Down,
      Drop,
    }

    public void MoveBlock(Move move)
    {
      if (m_nState == State.Running)
      {
        switch (move)
        {
          case Move.Left:
            m_blockCurrent.Move(-1, 0, 0);
            break;
          case Move.Rotate:
            m_blockCurrent.Move(0, 0, 1);
            break;
          case Move.Right:
            m_blockCurrent.Move(1, 0, 0);
            break;
          case Move.Up:
            m_blockCurrent.Move(0, 1, 0);
            break;
          case Move.Down:
            m_blockCurrent.Move(0, -1, 0);
            break;
          case Move.Drop:
            m_blockCurrent.destination_dy = 0;

            while (m_blockCurrent.IsMoveValid(0, -1, 0))
            {
              m_blockCurrent.Y--;
            }

            RenderSound(Sound.Knock);

            m_blockCurrent.Drop(this);
            m_blockCurrent.Clone(m_blockNext);
            m_blockNext.Create();
            m_blockCurrent.Y -= 5;

            break;
        }
      }
    }

    #endregion

    #region Member variables

    // game variables
    private int m_nFallingRow = Height;
    private int m_nFallingTime = 0;
    private int m_nScore = 0;
    private int m_nLevel = 0;
    private const int m_nLevelMax = 9;
    private int m_nLines = 0;
    private int[,] m_nBlock = new int[Width,Height];
    internal Block m_blockCurrent = null;
    internal Block m_blockNext = null;
    public State m_nState = State.Stopped;
    private int m_nLastBlockTick = 0;
    protected static int[] _Timeouts = new int[] {500, 450, 400, 350, 300, 250, 200, 150, 100, 50};

    protected static Color[] _Colors = new Color[]
                                         {
                                           Color.Black, Color.Red, Color.Green, Color.Blue, Color.Orange, Color.Cyan,
                                           Color.Gray, Color.Yellow
                                         };

    private IHostTetris m_pHost;

    #endregion Member Variables
  }

  internal class Block
  {
    public Block(Game game)
    {
      m_theGame = game;
    }

    #region Properties

    public int X
    {
      set { m_nX = value; }
      get { return m_nX; }
    }

    public int Y
    {
      set { m_nY = value; }
      get { return m_nY; }
    }

    public Color Color
    {
      get { return _Colors[m_nColor]; }
    }

    public int ColorIndex
    {
      get { return m_nColor; }
    }

    public int Rotation
    {
      set { m_nRotation = value; }
      get { return m_nRotation; }
    }

    public int Width
    {
      get { return 0; }
    }

    public int Height
    {
      get { return 0; }
    }

    #endregion Properties

    public void Clone(Block src)
    {
      y_start = src.y_start;
      m_nX = src.m_nX;
      m_nY = src.m_nY;
      m_nFreefalls = src.m_nFreefalls;
      m_nRotation = src.m_nRotation;
      m_nData = src.m_nData;
      m_nColor = src.m_nColor;
    }

    public bool IsMoveValid(int dx, int dy, int dr)
    {
      Block temp = new Block(m_theGame);

      temp.Clone(m_theGame.CurrentBlock);
      temp.X += dx;
      temp.Y += dy;
      temp.Rotation = (temp.Rotation + 4 + dr)%4;

      // get coordinates
      float[] x = new float[4];
      float[] y = new float[4];

      temp.ToVirtualCoordinates(ref x, ref y);

      // validate...
      for (int i = 0; i < 4; i++)
      {
        // out of horizontal border
        if (x[i] < 0 || x[i] >= Game.Width)
        {
          return false;
        }

        // out of vertical border
        if (y[i] < 0)
        {
          return false;
        }

        if (y[i] >= Game.Height)
        {
          return true;
        }

        // on another block
        if (m_theGame.Block[(int) x[i], (int) y[i]] != 0)
        {
          return false;
        }
      }

      // indicate success
      return true;
    }

    // ToCoordinates - convert data to coordinates
    public void ToCoordinates(ref float[] x, ref float[] y)
    {
      ToVirtualCoordinates(ref x, ref y);

      if (destination_dy != 0)
      {
        if ((float) (Environment.TickCount - y_start)*Game.FallingSpeed > 1)
        {
          destination_dy = 0;
        }
      }

      // add movement offset
      for (int i = 0; i < 4; i++)
      {
        if (destination_dy != 0)
        {
          y[i] = y[i] + 1 - (float) (Environment.TickCount - y_start)*Game.FallingSpeed;
        }
      }
    }

    // get virtual coordinates
    private void ToVirtualCoordinates(ref float[] x, ref float[] y)
    {
      // extract x-coordinates
      x[0] = (float) ((_Blocks[m_nRotation, m_nData] >> 14) & 3);
      x[1] = (float) ((_Blocks[m_nRotation, m_nData] >> 10) & 3);
      x[2] = (float) ((_Blocks[m_nRotation, m_nData] >> 6) & 3);
      x[3] = (float) ((_Blocks[m_nRotation, m_nData] >> 2) & 3);

      // extract y-coordinates
      y[0] = (float) ((_Blocks[m_nRotation, m_nData] >> 12) & 3);
      y[1] = (float) ((_Blocks[m_nRotation, m_nData] >> 8) & 3);
      y[2] = (float) ((_Blocks[m_nRotation, m_nData] >> 4) & 3);
      y[3] = (float) ((_Blocks[m_nRotation, m_nData]) & 3);

      // map to complete coordinate system
      for (int i = 0; i < 4; i++)
      {
        x[i] += this.X - 2;
        y[i] += this.Y - 2;
      }
    }

    public bool Freefall()
    {
      if (Move(0, -1, 0))
      {
        m_nFreefalls++;
        return true;
      }

      return false;
    }

    public bool Move(int dx, int dy, int dr)
    {
      if (destination_dy != 0)
      {
        destination_dy = 0;
      }

      if (IsMoveValid(dx, dy, dr))
      {
        // apply position / rotation
        m_nRotation = ((m_nRotation + 4) + dr)%4;
        m_nX += dx;
        m_nY += dy;
        destination_dy += dy;
        y_start = Environment.TickCount;

        return true;
      }

      return false;
    }

    public bool Create()
    {
      Random r = new Random();

      m_nRotation = 0;
      m_nData = r.Next(7);
      m_nColor = m_nData + 1;
      m_nFreefalls = 0;
      m_nX = Game.Width/2;
      m_nY = Game.Height + this.Rectangle.Height + 3;

      return IsMoveValid(0, -5, 0);
    }

    public Rectangle Rectangle
    {
      get
      {
        int[] x = new int[4];
        int[] y = new int[4];
        Rectangle r = new Rectangle(4, 4, 0, 0);

        // extract x-coordinates
        x[0] = ((_Blocks[m_nRotation, m_nData] >> 14) & 3);
        x[1] = ((_Blocks[m_nRotation, m_nData] >> 10) & 3);
        x[2] = ((_Blocks[m_nRotation, m_nData] >> 6) & 3);
        x[3] = ((_Blocks[m_nRotation, m_nData] >> 2) & 3);

        // extract y-coordinates
        y[0] = 3 - ((_Blocks[m_nRotation, m_nData] >> 12) & 3);
        y[1] = 3 - ((_Blocks[m_nRotation, m_nData] >> 8) & 3);
        y[2] = 3 - ((_Blocks[m_nRotation, m_nData] >> 4) & 3);
        y[3] = 3 - ((_Blocks[m_nRotation, m_nData]) & 3);

        // find maxima
        for (int i = 0; i < 4; i++)
        {
          if (x[i] < r.X)
          {
            r.X = x[i];
          }
          if (y[i] < r.Y)
          {
            r.Y = y[i];
          }
        }

        for (int i = 0; i < 4; i++)
        {
          if (r.X - x[i] + 1 > r.Width)
          {
            r.Width = r.X - x[i] + 1;
          }
          if (r.Y - y[i] + 1 > r.Height)
          {
            r.Height = r.Y - y[i] + 1;
          }
        }

        return r;
      }
    }

    public void Drop(Game game)
    {
      // get coordinates
      float[] x = new float[4];
      float[] y = new float[4];

      ToCoordinates(ref x, ref y);

      for (int i = 0; i < 4; i++)
      {
        if ((x[i] < Game.Width) && (y[i] < Game.Height))
        {
          game.Block[(int) x[i], (int) (y[i])] = m_nColor;
        }
      }

      game.Score += ((24 + (3*game.Level)) - m_nFreefalls);
      game.CheckLines();
    }

    #region Member variables

    public Game m_theGame = null;
    public int destination_dy = 0;
    protected int y_start = 0;
    protected int m_nX = 0;
    protected int m_nY = 0;
    protected int m_nFreefalls = 0;
    protected int m_nRotation = 0;
    protected int m_nData = 0;
    protected int m_nColor = 0;
//		protected static Color[]	_Colors = new Color[] { Color.Black, Color.FromArgb(255, 0, 0), Color.FromArgb(0, 128, 0), Color.FromArgb(0, 0, 255), Color.FromArgb(255, 128, 0), Color.FromArgb(0, 128, 255), Color.FromArgb(128, 128, 128), Color.FromArgb(255, 255, 0) };
//		protected static Color[]	_Colors = new Color[]  { Color.Black, Color.FromArgb(229, 81, 87), Color.FromArgb(107, 165, 131), Color.FromArgb(32, 60, 110), Color.FromArgb(255, 128, 0), Color.FromArgb(0, 128, 255), Color.FromArgb(194, 194, 194), Color.FromArgb(255, 242, 0) };
    protected static Color[] _Colors = new Color[]
                                         {
                                           Color.Black, Color.Red, Color.Green, Color.Blue, Color.Orange, Color.Cyan,
                                           Color.Gray, Color.Yellow
                                         };

    protected static int[,] _Blocks = new int[4,7]
                                        {
                                          //	O     I      S      Z      L      T      J
                                          {22185, 9902, 42398, 42653, 42725, 27369, 27373},
                                          {22185, 47768, 44013, 44703, 43483, 44699, 43935},
                                          {22185, 9902, 42398, 42653, 42735, 42731, 42622},
                                          {22185, 47768, 44013, 44703, 43447, 42651, 43925},
                                        };

    #endregion Member Variables
  }
}