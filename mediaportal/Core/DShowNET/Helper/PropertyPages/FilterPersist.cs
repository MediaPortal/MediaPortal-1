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
using System.IO;
using DirectShowLib;
using MediaPortal.GUI.Library;

namespace DShowNET.Helper
{
  /// <summary>
  /// 
  /// </summary>
  public class FilterPersist
  {
    private ICaptureGraphBuilder2 m_captureGraphBuilder;
    private IBaseFilter m_videoCompressorFilter;
    private IBaseFilter m_audioCompressorFilter;

    public FilterPersist(ICaptureGraphBuilder2 captureGraphBuilder,
                         IBaseFilter videoCompressorFilter,
                         IBaseFilter audioCompressorFilter)
    {
      m_captureGraphBuilder = captureGraphBuilder;
      m_videoCompressorFilter = videoCompressorFilter;
      m_audioCompressorFilter = audioCompressorFilter;
    }


    public void LoadSettings(int ID)
    {
      Log.Info("Load settings card:{0}", ID);
      try
      {
        PropertyPageCollection propertyPages = new PropertyPageCollection(m_captureGraphBuilder, m_videoCompressorFilter,
                                                                          m_audioCompressorFilter);

        foreach (PropertyPage p in propertyPages)
        {
          if (p.SupportsPersisting)
          {
            string strFName = String.Format(@"filters\card{0}_{1}.dat", ID, p.Name);
            using (FileStream fs = new FileStream(strFName, FileMode.Open, FileAccess.Read))
            {
              byte[] byData = new byte[fs.Length];
              fs.Read(byData, 0, (int) fs.Length);
              p.State = byData;
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Info("ex:{0} {1} {2}", ex.Source, ex.StackTrace, ex.Message);
      }
    }


    public void SaveSettings(int ID)
    {
      try
      {
        Log.Info("Save settings card:{0}", ID);
        Directory.CreateDirectory("filters");
        PropertyPageCollection propertyPages = new PropertyPageCollection(m_captureGraphBuilder, m_videoCompressorFilter,
                                                                          m_audioCompressorFilter);
        foreach (PropertyPage p in propertyPages)
        {
          if (p.SupportsPersisting)
          {
            string strFName = String.Format(@"filters\card{0}_{1}.dat", ID, p.Name);
            using (FileStream fs = new FileStream(strFName, FileMode.Create, FileAccess.Write))
            {
              byte[] byData = p.State;
              fs.Write(byData, 0, byData.Length);
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Info("ex:{0} {1} {2}", ex.Source, ex.StackTrace, ex.Message);
      }
    }
  }
}