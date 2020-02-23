using System.Collections;
using System.Text;
using InfinityScript;
using static InfinityScript.GSCFunctions;

namespace AIZombiesSupreme
{
    public class hud : BaseScript
    {
        public static uint EMPTime = 0;
        private static readonly string[] PerkDescs = { AIZ.gameStrings[102], AIZ.gameStrings[103], AIZ.gameStrings[104], AIZ.gameStrings[105], AIZ.gameStrings[106], AIZ.gameStrings[107], AIZ.gameStrings[108], AIZ.gameStrings[109] };
        public static bool powerBox = false;
        private static bool killHud = false;
        private static bool doubleHud = false;

        private static HudElem round;
        private static HudElem zombieCounter;
        private static HudElem powerup1;
        private static HudElem powerup2;
        //private static HudElem info;
        public static HudElem intermission;
        private static HudElem roundStart;
        private static HudElem roundEnd;
        public static HudElem powerHud;

        public static bool stringsCleared = false;
        //Voting
        private static readonly string[] mapList = new string[36]{"-", "mp_alpha", "mp_bootleg", "mp_bravo", "mp_carbon", "mp_dome"
        , "mp_exchange", "mp_hardhat", "mp_interchange", "mp_lambeth", "mp_mogadishu", "mp_paris", "mp_plaza2",
        "mp_radar", "mp_seatown", "mp_underground", "mp_village", "mp_italy", "mp_park", "mp_morningwood", "mp_overwatch", "mp_aground_ss",
        "mp_courtyard_ss", "mp_cement", "mp_hillside_ss", "mp_meteora", "mp_qadeem", "mp_restrepo_ss", "mp_terminal_cls", "mp_crosswalk_ss",
        "mp_six_ss", "mp_burn_ss", "mp_shipbreaker", "mp_roughneck", "mp_nola", "mp_moab"};
        public static readonly string[] mapNames = new string[36];
        public static readonly string[] mapDesc = new string[36];
        //private static int mapSelection = 0;
        private static byte[] mapVotes = new byte[3]{0, 0, 0};
        private static byte[] mapLists = new byte[3]{0, 0, 0};
        private static bool votingFinished = false;

        public static void createPlayerHud(Entity player)
        {
            if (player.HasField("aizHud_created")) return;

            //Ammo counters
            HudElem ammoSlash = HudElem.CreateFontString(player, HudElem.Fonts.HudSmall, 1f);
            ammoSlash.SetPoint("bottom right", "bottom right", -150, -28);
            ammoSlash.HideWhenInMenu = true;
            ammoSlash.HideWhenDead = true;
            ammoSlash.Archived = true;
            ammoSlash.LowResBackground = true;
            ammoSlash.AlignX = HudElem.XAlignments.Left;
            ammoSlash.Alpha = 1;
            ammoSlash.SetText("/");
            ammoSlash.Sort = 0;

            HudElem ammoStock = HudElem.CreateFontString(player, HudElem.Fonts.HudSmall, 1f);
            ammoStock.Parent = ammoSlash;
            ammoStock.SetPoint("bottom left", "bottom left", 8, 0);
            ammoStock.HideWhenInMenu = true;
            ammoStock.HideWhenDead = true;
            ammoStock.Archived = true;
            ammoStock.SetValue(0);
            ammoStock.Sort = 0;

            HudElem ammoClip = HudElem.CreateFontString(player, HudElem.Fonts.HudBig, 1f);
            ammoClip.Parent = ammoSlash;
            ammoClip.SetPoint("right", "right", -5, -4);
            ammoClip.HideWhenInMenu = true;
            ammoClip.HideWhenDead = true;
            ammoClip.Archived = true;
            ammoClip.SetValue(0);
            ammoClip.Sort = 0;

            HudElem weaponName = HudElem.CreateFontString(player, HudElem.Fonts.HudSmall, 1f);
            weaponName.SetPoint("bottom right", "bottom right", -140, -8);
            weaponName.HideWhenInMenu = true;
            weaponName.HideWhenDead = true;
            weaponName.Archived = true;
            weaponName.Alpha = 1;
            weaponName.SetText(string.Empty);
            weaponName.Sort = 0;

            //Set out player fields for ammo hud
            player.SetField("hud_ammoSlash", ammoSlash);
            player.SetField("hud_ammoStock", ammoStock);
            player.SetField("hud_ammoClip", ammoClip);
            player.SetField("hud_weaponName", weaponName);

            //Item divider
            HudElem divider = HudElem.CreateIcon(player, "hud_iw5_divider", 200, 24);
            divider.SetPoint("BOTTOMRIGHT", "BOTTOMRIGHT", -67, -20);
            divider.HideWhenInMenu = true;
            divider.HideWhenDead = true;
            divider.Alpha = 1;
            divider.Archived = true;
            player.SetField("hud_divider", divider);
            divider.Sort = 1;

            //Hitmarker
            HudElem hitFeedback = NewClientHudElem(player);
            hitFeedback.HorzAlign = HudElem.HorzAlignments.Center;
            hitFeedback.VertAlign = HudElem.VertAlignments.Middle;
            hitFeedback.X = -12;
            hitFeedback.Y = -12;
            hitFeedback.Alpha = 0;
            hitFeedback.Archived = true;
            hitFeedback.HideWhenDead = false;
            hitFeedback.SetShader("damage_feedback", 24, 48);
            hitFeedback.Sort = 2;
            player.SetField("hud_damageFeedback", hitFeedback);

            //Perk hud
            HudElem jugg = HudElem.CreateIcon(player, "specialty_placeholder", 30, 30);
            jugg.SetPoint("BOTTOM LEFT", "BOTTOM LEFT", 0, -54);
            jugg.HideWhenInMenu = true;
            jugg.HideWhenDead = true;
            jugg.Foreground = true;
            jugg.Archived = true;
            jugg.Alpha = 0;
            jugg.Sort = 3;

            HudElem stamina = HudElem.CreateIcon(player, "specialty_placeholder", 30, 30);
            stamina.Parent = jugg;
            stamina.SetPoint("CENTER RIGHT", "CENTER RIGHT", 32, 0);
            stamina.HideWhenInMenu = true;
            stamina.HideWhenDead = true;
            stamina.Foreground = true;
            stamina.Archived = true;
            stamina.Alpha = 0;
            stamina.Sort = 4;

            HudElem speed = HudElem.CreateIcon(player, "specialty_placeholder", 30, 30);
            speed.Parent = jugg;
            speed.SetPoint("CENTER RIGHT", "CENTER RIGHT", 64, 0);
            speed.HideWhenInMenu = true;
            speed.HideWhenDead = true;
            speed.Foreground = true;
            speed.Archived = true;
            speed.Alpha = 0;
            speed.Sort = 5;

            HudElem mulekick = HudElem.CreateIcon(player, "specialty_placeholder", 30, 30);
            mulekick.Parent = jugg;
            mulekick.SetPoint("CENTER RIGHT", "CENTER RIGHT", 96, 0);
            mulekick.HideWhenInMenu = true;
            mulekick.HideWhenDead = true;
            mulekick.Foreground = true;
            mulekick.Archived = true;
            mulekick.Alpha = 0;
            mulekick.Sort = 6;

            HudElem dtap = HudElem.CreateIcon(player, "specialty_placeholder", 30, 30);
            dtap.Parent = jugg;
            dtap.SetPoint("CENTER RIGHT", "CENTER RIGHT", 128, 0);
            dtap.HideWhenInMenu = true;
            dtap.HideWhenDead = true;
            dtap.Foreground = true;
            dtap.Archived = true;
            dtap.Alpha = 0;
            dtap.Sort = 7;

            HudElem stalker = HudElem.CreateIcon(player, "specialty_placeholder", 30, 30);
            stalker.Parent = jugg;
            stalker.SetPoint("CENTER RIGHT", "CENTER RIGHT", 160, 0);
            stalker.HideWhenInMenu = true;
            stalker.HideWhenDead = true;
            stalker.Foreground = true;
            stalker.Archived = true;
            stalker.Alpha = 0;
            stalker.Sort = 8;

            HudElem perk7 = HudElem.CreateIcon(player, "specialty_placeholder", 40, 40);
            perk7.SetPoint("BOTTOM MIDDLE", "BOTTOM MIDDLE", 400, -5);
            perk7.HideWhenInMenu = true;
            perk7.HideWhenDead = true;
            perk7.Foreground = true;
            perk7.Archived = true;
            perk7.Alpha = 0;
            perk7.Sort = 9;

            //Score hud
            HudElem scoreHud = HudElem.CreateFontString(player, HudElem.Fonts.HudSmall, 1);
            scoreHud.SetPoint("TOP RIGHT", "TOP RIGHT", -120, 10);
            scoreHud.HideWhenInMenu = true;
            scoreHud.Foreground = true;
            scoreHud.Archived = true;
            scoreHud.Alpha = 1;
            scoreHud.Color = new Vector3(0, 0.9f, 0);
            scoreHud.GlowColor = new Vector3(0, 0.9f, 0);
            scoreHud.GlowAlpha = 0.5f;
            scoreHud.SetText(AIZ.gameStrings[180]);
            scoreHud.Sort = 10;
            HudElem scoreNumber = HudElem.CreateFontString(player, HudElem.Fonts.HudSmall, 1);
            scoreNumber.Parent = scoreHud;
            scoreNumber.SetPoint("RIGHT", "RIGHT", 0, 2);
            scoreNumber.AlignX = HudElem.XAlignments.Left;
            scoreNumber.HideWhenInMenu = true;
            scoreNumber.Foreground = true;
            scoreNumber.Archived = true;
            scoreNumber.Alpha = 1;
            scoreNumber.Color = new Vector3(0, 0.9f, 0);
            scoreNumber.GlowColor = new Vector3(0, 0.9f, 0);
            scoreNumber.GlowAlpha = 0.5f;
            scoreNumber.SetValue(player.GetField<int>("cash"));
            scoreNumber.Sort = 10;

            HudElem pointHud = HudElem.CreateFontString(player, HudElem.Fonts.HudSmall, 1);
            pointHud.Parent = scoreHud;
            pointHud.SetPoint("LEFT", "LEFT", -74, 12);
            pointHud.HideWhenInMenu = true;
            pointHud.Foreground = true;
            pointHud.Archived = true;
            pointHud.Alpha = 1;
            pointHud.Color = new Vector3(0, 0.85f, 0.85f);
            pointHud.GlowColor = new Vector3(0, 0.85f, 0.85f);
            pointHud.GlowAlpha = 0.5f;
            pointHud.SetText(AIZ.gameStrings[181]);
            pointHud.Sort = 11;
            HudElem pointNumber = HudElem.CreateFontString(player, HudElem.Fonts.HudSmall, 1);
            pointNumber.Parent = pointHud;
            pointNumber.SetPoint("LEFT", "LEFT", 120, 0);//Set value to top left
            pointNumber.AlignX = HudElem.XAlignments.Left;
            pointNumber.HideWhenInMenu = true;
            pointNumber.Foreground = true;
            pointNumber.Archived = true;
            pointNumber.Alpha = 1;
            pointNumber.Color = new Vector3(0, 0.85f, 0.85f);
            pointNumber.GlowColor = new Vector3(0, 0.85f, 0.85f);
            pointNumber.GlowAlpha = 0.5f;
            pointNumber.SetValue(player.GetField<int>("points"));

            //Score popups
            HudElem scorePop = HudElem.CreateFontString(player, HudElem.Fonts.HudSmall, 1.5f);
            //HudElem scorePop = HudElem.GetHudElem(65537);
            scorePop.SetPoint("BOTTOMCENTER", "BOTTOMCENTER", 0, -75);
            scorePop.HideWhenInMenu = true;
            scorePop.Archived = true;
            scorePop.Alpha = 1;
            scorePop.GlowAlpha = 0.3f;
            scorePop.SetField("addScore", 0);
            scorePop.Sort = 15;

            HudElem scoreLine = HudElem.CreateIcon(player, "line_horizontal", 192, 2);
            scoreLine.SetPoint("BOTTOMCENTER", "BOTTOMCENTER", 0, -76);
            scoreLine.HideWhenInMenu = true;
            scoreLine.Archived = true;
            scoreLine.Alpha = 0;
            scoreLine.Sort = 15;

            HudElem scoreMessage = HudElem.CreateFontString(player, HudElem.Fonts.HudSmall, 1);
            //HudElem scoreMessage = HudElem.GetHudElem(65538);
            scoreMessage.SetPoint("BOTTOMCENTER", "BOTTOMCENTER", 0, -60);
            scoreMessage.HideWhenInMenu = true;
            scoreMessage.Archived = true;
            scoreMessage.Alpha = 1;
            scoreMessage.GlowColor = new Vector3(0, 0.65f, 1);
            scoreMessage.GlowAlpha = 0.3f;
            scoreMessage.Sort = 15;

            //Streaklist
            HudElem killstreakList = HudElem.CreateFontString(player, HudElem.Fonts.HudBig, .60f);
            killstreakList.SetPoint("BOTTOM RIGHT", "BOTTOM RIGHT", -70, -150);
            killstreakList.AlignX = HudElem.XAlignments.Left;
            killstreakList.HideWhenInMenu = true;
            killstreakList.HideWhenDead = true;
            killstreakList.Archived = true;
            killstreakList.Alpha = 1;
            killstreakList.SetText("");
            killstreakList.SetField("text", "");
            killstreakList.Sort = 16;

            //usables message
            HudElem message = HudElem.CreateFontString(player, HudElem.Fonts.Default, 1.6f);
            message.SetPoint("CENTER", "CENTER", 0, 110);
            message.HideWhenInMenu = true;
            message.HideWhenDead = true;
            //message.Foreground = true;
            message.Alpha = 0;
            message.Archived = true;
            message.Sort = 20;
            player.SetField("hud_message", message);

            //Finish out player fields
            player.SetField("hud_perk1", jugg);
            player.SetField("hud_perk2", stamina);
            player.SetField("hud_perk3", speed);
            player.SetField("hud_perk4", mulekick);
            player.SetField("hud_perk5", dtap);
            player.SetField("hud_perk6", stalker);
            player.SetField("hud_perk7", perk7);
            player.SetField("hud_scoreCount", scoreNumber);
            player.SetField("hud_scorePop", scorePop);
            player.SetField("hud_scoreLine", scoreLine);
            player.SetField("hud_score", scoreHud);
            player.SetField("hud_point", pointHud);
            player.SetField("hud_pointNumber", pointNumber);
            player.SetField("hud_scoreMessage", scoreMessage);
            player.SetField("hud_killstreakList", killstreakList);
            //player.SetField("hud_zombieCounter", zombieCounter);
            //player.SetField("hud_round", round);
            player.SetField("aizHud_created", true);

            //Update our ammo counters
            //updateAmmoHud(player, true);
        }

