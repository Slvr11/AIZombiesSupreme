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

//TODO: fix waypoint baking(some waypoints seem to bake through walls/to 0), fix bots being immortal, fix gametype string address to not require a scan

namespace AIZombiesSupreme
{
    public class AIZ : BaseScript
    {
        public static readonly string _mapname = getMapname();
        public static string gameLanguage = "english";
        public static readonly string[] gameStrings = initGameStrings();
        public static string zombieMapname = null;
        public static bool isHellMap;
        public static string zState = "intermission";
        public static bool gameEnded = false;
        public static bool gameStarted = false;
        public static byte intermissionTimerNum = 30;
        //private static bool firstIntermission = true;
        public static int timePlayedMinutes = 0;
        public static int timePlayed = 0;
        //private static bool secondsTimerStarted = false;
        public static bool intermissionTimerStarted = false;
        public static readonly string[] zombieDeath = { gameStrings[0], gameStrings[1], gameStrings[2], gameStrings[3], gameStrings[4], gameStrings[5], gameStrings[6], gameStrings[7], gameStrings[8], gameStrings[9], gameStrings[10], gameStrings[11], gameStrings[12], gameStrings[13] };
        public static readonly string bodyModel = getPlayerModelsForLevel(false);
        public static readonly string headModel = getPlayerModelsForLevel(true);
        public static readonly Random rng = new Random();
        public static int maxPlayerHealth = 101;
        public static int maxPlayerHealth_Jugg = 251;
        public static bool powerActivated = false;
        public static bool tempPowerActivated = false;
        public static readonly string version = "1.5";
        public static readonly string dev = "Slvr99";

        private static readonly int[] expectedDevResults = new int[100];//Set to 100 and generate results at runtime
        private static readonly string[] devConditions = new string[5] { "cardNameplate", "money", "teamkills", "pastTitleData,prestigemw2", "pastTitleData,rankmw2" };
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
        public static bool botDeathVoices = false;
        public static bool fullFireSale = false;
        public static float damageGracePeriod = 0.25f;
        private static readonly string pauseMenu = "class";
        public static string vision = "";
        public static readonly string bossVision = "cobra_sunset1";
        public static readonly string hellVision = "cobra_sunset3";

        public static int mapHeight = 0;

        public static byte currentRayguns = 0;
        public static readonly byte maxRayguns = 2;
        public static byte currentThunderguns = 0;
        public static readonly byte maxThunderguns = 1;
        public static byte currentZappers = 0;

