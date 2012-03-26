﻿using System.Windows;

namespace watch_assistant.View.MainWindow
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new watch_assistant.ViewModel.MainWindow.MainWindowViewModel(this);
        }
    }
}
