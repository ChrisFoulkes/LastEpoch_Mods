using MelonLoader;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Items
{
    [RegisterTypeInIl2Cpp]
    public class Items_AutoStore_WithTimer : MonoBehaviour
    {
        public static Items_AutoStore_WithTimer instance { get; private set; }
        public Items_AutoStore_WithTimer(System.IntPtr ptr) : base(ptr) { }

        public static System.DateTime StartTime;
        public static System.DateTime LastTime;
        public static bool running = false;

        void Awake()
        {
            instance = this;
        }
        void Update()
        {
            if ((Scenes.IsGameScene()) && (!Save_Manager.instance.IsNullOrDestroyed()) && (!Save_Manager.instance.data.IsNullOrDestroyed()))
            {
                if (Save_Manager.instance.data.Items.Pickup.Enable_AutoStore_Timer)
                {
                    if (!running)
                    {
                        StartTime = System.DateTime.Now;
                        running = true;
                    }
                    if (running)
                    {
                        if (GetElapsedTime() > Save_Manager.instance.data.Items.Pickup.AutoStore_Timer)
                        {
                            Items_AutoStore_OnPickup.StoreNow();
                            running = false;
                        }
                    }
                }
            }
            else { running = false; }
        }
        double GetElapsedTime()
        {
            LastTime = System.DateTime.Now;
            var elaspedTime = LastTime - StartTime;

            return elaspedTime.TotalSeconds;
        }
    }
}
