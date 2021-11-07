using System.Collections.Generic;
using System.Collections;
using InfinityScript;
using static InfinityScript.GSCFunctions;
using static AIZombiesSupreme.botUtil;

namespace AIZombiesSupreme
{
    public class bots : BaseScript
    {

        public static bool spawnBot(int spawnLoc, bool isCrawler)
        {
            if ((!isCrawler && botPool.Count == 0) || (isCrawler && crawlerPool.Count == 0)) return true;//True so in case all 30 have spawned, don't error out
            Entity bot;
            if (isCrawler) bot = crawlerPool[0];
            else bot = botPool[0];

            if (botSpawns.Count == 0)
            {
                Utilities.PrintToConsole(AIZ.gameStrings[100]);
                Announcement(AIZ.gameStrings[101]);
                return false;
            }

            bot.Origin = botSpawns[spawnLoc] + new Vector3(AIZ.rng.Next(20), AIZ.rng.Next(20), 0);
            bot.Angles = spawnAngles[spawnLoc];
            bot.Show();
            bot.ShowAllParts();

            if (isCrawler) playAnimOnBot(bot, crawlerAnim_idle);
            else playAnimOnBot(bot, anim_idle);

            bot.SetField("state", "idle");
            if (!isCrawler && bot.HasField("head"))
            {
                Entity botHead = bot.GetField<Entity>("head");
                botHead.Show();
                //Remove helmet
                //botHead.HidePart("j_head_end");
                //botHead.HidePart("j_helmet");
                //botHead.HidePart("j_collar_rear");
                bot.GetField<Entity>("headHitbox").SetCanDamage(true);
            }
            bot.SetField("isAlive", true);
            bot.SetField("isAttacking", false);
            updateBotLastActiveTime(bot);
            spawnedBots++;
            Entity botHitbox = bot.GetField<Entity>("hitbox");
            if (isCrawler) botHitbox.SetField("currentHealth", crawlerHealth);
            else botHitbox.SetField("currentHealth", health);
            botHitbox.SetField("damageTaken", 0);
            botHitbox.SetCanDamage(true);
            botHitbox.SetCanRadiusDamage(true);
            //botHitbox.Show();
            botHitbox.SetModel("com_plasticcase_dummy");

            botsInPlay.Add(bot);
            if (isCrawler) crawlerPool.Remove(bot);
            else botPool.Remove(bot);

            onBotUpdate();

            OnInterval(100, () => botAI(bot, botHitbox, isCrawler, false));

            //Check for waypoints on spawn once
            foreach (Entity v in mapEdit.waypoints)
            {
                bool waypointTrace = SightTracePassed(bot.GetTagOrigin("j_head"), v.Origin, false, botHitbox);//Check for waypoints
                if (waypointTrace)
                {
                    bot.SetField("currentWaypoint", v);//Set the first seen one as current
                    bot.SetField("visibleWaypoints", new Parameter(v.GetField<List<Entity>>("visiblePoints")));
                    break;
                }
            }

            return true;
        }
        public static bool spawnBossBot(int spawnLoc)
        {
            if (bossPool.Count == 0) return true;//True so in case max have spawned, don't error out
            Entity bot = bossPool[0];

            if (botSpawns.Count == 0)
            {
                Utilities.PrintToConsole(AIZ.gameStrings[100]);
                Announcement(AIZ.gameStrings[101]);
                return false;
            }

            int randomInt = AIZ.rng.Next(20);
            bot.Origin = botSpawns[spawnLoc] + new Vector3(randomInt, randomInt, 0);
            bot.Angles = spawnAngles[spawnLoc];
            bot.Show();
            bot.ShowAllParts();

            playAnimOnBot(bot, anim_idle);

            bot.SetField("state", "idle");
            bot.SetField("isAlive", true);
            bot.SetField("isAttacking", false);
            //int time = GetTime();
            //bot.SetField("lastActiveTime", time);
            spawnedBots++;
            Entity botHitbox = bot.GetField<Entity>("hitbox");
            botHitbox.SetField("currentHealth", bossHealth);
            botHitbox.SetField("damageTaken", 0);
            botHitbox.SetCanDamage(true);
            botHitbox.SetCanRadiusDamage(true);
            //botHitbox.Show();
            botHitbox.SetModel("com_plasticcase_dummy");

            botsInPlay.Add(bot);
            bossPool.Remove(bot);

            onBotUpdate();

            OnInterval(100, () => botAI(bot, botHitbox, false, true));

            roundSystem.checkForCompass();

            //Check for waypoints on spawn once
            foreach (Entity v in mapEdit.waypoints)
            {
                bool waypointTrace = SightTracePassed(bot.GetTagOrigin("j_head"), v.Origin, false, botHitbox);//Check for waypoints
                if (waypointTrace)
                {
                    bot.SetField("currentWaypoint", v);//Set the first seen one as current
                    bot.SetField("visibleWaypoints", new Parameter(v.GetField<List<Entity>>("visiblePoints")));
                    break;
                }
            }

            return true;
        }

