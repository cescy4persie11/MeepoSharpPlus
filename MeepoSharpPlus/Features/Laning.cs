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
using MeepoSharpPlus.Abilities;

namespace MeepoSharpPlus.Features
{
    public class Laning
    {
        public Laning()
        {

        }

        private static readonly Dictionary<Vector3, string> LaneDictionary = new Dictionary<Vector3, string>()
        {
            {new Vector3(-6080, 5805, 384), "top"},
            {new Vector3(-6600, -3000, 384), "top"},
            {new Vector3(2700, 5600, 384), "top"},


            {new Vector3(5807, -5785, 384), "bot"},
            {new Vector3(-3200, -6200, 384), "bot"},
            {new Vector3(6200, 2200, 384), "bot"},


            {new Vector3(-600, -300, 384), "middle"},
            {new Vector3(3600, 3200, 384), "middle"},
            {new Vector3(-4400, -3900, 384), "middle"}

        };

        private static readonly Dictionary<Unit, uint> LastCheckedHp = new Dictionary<Unit, uint>();

        public void Execute()
        {
            foreach (var m in Variables.MeepoList.Where(x => Variables.OrderStates[x.Handle] == Variables.OrderState.Laning))
            {
                Lane(m);
            }
        }

        public void Lane(Meepo me)
        {
            var handle = me.Handle;
            var creeps = Creeps.All.Where(x => x != null && x.IsValid && x.IsAlive && x.IsVisible).ToList();
            var creepsEnemy = creeps.Where(x => x.Team != me.Team).ToList();
            var creepsAlly = creeps.Where(x => x.Team == me.Team).ToList();
            var enemyHeroes = Heroes.GetByTeam(me.GetEnemyTeam()).Where(x => x.IsAlive).ToList();
            var towers = Towers.all.Where(x => x.Team != me.Team).Where(x => x.IsAlive).ToList();
            var creepWithEnemy =
                creepsAlly.FirstOrDefault(
                    x => x.MaximumHealth * 65 / 100 < x.Health && creepsEnemy.Any(y => y.Distance2D(x) <= 1000));
            var travelBoots = me.FindItem("item_travel_boots") ?? me.FindItem("item_travel_boots_2");
            if (travelBoots != null && creepWithEnemy != null && Variables.MenuManager.LanePushMenu.Item("AutoPush.TravelBoots").GetValue<bool>() && Utils.SleepCheck("TravelBoots." + handle))
            {
                if (travelBoots.CanBeCasted() && !creepsEnemy.Any(x => x.Distance2D(me) <= 1000))
                {
                    travelBoots.UseAbility(creepWithEnemy);
                    Utils.Sleep(500, "TravelBoots." + handle);
                    return;
                }
            }

            var nearestTower =
                towers.OrderBy(y => y.Distance2D(me))
                    .FirstOrDefault() ?? Fountain.GetEnemyFountain();
            var fountain = Fountain.GetAllyFountain();
            var curlane = GetCurrentLane(me);
            var clospoint = GetClosestPoint(curlane);
            var useThisShit = clospoint.Distance2D(fountain) - 250 > me.Distance2D(fountain);
            var name = Variables.MeepoSet.Find(x => x.Handle == me.Handle).Handle.ToString();
            if (nearestTower != null && Utils.SleepCheck(name + "attack"))
            {
                var pos = curlane == "mid" || !useThisShit ? nearestTower.Position : clospoint;
                var dist = Variables.MenuManager.LanePushMenu.Item("AutoPush.EscapeRange").GetValue<Slider>().Value;
                if (Variables.MenuManager.LanePushMenu.Item("AutoPush.EscapeFromAnyEnemyHero").GetValue<bool>() &&
                    enemyHeroes.Any(x => x.Distance2D(me) <= dist)) //escape from hero
                {
                    Variables.OrderStates[handle] = Variables.OrderState.Escape;
                    Variables.NeedHeal[handle] = true;
                }
                else if (creepsAlly.Any(x => x.Distance2D(nearestTower) <= 800) ||
                         me.Distance2D(nearestTower) > 800)
                {
                    //under tower
                    var hpwasChanged = CheckForChangedHealth(me);
                    if (hpwasChanged)
                    {
                        var allyCreep = creepsAlly.OrderBy(x => x.Distance2D(me)).First();
                        if (allyCreep != null)
                        {
                            var towerPos = nearestTower.Position;
                            var ang = allyCreep.FindAngleBetween(towerPos, true);
                            var p = new Vector3((float)(allyCreep.Position.X - 250 * Math.Cos(ang)),
                                (float)(allyCreep.Position.Y - 250 * Math.Sin(ang)), 0);
                            me.Move(p);
                            me.Attack(allyCreep, true);
                            Utils.Sleep(1200, name + "attack");
                        }
                        else
                        {
                            var towerPos = nearestTower.Position;
                            var ang = me.FindAngleBetween(towerPos, true);
                            var p = new Vector3((float)(towerPos.X - 1250 * Math.Cos(ang)),
                                (float)(towerPos.Y - 1250 * Math.Sin(ang)), 0);
                            me.Move(p);
                            Utils.Sleep(500, name + "attack");
                        }
                    }
                    else
                    {
                        var act = me.NetworkActivity;
                        if (!Utils.SleepCheck("attack_time" + name))
                            return;

                        if (Variables.MenuManager.LanePushMenu.Item("AutoPush.LastHitMode").GetValue<bool>())
                        {
                            var bestEnemyCreep =
                                creepsEnemy.Where(x => x.Health < me.DamageAverage && x.Distance2D(me) <= 800)
                                    .OrderBy(x => x.Distance2D(me))
                                    .FirstOrDefault();
                            if (bestEnemyCreep != null)
                            {
                                me.Attack(bestEnemyCreep);
                                Utils.Sleep(UnitDatabase.GetAttackPoint(me) * 1000, "attack_time" + name);
                            }
                            else
                            {
                                /*if (act == NetworkActivity.Attack || act == NetworkActivity.Attack2)
                                {
                                    me.Stop();
                                }*/
                                if (act == NetworkActivity.Idle)
                                {
                                    me.Attack(pos);
                                }
                            }
                        }
                        else
                        {
                            if (act == NetworkActivity.Idle) me.Attack(pos);
                        }

                        if (Variables.MenuManager.LanePushMenu.Item("AutoPush.AutoW").GetValue<bool>() && Variables.poofList[handle] != null)
                        {
                            var w = Variables.poofList[handle];
                            var castRange = w.GetRealCastRange();
                            if (w.CanBeCasted() &&
                                creepsEnemy.Any(x => x.Distance2D(me) <= castRange && x.Health <= 60 + 20 * w.Level) &&
                                Utils.SleepCheck("w_push" + name))
                            {

                                w.Use(me);
                                Utils.Sleep(250, "w_push" + name);
                            }
                        }
                    }
                    Utils.Sleep(100, name + "attack");
                }
                else
                {
                    var towerPos = nearestTower.Position;
                    var ang = me.FindAngleBetween(towerPos, true);
                    var p = new Vector3((float)(me.Position.X - 1000 * Math.Cos(ang)),
                        (float)(me.Position.Y - 1000 * Math.Sin(ang)), 0);
                    me.Move(p);
                    Utils.Sleep(200, name + "attack");
                }
            }
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

        private static string GetCurrentLane(Unit me)
        {
            return LaneDictionary.OrderBy(x => x.Key.Distance2D(me)).First().Value;
        }

        private static Vector3 GetClosestPoint(string pos)
        {
            var list = LaneDictionary.Keys.ToList();
            switch (pos)
            {
                case "top":
                    return list[0];
                case "bot":
                    return list[3];
                default:
                    return list[6];
            }
        }
    }
}
