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
using System.Collections.Generic;
using System.Text;
using System.Web;
using NHibernate;
using NHibernate.Cfg;
using log4net;

namespace MPRepository.Support
{
  /// <summary>
  /// This class helps with accessing the database
  /// </summary>
  public sealed class DatabaseHelper
  {

    #region log4net
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger("GeneralLog");
    #endregion //log4net

    private const string CurrentSessionKey = "nhibernate.current_session";
    private static readonly ISessionFactory sessionFactory;

    static DatabaseHelper()
    {
      try
      {
        log4net.Config.XmlConfigurator.Configure();
        log.Debug("Initializing database access");
        Configuration cfg = new Configuration();
        cfg.Configure();
        cfg.AddAssembly(typeof(MPRepository.Items.MPItem).Assembly);
        sessionFactory = cfg.BuildSessionFactory();
        log.Info("Session factory available");
      }
      catch (Exception ex)
      {
        log.Error(ex.ToString());
      }
    }

    public static ISession GetCurrentSession()
    {
      // For now, opens a new session on each request
      // TODO: Implement connection pool

      if (sessionFactory == null)
      {
        log.Error("SessionFactory is null when trying to access it");
        throw new InvalidOperationException("Unable to open a session because access to the data was not initialized correctly");
      }
      return sessionFactory.OpenSession();

    }

    public static void CloseSession(ISession session)
    {

      if (session == null)
      {
          // No current session
          return;
      }

      session.Close();
      
    }

    public static void CloseSessionFactory()
    {
      if (sessionFactory != null)
      {
        sessionFactory.Close();
      }
    }

  }
}