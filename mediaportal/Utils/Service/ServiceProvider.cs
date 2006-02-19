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

namespace MediaPortal.Utils
{
    public class ServiceProvider
    {
        private Hashtable m_Services = null;
        
        public ServiceProvider()
        {
            m_Services = new Hashtable();
        }

        public bool IsRegistered(string ServiceId)
        {
            return m_Services.ContainsKey(ServiceId);
        }

        public void Add(Service ServiceClass)
        {
            if (ServiceClass.GetServiceId() != string.Empty)
            {
                m_Services.Add(ServiceClass.GetServiceId(), ServiceClass);
            }
        }

        public object Get(string ServiceId)
        {
            if (m_Services.ContainsKey(ServiceId))
            {
                return m_Services[ServiceId];
            }

            return null;
        }

        public void Remove(string ServiceId)
        {
            if (m_Services.ContainsKey(ServiceId))
            {
                m_Services.Remove(ServiceId);
            }
        }

        public void Replace(Service ServiceClass)
        {
            Remove(ServiceClass.GetServiceId());
            Add(ServiceClass);
        }
    }
}
