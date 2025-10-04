using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Steamworks;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Nessie.ATLYSS.EasySettings;
using UnityEngine;

namespace ServerPrioritizer;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("EasySettings", BepInDependency.DependencyFlags.HardDependency)]
[BepInProcess("ATLYSS.exe")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private static ConfigEntry<string> _desiredLobbyNameConfig;
    private static string _cachedDesiredLobbyName;

    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        _desiredLobbyNameConfig = Config.Bind(
            "General",
            "ServerName",
            "Eu 24/7 SFW",
            "Enter the name of the ATLYSS server you want to see at the top of the list here."
        );

        _cachedDesiredLobbyName = _desiredLobbyNameConfig.Value;

        Logger.LogInfo("EasySettings found – config UI enabled.");

        Settings.OnInitialized.AddListener(() =>
        {
            SettingsTab tab = Settings.ModTab;
            tab.AddHeader("Server Prioritizer");
            tab.AddTextField("Server Name", _desiredLobbyNameConfig, "Server Name");
        });
        Settings.OnApplySettings.AddListener(() => 
        {
            Config.Save();
            _cachedDesiredLobbyName = _desiredLobbyNameConfig.Value;
            Logger.LogInfo($"Desired server name set to '{_cachedDesiredLobbyName}'");
        });

        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

        Logger.LogInfo($"Desired server name set to '{_cachedDesiredLobbyName}'");
    }

    [HarmonyPatch(typeof(LobbyListManager))]
    [HarmonyPatch("Iterate_SteamLobbies")]
    public class IterateSteamLobbiesPatch
    {
        public static void Postfix(LobbyListManager __instance)
        {
            if (string.IsNullOrWhiteSpace(_cachedDesiredLobbyName) || __instance._lobbyListEntries == null || __instance._lobbyListEntries.Count == 0)
            {
                Logger.LogInfo("_lobbyListEntries is empty");
                return;
            }

            GameObject prioritizedLobbyGameObject = null;
            int prioritizedLobbyIndex = -1;

            for (int i = 0; i < __instance._lobbyListEntries.Count; i++)
            {
                GameObject currentLobbyGo = __instance._lobbyListEntries[i];

                LobbyDataEntry lobEntry = currentLobbyGo.GetComponent<LobbyDataEntry>();

                if (lobEntry != null && lobEntry._lobbyName == _cachedDesiredLobbyName)
                {
                    prioritizedLobbyGameObject = currentLobbyGo;
                    prioritizedLobbyIndex = i;
                    break;
                }
            }

            if (prioritizedLobbyGameObject != null && prioritizedLobbyIndex != 0)
            {
                __instance._lobbyListEntries.RemoveAt(prioritizedLobbyIndex);
                __instance._lobbyListEntries.Insert(0, prioritizedLobbyGameObject);
                
                prioritizedLobbyGameObject.transform.SetSiblingIndex(0);

                Logger.LogInfo($"Moved '{_cachedDesiredLobbyName}' to the top of the server list.");
            }
        }
    }

    [HarmonyPatch(typeof(SteamLobby))]
    [HarmonyPatch("GetLobbiesList")]
    public static class GetLobbiesListPatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            MethodInfo targetMethod = AccessTools.Method(typeof(SteamMatchmaking), nameof(SteamMatchmaking.AddRequestLobbyListResultCountFilter));

            for (int i = 0; i < codes.Count; i++)
            { 
                if (codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo method && method == targetMethod)
                {
                    if (i > 0 && codes[i-1].opcode == OpCodes.Ldc_I4_S && (sbyte)codes[i-1].operand == 60)
                    {
                        codes.RemoveAt(i - 1); // removes the filter instruction
                        codes.RemoveAt(i - 1); // removes the call instruction that calls the filter

                        Logger.LogInfo("Successfully patched out SteamMatchmaking.AddRequestLobbyListResultCountFilter(60).");
                        i -= 2;
                    }
                }
            }

            return codes;
        }
    }
}
