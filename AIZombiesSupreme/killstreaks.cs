using System.Collections;
using InfinityScript;
using static InfinityScript.GSCFunctions;
using System.Collections.Generic;

namespace AIZombiesSupreme
{
    public class killstreaks : BaseScript
    {
        public static bool nukeInbound = false;
        public static int empKills = 25;
        private static byte nukeTime = 10;
        public static int mapStreakKills = -1;
        public static string mapStreakWeapon = "killstreak_double_uav_mp";
        public static string mapStreakIcon = "white";
        public static string mapStreakName = AIZ.gameStrings[215];
        private static bool littlebirdOut = false;
        public static bool mapStreakOut = false;

        public static readonly string botAnim_idle = "pb_stand_alert";
        public static readonly string botAnim_idleRPG = "pb_stand_alert_RPG";
        public static readonly string botAnim_idleMG = "pb_stand_alert_mg";
        public static readonly string botAnim_idlePistol = "pb_stand_alert_pistol";
        public static readonly string botAnim_run = "pb_sprint_assault";
        public static readonly string botAnim_runSMG = "pb_sprint_smg";
        public static readonly string botAnim_runMG = "pb_sprint_lmg";
        public static readonly string botAnim_runPistol = "pb_pistol_run_fast";
        public static readonly string botAnim_runSniper = "pb_sprint_sniper";
        public static readonly string botAnim_runShotgun = "pb_sprint_shotgun";
        public static readonly string botAnim_runRPG = "pb_sprint_RPG";
        public static readonly string botAnim_shoot = "pt_stand_shoot";
        public static readonly string botAnim_shootRPG = "pt_stand_shoot_RPG";
        public static readonly string botAnim_shootMG = "pt_stand_shoot_mg";
        public static readonly string botAnim_shootPistol = "pt_stand_shoot_pistol";
        public static readonly string botAnim_reload = "pt_reload_stand_auto";
        public static readonly string botAnim_reloadRPG = "pt_reload_stand_RPG";
        public static readonly string botAnim_reloadPistol = "pt_reload_stand_pistol";
        public static readonly string botAnim_reloadMG = "pt_reload_stand_mg";
        public static readonly string botWeaponModel_subBot = "iw5_ump45_mp";
        public static readonly string botWeapon_subBot = "artillery_mp";//If these get changed you MUST update the weapon patches for the new weapon
        public static readonly string botWeaponModel_LMGBot = "iw5_sa80_mp";
        public static readonly string botWeapon_LMGBot = "ugv_turret_mp";

        private static Entity level = Entity.Level;

        public static int heliHeight = 1500;

        public static bool visionRestored = false;

        public static void checkKillstreak(Entity player)
        {
            int streak = player.Kills;
            if (streak == 50)
            {
                player.PlayLocalSound("US_1mc_achieve_predator");
                player.ShowHudSplash("predator_missile", 0, streak);
                player.GiveWeapon("killstreak_predator_missile_mp", 0, false);
                player.SetActionSlot(4, "weapon", "killstreak_predator_missile_mp");
                player.SetField("ownsPredator", true);
            }
            else if (streak == 100)
            {
                player.PlayLocalSound("US_1mc_achieve_sentrygun");
                player.ShowHudSplash("sentry", 0, streak);
                player.GiveWeapon("killstreak_sentry_mp", 0, false);
                player.SetActionSlot(5, "weapon", "killstreak_sentry_mp");
                player.SetField("ownsSentry", true);
            }
            else if (streak == 150)
            {
                player.PlayLocalSound("US_1mc_achieve_carepackage");
                player.ShowHudSplash("airdrop_assault", 0, streak);
                player.GiveWeapon("airdrop_marker_mp", 0, false);
                player.SetActionSlot(5, "weapon", "airdrop_marker_mp");
                player.SetField("ownsAirdrop", true);
            }
            else if (streak == empKills)
            {
                player.PlayLocalSound("US_1mc_achieve_emp");
                player.ShowHudSplash("emp", 0, streak);
                player.GiveWeapon("killstreak_emp_mp", 0, false);
                player.SetActionSlot(6, "weapon", "killstreak_emp_mp");
                player.SetField("ownsEMP", true);
            }
            else if (streak == 200)
            {
                player.PlayLocalSound("US_1mc_achieve_airstrike");
                player.ShowHudSplash("airstrike", 0, streak);
                player.GiveWeapon("strike_marker_mp", 0, false);
                player.SetActionSlot(5, "weapon", "strike_marker_mp");
                player.SetField("ownsAirstrike", true);
            }
            else if (streak == 250)
            {
                player.PlayLocalSound("US_1mc_achieve_turret");
                player.ShowHudSplash("gl_turret", 0, streak);
                player.GiveWeapon("killstreak_remote_turret_mp", 0, false);
                player.SetActionSlot(5, "weapon", "killstreak_remote_turret_mp");
                player.SetField("ownsSentryGL", true);
            }
            else if (streak == 300)
            {
                //player.PlayLocalSound("US_1mc_");
                player.ShowHudSplash("deployable_exp_ammo", 0, streak);
                player.GiveWeapon("deployable_vest_marker_mp", 0, false);
                player.SetActionSlot(6, "weapon", "deployable_vest_marker_mp");
                player.SetField("ownsExpAmmo", true);
            }
            else if (streak == 400)
            {
                player.PlayLocalSound("US_1mc_achieve_emergairdrop");
                player.ShowHudSplash("airdrop_mega", 0, streak);
                player.GiveWeapon("airdrop_trap_marker_mp", 0, false);
                player.SetActionSlot(7, "weapon", "airdrop_trap_marker_mp");
                player.SetField("ownsEmergencyAirdrop", true);
            }
            else if (streak == 500)
            {
                player.PlayLocalSound("US_1mc_achieve_moab");
                player.ShowHudSplash("nuke", 0, streak);
                player.GiveWeapon("killstreak_helicopter_mp", 0, false);
                player.SetActionSlot(7, "weapon", "killstreak_helicopter_mp");
                player.SetField("ownsNuke", true);
            }
            else if (streak == 750)
            {
                player.PlayLocalSound("US_1mc_achieve_dragonfly");
                player.ShowHudSplash("remote_uav", 0, streak);
                player.GiveWeapon("killstreak_uav_mp", 0, false);
                player.SetActionSlot(4, "weapon", "killstreak_uav_mp");
                player.SetField("ownsLittlebird", true);
            }
            else if (streak == 800)
            {
                player.PlayLocalSound("US_1mc_achieve_heli_sniper");
                player.ShowHudSplash("heli_sniper", 0, streak);
                player.GiveWeapon("killstreak_ims_mp", 0, false);
                player.SetActionSlot(4, "weapon", "killstreak_ims_mp");
                player.SetField("ownsHeliSniper", true);
            }
            /*
            else if (streak == 1000 && AIZ._mapname != "mp_carbon" && AIZ._mapname != "mp_cement" && !player.GetField<bool>("ownsBot"))//Disabled on carbon and cement for crappy optimization
            {
                player.SetField("ownsBot", true);
                player.IPrintLnBold(AIZ.gameStrings[216]);
                AfterDelay(1000, () => player.IPrintLnBold(AIZ.gameStrings[217]));
                spawnBotForPlayer(player);
            }
            */
            else if (streak == 350 && !player.GetField<bool>("ownsSubBot"))
            {
                player.IPrintLnBold(string.Format(AIZ.gameStrings[218], AIZ.gameStrings[226]));
                player.PlayLocalSound("mp_killstreak_juggernaut");
                player.GiveWeapon("killstreak_triple_uav_mp", 0, false);
                player.SetActionSlot(6, "weapon", "killstreak_triple_uav_mp");
                player.SetField("ownsSubBot", true);
            }
            else if (streak == 600 && !player.GetField<bool>("ownsLMGBot"))
            {
                player.IPrintLnBold(string.Format(AIZ.gameStrings[218], AIZ.gameStrings[227]));
                player.PlayLocalSound("mp_killstreak_juggernaut");
                player.GiveWeapon("killstreak_counter_uav_mp", 0, false);
                player.SetActionSlot(7, "weapon", "killstreak_counter_uav_mp");
                player.SetField("ownsLMGBot", true);
            }
            else if (streak == mapStreakKills)
            {
                player.PlayLocalSound("counter_uav_activate");
                player.GiveWeapon(mapStreakWeapon, 0, false);
                player.SetActionSlot(7, "weapon", mapStreakWeapon);
                if (mapStreakName == AIZ.gameStrings[333])
                    player.ShowHudSplash("mobile_mortar", 0, streak);
                else
                    player.IPrintLnBold(string.Format(AIZ.gameStrings[218], mapStreakName));
                player.SetField("ownsMapStreak", true);
            }
            shuffleStreaks(player);//update HUD
        }

        public static void giveKillstreak(Entity player, int streak)
        {
            int oldStreak = player.Kills;
            switch (streak)
            {
                case 0:
                    bonusDrops.spawnBonusDrop(bonusDrops.dropTypes.ammo, player.Origin);
                    return;
                case 1:
                    streak = 50;
                    break;
                case 2:
                    streak = 100;
                    break;
                case 3:
                    streak = empKills;
                    break;
                case 4:
                    streak = 300;
                    break;
                case 5:
                    streak = 500;
                    break;
                case 6:
                    streak = 750;
                    break;
                case 7:
                    streak = 800;
                    break;
                case 8:
                    streak = 200;
                    break;
                case 10:
                    streak = 150;
                    break;
                case 9:
                    streak = 250;
                    break;
                case 11:
                    streak = 400;
                    break;
                case 12:
                    bonusDrops.spawnBonusDrop(bonusDrops.dropTypes.doublePoints, player.Origin);
                    return;
                case 13:
                    bonusDrops.spawnBonusDrop(bonusDrops.dropTypes.instaKill, player.Origin);
                    return;
                case 14:
                    bonusDrops.spawnBonusDrop(bonusDrops.dropTypes.nuke, player.Origin);
                    return;
                case 15:
                    bonusDrops.spawnBonusDrop(bonusDrops.dropTypes.gun, player.Origin);
                    return;
                default:
                    return;
            }
            player.Kills = streak;
            checkKillstreak(player);
            player.Kills = oldStreak;
        }
        public static string getKillstreakCrateIcon(int streak)
        {
            switch (streak)
            {
                case 0:
                    return "waypoint_ammo_friendly";
                case 1:
                    return "specialty_predator_missile_crate";
                case 2:
                    return "specialty_sentry_gun_crate";
                case 3:
                    return "specialty_emp_crate";
                case 4:
                    return "specialty_deployable_vest_crate";
                case 5:
                    return "dpad_killstreak_nuke";
                case 6:
                    return "specialty_ac130_crate";
                case 7:
                    return "headicon_heli_extract_point";
                case 8:
                    return "specialty_precision_airstrike_crate";
                case 9:
                    return "specialty_sam_turret_crate";
                case 10:
                    return "specialty_carepackage";
                case 11:
                    return "specialty_airdrop_emergency";
            }

            return "white";
        }

        public static void executeKillstreak(Entity player, string newWeap)
        {
            if (!AIZ.isPlayer(player) || !player.HasField("isDown")) return;
            hud.updateAmmoHud(player, false);
            //killstreaks
            if (newWeap != player.CurrentWeapon) return;

            if (player.GetField<bool>("ownsPredator") && newWeap == "killstreak_predator_missile_mp")
            {
                StartAsync(launchMissile(player));
                AfterDelay(750, () =>
                    player.TakeWeapon("killstreak_predator_missile_mp"));
                player.SwitchToWeapon(player.GetField<string>("lastDroppableWeapon"));
            }
            else if (player.GetField<bool>("ownsSentry") && newWeap == "killstreak_sentry_mp")
            {
                spawnSentry(player, false);
                //Entity sentryModel = d_killstreaks.spawnSentry(player);
                //if (sentryModel != null)
                //d_killstreaks.sentryHoldWatcher(player, sentryModel, true);
                player.SwitchToWeapon(player.GetField<string>("lastDroppableWeapon"));
                //player.DisableWeapons();
            }
            else if (player.GetField<bool>("ownsSentryGL") && newWeap == "killstreak_remote_turret_mp")
            {
                spawnSentry(player, true);
                player.SwitchToWeapon(player.GetField<string>("lastDroppableWeapon"));
            }
            else if (player.GetField<bool>("ownsLittlebird") && newWeap == "killstreak_uav_mp")
            {
                if (!littlebirdOut)
                {
                    Entity littlebirdModel = spawnLittlebird(player);
                    if (littlebirdModel != null)
                        holdLittlebird(player, littlebirdModel);
                    else return;
                }
                else
                {
                    player.IPrintLnBold(AIZ.gameStrings[220]);
                    player.SwitchToWeapon(player.GetField<string>("lastDroppableWeapon"));
                    player.EnableWeapons();
                    return;
                }
                player.SwitchToWeapon(player.GetField<string>("lastDroppableWeapon"));
                player.DisableWeapons();
            }
            else if (player.GetField<bool>("ownsEMP") && newWeap == "killstreak_emp_mp")
            {
                StartAsync(visionRestore(player));
                player.SetField("ownsEMP", false);
                AfterDelay(1250, () =>
                        player.TakeWeapon("killstreak_emp_mp"));
                player.SwitchToWeapon(player.GetField<string>("lastDroppableWeapon"));
            }
            else if (player.GetField<bool>("ownsNuke") && newWeap == "killstreak_helicopter_mp")
            {
                bool success = nuke(player);

                if (!success)
                {
                    player.SwitchToWeapon(player.GetField<string>("lastDroppableWeapon"));
                    return;
                }

                player.SetField("ownsNuke", false);
                AfterDelay(1250, () =>
                        player.TakeWeapon("killstreak_helicopter_mp"));
                player.SwitchToWeapon(player.GetField<string>("lastDroppableWeapon"));
            }
            else if (player.GetField<bool>("ownsHeliSniper") && newWeap == "killstreak_ims_mp")
            {
                if (player.HasField("isCurrentlyTeleported"))
                {
                    player.IPrintLnBold(AIZ.gameStrings[221]);
                    player.SwitchToWeapon(player.GetField<string>("lastDroppableWeapon"));
                    return;
                }
                Vector3 origin = player.GetOrigin();
                Vector3 pos = origin + new Vector3(AIZ.rng.Next(-100, 100), AIZ.rng.Next(-100, 100), 0);
                if (canCallInHeliSniper(pos))
                {
                    Vector3 angles = player.GetPlayerAngles();
                    AIZ.teamSplash("used_heli_sniper", player);
                    callHeliSniper(player, pos);
                    player.SetField("ownsHeliSniper", false);
                    player.PlaySound("US_1mc_use_heli_sniper");
                    AfterDelay(1250, () =>
                        player.TakeWeapon("killstreak_ims_mp"));
                    player.SwitchToWeapon(player.GetField<string>("lastDroppableWeapon"));
                }
                else
                {
                    player.IPrintLnBold(AIZ.gameStrings[222]);
                    player.SwitchToWeapon(player.GetField<string>("lastDroppableWeapon"));
                }
            }
            else if (player.GetField<bool>("ownsMapStreak") && newWeap == mapStreakWeapon)
            {
                if (AIZ._mapname == "mp_bravo" ||
                    AIZ._mapname == "mp_carbon" ||
                    AIZ._mapname == "mp_roughneck")//Mapstreaks without an instant call-in
                    return;

                bool success = tryUseMapStreak(player);
                string lastWeapon = player.GetField<string>("lastDroppableWeapon");

                if (!success)
                {
                    player.IPrintLnBold(mapStreakName + AIZ.gameStrings[223]);
                    player.SwitchToWeapon(lastWeapon);
                    return;
                }

                player.SetField("ownsMapStreak", false);
                shuffleStreaks(player);
                AfterDelay(750, () =>
                        player.TakeWeapon(mapStreakWeapon));
                player.SwitchToWeapon(lastWeapon);
            }
            else if (player.GetField<bool>("ownsSubBot") && newWeap == "killstreak_triple_uav_mp")
            {
                if (player.HasField("bot"))
                {
                    player.IPrintLnBold(AIZ.gameStrings[219]);
                    player.SwitchToWeapon(player.GetField<string>("lastDroppableWeapon"));
                    return;
                }

                spawnBotForPlayer(player, botWeapon_subBot, 120);
                player.SwitchToWeapon(player.GetField<string>("lastDroppableWeapon"));
                AfterDelay(1250, () =>
                    player.TakeWeapon("killstreak_triple_uav_mp"));
            }
            else if (player.GetField<bool>("ownsLMGBot") && newWeap == "killstreak_counter_uav_mp")
            {
                if (player.HasField("bot"))
                {
                    player.IPrintLnBold(AIZ.gameStrings[219]);
                    player.SwitchToWeapon(player.GetField<string>("lastDroppableWeapon"));
                    return;
                }

                spawnBotForPlayer(player, botWeapon_LMGBot, 90);
                player.SwitchToWeapon(player.GetField<string>("lastDroppableWeapon"));
                AfterDelay(1250, () =>
                    player.TakeWeapon("killstreak_counter_uav_mp"));
            }
        }

