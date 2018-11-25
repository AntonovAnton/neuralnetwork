using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409
namespace NeuralNetworkDemoApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private DemoViewModel _viewModel;

        public MainPage()
        {
            this.InitializeComponent();
            DataContextChanged += OnMainPageDataContextChanged;
            VisualisationControl.CreateResources += VisualisationControlCreateResources;
        }

        private async void VisualisationControlCreateResources(CanvasAnimatedControl sender, CanvasCreateResourcesEventArgs args)
        {
            await _viewModel.InitTrain();
            DemoTasks.SelectionChanged += CurrentDemoChanged;
        }

        private void OnMainPageDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            DataContextChanged -= OnMainPageDataContextChanged;
            _viewModel = (DemoViewModel)DataContext;
            _viewModel.Initialize(VisualisationControl);
        }

        private void RunButtonOnClick(object sender, RoutedEventArgs e)
        {
            if (_viewModel.IsStarted)
            {
                _viewModel.StopTrain();
            }
            else
            {
                _viewModel.StartTrain();
            }
        }

        private async void InitButtonClick(object sender, RoutedEventArgs e)
        {
            await _viewModel.InitTrain();
        }

        private void SpeedValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            _viewModel.SpeedValue = (byte)e.NewValue;
        }

        private async void CurrentDemoChanged(object sender, SelectionChangedEventArgs e)
        {
            await _viewModel.InitTrain();
        }

        private void CheckButtonClick(object sender, RoutedEventArgs e)
        {
            double[] input = _viewModel.Input.Split(',').Select(l => double.Parse(l)).ToArray();
            double[] output = _viewModel.CurrentDemo.NeuralNetwork.Update(input);
            _viewModel.Output = string.Join(",", output);
        }
    }
}
