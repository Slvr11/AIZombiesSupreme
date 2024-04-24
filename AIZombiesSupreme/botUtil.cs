using System;
using System.Collections.Generic;
using System.Collections;
using InfinityScript;
using static InfinityScript.GSCFunctions;

namespace AIZombiesSupreme
{
    public class botUtil : BaseScript
    {
        public static Action onBotUpdate;

        public static List<Entity> botsInPlay = new List<Entity>();
        public static List<Entity> botPool = new List<Entity>();
        public static List<Entity> crawlerPool = new List<Entity>();
        public static List<Entity> bossPool = new List<Entity>();
        public static List<Entity> glowsticks = new List<Entity>();

        public static uint botsForWave = 0;
        public static int health = 100;
        public static int crawlerHealth = 110;
        public static int bossHealth = 2500;
        public static int healthScalar = 2;
        public static int dmg = 50;
        public static uint spawnedBots = 0;
        public static bool useAltHeads = false;
        public static bool useAltBodies = false;
        public static bool useAlternatingThread = false;

        public static List<Vector3> botSpawns = new List<Vector3>();
        public static List<Vector3> spawnAngles = new List<Vector3>();

        public static readonly string anim_lose = "pb_chicken_dance_crouch";
        public static readonly string anim_idle = "pb_stand_alert_pistol";
        public static readonly string anim_run = "pb_sprint_akimbo";
        public static readonly string anim_walk = "pb_stand_shoot_walk_forward_unarmed";
        public static readonly string anim_runHurt = "pb_stumble_pistol_forward";
        public static readonly string anim_walkHurt = "pb_stumble_pistol_walk_forward";
        public static readonly string[] anim_deaths = { "pb_stand_death_frontspin", "pb_stand_death_shoulderback" };//"pb_shotgun_death_legs", "pb_stand_death_leg_kickup", "pb_stand_death_tumbleback", "pb_stand_death_head_collapse", "pb_stand_death_nervedeath", "pb_stand_death_leg", "pb_stand_death_chest_spin" , "pb_stand_death_legs", "pb_stand_death_chest_blowback", "pb_stand_death_lowerback"
        public static readonly string anim_attack = "pt_melee_right2right_2";
        //public static readonly string[] anim_death_explode = { "pb_explosion_death_B1"};//"pb_stand_death_kickup", "pb_explosion_death_B3", "pb_explosion_death_B2" 
        public static readonly string anim_death_nuke = "pb_stand_death_neckdeath";
        public static readonly string crawlerAnim_idle = "pb_prone_hold";
        public static readonly string crawlerAnim_death = "pb_prone_death_quickdeath";
        public static readonly string crawlerAnim_attack = "pb_dive_front_impact";
        public static readonly string crawlerAnim_walk = "pb_prone_crawl_akimbo";
        public static readonly string anim_cripple = "pb_crouch2prone";

        public static readonly string botModel = AIZ.getBotModelsForLevel(false);
        public static readonly string botHeadModel = AIZ.getBotModelsForLevel(true);
        public static readonly string botCrawlerModel = AIZ.getCrawlerModelForLevel();

        public static uint instaKillTime = 0;
        public static uint doublePointsTime = 0;
        private static bool instaKillTimerStarted = false;
        private static bool doublePointsTimerStarted = false;
        public static bool freezerActivated = false;
        public static bool perkDropsEnabled = true;
        public static byte nukeOffsetScalar = 1;

        public static bool startBotSpawn()
        {
                if (AIZ.gameEnded) return false;

                if (botsInPlay.Count >= 25)
                    return true;
                else if (spawnedBots == botsForWave)
                    return false;
                else
                {
                    int randomSpawn = AIZ.rng.Next(botSpawns.Count);
                    if (roundSystem.isBossWave) return bots.spawnBossBot(randomSpawn);
                    else if (roundSystem.isCrawlerWave) return bots.spawnBot(randomSpawn, true);
                    else return bots.spawnBot(randomSpawn, false);
                }
        }

