using ChattingServerConsoleClient;

namespace ChattingServiceConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleClient client = new ConsoleClient();
            client.Run();
        }
    }
}