        public static void createServerHud()
        {
            round = HudElem.CreateServerFontString(HudElem.Fonts.HudBig, 2);
            round.SetPoint("BOTTOM LEFT", "BOTTOM LEFT", 10, -5);
            round.HideWhenInMenu = true;
            round.Archived = true;
            //round.SetText("^1ERROR");
            round.GlowAlpha = 0.9f;
            round.GlowColor = new Vector3(0.5f, 0, 0);
            round.Color = new Vector3(0.9f, 0, 0);
            round.SetValue((int)roundSystem.Wave);
            round.Sort = 13;
            round.LowResBackground = false;
            roundSystem.onRoundChange += () => StartAsync(OnRoundChange());
            //OnNotify("roundChange", () => StartAsync(OnRoundChange(player)));

            zombieCounter = HudElem.CreateServerFontString(HudElem.Fonts.HudSmall, 1.3f);
            zombieCounter.SetPoint("BOTTOM LEFT", "BOTTOM LEFT", 80, -15);
            zombieCounter.HideWhenInMenu = true;
            zombieCounter.Archived = true;
            zombieCounter.Color = new Vector3(1, 0.5f, 0);
            zombieCounter.Alpha = 1;
            zombieCounter.SetText(AIZ.gameStrings[182] + "0");
            zombieCounter.Sort = 14;
            botUtil.onBotUpdate += updateZombieCounterForPlayer;

            AfterDelay(50, () =>
            {
                if (!AIZ.isHellMap)
                {
                    powerHud = HudElem.CreateServerFontString(HudElem.Fonts.HudSmall, 1);
                    //powerHud.Parent = scoreHud;
                    powerHud.SetPoint("TOP RIGHT", "TOP RIGHT", 0, 30);
                    powerHud.HideWhenInMenu = true;
                    powerHud.Foreground = true;
                    powerHud.Archived = true;
                    powerHud.Alpha = 1;
                    powerHud.Color = new Vector3(0.9f, 0, 0);
                    powerHud.SetText(AIZ.gameStrings[183]);
                    //player.SetField("hud_power", new Parameter(powerHud));
                    powerHud.Sort = 12;
                }
            });
        }

        private static void updateZombieCounterForPlayer()
        {
            zombieCounter.SetText(AIZ.gameStrings[182] + botUtil.botsInPlay.Count);
        }

        private static IEnumerator OnRoundChange()
        {
            //HudElem round = player.GetField<HudElem>("hud_round");
            round.FadeOverTime(1);
            round.Alpha = 0;
            foreach (Entity player in Players)
            {
                if (!AIZ.isPlayer(player) || !player.IsAlive || !player.HasField("isDown")) continue;
                player.SetField("GamblerReady", 1);
                player.GiveMaxAmmo("frag_grenade_mp");
                if (player.HasWeapon("lightstick_mp")) player.GiveMaxAmmo("lightstick_mp");
                updateAmmoHud(player, false);
            }

            yield return Wait(1);

            StartAsync(roundStartHud());
            round.SetValue((int)roundSystem.Wave);
            round.FadeOverTime(1);
            round.Alpha = 1;
        }

        public static void destroyPlayerHud(Entity player)
        {
            if (!player.HasField("aizHud_created")) return;
            HudElem[] aizHUD = new HudElem[22] {
                player.GetField<HudElem>("hud_ammoSlash"),
                player.GetField<HudElem>("hud_ammoStock"),
                player.GetField<HudElem>("hud_ammoClip"),
                player.GetField<HudElem>("hud_perk1"),
                player.GetField<HudElem>("hud_perk2"),
                player.GetField<HudElem>("hud_perk3"),
                player.GetField<HudElem>("hud_perk4"),
                player.GetField<HudElem>("hud_perk5"),
                player.GetField<HudElem>("hud_perk6"),
                player.GetField<HudElem>("hud_perk7"),
                player.GetField<HudElem>("hud_scoreCount"),
                player.GetField<HudElem>("hud_scorePop"),
                player.GetField<HudElem>("hud_scoreLine"),
                player.GetField<HudElem>("hud_scoreMessage"),
                player.GetField<HudElem>("hud_score"),
                player.GetField<HudElem>("hud_point"),
                player.GetField<HudElem>("hud_pointNumber"),
                player.GetField<HudElem>("hud_divider"),
                player.GetField<HudElem>("hud_killstreakList"),
                player.GetField<HudElem>("hud_weaponName"),
                player.GetField<HudElem>("hud_damageFeedback"),
                player.GetField<HudElem>("hud_message") };

            foreach (HudElem hud in aizHUD)
            {
                //hud.Reset();
                if (hud == null) continue;
                hud.Destroy();
            }

            player.ClearField("hud_ammoSlash");
            player.ClearField("hud_ammoStock");
            player.ClearField("hud_ammoClip");
            player.ClearField("hud_perk1");
            player.ClearField("hud_perk2");
            player.ClearField("hud_perk3");
            player.ClearField("hud_perk4");
            player.ClearField("hud_perk5");
            player.ClearField("hud_perk6");
            player.ClearField("hud_perk7");
            player.ClearField("hud_scoreCount");
            player.ClearField("hud_scorePop");
            player.ClearField("hud_scoreLine");
            player.ClearField("hud_scoreMessage");
            player.ClearField("hud_score");
            player.ClearField("hud_point");
            player.ClearField("hud_pointNumber");
            player.ClearField("hud_divider");
            player.ClearField("hud_killstreakList");
            player.ClearField("hud_weaponName");
            player.ClearField("hud_damageFeedback");
            player.ClearField("hud_message");
            player.ClearField("aizHud_created");
        }

        /*
        public static void resetGameHud()
        {
            for (int i = 65536; i < 66559; i++)
            {
                HudElem h = HudElem.GetHudElem(i);
                if (h != null)
                {
                    h.Reset();
                }
            }
        }
        */