        public static void killBotIfUnderMap(Entity bot)
        {
            if (!bot.HasField("isAlive")) return;
            if (bot.GetField<bool>("isAlive") && bot.Origin.Z < AIZ.mapHeight)
            {
                if (bot.HasField("isBoss"))
                    StartAsync(killBotOnNuke(bot, false, true));
                else if (!bot.HasField("head"))
                    StartAsync(killBotOnNuke(bot, true, false));
                else
                    StartAsync(killBotOnNuke(bot, false, false));
            }
        }
        //To kill off a bot but respawn them right after
        public static void killBotAndRespawn(Entity bot)
        {
            bot.SetField("isAlive", false);
            if (bot.HasField("isOnCompass") && bot.GetField<bool>("isOnCompass"))
            {
                mapEdit.removeObjID(bot);
                bot.SetField("isOnCompass", false);
            }
            Entity hitbox = bot.GetField<Entity>("hitbox");
            hitbox.SetCanDamage(false);
            hitbox.SetCanRadiusDamage(false);
            hitbox.SetModel("tag_origin");//Change model to avoid the dead bot's hitbox blocking shots
            bot.MoveTo(bot.Origin, 0.05f);

            bool isCrawler = !bot.HasField("head");
            bool isBoss = bot.HasField("isBoss");

            if (isCrawler || bot.HasField("hasBeenCrippled"))
                playAnimOnBot(bot, crawlerAnim_death);
            else
                playAnimOnBot(bot, anim_death_nuke);

            if (bot.HasField("hasBeenCrippled")) bot.ClearField("hasBeenCrippled");

            if (isCrawler) AfterDelay(500, () => bot.MoveTo(bot.Origin + new Vector3(0, 0, 2500), 5));
            AfterDelay(5000, () => despawnBot(bot, isCrawler, isBoss));
            botsInPlay.Remove(bot);
            onBotUpdate();

            if (botsForWave == spawnedBots)
            {
                spawnedBots--;
                OnInterval(500, startBotSpawn);//Restart spawns with less spawnedBots if at the end of the round. This will spawn a new bot correctly. Else, the loop will be running and automatically respawn a bot.
            }
            else spawnedBots--;

            AfterDelay(1500, () => roundSystem.checkForCompass());//Re-apply compass if needed
        }

        private static void despawnBot(Entity bot, bool isCrawler, bool isBoss)
        {
            bot.Hide();
            if (bot.HasField("head"))
            {
                Entity botHead = bot.GetField<Entity>("head");
                botHead.Hide();
            }
            bot.Origin = Vector3.Zero;
            if (isCrawler) crawlerPool.Add(bot);
            else if (isBoss) bossPool.Add(bot);
            else botPool.Add(bot);
        }

        public static void updateBotLastActiveTime(Entity bot)
        {
            int time = GetTime();
            bot.SetField("lastActiveTime", time);
        }

