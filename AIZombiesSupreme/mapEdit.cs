using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using InfinityScript;
using static InfinityScript.GSCFunctions;

namespace AIZombiesSupreme
{
    public class mapEdit : BaseScript
    {
        public static readonly Entity _airdropCollision = AIZ.getCrateCollision(false);
        public static readonly Entity _caseCollision = AIZ.getCrateCollision(true);
        public static List<Vector3> SpawnLocs = new List<Vector3>();
        public static List<Vector3> SpawnAngles = new List<Vector3>();
        public static List<Entity> waypoints = new List<Entity>();
        public static Vector3[][] boxLocations;
        private static byte boxMaxUses = 15;
        public static List<string> maplist = new List<string>();
        public static byte randomMap = 0;
        public static Dictionary<Entity, byte> _objIDs = new Dictionary<Entity, byte>();
        public static bool[] _objIDList = new bool[32];
        public static bool[] _realObjIDList = new bool[32];
        //public static byte easterEggStep = 0;

        public static bool sale = false;
        private static byte boxCounter = 0;
        private static byte boxIndex = 0;

        private static Entity[] moonTriggers = new Entity[7];

        private static readonly int maxBankBalance = 100000;

        //private readonly static Entity level = Entity.GetEntity(2046);

        public static bool hellMapSetting = false;

        public static List<Entity> usables = new List<Entity>();

        public static readonly string teddyModel = AIZ.getTeddyModelForLevel();

        public static void createWaypoints()
        {
            var customWaypoints = checkExtraMapWaypoints();
            if (customWaypoints != null)
            {
                char[] newLineChars = new char[] {'\n', '\r'};
                foreach (Vector3 point in customWaypoints)
                {
                    Entity wp = Spawn("script_origin", point + new Vector3(0, 0, 5));
                    waypoints.Add(wp);
                }
                if (waypoints.Count > 0) AfterDelay(100, bakeWaypoints);
                return;
            }

            string file = maplist[randomMap].Replace(".map", ".wyp");
            if (!File.Exists(file))
                return;
            //if (randomMap != -1) file = "scripts\\maps\\" + _mapname + "_aiz_waypoints_" + randomMap + ".txt";
            foreach (string s in File.ReadAllLines(file))
            {
                Vector3 point = AIZ.parseVec3(s);
                if (point.Equals(Vector3.Zero)) continue;
                Entity wp = Spawn("script_origin", point + new Vector3(0, 0, 5));
                waypoints.Add(wp);
            }
            if (waypoints.Count > 0) AfterDelay(100, bakeWaypoints);
        }

        private static void bakeWaypoints()
        {
            List<Entity> badPoints = new List<Entity>();
            foreach (Entity wp in waypoints)
            {
                List<Entity> bakes = new List<Entity>();
                //for (int i = 0; i < waypoints.Count; i++)
                foreach (Entity p in waypoints)
                {
                    if (p == wp) continue;//No unneccesary trace
                    if (p.Origin.DistanceTo(wp.Origin) > 1500) continue;//Don't trace for too far away points

                    bool trace = SightTracePassed(wp.Origin + new Vector3(0, 0, 5), p.Origin + new Vector3(0, 0, 5), false);
                    if (trace)
                        bakes.Add(p);
                }
                if (bakes.Count > 0) wp.SetField("visiblePoints", new Parameter(bakes));
                else
                    badPoints.Add(wp);
            }

            validateWaypoints(badPoints);

            //Set current wps to never change
            foreach (Entity wp in waypoints)
                wp.WillNeverChange();

            //Debug
            /*
            AIZ.printToConsole("Bad waypoints: " + string.Join(", ", badPoints));
            foreach (Entity wp in waypoints)
            {
                List<Entity> visiblePoints = wp.GetField<List<Entity>>("visiblePoints");
                AIZ.printToConsole("Waypoint {0} links: " + string.Join(", ", visiblePoints), wp.EntRef);
            }
            */
        }
        private static void validateWaypoints(List<Entity> badPoints)
        {
            if (badPoints.Count > 0)
            {
                foreach (Entity p in badPoints)
                {
                    foreach (Entity goodPoint in waypoints)
                    {
                        if (goodPoint == p)
                            continue;

                        if (badPoints.Contains(goodPoint))
                            continue;

                        List<Entity> visiblePoints = goodPoint.GetField<List<Entity>>("visiblePoints");
                        for (int i = 0; i < visiblePoints.Count; i++)
                        {
                            if (visiblePoints[i] == p)//If a good point linked to this bad point, unlink them
                            {
                                visiblePoints.Remove(p);
                            }
                        }

                        if (visiblePoints.Count == 0)//If that was the only link to the good point, it is now bad.
                        {
                            badPoints.Add(goodPoint);
                            validateWaypoints(badPoints);
                            return;
                        }

                        goodPoint.SetField("visiblePoints", new Parameter(visiblePoints));
                    }

                    waypoints.Remove(p);
                    p.Delete();
                    AIZ.printToConsole(AIZ.gameStrings[237]);
                }
            }
            //badPoints.Clear();
        }

        private static void executeUsable(string type, Entity player, Entity ent)
        {
            switch (type)
            {
                case "revive":
                    revivePlayer(ent, player);
                    break;
                case "door":
                    useDoor(ent, player);
                    break;
                case "randombox":
                    StartAsync(useBox(ent, player));
                    break;
                case "pap":
                    StartAsync(usePapBox(ent, player, player.CurrentWeapon));
                    break;
                case "gambler":
                    useGambler(ent, player);
                    break;
                case "perk1":
                    usePerk(ent, player, 1);
                    break;
                case "perk2":
                    usePerk(ent, player, 2);
                    break;
                case "perk3":
                    usePerk(ent, player, 3);
                    break;
                case "perk4":
                    usePerk(ent, player, 4);
                    break;
                case "perk5":
                    usePerk(ent, player, 5);
                    break;
                case "perk6":
                    usePerk(ent, player, 6);
                    break;
                case "perk7":
                    usePerk(ent, player, 7);
                    break;
                case "perk8":
                    usePerk(ent, player, 8);
                    break;
                case "ammo":
                    useAmmo(ent, player);
                    break;
                case "bank":
                    useBank(player, true);
                    break;
                case "power":
                    usePower(ent, player);
                    break;
                case "killstreak":
                    useKillstreak(ent, player);
                    break;
                case "linker":
                    StartAsync(linkTeleporter(ent, player));
                    break;
                case "teleporter":
                    StartAsync(useTeleporter(ent, player));
                    break;
                case "elevator":
                    useElevator(ent, player);
                    break;
                case "carePackage":
                    grabCarePackage(ent, player);
                    break;
                case "expAmmo":
                    setExpAmmo(ent, player);
                    break;
                case "heliExtraction":
                    heliSniper_boardHeli(ent, player);
                    break;
                case "wallweapon":
                    usedWallWeapon(ent, player);
                    break;
                case "helmet":
                    useHelmet(player);
                    break;
                case "dome_eeDog":
                    dome_checkEasterEggTrigger1(ent, player);
                    break;
                case "dome_eeBunkerCabinet":
                    dome_checkEasterEggStep4_A(ent);
                    break;
                case "dome_eeDomeCabinet":
                    dome_checkEasterEggStep4_B(ent);
                    break;
                case "giftTrigger":
                    givePlayerCash(player, ent.GetField<Entity>("owner"));
                    break;
                default:
                    break;
            }
        }

        public static void removeUsable(Entity usable)
        {
            if (_objIDs.ContainsKey(usable))
            {
                removeObjID(usable);
            }
            //Entity trigger = usable.GetField<Entity>("trigger");
            usables.Remove(usable);
            if (!usables.Contains(usable))
            {
                if (usable.HasField("pieces"))
                {
                    List<Entity> pieces = usable.GetField<List<Entity>>("pieces");
                    foreach (Entity p in pieces) p.Delete();
                }
                usable.Delete();
                //trigger.Delete();
            }
            else
                AIZ.printToConsole("A usable ({0}) was not correctly removed from the list of usables!", usable.GetField("usabletype"));
        }

        public static int getNextObjID()
        {
            for (int i = 0; i < _objIDList.Length; i++)
            {
                if (!_objIDList[31 - i] && !_realObjIDList[i]) return i;
            }
            return 0;
        }
        public static int getNextRealObjID()
        {
            for (int i = 0; i < _realObjIDList.Length; i++)
            {
                if (!_realObjIDList[i]) return i;
            }
            return _realObjIDList.Length;
        }

        public static void removeObjID(Entity ent)
        {
            if (_objIDs.ContainsKey(ent))
            {
                int icon = _objIDs[ent];
                Objective_Delete(icon);
                _objIDList[icon] = false;
                _objIDs.Remove(ent);
                //usable.ClearField("objID");
            }
        }

        public static void addObjID(Entity ent, int id)
        {
            if (!_objIDs.ContainsKey(ent)) _objIDs.Add(ent, (byte)id);
            else
            {
                /*
                g_AIZ.printToConsole("An entity tried to apply a currently used objID. Re-allocating id...");
                removeObjID(ent);
                addObjID(ent, id);
                */
                AIZ.printToConsole("An entity tried to apply a currently used objID. No objID will be set.");
                return;

            }

            _objIDList[id] = true;
        }

        public static void trackUsablesForPlayer(Entity player)
        {
            //foreach (Entity player in Players)
            //player.OnNotify("use_button_pressed", checkPlayerUsables);//Might need to change to global/Add entity argument
            /*
            player.OnNotify("trigger", (ent, ent2) =>
            {
                if (!player.GetField<bool>("hasMessageUp")) handleUsableMessage((Entity)ent2);
            });//Global?
            */
            OnInterval(250, () => handleUsableMessage(player));
        }

        private static bool handleUsableMessage(Entity player)
        {
            if (AIZ.gameEnded) return false;
            if (!player.IsAlive || !AIZ.isPlayer(player)) return false;
            foreach (Entity usable in usables)
            {
                if (!usable.HasField("range")) continue;

                if (usable.GetField<string>("usabletype") == "giftTrigger")
                    if (usable.GetField<Entity>("owner") == player) continue;

                if (player.Origin.DistanceTo(usable.Origin) < usable.GetField<int>("range"))
                {
                    displayUsableHintMessage(player, usable);
                    return false;//We found a usable close enough, get out of this loop
                }
                //return true;
            }
            return true;
        }

        private static void displayUsableHintMessage(Entity player, Entity usable)
        {
            if (!player.HasField("hud_message")) return;
            player.SetField("hasMessageUp", true);
            HudElem message = player.GetField<HudElem>("hud_message");
            message.Alpha = .85f;
            hud._setText(message, getUsableText(usable, player));
            //player.SetField("hud_message", message);
            OnInterval(250, () => watchPlayerLeaveUsable(player, usable, message));
        }
        private static bool watchPlayerLeaveUsable(Entity player, Entity usable, HudElem message)
        {
            if (AIZ.gameEnded) return false;
            if (!AIZ.isPlayer(player) || !player.IsAlive) return false;
            hud._setText(message, getUsableText(usable, player));
            if (player.Origin.DistanceTo(usable.Origin) > usable.GetField<int>("range"))
            {
                message.Alpha = 0;
                hud._setText(message, "");
                //message.Destroy();
                player.SetField("hasMessageUp", false);
                //player.ClearField("hud_message");
                if (player.IsAlive) trackUsablesForPlayer(player);
                return false;
            }
            else return true;
        }

        public static void checkPlayerUsables(Entity player)
        {
            if (player.IsAlive)
            {
                foreach (Entity usable in usables)
                {
                    if (usable.HasField("range") && player.Origin.DistanceTo(usable.Origin) < usable.GetField<int>("range"))
                    {
                        if (usable.GetField<string>("usabletype") == "giftTrigger") continue;

                        //Log.Write(LogLevel.All, "Usable {0} found", usable.GetField<string>("usabletype"));
                        executeUsable(usable.GetField<string>("usabletype"), player, usable);
                        return;//We found a usable close enough, get out of this loop
                    }
                }
                /*
                if (Entity.Level.HasField("allowGifting"))
                {
                    foreach (Entity giftTrigger in usables.FindAll((u) => u.GetField<string>("usabletype") == "giftTrigger"))
                    {
                        if (giftTrigger.GetField<string>("usabletype") == "giftTrigger")
                            if (giftTrigger.GetField<Entity>("owner") == player) continue;

                        executeUsable(giftTrigger.GetField<string>("usabletype"), player, giftTrigger);
                        return;
                    }
                }
                */
            }
        }

        #region special usable logic
        private static void revivePlayer(Entity reviveTrigger, Entity reviver)
        {
            if (reviver.GetField<bool>("isCarryingSentry")) return;
            if (reviver.GetField<bool>("isDown")) return;
            if (reviver.SessionTeam != "allies") return;
            if (reviveTrigger.GetField<Entity>("player") == reviver) return;
            if (reviveTrigger.GetField<Entity>("user") != reviveTrigger) return;//To avoid multiple revivers at a time
            reviveTrigger.GetField<Entity>("player").IPrintLnBold("Being revived by " + reviver.Name + "...");
            HudElem progressBar = hud.createPrimaryProgressBar(reviver, 0, 0);
            //progressBar.SetPoint("center", "center", 0, -61);
            progressBar.SetField("isScaling", false);
            reviveTrigger.SetField("user", reviver);
            reviveTrigger.SetField("reviveCounter", 1);
            OnInterval(50, () => revivePlayer_logicLoop(reviver, reviveTrigger, progressBar));
        }
        private static bool revivePlayer_logicLoop(Entity reviver, Entity reviveTrigger, HudElem progressBar)
        {
            if (AIZ.gameEnded)
            {
                progressBar.GetField("bar").As<HudElem>().Destroy();
                progressBar.Destroy();
                return false;
            }
            if (reviver.UseButtonPressed() && reviveTrigger.GetField<Entity>("player").IsAlive && reviver.Origin.DistanceTo(reviveTrigger.Origin) < 75 && !reviver.GetField<bool>("isDown"))
            {
                int reviveCounter = reviveTrigger.GetField<int>("reviveCounter");
                reviver.DisableWeapons();
                reviveCounter++;
                if (reviver.GetField<bool>("autoRevive")) reviveCounter++;//Double time
                reviveTrigger.SetField("reviveCounter", reviveCounter);

                if (!(bool)progressBar.GetField("isScaling"))
                {
                    progressBar.SetField("isScaling", true);
                    if (reviver.GetField<bool>("autoRevive")) hud.updateBar(progressBar, 120, 2.5f);
                    else hud.updateBar(progressBar, 120, 5);
                }
                if (reviveCounter >= 100)
                {
                    Entity downedPlayer = reviveTrigger.GetField<Entity>("player");
                    downedPlayer.LastStandRevive();
                    reviver.EnableWeapons();
                    downedPlayer.SetField("isDown", false);
                    if (!AIZ.isHellMap || (AIZ.isHellMap && killstreaks.visionRestored)) downedPlayer.VisionSetNakedForPlayer(AIZ.vision);
                    else downedPlayer.VisionSetNakedForPlayer(AIZ.hellVision);
                    downedPlayer.SetCardDisplaySlot(reviver, 5);
                    downedPlayer.ShowHudSplash("revived", 1);
                    downedPlayer.EnableWeaponSwitch();
                    downedPlayer.EnableOffhandWeapons();
                    List<string> weaponList = downedPlayer.GetField<List<string>>("weaponsList");
                    if (!weaponList.Contains("iw5_usp45_mp"))
                    {
                        downedPlayer.TakeWeapon("iw5_usp45_mp");
                        downedPlayer.SwitchToWeapon(downedPlayer.GetField<string>("lastDroppableWeapon"));
                    }
                    //StartAsync(AIZ.restoreWeaponIfEmptyHanded(downedPlayer));

                    downedPlayer.Health = downedPlayer.MaxHealth;
                    reviveTrigger.GetField<HudElem>("icon").Destroy();
                    //downedPlayer.HeadIcon = "";
                    //downedPlayer.HeadIconTeam = "axis";
                    progressBar.GetField("bar").As<HudElem>().Destroy();
                    progressBar.Destroy();
                    int amount = downedPlayer.Score / 30;
                    amount -= amount % 10;//Remove the difference
                    hud.scoreMessage(reviver, "Revived " + downedPlayer.Name + "!");
                    reviver.SetField("cash", reviver.GetField<int>("cash") + amount);
                    hud.scorePopup(reviver, amount);
                    reviver.Assists++;
                    reviveTrigger.ClearField("reviveCounter");
                    removeUsable(reviveTrigger);
                    return false;
                }
                return true;
            }
            else
            {
                Entity downedPlayer = reviveTrigger.GetField<Entity>("player");
                reviveTrigger.SetField("user", reviveTrigger);
                progressBar.GetField("bar").As<HudElem>().Destroy();
                progressBar.Destroy();
                reviveTrigger.SetField("reviveCounter", 1);
                reviver.EnableWeapons();
                if (!downedPlayer.IsAlive)
                {
                    removeUsable(reviveTrigger);
                    return false;
                }
                else return false;
            }
        }

        private static void grabCarePackage(Entity package, Entity player)
        {
            //if (package.GetField<Entity>("user") != package) return;//Default so that there can't be multiple users
            if (player.GetField<bool>("isCarryingSentry")) return;
            if (player.SessionTeam != "allies") return;

            HudElem progressBar = hud.createPrimaryProgressBar(player, 0, 0);
            progressBar.SetField("isScaling", false);

            //package.SetField("user", player);
            player.SetField("percent", 0);

            OnInterval(50, () => grabCarePackage_logicLoop(player, package, progressBar));
        }
        private static bool grabCarePackage_logicLoop(Entity player, Entity package, HudElem progressBar)
        {
            if (AIZ.gameEnded)
            {
                progressBar.GetField("bar").As<HudElem>().Destroy();
                progressBar.Destroy();
                return false;
            }
            if (!AIZ.isPlayer(player)) return false;

            if (player.UseButtonPressed() && player.Origin.DistanceTo(package.Origin) < 75)
            {
                int percent = player.GetField<int>("percent");

                player.DisableWeapons();
                Entity owner = package.GetField<Entity>("owner");

                if (owner == player)
                    percent += 10;
                else
                    percent++;
                player.SetField("percent", percent);

                if (!(bool)progressBar.GetField("isScaling") && owner == player)
                {
                    progressBar.SetField("isScaling", true);
                    //progressBar.ScaleOverTime(1, 100, 10);
                    hud.updateBar(progressBar, 120, .5f);
                }
                else if (!(bool)progressBar.GetField("isScaling") && owner != player)
                {
                    progressBar.SetField("isScaling", true);
                    //progressBar.ScaleOverTime(2.5f, 100, 10);
                    hud.updateBar(progressBar, 120, 5f);
                }
                if (percent >= 100)
                {
                    player.EnableWeapons();
                    progressBar.GetField("bar").As<HudElem>().Destroy();
                    progressBar.Destroy();
                    if (player != owner && AIZ.isPlayer(owner))
                    {
                        owner.SetField("cash", owner.GetField<int>("cash") + 100);
                        hud.scorePopup(owner, 100);
                        hud.scoreMessage(owner, "Shared Care Package!");
                    }
                    PlayFX(AIZ.fx_crateCollectSmoke, package.Origin);
                    PlaySoundAtPos(package.Origin, "crate_impact");
                    player.ClearField("percent");
                    removeUsable(package);
                    killstreaks.giveKillstreak(player, package.GetField<int>("streak"));
                    return false;
                }
                return true;
            }
            else
            {
                progressBar.GetField("bar").As<HudElem>().Destroy();
                progressBar.Destroy();
                player.ClearField("percent");
                player.EnableWeapons();
                return false;
            }
        }

        private static void setExpAmmo(Entity box, Entity player)
        {
            if (!player.HasField("isDown")) return;
            if (player.GetField<bool>("hasExpAmmoPerk")) return;
            if (player.GetField<bool>("isCarryingSentry")) return;
            if (player.SessionTeam != "allies") return;

            HudElem progressBar = hud.createPrimaryProgressBar(player, 0, 0);
            progressBar.SetField("isScaling", false);

            player.SetField("percent", 0);

            OnInterval(50, () => setExpAmmo_logicLoop(box, player, progressBar));
        }
        private static bool setExpAmmo_logicLoop(Entity box, Entity player, HudElem progressBar)
        {
            if (AIZ.gameEnded)
            {
                progressBar.GetField("bar").As<HudElem>().Destroy();
                progressBar.Destroy();
                return false;
            }
            if (player.UseButtonPressed() && player.Origin.DistanceTo(box.Origin) < 75)
            {
                int grabCounter = player.GetField<int>("percent");

                player.DisableWeapons();
                grabCounter += 2;
                player.SetField("percent", grabCounter);

                if (!(bool)progressBar.GetField("isScaling"))
                {
                    progressBar.SetField("isScaling", true);
                    hud.updateBar(progressBar, 120, 2.5f);
                }
                if (grabCounter >= 100)
                {
                    progressBar.GetField("bar").As<HudElem>().Destroy();
                    progressBar.Destroy();
                    player.SetPerk("specialty_explosivebullets", true, true);
                    player.SetField("hasExpAmmoPerk", true);
                    player.ClearField("percent");

                    List<string> weaponsList = player.GetField<List<string>>("weaponsList");
                    foreach (string weapon in weaponsList)
                    {
                        string type = WeaponType(weapon);
                        if (type == "projectile" || type == "grenade" || AIZ.isRayGun(weapon)) continue;

                        player.SetWeaponAmmoClip(weapon, 0);
                        player.GiveMaxAmmo(weapon);
                    }

                    player.EnableWeapons();
                    Entity owner = box.GetField<Entity>("owner");
                    if (player != owner)
                    {
                        owner.SetField("cash", owner.GetField<int>("cash") + 50);
                        hud.scorePopup(owner, 50);
                        hud.scoreMessage(owner, "Explosive Ammo Shared!");
                    }
                    return false;
                }
                return true;
            }
            else
            {
                progressBar.GetField("bar").As<HudElem>().Destroy();
                progressBar.Destroy();
                player.ClearField("percent");
                player.EnableWeapons();
                return false;
            }
        }

        private static void heliSniper_boardHeli(Entity node, Entity player)
        {
            if (player.GetField<bool>("isCarryingSentry")) return;
            if (player.SessionTeam != "allies") return;
            if (AIZ.isWeaponDeathMachine(player.CurrentWeapon)) return;
            if (player != node.GetField<Entity>("heli").GetField<Entity>("owner") || player.GetField<bool>("isDown")) return;

            node.SetField("percent", 0);
            OnInterval(50, () => heliSniper_boardHeli_holdLogicLoop(player, node));
        }
        private static bool heliSniper_boardHeli_holdLogicLoop(Entity player, Entity node)
        {
            if (AIZ.gameEnded) return false;
            if (player.UseButtonPressed() && player.Origin.DistanceTo(node.Origin) < 75)
            {
                node.SetField("percent", node.GetField<int>("percent") + 1);
                if (node.GetField<int>("percent") >= 5)
                {
                    StartAsync(killstreaks.heliSniper_doBoarding(node.GetField<Entity>("heli"), player));
                    if (node.HasField("icon")) node.GetField<HudElem>("icon").Destroy();
                    node.ClearField("percent");
                    removeUsable(node);
                    return false;
                }
                return true;
            }
            else
            {
                //grabCounter = 1;
                return false;
            }
        }

