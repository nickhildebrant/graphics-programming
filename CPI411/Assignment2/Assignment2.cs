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

        Matrix world, view, projection;

        Vector3 cameraPosition;
        Vector3 cameraLookat;
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

        public Assignment2()
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

            //font = Content.Load<SpriteFont>("font");

            effect = Content.Load<Effect>("SimpleShading");
            model = Content.Load<Model>("bunnyUV");
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
            view = Matrix.CreateLookAt(distance * cameraPosition, new Vector3(cameraX, cameraY, 0), Vector3.Transform(Vector3.Up, Matrix.CreateRotationX(cameraAngle1) * Matrix.CreateRotationY(cameraAngle2)));
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), GraphicsDevice.Viewport.AspectRatio, 0.1f, 1000);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = new DepthStencilState();

            effect.CurrentTechnique = effect.Techniques[0]; // 0 per vertex, 1 per pixel
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in model.Meshes)
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

            base.Draw(gameTime);
        }
    }
}