        public static IEnumerator clearAllGameStrings()
        {
            if (stringsCleared) yield return null;

            //Log.Write(LogLevel.Info, "Clearing all strings");
            zombieCounter.ClearAllTextAfterHudelem();//Clear all strings from one hud

            //resetGameHud();

            //yield return WaitForFrame();//To allow game to catch up before sending string info

            //Clear all client strings first
            foreach (Entity player in Players)
            {
                if (!player.HasField("aizHud_created")) continue;
                HudElem[] aizHUD = new HudElem[9] { player.GetField<HudElem>("hud_ammoSlash"),
                player.GetField<HudElem>("hud_ammoStock"),
                player.GetField<HudElem>("hud_ammoClip"),
                player.GetField<HudElem>("hud_scoreMessage"),
                player.GetField<HudElem>("hud_score"),
                player.GetField<HudElem>("hud_point"),
                player.GetField<HudElem>("hud_killstreakList"),
                player.GetField<HudElem>("hud_weaponName"),
                player.GetField<HudElem>("hud_message") };

                //reassign configstrings
                aizHUD[0].SetText("/");

                //aizHUD[1].SetText("");
                //aizHUD[2].SetText("");
                //updateAmmoHud(player, false);//Update hud 0-2
                aizHUD[3].SetText("");
                //scorePopup(player, 0);
                //scoreMessage(player, "");
                aizHUD[4].SetText(AIZ.gameStrings[180]);
                aizHUD[5].SetText(AIZ.gameStrings[181]);
                aizHUD[6].SetText((string)aizHUD[6].GetField("text"));
                aizHUD[7].SetText("");
                aizHUD[8].SetText("");

                //Temp huds in case they are on screen at reset
                //intro
                if (player.HasField("hud_intro"))
                {
                    //Log.Write(LogLevel.All, "Has Intro");
                    HudElem h = player.GetField<HudElem>("hud_intro");
                    h.SetText(string.Format(AIZ.gameStrings[22],
                            player.Name, AIZ.getZombieMapname(), roundSystem.totalWaves, AIZ.version));
                }
                //perk earns
                if (player.HasField("hud_earnPerk"))
                {
                    //Log.Write(LogLevel.All, "Has Earns");
                    HudElem[] h = player.GetField<HudElem[]>("hud_earnPerk");
                    h[0].SetText((string)h[0].GetField("text"));
                    h[1].SetText((string)h[1].GetField("text"));
                }
                //hints
                /*
                if (player.HasField("hud_lbHint"))
                {
                    Log.Write(LogLevel.All, "Has Hint");
                    HudElem h = player.GetField<HudElem>("hud_lbHint");
                    h.SetText(AIZ.gameStrings[228]);
                }
                */
            }

            //Clear server hud
            zombieCounter.SetText(AIZ.gameStrings[182] + botUtil.botsInPlay.Count);

            if (!AIZ.isHellMap)
            {
                if (powerBox) powerHud.SetText(AIZ.gameStrings[275]);
                else if (!powerBox && AIZ.tempPowerActivated) powerHud.SetText(string.Format(AIZ.gameStrings[191], EMPTime));
                else powerHud.SetText("Power has not been activated");
            }

            //temp huds
            /*Disabling these checks because currently the reset happens in round. If we change the reset then we need to re-enable this
            if (roundEnd != null)
            {
                //Log.Write(LogLevel.All, "Round End");
                if (roundSystem.isBossWave) roundEnd.SetText(AIZ.gameStrings[184]);
                else if (roundSystem.isCrawlerWave) roundEnd.SetText(AIZ.gameStrings[186]);
                else roundEnd.SetText(AIZ.gameStrings[188] + roundSystem.Wave + AIZ.gameStrings[189]);
            }
            if (roundStart != null)
            {
                //Log.Write(LogLevel.All, "Round Start");
                roundStart.SetText(AIZ.gameStrings[188] + roundSystem.Wave);
            }
            if (intermission != null)
            {
                //Log.Write(LogLevel.All, "Intermission");
                intermission.SetText("Next Round In: " + AIZ.intermissionTimerNum.ToString());
            }
            */
            //if (info != null) info.SetText("");

            stringsCleared = true;
        }

        public static void updateAmmoHud(Entity player, bool updateName, string newWeapon = "")
        {
            if (!player.HasField("aizHud_created") || (player.HasField("aizHud_created") && !player.GetField<bool>("aizHud_created")))
                return;

            HudElem ammoStock = player.GetField<HudElem>("hud_ammoStock");
            HudElem ammoClip = player.GetField<HudElem>("hud_ammoClip");
            HudElem ammoSlash = player.GetField<HudElem>("hud_ammoSlash");
            string weapon = player.CurrentWeapon;
            if (newWeapon != "") weapon = newWeapon;

            //build grenades hud
            string grenade = player.GetAmmoCount("frag_grenade_mp") > 0 ? createHudShaderString("hud_us_grenade", false, 48, 48) : "  ";
            string special = (player.HasWeapon("lightstick_mp") && player.GetAmmoCount("lightstick_mp") > 0) ? createHudShaderString("specialty_tacticalinsertion") : "  ";
            //string slashText = "/\n     " + special + "  " + grenade;

            if (AIZ.isSpecialWeapon(weapon) || AIZ.isWeaponDeathMachine(weapon))
            {
                ammoStock.Alpha = 0;
                ammoClip.Alpha = 0;
                //ammoSlash.Alpha = 0;
                ammoSlash.SetText(" \n      " + grenade + special);
            }
            else
            {
                if (weapon == "uav_strike_missile_mp")
                {
                    ammoStock.SetValue(player.GetField<int>("thundergun_stock"));
                    //ammoStock.SetValue(player.GetWeaponAmmoStock(weapon));
                    ammoClip.SetValue(player.GetField<int>("thundergun_clip"));
                }
                else if (weapon == "uav_strike_projectile_mp")
                {
                    ammoStock.SetValue(player.GetField<int>("zeus_stock"));
                    //ammoStock.SetValue(player.GetWeaponAmmoStock(weapon));
                    ammoClip.SetValue(player.GetField<int>("zeus_clip"));
                }
                else
                {
                    ammoStock.SetValue(player.GetWeaponAmmoStock(weapon));
                    ammoClip.SetValue(player.GetWeaponAmmoClip(weapon));
                }
                ammoStock.Alpha = 1;
                ammoClip.Alpha = 1;
                //ammoSlash.Alpha = 1;
                ammoSlash.SetText("/\n      " + grenade + special);
            }

            if (updateName) StartAsync(updateWeaponName(player, weapon));
        }

        private static IEnumerator updateWeaponName(Entity player, string weapon)
        {
            if (!player.HasField("aizHud_created")) yield break;
            HudElem weaponName = player.GetField<HudElem>("hud_weaponName");
            weaponName.Alpha = 1;
            weaponName.SetText(getWeaponName(weapon));
            yield return Wait(1);
            weaponName.FadeOverTime(1);
            weaponName.Alpha = 0;
            //yield return Wait(1);
            //weaponName.Destroy();
        }

