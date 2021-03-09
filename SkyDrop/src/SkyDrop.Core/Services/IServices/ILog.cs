using System;
using System.Runtime.CompilerServices;

// In exceptional cases, tooling may be placed into the root namespace to gain accessibility to the members everywhere.
namespace SkyDrop
{
    public interface ILog
    {
        public void Exception(Exception exception,
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0);

        void Error(string errorMessage, Exception ex,
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0);

        void Error(string errorMessage,
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0);
    }
}
