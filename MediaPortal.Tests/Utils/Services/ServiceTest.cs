#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using MediaPortal.Services;
using MediaPortal.Tests.MockObjects;
using NUnit.Framework;

namespace MediaPortal.Tests.Utils.Services
{
    [TestFixture]
    [Category("ServiceProvider")]
    public class ServiceTest
    {
        [Test]
        public void TestServiceCreatorCallback()
        {
            ServiceProvider provider = new ServiceProvider();
            provider.Add<ILog>(new ServiceCreatorCallback<ILog>(ServiceRequested));
            ILog log = provider.Get<ILog>();
            Assert.IsNotNull(log);
        }

        /// <summary>
        /// Tests whether we are getting an exception when we try to add a service that is already
        /// registered.
        /// </summary>
        [Test]
        [
            ExpectedException(typeof(ArgumentException),
                "A service of type MediaPortal.Services.ILog is already present")]
        public void TestAddDuplicateService1()
        {
            ServiceProvider provider = new ServiceProvider();
            ILog log1 = new NoLog();
            ILog log2 = new NoLog();
            provider.Add<ILog>(log1);
            provider.Add<ILog>(log2);
        }

        /// <summary>
        /// Tests whether we can replace a service callback with a real service implementation using the Add method
        /// </summary>
        [Test]
        public void TestAddDuplicateService2()
        {
            ServiceProvider provider = new ServiceProvider();
            provider.Add<ILog>(new ServiceCreatorCallback<ILog>(ServiceRequested));
            ILog log1 = new NoLog();
            provider.Add<ILog>(log1);
        }

        private ILog ServiceRequested(ServiceProvider provider)
        {
            return new NoLog();
        }
        
        //[Test]
        //public void TestNamedInstanceServices()
        //{
        //    ServiceProvider provider = new ServiceProvider();
        //    provider.Add<ILogManager>(new LogManager());
        //    ILog log = provider.Get<ILogManager,ILog >("MediaPortal");
        //    log.Debug("Hello");
        //}
    }
}