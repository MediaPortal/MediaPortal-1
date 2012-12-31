#region Usings

using System;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;

#endregion

namespace MediaPortal.Plugins.Process
{
  /// <summary>
  /// Wrapper class for the master volume handling (dependent on OS, configuration and MP version) 
  /// </summary>
  public class MasterVolume : IDisposable
  {
    #region Variables

    private MasterVolumeHelper _masterVolumeHelper = null;

    #endregion

    #region Public methods

    public MasterVolume()
    {
      if (Environment.OSVersion.Version.Major >= 6)
      {
        using (Settings reader = new MPSettings())
        {
          // If "Upmix stereo to 5.1/7.1" is enabled or MP volume control is set to "Wave" (MP 1.3 only),
          // the MP volume handler will control the wave volume. Thus we use our own master volume helper. 
          if (reader.GetValueAsBool("volume", "digital", false))
          {
            Log.Debug("PS: Use the PowerScheduler master volume helper, since MP's volume handler is set to control the wave volume");
            try
            {
              _masterVolumeHelper = new MasterVolumeHelper();
              Log.Debug("PS: Default audio device is \"{0}\" (state: {1})", _masterVolumeHelper.FriendlyName, _masterVolumeHelper.State);
            }
            catch (Exception ex)
            {
              Log.Error("PS: Exception in creating MasterVolumehelper: {0}", ex);
            }
            return;
          }
        }
        try
        {
          // Since MP 1.3.x MP can control the master volume on its own ("MediaPortal.Mixer.AEDev" class exists)
          Type.GetType("MediaPortal.Mixer.AEDev");
          Log.Debug("PS: Use MP's volume handler, since it controls the master volume");
        }
        catch (TypeLoadException)
        {
          // "MediaPortal.Mixer.AEDev" class does not exist, so use own master volume helper
          Log.Debug("PS:Use the PowerScheduler master volume helper, since MP's volume handler cannot control the master volume");
          try
          {
            _masterVolumeHelper = new MasterVolumeHelper();
            Log.Debug("PS: Default audio device is \"{0}\" (state: {1})", _masterVolumeHelper.FriendlyName, _masterVolumeHelper.State);
          }
          catch (Exception ex)
          {
            Log.Error("PS: Exception in creating MasterVolumehelper: {0}", ex);
          }
        }
        return;
      }
      Log.Debug("PS: Use MP's volume handler, since we are running Windows XP");
    }

    public void Dispose()
    {
      if (_masterVolumeHelper != null)
      {
        _masterVolumeHelper.Dispose();
        _masterVolumeHelper = null;
      }
    }

    #endregion

    #region Public properties

    public virtual bool IsMuted
    {
      get
      {
        try
        {
          if (_masterVolumeHelper != null)
          {
            return _masterVolumeHelper.Muted;
          }
          else
          {
            return VolumeHandler.Instance.IsMuted;
          }
        }
        catch (Exception ex)
        {
          Log.Error("PS: Exception in IsMuted (get): {0}", ex);
        }
        return false;
      }
      set
      {
        try
        {
          if (_masterVolumeHelper != null)
          {
            _masterVolumeHelper.Muted = value;
          }
          else
          {
            VolumeHandler.Instance.IsMuted = value;
          }
        }
        catch (Exception ex)
        {
          Log.Error("PS: Exception in IsMuted (set): {0}", ex);
        }
      }
    }

    #endregion Properties
  }
}