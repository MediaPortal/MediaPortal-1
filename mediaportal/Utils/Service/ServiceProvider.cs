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

namespace MediaPortal.Utils.Service
{
    class ServiceProvider
    {
        private static Hashtable m_Services = null;

        public ServiceProvider()
        {
            m_Services = new Hashtable();
        }

        public void Add(object ServiceClass)
        {
            Type ServiceType = ServiceClass.GetType();

            if( !m_Services.ContainsKey(ServiceType.FullName) )
            {
                m_Services.Add(ServiceType.FullName, ServiceClass);
            }
        }

        public object Get(object ServiceClass)
        {
            Type ServiceType = ServiceClass.GetType();

            if (m_Services.ContainsKey(ServiceType.FullName))
            {
                return m_Services[ServiceType.FullName];
            }

            return null;
        }

        public void Remove(object ServiceClass)
        {
            Type ServiceType = ServiceClass.GetType();

            if (m_Services.ContainsKey(ServiceType.FullName))
            {
                m_Services.Remove(ServiceType.FullName);
            }
        }
    }
}
