using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeepoSharpPlus
{
    public class EarthBind
    {
        private readonly Ability ability;

        private uint Level
        {
            get
            {
                return this.ability.Level;
            }
        }

        public EarthBind(Ability ability)
        {
            this.ability = ability;
        }

        public bool CanBeCasted()
        {
            return this.ability.CanBeCasted();
        }

        public uint CastRange()
        {
            return this.ability.CastRange;
        }

        public double GetHitDelay(Hero target)
        {
            return this.ability.GetHitDelay(target, this.ability.Name);
        }

        public void CastSpell(Hero target)
        {
            if (Utils.SleepCheck("earthbind"))
            {
                this.ability.CastSkillShot(target);
                Utils.Sleep(100, "earthbind");
            }
        }
    }
}
