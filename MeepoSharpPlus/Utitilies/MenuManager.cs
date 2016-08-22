using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Extensions.Damage;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using Ensage.Heroes;
using SharpDX;

namespace MeepoSharpPlus.Utitilies
{
    public class MenuManager
    {
        public Menu Menu { get; private set; }

        public readonly MenuItem ComboMenu;

        public readonly MenuItem PoofToSelectedMenu;

        public readonly Menu EscapeMenu;

        public readonly MenuItem EscapeHotkey = new MenuItem("hotkey.Escape", "Escape for selected Meepo(s)");

        public readonly MenuItem EscapeHealthMenu;

        public readonly MenuItem EscapeHealthPercentageMenu;

        public readonly Menu LanePushMenu;

        public readonly MenuItem LanePushHotkey = new MenuItem("AutoPush.Enable", "Push Lane By Selected Meepo(s)");

        public readonly Menu JungleFarmMenu;

        public readonly MenuItem JungleFarmHotkey = new MenuItem("JungleFarm.Enable", "Jungle Farm By Selected Meepo(s)");

        public readonly Menu StackJungleMenu;

        public readonly MenuItem StackJungleHotkey = new MenuItem("JungleStack.Enable", "Jungle Stacking By Selected Meepo(s)");

        public readonly Menu DrawingMenu;

