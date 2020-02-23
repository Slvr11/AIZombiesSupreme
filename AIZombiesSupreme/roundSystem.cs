using System;
using InfinityScript;
using static InfinityScript.GSCFunctions;

namespace AIZombiesSupreme
{
    public class roundSystem : BaseScript
    {
        public static Action onRoundChange;

        public static uint Wave = 0;
        private static uint bossCount = 1;
        private static uint crawlerCount = 25;
        public static uint totalWaves = 30;

        public static bool isCrawlerWave = false;
        public static bool isBossWave = false;

        //private readonly static Entity level = Entity.GetEntity(2046);

        public static void startNextRound()
        {
            checkForEndGame();//Before we start, make sure there are players to start
            Wave++;
            //foreach (Entity players in Players)
                //if (AIZ.isPlayer(players) && players.HasField("isDown")) players.SetField("currentRound", Wave);
            //level.Notify("round_changed");
            botUtil.spawnedBots = 0;
            isCrawlerWave = Wave % 5 == 0 && !isBossWave;
            isBossWave = Wave % 10 == 0;
            if (isBossWave)
            {
                botUtil.botsForWave = bossCount;
                if (bossCount == 1) bossCount = 5;
                else bossCount += 5;
                foreach (Entity players in Players) if (AIZ.isPlayer(players) && players.HasField("isDown") && !players.GetField<bool>("isDown")) players.VisionSetNakedForPlayer(AIZ.bossVision);
            }
            else if (isCrawlerWave)
            {
                //e_hud.stringsCleared = false;
                botUtil.botsForWave = crawlerCount;
                crawlerCount += 25;
                if (Wave != 5) botUtil.crawlerHealth += 250;
            }
            else//isNormalWave
            {
                botUtil.botsForWave = 10 * Wave;
                if (Wave != 1) botUtil.health += botUtil.healthScalar;
            }

            if (!isBossWave) hud.stringsCleared = false;
            
            checkForHellMapVision();
            OnInterval(500, botUtil.startBotSpawn);
            onRoundChange();
            AIZ.zState = "ingame";
            foreach (Entity players in Players)
            {
                if (AIZ.isPlayer(players) && players.HasField("isDown"))
                {
                    players.PlayLocalSound("mp_bonus_end");
                    int randomStart = AIZ.rng.Next(6);
                    switch (randomStart)
                    {
                        case 0:
                            players.PlayLocalSound("US_1mc_fightback");
                            break;
                        case 1:
                            players.PlayLocalSound("US_1mc_goodtogo");
                            break;
                        case 2:
                            players.PlayLocalSound("US_1mc_holddown");
                            break;
                        case 3:
                            players.PlayLocalSound("US_1mc_keepfighting");
                            break;
                        case 4:
                            players.PlayLocalSound("US_1mc_pushforward");
                            break;
                        case 5:
                            players.PlayLocalSound("US_1mc_readytomove");
                            break;
                    }
                }
            }
        }

        public static void checkForEndRound()
        {
            if (botUtil.botsInPlay.Count == 0 && botUtil.botsForWave == botUtil.spawnedBots)
            {
                if (Wave == totalWaves)
                {
                    AIZ.zState = "ended";
                    StartAsync(hud.endGame(true));
                    return;
                }
                //g_AIZ.zState = "intermission";
                AfterDelay(100, () => AIZ.startIntermission());
                foreach (Entity players in Players)
                {
                    if (AIZ.isPlayer(players) && players.HasField("isDown") && !players.GetField<bool>("isDown") && (!AIZ.isHellMap || (AIZ.isHellMap && killstreaks.visionRestored))) players.VisionSetNakedForPlayer(AIZ.vision);
                    if (AIZ.isPlayer(players))
                    {
                        players.PlayLocalSound("mp_bonus_start");
                        players.PlayLocalSound("US_1mc_encourage_win");
                    }
                    if (isCrawlerWave || isBossWave) AIZ.giveMaxAmmo(players);
                }
            }
            //if (!e_hud.stringsCleared && !isBossWave && Wave > 4 && f_botUtil.botsInPlay.Count < 10 && f_botUtil.botsForWave - f_botUtil.spawnedBots == 0) StartAsync(e_hud.clearAllGameStrings());
            checkForCompass();
        }

        private static void checkForHellMapVision()
        {
            if (AIZ.isHellMap)
            {
                foreach (Entity player in Players)
                {
                    if (AIZ.isPlayer(player) && player.HasField("isDown") && !player.GetField<bool>("isDown") && !killstreaks.visionRestored)
                    {
                        player.VisionSetNakedForPlayer(AIZ.hellVision);
                    }
                }
            }
        }

        public static void checkForCompass()
        {
            int veh = GetNumVehicles();
            if (((botUtil.botsInPlay.Count < 11 && veh == 0) || (botUtil.botsInPlay.Count < 6 && veh > 0)) && botUtil.botsForWave - botUtil.spawnedBots == 0)
            {
                if (!hud.stringsCleared && !isBossWave && Wave > 4 && botUtil.botsInPlay.Count > 1 && !killstreaks.nukeInbound) StartAsync(hud.clearAllGameStrings());
                foreach (Entity bot in botUtil.botsInPlay)
                {
                    if (bot.GetField<bool>("isOnCompass") || !bot.GetField<bool>("isAlive")) continue;
                    int curObjID = 31 - mapEdit.getNextObjID();
                    bot.SetField("isOnCompass", true);
                    //bot.SetField("compassID", curObjID);
                    Objective_Add(curObjID, "active", bot.Origin, "compassping_enemy");
                    //Objective_Icon(curObjID, "compassping_enemy");
                    Objective_Team(curObjID, "allies");
                    Objective_OnEntity(curObjID, bot);
                    //h_mapEdit._objIDList[curObjID] = true;
                    //if (!h_mapEdit._objIDs.ContainsKey(bot))
                    //h_mapEdit._objIDs.Add(bot, curObjID);
                    //else h_mapEdit._objIDs[bot] = curObjID;
                    mapEdit.addObjID(bot, curObjID);
                }
            }
        }

        public static void checkForEndGame()
        {
            int playersAlive = GetTeamPlayersAlive("allies");
            if (playersAlive == 1 && AIZ.zState == "ingame")
            {
                foreach (Entity player in Players)
                {
                    if (player.IsAlive && player.HasField("isDown"))
                    {
                        AfterDelay(5000, () => player.PlayLocalSound("US_1mc_lastalive"));
                        break;
                    }
                }
            }
            else if (playersAlive == 0)
                StartAsync(hud.endGame(false));
        }
    }
}