        public static void spawnBot(bool isCrawler)
        {
            Entity bot = Spawn("script_model", Vector3.Zero);
            bot.Angles = Vector3.Zero;
            bot.EnableLinkTo();
            if (isCrawler)
            {
                bot.SetModel(botCrawlerModel);
                playAnimOnBot(bot, crawlerAnim_idle);
            }
            else
            {
                if (useAltBodies)
                    bot.SetModel("defaultactor");
                else bot.SetModel(botModel);
                Entity bothead = Spawn("script_model", bot.Origin);
                if (useAltHeads)
                {
                    switch (AIZ._mapname)
                    {
                        case "mp_bootleg":
                        case "mp_seatown":
                        case "mp_shipbreaker":
                        case "mp_six_ss":
                            bothead.SetModel("chicken");
                            break;
                        case "mp_overwatch":
                            bothead.SetModel("chicken_black_white");
                            break;
                        default:
                            bothead.SetModel(botHeadModel);
                            break;
                    }
                    bothead.LinkTo(bot, "j_spine4", Vector3.Zero, new Vector3(0, 70, 90));
                }
                else
                {
                    bothead.SetModel(botHeadModel);
                    bothead.LinkTo(bot, "j_spine4", Vector3.Zero, Vector3.Zero);
                }
                bot.SetField("head", bothead);
                bothead.Hide();
                playAnimOnBot(bot, anim_idle);
            }
            bot.SetField("isAlive", false);
            bot.Hide();

            if (isCrawler) crawlerPool.Add(bot);
            else botPool.Add(bot);

            Entity botHitbox = Spawn("script_model", bot.Origin + new Vector3(0, 0, 30));
            botHitbox.SetModel("com_plasticcase_dummy");
            botHitbox.Angles = Vector3.Zero;
            botHitbox.Hide();
            botHitbox.SetCanDamage(false);
            botHitbox.SetCanRadiusDamage(false);
            if (isCrawler) botHitbox.SetField("currentHealth", crawlerHealth);
            else botHitbox.SetField("currentHealth", health);
            botHitbox.SetField("damageTaken", 0);
            int xAngleOffset = isCrawler ? 90 : 0;
            int xOffset = isCrawler ? -30 : 0;
            botHitbox.LinkTo(bot, "j_mainroot", new Vector3(xOffset, 0, 0), new Vector3(0, xAngleOffset, -90));
            botHitbox.SetField("parent", bot);
            bot.SetField("hitbox_linkOffset_y", 0);

            if (!isCrawler)
            {
                Vector3 headOrigin = bot.GetTagOrigin("j_head");
                //Vector3 headAngles = bot.GetTagAngles("j_head");
                Entity headHitbox = Spawn("script_model", headOrigin);
                headHitbox.SetModel("ims_scorpion_explosive1");
                headHitbox.Angles = Vector3.Zero;
                headHitbox.Hide();
                headHitbox.SetCanDamage(true);
                headHitbox.SetCanRadiusDamage(false);
                headHitbox.LinkTo(bot, "j_head", Vector3.Zero, Vector3.Zero);
                headHitbox.SetField("parent", bot);
                bot.SetField("headHitbox", headHitbox);

                headHitbox.OnNotify("damage", (ent, damage, attacker, direction_vec, point, meansOfDeath, modelName, partName, tagName, iDFlags, weapon) => onBotDamage(botHitbox, damage, attacker, direction_vec, point, "MOD_HEADSHOT", modelName, partName, tagName, iDFlags, weapon, isCrawler, false));
            }

            bot.SetField("hitbox", botHitbox);
            bot.SetField("state", "idle");
            bot.SetField("isAttacking", false);
            bot.SetField("currentWaypoint", bot);
            bot.SetField("isOnCompass", false);
            bot.SetField("primedForNuke", false);
            botHitbox.SetField("canBeDamaged", true);

            botHitbox.OnNotify("damage", (ent, damage, attacker, direction_vec, point, meansOfDeath, modelName, partName, tagName, iDFlags, weapon) => onBotDamage(botHitbox, damage, attacker, direction_vec, point, meansOfDeath, modelName, partName, tagName, iDFlags, weapon, isCrawler, false));
            bonusDrops.onNuke += () => StartAsync(killBotOnNuke(bot, isCrawler, false));
        }
        public static bool spawnBot_boss()
        {
            Entity bot = Spawn("script_model", Vector3.Zero);
            bot.Angles = Vector3.Zero;
            bot.EnableLinkTo();
            bot.SetModel("mp_fullbody_opforce_juggernaut");
            playAnimOnBot(bot, anim_idle);
            bot.SetField("isAlive", false);
            bot.Hide();
            bossPool.Add(bot);

            Entity botHitbox = Spawn("script_model", bot.Origin + new Vector3(0, 0, 30));
            botHitbox.SetModel("com_plasticcase_dummy");
            botHitbox.Angles = new Vector3(90, bot.Angles.Y, 0);
            botHitbox.SetCanDamage(false);
            botHitbox.SetCanRadiusDamage(false);
            botHitbox.LinkTo(bot, "j_mainroot", Vector3.Zero, Vector3.Zero);
            botHitbox.Hide();
            botHitbox.SetField("currentHealth", bossHealth);
            botHitbox.SetField("damageTaken", 0);
            botHitbox.SetField("parent", bot);
            botHitbox.SetField("isBoss", true);
            bot.SetField("isBoss", true);
            bot.SetField("hitbox", botHitbox);
            bot.SetField("hitbox_linkOffset_y", 0);
            bot.SetField("state", "idle");
            bot.SetField("isAttacking", false);
            bot.SetField("currentWaypoint", bot);
            bot.SetField("isOnCompass", false);
            bot.SetField("primedForNuke", false);
            bot.SetField("lastActiveTime", 0);//Bosses won't die from time
            botHitbox.SetField("canBeDamaged", true);

            bonusDrops.onNuke += () => StartAsync(killBotOnNuke(bot, false, true));
            botHitbox.OnNotify("damage", (entity, damage, attacker, direction_vec, point, meansOfDeath, modelName, partName, tagName, iDFlags, weapon) => onBotDamage(entity, damage, attacker, direction_vec, point, meansOfDeath, modelName, partName, tagName, iDFlags, weapon, false, true));

            return true;
        }

