using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;

namespace ICSharpCode.SharpZipLib
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
        public static T ThrowInvalidDataException<T>(string message)
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
    }
}
