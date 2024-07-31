using System;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.GameSystems.TextSurfaceScripts;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;
using VRageRender;
using Digi;
using System.Collections.Generic;

namespace PEPCO
{
    // Text Surface Scripts (TSS) can be selected in any LCD's scripts list.
    // These are meant as fast no-sync (sprites are not sent over network) display scripts, and the Run() method only executes player-side (no DS).
    // You can still use a session comp and access it through this to use for caches/shared data/etc.
    //
    // The display name has localization support aswell, same as a block's DisplayName in SBC.
    [MyTextSurfaceScript("NavigationScreenTSS", "Navigation Screen")]
    public class NavigationScreenTSS : MyTSSCommon
    {
        public override ScriptUpdate NeedsUpdate => ScriptUpdate.Update10; // frequency that Run() is called.

        private readonly IMyTerminalBlock TerminalBlock;
        
        RectangleF _viewport;
        bool zoomed = false;

        public NavigationScreenTSS(IMyTextSurface surface, IMyCubeBlock block, Vector2 size) : base(surface, block, size)
        {
            TerminalBlock = (IMyTerminalBlock)block; // internal stored m_block is the ingame interface which has no events, so can't unhook later on, therefore this field is required.
            TerminalBlock.OnMarkForClose += BlockMarkedForClose; // required if you're gonna make use of Dispose() as it won't get called when block is removed or grid is cut/unloaded.

            // Called when script is created.
            // This class is instanced per LCD that uses it, which means the same block can have multiple instances of this script aswell (e.g. a cockpit with all its screens set to use this script).
        }

        public override void Dispose()
        {
            base.Dispose(); // do not remove
            TerminalBlock.OnMarkForClose -= BlockMarkedForClose;

            // Called when script is removed for any reason, so that you can clean up stuff if you need to.
        }

        void BlockMarkedForClose(IMyEntity ent)
        {
            Dispose();
        }

        // gets called at the rate specified by NeedsUpdate
        // it can't run every tick because the LCD is capped at 6fps anyway.
        public override void Run()
        {
            try
            {
                base.Run(); // do not remove

                //Test if the block fits the required gamelogic
                if (TerminalBlock?.GameLogic?.GetAs<NavigationScreenLogic>() != null) Draw();
                else throw new Exception("Oh noes an error :}");


                MatrixD camWM = MyAPIGateway.Session.Camera.WorldMatrix;

                const double MaxDistance = 15;

                //If distance between camera and block is more than 15 meters return
                if (Vector3D.Distance(camWM.Translation, TerminalBlock.GetPosition()) > MaxDistance) return;

                
                //Proceeds only if the player is looking at the screen
                if (MyAPIGateway.Input.IsKeyPress(VRage.Input.MyKeys.PageDown))
                {
                    var logic = TerminalBlock?.GameLogic?.GetAs<NavigationScreenLogic>();
                    if (logic != null)
                        logic.NavigationScreenZoom++;
                }
                else if (MyAPIGateway.Input.IsKeyPress(VRage.Input.MyKeys.PageUp))
                {
                    var logic = TerminalBlock?.GameLogic?.GetAs<NavigationScreenLogic>();
                    if (logic != null)
                        logic.NavigationScreenZoom--;
                }
                else if (MyAPIGateway.Input.IsKeyPress(VRage.Input.MyKeys.End))
                {
                    var logic = TerminalBlock?.GameLogic?.GetAs<NavigationScreenLogic>();
                    if (logic != null)
                        logic.NavigationScreenZoom = 1;
                }
                else if (MyAPIGateway.Input.IsKeyPress(VRage.Input.MyKeys.Home))
                {
                    var logic = TerminalBlock?.GameLogic?.GetAs<NavigationScreenLogic>();
                    if (logic != null)
                        logic.NavigationScreenZoom = 10;
                }


            }
            catch (Exception e) // no reason to crash the entire game just for an LCD script, but do NOT ignore them either, nag user so they report it :}
            {
                DrawError(e);
            }
        }

