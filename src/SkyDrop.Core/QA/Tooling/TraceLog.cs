using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

// In exceptional cases, tooling may be placed into the root namespace to gain accessibility to the members everywhere.
// ReSharper disable once CheckNamespace
namespace SkyDrop
{
    public static class TraceLog
    {
        // help from https://stackoverflow.com/a/39137495/9436321

        [Conditional("DEBUG")]
        public static void Trace(this ILog t, string message,
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            Print(message, sourceFilePath, sourceLineNumber);
        }

        [Conditional("DEBUG")]
        public static void Print(string message, string sourceFilePath, int sourceLineNumber)
        {
            try
            {
                string fileName = Path.GetFileName(sourceFilePath);

                Debug.WriteLine($"{fileName}:{sourceLineNumber} " + message);
            }
            catch (Exception)
            {
                Debug.WriteLine(message);
            }
        }
    }
}