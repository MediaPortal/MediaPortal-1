using System.Configuration;
using System.Data.Common;
using System.Data.Objects;
using System;
using System.Reflection;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.ObjContext
{
    public interface IObjectContextBuilder<T> where T : System.Data.Objects.ObjectContext
    {
        T BuildObjectContext();
    }

    public class ObjectContextBuilder<T> : DbModelBuilder, IObjectContextBuilder<T> where T : System.Data.Objects.ObjectContext
    {
        private readonly DbProviderFactory _factory;
        private readonly ConnectionStringSettings _cnStringSettings;
        private readonly bool _recreateDatabaseIfExists;
        private readonly bool _lazyLoadingEnabled;

        public ObjectContextBuilder(string connectionStringName, string[] mappingAssemblies, bool recreateDatabaseIfExists, bool lazyLoadingEnabled)
        {
            Conventions.Remove<IncludeMetadataConvention>();

            _cnStringSettings = ConfigurationManager.ConnectionStrings[connectionStringName];
            _factory = DbProviderFactories.GetFactory(_cnStringSettings.ProviderName);
            _recreateDatabaseIfExists = recreateDatabaseIfExists;
            _lazyLoadingEnabled = lazyLoadingEnabled;

            AddConfigurations(mappingAssemblies);
        }

        /// <summary>
        /// Creates a new <see cref="ObjectContext"/>.
        /// </summary>
        /// <param name="lazyLoadingEnabled">if set to <c>true</c> [lazy loading enabled].</param>
        /// <param name="recreateDatabaseIfExist">if set to <c>true</c> [recreate database if exist].</param>
        /// <returns></returns>
        public T BuildObjectContext()
        {
            var cn = _factory.CreateConnection();
            cn.ConnectionString = _cnStringSettings.ConnectionString;

            var dbModel = Build(cn);

            System.Data.Objects.ObjectContext ctx = dbModel.Compile().CreateObjectContext<System.Data.Objects.ObjectContext>(cn);
            ctx.ContextOptions.LazyLoadingEnabled = _lazyLoadingEnabled;

            if (!ctx.DatabaseExists())
            {
                ctx.CreateDatabase();
            }
            else if (_recreateDatabaseIfExists)
            {
                ctx.DeleteDatabase();
                ctx.CreateDatabase();
            }           

            return (T)ctx;
        }

        /// <summary>
        /// Adds mapping classes contained in provided assemblies and register entities as well
        /// </summary>
        /// <param name="mappingAssemblies"></param>
        private void AddConfigurations(string[] mappingAssemblies)
        {
            if (mappingAssemblies == null || mappingAssemblies.Length == 0)
            {
                throw new ArgumentNullException("mappingAssemblies", "You must specify at least one mapping assembly");
            }

            bool hasMappingClass = false;
            foreach (string mappingAssembly in mappingAssemblies)
            {
                Assembly asm = Assembly.LoadFrom(MakeLoadReadyAssemblyName(mappingAssembly));

                foreach (Type type in asm.GetTypes())
                {
                    if (!type.IsAbstract)
                    {
                        if (type.BaseType.IsGenericType && (type.BaseType.GetGenericTypeDefinition() == typeof(EntityTypeConfiguration<>)))
                        {
                            hasMappingClass = true;

                            // http://areaofinterest.wordpress.com/2010/12/08/dynamically-load-entity-configurations-in-ef-codefirst-ctp5/
                            dynamic configurationInstance = Activator.CreateInstance(type);
                            Configurations.Add(configurationInstance);
                        }
                    }
                }
            }

            if (!hasMappingClass)
            {
                throw new ArgumentException("No mapping class found!");
            }
        }        

        /// <summary>
        /// Ensures the assembly name is qualified
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        private static string MakeLoadReadyAssemblyName(string assemblyName)
        {
            return (assemblyName.IndexOf(".dll") == -1)
                ? assemblyName.Trim() + ".dll"
                : assemblyName.Trim();
        }
    }
}