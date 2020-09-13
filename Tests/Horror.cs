namespace Tests {
    // https://github.com/dotnet/runtime/issues/20377#issue-558157669
    public class Horror {
        public void Moo(int x, int[] y) { }
        public void Moo<T>(T x, T[] y) { }
        public void Moo<T>(int x, int[] y) { }
        public void Moo<T, U>(T x, U[] y) { }
        public void Moo<T, U>(int x, int[] y) { }
    }
}
