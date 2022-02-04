using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discordian.Core.Models;
using Discordian.Core.Models.Charts;
using Discordian.Services;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Discordian.Views
{
    public sealed partial class BotsDetailControl : UserControl
    {
        public Bot ListMenuItem
        {
            get { return GetValue(ListMenuItemProperty) as Bot; }
            set { SetValue(ListMenuItemProperty, value); }
        }

        public static readonly DependencyProperty ListMenuItemProperty = DependencyProperty.Register("ListMenuItem", typeof(Bot), typeof(BotsDetailControl), new PropertyMetadata(null, OnListMenuItemPropertyChanged));

        public BotsDetailControl()
        {
            InitializeComponent();
        }

        public void StartLoader()
        {
            this.ProgressSpinnerForWeeklyMessages.IsActive = true;
        }

        public void StopLoader()
        {
            this.ProgressSpinnerForWeeklyMessages.IsActive = false;
        }

        private async static void OnListMenuItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            var control = d as BotsDetailControl;

            if (control.ListMenuItem != null)
            {
                control.barSeries.DataContext = new List<Data>();
                control.StartLoader();
                var data = await DiscordApiClient.GetBotMessageCountForLastSevenDaysAsync(control.ListMenuItem.DiscordData, control.ListMenuItem.Credentials.Token);
                control.barSeries.DataContext = data;
                control.ForegroundElement.ChangeView(0, 0, 1);
                control.StopLoader();
            }
        }
    }
}
