using Doniyors.Data;
using Doniyors.Data.Entities;
using telegram_bot.DAL.Repositories.Transactions;
using telegram_bot.DAL.Repositories.Travels;
using telegram_bot.Models;
using telegram_bot.Services.QrCode;

namespace telegram_bot.Services
{
    /// <summary>
    /// Crediting members for an adventure: an administrator picks the adventure,
    /// then photographs members' QR codes (the code on the Mini App's QR tab,
    /// which carries the member's <c>QrToken</c>).
    /// </summary>
    public class QrScanService
    {
        /// <summary>Most adventures the picker offers.</summary>
        private const int MaxAdventures = 20;

        /// <summary>Width of <c>Users.QrToken</c>; longer text is some other QR code.</summary>
        private const int MaxQrTokenLength = 255;

        /// <summary>
        /// How long after its start an adventure can still be picked. Codes are
        /// scanned on the day, usually once the adventure has begun, so a strict
        /// "still in the future" filter would hide it exactly when it is needed.
        /// </summary>
        private static readonly TimeSpan ScanGracePeriod = TimeSpan.FromDays(1);

        private readonly ITravelRepository _travelRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IQrCodeReader _qrCodeReader;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<QrScanService> _logger;

        public QrScanService(
            ITravelRepository travelRepository,
            ITransactionRepository transactionRepository,
            IQrCodeReader qrCodeReader,
            TimeProvider timeProvider,
            ILogger<QrScanService> logger)
        {
            _travelRepository = travelRepository;
            _transactionRepository = transactionRepository;
            _qrCodeReader = qrCodeReader;
            _timeProvider = timeProvider;
            _logger = logger;
        }

        /// <summary>
        /// Administrators with an active account. Checked on every step, not
        /// only at the start, so an account demoted or disabled mid-session
        /// stops crediting at once.
        /// </summary>
        public static bool CanScan(User? user) =>
            user is { Status: true }
            && user.TypeUserId is UserProvisioning.SuperAdminRoleId or UserProvisioning.AdminRoleId;

        public Task<IReadOnlyList<ScanAdventure>> GetScannableAdventuresAsync(
            CancellationToken cancellationToken = default)
        {
            return _travelRepository.GetScannableAsync(ScanWindowStart, MaxAdventures, cancellationToken);
        }

        /// <summary>An adventure being picked — only one the picker would still offer.</summary>
        public Task<ScanAdventure?> GetScannableAdventureAsync(
            int travelId,
            CancellationToken cancellationToken = default)
        {
            return _travelRepository.GetActiveAsync(travelId, ScanWindowStart, cancellationToken);
        }

        /// <summary>
        /// The adventure a running session credits. It only has to be active:
        /// its date was checked when it was picked, and a queue that runs past
        /// the grace period should not be cut off halfway.
        /// </summary>
        public Task<ScanAdventure?> GetActiveAdventureAsync(
            int travelId,
            CancellationToken cancellationToken = default)
        {
            return _travelRepository.GetActiveAsync(travelId, null, cancellationToken);
        }

        public IReadOnlyList<string> ReadCodes(Stream image) => _qrCodeReader.Read(image);

        /// <summary>Credits the member behind each code; one result per code, in order.</summary>
        public async Task<IReadOnlyList<ScanAward>> AwardAsync(
            IReadOnlyList<string> codes,
            ScanAdventure adventure,
            long adminTgUserId,
            CancellationToken cancellationToken = default)
        {
            var awards = new List<ScanAward>(codes.Count);

            foreach (var code in codes)
            {
                if (code.Length > MaxQrTokenLength)
                {
                    awards.Add(new ScanAward(ScanAwardStatus.UnknownCode));
                    continue;
                }

                var award = await _transactionRepository.AwardAdventureAsync(
                    code,
                    adventure.Id,
                    adventure.Points,
                    _timeProvider.GetUtcNow(),
                    cancellationToken);

                // Transactions do not record who created them, so the log is
                // the audit trail for scanned credits.
                _logger.LogInformation(
                    "QR scan for TravelId={TravelId} by admin TgUserId={AdminTgUserId}: {Status} UserId={UserId}",
                    adventure.Id,
                    adminTgUserId,
                    award.Status,
                    award.UserId);

                awards.Add(award);
            }

            return awards;
        }

        private DateTimeOffset ScanWindowStart => _timeProvider.GetUtcNow() - ScanGracePeriod;
    }
}
