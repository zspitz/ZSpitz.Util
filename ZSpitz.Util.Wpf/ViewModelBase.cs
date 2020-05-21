using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using static ZSpitz.Util.Functions;

namespace ZSpitz.Util.Wpf {
    public abstract class ViewModelBase<TModel> : INotifyPropertyChanged {
        public TModel Model { get; }

        protected ViewModelBase(TModel model) => Model = model;

        public event PropertyChangedEventHandler? PropertyChanged;

        private void Invoke(string? name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        /// <summary>Raises change notification for fields defined in the current class</summary>
        protected void NotifyChanged<T>(ref T current, T newValue, [CallerMemberName] string? name = null) {
            if (IsEqual(current, newValue)) { return; }
            current = newValue;
            Invoke(name);
        }

        /// <summary>
        /// Raises change notification for fields not defined in the current class (e.g. the model class)
        /// because fields defined in another class cannot be passed with the ref modifier
        /// </summary>
        protected void NotifyChanged<T>(T current, T newValue, Action? setter = null, [CallerMemberName] string? name = null) {
            if (IsEqual(current, newValue)) { return; }
            setter?.Invoke();
            Invoke(name);
        }
    }
}
