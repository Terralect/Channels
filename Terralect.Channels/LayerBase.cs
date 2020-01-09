using System.Collections.Generic;
using System.Threading.Tasks;

namespace Terralect.Channels
{
    public abstract class LayerBase
    {
        internal abstract Task<IEnumerable<TOutput>> ProcessNext<TInput, TOutput>(IEnumerable<TInput> input)
            where TInput : class
            where TOutput : class;
        internal abstract void SetCurrentChannel(Channel value);
    }
}