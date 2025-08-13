namespace IgniteLifeApi.Domain.Constants
{
    public static class FieldLengths
    {
        // Text shown in cards/lists
        public const int Title = 200;

        // Long-form marketing copy;
        public const int Description = 4000;

        // For short notes, summaries, or brief descriptions
        public const int ShortText = 512;

        // Accessibility
        public const int ImageAltText = 256;

        // URL limits (common safe upper bound)
        public const int Url = 2048;

        // Optional: if you keep a user-facing scheduling note
        public const int SchedulingNote = 1000;

        // Security / tokens
        // Hex-encoded SHA-256 string length
        public const int TokenHashHexSha256 = 64;

        // For encoded password/token 
        public const int EncodedHash = 255;
    }
}
