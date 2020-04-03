using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ZSpitz.Util;

namespace ZSpitz.Util.Wpf {
    public abstract class ViewModelBase<TModel> : INotifyPropertyChanged {
        public TModel Model { get; }

        protected ViewModelBase(TModel model, IEnumerable<KeyValuePair<string, RelayCommand>>? commands = null) {
            Model = model;

            if (commands is null) { return; }

            if (!(commands is Dictionary<string, RelayCommand>)) {
                var lookup = commands.ToLookup();
                var duplicates = lookup.FirstOrDefault(x => x.Count() > 1);
                if (duplicates is { }) {
                    throw new InvalidOperationException($"Multiple commands for name '{duplicates.Key}'.");
                }
            }

            commands.AddRangeTo(this.commands);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private bool IsEqual<T>(T current, T newValue) => EqualityComparer<T>.Default.Equals(current, newValue);
        private void Invoke(string? name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        /// <summary>Raises change notification for fields defined in the current class</summary>
        protected void NotifyChanged<T>(ref T current, T newValue, [CallerMemberName] string? name = null) {
            if (IsEqual(current, newValue)) { return; }
            current = newValue;
            Invoke(name);
        }

        /// <summary>Raises change notification for fields not defined in the current class (e.g. the model class)</summary>
        protected void NotifyChanged<T>(T current, T newValue, Action? setter = null, [CallerMemberName] string? name = null) {
            if (IsEqual(current, newValue)) { return; }
            setter?.Invoke();
            Invoke(name);
        }

        private Dictionary<string, RelayCommand> commands = new Dictionary<string, RelayCommand>();

        protected void SetCommand(string commandName, RelayCommand command) {
            if (commandName is null) { throw new ArgumentNullException("commandName"); }
            var current = GetCommand(commandName);
            NotifyChanged(current, command, () => commands[commandName] = command);
        }
        protected RelayCommand? GetCommand(string commandName) {
            commands.TryGetValue(commandName, out RelayCommand? command);
            return command;
        }
    }
}