        public static string getWeaponName(string weapon)
        {
            weapon = AIZ.trimWeaponScope(weapon);

            switch (weapon)
            {
                case "iw5_usp45_mp":
                    return "USP .45";
                case "iw5_usp45_mp_akimbo_silencer02":
                    return "^1Mustang & Sally";
                case "iw5_p99_mp":
                    return "P99";
                case "iw5_p99_mp_tactical_xmags":
                    return "^1Puncher99";
                case "iw5_fnfiveseven_mp":
                    return "Five Seven";
                case "iw5_fnfiveseven_mp_akimbo_xmags":
                    return "^1Fifty Seven";
                case "iw5_deserteagle_mp":
                    return "Desert Eagle";
                case "iw5_deserteagle_mp_silencer02_xmags":
                    return "^1Desert Snake";
                case "iw5_mp412_mp":
                    return "MP412";
                case "iw5_mp412jugg_mp_xmags":
                    return "^2Overlord #412";
                case "iw5_44magnum_mp":
                    return ".44 Magnum";
                case "iw5_44magnum_mp_akimbo_xmags":
                    return "^1Anaconda x 2";
                case "iw5_fmg9_mp":
                    return "FMG9";
                case "iw5_fmg9_mp_akimbo_xmags":
                    return "^1Full Motion Glock 18";
                case "iw5_g18_mp":
                    return "G18";
                case "iw5_g18_mp_akimbo_xmags":
                    return "^1Groundpounder18";
                case "iw5_skorpion_mp":
                    return "Skorpion";
                case "iw5_skorpion_mp_akimbo_xmags":
                    return "^1Tarantula & Cobra";
                case "iw5_mp9_mp":
                    return "MP9";
                case "iw5_mp9_mp_reflexsmg_xmags":
                    return "^1Meat Packer 9";
                case "iw5_smaw_mp":
                    return "SMAW";
                case "rpg_mp":
                    return "^1Role Playing Gun-7";
                case "iw5_xm25_mp":
                    return "XM25";
                case "xm25_mp":
                    return "^1eXtreme Massacre";
                case "iw5_m4_mp":
                    return "M4A1";
                case "iw5_m4_mp_reflex_xmags_camo11":
                    return "^1Mad4Assault";
                case "iw5_m16_mp":
                    return "M16";
                case "iw5_m16_mp_rof_xmags_camo11":
                    return "^1Skull Crusher 16";
                case "iw5_cm901_mp":
                    return "CM901";
                case "iw5_cm901_mp_acog_xmags_camo11":
                    return "^1Crush Manager 991";
                case "iw5_type95_mp":
                    return "Type 95";
                case "iw5_type95_mp_reflex_xmags_camo11":
                    return "^1Type 190";
                case "iw5_acr_mp":
                    return "ACR 6.8";
                case "iw5_acr_mp_eotech_xmags_camo11":
                    return "^1Masada 1216";
                case "iw5_mk14_mp":
                    return "MK14";
                case "iw5_mk14_mp_reflex_xmags_camo11":
                    return "^1Massive Killer 28";
                case "iw5_ak47_mp":
                    return "AK-47";
                case "iw5_ak47_mp_gp25_xmags_camo11":
                    return "^1AK74G";
                case "iw5_g36c_mp":
                    return "G36C";
                case "iw5_g36c_mp_hybrid_xmags_camo11":
                    return "^1G36 Capper";
                case "iw5_scar_mp":
                    return "SCAR-L";
                case "iw5_scar_mp_eotech_xmags_camo11":
                    return "^1SCAR-L Bar-B-Que";
                case "iw5_fad_mp":
                    return "FAD";
                case "iw5_fad_mp_m320_xmags_camo11":
                    return "^1Functional Annihilation Device";
                case "iw5_mp5_mp":
                    return "MP5";
                case "iw5_mp5_mp_reflexsmg_xmags_camo11":
                    return "^1craMP5";
                case "iw5_ump45_mp":
                    return "UMP45";
                case "iw5_ump45_mp_eotechsmg_xmags_camo11":
                    return "^1U45 Hologram";
                case "iw5_pp90m1_mp":
                    return "PP90M1";
                case "iw5_pp90m1_mp_silencer_xmags_camo11":
                    return "^1PeePee90Mark1";
                case "iw5_p90_mp":
                    return "P90";
                case "iw5_p90_mp_rof_xmags_camo11":
                    return "^1Passive Aggressor";
                case "iw5_m9_mp":
                    return "PM-9";
                case "iw5_m9_mp_thermalsmg_xmags_camo11":
                    return "^1Suzi-Cue";
                case "iw5_mp7_mp":
                    return "MP7";
                case "iw5_mp7_mp_silencer_xmags_camo11":
                    return "^1Mortal Punisher 7";
                case "iw5_dragunov_mp_dragunovscope":
                    return "Dragunov";
                case "iw5_dragunov_mp_acog_xmags_camo11":
                    return "^1DragonBreath";
                case "iw5_barrett_mp_barrettscope":
                    return "Barrett .50 Cal";
                case "iw5_barrett_mp_acog_xmags_camo11":
                    return "^1Barrett Roller .55 Cal";
                case "iw5_l96a1_mp_l96a1scope":
                    return "L118A";
                case "iw5_l96a1_mp_l96a1scopevz_xmags_camo11":
                    return "^1L911C";
                case "iw5_as50_mp_as50scope":
                    return "AS50";
                case "iw5_as50_mp_acog_xmags_camo11":
                    return "^1AW-50";
                case "iw5_rsass_mp_rsassscope":
                    return "RSASS";
                case "iw5_rsass_mp_thermal_xmags_camo11":
                    return "^1R's Ass";
                case "iw5_msr_mp_msrscope":
                    return "MSR";
                case "iw5_msr_mp_msrscope_silencer03_xmags_camo11":
                    return "^1Mark SetteR";
                case "iw5_sa80_mp":
                    return "L86 LSW";
                case "iw5_sa80_mp_reflexlmg_xmags_camo11":
                    return "^1Lasserator86";
                case "iw5_mg36_mp":
                    return "MG36";
                case "iw5_mg36_mp_grip_xmags_camo11":
                    return "^1Masseration Gun 72";
                case "iw5_pecheneg_mp":
                    return "PKP Pecheneg";
                case "iw5_pecheneg_mp_thermal_xmags_camo11":
                    return "^1PKP Pet-ur-egg";
                case "iw5_mk46_mp":
                    return "MK46";
                case "iw5_mk46_mp_silencer_xmags_camo11":
                    return "^1MarKer902";
                case "iw5_m60_mp":
                    return "M60E4";
                case "iw5_m60_mp_reflexlmg_xmags_camo11":
                    return "^2Manhandler120";
                case "iw5_m60jugg_mp_eotechlmg_camo07":
                    return "^2AUG HBAR";
                case "iw5_m60jugg_mp_silencer_thermal_camo08":
                    return "^2AUX CrowBAR";
                case "m320_mp":
                    return "M320 GLM";
                case "iw5_usas12_mp":
                    return "USAS-12";
                case "iw5_usas12_mp_reflex_xmags_camo11":
                    return "^1USedASs-24";
                case "iw5_ksg_mp":
                    return "KSG";
                case "iw5_ksg_mp_grip_xmags_camo11":
                    return "^1Killing Spree Gun";
                case "iw5_spas12_mp":
                    return "SPAS-12";
                case "iw5_spas12_mp_grip_xmags_camo11":
                    return "^1SPAZ-24";
                case "iw5_striker_mp":
                    return "Striker";
                case "iw5_striker_mp_grip_xmags_camo11":
                    return "^1Strike-Out";
                case "iw5_aa12_mp":
                    return "AA12";
                case "iw5_aa12_mp_grip_xmags_camo11":
                    return "^1AutoAssassinator24";
                case "iw5_1887_mp":
                    return "Model 1887";
                case "iw5_1887_mp_camo11":
                    return "^1Model 1337";
                case "riotshield_mp":
                    return "Riot Shield";
                case "iw5_riotshield_mp":
                    return "^1Reinforced Internal Optimal Titanium Shield";
                case "gl_mp":
                    return "^1M640";
                case "iw5_skorpion_mp_eotechsmg":
                    return "^2Ray Gun";
                case "iw5_skorpion_mp_eotechsmg_xmags":
                    return "^1Porter's X2 Ray Gun";
                case "uav_strike_missile_mp":
                    return "^2Thundergun";
                case "uav_strike_projectile_mp":
                    //if (AIZ.altWeaponNames) return "^1Crippling Depression";
                    return "^1Zeus Cannon";
                case "stinger_mp":
                    return "^2Zapper";
                case "uav_strike_marker_mp":
                    return "^2NZ75";
                    //return "^2F2000 ACOG Scope";
                case "defaultweapon_mp":
                    return "^2Hand-gun";
                case "iw5_pecheneg_mp_rof_thermallmg":
                    return "^8Death Machine";
                case "airdrop_trap_marker_mp":
                    return "Emergency Airdrop Marker";
                case "airdrop_marker_mp":
                    return "Care Package Marker";
                case "strike_marker_mp":
                    return "Airstrike Marker";
                case "lightstick_mp":
                    return "^2Glowstick";
                case "at4_mp":
                    return "^2AT4-HS";
                case "iw5_mk12spr_mp_acog_xmags":
                    return "^1MK12 SPR";
                case "deployable_vest_marker_mp":
                    return "Explosive Ammo Marker";
                case "none":
                    return "No Weapon";
                default:
                    return "";
            }
        }

        public static void scorePopup(Entity player, int amount)
        {
            if (Entity.Level.HasField("isBlackFriday") && amount < 0)
            {
                amount /= 2;
                player.SetField("cash", player.GetField<int>("cash") + Abs(amount));//Yeah I know it's lazy but who gives a fuck =P
            }

            if (!player.HasField("aizHud_created")) return;
            HudElem score = player.GetField<HudElem>("hud_scorePop");
            HudElem scoreLine = player.GetField<HudElem>("hud_scoreLine");
            //int addScore = score.GetField<int>("addScore");
            scoreLine.Alpha = 1;
            score.SetField("addScore", (int)score.GetField("addScore") + amount);
            //addScore = score.GetField<int>("addScore");
            int scoreAdd = (int)score.GetField("addScore");
            int oldScore = scoreAdd - amount;
            if (scoreAdd > 0)
            {
                score.Color = new Vector3(0, 0.8f, 0);
                score.GlowColor = new Vector3(0, 0.8f, 0);
                scoreLine.Color = new Vector3(0, 0.8f, 0);
            }
            else if (scoreAdd < 0)
            {
                score.Color = new Vector3(0.8f, 0, 0);
                score.GlowColor = new Vector3(0.8f, 0, 0);
                scoreLine.Color = new Vector3(0.8f, 0, 0);
            }
            HudElem scoreCountHud = player.GetField<HudElem>("hud_scoreCount");
            scoreCountHud.SetValue(player.GetField<int>("cash"));
            player.Score = player.GetField<int>("cash");
            score.SetValue(scoreAdd);
            score.SetPulseFX(80, 3000, 600);
            AfterDelay(3600, () => checkForScoreChainEnd(score, scoreLine, oldScore, amount));
        }
        private static void checkForScoreChainEnd(HudElem score, HudElem scoreLine, int oldScore, int amount)
        {
            if (oldScore + amount == (int)score.GetField("addScore"))
            {
                score.SetField("addScore", 0);
                scoreLine.FadeOverTime(0.6f);
                scoreLine.Alpha = 0;
            }
        }

        public static void scoreMessage(Entity player, string message)
        {
            if (!player.HasField("aizHud_created")) return;
            HudElem messageHud = player.GetField<HudElem>("hud_scoreMessage");
            messageHud.SetText(message);
            messageHud.SetPulseFX(80, 3000, 600);
        }

        public static void updatePerksHud(Entity player, bool reset, bool instant = false)
        {
            if (!player.HasField("aizHud_created")) return;
            string Perk = player.GetField<string>("PerkBought");

            //bool hasPerk7 = player.GetField<bool>("autoRevive");

            if (reset)
            {
                for (int i = 1; i < 8; i++)
                {
                    player.GetField<HudElem>("hud_perk" + i).Alpha = 0;
                    player.SetField("perk" + i + "HudDone", false);
                }
                return;
            }

            if (player.IsAlive)
            {
                if (Perk == "waypoint_revive")//Check for autoRevive first off
                {
                    if (instant)
                        setPerkHudSlot(player, 7, Perk);
                    else
                        AfterDelay(9000, () => setPerkHudSlot(player, 7, Perk));
                    player.SetField("perk7HudDone", true);
                    player.SetField("PerkBought", "");
                    return;
                }

                for (int i = 1; i < 7; i++)
                {
                    if (!player.GetField<bool>("perk" + i + "HudDone"))
                    {
                        //These are timed to the popup animation when given.
                        if (instant)
                            setPerkHudSlot(player, i, Perk);
                        else
                            AfterDelay(9050, () => setPerkHudSlot(player, i, Perk));
                        player.SetField("perk" + i + "HudDone", true);
                        break;
                    }
                }
                player.SetField("PerkBought", "");

            }
        }
        private static void setPerkHudSlot(Entity player, int slot, string Perk)
        {
            if (!player.IsAlive) return;
            HudElem perkIcon = player.GetField<HudElem>("hud_perk" + slot);
            perkIcon.SetShader(Perk, 30, 30);
            perkIcon.Alpha = 1;
        }

        public static IEnumerator showBoughtPerk(Entity player, string Name, string ImageName, int index)
        {
            HudElem Desc = HudElem.CreateFontString(player, HudElem.Fonts.HudSmall, 1.5f);
            Desc.SetText(PerkDescs[index]);
            Desc.SetField("text", PerkDescs[index]);
            Desc.SetPoint("CENTER", "CENTER", 0, -100);
            Desc.Color = new Vector3(0.99f, 1, 0.8f);
            Desc.HideWhenInMenu = true;
            Desc.Archived = true;
            Desc.Alpha = 0;
            HudElem PerkName = HudElem.CreateFontString(player, HudElem.Fonts.HudSmall, 1.7f);
            PerkName.SetText(Name);
            PerkName.SetField("text", Name);
            PerkName.SetPoint("CENTER", "CENTER", 0, -170);
            PerkName.Color = new Vector3(0.99f, 1, 0.8f);
            PerkName.HideWhenInMenu = true;
            PerkName.Archived = true;
            PerkName.Alpha = 0;
            HudElem Image = NewClientHudElem(player);
            Image.SetShader(ImageName, 50, 50);
            Image.X = 0;
            Image.Y = -130;
            Image.AlignX = HudElem.XAlignments.Center;
            Image.AlignY = HudElem.YAlignments.Middle;
            Image.HorzAlign = HudElem.HorzAlignments.Center;
            Image.VertAlign = HudElem.VertAlignments.Middle;
            Image.HideWhenInMenu = true;
            Image.Archived = true;
            Image.Alpha = 0;

            HudElem[] huds = new HudElem[2] { Desc, PerkName};
            player.SetField("hud_earnPerk", new Parameter(huds));

            int ImageX;
            if (index == 6) ImageX = getPerkPath(player, true);
            else ImageX = getPerkPath(player, false);

            yield return Wait(1);

            Desc.FadeOverTime(0.6f);
            PerkName.FadeOverTime(0.6f);
            Image.FadeOverTime(0.6f);
            Desc.Alpha = 1;
            PerkName.Alpha = 1;
            Image.Alpha = 1;

            yield return Wait(5);

            Desc.FadeOverTime(0.6f);
            Desc.Alpha = 0;
            PerkName.FadeOverTime(0.6f);
            PerkName.Alpha = 0;
            if (index == 6) Image.ScaleOverTime(3, 40, 40);
            else Image.ScaleOverTime(3, 30, 30);
            Image.MoveOverTime(2.9f);
            Image.X = ImageX;
            if (index == 6) Image.Y = 211;
            else Image.Y = 170;
            yield return Wait(.6f);
            Desc.Destroy();
            PerkName.Destroy();
            player.ClearField("hud_earnPerk");

            yield return Wait(2.30f);

            Image.Destroy();
        }

