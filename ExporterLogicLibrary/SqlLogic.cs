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

        public static List<List<CellModel>> GetContentForTable(string server, string database, string table, List<ColumnModel>? columns = null)
        {
            if (table == string.Empty)
                throw new ArgumentException(Properties.Resources.EXCEPTION_TABLE_MISSING);

            columns ??= GetColumnsForTable(server, database, table);

            List<List<CellModel>> output = [];

            using (SqlConnection cnn = GetOpenConnection(server, database))
            {
                SqlCommand cmd = new($"SELECT {string.Join(", ", columns.Select(cm => "[" + cm.Name + "]"))} FROM [{table}];", cnn);

                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    List<CellModel> line = [];
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        if (reader[i] is byte[] v)
                            line.Add(new CellModel() { Type = columns[i].Type, Value = $"0x{BitConverter.ToString(v).Replace("-", "")}" });
                        else if (reader[i] is DateTime)
                        {
                            DateTime dt = reader.GetDateTime(i);
                            line.Add(new CellModel() { Type = columns[i].Type, Value = $"{dt:yyyy-MM-dd HH:mm:ss.fff}" });
                        }
                        else
                            line.Add(new CellModel() { Type = columns[i].Type, Value = $"{reader[i]}" });
                    }

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
                output = cnn.Query<string>("SELECT [name] FROM sys.databases;").AsList();
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