        private static void shuffleStreaks(Entity player)
        {
            if (player.GetField<bool>("ownsLittlebird"))
                player.SetActionSlot(4, "weapon", "killstreak_uav_mp");
            else if (player.GetField<bool>("ownsPredator"))
                player.SetActionSlot(4, "weapon", "killstreak_predator_missile_mp");
            else if (player.GetField<bool>("ownsHeliSniper"))
               player.SetActionSlot(4, "weapon", "killstreak_ims_mp");

            if (player.GetField<bool>("ownsAirstrike"))
                player.SetActionSlot(5, "weapon", "strike_marker_mp");
            else if (player.GetField<bool>("ownsAirdrop"))
            {
                player.SetActionSlot(5, "weapon", "airdrop_marker_mp");
                //Fix for no ammo on chaining crate streaks
                player.SetWeaponAmmoClip("airdrop_marker_mp", 1);
            }
            else if (player.GetField<bool>("ownsSentry"))
                player.SetActionSlot(5, "weapon", "killstreak_sentry_mp");

            if (player.GetField<bool>("ownsSubBot"))
                player.SetActionSlot(6, "weapon", "killstreak_triple_uav_mp");
            else if (player.GetField<bool>("ownsExpAmmo"))
                player.SetActionSlot(6, "weapon", "deployable_vest_marker_mp");
            else if (player.GetField<bool>("ownsSentryGL"))
                player.SetActionSlot(6, "weapon", "killstreak_remote_turret_mp");
            else if (player.GetField<bool>("ownsEMP"))
                player.SetActionSlot(6, "weapon", "killstreak_emp_mp");

            if (player.GetField<bool>("ownsNuke"))
                player.SetActionSlot(7, "weapon", "killstreak_helicopter_mp");
            else if (player.GetField<bool>("ownsEmergencyAirdrop"))
            {
                player.SetActionSlot(7, "weapon", "airdrop_trap_marker_mp");
                //Fix for no ammo on chaining crate streaks
                player.SetWeaponAmmoClip("airdrop_trap_marker_mp", 1);
            }
            else if (player.GetField<bool>("ownsLMGBot"))
                player.SetActionSlot(7, "weapon", "killstreak_counter_uav_mp");
            if (player.GetField<bool>("ownsMapStreak"))
                player.SetActionSlot(7, "weapon", mapStreakWeapon);

            //Set the HUD for this
            string[] streaks = new string[4] { "", "", "", "" };
            if (player.GetField<bool>("ownsLittlebird"))
                streaks[0] = hud.createHudShaderString("specialty_ac130") + "[{+actionslot 4}]";
            else if (player.GetField<bool>("ownsPredator"))
                streaks[0] = hud.createHudShaderString("specialty_predator_missile") + "[{+actionslot 4}]";
            else if (player.GetField<bool>("ownsHeliSniper"))
                streaks[0] = hud.createHudShaderString("specialty_helicopter_flares") + "[{+actionslot 4}]";

            if (player.GetField<bool>("ownsAirstrike"))
                streaks[1] = hud.createHudShaderString("specialty_precision_airstrike") + "[{+actionslot 5}]";
            else if (player.GetField<bool>("ownsAirdrop"))
                streaks[1] = hud.createHudShaderString("specialty_carepackage_crate") + "[{+actionslot 5}]";
            else if (player.GetField<bool>("ownsSentry"))
                streaks[1] = hud.createHudShaderString("specialty_airdrop_sentry_minigun") + "[{+actionslot 5}]";

            if (player.GetField<bool>("ownsSubBot"))
                streaks[2] = hud.createHudShaderString("group_icon") + "[{+actionslot 6}]";
            else if (player.GetField<bool>("ownsExpAmmo"))
                streaks[2] = hud.createHudShaderString("specialty_deployable_vest") + "[{+actionslot 6}]";
            else if (player.GetField<bool>("ownsSentryGL"))
                streaks[2] = hud.createHudShaderString("specialty_remote_mg_turret") + "[{+actionslot 6}]";
            else if (player.GetField<bool>("ownsEMP"))
                streaks[2] = hud.createHudShaderString("specialty_emp") + "[{+actionslot 6}]";

            if (player.GetField<bool>("ownsNuke"))
                streaks[3] = hud.createHudShaderString("dpad_killstreak_nuke_static") + "[{+actionslot 7}]";
            else if (player.GetField<bool>("ownsEmergencyAirdrop"))
                streaks[3] = hud.createHudShaderString("specialty_airdrop_emergency") + "[{+actionslot 7}]";
            else if (player.GetField<bool>("ownsLMGBot"))
                streaks[3] = hud.createHudShaderString("group_icon") + "[{+actionslot 7}]";
            if (player.GetField<bool>("ownsMapStreak"))
                streaks[3] = hud.createHudShaderString(mapStreakIcon) + "[{+actionslot 7}]";

            if (!player.HasField("aizHud_created")) return;

            HudElem list = player.GetField<HudElem>("hud_killstreakList");
            string newText = streaks[0] + "\n\n" + streaks[1] + "\n\n" + streaks[2] + "\n\n" + streaks[3];
            if ((string)list.GetField("text") == newText) return;
            list.SetField("text", newText);
            hud._setText(list, newText);
        }

        public static void spawnSentry(Entity player, bool isGL)
        {
            if (player.GetField<bool>("isCarryingSentry")) return;

            string weapon = isGL ? "manned_gl_turret_mp" : "sentry_minigun_mp";
            string model = isGL ? "sentry_grenade_launcher" : "sentry_minigun";
            Entity turret = SpawnTurret("misc_turret", player.Origin, weapon);
            turret.Angles = new Vector3(0, player.GetPlayerAngles().Y, 0);
            turret.SetModel("sentry_minigun");
            turret.SetField("baseModel", model);
            //turret.Health = 1000;
            //turret.SetCanDamage(true);
            turret.MakeTurretInOperable();
            turret.SetRightArc(80);
            turret.SetLeftArc(80);
            turret.SetBottomArc(50);
            turret.MakeUnUsable();
            turret.SetDefaultDropPitch(-89.0f);
            if (isGL) turret.SetConvergenceTime(1);
            turret.SetTurretModeChangeWait(true);
            turret.SetMode("sentry_offline");
            turret.SetField("owner", player);
            turret.SetTurretTeam("allies");
            turret.SetSentryOwner(player);
            turret.SetField("isSentry", true);
            turret.SetField("isSentryGL", isGL);
            if (isGL) turret.SetField("readyToFire", true);

            turret.SetTurretMinimapVisible(true);
            int objID = mapEdit.getNextRealObjID();
            mapEdit._realObjIDList[objID] = true;
            turret.SetField("realObjID", objID);

            turret.SetField("isBeingCarried", false);
            turret.SetField("canBePlaced", true);
            turret.SetField("timeLeft", 90);
            if (isGL) turret.SetField("timeLeft", 120);
            turret.SetField("target", turret);
            Entity trigger = Spawn("script_origin", turret.Origin);
            turret.SetField("trigger", trigger);
            trigger.LinkTo(turret, "tag_origin");
            trigger.SetField("turret", turret);
            mapEdit.makeUsable(trigger, "sentryPickup", 96);

            OnInterval(1000, () => sentry_timer(turret));
            OnInterval(50, () => sentry_targeting(turret));
            pickupSentry(player, turret, true);
        }

        public static void pickupSentry(Entity player, Entity sentry, bool canCancel)
        {
            if (sentry.GetField<bool>("isBeingCarried") || player.GetField<bool>("isCarryingSentry") || player != sentry.GetField<Entity>("owner")) return;

            sentry.ClearTargetEntity();
            sentry.SetMode("sentry_offline");
            sentry.SetField("isBeingCarried", true);
            player.SetField("isCarryingSentry", true);
            player.SetField("isCarryingSentry_alt", true);//Used to fix a bug allowing players to 'faux-cancel' a placement causing a persistant sentry being held
            sentry.SetField("canBePlaced", true);
            player.DisableWeapons();
            //sentry.SetCanDamage(false);
            sentry.SetSentryCarrier(player);
            sentry.SetModel(sentry.GetField<string>("baseModel") + "_obj");

            OnInterval(50, () => sentryHoldWatcher(player, sentry, canCancel));
        }
        private static bool sentryHoldWatcher(Entity player, Entity sentry, bool canCancel)
        {
            if (AIZ.gameEnded)
            {
                sentry.SetSentryCarrier(null);
                sentry.Delete();
                return false;
            }
            if (!player.IsAlive || !AIZ.isPlayer(player)) return false;
            if (sentry.GetField<bool>("canBePlaced") && player.GetField<bool>("isCarryingSentry") && player.AttackButtonPressed() && player.IsOnGround())
            {
                player.EnableWeapons();
                if (canCancel && sentry.GetField<bool>("isSentryGL"))
                {
                    AIZ.teamSplash("used_gl_turret", player);
                    player.TakeWeapon("killstreak_remote_turret_mp");
                    player.SwitchToWeapon(player.GetField<string>("lastDroppableWeapon"));
                    player.SetField("ownsSentryGL", false);
                    shuffleStreaks(player);
                }
                else if (canCancel)
                {
                    AIZ.teamSplash("used_sentry", player);
                    player.TakeWeapon("killstreak_sentry_mp");
                    player.SwitchToWeapon(player.GetField<string>("lastDroppableWeapon"));
                    player.SetField("ownsSentry", false);
                    shuffleStreaks(player);
                }
                player.SetField("isCarryingSentry", false);
                player.ClearField("isCarryingSentry_alt");
                //sentry.SetField("carriedBy");
                sentry.SetSentryCarrier(null);
                Vector3 angleToForward = AnglesToForward(new Vector3(0, player.GetPlayerAngles().Y, 0));
                sentry.Origin = player.Origin + angleToForward * 50;
                sentry.Angles = new Vector3(0, player.GetPlayerAngles().Y, 0);
                sentry.SetField("isBeingCarried", false);
                sentry.SetModel(sentry.GetField<string>("baseModel"));
                sentry.PlaySound("sentry_gun_plant");
                //turret.SetCanDamage(true);
                sentry.SetMode("sentry");
                //AfterDelay(500, () => StartAsync(handlePickup(sentry)));
                AfterDelay(500, () => handlePickupInterval(sentry));
                return false;
            }
            else return true;
        }

        private static bool sentry_timer(Entity sentry)
        {
            if (AIZ.gameEnded) return false;
            if (sentry.GetField<bool>("isBeingCarried") && sentry.GetField<Entity>("owner").IsAlive) return true;
            sentry.SetField("timeLeft", sentry.GetField<int>("timeLeft") - 1);
            if (sentry.GetField<int>("timeLeft") > 0 && sentry.GetField<Entity>("owner").IsAlive) return true;
            else
            {
                StartAsync(destroySentry(sentry));
                return false;
            }
        }
        private static bool sentry_targeting(Entity sentry)
        {
            if (AIZ.gameEnded) return false;
            if (sentry.GetField<int>("timeLeft") > 0)
            {
                if (!sentry.GetField<bool>("isBeingCarried"))
                {
                    sentry.SetField("target", sentry);
                    foreach (Entity b in botUtil.botsInPlay)
                    {
                        if (!b.GetField<bool>("isAlive")) continue;
                        Entity botHitbox = b.GetField<Entity>("hitbox");

                        if (sentry.GetField<bool>("isSentryGL"))
                            if (botHitbox.Origin.DistanceTo(sentry.Origin) > 1500) continue;

                        bool tracePass = SightTracePassed(sentry.GetTagOrigin("tag_flash"), botHitbox.Origin, false, botHitbox);
                        if (!tracePass)
                            continue;

                        float yaw = VectorToAngles(botHitbox.Origin - sentry.Origin).Y;
                        float clamp = yaw - sentry.Angles.Y;
                        //Log.Write(LogLevel.Debug, "Sentry: {0}, Bot: {1}, Angle: {2}", sentryYaw, yaw, clamp);
                        if (clamp < 290 && clamp > 70)
                            continue;
                        sentry.SetField("target", botHitbox);
                        break;
                    }

                    if (sentry.GetField<Entity>("target") != sentry)
                    {
                        sentry.SetTargetEntity(sentry.GetField<Entity>("target"));
                        if (sentry.GetField<bool>("isSentryGL") && sentry.GetField<bool>("readyToFire")) StartAsync(sentryGL_fireTurret(sentry));
                        else if (!sentry.GetField<bool>("isSentryGL")) sentry.ShootTurret();
                    }
                    else
                        sentry.ClearTargetEntity();

                    return true;
                }
                else return true;
            }
            else return false;
        }
        private static IEnumerator sentryGL_fireTurret(Entity sentry)
        {
            sentry.SetField("readyToFire", false);

            yield return Wait(1);
            sentry.ShootTurret();

            yield return Wait(3);

            sentry.SetField("readyToFire", true);
        }
        /*
        private static IEnumerator handlePickup(Entity turret)
        {
            if (!Utilities.isEntDefined(turret)) yield break;

            Entity owner = turret.GetField<Entity>("owner");
            yield return owner.WaitTill("use_button_pressed:" + owner.EntRef.ToString());
            Entity trigger = turret.GetField<Entity>("trigger");

            if (!Utilities.isEntDefined(trigger)) yield break;

            bool isTouching = owner.IsTouching(trigger);
            bool isOnGround = owner.IsOnGround();

            if (owner.IsAlive && isTouching && isOnGround)
            {
                //bool useButtonPressed = owner.UseButtonPressed();

                if (!owner.GetField<bool>("isCarryingSentry") && !turret.GetField<bool>("isBeingCarried"))
                    sentryHoldWatcher(owner, turret, false);
                yield break;
            }
            else { StartAsync(handlePickup(turret)); yield break; }
        }
        */
        private static void handlePickupInterval(Entity turret)
        {
            OnInterval(100, () => watchForSentryPickup(turret, turret.GetField<Entity>("owner"), turret.GetField<Entity>("trigger")));
        }
        private static bool watchForSentryPickup(Entity turret, Entity owner, Entity trigger)
        {
            if (AIZ.gameEnded) return false;
            if (owner.IsAlive && owner.IsTouching(trigger) && owner.IsOnGround() && owner.UseButtonPressed())
            {
                if (!owner.GetField<bool>("isCarryingSentry") && !turret.GetField<bool>("isBeingCarried"))
                    pickupSentry(owner, turret, false);
                return false;
            }
            if (owner.IsAlive && turret.Health > 0) return true;
            else return false;
        }

        public static IEnumerator destroySentry(Entity sentry)
        {
            if (!sentry.HasField("isSentry"))
            {
                Utilities.PrintToConsole("Tried to destroy a sentry that was not defined!");
                yield break;
            }

            sentry.SetField("timeLeft", 0);

            Entity trigger = sentry.GetField<Entity>("trigger");
            //if (Utilities.isEntDefined(trigger))
            mapEdit.removeUsable(trigger);

            //Entity fx = sentry.GetField<Entity>("flashFx");
            //if (Utilities.isEntDefined(fx))
                //fx.Delete();

            sentry.ClearTargetEntity();
            sentry.SetCanDamage(false);
            sentry.SetDefaultDropPitch(40);
            sentry.SetMode("sentry_offline");
            sentry.Health = 0;
            sentry.SetModel(sentry.GetField<string>("baseModel") + "_destroyed");
            sentry.PlaySound("sentry_explode");

            Entity owner = sentry.GetField<Entity>("owner");
            if (owner.IsAlive)
            {
                if (sentry.GetField<bool>("isSentryGL")) owner.PlayLocalSound("US_1mc_turret_destroyed");
                else owner.PlayLocalSound("US_1mc_sentry_gone");
            }

            PlayFXOnTag(AIZ.fx_sentryExplode, sentry, "tag_aim");
            yield return Wait(1.5f);
            sentry.PlaySound("sentry_explode_smoke");
            PlayFXOnTag(AIZ.fx_sentrySmoke, sentry, "tag_aim");

            yield return Wait(5.5f);

            PlayFX(AIZ.fx_sentryDeath, sentry.Origin + new Vector3(0, 0, 20));
            int objID = sentry.GetField<int>("realObjID");
            mapEdit._realObjIDList[objID] = false;
            sentry.ClearField("realObjID");
            sentry.ClearField("owner");
            sentry.ClearField("isSentry");
            sentry.ClearField("isSentryGL");
            sentry.Delete();
        }

        public static IEnumerator launchMissile(Entity owner)
        {
            hud.scoreMessage(owner, AIZ.gameStrings[302] + "!");
            owner.SetField("ownsPredator", false);
            //foreach (Entity player in Players)
            //if (AIZ.isPlayer(player)) 
            owner.PlaySound("US_1mc_use_predator");
            shuffleStreaks(owner);
            //_main.Main.teamSplash("", owner);

            Entity missile;
            int randomInt = AIZ.rng.Next(botUtil.botsInPlay.Count);
            int randomIntSpawns = AIZ.rng.Next(botUtil.botSpawns.Count);

            if (botUtil.botsInPlay.Count != 0) missile = Spawn("script_model", botUtil.botsInPlay[randomInt].Origin + new Vector3(0, 0, 10000));
            else missile = Spawn("script_model", botUtil.botSpawns[randomIntSpawns] + new Vector3(0, 0, 10000));

            if (missile == null) yield break;

            missile.SetModel("projectile_cbu97_clusterbomb");
            missile.Angles = new Vector3(90, 0, 0);
            missile.MoveTo(missile.Origin - new Vector3(0, 0, 9950), 4);
            missile.PlayLoopSound("move_remotemissile_proj_flame");
            yield return Wait(4.05f);

            PhysicsExplosionSphere(missile.Origin, 400, 200, 7);
            Entity fx = SpawnFX(AIZ.fx_explode, missile.Origin);
            TriggerFX(fx);
            //fx.Notify("death");
            //missile.StopLoopSound();
            missile.PlaySound("exp_suitcase_bomb_main");
            missile.Hide();
            RadiusDamage(missile.Origin, 1500, 100000, 100, owner);
            yield return Wait(5);

            missile.Delete();
            fx.Delete();
        }

