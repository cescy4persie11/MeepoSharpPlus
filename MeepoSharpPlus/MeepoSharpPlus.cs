using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage;
using MeepoSharpPlus.Utitilies;
using Ensage.Heroes;
using MeepoSharpPlus.Features;

namespace MeepoSharpPlus
{
    class MeepoSharpPlus
    {
        private bool pause;


        private static Hero Me
        {
            get
            {
                return Variables.Hero;
            }
        }

        private static List<Meepo> MeepoList
        {
            get
            {
                return Variables.MeepoList;
            }
        }


        public MeepoSharpPlus()
        {

        }

        private static Hero _globalTarget;

        private MeepoControls meepoControls;

        private JungleFarm jungleFarm;

        private Escape escape;

        private Laning laning;

        private PoofAllToClosestToMouse poofAllToCloestToMouse;

        private Combo combo;

        private Stack stack;

        public void OnDraw()
        {
            if (this.pause || Variables.Hero == null || !Variables.Hero.IsValid || !Variables.Hero.IsAlive)
            {
                return;
            }
            meepoControls.DrawNumOfMeepos();
        }

        public void OnClose()
        {
            this.pause = true;
            if (Variables.MenuManager != null)
            {
                Variables.MenuManager.Menu.RemoveFromMainMenu();
            }
            
        }

        public void OnLoad()
        {
            Variables.Hero = ObjectManager.LocalHero;
            this.pause = Variables.Hero.ClassID != ClassID.CDOTA_Unit_Hero_Meepo;
            if (this.pause) return;
            Variables.MenuManager = new MenuManager(Me.Name);
            Variables.MenuManager.Menu.AddToMainMenu();
            Variables.EnemyTeam = Me.GetEnemyTeam();
            this.meepoControls = new MeepoControls();
            this.jungleFarm = new JungleFarm();
            this.stack = new Stack();
            this.escape = new Escape();
            this.laning = new Laning();
            this.combo = new Combo();
            this.poofAllToCloestToMouse = new PoofAllToClosestToMouse();
            foreach (var camp in JungleCamps.GetCamps)
            {
                camp.canBeHere = true;
            }
            Game.PrintMessage(
                "MeepoSharpPlus" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version + " loaded",
                MessageType.LogMessage);

        }

        public void OnWndProc(WndEventArgs args)
        {
            if (this.pause || Variables.Hero == null || !Variables.Hero.IsValid || !Variables.Hero.IsAlive)
            {
                return;
            }
            //Console.WriteLine("pressed " + args.WParam);
            meepoControls.Game_OnWndProc(args);
        }

        public void Game_OnUpdate_Controls()
        {
            if (this.pause || Variables.Hero == null || !Variables.Hero.IsValid || !Variables.Hero.IsAlive)
            {
                return;
            }
            meepoControls.RefreshMeepoList();
            meepoControls.MeepoModeSwitch();
        }

        public void Game_OnUpdate_Features()
        {
            
            if (this.pause || Variables.Hero == null || !Variables.Hero.IsValid || !Variables.Hero.IsAlive)
            {
                return;
            }
			jungleFarm.Camp_Update();
            jungleFarm.Execute();
            stack.Execute();
            escape.Execute(null);
            laning.Execute();
        }

        public void Game_OnUpdate_PoofAll()
        {
            if (this.pause || Variables.Hero == null || !Variables.Hero.IsValid || !Variables.Hero.IsAlive)
            {
                return;
            }

            if (!Variables.PoofSelectedPressed) return;
            poofAllToCloestToMouse.Execute(null);

        }


        public void Game_OnUpdate_Combo()
        {
            if (this.pause || Variables.Hero == null || !Variables.Hero.IsValid || !Variables.Hero.IsAlive)
            {
                return;
            }

            if (!Variables.ComboPressed) return;

            if ((_globalTarget != null && !_globalTarget.IsAlive))
            {
                _globalTarget = null;
                combo.FlushEffect();
                return;
            }

            if (_globalTarget == null || !_globalTarget.IsValid || !Variables.MenuManager.Menu.Item("LockTarget").GetValue<bool>())
            {
                _globalTarget = Me.ClosestToMouseTarget(300);
            }
            if (!Me.IsAlive) return;
            if (_globalTarget == null || !_globalTarget.IsValid || !_globalTarget.IsAlive) return;
            combo.Execute(_globalTarget);
        }

        

        public void Player_OnExecuteOrder(Player sender, ExecuteOrderEventArgs args)
        {

            if (this.pause || Variables.Hero == null || !Variables.Hero.IsValid || !Variables.Hero.IsAlive)
            {
                return;
            }
            
            meepoControls.Player_OnExecuteAction(sender, args);
        }


    }
}
