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
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using MPRepository.Users;
using MPRepository.Controller;

namespace MPRepository.Web
{
  public partial class login : System.Web.UI.Page
  {

    #region log4net
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger("WebLog");
    #endregion //log4net
    
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    protected void Login1_Authenticate(object sender, AuthenticateEventArgs e)
    {
      e.Authenticated = false;

      // TODO: replace with real authentication
      if (Login1.Password != "password")
      {
        return;
      }

      // Add user to session
      string handle = Login1.UserName;
      MPRSession session = MPRController.StartSession();
      IList<MPUser> users = MPRController.RetrieveEquals<MPUser>(session, "Handle", handle);

      if (users.Count != 1) // Either none or a serious error
      {
        MPRController.EndSession(session, false);
        return;
      }

      MPUser user = users[0];
      user.LastLogin = DateTime.Now;
      MPRController.Save<MPUser>(session, user);

      HttpContext.Current.Session["MPUser"] = user;

      MPRController.EndSession(session, true);

      e.Authenticated = true;

      log.Info(String.Format("User {0} has logged in", user.Handle));

      FormsAuthentication.RedirectFromLoginPage(Login1.UserName, Login1.RememberMeSet);

    }
  }
}