        public static IEnumerator visionRestore(Entity owner)
        {
            AIZ.teamSplash("used_emp", owner);
            owner.SetField("ownsEMP", false);
            shuffleStreaks(owner);
            if (AIZ.isHellMap)
            {
                visionRestored = true;
                VisionSetNaked(AIZ.vision, 1);
                VisionSetPain("near_death_mp");

                foreach (Entity player in Players)
                {
                    if (!player.IsAlive) continue;
                    //player.Call("playlocalsound", "US_1mc_use_emp");
                    player.PlayLocalSound("emp_activate");

                    player.VisionSetNakedForPlayer("end_game2", .5f);

                    yield return Wait(.8f);
                    if (!player.GetField<bool>("isDown") && !roundSystem.isBossWave) player.VisionSetNakedForPlayer(AIZ.vision);
                    else if (!player.GetField<bool>("isDown") && roundSystem.isBossWave) player.VisionSetNakedForPlayer(AIZ.bossVision);
                    else player.VisionSetNakedForPlayer("cheat_bw");
                }
            }
            else//Alt funtion for power maps which temporarily activate all power based items for a minute. Stackable
            {
                AIZ.powerActivated = true;
                hud.EMPTime += 60;
                if (!AIZ.tempPowerActivated)
                    hud.tempPowerHud();
                foreach (Entity players in Players)
                {
                    if (!players.IsAlive) continue;
                    players.PlayLocalSound("predator_drone_static");
                    players.SetEMPJammed(true);
                    AfterDelay(1000, () =>
                        players.SetEMPJammed(false));
                }
                AIZ.tempPowerActivated = true;
            }
        }

        public static void airStrike(Entity marker, Vector3 pos)
        {
            Entity owner;
            if (marker.HasField("owner")) owner = marker.GetField<Entity>("owner");
            else
            {
                AIZ.printToConsole("A marker doesn't have an owner setup!");
                return;
            }
            marker.Delete();
            owner.SetField("ownsAirstrike", false);
            shuffleStreaks(owner);
            owner.PlaySound("US_1mc_use_airstrike");
            StartAsync(doAirstrike(pos, owner));
        }

        private static IEnumerator doAirstrike(Vector3 pos, Entity owner)
        {
            Vector3 strikeOrigin = pos + new Vector3(0, 0, 10000);
            yield return Wait(1);
            MagicBullet("ac130_105mm_mp", strikeOrigin, pos + new Vector3(AIZ.rng.Next(100), AIZ.rng.Next(100), 0), owner);
            yield return Wait(1);
            MagicBullet("ac130_105mm_mp", strikeOrigin, pos + new Vector3(AIZ.rng.Next(100), AIZ.rng.Next(100), 0), owner);
            yield return Wait(1);
            MagicBullet("ac130_105mm_mp", strikeOrigin, pos + new Vector3(AIZ.rng.Next(100), AIZ.rng.Next(100), 0), owner);
            yield return Wait(1);
            MagicBullet("ac130_105mm_mp", strikeOrigin, pos + new Vector3(AIZ.rng.Next(100), AIZ.rng.Next(100), 0), owner);
            yield return Wait(1);
            MagicBullet("ac130_105mm_mp", strikeOrigin, pos + new Vector3(AIZ.rng.Next(100), AIZ.rng.Next(100), 0), owner);
        }

        public static bool nuke(Entity player)
        {
            if (AIZ.gameEnded) return false;

            if (nukeInbound)
            {
                player.IPrintLnBold(AIZ.gameStrings[224]);
                return false;
            }
            nukeInbound = true;
            AfterDelay(8000, playNukeWhoosh);
            AfterDelay(11000, () => { if (!AIZ.gameEnded) botUtil.nukeDetonation(player, true); });
            //AfterDelay(10000, () => StartAsync(nukeSloMo()));

            AIZ.teamSplash("used_nuke", player);
            player.SetField("ownsNuke", false);
            shuffleStreaks(player);

            HudElem nukeTimer = NewTeamHudElem("allies");
            nukeTimer.X = 0;
            nukeTimer.Y = 0;
            nukeTimer.AlignX = HudElem.XAlignments.Center;
            nukeTimer.AlignY = HudElem.YAlignments.Middle;
            nukeTimer.HorzAlign = HudElem.HorzAlignments.Center;
            nukeTimer.VertAlign = HudElem.VertAlignments.Middle;
            //nukeTimer.SetPoint("CENTER", "CENTER", 0, -75);
            nukeTimer.Foreground = true;
            nukeTimer.Alpha = 1;
            nukeTimer.HideWhenInMenu = true;
            nukeTimer.Font = HudElem.Fonts.HudBig;
            nukeTimer.FontScale = 1;
            hud._setText(nukeTimer, string.Format(AIZ.gameStrings[225], 10));
            nukeTimer.Color = new Vector3(.7f, 0, 0);
            nukeTimer.GlowColor = new Vector3(0, 0, .5f);
            nukeTimer.GlowAlpha = .4f;

            nukeTime = 10;
            OnInterval(1000, () => nukeCountdown(player, nukeTimer));

            foreach (Entity players in Players)
            {
                if (!AIZ.isPlayer(players)) continue;
                StartAsync(nuke_restorePlayerVision(players));
            }

            return true;
        }
        private static void playNukeWhoosh()
        {
            foreach (Entity player in Players)
            {
                player.PlayLocalSound("slomo_whoosh");
            }
        }
        private static IEnumerator nukeSloMo()
        {
            SetSlowMotion(1f, .35f, .5f);

            yield return Wait(.5f);

            SetDvar("fixedtime", 1.1f);
            foreach (Entity player in Players)
            {
                player.SetClientDvar("fixedtime", 1.1f);
            }

            yield return Wait(5);//Realtime wait, NOT slomo time

            SetSlowMotion(.35f, 1, .1f);
            nukeSloMo_ResetFixedTime();
        }
        private static void nukeSloMo_ResetFixedTime()
        {
            foreach (Entity player in Players)
            {
                player.SetClientDvar("fixedtime", 0);
            }
            SetDvar("fixedtime", 0);
        }
        private static bool nukeCountdown(Entity player, HudElem nukeTimer)
        {
            if (AIZ.gameEnded)
            {
                nukeTimer.Destroy();
                return false;
            }
            StartAsync(nuke_fontPulse(nukeTimer));
            nukeTime--;
            PlaySoundAtPos(Vector3.Zero, "mp_killstreak_nuclearstrike");
            if (nukeTime > 0) return true;
            else { AfterDelay(350, () => nukeTimer.Destroy()); return false; }
        }
        private static IEnumerator nuke_fontPulse(HudElem nukeTimer)
        {
            nukeTimer.ChangeFontScaleOverTime(.2f);
            nukeTimer.FontScale = 1.25f;
            yield return Wait(.2f);

            if (nukeTime > 0) hud._setText(nukeTimer, string.Format(AIZ.gameStrings[225], nukeTime));
            nukeTimer.ChangeFontScaleOverTime(.2f);
            nukeTimer.FontScale = 1;
        }
        private static IEnumerator nuke_restorePlayerVision(Entity player)
        {
            yield return Wait(9);

            if (AIZ.gameEnded) yield break;
            player.VisionSetNakedForPlayer("mpnuke", 5);
            player.PlayLocalSound("nuke_explosion");
            player.PlayLocalSound("nuke_wave");
            yield return Wait(5);

            if (player.IsAlive && player.GetField<bool>("isDown")) player.VisionSetNakedForPlayer("cheat_bw", 1);
            else player.VisionSetNakedForPlayer(AIZ.vision, 5);
        }

        public static Entity spawnLittlebird(Entity player)
        {
            if (player.GetField<bool>("isCarryingSentry")) return null;

            Vector3 angleToForward = AnglesToForward(new Vector3(0, player.GetPlayerAngles().Y, 0));
            //Entity turret = Call<Entity>("spawn", "script_model", player.Origin + angleToForward * 50);
            Entity turret = SpawnTurret("misc_turret", player.Origin + angleToForward * 50, "sentry_minigun_mp");
            turret.Angles = new Vector3(0, player.GetPlayerAngles().Y, 0);
            turret.SetModel("mp_trophy_system_folded");
            turret.MakeTurretInOperable();
            turret.MakeUsable();
            turret.EnablePlayerUse(player);
            turret.SetDefaultDropPitch(-89.0f);
            turret.SetTurretModeChangeWait(true);
            turret.SetMode("sentry_offline");
            turret.SetField("owner", player);
            turret.SetSentryOwner(player);
            turret.SetTurretTeam("allies");
            //turret.SetTurretMinimapVisible(true);
            turret.HideAllParts();
            turret.SetField("isBeingCarried", true);
            turret.SetField("canBePlaced", true);

            Entity visual = Spawn("script_model", turret.Origin);
            visual.SetModel("test_vehicle_little_bird_toy_placement");
            visual.EnableLinkTo();
            visual.Angles = turret.Angles;
            visual.LinkTo(turret);
            turret.SetField("visual", visual);
            return turret;
        }
        public static void holdLittlebird(Entity player, Entity bird)
        {
            bird.SetField("isBeingCarried", true);
            player.SetField("isCarryingSentry", true);
            bird.SetField("canBePlaced", true);
            player.DisableWeapons();
            bird.SetSentryCarrier(player);
            bird.SetCanDamage(false);
            OnInterval(100, () => littlebirdHoldWatcher(bird, player));
        }
        private static bool littlebirdHoldWatcher(Entity bird, Entity player)
        {
            if (AIZ.gameEnded)
            {
                bird.SetSentryCarrier(null);
                bird.GetField<Entity>("visual").Delete();
                bird.Delete();
                return false;
            }

            Entity birdVisual = bird.GetField<Entity>("visual");
            Vector3 anglesToForward = AnglesToForward(new Vector3(0, player.GetPlayerAngles().Y, 0));
            Vector3 traceOrigin = player.Origin + (anglesToForward * 75);
            bool trace = SightTracePassed(traceOrigin + new Vector3(0, 0, 25), traceOrigin + new Vector3(0, 0, 500), false, birdVisual);

            if (trace && birdVisual.Model == "test_vehicle_little_bird_toy_placement_failed")
            {
                //Utilities.PrintToConsole("Trace passed and setting model");
                birdVisual.SetModel("test_vehicle_little_bird_toy_placement");
                bird.SetField("canBePlaced", true);
            }
            else if (!trace && birdVisual.Model == "test_vehicle_little_bird_toy_placement")
            {
                //Utilities.PrintToConsole("Trace failed and setting model");
                birdVisual.SetModel("test_vehicle_little_bird_toy_placement_failed");
                bird.SetField("canBePlaced", false);
            }

            if (player.IsAlive && player.AttackButtonPressed() && bird.GetField<bool>("canBePlaced") && player.GetField<bool>("isCarryingSentry"))
            {
                player.EnableWeapons();
                AfterDelay(750, () => player.TakeWeapon("killstreak_uav_mp"));
                string lastWeapon = player.GetField<string>("lastDroppableWeapon");
                player.SwitchToWeapon(lastWeapon);
                player.SetField("ownsLittlebird", false);
                //foreach (Entity players in Players)
                //if (AIZ.isPlayer(players)) 
                player.PlaySound("US_1mc_use_dragonfly");
                shuffleStreaks(player);
                player.SetField("isCarryingSentry", false);
                bird.SetSentryCarrier(null);
                bird.SetField("isBeingCarried", false);
                bird.PlaySound("sentry_gun_plant");
                AIZ.teamSplash("used_remote_uav", player);
                float playerAngleY = player.GetPlayerAngles().Y;
                Vector3 angleToForward = AnglesToForward(new Vector3(0, playerAngleY, 0));
                Vector3 sentryAngles = new Vector3(0, playerAngleY, 0);
                Vector3 origin = player.Origin + angleToForward * 50;
                spawnRemoteUAV(player, origin, sentryAngles);
                bird.Delete();
                birdVisual.Delete();
                return false;
            }
            else return true;
        }

        private static void spawnRemoteUAV(Entity owner, Vector3 pos, Vector3 angles)
        {
            Entity uav = SpawnHelicopter(owner, pos + new Vector3(0, 0, 50), angles, "remote_uav_mp", "vehicle_remote_uav");
            littlebirdOut = true;

            int objID = mapEdit.getNextRealObjID();
            mapEdit._realObjIDList[objID] = true;
            uav.SetField("realObjID", objID);

            uav.Angles = angles;
            uav.SetField("owner", owner);
            owner.SetField("ownedLittlebird", uav);
            uav.SetField("isAlive", true);
            uav.SetField("timeLeft", 60);
            uav.SetField("targetDest", new Vector3(pos.X, pos.Y, GetGroundPosition(pos, 2).Z + 500));
            uav.SetField("target", uav);
            uav.SetVehicleTeam("allies");
            uav.SetVehWeapon("remote_uav_weapon_mp");
            uav.SetSpeed(50, 15, 15);
            uav.SetTurningAbility(.5f);
            uav.SetYawSpeed(100, 50, 25);
            uav.SetHoverParams(10, 50, 25);
            uav.SetVehicleLookAtText(owner.Name, "");
            uav.SetVehGoalPos(uav.GetField<Vector3>("targetDest"), true);
            uav.ClearTargetYaw();
            uav.ClearGoalYaw();

            HudElem hint = HudElem.CreateFontString(owner, HudElem.Fonts.HudSmall, 1);
            hint.SetPoint("CENTER", "CENTER", 0, 140);
            hud._setText(hint, AIZ.gameStrings[228]);
            //owner.SetField("hud_lbHint", hint);
            AfterDelay(4000, () =>
                hint.Destroy());

            OnInterval(1000, () => dragonfly_timer(uav));
            OnInterval(50, () => dragonfly_targeting(uav));
        }
        private static bool dragonfly_timer(Entity uav)
        {
            if (!uav.GetField<bool>("isAlive")) return false;
            uav.SetField("timeLeft", uav.GetField<int>("timeLeft") - 1);
            //Log.Write(LogLevel.All, "Time is {0}", uav.GetField<int>("timeLeft"));
            if (uav.GetField<int>("timeLeft") > 0 && uav.GetField<Entity>("owner").IsAlive) return true;
            else
            {
                StartAsync(destroyLittlebird(uav));
                return false;
            }
        }
        private static bool dragonfly_targeting(Entity uav)
        {
            if (AIZ.gameEnded) return false;

            if (uav.GetField<bool>("isAlive"))
            {
                Vector3 uavTargetDest = uav.GetField<Vector3>("targetDest");
                if (uav.Origin.DistanceTo(uavTargetDest) > 50)
                {
                    uav.SetVehGoalPos(uavTargetDest, true);
                    Vector3 targetYaw = VectorToAngles(uav.Origin - uavTargetDest);
                    uav.SetGoalYaw(targetYaw.Y);
                }
                uav.SetField("target", uav);
                foreach (Entity b in botUtil.botsInPlay)
                {
                    if (!b.GetField<bool>("isAlive")) continue;
                    Entity botHitbox = b.GetField<Entity>("hitbox");
                    Vector3 flashTag = uav.GetTagOrigin("tag_flash");
                    Vector3 botOrigin = botHitbox.Origin;
                    bool tracePass = SightTracePassed(flashTag, botOrigin, false, botHitbox);
                    if (!tracePass)
                        continue;

                    uav.SetField("target", botHitbox);
                    break;
                }
                if (uav.GetField<Entity>("target") != uav)
                {
                    Vector3 forward = VectorToAngles(uav.GetField<Entity>("target").Origin - uav.Origin);
                    uav.SetGoalYaw(forward.Y);
                    //Vector3 flashTag = uav.GetTagOrigin("tag_flash");
                    //Entity owner = uav.GetField<Entity>("owner");
                    //MagicBullet("cobra_20mm_mp", flashTag, target.Origin, owner);
                    uav.SetTurretTargetEnt(uav.GetField<Entity>("target"));
                    uav.FireWeapon("tag_flash", uav.GetField<Entity>("target"));
                    //playUAVFireSounds(uav);
                }
                return true;
            }
            else return false;
        }

