using System.Collections.Generic;

namespace Terralect.Channels
{
    public interface ISplitInput<TType>
    {
        IEnumerable<IEnumerable<TType>> SplitInput(IEnumerable<TType> input);
    }
}