using CPI411.SimpleEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Assignment2
{
    public class Assignment2 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Model currentModel;
        Model box, bunny, sphere, teapot, torus, helicopter;
        string modelName = "Helicopter";

        Effect effect;
        Texture2D texture;
        SpriteFont font;

        bool showInfo = true, showHelp = true;

        Matrix world, view, projection;

        Vector3 cameraPosition;
        Vector3 cameraTarget;

        Vector4 ambient = new Vector4(0, 0, 0, 0);
        float ambientIntensity = 0.1f;
        Vector4 diffuseColor = new Vector4(1, 1, 1, 1);
        Vector3 diffuseLightDirection = new Vector3(1, 1, 1);
        float diffuseIntensity = 1.0f;

        Vector3 lightPosition = new Vector3(0, 0, 1);
        Vector3 lightDirection = new Vector3(0.5f, 0.6f, 0.4f);
        Matrix lightView;
        Matrix lightProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1f, 1f, 50f);

        Vector4 specularColor = new Vector4(1, 1, 1, 1);
        float specularIntensity = 1.0f;
        float shininess = 20f;

        float cameraAngleX, cameraAngleY;
        float lightAngleX, lightAngleY;
        float distance = 15f;

        int skyboxNumber = 0;
        string skyboxName = "Test Skybox";
        Skybox currentSkybox, testSkybox, officeSkybox, daytimeSkybox, selfSkybox;

        int shaderNumber = 0;
        string shaderName = "Reflection Shader";

        float reflectionIntensity = 0.5f;

        float fresnelPower = 2;
        float fresnelScale = 10;
        float fresnelBias = 0.5f;

        float redRatio = 0.1f;
        float greenRatio = 0.1f;
        float blueRatio = 0.1f;

        bool showTexture = false;

        MouseState previousMouseState;
        KeyboardState previousKeyboardState;

        public Assignment2()
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

            effect = Content.Load<Effect>("LightingShader");
            helicopter = Content.Load<Model>("Helicopter");
            box = Content.Load<Model>("box");
            bunny = Content.Load<Model>("bunnyUV");
            sphere = Content.Load<Model>("sphere");
            teapot = Content.Load<Model>("teapot");
            torus = Content.Load<Model>("Torus");
            texture = Content.Load<Texture2D>("HelicopterTexture");

            // Loading Test Skybox
            string[] testSkyboxTextures =
            {
                "Test/debug_posx", "Test/debug_negx",
                "Test/debug_posy", "Test/debug_negy",
                "Test/debug_posz", "Test/debug_negz",
            };
            testSkybox = new Skybox(testSkyboxTextures, 256, Content, GraphicsDevice);

            // Loading office skybox
            string[] officeSkyboxTextures =
            {
                "Office/nvlobby_new_posx", "Office/nvlobby_new_negx",
                "Office/nvlobby_new_posy", "Office/nvlobby_new_negy",
                "Office/nvlobby_new_posz", "Office/nvlobby_new_negz"
            };
            officeSkybox = new Skybox(officeSkyboxTextures, Content, GraphicsDevice);

            // loading daytime skybox
            string[] daytimeSkyboxTextures =
            {
                "Daytime/grandcanyon_posx", "Daytime/grandcanyon_negx",
                "Daytime/grandcanyon_posy", "Daytime/grandcanyon_negy",
                "Daytime/grandcanyon_posz", "Daytime/grandcanyon_negz"
            };
            daytimeSkybox = new Skybox(daytimeSkyboxTextures, Content, GraphicsDevice);

            // loading the self skybox
            string[] selfSkyboxTextures =
            {
                "Self/hills_posx", "Self/hills_negx",
                "Self/hills_posy", "Self/hills_negy",
                "Self/hills_posz", "Self/hills_negz",
            };
            selfSkybox = new Skybox(selfSkyboxTextures, Content, GraphicsDevice);

            string[] skyboxTextures =
            {
                "skybox/SunsetPNG2",
                "skybox/SunsetPNG1",
                "skybox/SunsetPNG4",
                "skybox/SunsetPNG3",
                "skybox/SunsetPNG6",
                "skybox/SunsetPNG5",
            };

            currentSkybox = new Skybox(skyboxTextures, Content, GraphicsDevice);
            skyboxNumber = 20;

            currentModel = helicopter;
            //currentSkybox = testSkybox;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.D1)) { currentModel = box; modelName = "Box"; }
            if (Keyboard.GetState().IsKeyDown(Keys.D2)) { currentModel = sphere; modelName = "Sphere"; }
            if (Keyboard.GetState().IsKeyDown(Keys.D3)) { currentModel = torus; modelName = "Torus"; }
            if (Keyboard.GetState().IsKeyDown(Keys.D4)) { currentModel = teapot; modelName = "Teapot"; }
            if (Keyboard.GetState().IsKeyDown(Keys.D5)) { currentModel = bunny; modelName = "Bunny"; }
            if (Keyboard.GetState().IsKeyDown(Keys.D6)) { currentModel = helicopter; modelName = "Helicopter"; }

            if (Keyboard.GetState().IsKeyDown(Keys.F7)) { shaderNumber = 0; shaderName = "Reflection Shader"; }
            if (Keyboard.GetState().IsKeyDown(Keys.F8)) { shaderNumber = 1; shaderName = "Refraction Shader"; }
            if (Keyboard.GetState().IsKeyDown(Keys.F9)) { shaderNumber = 2; shaderName = "Refraction + Dispersion Shader"; }
            if (Keyboard.GetState().IsKeyDown(Keys.F10)) { shaderNumber = 3; shaderName = "Fresnel Shader"; }

            if (Keyboard.GetState().IsKeyDown(Keys.D7)) { skyboxNumber = 0; skyboxName = "Test Colors"; }
            if (Keyboard.GetState().IsKeyDown(Keys.D8)) { skyboxNumber = 1; skyboxName = "Office Room"; }
            if (Keyboard.GetState().IsKeyDown(Keys.D9)) { skyboxNumber = 2; skyboxName = "Daytime Sky - Grand Canyon"; }
            if (Keyboard.GetState().IsKeyDown(Keys.D0)) { skyboxNumber = 3; skyboxName = "Self Textures - Mountains"; }

            if (Keyboard.GetState().IsKeyDown(Keys.OemPlus)) reflectionIntensity += 0.01f;
            if (Keyboard.GetState().IsKeyDown(Keys.OemMinus)) reflectionIntensity -= 0.01f;

            if (Keyboard.GetState().IsKeyDown(Keys.R) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift)) redRatio += 0.04f;
            if (Keyboard.GetState().IsKeyDown(Keys.G) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift)) greenRatio += 0.04f;
            if (Keyboard.GetState().IsKeyDown(Keys.B) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift)) blueRatio += 0.04f;

            if (Keyboard.GetState().IsKeyDown(Keys.R) && Keyboard.GetState().IsKeyDown(Keys.LeftShift)) redRatio -= 0.04f;
            if (Keyboard.GetState().IsKeyDown(Keys.G) && Keyboard.GetState().IsKeyDown(Keys.LeftShift)) greenRatio -= 0.04f;
            if (Keyboard.GetState().IsKeyDown(Keys.B) && Keyboard.GetState().IsKeyDown(Keys.LeftShift)) blueRatio -= 0.04f;

            if (Keyboard.GetState().IsKeyDown(Keys.Q) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift)) fresnelPower += 0.2f;
            if (Keyboard.GetState().IsKeyDown(Keys.W) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift)) fresnelScale += 1f;
            if (Keyboard.GetState().IsKeyDown(Keys.E) && !Keyboard.GetState().IsKeyDown(Keys.LeftShift)) fresnelBias += 0.02f;

            if (Keyboard.GetState().IsKeyDown(Keys.Q) && Keyboard.GetState().IsKeyDown(Keys.LeftShift)) fresnelPower -= 0.2f;
            if (Keyboard.GetState().IsKeyDown(Keys.W) && Keyboard.GetState().IsKeyDown(Keys.LeftShift)) fresnelScale -= 1f;
            if (Keyboard.GetState().IsKeyDown(Keys.E) && Keyboard.GetState().IsKeyDown(Keys.LeftShift)) fresnelBias -= 0.02f;

            // Info UI + Help UI
            if (Keyboard.GetState().IsKeyDown(Keys.H) && previousKeyboardState.IsKeyUp(Keys.H)) { showInfo = !showInfo; }
            if (Keyboard.GetState().IsKeyDown(Keys.OemQuestion) && previousKeyboardState.IsKeyUp(Keys.OemQuestion)) { showHelp = !showHelp; }

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

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Picking the skybox
            switch(skyboxNumber)
            {
                case 0:
                    currentSkybox = testSkybox;
                    break;

                case 1:
                    currentSkybox = officeSkybox;
                    break;

                case 2:
                    currentSkybox = daytimeSkybox;
                    break;

                case 3:
                    currentSkybox = selfSkybox;
                    break;

                default:
                    break;
            }

            RasterizerState originalRasterizerState = _graphics.GraphicsDevice.RasterizerState;
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            _graphics.GraphicsDevice.RasterizerState = rasterizerState;
            currentSkybox.Draw(view, projection, cameraPosition);
            _graphics.GraphicsDevice.RasterizerState = originalRasterizerState;

            DrawModelWithEffect();

            _spriteBatch.Begin();
            if (showInfo)
            {
                int i = 0;
                _spriteBatch.DrawString(font, "Camera Position: (" + cameraPosition.X.ToString("0.00") + ", " + cameraPosition.Y.ToString("0.00") + ", " + cameraPosition.Z.ToString("0.00") + ")", Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Green);
                _spriteBatch.DrawString(font, "Camera Angle: (" + cameraAngleX.ToString("0.00") + ", " + cameraAngleY.ToString("0.00") + ")", Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Green);
                _spriteBatch.DrawString(font, "Light Angle: (" + lightAngleX.ToString("0.00") + ", " + lightAngleY.ToString("0.00") + ")", Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Green);
                _spriteBatch.DrawString(font, "Shader Type: " + shaderName, Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Green);
                _spriteBatch.DrawString(font, "Specular Intensity: " + specularIntensity.ToString("0.00"), Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Green);
                _spriteBatch.DrawString(font, "Reflection Intensity: " + reflectionIntensity.ToString("0.00"), Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Green);
                _spriteBatch.DrawString(font, "Light Color: (" + specularColor.X.ToString("0.00") + ", " + specularColor.Y.ToString("0.00") + ", " + specularColor.Z.ToString("0.00") + ")", Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Green);
                _spriteBatch.DrawString(font, "Shininess: " + shininess.ToString("0.00"), Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Green);
            }
            if (showHelp)
            {
                int i = 0;
                _spriteBatch.DrawString(font, "Press H to show/hide the Info Menu", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Green);
                _spriteBatch.DrawString(font, "Press ? to show/hide the Help Menu", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Green);
                _spriteBatch.DrawString(font, "Left Click + Drag Rotates the Camera", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Green);
                _spriteBatch.DrawString(font, "Right Click + Drag Zooms In/Out", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Green);
                _spriteBatch.DrawString(font, "Middle Mouse + Drag Translates Camera", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Green);
                _spriteBatch.DrawString(font, "Arrow Keys Rotates the Light", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Green);
                _spriteBatch.DrawString(font, "S Key: Resets the Camera and Light", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Green);
                _spriteBatch.DrawString(font, "Hold Shift to Decrease the Below Values", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Green);
                _spriteBatch.DrawString(font, "Q Key: Fresnel Power", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Green);
                _spriteBatch.DrawString(font, "W Key: Fresnel Scale", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Green);
                _spriteBatch.DrawString(font, "E Key: Fresnel Bias", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Green);
                _spriteBatch.DrawString(font, "R Key: ETA Red Ratio", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Green);
                _spriteBatch.DrawString(font, "G Key: ETA Green Ratio", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Green);
                _spriteBatch.DrawString(font, "B Key: ETA Blue Ratio", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Green);
                _spriteBatch.DrawString(font, "+/- : Reflectivity", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Green);
                _spriteBatch.DrawString(font, "1-2-3-4-5-6: Change Model", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Green);
                _spriteBatch.DrawString(font, "7-8-9-0: Change Skybox", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Green);
                _spriteBatch.DrawString(font, "F7-F8-F9-F10 Change Shader", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Green);
            }
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        void DrawModelWithEffect()
        {
            effect.CurrentTechnique = effect.Techniques[shaderNumber];
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in currentModel.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        effect.Parameters["World"].SetValue(mesh.ParentBone.Transform);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);

                        Matrix worldInverseTransposeMatrix = Matrix.Transpose(Matrix.Invert(mesh.ParentBone.Transform));
                        effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTransposeMatrix);

                        effect.Parameters["CameraPosition"].SetValue(cameraPosition);

                        effect.Parameters["AmbientColor"].SetValue(ambient);
                        effect.Parameters["AmbientIntensity"].SetValue(ambientIntensity);

                        effect.Parameters["DiffuseLightDirection"].SetValue(lightDirection);
                        effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
                        effect.Parameters["DiffuseIntensity"].SetValue(diffuseIntensity);

                        effect.Parameters["SpecularColor"].SetValue(specularColor);
                        effect.Parameters["SpecularIntensity"].SetValue(shininess);

                        effect.Parameters["FresnelEtaRatio"].SetValue(new Vector3(redRatio, greenRatio, blueRatio));

                        effect.Parameters["Reflectivity"].SetValue(reflectionIntensity);

                        effect.Parameters["FresnelPower"].SetValue(fresnelPower);
                        effect.Parameters["FresnelBias"].SetValue(fresnelBias);
                        effect.Parameters["FresnelScale"].SetValue(fresnelScale);

                        if (modelName == "Helicopter") { effect.Parameters["decalMap"].SetValue(texture); }
                        effect.Parameters["environmentMap"].SetValue(currentSkybox.skyBoxTexture);

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