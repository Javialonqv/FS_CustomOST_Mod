using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;

namespace FS_CustomOST
{
    [HarmonyPatch(typeof(Controls), nameof(Controls.KillCharacter), new Type[] { typeof(bool), typeof(bool) })]
    public static class KillPatch
    {
        public static void Prefix()
        {
            Melon<OST_Main>.Instance.SaveOSTCurrentTime();
        }

        public static void Postfix()
        {
            Melon<OST_Main>.Instance.OnDead();
        }
    }

    [HarmonyPatch(typeof(InGameUIManager), nameof(InGameUIManager.OnResumeGame))]
    public static class ResumePatch
    {
        public static bool Prefix()
        {
            return true;
        }
    }
}
