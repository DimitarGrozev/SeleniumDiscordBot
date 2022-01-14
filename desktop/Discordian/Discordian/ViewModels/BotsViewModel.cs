using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Discordian.Core.Models;
using Discordian.Core.Services;

using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Uwp.UI.Controls;

namespace Discordian.ViewModels
{
    public class BotsViewModel : ObservableObject
    {
        private Bot _selected;

        public Bot Selected
        {
            get { return _selected; }
            set { SetProperty(ref _selected, value); }
        }

        public ObservableCollection<Bot> SampleItems { get; private set; } = new ObservableCollection<Bot>();

        public BotsViewModel()
        {
        }

        public async Task LoadDataAsync(ListDetailsViewState viewState)
        {
            SampleItems.Clear();

            var data = await SampleDataService.GetBotList();

            foreach (var item in data)
            {
                SampleItems.Add(item);
            }

            if (viewState == ListDetailsViewState.Both)
            {
                Selected = SampleItems.First();
            }
        }
    }
}
