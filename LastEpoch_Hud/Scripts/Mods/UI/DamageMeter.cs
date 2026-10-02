using HarmonyLib;
using Il2Cpp;
using Il2CppLE.AssetBundles;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.Mods.UI
{
    [RegisterTypeInIl2Cpp]
    public class DamageMeter : MonoBehaviour
    {
        public static DamageMeter instance { get; private set; }
        public DamageMeter(System.IntPtr ptr) : base(ptr) { }
        
        //Main
        public static GameObject DamageMeter_prefab = null;
        public static GameObject DamageMeter_obj = null;
        public static GameObject DamageMeter_content = null;
        public static TextMeshProUGUI MenuText = null;
        public static Button OnOff_btn = null;
        public static bool On = false;
        public static GameObject Reset_obj = null;
        public static Button Reset_btn = null;
        public static Image OnOff_Image = null;
        public static Sprite On_sprite = null;
        public static Sprite Off_sprite = null;

        //Skills
        public static GameObject Skill_prefab = null;
        public static System.Collections.Generic.List<Ability> Abilities = new System.Collections.Generic.List<Ability>();
        public static System.Collections.Generic.List<Skill> Skills = new System.Collections.Generic.List<Skill>();
        public static float TotalDamageDeal = 0f;
        public static float RecordedSeconds = 0f; //Game time spent recording since the first hit, pauses excluded
        public static Text Title_Text = null;
        public const int DamageType_Flat = 1;
        public const int DamageType_Dps = 2;

        public static float Dps(float damage)
        {
            return RecordedSeconds > 0f ? damage / RecordedSeconds : 0f;
        }
        public static string FormatNumber(float value)
        {
            if (value >= 1000000f) { return (value / 1000000f).ToString("0.00") + "M"; }
            if (value >= 10000f) { return (value / 1000f).ToString("0.0") + "k"; }
            return System.Convert.ToInt32(value).ToString();
        }

        //Settings
        public static GameObject Settings_panel = null;
        public static GameObject Settings_obj = null;
        public static Button Settings_btn = null;
        public static Dropdown DamageType_Dropdown = null;
        public static Toggle SeparateDot_Toggle = null;

        //Details
        public static GameObject Details_TopContent_prefab = null;
        public static GameObject Details_BottomContent_prefab = null;
        public static GameObject Details_panel = null;
        public static GameObject Details_TopCircleContent = null;
        public static GameObject Details_TopContent = null;
        public static GameObject Details_BottomCircleContent = null;
        public static GameObject Details_BottomContent = null;
        public static System.Collections.Generic.List<GameObject> details_skills = new System.Collections.Generic.List<GameObject>();
        public static string Details_TopSkillsName = "TopSkill_";
        public static Color[] colors = { Color.green, Color.blue, Color.gray, Color.cyan, Color.green, Color.white, Color.grey, Color.magenta, Color.black, Color.yellow };

        public static int Frames = 30; //Update append every x frames (to avoid too much performance cost)
        public static int Current_Frame = 0;

        void Awake()
        {
            instance = this;
            UI.Reset();
        }

        static KeyCode ToggleKey()
        {
            // Saves written before this bind existed load it as None.
            KeyCode key = KeyCode.None;
            if (!Save_Manager.instance.IsNullOrDestroyed()) { key = Save_Manager.instance.data.KeyBinds.DamageMeter; }
            return key == KeyCode.None ? KeyCode.F7 : key;
        }

        static bool BottomScreenReady()
        {
            return !Refs_Manager.game_uibase.IsNullOrDestroyed()
                && !Refs_Manager.game_uibase.bottomScreenMenu.IsNullOrDestroyed();
        }

        static void InitWithBottomScreen()
        {
            if (!Assets.Loaded()) { Assets.Load(); }
            if (!Assets.Loaded()) { return; }
            if (Abilities.Count == 0)
            {
                foreach (Ability ab in Resources.FindObjectsOfTypeAll<Ability>())
                {
                    if (!Abilities.Contains(ab)) { Abilities.Add(ab); }
                }
                foreach (Ability ab in NewItems.Items_Mjolner.Trigger.Abilities)
                {
                    if (!Abilities.Contains(ab)) { Abilities.Add(ab); }
                }
            }
            if (!UI.Initialized) { UI.Init(); }
        }

        static Il2CppSystem.Collections.Generic.List<Summoned> PlayerSummons()
        {
            if (Refs_Manager.player_actor.IsNullOrDestroyed()) { return null; }
            SummonTracker tracker = null;
            if (!Refs_Manager.player_actor.TryGetExistingSummonTracker(out tracker) || tracker.IsNullOrDestroyed()) { return null; }
            return tracker.summons;
        }

        static readonly System.Collections.Generic.Dictionary<string, LoadRef<Sprite>> ability_icons = new System.Collections.Generic.Dictionary<string, LoadRef<Sprite>>();
        static Sprite AbilityIcon(Ability ability)
        {
            if (ability.IsNullOrDestroyed()) { return null; }
            // The load ref is kept for the session so the sprite stays loaded while the meter shows it.
            if (!ability_icons.TryGetValue(ability.abilityName, out LoadRef<Sprite> loadRef))
            {
                SoftRef<Sprite> softRef = ability.abilitySpriteSoftRef;
                if (softRef == null || !softRef) { return null; }
                loadRef = SoftRefExtensions.CreateLoadRef(softRef, "LastEpoch_Hud", 0);
                if (loadRef == null) { return null; }
                loadRef.BlockForLoad();
                ability_icons[ability.abilityName] = loadRef;
            }
            return loadRef.AssetOrNull;
        }

        void Update()
        {
            if (Scenes.IsGameScene() && !UI.Initialized && BottomScreenReady()) { InitWithBottomScreen(); }
            if (!Assets.Loaded()) { return; }
            if (Scenes.IsGameScene())
            {
                if (!UI.Initialized) { return; }
                if (!MenuText.IsNullOrDestroyed()) { MenuText.text = "Damage Meter"; }
                if (!DamageMeter_obj.IsNullOrDestroyed())
                {
                    if (Input.GetKeyDown(ToggleKey())) { DamageMeter_obj.active = !DamageMeter_obj.active; }
                    if (!DamageMeter_obj.active) { On = false; }
                    if (On && Skills.Count > 0) { RecordedSeconds += Time.deltaTime; }
                    if (On) { UI.Update(); }
                }
                if (!OnOff_Image.IsNullOrDestroyed())
                {
                    if (On) { OnOff_Image.sprite = On_sprite; }
                    else { OnOff_Image.sprite = Off_sprite; }
                }
                if (!Reset_obj.IsNullOrDestroyed())
                {
                    if (Skills.Count > 0) { Reset_obj.active = true; }
                    else { Reset_obj.active = false; }
                }
            }
            else
            {
                On = false;
                Abilities = new System.Collections.Generic.List<Ability>();
                if (!DamageMeter_obj.IsNullOrDestroyed())
                {
                    if (DamageMeter_obj.active) { DamageMeter_obj.active = false; }
                }
                UI.Initialized = false;
            }
        }
        
        public class Assets
        {
            private static bool loading = false;

            public static bool Loaded()
            {
                bool result = false;
                if ((!DamageMeter_prefab.IsNullOrDestroyed()) && (!Skill_prefab.IsNullOrDestroyed())) { result = true; }

                return result;
            }
            public static void Load()
            {
                if ((!Hud_Manager.asset_bundle.IsNullOrDestroyed()) && (!loading))
                {                    
                    loading = true;
                    foreach (string name in Hud_Manager.asset_bundle.GetAllAssetNames())
                    {
                        if (name.Contains("/damagemeter/"))
                        {
                            if (Functions.Check_Prefab(name) && name.Contains("damagemeter.prefab"))
                            {
                                DamageMeter_prefab = Hud_Manager.asset_bundle.LoadAsset(name).TryCast<GameObject>();
                            }
                            else if (Functions.Check_Prefab(name) && name.Contains("skill.prefab"))
                            {
                                Skill_prefab = Hud_Manager.asset_bundle.LoadAsset(name).TryCast<GameObject>();
                            }
                            else if (Functions.Check_Prefab(name) && name.Contains("topcontent.prefab"))
                            {
                                Details_TopContent_prefab = Hud_Manager.asset_bundle.LoadAsset(name).TryCast<GameObject>();
                            }
                            else if (Functions.Check_Prefab(name) && name.Contains("bottomcontent.prefab"))
                            {
                                Details_BottomContent_prefab = Hud_Manager.asset_bundle.LoadAsset(name).TryCast<GameObject>();
                            }
                        }
                    }
                    loading = false;
                    // Only managed statics reference these prefabs, so the scene-load
                    // UnloadUnusedAssets pass would destroy them without this flag.
                    foreach (GameObject prefab in new GameObject[] { DamageMeter_prefab, Skill_prefab, Details_TopContent_prefab, Details_BottomContent_prefab })
                    {
                        if (!prefab.IsNullOrDestroyed()) { prefab.hideFlags |= HideFlags.DontUnloadUnusedAsset; }
                    }
                    if (!Loaded()) { Main.logger_instance.Error("Damage Meter : damagemeter.prefab or skill.prefab not found in asset bundle"); }
                }
            }
        }
        public class UI
        {
            public static bool Initializing = false;
            public static bool Initialized = false;

            public static void Init()
            {
                if (!Initializing && !Refs_Manager.game_uibase.IsNullOrDestroyed())
                {
                    Initializing = true;
                    DamageMeter_obj = Instantiate(DamageMeter_prefab, Vector3.zero, Quaternion.identity);
                    DontDestroyOnLoad(DamageMeter_obj);
                    DamageMeter_obj.active = false;
                    DamageMeter_obj.transform.SetParent(Refs_Manager.game_uibase.transform);
                    UIMouseListener mouse_listener = DamageMeter_obj.AddComponent<UIMouseListener>(); //Block mouse clicks over the meter
                    // Without this, the game treats the open meter as a panel that blocks world actions, so controller skills stop working.
                    mouse_listener.allowWorldActions = true;
                    if (!DamageMeter_obj.IsNullOrDestroyed())
                    {
                        GameObject images = Functions.GetChild(DamageMeter_obj, "Images");
                        if (!images.IsNullOrDestroyed())
                        {
                            GameObject On_obj = Functions.GetChild(images, "On");
                            if (!On_obj.IsNullOrDestroyed())
                            {
                                Image On_image = On_obj.GetComponent<Image>();
                                if (!On_image.IsNullOrDestroyed()) { On_sprite = On_image.sprite; }                                
                            }
                            GameObject Off_obj = Functions.GetChild(images, "Off");
                            if (!Off_obj.IsNullOrDestroyed())
                            {
                                Image Off_image = Off_obj.GetComponent<Image>();
                                if (!Off_image.IsNullOrDestroyed()) { Off_sprite = Off_image.sprite; }
                            }
                        }
                        GameObject panel = Functions.GetChild(DamageMeter_obj, "Panel");
                        if (!panel.IsNullOrDestroyed())
                        {
                            DamageMeter_content = Functions.GetChild(panel, "Content");
                            GameObject title = Functions.GetChild(panel, "Title");
                            if (!title.IsNullOrDestroyed())
                            {
                                GameObject title_name = Functions.GetChild(title, "Name");
                                if (!title_name.IsNullOrDestroyed()) { Title_Text = title_name.GetComponent<Text>(); }

                                GameObject on_off = Functions.GetChild(title, "OnOff");
                                if (!on_off.IsNullOrDestroyed())
                                {
                                    OnOff_btn = on_off.GetComponent<Button>();
                                    if (!OnOff_btn.IsNullOrDestroyed()) { Events.Set(OnOff_btn, Events.OnOff_OnClick_Action); }
                                    GameObject image = Functions.GetChild(on_off, "Image");
                                    if (!image.IsNullOrDestroyed())
                                    {
                                        OnOff_Image = image.GetComponent<Image>();
                                        if (!OnOff_Image.IsNullOrDestroyed()) { OnOff_Image.sprite = Off_sprite; }
                                    }
                                }

                                Reset_obj = Functions.GetChild(title, "Reset");
                                if (!Reset_obj.IsNullOrDestroyed())
                                {
                                    Reset_btn = Reset_obj.GetComponent<Button>();
                                    if (!Reset_btn.IsNullOrDestroyed()) { Events.Set(Reset_btn, Events.Reset_OnClick_Action); }
                                }

                                Settings_obj = Functions.GetChild(title, "Settings");
                                if (!Settings_obj.IsNullOrDestroyed())
                                {
                                    Settings_btn = Settings_obj.GetComponent<Button>();
                                    if (!Settings_btn.IsNullOrDestroyed()) { Events.Set(Settings_btn, Events.Settings_OnClick_Action); }
                                }
                            }
                        }
                        Settings_panel = Functions.GetChild(DamageMeter_obj, "SettingsPanel");
                        if (!Settings_panel.IsNullOrDestroyed())
                        {
                            GameObject content = Functions.GetChild(Settings_panel, "Content");
                            if (!content.IsNullOrDestroyed())
                            {
                                GameObject dropdown = Functions.GetChild(content, "DamageType");
                                if (!dropdown.IsNullOrDestroyed())
                                {
                                    DamageType_Dropdown = dropdown.GetComponent<Dropdown>();
                                    if (!DamageType_Dropdown.IsNullOrDestroyed())
                                    {
                                        //The prefab only ships Percent and Flat
                                        if (DamageType_Dropdown.options.Count <= DamageType_Dps)
                                        {
                                            DamageType_Dropdown.options.Add(new Dropdown.OptionData("Damage per Second"));
                                        }
                                        if ((Save_Manager.instance.data.DamageMeter.DamageType > 0) &&
                                            (Save_Manager.instance.data.DamageMeter.DamageType <= DamageType_Dps))
                                        {
                                            DamageType_Dropdown.value = Save_Manager.instance.data.DamageMeter.DamageType;
                                        }
                                        else { DamageType_Dropdown.value = 0; }                                            
                                        Events.Set_DropDown_Event(DamageType_Dropdown, Events.DamageType_Dropdown_Action);
                                    }                                    
                                }
                                GameObject toggle = Functions.GetChild(content, "SeparateDot");
                                if (!dropdown.IsNullOrDestroyed())
                                {
                                    SeparateDot_Toggle = toggle.GetComponent<Toggle>();
                                    if (!SeparateDot_Toggle.IsNullOrDestroyed())
                                    {
                                        SeparateDot_Toggle.isOn = Save_Manager.instance.data.DamageMeter.SeparateHitAndDot;
                                        Events.Set_Toggle_Event(SeparateDot_Toggle, Events.SeparateDot_Toggle_Action);
                                    }                                    
                                }
                            }
                        }
                        Details_panel = Functions.GetChild(DamageMeter_obj, "DetailsPanel");
                        if (!Details_panel.IsNullOrDestroyed())
                        {
                            GameObject title = Functions.GetChild(Details_panel, "Title");
                            if (!title.IsNullOrDestroyed())
                            {
                                GameObject button = Functions.GetChild(title, "CloseButton");
                                if (!button.IsNullOrDestroyed())
                                {
                                    Button btn = button.GetComponent<Button>();
                                    if (!btn.IsNullOrDestroyed())
                                    {
                                        Events.Set(btn, Events.CloseDetails_OnClick_Action);
                                    }
                                }
                            }
                            GameObject top_content = Functions.GetChild(Details_panel, "TopContent");
                            if (!top_content.IsNullOrDestroyed())
                            {
                                Details_TopCircleContent = Functions.GetChild(top_content, "L");
                                GameObject r_content = Functions.GetChild(top_content, "R");
                                if (!r_content.IsNullOrDestroyed())
                                {
                                    GameObject r_content2 = Functions.GetChild(r_content, "Content");
                                    if (!r_content2.IsNullOrDestroyed())
                                    {
                                        GameObject viewport = Functions.GetChild(r_content2, "Viewport");
                                        if (!viewport.IsNullOrDestroyed())
                                        {
                                            Details_TopContent = Functions.GetChild(viewport, "Content");
                                        }
                                    }
                                }
                            }
                            GameObject bottom_content = Functions.GetChild(Details_panel, "BottomContent");
                            if (!bottom_content.IsNullOrDestroyed())
                            {
                                Details_BottomCircleContent = Functions.GetChild(bottom_content, "L");
                                GameObject r_content = Functions.GetChild(bottom_content, "R");
                                if (!r_content.IsNullOrDestroyed())
                                {
                                    GameObject r_content2 = Functions.GetChild(r_content, "Content");
                                    if (!r_content2.IsNullOrDestroyed())
                                    {
                                        GameObject viewport = Functions.GetChild(r_content2, "Viewport");
                                        if (!viewport.IsNullOrDestroyed())
                                        {
                                            Details_BottomContent = Functions.GetChild(viewport, "Content");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    //menu
                    if (!Refs_Manager.game_uibase.IsNullOrDestroyed())
                    {
                        GameObject go = Refs_Manager.game_uibase.bottomScreenMenu.gameObject; //BottomScreenMenu
                        if (!go.IsNullOrDestroyed())
                        {
                            GameObject panel = Functions.GetChild(go, "BottomScreenMenuPanel");
                            if (!panel.IsNullOrDestroyed())
                            {
                                BottomScreenMenu bottomScreenMenu = panel.GetComponent<BottomScreenMenu>();
                                if (!bottomScreenMenu.IsNullOrDestroyed())
                                {
                                    bottomScreenMenu.buttonsToDisableInOnline = new Il2CppSystem.Collections.Generic.List<Button>();
                                }

                                GameObject shop = Functions.GetChild(panel, "Shop");
                                if (!shop.IsNullOrDestroyed())
                                {
                                    Button shop_btn = shop.GetComponent<Button>();
                                    Events.Set(shop_btn, Events.ToggleVisibility_OnClick_Action);
                                    shop_btn.interactable = true;

                                    // BottomScreenMenu.OnEnable still uses this CanvasGroup, so unlock it rather than destroying it.
                                    CanvasGroup canvasGroup = shop.GetComponent<CanvasGroup>();
                                    if (!canvasGroup.IsNullOrDestroyed())
                                    {
                                        canvasGroup.alpha = 1f;
                                        canvasGroup.interactable = true;
                                        canvasGroup.blocksRaycasts = true;
                                    }

                                    GameObject text_obj = Functions.GetChild(shop, "TextMeshPro Text");
                                    if (!text_obj.IsNullOrDestroyed())
                                    {
                                        MenuText = text_obj.GetComponent<TextMeshProUGUI>();                                        
                                    }
                                }
                            }
                        }
                    }
                    UI.Reset();
                    Initialized = true;
                    Initializing = false;
                    Main.logger_instance.Msg("Damage Meter : UI initialized, toggle with " + ToggleKey() + " or the Shop button");
                }
            }                        
            public static void AddSkill(string ability_name, float damage, bool hit, bool crit, bool kill, float overkill, string target_name, Sprite icon_override)
            {
                if (!Skill_prefab.IsNullOrDestroyed())
                {
                    GameObject skill_obj = Instantiate(Skill_prefab, Vector3.zero, Quaternion.identity);
                    skill_obj.active = false;
                    skill_obj.transform.SetParent(DamageMeter_content.transform);
                    Sprite icon = icon_override.IsNullOrDestroyed() ? GetIcon(ability_name) : icon_override;
                    System.Collections.Generic.List<float> damages = new System.Collections.Generic.List<float>();
                    damages.Add(damage);
                    System.Collections.Generic.List<bool> hits = new System.Collections.Generic.List<bool>();
                    hits.Add(hit);
                    System.Collections.Generic.List<bool> crits = new System.Collections.Generic.List<bool>();
                    crits.Add(crit);
                    System.Collections.Generic.List<bool> kills = new System.Collections.Generic.List<bool>();
                    kills.Add(kill);
                    System.Collections.Generic.List<float> overkills = new System.Collections.Generic.List<float>();
                    overkills.Add(overkill);
                    System.Collections.Generic.List<string> target_names = new System.Collections.Generic.List<string>();
                    target_names.Add(target_name);
                    Skills.Add(new Skill
                    {
                        Obj = skill_obj,
                        Icon = icon,
                        AbilityName = ability_name,                        
                        Damages = damages,
                        Hits = hits,
                        Crits = crits,
                        Kills = kills,
                        Overkills = overkills,
                        TargetName = target_names
                    });
                }
            }
            public static void SetTitle()
            {
                if (Title_Text.IsNullOrDestroyed()) { return; }
                if (RecordedSeconds <= 0f) { Title_Text.text = "DPS"; }
                else
                {
                    System.TimeSpan elapsed = System.TimeSpan.FromSeconds(RecordedSeconds);
                    Title_Text.text = "DPS " + FormatNumber(Dps(TotalDamageDeal)) + "  (" + (int)elapsed.TotalMinutes + ":" + elapsed.Seconds.ToString("00") + ")";
                }
            }
            public static void Update()
            {
                Current_Frame++;
                if (Current_Frame >= Frames)
                {
                    Current_Frame = 0;
                    DamageDeal.FlushAilments();
                    SetTitle();
                    /*bool Separate_Dot = false;
                    if (!SeparateDot_Toggle.IsNullOrDestroyed())
                    {
                        if (SeparateDot_Toggle.isOn) { Separate_Dot = true; }
                    }*/
                    foreach (Skill skill in Skills)
                    {
                        if (!skill.Obj.IsNullOrDestroyed())
                        {
                            //if (((!Separate_Dot) && (!skill.Dot)) || (Separate_Dot))
                            //{
                                Button btn = skill.Obj.GetComponent<Button>();
                                if (!btn.IsNullOrDestroyed())
                                {
                                    Events.Set(btn, Events.OpenDetails_OnClick_Action);
                                }
                                GameObject infos = Functions.GetChild(skill.Obj, "Infos");
                                //Set icon and SkillName                         
                                if ((!infos.IsNullOrDestroyed()))
                                {
                                    GameObject icon_obj = Functions.GetChild(infos, "Icon");
                                    if ((!icon_obj.IsNullOrDestroyed()))
                                    {
                                        GameObject image_obj = Functions.GetChild(icon_obj, "Image");
                                        if ((!image_obj.IsNullOrDestroyed()))
                                        {
                                            Image icon = image_obj.GetComponent<Image>();
                                            if (!icon.IsNullOrDestroyed()) { icon.sprite = skill.Icon; }
                                        }
                                    }
                                    GameObject skill_name_obj = Functions.GetChild(infos, "SkillName");
                                    if ((!skill_name_obj.IsNullOrDestroyed()))
                                    {
                                        GameObject text_obj = Functions.GetChild(skill_name_obj, "Text");
                                        if ((!text_obj.IsNullOrDestroyed()))
                                        {
                                            Text text = text_obj.GetComponent<Text>();
                                            if (!text.IsNullOrDestroyed())
                                            {
                                                text.text = skill.AbilityName;
                                                //if (skill.Dot) { text.text += " (DoT)"; }
                                            }
                                        }
                                    }
                                }
                                //Set damage and slider
                                float damage = 0f;
                                foreach (float f in skill.Damages) { damage += f; }
                                /*if (!Separate_Dot)
                                {
                                    foreach (Skill s in Skills)
                                    {
                                        if ((s.AbilityName == skill.AbilityName) && (s.Dot))
                                        {
                                            foreach (float f in s.Damages) { damage += f; }
                                            break;
                                        }
                                    }
                                }*/
                                if ((!infos.IsNullOrDestroyed()))
                                {
                                    GameObject percent_obj = Functions.GetChild(infos, "Percent");
                                    if ((!percent_obj.IsNullOrDestroyed()))
                                    {
                                        GameObject text_obj = Functions.GetChild(percent_obj, "Text");
                                        if ((!text_obj.IsNullOrDestroyed()))
                                        {
                                            Text text = text_obj.GetComponent<Text>();
                                            if (!text.IsNullOrDestroyed())
                                            {
                                                int damage_type = 0;
                                                if (!DamageType_Dropdown.IsNullOrDestroyed()) { damage_type = DamageType_Dropdown.value; }

                                                if (damage_type == DamageType_Flat) { text.text = FormatNumber(damage); }
                                                else if (damage_type == DamageType_Dps) { text.text = FormatNumber(Dps(damage)); }
                                                else
                                                {
                                                    float damage_percent = ((damage * 100) / TotalDamageDeal);
                                                    text.text = damage_percent.ToString("0.0") + " %";
                                                }
                                            }
                                        }
                                    }
                                }
                                GameObject skill_bar_obj = Functions.GetChild(skill.Obj, "SkillBar");
                                if (!skill_bar_obj.IsNullOrDestroyed())
                                {
                                    Slider slider = skill_bar_obj.GetComponent<Slider>();
                                    if (!slider.IsNullOrDestroyed()) { slider.value = damage * 100 / TotalDamageDeal; }
                                }
                                if (!skill.Obj.active) { skill.Obj.active = true; }
                            //}
                            //else if (skill.Obj.active) { skill.Obj.active = false; }
                        }
                    }
                }
            }
            public static Sprite GetIcon(string ability_name)
            {
                Sprite result = null;
                try
                {
                    foreach (Ability ab in Abilities)
                    {
                        if (ab.abilityName == ability_name)
                        {
                            result = AbilityIcon(ab);
                            if (!result.IsNullOrDestroyed()) { break; }
                        }
                    }
                    var summons = PlayerSummons();
                    if (result == null && summons != null)
                    {
                        foreach (Summoned summoned in summons) //Summon
                        {
                            AbilityList abilitylist = summoned.gameObject.GetComponent<AbilityList>();
                            if (!abilitylist.IsNullOrDestroyed())
                            {
                                foreach (Ability ability in abilitylist.abilities)
                                {
                                    //if (ability.name == obj_name)
                                    if (ability.abilityName == ability_name)
                                    {
                                        CreationReferences creationReferences = summoned.gameObject.GetComponent<CreationReferences>();
                                        if (!creationReferences.IsNullOrDestroyed())
                                        {
                                            if (!creationReferences.thisAbility.IsNullOrDestroyed())
                                            {
                                                result = AbilityIcon(creationReferences.thisAbility);
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                catch { }

                return result;
            }
            public static void Reset()
            {
                Skills = new System.Collections.Generic.List<Skill>();
                TotalDamageDeal = 0f;
                RecordedSeconds = 0f;
                DamageDeal.ClearPending();
                SetTitle();
                if (!DamageMeter_content.IsNullOrDestroyed())
                {
                    foreach (GameObject go in Functions.GetAllChild(DamageMeter_content)) { Object.Destroy(go); }
                }
            }
            public static void OpenDetails()
            {
                if (!Details_panel.IsNullOrDestroyed())
                {
                    if (!Details_panel.active)
                    {
                        GameObject TopCircle = null;
                        RectTransform TopCircleRectTransform = null;
                        if (!Details_TopCircleContent.IsNullOrDestroyed())
                        {
                            TopCircle = Functions.GetChild(Details_TopCircleContent, "TopCircle");
                            if (!TopCircle.IsNullOrDestroyed())
                            {
                                TopCircleRectTransform = TopCircle.GetComponent<RectTransform>();
                            }
                        }
                        int i = 0;
                        float rotation = 0f;
                        details_skills = new System.Collections.Generic.List<GameObject>();
                        foreach (Skill skill in Skills)
                        {
                            float damage = 0f;
                            foreach (float f in skill.Damages) { damage += f; }
                            float damage_percent = ((damage * 100) / TotalDamageDeal); //0 to 100%

                            if (!skill.Obj.IsNullOrDestroyed())
                            {
                                int color_index = i;
                                if (i > colors.Length)
                                {
                                    int multiplier = i / colors.Length;
                                    color_index = i - (multiplier * colors.Length);
                                }
                                //Pie charts                            
                                if (!Details_TopCircleContent.IsNullOrDestroyed())
                                {
                                    GameObject details_topcircle = Instantiate(TopCircle, Vector3.zero, Quaternion.identity);
                                    DontDestroyOnLoad(details_topcircle);
                                    details_topcircle.transform.SetParent(Details_TopCircleContent.transform);
                                    RectTransform rect_transform = details_topcircle.GetComponent<RectTransform>();
                                    if ((!rect_transform.IsNullOrDestroyed()) && (!TopCircleRectTransform.IsNullOrDestroyed()))
                                    {
                                        rect_transform.offsetMax = TopCircleRectTransform.offsetMax;
                                        rect_transform.offsetMin = TopCircleRectTransform.offsetMin;
                                    }
                                    Image details_topcircle_image = details_topcircle.GetComponent<Image>();
                                    if (!details_topcircle_image.IsNullOrDestroyed())
                                    {
                                        details_topcircle_image.color = colors[color_index];
                                        details_topcircle_image.fillAmount = damage_percent / 100;
                                    }
                                    details_topcircle.transform.Rotate(new Vector3(0, 0, (rotation * -1)));
                                    rotation += (damage_percent / 100) * 360;
                                    details_topcircle.active = true;
                                }
                                if ((!Details_TopContent.IsNullOrDestroyed()) && (!Details_TopContent_prefab.IsNullOrDestroyed()))
                                {                                    
                                    GameObject details_topskill = Instantiate(Details_TopContent_prefab, Vector3.zero, Quaternion.identity);
                                    DontDestroyOnLoad(details_topskill);
                                    details_skills.Add(details_topskill);
                                    details_topskill.transform.SetParent(Details_TopContent.transform);
                                    Button btn = details_topskill.GetComponent<Button>();
                                    if (!btn.IsNullOrDestroyed()) { btn.name = Details_TopSkillsName + i; }
                                    GameObject color = Functions.GetChild(details_topskill, "Color");
                                    if (!color.IsNullOrDestroyed())
                                    {
                                        GameObject image_obj = Functions.GetChild(color, "Image");
                                        {
                                            Image image = image_obj.GetComponent<Image>();
                                            if (!image.IsNullOrDestroyed())
                                            {
                                                image.color = colors[color_index];
                                            }
                                        }
                                    }
                                    GameObject ability_name = Functions.GetChild(details_topskill, "AbilityName");
                                    if (!ability_name.IsNullOrDestroyed())
                                    {
                                        Text text = ability_name.GetComponent<Text>();
                                        if (!text.IsNullOrDestroyed())
                                        {
                                            text.text = skill.AbilityName;
                                            //if (skill.Dot) { text.text += " (Dot)"; }
                                        }
                                    }
                                    GameObject flat_damage = Functions.GetChild(details_topskill, "FlatDamage");
                                    if (!flat_damage.IsNullOrDestroyed())
                                    {
                                        Text text = flat_damage.GetComponent<Text>();
                                        if (!text.IsNullOrDestroyed())
                                        {
                                            text.text = System.Convert.ToString((int)damage);
                                        }
                                    }
                                    GameObject percent_damage = Functions.GetChild(details_topskill, "PercentageDamage");
                                    if (!percent_damage.IsNullOrDestroyed())
                                    {
                                        Text text = percent_damage.GetComponent<Text>();
                                        if (!text.IsNullOrDestroyed())
                                        {
                                            text.text = System.Convert.ToInt32(damage_percent) + " %";
                                        }
                                    }
                                    details_topskill.active = true;
                                }
                            }
                            i++;
                        }
                        if (i > 0) { ShowDetails_Bottom(0); }
                        Details_panel.active = true;
                    }
                }
            }
            public static void CloseDetails()
            {
                if (!Details_panel.IsNullOrDestroyed())
                {
                    if (Details_panel.active)
                    {
                        Details_panel.active = false;
                        if (!Details_TopCircleContent.IsNullOrDestroyed())
                        {
                            foreach (GameObject obj in Functions.GetAllChild(Details_TopCircleContent))
                            {
                                if (obj.name != "TopCircle")
                                {
                                    Object.Destroy(obj);
                                }
                            }
                        }
                        if (!Details_TopContent.IsNullOrDestroyed())
                        {
                            foreach (GameObject obj in Functions.GetAllChild(Details_TopContent))
                            {
                                Object.Destroy(obj);
                            }
                        }
                        if (!Details_BottomCircleContent.IsNullOrDestroyed())
                        {
                            foreach (GameObject obj in Functions.GetAllChild(Details_BottomCircleContent))
                            {
                                if ((obj.name != "Title") && (obj.name != "BottomCircle"))
                                {
                                    Object.Destroy(obj);
                                }
                            }
                        }
                        if (!Details_BottomContent.IsNullOrDestroyed())
                        {
                            foreach (GameObject obj in Functions.GetAllChild(Details_BottomContent))
                            {
                                Object.Destroy(obj);
                            }
                        }
                    }
                }
            }
            public static void ShowDetails_Bottom(int i)
            {
                if (i < Skills.Count)
                {
                    if ((!Details_BottomCircleContent.IsNullOrDestroyed()) && (!Details_BottomContent.IsNullOrDestroyed()))
                    {
                        foreach (GameObject go in details_skills) //Show selected
                        {
                            bool selected = false;
                            GameObject ability_name = Functions.GetChild(go, "AbilityName");
                            if (!ability_name.IsNullOrDestroyed())
                            {
                                Text text = ability_name.GetComponent<Text>();
                                if (!text.IsNullOrDestroyed())
                                {
                                    if (text.text == Skills[i].AbilityName) { selected = true; }
                                }
                            }

                            Image img = go.GetComponent<Image>();
                            if (!img.IsNullOrDestroyed())
                            {
                                if (selected) { img.enabled = true; }
                                else { img.enabled = false; }
                            }
                        }
                        foreach (GameObject obj in Functions.GetAllChild(Details_BottomCircleContent))
                        {
                            if ((obj.name != "Title") && (obj.name != "BottomCircle")) { Object.Destroy(obj); }
                        }
                        foreach (GameObject obj in Functions.GetAllChild(Details_BottomContent)) { Object.Destroy(obj); }
                        GameObject text_obj = Functions.GetChild(Details_BottomCircleContent, "Title");
                        if (!text_obj.IsNullOrDestroyed())
                        {
                            Text text = text_obj.GetComponent<Text>();
                            if (!text.IsNullOrDestroyed()) { text.text = Skills[i].AbilityName; }
                        }
                        GameObject Circle = Functions.GetChild(Details_BottomCircleContent, "BottomCircle");
                        RectTransform CircleRectTransform = null;
                        if (!Circle.IsNullOrDestroyed()) { CircleRectTransform = Circle.GetComponent<RectTransform>(); }
                        float total_damage = 0f;
                        
                        float dot_damage = 0f;
                        float dot_damage_percent = 0f;
                        int dot_count = 0;
                        int dot_miss_count = 0;
                        float dot_min = 0f;
                        float dot_avg = 0f;
                        float dot_max = 0f;

                        float hit_damage = 0f;
                        float hit_damage_percent = 0f;
                        int hit_count = 0;
                        int hit_miss_count = 0;
                        float hit_min = 0f;
                        float hit_avg = 0f;
                        float hit_max = 0f;

                        float crit_damage = 0f;
                        float crit_damage_percent = 0f;
                        int crit_count = 0;
                        int crit_miss_count = 0;
                        float crit_min = 0f;
                        float crit_avg = 0f;
                        float crit_max = 0f;

                        foreach (float f in Skills[i].Damages) { total_damage += f; }   //Total damage
                        for (int j = 0; j < Skills[i].Damages.Count; j++)
                        {                            
                            if ((!Skills[i].Hits[j]) && (!Skills[i].Crits[j]))      //dot
                            {
                                if (dot_damage == 0f)
                                {
                                    dot_min = Skills[i].Damages[j];
                                    dot_max = Skills[i].Damages[j];
                                }
                                else
                                {
                                    if (Skills[i].Damages[j] < dot_min)
                                    {
                                        dot_min = Skills[i].Damages[j];
                                    }
                                    if (Skills[i].Damages[j] > dot_max)
                                    {
                                        dot_max = Skills[i].Damages[j];
                                    }
                                }
                                dot_damage += Skills[i].Damages[j];
                                dot_count++;
                                if (Skills[i].Damages[j] == 0f) { dot_miss_count++; }
                            }
                            else if (!Skills[i].Crits[j])                           //hit
                            {
                                if (hit_damage == 0f)
                                {
                                    hit_min = Skills[i].Damages[j];
                                    hit_max = Skills[i].Damages[j];
                                }
                                else
                                {
                                    if (Skills[i].Damages[j] < hit_min)
                                    {
                                        hit_min = Skills[i].Damages[j];
                                    }
                                    if (Skills[i].Damages[j] > hit_max)
                                    {
                                        hit_max = Skills[i].Damages[j];
                                    }
                                }
                                hit_damage += Skills[i].Damages[j];
                                hit_count++;
                                if (Skills[i].Damages[j] == 0f) { hit_miss_count++; }
                            }
                            else                                                    //crit
                            {
                                if (crit_damage == 0f)
                                {
                                    crit_min = Skills[i].Damages[j];
                                    crit_max = Skills[i].Damages[j];
                                }
                                else
                                {
                                    if (Skills[i].Damages[j] < crit_min)
                                    {
                                        crit_min = Skills[i].Damages[j];
                                    }
                                    if (Skills[i].Damages[j] > crit_max)
                                    {
                                        crit_max = Skills[i].Damages[j];
                                    }
                                }
                                crit_damage += Skills[i].Damages[j];
                                crit_count++;
                                if (Skills[i].Damages[j] == 0f) { crit_miss_count++; }
                            }
                        }
                        dot_avg = dot_damage / dot_count;
                        hit_avg = hit_damage / hit_count;
                        crit_avg = crit_damage / crit_count;
                        dot_damage_percent = ((dot_damage * 100) / total_damage);
                        hit_damage_percent = ((hit_damage * 100) / total_damage);
                        crit_damage_percent = ((crit_damage * 100) / total_damage);

                        float rotation = 0f;
                        int color_index = 0;
                        if (dot_damage > 0f)
                        {
                            CreateCircle(Details_BottomCircleContent, Circle, CircleRectTransform, dot_damage_percent, rotation, color_index);
                            rotation += (dot_damage_percent / 100) * 360;
                            if (!Details_BottomContent_prefab.IsNullOrDestroyed())
                            {
                                CreateDetailsBottom(color_index, "Dot", System.Convert.ToInt32(dot_min), System.Convert.ToInt32(dot_avg), System.Convert.ToInt32(dot_max), dot_count, dot_damage_percent);
                            }
                            color_index++;
                        }
                        if (hit_damage > 0f)
                        {
                            CreateCircle(Details_BottomCircleContent, Circle, CircleRectTransform, hit_damage_percent, rotation, color_index);
                            rotation += (hit_damage_percent / 100) * 360;
                            if (!Details_BottomContent_prefab.IsNullOrDestroyed())
                            {
                                CreateDetailsBottom(color_index, "Hit", System.Convert.ToInt32(hit_min), System.Convert.ToInt32(hit_avg), System.Convert.ToInt32(hit_max), hit_count, hit_damage_percent);
                            }
                            color_index++;
                        }
                        if (crit_damage > 0f)
                        {
                            CreateCircle(Details_BottomCircleContent, Circle, CircleRectTransform, crit_damage_percent, rotation, color_index);
                            rotation += (crit_damage_percent / 100) * 360;
                            if (!Details_BottomContent_prefab.IsNullOrDestroyed())
                            {
                                CreateDetailsBottom(color_index, "Crit", System.Convert.ToInt32(crit_min), System.Convert.ToInt32(crit_avg), System.Convert.ToInt32(crit_max), crit_count, crit_damage_percent);
                            }
                            color_index++;
                        }
                    }
                }
            }
            public static void CreateCircle(GameObject parent, GameObject Circle, RectTransform CircleRectTransform, float percent, float rotation, int color_index)
            {
                GameObject circle = Instantiate(Circle, Vector3.zero, Quaternion.identity);
                DontDestroyOnLoad(circle);
                circle.transform.SetParent(parent.transform);
                RectTransform rect_transform = circle.GetComponent<RectTransform>();
                if ((!rect_transform.IsNullOrDestroyed()) && (!CircleRectTransform.IsNullOrDestroyed()))
                {
                    rect_transform.offsetMax = CircleRectTransform.offsetMax;
                    rect_transform.offsetMin = CircleRectTransform.offsetMin;
                }
                Image details_topcircle_image = circle.GetComponent<Image>();
                if (!details_topcircle_image.IsNullOrDestroyed())
                {
                    details_topcircle_image.color = colors[color_index];
                    details_topcircle_image.fillAmount = percent / 100;
                }
                circle.transform.Rotate(new Vector3(0, 0, (rotation * -1)));
                circle.active = true;
            }
            public static void CreateDetailsBottom(int color_index, string type, int min, int avg, int max, int count, float percent)
            {
                GameObject details_bottom = Instantiate(Details_BottomContent_prefab, Vector3.zero, Quaternion.identity);
                DontDestroyOnLoad(details_bottom);
                details_bottom.transform.SetParent(Details_BottomContent.transform);
                GameObject color = Functions.GetChild(details_bottom, "Color");
                if (!color.IsNullOrDestroyed())
                {
                    GameObject image_obj = Functions.GetChild(color, "Image");
                    {
                        Image image = image_obj.GetComponent<Image>();
                        if (!image.IsNullOrDestroyed())
                        {
                            image.color = colors[color_index];
                        }
                    }
                }
                GameObject damage_type = Functions.GetChild(details_bottom, "Type");
                if (!damage_type.IsNullOrDestroyed())
                {
                    Text text = damage_type.GetComponent<Text>();
                    if (!text.IsNullOrDestroyed()) { text.text = type; }
                }

                GameObject damage_min = Functions.GetChild(details_bottom, "Min");
                if (!damage_min.IsNullOrDestroyed())
                {
                    Text text = damage_min.GetComponent<Text>();
                    if (!text.IsNullOrDestroyed()) { text.text = System.Convert.ToString(min); }
                }
                GameObject damage_avg = Functions.GetChild(details_bottom, "Avg");
                if (!damage_avg.IsNullOrDestroyed())
                {
                    Text text = damage_avg.GetComponent<Text>();
                    if (!text.IsNullOrDestroyed()) { text.text = System.Convert.ToString(avg); }
                }
                GameObject damage_max = Functions.GetChild(details_bottom, "Max");
                if (!damage_max.IsNullOrDestroyed())
                {
                    Text text = damage_max.GetComponent<Text>();
                    if (!text.IsNullOrDestroyed()) { text.text = System.Convert.ToString(max); ; }
                }
                GameObject damage_count = Functions.GetChild(details_bottom, "Count");
                if (!damage_count.IsNullOrDestroyed())
                {
                    Text text = damage_count.GetComponent<Text>();
                    if (!text.IsNullOrDestroyed()) { text.text = System.Convert.ToString(count); }
                }
                GameObject damage_percent = Functions.GetChild(details_bottom, "Percent");
                if (!damage_percent.IsNullOrDestroyed())
                {
                    Text text = damage_percent.GetComponent<Text>();
                    if (!text.IsNullOrDestroyed()) { text.text = System.Convert.ToInt32(percent) + " %"; }
                }
                details_bottom.active = true;
            }
        }
        public class Events
        {
            public static void Set(Button btn, UnityEngine.Events.UnityAction action)
            {
                if (!btn.IsNullOrDestroyed())
                {
                    btn.onClick = new Button.ButtonClickedEvent();
                    btn.onClick.AddListener(action);
                }
            }
            public static void Set_Toggle_Event(Toggle toggle, UnityEngine.Events.UnityAction<bool> action)
            {
                toggle.onValueChanged = new Toggle.ToggleEvent();
                toggle.onValueChanged.AddListener(action);
            }
            public static void Set_DropDown_Event(Dropdown dropdown, UnityEngine.Events.UnityAction<int> action)
            {
                dropdown.onValueChanged = new Dropdown.DropdownEvent();
                dropdown.onValueChanged.AddListener(action);
            }

            public static readonly System.Action ToggleVisibility_OnClick_Action = new System.Action(ToggleVisibility_Click);
            public static void ToggleVisibility_Click()
            {
                DamageMeter_obj.active = !DamageMeter_obj.active;
            }

            public static readonly System.Action OnOff_OnClick_Action = new System.Action(OnOff_Click);
            public static void OnOff_Click()
            {
                On = !On;
            }

            public static readonly System.Action Reset_OnClick_Action = new System.Action(Reset_Click);
            public static void Reset_Click()
            {
                UI.Reset();                
            }

            public static readonly System.Action Settings_OnClick_Action = new System.Action(Settings_Click);
            public static void Settings_Click()
            {
                Settings_panel.active = !Settings_panel.active;
                DamageMeter_content.active = !Settings_panel.active;
            }

            public static readonly System.Action OpenDetails_OnClick_Action = new System.Action(OpenDetails);
            public static void OpenDetails()
            {
                UI.OpenDetails();
            }

            public static readonly System.Action CloseDetails_OnClick_Action = new System.Action(CloseDetails);
            public static void CloseDetails()
            {
                UI.CloseDetails();
            }

            public static readonly System.Action<int> DamageType_Dropdown_Action = new System.Action<int>(Set_DamageType);
            private static void Set_DamageType(int value)
            {
                if ((!Save_Manager.instance.IsNullOrDestroyed()) && (!DamageType_Dropdown.IsNullOrDestroyed()))
                {
                    Save_Manager.instance.data.DamageMeter.DamageType = DamageType_Dropdown.value;
                }
            }

            public static readonly System.Action<bool> SeparateDot_Toggle_Action = new System.Action<bool>(Set_SeparateDot_Enable);
            private static void Set_SeparateDot_Enable(bool enable)
            {
                if ((!Save_Manager.instance.IsNullOrDestroyed()) && (!SeparateDot_Toggle.IsNullOrDestroyed()))
                {
                    Save_Manager.instance.data.DamageMeter.SeparateHitAndDot = SeparateDot_Toggle.isOn;
                }
            }

            //Select Top Skill
            [HarmonyPatch(typeof(Button), "Press")]
            public class Button_Press
            {
                [HarmonyPostfix]
                static void Postfix(ref Button __instance)
                {
                    try
                    {
                        if (Details_panel.active)
                        {
                            if (__instance.name.Contains(Details_TopSkillsName))
                            {
                                int i = System.Convert.ToInt32(__instance.name.Split('_')[1]);
                                UI.ShowDetails_Bottom(i);
                            }
                        }
                    }
                    catch { }
                }
            }
        }
        public class DamageDeal
        {
            [HarmonyPatch(typeof(AbilityEventListener), "DetailedAbilityEvent")]
            public class AbilityEventListener_DetailedAbilityEvent
            {
                [HarmonyPostfix]
                static void Postfix(DetailedAbilityEvent __0)
                {
                    if ((Scenes.IsGameScene()) && (On))
                    {
                        bool target_is_player = false;
                        if (__0.target == Refs_Manager.player_actor) { target_is_player = true; }
                        bool target_is_summon = false;
                        var summons = PlayerSummons();
                        if (summons != null)
                        {
                            foreach (Summoned summoned in summons)
                            {
                                if (summoned.actor == __0.target) { target_is_summon = true; break; }
                            }
                        }
                        if ((!target_is_player) && (!target_is_summon))
                        {
                            Record(__0.ability.abilityName, __0.damageDealt, __0.hit, __0.crit, __0.kill, __0.overkill, __0.target.name, null);
                        }
                    }
                }
            }

            static void Record(string name, float damage, bool hit, bool crit, bool kill, float overkill, string target_name, Sprite icon)
            {
                TotalDamageDeal += damage;
                foreach (Skill skill in Skills)
                {
                    if (skill.AbilityName == name)
                    {
                        skill.Damages.Add(damage);
                        skill.Hits.Add(hit);
                        skill.Crits.Add(crit);
                        skill.Kills.Add(kill);
                        skill.Overkills.Add(overkill);
                        skill.TargetName.Add(target_name);
                        return;
                    }
                }
                UI.AddSkill(name, damage, hit, crit, kill, overkill, target_name, icon);
            }

            static bool IsPlayerOrSummon(Actor actor)
            {
                if (actor.IsNullOrDestroyed()) { return false; }
                if (actor == Refs_Manager.player_actor) { return true; }
                var summons = PlayerSummons();
                if (summons != null)
                {
                    foreach (Summoned summoned in summons)
                    {
                        if (summoned.actor == actor) { return true; }
                    }
                }
                return false;
            }

            // Ailment ticks never raise DetailedAbilityEvent; they reach ProtectionClass.ApplyDamage with the
            // ActiveAilment as their DamageSource. ApplyDamage's ValueTuple result does not marshal through the
            // detour, so the amount is read from the BaseHealth.HealthDamage call it makes on the target.
            // Ticks arrive every frame per target, so they are summed here and written to the rows by
            // FlushAilments rather than stored one by one.
            struct PendingAilment
            {
                public Ailment Ailment;
                public float Damage;
                public string TargetName;
            }
            static readonly System.Collections.Generic.Dictionary<string, PendingAilment> pending_ailments = new System.Collections.Generic.Dictionary<string, PendingAilment>();
            static readonly System.Collections.Generic.Dictionary<string, LoadRef<Sprite>> ailment_icons = new System.Collections.Generic.Dictionary<string, LoadRef<Sprite>>();
            static bool ailment_logged = false;
            static Ailment current_ailment = null;
            static BaseHealth current_target = null;

            [HarmonyPatch(typeof(ProtectionClass), "ApplyDamage")]
            public class ProtectionClass_ApplyDamage
            {
                [HarmonyPrefix]
                static void Prefix(ProtectionClass __instance, DamageSource __1)
                {
                    current_ailment = null;
                    current_target = null;
                    if (!On || __1 == null || !Scenes.IsGameScene()) { return; }
                    var info = __1.GetDamageSourceInfo();
                    Ailment ailment = info.Item1;
                    if (ailment.IsNullOrDestroyed()) { return; } //Hits are counted by DetailedAbilityEvent
                    Actor target = __instance.actor;
                    if (target.IsNullOrDestroyed() || !IsPlayerOrSummon(info.Item5) || IsPlayerOrSummon(target)) { return; }
                    current_ailment = ailment;
                    current_target = target.health;
                }
                [HarmonyPostfix]
                static void Postfix()
                {
                    current_ailment = null;
                    current_target = null;
                }
            }

            [HarmonyPatch(typeof(BaseHealth), "HealthDamage")]
            public class BaseHealth_HealthDamage
            {
                [HarmonyPrefix]
                static void Prefix(BaseHealth __instance, float __0)
                {
                    if (current_ailment == null || __0 <= 0f || __instance != current_target) { return; }
                    string name = AilmentName(current_ailment);
                    if (!ailment_logged)
                    {
                        ailment_logged = true;
                        Main.logger_instance.Msg("Damage Meter : first ailment tick " + name + " damage=" + __0);
                    }
                    pending_ailments.TryGetValue(name, out PendingAilment pending);
                    pending.Ailment = current_ailment;
                    pending.Damage += __0;
                    pending.TargetName = __instance.gameObject.name;
                    pending_ailments[name] = pending;
                }
            }

            static string AilmentName(Ailment ailment)
            {
                string name = ailment.displayName;
                if (string.IsNullOrEmpty(name)) { name = ailment.name; }
                return name ?? "Ailment";
            }

            static Sprite AilmentIcon(string name, Ailment ailment)
            {
                if (!ailment_icons.TryGetValue(name, out LoadRef<Sprite> loadRef))
                {
                    SoftRef<Sprite> softRef = ailment.getIcon();
                    if (softRef == null || !softRef) { return null; }
                    loadRef = SoftRefExtensions.CreateLoadRef(softRef, "LastEpoch_Hud", 0);
                    if (loadRef == null) { return null; }
                    loadRef.BlockForLoad();
                    ailment_icons[name] = loadRef;
                }
                return loadRef.AssetOrNull;
            }

            public static void FlushAilments()
            {
                if (pending_ailments.Count == 0) { return; }
                foreach (var entry in pending_ailments)
                {
                    Record(entry.Key, entry.Value.Damage, false, false, false, 0f, entry.Value.TargetName, AilmentIcon(entry.Key, entry.Value.Ailment));
                }
                pending_ailments.Clear();
            }

            public static void ClearPending()
            {
                pending_ailments.Clear();
            }
        }
        public struct Skill
        {
            public GameObject Obj;
            public Sprite Icon;
            public string AbilityName;
            public System.Collections.Generic.List<float> Damages;
            public System.Collections.Generic.List<bool> Hits;
            public System.Collections.Generic.List<bool> Crits;
            public System.Collections.Generic.List<bool> Kills;
            public System.Collections.Generic.List<float> Overkills;
            public System.Collections.Generic.List<string> TargetName;
        }
    }
}
