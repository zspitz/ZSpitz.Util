using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using static System.Windows.Visibility;
using static ZSpitz.Util.Functions;

namespace ZSpitz.Util.Wpf {
    public abstract class ReadOnlyConverterBase : IValueConverter {
        protected readonly object UnsetValue = DependencyProperty.UnsetValue;
        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => UnsetValue;
    }
    public abstract class ReadOnlyMultiConverterBase : IMultiValueConverter {
        protected readonly object UnsetValue = DependencyProperty.UnsetValue;
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

    public class MultiVisibilityConverter : ReadOnlyMultiConverterBase {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) =>
            values.Cast<Visibility>().All(x => x == Visible) ? Visible : Collapsed;
    }

    public class TruthyVisibilityConverter : ReadOnlyConverterBase {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value switch {
                string s => !s.IsNullOrWhitespace(),
                Uri => true,
                bool b => b,
                null => false,
                _ when value.GetType().UnderlyingIfNullable().IsNumeric() => ((dynamic)value) == 0,
                _ => throw new NotImplementedException();
            } ? Visible : Collapsed;
    }
}