        private static void givePlayerCash(Entity sender, Entity recipient)
        {
            if (!recipient.HasField("isDown")) return;
            if (!IsPlayer(recipient)) return;
            if (sender == recipient) return;
            if (!recipient.IsAlive || !sender.IsAlive) return;
            if (sender.GetField<int>("cash") < 500) return;

            sender.SetField("cash", sender.GetField<int>("cash") - 500);
            hud.scorePopup(sender, -500);
            string icon = hud.createHudShaderString("cardicon_girlskull", false, 64, 64);
            string senderMessage = icon + string.Format(AIZ.gameStrings[238], recipient.Name) + icon;
            hud.scoreMessage(sender, senderMessage);

            recipient.SetField("cash", recipient.GetField<int>("cash") + 500);
            hud.scorePopup(recipient, 500);
            string recipientMessage = icon + string.Format(AIZ.gameStrings[239], sender.Name) + icon;
            hud.scoreMessage(recipient, recipientMessage);
        }
        #endregion

        #region stucture creators
        public static void randomWeaponCrate(Vector3 origin, Vector3 angles, int? objID = null, int currentLoc = 0)
        {
            Entity crate = spawnCrate(origin, angles, false, false);
            int curObjID;
            if (objID != null)
                curObjID = objID.Value;
            else
                curObjID = 31 - getNextObjID();
            Objective_Add(curObjID, "active", crate.Origin, "weapon_colt_45"); // objective_add
            Objective_Team(curObjID, "allies");
            crate.SetField("objID", curObjID);
            addObjID(crate, curObjID);
            /*
            HudElem HeadIcon = NewHudElem();
            HeadIcon.X = origin.X;
            HeadIcon.Y = origin.Y;
            HeadIcon.Z = origin.Z + 40;
            HeadIcon.Alpha = 0.85f;
            HeadIcon.SetShader("weapon_colt_45", 10, 10);
            HeadIcon.SetWaypoint(true, false, false);
            crate.SetField("icon", HeadIcon);
            */

            crate.SetField("state", "idle");
            crate.SetField("weapon", 0);
            crate.SetField("player", "");
            crate.SetField("destroyed", true);
            crate.SetField("lastLocation", currentLoc);

            Entity weapon = Spawn("script_model", crate.Origin + new Vector3(0, 0, 20));
            weapon.SetModel("viewmodel_metal_gear_gun");
            weapon.HidePart("tag_silencer");
            weapon.HidePart("tag_knife");
            crate.SetField("weaponEnt", weapon);

            OnInterval(3000, () => rotateWeaponCrateWeapon(weapon, crate));

            makeUsable(crate, "randombox", 75);

            if (Entity.Level.HasField("isXmas"))
                AIZ.spawnXmasLightsOnUsable(crate);
            //return crate;
        }
        private static bool rotateWeaponCrateWeapon(Entity weapon, Entity crate)
        {
            if (crate.GetField<string>("state") == "idle")
            {
                weapon.RotateYaw(360, 3);
                return true;
            }
            else if (!AIZ.gameEnded) return true;
            else return false;
        }

        private static void papCrate(Vector3 origin, Vector3 angles)
        {
            Entity crate = Spawn("script_model", origin - new Vector3(0, 0, 15));
            crate.SetModel("com_plasticcase_beige_big");
            if (_caseCollision != null) crate.CloneBrushModelToScriptModel(_caseCollision);
            else crate.CloneBrushModelToScriptModel(_airdropCollision);
            crate.Angles = angles;
            int curObjID = 31 - getNextObjID();
            Objective_Add(curObjID, "active", crate.Origin, "cardicon_brassknuckles");
            //Call(435, curObjID, new Parameter(crate.Origin)); // objective_position
            //Call(434, curObjID, "cardicon_brassknuckles"); // objective_icon
            Objective_Team(curObjID, "allies");
            addObjID(crate, curObjID);
            /*
            HudElem HeadIcon = NewHudElem();
            HeadIcon.X = origin.X;
            HeadIcon.Y = origin.Y;
            HeadIcon.Z = origin.Z + 40;
            HeadIcon.Alpha = 0.85f;
            HeadIcon.SetShader("cardicon_brassknuckles", 10, 10);
            HeadIcon.SetWaypoint(true, false, false);
            */
            crate.SetField("state", "idle");
            crate.SetField("weapon", "");
            crate.SetField("player", Entity.Level);

            Entity weapon = Spawn("script_model", crate.Origin + new Vector3(0, 0, 10));
            weapon.SetModel("tag_origin");
            weapon.Hide();
            weapon.EnableLinkTo();
            crate.SetField("weaponEnt", weapon);

            Entity[] attachments = new Entity[2];
            attachments[0] = Spawn("script_model", weapon.Origin);
            attachments[0].SetModel("tag_origin");
            attachments[0].LinkTo(weapon);
            attachments[0].Hide();
            attachments[1] = Spawn("script_model", weapon.Origin);
            attachments[1].SetModel("tag_origin");
            attachments[1].LinkTo(weapon);
            attachments[1].Hide();
            crate.SetField("attachments", new Parameter(attachments));

            makeUsable(crate, "pap", 75);
            //return crate;
        }

        private static void gamblerCrate(Vector3 origin, Vector3 angles)
        {
            Entity crate = spawnCrate(origin, angles, false, false);
            Entity laptop = Spawn("script_model", new Vector3(origin.X, origin.Y, origin.Z + 22));
            laptop.Angles = new Vector3(0, 90, 0);
            laptop.SetModel("com_laptop_2_open");
            crate.SetField("laptop", laptop);
            crate.SetField("GamblerInUse", false);
            OnInterval(4000, () => rotateEntity(laptop));
            int curObjID = 31 - getNextObjID();
            Objective_Add(curObjID, "active", crate.Origin, "cardicon_8ball"); // objective_add
            //Call(435, curObjID, new Parameter(crate.Origin)); // objective_position
            //Call(434, curObjID, "cardicon_8ball"); // objective_icon
            Objective_Team(curObjID, "allies");
            addObjID(crate, curObjID);
            /*
            HudElem HeadIcon = NewHudElem();
            HeadIcon.X = origin.X;
            HeadIcon.Y = origin.Y;
            HeadIcon.Z = origin.Z + 40;
            HeadIcon.Alpha = 0.85f;
            HeadIcon.SetShader("cardicon_8ball", 10, 10);
            HeadIcon.SetWaypoint(true, false, false);
            */
            makeUsable(crate, "gambler", 75);
            //return crate;
        }

        private static void perkCrate(Vector3 origin, Vector3 angles, int perk)
        {
            Entity crate = spawnCrate(origin, angles, false, false);
            string model;
            if (perk == -1 || perk == 1 || perk == 2 || perk == 5) model = "com_plasticcase_enemy";
            else model = "com_plasticcase_friendly";
            crate.SetModel(model);
            int curObjID = 31 - getNextObjID();
            string icon;
            int cost = 5000;
            switch (perk)
            {
                case 1:
                case -1:
                    cost = 2500;
                    icon = "cardicon_juggernaut_1";
                    break;
                case 2:
                    cost = 2000;
                    icon = "specialty_longersprint_upgrade";
                    break;
                case 3:
                    cost = 3000;
                    icon = "specialty_fastreload_upgrade";
                    break;
                case 4:
                    cost = 4000;
                    icon = "specialty_twoprimaries_upgrade";
                    break;
                case 5:
                    cost = 2000;
                    icon = "weapon_attachment_rof";
                    break;
                case 6:
                    cost = 1500;
                    icon = "specialty_stalker_upgrade";
                    break;
                case 7:
                    cost = 1500;
                    icon = "waypoint_revive";
                    break;
                case 8:
                    cost = 4000;
                    icon = "specialty_scavenger_upgrade";
                    break;
                default:
                    icon = "white";
                    break;
            }
            Objective_Add(curObjID, "active", crate.Origin, icon); // objective_add
            //Call(435, curObjID, new Parameter(crate.Origin)); // objective_position
            //Call(434, curObjID, "cardicon_juggernaut_1"); // objective_icon
            Objective_Team(curObjID, "allies");
            addObjID(crate, curObjID);
            crate.SetField("cost", cost);
            makeUsable(crate, "perk" + perk, 75);
            if (perk == -1) spawnRebar(origin, crate);
            //return crate;
        }

        private static void ammoCrate(Vector3 origin, Vector3 angles)
        {
            Entity crate = spawnCrate(origin, angles, false, false);
            int curObjID = 31 - getNextObjID();
            Objective_Add(curObjID, "active", crate.Origin, "waypoint_ammo_friendly");//waypoint_ammo_friendly or airdrop_icon
            Objective_Team(curObjID, "allies");
            addObjID(crate, curObjID);
            /*
            HudElem HeadIcon = NewHudElem();
            HeadIcon.X = origin.X;
            HeadIcon.Y = origin.Y;
            HeadIcon.Z = origin.Z + 40;
            HeadIcon.Alpha = 0.85f;
            HeadIcon.SetShader("airdrop_icon", 10, 10);
            HeadIcon.SetWaypoint(true, false, false);
            */

            Entity ammoBox = Spawn("script_model", origin + new Vector3(0, 0, 20));
            ammoBox.Angles = Vector3.Zero;
            ammoBox.SetModel("weapon_m60_clip_iw5");
            Entity ammoRotater = Spawn("script_model", origin + new Vector3(0, 0, 20));
            ammoRotater.Angles = Vector3.Zero;
            ammoRotater.SetModel("tag_origin");
            ammoBox.EnableLinkTo();
            ammoBox.LinkTo(ammoRotater, "tag_origin", new Vector3(7, -4, 0), Vector3.Zero);
            OnInterval(4000, () => rotateEntity(ammoRotater));

            crate.SetField("used", false);
            makeUsable(crate, "ammo", 75);
            //return crate;
        }

        private static void killstreakCrate(Vector3 origin, Vector3 angles)
        {
            Entity crate = spawnCrate(origin, angles, false, false);
            crate.SetModel("com_plasticcase_enemy");
            int curObjID = 31 - getNextObjID();
            Objective_Add(curObjID, "active", crate.Origin, "cardicon_aircraft_01");
            //Call(435, curObjID, new Parameter(crate.Origin)); // objective_position
            //Call(434, curObjID, "cardicon_brassknuckles"); // objective_icon
            Objective_Team(curObjID, "allies");
            addObjID(crate, curObjID);
            /*
            HudElem HeadIcon = NewHudElem();
            HeadIcon.X = origin.X;
            HeadIcon.Y = origin.Y;
            HeadIcon.Z = origin.Z + 40;
            HeadIcon.Alpha = 0.85f;
            HeadIcon.SetShader("cardicon_brassknuckles", 10, 10);
            HeadIcon.SetWaypoint(true, false, false);
            */
            Entity remote = Spawn("script_model", origin + new Vector3(0, 0, 20));
            remote.Angles = Vector3.Zero;
            remote.SetModel("viewmodel_uav_radio");
            OnInterval(4000, () => rotateEntity(remote));

            makeUsable(crate, "killstreak", 75);
            //return crate;
        }

        private static bool rotateEntity(Entity ent)
        {
            if (AIZ.gameEnded) return false;
            ent.RotateYaw(360, 4);
            return true;
        }

        private static void powerCrate(Vector3 origin, Vector3 angles)
        {
            Entity crate = spawnCrate(origin, angles, false, false);
            int curObjID = 31 - getNextObjID();
            Objective_Add(curObjID, "active", crate.Origin, "cardicon_bulb");
            //Call(435, curObjID, new Parameter(crate.Origin)); // objective_position
            //Call(434, curObjID, "cardicon_brassknuckles"); // objective_icon
            Objective_Team(curObjID, "allies");
            crate.SetField("objID", curObjID);
            addObjID(crate, curObjID);
            /*
            HudElem HeadIcon = NewHudElem();
            HeadIcon.X = origin.X;
            HeadIcon.Y = origin.Y;
            HeadIcon.Z = origin.Z + 40;
            HeadIcon.Alpha = 0.85f;
            HeadIcon.SetShader("cardicon_bulb", 10, 10);
            HeadIcon.SetWaypoint(true, false, false);
            */
            //Objective_Delete(curObjID);
            //crate.SetField("icon", HeadIcon);

            Entity fx = SpawnFX(AIZ.fx_glow, crate.Origin + new Vector3(0, 0, 20));
            Entity fx2 = SpawnFX(AIZ.fx_glow2, crate.Origin + new Vector3(0, 0, 30));
            OnInterval(100, () => runPowerCrateFX(fx, fx2));

            crate.SetField("fx", fx);
            crate.SetField("bought", false);
            makeUsable(crate, "power", 75);
            //return crate;
        }

        private static bool runPowerCrateFX(Entity fx, Entity fx2)
        {
            TriggerFX(fx);
            TriggerFX(fx2);
            if (fx.HasField("delete"))
            {
                fx.ClearField("delete");
                fx.Delete();
                fx2.Delete();
                return false;
            }
            return true;
        }

        private static Entity wallWeapon(Vector3 origin, Vector3 angles, string weapon, int price)
        {
            Entity wep = Spawn("script_model", origin);
            string model = GetWeaponModel(weapon);
            wep.SetModel(model);
            wep.Angles = angles;
            wep.SetField("price", price);
            wep.SetField("weapon", weapon);
            wep.SetField("bought", false);
            makeUsable(wep, "wallweapon", 70);
            return wep;
        }

        private static void createRamp(Vector3 top, Vector3 bottom)
        {
            float distance = top.DistanceTo(bottom);
            int blocks = (int)Math.Ceiling(distance / 30);
            Vector3 A = new Vector3((top.X - bottom.X) / blocks, (top.Y - bottom.Y) / blocks, (top.Z - bottom.Z) / blocks);
            Vector3 temp = VectorToAngles(top - bottom);
            Vector3 BA = new Vector3(temp.Z, temp.Y + 90, temp.X);
            for (int b = 0; b <= blocks; b++)
            {
                spawnCrate(bottom + (A * b), BA, false, false);
            }
        }

        private static void spawnRebar(Vector3 origin, Entity crate)
        {
            if (AIZ._mapname != "mp_interchange") return;
            Entity rebar = Spawn("script_model", origin - new Vector3(0, 100, 45));
            rebar.SetModel("concrete_slabs_lrg1");
            StartAsync(initJuggAnim(crate, rebar));
        }
        private static IEnumerator initJuggAnim(Entity crate, Entity rebar)
        {
            yield return Wait(30);
            if (!AIZ.powerActivated)
            {
                StartAsync(initJuggAnim(crate, rebar));
                yield break;
            }
            else if (hud.EMPTime == 0)
            {
                foreach (Entity player in Players)
                {
                    if (AIZ.isPlayer(player)) player.PlayLocalSound("nuke_wave");
                }

                if (crate.HasField("fx_xmas"))
                {
                    Entity[] fx = crate.GetField<Entity[]>("fx_xmas");
                    StopFXOnTag(AIZ.fx_rayGunUpgrade, fx[0], "tag_origin");
                    StopFXOnTag(AIZ.fx_rayGun, fx[1], "tag_origin");
                    StopFXOnTag(AIZ.fx_rayGun, fx[2], "tag_origin");
                    StopFXOnTag(AIZ.fx_rayGunUpgrade, fx[3], "tag_origin");

                    AfterDelay(6000, () => PlayFXOnTag(AIZ.fx_rayGunUpgrade, fx[0], "tag_origin"));
                    AfterDelay(6200, () => PlayFXOnTag(AIZ.fx_rayGun, fx[1], "tag_origin"));
                    AfterDelay(6400, () => PlayFXOnTag(AIZ.fx_rayGun, fx[2], "tag_origin"));
                    AfterDelay(6800, () => PlayFXOnTag(AIZ.fx_rayGunUpgrade, fx[3], "tag_origin"));
                }

                yield return Wait(2.8f);

                Entity rockCrumble = SpawnFX(AIZ.fx_rock, rebar.Origin);
                TriggerFX(rockCrumble);
                Entity Smoke2 = SpawnFX(AIZ.fx_crateSmoke, rebar.Origin + new Vector3(100, 0, 0));
                TriggerFX(Smoke2);
                Entity Smoke3 = SpawnFX(AIZ.fx_crateSmoke, rebar.Origin - new Vector3(100, 0, 0));
                TriggerFX(Smoke3);
                Earthquake(0.5f, 6.5f, rebar.Origin - new Vector3(0, 0, 500), 5000);

                yield return Wait(2);

                rebar.PlaySound("talon_destroyed");
                rebar.RotateTo(new Vector3(50, 0, -25), 4, 0.5f, 1);
                rebar.MoveTo(rebar.Origin - new Vector3(0, 0, 50), 4, 0.5f, 1);

                yield return Wait(1);

                Vector3 dropImpulse = new Vector3(300, 50, -60);
                crate.PhysicsLaunchServer(new Vector3(0, 0, 0), dropImpulse);

                yield return Wait(1.5f);

                Entity crateSmoke = SpawnFX(AIZ.fx_crateSmoke, crate.Origin);
                TriggerFX(crateSmoke);

                yield return Wait(3);

                crateSmoke.Delete();
                rockCrumble.Delete();
                Smoke2.Delete();
                Smoke3.Delete();

                yield return Wait(1);
                makeUsable(crate, "perk1", 75);
            }
            else
            {
                StartAsync(initJuggAnim(crate, rebar));
                yield break;
            }
        }

        private static void createDoor(Vector3 open, Vector3 close, Vector3 angle, int size, int height, int range, int cost, Vector3? newSpawn = null, Vector3? newSpawnAngles = null)
        {
            double offset = (((size / 2) - 0.5) * -1);
            Entity center = Spawn("script_model", close);
            List<Entity> pieces = new List<Entity>();
            for (int j = 0; j < size; j++)
            {
                Entity door = spawnCrate(close + (new Vector3(0, 30, 0) * (float)offset), new Vector3(0, 0, 0), false, false);
                door.SetModel("com_plasticcase_enemy");
                door.EnableLinkTo();
                door.LinkTo(center);
                pieces.Add(door);
                for (int h = 1; h < height; h++)
                {
                    Entity door2 = spawnCrate(close + (new Vector3(0, 30, 0) * (float)offset) - (new Vector3(70, 0, 0) * h), new Vector3(0, 0, 0), false, false);
                    door2.SetModel("com_plasticcase_enemy");
                    door2.EnableLinkTo();
                    door2.LinkTo(center);
                    pieces.Add(door2);
                }
                offset += 1;
            }
            center.Angles = angle;
            center.SetField("open", open);
            center.SetField("close", close);
            center.SetField("cost", cost);
            center.SetField("pieces", new Parameter(pieces));

            makeUsable(center, "door", range);
            //center.Call(33529, new Parameter(center.GetField<Vector3>("close"))); // moveto
            center.SetField("state", "close");

            if (newSpawn != null)
            {
                center.SetField("spawn", newSpawn);
                center.SetField("spawnAngles", newSpawnAngles);
            }
        }

        private static void createWall(Vector3 start, Vector3 end, bool invisible, bool death)
        {
            float D = new Vector3(start.X, start.Y, 0).DistanceTo(new Vector3(end.X, end.Y, 0));
            float H = new Vector3(0, 0, start.Z).DistanceTo(new Vector3(0, 0, end.Z));
            int blocks = (int)Math.Round(D / 60, 0);
            int height = (int)Math.Round(H / 30, 0);

            Vector3 C = end - start;
            Vector3 A = new Vector3(C.X / blocks, C.Y / blocks, C.Z / height);
            float TXA = A.X / 4;
            float TYA = A.Y / 4;
            Vector3 angle = VectorToAngles(C);
            angle = new Vector3(0, angle.Y, 90);
            //Entity center = Spawn("script_origin", new Vector3(
            //(start.X + end.X) / 2, (start.Y + end.Y) / 2, (start.Z + end.Z) / 2));
            for (int h = 0; h < height; h++)
            {
                Entity crate = spawnCrate((start + new Vector3(TXA, TYA, 15) + (new Vector3(0, 0, A.Z) * h)), angle, invisible, death);
                //crate.EnableLinkTo();
                //crate.LinkTo(center);
                crate.WillNeverChange();
                if (death) OnInterval(200, () => setDeathWall(crate));
                for (int i = 0; i < blocks; i++)
                {
                    crate = spawnCrate(start + (new Vector3(A.X, A.Y, 0) * i) + new Vector3(0, 0, 15) + (new Vector3(0, 0, A.Z) * h), angle, invisible, death);
                    //crate.EnableLinkTo();
                    //crate.LinkTo(center);
                    crate.WillNeverChange();
                    if (death) OnInterval(200, () => setDeathWall(crate));
                }
                crate = spawnCrate(new Vector3(end.X, end.Y, start.Z) + new Vector3(TXA * -1, TYA * -1, 15) + (new Vector3(0, 0, A.Z) * h), angle, invisible, death);
                //crate.EnableLinkTo();
                //crate.LinkTo(center);
                crate.WillNeverChange();
                if (death) OnInterval(200, () => setDeathWall(crate));
            }
            //return center;
        }

        private static bool setDeathWall(Entity crate)
        {
            if (AIZ.gameEnded) return false;
            foreach (Entity player in Players)
            {
                if (player.IsAlive && player.Origin.DistanceTo(crate.Origin) < 60)
                    player.Suicide();
            }
            return true;
        }

        private static void createFloor(Vector3 corner1, Vector3 corner2, bool invisible, bool death)
        {
            float width = corner1.X - corner2.X;
            if (width < 0) width = width * -1;
            float length = corner1.Y - corner2.Y;
            if (length < 0) length = length * -1;

            int bwide = (int)Math.Round(width / 50, 0);
            int blength = (int)Math.Round(length / 30, 0);
            Vector3 C = corner2 - corner1;
            Vector3 A = new Vector3(C.X / bwide, C.Y / blength, 0);
            //Entity center = Spawn("script_origin", new Vector3(
            //(corner1.X + corner2.X) / 2, (corner1.Y + corner2.Y) / 2, corner1.Z));
            for (int i = 0; i < bwide; i++)
            {
                for (int j = 0; j < blength; j++)
                {
                    Entity crate = spawnCrate(corner1 + (new Vector3(A.X, 0, 0) * i) + (new Vector3(0, A.Y, 0) * j), new Vector3(0, 0, 0), invisible, death);
                    //crate.EnableLinkTo();
                    //crate.LinkTo(center);
                    crate.WillNeverChange();
                }
            }
            //return center;
        }

        private static void createElevator(Vector3 enter, Vector3 exit)
        {
            Entity flag = Spawn("script_model", enter);
            flag.SetModel(getAlliesFlagModel(AIZ._mapname));
            Entity flag2 = Spawn("script_model", exit);
            flag2.SetModel(getAxisFlagModel(AIZ._mapname));
            //Entity trigger = Spawn("trigger_radius", flag.Origin + new Vector3(0, 0, 50), 0, 50, 50);
            //trigger.Code_Classname = "trigger_teleport";
            //trigger.SetField("endPos", exit);

            OnInterval(200, () => watchElevator(enter, exit));
        }

        private static bool watchElevator(Vector3 enter, Vector3 exit)
        {
            foreach (Entity player in Players)
            {
                if (player.IsAlive && player.Origin.DistanceTo(enter) <= 50)
                {
                    player.SetOrigin(exit);
                }
            }
            return true;
        }

