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
using System.Windows.Forms;
using System.Drawing;
using MediaPortal.GUI.Library;

namespace MediaPortal.Player
{
  public enum VideoStreamType
  {
    MPEG2=1,
    H264=2,
    MPEG4
  }
  public class VideoStreamFormat
  {
    public VideoStreamType streamType;
    public int width;
    public int height;
    public int arX;
    public int arY;
    public int bitrate;
    public bool isInterlaced;

    private bool _valid;

    public bool IsValid
    {
      get
      {
        return _valid;
      }
      set
      {
        _valid = value;
      }
    }

    public VideoStreamFormat()
    {
      IsValid=false;
    }
    public VideoStreamFormat(VideoStreamType streamType, int width, int height, int arX, int arY, int bitrate, bool isInterlaced)
    {
      IsValid = true;
      this.streamType = streamType;
      this.width = width;
      this.height = height;
      this.arX = arX;
      this.arY = arY;
      this.bitrate = bitrate;
      this.isInterlaced = isInterlaced;
    }
    public override string ToString()
    {
      return String.Format("streamtype={0} resolution={1}x{2} aspect ratio={3}:{4} bitrate={5} isInterlaced={6}",streamType.ToString(), width, height, arX,arY, bitrate, isInterlaced);
    }
  }

  /// <summary>
  /// This class holds the IPlayer interface which must be implemented by any interal player like
  /// - audio players
  /// - video players
  /// - tv timeshifting players
  /// </summary>
  public class IPlayer
  {
    protected Rectangle _videoRectangle = new Rectangle(0, 0, 0, 0);
    protected Rectangle _sourceRectangle = new Rectangle(0, 0, 0, 0);
    protected bool _isVisible = false;

    /// <summary>
    /// Default ctor
    /// </summary>
    public IPlayer()
    {
    }

    /// <summary>
    /// Method to handle any windows message
    /// MP will route any window message to the players so they can react on it
    /// by overriding this mehtod
    /// </summary>
    /// <param name="m">Message</param>
    public virtual void WndProc(ref Message m)
    {
    }

    /// <summary>
    /// This method is used to start playing a file
    /// </summary>
    /// <param name="strFile">file to play</param>
    /// <returns>
    /// true: file is playing
    /// false: unable to play file
    /// </returns>
    public virtual bool Play(string strFile)
    {
      return false;
    }

    /// <summary>
    /// Nearly the same as Play(), but usefull for web streams to give the real name of the stream and not url
    /// </summary>
    /// <param name="strFile">file to play</param>
    /// <param name="streamName">real name of the stream</param>
    /// <returns>
    /// true: file is playing
    /// false: unable to play file
    /// </returns>
    public virtual bool PlayStream(string strFile,string streamName)
    {
        return false;
    }

    public virtual bool SupportsReplay
    {
      get
      {
        return false;
      }
    }

    /// <summary>
    /// Property which indicate if we're playing an audio CD or not
    /// </summary>
    public virtual bool IsCDA
    {
      get
      {
        return false;
      }
    }

    /// <summary>
    /// Property which indicate if we're playing a DVD or not
    /// </summary>
    public virtual bool IsDVD
    {
      get
      {
        return false;
      }
    }

    /// <summary>
    /// Property which indicate if we're playing a DVD Menu or not
    /// </summary>
    public virtual bool IsDVDMenu
    {
      get
      {
        return false;
      }
    }

    /// <summary>
    /// Property which indicate if we're playing a TV recording or timeshifting file 
    /// </summary>
    public virtual bool IsTV
    {
      get
      {
        return false;
      }
    }

    /// <summary>
    /// Property which indicate if we're playing radio (streamed or local FM)
    /// </summary>
    public virtual bool IsRadio
    {
      get
      {
        return false;
      }
    }

    /// <summary>
    /// this method can be overriden if the player needs to respond to user actions
    /// like keypresses and mouse events
    /// </summary>
    public virtual bool OnAction(Action action)
    {
      return false;
    }

    /// <summary>
    /// Property to get/set the top left x-position of the video window
    /// </summary>
    public virtual int PositionX
    {
      get { return 0; }
      set { }
    }

    /// <summary>
    /// Property to get/set the top left y-position of the video window
    /// </summary>
    public virtual int PositionY
    {
      get { return 0; }
      set { }
    }

    /// <summary>
    /// Property to get/set the width of the video window
    /// </summary>
    public virtual int RenderWidth
    {
      get { return 0; }
      set { }
    }

