using System;

namespace Valenzuela.RetryPolicy
{
    /// <summary>
    /// Exception thrown when all retries have failed
    /// </summary>
    /// <seealso cref="System.ApplicationException" />
    [Serializable]
    public class RetryPolicyException : ApplicationException
    {
        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public object State { get; private set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="RetryPolicyException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="state">The state.</param>
        /// <param name="innerException">The inner exception.</param>
        public RetryPolicyException(string message, object state, Exception innerException)
            : base(message, innerException)
        {
            State = state;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="RetryPolicyException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="state">The state.</param>
        public RetryPolicyException(string message, object state)
            : base(message)
        {
            State = state;
        }
    }
}
