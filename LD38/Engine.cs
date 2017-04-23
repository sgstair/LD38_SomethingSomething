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

        public static float AspectRatio
        {
            get
            {
                return (float)g.PresentationParameters.BackBufferWidth / g.PresentationParameters.BackBufferHeight;
            }
        }


        public static void LoadContent(ContentManager Content)
        {
            ColorEffect = Content.Load<Effect>("Shaders/Color");
            TextureEffect = Content.Load<Effect>("Shaders/Texture");
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
