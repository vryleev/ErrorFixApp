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
        private readonly string _user = "Release";
        private string _baseName;

        private static string BaseNameToAdd;

        private Task _addTask = null;
        private Task _updateTask = null;

        

        private readonly ConcurrentQueue<ErrorEntity> _queueToAdd = new ConcurrentQueue<ErrorEntity>();
        private readonly ConcurrentQueue<ErrorEntity> _queueToUpdate = new ConcurrentQueue<ErrorEntity>();

        private readonly Logger _log =
             new LoggerConfiguration().
                 MinimumLevel.Debug().
                 WriteTo.Console().
                 WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day).CreateLogger();  
        public SqLiteManager(string userName)
        {
            if (userName != String.Empty)
            {
                _user = userName;
            }
            
            _baseName = $"{BaseDir}\\{DateTime.Today.Date:yy-MM-dd}-{_user}.db3";
            BaseNameToAdd = $"{BaseDir}\\{DateTime.Today.Date:yy-MM-dd}-{_user}.db3";

            Log.Information("SqLiteManage constructor");
            if (!Directory.Exists(BaseDir))
            {
                Directory.CreateDirectory(BaseDir);
            }

            //CreateDb();

            _addTask = Task.Factory.StartNew(CheckQueueToAdd);
            _updateTask = Task.Factory.StartNew(CheckQueueToUpdate);
            
           
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
                            [username]  TEXT NULL,
                            [errortype] TEXT NULL,
                            [priority] TEXT NULL
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
            return $"{DateTime.Today.Date:yy-MM-dd}-{_user}";
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
        
        public void UpdateErrorInDb(ErrorEntity error)
        {
            _queueToUpdate.Enqueue(error);
        }

        private void CheckQueueToAdd()
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
        
        private void CheckQueueToUpdate()
        {
            while (IsCheckQueue)
            {
                while (_queueToUpdate.TryDequeue(out var error))
                {
                    _log.Debug($"Queue count = {_queueToAdd.Count}");
                    UpdateEntity(error);
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
                            $"INSERT INTO [RouteErrors] " +
                            $"(imagev, imagem, comment, position, timestamp, routeName, username, errorType, priority) " +
                            $"VALUES (@0,@1,'{error.Comment}','{error.Position}','{error.TimeStamp}','{error.RouteName}'," +
                            $"'{error.User}', '{error.ErrorType}', '{error.Priority}');";
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
        
        private void UpdateEntity(ErrorEntity error, string baseName = null)
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
                        command.CommandText =
                            $"update [RouteErrors] " +
                            $"set imagev = @0, imagem=@1, comment='{error.Comment}', " +
                            $"position='{error.Position}',timestamp='{error.TimeStamp}',routeName='{error.RouteName}', "+
                            $"username='{error.User}',errorType='{error.ErrorType}',priority='{error.Priority}' " +
                            $"where id='{error.Id}'";
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

        public int DeleteErrorFromDb(int id, string baseName = null)
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
                        command.CommandText =
                            $"delete from [RouteErrors] where id = '{id}'";
                        
                        command.CommandType = CommandType.Text;
                        try
                        {
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            return 0;
                        }
                    }
                }
            }

            return 1;
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
        
        public int GetMaxId(string baseName = null)
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
                            command.CommandText = "SELECT * FROM [RouteErrors] ORDER BY id DESC LIMIT 1";
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
                            $"Select imagev, imagem, comment, position, timestamp, routeName, id, username, errortype, priority " +
                            $"from [RouteErrors] where id = '{id}'";
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
                                    if (rdr[8] is DBNull)
                                    {
                                        error.ErrorType = string.Empty;
                                    }
                                    else
                                    {
                                        error.ErrorType = (String) rdr[8];
                                    }
                                    if (rdr[9] is DBNull)
                                    {
                                        error.Priority = "Normal";
                                    }
                                    else
                                    {
                                        error.Priority = (String) rdr[9];
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
            if (exMessage.ToLower().Contains("username"))
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
            if (exMessage.ToLower().Contains("errortype"))
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
                            command.CommandText = @"ALTER TABLE [RouteErrors] ADD COLUMN [errortype]  TEXT NULL;";
                            command.CommandType = CommandType.Text;
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            if (exMessage.ToLower().Contains("priority"))
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
                            command.CommandText = @"ALTER TABLE [RouteErrors] ADD COLUMN [priority]  TEXT NULL;";
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
                            $"Select imagev, imagem, comment, position, timestamp, routeName, id, username, errortype, priority " +
                            $"from RouteErrors";
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
                                    if (rdr[8] is DBNull)
                                    {
                                        error.ErrorType = "base";
                                    }
                                    else
                                    {
                                        error.ErrorType = (String) rdr[8];
                                    }
                                    if (rdr[9] is DBNull)
                                    {
                                        error.Priority = "Normal";
                                    }
                                    else
                                    {
                                        error.Priority = (String) rdr[9];
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
                            UpdateDbStructure(ex.Message, baseName);
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

        public void StopTasks()
        {
            _addTask.Dispose();
            _updateTask.Dispose();
        }
    }
}