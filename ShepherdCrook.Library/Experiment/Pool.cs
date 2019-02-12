using System.Collections.Generic;

namespace ShepherdCrook.Library.Experiment
{
    /// <summary>
    /// A group of networks.
    /// </summary>
    public class Pool
    {
        private List<Agent> m_agentsBufferLive;
        private List<Agent> m_agentsBufferNextGen;
        private readonly List<Agent> m_agentsBufferA = new List<Agent>();
        private readonly List<Agent> m_agentsBufferB = new List<Agent>();

        public List<Agent> Agents { get { return m_agentsBufferLive; } }

        public Pool()
        {
            m_agentsBufferLive = m_agentsBufferA;
            m_agentsBufferNextGen = m_agentsBufferB;
        }

        public void SortAgentsByFitness()
        {
            m_agentsBufferLive.Sort(delegate (Agent a, Agent b)
            {
                return b.FitnessScore.CompareTo(a.FitnessScore);
            });
        }

        public void PrepareForNextGen()
        {
            m_agentsBufferNextGen.Clear();
        }

        public void AddAgentToNextGen(Agent a)
        {
            m_agentsBufferNextGen.Add(a);
        }

        public void MakeNextGenLive()
        {
            List<Agent> previousLive = m_agentsBufferLive;
            m_agentsBufferLive = m_agentsBufferNextGen;
            m_agentsBufferNextGen = previousLive;
        }
    }
}
