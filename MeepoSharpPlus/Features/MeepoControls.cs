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
using System.Globalization;
using MeepoSharpPlus.Abilities;

namespace MeepoSharpPlus.Features
{
    public class MeepoControls
    {
        //provide features to do smart controls and switch modes for each meepo.
        public MeepoControls()
        {

        }

        private static List<Entity> SelectedMeepo
        {
            get
            {
                return Variables.SelectedMeepo;
            }
        }

        private Hero me
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


        private Dictionary<uint, Variables.OrderState> orderState
        {
            get
            {
                return Variables.OrderStates;
            }
        }

        private Dictionary<uint, Variables.OrderState> lastOrderState
        {
            get
            {
                return Variables.LastOrderStates;
            }
        }

        private static  EarthBind earthBind;

        private static Poof poof;

        private static readonly Vector2 IconPos = new Vector2(120, 71);

        private const int IconSize = 82;

        private static readonly bool[] MenuIsOpen = new bool[10];

        private static int SelectedId;

        private static bool _leftMouseIsPress;

        public void MeepoModeSwitch()
        {
            RefreshMeepoList();
            if (Variables.LanePushPressed)
            {
                if (Utils.SleepCheck("button_cd"))
                {

                    Console.WriteLine("Lane Push switched");
                    Console.WriteLine("Selected Meepo is " + SelectedMeepo.Select(x => x.Handle).Count());
                    foreach (var handle in SelectedMeepo.Select(x => x.Handle))
                    {
                        if (Variables.OrderStates[handle] == Variables.OrderState.Laning)
                            Variables.OrderStates[handle] = Variables.OrderState.Idle;
                        else
                            Variables.OrderStates[handle] = Variables.OrderState.Laning;
                        Console.WriteLine("mode is " + Variables.OrderStates[handle].ToString());
                    }
                    Utils.Sleep(150, "button_cd");
                
                }
            }
            if (Variables.JunglePressed)
            {
                if (Utils.SleepCheck("button_cd"))
                {
                    foreach (var handle in SelectedMeepo.Select(x => x.Handle))
                    {
                        if (Variables.OrderStates[handle] == Variables.OrderState.Jungle)
                            Variables.OrderStates[handle] = Variables.OrderState.Idle;
                        else
                            Variables.OrderStates[handle] = Variables.OrderState.Jungle;
                    }
                    Utils.Sleep(150, "button_cd");

                }
            }
            if (Variables.StackPressed)
            {
                if (Utils.SleepCheck("button_cd"))
                {
                    foreach (var me in SelectedMeepo)
                    {
                        var handle = me.Handle;
                        if (Variables.OrderStates[handle] == Variables.OrderState.Stacking)
                        {
                            Variables.OrderStates[handle] = Variables.OrderState.Idle;
                        }
                        else
                            Variables.OrderStates[handle] = Variables.OrderState.Stacking;
                    }
                    Utils.Sleep(150, "button_cd");

                }
            }

            if (Variables.EscapePressed)
            {
                if (Utils.SleepCheck("button_cd"))
                {
                    foreach (var handle in SelectedMeepo.Select(me => me.Handle))
                    {
                        if (Variables.OrderStates[handle] == Variables.OrderState.Escape)
                            Variables.OrderStates[handle] = Variables.OrderState.Idle;
                        else
                        {
                            Variables.NeedHeal[handle] = true;
                            Variables.OrderStates[handle] = Variables.OrderState.Escape;
                        }
                    }
                    Utils.Sleep(150, "button_cd");
                }
            }




        }

