using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TestWpfApp.Properties;

namespace TestWpfApp
{
    public class RandomNames
    {
        private readonly string[] _names;
        private readonly Random _random = new Random();

        public RandomNames()
        {
            string all = Resources.Names;

            _names = all.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
        }

        public void Fill(TestData data, IEnumerable<string> types)
        {
            int loc = _random.Next(0, _names.Length);

            string[] split = _names[loc].Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

            data.Name = split[0];
            data.LastName = split[1];
            data.Age = _random.Next(20, 50);
            data.Type = types.ElementAt(_random.Next(0, 100) > 50 ? 0 : 1);
        }

        public string[] Names => _names;
    }
}