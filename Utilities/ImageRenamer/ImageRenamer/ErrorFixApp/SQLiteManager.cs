using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows;

namespace ErrorFixApp
{
    public class SQLiteManager
    {
        readonly static string baseDir = $"{Directory.GetCurrentDirectory()}\\RouteErrors";    
        
        private string _baseName = $"{baseDir}\\{DateTime.Today.Date.ToString("dd_MM_yy")}_RouteErrors.db3";
        
        public SQLiteManager()
        {
            
            if (!Directory.Exists(baseDir))
            {
                Directory.CreateDirectory(baseDir);
            }

            if (!File.Exists(_baseName))
            {
               SQLiteConnection.CreateFile(_baseName);
               SQLiteFactory factory = (SQLiteFactory)DbProviderFactories.GetFactory("System.Data.SQLite");
               using (SQLiteConnection connection = (SQLiteConnection)factory.CreateConnection())
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

        public void AddErrorToDB(ErrorDetail error)
        {
            SQLiteFactory factory = (SQLiteFactory)DbProviderFactories.GetFactory("System.Data.SQLite");
            using (SQLiteConnection connection = (SQLiteConnection)factory.CreateConnection())
            {
                connection.ConnectionString = "Data Source = " + _baseName;
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = $"INSERT INTO RouteErrors (imagev, imagem, comment, position, timestamp, routeName) VALUES (@0,@1,'{error.Comment}','{error.Position}','{error.TimeStamp}','{error.RouteName}');";
                    SQLiteParameter param0 = new SQLiteParameter("@0", System.Data.DbType.Binary);
                    param0.Value = ImageUtils.ImageToByte(error.ImageV,ImageFormat.Jpeg);
                    command.Parameters.Add(param0);
                    
                    SQLiteParameter param1 = new SQLiteParameter("@1", System.Data.DbType.Binary);
                    param1.Value = ImageUtils.ImageToByte(error.ImageM,ImageFormat.Jpeg);
                    command.Parameters.Add(param1);
                    
                    // SQLiteParameter param2 = new SQLiteParameter("@2", System.Data.DbType.);
                    // param2.Value = Encoding.UTF8.GetBytes(error.Comment);
                    // command.Parameters.Add(param2);
                    
                    command.CommandType = CommandType.Text;
                    try{
                        int res = command.ExecuteNonQuery();
                        
                        command.CommandText = "SELECT count(*) FROM [RouteErrors]";
                        object count = command.ExecuteScalar();
                        MessageBox.Show($"Добавлено ошибок: {res}, всего ошибок: {count}");
                    }
                    catch (Exception exc1){
                        MessageBox.Show(exc1.Message);
                    }
                }
            }
        }
        
        public void LoadError(ErrorDetail error, string baseName, int id)
        {
            SQLiteFactory factory = (SQLiteFactory)DbProviderFactories.GetFactory("System.Data.SQLite");
            using (SQLiteConnection connection = (SQLiteConnection)factory.CreateConnection())
            {
                connection.ConnectionString = "Data Source = " + baseName;
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = $"Select imagev, imagem, comment, position, timestamp, routeName, id from RouteErrors where id = '{id}'";
                    try
                    {
                        IDataReader rdr = command.ExecuteReader();
                        try
                        {
                            while (rdr.Read())
                            {
                                byte[] a = (System.Byte[])rdr[0];
                                error.BImageV = ImageUtils.BitmapToImageSource((Bitmap)ImageUtils.ByteToImage(a));
                                byte[] b = (System.Byte[])rdr[1];
                                error.BImageM = ImageUtils.BitmapToImageSource((Bitmap)ImageUtils.ByteToImage(b));
                                error.Comment = (String)rdr[2];
                                error.Position = (String)rdr[3];
                                error.TimeStamp = (String)rdr[4];
                                error.RouteName = (String)rdr[5];
                                error.Id = Convert.ToInt32(rdr[6]);
                            }
                        }
                        catch (Exception exc) { MessageBox.Show(exc.Message); }
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message); }
                }
            }
            
            
        }   
        
        public List<ErrorDetail> LoadErrors(string baseName)
        {
            List<ErrorDetail> errors = new List<ErrorDetail>();
            SQLiteFactory factory = (SQLiteFactory)DbProviderFactories.GetFactory("System.Data.SQLite");
            using (SQLiteConnection connection = (SQLiteConnection)factory.CreateConnection())
            {
                connection.ConnectionString = "Data Source = " + baseName;
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = $"Select imagev, imagem, comment, position, timestamp, routeName, id from RouteErrors";
                    try
                    {
                        IDataReader rdr = command.ExecuteReader();
                        try
                        {
                            while (rdr.Read())
                            {
                                ErrorDetail error = new ErrorDetail();
                                byte[] a = (System.Byte[])rdr[0];
                                error.ImageV = ImageUtils.ByteToImage(a);
                                byte[] b = (System.Byte[])rdr[1];
                                error.ImageM = ImageUtils.ByteToImage(b);
                                error.Comment = (String)rdr[2];
                                error.Position = (String)rdr[3];
                                error.TimeStamp = (String)rdr[4];
                                error.RouteName = (String)rdr[5];
                                error.Id = Convert.ToInt32(rdr[6]);
                                errors.Add(error);
                            }
                        }
                        catch (Exception exc) { MessageBox.Show(exc.Message); }
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message); }
                }
                connection.Close();
            }

            return errors;


        }   
    }
}