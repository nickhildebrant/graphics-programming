using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lab04
{
    public class Lab04 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Model model;
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
        float shininess = 20f;

        float angle, angle2;
        float distance = 1f;

        MouseState previousMouseState;

        int currTechnique = 0;

        public Lab04()
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
            model = Content.Load<Model>("Torus");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

            if (Mouse.GetState().LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Pressed)
            {
                angle -= (previousMouseState.X - Mouse.GetState().X) / 100f;
                angle2 -= (previousMouseState.Y - Mouse.GetState().Y) / 100f;
            }

            if (Mouse.GetState().RightButton == ButtonState.Pressed)
            {
                distance += 0.1f * (Mouse.GetState().Y - previousMouseState.Y);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                shininess += 0.2f;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                shininess -= 0.2f;
            }

            // Gouraud shader
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                currTechnique = 0;
            }

            // Phong Shader
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                currTechnique = 1;
            }

            // Toon Shader
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                currTechnique = 2;
            }

            cameraPosition = Vector3.Transform(new Vector3(0, 0, 20), Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));
            view = Matrix.CreateLookAt(cameraPosition, new Vector3(), Vector3.Up);

            //camera = Vector3.Transform(distance * new Vector3(0, 0, 20), Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));
            //view = Matrix.CreateLookAt(camera, Vector3.Zero, Vector3.UnitY);

            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), GraphicsDevice.Viewport.AspectRatio, 0.1f, 100);

            previousMouseState = Mouse.GetState();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            effect.CurrentTechnique = effect.Techniques[currTechnique]; // 0 per vertex, 1 per pixel
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in model.Meshes)
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