namespace backend.Services.Common
{
    public enum SaveOutcome
    {
        Success,
        NotFound,
        Conflict
    }

    /// <summary>
    /// Outcome of a write operation, so services can report a failed business
    /// rule without throwing and controllers can map it to a status code.
    /// </summary>
    public sealed class SaveResult<T>
    {
        private SaveResult(SaveOutcome outcome, T? value, string? error, string? code)
        {
            Outcome = outcome;
            Value = value;
            Error = error;
            Code = code;
        }

        public SaveOutcome Outcome { get; }

        public T? Value { get; }

        /// <summary>English description, for logs and callers that do not translate.</summary>
        public string? Error { get; }

        /// <summary>
        /// One of <see cref="ErrorCodes"/>. The admin panel shows its translation
        /// in the administrator's own language.
        /// </summary>
        public string? Code { get; }

        public static SaveResult<T> Success(T value) =>
            new(SaveOutcome.Success, value, null, null);

        public static SaveResult<T> NotFound(string error) =>
            new(SaveOutcome.NotFound, default, error, null);

        public static SaveResult<T> Conflict(string code, string error) =>
            new(SaveOutcome.Conflict, default, error, code);
    }
}