        public static void dragonfly_rerouteWatcher(Entity player)
        {
            if ((player.HasField("ownedLittlebird") && !player.GetField<Entity>("ownedLittlebird").GetField<bool>("isAlive"))) return;
            if (player.HasField("isCarryingSentry_alt")) return;//Used to fix a bug allowing players to 'faux-cancel' a placement causing a persistant sentry being held

            else if (player.GetField<bool>("isCarryingSentry"))
            {
                player.SetField("isCarryingSentry", false);
                player.EnableWeapons();
                return;
            }
            player.SetField("canReroute", true);
            player.SetField("isCarryingSentry", true);//Setting this for a check and to keep from picking up sentrys
            player.DisableWeapons();

            Entity marker = Spawn("script_model", player.Origin);
            marker.Angles = new Vector3(-90, 0, 0);
            marker.SetModel("tag_origin");
            marker.SetField("FXPlaying", false);

            AfterDelay(150, () => dragonfly_initRerouteMarker(player, marker));
        }
        private static void dragonfly_initRerouteMarker(Entity player, Entity marker)
        {
            PlayFXOnTagForClients(AIZ.fx_uavRoute, marker, "tag_origin", player);
            marker.SetField("passFXPlaying", true);
            marker.SetField("failFXPlaying", false);

            OnInterval(50, () => dragonfly_moveRerouteMarker(player, marker));
        }
        private static bool dragonfly_moveRerouteMarker(Entity player, Entity marker)
        {
            if (AIZ.gameEnded)
            {
                marker.Delete();
                return false;
            }

            if (!player.HasField("ownedLittlebird"))
            {
                player.EnableWeapons();
                player.SetField("isCarryingSentry", false);
                marker.Delete();
                return false;
            }
            float markerGround = GetGroundPosition(marker.Origin, 1).Z;
            if (markerGround == marker.Origin.Z) markerGround = player.Origin.Z;
            Vector3 playerOrigin = player.Origin;
            Vector3 angleToForward = AnglesToForward(player.GetPlayerAngles());
            //Vector3 position = PhysicsTrace(playerOrigin, playerOrigin + (angleToForward * 250));
            Vector3 position = playerOrigin + (angleToForward * 250);
            marker.MoveTo(new Vector3(position.X, position.Y, markerGround), .1f);

            if (marker.Origin.Z > (playerOrigin.Z + 500) || marker.Origin.Z < (playerOrigin.Z - 500))
                marker.Origin = position;

            //bool tracePass = SightTracePassed(marker.Origin + new Vector3(0, 0, 500), marker.Origin + new Vector3(0, 0, 10), false, player);
            Entity littlebird = player.GetField<Entity>("ownedLittlebird");
            bool tracePass = SightTracePassed(littlebird.Origin, marker.Origin + new Vector3(0, 0, 500), false, littlebird);
            if (tracePass) player.SetField("canReroute", true);
            else player.SetField("canReroute", false);

            if (player.GetField<bool>("canReroute") && !marker.GetField<bool>("passFXPlaying"))
            {
                //Utilities.PrintToConsole("Can reroute and starting pass FX");
                StopFXOnTag(AIZ.fx_uavRouteFail, marker, "tag_origin");
                marker.SetField("failFXPlaying", false);
                marker.SetField("passFXPlaying", true);
                AfterDelay(50, () =>
                    PlayFXOnTagForClients(AIZ.fx_uavRoute, marker, "tag_origin", player));
            }
            else if (!player.GetField<bool>("canReroute") && !marker.GetField<bool>("failFXPlaying"))
            {
                //Utilities.PrintToConsole("Cant reroute and starting fail FX");
                StopFXOnTag(AIZ.fx_uavRoute, marker, "tag_origin");
                marker.SetField("passFXPlaying", false);
                marker.SetField("failFXPlaying", true);
                AfterDelay(50, () =>
                    PlayFXOnTagForClients(AIZ.fx_uavRouteFail, marker, "tag_origin", player));
            }

            if (player.AttackButtonPressed() && player.HasField("ownedLittlebird") && player.GetField<bool>("isCarryingSentry") && player.GetField<bool>("canReroute"))
            {
                if (littlebird.GetField<bool>("isAlive"))
                {
                    littlebird.SetField("targetDest", marker.Origin + new Vector3(0, 0, 500));
                    player.PlayLocalSound("recondrone_tag");
                }
                player.EnableWeapons();
                player.SetField("isCarryingSentry", false);
                marker.Delete();
                return false;
            }
            else if (player.HasField("ownedLittlebird") && player.GetField<bool>("isCarryingSentry")) return true;
            else
            {
                player.EnableWeapons();
                player.SetField("isCarryingSentry", false);
                marker.Delete();
                return false;
            }
        }

        private static IEnumerator destroyLittlebird(Entity uav)
        {
            uav.SetField("isAlive", false);
            Entity fx = SpawnFX(AIZ.fx_sentryDeath, uav.Origin);
            TriggerFX(fx);
            uav.Hide();
            uav.TurnEngineOff();
            uav.PlaySound("recondrone_destroyed");
            uav.GetField<Entity>("owner").ClearField("ownedLittlebird");
            yield return Wait(5);
            uav.FreeHelicopter();
            littlebirdOut = false;

            int objID = uav.GetField<int>("realObjID");
            mapEdit._realObjIDList[objID] = false;
            uav.ClearField("realObjID");

            uav.Delete();
            fx.Delete();
        }

        private static void spawnBotForPlayer(Entity owner, string weapon = "", int time = 0)
        {
            if (weapon == botWeapon_subBot)
            {
                owner.SetField("ownsSubBot", false);
                hud.scoreMessage(owner, AIZ.gameStrings[226] + "!");
            }
            else if (weapon == botWeapon_LMGBot)
            {
                owner.SetField("ownsLMGBot", false);
                hud.scoreMessage(owner, AIZ.gameStrings[227] + "!");
            }
            shuffleStreaks(owner);

            Entity bot = Spawn("script_model", owner.Origin);
            bot.Angles = owner.Angles;
            bot.EnableLinkTo();
            bot.SetModel(AIZ.bodyModel);
            bot.SetField("isMoving", false);
            //bot.SetField("isShooting", false);//Moved to "state"
            bot.SetField("currentGun", "");

            Vector3 weaponTag = bot.GetTagOrigin("tag_weapon_left");
            Entity gun = Spawn("script_model", weaponTag);
            gun.SetModel("defaultweapon");
            bot.SetField("gun", gun);

            Entity minimapIcon = SpawnPlane(owner, "script_model", bot.Origin, "compassping_friendly_party_mp", "compassping_friendly_party_mp");
            minimapIcon.Angles = bot.Angles;
            minimapIcon.LinkTo(bot);
            bot.SetField("icon", minimapIcon);

            Entity bothead = Spawn("script_model", bot.Origin);
            bothead.SetModel(AIZ.headModel);
            bothead.LinkTo(bot, "j_spine4", Vector3.Zero, Vector3.Zero);
            bot.SetField("head", bothead);
            bot.SetField("state", "idle");
            bot.SetField("animType", "ar");
            bot.SetField("shots", 0);
            owner.SetField("bot", bot);
            bot.SetField("owner", owner);
            bot.SetField("target", bot);
            botUtil.playAnimOnBot(bot, botAnim_idle);

            OnInterval(100, () => botMovement(bot));
            if (weapon == "") updateBotGun(bot);
            else updateBotGun(bot, weapon);
            OnInterval(100, () => personalBotTargeting(bot));

            if (time > 0)
                AfterDelay(time * 1000, () => StartAsync(killPlayerBot(owner)));
        }
        private static bool botMovement(Entity bot)
        {
            if (AIZ.gameEnded) return false;
            if (bot.GetField<string>("state") == "dead") return false;

            if (bot.GetField<string>("state") != "shooting")
            {
                Vector3 target = bot.GetField<Entity>("owner").Origin;
                if (bot.Origin.DistanceTo(target) >= 200)
                {
                    bot.SetField("state", "running");
                    if (!bot.GetField<bool>("isMoving"))
                    {
                        switch (bot.GetField<string>("animType"))
                        {
                            case "pistol":
                                botUtil.playAnimOnBot(bot, botAnim_runPistol);
                                break;
                            case "mg":
                                botUtil.playAnimOnBot(bot, botAnim_runMG);
                                break;
                            case "rocketlauncher":
                                botUtil.playAnimOnBot(bot, botAnim_runRPG);
                                break;
                            case "spread":
                                botUtil.playAnimOnBot(bot, botAnim_runShotgun);
                                break;
                            case "sniper":
                                botUtil.playAnimOnBot(bot, botAnim_runSniper);
                                break;
                            case "smg":
                                botUtil.playAnimOnBot(bot, botAnim_runSMG);
                                break;
                            default:
                                botUtil.playAnimOnBot(bot, botAnim_run);
                                break;
                        }
                        bot.SetField("isMoving", true);
                    }
                    float angle = VectorToAngles(target - bot.Origin).Y;
                    bot.MoveTo(new Vector3(target.X, target.Y, bot.GetField<Entity>("owner").Origin.Z), (bot.Origin.DistanceTo(target) / 150));
                    bot.RotateTo(new Vector3(0, angle, 0), .2f);
                }
                else if (bot.Origin.DistanceTo(target) < 200 && bot.GetField<bool>("isMoving"))
                {
                    bot.Origin = bot.Origin;
                    bot.SetField("state", "idle");
                    switch (bot.GetField<string>("animType"))
                    {
                        case "pistol":
                            botUtil.playAnimOnBot(bot, botAnim_idlePistol);
                            break;
                        case "mg":
                            botUtil.playAnimOnBot(bot, botAnim_idleMG);
                            break;
                        case "rocketlauncher":
                            botUtil.playAnimOnBot(bot, botAnim_idleRPG);
                            break;
                        default:
                            botUtil.playAnimOnBot(bot, botAnim_idle);
                            break;
                    }
                    bot.SetField("isMoving", false);
                }
                return true;
            }
            else return true;
        }
        public static void updateBotGun(Entity bot, string weapon = "")
        {
            if (bot.GetField<string>("state") == "dead") return;

            Entity owner = bot.GetField<Entity>("owner");
            string newBotWeapon = weapon == "" ? owner.CurrentWeapon : weapon;

            if ((bot.GetField<string>("currentGun") != newBotWeapon && !AIZ.isKillstreakWeapon(newBotWeapon) && !(AIZ.isThunderGun(newBotWeapon) || AIZ.isRayGun(newBotWeapon) || newBotWeapon == "stinger_mp")))
            {
                Entity gun = bot.GetField<Entity>("gun");
                string weaponModel;
                if (newBotWeapon == botWeapon_subBot)
                    weaponModel = GetWeaponModel(botWeaponModel_subBot);
                else if (newBotWeapon == botWeapon_LMGBot)
                    weaponModel = GetWeaponModel(botWeaponModel_LMGBot);
                else weaponModel = GetWeaponModel(newBotWeapon);

                if (weaponModel == null || weaponModel == "") weaponModel = "defaultweapon";
                gun.SetModel(weaponModel);
                bot.SetField("currentGun", newBotWeapon);

                string weaponClass;
                if (newBotWeapon == botWeapon_subBot)
                    weaponClass = "smg";
                else if (newBotWeapon == botWeapon_LMGBot)
                    weaponClass = "mg";
                else weaponClass = WeaponClass(newBotWeapon);

                bot.SetField("animType", weaponClass);
                int clipCount = WeaponClipSize(newBotWeapon);
                if (newBotWeapon == botWeapon_LMGBot)
                    clipCount = 100;
                bot.SetField("clipSize", clipCount);

                gun.Angles = bot.GetTagAngles("tag_weapon_left");
                switch (bot.GetField<string>("animType"))
                {
                    case "pistol":
                    case "spread":
                        gun.LinkTo(bot, "tag_weapon_right", Vector3.Zero, Vector3.Zero);
                        break;
                    default:
                        gun.LinkTo(bot, "tag_weapon_left", Vector3.Zero, Vector3.Zero);
                        break;
                }
                if (bot.GetField<string>("state") != "running")
                    switch (bot.GetField<string>("animType"))
                    {
                        case "pistol":
                            botUtil.playAnimOnBot(bot, botAnim_idlePistol);
                            break;
                        case "mg":
                            botUtil.playAnimOnBot(bot, botAnim_idleMG);
                            break;
                        case "rocketlauncher":
                            botUtil.playAnimOnBot(bot, botAnim_idleRPG);
                            break;
                        default:
                            botUtil.playAnimOnBot(bot, botAnim_idle);
                            break;
                    }
                else
                    switch (bot.GetField<string>("animType"))
                    {
                        case "pistol":
                            botUtil.playAnimOnBot(bot, botAnim_runPistol);
                            break;
                        case "mg":
                            botUtil.playAnimOnBot(bot, botAnim_runMG);
                            break;
                        case "rocketlauncher":
                            botUtil.playAnimOnBot(bot, botAnim_runRPG);
                            break;
                        case "spread":
                            botUtil.playAnimOnBot(bot, botAnim_runShotgun);
                            break;
                        case "sniper":
                            botUtil.playAnimOnBot(bot, botAnim_runSniper);
                            break;
                        case "smg":
                            botUtil.playAnimOnBot(bot, botAnim_runSMG);
                            break;
                        default:
                            botUtil.playAnimOnBot(bot, botAnim_run);
                            break;
                    }
            }
        }
        private static bool personalBotTargeting(Entity bot)
        {
            if (AIZ.gameEnded) return false;
            if (bot.GetField<string>("state") == "dead") return false;
            if (bot.GetField<string>("currentGun") == "none" || bot.GetField<string>("currentGun").Contains("killstreak") || bot.GetField<string>("currentGun").Contains("airdrop")) return true;
            if (bot.GetField<Entity>("target") != bot) return true;

            foreach (Entity zombie in botUtil.botsInPlay)
            {
                if (!zombie.GetField<bool>("isAlive")) continue;
                bool tracePass = SightTracePassed(bot.Origin + new Vector3(0, 0, 30), zombie.Origin + new Vector3(0, 0, 70), false, bot);
                if (!tracePass) continue;
                bot.SetField("target", zombie);
                break;
            }
            if (bot.GetField<Entity>("target") != bot && bot.GetField<string>("state") != "shooting")
            {
                float anglesY = VectorToAngles(bot.GetField<Entity>("target").Origin - bot.Origin).Y;
                bot.RotateTo(new Vector3(0, anglesY, 0), 0.4f);
                bot.SetField("state", "shooting");
                bot.Origin = bot.Origin;
                int waitForShot;
                switch (bot.GetField<string>("animType"))
                {
                    case "pistol":
                        waitForShot = 300;
                        break;
                    case "smg":
                        waitForShot = 50;
                        break;
                    case "rifle":
                        waitForShot = 100;
                        break;
                    case "spread":
                        waitForShot = 1000;
                        break;
                    case "mg":
                        waitForShot = 100;
                        break;
                    case "sniper":
                        waitForShot = 1000;
                        break;
                    case "rocketlauncher":
                        waitForShot = 2000;
                        break;
                    default:
                        waitForShot = 100;
                        break;
                }
                if (bot.GetField<string>("currentGun").Contains("dragunov_mp") || bot.GetField<string>("currentGun").Contains("mk12spr_mp"))//Fixes the bug with the bot fast firing a sniper
                    waitForShot = 1000;

                //bot.SetField("shots", 0);
                OnInterval(waitForShot, () => fireBotWeapon(bot));
            }
            return true;
        }
        private static bool fireBotWeapon(Entity bot)
        {
            if (AIZ.gameEnded) return false;
            if (bot.GetField<string>("state") == "dead") return false;
            if (!bot.GetField<Entity>("target").GetField<bool>("isAlive") || bot.GetField<Entity>("target") == bot)
            {
                switch (bot.GetField<string>("animType"))
                {
                    case "pistol":
                        botUtil.playAnimOnBot(bot, botAnim_idlePistol);
                        break;
                    case "mg":
                        botUtil.playAnimOnBot(bot, botAnim_idleMG);
                        break;
                    case "rocketlauncher":
                        botUtil.playAnimOnBot(bot, botAnim_idleRPG);
                        break;
                    default:
                        botUtil.playAnimOnBot(bot, botAnim_idle);
                        break;
                }
                bot.SetField("state", "idle");
                bot.SetField("isMoving", false);
                bot.SetField("target", bot);
                return false;
            }

            bot.RotateTo(new Vector3(0, VectorToYaw(bot.GetField<Entity>("target").Origin - bot.Origin), 0), 0.4f);

            switch (bot.GetField<string>("animType"))
            {
                case "pistol":
                    botUtil.playAnimOnBot(bot, botAnim_shootPistol);
                    break;
                case "mg":
                    botUtil.playAnimOnBot(bot, botAnim_shootMG);
                    break;
                case "rocketlauncher":
                    botUtil.playAnimOnBot(bot, botAnim_shootRPG);
                    break;
                default:
                    botUtil.playAnimOnBot(bot, botAnim_shoot);
                    break;
            }
            Entity botGunEnt = bot.GetField<Entity>("gun");
            Vector3 flashTag = botGunEnt.GetTagOrigin("tag_flash");
            string botGun = bot.GetField<string>("currentGun");
            MagicBullet(botGun, flashTag, bot.GetField<Entity>("target").Origin + new Vector3(AIZ.rng.Next(30), AIZ.rng.Next(30), AIZ.rng.Next(25, 55)), bot);

            if (botGun == botWeapon_subBot) PlaySoundAtPos(flashTag, "weap_ump45_fire_npc");//Hack in firing sound

            bot.SetField("shots", bot.GetField<int>("shots") + 1);
            int ammo = bot.GetField<int>("clipSize");
            if (bot.GetField<int>("shots") >= ammo)
            {
                /*
                Entity clip = Spawn("script_model", bot.Origin);
                clip.SetModel(AIZ.getWeaponClipModel(botGun));
                clip.LinkTo(bot, "tag_weapon_right", Vector3.Zero, Vector3.Zero);
                */
                botGunEnt.HidePart("tag_clip");
                switch (bot.GetField<string>("animType"))
                {
                    case "pistol":
                        botUtil.playAnimOnBot(bot, botAnim_reloadPistol);
                        bot.PlaySound("weap_usp45_reload_npc");
                        AfterDelay(1500, () => resetBotWeaponAnim(bot, botGun, botAnim_idlePistol));
                        break;
                    case "mg":
                        botUtil.playAnimOnBot(bot, botAnim_reloadMG);
                        bot.PlaySound("weap_m60_reload_npc");
                        AfterDelay(4000, () => resetBotWeaponAnim(bot, botGun, botAnim_idleMG));
                        break;
                    case "rocketlauncher":
                        botUtil.playAnimOnBot(bot, botAnim_reloadRPG);
                        bot.PlaySound("weap_rpg_reload_npc");
                        AfterDelay(2500, () => resetBotWeaponAnim(bot, botGun, botAnim_idleRPG));
                        break;
                    default:
                        botUtil.playAnimOnBot(bot, botAnim_reload);
                        bot.PlaySound("weap_ak47_reload_npc");
                        AfterDelay(2000, () => resetBotWeaponAnim(bot, botGun, botAnim_idle));
                        break;
                }
                return false;
            }
            return true;
        }
        private static void resetBotWeaponAnim(Entity bot, string oldGun, string anim)
        {
            if (bot.GetField<string>("state") == "dead") return;
            if (!bot.HasField("gun")) return;
            Entity botGunEnt = bot.GetField<Entity>("gun");
            botUtil.playAnimOnBot(bot, anim);
            if (oldGun == bot.GetField<string>("currentGun")) botGunEnt.ShowPart("tag_clip");//Avoid trying to show the tag if we switched guns, causes a crash
            bot.SetField("shots", 0);
            bot.SetField("state", "idle");
            bot.SetField("isMoving", false);
            bot.SetField("target", bot);
        }

