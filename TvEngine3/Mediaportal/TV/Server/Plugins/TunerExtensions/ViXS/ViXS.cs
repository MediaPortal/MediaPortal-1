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
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.ViXS
{
  /// <summary>
  /// This class provides clear QAM tuning support for ATSC/QAM tuners that use ViXS
  /// chipsets/demodulators, such as Saber (DA-1N1-E, DA-1N1-I), VistaView and Asus tuners.
  /// </summary>
  public class ViXS : BaseTunerExtension, IDisposable
  {
    #region constants

    private static readonly Guid BDA_EXTENSION_PROPERTY_SET = new Guid(0x02779308, 0x77d8, 0x4914, 0x9f, 0x15, 0x7f, 0xa6, 0xe1, 0x55, 0x84, 0xc7);

    #endregion

    #region variables

    private bool _isVixs = false;
    private MicrosoftAtscQam.MicrosoftAtscQam _microsoftInterface = null;

    #endregion

    #region ITunerExtension members

    /// <summary>
    /// The loading priority for the extension.
    /// </summary>
    public override byte Priority
    {
      get
      {
        return 50;
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
      this.LogDebug("ViXS: initialising");

      if (_isVixs)
      {
        this.LogWarn("ViXS: extension already initialised");
        return true;
      }

      if (!tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.Atsc) && !tunerSupportedBroadcastStandards.HasFlag(BroadcastStandard.Scte))
      {
        this.LogDebug("ViXS: tuner type not supported");
        return false;
      }

      _microsoftInterface = new MicrosoftAtscQam.MicrosoftAtscQam(BDA_EXTENSION_PROPERTY_SET);
      if (!_microsoftInterface.Initialise(tunerExternalId, tunerSupportedBroadcastStandards, context))
      {
        this.LogDebug("ViXS: base Microsoft interface not supported");
        _microsoftInterface.Dispose();
        _microsoftInterface = null;
        return false;
      }

      this.LogInfo("ViXS: extension supported");
      _isVixs = true;
      return true;
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

    ~ViXS()
    {
      Dispose(false);
    }

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    /// <param name="isDisposing"><c>True</c> if the instance is being disposed.</param>
    private void Dispose(bool isDisposing)
    {
      if (isDisposing)
      {
        if (_microsoftInterface != null)
        {
          _microsoftInterface.Dispose();
          _microsoftInterface = null;
        }
        _isVixs = false;
      }
    }

    #endregion
  }
}