using System;
using System.Diagnostics;
using System.Windows.Input;
using ZSpitz.Util;

namespace ZSpitz.Util.Wpf {
    public class LaunchUrlOrFileCommand : ICommand {
        public bool CanExecute(object parameter) =>
            parameter switch {
                Uri _ => true,
                string _ => true,
                _ => false
            };

        public void Execute(object parameter) {
            var psi = parameter switch {
                Uri uri => new ProcessStartInfo(uri.ToString()),
                string urlPath => new ProcessStartInfo(urlPath),
                (string url, string args) => new ProcessStartInfo(url, args),
                _ => null
            };
            if (psi is null || psi.FileName.IsNullOrWhitespace()) { return; }
            psi.UseShellExecute = true;
            Process.Start(psi);
        }

        public event EventHandler? CanExecuteChanged;
    }
}