        public static void onBotDamage(Entity hitbox, Parameter damage, Parameter attacker, Parameter direction_vec, Parameter point, Parameter meansOfDeath, Parameter modelName, Parameter partName, Parameter tagName, Parameter iDFlags, Parameter weapon, bool isCrawler, bool isBoss)
        {
            if ((string)weapon == "iw5_usp45_mp_akimbo_silencer02" || AIZ.isRayGun((string)weapon)) return;

            Entity currentBot = hitbox.GetField<Entity>("parent");
            if (!botsInPlay.Contains(currentBot)) return;
            Entity player = (Entity)attacker;

            if ((string)weapon == "remote_uav_weapon_mp" && attacker.As<Entity>().HasField("owner"))//UAV tweaks
            {
                player = attacker.As<Entity>().GetField<Entity>("owner");
                meansOfDeath = "MOD_PASSTHRU";
                damage = 50;
            }
            else if ((string)weapon == "sentry_minigun_mp" && attacker.As<Entity>().HasField("owner"))//Sentry tweaks
            {
                player = attacker.As<Entity>().GetField<Entity>("owner");
                meansOfDeath = "MOD_PASSTHRU";
                damage = 10;
            }
            else if ((string)weapon == "manned_gl_turret_mp" && attacker.As<Entity>().HasField("owner"))//Sentry tweaks
            {
                player = attacker.As<Entity>().GetField<Entity>("owner");
                meansOfDeath = "MOD_PASSTHRU";
                damage = 300;
            }
            else if (((string)weapon == "remote_tank_projectile_mp" || AIZ.isThunderGun((string)weapon)) && attacker.As<Entity>().HasField("owner"))
            {
                player = attacker.As<Entity>().GetField<Entity>("owner");
                meansOfDeath = "MOD_PASSTHRU";
            }
            else if ((string)weapon == "ac130_25mm_mp")//A10 tweaks
            {
                //player = attacker.As<Entity>().GetField<Entity>("owner");
                meansOfDeath = "MOD_PASSTHRU";
            }
            else if (attacker.As<Entity>().HasField("owner"))//Killstreak bot weapons
            {
                player = attacker.As<Entity>().GetField<Entity>("owner");
                meansOfDeath = "MOD_PASSTHRU";
                if (AIZ.isHellMap) damage = 15;//Hellmap damage
                else damage = 35 / (1 + ((int)roundSystem.Wave / 2));//Base damage
            }

            if ((string)meansOfDeath == "MOD_BLEEDOUT")
            {
                Vector3 org = currentBot.GetTagOrigin("j_head");
                PlayFX(AIZ.fx_headshotBlood, org);
                doBotDamage((int)damage, player, (string)weapon, hitbox, (string)meansOfDeath, point.As<Vector3>(), true);
            }
            else
            {
                if ((string)weapon != "sentry_minigun_mp" && (string)weapon != "manned_gl_turret_mp" && (string)weapon != "remote_uav_weapon_mp" && (string)meansOfDeath != "MOD_EXPLOSIVE_BULLET") PlayFX(AIZ.fx_blood, point.As<Vector3>());//Only play FX if the weapon isn't a script weapon
                doBotDamage((int)damage, player, (string)weapon, hitbox, (string)meansOfDeath, point.As<Vector3>());
            }
            string botState = currentBot.GetField<string>("state");
            if (botState != "hurt" && botState != "attacking" && (string)meansOfDeath != "MOD_BLEEDOUT")
            {
                if (!isCrawler && !isBoss)
                {
                    playAnimOnBot(currentBot, getHurtAnim(hitbox));
                }
                else if (isBoss)
                {
                    playAnimOnBot(currentBot, anim_runHurt);
                }
                currentBot.SetField("state", "hurt");
                AfterDelay(500, () =>
                    currentBot.SetField("state", "post_hurt"));
            }

            updateBotLastActiveTime(currentBot);

            if (hitbox.GetField<int>("damageTaken") >= hitbox.GetField<int>("currentHealth"))
            {
                currentBot.SetField("isAlive", false);
                if (currentBot.HasField("isOnCompass") && currentBot.GetField<bool>("isOnCompass"))
                {
                    /*
                    Objective_Delete(currentBot.GetField<int>("compassID"));
                    h_mapEdit._objIDList[currentBot.GetField<int>("compassID")] = false;
                    h_mapEdit._objIDs.Remove(currentBot);
                    currentBot.ClearField("compassID");
                    */
                    mapEdit.removeObjID(currentBot);
                    currentBot.SetField("isOnCompass", false);
                }
                hitbox.SetCanDamage(false);
                hitbox.SetCanRadiusDamage(false);
                hitbox.SetModel("tag_origin");//Change model to avoid the dead bot's hitbox blocking shots
                //if (isBoss) hitbox.Delete();
                if (AIZ.isPlayer(player))
                {
                    if (currentBot.HasField("primedForNuke") && !currentBot.GetField<bool>("primedForNuke"))
                    {
                        int pointGain = 50;
                        if ((string)meansOfDeath == "MOD_HEADSHOT") pointGain = 100;
                        if ((string)meansOfDeath == "MOD_MELEE") pointGain = 130;

                        if (doublePointsTime > 0) pointGain *= 2;

                        if ((string)meansOfDeath != "MOD_PASSTHRU")
                        {
                            player.SetField("cash", player.GetField<int>("cash") + pointGain);
                            hud.scorePopup(player, pointGain);
                        }
                        AIZ.addRank(player, pointGain);
                    }
                    player.Kills++;
                    if (player.HasField("aizHud_created"))
                    {
                        player.SetField("points", player.GetField<int>("points") + 1);
                        HudElem pointNumber = player.GetField<HudElem>("hud_pointNumber");
                        pointNumber.SetValue(player.GetField<int>("points"));
                    }
                    killstreaks.checkKillstreak(player);
                }
                currentBot.MoveTo(currentBot.Origin, 0.05f);

                if (isCrawler || currentBot.HasField("hasBeenCrippled")) playAnimOnBot(currentBot, crawlerAnim_death);
                else
                {
                    //Log.Write(LogLevel.All, (string)meansOfDeath);
                    /*
                    if ((string)meansOfDeath == "MOD_EXPLOSIVE" || (string)meansOfDeath == "MOD_GRENADE_SPLASH")
                    {
                        currentBot.Angles = VectorToAngles(point.As<Vector3>() - currentBot.Origin);
                        int randomAnim = g_AIZ.rng.Next(anim_death_explode.Length);
                        currentplayAnimOnBot(bot, anim_death_explode[randomAnim]);
                    }
                    else
                    */
                    //{
                        int randomAnim = AIZ.rng.Next(anim_deaths.Length);
                        playAnimOnBot(currentBot, anim_deaths[randomAnim]);
                    //}
                }

                if (currentBot.HasField("hasBeenCrippled")) currentBot.ClearField("hasBeenCrippled");

                if (isCrawler) AfterDelay(500, () => currentBot.MoveTo(currentBot.Origin + new Vector3(0, 0, 2500), 5));

                AfterDelay(5000, () => despawnBot(currentBot, isCrawler, isBoss));
                botsInPlay.Remove(currentBot);
                onBotUpdate();
                roundSystem.checkForEndRound();
                if (isCrawler && roundSystem.isCrawlerWave && (botsInPlay.Count == 0 && botsForWave == spawnedBots) && perkDropsEnabled && AIZ.isHellMap) { bonusDrops.spawnBonusDrop(bonusDrops.dropTypes.perk, currentBot.Origin); return; }
                if (!isBoss)
                {
                    bonusDrops.dropTypes bonusType = bonusDrops.checkForBonusDrop();
                    if (bonusType != bonusDrops.dropTypes.none)
                        bonusDrops.spawnBonusDrop(bonusType, currentBot.Origin);
                }
            }
        }

