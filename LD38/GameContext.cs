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
    class GameContext : IGameContext
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

                if (Parent.RightHeld())
                {
                    int dx = mouse.X - lastMouse.X;

                    viewAngle += dx * 0.01f;
                    viewAngle = (float)(viewAngle - Math.Floor(viewAngle / Math.PI / 2) * Math.PI * 2);
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
            Engine.MatPerspective = Matrix.CreatePerspectiveFieldOfView((float)(Math.PI / 2), Engine.AspectRatio, 0.5f, 30) * Matrix.CreateScale(-1, 1, 1);
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
