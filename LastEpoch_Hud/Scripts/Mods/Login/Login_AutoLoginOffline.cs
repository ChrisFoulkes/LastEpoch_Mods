using HarmonyLib;

namespace LastEpoch_Hud.Scripts.Mods.Login
{
    public class Login_AutoLoginOffline
    {
        public static bool CanRun()
        {
            if (!Save_Manager.instance.IsNullOrDestroyed())
            {
                if (!Save_Manager.instance.data.IsNullOrDestroyed())
                {
                    return Save_Manager.instance.data.Login.Enable_AutoLoginOffline;
                }
                else { return false; }
            }
            else { return false; }
        }
        /*public static void Hide_Online(ref Il2CppLE.UI.Login.UnityUI.LandingZonePanel __instance)
        {
            if (!__instance.playOnlineButton.IsNullOrDestroyed()) { __instance.playOnlineButton.gameObject.SetActive(false); }
            if (!__instance.manageAccountButton.IsNullOrDestroyed()) { __instance.manageAccountButton.gameObject.SetActive(false); }
        }*/
        public static void AutoClickOffline(ref Il2CppLE.UI.Login.UnityUI.LandingZonePanel __instance)
        {
            // Do not call OnPlayOfflineClicked from OnEnable. On the current client that
            // hides the landing buttons and the Play Offline screen never finishes opening.
            __instance.OnPlayOfflineClicked();
        }
    }
}
