using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ensage;
using MeepoSharpPlus.Utitilies;
using Ensage.Heroes;
using MeepoSharpPlus.Features;
using MeepoSharpPlus.Abilities;

namespace MeepoSharpPlus
{
    public static class Variables
    {
        public static Team EnemyTeam { get; set; }

        public static Hero Hero { get; set; }

        public static List<Meepo> MeepoList { get; set; }

        public static List<Entity> SelectedMeepo { get; set; }

        //public static EarthBind earthBind;

        //public static Poof poof;

        public static MenuManager MenuManager { get; set; }

        public static float TickCount
        {
            get
            {
                return Environment.TickCount & int.MaxValue;
            }
        }

        public static bool ComboPressed
        {
            get
            {
                return MenuManager.ComboPressed;
            }
        }

        public static bool EscapePressed
        {
            get
            {
                return MenuManager.EscapePressed;
            }
        }

        public static bool StackPressed
        {
            get
            {
                return MenuManager.StackPressed;
            }
        }

        public static bool JunglePressed
        {
            get
            {
                return MenuManager.JunglePressed;
            }
        }

        public static bool LanePushPressed
        {
            get
            {
                return MenuManager.LanePushPressed;
            }
        }

        public static bool PoofSelectedPressed
        {
            get
            {
                return MenuManager.PoofSelectedPressed;
            }
        }

        public static readonly Dictionary<uint, OrderState> OrderStates = new Dictionary<uint, OrderState>();
        public static readonly Dictionary<uint, OrderState> LastOrderStates = new Dictionary<uint, OrderState>();
        public static readonly Dictionary<uint, bool> NeedHeal = new Dictionary<uint, bool>();

        public static readonly List<MeepoSettings> MeepoSet = new List<MeepoSettings>();

        public static readonly Dictionary<uint, EarthBind> earthBindList = new Dictionary<uint, EarthBind>();
        public static readonly Dictionary<uint, Poof> poofList = new Dictionary<uint, Poof>();
        //private static readonly Dictionary<uint, ParticleEffect> Effects = new Dictionary<uint, ParticleEffect>();

        public enum OrderState
        {
            Idle,
            Jungle,
            Stacking,
            Laning,
            Escape,
            InCombo
        }
    }
}