        public void RefreshMeepoList()
        {
            // update selected meepo
            if (Utils.SleepCheck("SelectChecker"))
            {
                Variables.SelectedMeepo = ObjectManager.LocalPlayer.Selection.Where(x => x.ClassID == ClassID.CDOTA_Unit_Hero_Meepo).ToList();
            }
            // update meepolist
            if (!Utils.SleepCheck("MeepoRefresh")) return;
            Utils.Sleep(500, "MeepoRefresh");
            
            Variables.MeepoList =
                ObjectManager.GetEntities<Meepo>()
                    .Where(x => x.IsValid && !x.IsIllusion() && x.Team == me.Team).ToList(); /*.OrderBy(x => x.Handle)*/
            //if (MeepoList.Count >= 1 + me.Spellbook.Spell4.Level + (me.AghanimState() ? 1 : 0)) return;
            foreach (var meepo in MeepoList)
            {
                var handle = meepo.Handle;
                Variables.OrderState state;
                Variables.OrderState Laststate;
                if (!Variables.OrderStates.TryGetValue(handle, out state))
                {
                    Variables.OrderStates.Add(handle, Variables.OrderState.Idle);
                }
                if (!Variables.LastOrderStates.TryGetValue(handle, out Laststate))
                {
                    Variables.LastOrderStates.Add(handle, Variables.OrderState.Idle);
                }
                //Ability q, w;
                if (!Variables.earthBindList.TryGetValue(handle, out earthBind))
                    Variables.earthBindList[handle] = new EarthBind(meepo.Spellbook.Spell1);
                if (!Variables.poofList.TryGetValue(handle, out poof))
                    Variables.poofList[handle] = new Poof(meepo.Spellbook.Spell2);
                foreach (var m in MeepoList.Where(m => !Variables.MeepoSet.Any(x => Equals(x.Hero, meepo))))
                {
                    Variables.MeepoSet.Add(new MeepoSettings(m));
                }
            }
        }

        public void DrawNumOfMeepos()
        {
            if (Variables.MenuManager.DrawingMenu.Item("Drawing.NumOfMeepoInMenu").GetValue<bool>())
            {
                var count = MeepoList.Count();
                
                foreach (var meepo in MeepoList)
                {

                    var handle = meepo.Handle;
                    count--;

                    var sizeY = HUDInfo.GetHpBarSizeY();
                    var pos = IconPos + new Vector2(0, IconSize) * count;
                    var textSize = Drawing.MeasureText((count + 1).ToString(CultureInfo.InvariantCulture), "Arial",
                        new Vector2((float)(sizeY * 3), 100), FontFlags.AntiAlias);
                    var textPos = pos - new Vector2(sizeY / 2 + textSize.Y, 0);
                    Drawing.DrawText(
                        (count + 1).ToString(CultureInfo.InvariantCulture),
                        textPos - new Vector2(72, 0),
                        new Vector2((float)(sizeY * 3), 100),
                        Color.White,
                        FontFlags.AntiAlias | FontFlags.StrikeOut);
                    DrawButton(IconPos + new Vector2(0, IconSize) * count, 70, 30, ref MenuIsOpen[count],
                        new Color(0, 155, 255, 150), new Color(0, 0, 0, 100), Variables.OrderStates[handle].ToString(),
                        SelectedMeepo.Contains(meepo));
                
                    
                    if (MenuIsOpen[count])
                    {
                        SelectedId = 0;
                        DrawButton(IconPos + new Vector2(0, IconSize) * count + new Vector2(70, 0), 70, 30, ref SelectedId, 1,
                            new Color(0, 0, 0, 100),
                            Variables.OrderState.Idle.ToString());
                        DrawButton(IconPos + new Vector2(0, IconSize) * count + new Vector2(70, 30), 70, 30,
                            ref SelectedId, 2,
                            new Color(0, 0, 0, 100),
                            Variables.OrderState.Jungle.ToString());
                        DrawButton(IconPos + new Vector2(0, IconSize) * count + new Vector2(140, 0), 70, 30,
                            ref SelectedId, 3,
                            new Color(0, 0, 0, 100),
                            Variables.OrderState.Stacking.ToString());
                        DrawButton(IconPos + new Vector2(0, IconSize) * count + new Vector2(140, 30), 70, 30,
                            ref SelectedId, 4,
                            new Color(0, 0, 0, 100),
                            Variables.OrderState.Laning.ToString());
                        DrawButton(IconPos + new Vector2(0, IconSize) * count + new Vector2(71 + 70 / 2, 60), 70, 30,
                            ref SelectedId, 5,
                            new Color(0, 0, 0, 100),
                            Variables.OrderState.Escape.ToString());
                        if (SelectedId != 0)
                        {
                            Variables.OrderStates[handle] = (Variables.OrderState)SelectedId - 1;
                            MenuIsOpen[count] = false;
                            if (Variables.OrderStates[handle] == Variables.OrderState.Escape)
                            {
                                Variables.NeedHeal[handle] = true;
                            }
                        }
                    }
                    if (Variables.MenuManager.DrawingMenu.Item("Drawing.NumOfMeepo").GetValue<bool>())
                    {
                        var w2SPos = HUDInfo.GetHPbarPosition(meepo);
                        if (w2SPos.X > 0 && w2SPos.Y > 0)
                        {
                            var sizeX = HUDInfo.GetHPBarSizeX();
                            sizeY = HUDInfo.GetHpBarSizeY();
                            var text = count + 1;
                            textSize = Drawing.MeasureText(text.ToString(CultureInfo.InvariantCulture), "Arial",
                                new Vector2((float)(sizeY * 3), 100), FontFlags.AntiAlias);
                            textPos = w2SPos + new Vector2(sizeY / 2 + textSize.Y, 0);
                            Drawing.DrawText(
                                text.ToString(CultureInfo.InvariantCulture),
                                textPos + new Vector2(0, -50),
                                new Vector2((float)(sizeY * 3), 100),
                                Color.White,
                                FontFlags.AntiAlias | FontFlags.StrikeOut);
                        }
                    }
                    if (Variables.MenuManager.DrawingMenu.Item("Drawing.NumOfMeepoOnMinimap").GetValue<bool>())
                    {
                        var w2SPos = WorldToMinimap(meepo.NetworkPosition);
                        sizeY = HUDInfo.GetHpBarSizeY();
                        var text = count + 1;
                        Drawing.DrawText(
                            text.ToString(CultureInfo.InvariantCulture),
                            w2SPos + new Vector2(-5, -33),
                            new Vector2((float)(sizeY * 3), 100),
                            Color.White,
                            FontFlags.AntiAlias | FontFlags.StrikeOut);
                    }
                    
                }
            }

        }

