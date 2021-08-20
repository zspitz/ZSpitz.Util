using System.Windows;
using System.Windows.Controls;
using static System.Windows.FrameworkPropertyMetadataOptions;
using static ZSpitz.Util.Wpf.Functions;

namespace ZSpitz.Util.Wpf {
    public class BindableSelectionTextBox : TextBox {
        public static readonly DependencyProperty BindableSelectionStartProperty =
            DPRegister<int, BindableSelectionTextBox>(0, BindsTwoWayByDefault, (textBox, newValue, oldValue) => {
                if (!textBox.changeFromUI) {
                    textBox.SelectionStart = newValue;
                } else {
                    textBox.changeFromUI = false;
                }
            });

        public static readonly DependencyProperty BindableSelectionLengthProperty =
            DPRegister<int, BindableSelectionTextBox>(0, BindsTwoWayByDefault, (textBox, newValue, oldValue) => {
                if (!textBox.changeFromUI) {
                    textBox.SelectionLength = newValue;
                } else {
                    textBox.changeFromUI = false;
                }
            });

        private bool changeFromUI;

        public BindableSelectionTextBox() : base() => SelectionChanged += onSelectionChanged;

        public int BindableSelectionStart {
            get => (int)GetValue(BindableSelectionStartProperty);
            set => SetValue(BindableSelectionStartProperty, value);
        }

        public int BindableSelectionLength {
            get => (int)GetValue(BindableSelectionLengthProperty);
            set => SetValue(BindableSelectionLengthProperty, value);
        }

        private void onSelectionChanged(object sender, RoutedEventArgs e) {
            if (BindableSelectionStart != SelectionStart) {
                changeFromUI = true;
                BindableSelectionStart = SelectionStart;
            }

            if (BindableSelectionLength != SelectionLength) {
                changeFromUI = true;
                BindableSelectionLength = SelectionLength;
            }
        }
    }
}