        public static IEnumerator roundStartHud()
        {
            roundStart = HudElem.CreateServerFontString(HudElem.Fonts.HudBig, 1.5f);
            roundStart.SetPoint("TOPCENTER", "TOPCENTER", 0, 5);
            roundStart.GlowAlpha = 0.7f;
            roundStart.GlowColor = new Vector3(0, 0, 1);
            roundStart.Alpha = 0;
            roundStart.Archived = true;
            roundStart.SetText(AIZ.gameStrings[188] + roundSystem.Wave);
            roundStart.FadeOverTime(1);
            roundStart.Alpha = 1;

            yield return Wait(5);

            roundStart.FadeOverTime(1);
            roundStart.Alpha = 0;

            yield return Wait(1);

            roundStart.Destroy();
            roundStart = null;
        }

        public static IEnumerator roundEndHud()
        {
            roundEnd = HudElem.CreateServerFontString(HudElem.Fonts.HudBig, 1.5f);
            roundEnd.SetPoint("TOPCENTER", "TOPCENTER", 0, 5);
            roundEnd.GlowAlpha = 0.7f;
            roundEnd.GlowColor = new Vector3(0, 0, 1);
            roundEnd.Archived = false;
            roundEnd.Alpha = 0;
            if (roundSystem.isBossWave)
            {
                roundEnd.SetText(AIZ.gameStrings[184]);
                foreach (Entity players in Players)
                {
                    if (!players.IsAlive) continue;
                    players.SetField("cash", players.GetField<int>("cash") + 1000);
                    scorePopup(players, 1000);
                    scoreMessage(players, AIZ.gameStrings[185]);
                    AIZ.giveMaxAmmo(players);
                }
            }
            else if (roundSystem.isCrawlerWave)
            {
                roundEnd.SetText(AIZ.gameStrings[186]);
                foreach (Entity players in Players)
                {
                    if (!players.IsAlive) continue;
                    AIZ.giveMaxAmmo(players);
                    scoreMessage(players, AIZ.gameStrings[187]);
                }
            }
            else roundEnd.SetText(AIZ.gameStrings[188] + roundSystem.Wave + AIZ.gameStrings[189]);
            roundEnd.FadeOverTime(1);
            roundEnd.Alpha = 1;

            yield return Wait(5);

            roundEnd.FadeOverTime(1);
            roundEnd.Alpha = 0;

            yield return Wait(1);

            roundEnd.Destroy();
            roundEnd = null;
        }

        public static IEnumerator powerBoughtHud(Entity buyer)
        {
            if (AIZ.isHellMap) yield break;
            //HudElem power = player.GetField<HudElem>("hud_power");
            HudElem powerMessage = HudElem.CreateServerFontString(HudElem.Fonts.HudSmall, 1.5f);
            powerMessage.SetPoint("CENTER", "CENTER", 1000, -150);
            powerMessage.HideWhenInMenu = true;
            powerMessage.Foreground = true;
            powerMessage.Archived = false;
            powerMessage.Color = new Vector3(0, 0.85f, 0.9f);
            powerMessage.SetText(AIZ.gameStrings[190]);
            HudElem powerName = HudElem.CreateServerFontString(HudElem.Fonts.HudSmall, 1.5f);
            powerName.SetPoint("CENTER", "CENTER", -700, -130);
            powerName.HideWhenInMenu = true;
            powerName.Foreground = true;
            powerName.Archived = false;
            powerName.Color = new Vector3(0, 0.85f, 0.9f);
            powerName.SetPlayerNameString(buyer);
            powerMessage.SetPoint("CENTER", "CENTER", 0, -150, 3);
            powerName.SetPoint("CENTER", "CENTER", 0, -130, 3);

            yield return Wait(7);

            powerMessage.FadeOverTime(1);
            powerMessage.Alpha = 0;
            powerName.FadeOverTime(1);
            powerName.Alpha = 0;

            yield return Wait(1);

            powerMessage.Destroy();
            powerName.Destroy();
        }
        public static void tempPowerHud()
        {
            if (powerBox) return;
            //HudElem power = player.GetField<HudElem>("hud_power");
            powerHud.Color = new Vector3(0.9f, 0.9f, 0);
            powerHud.SetText(string.Format(AIZ.gameStrings[191], EMPTime));
            if (!AIZ.tempPowerActivated)
                OnInterval(1000, runTempPowerTimer);
        }
        private static bool runTempPowerTimer()
        {
            if (AIZ.gameEnded) return false;
            if (powerBox) return false;
            EMPTime--;
            powerHud.SetText(string.Format(AIZ.gameStrings[191], EMPTime));
            if (EMPTime == 0 && !powerBox)
            {
                AIZ.powerActivated = false;
                AIZ.tempPowerActivated = false;
                powerHud.Color = new Vector3(0.9f, 0, 0);
                powerHud.SetText(AIZ.gameStrings[183]);
                return false;
            }
            else return true;
        }

        public static IEnumerator endGame(bool win)
        {
            if (AIZ.gameEnded) yield break;
            AIZ.gameEnded = true;
            //restore gameInfoString
            AIZ.restoreGameInfo();
            MakeDvarServerInfo("scr_gameended", "1");
            //Currently we hack in the losing code and piggy back off the win code

            yield return Wait(1);

            SetTeamRadar("allies", false);
            Entity bestPlayer = AIZ.getPlayerWithMostKills();
            Entity globalCam = Entity.Level;
            //if (killHud) 
            if (!win)
            {
                globalCam = Spawn("script_model", bestPlayer.Origin + new Vector3(0, 100, 100));
                globalCam.SetModel("tag_origin");
                Vector3 angles = VectorToAngles(bestPlayer.Origin - globalCam.Origin);
                globalCam.Angles = angles;
                globalCam.NotSolid();
                globalCam.EnableLinkTo();
                globalCam.MoveTo(globalCam.Origin + new Vector3(0, 0, 2000), 25);
                //bestPlayer.Notify("game_ended", "axis");
                Notify("game_win", "axis");
            }
            else
            {
                //bestPlayer.Notify("game_ended", "allies");
                Notify("game_win", "allies");
            }
            Notify("game_over");
            Notify("game_ended");
            Notify("block_notifies");

            if (!AIZ.isHellMap) powerHud.Alpha = 0;
            zombieCounter.Alpha = 0;
            round.Alpha = 0;

            foreach (Entity player in Players)
            {
                if (!AIZ.isPlayer(player)) continue;

                if (player.GetField<bool>("isDown"))
                    AIZ.autoRevive_revivePlayer(player, null);

                player.SetField("isDown", true);

                if (player.HasField("bot")) StartAsync(killstreaks.killPlayerBotOnDeath(player));

                player.SessionTeam = "allies";
                player.SessionState = "spectating";
                player.SetClientDvar("g_scriptMainMenu", "");

                //if (win) player.PlayLocalSound("victory_music");

                player.FreezeControls(true);
                player.NotSolid();
                destroyPlayerHud(player);
                Vector3 camPos = player.GetEye();
                Entity cam = Spawn("script_model", camPos);
                if (win)
                {
                    cam.SetModel("tag_origin");
                    Vector3 moveToAngles = VectorToAngles(bestPlayer.Origin - (bestPlayer.Origin + new Vector3(200, 200, 200)));
                    Vector3 angle = player.GetPlayerAngles();
                    cam.Angles = angle;
                    cam.NotSolid();
                    cam.EnableLinkTo();
                    cam.MoveTo(bestPlayer.Origin + new Vector3(200, 200, 200), 5, 1, 1);
                    //player.SetPlayerAngles(moveToLoc);
                    cam.RotateTo(moveToAngles, 5, 1, 1);
                    AfterDelay(5000, () =>
                            cam.MoveTo(cam.Origin + new Vector3(0, 0, 1900), 25, 1, 1));
                    if (player != bestPlayer)
                        player.PlayerHide();
                    else StartAsync(endGame_createTopPlayerClone(player));

                    AfterDelay(50, () => player.TakeAllWeapons());
                    player.PlayerLinkToAbsolute(cam);//player.CameraLinkTo(cam, "tag_origin");
                }
                else
                {
                    //player.PlayLocalSound("mp_killstreak_carepackage");
                    //player.PlayLocalSound("defeat_music");
                    player.SetPlayerAngles(globalCam.Angles);
                    player.CameraLinkTo(globalCam, "tag_origin", Vector3.Zero, Vector3.Zero);
                }
                player.PlayerLinkedSetViewZNear(false);

                StartAsync(doOutro(player, win));
            }

            yield return Wait(5);

            foreach (Entity player in Players)
            {
                if (!AIZ.isPlayer(player)) continue;
                player.PlayLocalSound("nuke_explosion");
                player.PlayLocalSound("nuke_wave");
                StartAsync(endGameVision(player));
            }

            if (!win)
                StartAsync(endGame_hideAllBots());

            yield return Wait(2);
            HudElem[] endGameScreen;
            if (win)
            {
                SetWinningTeam("allies");
                SetMatchData("victor", "allies");

                int randomMessage = AIZ.rng.Next(AIZ.zombieDeath.Length);
                endGameScreen = createEndGameScreen(win, AIZ.zombieDeath[randomMessage]);
            }
            else
            {
                SetWinningTeam("axis");
                SetMatchData("victor", "axis");
                endGameScreen = createEndGameScreen(win, AIZ.gameStrings[193]);
            }

            yield return Wait(18);
            //SetGameEndTime(0);
            if (AIZ.voting)
            {
                if (endGameScreen != null)
                {
                    foreach (HudElem h in endGameScreen)
                        h.Destroy();
                }

                initVoting();
            }
            else
            {
                int maxMapsCount = 35;
                if (!AIZ.dlcEnabled) maxMapsCount = 15;

                Utilities.ExecuteCommand("map " + mapList[AIZ.rng.Next(0, maxMapsCount)]);
            }
        }

