using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Serilog.Core;

namespace ErrorDataLayer
{
    public class SqLiteManager
    {
        public static readonly string BaseDir = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\RouteErrors";
        public static bool IsCheckQueue = true;

        private string _baseName = $"{BaseDir}\\{DateTime.Today.Date:dd_MM_yy}_RouteErrors.db3";

        static readonly string BaseNameToAdd = $"{BaseDir}\\{DateTime.Today.Date:dd_MM_yy}_RouteErrors.db3";

        private readonly ConcurrentQueue<ErrorEntity> _queueToAdd = new ConcurrentQueue<ErrorEntity>();

        private readonly Logger _log =
             new LoggerConfiguration().
                 MinimumLevel.Debug().
                 WriteTo.Console().
                 WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day).CreateLogger();  
        public SqLiteManager()
        {
            Log.Information("SqLiteManage constructor");
            if (!Directory.Exists(BaseDir))
            {
                Directory.CreateDirectory(BaseDir);
            }

            //CreateDb();

            Task.Factory.StartNew(CheckQueue);
        }

        private static void CreateDb()
        {
            if (!File.Exists(BaseNameToAdd))
            {
                SQLiteConnection.CreateFile(BaseNameToAdd);
                SQLiteFactory factory = (SQLiteFactory)DbProviderFactories.GetFactory("System.Data.SQLite");
                using (SQLiteConnection connection = (SQLiteConnection)factory.CreateConnection())
                {
                    if (connection != null)
                    {
                        connection.ConnectionString = "Data Source = " + BaseNameToAdd;
                        connection.Open();

                        using (SQLiteCommand command = new SQLiteCommand(connection))
                        {
                            command.CommandText = @"CREATE TABLE [RouteErrors] (
                            [id] integer PRIMARY KEY AUTOINCREMENT NOT NULL,
                            [imagev] BLOB NOT NULL,
                            [imagem] BLOB NOT NULL,
                            [comment] TEXT NOT NULL,
                            [position] TEXT NOT NULL,
                            [timestamp] TEXT NOT NULL,
                            [routeName] TEXT NOT NULL,
                            [username]  TEXT NULL                  
                            );";
                            command.CommandType = CommandType.Text;
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public string GetDbToAdd()
        {
            return $"{DateTime.Today.Date:dd_MM_yy}_RouteErrors";
        }

        public void SetBaseName(string baseName)
        {
            _baseName = $"{BaseDir}\\{baseName}.db3";
        }

        public List<String> GetAvailableDb()
        {
            List<String> dbList = new List<string>();
            var files = Directory.GetFiles(BaseDir, "*.db3");
            foreach (var file in files)
            {
                dbList.Add(Path.GetFileNameWithoutExtension(file));
            }
            return dbList;
        }

        public void AddErrorToDb(ErrorEntity error)
        {
            CreateDb();
            _queueToAdd.Enqueue(error);
        }

        private void CheckQueue()
        {
            while (IsCheckQueue)
            {
                while (_queueToAdd.TryDequeue(out var error))
                {
                    _log.Debug($"Queue count = {_queueToAdd.Count}");
                    SaveToDb(error);
                }
                Thread.Sleep(100);
                
            }
        }

        private void SaveToDb(ErrorEntity error)
        {
            SQLiteFactory factory = (SQLiteFactory) DbProviderFactories.GetFactory("System.Data.SQLite");
            using (SQLiteConnection connection = (SQLiteConnection) factory.CreateConnection())
            {
                if (connection != null)
                {
                    connection.ConnectionString = "Data Source = " + BaseNameToAdd;
                    connection.Open();

                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText =
                            $"INSERT INTO RouteErrors (imagev, imagem, comment, position, timestamp, routeName, username) VALUES (@0,@1,'{error.Comment}','{error.Position}','{error.TimeStamp}','{error.RouteName}','{error.User}');";
                        SQLiteParameter param0 = new SQLiteParameter("@0", DbType.Binary)
                        {
                            Value = error.ImageV
                        };
                        command.Parameters.Add(param0);

                        SQLiteParameter param1 = new SQLiteParameter("@1", DbType.Binary)
                        {
                            Value = error.ImageM
                        };
                        command.Parameters.Add(param1);

                        command.CommandType = CommandType.Text;
                        try
                        {
                           command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            UpdateDbStructure(ex.Message, Path.GetFileNameWithoutExtension(BaseNameToAdd));
                        }
                    }
                }
            }
        }

        public int GetErrorCount(string baseName = null)
        {
            int res = -1;
            SQLiteFactory factory = (SQLiteFactory) DbProviderFactories.GetFactory("System.Data.SQLite");
            using (SQLiteConnection connection = (SQLiteConnection) factory.CreateConnection())
            {
                if (connection != null)
                {
                    SetConnectionString(baseName, connection);

                    connection.Open();

                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        try
                        {
                            command.CommandType = CommandType.Text;
                            command.CommandText = "SELECT count(*) FROM [RouteErrors]";
                            object count = command.ExecuteScalar();
                            return Convert.ToInt32(count);
                        }
                        catch (Exception exc1)
                        {
                            _log.Error(exc1.Message);
                        }
                    }
                }
            }

            return res;
        }

        public ErrorEntity LoadError(int id, string baseName = null)
        {
            ErrorEntity error = new ErrorEntity();
            SQLiteFactory factory = (SQLiteFactory) DbProviderFactories.GetFactory("System.Data.SQLite");
            using (SQLiteConnection connection = (SQLiteConnection) factory.CreateConnection())
            {
                if (connection != null)
                {
                    SetConnectionString(baseName, connection);
                    connection.Open();

                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText =
                            $"Select imagev, imagem, comment, position, timestamp, routeName, id, username from RouteErrors where id = '{id}'";
                        try
                        {
                            IDataReader rdr = command.ExecuteReader();
                            try
                            {
                                while (rdr.Read())
                                {
                                    error.ImageV = (Byte[]) rdr[0];
                                    error.ImageM = (Byte[]) rdr[1];
                                    error.Comment = (String) rdr[2];
                                    error.Position = (String) rdr[3];
                                    error.TimeStamp = (String) rdr[4];
                                    error.RouteName = (String) rdr[5];
                                    error.Id = Convert.ToInt32(rdr[6]);
                                    if (rdr[7] is DBNull)
                                    {
                                        error.User = string.Empty;
                                    }
                                    else
                                    {
                                        error.User = (String) rdr[7];
                                    }
                                    
                                    
                                }
                            }
                            catch (Exception exc)
                            {
                                _log.Error(exc.Message);
                            }
                        }
                        catch (Exception ex)
                        {
                            _log.Error(ex.Message);
                            UpdateDbStructure(ex.Message, baseName);
                        }
                    }
                }
            }

            return error;
        }

        private void UpdateDbStructure(string exMessage, string baseName)
        {
            if (exMessage.Contains("username"))
            {
                SQLiteFactory factory = (SQLiteFactory) DbProviderFactories.GetFactory("System.Data.SQLite");
                using (SQLiteConnection connection = (SQLiteConnection) factory.CreateConnection())
                {
                    if (connection != null)
                    {
                        SetConnectionString(baseName, connection);
                        connection.Open();

                        using (SQLiteCommand command = new SQLiteCommand(connection))
                        {
                            command.CommandText = @"ALTER TABLE [RouteErrors] ADD COLUMN [username]  TEXT NULL;";
                            command.CommandType = CommandType.Text;
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public List<ErrorEntity> LoadErrors(string baseName = null)
        {
            List<ErrorEntity> errors = new List<ErrorEntity>();
            SQLiteFactory factory = (SQLiteFactory) DbProviderFactories.GetFactory("System.Data.SQLite");
            using (SQLiteConnection connection = (SQLiteConnection) factory.CreateConnection())
            {
                if (connection != null)
                {
                    SetConnectionString(baseName, connection);
                    connection.Open();

                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText =
                            $"Select imagev, imagem, comment, position, timestamp, routeName, id, username from RouteErrors";
                        try
                        {
                            IDataReader rdr = command.ExecuteReader();
                            try
                            {
                                while (rdr.Read())
                                {
                                    ErrorEntity error = new ErrorEntity
                                    {
                                        ImageV = (Byte[]) rdr[0],
                                        ImageM = (Byte[]) rdr[1],
                                        Comment = (String) rdr[2],
                                        Position = (String) rdr[3],
                                        TimeStamp = (String) rdr[4],
                                        RouteName = (String) rdr[5],
                                        Id = Convert.ToInt32(rdr[6])
                                    };
                                    if (rdr[7] is DBNull)
                                    {
                                        error.User = string.Empty;
                                    }
                                    else
                                    {
                                        error.User = (String) rdr[7];
                                    }
                                    errors.Add(error);
                                }
                            }
                            catch (Exception exc)
                            {
                                _log.Error(exc.Message);
                            }
                        }
                        catch (Exception ex)
                        {
                            _log.Error(ex.Message);
                        }
                    }

                    connection.Close();
                }
            }

            return errors;
        }

        private void SetConnectionString(string baseName, SQLiteConnection connection)
        {
            if (baseName == null)
            {
                connection.ConnectionString = "Data Source = " + _baseName;
            }
            else
            {
                connection.ConnectionString = "Data Source = " + $"{BaseDir}\\{baseName}.db3";
            }
        }
    }
}