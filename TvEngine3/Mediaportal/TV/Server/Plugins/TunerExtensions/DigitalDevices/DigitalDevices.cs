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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using DirectShowLib;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.Plugins.Base.Interfaces;
using Mediaportal.TV.Server.Plugins.TunerExtension.DigitalDevices.Config;
using Mediaportal.TV.Server.Plugins.TunerExtension.DigitalDevices.Service;
using Mediaportal.TV.Server.SetupControls;
using Mediaportal.TV.Server.TVControl.Interfaces.Services;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Dvb.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Mpeg2Ts;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension.Enum;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.DigitalDevices
{
  /// <summary>
  /// A class for handling conditional access, DiSEqC and stream selection for
  /// Digital Devices tuners (and clones from Mystique).
  /// </summary>
  public class DigitalDevices : BaseTunerExtension, IConditionalAccessMenuActions, IConditionalAccessProvider, IDirectShowAddOnDevice, IDiseqcDevice, IDisposable, IStreamSelector, ITvServerPlugin, ITvServerPluginCommunication
  {
    #region enums

    private enum BdaExtensionProperty
    {
      SignalType = 0,
      PlpIds
    }

    private enum DecryptChainingRestriction : uint
    {
      None = 0,
      NoBackwardChaining = 0x40000000,
      NoForwardChaining = 0x80000000
    }

    [Flags]
    private enum PlpInfoFlag : uint
    {
      FutureExtensionFramesExist = 1,
      IsDvbT2LiteProfile = 2
    }

    #endregion

    private class SharedCiContext
    {
      #region information

      public string DevicePath;

      #endregion

      #region live variables

      // An identifier for the  menu/message most recently received from the
      // CAM.
      public int CamMenuId = -1;

      // The external ID of the tuner that "owns" this CI. The owner is the
      // tuner that most recently interacted with the CI - either through
      // loading, or sending decrypt or enter menu requests. We send messages
      // from the CAM to the owner. In the case where the CI is linked to
      // multiple tuners, this avoids CAM and user confusion.
      private string _ownerExternalId = null;
      private int _ownerIndex = -1;
      private int _previousOwnerIndex = -1;

      private CiSlot _slot = null;

      // Programs decrypted using MTD. Tuner device path => program number.
      public IDictionary<string, uint> MtdPrograms = new Dictionary<string, uint>(4);

      // Programs decrypted using traditional CA PMT. Program number => PMT.
      public IDictionary<uint, TableProgramMap> McdPrograms = new Dictionary<uint, TableProgramMap>(4);

      #endregion

      #region hardware/driver/API state

      public bool IsCamReady = false;                         // True if the CAM root menu title can be successfully retrieved.
      public string CamMenuTitle = string.Empty;              // The CAM's root menu title.
      public IList<ushort> CamCasIds = new List<ushort>(20);  // The CA system IDs that the CAM claims to support.
      public int CiBitRate = -1;
      public int CiMaxBitRate = -1;
      public int CiTunerCount = 0;                            // The number of tuners linked to the CI slot.

      #endregion

      #region configuration

      private CiSlotConfig _config = null;

      #endregion

      public SharedCiContext(string devicePath, string deviceName)
      {
        DevicePath = devicePath;
        _config = new CiSlotConfig(devicePath, deviceName);
      }

      public string OwnerExternalId
      {
        get
        {
          return _ownerExternalId;
        }
      }

      public int OwnerIndex
      {
        get
        {
          return _ownerIndex;
        }
      }

      public CiSlot Slot
      {
        get
        {
          return _slot;
        }
      }

      public int DecryptLimit
      {
        get
        {
          return _config.DecryptLimit;
        }
      }

      public HashSet<string> Providers
      {
        get
        {
          return _config.Providers;
        }
      }

      public void SetOwner(string externalId, int index, CiSlot slot, bool expectedAlreadyOwner = false)
      {
        if (string.IsNullOrEmpty(externalId))
        {
          this.LogDebug("Digital Devices: tuner {0} releasing ownership of CI {1}", _ownerIndex, _slot.Index);
        }
        else if (string.IsNullOrEmpty(_ownerExternalId))
        {
          this.LogDebug("Digital Devices: tuner {0} taking ownership of unmanaged CI {1}", index, slot.Index);
        }
        else if (!string.Equals(_ownerExternalId, externalId))
        {
          if (expectedAlreadyOwner)
          {
            this.LogWarn("Digital Devices: tuner {0} reclaiming ownership of CI {1} currently owned by tuner {2}", index, slot.Index, _ownerIndex);
          }
          else
          {
            this.LogDebug("Digital Devices: tuner {0} taking ownership of CI {1} currently owned by tuner {2}", index, slot.Index, _ownerIndex);
          }
        }

        if (index >= 0)
        {
          _previousOwnerIndex = _ownerIndex;
        }
        _ownerExternalId = externalId;
        _ownerIndex = index;
        _slot = slot;
      }

      public bool UpdateStateInfo()
      {
        bool isChanged = false;
        int hr;

        bool isCamReady;
        string camMenuTitle;
        IList<ushort> camCasIds;
        int ciBitRate;
        int ciMaxBitRate;
        int ciTunerCount;

        isCamReady = (Slot.GetCamMenuTitle(out camMenuTitle) == (int)NativeMethods.HResult.S_OK);
        if (!isCamReady)
        {
          camCasIds = new List<ushort>(0);
        }
        else
        {
          hr = Slot.GetCamCaSystemIds(out camCasIds);
          if (hr != (int)NativeMethods.HResult.S_OK)
          {
            this.LogWarn("Digital Devices: failed to read CAM CA system IDs, hr = 0x{0:x}", hr);
          }
        }

        hr = Slot.GetCiBitRate(out ciBitRate);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogWarn("Digital Devices: failed to read CI bit rate, hr = 0x{0:x}", hr);
        }

        hr = Slot.GetCiMaxBitRate(out ciMaxBitRate);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogWarn("Digital Devices: failed to read maximum CI bit rate, hr = 0x{0:x}", hr);
        }

        hr = Slot.GetCiTunerCount(out ciTunerCount);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogWarn("Digital Devices: failed to read CI tuner count, hr = 0x{0:x}", hr);
        }

        if (isCamReady != IsCamReady ||
          !string.Equals(camMenuTitle, CamMenuTitle) ||
          camCasIds.Count != CamCasIds.Count ||
          ciBitRate != CiBitRate ||
          ciMaxBitRate != CiMaxBitRate ||
          ciTunerCount != CiTunerCount)
        {
          isChanged = true;
        }
        else
        {
          foreach (ushort casId in camCasIds)
          {
            if (!CamCasIds.Contains(casId))
            {
              isChanged = true;
              break;
            }
          }
        }

        IsCamReady = isCamReady;
        CamMenuTitle = camMenuTitle;
        CamCasIds = camCasIds;
        CiBitRate = ciBitRate;
        CiMaxBitRate = ciMaxBitRate;
        CiTunerCount = ciTunerCount;
        return isChanged;
      }

      public bool UpdateConfig()
      {
        bool isChanged = false;
        int decryptLimit = _config.DecryptLimit;
        HashSet<string> providers = new HashSet<string>(_config.Providers);

        if (_isPluginEnabled)
        {
          _config.LoadSettings();
        }
        else
        {
          _config.ResetSettings();
        }

        if (decryptLimit != _config.DecryptLimit)
        {
          isChanged = true;
        }
        if (providers.Count != _config.Providers.Count)
        {
          isChanged = true;
        }
        else
        {
          foreach (string provider in _config.Providers)
          {
            if (!providers.Contains(provider))
            {
              isChanged = true;
              break;
            }
          }
        }
        return isChanged;
      }
    }

    private class PrivateCiContext
    {
      public DsDevice Device;
      public IBaseFilter Filter;
      public CiSlot Slot;

      public PrivateCiContext(DsDevice device, IBaseFilter filter, int index)
      {
        Device = device;
        Filter = filter;
        Slot = new CiSlot(index, filter);
      }
    }

    #region structs

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct PlpInfo      // DD_T2_PLPIDS
    {
      public int Length;
      public int Version;
      public int PlpId;
      public int CommonPlpId;   // 0xFFFFFFFF = no common PLP
      public PlpInfoFlag Flags;
      private int Reserved;
      public int PlpCount;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
      public byte[] PlpIdList;
    }

    #endregion

    #region constants

    private static readonly Guid BDA_EXTENSION_PROPERTY_SET = new Guid(0x0aa8a603, 0xa240, 0x11de, 0xb1, 0x30, 0x00, 0x00, 0x00, 0x00, 0x4d, 0x56);

    private static readonly int PLP_INFO_SIZE = Marshal.SizeOf(typeof(PlpInfo));  // 284

    private const int MMI_HANDLER_THREAD_WAIT_TIME = 500;   // unit = ms

    #endregion

    #region variables

    private static bool _isPluginEnabled = false;
    private static IDictionary<string, SharedCiContext> _sharedCiContexts = new Dictionary<string, SharedCiContext>(4); // One entry per CI/CAM attached to this PC. CI slot device path => context.
    private static object _sharedCiContextsLock = new object();
    private static DigitalDevicesConfigService _service = new DigitalDevicesConfigService();

    private bool _isDigitalDevices = false;
    private bool _isCaInterfaceOpen = false;
    private int _tunerIndex = -1;
    private string _tunerExternalId = string.Empty;
    private bool _isCiSlotPresent = false;

    // For CI/CAM interaction.
    private IDictionary<string, PrivateCiContext> _privateCiContexts = null;  // One entry per CI/CAM linked to this tuner. CI slot device path => context.
    private HashSet<string> _ciSlotsWithChangedServices = null;
    private IFilterGraph2 _graph = null;
    private string _menuContext = null;                                       // The device path of the CI slot which most recently sent or received a menu/message.
    private int _menuSlotIndex = -1;                                          // The CI slot index corresponding with _menuContext.
    private IList<string> _rootMenuChoices = null;                            // The device paths of each of the CI slots which have an entry in the root menu.

    private Thread _mmiHandlerThread = null;
    private AutoResetEvent _mmiHandlerThreadStopEvent = null;
    private object _mmiLock = new object();
    private IConditionalAccessMenuCallBack _caMenuCallBack = null;
    private object _caMenuCallBackLock = new object();

    // For DiSEqC support.
    private IDiseqcDevice _diseqcInterface = null;

    // For PLP selection.
    private IKsPropertySet _propertySet = null;
    private IntPtr _plpBuffer = IntPtr.Zero;

    #endregion

    #region MMI handler thread

    /// <summary>
    /// Start a thread to receive MMI messages from the CAM(s).
    /// </summary>
    private void StartMmiHandlerThread()
    {
      // Don't start a thread if the interface has not been opened.
      if (!_isCaInterfaceOpen)
      {
        return;
      }

      lock (_mmiLock)
      {
        // Kill the existing thread if it is in "zombie" state.
        if (_mmiHandlerThread != null && !_mmiHandlerThread.IsAlive)
        {
          StopMmiHandlerThread();
        }

        if (_mmiHandlerThread == null)
        {
          this.LogDebug("Digital Devices: starting new MMI handler thread");
          _mmiHandlerThreadStopEvent = new AutoResetEvent(false);
          _mmiHandlerThread = new Thread(new ThreadStart(MmiHandler));
          _mmiHandlerThread.Name = "Digital Devices MMI handler";
          _mmiHandlerThread.IsBackground = true;
          _mmiHandlerThread.Priority = ThreadPriority.Lowest;
          _mmiHandlerThread.Start();
        }
      }
    }

    /// <summary>
    /// Stop the thread that receives MMI messages from the CAM(s).
    /// </summary>
    private void StopMmiHandlerThread()
    {
      lock (_mmiLock)
      {
        if (_mmiHandlerThread != null)
        {
          if (!_mmiHandlerThread.IsAlive)
          {
            this.LogWarn("Digital Devices: aborting old MMI handler thread");
            _mmiHandlerThread.Abort();
          }
          else
          {
            _mmiHandlerThreadStopEvent.Set();
            if (!_mmiHandlerThread.Join(MMI_HANDLER_THREAD_WAIT_TIME * 2))
            {
              this.LogWarn("Digital Devices: failed to join MMI handler thread, aborting thread");
              _mmiHandlerThread.Abort();
            }
          }
          _mmiHandlerThread = null;
          if (_mmiHandlerThreadStopEvent != null)
          {
            _mmiHandlerThreadStopEvent.Close();
            _mmiHandlerThreadStopEvent = null;
          }
        }
      }
    }

    /// <summary>
    /// Thread function for receiving MMI messages from the CAM(s).
    /// </summary>
    private void MmiHandler()
    {
      this.LogDebug("Digital Devices: MMI handler thread start polling");
      try
      {
        while (!_mmiHandlerThreadStopEvent.WaitOne(MMI_HANDLER_THREAD_WAIT_TIME))
        {
          lock (_sharedCiContextsLock)
          {
            foreach (SharedCiContext sharedContext in _sharedCiContexts.Values)
            {
              if (string.IsNullOrEmpty(sharedContext.OwnerExternalId))
              {
                PrivateCiContext privateContext;
                if (_privateCiContexts.TryGetValue(sharedContext.DevicePath, out privateContext))
                {
                  sharedContext.SetOwner(_tunerExternalId, _tunerIndex, privateContext.Slot);
                }
                else
                {
                  continue;
                }
              }
              else if (!sharedContext.OwnerExternalId.Equals(_tunerExternalId))
              {
                continue;
              }

              if (sharedContext.UpdateConfig())
              {
                // Config has changed.
                this.LogInfo("Digital Devices: slot {0} config change", sharedContext.Slot.Index);
                this.LogDebug("  decrypt limit   = {0}", sharedContext.DecryptLimit);
                this.LogDebug("  providers       = {0}", string.Join(", ", sharedContext.Providers));
              }

              if (sharedContext.UpdateStateInfo())
              {
                // State has changed.
                this.LogInfo("Digital Devices: slot {0} state change", sharedContext.Slot.Index);
                this.LogInfo("  is CAM ready    = {0}", sharedContext.IsCamReady);
                if (sharedContext.IsCamReady)
                {
                  this.LogDebug("  CAM title       = {0}", sharedContext.CamMenuTitle);
                  this.LogDebug("  # CAS IDs       = {0}", sharedContext.CamCasIds.Count);
                  for (int i = 0; i < sharedContext.CamCasIds.Count; i++)
                  {
                    this.LogDebug("    {0, -13} = 0x{1:x4}", i + 1, sharedContext.CamCasIds[i]);
                  }
                }
                this.LogDebug("  CI bit rate     = {0} b/s", sharedContext.CiBitRate);
                this.LogDebug("  CI max bit rate = {0} b/s", sharedContext.CiMaxBitRate);
                this.LogDebug("  CI tuner count  = {0}", sharedContext.CiTunerCount);
              }

              if (!sharedContext.IsCamReady)
              {
                sharedContext.McdPrograms.Clear();
                sharedContext.MtdPrograms.Clear();
                continue;
              }

              int id;
              CiSlot.MenuType type;
              IList<string> strings;
              int answerLength;
              int hr = sharedContext.Slot.GetCamMenu(out id, out type, out strings, out answerLength);
              if (hr == (int)NativeMethods.HResult.S_OK)
              {
                // Is this a menu that we haven't seen before?
                if (sharedContext.CamMenuId == id)
                {
                  continue;
                }
                _menuContext = sharedContext.DevicePath;
                _menuSlotIndex = sharedContext.Slot.Index;
                if (sharedContext.CamMenuId == -1)
                {
                  // The first menu is provided to enable us sync with the driver's current ID.
                  sharedContext.CamMenuId = id;
                  continue;
                }
                sharedContext.CamMenuId = id;
                if (type == CiSlot.MenuType.Unknown)
                {
                  this.LogError("Digital Devices: received unknown/unsupported menu type");
                  continue;
                }

                this.LogInfo("Digital Devices: slot {0} received new menu", sharedContext.Slot.Index);
                this.LogDebug("  id        = {0}", id);
                this.LogDebug("  type      = {0}", type);

                lock (_caMenuCallBackLock)
                {
                  if (_caMenuCallBack == null)
                  {
                    this.LogDebug("Digital Devices: menu call back not set");
                  }

                  if (type == CiSlot.MenuType.Menu || type == CiSlot.MenuType.List)
                  {
                    this.LogDebug("  title     = {0}", strings[0]);
                    this.LogDebug("  sub-title = {0}", strings[1]);
                    this.LogDebug("  footer    = {0}", strings[2]);
                    this.LogDebug("  # entries = {0}", strings.Count - 3);

                    if (_caMenuCallBack != null)
                    {
                      _caMenuCallBack.OnCiMenu(strings[0], strings[1], strings[2], strings.Count - 3);
                    }

                    for (int i = 3; i < strings.Count; i++)
                    {
                      string entry = strings[i];
                      this.LogDebug("    {0, -7} = {1}", i - 2, entry);
                      if (_caMenuCallBack != null)
                      {
                        _caMenuCallBack.OnCiMenuChoice(i - 3, entry);
                      }
                    }
                  }
                  else if (type == CiSlot.MenuType.Enquiry)
                  {
                    this.LogDebug("  prompt    = {0}", strings[0]);
                    this.LogDebug("  length    = {0}", answerLength);
                    if (_caMenuCallBack != null)
                    {
                      _caMenuCallBack.OnCiRequest(false, (uint)answerLength, strings[0]);
                    }
                  }
                }
              }
              else if (hr != unchecked((int)0x8007001f))
              {
                // Attempting to check for a menu when the menu has not previously been
                // opened seems to fail (HRESULT 0x8007001f). Don't flood the logs...
                this.LogError("Digital Devices: failed to read MMI, hr = 0x{0:x}", hr);
              }
            }
          }
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        this.LogError(ex, "Digital Devices: MMI handler thread exception");
        return;
      }
      this.LogDebug("Digital Devices: MMI handler thread stop polling");
    }

    #endregion

    #region ITunerExtension members

    /// <summary>
    /// A human-readable name for the extension.
    /// </summary>
    public override string Name
    {
      get
      {
        return "Digital Devices";
      }
    }

    /// <summary>
    /// Attempt to initialise the interfaces used by the extension.
    /// </summary>
    /// <param name="tunerExternalId">The external identifier for the tuner.</param>
    /// <param name="tunerSupportedBroadcastStandards">The broadcast standards supported by the tuner (eg. DVB-T, DVB-T2... etc.).</param>
    /// <param name="context">Context required to initialise the interfaces.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public override bool Initialise(string tunerExternalId, BroadcastStandard tunerSupportedBroadcastStandards, object context)
    {
      this.LogDebug("Digital Devices: initialising");

      if (_isDigitalDevices)
      {
        this.LogWarn("Digital Devices: extension already initialised");
        return true;
      }

      // Digital Devices components have a common section in their device path.
      if (string.IsNullOrEmpty(tunerExternalId))
      {
        this.LogDebug("Digital Devices: tuner external identifier is not set");
        return false;
      }
      if (!DigitalDevicesHardware.IsDevice(tunerExternalId, out _tunerIndex))
      {
        this.LogDebug("Digital Devices: incompatible tuner");
        return false;
      }

      this.LogInfo("Digital Devices: extension supported");
      _isDigitalDevices = true;
      _tunerExternalId = tunerExternalId;

      // Check if DiSEqC is supported (only relevant for satellite tuners).
      if ((tunerSupportedBroadcastStandards & BroadcastStandard.MaskSatellite) != 0)
      {
        // The driver supports the standard IBDA_DiseqCommand interface,
        // however there are a few subtle but critical implementation quirks:
        // - LnbSource, Repeats and UseToneBurst are not supported
        // - DiSEqC commands should be *disabled* (yes, you read that
        //   correctly) in order for commands to be sent successfully
        _diseqcInterface = new MicrosoftBdaDiseqc.MicrosoftBdaDiseqc(true, false, false, 0, false, true);
        if (!_diseqcInterface.Initialise(tunerExternalId, tunerSupportedBroadcastStandards, context))
        {
          this.LogWarn("Digital Devices: failed to initialise DiSEqC interface");
          IDisposable d = _diseqcInterface as IDisposable;
          if (d != null)
          {
            d.Dispose();
          }
          _diseqcInterface = null;
        }
      }

      // Check for DVB-X2 PLP selection support.
      if ((tunerSupportedBroadcastStandards & BroadcastStandard.MaskDvb2) != 0)
      {
        _propertySet = context as IKsPropertySet;
        if (_propertySet == null)
        {
          this.LogDebug("Digital Devices: stream selection not supported, context is not a property set");
        }
        else
        {
          KSPropertySupport support;
          int hr = _propertySet.QuerySupported(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.PlpIds, out support);
          if (hr == (int)NativeMethods.HResult.S_OK && support.HasFlag(KSPropertySupport.Get) && support.HasFlag(KSPropertySupport.Set))
          {
            this.LogDebug("Digital Devices: stream selection supported");
            _plpBuffer = Marshal.AllocCoTaskMem(PLP_INFO_SIZE);
          }
          else
          {
            this.LogDebug("Digital Devices: stream selection property not supported, hr = 0x{0:x}, support = {1}", hr, support);
            _propertySet = null;
          }
        }
      }

      return true;
    }

    #region tuner state change call backs

    /// <summary>
    /// This call back is invoked before a tune request is assembled.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is currently tuned to..</param>
    /// <param name="channel">The channel that the tuner will been tuned to.</param>
    /// <param name="action">The action to take, if any.</param>
    public override void OnBeforeTune(ITuner tuner, IChannel currentChannel, ref IChannel channel, out TunerAction action)
    {
      this.LogDebug("Digital Devices: on before tune call back");
      action = TunerAction.Default;

      if (!_isDigitalDevices)
      {
        this.LogWarn("Digital Devices: not initialised or interface not supported");
        return;
      }

      IChannelSatellite satelliteChannel = channel as IChannelSatellite;
      if (satelliteChannel == null)
      {
        return;
      }

      // Required by Digital Devices Max S8. Cine and DuoFlex S2 tuners
      // auto-detect modulation.
      // Refer to http://forum.team-mediaportal.com/threads/dd-max-s8-tuning-problems.129173/page-3
      if (satelliteChannel is ChannelDvbS2)
      {
        if (Environment.OSVersion.Version.Major >= 6) // Vista or later
        {
          if (satelliteChannel.ModulationScheme == ModulationSchemePsk.Psk4)
          {
            satelliteChannel.ModulationScheme = (ModulationSchemePsk)DirectShowLib.BDA.ModulationType.ModNbcQpsk;
          }
          else if (satelliteChannel.ModulationScheme == ModulationSchemePsk.Psk8)
          {
            satelliteChannel.ModulationScheme = (ModulationSchemePsk)DirectShowLib.BDA.ModulationType.ModNbc8Psk;
          }
        }
        else
        {
          // XP: use DD Control Center setting, which must be set to DVB-S2.
          satelliteChannel.ModulationScheme = (ModulationSchemePsk)DirectShowLib.BDA.ModulationType.ModNotSet;
        }
      }
      else if (satelliteChannel.ModulationScheme == ModulationSchemePsk.Psk4)
      {
        satelliteChannel.ModulationScheme = (ModulationSchemePsk)DirectShowLib.BDA.ModulationType.ModQpsk;
      }
      this.LogDebug("  modulation = {0}", satelliteChannel.ModulationScheme);
    }

    /// <summary>
    /// This call back is invoked after a tune request is submitted, when the
    /// tuner is started but before signal lock is checked.
    /// </summary>
    /// <param name="tuner">The tuner instance that this extension instance is associated with.</param>
    /// <param name="currentChannel">The channel that the tuner is tuned to.</param>
    public override void OnStarted(ITuner tuner, IChannel currentChannel)
    {
      // Ensure the MMI handler thread is always running when the graph is running.
      StartMmiHandlerThread();
    }

    #endregion

    #endregion

    #region IDirectShowAddOnDevice member

    /// <summary>
    /// Insert and connect additional filter(s) into the graph.
    /// </summary>
    /// <param name="graph">The tuner filter graph.</param>
    /// <param name="lastFilter">The source filter (usually either a capture/receiver or
    ///   multiplexer filter) to connect the [first] additional filter to.</param>
    /// <returns><c>true</c> if one or more additional filters were successfully added to the graph, otherwise <c>false</c></returns>
    public bool AddToGraph(IFilterGraph2 graph, ref IBaseFilter lastFilter)
    {
      this.LogDebug("Digital Devices: add to graph");

      if (!_isDigitalDevices)
      {
        this.LogWarn("Digital Devices: not initialised or interface not supported");
        return false;
      }
      if (graph == null)
      {
        this.LogError("Digital Devices: failed to add filter(s) to graph, graph is null");
        return false;
      }
      if (lastFilter == null)
      {
        this.LogError("Digital Devices: failed to add filter(s) to graph, last filter is null");
        return false;
      }
      if (_privateCiContexts != null && _privateCiContexts.Count > 0)
      {
        this.LogWarn("Digital Devices: {0} CI filter(s) already in graph", _privateCiContexts.Count);
        return true;
      }

      // This will be our list of CI contexts.
      _privateCiContexts = new Dictionary<string, PrivateCiContext>(4);
      _isCiSlotPresent = false;
      _graph = graph;
      Guid filterIid = typeof(IBaseFilter).GUID;
      while (true)
      {
        // Stage 1: if the last output pin doesn't have any mediums then no [further] CI slots
        // are configured for this tuner.
        IPin lastFilterOutputPin = DsFindPin.ByDirection(lastFilter, PinDirection.Output, 0);
        if (lastFilterOutputPin == null)
        {
          this.LogError("Digital Devices: failed to add filter(s) to graph, upstream filter doesn't have an output pin");
          return false;
        }
        try
        {
          ICollection<RegPinMedium> mediums = GetPinMediums(lastFilterOutputPin);
          if (mediums.Count == 0)
          {
            this.LogDebug("Digital Devices: no [more] CI filters available or configured for this tuner");
            break;
          }

          // Stage 2: see if there are any more CI filters that we can add to the graph. We re-loop
          // over all capture devices because the CI filters have to be connected in a specific
          // order which is not guaranteed to be the same as the capture device array order.
          bool addedFilter = false;
          DsDevice[] captureDevices = DsDevice.GetDevicesOfCat(FilterCategory.BDAReceiverComponentsCategory);
          foreach (DsDevice captureDevice in captureDevices)
          {
            // We're looking for a Digital Devices common interface device.
            int ciIndex;
            if (!DigitalDevicesHardware.IsCiDevice(captureDevice, out ciIndex))
            {
              captureDevice.Dispose();
              continue;
            }

            // Stage 3: okay, we've got a CI device. Create the filter.
            object obj;
            try
            {
              captureDevice.Mon.BindToObject(null, null, ref filterIid, out obj);
            }
            catch (Exception ex)
            {
              this.LogError(ex, "Digital Devices: failed to create CI filter, name = {0}, device path = {1}", captureDevice.Name, captureDevice.DevicePath);
              captureDevice.Dispose();
              continue;
            }

            // Stage 4: will this filter connect?
            int hr = (int)NativeMethods.HResult.E_FAIL;
            IBaseFilter tmpCiFilter = obj as IBaseFilter;
            IPin tmpFilterInputPin = DsFindPin.ByDirection(tmpCiFilter, PinDirection.Input, 0);
            try
            {
              ICollection<RegPinMedium> tmpMediums = GetPinMediums(tmpFilterInputPin);
              foreach (RegPinMedium m1 in mediums)
              {
                foreach (RegPinMedium m2 in tmpMediums)
                {
                  if (m1.clsMedium == m2.clsMedium && m1.dw1 == m2.dw1 && m1.dw2 == m2.dw2)
                  {
                    // Stage 5: yes! Add and connect the filter.
                    hr = _graph.AddFilter(tmpCiFilter, captureDevice.Name);
                    if (hr != (int)NativeMethods.HResult.S_OK)
                    {
                      this.LogError("Digital Devices: failed to add the filter for {0} {1} to the graph, hr = 0x{2:x}", captureDevice.Name, captureDevice.DevicePath, hr);
                      break;
                    }
                    hr = _graph.ConnectDirect(lastFilterOutputPin, tmpFilterInputPin, null);
                    if (hr != (int)NativeMethods.HResult.S_OK)
                    {
                      this.LogError("Digital Devices: failed to connect the matching filter for {0} {1} into the graph, hr = 0x{2:x}", captureDevice.Name, captureDevice.DevicePath, hr);
                      _graph.RemoveFilter(tmpCiFilter);
                    }
                    break;
                  }
                }
                if (hr == (int)NativeMethods.HResult.S_OK)
                {
                  break;
                }
              }
            }
            finally
            {
              Release.ComObject("Digital Devices CI filter input pin", ref tmpFilterInputPin);
            }
            if (hr != (int)NativeMethods.HResult.S_OK)
            {
              Release.ComObject("Digital Devices CI filter", ref tmpCiFilter);
              captureDevice.Dispose();
              continue;
            }

            // Excellent - CI filter successfully added!
            this.LogDebug("Digital Devices:   added CI {0}, name = {1}, device path = {2}", ciIndex, captureDevice.Name, captureDevice.DevicePath);
            PrivateCiContext context = new PrivateCiContext(captureDevice, tmpCiFilter, ciIndex);
            _privateCiContexts.Add(captureDevice.DevicePath, context);
            lastFilter = tmpCiFilter;
            addedFilter = true;
            _isCiSlotPresent = true;
            break;
          }

          // Insurance: we don't want to get stuck in an endless loop.
          if (!addedFilter)
          {
            this.LogWarn("Digital Devices: filter not added, exiting loop");
            break;
          }
        }
        finally
        {
          Release.ComObject("Digital Devices upstream filter output pin", ref lastFilterOutputPin);
        }
      }

      this.LogInfo("Digital Devices: total of {0} CI filter(s) in the graph", _privateCiContexts.Count);
      return _isCiSlotPresent;
    }

    /// <summary>
    /// Get the mediums for a pin.
    /// </summary>
    /// <param name="pin">The pin.</param>
    /// <returns>the pin's mediums (if any); <c>null</c> if the pin is not a KS pin</returns>
    private static ICollection<RegPinMedium> GetPinMediums(IPin pin)
    {
      IKsPin ksPin = pin as IKsPin;
      if (ksPin == null)
      {
        return new List<RegPinMedium>(0);
      }

      IntPtr ksMultiple = IntPtr.Zero;
      int hr = ksPin.KsQueryMediums(out ksMultiple);
      // Can return 1 (S_FALSE) for non-error scenarios.
      if (hr < (int)NativeMethods.HResult.S_OK)
      {
        return new List<RegPinMedium>(0);
      }
      try
      {
        int mediumCount = Marshal.ReadInt32(ksMultiple, sizeof(int));
        List<RegPinMedium> mediums = new List<RegPinMedium>(mediumCount);
        IntPtr mediumPtr = IntPtr.Add(ksMultiple, 8);
        int regPinMediumSize = Marshal.SizeOf(typeof(RegPinMedium));
        for (int i = 0; i < mediumCount; i++)
        {
          RegPinMedium m = (RegPinMedium)Marshal.PtrToStructure(mediumPtr, typeof(RegPinMedium));
          // Exclude invalid and non-meaningful mediums.
          if (m.clsMedium != Guid.Empty && m.clsMedium != TveGuid.KS_MEDIUM_SET_ID_STANDARD)
          {
            mediums.Add(m);
          }
          mediumPtr = IntPtr.Add(mediumPtr, regPinMediumSize);
        }
        return mediums;
      }
      finally
      {
        Marshal.FreeCoTaskMem(ksMultiple);
      }
    }

    #endregion

    #region IStreamSelector members

    /// <summary>
    /// Get the identifiers for the available streams.
    /// </summary>
    /// <param name="streamIds">The stream identifiers.</param>
    /// <returns><c>true</c> if the stream identifiers are retrieved successfully, otherwise <c>false</c></returns>
    public bool GetAvailableStreamIds(out ICollection<int> streamIds)
    {
      this.LogDebug("Digital Devices: get available stream IDs");
      streamIds = null;

      if (!_isDigitalDevices || _propertySet == null)
      {
        this.LogWarn("Digital Devices: not initialised or interface not supported");
        return false;
      }

      int returnedByteCount;
      int hr = _propertySet.Get(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.PlpIds, _plpBuffer, PLP_INFO_SIZE, _plpBuffer, PLP_INFO_SIZE, out returnedByteCount);
      if (hr == (int)NativeMethods.HResult.S_OK && returnedByteCount == PLP_INFO_SIZE)
      {
        this.LogDebug("Digital Devices: result = success");
        PlpInfo info = (PlpInfo)Marshal.PtrToStructure(_plpBuffer, typeof(PlpInfo));
        streamIds = new List<int>(info.PlpCount);
        for (int i = 0; i < info.PlpCount; i++)
        {
          streamIds.Add(info.PlpIdList[i]);
        }
        return true;
      }

      this.LogError("Digital Devices: failed to get available stream IDs, hr = 0x{0:x}, byte count = {1}", hr, returnedByteCount);
      return false;
    }

    /// <summary>
    /// Select a stream.
    /// </summary>
    /// <param name="streamId">The identifier of the stream to select.</param>
    /// <returns><c>true</c> if the stream is selected successfully, otherwise <c>false</c></returns>
    public bool SelectStream(int streamId)
    {
      this.LogDebug("Digital Devices: select stream, stream ID = {0}", streamId);

      if (!_isDigitalDevices || _propertySet == null)
      {
        this.LogWarn("Digital Devices: not initialised or interface not supported");
        return false;
      }

      PlpInfo info = new PlpInfo();
      info.PlpId = streamId;
      info.Version = 1;
      info.Length = PLP_INFO_SIZE;
      Marshal.StructureToPtr(info, _plpBuffer, false);
      int hr = _propertySet.Set(BDA_EXTENSION_PROPERTY_SET, (int)BdaExtensionProperty.PlpIds, _plpBuffer, PLP_INFO_SIZE, _plpBuffer, PLP_INFO_SIZE);
      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Digital Devices: result = success");
        return true;
      }

      this.LogError("Digital Devices: failed to select stream, hr = 0x{0:x}, stream ID = {1}", hr, streamId);
      return false;
    }

    #endregion

    #region ITvServerPlugin members

    /// <summary>
    /// The version of this TV Server plugin.
    /// </summary>
    public string Version
    {
      get
      {
        return "1.0.0.0";
      }
    }

    /// <summary>
    /// The author of this TV Server plugin.
    /// </summary>
    public string Author
    {
      get
      {
        return "mm1352000";
      }
    }

    /// <summary>
    /// Get an instance of the configuration section for use in TV Server configuration (SetupTv).
    /// </summary>
    public SectionSettings Setup
    {
      get
      {
        return new DigitalDevicesConfig();
      }
    }

    /// <summary>
    /// Start this TV Server plugin.
    /// </summary>
    public void Start(IInternalControllerService controllerService)
    {
      this.LogDebug("Digital Devices: plugin enabled");
      _isPluginEnabled = true;
    }

    /// <summary>
    /// Stop this TV Server plugin.
    /// </summary>
    public void Stop()
    {
      this.LogDebug("Digital Devices: plugin disabled");
      _isPluginEnabled = false;
    }

    #endregion

    #region ITvServerPluginCommunication members

    /// <summary>
    /// Supply a service class implementation for client-server plugin communication.
    /// </summary>
    public object GetServiceInstance
    {
      get
      {
        return _service;
      }
    }

    /// <summary>
    /// Supply a service class interface for client-server plugin communication.
    /// </summary>
    public Type GetServiceInterfaceForContractType
    {
      get
      {
        return typeof(IDigitalDevicesConfigService);
      }
    }

    #endregion

    #region IConditionalAccessProvider members

    /// <summary>
    /// Open the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.Open()
    {
      this.LogDebug("Digital Devices: open conditional access interface");

      if (!_isDigitalDevices)
      {
        this.LogWarn("Digital Devices: not initialised or interface not supported");
        return false;
      }
      if (!_isCiSlotPresent)
      {
        this.LogDebug("Digital Devices: CI filter(s) not added to the BDA filter graph");
        return false;
      }
      if (_isCaInterfaceOpen)
      {
        this.LogWarn("Digital Devices: conditional access interface is already open");
        return true;
      }

      lock (_sharedCiContextsLock)
      {
        foreach (PrivateCiContext privateContext in _privateCiContexts.Values)
        {
          int ciIndex = privateContext.Slot.Index;
          string ciDevicePath = privateContext.Device.DevicePath;
          bool isFirstLoad = true;
          SharedCiContext sharedContext;
          if (_sharedCiContexts.TryGetValue(ciDevicePath, out sharedContext))
          {
            if (!string.IsNullOrEmpty(sharedContext.OwnerExternalId))
            {
              this.LogDebug("Digital Devices: tuner {0} already owns CI {1}", sharedContext.OwnerIndex, ciIndex);
              continue;
            }
            isFirstLoad = false;
          }
          else
          {
            this.LogDebug("Digital Devices: tuner {0} loading new CI {1}", _tunerIndex, ciIndex);
            sharedContext = new SharedCiContext(ciDevicePath, privateContext.Device.Name);
            _sharedCiContexts.Add(ciDevicePath, sharedContext);
          }
          sharedContext.SetOwner(_tunerExternalId, _tunerIndex, privateContext.Slot);
          bool isChanged = sharedContext.UpdateStateInfo();
          isChanged |= sharedContext.UpdateConfig();

          if (isFirstLoad || isChanged)
          {
            this.LogDebug("  decrypt limit   = {0}", sharedContext.DecryptLimit);
            this.LogDebug("  providers       = {0}", string.Join(", ", sharedContext.Providers));
            this.LogDebug("  is CAM ready    = {0}", sharedContext.IsCamReady);
            if (sharedContext.IsCamReady)
            {
              this.LogDebug("  CAM title       = {0}", sharedContext.CamMenuTitle);
              this.LogDebug("  # CAS IDs       = {0}", sharedContext.CamCasIds.Count);
              for (int i = 0; i < sharedContext.CamCasIds.Count; i++)
              {
                this.LogDebug("    {0, -13} = 0x{1:x4}", i + 1, sharedContext.CamCasIds[i]);
              }
            }
            else
            {
              sharedContext.McdPrograms.Clear();
              sharedContext.MtdPrograms.Clear();
            }
            this.LogDebug("  CI bit rate     = {0} b/s", sharedContext.CiBitRate);
            this.LogDebug("  CI max bit rate = {0} b/s", sharedContext.CiMaxBitRate);
            this.LogDebug("  CI tuner count  = {0}", sharedContext.CiTunerCount);
          }
        }
      }

      _ciSlotsWithChangedServices = new HashSet<string>();
      _isCaInterfaceOpen = true;
      StartMmiHandlerThread();

      this.LogDebug("Digital Devices: result = success");
      return true;
    }

    /// <summary>
    /// Close the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.Close()
    {
      return CloseConditionalAccessInterface(true);
    }

    private bool CloseConditionalAccessInterface(bool isDisposing)
    {
      this.LogDebug("Digital Devices: close conditional access interface");

      if (isDisposing)
      {
        StopMmiHandlerThread();

        lock (_sharedCiContextsLock)
        {
          foreach (string ciDevicePath in _privateCiContexts.Keys)
          {
            SharedCiContext sharedContext = _sharedCiContexts[ciDevicePath];
            if (_tunerExternalId.Equals(sharedContext.OwnerExternalId))
            {
              sharedContext.SetOwner(null, -1, null, true);
            }
            sharedContext.MtdPrograms.Remove(_tunerExternalId);
            if (sharedContext.CiTunerCount == 1)
            {
              sharedContext.McdPrograms.Clear();
            }
          }
        }

        // We reserve the removal of the filters from the graph for when the tuner is disposed,
        // otherwise the interface cannot easily be re-opened.
        _ciSlotsWithChangedServices = null;
      }

      _isCaInterfaceOpen = false;

      this.LogDebug("Digital Devices: result = success");
      return true;
    }

    /// <summary>
    /// Reset the conditional access interface.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully reset, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.Reset()
    {
      this.LogDebug("Digital Devices: reset conditional access interface");

      if (!_isDigitalDevices)
      {
        this.LogWarn("Digital Devices: not initialised or interface not supported");
        return false;
      }
      if (!_isCiSlotPresent)
      {
        this.LogDebug("Digital Devices: CI slot not present");
        return false;
      }

      bool success = (this as IConditionalAccessProvider).Close();

      // Reset the slot selection for menu browsing.
      _menuContext = null;
      _menuSlotIndex = -1;

      // We reset all the CI filters in the graph. Note this may stop
      // decryption or streaming of channels that other tuners are receiving.
      // We trust that this is necessary.
      foreach (PrivateCiContext context in _privateCiContexts.Values)
      {
        this.LogDebug("Digital Devices: reset slot {0}", context.Slot.Index);
        int hr = context.Slot.ResetCam();
        if (hr == (int)NativeMethods.HResult.S_OK)
        {
          this.LogDebug("Digital Devices: result = success");
        }
        else
        {
          this.LogError("Digital Devices: failed to reset CI slot {0}, hr = 0x{1:x}", context.Slot.Index, hr);
          success = false;
        }
      }
      return success && (this as IConditionalAccessProvider).Open();
    }

    /// <summary>
    /// Determine whether the conditional access interface is ready to receive commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is ready, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.IsReady()
    {
      this.LogDebug("Digital Devices: is conditional access interface ready");

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("Digital Devices: not initialised or interface not supported");
        return false;
      }

      // Unfortunately We can't directly determine if the CAM(s) are ready. We
      // can only assume that the CAM is ready if we're able to read the root
      // menu title. We can't be sure which CAM the caller is interested in, so
      // we return true if any one CAM is ready.
      bool isCamReady = false;
      lock (_sharedCiContextsLock)
      {
        foreach (string ciDevicePath in _privateCiContexts.Keys)
        {
          if (_sharedCiContexts[ciDevicePath].IsCamReady)
          {
            isCamReady = true;
            break;
          }
        }
      }

      this.LogDebug("Digital Devices: result = {0}", isCamReady);
      return isCamReady;
    }

    /// <summary>
    /// Determine whether the conditional access interface requires access to
    /// the MPEG 2 conditional access table in order to successfully decrypt
    /// programs.
    /// </summary>
    /// <returns><c>true</c> if access to the MPEG 2 conditional access table is required in order to successfully decrypt programs, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.IsConditionalAccessTableRequiredForDecryption()
    {
      return false;
    }

    /// <summary>
    /// Send a command to to the conditional access interface.
    /// </summary>
    /// <param name="listAction">It is assumed that the interface may be able to decrypt one or more programs
    ///   simultaneously. This parameter gives the interface an indication of the number of programs that it
    ///   will be expected to manage.</param>
    /// <param name="command">The type of command.</param>
    /// <param name="pmt">The program's map table.</param>
    /// <param name="cat">The conditional access table for the program's transport stream.</param>
    /// <param name="programProvider">The program's provider.</param>
    /// <returns><c>true</c> if the command is successfully sent, otherwise <c>false</c></returns>
    bool IConditionalAccessProvider.SendCommand(CaPmtListManagementAction listAction, CaPmtCommand command, TableProgramMap pmt, TableConditionalAccess cat, string programProvider)
    {
      this.LogDebug("Digital Devices: send conditional access command, list action = {0}, command = {1}", listAction, command);

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("Digital Devices: not initialised or interface not supported");
        return false;
      }
      if (command == CaPmtCommand.OkMmi || command == CaPmtCommand.Query)
      {
        this.LogError("Digital Devices: conditional access command type {0} is not supported", command);
        return true;
      }
      if (pmt == null)
      {
        this.LogError("Digital Devices: failed to send conditional access command, PMT not supplied");
        return true;
      }

      uint mtdProgramNumber = (uint)pmt.ProgramNumber | (uint)DecryptChainingRestriction.NoForwardChaining | (uint)DecryptChainingRestriction.NoBackwardChaining;

      // "Not selected" commands means stop decrypting. We don't actually stop
      // decryption as it may disrupt other streams, but we record the change.
      if (command == CaPmtCommand.NotSelected)
      {
        lock (_sharedCiContextsLock)
        {
          foreach (string ciDevicePath in _privateCiContexts.Keys)
          {
            SharedCiContext sharedContext = _sharedCiContexts[ciDevicePath];
            // MTD.
            uint currentMtdProgramNumber;
            if (sharedContext.MtdPrograms.TryGetValue(_tunerExternalId, out currentMtdProgramNumber) && currentMtdProgramNumber == mtdProgramNumber)
            {
              sharedContext.MtdPrograms.Remove(_tunerExternalId);
            }

            // MCD
            if (sharedContext.CiTunerCount == 1)
            {
              sharedContext.McdPrograms.Remove(pmt.ProgramNumber);
            }
          }
        }

        this.LogDebug("Digital Devices: result = success");
        return true;
      }

      if (listAction == CaPmtListManagementAction.First)
      {
        _ciSlotsWithChangedServices = new HashSet<string>();
      }

      this.LogDebug("Digital Devices: program number = {0}, provider = {1}", pmt.ProgramNumber, programProvider ?? string.Empty);
      int hr = (int)NativeMethods.HResult.S_OK;
      lock (_sharedCiContextsLock)
      {
        // Find a CI slot that we can use to decrypt the service.
        SharedCiContext selectedCiSlot = null;
        foreach (string ciDevicePath in _privateCiContexts.Keys)
        {
          SharedCiContext sharedContext = _sharedCiContexts[ciDevicePath];
          this.LogDebug("  slot {0}, {1}...", sharedContext.Slot.Index, sharedContext.CamMenuTitle);

          // Is the CAM able to decrypt the channel?
          if (!sharedContext.IsCamReady)
          {
            this.LogDebug("    CAM not ready");
            sharedContext.McdPrograms.Clear();
            sharedContext.MtdPrograms.Clear();
            continue;
          }
          if (!string.IsNullOrEmpty(programProvider) && sharedContext.Providers.Count > 0 && !sharedContext.Providers.Contains(programProvider))
          {
            this.LogDebug("    provider not supported");
            continue;
          }

          // When updating, we prefer to select the CI/CAM that is already decrypting the service.
          if (listAction == CaPmtListManagementAction.Update)
          {
            if (sharedContext.CiTunerCount == 1)
            {
              if (sharedContext.McdPrograms.ContainsKey(pmt.ProgramNumber))
              {
                this.LogDebug("    provider supported, found program in MCD list");
                selectedCiSlot = sharedContext;
                break;
              }
            }
            else
            {
              uint currentMtdProgramNumber;
              if (sharedContext.MtdPrograms.TryGetValue(_tunerExternalId, out currentMtdProgramNumber) && currentMtdProgramNumber == mtdProgramNumber)
              {
                this.LogDebug("    provider supported, found program in MTD list");
                selectedCiSlot = sharedContext;
                break;
              }
            }
          }

          // Does the CAM have the capacity to decrypt the channel?
          int currentDecryptCount = 0;
          if (sharedContext.CiTunerCount == 1)
          {
            currentDecryptCount = sharedContext.McdPrograms.Count;
          }
          else
          {
            if (sharedContext.MtdPrograms.ContainsKey(_tunerExternalId))
            {
              // Can't use this CAM because the 1 MTD slot for this tuner is
              // being used already for a different service.
              this.LogDebug("    provider supported, MTD already active");
              continue;
            }
            currentDecryptCount = sharedContext.MtdPrograms.Count;
          }

          if (sharedContext.DecryptLimit > 0 && currentDecryptCount >= sharedContext.DecryptLimit)
          {
            this.LogDebug("    provider supported, decrypt limit status = {0}/{1}, not possible to decrypt", currentDecryptCount, sharedContext.DecryptLimit);
          }
          else
          {
            this.LogDebug("    provider supported, decrypt limit status = {0}/{1}, possible to decrypt", currentDecryptCount, sharedContext.DecryptLimit);
            selectedCiSlot = sharedContext;
            if (listAction != CaPmtListManagementAction.Update)
            {
              // This CI might be okay for an update insomuchas it is capable
              // of decrypting the service... but it might not be the ideal
              // match (ie. the CI already decrypting the service).
              break;
            }
          }
        }

        if (selectedCiSlot == null)
        {
          this.LogError("Digital Devices: failed to send conditional access command, no slots available");
          return false;
        }

        // If we don't own the CI and this is the last command we expect to
        // send, take ownership. We should handle decrypt failure messages.
        if (!_tunerExternalId.Equals(selectedCiSlot.OwnerExternalId) && (selectedCiSlot.CiTunerCount != 1 || (listAction != CaPmtListManagementAction.First || listAction != CaPmtListManagementAction.More)))
        {
          selectedCiSlot.SetOwner(_tunerExternalId, _tunerIndex, _privateCiContexts[selectedCiSlot.DevicePath].Slot);
        }

        // MTD or MCD?
        if (selectedCiSlot.CiTunerCount == 1)
        {
          selectedCiSlot.McdPrograms[pmt.ProgramNumber] = pmt;
          if (listAction == CaPmtListManagementAction.Add || listAction == CaPmtListManagementAction.Update)
          {
            this.LogDebug("Digital Devices: sending MCD add/update decrypt request");
            hr = selectedCiSlot.Slot.SendCaPmt(pmt.GetCaPmt(listAction, command));
          }
          else if (listAction == CaPmtListManagementAction.First || listAction == CaPmtListManagementAction.More)
          {
            _ciSlotsWithChangedServices.Add(selectedCiSlot.DevicePath);
          }
          else
          {
            this.LogDebug("Digital Devices: sending MCD decrypt request(s)");
            foreach (string ciSlotDevicePath in _ciSlotsWithChangedServices)
            {
              SharedCiContext sharedContext = _sharedCiContexts[ciSlotDevicePath];
              this.LogDebug("  slot {0}, {1}...", sharedContext.Slot.Index, sharedContext.CamMenuTitle);
              int i = 1;
              foreach (TableProgramMap ciPmt in sharedContext.McdPrograms.Values)
              {
                CaPmtListManagementAction action = CaPmtListManagementAction.More;
                if (i == 1)
                {
                  if (sharedContext.McdPrograms.Count == 1)
                  {
                    action = CaPmtListManagementAction.Only;
                  }
                  else
                  {
                    action = CaPmtListManagementAction.First;
                  }
                }
                else if (sharedContext.McdPrograms.Count == i)
                {
                  action = CaPmtListManagementAction.Last;
                }

                int hr2 = selectedCiSlot.Slot.SendCaPmt(ciPmt.GetCaPmt(action, CaPmtCommand.OkDescrambling));
                hr |= hr2;
                this.LogDebug("    program number {0}, action {1}, hr = 0x{2:x}", ciPmt.ProgramNumber, action, hr2);
                i++;
              }
            }
          }
        }
        else
        {
          uint currentProgramNumber;
          if (selectedCiSlot.MtdPrograms.TryGetValue(_tunerExternalId, out currentProgramNumber) && currentProgramNumber == mtdProgramNumber)
          {
            // The driver ignores decrypt requests for the program number that
            // it is currently decrypting. This will force it to pay attention.
            this.LogDebug("Digital Devices: sending MTD decrypt update request");
            hr = selectedCiSlot.Slot.DecryptService(0);
            if (hr != (int)NativeMethods.HResult.S_OK)
            {
              this.LogWarn("Digital Devices: failed to send conditional access MTD update command, hr = 0x{0:x}", hr);
            }
          }
          this.LogDebug("Digital Devices: sending MTD decrypt request");
          selectedCiSlot.MtdPrograms[_tunerExternalId] = mtdProgramNumber;
          hr = selectedCiSlot.Slot.DecryptService(mtdProgramNumber);
        }
      }

      if (hr == (int)NativeMethods.HResult.S_OK)
      {
        this.LogDebug("Digital Devices: result = success");
        return true;
      }

      this.LogError("Digital Devices: failed to send conditional access command, hr = 0x{0:x}", hr);
      return false;
    }

    #endregion

    #region IConditionalAccessMenuActions members

    /// <summary>
    /// Set the menu call back delegate.
    /// </summary>
    /// <param name="callBack">The call back delegate.</param>
    void IConditionalAccessMenuActions.SetCallBack(IConditionalAccessMenuCallBack callBack)
    {
      lock (_caMenuCallBackLock)
      {
        _caMenuCallBack = callBack;
      }
      StartMmiHandlerThread();
    }

    /// <summary>
    /// Send a request from the user to the CAM to open the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.Enter()
    {
      this.LogDebug("Digital Devices: enter menu");

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("Digital Devices: not initialised or interface not supported");
        return false;
      }
      StartMmiHandlerThread();

      // If there are multiple CAMs available then we present the user with a
      // "fake" menu that allows them to choose which CAM they are interested
      // in. The choices are the root menu names for each of the CAMs.
      List<string> entries = new List<string>(_privateCiContexts.Count);
      _rootMenuChoices = new List<string>(_privateCiContexts.Count);
      string selectedCiSlotDevicePath = null;
      foreach (string ciSlotDevicePath in _privateCiContexts.Keys)
      {
        SharedCiContext sharedContext = _sharedCiContexts[ciSlotDevicePath];
        if (sharedContext.IsCamReady)
        {
          entries.Add(sharedContext.CamMenuTitle);
          _rootMenuChoices.Add(ciSlotDevicePath);
          selectedCiSlotDevicePath = ciSlotDevicePath;
        }
      }
      this.LogDebug("Digital Devices: there are {0} CI slot(s) present containing {1} CAM(s)", _privateCiContexts.Count, entries.Count);

      // If only one CAM is available then enter the menu directly.
      if (entries.Count == 1)
      {
        this.LogDebug("Digital Devices: entering menu directly");
        return EnterMenu(selectedCiSlotDevicePath);
      }

      this.LogDebug("Digital Devices: opening root menu");
      lock (_caMenuCallBackLock)
      {
        if (_caMenuCallBack == null)
        {
          this.LogDebug("Digital Devices: menu call back not set");
          return false;
        }

        _caMenuCallBack.OnCiMenu("CAM Selection", "Please select a CAM.", string.Empty, entries.Count);
        int i = 0;
        foreach (string entry in entries)
        {
          _caMenuCallBack.OnCiMenuChoice(i++, entry);
          this.LogDebug("  {0} = {1}", i, entry);
        }
      }

      // Reset the menu context. The user will choose the CAM they want to interact with.
      _menuContext = null;
      _menuSlotIndex = -1;

      this.LogDebug("Digital Devices: result = success");
      return true;
    }

    /// <summary>
    /// Enter the menu for a specific CAM/slot.
    /// </summary>
    /// <param name="ciSlotDevicePath">The device path of the CI slot containing the CAM.</param>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    private bool EnterMenu(string ciSlotDevicePath)
    {
      CiSlot privateSlot = _privateCiContexts[ciSlotDevicePath].Slot;
      this.LogDebug("Digital Devices: slot {0} enter menu", privateSlot.Index);

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("Digital Devices: not initialised or interface not supported");
        return false;
      }

      lock (_sharedCiContextsLock)
      {
        SharedCiContext context;
        if (!_sharedCiContexts.TryGetValue(ciSlotDevicePath, out context))
        {
          this.LogError("Digital Devices: failed to enter menu, no slot for context {0}", ciSlotDevicePath);
          return false;
        }
        context.SetOwner(_tunerExternalId, _tunerIndex, privateSlot);
        int hr = context.Slot.EnterCamMenu();
        if (hr == (int)NativeMethods.HResult.S_OK)
        {
          this.LogDebug("Digital Devices: result = success");
          // Future menu interactions will be passed to this CI slot/CAM.
          _menuContext = ciSlotDevicePath;
          _menuSlotIndex = context.Slot.Index;
          DigitalDevicesHardware.IsDevice(ciSlotDevicePath, out _menuSlotIndex);
          return true;
        }

        this.LogError("Digital Devices: failed to enter menu, hr = 0x{0:x}", hr);
        return false;
      }
    }

    /// <summary>
    /// Send a request from the user to the CAM to close the menu.
    /// </summary>
    /// <returns><c>true</c> if the request is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.Close()
    {
      this.LogDebug("Digital Devices: slot {0} close menu", _menuSlotIndex);

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("Digital Devices: not initialised or interface not supported");
        return false;
      }
      StartMmiHandlerThread();

      lock (_sharedCiContextsLock)
      {
        SharedCiContext context;
        if (!_sharedCiContexts.TryGetValue(_menuContext, out context))
        {
          this.LogError("Digital Devices: failed to close menu, no slot for context {0}", _menuContext);
          return false;
        }
        // We're closing the menu. No need to force ownership unless the CI is
        // not owned (in which case the slot would be null).
        if (string.IsNullOrEmpty(context.OwnerExternalId))
        {
          context.SetOwner(_tunerExternalId, _tunerIndex, _privateCiContexts[_menuContext].Slot);
        }
        else if (!_tunerExternalId.Equals(context.OwnerExternalId))
        {
          this.LogWarn("Digital Devices: non-owning tuner {0} closing menu for CI {1} owned by tuner {2}", _tunerIndex, context.Slot.Index, context.OwnerIndex);
        }
        int hr = context.Slot.CloseCamMenu();
        if (hr == (int)NativeMethods.HResult.S_OK)
        {
          this.LogDebug("Digital Devices: result = success");
          return true;
        }

        this.LogError("Digital Devices: failed to close menu, hr = 0x{0:x}", hr);
        return false;
      }
    }

    /// <summary>
    /// Send a menu entry selection from the user to the CAM.
    /// </summary>
    /// <param name="choice">The index of the selection as an unsigned byte value.</param>
    /// <returns><c>true</c> if the selection is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.SelectEntry(byte choice)
    {
      this.LogDebug("Digital Devices: slot {0} select menu entry, choice = {1}", _menuSlotIndex, choice);

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("Digital Devices: not initialised or interface not supported");
        return false;
      }
      StartMmiHandlerThread();

      // Is the user really interacting with the CAM menu, or are they interacting with
      // our "fake" root menu?
      if (_menuContext == null)
      {
        if (choice == 0)
        {
          this.LogDebug("Digital Devices: close root menu");
          lock (_caMenuCallBackLock)
          {
            if (_caMenuCallBack != null)
            {
              _caMenuCallBack.OnCiCloseDisplay(0);
            }
            else
            {
              this.LogDebug("Digital Devices: menu call back not set");
            }
          }
          return true;
        }
        else
        {
          if (choice > _rootMenuChoices.Count)
          {
            this.LogError("Digital Devices: selected root menu entry {0} is invalid", choice);
            return false;
          }
          return EnterMenu(_rootMenuChoices[choice - 1]);
        }
      }

      // The DD API doesn't pass back close requests from the CAM, so always
      // assume a selection closes the menu. If the CAM responds with a new
      // menu we can always reopen it.
      lock (_caMenuCallBackLock)
      {
        this.LogDebug("Digital Devices: close menu on selection");
        if (_caMenuCallBack != null)
        {
          _caMenuCallBack.OnCiCloseDisplay(0);
        }
        else
        {
          this.LogDebug("Digital Devices: menu call back not set");
        }
      }

      lock (_sharedCiContextsLock)
      {
        SharedCiContext context;
        if (!_sharedCiContexts.TryGetValue(_menuContext, out context))
        {
          this.LogError("Digital Devices: failed to select menu entry, no slot for context {0}", _menuContext);
          return false;
        }
        context.SetOwner(_tunerExternalId, _tunerIndex, _privateCiContexts[_menuContext].Slot, true);
        int hr = context.Slot.SelectCamMenuEntry(context.CamMenuId, choice);
        if (hr == (int)NativeMethods.HResult.S_OK)
        {
          this.LogDebug("Digital Devices: result = success");
          return true;
        }

        this.LogError("Digital Devices: failed to select menu entry, hr = 0x{0:x}", hr);
        return false;
      }
    }

    /// <summary>
    /// Send an answer to an enquiry from the user to the CAM.
    /// </summary>
    /// <param name="cancel"><c>True</c> to cancel the enquiry.</param>
    /// <param name="answer">The user's answer to the enquiry.</param>
    /// <returns><c>true</c> if the answer is successfully passed to and processed by the CAM, otherwise <c>false</c></returns>
    bool IConditionalAccessMenuActions.AnswerEnquiry(bool cancel, string answer)
    {
      if (answer == null)
      {
        answer = string.Empty;
      }
      this.LogDebug("Digital Devices: slot {0} answer enquiry, answer = {1}, cancel = {2}", _menuSlotIndex, answer, cancel);

      if (!_isCaInterfaceOpen)
      {
        this.LogWarn("Digital Devices: not initialised or interface not supported");
        return false;
      }
      StartMmiHandlerThread();

      lock (_sharedCiContextsLock)
      {
        SharedCiContext context;
        if (!_sharedCiContexts.TryGetValue(_menuContext, out context))
        {
          this.LogError("Digital Devices: failed to answer enquiry, no slot for context {0}", _menuContext);
          return false;
        }
        context.SetOwner(_tunerExternalId, _tunerIndex, _privateCiContexts[_menuContext].Slot, true);
        int hr = context.Slot.AnswerCamMenuEnquiry(context.CamMenuId, answer);
        if (hr == (int)NativeMethods.HResult.S_OK)
        {
          this.LogDebug("Digital Devices: result = success");
          return true;
        }
        this.LogError("Digital Devices: failed to answer enquiry, hr = 0x{0:x}", hr);
        return false;
      }
    }

    #endregion

    #region IDiseqcDevice members

    /// <summary>
    /// Send an arbitrary DiSEqC command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SendCommand(byte[] command)
    {
      if (_diseqcInterface != null)
      {
        return _diseqcInterface.SendCommand(command);
      }
      return false;
    }

    /// <summary>
    /// Send a tone/data burst command.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <returns><c>true</c> if the command is sent successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SendCommand(ToneBurst command)
    {
      if (_diseqcInterface != null)
      {
        // AFAIK not supported by the driver.
        return _diseqcInterface.SendCommand(command);
      }
      return false;
    }

    /// <summary>
    /// Set the 22 kHz continuous tone state.
    /// </summary>
    /// <param name="state">The state to set.</param>
    /// <returns><c>true</c> if the tone state is set successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.SetToneState(Tone22kState state)
    {
      if (_diseqcInterface != null)
      {
        // Set by tune request LNB frequency parameters.
        return _diseqcInterface.SetToneState(state);
      }
      return false;
    }

    /// <summary>
    /// Retrieve the response to a previously sent DiSEqC command (or alternatively, check for a command
    /// intended for this tuner).
    /// </summary>
    /// <param name="response">The response (or command).</param>
    /// <returns><c>true</c> if the response is read successfully, otherwise <c>false</c></returns>
    bool IDiseqcDevice.ReadResponse(out byte[] response)
    {
      if (_diseqcInterface != null)
      {
        return _diseqcInterface.ReadResponse(out response);
      }
      response = null;
      return false;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    ~DigitalDevices()
    {
      Dispose(false);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (_isDigitalDevices)
      {
        CloseConditionalAccessInterface(isDisposing);
      }

      if (isDisposing)
      {
        if (_privateCiContexts != null)
        {
          foreach (PrivateCiContext context in _privateCiContexts.Values)
          {
            if (context.Slot != null)
            {
              context.Slot.Dispose();
              context.Slot = null;
            }
            if (context.Device != null)
            {
              context.Device.Dispose();
              context.Device = null;
            }
            if (context.Filter != null)
            {
              if (_graph != null)
              {
                _graph.RemoveFilter(context.Filter as IBaseFilter);
              }
              Release.ComObject("Digital Devices CI filter", ref context.Filter);
            }
          }
          _privateCiContexts = null;
        }
        Release.ComObject("Digital Devices graph", ref _graph);

        IDisposable d = _diseqcInterface as IDisposable;
        if (d != null)
        {
          d.Dispose();
        }
        _diseqcInterface = null;

        _propertySet = null;
      }

      if (_plpBuffer != IntPtr.Zero)
      {
        Marshal.FreeCoTaskMem(_plpBuffer);
        _plpBuffer = IntPtr.Zero;
      }

      _isDigitalDevices = false;
    }

    #endregion
  }
}