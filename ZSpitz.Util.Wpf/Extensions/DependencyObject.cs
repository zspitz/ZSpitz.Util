using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using static System.Linq.Enumerable;

namespace ZSpitz.Util.Wpf {
    public static class DependencyObjectExtensions {
        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject o) where T : DependencyObject {
            if (o == null) { yield break; }
            foreach (var child in Range(0, VisualTreeHelper.GetChildrenCount(o)).Select(index => VisualTreeHelper.GetChild(o, index))) {
                if (child is T childAsT) { yield return childAsT; }
                foreach (var descendant in FindVisualChildren<T>(child)) {
                    yield return descendant;
                }
            }
        }

        private static IEnumerable<T> FindLogicalChildrenBase<T>(DependencyObject o) where T : DependencyObject {
            if (o == null) { yield break; }
            foreach (var child in LogicalTreeHelper.GetChildren(o)) {
                if (child is T childAsT) { yield return childAsT; }
                if (child is DependencyObject d) {
                    foreach (var descendant in FindLogicalChildrenBase<T>(d)) {
                        yield return descendant;
                    }
                }
            }
        }
        public static IEnumerable<T> FindLogicalChildren<T>(this FrameworkElement fe) where T : DependencyObject => FindLogicalChildrenBase<T>(fe);
        public static IEnumerable<T> FindLogicalChildren<T>(this FrameworkContentElement fce) where T : DependencyObject => FindLogicalChildrenBase<T>(fce);

        /// <summary>Sets the value of the <paramref name="property"/> only if it hasn't been explicitly set.</summary>
        public static bool SetIfDefault<T>(this DependencyObject o, DependencyProperty property, T value) {
            if (DependencyPropertyHelper.GetValueSource(o, property).BaseValueSource == BaseValueSource.Default) {
                o.SetValue(property, value);
                return true;
            }
            return false;
        }
    }
}
