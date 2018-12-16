using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork
{
    public class NeuralNetwork
    {
        private readonly Random _rnd;
        private readonly int[] _layers;

        private double[][] _activations;
        private double[][][] _weight;

        public NeuralNetwork(int[] layers, int randomSeed)
        {
            if (layers == null)
            {
                throw new ArgumentNullException(nameof(layers));
            }
            if (layers.Length < 2)
            {
                throw new ArgumentException();
            }

            _rnd = new Random(randomSeed);
            _layers = layers;
        }

        public int[] Layers => _layers;

        public double[][] Activations => _activations;

        public double[][][] Weight => _weight;

        public double MaxWeight { get; private set; } = 1d;

        public void Initialize()
        {
            _activations = new double[_layers.Length][];
            for (int i = 0; i < _layers.Length; i++)
            {
                _activations[i] = new double[_layers[i]]; //set activation value for each neuron
            }

            _weight = new double[_layers.Length - 1][][];
            for (int i = 0; i < _layers.Length - 1; i++) //by each layer without output layer
            {
                _weight[i] = new double[_layers[i]][];
                for (int j = 0; j < _layers[i]; j++) //by each neuron in current layer
                {
                    _weight[i][j] = new double[_layers[i + 1]];
                    for (int k = 0; k < _layers[i + 1]; k++) //neuron to neuron in next layer
                    {
                        _weight[i][j][k] = Random(-1d, 1d);
                        if (_weight[i][j][k] > MaxWeight)
                        {
                            MaxWeight = _weight[i][j][k];
                        }
                    }
                }
            }
        }

        public double[] Update(double[] inputs)
        {
            //https://mattmazur.com/2015/03/17/a-step-by-step-backpropagation-example/
            if (inputs.Length != _layers[0])
            {
                throw new ArgumentException(nameof(inputs));
            }

            for (int i = 0; i < inputs.Length; i++)
            {
                _activations[0][i] = inputs[i]; //set activation value for input layer
            }
            for (int i = 0; i < _layers.Length - 1; i++)
            {
                for (int k = 0; k < _layers[i + 1]; k++)
                {
                    var sum = 0d;
                    for (int j = 0; j < _layers[i]; j++)
                    {
                        sum += _activations[i][j] * _weight[i][j][k];
                    }
                    _activations[i + 1][k] = Sigmoid(sum);
                }
            }

            return _activations[_activations.Length - 1];
        }

        public double BackPropagate(double[] targets, double learningRate = 1d)
        {
            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }
            if (targets.Length != _layers[_layers.Length - 1])
            {
                throw new ArgumentException(nameof(targets));
            }

            var delta = new double[_layers.Length][];
            for (int i = 0; i < _layers.Length; i++)
            {
                delta[i] = new double[_layers[i]];
                for (int j = 0; j < _layers[i]; j++)
                {
                    delta[i][j] = 0d;
                }
            }

            //output
            var outputLayerIndex = _layers.Length - 1;
            for (int i = 0; i < _layers[outputLayerIndex]; i++)
            {
                //delta by https://en.wikipedia.org/wiki/Delta_rule
                delta[outputLayerIndex][i] = (targets[i] - _activations[outputLayerIndex][i]) * Dsigm(_activations[outputLayerIndex][i]);
            }

            //hidden
            for (int i = _layers.Length - 2; i >= 0; i--)
            {
                for (int j = 0; j < _layers[i]; j++)
                {
                    for (int k = 0; k < _layers[i + 1]; k++)
                    {
                        delta[i][j] += delta[i + 1][k] * _weight[i][j][k];
                    }
                    delta[i][j] *= Dsigm(_activations[i][j]);
                }
            }

            //update weights
            for (int i = 0; i < _layers.Length - 1; i++)
            {
                for (int j = 0; j < _layers[i]; j++)
                {
                    for (int k = 0; k < _layers[i + 1]; k++)
                    {
                        _weight[i][j][k] = _weight[i][j][k] + learningRate * delta[i + 1][k] * _activations[i][j];
                        if (_weight[i][j][k] > MaxWeight)
                        {
                            MaxWeight = _weight[i][j][k];
                        }
                    }
                }
            }
            var error = 0d;
            for (int i = 0; i < targets.Length; i++)
                error = Math.Max(Math.Abs(targets[i] - _activations[outputLayerIndex][i]), error);
            return error;
        }

        private double Random(double a, double b)
        {
            return (b - a) * _rnd.NextDouble() + a;
        }

        private double Sigmoid(double x)
        {
            return 1d / (1d + Math.Exp(-x));
        }

        private double Dsigm(double y)
        {
            return y * (1 - y);
        }
    }
}
