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

/*  Geochron.cs
 *
 *  Class to create bitmap todisplay day/night regions of Earth
 *
 *  By Andy Qua
 *
 * Uses code derived from work by Rich Townsend (rhdt@star.ucl.ac.uk)
 *
 *  Distributed under the GNU GPL; see:
 *   http://www.gnu.org/licenses/gpl.html
 */

using System;
using System.Drawing;
using MediaPortal.Util;

namespace MediaPortal.GUI.Weather
{
  public class Geochron
  {
    private const double DtoR = Math.PI / 180;
    private const double RtoD = 180 / Math.PI;

    private const double dayAltMin = 0; // Minimum solar altitude for daytime
    private const double nightAltMax = -9; // Maximum solar altitude for nighttime

    private ImageHandler day;
    private ImageHandler night;

    private int n_x;
    private int n_y;

    //    bool simMode = false;
    //    int offset = 0;

    public Geochron(string path)
    {
      // Set up the image URLs
      Bitmap dayImage = new Bitmap(path + "/animations/day.png");
      Bitmap nightImage = new Bitmap(path + "/animations/night.png");


      this.n_x = dayImage.Width;
      this.n_y = dayImage.Height;

      // Store the images
      day = new ImageHandler(dayImage);
      night = new ImageHandler(nightImage);
    }

    public void getWidthHeight(out int width, out int height)
    {
      width = n_x;
      height = n_y;
    }


    public void update(ref Bitmap bitmap, DateTime time)
    {
      ImageHandler blend = new ImageHandler(bitmap);
      // Lock bitmaps
      day.lockBitmap();
      night.lockBitmap();
      blend.lockBitmap();

      // Calculate the Greenwich Sidereal Time
      double GST = getGreenwichSiderealTime(time);

      // Calculate the solar right ascension and declination

      double alpha = getSolarRightAscension(time);
      double delta = getSolarDeclination(time);

      // Loop over the y-pixels of the night/day images
      int i_x;
      int i_y;

      for (i_x = 0; i_x < this.n_x; i_x++)
      {
        // Calculate the longitude
        double longitude = 180 - (i_x + 0.5) / this.n_x * 360;

        // Calculate the solar hour angle
        double HA = GST * 360 / 24 - longitude - alpha;

        for (i_y = 0; i_y < this.n_y; i_y++)
        {
          // Calculate the latitude
          double latitude = 90 - (i_y + 0.5) / this.n_y * 180;

          // Calculate the altitude of the sun
          double alt = getAltitude(HA, delta, latitude, longitude);

          // Work out the interpolation factor for drawing the pixel
          double intFactor;

          if (alt > dayAltMin)
          {
            intFactor = 1;
          }
          else if (alt < nightAltMax)
          {
            intFactor = 0;
          }
          else
          {
            intFactor = (alt - nightAltMax) / (dayAltMin - nightAltMax);
          }

          // Get the RGB pixels of the day and night images
          int dayRGB = (int)this.day.getPixel(i_x, i_y);
          int nightRGB = (int)this.night.getPixel(i_x, i_y);

          int dayR = dayRGB >> 16 & 0xFF;
          int dayG = dayRGB >> 8 & 0xFF;
          int dayB = dayRGB & 0xFF;

          int nightR = nightRGB >> 16 & 0xFF;
          int nightG = nightRGB >> 8 & 0xFF;
          int nightB = nightRGB & 0xFF;

          // Calculate the interpolated value of the blended pixel
          int blendedRGB = (int)(dayR * intFactor + nightR * (1 - intFactor)) << 16 |
                           (int)(dayG * intFactor + nightG * (1 - intFactor)) << 8 |
                           (int)(dayB * intFactor + nightB * (1 - intFactor));

          blend.setPixel(i_x, i_y, blendedRGB);
        }
      }
      // Lock bitmaps
      day.unlockBitmap();
      night.unlockBitmap();
      blend.unlockBitmap();
    }

    private double getDaysSinceJ2000(DateTime calendar)
    {
      // Calculate the number of days from the epoch J2000.0
      // the specified date
      int year = calendar.Year;
      int month = calendar.Month;
      int day = calendar.Day;

      double D0 = 367 * year - 7 * (year + (month + 9) / 12) / 4 + 275 * month / 9 + day - 730531.5;
      double D = D0 + getGreenwichMeanTime(calendar) / 24;

      return D;
    }

    private double getGreenwichMeanTime(DateTime calendar)
    {
      // Get the Greenwich Mean Time, in hours
      int hour = calendar.Hour;
      int minute = calendar.Minute;
      int second = calendar.Second;

      double GMT = hour + minute / 60 + second / 3600;
      return GMT;
    }


    private double getGreenwichSiderealTime(DateTime calendar)
    {
      // Get the number of days since J2000.0
      double D = getDaysSinceJ2000(calendar);

      // Calculate the GST
      double T = D / 36525;
      double GST = (280.46061837 + 360.98564736629 * D + 0.000388 * T * T) * 24 / 360;

      // Phase it to within 24 hours
      while (GST < 0)
      {
        GST += 24;
      }
      while (GST >= 24)
      {
        GST -= 24;
      }

      return GST;
    }


    private double getSolarRightAscension(DateTime calendar)
    {
      // Calculate the number of days from the epoch J2000.0
      double D = getDaysSinceJ2000(calendar);

      // Convert this into centuries
      double T = D / 36525;

      // Calculate the mean longitude and anomaly
      double L = 279.697 + 36000.769 * T;
      double M = 358.476 + 35999.050 * T;

      // Calculate the true longitude
      double lambda = L + (1.919 - 0.005 * T) * Math.Sin(M * DtoR) + 0.020 * Math.Sin(2 * M * DtoR);

      // Calculate the obliquity
      double epsilon = 23.452 - 0.013 * T;

      // Calculate the right ascension, in degrees
      double alpha = Math.Atan2(Math.Sin(lambda * DtoR) * Math.Cos(epsilon * DtoR), Math.Cos(lambda * DtoR)) * RtoD;

      return alpha;
    }


    private double getSolarDeclination(DateTime calendar)
    {
      // Calculate the number of days from the epoch J2000.0
      double D = getDaysSinceJ2000(calendar) + getGreenwichMeanTime(calendar) / 24;

      // Convert this into centuries
      double T = D / 36525;

      // Calculate the obliquity
      double epsilon = 23.452 - 0.013 * T;

      // Calculate the declination, in degrees
      double delta = Math.Asin(Math.Sin(getSolarRightAscension(calendar) * DtoR) * Math.Sin(epsilon * DtoR)) * RtoD;

      return delta;
    }

    private double getAltitude(double HA, double delta, double latitude, double longitude)
    {
      // Calculate the altitude, in degrees
      double alt = Math.Asin(Math.Sin(latitude * DtoR) * Math.Sin(delta * DtoR) +
                             Math.Cos(latitude * DtoR) * Math.Cos(delta * DtoR) * Math.Cos(HA * DtoR)) * RtoD;

      return alt;
    }
  }
}