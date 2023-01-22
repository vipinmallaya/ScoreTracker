using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoreTracker.ViewModel
{
    [INotifyPropertyChanged]
    public partial class BaseViewModel : IQueryAttributable
    {
        public AsyncRelayCommand PageAppearingCommand { get; }
        public AsyncRelayCommand PageDisAppearingCommand { get; }

        public BaseViewModel()
        {

            PageAppearingCommand = new AsyncRelayCommand(PageAppearingAction);
            PageDisAppearingCommand = new AsyncRelayCommand(PageDisAppearingAction);
        }

        protected virtual Task PageDisAppearingAction()
        {
            return Task.CompletedTask;
        }

        protected virtual Task PageAppearingAction()
        {
            return Task.CompletedTask;
        }

        public virtual void ApplyQueryAttributes(IDictionary<string, object> query)
        {

        }
    }
}
