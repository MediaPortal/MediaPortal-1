using System;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// 
	/// </summary>
	public class Animator
  {
    const int NUMBEROFFRAMES        = 25; // total 25 frames
    const int FRAME_DURATION_IN_MSEC= 20; // 20 msec / frame
    public enum AnimationType
    {
      None,
      FlyInFromLeft,
      FlyInFromRight,
      FlyInFromTop,
      FlyInFromBottom,
      ZoomInFromMiddle
    };
    protected AnimationType animType=AnimationType.None;
    protected DateTime      m_DateTimeStart=DateTime.MinValue;
    protected bool          m_Animating=false;
		public Animator(AnimationType type)
		{
      animType=type;
      m_DateTimeStart=DateTime.Now;
      m_Animating=true;
		}

    public bool IsDone()
    {
      if (!m_Animating) return true;
      if (animType == AnimationType.None) return true;
      return false;
    }

    public void Animate(ref int x, ref int y, ref int width, ref int height)
    {
      if ( IsDone() ) return;
      
      TimeSpan ts =DateTime.Now-m_DateTimeStart;
      int iFrame = (int)Math.Floor(ts.TotalMilliseconds/FRAME_DURATION_IN_MSEC);
      if (iFrame > NUMBEROFFRAMES) 
      {
        m_Animating=false;
        return;
      }

      int posx=x;
      int posy=y;
      int w=width;
      int h=height;
      switch (animType )
      {
          case AnimationType.FlyInFromLeft:
          {
            int iStepX= (x+width) / NUMBEROFFRAMES;
            if (iStepX<=0) iStepX=1;
            posx = iStepX*iFrame;
            posx -=width;
            if (posx > x) posx=x;
          }
          break;

        case AnimationType.FlyInFromRight:
        {
          int iStepX= (GUIGraphicsContext.Width-x) / NUMBEROFFRAMES;
          if (iStepX<=0) iStepX=1;
          posx = x + GUIGraphicsContext.Width- (iStepX*iFrame);
          if (posx < x) posx=x;
        }
          break;

        case AnimationType.FlyInFromTop:
        {
          int iStepy= (y+height) / NUMBEROFFRAMES;
          if (iStepy<=0) iStepy=1;
          posy = iStepy*iFrame;
          posy -=height;
          if (posy > y) posy=y;
        }
          break;


        case AnimationType.FlyInFromBottom:
        {
          int iStepY= (GUIGraphicsContext.Height-y) / NUMBEROFFRAMES;
          if (iStepY<=0) iStepY=1;
          posy = y + GUIGraphicsContext.Height- (iStepY*iFrame);
          if (posy < y) posy=y;
        }
          break;
        case AnimationType.ZoomInFromMiddle:
        {
          int iStepY= (height/2) / (NUMBEROFFRAMES );
          if (iStepY<=0) iStepY=1;
          int iStepX= (width/2) / (NUMBEROFFRAMES );
          if (iStepX<=0) iStepX=1;

          iStepY*=iFrame;
          iStepX*=iFrame;


          posy = y+(height/2)-iStepY;
          posx = x+(width/2)-iStepX;
          h=iStepY*2;
          w=iStepX*2;
        }
        break;
      }
      x=posx;
      y=posy;
      width=w;
      height=h;
    }
	}
}