        private static void realElevator(Vector3 start, Vector3 angle, Vector3 end, Vector3 drop)
        {
            Entity elevator = spawnCrate(start, angle, false, false);
            elevator.SetField("startPos", start);
            elevator.SetField("endPos", end);
            elevator.SetField("dropPos", drop);
            elevator.SetField("isMoving", false);
            makeUsable(elevator, "elevator", 50);
        }

        private static void createTeleporter(Vector3 startPos, Vector3 startAngles, Vector3 endPos, Vector3 endAngles, Vector3 linkerPos, Vector3 linkerAngles, int time)
        {
            Entity teleporter = Spawn("script_model", startPos + new Vector3(0, 0, 45));
            teleporter.Angles = startAngles;
            teleporter.SetModel("tag_origin");
            teleporter.Hide();
            teleporter.EnableLinkTo();
            teleporter.SetField("isLinked", false);
            teleporter.SetField("endPos", endPos);
            teleporter.SetField("endAngles", endAngles);
            teleporter.SetField("teleTime", time);
            Entity[] floorsActive = new Entity[6];
            for (int i = 0; i < 6; i++)
            {
                Entity floor = spawnCrate(startPos, Vector3.Zero, false, false);
                floor.SetModel("com_plasticcase_enemy");
                Entity floorActive = Spawn("script_model", startPos);
                floorActive.SetModel("com_plasticcase_trap_bombsquad");
                floorActive.Hide();
                //floor.Call("solid");
                Vector3 offset = new Vector3(0, 0, 0);
                switch (i)
                {
                    case 0:
                        offset = new Vector3(28, 30, -45);
                        break;
                    case 1:
                        offset = new Vector3(-28, 30, -45);
                        break;
                    case 2:
                        offset = new Vector3(28, -30, -45);
                        break;
                    case 3:
                        offset = new Vector3(-28, -30, -45);
                        break;
                    case 4:
                        offset = new Vector3(28, 0, -45);
                        break;
                    case 5:
                        offset = new Vector3(-28, 0, -45);
                        break;
                }
                floor.LinkTo(teleporter, "tag_origin", offset);
                floorActive.LinkTo(teleporter, "tag_origin", offset);
                floorsActive[i] = floorActive;
            }
            teleporter.SetField("floorsActive", new Parameter(floorsActive));
            teleporter.SetField("state", 0);
            Entity linker = Spawn("script_model", linkerPos + new Vector3(0, 0, 45));
            linker.Angles = linkerAngles;
            linker.SetModel("weapon_radar");
            linker.SetField("teleporter", teleporter);
            //HudElem HeadIcon = HudElem.NewHudElem();
            //HeadIcon.X = linkerPos.X;
            //HeadIcon.Y = linkerPos.Y;
            //HeadIcon.Z = linkerPos.Z + 75;
            //HeadIcon.Alpha = 0.85f;
            //HeadIcon.SetShader("cardicon_illuminati", 3, 3);
            //HeadIcon.Call("setwaypoint", true, false);
            //linker.SetField("icon", new Parameter(HeadIcon));
            makeUsable(linker, "linker", 75);
            makeUsable(teleporter, "teleporter", 80);
        }

        private static void spawnModel(string model, Vector3 origin, Vector3 angles)
        {
            Entity ent = Spawn("script_model", origin);
            ent.SetModel(model);
            ent.Angles = angles;

            checkModelForAnim(ent);
            //return ent;
        }
        private static void checkModelForAnim(Entity model)
        {
            string modelName = model.Model;

            if (modelName.StartsWith("plastic_fence_"))
            {
                PreCacheMpAnim(modelName + "_med_01");
                model.ScriptModelPlayAnim(modelName + "_med_01");
            }
            else if (modelName.StartsWith("fence_tarp_"))
            {
                if (modelName != "fence_tarp_134x76")
                {
                    PreCacheMpAnim(modelName + "_med_01");
                    model.ScriptModelPlayAnim(modelName + "_med_01");
                }
                else if (modelName == "fence_tarp_134x76")
                {
                    PreCacheMpAnim(modelName + "_med_02");
                    model.ScriptModelPlayAnim(modelName + "_med_02");
                }

            }
            else if (modelName.StartsWith("oil_pump_jack"))
            {
                PreCacheMpAnim("oilpump_pump01");
                model.ScriptModelPlayAnim("oilpump_pump01");
            }
            else if (modelName.StartsWith("machinery_windmill"))
            {
                PreCacheMpAnim("windmill_spin_med");
                model.ScriptModelPlayAnim("windmill_spin_med");
            }
        }

        public static Entity spawnCrate(Vector3 origin, Vector3 angles, bool Invisible, bool Death)
        {
            Entity ent = Spawn("script_model", origin);
            if (!Invisible && !Death) ent.SetModel("com_plasticcase_friendly");
            ent.Angles = angles;
            if (!Death)
            {
                ent.CloneBrushModelToScriptModel(_airdropCollision);
                ent.SetContents(1);
                //ent.Solid();
            }
            return ent;
        }

        private static bool monitorFallDeath(int height)
        {
            if (AIZ.gameEnded) return false;
            foreach (Entity player in Players)
            {
                if (player.IsAlive)
                {
                    if (player.Origin.Z < height)
                        player.Suicide();
                }
            }
            return true;
        }

        private static void setupSpaceLimit(bool isX, float min, float max)
        {
            if (isX)
                OnInterval(200, () => watchSpaceLimit_xAxis(min, max));
            else
                OnInterval(200, () => watchSpaceLimit_yAxis(min, max));
        }

        private static bool watchSpaceLimit_xAxis(float min, float max)
        {
                foreach (Entity player in Players)
                {
                    if (player.IsAlive)
                    {
                        Vector3 origin = player.Origin;
                        if (origin.X < min) player.SetOrigin(new Vector3(min + 10, origin.Y, origin.Z));
                        else if (origin.X > max) player.SetOrigin(new Vector3(max - 10, origin.Y, origin.Z));
                    }
                }
                if (!AIZ.gameEnded) return true;
                else return false;
        }
        private static bool watchSpaceLimit_yAxis(float min, float max)
        {
            foreach (Entity player in Players)
            {
                if (player.IsAlive)
                {
                    Vector3 origin = player.Origin;
                    if (origin.Y < min) player.SetOrigin(new Vector3(origin.X, min + 10, origin.Z));
                    else if (origin.Y > max) player.SetOrigin(new Vector3(origin.X, max - 10, origin.Z));
                }
            }
            if (!AIZ.gameEnded) return true;
            else return false;
        }

        private static void spawnBank(Vector3 origin, Vector3 angles)
        {
            Entity ent = spawnCrate(origin, angles, false, false);
            makeUsable(ent, "bank", 75);
            //return ent;
        }
        #endregion

        #region use logic
        private static IEnumerator useBox(Entity box, Entity player)
        {
            if (player.SessionTeam == "axis" || player.GetField<bool>("isDown") || player.CurrentWeapon.Contains("killstreak") || AIZ.isWeaponDeathMachine(player.CurrentWeapon) || (player.CurrentWeapon.Contains("marker") && player.CurrentWeapon != "uav_strike_marker_mp") || player.CurrentWeapon == "none") yield break;
            if (player.IsSwitchingWeapon()) yield break;
            //Entity boxPlayer = box.GetField<Entity>("player");
            if (box.GetField<string>("state") == "waiting" && box.GetField<Entity>("player") == player)
            {
                int boxWeapon = box.GetField<int>("weapon");
                string name = weaponNames[boxWeapon];

                if (AIZ.isRayGun(name))
                {
                    box.PlaySound("copycat_steal_class");
                    AIZ.currentRayguns++;
                }
                else if (AIZ.isThunderGun(name))
                {
                    box.PlaySound("copycat_steal_class");
                    AIZ.currentThunderguns++;
                }

                if ((player.GetField<bool>("perk4bought") && player.GetField<bool>("newGunReady") && player.GetField<string>("perk4weapon") == "") || (player.GetField<bool>("perk4bought") && player.GetField<string>("perk4weapon") != name && player.CurrentWeapon == player.GetField<string>("perk4weapon")))
                    player.SetField("perk4weapon", name);

                if (AIZ.isThunderGun(player.CurrentWeapon))
                    AIZ.currentThunderguns--;
                else if (AIZ.isRayGun(player.CurrentWeapon))
                    AIZ.currentRayguns--;

                if (!player.HasWeapon(name) && !player.GetField<bool>("newGunReady"))
                {
                    if (name != "lightstick_mp")
                    {
                        AIZ.updatePlayerWeaponsList(player, player.CurrentWeapon, true);
                        player.TakeWeapon(player.CurrentWeapon);
                    }
                    else hud.updateAmmoHud(player, false);
                }
                player.GiveWeapon(name);
                player.GiveMaxAmmo(name);

                if (name != "lightstick_mp")
                {
                    player.SwitchToWeapon(name);
                    AIZ.updatePlayerWeaponsList(player, name);
                    player.SetField("newGunReady", false);
                }
                else hud.updateAmmoHud(player, false);
                player.PlayLocalSound("ammo_crate_use");
                Entity weaponEnt = box.GetField<Entity>("weaponent");
                //weaponEnt.Hide();
                weaponEnt.SetModel("viewmodel_metal_gear_gun");
                weaponEnt.HidePart("tag_silencer");
                weaponEnt.HidePart("tag_knife");
                weaponEnt.MoveTo(box.Origin + new Vector3(0, 0, 20), 1, .3f, .6f);
                //_destroyed = true;
                box.SetField("destroyed", true);
                box.SetField("state", "post_pickup");
                yield return Wait(2);
                box.SetField("state", "idle");
                yield break;
            }

            if (box.GetField<string>("state") != "idle") yield break;

            if (player.GetField<int>("cash") < 10 && sale)
            {
                player.IPrintLn(AIZ.gameStrings[240]);
                yield break;
            }
            else if (player.GetField<int>("cash") < 950 && !sale)
            {
                player.IPrintLn(AIZ.gameStrings[241]);
                yield break;
            }

            if (box.GetField<string>("state") == "idle")
            {
                if (sale)
                {
                    player.SetField("cash", player.GetField<int>("cash") - 10);
                    hud.scorePopup(player, -10);
                }
                else
                {
                    player.SetField("cash", player.GetField<int>("cash") - 950);
                    hud.scorePopup(player, -950);
                }
                hud.scoreMessage(player, AIZ.gameStrings[242]);
            }
            box.SetField("state", "inuse");
            player.PlayLocalSound("achieve_bomb");
            Entity weapon = box.GetField<Entity>("weaponEnt");
            //weapon.Origin = box.Origin;
            //weapon.Show();
            weapon.SetModel(weaponModels[0]);
            //weapon.Angles = angles;
            weapon.RotateTo(box.Angles, 1, 0, .5f);
            //_destroyed = false;
            box.SetField("destroyed", false);

            boxCounter = 0;
            boxIndex = 0;

            weapon.MoveTo(box.Origin + new Vector3(0, 0, 40), 3, 0, .5f);

            OnInterval(50, () => box_rollWeapon(weapon));

            yield return Wait(3);

            bool isBear = RandomInt(boxMaxUses) == boxMaxUses - 1;//Random number is max
            if (isBear && boxMaxUses < 13 && !sale)
            {
                if (AIZ._mapname != "mp_six_ss") weapon.Angles -= new Vector3(0, 90, 0);
                StartAsync(moveWeaponBox(box, weapon));
                //give player back their 'hard earned' moo-lah
                player.SetField("cash", player.GetField<int>("cash") + 950);
                hud.scorePopup(player, 950);
                yield break;
            }
            else if (!sale) boxMaxUses--;

            if ((localizedNames[boxIndex] == "Ray Gun" && (AIZ.currentRayguns >= AIZ.maxRayguns || (player.HasWeapon("iw5_skorpion_mp_eotechsmg_scope07") || player.HasWeapon("iw5_skorpion_mp_eotechsmg_xmags_scope07"))))
            || (localizedNames[boxIndex] == "Thundergun" && (AIZ.currentThunderguns >= AIZ.maxThunderguns || (player.HasWeapon("uav_strike_missile_mp") || player.HasWeapon("uav_strike_projectile_mp")))))
            {
                boxIndex = (byte)AIZ.rng.Next(weaponModels.Length - 2);
                weapon.SetModel(weaponModels[boxIndex]);
            }

            if (player.HasWeapon(weaponNames[boxIndex]) || AIZ.hasUpgradedWeapon(player, AIZ.getWeaponUpgrade(weaponNames[boxIndex])))//Just reroll
            {
                boxIndex = (byte)AIZ.rng.Next(weaponModels.Length);
                weapon.SetModel(weaponModels[boxIndex]);
                if (player.HasWeapon(weaponNames[boxIndex]) || AIZ.hasUpgradedWeapon(player, AIZ.getWeaponUpgrade(weaponNames[boxIndex])))//If again, reroll with seperate RNG
                {
                    boxIndex = (byte)RandomInt(weaponModels.Length);
                    weapon.SetModel(weaponModels[boxIndex]);
                }
            }

            box.SetField("state", "waiting");
            box.SetField("weapon", boxIndex);
            weapon.SetModel(weaponModels[boxIndex]);
            weapon.MoveTo(box.Origin + new Vector3(0, 0, 20), 10, 0, 1);
            box.SetField("player", player);

            yield return Wait(11);

            if (box.GetField<string>("state") != "idle" && weapon.Origin.Equals(box.Origin + new Vector3(0, 0, 20)))
            {
                if (!box.GetField<bool>("destroyed"))
                {
                    box.SetField("state", "idle");
                    box.SetField("weapon", 0);
                    box.SetField("destroyed", true);
                    //weapon.Hide();
                    weapon.SetModel("viewmodel_metal_gear_gun");
                    weapon.HidePart("tag_silencer");
                    weapon.HidePart("tag_knife");
                }
            }
        }
        private static bool box_rollWeapon(Entity weapon)
        {
            boxIndex = (byte)AIZ.rng.Next(weaponModels.Length);
            weapon.SetModel(weaponModels[boxIndex]);
            boxCounter++;
            if (boxCounter == 60) return false;
            return true;
        }

        private static IEnumerator moveWeaponBox(Entity box, Entity bear)
        {
            box.CloneBrushModelToScriptModel(bear);
            box.PlaySound("mp_last_stand");
            usables.Remove(box);
            bear.MoveTo(bear.Origin + new Vector3(0, 0, 60), 3, 1);
            bear.SetModel(teddyModel);
            box.SetField("isRotating", true);

            if (box.HasField("fx_xmas"))
            {
                Array.ForEach(box.GetField<Entity[]>("fx_xmas"), (fx) => fx.Delete());
                box.ClearField("fx_xmas");
            }

            yield return Wait(3);

            PlayFX(AIZ.fx_disappear, bear.Origin);
            bear.Delete();
            box.MoveTo(box.Origin + new Vector3(0, 0, 80), 5, 3);

            box.RotatePitch(-25, .35f, .15f, .15f);

            yield return Wait(.4f);

            OnInterval(800, () => rotateBoxLoop(box));

            yield return Wait(4.6f);

            box.ClearField("isRotating");
            PlayFX(AIZ.fx_disappear, box.Origin);
            int objID = box.GetField<int>("objID");
            Objective_Delete(objID);
            _objIDs.Remove(box);//Removing from internal list only to avoid overwrite
            box.Delete();
            int newLoc = AIZ.rng.Next(boxLocations.Length);
            if (newLoc == box.GetField<int>("lastLocation"))//Reroll
                newLoc = AIZ.rng.Next(boxLocations.Length);

            //Log.Write(LogLevel.All, "New box location: {0}", newLoc);
            boxMaxUses = 15;//Reset uses
            yield return Wait(1.5f);
            randomWeaponCrate(boxLocations[newLoc][0], boxLocations[newLoc][1], objID, newLoc);
        }
        private static bool rotateBoxLoop(Entity box)
        {
            if (!box.HasField("isRotating")) return false;
            box.RotatePitch(50, .35f, .15f, .15f);
            AfterDelay(400, () => box.RotatePitch(-50, .35f, .15f, .15f));
            return true;
        }

        private static IEnumerator usePapBox(Entity box, Entity player, string currentGun)
        {
            if (player.SessionTeam != "allies") yield break;
            if (player.CurrentWeapon.Contains("killstreak")) yield break;
            if (player.IsSwitchingWeapon()) yield break;
            //if (currentGun == "" || currentGun == "none") return;//If we have no gun, no PAP

            string gun = box.GetField<string>("weapon");

            if (box.GetField<string>("state") == "waiting" && box.GetField<Entity>("player") == player)
            {
                player.GiveWeapon(gun);
                player.GiveMaxAmmo(gun);
                player.SwitchToWeapon(gun);
                //player.SetField("newGunReady", false);

                if (gun == "gl_mp")
                {
                    player.SetField("newGunReady", true);
                    if (player.GetField<string>("perk4weapon") == "m320_mp") player.SetField("perk4weapon", "");
                }
                else AIZ.updatePlayerWeaponsList(player, gun);

                if (player.GetField<string>("perk4weapon") == box.GetField<string>("oldWeapon")) player.SetField("perk4weapon", gun);

                player.PlayLocalSound("oldschool_pickup");
                Entity weaponEnt = box.GetField<Entity>("weaponEnt");
                Entity[] attachments = box.GetField<Entity[]>("attachments");
                foreach (Entity a in attachments) { a.SetModel("tag_origin"); a.Hide(); }
                weaponEnt.SetModel("tag_origin");
                weaponEnt.Hide();

                yield return Wait(1);//Wait a second until being ready again to fix the rapid upgrade bug

                box.SetField("destroyed", true);
                box.SetField("state", "idle");
                box.SetField("player", Entity.Level);
                yield break;
            }

            if (!AIZ.powerActivated || player.GetField<bool>("isDown")) yield break;

            if (box.GetField<string>("state") != "idle") yield break;
            if (player.GetField<int>("cash") < 5000) yield break;
            if (AIZ.getWeaponUpgrade(currentGun) == string.Empty) yield break;//Don't PAP already PAPed guns
            if (box.GetField<string>("state") == "idle")
            {
                player.SetField("cash", player.GetField<int>("cash") - 5000);
                hud.scorePopup(player, -5000);
                hud.scoreMessage(player, AIZ.gameStrings[243]);
            }
            box.SetField("state", "inuse");
            box.SetField("player", player);
            Entity weapon = box.GetField<Entity>("weaponEnt");
            weapon.Angles = box.Angles;
            string weaponModel = GetWeaponModel(currentGun);
            weapon.Show();
            weapon.SetModel(weaponModel);
            weapon.Origin = box.Origin + new Vector3(0, 0, 40);
            string upgradeWeapon = AIZ.getWeaponUpgrade(currentGun);
            if (AIZ.weaponHasOptic(upgradeWeapon) && upgradeWeapon != "iw5_skorpion_mp_eotechsmg_xmags_scope7" && upgradeWeapon != "iw5_m60jugg_mp_thermal_silencer_camo08") upgradeWeapon += "_scope" + AIZ.rng.Next(0, 6);
            box.SetField("weapon", upgradeWeapon);
            box.SetField("oldWeapon", player.CurrentWeapon);
            AIZ.updatePlayerWeaponsList(player, player.CurrentWeapon, true);
            player.TakeWeapon(player.CurrentWeapon);
            //player.SetField("newGunReady", true);
            if (player.GetField<List<string>>("weaponsList").Count != 0)
                player.SwitchToWeapon(player.GetField<List<string>>("weaponsList")[0]);
            box.SetField("destroyed", false);
            box.PlaySound("elev_run_start");
            yield return Wait(1);

            //box.PlaySound("elev_run_start");
            box.PlayLoopSound("elev_run_loop");
            weapon.MoveTo(box.Origin + new Vector3(0, 0, 10), 2);
            yield return Wait(2);

            string wep = box.GetField<string>("weapon");
            weapon.SetModel(AIZ.getWeaponUpgradeModel(currentGun));

            //Check attachments

            string tag = "tag_origin";
            Vector3 tagOffset = Vector3.Zero;
            string model = "tag_origin";

            foreach (string a in AIZ.getWeaponAttachments(wep))
            {
                switch (a)
                {
                    case "reflex":
                    case "reflexsmg":
                    case "reflexlmg":
                        tag = "tag_red_dot";
                        model = "weapon_reflex_iw5";
                        break;
                    case "acog":
                    case "acogsmg":
                    case "acoglmg":
                        tag = "tag_acog_2";
                        model = "weapon_acog";
                        break;
                    case "grip":
                        tag = "tag_foregrip";
                        model = "weapon_remington_foregrip";
                        break;
                    case "akimbo":
                        tag = "tag_weapon";
                        tagOffset = new Vector3(6, 5, 2);
                        model = weapon.Model;
                        break;
                    case "thermal":
                    case "thermalsmg":
                    case "thermallmg":
                        tag = "tag_thermal_scope";
                        model = "weapon_thermal_scope";
                        break;
                    case "shotgun":
                        tag = "tag_shotgun";
                        model = "weapon_shotgun";
                        break;
                    case "heartbeat":
                        tag = "tag_heartbeat";
                        model = "weapon_heartbeat_iw5";
                        break;
                    case "eotech":
                    case "eotechsmg":
                    case "eotechlmg":
                        tag = "tag_eotech";
                        model = "weapon_eotech";
                        break;
                    case "gl":
                        tag = "tag_m203";
                        model = "weapon_m203";
                        break;
                    case "gp25":
                        tag = "tag_gp25";
                        model = "weapon_gp25";
                        break;
                    case "m320":
                        tag = "tag_m320";
                        model = "weapon_m320";
                        break;
                    case "silencer":
                        tag = "tag_flash";
                        model = "weapon_silencer_01";
                        break;
                    case "silencer02":
                        tag = "tag_flash";
                        model = "weapon_silencer_02";
                        break;
                    case "silencer03":
                        tag = "tag_flash";
                        model = "weapon_silencer_03";
                        break;
                    case "hamrhybrid":
                        tag = "tag_hamr_hybrid";
                        model = "weapon_hamr_hybrid";
                        break;
                    case "hybrid":
                        tag = "tag_magnifier";
                        model = "weapon_magnifier";
                        break;
                    case "tactical":
                        tag = "tag_weapon";
                        model = "weapon_parabolic_knife";
                        tagOffset = new Vector3(2, 0, -3);
                        break;
                    case "dragunovscopevz":
                    case "rsassscopevz":
                    case "as50scopevz":
                    case "dragunovscope":
                    case "rsassscope":
                    case "as50scope":
                        tag = "tag_" + wep.Split('_')[1] + "_scope";
                        model = "weapon_" + wep.Split('_')[1] + "_scope_iw5";
                        break;
                    case "l96a1scope":
                    case "l96a1scopevz":
                        tag = "tag_scope";
                        model = "weapon_" + wep.Split('_')[1] + "_scope_iw5";
                        break;
                    case "barrettscope":
                    case "barrettscopevz":
                        tag = "tag_m82_scope";
                        model = "weapon_m82_scope_iw5";
                        break;
                    case "msrscope":
                    case "msrscopevz":
                        tag = "tag_scope";
                        model = "weapon_remington_" + wep.Split('_')[1] + "_scope_iw5";
                        break;
                }

                Vector3 tagOrigin = weapon.GetTagOrigin(tag);
                Vector3 tagAngles = weapon.GetTagAngles(tag);
                Entity[] attachEnts = box.GetField<Entity[]>("attachments");

                if (attachEnts[0].Model == "tag_origin")
                {
                    attachEnts[0].Show();
                    attachEnts[0].Unlink();
                    attachEnts[0].Angles = tagAngles;
                    attachEnts[0].SetModel(model);
                    attachEnts[0].Origin = tagOrigin + tagOffset;
                    attachEnts[0].LinkTo(weapon, tag, tagOffset);
                }
                else
                {
                    attachEnts[1].Show();
                    attachEnts[1].Unlink();
                    attachEnts[1].Angles = tagAngles;
                    attachEnts[1].SetModel(model);
                    attachEnts[1].Origin = tagOrigin + tagOffset;
                    attachEnts[1].LinkTo(weapon, tag, tagOffset);
                }

                box.SetField("attachments", new Parameter(attachEnts));
            }

            weapon.MoveTo(box.Origin + new Vector3(0, 0, 60), 2);
            yield return Wait(1);

            box.StopLoopSound();
            box.PlaySound("elev_run_end");
            yield return Wait(1);

            box.PlaySound("elev_bell_ding");
            box.SetField("state", "waiting");
            weapon.MoveTo(box.Origin + new Vector3(0, 0, 25), 10);
            yield return Wait(10.5f);

            if (weapon.Model == "tag_origin") yield break;
            if (box.GetField<string>("state") != "idle" && box.GetField<string>("weapon") == upgradeWeapon && box.GetField<Entity>("player") == player)
            {
                if (!box.GetField<bool>("destroyed"))
                {
                    foreach (Entity a in box.GetField<Entity[]>("attachments"))
                    {
                        a.SetModel("tag_origin");
                        a.Hide();
                    }
                    weapon.SetModel("tag_origin");
                    weapon.Hide();
                    box.SetField("state", "idle");
                    box.SetField("destroyed", true);
                    box.SetField("player", Entity.Level);
                    player.SetField("newGunReady", true);
                }
            }
        }

