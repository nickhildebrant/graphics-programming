using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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

        Random random;

        bool showInfo = true, showHelp = true;

        Matrix world = Matrix.Identity;
        Matrix view, projection;

        Vector3 cameraPosition;
        Vector3 cameraTarget = Vector3.Zero;

        float cameraAngleX = -30, cameraAngleY = -30;
        float distance = 15f;

        MouseState previousMouseState;
        KeyboardState previousKeyboardState;

        bool triangleColor = false, areVerticesColorful = false;
        bool toggleHeightColor = false, toggleTexture = false;
        bool isLatest = true;
        float textureDisplacement = 1f;
        float tesselation = 8;
        float geometryGeneration = 5;

        VertexBuffer triangleBuffer;

        List<VertexPositionColorNormalTexture> vertices = new List<VertexPositionColorNormalTexture>
        {
            // Bottom
            new VertexPositionColorNormalTexture(new Vector3(-10, 0, 10), Color.Gray, Vector3.Up, new Vector2(0, 1)),       // Top left
            new VertexPositionColorNormalTexture(new Vector3(10, 0, 10), Color.Gray, Vector3.Up, new Vector2(1, 1)),        // Top right
            new VertexPositionColorNormalTexture(new Vector3(10, 0, -10), Color.Gray, Vector3.Up, new Vector2(1, 0)),       // Bottom right

            new VertexPositionColorNormalTexture(new Vector3(10, 0, -10), Color.LightGray, Vector3.Up, new Vector2(1, 0)),  // Bottom right
            new VertexPositionColorNormalTexture(new Vector3(-10, 0, -10), Color.LightGray, Vector3.Up, new Vector2(0, 0)), // Bottom left
            new VertexPositionColorNormalTexture(new Vector3(-10, 0, 10), Color.LightGray, Vector3.Up, new Vector2(0, 1)),   // Top left

            /*
            /// TOP
            new VertexPositionColorNormalTexture(new Vector3(-10, 10, 10), Color.Gray, Vector3.Up, new Vector2(-10, 10)),       // Top left
            new VertexPositionColorNormalTexture(new Vector3(10, 10, 10), Color.Gray, Vector3.Up, new Vector2(10, 10)),        // Top right
            new VertexPositionColorNormalTexture(new Vector3(10, 10, -10), Color.Gray, Vector3.Up, new Vector2(10, 10)),       // Bottom right

            new VertexPositionColorNormalTexture(new Vector3(10, 10, -10), Color.LightGray, Vector3.Up, new Vector2(10, 10)),  // Bottom right
            new VertexPositionColorNormalTexture(new Vector3(-10, 10, -10), Color.LightGray, Vector3.Up, new Vector2(-10, 10)), // Bottom left
            new VertexPositionColorNormalTexture(new Vector3(-10, 10, 10), Color.LightGray, Vector3.Up, new Vector2(-10, 10)),   // Top left

            /// Front
            new VertexPositionColorNormalTexture(new Vector3(10, -10, 10), Color.Gray, Vector3.Forward, new Vector2(10, -10)),       // Top left
            new VertexPositionColorNormalTexture(new Vector3(10, 10, 10), Color.Gray, Vector3.Forward, new Vector2(10, 10)),        // Top right
            new VertexPositionColorNormalTexture(new Vector3(10, 10, -10), Color.Gray, Vector3.Forward, new Vector2(10, 10)),       // Bottom right

            new VertexPositionColorNormalTexture(new Vector3(10, 10, -10), Color.LightGray, Vector3.Forward, new Vector2(10, 10)),  // Bottom right
            new VertexPositionColorNormalTexture(new Vector3(10, -10, -10), Color.LightGray, Vector3.Forward, new Vector2(10, -10)), // Bottom left
            new VertexPositionColorNormalTexture(new Vector3(10, -10, 10), Color.LightGray, Vector3.Forward, new Vector2(10, -10)),   // Top left

            /// Back
            new VertexPositionColorNormalTexture(new Vector3(-10, -10, 10), Color.Gray, Vector3.Backward, new Vector2(-10, -10)),       // Top left
            new VertexPositionColorNormalTexture(new Vector3(-10, 10, 10), Color.Gray, Vector3.Backward, new Vector2(-10, 10)),        // Top right
            new VertexPositionColorNormalTexture(new Vector3(-10, 10, -10), Color.Gray, Vector3.Backward, new Vector2(-10, 10)),       // Bottom right

            new VertexPositionColorNormalTexture(new Vector3(-10, 10, -10), Color.LightGray, Vector3.Backward, new Vector2(-10, 10)),  // Bottom right
            new VertexPositionColorNormalTexture(new Vector3(-10, -10, -10), Color.LightGray, Vector3.Backward, new Vector2(-10, -10)), // Bottom left
            new VertexPositionColorNormalTexture(new Vector3(-10, -10, 10), Color.LightGray, Vector3.Backward, new Vector2(-10, -10)),   // Top left

            // Left
            new VertexPositionColorNormalTexture(new Vector3(10, 10, 10), Color.Gray, Vector3.Left, new Vector2(10, 10)),       // Top left
            new VertexPositionColorNormalTexture(new Vector3(-10, 10, 10), Color.Gray, Vector3.Left, new Vector2(-10, 10)),        // Top right
            new VertexPositionColorNormalTexture(new Vector3(-10, -10, 10), Color.Gray, Vector3.Left, new Vector2(-10, -10)),       // Bottom right

            new VertexPositionColorNormalTexture(new Vector3(-10, -10, -10), Color.LightGray, Vector3.Left, new Vector2(-10, -10)),       // Top left
            new VertexPositionColorNormalTexture(new Vector3(10, -10, -10), Color.LightGray, Vector3.Left, new Vector2(10, -10)),        // Top right
            new VertexPositionColorNormalTexture(new Vector3(10, 10, -10), Color.LightGray, Vector3.Left, new Vector2(10, 10)),       // Bottom right

            // Right
            new VertexPositionColorNormalTexture(new Vector3(10, 10, -10), Color.Gray, Vector3.Right, new Vector2(10, 10)),  // Bottom right
            new VertexPositionColorNormalTexture(new Vector3(-10, -10, -10), Color.Gray, Vector3.Right, new Vector2(-10, -10)), // Bottom left
            new VertexPositionColorNormalTexture(new Vector3(-10, 10, -10), Color.Gray, Vector3.Right, new Vector2(-10, 10)),   // Top left

            new VertexPositionColorNormalTexture(new Vector3(-10, -10, 10), Color.LightGray, Vector3.Right, new Vector2(-10, -10)),  // Bottom right
            new VertexPositionColorNormalTexture(new Vector3(10, 10, 10), Color.LightGray, Vector3.Right, new Vector2(10, 10)), // Bottom left
            new VertexPositionColorNormalTexture(new Vector3(10, -10, 10), Color.LightGray, Vector3.Right, new Vector2(10, -10)),   // Top left
            */
        };

        int subdivisionIteration = 0;
        List<VertexPositionColorNormalTexture[]> iterationsList = new List<VertexPositionColorNormalTexture[]>();

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

            triangleBuffer = GenerateVertices();

            iterationsList.Add(vertices.ToArray());

            random = new Random();

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

            // Increase the tesselation height
            if(Keyboard.GetState().IsKeyDown(Keys.D))
            { 
                textureDisplacement += Keyboard.GetState().IsKeyDown(Keys.LeftShift) ? 0.01f : -0.01f; 
            }

            // Toggle the heightmap colors
            if(Keyboard.GetState().IsKeyDown(Keys.E)) { toggleHeightColor = true; }
            else { toggleHeightColor = false; }

            // Toggle displaying the texture
            if (Keyboard.GetState().IsKeyDown(Keys.R)) { toggleTexture = true; }
            else { toggleTexture = false; }

            // Toggle triangle visualization
            if (Keyboard.GetState().IsKeyDown(Keys.Space) && !previousKeyboardState.IsKeyDown(Keys.Space)) { VisualizeTriangles(); }

            // Make all triangles white
            if (Keyboard.GetState().IsKeyDown(Keys.C) && !previousKeyboardState.IsKeyDown(Keys.C)) { ClearAllTriangles(); }

            // Control the current subdivision algorithm
            if (Keyboard.GetState().IsKeyDown(Keys.Up) && !previousKeyboardState.IsKeyDown(Keys.Up))
            {
                if(subdivisionIteration < 12)
                {
                    int power = (int)Math.Pow(2, (double)(1 + 2 * (subdivisionIteration + 1)));
                    //if (power > iterationsList[iterationsList.Count - 1].Length / 3) CatmullClarkSubdivision();

                    if(isLatest) CatmullClarkSubdivision();

                    if (subdivisionIteration < iterationsList.Count - 1 && iterationsList != null) 
                    { 
                        subdivisionIteration++;
                        if (subdivisionIteration + 1 == iterationsList.Count) isLatest = true;
                    }
                }
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Down) && !previousKeyboardState.IsKeyDown(Keys.Down)) 
            {
                if (subdivisionIteration > 0 && iterationsList.Count > 0)
                {
                    subdivisionIteration--;
                    isLatest = false;
                }
            }

            // Info UI + Help UI
            if (Keyboard.GetState().IsKeyDown(Keys.H) && !previousKeyboardState.IsKeyDown(Keys.H)) { showInfo = !showInfo; }
            if (Keyboard.GetState().IsKeyDown(Keys.OemQuestion) && !previousKeyboardState.IsKeyDown(Keys.OemQuestion)) { showHelp = !showHelp; }

            // Reset the camera
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            { 
                cameraAngleX = cameraAngleY = -30; 
                distance = 15; 
                cameraTarget = Vector3.Zero;
                iterationsList.Clear();
                vertices = new List<VertexPositionColorNormalTexture>
                {
                    // Bottom
                    new VertexPositionColorNormalTexture(new Vector3(-10, 0, 10), Color.Gray, Vector3.Up, new Vector2(0, 1)),       // Top left
                    new VertexPositionColorNormalTexture(new Vector3(10, 0, 10), Color.Gray, Vector3.Up, new Vector2(1, 1)),        // Top right
                    new VertexPositionColorNormalTexture(new Vector3(10, 0, -10), Color.Gray, Vector3.Up, new Vector2(1, 0)),       // Bottom right

                    new VertexPositionColorNormalTexture(new Vector3(10, 0, -10), Color.LightGray, Vector3.Up, new Vector2(1, 0)),  // Bottom right
                    new VertexPositionColorNormalTexture(new Vector3(-10, 0, -10), Color.LightGray, Vector3.Up, new Vector2(0, 0)), // Bottom left
                    new VertexPositionColorNormalTexture(new Vector3(-10, 0, 10), Color.LightGray, Vector3.Up, new Vector2(0, 1)),   // Top left
                };

                iterationsList.Add(vertices.ToArray());
                subdivisionIteration = 0;
            }

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
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            effect.Parameters["World"].SetValue(world);
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);
            Matrix worldInverseTransposeMatrix = Matrix.Transpose(Matrix.Invert(world));
            effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTransposeMatrix);

            effect.Parameters["DisplacementTexture"].SetValue(Content.Load<Texture>("perlinNoise"));
            effect.Parameters["DisplacementHeight"].SetValue(textureDisplacement);

            effect.Parameters["HeightMapColors"].SetValue(toggleHeightColor);
            effect.Parameters["ShowTexture"].SetValue(toggleTexture);

            //effect.Parameters["TesselationFactor"].SetValue(tesselation);
            //effect.Parameters["GeometryGeneration"].SetValue(geometryGeneration);
            //effect.Parameters["TextureDisplacement"].SetValue(textureDisplacement);

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives<VertexPositionColorNormalTexture>(PrimitiveType.TriangleList, iterationsList[subdivisionIteration], 0, iterationsList[subdivisionIteration].Length / 3);
            }

            _spriteBatch.Begin();
            if (showInfo)
            {
                int i = 0;
                _spriteBatch.DrawString(font, "Camera Position: (" + cameraPosition.X.ToString("0.00") + ", " + cameraPosition.Y.ToString("0.00") + ", " + cameraPosition.Z.ToString("0.00") + ")", Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "Camera Angle: (" + cameraAngleX.ToString("0.00") + ", " + cameraAngleY.ToString("0.00") + ")", Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "Number of Vertices: " + iterationsList[subdivisionIteration].Length, Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "Number of Triangles: " + iterationsList[subdivisionIteration].Length / 3, Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "Size of Iteration Buffer: " + iterationsList.Count, Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "UP/DOWN - Subdivision Iteration: " + subdivisionIteration, Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "d/D - Change Displacement Height: " + textureDisplacement.ToString("0.00"), Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "Showing Heightmap Colors: " + toggleHeightColor, Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "Showing the Texture Used: " + toggleTexture, Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Black);
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
                _spriteBatch.DrawString(font, "UP: Subdivide the Triangles", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "C: Clear Triangle Colors", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "E: Toggle Height Map Colors", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "R: Toggle Height Map Colors", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
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

            Vector2 uv0 = Vector2.Zero,
                    uv1 = Vector2.Zero,
                    uv2 = Vector2.Zero;

            List<VertexPositionColorNormalTexture> subdivisionVertices = new List<VertexPositionColorNormalTexture>();
            int j = 0;
            for (int i = 0; i < vertices.Count; i++)
            {
                if (i % 3 == 0) { vertex0 = vertices[i].Position; uv0 = vertices[i].TextureCoordinate; j = 1; }
                else if (i % 3 == 1) { vertex1 = vertices[i].Position; uv1 = vertices[i].TextureCoordinate; j = 2; }
                else { vertex2 = vertices[i].Position; uv2 = vertices[i].TextureCoordinate; j = 3; }

                if(j == 3)
                {
                    // Keeping top edge
                    vertexColor.R = (byte)random.Next(0, 255);
                    vertexColor.G = (byte)random.Next(0, 255);
                    vertexColor.B = (byte)random.Next(0, 255);
                    subdivisionVertices.Add(new VertexPositionColorNormalTexture(vertex0, vertexColor, Vector3.Up, new Vector2(uv0.X, uv0.Y)));
                    subdivisionVertices.Add(new VertexPositionColorNormalTexture((vertex0 + vertex1) / 2, vertexColor, Vector3.Up, new Vector2((uv0.X + uv1.X) / 2, (uv0.Y + uv1.Y) / 2)));
                    subdivisionVertices.Add(new VertexPositionColorNormalTexture((vertex0 + vertex2) / 2, vertexColor, Vector3.Up, new Vector2((uv0.X + uv2.X) / 2, (uv0.Y + uv2.Y) / 2)));
                    triangleColor = !triangleColor;
                    //vertexColor = triangleColor ? Color.Gray : Color.LightGray;

                    // Middle triangle
                    vertexColor.R = (byte)random.Next(0, 255);
                    vertexColor.G = (byte)random.Next(0, 255);
                    vertexColor.B = (byte)random.Next(0, 255);
                    subdivisionVertices.Add(new VertexPositionColorNormalTexture((vertex0 + vertex1) / 2, vertexColor, Vector3.Up, new Vector2((uv0.X + uv1.X) / 2, (uv0.Y + uv1.Y) / 2)));
                    subdivisionVertices.Add(new VertexPositionColorNormalTexture((vertex0 + vertex2) / 2, vertexColor, Vector3.Up, new Vector2((uv0.X + uv2.X) / 2, (uv0.Y + uv2.Y) / 2)));
                    subdivisionVertices.Add(new VertexPositionColorNormalTexture((vertex1 + vertex2) / 2, vertexColor, Vector3.Up, new Vector2((uv1.X + uv2.X) / 2, (uv1.Y + uv2.Y) / 2)));
                    triangleColor = !triangleColor;
                    //vertexColor = triangleColor ? Color.Gray : Color.LightGray;

                    // Hypotenuse bottom
                    vertexColor.R = (byte)random.Next(0, 255);
                    vertexColor.G = (byte)random.Next(0, 255);
                    vertexColor.B = (byte)random.Next(0, 255);
                    subdivisionVertices.Add(new VertexPositionColorNormalTexture(vertex1, vertexColor, Vector3.Up, new Vector2(uv1.X, uv1.Y)));
                    subdivisionVertices.Add(new VertexPositionColorNormalTexture((vertex0 + vertex1) / 2, vertexColor, Vector3.Up, new Vector2((uv0.X + uv1.X) / 2, (uv0.Y + uv1.Y) / 2)));
                    subdivisionVertices.Add(new VertexPositionColorNormalTexture((vertex1 + vertex2) / 2, vertexColor, Vector3.Up, new Vector2((uv1.X + uv2.X) / 2, (uv1.Y + uv2.Y) / 2)));
                    triangleColor = !triangleColor;
                    //vertexColor = triangleColor ? Color.Gray : Color.LightGray;

                    // Hypotenuse top
                    vertexColor.R = (byte)random.Next(0, 255);
                    vertexColor.G = (byte)random.Next(0, 255);
                    vertexColor.B = (byte)random.Next(0, 255);
                    subdivisionVertices.Add(new VertexPositionColorNormalTexture(vertex2, vertexColor, Vector3.Up, new Vector2(uv2.X, uv2.Y)));
                    subdivisionVertices.Add(new VertexPositionColorNormalTexture((vertex0 + vertex2) / 2, vertexColor, Vector3.Up, new Vector2((uv0.X + uv2.X) / 2, (uv0.Y + uv2.Y) / 2)));
                    subdivisionVertices.Add(new VertexPositionColorNormalTexture((vertex2 + vertex1) / 2, vertexColor, Vector3.Up, new Vector2((uv2.X + uv1.X) / 2, (uv2.Y + uv1.Y) / 2)));
                    triangleColor = !triangleColor;
                    //vertexColor = triangleColor ? Color.Gray : Color.LightGray;

                    vertex0 = vertex1 = vertex2 = Vector3.Zero;
                    uv0 = uv1 = uv2 = Vector2.Zero;
                    j = 0;
                }
            }

            iterationsList.Add(subdivisionVertices.ToArray());
            vertices = subdivisionVertices;
        }

        /// <summary>
        /// This method makes all triangles white
        /// </summary>
        private void ClearAllTriangles()
        {
            for(int i = 0; i < iterationsList[subdivisionIteration].Length; i++)
            {
                Vector3 vertexPosition = iterationsList[subdivisionIteration][i].Position;
                Vector2 textureCoordinate = iterationsList[subdivisionIteration][i].TextureCoordinate;
                iterationsList[subdivisionIteration][i] = new VertexPositionColorNormalTexture(vertexPosition, Color.White, Vector3.Up, textureCoordinate);
            }
        }

        /// <summary>
        /// This method will toggle between the color white and the colors being visualized by RBGW coloring
        /// </summary>
        private void VisualizeTriangles()
        {
            int colorNumber = 0; // 0 is red, 1 is green, 2 is blue, 3 is white
            int j = 0;
            Color vertexColor = new Color(255, 255, 255);
            for (int i = 0; i < iterationsList[subdivisionIteration].Length; i++)
            {
                Vector3 vertexPosition = iterationsList[subdivisionIteration][i].Position;
                Vector2 textureCoordinate = iterationsList[subdivisionIteration][i].TextureCoordinate;
                if (!areVerticesColorful)
                {
                    switch (colorNumber)
                    {
                        case 0:
                            iterationsList[subdivisionIteration][i] = new VertexPositionColorNormalTexture(vertexPosition, Color.Gray, Vector3.Up, textureCoordinate);
                            break;

                        case 1:
                            iterationsList[subdivisionIteration][i] = new VertexPositionColorNormalTexture(vertexPosition, Color.LightGray, Vector3.Up, textureCoordinate);
                            break;

                        case 2:
                            iterationsList[subdivisionIteration][i] = new VertexPositionColorNormalTexture(vertexPosition, Color.LightSlateGray, Vector3.Up, textureCoordinate);
                            break;

                        case 3:
                            iterationsList[subdivisionIteration][i] = new VertexPositionColorNormalTexture(vertexPosition, Color.WhiteSmoke, Vector3.Up, textureCoordinate);
                            break;

                        case 4:
                            iterationsList[subdivisionIteration][i] = new VertexPositionColorNormalTexture(vertexPosition, Color.GhostWhite, Vector3.Up, textureCoordinate);
                            break;

                        case 5:
                            iterationsList[subdivisionIteration][i] = new VertexPositionColorNormalTexture(vertexPosition, Color.DimGray, Vector3.Up, textureCoordinate);
                            break;

                        case 6:
                            iterationsList[subdivisionIteration][i] = new VertexPositionColorNormalTexture(vertexPosition, Color.Black, Vector3.Up, textureCoordinate);
                            break;

                        default:
                            iterationsList[subdivisionIteration][i] = new VertexPositionColorNormalTexture(vertexPosition, Color.White, Vector3.Up, textureCoordinate);
                            break;
                    }
                }
                else
                {
                    if(j == 0)
                    {
                        vertexColor.R = (byte)random.Next(0, 255);
                        vertexColor.G = (byte)random.Next(0, 255);
                        vertexColor.B = (byte)random.Next(0, 255);
                    }

                    iterationsList[subdivisionIteration][i] = new VertexPositionColorNormalTexture(vertexPosition, vertexColor, Vector3.Up, textureCoordinate);
                }

                if(j == 2)
                {
                    if (colorNumber == 6) colorNumber = 0;
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

        // Creates the triange for rendering
        private VertexBuffer GenerateVertices()
        {
            VertexPositionColorNormalTexture[] vertexArray = new VertexPositionColorNormalTexture[]
            {
                new VertexPositionColorNormalTexture(new Vector3(0, 10, 0), Color.Gray, Vector3.Up, new Vector2(0, 10)),       // Top left
                new VertexPositionColorNormalTexture(new Vector3(10, 0, 0), Color.Gray, Vector3.Up, new Vector2(10, 0)),        // Top right
                new VertexPositionColorNormalTexture(new Vector3(-10, 0, 0), Color.Gray, Vector3.Up, new Vector2(-10, 0)),       // Bottom right
            };

            //var vertices = new VertexPositionTexture[] {
            //    new VertexPositionTexture(new Vector3( 0, 1, 0), new Vector2(0, 0)),
            //    new VertexPositionTexture(new Vector3( 1, 0, 0), new Vector2(0, 1)),
            //    new VertexPositionTexture(new Vector3(-1, 0, 0), new Vector2(1, 1)),
            //};

            var vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColorNormalTexture), vertexArray.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertexArray);
            return vertexBuffer;
        }
    }
}