        public static bool botAI(Entity ai, Entity botHitbox, bool isCrawler, bool isBoss)
        {
            if (AIZ.gameEnded) return false;
            if (!ai.HasField("isAlive")) return false;//Return if our bot isn't set up correctly

            if (!ai.GetField<bool>("isAlive") || !botsInPlay.Contains(ai) || botHitbox.GetField<int>("currentHealth") <= botHitbox.GetField<int>("damageTaken")) return false;
            killBotIfUnderMap(ai);
            if (!ai.GetField<bool>("isAlive")) return false;//Do another check after height check

            //check time inactivity
            if (GetTime() > ai.GetField<int>("lastActiveTime") + 120000 && !isBoss && !freezerActivated)
            {
                killBotAndRespawn(ai);
                return false;
            }

            Entity target = null;
            Vector3 botOrigin = ai.Origin;
            Vector3 botHeadTag = ai.GetTagOrigin("j_head");// + new Vector3 (0, 0, 5);

            #region targeting
            if (glowsticks.Count != 0)//Find a glowstick first
            {
                foreach (Entity g in glowsticks)
                {
                    if (freezerActivated) break;
                    if (AIZ.isGlowstick(ai.GetField<Entity>("currentWaypoint"))) { target = ai.GetField<Entity>("currentWaypoint"); break; }
                    if (botOrigin.DistanceTo(g.Origin) > 500) continue;
                    if (SightTracePassed(botHeadTag, g.Origin, false, botHitbox))
                    {
                        target = g;
                        //ai.ClearField("currentWaypoint");
                        ai.SetField("currentWaypoint", g);
                        ai.ClearField("visibleWaypoints");
                        break;
                    }
                    //else
                    //{
                        //Log.Write(LogLevel.All, "No trace available");
                    //}
                }
            }
            if (target == null && !freezerActivated)//If we haven't found a glowstick, find a real target
            {
                float tempDist = 999999999;
                foreach (Entity p in Players)//Find a player
                {
                    if (!p.HasField("isDown")) continue;//Skip this player if they're not initiated for aiz

                    if (p.SessionTeam != "allies" || !p.IsAlive || p.GetField<bool>("isDown")) continue;
                    if (p.GetField<bool>("isInHeliSniper")) continue;

                    Vector3 playerOrigin = p.Origin;

                    int targetDistance = 600;
                    if (AIZ._mapname == "mp_radar" && killstreaks.mapStreakOut) targetDistance = 200;
                    if (botOrigin.DistanceTo(playerOrigin) > targetDistance) continue;

                    Vector3 playerHeadTag = p.GetTagOrigin("j_head");
                    bool trace;
                    if (!isCrawler && !isBoss)
                        trace = SightTracePassed(botHeadTag, playerHeadTag, false, botHitbox, ai.GetField<Entity>("head"), ai.GetField<Entity>("headHitbox"));
                    else trace = SightTracePassed(botHeadTag, playerHeadTag, false, botHitbox);
                    if (trace)
                    {
                        //Log.Write(LogLevel.All, "Traced {0}", p.Name);
                        //if (target != null)
                        {
                            bool isCloser = playerOrigin.DistanceTo(botOrigin) < tempDist;//Closer(botHeadTag, playerHeadTag, targetHeadTag);
                            if (isCloser)
                            {
                                tempDist = playerOrigin.DistanceTo(botOrigin);
                                target = p;
                                //ai.ClearField("currentWaypoint");
                                ai.SetField("currentWaypoint", ai);
                                ai.ClearField("visibleWaypoints");
                                //Log.Write(LogLevel.All, "{0} is closer", target.Name);
                            }
                        }
                        /*
                        else
                        {
                            target = p;
                            //ai.ClearField("currentWaypoint");
                            ai.SetField("currentWaypoint", ai);
                            ai.ClearField("visibleWaypoints");
                            //Log.Write(LogLevel.All, "Target is null");
                        }
                        */
                    }
                    //Attacking players
                    if (botHitbox.Origin.DistanceTo(playerOrigin) <= 50 && !ai.GetField<bool>("isAttacking"))
                        StartAsync(ai_attackPlayer(ai, p, isCrawler, isBoss));
                    //End attacking
                }
                if (target == null)//No players, find a waypoint
                {
                    if (ai.HasField("currentWaypoint") && ai.HasField("visibleWaypoints"))
                    {
                        //Entity currentWaypoint = ai.GetField<Entity>("currentWaypoint");
                        //if (currentWaypoint.HasField("visiblePoints") && !ai.HasField("visibleWaypoints")) ai.SetField("visibleWaypoints", new Parameter(currentWaypoint.GetField<List<Entity>>("visiblePoints")));
                        if (ai.GetField<Entity>("currentWaypoint") == ai && ai.HasField("visibleWaypoints"))
                        {
                            List<Entity> visibleWaypoints = ai.GetField<List<Entity>>("visibleWaypoints");
                            ai.SetField("currentWaypoint", visibleWaypoints[AIZ.rng.Next(visibleWaypoints.Count)]);
                        }
                        else if (botOrigin.DistanceTo(ai.GetField<Entity>("currentWaypoint").Origin) < 50)
                        {
                            ai.SetField("visibleWaypoints", new Parameter(ai.GetField<Entity>("currentWaypoint").GetField<List<Entity>>("visiblePoints")));
                            ai.SetField("currentWaypoint", ai);
                            //visibleWaypoints.Clear();
                            return true;
                        }
                    }
                    else if (!ai.HasField("currentWaypoint") || !ai.HasField("visibleWaypoints"))//Recalculate point
                    {
                        foreach (Entity v in mapEdit.waypoints)
                        {
                            //Check for waypoints
                            if (SightTracePassed(botHeadTag, v.Origin, false, botHitbox))
                            {
                                ai.SetField("currentWaypoint", v);//Set the first seen one as current
                                ai.SetField("visibleWaypoints", new Parameter(v.GetField<List<Entity>>("visiblePoints")));
                                break;
                            }
                        }
                    }
                    if (ai.HasField("currentWaypoint") && ai.GetField<Entity>("currentWaypoint") != ai) target = ai.GetField<Entity>("currentWaypoint");
                }
            }
            #endregion
            //Now we are done targeting, do the action for the target

            #region motion
            if (ai.GetField<bool>("isAttacking")) return true;//Stop moving to attack. Prevent bots getting stuck into players
            /*
            foreach (Entity bot in botsInPlay)//Prevent bots from combining into each other
            {
                if (ai == bot) continue;
                Vector3 closeOrigin = bot.Origin;
                if (botOrigin.DistanceTo(closeOrigin) < 10)//Move away from the bot and recalc
                {
                    Vector3 dir = VectorToAngles(closeOrigin - botOrigin);
                    Vector3 awayPos = botOrigin - dir * 100;
                    ai.MoveTo(awayPos, botOrigin.DistanceTo(awayPos) / 120);
                    ai.RotateTo(new Vector3(0, -dir.Y, 0), .3f, .1f, .1f);
                    return true;
                }
            }
            */
            
            float Ground = GetGroundPosition(botOrigin, 12).Z;

            if (target != null && glowsticks.Count == 0)//Move to our target if there are no glowsticks
            {
                Vector3 targetOrigin = target.Origin;
                //if (target.AIZ.isPlayer) targetHeadTag = target.GetTagOrigin("j_head");
                //else targetHeadTag = target.Origin;
                float angleY = VectorToAngles(targetOrigin - botOrigin).Y;
                ai.RotateTo(new Vector3(0, angleY, 0), .3f, .1f, .1f);

                if (botOrigin.DistanceTo2D(targetOrigin) < 100 || Ground == botOrigin.Z) Ground = targetOrigin.Z;

                int speed = 100;
                float distance = botOrigin.DistanceTo(targetOrigin);

                if (((isInPeril(botHitbox) && !ai.HasField("hasBeenCrippled")) && !ai.HasField("hasBeenCrippled")) || isBoss)
                    speed = 170;
                else if (ai.HasField("hasBeenCrippled"))
                    speed = 30;
                else if (ai.HasField("inBarbedWire") || (AIZ._mapname == "mp_radar" && killstreaks.mapStreakOut))
                    speed = 50;
                float groundDist = Ground - botOrigin.Z;
                groundDist *= 8;//Overcompansate to move faster and track along ground in a better way
                if (Ground == targetOrigin.Z) groundDist = 0;//Fix 'jumping bots'

                ai.MoveTo(new Vector3(targetOrigin.X, targetOrigin.Y, Ground + groundDist), distance / speed);

                string state = ai.GetField<string>("state");
                if ((state == "post_hurt" || state == "idle" || state == "dancing") && state != "hurt" && state != "attacking")
                {
                    if (isCrawler || ai.HasField("hasBeenCrippled")) playAnimOnBot(ai, crawlerAnim_walk);
                    else if (isBoss) playAnimOnBot(ai, anim_run);
                    else
                    {
                        if (isInPeril(botHitbox)) playAnimOnBot(ai, anim_run);
                        else playAnimOnBot(ai, anim_walk);
                    }
                    ai.SetField("state", "moving");
                }
            }
            else if (target != null && (glowsticks.Count > 0 && AIZ.isGlowstick(target)))//Move towards a glowstick and dance
            {
                Vector3 targetOrigin = target.Origin;
                if (Ground == botOrigin.Z) Ground = targetOrigin.Z;
                float angleY = VectorToAngles(targetOrigin - botOrigin).Y;
                ai.RotateTo(new Vector3(0, angleY, 0), .3f, .1f, .1f);
                string state = ai.GetField<string>("state");

                if (botOrigin.DistanceTo(targetOrigin) > 50)
                {
                    int speed = 100;
                    float distance = botOrigin.DistanceTo(targetOrigin);

                    if (((isInPeril(botHitbox) && !ai.HasField("hasBeenCrippled")) && !ai.HasField("hasBeenCrippled")) || isBoss)
                        speed = 170;
                    else if (ai.HasField("hasBeenCrippled"))
                        speed = 30;
                    else if (ai.HasField("inBarbedWire") || (AIZ._mapname == "mp_radar" && killstreaks.mapStreakOut))
                        speed = 50;
                    float groundDist = Ground - botOrigin.Z;
                    groundDist *= 8;//Overcompansate to move faster and track along ground in a better way
                    if (Ground == targetOrigin.Z) groundDist = 0;//Fix 'jumping bots'

                    ai.MoveTo(new Vector3(targetOrigin.X, targetOrigin.Y, Ground + groundDist), distance / speed);
                }
                else if (state != "dancing")
                {
                    ai.Origin = botOrigin;
                    playAnimOnBot(ai, anim_lose);
                    ai.SetField("state", "dancing");
                    return true;
                }
                if ((state == "post_hurt" || state == "idle") && state != "hurt" && state != "attacking")
                {
                    if (isCrawler) playAnimOnBot(ai, crawlerAnim_walk);
                    else if (isBoss) playAnimOnBot(ai, anim_run);
                    else playAnimOnBot(ai, anim_walk);
                    ai.SetField("state", "moving");
                }
            }
            else if (target != null && (glowsticks.Count > 0 && !AIZ.isGlowstick(target)))//Move towards a player while a glowstick is out but not in sight
            {
                Vector3 targetOrigin = target.Origin;
                if (Ground == botOrigin.Z) Ground = targetOrigin.Z;
                float angleY = VectorToAngles(targetOrigin - botOrigin).Y;
                ai.RotateTo(new Vector3(0, angleY, 0), .3f, .1f, .1f);

                int speed = 100;
                float distance = botOrigin.DistanceTo(targetOrigin);

                if (((isInPeril(botHitbox) && !ai.HasField("hasBeenCrippled")) && !ai.HasField("hasBeenCrippled")) || isBoss)
                    speed = 170;
                else if (ai.HasField("hasBeenCrippled"))
                    speed = 30;
                else if (ai.HasField("inBarbedWire") || (AIZ._mapname == "mp_radar" && killstreaks.mapStreakOut))
                    speed = 50;
                float groundDist = Ground - botOrigin.Z;
                groundDist *= 8;//Overcompansate to move faster and track along ground in a better way
                if (Ground == targetOrigin.Z) groundDist = 0;//Fix 'jumping bots'

                ai.MoveTo(new Vector3(targetOrigin.X, targetOrigin.Y, Ground + groundDist), distance / speed);

                string state = ai.GetField<string>("state");
                if ((state == "post_hurt" || state == "idle" || state == "dancing") && state != "hurt" && state != "attacking")
                {
                    if (isCrawler || ai.HasField("hasBeenCrippled")) playAnimOnBot(ai, crawlerAnim_walk);
                    else if (isBoss) playAnimOnBot(ai, anim_run);
                    else
                    {
                        if (isInPeril(botHitbox)) playAnimOnBot(ai, anim_run);
                        else playAnimOnBot(ai, anim_walk);
                    }
                    ai.SetField("state", "moving");
                }
            }
            else//failsafe, just stand still if there is no other options
            {
                ai.MoveTo(new Vector3(botOrigin.X, botOrigin.Y, Ground), 1);
                string state = ai.GetField<string>("state");
                if (state != "idle" && state != "hurt" && state != "attacking")
                {
                    if (isCrawler || ai.HasField("hasBeenCrippled")) playAnimOnBot(ai, crawlerAnim_idle);
                    else playAnimOnBot(ai, anim_idle);
                    ai.SetField("state", "idle");
                }
            }
            #endregion

            //Moving to standalone interval
            //ResetTimeout();
            return true;
        }