        public static IEnumerator killBotOnNuke(Entity bot, bool isCrawler, bool boss)
        {
            if (!bot.HasField("isAlive")) yield break;

            bot.SetField("primedForNuke", true);

            yield return Wait(RandomFloatRange(0, 4) * nukeOffsetScalar);

            bot.SetField("primedForNuke", false);
            if (!bot.GetField<bool>("isAlive") || !botsInPlay.Contains(bot)) yield break;
            bot.SetField("isAlive", false);
            Entity botHitbox = bot.GetField<Entity>("hitbox");
            if (bot.GetField<bool>("isOnCompass"))
            {
                mapEdit.removeObjID(bot);
                bot.SetField("isOnCompass", false);
            }
            botHitbox.SetCanDamage(false);
            botHitbox.SetCanRadiusDamage(false);
            bot.MoveTo(bot.Origin, 0.05f);
            string deathAnim;
            if (isCrawler || bot.HasField("hasBeenCrippled")) deathAnim = crawlerAnim_death;
            else deathAnim = anim_death_nuke;
            playAnimOnBot(bot, deathAnim);

            if (!isCrawler && !boss)
            {
                Entity head = bot.GetField<Entity>("head");
                head.Hide();
                bot.GetField<Entity>("headHitbox").SetCanDamage(false);
                if (nukeOffsetScalar != 0) PlayFX(AIZ.fx_headshotBlood, head.Origin);
                head.PlaySound("melee_knife_hit_watermelon");

                if (bot.HasField("hasBeenCrippled")) bot.ClearField("hasBeenCrippled");
            }

            if (isCrawler) AfterDelay(500, () => bot.MoveTo(bot.Origin + new Vector3(0, 0, 2500), 5));
            //else if (boss) AfterDelay(1000, () => bot.StartRagdoll());
            //if (!boss)
            botsInPlay.Remove(bot);
            onBotUpdate();
            if (isCrawler && roundSystem.isCrawlerWave && (botsInPlay.Count == 0 && botsForWave == spawnedBots) && perkDropsEnabled && AIZ.isHellMap) bonusDrops.spawnBonusDrop(bonusDrops.dropTypes.perk, bot.Origin);
            roundSystem.checkForEndRound();

            yield return Wait(5);
            despawnBot(bot, isCrawler, boss);
        }

