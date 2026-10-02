using HarmonyLib;
using Il2Cpp;

namespace LastEpoch_Hud.Scripts.Mods.Skills
{
    public class Skills_Level
    {
        public static bool CanRun()
        {
            if ((Scenes.IsGameScene()) && (!Save_Manager.instance.IsNullOrDestroyed()) && (!Refs_Manager.player_treedata.IsNullOrDestroyed()))
            {
                if ((!Save_Manager.instance.data.IsNullOrDestroyed()) && (!Refs_Manager.player_treedata.specialisedSkillTrees.IsNullOrDestroyed()))
                {
                    return Save_Manager.instance.data.Skills.Enable_SkillLevel;
                }
                else { return false; }
            }
            else { return false; }
        }

        public static byte ChosenLevel()
        {
            int level = (int)Save_Manager.instance.data.Skills.SkillLevel;
            if (level < 0) { level = 0; }
            if (level > byte.MaxValue) { level = byte.MaxValue; }
            return (byte)level;
        }

        static bool writing;

        static void Apply(LocalTreeData tree, Ability ability)
        {
            if (writing || !CanRun() || tree == null || ability.IsNullOrDestroyed() || tree.specialisedSkillTrees.IsNullOrDestroyed()) { return; }
            byte level = ChosenLevel();
            foreach (LocalTreeData.SkillTreeData data in tree.specialisedSkillTrees)
            {
                if (data == null || data.ability.IsNullOrDestroyed()) { continue; }
                if (data.ability.abilityName != ability.abilityName) { continue; }
                writing = true;
                data.level = level;
                writing = false;
                return;
            }
        }

        [HarmonyPatch(typeof(SkillsPanelManager), "OnOpenSkillTree")]
        public class SkillsPanelManager_OnOpenSkillTree
        {
            [HarmonyPostfix]
            static void Postfix(SkillsPanelManager __instance, SkillTree __0)
            {
                try
                {
                    if (__0.IsNullOrDestroyed()) { return; }
                    Apply(Refs_Manager.player_treedata, __0.ability);
                    if (!__instance.IsNullOrDestroyed()) { __instance.updateVisuals(false); }
                }
                catch { Main.logger_instance?.Msg("SkillsPanelManager.OnOpenSkillTree() ERROR"); }
            }
        }

        [HarmonyPatch(typeof(LocalTreeData), "getAbilityLevel")]
        public class LocalTreeData_getAbilityLevel
        {
            [HarmonyPostfix]
            static void Postfix(LocalTreeData __instance, Ability __0, ref byte __result)
            {
                if (writing || !CanRun() || __0.IsNullOrDestroyed() || __instance.specialisedSkillTrees.IsNullOrDestroyed()) { return; }
                byte level = ChosenLevel();
                foreach (LocalTreeData.SkillTreeData data in __instance.specialisedSkillTrees)
                {
                    if (data == null || data.ability.IsNullOrDestroyed()) { continue; }
                    if (data.ability.abilityName != __0.abilityName) { continue; }
                    writing = true;
                    data.level = level;
                    writing = false;
                    __result = level;
                    return;
                }
            }
        }
    }
}
