using System;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// Class which can do animatons on controls
	/// </summary>
	public class Animator
  {
    const int NUMBEROFFRAMES        = 25; // total 25 frames
    const int FRAME_DURATION_IN_MSEC= 20; // 20 msec / frame

		// type of animations
    public enum AnimationType
    {
      None,
      FlyInFromLeft,
      FlyInFromRight,
      FlyInFromTop,
      FlyInFromBottom,
      ZoomInFromMiddle
    };


    protected AnimationType animType=AnimationType.None;						//current animation type
    protected DateTime      m_DateTimeStart=DateTime.MinValue;			//time animation started
    protected bool          m_Animating=false;											//boolean indicating if we're animating


		public Animator(AnimationType type)
		{
      animType=type;
      m_DateTimeStart=DateTime.Now;
      m_Animating=true;
		}

		/// <summary>
		/// Method which returns true if the animation has ended
		/// </summary>
		/// <returns>true : animation has ended
		///          false: animation is still busy
		/// </returns>
    public bool IsDone()
    {
      if (!m_Animating) return true;
      if (animType == AnimationType.None) return true;
      return false;
    }

		/// <summary>
		/// Method which does the next step of the animation
		/// It will modify the x,y,width,height parameters 
		/// based on the current animation and current frame
		/// </summary>
		/// <param name="x">x-coordinate of control</param>
		/// <param name="y">y-coordinate of control</param>
		/// <param name="width">width of control</param>
		/// <param name="height">height of control</param>
    public void Animate(long timePassed,ref int x, ref int y, ref int width, ref int height)
    {
			//if animation has ended, then just return
      if ( IsDone() ) return;
      
			//check if animation should end
      TimeSpan ts =DateTime.Now-m_DateTimeStart;
      int iFrame = (int)Math.Floor(ts.TotalMilliseconds/FRAME_DURATION_IN_MSEC);
      if (iFrame > NUMBEROFFRAMES) 
      {
				//yes, then end the animation
        m_Animating=false;
        return;
      }

			//keep copy of original control rectangle
      int posx=x;
      int posy=y;
      int w=width;
      int h=height;

			//modify the coordinates,width,height for the current animation type
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
			// and return the modified coordinates,with,height
      x=posx;
      y=posy;
      width=w;
      height=h;
    }
	}
}
