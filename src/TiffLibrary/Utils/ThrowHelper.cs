using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;

namespace TiffLibrary
{
    internal static class ThrowHelper
    {
        [DoesNotReturn]
        public static void ThrowInvalidDataException()
        {
            throw new InvalidDataException();
        }

        [DoesNotReturn]
        public static void ThrowInvalidDataException(string message)
        {
            throw new InvalidDataException(message);
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException()
        {
            throw new InvalidOperationException();
        }

        [DoesNotReturn]
        public static void ThrowInvalidOperationException(string message)
        {
            throw new InvalidOperationException(message);
        }

        [DoesNotReturn]
        public static T ThrowInvalidOperationException<T>(string message)
        {
            throw new InvalidOperationException(message);
        }

        public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression("argument")] string? paramName = null)
        {
            if (argument is null)
            {
                ThrowArgumentNullException(paramName);
            }
        }

        [DoesNotReturn]
        public static void ThrowArgumentNullException(string? paramName)
        {
            throw new ArgumentNullException(paramName);
        }

        [DoesNotReturn]
        public static void ThrowArgumentOutOfRangeException(string paramName)
        {
            throw new ArgumentOutOfRangeException(paramName);
        }

        [DoesNotReturn]
        public static void ThrowArgumentOutOfRangeException(string paramName, string message)
        {
            throw new ArgumentOutOfRangeException(paramName, message);
        }

        [DoesNotReturn]
        public static void ThrowArgumentException(string message)
        {
            throw new ArgumentException(message);
        }

        [DoesNotReturn]
        public static void ThrowArgumentException(string message, string paramName)
        {
            throw new ArgumentException(message, paramName);
        }

        [DoesNotReturn]
        public static void ThrowNotSupportedException(string? message = null)
        {
            throw new NotSupportedException(message);
        }

        [DoesNotReturn]
        public static void ThrowObjectDisposedException(string? objectName)
        {
            throw new NotSupportedException(objectName);
        }

        [DoesNotReturn]
        public static T ThrowObjectDisposedException<T>(string? objectName)
        {
            throw new NotSupportedException(objectName);
        }

#pragma warning disable CA2201 // Do not raise reserved exception types

        [DoesNotReturn]
        public static T ThrowIndexOutOfRangeException<T>()
        {
            throw new IndexOutOfRangeException();
        }

        [DoesNotReturn]
        public static void ThrowIndexOutOfRangeException()
        {
            throw new IndexOutOfRangeException();
        }

#pragma warning restore CA2201
    }
}
