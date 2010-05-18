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

using System;
using DirectShowLib;
using TvLibrary.Implementations.DVB;

namespace TvLibrary.Implementations.Analog.QualityControl
{
  /// <summary>
  /// Class which implements control of quality trough the use of the IEncoderAPI interface
  /// </summary>
  public class EncoderAPIControl : BaseControl
  {
    #region variable

#pragma warning disable 618,612
    /// <summary>
    /// Instance of the encoder that supports the IEncoderAPI
    /// </summary>
    private readonly IEncoderAPI _encoderAPI;

    #endregion

    #region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="EncoderAPIControl"/> class.
    /// </summary>
    /// <param name="configuration">The encoder settings to use.</param>
    /// <param name="encoderAPI">The IEncoderAPI interface to the filter that must be used to control the quality.</param>
    public EncoderAPIControl(Configuration configuration, IEncoderAPI encoderAPI)
      : base(configuration)
    {
      _encoderAPI = encoderAPI;
      Log.Log.WriteFile("analog: IEncoderAPI supported by: " +
                        FilterGraphTools.GetFilterName(_encoderAPI as IBaseFilter) + "; Checking capabilities ");
      CheckCapabilities();
    }

    #endregion

#pragma warning restore 618,612

    #region protected method

    /// <summary>
    /// Checks if the encoder supports the given GUID
    /// </summary>
    /// <param name="guid">GUID</param>
    /// <returns>HR return value</returns>
    protected override int IsSupported(Guid guid)
    {
      return _encoderAPI.IsSupported(guid);
    }

    /// <summary>
    /// Sets the value for the give GUID
    /// </summary>
    /// <param name="guid">GUID</param>
    /// <param name="newBitRateModeO">Bit Rate Mode object</param>
    /// <returns>HR result</returns>
    protected override int SetValue(Guid guid, ref object newBitRateModeO)
    {
      return _encoderAPI.SetValue(guid, ref newBitRateModeO);
    }

    /// <summary>
    /// Gets the parameter range for the given GUID
    /// </summary>
    /// <param name="guid">GUID</param>
    /// <param name="valueMin">minimum out value</param>
    /// <param name="valueMax">maximum out value</param>
    /// <param name="steppingDelta">stepping delta out value</param>
    /// <returns>HR result</returns>
    protected override int GetParameterRange(Guid guid, out object valueMin, out object valueMax,
                                             out object steppingDelta)
    {
      return _encoderAPI.GetParameterRange(guid, out valueMin, out valueMax, out steppingDelta);
    }

    /// <summary>
    /// Returns the Default value for the given GUID
    /// </summary>
    /// <param name="guid">GUID</param>
    /// <param name="qualityObject">Quality out value</param>
    /// <returns>HR result object</returns>
    protected override object GetDefaultValue(Guid guid, out object qualityObject)
    {
      return _encoderAPI.GetDefaultValue(guid, out qualityObject);
    }

    #endregion
  }
}