using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class PoofAllToClosestToMouse
    {
        public PoofAllToClosestToMouse()
        {

        }

        private List<Meepo> meepoList
        {
            get
            {
                return Variables.MeepoList;
            }
        }

        public void Execute(Hero target)
        {
            var ClosestMeepoToMouse = Variables.MeepoList.Where(x => x.IsAlive && Variables.OrderStates[x.Handle] != Variables.OrderState.Escape
                                                      && x.Distance2D(Game.MousePosition) <= 1000)
                                                      .OrderBy(y => y.Distance2D(Game.MousePosition)).DefaultIfEmpty(null).FirstOrDefault();
            if (ClosestMeepoToMouse == null) return;
            if (target == null)
            {
                if (Utils.SleepCheck("poofall"))
                {

                    // all others meepos 
                    foreach (var m in Variables.MeepoList.Where(x => !x.Equals(ClosestMeepoToMouse) && Variables.OrderStates[x.Handle] != Variables.OrderState.Escape))
                    {
                        if (Variables.poofList[m.Handle].CanBeCasted())
                        {
                            Variables.poofList[m.Handle].UseForAllMeepos(ClosestMeepoToMouse);
                        }

                    }
                    Utils.Sleep(100, "poofall");
                }
            }
            else
            {
                if (Utils.SleepCheck("move"))
                {
                    ClosestMeepoToMouse.Attack(target);
                    Utils.Sleep(100, "move");
                }
                if (Utils.SleepCheck("poofall"))
                {
                    if (CursorOnMinimap())
                    {
                        var ClosestMeepoToMiniMap = Variables.MeepoList.OrderBy(x => Game.MouseScreenPosition.Distance(WorldToMinimap(x.NetworkPosition))).DefaultIfEmpty(null).FirstOrDefault();
                        foreach (var m in Variables.MeepoList.Where(x => !x.Equals(ClosestMeepoToMiniMap)))
                        {
                            if (Variables.poofList[m.Handle].CanBeCasted())
                            {
                                Variables.poofList[m.Handle].UseForAllMeepos(ClosestMeepoToMiniMap);
                            }
                        }
                    }
                    else {
                        foreach (var m in Variables.MeepoList.Where(x => !x.Equals(ClosestMeepoToMouse)))
                        {
                            if (Variables.poofList[m.Handle].CanBeCasted())
                            {
                                Variables.poofList[m.Handle].UseForAllMeepos(ClosestMeepoToMouse);
                            }
                        }
                    }
                        
                    
                    Utils.Sleep(100, "poofall");
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
    }
}
