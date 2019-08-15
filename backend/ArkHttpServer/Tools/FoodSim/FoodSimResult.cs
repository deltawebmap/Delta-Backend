using ArkSaveEditor.World.WorldTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer.Tools.FoodSim
{
    /// <summary>
    /// Returned by the simulation when it is complete
    /// </summary>
    public class FoodSimResult
    {
        /// <summary>
        /// The source dino
        /// </summary>
        public ArkDinosaur source;

        /// <summary>
        /// When the food for the dino will be fully depleted. In seconds, from the start of the simulation
        /// </summary>
        public float foodDepletionTime;

        /// <summary>
        /// The age of the dino when the food runs out.
        /// </summary>
        public float ageWhenFoodDepletes;

        /// <summary>
        /// The total food energy used
        /// </summary>
        public float foodUsed;
    }
}
