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
    public class Combo
    {
        public Combo()
        {

        }

        private static readonly Dictionary<uint, ParticleEffect> Effects = new Dictionary<uint, ParticleEffect>();

        public void Execute(Hero target)
        {
            Update(Variables.Hero);
            var theClosestMeepo = Variables.MeepoList.OrderBy(target.Distance2D).First();
            var dist = theClosestMeepo.Distance2D(target) + Variables.Hero.HullRadius + target.HullRadius;
            var targetPos = target.Position;
            if (Variables.OrderStates[Variables.Hero.Handle] != Variables.OrderState.Escape)
            {
                if (blink != null && blink.CanBeCasted() && dist <= 1150 && dist >= 250 && Utils.SleepCheck("Blink"))
                {
                    blink.UseAbility(targetPos);
                    Utils.Sleep(250, "Blink");
                }
                var bkb = target.FindItem("item_black_king_bar");
                if (bkb != null && bkb.CanBeCasted() && hex != null && hex.CanBeCasted(target) &&
                    Utils.SleepCheck("hex"))
                {
                    hex.UseAbility(target);
                    Utils.Sleep(250, "hex");
                }
                if (orchid != null && orchid.CanBeCasted(target) && !target.IsHexed() && Utils.SleepCheck("orchid") &&
                    Utils.SleepCheck("hex"))
                {
                    orchid.UseAbility(target);
                    Utils.Sleep(250, "orchid");
                }
                if (hex != null && hex.CanBeCasted(target) && !target.IsSilenced() && Utils.SleepCheck("hex") &&
                    Utils.SleepCheck("orchid"))
                {
                    hex.UseAbility(target);
                    Utils.Sleep(250, "hex");
                }
                if (eb != null && eb.CanBeCasted(target) && Utils.SleepCheck("eb"))
                {
                    eb.UseAbility(target);
                    Utils.Sleep(250, "eb");
                }
            }

            foreach (
                var handle in
                    Variables.MeepoList.Where(x => x.IsAlive && Variables.OrderStates[x.Handle] != Variables.OrderState.Escape)
                        .Select(meepo => meepo.Handle))
            {
                Variables.OrderStates[handle] = Variables.OrderState.InCombo;
            }
            foreach (var meepo in Variables.MeepoList.Where(x => x.IsAlive && Variables.OrderStates[x.Handle] == Variables.OrderState.InCombo).OrderBy(y => y.Distance2D(target)))
            {
                DrawEffects(meepo, target);
                //closest meepo Q or attack
                CastEarthBind(target, meepo);
                // Cast Poof
                var w = Variables.poofList[meepo.Handle];
                var castRange = Variables.poofList[meepo.Handle].GetRealCastRange();
                var handle = meepo.Handle;
                var mod = target.FindModifier("modifier_meepo_earthbind");
                var remTime = mod != null ? mod.RemainingTime : 0;
                
                if ((!Equals(theClosestMeepo, meepo) || target.IsHexed() || target.IsStunned() ||
                     target.MovementSpeed <= 200 || (remTime > 1.3)) && w.CanBeCasted() &&
                    dist <= castRange &&
                    Utils.SleepCheck("Period_w" + handle))
                {
                    w.UseForAllMeepos(theClosestMeepo);
                    Utils.Sleep(1500, "Period_w" + handle);
                }
                if (!Utils.SleepCheck("Period_w" + handle))
                {
                    if (dist >= castRange + 150)
                    {
                        Utils.Sleeps.Remove("Period_w" + handle);
                        meepo.Stop();
                    }
                }

                if (Utils.SleepCheck("attack_rate" + handle))
                {
                    Utils.Sleep(250, "attack_rate" + handle);
                    if (!target.IsVisible)
                    {
                        meepo.Move(Prediction.InFront(target, 250));
                    }
                    else
                    {
                        meepo.Attack(target);
                    }
                }
            }
        }

        private void Update(Hero me)
        {
            if (blink == null || !blink.IsValid)
            {
                blink = me.FindItem("item_blink");
            }

            if (eb == null || !eb.IsValid)
            {
                eb = me.FindItem("item_ethereal_blade");
            }
            if (hex == null || !hex.IsValid)
            {
                hex = me.FindItem("item_sheepstick");
            }
            if (orchid == null || !orchid.IsValid)
            {
                orchid = me.FindItem("item_orchid");
            }
            if (aghainim == null || !aghainim.IsValid)
            {
                aghainim = me.FindItem("item_ultimate_scepter");
            }


        }

        private static Item blink, meka, aghainim, hex, orchid, eb;

        private void CastEarthBind(Hero target, Meepo m)
        {
            var mod = target.FindModifier("modifier_meepo_earthbind");
            var remTime = mod != null ? mod.RemainingTime : 0;
            var dist = m.Distance2D(target) + m.HullRadius + target.HullRadius;
            var q = Variables.earthBindList[m.Handle];
            if (q != null && q.CanBeCasted() && dist <= q.CastRange() &&
                (mod == null || remTime <= .7) &&
                Utils.SleepCheck("Period_q"))
            {
                q.CastSpell(target);
                Utils.Sleep(q.GetHitDelay(target) * 1000 + 100, "Period_q");
            }
        }

        public void DrawEffects(Meepo meepo, Hero target)
        {
            ParticleEffect effect;
            var handle = meepo.Handle;
            if (!Effects.TryGetValue(handle, out effect))
            {
                Effects.Add(handle, new ParticleEffect(@"particles\ui_mouseactions\range_finder_tower_aoe.vpcf", target));
            }
            if (effect == null) return;
            effect.SetControlPoint(2, new Vector3(meepo.Position.X, meepo.Position.Y, meepo.Position.Z));
            effect.SetControlPoint(6, new Vector3(1, 0, 0));
            effect.SetControlPoint(7, new Vector3(target.Position.X, target.Position.Y, target.Position.Z));
        }

        public void FlushEffect()
        {
            foreach (var meepo in Variables.MeepoList)
            {
                ParticleEffect effect;
                var handle = meepo.Handle;
                if (Variables.OrderStates[handle] == Variables.OrderState.InCombo) Variables.OrderStates[handle] = Variables.OrderState.Idle;
                if (!Effects.TryGetValue(handle, out effect)) continue;
                effect.Dispose();
                Effects.Remove(handle);
            }
        }
    }
}