        private static void useGambler(Entity box, Entity player)
        {
            if (player.SessionTeam != "allies") return;
            if (box.GetField<bool>("GamblerInUse"))
            {
                player.IPrintLn(AIZ.gameStrings[244]);
                return;
            }
            if (!player.GetField<bool>("GamblerReady") && !box.GetField<bool>("GamblerInUse"))
            {
                player.IPrintLnBold(AIZ.gameStrings[245]);
                return;
            }
            if (player.GetField<int>("cash") >= 1000)
            {
                player.SetField("cash", player.GetField<int>("cash") - 1000);
                hud.scorePopup(player, -1000);
                hud.scoreMessage(player, AIZ.gameStrings[246]);
                box.SetField("GamblerInUse", true);
                player.SetField("GamblerReady", false);
                //level.Notify("use_gambler");
                player.IPrintLnBold(AIZ.gameStrings[248]);
                StartAsync(gamblerCountdown(player));
                Entity laptop = box.GetField<Entity>("laptop");
                laptop.MoveTo(box.Origin + new Vector3(0, 0, 38), 4);
                AfterDelay(8500, () => laptop.MoveTo(box.Origin + new Vector3(0, 0, 22), 4));
                AfterDelay(12500, () => gamblerRoll(player, box));
            }
        }

        private static void gamblerRoll(Entity player, Entity box)
        {
            box.SetField("GamblerInUse", false);
            if (!player.IsAlive) return;
            switch (AIZ.rng.Next(22))
            {
                case 0:
                    //Extra weapon
                    player.GiveWeapon("defaultweapon_mp");
                    AIZ.updatePlayerWeaponsList(player, "defaultweapon_mp");
                    StartAsync(AIZ.switchToWeapon_delay(player, "defaultweapon_mp", .1f));
                    player.IPrintLnBold(string.Format(AIZ.gameStrings[249], "an extra weapon slot"));
                    
                    break;
                case 1:
                    //500 points
                    player.SetField("cash", player.GetField<int>("cash") + 500);
                    hud.scorePopup(player, 500);
                    player.IPrintLnBold(string.Format(AIZ.gameStrings[249], "500 points"));
                    
                    player.PlayFX(AIZ.fx_money, player.Origin);
                    break;
                case 2:
                    //1000 points
                    player.SetField("cash", player.GetField<int>("cash") + 1000);
                    hud.scorePopup(player, 1000);
                    player.IPrintLnBold(string.Format(AIZ.gameStrings[249], "1000 points"));
                    
                    player.PlayFX(AIZ.fx_money, player.Origin);
                    break;
                case 3:
                    //1500 points
                    player.SetField("cash", player.GetField<int>("cash") + 1500);
                    hud.scorePopup(player, 1500);
                    player.IPrintLnBold(string.Format(AIZ.gameStrings[249], "1500 points"));
                    
                    player.PlayFX(AIZ.fx_money, player.Origin);
                    break;
                case 4:
                    //2000 points
                    player.SetField("cash", player.GetField<int>("cash") + 2000);
                    hud.scorePopup(player, 2000);
                    player.IPrintLnBold(string.Format(AIZ.gameStrings[249], "2000 points"));
                    
                    player.PlayFX(AIZ.fx_money, player.Origin);
                    break;
                case 5:
                    //5000 points
                    player.SetField("cash", player.GetField<int>("cash") + 5000);
                    hud.scorePopup(player, 5000);
                    player.IPrintLnBold(string.Format(AIZ.gameStrings[249], "5000 points"));
                    
                    player.PlayFX(AIZ.fx_money, player.Origin);
                    break;
                case 6:
                    //7500 points
                    player.SetField("cash", player.GetField<int>("cash") + 7500);
                    hud.scorePopup(player, 7500);
                    player.IPrintLnBold(string.Format(AIZ.gameStrings[249], "7500 points"));
                    
                    player.PlayFX(AIZ.fx_money, player.Origin);
                    break;
                case 7:
                    //10000 points
                    player.SetField("cash", player.GetField<int>("cash") + 10000);
                    hud.scorePopup(player, 10000);
                    player.IPrintLnBold(string.Format(AIZ.gameStrings[249], "10000 points"));
                    
                    player.PlayFX(AIZ.fx_money, player.Origin);
                    break;
                case 8:
                    //lose 500
                    player.SetField("cash", player.GetField<int>("cash") - 500);
                    if (player.GetField<int>("cash") < 0)
                        player.SetField("cash", 0);
                    hud.scorePopup(player, -500);
                    player.IPrintLnBold(string.Format(AIZ.gameStrings[250], "500 points"));
                    
                    break;
                case 9:
                    //lose all perks
                    if (!player.HasField("allPerks"))
                    {
                        if (player.GetField<bool>("perk1bought"))
                        {
                            player.MaxHealth = 100;
                            player.Health = 100;
                            player.SetField("perk1bought", false);
                        }
                        if (player.GetField<bool>("perk2bought"))
                        {
                            player.UnSetPerk("specialty_lightweight");
                            //player.UnSetPerk("specialty_marathon");
                            player.UnSetPerk("specialty_longersprint");
                            player.SetField("perk2bought", false);
                        }
                        if (player.GetField<bool>("perk3bought"))
                        {
                            player.UnSetPerk("specialty_fastreload");
                            //player.UnSetPerk("specialty_quickswap");
                            player.UnSetPerk("specialty_quickdraw");
                            player.SetField("perk3bought", false);
                        }
                        if (player.GetField<bool>("perk4bought"))
                        {
                            if (player.HasField("perk4weapon") && player.HasWeapon(player.GetField<string>("perk4weapon")))
                            {
                                string perk4weapon = player.GetField<string>("perk4weapon");
                                if (AIZ.isThunderGun(perk4weapon))
                                    AIZ.currentThunderguns--;
                                else if (AIZ.isRayGun(perk4weapon))
                                    AIZ.currentRayguns--;
                                player.TakeWeapon(perk4weapon);
                                AIZ.updatePlayerWeaponsList(player, perk4weapon, true);
                            }
                            else
                            {
                                if (AIZ.isThunderGun(player.CurrentWeapon))
                                    AIZ.currentThunderguns--;
                                else if (AIZ.isRayGun(player.CurrentWeapon))
                                    AIZ.currentRayguns--;
                                player.TakeWeapon(player.CurrentWeapon);
                                AIZ.updatePlayerWeaponsList(player, player.CurrentWeapon, true);
                            }
                            player.SetField("perk4bought", false);
                            player.SetField("perk4weapon", "");
                        }
                        if (player.GetField<bool>("perk5bought"))
                        {
                            player.UnSetPerk("specialty_rof");
                            player.SetField("perk5bought", false);
                        }
                        if (player.GetField<bool>("perk6bought"))
                        {
                            player.UnSetPerk("specialty_stalker");
                            player.SetField("perk6bought", false);
                        }
                        if (player.GetField<int>("perk7bought") > 0)
                            player.SetField("autoRevive", false);

                        hud.updatePerksHud(player, true);
                        player.SetField("totalPerkCount", 0);
                    }
                    player.IPrintLnBold(string.Format(AIZ.gameStrings[250], "all of your perks"));
                    
                    break;
                case 10:
                    //lose all perks and 200 points
                    player.SetField("cash", player.GetField<int>("cash") - 200);
                    if (player.GetField<int>("cash") < 0)
                        player.SetField("cash", 0);
                    hud.scorePopup(player, -200);

                    if (!player.HasField("allPerks"))
                    {
                        if (player.GetField<bool>("perk1bought"))
                        {
                            player.MaxHealth = 100;
                            player.Health = 100;
                            player.SetField("perk1bought", false);
                        }
                        if (player.GetField<bool>("perk2bought"))
                        {
                            player.UnSetPerk("specialty_lightweight");
                            //player.UnSetPerk("specialty_marathon");
                            player.UnSetPerk("specialty_longersprint");
                            player.SetField("perk2bought", false);
                        }
                        if (player.GetField<bool>("perk3bought"))
                        {
                            player.UnSetPerk("specialty_fastreload");
                            //player.UnSetPerk("specialty_quickswap");
                            player.UnSetPerk("specialty_quickdraw");
                            player.SetField("perk3bought", false);
                        }
                        if (player.GetField<bool>("perk4bought"))
                        {
                            if (player.HasField("perk4weapon") && player.HasWeapon(player.GetField<string>("perk4weapon")))
                            {
                                string perk4weapon = player.GetField<string>("perk4weapon");
                                if (AIZ.isThunderGun(perk4weapon))
                                    AIZ.currentThunderguns--;
                                else if (AIZ.isRayGun(perk4weapon))
                                    AIZ.currentRayguns--;
                                player.TakeWeapon(perk4weapon);
                                AIZ.updatePlayerWeaponsList(player, perk4weapon, true);
                            }
                            else
                            {
                                if (player.CurrentWeapon == "uav_strike_missile_mp" || player.CurrentWeapon == "uav_strike_projectile_mp")
                                    AIZ.currentThunderguns--;
                                else if (player.CurrentWeapon == "iw5_skorpion_mp_eotechsmg_scope7" || player.CurrentWeapon == "iw5_skorpion_mp_eotechsmg_xmags_scope7")
                                    AIZ.currentRayguns--;
                                player.TakeWeapon(player.CurrentWeapon);
                                AIZ.updatePlayerWeaponsList(player, player.CurrentWeapon, true);
                            }
                            player.SetField("perk4bought", false);
                            player.SetField("perk4weapon", "");
                        }
                        if (player.GetField<bool>("perk5bought"))
                        {
                            player.UnSetPerk("specialty_rof");
                            player.SetField("perk5bought", false);
                        }
                        if (player.GetField<bool>("perk6bought"))
                        {
                            player.UnSetPerk("specialty_stalker");
                            player.SetField("perk6bought", false);
                        }
                        if (player.GetField<int>("perk7bought") > 0)
                            player.SetField("autoRevive", false);

                        hud.updatePerksHud(player, true);
                        player.SetField("totalPerkCount", 0);
                    }
                    player.IPrintLnBold(string.Format(AIZ.gameStrings[250], "all of your perks and 200 points"));
                    
                    break;
                case 11:
                    //double health
                    player.IPrintLnBold(AIZ.gameStrings[251]);
                    StartAsync(AIZ.setTempHealth(player, player.Health * 2, 30, AIZ.gameStrings[252]));
                    
                    break;
                case 12:
                    //inf health
                    player.IPrintLnBold(AIZ.gameStrings[253]);
                    StartAsync(AIZ.setTempHealth(player, 999999999, 30, AIZ.gameStrings[254]));
                    
                    break;
                case 13:
                    //model1887
                    player.IPrintLnBold(string.Format(AIZ.gameStrings[249], "a Model 1887"));

                    if (AIZ.isThunderGun(player.CurrentWeapon))
                        AIZ.currentThunderguns--;
                    else if (AIZ.isRayGun(player.CurrentWeapon))
                        AIZ.currentRayguns--;

                    player.TakeWeapon(player.CurrentWeapon);
                    player.GiveWeapon("iw5_1887_mp");
                    AIZ.updatePlayerWeaponsList(player, "iw5_1887_mp");
                    StartAsync(AIZ.switchToWeapon_delay(player, "iw5_1887_mp", .1f));
                    
                    break;
                case 14:
                    //max ammo
                    player.IPrintLnBold(AIZ.gameStrings[255]);
                    int roll = AIZ.rng.Next(2);

                    if (roll == 0)
                    {
                        AfterDelay(1500, () => AIZ.giveMaxAmmo(player));
                        AfterDelay(1500, () => player.IPrintLnBold(AIZ.gameStrings[256]));
                    }
                    else
                        AfterDelay(1500, () => player.IPrintLnBold(AIZ.gameStrings[257]));

                    
                    break;
                case 15:
                    //lose 1000
                    player.SetField("cash", player.GetField<int>("cash") - 1000);
                    if (player.GetField<int>("cash") < 0)
                        player.SetField("cash", 0);
                    hud.scorePopup(player, -1000);
                    player.IPrintLnBold(string.Format(AIZ.gameStrings[250], "1000 points"));
                    
                    break;
                case 16:
                    //lose all $
                    int cash = -player.GetField<int>("cash");
                    player.SetField("cash", 0);
                    hud.scorePopup(player, cash);
                    player.IPrintLnBold(string.Format(AIZ.gameStrings[250], "all of your points"));
                    
                    break;
                case 17:
                    //live or die
                    player.IPrintLnBold(AIZ.gameStrings[258]);
                    StartAsync(gambler_determineDeath(player));
                    
                    break;
                case 18:
                    player.IPrintLnBold(string.Format(AIZ.gameStrings[247], "nothing"));
                    
                    break;
                case 19:
                    player.IPrintLnBold(string.Format(AIZ.gameStrings[249], "an extra free perk"));
                    StartAsync(bonusDrops.giveRandomPerk(player));
                    
                    break;
                case 20:
                    player.IPrintLnBold(string.Format(AIZ.gameStrings[250], "your current weapon"));
                    if (AIZ.mayDropWeapon(player.CurrentWeapon) && player.GetField<List<string>>("weaponsList").Count > 1)
                    {
                        AIZ.updatePlayerWeaponsList(player, player.CurrentWeapon, true);
                        if (AIZ.isRayGun(player.CurrentWeapon)) AIZ.currentRayguns--;
                        else if (AIZ.isThunderGun(player.CurrentWeapon)) AIZ.currentThunderguns--;
                        player.TakeWeapon(player.CurrentWeapon);
                        List<string> weaponsList = player.GetField<List<string>>("weaponsList");
                        StartAsync(AIZ.switchToWeapon_delay(player, weaponsList[0], .4f));
                        player.SetField("newGunReady", true);
                    }
                    
                    break;
                case 21:
                    player.IPrintLnBold(string.Format(AIZ.gameStrings[249], "a random killstreak"));
                    int randomStreak = AIZ.rng.Next(1, 12);
                    killstreaks.giveKillstreak(player, randomStreak);
                    
                    break;
                default:
                    player.IPrintLnBold(string.Format(AIZ.gameStrings[247], "nothing"));
                    
                    break;
            }
        }

        private static IEnumerator gambler_determineDeath(Entity player)
        {
            yield return Wait(1.5f);
            player.IPrintLnBold("^24");
            yield return Wait(1);
            player.IPrintLnBold("^23");
            yield return Wait(1);
            player.IPrintLnBold("^22");
            yield return Wait(1);
            player.IPrintLnBold("^21");
            yield return Wait(1);
            int roll = AIZ.rng.Next(4);
            switch (roll)
            {
                case 2:
                    AfterDelay(0, () => player.Suicide());
                    break;
                default:
                    player.IPrintLnBold(AIZ.gameStrings[259]);
                    break;
            }
        }

        private static IEnumerator gamblerCountdown(Entity player)
        {
            yield return Wait(1.5f);
            player.IPrintLnBold("^210");
            yield return Wait(1);
            player.IPrintLnBold("^29");
            yield return Wait(1);
            player.IPrintLnBold("^28");
            yield return Wait(1);
            player.IPrintLnBold("^27");
            yield return Wait(1);
            player.IPrintLnBold("^26");
            yield return Wait(1);
            player.IPrintLnBold("^25");
            yield return Wait(1);
            player.IPrintLnBold("^24");
            yield return Wait(1);
            player.IPrintLnBold("^23");
            yield return Wait(1);
            player.IPrintLnBold("^22");
            //level.Notify("gambler_done");
            yield return Wait(1);
            player.IPrintLnBold("^21");
        }

        private static void useAmmo(Entity box, Entity player)
        {
            if (player.SessionTeam != "allies" || box.GetField<bool>("used")) return;
            int cost = 4500 + player.GetField<int>("ammoCostAddition");
            if (player.GetField<int>("cash") < cost)
            {
                player.IPrintLn(AIZ.gameStrings[260] + cost.ToString());
                box.SetField("used", true);
                AfterDelay(1000, () =>
                    box.SetField("used", false));
                return;
            }
            else
            {
                player.SetField("cash", player.GetField<int>("cash") - cost);
                hud.scorePopup(player, -cost);
                hud.scoreMessage(player, AIZ.gameStrings[261]);
                player.SetField("ammoCostAddition", player.GetField<int>("ammoCostAddition") * 2);
                if (player.GetField<int>("ammoCostAddition") == 0) player.SetField("ammoCostAddition", 500);
                box.SetField("used", true);
                AIZ.giveMaxAmmo(player);
                player.PlayLocalSound("ammo_crate_use");
                AfterDelay(1000, () =>
                    box.SetField("used", false));
            }
        }

        private static void useKillstreak(Entity box, Entity player)
        {
            if (!AIZ.powerActivated) return;
            if (player.SessionTeam != "allies") return;
            if (!player.HasField("aizHud_created")) return;
            if (player.GetField<int>("points") < 200) return;
            else
            {
                player.SetField("points", player.GetField<int>("points") - 200);
                hud.scorePopup(player, -200);
                hud.scoreMessage(player, AIZ.gameStrings[262]);
                HudElem pointNumber = player.GetField<HudElem>("hud_pointNumber");
                int points = player.GetField<int>("points");
                pointNumber.SetValue(points);
                int randomKS = AIZ.rng.Next(1, 12);
                killstreaks.giveKillstreak(player, randomKS);
            }
        }

        public static void useBank(Entity player, bool isWithdraw)
        {
            if (!hud.powerBox && !AIZ.isHellMap) return;//BS 12/1/21 - Changing this to require the power box instead of power in favor of keeping EMPs from enabling this
            if (player.SessionTeam != "allies" || !player.IsAlive) return;
            if (!isWithdraw)
            {
                if (player.GetField<int>("cash") < 1000) return;
                int totalBalance = (int)player.GetPlayerData("money");

                if (totalBalance >= maxBankBalance)
                {
                    if (totalBalance > maxBankBalance)//Reset bank to max if players are above max
                    {
                        totalBalance = maxBankBalance;
                        player.SetPlayerData("money", totalBalance);
                    }
                    player.IPrintLnBold(string.Format(AIZ.gameStrings[328]));
                    return;
                }

                totalBalance += 1000;
                //Log.Write(LogLevel.All, "tmpBalance: {0}", totalBalance);
                player.SetField("cash", player.GetField<int>("cash") - 1000);
                hud.scorePopup(player, -1000);
                hud.scoreMessage(player, "Bank Deposit!");//Quick fix, no localized string for this for some reason
                player.SetPlayerData("money", totalBalance);
                player.IPrintLnBold(string.Format(AIZ.gameStrings[263], totalBalance));
            }
            else
            {
                if ((int)player.GetPlayerData("money") < 1000)
                {
                    player.IPrintLnBold(string.Format(AIZ.gameStrings[263], 0));
                    return;
                }
                int totalBalance = (int)player.GetPlayerData("money");
                totalBalance -= 1000;
                if (totalBalance > maxBankBalance)//Reset bank to max if players are above max
                {
                    totalBalance = maxBankBalance;
                }
                //Log.Write(LogLevel.All, "tmpBalance: {0}", totalBalance);
                player.SetField("cash", player.GetField<int>("cash") + 900);
                hud.scorePopup(player, 900);
                hud.scoreMessage(player, AIZ.gameStrings[264]);
                player.SetPlayerData("money", totalBalance);
                player.IPrintLnBold(string.Format(AIZ.gameStrings[263], totalBalance));
            }
        }

        private static IEnumerator linkTeleporter(Entity linker, Entity player)
        {
            if (player.SessionTeam != "allies") yield break;
            if (!AIZ.powerActivated) yield break;
            Entity teleporter = linker.GetField<Entity>("teleporter");
            if (teleporter.GetField<bool>("isLinked")) yield break;
            player.PlaySound("item_nightvision_on");
            teleporter.SetField("isLinked", true);
            PlayFX(AIZ.fx_sparks, linker.Origin, new Vector3(0, linker.Angles.Y, 90));
            yield return Wait(1);
            teleporter.PlaySound("item_nightvision_on");
            Entity[] teleporterFloors = teleporter.GetField<Entity[]>("floorsActive");
            foreach (Entity f in teleporterFloors)
                f.Show();
        }

