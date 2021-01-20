using System.IO.Pipelines;

namespace Port.Server
{
    internal static class ReadResultExtensions
    {
        internal static bool HasMoreData(
            this ReadResult readResult)
            => readResult.IsCanceled == false &&
               readResult.IsCompleted == false;

        internal static string GetStatusAsString(
            this ReadResult readResult)
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