using ArkSaveEditor;
using ArkSaveEditor.ArkEntries;
using ArkSaveEditor.World.WorldTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArkHttpServer.Tools.FoodSim
{
    /// <summary>
    /// A simulation of dinos and their food consumption. Can figure out how much food dinos can use when accessing shared feeding bins.
    /// </summary>
    public class DinoFoodSimulation
    {
        /// <summary>
        /// Dinos inside of the simulation
        /// </summary>
        public List<FoodSimDino> dinos = new List<FoodSimDino>();

        /// <summary>
        /// Feeding bins inside the simulation
        /// </summary>
        public List<FoodSimFeedingBin> feedingBins = new List<FoodSimFeedingBin>();

        /// <summary>
        /// Time offset from when this was first captured. Starts at 0. In seconds.
        /// </summary>
        public float timeOffset = 0;

        /// <summary>
        /// Ticks the world. Does not send events.
        /// </summary>
        /// <param name="deltaTime">Time, in seconds, to advance the timer by.</param>
        public void Tick(float deltaTime)
        {
            //Tick dino age
            foreach(var d in dinos)
            {
                //Fix
                if (d.age == 0)
                    d.age = 1;

                //Add
                /*if (d.source.dino_entry.extraBabyAgeSpeedMultiplier > 0)
                    d.age += (d.source.dino_entry.babyAgeSpeed * d.source.dino_entry.extraBabyAgeSpeedMultiplier * deltaTime);
                else
                    d.age += (d.source.dino_entry.babyAgeSpeed * deltaTime);*/

                //Cap
                if (d.age > 1)
                    d.age = 1;
            }

            //Tick food
            foreach(var d in dinos)
            {
                //Calculate the food debt. This is how much food we need this tick.
                float debt = deltaTime * d.source.dino_entry.statusComponent.baseFoodConsumptionRate * d.source.dino_entry.statusComponent.foodConsumptionMultiplier;
                if (d.GetIsBaby())
                    debt *= d.source.dino_entry.statusComponent.babyDinoConsumingFoodRateMultiplier * d.source.dino_entry.statusComponent.extraBabyDinoConsumingFoodRateMultiplier;

                //Subtract the debt from the dino's food. (Debt is negative)
                d.currentFood += debt;

                //Check inventory
                foreach(var i in d.inventory.items)
                {
                    //Make sure we have at least one of these items
                    if (i.stackSize <= 0)
                        continue;
                    
                    //Get the food energy this grants from the item data
                    var item_entry = ArkImports.GetItemDataByClassname(i.classname);
                    if (item_entry == null)
                        continue;
                    if (!item_entry.addStatusValues.ContainsKey("EPrimalCharacterStatusValue::Food"))
                        continue;
                    var foodAddStatusValue = item_entry.addStatusValues["EPrimalCharacterStatusValue::Food"];
                    double food_energy = foodAddStatusValue.baseAmountToAdd;
                    
                    //Ensure this is a food we can consume
                    ArkDinoFood food_entry = null;
                    List<ArkDinoFood> scan_data; //The list of foods that we will scan.
                    if (d.GetIsBaby())
                        scan_data = d.source.dino_entry.childFoods;
                    else
                        scan_data = d.source.dino_entry.adultFoods;
                    foreach(var ie in scan_data)
                    {
                        if (CompareClassnames(ie.classname, i.classname))
                            food_entry = ie;
                    }
                    if (food_entry == null)
                        continue;

                    //Calculate food energy with this
                    food_energy *= food_entry.foodEffectivenessMultiplier;

                    //If we have space to consume this, do so
                    while(d.currentFood + food_energy <= d.maxFood && i.stackSize > 0)
                    {
                        i.stackSize--;
                        d.currentFood += (float)food_energy;
                        d.foodUsed += (float)food_energy;
                    }
                }
            }

            //Add to time offset
            timeOffset += deltaTime;
        }

        /// <summary>
        /// Runs the simulation and returns the results
        /// </summary>
        public List<FoodSimResult> RunSimulation(float accuracy)
        {
            //Run until we get the results
            List<FoodSimResult> results = new List<FoodSimResult>();
            while(results.Count() < dinos.Count())
            {
                //Tick
                Tick(accuracy);
                
                //Check
                foreach(var d in dinos)
                {
                    //Skip if we already have this
                    if (results.Where(x => x.source == d.source).Count() != 0)
                        continue;

                    //Check if we're out of food
                    if (d.currentFood > 0)
                        continue;

                    //Add it
                    results.Add(new FoodSimResult
                    {
                        source = d.source,
                        ageWhenFoodDepletes = d.age,
                        foodDepletionTime = timeOffset,
                        foodUsed = d.foodUsed
                    });
                }
            }
            return results;
        }

        private bool CompareClassnames(string n1, string n2)
        {
            if (n1.EndsWith("_C"))
                n1 = n1.Substring(0, n1.Length - 2);
            if (n2.EndsWith("_C"))
                n2 = n2.Substring(0, n2.Length - 2);
            return n1 == n2;
        }

        /// <summary>
        /// Adds a dinosaur to the simulation. Returns true if adding was ok, false if not.
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public bool AddDino(ArkDinosaur d)
        {
            //Check if we have a entry
            if (d.dino_entry == null)
                return false;

            //Create dino item and add it
            dinos.Add(new FoodSimDino
            {
                age = d.babyAge,
                currentFood = d.currentStats.food,
                foodUsed = 0,
                inventory = d.GetInventory(),
                maxFood = d.GetMaxStats().food,
                source = d
            });
            return true;
        }
    }
}
