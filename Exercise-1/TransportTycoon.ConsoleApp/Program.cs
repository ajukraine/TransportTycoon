using TransportTycoon.Domain;

namespace TransportTycoon.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var output = Solution.Solve(args[0]);

            System.Console.WriteLine(output);
        }
    }
}
