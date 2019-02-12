using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShepherdCrook.Library.ANN;
using System.Collections.Generic;
using ShepherdCrook.Library.Math;

namespace ShepherdCrook.UnitTests
{
    [TestClass]
    public class TestNetworks
    {
        [TestMethod]
        public void TestNetwork1()
        {
            List<Network> networks = ANNIO.LoadFromFile("TestFiles\\TestNetwork1.txt");
            Assert.AreEqual(networks.Count, 1);
            Network network = networks[0];

            List<Double> inputVector = new List<Double> { 0.0};
            List<Double> outputVector = network.ComputeActivation(inputVector);
            Assert.IsTrue(ActivationFunctions.DoublesEqualWithEpsilon(outputVector[0], 0.98201379));

            inputVector = new List<Double> { 1.0 };
            outputVector = network.ComputeActivation(inputVector);
            Assert.IsTrue(ActivationFunctions.DoublesEqualWithEpsilon(outputVector[0], 0.995929862));
        }

        //[TestMethod]
        //public void TestNetwork2()
        //{
        //    List<Network> networks = ANNIO.LoadFromFile("TestFiles\\TestNetwork2.txt");
        //    Assert.AreEqual(networks.Count, 1);
        //    Network network = networks[0];

        //    List<Double> inputVector = new List<Double> { 0.0 };
        //    List<Double> outputVector = network.ComputeActivation(inputVector);
        //    Assert.IsTrue(ActivationFunctions.DoublesEqualWithEpsilon(outputVector[0], 0.98201379));

        //    inputVector = new List<Double> { 1.0 };
        //    outputVector = network.ComputeActivation(inputVector);
        //    Assert.IsTrue(ActivationFunctions.DoublesEqualWithEpsilon(outputVector[0], 0.995929862));
        //}
    }
}
