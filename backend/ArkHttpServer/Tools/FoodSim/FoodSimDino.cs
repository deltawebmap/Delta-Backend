using ArkSaveEditor.World.WorldTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer.Tools.FoodSim
{
    /// <summary>
    /// A dino in world.
    /// </summary>
    public class FoodSimDino
    {
        /// <summary>
        /// The source dinosaur
        /// </summary>
        public ArkDinosaur source;

        /// <summary>
        /// The source inventory
        /// </summary>
        public ArkInventory inventory;

        /// <summary>
        /// How much food this dino has used so far in the simulation.
        /// </summary>
        public float foodUsed;

        /// <summary>
        /// The dino age
        /// </summary>
        public float age;

        /// <summary>
        /// How much food is currently in the dino
        /// </summary>
        public float currentFood;

        /// <summary>
        /// The maximum food the dino can have
        /// </summary>
        public float maxFood;

        /// <summary>
        /// Returns true if this is a baby.
        /// </summary>
        /// <returns></returns>
        public bool GetIsBaby()
        {
            return age < 1;
        }
    }
}