        public static IEnumerator killPlayerBot(Entity owner)
        {
            Entity bot = owner.GetField<Entity>("bot");
            bot.SetField("state", "dead");
            //owner.SetField("ownsBot", false);
            owner.ClearField("bot");
            bot.PlaySound("generic_death_american_1");

            Entity botGun = bot.GetField<Entity>("gun");
            botGun.Delete();
            bot.ClearField("gun");

            Entity minimapIcon = bot.GetField<Entity>("icon");
            minimapIcon.Delete();
            bot.ClearField("icon");

            bot.StartRagdoll();
            PhysicsExplosionSphere(bot.Origin, 75, 75, AIZ.rng.Next(1, 3));

            yield return Wait(5);

            Entity head = bot.GetField<Entity>("head");
            head.Delete();
            bot.ClearField("head");
            bot.Delete();
        }

        public static bool canCallInHeliSniper(Vector3 pos)
        {
            Vector3 endPos;
            if (heliHeight > pos.Z && heliHeight - pos.Z > 500)
                endPos = new Vector3(pos.X, pos.Y, heliHeight);
            else endPos = pos + new Vector3(0, 0, 2000);
            
            return SightTracePassed(pos + new Vector3(0, 0, 15), endPos, false);
        }

        public static void callHeliSniper(Entity owner, Vector3 location)
        {
            Vector3 pathStart;
            //shuffleStreaks(owner);
            if (heliHeight > location.Z && heliHeight - location.Z > 500)
                pathStart = new Vector3(location.X - 10000, location.Y, heliHeight);
            else pathStart = location + new Vector3(-10000, 0, 2000);

            Vector3 angles = VectorToAngles(location - pathStart);
            Vector3 forward = AnglesToForward(angles);
            Entity lb = SpawnHelicopter(owner, pathStart, forward, "attack_littlebird_mp", "vehicle_little_bird_armed");

            int objID = mapEdit.getNextRealObjID();
            mapEdit._realObjIDList[objID] = true;
            lb.SetField("realObjID", objID);

            //lb.MakeVehicleSolidSphere(128, 0);//Disabled to avoid trolling players

            lb.Angles = new Vector3(0, angles.Y, 0);
            lb.SetField("owner", owner);
            lb.SetVehicleTeam("allies");
            lb.EnableLinkTo();
            lb.SetSpeed(375, 225, 75);
            lb.SetHoverParams(5, 10, 5);
            lb.SetTurningAbility(.75f);
            lb.SetField("hasPassenger", false);
            lb.SetField("doneService", false);
            lb.SetField("readyForEnter", false);
            lb.SetField("heliTime", 120);
            owner.SetField("ownsHeliSniper", false);
            shuffleStreaks(owner);
            StartAsync(heliSniper_flyIn(owner, lb, location));
            OnInterval(200, () => heliSniper_leaveOnPlayerDeath(lb, owner));
        }
        private static IEnumerator heliSniper_flyIn(Entity owner, Entity lb, Vector3 loc)
        {
            if (heliHeight > loc.Z && heliHeight - loc.Z > 500)
                lb.SetVehGoalPos(new Vector3(loc.X, loc.Y, heliHeight), true);
            else
                lb.SetVehGoalPos(loc + new Vector3(0, 0, 2000), true);

            yield return Wait(5.5f);

            if (lb.GetField<bool>("doneService")) yield break;
            lb.SetSpeed(150, 50, 25);
            lb.SetVehGoalPos(loc + new Vector3(0, 0, 150), true);
            Vector3 toPlayer = VectorToAngles(owner.Origin - lb.Origin);
            lb.SetTargetYaw(toPlayer.Y - 90);
            //lb.SetLookAtEnt(owner);
            yield return Wait(5);

            if (lb.GetField<bool>("doneService")) yield break;
            lb.SetField("flying", false);
            OnInterval(1000, () => heliSniper_runTimer(lb));
            lb.SetField("readyForEnter", true);
            Entity enterPos = Spawn("script_model", lb.Origin);
            enterPos.SetModel("tag_origin");
            enterPos.LinkTo(lb, "tag_player_attach_left", new Vector3(0, 25, -10), Vector3.Zero);
            enterPos.SetField("heli", lb);
            enterPos.SetField("range", 80);
            enterPos.SetField("usabletype", "heliExtraction");
            lb.SetField("enterNode", enterPos);
            HudElem headIcon = NewClientHudElem(owner);
            headIcon.Archived = false;
            headIcon.X = enterPos.Origin.X;
            headIcon.Y = enterPos.Origin.Y;
            headIcon.Z = enterPos.Origin.Z + 40;
            headIcon.Alpha = .85f;
            headIcon.SetShader("headicon_heli_extract_point", 10, 10);
            headIcon.SetWaypoint(true, true, false);
            headIcon.SetTargetEnt(enterPos);
            enterPos.SetField("icon", new Parameter(headIcon));
            mapEdit.makeUsable(enterPos, "heliExtraction", 70);
        }
        private static bool heliSniper_runTimer(Entity lb)
        {
            if (!lb.GetField<bool>("flying") && !lb.GetField<bool>("doneService"))
            {
                int time = lb.GetField<int>("heliTime");
                lb.SetField("heliTime", time - 1);
                if (AIZ.gameEnded) { lb.SetField("hasPassenger", false); time = 0; }
                if (time == 0)
                {
                    StartAsync(heliSniper_leave(lb));
                    return false;
                }
                else return true;
            }
            else if (!lb.GetField<bool>("doneService")) return true;
            else return false;
        }
        public static IEnumerator heliSniper_doBoarding(Entity heli, Entity player)
        {
            //player.Hide();
            player.SetField("notTargetable", true);
            player.SetField("heliSniperLastInsertionPos", player.Origin);
            heli.SetField("flying", true);
            player.DisableWeapons();
            player.FreezeControls(true);
            player.GiveWeapon("iw5_mk12spr_mp_acog_xmags");
            player.GiveMaxAmmo("iw5_mk12spr_mp_acog_xmags");
            //Entity visual = player.ClonePlayer();
            //visual.Origin = player.Origin;
            //visual.Hide();
            yield return Wait(.8f);

            Vector3 eye = player.GetEye();
            Entity cam = Spawn("script_model", eye);
            cam.Angles = player.GetPlayerAngles();
            cam.SetModel("tag_origin");
            cam.EnableLinkTo();
            player.PlayerLinkToAbsolute(cam, "tag_origin");
            heli.SetField("hasPassenger", true);

            if (AIZ.gameEnded) yield break;//Abort if game has ended

            StartAsync(heliSniper_animateEnter(player, cam, heli));
        }
        public static IEnumerator heliSniper_animateEnter(Entity player, Entity cam, Entity heli)
        {
            Vector3 tag = heli.GetTagOrigin("tag_player_attach_left") + new Vector3(0, 0, 30);
            Vector3 tagAngles = heli.GetTagAngles("tag_player_attach_left");
            Vector3 front = tag + AnglesToForward(tagAngles + new Vector3(0, 90, 0)) * 50;
            Vector3 angleToHeli = VectorToAngles(tag - front);
            cam.MoveTo(front - new Vector3(0, 0, 100), .5f, .1f, .1f);
            cam.RotateTo(angleToHeli, .5f, .1f, .1f);

            //--Begin movement onto heli--
            //Possibly add our viewmodel here, show it to player only, and play a viewmodel_ anim on it if we can find a mantle anim
            yield return Wait(.55f);
            cam.RotatePitch(-60, 1, .1f, .2f);
            cam.MoveTo(front - new Vector3(0, 0, 50), .5f, .1f, .1f);
            yield return Wait(1.05f);
            cam.MoveTo(front - new Vector3(0, 0, 50), 1.3f, .3f, .3f);
            cam.RotatePitch(65, 2, 1, .5f);
            yield return Wait(2.05f);
            cam.MoveTo(tag - new Vector3(0, 0, 30), 2.5f, .5f, .5f);
            cam.RotatePitch(35, 1.4f, .3f, .3f);
            yield return Wait(1.5f);
            cam.RotatePitch(-40, 1, .4f, .4f);
            yield return Wait(1.05f);
            cam.MoveTo(tag - new Vector3(0, 0, 30), 1.5f, .1f, .1f);
            cam.RotateYaw(180, 1.5f, .5f, .5f);
            //clone.RotateYaw(180, 1.5f, .5f, .5f);
            yield return Wait(1.55f);

            if (AIZ.gameEnded) yield break;//Abort if game has ended, so we don't unlink them from endCam

            player.Unlink();
            player.SetPlayerAngles(cam.Angles);
            player.PlayerLinkTo(heli, "tag_player_attach_left", .5f, 10, 170, 30, 150, false);
            player.SetStance("crouch");
            cam.Delete();
            player.Show();
            player.AllowJump(false);
            player.AllowSprint(false);
            OnInterval(50, () => heliSniper_monitorStance(player));
            player.FreezeControls(false);
            player.EnableWeapons();
            player.SwitchToWeapon("iw5_mk12spr_mp_acog_xmags");
            player.DisableWeaponSwitch();
            player.SetSpreadOverride(1);
            AfterDelay(2000, () => player.Player_RecoilScaleOn(0));
            StartAsync(heliSniper_flyUp(heli, player));
            OnInterval(1000, () => heliSniper_leaveOnAmmoDepleted(heli, player));
        }
        private static bool heliSniper_monitorStance(Entity player)
        {
            if (!player.IsAlive) return false;

            if (player.GetStance() != "crouch") player.SetStance("crouch");
            if (player.GetField<bool>("notTargetable")) return true;
            else return false;
        }
        private static IEnumerator heliSniper_flyUp(Entity lb, Entity player)
        {
            //lb.SetField("flying", true);
            lb.SetAcceleration(25);
            lb.SetDeceleration(15);
            lb.SetSpeed(50);
            lb.SetVehGoalPos(lb.Origin + new Vector3(0, 0, 800), true);
            yield return Wait(2.5f);

            lb.ClearTargetYaw();
            lb.ClearGoalYaw();
            lb.SetField("flying", false);
            lb.SetYawSpeed(100, 50, 50, .2f);
            lb.SetHoverParams(15, 10, 10);
            OnInterval(100, () => heliSniper_watchViewClamp(lb, player));
        }
        private static bool heliSniper_watchViewClamp(Entity lb, Entity player)
        {
                float yaw = player.GetPlayerAngles().Y;
                Vector3 lbAngles = lb.Angles;
                float clamp = lbAngles.Y - yaw;
                if (clamp > -175 && clamp < -130)
                    lb.SetGoalYaw(lbAngles.Y + 10);
                else if (clamp < 15 && clamp > -30)
                    lb.SetGoalYaw(lbAngles.Y - 10);

                if (!lb.GetField<bool>("doneService")) return true;
                else return false;
        }
        private static bool heliSniper_leaveOnAmmoDepleted(Entity heli, Entity player)
        {
                if (player.GetAmmoCount("iw5_mk12spr_mp_acog_xmags") == 0)
                {
                    heli.SetField("heliTime", 0);
                    return false;
                }
                return true;
        }
        private static bool heliSniper_leaveOnPlayerDeath(Entity lb, Entity player)
        {
                if (!player.IsAlive)
                {
                    lb.SetField("hasPassenger", false);
                    StartAsync(heliSniper_leave(lb));
                }

                if (player.IsAlive && !lb.GetField<bool>("doneService")) return true;
                else return false;
        }
        private static IEnumerator heliSniper_leave(Entity lb)
        {
            bool hasPassenger = lb.GetField<bool>("hasPassenger");
            Entity owner = lb.GetField<Entity>("owner");
            lb.SetField("flying", true);
            lb.SetField("doneService", true);
            if (hasPassenger)
            {
                lb.SetSpeed(150, 50, 50);
                lb.SetVehGoalPos(lb.Origin - new Vector3(0, 0, 800), true);
                lb.ClearGoalYaw();
                Vector3 toInsertionPos = VectorToAngles(owner.GetField<Vector3>("heliSniperLastInsertionPos") - lb.Origin);
                lb.SetTargetYaw(toInsertionPos.Y - 90);
                
                yield return Wait(3.05f);

                owner.EnableWeaponSwitch();
                owner.SwitchToWeapon(owner.GetField<string>("lastDroppableWeapon"));
                owner.DisableWeaponSwitch();

                yield return Wait(.75f);

                owner.TakeWeapon("iw5_mk12spr_mp_acog_xmags");
                owner.Unlink();
                owner.EnableWeaponSwitch();
                owner.ResetSpreadOverride();
                owner.Player_RecoilScaleOff();
                owner.AllowSprint(true);
                owner.AllowJump(true);
                OnInterval(50, () => heliSniper_checkForPlayerClipping(owner));
                lb.SetField("hasPassenger", false);
                lb.ClearGoalYaw();
                lb.ClearTargetYaw();
                lb.SetVehGoalPos(lb.Origin + new Vector3(0, 0, 1800), true);

                yield return Wait(5.05f);

                lb.SetSpeed(350, 225, 75);
                lb.SetVehGoalPos(lb.Origin + new Vector3(-100000, 0, 0), false);

                yield return Wait(5);

                lb.FreeHelicopter();

                int objID = lb.GetField<int>("realObjID");
                mapEdit._realObjIDList[objID] = false;
                lb.ClearField("realObjID");

                lb.Delete();
            }
            else
            {
                if (lb.HasField("enterNode"))
                {
                    Entity node = lb.GetField<Entity>("enterNode");
                    node.GetField<HudElem>("icon").Destroy();
                    mapEdit.removeUsable(node);
                }
                lb.SetVehGoalPos(lb.Origin + new Vector3(0, 0, 800), true);
                Vector3 angles = VectorToAngles(lb.Origin + new Vector3(-10000, 0, 0) - lb.Origin);
                lb.SetTargetYaw(angles.Y);
                yield return Wait(3.05f);

                lb.SetSpeed(350, 225, 75);
                lb.SetVehGoalPos(lb.Origin + new Vector3(-10000, 0, 0), false);
                yield return Wait(5);

                lb.FreeHelicopter();

                int objID = lb.GetField<int>("realObjID");
                mapEdit._realObjIDList[objID] = false;
                lb.ClearField("realObjID");

                lb.Delete();
            }
        }

        private static bool heliSniper_checkForPlayerClipping(Entity player)
        {
            //Vector3 ground = PlayerPhysicsTrace(player.Origin, player.Origin - new Vector3(0, 0, 100));
            bool isGrounded = player.IsOnGround();
            if (!isGrounded && AIZ.isPlayer(player))
            {
                //Push the player towards their last insertion position, likely in a legal space instead of outside of the maps
                var toInsertionPos = player.GetField<Vector3>("heliSniperLastInsertionPos") - player.Origin;
                toInsertionPos = VectorNormalize(toInsertionPos);
                var targetOffset = new Vector3(toInsertionPos.X, toInsertionPos.Y, -1);
                player.SetOrigin(player.Origin + targetOffset);

                return true;
            }
            player.SetField("notTargetable", false);
            return false;
        }

        public static void callAirdrop(Entity marker, Vector3 location)
        {
            Entity owner;
            if (marker.HasField("owner")) owner = marker.GetField<Entity>("owner");
            else
            {
                AIZ.printToConsole("A marker doesn't have an owner setup!");
                return;
            }
            Vector3 pathStart = location + new Vector3(-10000, 0, 1800);
            Vector3 angles = VectorToAngles(location - pathStart);
            Vector3 forward = AnglesToForward(angles);
            Entity lb = SpawnHelicopter(owner, pathStart, forward, "littlebird_mp", "vehicle_little_bird_armed");

            int objID = mapEdit.getNextRealObjID();
            mapEdit._realObjIDList[objID] = true;
            lb.SetField("realObjID", objID);

            //float getNorthYaw = Call<Vector3>(247, marker.Origin - lb.Origin).Y;
            //lb.SetField("angles", new Parameter(new Vector3(0, getNorthYaw, 0)));
            lb.SetField("owner", owner);
            lb.SetVehicleTeam(owner.SessionTeam);
            lb.EnableLinkTo();
            lb.SetSpeed(375, 225, 75);
            lb.SetTurningAbility(.3f);
            marker.Delete();
            owner.SetField("ownsAirdrop", false);
            shuffleStreaks(owner);
            owner.PlaySound("US_1mc_use_carepackage");
            StartAsync(airdropFly(owner, lb, location));
        }
        private static IEnumerator airdropFly(Entity owner, Entity lb, Vector3 dropLocation)
        {
            lb.SetVehGoalPos(dropLocation + new Vector3(0, 0, 1800), true);
            Vector3 crateTag = lb.GetTagOrigin("tag_ground");
            Entity crate = Spawn("script_model", crateTag);
            crate.SetModel("com_plasticcase_friendly");
            crate.Angles = new Vector3(0, AIZ.rng.Next(360), 0);
            crate.LinkTo(lb, "tag_ground");
            yield return Wait(5.2f);

            crate.Unlink();
            crate.CloneBrushModelToScriptModel(mapEdit._airdropCollision);
            crate.SetContents(1);
            StartAsync(dropTheCrate(crate, owner, true));

            yield return Wait(1);

            lb.SetVehGoalPos(dropLocation + new Vector3(50000, 0, 1800), true);

            yield return Wait(7);

            lb.FreeHelicopter();

            int objID = lb.GetField<int>("realObjID");
            mapEdit._realObjIDList[objID] = false;
            lb.ClearField("realObjID");

            lb.Delete();
        }

