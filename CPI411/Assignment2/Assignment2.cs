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

        Model model;
        Effect effect;
        Texture2D texture;

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

        Vector4 specularColor = new Vector4(1, 1, 1, 1);
        float specularIntensity = 1.0f;
        float shininess = 20f;

        float cameraAngleX, cameraAngleY;
        float lightAngleX, lightAngleY;
        float distance = 10f;

        int skyboxNumber = 7;
        string skyboxName = "Test Skybox";
        Skybox currentSkybox, testSkybox, officeSkybox, daytimeSkybox, selfSkybox;

        int shaderNumber = 0;
        string shaderName = "Reflection Shader";

        float reflectionIntensity = 0.5f;

        float fresnelPower = 2;
        float fresnelScale = 15;
        float fresnelBias = 0.5f;

        float redRatio = 1f;
        float greenRatio = 1f;
        float blueRatio = 1f;

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

            //font = Content.Load<SpriteFont>("font");

            effect = Content.Load<Effect>("LightingShader");
            model = Content.Load<Model>("bunnyUV");
            texture = Content.Load<Texture2D>("HelicopterTexture");

            // Loading Test Skybox
            string[] testSkyboxTextures =
            {
                "Test/debug_posx", "Test/debug_negx",
                "Test/debug_posy", "Test/debug_negy",
                "Test/debug_posz", "Test/debug_negz",
            };
            testSkybox = new Skybox(testSkyboxTextures, 256, Content, GraphicsDevice);

            string[] officeSkyboxTextures =
            {
                "Office/nvlobby_new_posx", "Office/nvlobby_new_negx",
                "Office/nvlobby_new_posy", "Office/nvlobby_new_negy",
                "Office/nvlobby_new_posz", "Office/nvlobby_new_negz"
            };
            officeSkybox = new Skybox(officeSkyboxTextures, Content, GraphicsDevice);

            string[] daytimeSkyboxTextures =
            {
                "Daytime/grandcanyon_posx", "Daytime/grandcanyon_negx",
                "Daytime/grandcanyon_posy", "Daytime/grandcanyon_negy",
                "Daytime/grandcanyon_posz", "Daytime/grandcanyon_negz"
            };
            daytimeSkybox = new Skybox(daytimeSkyboxTextures, Content, GraphicsDevice);

            currentSkybox = testSkybox;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

            // Reset the camera
            if (Keyboard.GetState().IsKeyDown(Keys.S)) { cameraAngleX = cameraAngleY = lightAngleX = lightAngleY = 0; distance = 30; cameraTarget = Vector3.Zero; }

            // Distance control
            if (previousMouseState.RightButton == ButtonState.Pressed && Mouse.GetState().RightButton == ButtonState.Pressed)
            {
                distance += (Mouse.GetState().X - previousMouseState.X) / 100f;
            }

            // Camera control
            if (previousMouseState.LeftButton == ButtonState.Pressed && Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                cameraAngleX += (previousMouseState.X - Mouse.GetState().X);
                cameraAngleY += (previousMouseState.Y - Mouse.GetState().Y);
            }

            if (previousMouseState.MiddleButton == ButtonState.Pressed && Mouse.GetState().MiddleButton == ButtonState.Pressed)
            {
                Vector3 ViewRight = Vector3.Transform(Vector3.UnitX, Matrix.CreateRotationX(cameraAngleY) * Matrix.CreateRotationY(cameraAngleX));
                Vector3 ViewUp = Vector3.Transform(Vector3.UnitY, Matrix.CreateRotationX(cameraAngleY) * Matrix.CreateRotationY(cameraAngleX));
                cameraTarget -= ViewRight * (Mouse.GetState().X - previousMouseState.X) / 10f;
                cameraTarget += ViewUp * (Mouse.GetState().Y - previousMouseState.Y) / 10f;
            }


            cameraPosition = Vector3.Transform(new Vector3(0, 0, distance), Matrix.CreateTranslation(cameraTarget) * Matrix.CreateRotationX(MathHelper.ToRadians(cameraAngleY)) * Matrix.CreateRotationY(MathHelper.ToRadians(cameraAngleX)));
            view = Matrix.CreateLookAt(cameraPosition, cameraTarget, Vector3.Transform(Vector3.UnitY, Matrix.CreateRotationX(MathHelper.ToRadians(cameraAngleY)) * Matrix.CreateRotationY(MathHelper.ToRadians(cameraAngleX))));

            lightPosition = Vector3.Transform(new Vector3(0, 0, 10), Matrix.CreateRotationX(lightAngleY) * Matrix.CreateRotationY(lightAngleX));
            //lightView = Matrix.CreateLookAt(lightPosition, Vector3.Zero, Vector3.Transform(Vector3.UnitY, Matrix.CreateRotationX(angleL2) * Matrix.CreateRotationY(angleL)));
            //lightProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1f, 1f, 50f);

            previousMouseState = Mouse.GetState();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            RasterizerState originalRasterizerState = _graphics.GraphicsDevice.RasterizerState;
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            _graphics.GraphicsDevice.RasterizerState = rasterizerState;
            currentSkybox.Draw(view, projection, cameraPosition);
            _graphics.GraphicsDevice.RasterizerState = originalRasterizerState;

            DrawModelWithEffect();

            base.Draw(gameTime);
        }

        void DrawModelWithEffect()
        {
            effect.CurrentTechnique = effect.Techniques[shaderNumber];
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

                        effect.Parameters["AmbientColor"].SetValue(ambient);
                        effect.Parameters["AmbientIntensity"].SetValue(ambientIntensity);

                        effect.Parameters["DiffuseLightDirection"].SetValue(lightDirection);
                        effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
                        effect.Parameters["DiffuseIntensity"].SetValue(diffuseIntensity);

                        effect.Parameters["SpecularColor"].SetValue(specularColor);
                        effect.Parameters["SpecularIntensity"].SetValue(shininess);

                        //effect.Parameters["EtaRatio"].SetValue(etaRatio);
                        //effect.Parameters["FresnelEtaRatio"].SetValue(fresnelEtaRatio);

                        effect.Parameters["Reflectivity"].SetValue(reflectionIntensity);

                        effect.Parameters["FresnelPower"].SetValue(fresnelPower);
                        effect.Parameters["FresnelBias"].SetValue(fresnelBias);
                        effect.Parameters["FresnelScale"].SetValue(fresnelScale);

                        if(showTexture) effect.Parameters["decalMap"].SetValue(texture);
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