        private static IEnumerator endGame_hideAllBots()
        {
            yield return Wait(2);

            foreach (Entity bot in botUtil.botsInPlay)
            {
                bot.HideAllParts();
                if (bot.HasField("head")) bot.GetField<Entity>("head").Hide();
            }
        }

        private static IEnumerator endGame_createTopPlayerClone(Entity player)
        {
            yield return WaitForFrame();

            Entity clone = player.ClonePlayer(6);
            Entity head = Spawn("script_model", clone.Origin);
            string headModel = player.GetAttachModelName(0);
            head.SetModel(headModel);
            head.LinkTo(clone, "j_spine4", Vector3.Zero, Vector3.Zero);
            player.PlayerHide();
        }

        private static IEnumerator endGameVision(Entity player)
        {
            player.VisionSetNakedForPlayer("end_game2", 2.5f);
            yield return Wait(3);
            player.VisionSetNakedForPlayer("mpnuke_aftermath");
        }

        private static IEnumerator doOutro(Entity player, bool win)
        {
            HudElem[] endGameText = createEndGameSequenceForPlayer(player, win);
            endGameText[0].Alpha = 1;
            player.PlayLocalSound("weap_barrett_fire_aki");
            Earthquake(.25f, .3f, player.Origin, 5000);
            yield return Wait(.9f);
            endGameText[1].Alpha = 1;
            player.PlayLocalSound("weap_barrett_fire_aki");
            Earthquake(.25f, .3f, player.Origin, 5000);
            yield return Wait(.9f);
            endGameText[2].Alpha = 1;
            player.PlayLocalSound("weap_barrett_fire_aki");
            Earthquake(.25f, .3f, player.Origin, 5000);
            yield return Wait(.9f);
            endGameText[3].Alpha = 1;
            player.PlayLocalSound("weap_barrett_fire_aki");
            Earthquake(.25f, .3f, player.Origin, 5000);
            yield return Wait(.9f);
            endGameText[4].Alpha = 1;
            player.PlayLocalSound("weap_barrett_fire_aki");
            Earthquake(.25f, .3f, player.Origin, 5000);

            yield return Wait(3);

            foreach (HudElem h in endGameText)
            {
                h.FadeOverTime(.8f);
                h.Alpha = 0;
                AfterDelay(800, () => h.Destroy());
            }
        }

        private static HudElem[] createEndGameScreen(bool win, string endText)
        {
            HudElem outcomeTitle = HudElem.CreateServerFontString(HudElem.Fonts.HudBig, 1.5f);
            outcomeTitle.SetPoint("CENTER", "", 0, -134);
            outcomeTitle.Foreground = true;
            outcomeTitle.GlowAlpha = 1;
            outcomeTitle.HideWhenInMenu = false;
            outcomeTitle.Archived = false;

            HudElem outcomeText = HudElem.CreateServerFontString(HudElem.Fonts.HudBig, 1);
            outcomeText.Parent = outcomeTitle;
            outcomeText.Foreground = true;
            outcomeText.SetPoint("TOP", "BOTTOM", 0, 18);
            outcomeText.GlowAlpha = 1;
            outcomeText.HideWhenInMenu = false;
            outcomeText.Archived = false;

            outcomeTitle.GlowColor = new Vector3(0, 0, 0);
            if (win)
            {
                outcomeTitle.SetText(AIZ.gameStrings[194]);
                outcomeTitle.Color = new Vector3(.3f, .7f, .2f);
            }
            else
            {
                outcomeTitle.SetText(AIZ.gameStrings[195]);
                outcomeTitle.Color = new Vector3(.7f, .3f, .2f);
            }
            outcomeText.GlowColor = new Vector3(.2f, .3f, .7f);
            outcomeText.SetText(endText);
            outcomeTitle.SetPulseFX(100, 60000, 1000);
            outcomeText.SetPulseFX(100, 60000, 1000);

            HudElem leftIcon = NewHudElem();
            leftIcon.SetShader("iw5_cardicon_soap", 70, 70);
            leftIcon.Parent = outcomeText;
            leftIcon.SetPoint("TOP", "BOTTOM", -60, 45);
            //leftIcon.SetShader("cardicon_soap", 70, 70);
            leftIcon.Foreground = true;
            leftIcon.HideWhenInMenu = false;
            leftIcon.Archived = false;
            leftIcon.Alpha = 0;
            leftIcon.FadeOverTime(.5f);
            leftIcon.Alpha = 1;

            HudElem rightIcon = NewHudElem();
            rightIcon.SetShader("iw5_cardicon_nuke", 70, 70);
            rightIcon.Parent = outcomeText;
            rightIcon.SetPoint("TOP", "BOTTOM", 60, 45);
            //rightIcon.SetShader("cardicon_nuke", 70, 70);
            rightIcon.Foreground = true;
            rightIcon.HideWhenInMenu = false;
            rightIcon.Archived = false;
            rightIcon.Alpha = 0;
            rightIcon.FadeOverTime(.5f);
            rightIcon.Alpha = 1;

            HudElem leftScore = HudElem.CreateServerFontString(HudElem.Fonts.HudBig, 1.25f);
            leftScore.Parent = leftIcon;
            leftScore.SetPoint("TOP", "BOTTOM", 0, 0);
            if (win)
            {
                leftScore.GlowColor = new Vector3(.2f, .8f, .2f);
                leftScore.SetText(AIZ.gameStrings[196]);
            }
            else
            {
                leftScore.GlowColor = new Vector3(.8f, .2f, .2f);
                leftScore.SetText(AIZ.gameStrings[197]);
            }
            leftScore.GlowAlpha = 1;
            leftScore.Foreground = true;
            leftScore.HideWhenInMenu = false;
            leftScore.Archived = false;
            leftScore.SetPulseFX(100, 60000, 1000);

            HudElem rightScore = HudElem.CreateServerFontString(HudElem.Fonts.HudBig, 1.25f);
            rightScore.Parent = rightIcon;
            rightScore.SetPoint("TOP", "BOTTOM", 0);
            rightScore.GlowAlpha = 1;
            if (!win)
            {
                rightScore.GlowColor = new Vector3(.2f, .8f, .2f);
                rightScore.SetText(AIZ.gameStrings[196]);
            }
            else
            {
                rightScore.GlowColor = new Vector3(.8f, .2f, .2f);
                rightScore.SetText(AIZ.gameStrings[197]);
            }
            rightScore.Foreground = true;
            rightScore.HideWhenInMenu = false;
            rightScore.Archived = false;
            rightScore.SetPulseFX(100, 60000, 1000);

            return new HudElem[] {outcomeTitle, outcomeText, rightScore, leftScore, rightIcon, leftIcon };
        }

        private static HudElem[] createEndGameSequenceForPlayer(Entity player, bool win)
        {
            HudElem[] endGameText = new HudElem[5];
            endGameText[0] = HudElem.CreateFontString(player, HudElem.Fonts.HudBig, .85f);
            endGameText[0].SetPoint("TOPMIDDLE", "TOPMIDDLE", 0, 60);
            endGameText[0].Color = new Vector3(1, 0, 0);
            endGameText[0].SetText(AIZ.gameStrings[198]);
            endGameText[0].Alpha = 0;
            endGameText[1] = HudElem.CreateFontString(player, HudElem.Fonts.HudBig, .85f);
            endGameText[1].SetPoint("TOPMIDDLE", "TOPMIDDLE", 0, 75);
            endGameText[1].Color = new Vector3(1, .5f, .3f);
            endGameText[1].SetText(AIZ.timePlayedMinutes.ToString() + AIZ.gameStrings[199]);
            endGameText[1].Alpha = 0;
            endGameText[2] = HudElem.CreateFontString(player, HudElem.Fonts.HudBig, .85f);
            endGameText[2].SetPoint("TOPMIDDLE", "TOPMIDDLE", 0, 90);
            endGameText[2].Color = new Vector3(1, 1, 0);
            endGameText[2].SetText(AIZ.timePlayed.ToString() + AIZ.gameStrings[200]);
            endGameText[2].Alpha = 0;
            endGameText[3] = HudElem.CreateFontString(player, HudElem.Fonts.HudBig, .85f);
            endGameText[3].SetPoint("TOPMIDDLE", "TOPMIDDLE", 0, 105);
            endGameText[3].Color = new Vector3(1, 0, 0);
            if (win) endGameText[3].SetText(AIZ.gameStrings[201] + roundSystem.totalWaves.ToString());
            else endGameText[3].SetText(AIZ.gameStrings[201] + roundSystem.Wave.ToString());
            endGameText[3].Alpha = 0;
            endGameText[4] = HudElem.CreateFontString(player, HudElem.Fonts.HudBig, .85f);
            endGameText[4].SetPoint("TOPMIDDLE", "TOPMIDDLE", 0, 120);
            endGameText[4].Color = new Vector3(1, .5f, .3f);
            if (win) endGameText[4].SetText(AIZ.gameStrings[202]);
            else endGameText[4].SetText(AIZ.gameStrings[203]);
            endGameText[4].Alpha = 0;
            return endGameText;
        }

