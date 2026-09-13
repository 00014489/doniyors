namespace telegram_bot.Models
{
    /// <summary>An adventure an administrator can credit members for.</summary>
    public sealed record ScanAdventure(
        int Id,
        string Title,
        DateTimeOffset TravelDate,
        int Points);

    /// <summary>Kept in the session's <c>DataJson</c> while an administrator is scanning.</summary>
    public sealed record QrScanSession(int TravelId);

    public enum ScanAwardStatus
    {
        /// <summary>The member was credited just now.</summary>
        Awarded,

        /// <summary>The member already holds an active credit for this adventure.</summary>
        AlreadyAwarded,

        /// <summary>The account exists but has been disabled.</summary>
        UserDisabled,

        /// <summary>No member carries this QR token.</summary>
        UnknownCode,
    }

    /// <summary>What happened to one scanned code.</summary>
    /// <param name="Balance">The member's balance after the scan; 0 when there is no member.</param>
    public sealed record ScanAward(
        ScanAwardStatus Status,
        int UserId = 0,
        string FirstName = "",
        string UserName = "",
        int Balance = 0);
}
