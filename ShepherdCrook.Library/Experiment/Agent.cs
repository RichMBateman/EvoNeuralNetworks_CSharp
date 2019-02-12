using ShepherdCrook.Library.ANN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShepherdCrook.Library.Experiment
{
    /// <summary>
    /// Represents an intelligent (we hope) actor in an experiment.
    /// Uses a neural network to make decisions.
    /// </summary>
    public class Agent
    {
        #region Private Members

        private readonly Network m_network;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new agent given a network.
        /// </summary>
        public Agent(Network n)
        {
            m_network = n;
            Age = 0;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The neural network brain of this agent.
        /// </summary>
        public Network NeuralNetwork { get { return m_network; } }
        /// <summary>
        /// A score representing the latest fitness of this individual.
        /// </summary>
        public double FitnessScore { get; set; }
        /// <summary>
        /// How old is this agent?
        /// </summary>
        public int Age { get; set; }

        #endregion

        #region Public Methods

        public Agent DeepCopy()
        {
            Network n = m_network.Copy();
            Agent a = new Agent(n);
            a.FitnessScore = FitnessScore;
            return a;
        }

        public override string ToString()
        {
            return "Id#" + m_network.Id + ", F=" + FitnessScore.ToString("F3");
        }

        #endregion
    }
}
