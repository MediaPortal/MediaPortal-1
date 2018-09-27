#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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

using System;
using DirectShowLib;
using DShowNET.Helper;
using MediaPortal.Player.PostProcessing;
using MediaPortal.Profile;

namespace MediaPortal.Player.LAV
{
  public class LavEngine : IPostProcessingEngine
  {
    private IBaseFilter _baseFilterLavAudio;
    private bool hasPostProcessing = false;
    protected int audiodelayInterval;

    #region ISubEngine Members

    public bool LoadSubtitles(IGraphBuilder graphBuilder, string filename)
    {
      DirectShowUtil.FindFilterByClassID(graphBuilder, ClassId.LAVAudio, out _baseFilterLavAudio);
      if (_baseFilterLavAudio == null)
        return false;

      return true;
    }

   #endregion

    #region IPostProcessing Members

    public bool EnableDeinterlace { get; set; }
    public bool EnableCrop { get; set; }

    public bool LoadPostProcessing(IGraphBuilder graphBuilder)
    {
      using (Settings xmlreader = new MPSettings())
      {
        audiodelayInterval = xmlreader.GetValueAsInt("FFDShow", "audiodelayInterval", 50);
      }

      DirectShowUtil.FindFilterByClassID(graphBuilder, ClassId.LAVAudio, out _baseFilterLavAudio);
      if (_baseFilterLavAudio != null)
      {
        hasPostProcessing = true;
        return true;
      }

      return false;
    }

    public int CropVertical { get; set; }
    public int CropHorizontal { get; set; }

    public bool HasPostProcessing
    {
      get { return hasPostProcessing; }
    }

    public bool EnableResize { get; set; }
    public bool EnablePostProcess { get; set; }

    public int AudioDelayLav
    {
      get
      {
        int delay = 0;
        bool enable;
        ILAVAudioSettings asett = _baseFilterLavAudio as ILAVAudioSettings;
        if (asett == null) return delay;
        var hr = asett.GetAudioDelay(out enable, out delay);
        DsError.ThrowExceptionForHR(hr);
        return delay;
      }
      set
      {
        ILAVAudioSettings asett = _baseFilterLavAudio as ILAVAudioSettings;
        if (asett == null) return;
        var hr = asett.SetAudioDelay(true, value);
        DsError.ThrowExceptionForHR(hr);
      }
    }

    public int AudioDelay
    {
      get { return AudioDelayLav; }
      set { AudioDelayLav = value; }
    }

    public int AudioDelayInterval => audiodelayInterval;

    public void AudioDelayMinus()
    {
      ILAVAudioSettings asett = _baseFilterLavAudio as ILAVAudioSettings;
      if (asett != null)
      {
        bool enable;
        int delay;
        var hr = asett.GetAudioDelay(out enable, out delay);
        DsError.ThrowExceptionForHR(hr);
        hr = asett.SetAudioDelay(true, delay - AudioDelayInterval);
        DsError.ThrowExceptionForHR(hr);
      }
    }

    public void FreePostProcess()
    {
      if (_baseFilterLavAudio != null)
      {
        DirectShowUtil.CleanUpInterface(_baseFilterLavAudio);
        _baseFilterLavAudio = null;
      }
    }


    public void AudioDelayPlus()
    {
      ILAVAudioSettings asett = _baseFilterLavAudio as ILAVAudioSettings;
      if (asett != null)
      {
        bool enable;
        int delay;
        var hr = asett.GetAudioDelay(out enable, out delay);
        DsError.ThrowExceptionForHR(hr);
        hr = asett.SetAudioDelay(true, delay + AudioDelayInterval);
        DsError.ThrowExceptionForHR(hr);
      }
    }

    #endregion
  }
}