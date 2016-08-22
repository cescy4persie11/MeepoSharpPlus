using Ensage;
using Ensage.Heroes;
using MeepoSharpPlus.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeepoSharpPlus.Features
{
    public class MeepoSettings
    {
        public Meepo Hero { get; set; }
        public uint Handle { get; set; }
        public bool MainMenu { get; set; }

        public bool IsAlive
        {
            get { return Hero.IsAlive; }
        }
        public Variables.OrderState CurrentOrderState
        {
            get
            {
                var handle = Hero.Handle;
                return Variables.OrderStates[handle];
            }
            set { }
        }

        public EarthBind earthBind
        {
            get { return Variables.earthBindList[Hero.Handle]; }
        }
        public Poof poof
        {
            get { return Variables.poofList[Hero.Handle]; }
        }

        public int Id { get; set; }

        public MeepoSettings(Meepo meepo)
        {
            Hero = meepo;
            Handle = meepo.Handle;
            MainMenu = false;
            CurrentOrderState = Variables.OrderStates[Handle];
            Id = (byte)(Variables.MeepoSet.Count + 1);
            Game.PrintMessage("Init new Meepo: " + string.Format("Menu: {0}; CurrentOderState: {1}; Id:{2} ;", MainMenu, CurrentOrderState, Id), MessageType.ChatMessage);
        }
    }
}
