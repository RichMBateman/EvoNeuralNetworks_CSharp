using ShepherdCrook.Library.Experiment;
using ShepherdCrook.Library.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShepherdCrook.LearningSimpleFunctions
{
    public class XORExperiment : BaseExperiment
    {
        public XORExperiment()
        {
            ConfigExperiment cfg = new ConfigExperiment(2, 1);
            InitializeWithConfig(cfg);
        }
        protected override void EvaluateFitness(Agent a)
        {
            List<double> input0_0 = new List<double> { 0, 0 };
            List<double> input0_1 = new List<double> { 0, 1 };
            List<double> input1_0 = new List<double> { 1, 0 };
            List<double> input1_1 = new List<double> { 1, 1 };

            double fitnessTotal = 0;

            // Because of recurrent connections, take the output 3 times and divide by 3.
            for (int i = 0; i <= 2; i++)
            {

                double output0_0 = a.NeuralNetwork.ComputeActivation(input0_0)[0];
                double output0_1 = a.NeuralNetwork.ComputeActivation(input0_1)[0];
                double output1_0 = a.NeuralNetwork.ComputeActivation(input1_0)[0];
                double output1_1 = a.NeuralNetwork.ComputeActivation(input1_1)[0];

                // With this method, you will see a wide range of fitness scores.
                double fitnessAdj0_0 = .25 - output0_0;
                double fitnessAdj0_1 = output0_1 - 0.75;
                double fitnessAdj1_0 = output1_0 - 0.75;
                double fitnessAdj1_1 = .25 - output1_1;

                fitnessTotal += fitnessAdj0_0 + fitnessAdj0_1 + fitnessAdj1_0 + fitnessAdj1_1;
            }

            // I previously only added 0.25 points EXACTLY if the network got less than .1, >= 0.9 
            // I think this is a worse way to score that inhibits learning.

            a.FitnessScore = fitnessTotal / 3.0;
        }
    }
}
