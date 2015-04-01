using System;
using ConsoleHelpers.Whale;
using ConsoleHelpers.WindowsService;

namespace ConsoleHelpers.WindowsServiceSamples
{
    internal class Program
    {
        private static readonly EmptyLogger _logger = new EmptyLogger();

        private static void Main(string[] args)
        {
            SampleWinServiceExecutor sampleWinServiceExecutor = null;
            try
            {
                try
                {
                    sampleWinServiceExecutor = new SampleWinServiceExecutor();
                }
                catch (Exception e)
                {
                    _logger.Fatal("Не удалось создать сервис.");
                }

                if (sampleWinServiceExecutor != null)
                {
                    if (!Environment.UserInteractive)
                    {
                        _logger.Info("Starting as Windows Service");
                        sampleWinServiceExecutor.RunAsWinService();
                    }
                    else
                    {
                        _logger.Info("Starting as Console Application with arguments: " + String.Join(" ", args));
                        try
                        {
                            CustomConsoleHelpers.DisableConsoleCloseButton();
                            CustomConsoleHelpers.SetConsoleTitleFromAssemblyInfo(sampleWinServiceExecutor.DisplayName);
                            Console.WriteLine(ASCIIWhale.HappyWhale);
                        }
                        catch (Exception e)
                        {
                            _logger.Error("Error while setting console properties.");
                        }

                        try
                        {
                            _logger.Debug("Parsing arguments");
                            sampleWinServiceExecutor.ParseWinServiceArgsWithMenu();
                        }
                        catch (Exception e)
                        {
                            _logger.Fatal("Parsing arguments failed.");
                        }
                    }
                }

                if (Environment.UserInteractive)
                {
                    Console.WriteLine(ASCIIWhale.DeadWhale);
                    Console.WriteLine("Type 'exit' to quit...");
                    CustomConsoleHelpers.WaitForInput("exit");
                }
            }
            finally
            {
                if (sampleWinServiceExecutor != null)
                    sampleWinServiceExecutor.Dispose();
            }
        }
    }
}