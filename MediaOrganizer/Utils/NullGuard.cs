using System.Runtime.CompilerServices;

namespace MediaOrganizer.Utils
{
    /// <summary>
    /// Small helpers for throwing standardized exceptions for null.
    /// </summary>
    public static class NullGuard
    {
        /// <summary>
        /// Exception is thrown when <paramref name="value"/> is null. Exception message includes name of passed parameter or expression.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="value">Value to validate.</param>
        /// <param name="parameterExpression">Caller expression used to render a helpful message.</param>
        /// <exception cref="InvalidOperationException" />
        public static void ThrowIfNull<T>(T? value, [CallerArgumentExpression("value")] string parameterExpression = "")
        {
            if (value is null) 
            {
                throw new InvalidOperationException($"Required parameter {parameterExpression} was null.");
            }
        }
    }
}