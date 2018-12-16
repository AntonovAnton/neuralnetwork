using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Game.Engine.Win2d;
using Microsoft.Graphics.Canvas.UI.Xaml;
using NeuralNetwork;
using System.Collections.ObjectModel;

namespace NeuralNetworkDemoApp
{
    public class DemoViewModel : INotifyPropertyChanged
    {
        private CanvasAnimatedControl _canvasAnimatedControl;
        private Game<DemoMoveController, NeuralNetworkScene> _game;
        private NeuralNetworkTrainer _neuralNetworkTrainer;
        private bool _overtraining = false;
        
        private string _runButtonText = "Run";
        private string _hiddenLayers = string.Empty;
        private double _learningRate = 1d;
        private byte _speedValue = 100;
        private double _targetError = 0.05d;
        private int _totalSteps;
        private double _totalError = 1d;
        private string _input;
        private string _output;
        private string _targetOutput;

        public ObservableCollection<NeuralNetworkTrainer> DemoList { get; } = new ObservableCollection<NeuralNetworkTrainer>();

        public NeuralNetworkTrainer CurrentDemo
        {
            get { return _neuralNetworkTrainer; }
            set
            {
                if (_neuralNetworkTrainer != value)
                {
                    _neuralNetworkTrainer = value;
                    RaisePropertyChanged();
                    HiddenLayers = string.Join(",", _neuralNetworkTrainer.HiddenLayers ?? new[] { 2, 2 } );
                }
            }
        }

        public string RunButtonText
        {
            get { return _runButtonText; }
            set
            {
                if (_runButtonText != value)
                {
                    _runButtonText = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string HiddenLayers
        {
            get { return _hiddenLayers; }
            set
            {
                if (_hiddenLayers != value)
                {
                    _hiddenLayers = value;
                    RaisePropertyChanged();
                }
            }
        }

        public double LearningRate
        {
            get { return _learningRate; }
            set
            {
                if (_learningRate != value)
                {
                    _learningRate = value;
                    if (CurrentDemo != null)
                    {
                        CurrentDemo.LearningRate = _learningRate;
                    }
                    RaisePropertyChanged();
                }
            }
        }

        public byte SpeedValue
        {
            get { return _speedValue; }
            set
            {
                if (_speedValue != value)
                {
                    _speedValue = value;
                    if (CurrentDemo != null)
                    {
                        CurrentDemo.Speed = _speedValue;
                    }
                    RaisePropertyChanged();
                }
            }
        }

        public double TargetError
        {
            get { return _targetError; }
            set
            {
                if (_targetError != value)
                {
                    _targetError = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int TotalSteps
        {
            get { return _totalSteps; }
            set
            {
                if (_totalSteps != value)
                {
                    _totalSteps = value;
                    RaisePropertyChanged();
                }
            }
        }

        public double TotalError
        {
            get { return _totalError; }
            set
            {
                if (_totalError != value)
                {
                    _totalError = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region Checking properties

        public string Input
        {
            get { return _input; }
            set
            {
                if (_input != value)
                {
                    _input = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string Output
        {
            get { return _output; }
            set
            {
                if (_output != value)
                {
                    _output = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string TargetOutput
        {
            get { return _targetOutput; }
            set
            {
                if (_targetOutput != value)
                {
                    _targetOutput = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        public bool IsStarted
        {
            get { return _neuralNetworkTrainer == null ? false : _neuralNetworkTrainer.IsStarted; }
        }

        public void Initialize(CanvasAnimatedControl canvasAnimatedControl)
        {
            _canvasAnimatedControl = canvasAnimatedControl;
            _game = new Game<DemoMoveController, NeuralNetworkScene>(_canvasAnimatedControl);
            RunButtonText = "Run train";

            DemoList.Add(new Xor2Trainer());
            DemoList.Add(new Xor3Trainer());
            //DemoList.Add(new QratorTaskTrainer());
            CurrentDemo = DemoList[0];
        }

        internal async Task InitTrain()
        {
            await Reset();

            CurrentDemo.TrainingEnded -= TrainingEnded;
            CurrentDemo.TrainingEnded += TrainingEnded;

            if (string.IsNullOrWhiteSpace(HiddenLayers) == false)
            {
                CurrentDemo.HiddenLayers = HiddenLayers.Split(',').Select(l => int.Parse(l)).ToArray();
            }
            else
            {
                CurrentDemo.HiddenLayers = new int[] { };
            }
            CurrentDemo.Speed = SpeedValue;
            CurrentDemo.LearningRate = LearningRate;
            CurrentDemo.TargetError = TargetError;
            CurrentDemo.CreateNeuralNetwork();
            TotalSteps = CurrentDemo.TotalSteps;
            TotalError = CurrentDemo.TotalError;

            Input = string.Join(",", CurrentDemo.TrainingSet[0]);
            Output = string.Join(",", CurrentDemo.NeuralNetwork.Update(CurrentDemo.TrainingSet[0]));
            TargetOutput = string.Join(",", CurrentDemo.Targets[0]);

            var scene = new NeuralNetworkScene(CurrentDemo.NeuralNetwork);
            await _game.Start(scene);
        }
        
        public void StartTrain()
        {
            CurrentDemo.TrainingStepEnded -= StepEnded;
            CurrentDemo.TrainingStepEnded += StepEnded;

            RunButtonText = "Pause";
            CurrentDemo.StartTrain(int.MaxValue, _overtraining);
        }

        public void StopTrain()
        {
            CurrentDemo.TrainingStepEnded -= StepEnded;

            RunButtonText = _overtraining ? "Run overtrain" : "Run train";
            CurrentDemo.StopTrain();
        }

        private async Task Reset()
        {
            foreach (var demo in DemoList)
            {
                if (demo.IsStarted)
                {
                    demo.StopTrain();
                }
            }

            await _game.Stop();
            _overtraining = false;
            RunButtonText = "Run train";
        }

        private void TrainingEnded(object sender, EventArgs e)
        {
            _neuralNetworkTrainer.TrainingEnded -= TrainingEnded;
            _neuralNetworkTrainer.TrainingStepEnded -= StepEnded;

            RunButtonText = "Run overtrain";
            _overtraining = true;
        }

        private async void StepEnded(object sender, TrainingStepEndedEventArgs e)
        {
            if(CurrentDemo == null || CurrentDemo.IsStarted == false)
            {
                return;
            }

            TotalSteps = e.Step;
            TotalError = e.TotalError;
            Input = string.Join(",", e.Input);
            Output = string.Join(",", e.Output);
            TargetOutput = string.Join(",", e.TargetOutput);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            //Accessing Xaml objects should be done from the UI thread.
            var handler = PropertyChanged;
            if (handler != null)
            {
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    handler.Invoke(this, new PropertyChangedEventArgs(propertyName));
                });
            }
        }
    }
}