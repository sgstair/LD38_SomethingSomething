using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD38
{
    class MapRenderer
    {
        public GameMap Map;

        MapMesh WorkMesh;
        Texture2D MapTexture;
        Vector3 CameraLoc, CameraLookAt;

        public MapRenderer(GameMap useMap, Texture2D mapTex)
        {
            Map = useMap;
            MapTexture = mapTex;
            WorkMesh = new MapMesh(MapTexture);
        }


        public void SetCameraLocation(Vector3 location, Vector3 lookAt)
        {
            CameraLoc = location;
            CameraLookAt = lookAt;

            // Setup engine for camera

            Engine.MatPerspective = Matrix.CreatePerspectiveFieldOfView((float)(Math.PI / 2), Engine.AspectRatio, 0.5f, 30) * Matrix.CreateScale(-1, 1, 1);
            Engine.MatWorld = Matrix.Identity;
            Engine.MatView = Matrix.CreateLookAt(location, lookAt, Vector3.UnitZ);

            BlendState bs = new BlendState();

            bs.ColorBlendFunction = BlendFunction.Add;
            bs.ColorSourceBlend = Blend.SourceAlpha;
            bs.ColorDestinationBlend = Blend.InverseSourceAlpha;

            Engine.g.BlendState = bs;
            Engine.g.DepthStencilState = DepthStencilState.Default;

        }



        public Point? FindIntersectingPoint(Vector3 origin, Vector3 direction)
        {
            Vector3 loc = FindIntersectingLocation(origin, direction);
            if (IsInsideMap(loc))
            {
                return new Point((int)Math.Floor(loc.X), (int)Math.Floor(loc.Y));
            }
            return null;
        }

        public bool IsInsideMap(Vector3 location)
        {
            if (location.X < 0 || location.Y < 0 || location.X >= Map.Width || location.Y >= Map.Height) return false;
            return true;
        }

        public Vector3 FindIntersectingLocation(Vector3 origin, Vector3 direction)
        {
            if (direction.Z >= 0) return Vector3.UnitX * -1;

            for (int i = Map.MaxLayer; i >= 0; i--)
            {
                float z = i * Map.LayerHeight;
                if (z >= origin.Z) continue;

                float dz = origin.Z - z;

                Vector3 intersection = origin - direction * (dz / direction.Z);
                int tx = (int)Math.Floor(intersection.X);
                int ty = (int)Math.Floor(intersection.Y);

                if (tx >= 0 && ty >= 0 && tx < Map.Width && ty < Map.Height)
                {
                    if (Map.Tiles[tx, ty].Level == i)
                    {
                        intersection.Z = Map.MapHeight(intersection.X, intersection.Y);
                        return intersection;
                    }
                }
                // Todo: need to add some future logic to also correctly select when not pointing at a tile rectangle itself (slopes, cliffs)
            }
            return Vector3.UnitX * -1; // Effectively "nothing"
        }

        public Vector3 FindIntersectingLocation(Point screenLocation)
        {
            return FindIntersectingLocation(CameraLoc, MouseRay(screenLocation));
        }

        public Vector3 ProjectOntoPlane(Point screenLocation, float zPlane)
        {
            Vector3 Loc = CameraLoc;
            Vector3 ray = MouseRay(screenLocation);

            float zDist = Loc.Z - zPlane;
            Loc += ray * (zDist / ray.Z);

            return Loc;
        }

        public Vector3 MouseRay(Point screenLocation)
        {
            int height = Engine.g.PresentationParameters.BackBufferHeight;
            int width = Engine.g.PresentationParameters.BackBufferWidth;
            float tx = (float)(screenLocation.X - width / 2) / (width / 2);
            float ty = (float)(screenLocation.Y - height / 2) / (height / 2);
            Vector3 cameraLoc = CameraLoc;
            Vector3 forward = CameraLookAt - cameraLoc;
            forward.Normalize();
            Vector3 right = Vector3.Cross(Vector3.UnitZ, forward);
            Vector3 up = Vector3.Cross(forward, right);
            right.Normalize();
            up.Normalize();
            return forward - up * ty + right * tx * Engine.AspectRatio;
        }


        public void DrawMap()
        {
            ResetBillboardQueue();

            // Pass 1, draw all terrain
            WorkMesh.Clear();
            for (int y = 0; y < Map.Height; y++)
            {
                for (int x = 0; x < Map.Width; x++)
                {
                    DrawBaseTile(x, y);

                    // Identify all cliffs connecting to x+1 and y+1 and render them.
                    //if (Map[x, y].IsFlat || Map[x, y].Content == TileType.Ramp)
                    {
                        int level = Map[x, y].Level;
                        if (x + 1 < Map.Width && level != Map[x + 1, y].Level)
                        {
                            int otherLevel = Map[x + 1, y].Level;
                            int dl = level - otherLevel;
                            {
                                // Need cliff
                                if (level < otherLevel)
                                {
                                    DrawCliff(x + 1, y, 0, 1, level, otherLevel - level);
                                }
                                else
                                {
                                    DrawCliff(x + 1, y + 1, 0, -1, otherLevel, level - otherLevel);
                                }
                            }
                        }

                        if (y + 1 < Map.Height && level != Map[x, y + 1].Level)
                        {
                            int otherLevel = Map[x, y + 1].Level;
                            int dl = level - otherLevel;
                            {
                                // Need cliff
                                if (level < otherLevel)
                                {
                                    DrawCliff(x + 1, y + 1, -1, 0, level, otherLevel - level);
                                }
                                else
                                {
                                    DrawCliff(x, y + 1, 1, 0, otherLevel, level - otherLevel);
                                }
                            }
                        }


                    }
                }
            }

            // Draw any cliffs on the outer faces of the map.
            for (int x = 0; x < Map.Width; x++)
            {
                if (Map[x, 0].Level != 0)
                {
                    DrawCliff(x + 1, 0, -1, 0, 0, Map[x, 0].Level);
                }
                if (Map[x, Map.Height - 1].Level != 0)
                {
                    DrawCliff(x, Map.Height, 1, 0, 0, Map[x, Map.Height - 1].Level);
                }
            }
            for (int y = 0; y < Map.Height; y++)
            {
                if (Map[0, y].Level != 0)
                {
                    DrawCliff(0, y, 0, 1, 0, Map[0, y].Level);
                }
                if (Map[Map.Width - 1, y].Level != 0)
                {
                    DrawCliff(Map.Width, y + 1, 0, -1, 0, Map[Map.Width - 1, y].Level);
                }
            }

            WorkMesh.RenderOut();

            // Pass 2, draw water
            // (For now this is just being done as a flat tile)

            // Pass 3, draw buildings / resources
            WorkMesh.Clear();
            for (int y = 0; y < Map.Height; y++)
            {
                for (int x = 0; x < Map.Width; x++)
                {

                    DrawTileStructures(x, y);
                }
            }
            WorkMesh.RenderOut();

            // Pass 4, draw units



        }

        public void DrawBillboards()
        {
            WorkMesh.Clear();
            RenderBillboardQueue();
            WorkMesh.RenderOut();
        }



        public void DrawTileHighlight(int x, int y)
        {
            float z = Map.Tiles[x, y].Level;
            RenderFlatQuad(x, y, x + 1, y + 1, z + 0.05f, new Color(127, 255, 192, 128));
        }
        public void DrawTileHighlight(int x, int y, Color c)
        {
            float z = Map.Tiles[x, y].Level;
            RenderFlatQuad(x, y, x + 1, y + 1, z + 0.05f, c);
        }



        public void DrawSelectionCursor(Vector3 location, float size, Color c, float zBoost = 0)
        {
            // Reuse storage quad because it's perfect.
            WorkMesh.Clear();
            int tile = 0x04;

            float z = location.Z + 0.015f + zBoost;
            float x = location.X;
            float y = location.Y;
            float half = size / 2;
            WorkMesh.AddRampQuadColor(tile, x - half, y - half, x + half, y + half, z, z, z, z, 0, c);

            WorkMesh.RenderOut();
        }

        public void DrawTileIcon(int tileID, Point screenLocation, float size, Color c)
        {
            float dtx = 0.001f;
            float dt = 16.0f / MapTexture.Width;
            float tx = (tileID & 0xFF) * dt;
            float ty = (tileID >> 8) * dt;

            VertexPositionColorTexture[] vpct = new VertexPositionColorTexture[4];

            for (int i = 0; i < 4; i++)
            {
                vpct[i].Color = c;
            }

            vpct[0].TextureCoordinate = new Vector2(tx + dtx, ty + dtx);
            vpct[1].TextureCoordinate = new Vector2(tx + dt - dtx, ty + dtx);
            vpct[2].TextureCoordinate = new Vector2(tx + dtx, ty + dt - dtx);
            vpct[3].TextureCoordinate = new Vector2(tx + dt - dtx, ty + dt - dtx);

            vpct[0].Position = Engine.ScreenCoord(screenLocation.X, screenLocation.Y);
            vpct[1].Position = Engine.ScreenCoord(screenLocation.X + size, screenLocation.Y);
            vpct[2].Position = Engine.ScreenCoord(screenLocation.X, screenLocation.Y + size);
            vpct[3].Position = Engine.ScreenCoord(screenLocation.X + size, screenLocation.Y + size);

            Engine.Draw2DColorTexturePixel(vpct, 0, 2, PrimitiveType.TriangleStrip);

        }

        public void DrawTileIcon(int tileID, Point screenLocation, float size)
        {
            DrawTileIcon(tileID, screenLocation, size, Color.White);
        }




        void DrawBaseTile(int x, int y)
        {
            float z = Map.Tiles[x, y].Level * Map.LayerHeight;
            int tile = Map[x,y].Variation;
            int rotate = Map[x,y].Rotation;
            if (Map.Tiles[x, y].IsFlat)
            {
                //RenderFlatQuad(x, y, x + 1, y + 1, z);
                WorkMesh.AddRampQuad(tile, x, y, x + 1, y + 1, z, z, z, z, rotate, true);
            }
            if (Map.Tiles[x, y].Content == TileType.Water || Map.Tiles[x, y].Content == TileType.Bridge)
            {
                // Future, water rendering might be more complex.
                tile = 0x200;
                if (Map.Tiles[x, y].Content == TileType.Bridge) tile = 0x201;

                WorkMesh.AddRampQuad(tile, x, y, x + 1, y + 1, z, z, z, z, rotate, true);
            }

            if (Map.Tiles[x, y].Content == TileType.Ramp)
            {
                int level = Map.Tiles[x, y].Level;
                float z2 = (level + 1) * Map.LayerHeight;
                switch(Map.RampDirection(x, y))
                {
                    case 0: // No ramp;
                        WorkMesh.AddRampQuad(tile, x, y, x + 1, y + 1, z, z, z, z, rotate, true);
                        break;
                    case 1: // +x
                        GenerateRampGeometry(tile, x, y, x + 1, y + 1, z, z2, z2, z, rotate);
                        break;
                    case 2: // +y
                        GenerateRampGeometry(tile, x, y, x + 1, y + 1, z, z, z2, z2, rotate);
                        break;
                    case 3: // -x
                        GenerateRampGeometry(tile, x, y, x + 1, y + 1, z2, z, z, z2, rotate);
                        break;
                    case 4: // -y
                        GenerateRampGeometry(tile, x, y, x + 1, y + 1, z2, z2, z, z, rotate);
                        break;
                }

            }
        }

        void DrawTileStructures(int x, int y)
        {
            GameMapTile t = Map[x, y];

            switch (t.Content)
            {
                case TileType.Forest:
                    DrawForest(x, y);
                    break;
                case TileType.Mine:
                    DrawMine(x, y);
                    break;
                case TileType.Center:
                    DrawTownCenter(x, y);
                    break;
                case TileType.House:
                    DrawHouse(x, y);
                    break;
                case TileType.Storage:
                    DrawStorage(x, y);
                    break;
                case TileType.Turret:
                    break;
            }

        }

        void DrawForest(int x, int y)
        {
            int tile = 0x06;

            QueueBillboard(Map.CenterPoint(x, y), tile, 1.6f, 1.6f, 0.3f);
        }

        void DrawStorage(int x, int y)
        {
            // Draw a transparent tile at a slightly elevated level (decal-like)
            int tile = 0x04;

            float z = Map.Tiles[x, y].Level * Map.LayerHeight + 0.01f;
            WorkMesh.AddRampQuad(tile, x, y, x + 1, y + 1, z, z, z, z);

            // Todo: visually represent how full the storage area is.
        }


        void DrawMine(int x, int y)
        {
            int mineDirection = Map.MineDirection(x, y);
            if (mineDirection == 0) return; // Unrenderable.
            int tile = 0x05;
            float delta = 0.499f;
            int dx, dy;
            Map.GetDirection(mineDirection, out dx, out dy);

            Vector3 pt = Map.CenterPoint(x, y) + new Vector3(dx,dy,0) * delta;
            Vector3 right = new Vector3(-dy, dx, 0);

            WorkMesh.AddArbitraryQuad(tile, pt - right * delta + Vector3.UnitZ,
                pt + right * delta + Vector3.UnitZ, pt + right * delta, pt - right * delta);
        }


        delegate void SideRendering(int index, Vector3 pt, Vector3 right, Vector3 up, Vector3 normal);
        void DrawSides(int tx, int ty, float baseSize, SideRendering callback)
        {
            float z = Map[tx, ty].Level * Map.LayerHeight;
            Vector3 center = new Vector3(tx + 0.5f, ty + 0.5f, z);
            Vector3 dx = Vector3.UnitX * baseSize / 2;
            Vector3 dy = Vector3.UnitY * baseSize / 2;
            // index 0 faces +y. Other faces wrap around to the right.
            callback(0, center + dy - dx, Vector3.UnitX, Vector3.UnitZ, Vector3.UnitY);
            callback(1, center + dy + dx, -Vector3.UnitY, Vector3.UnitZ, Vector3.UnitX);
            callback(2, center - dy + dx, -Vector3.UnitX, Vector3.UnitZ, -Vector3.UnitY);
            callback(3, center - dy - dx, Vector3.UnitY, Vector3.UnitZ, -Vector3.UnitX);
        }

        void DrawTownCenter(int tx, int ty)
        {
            float height = 1.0f;
            float roofHeight = height + 0.3f;
            float roofSize = 0.8f;
            float topSize = 0.7f;
            float baseSize = 0.6f;
            float adjustTop = (topSize - baseSize) / 2;
            float adjustRoof = (roofSize - baseSize) / 2;
            int tileBase = 0x300;

            DrawSides(tx, ty, baseSize, (i, pt, right, up, normal) =>
              {
                  Vector3 top1 = pt + up * height + normal * adjustTop + right * -adjustTop;
                  Vector3 top2 = top1 + right * topSize;
                  WorkMesh.AddArbitraryQuad(tileBase + i, top1, top2, pt + right * baseSize, pt);

                  top1 = pt + up * height + normal * adjustRoof + right * -adjustRoof;
                  top2 = top1 + right * roofSize;
                  Vector3 peak = pt + right * baseSize / 2 - normal * baseSize / 2 + up * roofHeight;

                  float roofFlip = 0;
                  if ((i & 1) == 1) roofFlip = 1;
                  WorkMesh.AddTexTri(tileBase + 4, top2, top1, peak, new Vector2(1 - roofFlip, 1), new Vector2(roofFlip, 1), new Vector2(0.5f, 0.3f));
              });
        }

        void DrawHouse(int tx, int ty)
        {
            float height = 0.6f;
            float roofHeight = height + 0.3f;
            float roofSize = 0.8f;
            float topSize = 0.7f;
            float baseSize = 0.7f;
            float adjustTop = (topSize - baseSize) / 2;
            float adjustRoof = (roofSize - baseSize) / 2;
            int tileBase = 0x400;

            DrawSides(tx, ty, baseSize, (i, pt, right, up, normal) =>
            {
                Vector3 top1 = pt + up * height + normal * adjustTop + right * -adjustTop;
                Vector3 top2 = top1 + right * topSize;
                WorkMesh.AddArbitraryQuad(tileBase + i, top1, top2, pt + right * baseSize, pt);

                top1 = pt + up * height + normal * adjustRoof + right * -adjustRoof;
                top2 = top1 + right * roofSize;
                Vector3 peak = pt + right * baseSize / 2 - normal * baseSize / 2 + up * roofHeight;

                float roofFlip = 0;
                if ((i & 1) == 1) roofFlip = 1;
                WorkMesh.AddTexTri(tileBase + 4, top2, top1, peak, new Vector2(1 - roofFlip, 1), new Vector2(roofFlip, 1), new Vector2(0.5f, 0.3f));
            });
        }


        void DrawCliff(float x, float y, float dx, float dy, int baseLevel, int height)
        {
            while (height>0)
            {
                float baseZ = baseLevel * Map.LayerHeight;
                float topZ = (baseLevel + 1) * Map.LayerHeight;

                WorkMesh.AddArbitraryQuad(0x100, new Vector3(x, y, topZ), new Vector3(x + dx, y + dy, topZ),
                                                 new Vector3(x + dx, y + dy, baseZ), new Vector3(x, y, baseZ));

                baseLevel++;
                height--;
            }
        }

        void GenerateRampGeometry(int tile, float x, float y, float x2, float y2, float z1, float z2, float z3, float z4, int rotate = 0)
        {
            WorkMesh.AddRampQuad(tile, x, y, x2, y2, z1, z2, z3, z4, rotate, true);

            // Now add cliff side triangles.
            if(z1 != z2)
            {
                AddRampTriangle(0x100, x2, y, -1, 0, z2, z1);
            }
            if(z2!= z3)
            {
                AddRampTriangle(0x100, x2, y2, 0, -1, z3, z2);
            }
            if(z3 != z4)
            {
                AddRampTriangle(0x100, x, y2, 1, 0, z4, z3);
            }
            if(z1 != z4)
            {
                AddRampTriangle(0x100, x, y, 0, 1, z1, z4);
            }
        }
        void AddRampTriangle(int tile, float x, float y, float dx, float dy, float z1, float z2)
        {
            float z = Math.Min(z1, z2);
            Vector3 p1, p2, p3;
            Vector2 t1, t2, t3;

            p1 = new Vector3(x + dx, y + dy, z);
            p2 = new Vector3(x, y, z);
            t1 = new Vector2(1, 1);
            t2 = new Vector2(0, 1);

            if(z1 > z)
            {
                p3 = new Vector3(x, y, z1);
                t3 = new Vector2(0, 0);
            }
            else
            {
                p3 = new Vector3(x+dx, y+dy, z2);
                t3 = new Vector2(1, 0);
            }
            WorkMesh.AddTexTri(tile, p1, p2, p3, t1, t2, t3);
        }


        void RenderFlatQuad(float x1, float y1, float x2, float y2, float z, Color c)
        {
            VertexPositionColor[] vpc = new VertexPositionColor[4];

            vpc[0].Position = new Vector3(x1, y1, z);
            vpc[2].Position = new Vector3(x1, y2, z);
            vpc[1].Position = new Vector3(x2, y1, z);
            vpc[3].Position = new Vector3(x2, y2, z);

            for (int i = 0; i < 4; i++)
            {
                vpc[i].Color = c;
            }

            Engine.DrawColor(vpc, 0, 2, PrimitiveType.TriangleStrip);
        }


        void QueueBillboard(Vector3 location, int tileID, float width = 1.0f, float height = 1.0f, float advance = 0f)
        {
            QueuedBillboard qb = new QueuedBillboard() { Width = width, Height = height, TileId = tileID };

            float tiltPercent = 0.2f;// How far off straight-up the sprite will tilt to better interface the camera.
            float turnPercent = 0.0f; // How much to adjust to react to the camera location.
            //Vector3 toCamera = CameraLoc - location;
            Vector3 toCamera = CameraLoc - CameraLookAt;
            if (advance != 0)
            {
                Vector3 advanceDirection = toCamera;
                advanceDirection.Z = 0;
                advanceDirection.Normalize();
                location += advanceDirection * advance;
            }
            toCamera.Normalize();
            if(QueuedBillboards.Count != 0)
            {
                qb.DistanceToCamera = Vector3.Dot((QueuedBillboards[0].Location - location), toCamera); 
            }

            Vector3 actualCameraDirection = CameraLoc - location;
            actualCameraDirection.Normalize();
            toCamera += actualCameraDirection * turnPercent;
            toCamera.Normalize();


            Vector3 right = Vector3.Cross(toCamera, Vector3.UnitZ);
            right.Normalize();
            Vector3 up = Vector3.UnitZ;
            Vector3 tiltedUp = Vector3.Cross(right, toCamera);
            tiltedUp.Normalize();


            up = up + tiltedUp * tiltPercent;
            up.Normalize();

            qb.Location = location;
            qb.Right = right;
            qb.Up = up;
            

            QueuedBillboards.Add(qb);
        }

        class QueuedBillboard
        {
            public float DistanceToCamera;
            public Vector3 Location, Right, Up;
            public float Width, Height;
            public int TileId;
        }
        List<QueuedBillboard> QueuedBillboards = new List<QueuedBillboard>();

        void ResetBillboardQueue()
        {
            QueuedBillboards.Clear();
        }

        void RenderBillboardQueue()
        {
            foreach(var b in QueuedBillboards.OrderByDescending(q => q.DistanceToCamera))
            {
                Vector3 pt1 = b.Location - b.Right * b.Width / 2 + b.Up * b.Height;
                WorkMesh.AddArbitraryQuad(b.TileId, pt1, pt1 + b.Right * b.Width, b.Location + b.Right * b.Width / 2, b.Location - b.Right * b.Width / 2);
            }
        }
    }

    class MapMesh
    {
        List<VertexPositionColorTexture> vtx;
        List<int> indexes;
        Texture2D MapTexture;

        public MapMesh(Texture2D mapTex)
        {
            vtx = new List<VertexPositionColorTexture>();
            indexes = new List<int>();
            MapTexture = mapTex;
        }

        public void Clear()
        {
            vtx.Clear();
            indexes.Clear();
        }

        void Append(VertexPositionColorTexture[] vtxadd, int[] indexAdd)
        {
            int vtxloc = vtx.Count;
            int indexloc = indexes.Count;
            vtx.AddRange(vtxadd);
            indexes.AddRange(indexAdd);
            for(int i=0;i<indexAdd.Length;i++)
            {
                indexes[indexloc + i] += vtxloc;
            }
        }

        void AppendQuad(params VertexPositionColorTexture[] vtxs)
        {
            if (vtxs.Length != 4) throw new ArgumentException("You call that a quad?");
            int[] index = new int[] { 0, 1, 3, 1, 2, 3 };
            Append(vtxs, index);
        }


        /// <summary>
        /// Some reference for future use...
        /// Draws a quad from x1,y1 - x2,y1 - x2,y2 - x1,y2; the 4 z values map to those locations
        /// TileId is 0xYYXX where x is the x 16-pixel index into the texture, and y is for y. (Allow future texture expansion if necessary, probably will be)
        /// rotate is 0-3. 0 will apply the texture so +x and +y in the tile map to +x/+y in the world space. Positive rotations rotate clockwise.
        /// </summary>
        public void AddRampQuad(int tileID, float x1, float y1, float x2, float y2, float z1, float z2, float z3, float z4, int rotate = 0, bool gradientZ = false)
        {
            AddArbitraryQuad(tileID,
                new Vector3(x1, y1, z1),
                new Vector3(x2, y1, z2),
                new Vector3(x2, y2, z3),
                new Vector3(x1, y2, z4),
                rotate, gradientZ);
        }

        // Same as AddRampQuad, but you specify the 4 points directly.
        public void AddArbitraryQuad(int tileID, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, int rotate = 0, bool gradientZ=false)
        {
            VertexPositionColorTexture[] vpct = new VertexPositionColorTexture[4];

            float dtx = 0.001f;
            float dt = 16.0f / MapTexture.Width;
            float tx = (tileID & 0xFF) * dt;
            float ty = (tileID >> 8) * dt;

            for (int i = 0; i < 4; i++)
            {
                vpct[i].Color = Color.White;
            }
            if(gradientZ)
            {
                vpct[0].Color = ColorFromZValue(p1.Z);
                vpct[1].Color = ColorFromZValue(p2.Z);
                vpct[2].Color = ColorFromZValue(p3.Z);
                vpct[3].Color = ColorFromZValue(p4.Z);
            }
            vpct[0].Position = p1;
            vpct[1].Position = p2;
            vpct[2].Position = p3;
            vpct[3].Position = p4;

            vpct[(4 - rotate) & 3].TextureCoordinate = new Vector2(tx + dtx, ty + dtx);
            vpct[(5 - rotate) & 3].TextureCoordinate = new Vector2(tx + dt - dtx, ty + dtx);
            vpct[(6 - rotate) & 3].TextureCoordinate = new Vector2(tx + dt - dtx, ty + dt - dtx);
            vpct[(7 - rotate) & 3].TextureCoordinate = new Vector2(tx + dtx, ty + dt - dtx);

            AppendQuad(vpct);
        }




        Color ColorFromZValue(float z)
        {
            // 100% at +8, 80% at +0
            z = 0.6f + (float)(Math.Min(1,Math.Max(0,(z / 8))) * 0.4);
            return new Color(z, z, z);
        }



        /// <summary>
        /// Some reference for future use...
        /// Draws a quad from x1,y1 - x2,y1 - x2,y2 - x1,y2; the 4 z values map to those locations
        /// TileId is 0xYYXX where x is the x 16-pixel index into the texture, and y is for y. (Allow future texture expansion if necessary, probably will be)
        /// rotate is 0-3. 0 will apply the texture so +x and +y in the tile map to +x/+y in the world space. Positive rotations rotate clockwise.
        /// </summary>
        public void AddRampQuadColor(int tileID, float x1, float y1, float x2, float y2, float z1, float z2, float z3, float z4, int rotate, Color c)
        {
            AddArbitraryQuadColor(tileID,
                new Vector3(x1, y1, z1),
                new Vector3(x2, y1, z2),
                new Vector3(x2, y2, z3),
                new Vector3(x1, y2, z4),
                rotate, c);
        }

        // Same as AddRampQuad, but you specify the 4 points directly.
        public void AddArbitraryQuadColor(int tileID, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, int rotate, Color c)
        {
            VertexPositionColorTexture[] vpct = new VertexPositionColorTexture[4];

            float dtx = 0.001f;
            float dt = 16.0f / MapTexture.Width;
            float tx = (tileID & 0xFF) * dt;
            float ty = (tileID >> 8) * dt;

            for (int i = 0; i < 4; i++)
            {
                vpct[i].Color = c;
            }
            vpct[0].Position = p1;
            vpct[1].Position = p2;
            vpct[2].Position = p3;
            vpct[3].Position = p4;

            vpct[(4 - rotate) & 3].TextureCoordinate = new Vector2(tx + dtx, ty + dtx);
            vpct[(5 - rotate) & 3].TextureCoordinate = new Vector2(tx + dt - dtx, ty + dtx);
            vpct[(6 - rotate) & 3].TextureCoordinate = new Vector2(tx + dt - dtx, ty + dt - dtx);
            vpct[(7 - rotate) & 3].TextureCoordinate = new Vector2(tx + dtx, ty + dt - dtx);

            AppendQuad(vpct);
        }



        public void AddTexTri(int tileID, Vector3 p1, Vector3 p2, Vector3 p3, Vector2 uv1, Vector2 uv2, Vector2 uv3)
        {
            VertexPositionColorTexture[] vpct = new VertexPositionColorTexture[3];
            int[] triIdx = new int[3] { 0, 1, 2 };

            float dtx = 0.001f;
            float dt = 16.0f / MapTexture.Width;
            float tx = (tileID & 0xFF) * dt;
            float ty = (tileID >> 8) * dt;

            for (int i = 0; i < 3; i++)
            {
                vpct[i].Color = Color.White;
            }
            vpct[0].Position = p1;
            vpct[1].Position = p2;
            vpct[2].Position = p3;
            vpct[0].TextureCoordinate = new Vector2(tx + dtx, ty + dtx) + uv1 * (dt - dtx * 2);
            vpct[1].TextureCoordinate = new Vector2(tx + dtx, ty + dtx) + uv2 * (dt - dtx * 2);
            vpct[2].TextureCoordinate = new Vector2(tx + dtx, ty + dtx) + uv3 * (dt - dtx * 2);

            Append(vpct, triIdx);
        }


        public void RenderOut()
        {
            if (vtx.Count == 0 || indexes.Count == 0) return;
            Engine.Tex0 = MapTexture;
            Engine.DrawIndexedColorTexturePixel(vtx.ToArray(), 0, vtx.Count, indexes.ToArray(), 0, indexes.Count / 3);
        }

    }

}
