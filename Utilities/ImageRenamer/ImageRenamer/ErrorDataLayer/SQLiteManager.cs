using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Serilog.Core;

namespace ErrorDataLayer
{
    public static class SqLiteManager
    {
        private static readonly string BaseDir = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\RouteErrors";
        public static bool IsCheckQueue = true;
        public static string User = "Release";
        private static string _baseName;

        private static readonly string BaseNameToAdd;

        private static readonly Task AddTask;
        private static readonly Task UpdateTask;

        private static readonly CancellationTokenSource CancelTokenSource = new CancellationTokenSource();
        private static CancellationToken _token = CancelTokenSource.Token;
       

        

        private static readonly ConcurrentQueue<object> QueueToAdd = new ConcurrentQueue<object>();
        private static readonly ConcurrentQueue<ErrorEntity> QueueToUpdate = new ConcurrentQueue<ErrorEntity>();

        private static readonly Logger Log =
             new LoggerConfiguration().
                 MinimumLevel.Debug().
                 WriteTo.Console().
                 WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day).CreateLogger();  
        static SqLiteManager()
        {
            _baseName = $"{BaseDir}\\{DateTime.Today.Date:yy-MM-dd}-{User}.db3";
            BaseNameToAdd = $"{BaseDir}\\{DateTime.Today.Date:yy-MM-dd}-{User}.db3";

            Serilog.Log.Information("SqLiteManage constructor");
            if (!Directory.Exists(BaseDir))
            {
                Directory.CreateDirectory(BaseDir);
            }

            //CreateDb();

            AddTask = Task.Factory.StartNew(CheckQueueToAdd,_token);
            UpdateTask = Task.Factory.StartNew(CheckQueueToUpdate,_token);
            
           
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
                        using (SQLiteCommand command = new SQLiteCommand(connection))
                        {
                            command.CommandText = @"CREATE TABLE [ExportDates] (
                            [id] integer PRIMARY KEY AUTOINCREMENT NOT NULL,
                            [exportDate] TEXT NULL,
                            [user] TEXT NULL
                            );";
                            command.CommandType = CommandType.Text;
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public static string GetDbToAdd()
        {
            return $"{DateTime.Today.Date:yy-MM-dd}-{User}";
        }

        public static void SetBaseName(string baseName)
        {
            _baseName = $"{BaseDir}\\{baseName}.db3";
            CheckDb();
           
        }

        public static List<String> GetAvailableDb()
        {
            List<String> dbList = new List<string>();
            var files = Directory.GetFiles(BaseDir, "*.db3");
            foreach (var file in files)
            {
                dbList.Add(Path.GetFileNameWithoutExtension(file));
            }
            return dbList;
        }
        
        public static void AddErrorToDb(ErrorEntity error)
        {
            CreateDb();
            QueueToAdd.Enqueue(error);
        }
        public static void AddExportDateToDb(string baseName)
        {
           QueueToAdd.Enqueue(baseName);
        }
        
        public static void UpdateErrorInDb(ErrorEntity error)
        {
            QueueToUpdate.Enqueue(error);
        }

        private static void CheckQueueToAdd()
        {
            while (IsCheckQueue)
            {
                while (QueueToAdd.TryDequeue(out var obj))
                {
                    Log.Debug($"Queue count = {QueueToAdd.Count}");
                    if (obj is ErrorEntity e)
                    {
                        SaveErrorToDb(e);
                    }

                    if (obj is string str)
                    {
                        SaveExportDateToDb(str);
                    }
                }
                Thread.Sleep(100);
                
            }
        }
        
        private static void CheckQueueToUpdate()
        {
            while (IsCheckQueue)
            {
                while (QueueToUpdate.TryDequeue(out var error))
                {
                    Log.Debug($"Queue count = {QueueToAdd.Count}");
                    UpdateEntity(error);
                }
                Thread.Sleep(100);
                
            }
        }

        private static void SaveErrorToDb(ErrorEntity error)
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
                            Log.Error($"{ex.Message}");
                        }
                    }
                }
            }
        }
        private static void SaveExportDateToDb(string baseName = null)
        {
            CheckDb(baseName);
            string datetime = GetDbToAdd();
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
                            $"INSERT INTO [ExportDates] " +
                            $"(exportDate) " +
                            $"VALUES ('{datetime}');";
                      
                        command.CommandType = CommandType.Text;
                        try
                        {
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"{ex.Message}");
                        }
                    }
                }
            }
        }
        
        private static void UpdateEntity(ErrorEntity error, string baseName = null)
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
                            Log.Error($"{ex.Message}");
                        }
                    }
                }
            }
        }

        public static int DeleteErrorFromDb(int id, string baseName = null)
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
                           Log.Error($"{ex.Message}");
                            return 0;
                        }
                    }
                }
            }

            return 1;
        }

        public static int GetErrorCount(string baseName = null)
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
                            Log.Error(exc1.Message);
                        }
                    }
                }
            }
            return res;
        }
        
        public static int GetMaxId(string baseName = null)
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
                            Log.Error(exc1.Message);
                        }
                    }
                }
            }
            return res;
        }

        public static ErrorEntity LoadError(int id, string baseName = null)
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
                                Log.Error(exc.Message);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.Message);
                            
                        }
                    }
                }
            }

            return error;
        }

        private static void CheckDb(string baseName = null)
        {
           
            SQLiteFactory factory = (SQLiteFactory)DbProviderFactories.GetFactory("System.Data.SQLite");
            using (SQLiteConnection connection = (SQLiteConnection)factory.CreateConnection())
            {
                if (connection != null)
                {
                    SetConnectionString(baseName, connection);
                    connection.Open();

                    CheckRouteTableColumns(connection);
                    CheckExportTableExisting(connection);
                }
            }
        }

        private static void CheckRouteTableColumns(SQLiteConnection connection)
        {
            List<string> columns = new List<string>();
            using (SQLiteCommand command = new SQLiteCommand(connection))
            {
                command.CommandText =
                    $"PRAGMA table_info([RouteErrors])";
                try
                {
                    IDataReader rdr = command.ExecuteReader();
                    while (rdr.Read())
                    {
                        columns.Add((String)rdr[1]);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                }
            }

            if (columns.Find(x => x.Contains("username")) == null)
            {
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"ALTER TABLE [RouteErrors] ADD COLUMN [username]  TEXT NULL;";
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
            }

            if (columns.Find(x => x.Contains("errortype")) == null)
            {
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"ALTER TABLE [RouteErrors] ADD COLUMN [errortype]  TEXT NULL;";
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
            }

            if (columns.Find(x => x.Contains("priority")) == null)
            {
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"ALTER TABLE [RouteErrors] ADD COLUMN [priority]  TEXT NULL;";
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
            }
        }
        
        private static void CheckExportTableExisting(SQLiteConnection connection)
        {
            List<string> tables = new List<string>();
            using (SQLiteCommand command = new SQLiteCommand(connection))
            {
                command.CommandText =
                    $"PRAGMA table_list";
                try
                {
                    IDataReader rdr = command.ExecuteReader();
                    while (rdr.Read())
                    {
                        tables.Add((String)rdr[1]);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                }
            }

            if (tables.Find(x => x.Contains("ExportDates")) == null)
            {
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"CREATE TABLE [ExportDates] (
                            [id] integer PRIMARY KEY AUTOINCREMENT NOT NULL,
                            [exportDate] TEXT NULL,
                            [user] TEXT NULL
                            );";
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
            }
        }

        

        public static List<ErrorEntity> LoadErrors(string baseName = null)
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
                                Log.Error(exc.Message);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.Message);
                        }
                    }

                    connection.Close();
                }
            }

            return errors;
        }
        
        public static string GetLastExportDate(string baseName = null)
        {
            List<string> exportDates = new List<string>();
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
                            $"select exportDate " +
                            $"from ExportDates";
                        try
                        {
                            IDataReader rdr = command.ExecuteReader();
                            try
                            {
                                while (rdr.Read())
                                {
                                    exportDates.Add((String) rdr[0]);
                                }
                            }
                            catch (Exception exc)
                            {
                                Log.Error(exc.Message);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.Message);
                        }
                    }

                    connection.Close();
                }
            }

            return exportDates.LastOrDefault();
        }

        private static void SetConnectionString(string baseName, SQLiteConnection connection)
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

        public static void StopTasks()
        {
            CancelTokenSource.Cancel();
            CancelTokenSource.Dispose();
            
            //AddTask.Dispose();
            //UpdateTask.Dispose();
        }
    }
}