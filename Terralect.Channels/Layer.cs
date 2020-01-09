using System.Collections.Generic;
using System.Threading.Tasks;

namespace Terralect.Channels
{
    public abstract class LayerBase<TIn, TOut> : LayerBase 
        where TIn : class 
        where TOut : class
    {
        private readonly List<Operation<TIn,TOut>> operations;
        private Channel currentChannel;
        private ISplitInput<TIn> inputSplitter;
        
        public LayerBase()
        {
            operations = new List<Operation<TIn, TOut>>();
        }

        public void AssignSplitter(ISplitInput<TIn> value)
        {
            inputSplitter = value;
        }

        internal override void SetCurrentChannel(Channel value)
        {
            currentChannel = value;
        }
        
        public LayerBase<TIn, TOut> AddOperation(Operation<TIn, TOut> value)
        {
            operations.Add(value);
            return this;
        }

        internal override async Task<IEnumerable<TOutput>> ProcessNext<TInput, TOutput>(IEnumerable<TInput> input)
        {
            return (IEnumerable<TOutput>) await Process((IEnumerable<TIn>) input);
        }

        public abstract Task<IEnumerable<TOut>> Process(IEnumerable<TIn> input);
        
        public async Task<List<TOut>> PerformOperations(IEnumerable<TIn> input)
        {
            List<Task<IEnumerable<TOut>>> taskData;
            if (inputSplitter != null)
            {
                var partitions = inputSplitter.SplitInput(input);
                taskData = new List<Task<IEnumerable<TOut>>>();
                foreach (var partition in partitions)
                {
                    taskData.AddRange(RunOperationTasks(partition));
                }
            }
            else
            {
                taskData = RunOperationTasks(input);   
            }
            await Task.WhenAll(taskData);
            var toReturn = new List<TOut>();
            foreach (var task in taskData)
                toReturn.AddRange(task.Result);
            return toReturn;
        }

        private List<Task<IEnumerable<TOut>>> RunOperationTasks(IEnumerable<TIn> input)
        {
            var tasks = new List<Task<IEnumerable<TOut>>>();
            foreach (var operation in operations)
            {
                var task = Task.Run(() =>
                {
                    var dataOut = operation.Process(input);
                    return dataOut;
                });
                tasks.Add(task);
            }
            return tasks;
        }

        public async Task<IEnumerable<TOut>> NextLayer<TNextInput>(IEnumerable<TNextInput> input) 
            where TNextInput : class
        {
            return await currentChannel.NextLayer<TNextInput, TOut>(input);
        }
    }
}