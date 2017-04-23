using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD38
{
    class GameMap
    {
        public readonly float LayerHeight = 1;
        public readonly int MaxLayer = 7;

        public readonly int Width, Height;
        public GameMap(int mapWidth, int mapHeight)
        {
            Width = mapWidth;
            Height = mapHeight;
            Tiles = new GameMapTile[Width, Height];
            RandomizeTiles();
        }

        public void RandomizeTiles()
        {
            Random r = new Random();
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int alternateCount = AlternateCount(Tiles[x, y].Content);
                    Tiles[x, y].Rotation = (byte)r.Next(4);
                    Tiles[x, y].Variation = (byte)r.Next(alternateCount);
                }
            }
        }

        public int AlternateCount(TileType t)
        {
            if (t == TileType.Water || t == TileType.Bridge) return 1;
            return 3;
        }


        /// <summary>
        /// Ramp direction for a tile. 0 = flat, 1 = top is +x, 2=+y, 3=-x, 4=-y
        /// </summary>
        /// <returns></returns>
        public int RampDirection(int x, int y)
        {
            int l = Tiles[x, y].Level;
            if (Tiles[x, y].Content != TileType.Ramp) return 0;
            if (x > 0 && (x + 1) < Width)
            {
                // x has priority
                int ln = Tiles[x - 1, y].Level;
                int lp = Tiles[x + 1, y].Level;

                if (ln == l - 1 && RampDirection(x - 1, y) == 1) ln = l;
                if (lp == l - 1 && RampDirection(x + 1, y) == 3) lp = l;

                if (ln == l && lp == (l + 1)) return 1; // +x
                if (lp == l && ln == (l + 1)) return 3; // -x
            }
            if (y > 0 && (y + 1) < Height)
            {
                int ln = Tiles[x, y - 1].Level;
                int lp = Tiles[x, y + 1].Level;

                if (ln == l - 1 && RampDirection(x, y - 1) == 2) ln = l;
                if (lp == l - 1 && RampDirection(x, y + 1) == 4) lp = l;

                if (ln == l && lp == (l + 1)) return 2; // +y
                if (lp == l && ln == (l + 1)) return 4; // -y
            }

            return 0;
        }

        /// <summary>
        /// Which cliff is the mine attached to? 0 = none/error, 1 = +x, 2=+y, 3=-x, 4=-y
        /// </summary>
        public int MineDirection(int x, int y)
        {
            for(int i=1;i<5;i++)
            {
                if (TileInDirection(x, y, i).Level > Tiles[x, y].Level) return i;
            }
            return 0;
        }

        /// <summary>
        /// Return the tile you would get if you moved in a certain direction. Returns the current tile if that tile would be invalid
        /// </summary>
        public GameMapTile TileInDirection(int x, int y, int direction)
        {
            int dx, dy;
            GetDirection(direction, out dx, out dy);
            int x2 = x + dx;
            int y2 = y + dy;
            if(x2 < 0 || y2 < 0 || x2 >= Width || y2 >= Height) { return Tiles[x, y]; }
            return Tiles[x2, y2];
        }

        public void GetDirection(int direction, out int dx, out int dy)
        {
            dx = 0;
            dy = 0;
            switch(direction)
            {
                case 0:
                case 1: dx = 1; break;
                case 2: dy = 1; break;
                case 3: dx = -1; break;
                case 4: dy = -1; break;
            }
        }

        public Vector3 CenterPoint(int tx, int ty)
        {
            float z = Tiles[tx, ty].Level * LayerHeight;
            return new Vector3(tx + 0.5f, ty + 0.5f, z);
        }
        public Vector3 CenterPoint(Point pt)
        {
            return CenterPoint(pt.X, pt.Y);
        }


        public GameMapTile[,] Tiles;

        public GameMapTile this[int x, int y] {  get { return Tiles[x, y]; } }
        public GameMapTile this[Point p] {  get { return Tiles[p.X, p.Y]; }  }

        const int FileMagic = 0x123456;

        public IEnumerable<Point> EnumerateMap()
        {
            for(int y=0;y< Height;y++)
            {
                for(int x=0;x< Width;x++)
                {
                    yield return new Point(x, y);
                }
            }
        }

        public byte[] SaveMapData()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write(FileMagic); // Magic
            bw.Write(Width);
            bw.Write(Height);
            bw.Write((int)0); // Complex buildings
            bw.Write((int)0); // Whatever else I need to add.

            for(int y=0;y< Height;y++)
            {
                for(int x=0;x<Width;x++)
                {
                    Tiles[x, y].Serialize(bw);
                }
            }

            return ms.ToArray();
        }

        public static GameMap LoadMapData(byte[] dataStream)
        {
            MemoryStream ms = new MemoryStream(dataStream);
            BinaryReader br = new BinaryReader(ms);

            int magic = br.ReadInt32();
            if (magic != FileMagic) throw new Exception("File signature incorrect");
            int width = br.ReadInt32();
            int height = br.ReadInt32();
            int buildings = br.ReadInt32();
            int reserved = br.ReadInt32();

            GameMap m = new GameMap(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    m.Tiles[x, y] = GameMapTile.Deserialize(br);
                }
            }
            return m;
        }

    }

    enum TileType
    {
        Land, // Nothing interesting, just standard dirt
        Ramp, // Land connecting adjacent levels of different heights (as possible) Ramp Level is the bottom-most level it will connect.
        Water, // impassible terrain
        Bridge, // Water with a structure allowing units to pass
        Forest, // Wood/Food resource
        Mine, // Stone/Metal resource
        Storage, // Storage yard
        Center, // Town center
        House, // Residential
        Turret, // Defensive turret
        Tech, // Part of a multitile building. Reference the map entities for what exactly it is.
    }

    struct GameMapTile
    {
        public byte Level; // 0 is the lowest level
        public byte Variation; // different variations of tiles.
        public byte Rotation; // 0-3
        public byte Owner; // Owning team, 0-15
        public TileType Content;
        public int ResourceValue;
        public int ResourceValue2;

        public bool IsFlat
        {
            get
            {
                if (Content == TileType.Ramp) return false;
                if (Content == TileType.Water) return false;
                if (Content == TileType.Bridge) return false;
                return true;
            }
        }

        public bool IsNavigable
        {
            get
            {
                if (Content == TileType.Water) return false;
                return true;
            }
        }

        public void SetOwner(int owner)
        {
            Owner = (byte)owner;
        }

        public void Serialize(BinaryWriter bw)
        {
            bw.Write((byte)Content);
            bw.Write(Level);
            bw.Write(Variation);
            bw.Write((byte)((Rotation&3)|(Owner<<4));
            bw.Write(ResourceValue);
            bw.Write(ResourceValue2);
        }

        public static GameMapTile Deserialize(BinaryReader br)
        {
            GameMapTile t = new GameMapTile();
            t.Content = (TileType)br.ReadByte();
            t.Level = br.ReadByte();
            t.Variation = br.ReadByte();
            t.Rotation = br.ReadByte();
            t.Owner = (byte)(t.Rotation >> 4);
            t.Rotation = (byte)(t.Rotation & 3);
            t.ResourceValue = br.ReadInt32();
            t.ResourceValue2 = br.ReadInt32();
            return t;
        }
    }
}