        private static Vector2 WorldToMinimap(Vector3 pos)
        {
            const float MapLeft = -8000;
            const float MapTop = 7350;
            const float MapRight = 7500;
            const float MapBottom = -7200;
            var MapWidth = Math.Abs(MapLeft - MapRight);
            var MapHeight = Math.Abs(MapBottom - MapTop);

            var _x = pos.X - MapLeft;
            var _y = pos.Y - MapBottom;

            float dx, dy, px, py;
            if (Math.Round((float)Drawing.Width / Drawing.Height, 1) >= 1.7)
            {
                dx = 272f / 1920f * Drawing.Width;
                dy = 261f / 1080f * Drawing.Height;
                px = 11f / 1920f * Drawing.Width;
                py = 11f / 1080f * Drawing.Height;
            }
            else if (Math.Round((float)Drawing.Width / Drawing.Height, 1) >= 1.5)
            {
                dx = 267f / 1680f * Drawing.Width;
                dy = 252f / 1050f * Drawing.Height;
                px = 10f / 1680f * Drawing.Width;
                py = 11f / 1050f * Drawing.Height;
            }
            else
            {
                dx = 255f / 1280f * Drawing.Width;
                dy = 229f / 1024f * Drawing.Height;
                px = 6f / 1280f * Drawing.Width;
                py = 9f / 1024f * Drawing.Height;
            }
            var MinimapMapScaleX = dx / MapWidth;
            var MinimapMapScaleY = dy / MapHeight;

            var scaledX = Math.Min(Math.Max(_x * MinimapMapScaleX, 0), dx);
            var scaledY = Math.Min(Math.Max(_y * MinimapMapScaleY, 0), dy);

            var screenX = px + scaledX;
            var screenY = Drawing.Height - scaledY - py;

            return new Vector2((float)Math.Floor(screenX), (float)Math.Floor(screenY));

        }

        private bool CursorOnMinimap()
        {
            return (Game.MouseScreenPosition.X > 0 && Game.MouseScreenPosition.X <= 300) && (Game.MouseScreenPosition.Y > 800 && Game.MouseScreenPosition.Y <= 1077);
        }

