#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using MediaPortal.Player;

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
          Normal,       // pan scan
          Original,     // original source format
          Zoom,         // widescreen
          Zoom14to9,    // 4:3 on 16:9 screens
          Stretch,      // letterbox
          LetterBox43,  // letterbox 4:3
          SmartStretch  // smart stretch zoom
        }

        int _imageWidth = 100;				// width of the video window or image
        int _imageHeight = 100;				// height of the height window or image
        int m_ScreenWidth = 100;				// width of the screen
        int m_ScreenHeight = 100;				// height of the screen
        Type m_eType = Type.Normal;			// type of transformation used
        float m_fPixelRatio = 1.0f;				// pixelratio correction 
        bool m_bUseNonLinearStretch = false; //AR uses non-linear stretch or not




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
        public void GetWindow(out System.Drawing.Rectangle rSource, out System.Drawing.Rectangle rDest)
        {
            float fSourceFrameRatio = CalculateFrameAspectRatio();
            CropSettings cropSettings = new CropSettings();
            GetWindow(fSourceFrameRatio, out rSource, out rDest, cropSettings);
        }

        public void GetWindow(int arVideoWidth, int arVideoHeight, out System.Drawing.Rectangle rSource, out System.Drawing.Rectangle rDest)
        {
            CropSettings cropSettings = new CropSettings();
            GetWindow(arVideoWidth, arVideoHeight, out rSource, out rDest, cropSettings);
        }

        // used from planescene
        public void GetWindow(int arVideoWidth, int arVideoHeight, out System.Drawing.Rectangle rSource, out System.Drawing.Rectangle rDest, CropSettings cropSettings)
        {
            float fSourceFrameRatio = (float)arVideoWidth / (float)arVideoHeight;
            GetWindow(fSourceFrameRatio, out rSource, out rDest, cropSettings);
        }
        public void GetWindow(int arVideoWidth, int arVideoHeight, out System.Drawing.Rectangle rSource, out System.Drawing.Rectangle rDest, out bool bUseNonLinearStretch, CropSettings cropSettings)
        {
            GetWindow(arVideoWidth, arVideoHeight, out rSource, out rDest, cropSettings);
            bUseNonLinearStretch = m_bUseNonLinearStretch;
        }
        public void GetWindow(float fSourceFrameRatio, out System.Drawing.Rectangle rSource, out System.Drawing.Rectangle rDest, CropSettings cropSettings)
        {
            float fOutputFrameRatio = fSourceFrameRatio / PixelRatio;

            // make sure the crop settings are acceptable
            cropSettings = cropSettings.EnsureSanity(ImageWidth, ImageHeight);

            int cropW = cropSettings.Left + cropSettings.Right;
            int cropH = cropSettings.Top + cropSettings.Bottom;

            // the source image dimensions when taking into
            // account the crop settings
            int croppedImageWidth = ImageWidth - cropW;
            int croppedImageHeight = ImageHeight - cropH;
            
            // suggested by ziphnor
            float fSourcePixelRatio = fSourceFrameRatio / ((float)ImageWidth / (float)ImageHeight);
            float fCroppedOutputFrameRatio = fSourcePixelRatio * ((float)croppedImageWidth / (float)croppedImageHeight) / PixelRatio;

            //Log.Debug("croppedImage W/H {0}x{1}", croppedImageWidth, croppedImageHeight);
            //Log.Debug("fOutputFrameRatio : {0}", fOutputFrameRatio);
            //Log.Debug("fCroppedOutputFrameRatio : {0}", fCroppedOutputFrameRatio);
            //Log.Debug("fSourceFrameRatio: {0}", fSourceFrameRatio);
            
            //don't use non linear stretch by default
            m_bUseNonLinearStretch = false;          

            switch (ARType)
            {
                case Type.Stretch:
                    {
                        rSource = new System.Drawing.Rectangle(cropSettings.Left, cropSettings.Top, croppedImageWidth, croppedImageHeight);
                        rDest = new System.Drawing.Rectangle(0, 0, ScreenWidth, ScreenHeight);
                    }
                    break;

                case Type.Zoom:
                    {
                        // calculate AR compensation (see http://www.iki.fi/znark/video/conversion)
                        // assume that the movie is widescreen first, so use full height
                        float fVertBorder = 0;
                        float fNewHeight = (float)(ScreenHeight);
                        float fNewWidth = fNewHeight * fCroppedOutputFrameRatio;
                        float fHorzBorder = (fNewWidth - (float)ScreenWidth) / 2.0f;
                        float fFactor = fNewWidth / ((float)ImageWidth);
                        fFactor *= PixelRatio;
                        fHorzBorder = fHorzBorder / fFactor;

                        if ((int)fNewWidth < ScreenWidth)
                        {
                            fHorzBorder = 0;
                            fNewWidth = (float)(ScreenWidth);
                            fNewHeight = fNewWidth / fCroppedOutputFrameRatio;
                            fVertBorder = (fNewHeight - (float)ScreenHeight) / 2.0f;
                            fFactor = fNewWidth / ((float)ImageWidth);
                            fFactor *= PixelRatio;
                            fVertBorder = fVertBorder / fFactor;
                        }

                        rSource = new System.Drawing.Rectangle((int)fHorzBorder,
                                                             (int)fVertBorder,
                                                             (int)((float)ImageWidth - 2.0f * fHorzBorder),
                                                             (int)((float)ImageHeight - 2.0f * fVertBorder));
                        rDest = new System.Drawing.Rectangle(0, 0, ScreenWidth, ScreenHeight);
                        AdjustSourceForCropping(ref rSource, cropSettings);
                    }
                    break;

                  case Type.Normal:
                    {
                      // maximize the movie width
                      float fNewWidth = (float)ScreenWidth;
                      float fNewHeight = (float)(fNewWidth / fCroppedOutputFrameRatio);

                      if (fNewHeight > ScreenHeight)
                      {
                        fNewHeight = ScreenHeight;
                        fNewWidth = fNewHeight * fCroppedOutputFrameRatio;
                      }

                      // this shouldnt happen, but just make sure that everything still fits onscreen
                      if (fNewWidth > ScreenWidth || fNewHeight > ScreenHeight)
                      {
                        fNewWidth = (float)ImageWidth;
                            fNewWidth = (float)croppedImageWidth;
                            fNewHeight = (float)croppedImageHeight;
                      }

                      // Centre the movie
                      float iPosY = (ScreenHeight - fNewHeight) / 2;
                      float iPosX = (ScreenWidth - fNewWidth) / 2;

                        rSource = new System.Drawing.Rectangle(cropSettings.Left, cropSettings.Top, croppedImageWidth, croppedImageHeight);
                      rDest = new System.Drawing.Rectangle((int)iPosX, (int)iPosY, (int)(fNewWidth + 0.5f), (int)(fNewHeight + 0.5f));

                      //AdjustForCropping(ref rSource, ref rDest, cropSettings, true);
                    }
                    break;                    

                  case Type.Original:
                    {
                        // maximize the movie width
                        float fNewWidth = (float)Math.Min(ImageWidth, ScreenWidth);
                        float fNewHeight = (float)(fNewWidth / fOutputFrameRatio);

                        if (fNewHeight > ScreenHeight)
                        {
                            fNewHeight = Math.Min(ImageHeight,ScreenHeight);
                            fNewWidth = fNewHeight * fOutputFrameRatio;
                        }

                        // this shouldnt happen, but just make sure that everything still fits onscreen
                        if (fNewWidth > ScreenWidth || fNewHeight > ScreenHeight)
                        {
                            Log.Error("Original Zoom Mode: 'this shouldnt happen' {0}x{1}",fNewWidth,fNewHeight);
                            goto case Type.Normal;
                        }

                        // Centre the movie
                        float iPosY = (ScreenHeight - fNewHeight) / 2;
                        float iPosX = (ScreenWidth - fNewWidth) / 2;

                        // The original zoom mode ignores cropping parameters:
                        rSource = new System.Drawing.Rectangle(0, 0, ImageWidth, ImageHeight);
                        rDest = new System.Drawing.Rectangle((int)iPosX, (int)iPosY, (int)(fNewWidth + 0.5f), (int)(fNewHeight + 0.5f));
                    }
                    break;
                
                case Type.LetterBox43:
                    {
                        // shrink movie 33% vertically
                        float fNewWidth = (float)ScreenWidth;
                        float fNewHeight = (float)(fNewWidth / fOutputFrameRatio);
                        fNewHeight *= (1.0f - 0.33333333333f);

                        if (fNewHeight > ScreenHeight)
                        {
                            fNewHeight = ScreenHeight;
                            fNewHeight *= (1.0f - 0.33333333333f);
                            fNewWidth = fNewHeight * fOutputFrameRatio;
                        }

                        // this shouldnt happen, but just make sure that everything still fits onscreen
                        if (fNewWidth > ScreenWidth || fNewHeight > ScreenHeight)
                        {
                            fNewWidth = (float)ImageWidth;
                            fNewHeight = (float)ImageHeight;
                        }

                        // Centre the movie
                        float iPosY = (ScreenHeight - fNewHeight) / 2;
                        float iPosX = (ScreenWidth - fNewWidth) / 2;

                        rSource = new System.Drawing.Rectangle(0, 0, ImageWidth, ImageHeight);
                        rDest = new System.Drawing.Rectangle((int)iPosX, (int)iPosY, (int)(fNewWidth + 0.5f), (int)(fNewHeight + 0.5f));
                        AdjustSourceForCropping(ref rSource, cropSettings);
                    }
                    break;

                  case Type.SmartStretch:
                    {
                      // If screen is 16:9 do non-linear stretch, otherwise panscan
                      float fScreenRatio = (float)ScreenWidth / ScreenHeight;
                      fScreenRatio *= PixelRatio;
                      if (fScreenRatio < 1.6)
                      {
                        // pan and scan
                        // assume that the movie is widescreen first, so use full height
                        float fVertBorder = 0;
                        float fNewHeight = (float)(ScreenHeight);
                        float fNewWidth = fNewHeight * fOutputFrameRatio * 1.66666666667f;
                        float fHorzBorder = (fNewWidth - (float)ScreenWidth) / 2.0f;
                        float fFactor = fNewWidth / ((float)ImageWidth);
                        fFactor *= PixelRatio;
                        fHorzBorder = fHorzBorder / fFactor;

                        if ((int)fNewWidth < ScreenWidth)
                        {
                          fHorzBorder = 0;
                          fNewWidth = (float)(ScreenWidth);
                          fNewHeight = fNewWidth / fOutputFrameRatio;
                          fVertBorder = (fNewHeight - (float)ScreenHeight) / 2.0f;
                          fFactor = fNewWidth / ((float)ImageWidth);
                          fFactor *= PixelRatio;
                          fVertBorder = fVertBorder / fFactor;
                        }

                        rSource = new System.Drawing.Rectangle((int)fHorzBorder,
                                                              (int)fVertBorder,
                                                              (int)((float)ImageWidth - 2.0f * fHorzBorder),
                                                              (int)((float)ImageHeight - 2.0f * fVertBorder));
                        rDest = new System.Drawing.Rectangle(0, 0, ScreenWidth, ScreenHeight);
                        AdjustSourceForCropping(ref rSource, cropSettings);
                      }
                      else
                      {
                        m_bUseNonLinearStretch = true;
                        //here we just need to remove 8.3% from top/bottom of the image (=Zoom in 9.1%)
                        //The non-linear stretching part will be handled by the VMR9 presenter
                        int newTop = cropSettings.Top + (int)(0.083f * ((float)croppedImageHeight) / 2.0f);
                        int newHeight = (int)(((float)croppedImageHeight) * (1.0f - 0.083f));

                        rSource = new System.Drawing.Rectangle(cropSettings.Left, newTop, croppedImageWidth, newHeight);
                        rDest = new System.Drawing.Rectangle(0, 0, ScreenWidth, ScreenHeight);
                      }
                    }
                    break;

                case Type.Zoom14to9:
                    {
                        // fit the image to screen size
                        float fNewWidth = (float)ScreenWidth;
                        float fNewHeight = (float)(fNewWidth / fOutputFrameRatio);

                        if (fNewHeight > ScreenHeight)
                        {
                            fNewHeight = ScreenHeight;
                            fNewWidth = fNewHeight * fOutputFrameRatio;
                        }

                        float iPosX = 0;
                        float iPosY = 0;
                        float fVertBorder = 0;
                        float fHorzBorder = 0;
                        float fFactor = fNewWidth / ((float)ImageWidth);
                        fFactor *= PixelRatio;
                        // increase the image size by 12.5% and crop or pad if needed
                        fNewHeight = fNewHeight * 1.125f;
                        fNewWidth = fNewHeight * fOutputFrameRatio;

                        if ((int)fNewHeight < ScreenHeight)
                        {
                            fHorzBorder = (fNewWidth - (float)ScreenWidth) / 2.0f;
                            fHorzBorder = fHorzBorder / fFactor;
                            iPosY = (ScreenHeight - fNewHeight) / 2;
                        }

                        if ((int)fNewWidth < ScreenWidth)
                        {
                            fVertBorder = (fNewHeight - (float)ScreenHeight) / 2.0f;
                            fVertBorder = fVertBorder / fFactor;
                            iPosX = (ScreenWidth - fNewWidth) / 2;
                        }

                        if ((int)fNewWidth > ScreenWidth && (int)fNewHeight > ScreenHeight)
                        {
                            fHorzBorder = (fNewWidth - (float)ScreenWidth) / 2.0f;
                            fHorzBorder = fHorzBorder / fFactor;
                            fVertBorder = (fNewHeight - (float)ScreenHeight) / 2.0f;
                            fVertBorder = fVertBorder / fFactor;
                        }

                        rSource = new System.Drawing.Rectangle((int)fHorzBorder,
                                                             (int)fVertBorder,
                                                             (int)((float)ImageWidth - 2.0f * fHorzBorder),
                                                             (int)((float)ImageHeight - 2.0f * fVertBorder));
                        rDest = new System.Drawing.Rectangle((int)iPosX, (int)iPosY, (int)(fNewWidth - (2.0f * fHorzBorder * fFactor) + 0.5f), (int)(fNewHeight - (2.0f * fVertBorder * fFactor) + 0.5f));
                        AdjustSourceForCropping(ref rSource, cropSettings);
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
        /// Adjusts the source and destination rectangles according to the cropping parameters, maintaining the RATIO between the source and destination
        /// aspect ratio.
        /// 
        /// Note:
        /// Only used for the normal aspect right now, so could as well be coded directly into that zoom mode. 
        /// But maybe it could be useful elsewhere as well and no changes have had to be made to the zoom mode code.
        /// </summary>
        /// <param name="rSource"></param>
        /// <param name="rDest"></param>
        /// <param name="cropSettings"></param>
        /*
        void AdjustForCropping(ref System.Drawing.Rectangle rSource, ref System.Drawing.Rectangle rDest, CropSettings cropSettings, bool strictKeepAspect)
        {
          // temp
          //return;
          float destAspect = rDest.Width / (float)rDest.Height;
          float sourceAspect = rSource.Width / (float)rSource.Height;
          float croppedSourceAspect = (rSource.Width - cropSettings.Right - cropSettings.Left) / (float)(rSource.Height - cropSettings.Top - cropSettings.Bottom);
          float originalAspectChange = destAspect / sourceAspect;

      
          //if (( Math.Abs( destAspect - sourceAspect ) < 0.001 ))
          //  return;

          Log.Debug("AdjustForCropping: rSrc =  " + rSource.ToString());
          Log.Debug("AdjustForCropping: rDest =  " + rDest.ToString());

          float newDestAspect = croppedSourceAspect * originalAspectChange;

          Log.Debug("newDestAspect: " + newDestAspect + " destAspect: " + destAspect);
          Log.Debug("sourceAspect: " + sourceAspect + " croppedSrcAspect: " + croppedSourceAspect);

          if (newDestAspect > destAspect)
          {
            Log.Debug("CROPADJUST : DEST NOT WIDE ENOUGH (ie someone cropped top or bottom)");
            // destination needs to be wider

            // width needed to preserve height
            float widthNeeded = rDest.Height * newDestAspect;

            if (widthNeeded > ScreenWidth && strictKeepAspect)
            {
              // decrease height
              Log.Info("CROPADJUST : NOT ENOUGH WIDTH, needs " + widthNeeded);
              // take all the width we can
              rDest.Width = ScreenWidth;
              // and reduce height for the rest
              rDest.Height = (int)(rDest.Width / newDestAspect); ;
              Log.Info("New height : " + rDest.Height);
            }
            else
            {
              Log.Info("CROPADJUST : ENOUGH WIDTH");
              rDest.Width = (int)widthNeeded;
            }
          }
          else if (newDestAspect < destAspect)
          {
            // destination needs to be taller
            Log.Info("CROPADJUST : DEST TOO WIDE (ie someone cropped left or right");

            int heightNeeded = (int)(rDest.Width * newDestAspect);
            int heightIncrease = heightNeeded - rDest.Height;

            Log.Info("HeightNeeded = " + heightNeeded);
            Log.Info("heightIncrease= " + heightIncrease);

            if (heightNeeded > ScreenHeight && strictKeepAspect)
            {
              Log.Info("CROPADJUST: NOT ENOUGH HEIGHT, ADJUSTING WIDTH");
              rDest.Height = ScreenHeight;
              int newWidth = (int)(newDestAspect * rDest.Height);
              rDest.Width = newWidth;
              Log.Debug("rDest.Width now " + newWidth);
            }
            else
            {
              rDest.Height = heightNeeded;
            }
          }

          rDest.Y = ScreenHeight / 2 - rDest.Height / 2;
          rDest.X = ScreenWidth / 2 - rDest.Width / 2;

          if (rDest.X < 0) Log.Error("Geometry.AdjustForCropping miscalculated, produced NEGATIVE X coordinate!");
          if (rDest.Y < 0) Log.Error("Geometry.AdjustForCropping miscalculated, produced NEGATIVE Y coordinate!");

          AdjustSourceForCropping(ref rSource, cropSettings);
        }
        */
        /// <summary>
        /// Adjusts only the source rectangle according to the cropping parameters.
        /// </summary>
        /// <param name="rSource"></param>
        /// <param name="cropSettings"></param>
        void AdjustSourceForCropping(ref System.Drawing.Rectangle rSource, CropSettings cropSettings)
        {
            rSource.Y += cropSettings.Top;
            rSource.Height -= cropSettings.Top;
            rSource.Height -= cropSettings.Bottom;

            rSource.X += cropSettings.Left;
            rSource.Width -= cropSettings.Left;
            rSource.Width -= cropSettings.Right;
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
