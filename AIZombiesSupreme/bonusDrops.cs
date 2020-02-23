using System;
using System.Linq;
using System.Collections;
using InfinityScript;
using static InfinityScript.GSCFunctions;

namespace AIZombiesSupreme
{
    public class bonusDrops : BaseScript
    {
        public static Action onNuke;

        public enum dropTypes
        {
            instaKill,
            doublePoints,
            ammo,
            nuke,
            gun,
            cash,
            points,
            sale,
            perk,
            freeze,
            none
        }

        public static dropTypes checkForBonusDrop()
        {
            int randomInt;
            if (roundSystem.Wave <= 5) randomInt = AIZ.rng.Next(50);
            else if (roundSystem.Wave <= 10) randomInt = AIZ.rng.Next(100);
            else randomInt = AIZ.rng.Next(150);
            if (randomInt == 5)
                return dropTypes.instaKill;
            else if (randomInt == 10)
                return dropTypes.doublePoints;
            else if (randomInt == 15)
                return dropTypes.ammo;
            else if (randomInt == 20)
                return dropTypes.nuke;
            else if (randomInt == 25)
                return dropTypes.gun;
            else if (randomInt == 30)
                return dropTypes.cash;
            else if (randomInt == 35)
                return dropTypes.points;
            else if (randomInt == 40 && !botUtil.freezerActivated)
                return dropTypes.freeze;
            else if (randomInt == 45 && !mapEdit.sale && roundSystem.Wave > 10)
                return dropTypes.sale;
            else return dropTypes.none;
        }

        public static void activateBonusDrop(Entity player, Entity bonus)
        {
            switch (bonus.GetField<string>("type"))
            {
                case "instaKill":
                    botUtil.instaKillTime += 30;
                    botUtil.startInstakill();
                    bonus.PlaySound("mp_level_up");
                    break;
                case "doublePoints":
                    botUtil.doublePointsTime += 30;
                    botUtil.startDoublePoints();
                    bonus.PlaySound("mp_level_up");
                    break;
                case "ammo":
                    foreach (Entity players in Players)
                        if (players.IsAlive) AIZ.giveMaxAmmo(players);
                    break;
                case "nuke":
                    StartAsync(doNuke(bonus));
                    break;
                case "cash":
                    bonus.PlaySound("mp_level_up");
                    foreach (Entity players in Players)
                    {
                        if (players.IsAlive && players.HasField("aizHud_created"))
                        {
                            players.SetField("cash", players.GetField<int>("cash") + 1000);
                            hud.scorePopup(players, 1000);
                            hud.scoreMessage(players, AIZ.gameStrings[97]);
                        }
                    }
                    break;
                case "points":
                    bonus.PlaySound("mp_level_up");
                    foreach (Entity players in Players)
                    {
                        if (players.IsAlive && players.HasField("aizHud_created"))
                        {
                            int points = players.GetField<int>("points");
                            points += 10;
                            players.SetField("points", points);
                            //e_hud.scorePopup(players, 10);
                            hud.scoreMessage(players, AIZ.gameStrings[98]);
                            HudElem pointNumber = players.GetField<HudElem>("hud_pointNumber");
                            pointNumber.SetValue(points);
                        }
                    }
                    break;
                case "freeze":
                    StartAsync(doFreezer(bonus));
                    break;
                case "sale":
                    mapEdit.startSale();
                    mapEdit.sale = true;
                    AfterDelay(30000, () => mapEdit.sale = false);
                    break;
                case "perk":
                    giveRandomPerkToAll();
                    break;
                case "gun":
                    player.PlayLocalSound("mp_level_up");
                    StartAsync(giveDeathMachine(player));
                    break;
                default:
                    break;
            }
            hud.showPowerUpHud(bonus.GetField<string>("type"), player);
        }
        private static IEnumerator giveDeathMachine(Entity player)
        {
            player.GiveWeapon("iw5_pecheneg_mp_thermal_rof");
            StartAsync(AIZ.switchToWeapon_delay(player, "iw5_pecheneg_mp_thermal_rof", .2f));
            player.DisableWeaponSwitch();
            player.AllowAds(false);
            player.SetPerk("specialty_rof", true, false);
            player.SetClientDvar("perk_weapRateMultiplier", "0.5");
            player.SetField("hasAlteredROF", true);
            //player.SetPerk("specialty_bulletaccuracy", true, false);
            //player.SetClientDvar("ui_drawCrosshair", "0");
            player.SetSpreadOverride(1);
            yield return Wait(30);

            if (AIZ.isPlayer(player) && player.IsAlive)
            {
                if (!player.GetField<bool>("perk5bought")) player.UnSetPerk("specialty_rof", true);
                //player.UnSetPerk("specialty_bulletaccuracy", true);
                //player.SetClientDvar("ui_drawCrosshair", "1");
                player.TakeWeapon("iw5_pecheneg_mp_thermal_rof");
                player.SwitchToWeapon(player.GetField<string>("lastDroppableWeapon"));
                player.EnableWeaponSwitch();
                player.AllowAds(true);
                player.ResetSpreadOverride();
            }
        }

