#region Copyright (C) 2005-2009 Team MediaPortal

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

#endregion

using System;
using System.Data;
using System.Configuration;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using MPRepository.Support;
using MPRepository.Controller;
using MPRepository.Users;
using MPRepository.Items;
using MPRepository.Storage;

namespace MPRepository.Web.Support
{
  /// <summary>
  /// This class provides utilities for the web application
  /// </summary>
  public static class SessionUtils
  {

    #region Public Methods

    public static MPUser GetCurrentUser()
    {
      MPUser user = (MPUser)HttpContext.Current.Session["MPUser"];
      if (user != null)
      {
        return user;
      }

      string handle = HttpContext.Current.User.Identity.Name;
      if (handle != null)
      {
        // user has been authenticated before (marked remember me)
        // TODO: move this logic to the PostAuthenticateRequest event handler

        MPRSession session = MPRController.StartSession();

        IList<MPUser> users = MPRController.RetrieveEquals<MPUser>(session, "Handle", handle);
        if (users.Count == 1)
        {
          user = users[0];
          user.LastLogin = DateTime.Now;
          MPRController.Save<MPUser>(session, user);
          MPRController.EndSession(session, true);

          HttpContext.Current.Session["MPUser"] = user;
          return user;
        }

        MPRController.EndSession(session, false);
      }

      throw new InvalidUserException("The session doesn't contain a valid user");
    }

    #endregion

  }
}
