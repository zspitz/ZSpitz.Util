using System;
using System.Diagnostics;
using System.Windows.Documents;
using System.Windows.Input;
using ZSpitz.Util;

namespace ZSpitz.Util.Wpf {
    public class LaunchUrlOrFileCommand : ICommand {
        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) {
            var (filename, args) = parameter switch {
                (string url1, string args1) => (url1, args1),
                Uri uri => (uri.ToString(), null),
                string urlPath => (urlPath, null),
                Hyperlink link => (link.NavigateUri.ToString(), null),
                _ => (null, null)
            };
            if (filename.IsNullOrWhitespace()) { return; }
            var psi = args switch {
                null => new ProcessStartInfo(filename),
                _ => new ProcessStartInfo(filename, args)
            };
            psi.UseShellExecute = true;
            Process.Start(psi);
        }

        public event EventHandler? CanExecuteChanged;
    }
}
