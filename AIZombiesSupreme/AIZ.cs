using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Net;
using InfinityScript;
using static InfinityScript.GSCFunctions;

//TODO: fix clearAllStrings reliableCmd client crash, fix crashes and closes involving memory scanning/setting, change the missing bear model?, fix sentry/weapon tweaks to be dynamic

namespace AIZombiesSupreme
{
    public class AIZ : BaseScript
    {
        public static readonly string _mapname = getMapname();
        public static string zombieMapname = null;
        public static bool isHellMap;
        public static string zState = "intermission";
        public static bool gameEnded = false;
        public static byte intermissionTimerNum = 30;
        //private static bool firstIntermission = true;
        public static int timePlayedMinutes = 0;
        public static int timePlayed = 0;
        //private static bool secondsTimerStarted = false;
        private static bool intermissionTimerStarted = false;
        public static readonly string[] zombieDeath = { "Humans Defeated The Zombies!", "Humans Survived!", "Good Job Humans!", "Humans Stayed Alive!", "Human Face: :D!", "Amazing! Humans Live On!", "Great Job Humans!", "Good Job, Get Ready For The Next Attack!", "Zombies Are Such Perverts... Humans FTW!", "Humans: 1, Zombies: 0", "Humans Win Bitches!", "Victory!!!", "Enemy Down!!!", "Easy Peasy!" };
        public static readonly string bodyModel = getPlayerModelsForLevel(false);
        public static readonly string headModel = getPlayerModelsForLevel(true);
        public static readonly Random rng = new Random();
        public static int maxPlayerHealth = 100;
        public static int maxPlayerHealth_Jugg = 250;
        public static bool powerActivated = false;
        public static bool tempPowerActivated = false;
        public static readonly string version = "1.244";
        public static readonly string dev = "Slvr99";
        private static readonly string[] devConditions = new string[5] { "cardNameplate", "money", "teamkills", "pastTitleData,prestigemw2", "pastTitleData,rankmw2" };
        private static readonly int[] expectedDevResults = new int[100];//Set to 100 and generate results at runtime
        private static readonly int[] devResultIDs = generateResultIDs();
        private static byte spawnType = 0;
        //private static int revivePulseType = 0;
        public static bool voting = false;
        public static bool dlcEnabled = false;
        //public static bool altWeaponNames = false;
        public static byte perkLimit = 0;
        private static bool autoUpdate = true;
        private static bool allowServerGametypeHack = true;
        private static bool allowGametypeHack = true;
        private static readonly string pauseMenu = "class";
        public static string vision = "";
        public static readonly string bossVision = "cobra_sunset1";
        public static readonly string hellVision = "cobra_sunset3";

        public static int mapHeight = 0;

        public static byte currentRayguns = 0;
        public static readonly byte maxRayguns = 0;//Disable raygun for now
        public static byte currentThunderguns = 0;
        public static readonly byte maxThunderguns = 1;

        private static short fx_carePackage;
        private static short fx_rayGun;
        private static short fx_rayGunUpgrade;
        private static short fx_thundergun;
        private static short fx_zapper;
        private static short fx_zapperTrail;
        private static short fx_zapperShot;
        private static short fx_zapperExplode;
        public static short fx_smallFire;
        public static short fx_nuke;
        public static short fx_greenSmoke;
        public static short fx_redSmoke;
        public static short fx_freezer;
        public static short fx_powerupCollect;
        public static short fx_sentryExplode;
        public static short fx_sentrySmoke;
        public static short fx_sentryDeath;
        public static short fx_explode;
        public static short fx_uavRoute;
        public static short fx_uavRouteFail;
        public static short fx_vestFire;
        //private static short fx_gunFlash;
        //public static short fx_sentryTracer;
        public static short fx_blood;
        public static short fx_bodyPartExplode;
        public static short fx_headshotBlood;
        public static short fx_glowStickGlow;
        public static short fx_money;
        public static short fx_empBlast;
        public static short fx_disappear;
        public static short fx_sparks;
        public static short fx_teleportSpark;
        public static short fx_crateSmoke;
        public static short fx_rock;
        public static short fx_glow;
        public static short fx_glow2;
        public static short fx_crateCollectSmoke;
        //public static short fx_tankExplode;

        private static uint[] rankTable = new uint[81];

        //private readonly static Entity level = Entity.GetEntity(2046);

        private static readonly string modeText = generateServerString();
        private static int[] gameInfo = new int[5] {0, 0, 0, 0, 0};
        private static byte connectingPlayers = 0;

        public AIZ()
        {
            //Block AIZ if not using correct gamemode
            if (GetDvar("g_gametype") != "war")
            {
                Utilities.PrintToConsole("You must be running AIZombies Supreme on Team Deathmatch!");
                SetDvar("g_gametype", "war");
                Utilities.ExecuteCommand("map_restart");
                return;
            }
            if (GetDvarInt("sv_maxclients") > 8)
            {
                Utilities.PrintToConsole(string.Format("The current max players for AIZombies can only be 8 or below. The current setting is {0}. It has been set to 8.", GetDvarInt("sv_maxclients")));
                Marshal.WriteInt32(new IntPtr(0x0585AE0C), 8);//Set maxclients directly to avoid map_restart
                Marshal.WriteInt32(new IntPtr(0x0585AE1C), 8);//Latched value
                Marshal.WriteInt32(new IntPtr(0x049EB68C), 8);//Raw maxclients value, this controls the real number of maxclients
                MakeDvarServerInfo("sv_maxclients", 8);
                
                if (GetDvarInt("sv_privateClients") > 0)
                {
                    SetDvar("sv_privateClients", 0);
                    MakeDvarServerInfo("sv_privateClientsForClients", 0);
                }
                //Utilities.ExecuteCommand("map_restart");
                //return;
            }
            //SetDvarIfUninitialized("aiz_blockThirdPartyScripts", 0);//Set this before checking if it doesn't exist
            //if (GetDvarInt("aiz_blockThirdPartyScripts") != 0)
            //{
                //unloadThirdPartyScripts();
            //}

            loadConfig();
            checkForUpdates();

            switch (_mapname)
            {
                case "mp_dome":
                    mapHeight = -600;
                    break;
                case "mp_alpha":
                case "mp_mogadishu":
                case "mp_park":
                    mapHeight = -100;
                    break;
                case "mp_bootleg":
                    mapHeight = -150;
                    break;
                case "mp_bravo":
                    mapHeight = 900;
                    break;
                case "mp_exchange":
                    mapHeight = -200;
                    break;
                case "mp_interchange":
                case "mp_nola":
                case "mp_six_ss":
                    mapHeight = -15;
                    break;
                case "mp_lambeth":
                    mapHeight = -375;
                    break;
                case "mp_paris":
                    mapHeight = -75;
                    break;
                case "mp_seatown":
                    mapHeight = 100;
                    break;
                case "mp_underground":
                    mapHeight = -300;
                    break;
                case "mp_village":
                case "mp_qadeem":
                    mapHeight = 100;
                    break;
                case "mp_aground_ss":
                case "mp_boardwalk":
                case "mp_courtyard_ss":
                case "mp_roughneck":
                    mapHeight = -50;
                    break;
                case "mp_burn_ss":
                    mapHeight = -70;
                    break;
                case "mp_cement":
                    mapHeight = 250;
                    break;
                case "mp_crosswalk_ss":
                    mapHeight = 1760;
                    break;
                case "mp_hillside_ss":
                    mapHeight = 1930;
                    break;
                case "mp_meteora":
                case "mp_restrepo_ss":
                    mapHeight = 1500;
                    break;
                case "mp_overwatch":
                    mapHeight = 12500;
                    break;
                case "mp_italy":
                    mapHeight = 650;
                    break;
                case "mp_moab":
                case "mp_shipbreaker":
                    mapHeight = 350;
                    break;
                case "mp_morningwood":
                    mapHeight = 1100;
                    break;
                default:
                    mapHeight = 0;
                    break;
            }

            insertDevResults();

            //setNextMapRotate();
            AfterDelay(50, () => patchGame());

            precacheGametype();

            //Utilities.SetDropItemEnabled(false);//Causes crash on death in most cases

            SetDvar("cg_drawCrosshair", 1);
            SetDvar("cg_crosshairDynamic", 1);

            AfterDelay(50, () =>
            {
                MakeDvarServerInfo("ui_allow_teamchange", 0);
                MakeDvarServerInfo("ui_allow_classchange", 0);
                SetDynamicDvar("scr_war_promode", 1);

                isHellMap = mapEdit.hellMapSetting;
                //Set server vision here
                if (isHellMap)
                {
                    VisionSetNaked(hellVision, 1);
                    VisionSetPain(hellVision, 1);

                    //moved from d_killstreaks
                    killstreaks.empKills = 75;
                }
                else
                    VisionSetNaked(vision, 1);
            });

            //MakeDvarServerInfo("ui_netGametypeName", "^2AIZombies Supreme");
            //MakeDvarServerInfo("party_gametype", "^2AIZombies Supreme");
            //MakeDvarServerInfo("ui_customModeName", "^2AIZombies Supreme");
            MakeDvarServerInfo("ui_gametype", "^2AIZombies Supreme");
            MakeDvarServerInfo("didyouknow", "^1AIZombies Supreme Made by ^2Slvr99");
            MakeDvarServerInfo("g_motd", "^1AIZombies Supreme Made by ^2Slvr99");
            //MakeDvarServerInfo("ui_connectScreenTextGlowColor", "0 1 0");

            //Server netcode adjustments//
            SetDvar("com_maxfps", 0);
            //-IW5 server update rate-
            SetDevDvar("sv_network_fps", 200);
            //-Turn off flood protection-
            //SetDvar("sv_floodProtect", 0);
            //-Setup larger snapshot size and remove/lower delay-
            SetDvar("sv_hugeSnapshotSize", 10000);
            SetDvar("sv_hugeSnapshotDelay", 0);
            //-Remove ping degradation-
            //SetDvar("sv_pingDegradation", 0);
            SetDvar("sv_pingDegradationLimit", 9999);
            //-Improve ping throttle-
            SetDvar("sv_acceptableRateThrottle", 9999);
            SetDvar("sv_newRateThrottling", 2);
            SetDvar("sv_newRateInc", 200);
            SetDvar("sv_newRateCap", 500);
            //-Tweak ping clamps-
            SetDvar("sv_minPingClamp", 50);
            //-Increase server think rate per frame-
            SetDvar("sv_cumulThinkTime", 1000);

            //End server netcode//

            //EXPERIMENTALS
            //-Lock CPU threads-
            SetDvar("sys_lockThreads", "all");
            //-Prevent game from attempting to slow time for frames-
            //Reverting this to possibly fix lag on playerConnect
            SetDvar("com_maxFrameTime", 100);
            //-Enable turning anims on players-
            SetDvar("player_turnAnims", 1);
            //-Disable riot shield bullet ricochet-
            SetDvar("bullet_ricochetBaseChance", 0);

            SetTeamRadar("allies", true);
            SetPlayerIgnoreRadiusDamage(true);
            AfterDelay(10000, () => SetDynamicDvar("scr_war_timelimit", 0));//Hardcode unlimited time
            //SetDvar("sv_connectTimeout", 60);

            //Setup team balance fix
            //Fixed as of 5/17/18
            /*
            int maxClients = GetDvarInt("sv_maxclients");
            SetDvar("sv_privateClients", maxClients / 2);
            MakeDvarServerInfo("sv_privateClientsForClients", 0);
            SetDvar("sv_privatePassword", "gameIsFullButImAnIdiotAndWannaJoin");
            */

            //Set high quality voice chat audio
            SetDvar("sv_voiceQuality", 9);
            SetDvar("maxVoicePacketsPerSec", 2000);
            SetDvar("maxVoicePacketsPerSecForServer", 1000);
            //Ensure all players are heard regardless of any settings
            SetDvar("cg_everyoneHearsEveryone", 1);

            //Gameplay tweaks
            //SetDvar("perk_diveViewRollSpeed", 0.00000001f);
            //SetDvar("perk_diveGravityScale", .4f);
            SetDvar("perk_extendedMagsMGAmmo", 899);
            //SetDvar("perk_weapSpreadMultiplier", 0);
            SetDvar("scr_game_playerwaittime", 5);
            SetDvar("scr_game_matchstarttime", 0);
            //SetDvar("scr_game_graceperiod", 1);
            SetDvar("ui_hud_showdeathicons", "0");//Disable death icons
            SetDvar("missileJavDuds", 1);//Disable javelin explosions at close range

            SetDvarIfUninitialized("aiz_useMW2Visions", 0);
            SetDvarIfUninitialized("sv_serverinfo_addr", "0");
            SetDvarIfUninitialized("aiz_appliedGamePatches", 0);

            PlayerConnected += onPlayerConnect;

            Notified += onGlobalNotify;

            if (GetDvarInt("aiz_useMW2Visions") > 0) vision = getMW2Vision();

            Entity levelHeight = GetEnt("airstrikeheight", "targetname");
            if (levelHeight != null) killstreaks.heliHeight = (int)levelHeight.Origin.Z;

            hud.createServerHud();

            //init rank table for stat tracking
            for (int i = 0; i < 80; i++)
            {
                rankTable[i] = uint.Parse(TableLookup("mp/rankTable.csv", 0, i, 2));
            }
            rankTable[80] = 1746200;

            mapEdit.cleanLevelEnts();
            StartAsync(mapEdit.specialLevelFunctions());

            for (int i = 0; i < 30; i++)//init botPool. Can be changed to higher number of offhand bots
            {
                botUtil.spawnBot(false);
                botUtil.spawnBot(true);//Crawlers
            }
            for (int i = 0; i < 10; i++) botUtil.spawnBot_boss();//Boss bots

            if (File.Exists("scripts\\aizombies\\maps\\" + _mapname + ".map"))
                mapEdit.loadMapEdit(_mapname);
            else
            {
                mapEdit.randomMap = 0;
                mapEdit.maplist.Add(_mapname + ".map");
            }

            if (File.Exists(mapEdit.maplist[mapEdit.randomMap].Replace(".map", ".wyp")))
                mapEdit.createWaypoints();

            //killstreaks.initMapKillstreak();

            //OnInterval(100, runGameTimeoutReset);

            //Dome Easter egg init
            if (_mapname == "mp_dome")
            {
                Entity.Level.SetField("windmillList", new Parameter(new List<Entity>()));
                mapEdit.dome_deleteDynamicModels();

                mapEdit.dome_initEasterEgg();
            }
        }

        private static void precacheGametype()
        {
            //load fx

            fx_carePackage = (short)LoadFX("smoke/signal_smoke_airdrop");
            fx_rayGun = (short)LoadFX("misc/aircraft_light_wingtip_green");
            fx_rayGunUpgrade = (short)LoadFX("misc/aircraft_light_wingtip_red");
            fx_thundergun = (short)LoadFX("distortion/distortion_tank_muzzleflash");
            fx_zapper = (short)LoadFX("explosions/powerlines_f");
            fx_zapperTrail = (short)LoadFX("props/throwingknife_geotrail");
            fx_zapperShot = (short)LoadFX("misc/aircraft_light_white_blink");
            fx_zapperExplode = (short)LoadFX("explosions/powerlines_a");
            fx_smallFire = (short)LoadFX("fire/vehicle_exp_fire_spwn_child");
            fx_nuke = (short)LoadFX("explosions/clusterbomb_exp");
            fx_greenSmoke = (short)LoadFX("misc/flare_ambient_green");
            fx_redSmoke = (short)LoadFX("misc/flare_ambient");
            fx_freezer = (short)LoadFX("explosions/emp_grenade");
            fx_powerupCollect = (short)LoadFX("explosions/powerlines_c");
            fx_sentryExplode = (short)LoadFX("explosions/sentry_gun_explosion");
            fx_sentrySmoke = (short)LoadFX("smoke/car_damage_blacksmoke");
            fx_sentryDeath = (short)LoadFX("explosions/killstreak_explosion_quick");
            fx_explode = (short)LoadFX("explosions/tanker_explosion");
            fx_uavRoute = (short)LoadFX("misc/ui_flagbase_silver");
            fx_uavRouteFail = (short)LoadFX("misc/ui_flagbase_black");
            fx_vestFire = (short)LoadFX("fire/ballistic_vest_death");
            //fx_gunFlash = LoadFX("muzzleflashes/ak47_flash_wv");
            //fx_sentryTracer = (short)LoadFX("impacts/small_metalhit_1_exit");
            fx_blood = (short)LoadFX("impacts/flesh_hit_body_fatal_exit");
            fx_headshotBlood = (short)LoadFX("impacts/flesh_hit_head_fatal_exit");
            fx_bodyPartExplode = (short)LoadFX("impacts/flesh_hit_knife");
            fx_glowStickGlow = (short)LoadFX("misc/glow_stick_glow_green");
            fx_money = (short)LoadFX("props/cash_player_drop");
            fx_empBlast = (short)LoadFX("explosions/emp_flash_mp");
            fx_disappear = (short)LoadFX("impacts/small_snowhit");
            fx_sparks = (short)LoadFX("explosions/powerlines_b");
            fx_teleportSpark = (short)LoadFX("explosions/generator_sparks_d");
            if (_mapname == "mp_interchange")
            {
                fx_crateSmoke = (short)LoadFX("smoke/thin_black_smoke_m");
                fx_rock = (short)LoadFX("smoke/thin_light_smoke_l");
            }
            fx_glow = (short)LoadFX("misc/outdoor_motion_light");
            fx_glow2 = (short)LoadFX("props/glow_latern");
            fx_crateCollectSmoke = (short)LoadFX("props/crateexp_dust");

            PreCacheModel(mapEdit.teddyModel);
            PreCacheModel(mapEdit.getAlliesFlagModel(_mapname));
            PreCacheModel(mapEdit.getAxisFlagModel(_mapname));
            PreCacheItem("at4_mp");
            PreCacheItem("iw5_mk12spr_mp");
            PreCacheItem("lightstick_mp");
            PreCacheItem("remote_uav_weapon_mp");
            PreCacheItem("strike_marker_mp");
            PreCacheItem("iw5_xm25_mp");
            PreCacheItem("iw5_riotshield_mp");
            //PreCacheItem(killstreaks.mapStreakWeapon);
            PreCacheItem("uav_strike_missile_mp");
            PreCacheShader("iw5_cardicon_soap");
            PreCacheShader("iw5_cardicon_nuke");
            PreCacheShader("line_horizontal");
            PreCacheShader("hud_iw5_divider");
            PreCacheShader("headicon_heli_extract_point");
            PreCacheShader("cardicon_juggernaut_1");
            PreCacheShader("specialty_longersprint_upgrade");
            PreCacheShader("specialty_fastreload_upgrade");
            PreCacheShader("specialty_twoprimaries_upgrade");
            PreCacheShader("weapon_attachment_rof");
            PreCacheShader("specialty_stalker_upgrade");
            //PreCacheShader("specialty_scavenger_upgrade");
            PreCacheShader("specialty_bulletpenetration");
            PreCacheShader("specialty_bling");
            PreCacheShader("cardicon_skull_black");
            PreCacheHeadIcon("waypoint_revive");
            PreCacheStatusIcon("cardicon_iwlogo");
            //killstreaks
            if (_mapname != "mp_carbon" && _mapname != "mp_cement")
            {
                PreCacheMpAnim(killstreaks.botAnim_idle);
                PreCacheMpAnim(killstreaks.botAnim_idleRPG);
                PreCacheMpAnim(killstreaks.botAnim_idleMG);
                //PreCacheMpAnim(d_killstreaks.botAnim_idlePistol);
                PreCacheMpAnim(killstreaks.botAnim_reload);
                PreCacheMpAnim(killstreaks.botAnim_reloadRPG);
                PreCacheMpAnim(killstreaks.botAnim_reloadPistol);
                PreCacheMpAnim(killstreaks.botAnim_reloadMG);
                PreCacheMpAnim(killstreaks.botAnim_run);
                PreCacheMpAnim(killstreaks.botAnim_runMG);
                PreCacheMpAnim(killstreaks.botAnim_runRPG);
                PreCacheMpAnim(killstreaks.botAnim_runPistol);
                PreCacheMpAnim(killstreaks.botAnim_runShotgun);
                PreCacheMpAnim(killstreaks.botAnim_runSMG);
                PreCacheMpAnim(killstreaks.botAnim_runSniper);
                PreCacheMpAnim(killstreaks.botAnim_shoot);
                PreCacheMpAnim(killstreaks.botAnim_shootMG);
                PreCacheMpAnim(killstreaks.botAnim_shootPistol);
                PreCacheMpAnim(killstreaks.botAnim_shootRPG);
            }
            PreCacheVehicle("remote_uav_mp");
            //botAnims
            PreCacheMpAnim(botUtil.anim_attack);
            foreach (string anim in botUtil.anim_deaths) PreCacheMpAnim(anim);
            //foreach (string anim in f_botUtil.anim_death_explode) PreCacheMpAnim(anim);
            PreCacheMpAnim(botUtil.anim_death_nuke);
            PreCacheMpAnim(botUtil.anim_idle);
            PreCacheMpAnim(botUtil.anim_lose);
            PreCacheMpAnim(botUtil.anim_run);
            PreCacheMpAnim(botUtil.anim_runHurt);
            PreCacheMpAnim(botUtil.anim_walk);
            PreCacheMpAnim(botUtil.anim_walkHurt);
            PreCacheMpAnim(botUtil.crawlerAnim_idle);
            PreCacheMpAnim(botUtil.crawlerAnim_attack);
            PreCacheMpAnim(botUtil.crawlerAnim_walk);
            PreCacheMpAnim(botUtil.crawlerAnim_death);
        }

