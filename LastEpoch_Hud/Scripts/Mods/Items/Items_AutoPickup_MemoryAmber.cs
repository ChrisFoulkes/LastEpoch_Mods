using HarmonyLib;

namespace LastEpoch_Hud.Scripts.Mods.Items
{
    public class Items_AutoPickup_MemoryAmber
    {
        static int depth;
        static uint idBefore;

        public static bool CanRun()
        {
            if ((Scenes.IsGameScene()) && (!Save_Manager.instance.IsNullOrDestroyed()) && (!Refs_Manager.player_actor.IsNullOrDestroyed()))
            {
                return Save_Manager.instance.data.Items.Pickup.Enable_AutoPickup_MemoryAmber;
            }
            else { return false; }
        }

        public static void Begin()
        {
            if (!CanRun()) { return; }
            if (depth == 0)
            {
                Il2CppLE.Factions.PickupableObjectSet set = CurrentSet();
                idBefore = set.IsNullOrDestroyed() ? 0u : set.NextID;
            }
            depth++;
        }

        public static void End()
        {
            if ((!CanRun()) || (depth == 0)) { return; }
            depth--;
            if (depth == 0) { PickupSince(idBefore); }
        }

        static Il2CppLE.Factions.PickupableObjectSet CurrentSet()
        {
            Il2CppLE.Factions.PickupableObjectsManager manager = Il2CppLE.Factions.PickupableObjectsManager.Instance;
            if (manager.IsNullOrDestroyed()) { return null; }
            return manager.singleplayerPickupableObjects;
        }

        static void PickupSince(uint firstId)
        {
            Il2CppLE.Factions.PickupableObjectsManager manager = Il2CppLE.Factions.PickupableObjectsManager.Instance;
            Il2CppLE.Factions.PickupableObjectSet set = CurrentSet();
            if (manager.IsNullOrDestroyed() || set.IsNullOrDestroyed() || set.pickupables == null) { return; }
            if (!set.pickupables.ContainsKey(Il2CppLE.Factions.PickupableObjectType.MemoryAmber)) { return; }

            Il2CppSystem.Collections.Generic.List<Il2CppLE.Factions.PickupableObject> amber = set.pickupables[Il2CppLE.Factions.PickupableObjectType.MemoryAmber];
            if (amber == null) { return; }
            for (int index = amber.Count - 1; index >= 0; index--)
            {
                Il2CppLE.Factions.PickupableObject pickupable = amber[index];
                if ((pickupable == null) || (pickupable.Id < firstId)) { continue; }
                manager.PickupObject(set, pickupable, index);
            }
        }

        [HarmonyPatch(typeof(Il2Cpp.SilkenCocoonData), "DropMemoryAmber")]
        public class DropMemoryAmber
        {
            [HarmonyPrefix]
            static void Prefix() { Begin(); }
            [HarmonyPostfix]
            static void Postfix() { End(); }
        }

        [HarmonyPatch(typeof(Il2Cpp.SilkenCocoonData), "DropMemoryAmberInPiles")]
        public class DropMemoryAmberInPiles
        {
            [HarmonyPrefix]
            static void Prefix() { Begin(); }
            [HarmonyPostfix]
            static void Postfix() { End(); }
        }

        [HarmonyPatch(typeof(Il2Cpp.SilkenCocoonData), "DropMemoryAmberInPilesForWeaverMembers")]
        public class DropMemoryAmberInPilesForWeaverMembers
        {
            [HarmonyPrefix]
            static void Prefix() { Begin(); }
            [HarmonyPostfix]
            static void Postfix() { End(); }
        }

        [HarmonyPatch(typeof(Il2Cpp.SilkenCocoonData), "DropMemoryAmberInPilesForExplicitActor")]
        public class DropMemoryAmberInPilesForExplicitActor
        {
            [HarmonyPrefix]
            static void Prefix() { Begin(); }
            [HarmonyPostfix]
            static void Postfix() { End(); }
        }
    }
}
