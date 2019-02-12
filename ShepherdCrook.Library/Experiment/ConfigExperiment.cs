using ShepherdCrook.Library.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShepherdCrook.Library.Experiment
{
    public enum MutationType
    {
        None,
        ModifyWeight,
        AddLink,
        AddNode,
        DeleteLink,
        DeleteNode,
    }
    public class ConfigExperiment
    {
        #region Constants / Defaults

        public const double DefaultDesiredFitness = 0.80;

        public const int DefaultMaxPoolCount = 500;
        public const int DefaultBestAgentsCount = 100;
        public const int DefaultPoolSize = 25;
        public const int DefaultNextGenNumToPreserve = 5;
        public const int DefaultNextGenNumToBreed = 15;
        public const int DefaultNextGenNumToMutateSimple = 5;
        public const int DefaultNumNewPoolsToCreate = 10;

        public const int DefaultNumGenerationIterations = 100;

        public const int DefaultMutationAmountModifyWeight = 100;
        public const int DefaultMutationAmountAddLink = 10;
        public const int DefaultMutationAmountAddNode = 10;
        public const int DefaultMutationAmountDeleteLink = 5;
        public const int DefaultMutationAmountDeleteNode = 5;

        public const bool DefaultEnableNetworkVerifications = true;

        #endregion

        #region Private Members

        private int m_mutationAmountTotal = 0;

        private int m_mutationAmountModifyWeightScaled = 0;
        private int m_mutationAmountAddLinkScaled = 0;
        private int m_mutationAmountAddNodeScaled = 0;
        private int m_mutationAmountDeleteLinkScaled = 0;
        private int m_mutationAmountDeleteNodeScaled = 0;

        private int m_mutationAmountModifyWeight = 0;
        private int m_mutationAmountAddLink = 0;
        private int m_mutationAmountAddNode = 0;
        private int m_mutationAmountDeleteLink = 0;
        private int m_mutationAmountDeleteNode = 0;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the experiment configuration.
        /// Arguments supplied to the constructor represent non-optional parameters
        /// (i.e., parameters for which no sensible default exists).
        /// </summary>
        public ConfigExperiment(int numInputs, int numOutputs)
        {
            EnableNetworkVerifications = DefaultEnableNetworkVerifications;

            DesiredFitness = DefaultDesiredFitness;

            NumInputs = numInputs;
            NumOutputs = numOutputs;

            MaxPoolCount = DefaultMaxPoolCount;
            BestAgentCount = DefaultBestAgentsCount;
            PoolSize = DefaultPoolSize;

            NumGenerationIterations = DefaultNumGenerationIterations;

            MutationAmountModifyWeight = DefaultMutationAmountModifyWeight;
            MutationAmountAddLink = DefaultMutationAmountAddLink;
            MutationAmountAddNode = DefaultMutationAmountAddNode;
            MutationAmountDeleteLink = DefaultMutationAmountDeleteLink;
            MutationAmountDeleteNode = DefaultMutationAmountDeleteNode;

            NextGenNumToPreserve = DefaultNextGenNumToPreserve;
            NextGenNumToBreed = DefaultNextGenNumToBreed;
            NextGenNumToMutateSimple = DefaultNextGenNumToMutateSimple;

            NumNewPoolsToCreate = DefaultNumNewPoolsToCreate;
    }

        #endregion

        #region Public Properties

        /// <summary>
        /// Whether the network should perform verification on its topology to look for errors.
        /// This is a feature useful for debugging, but shouldn't be enabled when performance is a concern.
        /// </summary>
        public bool EnableNetworkVerifications { get; set; }

        /// <summary>
        /// Defines the key stopping criteria for the algorithm.  It will keep training until it 
        /// evolves a network with the desired fitness.
        /// </summary>
        public double DesiredFitness { get; set; }

        public int NumInputs { get; set; }
        public int NumOutputs { get; set; }

        public int MaxPoolCount { get; set; }
        public int BestAgentCount { get; set; }
        public int PoolSize { get; set; }

        public int NextGenNumToPreserve { get; set; }
        public int NextGenNumToBreed { get; set; }
        public int NextGenNumToMutateSimple { get; set; }
        public int NumNewPoolsToCreate { get; set; }

        public int NumGenerationIterations { get; set; }

        public int MutationAmountModifyWeight
        {
            get { return m_mutationAmountModifyWeight; }
            set
            {
                m_mutationAmountModifyWeight = value;
                UpdateMutationAmounts();
            }
        }
        public int MutationAmountAddLink
        {
            get { return m_mutationAmountAddLink; }
            set
            {
                m_mutationAmountAddLink = value;
                UpdateMutationAmounts();
            }
        }
        public int MutationAmountAddNode
        {
            get { return m_mutationAmountAddNode; }
            set
            {
                m_mutationAmountAddNode = value;
                UpdateMutationAmounts();
            }
        }
        public int MutationAmountDeleteLink
        {
            get { return m_mutationAmountDeleteLink; }
            set
            {
                m_mutationAmountDeleteLink = value;
                UpdateMutationAmounts();
            }
        }
        public int MutationAmountDeleteNode
        {
            get { return m_mutationAmountDeleteNode; }
            set
            {
                m_mutationAmountDeleteNode = value;
                UpdateMutationAmounts();
            }
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Choose a random mutation type, based on the amounts assigned to each type.
        /// The higher an amount, the more likely that type will be selected.
        /// </summary>
        public MutationType ChooseMutationType()
        {
            int selection = RNG.Rnd(m_mutationAmountTotal);

            if (selection < m_mutationAmountModifyWeightScaled) return MutationType.ModifyWeight;
            if (selection < m_mutationAmountAddLinkScaled) return MutationType.AddLink;
            if (selection < m_mutationAmountAddNodeScaled) return MutationType.AddNode;
            if (selection < m_mutationAmountDeleteLinkScaled) return MutationType.DeleteLink;
            if (selection < m_mutationAmountDeleteNodeScaled) return MutationType.DeleteNode;

            return MutationType.None;
        }

        #endregion

        #region Private Methods

        private void UpdateMutationAmounts()
        {
            m_mutationAmountTotal = MutationAmountModifyWeight +
                    MutationAmountAddLink +
                    MutationAmountAddNode +
                    MutationAmountDeleteLink +
                    MutationAmountDeleteNode;

            m_mutationAmountModifyWeightScaled = MutationAmountModifyWeight;
            m_mutationAmountAddLinkScaled = m_mutationAmountModifyWeightScaled + MutationAmountAddLink;
            m_mutationAmountAddNodeScaled = m_mutationAmountAddLinkScaled + MutationAmountAddNode;
            m_mutationAmountDeleteLinkScaled = m_mutationAmountAddNodeScaled + MutationAmountDeleteLink;
            m_mutationAmountDeleteNodeScaled = m_mutationAmountDeleteLinkScaled + MutationAmountDeleteNode;
        }

        #endregion
    }
}