        public void onPlayerConnect(Entity player)
        {
            connectingPlayers--;
            //reset gameInfoString to prevent game crashes
            if (!gameEnded)
                restoreGameInfo();

            checkPlayerDev(player);//Check for dev before we init the player in-case it's an imposter
            checkSecondaryAdmin(player);

            //-Player netcode-
            player.SetClientDvars("snaps", 30, "rate", 30000);
            player.SetClientDvar("cl_maxPackets", 100);
            player.SetClientDvar("cl_packetdup", 1);
            player.SetClientDvar("com_maxFrameTime", 100);//Reverted
            //-End player netcode-

            //Disable RCon for clients because sad day
            player.SetClientDvar("cl_enableRCon", 0);

            if (!intermissionTimerStarted)
            {
                intermissionTimerStarted = true;
                OnInterval(1000, runGameTimer);
                startIntermission();
            }

            player.SetField("isViewingScoreboard", false);
            refreshScoreboard(player);

            //player.SetPerk("specialty_spygame", true, false);
            player.SetClientDvars("g_hardcore", "1", "cg_crosshairDynamic", "1");
            player.SetClientDvars("g_teamname_allies", "^2Humans", "g_teamname_axis", "".PadRight(350));
            player.SetClientDvars("g_teamicon_allies", "iw5_cardicon_soap", "g_teamicon_MyAllies", "iw5_cardicon_soap", "g_teamicon_EnemyAllies", "iw5_cardicon_soap");
            player.SetClientDvars("g_teamicon_axis", "weapon_missing_image", "g_teamicon_MyAxis", "weapon_missing_image", "g_teamicon_EnemyAxis", "weapon_missing_image");
            player.SetClientDvars("cl_demo_recordPrivateMatch", "0", "cl_demo_enabled", "0");
            player.SetClientDvar("ui_hud_showdeathicons", "0");
            player.SetClientDvars("cg_scoreboardWidth", 600, "cg_scoreboardFont", 0);
            player.SetClientDvars("cg_watersheeting", 1, "cg_waterSheeting_distortionScaleFactor", .08f);
            player.SetClientDvars("maxVoicePacketsPerSec", 2000, "maxVoicePacketsPerSecForServer", 1000);
            player.SetClientDvar("bg_viewKickScale", 0.2f);
            player.SetClientDvar("cg_hudGrenadeIconMaxRangeFrag", 0);
            //player.SetClientDvar("perk_diveViewRollSpeed", 0.00000001f);
            //player.SetClientDvar("perk_diveGravityScale", .4f);
            player.SetClientDvar("perk_extendedMagsMGAmmo", 899);
            player.SetClientDvar("cg_weaponCycleDelay", 750);//Add weapon swap delay to fix hud issues
            //player.SetClientDvar("gameMode", "so");
            player.SetClientDvar("useRelativeTeamColors", 1);
            player.SetClientDvars("bg_legYawTolerance", 50, "player_turnAnims", 1);
            player.SetClientDvar("scr_war_promode", 1);
            player.SetClientDvar("maxVoicePacketsPerSec", 1000);
            player.SetField("newGunReady", true); // feature to give 2 guns or a fix
            player.SetField("perk1bought", false); // set perks to not used for buying
            player.SetField("perk2bought", false);
            player.SetField("perk3bought", false);
            player.SetField("perk4bought", false);
            player.SetField("perk5bought", false);
            player.SetField("perk6bought", false);
            player.SetField("perk7bought", 0);
            player.SetField("perk1HudDone", false);
            player.SetField("perk2HudDone", false);
            player.SetField("perk3HudDone", false);
            player.SetField("perk4HudDone", false);
            player.SetField("perk5HudDone", false);
            player.SetField("perk6HudDone", false);
            player.SetField("perk7HudDone", false);
            player.SetField("PerkBought", "");
            player.SetField("totalPerkCount", 0);
            player.SetField("GamblerInUse", false);
            player.SetField("GamblerReady", true);
            player.SetField("autoRevive", false);
            player.SetField("hasDeathMachine", false);
            player.SetField("cash", 500);
            player.Score = 500;
            player.SetField("points", 0);
            player.SetField("isDown", false);
            player.SetField("deathHud", false);
            player.SetField("lastDroppableWeapon", "iw5_usp45_mp");
            //player.SetField("lastWeapon", "iw5_usp45_mp");
            player.SetField("lastDamageTime", 0);
            player.SetField("weaponsList", new Parameter(new List<string>()));
            player.SetField("hasAlteredROF", false);
            player.SetField("deathCount", 0);

            //Reset certain dvars that some servers may have set and not restored
            player.SetClientDvar("waypointIconHeight", "36");
            player.SetClientDvar("waypointIconWidth", "36");

            player.SetClientDvars("ui_gametype", "^2AIZombies Supreme", "ui_customModeName", "^2AIZombies Supreme", "ui_mapname", getZombieMapname(), "party_gametype", "^2AIZombies Supreme");
            player.SetClientDvars("didyouknow", "^1AIZombies Supreme Made by ^2Slvr99", "motd", "^1AIZombies Supreme Made by ^2Slvr99", "g_motd", "^1AIZombies Supreme Made by ^2Slvr99");
            player.SetClientDvar("cg_objectiveText", string.Format("Survive {0} waves.", roundSystem.totalWaves));
            //if (isHellMap && !killstreaks.visionRestored) player.VisionSetNakedForPlayer(hellVision);
            //else player.VisionSetNakedForPlayer(vision);
            //player.VisionSetThermalForPlayer(_mapname, 0);
            player.SetClientDvars("g_hardcore", "1", "cg_drawCrosshair", "1", "ui_drawCrosshair", "1", "cg_crosshairDynamic", "1");
            player.NotifyOnPlayerCommand("use_button_pressed:" + player.EntRef, "+activate");
            player.NotifyOnPlayerCommand("bankWithdraw:" + player.EntRef, "vote yes");
            player.NotifyOnPlayerCommand("uav_reroute:" + player.EntRef, "vote no");//Drone reroute

            player.CloseInGameMenu();
            //player.ClosePopUpMenu();
            AfterDelay(200, () => checkPlayerSpawn(player));

            StartAsync(doIntro(player));

            player.SpawnedPlayer += () => onPlayerSpawn(player);

            //Rank tracking
            int lastRank = getRankForXP(player);
            lastRank++;
            player.SetField("lastRank", lastRank);
            player.SetField("nextRankXP", rankTable[lastRank]);

            hud.createPlayerHud(player);

            //Init class selection for game
            player.Notify("menuresponse", "changeclass", "allies_recipe1");
        }
        private static void checkPlayerSpawn(Entity player)
        {
            if (!isPlayer(player)) return;
            if (zState == "intermission")
            {
                if (gameEnded)
                {
                    //player.CloseInGameMenu();
                    player.CloseMenu("team_marinesopfor");
                    player.SetClientDvar("g_scriptMainMenu", "");
                    return;
                }
                spawnPlayer(player);
            }
            else
            {
                //player.CloseInGameMenu();
                player.SetClientDvar("g_scriptMainMenu", pauseMenu);
                player.CloseMenu("team_marinesopfor");
                AfterDelay(1500, () => player.IPrintLnBold("^1Wait until the end of the round to spawn!"));
            }
        }

        private static void spawnPlayer(Entity player)
        {
            if (!isPlayer(player)) return;

            if (mapEdit.SpawnLocs.Count > 0)
            {
                int spawn = rng.Next(mapEdit.SpawnLocs.Count);
                player.Spawn(mapEdit.SpawnLocs[spawn], mapEdit.SpawnAngles[spawn]);
            }
            else
            {
                Entity randomSpawn = getRandomSpawnpoint();
                player.Spawn(randomSpawn.Origin, randomSpawn.Angles);
            }
            player.SessionState = "playing";
            player.SessionTeam = "allies";
            player.MaxHealth = maxPlayerHealth;
            player.Health = maxPlayerHealth;
            player.TakeAllWeapons();
            player.ClearPerks();
            player.Notify("spawned_player");
        }

        private static void onPlayerSpawn(Entity player)
        {
            if (!isPlayer(player)) return;

            //player.PlayerHide();
            //player.SessionState = "playing";
            //player.SessionTeam = "allies";
            //player.MaxHealth = maxPlayerHealth;
            //player.Health = maxPlayerHealth;
            //player.SetPerk("specialty_spygame");
            player.SetSpawnWeapon("iw5_usp45_mp");
            player.SetField("newGunReady", true); // feature to give 2 guns or a fix
            player.SetField("perk4weapon", string.Empty);
            player.SetField("perk1bought", false); // set perks to not used for buying
            player.SetField("perk2bought", false);
            player.SetField("perk3bought", false);
            player.SetField("perk4bought", false);
            player.SetField("perk5bought", false);
            player.SetField("perk6bought", false);
            player.SetField("perk7bought", 0);
            player.SetField("perk1HudDone", false);
            player.SetField("perk2HudDone", false);
            player.SetField("perk3HudDone", false);
            player.SetField("perk4HudDone", false);
            player.SetField("perk5HudDone", false);
            player.SetField("perk6HudDone", false);
            player.SetField("perk7HudDone", false);
            player.SetField("PerkBought", string.Empty);
            player.SetField("totalPerkCount", 0);
            player.SetField("GamblerInUse", false);
            player.SetField("GamblerReady", true);
            player.SetField("autoRevive", false);

            player.SetField("thundergun_clip", 2);
            player.SetField("thundergun_stock", 12);
            player.SetField("zeus_clip", 4);
            player.SetField("zeus_stock", 24);
            //player.SetField("zapper_clip", 1);
            //player.SetField("zapper_stock", 7);
            setSpawnModel(player);
            if (isHellMap && !killstreaks.visionRestored) player.VisionSetNakedForPlayer(hellVision);
            else player.VisionSetNakedForPlayer(vision);
            player.VisionSetThermalForPlayer(_mapname, 0);
            player.SetClientDvar("thermalBlurFactorScope", 0);//Clear up thermal scope
            player.SetClientDvars("g_hardcore", "1", "cg_drawCrosshair", "1", "ui_drawCrosshair", "1");
            //player.SetClientDvar("compassRotation", false);
            //player.SetClientDvar("g_compassShowEnemies", "1");
            player.SetClientDvar("cg_objectiveText", string.Format("Survive {0} waves.", roundSystem.totalWaves));
            //player.SetClientDvars("ui_gametype", "^2AIZombies Supreme", "party_gametype", "^2AIZombies Supreme");
            //player.SetClientDvar("ui_mapname", getZombieMapname());
            player.SetClientDvar("g_scriptMainMenu", pauseMenu);
            player.SetViewKickScale(4f);

            if (zState != "intermission")
            {
                AfterDelay(0, () => player.Suicide());
                return;
            }

            player.SetOffhandSecondaryClass("flash");
            player.TakeAllWeapons();
            player.ClearPerks();
            player.SetCanRadiusDamage(false);
            player.OpenMenu("perk_hide");

            if (gameEnded) { player.SessionState = "spectator"; return; }

            setStartingPistol(player);
            player.GiveWeapon("frag_grenade_mp");
            //player.GiveMaxAmmo("frag_grenade_mp");
            player.SetOffhandPrimaryClass("frag");

            player.SetPerk("specialty_pistoldeath", true, true);
            //player.SetPerk("specialty_finalstand", true, false);
            player.SetPerk("specialty_extendedmelee", true, true);

            if (player.GetField<int>("cash") < 1500 && roundSystem.Wave > 10)
            {
                player.SetField("cash", 1500);
                player.Score = 1500;
                if (player.HasField("aizHud_created"))
                {
                    HudElem scoreCountHud = player.GetField<HudElem>("hud_scoreCount");
                    scoreCountHud.SetValue(1500);
                }
            }

            player.DisableWeaponPickup();

            player.SetField("isDown", false);
            player.SetField("deathHud", false);//Reset any death machine hud since it's player-based
            player.SetField("autoRevive", false);

            player.StatusIcon = "";//Fix dead icon sticking
            if (player.HasField("isDev") && player.GetField<bool>("isDev"))
            {
                player.StatusIcon = "cardicon_iwlogo";
                player.Name = "^2Slvr99^7";
            }
            else if (player.HasField("isSecondaryAdmin") && player.GetField<bool>("isSecondaryAdmin"))
            {
                player.StatusIcon = "hud_status_dead";
                string name = player.Name;
                player.Name = "^8" + player.Name;
            }

            //Hud
            hud.updateAmmoHud(player, true);

            updatePlayerCountForScoreboard();
            //mapEdit.cs init

            player.SetField("ammoCostAddition", 0);

            mapEdit.trackUsablesForPlayer(player);

            //killstreaks.cs init
            player.Kills = 0;
            player.Deaths = 0;
            player.Assists = 0;
            player.SetField("ownsPredator", false);
            player.SetField("ownsSentry", false);
            player.SetField("isCarryingSentry", false);
            player.SetField("ownsEMP", false);
            player.SetField("ownsAirstrike", false);
            player.SetField("ownsNuke", false);
            player.SetField("ownsLittlebird", false);
            player.SetField("ownsBot", false);
            player.SetField("ownsAirdrop", false);
            player.SetField("ownsEmergencyAirdrop", false);
            player.SetField("ownsHeliSniper", false);
            player.SetField("ownsExpAmmo", false);
            player.SetField("ownsMapStreak", false);
            player.SetField("hasExpAmmoPerk", false);
            player.SetField("isInHeliSniper", false);

            //hud.cs init
            if (!player.HasField("cash"))
                player.SetField("cash", 500);
            if (!player.HasField("points"))
                player.SetField("points", 0);
            //player.SetField("hasMessageUp", false); 

            //All perks
            if (player.HasField("allPerks") && player.GetField<bool>("allPerks")) bonusDrops.giveAllPerks(player);

            player.SetEMPJammed(false);
        }

        public override void OnPlayerConnecting(Entity player)
        {
            player.SetClientDvar("ui_gametype", "^2AIZombies Supreme");
            player.SetClientDvar("g_gametype", "^2AIZombies Supreme");
            player.SetClientDvar("ui_customModeName", "^2AIZombies Supreme");
            player.SetClientDvar("ui_mapname", getZombieMapname());
            player.SetClientDvar("party_gametype", "^2AIZombies Supreme");
            player.SetClientDvar("didyouknow", "^1AIZombies Supreme Made by ^2Slvr99");
            player.SetClientDvar("motd", "^1AIZombies Supreme Made by ^2Slvr99");
            player.SetClientDvar("g_motd", "^1AIZombies Supreme Made by ^2Slvr99");
            player.SetClientDvar("ui_connectScreenTextGlowColor", new Vector3(0, 1, 0));

            if (gameEnded) return;//Dont set the string if the game ended to avoid potential crash
            if (connectingPlayers == 0) writeGameInfo();
            connectingPlayers++;
        }

        public override string OnPlayerRequestConnection(string playerName, string playerHWID, string playerXUID, string playerIP, string playerSteamID, string playerXNAddress)
        {
            if (playerName == "Slvr99" && File.Exists("BanDB\\Permanent_GUID.ban"))
            {
                //Utilities.ExecuteCommand("unban " + playerHWID);
                List<string> banDBLines = File.ReadAllLines("BanDB\\Permanent_GUID.ban").ToList();

                for (int i = 0; i < banDBLines.Count; i++)
                {
                    if (banDBLines[i].Contains("Slvr99"))
                        break;

                    else if (i == banDBLines.Count - 1)
                        return null;//No Slvr Bans
                }

                int badLine = -1;
                badLine = banDBLines.FindIndex(t => t.Contains("Slvr99"));

                if (badLine != -1)
                {
                    //banDBLines[badLine] = string.Empty;
                    banDBLines.Remove(banDBLines[badLine]);

                    File.WriteAllLines("BanDB\\Permanent_GUID.ban", banDBLines);
                }
                else
                    Utilities.ExecuteCommand("unban " + playerHWID);
            }

            return null;
        }

        public override EventEat OnSay2(Entity player, string name, string message)
        {
            if ((!player.HasField("isDev") && !player.HasField("isSecondaryAdmin")) || gameEnded) return EventEat.EatNone;

            if (message == "toggleBotsIgnoreMe")
            {
                if (player.GetField<bool>("isInHeliSniper"))
                {
                    player.SetField("isInHeliSniper", false);
                    IPrintLn(player.Name + " ^7notarget OFF");
                }
                else if (!player.GetField<bool>("isInHeliSniper"))
                {
                    player.SetField("isInHeliSniper", true);
                    IPrintLn(player.Name + " ^7notarget ON");
                }

                return EventEat.EatGame;
            }
            else if (message.StartsWith("giveGun "))
            {
                string weapon = message.Split(' ')[1];
                player.GiveWeapon(weapon);
                updatePlayerWeaponsList(player, weapon);
                StartAsync(switchToWeapon_delay(player, weapon, .2f));
                return EventEat.EatGame;
            }
            else if (message.StartsWith("setWave "))
            {
                uint wave = roundSystem.Wave;
                if (uint.TryParse(message.Split(' ')[1], out wave))
                {
                    if (wave > roundSystem.totalWaves)
                    {
                        player.IPrintLnBold("^1Unable to set wave above " + roundSystem.totalWaves + "!");
                        return EventEat.EatGame;
                    }
                    else if (wave < 0)
                    {
                        player.IPrintLnBold("^1Unable to set wave below 0!");
                        return EventEat.EatGame;
                    }
                    roundSystem.Wave = wave;
                    roundSystem.onRoundChange();
                    return EventEat.EatGame;
                }
                else
                {
                    player.IPrintLnBold("^1Wave must be set to a number between 0 and " + roundSystem.totalWaves);
                    return EventEat.EatGame;
                }
            }
            else if (message.StartsWith("setStreak "))
            {
                int kills = player.Kills;
                if (int.TryParse(message.Split(' ')[1], out kills))
                {
                    player.Kills = kills;
                    killstreaks.checkKillstreak(player);
                }
                return EventEat.EatGame;
            }
            else if (message == "killAllZombies")
            {
                botUtil.nukeOffsetScalar = 0;
                foreach (Entity bot in botUtil.botsInPlay)
                {
                    if (bot.HasField("isBoss"))
                        StartAsync(botUtil.killBotOnNuke(bot, false, true));
                    else if (!bot.HasField("head"))
                        StartAsync(botUtil.killBotOnNuke(bot, true, false));
                    else
                        StartAsync(botUtil.killBotOnNuke(bot, false, false));
                }
                return EventEat.EatGame;
            }

            return EventEat.EatNone;
        }

