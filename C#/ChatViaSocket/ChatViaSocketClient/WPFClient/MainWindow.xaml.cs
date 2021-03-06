﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ChatViaSocketClient;

namespace WPFClient
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartTestButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ThreadPool.QueueUserWorkItem((x) =>
                {
                    Program.Start(null);
                });
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
        }

        private void StopTestButton_OnClick(object sender, RoutedEventArgs e)
        {
            Program.Stop();
        }
    }
}
