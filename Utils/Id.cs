using System;
using System.Linq;

namespace AsinoPuzzles.Functions.Utils
{
    public sealed class IdUtils
    {
        // This is weird I know. Sometimes I just do weird things, it's my code
        public static string CreateRandomId(int attempt)
        {
            var numbers = "1234567890";
            var random = new Random();
            var template = new char[] { '#', '-', '#', '#', '#', '-', '#', '#', '#', '#' };

            if (attempt == 1)
            {
                template = new char[] { '#', '-', '#', '#' };
            }
            else if (attempt == 2)
            {
                template = new char[] { '#', '-', '#', '#', '#' };
            }
            else if (attempt == 3)
            {
                template = new char[] { '#', '#', '-', '#', '#', '#' };
            }
            else if (attempt == 4)
            {
                template = new char[] { '#', '-', '#', '#', '-', '#', '#', '#' };
            }
            else if (attempt == 5)
            {
                template = new char[] { '#', '#', '#', '-', '#', '#', '#', '#' };
            }

            template = template.Select(c => c == '-' ? c : numbers[random.Next(0, numbers.Length)]).ToArray();

            return new string(template);
        }
    }
}