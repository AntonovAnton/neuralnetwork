using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeuralNetwork;

namespace NeuralNetworkDemoApp
{
    public class Xor3Trainer : NeuralNetworkTrainer
    {
        public override string Name => "Xor logic - 3 inputs";

        public override int InputLayer => 3;
                
        public override int OutputLayer => 1;

        public Xor3Trainer()
        {
            HiddenLayers = new[] { 4, 4 };

            var trainingset = new double[8][];
            trainingset[0] = new double[] { 0, 0, 0 };
            trainingset[1] = new double[] { 0, 0, 1 };
            trainingset[2] = new double[] { 0, 1, 0 };
            trainingset[3] = new double[] { 0, 1, 1 };
            trainingset[4] = new double[] { 1, 0, 0 };
            trainingset[5] = new double[] { 1, 0, 1 };
            trainingset[6] = new double[] { 1, 1, 0 };
            trainingset[7] = new double[] { 1, 1, 1 };
            TrainingSet = trainingset;

            var targets = new double[8][];
            targets[0] = new double[] { 0 };
            targets[1] = new double[] { 1 };
            targets[2] = new double[] { 1 };
            targets[3] = new double[] { 0 };
            targets[4] = new double[] { 1 };
            targets[5] = new double[] { 0 };
            targets[6] = new double[] { 0 };
            targets[7] = new double[] { 1 };
            Targets = targets;
        }
    }
}
