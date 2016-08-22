using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ensage.Heroes;
using Ensage.Common.Objects;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Extensions.Damage;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using MeepoSharpPlus.Utitilies;
using SharpDX;

namespace MeepoSharpPlus.Features
{
    public class JungleFarm
    {
        public JungleFarm()
        {

        }

        private List<Meepo> meepoList
        {
            get
            {
                return Variables.MeepoList;
            }
        }

        private List<MeepoSettings> MeepoSet
        {
            get
            {
                return Variables.MeepoSet;
            }
        }

        private Hero me
        {
            get
            {
                return Variables.Hero;
            }
        }

        private static readonly Dictionary<Unit, uint> LastCheckedHp = new Dictionary<Unit, uint>();

        public void Execute()
        {
            foreach (var m in meepoList.Where(x => Variables.OrderStates[x.Handle] == Variables.OrderState.Jungle))
            {
                Farm(m);
            }
        }

        public void Camp_Update()
        {
            if (!Utils.SleepCheck("Camp.Update"))
                return;
            Utils.Sleep(150, "Camp.Update");
            var curSec = (61 - Game.GameTime % 60) * 1000;
            foreach (var camp in Utitilies.JungleCamps.GetCamps)
            {
                foreach (
                    var enemy in
                        from meepo in
                            Variables.MeepoSet.Where(x => x.CurrentOrderState == Variables.OrderState.Jungle && x.IsAlive)
                                .Select(y => y.Hero)
                        let heroDist = meepo.Distance2D(camp.CampPosition)
                        where heroDist < 100
                        select ObjectManager.GetEntities<Unit>()
                            .Any(
                                x =>
                                    x.IsAlive && x.IsVisible && x.Team != Variables.Hero.Team && x.Distance2D(meepo) <= 500 &&
                                    !x.IsWaitingToSpawn))
                {
                    camp.canBeHere = enemy;

                    //Print("[CampStatus]: "+enemy);
                    if (enemy || camp.delayed) continue;
                    camp.delayed = true;
                    DelayAction.Add(curSec, () =>
                    {
                        camp.delayed = false;
                        camp.canBeHere = true;
                        //Print("[CampStatus]: delayAction" + camp.CanBeHere);
                    });
                }
            }
        }

        public void Farm(Hero me)
        {
            var s = Utitilies.JungleCamps.FindClosestCamp(me, Variables.MenuManager.JungleFarmMenu.Item("JungleFarm.TeamCheck").GetValue<bool>(),
                Variables.MenuManager.JungleFarmMenu.Item("JungleFarm.Ancient").GetValue<bool>());
            //var s = Ensage.Common.Objects.JungleCamps.FindClosestCamp(me.Position);
            var enemyHeroes = Heroes.GetByTeam(me.GetEnemyTeam()).Where(x => x.IsAlive).ToList();
            var dist = Variables.MenuManager.JungleFarmMenu.Item("JungleFarm.EscapeRange").GetValue<Slider>().Value;
            if (Variables.MenuManager.JungleFarmMenu.Item("JungleFarm.EscapeFromAnyEnemyHero").GetValue<bool>() &&
                    enemyHeroes.Any(x => x.Distance2D(me) <= dist)) //escape from hero
            {
                var handle = me.Handle;
                Variables.OrderStates[handle] = Variables.OrderState.Escape;
                Variables.NeedHeal[handle] = true;
            }
            string name;

            if (s == null)
            {
                s = Utitilies.JungleCamps.GetCamps.Where(y => y.canBeHere).OrderBy(x => x.CampPosition.Distance2D(me)).FirstOrDefault();
                if (s != null)
                {
                    name = MeepoSet.Find(x => Equals(x.Hero, me)).Handle.ToString();
                    if (Utils.SleepCheck("MOVIER_jungle" + name))
                    {
                        me.Move(s.StackPosition);
                        Utils.Sleep(500, "MOVIER_jungle" + name);
                    }
                }
            }

            
            name = MeepoSet.Find(x => Equals(x.Hero, me)).Handle.ToString();
            
            var anyMeepo =
                MeepoSet.Where(
                    x =>
                        x.CurrentOrderState == Variables.OrderState.Jungle && x.IsAlive && x.Handle != me.Handle &&
                        x.Hero.Health >= Variables.MenuManager.EscapeMenu.Item("Escape.MinRange").GetValue<Slider>().Value)
                    .OrderBy(y => y.Hero.Distance2D(me))
                    .FirstOrDefault();
            if (anyMeepo != null && me.Health <= 500 && anyMeepo.Hero.Distance2D(me) <= 400 &&
                CheckForChangedHealth(me))
            {
                if (!Utils.SleepCheck(name + "attack_test")) return;
                Utils.Sleep(200, name + "attack_test");
                var enemy =
                    ObjectManager.GetEntities<Unit>()
                        .FirstOrDefault(
                            x =>
                                x.IsAlive && x.IsVisible && x.Team != me.Team && x.Distance2D(me) <= 500 &&
                                !x.IsWaitingToSpawn);
                if (enemy != null)
                {
                    var creep = enemy.Position;
                    var ang = me.FindAngleBetween(creep, true);
                    var p = new Vector3((float)(me.Position.X - 250 * Math.Cos(ang)),
                        (float)(me.Position.Y - 250 * Math.Sin(ang)), 0);
                    me.Move(p);
                }
                me.Attack(anyMeepo.Hero, true);
                return;
            }
            
            
            var mySet = MeepoSet.Find(x => Equals(x.Hero, me));
            var w = mySet.poof;
            if (w != null && w.CanBeCasted())
            {
                var enemy =
                    ObjectManager
                        .GetEntities<Unit>(
                        )
                        .FirstOrDefault(
                            x =>
                                x.IsAlive && x.Health > 80 && x.IsVisible && x.Team != me.Team &&
                                x.Distance2D(me) <= 375 &&
                                !x.IsWaitingToSpawn);
                if (enemy != null && Utils.SleepCheck("jungle_farm_w" + name))
                {
                    w.Use(enemy.Position);
                    Utils.Sleep(1500, "jungle_farm_w_inCasting" + name);
                    Utils.Sleep(250, "jungle_farm_w" + name);
                }
                else if (enemy == null && !Utils.SleepCheck("jungle_farm_w_inCasting" + name) &&
                         Utils.SleepCheck("jungle_farm_w_stop" + name))
                {
                    me.Stop();
                    Utils.Sleeps.Remove("jungle_farm_w_inCasting" + name);
                    Utils.Sleep(500, "jungle_farm_w_stop" + name);
                }
            }
            if (!Utils.SleepCheck(name + "attack") || me.IsAttacking()) return;
            Utils.Sleep(500, name + "attack");
            me.Attack(s.CampPosition);
            
        }

        private static bool CheckForChangedHealth(Unit me)
        {
            uint health;
            if (!LastCheckedHp.TryGetValue(me, out health))
            {
                LastCheckedHp.Add(me, me.Health);
            }
            var boolka = health > me.Health;
            LastCheckedHp[me] = me.Health;
            return boolka;
        }

        private bool noNeutralsAround(Hero me)
        {
            var Neutrals = ObjectManager.GetEntities<Unit>().Where(x => x != null && x.IsValid && x.IsAlive && x.IsVisible && x.IsNeutral &&
                                                                    x.ClassID.Equals(ClassID.CDOTA_BaseNPC_Creep_Neutral) &&
                                                                    x.Distance2D(me) <= 500);
            if (Neutrals == null) return true;
            return false;
        }
    }
}
