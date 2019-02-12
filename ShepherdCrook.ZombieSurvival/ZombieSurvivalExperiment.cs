using ShepherdCrook.Library.Experiment;
using System.Collections.Generic;

namespace ShepherdCrook.ZombieSurvival
{
    public class ZombieSurvivalExperiment : BaseExperiment
    {
        private const int NumInputs = 54;
        private const int NumOutputs = 4;
        private readonly GameWorld m_gameWorld = new GameWorld();

        public ZombieSurvivalExperiment()
        {
            ConfigExperiment cfg = new ConfigExperiment(NumInputs, NumOutputs);
            InitializeWithConfig(cfg);
        }

        public GameWorld GameWorld { get { return m_gameWorld; } }

        public List<double> GetAgentOutput(Agent a, List<double> input, 
            out double walkRightActivation, out double walkLeftActivation,
            out double shootRightActivation, out double shootLeftActivation)
        {
            List<double> output = a.NeuralNetwork.ComputeActivation(input);
            walkRightActivation = output[0];
            walkLeftActivation = output[1];
            shootRightActivation = output[2];
            shootLeftActivation = output[3];
            return output;
        }

        protected override void EvaluateFitness(Agent a)
        {
            m_gameWorld.Reset();
            while(!m_gameWorld.SimulationOver)
            {
                List<double> input = BuildInputArrayForWorld();
                double walkRightActivation;
                double walkLeftActivation;
                double shootRightActivation;
                double shootLeftActivation;
                List<double> output = GetAgentOutput(a, input, out walkRightActivation,
                    out walkLeftActivation, out shootRightActivation, out shootLeftActivation);

                Direction toWalk = Direction.None;
                Direction toShoot = Direction.None;
                GetAgentDecisions(walkRightActivation, walkLeftActivation, shootRightActivation, shootLeftActivation,
                    out toWalk, out toShoot);



                m_gameWorld.Tick(toWalk, toShoot);
            }

            a.FitnessScore = m_gameWorld.Score;
        }

        public void GetAgentDecisions(double walkRightActivation, double walkLeftActivation,
            double shootRightActivation, double shootLeftActivation,
            out Direction toWalk, out Direction toShoot)
        {
            toWalk = Direction.None;
            if (walkRightActivation > 0.10 || walkLeftActivation > 0.10)
            {
                toWalk = (walkRightActivation > walkLeftActivation ? Direction.Right : Direction.Left);
            }

            toShoot = Direction.None;
            if (shootRightActivation > 0.10 || shootLeftActivation > 0.10)
            {
                toWalk = (shootRightActivation > shootLeftActivation ? Direction.Right : Direction.Left);
            }
        }

        public List<double> BuildInputArrayForWorld()
        {
            double[] inputArrayZomLeft = new double[8];
            double[] inputArrayZomRight = new double[8];
            double[] inputArrayBulletLeft = new double[8];
            double[] inputArrayBulletRight = new double[8];
            double[] inputArrayAmmoLeft = new double[8];
            double[] inputArrayAmmoRight= new double[8];
            bool isZombieLeft = false, isZombieRight = false,
                isBulletLeft = false, isBulletRight = false, 
                isAmmoLeft=false, isAmmoRight=false;

            foreach (GameObject go in m_gameWorld.allObjects)
            {
                int distance;
                Direction direction;
                switch(go.Type)
                {
                    case GameObjectType.Zombie:
                        m_gameWorld.CalcDistAndDirectionFromHuman(go, out distance, out direction);
                        if(distance < GameWorld.HumanVision)
                        {
                            switch (direction)
                            {
                                case Direction.Left:
                                    isZombieLeft = true;
                                    inputArrayZomLeft[distance] = 1;
                                    break;
                                case Direction.Right:
                                    isZombieRight = true;
                                    inputArrayZomRight[distance] = 1;
                                    break;
                            }
                        }
                        break;
                    case GameObjectType.Bullet:
                        m_gameWorld.CalcDistAndDirectionFromHuman(go, out distance, out direction);
                        if (distance < GameWorld.HumanVision)
                        {
                            switch (direction)
                            {
                                case Direction.Left:
                                    isBulletLeft = true;
                                    inputArrayBulletLeft[distance] = 1;
                                    break;
                                case Direction.Right:
                                    isBulletRight = true;
                                    inputArrayBulletRight[distance] = 1;
                                    break;
                            }
                        }
                        break;
                    case GameObjectType.Ammo:
                        m_gameWorld.CalcDistAndDirectionFromHuman(go, out distance, out direction);
                        if (distance < GameWorld.HumanVision)
                        {
                            switch (direction)
                            {
                                case Direction.Left:
                                    isAmmoLeft = true;
                                    inputArrayAmmoLeft[distance] = 1;
                                    break;
                                case Direction.Right:
                                    isAmmoRight = true;
                                    inputArrayAmmoRight[distance] = 1;
                                    break;
                            }
                        }
                        break;
                }
            }

            List<double> inputArray = new List<double>();
            inputArray.Add((isZombieLeft ? 1 : 0));
            inputArray.AddRange(inputArrayZomLeft);
            inputArray.Add((isZombieRight ? 1 : 0));
            inputArray.AddRange(inputArrayZomRight);
            inputArray.Add((isBulletLeft ? 1 : 0));
            inputArray.AddRange(inputArrayBulletLeft);
            inputArray.Add((isBulletRight ? 1 : 0));
            inputArray.AddRange(inputArrayBulletRight);
            inputArray.Add((isAmmoLeft ? 1 : 0));
            inputArray.AddRange(inputArrayAmmoLeft);
            inputArray.Add((isAmmoRight ? 1 : 0));
            inputArray.AddRange(inputArrayAmmoRight);
            return inputArray;
        }
    }
}
