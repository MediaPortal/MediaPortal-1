/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// Class which can do transformations for video windows
  /// currently it supports Zoom, Zoom 14:9, normal, stretch, original, letterbox 4:3 and panscan 4:3
  /// </summary>
  public class Geometry
  {
    public enum Type
    {
      Zoom,     //widescreen
      Normal,   //pan scan
      Stretch,  //letterbox
      Original, //original source format
      LetterBox43, // Letterbox 4:3
      PanScan43, // Pan&Scan 4:3
      Zoom14to9 // 4:3 on 16:9 screens
    }
    int _imageWidth = 100;				// width of the video window or image
    int _imageHeight = 100;				// height of the height window or image
    int m_ScreenWidth = 100;				// width of the screen
    int m_ScreenHeight = 100;				// height of the screen
    Type m_eType = Type.Normal;			// type of transformation used
    float m_fPixelRatio = 1.0f;				// pixelratio correction 


    /// <summary>
    /// Empty constructor
    /// </summary>
    public Geometry()
    {
    }

    /// <summary>
    /// property to get/set the width of the video/image
    /// </summary>
    public int ImageWidth
    {
      get { return _imageWidth; }
      set { _imageWidth = value; }
    }

    /// <summary>
    /// property to get/set the height of the video/image
    /// </summary>
    public int ImageHeight
    {
      get { return _imageHeight; }
      set { _imageHeight = value; }
    }

    /// <summary>
    /// property to get/set the width of the screen
    /// </summary>
    public int ScreenWidth
    {
      get { return m_ScreenWidth; }
      set { m_ScreenWidth = value; }
    }

    /// <summary>
    /// property to get/set the height of the screen
    /// </summary>
    public int ScreenHeight
    {
      get { return m_ScreenHeight; }
      set { m_ScreenHeight = value; }
    }

    /// <summary>
    /// property to get/set the transformation type
    /// </summary>
    public Geometry.Type ARType
    {
      get { return m_eType; }
      set { m_eType = value; }
    }

    /// <summary>
    /// property to get/set the pixel ratio 
    /// </summary>
    public float PixelRatio
    {
      get { return m_fPixelRatio; }
      set { m_fPixelRatio = value; }
    }

    /// <summary>
    /// Method todo the transformation.
    /// It will calculate 2 rectangles. A source and destination rectangle based on the
    /// current transformation , image width/height and screen width/height
    /// the returned source rectangle specifies which part of the image/video should be copied
    /// the returned destination rectangle specifies where the copied part should be presented on screen
    /// </summary>
    /// <param name="rSource">rectangle containing the source rectangle of the image/video</param>
    /// <param name="rDest">rectangle  containing the destination rectangle of the image/video</param>
    public void GetWindow( out System.Drawing.Rectangle rSource, out System.Drawing.Rectangle rDest )
    {
      float fSourceFrameRatio = CalculateFrameAspectRatio();
      GetWindow(fSourceFrameRatio, out rSource, out rDest);
    }
    public void GetWindow( int arVideoWidth, int arVideoHeight, out System.Drawing.Rectangle rSource, out System.Drawing.Rectangle rDest )
    {
      float fSourceFrameRatio = (float)arVideoWidth / (float)arVideoHeight;
      GetWindow(fSourceFrameRatio, out rSource, out rDest);
    }
    public void GetWindow( float fSourceFrameRatio, out System.Drawing.Rectangle rSource, out System.Drawing.Rectangle rDest )
    {
      float fOutputFrameRatio = fSourceFrameRatio / PixelRatio;

      switch ( ARType )
      {
        case Type.Stretch:
          {
            rSource = new System.Drawing.Rectangle(0, 0, ImageWidth, ImageHeight);
            rDest = new System.Drawing.Rectangle(0, 0, ScreenWidth, ScreenHeight);
          }
          break;

        case Type.Zoom:
          {
            // calculate AR compensation (see http://www.iki.fi/znark/video/conversion)
            // assume that the movie is widescreen first, so use full height
            float fVertBorder = 0;
            float fNewHeight = (float)( ScreenHeight );
            float fNewWidth = fNewHeight * fOutputFrameRatio;
            float fHorzBorder = ( fNewWidth - (float)ScreenWidth ) / 2.0f;
            float fFactor = fNewWidth / ( (float)ImageWidth );
            fHorzBorder = fHorzBorder / fFactor;

            if ( (int)fNewWidth < ScreenWidth )
            {
              fHorzBorder = 0;
              fNewWidth = (float)( ScreenWidth );
              fNewHeight = fNewWidth / fOutputFrameRatio;
              fVertBorder = ( fNewHeight - (float)ScreenHeight ) / 2.0f;
              fFactor = fNewWidth / ( (float)ImageWidth );
              fVertBorder = fVertBorder / fFactor;
            }

            rSource = new System.Drawing.Rectangle((int)fHorzBorder,
                                                 (int)fVertBorder,
                                                 (int)( (float)ImageWidth - 2.0f * fHorzBorder ),
                                                 (int)( (float)ImageHeight - 2.0f * fVertBorder ));
            rDest = new System.Drawing.Rectangle(0, 0, ScreenWidth, ScreenHeight);
          }
          break;

        case Type.Normal:
          {
            // maximize the movie width
            float fNewWidth = (float)ScreenWidth;
            float fNewHeight = (float)( fNewWidth / fOutputFrameRatio );

            if ( fNewHeight > ScreenHeight )
            {
              fNewHeight = ScreenHeight;
              fNewWidth = fNewHeight * fOutputFrameRatio;
            }

            // this shouldnt happen, but just make sure that everything still fits onscreen
            if ( fNewWidth > ScreenWidth || fNewHeight > ScreenHeight )
            {
              fNewWidth = (float)ImageWidth;
              fNewHeight = (float)ImageHeight;
            }

            // Centre the movie
            float iPosY = ( ScreenHeight - fNewHeight ) / 2;
            float iPosX = ( ScreenWidth - fNewWidth ) / 2;

            rSource = new System.Drawing.Rectangle(0, 0, ImageWidth, ImageHeight);
            rDest = new System.Drawing.Rectangle((int)iPosX, (int)iPosY, (int)( fNewWidth + 0.5f ), (int)( fNewHeight + 0.5f ));
          }
          break;

        case Type.Original:
          {
            // maximize the movie width
            float fNewWidth = (float)ImageWidth;
            float fNewHeight = (float)( fNewWidth / fOutputFrameRatio );

            if ( fNewHeight > ScreenHeight )
            {
              fNewHeight = ImageHeight;
              fNewWidth = fNewHeight * fOutputFrameRatio;
            }

            // this shouldnt happen, but just make sure that everything still fits onscreen
            if ( fNewWidth > ScreenWidth || fNewHeight > ScreenHeight )
            {
              goto case Type.Normal;
            }

            // Centre the movie
            float iPosY = ( ScreenHeight - fNewHeight ) / 2;
            float iPosX = ( ScreenWidth - fNewWidth ) / 2;

            rSource = new System.Drawing.Rectangle(0, 0, ImageWidth, ImageHeight);
            rDest = new System.Drawing.Rectangle((int)iPosX, (int)iPosY, (int)( fNewWidth + 0.5f ), (int)( fNewHeight + 0.5f ));
          }
          break;

        case Type.LetterBox43:
          {
            // shrink movie 33% vertically
            float fNewWidth = (float)ScreenWidth;
            float fNewHeight = (float)( fNewWidth / fOutputFrameRatio );
            fNewHeight *= ( 1.0f - 0.33333333333f );

            if ( fNewHeight > ScreenHeight )
            {
              fNewHeight = ScreenHeight;
              fNewHeight *= ( 1.0f - 0.33333333333f );
              fNewWidth = fNewHeight * fOutputFrameRatio;
            }

            // this shouldnt happen, but just make sure that everything still fits onscreen
            if ( fNewWidth > ScreenWidth || fNewHeight > ScreenHeight )
            {
              fNewWidth = (float)ImageWidth;
              fNewHeight = (float)ImageHeight;
            }

            // Centre the movie
            float iPosY = ( ScreenHeight - fNewHeight ) / 2;
            float iPosX = ( ScreenWidth - fNewWidth ) / 2;

            rSource = new System.Drawing.Rectangle(0, 0, ImageWidth, ImageHeight);
            rDest = new System.Drawing.Rectangle((int)iPosX, (int)iPosY, (int)( fNewWidth + 0.5f ), (int)( fNewHeight + 0.5f ));

          }
          break;

        case Type.PanScan43:
          {
            // assume that the movie is widescreen first, so use full height
            float fVertBorder = 0;
            float fNewHeight = (float)( ScreenHeight );
            float fNewWidth = fNewHeight * fOutputFrameRatio * 1.66666666667f;
            float fHorzBorder = ( fNewWidth - (float)ScreenWidth ) / 2.0f;
            float fFactor = fNewWidth / ( (float)ImageWidth );
            fHorzBorder = fHorzBorder / fFactor;

            if ( (int)fNewWidth < ScreenWidth )
            {
              fHorzBorder = 0;
              fNewWidth = (float)( ScreenWidth );
              fNewHeight = fNewWidth / fOutputFrameRatio;
              fVertBorder = ( fNewHeight - (float)ScreenHeight ) / 2.0f;
              fFactor = fNewWidth / ( (float)ImageWidth );
              fVertBorder = fVertBorder / fFactor;
            }

            rSource = new System.Drawing.Rectangle((int)fHorzBorder,
                                                  (int)fVertBorder,
                                                  (int)( (float)ImageWidth - 2.0f * fHorzBorder ),
                                                  (int)( (float)ImageHeight - 2.0f * fVertBorder ));
            rDest = new System.Drawing.Rectangle(0, 0, ScreenWidth, ScreenHeight);
          }
          break;

        case Type.Zoom14to9:
          {
            // fit the image to screen size
            float fNewWidth = (float)ScreenWidth;
            float fNewHeight = (float)( fNewWidth / fOutputFrameRatio );

            if ( fNewHeight > ScreenHeight )
            {
              fNewHeight = ScreenHeight;
              fNewWidth = fNewHeight * fOutputFrameRatio;
            }

            float iPosX = 0;
            float iPosY = 0;
            float fVertBorder = 0;
            float fHorzBorder = 0;
            float fFactor = fNewWidth / ( (float)ImageWidth );
            // increase the image size by 12.5% and crop or pad if needed
            fNewHeight = fNewHeight * 1.125f;
            fNewWidth = fNewHeight * fOutputFrameRatio;

            if ( (int)fNewHeight < ScreenHeight )
            {
              fHorzBorder = ( fNewWidth - (float)ScreenWidth ) / 2.0f;
              fHorzBorder = fHorzBorder / fFactor;
              iPosY = ( ScreenHeight - fNewHeight ) / 2;
            }

            if ( (int)fNewWidth < ScreenWidth )
            {
              fVertBorder = ( fNewHeight - (float)ScreenHeight ) / 2.0f;
              fVertBorder = fVertBorder / fFactor;
              iPosX = ( ScreenWidth - fNewWidth ) / 2;
            }

            if ( (int)fNewWidth > ScreenWidth && (int)fNewHeight > ScreenHeight )
            {
              fHorzBorder = ( fNewWidth - (float)ScreenWidth ) / 2.0f;
              fHorzBorder = fHorzBorder / fFactor;
              fVertBorder = ( fNewHeight - (float)ScreenHeight ) / 2.0f;
              fVertBorder = fVertBorder / fFactor;
            }

            rSource = new System.Drawing.Rectangle((int)fHorzBorder,
                                                 (int)fVertBorder,
                                                 (int)( (float)ImageWidth - 2.0f * fHorzBorder ),
                                                 (int)( (float)ImageHeight - 2.0f * fVertBorder ));
            rDest = new System.Drawing.Rectangle((int)iPosX, (int)iPosY, (int)( fNewWidth - ( 2.0f * fHorzBorder * fFactor ) + 0.5f ), (int)( fNewHeight - ( 2.0f * fVertBorder * fFactor ) + 0.5f ));
          }
          break;

        default:
          {
            rSource = new System.Drawing.Rectangle(0, 0, ImageWidth, ImageHeight);
            rDest = new System.Drawing.Rectangle(0, 0, ScreenWidth, ScreenHeight);
          }
          break;
      }
    }


    /// <summary>
    /// Calculates the aspect ratio for the current image/video window
    /// <returns>float value containing the aspect ratio
    /// </returns>
    /// </summary>
    float CalculateFrameAspectRatio()
    {
      return (float)ImageWidth / (float)ImageHeight;
    }

  }
}
