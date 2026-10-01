using HarmonyLib;
using Il2CppLE.UI.Minimap;

namespace LastEpoch_Hud.Scripts.Mods.Minimap
{
    internal class Minimap_FogOfWar
    {
        public static bool CanRun()
        {
            if ((Scenes.IsGameScene()) && (!Save_Manager.instance.IsNullOrDestroyed()))
            {
                if (!Save_Manager.instance.data.IsNullOrDestroyed())
                {
                    return Save_Manager.instance.data.Scenes.Minimap.Enable_RemoveFogOfWar;
                }
                else { return false; }
            }
            else { return false; }
        }

        [HarmonyPatch(typeof(Il2CppLE.UI.Minimap.Minimap), "Awake")]
        public class Minimap_Awake
        {
            [HarmonyPostfix]
            static void Postfix(Il2CppLE.UI.Minimap.Minimap __instance)
            {
                if (!CanRun() || __instance.IsNullOrDestroyed())
                {
                    return;
                }

                __instance.RevealRadius = int.MaxValue;
                __instance.maxDistance = int.MaxValue;
            }
        }
    }
}
