using ShepherdCrook.Library.Math;
using System.Collections.Generic;

namespace ShepherdCrook.ZombieSurvival
{
    public enum Direction
    {
        None,
        Left,
        Right,
    }
    public class GameWorld
    {
        public const int WorldLength = 32; // Cells are from 0-31
        public const int HumanVision = 6; // The human can see 6 cells in front and 6 behind.
        public GameObject Human;
        public readonly List<GameObject> allObjects = new List<GameObject>();
        public readonly List<GameObject> ammoPickups = new List<GameObject>();
        public readonly List<GameObject> zombies = new List<GameObject>();
        public readonly List<GameObject> bullets = new List<GameObject>();
        public int CurrentAmmo;
        public int Score;
        public int MaxDuration = 600;
        public int Duration;
        public bool SimulationOver;

        public GameWorld()
        {
            Human = new GameObject();
            Human.IsPermanent = true;
            Human.Type = GameObjectType.Human;
            Reset();
        }

        public void Reset()
        {
            SimulationOver = false;

            Human.PosX = 16;
            CurrentAmmo = 6;
            Score = 0;
            Duration = MaxDuration;

            allObjects.Clear();
            ammoPickups.Clear();
            zombies.Clear();
            bullets.Clear();
            allObjects.Add(Human);
        }

        public void CalcDistAndDirectionFromHuman(GameObject tgt, out int distance, out Direction dir)
        {
            int leftDistance = Human.PosX - tgt.PosX;
            int rightDistance = tgt.PosX - Human.PosX;
            if (leftDistance < 0) leftDistance += WorldLength;
            if (rightDistance < 0) rightDistance += WorldLength;
            if(leftDistance < rightDistance)
            {
                distance = leftDistance;
                dir = Direction.Left;
            }
            else
            {
                distance = rightDistance;
                dir = Direction.Right;
            }
        }

        public void Tick(Direction move, Direction shoot)
        {
            // Spawn ammo and zombies.
            SpawnAmmo();
            SpawnZombies();

            // Set human's direction
            Human.Dir = move;

            // Move all game objects
            foreach (GameObject go in allObjects)
            {
                MoveGameObject(go);
            }

            // Attempt to shoot
            if (shoot != Direction.None && CurrentAmmo > 0)
            {
                CurrentAmmo--;
                GameObject bullet = new GameObject();
                bullet.Type = GameObjectType.Bullet;
                bullet.PosX = Human.PosX;
                bullet.Dir = shoot;
                bullet.TicksRemaining = 8;
                bullets.Add(bullet);
                allObjects.Add(bullet);
            }
            // Check for all collisions
            if(Duration <= 0 || CheckForHumanDeath())
            {
                SimulationOver = true;
            }
            else
            {
                CheckForShotZombies();
                CheckForAmmoPickup();

                RemoveExpiredObjects(allObjects);
                RemoveExpiredObjects(zombies);
                RemoveExpiredObjects(ammoPickups);
                RemoveExpiredObjects(bullets);

                Score++;
                Duration--;
            }


        }

        private void RemoveExpiredObjects(List<GameObject> objects)
        {
            List<GameObject> toRemove = new List<GameObject>();
            foreach(GameObject go in objects)
            {
                if(!go.IsPermanent && go.TicksRemaining <= 0)
                {
                    toRemove.Add(go);
                }
            }
            foreach(GameObject removeMe in toRemove)
            {
                objects.Remove(removeMe);
            }
        }

        private void CheckForAmmoPickup()
        {
            foreach (GameObject ammo in ammoPickups)
            {
                if (ammo.PosX == Human.PosX)
                {
                    ammo.IsPermanent = false;
                    ammo.TicksRemaining = 0;
                    CurrentAmmo += 6;
                }
            }
        }

        private void CheckForShotZombies()
        {
            foreach(GameObject bullet in bullets)
            {
                foreach(GameObject zombie in zombies)
                {
                    if(bullet.PosX == zombie.PosX)
                    {
                        zombie.IsPermanent = false;
                        zombie.TicksRemaining = 0;
                        bullet.TicksRemaining = 0;
                    }
                }
            }
        }

        private bool CheckForHumanDeath()
        {
            bool isHumanDead = false;
            foreach(GameObject zombie in zombies)
            {
                if(zombie.PosX == Human.PosX)
                {
                    isHumanDead = true;
                    break;
                }
            }
            return isHumanDead;
        }

        private void MoveGameObject(GameObject go)
        {
            int position = go.PosX;
            switch (go.Dir)
            {
                case Direction.Left:
                    position--;
                    break;
                case Direction.Right:
                    position++;
                    break;
                case Direction.None:
                    break;
            }
            
            go.PosX = CorrectPosition(position);
            if (!go.IsPermanent) go.TicksRemaining--;
        }

        private int GetOutOfSightPosition()
        {
            int position = Human.PosX;
            // First decide whether you want left or right.
            int sign = (RNG.Rnd() < 0.5 ? -1 : +1);
            // Put object vision x2 away from human.
            position += (2 * HumanVision);
            position = CorrectPosition(position);

            return position;
        }

        private Direction ChooseRandomDirection()
        {
            return (RNG.Rnd() < 0.5 ? Direction.Left : Direction.Right);
        }

        private int CorrectPosition(int position)
        {
            int corrected = position;
            if (position < 0) corrected = position + WorldLength - 1;
            if (position >= WorldLength) corrected = position - WorldLength;
            return corrected;
        }

        private void SpawnAmmo()
        {
            if(ammoPickups.Count == 0)
            {
                GameObject ammo = new GameObject();
                ammo.Type = GameObjectType.Ammo;
                ammo.Dir = Direction.None;
                ammo.PosX = GetOutOfSightPosition();
                this.ammoPickups.Add(ammo);
                this.allObjects.Add(ammo);
            }
        }

        private void SpawnZombies()
        {
            if (zombies.Count < 2)
            {
                GameObject z = new GameObject();
                z.Type = GameObjectType.Zombie;
                z.Dir = ChooseRandomDirection();
                z.PosX = GetOutOfSightPosition();
                this.zombies.Add(z);
                this.allObjects.Add(z);
            }
        }
    }
}
