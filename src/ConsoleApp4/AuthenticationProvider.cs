using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp4
{
    public static class AuthenticationProvider
    {
        private const string filePath = "../../../credentials.txt";

        public static Tuple<string, string> GetCredentials()
        {
            var credentials = File.ReadAllLines(filePath);
            var tuple = Tuple.Create(credentials[0], credentials[1]);

            return tuple;
        }
    }
}
