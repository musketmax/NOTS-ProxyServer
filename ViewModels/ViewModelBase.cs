using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProxyServer_NOTS.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(T currentValue, T newValue, Action DoSet, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(currentValue, newValue))
            {
                DoSet.Invoke();
                NotifyPropertyChanged(propertyName);
                return true;
            }

            return false;
        }

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (propertyName != null)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
