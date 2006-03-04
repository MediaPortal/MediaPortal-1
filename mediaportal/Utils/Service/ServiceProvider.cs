/* 
 *	Copyright (C) 2005 Team MediaPortal
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
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.Utils.Services
{
  public class ServiceProvider
  {
    private Dictionary<Type, object> services;

    public ServiceProvider()
    {
      services = new Dictionary<Type, object>();
    }

    public bool IsRegistered<T>()
    {
      return services.ContainsKey(typeof(T));
    }

		public void Add<T>(object service)
    {
      // Make sure service implements type
			Type t = typeof(T);
      services.Add(t, service);
    }

    public T Get<T>()
    {
      Type t = typeof(T);
      if (services.ContainsKey(t))
      {
        return (T)services[t];
      }
      return default(T);
    }

    public void Remove<T>()
    {
      Type t = typeof(T);
      if (services.ContainsKey(t))
      {
          services.Remove(t);
      }
    }
  }
}
