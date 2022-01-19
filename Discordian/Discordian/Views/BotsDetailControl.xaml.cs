using System;

using Discordian.Core.Models;

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

        private static void OnListMenuItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as BotsDetailControl;
            control.ForegroundElement.ChangeView(0, 0, 1);
        }
    }
}
