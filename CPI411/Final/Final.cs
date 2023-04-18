using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

using System;
using System.Collections.Generic;

namespace Final
{
    public class Final : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        SpriteFont font;
        Model model;
        Texture2D[] textures;
        Effect effect;

        bool showInfo = true, showHelp = true;

        Matrix world = Matrix.Identity;
        Matrix view, projection;

        Vector3 cameraPosition;
        Vector3 cameraTarget;

        float cameraAngleX = -30, cameraAngleY = -30;
        float distance = 15;

        MouseState previousMouseState;
        KeyboardState previousKeyboardState;

        bool triangleColor = false, areVerticesColorful = false;
        List<VertexPositionColor> vertices = new List<VertexPositionColor>
        {
            new VertexPositionColor(new Vector3(-10, 0, 10), Color.Gray),       // Top left
            new VertexPositionColor(new Vector3(10, 0, 10), Color.Gray),      // Top right
            new VertexPositionColor(new Vector3(10, 0, -10), Color.Gray),      // Bottom right

            new VertexPositionColor(new Vector3(10, 0, -10), Color.LightGray),     // Bottom right
            new VertexPositionColor(new Vector3(-10, 0, -10), Color.LightGray),    // Bottom left
            new VertexPositionColor(new Vector3(-10, 0, 10), Color.LightGray)      // Top left
        };

