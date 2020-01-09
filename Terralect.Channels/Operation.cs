using System.Collections.Generic;

namespace Terralect.Channels
{
    public abstract class Operation<TIn, TOut>
    {
        public abstract IEnumerable<TOut> Process(IEnumerable<TIn> input);
    }
}