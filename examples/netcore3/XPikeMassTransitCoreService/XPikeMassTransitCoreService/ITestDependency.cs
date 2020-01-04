using System.Threading.Tasks;

namespace XPikeMassTransitCoreService
{
    public interface ITestDependency
    {
        Task FakeActivityAsync();
    }
}