        public static void callEmergencyAirdrop(Entity marker, Vector3 location)
        {
            Entity owner;
            if (marker.HasField("owner")) owner = marker.GetField<Entity>("owner");
            else
            {
                AIZ.printToConsole("A marker doesn't have an owner set up!");
                return;
            }
            /*
            Vector3 direction;
            if (marker.HasField("direction")) direction = marker.GetField<Vector3>("direction");
            else
            {
                Log.Write(LogLevel.All, "A marker doesn't have a direction set up!");
                direction = Vector3.Zero;
            }
            */
            Vector3 yaw = VectorToAngles(location - owner.Origin);
            Vector3 direction = new Vector3(0, yaw.Y, 0);
            Vector3 forward = AnglesToForward(direction);
            Vector3 start = location + (forward * -15000);
            Vector3 pathStart = new Vector3(start.X, start.Y, location.Z + 1800);
            Entity c130 = SpawnPlane(owner, "script_model", pathStart, "compass_objpoint_c130_friendly", "compass_objpoint_c130_enemy");

            int objID = mapEdit.getNextRealObjID();
            mapEdit._realObjIDList[objID] = true;
            c130.SetField("realObjID", objID);

            c130.SetModel("vehicle_ac130_low_mp");
            c130.Angles = direction;
            //float getNorthYaw = VectorToAngles(marker.Origin - pathStart).Y;
            //c130.Angles = new Vector3(0, getNorthYaw, 0);
            marker.Delete();
            owner.SetField("ownsEmergencyAirdrop", false);
            shuffleStreaks(owner);
            owner.PlaySound("US_1mc_use_airdrop");
            StartAsync(emergencyAirdropFly(owner, c130, location, forward));
        }
        private static IEnumerator emergencyAirdropFly(Entity owner, Entity c130, Vector3 location, Vector3 forward)
        {
            Vector3 dropLocation = new Vector3(location.X, location.Y, location.Z + 1800);
            c130.PlayLoopSound("veh_ac130_dist_loop");
            c130.MoveTo(dropLocation, 7.5f);
            Entity[] crates = new Entity[4];
            crates[0] = Spawn("script_model", dropLocation + (forward * 100));
            crates[0].SetField("crateIndex", 0);
            crates[1] = Spawn("script_model", dropLocation + (forward * 200));
            crates[1].SetField("crateIndex", 1);
            crates[2] = Spawn("script_model", dropLocation + (forward * 300));
            crates[2].SetField("crateIndex", 2);
            crates[3] = Spawn("script_model", dropLocation);
            crates[3].SetField("crateIndex", 3);
            foreach (Entity crate in crates)
            {
                crate.SetModel("com_plasticcase_friendly");
                crate.Hide();
            }
            yield return Wait(7.5f);
            foreach (Entity crate in crates)
                StartAsync(dropTheCrate(crate, owner, false));
            c130.MoveTo(dropLocation + (forward * 15000), 7.5f);

            yield return Wait(7.5f);
            int objID = c130.GetField<int>("realObjID");
            mapEdit._realObjIDList[objID] = false;
            c130.ClearField("realObjID");
            c130.Delete();
        }
        private static IEnumerator dropTheCrate(Entity crate, Entity owner, bool isNormalAirdrop)
        {
            if (!isNormalAirdrop)
                crate.Angles = new Vector3(0, AIZ.rng.Next(360), 0);

            crate.CloneBrushModelToScriptModel(mapEdit._airdropCollision);

            if (!isNormalAirdrop)
            {
                int crateIndex = crate.GetField<int>("crateIndex");
                switch (crateIndex)
                {
                    case 0:
                        StartAsync(startCratePhysics(crate, .15f));
                        break;
                    case 1:
                        StartAsync(startCratePhysics(crate, .3f));
                        break;
                    case 2:
                        StartAsync(startCratePhysics(crate, .45f));
                        break;
                    default:
                        StartAsync(startCratePhysics(crate, 0));
                        break;
                }
            }
            else
                StartAsync(startCratePhysics(crate, 0));

            yield return Wait(5);

            int curObjID = 31 - mapEdit.getNextObjID();
            Objective_Add(curObjID, "active", crate.Origin, "compass_objpoint_ammo_friendly");
            //Objective_OnEntity(curObjID, crate);
            crate.SetField("objID", curObjID);
            mapEdit.addObjID(crate, curObjID);
            crate.SetField("range", 75);
            crate.SetField("usabletype", "carePackage");
            crate.SetField("user", crate);
            crate.SetField("streak", AIZ.rng.Next(10));
            crate.SetField("owner", owner);
            mapEdit.makeUsable(crate, "carePackage", 75);

            string iconName = getKillstreakCrateIcon(crate.GetField<int>("streak"));
            Entity iconEnt = Spawn("script_origin", crate.Origin + new Vector3(0, 0, 30));
            HudElem icon = hud.createCarePackageIcon(iconEnt, iconName);
            crate.SetField("icon", icon);
            List<Entity> iconPart = new List<Entity>();
            iconPart.Add(iconEnt);
            crate.SetField("pieces", new Parameter(iconPart));//Set to allow removeUsable to delete

            StartAsync(watchCrateUsage(crate));
        }

        private static IEnumerator startCratePhysics(Entity crate, float delay)
        {
            yield return Wait(delay);

            Vector3 dropImpulse = new Vector3(AIZ.rng.Next(5), AIZ.rng.Next(5), AIZ.rng.Next(5));
            crate.PhysicsLaunchServer(new Vector3(0, 0, 0), dropImpulse);
            crate.Show();
        }

        private static IEnumerator watchCrateUsage(Entity crate)
        {
            yield return Wait(90);

            if (mapEdit.usables.Contains(crate))
                mapEdit.removeUsable(crate);
        }

        public static void deployableExpAmmo(Entity marker, Vector3 pos)
        {
            Entity owner;
            if (marker.HasField("owner")) owner = marker.GetField<Entity>("owner");
            else
            {
                AIZ.printToConsole("A marker doesn't have an owner setup!");
                marker.Delete();
                return;
            }
            AIZ.teamSplash("used_deployable_exp_ammo", owner);
            Entity box = Spawn("script_model", pos);
            box.Angles = new Vector3(0, 0, 90);
            box.SetField("owner", owner);
            box.SetField("range", 75);
            box.SetField("usabletype", "expAmmo");
            box.SetModel("weapon_oma_pack");
            box.PlaySoundToTeam("mp_vest_deployed_ui", "allies");
            box.PlaySound("exp_ammo_box");
            marker.Delete();
            owner.SetField("ownsExpAmmo", false);
            shuffleStreaks(owner);
            mapEdit.makeUsable(box, "expAmmo", 75);
            int curObjID = 31 - mapEdit.getNextObjID();
            Objective_Add(curObjID, "active", box.Origin, "compass_objpoint_deploy_friendly");
            box.SetField("objID", curObjID);
            mapEdit.addObjID(box, curObjID);

            HudElem icon = NewTeamHudElem("allies");
            icon.SetShader("waypoint_ammo_friendly", 8, 8);
            icon.Alpha = .85f;
            icon.X = box.Origin.X;
            icon.Y = box.Origin.Y;
            icon.Z = box.Origin.Z + 20;
            icon.Archived = false;
            icon.SetWaypoint(true, false);
            box.SetField("icon", icon);

            AfterDelay(60000, () => destroyDeployableAmmo(box));
        }
        private static void destroyDeployableAmmo(Entity box)
        {
            PlayFX(AIZ.fx_vestFire, box.Origin);
            mapEdit.removeUsable(box);
            foreach (Entity player in Players)
            {
                if (!player.IsAlive) continue;
                if (player.HasField("hasExpAmmoPerk") && !player.GetField<bool>("hasExpAmmoPerk")) continue;

                player.UnSetPerk("specialty_explosivebullets");
                player.IPrintLnBold("Explosive Bullets gone");
                player.SetField("hasExpAmmoPerk", false);
            }
        }
        #region mapstreaks
        public static void initMapKillstreak()
        {
            switch (AIZ._mapname)
            {
                case "mp_alpha":
                    mapStreakIcon = "objpoint_default";
                    mapStreakName = AIZ.gameStrings[231];//Each use lowers defcon number, Defcon 1 triggers multiple package drops
                    mapStreakKills = 125;
                    level.SetField("defcon", 6);
                    return;
                case "mp_bootleg":
                    mapStreakIcon = "^2ac130_overlay_grain";
                    mapStreakName = AIZ.gameStrings[234];
                    mapStreakKills = 450;
                    return;
                case "mp_bravo":
                    mapStreakIcon = "viper_locked_box";
                    mapStreakName = AIZ.gameStrings[330];
                    mapStreakKills = 125;
                    mapStreakWeapon = "claymore_mp";
                    return;
                case "mp_carbon":
                case "mp_roughneck":
                    mapStreakIcon = "specialty_c4death";
                    mapStreakName = AIZ.gameStrings[331];
                    mapStreakKills = 175;
                    mapStreakWeapon = "airdrop_juggernaut_def_mp";

                    LoadFX("smoke/jet_contrail");
                    return;
                case "mp_dome":
                    mapStreakName = AIZ.gameStrings[230];
                    mapStreakIcon = "dpad_killstreak_talon_static";
                    mapStreakKills = 650;

                    AIZ.fx_tankExplode = (short)LoadFX("explosions/helicopter_explosion_pavelow");
                    return;
                case "mp_paris":
                    mapStreakName = AIZ.gameStrings[332];//Poison Gas Attack
                    mapStreakIcon = "death_moab";
                    mapStreakKills = 325;

                    LoadFX("smoke/airdrop_flare_mp_effect_now");
                    return;
                case "mp_plaza2":
                    mapStreakName = AIZ.gameStrings[333];//Mortar Team
                    mapStreakIcon = "hud_icon_artillery";
                    mapStreakKills = 325;

                    LoadFX("smoke/smoke_geotrail_m203");
                    LoadFX("explosions/artilleryexp_dirt_brown");
                    return;
                case "mp_radar":
                    mapStreakName = AIZ.gameStrings[334];//Blizzard
                    mapStreakIcon = "ac130_overlay_grain";
                    mapStreakKills = 425;
                    return;
                case "mp_courtyard_ss":
                    mapStreakName = AIZ.gameStrings[335];//Volcanic Eruption
                    mapStreakIcon = "death_nuke";
                    mapStreakKills = 1000;
                    return;
                case "mp_hillside_ss":
                    mapStreakName = AIZ.gameStrings[336];//Assault Boats
                    mapStreakIcon = "objective_friendly";
                    mapStreakKills = 550;

                    //LoadFX("treadfx/heli_water");
                    return;
                case "mp_burn_ss":
                    mapStreakIcon = "specialty_precision_airstrike";
                    mapStreakName = AIZ.gameStrings[232];
                    mapStreakKills = 850;

                    LoadFX("fire/jet_afterburner_harrier");
                    LoadFX("smoke/jet_contrail");
                    LoadFX("misc/aircraft_light_red_blink");
                    return;
                default:
                    return;//Return so we dont precache extra assets
            }

            //PreCacheShader(mapStreakIcon);
        }
        private static bool tryUseMapStreak(Entity player)
        {
            if (AIZ._mapname == "mp_alpha")
            {
                decrementDefconLevel(player);
            }
            else if (AIZ._mapname == "mp_bootleg")
            {
                if (!mapStreakOut) startAcidRain(player);
                else return false;
            }
            //else if (AIZ._mapname == "mp_bravo")
                //spawnBarbWire(player);
            //else if (AIZ._mapname == "mp_carbon")
                //spawnOilSpill();
            else if (AIZ._mapname == "mp_dome")
            {
                if (!mapStreakOut) spawnTanks(player);
                else return false;
            }
            else if (AIZ._mapname == "mp_paris")
            {
                if (!mapStreakOut) startPoisonGas(player);
                else return false;
            }
            else if (AIZ._mapname == "mp_plaza2")
            {
                if (!mapStreakOut) startMortarTeam(player);
                else return false;
            }
            else if (AIZ._mapname == "mp_radar")
            {
                if (!mapStreakOut) startBlizzard(player);
                else return false;
            }
            else if (AIZ._mapname == "mp_courtyard_ss")
            {
                if (!mapStreakOut) startVolcanoEruption(player);
                else
                {
                    player.IPrintLnBold(AIZ.gameStrings[340]);
                    player.SetField("cash", player.GetField<int>("cash") + 10000);
                    hud.scorePopup(player, 10000);
                    return true;
                }
            }
            else if (AIZ._mapname == "mp_hillside_ss")
            {
                if (!mapStreakOut) spawnAssaultBoats(player);
                else return false;
            }
            else if (AIZ._mapname == "mp_burn_ss")
            {
                if (!mapStreakOut) spawnA10(player);
                else return false;
            }
            else
            {
                return false;
            }

            shuffleStreaks(player);
            return true;
        }

        private static void decrementDefconLevel(Entity player)
        {
            int currentDefcon = level.GetField<int>("defcon");
            currentDefcon--;
            Announcement(string.Format(AIZ.gameStrings[236], currentDefcon));

            if (currentDefcon == 3)
            {
                PlaySoundAtPos(Vector3.Zero, "mp_defcon_down");
                AIZ.teamSplash("two_from_defcon", player);
            }
            else if (currentDefcon == 2)
            {
                PlaySoundAtPos(Vector3.Zero, "mp_defcon_down");
                AIZ.teamSplash("one_from_defcon", player);
            }
            else if (currentDefcon == 1)
            {
                PlaySoundAtPos(Vector3.Zero, "mp_defcon_one");
                currentDefcon = 5;
                StartAsync(doDefconDrop(player));
                AfterDelay(3000, () => AIZ.teamSplash("callout_earned_carepackage", player));
            }
            else
            {
                PlaySoundAtPos(Vector3.Zero, "mp_defcon_down");
                AIZ.teamSplash("changed_defcon", player);
            }

            //AfterDelay(1000, () => level.PlaySound("US_1mc_defcon_" + currentDefcon));

            level.SetField("defcon", currentDefcon);
        }
        private static IEnumerator doDefconDrop(Entity player)
        {
            Entity marker = Spawn("script_model", Vector3.Zero);
            marker.SetModel("tag_origin");
            marker.Hide();
            marker.SetField("owner", player);

            //Call two Emergency airdrops
            Vector3 location = GetGroundPosition(player.Origin.Around(10), 40);
            callEmergencyAirdrop(marker, location);

            yield return Wait(3);

            marker = Spawn("script_model", Vector3.Zero);
            marker.SetModel("tag_origin");
            marker.Hide();
            marker.SetField("owner", player);

            location = GetGroundPosition(player.Origin.Around(10), 40);
            callEmergencyAirdrop(marker, location);
        }

        private static void startAcidRain(Entity player)
        {
            Announcement(AIZ.gameStrings[329]);
            hud.scoreMessage(player, AIZ.gameStrings[234] + "!");
            mapStreakOut = true;

            foreach (Entity players in Players)
            {
                players.VisionSetNakedForPlayer("tulsa", 2);
            }

            StartAsync(runAcidRain(player));

            AfterDelay(70000, () => mapStreakOut = false);
        }
        private static IEnumerator runAcidRain(Entity player)
        {
            int rainFX = LoadFX("weather/rain_mp_bootleg");
            Entity[] rain = new Entity[10];
            for (int i = 0; i < 10; i++)
            {
                rain[i] = SpawnFX(rainFX, player.Origin);
                TriggerFX(rain[i]);

                yield return Wait(1f);
            }

            while (mapStreakOut)
            {
                if (botUtil.botsInPlay.Count > 0)
                {
                    int randomBot = AIZ.rng.Next(botUtil.botsInPlay.Count);
                    Entity bot = botUtil.botsInPlay[randomBot];

                    if (!bot.GetField<bool>("isAlive"))
                        continue;

                    Entity botHitbox = bot.GetField<Entity>("hitbox");
                    bool isCrawler = !bot.HasField("head") && !bot.HasField("isBoss");

                    botUtil.onBotDamage(botHitbox, 50, player, Vector3.Zero, Vector3.Zero, "MOD_PASSTHRU", "", "", "", "", "bombsite_mp", isCrawler, bot.HasField("isBoss"));
                }

                yield return Wait(RandomFloatRange(.1f, 1f));
            }

            yield return Wait(1);

            foreach (Entity players in Players)
            {
                if (!players.GetField<bool>("isDown") && !roundSystem.isBossWave) players.VisionSetNakedForPlayer(AIZ.vision, 4);
                else if (!players.GetField<bool>("isDown") && roundSystem.isBossWave) players.VisionSetNakedForPlayer(AIZ.bossVision, 4);
                else players.VisionSetNakedForPlayer("cheat_bw");
            }

            foreach (Entity fx in rain)
                fx.Delete();
        }