        public static void showPowerUpHud(string type, Entity player)
        {
            if (type == "gun")
            {
                HudElem personalInfo = HudElem.CreateFontString(player, HudElem.Fonts.HudSmall, 1);
                personalInfo.SetPoint("Center", "center");
                personalInfo.SetText(AIZ.gameStrings[204]);
                personalInfo.HideWhenInMenu = true;
                personalInfo.Alpha = 1;
                personalInfo.FadeOverTime(2);
                personalInfo.Alpha = 0;
                personalInfo.ChangeFontScaleOverTime(2);
                personalInfo.FontScale = .8f;
                AfterDelay(2000, () => personalInfo.Destroy());

                if (player.GetField<bool>("deathHud")) return;
                player.SetField("deathHud", true);
                HudElem icon = NewClientHudElem(player);//HudElem.CreateServerFontString(HudElem.Fonts.HudBig, 1.5f);
                icon.Archived = false;
                icon.Foreground = true;
                icon.HideWhenInMenu = true;
                icon.HorzAlign = HudElem.HorzAlignments.Center_Adjustable;
                icon.VertAlign = HudElem.VertAlignments.Bottom;
                icon.AlignX = HudElem.XAlignments.Center;
                icon.AlignY = HudElem.YAlignments.Middle;
                icon.X = 75;
                icon.Y = 0;
                icon.Alpha = 1;
                icon.SetShader("specialty_bulletpenetration", 48, 48);
                AfterDelay(22000, () => startDeathMachineHudFlash(icon, player));
                return;
            }

            HudElem info = createPowerupHintText();
            //info.SetPoint("center", "center", 1, -50, 2);
            info.ChangeFontScaleOverTime(2);
            info.FontScale = .8f;
            AfterDelay(2000, () => info.Destroy());

            if (type == "instakill")
            {
                info.SetText(AIZ.gameStrings[205]);
                if (killHud) return;
                powerup1 = createPowerupHud(0);
                powerup1.SetShader("cardicon_skull_black", 48, 48);
                killHud = true;
                OnInterval(1000, watchInstakillFlash);
            }
            else if (type == "2points")
            {
                info.SetText(AIZ.gameStrings[206]);
                if (doubleHud) return;
                powerup2 = createPowerupHud(1);
                powerup2.SetShader("specialty_bling", 48, 48);
                doubleHud = true;
                OnInterval(1000, watchDoubleFlash);
            }
            else if (type == "ammo")
                info.SetText(AIZ.gameStrings[207]);
            else if (type == "nuke")
                info.SetText(AIZ.gameStrings[208]);
            else if (type == "sale")
                info.SetText(AIZ.gameStrings[209]);
            else if (type == "freeze")
                info.SetText(AIZ.gameStrings[210]);
        }
        private static void startDeathMachineHudFlash(HudElem icon, Entity player)
        {
            OnInterval(1000, () => deathMachineHudFlash(icon, player));
            AfterDelay(8000, () => player.SetField("deathHud", false));
        }
        private static bool deathMachineHudFlash(HudElem icon, Entity player)
        {
            if (AIZ.gameEnded) return false;
            if (!player.GetField<bool>("deathHud"))
            {
                icon.Destroy();
                return false;
            }

            icon.FadeOverTime(.5f);
            icon.Alpha = 1;
            StartAsync(fadeOutIcon(icon, .5f, .5f));

            return true;
        }
        private static bool watchInstakillFlash()
        {
            if (AIZ.gameEnded)
            {
                powerup1.Destroy();
                return false;
            }
            if (botUtil.instaKillTime < 10)
            {
                if (botUtil.instaKillTime == 0)
                {
                    powerup1.Destroy();
                    powerup1 = null;
                    killHud = false;
                    return false;
                }

                powerup1.FadeOverTime(.5f);
                powerup1.Alpha = 1;
                StartAsync(fadeOutIcon(powerup1, .5f, .5f));
                return true;
            }
            else { powerup1.Alpha = 1; return true; }
        }
        private static bool watchDoubleFlash()
        {
            if (AIZ.gameEnded)
            {
                powerup2.Destroy();
                return false;
            }
            if (botUtil.doublePointsTime < 10)
            {
                if (botUtil.doublePointsTime == 0)
                {
                    powerup2.Destroy();
                    powerup2 = null;
                    doubleHud = false;
                    return false;
                }

                powerup2.FadeOverTime(.5f);
                powerup2.Alpha = 1;
                StartAsync(fadeOutIcon(powerup2, .5f, .5f));

                return true;
            }
            else { powerup2.Alpha = 1; return true; }
        }

        private static void initVoting()
        {
            //Huds draw from left to right
            HudElem[] currentMapDescs = new HudElem[3];
            HudElem[] currentMapVotes = new HudElem[3];
            HudElem controls;
            HudElem timer;
            HudElem title;

            for (int i = 0; i < 3; i++)
            {
                currentMapDescs[i] = HudElem.CreateServerFontString(HudElem.Fonts.HudBig, .75f);
                currentMapDescs[i].Alpha = 1;
                currentMapDescs[i].HideWhenDead = false;
                currentMapDescs[i].HideWhenInDemo = true;
                currentMapDescs[i].HideWhenInMenu = false;
                currentMapDescs[i].Sort = i;

                currentMapVotes[i] = HudElem.CreateServerFontString(HudElem.Fonts.Objective, 1);
                currentMapVotes[i].Parent = currentMapDescs[i];
                currentMapVotes[i].AlignX = HudElem.XAlignments.Center;
                currentMapVotes[i].AlignY = HudElem.YAlignments.Bottom;
                currentMapVotes[i].Color = new Vector3(1, 1, 0);
                currentMapVotes[i].Alpha = 1;
                currentMapVotes[i].GlowAlpha = .5f;
                currentMapVotes[i].GlowColor = new Vector3(0, 1, 0);
                currentMapVotes[i].HideWhenDead = false;
                currentMapVotes[i].HideWhenInDemo = true;
                currentMapVotes[i].HideWhenInMenu = false;
                currentMapVotes[i].HorzAlign = HudElem.HorzAlignments.Center_Adjustable;
                currentMapVotes[i].VertAlign = HudElem.VertAlignments.Center_Adjustable;
                currentMapVotes[i].Sort = i;
                currentMapVotes[i].SetPoint("bottom", "bottom", 0, 60);
                currentMapVotes[i].SetValue(0);
            }
            currentMapDescs[0].SetPoint("center", "center", -200, 0);
            currentMapDescs[1].SetPoint("center", "center", 200, 0);
            currentMapDescs[2].SetPoint("center", "center", 0, 150);
            currentMapVotes[2].SetPoint("bottom", "bottom", 0, 30);
            currentMapDescs[2].FontScale = 1;
            currentMapDescs[2].SetText(AIZ.gameStrings[211]);

            controls = HudElem.CreateServerFontString(HudElem.Fonts.Bold, 1.4f);
            controls.AlignX = HudElem.XAlignments.Center;
            controls.AlignY = HudElem.YAlignments.Bottom;
            controls.Alpha = 1;
            //controls.Color = new Vector3(0, 0, 1);
            controls.HideWhenDead = false;
            controls.HideWhenInDemo = true;
            controls.HideWhenInMenu = true;
            controls.HorzAlign = HudElem.HorzAlignments.Center_Adjustable;
            controls.Sort = 4;
            controls.VertAlign = HudElem.VertAlignments.Bottom_Adjustable;
            controls.SetText(AIZ.gameStrings[212]);

            timer = HudElem.CreateServerFontString(HudElem.Fonts.Objective, 1f);
            timer.AlignX = HudElem.XAlignments.Center;
            timer.AlignY = HudElem.YAlignments.Top;
            timer.Alpha = 1;
            timer.HideWhenDead = false;
            timer.HideWhenInDemo = true;
            timer.HideWhenInMenu = false;
            timer.HorzAlign = HudElem.HorzAlignments.Center_Adjustable;
            timer.Sort = 4;
            timer.VertAlign = HudElem.VertAlignments.Top_Adjustable;
            timer.SetTimer(20);

            title = HudElem.CreateServerFontString(HudElem.Fonts.HudBig, 1.2f);
            title.Alpha = 1;
            title.Color = new Vector3(0, 1, 0);
            title.GlowColor = new Vector3(0, 1, 0);
            title.GlowAlpha = .8f;
            title.HideWhenDead = false;
            title.HideWhenInDemo = true;
            title.HideWhenInMenu = false;
            title.SetPoint("center", "top", 0, 100);
            title.SetText(AIZ.gameStrings[213]);

            //Determine maps
            int maxMapsCount = 36;
            if (!AIZ.dlcEnabled) maxMapsCount = 17;

            mapLists[0] = (byte)AIZ.rng.Next(1, maxMapsCount);

            mapLists[1] = (byte)AIZ.rng.Next(1, maxMapsCount);
            if (mapLists[1] == mapLists[0])//If we rolled the same map then retry with in-game randoms
                mapLists[1] = (byte)RandomIntRange(1, maxMapsCount);

            mapLists[2] = (byte)RandomIntRange(1, maxMapsCount);
            //End determining

            for (int i = 0; i < 2; i++)
            {
                currentMapDescs[i].SetText("^5" + mapNames[mapLists[i]] + "\n^7" + mapDesc[mapLists[i]]);
            }

            //Init player commands
            foreach (Entity players in Players)
            {
                if (!players.HasField("isDown")) continue;//Joining players cannot vote

                players.VisionSetNakedForPlayer("black_bw", .8f);

                //Actionslot 4, 5, and 6 for votes 1, 2, and 3
                players.NotifyOnPlayerCommand("vote1", "+actionslot 4");
                players.NotifyOnPlayerCommand("vote2", "+actionslot 5");
                players.NotifyOnPlayerCommand("vote3", "+actionslot 6");

                players.SetField("currentMapVote", -1);

                players.OnNotify("vote1", (player) =>
                {
                    if (votingFinished) return;
                    if (player.GetField<int>("currentMapVote") == 0)
                        return;
                    else if (player.GetField<int>("currentMapVote") == 1)
                        mapVotes[1]--;
                    else if (player.GetField<int>("currentMapVote") == 2)
                        mapVotes[2]--;

                    player.SetField("currentMapVote", 0);
                    mapVotes[0]++;
                    for (int i = 0; i < 3; i++)
                    {
                        currentMapVotes[i].SetValue(mapVotes[i]);
                    }
                });
                players.OnNotify("vote2", (player) =>
                {
                    if (votingFinished) return;
                    if (player.GetField<int>("currentMapVote") == 1)
                        return;
                    else if (player.GetField<int>("currentMapVote") == 0)
                        mapVotes[0]--;
                    else if (player.GetField<int>("currentMapVote") == 2)
                        mapVotes[2]--;

                    player.SetField("currentMapVote", 1);
                    mapVotes[1]++;
                    for (int i = 0; i < 3; i++)
                    {
                        currentMapVotes[i].SetValue(mapVotes[i]);
                    }
                });
                players.OnNotify("vote3", (player) =>
                {
                    if (votingFinished) return;
                    if (player.GetField<int>("currentMapVote") == 2)
                        return;
                    else if (player.GetField<int>("currentMapVote") == 1)
                        mapVotes[1]--;
                    else if (player.GetField<int>("currentMapVote") == 0)
                        mapVotes[0]--;

                    player.SetField("currentMapVote", 2);
                    mapVotes[2]++;
                    for (int i = 0; i < 3; i++)
                    {
                        currentMapVotes[i].SetValue(mapVotes[i]);
                    }
                });
            }

            StartAsync(watchVotesTimer(currentMapDescs, currentMapVotes, timer, title));
        }

        private static IEnumerator watchVotesTimer(HudElem[] currentMapDescs, HudElem[] currentMapVotes, HudElem timer, HudElem title)
        {
            yield return Wait(20);

            timer.FadeOverTime(1);
            timer.Alpha = 0;
            title.FadeOverTime(1);
            title.Alpha = 0;

            votingFinished = true;

            //Determine winner
            int winner;

            if (mapVotes[0] > mapVotes[1] && mapVotes[0] >= mapVotes[2]) winner = 0;
            else if (mapVotes[1] > mapVotes[0] && mapVotes[1] >= mapVotes[2]) winner = 1;
            else winner = 2;

            for (int i = 0; i < 3; i++)
            {
                if (i == winner) continue;
                currentMapDescs[i].FadeOverTime(1);
                currentMapDescs[i].Alpha = 0;
                currentMapVotes[i].FadeOverTime(1);
                currentMapVotes[i].Alpha = 0;
            }

            currentMapVotes[winner].Destroy();
            //currentMapDescs[winner].MoveOverTime(2);
            currentMapDescs[winner].SetPoint("center", "center", 0, 0, 2);
            currentMapDescs[winner].ChangeFontScaleOverTime(2);
            currentMapDescs[winner].FontScale = 1.25f;

            yield return Wait(7);

            Utilities.ExecuteCommand("map " + mapList[mapLists[winner]].ToString());
        }

