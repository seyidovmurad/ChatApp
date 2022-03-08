using ChatClient.Services;
using ChatClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ChatClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var service = new ChatService();

            ChatViewModel chatViewModel = new ChatViewModel(service);

            MainWindow window = new MainWindow
            {
                DataContext = new MainViewModel(chatViewModel)
            };
            window.Show();
        }
    }
}
