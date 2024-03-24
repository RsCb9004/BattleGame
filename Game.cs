using System.Collections;

namespace RsCb.TurnBasedGame {

	public struct Joint<T> : IEnumerable<T> {

		public T here, there;

		public Joint(T _here, T _there) {
			here = _here; there = _there;
		}

		public static bool Sync(Joint<T> local, Joint<T> target) =>
			Equals(local.here, target.there) && Equals(local.there, target.here);

		public readonly IEnumerator<T> GetEnumerator() {
			yield return here;
			yield return there;
		}

		readonly IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}

	internal interface IGame<TOperation, THash, TResult> {

		public string Info { get; }
		public TResult Result { get; }

		public bool CheckInfo(string info);
		public bool Operate(Joint<TOperation> operation);
		public THash GetHash();
		public bool CheckHash(THash hash);
	}
}