        public MenuManager(string heroName)
        {
            this.Menu = new Menu("MeepoSharpPlus", "meeporez", true, "npc_dota_hero_meepo", true);
            // Lane Menu
            this.LanePushMenu = new Menu("LanePushMenu", "LanePushMenu", false, "npc_dota_hero_meepo", true);         
            this.LanePushMenu.AddItem(this.LanePushHotkey.SetValue(new KeyBind('Z',KeyBindType.Press)));
            this.LanePushMenu.AddItem(new MenuItem("AutoPush.EscapeRange", "Range For Escape").SetValue(new Slider(1500, 0, 5000)));
            this.LanePushMenu.AddItem(new MenuItem("AutoPush.AutoW", "use w for lasthitting").SetValue(true));
            this.LanePushMenu.AddItem(new MenuItem("AutoPush.TravelBoots", "use TravelBoots").SetValue(false));
            this.LanePushMenu.AddItem(new MenuItem("AutoPush.LastHitMode", "Try to last hit creeps").SetValue(true));
            this.LanePushMenu.AddItem(new MenuItem("AutoPush.Enable", "Push Lane By Selected Meepo(s)").SetValue(new KeyBind('Z',
                    KeyBindType.Press)));

            //
            // Jungle Menu
            this.JungleFarmMenu = new Menu("JungleFarmMenu", "JungleFarmMenu", false, "npc_dota_hero_meepo", true);             
            this.JungleFarmMenu.AddItem(this.JungleFarmHotkey.SetValue(new KeyBind('X', KeyBindType.Press)));
            this.JungleFarmMenu.AddItem(new MenuItem("JungleFarm.EscapeFromAnyEnemyHero", "Escape From Enemy Hero in selected range").SetValue(true));
            this.JungleFarmMenu.AddItem(new MenuItem("JungleFarm.EscapeRange", "Range For Escape").SetValue(new Slider(1500, 0, 5000)));
            this.JungleFarmMenu.AddItem(new MenuItem("JungleFarm.AutoW", "use w for farming").SetValue(true));
            this.JungleFarmMenu.AddItem(new MenuItem("JungleFarm.TeamCheck", "Farm enemy jungle too").SetValue(false));
            this.JungleFarmMenu.AddItem(new MenuItem("JungleFarm.Ancient", "Farm ancients too").SetValue(false));
            
            
            //
            // Stack Menu
            
            this.StackJungleMenu = new Menu("StackJungleMenu", "StackJungleMenu", false, "npc_dota_hero_meepo", true);
            this.StackJungleMenu.AddItem(this.StackJungleHotkey.SetValue(new KeyBind('C', KeyBindType.Press)));
            this.StackJungleMenu.AddItem(new MenuItem("JungleStack.EscapeFromAnyEnemyHero", "Escape From Enemy Hero in selected range").SetValue(true));
            this.StackJungleMenu.AddItem(new MenuItem("JungleStack.EscapeRange", "Range For Escape").SetValue(new Slider(1500, 0, 5000)));
            this.StackJungleMenu.AddItem(new MenuItem("JungleStack.TeamCheck", "Stack enemy jungle too").SetValue(false));
            this.StackJungleMenu.AddItem(new MenuItem("JungleStack.Ancient", "Stack ancients too").SetValue(false));
            //
            // Escape Menu            
            this.EscapeMenu = new Menu("EscapeMenu", "EscapeMenu", false, "npc_dota_hero_meepo", true);
            this.EscapeMenu.AddItem(this.EscapeHotkey.SetValue(new KeyBind('V', KeyBindType.Press)));
            this.EscapeMenu.AddItem(new MenuItem("Escape.MinRange", "Min health for auto escape").SetValue(new Slider(300, 0, 4000)));
            this.EscapeMenu.AddItem(new MenuItem("Escape.MinRangePercent", "Min health for auto escape (%)").SetValue(new Slider(15, 0, 100)));


            // Drawing Menu
            this.DrawingMenu = new Menu("DrawingMenu", "DrawingMenu", false, "npc_dota_hero_meepo", true);
            this.DrawingMenu.AddItem(new MenuItem("Drawing.DamageFromPoof", "Draw Poof count on enemy").SetValue(true));
            this.DrawingMenu.AddItem(new MenuItem("Drawing.NumOfMeepo", "Draw Number for each meepo").SetValue(true));
            this.DrawingMenu.AddItem(new MenuItem("Drawing.NumOfMeepoOnMinimap", "Draw Number for each meepo on minimap").SetValue(true));
            this.DrawingMenu.AddItem(new MenuItem("Drawing.NumOfMeepoInMenu", "Draw Number for each meepo in OverlayMenu").SetValue(true));

            
            this.ComboMenu = new MenuItem("hotkey", "Hotkey").SetValue(new KeyBind('D', KeyBindType.Press));
            this.PoofToSelectedMenu = new MenuItem("hotkey.PoofAll", "Poof all to selected meepo").SetValue(new KeyBind('W', KeyBindType.Press));

            this.Menu.AddSubMenu(this.StackJungleMenu);
            this.Menu.AddSubMenu(this.EscapeMenu);
            this.Menu.AddSubMenu(this.DrawingMenu);
            this.Menu.AddSubMenu(this.LanePushMenu);
            this.Menu.AddSubMenu(this.JungleFarmMenu);
            this.Menu.AddItem(this.ComboMenu);
            this.Menu.AddItem(this.PoofToSelectedMenu);
            this.Menu.AddItem(new MenuItem("LockTarget", "LockTarget").SetValue(true));
            //this.Menu.AddToMainMenu();
        }

        public bool LanePushPressed
        {
            get
            {
                return this.LanePushHotkey.GetValue<KeyBind>().Active;
            }
        }

        public bool EscapePressed
        {
            get
            {
                return this.EscapeHotkey.GetValue<KeyBind>().Active;
            }
        }

        public bool JunglePressed
        {
            get
            {
                return this.JungleFarmHotkey.GetValue<KeyBind>().Active;
            }
        }

        public bool StackPressed
        {
            get
            {
                return this.StackJungleHotkey.GetValue<KeyBind>().Active;
            }
        }

        public bool ComboPressed
        {
            get
            {
                return this.ComboMenu.GetValue<KeyBind>().Active;
            }
        }

        public bool PoofSelectedPressed
        {
            get
            {
                return this.PoofToSelectedMenu.GetValue<KeyBind>().Active;
            }
        }


    }
}
    
