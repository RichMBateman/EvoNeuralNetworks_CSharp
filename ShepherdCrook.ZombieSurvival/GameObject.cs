using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShepherdCrook.ZombieSurvival
{
    public enum GameObjectType
    {
        Human,
        Zombie,
        Bullet,
        Ammo
    }
    public class GameObject
    {
        public int PosX;
        public GameObjectType Type;
        public Direction Dir;
        public int TicksRemaining;
        public bool IsPermanent;
    }
}
