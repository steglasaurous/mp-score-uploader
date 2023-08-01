using HarmonyLib;

namespace MPScoreUploader
{
    internal class RuntimePatch
    {
        public static void PatchAll()
        {
            HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("dev.kk964.websockets");
            harmony.PatchAll();

            foreach (var method in harmony.GetPatchedMethods())
            {
                MPScoreUploader.Instance.LoggerInstance.Msg($"[Websocket] Successfully patched \"{method.Name}\"");
            }
        }
    }

}

[HarmonyPatch(typeof(GameControlManager), "Awake")]
public class GameControlManagerPatch
{
    [HarmonyPostfix]
    public static void PostFix()
    {
        MPScoreUploader.MPScoreUploader.Instance.GameManagerInit();
    }
}
