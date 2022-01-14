using System;

using Discordian.ViewModels;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Discordian.Views
{
    public sealed partial class BotsPage : Page
    {
        public BotsViewModel ViewModel { get; } = new BotsViewModel();

        public BotsPage()
        {
            InitializeComponent();
            Loaded += BotsPage_Loaded;
        }

        private async void BotsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadDataAsync(ListDetailsViewControl.ViewState);
        }

        private async void CreateBotButton_Click(object sender, RoutedEventArgs e)
        {
            var input = new TextBox();
            var input2 = new TextBox();

            var dialogContent = new StackPanel();
            dialogContent.Children.Add(input);
            dialogContent.Children.Add(input2);

            ContentDialog createDialog = new ContentDialog
            {
                Title = "Create new bot",
                Content = dialogContent,
                CloseButtonText = "Ok"
            };

            ContentDialogResult result = await createDialog.ShowAsync();
        }
    }
}
