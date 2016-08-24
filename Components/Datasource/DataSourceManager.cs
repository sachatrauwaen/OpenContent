using DotNetNuke.Collections.Internal;
using DotNetNuke.Framework.Reflections;
using DotNetNuke.Instrumentation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Satrabel.OpenContent.Components.Datasource
{
    public class DataSourceManager
    {

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DataSourceManager));
        private static NaiveLockingList<IDataSource> _dataSources;

        public static void RegisterDataSources()
        {
            _dataSources = new NaiveLockingList<IDataSource>();

            foreach (IDataSource ds in GetDataSources())
            {
                _dataSources.Add(ds);
            }
        }

        private static IEnumerable<IDataSource> GetDataSources()
        {
            var typeLocator = new TypeLocator();
            IEnumerable<Type> types = typeLocator.GetAllMatchingTypes(IsValidDataSourceProvider);

            foreach (Type filterType in types)
            {
                IDataSource filter;
                try
                {
                    filter = Activator.CreateInstance(filterType) as IDataSource;
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat("Unable to create {0} while GetDatasources.  {1}", filterType.FullName,
                                 e.Message);
                    filter = null;
                }

                if (filter != null)
                {
                    yield return filter;
                }
            }
        }

        internal static bool IsValidDataSourceProvider(Type t)
        {
            return t != null && t.IsClass && !t.IsAbstract && t.IsVisible && typeof(IDataSource).IsAssignableFrom(t);
        }

        public static IDataSource GetDataSource(string name)
        {
            if (string.IsNullOrEmpty(name))
                name = "OpenContent";

            var dataSource = _dataSources.SingleOrDefault(ds => ds.Name == name);
            if (dataSource == null)
            {
                throw new ArgumentException(string.Format("DataSource provider {0} dont exist", name));
            }
            return dataSource;
        }
    }

}