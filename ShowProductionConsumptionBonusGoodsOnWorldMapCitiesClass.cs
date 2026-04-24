using HarmonyLib;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using zip.lexy.tgame.city;
using zip.lexy.tgame.simulation.consumption;
using zip.lexy.tgame.state.building;
using zip.lexy.tgame.state.city;

namespace ShowProductionConsumptionBonusGoodsOnWorldMapCitiesClass
{
    public class ShowProductionConsumptionBonusGoodsOnWorldMapCitiesMod : MelonMod
    {
        [HarmonyPatch(typeof(WorldCityDetails), "ShowCityBonuses")]
        private static class ShowCityBonusesPatch
        {
            private static void Postfix(WorldCityDetails __instance)
            {
                // 1. Access the city instance
                FieldInfo cityField = typeof(WorldCityDetails).GetField("city", BindingFlags.NonPublic | BindingFlags.Instance);
                City city = (City)cityField.GetValue(__instance);
                if (city == null) return;

                // 2. Get the UI row
                FieldInfo bonusRowField = typeof(WorldCityDetails).GetField("bonusGoodsRow", BindingFlags.NonPublic | BindingFlags.Instance);
                Transform bonusRow = (Transform)bonusRowField.GetValue(__instance);

                // 3. Setup the label GameObject
                Transform existing = bonusRow.Find("surplus-deficit-label");
                GameObject labelObj = existing != null ? existing.gameObject : new GameObject("surplus-deficit-label");
                labelObj.transform.SetParent(bonusRow, false);

                RectTransform labelRect = labelObj.GetComponent<RectTransform>();
                TextMeshProUGUI textMesh = labelObj.GetComponent<TextMeshProUGUI>() ?? labelObj.AddComponent<TextMeshProUGUI>();

                // 4. Build the combined string for the 3 bonuses
                var prodDict = Production.GetCityProduction(city);
                var consDict = Consumption.GetCityConsumption(city);

                List<string> labels = new List<string>();

                // We take 3 to match the game's default "bonusGoodsIcons" limit
                foreach (string goodId in city.bonuses.Take(3))
                {
                    float prod = 0;
                    float cons = 0;

                    if (prodDict.TryGetValue(goodId, out var pStack)) prod = pStack.amount;
                    if (consDict.TryGetValue(goodId, out var cStack)) cons = cStack.amount;

                    // Format this specific good and add to list
                    labels.Add(FormatLabel(prod, cons));
                }

                // 5. Join them with spacing to align (roughly) under the icons
                // You may need to adjust the number of spaces based on icon width
                textMesh.text = string.Join("    ", labels);

                textMesh.fontSize = 16f;
                textMesh.alignment = TextAlignmentOptions.Center;

                // Offset the text so it sits below the icons
                labelObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(5, 20);
                labelObj.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
                labelObj.GetComponent<RectTransform>().anchorMin = new Vector2(0.1f, 0.5f);
            }

            private static string FormatLabel(float prod, float cons)
            {
                string pCol = prod == 0f ? "grey" : "#00FF00"; // Bright Green
                string cCol = cons == 0f ? "grey" : "#FF0000"; // Bright Red
                return $"<color={pCol}>{prod:F1}</color>/<color={cCol}>{cons:F1}</color>";
            }
        }
    }
}