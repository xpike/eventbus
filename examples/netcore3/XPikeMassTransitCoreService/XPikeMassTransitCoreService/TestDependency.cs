using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XPikeMassTransitCoreService
{
    public class TestDependency 
        : ITestDependency
    {
        public async Task FakeActivityAsync()
        {
            Console.WriteLine("Starting fake activity.");
            await Task.Delay(50);
            Console.WriteLine("Fake activity complete.");
        }
    }
}
