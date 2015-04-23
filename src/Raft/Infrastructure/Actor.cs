using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace Raft.Infrastructure
{
    internal abstract class Actor<TMessage> : IHandle<TMessage>
    {
        private readonly List<IDisposable> _sourceLinks;
        protected readonly ActionBlock<TMessage> MessagePipeline;

        protected Actor()
        {
             _sourceLinks = new List<IDisposable>();
            MessagePipeline = new ActionBlock<TMessage>(new Action<TMessage>(Handle));
        }

        public abstract void Handle(TMessage message);

        public void AddSourceLink(ISourceBlock<TMessage> source)
        {
            _sourceLinks.Add(source.LinkTo(MessagePipeline));
        }

        protected void CompleteActor()
        {
            MessagePipeline.Complete();
            MessagePipeline.Completion.Wait();

            _sourceLinks.ForEach(x => x.Dispose());
        }
    }
}
