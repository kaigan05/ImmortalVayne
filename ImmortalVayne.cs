using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace ImmortalVayne
{
    internal class ImmortalVayne
    {
        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static String CharName = "Vayne";
        public static LeagueSharp.Common.Spell Q, W, E, R;
        private static Spell[] _levelUpSeq ;
        private static Spell[] _levelUpSeq2;
       private static Activator _activator;
        private static MenuWrapper _menu;
        private static Orbwalking.Orbwalker _cOrbwalker;
        public static readonly Dictionary<string, MenuWrapper.BoolLink> BoolLinks =
            new Dictionary<string, MenuWrapper.BoolLink>();

        public static readonly Dictionary<string, MenuWrapper.CircleLink> CircleLinks =
            new Dictionary<string, MenuWrapper.CircleLink>();

        public static readonly Dictionary<string, MenuWrapper.KeyBindLink> KeyLinks =
            new Dictionary<string, MenuWrapper.KeyBindLink>();

        public static readonly Dictionary<string, MenuWrapper.SliderLink> SliderLinks =
            new Dictionary<string, MenuWrapper.SliderLink>();
        public static readonly Dictionary<string, MenuWrapper.StringListLink> StringListLinks =
           new Dictionary<string, MenuWrapper.StringListLink>();

        public ImmortalVayne()
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
        private static void SetSpells()
        {
            Q = new Spell(SpellSlot.Q, 300f);
            E = new Spell(SpellSlot.E, 650f);
            W = new Spell(SpellSlot.W);
            R = new Spell(SpellSlot.R);
            E.SetTargetted(0.25f, 2200f);
            _levelUpSeq = new[] { Q, E, W, W, W, R, W, Q, W, Q, R, Q, Q, E, E, R, E, E };
            _levelUpSeq2 = new[] { Q, W, E, W, W, R, W, Q, W, Q, R, Q, Q, E, E, R, E, E };
            Player.Spellbook.LevelUpSpell(_levelUpSeq[0].Slot);
        }

        private void Game_OnGameLoad(EventArgs args)
        {
            _activator=new Activator();
            //GankDetector.AddGankMenu();
            if (Player.ChampionName != CharName) return;
            SetSpells();
            _menu = new MenuWrapper("Immortal Vayne");
            var combo = _menu.MainMenu.AddSubMenu("Commbo");
            BoolLinks.Add("UseQC", combo.AddLinkedBool("Use Q in Combo"));
            BoolLinks.Add("UseEC", combo.AddLinkedBool("Use E in Combo"));
            var farm = _menu.MainMenu.AddSubMenu("Farm");
            BoolLinks.Add("UseQLH", farm.AddLinkedBool("Use Q in Last Hit"));
            BoolLinks.Add("UseQLC", farm.AddLinkedBool("Use Q in Lane Clear"));
            SliderLinks.Add("QManaLH", farm.AddLinkedSlider("Min Q Mana % LH",35,1,100));
            SliderLinks.Add("QManaLC", farm.AddLinkedSlider("Min Q Mana % LC", 35, 1, 100));
            var misc = _menu.MainMenu.AddSubMenu("Misc");
            BoolLinks.Add("Packets", misc.AddLinkedBool("Packet Casting"));
            BoolLinks.Add("AntiGP", misc.AddLinkedBool("Anti Gapcloser"));//Interrupter
            BoolLinks.Add("Interrupt", misc.AddLinkedBool("Interrupter"));
            BoolLinks.Add("AutoE", misc.AddLinkedBool("Auto E"));
            BoolLinks.Add("TryQE", misc.AddLinkedBool("Auto Q->E"));
            BoolLinks.Add("NoEEnT", misc.AddLinkedBool("No E Under enemy turret"));
            KeyLinks.Add("ThreshLantern", misc.AddLinkedKeyBind("Grab Thresh..", "S".ToCharArray()[0], KeyBindType.Press));
            var noComdemn = misc.AddSubMenu("Don't Condemn");
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                BoolLinks.Add(hero.ChampionName, noComdemn.AddLinkedBool(hero.ChampionName,false));
            }
            KeyLinks.Add("eNextAA", misc.AddLinkedKeyBind("E after next AA", "T".ToCharArray()[0], KeyBindType.Toggle));
            var autoLevel = misc.AddSubMenu("Auto Level");
            StringListLinks.Add("AutoLevel", autoLevel.AddLinkedStringList("Auto Level", new[] { "Q E W start", "Q W E start" }));
            BoolLinks.Add("ActiveAutoLevel", autoLevel.AddLinkedBool("Active"));

            SliderLinks.Add("PushDistance", misc.AddLinkedSlider("Push Distance", 450, 350, 470));
            var item = _menu.MainMenu.AddSubMenu("Items");
            var qss=item.AddSubMenu("Quicksilver");
            BoolLinks.Add("Stun", qss.AddLinkedBool("Stuns"));
            BoolLinks.Add("Charm", qss.AddLinkedBool("Charms"));
            BoolLinks.Add("Taunt", qss.AddLinkedBool("Taunts"));
            BoolLinks.Add("Fear", qss.AddLinkedBool("Fears"));
            BoolLinks.Add("Snare", qss.AddLinkedBool("Snares"));
            BoolLinks.Add("Silence", qss.AddLinkedBool("Silences"));
            BoolLinks.Add("Supression", qss.AddLinkedBool("Supression"));
            BoolLinks.Add("Polymorph", qss.AddLinkedBool("Polymorphs"));
            BoolLinks.Add("Blind", qss.AddLinkedBool("Blinds"));
            BoolLinks.Add("Slow", qss.AddLinkedBool("Slows"));
            BoolLinks.Add("Poison", qss.AddLinkedBool("Poisons"));
            var qsSpell=qss.AddSubMenu("QS Spells");
            foreach (var t in _activator.BuffList)
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
                {
                    if (t.ChampionName == enemy.ChampionName)
                        BoolLinks.Add(t.BuffName, qsSpell.AddLinkedBool(t.DisplayName, t.DefaultValue));
                }
            }
            BoolLinks.Add("Botrk", item.AddLinkedBool("Use Blade of the Ruined King"));
            BoolLinks.Add("Youmuu", item.AddLinkedBool("Use Youmuu's Ghostblade"));
            BoolLinks.Add("Cutlass", item.AddLinkedBool("Use Bilgewater Cutlass"));
            SliderLinks.Add("OwnHPercBotrk", item.AddLinkedSlider("Min Own H. % Botrk", 50, 1, 100));
            SliderLinks.Add("EnHPercBotrk", item.AddLinkedSlider("Min Enemy H. % Botrk", 20, 1, 100));
            var drawing = _menu.MainMenu.AddSubMenu("Drawing Options");
            CircleLinks.Add("DrawQ", drawing.AddLinkedCircle("Draw Q Range", true, Color.HotPink, E.Range));
            CircleLinks.Add("DrawAA", drawing.AddLinkedCircle("Draw AA & E Range", true, Color.SpringGreen, E.Range));
            CircleLinks.Add("DrawEAA", drawing.AddLinkedCircle("Draw Enemy AA Range", true, Color.Tomato, E.Range));
            CircleLinks.Add("DrawCond", drawing.AddLinkedCircle("Draw Pos. Aft. E if Stun", true, Color.Red, E.Range));
            _cOrbwalker = _menu.Orbwalker;
            
            Orbwalking.AfterAttack += OrbwalkerAfterAttack;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            CustomEvents.Unit.OnLevelUp += OnLevelUp;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.PrintChat("<font color='#FF0000'>Immortal Vayne</font> <font color='#FFFFFF'>loaded!!</font>");
        }
        private void OnLevelUp(Obj_AI_Base sender, CustomEvents.Unit.OnLevelUpEventArgs args)
        {
                if (!BoolLinks["ActiveAutoLevel"].Value)
                    return;
                if (StringListLinks["AutoLevel"].Value.SelectedIndex == 0)
                {
                    Player.Spellbook.LevelUpSpell(_levelUpSeq[args.NewLevel - 1].Slot);

                }
                else if (StringListLinks["AutoLevel"].Value.SelectedIndex == 1)
                {
                    Player.Spellbook.LevelUpSpell(_levelUpSeq2[args.NewLevel - 1].Slot);
                }
            
        }

        private void OrbwalkerAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe || !(target is Obj_AI_Hero) || _cOrbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
                return;
            var tar = (Obj_AI_Hero)target;
            if (BoolLinks["UseQC"].Value)
                TryQeCheck(tar);
            if (E.IsReady() && tar.IsValidTarget(E.Range) && KeyLinks["eNextAA"].Value.Active)
            {
                CastE(tar);
                _menu.MainMenu.MenuHandle.SubMenu("misc").Item("misceafternextaa").SetValue(new KeyBind(KeyLinks["eNextAA"].Value.Key, KeyBindType.Toggle, false));
            }
            UseItems(tar);
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead) return;
            CleanBuffType();
            CleanserBySpell();
            if (IsMenuEnabled("AutoE"))
            {
                Obj_AI_Hero tar;
                if (CondemnCheck(Player.Position, out tar))
                {
                    CastE(tar);
                }
            }
            if (KeyLinks["ThreshLantern"].Value.Active)
                TakeLantern();
            QFarmCheck();
            if (_cOrbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo ||
                _cOrbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                Obj_AI_Hero tar2;
                if (IsMenuEnabled("UseEC") && CondemnCheck(Player.ServerPosition, out tar2))
                {
                    CastE(tar2);
                }
            }
        }


        private void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            var drawAa = CircleLinks["DrawAA"].Value;
            var drawEaaq = CircleLinks["DrawEAA"].Value;
            var drawCond = CircleLinks["DrawCond"].Value;
            var drawQ = CircleLinks["DrawQ"].Value;
            if (drawAa.Active) Utility.DrawCircle(Player.Position, Player.AttackRange + 100, drawAa.Color);
            if (drawEaaq.Active)
            {
                foreach (
                    Obj_AI_Hero hero in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(
                                hero => hero.IsEnemy && hero.IsValidTarget() && hero.Distance(Player.Position) <= 1500))
                {
                    Utility.DrawCircle(hero.Position, hero.AttackRange, drawEaaq.Color);
                }
            }
            if (drawQ.Active) Utility.DrawCircle(Player.Position, Q.Range, drawQ.Color);
            if (drawCond.Active) DrawPostCondemn();
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var gpSender = (Obj_AI_Hero) gapcloser.Sender;
            if (!IsMenuEnabled("AntiGP") || !E.IsReady() || !gpSender.IsValidTarget()) return;
            CastE(gpSender);
        }

        private void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            var sender = (Obj_AI_Hero) unit;
            if (!IsMenuEnabled("Interrupt") || !E.IsReady() || !sender.IsValidTarget()) return;
            CastE(sender);
        }

        private bool CondemnCheck(Vector3 position, out Obj_AI_Hero target)
        {
            if (IsUnderEnTurret(Player.Position) && IsMenuEnabled("NoEEnT"))
            {
                target = null;
                return false;
            }
            
            foreach (
                Obj_AI_Hero en in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            hero =>
                                hero.IsEnemy && hero.IsValidTarget() && !IsMenuEnabled(hero.ChampionName) &&
                                hero.Distance(Player.Position) <= E.Range))
            {
                PredictionOutput ePred = E.GetPrediction(en);
                int pushDist = SliderLinks["PushDistance"].Value.Value;
                for (int i = 0; i < pushDist; i += (int) en.BoundingRadius)
                {
                    Vector3 loc3 = ePred.UnitPosition.To2D().Extend(position.To2D(), -i).To3D();
                    if (IsWall(loc3) || IsUnderTurret(loc3))
                    {
                        target = en;
                        return true;
                    }
                }
            }
            target = null;
            return false;
        }

        private void QFarmCheck()
        {
            if (!Q.IsReady()) return;
            Vector2 posAfterQ = Player.Position.To2D().Extend(Game.CursorPos.To2D(), 300);
            IEnumerable<Obj_AI_Base> minList =
                MinionManager.GetMinions(Player.Position, 550f).Where(min =>
                    HealthPrediction.GetHealthPrediction(min,
                        (int) (Q.Delay + min.Distance(posAfterQ)/Orbwalking.GetMyProjectileSpeed())*1000) + Game.Ping <=
                    (Q.GetDamage(min) + Player.GetAutoAttackDamage(min))
                    &&
                    HealthPrediction.GetHealthPrediction(min,
                        (int) (Q.Delay + min.Distance(posAfterQ)/Orbwalking.GetMyProjectileSpeed())*1000) + Game.Ping >
                    0);
            if (minList.Count()!=0&&!minList.First().IsValidTarget()) return;
            //CastQ(Vector3.Zero, minList.First());
        }

        private void TryQeCheck(Obj_AI_Hero target)
        {
            if (!Q.IsReady() || !target.IsValidTarget()) return;
            if (!IsMenuEnabled("TryQE") || !E.IsReady())
            {
                CastQ(Game.CursorPos, target);
            }
            else
            {
                for (int I = 0; I <= 360; I += 65)
                {
                    Vector3 f1 =
                        new Vector2(Player.Position.X + (float) (300*Math.Cos(I*(Math.PI/180))),
                            Player.Position.Y + (float) (300*Math.Sin(I*(Math.PI/180)))).To3D();
                    Obj_AI_Hero targ;
                    if (CondemnCheck(f1, out targ))
                    {
                        CastTumblePos(f1, target);
                        CastE(target);
                        return;
                    }
                }
                CastQ(Game.CursorPos, target);
            }
        }

        private void CastQ(Vector3 pos, Obj_AI_Base target)
        {
            if (!Q.IsReady() || !target.IsValidTarget()) return;
            switch (_cOrbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.LastHit:
                    int manaLh = SliderLinks["QManaLH"].Value.Value;
                    if (Player.ManaPercentage() >= manaLh && IsMenuEnabled("UseQLH"))
                    {
                        CastTumblePos(pos, target);
                    }
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    var manaLc = SliderLinks["QManaLC"].Value.Value;
                    if (Player.ManaPercentage() >= manaLc && IsMenuEnabled("UseQLC"))
                    {
                        CastTumblePos(pos, target);
                    }
                    break;
                case Orbwalking.OrbwalkingMode.Combo:
                case Orbwalking.OrbwalkingMode.Mixed:
                    CastTumblePos(pos, target);
                    break;
            }
        }

        private void CastTumblePos(Vector3 pos, Obj_AI_Base target)
        {
            Vector3 posAfterTumble =
                ObjectManager.Player.ServerPosition.To2D().Extend(pos.To2D(), 300).To3D();
            float distanceAfterTumble = Vector3.DistanceSquared(posAfterTumble, target.ServerPosition);
            if (distanceAfterTumble < 550*550 && distanceAfterTumble > 100*100)
                Q.Cast(pos, IsMenuEnabled("Packets"));
        }

        private void CastE(Obj_AI_Hero target)
        {
            if (!E.IsReady() || !target.IsValidTarget(E.Range)) return;
            E.Cast(target, IsMenuEnabled("Packets"));
        }

        private static bool IsWall(Vector3 pos)
        {
            CollisionFlags cFlags = NavMesh.GetCollisionFlags(pos);
            return (cFlags == CollisionFlags.Wall);
        }

        private static bool IsUnderTurret(Vector3 position)
        {
            return ObjectManager.Get<Obj_AI_Turret>().Where(turr => turr.IsAlly && (turr.Health != 0)).Any(tur => tur.Distance(position) <= 975f);
        }

        private static bool IsUnderEnTurret(Vector3 position)
        {
            return ObjectManager.Get<Obj_AI_Turret>().Where(turr => turr.IsEnemy && (turr.Health != 0)).Any(tur => tur.Distance(position) <= 975f);
        }

        public static bool IsMenuEnabled(string value)
        {
            return BoolLinks[value].Value;
        }
        //private static float GetPerValue(bool mana)
        //{
        //    if (mana) return (Player.Mana / Player.MaxMana) * 100;
        //    return (Player.Health / Player.MaxHealth) * 100;
        //}

        //private static float GetPerValueTarget(Obj_AI_Hero target)
        //{
        //    return (target.Health / target.MaxHealth) * 100;
        //}

        private void DrawPostCondemn()
        {
            var drawCond = CircleLinks["DrawCond"].Value;
            foreach (
                Obj_AI_Hero en in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            hero =>
                                hero.IsEnemy && hero.IsValidTarget() && !BoolLinks[hero.ChampionName].Value &&
                                hero.Distance(Player.Position) <= E.Range))
            {
                PredictionOutput ePred = E.GetPrediction(en);
                int pushDist = SliderLinks["PushDistance"].Value.Value;
                for (int i = 0; i < pushDist; i += (int) en.BoundingRadius)
                {
                    Vector3 loc3 = ePred.UnitPosition.To2D().Extend(Player.Position.To2D(), -i).To3D();
                    if (IsWall(loc3) || loc3.UnderTurret(true)) Utility.DrawCircle(loc3, 100f, drawCond.Color);
                }
            }
        }

        #region Items & Tumble

        private void UseItems(Obj_AI_Hero tar)
        {
            if ((BoolLinks["Botrk"].Value && _cOrbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo) &&
                (SliderLinks["OwnHPercBotrk"].Value.Value <= Player.HealthPercentage()) &&
                ((SliderLinks["EnHPercBotrk"].Value.Value <= tar.HealthPercentage())))
            {
                UseItem(3153, tar);
            }
            if (BoolLinks["Youmuu"].Value && _cOrbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                UseItem(3142);
            }
            if (BoolLinks["Cutlass"].Value && _cOrbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                UseItem(3144, tar);
            }
        }

        private static void TakeLantern()
        {
            foreach (GameObject obj in ObjectManager.Get<GameObject>())
            {
                if (obj.Name.Contains("ThreshLantern") &&
                    obj.Position.Distance(ObjectManager.Player.ServerPosition) <= 500 && obj.IsAlly)
                {
                    GamePacket pckt =
                        Packet.C2S.InteractObject.Encoded(
                            new Packet.C2S.InteractObject.Struct(ObjectManager.Player.NetworkId, obj.NetworkId));
                    pckt.Send();
                    return;
                }
            }
        }

        #endregion
        public static void UseItem(int id, Obj_AI_Hero target = null)
        {
            if (Items.HasItem(id) && Items.CanUseItem(id))
            {
                Items.UseItem(id, target);
            }
        }
        private static void Cleanse()
        {
            if (Items.HasItem(3140)) UseItem(3140, Player); //QSS
            if (Items.HasItem(3139)) UseItem(3139, Player); //Mercurial
            if (Items.HasItem(3137)) UseItem(3137, Player); //Dervish Blade
        }
        private static void CleanBuffType()
        {
            if (Items.HasItem(3139) || Items.HasItem(3140) || Items.HasItem(3137))
            {
                if (UnitBuffs(Player) >= 1)
                    Cleanse();
            }
            
        }
        internal static void CleanserBySpell()
        {
            if (Items.HasItem(3139) || Items.HasItem(3140) || Items.HasItem(3137))
            {
                List<BuffList> ccList = (from spell in _activator.BuffList
                                   where Player.HasBuff(spell.BuffName)
                                         select new BuffList { BuffName = spell.BuffName }).ToList();
                foreach (BuffList cc in ccList)
                {
                    if (IsMenuEnabled(cc.BuffName))
                    {
                        if (cc.BuffName == "zedulttargetmark")
                        {
                            Utility.DelayAction.Add(cc.Delay, Cleanse);
                        }
                        else
                        {
                            Cleanse();
                        }
                    }
                }
            }
            
        }

        private static int UnitBuffs(Obj_AI_Hero unit)
        {
            int cc = 0;
            if (BoolLinks["Slow"].Value)
                if (unit.HasBuffOfType(BuffType.Slow))
                    cc += 1;
            if (BoolLinks["Blind"].Value)
                if (unit.HasBuffOfType(BuffType.Blind))
                    cc += 1;
            if (BoolLinks["Charm"].Value)
                if (unit.HasBuffOfType(BuffType.Charm))
                    cc += 1;
            if (BoolLinks["Fear"].Value)
                if (unit.HasBuffOfType(BuffType.Fear))
                    cc += 1;
            if (BoolLinks["Snare"].Value)
                if (unit.HasBuffOfType(BuffType.Snare))
                    cc += 1;
            if (BoolLinks["Taunt"].Value)
                if (unit.HasBuffOfType(BuffType.Taunt))
                    cc += 1;
            if (BoolLinks["Supression"].Value)
                if (unit.HasBuffOfType(BuffType.Suppression))
                    cc += 1;
            if (BoolLinks["Stun"].Value)
                if (unit.HasBuffOfType(BuffType.Stun))
                    cc += 1;
            if (BoolLinks["Polymorph"].Value)
                if (unit.HasBuffOfType(BuffType.Polymorph))
                    cc += 1;
            if (BoolLinks["Silence"].Value)
                if (unit.HasBuffOfType(BuffType.Silence))
                    cc += 1;
            if (BoolLinks["Poison"].Value)
                if (unit.HasBuffOfType(BuffType.Poison))
                    cc += 1;
            return cc;
        }
    }
}