        private static IEnumerator playTeleporterSounds(Entity teleporter)
        {
            yield return Wait(1);
            PlaySoundAtPos(teleporter.GetField<Vector3>("endPos"), "emp_grenade_detonate");
            yield return Wait(teleporter.GetField<int>("teleTime"));

            teleporter.PlaySound("ims_trigger");
            yield return Wait(1);
            teleporter.PlaySound("emp_grenade_detonate");
        }
        private static IEnumerator useTeleporter(Entity teleporter, Entity user)
        {
            if (user.SessionTeam != "allies") yield break;
            if (!teleporter.GetField<bool>("isLinked") || teleporter.GetField<int>("state") != 0) yield break;
            teleporter.PlaySound("mine_betty_spin");
            PlayFX(AIZ.fx_teleportSpark, teleporter.Origin);
            teleporter.SetField("state", 1);//Cooldown
            StartAsync(playTeleporterSounds(teleporter));

            foreach (Entity players in Players)
            {
                if (!players.IsAlive) continue;
                if (players.SessionTeam != "allies") continue;
                if (players.Origin.DistanceTo(teleporter.Origin) < 80 && user.IsAlive) StartAsync(teleportPlayer(players, teleporter));
            }
            yield return Wait(1);
            //teleporter.SetField("isLinked", false);
            foreach (Entity f in teleporter.GetField<Entity[]>("floorsActive"))
                f.Hide();

            int time = teleporter.GetField<int>("teleTime");
            yield return Wait(time);
            teleporter.SetField("isLinked", false);
            teleporter.SetField("state", 0);
        }
        private static IEnumerator teleportPlayer(Entity players, Entity teleporter)
        {
            players.PlayLocalSound("counter_uav_activate");
            //Log.Write(LogLevel.All, players.Name);
            players.DisableWeapons();
            players.VisionSetNakedForPlayer("cheat_chaplinnight", 1f);
            players.SetWaterSheeting(1, 2);
            players.FreezeControls(true);
            players.SetField("isCurrentlyTeleported", true);
            yield return Wait(1);

            players.PlaySound("emp_grenade_detonate");
            if ((AIZ.isHellMap && !killstreaks.visionRestored)) players.VisionSetNakedForPlayer("cobra_sunset3", 1f);
            else if (roundSystem.isBossWave) players.VisionSetNakedForPlayer(AIZ.bossVision, 1f);
            else players.VisionSetNakedForPlayer(AIZ.vision, 1f);
            players.SetOrigin(teleporter.GetField<Vector3>("endPos"));
            players.SetPlayerAngles(teleporter.GetField<Vector3>("endAngles"));
            players.FreezeControls(false);
            players.EnableWeapons();
            HudElem timer = HudElem.CreateFontString(players, HudElem.Fonts.HudSmall, 1.5f);
            timer.SetPoint("TOPCENTER", "TOPCENTER", 0, 80, 2);
            timer.HideWhenInMenu = true;
            timer.Alpha = 1;
            int time = teleporter.GetField<int>("teleTime");
            timer.SetTimer(time);
            yield return Wait(time);

            if (!players.IsAlive) { timer.Destroy(); yield break; }
            players.VisionSetNakedForPlayer("cheat_chaplinnight", 1f);
            timer.Destroy();
            PlayFX(AIZ.fx_teleportSpark, players.Origin);
            players.PlayLocalSound("counter_uav_deactivate");
            yield return Wait(1);

            if (!players.GetField<bool>("isDown") && !roundSystem.isBossWave) players.VisionSetNakedForPlayer(AIZ.vision, 1f);
            else if (!players.GetField<bool>("isDown")) players.VisionSetNakedForPlayer(AIZ.bossVision, 1f);
            else players.VisionSetNakedForPlayer("cheat_bw", 1f);

            PlayFX(AIZ.fx_teleportSpark, teleporter.Origin);
            players.SetOrigin(teleporter.Origin);
            //Vector3 teleporterAngles = teleporter.Angles;
            players.SetPlayerAngles(teleporter.Angles);
            players.ClearField("isCurrentlyTeleported");
        }

        private static void useElevator(Entity elevator, Entity player)
        {
            if (player.GetField<bool>("isDown") || !AIZ.powerActivated || !player.IsAlive || player.GetField<int>("cash") < 500 || elevator.GetField<bool>("isMoving")) return;
            player.SetField("cash", player.GetField<int>("cash") - 500);
            hud.scorePopup(player, -500);
            hud.scoreMessage(player, AIZ.gameStrings[265]);
            elevator.SetField("isMoving", true);

            if (elevator.HasField("fx_xmas"))
            {
                Entity[] fx = elevator.GetField<Entity[]>("fx_xmas");

                StopFXOnTag(AIZ.fx_rayGunUpgrade, fx[0], "tag_origin");
                StopFXOnTag(AIZ.fx_rayGun, fx[1], "tag_origin");
                StopFXOnTag(AIZ.fx_rayGun, fx[2], "tag_origin");
                StopFXOnTag(AIZ.fx_rayGunUpgrade, fx[3], "tag_origin");

                AfterDelay(100, () => PlayFXOnTag(AIZ.fx_rayGunUpgrade, fx[0], "tag_origin"));
                AfterDelay(200, () => PlayFXOnTag(AIZ.fx_rayGun, fx[1], "tag_origin"));
                AfterDelay(300, () => PlayFXOnTag(AIZ.fx_rayGun, fx[2], "tag_origin"));
                AfterDelay(400, () => PlayFXOnTag(AIZ.fx_rayGunUpgrade, fx[3], "tag_origin"));
            }

            Vector3 start = elevator.GetField<Vector3>("startPos");
            Vector3 end = elevator.GetField<Vector3>("endPos");
            Vector3 drop = elevator.GetField<Vector3>("dropPos");
            player.PlayerLinkTo(elevator, "tag_origin", 0, 180, 180, 180, 180, true);
            elevator.MoveTo(end, 5, 1, 1);
            AfterDelay(5000, () => elevator_dropOffPlayer(player, elevator, start, drop));
        }
        private static void elevator_dropOffPlayer(Entity player, Entity elevator, Vector3 start, Vector3 drop)
        {
            AfterDelay(5000, () => elevator.SetField("isMoving", false));
            elevator.MoveTo(start, 5, 1, 1);

            if (!player.IsAlive)
                return;

            player.Unlink();
            player.SetOrigin(drop);
        }

        private static void usePerk(Entity box, Entity player, int perk)
        {
            if (!AIZ.powerActivated || player.GetField<bool>("isDown")) return;
            if (player.SessionTeam != "allies") return;
            if (player.GetField<int>("totalPerkCount") >= AIZ.perkLimit && AIZ.perkLimit > 0) return;
            if (perk != 7 && player.GetField<bool>("perk" + perk + "bought")) return;
            else if (perk == 7 && (player.GetField<int>("perk" + perk + "bought") > 2 || player.GetField<bool>("autoRevive"))) return;
            if (player.GetField<int>("cash") < box.GetField<int>("cost")) return;

            int cost = box.GetField<int>("cost");
            string name = "";
            string icon = "white";
            string[] perks = { };
            switch (perk)
            {
                case 1:
                    name = AIZ.gameStrings[266];
                    perks = new string[] { "specialty_armorvest" };
                    icon = "cardicon_juggernaut_1";
                    break;
                case 2:
                    name = AIZ.gameStrings[267];
                    perks = new string[] { "specialty_lightweight", "specialty_longersprint" };//"specialty_marathon"
                    icon = "specialty_longersprint_upgrade";
                    break;
                case 3:
                    name = AIZ.gameStrings[268];
                    perks = new string[] { "specialty_fastreload", "specialty_quickdraw" };//, "specialty_quickswap"
                    icon = "specialty_fastreload_upgrade";
                    break;
                case 4:
                    name = AIZ.gameStrings[269];
                    icon = "specialty_twoprimaries_upgrade";
                    break;
                case 5:
                    name = AIZ.gameStrings[270];
                    perks = new string[] { "specialty_rof" };
                    icon = "weapon_attachment_rof";
                    break;
                case 6:
                    name = AIZ.gameStrings[271];
                    perks = new string[] { "specialty_stalker" };
                    icon = "specialty_stalker_upgrade";
                    break;
                case 7:
                    name = AIZ.gameStrings[272];
                    icon = "waypoint_revive";
                    break;
                case 8:
                    name = AIZ.gameStrings[273];
                    perks = new string[] { "specialty_scavenger" };
                    icon = "specialty_scavenger_upgrade";
                    break;
            }
            player.SetField("cash", player.GetField<int>("cash") - cost);
            hud.scorePopup(player, -cost);
            hud.scoreMessage(player, name + "!");
            foreach (string perkName in perks) player.SetPerk(perkName, true, false);
            player.SetBlurForPlayer(10, 0.3f);
            AfterDelay(700, () =>
                player.SetBlurForPlayer(0, 0.3f));
            if (perk == 1)
            {
                player.MaxHealth = AIZ.maxPlayerHealth_Jugg;
                player.Health = AIZ.maxPlayerHealth_Jugg;
            }
            else if (perk == 4)
                player.SetField("NewGunReady", true);
            else if (perk == 7)
                player.SetField("autoRevive", true);
            if (perk != 7) player.SetField("perk" + perk + "bought", true);
            else if (perk == 7) player.SetField("perk" + perk + "bought", player.GetField<int>("perk7bought") + 1);
            player.SetField("totalPerkCount", player.GetField<int>("totalPerkCount") + 1);
            player.SetField("PerkBought", icon);
            player.PlayLocalSound("earn_perk");
            StartAsync(hud.showBoughtPerk(player, name, icon, perk-1));
            AfterDelay(50, () => hud.updatePerksHud(player, false));
        }

        private static void usePower(Entity box, Entity player)
        {
            if (player.SessionTeam != "allies") return;
            if (player.GetField<int>("cash") < 10000) return;
            if (box.GetField<bool>("bought")) return;
            player.SetField("cash", player.GetField<int>("cash") - 10000);
            hud.scorePopup(player, -10000);
            hud.scoreMessage(player, AIZ.gameStrings[274]);
            box.SetField("bought", true);
            box.CloneBrushModelToScriptModel(player);
            box.MoveTo(box.Origin + new Vector3(0, 0, 2000), 5);
            box.GetField<Entity>("fx").SetField("delete", true);
            if (box.HasField("fx_xmas"))
            {
                Array.ForEach(box.GetField<Entity[]>("fx_xmas"), (fx) => fx.Delete());
                box.ClearField("fx_xmas");
            }
            hud.EMPTime = 0;
            //int objID = _objIDs[box];
            //Objective_Delete(objID);
            removeObjID(box);
            hud.powerBox = true;
            AfterDelay(5000, () => powerBoxActivate(box, player));
        }
        private static void powerBoxActivate(Entity box, Entity player)
        {
            PlayFX(AIZ.fx_empBlast, box.Origin);
            PlaySoundAtPos(box.Origin, "nuke_explosion");
            PlaySoundAtPos(box.Origin, "nuke_wave");
            removeUsable(box);
            //box.Delete();
            AIZ.powerActivated = true;
            if (hud.powerHud != null)
            {
                hud.powerHud.Color = new Vector3(0, 0.9f, 0);
                hud._setText(hud.powerHud, AIZ.gameStrings[275]);
            }
            StartAsync(hud.powerBoughtHud(player));
        }

        private static void useDoor(Entity door, Entity player)
        {
            if (!player.IsAlive) return;
            if (player.SessionTeam == "allies")
            {
                if (door.GetField<string>("state") != "close") return;
                int cost = door.GetField<int>("cost");
                if (player.GetField<int>("cash") < cost) return;
                door.SetField("state", "open");
                player.SetField("cash", player.GetField<int>("cash") - cost);
                hud.scorePopup(player, -cost);
                hud.scoreMessage(player, AIZ.gameStrings[276]);
                door.MoveTo(door.GetField<Vector3>("open"), 1);
                if (door.HasField("spawn"))
                {
                    botUtil.botSpawns.Add(door.GetField<Vector3>("spawn"));
                    botUtil.spawnAngles.Add(door.GetField<Vector3>("spawnAngles"));
                }
                AfterDelay(1100, () => removeUsable(door));
            }
        }

        private static void usedWallWeapon(Entity box, Entity player)
        {
            if (player.SessionTeam != "allies") return;

            int price = box.GetField<int>("price");
            if (player.HasWeapon(box.GetField<string>("weapon"))) price /= 2;
            if (player.GetField<int>("cash") < price) return;
            if (player.IsAlive)
            {
                if (!box.GetField<bool>("bought")) box.SetField("bought", true);
                player.SetField("cash", player.GetField<int>("cash") - price);
                hud.scorePopup(player, -price);
                string weapon = box.GetField<string>("weapon");
                List<string> weaponList = player.GetField<List<string>>("weaponsList");
                if (!player.GetField<bool>("newGunReady") && !player.HasWeapon(weapon))
                {
                    if (AIZ.isThunderGun(player.CurrentWeapon))
                        AIZ.currentThunderguns--;
                    else if (AIZ.isRayGun(player.CurrentWeapon))
                        AIZ.currentRayguns--;

                    AIZ.updatePlayerWeaponsList(player, player.CurrentWeapon, true);
                    player.TakeWeapon(player.CurrentWeapon);
                }
                player.GiveWeapon(weapon);
                AIZ.updatePlayerWeaponsList(player, weapon);
                player.GiveMaxAmmo(weapon);
                player.SwitchToWeapon(weapon);
                player.PlayLocalSound("oldschool_pickup");
            }
        }
        #endregion

        #region get texts
        public static string getUsableText(Entity usable, Entity player)
        {
            if (player.SessionTeam != "allies") return string.Empty;
            if (AIZ.gameEnded) return string.Empty;
            switch (usable.GetField<string>("usabletype"))
            {
                case "revive":
                    Entity downed = usable.GetField<Entity>("player");
                    if (player == downed || usable.GetField<Entity>("user") == player) return "";
                    else if (player.GetField<bool>("isDown")) return "";
                    else if (usable.GetField<Entity>("user") != usable) return downed.Name + AIZ.gameStrings[299];
                    else return AIZ.gameStrings[300] + downed.Name;
                case "giftTrigger":
                    if (player == usable.GetField<Entity>("owner")) return "";
                    if (!usable.GetField<Entity>("owner").IsAlive || usable.GetField<Entity>("owner").GetField<bool>("isDown") || !AIZ.isPlayer(usable.GetField<Entity>("owner"))) return "";
                    return AIZ.gameStrings[277] + usable.GetField<Entity>("owner").Name;
                case "door":
                    if (usable.GetField<string>("state") == "close")
                        return string.Format(AIZ.gameStrings[278], usable.GetField<int>("cost"), "[{+activate}]");
                    else return "";
                case "randombox":
                    if (usable.GetField<string>("state").Equals("inuse") || usable.GetField<string>("state") == "post_pickup") return "";
                    if (usable.GetField<string>("state").Equals("waiting"))
                    {
                        if (usable.GetField<Entity>("player") == player)
                            return AIZ.gameStrings[279] + localizedNames[usable.GetField<int>("weapon")];
                        return "";
                    }
                    if (sale) return AIZ.gameStrings[280];
                    else return AIZ.gameStrings[281];
                case "pap":
                    if (!AIZ.powerActivated) return AIZ.gameStrings[282];
                    if (usable.GetField<string>("state").Equals("inuse")) return "";
                    if (usable.GetField<string>("state").Equals("waiting"))
                    {
                        if (usable.GetField<Entity>("player") == player)
                            return AIZ.gameStrings[283];
                        return "";
                    }
                    return AIZ.gameStrings[284];
                case "gambler":
                    if (!player.GetField<bool>("GamblerInUse")) return AIZ.gameStrings[285];
                    else return "";
                case "killstreak":
                    if (!AIZ.powerActivated) return AIZ.gameStrings[282];
                    else return AIZ.gameStrings[286];
                case "teleporter":
                    if (usable.GetField<int>("state") > 0) return AIZ.gameStrings[287];
                    else if (!usable.GetField<bool>("isLinked")) return AIZ.gameStrings[288];
                    else return AIZ.gameStrings[289];
                case "linker":
                    if (!AIZ.powerActivated) return AIZ.gameStrings[282];
                    else if (usable.GetField<Entity>("teleporter").GetField<int>("state") > 0) return AIZ.gameStrings[287];
                    else if (usable.GetField<Entity>("teleporter").GetField<bool>("isLinked")) return "";
                    else return AIZ.gameStrings[290];
                case "elevator":
                    if (!AIZ.powerActivated) return AIZ.gameStrings[282];
                    else if (usable.GetField<bool>("isMoving")) return "";
                    else return AIZ.gameStrings[291];
                case "bank":
                    if (!hud.powerBox && !AIZ.isHellMap) return AIZ.gameStrings[282];//BS 12/1/21 - Changing this to require the power box instead of power in favor of keeping EMPs from enabling this
                    else return AIZ.gameStrings[292];
                case "perk1":
                    if (!AIZ.powerActivated) return AIZ.gameStrings[282];
                    if (player.GetField<bool>("perk1bought")) return string.Format(AIZ.gameStrings[293], "Juggernog");
                    if (player.GetField<int>("totalPerkCount") >= AIZ.perkLimit && AIZ.perkLimit > 0) return string.Format(AIZ.gameStrings[295], AIZ.perkLimit);
                    else return string.Format(AIZ.gameStrings[294], "Juggernog", 2500, "[{+activate}]");
                case "perk2":
                    if (!AIZ.powerActivated) return AIZ.gameStrings[282];
                    if (player.GetField<bool>("perk2bought")) return string.Format(AIZ.gameStrings[293], "Stamin-Up");
                    if (player.GetField<int>("totalPerkCount") >= AIZ.perkLimit && AIZ.perkLimit > 0) return string.Format(AIZ.gameStrings[295], AIZ.perkLimit);
                    else return string.Format(AIZ.gameStrings[294], "Stamin-Up", 2000, "[{+activate}]");
                case "perk3":
                    if (!AIZ.powerActivated) return AIZ.gameStrings[282];
                    if (player.GetField<bool>("perk3bought")) return string.Format(AIZ.gameStrings[293], "Speed Cola");
                    if (player.GetField<int>("totalPerkCount") >= AIZ.perkLimit && AIZ.perkLimit > 0) return string.Format(AIZ.gameStrings[295], AIZ.perkLimit);
                    else return string.Format(AIZ.gameStrings[294], "Speed Cola", 3000, "[{+activate}]");
                case "perk4":
                    if (!AIZ.powerActivated) return AIZ.gameStrings[282];
                    if (player.GetField<bool>("perk4bought")) return string.Format(AIZ.gameStrings[293], "Mule Kick");
                    if (player.GetField<int>("totalPerkCount") >= AIZ.perkLimit && AIZ.perkLimit > 0) return string.Format(AIZ.gameStrings[295], AIZ.perkLimit);
                    else return string.Format(AIZ.gameStrings[294], "Mule Kick", 4000, "[{+activate}]");
                case "perk5":
                    if (!AIZ.powerActivated) return AIZ.gameStrings[282];
                    if (player.GetField<bool>("perk5bought")) return string.Format(AIZ.gameStrings[293], "Double Tap");
                    if (player.GetField<int>("totalPerkCount") >= AIZ.perkLimit && AIZ.perkLimit > 0) return string.Format(AIZ.gameStrings[295], AIZ.perkLimit);
                    else return string.Format(AIZ.gameStrings[294], "Double Tap", 2000, "[{+activate}]");
                case "perk6":
                    if (!AIZ.powerActivated) return AIZ.gameStrings[282];
                    if (player.GetField<bool>("perk6bought")) return string.Format(AIZ.gameStrings[293], "Stalker Soda");
                    if (player.GetField<int>("totalPerkCount") >= AIZ.perkLimit && AIZ.perkLimit > 0) return string.Format(AIZ.gameStrings[295], AIZ.perkLimit);
                    else return string.Format(AIZ.gameStrings[294], "Stalker Soda", 1500, "[{+activate}]");
                case "perk7":
                    if (!AIZ.powerActivated) return AIZ.gameStrings[282];
                    if (player.GetField<bool>("autoRevive")) return string.Format(AIZ.gameStrings[293], "Quick Revive Pro");
                    else if (player.GetField<int>("perk7bought") >= 3) return "You already bought Quick Revive Pro three times!";
                    if (player.GetField<int>("totalPerkCount") >= AIZ.perkLimit && AIZ.perkLimit > 0) return string.Format(AIZ.gameStrings[295], AIZ.perkLimit);
                    else return string.Format(AIZ.gameStrings[294], "Quick Revive Pro", 1500, "[{+activate}]");
                case "perk8":
                    if (!AIZ.powerActivated) return AIZ.gameStrings[282];
                    if (player.GetField<bool>("perk8bought")) return string.Format(AIZ.gameStrings[293], "Scavenge-Aid");
                    if (player.GetField<int>("totalPerkCount") >= AIZ.perkLimit && AIZ.perkLimit > 0) return string.Format(AIZ.gameStrings[295], AIZ.perkLimit);
                    else return string.Format(AIZ.gameStrings[294], "Scavenge-Aid", 4000, "[{+activate}]");
                case "ammo":
                    if (usable.GetField<bool>("used")) return "";
                    else return string.Format(AIZ.gameStrings[294], AIZ.gameStrings[301], (4500 + player.GetField<int>("ammoCostAddition")), "[{+activate}]");
                case "power":
                    return AIZ.gameStrings[296];
                case "expAmmo":
                    if (player.GetField<bool>("hasExpAmmoPerk")) return "";
                    else return AIZ.gameStrings[297];
                case "heliExtraction":
                    Entity lb = usable.GetField<Entity>("heli");
                    Entity owner = lb.GetField<Entity>("owner");
                    if (player != owner) return "";
                    else return AIZ.gameStrings[298];
                case "carePackage":
                    string streak = "";
                    switch (usable.GetField<int>("streak"))
                    {
                        case 0:
                            streak = AIZ.gameStrings[301];
                            break;
                        case 1:
                            streak = AIZ.gameStrings[302];
                            break;
                        case 2:
                            streak = AIZ.gameStrings[303];
                            break;
                        case 3:
                            if (AIZ.isHellMap)
                                streak = AIZ.gameStrings[304];
                            else
                                streak = AIZ.gameStrings[305];
                            break;
                        case 4:
                            streak = AIZ.gameStrings[306];
                            break;
                        case 5:
                            streak = AIZ.gameStrings[229];
                            break;
                        case 6:
                            streak = AIZ.gameStrings[308];
                            break;
                        case 7:
                            streak = AIZ.gameStrings[309];
                            break;
                        case 9:
                            streak = AIZ.gameStrings[310];
                            break;
                        case 12:
                            streak = AIZ.gameStrings[311];
                            break;
                        case 13:
                            streak = AIZ.gameStrings[312];
                            break;
                        case 14:
                            streak = AIZ.gameStrings[313];
                            break;
                        case 15:
                            streak = AIZ.gameStrings[314];
                            break;
                        case 8:
                            streak = AIZ.gameStrings[315];
                            break;
                    }
                    return AIZ.gameStrings[316] + streak;
                case "wallweapon":
                    string weapon = usable.GetField<string>("weapon");
                    string weaponName = hud.getWeaponName(weapon);
                    if (weaponName == "") weaponName = AIZ.gameStrings[317];
                    int cost = usable.GetField<int>("price");

                    if (usable.HasField("script_noteworthy")) return AIZ.gameStrings[318];

                    if (!player.HasWeapon(weapon) && cost != 0) return string.Format(AIZ.gameStrings[319], weaponName, cost, "[{+activate}]");
                    else if (!player.HasWeapon(weapon) && cost == 0) return AIZ.gameStrings[320] + weaponName;
                    else return AIZ.gameStrings[316] + (cost/2);
                case "helmet":
                    if (player.HasField("helmet") && player.GetField<bool>("helmet")) return "";
                    return AIZ.gameStrings[320] + " P.E.S.";
                default:
                    return "";
            }
        }
        #endregion

        #region moonutils
        private static void spawnMoonHelmet(Vector3 location, Vector3 angles)
        {
            Entity helmet = Spawn("script_model", location);
            helmet.Angles = angles;
            helmet.SetModel("mp_fullbody_ally_juggernaut");
            helmet.HidePart("j_spine4");
            helmet.HidePart("j_elbow_ri");
            helmet.HidePart("j_hip_ri");

            makeUsable(helmet, "helmet", 50);
        }

