using Game.Engine.Win2d;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Windows.Foundation;
using Windows.UI;

namespace NeuralNetworkDemoApp
{
    public class NeuralNetworkScene : Scene
    {
        private readonly NeuralNetwork.NeuralNetwork _nn;

        private readonly List<Neuron> _neurals = new List<Neuron>();
        private readonly List<Neuron.Link> _links = new List<Neuron.Link>();

        public IReadOnlyList<Neuron> Neurals => _neurals;

        public List<Neuron.Link> Links => _links;

        public NeuralNetworkScene(NeuralNetwork.NeuralNetwork nn)
        {
            _nn = nn;
        }

        protected override Task DoInitializeAsync()
        {
            Size = new Size(800, 400);
            
            //input -> hidden -> output
            for (int i = 0; i < _nn.Layers.Length - 1; i++)
            {
                for (int j = 0; j < _nn.Layers[i]; j++)
                {
                    var from = GetOrAddNeuron(i, j);
                    for (int k = 0; k < _nn.Layers[i + 1]; k++)
                    {
                        var to = GetOrAddNeuron(i + 1, k);
                        var link = new Neuron.Link(this, from, to);
                        _links.Add(link);
                        AddElement(link);
                    }
                }
            }

            return Task.CompletedTask;
        }

        private Vector2 GetLocation(int i, int j)
        {
            var distanceX = Size.Width / (_nn.Layers.Length);
            var distanceY = Size.Height / (_nn.Layers[i]);
            return new Vector2(
                (float) ((distanceX / 2) + distanceX * i),
                (float) ((distanceY / 2) + distanceY * j));
        }

        private Neuron GetOrAddNeuron(int i, int j)
        {
            var neural = Neurals.FirstOrDefault(n => n.I == i && n.J == j);
            if (neural != null)
            {
                return neural;
            }
            neural = new Neuron(this, _nn, i, j) {Location = GetLocation(i, j)};
            neural.Radius = (float)(neural.Location.Y * 0.2d / (j + 0.5)); //20% on the distance between neurons
            if (neural.Radius > 25)
            {
                neural.Radius = 25;
            }
            _neurals.Add(neural);
            AddElement(neural);
            return neural;
        }

        protected override void DoDraw(CanvasDrawingSession canvasDrawingSession)
        {
        }

        protected override void DoUpdate()
        {
        }
    }
}
