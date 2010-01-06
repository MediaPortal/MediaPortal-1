/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

using System;
using System.Collections.Generic;
using System.Text;

namespace TvLibrary.Interfaces
{
  /// <summary>
  /// Interface for CI menu actions;
  /// Each DVB card has to support these for CI menu access
  /// </summary>
  public interface ICiMenuActions
  {
    /// <summary>
    /// Set CI menu callback handler
    /// </summary>
    /// <param name="ciMenuHandler">callback handler</param>
    /// <returns>true</returns>
    bool SetCiMenuHandler(ICiMenuCallbacks ciMenuHandler);

    /// <summary>
    /// Enter CI menu
    /// </summary>
    /// <returns>true is successful</returns>
    bool EnterCIMenu();

    /// <summary>
    /// Closes CI menu
    /// </summary>
    /// <returns>true is successful</returns>
    bool CloseCIMenu();

    /// <summary>
    /// Selects a menu choice
    /// </summary>
    /// <param name="choice">choice (0 means back)</param>
    /// <returns>true if successful</returns>
    bool SelectMenu(byte choice);

    /// <summary>
    /// Send a menu answer after CAM inquiry
    /// </summary>
    /// <param name="Cancel">true to cancel</param>
    /// <param name="Answer">answer string</param>
    /// <returns>true if successful</returns>
    bool SendMenuAnswer(bool Cancel, string Answer);
  }
}