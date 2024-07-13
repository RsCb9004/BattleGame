using System.Collections;

namespace BattleGame {

	interface IContravariant<in T> { }
	interface IConvariant<out T> { }

	public struct Joint<T> : IContravariant<T>, IConvariant<T>, IEnumerable<T> {

		public T here, there;

		public Joint(T _here, T _there) {
			here = _here; there = _there;
		}

		public readonly Joint<T> Swapped() =>
			new(there, here);

		public readonly void ForEach(Action<T> action) {
			action.Invoke(here); action.Invoke(there);
		}

		public readonly Joint<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter) =>
			new(converter.Invoke(here), converter.Invoke(there));

		public readonly IEnumerator<T> GetEnumerator() {
			yield return here;
			yield return there;
		}

		readonly IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

	}
}
