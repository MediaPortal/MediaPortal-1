#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// Class which can do animatons on controls
  /// </summary>
  public class Animator
  {
    // type of animations
    public enum AnimationType
    {
      None,
      FlyInFromLeft,
      FlyInFromRight,
      FlyInFromTop,
      FlyInFromBottom,
      ZoomInFromMiddle
    } ;


    protected AnimationType animType = AnimationType.None; //current animation type
    protected bool m_Animating = false; //boolean indicating if we're animating
    protected float fTime;

    public Animator(AnimationType type)
    {
      animType = type;
      fTime = 0f;
      m_Animating = true;
    }

    private float GetTotalFrameTime()
    {
      // Frames e.g. 25  /  Frame time in msec
      return ((float)GUIGraphicsContext.MaxFPS / 2) * (1 / (float)GUIGraphicsContext.MaxFPS);
    }

    /// <summary>
    /// Method which returns true if the animation has ended
    /// </summary>
    /// <returns>true : animation has ended
    ///          false: animation is still busy
    /// </returns>
    public bool IsDone()
    {
      if (!m_Animating)
      {
        return true;
      }
      if (animType == AnimationType.None)
      {
        return true;
      }
      return false;
    }

    public void AnimateColor(ref long colorDiffuse)
    {
      //if animation has ended, then just return
      if (IsDone())
      {
        return;
      }

      //check if animation should end
      float fTotalTime = GetTotalFrameTime();
      if (fTime >= fTotalTime)
      {
        //yes, then end the animation
        m_Animating = false;
        return;
      }
      float alphaValue = (float)(colorDiffuse >> 24);
      float fPercent = fTime / fTotalTime;
      alphaValue *= fPercent;
      long lAlpha = (long)alphaValue;
      lAlpha <<= 24;
      colorDiffuse = colorDiffuse & 0x00ffffff;
      colorDiffuse |= lAlpha;
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
    public void Animate(float timePassed, ref int x, ref int y, ref int width, ref int height, ref long colorDiffuse)
    {
      //if animation has ended, then just return
      if (IsDone())
      {
        return;
      }

      //check if animation should end
      float fTotalTime = GetTotalFrameTime();
      float iStepX = 0;
      float iStepY = 0;
      if (fTime >= fTotalTime)
      {
        //yes, then end the animation
        m_Animating = false;
        return;
      }

      //keep copy of original control rectangle
      float posx = (float)x;
      float posy = (float)y;
      float w = (float)width;
      float h = (float)height;
      /*
            float alphaValue= (float)(colorDiffuse >>24);
            float fPercent=fTime/fTotalTime;
            alphaValue *=fPercent;
            long lAlpha=(long)alphaValue;
            lAlpha<<=24;
            colorDiffuse =colorDiffuse  & 0x00ffffff;
            colorDiffuse|=lAlpha;
      */

      //modify the coordinates,width,height for the current animation type
      switch (animType)
      {
        case AnimationType.FlyInFromLeft:
          iStepX = ((float)(x + width)) / fTotalTime;
          if (iStepX <= 0)
          {
            iStepX = 1;
          }
          posx = iStepX * fTime;
          posx -= (float)width;
          if (posx > x)
          {
            posx = x;
          }
          break;
        case AnimationType.FlyInFromRight:
          iStepX = ((float)(GUIGraphicsContext.Width - x)) / fTotalTime;
          if (iStepX <= 0)
          {
            iStepX = 1;
          }
          posx = x + GUIGraphicsContext.Width - (iStepX * fTime);
          if (posx < x)
          {
            posx = x;
          }
          break;
        case AnimationType.FlyInFromTop:
          iStepY = ((float)(y + height)) / fTotalTime;
          if (iStepY <= 0)
          {
            iStepY = 1;
          }
          posy = iStepY * fTime;
          posy -= height;
          if (posy > y)
          {
            posy = y;
          }
          break;
        case AnimationType.FlyInFromBottom:
          iStepY = ((float)(GUIGraphicsContext.Height - y)) / fTotalTime;
          if (iStepY <= 0)
          {
            iStepY = 1;
          }
          posy = GUIGraphicsContext.Height - (iStepY * fTime);
          if (posy < y)
          {
            posy = y;
          }
          break;
        case AnimationType.ZoomInFromMiddle:
          iStepY = ((float)(height / 2)) / (fTotalTime);
          if (iStepY <= 0)
          {
            iStepY = 1;
          }
          iStepX = ((float)(width / 2)) / (fTotalTime);
          if (iStepX <= 0)
          {
            iStepX = 1;
          }

          iStepY *= fTime;
          iStepX *= fTime;


          posy = y + (height / 2) - iStepY;
          posx = x + (width / 2) - iStepX;
          h = iStepY * 2;
          w = iStepX * 2;
          break;
      }
      // and return the modified coordinates,with,height
      x = (int)posx;
      y = (int)posy;
      width = (int)w;
      height = (int)h;
    }

    public void Advance(float timePassed)
    {
      fTime += timePassed;
      if (fTime >= GetTotalFrameTime())
      {
        //yes, then end the animation
        m_Animating = false;
        return;
      }
    }
  }
}