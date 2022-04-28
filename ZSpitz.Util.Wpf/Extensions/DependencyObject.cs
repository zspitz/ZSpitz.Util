using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using static System.Linq.Enumerable;
using static System.Windows.Media.VisualTreeHelper;

namespace ZSpitz.Util.Wpf {
    public static class DependencyObjectExtensions {
        // https://stackoverflow.com/a/2012298/111794
        // The mentioned caveat about increased memory for the queue doesn't apply here;
        // the visual tree objects referenced in the queue are created/destroyed/referenced independtly of the queue
        // by the visual tree
        public static IEnumerable<DependencyObject> VisualChildren(this DependencyObject? root, bool andSelf = false) {
            if (root is null) { yield break; }

            var roots =
                andSelf ?
                    Repeat(root, 1) :
                    Range(0, GetChildrenCount(root))
                        .Select(i => GetChild(root, i));

            Queue<DependencyObject> toProcess = new(roots);
            while (toProcess.Count > 0) {
                var item = toProcess.Dequeue();
                yield return item;
                var count = GetChildrenCount(item);
                for (var i = 0; i < count; i++) {
                    toProcess.Enqueue(GetChild(item, i));
                }
            }
        }

        public static IEnumerable<T> VisualChildren<T>(this DependencyObject? o) where T : DependencyObject =>
            VisualChildren(o).OfType<T>();

        [Obsolete("Use VisualChildren")]
        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject o) where T : DependencyObject {
            if (o == null) { yield break; }
            foreach (var child in Range(0, GetChildrenCount(o)).Select(index => GetChild(o, index))) {
                if (child is T childAsT) { yield return childAsT; }
                foreach (var descendant in FindVisualChildren<T>(child)) {
                    yield return descendant;
                }
            }
        }

        public static IEnumerable<object?> LogicalChildren(this DependencyObject root, bool andSelf = false) {
            if (root is null) { yield break; }

            var roots = (
                andSelf ?
                    Repeat(root, 1) :
                    LogicalTreeHelper.GetChildren(root)
            ).Cast<object>();

            Queue<object?> toProcess = new(roots);
            while (toProcess.Count > 0) {
                var item = toProcess.Dequeue();
                yield return item;
                if (item is DependencyObject o) {
                    foreach (var child in LogicalTreeHelper.GetChildren(o)) {
                        toProcess.Enqueue(child);
                    }
                }
            }
        }

        public static IEnumerable<T> LogicalChildren<T>(this DependencyObject root, bool andSelf = false) =>
            root.LogicalChildren(andSelf).OfType<T>();

        [Obsolete("Use LogicalChildren")]
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
        [Obsolete("Use LogicalChildren")] public static IEnumerable<T> FindLogicalChildren<T>(this FrameworkElement fe) where T : DependencyObject => FindLogicalChildrenBase<T>(fe);
        [Obsolete("Use LogicalChildren")] public static IEnumerable<T> FindLogicalChildren<T>(this FrameworkContentElement fce) where T : DependencyObject => FindLogicalChildrenBase<T>(fce);

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
