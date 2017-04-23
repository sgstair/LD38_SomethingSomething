using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD38
{
    class UiSystem
    {
        List<UiButton> Buttons = new List<UiButton>();

        public UiSystem()
        {
        }

        public void Reset()
        {
            Buttons.Clear();
        }

        public bool TestHit(int x, int y)
        {
            foreach(var btn in Buttons)
            {
                if (btn.TestHit(x, y)) return true;
            }
            return false;
        }
        public bool DidClick(int x, int y)
        {
            foreach (var btn in Buttons)
            {
                if (btn.DidClick(x, y)) return true;
            }
            return false;
        }

        public void AddButton(UiButton btn)
        {
            Buttons.Add(btn);
        }

        public void Render()
        {
            
            foreach(var btn in Buttons)
            {
                btn.Render();
            }
        }
    }



    class UiButton
    {
        public delegate void UiClick(UiButton clicked);

        const float margin = 5;
        public float x, y, width, height;
        public readonly string buttonText;
        public Color BtnColor = Color.DarkGray;
        public Color TextColor = Color.White;
        public float clickEffect = 0;

        Vector2 buttonTextSize;
        public event UiClick Click;
        public UiButton(string text)
        {
            buttonText = text;
            buttonTextSize = Engine.MeasureString(text);
            width = buttonTextSize.X + margin * 2;
            height = buttonTextSize.Y + margin * 2;
            x = 10;
            y = 10;
        }


        public bool TestHit(int x, int y)
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

        public bool DidClick(int x, int y)
        {
            if(TestHit(x, y))
            {
                clickEffect = 1;
                OnClick();
                return true;
            }
            return false;
        }

        void OnClick()
        {
            UiClick c = Click;
            if(c != null)
            {
                c(this);
            }
        }

        public void Render()
        {
            VertexPositionColor[] vpc = new VertexPositionColor[4];

            Color c = BtnColor;
            if(clickEffect > 0)
            {
                clickEffect = clickEffect * 0.8f;
                if (clickEffect < 0.0001f) clickEffect = 0;
                c = new Color(c.ToVector3() * (1-clickEffect) + Color.White.ToVector3() * clickEffect);
            }

            for(int i=0;i<4;i++)
            {
                vpc[i].Color = c;
            }
            vpc[0].Position = Engine.ScreenCoord(x, y);
            vpc[1].Position = Engine.ScreenCoord(x+width, y);
            vpc[2].Position = Engine.ScreenCoord(x, y+height);
            vpc[3].Position = Engine.ScreenCoord(x+width, y+height);

            Engine.Draw2DColor(vpc, 0, 2, PrimitiveType.TriangleStrip);

            Engine.DrawText(new Vector2(x + width / 2, y + height / 2) - buttonTextSize / 2, buttonText, TextColor);

        }
    }

}