        public Final()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            world = Matrix.Identity;
            view = Matrix.CreateLookAt(cameraPosition, new Vector3(), new Vector3(0, 0, 0));
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), GraphicsDevice.Viewport.AspectRatio, 0.1f, 100);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("font");
            effect = Content.Load<Effect>("SubdivisionShader");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

            // Subdivide the polygon
            if(Keyboard.GetState().IsKeyDown(Keys.D) && !previousKeyboardState.IsKeyDown(Keys.D)) { CatmullClarkSubdivision(); }

            // Toggle triangle visualization
            if (Keyboard.GetState().IsKeyDown(Keys.Space) && !previousKeyboardState.IsKeyDown(Keys.Space)) { VisualizeTriangles(); }

            // Info UI + Help UI
            if (Keyboard.GetState().IsKeyDown(Keys.H) && !previousKeyboardState.IsKeyDown(Keys.H)) { showInfo = !showInfo; }
            if (Keyboard.GetState().IsKeyDown(Keys.OemQuestion) && !previousKeyboardState.IsKeyDown(Keys.OemQuestion)) { showHelp = !showHelp; }

            // Reset the camera
            if (Keyboard.GetState().IsKeyDown(Keys.S)) { cameraAngleX = cameraAngleY = -30; distance = 15; cameraTarget = Vector3.Zero; }

            // Distance control
            if (previousMouseState.RightButton == ButtonState.Pressed && Mouse.GetState().RightButton == ButtonState.Pressed)
            {
                distance += (Mouse.GetState().X - previousMouseState.X) / 100f;
            }

            // Camera rotation control
            if (previousMouseState.LeftButton == ButtonState.Pressed && Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                cameraAngleX += (previousMouseState.X - Mouse.GetState().X);
                cameraAngleY += (previousMouseState.Y - Mouse.GetState().Y);
            }

            // Camera translation control
            if (previousMouseState.MiddleButton == ButtonState.Pressed && Mouse.GetState().MiddleButton == ButtonState.Pressed)
            {
                Vector3 ViewRight = Vector3.Transform(Vector3.UnitX, Matrix.CreateRotationX(cameraAngleY) * Matrix.CreateRotationY(cameraAngleX));
                Vector3 ViewUp = Vector3.Transform(Vector3.UnitY, Matrix.CreateRotationX(cameraAngleY) * Matrix.CreateRotationY(cameraAngleX));
                cameraTarget -= ViewRight * (Mouse.GetState().X - previousMouseState.X) / 10f;
                cameraTarget += ViewUp * (Mouse.GetState().Y - previousMouseState.Y) / 10f;
            }

            cameraPosition = Vector3.Transform(new Vector3(0, 0, distance), Matrix.CreateTranslation(cameraTarget) * Matrix.CreateRotationX(MathHelper.ToRadians(cameraAngleY)) * Matrix.CreateRotationY(MathHelper.ToRadians(cameraAngleX)));
            view = Matrix.CreateLookAt(cameraPosition, cameraTarget, Vector3.Transform(Vector3.UnitY, Matrix.CreateRotationX(MathHelper.ToRadians(cameraAngleY)) * Matrix.CreateRotationY(MathHelper.ToRadians(cameraAngleX))));

            previousMouseState = Mouse.GetState();
            previousKeyboardState = Keyboard.GetState();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            effect.Parameters["World"].SetValue(world);
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, vertices.ToArray(), 0, vertices.Count / 3);
            }

            _spriteBatch.Begin();
            if (showInfo)
            {
                int i = 0;
                _spriteBatch.DrawString(font, "Camera Position: (" + cameraPosition.X.ToString("0.00") + ", " + cameraPosition.Y.ToString("0.00") + ", " + cameraPosition.Z.ToString("0.00") + ")", Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "Camera Angle: (" + cameraAngleX.ToString("0.00") + ", " + cameraAngleY.ToString("0.00") + ")", Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "Number of Vertices: " + vertices.Count, Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "Number of Triangles: " + vertices.Count / 3, Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Black);
            }
            if (showHelp)
            {
                int i = 0;
                _spriteBatch.DrawString(font, "Press H to show/hide the Info Menu", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "Press ? to show/hide the Help Menu", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "Left Click + Drag Rotates the Camera", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "Right Click + Drag Zooms In/Out", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "Middle Mouse + Drag Translates Camera", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "S Key: Resets the Camera and Light", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "Space: Toggle Triangle Colors", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
            }
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Main Subdivision algorithm that divides each triangle into 4 triangles.
        /// TODO: Improve on subdivision and clamp amount of subdivisions allowed
        /// </summary>
        private void CatmullClarkSubdivision()
        {
            Color vertexColor = triangleColor ? Color.Gray : Color.LightGray;
            Vector3 vertex0 = Vector3.Zero,
                    vertex1 = Vector3.Zero, 
                    vertex2 = Vector3.Zero;

            List<VertexPositionColor> subdivisionVertices = new List<VertexPositionColor>();
            int j = 0;
            for (int i = 0; i < vertices.Count; i++)
            {
                if (i % 3 == 0) { vertex0 = vertices[i].Position; j = 1; }
                else if (i % 3 == 1) { vertex1 = vertices[i].Position; j = 2; }
                else { vertex2 = vertices[i].Position; j = 3; }

                if(j == 3)
                {
                    // Keeping top edge
                    vertexColor = Color.Red;
                    subdivisionVertices.Add(new VertexPositionColor(vertex0, vertexColor));
                    subdivisionVertices.Add(new VertexPositionColor((vertex0 + vertex1) / 2, vertexColor));
                    subdivisionVertices.Add(new VertexPositionColor((vertex0 + vertex2) / 2, vertexColor));
                    triangleColor = !triangleColor;
                    //vertexColor = triangleColor ? Color.Gray : Color.LightGray;

                    // Middle triangle
                    vertexColor = Color.Green;
                    subdivisionVertices.Add(new VertexPositionColor((vertex0 + vertex1) / 2, vertexColor));
                    subdivisionVertices.Add(new VertexPositionColor((vertex0 + vertex2) / 2, vertexColor));
                    subdivisionVertices.Add(new VertexPositionColor((vertex1 + vertex2) / 2, vertexColor));
                    triangleColor = !triangleColor;
                    //vertexColor = triangleColor ? Color.Gray : Color.LightGray;

                    // Hypotenuse bottom
                    vertexColor = Color.Blue;
                    subdivisionVertices.Add(new VertexPositionColor(vertex1, vertexColor));
                    subdivisionVertices.Add(new VertexPositionColor((vertex0 + vertex1) / 2, vertexColor));
                    subdivisionVertices.Add(new VertexPositionColor((vertex1 + vertex2) / 2, vertexColor));
                    triangleColor = !triangleColor;
                    //vertexColor = triangleColor ? Color.Gray : Color.LightGray;

                    // Hypotenuse top
                    vertexColor = Color.White;
                    subdivisionVertices.Add(new VertexPositionColor(vertex2, vertexColor));
                    subdivisionVertices.Add(new VertexPositionColor((vertex0 + vertex2) / 2, vertexColor));
                    subdivisionVertices.Add(new VertexPositionColor((vertex2 + vertex1) / 2, vertexColor));
                    triangleColor = !triangleColor;
                    //vertexColor = triangleColor ? Color.Gray : Color.LightGray;

                    vertex0 = vertex1 = vertex2 = Vector3.Zero;
                    j = 0;
                }
            }

            vertices = subdivisionVertices;
        }

        /// <summary>
        /// This method will toggle between the color white and the colors being visualized by RBGW coloring
        /// </summary>
        private void VisualizeTriangles()
        {
            int colorNumber = 0; // 0 is red, 1 is green, 2 is blue, 3 is white
            int j = 0;
            for(int i = 0; i < vertices.Count; i++)
            {
                Vector3 vertexPosition = vertices[i].Position;
                if (areVerticesColorful)
                {
                    switch (colorNumber)
                    {
                        case 0:
                            vertices[i] = new VertexPositionColor(vertexPosition, Color.Red);
                            break;

                        case 1:
                            vertices[i] = new VertexPositionColor(vertexPosition, Color.Green);
                            break;

                        case 2:
                            vertices[i] = new VertexPositionColor(vertexPosition, Color.Blue);
                            break;

                        case 3:
                            vertices[i] = new VertexPositionColor(vertexPosition, Color.Orange);
                            break;

                        case 4:
                            vertices[i] = new VertexPositionColor(vertexPosition, Color.Purple);
                            break;

                        case 5:
                            vertices[i] = new VertexPositionColor(vertexPosition, Color.Yellow);
                            break;

                        default:
                            vertices[i] = new VertexPositionColor(vertexPosition, Color.White);
                            break;
                    }
                }
                else
                {
                    switch(colorNumber)
                    {
                        case 0:
                            vertices[i] = new VertexPositionColor(vertexPosition, Color.Gray);
                            break;

                        case 1:
                            vertices[i] = new VertexPositionColor(vertexPosition, Color.LightGray);
                            break;

                        case 2:
                            vertices[i] = new VertexPositionColor(vertexPosition, Color.LightSlateGray);
                            break;

                        case 3:
                            vertices[i] = new VertexPositionColor(vertexPosition, Color.WhiteSmoke);
                            break;

                        case 4:
                            vertices[i] = new VertexPositionColor(vertexPosition, Color.GhostWhite);
                            break;

                        case 5:
                            vertices[i] = new VertexPositionColor(vertexPosition, Color.DimGray);
                            break;

                        default:
                            vertices[i] = new VertexPositionColor(vertexPosition, Color.White);
                            break;
                    }
                }

                if(j == 2)
                {
                    if (colorNumber == 5) colorNumber = 0;
                    else colorNumber++;

                    j = 0;
                }
                else
                {
                    j++;
                }

            }

            areVerticesColorful = !areVerticesColorful;
        }
    }
}