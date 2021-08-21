using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DataTesting.UnitTest.Helpers
{

    /// <summary>
    /// Every unit test classes need to implement (inherit) this class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TestBase<T> : IDisposable
    {

        private readonly IDatabaseHelper<T> databaseHelper;
        public static IServiceProvider ServiceProvider { get; set; }


        /// <summary>
        /// Use this method if is necessary to inject a dependency
        /// </summary>
        public TestBase()
        {

            var serviceCollection = new ServiceCollection();

            List<Assembly> assemblyList = new List<Assembly>();
            assemblyList.Add(Assembly.Load("DataTesting.UnitTest"));

            serviceCollection.Scan(scan => scan
            .FromAssemblies(assemblyList)
                   .AddClasses(classes => classes.Where(type => type.Name.Contains("Helper")))
                   .AsImplementedInterfaces()
                   .WithSingletonLifetime());

            ServiceProvider = serviceCollection.BuildServiceProvider();
            this.databaseHelper = ServiceProvider.GetService<IDatabaseHelper<T>>();

        }


        /// <summary>
        /// This method will execute Rollback and Close Connection everytime that a UnitTest run.
        /// </summary>
        public void Dispose()
        {
            if (databaseHelper.GetInfoSqlTransaction() != null)
            {
                databaseHelper.Rollback();
            }

            if (databaseHelper.GetInfoSqlConnection() != null && databaseHelper.GetInfoSqlConnection().State == System.Data.ConnectionState.Open)
            {
                databaseHelper.GetInfoSqlConnection().Close();
                databaseHelper.GetInfoSqlConnection().Dispose();
            }
        }
    }
}