        private static void doBotDamage(int damage, Entity player, string weapon, Entity botHitbox, string MOD, Vector3 point, bool skipFeedback = false)
        {
            if (!botHitbox.GetField<bool>("canBeDamaged"))
            {
                return;
            }

            int hitDamage;
            if (AIZ.weaponIsUpgrade(weapon)) hitDamage = damage / 2;//Base upgraded damage
            else if (AIZ.isHellMap) hitDamage = damage / 2;//Hellmap damage
            else hitDamage = damage / (1 + ((int)roundSystem.Wave / 2));//Base damage

            if (MOD == "MOD_MELEE") hitDamage = damage / (((int)roundSystem.Wave + 1) / 2);//Melee damage
            if (weapon == "iw5_p99_mp_tactical_xmags" && MOD == "MOD_MELEE") hitDamage = 350;//P99 Upgraded damage
            if (weapon == "iw5_riotshield_mp") hitDamage = damage*2;//Upgraded shield
            if (AIZ.isWeaponDeathMachine(weapon)) hitDamage = damage * 4;

            if (MOD != "MOD_MELEE")
            {
                if (weapon.Contains("iw5_deserteagle_mp") || weapon == "at4_mp" || weapon.Contains("iw5_44magnum_mp") || weapon.StartsWith("iw5_mp412")) hitDamage = damage;//Specials damage
                //Weapon tweaks
                if (AIZ.isSniper(weapon) || weapon.Contains("iw5_dragunov_mp")) hitDamage = (damage *= 2);//Sniper damage
                if (AIZ.isShotgun(weapon))
                {
                    hitDamage = (int)(hitDamage * 14f);//Shotgun multiplier
                    StartAsync(setBotImmunity(botHitbox));
                }

                if (weapon == "gl_mp") hitDamage = 10000;//GL
                else if (weapon == "iw5_xm25_mp") hitDamage = damage;
                else if (weapon == "xm25_mp") hitDamage = damage * 2;
                else if (weapon == "iw5_mk14_mp") hitDamage *= 2;
                else if (weapon.StartsWith("iw5_mk14_mp_reflex_xmags_camo11")) hitDamage *= 3;
                else if (weapon == "iw5_1887_mp_camo11") hitDamage = 200;
                else if (weapon == "iw5_mk12spr_mp_acog_xmags") hitDamage = 500;//Heli Sniper damage

                //if (weapon == "uav_strike_missile_mp") hitDamage = damage;//Thundergun
            }

            if (MOD == "MOD_HEADSHOT") hitDamage *= 3;

            else if (MOD == "MOD_PASSTHRU") hitDamage = damage;//Script usage

            else if ((MOD == "MOD_EXPLOSIVE" || MOD == "MOD_GRENADE_SPLASH") && botHitbox.GetField<int>("damageTaken") >= botHitbox.GetField<int>("currentHealth") * 0.7f && botHitbox.GetField<Entity>("parent").HasField("head"))
                botToCrawler(botHitbox);

            if (instaKillTime > 0 && !botHitbox.HasField("isBoss")) botHitbox.SetField("damageTaken", botHitbox.GetField<int>("currentHealth"));
            else
                botHitbox.SetField("damageTaken", botHitbox.GetField<int>("damageTaken") + hitDamage);

            if ((botHitbox.GetField<int>("damageTaken") >= botHitbox.GetField<int>("currentHealth") * 0.85f && MOD == "MOD_HEADSHOT" && botHitbox.GetField<Entity>("parent").HasField("head")) || (instaKillTime > 0 && botHitbox.GetField<Entity>("parent").HasField("head")))
            {
                Entity head = botHitbox.GetField<Entity>("parent").GetField<Entity>("head");
                head.Hide();
                botHitbox.GetField<Entity>("parent").GetField<Entity>("headHitbox").SetCanDamage(false);
                PlayFX(AIZ.fx_headshotBlood, head.Origin);
                if (instaKillTime == 0) OnInterval(1000, () => runBotBleedout(player, botHitbox));
            }

            if (!AIZ.isPlayer(player) || !player.HasField("isDown")) return;

            if (!botHitbox.GetField<Entity>("parent").GetField<bool>("primedForNuke"))
            {
                int pointGain = 5;
                if (AIZ.isHellMap) pointGain = 10;
                if (doublePointsTime > 0) pointGain *= 2;

                if (MOD != "MOD_PASSTHRU")
                {
                    player.SetField("cash", player.GetField<int>("cash") + pointGain);
                    hud.scorePopup(player, pointGain);
                }
                AIZ.addRank(player, pointGain);
            }

            if (weapon == "claymore_mp")
            {
                HudElem wireFeedback = player.GetField<HudElem>("hud_damageFeedback");
                wireFeedback.Alpha = 1;
                player.PlayLocalSound("melee_knife_hit_other");
                wireFeedback.FadeOverTime(1);
                wireFeedback.Alpha = 0;
                return;
            }

            if (skipFeedback || !AIZ.isPlayer(player) || !player.HasField("hud_damageFeedback")) return;

            HudElem combatHighFeedback = player.GetField<HudElem>("hud_damageFeedback");
            combatHighFeedback.Alpha = 1;
            player.PlayLocalSound("MP_hit_alert");
            combatHighFeedback.FadeOverTime(1);
            combatHighFeedback.Alpha = 0;
        }

