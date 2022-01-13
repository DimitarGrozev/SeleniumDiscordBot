using ConsoleApp4.Models;
using System;
using System.Text.Json;

namespace ConsoleApp4.Utilities
{
    public class ServiceProvider<T> where T : Entity
    {
        private readonly string filePath;

        public ServiceProvider(string filePath)
        {
            this.filePath = filePath;
        }

        public T Provide()
        {
            if (this.filePath is null)
                throw new ArgumentNullException("File path cannot be null!");

            var data = JsonSerializer.Deserialize<T>(this.filePath);

            return data;
        }
    }
}