        private static void spawnMoonExcavator(Vector3 location, Vector3 angles, string name)
        {
            Entity excavatorBase = Spawn("script_model", location);
            excavatorBase.Angles = angles;
            excavatorBase.SetModel("machinery_windmill");
            Entity excavator = Spawn("script_model", location);
            excavator.Angles = angles;
            excavator.SetModel("vehicle_pavelow_opforce");
            excavator.HidePart("body_animate_jnt");
            excavator.HidePart("j_door_ri");
            excavator.LinkTo(excavatorBase, "j_top", Vector3.Zero, Vector3.Zero);
        }

        private static void useHelmet(Entity player)
        {
            if (player.HasField("helmet")) return;
            player.PlayLocalSound("mp_killstreak_equip_done");
            player.IPrintLnBold(AIZ.gameStrings[322]);
            player.GiveWeapon("trophy_mp");
            player.SetActionSlot(3, "weapon", "trophy_mp");
            player.SetField("helmet", true);
        }
        public static void putOnHelmet(Entity player)
        {
            player.DisableWeaponSwitch();
            player.PlayLocalSound("foly_onemanarmy_bag3_plr");
            player.SetWeaponAmmoClip("trophy_mp", 0);
            player.SetWeaponAmmoStock("trophy_mp", 0);
            AfterDelay(3000, () =>
            {
                HudElem helmet = HudElem.CreateIcon(player, "goggles_overlay", 640, 480);
                helmet.Alpha = 1;
                helmet.Archived = true;
                helmet.Foreground = false;
                helmet.HideIn3rdPerson = true;
                helmet.HideWhenDead = true;
                helmet.HideWhenInMenu = false;
                helmet.HorzAlign = HudElem.HorzAlignments.Fullscreen;
                helmet.VertAlign = HudElem.VertAlignments.Fullscreen;
                player.SetField("hud_helmet", helmet);
                player.EnableWeaponSwitch();
                player.SwitchToWeapon(player.GetField<string>("lastDroppableWeapon"));
                player.SetWeaponAmmoClip("trophy_mp", 1);
                player.SetWeaponAmmoStock("trophy_mp", 1);
                player.SetField("hasHelmetOn", true);
                player.Notify("helmet_on");
                monitorHelmetOnForBreathing(player);
                player.RemoteCameraSoundscapeOn();
                player.SetReverb("snd_enveffectsprio_level", "sewer", .1f, 1, 0);
                
                player.Attach("ims_scorpion_explosive1", "j_head", true);

                player.IPrintLnBold(AIZ.gameStrings[323]);
            });
        }
        public static void takeOffHelmet(Entity player)
        {
            player.DisableWeaponSwitch();
            player.PlayLocalSound("foly_onemanarmy_bag3_plr");
            AfterDelay(1500, () =>
            {
                player.EnableWeaponSwitch();
                player.SwitchToWeapon(player.GetField<string>("lastDroppableWeapon"));
            });
            HudElem helmet = player.GetField<HudElem>("hud_helmet");
            helmet.Destroy();
            player.Detach("ims_scorpion_explosive1", "j_head");
            player.RemoteCameraSoundscapeOff();
            player.SetReverb("snd_enveffectsprio_level", "default");
            AfterDelay(2000, () => player.ClearField("hasHelmetOn"));
        }

        private static void monitorHelmetOnForBreathing(Entity player)
        {
            OnInterval(4000, () =>
            {
                if (!player.HasField("hasHelmetOn")) return false;
                player.PlayLocalSound("juggernaut_breathing_sound");
                return true;
            });
        }

        private static void monitorMoonGravity()
        {
            OnInterval(500, () =>
            {
                //Check for gravity and air here
                //Glass values:
                //Building: 
                //150 -153 = front right
                //96, 103, 42, 54 = top front
                //146-149 = front left
                //1, 80, 93, 94, 129, 135, 136, 137 = front far left
                //132, 133, 134, 99, 98, 97, 95, 5= front far right
                //6-12 = back hallway right
                //77-84 = back hallway right 2
                //85-90 = back hallway left 2
                //91, 92, 108, 109-116 = back hallway left
                //105-107 = back entrance
                //Dome:
                //155-160
                foreach (Entity player in Players)
                {
                    if (!IsAlive(player)) continue;

                    if (player.IsTouching(moonTriggers[0]) || player.IsTouching(moonTriggers[1]) || player.IsTouching(moonTriggers[2]) ||
                        player.IsTouching(moonTriggers[3]) || player.IsTouching(moonTriggers[4]) || player.IsTouching(moonTriggers[5]) ||
                        player.IsTouching(moonTriggers[6]))
                    {
                        if (player.HasField("moonGravity"))
                        {
                            player.UnSetPerk("specialty_jumpdive", true);
                            player.ClearField("moonGravity");
                        }
                        if (!player.HasField("isInside"))
                        {
                            player.Notify("helmet_on");
                            player.SetField("isInside", true);
                        }
                    }
                    else
                    {
                        if (player.HasField("isInside") && player.GetField<bool>("isInside"))
                            player.ClearField("isInside");
                        if (!player.HasField("moonGravity"))
                        {
                            player.SetPerk("specialty_jumpdive", true, true);
                            player.SetField("moonGravity", true);
                        }
                        if (!player.HasField("hasHelmetOn") && !player.HasField("isSuffocating"))
                            suffocatePlayer(player);
                    }
                }
                return true;
            });
        }
        private static void suffocatePlayer(Entity player)
        {
            if (player.GetField<bool>("isDown")) return;

            player.SetField("isSuffocating", true);
            player.PlayLocalSound("breathing_heartbeat");

            AfterDelay(3000, () =>
            {
                if (player.HasField("hasHelmetOn") || player.GetField<bool>("isDown") || player.HasField("isInside"))
                {
                    player.ClearField("isSuffocating");
                    return;
                }

                player.IPrintLnBold(AIZ.gameStrings[324]);
                player.PlayLocalSound("breathing_hurt");
                player.SetBlurForPlayer(.5f, .25f);
                AfterDelay(500, () =>
                {
                    player.SetBlurForPlayer(0, .25f);
                    AfterDelay(3000, () =>
                    {
                        if (player.HasField("hasHelmetOn") || player.GetField<bool>("isDown") || player.HasField("isInside"))
                        {
                            player.ClearField("isSuffocating");
                            player.PlayLocalSound("weap_sniper_breathout");
                            return;
                        }

                        player.PlayLocalSound("breathing_heartbeat");
                        player.SetBlurForPlayer(1f, .25f);
                        AfterDelay(500, () =>
                        {
                            player.SetBlurForPlayer(0, .25f);
                            AfterDelay(3000, () =>
                            {
                                if (player.HasField("hasHelmetOn") || player.GetField<bool>("isDown") || player.HasField("isInside"))
                                {
                                    player.ClearField("isSuffocating");
                                    player.PlayLocalSound("weap_sniper_breathgasp");
                                    return;
                                }

                                player.IPrintLnBold(AIZ.gameStrings[324]);
                                player.PlayLocalSound("breathing_hurt");
                                player.SetBlurForPlayer(1.5f, .25f);
                                AfterDelay(500, () =>
                                {
                                    player.SetBlurForPlayer(0, .25f);
                                    AfterDelay(3000, () =>
                                    {
                                        if (player.HasField("hasHelmetOn") || player.GetField<bool>("isDown") || player.HasField("isInside"))
                                        {
                                            player.ClearField("isSuffocating");
                                            player.PlayLocalSound("weap_sniper_breathgasp");
                                            return;
                                        }

                                        player.PlayLocalSound("breathing_hurt");
                                        player.SetBlurForPlayer(2f, .25f);
                                        AfterDelay(500, () =>
                                        {
                                            player.SetBlurForPlayer(0, .25f);
                                            AfterDelay(3000, () =>
                                            {
                                                if (player.HasField("hasHelmetOn") || player.GetField<bool>("isDown") || player.HasField("isInside"))
                                                {
                                                    player.ClearField("isSuffocating");
                                                    player.PlayLocalSound("weap_sniper_breathgasp");
                                                    return;
                                                }

                                                player.FinishPlayerDamage(null, null, player.Health, 0, "MOD_FALLING", "none", player.Origin, Vector3.Zero, "j_neck", 0);
                                                AfterDelay(50, () => player.ClearField("isSuffocating"));
                                            });
                                        });
                                    });
                                });
                            });
                        });
                    });
                });
            });
        }
        /*
        private static IEnumerator suffocatePlayer(Entity player)
        {
            if (player.GetField<bool>("isDown")) yield break;

            player.SetField("isSuffocating", true);
            string helmetStatus = "timeout";
            player.PlayLocalSound("breathing_heartbeat");
            yield return player.WaitTill_notify_or_timeout("helmet_on", 3, value => helmetStatus = value);
            if (helmetStatus != "timeout" || player.HasField("hasHelmetOn"))
            {
                player.ClearField("isSuffocating");
                yield break;
            }
            player.IPrintLnBold(AIZ.gameStrings[324]);
            player.PlayLocalSound("breathing_hurt");
            player.SetBlurForPlayer(.5f, .25f);
            yield return Wait(.5f);
            player.SetBlurForPlayer(0, .25f);
            yield return player.WaitTill_notify_or_timeout("helmet_on", 3, value => helmetStatus = value);
            if (helmetStatus != "timeout" || player.HasField("hasHelmetOn"))
            {
                player.ClearField("isSuffocating");
                player.PlayLocalSound("weap_sniper_breathout");
                yield break;
            }
            player.PlayLocalSound("breathing_heartbeat");
            player.SetBlurForPlayer(1f, .25f);
            yield return Wait(.5f);
            player.SetBlurForPlayer(0, .25f);
            yield return player.WaitTill_notify_or_timeout("helmet_on", 3, value => helmetStatus = value);
            if (helmetStatus != "timeout" || player.HasField("hasHelmetOn"))
            {
                player.ClearField("isSuffocating");
                player.PlayLocalSound("weap_sniper_breathgasp");
                yield break;
            }
            player.PlayLocalSound("breathing_hurt");
            player.SetBlurForPlayer(1.5f, .25f);
            yield return Wait(.5f);
            player.SetBlurForPlayer(0, .25f);
            yield return player.WaitTill_notify_or_timeout("helmet_on", 3, value => helmetStatus = value);
            if (helmetStatus != "timeout" || player.HasField("hasHelmetOn"))
            {
                player.ClearField("isSuffocating");
                player.PlayLocalSound("weap_sniper_breathgasp");
                yield break;
            }
            player.PlayLocalSound("breathing_hurt");
            player.SetBlurForPlayer(2f, .25f);
            yield return Wait(.5f);
            player.SetBlurForPlayer(0, .25f);
            yield return player.WaitTill_notify_or_timeout("helmet_on", 3, value => helmetStatus = value);
            if (helmetStatus != "timeout" || player.HasField("hasHelmetOn"))
            {
                player.ClearField("isSuffocating");
                player.PlayLocalSound("weap_sniper_breathgasp");
                yield break;
            }
            player.FinishPlayerDamage(null, null, player.Health, 0, "MOD_FALLING", "none", player.Origin, Vector3.Zero, "j_neck", 0);
            AfterDelay(50, () => player.ClearField("isSuffocating"));
            yield break;
        }
        */
        #endregion

        public static void startSale()
        {
            if (sale) return;

            Entity fx = null;
            foreach (Entity usable in usables)
            {
                if (usable.HasField("usabletype") && usable.GetField<string>("usabletype") == "randombox")
                {
                    fx = SpawnFX(AIZ.fx_smallFire, usable.GetField<Entity>("weaponEnt").Origin);
                    TriggerFX(fx);
                    //usable.PlayLoopSound("elev_musak_loop");
                }
            }

            if (fx == null) return;

            OnInterval(1000, () => deleteSaleFXOnEnd(fx));

        }

        private static bool deleteSaleFXOnEnd(Entity fx)
        {
            if (!sale) { fx.Delete(); return false; }
            return true;
        }

        public static void makeUsable(Entity ent, string type, int range)
        {
            ent.SetField("usabletype", type);
            ent.SetField("range", range);
            /*
            Entity trigger = Spawn("trigger_radius", ent.Origin, 0, range, range);
            //trigger.Code_Classname = "trigger_" + type;
            trigger.SetField("type", type);
            trigger.LinkTo(ent);
            trigger.SetField("parent", ent);
            ent.SetField("trigger", trigger);
            */
            usables.Add(ent);
        }

        public static void loadMapEditHotfix(string hotfix)
        {
            //TextReader mapFile = File.OpenText(maplist[randomMap]);
            //string lastEdit = mapFile.ReadToEnd();
            //string[] edits = lastEdit.Split('\n');

            AIZ.printToConsole("Adding map hotfix line: {0}", hotfix);
            spawnMapEditObject(hotfix);
        }

        public static void loadMapEdit(string mapname)
        {
            try
            {
                if (checkExtraMaps(mapname))
                    return;

                maplist.Add("scripts\\aizombies\\maps\\" + mapname + ".map");
                for (int i = 1; i < 11; i++)//Setup our filelist
                {
                    //maplist[i] = "scripts\\maps\\" + mapname + "_aiz.txt";//Default
                    string newFile = "scripts\\aizombies\\maps\\" + mapname + "_" + i + ".map";
                    if (File.Exists(newFile))//If we have a new mapfile (_#) set it in our list
                        maplist.Add(newFile);
                }
                int random = new Random().Next(maplist.Count);
                randomMap = (byte)random;
                StreamReader map = new StreamReader(maplist[randomMap]);//Randomly pick from our list
                while (!map.EndOfStream)
                {
                    string line = map.ReadLine();

                    spawnMapEditObject(line);
                }
            }
            catch (Exception e)
            {
                AIZ.printToConsole(AIZ.gameStrings[325], mapname, e.Message);
            }
        }
        private static bool checkExtraMaps(string mapName)
        {
            if (AIZ.rng.Next(100) > 25)//25% chance of a new map
                return false;

            if (mapName == "mp_terminal_cls")
            {
                spawnMapEditObject("bank: (1753.916, 6313.605, 222.125) ; (90, -88.69812, 0)");
                spawnMapEditObject("ammo: (1676.991, 5801.993, 207.125) ; (0, -266.6986, 0)");
                spawnMapEditObject("killstreak: (2135.297, 5631.177, 207.125) ; (0, 89.25842, 0)");
                spawnMapEditObject("pap: (2264.902, 6071.276, 207.125) ; (0, 0, 0)");
                spawnMapEditObject("perk1: (2429.498, 5215.631, 222.125) ; (90, -270.6921, 0)");
                spawnMapEditObject("perk7: (2729.5, 5680.35, 222.125) ; (90, -137.1478, 0)");
                spawnMapEditObject("spawn: (1785.736, 6189.03, 192.125) ; (0, -38.21594, 0)");
                spawnMapEditObject("spawn: (1781.029, 5813.608, 192.125) ; (0, 39.02893, 0)");
                spawnMapEditObject("spawn: (2039.405, 6183.669, 192.125) ; (0, -130.6274, 0)");
                spawnMapEditObject("spawn: (2077.577, 5833.555, 192.125) ; (0, 129.9463, 0)");
                spawnMapEditObject("spawn: (2257.331, 5646.044, 192.125) ; (0, -47.80701, 0)");
                spawnMapEditObject("spawn: (2585.548, 5644.205, 192.125) ; (0, -137.7411, 0)");
                spawnMapEditObject("spawn: (2592.315, 5294.025, 192.125) ; (0, 132.2589, 0)");
                spawnMapEditObject("spawn: (2283.95, 5302.396, 192.125) ; (0, 41.44592, 0)");
                spawnMapEditObject("spawn: (2632.224, 5925.575, 192.125) ; (0, -135.9943, 0)");
                spawnMapEditObject("zombiespawn: (1485.979, 6356.389, 192.125) ; (0, -90.03296, 0)");
                spawnMapEditObject("zombiespawn: (1494.219, 5644.744, 192.125) ; (0, 89.62097, 0)");
                spawnMapEditObject("zombiespawn: (2090.69, 5030.374, 192.125) ; (0, 0.7635498, 0)");
                spawnMapEditObject("zombiespawn: (2778.992, 5039.324, 192.125) ; (0, -179.0387, 0)");
                spawnMapEditObject("invisiblewall: (2134.626, 6025.601, 224.125) ; (2135.414, 6126.643, 302.2602)");
                spawnMapEditObject("gambler: (1903.275, 6017.347, 239.625) ; (0, -181.8677, 0)");
                spawnMapEditObject("invisiblewall: (2391.254, 6006.597, 230.125) ; (2170.693, 6008.345, 301.5245)");
                spawnMapEditObject("invisiblewall: (2338.146, 6237.104, 192.125) ; (2266.125, 6237.368, 280.6372)");
                spawnMapEditObject("doorandspawn: (2503.875, 6188.8, 239.9236) ; (2138.434, 6183.225, 192.125) ; (90, 176.862, 0) ; 2 ; 2 ; 100 ; 10000 ; (2413.63, 6290.377, 192.125) ; (0, 178.0719, 0)");
                spawnMapEditObject("model:police_barrier_01; (2306.244, 6234.095, 192.125) ; (0, 0.01191906, 0)");
                spawnMapEditObject("mapname:Burger Town Of Death");
                spawnMapEditObject("hellMap:True");
                spawnMapEditObject("maxWave:20");
                spawnMapEditObject("randombox: (2793.336, 5440.894, 207.125) ; (0, -90.06592, 0) ; (1750.761, 6016.971, 207.125) ; (0, 89.6759, 0) ; (2386.846, 5730.87, 207.125) ; (0, 270.209, 0) ; (2430.531, 5773.469, 207.125) ; (0, 180.443, 0) ; (2131.326, 5285.379, 207.125) ; (0, 89.63745, 0)");
                randomMap = 255;
                return true;
            }

            return false;
        }
        private static Vector3[] checkExtraMapWaypoints()
        {
            switch (randomMap)
            {
                case 255:
                    return new Vector3[] { new Vector3(2569.506f,5038.915f,192.125f),
                                            new Vector3(2350.767f,5037.602f,192.125f),
                                            new Vector3(2309.466f,5275.099f,192.125f),
                                            new Vector3(2598.073f,5288.062f,192.125f),
                                            new Vector3(2596.918f,5618.59f,192.125f),
                                            new Vector3(2256.84f,5628.258f,192.125f),
                                            new Vector3(2247.064f,5872.119f,192.125f),
                                            new Vector3(2566.791f,5852.317f,192.125f),
                                            new Vector3(2049.795f,5885.242f,192.125f),
                                            new Vector3(2056.774f,6164.297f,192.125f),
                                            new Vector3(1765.833f,6148.722f,192.125f),
                                            new Vector3(1789.686f,5888.189f,192.125f),
                                            new Vector3(1475.903f,5880.878f,192.125f),
                                            new Vector3(1494.784f,6194.971f,192.125f),
                                            new Vector3(2306.309f,6294.527f,192.125f),
                                            new Vector3(2295.443f,6176.554f,192.125f) };
                default:
                    return null;
            }
        }

