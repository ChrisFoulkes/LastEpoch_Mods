using HarmonyLib;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Character
{
    public class Character_MemoryAmber_Multiplier
    {
        static int depth;

        public static bool CanRun()
        {
            if ((Scenes.IsGameScene()) && (!Save_Manager.instance.IsNullOrDestroyed()) && (!Refs_Manager.player_actor.IsNullOrDestroyed()))
            {
                return Save_Manager.instance.data.Character.Cheats.Enable_MemoryAmberMultiplier;
            }
            else { return false; }
        }

        static void Multiply(ref uint amount)
        {
            uint multiplier = Save_Manager.instance.data.Character.Cheats.MemoryAmberMultiplier;
            if (multiplier < 1) { multiplier = 1; }
            ulong scaled = (ulong)amount * multiplier;
            if (scaled > uint.MaxValue) { scaled = uint.MaxValue; }
            amount = (uint)scaled;
        }

        static void Prefix(Il2CppLE.Factions.PickupableObjectType type, ref uint amount)
        {
            if (type != Il2CppLE.Factions.PickupableObjectType.MemoryAmber) { return; }
            if ((depth == 0) && (CanRun())) { Multiply(ref amount); }
            depth++;
        }

        static void Postfix(Il2CppLE.Factions.PickupableObjectType type)
        {
            if ((type != Il2CppLE.Factions.PickupableObjectType.MemoryAmber) || (depth == 0)) { return; }
            depth--;
        }

        [HarmonyPatch(typeof(Il2CppLE.Factions.PickupableObjectsManager), "CreatePickupableObjectForPlayer", new System.Type[] { typeof(Il2CppLE.Factions.PickupableObjectType), typeof(Il2CppLE.Factions.PickupableObjectSet), typeof(Vector3), typeof(uint) })]
        public class CreateForPlayerSet
        {
            [HarmonyPrefix]
            static void Prefix(Il2CppLE.Factions.PickupableObjectType __0, ref uint __3) { Character_MemoryAmber_Multiplier.Prefix(__0, ref __3); }

            [HarmonyPostfix]
            static void Postfix(Il2CppLE.Factions.PickupableObjectType __0) { Character_MemoryAmber_Multiplier.Postfix(__0); }
        }

        [HarmonyPatch(typeof(Il2CppLE.Factions.PickupableObjectsManager), "CreatePickupableObjectForPlayer", new System.Type[] { typeof(Il2CppLE.Factions.PickupableObjectType), typeof(Il2Cpp.Actor), typeof(Vector3), typeof(uint) })]
        public class CreateForPlayerActor
        {
            [HarmonyPrefix]
            static void Prefix(Il2CppLE.Factions.PickupableObjectType __0, ref uint __3) { Character_MemoryAmber_Multiplier.Prefix(__0, ref __3); }

            [HarmonyPostfix]
            static void Postfix(Il2CppLE.Factions.PickupableObjectType __0) { Character_MemoryAmber_Multiplier.Postfix(__0); }
        }
    }
}
