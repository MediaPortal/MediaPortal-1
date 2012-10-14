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

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces.Device
{
  /// <summary>
  /// An interface for devices that implement conditional access features (most commonly service decryption).
  /// </summary>
  public interface IConditionalAccessProvider : ICustomDevice
  {
    /// <summary>
    /// Open the conditional access interface. For the interface to be opened successfully it is expected
    /// that any necessary hardware (such as a CI slot) is connected.
    /// </summary>
    /// <remarks>
    /// The interface may not be able to respond to commands immediately. Some providers only attempt to
    /// initialise hardware when the interface is opened, and initialising the hardware may take in the
    /// order of ten seconds.
    /// </remarks>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    bool OpenInterface();

    /// <summary>
    /// Close the conditional access interface.
    /// </summary>
    /// <remarks>
    /// It is expected that it would be unnecessary to call this function directly. Disposing the
    /// ICustomDevice instance should automatically close the interface.
    /// </remarks>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    bool CloseInterface();

    /// <summary>
    /// Reset the conditional access interface.
    /// </summary>
    /// <remarks>
    /// This might be necessary after an error condition is detected.
    /// </remarks>
    /// <param name="rebuildGraph">This parameter will be set to <c>true</c> if the BDA graph must be rebuilt
    ///   for the interface to be completely and successfully reset.</param>
    /// <returns><c>true</c> if the interface is successfully reopened, otherwise <c>false</c></returns>
    bool ResetInterface(out bool rebuildGraph);

    /// <summary>
    /// Determine whether the conditional access interface is ready to receive commands.
    /// </summary>
    /// <remarks>
    /// Ready means that necessary hardware (such as a conditional access module and smart card) is present
    /// and initialised, and necessary software interfaces have been opened/initialised.
    /// </remarks>
    /// <returns><c>true</c> if the interface is ready, otherwise <c>false</c></returns>
    bool IsInterfaceReady();

    /// <summary>
    /// Send a command to to the conditional access interface.
    /// </summary>
    /// <remarks>
    /// This function can be used to [for example]:
    /// - request that a service be decrypted
    /// - determine whether a service can be decrypted
    /// - indicate that a service need not be decrypted
    /// </remarks>
    /// <param name="channel">The channel information associated with the service which the command relates to.</param>
    /// <param name="listAction">It is assumed that the interface may be able to decrypt one or more services
    ///   simultaneously. This parameter gives the interface an indication of the number of services that it
    ///   will be expected to manage.</param>
    /// <param name="command">The type of command.</param>
    /// <param name="pmt">The programme map table for the service.</param>
    /// <param name="cat">The conditional access table for the service.</param>
    /// <returns><c>true</c> if the command is successfully sent, otherwise <c>false</c></returns>
    bool SendCommand(IChannel channel, CaPmtListManagementAction listAction, CaPmtCommand command, Pmt pmt, Cat cat);
  }
}