        public static void spawnBonusDrop(dropTypes type, Vector3 loc)
        {
            Entity bonus = Spawn("script_model", loc + new Vector3(0, 0, 30));
            bonus.Angles = Vector3.Zero;
            if (type == dropTypes.doublePoints)
            {
                bonus.SetModel("com_plasticcase_friendly");
                Entity greenfx = SpawnFX(AIZ.fx_greenSmoke, bonus.Origin);
                TriggerFX(greenfx);
                bonus.SetField("attachedFX", greenfx);
            }
            else if (type == dropTypes.ammo)
            {
                bonus.SetModel("com_plasticcase_friendly");
            }
            else if (type == dropTypes.instaKill)
            {
                bonus.SetModel("com_plasticcase_trap_friendly");
                Entity firefx = SpawnFX(AIZ.fx_smallFire, bonus.Origin + new Vector3(0, 0, 10));
                TriggerFX(firefx);
                bonus.SetField("attachedFX", firefx);
            }
            else if (type == dropTypes.nuke)
            {
                bonus.Origin += new Vector3(0, 0, 15);
                bonus.SetModel("projectile_cbu97_clusterbomb");
                bonus.Angles = new Vector3(-90, 0, 0);
            }
            else if (type == dropTypes.freeze)
            {
                bonus.Origin += new Vector3(0, 0, 15);
                bonus.SetModel("mp_trophy_system_folded");
                bonus.Angles = new Vector3(-90, 0, 0);
            }
            else if (type == dropTypes.gun)
            {
                bonus.SetModel("weapon_pecheneg_iw5");
                Entity redfx = SpawnFX(AIZ.fx_redSmoke, bonus.Origin);
                TriggerFX(redfx);
                bonus.SetField("attachedFX", redfx);
            }
            else if (type == dropTypes.cash)
            {
                bonus.SetModel("fx_cash01");
                Entity greenfx = SpawnFX(AIZ.fx_greenSmoke, bonus.Origin);
                TriggerFX(greenfx);
                bonus.SetField("attachedFX", greenfx);
            }
            else if (type == dropTypes.points)
            {
                bonus.SetModel("prop_dogtags_foe");
            }
            else if (type == dropTypes.sale)
            {
                bonus.SetModel("com_plasticcase_dummy");
                Entity firefx = SpawnFX(AIZ.fx_smallFire, bonus.Origin);
                TriggerFX(firefx);
                bonus.SetField("attachedFX", firefx);
            }
            else if (type == dropTypes.perk)
            {
                bonus.SetModel("ims_scorpion_explosive1");
                Entity greenfx = SpawnFX(AIZ.fx_greenSmoke, bonus.Origin);
                TriggerFX(greenfx);
                bonus.SetField("attachedFX", greenfx);
            }

            bonus.SetField("type", type.ToString());
            bonus.SetField("isPowerupDrop", true);

            AfterDelay(20000, () => StartAsync(startBonusFlash(bonus)));

            OnInterval(100, () => checkForPowerupCollection(bonus));
            OnInterval(4000, () => rotatePowerup(bonus));
        }
        private static bool rotatePowerup(Entity bonus)
        {
            if (!bonus.HasField("isPowerupDrop")) return false;
            //if (!Utilities.isEntDefined(bonus)) return false;
            bonus.RotateYaw(360, 4);
            return true;
        }
        private static bool checkForPowerupCollection(Entity bonus)
        {
            if (!bonus.HasField("isPowerupDrop")) return false;
            //if (!Utilities.isEntDefined(bonus)) return false;
            foreach (Entity players in Players)
            {
                if (!players.IsAlive || players.Origin.DistanceTo(bonus.Origin) > 65 || players.SessionTeam != "allies") continue;
                if ((players.GetField<bool>("isDown") || AIZ.isWeaponDeathMachine(players.CurrentWeapon)) && bonus.GetField<string>("type") == "gun") continue;

                activateBonusDrop(players, bonus);
                if (bonus.HasField("attachedFX"))
                {
                    Entity fx = bonus.GetField<Entity>("attachedFX");
                    fx.Delete();
                    bonus.ClearField("attachedFX");
                }
                bonus.ClearField("isPowerupDrop");
                if (bonus.GetField<string>("type") != "nuke" && bonus.GetField<string>("type") != "freeze")
                {
                    PlayFX(AIZ.fx_powerupCollect, bonus.Origin, Vector3.Zero, Vector3.Zero);
                    bonus.Delete();
                }
                return false;
            }
            return true;
        }
        private static IEnumerator startBonusFlash(Entity bonus)
        {
            if (!bonus.HasField("isPowerupDrop")) yield break;

            OnInterval(2000, () => bonusFlash(bonus));

            yield return Wait(10);

            if (!bonus.HasField("isPowerupDrop")) yield break;
            if (bonus.HasField("attachedFX"))
            {
                bonus.GetField<Entity>("attachedFX").Delete();
                bonus.ClearField("attachedFX");
            }
            bonus.ClearField("isPowerupDrop");
            bonus.Delete();
        }
        private static bool bonusFlash(Entity bonus)
        {
            if (!bonus.HasField("isPowerupDrop")) return false;

            bonus.Hide();
            AfterDelay(1000, () => bonus.Show());

            if (bonus.HasField("isPowerupDrop")) return true;
            else return false;
        }

