using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Core.Resource;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using Castle.Windsor.Installer;

namespace MediaPortal.Common.Utils
{
    public static class WindsorService
    {
        private static bool _isInstalled = false;
        private static readonly object _locker = new object();
        private static IWindsorContainer _container = new WindsorContainer(new XmlInterpreter());
        private static readonly IList<IWindsorInstaller> _installers = new List<IWindsorInstaller>();

        /// <summary>
        /// Adds installer to the container.
        /// </summary>
        /// <param name="installer"></param>
        public static void AddInstaller(IWindsorInstaller installer)
        {
            lock (_locker)
            {
                if (!_installers.Contains<IWindsorInstaller>(installer))
                {
                    _installers.Add(installer);
                }
            }
        }

        /// <summary>
        /// Adds installer to the container using embedded xml configuration file.
        /// </summary>
        /// <param name="assembly">Assembly containing the configuration file.</param>
        /// <param name="filename">The name of the configuration file;
        /// path must be included (e.g.: Configuration/Components.xml).</param>
        public static void AddAssemblyResourceInstaller(string assembly, string filename)
        {
            string assemblyResourcePath =
                String.Format("assembly://{0}/{1}", assembly, filename);

            IWindsorInstaller installer =
                Configuration.FromXml(new AssemblyResource(assemblyResourcePath));

            lock (_locker)
            {
                if (!_installers.Contains<IWindsorInstaller>(installer))
                {
                    _installers.Add(installer);
                }
            }
        }

        /// <summary>
        /// Installs container.
        /// </summary>
        public static void Install()
        {
            if (!_isInstalled)
            {
                if (_installers.Count > 0)
                {
                    _container.Install(_installers.ToArray());
                    _isInstalled = true;
                }
                else
                {
                    throw new Exception(
                        "Can not install container: no installers have been added.");
                }
            }
            else
            {
                throw new Exception("Container is already installed! "
                    + "Clear() needs to be called before you can "
                    + "(re)install container.");
            }
        }

        /// <summary>
        /// Clears the container.
        /// </summary>
        public static void Clear()
        {
            _container.Dispose();
            _container = new WindsorContainer();
            _isInstalled = false;
        }

        /// <summary>
        /// Resolves object from container.
        /// </summary>
        /// <typeparam name="T">Type of object to resolve.</typeparam>
        /// <returns></returns>
        public static T Resolve<T>()
        {
            return _container.Resolve<T>();
        }

        /// <summary>
        /// Releases object fron container and frees up resources.
        /// </summary>
        /// <param name="instance"></param>
        public static void Release(object instance)
        {
            _container.Release(instance);
        }

        /// <summary>
        /// Adds a component to the container.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="instance">The instance of the object to add.</param>
        /*public static void Register<T>(T instance) where T : class
        {
            container.Register(Component.For<T>().Instance(instance));
        }*/

        public static void Register(params IRegistration[] registrations)
        {
            _container.Register(registrations);
        }

        public static T[] ResolveAll<T>()
        {
            return _container.ResolveAll<T>();            
        }
    }
}
