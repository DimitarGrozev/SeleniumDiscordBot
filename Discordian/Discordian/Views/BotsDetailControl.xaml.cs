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
            get
            {
                var bot = GetValue(ListMenuItemProperty) as Bot;

                if (bot != null)
                {
                    ProgressSpinnerForWeeklyMessages.IsActive = true;
                    var statistics = new List<Data>();

                    new Task( async () =>
                    {
                       statistics = await DiscordApiClient.GetBotMessageCountForLastSevenDaysAsync(bot.DiscordData, bot.Credentials.Token);
                    }).Start();

                    this.ChartContext = statistics;
                    ProgressSpinnerForWeeklyMessages.IsActive = false;
                }

                return bot;
            }
            set { SetValue(ListMenuItemProperty, value); }
        }

        public static readonly DependencyProperty ListMenuItemProperty = DependencyProperty.Register("ListMenuItem", typeof(Bot), typeof(BotsDetailControl), new PropertyMetadata(null, OnListMenuItemPropertyChanged));

        public List<Data> ChartContext
        {
            get { return GetValue(ChartContextProperty) as List<Data>; }
            set { SetValue(ChartContextProperty, value); }
        }

        public static readonly DependencyProperty ChartContextProperty = DependencyProperty.Register("ChartContext", typeof(List<Data>), typeof(BotsDetailControl), new PropertyMetadata(null, OnListMenuItemPropertyChanged));

        public BotsDetailControl()
        {
            InitializeComponent();
        }

        private static void OnListMenuItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as BotsDetailControl;
            control.ForegroundElement.ChangeView(0, 0, 1);
        }
    }
}
