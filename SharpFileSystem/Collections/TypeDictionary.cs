﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace SharpFileSystem.Collections {

    public class TypeDictionary<T> : ITypeDictionary<T>, IServiceProvider {
        readonly IDictionary<Type, ICollection<T>> _types = new Dictionary<Type, ICollection<T>>();

        #region public transformation methods

        public void Add(T item) {
            if(item == null) throw new ArgumentNullException(nameof(item));

            var itemType = item.GetType();
            foreach(var type in GetSubTypes(itemType)) {
                var itemsOfType = EnsureType(type);
                if(!itemsOfType.Contains(item)) itemsOfType.Add(item);
            }
        }

        public bool Remove(T item) {
            var itemType = item.GetType();
            foreach(var type in GetSubTypes(itemType)) {
                if(_types.TryGetValue(type, out var itemsOfType)) {
                    if(!itemsOfType.Remove(item)) return false;
                    if(itemsOfType.Count == 0) _types.Remove(type);
                } else {
                    return false;
                }
            }
            return true;
        }

        public void Clear() {
            _types.Clear();
        }

        #endregion

        #region public query methods

        public IEnumerable<T> this[Type type] => Get(type);

        #region Get methods

        public IEnumerable<T> Get(Type type) {
            if(_types.TryGetValue(type, out var itemsOfType)) {
                foreach(var item in itemsOfType) {
                    yield return item;
                }
            }
        }

        public IEnumerable<TGet> Get<TGet>() {
            if(_types.TryGetValue(typeof(TGet), out var itemsOfType)) {
                foreach(object item in itemsOfType) {
                    yield return (TGet)item;
                }
            }
        }

        #endregion

        #region GetExplicit methods

        public IEnumerable<T> GetExplicit(Type type) {
            if(type.IsAbstract) throw new ArgumentException("The specified type is not a instantiatable type and cannot be explicitly returned.", nameof(type));

            ValidateType(type);
            if(_types.TryGetValue(type, out var itemsOfType)) {
                foreach(var item in itemsOfType) {
                    if(item.GetType() == type) yield return item;
                }
            }
        }

        public IEnumerable<TGet> GetExplicit<TGet>() where TGet : T {
            foreach(var item in GetExplicit(typeof(TGet))) {
                yield return (TGet)item;
            }
        }

        #endregion

        #region GetSingle methods

        public T GetSingle(Type type) {
            if(_types.TryGetValue(type, out var itemsOfType)) {
                foreach(var item in itemsOfType) {
                    return item;
                }
            }
            return default(T);
        }

        public TGet GetSingle<TGet>() {
            return (TGet)(object)GetSingle(typeof(TGet));
        }

        #endregion

        #region GetSingleExplicit Methods

        public T GetSingleExplicit(Type type) {
            if(type.IsAbstract) throw new ArgumentException("The specified type is not a instantiatable type and cannot be explicitly returned.", nameof(type));

            ValidateType(type);
            if(_types.TryGetValue(type, out var itemsOfType)) {
                foreach(var item in itemsOfType) {
                    if(item.GetType() == type) return item;
                }
            }
            return default(T);
        }

        public TGet GetSingleExplicit<TGet>() where TGet : T {
            return (TGet)GetSingleExplicit(typeof(TGet));
        }

        #endregion

        #region Contains methods

        public bool Contains(Type type) {
            if(type == null) return false;
            return _types.ContainsKey(type);
        }

        public bool Contains<TContains>() {
            return Contains(typeof(TContains));
        }

        public bool Contains(T item) {
            if(item == null) return false;

            var itemType = item.GetType();
            if(_types.TryGetValue(itemType, out var itemsOfType)) return itemsOfType.Contains(item);
            return false;
        }

        #endregion

        public int Count => GetBaseCollection().Count;

        public void CopyTo(T[] array, int arrayIndex) {
            GetBaseCollection().CopyTo(array, arrayIndex);
        }

        bool ICollection<T>.IsReadOnly => false;

        #region GetEnumerator methods

        public IEnumerator<T> GetEnumerator() {
            return GetBaseCollection().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetBaseCollection().GetEnumerator();
        }

        #endregion

        #endregion

        #region IServiceProvider members

        object IServiceProvider.GetService(Type serviceType) {
            return Get(serviceType);
        }

        #endregion

        #region private helper methods

        ICollection<T> GetBaseCollection() {
            return EnsureType(GetBaseType());
        }

        Type GetBaseType() {
            return typeof(T);
        }

        void ValidateType(Type type) {
            if(!GetBaseType().IsAssignableFrom(type)) {
                throw new ArgumentException($"The specified type is not a subtype of '{GetBaseType().ToString()}'.", nameof(type));
            }
        }

        ICollection<T> AddType(Type type) {
            var result = new LinkedList<T>();
            _types.Add(type, result);
            return result;
        }

        ICollection<T> EnsureType(Type type) {
            if(_types.TryGetValue(type, out var result)) return result;
            return AddType(type);
        }

        IEnumerable<Type> GetSubTypes(Type type) {
            var currentType = type;
            while(currentType != GetBaseType().BaseType && currentType != null) {
                yield return currentType;
                currentType = currentType.BaseType;
            }
            foreach(var interfaceType in type.GetInterfaces()) {
                yield return interfaceType;
            }
        }

        #endregion
    }
}
