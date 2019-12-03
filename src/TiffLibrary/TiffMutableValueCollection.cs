using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace TiffLibrary
{
    [DebuggerTypeProxy(typeof(TiffValueCollectionDebugView<>))]
    [DebuggerDisplay("{ToString(),raw}")]
    internal struct TiffMutableValueCollection<T>
    {
        internal readonly T[] _values;
        internal T _firstValue;

        public static readonly TiffValueCollection<T> Empty = default;

        public TiffMutableValueCollection(int count)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            _values = count == 1 ? Array.Empty<T>() : new T[count];
            _firstValue = default;
        }

        public T this[int index]
        {
            set
            {
                if (_values is null)
                {
                    ThrowIndexOutOfRangeException();
                }

                if (_values.Length == 0)
                {
                    if (index != 0)
                    {
                        ThrowIndexOutOfRangeException();
                    }
                }
                else if ((uint)index >= (uint)_values.Length)
                {
                    ThrowIndexOutOfRangeException();
                }
                else
                {
                    _values[index] = value;
                }

                if (index == 0)
                {
                    _firstValue = value;
                }

            }
            get => _values is null ? ThrowIndexOutOfRangeException() : (index == 0 ? _firstValue : _values[index]);
        }

        private static T ThrowIndexOutOfRangeException()
        {
            throw new IndexOutOfRangeException();
        }

        public readonly bool IsEmpty => _values is null;
        public readonly int Count => _values is null ? 0 : Math.Max(_values.Length, 1);

        public override string ToString()
        {
            return $"TiffLibrary.TiffMutableValueCollection<{typeof(T).Name}>[{Count}]";
        }

    }

    internal static class TiffMutableValueCollectionExtensionsInternal
    {
        public static TiffValueCollection<T> GetReadOnlyView<T>(this in TiffMutableValueCollection<T> collection)
        {
            if (collection.IsEmpty)
            {
                return default;
            }
            return Unsafe.As<TiffMutableValueCollection<T>, TiffValueCollection<T>>(ref Unsafe.AsRef(in collection));
        }

        public static TiffValueCollection<T> Clone<T>(this in TiffMutableValueCollection<T> collection)
        {
            if (collection.IsEmpty)
            {
                return default;
            }
            if (collection._values.Length == 0)
            {
                return new TiffValueCollection<T>(collection._firstValue);
            }
            return new TiffValueCollection<T>(collection._values.AsSpan());
        }
    }
}