        private static IEnumerator setBotImmunity(Entity bot)
        {
            bot.SetField("canBeDamaged", false);//Shotgun pellet delay. This fixes the bug where shotgun hits count every pellet for score
            yield return WaitForFrame();
            bot.SetField("canBeDamaged", true);
        }

        private static bool runBotBleedout(Entity player, Entity botHitbox)
        {
            Entity bot = botHitbox.GetField<Entity>("parent");
            if (!bot.HasField("isAlive") || !bot.GetField<bool>("isAlive")) return false;//Check before we register the hit

            onBotDamage(botHitbox, 20, player, Vector3.Zero, Vector3.Zero, "MOD_BLEEDOUT", "", "head", "j_head", 0, "", false, false);

            return true;
        }

        private static void botToCrawler(Entity botHitbox)
        {
            Entity bot = botHitbox.GetField<Entity>("parent");
            if (!bot.HasField("isAlive") || !bot.GetField<bool>("isAlive")) return;

            bot.HidePart("j_knee_le");
            PlayFX(AIZ.fx_bodyPartExplode, bot.GetTagOrigin("j_knee_le"));
            bot.HidePart("j_knee_ri");
            PlayFX(AIZ.fx_bodyPartExplode, bot.GetTagOrigin("j_knee_ri"));
            bot.SetField("hasBeenCrippled", true);//...with depression
        }

        public static void nukeDetonation(Entity owner, bool isStreak)
        {
            nukeOffsetScalar = 1;//Reset the scalar
            if (isStreak && owner != null && AIZ.isPlayer(owner))
            {
                int total = botsInPlay.Count;
                owner.SetField("cash", owner.GetField<int>("cash") + (100 * total));
                hud.scorePopup(owner, 100 * total);
                hud.scoreMessage(owner, AIZ.gameStrings[208]);
            }
            if (isStreak)
            {
                nukeOffsetScalar = 0;
                //AfterDelay(50, () => nukeOffsetScalar = 1);
            }
            bonusDrops.onNuke();

            AfterDelay(5000, () => killstreaks.nukeInbound = false);
        }


