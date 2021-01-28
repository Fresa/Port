using System.IO.Pipelines;

namespace Spdy.Extensions
{
    internal static class FlushResultExtensions
    {
        internal static bool HasMore(
            this FlushResult readResult)
            => readResult.IsCanceled == false &&
               readResult.IsCompleted == false;

        internal static string GetStatusAsString(
            this FlushResult readResult)
        {
            var status = string.Empty;
            if (readResult.IsCanceled)
            {
                status += "Canceled";
            }

            if (readResult.IsCompleted)
            {
                status += (status == string.Empty ? "" : " and ") + "Completed";
            }

            return status;
        }
    }
}