        private static void DrawButton(Vector2 a, float w, float h, ref bool clicked, Color @on, Color off,
            string drawOnButtonText = "", bool isSelected = false)
        {
            var isIn = Utils.IsUnderRectangle(Game.MouseScreenPosition, a.X, a.Y, w, h);
            if (_leftMouseIsPress && Utils.SleepCheck("ClickButtonCd") && isIn)
            {
                Console.WriteLine("clicked? " + _leftMouseIsPress);
                clicked = !clicked;
                Utils.Sleep(250, "ClickButtonCd");
            }
            var newColor = isIn
                ? new Color((int)(clicked ? @on.R : off.R), clicked ? @on.G : off.G, clicked ? @on.B : off.B, 150)
                : clicked ? @on : off;
            var textSize = Drawing.MeasureText(
                drawOnButtonText,
                "Arial",
                new Vector2((float)(h * .50), 100),
                FontFlags.AntiAlias);
            var textPos = a + new Vector2(5, (float)((h * 0.5) - (textSize.Y * 0.5)));
            Drawing.DrawRect(a, new Vector2(w, h), newColor);
            Drawing.DrawText(
                drawOnButtonText,
                textPos,
                new Vector2((float)(h * .50), 100),
                Color.White,
                FontFlags.AntiAlias | FontFlags.Additive | FontFlags.Custom);
            if (isSelected)
            {
                Drawing.DrawRect(a, new Vector2(w, h), Color.YellowGreen, true);
            }
        }

        private static void DrawButton(Vector2 a, float w, float h, ref int id, int needed, Color @on,
            string drawOnButtonText = "")
        {
            var isIn = Utils.IsUnderRectangle(Game.MouseScreenPosition, a.X, a.Y, w, h);
            if (_leftMouseIsPress && Utils.SleepCheck("ClickButtonCd") && isIn)
            {
                id = needed;
                Utils.Sleep(250, "ClickButtonCd");
            }
            var newColor = @on;
            var textSize = Drawing.MeasureText(
                drawOnButtonText,
                "Arial",
                new Vector2((float)(h * .50), 100),
                FontFlags.AntiAlias);
            var textPos = a + new Vector2(5, (float)((h * 0.5) - (textSize.Y * 0.5)));
            Drawing.DrawRect(a, new Vector2(w, h), newColor);
            Drawing.DrawRect(a, new Vector2(w, h), Color.Black, true);
            Drawing.DrawText(
                drawOnButtonText,
                textPos,
                new Vector2((float)(h * .50), 100),
                Color.White,
                FontFlags.AntiAlias | FontFlags.Additive | FontFlags.Custom);
        }

        public void Game_OnWndProc(WndEventArgs args)
        {
            if (args.WParam != 1 || Game.IsChatOpen)
            {
                _leftMouseIsPress = false;
                return;
            }
            _leftMouseIsPress = true;      
        }

        public void Player_OnExecuteAction(Player sender, ExecuteOrderEventArgs args)
        {
            if (Variables.Hero == null || !Variables.Hero.IsValid || !Variables.Hero.IsAlive) return;
            var me = sender.Selection.First();
            if (args.Order == Order.MoveLocation)
            {
                foreach (var m in Variables.MeepoSet.Where(m => SelectedMeepo.Contains(m.Hero)))
                {                  
                    if(m != null)
                    {
                        if(m.CurrentOrderState != Variables.OrderState.Escape)
                            Variables.OrderStates[m.Handle] = Variables.OrderState.Idle;
                    }
                }
            }

            if (args.Ability == null) return;
            
            if(args.Ability.Name.Equals("meepo_poof"))
            {
                if (Utils.SleepCheck("poof"))
                {
                    foreach (var m in Variables.SelectedMeepo)
                    {
                        if (CursorOnMinimap())
                        {
                            if (Variables.poofList[m.Handle].CanBeCasted())
                            {
                                var ClosestMeepoToMiniMap = Variables.MeepoList.OrderBy(x => Game.MouseScreenPosition.Distance(WorldToMinimap(x.NetworkPosition))).DefaultIfEmpty(null).FirstOrDefault();
                                Variables.poofList[m.Handle].UseForAllMeepos(ClosestMeepoToMiniMap);
                            }
                        }
                        else
                        {
                            if (Variables.poofList[m.Handle].CanBeCasted())
                            {
                                Variables.poofList[m.Handle].UseForAllMeepos(Game.MousePosition);
                            }
                        }

                        
                    }
                    Utils.Sleep(100, "poof");
                }
            }

            
        }


    }
}
