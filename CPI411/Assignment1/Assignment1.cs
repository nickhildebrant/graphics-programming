using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Assignment1
{
    public class Assignment1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        SpriteFont font;

        Model currentModel;
        Model box, bunny, sphere, teapot, torus;
        Effect effect;

        string shaderName = "Gouraud - Per Vertex";
        string modelName = "Cube";

        Matrix world, view, projection;

        Vector3 cameraPosition;
        float cameraX = 0, cameraY = 0;

        Vector4 ambient = new Vector4(0, 0, 0, 0);
        float ambientIntensity = 0.1f;
        Vector4 diffuseColor = new Vector4(1, 1, 1, 1);
        Vector3 diffuseLightDirection = new Vector3(1, 1, 1);
        float diffuseIntensity = 1.0f;

        Vector3 lightDirection = new Vector3(0.5f, 0.6f, 0.4f);

        Vector4 specularColor = new Vector4(1, 1, 1, 1);
        float specularIntensity = 1.0f;
        float shininess = 20f;

        float cameraAngle1, cameraAngle2;
        float lightAngle1, lightAngle2;
        float distance = 1f;

        MouseState previousMouseState;
        KeyboardState previousKeyboardState;

        int currTechnique = 0;

        bool showInfo = true, showHelp = true;

        public Assignment1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("font");

            effect = Content.Load<Effect>("SimpleShading");

            box = Content.Load<Model>("box");
            bunny = Content.Load<Model>("bunny");
            sphere = Content.Load<Model>("sphere");
            teapot = Content.Load<Model>("teapot");
            torus = Content.Load<Model>("Torus");

            currentModel = box;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

            // Resets Camera & Light
            if (Keyboard.GetState().IsKeyDown(Keys.S) && previousKeyboardState.IsKeyUp(Keys.S))
            {
                cameraAngle1 = 0;
                cameraAngle2 = 0;

                cameraX = 0;
                cameraY = 0;

                distance = 1;

                lightDirection = new Vector3(0.5f, 0.6f, 0.4f);

                lightAngle1 = 0;
                lightAngle2 = 0;

                diffuseColor = new Vector4(1, 1, 1, 1);
                specularColor = new Vector4(1, 1, 1, 1);
                specularIntensity = 1f;
                diffuseIntensity = 1f;
            }

            // Camera rotation controller
            if (Mouse.GetState().LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Pressed)
            {
                cameraAngle2 -= (previousMouseState.X - Mouse.GetState().X) / 100f;
                cameraAngle1 -= (previousMouseState.Y - Mouse.GetState().Y) / 100f;
            }

            // Camera distance controller
            if (Mouse.GetState().RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Pressed)
            {
                distance += (float)gameTime.ElapsedGameTime.TotalSeconds * (Mouse.GetState().Y - previousMouseState.Y);
            }

            // Camera translation controller
            if (Mouse.GetState().MiddleButton == ButtonState.Pressed && previousMouseState.MiddleButton == ButtonState.Pressed)
            {
                cameraX += (Mouse.GetState().X - previousMouseState.X) / 100f;
                cameraY += (Mouse.GetState().Y - previousMouseState.Y) / 100f;
            }

            cameraPosition = Vector3.Transform(new Vector3(cameraX, cameraY, 20), Matrix.CreateRotationX(cameraAngle1) * Matrix.CreateRotationY(cameraAngle2));
            view = Matrix.CreateLookAt(distance * cameraPosition, new Vector3(cameraX, cameraY, 0), Vector3.Up);
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), GraphicsDevice.Viewport.AspectRatio, 0.1f, 1000);

            // Left decrease lightX
            if (Keyboard.GetState().IsKeyDown(Keys.Left)) { lightDirection.X -= 0.1f; lightAngle2 -= 0.1f; }

            // Right increase lightX
            if (Keyboard.GetState().IsKeyDown(Keys.Right)) { lightDirection.X += 0.1f; lightAngle2 += 0.1f; }

            // Up increase lightY
            if (Keyboard.GetState().IsKeyDown(Keys.Up)) { lightDirection.Y += 0.1f; lightAngle1 -= 0.1f; }

            // Down decrease lightY
            if (Keyboard.GetState().IsKeyDown(Keys.Down)) { lightDirection.Y -= 0.1f; lightAngle1 += 0.1f; }

            lightDirection = Vector3.Transform(new Vector3(0.5f, 0.6f, 0.4f), Matrix.CreateRotationX(lightAngle1) * Matrix.CreateRotationY(lightAngle2));

            // Increase Diffuse Intensity
            if (Keyboard.GetState().IsKeyDown(Keys.L) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift)) { diffuseIntensity += 0.01f; }

            // Decrease Diffuse Intensity
            if (Keyboard.GetState().IsKeyDown(Keys.L) && Keyboard.GetState().IsKeyDown(Keys.LeftShift) && diffuseIntensity > 0) { diffuseIntensity -= 0.01f; }

            // Increase red in light
            if (Keyboard.GetState().IsKeyDown(Keys.R) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift) && specularColor.X < 1.0f) { specularColor.X += 0.01f; diffuseColor.X += 0.01f; }

            // Decreases red in light
            if (Keyboard.GetState().IsKeyDown(Keys.R) && Keyboard.GetState().IsKeyDown(Keys.LeftShift) && specularColor.X > 0f) { specularColor.X -= 0.01f; diffuseColor.X -= 0.01f; }

            // Increase green in light
            if (Keyboard.GetState().IsKeyDown(Keys.G) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift) && specularColor.Y < 1.0f) { specularColor.Y += 0.01f; diffuseColor.Y += 0.01f; }

            // Decreases green in light
            if (Keyboard.GetState().IsKeyDown(Keys.G) && Keyboard.GetState().IsKeyDown(Keys.LeftShift) && specularColor.Y > 0f) { specularColor.Y -= 0.01f; diffuseColor.Y -= 0.01f; }

            // Increase blue in light
            if (Keyboard.GetState().IsKeyDown(Keys.B) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift) && specularColor.Z < 1.0f) { specularColor.Z += 0.01f; diffuseColor.Z += 0.01f; }

            // Decreases blue in light
            if (Keyboard.GetState().IsKeyDown(Keys.B) && Keyboard.GetState().IsKeyDown(Keys.LeftShift) && specularColor.Z > 0f) { specularColor.Z -= 0.01f; diffuseColor.Z -= 0.01f; }

            // Info UI
            if (Keyboard.GetState().IsKeyDown(Keys.H) && previousKeyboardState.IsKeyUp(Keys.H)) { showInfo = !showInfo; }

            // Info UI
            if (Keyboard.GetState().IsKeyDown(Keys.OemQuestion) && previousKeyboardState.IsKeyUp(Keys.OemQuestion)) { showHelp = !showHelp; }

            // 1 - Box Model
            if (Keyboard.GetState().IsKeyDown(Keys.D1)) { currentModel = box; modelName = "Box"; }

            // 2 - Sphere Model
            if (Keyboard.GetState().IsKeyDown(Keys.D2)) { currentModel = sphere; modelName = "Sphere"; }

            // 3 - Torus Model
            if (Keyboard.GetState().IsKeyDown(Keys.D3)) { currentModel = torus; modelName = "Torus"; }

            // 4 - Teapot Model
            if (Keyboard.GetState().IsKeyDown(Keys.D4)) { currentModel = teapot; modelName = "Teapot"; }

            // 5 - Bunny Model
            if (Keyboard.GetState().IsKeyDown(Keys.D5)) { currentModel = bunny; modelName = "Bunny"; }

            // F1 - Gouraud shader
            if (Keyboard.GetState().IsKeyDown(Keys.F1)) { currTechnique = 0; shaderName = "Gouraud - Per Vertex"; }

            // F2 - Phong Shader
            if (Keyboard.GetState().IsKeyDown(Keys.F2)) { currTechnique = 1; shaderName = "Phong"; }

            // F3 - Phong-Blinn Shader
            if (Keyboard.GetState().IsKeyDown(Keys.F3)) { currTechnique = 2; shaderName = "Phong-Blinn"; }

            // F4 - Schlick Shader
            if (Keyboard.GetState().IsKeyDown(Keys.F4)) { currTechnique = 3; shaderName = "Schlick"; }

            // F5 - Toon Shader
            if (Keyboard.GetState().IsKeyDown(Keys.F5)) { currTechnique = 4; shaderName = "Toon"; }

            // F6 - Half Life Shader
            if (Keyboard.GetState().IsKeyDown(Keys.F6)) { currTechnique = 5; shaderName = "Half-Life"; }

            // + Increase specular intensity
            if (!Keyboard.GetState().IsKeyDown(Keys.LeftShift) && !Keyboard.GetState().IsKeyDown(Keys.LeftControl) && Keyboard.GetState().IsKeyDown(Keys.OemPlus)) 
            { specularIntensity += 0.2f; }

            // - Decrease specular intensity
            if (!Keyboard.GetState().IsKeyDown(Keys.LeftShift) && !Keyboard.GetState().IsKeyDown(Keys.LeftControl) && Keyboard.GetState().IsKeyDown(Keys.OemMinus)) 
            { specularIntensity -= 0.2f; }

            // L-Ctrl + Increase shininess
            if (!Keyboard.GetState().IsKeyDown(Keys.LeftShift) && Keyboard.GetState().IsKeyDown(Keys.LeftControl) && Keyboard.GetState().IsKeyDown(Keys.OemPlus)) 
            { shininess += 0.2f; }

            // L-Ctrl - Decrease shininess
            if (!Keyboard.GetState().IsKeyDown(Keys.LeftShift) && Keyboard.GetState().IsKeyDown(Keys.LeftControl) && Keyboard.GetState().IsKeyDown(Keys.OemMinus)) 
            { shininess -= 0.2f; }

            // L-Shift + Increase light intensity
            if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) && !Keyboard.GetState().IsKeyDown(Keys.LeftControl) && Keyboard.GetState().IsKeyDown(Keys.OemPlus))
            { diffuseIntensity += 0.2f; }

            // L-Shift - Decrease light intensity
            if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) && !Keyboard.GetState().IsKeyDown(Keys.LeftControl) && Keyboard.GetState().IsKeyDown(Keys.OemMinus))
            { diffuseIntensity -= 0.2f; }

            previousMouseState = Mouse.GetState();
            previousKeyboardState = Keyboard.GetState();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = new DepthStencilState();

            effect.CurrentTechnique = effect.Techniques[currTechnique]; // 0 per vertex, 1 per pixel
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in currentModel.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        effect.Parameters["World"].SetValue(mesh.ParentBone.Transform);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);

                        effect.Parameters["AmbientColor"].SetValue(ambient);
                        effect.Parameters["AmbientIntensity"].SetValue(ambientIntensity);
                        effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
                        effect.Parameters["DiffuseIntensity"].SetValue(diffuseIntensity);
                        effect.Parameters["DiffuseLightDirection"].SetValue(diffuseLightDirection);

                        effect.Parameters["SpecularColor"].SetValue(specularColor);
                        effect.Parameters["SpecularIntensity"].SetValue(specularIntensity);
                        effect.Parameters["Shininess"].SetValue(shininess);

                        effect.Parameters["LightPosition"].SetValue(lightDirection);
                        effect.Parameters["CameraPosition"].SetValue(cameraPosition);

                        Matrix worldInverseTranspose = Matrix.Transpose(Matrix.Invert(mesh.ParentBone.Transform));
                        effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTranspose);
                        pass.Apply();

                        GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                        GraphicsDevice.Indices = part.IndexBuffer;

                        GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
                    }
                }
            }

            _spriteBatch.Begin();
            if (showInfo)
            {
                _spriteBatch.DrawString(font, "Camera Position: (" + cameraPosition.X.ToString("0.00") + ", " + cameraPosition.Y.ToString("0.00") + ", " + cameraPosition.Z.ToString("0.00") + ")", Vector2.UnitX + Vector2.UnitY * 5, Color.White);
                _spriteBatch.DrawString(font, "Camera Angle: (" + cameraAngle2.ToString("0.00") + ", " + cameraAngle1.ToString("0.00") + ")", Vector2.UnitX + Vector2.UnitY * 25, Color.White);
                _spriteBatch.DrawString(font, "Light Angle: (" + lightAngle2.ToString("0.00") + ", " + lightAngle1.ToString("0.00") + ")", Vector2.UnitX + Vector2.UnitY * 45, Color.White);
                _spriteBatch.DrawString(font, "Shader Type: " + shaderName, Vector2.UnitX + Vector2.UnitY * 65, Color.White);
                _spriteBatch.DrawString(font, "Specular Intensity: " + specularIntensity.ToString("0.00"), Vector2.UnitX + Vector2.UnitY * 85, Color.White);
                _spriteBatch.DrawString(font, "Light Intensity: " + diffuseIntensity.ToString("0.00"), Vector2.UnitX + Vector2.UnitY * 105, Color.White);
                _spriteBatch.DrawString(font, "Light Color: (" + specularColor.X.ToString("0.00") + ", " + specularColor.Y.ToString("0.00") + ", " + specularColor.Z.ToString("0.00") + ")", Vector2.UnitX + Vector2.UnitY * 125, Color.White);
                _spriteBatch.DrawString(font, "Shininess: " + shininess.ToString("0.00"), Vector2.UnitX + Vector2.UnitY * 145, Color.White);
            }

            if (showHelp)
            {
                _spriteBatch.DrawString(font, "Press H to show/hide the Info Menu", Vector2.UnitX * 500 + Vector2.UnitY * 5, Color.White);
                _spriteBatch.DrawString(font, "Press ? to show/hide the Help Menu", Vector2.UnitX * 500 + Vector2.UnitY * 25, Color.White);
                _spriteBatch.DrawString(font, "Left Click + Drag Rotates the Camera", Vector2.UnitX * 500 + Vector2.UnitY * 45, Color.White);
                _spriteBatch.DrawString(font, "Right Click + Drag Zooms In/Out", Vector2.UnitX * 500 + Vector2.UnitY * 65, Color.White);
                _spriteBatch.DrawString(font, "Middle Mouse + Drag Translates Camera", Vector2.UnitX * 500 + Vector2.UnitY * 85, Color.White);
                _spriteBatch.DrawString(font, "Arrow Keys Rotates the Light", Vector2.UnitX * 500 + Vector2.UnitY * 105, Color.White);
                _spriteBatch.DrawString(font, "S Key: Resets the Camera and Light", Vector2.UnitX * 500 + Vector2.UnitY * 125, Color.White);
                _spriteBatch.DrawString(font, "Hold Shift to Decrease the Below Values", Vector2.UnitX * 500 + Vector2.UnitY * 145, Color.White);
                _spriteBatch.DrawString(font, "L Key: Intensity of Light", Vector2.UnitX * 500 + Vector2.UnitY * 165, Color.White);
                _spriteBatch.DrawString(font, "R Key: Red Value of Light", Vector2.UnitX * 500 + Vector2.UnitY * 185, Color.White);
                _spriteBatch.DrawString(font, "G Key: Green Value of Light", Vector2.UnitX * 500 + Vector2.UnitY * 205, Color.White);
                _spriteBatch.DrawString(font, "B Key: Blue Value of Light", Vector2.UnitX * 500 + Vector2.UnitY * 225, Color.White);
                _spriteBatch.DrawString(font, "+/- : Specular Intensity", Vector2.UnitX * 500 + Vector2.UnitY * 245, Color.White);
                _spriteBatch.DrawString(font, "L-CTRL & +/- : Shininess", Vector2.UnitX * 500 + Vector2.UnitY * 265, Color.White);
                _spriteBatch.DrawString(font, "1-2-3-4-5: Change Model", Vector2.UnitX * 500 + Vector2.UnitY * 285, Color.White);
                _spriteBatch.DrawString(font, "F1-F2-F3-F4-F5-F6 Change Shader", Vector2.UnitX * 500 + Vector2.UnitY * 305, Color.White);
            }
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}