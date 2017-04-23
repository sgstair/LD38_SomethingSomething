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


        float buttony;
        void SetupButtons()
        {
            ui.Reset();
            buttony = Engine.g.Viewport.Height / 3;
            AddButton("Start Game", ClickStartGame);
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

            ui.Render();
        }
    }
}
