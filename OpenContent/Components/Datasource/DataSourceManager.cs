using DotNetNuke.Collections.Internal;
using DotNetNuke.Framework.Reflections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Satrabel.OpenContent.Components.Datasource
{
    public class DataSourceManager
    {

        private static readonly ILogAdapter Logger = App.Services.CreateLogger(typeof(DataSourceManager));
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
                    Logger.Error($"Unable to create {filterType.FullName} while GetDatasources. {e.Message}");
                    filter = null;
                }

                if (filter != null)
                {
                    yield return filter;
                }
            }
        }

        private static bool IsValidDataSourceProvider(Type t)
        {
            return t != null && t.IsClass && !t.IsAbstract && t.IsVisible && typeof(IDataSource).IsAssignableFrom(t);
        }

        public static IDataSource GetDataSource(string name)
        {
            if (string.IsNullOrEmpty(name))
                name = App.Config.Opencontent;

            var dataSource = _dataSources.SingleOrDefault(ds => ds.Name == name);
            if (dataSource == null)
            {
                throw new ArgumentException($"DataSource provider {name} doesn't exist");
            }
            return dataSource;
        }
    }

}