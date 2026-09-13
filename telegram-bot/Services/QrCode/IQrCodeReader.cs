namespace telegram_bot.Services.QrCode
{
    public interface IQrCodeReader
    {
        /// <summary>
        /// The text of every QR code in the image, without duplicates. Empty
        /// when there is none, or when the bytes are not an image at all.
        /// </summary>
        IReadOnlyList<string> Read(Stream image);
    }
}
