using HarmonyLib;
using Il2Cpp;
using Il2CppLE.Factions;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Factions.TheWoven
{
    public class Faction_Woven_TreePoints
    {
        static bool capturedOriginal;
        static ushort originalEarned;
        static bool readingOriginal;
        static bool readingMax;
        static bool writing;

        public const int SliderMax = 200;

        public static int GameMax()
        {
            readingMax = true;
            int max = 0;
            try { max = TheWeaver.MaxWeaverPoints; }
            catch { }
            readingMax = false;
            if (max < 1) { return 0; }
            if (max > ushort.MaxValue) { return ushort.MaxValue; }
            return max;
        }

        public static bool CanRun()
        {
            if ((Scenes.IsGameScene()) && (!Save_Manager.instance.IsNullOrDestroyed()))
            {
                return Save_Manager.instance.data.Factions.TheWoven.Enable_TreePoints;
            }
            else { return false; }
        }

        public static int ClampPoints(int points)
        {
            if (points < 0) { return 0; }
            if (points > ushort.MaxValue) { return ushort.MaxValue; }
            return points;
        }

        public static void ApplyToPlayer()
        {
            try
            {
                if (!Refs_Manager.player_treedata.IsNullOrDestroyed() && Refs_Manager.player_treedata.weaverTree != null)
                {
                    WritePoints(Refs_Manager.player_treedata.weaverTree);
                }
                RefreshOpenUi();
            }
            catch { }
        }

        static void WritePoints(LocalTreeData.WeaverTreeData tree)
        {
            if (writing || tree == null) { return; }
            writing = true;
            try
            {
                if (!capturedOriginal)
                {
                    readingOriginal = true;
                    originalEarned = tree.EarnedWeaverPoints;
                    readingOriginal = false;
                    capturedOriginal = true;
                }

                if (CanRun())
                {
                    tree.EarnedWeaverPoints = (ushort)ClampPoints(Save_Manager.instance.data.Factions.TheWoven.TreePoints);
                }
                else
                {
                    tree.EarnedWeaverPoints = originalEarned;
                }
            }
            finally { writing = false; }
        }

        static void RefreshOpenUi()
        {
            foreach (FactionRankUIWeaver ui in Object.FindObjectsOfType<FactionRankUIWeaver>())
            {
                if (ui != null) { ui.UpdateUnspentPointsRoot(); }
            }
        }

        [HarmonyPatch(typeof(LocalTreeData.WeaverTreeData), "get_EarnedWeaverPoints")]
        public class WeaverTreeData_GetEarned
        {
            [HarmonyPostfix]
            static void Postfix(ref ushort __result)
            {
                if (readingOriginal || writing || !CanRun()) { return; }
                __result = (ushort)ClampPoints(Save_Manager.instance.data.Factions.TheWoven.TreePoints);
            }
        }

        [HarmonyPatch(typeof(TheWeaver), "get_EarnedWeaverPoints")]
        public class TheWeaver_GetEarned
        {
            [HarmonyPostfix]
            static void Postfix(ref int __result)
            {
                if (!CanRun()) { return; }
                __result = ClampPoints(Save_Manager.instance.data.Factions.TheWoven.TreePoints);
            }
        }

        [HarmonyPatch(typeof(TheWeaver), "get_MaxWeaverPoints")]
        public class TheWeaver_GetMax
        {
            [HarmonyPostfix]
            static void Postfix(ref int __result)
            {
                if (readingMax || !CanRun()) { return; }
                int points = ClampPoints(Save_Manager.instance.data.Factions.TheWoven.TreePoints);
                if (points > __result) { __result = points; }
            }
        }

        [HarmonyPatch(typeof(LocalTreeData.WeaverTreeData), "getUnspentPoints")]
        public class WeaverTreeData_GetUnspent
        {
            [HarmonyPrefix]
            static void Prefix(LocalTreeData.WeaverTreeData __instance)
            {
                if (!CanRun()) { return; }
                WritePoints(__instance);
            }
        }

        [HarmonyPatch(typeof(FactionRankUIWeaver), "OpenWeaverTree")]
        public class FactionRankUIWeaver_Open
        {
            [HarmonyPrefix]
            static void Prefix()
            {
                ApplyToPlayer();
            }
        }

        [HarmonyPatch(typeof(FactionRankUIWeaver), "Awake")]
        public class FactionRankUIWeaver_Awake
        {
            [HarmonyPostfix]
            static void Postfix()
            {
                ApplyToPlayer();
            }
        }
    }
}
