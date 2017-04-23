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
    class MapEditorContext : IGameContext
    {
        Game1 Parent;

        GameMap Map;
        MapRenderer DrawMap;

        float viewAngle;

        Vector3 cameraLookAt;
        Texture2D MapTiles;

        UiSystem ui = new UiSystem();
        Random r = new Random();

        enum EditorState
        {
            Move,
            Up,
            Down,
            Tool
        }

        EditorState CurrentState = EditorState.Move;
        TileType SelectedTool = TileType.Land;

        public MapEditorContext(Game1 rootGame)
        {
            Parent = rootGame;

            Map = new GameMap(16, 16);
            
            viewAngle = (float)(Math.PI / 4);
        }


        public void LoadContent(ContentManager Content)
        {
            MapTiles = Content.Load<Texture2D>("tex/Tiles");
            DrawMap = new MapRenderer(Map, MapTiles);
            MakeButtons();
        }

        float buttony;
        void MakeButtons()
        {
            ui.Reset();
            buttony = 50;
            AddButton("Load", ClickLoad);
            AddButton("Save", ClickSave);
            AddButton("Move", ClickMove);
            AddButton("Up", ClickUp);
            AddButton("Down", ClickDown);
            for(int i=0;i<(int)TileType.Tech;i++)
            {
                TileType t = (TileType)i;
                AddButton(t.ToString(), ClickType);
            }
        }
        void AddButton(string text, UiButton.UiClick callback)
        {
            UiButton btn = new UiButton(text);
            btn.y = buttony;
            buttony += btn.height + 5;
            btn.Click += callback;
            ui.AddButton(btn);
        }

        const string MapName = "SavedMap.mp";

        void ClickLoad(UiButton btn)
        {
            if (System.IO.File.Exists(MapName))
            {
                byte[] mapData = System.IO.File.ReadAllBytes(MapName);
                try
                {
                    Map = GameMap.LoadMapData(mapData);
                    DrawMap = new MapRenderer(Map, MapTiles);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Print("Exception reading map file: " + ex.ToString());
                }
            }
        }
        void ClickSave(UiButton btn)
        {
            byte[] mapData = Map.SaveMapData();
            System.IO.File.WriteAllBytes(MapName, mapData);
        }
        void ClickMove(UiButton btn)
        {
            CurrentState = EditorState.Move;
        }
        void ClickUp(UiButton btn)
        {
            CurrentState = EditorState.Up;
        }
        void ClickDown(UiButton btn)
        {
            CurrentState = EditorState.Down;
        }
        void ClickType(UiButton btn)
        {
            CurrentState = EditorState.Tool;
            SelectedTool = (TileType)Enum.Parse(typeof(TileType), btn.buttonText);
        }

        public void UnloadContent()
        {

        }

        Point? tileHighlight = null;


        HashSet<Point> AffectedTiles = new HashSet<Point>();

        

        void ApplyTool()
        {
            if(tileHighlight.HasValue)
            {
                if(!AffectedTiles.Contains(tileHighlight.Value))
                {
                    AffectedTiles.Add(tileHighlight.Value);
                    GameMapTile t = Map.Tiles[tileHighlight.Value.X, tileHighlight.Value.Y];
                    switch (CurrentState)
                    {
                        case EditorState.Up:
                            if(t.Level != Map.MaxLayer) t.Level++;
                            break;
                        case EditorState.Down:
                            if (t.Level != 0) t.Level--;
                            break;
                        case EditorState.Tool:
                            t.Content = SelectedTool;
                            t.Rotation = (byte)r.Next(4);
                            t.Variation = (byte)r.Next(Map.AlternateCount(t.Content));
                            break;
                    }
                    
                    Map.Tiles[tileHighlight.Value.X, tileHighlight.Value.Y] = t;
                }
            }
        }

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

                    tileHighlight = DrawMap.FindIntersectingPoint(cameraLoc, forward - up * ty + right * tx * Engine.AspectRatio);
                }

                if (Parent.LeftClick())
                {
                    AffectedTiles.Clear();
                    ui.DidClick(mouse.X, mouse.Y);
                }

                if (Parent.LeftHeld())
                {
                    if (CurrentState == EditorState.Move || Parent.KeyDown(Keys.LeftShift))
                    {
                        int dx = mouse.X - lastMouse.X;
                        int dy = mouse.Y - lastMouse.Y;

                        cameraLookAt += new Vector3(CameraForward() * dy * 0.02f, 0);
                        cameraLookAt -= new Vector3(CameraRight() * dx * 0.02f, 0);
                    }
                    else
                    {
                        if(!ui.TestHit(mouse.X, mouse.Y))
                        {
                            ApplyTool();
                        }
                    }
                }

                foreach (Keys k in Parent.HeldKeys())
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
            
            DrawMap.SetCameraLocation(CameraLocation(), cameraLookAt);



            DrawMap.DrawMap();

            // Draw UI stuff
            if(tileHighlight != null)
            {
                DrawMap.DrawTileHighlight(tileHighlight.Value.X, tileHighlight.Value.Y);
            }

            // Finish with billboards
            DrawMap.DrawBillboards();


            // top level UI stuff.
            if(tileHighlight != null)
            {
                int x = tileHighlight.Value.X;
                int y = tileHighlight.Value.Y;
                string text = string.Format("({0},{1},{2}): {3}", x, y, Map[x, y].Level, Map[x, y].Content);
                Engine.DrawText(new Vector2(200, 10), text, Color.Black);
            }
            // Inform user about current state of editor
            {
                string text = "";
                switch(CurrentState)
                {
                    case EditorState.Move: text = "(Move)"; break;
                    case EditorState.Up: text = "(Raise Terrain)"; break;
                    case EditorState.Down: text = "(Lower Terrain)"; break;
                    case EditorState.Tool: text = "(Tool: " + SelectedTool.ToString() + ")"; break;
                }
                Engine.DrawText(new Vector2(10, 10), text, Color.Black);
            }

            ui.Render();
        }
    }
}
