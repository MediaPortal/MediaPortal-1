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
using UPnP.Infrastructure.CP.DeviceTree;

namespace TvLibrary.Implementations.Dri.Service
{
  public sealed class DriCasCaptureMode
  {
    private readonly string _name;
    private static readonly IDictionary<string, DriCasCaptureMode> _values = new Dictionary<string, DriCasCaptureMode>();

    public static readonly DriCasCaptureMode Live = new DriCasCaptureMode("Live");
    public static readonly DriCasCaptureMode Buffer = new DriCasCaptureMode("Buffer");
    public static readonly DriCasCaptureMode Record = new DriCasCaptureMode("Record");

    private DriCasCaptureMode(string name)
    {
      _name = name;
      _values.Add(name, this);
    }

    public override string ToString()
    {
      return _name;
    }

    public override bool Equals(object obj)
    {
      DriCasCaptureMode captureMode = obj as DriCasCaptureMode;
      if (captureMode != null && this == captureMode)
      {
        return true;
      }
      return false;
    }

    public static ICollection<DriCasCaptureMode> Values
    {
      get { return _values.Values; }
    }

    public static explicit operator DriCasCaptureMode(string name)
    {
      DriCasCaptureMode value = null;
      if (!_values.TryGetValue(name, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator string(DriCasCaptureMode captureMode)
    {
      return captureMode._name;
    }
  }

  public sealed class DriCasDescramblingStatus
  {
    private readonly string _name;
    private static readonly IDictionary<string, DriCasDescramblingStatus> _values = new Dictionary<string, DriCasDescramblingStatus>();

    /// <summary>
    /// NoCard response or ca_pmt_reply() with ca_enable = 0x74 to 0xFF.
    /// </summary>
    public static readonly DriCasDescramblingStatus Unknown = new DriCasDescramblingStatus("Unknown");
    /// <summary>
    /// ca_pmt_reply() with ca_enable = 0x01
    /// </summary>
    public static readonly DriCasDescramblingStatus Possible = new DriCasDescramblingStatus("Possible");
    /// <summary>
    /// ca_pmt_reply() with ca_enable = 0x02
    /// </summary>
    public static readonly DriCasDescramblingStatus PossiblePurchaseDialog = new DriCasDescramblingStatus("Possible (purchase dialog)");
    /// <summary>
    /// ca_pmt_reply() with ca_enable = 0x03
    /// </summary>
    public static readonly DriCasDescramblingStatus PossibleTechnicalDialog = new DriCasDescramblingStatus("Possible (technical dialogue)");
    /// <summary>
    /// ca_pmt_reply() with ca_enable = 0x71
    /// </summary>
    public static readonly DriCasDescramblingStatus NotPossibleNoEntitlement = new DriCasDescramblingStatus("Not possible (no entitlement)");
    /// <summary>
    /// ca_pmt_reply() with ca_enable = 0x73
    /// </summary>
    public static readonly DriCasDescramblingStatus NotPossibleTechnicalReason = new DriCasDescramblingStatus("Not possible (technical reason)");

    private DriCasDescramblingStatus(string name)
    {
      _name = name;
      _values.Add(name, this);
    }

    public override string ToString()
    {
      return _name;
    }

    public override bool Equals(object obj)
    {
      DriCasDescramblingStatus descramblingStatus = obj as DriCasDescramblingStatus;
      if (descramblingStatus != null && this == descramblingStatus)
      {
        return true;
      }
      return false;
    }

    public static ICollection<DriCasDescramblingStatus> Values
    {
      get { return _values.Values; }
    }

    public static explicit operator DriCasDescramblingStatus(string name)
    {
      DriCasDescramblingStatus value = null;
      if (!_values.TryGetValue(name, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator string(DriCasDescramblingStatus descramblingStatus)
    {
      return descramblingStatus._name;
    }
  }

  public sealed class DriCasCardStatus
  {
    private readonly string _name;
    private static readonly IDictionary<string, DriCasCardStatus> _values = new Dictionary<string, DriCasCardStatus>();

    /// <summary>
    /// If a Card is inserted with no error and no firmware upgrade condition.
    /// </summary>
    public static readonly DriCasCardStatus Inserted = new DriCasCardStatus("Inserted");
    /// <summary>
    /// If there is no Card inserted.
    /// </summary>
    public static readonly DriCasCardStatus Removed = new DriCasCardStatus("Removed");
    /// <summary>
    /// If a Card is inserted and there is an error detected as defined in Appendix E of [CCIF].
    /// </summary>
    public static readonly DriCasCardStatus Error = new DriCasCardStatus("Error");
    /// <summary>
    /// If a Card is inserted with no error, but there is a pending
    /// firmware_upgrade() APDU. This can also be used to inform the DRIR the
    /// DRIT is upgrading after receiving a CVT message passed from the
    /// CableCARD Device.
    /// </summary>
    public static readonly DriCasCardStatus FirmwareUpgrade = new DriCasCardStatus("Firmware Upgrade");

    private DriCasCardStatus(string name)
    {
      _name = name;
      _values.Add(name, this);
    }

    public override string ToString()
    {
      return _name;
    }

    public override bool Equals(object obj)
    {
      DriCasCardStatus cardStatus = obj as DriCasCardStatus;
      if (cardStatus != null && this == cardStatus)
      {
        return true;
      }
      return false;
    }

    public static ICollection<DriCasCardStatus> Values
    {
      get { return _values.Values; }
    }

    public static explicit operator DriCasCardStatus(string name)
    {
      DriCasCardStatus value = null;
      if (!_values.TryGetValue(name, out value))
      {
        return null;
      }
      return value;
    }

    public static implicit operator string(DriCasCardStatus cardStatus)
    {
      return cardStatus._name;
    }
  }

  public enum DriCasMmiAction : byte
  {
    Close = 0,
    Open = 1
  }

  public enum DriCasMmiDisplayType : byte
  {
    FullScreen = 0,
    Overlay = 1,
    NewWindow = 2
    // 0x03..0xff reserved
  }

  public class CasService : BaseService
  {
    private CpAction _getCardStatusAction = null;
    private CpAction _getEntitlementAction = null;
    private CpAction _notifyMmiCloseAction = null;
    private CpAction _setChannelAction = null;
    private CpAction _setPreferredLanguageAction = null;

    public CasService(CpDevice device)
      : base(device, "urn:opencable-com:serviceId:urn:schemas-opencable-com:service:CAS")
    {
      _service.Actions.TryGetValue("GetCardStatus", out _getCardStatusAction);
      _service.Actions.TryGetValue("GetEntitlement", out _getEntitlementAction);
      _service.Actions.TryGetValue("NotifyMmiClose", out _notifyMmiCloseAction);
      _service.Actions.TryGetValue("SetChannel", out _setChannelAction);
      _service.Actions.TryGetValue("SetPreferredLanguage", out _setPreferredLanguageAction);
    }

    /// <summary>
    /// Upon receipt of the GetCardStatus action, the DRIT SHALL retrieve fromthe Card all the necessary information to
    /// respond with updated information in less than 1s.
    /// </summary>
    /// <param name="currentCardStatus">This argument provides the value of the CardStatus state variable when the action response is created.</param>
    /// <param name="currentCardManufacturer">This argument provides the value of the CardManufacturer state variable when the action response is created.</param>
    /// <param name="currentCardVersion">This argument provides the value of the CardVersion state variable when the action response is created.</param>
    /// <param name="currentDaylightSaving">This argument provides the value of the DaylightSaving state variable when the action response is created.</param>
    /// <param name="currentEaLocationCode">This argument provides the value of the EALocationCode state variable when the action response is created.</param>
    /// <param name="currentRatingRegion">This argument provides the value of the RatingRegion state variable when the action response is created.</param>
    /// <param name="currentTimeZone">This argument provides the value of the TimeZone state variable when the action response is created.</param>
    public void GetCardStatus(out DriCasCardStatus currentCardStatus, out string currentCardManufacturer,
                              out string currentCardVersion, out bool currentDaylightSaving, out UInt32 currentEaLocationCode,
                              out byte currentRatingRegion, out Int32 currentTimeZone)
    {
      IList<object> outParams = _getCardStatusAction.InvokeAction(null);
      currentCardStatus = (DriCasCardStatus)(string)outParams[0];
      currentCardManufacturer = (string)outParams[1];
      currentCardVersion = (string)outParams[2];
      currentDaylightSaving = (bool)outParams[3];
      currentEaLocationCode = (uint)outParams[4];
      currentRatingRegion = (byte)outParams[5];
      currentTimeZone = (int)outParams[6];
    }

    /// <summary>
    /// Upon receipt of the GetEntitlement action, the DRIT SHALL perform in sequence the following actions in less than 1s.
    /// 1.  Clear all the previous program PID from the PidList (Mux service) state variable.
    /// 2.  Configure the tuner based on the service information tables received from the Card and either of the
    ///     NewChannelNumber or the NewSourceId arguments. If both are defined and don’t point to the same
    ///     physical channel, then NewChannelNumber shall prevail.
    /// 3.  Set the ProgramNumber (Mux service) state variable.
    /// 4.  Reset the ACCI status message (see section 8.3) to 0.
    /// 5.  Setthe DescramblingStatus state variable to “Unknown”.
    /// 6.  Set the DescramblingMessage state variable to Null.
    /// 7.  Send a ca_pmt(Query) APDU to the Card.
    /// </summary>
    /// <param name="newChannelNumber">This argument sets the VirtualChannelNumber state variable.</param>
    /// <param name="newSourceId">This argument sets the VirtualChannelNumber state variable.</param>
    /// <param name="currentEntitlement">This argument provides the value of the DescramblingStatus state variable when the action response is created.</param>
    /// <param name="entitlementMessage">This argument provides the value of the DescramblingMessage state variable when the action response is created.</param>
    /// <returns><c>true</c> if the action is executed, otherwise <c>false</c></returns>
    public bool GetEntitlement(UInt32 newChannelNumber, UInt32 newSourceId, out DriCasDescramblingStatus currentEntitlement,
                                out string entitlementMessage)
    {
      currentEntitlement = DriCasDescramblingStatus.Unknown;
      entitlementMessage = string.Empty;
      if (_getEntitlementAction == null)
      {
        Log.Log.Debug("DRI: device {0} does not implement a CAS GetEntitlement action", _device.UDN);
        return false;
      }

      IList<object> outParams = _getEntitlementAction.InvokeAction(new List<object> { newChannelNumber, newSourceId });
      currentEntitlement = (DriCasDescramblingStatus)(string)outParams[0];
      entitlementMessage = (string)outParams[1];
      return true;
    }

    /// <summary>
    /// Upon receipt of the NotifyMmiClose action, the DRIT SHALL send a close_mmi_cnf() APDU to the Card with the
    /// MMI dialog number in less than 1s.
    /// </summary>
    /// <param name="mmiDialogNumber">This argument sets the A_ARG_TYPE_MMIDialogNumber state variable.</param>
    public void NotifyMmiClose(byte mmiDialogNumber)
    {
      _notifyMmiCloseAction.InvokeAction(new List<object> { mmiDialogNumber });
    }

    /// <summary>
    /// Upon receipt of the SetChannel action, the DRIT SHALL performin sequence the following actions in less than 1s.
    /// 1.  Clear all the previous program PID from the PidList (Mux service) state variable.
    /// 2.  Configure the tuner based on the service information tables received from the Card and either of the
    ///     NewChannelNumber or the NewSourceId arguments. If both are defined and don’t point to the same
    ///     physical channel, then NewChannelNumber prevails.
    /// 3.  Set the ProgramNumber (Mux service) state variable.
    /// 4.  Reset the ACCI status message (see section 8.3) to 0.
    /// 5.  Setthe DescramblingStatus (CAS service) state variable to “Unknown”.
    /// 6.  Set the DescramblingMessage (CAS service) state variable to Null.
    /// 7.  Send a ca_pmt(Ok_descrambling) APDU to the Card.
    /// </summary>
    /// <param name="newChannelNumber">This argument sets the VirtualChannelNumber state variable.</param>
    /// <param name="newSourceId">This argument sets the SourceId state variable.</param>
    /// <param name="newCaptureMode">This argument sets the CaptureMode state variable.</param>
    /// <param name="pcrLockStatus">This argument provides the value of A_ARG_TYPE_PCRLock state variable when the action response is created.</param>
    public void SetChannel(UInt32? newChannelNumber, UInt32? newSourceId, DriCasCaptureMode newCaptureMode, out bool pcrLockStatus)
    {
      // Note that the input argument order doesn't match the DRI
      // specification. This seems to be due to the hardware vendors choosing
      // to support the MS PBDA solution from WMC. The MS implementation sends
      // the parameters in the wrong order!
      IList<object> outParams = _setChannelAction.InvokeAction(new List<object> { newChannelNumber, newSourceId, newCaptureMode.ToString() });
      pcrLockStatus = (bool)outParams[0];
    }

    /// <summary>
    /// Upon receipt of the SetPreferredLanguage action, the DRIT SHALL send a feature_parameters() APDU to the
    /// Card with the language() parameter set to NewLanguage.
    /// </summary>
    /// <param name="newLanguage">This argument sets the Language state variable. The value should be an ISO639 code.</param>
    /// <returns><c>true</c> if the action is executed, otherwise <c>false</c></returns>
    public bool SetPreferredLanguage(string newLanguage)
    {
      if (_setPreferredLanguageAction == null)
      {
        Log.Log.Debug("DRI: device {0} does not implement a CAS SetPreferredLanguage action", _device.UDN);
        return false;
      }

      _setPreferredLanguageAction.InvokeAction(new List<object> { newLanguage });
      return true;
    }
  }
}
