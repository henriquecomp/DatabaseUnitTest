using DataTesting.UnitTest.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTesting.UnitTest.Helpers
{
    /// <summary>
    /// This interface/class is responsible to load csv data files into database or create a model from csv data file.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICsvHelper<T>
    {

        /// <summary>
        /// Load csv data file into an object WITHOUT load it on database
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="delimiter">Delimiter on csv data file</param>
        /// <returns></returns>
        IEnumerable<T> LoadCSVToModel(string path, char delimiter = ';');

        /// <summary>
        /// Load csv data file into a database
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="table">Database table name</param>
        /// <param name="delimiter">Delimiter on csv data file</param>
        /// <returns></returns>
        bool LoadCSVToDB(string path, string table, char delimiter = ';');
    }


    public class CsvHelper<T> : ICsvHelper<T>
    {
        private readonly IDatabaseHelper<T> databaseHelper;

        public CsvHelper(IDatabaseHelper<T> databaseHelper)
        {
            this.databaseHelper = databaseHelper;
        }

        public IEnumerable<T> LoadCSVToModel(string path, char delimiter = ';')
        {
            path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            var lines = File.ReadAllLines(path);

            if (lines.Count() <= 1)
            {
                throw new ArgumentNullException("Arquivo não pode ser em branco e deve conter mais que uma linha!");
            }

            var columns = lines[0].Split(delimiter);
            var dataTable = new DataTable();
            foreach (var c in columns)
                dataTable.Columns.Add(c);

            for (int i = 1; i < lines.Count(); i++)
                dataTable.Rows.Add(lines[i].Split(delimiter));

            return Helper.ConvertDataTable<T>(dataTable);

        }

        public bool LoadCSVToDB(string path, string table, char delimiter = ';')
        {
            path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            var lines = File.ReadAllLines(path);

            if (lines.Count() == 0)
            {
                return false;
            }

            var columns = lines[0].Split(delimiter);
            var dataTable = new DataTable();
            foreach (var c in columns)
                dataTable.Columns.Add(c);

            for (int i = 1; i < lines.Count(); i++)
                dataTable.Rows.Add(lines[i].Split(delimiter));

            databaseHelper.LoadData(table, dataTable);

            return true;
        }


    }
}
