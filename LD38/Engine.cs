using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD38
{
    /// <summary>
    /// Convenience class to wrap around core functionality of DirectX to make life easier.
    /// </summary>
    class Engine
    {
        public static GraphicsDevice g;

        static Effect ColorEffect;
        static Effect TextureEffect;

        public static Matrix MatWorld;
        public static Matrix MatView;
        public static Matrix MatPerspective;

        public static Texture Tex0;

        static SpriteFont Font;
        static SpriteBatch FontBatch;

        public static float AspectRatio
        {
            get
            {
                return (float)g.PresentationParameters.BackBufferWidth / g.PresentationParameters.BackBufferHeight;
            }
        }


        public static Vector3 ScreenCoord(float x, float y)
        {
            x = (x - g.Viewport.Width / 2) / (g.Viewport.Width / 2);
            y = -(y - g.Viewport.Height / 2) / (g.Viewport.Height / 2);
            return new Vector3(x, y, 0.01f);
        }

        public static void LoadContent(ContentManager Content)
        {
            ColorEffect = Content.Load<Effect>("Shaders/Color");
            TextureEffect = Content.Load<Effect>("Shaders/Texture");
            Font = Content.Load<SpriteFont>("font/Font");
            FontBatch = new SpriteBatch(g);
        }

        public static Vector2 MeasureString(string text)
        {
            return Font.MeasureString(text);
        }
        public static void DrawText(Vector2 location, string text, Color color, float scale = 1.0f)
        {
            FontBatch.Begin();
            FontBatch.DrawString(Font, text, location, color, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
            FontBatch.End();
        }


        public static void DrawScreenRect(Rectangle rc, Color color)
        {
            VertexPositionColor[] vpc = new VertexPositionColor[4];

            for (int i = 0; i < 4; i++)
            {
                vpc[i].Color = color;
            }
            vpc[0].Position = Engine.ScreenCoord(rc.Left, rc.Top);
            vpc[1].Position = Engine.ScreenCoord(rc.Right, rc.Top);
            vpc[2].Position = Engine.ScreenCoord(rc.Left, rc.Bottom);
            vpc[3].Position = Engine.ScreenCoord(rc.Right, rc.Bottom);

            Engine.Draw2DColor(vpc, 0, 2, PrimitiveType.TriangleStrip);
        }


        public static void Draw2DColor(VertexPositionColor[] vpc, int start, int count, PrimitiveType type = PrimitiveType.TriangleList)
        {
            ColorEffect.CurrentTechnique = ColorEffect.Techniques["BasicColor"];
            foreach (var pass in ColorEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                g.DrawUserPrimitives(type, vpc, start, count);
            }
        }


        public static void DrawColor(VertexPositionColor[] vpc, int start, int count, PrimitiveType type = PrimitiveType.TriangleList)
        {
            ColorEffect.CurrentTechnique = ColorEffect.Techniques["TransformColor"];
            ColorEffect.Parameters["MatWorld"].SetValue(MatWorld);
            ColorEffect.Parameters["MatView"].SetValue(MatView);
            ColorEffect.Parameters["MatProjection"].SetValue(MatPerspective);

            foreach (var pass in ColorEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                g.DrawUserPrimitives(type, vpc, start, count);
            }
        }


        public static void Draw2DColorTexture(VertexPositionColorTexture[] vpc, int start, int count, PrimitiveType type = PrimitiveType.TriangleList)
        {
            TextureEffect.CurrentTechnique = TextureEffect.Techniques["BasicColorTexture"];
            TextureEffect.Parameters["Tex"].SetValue(Tex0);
            foreach (var pass in TextureEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                g.DrawUserPrimitives(type, vpc, start, count);
            }
        }

        public static void DrawColorTexture(VertexPositionColorTexture[] vpc, int start, int count, PrimitiveType type = PrimitiveType.TriangleList)
        {
            TextureEffect.CurrentTechnique = TextureEffect.Techniques["TransformColorTexture"];
            TextureEffect.Parameters["MatWorld"].SetValue(MatWorld);
            TextureEffect.Parameters["MatView"].SetValue(MatView);
            TextureEffect.Parameters["MatProjection"].SetValue(MatPerspective);
            TextureEffect.Parameters["Tex"].SetValue(Tex0);

            foreach (var pass in TextureEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                g.DrawUserPrimitives(type, vpc, start, count);
            }
        }

        public static void DrawIndexedColorTexture(VertexPositionColorTexture[] vpc, int vtxstart, int vtxcount, int[] indexes, int indexstart, int primitivecount, PrimitiveType type = PrimitiveType.TriangleList)
        {
            TextureEffect.CurrentTechnique = TextureEffect.Techniques["TransformColorTexture"];
            TextureEffect.Parameters["MatWorld"].SetValue(MatWorld);
            TextureEffect.Parameters["MatView"].SetValue(MatView);
            TextureEffect.Parameters["MatProjection"].SetValue(MatPerspective);
            TextureEffect.Parameters["Tex"].SetValue(Tex0);

            foreach (var pass in TextureEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                g.DrawUserIndexedPrimitives(type, vpc, vtxstart, vtxcount, indexes, indexstart, primitivecount);
            }
        }


        public static void Draw2DColorTexturePixel(VertexPositionColorTexture[] vpc, int start, int count, PrimitiveType type = PrimitiveType.TriangleList)
        {
            TextureEffect.CurrentTechnique = TextureEffect.Techniques["BasicColorTexturePixel"];
            TextureEffect.Parameters["Tex"].SetValue(Tex0);
            foreach (var pass in TextureEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                g.DrawUserPrimitives(type, vpc, start, count);
            }
        }

        public static void DrawColorTexturePixel(VertexPositionColorTexture[] vpc, int start, int count, PrimitiveType type = PrimitiveType.TriangleList)
        {
            TextureEffect.CurrentTechnique = TextureEffect.Techniques["TransformColorTexturePixel"];
            TextureEffect.Parameters["MatWorld"].SetValue(MatWorld);
            TextureEffect.Parameters["MatView"].SetValue(MatView);
            TextureEffect.Parameters["MatProjection"].SetValue(MatPerspective);
            TextureEffect.Parameters["Tex"].SetValue(Tex0);

            foreach (var pass in TextureEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                g.DrawUserPrimitives(type, vpc, start, count);
            }
        }

        public static void DrawIndexedColorTexturePixel(VertexPositionColorTexture[] vpc, int vtxstart, int vtxcount, int[] indexes, int indexstart, int primitivecount, PrimitiveType type = PrimitiveType.TriangleList)
        {
            TextureEffect.CurrentTechnique = TextureEffect.Techniques["TransformColorTexturePixel"];
            TextureEffect.Parameters["MatWorld"].SetValue(MatWorld);
            TextureEffect.Parameters["MatView"].SetValue(MatView);
            TextureEffect.Parameters["MatProjection"].SetValue(MatPerspective);
            TextureEffect.Parameters["Tex"].SetValue(Tex0);

            foreach (var pass in TextureEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                g.DrawUserIndexedPrimitives(type, vpc, vtxstart, vtxcount, indexes, indexstart, primitivecount);
            }
        }

    }
}
