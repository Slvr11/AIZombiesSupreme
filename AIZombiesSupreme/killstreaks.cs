using System.Collections;
using InfinityScript;
using static InfinityScript.GSCFunctions;

namespace AIZombiesSupreme
{
    public class killstreaks : BaseScript
    {
        public static bool nukeInbound = false;
        public static int empKills = 25;
        private static byte nukeTime = 10;
        //public static int mapStreakKills = -1;
        //public static string mapStreakWeapon = "killstreak_double_uav_mp";
        //public static string mapStreakIcon = "specialty_placeholder";
        //public static string mapStreakName = "Map Killstreak";
        private static bool littlebirdOut = false;
        //private static bool mapStreakOut = false;

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
            else if (streak == 1000 && AIZ._mapname != "mp_carbon" && AIZ._mapname != "mp_cement" && !player.GetField<bool>("ownsBot"))//Disabled on carbon and cement for crappy optimization
            {
                player.SetField("ownsBot", true);
                player.IPrintLnBold("1000 Kill Streak!");
                AfterDelay(1000, () => player.IPrintLnBold("Permanent Bot Achieved!"));
                spawnBotForPlayer(player);
            }
            /*
            else if (streak == mapStreakKills)
            {
                player.PlayLocalSound("mp_killstreak_choppergunner");
                player.GiveWeapon(mapStreakWeapon, 0, false);
                player.SetActionSlot(7, "weapon", mapStreakWeapon);
                player.IPrintLnBold("^3" + mapStreakName + " ^7ready for usage!");
                player.SetField("ownsMapStreak", true);
            }
            */
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
                case 9:
                    streak = 150;
                    break;
                case 10:
                    streak = 400;
                    break;
                case 11:
                    bonusDrops.spawnBonusDrop(bonusDrops.dropTypes.doublePoints, player.Origin);
                    return;
                case 12:
                    bonusDrops.spawnBonusDrop(bonusDrops.dropTypes.instaKill, player.Origin);
                    return;
                case 13:
                    bonusDrops.spawnBonusDrop(bonusDrops.dropTypes.nuke, player.Origin);
                    return;
                case 14:
                    bonusDrops.spawnBonusDrop(bonusDrops.dropTypes.gun, player.Origin);
                    return;
                default:
                    return;
            }
            player.Kills = streak;
            checkKillstreak(player);
            player.Kills = oldStreak;
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
                spawnSentry(player);
                //Entity sentryModel = d_killstreaks.spawnSentry(player);
                //if (sentryModel != null)
                //d_killstreaks.sentryHoldWatcher(player, sentryModel, true);
                player.SwitchToWeapon(player.GetField<string>("lastDroppableWeapon"));
                //player.DisableWeapons();
            }
            else if (player.GetField<bool>("ownsLittlebird") && newWeap == "killstreak_uav_mp")
            {
                if (!littlebirdOut)
                {
                    Entity littlebirdModel = spawnLittlebird(player);
                    if (littlebirdModel != null)
                        littlebirdHoldWatcher(player, littlebirdModel);
                    else return;
                }
                else
                {
                    player.IPrintLnBold("Airspace too crowded.");
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
                    player.IPrintLnBold("Cannot call in Heli Sniper while teleporting");
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
                    player.IPrintLnBold("Cannot call in Heli Sniper here");
                    player.SwitchToWeapon(player.GetField<string>("lastDroppableWeapon"));
                }
            }
            /*
            else if (player.GetField<bool>("ownsMapStreak") && newWeap == mapStreakWeapon)
            {
                bool success = tryUseMapStreak(player);
                string lastWeapon = player.GetField<string>("lastDroppableWeapon");

                if (!success)
                {
                    player.SwitchToWeapon(lastWeapon);
                    return;
                }

                player.SetField("ownsMapStreak", false);
                AfterDelay(750, () =>
                        player.TakeWeapon(mapStreakWeapon));
                player.SwitchToWeapon(lastWeapon);
            }
            */
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

            if (player.GetField<bool>("ownsExpAmmo"))
                player.SetActionSlot(6, "weapon", "deployable_vest_marker_mp");
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
            //if (player.GetField<bool>("ownsMapStreak"))
            //player.SetActionSlot(7, "weapon", mapStreakWeapon);

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

            if (player.GetField<bool>("ownsExpAmmo"))
                streaks[2] = hud.createHudShaderString("specialty_deployable_vest") + "[{+actionslot 6}]";
            else if (player.GetField<bool>("ownsEMP"))
                streaks[2] = hud.createHudShaderString("specialty_emp") + "[{+actionslot 6}]";

            if (player.GetField<bool>("ownsNuke"))
                streaks[3] = hud.createHudShaderString("dpad_killstreak_nuke_static") + "[{+actionslot 7}]";
            else if (player.GetField<bool>("ownsEmergencyAirdrop"))
                streaks[3] = hud.createHudShaderString("specialty_airdrop_emergency") + "[{+actionslot 7}]";
            //if (player.GetField<bool>("ownsMapStreak"))
            //streaks[3] = hud.createHudShaderString(mapStreakIcon) + "[{+actionslot 7}]";

            if (!player.HasField("aizHud_created")) return;

            HudElem list = player.GetField<HudElem>("hud_killstreakList");
            string newText = streaks[0] + "\n\n" + streaks[1] + "\n\n" + streaks[2] + "\n\n" + streaks[3];
            if ((string)list.GetField("text") == newText) return;
            list.SetField("text", newText);
            list.SetText(newText);
        }

        public static void spawnSentry(Entity player)
        {
            if (player.GetField<bool>("isCarryingSentry")) return; 

            Entity turret = SpawnTurret("misc_turret", player.Origin, "sentry_minigun_mp");
            turret.Angles = new Vector3(0, player.GetPlayerAngles().Y, 0);
            turret.SetModel("sentry_minigun");
            //turret.Health = 1000;
            //turret.SetCanDamage(true);
            turret.MakeTurretInOperable();
            turret.SetRightArc(80);
            turret.SetLeftArc(80);
            turret.SetBottomArc(50);
            turret.MakeUnUsable();
            turret.SetDefaultDropPitch(-89.0f);
            turret.SetTurretModeChangeWait(true);
            turret.SetMode("sentry_offline");
            turret.SetField("owner", player);
            turret.SetTurretTeam("allies");
            turret.SetSentryOwner(player);
            turret.SetField("isSentry", true);

            turret.SetTurretMinimapVisible(true);
            int objID = mapEdit.getNextRealObjID();
            mapEdit._realObjIDList[objID] = true;
            turret.SetField("realObjID", objID);

            turret.SetField("isBeingCarried", true);
            turret.SetField("canBePlaced", true);
            turret.SetField("timeLeft", 90);
            turret.SetField("target", turret);
            Entity trigger = Spawn("trigger_radius", turret.Origin + new Vector3(0, 0, 1), 0, 105, 64);
            turret.SetField("trigger", trigger);
            trigger.EnableLinkTo();
            trigger.LinkTo(turret);

            OnInterval(1000, () => sentry_timer(turret));
            OnInterval(50, () => sentry_targeting(turret));
            sentryHoldWatcher(player, turret, true);
        }

        public static void sentryHoldWatcher(Entity player, Entity sentry, bool canCancel)
        {
            sentry.ClearTargetEntity();
            sentry.SetMode("sentry_offline");
            sentry.SetField("isBeingCarried", true);
            player.SetField("isCarryingSentry", true);
            sentry.SetField("canBePlaced", true);
            player.DisableWeapons();
            //sentry.SetCanDamage(false);
            sentry.SetSentryCarrier(player);
            sentry.SetModel("sentry_minigun_obj");

            OnInterval(50, () =>
            {
                if (AIZ.gameEnded) return false;
                if (!player.IsAlive || !AIZ.isPlayer(player)) return false;
                if (sentry.GetField<bool>("canBePlaced") && player.GetField<bool>("isCarryingSentry") && player.AttackButtonPressed() && player.IsOnGround())
                {
                    player.EnableWeapons();
                    if (canCancel)
                    {
                        AIZ.teamSplash("used_sentry", player);
                        player.TakeWeapon("killstreak_sentry_mp");
                        player.SwitchToWeapon(player.GetField<string>("lastDroppableWeapon"));
                        player.SetField("ownsSentry", false);
                        shuffleStreaks(player);
                    }
                    player.SetField("isCarryingSentry", false);
                    //sentry.SetField("carriedBy");
                    sentry.SetSentryCarrier(null);
                    Vector3 angleToForward = AnglesToForward(new Vector3(0, player.GetPlayerAngles().Y, 0));
                    sentry.Origin = player.Origin + angleToForward * 50;
                    sentry.Angles = new Vector3(0, player.GetPlayerAngles().Y, 0);
                    sentry.SetField("isBeingCarried", false);
                    sentry.SetModel("sentry_minigun");
                    sentry.PlaySound("sentry_gun_plant");
                    //turret.SetCanDamage(true);
                    sentry.SetMode("sentry");   
                    //AfterDelay(500, () => StartAsync(handlePickup(sentry)));
                    AfterDelay(500, () => handlePickupInterval(sentry));
                    return false;
                }
                else return true;
            });
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
                        sentry.ShootTurret();
                    }
                    else
                        sentry.ClearTargetEntity();

                    return true;
                }
                else return true;
            }
            else return false;
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
            //if (!Utilities.isEntDefined(turret)) return;

            Entity owner = turret.GetField<Entity>("owner");
            Entity trigger = turret.GetField<Entity>("trigger");

            //if (!Utilities.isEntDefined(trigger)) return;

            OnInterval(100, () =>
            {
                if (AIZ.gameEnded) return false;
                if (owner.IsAlive && owner.IsTouching(trigger) && owner.IsOnGround() && owner.UseButtonPressed())
                {
                    if (!owner.GetField<bool>("isCarryingSentry") && !turret.GetField<bool>("isBeingCarried"))
                        sentryHoldWatcher(owner, turret, false);
                    return false;
                }
                if (owner.IsAlive && turret.Health > 0) return true;
                else return false;
            });
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
            trigger.Delete();

            //Entity fx = sentry.GetField<Entity>("flashFx");
            //if (Utilities.isEntDefined(fx))
                //fx.Delete();

            sentry.ClearTargetEntity();
            sentry.SetCanDamage(false);
            sentry.SetDefaultDropPitch(40);
            sentry.SetMode("sentry_offline");
            sentry.Health = 0;
            sentry.SetModel("sentry_minigun_destroyed");
            sentry.PlaySound("sentry_explode");

            Entity owner = sentry.GetField<Entity>("owner");
            if (owner.IsAlive) owner.PlayLocalSound("US_1mc_sentry_gone");

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
            sentry.Delete();
        }

        public static IEnumerator launchMissile(Entity owner)
        {
            hud.scoreMessage(owner, "Missile!");
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
                foreach (Entity player in Players)
                {
                    if (!player.IsAlive) continue;
                    //player.Call("playlocalsound", "US_1mc_use_emp");
                    player.PlayLocalSound("emp_activate");

                    VisionSetNaked(AIZ.vision, 1);
                    VisionSetPain("near_death_mp");
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
                player.IPrintLnBold("Nuke already inbound!");
                return false;
            }
            nukeInbound = true;
            AfterDelay(11000, () => { if (!AIZ.gameEnded) botUtil.nukeDetonation(player, true); });
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
            nukeTimer.SetText("Nuke Incoming In: 10");
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

            if (nukeTime > 0) nukeTimer.SetText("Nuke Incoming In: " + nukeTime);
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
        public static void littlebirdHoldWatcher(Entity player, Entity bird)
        {
            bird.SetField("isBeingCarried", true);
            player.SetField("isCarryingSentry", true);
            bird.SetField("canBePlaced", true);
            player.DisableWeapons();
            bird.SetSentryCarrier(player);
            bird.SetCanDamage(false);
            OnInterval(100, () =>
            {
                if (AIZ.gameEnded) return false;

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
                    bird.GetField<Entity>("visual").Delete();
                    return false;
                }
                else return true;
            });
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
            hint.SetText("Press ^3[{vote no}] ^7to re-route the drone");
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
            if (AIZ.gameEnded) return false;

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

        private static void spawnBotForPlayer(Entity owner)
        {
            Entity bot = Spawn("script_model", owner.Origin);
            bot.Angles = owner.Angles;
            bot.EnableLinkTo();
            bot.SetModel(AIZ.bodyModel);
            bot.SetField("isMoving", false);
            //bot.SetField("isShooting", false);//Moved to "state"
            bot.SetField("currentGun", "");

            Entity gun = Spawn("script_model", bot.GetTagOrigin("tag_weapon_left"));
            gun.SetModel("defaultweapon");
            bot.SetField("gun", gun);

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
            updateBotGun(bot);
            OnInterval(100, () => personalBotTargeting(bot));
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
                    bot.MoveTo(new Vector3(target.X, target.Y, bot.GetField<Entity>("owner").Origin.Z), (bot.Origin.DistanceTo(target) / 130));
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
        public static void updateBotGun(Entity bot)
        {
            if (bot.GetField<string>("state") == "dead") return;

            Entity owner = bot.GetField<Entity>("owner");
            if (bot.GetField<string>("currentGun") != owner.CurrentWeapon && !AIZ.isKillstreakWeapon(owner.CurrentWeapon) && !(AIZ.isThunderGun(owner.CurrentWeapon) || AIZ.isRayGun(owner.CurrentWeapon) || owner.CurrentWeapon == "stinger_mp"))
            {
                Entity gun = bot.GetField<Entity>("gun");
                string weaponModel = GetWeaponModel(owner.CurrentWeapon);
                if (weaponModel == null || weaponModel == "") weaponModel = "defaultweapon";
                gun.SetModel(weaponModel);
                bot.SetField("currentGun", owner.CurrentWeapon);
                bot.SetField("animType", WeaponClass(owner.CurrentWeapon));
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
            MagicBullet(botGun, flashTag, bot.GetField<Entity>("target").Origin + new Vector3(AIZ.rng.Next(30), AIZ.rng.Next(30), AIZ.rng.Next(25, 55)), bot.GetField<Entity>("owner"));

            bot.SetField("shots", bot.GetField<int>("shots") + 1);
            int ammo = WeaponClipSize(botGun);
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
            bot.SetField("target", bot);
        }

        public static void killPlayerBotOnDeath(Entity owner)
        {
            Entity bot = owner.GetField<Entity>("bot");
            bot.SetField("state", "dead");
            owner.SetField("ownsBot", false);
            owner.ClearField("bot");
            bot.PlaySound("generic_death_american_1");

            Entity botGun = bot.GetField<Entity>("gun");
            botGun.Delete();
            bot.ClearField("gun");

            bot.StartRagdoll();
            PhysicsExplosionSphere(bot.Origin, 75, 75, AIZ.rng.Next(1, 3));
            AfterDelay(5000, () =>
            {
                Entity head = bot.GetField<Entity>("head");
                head.Delete();
                bot.Delete();
            });
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
            StartAsync(heliSniperFlyIn(owner, lb, location));
            heliSniper_leaveOnPlayerDeath(lb, owner);
        }
        private static IEnumerator heliSniperFlyIn(Entity owner, Entity lb, Vector3 loc)
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
            OnInterval(1000, () => heliTimer(lb));
            lb.SetField("readyForEnter", true);
            Entity enterPos = Spawn("script_model", lb.Origin);
            enterPos.SetModel("tag_origin");
            enterPos.LinkTo(lb, "tag_player_attach_left", new Vector3(0, 25, -10), Vector3.Zero);
            enterPos.SetField("heli", lb);
            enterPos.SetField("range", 80);
            enterPos.SetField("usabletype", "heliExtraction");
            lb.SetField("enterNode", enterPos);
            HudElem headIcon = NewClientHudElem(owner);
            //headIcon.Archived = true;
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
        private static bool heliTimer(Entity lb)
        {
            if (!lb.GetField<bool>("flying") && !lb.GetField<bool>("doneService"))
            {
                int time = lb.GetField<int>("heliTime");
                lb.SetField("heliTime", time - 1);
                if (AIZ.gameEnded) { lb.SetField("hasPassenger", false); time = 0; }
                if (time == 0)
                {
                    StartAsync(heliLeave(lb));
                    return false;
                }
                else return true;
            }
            else if (!lb.GetField<bool>("doneService")) return true;
            else return false;
        }
        public static IEnumerator doHeliBoard(Entity heli, Entity player)
        {
            //player.Hide();
            player.SetField("isInHeliSniper", true);
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

            StartAsync(animateHeliEnter(player, cam, heli));
        }
        public static IEnumerator animateHeliEnter(Entity player, Entity cam, Entity heli)
        {
            Vector3 tag = heli.GetTagOrigin("tag_player_attach_left") + new Vector3(0, 0, 30);
            Vector3 tagAngles = heli.GetTagAngles("tag_player_attach_left");
            Vector3 front = tag + AnglesToForward(tagAngles + new Vector3(0, 90, 0)) * 50;
            Vector3 angleToHeli = VectorToAngles(tag - front);
            cam.MoveTo(front - new Vector3(0, 0, 100), .5f, .1f, .1f);
            cam.RotateTo(angleToHeli, .5f, .1f, .1f);
            //clone.Show();
            //clone.RotateTo(angleToHeli, .5f, .3f, .3f);
            //clone.LinkTo(player, "tag_origin");

            //--Begin movement onto heli--
            //Possible add our viewmodel here, show it to player only, and play a viewmodel_ anim on it if we can find a mantle anim
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
            OnInterval(50, () =>
            {
                if (player.GetStance() != "crouch") player.SetStance("crouch");
                if (player.GetField<bool>("isInHeliSniper")) return true;
                else return false;
            });
            player.FreezeControls(false);
            player.EnableWeapons();
            player.SwitchToWeapon("iw5_mk12spr_mp_acog_xmags");
            player.DisableWeaponSwitch();
            player.SetSpreadOverride(1);
            AfterDelay(2000, () => player.Player_RecoilScaleOn(0));
            StartAsync(heliSniperFlyUp(heli, player));
            heliSniper_leaveOnAmmoDepleted(heli, player);
        }
        private static IEnumerator heliSniperFlyUp(Entity lb, Entity player)
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
            OnInterval(100, () => watchHeliViewClamp(lb, player));
        }
        private static bool watchHeliViewClamp(Entity lb, Entity player)
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
        private static void heliSniper_leaveOnAmmoDepleted(Entity heli, Entity player)
        {
            OnInterval(1000, () =>
            {
                if (player.GetAmmoCount("iw5_mk12spr_mp_acog_xmags") == 0)
                {
                    heli.SetField("heliTime", 0);
                    return false;
                }
                return true;
            });
        }
        private static void heliSniper_leaveOnPlayerDeath(Entity lb, Entity player)
        {
            OnInterval(200, () =>
            {
                if (!player.IsAlive)
                {
                    lb.SetField("hasPassenger", false);
                    StartAsync(heliLeave(lb));
                }

                if (player.IsAlive && !lb.GetField<bool>("doneService")) return true;
                else return false;
            });
        }
        private static IEnumerator heliLeave(Entity lb)
        {
            bool hasPassenger = lb.GetField<bool>("hasPassenger");
            Entity owner = lb.GetField<Entity>("owner");
            lb.SetField("flying", true);
            lb.SetField("doneService", true);
            if (hasPassenger)
            {
                lb.SetSpeed(150, 50, 50);
                lb.SetVehGoalPos(lb.Origin - new Vector3(0, 0, 800), true);
                lb.SetTargetYaw(lb.Angles.Y);
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
                //Temporary fix, push player forward on X axis until they're unstuck
                player.SetOrigin(player.Origin + new Vector3(1, 0, -1));
                return true;
            }
            player.SetField("isInHeliSniper", false);
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
            Vector3 dropImpulse = new Vector3(AIZ.rng.Next(5), AIZ.rng.Next(5), AIZ.rng.Next(5));
            crate.PhysicsLaunchServer(Vector3.Zero, dropImpulse);
            yield return Wait(1);

            lb.SetVehGoalPos(dropLocation + new Vector3(10000, 0, 1800), false);
            yield return Wait(4);

            int curObjID = 31 - mapEdit.getNextObjID();
            Objective_Add(curObjID, "active", crate.Origin, "compass_objpoint_ammo_friendly");
            //Objective_OnEntity(curObjID, crate);
            crate.SetField("objID", curObjID);
            mapEdit.addObjID(crate, curObjID);
            crate.SetField("range", 75);
            crate.SetField("usabletype", "carePackage");
            crate.SetField("user", crate);
            crate.SetField("streak", AIZ.rng.Next(9));
            crate.SetField("owner", owner);
            mapEdit.makeUsable(crate, "carePackage", 75);
            StartAsync(watchCrateUsage(crate));
            yield return Wait(5);

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
                StartAsync(dropTheCrate(crate, owner));
            c130.MoveTo(dropLocation + (forward * 15000), 7.5f);

            yield return Wait(7.5f);
            int objID = c130.GetField<int>("realObjID");
            mapEdit._realObjIDList[objID] = false;
            c130.ClearField("realObjID");
            c130.Delete();
        }
        private static IEnumerator dropTheCrate(Entity crate, Entity owner)
        {
            crate.Angles = new Vector3(0, AIZ.rng.Next(360), 0);
            crate.CloneBrushModelToScriptModel(mapEdit._airdropCollision);
            Vector3 dropImpulse = new Vector3(AIZ.rng.Next(5), AIZ.rng.Next(5), AIZ.rng.Next(5));
            int crateIndex = crate.GetField<int>("crateIndex");
            if (crateIndex == 0)
            {
                AfterDelay(100, () =>
                {
                    crate.PhysicsLaunchServer(new Vector3(0, 0, 0), dropImpulse);
                    crate.Show();
                });
            }
            else if (crateIndex == 1)
            {
                AfterDelay(250, () =>
                {
                    crate.PhysicsLaunchServer(new Vector3(0, 0, 0), dropImpulse);
                    crate.Show();
                });
            }
            else if (crateIndex == 2)
            {
                AfterDelay(400, () =>
                {
                    crate.PhysicsLaunchServer(new Vector3(0, 0, 0), dropImpulse);
                    crate.Show();
                });
            }
            else
            {
                crate.PhysicsLaunchServer(new Vector3(0, 0, 0), dropImpulse);
                crate.Show();
            }

            yield return Wait(5);
            int curObjID = 31 - mapEdit.getNextObjID();
            Objective_Add(curObjID, "active", crate.Origin, "compass_objpoint_ammo_friendly");
            //Objective_OnEntity(curObjID, crate);
            crate.SetField("objID", curObjID);
            mapEdit.addObjID(crate, curObjID);
            crate.SetField("range", 75);
            crate.SetField("usabletype", "carePackage");
            crate.SetField("user", crate);
            crate.SetField("streak", AIZ.rng.Next(9));
            crate.SetField("owner", owner);
            mapEdit.makeUsable(crate, "carePackage", 75);
            StartAsync(watchCrateUsage(crate));
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
        /*
        public static void initMapKillstreak()
        {
            switch (AIZ._mapname)
            {
                case "mp_dome":
                    //mapStreakIcon = "death_nuke";
                    //mapStreakName = "M.O.A.B.";
                    mapStreakName = "Tank Strike";
                    mapStreakIcon = "dpad_killstreak_talon_static";
                    //mapStreakKills = 650;

                    AIZ.fx_tankExplode = LoadFX("explosions/helicopter_explosion_pavelow");
                    break;
                case "mp_alpha":
                    //mapStreakIcon = "";
                    mapStreakName = "Defcon Trigger System";//Each use lowers defcon number, Defcon 1 triggers multiple package drops
                    //mapStreakKills = 125;
                    level.SetField("defcon", 6);
                    return;
            //case "":
            //    //case "":
            //    mapStreakName = "Flamethrower";
            //    //case "":
            //    mapStreakName = "Flash Flood";
            //    //case "":
            //    mapStreakName = "Drivable Humvee";
            //    //case "":
            //    mapStreakName = "";
            //    break;
                default:
                    return;//Return so we dont precache extra assets
            }

            PreCacheShader(mapStreakIcon);
        }
        private static bool tryUseMapStreak(Entity player)
        {
            if (AIZ._mapname == "mp_dome")
                if (!mapStreakOut) spawnTanks(player);
                else return false;
            //return tryUseMoab(player);
            else if (AIZ._mapname == "mp_alpha")
                decrementDefconLevel(player);
            //else if (AIZ._mapname == "mp_burn_ss")
            //spawnA10(player);
            else
            {
                return false;
            }

            return true;
        }

        private static bool tryUseMoab(Entity player)
        {
            if (AIZ.gameEnded) return false;

            if (nukeInbound)
            {
                player.IPrintLnBold("M.O.A.B. already inbound!");
                return false;
            }
            nukeInbound = true;
            AfterDelay(11000, () => { if (!AIZ.gameEnded) moabDetonation(player); });
            AIZ.teamSplash("used_nuke", player);
            player.SetField("ownsMapStreak", false);
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
            nukeTimer.SetText("M.O.A.B. Incoming In: 10");
            nukeTimer.Color = new Vector3(.7f, 0, 0);
            nukeTimer.GlowColor = new Vector3(0, 0, .5f);
            nukeTimer.GlowAlpha = .4f;

            int nukeTime = 10;
            OnInterval(1000, () =>
            {
                if (AIZ.gameEnded) return false;
                nukeTimer.ChangeFontScaleOverTime(.2f);
                nukeTimer.FontScale = 1.25f;
                AfterDelay(200, () =>
                {
                    if (nukeTime > 0) nukeTimer.SetText("M.O.A.B. Incoming In: " + nukeTime);
                    nukeTimer.ChangeFontScaleOverTime(.2f);
                    nukeTimer.FontScale = 1;
                });
                nukeTime--;
                level.PlaySound("ui_mp_nukebomb_timer");
                if (nukeTime > 0) return true;
                else { AfterDelay(250, () => nukeTimer.Destroy()); return false; }
            });

            foreach (Entity players in Players)
            {
                if (!AIZ.isPlayer(players)) continue;
                AfterDelay(9000, () =>
                {
                    if (AIZ.gameEnded) return;
                    players.VisionSetNakedForPlayer("mpnuke", 5);
                    players.PlayLocalSound("nuke_explosion");
                    players.PlayLocalSound("nuke_wave");
                });
                AfterDelay(16000, () =>
                {
                    if (AIZ.gameEnded) return;
                    if (players.IsAlive && players.GetField<bool>("isDown")) players.VisionSetNakedForPlayer("cheat_bw", 1);
                    else players.VisionSetNakedForPlayer(AIZ.vision, 5);
                });
            }

            return true;
        }
        private static void moabDetonation(Entity player)
        {
            AfterDelay(1000, () => botUtil.nukeDetonation(player, true));

            Entity check = GetEnt("moabHasDetonated", "targetname");
            if (check != null) return;

            //Spawn our check ent
            Entity checker = Spawn("script_origin", Vector3.Zero);
            checker.TargetName = "moabHasDetonated";

            //Destroy destructibles
            foreach (Entity e in AIZ.getAllEntitiesWithName("destructible_toy"))
            {
                int delay = AIZ.rng.Next(500);
                AfterDelay(delay, () => e.Notify("damage", e.Health, "", new Vector3(0, 0, 0), new Vector3(0, 0, 0), "MOD_EXPLOSIVE", "", "", "", 0, "frag_grenade_mp"));
            }
            foreach (Entity e in AIZ.getAllEntitiesWithName("destructible_vehicle"))
            {
                int delay = AIZ.rng.Next(500);
                AfterDelay(delay, () => e.Notify("damage", 999999, "", new Vector3(0, 0, 0), new Vector3(0, 0, 0), "MOD_EXPLOSIVE", "", "", "", 0, "frag_grenade_mp"));
            }
            foreach (Entity e in AIZ.getAllEntitiesWithName("explodable_barrel"))
            {
                int delay = AIZ.rng.Next(500);
                AfterDelay(delay, () => e.Notify("damage", 999999, "", new Vector3(0, 0, 0), new Vector3(0, 0, 0), "MOD_EXPLOSIVE", "", "", "", 0, "frag_grenade_mp"));
            }
            foreach (Entity e in AIZ.getAllEntitiesWithName("pipe_shootable"))
            {
                int delay = AIZ.rng.Next(500);
                AfterDelay(delay, () => e.Notify("damage", e.Health, level, Vector3.RandomXY(), new Vector3(0, 0, 0), "MOD_PISTOL_BULLET"));
            }
            //Destroy glass
            int glassCount = GetEntArray("glass", "targetname").GetHashCode();
            for (int i = 0; i < glassCount; i++)
                DestroyGlass(i);
            //Do other misc things
            foreach (Entity e in AIZ.getAllEntitiesWithName("dynamic_model"))
            {
                string model = e.Model;
                if (model.StartsWith("fence_tarp_"))
                {
                    Vector3 origin = e.Origin;
                    Vector3 angles = e.Angles;
                    Vector3 forward = AnglesToForward(angles);
                    Vector3 up = AnglesToUp(angles);
                    Entity fire = SpawnFX(AIZ.fx_smallFire, origin, forward, up);
                    TriggerFX(fire);
                    AfterDelay(3000, () =>
                    {
                        fire.Delete();
                        e.ScriptModelClearAnim();
                        e.Hide();
                    });
                }
                //Used in EE
                //else if (model == "machinery_windmill")
                //{
                    //e.Call("rotateroll", 80, 2, .25f, .1f);
                    //e.AfterDelay(1000, (ent) => ent.Call("scriptmodelclearanim"));
                //}
                else if (model.Contains("foliage"))
                    e.Origin -= new Vector3(0, 0, 30);
                else if (model.Contains("oil_pump_jack"))
                    e.ScriptModelClearAnim();
                else if (model == "accessories_windsock_large")
                {
                    e.ScriptModelClearAnim();
                    e.Origin += new Vector3(0, 0, 20);
                    Entity bounds = Spawn("script_model", e.Origin + new Vector3(15, -7, 0));
                    Entity bound2 = Spawn("script_model", e.Origin + new Vector3(70, -38, 0));
                    bounds.SetModel("com_plasticcase_dummy");
                    bound2.SetModel("com_plasticcase_dummy");
                    bounds.Hide();
                    bound2.Hide();
                    bounds.CloneBrushModelToScriptModel(mapEdit._airdropCollision);
                    bound2.CloneBrushModelToScriptModel(mapEdit._airdropCollision);
                    bounds.Angles = e.Angles + new Vector3(0, 90, 0);
                    bound2.Angles = bounds.Angles;
                    bounds.EnableLinkTo();
                    e.LinkTo(bounds);
                    bound2.LinkTo(bounds);
                    Vector3 launchImpulse = new Vector3(-400, -250, 10);
                    bounds.PhysicsLaunchServer(new Vector3(0, 0, 0), launchImpulse);
                }
            }
        }

        private static void decrementDefconLevel(Entity player)
        {
            player.SetField("ownsMapStreak", false);
            shuffleStreaks(player);

            int currentDefcon = level.GetField<int>("defcon");
            currentDefcon--;
            Announcement("Defcon is at level " + currentDefcon);

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
                //doDefconDrop(player);
                AfterDelay(3000, () => AIZ.teamSplash("callout_top_gun_rank", player));//callout_earned_carepackage
            }
            else
            {
                PlaySoundAtPos(Vector3.Zero, "mp_defcon_down");
                AIZ.teamSplash("changed_defcon", player);
            }

            //AfterDelay(1000, () => level.PlaySound("US_1mc_defcon_" + currentDefcon));

            level.SetField("defcon", currentDefcon);
        }
        private static void doDefconDrop(Entity player)
        {
            
        }

        private static void spawnTanks(Entity player)
        {
            AIZ.teamSplash("used_remote_tank", player);

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

            AfterDelay(8050, () =>//Tank1 arrives
            {
                tank1Turret.Unlink();
                tank1Turret.Origin = tank1.GetField<Vector3>("perchPos");
                tank_searchForTargets(tank1);
                tank_timer(tank1);
            });
            AfterDelay(10050, () =>//Tank2 arrives
            {
                tank2Turret.Unlink();
                tank2Turret.Origin = tank2.GetField<Vector3>("perchPos");
                tank_searchForTargets(tank2);
                tank_timer(tank2);
            });
            AfterDelay(6050, () =>//Tank3 arrives
            {
                tank3Turret.Unlink();
                tank3Turret.Origin = tank3.GetField<Vector3>("perchPos");
                tank_searchForTargets(tank3);
                tank_timer(tank3);
            });
        }
        private static void tank_timer(Entity tank)
        {
            AfterDelay(45000 + AIZ.rng.Next(5000), () =>
            {
                if (tank.GetField<bool>("isEngaging"))
                {
                    OnInterval(500, () =>
                    {
                        if (tank.GetField<bool>("isEngaging")) return true;
                        else
                        {
                            tank.SetField("destroyed", true);
                            tank_flee(tank);
                            return false;
                        }
                    });
                }
                else
                {
                    //Log.Debug("Destroying a tank");
                    tank.SetField("destroyed", true);
                    //tank_destroyTank(tank);
                    tank_flee(tank);
                }
            });
        }
        private static void tank_searchForTargets(Entity tank)
        {
            OnInterval(10000, () =>
            {
                if (tank.GetField<bool>("destroyed")) return false;

                foreach (Entity bot in botUtil.botsInPlay)
                {
                    if (!bot.GetField<bool>("isAlive")) continue;

                    Vector3 eye = tank.GetField<Vector3>("eyePos");
                    Entity botHitbox = bot.GetField<Entity>("hitbox");

                    bool trace = SightTracePassed(eye, botHitbox.Origin + 30, false);
                    if (!trace) continue;

                    tank_engageTarget(tank, botHitbox);
                    break;
                }
                return true;
            });
        }
        private static void tank_engageTarget(Entity tank, Entity target)
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
            AfterDelay(2050, () =>
            {
                tankTurret.LinkTo(tank, "body_animate_jnt", new Vector3(0, 0, -44), new Vector3(0, yaw - localYaw, 0));
                tank.FireWeapon("origin_animate_jnt", tank, Vector3.Zero);
                tankTurret.Angles = targetAngles;
                Vector3 start = tankTurret.GetTagOrigin("tag_flash");
                Vector3 angles = AnglesToForward(tankTurret.Angles);
                Entity missile = MagicBullet(tank.GetField<string>("weapon"), start, tankTurret.Origin + angles * 7000, tank);
                missile.SetTargetEnt(target);
                missile.SetFlightModeTop();
                tank.SetField("isEngaging", false);
            });
        }
        private static void tank_flee(Entity tank)
        {
            Entity missile = MagicBullet("remote_mortar_missile_mp", tank.GetField<Vector3>("perchPos") + new Vector3(0, 0, 5000), tank.GetField<Vector3>("perchPos"));
            missile.OnNotify("explode", (m, pos) =>
            {
                if (Utilities.isEntDefined(tank) && tank.TargetName == "vehicle_tank")
                    tank_destroyTank(tank);
            });
        }
        private static void tank_destroyTank(Entity tank)
        {
            //tank.PlaySound("");
            PlayFX(AIZ.fx_tankExplode, tank.Origin);
            tank.GetField<Entity>("turret").Delete();
            tank.ClearField("turret");
            tank.FreeVehicle();
            tank.Delete();
        }
        */
        #endregion
    }
}
