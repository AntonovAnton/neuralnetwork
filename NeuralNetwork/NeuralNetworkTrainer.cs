using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeuralNetwork
{
    public abstract class NeuralNetworkTrainer
    {
        private NeuralNetwork _nn;
        private int _totalSteps;
        private double _totalError;

        private CancellationTokenSource _cancellationTokenSource;
        private Task _learningTask;

        public EventHandler<EventArgs> TrainingEnded;
        public EventHandler<TrainingStepEndedEventArgs> TrainingStepEnded;

        public void CreateNeuralNetwork()
        {
            int randomSeed = new object().GetHashCode();
            var layers = new List<int>();
            layers.Add(InputLayer);
            layers.AddRange(HiddenLayers);
            layers.Add(OutputLayer);
            var neuralNetwork = new NeuralNetwork(layers.ToArray(), randomSeed);
            neuralNetwork.Initialize();
            SetNeuralNetwork(neuralNetwork);
        }

        public void SetNeuralNetwork(NeuralNetwork nn)
        {
            #region Validate

            if (nn == null)
            {
                throw new ArgumentNullException(nameof(nn));
            }

            if (TrainingSet == null || TrainingSet.Length == 0)
            {
                throw new InvalidOperationException("Input values aren't set!");
            }

            if (Targets == null || Targets.Length == 0)
            {
                throw new InvalidOperationException("Target values aren't set!");
            }

            if (TrainingSet.Length != Targets.Length)
            {
                throw new InvalidOperationException("Count of input values don't equal count of target values!");
            }

            var inputLayer = nn.Layers[0];
            if (TrainingSet.Any(s => s.Length != inputLayer))
            {
                throw new ArgumentException("Count of inputs of the neural network don't equal count of values of the training set", nameof(nn));
            }

            var outputLayer = nn.Layers[nn.Layers.Length - 1];
            if (Targets.Any(t => t.Length != outputLayer))
            {
                throw new ArgumentException("Count of outputs of the neural network don't equal count of targets", nameof(nn));
            }

            #endregion

            _nn = nn;
            _totalSteps = 0;
            _totalError = 1d;
        }

        public abstract string Name { get; }

        public abstract int InputLayer { get; }

        public int[] HiddenLayers { get; set; } = new[] { 2, 2 };

        public abstract int OutputLayer { get; }

        public NeuralNetwork NeuralNetwork => _nn;

        public double LearningRate { get; set; } = 1d;

        public double TargetError { get; set; } = 0.005d;

        public double[][] TrainingSet { get; protected set; }

        public double[][] Targets { get; protected set; }

        public byte Speed { get; set; } = 100;

        public int TotalSteps => _totalSteps;

        public double TotalError => _totalError;

        public bool IsStarted => _learningTask != null &&
                                _learningTask.IsCanceled == false &&
                                _learningTask.IsCompleted == false &&
                                _learningTask.IsFaulted == false;
                
        public void StartTrain(int steps = Int32.MaxValue, bool overtraining = false)
        {
            if (IsStarted)
            {
                throw new InvalidOperationException("Training is already started!");
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _learningTask = Task.Run(() =>
            {
                var errors = new double[Targets.Length];
                for (int i = 0; i < errors.Length; i++)
                {
                    errors[i] = 1d;
                }
                for (int i = 0; i < steps; i++)
                {
                    _totalSteps++;

                    Task.Delay(1000 - Speed * 10).Wait();
                    var index = i % Targets.Length;

                    var input = TrainingSet[index];
                    var output = _nn.Update(input);
                    var targetOutput = Targets[index];

                    errors[index] = _nn.BackPropagate(targetOutput, LearningRate);
                    _totalError = errors.Max();

                    TrainingStepEnded?.Invoke(this, new TrainingStepEndedEventArgs(input, output, targetOutput, _totalSteps, _totalError));

                    if ((overtraining == false && _totalError < TargetError))
                    {
                        TrainingEnded?.Invoke(this, EventArgs.Empty);
                        break;
                    }

                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        break;
                    }
                }
            });
        }

        public void StopTrain()
        {
            _cancellationTokenSource?.Cancel();
        }
    }

    public class TrainingStepEndedEventArgs : EventArgs
    {
        private readonly double[] _input;
        private readonly double[] _output;
        private readonly double[] _targetOutput;
        private readonly int _step;
        private readonly double _totalError;

        public TrainingStepEndedEventArgs(double[] input, double[] output, double[] targetOutput, int step, double totalError)
            : base()
        {
            _input = input;
            _output = output;
            _targetOutput = targetOutput;
            _step = step;
            _totalError = totalError;
        }

        public double[] Input => _input;
        public double[] Output => _output;
        public double[] TargetOutput => _targetOutput;
        public int Step => _step;
        public double TotalError => _totalError;
    }
}
