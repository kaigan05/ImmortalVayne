using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using SharpDX.Direct3D9;
using Font = SharpDX.Direct3D9.Font;
namespace ImmortalVayne
{
    public class Time
    {
        public bool CalledInvisible;
        public bool CalledVisible;
        public int InvisibleTime;
        public int VisibleTime;
    }

    public static class GankDetector
    {
        public static Font Text;
        public static Menu MenuGank;
        private static readonly Dictionary<Obj_AI_Hero, Time> Enemies = new Dictionary<Obj_AI_Hero, Time>();

        public static void AddGankMenu()
        {
            AddMenu();
            Text = new Font(
                    Drawing.Direct3DDevice,
                    new FontDescription
                    {
                        FaceName = "Calibri",
                        Height = 50,
                        OutputPrecision = FontPrecision.Default,
                        Quality = FontQuality.Default,
                    });
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsEnemy)
                {
                    Enemies.Add(hero, new Time());
                }
            }
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnPreReset += eventArgs => Text.OnLostDevice();
            Drawing.OnPostReset += eventArgs => Text.OnResetDevice();
            Drawing.OnDraw += Drawing_OnDraw;
            AppDomain.CurrentDomain.DomainUnload += (sender, eventArgs) => Text.Dispose();
            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) => Text.Dispose();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (IsActive())
            {
                int drawGank = MenuGank.Item("RDetect").GetValue<Slider>().Value;
                foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
                {
                    if (hero.IsVisible && hero.IsEnemy && hero.IsValid && !hero.IsDead &&
                        hero.Distance(ObjectManager.Player.Position) <= drawGank + 1000 &&
                        hero.Distance(ObjectManager.Player.Position) >= 1000)
                    {
                        Utility.DrawCircle(hero.Position, drawGank, Color.Red, 20);
                    }
                }
            }
        }

        public static bool IsActive()
        {
            return MenuGank.Item("ADetect").GetValue<bool>();
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (!IsActive())
                return;
            foreach (var enemy in Enemies)
            {
                UpdateTime(enemy);
            }
        }

        private static void UpdateTime(KeyValuePair<Obj_AI_Hero, Time> enemy)
        {
            Obj_AI_Hero hero = enemy.Key;
            if (!hero.IsValid)
                return;
            if (hero.IsVisible)
            {
                ChatAndPing(enemy);
                Enemies[hero].InvisibleTime = 0;
                Enemies[hero].VisibleTime = (int) Game.Time;
            }
            else
            {
                if (Enemies[hero].VisibleTime != 0)
                {
                    Enemies[hero].InvisibleTime = (int) (Game.Time - Enemies[hero].VisibleTime);
                }
                else
                {
                    Enemies[hero].InvisibleTime = 0;
                }
            }
        }

        public static void AddMenu()
        {
            MenuGank = new Menu("GankDetector", "GDetect", true);
            MenuGank.AddItem(new MenuItem("RDetect", "Range").SetValue(new Slider(2500, 1, 3000)));
            MenuGank.AddItem(new MenuItem("TPing", "Ping").SetValue(new StringList(new[] {"Local Ping", "Server Ping"})));
            MenuGank.AddItem(new MenuItem("ADetect", "Active").SetValue(true));
            MenuGank.AddToMainMenu();
        }
        public static void ChatAndPing(KeyValuePair<Obj_AI_Hero, Time> enemy)
        {
            if (IsActive())
            {
                Obj_AI_Hero hero = enemy.Key;
                int drawGank = MenuGank.Item("RDetect").GetValue<Slider>().Value;
                if (hero.IsValid && !hero.IsDead && hero.Distance(ObjectManager.Player.Position) <= drawGank + 1000)
                {
                    if (enemy.Value.InvisibleTime > 5)
                    {

                        string note = "Gank: " + hero.ChampionName;
                        int x = Drawing.Width / 2 - note.Length * 19 / 2;
                        int y = Drawing.Height / 2;
                        var t = MenuGank.Item("TPing").GetValue<StringList>();
                        switch (t.SelectedIndex)
                        {
                            case 0:
                                Text.DrawText(null, note,x,y, new ColorBGRA(255, 0, 0, 255));
                                //Drawing.DrawText(Drawing.Width * 0.44f, Drawing.Height * 0.7f, Color.PaleVioletRed, "Gank: " + ObjectManager.Player.ChampionName);
                                Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(hero.Position.X, hero.Position.Y,
                    0, 0, Packet.PingType.Danger)).Process();
                                break;
                            case 1:
                                Text.DrawText(null, note,x,y, new ColorBGRA(255, 0, 0, 255));
                               Packet.C2S.Ping.Encoded(new Packet.C2S.Ping.Struct(ObjectManager.Player.Position.X + 100,
                    ObjectManager.Player.Position.Y + 100, 0, Packet.PingType.Danger)).Send();
                                break;
                        }
                    }
                }
            }
        }
    }
}