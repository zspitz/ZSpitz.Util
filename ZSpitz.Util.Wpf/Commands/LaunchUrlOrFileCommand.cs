using System;
using System.Diagnostics;
using System.Windows.Documents;
using System.Windows.Input;
using ZSpitz.Util;

namespace ZSpitz.Util.Wpf {
    public class LaunchUrlOrFileCommand : ICommand {
        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) {
            ProcessStartInfo psi;
            if (parameter is (string url, string args)) {
                psi = new ProcessStartInfo(url, args);
            } else {
                psi = new ProcessStartInfo(parameter switch {
                    Uri uri => uri.ToString(),
                    string urlPath => urlPath,
                    Hyperlink link => link.NavigateUri.ToString(),
                    _ => null
                });
            }
            if (psi is null || psi.FileName.IsNullOrWhitespace()) { return; }
            psi.UseShellExecute = true;
            Process.Start(psi);
        }

        public event EventHandler? CanExecuteChanged;
    }
}