    /// <summary>
    /// Property to get/set the height the video window
    /// </summary>
    public virtual int RenderHeight
    {
      get { return 0; }
      set { }
    }

    /// <summary>
    /// Property to show/hide the video window
    /// </summary>
    public virtual bool Visible
    {
      get { return _isVisible; }
      set
      {
        if (value == _isVisible) return;
        _isVisible = value;
      }
    }

    public virtual int Width
    {
      get { return 0; }
      set { }
    }

    public virtual int Height
    {
      get { return 0; }
      set { }
    }

    /// <summary>
    /// Property to put video window fullscreen or preview mode
    /// </summary>
    public virtual bool FullScreen
    {
      get { return GUIGraphicsContext.IsFullScreenVideo; }
      set { }
    }

    /// <summary>
    /// Property to set the playback speed (-32x,-16x,-8x,-4x,-2x,1x,2x,4x,8x,16x)
    /// </summary>
    public virtual int Speed
    {
      get { return 1; }
      set { }
    }

    /// <summary>
    /// Property to set the volume (0%-100%)
    /// </summary>
    public virtual int Volume
    {
      get { return 100; }
      set { }
    }

    /// <summary>
    /// Property to get the total duration of the currently playing file (in secs)
    /// </summary>
    public virtual double Duration
    {
      get { return 0; }
    }

    /// <summary>
    /// Property to get the current position in the currently playing file (in secs)
    /// </summary>
    public virtual double CurrentPosition
    {
      get { return 0; }
    }

    /// <summary>
    /// Property to get the stream position in the currently playing file (in secs)
    /// </summary>
    public virtual double StreamPosition
    {
      get { return 0; }
    }

    /// <summary>
    /// Property to get the content start in the currently playing file (in secs)
    /// </summary>
    public virtual double ContentStart
    {
      get { return 0; }
    }

    /// <summary>
    /// Method to pause or unpause
    /// </summary>
    public virtual void Pause()
    {
    }

    /// <summary>
    /// Method to stop playing
    /// </summary>
    public virtual void Stop()
    {
    }

    /// <summary>
    /// Method to stop playing
    /// </summary>
    public virtual void Stop(bool keepExclusiveModeOn)
    {
      Stop(); // currently only TsReaderPlayer uses this
    }

    /// <summary>
    /// Method to stop playing but at the same time keep timeshifting on server
    /// </summary>
    public virtual void StopAndKeepTimeShifting()
    {
    }

    /// <summary>
    /// Property which indicates if the playback is paused or not
    /// </summary>
    public virtual bool Paused
    {
      get { return false; }
    }

    /// <summary>
    /// Property which indicates if we're playing a file
    /// <remarks>This should return true even if current playback is paused</remarks>
    /// </summary>
    public virtual bool Playing
    {
      get { return false; }
    }

    /// <summary>
    /// Property which returns the PlaybackType (implemented in BASS)
    /// <remarks>0 = Normal, 1 = Gapless, 2 = Crossfade</remarks>
    /// </summary>
    public virtual int PlaybackType
    {
      get { return -1; }
    }

    /// <summary>
    /// Property which indicates if we stopped playing a file
    /// </summary>
    public virtual bool Stopped
    {
      get { return false; }
    }

    /// <summary>
    /// Property which indicates if the graph is initializing
    /// </summary>
    public virtual bool Initializing
    {
        get { return false; }
    }

    /// <summary>
    /// Property which returns the current filename
    /// </summary>
    public virtual string CurrentFile
    {
      get { return ""; }
    }

    /// <summary>
    /// Property to get/set the zoom/AR mode
    /// </summary>
    public virtual Geometry.Type ARType
    {
      get { return GUIGraphicsContext.ARType; }
      set
      {
      }
    }

    /// <summary>
    /// Method to seek to a specific point relative from the current position
    /// </summary>
    /// <param name="dTime">relative time in secs</param>
    public virtual void SeekRelative(double dTime)
    {
    }

    /// <summary>
    /// Method to seek to a specific point relative 
    /// </summary>
    /// <param name="dTime">absolute time in secs</param>
    public virtual void SeekAbsolute(double dTime)
    {
    }

    /// <summary>
    /// Method to seek to a specific point relative to the current position
    /// </summary>
    /// <param name="iPercentage">percentage (-100% to +100%) relative to the current position</param>
    public virtual void SeekRelativePercentage(int iPercentage)
    {
    }

