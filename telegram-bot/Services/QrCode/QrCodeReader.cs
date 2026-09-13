using SkiaSharp;
using ZXing;
using ZXing.Common;

namespace telegram_bot.Services.QrCode
{
    public sealed class QrCodeReader : IQrCodeReader
    {
        /// <summary>
        /// Longest side the image is scanned at. A photo sent as a file can be
        /// 4000 px or more; at this size a code filling a tenth of the frame is
        /// still ~200 px across, plenty for the detector, and the scan runs
        /// several times faster.
        /// </summary>
        private const int MaxDimension = 2048;

        private readonly ILogger<QrCodeReader> _logger;

        public QrCodeReader(ILogger<QrCodeReader> logger)
        {
            _logger = logger;
        }

        public IReadOnlyList<string> Read(Stream image)
        {
            try
            {
                using var bitmap = Decode(image);

                if (bitmap is null)
                    return [];

                var source = new RGBLuminanceSource(
                    bitmap.Bytes,
                    bitmap.Width,
                    bitmap.Height,
                    RGBLuminanceSource.BitmapFormat.RGBA32);

                // A new reader per call: ZXing readers keep state between
                // decodes, and webhook requests run concurrently.
                var reader = new BarcodeReaderGeneric
                {
                    // Finding a QR code does not depend on its orientation.
                    AutoRotate = false,
                    Options = new DecodingOptions
                    {
                        PossibleFormats = [BarcodeFormat.QR_CODE],
                        TryHarder = true,
                    },
                };

                // One photo may hold several members' codes. The single-code
                // reader uses a different detector, so it is a cheap second
                // attempt before asking the administrator to retake the photo.
                var results = reader.DecodeMultiple(source)
                    ?? (reader.Decode(source) is { } single ? [single] : []);

                return results
                    .Select(result => result.Text?.Trim())
                    .OfType<string>()
                    .Where(text => text.Length > 0)
                    .Distinct(StringComparer.Ordinal)
                    .ToList();
            }
            catch (Exception ex)
            {
                // A malformed file is the sender's problem, not a bot failure:
                // report "no code found" rather than dropping the reply.
                _logger.LogWarning(ex, "Could not scan an image for QR codes.");

                return [];
            }
        }

        /// <summary>RGBA pixels, at most <see cref="MaxDimension"/> on the longest side.</summary>
        private static SKBitmap? Decode(Stream image)
        {
            using var codec = SKCodec.Create(image);

            // Not a format Skia reads, e.g. a HEIC straight off an iPhone.
            if (codec is null)
                return null;

            var info = codec.Info;

            var scale = Math.Min(1f, (float)MaxDimension / Math.Max(info.Width, info.Height));

            // JPEG and WebP can decode straight to a smaller size, which avoids
            // allocating the full-resolution frame only to shrink it. Other
            // formats report their full size back and are resized below.
            var size = codec.GetScaledDimensions(scale);

            var bitmap = SKBitmap.Decode(
                codec,
                new SKImageInfo(size.Width, size.Height, SKColorType.Rgba8888, SKAlphaType.Unpremul));

            if (bitmap is null || Math.Max(bitmap.Width, bitmap.Height) <= MaxDimension)
                return bitmap;

            using (bitmap)
            {
                var ratio = (float)MaxDimension / Math.Max(bitmap.Width, bitmap.Height);

                var resized = new SKImageInfo(
                    Math.Max(1, (int)(bitmap.Width * ratio)),
                    Math.Max(1, (int)(bitmap.Height * ratio)),
                    SKColorType.Rgba8888,
                    SKAlphaType.Unpremul);

                return bitmap.Resize(
                    resized,
                    new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
            }
        }
    }
}
