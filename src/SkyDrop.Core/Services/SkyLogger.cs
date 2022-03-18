using System;
using System.Runtime.CompilerServices;
using FFImageLoading.Helpers;
using MvvmCross;
using MvvmCross.Logging;

//// In exceptional cases, tooling may be placed into the root namespace to gain accessibility to the members everywhere.
//namespace SkyDrop
//{
//    public class SkyLogger : ILog, IMiniLogger
//    {
//        // private static SkyLogger _instance;
//        // public static SkyLogger Instance => _instance ?? (SkyLogger) Mvx.IoCProvider.Resolve<ILog>();

//        private static IMvxLog _mvxLog;

//        public SkyLogger(IMvxLogProvider logProvider)
//        {
//            _mvxLog = logProvider.GetLogFor<SkyLogger>();
//        }

//        // IMiniLogger methods for FFImageLoading logging
//        public void Debug(string message) => Print(message, "", 1);

//        public void Error(string errorMessage) => Print(errorMessage, "", 1);

//        public void Error(string errorMessage, Exception exception)
//        {
//            Exception(exception);

//            _mvxLog.Debug("_mvxLog IMiniLogger error: ");
//            _mvxLog.Debug($"{exception.GetType()}: {exception.Message}");
//            _mvxLog.Debug(exception.StackTrace);
//        }


//        // SkyLogger

//        public void Error(string errorMessage, Exception ex,
//            [CallerFilePath] string sourceFilePath = "",
//            [CallerLineNumber] int sourceLineNumber = 0)
//        {
//            Error(errorMessage);
//            Exception(ex);
//        }

//        public void Error(string errorMessage,
//            [CallerFilePath] string sourceFilePath = "",
//            [CallerLineNumber] int sourceLineNumber = 0) => PrintError(errorMessage, sourceFilePath, sourceLineNumber);

//        protected long exceptionCount = 0;

//        public void Exception(System.Exception ex,
//            [CallerFilePath] string sourceFilePath = "",
//            [CallerLineNumber] int sourceLineNumber = 0)
//        {
//            if (ex == null)
//            {
//                Error("ex == null", sourceFilePath, sourceLineNumber);
//                return;
//            }

//            exceptionCount++;

//            PrintError($"Encoutered exception no# {exceptionCount}", sourceFilePath, sourceLineNumber);

//            PrintExceptionInfo(ex, isInnerException: false, sourceFilePath, sourceLineNumber);
//        }

//        protected void PrintExceptionInfo(System.Exception ex, bool isInnerException, string sourceFilePath, int sourceLineNumber)
//        {
//            if (isInnerException)
//            {
//                PrintError("Logging exception - the inner exception", sourceFilePath, sourceLineNumber);
//            }

//            Print($"{ex.GetType()}: {ex.Message}", sourceFilePath, sourceLineNumber);
//            Print(ex.StackTrace, sourceFilePath, sourceLineNumber);

//            if (ex.InnerException != null)
//                PrintExceptionInfo(ex.InnerException, isInnerException: true, sourceFilePath, sourceLineNumber);
//        }

//        private void PrintError(string message,
//                                string sourceFilePath,
//                                int sourceLineNumber,
//                                bool printIf = true)
//        {
//            if (printIf)
//                Print($"[ERROR] {message}", sourceFilePath, sourceLineNumber);
//        }

//        private void Print(string message,
//                           string sourceFilePath,
//                           int sourceLineNumber)
//        {
//            TraceLog.Print(message, sourceFilePath, sourceLineNumber);
//        }
//    }

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
//}
