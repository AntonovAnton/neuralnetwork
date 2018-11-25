using System;
using System.Numerics;
using Game.Engine.Win2d;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;

namespace NeuralNetworkDemoApp
{
    public class Neuron : SceneElement
    {
        private readonly NeuralNetwork.NeuralNetwork _nn;
        private readonly int _i;
        private readonly int _j;

        private double Activation => _nn.Activations[_i][_j];
        
        public float Radius { get; internal set; } = 5f;

        public Vector2 Location { get; internal set; }

        public int I => _i;

        public int J => _j;

        public Neuron(Scene scene, NeuralNetwork.NeuralNetwork nn, int i, int j)
            : base(scene)
        {
            _nn = nn;
            _i = i;
            _j = j;
        }

        protected override bool DoIntersect(ISceneElement other)
        {
            return false;
        }

        protected override void DoDraw(CanvasDrawingSession ds)
        {
            ds.DrawText(
                Activation.ToString("#0.####"),
                Location, Colors.Red,
                new CanvasTextFormat()
                {
                    HorizontalAlignment = CanvasHorizontalAlignment.Center,
                    VerticalAlignment = CanvasVerticalAlignment.Center,
                    FontSize = Radius / 2
                });

            ds.DrawCircle(Location, Radius, Colors.Black);
        }

        public class Link : SceneElement
        {
            public Neuron From { get; }

            public Neuron To { get; }

            public double Weight => From._nn.Weight[From._i][From._j][To._j];

            public bool ShowToolTip { get; internal set; }
            public Vector2 ToolTipPosition { get; internal set; }

            public Link(Scene scene, Neuron from, Neuron to) : base(scene)
            {
                if (from._i != to._i - 1)
                {
                    throw new ArgumentException("Neuron isn't neighbour", nameof(to));
                }

                From = from;
                To = to;

                var firstPoint = new Vector2(From.Location.X + From.Radius, From.Location.Y);
                var secondPoint = new Vector2(To.Location.X - To.Radius, To.Location.Y);
                ToolTipPosition = (secondPoint - firstPoint) / 2 + firstPoint;
            }

            protected override bool DoIntersect(ISceneElement other)
            {
                return false;
            }

            public bool Intersect(Vector2 point)
            {
                var weight = Weight;
                var normalAbsWeight = Math.Abs(Normalize(weight) * 5);
                var firstPoint = new Vector2(From.Location.X + From.Radius, From.Location.Y);
                var secondPoint = new Vector2(To.Location.X - To.Radius, To.Location.Y);
                var distance = Math.Abs((secondPoint.Y - firstPoint.Y) * point.X - (secondPoint.X - firstPoint.X) * point.Y + secondPoint.X * firstPoint.Y - secondPoint.Y * firstPoint.X) / Math.Sqrt(Math.Pow(secondPoint.Y - firstPoint.Y, 2) + Math.Pow(secondPoint.X - firstPoint.X, 2));
                if (distance <= normalAbsWeight)
                {
                    return true;
                }

                return false;
            }

            protected override void DoDraw(CanvasDrawingSession ds)
            {
                var weight = Weight;
                var normalWeight = Normalize(weight) * 5;
                ds.DrawLine(new Vector2(From.Location.X + From.Radius, From.Location.Y),
                    new Vector2(To.Location.X - To.Radius, To.Location.Y), weight < 0d ? Colors.Red : Colors.Blue, (float) normalWeight);

                if (ShowToolTip)
                {
                    ds.DrawText(
                        Weight.ToString(),
                        ToolTipPosition, Colors.Black,
                        new CanvasTextFormat()
                        {
                            HorizontalAlignment = CanvasHorizontalAlignment.Center,
                            VerticalAlignment = CanvasVerticalAlignment.Center,
                            FontSize = 11
                        });
                }
            }

            private double Normalize(double x)
            {
                x = Math.Abs(x);
                if (x < Double.Epsilon)
                {
                    return 0.01d;
                }

                return x / From._nn.MaxWeight;
            }
        }
    }
}