        public static void spawnBarbWire(Entity marker, Vector3 position)
        {
            Entity owner;
            if (marker.HasField("owner")) owner = marker.GetField<Entity>("owner");
            else
            {
                AIZ.printToConsole("A marker doesn't have an owner set up!");
                return;
            }
            marker.Delete();

            owner.SetField("ownsMapStreak", false);
            shuffleStreaks(owner);

            Vector3 angles = owner.Angles;
            Vector3 forward = AnglesToForward(angles) * 40;
            Entity wire = GSCFunctions.Spawn("script_model", (position - new Vector3(0, 0, 10)) + forward);
            wire.Angles = owner.Angles - new Vector3(0, 90, 0);
            wire.SetModel("mil_barbedwire4");
            Entity trigger = GSCFunctions.Spawn("trigger_radius", wire.Origin, 0, 48, 16);
            trigger.LinkTo(wire, "barbed_wire_04", Vector3.Zero, Vector3.Zero);
            wire.SetField("trigger", trigger);
            wire.SetField("owner", owner);
            owner.SetField("barbed_wire", wire);

            wire.PlaySound("detpack_plant");

            StartAsync(watchBarbedWire(wire, owner));
            AfterDelay(90000, () => wire.SetField("destroy", true));
        }
        private static IEnumerator watchBarbedWire(Entity wire, Entity owner)
        {
            Entity trigger = wire.GetField<Entity>("trigger");
            while (!wire.HasField("destroy"))
            {
                if (botUtil.botsInPlay.Count > 0)
                {
                    for (int i = 0; i < botUtil.botsInPlay.Count; i++)
                    {
                        Entity bot = botUtil.botsInPlay[i];
                        if (!bot.GetField<bool>("isAlive"))
                            continue;

                        if (bot.IsTouching(trigger))
                        {
                            wire.PlaySound("shell_eject_pistol_metalthin");

                            bot.SetField("inBarbedWire", true);
                            bool isCrawler = !bot.HasField("head") && !bot.HasField("isBoss");
                            botUtil.onBotDamage(bot.GetField<Entity>("hitbox"), 15, owner, Vector3.Zero, Vector3.Zero, "MOD_PASSTHRU", "", "", "", 0, "claymore_mp", isCrawler, bot.HasField("isBoss"));
                        }
                        else if (bot.HasField("inBarbedWire")) bot.ClearField("inBarbedWire");
                    }
                }

                foreach (Entity player in Players)
                {
                    if (!player.IsAlive) continue;

                    if (player.IsTouching(trigger))
                    {
                        Vector3 movement = player.GetNormalizedMovement();
                        if (movement.X > 0 || movement.Y > 0)
                            wire.PlaySound("shell_eject_pistol_metalthin");
                    }
                }

                yield return Wait(.25f);
            }

            destroyBarbedWire(wire);
        }
        public static void destroyBarbedWire(Entity wire)
        {
            Entity trigger = wire.GetField<Entity>("trigger");
            trigger.Unlink();
            trigger.Delete();
            wire.ClearField("trigger");
            wire.ClearField("destroy");
            wire.GetField<Entity>("owner").ClearField("barbed_wire");
            wire.ClearField("owner");

            PlayFX(AIZ.fx_sentryDeath, wire.Origin);
            wire.PlaySound("shell_eject_pistol_metalthick");

            wire.Delete();
        }

        public static void spawnOilSpill(Entity marker, Vector3 pos)
        {
            Entity owner;
            if (marker.HasField("owner")) owner = marker.GetField<Entity>("owner");
            else
            {
                AIZ.printToConsole("A marker doesn't have an owner set up!");
                return;
            }
            marker.Delete();

            owner.SetField("ownsMapStreak", false);
            shuffleStreaks(owner);

            int oilFire;
            if (AIZ._mapname == "mp_carbon") oilFire = LoadFX("fire/flame_refinery_small_far_3");
            else if (AIZ._mapname == "mp_roughneck") oilFire = LoadFX("maps/mp_roughneck/mp_rn_rigpipefire");
            else return;

            Entity fireSound = Spawn("script_origin", pos);
            Entity fire = SpawnFX(oilFire, pos);
            TriggerFX(fire);

            fire.PlaySound("flashbang_explode_default");
            fireSound.PlayLoopSound("medfire");

            StartAsync(runOilSpill(fire, fireSound, owner));
        }
        private static IEnumerator runOilSpill(Entity fire, Entity fireSound, Entity owner)
        {
            int damageCount = 0;
            while (damageCount < 30)
            {
                RadiusDamage(fire.Origin, 128, 100, 50, owner);
                damageCount++;

                yield return Wait(.5f);
            }

            fireSound.StopSound();

            yield return WaitForFrame();

            fireSound.Delete();
            fire.Delete();
        }

        private static void startPoisonGas(Entity player)
        {
            Announcement(AIZ.gameStrings[337]);
            mapStreakOut = true;

            StartAsync(runPoisonGas(player));

            AfterDelay(60000, () => mapStreakOut = false);
        }
        private static IEnumerator runPoisonGas(Entity player)
        {
            int gasFX = LoadFX("smoke/airdrop_flare_mp_effect_now");
            Entity[] gas = new Entity[botUtil.botSpawns.Count];

            for (int i = 0; i < gas.Length; i++)
            {
                gas[i] = SpawnFX(gasFX, botUtil.botSpawns[i]);
                TriggerFX(gas[i]);

                yield return Wait(.1f);
            }

            while (mapStreakOut)
            {
                if (botUtil.botsInPlay.Count > 0)
                {
                    for (int i = 0; i < botUtil.botsInPlay.Count; i++)
                    {
                        Entity bot = botUtil.botsInPlay[i];

                        if (!bot.GetField<bool>("isAlive"))
                            continue;

                        foreach (Entity gasPoint in gas)
                        {
                            if (bot.Origin.DistanceTo(gasPoint.Origin) < 300)
                            {
                                Entity botHitbox = bot.GetField<Entity>("hitbox");
                                bool isCrawler = !bot.HasField("head") && !bot.HasField("isBoss");

                                botUtil.onBotDamage(botHitbox, 50, player, Vector3.Zero, Vector3.Zero, "MOD_PASSTHRU", "", "", "", "", "bombsite_mp", isCrawler, bot.HasField("isBoss"));

                                break;
                            }
                        }
                    }
                }

                yield return Wait(RandomFloatRange(.1f, 1f));
            }

            yield return Wait(1);

            foreach (Entity fx in gas)
                fx.Delete();
        }

        private static void spawnTanks(Entity player)
        {
            AIZ.teamSplash("used_remote_tank", player);
            mapStreakOut = true;

            Entity tank1 = SpawnVehicle("vehicle_m1a1_abrams_dmg", "vehicle_tank", "remote_ugv_mp", new Vector3(-3113, 524, -152), new Vector3(3, 8, 7), player);
            tank1.Health = 1000;
            tank1.MaxHealth = 1000;
            tank1.SetCanDamage(false);
            tank1.EnableLinkTo();
            tank1.SetSpeed(20, 10, 5);
            tank1.SetField("departPos", new Vector3(-3332, 434, -148));
            tank1.SetField("perchPos", new Vector3(-1136, 760, -214));
            tank1.SetField("eyePos", new Vector3(-968, 895, -182));
            tank1.SetField("destroyed", false);
            tank1.SetField("isEngaging", false);
            tank1.SetField("weapon", "remote_tank_projectile_mp");
            tank1.SetField("owner", player);
            tank1.SetVehicleLookAtText("The Knockout");
            tank1.SetVehWeapon("sam_mp");
            tank1.SetVehicleTeam("allies");
            //Hide turret
            tank1.HidePart("turret_animate_jnt");
            tank1.HidePart("barrel_animate_jnt");
            tank1.HidePart("hatch_l_open_jnt");
            tank1.HidePart("hatch_r_open_jnt");
            //Spawn a script turret
            Entity tank1Turret = SpawnTurret("misc_turret", tank1.Origin, "sam_mp");
            tank1Turret.Angles = tank1.Angles;
            tank1Turret.SetModel(tank1.Model);
            tank1Turret.SetCanDamage(false);
            tank1Turret.SetSentryOwner(player);
            tank1Turret.MakeUnUsable();
            tank1Turret.SetDefaultDropPitch(0);
            tank1Turret.HidePart("body_animate_jnt");
            tank1Turret.HidePart("left_wheel_09_jnt");
            tank1Turret.HidePart("right_wheel_09_jnt");
            tank1Turret.LinkTo(tank1, "body_animate_jnt", new Vector3(0, 0, -44), Vector3.Zero);
            tank1Turret.SetField("currentYaw", tank1Turret.Angles.Y);
            tank1.SetField("turret", tank1Turret);

            Entity tank2 = SpawnVehicle("vehicle_m1a1_abrams_dmg", "vehicle_tank", "remote_ugv_mp", new Vector3(3327, -2066, -575), new Vector3(-10, 140, -14), player);
            tank2.Health = 1000;
            tank2.MaxHealth = 1000;
            tank2.SetCanDamage(false);
            tank2.EnableLinkTo();
            tank2.SetSpeed(20, 10, 5);
            tank2.SetField("departPos", new Vector3(3282, -2057, -585));
            tank2.SetField("perchPos", new Vector3(2488, -1335, -352));
            tank2.SetField("eyePos", new Vector3(1511, -620, -119));
            tank2.SetField("destroyed", false);
            tank2.SetField("isEngaging", false);
            tank2.SetField("weapon", "uav_strike_projectile_mp");
            tank2.SetField("owner", player);
            tank2.SetVehicleLookAtText("The Pounder");
            tank2.SetVehWeapon("sam_mp");
            tank2.SetVehicleTeam("allies");
            //Hide turret
            tank2.HidePart("turret_animate_jnt");
            tank2.HidePart("barrel_animate_jnt");
            tank2.HidePart("hatch_l_open_jnt");
            tank2.HidePart("hatch_r_open_jnt");
            //Spawn a script turret
            Entity tank2Turret = SpawnTurret("misc_turret", tank2.Origin, "sam_mp");
            tank2Turret.Angles = tank2.Angles;
            tank2Turret.SetModel(tank2.Model);
            tank2Turret.SetCanDamage(false);
            tank2Turret.SetSentryOwner(player);
            tank2Turret.MakeUnUsable();
            tank2Turret.SetDefaultDropPitch(0);
            tank2Turret.HidePart("body_animate_jnt");
            tank2Turret.HidePart("left_wheel_09_jnt");
            tank2Turret.HidePart("right_wheel_09_jnt");
            tank2Turret.LinkTo(tank2, "body_animate_jnt", new Vector3(0, 0, -44), Vector3.Zero);
            tank2Turret.SetField("currentYaw", tank2Turret.Angles.Y);
            tank2.SetField("turret", tank2Turret);

            Entity tank3 = SpawnVehicle("vehicle_m1a1_abrams_dmg", "vehicle_tank", "remote_ugv_mp", new Vector3(5440, 3933, 686), new Vector3(0, -144, 0), player);
            tank3.Health = 1000;
            tank3.MaxHealth = 1000;
            tank3.SetCanDamage(false);
            tank3.EnableLinkTo();
            tank3.SetSpeed(20, 10, 5);
            tank3.SetField("departPos", new Vector3(5440, 3933, 686));
            tank3.SetField("perchPos", new Vector3(4212, 3164, 587));
            tank3.SetField("eyePos", new Vector3(2257, 1655, 519));
            tank3.SetField("destroyed", false);
            tank3.SetField("isEngaging", false);
            tank3.SetField("weapon", "remote_tank_projectile_mp");
            tank3.SetField("owner", player);
            tank3.SetVehicleLookAtText("Kenshin13");
            tank3.SetVehWeapon("sam_mp");
            tank3.SetVehicleTeam("allies");
            //Hide turret
            tank3.HidePart("turret_animate_jnt");
            tank3.HidePart("barrel_animate_jnt");
            tank3.HidePart("hatch_l_open_jnt");
            tank3.HidePart("hatch_r_open_jnt");
            //Spawn a script turret
            Entity tank3Turret = SpawnTurret("misc_turret", tank3.Origin, "sam_mp");
            tank3Turret.Angles = tank3.Angles;
            tank3Turret.SetModel(tank3.Model);
            tank3Turret.SetCanDamage(false);
            tank3Turret.SetSentryOwner(player);
            tank3Turret.MakeUnUsable();
            tank3Turret.SetDefaultDropPitch(0);
            tank3Turret.HidePart("body_animate_jnt");
            tank3Turret.HidePart("left_wheel_09_jnt");
            tank3Turret.HidePart("right_wheel_09_jnt");
            tank3Turret.LinkTo(tank3, "body_animate_jnt", new Vector3(0, 0, -44), Vector3.Zero);
            tank3Turret.SetField("currentYaw", tank3Turret.Angles.Y);
            tank3.SetField("turret", tank3Turret);

            //Drive the tanks to their designated perch
            tank1.MoveTo(tank1.GetField<Vector3>("perchPos"), 8);
            tank2.MoveTo(tank2.GetField<Vector3>("perchPos"), 10);
            tank3.MoveTo(tank3.GetField<Vector3>("perchPos"), 6);

            AfterDelay(8050, () => tank_arriveAtPerch(tank1, tank1Turret));
            AfterDelay(10050, () => tank_arriveAtPerch(tank2, tank2Turret));
            AfterDelay(6050, () => tank_arriveAtPerch(tank3, tank3Turret));
        }
        private static void tank_arriveAtPerch(Entity tank, Entity tankTurret)
        {
            tankTurret.Unlink();
            tankTurret.Origin = tank.GetField<Vector3>("perchPos");
            OnInterval(10000, () => tank_searchForTargets(tank));
            StartAsync(tank_timer(tank));
        }
        private static IEnumerator tank_timer(Entity tank)
        {
            yield return Wait(45 + AIZ.rng.Next(5));

            if (tank.GetField<bool>("isEngaging"))
            {
                OnInterval(500, () => tank_waitForFlee(tank));
            }
            else
            {
                tank.SetField("destroyed", true);
                tank_flee(tank);
            }
        }
        private static bool tank_waitForFlee(Entity tank)
        {
            if (tank.GetField<bool>("isEngaging")) return true;
            else
            {
                tank.SetField("destroyed", true);
                tank_flee(tank);
                return false;
            }
        }
        private static bool tank_searchForTargets(Entity tank)
        {
            if (tank.GetField<bool>("destroyed")) return false;

            foreach (Entity bot in botUtil.botsInPlay)
            {
                if (!bot.GetField<bool>("isAlive")) continue;

                Vector3 eye = tank.GetField<Vector3>("eyePos");
                Entity botHitbox = bot.GetField<Entity>("hitbox");

                bool trace = SightTracePassed(eye, botHitbox.Origin + 30, false);
                if (!trace) continue;

                StartAsync(tank_engageTarget(tank, botHitbox));
                break;
            }
            return true;
        }
        private static IEnumerator tank_engageTarget(Entity tank, Entity target)
        {
            Entity tankTurret = tank.GetField<Entity>("turret");
            tankTurret.Unlink();
            tankTurret.Origin = tank.GetField<Vector3>("perchPos");
            float yaw = VectorToYaw(target.Origin - tankTurret.Origin);
            float localYaw = tank.Angles.Y;
            Vector3 targetAngles;
            float currentYaw = tankTurret.GetField<int>("currentYaw");
            //Log.Debug("Set yaw is {0}; Current Yaw is {1}; yaw - current is {2}; localYaw {3}", yaw, currentYaw, yaw - currentYaw, localYaw);
            tankTurret.AddYaw((int)(yaw - currentYaw));
            targetAngles = tankTurret.Angles;
            tankTurret.SetField("currentYaw", yaw);
            tankTurret.AddYaw((int)-(yaw - currentYaw));
            tankTurret.RotateTo(targetAngles, 2);
            tank.SetField("isEngaging", true);

            yield return Wait(2.05f);

            tankTurret.LinkTo(tank, "body_animate_jnt", new Vector3(0, 0, -44), new Vector3(0, yaw - localYaw, 0));
            tank.FireWeapon("origin_animate_jnt", tank, Vector3.Zero);
            tankTurret.Angles = targetAngles;
            Vector3 start = tankTurret.GetTagOrigin("tag_flash");
            Vector3 angles = AnglesToForward(tankTurret.Angles);
            Entity missile = MagicBullet(tank.GetField<string>("weapon"), start, tankTurret.Origin + angles * 7000, tank);
            missile.SetTargetEnt(target);
            missile.SetFlightModeTop();
            tank.SetField("isEngaging", false);
        }
        private static void tank_flee(Entity tank)
        {
            Entity missile = MagicBullet("remote_mortar_missile_mp", tank.GetField<Vector3>("perchPos") + new Vector3(0, 0, 5000), tank.GetField<Vector3>("perchPos"));
            missile.OnNotify("explode", (m, pos) =>
            {
                if (Utilities.isEntDefined(tank) && tank.TargetName == "vehicle_tank")
                    tank_destroyTank(tank, pos.As<Vector3>());
            });
        }
        private static void tank_destroyTank(Entity tank, Vector3 pos)
        {
            //tank.PlaySound("");
            PlayFX(AIZ.fx_tankExplode, pos);
            tank.GetField<Entity>("turret").Delete();
            tank.ClearField("turret");
            tank.FreeVehicle();
            tank.Delete();

            mapStreakOut = false;
        }

        private static void startMortarTeam(Entity owner)
        {
            hud.scoreMessage(owner, AIZ.gameStrings[333] + "!");

            mapStreakOut = true;

            OnInterval(1750, () => runMortarTeam(owner));

            AfterDelay(15000, () => mapStreakOut = false);
        }
        private static bool runMortarTeam(Entity owner)
        {
            if (!mapStreakOut)
                return false;

            if (botUtil.botsInPlay.Count == 0)
                return true;

            StartAsync(launchMortar(owner));

            return true;
        }
        private static IEnumerator launchMortar(Entity owner)
        {
            Entity targetBot = botUtil.botsInPlay[AIZ.rng.Next(botUtil.botsInPlay.Count)];
            Vector3 endPos = targetBot.Origin;
            Entity mortar = Spawn("script_model", endPos + new Vector3(0, 0, 5000));
            mortar.SetModel("projectile_at4");
            mortar.Angles = new Vector3(90, 0, 0);
            mortar.PlaySound("bmp_fire");

            yield return WaitForFrame();

            int trailFX = LoadFX("smoke/smoke_geotrail_m203");
            PlayFXOnTag(trailFX, mortar, "tag_fx");
            int explodeFX = LoadFX("explosions/artilleryexp_dirt_brown");

            mortar.MoveTo(endPos, 2);

            yield return Wait(2);

            RadiusDamage(endPos, 512, 500, 100, owner);
            PlayFX(explodeFX, endPos);
            mortar.PlaySound("detpack_explo_layer");
            mortar.Delete();
        }

