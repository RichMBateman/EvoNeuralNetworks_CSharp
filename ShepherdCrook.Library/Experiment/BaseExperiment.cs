using ShepherdCrook.Library.ANN;
using ShepherdCrook.Library.Math;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShepherdCrook.Library.Experiment
{
    /// <summary>
    /// An abstract base class from which to inherit to create an experiment.
    /// </summary>
    public abstract class BaseExperiment
    {
        #region Protected Properties

        /// <summary>
        /// Whether this experiment has been initialized.
        /// </summary>
        protected bool m_isInitialized = false;
        /// <summary>
        /// A list of the absolute best agents of all time.
        /// We never use this list to populate pools; only as a reference.
        /// </summary>
        protected readonly List<Agent> m_bestAgentsAllTime = new List<Agent>();
        /// <summary>
        /// A set of isolated populations.  Populations may grow in count until it hits a limit.
        /// At that point, the worst performing populations will be killed.
        /// </summary>
        protected readonly List<Pool> m_pools = new List<Pool>();
        /// <summary>
        /// The configuration for the experiment.
        /// </summary>
        protected ConfigExperiment m_config;
        /// <summary>
        /// The number of inputs each network will always have.  Excludes the bias.
        /// This number never changes throughout the course of the experiment.
        /// </summary>
        protected int m_numInputs;
        /// <summary>
        /// The number of outputs each network will always have.
        /// This number never changes throughout the course of the experiment.
        /// </summary>
        protected int m_numOutputs;

        #endregion

        #region Public Properties

        /// <summary>
        /// The best performing agents that this experiment has seen.
        /// </summary>
        public List<Agent> BestAgents { get { return m_bestAgentsAllTime; } }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// A method that should test how this agent performs in the scenario, and assigns it a fitness score.
        /// </summary>
        protected abstract void EvaluateFitness(Agent a);

        #endregion

        #region Public Methods

        public void Run()
        {
            Stopwatch sw = Stopwatch.StartNew();
            if (!m_isInitialized) throw new Exception("Failed to initialize experiment with a configuration.");

            CreateInitialPool();
            double bestFitness = GetBestFitness();
            sw.Stop();
            Console.WriteLine("Elapsed MS: " + sw.ElapsedMilliseconds);
            sw.Restart();
            while(bestFitness < m_config.DesiredFitness)
            {
                // For each pool, for some number of iterations:
                // >Evaluate the members of the pool
                // >Add the best performers to the list of top performers
                foreach (Pool p in m_pools)
                {
                    int numGenerationIterations = m_config.NumGenerationIterations;
                    while (numGenerationIterations > 0)
                    {
                        EvaluatePool(p);
                        CreateNextGenForPool(p);
                        numGenerationIterations--;
                    }
                    // Do one final evaluation.
                    EvaluatePool(p);
                    p.SortAgentsByFitness();
                    sw.Stop();
                    Console.WriteLine("Elapsed MS for Pool Generations: " + sw.ElapsedMilliseconds);
                    sw.Restart();
                }
                bestFitness = GetBestFitness();
                // At this point, all the pools have gone through many mutations, breeding, etc.
                // It's time to make some new populations, and eliminate old ones.
                EliminateWorstPoolsIfNecessary();
                CreatePoolsWithNewTopologies();

                Console.WriteLine("Best fitness is: " + bestFitness);
                sw.Restart();
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Initialize the experiment with a configuration.
        /// </summary>
        /// <param name="config"></param>
        protected void InitializeWithConfig(ConfigExperiment config)
        {
            m_isInitialized = true;
            m_config = config;
            m_numInputs = config.NumInputs;
            m_numOutputs = config.NumOutputs;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates the first pool, which is just the minimal network.
        /// </summary>
        private void CreateInitialPool()
        {
            Pool p = new Pool();
            for(int agentIndex = 1; agentIndex <= m_config.PoolSize; agentIndex++)
            {
                Network nn = new Network(m_numInputs, m_numOutputs);
                nn.RandomizeWeights();
                Agent a = new Agent(nn);
                p.Agents.Add(a);
            }
            m_pools.Add(p);
        }

        /// <summary>
        /// Looks at the top performers and returns the absolute best fitness.
        /// If there are no top performers, returns the lowest fitness possible.
        /// </summary>
        private double GetBestFitness()
        {
            double bestFitness = Double.MinValue;
            if(m_bestAgentsAllTime.Count > 0)
            {
                SortTopPerformersList();
                bestFitness = m_bestAgentsAllTime[0].FitnessScore;
            }
            return bestFitness;
        }

        private void CreateNextGenForPool(Pool p)
        {
            p.SortAgentsByFitness();
            p.PrepareForNextGen();
            // The best agents are preserved
            for(int i = 0; i < m_config.NextGenNumToPreserve; i++)
            {
                Agent elite = p.Agents[i];
                elite.Age++;
                elite.FitnessScore = 0;
                p.AddAgentToNextGen(elite);
            }
            // Some agents are bred
            for(int i = 0; i < m_config.NextGenNumToBreed; i++)
            {
                Agent parent1 = p.Agents[RNG.Rnd(p.Agents.Count)];
                Agent parent2 = p.Agents[RNG.Rnd(p.Agents.Count)];
                Agent child = BreedTwoAgents(parent1, parent2);
                child.FitnessScore = 0;
                p.AddAgentToNextGen(child);
            }
            // Some agents are mutated (again, simple mutations)
            for (int i = 0; i < m_config.NextGenNumToMutateSimple; i++)
            {
                Agent victim = p.Agents[RNG.Rnd(p.Agents.Count)].DeepCopy();
                MutateAgentSimple(victim);
                victim.FitnessScore = 0;
                p.AddAgentToNextGen(victim);
            }

            p.MakeNextGenLive();
        }

        private void EliminateWorstPoolsIfNecessary()
        {
            int maxPoolsForCreation = (m_config.MaxPoolCount - m_config.NumNewPoolsToCreate + 1);
            if (m_pools.Count >= maxPoolsForCreation)
            {
                m_pools.Sort(delegate (Pool a, Pool b)
                {
                    return b.Agents[0].FitnessScore.CompareTo(a.Agents[0].FitnessScore);
                });
                while (m_pools.Count >= maxPoolsForCreation)
                {
                    // Removes the last pool
                    m_pools.RemoveAt(m_pools.Count - 1);
                }
            }
        }

        private void CreatePoolsWithNewTopologies()
        {
            List<Pool> newPools = new List<Pool>();
            int poolsToMake = m_config.NumNewPoolsToCreate;
            while(poolsToMake > 0)
            {
                Pool newPool = new Pool();
                Pool randomSelection = m_pools[RNG.Rnd(m_pools.Count)];
                Agent bestAgentTemplate = randomSelection.Agents[0].DeepCopy();
                double selection = RNG.Rnd();
                if(selection <= 0.40)
                {
                    bestAgentTemplate.NeuralNetwork.Mutator.MutateNewNode();
                }
                else if(selection <= 0.80)
                {
                    bestAgentTemplate.NeuralNetwork.Mutator.MutateNewLink();
                }
                else if(selection <= 0.90)
                {
                    bestAgentTemplate.NeuralNetwork.Mutator.MutateDeleteLink();
                }
                else
                {
                    bestAgentTemplate.NeuralNetwork.Mutator.MutateDeleteNode();
                }

                for(int i = 1; i <= m_config.PoolSize; i++)
                {
                    Agent a = bestAgentTemplate.DeepCopy();
                    if(RNG.Rnd() < 0.5)
                    {
                        MutateAgentSimple(a);
                    }
                    else
                    {
                        a.NeuralNetwork.RandomizeWeights();
                    }
                    newPool.Agents.Add(a);
                }
                newPools.Add(newPool);
                poolsToMake--;
            }
            m_pools.AddRange(newPools);
        }

        private void SortTopPerformersList()
        {
            m_bestAgentsAllTime.Sort(delegate (Agent a, Agent b)
            {
                return b.FitnessScore.CompareTo(a.FitnessScore);
            });
        }

        private void EvaluatePool(Pool p)
        {
            foreach(Agent a in p.Agents)
            {
                EvaluateFitness(a);
                CheckEligibilityForBestPerformerList(a);
            }
        }

        private void CheckEligibilityForBestPerformerList(Agent a)
        {
            if (m_bestAgentsAllTime.Count < m_config.BestAgentCount)
            {
                m_bestAgentsAllTime.Add(a.DeepCopy());
            }
            else
            {
                double lowestFitness = m_bestAgentsAllTime.Last().FitnessScore;
                if(a.FitnessScore > lowestFitness)
                {
                    m_bestAgentsAllTime.RemoveAt(m_bestAgentsAllTime.Count - 1);
                    m_bestAgentsAllTime.Add(a.DeepCopy());
                    SortTopPerformersList();
                }
            }
        }

        /// <summary>
        /// Performs a mutation that does not affect the topology of the network.
        /// </summary>
        /// <param name="a"></param>
        private void MutateAgentSimple(Agent a)
        {
            a.NeuralNetwork.Mutator.MutateWeight();
        }

        /// <summary>
        /// Takes two agents (with the same network topology) and breeds a child.
        /// </summary>
        private Agent BreedTwoAgents(Agent a, Agent b)
        {
            Agent child = a.DeepCopy();
            List<Link> childLinks = new List<Link>(child.NeuralNetwork.MapIdToAllLinks.Values);

            for(int l = 0; l < childLinks.Count; l++)
            {
                if(RNG.Rnd() < 0.5)
                {
                    childLinks[l].Weight = a.NeuralNetwork.MapIdToAllLinks[childLinks[l].Id].Weight;
                }
                else
                {
                    childLinks[l].Weight = b.NeuralNetwork.MapIdToAllLinks[childLinks[l].Id].Weight;
                }
            }

            return child;
        }

        private void MutateAgent(Agent a)
        {
            MutationType mutationToPerform = m_config.ChooseMutationType();
            switch(mutationToPerform)
            {
                case MutationType.AddLink: a.NeuralNetwork.Mutator.MutateNewLink(); break;
                case MutationType.AddNode: a.NeuralNetwork.Mutator.MutateNewNode(); break;
                case MutationType.DeleteLink: a.NeuralNetwork.Mutator.MutateDeleteLink(); break;
                case MutationType.DeleteNode: a.NeuralNetwork.Mutator.MutateDeleteNode(); break;
                case MutationType.ModifyWeight: a.NeuralNetwork.Mutator.MutateWeight(); break;
            }
        }

        #endregion
    }
}