        void Draw() // this is a custom method which is called in Run().
        {

            // Calculate the viewport offset by centering the surface size onto the texture size
            _viewport = new RectangleF(
                (Surface.TextureSize - Surface.SurfaceSize) / 2f,
                Surface.SurfaceSize
            );

            Vector2 screenSize = Surface.SurfaceSize;
            Vector2 screenCorner = (Surface.TextureSize - screenSize) * 0.5f;

            var logic = TerminalBlock?.GameLogic?.GetAs<NavigationScreenLogic>(); // get the gamelogic comp from the block

            if (logic == null) return; // if the block doesn't have the comp, don't draw anything

            double lonFraction = logic.longitudeFraction;
            double latFraction = logic.latitudeFraction;

            double NavigationScreenChevronScale = logic.NavigationScreenChevronScale;

            Color NavigationScreenChevronColor = logic.NavigationScreenChevronColor;

            double heading = logic.heading;

            string mapName = logic.mapName;

            float zoomMultiplier = TerminalBlock?.GameLogic?.GetAs<NavigationScreenLogic>()?.NavigationScreenZoom ?? 1;
            //Log.Info($"Multiplier: {zoomMultiplier}");

            // was            Vector2 offsetVector = new Vector2(1 + (float)lonFraction, 1 + (float)latFraction);
            Vector2 offsetVector = new Vector2(1 + (float)lonFraction * zoomMultiplier, 1 + (float)latFraction * zoomMultiplier);

            var frame = Surface.DrawFrame();

            // Drawing sprites works exactly like in PB API.
            // Therefore this guide applies: https://github.com/malware-dev/MDK-SE/wiki/Text-Panels-and-Drawing-Sprites

            // there are also some helper methods from the MyTSSCommon that this extends.
            // like: AddBackground(frame, Surface.ScriptBackgroundColor); - a grid-textured background

            // the colors in the terminal are Surface.ScriptBackgroundColor and Surface.ScriptForegroundColor, the other ones without Script in name are for text/image mode.






            // Create background sprite
            var sprite = new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Data = mapName,
                Position = _viewport.Center * offsetVector,
                Size = screenSize * zoomMultiplier,
                Color = Color.White.Alpha(1f),
                Alignment = TextAlignment.CENTER
            };

            



            var spriteLeft = sprite;
            spriteLeft.Position -= new Vector2(screenSize.X * zoomMultiplier, 0);
            var spriteRight = sprite;
            spriteRight.Position += new Vector2(screenSize.X * zoomMultiplier, 0);
            

            var spriteLeftUp = sprite;
            spriteLeftUp.Position -= new Vector2(screenSize.X/2 * zoomMultiplier, screenSize.Y * zoomMultiplier);
            spriteLeftUp.Size = new Vector2(screenSize.X, -screenSize.Y) * zoomMultiplier;
            var spriteRightUp = sprite;
            spriteRightUp.Position += new Vector2(screenSize.X * zoomMultiplier, -screenSize.Y * zoomMultiplier);
            spriteRightUp.Size = new Vector2(screenSize.X, -screenSize.Y) * zoomMultiplier;
            var spriteLeftDown = sprite;
            spriteLeftDown.Position -= new Vector2(screenSize.X/2 * zoomMultiplier, -screenSize.Y * zoomMultiplier);
            spriteLeftDown.Size = new Vector2(screenSize.X, -screenSize.Y) * zoomMultiplier;
            var spriteRightDown = sprite;
            spriteRightDown.Position += new Vector2(screenSize.X / 2 * zoomMultiplier, screenSize.Y * zoomMultiplier);
            spriteRightDown.Size = new Vector2(screenSize.X, -screenSize.Y) * zoomMultiplier;

            frame.Add(sprite);
            frame.Add(spriteLeft);
            frame.Add(spriteRight);
            frame.Add(spriteLeftUp);
            frame.Add(spriteRightUp);
            frame.Add(spriteLeftDown);
            frame.Add(spriteRightDown);


            //Add a sprite with the width of the screen and 5% of the height
            var mapMarker = new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Data = "Triangle",
                Position = _viewport.Center,
                Size = new Vector2(25,50) * (float)NavigationScreenChevronScale,
                RotationOrScale = (float)heading,                //Rotate the sprite by the heading
                Color = NavigationScreenChevronColor,
                Alignment = TextAlignment.CENTER
            };
            frame.Add(mapMarker);

            AddBackground(frame, Color.Black);

            frame.Dispose(); // send sprites to the screen

            zoomed = false;
        }

        void DrawError(Exception e)
        {
            MyLog.Default.WriteLineAndConsole($"{e.Message}\n{e.StackTrace}");

            try // first try printing the error on the LCD
            {
                Vector2 screenSize = Surface.SurfaceSize;
                Vector2 screenCorner = (Surface.TextureSize - screenSize) * 0.5f;

                var frame = Surface.DrawFrame();

                var bg = new MySprite(SpriteType.TEXTURE, "SquareSimple", null, null, Color.Black);
                frame.Add(bg);

                var text = MySprite.CreateText($"ERROR: {e.Message}\n{e.StackTrace}\n\nPlease send screenshot of this to mod author.\n{MyAPIGateway.Utilities.GamePaths.ModScopeName}", "White", Color.Red, 0.7f, TextAlignment.LEFT);
                text.Position = screenCorner + new Vector2(16, 16);
                frame.Add(text);

                frame.Dispose();
            }
            catch (Exception e2)
            {
                MyLog.Default.WriteLineAndConsole($"Also failed to draw error on screen: {e2.Message}\n{e2.StackTrace}");

                if (MyAPIGateway.Session?.Player != null)
                    MyAPIGateway.Utilities.ShowNotification($"[ ERROR: {GetType().FullName}: {e.Message} | Send SpaceEngineers.Log to mod author ]", 10000, MyFontEnum.Red);
            }
        }
    }
}