        private static IEnumerator ai_attackPlayer(Entity ai, Entity target, bool isCrawler, bool isBoss)
        {
            ai.SetField("isAttacking", true);

            updateBotLastActiveTime(ai);

            if (ai.GetField<bool>("primedForNuke"))
            {
                playAnimOnBot(ai, anim_lose);
                yield break;
            }
            else if (isCrawler || ai.HasField("hasBeenCrippled")) playAnimOnBot(ai, crawlerAnim_attack);
            else playAnimOnBot(ai, anim_attack);

            yield return Wait(.1f);
            PlayFX(AIZ.fx_blood, target.Origin + new Vector3(0, 0, 30));

            Vector3 dir = VectorToAngles(ai.Origin - target.Origin);
            dir.Normalize();
            float hitDir = dir.Y - target.GetPlayerAngles().Y;

            if ((target.HasWeapon("riotshield_mp") || target.HasWeapon("iw5_riotshield_mp")) && ((target.CurrentWeapon != "riotshield_mp" && target.CurrentWeapon != "iw5_riotshield_mp" && hitDir > -80 && hitDir < 80) || (target.CurrentWeapon == "riotshield_mp" || target.CurrentWeapon == "iw5_riotshield_mp")))
            {
                target.PlaySound("melee_hit");
                target.FinishPlayerDamage(null, null, dmg / 2, 0, "MOD_FALLING", "none", target.Origin, dir, "none", 0);
            }
            else
            {
                target.PlaySound("melee_punch_other");
                target.FinishPlayerDamage(null, null, dmg, 0, "MOD_FALLING", "none", target.Origin, dir, "none", 0);
            }
            int time = GetTime();
            target.SetField("lastDamageTime", time);

            yield return Wait(.6f);
            if ((isCrawler || ai.HasField("hasBeenCrippled")) && ai.GetField<bool>("isAlive"))
                playAnimOnBot(ai, crawlerAnim_walk);
            else if (isBoss && ai.GetField<bool>("isAlive"))
                playAnimOnBot(ai, anim_run);
            else
            {
                if (ai.GetField<bool>("isAlive"))
                {
                    if (isInPeril(ai.GetField<Entity>("hitbox"))) playAnimOnBot(ai, anim_run);
                    else playAnimOnBot(ai, anim_walk);
                }
            }
            if (ai.GetField<bool>("isAlive")) ai.SetField("isAttacking", false);

            if (ai.GetField<bool>("primedForNuke")) yield break;

            yield return Wait(7);
            if (target.GetField<int>("lastDamageTime") == time && target.SessionState == "playing")
                target.Health = target.MaxHealth;
        }
    }
}