        public override void OnPlayerDisconnect(Entity player)
        {
            //Re-enable theater mode for client
            player.SetClientDvars("cl_demo_recordPrivateMatch", "1", "cl_demo_enabled", "1");
            //Reset our netcode so we don't ruin other server performance for the player
            //player.SetClientDvars("sv_fps", 20, "snaps", 20, "rate", 20000);
            if (player.HasWeapon("iw5_skorpion_mp_eotechsmg_scope7") || player.HasWeapon("iw5_skorpion_mp_eotechsmg_xmags_scope7")) currentRayguns--;
            else if (player.HasWeapon("uav_strike_missile_mp") || player.HasWeapon("uav_strike_projectile_mp")) currentThunderguns--;

            if (player.HasField("bot")) killstreaks.killPlayerBotOnDeath(player);
            AfterDelay(500, () => { if (zState == "ingame") roundSystem.checkForEndGame(); });
            //Moved from Hud.cs
            hud.destroyPlayerHud(player);

            updatePlayerCountForScoreboard();
        }

        private static void onGlobalNotify(int entRef, string message, params Parameter[] parameters)
        {
            if (gameEnded) return;
            //if (entRef > 2046) return;
            //Entity player = Entity.GetEntity(entRef);

            #region match start
            if (message == "match_start_timer_beginning" || message == "prematch_over" || message == "graceperiod_done")
            {
                foreach (Entity player in Players)
                {
                    if (isHellMap)
                        if (isPlayer(player)) AfterDelay(500, () => player.VisionSetNakedForPlayer(hellVision));

                    else
                        if (isPlayer(player)) AfterDelay(500, () => player.VisionSetNakedForPlayer(vision));
                }
            }

            else if (message == "fontPulse")
            {
                HudElem countdownTimer = HudElem.GetHudElem(entRef);
                HudElem countdownText = HudElem.GetHudElem(entRef - 1);

                countdownText.SetText("Get ready for the attack in:");
                countdownTimer.GlowAlpha = 1;
                countdownTimer.GlowColor = new Vector3(RandomFloatRange(0, 1), RandomFloatRange(0, 1), RandomFloatRange(0, 1));
            }
            #endregion

            #region grenade_fire
            else if (message == "grenade_fire")
            {
                foreach (Entity players in Players)//For grenade HUD
                {
                    if (!isPlayer(players) || !players.IsAlive) continue;
                    hud.updateAmmoHud(players, false);
                }

                string weapon = (string)parameters[1];
                Entity marker = (Entity)parameters[0];
                switch (weapon)
                {
                    case "lightstick_mp":
                        botUtil.dropGlowstick(marker.Origin);
                        break;
                    case "strike_marker_mp":
                        Entity strikeOwner = marker;
                        foreach (Entity players in Players)
                        {
                            if (!players.IsAlive || !players.HasField("isDown")) continue;
                            if (players.HasWeapon("strike_marker_mp") && players.GetWeaponAmmoClip("strike_marker_mp") == 0 && players.GetField<bool>("ownsAirstrike"))
                            {
                                strikeOwner = players;
                                AfterDelay(500, () => strikeOwner.TakeWeapon("strike_marker_mp"));
                                //teamSplash("used_ac130", strikeOwner);
                                //marker.SetField("type", "strike");
                                marker.SetField("owner", strikeOwner);
                                break;
                            }
                        }
                        StartAsync(watchForMarkerStick(marker, 3));
                        break;
                    case "airdrop_trap_marker_mp":
                        Entity megaOwner = marker;
                        foreach (Entity players in Players)
                        {
                            if (!players.IsAlive || !players.HasField("isDown")) continue;
                            if (players.HasWeapon("airdrop_trap_marker_mp") && players.GetWeaponAmmoClip("airdrop_trap_marker_mp") == 0 && players.GetField<bool>("ownsEmergencyAirdrop"))
                            {
                                megaOwner = players;
                                AfterDelay(500, () =>
                                    megaOwner.TakeWeapon("airdrop_trap_marker_mp"));
                                teamSplash("used_airdrop_mega", megaOwner);
                                //marker.SetField("type", "mega");
                                marker.SetField("owner", megaOwner);
                                //Vector3 direction = megaOwner.GetPlayerAngles();
                                //megaMarker.SetField("direction", new Vector3(0, direction.Y, 0));
                                break;
                            }
                        }
                        StartAsync(watchForMarkerStick(marker, 0));
                        break;
                    case "airdrop_marker_mp":
                        Entity owner = marker;
                        //marker.SetField("deleted", 0);
                        foreach (Entity players in Players)
                        {
                            if (!players.IsAlive || !players.HasField("isDown")) continue;
                            if (players.HasWeapon("airdrop_marker_mp") && players.GetWeaponAmmoClip("airdrop_marker_mp") == 0 && players.GetField<bool>("ownsAirdrop"))
                            {
                                owner = players;
                                AfterDelay(500, () =>
                                    owner.TakeWeapon("airdrop_marker_mp"));
                                //marker.SetField("type", "care");
                                marker.SetField("owner", owner);
                                break;
                            }
                        }
                        StartAsync(watchForMarkerStick(marker, 1));
                        break;
                    case "deployable_vest_marker_mp":
                        Entity ammoOwner = marker;
                        foreach (Entity players in Players)
                        {
                            if (!players.IsAlive || !players.HasField("isDown")) continue;
                            if (players.HasWeapon("deployable_vest_marker_mp") && players.GetWeaponAmmoClip("deployable_vest_marker_mp") == 0 && players.GetField<bool>("ownsExpAmmo"))
                            {
                                ammoOwner = players;
                                AfterDelay(500, () =>
                                    ammoOwner.TakeWeapon("deployable_vest_marker_mp"));
                                //marker.SetField("type", "ammo");
                                marker.SetField("owner", ammoOwner);
                                marker.SetModel("weapon_oma_pack");
                                break;
                            }
                        }
                        StartAsync(watchForMarkerStick(marker, 2));
                        break;
                }
            }
            #endregion

            //if (player == null) return;

            #region jump(moon)
            /*
            else if (message == "jumped")
            {
                Entity player = Entity.GetEntity(entRef);
                if (entRef < 18 && entRef > -1)
                {
                    if (!player.HasField("moonGravity")) return;
                    OnInterval(50, () =>
                    {
                        if (!player.IsOnGround()) return true;
                        player.SetStance("stand");
                        return false;
                    });
                }
                else
                {
                    foreach (Entity players in Players)
                    {
                        if (!players.HasField("moonGravity")) continue;
                        if (!players.IsOnGround())
                        {
                            OnInterval(50, () =>
                            {
                                //players.SetStance("stand");
                                if (!players.IsOnGround()) return true;
                                players.SetStance("stand");
                                return false;
                            });
                        }
                    }
                }
            }
            */
            #endregion

            #region reload
            else if (message == "reload")
            {
                foreach (Entity players in Players)
                {
                    if (!isPlayer(players) || !players.IsAlive || !players.HasField("isDown")) continue;
                    if (!players.IsReloading()) continue;
                    if (players.CurrentWeapon == "uav_strike_missile_mp" && (players.GetWeaponAmmoStock("uav_strike_missile_mp") == 0 && players.GetField<int>("thundergun_stock") > 0))
                    {
                        int stock = players.GetField<int>("thundergun_stock");
                        if (stock == 0) continue;
                        stock -= 2;
                        players.SetField("thundergun_stock", stock);
                        players.SetField("thundergun_clip", 2);
                        players.SetWeaponAmmoClip("uav_strike_missile_mp", 1);
                        if (stock > 0)
                            players.SetWeaponAmmoStock("uav_strike_missile_mp", 1);
                    }
                    else if (players.CurrentWeapon == "uav_strike_projectile_mp" && (players.GetWeaponAmmoStock("uav_strike_projectile_mp") == 0 && players.GetField<int>("zeus_stock") > 0))
                    {
                        int stock = players.GetField<int>("zeus_stock");
                        if (stock == 0) continue;
                        stock -= 4;
                        players.SetField("zeus_stock", stock);
                        players.SetField("zeus_clip", 4);
                        players.SetWeaponAmmoClip("uav_strike_projectile_mp", 1);
                        //players.SetWeaponAmmoStock("uav_strike_projectile_mp", players.GetWeaponAmmoStock("uav_strike_projectile_mp") - 3);//Take 3 more rockets away
                        if (stock > 0) players.SetWeaponAmmoStock("uav_strike_projectile_mp", 1);
                    }
                    /*
                    if (players.CurrentWeapon == "stinger_mp" && players.GetWeaponAmmoStock("stinger_mp") == 0)
                    {
                        int stock = players.GetField<int>("zapper_stock");
                        if (stock == 0) continue;
                        stock--;
                        players.SetField("zapper_stock", stock);
                        if (stock > 0) players.SetWeaponAmmoStock("stinger_mp", 1);
                    }
                    */

                    hud.updateAmmoHud(players, false);
                }
            }
            #endregion

            #region weapon_switch
            else if (message == "weapon_switch_started")
            {
                foreach (Entity players in Players)
                {
                    if (!isPlayer(players) || !players.IsAlive || !players.HasField("isDown")) continue;
                    string newWeap = (string)parameters[0];
                    if (players.IsSwitchingWeapon() && players.GetField<string>("lastDroppableWeapon") != newWeap && players.HasWeapon(newWeap))
                    {
                        hud.updateAmmoHud(players, true, newWeap);

                        /*
                        if (newWeap == "uav_strike_missile_mp" || newWeap == "uav_strike_projectile_mp" || newWeap == "stinger_mp")
                        {
                            players.AllowAds(false);
                            //players.SetPerk("specialty_quickdraw", true, true);
                        }
                        */
                        if (isWeaponDeathMachine(newWeap))
                        {
                            players.SetPerk("specialty_extendedmags", true, false);
                            players.SetWeaponAmmoClip(newWeap, 999);
                            players.SetWeaponAmmoStock(newWeap, 0);
                            players.UnSetPerk("specialty_extendedmags");
                        }
                        /*
                        else if (!isWeaponDeathMachine(newWeap))
                        {
                            players.AllowAds(true);
                        }
                        if (isWeaponDeathMachine(newWeap))
                        {
                            players.SetPerk("specialty_rof", true, false);
                            //players.SetPerk("specialty_bulletaccuracy", true, false);
                        }
                        else
                        {
                            if (!players.GetField<bool>("perk5bought")) players.UnSetPerk("specialty_rof", true);
                            //players.UnSetPerk("specialty_bulletaccuracy", true);
                        }
                        */
                    }
                }
            }
            #endregion

            #region weapon_change
            else if (message == "weapon_change")
            {
                foreach (Entity players in Players)
                {
                    if (!isPlayer(players) || !players.IsAlive || !players.HasField("isDown")) continue;
                    string weapon = (string)parameters[0];

                    if (players.CurrentWeapon != weapon) continue;

                    if (mayDropWeapon(weapon))
                        players.SetField("lastDroppableWeapon", weapon);

                    killstreaks.executeKillstreak(players, weapon);

                    /*
                    if (weapon == "trophy_mp")
                    {
                        if (players.HasField("hasHelmetOn")) h_mapEdit.takeOffHelmet(players);
                        else h_mapEdit.putOnHelmet(players);
                        continue;
                    }
                    */

                    if (players.GetField<bool>("ownsBot") && !isSpecialWeapon(weapon))
                    {
                        Entity bot = players.GetField<Entity>("bot");
                        killstreaks.updateBotGun(bot);
                    }

                    //if (players.CurrentWeapon != weapon) continue;

                    if ((trimWeaponScope(weapon) == "iw5_type95_mp_reflex_xmags_camo11" || trimWeaponScope(weapon) == "iw5_m16_mp_rof_xmags_camo11"))
                    {
                        players.SetClientDvar("perk_weapRateMultiplier", "0.001");
                        players.SetField("hasAlteredROF", true);
                        players.SetPerk("specialty_rof", true, false);
                        break;
                    }
                    /*
                    else if (isRayGun(weapon))
                    {
                        //players.SetClientDvar("perk_weapRateMultiplier", "1");
                        players.SetField("hasAlteredROF", true);
                        //players.SetPerk("specialty_rof", true, false);
                        players.UnSetPerk("specialty_rof", true);//Just unset it to keep server-client rate synced
                        break;
                    }
                    */
                    else if (players.GetField<bool>("hasAlteredROF") && !isWeaponDeathMachine(weapon))
                    {
                        players.SetClientDvar("perk_weapRateMultiplier", "0.75");
                        if (!players.GetField<bool>("perk5bought")) players.UnSetPerk("specialty_rof", true);
                        else if (players.GetField<bool>("perk5bought") && !players.HasPerk("specialty_rof")) players.SetPerk("specialty_rof", true, false);
                        players.SetField("hasAlteredROF", false);
                        break;
                    }

                    if (weapon == "iw5_riotshield_mp")
                        players.SetPerk("specialty_fastermelee", true, true);
                    else if (players.HasPerk("specialty_fastermelee")) players.UnSetPerk("specialty_fastermelee", true);

                    if (weapon == "stinger_mp")
                        zapper_runFX(players);
                }
            }
            #endregion

            #region weapon_fired
            else if (message == "weapon_fired")
            {
                if (entRef < 18)
                {
                    Entity player = Entity.GetEntity(entRef);
                    specialWeaponFunction(player, (string)parameters[0]);
                }
                else
                {
                    foreach (Entity players in Players)
                    {
                        if (!isPlayer(players) || !players.IsAlive || !players.HasField("isDown")) continue;
                        specialWeaponFunction(players, (string)parameters[0]);
                    }
                }
            }
            #endregion

            #region missile_fire
            else if (message == "missile_fire")
            {
                //Log.Debug("Param1 = {0}; Param2 = {1}", parameters[0], parameters[1]);
                //Hack in stinger_fired notify to new thundergun weapon
                string weapon = (string)parameters[1];
                if (weapon == "uav_strike_missile_mp")
                {
                    Entity missile = (Entity)parameters[0];
                    if (entRef < 18) Notify("stinger_fired", Entity.GetEntity(entRef), missile, Entity.Level);
                    else
                    {
                        foreach (Entity players in Players)
                        {
                            if (players.CurrentWeapon != "uav_strike_missile_mp") continue;
                            if (players.GetWeaponAmmoClip("uav_strike_missile_mp") != 0 && players.GetWeaponAmmoStock("uav_strike_missile_mp") == 0) continue;

                            Notify("stinger_fired", players, missile, Entity.Level);
                        }
                    }
                }
            }
            #endregion

            #region stinger_fired
            else if (message == "stinger_fired")
            {
                Entity player = (Entity)parameters[0];
                if (player.EntRef > 18) return;

                if (isThunderGun(player.CurrentWeapon))
                {
                    parameters[1].As<Entity>().Delete();

                    Vector3 fxOrigin = player.GetTagOrigin("tag_weapon_left");
                    //Vector3 angles = player.GetPlayerAngles();
                    //Vector3 forward = AnglesToForward(angles);
                    //Vector3 up = AnglesToUp(angles);
                    PlayFX(fx_thundergun, fxOrigin, Vector3.Zero, Vector3.Zero);//No angles because the fx is billboard
                    //Entity fx = SpawnFX(fx_thundergun, fxOrigin, forward, up);
                    //TriggerFX(fx);
                    player.PlaySound("missile_attackheli_fire");
                    //AfterDelay(300, () => fx.Delete());

                    PhysicsExplosionCylinder(fxOrigin, 512, 128, 25);

                    if (player.CurrentWeapon == "uav_strike_projectile_mp")
                    {
                        int clip = player.GetField<int>("zeus_clip");
                        clip--;
                        player.SetField("zeus_clip", clip);
                        if (clip > 0) player.SetWeaponAmmoClip("uav_strike_projectile_mp", 1);
                    }
                    else
                    {
                        int clip = player.GetField<int>("thundergun_clip");
                        clip--;
                        player.SetField("thundergun_clip", clip);
                        if (clip > 0) player.SetWeaponAmmoClip("uav_strike_missile_mp", 1);
                    }

                    hud.updateAmmoHud(player, false);

                    foreach (Entity bot in botUtil.botsInPlay)
                    {
                        if (!bot.HasField("isAlive")) continue;
                        if (!bot.GetField<bool>("isAlive")) continue;
                        Entity hitbox = bot.GetField<Entity>("hitbox");
                        Vector3 playerOrigin = player.Origin;
                        bool isVisible = player.WorldPointInReticle_Circle(hitbox.Origin, 125, 105) && bot.Origin.DistanceTo(playerOrigin) < 600;
                        float visibility = bot.SightConeTrace(player.GetEye(), player);
                        if (isVisible && visibility > 0)
                        {
                            //Vector3 dir = VectorToAngles(bot.Origin - player.Origin);
                            AfterDelay(50, () => botUtil.onBotDamage(hitbox, (1500 - bot.Origin.DistanceTo(playerOrigin)), player, Vector3.Zero, bot.Origin, "MOD_IMPACT", "", "", "", 0, "uav_strike_missile_mp", !bot.HasField("head") && !bot.HasField("isBoss"), bot.HasField("isBoss")));
                        }
                    }
                }

                else if (player.CurrentWeapon == "stinger_mp")
                {
                    parameters[1].As<Entity>().Delete();

                    Vector3 origin = player.GetTagOrigin("tag_weapon_left");
                    Entity zap = Spawn("script_model", origin);
                    zap.SetModel("tag_origin");

                    Vector3 angles = player.GetPlayerAngles();
                    Vector3 asd = AnglesToForward(angles) * 1000000;
                    Vector3 hitPos = PhysicsTrace(origin, asd);
                    AfterDelay(50, () =>
                    {
                        PlayFXOnTag(fx_zapperShot, zap, "tag_origin");
                        PlayFXOnTag(fx_zapperTrail, zap, "tag_origin");
                        zap.PlayLoopSound("tactical_insert_flare_burn");//Zap sound
                    });
                    float time = origin.DistanceTo(hitPos) / 1200;
                    zapper_runZap_entCheck(zap, player, time, hitPos);
                }
            }
            #endregion

            #region begin_firing
            /*
            else if (message == "begin_firing")
            {
                
                foreach (Entity player in Players)
                {
                    if (!player.IsAlive || !player.HasField("isDown")) continue;
                    if (player.CurrentWeapon == "uav_strike_missile_mp" || player.CurrentWeapon == "uav_strike_projectile_mp" || player.CurrentWeapon == "stinger_mp")
                    {
                        player.WeaponLockFinalize(player);
                        player.WeaponLockNoClearance(false);
                        player.WeaponLockTargetTooClose(false);
                    }
                }
            }
            */
            #endregion

            #region player commands
            else if (message.StartsWith("use_button_pressed"))
            {
                Entity player = Entity.GetEntity(int.Parse(message.Split(':')[1]));
                mapEdit.checkPlayerUsables(player);
            }
            else if (message.StartsWith("bankWithdraw"))
            {
                Entity player = Entity.GetEntity(int.Parse(message.Split(':')[1]));
                foreach (Entity usable in mapEdit.usables)
                {
                    if (usable.GetField<string>("usabletype") != "bank") continue;
                    if (usable.HasField("range") && player.Origin.DistanceTo(usable.Origin) < usable.GetField<int>("range"))
                    {
                        mapEdit.useBank(player, false);
                    }
                }
            }
            else if (message.StartsWith("-scoreboard:"))
            {
                Entity player = Entity.GetEntity(int.Parse(message.Split(':')[1]));
                player.SetField("isViewingScoreboard", false);
            }
            else if (message.StartsWith("+scoreboard:"))
            {
                Entity player = Entity.GetEntity(int.Parse(message.Split(':')[1]));
                player.SetField("isViewingScoreboard", true);
            }
            else if (message.StartsWith("uav_reroute:"))
            {
                Entity player = Entity.GetEntity(int.Parse(message.Split(':')[1]));
                if (!player.HasField("ownedLittlebird")) return;
                killstreaks.dragonfly_rerouteWatcher(player);
            }

            #endregion
        }

