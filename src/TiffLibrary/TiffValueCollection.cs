using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace TiffLibrary
{
#pragma warning disable CA1815 // CA1815: Override equals and operator equals on value types
    /// <summary>
    /// Represents a list of values of specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The element type in the collection.</typeparam>
    [DebuggerTypeProxy(typeof(TiffValueCollectionDebugView<>))]
    [DebuggerDisplay("{ToString(),raw}")]
    public readonly struct TiffValueCollection<T> : IReadOnlyList<T>
#pragma warning restore CA1815 // CA1815: Override equals and operator equals on value types
    {
        internal readonly T[]? _values;
        internal readonly T _firstValue;

        /// <summary>
        /// Gets a empty list.
        /// </summary>
        internal static readonly TiffValueCollection<T> s_empty = default;

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
                _firstValue = default!;
                return;
            }

            if (values.Length == 0)
            {
                _values = null;
                _firstValue = default!;
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
                _firstValue = default!;
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
        /// Gets the value of the first element. If the list is empty, returns the default value of <typeparamref name="T"/>.
        /// </summary>
        /// <returns></returns>
        [return: MaybeNull]
        public T GetFirstOrDefault() => _firstValue;

        [DoesNotReturn]
        private static T ThrowIndexOutOfRangeException()
        {
            throw new IndexOutOfRangeException();
        }

        /// <summary>
        /// The enumerator for <see cref="TiffValueCollection{T}"/>.
        /// </summary>
        public struct Enumerator : IEnumerator<T>
        {
            private readonly T[]? _values;
            private readonly T _firstValue;
            private int _index;
            private int _count;
            private T _current;

            /// <inheritdoc />
            public T Current => _current;

            /// <inheritdoc />
            object? IEnumerator.Current => _current;

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
                _current = default!;
            }

            /// <inheritdoc />
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

            /// <inheritdoc />
            public void Reset()
            {
                _index = 0;
                _current = default!;
            }

            /// <inheritdoc />
            public void Dispose()
            {
                _index = 0;
                _current = default!;
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

        /// <inheritdoc />
        public override string ToString()
        {
            return $"TiffLibrary.TiffValueCollection<{typeof(T).Name}>[{Count}]";
        }
    }

#pragma warning disable CA1801, CA1812, CA1823
    internal sealed class TiffValueCollectionDebugView<T>
    {
        private readonly T[] _array;

        public TiffValueCollectionDebugView(TiffValueCollection<T> collection)
        {
            _array = collection.GetOrCreateArray();
        }

        public TiffValueCollectionDebugView(TiffMutableValueCollection<T> collection)
        {
            _array = Unsafe.As<TiffMutableValueCollection<T>, TiffValueCollection<T>>(ref collection).GetOrCreateArray();
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items => _array;
    }
#pragma warning restore

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
            return TiffValueCollection<T>.s_empty;
        }

        /// <summary>
        /// Create T[] and copy all the elements from <paramref name="values"/> into the array.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="values">The element collection to copy from.</param>
        /// <returns>The created array.</returns>
        public static T[] ToArray<T>(this TiffValueCollection<T> values)
        {
            if (values.IsEmpty)
            {
                return Array.Empty<T>();
            }
            Span<T> source = values._values;
            if (source.Length == 0)
            {
                return new T[] { values._firstValue };
            }
            T[] arr = new T[values.Count];
            source.CopyTo(arr);
            return arr;
        }

        /// <summary>
        /// Copy all the elements from <paramref name="values"/> into the destination span.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="values">The element collection to copy from.</param>
        /// <param name="destination">The destination span.</param>
        /// <returns>True if the destination span is big enough to copy to; otherwise, false.</returns>
        public static bool TryCopyTo<T>(this TiffValueCollection<T> values, Span<T> destination)
        {
            if (values.IsEmpty)
            {
                return true;
            }
            Span<T> source = values._values;
            if (source.Length == 0)
            {
                if (destination.IsEmpty)
                {
                    return false;
                }
                else
                {
                    destination[0] = values._firstValue;
                }
            }
            return source.TryCopyTo(destination);
        }

        /// <summary>
        /// Copy all the elements from <paramref name="values"/> into the destination span.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="values">The element collection to copy from.</param>
        /// <param name="destination">The destination span.</param>
        /// <exception cref="ArgumentException">The destination span is too small.</exception>
        public static void CopyTo<T>(this TiffValueCollection<T> values, Span<T> destination)
        {
            if (values.IsEmpty)
            {
                return;
            }
            Span<T> source = values._values;
            if (source.Length == 0)
            {
                if (destination.IsEmpty)
                {
                    throw new ArgumentException("Destination span is too small.", nameof(destination));
                }
                else
                {
                    destination[0] = values._firstValue;
                    return;
                }
            }
            if (source.Length > destination.Length)
            {
                throw new ArgumentException("Destination span is too small.", nameof(destination));
            }
            source.CopyTo(destination);
        }

        /// <summary>
        /// Gets the underlying array from the element collection.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="values">The element collection.</param>
        /// <param name="array">The underlying array if there is one.</param>
        /// <returns>True if the array is returned; false if there is no array backing the collection.</returns>
        public static bool UnsafeTryGetArray<T>(TiffValueCollection<T> values, [NotNullWhen(true)] out T[] array)
        {
            T[]? underlying = values._values;
            if (underlying is null || underlying.Length == 0)
            {
                array = null!;
                return false;
            }
            array = underlying;
            return true;
        }
    }

    internal static class TiffValueCollectionExtensionsInternal
    {
        public static T[] GetOrCreateArray<T>(this TiffValueCollection<T> collection)
        {
            T[]? values = collection._values;
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

        public static TResult[] GetOrCreateArray<T, TResult>(this TiffValueCollection<T> collection, Func<T, TResult> converter)
        {
            T[]? values = collection._values;
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

            T[]? values = collection._values;
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
    }

}
