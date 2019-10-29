using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TiffLibrary
{
#pragma warning disable CA1815 // CA1815: Override equals and operator equals on value types
    /// <summary>
    /// Represents a list of values of specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The element type in the collection.</typeparam>
    public readonly struct TiffValueCollection<T> : IReadOnlyList<T>
#pragma warning restore CA1815 // CA1815: Override equals and operator equals on value types
    {
        internal readonly T[] _values;
        internal readonly T _firstValue;

        /// <summary>
        /// Gets a empty list.
        /// </summary>
        public static readonly TiffValueCollection<T> Empty = default;

        /// <summary>
        /// Create the list with a single element.
        /// </summary>
        /// <param name="value">The specified element.</param>
        public TiffValueCollection(T value)
        {
            _values = Array.Empty<T>();
            _firstValue = value;
        }

        /// <summary>
        /// Creates a list and wraps <paramref name="values"/> as the underlying storage. However, if <paramref name="values"/> has only 1 elements, the element value is copied and the array reference is discarded.
        /// </summary>
        /// <param name="values">The array to be used.</param>
        internal TiffValueCollection(T[] values)
        {
            if (values is null)
            {
                _values = null;
                _firstValue = default;
                return;
            }

            if (values.Length == 0)
            {
                _values = null;
                _firstValue = default;
            }
            else if (values.Length == 1)
            {
                _values = Array.Empty<T>();
                _firstValue = values[0];
            }
            else
            {
                _values = values;
                _firstValue = values[0];
            }
        }

        /// <summary>
        /// Creates a list of enough space and copy all the elements from <paramref name="values"/> to the list.
        /// </summary>
        /// <param name="values">The elements to be copied from.</param>
        public TiffValueCollection(ReadOnlySpan<T> values)
        {
            if (values.Length == 0)
            {
                _values = null;
                _firstValue = default;
            }
            else if (values.Length == 1)
            {
                _values = Array.Empty<T>();
                _firstValue = values[0];
            }
            else
            {
                T[] array = new T[values.Length];
                values.CopyTo(array);
                _values = array;
                _firstValue = values[0];
            }
        }

        /// <summary>
        /// Gets the element of the specified index.
        /// </summary>
        /// <param name="index">A 0-based index.</param>
        /// <returns>The element value.</returns>
        public T this[int index] => _values is null ? ThrowIndexOutOfRangeException() : (index == 0 ? _firstValue : _values[index]);

        /// <summary>
        /// Gets whether the list is empty.
        /// </summary>
        public bool IsEmpty => _values is null;

        /// <summary>
        /// Gets the element count of the list.
        /// </summary>
        public int Count => _values is null ? 0 : Math.Max(_values.Length, 1);

        /// <summary>
        /// Gets the value of the first element. If the list is empty, returns null or the default value of <typeparamref name="T"/>.
        /// </summary>
        public T FirstOrDefault => _firstValue;

        private static T ThrowIndexOutOfRangeException()
        {
            throw new IndexOutOfRangeException();
        }

        /// <summary>
        /// The enumerator for <see cref="TiffValueCollection{T}"/>.
        /// </summary>
        public struct Enumerator : IEnumerator<T>
        {
            private readonly T[] _values;
            private readonly T _firstValue;
            private int _index;
            private int _count;
            private T _current;

            /// <summary>
            /// Gets the current element.
            /// </summary>
            public T Current => _current;


            /// <summary>
            /// Gets the current element.
            /// </summary>
            object IEnumerator.Current => _current;

            /// <summary>
            /// Creates an enumerator for <see cref="TiffValueCollection{T}"/>.
            /// </summary>
            /// <param name="values">The <see cref="TiffValueCollection{T}"/> to enumerate over.</param>
            public Enumerator(in TiffValueCollection<T> values)
            {
                _values = values._values;
                _firstValue = values._firstValue;
                _index = 0;
                _count = _values is null ? 0 : (_values.Length == 0 ? 1 : _values.Length);
                _current = default;
            }

            /// <summary>
            /// Move to the next element.
            /// </summary>
            /// <returns>True if the position is moved to the next element. False if there is no more element in the list.</returns>
            public bool MoveNext()
            {
                if (_values is null)
                {
                    return false;
                }
                int index = _index;
                index++;
                if (index > _count)
                {
                    return false;
                }
                _index = index;
                _current = _index == 1 ? _firstValue : _values[index - 1];
                return true;
            }

            /// <summary>
            /// Reset the enumerator.
            /// </summary>
            public void Reset()
            {
                _index = 0;
                _current = default;
            }

            /// <summary>
            /// Dispose the enumerator.
            /// </summary>
            public void Dispose()
            {
                _index = 0;
                _current = default;
            }
        }

        /// <summary>
        /// Gets a enumerator of the list.
        /// </summary>
        /// <returns>A enumerator of the list.</returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <summary>
        /// Gets a enumerator of the list.
        /// </summary>
        /// <returns>A enumerator of the list.</returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets a enumerator of the list.
        /// </summary>
        /// <returns>A enumerator of the list.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets a human-readable representation of the list.
        /// </summary>
        /// <returns>A human-readable representation of the list.</returns>
        public override string ToString()
        {
            return $"TiffLibrary.TiffValueCollection<{typeof(T).FullName}>[{Count}]";
        }
    }

    /// <summary>
    /// Helper methods for <see cref="TiffValueCollection{T}"/>.
    /// </summary>
    public static class TiffValueCollection
    {
        /// <summary>
        /// Creates a list and use <paramref name="array"/> as the underlying storage. However, if <paramref name="array"/> has only 1 elements, the element value is copied and the array reference is discarded.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="array">The array to be used.</param>
        /// <returns>The created list.</returns>
        public static TiffValueCollection<T> UnsafeWrap<T>(T[] array)
        {
            if (array is null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            return new TiffValueCollection<T>(array);
        }

        /// <summary>
        /// Create a <see cref="TiffValueCollection{T}"/> that contains a single element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="value">Value of the element.</param>
        /// <returns>The created <see cref="TiffValueCollection{T}"/>.</returns>
        public static TiffValueCollection<T> Single<T>(T value)
        {
            return new TiffValueCollection<T>(value);
        }

        /// <summary>
        /// Create a <see cref="TiffValueCollection{T}"/> that contains no element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <returns>The created <see cref="TiffValueCollection{T}"/>.</returns>
        public static TiffValueCollection<T> Empty<T>()
        {
            return default;
        }
    }

    internal static class TiffValueCollectionExtensionsInternal
    {
        public static T[] GetOrCreateArray<T>(this TiffValueCollection<T> collection)
        {
            T[] values = collection._values;
            if (values is null)
            {
                return Array.Empty<T>();
            }
            else if (values.Length == 0)
            {
                return new T[] { collection._firstValue };
            }
            else
            {
                return values;
            }
        }

#if NO_SYSTEM_CONVERTER

        public static TResult[] GetOrCreateArray<T, TResult>(this TiffValueCollection<T> collection, Func<T, TResult> converter)
        {
            T[] values = collection._values;
            if (values is null)
            {
                return Array.Empty<TResult>();
            }
            else if (values.Length == 0)
            {
                return new TResult[] { converter(collection._firstValue) };
            }
            else
            {
                TResult[] resultArray = new TResult[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    resultArray[i] = converter(values[i]);
                }
                return resultArray;
            }
        }

        public static TiffValueCollection<TDest> ConvertAll<T, TDest>(this TiffValueCollection<T> collection, Func<T, TDest> convertFunc)
        {
            if (typeof(T) == typeof(TDest))
            {
                return Unsafe.As<TiffValueCollection<T>, TiffValueCollection<TDest>>(ref collection);
            }

            T[] values = collection._values;
            if (values is null)
            {
                return default;
            }
            else if (values.Length == 0)
            {
                return new TiffValueCollection<TDest>(convertFunc(collection._firstValue));
            }
            else
            {
                TDest[] dest = new TDest[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    dest[i] = convertFunc(values[i]);
                }
                return new TiffValueCollection<TDest>(dest);
            }
        }

#else

        public static TResult[] GetOrCreateArray<T, TResult>(this TiffValueCollection<T> collection, Converter<T, TResult> converter)
        {
            T[] values = collection._values;
            if (values is null)
            {
                return Array.Empty<TResult>();
            }
            else if (values.Length == 0)
            {
                return new TResult[] { converter(collection._firstValue) };
            }
            else
            {
                return Array.ConvertAll(values, converter);
            }
        }

        public static TiffValueCollection<TDest> ConvertAll<T, TDest>(this TiffValueCollection<T> collection, Converter<T, TDest> convertFunc)
        {
            if (typeof(T) == typeof(TDest))
            {
                return Unsafe.As<TiffValueCollection<T>, TiffValueCollection<TDest>>(ref collection);
            }

            T[] values = collection._values;
            if (values is null)
            {
                return default;
            }
            else if (values.Length == 0)
            {
                return new TiffValueCollection<TDest>(convertFunc(collection._firstValue));
            }
            else
            {
                return new TiffValueCollection<TDest>(Array.ConvertAll(values, convertFunc));
            }
        }

#endif

        public static void CopyTo<T>(this TiffValueCollection<T> collection, Span<T> destination)
        {
            T[] values = collection._values;
            if (values is null)
            {
                return;
            }
            else if (values.Length == 0)
            {
                destination[0] = collection._firstValue;
            }
            else
            {
                values.AsSpan().CopyTo(destination);
            }
        }
    }

}
