using ArkSaveEditor.Entities.LowLevel.DotArk;
using ArkSaveEditor.World.WorldTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer.Tools.FoodSim
{
    public class FoodSimFeedingBin
    {
        /// <summary>
        /// The max range, in game units, of this feeding trough.
        /// </summary>
        public const float MAX_FEEDING_RANGE = 1;
        
        /// <summary>
        /// The linked structure to this.
        /// </summary>
        public ArkStructure source;

        /// <summary>
        /// The source inventory
        /// </summary>
        public ArkInventory inventory;

        public bool CheckIfDinoIsInRange(DotArkLocationData pos)
        {
            //The range on this is cubic, for some reason.
            float offsetX = MathF.Abs(pos.x - source.location.x);
            float offsetY = MathF.Abs(pos.y - source.location.y);
            float offsetZ = MathF.Abs(pos.z - source.location.z);

            //Check if all are in range
            return (offsetX < MAX_FEEDING_RANGE) && (offsetY < MAX_FEEDING_RANGE) && (offsetZ < MAX_FEEDING_RANGE);
        }
    }
}
