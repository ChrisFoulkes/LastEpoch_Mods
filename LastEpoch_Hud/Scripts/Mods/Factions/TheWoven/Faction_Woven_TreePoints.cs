using HarmonyLib;
using Il2CppLE.Factions;

namespace LastEpoch_Hud.Scripts.Mods.Factions.TheWoven
{
    public class Faction_Woven_TreePoints
    {
        public static bool CanRun()
        {
            if ((Scenes.IsGameScene()) && (!Save_Manager.instance.IsNullOrDestroyed()))
            {
                return Save_Manager.instance.data.Factions.TheWoven.Enable_TreePoints;
            }
            else { return false; }
        }

        [HarmonyPatch(typeof(FactionRankUIWeaver), "OnEnable")]
        public class FactionRankUIWeaver_OnEnable
        {
            [HarmonyPrefix]
            static void Prefix(ref FactionRankUIWeaver __instance)
            {
                if (!CanRun() || Refs_Manager.player_treedata.IsNullOrDestroyed() || Refs_Manager.player_treedata.weaverTree == null)
                {
                    return;
                }

                try
                {
                    int points = Save_Manager.instance.data.Factions.TheWoven.TreePoints;
                    if (points < 0) { points = 0; }
                    if (points > ushort.MaxValue) { points = ushort.MaxValue; }
                    Refs_Manager.player_treedata.weaverTree.EarnedWeaverPoints = (ushort)points;
                    __instance.UpdateUnspentPointsRoot();
                }
                catch { }
            }
        }
    }
}
