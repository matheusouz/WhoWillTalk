using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WhoWillTalk {
    public abstract class BaseViewModel : INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isBusy;
        public bool IsBusy {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public void SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action onChanged = null) {
            if (EqualityComparer<T>.Default.Equals(backingStore, value)) {
                return;
            }

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}