        private static void startBlizzard(Entity owner)
        {
            Announcement(AIZ.gameStrings[338]);
            mapStreakOut = true;

            StartAsync(runBlizzard(owner));

            AfterDelay(45000, () => mapStreakOut = false);
        }
        private static IEnumerator runBlizzard(Entity player)
        {
            int snowFX = LoadFX("snow/snow_blizzard_radar");
            Entity[] snow = new Entity[6];
            for (int i = 0; i < 6; i++)
            {
                snow[i] = SpawnFX(snowFX, player.Origin);
                TriggerFX(snow[i]);

                yield return Wait(1f);
            }

            while (mapStreakOut)
            {
                if (botUtil.botsInPlay.Count > 0)
                {
                    int randomBot = AIZ.rng.Next(botUtil.botsInPlay.Count);
                    Entity bot = botUtil.botsInPlay[randomBot];

                    if (!bot.GetField<bool>("isAlive"))
                        continue;

                    Entity botHitbox = bot.GetField<Entity>("hitbox");
                    bool isCrawler = !bot.HasField("head") && !bot.HasField("isBoss");

                    botUtil.onBotDamage(botHitbox, 20, player, Vector3.Zero, Vector3.Zero, "MOD_PASSTHRU", "", "", "", "", "bombsite_mp", isCrawler, bot.HasField("isBoss"));
                }

                yield return Wait(RandomFloatRange(.5f, 1.5f));
            }

            yield return Wait(1);

            foreach (Entity fx in snow)
                fx.Delete();
        }

        private static void startVolcanoEruption(Entity owner)
        {
            Announcement(AIZ.gameStrings[339]);
            PlaySoundAtPos(Vector3.Zero, "mp_lose_flag");
            mapStreakOut = true;

            StartAsync(doVolcanoEruption(owner));
        }
        private static IEnumerator doVolcanoEruption(Entity owner)
        {
            int fx_explosion = LoadFX("explosions/javelin_explosion");
            int fx_volcano = LoadFX("maps/mp_courtyard_ss/mp_ct_volcano");
            Vector3 volcano = new Vector3(584, 18103, 5665);

            Entity rumbleSound = Spawn("script_origin", new Vector3(-498, 1516, 503));
            rumbleSound.PlayLoopSound("elm_explosions_dist");

            yield return Wait(10);

            Earthquake(.5f, 10, rumbleSound.Origin, 999999);
            PlayFX(fx_explosion, volcano);

            yield return Wait(3f);

            PlaySoundAtPos(rumbleSound.Origin, "nuke_explosion_dist_sub");
            for (int i = 0; i < 5; i++)
            {
                Entity smoke = SpawnFX(fx_volcano, volcano);
                TriggerFX(smoke);
            }

            foreach (Entity players in Players)
            {
                players.VisionSetNakedForPlayer("dcemp_emp", 3);
            }

            Entity[] mapFX = new Entity[4];

            mapFX[0] = SpawnFX(fx_volcano, new Vector3(-585, -641, 11));
            mapFX[1] = SpawnFX(fx_volcano, new Vector3(-502, -1536, 15));
            mapFX[2] = SpawnFX(fx_volcano, new Vector3(383, -1518, 15));
            mapFX[3] = SpawnFX(fx_volcano, new Vector3(438, -646, 12));

            yield return Wait(1);

            foreach (Entity fx in mapFX)
                TriggerFX(fx);

            botUtil.nukeDetonation(owner, true);
            botUtil.spawnedBots = botUtil.botsForWave;
            PlaySoundAtPos(rumbleSound.Origin, "nuke_wave");

            yield return Wait(7);

            rumbleSound.StopLoopSound();
            rumbleSound.Delete();

            AIZ.vision = "dcburning_trenches";//Replace the vision for the match
            SetSunlight(new Vector3(0, 0, 0));//Remove sunlight
            foreach (Entity players in Players)
            {
                if (!players.GetField<bool>("isDown") && !roundSystem.isBossWave) players.VisionSetNakedForPlayer(AIZ.vision, 4);
                else if (!players.GetField<bool>("isDown") && roundSystem.isBossWave) players.VisionSetNakedForPlayer(AIZ.bossVision, 4);
                else players.VisionSetNakedForPlayer("cheat_bw");
            }

            yield return Wait(15);

            for (int i = 0; i < mapFX.Length; i++)
                mapFX[i].Delete();
        }

        private static void spawnAssaultBoats(Entity owner)
        {
            mapStreakOut = true;
            AIZ.teamSplash("used_remote_tank", owner);

            AfterDelay(70000, () => mapStreakOut = false);

            Entity[] boats = new Entity[2];

            boats[0] = Spawn("script_model", new Vector3(27999, -18866, 2140));
            boats[0].SetField("startPos", new Vector3(27999, -18866, 2140));
            boats[0].SetField("endPos", new Vector3(3700, -1000, 2140));
            boats[1] = Spawn("script_model", new Vector3(34606, 26639, 2140));
            boats[1].SetField("startPos", new Vector3(34606, 26639, 2140));
            boats[1].SetField("endPos", new Vector3(3700, 1250, 2140));

            foreach (Entity boat in boats)
            {
                boat.SetModel("yacht_modern_periph");
                Vector3 angles = VectorToAngles(boat.GetField<Vector3>("endPos") - boat.GetField<Vector3>("startPos"));
                boat.Angles = angles;
                boat.ScriptModelPlayAnim("yacht_modern_periph_idle1");
                boat.SetField("owner", owner);
                boat.PlayLoopSound("veh_ac130_ext_close");
                Entity turret = SpawnTurret("script_turret", boat.Origin + (AnglesToForward(boat.Angles) * 550) + (AnglesToUp(boat.Angles) * 170), "sentry_minigun_mp");
                turret.SetModel("mp_remote_turret");
                turret.Angles = boat.Angles;
                turret.MakeTurretInOperable();
                turret.SetRightArc(80);
                turret.SetLeftArc(80);
                turret.SetBottomArc(80);
                turret.MakeUnUsable();
                turret.SetDefaultDropPitch(-89.0f);
                turret.SetTurretModeChangeWait(true);
                turret.SetMode("sentry");
                turret.SetField("owner", owner);
                turret.SetTurretTeam("allies");
                turret.SetSentryOwner(owner);
                turret.LinkTo(boat, "tag_origin");
                turret.SetField("isBeingCarried", true);//This controls turret availability
                turret.SetField("timeLeft", 1);
                turret.SetField("isSentryGL", false);
                OnInterval(100, () => sentry_targeting(turret));
                OnInterval(5000, () => assaultBoats_runSentry(turret));
                boat.SetField("turret", turret);
                StartAsync(assaultBoats_rideIn(boat));

                //assaultBoat_runBoatFX(boat);
            }
        }
        private static bool assaultBoats_runSentry(Entity turret)
        {
            turret.SetField("isBeingCarried", !turret.GetField<bool>("isBeingCarried"));
            if (!turret.GetField<bool>("isBeingCarried"))
                turret.PlaySound("talon_reload");

            if (turret.GetField<int>("timeLeft") < 1)
                return false;

            return true;
        }
        private static IEnumerator assaultBoats_rideIn(Entity boat)
        {
            boat.MoveTo(boat.GetField<Vector3>("endPos"), 10, 0, 7.5f);

            yield return Wait(10);

           boat.StopLoopSound();
           boat.PlaySound("veh_ac130_ext_close_fade");

            yield return Wait(1);

            OnInterval(7500, () => assaultBoat_attack(boat));
        }
        private static bool assaultBoat_attack(Entity boat)
        {
            foreach (Entity bot in botUtil.botsInPlay)
            {
                if (!bot.GetField<bool>("isAlive")) continue;

                Vector3 eye = new Vector3(2272, -488, 3906);
                Entity botHitbox = bot.GetField<Entity>("hitbox");

                bool trace = SightTracePassed(eye, botHitbox.Origin + 30, false);
                if (!trace) continue;

                AfterDelay(RandomIntRange(100, 1000), () =>
                {
                    Entity missile = MagicBullet("javelin_mp", boat.Origin + new Vector3(0, 0, 100), bot.Origin, boat);
                    missile.SetTargetEnt(bot);
                    missile.SetFlightModeTop();
                });

                break;
            }

            if (!mapStreakOut)
            {
                StartAsync(assaultBoat_leave(boat));
                return false;
            }

            return true;
        }
        private static IEnumerator assaultBoat_leave(Entity boat)
        {
            boat.PlayLoopSound("veh_ac130_ext_close");
            Entity turret = boat.GetField<Entity>("turret");
            turret.SetField("isBeingCarried", true);
            turret.SetField("timeLeft", 0);

            Vector3 target = boat.GetField<Vector3>("startPos");
            float targetYaw = VectorToYaw(target - boat.Origin);

            boat.MoveTo(boat.GetField<Vector3>("endPos") - new Vector3(250, 0, 0), 2, .5f, .5f);
            boat.RotateTo(new Vector3(0, targetYaw, 0), 7, 2, 2);

            yield return Wait(3);

            boat.MoveTo(target, 10, 5);

            yield return Wait(10);

            turret.Delete();
            boat.StopLoopSound();
            boat.Delete();
        }

        private static void spawnA10(Entity owner)
        {
            //Sanity check
            if (AIZ._mapname != "mp_burn_ss") return;

            mapStreakOut = true;

            Entity a10 = SpawnHelicopter(owner, new Vector3(-25000, 0, heliHeight), Vector3.Zero, "harrier_mp", "vehicle_a10_warthog");//SpawnPlane(owner, "script_model", new Vector3(-15000, 0, heliHeight), "compass_objpoint_reaper_friendly", "compass_objpoint_reaper_enemy");

            int objID = mapEdit.getNextRealObjID();
            mapEdit._realObjIDList[objID] = true;
            a10.SetField("realObjID", objID);

            a10.SetField("owner", owner);
            a10.SetVehicleTeam("allies");
            a10.SetField("mapCenter", new Vector3(-800, -30, heliHeight));
            a10.SetField("state", "strafing");
            a10.SetField("hasFired", false);
            a10.SetField("timeLeft", 90);
            a10.SetSpeed(300, 80, 0);
            a10.SetSpeedImmediate(300);
            a10.SetCanDamage(false);
            a10.SetMaxPitchRoll(0, 75);
            a10.SetYawSpeed(50, 25, 20, .5f);
            a10.SetVehWeapon("ac130_25mm_mp");
            //owner.PlaySound("US_1mc_use_a10");
            owner.PlaySound("US_1mc_use_strafe");
            AIZ.teamSplash("used_a10_support", owner);

            Entity[] a10Sound = new Entity[5];
            for (int i = 0; i < a10Sound.Length; i++)
            {
                a10Sound[i] = Spawn("script_origin", a10.Origin);
                a10Sound[i].LinkTo(a10);
            }
            a10.SetField("sound", new Parameter(a10Sound));

            Entity[] a10Brr = new Entity[5];
            for (int i = 0; i < a10Brr.Length; i++)
            {
                a10Brr[i] = Spawn("script_origin", a10.Origin);
                a10Brr[i].LinkTo(a10);
            }
            a10.SetField("brrSound", new Parameter(a10Brr));

            StartAsync(doA10FlyBy(a10));
            StartAsync(playA10FX(a10));
            OnInterval(50, () => a10_targeting(a10));
            OnInterval(50, () => a10_monitorGoalPos(a10));
            OnInterval(1000, () => a10_timer(a10));
            AfterDelay(2000, () => owner.PlayLocalSound("US_1mc_KS_hqr_a10"));
        }
        private static IEnumerator doA10FlyBy(Entity a10)
        {
            yield return WaitForFrame();
            a10.SetVehGoalPos(a10.GetField<Vector3>("mapCenter"), false);
        }
        private static bool a10_targeting(Entity a10)
        {
            if (a10.GetField<string>("state") != "strafing") return true;
            if (a10.HasField("target")) return true;

            Vector3 gunTag = a10.GetTagOrigin("tag_gun");
            foreach (Entity bot in botUtil.botsInPlay)
            {
                if (!bot.HasField("isAlive")) continue;
                if (!bot.GetField<bool>("isAlive")) continue;

                Entity botHitbox = bot.GetField<Entity>("hitbox");
                if (a10.Origin.DistanceTo2D(botHitbox.Origin) > 6000) continue;

                bool trace = SightTracePassed(gunTag, botHitbox.Origin, false, botHitbox, a10);
                if (!trace) continue;

                a10.SetField("target", bot);
                a10.SetTurretTargetEnt(bot);
                OnInterval(100, () => a10_fireWeapon(a10, bot));
                a10.PlayLoopSound("pavelow_mg_loop");//Start our firing sound here
                Entity[] a10Sound = a10.GetField<Entity[]>("sound");
                foreach (Entity sound in a10Sound)
                    sound.PlayLoopSound("veh_ac130_ext_close");
                break;//Found a target, leave this loop to target that bot only
            }

            if (!a10.HasField("owner")) return false;
            return true;
        }
        private static bool a10_monitorGoalPos(Entity a10)
        {
            if (!a10.HasField("owner")) return false;
            if (a10.GetField<string>("state") != "strafing") return true;

            if (a10.Origin.DistanceTo2D(a10.GetField<Vector3>("mapCenter")) < 256)
                StartAsync(a10_uturn(a10));

            return true;
        }
        private static IEnumerator a10_uturn(Entity a10)
        {
            Vector3 right = AnglesToRight(new Vector3(a10.Angles.X, a10.Angles.Y, 0));
            if (RandomInt(100) < 50) right *= -1;
            Vector3 turnDirector = a10.GetField<Vector3>("mapCenter") + (right * 14000);

            a10.SetVehGoalPos(turnDirector, false);

            yield return Wait(1);

            Entity[] a10Sound = a10.GetField<Entity[]>("sound");
            foreach (Entity sound in a10Sound)
                sound.StopLoopSound();

            yield return Wait(1);

            if (a10.GetField<bool>("hasFired")) a10_playBrr(a10);

            if (!a10.HasField("owner")) yield break;//In case it left during this part of the uturn

            a10.SetField("state", "uturn");
            turnDirector.Y -= (right.Y * 4000);
            a10.SetVehGoalPos(turnDirector, false);

            yield return Wait(3);

            //a10.SetSpeedImmediate(300);
            a10.SetVehGoalPos(a10.GetField<Vector3>("mapCenter"), false);
            a10.SetField("state", "strafing");
        }
        private static IEnumerator a10_leave(Entity a10)
        {
            while (a10.GetField<string>("state") != "strafing")
                yield return Wait(.5f);

            a10.ClearField("owner");

            Vector3 forward = AnglesToForward(a10.Angles);
            a10.SetVehGoalPos(a10.Origin + (forward * 50000), false);

            yield return Wait(7);

            a10.ClearField("state");
            a10.ClearField("mapCenter");
            a10.ClearField("timeLeft");
            Entity[] a10Sound = a10.GetField<Entity[]>("sound");
            foreach (Entity sound in a10Sound)
                sound.Delete();
            Entity[] brrSound = a10.GetField<Entity[]>("brrSound");
            foreach (Entity sound in brrSound)
                sound.Delete();
            a10.FreeHelicopter();
            a10.Delete();
            mapStreakOut = false;
        }
        private static bool a10_fireWeapon(Entity a10, Entity target)
        {
            if (!a10.HasField("owner")) return false;
            if (!a10.HasField("target")) return false;

            if (!target.GetField<bool>("isAlive"))
            {
                a10.ClearField("target");//Target mysteriously died
                a10.ClearTurretTarget();
                a10.StopLoopSound();//Stop our fire sound
                return false;
            }

            a10.SetField("hasFired", true);

            Vector3 gunTag = a10.GetTagOrigin("tag_gun");
            MagicBullet("ac130_25mm_mp", gunTag, target.Origin, a10.GetField<Entity>("owner"));
            //a10.FireWeapon("tag_gun", target);

            bool trace = SightTracePassed(gunTag, target.Origin, false, target, a10);
            if (!trace)
            {
                a10.ClearField("target");//Lost sight of the target
                a10.ClearTurretTarget();
                a10.StopLoopSound();//Stop our fire sound
                return false;
            }
            return true;
        }
        private static bool a10_timer(Entity a10)
        {
            if (!a10.HasField("owner")) return false;
            a10.SetField("timeLeft", a10.GetField<int>("timeLeft") - 1);
            //Log.Write(LogLevel.All, "Time is {0}", uav.GetField<int>("timeLeft"));
            if (a10.GetField<int>("timeLeft") > 0 && a10.GetField<Entity>("owner").IsAlive) return true;
            else
            {
                StartAsync(a10_leave(a10));
                return false;
            }
        }
        private static void a10_playBrr(Entity a10)
        {
            a10.SetField("hasFired", false);
            Entity[] brrSound = a10.GetField<Entity[]>("brrSound");
            foreach (Entity brr in brrSound)
            {
                brr.PlayLoopSound("ac130_40mm_fire_npc");
                AfterDelay(1000, () => brr.StopLoopSound());
            }
        }
        private static IEnumerator playA10FX(Entity a10)
        {
            yield return Wait(.2f);

            int fx_afterburner = LoadFX("fire/jet_afterburner_harrier");
            int fx_contrail = LoadFX("smoke/jet_contrail");
            int fx_tail = LoadFX("misc/aircraft_light_red_blink");

            if (fx_contrail != 0)
            {
                PlayFXOnTag(fx_contrail, a10, "tag_right_wingtip");
                PlayFXOnTag(fx_contrail, a10, "tag_left_wingtip");
            }

            yield return Wait(.2f);

            if (fx_afterburner != 0)
            {
                PlayFXOnTag(fx_afterburner, a10, "tag_engine_right");
                PlayFXOnTag(fx_afterburner, a10, "tag_engine_left");
            }

            yield return Wait(.2f);
            PlayFXOnTag(AIZ.fx_rayGun, a10, "tag_left_wingtip");
            yield return Wait(.2f);
            PlayFXOnTag(AIZ.fx_rayGunUpgrade, a10, "tag_right_wingtip");

            if (fx_tail == 0) yield break;

            yield return Wait(.2f);
            PlayFXOnTag(fx_tail, a10, "tag_left_tail");
            yield return Wait(.2f);
            PlayFXOnTag(fx_tail, a10, "tag_right_tail");
        }
        #endregion
    }
}
