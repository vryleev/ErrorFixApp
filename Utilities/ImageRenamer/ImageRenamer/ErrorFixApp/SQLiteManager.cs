using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using ErrorFixApp.Properties;

namespace ErrorFixApp
{
    public class SqLiteManager
    {
        static readonly string BaseDir = $"{Directory.GetCurrentDirectory()}\\RouteErrors";    
        
        private readonly string _baseName = $"{BaseDir}\\{DateTime.Today.Date.ToString("dd_MM_yy")}_RouteErrors.db3";
        
        public SqLiteManager()
        {
            
            if (!Directory.Exists(BaseDir))
            {
                Directory.CreateDirectory(BaseDir);
            }

            if (!File.Exists(_baseName))
            {
               SQLiteConnection.CreateFile(_baseName);
               SQLiteFactory factory = (SQLiteFactory)DbProviderFactories.GetFactory("System.Data.SQLite");
               using (SQLiteConnection connection = (SQLiteConnection)factory.CreateConnection())
               {
                   if (connection != null)
                   {
                       connection.ConnectionString = "Data Source = " + _baseName;
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
                    [routeName] TEXT NOT NULL                    
                    );";
                           command.CommandType = CommandType.Text;
                           command.ExecuteNonQuery();
                       }
                   }
               }
            }
        }

        public void AddErrorToDb(ErrorDetail error)
        {
            SQLiteFactory factory = (SQLiteFactory)DbProviderFactories.GetFactory("System.Data.SQLite");
            using (SQLiteConnection connection = (SQLiteConnection)factory.CreateConnection())
            {
                if (connection != null)
                {
                    connection.ConnectionString = "Data Source = " + _baseName;
                    connection.Open();

                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText =
                            $"INSERT INTO RouteErrors (imagev, imagem, comment, position, timestamp, routeName) VALUES (@0,@1,'{error.Comment}','{error.Position}','{error.TimeStamp}','{error.RouteName}');";
                        SQLiteParameter param0 = new SQLiteParameter("@0", DbType.Binary)
                        {
                            Value = ImageUtils.ImageToByte(error.ImageV, ImageFormat.Jpeg)
                        };
                        command.Parameters.Add(param0);

                        SQLiteParameter param1 = new SQLiteParameter("@1", DbType.Binary)
                        {
                            Value = ImageUtils.ImageToByte(error.ImageM, ImageFormat.Jpeg)
                        };
                        command.Parameters.Add(param1);

                        command.CommandType = CommandType.Text;
                        try
                        {
                            int res = command.ExecuteNonQuery();

                            command.CommandText = "SELECT count(*) FROM [RouteErrors]";
                            object count = command.ExecuteScalar();
                            MessageBox.Show($"{Resources.AddedErrors}: {res}, {Resources.TotalErrors}: {count}");
                        }
                        catch (Exception exc1)
                        {
                            MessageBox.Show(exc1.Message);
                        }
                    }
                }
            }
        }
        
        public int GetErrorCount(string baseName)
        {
            int res = -1;
            SQLiteFactory factory = (SQLiteFactory)DbProviderFactories.GetFactory("System.Data.SQLite");
            using (SQLiteConnection connection = (SQLiteConnection)factory.CreateConnection())
            {
                if (connection != null)
                {
                    connection.ConnectionString = "Data Source = " + baseName;
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
                            MessageBox.Show(exc1.Message);
                        }
                    }
                }
            }

            return res;
        }
        
        public void LoadError(ErrorDetail error, string baseName, int id)
        {
            SQLiteFactory factory = (SQLiteFactory)DbProviderFactories.GetFactory("System.Data.SQLite");
            using (SQLiteConnection connection = (SQLiteConnection)factory.CreateConnection())
            {
                if (connection != null)
                {
                    connection.ConnectionString = "Data Source = " + baseName;
                    connection.Open();

                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText =
                            $"Select imagev, imagem, comment, position, timestamp, routeName, id from RouteErrors where id = '{id}'";
                        try
                        {
                            IDataReader rdr = command.ExecuteReader();
                            try
                            {
                                while (rdr.Read())
                                {
                                    byte[] a = (Byte[])rdr[0];
                                    error.BImageV = ImageUtils.BitmapToImageSource((Bitmap)ImageUtils.ByteToImage(a));
                                    byte[] b = (Byte[])rdr[1];
                                    error.BImageM = ImageUtils.BitmapToImageSource((Bitmap)ImageUtils.ByteToImage(b));
                                    error.Comment = (String)rdr[2];
                                    error.Position = (String)rdr[3];
                                    error.TimeStamp = (String)rdr[4];
                                    error.RouteName = (String)rdr[5];
                                    error.Id = Convert.ToInt32(rdr[6]);
                                }
                            }
                            catch (Exception exc)
                            {
                                MessageBox.Show(exc.Message);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                }
            }
            
            
        }   
        
        public List<ErrorDetail> LoadErrors(string baseName)
        {
            List<ErrorDetail> errors = new List<ErrorDetail>();
            SQLiteFactory factory = (SQLiteFactory)DbProviderFactories.GetFactory("System.Data.SQLite");
            using (SQLiteConnection connection = (SQLiteConnection)factory.CreateConnection())
            {
                if (connection != null)
                {
                    connection.ConnectionString = "Data Source = " + baseName;
                    connection.Open();

                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText =
                            $"Select imagev, imagem, comment, position, timestamp, routeName, id from RouteErrors";
                        try
                        {
                            IDataReader rdr = command.ExecuteReader();
                            try
                            {
                                while (rdr.Read())
                                {
                                    ErrorDetail error = new ErrorDetail();
                                    byte[] a = (Byte[])rdr[0];
                                    error.ImageV = ImageUtils.ByteToImage(a);
                                    byte[] b = (Byte[])rdr[1];
                                    error.ImageM = ImageUtils.ByteToImage(b);
                                    error.Comment = (String)rdr[2];
                                    error.Position = (String)rdr[3];
                                    error.TimeStamp = (String)rdr[4];
                                    error.RouteName = (String)rdr[5];
                                    error.Id = Convert.ToInt32(rdr[6]);
                                    errors.Add(error);
                                }
                            }
                            catch (Exception exc)
                            {
                                MessageBox.Show(exc.Message);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }

                    connection.Close();
                }
            }

            return errors;


        }   
    }
}