using HarmonyLib;
using Il2Cpp;
using Il2CppLE.Data;
using Il2CppTMPro;
using MelonLoader;
using Newtonsoft.Json;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.Mods.Bank
{
    [RegisterTypeInIl2Cpp]
    public class Bank_Quad : MonoBehaviour
    {
        public Bank_Quad(System.IntPtr ptr) : base(ptr) { }
        public static Bank_Quad instance { get; private set; }

        public static int character_index = -1;
        public static int backup_active_tab = -1;
        public static Vector2Int default_size = new Vector2Int(12, 17);
        public static Sprite default_grid = null;
        public static Vector2Int quad_size = new Vector2Int(24, 34);
        public static Sprite quad_grid = null;
        public static Image quad_overlay = null;
        static int presented_tab = -999;
        public static UIPanel stash_panel = null;
        public static StashItemContainer stash_item_container = null;
        public static StashItemContainerUI stash_item_container_ui = null;
        public static Image stash_grid_image = null;
        public static System.Collections.Generic.List<bool[]> occupied_slots = null;
        public static ItemContainersManager item_contenairs_manager = null;

        //configure
        public static ConfigureTabUI configure_tab_ui = null;
        public static GameObject QuadStash_obj = null;
        public static string toggle_str = "Quad Stash";
        public static string toggle_explain_str = "ReOpen this tab to take effect";
        public static Toggle configure_stash_toggle = null;
        public static TextMeshProUGUI configure_stash_toggle_title = null;
        public static TextMeshProUGUI configure_stash_toggle_explanation = null;
        public static string configure_stash_name_backup = "";
        public static bool open_configure = false;

        void Awake()
        {
            instance = this;

        }
        void Update()
        {
            if (!Refs_Manager.game_uibase.IsNullOrDestroyed())
            {
                if (!Scenes.IsGameScene())
                {
                    if (!stash_panel.IsNullOrDestroyed()) { stash_panel = null; } //Reset

                    if ((Refs_Manager.game_uibase.characterSelectOpen) && (!Refs_Manager.game_uibase.characterSelectPanel.IsNullOrDestroyed()))
                    {
                        GameObject char_selection_game_object = Refs_Manager.game_uibase.characterSelectPanel.instance;
                        if (!char_selection_game_object.IsNullOrDestroyed())
                        {
                            CharacterSelect char_select = char_selection_game_object.GetComponent<CharacterSelect>();
                            LocalCharacterSlots local_slots = char_selection_game_object.GetComponent<LocalCharacterSlots>();

                            if ((!char_select.IsNullOrDestroyed()) && (!local_slots.IsNullOrDestroyed()))
                            {
                                if (char_select.currentState == CharacterSelect.CharacterSelectState.LoadCharacter)
                                {
                                    int index = char_select.SelectedCharacterIndex;
                                    if ((index > -1) && (index != character_index) && (index < local_slots.characterSlots.Count))
                                    {
                                        character_index = index;
                                        Cycle cycle = local_slots.characterSlots[index].Cycle;
                                        string solo_char_name = "";
                                        StashType stashType = StashType.Softcore;

                                        if (local_slots.characterSlots[index].SoloChallenge)
                                        {
                                            solo_char_name = local_slots.characterSlots[index].CharacterName;
                                            Save.Data.path = Save.Data.base_path + cycle.ToString() + @"\" + solo_char_name + @"\";
                                        }
                                        else
                                        {
                                            if (local_slots.characterSlots[index].Hardcore) { stashType = StashType.Hardcore; }
                                            else { stashType = StashType.Softcore; }
                                            Save.Data.path = Save.Data.base_path + cycle.ToString() + @"\" + stashType.ToString() + @"\";
                                        }
                                        Save.Data.Load();
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    //Get Refs
                    if (stash_panel.IsNullOrDestroyed() && !StashPanelUI.Instance.IsNullOrDestroyed())
                    {
                        stash_panel = StashPanelUI.Instance.GetComponent<UIPanel>();
                        if (stash_panel.IsNullOrDestroyed())
                        {
                            stash_panel = StashPanelUI.Instance.GetComponentInParent<UIPanel>();
                        }
                    }
                    if ((!stash_panel.IsNullOrDestroyed()) && (/*(stash_item_container_ui.IsNullOrDestroyed()) ||*/ (stash_grid_image.IsNullOrDestroyed()) || (default_grid.IsNullOrDestroyed())))
                    {
                        if (!stash_panel.instance.IsNullOrDestroyed())
                        {
                            GameObject left_obj = Functions.GetChild(stash_panel.instance, "left-container");
                            if (!left_obj.IsNullOrDestroyed())
                            {
                                /*if (stash_item_container_ui.IsNullOrDestroyed())
                                {
                                    GameObject stash_obj = Functions.GetChild(left_obj, "Stash");
                                    if (!stash_obj.IsNullOrDestroyed()) {  stash_item_container_ui = stash_obj.GetComponent<StashItemContainerUI>(); }
                                }*/
                                if (stash_grid_image.IsNullOrDestroyed())
                                {
                                    GameObject grid_obj = Functions.GetChild(left_obj, "grid-img");
                                    if (!grid_obj.IsNullOrDestroyed()) { stash_grid_image = grid_obj.GetComponent<Image>(); }
                                }
                                if ((!stash_grid_image.IsNullOrDestroyed()) && (default_grid.IsNullOrDestroyed()))
                                {
                                    default_grid = stash_grid_image.activeSprite;
                                    Object.DontDestroyOnLoad(default_grid);
                                }
                            }
                        }
                    }
                    if ((!Hud_Manager.asset_bundle.IsNullOrDestroyed()) && (quad_grid.IsNullOrDestroyed()))
                    {
                        foreach (string name in Hud_Manager.asset_bundle.GetAllAssetNames())
                        {
                            if (name.Contains("/quadstash/"))
                            {
                                if ((Functions.Check_Texture(name)) && (name.Contains("quad_grid")))
                                {
                                    Texture2D texture = Hud_Manager.asset_bundle.LoadAsset(name).TryCast<Texture2D>();
                                    quad_grid = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                                    Object.DontDestroyOnLoad(quad_grid);
                                }
                            }
                        }
                    }

                    //Update UI
                    if (!stash_item_container_ui.IsNullOrDestroyed() && !stash_item_container_ui.gameObject.activeInHierarchy) { presented_tab = -999; }
                    if (!stash_item_container.IsNullOrDestroyed())
                    {
                        if (stash_item_container_ui.IsNullOrDestroyed()) { stash_item_container_ui = UnityEngine.Object.FindObjectOfType<StashItemContainerUI>(); }
                        int tab = stash_item_container.CurrentlyActiveTab;
                        if ((!stash_item_container_ui.IsNullOrDestroyed()) && (stash_item_container_ui.gameObject.activeInHierarchy) && (tab != presented_tab))
                        {
                            if (ApplyPresentation())
                            {
                                presented_tab = tab;
                                backup_active_tab = tab;
                            }
                        }
                    }
                    if (!configure_tab_ui.IsNullOrDestroyed())
                    {
                        if ((open_configure) && (configure_tab_ui.gameObject.active)) //DoOnce
                        {
                            open_configure = false;
                            configure_stash_name_backup = configure_tab_ui.nameInputTMP.text; //set backup name
                            if (!configure_stash_toggle_title.IsNullOrDestroyed())
                            {
                                if (configure_stash_toggle_title.text != toggle_str)
                                {
                                    configure_stash_toggle_title.text = toggle_str;
                                }                                
                            }
                            if (!configure_stash_toggle_explanation.IsNullOrDestroyed())
                            {
                                if (configure_stash_toggle_explanation.text != toggle_explain_str)
                                {
                                    configure_stash_toggle_explanation.text = toggle_explain_str;
                                }                                
                            }
                            if ((!Save.Data.UserTabs.IsNullOrDestroyed()) && (!configure_stash_toggle.IsNullOrDestroyed()))
                            {
                                configure_stash_toggle.isOn = Save.Data.UserTabs.names.Contains(configure_tab_ui.nameInputTMP.text);
                            }
                        }
                    }
                }
            }
        }

        static bool ApplyPresentation()
        {
            if (stash_item_container_ui.IsNullOrDestroyed()) { return false; }
            RectTransform rect = stash_item_container_ui.rectT;
            if (rect.IsNullOrDestroyed()) { return false; }
            float width = Mathf.Abs(rect.rect.width);
            float height = Mathf.Abs(rect.rect.height);
            if ((width < 10f) || (height < 10f)) { return false; }

            bool quad = Get.IsQuadStash();
            Vector2Int grid = quad ? quad_size : default_size;
            if (!stash_item_container_ui.Container.IsNullOrDestroyed())
            {
                ItemContainer active = stash_item_container_ui.Container.TryCast<ItemContainer>();
                if (!active.IsNullOrDestroyed()) { SetContainerSize(active, grid); }
            }

            stash_item_container_ui.slotSize = new Vector2(width / grid.x, height / grid.y);
            stash_item_container_ui.ReDrawWholeContainer();
            EnsureQuadSprite();
            EnsureOverlay(rect);
            if (!quad_overlay.IsNullOrDestroyed())
            {
                quad_overlay.gameObject.SetActive(quad);
                if (quad)
                {
                    quad_overlay.sprite = quad_grid;
                    quad_overlay.color = Color.white;
                    quad_overlay.rectTransform.SetAsFirstSibling();
                }
            }
            if (!stash_grid_image.IsNullOrDestroyed())
            {
                if (default_grid.IsNullOrDestroyed()) { default_grid = stash_grid_image.sprite; }
                stash_grid_image.enabled = !quad;
            }
            return true;
        }

        static void EnsureOverlay(RectTransform parent)
        {
            if (!quad_overlay.IsNullOrDestroyed()) { return; }
            Transform existing = parent.Find("quad_grid_overlay");
            GameObject overlay_obj = existing.IsNullOrDestroyed() ? new GameObject("quad_grid_overlay") : existing.gameObject;
            if (existing.IsNullOrDestroyed()) { overlay_obj.transform.SetParent(parent, false); }
            RectTransform overlay_rect = overlay_obj.GetComponent<RectTransform>();
            if (overlay_rect.IsNullOrDestroyed()) { overlay_rect = overlay_obj.AddComponent<RectTransform>(); }
            overlay_rect.anchorMin = Vector2.zero;
            overlay_rect.anchorMax = Vector2.one;
            overlay_rect.offsetMin = Vector2.zero;
            overlay_rect.offsetMax = Vector2.zero;
            overlay_rect.localScale = Vector3.one;
            quad_overlay = overlay_obj.GetComponent<Image>();
            if (quad_overlay.IsNullOrDestroyed()) { quad_overlay = overlay_obj.AddComponent<Image>(); }
            quad_overlay.raycastTarget = false;
            quad_overlay.type = Image.Type.Simple;
            quad_overlay.preserveAspect = false;
            if (stash_grid_image.IsNullOrDestroyed())
            {
                Image[] images = parent.GetComponentsInChildren<Image>(true);
                foreach (Image image in images)
                {
                    if (image.gameObject.name != "grid-img") { continue; }
                    stash_grid_image = image;
                    break;
                }
            }
        }

        static void EnsureQuadSprite()
        {
            if (!quad_grid.IsNullOrDestroyed()) { return; }
            if (!Hud_Manager.asset_bundle.IsNullOrDestroyed())
            {
                foreach (string name in Hud_Manager.asset_bundle.GetAllAssetNames())
                {
                    if ((!name.Contains("/quadstash/")) || (!name.Contains("quad_grid")) || (!Functions.Check_Texture(name))) { continue; }
                    Texture2D texture = Hud_Manager.asset_bundle.LoadAsset(name).TryCast<Texture2D>();
                    if (texture.IsNullOrDestroyed()) { continue; }
                    quad_grid = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    UnityEngine.Object.DontDestroyOnLoad(quad_grid);
                    return;
                }
            }
            quad_grid = BuildGridSprite(quad_size.x, quad_size.y);
            UnityEngine.Object.DontDestroyOnLoad(quad_grid);
        }

        static Sprite BuildGridSprite(int columns, int rows)
        {
            const int cell = 16;
            int width = columns * cell;
            int height = rows * cell;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color32[] pixels = new Color32[width * height];
            Color32 fill = new Color32(18, 16, 14, 210);
            Color32 line = new Color32(166, 124, 62, 255);
            for (int i = 0; i < pixels.Length; i++) { pixels[i] = fill; }
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    int left = x * cell;
                    int bottom = y * cell;
                    for (int i = 0; i < cell; i++)
                    {
                        pixels[bottom * width + left + i] = line;
                        pixels[(bottom + cell - 1) * width + left + i] = line;
                        pixels[(bottom + i) * width + left] = line;
                        pixels[(bottom + i) * width + left + cell - 1] = line;
                    }
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            texture.filterMode = FilterMode.Point;
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), cell);
        }

        static bool IsQuadContainer(ItemContainer container)
        {
            return (!container.IsNullOrDestroyed()) && (container.size.x == quad_size.x) && (container.size.y == quad_size.y);
        }

        static int ContainerIndex(ItemContainer container)
        {
            if ((stash_item_container.IsNullOrDestroyed()) || (container.IsNullOrDestroyed())) { return -1; }
            int index = 0;
            foreach (ItemContainer item in stash_item_container.containers)
            {
                if (item == container) { return index; }
                index++;
            }
            return -1;
        }

        static bool[] MapFor(ItemContainer container)
        {
            int index = ContainerIndex(container);
            if (index < 0) { return null; }
            if (occupied_slots == null) { occupied_slots = new System.Collections.Generic.List<bool[]>(); }
            while (occupied_slots.Count <= index) { occupied_slots.Add(new bool[quad_size.x * quad_size.y]); }
            bool[] map = occupied_slots[index];
            if ((map == null) || (map.Length != quad_size.x * quad_size.y))
            {
                map = new bool[quad_size.x * quad_size.y];
                occupied_slots[index] = map;
            }
            return map;
        }

        static void RebuildOccupied(ItemContainer container)
        {
            bool[] map = MapFor(container);
            if (map == null) { return; }
            for (int i = 0; i < map.Length; i++) { map[i] = false; }
            Il2CppSystem.Collections.Generic.List<ItemContainerEntry> content = container.GetContent();
            if (content == null) { return; }
            for (int i = 0; i < content.Count; i++)
            {
                ItemContainerEntry entry = content[i];
                if (entry == null) { continue; }
                foreach (int slot in Get.SlotsPosition(entry.Position, entry.size))
                {
                    if ((slot >= 0) && (slot < map.Length)) { map[slot] = true; }
                }
            }
        }

        static void SetContainerSize(ItemContainer container, Vector2Int size)
        {
            if (container.IsNullOrDestroyed()) { return; }
            if ((container.size.x > size.x) || (container.size.y > size.y)) { EjectOverflow(container, size); }
            container.size = size;
        }

        static void EjectOverflow(ItemContainer container, Vector2Int bounds)
        {
            Il2CppSystem.Collections.Generic.List<ItemContainerEntry> content = container.GetContent();
            if (content == null) { return; }
            var overflow = new System.Collections.Generic.List<ItemContainerEntry>();
            for (int i = 0; i < content.Count; i++)
            {
                ItemContainerEntry entry = content[i];
                if ((entry == null) || (entry.data.IsNullOrDestroyed())) { continue; }
                Vector2Int itemSize = entry.size;
                if (itemSize.x < 1) { itemSize.x = 1; }
                if (itemSize.y < 1) { itemSize.y = 1; }
                Vector2Int pos = entry.Position;
                if ((pos.x < 0) || (pos.y < 0) || (pos.x + itemSize.x > bounds.x) || (pos.y + itemSize.y > bounds.y)) { overflow.Add(entry); }
            }

            ItemContainer inventory = null;
            if (!ItemContainersManager.Instance.IsNullOrDestroyed()) { inventory = ItemContainersManager.Instance.inventory; }
            foreach (ItemContainerEntry entry in overflow)
            {
                if ((entry == null) || (entry.data.IsNullOrDestroyed())) { continue; }
                int qty = entry.Quantity;
                if (qty < 1) { qty = 1; }
                bool moved = false;
                if (!inventory.IsNullOrDestroyed()) { moved = inventory.TryAddItem(entry.data, qty, Context.SILENT); }
                if (moved)
                {
                    container.TryRemoveItem(entry, qty, Context.SILENT);
                    continue;
                }

                ItemData dropData = entry.data;
                ItemDataUnpacked unpacked = entry.data.TryCast<ItemDataUnpacked>();
                if (!unpacked.IsNullOrDestroyed())
                {
                    ItemDataUnpacked copy = unpacked.CreateGameplayDuplicate();
                    if (!copy.IsNullOrDestroyed()) { dropData = copy; }
                }
                container.TryRemoveItem(entry, qty, Context.SILENT);
                if (Refs_Manager.ground_item_manager.IsNullOrDestroyed() && !GroundItemManager.instance.IsNullOrDestroyed()) { Refs_Manager.ground_item_manager = GroundItemManager.instance; }
                if ((!Refs_Manager.player_actor.IsNullOrDestroyed()) && (!Refs_Manager.ground_item_manager.IsNullOrDestroyed()))
                {
                    Refs_Manager.ground_item_manager.dropItemForPlayer(Refs_Manager.player_actor, dropData, Refs_Manager.player_actor.position(), false);
                }
            }
        }

        public class Get
        {
            public static string ActiveTabName()
            {
                string r = "";
                try
                {
                    if (!stash_item_container.IsNullOrDestroyed())
                    {
                        int index = stash_item_container.CurrentlyActiveTab;
                        for (int i = 0; i < stash_item_container.LinkedStash.Tabs.Count; i++)
                        {
                            if (stash_item_container.LinkedStash.Tabs[i].TabId == index)
                            {
                                r = stash_item_container.LinkedStash.Tabs[i].DisplayName;
                                break;
                            }
                        }
                    }
                }
                catch { }

                return r;
            }
            public static string ActiveTabName(int index)
            {
                string r = "";
                try
                {
                    if (!stash_item_container.IsNullOrDestroyed())
                    {
                        for (int i = 0; i < stash_item_container.LinkedStash.Tabs.Count; i++)
                        {
                            if (stash_item_container.LinkedStash.Tabs[i].TabId == index)
                            {
                                r = stash_item_container.LinkedStash.Tabs[i].DisplayName;
                                break;
                            }
                        }
                    }
                }
                catch { }

                return r;
            }
            public static bool IsQuadStash()
            {
                bool r = false;
                try
                {
                    string tab_name = ActiveTabName();
                    if (!Save.Data.UserTabs.IsNullOrDestroyed())
                    {
                        if ((tab_name != "") && (Save.Data.UserTabs.names.Contains(tab_name))) { r = true; }
                    }
                }
                catch { }

                return r;
            }
            public static bool IsQuadStash(int index)
            {
                bool r = false;
                try
                {
                    string tab_name = ActiveTabName(index);
                    if (!Save.Data.UserTabs.IsNullOrDestroyed())
                    {
                        if ((tab_name != "") && (Save.Data.UserTabs.names.Contains(tab_name))) { r = true; }
                    }
                }
                catch { }

                return r;
            }
            public static System.Collections.Generic.List<int> SlotsPosition(Vector2Int slot_position, Vector2Int item_size)
            {
                System.Collections.Generic.List<int> positions = new System.Collections.Generic.List<int>();
                int base_position = slot_position.x + (slot_position.y * quad_size.x);
                for (int y = 0; y < item_size.y; y++)
                {
                    for (int x = 0; x < item_size.x; x++)
                    {
                        positions.Add(base_position + x + (y * quad_size.x));
                    }
                }

                return positions;
            }            
        }
        public class Save
        {
            public class Data
            {
                public static readonly string base_path = Directory.GetCurrentDirectory() + @"\Mods\LastEpoch_Hud\QuadStashs\";
                public static string filename = "QuadStashs.json";
                public static string path = "";
                public static Structures.tabs UserTabs = new Structures.tabs();

                public class Structures
                {
                    public struct tabs
                    {
                        public System.Collections.Generic.List<string> names;
                    }
                }

                public static void DefaultConfig()
                {
                    Main.logger_instance.Msg("QuadStashs : Make DefaultConfig");

                    UserTabs = new Structures.tabs
                    {
                        names = new System.Collections.Generic.List<string>()
                    };
                }
                public static void Load()
                {
                    Main.logger_instance.Msg("QuadStashs : Try to Load : " + Data.path + Data.filename);

                    if (!File.Exists(Data.path + filename))
                    {
                        DefaultConfig();
                        Save();
                    }
                    if (File.Exists(Data.path + filename))
                    {
                        try
                        {
                            Data.UserTabs = JsonConvert.DeserializeObject<Structures.tabs>(File.ReadAllText(Data.path + filename));
                            Main.logger_instance.Msg("QuadStashs : Loaded");
                        }
                        catch { Main.logger_instance.Error("QuadStashs : Error loading file : " + Data.path + filename); }
                    }
                }
                public static void Save()
                {
                    Main.logger_instance.Msg("QuadStashs : Save : " + Data.path + Data.filename);
                    string jsonString = JsonConvert.SerializeObject(Data.UserTabs, Newtonsoft.Json.Formatting.Indented);
                    if (!Directory.Exists(Data.path)) { Directory.CreateDirectory(Data.path); }
                    if (File.Exists(Data.path + Data.filename)) { File.Delete(Data.path + Data.filename); }
                    File.WriteAllText(Data.path + Data.filename, jsonString);
                }
            }
        }
        public class Hooks
        {
            [HarmonyPatch(typeof(StashItemContainerUI), "Awake")]
            public class StashItemContainerUI_Awake
            {
                [HarmonyPostfix]
                static void Postfix(ref StashItemContainerUI __instance)
                {
                    stash_item_container_ui = __instance;
                }
            }

            [HarmonyPatch(typeof(ItemContainersManager), "Awake")]
            public class ItemContainersManager_Awake
            {
                [HarmonyPrefix]
                static void Prefix(ref ItemContainersManager __instance)
                {
                    item_contenairs_manager = __instance;
                    occupied_slots = new System.Collections.Generic.List<bool[]>();
                    stash_item_container = null;
                    backup_active_tab = -1;
                }
            }

            [HarmonyPatch(typeof(TabbedItemContainer), "AddNewTab")]
            public class TabbedItemContainer_AddNewTab
            {
                [HarmonyPrefix]
                static void Prefix(ref TabbedItemContainer __instance, ref ItemContainer __0)
                {
                    if (stash_item_container.IsNullOrDestroyed()) { stash_item_container = __instance.TryCast<StashItemContainer>(); }
                    if (!stash_item_container.IsNullOrDestroyed())
                    {
                        int index = stash_item_container.containers.Count;
                        if (Get.IsQuadStash(index)) { __0.size = quad_size; }
                        else { __0.size = default_size; }
                        bool[] occupied = new bool[(quad_size.x * quad_size.y)];
                        for (int i = 0; i < occupied.Length; i++) { occupied[i] = false; }
                        occupied_slots.Add(occupied);
                    }
                }
            }

            [HarmonyPatch(typeof(ItemContainerUIWithSearch), "Tabbed_OnActiveTabChanged")]
            public class ItemContainerUIWithSearch_Tabbed_OnActiveTabChanged
            {
                [HarmonyPrefix]
                static void Prefix(ref ItemContainerUIWithSearch __instance, ref TabbedItemContainer __0)
                {
                    if (stash_item_container.IsNullOrDestroyed()) { stash_item_container = __0.TryCast<StashItemContainer>(); }
                    StashItemContainerUI stash_ui = __instance.TryCast<StashItemContainerUI>();
                    if (!stash_ui.IsNullOrDestroyed()) { stash_item_container_ui = stash_ui; }
                    presented_tab = -999;
                }
            }

            [HarmonyPatch(typeof(ItemContainer), "CheckSlotsOccupied")]
            public class ItemContainer_CheckSlotsOccupied
            {
                [HarmonyPrefix]
                static bool Prefix(ref ItemContainer __instance, ref bool __result, Vector2Int __0, Vector2Int __1)
                {
                    if (!IsQuadContainer(__instance)) { return true; }
                    bool[] map = MapFor(__instance);
                    bool occupied = false;
                    if (map != null)
                    {
                        foreach (int slot_index in Get.SlotsPosition(__0, __1))
                        {
                            if ((slot_index < 0) || (slot_index >= map.Length) || (map[slot_index]))
                            {
                                occupied = true;
                                break;
                            }
                        }
                    }
                    __result = occupied;
                    return false;
                }
            }

            [HarmonyPatch(typeof(ItemContainer), "SetSlotsOccupied")]
            public class ItemContainer_SetSlotsOccupied
            {
                [HarmonyPrefix]
                static bool Prefix(ref ItemContainer __instance, Vector2Int __0, Vector2Int __1, bool __2)
                {
                    if (!IsQuadContainer(__instance)) { return true; }
                    bool[] map = MapFor(__instance);
                    if (map != null)
                    {
                        foreach (int slot_index in Get.SlotsPosition(__0, __1))
                        {
                            if ((slot_index >= 0) && (slot_index < map.Length)) { map[slot_index] = __2; }
                        }
                    }
                    return false;
                }
            }

            [HarmonyPatch(typeof(ItemContainer), "Sort")]
            public class ItemContainer_Sort
            {
                [HarmonyPrefix]
                static void Prefix(ItemContainer __instance)
                {
                    if (IsQuadContainer(__instance)) { RebuildOccupied(__instance); }
                }
            }

            [HarmonyPatch(typeof(Il2CppLE.UI.PanelSystem.StashPanelV2), "OnSortClicked")]
            public class StashPanelV2_OnSortClicked
            {
                [HarmonyPrefix]
                static bool Prefix()
                {
                    if ((stash_item_container_ui.IsNullOrDestroyed()) || (stash_item_container_ui.Container.IsNullOrDestroyed())) { return true; }
                    ItemContainer active = stash_item_container_ui.Container.TryCast<ItemContainer>();
                    if (active.IsNullOrDestroyed()) { return true; }
                    if (IsQuadContainer(active)) { RebuildOccupied(active); }
                    active.Sort();
                    presented_tab = -999;
                    return false;
                }
            }

            [HarmonyPatch(typeof(ItemContainer), "GetItemsInArea")]
            public class ItemContainer_GetItemsInArea
            {
                [HarmonyPrefix]
                static bool Prefix(ItemContainer __instance, ref Il2CppSystem.Collections.Generic.HashSet<ItemContainerEntry> __result, Vector2Int __0, Vector2Int __1)
                {
                    bool r = true;
                    if ((__instance.id == ContainerID.STASH) && (__instance.size == quad_size))
                    {
                        Il2CppSystem.Collections.Generic.List<int> area_positions = new Il2CppSystem.Collections.Generic.List<int>();
                        foreach (int slot_index in Get.SlotsPosition(__0, __1)) { area_positions.Add(slot_index); }
                        Il2CppSystem.Collections.Generic.HashSet<ItemContainerEntry> item_list = new Il2CppSystem.Collections.Generic.HashSet<ItemContainerEntry>();
                        foreach (ItemContainerEntry item_container_entry in __instance.content)
                        {
                            System.Collections.Generic.List<int> item_positions = Get.SlotsPosition(item_container_entry.Position, item_container_entry.size);
                            foreach (int slot_position in item_positions)
                            {
                                if (area_positions.Contains(slot_position)) { item_list.Add(item_container_entry); }
                            }
                        }
                        __result = item_list;
                        r = false;
                    }

                    return r;
                }
            }

            static void ApplyToggleState(string tabName)
            {
                configure_stash_name_backup = tabName ?? "";
                if (!configure_stash_toggle_title.IsNullOrDestroyed()) { configure_stash_toggle_title.text = toggle_str; }
                if (!configure_stash_toggle_explanation.IsNullOrDestroyed()) { configure_stash_toggle_explanation.text = toggle_explain_str; }
                if ((!Save.Data.UserTabs.IsNullOrDestroyed()) && (Save.Data.UserTabs.names != null) && (!configure_stash_toggle.IsNullOrDestroyed()))
                {
                    configure_stash_toggle.SetIsOnWithoutNotify(Save.Data.UserTabs.names.Contains(configure_stash_name_backup));
                }
            }

            static void BindClone(GameObject clone)
            {
                QuadStash_obj = clone;
                clone.name = "quad_stash_row";
                clone.SetActive(true);
                Il2CppLE.UI.Components.CheckboxInput box = clone.GetComponent<Il2CppLE.UI.Components.CheckboxInput>();
                if (!box.IsNullOrDestroyed()) { box.enabled = false; }
                configure_stash_toggle = clone.GetComponentInChildren<Toggle>(true);
                if (!configure_stash_toggle.IsNullOrDestroyed()) { configure_stash_toggle.onValueChanged = new Toggle.ToggleEvent(); }
                TextMeshProUGUI[] labels = clone.GetComponentsInChildren<TextMeshProUGUI>(true);
                configure_stash_toggle_title = (labels != null && labels.Length > 0) ? labels[0] : null;
                configure_stash_toggle_explanation = (labels != null && labels.Length > 1) ? labels[1] : null;
            }

            static void InjectLegacy(ConfigureTabUI ui)
            {
                if (ui.IsNullOrDestroyed() || ui.contents.IsNullOrDestroyed()) { return; }
                GameObject content = ui.contents.gameObject;
                GameObject existing = Functions.GetChild(content, "quad_stash_row");
                if (!existing.IsNullOrDestroyed())
                {
                    if (QuadStash_obj.IsNullOrDestroyed()) { BindClone(existing); }
                    return;
                }
                if (ui.stashPriorityUI.IsNullOrDestroyed()) { return; }
                GameObject priority_section = Functions.GetChild(content, "Priority Section");
                GameObject row = Functions.GetChild(ui.stashPriorityUI.gameObject, "Explanation Row");
                if (priority_section.IsNullOrDestroyed() || row.IsNullOrDestroyed()) { return; }

                RectTransform priority_section_rect = priority_section.GetComponent<RectTransform>();
                float row_H = priority_section_rect.rect.height;
                float margin = 20 * priority_section_rect.lossyScale.x;
                GameObject clone = UnityEngine.Object.Instantiate(row, new Vector3(row.transform.position.x, row.transform.position.y - row_H - margin, row.transform.position.z), Quaternion.identity);
                clone.transform.SetParent(content.transform);
                BindClone(clone);
                GameObject toggle_obj = Functions.GetChild(clone, "Priority Toggle");
                if (!toggle_obj.IsNullOrDestroyed())
                {
                    configure_stash_toggle = toggle_obj.GetComponent<Toggle>();
                    if (!configure_stash_toggle.IsNullOrDestroyed()) { configure_stash_toggle.onValueChanged = new Toggle.ToggleEvent(); }
                }
                GameObject text_obj = Functions.GetChild(clone, "OptionText");
                if (!text_obj.IsNullOrDestroyed())
                {
                    GameObject title_obj = Functions.GetChild(text_obj, "Title");
                    if (!title_obj.IsNullOrDestroyed()) { configure_stash_toggle_title = title_obj.GetComponent<TextMeshProUGUI>(); }
                    GameObject explanation_obj = Functions.GetChild(text_obj, "Explanation");
                    if (!explanation_obj.IsNullOrDestroyed()) { configure_stash_toggle_explanation = explanation_obj.GetComponent<TextMeshProUGUI>(); }
                }
            }

            static void InjectCurrent(Il2Cpp.StashConfigureUI ui)
            {
                if (ui.IsNullOrDestroyed()) { return; }
                Il2CppLE.UI.Components.CheckboxInput source = null;
                if (!ui._ignoreFolderPriority.IsNullOrDestroyed()) { source = ui._ignoreFolderPriority; }
                else if ((!ui._stashPriorityUI.IsNullOrDestroyed()) && (!ui._stashPriorityUI._priorityToggle.IsNullOrDestroyed())) { source = ui._stashPriorityUI._priorityToggle; }
                if (source.IsNullOrDestroyed()) { return; }
                Transform parent = source.transform.parent;
                if ((!QuadStash_obj.IsNullOrDestroyed()) && (QuadStash_obj.transform.parent == parent)) { return; }

                GameObject clone = UnityEngine.Object.Instantiate(source.gameObject, parent);
                clone.transform.SetSiblingIndex(source.transform.GetSiblingIndex() + 1);
                BindClone(clone);
            }

            static void SaveQuadChoice(string currentName)
            {
                if ((Save.Data.UserTabs.IsNullOrDestroyed()) || (Save.Data.UserTabs.names == null) || (configure_stash_toggle.IsNullOrDestroyed())) { return; }
                if (currentName == null) { currentName = ""; }
                bool save = false;
                bool update_containers = false;
                if (!configure_stash_toggle.isOn)
                {
                    if (Save.Data.UserTabs.names.Contains(configure_stash_name_backup))
                    {
                        System.Collections.Generic.List<string> new_names = new System.Collections.Generic.List<string>();
                        foreach (string s in Save.Data.UserTabs.names)
                        {
                            if (s != configure_stash_name_backup) { new_names.Add(s); }
                        }
                        Save.Data.UserTabs.names = new_names;
                        save = true;
                        update_containers = true;
                    }
                }
                else if (currentName != configure_stash_name_backup)
                {
                    bool updated = false;
                    for (int i = 0; i < Save.Data.UserTabs.names.Count; i++)
                    {
                        if (Save.Data.UserTabs.names[i] == configure_stash_name_backup)
                        {
                            Save.Data.UserTabs.names[i] = currentName;
                            updated = true;
                        }
                    }
                    if (!updated)
                    {
                        Save.Data.UserTabs.names.Add(currentName);
                        update_containers = true;
                    }
                    save = true;
                }
                else if (!Save.Data.UserTabs.names.Contains(currentName))
                {
                    Save.Data.UserTabs.names.Add(currentName);
                    save = true;
                    update_containers = true;
                }
                if (save)
                {
                    Save.Data.Save();
                    Save.Data.Load();
                }
                if ((update_containers) && (!stash_item_container.IsNullOrDestroyed()))
                {
                    int i = 0;
                    foreach (ItemContainer item_container in stash_item_container.containers)
                    {
                        SetContainerSize(item_container, Get.IsQuadStash(i) ? quad_size : default_size);
                        i++;
                    }
                    presented_tab = -999;
                }
            }

            [HarmonyPatch(typeof(ConfigureTabUI), "OnModalOpen")]
            public class ConfigureTabUI_OnModalOpen
            {
                [HarmonyPostfix]
                static void Postfix(ref ConfigureTabUI __instance, string __3)
                {
                    configure_tab_ui = __instance;
                    open_configure = true;
                    InjectLegacy(__instance);
                    ApplyToggleState(__3);
                }
            }

            [HarmonyPatch(typeof(Il2Cpp.StashConfigureUI), "OnModalOpen")]
            public class StashConfigureUI_OnModalOpen
            {
                [HarmonyPostfix]
                static void Postfix(Il2Cpp.StashConfigureUI __instance, string __2)
                {
                    InjectCurrent(__instance);
                    ApplyToggleState(__2);
                }
            }

            [HarmonyPatch(typeof(Il2CppLE.UI.PanelSystem.StashPanelV2), "OnOpen")]
            public class StashPanelV2_OnOpen
            {
                [HarmonyPostfix]
                static void Postfix(Il2CppLE.UI.PanelSystem.StashPanelV2 __instance)
                {
                    if (__instance.IsNullOrDestroyed() || !stash_grid_image.IsNullOrDestroyed()) { return; }
                    Image[] images = __instance.GetComponentsInChildren<Image>(true);
                    if (images == null) { return; }
                    foreach (Image image in images)
                    {
                        if (image.gameObject.name != "grid-img") { continue; }
                        stash_grid_image = image;
                        if (default_grid.IsNullOrDestroyed())
                        {
                            default_grid = stash_grid_image.sprite;
                            if (!default_grid.IsNullOrDestroyed()) { UnityEngine.Object.DontDestroyOnLoad(default_grid); }
                        }
                        break;
                    }
                }
            }

            [HarmonyPatch(typeof(ConfigureTabUI), "ConfirmConfigure")]
            public class ConfigureTabUI_ConfirmConfigure
            {
                [HarmonyPrefix]
                static void Prefix(ref ConfigureTabUI __instance)
                {
                    string currentName = "";
                    if (!__instance.nameInputTMP.IsNullOrDestroyed()) { currentName = __instance.nameInputTMP.text; }
                    SaveQuadChoice(currentName);
                }
            }

            [HarmonyPatch(typeof(Il2Cpp.StashConfigureUI), "ConfirmConfigure")]
            public class StashConfigureUI_ConfirmConfigure
            {
                [HarmonyPrefix]
                static void Prefix(Il2Cpp.StashConfigureUI __instance)
                {
                    string currentName = "";
                    if ((!__instance._nameInput.IsNullOrDestroyed()) && (!__instance._nameInput._serializedInput.IsNullOrDestroyed()))
                    {
                        currentName = __instance._nameInput._serializedInput.text;
                    }
                    SaveQuadChoice(currentName);
                }
            }
        }
    }
}
