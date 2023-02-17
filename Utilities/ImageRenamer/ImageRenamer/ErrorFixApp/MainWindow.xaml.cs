﻿using System.ComponentModel;
using System.Windows.Input;

namespace ErrorFixApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            ((MainWindowModel)this.DataContext).ClosingDb();
        }
    }
}