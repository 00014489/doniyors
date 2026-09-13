using System;

namespace backend.DTOs
{
    /// <summary>
    /// Image metadata returned to the admin panel. The bytes themselves are served
    /// separately by the image endpoint so list payloads stay small.
    /// </summary>
    public class TravelImageDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }

    /// <summary>A file accepted for storage, already read out of the request.</summary>
    public sealed record TravelImageUpload(
        string FileName,
        string ContentType,
        byte[] Data);
}
