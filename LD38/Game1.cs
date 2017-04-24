using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Content;

namespace LD38
{


    public interface IGameContext
    {
        void LoadContent(ContentManager Content);

        void UnloadContent();

        void Update(GameTime gameTime);
        void Draw(GameTime gameTime);
    }

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        GameContext game;
        MapEditorContext editor;
        MenuContext menu;

        IGameContext ActiveContext;

        public void StartGame()
        {
            ActiveContext = game;
            game.SetupGame(Content, "Maps/Map1.mp");
        }
        public void StartGameSavedMap(byte[] mapData)
        {
            game.SetupGame(mapData);
            ActiveContext = game;
        }

        public void StartMapEditor()
        {
            ActiveContext = editor;
        }

        public void StartMenu()
        {
            ActiveContext = menu;
        }
        

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            graphics.PreferredBackBufferWidth = 1200;
            graphics.PreferredBackBufferHeight = 900;

            Content.RootDirectory = "Content";

            game = new GameContext(this);
            editor = new MapEditorContext(this);
            menu = new MenuContext(this);

            //Window.AllowUserResizing = true;
            RestoreWindowLocation();

            StartMenu();

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            if (Window != null)
            {
                // Save window position for next run.
                Properties.Settings.Default.WindowX = Window.Position.X;
                Properties.Settings.Default.WindowY = Window.Position.Y;
                Properties.Settings.Default.HaveWindowPosition = true;
                Properties.Settings.Default.Save();
            }

            base.OnExiting(sender, args);
        }

        void RestoreWindowLocation()
        {
            if(Properties.Settings.Default.HaveWindowPosition)
            {
                int x = Properties.Settings.Default.WindowX;
                int y = Properties.Settings.Default.WindowY;
                Window.Position = new Point(x, y);
                // Todo, recover if this is completely off screen.
            }
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Engine.g = GraphicsDevice;
            Engine.LoadContent(Content);
            game.LoadContent(Content);
            editor.LoadContent(Content);
            menu.LoadContent(Content);

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            game.UnloadContent();
            editor.UnloadContent();
            menu.UnloadContent();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            Engine.g = GraphicsDevice;
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            curMouse = Mouse.GetState();
            curKbdState = Keyboard.GetState();
            curKeys = curKbdState.GetPressedKeys();
            if (lastMouse == null) lastMouse = curMouse;
            if (lastKeys == null) lastKeys = curKeys;
            mouseCursor = curMouse.Position;

            ActiveContext.Update(gameTime);

            lastKeys = curKeys;
            lastMouse = curMouse;
            base.Update(gameTime);
        }

        Point? mouseCursor = null;
        public MouseState lastMouse;
        public Keys[] lastKeys;
        public MouseState curMouse;
        public Keys[] curKeys;
        public KeyboardState curKbdState;

        public IEnumerable<Keys> PressedKeys()
        {
            foreach(Keys k in curKeys)
            {
                if (lastKeys.Contains(k)) continue;

                yield return k;
            }
        }
        public IEnumerable<Keys> HeldKeys()
        {
            return curKeys;
        }
        public bool KeyDown(Keys k)
        {
            return curKbdState.IsKeyDown(k);
        }

        public bool LeftClick()
        {
            return curMouse.LeftButton == ButtonState.Pressed && lastMouse.LeftButton != ButtonState.Pressed;
        }
        public bool RightClick()
        {
            return curMouse.RightButton == ButtonState.Pressed && lastMouse.RightButton != ButtonState.Pressed;
        }
        public bool LeftHeld()
        {
            return curMouse.LeftButton == ButtonState.Pressed && lastMouse.LeftButton == ButtonState.Pressed;
        }
        public bool RightHeld()
        {
            return curMouse.RightButton == ButtonState.Pressed && lastMouse.RightButton == ButtonState.Pressed;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            Engine.g = GraphicsDevice;
            GraphicsDevice.Clear(Color.CornflowerBlue);


            ActiveContext.Draw(gameTime);


            if (mouseCursor != null)
            {
                int x = mouseCursor.Value.X;
                int y = mouseCursor.Value.Y;
                int height = Engine.g.PresentationParameters.BackBufferHeight;
                int width = Engine.g.PresentationParameters.BackBufferWidth;

                VertexPositionColor[] vpc = new VertexPositionColor[3];
                vpc[0].Position = new Vector3((float)(x - width / 2) / (width / 2), -(float)(y - height / 2) / (height / 2), 0.001f);
                vpc[1].Position = vpc[0].Position + new Vector3(20.0f / width, -15.0f / height, 0);
                vpc[2].Position = vpc[0].Position + new Vector3(10.0f / width, -30.0f / height, 0);
                for (int i = 0; i < 3; i++) { vpc[i].Color = Color.White; }
                Engine.Draw2DColor(vpc, 0, 1);
            }


            base.Draw(gameTime);
        }
    }

}
