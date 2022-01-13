using ConsoleApp4.Models;

namespace ConsoleApp4
{
    public class Configuration : Entity
    {
        public int MessageDelay { get; set; }

        public int ServerSwitchDelay{ get; set; }

        public string MessagesFilePath { get; set; }
    }
}
