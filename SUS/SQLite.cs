﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace SUS
{
    public class SqlWrapper
    {
        private const string Database = "GameStates.db3";
        private static string m_Source;
        private static List<string> m_Queries;

        public SqlWrapper()
        {
            if (string.IsNullOrEmpty(m_Source))
            {
                m_Source = $"Data Source={Database};Version=3;";
            }

            if (m_Queries == null)
            {
                m_Queries = new List<string>();
                if (m_Queries.Count == 0)
                {
                    QueryInit();
                }
            }

            // Setup the database if it doesn't exist.
            if (!File.Exists(Database))
            {
                setupDatabase();
            }
        }

        private void setupDatabase()
        {
            string[] tables =
            {
                "create table gamestates (ID int64, State VARBINARY(MAX)"
            };

            Console.WriteLine("[ Creating Database... ]");
            SQLiteConnection.CreateFile(Database);

            Console.WriteLine(" => Creating Tables...");
            // Establish connection to create tables
            using (var db = new SQLiteConnection(m_Source))
            {
                db.Open();
                using (var cmd = new SQLiteCommand(db))
                using (var transaction = db.BeginTransaction())
                {
                    // Create the tables.
                    foreach (var table in tables)
                    {
                        Console.WriteLine($"   => {table,-30}");
                        cmd.CommandText = table;
                        cmd.ExecuteNonQuery();
                    }

                    Console.WriteLine(" => Commiting changes.");
                    transaction.Commit();
                }
            }
            Console.WriteLine("[ Database Established. ]");
        }

        #region Queries
        // Returns all matches to the index.
        public HashSet<T> GetAll<T>(int index)
        {
            // Validate we have a database, if not create and return.
            if (!File.Exists(Database))
            {
                setupDatabase();
                return null;
            }

            // Get our query string, return if it is a bad string.
            var queryString = QueryGet(index);
            if (string.IsNullOrEmpty(queryString))
            {
                return null;
            }

            // Passed the initial checks, create the HashSet.
            var items = new HashSet<T>();

            // Begin out query.
            using (var dbConnection = new SQLiteConnection(m_Source))
            {
                dbConnection.Open();
                using (var fmd = dbConnection.CreateCommand())
                {
                    fmd.CommandText = queryString;
                    fmd.CommandType = CommandType.Text;

                    var r = fmd.ExecuteReader();
                    while (r.Read())
                    {
                        // Process data here.
                        var obj = (T)Activator.CreateInstance(typeof(T), r);
                        items.Add(obj);
                    }
                }
            }

            return items;
        }

        // Returns the first occurence.
        public T Get<T>(int index)
        {
            // Validate we have a database, if not create and return.
            if (!File.Exists(Database))
            {
                setupDatabase();
                return default(T);
            }

            // Get our query string, return if it is a bad string.
            var queryString = QueryGet(index);
            if (string.IsNullOrEmpty(queryString))
            {
                return default(T);
            }

            // Begin out query.
            using (var dbConnection = new SQLiteConnection(m_Source))
            {
                dbConnection.Open();
                using (var fmd = dbConnection.CreateCommand())
                {
                    fmd.CommandText = queryString;
                    fmd.CommandType = CommandType.Text;

                    var r = fmd.ExecuteReader();
                    while (r.Read())
                    {
                        // Process data here.
                        var obj = (T)Activator.CreateInstance(typeof(T), r);
                        return obj;
                    }
                }
            }

            return default(T);
        }

        public void Insert(int index, object toInsert)
        {
            // Validate we have a database, if not create it and prepare for processing.
            if (!File.Exists(Database))
            {
                setupDatabase();
            }

            // Get our query string, return if it is a bad string.
            var queryString = QueryGet(index);
            if (string.IsNullOrEmpty(queryString))
            {
                return;
            }

            // Begin out query.
            using (var dbConnection = new SQLiteConnection(m_Source))
            {
                dbConnection.Open();
                var fmd = dbConnection.CreateCommand();
                fmd.CommandText = queryString;
                fmd.CommandType = CommandType.Text;

                // Verify the object is compatible and meets the minimum requirements to be inserted and selected.
                if (!(toInsert is ISQLCompatible obj))
                {
                    return;
                }

                // Get our information to store and assign it back to the SQLiteCommand.
                obj.ToInsert(fmd);

                fmd.ExecuteNonQuery();
            }
        }
        #endregion

        #region Miscellaneous
        private static string QueryGet(int index)
        {
            // If it is an invalid number (OOB on the List) then return early.
            if (index + 1 > m_Queries.Count || index < 0)
            {
                return "";
            }

            return m_Queries[index];
        }

        private static void QueryInit()
        {
            m_Queries.Add("SELECT * FROM gamestates");
            m_Queries.Add("INSERT INTO gamestates (ID, GameState) VALUES (@p1, @p2)");
        }

        private void TableInit(ref SQLiteConnection db)
        {

        }
        #endregion
    }
}
