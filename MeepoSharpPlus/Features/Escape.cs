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
    public class Escape
    {
        public Escape()
        {

        }

        private List<Meepo> meepoList
        {
            get
            {
                return Variables.MeepoList;
            }
        }

        public void Execute(Hero target)
        {
            foreach (var meepo in meepoList)
            {
                UpdateEscapeState(meepo);
            }
            
            foreach (var m in meepoList.Where(m => Variables.OrderStates[m.Handle] == Variables.OrderState.Escape))
            {
                Escaping(m, Variables.earthBindList[m.Handle], Variables.poofList[m.Handle], target);
            }
        }

        public void UpdateEscapeState(Meepo me)
        {
            var handle = me.Handle;
            bool nh;
            if (!Variables.NeedHeal.TryGetValue(handle, out nh))
                Variables.NeedHeal.Add(handle, false);
            var perc = me.Health / (float)me.MaximumHealth * 100;
            if (Variables.NeedHeal[handle])
            {
                if ((perc > 95 &&
                     me.HasModifiers(new[] { "modifier_fountain_aura", "modifier_fountain_aura_buff" }, false)) ||
                    Variables.OrderStates[handle] != Variables.OrderState.Escape)
                {
                    Variables.NeedHeal[handle] = false;
                    Variables.OrderStates[handle] = Variables.OrderState.Idle;
                }
            }
            else
            {
                if (perc < Variables.MenuManager.EscapeMenu.Item("Escape.MinRangePercent").GetValue<Slider>().Value || me.Health <= Variables.MenuManager.EscapeMenu.Item("Escape.MinRange").GetValue<Slider>().Value)
                {
                    Variables.NeedHeal[handle] = true;
                    Variables.LastOrderStates[handle] = Variables.OrderStates[handle] == Variables.OrderState.Escape
                        ? Variables.OrderState.Idle
                        : Variables.OrderStates[handle];

                    Variables.OrderStates[handle] = Variables.OrderState.Escape;
                }
            }
        }

        public void Escaping(Meepo me, EarthBind earthBind, Poof poof, Hero _globalTarget)
        {
            var handle = me.Handle;
            //bool nh;
            //if (!Variables.NeedHeal.TryGetValue(handle, out nh))
            //    Variables.NeedHeal.Add(handle, false);
            var perc = me.Health / (float)me.MaximumHealth * 100;
            if (!me.HasModifiers(new[] { "modifier_fountain_aura", "modifier_fountain_aura_buff" }, false))
            {
                if (Utils.SleepCheck("move check" + handle))
                {
                    var anyEnemyHero =
                        Heroes.GetByTeam(me.GetEnemyTeam())
                            .FirstOrDefault(x => x.IsAlive && x.IsVisible && x.Distance2D(me) <= 800);
                    if (anyEnemyHero != null)
                    {
                        //var earthBind = Variables.earthBindList[handle];
                        if (earthBind != null && earthBind.CanBeCasted() && !anyEnemyHero.HasModifier("modifier_meepo_earthbind"))
                        {
                            earthBind.CastSpell(anyEnemyHero);
                        }
                    }
                    var anyAllyMeepoNearBase =
                        Variables.MeepoList.Where(
                            x =>
                                !Heroes.GetByTeam(me.GetEnemyTeam()).Any(y => y.Distance2D(x) <= 1500))
                            .OrderBy(z => z.Distance2D(Fountain.GetAllyFountain())).FirstOrDefault();
                    if (anyAllyMeepoNearBase == null)
                    {
                        me.Move(Fountain.GetAllyFountain().Position);
                    }
                    else
                    {
                        if (anyAllyMeepoNearBase == me)
                        {
                            me.Move(Fountain.GetAllyFountain().Position);
                        }
                        else
                        {
                            if (poof.CanBeCasted())
                            {
                                poof.Use(anyAllyMeepoNearBase.Position);
                            }
                            else
                            {
                                me.Move(Fountain.GetAllyFountain().Position);
                            }
                        }
                    }
                    
                    Utils.Sleep(500, "move check" + handle);
                }
            }
        }

    }
}
