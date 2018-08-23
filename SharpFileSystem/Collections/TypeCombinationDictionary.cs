using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpFileSystem.Collections {

    public class TypeCombinationDictionary<T> {
        readonly LinkedList<TypeCombinationEntry> _registrations = new LinkedList<TypeCombinationEntry>();

        public IEnumerable<TypeCombinationEntry> GetSupportedRegistrations(Type sourceType, Type destinationType) {
            return _registrations.Where(r => r.SourceType.IsAssignableFrom(sourceType) && r.DestinationType.IsAssignableFrom(destinationType));
        }

        public TypeCombinationEntry GetSupportedRegistration(Type sourceType, Type destinationType) {
            return GetSupportedRegistrations(sourceType, destinationType).FirstOrDefault();
        }

        public bool TryGetSupported(Type sourceType, Type destinationType, out T value) {
            var r = GetSupportedRegistration(sourceType, destinationType);
            if(r == null) {
                value = default(T);
                return false;
            }
            value = r.Value;
            return true;
        }

        public void AddFirst(Type sourceType, Type destinationType, T value) {
            _registrations.AddFirst(new TypeCombinationEntry(sourceType, destinationType, value));
        }

        public void AddLast(Type sourceType, Type destinationType, T value) {
            _registrations.AddLast(new TypeCombinationEntry(sourceType, destinationType, value));
        }

        #region sub classes

        public class TypeCombinationEntry {
            public Type SourceType { get; }
            public Type DestinationType { get; }
            public T Value { get; }

            /// <summary>
            /// Constructor
            /// </summary>
            public TypeCombinationEntry(Type sourceType, Type destinationType, T value) {
                this.SourceType = sourceType;
                this.DestinationType = destinationType;
                this.Value = value;
            }
        }

        #endregion
    }
}
