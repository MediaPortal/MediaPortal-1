using System;
using System.Windows.Forms;
using System.Drawing;




using MediaPortal.GUI.Library;


namespace MediaPortal.Player
{
	/// <summary>
	/// 
	/// </summary>
	public class IPlayer 
	{
    protected Rectangle m_VideoRect = new Rectangle(0,0,0,0);
    protected Rectangle m_SourceRect = new Rectangle(0,0,0,0);
    protected IRender                   m_renderFrame=null;
    public IPlayer()
		{
		}

    public virtual IRender RenderFrame
    {
      get { return m_renderFrame;}
      set { m_renderFrame=value;}
    }

    public virtual void WndProc( ref Message m )
    {
    }

    public virtual bool Play(string strFile)
    {
      return false;
    }
    public virtual bool IsDVD
    {
      get 
      {
        return false;
      }
    }
    public virtual bool IsTV
    {
      get 
      {
        return false;
      }
    }
    public virtual bool IsRadio
    {
      get 
      {
        return false;
      }
    }
    public virtual bool OnAction(Action action)
    {
      return false;
    }
    public virtual int PositionX
    {
      get {return 0;}
      set{}
    }
    public virtual int PositionY
    {
      get {return 0;}
      set{}
    }
    
    public virtual int RenderWidth
    {
      get {return 0;}
      set{}
    }
    public virtual int RenderHeight
    {
      get {return 0;}
      set{}
    }
    public virtual int Width
    {
      get {return 0;}
      set{}
    }
    public virtual int Height
    {
      get {return 0;}
      set{}
    }

    public virtual bool FullScreen
    {
      get {return GUIGraphicsContext.IsFullScreenVideo;}
      set{}
    }
    public virtual int Speed
    {
      get {return 1;}
      set{}
    }

    public virtual int Volume
    {
      get {return 100;}
      set{}
    }

    public virtual double Duration
    {
      get {return 0;}
    }

    public virtual double CurrentPosition
    {
      get {return 0;}
    }
    public virtual double ContentStart
    {
      get {return 0;}
    }

    public virtual void Pause()
    {
    }
    public virtual void Stop()
    {
    }

    public virtual bool Paused
    {
      get { return false;}
    }

    public virtual bool Playing
    {
      get { return false;}
    }

    public virtual bool Stopped
    {
      get { return false;}
    }
    public virtual string CurrentFile
    {
      get { return "";}
    }
    public virtual Geometry.Type ARType
    {
      get { return GUIGraphicsContext.ARType;}
      set 
      {
      }
    }

    public virtual void SeekRelative(double dTime)
    {
    }
    public virtual void SeekAbsolute(double dTime)
    {
    }
    public virtual void SeekRelativePercentage(int iPercentage)
    {
    }
    public virtual void SeekAsolutePercentage(int iPercentage)
    {
    }
    public virtual bool HasVideo
    {
      get { return false;}
    }
    public virtual void Process()
    {
    }
    public virtual int AudioStreams
    {
      get { return 1;}
    }
    public virtual int CurrentAudioStream
    {
      get { return 0;}
      set {}
    }
    public virtual string AudioLanguage(int iStream)
    {
      return "Unknown";
    }

    public virtual int SubtitleStreams
    {
      get { return 0;}
    }
    public virtual int CurrentSubtitleStream
    {
      get { return 0;}
      set {}
    }
    public virtual string SubtitleLanguage(int iStream)
    {
      return "Unknown";
    }

    public virtual void SetVideoWindow()
    {
    }

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

    public Rectangle VideoWindow
    {
      get { return m_VideoRect;}
    }
    public Rectangle SourceWindow
    {
      get { return m_SourceRect;}
    }

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

    public virtual bool CanSeek()
    {
      return true;
    }

    public virtual bool DoesOwnRendering
    {
      get { return false;}
    }
    public virtual bool Ended
    {
      get { return false;}
    }

    #region IDisposable Members

    public virtual void Release()
    {
      // TODO:  Add IPlayer.Dispose implementation
    }

    #endregion
  }
}
