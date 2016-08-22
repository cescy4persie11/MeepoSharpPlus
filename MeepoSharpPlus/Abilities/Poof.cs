using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeepoSharpPlus.Abilities
{
    public class Poof
    {
        private readonly Ability ability;

        public uint Level
        {
            get
            {
                return this.ability.Level;
            }
        }

        public Poof(Ability ability)
        {
            this.ability = ability;
        }

        public bool CanBeCasted()
        {
            return this.ability.CanBeCasted();
        }

        public float GetRealCastRange()
        {
            var range = ability.CastRange;
            if (range >= 1) return range;
            var data =
                ability.AbilitySpecialData.FirstOrDefault(
                    x => x.Name.Contains("radius") || (x.Name.Contains("range") && !x.Name.Contains("ranged")));
            if (data == null) return range;
            var level = ability.Level == 0 ? 0 : ability.Level - 1;
            range = (uint)(data.Count > 1 ? data.GetValue(level) : data.Value);
            return range;
        }

        public void Use(dynamic Target)
        {
            if (Utils.SleepCheck("poof"))
            {
                this.ability.UseAbility(Target);
                Utils.Sleep(100, "poof");
            }
        }

        public void UseForAllMeepos(dynamic Target)
        {
            this.ability.UseAbility(Target);
        }

        
    }
}
