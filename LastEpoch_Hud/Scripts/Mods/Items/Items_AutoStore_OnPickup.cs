using HarmonyLib;
using Il2Cpp;

namespace LastEpoch_Hud.Scripts.Mods.Items
{
    public class Items_AutoStore_OnPickup
    {
        static bool storing;

        public static void StoreNow()
        {
            if (storing) { return; }
            storing = true;
            try
            {
                ItemContainersManager manager = ItemContainersManager.Instance;
                if (manager.IsNullOrDestroyed()) { return; }
                manager.TryStoreMaterials(false);
                if (!manager.keys.IsNullOrDestroyed())
                {
                    MoveMatching(manager, data => Item.isKey(data.itemType), (data, qty) => manager.keys.TryAddItem(data, qty, Context.SILENT));
                }
                if (!manager.wovenEchoes.IsNullOrDestroyed())
                {
                    MoveMatching(manager, data => Item.isWovenEcho(data.itemType), (data, qty) => manager.wovenEchoes.TryAddItem(data, qty, Context.SILENT));
                }
            }
            catch { Main.logger_instance?.Msg("Items_AutoStore.StoreNow() ERROR"); }
            finally { storing = false; }
        }

        public static void StorePicked(ItemData data)
        {
            if (storing || data.IsNullOrDestroyed()) { return; }
            storing = true;
            try
            {
                ItemContainersManager manager = ItemContainersManager.Instance;
                if (manager.IsNullOrDestroyed() || manager.inventory.IsNullOrDestroyed()) { return; }
                ItemContainerEntry entry = FindEntry(manager.inventory, data);
                if (entry == null) { return; }

                int type = data.itemType;
                if (Item.isKey(type))
                {
                    MoveEntry(manager.inventory, entry, (item, qty) => (!manager.keys.IsNullOrDestroyed()) && manager.keys.TryAddItem(item, qty, Context.SILENT));
                    return;
                }
                if (Item.isWovenEcho(type))
                {
                    MoveEntry(manager.inventory, entry, (item, qty) => (!manager.wovenEchoes.IsNullOrDestroyed()) && manager.wovenEchoes.TryAddItem(item, qty, Context.SILENT));
                    return;
                }
                if (ItemList.isShard(type)) { manager.TryStoreShard(entry); return; }
                if (Item.isResonance(type)) { manager.TryStoreResonance(entry); return; }
                if ((type == Item.runeItemType) || (type == Item.glyphItemType)) { manager.TryStoreRuneOrGlyph(entry); return; }
                if (ItemList.isCraftingItem(type)) { manager.TryQuickStoreCraftingItem(entry); }
            }
            catch { }
            finally { storing = false; }
        }

        static ItemContainerEntry FindEntry(ItemContainer inventory, ItemData data)
        {
            Il2CppSystem.Collections.Generic.List<ItemContainerEntry> content = inventory.GetContent();
            if (content == null) { return null; }
            ItemContainerEntry sameType = null;
            for (int i = content.Count - 1; i >= 0; i--)
            {
                ItemContainerEntry entry = content[i];
                if ((entry == null) || (entry.data.IsNullOrDestroyed())) { continue; }
                if (entry.data == data) { return entry; }
                if ((sameType == null) && (entry.data.itemType == data.itemType)) { sameType = entry; }
            }
            return sameType;
        }

        static void MoveEntry(ItemContainer inventory, ItemContainerEntry entry, System.Func<ItemData, int, bool> add)
        {
            int qty = entry.Quantity;
            if (qty < 1) { qty = 1; }
            if (add(entry.data, qty)) { inventory.TryRemoveItem(entry, qty, Context.SILENT); }
        }

        static void MoveMatching(ItemContainersManager manager, System.Func<ItemData, bool> match, System.Func<ItemData, int, bool> add)
        {
            ItemContainer inventory = manager.inventory;
            if (inventory.IsNullOrDestroyed()) { return; }
            Il2CppSystem.Collections.Generic.List<ItemContainerEntry> content = inventory.GetContent();
            if (content == null) { return; }

            var pendingEntries = new System.Collections.Generic.List<ItemContainerEntry>();
            for (int i = 0; i < content.Count; i++)
            {
                ItemContainerEntry entry = content[i];
                if ((entry == null) || (entry.data.IsNullOrDestroyed())) { continue; }
                if (match(entry.data)) { pendingEntries.Add(entry); }
            }
            foreach (ItemContainerEntry entry in pendingEntries) { MoveEntry(inventory, entry, add); }
        }

        [HarmonyPatch(typeof(ItemContainersManager), "attemptToPickupItem")]
        public class ItemContainersManager_attemptToPickupItem
        {
            [HarmonyPostfix]
            static void Postfix(bool __result, ItemData __0)
            {
                if ((!__result) || (storing) || (__0.IsNullOrDestroyed()) || (Save_Manager.instance.IsNullOrDestroyed()) || (Save_Manager.instance.data.IsNullOrDestroyed())) { return; }
                if (!Save_Manager.instance.data.Items.Pickup.Enable_AutoStore_OnDrop) { return; }
                int type = __0.itemType;
                if ((ItemList.isCraftingItem(type)) || (Item.isKey(type)) || (Item.isWovenEcho(type))) { StorePicked(__0); }
            }
        }
    }
}
