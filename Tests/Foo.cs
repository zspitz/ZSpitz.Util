using System;

// This class only exists to test rendering of MemberInfo
#pragma warning disable
namespace Tests {
    internal class Foo {
        private Foo() { }
        private Foo(string prm1) { }
        static Foo() { }

        private string? FooField;
        private event EventHandler? FooEvent;
        private string? FooProperty { get; set; }
        private string FooMethod() => "";
        private string FooMethod(string prm1) => "";
        public string? FooPublicField;
        public event EventHandler? FooPublicEvent;
        public string? FooPublicProperty { get; set; }
        public string FooPublicMethod() => "";
        public string FooPublicMethod(string prm1) => "";
        private static string? FooStaticField;
        private static event EventHandler? FooStaticEvent;
        private static string? FooStaticProperty { get; set; }
        private static string FooStaticMethod() => "";
        private static string FooStaticMethod(string prm1) => "";
        public static string? FooPublicStaticField;
        public static event EventHandler? FooPublicStaticEvent;
        public static string? FooPublicStaticProperty { get; set; }
        public static string FooPublicStaticMethod() => "";
        public static string FooPublicStaticMethod(string prm1) => "";
    }
}
#pragma warning restore