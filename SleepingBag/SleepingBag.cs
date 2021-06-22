using BepInEx;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using Logger = Jotunn.Logger;
using MonoMod.RuntimeDetour;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using static MonoMod.Cil.RuntimeILReferenceBag.FastDelegateInvokers;

namespace SleepingBag
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class SleepingBag : BaseUnityPlugin
    {
        public const string PluginGUID = "com.SawlGoodman.ValheimSleepingBag";
        public const string PluginName = "Valheim Sleeping Bag";
        public const string PluginVersion = "1.0.0";
        private AssetBundle embeddedResourceBundle;
        private GameObject sleepingbag_item_pfb;
        private GameObject sleepingbag_piece_pfb;

        private void Awake()
        {
            // Load asset bundle from embedded resources
            embeddedResourceBundle = AssetUtils.LoadAssetBundleFromResources("sleepingbag", typeof(SleepingBag).Assembly);
            // Create item and piece from loaded asset bundle
            CreateSleepingBag();
            // Create localization
            Localization.CreateLocalization();
            // Clean up
            embeddedResourceBundle.Unload(false);
            // Create a hook to modify the bed's usage rules
            On.Bed.Interact += Bed_Interact;
        }

        private bool Bed_Interact(On.Bed.orig_Interact orig, Bed self, Humanoid human, bool repeat)
        {
            // this function overrides the Bed.Interact method to bypass roof check if the GameObject is a sleeping bag.
            if (repeat)
                return false;
            long myself = Game.instance.GetPlayerProfile().GetPlayerID();
            long belongsTo = self.GetOwner();
            Player player1 = human as Player;
            
            // if it doesn't belong to anybody
            if (belongsTo == 0L)
            {
                //then check if it is a sleeping bag, if so, bypass Roof Check
                if (!(self.gameObject.name == "sleepingbag_piece(Clone)") && !(self.gameObject.name == "sleepingbag_piece"))
                { 
                if (!self.CheckExposure(player1))
                    return false;
                }
                // now, it's mine
                self.SetOwner(myself, Game.instance.GetPlayerProfile().GetName());
                Game.instance.GetPlayerProfile().SetCustomSpawnPoint(self.GetSpawnPoint());
                human.Message(MessageHud.MessageType.Center, "$msg_spawnpointset");
            }
            //if it is mine
            else if (self.IsMine())
            {
                //if it's my current spawnpoint
                if (self.IsCurrent())
                {
                    //is it time to sleep ? else prevent sleeping
                    if (!EnvMan.instance.IsAfternoon() && !EnvMan.instance.IsNight())
                    {
                        human.Message(MessageHud.MessageType.Center, "$msg_cantsleep");
                        return false;
                    }
                    //all clear ? warm ? dry ? else prevent sleeping 
                    if (!self.CheckEnemies(player1) || (!self.CheckFire(player1) || !self.CheckWet(player1)))
                    {
                        //heck if it is a sleeping bag, if so, bypass Roof Check and go to sleep !
                        if (!(self.gameObject.name == "sleepingbag_piece(Clone)") && !(self.gameObject.name == "sleepingbag_piece"))
                        {
                            if (!self.CheckExposure(player1))
                            return false;
                        }
                    }
                    human.AttachStart(self.m_spawnPoint, true, true, "attach_bed", new Vector3(0.0f, 0.5f, 0.0f));
                    return false;
                }
                //then check if it is a sleeping bag, if so, bypass Roof Check
                if (!(self.gameObject.name == "sleepingbag_piece(Clone)") && !(self.gameObject.name == "sleepingbag_piece")) { 
                if (!self.CheckExposure(player1))
                    return false;
                }
                //else, define as current
                Game.instance.GetPlayerProfile().SetCustomSpawnPoint(self.GetSpawnPoint());
                human.Message(MessageHud.MessageType.Center, "$msg_spawnpointset");
            }
            return false;
           
        }

        private void CreateSleepingBag()
        {
            // Create and add the sleeping bag item
            sleepingbag_item_pfb = embeddedResourceBundle.LoadAsset<GameObject>("sleepingbag_item");
            CustomItem sleepingbag_item = new CustomItem(sleepingbag_item_pfb, false);
            ItemManager.Instance.AddItem(sleepingbag_item);

            // Create and add a recipe for the sleeping bag item
            CustomRecipe sleepingbag_item_rcp = new CustomRecipe(new RecipeConfig()
            {
                Item = "sleepingbag_item",
                CraftingStation = "piece_workbench",
                Amount = 1,
                Requirements = new RequirementConfig[]
                {
            new RequirementConfig {Item = "DeerHide", Amount = 3},
            new RequirementConfig {Item = "LeatherScraps", Amount = 2 }   
                }
            });
            ItemManager.Instance.AddRecipe(sleepingbag_item_rcp);

            //Create and add the sleeping bag piece
            sleepingbag_piece_pfb = embeddedResourceBundle.LoadAsset<GameObject>("sleepingbag_piece");
            var sleepingbag_piece = new CustomPiece(sleepingbag_piece_pfb,
                new PieceConfig
                {
                    PieceTable = "_HammerPieceTable",
                    Requirements = new[]
                    {
                new RequirementConfig { Item = "sleepingbag_item", Amount = 1, Recover = true }
                    }
                });
            PieceManager.Instance.AddPiece(sleepingbag_piece);
        }


        
    }
}