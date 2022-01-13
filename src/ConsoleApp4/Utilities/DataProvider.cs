using ConsoleApp4.Models;
using System;
using System.IO;
using System.Text.Json;

namespace ConsoleApp4.Utilities
{
    public class DataProvider<T>
    {
        private readonly string filePath;

        public DataProvider(string filePath)
        {
            this.filePath = filePath;
        }

        public T Provide()
        {
            if (this.filePath is null)
                throw new ArgumentNullException("File path cannot be null!");

            var data = File.ReadAllText(this.filePath);

            if (data is null)
                throw new ArgumentException($"Configuration for type: {typeof(T).Name} is null or empty");

            var entity = JsonSerializer.Deserialize<T>(data);

            if (entity is null)
                throw new InvalidDataException($"Something went wrong during configuration retrieval!");

            return entity;
        }
    }
}