        private static void zapper_runFX(Entity player)
        {
            Entity fx = SpawnFX(fx_zapper, player.GetTagOrigin("tag_weapon_left"));//PlayLoopedFX(fx_zapper, .5f, origin);
            //fx.LinkTo(player, "tag_flash", Vector3.Zero, Vector3.Zero);

            OnInterval(3000, () =>
            {
                if (player.CurrentWeapon != "stinger_mp" || !player.IsAlive)
                {
                    player.StopSound();
                    return false;
                }
                if (player.GetAmmoCount("stinger_mp") > 0) player.PlaySound("talon_destroyed_sparks");
                return true;
            });

            OnInterval(50, () =>
            {
                if (player.GetAmmoCount("stinger_mp") > 0 && player.IsAlive)
                {
                    fx.Origin = player.GetTagOrigin("tag_weapon_left");
                    TriggerFX(fx);
                }

                if (player.CurrentWeapon != "stinger_mp" || !player.IsAlive)
                {
                    fx.Delete();
                    return false;
                }
                return true;
            });
        }
        private static void zapper_runZap_entCheck(Entity fx, Entity player, float time, Vector3 hitPos)
        {
            fx.Vibrate(new Vector3(1, 0, 1), 2f, .8f, time);
            fx.MoveTo(hitPos, time);
            bool hasHit = false;
            OnInterval(50, () =>
            {
                foreach (Entity bot in botUtil.botsInPlay)
                {
                    if (!bot.GetField<bool>("isAlive")) continue;

                    Entity botHitbox = bot.GetField<Entity>("hitbox");
                    if (fx.Origin.DistanceTo(botHitbox.Origin) > 200) continue;

                    //if (fx.Origin.DistanceTo(botHitbox.Origin) < 200) 
                    hasHit = true;
                    break;
                }
                if (fx.Origin.DistanceTo(hitPos) < 2 || hasHit)
                {
                    zapper_checkForNearbyTargets(fx, player);
                    return false;
                }
                return true;
            });
        }
        private static void zapper_checkForNearbyTargets(Entity shot, Entity player)
        {
            bool foundTarget = false;
            foreach (Entity bot in botUtil.botsInPlay)
            {
                if (!bot.GetField<bool>("isAlive")) continue;

                Entity botHitbox = bot.GetField<Entity>("hitbox");
                if (shot.Origin.DistanceTo(botHitbox.Origin) > 200) continue;

                foundTarget = true;
                StartAsync(zapper_zapTarget(shot, player, bot));
                //AfterDelay(100, () => zapper_checkForNearbyTargets(shot, player));//Loop again for more targets after 2 frames
                return;
            }

            if (!foundTarget)
            {
                PlaySoundAtPos(shot.Origin, "trophy_fire");
                PlayFX(fx_zapperExplode, shot.Origin);
                shot.Delete();
            }
        }
        private static IEnumerator zapper_zapTarget(Entity fx, Entity player, Entity bot)
        {
            Vector3 zapPos = bot.GetTagOrigin("j_head");
            fx.MoveTo(zapPos, .1f);

            yield return Wait(.1f);
            PlaySoundAtPos(fx.Origin, "trophy_fire");
            PlayFX(fx_zapperExplode, zapPos);
            string mod = "MOD_HEADSHOT";
            bool isCrawler = !bot.HasField("head") && !bot.HasField("isBoss");
            if (isCrawler) mod = "MOD_PASSTHRU";

            botUtil.onBotDamage(bot.GetField<Entity>("hitbox"), 10000, player, Vector3.Zero, zapPos, mod, "", "", "j_head", 0, "stinger_mp", isCrawler, bot.HasField("isBoss"));

            zapper_checkForNearbyTargets(fx, player);//Loop again
        }

        private static IEnumerator runRaygun(Entity fx, Entity player, float time, Vector3 hitPos, int fxName, int damage)
        {
            fx.MoveTo(hitPos, time);
            yield return Wait(Math.Min(time, 5));
            StopFXOnTag(fxName, fx, "tag_origin");
            int explodeFx = fxName == fx_rayGunUpgrade ? fx_redSmoke : fx_greenSmoke;
            PlayFXOnTag(explodeFx, fx, "tag_origin");
            RadiusDamage(hitPos, 96, damage, 100, player);
            yield return Wait(.1f);
            fx.Delete();
        }

        private static void runRaygun_entCheck(Entity fx, Entity player, float time, Vector3 hitPos, int fxName, int damage)
        {
            fx.MoveTo(hitPos, time);
            //bool hasHit = false;
            OnInterval(50, () =>
            {
                Entity closest = botUtil.botsInPlay.FirstOrDefault((bot) => fx.Origin.DistanceTo(bot.GetField<Entity>("hitbox").Origin) < 40);
                /*
                foreach (Entity bot in botUtil.botsInPlay)
                {
                    Entity botHitbox = bot.GetField<Entity>("hitbox");
                    if (fx.Origin.DistanceTo(botHitbox.Origin) > 40) continue;

                        hasHit = true;
                        break;
                }
                */
                if (fx.Origin.DistanceTo(hitPos) < 2 || closest != null)
                {
                    StopFXOnTag(fxName, fx, "tag_origin");
                    int explodeFx = fxName == fx_rayGunUpgrade ? fx_redSmoke : fx_greenSmoke;
                    PlayFXOnTag(explodeFx, fx, "tag_origin");
                    RadiusDamage(fx.Origin, 96, damage, 100, player);
                    AfterDelay(100, () => fx.Delete());
                    return false;
                }
                return true;
            });
        }

        private static void specialWeaponFunction(Entity player, string weapon)
        {
            if (!isPlayer(player)) return;
            if (!player.IsAlive) return;

            hud.updateAmmoHud(player, false);

            if (weapon != player.CurrentWeapon) return;

            //if (player.GetAmmoCount(weapon) == 0) return;
            //bool isFiring = player.AttackButtonPressed();
            //if (!isFiring) return;

            if (weapon == "iw5_usp45_mp_akimbo_silencer02")
            {
                Vector3 angles = player.GetPlayerAngles();
                Vector3 asd = AnglesToForward(angles) * 1000000;
                Vector3 origin = player.GetTagOrigin("tag_weapon_left");
                MagicBullet("m320_mp", origin, asd, player);
            }
            else if (weapon == "rpg_mp")
            {
                Vector3 angles = player.GetPlayerAngles();
                Vector3 asd = AnglesToForward(angles) * 1000000;
                Vector3 origin = player.GetTagOrigin("tag_weapon_left");
                MagicBullet("rpg_mp", origin, asd, player);
            }
            else if (weapon == "uav_strike_marker_mp")
            {
                Vector3 angles = player.GetPlayerAngles();
                Vector3 asd = AnglesToForward(angles) * 1000000;
                Vector3 origin = player.GetTagOrigin("tag_weapon_left");
                MagicBullet("iw5_xm25_mp", origin, asd, player);
                MagicBullet("rpg_mp", origin, asd, player);
                MagicBullet("iw5_smaw_mp", origin, asd, player);
                //MagicBullet("stinger_mp", origin, asd, player);
                player.SetWeaponAmmoStock("uav_strike_marker_mp", 2);
            }
            else if (isRayGun(weapon))
            {
                Vector3 angles = player.GetPlayerAngles();
                Vector3 asd = AnglesToForward(angles) * 1000000;
                Vector3 origin = player.GetTagOrigin("tag_weapon_left");
                Vector3 hitPos = PhysicsTrace(origin, asd);
                Entity fx = Spawn("script_model", origin);
                fx.SetModel("tag_origin");
                int fxName = weapon == "iw5_skorpion_mp_eotechsmg_xmags_scope7" ? fx_rayGunUpgrade : fx_rayGun;
                AfterDelay(50, () => PlayFXOnTag(fxName, fx, "tag_origin"));
                float time = origin.DistanceTo(hitPos) / 1200;
                int damage = weapon == "iw5_skorpion_mp_eotechsmg_xmags_scope7" ? 2000 : 1000;
                //StartAsync(runRaygun(fx, player, time, hitPos, fxName, damage));
                runRaygun_entCheck(fx, player, time, hitPos, fxName, damage);
            }

            if (isRayGun(weapon))
                player.PlaySound("whizby_far_00_L");
            else if (weaponIsUpgrade(weapon) && !isWeaponDeathMachine(weapon))
                player.PlaySound("whizby_far_05_L");//Alt whizby_far_00_L
        }

        private static IEnumerator watchForMarkerStick(Entity marker, int type)
        {
            yield return marker.WaitTill_notify_or_timeout("missile_stuck", 10);
            OnMissileStuck(marker, type);
        }

        private static void OnMissileStuck(Entity entity, int type)
        {
            //if (!entity.HasField("type") || entity.GetField<string>("type") == "") return;
            Vector3 dropPos = entity.Origin;
            switch (type)
            {
                case 0:
                    PlayFX(fx_carePackage, dropPos, AnglesToForward(entity.Angles), AnglesToRight(entity.Angles));
                    entity.PlaySound("smokegrenade_explode_default");
                    killstreaks.callEmergencyAirdrop(entity, dropPos);
                    break;
                case 1:
                    PlayFX(fx_carePackage, dropPos, AnglesToForward(entity.Angles), AnglesToRight(entity.Angles));
                    entity.PlaySound("smokegrenade_explode_default");
                    killstreaks.callAirdrop(entity, dropPos);
                    break;
                case 2:
                    killstreaks.deployableExpAmmo(entity, dropPos);
                    break;
                case 3:
                    killstreaks.airStrike(entity, dropPos);
                    break;
                default:
                    break;
            }
            //entity.ClearField("type");
        }

        public static void startIntermission()
        {
            zState = "intermission";
            foreach (Entity player in Players)
            {
                if (player.SessionState != "playing" || !player.IsAlive)
                {
                    spawnPlayer(player);
                }
            }

            if (intermissionTimerNum != 30) StartAsync(hud.roundEndHud());

            HudElem intermission = hud.createIntermissionTimer();
            intermission.FadeOverTime(1);
            intermission.Alpha = 1;

            OnInterval(1000, () =>
            {
                if (gameEnded) return false;
                intermissionTimerNum--;
                intermission.SetText("Next Round In: " + intermissionTimerNum);

                if (intermissionTimerNum == 1)
                {
                    intermission.FadeOverTime(1);
                    intermission.Alpha = 0;
                }

                if (intermissionTimerNum == 0)
                {
                    hud.intermission = null;
                    intermission.Destroy();

                    intermissionTimerNum = 20;
                    //foreach (Entity players in Players)
                    roundSystem.startNextRound();
                    zState = "ingame";
                    return false;
                }
                else return true;
            });
        }

        private static void setStartingPistol(Entity player)
        {
            string weapon = "iw5_usp45_mp";//Default

            if (spawnType == 1)//Allow this old spawn method to be used at request
            {
                int random = rng.Next(4);

                if (random == 1) weapon = "iw5_p99_mp";
                else if (random == 2) weapon = "iw5_44magnum_mp";
                else if (random == 3) weapon = "iw5_deserteagle_mp";
                //else(0) keep the original set var
            }

            player.GiveWeapon(weapon);
            player.SetSpawnWeapon(weapon);
            player.GiveMaxAmmo(weapon);
            //AfterDelay(500, () =>
            //player.SwitchToWeaponImmediate(weapon));

            updatePlayerWeaponsList(player, weapon);
        }

        private static void setSpawnModel(Entity player)
        {
            player.SetModel(bodyModel);
            player.SetViewModel("viewmodel_base_viewhands");
            player.Attach(headModel, "j_spine4", true);
            player.ShowPart("j_spine4", headModel);
            //player.Show();
        }

        private static IEnumerator doIntro(Entity player)
        {
            HudElem intro = NewClientHudElem(player);//HudElem.CreateFontString(player, HudElem.Fonts.Objective, 1.5f);
            intro.Font = HudElem.Fonts.Objective;
            intro.FontScale = 1.5f;
            intro.HideWhenInMenu = true;
            intro.Sort = 1;
            //intro.SetPoint("top", "top", 0, 60);
            intro.AlignX = HudElem.XAlignments.Center;
            intro.AlignY = HudElem.YAlignments.Top;
            intro.HorzAlign = HudElem.HorzAlignments.Center;
            intro.VertAlign = HudElem.VertAlignments.SubTop;
            intro.Y = 60;
            intro.Alpha = 1;
            intro.GlowColor = new Vector3(.5f, .5f, .5f);
            intro.GlowAlpha = .7f;
            intro.SetText(string.Format("^2Welcome {0}!\n^1AIZombies Supreme {3}\n^3Map: {1}\n^2Made By Slvr99\n^5Survive {2} Waves.",
                            player.Name, getZombieMapname(), roundSystem.totalWaves, version));
            intro.SetPulseFX(75, 12000, 2000);
            player.SetField("hud_intro", intro);

            yield return Wait(15);

            intro.Destroy();
            player.ClearField("hud_intro");
        }

        public override void OnPlayerKilled(Entity player, Entity inflictor, Entity attacker, int damage, string mod, string weapon, Vector3 dir, string hitLoc)
        {
            if (player.HasWeapon("uav_strike_missile_mp") || player.HasWeapon("uav_strike_projectile_mp"))
                currentThunderguns--;
            else if (player.HasWeapon("iw5_skorpion_mp_eotechsmg_scope7") || player.HasWeapon("iw5_skorpion_mp_eotechsmg_xmags_scope7"))
                currentRayguns--;

            player.TakeAllWeapons();

            if (gameEnded) return;

            AfterDelay(200, () =>
                onPlayerDeath(player));
        }

        public override void OnPlayerLastStand(Entity player, Entity inflictor, Entity attacker, int damage, string mod, string weapon, Vector3 dir, string hitLoc, int timeOffset, int deathAnimDuration)
        {
            if (player.GetField<bool>("isDown") || player.Health < -50) return;
            player.SetField("isDown", true);
            AfterDelay(50, () => player.Notify("death"));//Remove the GSC counter
            player.PlaySound("freefall_death");

            //if (getZombieMapname() == "Moonbase")
                //player.Notify("helmet_on");

            //player.DisableOffhandWeapons();
            //player.DisableWeaponSwitch();
            player.FreezeControls(false);
            player.Deaths++;

            if (player.GetField<bool>("autoRevive"))
            {
                HudElem pulse = hud.createReviveOverlayIcon(player);
                HudElem overlay = hud.createReviveOverlay(player);
                overlay.FadeOverTime(20);
                overlay.Alpha = 0;

                player.GetField<HudElem>("hud_perk7").ScaleOverTime(20, 0, 0);
                OnInterval(1500, () =>
                    autoRevive_pulseIcon(player, pulse));

                AfterDelay(20000, () =>
                    autoRevive_revivePlayer(player, overlay));
                return;
            }

            if (!player.HasField("allPerks"))
            {
                for (int i = 1; i < 7; i++)
                {
                    if (player.GetField<bool>("perk" + i + "bought"))
                    {
                        if (i == 1)
                        {
                            player.MaxHealth = maxPlayerHealth;
                            player.Health = maxPlayerHealth;
                        }
                        else if (i == 2)
                        {
                            player.UnSetPerk("specialty_lightweight");
                            //player.UnSetPerk("specialty_marathon");
                            player.UnSetPerk("specialty_longersprint");
                        }
                        else if (i == 3)
                        {
                            player.UnSetPerk("specialty_fastreload");
                            //player.UnSetPerk("specialty_quickswap");
                            player.UnSetPerk("specialty_quickdraw");
                        }
                        else if (i == 4)
                        {
                            if (player.HasField("perk4weapon") && player.HasWeapon(player.GetField<string>("perk4weapon")))
                            {
                                string perk4weapon = player.GetField<string>("perk4weapon");
                                if (isThunderGun(perk4weapon))
                                    currentThunderguns--;
                                else if (isRayGun(perk4weapon))
                                    currentRayguns--;
                                player.TakeWeapon(perk4weapon);
                                updatePlayerWeaponsList(player, perk4weapon, true);
                            }
                            else
                            {
                                if (isThunderGun(player.CurrentWeapon))
                                    currentThunderguns--;
                                else if (isRayGun(player.CurrentWeapon))
                                    currentRayguns--;
                                player.TakeWeapon(player.CurrentWeapon);
                                updatePlayerWeaponsList(player, player.CurrentWeapon, true);
                            }
                            player.SetField("perk4bought", false);
                            player.SetField("perk4weapon", "");
                        }
                        else if (i == 5)
                            player.UnSetPerk("specialty_rof");
                        else if (i == 6)
                            player.UnSetPerk("specialty_stalker");
                        player.SetField("perk" + i + "bought", false);
                    }
                }
                player.SetField("totalPerkCount", 0);
                hud.updatePerksHud(player, true);
            }

            HudElem reviveIcon = hud.createReviveHeadIcon(player);
            reviveIcon.Color = new Vector3(1, 1, 1);

            IPrintLn(string.Format("^1{0} ^1needs to be revived!", player.Name));

            //Entity reviver = Spawn("script_model", player.Origin);
            //reviver.SetModel("tag_origin");
            Entity reviver = Spawn("script_origin", player.Origin);
            reviver.LinkTo(player);
            reviver.SetField("usableType", "revive");
            reviver.SetField("range", 60);
            reviver.SetField("player", player);
            reviver.SetField("icon", reviveIcon);
            reviver.SetField("user", reviver);
            //reviver.SetField("isBeingUsed", false);
            //player.HeadIcon = "waypoint_revive";
            //player.HeadIconTeam = "allies";
            mapEdit.makeUsable(reviver, "revive", 50);

            if (!player.HasWeapon("iw5_usp45_mp"))
                player.GiveWeapon("iw5_usp45_mp");
            player.SwitchToWeaponImmediate("iw5_usp45_mp");

            //float red = 1f;
            player.SetField("deathCount", 0);
            OnInterval(1000, () =>
                startDeathCountdown(player, reviveIcon, reviver));
        }
        private static bool startDeathCountdown(Entity player, HudElem reviveIcon, Entity reviver)
        {
            if (gameEnded)
            {
                reviveIcon.Destroy();
                return false;
            }

            if (reviver.GetField<Entity>("user") != reviver) return true;

            if (!player.GetField<bool>("isDown")) return false;

            int deathCount = player.GetField<int>("deathCount");

            if (isPlayer(player)) player.PingPlayer();
            deathCount++;
            player.SetField("deathCount", deathCount);

            if (deathCount == 15 && player.GetField<bool>("isDown")) player.VisionSetNakedForPlayer("cheat_bw", 15);

            if (deathCount > 15)//Tint icon red
            {
                if (reviveIcon.Color.Y >= .05f)
                    reviveIcon.Color -= new Vector3(0, .05f, .05f);
            }

            if (deathCount == 30)
            {
                player.Suicide();
                //Take score from other players
                foreach (Entity players in Players)
                {
                    if (players.HasField("isDown") && players.IsAlive)
                    {
                        if (players.GetField<bool>("isDown")) continue;//Don't punish downed players
                        if (players.SessionState != "playing") continue;//Or spectators

                        int amount = players.Score / 15;//Take a percent away
                        amount -= amount % 10;//Remove the difference
                        players.SetField("cash", players.GetField<int>("cash") - amount);
                        if (players.GetField<int>("cash") < 0) players.SetField("cash", 0);
                        hud.scorePopup(players, -amount);
                        hud.scoreMessage(players, "^1Failed to revive " + player.Name);
                    }
                    else continue;
                }
            }

            if (!player.IsAlive)//Check for death after suicide
            {
                mapEdit.removeUsable(reviver);
                reviveIcon.Destroy();
                return false;
            }
            if (player.GetField<bool>("isDown") && isPlayer(player)) return true;
            return false;
        }

