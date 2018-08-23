using System;
using System.Collections.Generic;

namespace SharpFileSystem {

    public interface ITypeDictionary<T> : ICollection<T> {

        #region indexers

        IEnumerable<T> this[Type type] { get; }

        #endregion

        #region query methods

        IEnumerable<T> Get(Type type);
        IEnumerable<T> GetExplicit(Type type);

        T GetSingle(Type type);
        T GetSingleExplicit(Type type);

        bool Contains(Type type);

        #endregion
    }
}
