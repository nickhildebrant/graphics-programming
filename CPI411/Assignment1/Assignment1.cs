using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Assignment1
{
    public class Assignment1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Model currentModel;
        Model box, bunny, sphere, teapot, torus;
        Effect effect;
        Matrix world, view, projection;

        Vector3 cameraPosition;

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

        int currTechnique = 0;

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

            // Camera rotation controller
            if (Mouse.GetState().LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Pressed)
            {
                cameraAngle2 -= (previousMouseState.X - Mouse.GetState().X) / 100f;
                cameraAngle1 -= (previousMouseState.Y - Mouse.GetState().Y) / 100f;
            }

            // Camera distance controller
            if (Mouse.GetState().RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Pressed)
            {
                var diff = (float)gameTime.ElapsedGameTime.TotalSeconds * (Mouse.GetState().Y - previousMouseState.Y);
                distance += diff;
            }

            cameraPosition = Vector3.Transform(new Vector3(0, 0, 20), Matrix.CreateRotationX(cameraAngle1) * Matrix.CreateRotationY(cameraAngle2));
            view = Matrix.CreateLookAt(distance * cameraPosition, new Vector3(), Vector3.Up);
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), GraphicsDevice.Viewport.AspectRatio, 0.1f, 1000);

            // 1 - Box Model
            if (Keyboard.GetState().IsKeyDown(Keys.D1)) { currentModel = box; }

            // 2 - Sphere Model
            if (Keyboard.GetState().IsKeyDown(Keys.D2)) { currentModel = sphere; }

            // 3 - Torus Model
            if (Keyboard.GetState().IsKeyDown(Keys.D3)) { currentModel = torus; }

            // 4 - Teapot Model
            if (Keyboard.GetState().IsKeyDown(Keys.D4)) { currentModel = teapot; }

            // 5 - Bunny Model
            if (Keyboard.GetState().IsKeyDown(Keys.D5)) { currentModel = bunny; }

            // F1 - Gouraud shader
            if (Keyboard.GetState().IsKeyDown(Keys.F1)) { currTechnique = 0; }

            // F2 - Phong Shader
            if (Keyboard.GetState().IsKeyDown(Keys.F2)) { currTechnique = 1; }

            // F3 - Toon Shader
            if (Keyboard.GetState().IsKeyDown(Keys.F3)) { currTechnique = 2; }

            // F4 - Half Life Shader
            if (Keyboard.GetState().IsKeyDown(Keys.F4)) { currTechnique = 3; }

            // F5 - Toon Shader
            if (Keyboard.GetState().IsKeyDown(Keys.F5)) { currTechnique = 4; }

            // F6 - Half Life Shader
            if (Keyboard.GetState().IsKeyDown(Keys.F6)) { currTechnique = 5; }

            // + Increase specular intensity
            if (Keyboard.GetState().IsKeyDown(Keys.OemPlus)) { specularIntensity += 0.2f; }

            // - Decrease specular intensity
            if (Keyboard.GetState().IsKeyDown(Keys.OemMinus)) { specularIntensity -= 0.2f; }

            // L-Ctrl + Increase shininess
            if (Keyboard.GetState().IsKeyDown(Keys.LeftControl) && Keyboard.GetState().IsKeyDown(Keys.OemPlus)) { shininess += 0.2f; }

            // L-Ctrl - Decrease shininess
            if (Keyboard.GetState().IsKeyDown(Keys.LeftControl) && Keyboard.GetState().IsKeyDown(Keys.OemMinus)) { shininess -= 0.2f; }

            previousMouseState = Mouse.GetState();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            effect.CurrentTechnique = effect.Techniques[currTechnique]; // 0 per vertex, 1 per pixel
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in currentModel.Meshes)
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

                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                        GraphicsDevice.Indices = part.IndexBuffer;

                        GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
                    }
                }
            }

            base.Draw(gameTime);
        }
    }
}