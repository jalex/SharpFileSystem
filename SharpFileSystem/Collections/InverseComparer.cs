using System.Collections.Generic;

namespace SharpFileSystem.Collections {

    public class InverseComparer<T> : IComparer<T> {

        #region properties

        public IComparer<T> Comparer { get; }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public InverseComparer(IComparer<T> comparer) {
            this.Comparer = comparer;
        }

        #region IComparer<T> members

        public int Compare(T x, T y) {
            return Comparer.Compare(y, x);
        }

        #endregion
    }
}