        private static void spawnMapEditObject(string line)
        {
            if (line.StartsWith("//") || line.Equals(string.Empty))
                return;

            string[] split = line.Split(':');
            if (split.Length < 1)
                return;

            string type = split[0];

            switch (type)
            {
                case "crate":
                    split = split[1].Split(';');
                    if (split.Length < 2) return;
                    spawnCrate(AIZ.parseVec3(split[0]), AIZ.parseVec3(split[1]), false, false);
                    break;
                case "invisiblecrate":
                    split = split[1].Split(';');
                    if (split.Length < 2) return;
                    spawnCrate(AIZ.parseVec3(split[0]), AIZ.parseVec3(split[1]), true, false);
                    break;
                case "ramp":
                    split = split[1].Split(';');
                    if (split.Length < 2) return;
                    createRamp(AIZ.parseVec3(split[0]), AIZ.parseVec3(split[1]));
                    break;
                case "teleport":
                    split = split[1].Split(';');
                    if (split.Length < 2) return;
                    createElevator(AIZ.parseVec3(split[0]), AIZ.parseVec3(split[1]));
                    break;
                case "timedTeleporter":
                    split = split[1].Split(';');
                    if (split.Length < 7) return;
                    createTeleporter(AIZ.parseVec3(split[0]), AIZ.parseVec3(split[1]), AIZ.parseVec3(split[2]), AIZ.parseVec3(split[3]), AIZ.parseVec3(split[4]), AIZ.parseVec3(split[5]), int.Parse(split[6]));
                    break;
                case "door":
                    split = split[1].Split(';');
                    if (split.Length < 7) return;
                    createDoor(AIZ.parseVec3(split[0]), AIZ.parseVec3(split[1]), AIZ.parseVec3(split[2]), int.Parse(split[3]), int.Parse(split[4]), int.Parse(split[5]), int.Parse(split[6]));
                    break;
                case "doorandspawn":
                    split = split[1].Split(';');
                    if (split.Length < 7) return;
                    if (split.Length < 9)
                    {
                        createDoor(AIZ.parseVec3(split[0]), AIZ.parseVec3(split[1]), AIZ.parseVec3(split[2]), int.Parse(split[3]), int.Parse(split[4]), int.Parse(split[5]), int.Parse(split[6]));
                        break;
                    }
                    createDoor(AIZ.parseVec3(split[0]), AIZ.parseVec3(split[1]), AIZ.parseVec3(split[2]), int.Parse(split[3]), int.Parse(split[4]), int.Parse(split[5]), int.Parse(split[6]), AIZ.parseVec3(split[7]), AIZ.parseVec3(split[8]));
                    break;
                case "wall":
                    split = split[1].Split(';');
                    if (split.Length < 2) return;
                    createWall(AIZ.parseVec3(split[0]), AIZ.parseVec3(split[1]), false, false);
                    break;
                case "invisiblewall":
                    split = split[1].Split(';');
                    if (split.Length < 2) return;
                    createWall(AIZ.parseVec3(split[0]), AIZ.parseVec3(split[1]), true, false);
                    break;
                case "deathwall":
                    split = split[1].Split(';');
                    if (split.Length < 2) return;
                    createWall(AIZ.parseVec3(split[0]), AIZ.parseVec3(split[1]), true, true);
                    break;
                case "randombox":
                    split = split[1].Split(';');
                    //if (split.Length < 2) return;
                    if (split.Length < 3) return;
                    boxLocations = new Vector3[split.Length / 2][];
                    int currentSplit = 0;
                    for (int i = 0; i < split.Length / 2; i++)
                    {
                        boxLocations[i] = new Vector3[2];
                        boxLocations[i][0] = AIZ.parseVec3(split[currentSplit]);
                        currentSplit++;
                        boxLocations[i][1] = AIZ.parseVec3(split[currentSplit]);
                        currentSplit++;
                    }
                    randomWeaponCrate(boxLocations[0][0], boxLocations[0][1]);
                    break;
                case "pap":
                    split = split[1].Split(';');
                    if (split.Length < 2) return;
                    papCrate(AIZ.parseVec3(split[0]), AIZ.parseVec3(split[1]));
                    break;
                case "gambler":
                    split = split[1].Split(';');
                    if (split.Length < 2) return;
                    gamblerCrate(AIZ.parseVec3(split[0]), AIZ.parseVec3(split[1]));
                    break;
                case "floor":
                    split = split[1].Split(';');
                    if (split.Length < 2) return;
                    createFloor(AIZ.parseVec3(split[0]), AIZ.parseVec3(split[1]), false, false);
                    break;
                case "invisiblefloor":
                    split = split[1].Split(';');
                    if (split.Length < 2) return;
                    createFloor(AIZ.parseVec3(split[0]), AIZ.parseVec3(split[1]), true, false);
                    break;
                case "deathfloor":
                    split = split[1].Split(';');
                    if (split.Length < 2) return;
                    createFloor(AIZ.parseVec3(split[0]), AIZ.parseVec3(split[1]), true, true);
                    break;
                case "elevator":
                    split = split[1].Split(';');
                    if (split.Length < 4) return;
                    realElevator(AIZ.parseVec3(split[0]), AIZ.parseVec3(split[1]), AIZ.parseVec3(split[2]), AIZ.parseVec3(split[3]));
                    break;
                case "model":
                    split = split[1].Split(';');
                    if (split.Length < 3) return;
                    spawnModel(split[0], AIZ.parseVec3(split[1]), AIZ.parseVec3(split[2]));
                    break;
                case "bank":
                    split = split[1].Split(';');
                    if (split.Length < 2) return;
                    spawnBank(AIZ.parseVec3(split[0]), AIZ.parseVec3(split[1]));
                    break;
                case "perk1":
                    split = split[1].Split(';');
                    if (split.Length < 2) return;
                    perkCrate(AIZ.parseVec3(split[0]), AIZ.parseVec3(split[1]), 1);
                    break;
                case "perk2":
                    split = split[1].Split(';');
                    if (split.Length < 2) return;
                    perkCrate(AIZ.parseVec3(split[0]), AIZ.parseVec3(split[1]), 2);
                    break;
                case "perk3":
                    split = split[1].Split(';');
                    if (split.Length < 2) return;
                    perkCrate(AIZ.parseVec3(split[0]), AIZ.parseVec3(split[1]), 3);
                    break;
                case "perk4":
                    split = split[1].Split(';');
                    if (split.Length < 2) return;
                    perkCrate(AIZ.parseVec3(split[0]), AIZ.parseVec3(split[1]), 4);
                    break;
                case "perk5":
                    split = split[1].Split(';');
                    if (split.Length < 2) return;
                    perkCrate(AIZ.parseVec3(split[0]), AIZ.parseVec3(split[1]), 5);
                    break;
                case "perk6":
                    split = split[1].Split(';');
                    if (split.Length < 2) return;
                    perkCrate(AIZ.parseVec3(split[0]), AIZ.parseVec3(split[1]), 6);
                    break;
                case "perk7":
                    split = split[1].Split(';');
                    if (split.Length < 2) return;
                    perkCrate(AIZ.parseVec3(split[0]), AIZ.parseVec3(split[1]), 7);
                    break;
                case "perk8":
                    split = split[1].Split(';');
                    if (split.Length < 2) return;
                    perkCrate(AIZ.parseVec3(split[0]), AIZ.parseVec3(split[1]), 8);
                    break;
                case "ammo":
                    split = split[1].Split(';');
                    if (split.Length < 2) return;
                    ammoCrate(AIZ.parseVec3(split[0]), AIZ.parseVec3(split[1]));
                    break;
                case "killstreak":
                    split = split[1].Split(';');
                    if (split.Length < 2) return;
                    killstreakCrate(AIZ.parseVec3(split[0]), AIZ.parseVec3(split[1]));
                    break;
                case "power":
                    split = split[1].Split(';');
                    if (split.Length < 2) return;
                    powerCrate(AIZ.parseVec3(split[0]), AIZ.parseVec3(split[1]));
                    break;
                case "spawn":
                    split = split[1].Split(';');
                    if (split.Length < 2) return;
                    SpawnLocs.Add(AIZ.parseVec3(split[0]));
                    SpawnAngles.Add(AIZ.parseVec3(split[1]));
                    break;
                case "zombiespawn":
                    split = split[1].Split(';');
                    if (split.Length < 2) return;
                    botUtil.botSpawns.Add(AIZ.parseVec3(split[0]));
                    botUtil.spawnAngles.Add(AIZ.parseVec3(split[1]));
                    break;
                case "perk1Interchange":
                    perkCrate(new Vector3(453.7388f, -415.7576f, 614.9092f), new Vector3(90, 0, 0), -1);
                    break;
                case "mapname":
                    if (split.Length < 1) return;
                    AIZ.zombieMapname = split[1];
                    break;
                case "hellMap":
                    if (split.Length < 1) return;
                    if (split[1] == "1" || split[1].ToLowerInvariant() == "true")
                    {
                        hellMapSetting = true;
                        AIZ.powerActivated = true;
                    }
                    else hellMapSetting = false;
                    break;
                case "maxWave":
                    if (split.Length < 1) return;
                    roundSystem.totalWaves = uint.Parse(split[1]);
                    break;
                case "xLimit":
                    split = split[1].Split(';');
                    if (split.Length < 2) return;
                    setupSpaceLimit(true, int.Parse(split[0]), int.Parse(split[1]));
                    break;
                case "yLimit":
                    split = split[1].Split(';');
                    if (split.Length < 2) return;
                    setupSpaceLimit(false, int.Parse(split[0]), int.Parse(split[1]));
                    break;
                case "minefield":
                    split = split[1].Split(';');
                    if (split.Length < 3) return;
                    Entity mine = Spawn("trigger_radius", AIZ.parseVec3(split[0]), 0, int.Parse(split[1]), int.Parse(split[2]));
                    mine.TargetName = "minefield";
                    break;
                case "radiation":
                    split = split[1].Split(';');
                    if (split.Length < 3) return;
                    Entity rad = Spawn("trigger_radius", AIZ.parseVec3(split[0]), 0, int.Parse(split[1]), int.Parse(split[2]));
                    rad.TargetName = "radiation";
                    break;
                case "fallLimit":
                    if (split.Length < 1) return;
                    int val = int.Parse(split[1]);
                    OnInterval(200, () => monitorFallDeath(val));
                    //Also set zombie death limit here
                    AIZ.mapHeight = val;
                    break;
                case "wallweapon":
                    split = split[1].Split(';');
                    if (split.Length < 4) return;
                    wallWeapon(AIZ.parseVec3(split[0]), AIZ.parseVec3(split[1]), split[2], int.Parse(split[3]));
                    break;
                case "mapcenter":
                    split = split[1].Split(';');
                    if (split.Length < 1) return;
                    SetMapCenter(AIZ.parseVec3(split[0]));
                    break;
                case "spacehelmet":
                    if (AIZ._mapname != "mp_dome") return;
                    split = split[1].Split(';');
                    if (split.Length < 2) return;
                    spawnMoonHelmet(AIZ.parseVec3(split[0]), AIZ.parseVec3(split[1]));
                    break;
                    /*
            case "excavator":
                if (AIZ._mapname != "mp_dome") return;
                split = split[1].Split(';');
                if (split.Length < 2) return;
                spawnMoonExcavator(AIZ.parseVec3(split[0]), AIZ.parseVec3(split[1]), split[2]);
                break;
                */
                default:
                    AIZ.printToConsole(AIZ.gameStrings[326], type);
                    break;
            }
        }

        public static IEnumerator specialLevelFunctions()
        {
            switch (AIZ._mapname)
            {
                case "mp_dome":
                    yield return WaitForFrame();
                    if (AIZ.getZombieMapname() == "Abandoned Outpost of Hell")
                    {
                        for (int i = 18; i < 1000; i++)
                        {
                            Entity ent = GetEntByNum(i);
                            if (ent == null) continue;
                            if (ent.Model == "vehicle_hummer_destructible")
                                ent.Delete();
                            else if (ent.TargetName == "explodable_barrel")
                            {
                                Entity col = GetEnt(ent.Target, "targetname");
                                if (col != null) col.Delete();
                                ent.Delete();
                            }
                        }
                    }
                    else if (AIZ.getZombieMapname() == "Deserted Dome")
                    {
                        //Add models to blocked areas
                        spawnModel("ap_table_piece_big_destroyed", new Vector3(546, 1733, -250), new Vector3(83, 184, 0));
                        spawnModel("ap_table_destroyed_01", new Vector3(556, 2378, -254), new Vector3(67, 355, 0));
                        spawnModel("ap_table_destroyed_01", new Vector3(467, 2238, -255), new Vector3(56, 269, 0));
                        spawnModel("metal_hanging_strips_med_01", new Vector3(442, 1899, -191), new Vector3(10, 168, 0));

                        //Roll a 50/50 for moon variant
                        int cointoss = AIZ.rng.Next(100);
                        if (cointoss % 2 == 0)
                        {
                            AIZ.zombieMapname = "Moonbase";
                            spawnMapEditObject("spacehelmet: (563.909, 2194.27, -253.7606) ; (0, 264.3212, 0)");
                            spawnMapEditObject("spacehelmet: (545.0822, 1920.188, -243.3936) ; (0, 86.3431, 0)");
                            spawnMapEditObject("spacehelmet: (1443.034, 1166.74, -254.1329) ; (0, 83.2894, 0)");
                            spawnMapEditObject("spacehelmet: (399.8792, -31.77756, -386.0158) ; (0, 182, 0)");
                            spawnMapEditObject("spacehelmet: (-233.4409, 39.72492, -385.3513) ; (0, 23, 0)");

                            //spawnMapEditObject("excavator: (-1303.499, -171.8676, 87.10009) ; (90, 352.4004, 0)");//Omicron

                            initMoon();
                        }
                    }

                    //Easter egg stuff
                    //Entity trigger = GetEnt("aiz_dome_trigger", "targetname");
                    //trigger.OnNotify("trigger", (t, ent) => domeEasterEgg(t, (Entity)ent));

                    break;
                case "mp_alpha":
                    for (int i = 18; i < 2000; i++)
                    {
                        Entity ent = GetEntByNum(i);
                        if (ent == null) continue;
                        string targetname = ent.TargetName;
                        if (targetname == null || targetname == "") continue;
                        bool isDestructible = (targetname == "explodable_barrel");

                        if (isDestructible)
                        {
                            if (ent.Target != "")
                            {
                                Entity col = GetEnt(ent.Target, "targetname");
                                if (col != null) col.Delete();
                            }
                            ent.Delete();
                        }
                    }
                    break;
                case "mp_carbon":
                    for (int i = 18; i < 2000; i++)
                    {
                        Entity ent = GetEntByNum(i);
                        if (ent == null) continue;
                        string targetname = ent.TargetName;
                        if (targetname == null || targetname == "") continue;
                        bool isDestructible = (targetname == "explodable_barrel" || targetname.Contains("destructible") || targetname.Contains("destructable"));

                        if (isDestructible && ent.Model != "vehicle_uk_utility_truck_destructible")
                        {
                            if (ent.Target != "")
                            {
                                Entity col = GetEnt(ent.Target, "targetname");
                                if (col != null) col.Delete();
                            }
                            ent.Delete();
                        }
                    }
                    break;
                case "mp_cement":
                    for (int i = 18; i < 2000; i++)
                    {
                        Entity ent = Entity.GetEntity(i);
                        if (ent.TargetName == "") continue;
                        string targetname = ent.TargetName;
                        if (targetname == null || targetname == "") continue;
                        bool isDestructible = (targetname == "explodable_barrel" || targetname.Contains("destructible") || targetname == "industrial_curtain" || targetname == "animated_model" || targetname == "com_wall_fan_blade_rotate");

                        if (isDestructible)
                        {
                            if (ent.Target != "")
                            {
                                Entity col = GetEnt(ent.Target, "targetname");
                                if (col != null) col.Delete();
                            }
                            ent.Delete();
                        }
                    }
                    break;
                case "mp_lambeth":
                case "mp_burn_ss":
                case "mp_crosswalk_ss":
                case "mp_hillside_ss":
                case "mp_plaza2":
                    for (int i = 18; i < 2000; i++)
                    {
                        Entity ent = GetEntByNum(i);
                        if (ent == null) continue;
                        string targetname = ent.TargetName;
                        if (targetname == null || targetname == "") continue;
                        if (targetname.Contains("destructible") || targetname.Contains("destructable"))
                        {
                            if (ent.Target != "")
                            {
                                Entity col;
                                col = GetEnt(ent.Target, "targetname");
                                if (col != null) col.Delete();
                            }
                            ent.Delete();
                        }
                    }
                    yield return WaitForFrame();
                    if (AIZ.getZombieMapname() == "Wastelands" && AIZ._mapname == "mp_lambeth")
                    {
                        //Patch out of bounds
                        createWall(new Vector3(-234, -4560, -271), new Vector3(-232, -3598, -240), true, false);
                    }
                    break;
                case "mp_bootleg":
                    for (int i = 18; i < 1000; i++)
                    {
                        Entity ent = GetEntByNum(i);
                        if (ent == null) continue;
                        string targetname = ent.TargetName;
                        if (targetname == null || targetname == "") continue;
                        if (targetname.Contains("destructible") && ent.Model != "india_vehicle_rksw")
                            ent.Delete();
                    }
                    break;
                case "mp_bravo":
                    yield return WaitForFrame();
                    if (AIZ.getZombieMapname() == "Rundown Village")
                    {
                        //Add a wall under the North wall
                        createWall(new Vector3(720, -1845, 1068), new Vector3(927, -1675, 1050), false, false);
                        //Add a death trigger by the west shack roof to avoid getting on top
                        createWall(new Vector3(351, -1351, 1135), new Vector3(358, -1448, 1170), true, true);
                        //Add a wall by the clubhouse
                        createWall(new Vector3(228, -1154, 971), new Vector3(228, -1120, 1025), true, false);
                        spawnModel("ch_washer_01", new Vector3(230, -1145, 971), new Vector3(0, 90, 0));
                        //Easter egg =P
                        Entity egg = wallWeapon(new Vector3(230, -1145, 965), new Vector3(90, 0, 0), "stinger_mp", 100000);
                        egg.SetField("script_noteworthy", "bravo_easter_egg");
                        foreach (Entity usable in usables)//Re-init the Stamin-up machine so one of the box spawns doesn't go inside it
                        {
                            if (usable.GetField<string>("usabletype") == "perk2")
                            {
                                usable.Origin = new Vector3(-61, -1748, 1144);
                                usable.Angles = new Vector3(90, -136, 0);
                                if (_objIDs.ContainsKey(usable))
                                {
                                    int objId = _objIDs[usable];
                                    Objective_Position(objId, usable.Origin);
                                }
                            }
                        }
                    }
                    break;
                case "mp_exchange":
                    for (int i = 18; i < 1000; i++)
                    {
                        Entity ent = GetEntByNum(i);
                        if (ent == null) continue;
                        string classname = ent.Classname;
                        if (classname == null || classname == "") continue;
                        if (classname == "trigger_hurt")
                        {
                            ent.Origin -= new Vector3(0, 0, 10000);
                        }
                    }
                    break;
                case "mp_terminal_cls":
                    yield return WaitForFrame();
                    if (AIZ.getZombieMapname() == "Airport Security")
                    {
                        for (int i = 18; i < 1000; i++)
                        {
                            Entity ent = GetEntByNum(i);
                            if (ent == null) continue;
                            string targetname = ent.TargetName;
                            if (targetname == null || targetname == "") continue;
                            if (targetname.Contains("toy") || targetname.Contains("vending") || targetname.Contains("destructible") || targetname.Contains("destructable"))
                            {
                                if (ent.Target != "")
                                {
                                    Entity col = GetEnt(ent.Target, "targetname");
                                    if (col != null) col.Delete();
                                }
                                ent.Delete();
                            }
                        }
                    }
                    else if (AIZ.getZombieMapname() == "Burger Town Of Death")
                    {
                        Entity[] gates = new Entity[2];
                        for (int i = 18; i < 1000; i++)
                        {
                            Entity ent = GetEntByNum(i);
                            if (ent == null) continue;
                            string targetname = ent.TargetName;
                            if (targetname == null || targetname == "") continue;
                            if (targetname == "gate_gate_closing")
                            {
                                if (gates[0] == null) gates[0] = ent;
                                else
                                {
                                    gates[1] = ent;
                                    break;
                                }
                            }
                        }

                        gates[0].Origin = new Vector3(2432, 5090, gates[1].Origin.Z);
                        gates[0].Angles -= new Vector3(0, 90, 0);

                        StartAsync(terminal_buildGates(gates));
                    }
                    break;
                case "mp_italy":
                    yield return WaitForFrame();
                    if (AIZ.getZombieMapname() == "Zombies a la Playa")
                    {
                        for (int i = 18; i < 2040; i++)
                        {
                            Entity ent = GetEntByNum(i);
                            if (ent == null) continue;
                            string classname = ent.Classname;
                            if (classname == null || classname == "") continue;
                            if (classname == "trigger_hurt")
                            {
                                ent.dmg = 0;
                                ent.Origin -= new Vector3(0, 0, 10000);//Move triggers away from map since dmg doesn't set properly
                            }
                        }
                    }
                    break;
                case "mp_hardhat":
                    yield return WaitForFrame();
                    if (AIZ.getZombieMapname() == "Infected Suburb")
                    {
                        //Add block under building
                        createFloor(new Vector3(2231f, -1829f, 395), new Vector3(1856f, -1975f, 395), true, false);
                    }
                    else if (AIZ.getZombieMapname() == "Construction Site Of Hell")
                    {
                        //Add a wall by the restaurant to keep players from getting on it
                        createWall(new Vector3(-1507, 474, 651), new Vector3(-1510, 41, 682), true, false);
                    }
                    break;
                case "mp_morningwood":
                    yield return Wait(1);
                    if (AIZ.getZombieMapname() == "Under Construction")
                    {
                        //Fix the TP linker location
                        foreach (Entity usable in usables)
                        {
                            if (usable.GetField<string>("usabletype") == "linker")
                            {
                                if (usable.Origin.DistanceTo(new Vector3(-1654, 1641, 1206)) < 5)
                                {
                                    usable.Origin = new Vector3(-1658, 1629, 1195);
                                    usable.Angles = new Vector3(90, 62, 0);
                                }
                            }
                        }
                    }
                    break;
                case "mp_shipbreaker":
                    yield return WaitForFrame();
                    if (AIZ.getZombieMapname() == "Ship-REKD")
                    {
                        //Close up broken wall by the large crane
                        createWall(new Vector3(97, 183, 759), new Vector3(17, 52, 934), true, false);
                    }
                    break;
            }
        }
        private static IEnumerator terminal_buildGates(Entity[] gates)
        {
            yield return Wait(.3f);

            Entity[] intro_col = new Entity[7];
            Entity[] intro_col2 = new Entity[7];
            for (int i = 0; i < 7; i++)
            {
                Entity gatePart = Spawn("script_model", gates[1].Origin - new Vector3(0, 0, 24 * i));
                gatePart.Angles = gates[1].Angles;
                gatePart.CloneBrushModelToScriptModel(gates[1]);
                gatePart.LinkTo(gates[1]);

                Entity gatePart2 = Spawn("script_model", gates[0].Origin - new Vector3(0, 0, 24 * i));
                gatePart2.Angles = gates[0].Angles;
                gatePart2.CloneBrushModelToScriptModel(gates[0]);
                gatePart2.LinkTo(gates[0]);
            }
            for (int i = 0; i < intro_col.Length; i++)
            {
                Entity gateCol = Spawn("script_model", gates[1].Origin - new Vector3(0, 0, 24 * i));
                gateCol.Angles = gates[1].Angles;
                gateCol.CloneBrushModelToScriptModel(gates[1]);
                gateCol.Hide();
                intro_col[i] = gateCol;

                Entity gateCol2 = Spawn("script_model", gates[0].Origin - new Vector3(0, 0, 24 * i));
                gateCol2.Angles = gates[0].Angles;
                gateCol2.CloneBrushModelToScriptModel(gates[0]);
                gateCol2.Hide();
                intro_col2[i] = gateCol2;
            }

            gates[0].Origin += new Vector3(0, 0, 143);
            gates[1].Origin += new Vector3(0, 0, 143);

            StartAsync(terminal_dropTheGates(gates, intro_col, intro_col2));
        }
        private static IEnumerator terminal_dropTheGates(Entity[] gates, Entity[] intro_col, Entity[] intro_col2)
        {
            while (!AIZ.intermissionTimerStarted)
                yield return WaitForFrame();

            yield return Wait(2);

            foreach (Entity gate in gates)
            {
                gate.MoveTo(gate.Origin - new Vector3(0, 0, 143), 2, 1.5f);
                gate.PlayLoopSound("ugv_engine_high");
            }

            yield return Wait(2);

            gates[0].StopLoopSound();
            gates[1].StopLoopSound();
            gates[0].PlaySound("physics_car_door_default");
            gates[1].PlaySound("physics_car_door_default");

            foreach (Entity col in intro_col)
                col.Delete();
            foreach (Entity col in intro_col2)
                col.Delete();
        }
        /*
        private static void domeEasterEgg(Entity trigger, Entity player)
        {
            bool eggIsVisible = player.WorldPointInReticle_Circle(new Vector3(-9.501169f, 233.3094f, -192.0296f), 64, 64);
            if (eggIsVisible)
            {
                player.PlayLocalSound("elm_dog");
            }
        }
        */
        public static void cleanLevelEnts()
        {
            //yield return Wait(.1f);
            for (int i = 18; i < 2000; i++)
            {
                Entity ent = GetEntByNum(i);
                if (ent == null) continue;
                string entTargetName = ent.TargetName;
                string entClassName = ent.Classname;
                if (entTargetName.StartsWith("killCamEnt_") || 
                (entClassName.StartsWith("mp_") && !entClassName.StartsWith("mp_tdm") && !entClassName.StartsWith("mp_global")) ||
                entTargetName.StartsWith("auto") || entTargetName.StartsWith("heli_") ||
                entTargetName == "flag_descriptor" ||
                entTargetName == "remote_uav_range" ||
                entTargetName == "radiotrigger" ||
                (entTargetName == "grnd_zone" || entTargetName == "grnd_dropZone"))
                    ent.Delete();
            }
        }
        public static IEnumerator dome_deleteDynamicModels()
        {
            List<Entity> newEnts = new List<Entity>();
            //List<Entity> entEdits = new List<Entity>();
            for (int i = 18; i < 2000; i++)
            {
                Entity ent = GetEntByNum(i);
                if (ent == null) continue;
                string entModel = ent.Model;
                if (ent.TargetName == "animated_model")
                {
                    Entity newEnt = Spawn("script_model", ent.Origin);//Spawn 'fake' model
                    newEnt.Angles = ent.Angles;
                    newEnt.SetModel(ent.Model);
                    newEnt.Hide();
                    newEnts.Add(newEnt);
                    ent.Delete();
                }
                //else if (entModel == "vehicle_hummer_destructible")
                //entEdits.Add(ent);
                //else if (entTargetName == "explodable_barrel")
                //entEdits.Add(ent);
            }

            yield return WaitForFrame();

            foreach (Entity ent in newEnts)
            {
                string model = ent.Model;
                if (model.StartsWith("fence_tarp_"))
                {
                    ent.TargetName = "dynamic_model";
                    if (model != "fence_tarp_134x76") ent.ScriptModelPlayAnim(model + "_med_01");
                    else if (model == "fence_tarp_134x76") ent.ScriptModelPlayAnim(model + "_med_02");
                }
                else if (model == "machinery_windmill")
                {
                    ent.TargetName = "dynamic_model";
                    ent.ScriptModelPlayAnim("windmill_spin_med");
                    List<Entity> dome_windmillList = Entity.Level.GetField<List<Entity>>("windmillList");
                    dome_windmillList.Add(ent);
                    Entity.Level.SetField("windmillList", new Parameter(dome_windmillList));
                }
                else if (model.Contains("foliage"))
                {
                    ent.TargetName = "dynamic_model";
                    ent.ScriptModelPlayAnim("foliage_desertbrush_1_sway");
                }
                else if (model.Contains("oil_pump_jack"))
                {
                    ent.TargetName = "dynamic_model";
                    ent.ScriptModelPlayAnim("oilpump_pump0" + (AIZ.rng.Next(2) + 1));
                }
                else if (model == "accessories_windsock_large")
                {
                    ent.TargetName = "dynamic_model";
                    ent.ScriptModelPlayAnim("windsock_large_wind_medium");
                }
                ent.Show();
            }
        }
        
