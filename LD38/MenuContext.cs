using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD38
{
    class MenuContext : IGameContext
    {
        Game1 Parent;

        UiSystem ui = new UiSystem();

        public MenuContext(Game1 rootGame)
        {
            Parent = rootGame;
        }

        string exceptionText = "";
        const string MapName = "SavedMap.mp";
        float buttony;
        void SetupButtons()
        {
            ui.Reset();
            buttony = Engine.g.Viewport.Height / 3;
            AddButton("Start Game", ClickStartGame);
            if(System.IO.File.Exists(MapName))
            {
                AddButton("Start Saved Map", ClickStartMap);
            }
            AddButton("Map Editor", ClickMapEditor);
            AddButton("Exit", ClickExit);
        }
        void AddButton(string text, UiButton.UiClick callback)
        {
            UiButton b = new UiButton(text);
            b.y = buttony;
            b.x = (Engine.g.Viewport.Width - b.width) / 2;

            buttony += b.height + 15;
            b.Click += callback;
            ui.AddButton(b);
        }

        void ClickStartGame(UiButton b)
        {
            Parent.StartGame();
        }
        void ClickStartMap(UiButton b)
        {
            try
            {
                byte[] mapData = System.IO.File.ReadAllBytes(MapName);
                Parent.StartGameSavedMap(mapData);
            }
            catch(Exception ex)
            {
                exceptionText = "Map failed to launch\n" + ex.ToString();
            }
        }
        void ClickMapEditor(UiButton b)
        {
            Parent.StartMapEditor();
        }
        void ClickExit(UiButton b)
        {
            Parent.Exit();
        }


        public void LoadContent(ContentManager Content)
        {
            SetupButtons();
        }

        public void UnloadContent()
        {

        }

        public void Update(GameTime gameTime)
        {
            if (Parent.LeftClick())
            {
                ui.DidClick(Parent.lastMouse.X, Parent.lastMouse.Y);
            }
        }

        public void Draw(GameTime gameTime)
        {
            string title = "Something Something";
            string subtitle = "LudumDare 38 Compo entry by sgstair";

            Vector2 size = Engine.MeasureString(title);
            float scale = (Engine.g.Viewport.Width - 200) / size.X;
            float scale2 = (Engine.g.Viewport.Height / 3 - 70) / size.Y;
            scale = Math.Min(scale, scale2);

            float y = Engine.g.Viewport.Height / 6;
            float x = Engine.g.Viewport.Width / 2;

            Engine.DrawText(new Vector2(x, y) - size * scale * 0.5f, title, Color.White, scale);

            y += size.Y * scale / 2 + 10;
            size = Engine.MeasureString(subtitle);
            y += size.Y / 2;
            Engine.DrawText(new Vector2(x, y) - size * 0.5f, subtitle, Color.White);

            Engine.DrawText(new Vector2(10, buttony + 10), exceptionText, Color.Red);


            ui.Render();
        }
    }
}
