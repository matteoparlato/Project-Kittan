using System;
using System.Threading;
using System.Threading.Tasks;

namespace Project_Kittan.Helpers
{
    /// <summary>
    /// ConsoleExtensions class
    /// </summary>
    public static class ConsoleExtensions
    {
        private static CancellationTokenSource TokenSource;

        private static Task Task;

        private static string LoadingText = string.Empty;

        /// <summary>
        /// Method which shows a console spinner with passed loading message.
        /// </summary>
        /// <param name="loadingText">The loading text</param>
        public static void Start(string loadingText)
        {
            TokenSource = new CancellationTokenSource();
            CancellationToken token = TokenSource.Token;

            if(string.IsNullOrWhiteSpace(loadingText))
            {
                loadingText = "Loading";
            }
            LoadingText = loadingText;

            Task = Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    var spinChars = new string[] { loadingText + ".", loadingText + "..", loadingText + "...", loadingText + "   " };

                    foreach (var spinChar in spinChars)
                    {
                        Console.Write(string.Concat("\r", spinChar, "\r"));

                        Task.Delay(250).Wait();
                    }
                }
            }, token);
        }

        /// <summary>
        /// Method which hides the previously shown spinner.
        /// </summary>
        public static void Stop()
        {
            TokenSource.Cancel();
            Task.Wait();
            Task = null;

            Console.Write(string.Concat("\r", LoadingText + "...done!", "\r"));
        }
    }
}
