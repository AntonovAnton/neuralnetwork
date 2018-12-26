using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using NeuralNetwork;
using Windows.Storage;

namespace NeuralNetworkDemoApp
{
    public class QratorTaskTrainer : NeuralNetworkTrainer
    {
        public override string Name => "Qrator task";

        public override int InputLayer => 1;
                
        public override int OutputLayer => 1;

        public QratorTaskTrainer()
        {
            HiddenLayers = new[] { 2, 2 };

            Initialize().Wait();
        }

        private async Task Initialize()
        {
            StorageFile file = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync("segments_study.csv");
            var text = await FileIO.ReadTextAsync(file);
            var strings = text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var dirty = new double[strings.Length - 1][];
            for (int i = 1; i < strings.Length; i++)
            {
                var s = strings[i].Split(',');
                var minutes = double.Parse(s[2]) * 60 + double.Parse(s[3]); //1440 min in day
                dirty[i - 1] = new double[] { minutes, double.Parse(s[4]), double.Parse(s[5]), double.Parse(s[6]) };
            }

            #region REMOVE ANOMALIES

            var skipCount = 10;
            var v0 = dirty.OrderBy(d => d[0]).ThenBy(d => d[1]).ToArray();
            for (int i = 0; i < v0.Length; i++)
            {
                if (i < skipCount || i > v0.Length - skipCount)
                {
                    v0[i] = new double[0];
                }
                else

                //remove anomalies first and last 5 rows
                if (Math.Abs(v0[i][0] - v0[i + 1][0]) > double.Epsilon)
                {
                    for (int j = -skipCount; j < skipCount; j++)
                    {
                        v0[i + j] = new double[0];
                    }
                    i += skipCount;
                }
            }

            var v1 = dirty.OrderBy(d => d[0]).ThenBy(d => d[2]).ToArray();
            for (int i = 0; i < v1.Length; i++)
            {
                if (i < skipCount || i > v1.Length - skipCount)
                {
                    v1[i] = new double[0];
                }
                else

                //remove anomalies first and last 5 rows
                if (Math.Abs(v1[i][0] - v1[i + 1][0]) > double.Epsilon)
                {
                    for (int j = -skipCount; j < skipCount; j++)
                    {
                        v1[i + j] = new double[0];
                    }
                    i += skipCount;
                }
            }

            var v2 = dirty.OrderBy(d => d[0]).ThenBy(d => d[3]).ToArray();
            for (int i = skipCount; i < v2.Length; i++)
            {
                if (i < skipCount || i > v2.Length - skipCount)
                {
                    v2[i] = new double[0];
                }
                else

                //remove anomalies first and last 5 rows
                if (Math.Abs(v2[i][0] - v2[i + 1][0]) > double.Epsilon)
                {
                    for (int j = -skipCount; j < skipCount; j++)
                    {
                        v2[i + j] = new double[0];
                    }
                    i += skipCount;
                }
            }

            #endregion

            #region AVERAGE BY MINUTES OD DAY

            var aver = new List<double[]>();
            var count = 0;
            var v0sum = 0d;
            var v1sum = 0d;
            var v2sum = 0d;
            for (int i = 0; i < dirty.Length; i++)
            {
                if(v0[i].Length == 0)
                {
                    if (count != 0)
                    {
                        aver.Add(new double[] { v0[i - 1][0], v0sum / count, v1sum / count, v2sum / count });
                    }
                    count = 0;
                    v0sum = 0;
                    v1sum = 0d;
                    v2sum = 0d;
                }
                else
                {
                    v0sum += v0[i][1];
                    v1sum += v1[i][2];
                    v2sum += v2[i][3];
                    count++;
                }
            }

            #endregion

            #region NORMALIZATION

            var trainingset = new double[aver.Count][];
            var targets = new double[aver.Count][];
            for (int i = 0; i < aver.Count; i++)
            {
                trainingset[i] = new double[] { aver[i][0] / 1440 }; //1440 min in day
                var normalized = Vector2.Normalize(new Vector2(1f, (float)aver[i][3]));
                targets[i] = new double[] { normalized.Y };
            }

            #endregion

            TrainingSet = trainingset;
            Targets = targets;
        }
    }
}
