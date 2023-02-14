using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Net.Mime;
using System.Reflection;

namespace ErrorDataLayer
{
    public class SqLiteManager
    {
        public static readonly string BaseDir = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\RouteErrors";

        private string _baseName = $"{BaseDir}\\{DateTime.Today.Date.ToString("dd_MM_yy")}_RouteErrors.db3";

        static readonly string BaseNameToAdd = $"{BaseDir}\\{DateTime.Today.Date.ToString("dd_MM_yy")}_RouteErrors.db3";

        public SqLiteManager()
        {
            if (!Directory.Exists(BaseDir))
            {
                Directory.CreateDirectory(BaseDir);
            }

            if (!File.Exists(BaseNameToAdd))
            {
                SQLiteConnection.CreateFile(BaseNameToAdd);
                SQLiteFactory factory = (SQLiteFactory) DbProviderFactories.GetFactory("System.Data.SQLite");
                using (SQLiteConnection connection = (SQLiteConnection) factory.CreateConnection())
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
            return $"{DateTime.Today.Date.ToString("dd_MM_yy")}_RouteErrors";
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
                            int res = command.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            UpdateDBStructure(ex.Message, Path.GetFileNameWithoutExtension(BaseNameToAdd));
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
                            //MessageBox.Show(exc1.Message);
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
                                    if (rdr[7].GetType() == typeof(System.DBNull))
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
                                //MessageBox.Show(exc.Message);
                            }
                        }
                        catch (Exception ex)
                        {
                            UpdateDBStructure(ex.Message, baseName);
                        }
                    }
                }
            }

            return error;
        }

        private void UpdateDBStructure(string exMessage, string baseName)
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
                                    ErrorEntity error = new ErrorEntity();
                                    error.ImageV = (Byte[]) rdr[0];
                                    error.ImageM = (Byte[]) rdr[1];
                                    error.Comment = (String) rdr[2];
                                    error.Position = (String) rdr[3];
                                    error.TimeStamp = (String) rdr[4];
                                    error.RouteName = (String) rdr[5];
                                    error.Id = Convert.ToInt32(rdr[6]);
                                    if (rdr[7].GetType() == typeof(System.DBNull))
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
                                //MessageBox.Show(exc.Message);
                            }
                        }
                        catch (Exception ex)
                        {
                            //MessageBox.Show(ex.Message);
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