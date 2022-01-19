using System;

using Discordian.ViewModels;

using Windows.UI.Xaml.Controls;

namespace Discordian.Views
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; } = new MainViewModel();

        public MainPage()
        {
            InitializeComponent();
        }
    }
}
