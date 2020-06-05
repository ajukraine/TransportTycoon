using NUnit.Framework;
using TransportTycoon.ConsoleApp;

namespace TransportTycoon.Tests
{
    public class SolutionTests
    {
        [TestCase("A", ExpectedResult=5)]
        [TestCase("AB", ExpectedResult=5)]
        [TestCase("BB", ExpectedResult=5)]
        [TestCase("ABB", ExpectedResult=7)]
        [TestCase("AABABBAB", ExpectedResult=29)]
        [TestCase("ABBBABAAABBB", ExpectedResult=41)]
        public int Solution_should_solve_the_problem(string input)
        {
            return Solution.Solve(input);
        }
    }
}