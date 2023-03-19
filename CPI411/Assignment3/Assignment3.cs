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

        Vector3 lightPosition = new Vector3(0, 1, 0);
        Matrix lightView;
        Matrix lightProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1f, 1f, 50f);

        float cameraAngleX, cameraAngleY;
        float lightAngleX, lightAngleY;
        float distance = 15f;

        Skybox skybox;

        Texture[] normalMaps;
        int normalMapNumber = 0;
        string pictureName = "Art";

        int shaderTechnique = 0;
        string shaderName = "Normal Map Shader";

        Vector3 uvScale = new Vector3(1.0f, 1.0f, 1.0f);
        float bumpHeight = 1.0f;
        bool mipmap = true;
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
            if (Keyboard.GetState().IsKeyDown(Keys.F1)) { shaderTechnique = 0; shaderName = "Normals"; }
            if (Keyboard.GetState().IsKeyDown(Keys.F2)) { shaderTechnique = 1; shaderName = "RGB World Space Normals"; }
            if (Keyboard.GetState().IsKeyDown(Keys.F3)) { shaderTechnique = 2; shaderName = "Tangent Space Normals"; }
            if (Keyboard.GetState().IsKeyDown(Keys.F4)) { shaderTechnique = 3; shaderName = "Reflective Bump Map"; }
            if (Keyboard.GetState().IsKeyDown(Keys.F5)) { shaderTechnique = 4; shaderName = "Refractive Bump Map"; }
            if (Keyboard.GetState().IsKeyDown(Keys.F6)) { shaderTechnique = 5; shaderName = "Tangent Space Bump Map, with normal map"; }
            if (Keyboard.GetState().IsKeyDown(Keys.F7)) { shaderTechnique = 6; shaderName = "Tangent Space Bump Map, without normal map"; }
            if (Keyboard.GetState().IsKeyDown(Keys.F8)) { shaderTechnique = 7; shaderName = "Normalizing Tangent after Interpolation, without normal map"; }
            if (Keyboard.GetState().IsKeyDown(Keys.F9)) { shaderTechnique = 8; shaderName = "Normalizing Tangent after Interpolation, with normal map"; }
            if (Keyboard.GetState().IsKeyDown(Keys.F10)) { shaderTechnique = 9; shaderName = "Normal Map as an Image"; }

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

            if (Keyboard.GetState().IsKeyDown(Keys.M) && !previousKeyboardState.IsKeyDown(Keys.M)) mipmap = !mipmap;

            if(Keyboard.GetState().IsKeyDown(Keys.U) && !previousKeyboardState.IsKeyDown(Keys.LeftShift)) { uvScale.X += 0.01f; }
            if(Keyboard.GetState().IsKeyDown(Keys.U) && previousKeyboardState.IsKeyDown(Keys.LeftShift)) { uvScale.X -= 0.01f; }
            
            if(Keyboard.GetState().IsKeyDown(Keys.V) && !previousKeyboardState.IsKeyDown(Keys.LeftShift)) { uvScale.Y += 0.01f; }
            if(Keyboard.GetState().IsKeyDown(Keys.V) && previousKeyboardState.IsKeyDown(Keys.LeftShift)) { uvScale.Y -= 0.01f; }

            if (Keyboard.GetState().IsKeyDown(Keys.W) && !previousKeyboardState.IsKeyDown(Keys.LeftShift)) { bumpHeight += 0.01f; uvScale.Z += 0.01f; }
            if (Keyboard.GetState().IsKeyDown(Keys.W) && previousKeyboardState.IsKeyDown(Keys.LeftShift)) { bumpHeight -= 0.01f; uvScale.Z -= 0.01f; }

            // Info UI + Help UI
            if (Keyboard.GetState().IsKeyDown(Keys.H) && !previousKeyboardState.IsKeyDown(Keys.H)) { showInfo = !showInfo; }
            if (Keyboard.GetState().IsKeyDown(Keys.OemQuestion) && !previousKeyboardState.IsKeyDown(Keys.OemQuestion)) { showHelp = !showHelp; }

            // Reset the camera
            if (Keyboard.GetState().IsKeyDown(Keys.S)) { cameraAngleX = cameraAngleY = lightAngleX = lightAngleY = 0; distance = 15; cameraTarget = Vector3.Zero; }

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

            if (Keyboard.GetState().IsKeyDown(Keys.Up)) lightAngleY += 0.1f;
            if (Keyboard.GetState().IsKeyDown(Keys.Down)) lightAngleY -= 0.1f;
            if (Keyboard.GetState().IsKeyDown(Keys.Left)) lightAngleX += 0.1f;
            if (Keyboard.GetState().IsKeyDown(Keys.Right)) lightAngleX -= 0.1f;

            lightPosition = Vector3.Transform(new Vector3(0, 0, 10), Matrix.CreateRotationX(lightAngleY) * Matrix.CreateRotationY(lightAngleX));
            lightView = Matrix.CreateLookAt(lightPosition, Vector3.Zero, Vector3.Transform(Vector3.UnitY, Matrix.CreateRotationX(lightAngleY) * Matrix.CreateRotationY(lightAngleX)));

            previousMouseState = Mouse.GetState();
            previousKeyboardState = Keyboard.GetState();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
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
                _spriteBatch.DrawString(font, "Camera Position: (" + cameraPosition.X.ToString("0.00") + ", " + cameraPosition.Y.ToString("0.00") + ", " + cameraPosition.Z.ToString("0.00") + ")", Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.White);
                _spriteBatch.DrawString(font, "Camera Angle: (" + cameraAngleX.ToString("0.00") + ", " + cameraAngleY.ToString("0.00") + ")", Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.White);
                _spriteBatch.DrawString(font, "Light Angle: (" + lightAngleX.ToString("0.00") + ", " + lightAngleY.ToString("0.00") + ")", Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.White);
                _spriteBatch.DrawString(font, "Shader Type: " + shaderName, Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.White);
                _spriteBatch.DrawString(font, "Normal Texture: " + pictureName, Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.White);
                _spriteBatch.DrawString(font, "Texture Tile Scale: (" + uvScale.X.ToString("0.00") + ", " + uvScale.Y.ToString("0.00") + ")", Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.White);
                _spriteBatch.DrawString(font, "Bump Height: " + bumpHeight.ToString("0.00"), Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.White);
                _spriteBatch.DrawString(font, "MipMap: " + mipmap, Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.White);
            }
            if (showHelp)
            {
                int i = 0;
                _spriteBatch.DrawString(font, "Press H to show/hide the Info Menu", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.White);
                _spriteBatch.DrawString(font, "Press ? to show/hide the Help Menu", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.White);
                _spriteBatch.DrawString(font, "Left Click + Drag Rotates the Camera", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.White);
                _spriteBatch.DrawString(font, "Right Click + Drag Zooms In/Out", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.White);
                _spriteBatch.DrawString(font, "Middle Mouse + Drag Translates Camera", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.White);
                _spriteBatch.DrawString(font, "Arrow Keys Rotates the Light", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.White);
                _spriteBatch.DrawString(font, "S Key: Resets the Camera and Light", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.White);
                _spriteBatch.DrawString(font, "Number Keys 1-8: Change Model Texture", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.White);
                _spriteBatch.DrawString(font, "M Key: Toggle MipMap", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.White);
                _spriteBatch.DrawString(font, "u/U: U-scale of tiling", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.White);
                _spriteBatch.DrawString(font, "v/V: V-scale of tiling", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.White);
                _spriteBatch.DrawString(font, "w/W: Change bump height", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.White);
                _spriteBatch.DrawString(font, "F1: Visualize the Normals", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.White);
                _spriteBatch.DrawString(font, "F2: RGB World Space", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.White);
                _spriteBatch.DrawString(font, "F3: Tangent Space with Normals", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.White);
                _spriteBatch.DrawString(font, "F4: Reflective bump mapping", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.White);
                _spriteBatch.DrawString(font, "F5: Refractive bump mapping", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.White);
                _spriteBatch.DrawString(font, "F6: Normalized Normal", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.White);
                _spriteBatch.DrawString(font, "F7: Un Normalized Tangent and Normal", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.White);
                _spriteBatch.DrawString(font, "F8: Normalized Tangent", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.White);
                _spriteBatch.DrawString(font, "F9: Normalized Tangent Normal", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.White);
                _spriteBatch.DrawString(font, "F10: View Normal Map", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.White);
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

                        effect.Parameters["LightStrength"].SetValue(200);

                        effect.Parameters["AmbientColor"].SetValue(new Vector4(0.1f, 0.1f, 0.1f, 0.1f));
                        effect.Parameters["AmbientIntensity"].SetValue(0.25f);
                        effect.Parameters["DiffuseIntensity"].SetValue(1.0f);
                        effect.Parameters["DiffuseColor"].SetValue(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                        effect.Parameters["SpecularIntensity"].SetValue(1.0f);
                        effect.Parameters["SpecularColor"].SetValue(new Vector3(1, 1, 1));
                        effect.Parameters["Shininess"].SetValue(20f);

                        effect.Parameters["NormalMap"].SetValue(normalMaps[normalMapNumber]);
                        effect.Parameters["UVScale"].SetValue(uvScale);
                        effect.Parameters["SkyboxTexture"].SetValue(skybox.skyBoxTexture);

                        effect.Parameters["EtaRatio"].SetValue(etaRatio);
                        effect.Parameters["MipMap"].SetValue(mipmap ? 1 : 0);


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