        private static short fx_carePackage;
        public static short fx_rayGun;
        public static short fx_rayGunUpgrade;
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
        public static short fx_tankExplode;
        public static short fx_crawlerExplode;
        //public static short fx_flamethrower;

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
                Utilities.PrintToConsole(gameStrings[14]);
                SetDvar("g_gametype", "war");
                Utilities.ExecuteCommand("map_restart");
                return;
            }
            if (GetDvarInt("sv_maxclients") > 8)
            {
                Utilities.PrintToConsole(string.Format(gameStrings[15], GetDvarInt("sv_maxclients")));
                Marshal.WriteInt32(new IntPtr(0x0585AE0C), 8);//Set maxclients directly to avoid map_restart
                Marshal.WriteInt32(new IntPtr(0x0585AE1C), 8);//Latched value
                Marshal.WriteInt32(new IntPtr(0x049EB68C), 8);//Raw maxclients value, this controls the real number of maxclients
                MakeDvarServerInfo("sv_maxclients", 8);
                
                if (GetDvarInt("sv_privateClients") > 0)
                {
                    SetDvar("sv_privateClients", 0);
                    //MakeDvarServerInfo("sv_privateClientsForClients", 0);
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
            AfterDelay(50, patchGame);

            killstreaks.initMapKillstreak();
            precacheGametype();

            //Utilities.SetDropItemEnabled(false);//Causes crash on death in most cases

            SetDvar("cg_drawCrosshair", 1);
            SetDvar("cg_crosshairDynamic", 1);

            StartAsync(initGameVisions());

            //MakeDvarServerInfo("ui_netGametypeName", "^2AIZombies Supreme");
            //MakeDvarServerInfo("party_gametype", "^2AIZombies Supreme");
            //MakeDvarServerInfo("ui_customModeName", "^2AIZombies Supreme");
            MakeDvarServerInfo("ui_gametype", "^2AIZombies Supreme");
            MakeDvarServerInfo("didyouknow", gameStrings[16]);
            MakeDvarServerInfo("g_motd", gameStrings[16]);
            MakeDvarServerInfo("motd", gameStrings[16]);
            //MakeDvarServerInfo("ui_connectScreenTextGlowColor", new Vector3(0, 1, 0));
            MakeDvarServerInfo("ui_allow_teamchange", 0);
            MakeDvarServerInfo("ui_allow_classchange", 0);
            SetDynamicDvar("scr_war_promode", 1);

            //Server netcode adjustments//
            SetDvar("com_maxfps", 0);
            //-IW5 server update rate-
            SetDevDvar("sv_network_fps", 200);
            //-Turn off flood protection-
            //SetDvar("sv_floodProtect", 0);
            //-Setup larger snapshot size and remove/lower delay-
            //Reverting
            SetDvar("sv_hugeSnapshotSize", 4000);
            SetDvar("sv_hugeSnapshotDelay", 200);
            //-Remove ping degradation-
            //SetDvar("sv_pingDegradation", 0);
            SetDvar("sv_pingDegradationLimit", 9999);
            //-Improve ping throttle-
            SetDvar("sv_acceptableRateThrottle", 9999);
            SetDvar("sv_newRateThrottling", 0);
            SetDvar("sv_newRateInc", 200);
            SetDvar("sv_newRateCap", 500);
            //-Tweak ping clamps-
            SetDvar("sv_minPingClamp", 50);
            //-Increase server think rate per frame-
            SetDvar("sv_cumulThinkTime", 1000);
            //-Disable playlist checking-
            SetDvar("playListUpdateCheckMinutes", 999999999);

            //End server netcode//

            //EXPERIMENTALS
            //-Lock CPU threads-
            //Reverting this since IS doesn't multithread
            SetDvar("sys_lockThreads", "none");
            //-Prevent game from attempting to slow time for frames-
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
            SetDvar("perk_diveViewRollSpeed", 0.00000001f);
            SetDvar("perk_diveGravityScale", .4f);
            SetDvar("perk_extendedMagsMGAmmo", 899);
            //SetDvar("perk_weapSpreadMultiplier", 0);
            SetDvar("scr_game_playerwaittime", 5);
            SetDvar("scr_game_matchstarttime", 0);
            //SetDvar("scr_game_graceperiod", 1);
            SetDvar("ui_hud_showdeathicons", "0");//Disable death icons
            SetDvar("missileJavDuds", 1);//Disable javelin explosions at close range

            SetDvarIfUninitialized("aiz_useMW2Visions", 0);
            SetDvarIfUninitialized("aiz_appliedGamePatches", 0);

            PlayerConnected += onPlayerConnect;

            Notified += onGlobalNotify;

            if (GetDvarInt("aiz_useMW2Visions") > 0) vision = getMW2Vision();

            Entity levelHeight = GetEnt("airstrikeheight", "targetname");
            if (levelHeight != null) killstreaks.heliHeight = (int)levelHeight.Origin.Z;

            hud.destroyGameHud();
            StartAsync(hud.createServerHud());

            //init rank table for stat tracking
            for (int i = 0; i < 80; i++)
            {
                rankTable[i] = uint.Parse(TableLookup("mp/rankTable.csv", 0, i, 2));
            }
            rankTable[80] = 1746200;

            //Seasonal elements
            checkForSeasons();

            StartAsync(mapEdit.cleanLevelEnts());
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

            mapEdit.createWaypoints();

            //OnInterval(100, runGameTimeoutReset);

            //Dome Easter egg init
            if (_mapname == "mp_dome")
            {
                Entity.Level.SetField("windmillList", new Parameter(new List<Entity>()));
                StartAsync(mapEdit.dome_deleteDynamicModels());

                mapEdit.dome_initEasterEgg();

                mapEdit.initDomeMoon();
            }

            //init voting map table
            int index = 0;
            for (int i = 109; i < 146; i++)
            {
                if (index == 0) hud.mapNames[index++] = "-";
                else if (i == 145) hud.mapNames[index++] = gameStrings[86];
                else hud.mapNames[index++] = gameStrings[i];
            }
            index = 0;
            for (int i = 144; i < 181; i++)
            {
                if (index == 0) hud.mapDesc[index++] = "-";
                else if (i == 180) hud.mapDesc[index++] = gameStrings[87];
                else hud.mapDesc[index++] = gameStrings[i];
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
            //fx_flamethrower = (short)LoadFX("impacts/pipe_fire");
            fx_crawlerExplode = (short)LoadFX("props/barrelExp");

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
            PreCacheItem(killstreaks.mapStreakWeapon);
            PreCacheItem("uav_strike_missile_mp");
            PreCacheTurret("manned_gl_turret_mp");
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
            PreCacheMiniMapIcon("compassping_friendly_party_mp");//Bot compass

            /*
            for (int i = 0; i < 12; i++)
            {
                string icon = killstreaks.getKillstreakCrateIcon(i);
                PreCacheShader(icon);
            }
            */
            //killstreaks
            PreCacheMpAnim(killstreaks.botAnim_idle);
            //PreCacheMpAnim(killstreaks.botAnim_idleRPG);
            PreCacheMpAnim(killstreaks.botAnim_idleMG);
            //PreCacheMpAnim(d_killstreaks.botAnim_idlePistol);
            PreCacheMpAnim(killstreaks.botAnim_reload);
            //PreCacheMpAnim(killstreaks.botAnim_reloadRPG);
            //PreCacheMpAnim(killstreaks.botAnim_reloadPistol);
            PreCacheMpAnim(killstreaks.botAnim_reloadMG);
            PreCacheMpAnim(killstreaks.botAnim_run);
            PreCacheMpAnim(killstreaks.botAnim_runMG);
            //PreCacheMpAnim(killstreaks.botAnim_runRPG);
            //PreCacheMpAnim(killstreaks.botAnim_runPistol);
            //PreCacheMpAnim(killstreaks.botAnim_runShotgun);
            PreCacheMpAnim(killstreaks.botAnim_runSMG);
            //PreCacheMpAnim(killstreaks.botAnim_runSniper);
            PreCacheMpAnim(killstreaks.botAnim_shoot);
            PreCacheMpAnim(killstreaks.botAnim_shootMG);
            //PreCacheMpAnim(killstreaks.botAnim_shootPistol);
            //PreCacheMpAnim(killstreaks.botAnim_shootRPG);

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

            //-Player netcode-
            player.SetClientDvars("snaps", 30, "rate", 30000);
            player.SetClientDvar("cl_maxPackets", 100);
            player.SetClientDvar("cl_packetdup", 1);
            player.SetClientDvar("com_maxFrameTime", 100);
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
            player.SetClientDvars("cg_drawCrosshair", "1", "ui_drawCrosshair", "1");
            player.SetClientDvars("g_teamname_allies", gameStrings[17], "g_teamname_axis", "".PadRight(300));
            player.SetClientDvars("g_teamicon_allies", "iw5_cardicon_soap", "g_teamicon_MyAllies", "iw5_cardicon_soap", "g_teamicon_EnemyAllies", "iw5_cardicon_soap");
            player.SetClientDvars("g_teamicon_axis", "weapon_missing_image", "g_teamicon_MyAxis", "weapon_missing_image", "g_teamicon_EnemyAxis", "weapon_missing_image");
            player.SetClientDvars("cl_demo_recordPrivateMatch", "0", "cl_demo_enabled", "0");
            player.SetClientDvar("cg_drawDamageFlash", 1);
            player.SetClientDvar("ui_hud_showdeathicons", "0");
            player.SetClientDvars("cg_scoreboardWidth", 600, "cg_scoreboardFont", 0);
            player.SetClientDvars("cg_watersheeting", 1, "cg_waterSheeting_distortionScaleFactor", .08f);
            player.SetClientDvars("maxVoicePacketsPerSec", 2000, "maxVoicePacketsPerSecForServer", 1000);
            player.SetClientDvar("bg_viewKickScale", 0.2f);
            player.SetClientDvar("cg_hudGrenadeIconMaxRangeFrag", 0);
            player.SetClientDvar("perk_diveViewRollSpeed", 0.00000001f);
            player.SetClientDvar("perk_diveGravityScale", .4f);
            player.SetClientDvar("perk_extendedMagsMGAmmo", 899);
            player.SetClientDvar("cg_weaponCycleDelay", 0);
            //player.SetClientDvar("gameMode", "so");
            player.SetClientDvar("useRelativeTeamColors", 1);
            player.SetClientDvars("bg_legYawTolerance", 50, "player_turnAnims", 1);
            player.SetClientDvar("scr_war_promode", 1);
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
            player.SetField("hasUsedBox", false);

            //Reset certain dvars that some servers may have set and not restored
            player.SetClientDvar("waypointIconHeight", "36");
            player.SetClientDvar("waypointIconWidth", "36");

            player.SetClientDvars("ui_gametype", "^2AIZombies Supreme", "ui_customModeName", "^2AIZombies Supreme", "ui_mapname", getZombieMapname(), "party_gametype", "^2AIZombies Supreme");
            player.SetClientDvars("didyouknow", gameStrings[16], "motd", gameStrings[16], "g_motd", gameStrings[16]);
            player.SetClientDvar("cg_objectiveText", string.Format(gameStrings[19], roundSystem.totalWaves));
            //if (isHellMap && !killstreaks.visionRestored) player.VisionSetNakedForPlayer(hellVision);
            //else player.VisionSetNakedForPlayer(vision);
            //player.VisionSetThermalForPlayer(_mapname, 0);
            player.NotifyOnPlayerCommand("use_button_pressed:" + player.EntRef, "+activate");
            player.NotifyOnPlayerCommand("bankWithdraw:" + player.EntRef, "vote yes");
            player.NotifyOnPlayerCommand("uav_reroute:" + player.EntRef, "vote no");//Drone reroute
            player.NotifyOnPlayerCommand("weapon_switch_pressed", "weapnext");
            player.OnNotify("weapon_switch_pressed", onWeaponSwitchPressed);
            player.SetField("weaponHudUpdateCount", 0);

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

            //Valentines day code
            if (Entity.Level.HasField("allowGifting"))
            {
                Entity giftTrigger = Spawn("script_model", player.Origin);
                giftTrigger.SetModel("tag_origin");
                giftTrigger.LinkTo(player);
                giftTrigger.SetField("owner", player);
                giftTrigger.SetField("range", 40);
                giftTrigger.SetField("usabletype", "giftTrigger");
                player.SetField("giftTrigger", giftTrigger);
                mapEdit.usables.Add(giftTrigger);
                //mapEdit.usables.Insert(mapEdit.usables.Count, giftTrigger);
            }
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
                AfterDelay(1500, () => player.IPrintLnBold(gameStrings[18]));
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
            player.SetClientDvar("cg_objectiveText", string.Format(gameStrings[19], roundSystem.totalWaves));
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
            player.SetActionSlot(3, "altMode");//Fix the ALTs not working

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
            player.SetField("ownsSentryGL", false);
            player.SetField("isCarryingSentry", false);
            player.SetField("ownsEMP", false);
            player.SetField("ownsAirstrike", false);
            player.SetField("ownsNuke", false);
            player.SetField("ownsLittlebird", false);
            player.SetField("ownsBot", false);
            player.SetField("ownsSubBot", false);
            player.SetField("ownsLMGBot", false);
            player.SetField("ownsAirdrop", false);
            player.SetField("ownsEmergencyAirdrop", false);
            player.SetField("ownsHeliSniper", false);
            player.SetField("ownsExpAmmo", false);
            player.SetField("ownsMapStreak", false);
            player.SetField("hasExpAmmoPerk", false);
            player.SetField("notTargetable", false);

            //hud.cs init
            if (!player.HasField("cash"))
                player.SetField("cash", 500);
            if (!player.HasField("points"))
                player.SetField("points", 0);
            //player.SetField("hasMessageUp", false); 

            //All perks
            if ((player.HasField("allPerks") && player.GetField<bool>("allPerks")) || (Entity.Level.HasField("allPerks") && Entity.Level.GetField<bool>("allPerks")))
            {
                bonusDrops.giveAllPerks(player);
                //Give hand-gun to fill second slot for mule kick
                player.GiveWeapon("defaultweapon_mp");
                updatePlayerWeaponsList(player, "defaultweapon_mp");
            }

            player.SetEMPJammed(false);
        }

        public override void OnPlayerConnecting(Entity player)
        {
            player.SetClientDvar("ui_gametype", "^2AIZombies Supreme");
            player.SetClientDvar("g_gametype", "^2AIZombies Supreme");
            player.SetClientDvar("ui_customModeName", "^2AIZombies Supreme");
            player.SetClientDvar("ui_mapname", getZombieMapname());
            player.SetClientDvar("party_gametype", "^2AIZombies Supreme");
            player.SetClientDvar("didyouknow", gameStrings[16]);
            player.SetClientDvar("motd", gameStrings[16]);
            player.SetClientDvar("g_motd", gameStrings[16]);
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
            if (!player.HasField("isDev") || gameEnded) return EventEat.EatNone;

            if (message == "toggleBotsIgnoreMe")
            {
                if (player.GetField<bool>("notTargetable"))
                {
                    player.SetField("notTargetable", false);
                    IPrintLn(player.Name + " ^7notarget OFF");
                }
                else if (!player.GetField<bool>("notTargetable"))
                {
                    player.SetField("notTargetable", true);
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
            else if (message == "resetServerDvars")
            {
                SetDvar("aiz_appliedGamePatches", 0);
                return EventEat.EatGame;
            }
            else if (message.StartsWith("setSLMaxStrings"))
            {
                int newMaxLimit = hud.maxUnlocalizedConfigStrings;
                if (int.TryParse(message.Split(' ')[1], out newMaxLimit))
                {
                    hud.maxUnlocalizedConfigStrings_danger = newMaxLimit;
                    Announcement("String Library Max String Count has been set to " + newMaxLimit.ToString());
                }
            }
            else if (message.StartsWith("setSLDangerZone"))
            {
                int newDangerZone = hud.maxUnlocalizedConfigStrings_danger;
                if (int.TryParse(message.Split(' ')[1], out newDangerZone))
                {
                    hud.maxUnlocalizedConfigStrings_danger = newDangerZone;
                    Announcement("String Library Danger Zone has been set to " + newDangerZone.ToString());
                }
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

            if (player.HasField("bot")) StartAsync(killstreaks.killPlayerBot(player));
            if (player.HasField("barbed_wire")) killstreaks.destroyBarbedWire(player.GetField<Entity>("barbed_wire"));
            AfterDelay(500, () => roundSystem.checkForEndGame());
            //Moved from Hud.cs
            hud.destroyPlayerHud(player);

            updatePlayerCountForScoreboard();

            if (player.HasField("giftTrigger"))
            {
                if (mapEdit.usables.Contains(player.GetField<Entity>("giftTrigger"))) mapEdit.removeUsable(player.GetField<Entity>("giftTrigger"));
                player.GetField<Entity>("giftTrigger").Delete();
                player.ClearField("giftTrigger");
            }
        }

        private static void onGlobalNotify(int entRef, string message, params Parameter[] parameters)
        {
            if (gameEnded) return;
            //if (entRef > 2046) return;
            //Entity player = Entity.GetEntity(entRef);

            #region match start
            if (!gameStarted)
            {
                if ((message == "match_start_timer_beginning" || message == "prematch_over"))
                {
                    foreach (Entity player in Players)
                    {
                        if (isHellMap)
                        {
                            if (isPlayer(player)) AfterDelay(500, () => player.VisionSetNakedForPlayer(hellVision));
                        }
                        else
                            if (isPlayer(player)) AfterDelay(500, () => player.VisionSetNakedForPlayer(vision));
                    }

                    if (message == "prematch_over")
                        gameStarted = true;
                }

                else if (message == "fontPulse")
                {
                    HudElem countdownTimer = HudElem.GetHudElem(entRef);
                    HudElem countdownText = HudElem.GetHudElem(entRef - 1);

                    hud._setText(countdownText, gameStrings[20]);
                    countdownTimer.GlowAlpha = 1;
                    countdownTimer.GlowColor = new Vector3(RandomFloatRange(0, 1), RandomFloatRange(0, 1), RandomFloatRange(0, 1));
                }
            }
            #endregion

            #region grenade_fire
            if (message == "grenade_fire")
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
                    case "claymore_mp":
                        Entity claymoreOwner = marker;
                        foreach (Entity players in Players)
                        {
                            if (!players.IsAlive || !players.HasField("isDown")) continue;
                            if (players.HasWeapon("claymore_mp") && players.GetWeaponAmmoClip("claymore_mp") == 0 && players.GetField<bool>("ownsMapStreak"))
                            {
                                claymoreOwner = players;
                                AfterDelay(500, () =>
                                    claymoreOwner.TakeWeapon("claymore_mp"));
                                //teamSplash("used_airdrop_mega", claymoreOwner);
                                marker.SetField("owner", claymoreOwner);
                                break;
                            }
                        }

                        killstreaks.spawnBarbWire(marker, claymoreOwner.Origin);
                        break;
                    case "airdrop_juggernaut_def_mp":
                        Entity oilOwner = marker;
                        foreach (Entity players in Players)
                        {
                            if (!players.IsAlive || !players.HasField("isDown")) continue;
                            if (players.HasWeapon("airdrop_juggernaut_def_mp") && players.GetWeaponAmmoClip("airdrop_juggernaut_def_mp") == 0 && players.GetField<bool>("ownsMapStreak"))
                            {
                                oilOwner = players;
                                AfterDelay(500, () =>
                                    oilOwner.TakeWeapon("airdrop_juggernaut_def_mp"));
                                //marker.SetField("type", "ammo");
                                marker.SetField("owner", oilOwner);
                                break;
                            }
                        }

                        int trailFX = LoadFX("smoke/jet_contrail");
                        PlayFXOnTag(trailFX, marker, "tag_origin");

                        StartAsync(watchForMarkerStick(marker, 4));
                        break;
                }
            }
            #endregion

            //if (player == null) return;

            #region jump(moon)
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
                    if (!players.IsSwitchingWeapon()) continue;

                    string newWeap = (string)parameters[0];
                    if (players.GetField<string>("lastDroppableWeapon") != newWeap && players.HasWeapon(newWeap))
                    {
                        hud.updateAmmoHud(players, true, newWeap);

                        if (isWeaponDeathMachine(newWeap))
                        {
                            players.SetPerk("specialty_extendedmags", true, false);
                            players.SetWeaponAmmoClip(newWeap, 999);
                            players.SetWeaponAmmoStock(newWeap, 0);
                            players.UnSetPerk("specialty_extendedmags");
                        }
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

                    if (mayDropWeapon(weapon) && !players.GetField<bool>("isDown"))
                        players.SetField("lastDroppableWeapon", weapon);

                    killstreaks.executeKillstreak(players, weapon);

                    if (weapon == "trophy_mp")
                    {
                        if (players.HasField("hasHelmetOn")) mapEdit.takeOffHelmet(players);
                        else mapEdit.putOnHelmet(players);
                        continue;
                    }

                    /*
                    if (players.GetField<bool>("ownsBot") && !isSpecialWeapon(weapon))
                    {
                        Entity bot = players.GetField<Entity>("bot");
                        killstreaks.updateBotGun(bot);
                    }
                    */

                    if ((trimWeaponScope(weapon) == "iw5_type95_mp_reflex_xmags_camo11" || trimWeaponScope(weapon) == "iw5_m16_mp_rof_xmags_camo11"))
                    {
                        players.SetClientDvar("perk_weapRateMultiplier", "0.001");
                        players.SetField("hasAlteredROF", true);
                        players.SetPerk("specialty_rof", true, false);
                        break;
                    }
                    else if (isRayGun(weapon))
                    {
                        //players.SetClientDvar("perk_weapRateMultiplier", "1");
                        players.SetField("hasAlteredROF", true);
                        //players.SetPerk("specialty_rof", true, false);
                        players.UnSetPerk("specialty_rof", true);//Just unset it to keep server-client rate synced
                        break;
                    }
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
                    PlayFX(fx_thundergun, fxOrigin, Vector3.Zero, Vector3.Zero);//No angles because the fx is billboard
                    //Entity fx = SpawnFX(fx_thundergun, fxOrigin, Vector3.Zero, Vector3.Zero);
                    //TriggerFX(fx);
                    //AfterDelay(300, () => fx.Delete());
                    player.PlaySound("missile_attackheli_fire");

                    PhysicsExplosionCylinder(fxOrigin, 512, 128, 25);

                    if (player.CurrentWeapon == "uav_strike_projectile_mp")
                    {
                        int clip = player.GetField<int>("zeus_clip");
                        clip--;
                        if (clip < 0) clip = 0;
                        player.SetField("zeus_clip", clip);
                        if (clip > 0) player.SetWeaponAmmoClip("uav_strike_projectile_mp", 1);
                    }
                    else
                    {
                        int clip = player.GetField<int>("thundergun_clip");
                        clip--;
                        if (clip < 0) clip = 0;
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
                    AfterDelay(50, () => zapper_playZapEffect(zap));
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

            #region anticheat
            else if (message.StartsWith("menuresponse") && entRef < 18)
            {
                if (parameters[0].As<string>().StartsWith("changeclass") || parameters[0].As<string>().StartsWith("givingLoadout"))//Anti force class
                {
                    if ((string)parameters[1] != "allies_recipe1" && (string)parameters[1] != "back")
                        Utilities.ExecuteCommand("kickclient " + entRef + " MP_CHANGE_CLASS_NEXT_SPAWN");
                }
                else if ((string)parameters[0] == "team_marinesopfor")//Anti force team
                {
                    if ((string)parameters[1] != "allies")
                        Utilities.ExecuteCommand("kickclient " + entRef + " MP_CANTJOINTEAM");
                }
            }
            #endregion
        }

        private static void zapper_playZapEffect(Entity zap)
        {
            PlayFXOnTag(fx_zapperShot, zap, "tag_origin");
            PlayFXOnTag(fx_zapperTrail, zap, "tag_origin");
            zap.PlayLoopSound("tactical_insert_flare_burn");//Zap sound
        }

        private static void zapper_runFX(Entity player)
        {
            Entity fx = SpawnFX(fx_zapper, player.GetTagOrigin("tag_weapon_left"));//PlayLoopedFX(fx_zapper, .5f, origin);
            //fx.LinkTo(player, "tag_flash", Vector3.Zero, Vector3.Zero);

            OnInterval(3000, () => zapper_runSoundEffects(player));

            OnInterval(50, () => zapper_runSparks(player, fx));
        }
        private static bool zapper_runSparks(Entity player, Entity fx)
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
        }
        private static bool zapper_runSoundEffects(Entity player)
        {
            if (player.CurrentWeapon != "stinger_mp" || !player.IsAlive)
            {
                player.StopSound();
                return false;
            }
            if (player.GetAmmoCount("stinger_mp") > 0) player.PlaySound("talon_destroyed_sparks");
            return true;
        }
        private static void zapper_runZap_entCheck(Entity fx, Entity player, float time, Vector3 hitPos)
        {
            fx.Vibrate(new Vector3(1, 0, 1), 2f, .8f, time);
            fx.MoveTo(hitPos, time);
            OnInterval(50, () => zapper_runEntCheck(fx, player, hitPos));
        }
        private static bool zapper_runEntCheck(Entity fx, Entity player, Vector3 hitPos)
        {
            bool hasHit = false;
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
            OnInterval(50, () => runRaygun_entCheckLoop(fx, player, hitPos, fxName, damage));
        }
        private static bool runRaygun_entCheckLoop(Entity fx, Entity player, Vector3 hitPos, int fxName, int damage)
        {
            Entity closest = botUtil.botsInPlay.FirstOrDefault((bot) => fx.Origin.DistanceTo(bot.GetField<Entity>("hitbox").Origin) < 40);

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
            /*
            else if (weapon == "iw5_pp90m1_mp_silencer_xmags_camo10")
            {
                Vector3 origin = player.GetTagOrigin("tag_weapon_left");
                Vector3 angles = player.GetTagAngles("tag_weapon_left");
                Vector3 forward = AnglesToForward(angles);
                Vector3 up = AnglesToUp(angles);
                Entity flameFX = PlayFX_Ret(fx_flamethrower, origin, forward, up);
                AfterDelay(50, () => flameFX.Delete());
            }
            */

            if (isRayGun(weapon))
                player.PlaySound("whizby_far_00_L");
            else if (weaponIsUpgrade(weapon) && !isWeaponDeathMachine(weapon))
                player.PlaySound("whizby_far_05_L");//Alt whizby_far_00_L
        }
        private static void onWeaponSwitchPressed(Entity player)
        {
            if (player.HasField("waitingForWeaponSwitch")) return;

            StartAsync(watchWeaponChange(player));
        }
        private static IEnumerator watchWeaponChange(Entity player)
        {
            if (!player.IsAlive) yield break;

            player.SetField("waitingForWeaponSwitch", true);

            while (player.IsSwitchingWeapon())
                yield return WaitForFrame();

            hud.updateAmmoHud(player, true);

            player.ClearField("waitingForWeaponSwitch");
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
                case 4:
                    killstreaks.spawnOilSpill(entity, dropPos);
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

            OnInterval(1000, () => runIntermissionTimer(intermission));
        }

        private static bool runIntermissionTimer(HudElem intermission)
        {
            if (gameEnded) return false;
            intermissionTimerNum--;
            hud._setText(intermission, gameStrings[21] + intermissionTimerNum);

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
                roundSystem.startNextRound();
                zState = "ingame";
                return false;
            }
            else return true;
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
            hud._setText(intro, string.Format(gameStrings[22],
                            player.Name, getZombieMapname(), roundSystem.totalWaves, version));
            intro.SetPulseFX(75, 12000, 2000);
            player.SetField("hud_intro", intro);

            yield return Wait(15);

            intro.Destroy();
            player.ClearField("hud_intro");
        }

        public override void OnPlayerDamage(Entity player, Entity inflictor, Entity attacker, int damage, int dFlags, string mod, string weapon, Vector3 point, Vector3 dir, string hitLoc)
        {
            if (!player.HasField("bot"))
                return;

            if (attacker == player.GetField<Entity>("bot"))
                player.Health += damage;
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
                            //player.SetField("perk4bought", false);
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

            IPrintLn(string.Format(gameStrings[23], player.Name));

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
                        hud.scoreMessage(players, gameStrings[24] + player.Name);
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

        public static IEnumerator reviveGracePeriod(Entity player)
        {
            player.SetField("notTargetable ", true);

            yield return Wait(1);

            player.SetField("notTargetable ", false);
        }

        private static void onPlayerDeath(Entity player)
        {
            if (player.HasField("bot") && player.GetField<Entity>("bot").GetField<string>("state") != "dead") StartAsync(killstreaks.killPlayerBot(player));
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
                hud._setText(ksList, "");
                ksList.SetField("text", "");
                HudElem message = player.GetField<HudElem>("hud_message");
                hud._setText(message, "");
            }
            if (getZombieMapname() == "Moonbase")
            {
                if (player.HasField("hasHelmetOn"))
                {
                    HudElem helmet = player.GetField<HudElem>("hud_helmet");
                    helmet.Destroy();
                    player.ClearField("hasHelmetOn");
                }
                if (player.HasField("helmet")) player.ClearField("helmet");
            }

            updatePlayerCountForScoreboard();

            clearPlayerWeaponsList(player);

            player.SetField("isDown", true);//Just in case it doesn't get set prior to this stage

            AfterDelay(500, () => player.IPrintLnBold(gameStrings[25]));

            IPrintLn(string.Format(gameStrings[26], player.Name));

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

        public static void autoRevive_revivePlayer(Entity player, HudElem overlay)
        {
            if (overlay != null) overlay.Destroy();
            if (!player.IsAlive || !isPlayer(player)) return;
            player.LastStandRevive();
            player.SetField("isDown", false);
            player.SetField("autoRevive", false);
            player.EnableWeaponSwitch();
            player.EnableOffhandWeapons();
            List<string> weaponList = player.GetField<List<string>>("weaponsList");
            if (!weaponList.Contains("iw5_usp45_mp"))
            {
                player.TakeWeapon("iw5_usp45_mp");
                player.SwitchToWeapon(player.GetField<string>("lastDroppableWeapon"));
            }
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
                printToConsole(gameStrings[27]);
                using (StreamWriter newCfg = new StreamWriter("scripts\\aizombies\\config.cfg"))
                {
                    newCfg.WriteLine("//AIZombies Supreme v{0} Config File//", version);
                    newCfg.WriteLine("Game Language: {0} //The language that AIZombies will be in. Valid languages are 'english', 'spanish', 'french', 'german', 'croation', and 'serbian'.", gameLanguage);
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
                    newCfg.WriteLine("Perk Limit: {0} //The max amount of perks a player can buy. 0 is no limit.", perkLimit);
                    newCfg.WriteLine("Zombie Death Voices: {0} //Enable or disable death voices when an AI zombie dies.", botDeathVoices ? "Enabled" : "Disabled");
                    newCfg.WriteLine("Full Fire Sale: {0} //Enable or disable if fire sale powerups should spawn all weapon boxes when collected.", fullFireSale ? "Enabled" : "Disabled");
                    newCfg.WriteLine("Damage Grace Period: {0} //How long to wait before a player can be attacked after already being attacked.", damageGracePeriod);
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
            if (!text.Split('\n')[0].StartsWith($"//AIZombies Supreme v{version} Config File//"))
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
                    newCfg.WriteLine("Game Language: {0} //The language that AIZombies will be in. Valid laguages are 'english', 'spanish', 'french', 'german', 'croation', and 'serbian'.", gameLanguage);
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
                    newCfg.WriteLine("Perk Limit: {0} //The max amount of perks a player can buy. 0 is no limit.", perkLimit);
                    newCfg.WriteLine("Zombie Death Voices: {0} //Enable or disable death voices when an AI zombie dies.", botDeathVoices ? "Enabled" : "Disabled");
                    newCfg.WriteLine("Full Fire Sale: {0} //Enable or disable if fire sale powerups should spawn all weapon boxes when collected.", fullFireSale ? "Enabled" : "Disabled");
                    newCfg.WriteLine("Damage Grace Period: {0} //How long to wait before a player can be attacked after already being attacked.", damageGracePeriod);
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
                    else printToConsole(gameStrings[28], maxPlayerHealth);
                    break;
                case "maxjuggernauthealth":
                    if (int.TryParse(value, out set))
                        maxPlayerHealth_Jugg = set;
                    else printToConsole(gameStrings[29], maxPlayerHealth_Jugg);
                    break;
                case "botstartinghealth":
                    if (int.TryParse(value, out set))
                        botUtil.health = set;
                    else printToConsole(gameStrings[30], botUtil.health);
                    break;
                case "crawlerhealth":
                    if (int.TryParse(value, out set))
                        botUtil.crawlerHealth = set;
                    else printToConsole(gameStrings[31], botUtil.crawlerHealth);
                    break;
                case "bosshealth":
                    if (int.TryParse(value, out set))
                        botUtil.bossHealth = set;
                    else printToConsole(gameStrings[32], botUtil.bossHealth);
                    break;
                case "bothealthfactor":
                    if (int.TryParse(value, out set))
                        botUtil.healthScalar = set;
                    else printToConsole(gameStrings[33], botUtil.healthScalar);
                    break;
                case "botdamage":
                    if (int.TryParse(value, out set))
                        botUtil.dmg = set;
                    else printToConsole(gameStrings[34], botUtil.dmg);
                    break;
                case "perkdrops":
                        botUtil.perkDropsEnabled = isValueEnabled(value);
                    break;
                case "mapvoting":
                        voting = isValueEnabled(value);
                    break;
                case "dlcmaps":
                        dlcEnabled = isValueEnabled(value);
                    break;
                /*
                case "altweaponnames":
                    if (isEnabled(value))
                        altWeaponNames = true;
                    else
                        altWeaponNames = false;
                    break;
                */
                case "perklimit":
                    byte setB;
                    if (byte.TryParse(value, out setB))
                        perkLimit = setB;
                    else printToConsole(gameStrings[35], perkLimit);
                    break;
                case "zombiedeathvoices":
                    botDeathVoices = isValueEnabled(value);
                    break;
                case "fullfiresale":
                    fullFireSale = isValueEnabled(value);
                    break;
                case "autoupdates":
                        autoUpdate = isValueEnabled(value);
                    break;
                case "customservergametype":
                        allowServerGametypeHack = isValueEnabled(value);
                    break;
                case "customgametype":
                        allowGametypeHack = isValueEnabled(value);
                    break;
                case "gamelanguage":
                    gameLanguage = value.ToLower();
                    break;
                case "damagegraceperiod":
                    float gracePeriod;
                    if (float.TryParse(value, out gracePeriod))
                        damageGracePeriod = gracePeriod;
                    //else printToConsole(gameStrings[80], damageGracePeriod);
                    break;
            }
        }
        private static bool isValueEnabled(string value)
            => value == "enabled" || value == "1";

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
                if (_mapname == "mp_alpha") return gameStrings[36];
                else if (_mapname == "mp_bootleg") return gameStrings[37];
                else if (_mapname == "mp_bravo") return gameStrings[38];
                else if (_mapname == "mp_carbon") return gameStrings[39];
                else if (_mapname == "mp_dome") return gameStrings[40];
                else if (_mapname == "mp_exchange") return gameStrings[41];
                else if (_mapname == "mp_hardhat") return gameStrings[42];
                else if (_mapname == "mp_interchange") return gameStrings[43];
                else if (_mapname == "mp_lambeth") return gameStrings[44];
                else if (_mapname == "mp_mogadishu") return gameStrings[45];
                else if (_mapname == "mp_paris") return gameStrings[46];
                else if (_mapname == "mp_plaza2") return gameStrings[47];
                else if (_mapname == "mp_radar") return gameStrings[48];
                else if (_mapname == "mp_seatown") return gameStrings[49];
                else if (_mapname == "mp_underground") return gameStrings[50];
                else if (_mapname == "mp_village") return gameStrings[51];
                else if (_mapname == "mp_italy") return gameStrings[52];
                else if (_mapname == "mp_park") return gameStrings[53];
                else if (_mapname == "mp_morningwood") return gameStrings[54];
                else if (_mapname == "mp_overwatch") return gameStrings[55];
                else if (_mapname == "mp_aground_ss") return gameStrings[56];
                else if (_mapname == "mp_courtyard_ss") return gameStrings[57];
                else if (_mapname == "mp_cement") return gameStrings[58];
                else if (_mapname == "mp_hillside_ss") return gameStrings[59];
                else if (_mapname == "mp_meteora") return gameStrings[60];
                else if (_mapname == "mp_qadeem") return gameStrings[61];
                else if (_mapname == "mp_restrepo_ss") return gameStrings[62];
                else if (_mapname == "mp_terminal_cls") return gameStrings[63];
                else if (_mapname == "mp_crosswalk_ss") return gameStrings[64];
                else if (_mapname == "mp_six_ss") return gameStrings[65];
                else if (_mapname == "mp_burn_ss") return gameStrings[66];
                else if (_mapname == "mp_shipbreaker") return gameStrings[67];
                else if (_mapname == "mp_roughneck") return gameStrings[68];
                else if (_mapname == "mp_nola") return gameStrings[69];
                else if (_mapname == "mp_moab") return gameStrings[70];
                else if (_mapname == "mp_boardwalk") return gameStrings[71];
                else return gameStrings[72];
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
            if (weapon == "claymore_mp") return true;
            if (weapon == "airdrop_juggernaut_def_mp") return true;
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
            OnInterval(50, () => runScoreboardUpdates(player));
        }
        private static bool runScoreboardUpdates(Entity player)
        {
            if (!isPlayer(player))
            {
                player.ClearField("isViewingScoreboard");
                return false;
            }
            if (!player.GetField<bool>("isViewingScoreboard")) return true;
            player.ShowScoreBoard();
            return true;
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

            if (weapon.Contains("trophy"))
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
                player.MaxHealth = maxPlayerHealth_Jugg;
                player.Health = player.MaxHealth;
            }
            else
            {
                player.Health = maxPlayerHealth;
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
        public static IEnumerator restoreWeaponIfEmptyHanded(Entity player, int waitTime = 2)
        {
            yield return Wait(waitTime);

            if (player.CurrentWeapon == "none")
            {
                List<string> weaponsList = player.GetField<List<string>>("weaponsList");
                foreach (string weapon in weaponsList)
                {
                    if (player.HasWeapon(weapon))
                    {
                        player.SwitchToWeapon(weapon);
                        break;
                    }
                }
            }
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
            player.GiveMaxAmmo("alt_iw5_fad_mp_m320_xmags_camo11");
            player.GiveMaxAmmo("alt_iw5_ak47_mp_gp25_xmags_camo11");
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
        /*
        private static bool runGameTimeoutReset()
        {
            ResetTimeout();
            if (gameEnded) return false;
            return true;
        }
        */
        private static IEnumerator initGameVisions()
        {
            yield return WaitForFrame();

            isHellMap = mapEdit.hellMapSetting;//Init hellMap setting here
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
        }

        private static void checkForSeasons()
        {
            DateTime date = DateTime.Today;
            int month = date.Month;
            int day = date.Day;

            if (month == 12 && day == 31 && date.TimeOfDay.Hours > 21)
            {
                //Setup new years ball for dropping
                LoadFX("smoke/smoke_geotrail_hellfire");//Preload effects for usage later
                LoadFX("misc/flares_cobra");
                LoadFX("misc/laser_glow");

                Entity[] ballNodes = getAllEntitiesWithName("heli_attack_area");
                Vector3 ballLoc = ballNodes[RandomInt(ballNodes.Length)].Origin;
                Vector3 ground = GetGroundPosition(ballLoc, 20);
                Vector3 trace = PhysicsTrace(ground + new Vector3(0, 0, 60), ground);
                float traceDistance = trace.DistanceTo(ground);
                while (traceDistance > 1)
                {
                    ballLoc = ballNodes[RandomInt(ballNodes.Length)].Origin;
                    ground = GetGroundPosition(ballLoc, 20);
                    trace = PhysicsTrace(ground + new Vector3(0, 0, 60), ground);
                    traceDistance = trace.DistanceTo(ground);
                }

                ballLoc = ground;

                Entity[] tower = new Entity[15];
                for (int i = 0; i < tower.Length; i++)
                    tower[i] = mapEdit.spawnCrate(ballLoc + new Vector3(0, 0, 60 * i), new Vector3(90, 0, 0), false, false);

                mapEdit.spawnCrate(tower[tower.Length - 1].Origin, Vector3.Zero, false, false);
                mapEdit.spawnCrate(tower[tower.Length - 1].Origin + new Vector3(60, 0, 0), Vector3.Zero, false, false);
                Entity ball = Spawn("script_model", tower[tower.Length - 1].Origin + new Vector3(60, 0, -60));
                ball.SetModel("test_sphere_silver");

                OnInterval(1000, () => watchForBallDrop(ball, ballLoc));
                //AfterDelay(30000, () => watchForBallDrop(ball, ballLoc));
            }
            else if (month == 2 && day == 14)
            {
                //Valentines, allow gifting to other players
                Entity.Level.SetField("allowGifting", true);
            }
            else if (month == 4 && day == 1)
            {
                //April Fools, change bot models
                botUtil.useAltBodies = true;
            }
            else if (month == 7 && day == 4)
            {
                //fourth of july, play fireworks near end of match
                LoadFX("smoke/smoke_geotrail_hellfire");//Preload effects for usage later
                LoadFX("misc/flares_cobra");
                LoadFX("misc/laser_glow");
                roundSystem.onRoundChange += startFireworks;
            }
            else if (month == 10 && day == 31)
            {
                //Halloween
                SetSunlight(Vector3.Zero);//Darker
                vision = "sepia";//icbm_sunrise1 / aftermath
            }
            else if (month == 11 && day == 21)
            {
                //Thanksgiving, play audio of 'thank you' after every purchase
                botUtil.useAltHeads = true;
            }
            else if (month == 11 && day > 22 && day < 30 && date.DayOfWeek == DayOfWeek.Thursday)
            {
                //Black Friday, all purchases cut in half
                Entity.Level.SetField("isBlackFriday", true);
            }
            else if (month == 12 && day == 18)
            {
                Entity.Level.SetField("isXmas", true);

                //Christmas, add snow and lights if possible
                SetSunlight(Vector3.Zero);//Darker
                vision = "bog_a_sunrise";

                //spawnSnow();
                AfterDelay(1000, () =>
                {
                    foreach (Entity crate in mapEdit.usables)
                    {
                        if (crate.GetField<string>("usabletype") == "randombox") continue;

                        if (crate.GetField<string>("usabletype") == "perk-1" || crate.GetField<string>("usabletype") == "elevator")//Interchange jugg & elevators, special case
                        {
                            Vector3 crateForward = AnglesToForward(crate.Angles);
                            Vector3 crateRight = AnglesToRight(crate.Angles);
                            Vector3 crateUp = AnglesToUp(crate.Angles);

                            Entity[] fx = new Entity[4];

                            fx[0] = Spawn("script_model", crate.Origin + (crateForward * 30) + (crateRight * 15) + (crateUp * 15));
                            fx[1] = Spawn("script_model", crate.Origin + (crateForward * 30) + (crateRight * -15) + (crateUp * 15));
                            fx[2] = Spawn("script_model", crate.Origin + (crateForward * -30) + (crateRight * 15) + (crateUp * 15));
                            fx[3] = Spawn("script_model", crate.Origin + (crateForward * -30) + (crateRight * -15) + (crateUp * 15));
                            foreach (Entity tag in fx)
                            {
                                tag.SetModel("tag_origin");
                                tag.LinkTo(crate);
                            }

                            AfterDelay(200, () => PlayFXOnTag(fx_rayGunUpgrade, fx[0], "tag_origin"));
                            AfterDelay(400, () => PlayFXOnTag(fx_rayGun, fx[1], "tag_origin"));
                            AfterDelay(600, () => PlayFXOnTag(fx_rayGun, fx[2], "tag_origin"));
                            AfterDelay(800, () => PlayFXOnTag(fx_rayGunUpgrade, fx[3], "tag_origin"));

                            crate.SetField("fx_xmas", new Parameter(fx));
                        }
                        else if (crate.Model == "com_plasticcase_friendly" || crate.Model == "com_plasticcase_enemy")
                            spawnXmasLightsOnUsable(crate);
                    }
                });
            }
        }

        public static void spawnXmasLightsOnUsable(Entity crate)
        {
            Vector3 crateForward = AnglesToForward(crate.Angles);
            Vector3 crateRight = AnglesToRight(crate.Angles);
            Vector3 crateUp = AnglesToUp(crate.Angles);

            Entity[] fx = new Entity[4];

            fx[0] = SpawnFX(fx_rayGunUpgrade, crate.Origin + (crateForward * 30) + (crateRight * 15) + (crateUp * 15));
            fx[1] = SpawnFX(fx_rayGun, crate.Origin + (crateForward * 30) + (crateRight * -15) + (crateUp * 15));
            fx[2] = SpawnFX(fx_rayGun, crate.Origin + (crateForward * -30) + (crateRight * 15) + (crateUp * 15));
            fx[3] = SpawnFX(fx_rayGunUpgrade, crate.Origin + (crateForward * -30) + (crateRight * -15) + (crateUp * 15));

            TriggerFX(fx[0]);
            TriggerFX(fx[1]);
            TriggerFX(fx[2]);
            TriggerFX(fx[3]);

            crate.SetField("fx_xmas", new Parameter(fx));
        }
        private static bool watchForBallDrop(Entity ball, Vector3 basePos)
        {
            TimeSpan time = DateTime.Today.TimeOfDay;
            if (time.Hours == 23 && time.Minutes == 59 && time.Seconds > 39)
            {
                StartAsync(startBallDropCountdown(ball, basePos));
                return false;
            }
            return true;
        }
        private static IEnumerator startBallDropCountdown(Entity ball, Vector3 basePos)
        {
            Vector3 soundOrigin = ball.Origin;
            int fx_countdownGlow = LoadFX("misc/laser_glow");
            Entity fx = SpawnFX(fx_countdownGlow, soundOrigin);
            PlaySoundAtPos(soundOrigin, "ui_mp_suitcasebomb_timer");
            yield return Wait(1);
            PlaySoundAtPos(soundOrigin, "ui_mp_suitcasebomb_timer");
            yield return Wait(1);
            PlaySoundAtPos(soundOrigin, "ui_mp_suitcasebomb_timer");
            yield return Wait(1);
            PlaySoundAtPos(soundOrigin, "ui_mp_suitcasebomb_timer");
            yield return Wait(1);
            PlaySoundAtPos(soundOrigin, "ui_mp_suitcasebomb_timer");
            yield return Wait(1);
            PlaySoundAtPos(soundOrigin, "ui_mp_suitcasebomb_timer");
            yield return Wait(1);
            PlaySoundAtPos(soundOrigin, "ui_mp_suitcasebomb_timer");
            yield return Wait(1);
            PlaySoundAtPos(soundOrigin, "ui_mp_suitcasebomb_timer");
            yield return Wait(1);
            PlaySoundAtPos(soundOrigin, "ui_mp_suitcasebomb_timer");
            yield return Wait(1);
            PlaySoundAtPos(soundOrigin, "ui_mp_suitcasebomb_timer");
            yield return Wait(1);
            PlaySoundAtPos(soundOrigin, "ui_mp_suitcasebomb_timer");
            TriggerFX(fx);
            yield return Wait(1);
            PlaySoundAtPos(soundOrigin, "ui_mp_suitcasebomb_timer");
            TriggerFX(fx);
            yield return Wait(1);
            PlaySoundAtPos(soundOrigin, "ui_mp_suitcasebomb_timer");
            TriggerFX(fx);
            yield return Wait(1);
            PlaySoundAtPos(soundOrigin, "ui_mp_suitcasebomb_timer");
            TriggerFX(fx);
            yield return Wait(1);
            PlaySoundAtPos(soundOrigin, "ui_mp_suitcasebomb_double_timer");
            TriggerFX(fx);
            yield return Wait(1);
            PlaySoundAtPos(soundOrigin, "ui_mp_suitcasebomb_double_timer");
            TriggerFX(fx);
            yield return Wait(1);
            PlaySoundAtPos(soundOrigin, "ui_mp_suitcasebomb_double_timer");
            TriggerFX(fx);
            yield return Wait(1);
            PlaySoundAtPos(soundOrigin, "ui_mp_suitcasebomb_double_timer");
            TriggerFX(fx);
            yield return Wait(1);
            PlaySoundAtPos(soundOrigin, "ui_mp_suitcasebomb_double_timer");
            TriggerFX(fx);
            yield return Wait(1);

            fx.Delete();
            PlaySoundAtPos(soundOrigin, "ac130_40mm_fire_npc");
            //ball.PhysicsLaunchServer(Vector3.Zero, Vector3.Zero);
            ball.MoveGravity(Vector3.Zero, 2);

            yield return Wait(1.5f);

            StartAsync(bonusDrops.doNuke(ball));
            PlaySoundAtPos(soundOrigin, "us_victory_music");

            for (int i = 0; i < 8; i++)
                AfterDelay(RandomIntRange(50, 5000), () => StartAsync(launchFirework(basePos.Around(100))));
        }

        private static void startFireworks()
        {
            for (int i = 0; i < 10; i++)
                AfterDelay(RandomIntRange(50, 10000), () => StartAsync(launchFirework(getRandomSpawnpoint().Origin.Around(1000))));
        }

        public static IEnumerator launchFirework(Vector3 location)
        {
            int fx_trail = LoadFX("smoke/angel_flare_geotrail");
            if (fx_trail == 0) yield break;
            int fx_explode = LoadFX("misc/flares_cobra");
            if (fx_explode == 0) yield break;

            Entity firework = Spawn("script_model", location);
            firework.SetModel("projectile_smartarrow");

            yield return Wait(.1f);

            PlayFXOnTag(fx_trail, firework, "tag_fx");
            PlaySoundAtPos(location, "reaper_fire");//ac130_25mm_fire_npc
            //firework.PlayLoopSound("move_rpg_proj_loop_1");

            Vector3 endPos = location + new Vector3(0, 0, 5000);
            endPos = endPos.Around(200);
            Vector3 angles = VectorToAngles(endPos - location);
            firework.Angles = angles;

            firework.MoveTo(endPos, 5, .05f, .5f);

            yield return Wait(5f);

            firework.Delete();
            for (int i = 0; i < 5; i++)
                PlayFX(fx_explode, endPos, Vector3.RandomXY(), Vector3.RandomXY());

            PlaySoundAtPos(endPos, "exp_ac130_40mm");

            yield return Wait(.1f);

            PlaySoundAtPos(endPos, "ac130_flare_burst");
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
            AfterDelay(1000, () => Utilities.ExecuteCommand("kickclient " + player.EntRef + gameStrings[73]));
            return false;
        }

        private void patchGame()
        {
            //This function is used to patch memory for certain features/tweaks
            memoryScanning.Mem.InitMemory();

            //Gametype setup
            if (allowServerGametypeHack) AfterDelay(500, memoryScanning.writeToServerInfoString);
            if (allowGametypeHack) AfterDelay(1000, memoryScanning.writeGameInfoString);

            if (GetDvarInt("aiz_appliedGamePatches") == 0) memoryScanning.writeWeaponPatches();
        }
#region memory scanning
        public class memoryScanning
        {
            [DllImport("kernel32.dll")]
            private static extern int VirtualQuery(IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);
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
            private static string[] weaponPatches = new string[] { "uav_strike_marker_mp", "at4_mp", "stinger_mp", "iw5_xm25_mp", "gl_mp", "uav_strike_missile_mp", "uav_strike_projectile_mp", "sentry_minigun_mp", killstreaks.botWeapon_subBot, killstreaks.botWeapon_LMGBot, "iw5_skorpion_mp" };
            private static List<IntPtr> raygunFireRatePtrs = new List<IntPtr>();
            private static int baseAddress = 0x0;
            private static int serverInfoStringAddress = 0x0;
            private static readonly int maxServerInfoStringTries = 10;
            private static int currentServerInfoStringTries = 0;

            public static class Mem
            {
                public static void InitMemory()
                {
                    SetDvarIfUninitialized("sv_serverInfoStringAddress", 0);
                    SetDvarIfUninitialized("sv_serverInfoStringAddressList", 0);
                    serverInfoStringAddress = GetDvarInt("sv_serverInfoStringAddress");

                    int baseAddressDvar = GetDvarInt("sv_baseAddress");
                    if (baseAddressDvar != 0)
                    {
                        baseAddress = baseAddressDvar;
                    }
                    else
                    {
                        Process thisProcess = Process.GetCurrentProcess();
                        ProcessModule thisModule = thisProcess.MainModule;
                        baseAddress = thisModule.BaseAddress.ToInt32();
                        SetDvarIfUninitialized("sv_baseAddress", baseAddress);
                        thisProcess.Dispose();
                    }
                }
                public static bool CanReadMemory(IntPtr address, uint length)
                {
                    MEMORY_BASIC_INFORMATION mem;
                    VirtualQuery(address, out mem, length);

                    if (mem.Protect == 0x40 || mem.Protect == 0x04 || mem.Protect == 0x02) return true;
                    return false;
                }
                public static string ReadString(int address, int maxlen = 0)
                    => ReadString(new IntPtr(address), maxlen);
                public static string ReadString(IntPtr address, int maxlen = 0)
                {
                    string ret = "";
                    maxlen = (maxlen == 0) ? int.MaxValue : maxlen;

                    byte[] buffer = new byte[maxlen];

                    Marshal.Copy(address, buffer, 0, maxlen);

                    ret = Encoding.ASCII.GetString(buffer);

                    return ret;
                }

                public static void WriteString(IntPtr address, string str, bool endZero = true)
                {
                    //if (!canReadAndWriteMemory(address, 1024)) return;

                    byte[] strarr = Encoding.ASCII.GetBytes(str);

                    Marshal.Copy(strarr, 0, address, strarr.Length);
                    if (endZero) Marshal.WriteByte(address + str.Length, 0);
                }
                public static IntPtr getProcessBaseAddress()
                    => Process.GetCurrentProcess().MainModule.BaseAddress;
                public static int getPtrAtLoc(int loc)
                    => Marshal.ReadInt32(new IntPtr(loc));
            }
            public static List<IntPtr> scanForServerInfo(int min, int max)
            {
                Process p = Process.GetCurrentProcess();
                List<IntPtr> ptrs = new List<IntPtr>();
                IntPtr currentAddr = new IntPtr(min);
                int bufferSize = 1024;
                byte[] buffer = new byte[bufferSize];
                string s = null;
                string test = @"gn\IW4\gt\aiz";
                string key = @"gn\IW5\gt\";

                for (; (int)currentAddr < max; currentAddr += 1024)
                {
                    if (!Mem.CanReadMemory(currentAddr, (uint)bufferSize)) continue;

                    s = null;
                    Marshal.Copy(currentAddr, buffer, 0, bufferSize);
                    s = Encoding.ASCII.GetString(buffer);

                    if (!string.IsNullOrEmpty(s))
                    {
                        if (s.Contains(key))
                        {
                            int offset = s.IndexOf("gn");

                            //Find out if this is real or not
                            Mem.WriteString(currentAddr + offset, test, false);
                            System.Threading.Thread.Sleep(50);
                            byte[] returnBuffer = new byte[test.Length];
                            string returned = Mem.ReadString(currentAddr + offset, 13);

                            if (test == returned)
                            {
                                ptrs.Add(currentAddr + offset);
                                //Utilities.PrintToConsole("Adding ptr " + (currentAddr + offset).ToString("X"));
                            }
                        }
                    }
                }
                return ptrs;
            }
            public static void scanServerInfo(object sender, DoWorkEventArgs e)
            {
                int[] arguments = e.Argument as int[];
                e.Result = scanForServerInfo(arguments[0], arguments[1]);
            }
            public static void scanForServerInfoString(int min, int max)
            {
                string sv_serverinfo_addr = GetDvar("sv_serverInfoStringAddressList");
                if (string.IsNullOrEmpty(sv_serverinfo_addr) || sv_serverinfo_addr == "0") //first start
                {
                    BackgroundWorker task = new BackgroundWorker();
                    task.DoWork += scanServerInfo;
                    task.RunWorkerAsync(new int[2] { min, max });

                    task.RunWorkerCompleted += new RunWorkerCompletedEventHandler(scanServerInfo_Completed);
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
                            AfterDelay(i, () => memoryScanning.Mem.WriteString(new IntPtr(addr), modeText));
                        }
                    }
                }
            }
            private static void scanServerInfo_Completed(object sender, RunWorkerCompletedEventArgs e)
            {
                ((BackgroundWorker)sender).Dispose();
                if (e.Cancelled)
                {
                    Utilities.PrintToConsole(gameStrings[74]);
                    return;
                }
                if (e.Error != null)
                {
                    Utilities.PrintToConsole(gameStrings[75] + e.Error.Message);
                    return;
                }

                List<IntPtr> addrs = e.Result as List<IntPtr>;
                if (addrs.Count == 0)
                {
                    Utilities.PrintToConsole(gameStrings[76]);
                    return;
                }

                setServerInfoPtrs(addrs);

               
            }
            private static void setServerInfoPtrs(List<IntPtr> addrs)
            {
                if (addrs.Count > 0)
                {
                    //save found address(es)
                    string addrDvar = string.Join(" ", addrs);
                    SetDvar("sv_serverInfoStringAddressList", addrDvar);
                    for (int i = 50; i <= addrs.Count * 50; i += 50)
                    {
                        int index = (i / 50) - 1;
                        IntPtr addr = addrs[index];
                        AfterDelay(i, () => memoryScanning.Mem.WriteString(addr, modeText));
                    }
                }
                else
                {
                    Utilities.PrintToConsole(gameStrings[84]);
                    return;
                }
            }
            public static List<IntPtr> scanForGameInfo()
            {
                List<IntPtr> ptrs = new List<IntPtr>();
                IntPtr currentAddr = new IntPtr(0x01B00000);
                int bufferSize = 512;
                byte[] buffer = new byte[bufferSize];
                string s = null;
                string key = @"\g_gametype\war\g_hardcore\";

                for (; (int)currentAddr < 0x01D00000; currentAddr += bufferSize)
                {
                    if (!Mem.CanReadMemory(currentAddr, (uint)bufferSize)) continue;

                    s = null;
                    Marshal.Copy(currentAddr, buffer, 0, bufferSize);
                    s = Encoding.ASCII.GetString(buffer);//Mem.ReadString(currentAddr, 512);

                    if (!string.IsNullOrEmpty(s))
                    {
                        //Utilities.PrintToConsole("Address " + currentAddr.ToString("X"));
                        if (s.Contains(key) && s.Contains(_mapname))
                        {
                            int offset = s.IndexOf("\\g_gametype");

                            //if (Marshal.ReadInt32(new IntPtr(currentAddr.ToInt32() + offset - 0x04)) != 1)
                                //continue;

                            //Utilities.PrintToConsole("Adding game ptr " + (currentAddr + offset).ToString("X"));
                            
                            ptrs.Add(currentAddr + offset);
                        }
                    }
                }

                return ptrs;
            }
            public static void scanGameInfo(object sender, DoWorkEventArgs e)
            {
                e.Result = scanForGameInfo();
            }
            public static void writeToServerInfoString()
            {
                IntPtr serverInfoPtr = new IntPtr(0x0013DD0C);
                /*
                if (!Mem.CanReadMemory(serverInfoPtr, 13))//If we can't access the ptr then just do a search
                {
                    memoryScanning.scanForServerInfoString(0x04000000, 0x0A000000);
                    return;
                }
                */
                string sv_serverinfo_addr = GetDvar("sv_serverInfoStringAddressList");
                if (sv_serverinfo_addr != "0") //If we already searched just use the search
                {
                    memoryScanning.scanForServerInfoString(0, 0);
                    return;
                }
                if (serverInfoStringAddress == 0)
                {
                    int infoStringAddress = Marshal.ReadInt32(serverInfoPtr);
                    if (infoStringAddress == 0x0 || infoStringAddress > 0x0A000000 || infoStringAddress < 0x04000000)
                    {
                        currentServerInfoStringTries++;
                        //If this ptr isn't working out then just do a scan
                        if (currentServerInfoStringTries >= maxServerInfoStringTries)
                        {
                            memoryScanning.scanForServerInfoString(0x04000000, 0x0A000000);
                            return;
                        }
                        AfterDelay(50, memoryScanning.writeToServerInfoString);
                        return;
                    }
                    infoStringAddress--;
                    serverInfoStringAddress = infoStringAddress;
                    SetDvar("sv_serverInfoStringAddress", serverInfoStringAddress);
                    writeServerInfoStringToPtr(serverInfoStringAddress);
                }
                else
                    writeServerInfoStringToPtr(serverInfoStringAddress);
            }
            private static void writeServerInfoStringToPtr(int? ptr = null)
            {
                int targetLoc = serverInfoStringAddress;
                if (ptr.HasValue)
                    targetLoc = ptr.Value;

                string output = Mem.ReadString(serverInfoStringAddress, 13);
                //Utilities.PrintToConsole("Server Output: " + output);

                if (output.StartsWith(@"gn\IW5\gt\war"))
                    memoryScanning.Mem.WriteString(new IntPtr(targetLoc), modeText);
            }
            
            private static void scanGameInfo_Completed(object sender, RunWorkerCompletedEventArgs e)
            {
                ((BackgroundWorker)sender).Dispose();
                if (e.Cancelled)
                {
                    Utilities.PrintToConsole(gameStrings[77]);
                    return;
                }
                if (e.Error != null)
                {
                    Utilities.PrintToConsole(gameStrings[78] + e.Error.Message);
                    return;
                }

                List<IntPtr> addrs = e.Result as List<IntPtr>;
                if (addrs.Count == 0)
                {
                    Utilities.PrintToConsole(gameStrings[79]);
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
            public static void writeWeaponPatches()
            {
                //Utilities.PrintToConsole("Writing weapon patches");

                IntPtr weaponLoc = new IntPtr(Mem.getPtrAtLoc(baseAddress + 0x012B35C8));
                string weaponName = Mem.ReadString(weaponLoc, 10);
                if (weaponName == "stinger_mp")
                {
                    //Utilities.PrintToConsole(weaponName);
                    //Stock value
                    Marshal.WriteInt32(weaponLoc + 0x230, 8);
                    //requireLockOn
                    Marshal.WriteByte(weaponLoc + 0x776, 0);
                    //adsFire
                    Marshal.WriteByte(weaponLoc + 0x788, 0);
                    //projImpactExplode
                    Marshal.WriteByte(weaponLoc + 0x796, 0);
                }

                weaponLoc = new IntPtr(Mem.getPtrAtLoc(baseAddress + 0x00F8AC80));
                weaponLoc += 0x27;
                weaponName = Mem.ReadString(weaponLoc, 20);
                if (weaponName == "uav_strike_marker_mp")
                {
                    //Utilities.PrintToConsole(weaponName);
                    //Stock value
                    Marshal.WriteInt32(weaponLoc + 0x239, 2);
                    //noAdsWhenMagEmpty
                    Marshal.WriteByte(weaponLoc + 0x782, 0);
                    //adsFire
                    Marshal.WriteByte(weaponLoc + 0x791, 0);
                }

                weaponLoc = new IntPtr(Mem.getPtrAtLoc(baseAddress + 0x00F8C540));
                weaponLoc += 0x2A;
                weaponName = Mem.ReadString(weaponLoc, 6);
                if (weaponName == "at4_mp")
                {
                    //Utilities.PrintToConsole(weaponName);
                    //Stock value
                    Marshal.WriteInt32(weaponLoc + 0x22E, 8);
                }

                weaponLoc = new IntPtr(Mem.getPtrAtLoc(baseAddress + 0x012B1774));
                weaponName = Mem.ReadString(weaponLoc, 11);
                if (weaponName == "iw5_xm25_mp")
                {
                    //Utilities.PrintToConsole(weaponName);
                    //Stock value
                    Marshal.WriteInt32(weaponLoc + 0x230, 8);
                }

                weaponLoc = new IntPtr(Mem.getPtrAtLoc(baseAddress + 0x00F8C060));
                weaponLoc += 0x01;
                weaponName = Mem.ReadString(weaponLoc, 5);
                if (weaponName == "gl_mp")
                {
                    //Utilities.PrintToConsole(weaponName);
                    //Stock value
                    Marshal.WriteInt32(weaponLoc + 0x22B, 8);
                }

                weaponLoc = new IntPtr(Mem.getPtrAtLoc(baseAddress + 0x01296B58));
                weaponName = Mem.ReadString(weaponLoc, 21);
                if (weaponName == "uav_strike_missile_mp")
                {
                    //Utilities.PrintToConsole(weaponName);
                    //aimDownSight
                    Marshal.WriteByte(weaponLoc + 0x78C, 0);
                    //adsFire
                    Marshal.WriteByte(weaponLoc + 0x794, 0);
                    //projImpactExplode
                    Marshal.WriteByte(weaponLoc + 0x7A2, 0);
                }

                weaponLoc = new IntPtr(Mem.getPtrAtLoc(baseAddress + 0x01298268));
                weaponName = Mem.ReadString(weaponLoc, 24);
                if (weaponName == "uav_strike_projectile_mp")
                {
                    //Utilities.PrintToConsole(weaponName);
                    //requireLockOn
                    Marshal.WriteByte(weaponLoc + 0x786, 0);
                    //aimDownSight
                    Marshal.WriteByte(weaponLoc + 0x790, 0);
                    //adsFire
                    Marshal.WriteByte(weaponLoc + 0x798, 0);
                    //projImpactExplode
                    Marshal.WriteByte(weaponLoc + 0x7A6, 0);
                }

                weaponLoc = new IntPtr(Mem.getPtrAtLoc(baseAddress + 0x0127A0E8));
                weaponName = Mem.ReadString(weaponLoc, 17);
                if (weaponName == "sentry_minigun_mp")
                {
                    //Utilities.PrintToConsole(weaponName);
                    //Damage
                    Marshal.WriteInt32(weaponLoc + 0x24C, 0);
                    //Ranged Damage
                    Marshal.WriteInt32(weaponLoc + 0x6B0, 0);
                }

                weaponLoc = new IntPtr(Mem.getPtrAtLoc(baseAddress + 0x0127CEC0));
                weaponName = Mem.ReadString(weaponLoc, killstreaks.botWeapon_subBot.Length);
                if (weaponName == killstreaks.botWeapon_subBot)
                {
                    //Utilities.PrintToConsole(weaponName);
                    //Damage
                    Marshal.WriteInt32(weaponLoc + 0x25A, 0);
                }

                weaponLoc = new IntPtr(Mem.getPtrAtLoc(baseAddress + 0x01277AB4));
                weaponName = Mem.ReadString(weaponLoc, killstreaks.botWeapon_LMGBot.Length);
                if (weaponName == killstreaks.botWeapon_LMGBot)
                {
                    //Utilities.PrintToConsole(weaponName);
                    //Damage
                    Marshal.WriteInt32(weaponLoc + 0x248, 0);
                    Marshal.WriteInt32(weaponLoc + 0x24C, 0);
                }
                
                weaponLoc = new IntPtr(Mem.getPtrAtLoc(baseAddress + 0x003B7524));
                weaponLoc += 0x5C;
                if (Marshal.ReadInt32(weaponLoc) == 70)
                {
                    //Utilities.PrintToConsole("iw5_skorpion_mp");
                    //Fire rate
                    Marshal.WriteInt32(weaponLoc, 300);
                }

                SetDvar("aiz_appliedGamePatches", 1);
            }
            public static void writeGameInfoString()
            {
                BackgroundWorker task = new BackgroundWorker();
                task.DoWork += scanGameInfo;
                task.RunWorkerAsync();

                task.RunWorkerCompleted += new RunWorkerCompletedEventHandler(scanGameInfo_Completed);
            }
        }
#endregion
        private static void writeGameInfo()
        {
            //Log.Debug("Writing to " + string.Join(", ", gameInfo));
            foreach (int ptr in gameInfo)
            {
                if (ptr == 0) return;

                try
                {
                    string infoText = memoryScanning.Mem.ReadString(ptr, 350);
                    if (!infoText.Contains("\\g_gametype\\")) return;
                    infoText = infoText.Trim();
                    infoText = infoText.Replace("g_gametype\\war", "g_gametype\\^2AIZombies Supreme");
                    memoryScanning.Mem.WriteString(new IntPtr(ptr), infoText);
                }
                catch
                {
                    printToConsole(gameStrings[88]);
                }
            }
        }
        public static void restoreGameInfo()
        {
            if (!allowGametypeHack) return;
            //Log.Debug("Restoring to " + string.Join(", ", gameInfo));
            foreach (int ptr in gameInfo)
            {
                if (ptr == 0) return;

                try
                { 
                    string infoText = memoryScanning.Mem.ReadString(ptr, 350);
                    if (!infoText.Contains("g_gametype\\")) return;
                    infoText = infoText.Trim();
                    infoText = infoText.Replace("g_gametype\\^2AIZombies Supreme", "g_gametype\\war");
                    memoryScanning.Mem.WriteString(new IntPtr(ptr), infoText);
                }
                catch
                {
                    printToConsole(gameStrings[89]);
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
                case "mp_paris":
                    return "test_sphere_redchrome";
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
            string privateClients = GetDvar("sv_privateClients");
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

            Utilities.PrintToConsole(gameStrings[90]);

            if (blacklistedScripts.Count != 0)
            {
                restoreGameInfo();
                Utilities.ExecuteCommand("map_restart");
            }
        }

        private static string[] initGameStrings()
        {
            string[] strings = new string[341];

            if (File.Exists("scripts\\aizombies\\config.cfg"))
            {
                using (StreamReader cfg = new StreamReader("scripts\\aizombies\\config.cfg"))//Read directly from language cfg since this runs before loadConfig()
                {
                    while (!cfg.EndOfStream)
                    {
                        string line = cfg.ReadLine();
                        if (line.StartsWith("Game Language:"))
                        {
                            line = clipSpaces(line);
                            line = line.Split('/')[0];
                            line = line.ToLowerInvariant();
                            string setting = line.Split(':')[0];
                            string value = line.Split(':')[1];
                            setGameSetting(setting, value);
                            break;
                        }
                    }
                }
            }

            //Utilities.PrintToConsole("Using language " + gameLanguage);

            switch (gameLanguage)
            {
                #region serbian/croation
                //Credits to SvetaSrbin
                case "serbian":
                case "croation":
                    strings[0] = "Ljudi su Pobedili Zombije!";
                    strings[1] = "Ljudi su preživeli!";
                    strings[2] = "Dobar posao ljudi!";
                    strings[3] = "Ljudi su ostali živi!";
                    strings[4] = "Ljudsko lice: :D!";
                    strings[5] = "Oldièno! Ljudi nastavljaju da žive!";
                    strings[6] = "Odlièan posao ljudi!";
                    strings[7] = "Dobar posao, Spremite se za sledeæi napad!";
                    strings[8] = "Zombiji su takvi perverznjaci... Ljudi za pobedu!";
                    strings[9] = "Ljudi: 1, Zombi: 0";
                    strings[10] = "Ljudi su pobedili kuèke!";
                    strings[11] = "Pobeda!!!";
                    strings[12] = "Neprijatelj je pao!!!";
                    strings[13] = "Laganica!";
                    strings[14] = "Moraš pokrenuti AIZombies Supreme on Team Deathmatch!";
                    strings[15] = "Trenutni maksimalni broj igraèa za AIZombies može biti 8 ili manje. Trenutna postavka je {0}. Postavljeno je na 8.";
                    strings[16] = "^1AIZombies Supreme Napravljeni od strane ^2Slvr99";
                    strings[17] = "^2Ljudi";
                    strings[18] = "^1Saèekajte kraj runde,da bi ste krenuli opet!";
                    strings[19] = "Preživeli {0} talas.";
                    strings[20] = "Spremite se za napad:";
                    strings[21] = "Sledeæa runda za: ";
                    strings[22] = "^2Dobrodošli {0}!\n^1AIZombies Supreme {3}\n^3Mapa: {1}\n^2Napravljeni od strane Slvr99\n^5Prezivi {2} Talas.";
                    strings[23] = "^1{0} ^1Treba oživljavanje!";
                    strings[24] = "^1Nije uspeo da se oživi ";
                    strings[25] = "^1Umro si. Saèekaj kraj runde,da bi krenuo opet.";
                    strings[26] = "^1{0} ^1 je ubijen.";
                    strings[27] = "Konfiguracioni fajl za AIZombies nije pronaðen! Kreiranje jednog...";
                    strings[28] = "Maksimalno zdravlje je postavljeno na pogrešnu vrednost u konfiguraciji!, Podesi na podrazumevano ({0})";
                    strings[29] = "Maksimalno Juggernog zdravlje je postavljeno na pogrešnu vrednost u konfiguraciji!, Podesi na podrazumevano ({0})";
                    strings[30] = "Zdravlje Bota je postavljeno na pogrešnu vrednost u konfiguraciji!, Podesi na podrazumevano ({0})";
                    strings[31] = "Crawler zdravlje je postavljeno na pogrešnu vrednost u konfiguraciji!, Podesi na podrazumevano ({0})";
                    strings[32] = "Boss zdravlje je postavljeno na pogrešnu vrednost u konfiguraciji!, Podesi na podrazumevano ({0})";
                    strings[33] = "Bot Health Factor je postavljen na pogrešnu vrednost u konfiguraciji!, Podesi na podrazumevano ({0})";
                    strings[34] = "Šteta Bota je podešena na pogrešnu vrednost u konfiguraciji!, Podesi na podrazumevano ({0})";
                    strings[35] = "Ogranièenje povlastica je postavljeno na pogrešnu vrednost u konfiguraciji!, Podesi na podrazumevano ({0})";
                    strings[36] = "Ulice Smrti";
                    strings[37] = "Oluja epidemije";
                    strings[38] = "Rundown Selo";
                    strings[39] = "Naftna rafinerija";
                    strings[40] = "Pustinjska ispostava";
                    strings[41] = "Napuštena podzemna železnica";
                    strings[42] = "Gradilište pakla";
                    strings[43] = "Srušeni podvožnjak";
                    strings[44] = "Napušteni put pakla";
                    strings[45] = "Docked Death";
                    strings[46] = "Ulice smrti";
                    strings[47] = "Apetit za kosti";
                    strings[48] = "Pogon pakla";
                    strings[49] = "Primorski hotel pakla";
                    strings[50] = "Gomila bola";
                    strings[51] = "Velika Crna smrt";
                    strings[52] = "El Brote";
                    strings[53] = "Wartorn";
                    strings[54] = "U izgradnji";
                    strings[55] = "Smrt u toku";
                    strings[56] = "Shipwrecked";
                    strings[57] = "Mrtvi Aqueduct";
                    strings[58] = "Silla Cement";
                    strings[59] = "Oceanside napad";
                    strings[60] = "Utoèište pakla";
                    strings[61] = "Raj";
                    strings[62] = "Ostaci pakla";
                    strings[63] = "Red smrti";
                    strings[64] = "Pandemic most";
                    strings[65] = "Nemrtva farma";
                    strings[66] = "Skrovište od pakla";
                    strings[67] = "U gradu";
                    strings[68] = "Naftna bušotina";
                    strings[69] = "Avenija smrti";
                    strings[70] = "Zarobljeni Kanjon";
                    strings[71] = "Pristanište";
                    strings[72] = "^1Nepoznata Mapa!";
                    strings[73] = "Molimo vas da ne imitirate developere.";
                    strings[74] = "Server gametype name search je otkazan iz nepoznatih razloga.";
                    strings[75] = "Došlo je do greške prilikom postavljanja servera za gametype name!: ";
                    strings[76] = "Došlo je do greške prilikom postavljanja gametype name: Nije pronaðena adresa!";
                    strings[77] = "Gametype pretraživanje imena j otkazano iz nepoznatih razloga.";
                    strings[78] = "Došlo je do greške prilikom postavljanja gametype imena!: ";
                    strings[79] = "Došlo je do greške prilikom podešavanja gametype imena : Adresa nije pronaðena!";
                    strings[80] = "Pritisnite ^3[{+activate}] ^7da pokupite Sentry Gun";
                    strings[81] = "Traženje oružja je dovršeno.";
                    strings[82] = "Potraga za oružjem je otkazana iz nepoznatih razloga! To može dovesti fo toga da se pojave bagovi za odreðena oružja.";
                    strings[83] = "Došlo je do greške pronalaženja oružja patch lokacije: ";
                    strings[84] = "Nije moguæe postaviti prilagoðeno ime u pregledaèu servera!";
                    strings[85] = "Nije bilo moguæe pronaæi podatke o oružju za {0}! Molim vas prijavite ovu grešku Slvr99";
                    strings[86] = "Boardwalk";
                    strings[87] = "Sajam na plaži koji su prekinuli nemrtvi";
                    strings[88] = "Nije moguæe postaviti prilagoðeno gametype ime.";
                    strings[89] = "Nije moguæe vratiti u prethodno stanje prilagoðenu nisku režima za igru! Ovo može prouzrokovati pad servera kada se mapa promeni!";
                    strings[90] = "Sve skripte treæeg lica su uèitane sa ovog servera zbog konflikta sa AIZombies.";
                    strings[91] = "Došlo je do greške prilikom obraæanja na serveru za proveru ispravke!: {0}";
                    strings[92] = "Došlo je do greške prilikom obraæanja na serveru za proveru ispravke!: Nema odgovora od servera.";
                    strings[93] = "Postoji ispravka(update) AIZombies ! Preuzmi verziju {0} sada...";
                    strings[94] = "Došlo je do greške prilikom preuzimanja ispravke(update) sa servera!: {0}";
                    strings[95] = "Došlo je do greške prilikom preuzimanja stare AIZombies datoteke! Uverite se da datoteka nije samo za èitanje,ili da nije otvorena negde drugde.";
                    strings[96] = "Preuzimanje završeno! Ispravke(update) æe stupiti na snagu kada se igra završi.";
                    strings[97] = "Ekstra novac!";
                    strings[98] = "Ekstra bonus poeni!";
                    strings[99] = "Nasumièni Perk!";
                    strings[100] = "Nema dostupnih botova! Molim vas da imate bar jedan \"zombiespawn\" u vašem fajlu za mape.";
                    strings[101] = "^1Nema dostupnih botova! Proverite konzolu za detalje";
                    strings[102] = "Više helta";
                    strings[103] = "Trèi brže i duže";
                    strings[104] = "Napuni brže";
                    strings[105] = "Dodatni slot za oružje";
                    strings[106] = "Brža brzina pucanja";
                    strings[107] = "Brže se kreæi dok nišaniš";
                    strings[108] = "Automatski budi oživljen brzo nakon pada\n+ Oživi svoj tim brže";
                    strings[109] = "Scavenge Free Ammo";
                    strings[110] = "Lockdown";
                    strings[111] = "Bootleg";
                    strings[112] = "Mission";
                    strings[113] = "Carbon";
                    strings[114] = "Dome";
                    strings[115] = "Downturn";
                    strings[116] = "Hardhat";
                    strings[117] = "Interchange";
                    strings[118] = "Fallen";
                    strings[119] = "Bakaara";
                    strings[120] = "Resistance";
                    strings[121] = "Arkaden";
                    strings[122] = "Outpost";
                    strings[123] = "Seatown";
                    strings[124] = "Underground";
                    strings[125] = "Village";
                    strings[126] = "Piazza";
                    strings[127] = "Liberation";
                    strings[128] = "Black Box";
                    strings[129] = "Overwatch";
                    strings[130] = "Aground";
                    strings[131] = "Erosion";
                    strings[132] = "Foundation";
                    strings[133] = "Getaway";
                    strings[134] = "Sanctuary";
                    strings[135] = "Oasis";
                    strings[136] = "Lookout";
                    strings[137] = "Terminal";
                    strings[138] = "Intersection";
                    strings[139] = "Vortex";
                    strings[140] = "U-Turn";
                    strings[141] = "Decommission";
                    strings[142] = "Offshore";
                    strings[143] = "Parish";
                    strings[144] = "Gulch";
                    strings[145] = "Jednom mirne ulice preplavljuju nemrtvi";
                    strings[146] = "Stormy town je zauzet od strane nemrtvih";
                    strings[147] = "A rundown vselo je zauzeto \nod strane nemrtvih";
                    strings[148] = "Nekada žive naftne rainerije";
                    strings[149] = "Napuštena ispostava u pustinji";
                    strings[150] = "Posledice opasne greške";
                    strings[151] = "Mala nedovršena gradilišta";
                    strings[152] = "Uništeni autoput od \nposledica izbijanja";
                    strings[153] = "Napušteni Ruski grad duhova";
                    strings[154] = "Afrièki grad napadnut od strane nemrtvih";
                    strings[155] = "Pariski okrug više nije bezbedan";
                    strings[156] = "Nemaèki trgovaèki centar opljaèkan od strane nemrtvih";
                    strings[157] = "Sibirska ispostavka evakuisana\nusled izbijanja virusa";
                    strings[158] = "Obalni grad nije bezbedan\nod nemrvih";
                    strings[159] = "Stanica podzemne železnice se koristikao\nevakuaciona ruta";
                    strings[160] = "Afrièko selo\nsa opasnom zarazom";
                    strings[161] = "Španski grad zaražen mrtvima";
                    strings[162] = "Vojna baza izgraðena u parku\nza zaštitu od nemrtvih";
                    strings[163] = "Mesto nesreæe pada aviona\npruzrokovano napadom virusa";
                    strings[164] = "Neboder napušten\nzbog izbijanja virusa";
                    strings[165] = "Olupina broda prouzrokovana virusom na moru";
                    strings[166] = "Aqueduct je zagaðen od snemrtvih";
                    strings[167] = "Fabrika cementa je zauzeta od strane nemrtvih";
                    strings[168] = "Mesto odmora koje je pretvoreno\nu noænu moru";
                    strings[169] = "Svetilište koje je razbijeno\nod èistoæe nemrtvih";
                    strings[170] = "Oaza koja je bila pregažena nemrtvima";
                    strings[171] = "Ostatak Reto je koristen za sve\nosim odmora";
                    strings[172] = "Ruski aerodrom preuzet od nemrtvih";
                    strings[173] = "Autoput u centru za napade";
                    strings[174] = "Napuštenu farmu je pogodilo\nviše od jednog problema";
                    strings[175] = "Autoput u pustinji nije siguran od nemrtvih";
                    strings[176] = "Olupina broda blizu grada\nkoji je inficiran";
                    strings[177] = "Naftna platforma je preplavljena\nviše od juggernauts";
                    strings[178] = "Zabavni grad nikada nije bio\nzauzet od strane nemrtvih";
                    strings[179] = "Pustinjski kanjon koji napreduje sa nemrtvima ";
                    strings[180] = "Novac: $";
                    strings[181] = "Bonus Poeni:";
                    strings[182] = "Zombiji: ";
                    strings[183] = "Napajanje nije aktivirano";
                    strings[184] = "Glavni talas preživljen!\n^320 Druga Intermission";
                    strings[185] = "Završen glavni talas!";
                    strings[186] = "Crawler Wave Survived!\n^320 Druga Intermission";
                    strings[187] = "Nagrada maksimalna municija!";
                    strings[188] = "Talas ";
                    strings[189] = " Preživeli!\n^320 Druga Intermission";
                    strings[190] = "Napajanje aktivirao";
                    strings[191] = "Napajanje je privremeno aktivirano na {0} sekundi";
                    strings[192] = "Napajanje nije aktivirano";
                    strings[193] = "Zombiji su pojeli ljude";
                    strings[194] = "Pobeda!";
                    strings[195] = "Poraz!";
                    strings[196] = "Pobeda";
                    strings[197] = "Poraz";
                    strings[198] = "Ljudi su preživeli za";
                    strings[199] = " Minuti";
                    strings[200] = " Sekunde";
                    strings[201] = "Talas preživljen: ";
                    strings[202] = "Dobili smo ovu bitku!";
                    strings[203] = "Zombiji su dobili ovu bitku";
                    strings[204] = "Mašina Smrti!";
                    strings[205] = "Mrtav na mestu!";
                    strings[206] = "Dupli Poeni!";
                    strings[207] = "Maksimalna Municija!";
                    strings[208] = "Nuklearka!";
                    strings[209] = "Prodaja Vatre!";
                    strings[210] = "Zamrzivaè!";
                    strings[211] = "^5Nasumièni";
                    strings[212] = "Glasaj koristeæi [{+actionslot 4}], [{+actionslot 5}], and [{+actionslot 6}] za odgovarajuæe mape!";
                    strings[213] = "Glasaj za drugu mapu!";
                    strings[214] = "Sledeæa runda za: {0}";
                    strings[215] = "Karta ubistava";
                    strings[216] = "1000 Kill Streak!";
                    strings[217] = "Permanent Bot Achieved!";
                    strings[218] = "^3{0} ^7spremno za upotrebu!";
                    strings[219] = "Previše je instaliranih botova.";
                    strings[220] = "Vazdušni prostor je pretrpan.";
                    strings[221] = "Ne možete pozvati Heli Sniper dok se teleportujete";
                    strings[222] = "Ne možete pozvati Heli Sniper ovde";
                    strings[223] = " nije dostupno.";
                    strings[224] = "Nuklearka je veæ isporuèena!";
                    strings[225] = "Nuklearka stiže za: {0}";
                    strings[226] = "Pod tim";
                    strings[227] = "LMG tim";
                    strings[228] = "Pritisni ^3[{vote no}] ^7da preusmeriš dron";
                    strings[229] = "M.O.A.B.";
                    strings[230] = "Tank Barrage";
                    strings[231] = "Defcon Trigger System";
                    strings[232] = "A-10 podrška";
                    strings[233] = "Bacaè plamena";
                    strings[234] = "Bujuca";
                    strings[235] = "Hamer u voznom stanju";
                    strings[236] = "Defcon je na nivou {0}";
                    strings[237] = "Taèka nije imala vidljivih veza! Brisanje taèke...";
                    strings[238] = " Nagrada $500 za {0}!";
                    strings[239] = " Primio $500 od {0}!";
                    strings[240] = "^1Nemate dovoljno novca za nasumièno oružje. Treba ^2$10";
                    strings[241] = "^1Nemate dovoljno novca za nasumièno oružje. Treba ^2$950";
                    strings[242] = "Nasumièno Oružje!";
                    strings[243] = "Oružje nadograðeno!";
                    strings[244] = "^1Kockar je veæ u upotrebi!";
                    strings[245] = "^1Kockara možeš koristiti samo jednom po rundi!";
                    strings[246] = "Kockar!";
                    strings[247] = "Kockar u Upotrebi";
                    strings[248] = "^2Tvoj rezultat æe se prikazati za 10 sekundi.";
                    strings[249] = "^2Pobedili ste {0}!";
                    strings[250] = "^1Izgubili ste {0}!";
                    strings[251] = "^2Duplo zdravlje na 30 sekundi!";
                    strings[252] = "Duplo zdravlje je završeno!";
                    strings[253] = "^2Beskonaèno zdravlje na 30 sekundi!";
                    strings[254] = "Beskonaèno zdravlje je gotovo!";
                    strings[255] = "^2Imaš 1/2 šanse za maksimalnom municijom!";
                    strings[256] = "^2Osvojili ste maksimalnu municiju!";
                    strings[257] = "^1Nema maksimalne municije.";
                    strings[258] = "^1Bog æe odluèiti da li živiš ili umireš za 5 sekundi";
                    strings[259] = "^2Živiš.";
                    strings[260] = "^1Nemate dovoljno novca za municiju. Potrebno ^2$";
                    strings[261] = "Municija!";
                    strings[262] = "Random Killstreak!";
                    strings[263] = "Trenutni balans: ${0}";
                    strings[264] = "Bank Withdraw";
                    strings[265] = "Lift!";
                    strings[266] = "Juggernog";
                    strings[267] = "Stamin-Up";
                    strings[268] = "Speed Cola";
                    strings[269] = "Mule Kick";
                    strings[270] = "Double Tap";
                    strings[271] = "Stalker Soda";
                    strings[272] = "Quick Revive Pro";
                    strings[273] = "Scavenge-Aid";
                    strings[274] = "Napajanje!";
                    strings[275] = "Napajanje je aktivirano";
                    strings[276] = "Otvorena vrata!";
                    strings[277] = "Pritisni ^3[{+activate}] ^7na poklon $^2500 ^7to ";
                    strings[278] = "Pritisni ^3{1} ^7da otvoriš vrata [Cena: {0}]";
                    strings[279] = "Pritisni ^3[{+activate}] ^7da trguješ oružjem: ";
                    strings[280] = "Pritisni ^3[{+activate}] ^7za nasumièno oružje [Cena: 10]";
                    strings[281] = "Pritisni ^3[{+activate}] ^7za nasumièno oružje [Cena: 950]";
                    strings[282] = "^1Napajanje mora biti aktivirano!";
                    strings[283] = "Pritisni ^3[{+activate}] ^7da uzmeš ^2svoje novo nadograðeno oružje";
                    strings[284] = "Pritisni ^3[{+activate}] ^7da nadogradiš svoje ^1trenutno oružje ^7[Cena: 5000]";
                    strings[285] = "Pritisni ^3[{+activate}] ^7da koristiš kockara [Cena: 1000]";
                    strings[286] = "Pritisni ^3[{+activate}] ^7da kupiš nasumièni killstreak [Cena: 200 ^5Bonus Poeni^7]";
                    strings[287] = "Teleporter je na hlaðenju.";
                    strings[288] = "^1Prvo morate da povežete teleporter!";
                    strings[289] = "Pritisni ^3[{+activate}] ^7da se teleportujete";
                    strings[290] = "Pritisni ^3[{+activate}] ^7da povežete teleporter";
                    strings[291] = "Pritisni ^3[{+activate}] ^7da koristite lift [Cena: 500]";
                    strings[292] = "Pritisni ^3[{+activate}] ^7da povuèete sa ATM [Iznos: 1000] [Cena: 100]\n\n                  Pritisni ^3[{vote yes}] ^7da ostaviš depozit ATM [Cena: 1000]";
                    strings[293] = "You already have {0}!";
                    strings[294] = "Pritisni ^3{2} ^7da kupiš {0} [Cena: {1}]";
                    strings[295] = "Možete nositi samo {0} pogodnost!";
                    strings[296] = "Pritisni ^3[{+activate}] ^7da aktivirate Napajanje [Cena: 10000]";
                    strings[297] = "Drži ^3[{+aktiviraj}] ^7za eksplozivnu municiju";
                    strings[298] = "Drži ^3[{+aktiviraj}] ^7da se ukrcate na helihopter";
                    strings[299] = " veæ se oživljava!";
                    strings[300] = "Drži ^3[{+aktiviraj}] ^7da oživiš ";
                    strings[301] = "Municija";
                    strings[302] = "Raketa";
                    strings[303] = "Sentry Gun";
                    strings[304] = "Vision Restorer";
                    strings[305] = "Power Surge";
                    strings[306] = "Deployable Explosive Ammo";
                    strings[307] = "M.O.A.B.";
                    strings[308] = "Dron mala ptica";
                    strings[309] = "Helihopterski snajper";
                    strings[310] = "Lanser granata";
                    strings[311] = "Dupli poeni";
                    strings[312] = "Mrtav na mestu";
                    strings[313] = "Nuklearka";
                    strings[314] = "Mašina Smrti";
                    strings[315] = "Vazdušni napad";
                    strings[316] = "Drži ^3[{+activate}] ^7za ";
                    strings[317] = "Oružje";
                    strings[318] = "Pritisni ^3[{+activate}] ^7da opereš svoje grehe.";
                    strings[319] = "Pritisni ^3{2} ^7za {0} ^7[trošak: {1}]";
                    strings[320] = "Pritisni ^3[{+activate}] ^7za ";
                    strings[321] = "Pritisni ^3{1} ^7za municiju [trošak: {0}]";
                    strings[322] = "Pritisni ^3[{+actionslot 3}] ^7za otpremanje P.E.S.";
                    strings[323] = "^5P.E.S. Aktiviran.";
                    strings[324] = "^5Molim vas aktivirajte P.E.S. ([{+actionslot 3}])";
                    strings[325] = "greška pri uèitavanju mapedit za mapu {0}: {1}";
                    strings[326] = "Nepoznata stavka MapEdit Entry {0}... ignorisanje";
                    strings[327] = "Nagraðeni ste sa svim pogodnostima!";
                    strings[328] = "Vaš je bankovni saldo već na maksimumu!";
                    strings[329] = "Dolazi kisela kiša!";
                    strings[330] = "Bodljikava žica";
                    strings[331] = "Izlijevanje nafte";
                    strings[332] = "Napad otrovnim plinom";
                    strings[333] = "Topništvo";
                    strings[334] = "Mećava";
                    strings[335] = "Vulkanska erupcija";
                    strings[336] = "Jurišne čamce";
                    strings[337] = "Dolazi otrovni plin!";
                    strings[338] = "Blizzard Inbound!";
                    strings[339] = "Erupcija vulkana neminovna!";
                    strings[340] = "Vulkan je već eruptirao.";
                    break;
                #endregion
                #region spanish
                case "spanish":
                    strings[0] = "Los humanos derrotaron a los zombies!";
                    strings[1] = "¡Los humanos sobrevivieron!";
                    strings[2] = "Buen trabajo humanos!";
                    strings[3] = "¡Los humanos se mantuvieron vivos!";
                    strings[4] = "Cara humana: :D!";
                    strings[5] = "¡Increíble! ¡Los seres humanos viven!";
                    strings[6] = "¡Grandes humanos de trabajo!";
                    strings[7] = "Buen trabajo, prepárate para el próximo ataque!";
                    strings[8] = "Los zombis son tales pervertidos ... ¡Humanos FTW!";
                    strings[9] = "Humanos: 1, Zombies: 0";
                    strings[10] = "¡Los humanos ganan perras!";
                    strings[11] = "¡¡¡Victoria!!!";
                    strings[12] = "Enemigo abajo !!!";
                    strings[13] = "¡Pan comido!";
                    strings[14] = "¡Debes estar ejecutando AIZombies Supreme en Team Deathmatch!";
                    strings[15] = "Los jugadores máximos actuales para AIZombies solo pueden ser 8 o menos. El ajuste actual es {0}. Se ha establecido en 8.";
                    strings[16] = "^1AIZombies Supreme Made by ^2Slvr99";
                    strings[17] = "^2humanos";
                    strings[18] = "^1¡Espera hasta el final de la ronda para desovar!";
                    strings[19] = "Sobrevivir a las olas {0}.";
                    strings[20] = "Prepárate para el ataque en:";
                    strings[21] = "Siguiente ronda en: ";
                    strings[22] = "^2Bienvenido {0}! \n^1AIZombies Supreme {3} \n^3Mapa: {1} \n^2Hecho por Slvr99 \n^5Survive {2} Ondas.";
                    strings[23] = "^1{0} ^1necesita ser revivido!";
                    strings[24] = "^1Falló para revivir";
                    strings[25] = "^1Has muerto. Espere hasta la próxima ronda para reaparecer.";
                    strings[26] = "^1{0} ^1ha sido asesinado.";
                    strings[27] = "Archivo de configuración para AIZombies no fue encontrado! Creando uno ...";
                    strings[28] = "¡El estado máximo de salud se estableció en un valor incorrecto en el cfg!";
                    strings[29] = "Max Juggernog Health se configuró en un valor incorrecto en el cfg !, Establecer como predeterminado ({0})";
                    strings[30] = "Bot Health se configuró en un valor incorrecto en el cfg !, Establecer como predeterminado ({0})";
                    strings[31] = "El estado del rastreador se estableció en un valor incorrecto en el cfg !, se estableció en predeterminado ({0})";
                    strings[32] = "Boss Health se estableció en un valor incorrecto en el cfg !, Establecer en predeterminado ({0})";
                    strings[33] = "El Factor de salud del bot se estableció en un valor incorrecto en el cfg !, Establecer como predeterminado ({0})";
                    strings[34] = "Bot Damage se estableció en un valor incorrecto en el cfg !, Establecer como predeterminado ({0})";
                    strings[35] = "Perk Limit se estableció en un valor incorrecto en el cfg !, Establecer como predeterminado ({0})";
                    strings[36] = "Calles de la muerte";
                    strings[37] = "Brote tormentoso";
                    strings[38] = "Pueblo en ruinas";
                    strings[39] = "Refinería de petróleo";
                    strings[40] = "Puesto avanzado del desierto";
                    strings[41] = "Metro abandonado";
                    strings[42] = "Sitio de construcción del infierno";
                    strings[43] = "Paso subterráneo demolido";
                    strings[44] = "Camino abandonado del infierno";
                    strings[45] = "Muerte en muelle";
                    strings[46] = "Callejón de la muerte";
                    strings[47] = "Buen provecho, que aproveche";
                    strings[48] = "Unidad de almacenamiento del infierno";
                    strings[49] = "Hotel de playa del infierno";
                    strings[50] = "Mucho dolor de coche";
                    strings[51] = "Gran muerte negra";
                    strings[52] = "El brote";
                    strings[53] = "Wartorn";
                    strings[54] = "En construcción";
                    strings[55] = "Muerte en progreso";
                    strings[56] = "Naufragio";
                    strings[57] = "Acueducto muerto";
                    strings[58] = "Silla de cemento";
                    strings[59] = "Ataque de oceanside";
                    strings[60] = "Santuario del infierno";
                    strings[61] = "Paraíso";
                    strings[62] = "Sitio de descanso del infierno";
                    strings[63] = "Corredor de la muerte";
                    strings[64] = "Puente pandemico";
                    strings[65] = "Granja de no muertos";
                    strings[66] = "Escondite del infierno";
                    strings[67] = "Chabolas";
                    strings[68] = "Plataforma petrolera";
                    strings[69] = "Avenida de la muerte";
                    strings[70] = "Cañon atrapado";
                    strings[71] = "El muelle";
                    strings[72] = "^1Mapa desconocido!";
                    strings[73] = "Por favor, no se haga pasar por el desarrollador.";
                    strings[74] = "La búsqueda del nombre del tipo de juego del servidor fue cancelada por una razón desconocida.";
                    strings[75] = "¡Hubo un error al configurar el nombre de gametype del servidor !:";
                    strings[76] = "Se produjo un error al configurar el nombre del tipo de juego del servidor: ¡No se encontraron direcciones!";
                    strings[77] = "La búsqueda de nombres de tipo de juego fue cancelada por una razón desconocida.";
                    strings[78] = "¡Hubo un error al establecer el nombre del tipo de juego !:";
                    strings[79] = "Se produjo un error al configurar el nombre del tipo de juego: ¡No se encontraron direcciones!";
                    strings[80] = "Presiona ^3[{+activate}] ^7para recoger la pistola centinela";
                    strings[81] = "";
                    strings[82] = "La búsqueda de parche de arma fue cancelada por una razón desconocida! Esto puede causar errores para ciertas armas.";
                    strings[83] = "Hubo un error al encontrar ubicaciones de parches de armas:";
                    strings[84] = "No se puede establecer un nombre de gametype personalizado en el navegador del servidor!";
                    strings[85] = "¡No se pudieron encontrar datos de armas para {0}! Por favor, informe este error a Slvr99";
                    strings[86] = "Boardwalk";
                    strings[87] = "Una feria playera interrumpida por los muertos vivientes";
                    strings[88] = "No se puede establecer un nombre de gametype personalizado.";
                    strings[89] = "No se puede restaurar la cadena de modo de juego personalizado! ¡Esto puede resultar en una falla del servidor cuando el mapa cambia!";
                    strings[90] = "Todos los scripts de terceros se han descargado de este servidor debido a un conflicto con AIZombies.";
                    strings[91] = "¡Hubo un error al contactar con el servidor de verificación de actualizaciones !: {0}";
                    strings[92] = "¡Hubo un error al contactar con el servidor de verificación de actualizaciones !: No hay respuesta del servidor.";
                    strings[93] = "¡Hay una actualización para AIZombies disponible! Descargando la versión {0} ahora ...";
                    strings[94] = "¡Hubo un error al descargar la actualización desde el servidor !: {0}";
                    strings[95] = "¡Hubo un error al reemplazar el viejo archivo AIZombies! Asegúrese de que el archivo no sea de solo lectura o que esté abierto en otro lugar.";
                    strings[96] = "¡Descarga completa! Las actualizaciones entrarán en vigor una vez que finalice el juego actual.";
                    strings[97] = "¡Dinero extra!";
                    strings[98] = "Puntos extra de bonificación!";
                    strings[99] = "Perk aleatorio!";
                    strings[100] = "No hay bot engendro disponible! Tenga al menos un \"zombiespawn \" en su archivo de mapa.";
                    strings[101] = "^1No bot engendra disponible! Compruebe la consola para más detalles";
                    strings[102] = "Mas salud";
                    strings[103] = "Sprint más rápido y más largo";
                    strings[104] = "Recargar más rápido";
                    strings[105] = "Una ranura de arma extra";
                    strings[106] = "Velocidad de fuego más rápida";
                    strings[107] = "Moviéndose más rápido mientras ADS";
                    strings[108] = "Revive automáticamente poco después de bajar \n+ Revive su equipo más rápido";
                    strings[109] = "Recolectar municiones gratis";
                    strings[110] = "Lockdown";
                    strings[111] = "Bootleg";
                    strings[112] = "Mission";
                    strings[113] = "Carbon";
                    strings[114] = "Dome";
                    strings[115] = "Downturn";
                    strings[116] = "Hardhat";
                    strings[117] = "Interchange";
                    strings[118] = "Fallen";
                    strings[119] = "Bakaara";
                    strings[120] = "Resistance";
                    strings[121] = "Arkaden";
                    strings[122] = "Outpost";
                    strings[123] = "Seatown";
                    strings[124] = "Underground";
                    strings[125] = "Village";
                    strings[126] = "Piazza";
                    strings[127] = "Liberation";
                    strings[128] = "Black Box";
                    strings[129] = "Overwatch";
                    strings[130] = "Aground";
                    strings[131] = "Erosion";
                    strings[132] = "Foundation";
                    strings[133] = "Getaway";
                    strings[134] = "Sanctuary";
                    strings[135] = "Oasis";
                    strings[136] = "Lookout";
                    strings[137] = "Terminal";
                    strings[138] = "Intersection";
                    strings[139] = "Vortex";
                    strings[140] = "U-Turn";
                    strings[141] = "Decommission";
                    strings[142] = "Offshore";
                    strings[143] = "Parish";
                    strings[144] = "Gulch";
                    strings[145] = "Una vez calles pacíficas invadidas por no muertos";
                    strings[146] = "Ciudad tormentosa tomada por los muertos vivientes";
                    strings[147] = "Un pueblo en ruinas tomado por los no muertos";
                    strings[148] = "Una refinería de petróleo una vez animada.";
                    strings[149] = "Un puesto de avanzada abandonado en el desierto.";
                    strings[150] = "Las consecuencias de un error peligroso.";
                    strings[151] = "Una pequeña obra en construcción sin terminar.";
                    strings[152] = "Una autopista destruida \nresultada de un brote";
                    strings[153] = "Un pueblo fantasma ruso abandonado";
                    strings[154] = "Una ciudad africana golpeada por los no muertos.";
                    strings[155] = "El distrito parisino ya no es seguro";
                    strings[156] = "Centro comercial alemán robado por los muertos vivientes.";
                    strings[157] = "Puesto avanzado siberiano evacuado \ndebido al brote";
                    strings[158] = "Una ciudad costera no es segura \nde los no muertos";
                    strings[159] = "Estación de metro utilizada como ruta de nevacuación.";
                    strings[160] = "Pueblo africano montado \ncon una enfermedad peligrosa";
                    strings[161] = "Un pueblo español infectado por muertos.";
                    strings[162] = "Una base militar construida en un parque para la protección contra los no muertos.";
                    strings[163] = "Sitio de colisión de un ataque aéreo \nvirus";
                    strings[164] = "Un rascacielos abandonado debido al brote.";
                    strings[165] = "Naufragio causado por un virus en el mar";
                    strings[166] = "Acueducto contaminado por los muertos vivientes.";
                    strings[167] = "Fábrica de cemento superada por los muertos vivientes.";
                    strings[168] = "Lugar de vacaciones que se convirtió en una pesadilla.";
                    strings[169] = "Un santuario que fue roto de la pureza de \nit por los no muertos.";
                    strings[170] = "Un oasis que fue invadido por los muertos vivientes.";
                    strings[171] = "Un repositorio de reposo que es útil para todo lo que no sea reposo.";
                    strings[172] = "Un aeropuerto ruso superado por los muertos vivientes.";
                    strings[173] = "Una autopista en el centro de los ataques.";
                    strings[174] = "Granja abandonada golpeada por \nmás de un problema";
                    strings[175] = "Autopista del desierto insegura de los muertos vivientes";
                    strings[176] = "Un naufragio cerca de una ciudad que ha sido infectado";
                    strings[177] = "Una plataforma petrolera invadida por \nmás que juggernauts";
                    strings[178] = "Un pueblo divertido que fue \novertaken por no-muerto";
                    strings[179] = "Quebrada del desierto prospera con muertos vivientes";
                    strings[180] = "Dinero: $";
                    strings[181] = "Puntos extra:";
                    strings[182] = "Zombies: ";
                    strings[183] = "El poder no esta activado";
                    strings[184] = "¡Boss Wave sobrevivió! \n^320 segundo intervalo";
                    strings[185] = "Ola de jefe completada!";
                    strings[186] = "¡La oruga de oruga sobrevivió! \n^320 segundo intervalo";
                    strings[187] = "¡Máxima munición otorgada!";
                    strings[188] = "Ola ";
                    strings[189] = " Sobrevivido! \n^320 Segundo Intermedio";
                    strings[190] = "Potencia activada por";
                    strings[191] = "El poder ha sido activado temporalmente por {0} segundos";
                    strings[192] = "El poder no esta activado";
                    strings[193] = "Los zombis se comieron a los humanos.";
                    strings[194] = "¡Victoria!";
                    strings[195] = "¡Derrota!";
                    strings[196] = "Ganar";
                    strings[197] = "Perder";
                    strings[198] = "Los humanos sobrevivieron para";
                    strings[199] = "Minutos";
                    strings[200] = "Segundos";
                    strings[201] = "Ondas sobrevividas:";
                    strings[202] = "¡Ganamos esta lucha!";
                    strings[203] = "Zombies ganaron esta lucha";
                    strings[204] = "¡Maquina de la muerte!";
                    strings[205] = "Insta-Kill!";
                    strings[206] = "Puntos dobles!";
                    strings[207] = "¡Municion maxima!";
                    strings[208] = "Nuke!";
                    strings[209] = "¡Venta de liquidacion!";
                    strings[210] = "¡Congelador!";
                    strings[211] = "^5Aleatorio";
                    strings[212] = "¡Vote usando [{+actionslot 4}], [{+actionslot 5}], y [{+actionslot 6}] para los mapas respectivos!";
                    strings[213] = "¡Vota por el siguiente mapa!";
                    strings[214] = "Próxima ronda en: {0}";
                    strings[215] = "Mapa Killstreak";
                    strings[216] = "1000 Racha de muertes!";
                    strings[217] = "Bot permanente logrado!";
                    strings[218] = "^3{0} ^7listo para usar!";
                    strings[219] = "Demasiados bots desplegados.";
                    strings[220] = "El espacio aéreo está demasiado lleno.";
                    strings[221] = "No se puede llamar a Heli Sniper mientras se teletransporta";
                    strings[222] = "No se puede llamar a Heli Sniper aquí";
                    strings[223] = "no disponible.";
                    strings[224] = "¡Nuke ya está entrante!";
                    strings[225] = "Nuke Incoming In: {0}";
                    strings[226] = "División del equipo";
                    strings[227] = "Equipo LMG";
                    strings[228] = "Presione ^3[{vote no}] ^7para reencaminar el drone";
                    strings[229] = "M.O.A.B.";
                    strings[230] = "Presa de tanque";
                    strings[231] = "Sistema de Activación Defcon";
                    strings[232] = "Soporte A-10";
                    strings[233] = "Echador de llama";
                    strings[234] = "Inundación repentina";
                    strings[235] = "Humvee manejable";
                    strings[236] = "Defcon está en el nivel {0}";
                    strings[237] = "¡Un waypoint no tenía enlaces visibles! Eliminando waypoint ...";
                    strings[238] = "¡Dotado $ 500 a {0}!";
                    strings[239] = "¡Recibió $ 500 de {0}!";
                    strings[240] = "^1No hay suficiente dinero para un Arma aleatoria. Necesita ^2$10";
                    strings[241] = "^1No hay suficiente dinero para un Arma aleatoria. Necesita ^2$950";
                    strings[242] = "Arma aleatoria!";
                    strings[243] = "Arma mejorada!";
                    strings[244] = "^1¡El jugador ya está en uso!";
                    strings[245] = "^1¡Sólo puedes usar el jugador una vez por ronda!";
                    strings[246] = "¡Jugador!";
                    strings[247] = "^1Has ganado {0}.";
                    strings[248] = "^2Sus resultados se mostrarán en 10 segundos.";
                    strings[249] = "^2¡Has ganado {0}!";
                    strings[250] = "^1Has perdido {0}!";
                    strings[251] = "^¡Disminuye la salud durante 30 segundos!";
                    strings[252] = "¡Se acabó la doble salud!";
                    strings[253] = "^2¡Salud infinita durante 30 segundos!";
                    strings[254] = "Salud infinita más!";
                    strings[255] = "^2¡Tienes 1/2 oportunidad de munición máxima!";
                    strings[256] = "^2¡Has ganado el Max Ammo!";
                    strings[257] = "^1No Max Munición.";
                    strings[258] = "^1Dios decide si vives o mueres en 5 segundos.";
                    strings[259] = "^2Vives.";
                    strings[260] = "^1No hay suficiente dinero para Munición. Necesita ^2$";
                    strings[261] = "Munición";
                    strings[262] = "Killstreak aleatorio!";
                    strings[263] = "Saldo actual: ${0}";
                    strings[264] = "Retiro de banco";
                    strings[265] = "¡Ascensor!";
                    strings[266] = "Juggernog";
                    strings[267] = "Stamin-Up";
                    strings[268] = "Speed ​​Cola";
                    strings[269] = "Patada de mula";
                    strings[270] = "Doble toque";
                    strings[271] = "Stalker Soda";
                    strings[272] = "Quick Revive Pro";
                    strings[273] = "Ayuda a la basura";
                    strings[274] = "¡Poder!";
                    strings[275] = "El poder ha sido activado";
                    strings[276] = "Puerta abierta!";
                    strings[277] = "Presione ^3[{+activate}] ^7para donar $^2500 ^7para ";
                    strings[278] = "Presione ^3{1} ^7para abrir la puerta [Costo: {0}]";
                    strings[279] = "Presiona ^3[{+activate}] ^7para intercambiar Armas: ";
                    strings[280] = "Presione ^3[{+activate}] ^7para un Arma aleatoria [Costo: 10]";
                    strings[281] = "Presione ^3[{+activate}] ^7para un Arma aleatoria [Costo: 950]";
                    strings[282] = "^1El poder debe estar activado!";
                    strings[283] = "Presiona ^3[{+activate}] ^7para tomar ^2tu nueva arma mejorada";
                    strings[284] = "Presiona ^3[{+activate}] ^7para actualizar tu ^1Arma actual ^7[Costo: 5000]";
                    strings[285] = "Presione ^3[{+activate}] ^7para usar el jugador [Costo: 1000]";
                    strings[286] = "Presione ^3[{+activate}] ^7para comprar un killstreak aleatorio [Costo: 200 ^5Puntos de bonificación ^7]";
                    strings[287] = "El teletransportador se está enfriando.";
                    strings[288] = "^1¡Debes enlazar el teletransportador primero!";
                    strings[289] = "Presione ^3[{+activate}] ^7para teletransportarse";
                    strings[290] = "Presione ^3[{+activate}] ^7para vincular el teletransportador";
                    strings[291] = "Presione ^3[{+activate}] ^7para usar el ascensor [Costo: 500]";
                    strings[292] = "Presione ^3[{+activate}] ^7para retirarse del cajero automático [Cantidad: 1000] [Costo: 100] \n\nPresione ^3[{vote yes}] ^7para depositar en el cajero automático [Costo: 1000]";
                    strings[293] = "¡Ya tienes {0}!";
                    strings[294] = "Presione ^3{2} ^7para comprar {0} [Costo: {1}]";
                    strings[295] = "¡Solo puedes llevar {0} beneficios!";
                    strings[296] = "Presione ^3[{+activate}] ^7para activar Energía [Costo: 10000]";
                    strings[297] = "Sostenga ^3[{+activate}] ^7para municiones explosivas";
                    strings[298] = "Mantenga presionado ^3[{+activate}] ^7para subir al Heli";
                    strings[299] = "ya está siendo revivido!";
                    strings[300] = "Mantenga presionado ^3[{+activate}] ^7para revivir";
                    strings[301] = "Munición";
                    strings[302] = "Misil";
                    strings[303] = "Arma de centinela";
                    strings[304] = "Restaurador de la visión";
                    strings[305] = "Sobrecarga de energía";
                    strings[306] = "Munición explosiva desplegable";
                    strings[307] = "M.O.A.B.";
                    strings[308] = "Abejita";
                    strings[309] = "Heli Sniper";
                    strings[310] = "Torreta Lanzagranadas";
                    strings[311] = "Puntos dobles";
                    strings[312] = "Insta-Kill";
                    strings[313] = "Nuke";
                    strings[314] = "Maquina de la muerte";
                    strings[315] = "Ataque aéreo";
                    strings[316] = "Mantenga presionado ^3[{+activate}] ^7para ";
                    strings[317] = "Arma";
                    strings[318] = "Presiona ^3[{+activate}] ^7para lavar tus pecados.";
                    strings[319] = "Presione ^3{2} ^7para {0} ^7[Costo: {1}]";
                    strings[320] = "Presione ^3[{+activate}] ^7para ";
                    strings[321] = "Presione ^3{1} ^7para Munición [Costo: {0}]";
                    strings[322] = "Presione ^3[{+actionslot 3}] ^7para equipar a P.E.S.";
                    strings[323] = "^5P.E.S. Activo.";
                    strings[324] = "^5Por favor, active P.E.S. ([{+actionslot 3}])";
                    strings[325] = "Error al cargar mapedit para el mapa {0}: {1}";
                    strings[326] = "Entrada de MapEdit Desconocida {0} ... ignorando";
                    strings[327] = "Todos los beneficios concedidos!";
                    strings[328] = "¡Tu saldo bancario ya está al máximo!";
                    strings[329] = "¡Lluvia ácida entrante!";
                    strings[330] = "Alambre de espino";
                    strings[331] = "Derrame de petróleo";
                    strings[332] = "Ataque de gas venenoso";
                    strings[333] = "Artillería";
                    strings[334] = "Tormenta de nieve";
                    strings[335] = "Erupción volcánica";
                    strings[336] = "Barcos de asalto";
                    strings[337] = "¡Gas venenoso entrante!";
                    strings[338] = "¡Blizzard Inbound!";
                    strings[339] = "¡Erupción volcánica inminente!";
                    strings[340] = "El volcán ya ha entrado en erupción.";
                    break;
                #endregion
                #region french
                case "french":
                    strings[0] = "Les humains ont vaincu les zombies!";
                    strings[1] = "Les humains ont survécu!";
                    strings[2] = "Bon travail aux humains!";
                    strings[3] = "Les humains sont restés vivants!";
                    strings[4] = "Visage humain: :D!";
                    strings[5] = "Incroyable! Les humains vivent!";
                    strings[6] = "Bravo les humains!";
                    strings[7] = "Bon travail, préparez-vous pour la prochaine attaque!";
                    strings[8] = "Les zombies sont de tels pervers ... Les humains FTW!";
                    strings[9] = "Humains: 1, Zombies: 0";
                    strings[10] = "Les humains gagnent des chiennes!";
                    strings[11] = "La victoire!!!";
                    strings[12] = "Ennemi bas !!!";
                    strings[13] = "Peasy facile!";
                    strings[14] = "Vous devez exécuter AIZombies Supreme dans le Match à mort par équipe!";
                    strings[15] = "Le nombre maximum de joueurs pour AIZombies ne peut être que 8 ou moins. Le paramètre actuel est {0}. Il a été réglé sur 8.";
                    strings[16] = "^1AIZombies Supreme Fabriqué par ^2Slvr99";
                    strings[17] = "^2Humains";
                    strings[18] = "^1Attendez la fin du tour pour frayer!";
                    strings[19] = "Survivre {0} vagues.";
                    strings[20] = "Préparez-vous pour l'attaque dans:";
                    strings[21] = "Prochain tour dans: ";
                    strings[22] = "^2Bienvenue {0}! \n^1AIZombies Supreme {3} \n^3Carte: {1} \n^2Fabriquée par Slvr99 \n^5Survive {2}.";
                    strings[23] = "^1{0} ^1Les besoins doivent être relancés!";
                    strings[24] = "^1Ne pas réussi à faire revivre";
                    strings[25] = "^1Vous êtes mort. Attendez le prochain tour pour réapparaître.";
                    strings[26] = "^1{0} ^1a été tué.";
                    strings[27] = "Le fichier de configuration pour AIZombies n'a pas été trouvé! En créer un ...";
                    strings[28] = "Max Health a été défini sur une valeur incorrecte dans cfg !, Paramétrez sur la valeur par défaut ({0}).";
                    strings[29] = "Max Juggernog Health a été défini sur une valeur incorrecte dans cfg !, Définir sur la valeur par défaut ({0}).";
                    strings[30] = "La santé du bot a été définie sur une valeur incorrecte dans cfg !, Définir à la valeur par défaut ({0}).";
                    strings[31] = "Santé du robot d'exploration a été définie sur une valeur incorrecte dans cfg !, Paramétrer sur la valeur par défaut ({0}).";
                    strings[32] = "Boss Health a été défini sur une valeur incorrecte dans cfg !, Paramétrez sur par défaut ({0})";
                    strings[33] = "Bot Health Factor a été défini sur une valeur incorrecte dans cfg !, Paramétrer sur la valeur par défaut ({0}).";
                    strings[34] = "Bot Damage a été défini sur une valeur incorrecte dans cfg !, Paramétrer sur la valeur par défaut ({0}).";
                    strings[35] = "Perk Limit a été défini sur une valeur incorrecte dans cfg !, définir sur par défaut ({0}).";
                    strings[36] = "Rues de la mort";
                    strings[37] = "Éclosion orageuse";
                    strings[38] = "Village délabré";
                    strings[39] = "Raffinerie de pétrole";
                    strings[40] = "Avant-poste du désert";
                    strings[41] = "Métro abandonné";
                    strings[42] = "Chantier de construction de l'enfer";
                    strings[43] = "Passage inférieur démoli";
                    strings[44] = "Route abandonnée de l'enfer";
                    strings[45] = "Mort amarrée";
                    strings[46] = "Allée de la mort";
                    strings[47] = "Appétit osseux";
                    strings[48] = "Unité de stockage de l'enfer";
                    strings[49] = "Hôtel balnéaire de l'enfer";
                    strings[50] = "Voiture beaucoup de douleur";
                    strings[51] = "Big Black Death";
                    strings[52] = "El Brote";
                    strings[53] = "Déchiré par la guerre";
                    strings[54] = "En construction";
                    strings[55] = "Mort en cours";
                    strings[56] = "Naufrage";
                    strings[57] = "Aqueduc mort";
                    strings[58] = "Silla Cement";
                    strings[59] = "Oceanside Attack";
                    strings[60] = "Sanctuaire de l'enfer";
                    strings[61] = "paradis";
                    strings[62] = "Site de repos de l'enfer";
                    strings[63] = "Couloir de la mort";
                    strings[64] = "Pont pandémique";
                    strings[65] = "Undead Farm";
                    strings[66] = "Cachette de l'enfer";
                    strings[67] = "Bidonville";
                    strings[68] = "Plate-forme pétrolière";
                    strings[69] = "Death Avenue";
                    strings[70] = "Canyon piégé";
                    strings[71] = "La jetée";
                    strings[72] = "^1Carte inconnue!";
                    strings[73] = " S'il vous plaît ne pas usurper l'identité du développeur.";
                    strings[74] = "La recherche du nom du type de jeu du serveur a été annulée pour une raison inconnue.";
                    strings[75] = "Une erreur s'est produite lors de la définition du nom du type de jeu du serveur !:";
                    strings[76] = "Une erreur s'est produite lors de la définition du nom du type de jeu du serveur: aucune adresse trouvée!";
                    strings[77] = "La recherche du nom du type de jeu a été annulée pour une raison inconnue.";
                    strings[78] = "Une erreur s'est produite lors de la définition du nom du type de jeu!:";
                    strings[79] = "Une erreur s'est produite lors de la définition du nom du type de jeu: Aucune adresse trouvée!";
                    strings[80] = "Appuyez sur ^3[{+activate}] ^7pour récupérer la mitrailleuse.";
                    strings[81] = "";
                    strings[82] = "La recherche de patch d'armes a été annulée pour une raison inconnue! Cela peut causer des bugs pour certaines armes.";
                    strings[83] = "Une erreur s'est produite lors de la recherche de l'emplacement des correctifs d'arme:";
                    strings[84] = "Impossible de définir un nom de type de jeu personnalisé dans le navigateur du serveur!";
                    strings[85] = "Impossible de trouver les données d'arme pour {0}! Veuillez signaler cette erreur à Slvr99";
                    strings[86] = "Boardwalk";
                    strings[87] = "Une foire en bord de mer interrompue par les morts-vivants";
                    strings[88] = "Impossible de définir un nom de type de jeu personnalisé.";
                    strings[89] = "Impossible de restaurer la chaîne de mode de jeu personnalisée! Cela peut entraîner un crash du serveur lorsque la carte change!";
                    strings[90] = "Tous les scripts tiers ont été déchargés de ce serveur en raison d'un conflit avec AIZombies.";
                    strings[91] = "Une erreur s'est produite lors de la connexion au serveur de vérification de la mise à jour !: {0}";
                    strings[92] = "Une erreur s'est produite lors de la connexion au serveur de vérification de la mise à jour !: Aucune réponse du serveur.";
                    strings[93] = "Il y a une mise à jour pour AIZombies disponible! Téléchargement de la version {0} maintenant ...";
                    strings[94] = "Une erreur s'est produite lors du téléchargement de la mise à jour à partir du serveur !: {0}";
                    strings[95] = "Une erreur s'est produite lors du remplacement de l'ancien fichier AIZombies! Assurez-vous que le fichier n'est pas en lecture seule ou est ouvert ailleurs.";
                    strings[96] = "Téléchargement terminé! Les mises à jour prendront effet à la fin du jeu en cours.";
                    strings[97] = "L'argent supplémentaire!";
                    strings[98] = "Points bonus supplémentaires!";
                    strings[99] = "Perk Aléatoire!";
                    strings[100] = "Pas de robots disponibles! Veuillez avoir au moins un \"zombiespawn \" dans votre fichier de carte.";
                    strings[101] = "^1Il n'y a pas de robots disponibles! Vérifiez la console pour plus de détails";
                    strings[102] = "Plus de santé";
                    strings[103] = "Sprint plus vite et plus longtemps";
                    strings[104] = "Recharger plus vite";
                    strings[105] = "Un emplacement d'arme supplémentaire";
                    strings[106] = "Cadence de tir plus rapide";
                    strings[107] = "Déplacement plus rapide tout en ADS";
                    strings[108] = "Être réactivé automatiquement peu de temps après la fin de la course \n+ réactiver votre équipe plus rapidement";
                    strings[109] = "Récupérer des munitions gratuites";
                    strings[110] = "Lockdown";
                    strings[111] = "Bootleg";
                    strings[112] = "Mission";
                    strings[113] = "Carbon";
                    strings[114] = "Dome";
                    strings[115] = "Downturn";
                    strings[116] = "Hardhat";
                    strings[117] = "Interchange";
                    strings[118] = "Fallen";
                    strings[119] = "Bakaara";
                    strings[120] = "Resistance";
                    strings[121] = "Arkaden";
                    strings[122] = "Outpost";
                    strings[123] = "Seatown";
                    strings[124] = "Underground";
                    strings[125] = "Village";
                    strings[126] = "Piazza";
                    strings[127] = "Liberation";
                    strings[128] = "Black Box";
                    strings[129] = "Overwatch";
                    strings[130] = "Aground";
                    strings[131] = "Erosion";
                    strings[132] = "Foundation";
                    strings[133] = "Getaway";
                    strings[134] = "Sanctuary";
                    strings[135] = "Oasis";
                    strings[136] = "Lookout";
                    strings[137] = "Terminal";
                    strings[138] = "Intersection";
                    strings[139] = "Vortex";
                    strings[140] = "U-Turn";
                    strings[141] = "Decommission";
                    strings[142] = "Offshore";
                    strings[143] = "Parish";
                    strings[144] = "Gulch";
                    strings[145] = "Des rues paisibles envahies par des morts-vivants";
                    strings[146] = "Ville orageuse reprise par les morts-vivants";
                    strings[147] = "Un village délabré repris \npar les morts-vivants";
                    strings[148] = "Une raffinerie de pétrole autrefois vivante";
                    strings[149] = "Un avant-poste abandonné dans le désert";
                    strings[150] = "Les conséquences d'une erreur dangereuse";
                    strings[151] = "Un petit chantier inachevé";
                    strings[152] = "Une autoroute détruite \nrésultant d'une épidémie";
                    strings[153] = "Une ville fantôme russe abandonnée";
                    strings[154] = "Une ville africaine touchée par les morts-vivants";
                    strings[155] = "Quartier parisien n'est plus sûr";
                    strings[156] = "Centre commercial allemand volé par les morts-vivants";
                    strings[157] = "Un avant-poste sibérien évacué du fait de l'épidémie";
                    strings[158] = "Une ville balnéaire pas sans danger \nde les morts-vivants";
                    strings[159] = "Station de métro utilisée comme voie d’évacuation";
                    strings[160] = "Un village africain atteint d'une grave maladie";
                    strings[161] = "Une ville espagnole infectée par le muertos";
                    strings[162] = "Une base militaire construite dans un parc \npour la protection contre les morts-vivants";
                    strings[163] = "Lieu d’accident suite à une attaque par un virus aérien";
                    strings[164] = "Un gratte-ciel abandonné à cause de l'épidémie";
                    strings[165] = "Naufrage causé par un virus en mer";
                    strings[166] = "Aqueduc contaminé par les morts-vivants";
                    strings[167] = "Une cimenterie rattrapée par les morts-vivants";
                    strings[168] = "Lieu de vacances qui a tourné \ninto un cauchemar";
                    strings[169] = "Un sanctuaire brisé de la pureté de \nit par les morts-vivants";
                    strings[170] = "Une oasis envahie par les morts-vivants";
                    strings[171] = "Un repos repo qui est utile pour tout autre chose que le repos";
                    strings[172] = "Un aéroport russe dépassé par les morts-vivants";
                    strings[173] = "Une autoroute au centre sur les attaques";
                    strings[174] = "Une ferme abandonnée touchée par plus d'un problème";
                    strings[175] = "Autoroute du désert dangereuse pour les morts-vivants";
                    strings[176] = "Un naufrage près d'une ville qui a été infectée";
                    strings[177] = "Une plate-forme pétrolière envahie par plus que les mastodontes";
                    strings[178] = "Une ville amusante qui a été \novertaken par des morts-vivants";
                    strings[179] = "Gulch du désert en plein essor";
                    strings[180] = "Argent: $";
                    strings[181] = "Points bonus:";
                    strings[182] = "Des morts-vivants: ";
                    strings[183] = "Le pouvoir n'est pas activé";
                    strings[184] = "Boss Wave a survécu! \n^320 Deuxième rencontre";
                    strings[185] = "Terminé Boss Wave!";
                    strings[186] = "Une vague de chenille a survécu! \n^320e seconde entre";
                    strings[187] = "Max Ammo récompensé!";
                    strings[188] = "Vague ";
                    strings[189] = " A survécu! \n^320 Deuxième entrée";
                    strings[190] = "Puissance activée par";
                    strings[191] = "L'alimentation a été temporairement activée pendant {0} secondes.";
                    strings[192] = "Le pouvoir n'est pas activé";
                    strings[193] = "Les zombies ont mangé les humains";
                    strings[194] = "La victoire!";
                    strings[195] = "Défaite!";
                    strings[196] = "Gagner";
                    strings[197] = "Perdre";
                    strings[198] = "Les humains ont survécu pour";
                    strings[199] = " Minutes";
                    strings[200] = " Secondes";
                    strings[201] = "Les vagues ont survécu:";
                    strings[202] = "Nous avons gagné ce combat!";
                    strings[203] = "Les zombies ont gagné ce combat";
                    strings[204] = "Machine de la mort!";
                    strings[205] = "Insta-Kill!";
                    strings[206] = "Double points!";
                    strings[207] = "Max Ammo!";
                    strings[208] = "Nuke!";
                    strings[209] = "Vente enflammée!";
                    strings[210] = "Congélateur!";
                    strings[211] = "^5Random";
                    strings[212] = "Votez en utilisant [{+actionslot 4}], [{+actionslot 5}] et [{+actionslot 6}] pour les cartes respectives!";
                    strings[213] = "Votez pour la prochaine carte!";
                    strings[214] = "Prochain tour: {0}";
                    strings[215] = "Carte Killstreak";
                    strings[216] = "1000 Kill Streak!";
                    strings[217] = "Bot permanent atteint!";
                    strings[218] = "^3{0} ^7Vous êtes prêt à utiliser!";
                    strings[219] = "Trop de bots déployés.";
                    strings[220] = "Espace aérien trop encombré.";
                    strings[221] = "Impossible d'appeler Heli Sniper pendant la téléportation";
                    strings[222] = "Impossible d'appeler Heli Sniper ici";
                    strings[223] = " indisponible.";
                    strings[224] = "Nuke déjà entrant!";
                    strings[225] = "Nuke Incoming In: {0}";
                    strings[226] = "Sous-équipe";
                    strings[227] = "L'équipe LMG";
                    strings[228] = "Appuyez sur ^3[{vote no}] ^7pour rediriger le drone.";
                    strings[229] = "M.O.A.B.";
                    strings[230] = "Barrage de chars";
                    strings[231] = "Système de déclenchement Defcon";
                    strings[232] = "A-10 Support";
                    strings[233] = "Lance-flammes";
                    strings[234] = "Inondation éclair";
                    strings[235] = "Humvee Drivable";
                    strings[236] = "Defcon est au niveau {0}";
                    strings[237] = "Un waypoint n'avait pas de liens visibles! Suppression d'un waypoint ...";
                    strings[238] = " Don de 500 $ à {0}!";
                    strings[239] = " Reçu 500 $ de {0}!";
                    strings[240] = "^1Pas assez d'argent pour une arme aléatoire. Besoin ^210$";
                    strings[241] = "^1Pas assez d'argent pour une arme aléatoire. Besoin ^2950$";
                    strings[242] = "Arme Aléatoire!";
                    strings[243] = "Arme améliorée!";
                    strings[244] = "^1Gamble est déjà utilisé!";
                    strings[245] = "^1Vous ne pouvez utiliser le joueur qu'une fois par tour!";
                    strings[246] = "Joueur!";
                    strings[247] = "^1Vous avez gagné {0}.";
                    strings[248] = "^2Vos résultats s'afficheront dans 10 secondes.";
                    strings[249] = "^2Vous avez gagné {0}!";
                    strings[250] = "^1Vous avez perdu {0}!";
                    strings[251] = "^Double santé pendant 30 secondes!";
                    strings[252] = "Double santé terminée!";
                    strings[253] = "^2Santé infinie pendant 30 secondes!";
                    strings[254] = "Santé infinie terminée!";
                    strings[255] = "^2Vous avez 1/2 chance pour Max Ammo!";
                    strings[256] = "^2Vous avez gagné le Max Ammo!";
                    strings[257] = "^1Non Max Ammo.";
                    strings[258] = "^1Dieu décide si tu vis ou meurs en 5 secondes";
                    strings[259] = "^2Vous vivez.";
                    strings[260] = "^1Pas assez d'argent pour Ammo. Besoin ^2$";
                    strings[261] = "Munitions!";
                    strings[262] = "Killstreak aléatoire!";
                    strings[263] = "Solde actuel: {0}$";
                    strings[264] = "Retrait bancaire";
                    strings[265] = "Ascenseur!";
                    strings[266] = "Juggernog";
                    strings[267] = "Stamin-Up";
                    strings[268] = "Cola Rapide";
                    strings[269] = "Mule Kick";
                    strings[270] = "Tapez deux fois";
                    strings[271] = "Stalker Soda";
                    strings[272] = "Quick Revive Pro";
                    strings[273] = "Aide au nettoyage";
                    strings[274] = "Puissance!";
                    strings[275] = "Le pouvoir a été activé";
                    strings[276] = "Porte ouverte!";
                    strings[277] = "Appuyez sur ^3[{+activate}] ^7pour cadeau $^2500 ^7pour ";
                    strings[278] = "Appuyez sur ^3{1} ^7pour ouvrir la porte [Coût: {0}]";
                    strings[279] = "Appuyez sur ^3[{+activate}] ^7pour échanger des armes: ";
                    strings[280] = "Appuyez sur ^3[{+activate}] ^7pour une arme aléatoire [Coût: 10]";
                    strings[281] = "Appuyez sur ^3[{+activate}] ^7pour une arme aléatoire [Coût: 950]";
                    strings[282] = "^1Power doit être activé!";
                    strings[283] = "Appuyez sur ^3[{+activate}] ^7pour prendre ^2votre nouvelle arme améliorée";
                    strings[284] = "Appuyez sur ^3[{+activate}] ^7pour mettre à niveau votre ^1Arme courante ^7[Coût: 5000]";
                    strings[285] = "Appuyez sur ^3[{+activate}] ^7pour utiliser le joueur [coût: 1000]";
                    strings[286] = "Appuyez sur ^3[{+activate}] ^7pour acheter un killstreak aléatoire [Coût: 200 ^5Bonus Points ^7]";
                    strings[287] = "Le téléporteur se refroidit.";
                    strings[288] = "^1Vous devez d'abord connecter le téléporteur!";
                    strings[289] = "Appuyez sur ^3[{+activate}] ^7pour vous téléporter";
                    strings[290] = "Appuyez sur ^3[{+activate}] ^7pour relier le téléporteur";
                    strings[291] = "Appuyez sur ^3[{+activate}] ^7pour utiliser l'ascenseur [Coût: 500]";
                    strings[292] = "Appuyez sur ^3[{+activate}] ^7pour vous retirer du guichet automatique [montant: 1000] [coût: 100] \n\nAppuyez sur ^3[{vote yes}] ^7pour déposer au guichet automatique [Coût: 1000]";
                    strings[293] = "Vous avez déjà {0}!";
                    strings[294] = "Appuyez sur ^3{2} ^7pour acheter {0} [Coût: {1}]";
                    strings[295] = "Vous ne pouvez porter que des {0} avantages!";
                    strings[296] = "Appuyez sur ^3[{+activate}] ^7pour activer l'alimentation [Coût: 10000]";
                    strings[297] = "Tenez ^3[{+activate}] ^7pour munitions explosives";
                    strings[298] = "Maintenez ^3[{+activate}] ^7pour monter à bord de l'héli";
                    strings[299] = " est déjà en train de revivre!";
                    strings[300] = "Maintenez ^3[{+activate}] ^7pour réactiver";
                    strings[301] = "Munitions";
                    strings[302] = "Missile";
                    strings[303] = "Sentinelle";
                    strings[304] = "Restaurateur de vision";
                    strings[305] = "Surtension";
                    strings[306] = "Munitions explosives déployables";
                    strings[307] = "M.O.A.B.";
                    strings[308] = "Drone petit oiseau";
                    strings[309] = "Heli Sniper";
                    strings[310] = "Tourelle de lance-grenades";
                    strings[311] = "Points doubles";
                    strings[312] = "Insta-Kill";
                    strings[313] = "Nuke";
                    strings[314] = "Machine de la mort";
                    strings[315] = "Raid aérien";
                    strings[316] = "Tenez ^3[{+activate}] ^7pour ";
                    strings[317] = "Arme";
                    strings[318] = "Appuyez sur ^3[{+activate}] ^7pour effacer vos péchés.";
                    strings[319] = "Appuyez sur ^3{2} ^7pour {0} ^7[Coût: {1}].";
                    strings[320] = "Appuyez sur ^3[{+activate}] ^7pour ";
                    strings[321] = "Appuyez sur ^3{1} ^7pour Ammo [Coût: {0}].";
                    strings[322] = "Appuyez sur ^3[{+actionslot 3}]] ^7pour équiper P.E.S.";
                    strings[323] = "^5P.E.S. Actif.";
                    strings[324] = "^5Veuillez activer P.E.S. ([{+actionslot 3}])";
                    strings[325] = "Erreur lors du chargement de mapedit pour la carte {0}: {1}";
                    strings[326] = "Entrée MapEdit inconnue {0} ... en ignorant";
                    strings[327] = "Tous les avantages récompensés!";
                    strings[328] = "Votre solde bancaire est déjà au maximum !";
                    strings[329] = "Pluie acide à venir !";
                    strings[330] = "Fil barbelé";
                    strings[331] = "Marée noire";
                    strings[332] = "Attaque au gaz empoisonné";
                    strings[333] = "Artillerie";
                    strings[334] = "Tempête De Neige";
                    strings[335] = "Éruption volcanique";
                    strings[336] = "Bateaux d'assaut";
                    strings[337] = "Arrivée de gaz empoisonné !";
                    strings[338] = "Blizzard entrant !";
                    strings[339] = "Éruption volcanique imminente !";
                    strings[340] = "Le volcan est déjà entré en éruption.";
                    break;
                #endregion
                #region german
                case "german":
                    strings[0] = "Menschen besiegten die Zombies!";
                    strings[1] = "Menschen haben überlebt!";
                    strings[2] = "Gute Arbeit, Menschen!";
                    strings[3] = "Menschen sind am Leben geblieben!";
                    strings[4] = "Menschliches Gesicht: :D!";
                    strings[5] = "Tolle! Menschen leben weiter!";
                    strings[6] = "Große Jobmenschen!";
                    strings[7] = "Gute Arbeit, machen Sie sich bereit für den nächsten Angriff!";
                    strings[8] = "Zombies sind solche Perversen ... Menschen FTW!";
                    strings[9] = "Menschen: 1, Zombies: 0";
                    strings[10] = "Menschen gewinnen Hündinnen!";
                    strings[11] = "Sieg!!!";
                    strings[12] = "Feind nieder !!!";
                    strings[13] = "Kinderleicht!";
                    strings[14] = "Du musst AIZombies Supreme im Team Deathmatch ausführen!";
                    strings[15] = "Die aktuellen maximalen Spieler für AIZombies können nur 8 oder weniger sein. Die aktuelle Einstellung ist {0}. Es wurde auf 8 eingestellt.";
                    strings[16] = "^1AIZombies Supreme Made von ^2Slvr99";
                    strings[17] = "^2Menschen";
                    strings[18] = "^1Warte bis zum Ende der Runde, um zu laichen!";
                    strings[19] = "{0} Wellen überleben.";
                    strings[20] = "Machen Sie sich bereit für den Angriff in:";
                    strings[21] = "Nächste Runde in: ";
                    strings[22] = "^2Willkommen {0}! \n^1AIZombies Supreme {3} \n^3Map: {1} \n^2Made By Slvr99 \n^5Überleben Sie {2} Wellen.";
                    strings[23] = "^1{0} ^1muss wiederbelebt werden!";
                    strings[24] = "^1konnte nicht wiederbelebt werden";
                    strings[25] = "Du bist gestorben. Warten Sie bis zur nächsten Runde, um erneut zu erscheinen.";
                    strings[26] = "^1{0} ^1wurde getötet.";
                    strings[27] = "Konfigurationsdatei für AIZombies wurde nicht gefunden! Einen erstellen ...";
                    strings[28] = "Max Health wurde in der cfg auf einen falschen Wert gesetzt.! Auf Standard setzen ({0})";
                    strings[29] = "Max Juggernog Health wurde in der cfg auf einen falschen Wert gesetzt.! Auf Standard setzen ({0})";
                    strings[30] = "Bot Health wurde in der cfg auf einen falschen Wert gesetzt.! Auf Standard setzen ({0})";
                    strings[31] = "Crawler Health wurde in der cfg auf einen falschen Wert gesetzt.! Auf Standard setzen ({0})";
                    strings[32] = "Boss Health wurde in der cfg auf einen falschen Wert gesetzt.! Auf Standard setzen ({0})";
                    strings[33] = "Der Bot Health Factor wurde in der cfg auf einen falschen Wert gesetzt.! Auf Standard setzen ({0})";
                    strings[34] = "Bot Damage wurde in der cfg auf einen falschen Wert gesetzt.! Auf Standard setzen ({0})";
                    strings[35] = "Perk Limit wurde in der cfg auf einen falschen Wert gesetzt.! Auf Standard setzen ({0})";
                    strings[36] = "Straßen des Todes";
                    strings[37] = "Stürmischer Ausbruch";
                    strings[38] = "Heruntergekommenes Dorf";
                    strings[39] = "Ölraffinerie";
                    strings[40] = "Außenposten der Wüste";
                    strings[41] = "Verlassene U-Bahn";
                    strings[42] = "Baustelle der Hölle";
                    strings[43] = "Abgerissene Unterführung";
                    strings[44] = "Verlassene Straße der Hölle";
                    strings[45] = "Angedockter Tod";
                    strings[46] = "Todesgasse";
                    strings[47] = "Guten Appetit";
                    strings[48] = "Speichereinheit der Hölle";
                    strings[49] = "Seaside Hotel der Hölle";
                    strings[50] = "Auto viel Schmerz";
                    strings[51] = "Großer schwarzer Tod";
                    strings[52] = "El Brote";
                    strings[53] = "Wartorn";
                    strings[54] = "Im Bau";
                    strings[55] = "Tod im Gange";
                    strings[56] = "Schiffbruch";
                    strings[57] = "Toter Aquädukt";
                    strings[58] = "Silla Cement";
                    strings[59] = "Angriff am Meer";
                    strings[60] = "Heiligtum der Hölle";
                    strings[61] = "Paradies";
                    strings[62] = "Rest der Hölle";
                    strings[63] = "Todeszelle";
                    strings[64] = "Pandemische Brücke";
                    strings[65] = "Undead Farm";
                    strings[66] = "Versteck der Hölle";
                    strings[67] = "Shantytown";
                    strings[68] = "Ölbohrinsel";
                    strings[69] = "Todesallee";
                    strings[70] = "Canyon gefangen";
                    strings[71] = "Der Pier";
                    strings[72] = "^1Unbekannte Karte!";
                    strings[73] = " Identifizieren Sie nicht den Entwickler.";
                    strings[74] = "Die Suche nach Gametype-Namen des Servers wurde aus einem unbekannten Grund abgebrochen.";
                    strings[75] = "Beim Festlegen des Gametyp-Namens des Servers ist ein Fehler aufgetreten:";
                    strings[76] = "Beim Festlegen des Gametypnamens des Servers ist ein Fehler aufgetreten: Keine Adressen gefunden!";
                    strings[77] = "Die Suche nach Gametyp-Namen wurde aus einem unbekannten Grund abgebrochen.";
                    strings[78] = "Beim Festlegen des Gametypnamens ist ein Fehler aufgetreten:";
                    strings[79] = "Beim Festlegen des Gametypnamens ist ein Fehler aufgetreten: Keine Adressen gefunden!";
                    strings[80] = "Drücken Sie ^3[{+activate}]^7 um die Sentry Gun aufzunehmen";
                    strings[81] = "";
                    strings[82] = "Die Suche nach Waffen-Patches wurde aus einem unbekannten Grund abgebrochen! Dies kann zu Fehlern bei bestimmten Waffen führen.";
                    strings[83] = "Es ist ein Fehler bei der Suche nach Positionen von Waffen-Patches aufgetreten";
                    strings[84] = "Der Name des benutzerdefinierten Gametyps kann nicht im Server-Browser festgelegt werden!";
                    strings[85] = "Waffendaten für {0} konnten nicht gefunden werden! Bitte melden Sie diesen Fehler an Slvr99";
                    strings[86] = "Boardwalk";
                    strings[87] = "Ein Jahrmarkt am Strand, unterbrochen von den Untoten";
                    strings[88] = "Der Name des benutzerdefinierten Gametyps kann nicht festgelegt werden.";
                    strings[89] = "Die benutzerdefinierte Gamemode-Zeichenfolge kann nicht wiederhergestellt werden! Dies kann zu einem Serverabsturz führen, wenn sich die Karte ändert.";
                    strings[90] = "Alle Skripte von Drittanbietern wurden aufgrund eines Konflikts mit AIZombies von diesem Server entladen.";
                    strings[91] = "Beim Kontaktieren des Update-Prüfservers ist ein Fehler aufgetreten !: {0}";
                    strings[92] = "Beim Kontaktieren des Update-Prüfservers ist ein Fehler aufgetreten !: Keine Antwort vom Server.";
                    strings[93] = "Es gibt ein Update für AIZombies! Version {0} wird jetzt heruntergeladen ...";
                    strings[94] = "Beim Herunterladen des Updates vom Server ist ein Fehler aufgetreten !: {0}";
                    strings[95] = "Beim Ersetzen der alten AIZombies-Datei ist ein Fehler aufgetreten! Stellen Sie sicher, dass die Datei nicht schreibgeschützt ist oder woanders geöffnet ist.";
                    strings[96] = "Download abgeschlossen! Die Updates werden wirksam, sobald das aktuelle Spiel beendet ist.";
                    strings[97] = "Zusätzliches Geld!";
                    strings[98] = "Extra Bonuspunkte!";
                    strings[99] = "Zufälliger Vorteil!";
                    strings[100] = "Keine Bot-Spawns verfügbar! Bitte haben Sie mindestens eine \"Zombiespawn\" in Ihrer Map-Datei.";
                    strings[101] = "^1Keine Bot-Laichen verfügbar! Überprüfen Sie die Konsole auf Details";
                    strings[102] = "Mehr Gesundheit";
                    strings[103] = "Sprint schneller und länger";
                    strings[104] = "Laden Sie schneller";
                    strings[105] = "Ein zusätzlicher Waffenplatz";
                    strings[106] = "Schnellere Feuerrate";
                    strings[107] = "Schneller in Bewegung während ADS";
                    strings[108] = "Automatisch nach dem Ausfall wiederbelebt werden \n+ Dein Team schneller wiederbeleben";
                    strings[109] = "Reinige freie Munition";
                    strings[110] = "Lockdown";
                    strings[111] = "Bootleg";
                    strings[112] = "Mission";
                    strings[113] = "Carbon";
                    strings[114] = "Dome";
                    strings[115] = "Downturn";
                    strings[116] = "Hardhat";
                    strings[117] = "Interchange";
                    strings[118] = "Fallen";
                    strings[119] = "Bakaara";
                    strings[120] = "Resistance";
                    strings[121] = "Arkaden";
                    strings[122] = "Outpost";
                    strings[123] = "Seatown";
                    strings[124] = "Underground";
                    strings[125] = "Village";
                    strings[126] = "Piazza";
                    strings[127] = "Liberation";
                    strings[128] = "Black Box";
                    strings[129] = "Overwatch";
                    strings[130] = "Aground";
                    strings[131] = "Erosion";
                    strings[132] = "Foundation";
                    strings[133] = "Getaway";
                    strings[134] = "Sanctuary";
                    strings[135] = "Oasis";
                    strings[136] = "Lookout";
                    strings[137] = "Terminal";
                    strings[138] = "Intersection";
                    strings[139] = "Vortex";
                    strings[140] = "U-Turn";
                    strings[141] = "Decommission";
                    strings[142] = "Offshore";
                    strings[143] = "Parish";
                    strings[144] = "Gulch";
                    strings[145] = "Einmal friedliche Straßen von Untoten überrannt";
                    strings[146] = "Stürmische Stadt, die von den Untoten übernommen wurde";
                    strings[147] = "Ein heruntergekommenes Dorf, das von den Untoten übernommen wurde";
                    strings[148] = "Eine einst lebhafte Ölraffinerie";
                    strings[149] = "Ein verlassener Außenposten in der Wüste";
                    strings[150] = "Die Folgen eines gefährlichen Fehlers";
                    strings[151] = "Eine kleine Baustelle unvollendet";
                    strings[152] = "Eine zerstörte Autobahn, die aus einem Ausbruch resultiert";
                    strings[153] = "Eine verlassene russische Geisterstadt";
                    strings[154] = "Eine afrikanische Stadt, die von den Untoten getroffen wird";
                    strings[155] = "Pariser Viertel nicht mehr sicher";
                    strings[156] = "Deutsche Mall von den Untoten ausgeraubt";
                    strings[157] = "Sibirischer Außenposten wurde zum Ausbruch evakuiert";
                    strings[158] = "Eine Stadt am Meer, nicht sicher vor den Untoten";
                    strings[159] = "U-Bahn-Station, die als \nVeranstaltungsweg benutzt wird";
                    strings[160] = "Afrikanisches Dorf mit gefährlicher Krankheit geritten";
                    strings[161] = "Eine spanische Stadt, die mit Muertos infiziert ist";
                    strings[162] = "Eine Militärbasis, die in einem Park zum Schutz vor Untoten errichtet wurde";
                    strings[163] = "Absturzstelle eines Luftvernichtungsangriffs";
                    strings[164] = "Ein Wolkenkratzer wurde wegen des Ausbruchs aufgegeben";
                    strings[165] = "Schiffbruch durch einen Virus auf See";
                    strings[166] = "Aquädukt durch die Untoten verunreinigt";
                    strings[167] = "Zementfabrik wird von Untoten überholt";
                    strings[168] = "Ein Urlaubsort, der zu einem Albtraum wurde";
                    strings[169] = "Ein Heiligtum, das von den Untoten aus der Reinheit gebrochen wurde";
                    strings[170] = "Eine Oase, die von Untoten überrannt wurde";
                    strings[171] = "Ein Rest-Repo, das für alles andere als für die Erholung nützlich ist";
                    strings[172] = "Ein russischer Flughafen, der von den Untoten überholt wird";
                    strings[173] = "Eine Autobahn in der Mitte bei den Angriffen";
                    strings[174] = "Verlassene Farm wurde um \nmehr als ein Problem getroffen";
                    strings[175] = "Wüstenautobahn unsicher vor Untoten";
                    strings[176] = "Ein Schiffbruch in der Nähe einer Stadt, die infiziert wurde";
                    strings[177] = "Eine Bohrinsel ist \nmehr als Moloch überfahren";
                    strings[178] = "Eine lustige Stadt, die von Untoten neu erfunden wurde";
                    strings[179] = "Wüstenschlucht mit Untoten gedeihen";
                    strings[180] = "Geld: $";
                    strings[181] = "Bonuspunkte:";
                    strings[182] = "Zombies: ";
                    strings[183] = "Die Stromversorgung ist nicht aktiviert";
                    strings[184] = "Boss Wave überlebt! \n^320 Sekunden Pause";
                    strings[185] = "Boss Wave abgeschlossen!";
                    strings[186] = "Crawler Wave überlebt! \n^320 Sekunden Pause";
                    strings[187] = "Max Munition ausgezeichnet!";
                    strings[188] = "Welle ";
                    strings[189] = " Überlebt! \n^320 Sekunden Pause";
                    strings[190] = "Energie aktiviert von";
                    strings[191] = "Die Stromversorgung wurde vorübergehend für {0} Sekunden aktiviert";
                    strings[192] = "Die Stromversorgung ist nicht aktiviert";
                    strings[193] = "Zombies aßen die Menschen";
                    strings[194] = "Sieg!";
                    strings[195] = "Niederlage!";
                    strings[196] = "Sieg";
                    strings[197] = "Verlieren";
                    strings[198] = "Menschen überlebten für";
                    strings[199] = " Protokoll";
                    strings[200] = " Sekunden";
                    strings[201] = "Wellen überlebt:";
                    strings[202] = "Wir haben diesen Kampf gewonnen!";
                    strings[203] = "Zombies haben diesen Kampf gewonnen";
                    strings[204] = "Todesmaschine!";
                    strings[205] = "Insta-Kill!";
                    strings[206] = "Doppelte Punkte";
                    strings[207] = "Max Munition";
                    strings[208] = "Nuke!";
                    strings[209] = "Feuer Sale!";
                    strings[210] = "Gefrierschrank!";
                    strings[211] = "^5Zufällig";
                    strings[212] = "Stimmen Sie mit [{+actionslot 4}], [{+actionslot 5}] und [{+actionslot 6}] für die jeweiligen Karten ab!";
                    strings[213] = "Stimmen Sie für die nächste Karte ab!";
                    strings[214] = "Nächste Runde in: {0}";
                    strings[215] = "Karte Killstreak";
                    strings[216] = "1000 Kill Streak!";
                    strings[217] = "Permanenter Bot erreicht!";
                    strings[218] = "^3{0} ^7zur Verwendung bereit!";
                    strings[219] = "Zu viele Bots bereitgestellt.";
                    strings[220] = "Luftraum zu voll.";
                    strings[221] = "Während des Teleportierens kann kein Heli Sniper angerufen werden";
                    strings[222] = "Kann Heli Sniper hier nicht anrufen";
                    strings[223] = " Nicht verfügbar.";
                    strings[224] = "Nuke schon inbound!";
                    strings[225] = "Nuke Incoming In: {0}";
                    strings[226] = "Unterguppe";
                    strings[227] = "LMG-Team";
                    strings[228] = "Drücken Sie ^3[{vote no}] ^7, um die Drohne neu zu routen";
                    strings[229] = "M.O.A.B.";
                    strings[230] = "Tank Barrage";
                    strings[231] = "Defcon-Auslösesystem";
                    strings[232] = "A-10-Unterstützung";
                    strings[233] = "Flammenwerfer";
                    strings[234] = "Sturzflut";
                    strings[235] = "Fahrbarer Humvee";
                    strings[236] = "Defcon ist auf Stufe {0}";
                    strings[237] = "Ein Wegpunkt hatte keine sichtbaren Links! Wegpunkt löschen ...";
                    strings[238] = " $500 an {0} geschenkt!";
                    strings[239] = " $500 von {0} erhalten!";
                    strings[240] = "^1Nicht genug Geld für eine Zufallswaffe. Benötigen Sie ^2$10";
                    strings[241] = "^1Nicht genug Geld für eine Zufallswaffe. Benötigen Sie ^2$950";
                    strings[242] = "Zufällige Waffe!";
                    strings[243] = "Waffe aufgerüstet!";
                    strings[244] = "^1Gambler wird bereits verwendet!";
                    strings[245] = "^1Der Spieler darf nur einmal pro Runde verwendet werden!";
                    strings[246] = "Spieler!";
                    strings[247] = "^1Sie haben {0} gewonnen.";
                    strings[248] = "^2Ihre Ergebnisse werden in 10 Sekunden angezeigt.";
                    strings[249] = "^2Sie haben {0} gewonnen!";
                    strings[250] = "^1Sie haben {0} verloren!";
                    strings[251] = "^Doppelte Gesundheit für 30 Sekunden!";
                    strings[252] = "Doppelte Gesundheit vorbei!";
                    strings[253] = "^2Unendliche Gesundheit für 30 Sekunden!";
                    strings[254] = "Unendliche Gesundheit vorbei!";
                    strings[255] = "^2Sie haben eine halbe Chance für Max Ammo!";
                    strings[256] = "^2Du hast die Max-Munition gewonnen!";
                    strings[257] = "^1No Max Munition.";
                    strings[258] = "^1Gott entscheidet, ob Sie in 5 Sekunden leben oder sterben";
                    strings[259] = "Du lebst.";
                    strings[260] = "^1Nicht Geld für Munition. Benötigen Sie ^2$";
                    strings[261] = "Munition!";
                    strings[262] = "Zufälliger Killstreak!";
                    strings[263] = "Aktueller Kontostand: ${0}";
                    strings[264] = "Banküberweisung";
                    strings[265] = "Aufzug!";
                    strings[266] = "Juggernog";
                    strings[267] = "Stamin-Up";
                    strings[268] = "Geschwindigkeit Cola";
                    strings[269] = "Maultier-Tritt";
                    strings[270] = "Doppeltippen Sie auf";
                    strings[271] = "Stalker Soda";
                    strings[272] = "Quick Revive Pro";
                    strings[273] = "Aufräumhilfe";
                    strings[274] = "Leistung!";
                    strings[275] = "Die Stromversorgung wurde aktiviert";
                    strings[276] = "Geöffnete Tür!";
                    strings[277] = "Drücken Sie ^3[{+activate}] ^7, um $^2500 ^7zu verschenken ";
                    strings[278] = "Drücken Sie ^3{1} ^7, um die Tür zu öffnen [Kosten: {0}]";
                    strings[279] = "Drücke ^3[{+activate}] ^7, um Waffen zu handeln: ";
                    strings[280] = "Drücke ^3[{+activate}] ^7für eine zufällige Waffe [Kosten: 10]";
                    strings[281] = "Drücke ^3[{+activate}] ^7für eine zufällige Waffe [Kosten: 950]";
                    strings[282] = "^1Power muss aktiviert sein!";
                    strings[283] = "Drücke ^3[{+activate}] ^7, um ^2deine neue verbesserte Waffe zu nehmen";
                    strings[284] = "Drücke ^3[{+activate}] ^7, um ein Upgrade deiner ^1Current-Waffe ^7[Kosten: 5000] durchzuführen.";
                    strings[285] = "Drücke ^3[{+activate}] ^7, um den Spieler zu benutzen [Kosten: 1000]";
                    strings[286] = "Drücke ^3[{+activate}] ^7, um eine zufällige Killstreak [Kosten: 200 ^5Bonus-Punkte ^7] zu kaufen.";
                    strings[287] = "Der Teleporter kühlt sich ab.";
                    strings[288] = "^1Sie müssen den Teleporter zuerst verbinden!";
                    strings[289] = "Drücken Sie ^3[{+activate}] ^7, um zu teleportieren";
                    strings[290] = "Drücken Sie ^3[{+ enable}] ^7, um den Teleporter zu verknüpfen";
                    strings[291] = "Drücken Sie ^3[{+activate}] ^7, um den Aufzug zu verwenden [Kosten: 500]";
                    strings[292] = "Drücken Sie ^3[{+activate}] ^7, um den Geldautomaten abzuheben [Betrag: 1000] [Kosten: 100] \n\nDrücken Sie ^3[{vote yes}] ^7, um den Geldautomaten zu hinterlegen [Kosten: 1000]";
                    strings[293] = "Sie haben bereits {0}!";
                    strings[294] = "Drücken Sie ^3{2} ^7, um {0} [Kosten: {1}] zu kaufen.";
                    strings[295] = "Sie dürfen nur {0} Vorteile haben!";
                    strings[296] = "Drücken Sie ^3[{+activate}] ^7, um die Stromversorgung [Kosten: 10000] zu aktivieren.";
                    strings[297] = "Halten Sie ^3[{+activate}] ^7für explosive Munition";
                    strings[298] = "Halten Sie ^3[{+ enable}] ^7gedrückt, um den Heli zu besteigen";
                    strings[299] = " wird bereits wiederbelebt!";
                    strings[300] = "Halten Sie ^3[{+ enable}] ^7gedrückt, um wieder zu beleben";
                    strings[301] = "Munition";
                    strings[302] = "Rakete";
                    strings[303] = "Geschützturm";
                    strings[304] = "Vision Restorer";
                    strings[305] = "Überspannung";
                    strings[306] = "Einsetzbare explosive Munition";
                    strings[307] = "M.O.A.B.";
                    strings[308] = "Littlebird-Drohne";
                    strings[309] = "Heli Sniper";
                    strings[310] = "Granatwerferwerfer";
                    strings[311] = "Doppelte Punkte";
                    strings[312] = "Insta-Kill";
                    strings[313] = "Nuke";
                    strings[314] = "Todesmaschine";
                    strings[315] = "Luftangriff";
                    strings[316] = "Halten Sie ^3[{+activate}] ^7für ";
                    strings[317] = "Waffe";
                    strings[318] = "Drücken Sie ^3[{+activate}] ^7, um Ihre Sünden wegzuwaschen.";
                    strings[319] = "Drücken Sie ^3{2} ^7für {0} ^7[Kosten: {1}]";
                    strings[320] = "Drücken Sie ^3[{+activate}] ^7für ";
                    strings[321] = "Drücke ^3{1} ^7für Munition [Kosten: {0}]";
                    strings[322] = "Drücken Sie ^3[{+actionslot 3}] ^7, um P.E.S.";
                    strings[323] = "^5P.E.S. Aktiv.";
                    strings[324] = "^5Bitte aktivieren Sie P.E.S. ([{+actionslot 3}])";
                    strings[325] = "Fehler beim Laden von Mapedit für Karte {0}: {1}";
                    strings[326] = "Unknown MapEdit Eintrag {0} ... wird ignoriert";
                    strings[327] = "Alle Vergünstigungen ausgezeichnet!";
                    strings[328] = "Ihr Bankguthaben ist bereits bei max!";
                    strings[329] = "Saurer Regen kommt!";
                    strings[330] = "Stacheldraht";
                    strings[331] = "Ölpest";
                    strings[332] = "Giftgasangriff";
                    strings[333] = "Artillerie";
                    strings[334] = "Schneesturm";
                    strings[335] = "Vulkanausbruch";
                    strings[336] = "Sturmboote";
                    strings[337] = "Giftgas eingehend!";
                    strings[338] = "Blizzard-Inbound!";
                    strings[339] = "Vulkanausbruch steht unmittelbar bevor!";
                    strings[340] = "Der Vulkan ist bereits ausgebrochen.";
                    break;
                #endregion
                #region russian
                /*
            case "russian":
                strings[0] = "Люди победили зомби!";
                strings[1] = "Люди выжили!";
                strings[2] = "Хорошая работа, люди!";
                strings[3] = "Люди остались живы!";
                strings[4] = "Человеческое лицо: :D!";
                strings[5] = "Удивительно! Люди живут дальше!";
                strings[6] = "Отличная работа, люди!";
                strings[7] = "Хорошая работа, будьте готовы к следующей атаке!";
                strings[8] = "Зомби такие извращенцы ... Люди FTW!";
                strings[9] = "Людей: 1, Зомби: 0";
                strings[10] = "Люди выигрывают суки!";
                strings[11] = "Победа !!!";
                strings[12] = "Враг убит!!!";
                strings[13] = "Очень просто!";
                strings[14] = "Вы должны запустить AIZombies Supreme в Team Deathmatch!";
                strings[15] = "Текущий максимум игроков для AIZombies может быть только 8 или ниже. Текущая настройка: {0}. Было установлено 8.";
                strings[16] = "^1AIZombies Supreme Сделано ^2Slvr99";
                strings[17] = "^2Humans";
                strings[18] = "^1Дождаться до конца раунда, чтобы появиться!";
                strings[19] = "Выжить {0} волн.";
                strings[20] = "Будьте готовы к атаке в:";
                strings[21] = "Следующий раунд в: ";
                strings[22] = "^2желанный {0}! \n^1AIZombies Supreme {3} \n^3карта: {1} \n^2Сделано Slvr99 \n^5Выжить {2} волны.";
                strings[23] = "^1{0} ^1нужно возродить!";
                strings[24] = "^1Не удалось оживить";
                strings[25] = "^1Вы умерли. Подождите до следующего раунда, чтобы возродиться.";
                strings[26] = "^1{0} ^1был убит.";
                strings[27] = "Файл конфигурации для AIZombies не найден! Создание одного ...";
                strings[28] = "Максимальное значение здоровья было установлено в cfg !, неверное значение по умолчанию ({0})";
                strings[29] = "Максимальному здоровью джаггернога было присвоено неверное значение в cfg !, задано значение по умолчанию ({0})";
                strings[30] = "Bot Health был установлен на неверное значение в cfg !, установлен по умолчанию ({0})";
                strings[31] = "Для Crawler Health установлено неверное значение в cfg !, задано значение по умолчанию ({0})";
                strings[32] = "Для здоровья босса было установлено неверное значение в cfg !, задано значение по умолчанию ({0})";
                strings[33] = "Bot Health Factor было установлено неверное значение в cfg !, установлено по умолчанию ({0})";
                strings[34] = "Bot Damage был установлен на неверное значение в cfg !, установлен по умолчанию ({0})";
                strings[35] = "Perk Limit был установлен на неверное значение в cfg !, установлен по умолчанию ({0})";
                strings[36] = "Улицы смерти";
                strings[37] = "Бурная вспышка";
                strings[38] = "Rundown Village";
                strings[39] = "Нефтеперегонный завод";
                strings[40] = "Пустыня Форпост";
                strings[41] = "Заброшенное Метро";
                strings[42] = "Строительная площадка Ада";
                strings[43] = "Разрушенный подземный переход";
                strings[44] = "Заброшенная дорога ада";
                strings[45] = "Состыкованная смерть";
                strings[46] = "Аллея Смерти";
                strings[47] = "Костный аппетит";
                strings[48] = "Единица хранения Ада";
                strings[49] = "Приморский Отель Ада";
                strings[50] = "Автомобиль много боли";
                strings[51] = "Большая Черная Смерть";
                strings[52] = "Эль Броте";
                strings[53] = "Обломок";
                strings[54] = "В разработке";
                strings[55] = "Смерть в прогрессе";
                strings[56] = "Кораблекрушение";
                strings[57] = "Мертвый Акведук";
                strings[58] = "Силла Цемент";
                strings[59] = "Атака со стороны океана";
                strings[60] = "Святилище ада";
                strings[61] = "Рай";
                strings[62] = "Место отдыха Ада";
                strings[63] = "Death Row";
                strings[64] = "Пандемический мост";
                strings[65] = "Ферма нежити";
                strings[66] = "Убежище Ада";
                strings[67] = "бидонвиль";
                strings[68] = "Нефтяная вышка";
                strings[69] = "Проспект смерти";
                strings[70] = "Захваченный каньон";
                strings[71] = "Пирс";
                strings[72] = "^1Неизвестная карта!";
                strings[73] = " Пожалуйста, не выдавайте себя за разработчика.";
                strings[74] = "Поиск имени типа игрового сервера был отменен по неизвестной причине.";
                strings[75] = "При установке имени игрового типа сервера произошла ошибка !:";
                strings[76] = "Произошла ошибка при установке имени типа сервера: адреса не найдены!";
                strings[77] = "Поиск по названию игрового типа был отменен по неизвестной причине.";
                strings[78] = "При установке имени типа игры произошла ошибка !:";
                strings[79] = "При установке имени типа игры произошла ошибка: адреса не найдены!";
                strings[80] = "Нажмите ^3[{+активировать}]^7 чтобы взять турель.";
                strings[81] = "";
                strings[82] = "Поиск патча оружия был отменен по неизвестной причине! Это может привести к появлению ошибок для определенного оружия.";
                strings[83] = "При поиске мест для патчей оружия произошла ошибка:";
                strings[84] = "Невозможно установить имя пользовательского типа игры в браузере сервера!";
                strings[85] = "Не удалось найти данные об оружии для {0}! Пожалуйста, сообщите об этой ошибке Slvr99";
                strings[86] = "Boardwalk";
                strings[87] = "Пляжная ярмарка, прерванная нежитью";
                strings[88] = "Невозможно установить название пользовательского игрового типа.";
                strings[89] = "Невозможно восстановить пользовательскую строку игрового режима! Это может привести к сбою сервера при изменении карты!";
                strings[90] = "Все сторонние скрипты были выгружены с этого сервера из-за конфликта с AIZombies.";
                strings[91] = "При подключении к серверу проверки обновлений произошла ошибка !: {0}";
                strings[92] = "Произошла ошибка при обращении к серверу проверки обновлений! Нет ответа от сервера.";
                strings[93] = "Доступно обновление для AIZombies! Загрузка версии {0} сейчас ...";
                strings[94] = "При загрузке обновления с сервера произошла ошибка !: {0}";
                strings[95] = "Произошла ошибка при замене старого файла AIZombies! Убедитесь, что файл не только для чтения или открыт в другом месте.";
                strings[96] = "Загрузка завершена! Обновления вступят в силу после окончания текущей игры.";
                strings[97] = "Дополнительные деньги!";
                strings[98] = "Дополнительные бонусные баллы!";
                strings[99] = "Случайный перк!";
                strings[100] = "Боты не появляются! Пожалуйста, имейте хотя бы одну \"зомби-пешку\" в вашем файле карты.";
                strings[101] = "^1Нет ботов не появляется! Проверьте консоль для деталей";
                strings[102] = "Больше здоровья";
                strings[103] = "Спринт быстрее и дольше";
                strings[104] = "Перезарядите быстрее";
                strings[105] = "Слот для дополнительного оружия";
                strings[106] = "Более высокая скорость стрельбы";
                strings[107] = "Быстрее двигаться во время рекламы";
                strings[108] = "Автоматически возродиться вскоре после падения \n+ Оживи свою команду быстрее";
                strings[109] = "Уничтожить боеприпасы";
                strings[110] = "Lockdown";
                strings[111] = "Bootleg";
                strings[112] = "Mission";
                strings[113] = "Carbon";
                strings[114] = "Dome";
                strings[115] = "Downturn";
                strings[116] = "Hardhat";
                strings[117] = "Interchange";
                strings[118] = "Fallen";
                strings[119] = "Bakaara";
                strings[120] = "Resistance";
                strings[121] = "Arkaden";
                strings[122] = "Outpost";
                strings[123] = "Seatown";
                strings[124] = "Underground";
                strings[125] = "Village";
                strings[126] = "Piazza";
                strings[127] = "Liberation";
                strings[128] = "Black Box";
                strings[129] = "Overwatch";
                strings[130] = "Aground";
                strings[131] = "Erosion";
                strings[132] = "Foundation";
                strings[133] = "Getaway";
                strings[134] = "Sanctuary";
                strings[135] = "Oasis";
                strings[136] = "Lookout";
                strings[137] = "Terminal";
                strings[138] = "Intersection";
                strings[139] = "Vortex";
                strings[140] = "U-Turn";
                strings[141] = "Decommission";
                strings[142] = "Offshore";
                strings[143] = "Parish";
                strings[144] = "Gulch";
                strings[145] = "Однажды мирные улицы наводнили нежить";
                strings[146] = "Бурный город захвачен нежитью";
                strings[147] = "Изношенная деревня, захваченная нежитью";
                strings[148] = "Некогда оживленный нефтеперерабатывающий завод";
                strings[149] = "Заброшенный аванпост в пустыне";
                strings[150] = "Последствия опасной ошибки";
                strings[151] = "Небольшая строительная площадка незакончена";
                strings[152] = "Разрушенная автострада \nВ результате вспышки";
                strings[153] = "Заброшенный русский город-призрак";
                strings[154] = "Африканский город, пораженный нежитью";
                strings[155] = "Парижский район больше не безопасен";
                strings[156] = "Немецкий торговый центр ограблен нежитью";
                strings[157] = "Сибирская застава эвакуирована \nдо вспышки";
                strings[158] = "Приморский город небезопасен от нежити";
                strings[159] = "Станция метро используется как маршрут эвакуации";
                strings[160] = "Африканская деревня с опасной болезнью";
                strings[161] = "Испанский город заражен муэрто";
                strings[162] = "Военная база построена в парке для защиты от нежити";
                strings[163] = "Место крушения воздушной атаки \nvirus";
                strings[164] = "Небоскреб заброшен из-за вспышки";
                strings[165] = "Кораблекрушение, вызванное вирусом в море";
                strings[166] = "Акведук загрязнен нежитью";
                strings[167] = "Цементный завод обогнал нежить";
                strings[168] = "Место для отдыха, которое превратилось \ninto в кошмар";
                strings[169] = "Святилище, разрушенное нежностью \nit's нежитью";
                strings[170] = "Оазис, наводненный нежитью";
                strings[171] = "Репозиторий для отдыха, полезный для всего, кроме отдыха";
                strings[172] = "Российский аэропорт обогнал нежить";
                strings[173] = "Автострада в центре на атаки";
                strings[174] = "Заброшенная ферма пострадала от \nmore одной проблемы";
                strings[175] = "Автострада пустыни небезопасна от нежити";
                strings[176] = "Кораблекрушение возле города \nthat был заражен";
                strings[177] = "Нефтяная вышка захвачена \nбольше, чем джагернауты";
                strings[178] = "Забавный город, который \novertaken от нежити";
                strings[179] = "Пустынный ущелье процветает с нежитью";
                strings[180] = "Деньги: $";
                strings[181] = "Бонусные очки:";
                strings[182] = "зомби: ";
                strings[183] = "Питание не активировано";
                strings[184] = "Босс Волна выжил! \n^320 Второй антракт";
                strings[185] = "Завершен Босс Волна!";
                strings[186] = "Волна гусеницы выжила! \n^320 Второй антракт";
                strings[187] = "Макс Боеприпасы Награжден!";
                strings[188] = "Волна ";
                strings[189] = " Выжил! \n^320 Второй антракт";
                strings[190] = "Мощность активируется";
                strings[191] = "Питание временно активировано на {0} секунд";
                strings[192] = "Питание не активировано";
                strings[193] = "Зомби съели людей";
                strings[194] = "Победа!";
                strings[195] = "Поражение!";
                strings[196] = "Выиграть";
                strings[197] = "потерять";
                strings[198] = "Люди выжили для";
                strings[199] = " минут";
                strings[200] = " секунд";
                strings[201] = "Волны выжили:";
                strings[202] = "Мы выиграли этот бой!";
                strings[203] = "Зомби выиграли эту битву";
                strings[204] = "Машина смерти!";
                strings[205] = "Insta-Kill!";
                strings[206] = "Двойные очки!";
                strings[207] = "Макс Боеприпасы!";
                strings[208] = "Nuke!";
                strings[209] = "Горячая распродажа!";
                strings[210] = "Морозильник!";
                strings[211] = "^5Random";
                strings[212] = "Проголосуйте, используя [{+actionslot 4}], [{+actionslot 5}] и [{+actionslot 6}] для соответствующих карт!";
                strings[213] = "Проголосуй за следующую карту!";
                strings[214] = "Следующий раунд: {0}";
                strings[215] = "Карта Killstreak";
                strings[216] = "1000 серий убийств!";
                strings[217] = "Постоянный бот достигнут!";
                strings[218] = "^3{0} ^7готов к использованию!";
                strings[219] = "Развернуто слишком много ботов.";
                strings[220] = "Воздушное пространство слишком переполнено.";
                strings[221] = "Не могу позвонить в Хели Снайпер во время телепортации";
                strings[222] = "Не могу позвонить в Хели Снайпер здесь";
                strings[223] = " недоступен.";
                strings[224] = "Нюка уже входящая!";
                strings[225] = "Nuke Incoming In: {0}";
                strings[226] = "Подгруппа";
                strings[227] = "Команда LMG";
                strings[228] = "Нажмите ^3[{vote no}] ^7, чтобы перенаправить дрон";
                strings[229] = "M.O.A.B.";
                strings[230] = "Танковый заграждение";
                strings[231] = "Система запуска Defcon";
                strings[232] = "Поддержка A-10";
                strings[233] = "Огнемет";
                strings[234] = "Внезапное наводнение";
                strings[235] = "Drivable Humvee";
                strings[236] = "Defcon находится на уровне {0}";
                strings[237] = "У путевой точки не было видимых ссылок! Удаление путевой точки ...";
                strings[238] = " Подарено $ 500 на {0}!";
                strings[239] = " Получил $ 500 от {0}!";
                strings[240] = "^1Не хватает денег для случайного оружия. Нужно ^2$ 10";
                strings[241] = "^1Не хватает денег для случайного оружия. Нужно ^2$ 950";
                strings[242] = "Случайное оружие!";
                strings[243] = "Оружие улучшено!";
                strings[244] = "^1Gambler уже используется!";
                strings[245] = "^1Вы можете использовать игрока только один раз за раунд!";
                strings[246] = "Азартный игрок!";
                strings[247] = "^1Вы выиграли {0}.";
                strings[248] = "^2Ваши результаты будут отображаться через 10 секунд.";
                strings[249] = "^2Вы выиграли {0}!";
                strings[250] = "^1Вы потеряли {0}!";
                strings[251] = "^Двойное здоровье на 30 секунд!";
                strings[252] = "Двойное здоровье окончено!";
                strings[253] = "^2Бесконечное здоровье на 30 секунд!";
                strings[254] = "Бесконечное здоровье окончено!";
                strings[255] = "^2У вас есть 1/2 шанса на Макс Боеприпасы!";
                strings[256] = "^2Вы выиграли максимальный патрон!";
                strings[257] = "^1Не Макс Боеприпасы.";
                strings[258] = "^1Бог решает, если вы живете или умрете за 5 секунд";
                strings[259] = "^2Вы живете.";
                strings[260] = "^1Не хватает денег на патроны. Нужно ^2$";
                strings[261] = "Патроны!";
                strings[262] = "Случайное убийство!";
                strings[263] = "Текущий баланс: $ {0}";
                strings[264] = "Банковский вывод";
                strings[265] = "Лифт!";
                strings[266] = "Juggernog";
                strings[267] = "Stamin-Up";
                strings[268] = "Speed ​​Cola";
                strings[269] = "Mule Kick";
                strings[270] = "Двойное нажатие";
                strings[271] = "Сталкер Сода";
                strings[272] = "Quick Revive Pro";
                strings[273] = "Мусоробот помощь";
                strings[274] = "Мощность!";
                strings[275] = "Сила была активирована";
                strings[276] = "Открытая дверь!";
                strings[277] = "Нажмите ^3[{+activate}] ^7, чтобы подарить $ ^2500 ^7, чтобы ";
                strings[278] = "Нажмите ^3{1} ^7, чтобы открыть дверь [Стоимость: {0}]";
                strings[279] = "Нажмите ^3[{+activate}] ^7, чтобы обменять оружие: ";
                strings[280] = "Нажмите ^3[{+activate}] ^7для случайного оружия [Стоимость: 10]";
                strings[281] = "Нажмите ^3[{+activate}] ^7для случайного оружия [Стоимость: 950]";
                strings[282] = "^1Сила должна быть активирована!";
                strings[283] = "Нажмите ^3[{+activate}] ^7, чтобы взять ^2ваше новое модернизированное оружие";
                strings[284] = "Нажмите ^3[{+activate}] ^7, чтобы обновить свое ^1Токружение оружия ^7[Стоимость: 5000]";
                strings[285] = "Нажмите ^3[{+activate}] ^7, чтобы использовать Игрока [Стоимость: 1000]";
                strings[286] = "Нажмите ^3[{+activate}] ^7, чтобы купить случайную серию убийств [Стоимость: 200 ^5Бонусных очков ^7]";
                strings[287] = "Телепорт охлаждается.";
                strings[288] = "^1Вы должны сначала связать телепорт!";
                strings[289] = "Нажмите ^3[{+activate}] ^7, чтобы телепортироваться";
                strings[290] = "Нажмите ^3[{+activate}] ^7, чтобы связать телепорт";
                strings[291] = "Нажмите ^3[{+activate}] ^7, чтобы использовать лифт [Стоимость: 500]";
                strings[292] = "Нажмите ^3[{+activate}] ^7, чтобы выйти из банкомата [Сумма: 1000] [Стоимость: 100] \n\nНажмите ^3[{vote yes}] ^7, чтобы внести в банкомат [Стоимость: 1000]";
                strings[293] = "У вас уже есть {0}!";
                strings[294] = "Нажмите ^3{2} ^7, чтобы купить {0} [Стоимость: {1}]";
                strings[295] = "Вы можете носить только {0} перков!";
                strings[296] = "Нажмите ^3[{+activate}] ^7, чтобы активировать питание [Стоимость: 10000]";
                strings[297] = "Удерживайте ^3[{+activate}] ^7для взрывных патронов";
                strings[298] = "Удерживайте ^3[{+activate}] ^7, чтобы сесть на Хели";
                strings[299] = " уже возрождается!";
                strings[300] = "Удерживайте ^3[{+activate}] ^7, чтобы оживить";
                strings[301] = "боеприпасы";
                strings[302] = "ракета";
                strings[303] = "Sentry Gun";
                strings[304] = "Vision Restorer";
                strings[305] = "Скачок напряжения";
                strings[306] = "Развертываемые взрывчатые боеприпасы";
                strings[307] = "M.O.A.B.";
                strings[308] = "Littlebird Drone";
                strings[309] = "Хели Снайпер";
                strings[310] = "Гранатомет турель";
                strings[311] = "Двойные очки";
                strings[312] = "Insta-убийство";
                strings[313] = "ядерное оружие";
                strings[314] = "Машина смерти";
                strings[315] = "Авиаудар";
                strings[316] = "Удерживайте ^3[{+activate}] ^7для ";
                strings[317] = "Оружие";
                strings[318] = "Нажмите ^3[{+activate}] ^7, чтобы смыть ваши грехи.";
                strings[319] = "Нажмите ^3{2} ^7для {0} ^7[Стоимость: {1}]";
                strings[320] = "Нажмите ^3[{+activate}] ^7для ";
                strings[321] = "Нажмите ^3{1} ^7для боеприпасов [Стоимость: {0}]";
                strings[322] = "Нажмите ^3[{+actionslot 3}] ^7, чтобы оборудовать P.E.S.";
                strings[323] = "^5P.E.S. Активный.";
                strings[324] = "^5Пожалуйста, активируйте P.E.S. ([{+actionslot 3}])";
                strings[325] = "Ошибка загрузки mapedit для карты {0}: {1}";
                strings[326] = "Неизвестная запись MapEdit {0} ... игнорируется";
                strings[327] = "Все льготы награждены!";
                strings[328] = "Ваш банковский счет уже максимален!";
                strings[329] = "Идет кислотный дождь!";
                strings[330] = "Колючая проволока";
                strings[331] = "Разлив нефти";
                strings[332] = "Атака ядовитым газом";
                strings[333] = "Артиллерия";
                strings[334] = "Метель";
                strings[335] = "Извержение вулкана";
                strings[336] = "Штурмовые лодки";
                strings[337] = "Входящий ядовитый газ!";
                strings[338] = "Blizzard Inbound!";
                strings[339] = "Извержение вулкана неизбежно!";
                strings[440] = "Вулкан уже извергся.";
                break;
                */
                #endregion
                #region english
                case "english":
                default:
                    strings[0] = "Humans Defeated The Zombies!";
                    strings[1] = "Humans Survived!";
                    strings[2] = "Good Job Humans!";
                    strings[3] = "Humans Stayed Alive!";
                    strings[4] = "Human Face: :D!";
                    strings[5] = "Amazing! Humans Live On!";
                    strings[6] = "Great Job Humans!";
                    strings[7] = "Good Job, Get Ready For The Next Attack!";
                    strings[8] = "Zombies Are Such Perverts... Humans FTW!";
                    strings[9] = "Humans: 1, Zombies: 0";
                    strings[10] = "Humans Win Bitches!";
                    strings[11] = "Victory!!!";
                    strings[12] = "Enemy Down!!!";
                    strings[13] = "Easy Peasy!";
                    strings[14] = "You must be running AIZombies Supreme on Team Deathmatch!";
                    strings[15] = "The current max players for AIZombies can only be 8 or below. The current setting is {0}. It has been set to 8.";
                    strings[16] = "^1AIZombies Supreme Made by ^2Slvr99";
                    strings[17] = "^2Humans";
                    strings[18] = "^1Wait until the end of the round to spawn!";
                    strings[19] = "Survive {0} waves.";
                    strings[20] = "Get ready for the attack in:";
                    strings[21] = "Next Round In: ";
                    strings[22] = "^2Welcome {0}!\n^1AIZombies Supreme {3}\n^3Map: {1}\n^2Made By Slvr99\n^5Survive {2} Waves.";
                    strings[23] = "^1{0} ^1needs to be revived!";
                    strings[24] = "^1Failed to revive ";
                    strings[25] = "^1You have died. Wait until the next round to respawn.";
                    strings[26] = "^1{0} ^1has been killed.";
                    strings[27] = "Configuration file for AIZombies was not found! Creating one...";
                    strings[28] = "Max Health was set to an incorrect value in the cfg!, Set to default ({0})";
                    strings[29] = "Max Juggernog Health was set to an incorrect value in the cfg!, Set to default ({0})";
                    strings[30] = "Bot Health was set to an incorrect value in the cfg!, Set to default ({0})";
                    strings[31] = "Crawler Health was set to an incorrect value in the cfg!, Set to default ({0})";
                    strings[32] = "Boss Health was set to an incorrect value in the cfg!, Set to default ({0})";
                    strings[33] = "Bot Health Factor was set to an incorrect value in the cfg!, Set to default ({0})";
                    strings[34] = "Bot Damage was set to an incorrect value in the cfg!, Set to default ({0})";
                    strings[35] = "Perk Limit was set to an incorrect value in the cfg!, Set to default ({0})";
                    strings[36] = "Streets of Death";
                    strings[37] = "Stormy Outbreak";
                    strings[38] = "Rundown Village";
                    strings[39] = "Oil Refinery";
                    strings[40] = "Desert Outpost";
                    strings[41] = "Abandoned Subway";
                    strings[42] = "Construction Site Of Hell";
                    strings[43] = "Demolished Underpass";
                    strings[44] = "Abandoned Road Of Hell";
                    strings[45] = "Docked Death";
                    strings[46] = "Death Alley";
                    strings[47] = "Bone Appetite";
                    strings[48] = "Storage Unit Of Hell";
                    strings[49] = "Seaside Hotel Of Hell";
                    strings[50] = "Car Lot Of Pain";
                    strings[51] = "Big Black Death";
                    strings[52] = "El Brote";
                    strings[53] = "Wartorn";
                    strings[54] = "Under Construction";
                    strings[55] = "Death In Progress";
                    strings[56] = "Shipwrecked";
                    strings[57] = "Dead Aqueduct";
                    strings[58] = "Silla Cement";
                    strings[59] = "Oceanside Attack";
                    strings[60] = "Sanctuary Of Hell";
                    strings[61] = "Paradise";
                    strings[62] = "Rest Site Of Hell";
                    strings[63] = "Death Row";
                    strings[64] = "Pandemic Bridge";
                    strings[65] = "Undead Farm";
                    strings[66] = "Hideout of Hell";
                    strings[67] = "Shantytown";
                    strings[68] = "Oil Rig";
                    strings[69] = "Death Avenue";
                    strings[70] = "Trapped Canyon";
                    strings[71] = "The Pier";
                    strings[72] = "^1Unknown Map!";
                    strings[73] = " Please do not impersonate the developer.";
                    strings[74] = "Server gametype name search was cancelled for an unknown reason.";
                    strings[75] = "There was an error setting the server gametype name!: ";
                    strings[76] = "There was an error setting the server gametype name: No addresses found!";
                    strings[77] = "Gametype name search was cancelled for an unknown reason.";
                    strings[78] = "There was an error setting the gametype name!: ";
                    strings[79] = "There was an error setting the gametype name: No addresses found!";
                    strings[80] = "Press ^3[{+activate}] ^7to pick up the Sentry Gun";
                    strings[81] = "";
                    strings[82] = "Weapon patch search was cancelled for an unknown reason! This may cause bugs to occur for certain weapons.";
                    strings[83] = "There was an error finding weapon patch locations: ";
                    strings[84] = "Unable to set custom gametype name in server browser!";
                    strings[85] = "Could not find weapon data for {0}! Please report this error to Slvr99";
                    strings[86] = "Boardwalk";
                    strings[87] = "A beachside fair interrupted by the undead";
                    strings[88] = "Unable to set custom gametype name.";
                    strings[89] = "Unable to restore the custom gamemode string! This may result in a server crash when the map changes!";
                    strings[90] = "All third party scripts have been unloaded from this server due to a conflict with AIZombies.";
                    strings[91] = "There was an error contacting the update check server!: {0}";
                    strings[92] = "There was an error contacting the update check server!: No response from the server.";
                    strings[93] = "There is an update for AIZombies available! Downloading version {0} now...";
                    strings[94] = "There was an error downloading the update from the server!: {0}";
                    strings[95] = "There was an error replacing the old AIZombies file! Make sure the file is not read-only or is opened somewhere else.";
                    strings[96] = "Download completed! The updates will take effect once the current game ends.";
                    strings[97] = "Extra Cash!";
                    strings[98] = "Extra Bonus Points!";
                    strings[99] = "Random Perk!";
                    strings[100] = "No bot spawns available! Please have at least one \"zombiespawn\" in your map file.";
                    strings[101] = "^1No bot spawns available! Check console for details";
                    strings[102] = "More health";
                    strings[103] = "Sprint faster and longer";
                    strings[104] = "Reload faster";
                    strings[105] = "An extra weapon slot";
                    strings[106] = "Faster rate of fire";
                    strings[107] = "Faster moving while ADS";
                    strings[108] = "Automatically be revived shortly after going down\n+ Revive your team faster";
                    strings[109] = "Scavenge Free Ammo";
                    strings[110] = "Lockdown";
                    strings[111] = "Bootleg";
                    strings[112] = "Mission";
                    strings[113] = "Carbon";
                    strings[114] = "Dome";
                    strings[115] = "Downturn";
                    strings[116] = "Hardhat";
                    strings[117] = "Interchange";
                    strings[118] = "Fallen";
                    strings[119] = "Bakaara";
                    strings[120] = "Resistance";
                    strings[121] = "Arkaden";
                    strings[122] = "Outpost";
                    strings[123] = "Seatown";
                    strings[124] = "Underground";
                    strings[125] = "Village";
                    strings[126] = "Piazza";
                    strings[127] = "Liberation";
                    strings[128] = "Black Box";
                    strings[129] = "Overwatch";
                    strings[130] = "Aground";
                    strings[131] = "Erosion";
                    strings[132] = "Foundation";
                    strings[133] = "Getaway";
                    strings[134] = "Sanctuary";
                    strings[135] = "Oasis";
                    strings[136] = "Lookout";
                    strings[137] = "Terminal";
                    strings[138] = "Intersection";
                    strings[139] = "Vortex";
                    strings[140] = "U-Turn";
                    strings[141] = "Decommission";
                    strings[142] = "Offshore";
                    strings[143] = "Parish";
                    strings[144] = "Gulch";
                    strings[145] = "Once peaceful streets overrun by undead";
                    strings[146] = "Stormy town taken over by the undead";
                    strings[147] = "A rundown village taken over\nby the undead";
                    strings[148] = "A once lively oil refinery";
                    strings[149] = "An abandoned outpost in the desert";
                    strings[150] = "The aftermath of a dangerous mistake";
                    strings[151] = "A small construction site unfinished";
                    strings[152] = "A destroyed freeway\nresulting from an outbreak";
                    strings[153] = "An abandoned russian ghost town";
                    strings[154] = "An african city hit by the undead";
                    strings[155] = "Parisian district not safe anymore";
                    strings[156] = "German mall robbed by the undead";
                    strings[157] = "Siberian outpost evacuated\ndue to the outbreak";
                    strings[158] = "A seaside town not safe\nfrom the undead";
                    strings[159] = "Subway station used as an\nevacuation route";
                    strings[160] = "African village ridden\nwith a dangerous disease";
                    strings[161] = "A spanish town infected by muertos";
                    strings[162] = "A military base built in a park\nfor protection against the undead";
                    strings[163] = "Crash site of an airborne\nvirus attack";
                    strings[164] = "A skyscraper abandoned due\nto the outbreak";
                    strings[165] = "Shipwreck caused by a virus at sea";
                    strings[166] = "Aqueduct contaminated by the undead";
                    strings[167] = "Cement factory overtaken by the undead";
                    strings[168] = "Vacation spot that turned\ninto a nightmare";
                    strings[169] = "A sanctuary that was broken from\nit's purity by the undead";
                    strings[170] = "An oasis that got overrun by undead";
                    strings[171] = "A rest repo that's useful for\nanything other than rest";
                    strings[172] = "A russian airport overtaken by the undead";
                    strings[173] = "A freeway in the centre on the attacks";
                    strings[174] = "Abandoned farm hit by\nmore than one problem";
                    strings[175] = "Desert freeway unsafe from the undead";
                    strings[176] = "A shipwreck near a town\nthat has been infected";
                    strings[177] = "An oil rig overrun by\nmore than juggernauts";
                    strings[178] = "A fun town that was\novertaken by undead";
                    strings[179] = "Desert gulch thriving with undead";
                    strings[180] = "Money: $";
                    strings[181] = "Bonus Points:";
                    strings[182] = "Zombies: ";
                    strings[183] = "Power is not activated";
                    strings[184] = "Boss Wave Survived!\n^320 Second Intermission";
                    strings[185] = "Completed Boss Wave!";
                    strings[186] = "Crawler Wave Survived!\n^320 Second Intermission";
                    strings[187] = "Max Ammo Awarded!";
                    strings[188] = "Wave ";
                    strings[189] = " Survived!\n^320 Second Intermission";
                    strings[190] = "Power Activated By";
                    strings[191] = "Power has been temporarily activated for {0} seconds";
                    strings[192] = "Power is not activated";
                    strings[193] = "Zombies ate the humans";
                    strings[194] = "Victory!";
                    strings[195] = "Defeat!";
                    strings[196] = "Win";
                    strings[197] = "Lose";
                    strings[198] = "Humans Survived for";
                    strings[199] = " Minutes";
                    strings[200] = " Seconds";
                    strings[201] = "Waves Survived: ";
                    strings[202] = "We Won This Fight!";
                    strings[203] = "Zombies Won This Fight";
                    strings[204] = "Death Machine!";
                    strings[205] = "Insta-Kill!";
                    strings[206] = "Double Points!";
                    strings[207] = "Max Ammo!";
                    strings[208] = "Nuke!";
                    strings[209] = "Fire Sale!";
                    strings[210] = "Freezer!";
                    strings[211] = "^5Random";
                    strings[212] = "Vote using [{+actionslot 4}], [{+actionslot 5}], and [{+actionslot 6}] for the respective maps!";
                    strings[213] = "Vote for the next map!";
                    strings[214] = "Next Round In: {0}";
                    strings[215] = "Map Killstreak";
                    strings[216] = "1000 Kill Streak!";
                    strings[217] = "Permanent Bot Achieved!";
                    strings[218] = "^3{0} ^7ready for usage!";
                    strings[219] = "Too many bots deployed.";
                    strings[220] = "Airspace too crowded.";
                    strings[221] = "Cannot call in Heli Sniper while teleporting";
                    strings[222] = "Cannot call in Heli Sniper here";
                    strings[223] = " not available.";
                    strings[224] = "Nuke already inbound!";
                    strings[225] = "Nuke Incoming In: {0}";
                    strings[226] = "Sub Team";
                    strings[227] = "LMG Team";
                    strings[228] = "Press ^3[{vote no}] ^7to re-route the drone";
                    strings[229] = "M.O.A.B.";
                    strings[230] = "Tank Barrage";
                    strings[231] = "Defcon Trigger System";
                    strings[232] = "A-10 Support";
                    strings[233] = "Flamethrower";
                    strings[234] = "Acid Rain";
                    strings[235] = "Drivable Humvee";
                    strings[236] = "Defcon is at level {0}";
                    strings[237] = "A waypoint had no visible links! Deleting waypoint...";
                    strings[238] = " Gifted $500 to {0}!";
                    strings[239] = " Recieved $500 from {0}!";
                    strings[240] = "^1Not enough money for a Random Weapon. Need ^2$10";
                    strings[241] = "^1Not enough money for a Random Weapon. Need ^2$950";
                    strings[242] = "Random Weapon!";
                    strings[243] = "Weapon Upgraded!";
                    strings[244] = "^1Gambler is already in use!";
                    strings[245] = "^1You may only use the gambler once per round!";
                    strings[246] = "Gambler!";
                    strings[247] = "^1You've won {0}.";
                    strings[248] = "^2Your results will display in 10 seconds.";
                    strings[249] = "^2You've won {0}!";
                    strings[250] = "^1You've lost {0}!";
                    strings[251] = "^2Double Health for 30 seconds!";
                    strings[252] = "Double Health over!";
                    strings[253] = "^2Infinite Health for 30 seconds!";
                    strings[254] = "Infinite Health over!";
                    strings[255] = "^2You have 1/2 chance for Max Ammo!";
                    strings[256] = "^2You've won the Max Ammo!";
                    strings[257] = "^1No Max Ammo.";
                    strings[258] = "^1God decides if you live or die in 5 seconds";
                    strings[259] = "^2You live.";
                    strings[260] = "^1Not enough money for Ammo. Need ^2$";
                    strings[261] = "Ammo!";
                    strings[262] = "Random Killstreak!";
                    strings[263] = "Current Balance: ${0}";
                    strings[264] = "Bank Withdraw";
                    strings[265] = "Elevator!";
                    strings[266] = "Juggernog";
                    strings[267] = "Stamin-Up";
                    strings[268] = "Speed Cola";
                    strings[269] = "Mule Kick";
                    strings[270] = "Double Tap";
                    strings[271] = "Stalker Soda";
                    strings[272] = "Quick Revive Pro";
                    strings[273] = "Scavenge-Aid";
                    strings[274] = "Power!";
                    strings[275] = "Power has been activated";
                    strings[276] = "Opened Door!";
                    strings[277] = "Press ^3[{+activate}] ^7to gift $^2500 ^7to ";
                    strings[278] = "Press ^3{1} ^7to open the door [Cost: {0}]";
                    strings[279] = "Press ^3[{+activate}] ^7to trade Weapons: ";
                    strings[280] = "Press ^3[{+activate}] ^7for a Random Weapon [Cost: 10]";
                    strings[281] = "Press ^3[{+activate}] ^7for a Random Weapon [Cost: 950]";
                    strings[282] = "^1Power must be activated!";
                    strings[283] = "Press ^3[{+activate}] ^7to take ^2your new upgraded weapon";
                    strings[284] = "Press ^3[{+activate}] ^7to upgrade your ^1Current Weapon ^7[Cost: 5000]";
                    strings[285] = "Press ^3[{+activate}] ^7to use the Gambler [Cost: 1000]";
                    strings[286] = "Press ^3[{+activate}] ^7to buy a random killstreak [Cost: 200 ^5Bonus Points^7]";
                    strings[287] = "The teleporter is cooling down.";
                    strings[288] = "^1You must link the teleporter first!";
                    strings[289] = "Press ^3[{+activate}] ^7to teleport";
                    strings[290] = "Press ^3[{+activate}] ^7to link the teleporter";
                    strings[291] = "Press ^3[{+activate}] ^7to use the elevator [Cost: 500]";
                    strings[292] = "Press ^3[{+activate}] ^7to withdraw from the ATM [Amount: 1000] [Cost: 100]\n\n                  Press ^3[{vote yes}] ^7to deposit to the ATM [Cost: 1000]";
                    strings[293] = "You already have {0}!";
                    strings[294] = "Press ^3{2} ^7to buy {0} [Cost: {1}]";
                    strings[295] = "You may only carry {0} perks!";
                    strings[296] = "Press ^3[{+activate}] ^7to activate Power [Cost: 10000]";
                    strings[297] = "Hold ^3[{+activate}] ^7for Explosive Ammo";
                    strings[298] = "Hold ^3[{+activate}] ^7to board the Heli";
                    strings[299] = " is already being revived!";
                    strings[300] = "Hold ^3[{+activate}] ^7to revive ";
                    strings[301] = "Ammo";
                    strings[302] = "Missile";
                    strings[303] = "Sentry Gun";
                    strings[304] = "Vision Restorer";
                    strings[305] = "Power Surge";
                    strings[306] = "Deployable Explosive Ammo";
                    strings[307] = "M.O.A.B.";
                    strings[308] = "Littlebird Drone";
                    strings[309] = "Heli Sniper";
                    strings[310] = "Grenade Launcher Turret";
                    strings[311] = "Double Points";
                    strings[312] = "Insta-Kill";
                    strings[313] = "Nuke";
                    strings[314] = "Death Machine";
                    strings[315] = "Airstrike";
                    strings[316] = "Hold ^3[{+activate}] ^7for ";
                    strings[317] = "Weapon";
                    strings[318] = "Press ^3[{+activate}] ^7to wash away your sins.";
                    strings[319] = "Press ^3{2} ^7for {0} ^7[Cost: {1}]";
                    strings[320] = "Press ^3[{+activate}] ^7for ";
                    strings[321] = "Press ^3{1} ^7for Ammo [Cost: {0}]";
                    strings[322] = "Press ^3[{+actionslot 3}] ^7to equip P.E.S.";
                    strings[323] = "^5P.E.S. Active.";
                    strings[324] = "^5Please activate P.E.S. ([{+actionslot 3}])";
                    strings[325] = "Error loading mapedit for map {0}: {1}";
                    strings[326] = "Unknown MapEdit Entry {0}... ignoring";
                    strings[327] = "All Perks Awarded!";
                    strings[328] = "Your bank balance is already at max!";
                    strings[329] = "Acid Rain Incoming!";
                    strings[330] = "Barbed Wire";
                    strings[331] = "Oil Spill";
                    strings[332] = "Poison Gas Attack";
                    strings[333] = "Artillery";
                    strings[334] = "Blizzard";
                    strings[335] = "Volcanic Eruption";
                    strings[336] = "Assault Boats";
                    strings[337] = "Poison Gas Inbound!";
                    strings[338] = "Blizzard Inbound!";
                    strings[339] = "Volcano Eruption Imminent!";
                    strings[340] = "The volcano has already erupted.";
                    break;
                    #endregion
            }

            return strings;
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
                Utilities.PrintToConsole(string.Format(gameStrings[91], e.Message));
            }
        }
        private static void recieveVersion(IAsyncResult version, HttpWebRequest site)
        {
            if (!site.HaveResponse)
            {
                Utilities.PrintToConsole(string.Format(gameStrings[92]));
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
                Utilities.PrintToConsole(string.Format(gameStrings[93], netVersion));
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
                Utilities.PrintToConsole(string.Format(gameStrings[94], e.Message));
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
                Utilities.PrintToConsole(gameStrings[95]);
                if (File.Exists("scripts\\AIZombies_update.dll")) File.Delete("scripts\\AIZombies_update.dll");
                return;
            }
            Utilities.PrintToConsole(gameStrings[96]);
        }
#endregion
    }
}
