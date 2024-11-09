namespace BattleGame {

    public struct Pair<T> {

        public T here, there;

        public Pair(T here, T there) {
            this.here = here; this.there = there;
        }

        public readonly Pair<T> Swapped() =>
            new(there, here);

        public readonly bool Contains(T item) =>
            Equals(here, item) || Equals(there, item);

        public readonly void ForEach(Action<T> action) {
            action.Invoke(here); action.Invoke(there);
        }

        public readonly void ForEach<TOther>(Pair<TOther> other, Action<T, TOther> action) {
            action.Invoke(here, other.here); action.Invoke(there, other.there);
        }

        public readonly Pair<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter) =>
            new(converter.Invoke(here), converter.Invoke(there));

        public readonly Pair<TOutput> Combine<TOther, TOutput>(Pair<TOther> other, Func<T, TOther, TOutput> zipper) =>
            new(zipper.Invoke(here, other.here), zipper.Invoke(there, other.there));

    }

}