        public static void startInstakill()
        {
            if (instaKillTime < 1) return;
            if (instaKillTimerStarted) return;
            instaKillTimerStarted = true;
            hud.showPowerUpHud("instakill", null);
            OnInterval(1000, runInstakillTimer);
        }

        private static bool runInstakillTimer()
        {
            if (AIZ.gameEnded) return false;
            instaKillTime--;
            if (instaKillTime == 0)
            {
                instaKillTimerStarted = false;
                return false;
            }
            else return true;
        }
        private static bool runDoublePointsTimer()
        {
            if (AIZ.gameEnded) return false;
            doublePointsTime--;
            if (doublePointsTime == 0)
            {
                doublePointsTimerStarted = false;
                return false;
            }
            else return true;
        }

        public static void startDoublePoints()
        {
            if (doublePointsTime < 1) return;
            if (doublePointsTimerStarted) return;
            doublePointsTimerStarted = true;
            hud.showPowerUpHud("2points", null);
            OnInterval(1000, runDoublePointsTimer);
        }

        public static string getHurtAnim(Entity bot)
        {
            if (isInPeril(bot))
                return anim_runHurt;
            else return anim_walkHurt;
        }

        public static bool isInPeril(Entity bot)
        {
            if (bot.GetField<int>("damageTaken") >= (bot.GetField<int>("currentHealth") * .8f))
                return true;
            else return false;
        }
        public static void playAnimOnBot(Entity bot, string anim)
        {
            bot.ScriptModelPlayAnim(anim);

            if (anim == anim_walk && bot.HasField("hitbox") && (bot.HasField("hitbox_linkOffset_y") && bot.GetField<int>("hitbox_linkOffset_y") == 0))
            {
                bool isCrawler = !bot.HasField("head") && !bot.HasField("isBoss");
                int xAngleOffset = isCrawler ? 90 : 0;
                int xOffset = isCrawler ? -30 : 0;
                Entity botHitbox = bot.GetField<Entity>("hitbox");
                botHitbox.LinkTo(bot, "j_mainroot", new Vector3(xOffset, -10, 0), new Vector3(0, xAngleOffset, -90));
                bot.SetField("hitbox_linkOffset_y", -10);
            }
            else if (bot.HasField("hitbox") && (bot.HasField("hitbox_linkOffset_y") && bot.GetField<int>("hitbox_linkOffset_y") == -10))
            {
                bool isCrawler = !bot.HasField("head") && !bot.HasField("isBoss");
                int xAngleOffset = isCrawler ? 90 : 0;
                int xOffset = isCrawler ? -30 : 0;
                Entity botHitbox = bot.GetField<Entity>("hitbox");
                botHitbox.LinkTo(bot, "j_mainroot", new Vector3(xOffset, 0, 0), new Vector3(0, xAngleOffset, -90));
                bot.SetField("hitbox_linkOffset_y", 0);
            }
            /*
            if (bot.HasField("head"))
            {
                bot.GetField<Entity>("head").ScriptModelPlayAnim(anim);
            }
            */
        }

        public static void dropGlowstick(Vector3 position)
        {
            float posGround = GetGroundPosition(position, 1).Z + 3;
            if (posGround == position.Z + 3) posGround -= 55;
            Entity glowstick = Spawn("script_model", new Vector3(position.X, position.Y, posGround));
            glowstick.SetField("isGlowstick", true);
            glowstick.SetModel("viewmodel_light_stick");

            Entity effect = SpawnFX(AIZ.fx_glowStickGlow, glowstick.Origin);
            glowstick.SetField("effect", effect);
            TriggerFX(effect);

            glowsticks.Add(glowstick);
            AfterDelay(15000, () => removeGlowstick(glowstick));
        }
        private static void removeGlowstick(Entity glowstick)
        {
            glowsticks.Remove(glowstick);
            glowstick.ClearField("isGlowstick");
            foreach (Entity bot in botsInPlay)
            {
                if (!bot.HasField("isAlive") || !bot.GetField<bool>("isAlive")) continue;
                if (bot.HasField("currentWaypoint") && bot.GetField<Entity>("currentWaypoint") == glowstick) bot.SetField("currentWaypoint", bot);
            }
            if (glowstick.HasField("effect"))
            {
                Entity effect = glowstick.GetField<Entity>("effect");
                effect.Delete();
                glowstick.ClearField("effect");
            }
            glowstick.Delete();
        }
    }
}
