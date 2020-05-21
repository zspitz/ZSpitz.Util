using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;
using static System.Windows.DependencyProperty;
using static System.Windows.Visibility;

namespace ZSpitz.Util.Wpf {
    public abstract class ReadOnlyConverterBase : IValueConverter {
        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => UnsetValue;
    }
    public abstract class ReadOnlyMultiConverterBase : IMultiValueConverter {
        public abstract object Convert(object[] values, Type targetType, object parameter, CultureInfo culture);
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => new[] { UnsetValue };
    }

    public class AnyVisibilityConverter : ReadOnlyConverterBase {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (((IEnumerable)value).Any()) { return Visible; }
            return Collapsed;
        }
    }

    [Obsolete("Use VisibilityConverter with MatchValue set to null and Invert set to true")]
    public class NotNullToVisibilityConverter : ReadOnlyConverterBase {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is null ? Collapsed : Visible;
    }

    public class VisibilityConverter : ReadOnlyConverterBase {
        public object MatchValue { get; set; } = true;
        public bool Invert { get; set; } = false;
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            Invert ?
                (Equals(value, MatchValue) ? Collapsed : Visible) :
                (Equals(value, MatchValue) ? Visible : Collapsed);
    }
}
