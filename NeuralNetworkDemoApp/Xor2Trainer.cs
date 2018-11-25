using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeuralNetwork;

namespace NeuralNetworkDemoApp
{
    public class Xor2Trainer : NeuralNetworkTrainer
    {
        public override string Name => "Xor logic - 2 inputs";

        public override int InputLayer => 2;
        
        public override int OutputLayer => 1;

        public Xor2Trainer()
        {
            HiddenLayers = new[] { 4, 4 };

            var trainingset = new double[4][];
            trainingset[0] = new double[] { 0, 0 };
            trainingset[1] = new double[] { 0, 1 };
            trainingset[2] = new double[] { 1, 0 };
            trainingset[3] = new double[] { 1, 1 };
            TrainingSet = trainingset;

            var targets = new double[4][];
            targets[0] = new double[] { 0 };
            targets[1] = new double[] { 1 };
            targets[2] = new double[] { 1 };
            targets[3] = new double[] { 0 };
            Targets = targets;
        }
    }
}
