using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Raft.Tests.Unit.TestHelpers
{
    internal class TestTaskScheduler : TaskScheduler
    {
        public TestTaskScheduler()
        {
            // Hacky is an understatement!
            var taskSchedulerType = typeof(TaskScheduler);
            var defaultTaskSchedulerField = taskSchedulerType.GetField("s_defaultTaskScheduler", BindingFlags.SetField | BindingFlags.Static | BindingFlags.NonPublic);
            defaultTaskSchedulerField.SetValue(null, this);
        }

        public readonly List<Task> TaskQueue = new List<Task>();

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return TaskQueue;
        }

        protected override void QueueTask(Task task)
        {
            TaskQueue.Add(task);
            TryExecuteTask(task);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return TryExecuteTask(task);
        }

        public void RunAll()
        {
            foreach (var task in TaskQueue)
            {
                task.RunSynchronously();
            }
        }
    }
}