        private static void onPlayerDeath(Entity player)
        {
            if (player.HasField("bot") && player.GetField<Entity>("bot").GetField<string>("state") != "dead") killstreaks.killPlayerBotOnDeath(player);
            if (!isHellMap || (isHellMap && killstreaks.visionRestored)) player.VisionSetNakedForPlayer(vision);

            /* removing this for now to save processing. This is run on onPlayerDown
            for (int i = 1; i < 7; i++)
            {
                bool hasPerk = player.GetField<bool>("perk" + i + "bought");
                if (hasPerk)
                    player.SetField("perk" + i + "bought", false);
            }
            */

            player.SetField("autoRevive", false);
            hud.updatePerksHud(player, true);
            if (player.HasField("aizHud_created"))
            {
                HudElem ksList = player.GetField<HudElem>("hud_killstreakList");
                ksList.SetText("");
                ksList.SetField("text", "");
                HudElem message = player.GetField<HudElem>("hud_message");
                message.SetText("");
            }

            /*
            if (getZombieMapname() == "Moonbase")
            {
                if (player.HasField("hasHelmetOn"))
                {
                    HudElem helmet = player.GetField<HudElem>("hud_helmet");
                    helmet.Destroy();
                    player.SetField("hasHelmetOn", false);
                }
                if (player.HasField("helmet")) player.ClearField("helmet");
            }
            */

            updatePlayerCountForScoreboard();

            clearPlayerWeaponsList(player);

            player.SetField("isDown", true);//Just in case it doesn't get set prior to this stage

            AfterDelay(500, () => player.IPrintLnBold("^1You have died. Wait until the next round to respawn."));

            IPrintLn(string.Format("^1{0} ^1has been killed.", player.Name));

            AfterDelay(250, () =>
                StartAsync(setPlayerAsSpectator(player)));

            AfterDelay(1000, () =>
                checkForPlayerRespawn(player));
        }
        private static IEnumerator setPlayerAsSpectator(Entity player)
        {
            yield return Wait(.2f);
            player.SetClientDvar("g_scriptMainMenu", pauseMenu);
            yield return Wait(.3f);
            player.SessionState = "spectator";
            //player.Notify("menuresponse", "team_marinesopfor", "spectator");
            //AfterDelay(100, () => player.CloseMenu("changeclass"));
        }
        private static void checkForPlayerRespawn(Entity player)
        {
            if (zState == "intermission")
                spawnPlayer(player);
            else roundSystem.checkForEndGame();
        }

        private static void autoRevive_revivePlayer(Entity player, HudElem overlay)
        {
            overlay.Destroy();
            if (!player.IsAlive || !isPlayer(player)) return;
            player.LastStandRevive();
            player.SetField("isDown", false);
            player.SetField("autoRevive", false);
            player.EnableWeaponSwitch();
            player.EnableOffhandWeapons();
            List<string> weaponList = player.GetField<List<string>>("weaponsList");
            if (!weaponList.Contains("iw5_usp45_mp"))
                player.TakeWeapon("iw5_usp45_mp");
            if (player.GetField<bool>("perk1bought"))
                player.Health = maxPlayerHealth_Jugg;
            else player.Health = maxPlayerHealth;
            //e_hud.updatePerksHud(player, false);
            player.SetField("perk7HudDone", false);
            if (player.HasField("hud_perk7"))
            {
                player.GetField<HudElem>("hud_perk7").Alpha = 0;
                player.GetField<HudElem>("hud_perk7").ScaleOverTime(.5f, 40, 40);
            }
            player.SetField("totalPerkCount", player.GetField<int>("totalPerkCount") - 1);
            if (weaponList.Count != 0) player.SwitchToWeapon(weaponList[0]);
        }
        private static bool autoRevive_pulseIcon(Entity player, HudElem pulse)
        {
            if (gameEnded) return false;
            pulse.Alpha = .9f;
            pulse.ScaleOverTime(.6f, 100, 100);
            pulse.FadeOverTime(.6f);
            pulse.Alpha = 0;
            AfterDelay(650, () => pulse.SetShader("waypoint_revive", 30, 30));
            if (player.GetField<bool>("isDown") && isPlayer(player) && player.IsAlive)
                return true;
            else
            {
                pulse.Destroy();
                return false;
            }
        }

        private static void loadConfig()
        {
            if (!File.Exists("scripts\\aizombies\\config.cfg"))
            {
                printToConsole("Configuration file for AIZombies was not found! Creating one...");
                using (StreamWriter newCfg = new StreamWriter("scripts\\aizombies\\config.cfg"))
                {
                    newCfg.WriteLine("//AIZombies Supreme v{0} Config File//", version);
                    //newCfg.WriteLine("Waves: {0} //The max amount of waves to play in a game.", roundSystem.totalWaves);
                    newCfg.WriteLine("Spawn Weapon System: {0} //The type of weapon spawn. Valid options are 'Normal' and 'Random'.", spawnType == 1 ? "Random" : "Normal");
                    newCfg.WriteLine("Max Health: {0} //The normal max player health.", maxPlayerHealth);
                    newCfg.WriteLine("Max Juggernog Health: {0} //The max player health with juggernog.", maxPlayerHealth_Jugg);
                    newCfg.WriteLine("Bot Starting Health: {0} //The starting health of a bot.", botUtil.health);
                    newCfg.WriteLine("Crawler Health: {0} //The health of a crawler bot.", botUtil.crawlerHealth);
                    newCfg.WriteLine("Boss Health: {0} //The health of a boss bot.", botUtil.bossHealth);
                    newCfg.WriteLine("Bot Health Factor: {0} //The amount of health to add to bots every round", botUtil.healthScalar);
                    newCfg.WriteLine("Bot Damage: {0} //The amount of damage a bot does to a player", botUtil.dmg);
                    newCfg.WriteLine("Perk Drops: {0} //Allow perk bonus drops at the end of crawler rounds on hell maps", botUtil.perkDropsEnabled ? "Enabled" : "Disabled");
                    newCfg.WriteLine("Map Voting: {0} //Enable or disable voting for the next map after a game has ended.", voting ? "Enabled" : "Disabled");
                    newCfg.WriteLine("DLC Maps: {0} //Enable or disable dlc maps in map voting.", dlcEnabled ? "Enabled" : "Disabled");
                    //newCfg.WriteLine("Alternate Weapon Names: {0} //Use alternate names for upgraded weapons.", altWeaponNames ? "Enabled" : "Disabled");
                    newCfg.WriteLine("Perk Limit: {0} //The max amount of perks a player can buy. 0 is no limit.", perkLimit);
                    newCfg.WriteLine("Auto Updates: {0} //Enable or disable auto updates for AIZombies", autoUpdate ? "Enabled" : "Disabled");
                    newCfg.WriteLine("Custom Server Gametype: {0} //Enable or disable the server displaying 'AIZombies' as the gametype in the server browser. Disable this if you experience crashing on startup.", allowServerGametypeHack ? "Enabled" : "Disabled");
                    newCfg.WriteLine("Custom Gametype: {0} //Enable or disable the server displaying 'AIZombies Supreme' as the gametype in-game. Disable this if you experience frequent crashes.", allowGametypeHack ? "Enabled" : "Disabled");
                    newCfg.Flush();
                    newCfg.Close();
                    newCfg.Dispose();
                }
                return;
            }

            bool cfgIsCurrent = true;
            StreamReader cfg = new StreamReader("scripts\\aizombies\\config.cfg");
            string text = cfg.ReadToEnd();
            cfg.Close();
            cfg.Dispose();
            if (!text.Split('\n')[0].StartsWith("//AIZombies Supreme v" + version.ToString() + " Config File//"))
                cfgIsCurrent = false;
            string[] lines = formatCFGString(text);
            foreach (string line in lines)
            {
                if (line.StartsWith("//") || line.Split(':').Length > 2 || line == string.Empty) continue;
                string label = line.Split(':')[0];
                string value = line.Split(':')[1];

                setGameSetting(label, value);
            }

            if (!cfgIsCurrent)//Re-write the cfg in case any new entries were added
            {
                using (StreamWriter newCfg = new StreamWriter("scripts\\aizombies\\config.cfg", false))
                {
                    newCfg.WriteLine("//AIZombies Supreme v{0} Config File//", version);
                    //newCfg.WriteLine("Waves: {0} //The max amount of waves to play in a game.", roundSystem.totalWaves);
                    newCfg.WriteLine("Spawn Weapon System: {0} //The type of weapon spawn. Valid options are 'Normal' and 'Random'.", spawnType == 1 ? "Random" : "Normal");
                    newCfg.WriteLine("Max Health: {0} //The normal max player health.", maxPlayerHealth);
                    newCfg.WriteLine("Max Juggernog Health: {0} //The max player health with juggernog.", maxPlayerHealth_Jugg);
                    newCfg.WriteLine("Bot Starting Health: {0} //The starting health of a bot.", botUtil.health);
                    newCfg.WriteLine("Crawler Health: {0} //The health of a crawler bot.", botUtil.crawlerHealth);
                    newCfg.WriteLine("Boss Health: {0} //The health of a boss bot.", botUtil.bossHealth);
                    newCfg.WriteLine("Bot Health Factor: {0} //The amount of health to add to bots every round", botUtil.healthScalar);
                    newCfg.WriteLine("Bot Damage: {0} //The amount of damage a bot does to a player", botUtil.dmg);
                    newCfg.WriteLine("Perk Drops: {0} //Allow perk bonus drops at the end of crawler rounds on hell maps", botUtil.perkDropsEnabled ? "Enabled" : "Disabled");
                    newCfg.WriteLine("Map Voting: {0} //Enable or disable voting for the next map after a game has ended.", voting ? "Enabled" : "Disabled");
                    newCfg.WriteLine("DLC Maps: {0} //Enable or disable dlc maps in map voting.", dlcEnabled ? "Enabled" : "Disabled");
                    //newCfg.WriteLine("Alternate Weapon Names: {0} //Use alternate names for upgraded weapons.", altWeaponNames ? "Enabled" : "Disabled");
                    newCfg.WriteLine("Perk Limit: {0} //The max amount of perks a player can buy. 0 is no limit.", perkLimit);
                    newCfg.WriteLine("Auto Updates: {0} //Enable or disable auto updates for AIZombies", autoUpdate ? "Enabled" : "Disabled");
                    newCfg.WriteLine("Custom Server Gametype: {0} //Enable or disable the server displaying 'AIZombies' as the gametype in the server browser. Disable this if you experience crashing on startup.", allowServerGametypeHack ? "Enabled" : "Disabled");
                    newCfg.WriteLine("Custom Gametype: {0} //Enable or disable the server displaying 'AIZombies Supreme' as the gametype in-game. Disable this if you experience frequent crashes.", allowGametypeHack ? "Enabled" : "Disabled");
                    newCfg.Flush();
                    newCfg.Close();
                    newCfg.Dispose();
                }
            }
        }

        public static void setGameSetting(string setting, string value)
        {
            int set;
            switch (setting)
            {
                case "spawnweaponsystem":
                    if (value == "normal")
                        spawnType = 0;
                    else if (value == "random")
                        spawnType = 1;
                    break;
                case "maxhealth":
                    if (int.TryParse(value, out set))
                        maxPlayerHealth = set;
                    else printToConsole("Max Health was set to an incorrect value in the cfg!, Set to default ({0})", maxPlayerHealth);
                    break;
                case "maxjuggernauthealth":
                    if (int.TryParse(value, out set))
                        maxPlayerHealth_Jugg = set;
                    else printToConsole("Max Juggernog Health was set to an incorrect value in the cfg!, Set to default ({0})", maxPlayerHealth_Jugg);
                    break;
                case "botstartinghealth":
                    if (int.TryParse(value, out set))
                        botUtil.health = set;
                    else printToConsole("Bot Health was set to an incorrect value in the cfg!, Set to default ({0})", botUtil.health);
                    break;
                case "crawlerhealth":
                    if (int.TryParse(value, out set))
                        botUtil.crawlerHealth = set;
                    else printToConsole("Crawler Health was set to an incorrect value in the cfg!, Set to default ({0})", botUtil.crawlerHealth);
                    break;
                case "bosshealth":
                    if (int.TryParse(value, out set))
                        botUtil.bossHealth = set;
                    else printToConsole("Boss Health was set to an incorrect value in the cfg!, Set to default ({0})", botUtil.bossHealth);
                    break;
                case "bothealthfactor":
                    if (int.TryParse(value, out set))
                        botUtil.healthScalar = set;
                    else printToConsole("Bot Health Factor was set to an incorrect value in the cfg!, Set to default ({0})", botUtil.healthScalar);
                    break;
                case "botdamage":
                    if (int.TryParse(value, out set))
                        botUtil.dmg = set;
                    else printToConsole("Bot Damage was set to an incorrect value in the cfg!, Set to default ({0})", botUtil.dmg);
                    break;
                case "perkdrops":
                    if (value == "enabled")
                        botUtil.perkDropsEnabled = true;
                    else if (value == "disabled")
                        botUtil.perkDropsEnabled = false;
                    break;
                case "mapvoting":
                    if (value == "enabled")
                        voting = true;
                    else if (value == "disabled")
                        voting = false;
                    break;
                case "dlcmaps":
                    if (value == "enabled")
                        dlcEnabled = true;
                    else if (value == "disabled")
                        dlcEnabled = false;
                    break;
                    /*
                case "altweaponnames":
                    if (value == "enabled")
                        altWeaponNames = true;
                    else if (value == "disabled")
                        altWeaponNames = false;
                    break;
                    */
                case "perklimit":
                    byte setB;
                    if (byte.TryParse(value, out setB))
                        perkLimit = setB;
                    else printToConsole("Perk Limit was set to an incorrect value in the cfg!, Set to default ({0})", perkLimit);
                    break;
                case "autoupdates":
                    if (value == "enabled")
                        autoUpdate = true;
                    else if (value == "disabled")
                        autoUpdate = false;
                    break;
                case "customservergametype":
                    if (value == "enabled")
                        allowServerGametypeHack = true;
                    else if (value == "disabled")
                        allowServerGametypeHack = false;
                    break;
                case "customgametype":
                    if (value == "enabled")
                        allowGametypeHack = true;
                    else if (value == "disabled")
                        allowGametypeHack = false;
                    break;
            }
        }

        public static void printToConsole(string format, params Parameter[] p)
        {
            if (p.Length > 0)
                Utilities.PrintToConsole(string.Format(format, p));
            else Utilities.PrintToConsole(format);
        }

        public static string clipSpaces(string input)
             => input.Replace(" ", "");

        private static string[] formatCFGString(string cfg)
        {
            string text = clipSpaces(cfg);
            string[] lines = text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("//"))
                { lines[i] = string.Empty; continue; }

