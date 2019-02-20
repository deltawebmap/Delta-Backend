using ArkHttpServer.Entities;
using ArkSaveEditor.ArkEntries;
using ArkSaveEditor.World.WorldTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHttpServer
{
    public static class BabyDinoStatsCalculator
    {
        public static double CalculateCurrentDinoFood(ArkDinoReply dinoData, double gameTimeOffset)
        {
            ArkDinoEntryStatusComponent entryStatusComponent = dinoData.dino_entry.statusComponent;
            double dinoFoodLossPerSecond = entryStatusComponent.baseFoodConsumptionRate * entryStatusComponent.extraBabyDinoConsumingFoodRateMultiplier * entryStatusComponent.babyDinoConsumingFoodRateMultiplier * entryStatusComponent.foodConsumptionMultiplier;
            return dinoData.dino.currentStats.food + (dinoFoodLossPerSecond * gameTimeOffset);
        }

        public static double CalculateTotalInventoryFood(ArkDinoReply dinoData)
        {
            //Get the food list of this dino class.
            List<ArkDinoFood> dinoFoodData = dinoData.dino_entry.childFoods;
            if (dinoFoodData == null)
            {
                //Fallback
                dinoFoodData = dinoData.dino_entry.adultFoods;
            }
            if (dinoFoodData == null)
            {
                throw new Exception("No dino food info found");
            }

            //Calculates the total food energy inside the inventory of this dino.
            double total = 0;
            for (int i = 0; i < dinoData.inventory_items.Count; i += 1)
            {
                //Loop through inventory items and get the total food energy.
                ArkPrimalItem item = dinoData.inventory_items[i];
                ArkItemEntry item_data = dinoData.item_class_data[item.classnameString];

                //Check if this item data can give food
                if (item_data.addStatusValues.ContainsKey("EPrimalCharacterStatusValue::Food"))
                {
                    ArkItemEntryAddStatus foodData = item_data.addStatusValues["EPrimalCharacterStatusValue::Food"];
                    double foodEnergy = foodData.baseAmountToAdd * item.stackSize; //Get the total energy, multiplied by the stack size, but before calculating the dino's food data.

                    //Check if we have data for this in the dino food data
                    for (int j = 0; j < dinoFoodData.Count; j += 1)
                    {
                        ArkDinoFood thisFoodData = dinoFoodData[i];
                        if (thisFoodData.classname == item.classnameString)
                        {
                            //Great. We got the info. Add it and break.
                            total += thisFoodData.foodEffectivenessMultiplier * foodEnergy;
                            break;
                        }
                    }
                }
            }

            return total;
        }

        public static double CalculateTotalDinoFood(ArkDinoReply dinoData, double gameTimeOffset)
        {
            return CalculateCurrentDinoFood(dinoData, gameTimeOffset) + CalculateTotalInventoryFood(dinoData);
        }

        public static double CalculateFoodLossPerSecond(ArkDinoReply dinoData)
        {
            ArkDinoEntryStatusComponent entryStatusComponent = dinoData.dino_entry.statusComponent;
            return entryStatusComponent.baseFoodConsumptionRate * entryStatusComponent.extraBabyDinoConsumingFoodRateMultiplier * entryStatusComponent.babyDinoConsumingFoodRateMultiplier * entryStatusComponent.foodConsumptionMultiplier;
        }

        public static double CalculateTimeToFoodDepletionMs(double foodAmount, ArkDinoReply dinoData)
        {
            double dinoFoodLossPerSecond = CalculateFoodLossPerSecond(dinoData);
            return (foodAmount / Math.Abs(dinoFoodLossPerSecond)) * 1000;
        }

        public static BabyDinoInfoPackage GetFullDinoInfo(ArkDinoReply dinoData, double gameTimeOffset)
        {
            //Calculate
            BabyDinoInfoPackage p = new BabyDinoInfoPackage();
            p.currentFood = CalculateCurrentDinoFood(dinoData, gameTimeOffset);
            p.inventoryFood = CalculateTotalInventoryFood(dinoData);
            p.totalCurrentFood = p.currentFood + p.inventoryFood;
            p.foodLossPerSecond = CalculateFoodLossPerSecond(dinoData);
            p.timeToDepletionMs = CalculateTimeToFoodDepletionMs(p.totalCurrentFood, dinoData);

            return p;
        }

        public class BabyDinoInfoPackage
        {
            public double currentFood;
            public double inventoryFood;
            public double totalCurrentFood;
            public double foodLossPerSecond;
            public double timeToDepletionMs;
            public double timeToDepletionString;
        }
    }
}
