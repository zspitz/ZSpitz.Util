namespace ZSpitz.Util {
    public static class LanguageNames {
        public const string CSharp = "C#";
        public const string VisualBasic = "Visual Basic";
    }

    public enum Language {
        CSharp,
        VisualBasic
    }

    public enum ConversionStrategy {
        None,
        Assignable,
        BuiltIn,
        Method
    }
}
