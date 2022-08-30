using BaseX;
using FrooxEngine;
using FrooxEngine.CommonAvatar;
using HarmonyLib;
using NeosModLoader;
using System.Collections.Generic;

namespace NotSoSimple
{
    public class Patch : NeosMod
    {
        public override string Name => "NotSoSimple";
        public override string Author => "Sox & LeCloutPanda";
        public override string Version => "1.2.0";

        public static ModConfiguration config;
        private static List<CloudUserRef> users = new List<CloudUserRef>();

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> Savent = new ModConfigurationKey<bool>("Savent", "Prevent others being able to save protected items.", () => true);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> Equipnt = new ModConfigurationKey<bool>("Equipnt", "Prevent others from being able to equip protected avatars.", () => true);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> Grabnt = new ModConfigurationKey<bool>("Grabnt", "Prevent others from being able to grab protected items.", () => true);

        public override void OnEngineInit()
        {
            config = GetConfiguration();
            config.Save(true);

            Harmony harmony = new Harmony($"dev.{Author}.{Name}");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(SimpleAvatarProtection))]
        class PatchSimpleAvatarProtection
        {
            [HarmonyPrefix]
            [HarmonyPatch("OnAwake")]
            static void OnAwake(SimpleAvatarProtection __instance, CloudUserRef ___User)
            {
                __instance.RunInUpdates(3, () =>
                {
                    if (___User.UserId == __instance.LocalUser.UserID)
                    {
                        AddProtections(__instance.Slot);
                    }
                });

            }
        }

        static void AddProtections(Slot slot)
        {
            if (config.GetValue(Savent))
            {
                var Ligma = slot.AttachComponent<ValueUserOverride<bool>>();
                Ligma.Persistent = false;
                var Balls = slot.AttachComponent<ChildrenSaveBlocker>();
                Balls.Persistent = false;
                Ligma.Target.Value = Balls.EnabledField.ReferenceID;
                Ligma.Default.Value = true;
                Ligma.SetOverride(slot.LocalUser, false);
            }
            if (config.GetValue(Equipnt))
            {
                var Ligma = slot.AttachComponent<ValueUserOverride<bool>>();
                Ligma.Persistent = false;
                var Balls = slot.AttachComponent<AvatarEquipBlock>();
                Balls.Persistent = false;
                Ligma.Target.Value = Balls.EnabledField.ReferenceID;
                Ligma.Default.Value = true;
                Ligma.SetOverride(slot.LocalUser, false);
            }
            if (config.GetValue(Grabnt))
            {
                var Ligma = slot.AttachComponent<ValueUserOverride<bool>>();
                Ligma.Persistent = false;
                var Balls = slot.AttachComponent<GrabBlock>();
                Balls.Persistent = false;
                Ligma.Target.Value = Balls.EnabledField.ReferenceID;
                Ligma.Default.Value = true;
                Ligma.SetOverride(slot.LocalUser, false);
            }
        }

        [HarmonyPatch(typeof(ChildrenSaveBlocker))]
        class PatchChildrenSaveBlocker
        {
            [HarmonyPrefix]
            [HarmonyPatch("OnSaving")]
            static bool OnSaving(ChildrenSaveBlocker __instance)
            {
                if (__instance.EnabledField == true)
                    return true;
                else if (__instance.EnabledField == false)
                    return false;

                return true;
            }
        }
    }
}