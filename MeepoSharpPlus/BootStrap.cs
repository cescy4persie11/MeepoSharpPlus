using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common;


namespace MeepoSharpPlus
{
    public class BootStrap
    {
        private MeepoSharpPlus meepoSharpPlus;

        public BootStrap()
        {
            this.meepoSharpPlus = new MeepoSharpPlus();
        }

        public void SubscribeEvents()
        {
            Events.OnLoad += this.Events_Onload;
            Events.OnClose += this.Events_OnClose;
            //Events.OnUpdate += this.Events_OnUpdate;
            //Game.OnUpdate += this.Game_OnUpdate_JungleFarm;
            Game.OnUpdate += this.Game_OnUpdate_Controls;
            Game.OnWndProc += this.Game_OnWndProc;
            Drawing.OnDraw += this.Drawing_OnDraw;
            //Drawing.OnDraw += this.Drawing_OnDraw;
            Player.OnExecuteOrder += this.Player_OnExecuteOrder;
        }


        private void Drawing_OnDraw(EventArgs args)
        {
            //throw new NotImplementedException();
            this.meepoSharpPlus.OnDraw();
        }

        private void Game_OnUpdate_Controls(EventArgs e)
        {
            //throw new NotImplementedException();
            this.meepoSharpPlus.Game_OnUpdate_Controls();
            this.meepoSharpPlus.Game_OnUpdate_Features();
            this.meepoSharpPlus.Game_OnUpdate_PoofAll();
            this.meepoSharpPlus.Game_OnUpdate_Combo();
            //this.meepoSharpPlus.Camp_Update(e); ;

        }


        private void Events_OnClose(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            this.meepoSharpPlus.OnClose();
        }

        private void Events_Onload(object sender, EventArgs e)
        {
            this.meepoSharpPlus.OnLoad();
        }

        private void Game_OnWndProc(WndEventArgs args)
        {
            this.meepoSharpPlus.OnWndProc(args);
        }

        private void Player_OnExecuteOrder(Player sender, ExecuteOrderEventArgs args)
        {
            if (sender.Equals(ObjectManager.LocalPlayer))
            {
                this.meepoSharpPlus.Player_OnExecuteOrder(sender, args);
            }
        }
    }
}
