using CPI411.SimpleEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Assignment3
{
    public class Assignment3 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Model model;

        Effect effect;
        SpriteFont font;

        bool showInfo = true, showHelp = true;

        Matrix world = Matrix.Identity;
        Matrix view, projection;

        Vector3 cameraPosition;
        Vector3 cameraTarget;

        Vector4 ambient = new Vector4(0, 0, 0, 0);
        float ambientIntensity = 0.1f;
        Vector4 diffuseColor = new Vector4(1, 1, 1, 1);
        float diffuseIntensity = 1.0f;

        Vector3 lightPosition = new Vector3(0, 1, 0);
        Matrix lightView;
        Matrix lightProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1f, 1f, 50f);

        Vector4 specularColor = new Vector4(1, 1, 1, 1);

        float cameraAngleX, cameraAngleY;
        float lightAngleX, lightAngleY;
        float distance = 15f;

        Skybox skybox;

        Texture[] normalMaps;
        int normalMapNumber = 0;
        string pictureName = "Art";

        int shaderTechnique = 0;
        string shaderName = "Reflection Shader";

        float bumpHeight;
        float etaRatio = 0.658f;

        MouseState previousMouseState;
        KeyboardState previousKeyboardState;

        public Assignment3()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), GraphicsDevice.Viewport.AspectRatio, 0.1f, 1000);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("font");

            effect = Content.Load<Effect>("NormalEffects");
            model = Content.Load<Model>("Torus");

            // Loading normal maps
            normalMaps = new Texture[8]
            {
                Content.Load<Texture>("art"),
                Content.Load<Texture>("BumpTest"),
                Content.Load<Texture>("crossHatch"),
                Content.Load<Texture>("monkey"),
                Content.Load<Texture>("round"),
                Content.Load<Texture>("saint"),
                Content.Load<Texture>("science"),
                Content.Load<Texture>("square"),
            };


            // Loading office skybox
            string[] officeSkyboxTextures =
            {
                "Office/nvlobby_new_posx", "Office/nvlobby_new_negx",
                "Office/nvlobby_new_negy", "Office/nvlobby_new_posy",
                "Office/nvlobby_new_posz", "Office/nvlobby_new_negz"
            };
            skybox = new Skybox(officeSkyboxTextures, Content, GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

            // Shaders
            if (Keyboard.GetState().IsKeyDown(Keys.F1)) shaderTechnique = 0;
            if (Keyboard.GetState().IsKeyDown(Keys.F2)) shaderTechnique = 1;
            if (Keyboard.GetState().IsKeyDown(Keys.F3)) shaderTechnique = 2;
            if (Keyboard.GetState().IsKeyDown(Keys.F4)) shaderTechnique = 3;
            if (Keyboard.GetState().IsKeyDown(Keys.F5)) shaderTechnique = 4;

            // Normal maps
            if (Keyboard.GetState().IsKeyDown(Keys.D1)) { normalMapNumber = 0; pictureName = "Art"; }
            if (Keyboard.GetState().IsKeyDown(Keys.D2)) { normalMapNumber = 1; pictureName = "BumpTest"; }
            if (Keyboard.GetState().IsKeyDown(Keys.D3)) { normalMapNumber = 2; pictureName = "CrossHatch"; }
            if (Keyboard.GetState().IsKeyDown(Keys.D4)) { normalMapNumber = 3; pictureName = "Monkey"; }
            if (Keyboard.GetState().IsKeyDown(Keys.D5)) { normalMapNumber = 4; pictureName = "Round"; }
            if (Keyboard.GetState().IsKeyDown(Keys.D6)) { normalMapNumber = 5; pictureName = "Saint"; }
            if (Keyboard.GetState().IsKeyDown(Keys.D7)) { normalMapNumber = 6; pictureName = "Science"; }
            if (Keyboard.GetState().IsKeyDown(Keys.D8)) { normalMapNumber = 7; pictureName = "Square"; }
            //if (Keyboard.GetState().IsKeyDown(Keys.D9)) { normalMapNumber = 8; pictureName = "NM"; }

            // Info UI + Help UI
            if (Keyboard.GetState().IsKeyDown(Keys.H) && !previousKeyboardState.IsKeyDown(Keys.H)) { showInfo = !showInfo; }
            if (Keyboard.GetState().IsKeyDown(Keys.OemQuestion) && !previousKeyboardState.IsKeyDown(Keys.OemQuestion)) { showHelp = !showHelp; }

            // Reset the camera
            if (Keyboard.GetState().IsKeyDown(Keys.S)) { cameraAngleX = cameraAngleY = lightAngleX = lightAngleY = 0; distance = 10; cameraTarget = Vector3.Zero; }

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

            if (Keyboard.GetState().IsKeyDown(Keys.Up)) lightAngleX += 1.0f;
            if (Keyboard.GetState().IsKeyDown(Keys.Down)) lightAngleX -= 1.0f;
            if (Keyboard.GetState().IsKeyDown(Keys.Left)) lightAngleY += 1.0f;
            if (Keyboard.GetState().IsKeyDown(Keys.Right)) lightAngleY -= 1.0f;

            lightPosition = Vector3.Transform(new Vector3(0, 0, 10), Matrix.CreateRotationX(lightAngleY) * Matrix.CreateRotationY(lightAngleX));
            lightView = Matrix.CreateLookAt(lightPosition, Vector3.Zero, Vector3.Transform(Vector3.UnitY, Matrix.CreateRotationX(lightAngleY) * Matrix.CreateRotationY(lightAngleX)));

            previousMouseState = Mouse.GetState();
            previousKeyboardState = Keyboard.GetState();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            RasterizerState originalRasterizerState = _graphics.GraphicsDevice.RasterizerState;
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            _graphics.GraphicsDevice.RasterizerState = rasterizerState;
            skybox.Draw(view, projection, cameraPosition);
            _graphics.GraphicsDevice.RasterizerState = originalRasterizerState;

            DrawModelWithEffect();

            _spriteBatch.Begin();
            if (showInfo)
            {
                int i = 0;
                _spriteBatch.DrawString(font, "Camera Position: (" + cameraPosition.X.ToString("0.00") + ", " + cameraPosition.Y.ToString("0.00") + ", " + cameraPosition.Z.ToString("0.00") + ")", Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "Camera Angle: (" + cameraAngleX.ToString("0.00") + ", " + cameraAngleY.ToString("0.00") + ")", Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "Light Angle: (" + lightAngleX.ToString("0.00") + ", " + lightAngleY.ToString("0.00") + ")", Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "Shader Type: " + shaderName, Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Black);
            }
            if (showHelp)
            {
                int i = 0;
                _spriteBatch.DrawString(font, "Press H to show/hide the Info Menu", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "Press ? to show/hide the Help Menu", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "Left Click + Drag Rotates the Camera", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "Right Click + Drag Zooms In/Out", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "Middle Mouse + Drag Translates Camera", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "Arrow Keys Rotates the Light", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "S Key: Resets the Camera and Light", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "Hold Shift to Decrease the Below Values", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "Q Key: Fresnel Power", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "W Key: Fresnel Scale", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "E Key: Fresnel Bias", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "R Key: ETA Red Ratio", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "G Key: ETA Black Ratio", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "B Key: ETA Blue Ratio", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "+/- : Reflectivity", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "1-2-3-4-5-6: Change Model", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "7-8-9-0: Change Skybox", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "F7-F8-F9-F10 Change Shader", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
            }
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        void DrawModelWithEffect()
        {
            effect.CurrentTechnique = effect.Techniques[shaderTechnique];
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        effect.Parameters["World"].SetValue(mesh.ParentBone.Transform);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);

                        Matrix worldInverseTransposeMatrix = Matrix.Transpose(Matrix.Invert(mesh.ParentBone.Transform));
                        effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTransposeMatrix);

                        effect.Parameters["CameraPosition"].SetValue(cameraPosition);
                        effect.Parameters["LightPosition"].SetValue(lightPosition);

                        effect.Parameters["DiffuseIntensity"].SetValue(1.0f);
                        effect.Parameters["DiffuseColor"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                        effect.Parameters["SpecularIntensity"].SetValue(1.0f);
                        effect.Parameters["SpecularColor"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                        effect.Parameters["Shininess"].SetValue(100.0f);

                        effect.Parameters["normalMap"].SetValue(normalMaps[normalMapNumber]);

                        pass.Apply();
                        GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                        GraphicsDevice.Indices = part.IndexBuffer;
                        GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
                    }
                }
            }
        }
    }
}