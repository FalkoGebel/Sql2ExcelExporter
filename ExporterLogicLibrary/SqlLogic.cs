﻿using Dapper;
using ExporterLogicLibrary.Models;
using Microsoft.Data.SqlClient;

namespace ExporterLogicLibrary
{
    public static class SqlLogic
    {
        public static List<ColumnModel> GetColumnsForTable(string server, string database, string table)
        {
            if (table == string.Empty)
                throw new ArgumentException(Properties.Resources.EXCEPTION_TABLE_MISSING);

            List<ColumnModel> output;

            using (SqlConnection cnn = GetOpenConnection(server, database))
            {
                output = cnn.Query<ColumnModel>($"SELECT [COLUMN_NAME] AS Name, [DATA_TYPE] AS Type FROM INFORMATION_SCHEMA.COLUMNS WHERE [TABLE_NAME] = '{table}';").AsList();
            }

            return output;
        }

        public static List<List<string>> GetContentForTable(string server, string database, string table, List<string>? columns = null)
        {
            if (table == string.Empty)
                throw new ArgumentException(Properties.Resources.EXCEPTION_TABLE_MISSING);

            List<List<string>> output = [];

            using (SqlConnection cnn = GetOpenConnection(server, database))
            {
                SqlCommand cmd = new($"SELECT {(columns == null ? "*" : string.Join(", ", columns))} FROM {table};", cnn);

                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    List<string> line = [];
                    for (int i = 0; i < reader.FieldCount; i++)
                        line.Add($"{reader[i]}");

                    output.Add(line);
                }
            }

            return output;
        }

        public static List<string> GetDatabasesFromServer(string server)
        {
            List<string> output;

            using (SqlConnection cnn = GetOpenConnection(server))
            {
                output = cnn.Query<string>("SELECT name FROM sys.databases;").AsList();
            }

            return output;
        }

        public static List<string> GetTablesForDatabase(string server, string database)
        {
            List<string> output;

            using (SqlConnection cnn = GetOpenConnection(server, database))
            {
                output = cnn.Query<string>("SELECT [TABLE_NAME] FROM INFORMATION_SCHEMA.TABLES WHERE [TABLE_TYPE] = 'BASE TABLE';").AsList();
            }

            return output;
        }

        private static SqlConnection GetOpenConnection(string server, string? database = null)
        {
            if (server == string.Empty)
                throw new ArgumentException(Properties.Resources.EXCEPTION_SERVER_MISSING);

            if (database == string.Empty)
                throw new ArgumentException(Properties.Resources.EXCEPTION_DATABASE_MISSING);

            string connectionString = $@"Data Source={server};{(database != null ? $"Initial Catalog={database};" : "")}Integrated Security=SSPI;TrustServerCertificate=true;";

            SqlConnection cnn = new(connectionString);
            cnn.Open();

            return cnn;
        }
    }
}