        #region moon
        /*
        public static void moon_doWarningMessage(Entity player, string text)
        {
            HudElem info = HudElem.CreateFontString(player, HudElem.Fonts.Bold, 3f);
            info.SetPoint("LEFT", "CENTER", 800, -170);
            info.Archived = false;
            info.Foreground = true;
            info.HideWhenInMenu = true;
            //info.HorzAlign = HudElem.HorzAlignments.Center_Adjustable;
            //info.VertAlign = HudElem.VertAlignments.Top;
            //info.AlignX = HudElem.XAlignments.Left;
            //info.AlignY = HudElem.YAlignments.Middle;
            //info.X = 0;
            //info.Y = 25;
            //info.Font = HudElem.Fonts.Bold;
            //info.FontScale = 3f;
            info.Color = new Vector3(1, 0, 0);
            info.SetText(text);
            info.Alpha = 1;
            int duration = text.Length * 200;
            Utilities.PrintToConsole("Duration = " + duration.ToString());
            info.SetPulseFX(125, duration, 200);
            //info.SetPoint("LEFT", "RIGHT", -1000, -170, (text.Length / 2));
            info.MoveOverTime((text.Length / 2));
            info.X -= 4000;
            AfterDelay(duration + 1000, () => info.Destroy());
        }
        public static IEnumerator moon_doInfoMessage(Entity player, params string[] text)
        {
            if (player != null && AIZ.isPlayer(player)) player.IPrintLnBold("^5" + text[0]);
            else IPrintLnBold("^5" + text[0]);
            if (text.Length < 2) yield return null;
            int time = 3;//text[1].Length * 100;
            for (int i = 1; i < text.Length; i++)
            {
                yield return Wait(time);
                if (player != null && AIZ.isPlayer(player)) player.IPrintLnBold("^5" + text[i]);
                else IPrintLnBold("^5" + text[i]);
            }
        }
        public static IEnumerator moon_doInfoMessage(params string[] text)
        {
            IPrintLnBold("^5" + text[0]);
            if (text.Length < 2) yield return null;
            int time = 3;//text[1].Length * 100;
            for (int i = 1; i < text.Length; i++)
            {
                yield return Wait(time);
                IPrintLnBold("^5" + text[i]);
            }
        }
        */
        #endregion

        private static HudElem createPowerupHud(int slot)
        {
            HudElem powerup = NewHudElem();//HudElem.CreateServerFontString(HudElem.Fonts.HudBig, 1.5f);
            int x = 0;
            if (slot == 1) x = -75;
            powerup.Archived = false;
            powerup.Foreground = true;
            powerup.HideWhenInMenu = true;
            powerup.HorzAlign = HudElem.HorzAlignments.Center_Adjustable;
            powerup.VertAlign = HudElem.VertAlignments.Bottom;
            powerup.AlignX = HudElem.XAlignments.Center;
            powerup.AlignY = HudElem.YAlignments.Middle;
            powerup.X = x;
            powerup.Y = 0;
            powerup.Alpha = 1;
            return powerup;
        }

        private static HudElem createPowerupHintText()
        {
            HudElem info = NewHudElem();
            info.Font = HudElem.Fonts.HudSmall;
            info.FontScale = 1;
            info.X = 0;
            info.Y = 0;
            info.VertAlign = HudElem.VertAlignments.Middle;
            info.HorzAlign = HudElem.HorzAlignments.Center_Adjustable;
            info.HideIn3rdPerson = false;
            info.HideWhenInMenu = true;
            info.Foreground = true;
            info.Archived = false;
            info.AlignY = HudElem.YAlignments.Middle;
            info.AlignX = HudElem.XAlignments.Center;
            info.Alpha = 1;
            info.FadeOverTime(2);
            info.Alpha = 0;
            return info;
        }

        public static string createHudShaderString(string shader, bool flipped = false, int width = 64, int height = 64)
        {
            byte[] str;
            byte flip;
            flip = (byte)(flipped ? 2 : 1);
            byte w = (byte)width;
            byte h = (byte)height;
            byte length = (byte)shader.Length;
            str = new byte[4] { flip, w, h, length };
            string ret = "^" + Encoding.UTF8.GetString(str);
            return ret + shader;
        }

        public static int getPerkPath(Entity player, bool Revive)
        {
            if (Revive)
                return 400;

            bool[] perkSlots = getPerkHudSlotsOpen(player);

            if (!perkSlots[0])
                return -410;
            else if (!perkSlots[1])
                return -378;
            else if (!perkSlots[2])
                return -346;
            else if (!perkSlots[3])
                return -314;
            else if (!perkSlots[4])
                return -282;
            else if (!perkSlots[5])
                return -250;
            else if (!perkSlots[6])
                return -186;
            else return -186;//Error
        }

        private static bool[] getPerkHudSlotsOpen(Entity player)
        {
            bool[] ret = new bool[7] { false, false, false, false, false, false, false };
            for (int i = 1; i < 8; i++)
                ret[i-1] = player.GetField<bool>("perk" + i + "HudDone");
            return ret;
        }

        public static IEnumerator fadeOutIcon(HudElem icon, float fadeTime, float delay = 0)
        {
            yield return Wait(delay);

            icon.FadeOverTime(fadeTime);
            icon.Alpha = 0;
        }

        public static HudElem createIntermissionTimer()
        {
            if (intermission != null) { intermission.Destroy(); intermission = null; };
            intermission = NewTeamHudElem("allies");
            intermission.X = 5;
            intermission.Y = 120;
            intermission.AlignX = HudElem.XAlignments.Left;
            intermission.AlignY = HudElem.YAlignments.Top;
            intermission.HorzAlign = HudElem.HorzAlignments.Left_Adjustable;
            intermission.VertAlign = HudElem.VertAlignments.Top_Adjustable;
            intermission.Foreground = true;
            intermission.Alpha = 0;
            intermission.Archived = true;
            intermission.HideWhenInMenu = true;
            intermission.Color = new Vector3(0, .85f, .85f);
            intermission.GlowColor = new Vector3(0, .85f, .85f);
            intermission.GlowAlpha = .5f;
            intermission.Font = HudElem.Fonts.HudBig;
            intermission.FontScale = .7f;
            intermission.SetText(AIZ.gameStrings[21] + AIZ.intermissionTimerNum);
            return intermission;
        }

        public static HudElem createReviveHeadIcon(Entity player)
        {
            HudElem icon = NewTeamHudElem("allies");
            icon.SetShader("waypoint_revive", 8, 8);
            icon.Alpha = .85f;
            icon.SetWaypoint(true, true);
            icon.SetTargetEnt(player);
            return icon;
        }

        public static HudElem createReviveOverlayIcon(Entity player)
        {
            HudElem icon = NewClientHudElem(player);
            HudElem perk;
            if (player.HasField("hud_perk7")) perk = player.GetField<HudElem>("hud_perk7");
            else perk = round;//Fallback just in-case. Should never happen
            icon.X = perk.X;
            icon.Y = perk.Y;
            icon.AlignX = perk.AlignX;
            icon.AlignY = perk.AlignY;
            icon.VertAlign = perk.VertAlign;
            icon.HorzAlign = perk.HorzAlign;
            icon.SetShader("waypoint_revive", perk.Width, perk.Height);
            icon.HideWhenInMenu = true;
            icon.Foreground = true;
            icon.Alpha = 0;
            return icon;
        }

        public static HudElem createReviveOverlay(Entity player)
        {
            HudElem icon = NewClientHudElem(player);
            //icon.SetPoint("center", "middle");
            icon.X = 0;
            icon.Y = 0;
            icon.AlignX = HudElem.XAlignments.Left;
            icon.AlignY = HudElem.YAlignments.Top;
            icon.HorzAlign = HudElem.HorzAlignments.Fullscreen;
            icon.VertAlign = HudElem.VertAlignments.Fullscreen;
            icon.SetShader("combathigh_overlay", 640, 480);
            icon.Sort = -10;
            //icon.Archived = true;
            icon.HideWhenInMenu = false;
            icon.HideIn3rdPerson = true;
            icon.Foreground = true;
            icon.Alpha = 1;
            return icon;
        }

        public static HudElem createPrimaryProgressBar(Entity player, int xOffset, int yOffset)
        {
            HudElem progressBar = HudElem.CreateIcon(player, "progress_bar_fill", 0, 9);//NewClientHudElem(player);
            progressBar.SetField("frac", 0);
            progressBar.Color = new Vector3(1, 1, 1);
            progressBar.Sort = -2;
            progressBar.Shader = "progress_bar_fill";
            progressBar.SetShader("progress_bar_fill", 1, 9);
            progressBar.Alpha = 1;
            progressBar.SetPoint("center", "", 0, -61);
            progressBar.AlignX = HudElem.XAlignments.Left;
            progressBar.X = -60;

            HudElem progressBarBG = HudElem.CreateIcon(player, "progress_bar_bg", 124, 13);//NewClientHudElem(player);
            progressBarBG.SetPoint("center", "", 0, -61);
            progressBarBG.SetField("bar", progressBar);
            progressBarBG.Sort = -3;
            progressBarBG.Color = new Vector3(0, 0, 0);
            progressBarBG.Alpha = .5f;
            //progressBarBG.Parent = progressBar;
            //progressBarBG.SetShader("progress_bar_bg", 124, 13);
            //progressBarBG.SetField("hidden", false);

            return progressBarBG;
        }

        public static void updateBar(HudElem barBG, int barFrac, float rateOfChange)
        {
            //int barWidth = (int)(barBG.Width * barFrac + .5f);

            //if (barWidth == null)
            //barWidth = 1;

            HudElem bar = (HudElem)barBG.GetField("bar");
            bar.SetField("frac", barFrac);
            //bar.SetShader("progress_bar_fill", barWidth, barBG.Height);

            if (rateOfChange > 0)
                bar.ScaleOverTime(rateOfChange, barFrac, bar.Height);
            else if (rateOfChange < 0)
                bar.ScaleOverTime(-1 * rateOfChange, barFrac, bar.Height);

            //bar.SetField("rateOfChange", rateOfChange);
            //int time = GetTime();
            //bar.SetField("lastUpdateTime", time);
        }
    }
}
