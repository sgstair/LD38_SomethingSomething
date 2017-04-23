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

        GameEngine Eng;
        GameMap Map;
        MapRenderer DrawMap;

        float viewAngle;

        Vector3 cameraLookAt;
        Vector3 cameraSmoothLookAt;
        Texture2D MapTiles;

        int PlayerIndex;

        public GameContext(Game1 rootGame)
        {
            Parent = rootGame;

            Map = new GameMap(16, 16);
            
            viewAngle = (float)(Math.PI / 4);
        }

        public void SetupGame(ContentManager content, string mapFilename)
        {
            // byte[] map = content.Load<byte[]>(mapFilename); // Todo: look into how to load this with content system.
            byte[] map = System.IO.File.ReadAllBytes(System.IO.Path.Combine(content.RootDirectory, mapFilename));

            Map = GameMap.LoadMapData(map);
            DrawMap = new MapRenderer(Map, MapTiles);
            Eng = new GameEngine(Map, 2);
            cameraLookAt = Eng.SuggestCameraStartLocation;
            cameraSmoothLookAt = cameraLookAt;
            PlayerIndex = 0;
        }


        public void LoadContent(ContentManager Content)
        {
            MapTiles = Content.Load<Texture2D>("tex/Tiles");
            DrawMap = new MapRenderer(Map, MapTiles);
        }

        public void UnloadContent()
        {

        }

        const int TopBarSize = 60;

        const int UiWidth = 400;
        const int DescriptionHeight = 120;
        const int ActionQueueHeight = 90;
        const int Spacing = 5;
        const int Margin = 3;
        const int TextHeight = 27;
        const int CancelWidth = 100;
        const int CancelHeight = 30;

        class Panel
        {
            public float x = Spacing, y, width = UiWidth, height;
            public bool Description, Action, Queue;
            public int index;
            public bool Clicked, CancelPressed, Failed;

            public bool TestHit(float x, float y)
            {
                if (x >= this.x && x < (this.x + width))
                {
                    if (y >= this.y && y < (this.y + height))
                    {
                        return true;
                    }
                }
                return false;
            }

            public bool TestCancelHit(float x, float y)
            {
                float x1 = this.x + width - Margin - CancelWidth;
                float y1 = this.y + height - Margin - CancelHeight;
                if (x >= x1 && x < (x1 + CancelWidth))
                {
                    if (y >= y1 && y < (y1 + CancelHeight))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        ActionDescription[] selActions;
        GameQueuedWork[] selQueue;

        void UpdateSelectionUiData()
        {
            selActions = null;
            selQueue = null;
            if(selectedUnit != null)
            {
                selActions = Eng.EnumerateActionsForUnit(selectedUnit);
            }
            else if(selectedTile != null)
            {
                selActions = Eng.EnumerateActionsForTile(PlayerIndex, selectedTile.Value);
                selQueue = Eng.EnumerateQueueForTile(PlayerIndex, selectedTile.Value);
            }
        }

        IEnumerable<Panel> GetUiPanels()
        {
            if (selectedUnit != null || selectedTile != null)
            {
                float y = TopBarSize + Spacing;
                yield return new Panel() { y = y, height = DescriptionHeight, Description = true };
                y += DescriptionHeight + Spacing;

                if(selActions != null)
                {
                    for(int i=0;i<selActions.Length;i++)
                    {
                        yield return new Panel() { y = y, height = ActionQueueHeight, Action = true, index = i };
                        y += ActionQueueHeight + Spacing;
                    }
                }

                if (selQueue != null)
                {
                    for (int i = 0; i < selQueue.Length; i++)
                    {
                        yield return new Panel() { y = y, height = ActionQueueHeight, Queue = true, index = i };
                        y += ActionQueueHeight + Spacing;
                    }
                }
            }
        }

        Panel GetPanelAtLocation(float x, float y)
        {
            foreach(Panel p in GetUiPanels())
            {
                if (p.TestHit(x, y))
                {
                    p.Clicked = true;
                    p.CancelPressed = p.TestCancelHit(x, y);
                    return p;
                }
            }
            return null;
        }

        void SetPanelFeedback(Panel p, bool success)
        {
            feedbackPanel = new Panel() { y = p.y, height = p.height, Failed = !success };
        }

        Panel feedbackPanel;
        Panel hoverPanel;

        void DrawUiRect(float x, float y, float width, float height, Color c, float expand = 1.0f)
        {
            if(expand != 1)
            {
                float dx = width * expand - width;
                float dy = height * expand - height;
                x -= dx / 2;
                y -= dy / 2;
                width = width * expand;
                height = height * expand;
            }
            int ix, iy, iw, ih;
            ix = (int)Math.Round(x);
            iy = (int)Math.Round(y);
            iw = (int)Math.Round(width);
            ih = (int)Math.Round(height);

            Engine.DrawScreenRect(new Rectangle(ix, iy, iw, ih), c);
        }
        void DrawUiRect(Panel p, Color c, float expand = 1.0f)
        {
            DrawUiRect(p.x, p.y, p.width, p.height, c, expand);
        }

        void RenderUi()
        {
            Color panelBackground = new Color(0, 0, 0, 140); // transparent black
            Color hoverBackground = new Color(128, 128, 128, 140); // transparent gray
            if (feedbackPanel != null)
            {
                float percent = feedbackPanel.index / 15.0f;
                Color c = new Color(0f, 0f, 0f, (1 - percent) * 0.7f);
                if (feedbackPanel.Failed) c.R = 255;
                DrawUiRect(feedbackPanel, c, 1 + percent * 0.3f);
                feedbackPanel.index++;
                if(feedbackPanel.index >= 16) feedbackPanel = null;
            }

            foreach(Panel p in GetUiPanels())
            {
                bool hover = false;
                Color bgColor = panelBackground;
                if(hoverPanel != null)
                {
                    if(hoverPanel.index == p.index && hoverPanel.Description == p.Description && hoverPanel.Action == p.Action && hoverPanel.Queue == p.Queue)
                    {
                        bgColor = hoverBackground;
                        hover = true;
                    }
                }

                DrawUiRect(p, panelBackground);

                if(p.Description)
                {
                    int totalHp = 0;
                    int curHp = 0;
                    string title = "Unknown";
                    if(selectedUnit != null)
                    {
                        // Get unit details
                        bool enemy = selectedUnit.Owner != PlayerIndex;

                    }
                    if(selectedTile != null)
                    {
                        // Get tile details
                        bool enemy = Map[selectedTile.Value].Owner != PlayerIndex;
                        if (!Eng.IsAttackable(selectedTile.Value)) enemy = false;

                        ResourceHarvestingDescription rh = Eng.ResourceDetailsForTile(selectedTile.Value);
                        if(rh != null)
                        {
                            title = rh.ResourceName;
                        }

                        BuildingProperties bp = Eng.StructureDetailsForTile(selectedTile.Value);
                        if(bp != null)
                        {
                            totalHp = bp.HP;
                            curHp = Map[selectedTile.Value].HP;
                            title = bp.Name;
                            if(enemy) { title = "Enemy " + title; }
                        }

                    }
                    // Draw common elements
                    Engine.DrawText(new Vector2(p.x + Margin, p.y + Margin), title, Color.White);

                }
                else if(p.Action)
                {
                    ActionDescription desc = selActions[p.index];
                    Engine.DrawText(new Vector2(p.x + Margin, p.y + Margin), desc.Name, Color.White);
                    Engine.DrawText(new Vector2(p.x + Margin*2, p.y + Margin*2+TextHeight), desc.Description, Color.White, 0.5f);
                    DrawResources(p.x + p.width - Margin, p.y + p.height - Margin - 32, PlayerResources.FromRequirements(desc.Cost), 32, 90, false, true);

                    // time cost
                    string text = ((float)desc.Cost.Ticks / GameEngine.TickRate).ToString("n1") + "s";
                    Vector2 sz = Engine.MeasureString(text);
                    Engine.DrawText(new Vector2(p.x + Margin, p.y + p.height - Margin - 16 - sz.Y / 2), text, Color.White);
                }
                else if(p.Queue)
                {
                    GameQueuedWork w = selQueue[p.index];
                    Engine.DrawText(new Vector2(p.x + Margin, p.y + Margin), w.TaskName, Color.White);
                    if(w.Active)
                    {
                        Engine.DrawText(new Vector2(p.x + Margin*2, p.y + Margin*2+TextHeight), w.TaskProgressText, Color.White);
                        float progressPercent = 0;
                        if(w.RequiredTicks > 0)
                        {
                            progressPercent = (float)w.CompletedTicks / w.RequiredTicks;
                        }
                        float y = (p.y + p.height - Margin - Margin * 4);
                        float x = (p.x + Margin * 2);
                        float wid = (p.width - Margin * 5 - CancelWidth);
                        float barwid = wid * progressPercent;
                        DrawUiRect(x, y, wid, Margin * 2, Color.Gray);
                        DrawUiRect(x, y, barwid, Margin * 2, Color.Green);
                    }
                    else
                    {
                        Engine.DrawText(new Vector2(p.x + Margin * 2, p.y + Margin * 2 + TextHeight), w.TaskProgressText, Color.White);
                    }
                    // Cancel button
                    bool cancelHover = hover && hoverPanel.CancelPressed;
                    {
                        float x = p.x + p.width - Margin * 2 - CancelWidth;
                        float y = p.y + p.height - Margin * 2 - CancelHeight;
                        DrawUiRect(x, y, CancelWidth, CancelHeight, cancelHover?Color.Red:Color.DarkRed);

                        string text = "Cancel";
                        Vector2 v = Engine.MeasureString(text);
                        Engine.DrawText(new Vector2(x + CancelWidth / 2 - v.X / 2, y + CancelHeight / 2 - v.Y / 2), text, Color.White);

                    }
                    if(cancelHover)
                    {
                        // Also show a resource refund popup.
                    }
                }

            }

        }

        void PanelClick(Panel p)
        {
            bool success = true;
            // Attempt to execute the action being clicked
            if(p.Action)
            {
                if(selectedTile != null)
                {
                    // Execute on tile action
                    EngineRequestStatus status = Eng.QueueActionForTile(PlayerIndex, selectedTile.Value, selActions[p.index]);
                    if (status != EngineRequestStatus.Completed) success = false;
                }
                if(selectedUnit != null)
                {
                    // Start process to choose location
                }
            }
            if(p.Queue)
            {
                if (!p.CancelPressed) return; // More strict about location when canceling things.

                // Execute queue cancel
                Eng.CancelQueueElement(PlayerIndex, selQueue[p.index]);
            }
            SetPanelFeedback(p, success);
        }


        // Right aligned
        void DrawResources(float x, float y, PlayerResources res, float size, float elementWidth=120, bool mainBar = true, bool highlightMissing = false)
        {
            // Main bar: Highlight red on near capacity >90%, Display capacity% bar below.
            // Otherwise: Ignore zero elements. Highlight red if HighLightMissing and not enough resources in category.

            for(int i = 3; i >= 0; i--)
            {
                ResourceType t = (ResourceType)i;
                int tileId = 0x104 + i;
                Color textColor = Color.White;
                float barPercent = 0;

                int value = res.GetResource(t);

                if (mainBar == false && value == 0) continue; // Skip empty value

                x -= elementWidth;

                // Choose text color
                if (mainBar)
                {
                    int maxValue = Eng.ResourceLimit[PlayerIndex].GetResource(t);
                    if(maxValue > 0) { barPercent = (float)value / maxValue; }

                    if (barPercent > 0.9f) textColor = Color.Red;
                }
                else
                {
                    if(highlightMissing)
                    {
                        int haveResources = Eng.Resources[PlayerIndex].GetResource(t);
                        if (value > haveResources) textColor = Color.Red;
                    }
                }

                // Draw icon
                DrawMap.DrawTileIcon(tileId, new Point((int)x, (int)y), size);

                // Center text by the icon
                string text = value.ToString();
                if (!mainBar && !highlightMissing) text = "+" + text;
                Vector2 textSize = Engine.MeasureString(text);
                Engine.DrawText(new Vector2(x + size + 5, y + size / 2 - textSize.Y / 2), text, textColor);

                if(mainBar)
                {
                    // Draw resource percent bar underneath.
                    Color baseColor = new Color(0, 0, 128);
                    Color barColor = new Color(64, 64, 255);

                    int ix = (int)Math.Round(x + 5);
                    int iw = (int)Math.Round(x + elementWidth - 5) - ix;
                    int iy = (int)Math.Round(y + size + 5);

                    int barWidth = (int)Math.Round(iw * barPercent);

                    Engine.DrawScreenRect(new Rectangle(ix, iy, iw, 10), baseColor);
                    Engine.DrawScreenRect(new Rectangle(ix, iy, barWidth, 10), barColor);
                }
            }

        }



        void ResetSelection()
        {
            selectedUnit = null;
            selectedTile = null;
            UpdateSelectionUiData();
        }
        void SelectUnit(GameUnit unit)
        {
            selectedUnit = unit;
        }

        void SelectTile(Point tile)
        {
            selectedTile = tile;
        }


        Vector3 mouseLocation;
        GameUnit highlightUnit = null;
        GameUnit selectedUnit = null;
        Point? tileHighlight = null;
        Point? selectedTile = null;

        bool ClickTerrain = false;
        float ClickZ;

        public void Update(GameTime gameTime)
        {
            // Handle some mouse input
            MouseState mouse = Parent.curMouse;
            MouseState lastMouse = Parent.lastMouse;

            Eng.UpdateTime(gameTime.ElapsedGameTime.TotalSeconds);


            if (Parent.IsActive)
            {
                if (mouse.LeftButton != ButtonState.Pressed) ClickTerrain = false;

                mouseLocation = DrawMap.FindIntersectingLocation(new Point(mouse.X, mouse.Y));
                // Recompute highlight
                highlightUnit = null;
                tileHighlight = null;

                // Can we snap to a unit at this location?

                // if no, can we snap to an interesting structure at this location?
                if (highlightUnit == null && DrawMap.IsInsideMap(mouseLocation))
                {
                    Point tileLocation = Map.TileFromLocation(mouseLocation);
                    if (Eng.IsStructure(tileLocation))
                    {
                        tileHighlight = tileLocation;
                    }
                }

                // Get latest data about UI to determine mouse hits
                UpdateSelectionUiData();
                hoverPanel = GetPanelAtLocation(mouse.X, mouse.Y);


                if (Parent.LeftClick())
                {
                    if (hoverPanel != null)
                    {
                        PanelClick(hoverPanel);
                    }
                    else
                    {
                        ClickTerrain = DrawMap.IsInsideMap(mouseLocation);
                        ClickZ = mouseLocation.Z;

                        ResetSelection();
                        if (highlightUnit != null)
                        {
                            SelectUnit(highlightUnit);
                        }
                        else if (tileHighlight != null)
                        {
                            SelectTile(tileHighlight.Value);
                        }
                    }
                }



                // Hold right = camera rotation
                if (Parent.RightHeld())
                {
                    int dx = mouse.X - lastMouse.X;

                    viewAngle += dx * 0.01f;
                    viewAngle = (float)(viewAngle - Math.Floor(viewAngle / Math.PI / 2) * Math.PI * 2);
                }
                if(Parent.LeftHeld() && hoverPanel == null)
                {
                    if(ClickTerrain)
                    {
                        Vector3 prevProj = DrawMap.ProjectOntoPlane(new Point(lastMouse.X, lastMouse.Y), ClickZ);
                        Vector3 curProj = DrawMap.ProjectOntoPlane(new Point(mouse.X, mouse.Y), ClickZ);
                        Vector3 delta = curProj - prevProj;
                        Vector2 newTarget = new Vector2(cameraLookAt.X + delta.X, cameraLookAt.Y + delta.Y);

                        cameraLookAt = Map.SurfaceLocation(newTarget);
                    }
                }
            }
            else
            {
                // When not active, slowly rotate around
                viewAngle += (float)(gameTime.ElapsedGameTime.TotalSeconds/16);
                viewAngle = (float)(viewAngle - Math.Floor(viewAngle / Math.PI / 2) * Math.PI * 2);
            }
            UpdateSelectionUiData();

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
            return cameraSmoothLookAt + new Vector3(CameraForward() * (-viewOut), viewUp);
        }

        void UpdateSmoothCamera()
        {
            float maxMove = 0.5f;
            float cutoff = 0.001f;
            float percentMove = 0.2f;

            float distance = (cameraSmoothLookAt - cameraLookAt).Length();
            if(distance < cutoff)
            {
                cameraSmoothLookAt = cameraLookAt;
                return;
            }
            if(distance * percentMove > maxMove)
            {
                percentMove = maxMove / distance;
            }
            cameraSmoothLookAt = cameraLookAt * percentMove + cameraSmoothLookAt * (1 - percentMove);
        }




        public void Draw(GameTime gameTime)
        {
            
            UpdateSmoothCamera();

            DrawMap.SetCameraLocation(CameraLocation(), cameraSmoothLookAt);


            DrawMap.DrawMap();


            // Drawing transparent layers in a specific order for proper layering
            if(selectedUnit != null)
            {
                bool enemy = selectedUnit.Owner != PlayerIndex;
                DrawMap.DrawSelectionCursor(mouseLocation, 0.5f, enemy?Color.Red:Color.Green);
            }

            if(tileHighlight != null)
            {
                // Render later, but prevent rendering other selection cues.
            }
            else if(highlightUnit != null)
            {
                bool enemy = highlightUnit.Owner != PlayerIndex;
                DrawMap.DrawSelectionCursor(Map.SurfaceLocation(highlightUnit.Location), 0.5f, enemy ? Color.Pink : Color.LightGreen);

            }
            else if(DrawMap.IsInsideMap(mouseLocation))
            {
                DrawMap.DrawSelectionCursor(mouseLocation, 0.3f, Color.Blue, 0.005f);
            }

            if (tileHighlight == selectedTile && tileHighlight != null)
            {
                DrawMap.DrawTileHighlight(tileHighlight.Value.X, tileHighlight.Value.Y);
            }
            else
            {
                if (tileHighlight != null)
                {
                    DrawMap.DrawTileHighlight(tileHighlight.Value.X, tileHighlight.Value.Y);
                }
                if (selectedTile != null)
                {
                    Color greenHighlight = new Color(128, 255, 128, 127);
                    Color redHighlight = new Color(255, 128, 128, 127);
                    bool enemy = Map[selectedTile.Value].Owner != PlayerIndex;
                    if (!Eng.IsAttackable(selectedTile.Value)) enemy = false; // Resource structures are not enemies.
                    DrawMap.DrawTileHighlight(selectedTile.Value.X, selectedTile.Value.Y, enemy?redHighlight:greenHighlight);
                }
            }

            DrawMap.DrawBillboards();

            // Disable Z-buffering and draw the UI on top
            Engine.g.DepthStencilState = DepthStencilState.None;

            // Render top resource bar
            Engine.DrawScreenRect(new Rectangle(0, 0, Engine.g.Viewport.Width, TopBarSize), new Color(0, 0, 0, 192));
            DrawResources(Engine.g.Viewport.Width - 5, 5, Eng.Resources[PlayerIndex], 32);

            // Render selected info panel / actions / queue
            RenderUi();

            //DrawMap.DrawTileIcon(0x106, new Point(100, 100), 200);

        }
    }
}
