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

using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Dvb.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Mpeg2Ts;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension
{
  /// <summary>
  /// An interface for tuners that implement conditional access features (most
  /// commonly program decryption).
  /// </summary>
  public interface IConditionalAccessProvider : ITunerExtension
  {
    /// <summary>
    /// Open the conditional access interface.
    /// </summary>
    /// <remarks>
    /// For the interface to be opened successfully it is expected that any
    /// necessary hardware (such as a common interface slot) is connected.
    /// Conditional access modules and smart cards may or may not be present.
    /// 
    /// This function should be called as the first interaction with the
    /// interface. The interface may not be able to respond to commands
    /// immediately. Some providers only attempt to initialise hardware when
    /// the interface is opened, and initialising the hardware may take in the
    /// order of ten seconds.
    /// </remarks>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    bool Open();

    /// <summary>
    /// Close the conditional access interface.
    /// </summary>
    /// <remarks>
    /// In general this function should only be called if the interface is
    /// open, however providers should not return an error if the interface is
    /// not open. It is expected that it would be unnecessary to call this
    /// function directly. Disposing the <see cref="ITunerExtension"/> instance
    /// should automatically close the interface.
    /// </remarks>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    bool Close();

    /// <summary>
    /// Reset the conditional access interface.
    /// </summary>
    /// <remarks>
    /// This might be necessary if an error condition is detected.
    /// </remarks>
    /// <returns><c>true</c> if the interface is successfully reset, otherwise <c>false</c></returns>
    bool Reset();

    /// <summary>
    /// Determine whether the conditional access interface is ready to receive commands.
    /// </summary>
    /// <remarks>
    /// Ready means that necessary hardware (such as a conditional access
    /// module and smart card) is present and initialised, and necessary
    /// software interfaces have been opened/initialised.
    /// </remarks>
    /// <returns><c>true</c> if the interface is ready, otherwise <c>false</c></returns>
    bool IsReady();

    /// <summary>
    /// Determine whether the conditional access interface requires access to
    /// the MPEG 2 conditional access table in order to successfully decrypt
    /// programs.
    /// </summary>
    /// <returns><c>true</c> if access to the MPEG 2 conditional access table is required in order to successfully decrypt programs, otherwise <c>false</c></returns>
    bool IsConditionalAccessTableRequiredForDecryption();

    /// <summary>
    /// Send a command to to the conditional access interface.
    /// </summary>
    /// <remarks>
    /// This function can be used to [for example]:
    /// - request that a program be decrypted
    /// - determine whether a program can be decrypted
    /// - indicate that a program need not be decrypted
    /// </remarks>
    /// <param name="listAction">It is assumed that the interface may be able to decrypt one or more programs
    ///   simultaneously. This parameter gives the interface an indication of the number of programs that it
    ///   will be expected to manage.</param>
    /// <param name="command">The type of command.</param>
    /// <param name="pmt">The program's map table.</param>
    /// <param name="cat">The conditional access table for the program's transport stream.</param>
    /// <param name="programProvider">The program's provider.</param>
    /// <returns><c>true</c> if the command is successfully sent, otherwise <c>false</c></returns>
    bool SendCommand(CaPmtListManagementAction listAction, CaPmtCommand command, TableProgramMap pmt, TableConditionalAccess cat, string programProvider);
  }
}