        public static string getAlliesFlagModel(string mapname)
        {
            switch (mapname)
            {
                case "mp_alpha":
                case "mp_dome":
                case "mp_exchange":
                case "mp_hardhat":
                case "mp_interchange":
                case "mp_lambeth":
                case "mp_radar":
                case "mp_cement":
                case "mp_hillside_ss":
                case "mp_morningwood":
                case "mp_overwatch":
                case "mp_park":
                case "mp_qadeem":
                case "mp_restrepo_ss":
                case "mp_terminal_cls":
                case "mp_roughneck":
                case "mp_boardwalk":
                case "mp_moab":
                case "mp_nola":
                    return "prop_flag_delta";
                case "mp_bootleg":
                case "mp_bravo":
                case "mp_carbon":
                case "mp_mogadishu":
                case "mp_village":
                case "mp_shipbreaker":
                    return "prop_flag_pmc";
                case "mp_paris":
                    return "prop_flag_gign";
                case "mp_plaza2":
                case "mp_seatown":
                case "mp_underground":
                case "mp_aground_ss":
                case "mp_courtyard_ss":
                case "mp_italy":
                case "mp_meteora":
                    return "prop_flag_sas";
            }
            return "";
        }
        public static string getAxisFlagModel(string mapname)
        {
            switch (mapname)
            {
                case "mp_alpha":
                case "mp_bootleg":
                case "mp_dome":
                case "mp_exchange":
                case "mp_hardhat":
                case "mp_interchange":
                case "mp_lambeth":
                case "mp_paris":
                case "mp_plaza2":
                case "mp_radar":
                case "mp_underground":
                case "mp_cement":
                case "mp_hillside_ss":
                case "mp_overwatch":
                case "mp_park":
                case "mp_restrepo_ss":
                case "mp_terminal_cls":
                case "mp_roughneck":
                case "mp_boardwalk":
                case "mp_moab":
                case "mp_nola":
                    return "prop_flag_speznas";
                case "mp_bravo":
                case "mp_carbon":
                case "mp_mogadishu":
                case "mp_village":
                case "mp_shipbreaker":
                    return "prop_flag_africa";
                case "mp_seatown":
                case "mp_aground_ss":
                case "mp_courtyard_ss":
                case "mp_meteora":
                case "mp_morningwood":
                case "mp_qadeem":
                case "mp_italy":
                    return "prop_flag_ic";
            }
            return "";
        }

        private static string[] weaponModels = {
                "weapon_steyr_blue_tiger" , "weapon_mp412", "weapon_desert_eagle_iw5",
            "weapon_ak47_iw5", "weapon_scar_iw5", "weapon_mp5_iw5", "weapon_p90_iw5",  "weapon_m60_iw5", "weapon_as50_iw5",
            "weapon_remington_msr_iw5",  "weapon_aa12_iw5", "weapon_model1887", "weapon_mp9_iw5", "weapon_walther_p99_iw5", "weapon_fn_fiveseven_iw5", "weapon_44_magnum_iw5", "weapon_fmg_iw5", "weapon_g18_iw5", "weapon_smaw",
            "weapon_xm25", "weapon_m320_gl", "weapon_m4_iw5", "weapon_m16_iw5", "weapon_cm901", "weapon_type95_iw5", "weapon_remington_acr_iw5", "weapon_m14_iw5", "weapon_g36_iw5", "weapon_fad_iw5", "weapon_ump45_iw5", "weapon_pp90m1_iw5", "weapon_uzi_m9_iw5", "weapon_mp7_iw5",
            "weapon_dragunov_iw5", "weapon_m82_iw5", "weapon_l96a1_iw5", "weapon_rsass_iw5", "weapon_sa80_iw5", "weapon_mg36", "weapon_pecheneg_iw5", "weapon_mk46_iw5", "weapon_usas12_iw5", "weapon_ksg_iw5", "weapon_spas12_iw5", "weapon_striker_iw5",
            "weapon_riot_shield_mp", "viewmodel_light_stick", "weapon_skorpion_iw5", "weapon_javelin"
        };
        private static string[] weaponNames = { "iw5_m60jugg_mp_eotechlmg_camo07", "iw5_mp412_mp", "iw5_deserteagle_mp",
            "iw5_ak47_mp", "iw5_scar_mp", "iw5_mp5_mp", "iw5_p90_mp", "iw5_m60_mp", "iw5_as50_mp_as50scope",
            "iw5_msr_mp_msrscope", "iw5_aa12_mp", "iw5_1887_mp", "iw5_mp9_mp", "iw5_p99_mp", "iw5_fnfiveseven_mp", "iw5_44magnum_mp", "iw5_fmg9_mp", "iw5_g18_mp", "iw5_smaw_mp",
            "iw5_xm25_mp", "m320_mp", "iw5_m4_mp", "iw5_m16_mp", "iw5_cm901_mp", "iw5_type95_mp", "iw5_acr_mp", "iw5_mk14_mp", "iw5_g36c_mp", "iw5_fad_mp", "iw5_ump45_mp", "iw5_pp90m1_mp", "iw5_m9_mp", "iw5_mp7_mp",
            "iw5_dragunov_mp_dragunovscope", "iw5_barrett_mp_barrettscope", "iw5_l96a1_mp_l96a1scope", "iw5_rsass_mp_rsassscope", "iw5_sa80_mp", "iw5_mg36_mp", "iw5_pecheneg_mp", "iw5_mk46_mp", "iw5_usas12_mp", "iw5_ksg_mp", "iw5_spas12_mp", "iw5_striker_mp",
            "riotshield_mp", "lightstick_mp", "iw5_skorpion_mp_eotechsmg_scope7", "uav_strike_missile_mp"
        };
        private static string[] localizedNames = { "AUG HBAR", "MP412", "Desert Eagle",
            "AK-47", "SCAR-L", "MP5", "P90", "M60", "AS50",
            "MSR", "AA-12", "Model 1887", "MP9", "P99", "Five Seven", "44. Magnum", "FMG9", "G18", "SMAW",
            "XM25", "M320 GLM", "M4A1", "M16", "CM901", "Type 95", "ACR 6.8", "MK14", "G36C", "FAD", "UMP45", "PP90M1", "PM-9", "MP7",
            "Dragunov", "Barrett .50 Cal", "L118A", "RSASS", "L86 LSW", "MG36", "PKP Pecheneg", "MK46", "USAS-12", "KSG", "SPAS-12", "Striker",
            "Riot Shield", "Glowstick", "Ray Gun", "Thundergun"
        };

        #region dome moon
        public static void initDomeMoon()
        {
            Entity bunkerZone = Entity.GetEntity(550);
            Entity bunkerEntranceZone = Entity.GetEntity(544);
            Entity backBuildingZone = Entity.GetEntity(538);
            Entity domeZone = Entity.GetEntity(59);
            if (bunkerZone.Classname == "trigger_multiple") bunkerZone.WillNeverChange();
            if (backBuildingZone.Classname == "trigger_multiple") backBuildingZone.WillNeverChange();
            if (domeZone.Classname == "trigger_multiple") domeZone.WillNeverChange();
            if (bunkerEntranceZone.Classname == "trigger_multiple") bunkerEntranceZone.WillNeverChange();

            Entity backHallwayZone = Spawn("trigger_radius", new Vector3(1302.512f, 1400.346f, -254.875f), 0, 186, 512);
            Entity buildingZone = Spawn("trigger_radius", new Vector3(980.6837f, 2035.045f, -254.875f), 0, 612, 512);
            Entity bunkerMidZone = Spawn("trigger_radius", new Vector3(-1108.737f, 901.4845f, -451.875f), 0, 312, 612);
            bunkerMidZone.Angles = new Vector3(90, 0, 0);
            bunkerMidZone.WillNeverChange();
            backHallwayZone.WillNeverChange();
            buildingZone.WillNeverChange();

            moonTriggers[0] = bunkerZone;
            moonTriggers[1] = bunkerEntranceZone;
            moonTriggers[2] = bunkerMidZone;
            moonTriggers[3] = backBuildingZone;
            moonTriggers[4] = backHallwayZone;
            moonTriggers[5] = buildingZone;
            moonTriggers[6] = domeZone;
        }
        private static void initMoon()
        {
            AmbientStop();
            SetSunlight(new Vector3(-1, -1, 1));
            VisionSetNaked("cobra_sunset3");
            AIZ.vision = "cobra_sunset3";
            monitorMoonGravity();

            for (int i = 0; i < 2000; i++)
            {
                Entity e = Entity.GetEntity(i);
                if (e.Model == "vehicle_hummer_destructible")
                {
                    e.SetModel("com_satellite_dish_big");
                    e.SetCanDamage(false);
                    OnInterval(4000, () => rotateEntity(e));
                }
            }
        }
        #endregion

        #region easter eggs
        #region dome
        public static void dome_initEasterEgg()
        {
            PreCacheShader("specialty_perks_all");

            Entity t = Spawn("script_origin", new Vector3(-40.87393f, 413.6856f, -393.5953f));//GetEnt("trigger_use_touch", "classname");
            if (t != null)
            {
                t.TargetName = "aiz_dome_trigger_1";
                //t.Origin = new Vector3(-40.87393f, 413.6856f, -393.5953f);
                t.SetField("hasBeenActivated", false);
                //t.WillNeverChange();

            }

            Entity t2 = Spawn("script_origin", new Vector3(-1442.411f, 1079.49f, -426.0376f));//GetEnt("sab_bomb_defuse_allies", "targetname");
            if (t2 != null)
            {
                t2.TargetName = "aiz_dome_cabinet_trigger_bunker";
                //t2.Origin = new Vector3(-1442.411f, 1079.49f, -426.0376f);
                t2.SetField("hasBeenActivated", false);
                //t2.WillNeverChange();
            }
            Entity t3 = Spawn("script_origin", new Vector3(-67.49985f, -236.227f, -390.375f));//GetEnt("sab_bomb_defuse_axis", "targetname");
            if (t3 != null)
            {
                t3.TargetName = "aiz_dome_cabinet_trigger_dome";
                //t3.Origin = new Vector3(-67.49985f, -236.227f, -390.375f);
                t3.SetField("hasBeenActivated", false);
                //t3.WillNeverChange();
            }

            Entity electronicCabinet1 = Spawn("script_model", new Vector3(-1482.5f, 1055.2f, -426.5f));
            electronicCabinet1.Angles = new Vector3(0, 115.9f, 0);
            electronicCabinet1.SetModel("icbm_electronic_cabinet2_busted");
            spawnCrate(electronicCabinet1.Origin + new Vector3(0, 0, 40), new Vector3(90, 115.9f, 0), true, false);//Collision
            electronicCabinet1.TargetName = "dome_cabinet_bunker";

            Entity electronicCabinet2 = Spawn("script_model", new Vector3(-111.5f, -257.2f, -390.4f));
            electronicCabinet2.Angles = new Vector3(0, 112.7f, 0);
            electronicCabinet2.SetModel("icbm_electronic_cabinet2_busted");
            spawnCrate(electronicCabinet2.Origin + new Vector3(0, 0, 40), new Vector3(90, 112.7f, 0), true, false);//Collision
            electronicCabinet2.TargetName = "dome_cabinet_dome";

            Entity triggerBunker = GetEnt("aiz_dome_cabinet_trigger_bunker", "targetname");
            triggerBunker.SetField("cabinet", electronicCabinet1);
            Entity triggerDome = GetEnt("aiz_dome_cabinet_trigger_dome", "targetname");
            triggerDome.SetField("cabinet", electronicCabinet2);


            Entity teddy1 = Spawn("script_model", new Vector3(521, 304, -343));
            teddy1.SetModel("com_teddy_bear");
            teddy1.Angles = new Vector3(0, 24, 0);
            teddy1.TargetName = "teddy_1";
            teddy1.SetCanDamage(true);
            teddy1.SetCanRadiusDamage(false);

            Entity teddy2 = Spawn("script_model", new Vector3(530, 369, -341));
            teddy2.SetModel("com_teddy_bear");
            teddy2.Angles = new Vector3(0, 210, 0);
            teddy2.TargetName = "teddy_2";
            teddy2.SetCanDamage(false);
            teddy2.SetCanRadiusDamage(false);
            teddy2.Hide();

            Entity teddy3 = Spawn("script_model", new Vector3(-773, 55, -362));
            teddy3.SetModel("com_teddy_bear");
            teddy3.Angles = new Vector3(0, 201, 0);
            teddy3.TargetName = "teddy_3";
            teddy3.SetCanDamage(false);
            teddy3.SetCanRadiusDamage(false);
            teddy3.Hide();

            Entity teddy4 = Spawn("script_model", new Vector3(590, -553, -370));
            teddy4.SetModel("com_teddy_bear");
            teddy4.Angles = new Vector3(85, 190, 0);
            teddy4.TargetName = "teddy_4";
            teddy4.SetCanDamage(false);
            teddy4.SetCanRadiusDamage(false);
            teddy4.Hide();

            StartAsync(dome_watchTeddyDamage(teddy1, teddy2, teddy3, teddy4));
        }
        private static IEnumerator dome_watchTeddyDamage(Entity teddy1, Entity teddy2, Entity teddy3, Entity teddy4)
        {
            yield return teddy1.WaitTill("damage");

            //teddy1.Hide();
            teddy1.MoveZ(30, 1, .5f);
            teddy1.SetCanDamage(false);
            yield return Wait(1);

            teddy1.PlaySound("re_pickup_paper");
            PlayFX(AIZ.fx_disappear, teddy1.Origin);
            teddy1.Hide();
            teddy3.Show();
            teddy3.SetCanDamage(true);
            yield return teddy3.WaitTill("damage");

            //teddy3.Hide();
            teddy3.MoveZ(30, 1, .5f);
            teddy3.SetCanDamage(false);
            yield return Wait(1);

            teddy3.PlaySound("re_pickup_paper");
            PlayFX(AIZ.fx_disappear, teddy3.Origin);
            teddy3.Hide();
            teddy2.Show();
            teddy2.SetCanDamage(true);
            yield return teddy2.WaitTill("damage");

            //teddy2.Hide();
            teddy2.MoveZ(30, 1, .5f);
            teddy2.SetCanDamage(false);
            yield return Wait(1);

            teddy2.PlaySound("re_pickup_paper");
            PlayFX(AIZ.fx_disappear, teddy2.Origin);
            teddy2.Hide();
            teddy4.Show();
            teddy4.SetCanDamage(true);
            yield return teddy4.WaitTill("damage");

            //teddy4.Hide();
            teddy4.MoveZ(30, 1, .5f);
            teddy4.SetCanDamage(false);
            yield return Wait(1);
            PlayFX(AIZ.fx_disappear, teddy4.Origin);
            teddy4.Hide();
            teddy4.PlaySound("scrambler_beep");

            Entity step2Trigger = GetEnt("aiz_dome_trigger_1", "targetname");
            if (step2Trigger != null) makeUsable(step2Trigger, "dome_eeDog", 150);
            //dome_watchEasterEggTrigger1();
        }
        private static void dome_checkEasterEggTrigger1(Entity trigger, Entity player)
        {
            //Entity trigger = GetEnt("aiz_dome_trigger_1", "targetname");
            //if (trigger != null)
            //{
            if (trigger.GetField<bool>("hasBeenActivated")) return;

            Vector3 viewPoint = new Vector3(26, 246, -187);
            bool isVisible = player.WorldPointInReticle_Circle(viewPoint, 65, 65);

            if (isVisible)
            {
                for (int i = 0; i < 50; i++)
                    player.PlayLocalSound("elm_dog");

                trigger.SetField("hasBeenActivated", true);
                removeUsable(trigger);
                dome_easterEggStep3();
            }
            //}
        }
        private static void dome_easterEggStep3()
        {
            foreach (Entity ent in Entity.Level.GetField<List<Entity>>("windmillList"))
            {
                ent.SetCanDamage(true);
                ent.SetCanRadiusDamage(true);
                ent.SetField("destroyed", false);
                ent.OnNotify("damage", (mill, damage, attacker, direction_vec, point, meansOfDeath, modelName, partName, tagName, iDFlags, weapon) =>
                dome_onWindmillDamage(mill, (int)damage, (Entity)attacker, direction_vec, point, (string)meansOfDeath, (string)modelName, (string)partName, (string)tagName, (int)iDFlags, (string)weapon));
            }
        }
        private static void dome_onWindmillDamage(Entity mill, int damage, Entity player, Parameter direction, Parameter point, string mod, string modelname, string partname, string tagname, int dFlags, string weapon)
        {
            if (mill.GetField<bool>("destroyed")) return; 

            if (mod == "MOD_PROJECTILE" || mod == "MOD_PROJECTILE_SPLASH" || mod == "MOD_GRENADE" || mod == "MOD_GRENADE_SPLASH" || mod == "MOD_EXPLOSIVE")
            {
                mill.SetField("destroyed", true);
                AfterDelay(1500, () => PlayFX(AIZ.fx_nuke, mill.Origin));
                mill.ScriptModelClearAnim();
                Vector3 dir = direction.As<Vector3>();
                dir.Normalize();
                Vector3 fallPos = new Vector3(dir.X * 90, 0, dir.Z * 90);
                mill.RotateTo(mill.Angles + fallPos, 3, .05f, .25f);
                mill.PlaySound("hind_helicopter_hit");
                AfterDelay(2800, () => mill.PlaySound("hind_helicopter_crash_dist"));
                List<Entity> dome_windmillList = Entity.Level.GetField<List<Entity>>("windmillList");
                dome_windmillList.Remove(mill);
                Entity.Level.SetField("windmillList", new Parameter(dome_windmillList));

                if (dome_windmillList.Count == 0)
                {
                    mill.PlaySound("new_perk_unlocks");
                    Entity triggerBunker = GetEnt("aiz_dome_cabinet_trigger_bunker", "targetname");
                    Entity triggerDome = GetEnt("aiz_dome_cabinet_trigger_dome", "targetname");
                    if (triggerBunker != null) makeUsable(triggerBunker, "dome_eeBunkerCabinet", 50);
                    if (triggerDome != null) makeUsable(triggerDome, "dome_eeDomeCabinet", 50);
                    //dome_easterEggStep4();
                }
            }
        }
        private static void dome_checkEasterEggStep4_A(Entity t)
        {
            if (t.GetField<bool>("hasBeenActivated")) return;

            t.SetField("hasBeenActivated", true);
            t.GetField<Entity>("cabinet").SetModel("icbm_electronic_cabinet2");
            t.PlaySound("switch_auto_lights_on");
            t.SetField("time", 0);
            Entity triggerDome = GetEnt("aiz_dome_cabinet_trigger_dome", "targetname");

            OnInterval(50, () => dome_monitorEasterEggStep4_A(t, triggerDome));
        }

        private static bool dome_monitorEasterEggStep4_A(Entity trigger, Entity triggerDome)
        {
            if (trigger.GetField<int>("time") > 10)
            {
                trigger.SetField("hasBeenActivated", false);
                trigger.GetField<Entity>("cabinet").SetModel("icbm_electronic_cabinet2_busted");
                trigger.PlaySound("switch_auto_lights_off");
                return false;
            }
            else if (triggerDome.GetField<bool>("hasBeenActivated"))
            {
                trigger.PlaySound("ims_plant");
                removeUsable(trigger);
                return false;
            }
            trigger.SetField("time", trigger.GetField<int>("time") + 1);
            return true;
        }
        private static void dome_checkEasterEggStep4_B(Entity t)//Bunker
        {
            if (t.GetField<bool>("hasBeenActivated")) return;

            t.SetField("hasBeenActivated", true);
            t.GetField<Entity>("cabinet").SetModel("icbm_electronic_cabinet2");
            t.PlaySound("switch_auto_lights_on");
            t.SetField("time", 0);
            Entity triggerBunker = GetEnt("aiz_dome_cabinet_trigger_bunker", "targetname");

            OnInterval(50, () => dome_monitorEasterEggStep4_B(t, triggerBunker));
        }
        private static bool dome_monitorEasterEggStep4_B(Entity trigger, Entity triggerBunker)
        {
            if (trigger.GetField<int>("time") > 10)
            {
                trigger.SetField("hasBeenActivated", false);
                trigger.GetField<Entity>("cabinet").SetModel("icbm_electronic_cabinet2_busted");
                trigger.PlaySound("switch_auto_lights_off");
                return false;
            }
            else if (triggerBunker.GetField<bool>("hasBeenActivated"))
            {
                trigger.PlaySound("ims_plant");
                removeUsable(trigger);
                dome_easterEggReward();
                return false;
            }
            trigger.SetField("time", trigger.GetField<int>("time") + 1);
            return true;
        }
        private static void easterEgg_awardAllPerks(Entity player = null)
        {
            if (player != null)
            {
                bonusDrops.giveAllPerks(player);
                player.SetField("allPerks", true);
                hud.scoreMessage(player, AIZ.gameStrings[327]);
            }
            else
            {
                foreach (Entity players in Players)
                {
                    if (!players.HasField("isDown")) continue;
                    if (!players.IsAlive) continue;

                    bonusDrops.giveAllPerks(players);
                    players.SetField("allPerks", true);
                    hud.scoreMessage(players, AIZ.gameStrings[327]);
                }
            }
        }
        public static void dome_easterEggReward()
        {
            easterEgg_awardAllPerks();

            Entity crate = Spawn("script_model", new Vector3(-92f, 2235f, -291f));
            crate.Angles = new Vector3(0, -3.8f, 0);
            crate.SetModel("com_plasticcase_green_big_us_dirt");
            if (_caseCollision == null) crate.CloneBrushModelToScriptModel(_airdropCollision);
            else crate.CloneBrushModelToScriptModel(_caseCollision);

            Entity weapCrate = Spawn("script_model", crate.Origin + new Vector3(0, 0, 30));
            weapCrate.Angles = crate.Angles;
            weapCrate.SetModel("com_plasticcase_green_rifle");

            Entity weapon = wallWeapon(weapCrate.Origin + new Vector3(15, 0, 10), weapCrate.Angles - new Vector3(0, 0, 90), "stinger_mp", 0);
            OnInterval(100, () => dome_watchEasterEggReward(weapon));
        }
        private static bool dome_watchEasterEggReward(Entity weapon)
        {
            if (weapon.GetField<bool>("bought"))
            {
                PlayFX(AIZ.fx_disappear, weapon.Origin);
                removeUsable(weapon);
                return false;
            }
            else return true;
        }
        #endregion
        #endregion
    }
}
