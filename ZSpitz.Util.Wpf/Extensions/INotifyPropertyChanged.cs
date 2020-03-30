using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ZSpitz.Util.Wpf.Core.Extensions {
    public static class INotifyPropertyChangedExtensions {
        // https://stackoverflow.com/a/60668668/111794
        public static void NotifyChanged<T>(
                this INotifyPropertyChanged inpc,
                ref T current, T newValue,
                PropertyChangedEventHandler handler,
                [CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(current, newValue)) { return; }
            current = newValue;
            handler?.Invoke(inpc, new PropertyChangedEventArgs(name));
        }
    }
}