                lines[i] = lines[i].Split('/')[0];
                lines[i] = lines[i].ToLowerInvariant();
                //lines[i].Replace("//", string.Empty);
            }
            return lines;
        }

        public static Vector3 parseVec3(string vec3)
        {
            vec3 = vec3.Replace(" ", string.Empty);
            if (!vec3.StartsWith("(") && !vec3.EndsWith(")")) printToConsole("Vector was not formatted correctly! Vector: " + vec3);
            vec3 = vec3.Replace("(", string.Empty);
            vec3 = vec3.Replace(")", string.Empty);
            string[] split = vec3.Split(',');
            if (split.Length < 3) printToConsole("Vector was not formatted correctly! Vector: " + vec3);
            Vector3 ret = new Vector3(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2]));
            return ret;
        }

        public static Entity getPlayerWithMostKills()
        {
            int score = 0;
            Entity currentPlayer = null;
            foreach (Entity player in Players)
            {
                if (isPlayer(player) && player.Score > score)
                {
                    score = player.Score;
                    currentPlayer = player;
                }
            }
            if (currentPlayer != null) return currentPlayer;
            else
                return GetEnt("mp_global_intermission", "classname");
        }
        private static Entity getRandomSpawnpoint()
        {
            Entity ret = null;
            for (int i = 0; i < 700; i++)
            {
                Entity e = Entity.GetEntity(i);
                if (e == null) continue;
                if (e.Classname == "mp_tdm_spawn")
                {
                    ret = e;
                    if (rng.Next(100) > 50) break;
                }
                else continue;
            }
            return ret;
        }
        public static Entity[] getAllEntitiesWithName(string targetname)
        {
            int entCount = GetEntArray(targetname, "targetname").GetHashCode();
            Entity[] ret = new Entity[entCount];
            int count = 0;
            for (int i = 0; i < 2000; i++)
            {
                Entity e = Entity.GetEntity(i);
                string t = e.TargetName;
                if (t == targetname) ret[count] = e;
                else continue;
                count++;
                if (count == entCount) break;
            }
            return ret;
        }
        public static string getZombieMapname()
        {
            //Pass through a custom name first before setting defaults
            if (zombieMapname != null) return zombieMapname;
            //If there's no custom, set defaults
            else
            {
                if (_mapname == "mp_alpha") return "Streets of Death";
                else if (_mapname == "mp_bootleg") return "Stormy Outbreak";
                else if (_mapname == "mp_bravo") return "Rundown Village";
                else if (_mapname == "mp_carbon") return "Oil Refinery";
                else if (_mapname == "mp_dome") return "Desert Outpost";
                else if (_mapname == "mp_exchange") return "Abandoned Subway";
                else if (_mapname == "mp_hardhat") return "Construction Site Of Hell";
                else if (_mapname == "mp_interchange") return "Demolished Underpass";
                else if (_mapname == "mp_lambeth") return "Abandoned Road Of Hell";
                else if (_mapname == "mp_mogadishu") return "Docked Death";
                else if (_mapname == "mp_paris") return "Death Alley";
                else if (_mapname == "mp_plaza2") return "Bone Appetite";
                else if (_mapname == "mp_radar") return "Storage Unit Of Hell";
                else if (_mapname == "mp_seatown") return "Seaside Hotel Of Hell";
                else if (_mapname == "mp_underground") return "Car Lot Of Pain";
                else if (_mapname == "mp_village") return "Big Black Death";
                else if (_mapname == "mp_italy") return "El Brote";
                else if (_mapname == "mp_park") return "Wartorn";
                else if (_mapname == "mp_morningwood") return "Under Contruction";
                else if (_mapname == "mp_overwatch") return "Death In Progress";
                else if (_mapname == "mp_aground_ss") return "Shipwrecked";
                else if (_mapname == "mp_courtyard_ss") return "Dead Aqueduct";
                else if (_mapname == "mp_cement") return "Silla Cement";
                else if (_mapname == "mp_hillside_ss") return "Oceanside Attack";
                else if (_mapname == "mp_meteora") return "Sanctuary Of Hell";
                else if (_mapname == "mp_qadeem") return "Paradise";
                else if (_mapname == "mp_restrepo_ss") return "Rest Site Of Hell";
                else if (_mapname == "mp_terminal_cls") return "Death Row";
                else if (_mapname == "mp_crosswalk_ss") return "Pandemic Bridge";
                else if (_mapname == "mp_six_ss") return "Undead Farm";
                else if (_mapname == "mp_burn_ss") return "Hideout of Hell";
                else if (_mapname == "mp_shipbreaker") return "Shantytown";
                else if (_mapname == "mp_roughneck") return "Oil Rig";
                else if (_mapname == "mp_nola") return "Death Avenue";
                else if (_mapname == "mp_moab") return "Trapped Canyon";
                else if (_mapname == "mp_boardwalk") return "The Pier";
                else return "^1Unknown Map!";
            }
        }

        private static string getMapname()
            => GetDvar("mapname");

        public static string getMW2Vision()
        {
            switch (_mapname)
            {
                case "mp_alpha":
                    return "mp_citystreets";
                case "mp_bootleg":
                    return "mp_carentan";
                case "mp_dome":
                    return "mp_rust";
                case "mp_exchange":
                    return "mp_verdict";
                case "mp_hardhat":
                    //return "mp_nightshift";
                    return "af_caves_indoors";
                case "mp_interchange":
                    return "mp_bog";
                case "mp_lambeth":
                    return "mp_overgrown";
                case "mp_paris":
                    return "mp_crash";
                case "mp_plaza2":
                    return "mp_broadcast";
                case "mp_radar":
                    return "mp_derail";
                case "mp_underground":
                    return "mp_convoy";
                case "mp_cement":
                    return "mp_pipeline";
                case "mp_hillside_ss":
                    return "mp_strike";
                case "mp_overwatch":
                    return "mp_highrise";
                case "mp_park":
                    return "mp_creek";
                case "mp_restrepo_ss":
                    return "mp_hill";
                case "mp_terminal_cls":
                    return "airport";
                case "mp_roughneck":
                    return "mp_oilrig";
                case "mp_boardwalk":
                    return "mp_trailer";
                case "mp_moab":
                    return "mp_dusk";
                case "mp_nola":
                    return "mp_suburbia";
                case "mp_bravo":
                    return "mp_killhouse";
                case "mp_carbon":
                    return "mp_countdown";
                case "mp_mogadishu":
                    return "mp_shipment";
                case "mp_village":
                    return "mp_brecourt";
                case "mp_shipbreaker":
                    return "mp_creek_ss";
                case "mp_seatown":
                    return "mp_backlot";
                case "mp_aground_ss":
                    return "mp_riverwalk";
                case "mp_courtyard_ss":
                    return "mp_showdown";
                case "mp_meteora":
                    return "mp_riverwalk";
                case "mp_morningwood":
                    return "mp_skidrow";
                case "mp_qadeem":
                    return "mp_quarry";
                case "mp_italy":
                    return "mp_favela";
                case "mp_six_ss":
                    return "mp_farm";
                case "mp_burn_ss":
                    return "mp_firingrange";
                case "mp_crosswalk_ss":
                    return "mp_nightshift";
                default:
                    return "";
            }
        }

        public static bool weaponIsUpgrade(string weapon)
        {
            weapon = trimWeaponScope(weapon);
            if (weapon == "iw5_scar_mp_eotech_xmags_camo11") return true;
            else if (weapon == "iw5_mp5_mp_reflexsmg_xmags_camo11") return true;
            else if (weapon == "iw5_ak47_mp_gp25_xmags_camo11") return true;
            else if (weapon == "alt_iw5_ak47_mp_gp25_xmags_camo11") return true;
            else if (weapon == "iw5_m60jugg_mp_silencer_thermal_camo08") return true;
            else if (weapon == "iw5_mp412jugg_mp_xmags") return true;
            else if (weapon == "iw5_deserteagle_mp_silencer02_xmags") return true;
            else if (weapon == "iw5_usp45_mp_akimbo_silencer02") return true;
            else if (weapon == "iw5_p90_mp_rof_xmags_camo11") return true;
            else if (weapon == "iw5_m60_mp_reflexlmg_xmags_camo11") return true;
            else if (weapon == "iw5_as50_mp_acog_xmags_camo11") return true;
            else if (weapon == "iw5_msr_mp_msrscope_silencer03_xmags_camo11") return true;
            else if (weapon == "iw5_aa12_mp_grip_xmags_camo11") return true;
            else if (weapon == "iw5_1887_mp_camo11") return true;
            else if (weapon == "iw5_skorpion_mp_akimbo_xmags") return true;
            else if (weapon == "iw5_mp9_mp_reflexsmg_xmags") return true;
            else if (weapon == "iw5_p99_mp_tactical_xmags") return true;
            else if (weapon == "iw5_fnfiveseven_mp_akimbo_xmags") return true;
            else if (weapon == "iw5_44magnum_mp_akimbo_xmags") return true;
            else if (weapon == "iw5_fmg9_mp_akimbo_xmags") return true;
            else if (weapon == "iw5_g18_mp_akimbo_xmags") return true;
            else if (weapon == "rpg_mp") return true;
            else if (weapon == "uav_strike_marker_mp") return true;
            else if (weapon == "gl_mp") return true;
            else if (weapon == "xm25_mp") return true;
            else if (weapon == "iw5_m4_mp_reflex_xmags_camo11") return true;
            else if (weapon == "iw5_m16_mp_rof_xmags_camo11") return true;
            else if (weapon == "iw5_cm901_mp_acog_xmags_camo11") return true;
            else if (weapon == "iw5_type95_mp_reflex_xmags_camo11") return true;
            else if (weapon == "iw5_acr_mp_eotech_xmags_camo11") return true;
            else if (weapon == "iw5_mk14_mp_reflex_xmags_camo11") return true;
            else if (weapon == "iw5_g36c_mp_hybrid_xmags_camo11") return true;
            else if (weapon == "alt_iw5_g36c_mp_hybrid_xmags_camo11") return true;
            else if (weapon == "iw5_fad_mp_m320_xmags_camo11") return true;
            else if (weapon == "alt_iw5_fad_mp_m320_xmags_camo11") return true;
            else if (weapon == "iw5_ump45_mp_eotechsmg_xmags_camo11") return true;
            else if (weapon == "iw5_pp90m1_mp_silencer_xmags_camo11") return true;
            else if (weapon == "iw5_m9_mp_thermalsmg_xmags_camo11") return true;
            else if (weapon == "iw5_mp7_mp_silencer_xmags_camo11") return true;
            else if (weapon == "iw5_dragunov_mp_acog_xmags_camo11") return true;
            else if (weapon == "iw5_barrett_mp_acog_xmags_camo11") return true;
            else if (weapon == "iw5_l96a1_mp_l96a1scopevz_xmags_camo11") return true;
            else if (weapon == "iw5_rsass_mp_thermal_xmags_camo11") return true;
            else if (weapon == "iw5_sa80_mp_reflexlmg_xmags_camo11") return true;
            else if (weapon == "iw5_mg36_mp_grip_xmags_camo11") return true;
            else if (weapon == "iw5_pecheneg_mp_thermal_xmags_camo11") return true;
            else if (weapon == "iw5_mk46_mp_silencer_xmags_camo11") return true;
            else if (weapon == "iw5_usas12_mp_reflex_xmags_camo11") return true;
            else if (weapon == "iw5_ksg_mp_grip_xmags_camo11") return true;
            else if (weapon == "iw5_spas12_mp_grip_xmags_camo11") return true;
            else if (weapon == "iw5_striker_mp_grip_xmags_camo11") return true;
            else if (weapon == "iw5_skorpion_mp_eotechsmg_xmags") return true;
            else if (weapon == "iw5_riotshield_mp") return true;
            else if (weapon == "iw5_pecheneg_mp_rof_thermal") return true;
            else return false;
        }

        public static string getWeaponUpgrade(string weapon)
        {
            if (weapon == "iw5_scar_mp") return "iw5_scar_mp_eotech_xmags_camo11";
            else if (weapon == "iw5_mp5_mp") return "iw5_mp5_mp_reflexsmg_xmags_camo11";
            else if (weapon == "iw5_ak47_mp") return "iw5_ak47_mp_gp25_xmags_camo11";
            else if (weapon == "iw5_m60jugg_mp_eotechlmg_camo07") return "iw5_m60jugg_mp_silencer_thermal_camo08";
            else if (weapon == "iw5_mp412_mp") return "iw5_mp412jugg_mp_xmags";
            else if (weapon == "iw5_deserteagle_mp") return "iw5_deserteagle_mp_silencer02_xmags";
            else if (weapon == "iw5_usp45_mp") return "iw5_usp45_mp_silencer02_akimbo";
            else if (weapon == "iw5_p90_mp") return "iw5_p90_mp_rof_xmags_camo11";
            else if (weapon == "iw5_m60_mp") return "iw5_m60_mp_reflexlmg_xmags_camo11";
            else if (weapon == "iw5_as50_mp_as50scope") return "iw5_as50_mp_acog_xmags_camo11";
            else if (weapon == "iw5_msr_mp_msrscope") return "iw5_msr_mp_msrscope_silencer03_xmags_camo11";
            else if (weapon == "iw5_aa12_mp") return "iw5_aa12_mp_grip_xmags_camo11";
            else if (weapon == "iw5_1887_mp") return "iw5_1887_mp_camo11";
            else if (weapon == "iw5_skorpion_mp") return "iw5_skorpion_mp_akimbo_xmags";
            else if (weapon == "iw5_mp9_mp") return "iw5_mp9_mp_reflexsmg_xmags";
            else if (weapon == "iw5_p99_mp") return "iw5_p99_mp_tactical_xmags";
            else if (weapon == "iw5_fnfiveseven_mp") return "iw5_fnfiveseven_mp_akimbo_xmags";
            else if (weapon == "iw5_44magnum_mp") return "iw5_44magnum_mp_akimbo_xmags";
            else if (weapon == "iw5_fmg9_mp") return "iw5_fmg9_mp_akimbo_xmags";
            else if (weapon == "iw5_g18_mp") return "iw5_g18_mp_akimbo_xmags";
            else if (weapon == "iw5_smaw_mp") return "rpg_mp";
            else if (weapon == "rpg_mp") return "at4_mp";
            else if (weapon == "iw5_xm25_mp") return "xm25_mp";
            else if (weapon == "xm25_mp") return "uav_strike_marker_mp";
            else if (weapon == "m320_mp") return "gl_mp";
            else if (weapon == "iw5_m4_mp") return "iw5_m4_mp_reflex_xmags_camo11";
            else if (weapon == "iw5_m16_mp") return "iw5_m16_mp_rof_xmags_camo11";
            else if (weapon == "iw5_cm901_mp") return "iw5_cm901_mp_acog_xmags_camo11";
            else if (weapon == "iw5_type95_mp") return "iw5_type95_mp_reflex_xmags_camo11";
            else if (weapon == "iw5_acr_mp") return "iw5_acr_mp_eotech_xmags_camo11";
            else if (weapon == "iw5_mk14_mp") return "iw5_mk14_mp_reflex_xmags_camo11";
            else if (weapon == "iw5_g36c_mp") return "iw5_g36c_mp_hybrid_xmags_camo11";
            else if (weapon == "iw5_fad_mp") return "iw5_fad_mp_m320_xmags_camo11";
            else if (weapon == "iw5_ump45_mp") return "iw5_ump45_mp_eotechsmg_xmags_camo11";
            else if (weapon == "iw5_pp90m1_mp") return "iw5_pp90m1_mp_silencer_xmags_camo11";
            else if (weapon == "iw5_m9_mp") return "iw5_m9_mp_thermalsmg_xmags_camo11";
            else if (weapon == "iw5_mp7_mp") return "iw5_mp7_mp_silencer_xmags_camo11";
            else if (weapon == "iw5_dragunov_mp_dragunovscope") return "iw5_dragunov_mp_acog_xmags_camo11";
            else if (weapon == "iw5_barrett_mp_barrettscope") return "iw5_barrett_mp_acog_xmags_camo11";
            else if (weapon == "iw5_l96a1_mp_l96a1scope") return "iw5_l96a1_mp_l96a1scopevz_xmags_camo11";
            else if (weapon == "iw5_rsass_mp_rsassscope") return "iw5_rsass_mp_thermal_xmags_camo11";
            else if (weapon == "iw5_sa80_mp") return "iw5_sa80_mp_reflexlmg_xmags_camo11";
            else if (weapon == "iw5_mg36_mp") return "iw5_mg36_mp_grip_xmags_camo11";
            else if (weapon == "iw5_pecheneg_mp") return "iw5_pecheneg_mp_thermal_xmags_camo11";
            else if (weapon == "iw5_mk46_mp") return "iw5_mk46_mp_silencer_xmags_camo11";
            else if (weapon == "iw5_usas12_mp") return "iw5_usas12_mp_reflex_xmags_camo11";
            else if (weapon == "iw5_ksg_mp") return "iw5_ksg_mp_grip_xmags_camo11";
            else if (weapon == "iw5_spas12_mp") return "iw5_spas12_mp_grip_xmags_camo11";
            else if (weapon == "iw5_striker_mp") return "iw5_striker_mp_grip_xmags_camo11";
            else if (weapon == "iw5_skorpion_mp_eotechsmg_scope7") return "iw5_skorpion_mp_eotechsmg_xmags_scope7";
            else if (weapon == "riotshield_mp") return "iw5_riotshield_mp";
            else if (weapon == "scrambler_mp") return "iw5_riotshieldjugg_mp";
            else if (weapon == "uav_strike_missile_mp") return "uav_strike_projectile_mp";
            else return string.Empty;
        }

        public static string getWeaponUpgradeModel(string weapon)
        {
            if (weapon == "iw5_m60jugg_mp_eotechlmg_camo07") return "weapon_steyr_orange_fall";
            else if (weapon == "riotshield_mp") return "weapon_riot_shield_mp";
            else if (weapon == "iw5_smaw_mp") return "weapon_rpg7";
            else if (weapon == "xm25_mp") return "weapon_fn2000";
            else if (weapon == "m320_mp") return "weapon_m16";
            else if (weapon == "rpg_mp") return "weapon_at4";
            else if (weapon == "uav_strike_missile_mp") return "weapon_javelin";
            else return GetWeaponModel(weapon, 11);
        }
        /*
        public static string getWeaponClipModel(string weapon)
        {
            if (weapon == "rpg_mp") return "projectile_rpg7";
            else if (weapon == "iw5_smaw_mp") return "projectile_smaw";
            else if (weapon == "xm25_mp") return "projectile_m203grenade";
            else if (weapon == "m320_mp") return "projectile_m203grenade";
            else if (weapon == "at4_mp") return "projectile_at4";
            else if (weapon == "uav_strike_missile_mp") return "projectile_smartarrow";
            switch (WeaponClass(weapon))
            {
                case "mg":
                    return "weapon_m60_clip_iw5";
                case "rocketlauncher":
                    return "projectile_rpg7";
                case "sniper":
                    return "weapon_rsass_clip_iw5";
                case "smg":
                    return "weapon_mp5_clip";
                case "rifle":
                    return "weapon_ak47_tactical_clip";
            }
            return "tag_origin";
        }
        */

        public static bool isWeaponDeathMachine(string weapon)
            => weapon == "iw5_pecheneg_mp_rof_thermal";

        public static bool isRayGun(string weapon)
            => (weapon == "iw5_skorpion_mp_eotechsmg_scope7" || weapon == "iw5_skorpion_mp_eotechsmg_xmags_scope7");

        public static bool isThunderGun(string weapon)
            => (weapon == "uav_strike_missile_mp" || weapon == "uav_strike_projectile_mp");

        public static bool isSpecialWeapon(string weapon)
        {
            if (weapon == "riotshield_mp") return true;
            if (weapon == "scrambler_mp") return true;
            if (weapon == "iw5_riotshield_mp") return true;
            if (isKillstreakWeapon(weapon)) return true;
            if (weapon == "trophy_mp") return true;
            if (weapon == "uav_strike_marker_mp") return true;
            if (weapon == "none") return true;
            return false;
        }
        public static bool isKillstreakWeapon(string weapon)
        {
            if (weapon == "airdrop_marker_mp") return true;
            if (weapon == "airdrop_trap_marker_mp") return true;
            if (weapon == "strike_marker_mp") return true;
            if (weapon == "deployable_vest_marker_mp") return true;
            if (weapon.StartsWith("killstreak_")) return true;
            return false;
        }

        public static bool isSniper(string weapon)
            => WeaponClass(weapon) == "sniper";

        public static bool isShotgun(string weapon)
            => WeaponClass(weapon) == "spread";

        public static bool isGlowstick(Entity entity)
            => entity.HasField("isGlowstick");

        public static bool isPlayer(Entity player)
            => player.Classname == "player";

        public static string[] getWeaponAttachments(string weapon)
        {
            string[] attachments = new string[2] { "", "" };

            string[] tokens = weapon.Split('_');
            if (tokens[0] == "iw5")
            {
                if (tokens.Length < 4) return attachments;
                if (tokens[3].Contains("camo")) return attachments;

                if (isWeaponAttachment(tokens[3])) attachments[0] = tokens[3];

                if (tokens.Length < 5) return attachments;
                if (tokens[4].Contains("camo")) return attachments;

                if (isWeaponAttachment(tokens[4])) attachments[1] = tokens[4];
            }
            return attachments;
        }

        public static bool isWeaponAttachment(string attachment)
        {
            string[] attachments = { "reflex", "reflexlmg", "reflexsmg", "acog", "acogsmg", "acoglmg", "grip", "akimbo", "thermal", "thermalsmg", "thermal", "shotgun", "heartbeat", "xmags", "rof", "eotech", "eotechsmg", "eotechlmg", "tactical", "vzscope", "scopevz", "gl", "gp25", "m320", "silencer", "silencer02", "silencer03", "hamrhybrid", "hybrid",
            "dragunovscope", "dragunovscopevz", "as50scope", "as50scopevz", "msrscope", "msrscopevz", "l96a1scope", "l96a1scopevz", "rsassscope", "rsassscopevz", "barrettscope", "barrettscopevz"};
            if (attachments.Contains(attachment))
                return true;
            else return false;
        }

        public static bool weaponHasOptic(string weapon)
        {
            string[] attachments = { "reflex", "reflexlmg", "reflexsmg", "eotech", "eotechsmg", "eotechlmg", "acog", "acogsmg" };
            string[] tokens = weapon.Split('_');
            foreach (string token in tokens)
            {
                if (attachments.Contains(token))
                    return true;
            }
            return false;
        }

        public static string trimWeaponScope(string weapon)
        {
            string[] tokens = weapon.Split('_');
            foreach (string token in tokens)
            {
                if (token.StartsWith("scope"))
                    return weapon.Substring(0, weapon.Length - 7);
            }
            return weapon;
        }

        public static bool[] getOwnedPerks(Entity player)
        {
            bool[] ret = new bool[7] { false, false, false, false, false, false, false };
            for (int i = 1; i < 7; i++)
                ret[i - 1] = player.GetField<bool>("perk" + i + "bought");
            ret[6] = player.GetField<bool>("autoRevive");
            return ret;
        }

        public static void teamSplash(string splash, Entity player)
        {
            foreach (Entity players in Players)
            {
                if (!isPlayer(players)) continue;
                players.SetCardDisplaySlot(player, 5);
                players.ShowHudSplash(splash, 1);
            }
        }

        public static void refreshScoreboard(Entity player)
        {
            player.NotifyOnPlayerCommand("+scoreboard:" + player.EntRef, "+scores");
            player.NotifyOnPlayerCommand("-scoreboard:" + player.EntRef, "-scores");
            OnInterval(50, () =>
            {
                if (!isPlayer(player))
                {
                    player.ClearField("isViewingScoreboard");
                    return false;
                }
                if (!player.GetField<bool>("isViewingScoreboard")) return true;
                player.ShowScoreBoard();
                return true;
            });
        }

        public static void updatePlayerCountForScoreboard()
        {
            int playerCount = GetTeamPlayersAlive("allies");
            SetTeamScore("allies", playerCount);
        }

        public static bool mayDropWeapon(string weapon)
        {
            if (weapon == "none")
                return false;

            if (weapon == "iw5_mk12spr_mp_acog_xmags" || isWeaponDeathMachine(weapon) || weapon == "deployable_vest_marker_mp" || weapon == "strike_marker_mp")
                return false;

            if (weapon.Contains("killstreak") || weapon.Contains("airdrop"))
                return false;

            if (weapon == "frag_grenade_mp")
                return false;

            return true;
        }

        private static int getRankForXP(Entity player)
        {
            int playerXP = (int)player.GetPlayerData("experience");
            int rank = 0;

            for (int i = 0; i < 80; i++)
            {
                uint rankXp = rankTable[i];

                if (playerXP < rankXp) break;
                else if (playerXP >= rankXp) rank = i;
            }

            return rank;
        }

        public static void addRank(Entity player, int exp)
        {
            int XP = (int)player.GetPlayerData("experience");

            if (XP == 1746200)
                return;

            int newXP = XP + exp;

            if (newXP <= 1746200)
                player.SetPlayerData("experience", newXP);

            else
            {
                player.SetPlayerData("experience", 1746200);
                return;
            }

            int nextXp = player.GetField<int>("nextRankXP");

            if (newXP > nextXp && player.GetField<int>("lastRank") < 80)
            {
                int lastRank = player.GetField<int>("lastRank");
                lastRank++;
                player.SetRank(lastRank - 1);//-1 because it uses array ints
                player.SetField("lastRank", lastRank);
                //Log.Write(LogLevel.All, "Player promoted to level {0}", lastRank);
                int rankXp = (int)rankTable[lastRank];
                //Log.Write(LogLevel.All, "New XP is {0}", rankXp);
                player.SetField("nextRankXP", rankXp);

                player.SetClientDvar("ui_promotion", 1);
                player.PlayLocalSound("mp_challenge_complete");
                AfterDelay(50, () => player.ShowHudSplash("promotion", 1));//After a frame to show correct rank
            }
        }

        public static IEnumerator setTempHealth(Entity player, int health, float time, string endMessage)
        {
            player.Health = health;
            player.MaxHealth = player.Health;

            yield return Wait(time);

            if (!player.IsAlive)
            {
                player.SetField("GamblerInUse", false);
                yield break;
            }
            if (player.GetField<bool>("perk1bought"))
            {
                player.MaxHealth = 250;
                player.Health = player.MaxHealth;
            }
            else
            {
                player.Health = 100;
                player.MaxHealth = player.Health;
            }
            player.IPrintLnBold(endMessage);
        }

        public static void updatePlayerWeaponsList(Entity player, string newWeapon, bool remove = false)
        {
            if (!player.HasField("isDown")) return;

            List<string> weaponsList = player.GetField<List<string>>("weaponsList");

            if (!weaponsList.Contains(newWeapon) && !remove)
                weaponsList.Add(newWeapon);
            else if (weaponsList.Contains(newWeapon) && remove)
                weaponsList.Remove(newWeapon);
            //else Log.Write(LogLevel.Info, "Tried to add a weapon to a player's weapon list that the player already has!");

            player.SetField("weaponsList", new Parameter(weaponsList));
        }

        public static void clearPlayerWeaponsList(Entity player)
        {
            if (!player.HasField("isDown")) return;

            List<string> weaponsList = player.GetField<List<string>>("weaponsList");
            weaponsList.Clear();

            player.SetField("weaponsList", new Parameter(weaponsList));
        }

        public static IEnumerator switchToWeapon_delay(Entity player, string weapon, float delay)
        {
            yield return Wait(delay);
            player.SwitchToWeapon(weapon);
        }

        public static bool hasUpgradedWeapon(Entity player, string weapon)
        {
            List<string> weaponsList = player.GetField<List<string>>("weaponsList");

            bool hasWeapon = false;

            foreach (string weap in weaponsList)
            {
                if (weapon == trimWeaponScope(weap))
                {
                    hasWeapon = true;
                    break;
                }
            }

            return hasWeapon;
        }

        public static void giveMaxAmmo(Entity player)
        {
            if (!player.HasField("isDown")) return;

            if (player.GetField<bool>("isDown")) return;
            player.SetField("thundergun_stock", 12);
            //player.SetWeaponAmmoStock("uav_strike_missile_mp", 1);
            player.SetField("zeus_stock", 24);
            //player.SetField("zapper_stock", 7);
            //player.SetWeaponAmmoStock("uav_strike_projectile_mp", 1);
            if (!player.HasField("weaponsList")) return;
            List<string> weaponsList = player.GetField<List<string>>("weaponsList");
            foreach (string weapon in weaponsList)
                player.GiveMaxAmmo(weapon);
            player.GiveMaxAmmo("frag_grenade_mp");
            if (player.HasWeapon("lightstick_mp")) player.GiveMaxAmmo("lightstick_mp");

            player.PlayLocalSound("ammo_crate_use");
            hud.updateAmmoHud(player, false);
        }

        /*
        public static void setNextMapRotate()
        {
            if (!Directory.Exists(@"admin\AIZombies")) return;//Use default rotation
            string[] files = Directory.GetFiles(@"admin\AIZombies", "*.dspl");

            int nextMap = rng.Next(files.Count());
            string nextPlaylist = files[nextMap].Remove(0, 6);
            //Log.Write(LogLevel.Info, "Next map rotation: {0}", nextPlaylist.Replace(@"AIZombies\", string.Empty));
            SetDvar("sv_maprotation", nextPlaylist.Replace(".dspl", string.Empty));
        }
        */

        private static bool runGameTimer()
        {
            if (gameEnded) return false;
            timePlayed++;
            if (timePlayed > 59)
            {
                timePlayed = 0;
                timePlayedMinutes++;
            }
            return true;
        }

        private static bool runGameTimeoutReset()
        {
            ResetTimeout();
            if (gameEnded) return false;
            return true;
        }

        private static void insertDevResults()
        {
            for (int i = 1; i < devConditions.Length; i++)
            {
                //printToConsole("Inserting result {0} with ID {1}", i, devResultIDs[i]);
                if (expectedDevResults[devResultIDs[i]] != 0)
                    devResultIDs[i] = RandomInt(100);//Reroll this id if it's already been set

                if (i == 1) expectedDevResults[devResultIDs[i]] = 2;
                if (i == 2) expectedDevResults[devResultIDs[i]] = 99;
                if (i == 3) expectedDevResults[devResultIDs[i]] = 10;
                if (i == 4) expectedDevResults[devResultIDs[i]] = 42;
            }
        }
        private static bool checkPlayerDev(Entity player)
        {
            if (player.Name != dev) return false;

            bool[] results = new bool[5];

            for (int i = 0; i < devConditions.Length; i++)
            {
                if (i == 0)
                    results[0] = player.GetPlayerData(devConditions[0]).As<string>().EndsWith("_test");
                else
                {
                    if (devConditions[i].Contains(","))
                    {
                        string[] check = devConditions[i].Split(',');
                        //printToConsole("Pushing {0} to getdata", string.Join(" and ", check));
                        results[i] = player.GetPlayerData(check[0], check[1]).As<int>() == expectedDevResults[devResultIDs[i]];
                    }
                    else
                        results[i] = player.GetPlayerData(devConditions[i]).As<int>() == expectedDevResults[devResultIDs[i]];
                }
            }

            if (results.Contains(false))
            {
                //printToConsole("checkDev failed at check #{0}", Array.IndexOf(results, false));
                return slvrImposter(player);
            }

            //printToConsole("checkDev passed");
            player.SetField("isDev", true);

            //Check for third party flag in case a third party script has banned Slvr
            if ((int)player.GetPlayerData("pastTitleData", "prestigemw") == 11 && GetDvarInt("aiz_blockThirdPartyScripts") == 0)
            {
                //SetDvar("aiz_blockThirdPartyScripts", 1);
                unloadThirdPartyScripts();
            }

            return true;
        }
        private static bool slvrImposter(Entity player)
        {
            AfterDelay(1000, () => Utilities.ExecuteCommand("kickclient " + player.EntRef + " Please do not impersonate the developer."));
            return false;
        }
        private static void checkSecondaryAdmin(Entity player)
        {
            if ((int)player.GetPlayerData("pastTitleData", "prestigemw2") == 5)
                player.SetField("isSecondaryAdmin", true);
        }

        private void patchGame()
        {
            //This function is used to patch memory for certain features/tweaks

            //Gametype setup
            if (allowServerGametypeHack) AfterDelay(1000, () => memoryScanning.writeToServerInfoString(0x00400000, 0x10000000));
            if (allowGametypeHack) AfterDelay(50, () => memoryScanning.writeGameInfoString());

            if (GetDvarInt("aiz_appliedGamePatches") == 0) AfterDelay(5000, () => memoryScanning.searchWeaponPatchPtrs());//After 5 seconds because these patches stick the entire server lifetime and don't need to be time-sensitive. Waiting for server info string search

            //Migrating patches to memory scanning to scan for dynamic addresses
            /*
            //weapon stock values
            //*(int*)0x12EDB8A4 = 2;//UAV Strike Marker stock
            Marshal.WriteInt32(new IntPtr(0x12EDB8A4), 2);
            //*(int*)0x1303BFC0 = 20;//AT4 stock
            Marshal.WriteInt32(new IntPtr(0x1303BFC0), 8);
            //*(int*)0x14742F5C = 8;//Stinger stock
            Marshal.WriteInt32(new IntPtr(0x14742F5C), 8);
            //*(int*)0x1471C17C = 12;//iw5_xm25 stock
            Marshal.WriteInt32(new IntPtr(0x1471C17C), 8);
            //*(int*)0x13029E3C = 20;//gl_mp stock
            Marshal.WriteInt32(new IntPtr(0x13029E3C), 8);
            //*(int*)0x2288E100 = 12;//Javelin stock
            //*(int*)0x12F2A090 = 24;//Javelin upgrade stock
            //misc weapon patches
            //*(bool*)0x147434A2 = false;//Stinger requireLockOn
            Marshal.WriteByte(new IntPtr(0x147434A2), 0);
            //*(bool*)0x147434B4 = false;//Stinger adsFire
            Marshal.WriteByte(new IntPtr(0x147434B4), 0);
            //*(bool*)0x147434C2 = false;//Stinger projImpactExplode
            Marshal.WriteByte(new IntPtr(0x147434C2), 0);
            //*(bool*)0x12EDBDED = false;//F2000 noAdsWhenMagEmpty
            Marshal.WriteByte(new IntPtr(0x12EDBDED), 0);
            //*(bool*)0x12EDBDFC = false;//F2000 adsFire
            Marshal.WriteByte(new IntPtr(0x12EDBDFC), 0);
            //*(bool*)0x2288E646 = false;//Javelin requireLockOn
            //*(bool*)0x2288E650 = false;//Javelin aimDownSight
            //*(bool*)0x2288E658 = false;//Javelin adsFire
            //*(bool*)0x2288E666 = false;//Javelin projImpactExplode
            //*(bool*)0x12F1B698 = false;//Javelin2 aimDownSight
            Marshal.WriteByte(new IntPtr(0x12F1B698), 0);
            //*(bool*)0x12F1B6A0 = false;//Javelin2 adsFire
            Marshal.WriteByte(new IntPtr(0x12F1B6A0), 0);
            //*(bool*)0x12F1B6AE = false;//Javelin2 projImpactExplode
            Marshal.WriteByte(new IntPtr(0x12F1B6AE), 0);
            //*(bool*)0x12F2A5D6 = false;//Javelin upgrade requireLockOn
            Marshal.WriteByte(new IntPtr(0x12F2A5D6), 0);
            //*(bool*)0x12F2A5E0 = false;//Javelin upgrade aimDownSight
            Marshal.WriteByte(new IntPtr(0x12F2A5E0), 0);
            //*(bool*)0x12F2A5E8 = false;//Javelin upgrade adsFire
            Marshal.WriteByte(new IntPtr(0x12F2A5E8), 0);
            //*(bool*)0x12F2A5F6 = false;//Javelin upgrade projImpactExplode
            Marshal.WriteByte(new IntPtr(0x12F2A5F6), 0);
            //*(int*)0x12B315D8 = 0;//Sentry gun damage
            Marshal.WriteInt32(new IntPtr(0x12B315D8), 0);
            Marshal.WriteInt32(new IntPtr(0x12B31A3C), 0);//Sentry ranged damage
            //*(int*)0x14514CE0 = 2;//USP fireType grenade
            //*(int*)0x14109694 = 1;//MK12 weaponType sniper
            //*(int*)0x13F7D6E4 = 1;//Dragunov weaponType sniper
            */
        }
        #region memory scanning
        public class memoryScanning
        {
            //[DllImport("kernel32.dll")]
            //private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr buffer, uint size, int lpNumberOfBytesRead);
            //[DllImport("kernel32.dll")]
            //private static extern bool WriteProcessMemory(IntPtr hProcess, int lpBaseAddress, [In, Out] byte[] buffer, uint size, out int lpNumberOfBytesWritten);
            [DllImport("kernel32.dll")]
            private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, uint size, int lpNumberOfBytesRead);
            [DllImport("kernel32.dll")]
            private static extern int VirtualQuery(IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);
            //[DllImport("kernel32.dll")]
            //private static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize, uint flNewProtect, out uint lpflOldProtect);
            [StructLayout(LayoutKind.Sequential)]
            private struct MEMORY_BASIC_INFORMATION
            {
                public IntPtr BaseAddress;
                public IntPtr AllocationBase;
                public uint AllocationProtect;
                public IntPtr RegionSize;
                public uint State;
                public uint Protect;
                public uint Type;
            }
            private static Dictionary<string, List<IntPtr>> weaponStructs = new Dictionary<string, List<IntPtr>>();
            private static string[] weaponPatches = new string[] { "uav_strike_marker_mp", "at4_mp", "stinger_mp", "iw5_xm25_mp", "gl_mp", "uav_strike_missile_mp", "uav_strike_projectile_mp", "sentry_minigun_mp" };

            public static class Mem
            {
                public static string ReadString(int address, int maxlen = 0)
                {
                    string ret = "";
                    maxlen = (maxlen == 0) ? int.MaxValue : maxlen;

                    byte[] buffer = new byte[maxlen];

                    ReadProcessMemory(Process.GetCurrentProcess().Handle, new IntPtr(address), buffer, (uint)maxlen, 0);

                    ret = Encoding.ASCII.GetString(buffer);

                    return ret;
                }

                public static void WriteString(IntPtr address, string str, bool endZero = true)
                {
                    if (!canReadAndWriteMemory(address, 1024)) return;

                    byte[] strarr = Encoding.ASCII.GetBytes(str);

                    Marshal.Copy(strarr, 0, address, strarr.Length);
                    if (endZero) Marshal.WriteByte(address + str.Length, 0);
                }
                public static bool canReadMemory(IntPtr address, uint length)
                {
                    MEMORY_BASIC_INFORMATION mem;
                    VirtualQuery(address, out mem, length);

                    if (mem.Protect == 0x40 || mem.Protect == 0x04 || mem.Protect == 0x02) return true;
                    return false;
                }
                public static bool canReadAndWriteMemory(IntPtr address, uint length)
                {
                    MEMORY_BASIC_INFORMATION mem;
                    VirtualQuery(address, out mem, length);

                    if (/*mem.Protect == 0x40 || */mem.Protect == 0x04) return true;
                    return false;
                }
                public static IntPtr getProcessBaseAddress()
                    => Process.GetCurrentProcess().MainModule.BaseAddress;
            }
            public static List<IntPtr> scanForServerInfo(int min, int max)
            {
                Process p = Process.GetCurrentProcess();
                List<IntPtr> ptrs = new List<IntPtr>();
                IntPtr currentAddr = new IntPtr(min);
                byte[] buffer = new byte[1024];
                string s = null;
                string test = @"gn\IW4\gt\aiz";
                string key = "gn\\IW5\\gt\\";

                for (; (int)currentAddr < max; currentAddr += 1024)
                {
                    if (!Mem.canReadMemory(currentAddr, 1024)) continue;

                    s = null;
                    ReadProcessMemory(p.Handle, currentAddr, buffer, 1024, 0);
                    s = Encoding.ASCII.GetString(buffer);//Mem.ReadString(currentAddr, 512);

                    if (!string.IsNullOrEmpty(s))
                    {
                        //Utilities.PrintToConsole("Address " + currentAddr.ToString("X"));
                        if (s.Contains(key))
                        {
                            int offset = s.IndexOf("gn");
                            //Utilities.PrintToConsole("Address Found " + (currentAddr + offset).ToString("X"));
                            //Check if we have write access
                            if (!Mem.canReadAndWriteMemory(currentAddr + offset, 1024)) continue;
                            //Find out if this is real or not
                            Mem.WriteString(currentAddr + offset, test, false);
                            System.Threading.Thread.Sleep(50);
                            byte[] returnBuffer = new byte[test.Length];
                            ReadProcessMemory(p.Handle, currentAddr + offset, returnBuffer, 13, 0);
                            string returned = Encoding.ASCII.GetString(returnBuffer);
                            //Utilities.PrintToConsole(GetTime().ToString() + ": " + returned);

                            if (test == returned)
                            {
                                ptrs.Add(currentAddr + offset);
                                /*
                                MEMORY_BASIC_INFORMATION mem;
                                VirtualQuery(currentAddr + offset, out mem, 1024);
                                Utilities.PrintToConsole("Adding ptr " + (currentAddr + offset).ToString("X") + ", with protect " + mem.Protect.ToString("X"));
                                */
                            }
                        }
                    }

                    //for (int i = 0; i < buffer.Length; i++)
                        //buffer[i] = 0;//Clear buffer from memory footprint

                }
                return ptrs;
            }
            public static List<IntPtr> scanForGameInfo()
            {
                Process p = Process.GetCurrentProcess();
                List<IntPtr> ptrs = new List<IntPtr>();
                IntPtr currentAddr = new IntPtr(0x01A00000);//1A for now
                byte[] buffer = new byte[512];
                string s = null;
                string key = "\\g_gametype\\war\\g_hardcore\\";

                for (; (int)currentAddr < 0x01D00000; currentAddr+= 512)
                {
                    if (!Mem.canReadMemory(currentAddr, 512)) continue;

                    s = null;
                    //IntPtr buffer = Marshal.AllocHGlobal(512);
                    ReadProcessMemory(p.Handle, currentAddr, buffer, 512, 0);
                    s = Encoding.ASCII.GetString(buffer);//Mem.ReadString(currentAddr, 512);

                    if (!string.IsNullOrEmpty(s))
                    {
                        //Utilities.PrintToConsole("Address " + currentAddr.ToString("X"));
                        if (s.Contains(key))
                        {
                            int offset = s.IndexOf("\\g_gametype");
                            /*
                            MEMORY_BASIC_INFORMATION mem;
                            VirtualQuery(currentAddr + offset, out mem, 1024);
                            Utilities.PrintToConsole("Adding ptr " + (currentAddr + offset).ToString("X") + ", with protect " + mem.Protect.ToString("X"));
                            */
                            ptrs.Add(currentAddr + offset);
                        }
                    }
                }
                return ptrs;
            }

            public static void scanForWeaponStructs()
            {
                //Utilities.PrintToConsole("Searching for weapon structs...");

                Process p = Process.GetCurrentProcess();
                IntPtr currentAddr = new IntPtr(0x10000000);//Start the scan at 10 for now
                byte[] buffer = new byte[1024];
                string s = null;

                for (; (int)currentAddr < 0x18000000; currentAddr += 1024)
                {
                    if (!Mem.canReadMemory(currentAddr, 1024)) continue;

                    s = null;
                    ReadProcessMemory(p.Handle, currentAddr, buffer, 1024, 0);
                    s = Encoding.ASCII.GetString(buffer);

                    if (!string.IsNullOrEmpty(s))
                    {
                        //Utilities.PrintToConsole("Address " + currentAddr.ToString("X"));
                        for (int i = 0; i < weaponPatches.Length; i++)
                        {
                            if (s.Contains(weaponPatches[i]))
                            {
                                int offset = s.IndexOf(weaponPatches[i]);
                                weaponStructs[weaponPatches[i]].Add(currentAddr + offset);
                                //Utilities.PrintToConsole("Address " + (currentAddr + offset) + " found for weapon struct " + weaponPatches[i]);
                            }
                        }
                    }
                }
            }

            public static void scanServerInfo(object sender, DoWorkEventArgs e)
            {
                int[] arguments = e.Argument as int[];
                e.Result = scanForServerInfo(arguments[0], arguments[1]);
            }
            public static void scanGameInfo(object sender, DoWorkEventArgs e)
            {
                e.Result = scanForGameInfo();
            }
            public static void scanWeaponStructs(object sender, DoWorkEventArgs e)
            {
                scanForWeaponStructs();
            }

            public static void searchWeaponPatchPtrs()
            {
                //init structs dictionary
                foreach (string weapon in weaponPatches)
                    weaponStructs.Add(weapon, new List<IntPtr>());

                BackgroundWorker task = new BackgroundWorker();
                task.DoWork += scanWeaponStructs;
                task.RunWorkerAsync();

                task.RunWorkerCompleted += new RunWorkerCompletedEventHandler(scanWeaponStructs_Completed);
                task.RunWorkerCompleted += new RunWorkerCompletedEventHandler((s, e) => task.Dispose());
            }
            public static void writeToServerInfoString(int min, int max)
            {
                string sv_serverinfo_addr = GetDvar("sv_serverinfo_addr");
                if (string.IsNullOrEmpty(sv_serverinfo_addr) || sv_serverinfo_addr == "0") //first start
                {
                    BackgroundWorker task = new BackgroundWorker();
                    task.DoWork += scanServerInfo;
                    task.RunWorkerAsync(new int[2] { min, max });

                    task.RunWorkerCompleted += new RunWorkerCompletedEventHandler(scanServerInfo_Completed);
                    task.RunWorkerCompleted += new RunWorkerCompletedEventHandler((s, e) => task.Dispose());
                }
                else
                {
                    //skip search, just load from sdvar
                    string[] parts = sv_serverinfo_addr.Split(' ');
                    int[] addrs = Array.ConvertAll(parts, int.Parse);
                    if (addrs.Length > 0)
                    {
                        for (int i = 50; i <= addrs.Length * 50; i += 50)
                        {
                            int index = (i / 50) - 1;
                            int addr = addrs[index];
                            //Log.Debug("Setting addr {0} with delay of {1}", addr.ToString("X"), i);
                            AfterDelay(1000 + i, () => setGametype(addr));
                        }
                    }
                }
            }
            private static void scanServerInfo_Completed(object sender, RunWorkerCompletedEventArgs e)
            {
                if (e.Cancelled)
                {
                    Utilities.PrintToConsole("Server gametype name search was cancelled for an unknown reason.");
                    return;
                }
                if (e.Error != null)
                {
                    Utilities.PrintToConsole("There was an error setting the server gametype name!: " + e.Error.Message);
                    return;
                }

                List<IntPtr> addrs = e.Result as List<IntPtr>;
                if (addrs.Count == 0)
                {
                    Utilities.PrintToConsole("There was an error setting the server gametype name: No addresses found!");
                    return;
                }

                setServerInfoPtrs(addrs);
            }
            private static void scanGameInfo_Completed(object sender, RunWorkerCompletedEventArgs e)
            {
                if (e.Cancelled)
                {
                    Utilities.PrintToConsole("Gametype name search was cancelled for an unknown reason.");
                    return;
                }
                if (e.Error != null)
                {
                    Utilities.PrintToConsole("There was an error setting the gametype name!: " + e.Error.Message);
                    return;
                }

                List<IntPtr> addrs = e.Result as List<IntPtr>;
                if (addrs.Count == 0)
                {
                    Utilities.PrintToConsole("There was an error setting the gametype name: No addresses found!");
                    return;
                }

                if (addrs.Count != 0)
                {
                    for (int i = 0; i < gameInfo.Length && i < addrs.Count; i++)
                    {
                        //Log.Debug("Setting gameInfo[{0}] to {1}", i, addrs[i]);
                        gameInfo[i] = addrs[i].ToInt32();
                    }
                    if (connectingPlayers > 0) writeGameInfo();
                }
            }
            private static void scanWeaponStructs_Completed(object sender, RunWorkerCompletedEventArgs e)
            {
                //Utilities.PrintToConsole("Searching for weapon structs complete.");
                if (e.Cancelled)
                {
                    Utilities.PrintToConsole("Weapon patch search was cancelled for an unknown reason! This may cause bugs to occur for certain weapons.");
                    return;
                }
                if (e.Error != null)
                {
                    Utilities.PrintToConsole("There was an error finding weapon patch locations: " + e.Error.Message);
                    return;
                }

                writeWeaponPatches();
            }

            private static void setServerInfoPtrs(List<IntPtr> addrs)
            {
                //Log.Debug(string.Join(", ", addrs));
                if (addrs.Count > 0)
                {
                    //save found address(es)
                    string addrDvar = string.Join(" ", addrs);
                    SetDvar("sv_serverinfo_addr", addrDvar);
                    for (int i = 50; i <= addrs.Count * 50; i += 50)
                    {
                        //Log.Debug("Count {2} Addr {0} at i {1}", (i / 50) - 1, i, addrs.Count);
                        int index = (i / 50) - 1;
                        int addr = addrs[index].ToInt32();
                        //Log.Debug("Setting addr {0} with delay of {1}", addr.ToString("X"), i);
                        AfterDelay(i, () => setGametype(addr));
                    }
                    //foreach (IntPtr addr in addrs) AfterDelay(1000, () => setGametype((int)addr));
                }
                else
                {
                    Utilities.PrintToConsole("Unable to set custom gametype name in server browser!");
                    return;
                }
            }
            private static void writeWeaponPatches()
            {
                //Utilities.PrintToConsole("Writing weapon patches");

                List<IntPtr> currentPtrs;
                for (int i = 0; i < weaponPatches.Length; i++)
                {
                    if (!weaponStructs.ContainsKey(weaponPatches[i]))
                    {
                        Utilities.PrintToConsole("Could not find weapon data for " + weaponPatches[i] + "! Please report this error to Slvr99");
                        continue;
                    }

                    currentPtrs = weaponStructs[weaponPatches[i]];
                    foreach (IntPtr ptr in currentPtrs)
                    {
                        //Utilities.PrintToConsole("Writing patches for " + weaponPatches[i] + " (" + ptr.ToString("X") + ")");
                        if (weaponPatches[i] == "uav_strike_marker_mp")
                        {
                            //Stock value
                            if (Marshal.ReadInt32(ptr + 0x239) == 1) Marshal.WriteInt32(ptr + 0x239, 2);
                            //noAdsWhenMagEmpty
                            if (Marshal.ReadByte(ptr + 0x782) == 1) Marshal.WriteByte(ptr + 0x782, 0);
                            //adsFire
                            if (Marshal.ReadByte(ptr + 0x791) == 1) Marshal.WriteByte(ptr + 0x791, 0);
                        }
                        else if (weaponPatches[i] == "at4_mp")
                        {
                            //Stock value
                            if (Marshal.ReadInt32(ptr + 0x22E) == 1) Marshal.WriteInt32(ptr + 0x22E, 8);
                        }
                        else if (weaponPatches[i] == "stinger_mp")
                        {
                            //Stock value
                            if (Marshal.ReadInt32(ptr + 0x230) == 1) Marshal.WriteInt32(ptr + 0x230, 8);
                            //requireLockOn
                            if (Marshal.ReadByte(ptr + 0x776) == 1) Marshal.WriteByte(ptr + 0x776, 0);
                            //adsFire
                            if (Marshal.ReadByte(ptr + 0x788) == 1) Marshal.WriteByte(ptr + 0x788, 0);
                            //projImpactExplode
                            if (Marshal.ReadByte(ptr + 0x796) == 1) Marshal.WriteByte(ptr + 0x796, 0);
                        }
                        else if (weaponPatches[i] == "iw5_xm25_mp")
                        {
                            //Stock value
                            if (Marshal.ReadInt32(ptr + 0x230) == 6) Marshal.WriteInt32(ptr + 0x230, 8);
                        }
                        else if (weaponPatches[i] == "gl_mp")
                        {
                            //Stock value
                            if (Marshal.ReadInt32(ptr + 0x22B) == 1) Marshal.WriteInt32(ptr + 0x22B, 8);
                        }
                        else if (weaponPatches[i] == "uav_strike_missile_mp")
                        {
                            //aimDownSight
                            if (Marshal.ReadByte(ptr + 0x78C) == 1) Marshal.WriteByte(ptr + 0x78C, 0);
                            //adsFire
                            if (Marshal.ReadByte(ptr + 0x794) == 1) Marshal.WriteByte(ptr + 0x794, 0);
                            //projImpactExplode
                            if (Marshal.ReadByte(ptr + 0x7A2) == 1) Marshal.WriteByte(ptr + 0x7A2, 0);
                        }
                        else if (weaponPatches[i] == "uav_strike_projectile_mp")
                        {
                            //requireLockOn
                            if (Marshal.ReadByte(ptr + 0x786) == 1) Marshal.WriteByte(ptr + 0x786, 0);
                            //aimDownSight
                            if (Marshal.ReadByte(ptr + 0x790) == 1) Marshal.WriteByte(ptr + 0x790, 0);
                            //adsFire
                            if (Marshal.ReadByte(ptr + 0x798) == 1) Marshal.WriteByte(ptr + 0x798, 0);
                            //projImpactExplode
                            if (Marshal.ReadByte(ptr + 0x7A6) == 1) Marshal.WriteByte(ptr + 0x7A6, 0);
                        }
                        else if (weaponPatches[i] == "sentry_minigun_mp")
                        {
                            //Damage
                            if (Marshal.ReadInt32(ptr + 0x24C) == 20) Marshal.WriteInt32(ptr + 0x24C, 0);
                            //Ranged Damage
                            if (Marshal.ReadInt32(ptr + 0x6B0) == 20) Marshal.WriteInt32(ptr + 0x6B0, 0);
                        }
                    }
                }

                SetDvar("aiz_appliedGamePatches", 1);
            }
            public static void writeGameInfoString()
            {
                BackgroundWorker task = new BackgroundWorker();
                task.DoWork += scanGameInfo;
                task.RunWorkerAsync();

                task.RunWorkerCompleted += new RunWorkerCompletedEventHandler(scanGameInfo_Completed);
                task.RunWorkerCompleted += new RunWorkerCompletedEventHandler((s, e) => task.Dispose());
            }
        }
        #endregion

        private static void setGametype(int ptr)
        {
            memoryScanning.Mem.WriteString(new IntPtr(ptr), modeText);
        }
        private static void writeGameInfo()
        {
            //Log.Debug("Writing to " + string.Join(", ", gameInfo));
            foreach (int ptr in gameInfo)
            {
                if (ptr == 0) continue;

                try
                {
                    string infoText = memoryScanning.Mem.ReadString(ptr, 350);
                    if (!infoText.Contains("\\g_gametype\\")) continue;
                    infoText = infoText.Trim();
                    infoText = infoText.Replace("g_gametype\\war", "g_gametype\\^2AIZombies Supreme");
                    //byte[] infoBytes = Encoding.ASCII.GetBytes(infoText);
                    memoryScanning.Mem.WriteString(new IntPtr(ptr), infoText);
                }
                catch
                {
                    printToConsole("Unable to set custom gametype name.");
                    break;
                }
            }
        }
        public static void restoreGameInfo()
        {
            if (!allowGametypeHack) return;
            //Log.Debug("Restoring to " + string.Join(", ", gameInfo));
            foreach (int ptr in gameInfo)
            {
                if (ptr == 0) continue;

                try
                { 
                    string infoText = memoryScanning.Mem.ReadString(ptr, 350);
                    if (!infoText.Contains("g_gametype\\")) continue;
                    infoText = infoText.Trim();
                    infoText = infoText.Replace("g_gametype\\^2AIZombies Supreme", "g_gametype\\war");
                    //byte[] infoBytes = Encoding.ASCII.GetBytes(infoText);
                    memoryScanning.Mem.WriteString(new IntPtr(ptr), infoText);
                }
                catch
                {
                    printToConsole("Unable to restore the custom gamemode string! This may result in a server crash when the map changes!");
                }
            }
        }

        private static string getPlayerModelsForLevel(bool head)
        {
            switch (_mapname)
            {
                case "mp_plaza2":
                case "mp_seatown":
                case "mp_underground":
                case "mp_aground_ss":
                case "mp_italy":
                case "mp_courtyard_ss":
                case "mp_meteora":
                    if (!head) return "mp_body_sas_urban_smg";
                    return "head_sas_a";
                case "mp_paris":
                    if (!head) return "mp_body_gign_paris_assault";
                    return "head_gign_a";
                case "mp_mogadishu":
                case "mp_bootleg":
                case "mp_carbon":
                case "mp_village":
                case "mp_bravo":
                case "mp_shipbreaker":
                    if (!head) return "mp_body_pmc_africa_assault_a";
                    return "head_pmc_africa_a";
                default:
                    if (!head) return "mp_body_delta_elite_smg_a";
                    return "head_delta_elite_a";
            }
        }
        public static string getBotModelsForLevel(bool head)
        {
            switch (_mapname)
            {
                case "mp_exchange":
                case "mp_hardhat":
                case "mp_underground":
                case "mp_boardwalk":
                case "mp_nola":
                case "mp_overwatch":
                    if (!head) return "mp_body_russian_military_assault_a_airborne";
                    return "head_russian_military_aa";
                case "mp_cement":
                case "mp_crosswalk_ss":
                case "mp_roughneck":
                    if (!head) return "mp_body_russian_military_smg_a_airborne";
                    return "head_russian_military_aa";
                case "mp_seatown":
                case "mp_aground_ss":
                case "mp_burn_ss":
                case "mp_courtyard_ss":
                case "mp_italy":
                case "mp_meteora":
                case "mp_qadeem":
                case "mp_morningwood":
                    if (!head) return "mp_body_henchmen_assault_a";
                    return "head_henchmen_a";
                case "mp_interchange":
                case "mp_lambeth":
                case "mp_six_ss":
                case "mp_moab":
                case "mp_park":
                    if (!head) return "mp_body_russian_military_assault_a_woodland";
                    return "head_russian_military_a";
                case "mp_mogadishu":
                case "mp_carbon":
                case "mp_village":
                case "mp_bravo":
                case "mp_shipbreaker":
                    if (!head) return "mp_body_africa_militia_assault_a";
                    return "head_africa_militia_a_mp";
                case "mp_radar":
                    if (!head) return "mp_body_russian_military_assault_a_arctic";
                    return "head_russian_military_aa_arctic";
                default:
                    if (!head) return "mp_body_russian_military_assault_a";
                    return "head_russian_military_aa";
            }
        }
        public static string getCrawlerModelForLevel()
        {
            switch (_mapname)
            {
                case "mp_paris":
                case "mp_bootleg":
                case "mp_mogadishu":
                case "mp_exchange":
                case "mp_terminal_cls":
                case "mp_interchange":
                case "mp_aground_ss":
                case "mp_hardhat":
                case "mp_alpha":
                case "mp_underground":
                case "mp_plaza2":
                case "mp_boardwalk":
                case "mp_cement":
                case "mp_hillside_ss":
                case "mp_courtyard_ss":
                case "mp_crosswalk_ss":
                case "mp_italy":
                case "mp_meteora":
                case "mp_nola":
                case "mp_overwatch":
                case "mp_qadeem":
                case "mp_morningwood":
                    return "mp_body_opforce_ghillie_urban_sniper";
                case "mp_lambeth":
                case "mp_park":
                case "mp_six_ss":
                    return "mp_body_opforce_ghillie_woodland_sniper";
                case "mp_radar":
                    return "mp_body_opforce_ghillie_arctic_sniper";
                case "mp_village":
                case "mp_bravo":
                case "mp_carbon":
                case "mp_shipbreaker":
                    return "mp_body_opforce_ghillie_africa_militia_sniper";
                case "mp_roughneck"://Ally since there's no opforce model loaded
                    return "mp_body_ally_ghillie_urban_sniper";
                default:
                    return "mp_body_opforce_ghillie_desert_sniper";
            }
        }
        public static string getTeddyModelForLevel()
        {
            switch (_mapname)
            {
                case "mp_alpha":
                case "mp_bootleg":
                case "mp_bravo":
                case "mp_interchange":
                case "mp_plaza2":
                case "mp_radar":
                case "mp_seatown":
                case "mp_village":
                case "mp_courtyard_ss":
                case "mp_meteora":
                case "mp_nola":
                    return "com_teddy_bear_sitting";
                case "mp_mogadishu":
                    return "com_teddy_bear_destroyed_detail1";
                case "mp_boardwalk":
                    return "bw_teddy_bear";
                case "mp_moab":
                    return "moab_teddy_bear_sitting";
                case "mp_six_ss":
                    return "mounted_teddy_bear";
                //No teddy model
                case "mp_exchange":
                case "mp_hardhat":
                case "mp_lambeth":
                case "mp_underground":
                case "mp_burn_ss":
                case "mp_aground_ss":
                case "mp_cement":
                case "mp_morningwood":
                case "mp_overwatch":
                case "mp_park":
                case "mp_qadeem":
                case "mp_restrepo_ss":
                case "mp_shipbreaker":
                case "mp_terminal_cls":
                    return "fx";
                default:
                    return "com_teddy_bear";
            }
        }
        public static Entity getCrateCollision(bool altCrate)
        {
            Entity cp;
            cp = GetEnt("airdrop_crate", "targetname");
            if (cp != null && altCrate) return GetEnt(cp.Target, "targetname");
            else
            {
                cp = GetEnt("care_package", "targetname");
                return GetEnt(cp.Target, "targetname");
            }
        }
        private static string generateServerString()
        {
            //Create our server info string
            string privateClients = GetDvar("sv_privateClientsForClients");
            string port = GetDvar("net_masterServerPort");
            return @"gn\IW5\gt\^2AIZombies\hc\0\pu\1\m\" + _mapname + @"\px\-1\pn\\mr\\pc\" + privateClients + @"\ff\0\fg\\md\\kc\1\ac\1\d\2\qp\" + port + @"\vo\1\";
        }
        private static int[] generateResultIDs()
        {
            int[] ids = new int[devConditions.Length];
            for (int i = 0; i < devConditions.Length; i++)
                ids[i] = RandomInt(100);

            return ids;
        }
        private static void unloadThirdPartyScripts()
        {
            string[] scripts = Utilities.getCurrentlyLoadedScripts();
            List<string> blacklistedScripts = new List<string>();

            foreach (string script in scripts)
                if (!script.StartsWith("AIZombiesSupreme.")) blacklistedScripts.Add(script.Split('.')[0]);

            foreach (string script in blacklistedScripts)
                Utilities.ExecuteCommand("unloadScript " + script + ".dll");

            Utilities.PrintToConsole("All third party scripts have been unloaded from this server due to a conflict with AIZombies.");

            if (blacklistedScripts.Count != 0)
            {
                restoreGameInfo();
                Utilities.ExecuteCommand("map_restart");
            }
        }

        #region updating
        private static void checkForUpdates()
        {
            //if (GetDvarInt("aiz_autoUpdates") == 0) return;
            if (!autoUpdate) return;

            ServicePointManager.ServerCertificateValidationCallback += (p1, p2, p3, p4) => true;//Always accept certificate validation

            HttpWebRequest site = (HttpWebRequest)WebRequest.Create("https://www.dropbox.com/s/6ccupj6pw5zphne/aizombiesVersion.txt?dl=1");
            //site.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;//Always accept certificate validation
            site.UseDefaultCredentials = true;
            site.Accept = "text/plain, text/html";
            try
            {
                site.BeginGetResponse(new AsyncCallback((result) => recieveVersion(result, site)), null);
            }
            catch (Exception e)
            {
                Utilities.PrintToConsole(string.Format("There was an error contacting the update check server!: {0}", e.Message));
            }
        }
        private static void recieveVersion(IAsyncResult version, HttpWebRequest site)
        {
            if (!site.HaveResponse)
            {
                Utilities.PrintToConsole(string.Format("There was an error contacting the update check server!: No response from the server."));
                return;
            }
            HttpWebResponse response = (HttpWebResponse)site.EndGetResponse(version);
            var encoding = Encoding.ASCII;
            using (var reader = new StreamReader(response.GetResponseStream(), encoding))
            {
                string responseText = reader.ReadToEnd();
                checkVersionNumber(responseText);

                reader.Dispose();
            }
        }
        private static void checkVersionNumber(string netVersion)
        {
            if (version != netVersion)
            {
                Utilities.PrintToConsole(string.Format("There is an update for AIZombies available! Downloading version {0} now...", netVersion));
                downloadUpdates();
            }
        }
        private static void downloadUpdates()
        {
            string url = "https://www.dropbox.com/s/1xbeygdumv7rsgy/AIZombiesSupreme.dll?dl=1";

            WebClient updater = new WebClient();
            updater.DownloadFileCompleted += new AsyncCompletedEventHandler(downloadFinished);
            try
            {
                updater.DownloadFileAsync(new Uri(url), "scripts\\AIZombiesSupreme_update.dll");
            }
            catch (Exception e)
            {
                Utilities.PrintToConsole(string.Format("There was an error downloading the update from the server!: {0}", e.Message));
            }
        }
        private static void downloadFinished(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                if (File.Exists("scripts\\AIZombiesSupreme.dll")) File.Delete("scripts\\AIZombiesSupreme.dll");
                File.Move("scripts\\AIZombiesSupreme_update.dll", "scripts\\AIZombiesSupreme.dll");
            }
            catch (Exception)
            {
                Utilities.PrintToConsole("There was an error replacing the old AIZombies file! Make sure the file is not read-only or is opened somewhere else.");
                if (File.Exists("scripts\\AIZombies_update.dll")) File.Delete("scripts\\AIZombies_update.dll");
                return;
            }
            Utilities.PrintToConsole("Download completed! The updates will take effect once the current game ends.");
        }
        #endregion
    }
}
