using EasyAllBobble;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Mono.Cecil;
using System.Reflection;
using UnityEngine;

namespace EasyDeliveryCoPlugin;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        Harmony val = new Harmony(MyPluginInfo.PLUGIN_NAME);
        val.PatchAll();
    }

    [HarmonyPatch]
    public class DashPatch : HarmonyPatch
    {
        private static MethodInfo TargetMethod()
        {
            return typeof(CarDamage).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        private static void Postfix(CarDamage __instance)
        {
            var displayBobble = __instance.transform.Find("BobbleHead").gameObject;
            var bobble2 = Instantiate(displayBobble, new Vector3(-1.0831f, 0.8019f, 0.0809f), Quaternion.Euler(0, 117.1353f, 0));

            // add to same parent as displayBobble
            bobble2.transform.parent = displayBobble.transform.parent;
        }
    }

    [HarmonyPatch]
    public class DisableDisplayBobblePatch : HarmonyPatch
    {
        private static MethodInfo TargetMethod()
        {
            return typeof(SnowcatManager).GetMethod("SetDisplayBobble", BindingFlags.Instance | BindingFlags.NonPublic);
        }
        private static void Postfix(SnowcatManager __instance)
        {
            __instance.carDisplayBobble.gameObject.SetActive(value: false);
        }
    }


    [HarmonyPatch]
    public class SnowcatPatch : HarmonyPatch
    {
        enum SnowCats
        {
            SD30 = 1,
            SD66,
            SD49,
            SD11,
            SD24,
            SD02,
            SD72,
            SD55,
            SD56,
            SD08,
            SD51,
            SD19,
            DD78
        }

        static SnowCats[] order = new SnowCats[]
        {
            SnowCats.SD24,
            SnowCats.SD56,
            SnowCats.DD78,
            SnowCats.SD55,
            SnowCats.SD49,
            SnowCats.SD02,
            SnowCats.SD19,
            SnowCats.SD66,
            SnowCats.SD08,
            SnowCats.SD72,
            SnowCats.SD51,
            SnowCats.SD30,
            SnowCats.SD11
        };


        private static MethodInfo TargetMethod()
        {
            return typeof(SnowcatManager).GetMethod("SetBobbles", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        private static void CreateBobbles(SnowcatManager snowcatManagaer, GameObject displayBobble, Transform carInt)
        {
            Logger.LogInfo("Creating bobbles...");
            var bobbleGO = new GameObject("Bobbles");
            bobbleGO.transform.parent = carInt;
            bobbleGO.transform.localPosition = displayBobble.transform.localPosition;
            bobbleGO.transform.localRotation = Quaternion.Euler(0, 0, 0);
            bobbleGO.transform.localScale = new Vector3(1, 1, 1);
            //snowcatManagaer.snowcatsActive.Length
            for (int i = 1; i <= order.Length; i++)
            {
                SnowCats idx = order[order.Length - i];

                var bobble = Instantiate(displayBobble, bobbleGO.transform);
                bobble.name = "Bobble" + (int)idx;
                bobble.transform.localPosition = new Vector3(-1.06f + (i - 1) * 0.175f, 0.0f, i < 5 ? -0.05f : 0.00f);
                bobble.transform.localRotation = Quaternion.Euler(0, 180, 0);
                bobble.transform.localScale = new Vector3(0.1406f, 0.1406f, 0.1406f);
                bobble.SetActive(true);

                Swing bswing = bobble.GetComponentInChildren<Swing>();

                // jitter swing parameters for bobbles, except DD78
                if (idx != SnowCats.DD78 )
                {
                    bswing.dampen = 0.98f + Random.Range(-0.004f, 0.004f);
                    if (idx == SnowCats.SD11) bswing.dampen = 0.991f;
                    bswing.spring = 50.0f + Random.Range(-5.0f, 5.0f);
                }

                MeshRenderer[] componentsInChildren = bobble.GetComponentsInChildren<MeshRenderer>();
                for (int j = 0; j < componentsInChildren.Length; j++)
                {
                    componentsInChildren[j].material.SetTexture("_BaseMap", snowcatManagaer.npcTextures[(int)idx - 1]);
                }

            }
        }

        private static void Postfix(SnowcatManager __instance)
        {
            var carInt = __instance.carDisplayBobble.transform.parent;

            __instance.carDisplayBobble.gameObject.SetActive(value:false);

            var bobbleGO = __instance.carDisplayBobble.transform.parent.Find("Bobbles");
            if (bobbleGO == null)
            {
                CreateBobbles(__instance, __instance.carDisplayBobble, carInt);
            }

            bobbleGO = __instance.carDisplayBobble.transform.parent.Find("Bobbles");
            for (int i = 1; i <= order.Length; i++)
            {
                bobbleGO.Find("Bobble" + i).gameObject.SetActive( __instance.snowcatsActive[i - 1]);
            }
        }
    }
}
