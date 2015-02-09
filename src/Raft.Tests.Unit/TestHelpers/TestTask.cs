using System.Threading.Tasks;

namespace Raft.Tests.Unit.TestHelpers
{
    internal class TestTask
    {
        public static Task Create()
        {
            return Create<int>();
        }

        public static Task<T> Create<T>()
        {
            var tcs = new TaskCompletionSource<T>();
            return tcs.Task;
        }
    }
}
