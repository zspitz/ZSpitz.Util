using System;
using System.Linq;
using System.Windows.Data;

namespace ZSpitz.Util.Wpf {
    public static class BindigBaseExtensions {
        public static string? Path(this BindingBase bindingBase) =>
            bindingBase switch {
                Binding binding => binding.Path.Path,
                MultiBinding multiBinding => multiBinding.Bindings.FirstOrDefault()?.Path(),
                PriorityBinding priorityBinding => priorityBinding.Bindings.FirstOrDefault()?.Path(),
                _ => throw new InvalidOperationException()
            };
    }
}
