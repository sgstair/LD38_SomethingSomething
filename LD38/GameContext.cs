using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD38
{
    /// <summary>
    /// Owns the input pipeline and rendering pipelien when in the game context.
    /// </summary>
    class GameContext
    {
        Game1 Parent;

        GameMap Map;
        MapRenderer DrawMap;

        float viewAngle;

        Vector3 cameraLookAt;
        Texture2D MapTiles;

        public GameContext(Game1 rootGame)
        {
            Parent = rootGame;

            Map = new GameMap(16, 16);
            
            viewAngle = (float)(Math.PI / 4);
        }


        public void LoadContent(ContentManager Content)
        {
            MapTiles = Content.Load<Texture2D>("tex/Tiles");
            DrawMap = new MapRenderer(Map, MapTiles);
        }

        public void UnloadContent()
        {

        }

        Point? tileHighlight = null;


        public void Update(GameTime gameTime)
        {
            // Handle some mouse input
            MouseState mouse = Parent.curMouse;
            MouseState lastMouse = Parent.lastMouse;


            if (Parent.IsActive)
            {

                if (Parent.RightClick())
                {
                    int dx = mouse.X - lastMouse.X;

                    viewAngle += dx * 0.01f;
                    viewAngle = (float)(viewAngle - Math.Floor(viewAngle / Math.PI / 2) * Math.PI * 2);
                }
                if (Parent.LeftClick())
                {
                    int dx = mouse.X - lastMouse.X;
                    int dy = mouse.Y - lastMouse.Y;

                    cameraLookAt += new Vector3(CameraForward() * dy * 0.02f, 0);
                    cameraLookAt += new Vector3(CameraRight() * dx * 0.02f, 0);

                }

                {
                    // Attempt to mouse-select a tile by raytracing.
                    int height = Engine.g.PresentationParameters.BackBufferHeight;
                    int width = Engine.g.PresentationParameters.BackBufferWidth;
                    float tx = (float)(mouse.X - width / 2) / (width / 2);
                    float ty = (float)(mouse.Y - height / 2) / (height / 2);
                    Vector3 cameraLoc = CameraLocation();
                    Vector3 forward = cameraLookAt - cameraLoc;
                    forward.Normalize();
                    Vector3 right = Vector3.Cross(Vector3.UnitZ, forward);
                    Vector3 up = Vector3.Cross(forward, right);
                    right.Normalize();
                    up.Normalize();

                    tileHighlight = DrawMap.FindIntersectingPoint(cameraLoc, forward - up * ty - right * tx * Engine.AspectRatio);
                }



                foreach (Keys k in Parent.PressedKeys())
                {

                    // Do some basic map editing
                    if (tileHighlight == null) continue;
                    GameMapTile t = Map.Tiles[tileHighlight.Value.X, tileHighlight.Value.Y];
                    switch (k)
                    {
                        case Keys.Up:
                            if (t.Level != Map.MaxLayer) t.Level++;
                            break;
                        case Keys.Down:
                            if (t.Level != 0) t.Level--;
                            break;

                        case Keys.D1: t.Content = TileType.Land; break;
                        case Keys.D2: t.Content = TileType.Ramp; break;
                        case Keys.D3: t.Content = TileType.Water; break;
                        case Keys.D4: t.Content = TileType.Bridge; break;
                        case Keys.D5: t.Content = TileType.Forest; break;
                        case Keys.D6: t.Content = TileType.Mine; break;
                        case Keys.D7: t.Content = TileType.Storage; break;
                        case Keys.D8: t.Content = TileType.Center; break;
                        case Keys.D9: t.Content = TileType.House; break;
                        case Keys.D0: t.Content = TileType.Turret; break;

                    }
                    Map.Tiles[tileHighlight.Value.X, tileHighlight.Value.Y] = t;
                }
            }
            else
            {
                // When not active, slowly rotate around
                viewAngle += (float)(gameTime.ElapsedGameTime.TotalSeconds/16);
                viewAngle = (float)(viewAngle - Math.Floor(viewAngle / Math.PI / 2) * Math.PI * 2);
            }

        }

        Vector2 CameraForward()
        {
            float vx = (float)Math.Cos(viewAngle);
            float vy = (float)Math.Sin(viewAngle);
            return new Vector2(vx, vy);
        }
        Vector2 CameraRight()
        {
            Vector2 v = CameraForward();
            return new Vector2(-v.Y, v.X);
        }

        Vector3 CameraLocation()
        {
            float viewOut = 6;
            float viewUp = 8;
            return cameraLookAt + new Vector3(CameraForward() * (-viewOut), viewUp);
        }


        public void Draw(GameTime gameTime)
        {
            Engine.MatPerspective = Matrix.CreatePerspectiveFieldOfView((float)(Math.PI / 2), Engine.AspectRatio, 0.5f, 30);
            Engine.MatWorld = Matrix.Identity;
            Vector3 cameraLoc = CameraLocation();
            Engine.MatView = Matrix.CreateLookAt(cameraLoc, cameraLookAt, Vector3.UnitZ);
            DrawMap.SetCameraLocation(cameraLoc, cameraLookAt);

            BlendState bs = new BlendState();

            bs.ColorBlendFunction = BlendFunction.Add;
            bs.ColorSourceBlend = Blend.SourceAlpha;
            bs.ColorDestinationBlend = Blend.InverseSourceAlpha;
            
            Engine.g.BlendState = bs;


            DrawMap.DrawMap();

            if(tileHighlight != null)
            {
                DrawMap.DrawTileHighlight(tileHighlight.Value.X, tileHighlight.Value.Y);
            }

            DrawMap.DrawBillboards();


        }
    }
}
