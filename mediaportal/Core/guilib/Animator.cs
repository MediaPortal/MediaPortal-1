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
    protected bool          m_Animating=false;											//boolean indicating if we're animating
		protected long          lTime;

		public Animator(AnimationType type)
		{
      animType=type;
      lTime=0;
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
			float fTotalTime=FRAME_DURATION_IN_MSEC*NUMBEROFFRAMES;
			float fTime=(float)lTime;
			if (lTime >= fTotalTime)
			{
				//yes, then end the animation
        m_Animating=false;
        return;
      }

			//keep copy of original control rectangle
      float posx=(float)x;
      float posy=(float)y;
      float w=(float)width;
      float h=(float)height;

			//modify the coordinates,width,height for the current animation type
      switch (animType )
      {
          case AnimationType.FlyInFromLeft:
          {
            float iStepX= ( (float)(x+width)) / fTotalTime;
            if (iStepX<=0) iStepX=1;
            posx =  iStepX*fTime;
            posx -= (float)width;
            if (posx > x) posx=x;
          }
          break;

        case AnimationType.FlyInFromRight:
        {
          float iStepX= ((float)(GUIGraphicsContext.Width-x)) / fTotalTime;
          if (iStepX<=0) iStepX=1;
          posx = x + GUIGraphicsContext.Width- (iStepX*fTime);
          if (posx < x) posx=x;
        }
          break;

        case AnimationType.FlyInFromTop:
        {
          float iStepy= ((float)(y+height)) / fTotalTime;
          if (iStepy<=0) iStepy=1;
          posy = iStepy*fTime;
          posy -=height;
          if (posy > y) posy=y;
        }
          break;


        case AnimationType.FlyInFromBottom:
        {
          float iStepY= ((float)(GUIGraphicsContext.Height-y)) / fTotalTime;
          if (iStepY<=0) iStepY=1;
          posy = y + GUIGraphicsContext.Height- (iStepY*fTime);
          if (posy < y) posy=y;
        }
          break;
        case AnimationType.ZoomInFromMiddle:
        {
          float iStepY= ((float)(height/2)) / (fTotalTime );
          if (iStepY<=0) iStepY=1;
          float iStepX= ((float)(width/2)) / (fTotalTime );
          if (iStepX<=0) iStepX=1;

          iStepY*=fTime;
          iStepX*=fTime;


          posy = y+(height/2)-iStepY;
          posx = x+(width/2)-iStepX;
          h=iStepY*2;
          w=iStepX*2;
        }
        break;
      }
			// and return the modified coordinates,with,height
      x=(int)posx;
      y=(int)posy;
      width=(int)w;
			height=(int)h;
			
    }
		public void Advance(long timePassed)
		{
			lTime+=timePassed;
		}
	}
	
}
