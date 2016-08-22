using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Extensions.Damage;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using Ensage.Heroes;
using MeepoSharpPlus.Utitilies;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MeepoSharpPlus.Features
{
    public class Stack
    {
        public Stack()
        {

        }

        private List<Meepo> meepoList
        {
            get
            {
                return Variables.MeepoList;
            }
        }

        public void Execute()
        {
            foreach (var m in meepoList.Where(x => Variables.OrderStates[x.Handle] == Variables.OrderState.Stacking))
            {
                StackJungle(m);
            }
        }

        public void StackJungle(Hero me)
        {
            var s = Utitilies.JungleCamps.FindClosestCampForStacking(me, Variables.MenuManager.StackJungleMenu.Item("JungleStack.TeamCheck").GetValue<bool>(),
                Variables.MenuManager.StackJungleMenu.Item("JungleStack.Ancient").GetValue<bool>());
            var enemyHeroes = Heroes.GetByTeam(me.GetEnemyTeam()).Where(x => x.IsAlive).ToList();
            var dist = Variables.MenuManager.StackJungleMenu.Item("JungleStack.EscapeRange").GetValue<Slider>().Value;
            if (Variables.MenuManager.StackJungleMenu.Item("JungleStack.EscapeFromAnyEnemyHero").GetValue<bool>() &&
                    enemyHeroes.Any(x => x.Distance2D(me) <= dist)) //escape from hero
            {
                var handle = me.Handle;
                Variables.OrderStates[handle] = Variables.OrderState.Escape;
                Variables.NeedHeal[handle] = true;
            }
            if (s == null) return;
            s.stacking = me;
            var set = Variables.MeepoSet.Find(x => Equals(x.Hero, me));
            var name = set.Handle.ToString();
            var sec = Game.GameTime % 60;
            var timeForStart = s.WaitPosition.Distance2D(s.CampPosition) / me.MovementSpeed;
            var time = s.StackTime - timeForStart - sec;
            //Print("Current Time: [" + sec + "] Time For Travel: [" + timeForStart + "] TimeForStartMoving: [" + (time - sec) + "]");
            //Print(time.ToString());
            if (time >= 0.5)
            {
                if (Utils.SleepCheck("move_cd2" + name))
                {
                    me.Move(s.WaitPosition);
                    Utils.Sleep(250, "move_cd2" + name);
                }
            }
            else if (Utils.SleepCheck("move_cd" + name))
            {
                var pos = s.CampPosition;
                var ang = me.FindAngleBetween(pos, true);
                var p = new Vector3((float)(pos.X - 80 * Math.Cos(ang)),
                    (float)(pos.Y - 80 * Math.Sin(ang)), 0);
                me.Move(p);
                me.Move(s.StackPosition, true);
                Utils.Sleep((60 - s.StackTime) * 1000 + 8000, "move_cd" + name);
            }
        }
    }
}
