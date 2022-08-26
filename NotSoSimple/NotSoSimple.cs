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
        public override string Version => "1.0.0";

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
                        users.Add(___User);
                        Slot slot = __instance.Slot;
                        AddProtections(__instance.Slot);
                    }
                });

            }
        }

        static void AddProtections(Slot slot)
        {
            if (config.GetValue(Savent)) slot.AttachComponent<ChildrenSaveBlocker>().Persistent = false;
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
            static bool OnSaving(SaveControl control, ChildrenSaveBlocker __instance)
            {
                foreach (CloudUserRef user in users)
                {
                    if (user.UserId == __instance.LocalUser.UserID) return false;
                }
                control.OnBeforeSaveStart(delegate
                {
                    List<Slot> markPersistent = Pool.BorrowList<Slot>();
                    foreach (Slot child in __instance.Slot.Children)
                    {
                        if (child.PersistentSelf)
                        {
                            markPersistent.Add(child);
                            child.PersistentSelf = false;
                        }
                    }

                    control.OnSaved(delegate
                    {
                        foreach (Slot item in markPersistent)
                        {
                            item.PersistentSelf = true;
                        }

                        Pool.Return(ref markPersistent);
                    });
                });

                return false;
            }
        }
    }
}