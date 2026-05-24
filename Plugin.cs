using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MonoMod.RuntimeDetour;
using ModMenu.Behaviors;

namespace ModMenu
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency(WKLib.WKLibPlugin.GUID)]
    internal sealed class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log { get; private set; }
        
        private void Awake()
        {
            Log = Logger;
            
            var harmony = new Harmony(PluginInfo.PLUGIN_GUID + ".patch");
            harmony.PatchAll();

            new Hook(AccessTools.Method(typeof(UI_SettingsMenu), "Start"), UI_SettingsMenu_StartHook);

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        private static void UI_SettingsMenu_StartHook(Action<UI_SettingsMenu> orig, UI_SettingsMenu self)
        {
            Templates.LoadTemplates(self.transform);
            if(!self.GetComponent<ModMenuBehavior>())
                self.gameObject.AddComponent<ModMenuBehavior>();
        }
    }
}
