using LeagueSharp;
using LeagueSharp.Common;
using PFTC.Model;
using PFTC.Util;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PFTC.Plugin
{
    internal class Poppy : PluginModel
    {
        public Spell Q;
        public Spell W;
        public Spell E;
        public Spell R;

        public Poppy()
        {
            Q = new Spell(SpellSlot.Q, 250);
            W = new Spell(SpellSlot.W, 0);
            E = new Spell(SpellSlot.E, 480);
            R = new Spell(SpellSlot.R, 900);

            Game.OnUpdate += GameOnOnGameUpdate;
            Drawing.OnDraw += DrawingOnOnDraw;
            OnBeforeAttack();

            MiscControl.PrintChat(MiscControl.stringColor(LanguageDic["load"], MiscControl.TableColor.Red));
        }

        private void DrawingOnOnDraw(EventArgs args)
        {
            var drawQ = GetBool("drawQ");
            var drawE = GetBool("drawE");
            var drawR = GetBool("drawR");

            var target = TargetSelector.GetTarget(1300, TargetSelector.DamageType.Magical);

            var p = Player.Position;

            if (GetBool("disableAll"))
                return;

            if (drawQ)
                Render.Circle.DrawCircle(p, Q.Range, Q.IsReady() ? System.Drawing.Color.Aqua : System.Drawing.Color.Red);

            if (drawE)
                Render.Circle.DrawCircle(p, E.Range, E.IsReady() ? System.Drawing.Color.Aqua : System.Drawing.Color.Red);

            if (drawR)
                Render.Circle.DrawCircle(p, R.Range, R.IsReady() ? System.Drawing.Color.Aqua : System.Drawing.Color.Red);

        }

        private void GameOnOnGameUpdate(EventArgs args)
        {

            switch (OrbwalkerMode)
            {
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;

                case Orbwalking.OrbwalkingMode.Combo:
                    Combar();
                    break;
            }

        }

        private void Combar()
        {
            var target = TargetSelector.GetTarget(1300, TargetSelector.DamageType.Physical);

            if(GetBool("comboE") && E.IsReady() && E.IsInRange(target) && colisionChecker(Player,target,E.Range) && !GetBool("nc"+target.ChampionName))
            {
                E.Cast(target);
            }

            if (GetBool("comboW") && W.IsReady() && target.Distance(Player.Position) <= 400 && !Q.IsInRange(target))
            {
                W.Cast();
            }

            if (GetBool("comboR") && R.IsReady() && R.IsInRange(target) && GetBool("nUlt" + target.ChampionName) && Player.GetEnemiesInRange(R.Range).Count >= GetValue<Slider>("minimumR").Value)
            {
                if (Utility.UnderTurret(target) && Q.IsReady() && E.IsReady())
                {
                    R.Cast(target);
                }
            }

            if(GetBool("comboR") && R.IsReady() && Player.HealthPercent <= GetValue<Slider>("hpautoR").Value)
            {
                var enemy = ObjectManager.Get<Obj_AI_Hero>()
                    .Where(x => !x.IsAlly)
                    .Where(x => !x.IsDead)
                    .Where(x => x.Direction != Player.Direction).LastOrDefault();

                if (enemy != null)
                {
                    R.Cast(enemy);
                }
            }

        }

        private void Harass()
        {
            var target = TargetSelector.GetTarget(1300, TargetSelector.DamageType.Physical);

            if (GetBool("harassE") && E.IsReady() && E.IsInRange(target) && colisionChecker(Player, target, E.Range) && !GetBool("nc" + target.ChampionName))
            {
                E.Cast(target);
            }

            if (GetBool("harassW") && W.IsReady() && target.Distance(Player.Position) <= 400 && !Q.IsInRange(target))
            {
                W.Cast();
            }
        }

        public void OnBeforeAttack()
        {
            Orbwalking.BeforeAttack += args =>
            {
                try
                {
                    if (args.Target.IsValid<Obj_AI_Minion>() && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                    {
                        args.Process = false;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            };
        }

        public override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (OrbwalkerMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (GetBool("comboQ") && Q.IsReady() && manaManager() && Q.IsInRange(target))
                {
                    Q.Cast();
                }
            }
            else if (OrbwalkerMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                if (GetBool("harassQ") && Q.IsReady() && manaManager() && Q.IsInRange(target))
                {
                    Q.Cast();
                }
            }
        }

        public override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (gapcloser.Sender.IsAlly || gapcloser.End.Distance(Player.Position) <= gapcloser.Sender.AttackRange)
            {
                return;
            }

            if (GetBool("antigapcloserW") && W.IsReady())
            {
                E.Cast(Player.Position.Extend(gapcloser.Sender.Position, 250));
            }
        }

        public override void OnPossibleToInterrupt(Obj_AI_Hero target, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (args.DangerLevel < Interrupter2.DangerLevel.High || target.IsAlly)
            {
                return;
            }

            if (GetBool("interruptE") && E.IsReady() && E.IsInRange(target))
            {
                E.Cast(target);
            }

        }

        public bool manaManager()
        {
            var mana = GetValue<Slider>("saveMana").Value;

            if (Player.Mana >= Player.MaxMana * (mana / 100))
            {
                return true;
            }

            return false;
        }

        public int enemiesInRange(Obj_AI_Hero obj, float range)
        {
            var nearEnemies =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(x => x.IsEnemy)
                        .Where(x => !x.IsDead)
                        .Where(x => x.Distance(obj.Position) <= range);
            return nearEnemies.Count();
        }

        private float GetComboDamage(Obj_AI_Base enemy)
        {
            if (enemy == null)
                return 0;

            double damage = 0d;

            if (Q.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);

            if (W.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.W) * 3;

            if (E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            if (R.IsReady())
            {
                damage += damage * 1.2;
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);
            }

            damage += Player.GetAutoAttackDamage(enemy) * 3;

            return (float)damage;
        }

        bool isUnderEnemyTurret(Vector3 Position)
        {
            foreach (var tur in ObjectManager.Get<Obj_AI_Turret>().Where(turr => turr.IsEnemy && (turr.Health != 0)))
            {
                if (tur.Distance(Position) <= 975f) return true;
            }
            return false;
        }

        bool colisionChecker(Obj_AI_Base from, Obj_AI_Base to, float range)
        {
            var pos = from.Position.Extend(to.Position, range);

            return pos.IsWall();
        }

        public override void Combo(Menu config)
        {
            config.AddItem(new MenuItem("comboQ", LanguageDic["use"] + " Q").SetValue(true));
            config.AddItem(new MenuItem("comboW", LanguageDic["use"] + " W").SetValue(true));
            config.AddItem(new MenuItem("comboE", LanguageDic["use"] + " E").SetValue(true));
            config.AddItem(new MenuItem("comboR", LanguageDic["use"] + " R").SetValue(true));
            var miscEMenu = new Menu("Ultimate", "ultChamp");
            {
                foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(x => !x.IsAlly))
                {
                    miscEMenu.AddItem(new MenuItem("nUlt" + hero.ChampionName, "Not Ult " + hero.ChampionName).SetValue(false));
                }
            }
            config.AddItem(new MenuItem("hpautoR", "% for auto R").SetValue(new Slider(50, 0, 100)));
            config.AddItem(new MenuItem("minimumR", LanguageDic["qnt"] + " R").SetValue(new Slider(2, 1, 5)));
        }

        public override void Harass(Menu config)
        {
            config.AddItem(new MenuItem("harassQ", LanguageDic["use"] + " Q").SetValue(true));
            config.AddItem(new MenuItem("harassW", LanguageDic["use"] + " W").SetValue(true));
            config.AddItem(new MenuItem("harassE", LanguageDic["use"] + " E").SetValue(false));
        }

        public override void Misc(Menu config)
        {
            var miscEMenu = new Menu("Charge", "chargeChamp");
            {
                foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(x => !x.IsAlly))
                {
                    miscEMenu.AddItem(new MenuItem("nc" + hero.ChampionName, "Not Charge " + hero.ChampionName).SetValue(false));
                }
            }
        }

        public override void Extra(Menu config)
        {
            config.AddItem(new MenuItem("antigapcloserW", LanguageDic["use"] + " W " + LanguageDic["gap"]).SetValue(true));
            config.AddItem(new MenuItem("interruptE", LanguageDic["use"] + " E " + LanguageDic["interrupt"]).SetValue(true));
        }

        public override void Drawings(Menu config)
        {
            config.AddItem(new MenuItem("disableAll", LanguageDic["disable"]).SetValue(false));
            config.AddItem(new MenuItem("drawQ", LanguageDic["show"] + " Q").SetValue(true));
            config.AddItem(new MenuItem("drawW", LanguageDic["show"] + " W").SetValue(true));
            config.AddItem(new MenuItem("drawE", LanguageDic["show"] + " E").SetValue(true));
            config.AddItem(new MenuItem("drawR", LanguageDic["show"] + " R").SetValue(true));
        }
    }
}