    /// <summary>
    /// Method to seek to a specific point 
    /// </summary>
    /// <param name="iPercentage">percentage (0 to +100%) </param>
    public virtual void SeekAsolutePercentage(int iPercentage)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="resumeData">resumeData</param>
    public virtual bool GetResumeState(out byte[] resumeData)
    {
      resumeData = null;
      return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="resumeData">resumeData</param>
    public virtual bool SetResumeState(byte[] resumeData)
    {
      return false;
    }

    /// <summary>
    /// Property which should return true if the player wants to show a video window
    /// </summary>
    public virtual bool HasVideo
    {
      get { return false; }
    }

    /// <summary>
    /// This method will be called on a regular basis by MP
    /// it allows the external player to do some work
    /// </summary>
    public virtual void Process()
    {
    }

    /// <summary>
    /// Property which returns the total number of audio streams available
    /// </summary>
    public virtual int AudioStreams
    {
      get { return 1; }
    }

    /// <summary>
    /// Property to get/set the current audio stream
    /// </summary>
    public virtual int CurrentAudioStream
    {
      get { return 0; }
      set { }
    }

    /// <summary>
    /// Property to get the name for an audio stream
    /// </summary>
    public virtual string AudioLanguage(int iStream)
    {
      return Strings.Unknown;
    }

        /// <summary>
    /// Property to get the type of an audio stream
    /// </summary>
    public virtual string AudioType(int iStream)
    {
      return Strings.Unknown;
    }    

    /// <summary>
    /// Property to get the total number of subtitle streams
    /// </summary>
    public virtual int SubtitleStreams
    {
      get { return 0; }
    }

    /// <summary>
    /// Property to get/set the current subtitle stream
    /// </summary>
    public virtual int CurrentSubtitleStream
    {
      get { return 0; }
      set { }
    }

    /// <summary>
    /// Property to get/set the name for a subtitle stream
    /// </summary>
    public virtual string SubtitleLanguage(int iStream)
    {
      return Strings.Unknown;
    }

    /// <summary>
    /// Property to get chapters
    /// </summary>
    public virtual double[] Chapters
    {
      get { return null; }
    }

    /// <summary>
    /// Method which is called by MP if the player needs to update its video window
    /// because the coordinates have been changed
    /// </summary>
    public virtual void SetVideoWindow()
    {
    }

    /// <summary>
    /// Property to get/set the contrast
    /// </summary>
    public virtual int Contrast
    {
      get
      {
        return 0;
      }
      set
      {
      }
    }

    /// <summary>
    /// Property to get/set the brightness
    /// </summary>
    public virtual int Brightness
    {
      get
      {
        return 0;
      }
      set
      {
      }
    }

    /// <summary>
    /// Property to get/set the gamme
    /// </summary>
    public virtual int Gamma
    {
      get
      {
        return 0;
      }
      set
      {
      }
    }

    /// <summary>
    /// Property which returns a rectangle for the video window
    /// </summary>
    public Rectangle VideoWindow
    {
      get { return _videoRectangle; }
    }
    public Rectangle SourceWindow
    {
      get { return _sourceRectangle; }
    }

    /// <summary>
    /// Property to enable/disable subtitles
    /// </summary>
    public virtual bool EnableSubtitle
    {
      get
      {
        return true;
      }
      set
      {
      }
    }

    public virtual int GetHDC()
    {
      return 0;
    }
    public virtual void ReleaseHDC(int HDC)
    {
    }

    /// <summary>
    /// Property which indicates if we can seek in the file
    /// </summary> 
    public virtual bool CanSeek()
    {
      return true;
    }

    /// <summary>
    /// Property which indicates if the file has ended (reached the end)
    /// </summary>
    public virtual bool Ended
    {
      get { return false; }
    }

    /// <summary>
    /// Property which indicates if the file is a tv timeshifting file or not
    /// </summary>
    public virtual bool IsTimeShifting
    {
      get { return false; }
    }
    public virtual void ContinueGraph()
    {
    }
    public virtual void PauseGraph()
    {
    }
    public virtual bool IsExternal
    {
      get
      {
        return false;
      }
    }

    public virtual VideoStreamFormat GetVideoFormat()
    {
      return new VideoStreamFormat();
    }

    public virtual eAudioDualMonoMode GetAudioDualMonoMode()
    {
      return eAudioDualMonoMode.UNSUPPORTED;
    }
    public virtual bool SetAudioDualMonoMode(eAudioDualMonoMode mode)
    {
      return false;
    }


    #region IDisposable Members
    public virtual void Release()
    {
      // TODO:  Add IPlayer.Dispose implementation
    }
    #endregion
  }
}