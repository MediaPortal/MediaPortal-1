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

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension
{
  /// <summary>
  /// An interface for tuners that have the ability to control aspects of the video and/or audio
  /// stream format/encoding.
  /// </summary>
  public interface IEncoder : ITunerExtension
  {
    /// <summary>
    /// Determine whether the encoder can manipulate a parameter.
    /// </summary>
    /// <param name="parameterId">The unique identifier for the parameter.</param>
    /// <returns><c>true</c> if the parameter can be manipulated, otherwise <c>false</c></returns>
    bool IsParameterSupported(Guid parameterId);

    /// <summary>
    /// Get the extents and resolution for a parameter.
    /// </summary>
    /// <remarks>
    /// It is assumed that the caller has first checked that the encoder supports the parameter.
    /// The encoder may only accept specific values for the parameter. In this case the function
    /// may return an error. Use GetParameterValues() to retrieve the set of supported values for
    /// such parameters.
    /// </remarks>
    /// <param name="parameterId">The unique identifier for the parameter.</param>
    /// <param name="minimum">The minimum value that the parameter may take.</param>
    /// <param name="maximum">The maximum value that the parameter may take.</param>
    /// <param name="resolution">The magnitude of the smallest adjustment that can be applied to
    ///   the parameter. In most cases the value of the parameter should be a multiple of th.</param>
    /// <returns><c>true</c> if the parameter extents and resolution are successfully retrieved, otherwise <c>false</c></returns>
    bool GetParameterRange(Guid parameterId, out object minimum, out object maximum, out object resolution);

    /// <summary>
    /// Get the accepted/supported values for a parameter.
    /// </summary>
    /// <remarks>
    /// It is assumed that the caller has first checked that the encoder supports the parameter.
    /// The encoder may accept a pseudo-continuous range of values for the parameter, in which case
    /// it may be more appropriate to call GetParameterRange().
    /// </remarks>
    /// <param name="parameterId">The unique identifier for the parameter.</param>
    /// <param name="values">The possible values that the parameter may take.</param>
    /// <returns><c>true</c> if the parameter values are successfully retrieved, otherwise <c>false</c></returns>
    bool GetParameterValues(Guid parameterId, out object[] values);

    /// <summary>
    /// Get the default value for a parameter.
    /// </summary>
    /// <remarks>
    /// It is assumed that the caller has first checked that the encoder supports the parameter.
    /// </remarks>
    /// <param name="parameterId">The unique identifier for the parameter.</param>
    /// <param name="value">The default value for the parameter.</param>
    /// <returns><c>true</c> if the default parameter value is successfully retrieved, otherwise <c>false</c></returns>
    bool GetParameterDefaultValue(Guid parameterId, out object value);

    /// <summary>
    /// Get the current value of a parameter.
    /// </summary>
    /// <remarks>
    /// It is assumed that the caller has first checked that the encoder supports the parameter.
    /// </remarks>
    /// <param name="parameterId">The unique identifier for the parameter.</param>
    /// <param name="value">The current value of the parameter.</param>
    /// <returns><c>true</c> if the current parameter value is successfully retrieved, otherwise <c>false</c></returns>
    bool GetParameterValue(Guid parameterId, out object value);

    /// <summary>
    /// Set the value of a parameter.
    /// </summary>
    /// <remarks>
    /// It is assumed that the caller has first checked that the encoder supports the parameter.
    /// </remarks>
    /// <param name="parameterId">The unique identifier for the parameter.</param>
    /// <param name="value">The new value for the parameter.</param>
    /// <returns><c>true</c> if the parameter value is successfully set, otherwise <c>false</c></returns>
    bool SetParameterValue(Guid parameterId, object value);
  }
}