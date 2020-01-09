using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Terralect.Channels
{
    public class Channel
    {
        private readonly List<LayerBase> layers;
        private int index;
        private int maxIndex;
        private readonly List<LayerBase> layersUsed;

        public ReadOnlyCollection<LayerBase> GetLayersLastUsed => layersUsed.AsReadOnly();

        public Channel()
        {
            layers = new List<LayerBase>();
            layersUsed = new List<LayerBase>();
        }
        
        public void AddLayer(LayerBase layerBase)
        {
            layers.Add(layerBase);
            layerBase.SetCurrentChannel(this);
            maxIndex = layers.Count - 1;
        }

        public async Task<IEnumerable<TOut>> ProcessAsync<TIn, TOut>(IEnumerable<TIn> input)
            where TIn : class
            where TOut : class
        {
            if (layers.Count == 0)
                throw new Exception("No layers have been configured");
            
            index = -1;
            layersUsed.Clear();
            return await NextLayer<TIn, TOut>(input);
        }
        
        internal async Task<IEnumerable<TOut>> NextLayer<TIn, TOut>(IEnumerable<TIn> input)
            where TIn : class
            where TOut : class
        {
            if (index == maxIndex)
                return (IEnumerable<TOut>)input;
            
            index++;
            var layerToRun = layers[index];
            layersUsed.Add(layerToRun);
            var output = await layerToRun.ProcessNext<TIn, TOut>(input);

            if (index == maxIndex)
                return output;

            return await NextLayer<TIn, TOut>((IEnumerable<TIn>) output);
        }
    }
}