        public static IEnumerator doNuke(Entity bonus)
        {
            PhysicsExplosionSphere(bonus.Origin, 5000000, 5000000, 10);
            Entity fx = SpawnFX(AIZ.fx_nuke, bonus.Origin);
            TriggerFX(fx);
            AfterDelay(5000, () =>
                fx.Delete());
            fx.PlaySound("exp_suitcase_bomb_main");
            botUtil.nukeDetonation(null, false);
            bonus.Delete();
            yield return Wait(3.5f);

            foreach (Entity players in Players)
            {
                if (!AIZ.isPlayer(players) || !players.IsAlive || !players.HasField("cash")) continue;
                if (botUtil.doublePointsTime > 0)
                {
                    players.SetField("cash", players.GetField<int>("cash") + 800);
                    hud.scorePopup(players, 800);
                }
                else
                {
                    players.SetField("cash", players.GetField<int>("cash") + 400);
                    hud.scorePopup(players, 400);
                }
                hud.scoreMessage(players, AIZ.gameStrings[208]);
            }

            //Testing
            //SetSlowMotion(1, 0.5f, .4f);
            //AfterDelay(1000, () => SetSlowMotion(.5f, 1, 1));
        }
        private static IEnumerator doFreezer(Entity bonus)
        {
            bonus.ClearField("isPowerupDrop");
            bonus.MoveTo(bonus.Origin + new Vector3(0, 0, 3000), 5);
            yield return Wait(5);
            PlaySoundAtPos(bonus.Origin, "exp_airstrike_bomb_layer");
            PlayFX(AIZ.fx_freezer, bonus.Origin);
            botUtil.freezerActivated = true;
            bonus.Delete();
            yield return Wait(17);
            botUtil.freezerActivated = false;
        }
        public static IEnumerator giveRandomPerk(Entity player, int perk = -1)
        {
            bool[] ownedPerks = AIZ.getOwnedPerks(player);
            if (!ownedPerks.Contains(false)) yield break;//Owns all perks, give up on life...

            if (perk == -1)
            {
                perk = AIZ.rng.Next(7);
                perk++;
            }

            if (ownedPerks[perk - 1])
            {
                //re-roll
                int randomPerk = AIZ.rng.Next(7);
                randomPerk++;
                StartAsync(giveRandomPerk(player, randomPerk));
                yield break;
            }

            hud.scoreMessage(player, AIZ.gameStrings[99]);

            switch (perk)
            {
                case 1:
                    player.MaxHealth = 250;
                    player.Health = player.MaxHealth;
                    player.SetField("PerkBought", "cardicon_juggernaut_1");
                    break;
                case 2:
                    player.SetPerk("specialty_lightweight", true, true);
                    //player.SetPerk("specialty_marathon", true, true);
                    player.SetPerk("specialty_longersprint", true, true);
                    player.SetField("PerkBought", "specialty_longersprint_upgrade");
                    break;
                case 3:
                    player.SetPerk("specialty_fastreload", true, true);
                    //player.SetPerk("specialty_quickswap", true, true);
                    player.SetPerk("specialty_quickdraw", true, true);
                    player.SetField("PerkBought", "specialty_fastreload_upgrade");
                    break;
                case 4:
                    player.SetField("NewGunReady", true);
                    player.SetField("PerkBought", "specialty_twoprimaries_upgrade");
                    break;
                case 5:
                    player.SetPerk("specialty_rof", true, true);
                    player.SetField("PerkBought", "weapon_attachment_rof");
                    break;
                case 6:
                    player.SetPerk("specialty_stalker", true, true);
                    player.SetField("PerkBought", "specialty_stalker_upgrade");
                    break;
                case 7:
                    player.SetField("autoRevive", true);
                    player.SetField("PerkBought", "waypoint_revive");
                    break;
                case 8:
                    player.SetPerk("specialty_scavenger", true, true);
                    player.SetField("PerkBought", "specialty_scavenger_upgrade");
                    break;
            }

            if (perk != 7) player.SetField("perk" + perk + "bought", true);
            else player.SetField("perk7bought", player.GetField<int>("perk7bought") + 1);

            HudElem perkIcon = NewClientHudElem(player);
            perkIcon.X = 0 * (perk - 1);
            perkIcon.Y = -54;
            perkIcon.AlignX = HudElem.XAlignments.Left;
            perkIcon.AlignY = HudElem.YAlignments.Bottom;
            perkIcon.VertAlign = HudElem.VertAlignments.Bottom_Adjustable;
            perkIcon.HorzAlign = HudElem.HorzAlignments.Left;
            perkIcon.SetShader(player.GetField<string>("perkBought"), 128, 128);
            perkIcon.Foreground = true;
            perkIcon.HideWhenInMenu = true;
            perkIcon.Alpha = 1;
            perkIcon.ScaleOverTime(1, 30, 30);

            player.PlayLocalSound("earn_perk");

            yield return Wait(1);

            perkIcon.Destroy();
            hud.updatePerksHud(player, false, true);
        }
        public static void giveAllPerks(Entity player)
        {
            bool[] ownedPerks = AIZ.getOwnedPerks(player);
            if (!ownedPerks.Contains(false)) return;//Owns all perks, give up on life...

            if (!ownedPerks[0])
            {
                player.MaxHealth = 250;
                player.Health = player.MaxHealth;
                player.SetField("PerkBought", "cardicon_juggernaut_1");
                player.SetField("perk1bought", true);
                hud.updatePerksHud(player, false, true);
            }
            if (!ownedPerks[1])
            {
                player.SetPerk("specialty_lightweight", true, true);
                //player.SetPerk("specialty_marathon", true, true);
                player.SetPerk("specialty_longersprint", true, true);
                player.SetField("PerkBought", "specialty_longersprint_upgrade");
                player.SetField("perk2bought", true);
                hud.updatePerksHud(player, false, true);
            }
            if (!ownedPerks[2])
            {
                player.SetPerk("specialty_fastreload", true, true);
                //player.SetPerk("specialty_quickswap", true, true);
                player.SetPerk("specialty_quickdraw", true, true);
                player.SetField("PerkBought", "specialty_fastreload_upgrade");
                player.SetField("perk3bought", true);
                hud.updatePerksHud(player, false, true);
            }
            if (!ownedPerks[3])
            {
                player.SetField("NewGunReady", true);
                player.SetField("PerkBought", "specialty_twoprimaries_upgrade");
                player.SetField("perk4bought", true);
                hud.updatePerksHud(player, false, true);
            }
            if (!ownedPerks[4])
            {
                player.SetPerk("specialty_rof", true, true);
                player.SetField("PerkBought", "weapon_attachment_rof");
                player.SetField("perk5bought", true);
                hud.updatePerksHud(player, false, true);
            }
            if (!ownedPerks[5])
            {
                player.SetPerk("specialty_stalker", true, true);
                player.SetField("PerkBought", "specialty_stalker_upgrade");
                player.SetField("perk6bought", true);
                hud.updatePerksHud(player, false, true);
            }
            if (!ownedPerks[6])
            {
                player.SetField("autoRevive", true);
                player.SetField("PerkBought", "waypoint_revive");
                player.SetField("perk7bought", player.GetField<int>("perk7bought") + 1);
                hud.updatePerksHud(player, false, true);
            }

            HudElem perkIcon = NewClientHudElem(player);
            perkIcon.X = 0;
            perkIcon.Y = 0;
            perkIcon.AlignX = HudElem.XAlignments.Center;
            perkIcon.AlignY = HudElem.YAlignments.Middle;
            perkIcon.VertAlign = HudElem.VertAlignments.Middle;
            perkIcon.HorzAlign = HudElem.HorzAlignments.Center_Adjustable;
            perkIcon.SetShader("specialty_perks_all", 128, 128);
            perkIcon.Foreground = true;
            perkIcon.HideWhenInMenu = true;
            perkIcon.Alpha = 1;
            perkIcon.ScaleOverTime(1, 512, 512);
            perkIcon.FadeOverTime(1);
            perkIcon.Alpha = 0;

            player.PlayLocalSound("earn_superbonus");
            AfterDelay(1000, () =>
                perkIcon.Destroy());
        }
        private static void giveRandomPerkToAll()
        {
            foreach (Entity player in Players)
            {
                if (!AIZ.isPlayer(player) || !player.IsAlive || !player.HasField("isDown")) continue;
                int perk = AIZ.rng.Next(7);
                perk++;
                //Log.Write(LogLevel.All, "Giving perk {0}", perk);
                StartAsync(giveRandomPerk(player, perk));
            }
        }
    }
}
