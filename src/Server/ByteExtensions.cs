namespace Port.Server
{
    internal static class ByteExtensions
    {
        internal static bool IsNumber(
            this byte @byte)
        {
            return @byte >= 48 &&
                   @byte <= 57;
        }
    }
}