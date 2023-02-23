using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using CPI411.SimpleEngine;

namespace Lab07
{
    public class Lab07 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Model model;
        Effect effect;
        Texture2D texture;

        Skybox skybox;

        Vector3 cameraPosition;
        Vector3 lightPosition;
        Matrix view;
        Matrix projection;

        float angle, angle2;
        float lightAngle1, lightAngle2;
        float distance = 10f;

        MouseState previousMouseState;

        public Lab07()
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
            model = Content.Load<Model>("Plane");
            effect = Content.Load<Effect>("BumpMap");
            texture = Content.Load<Texture2D>("NormalMaps/round");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

            // Camera rotation controller
            if (Mouse.GetState().LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Pressed)
            {
                angle -= (previousMouseState.X - Mouse.GetState().X) / 100f;
                angle2 -= (previousMouseState.Y - Mouse.GetState().Y) / 100f;
            }

            if (Mouse.GetState().RightButton == ButtonState.Pressed)
            {
                distance += 0.1f * (Mouse.GetState().Y - previousMouseState.Y);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Left)) lightAngle1 += 0.02f; 
            if (Keyboard.GetState().IsKeyDown(Keys.Right)) lightAngle1 -= 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Up)) lightAngle2 += 0.02f;
            if (Keyboard.GetState().IsKeyDown(Keys.Down)) lightAngle2 -= 0.02f;

            cameraPosition = Vector3.Transform(new Vector3(0, 0, 3), Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));
            view = Matrix.CreateLookAt(distance * cameraPosition, new Vector3(), Vector3.Transform(Vector3.Up, Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle)));
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), GraphicsDevice.Viewport.AspectRatio, 0.1f, 100);

            lightPosition = Vector3.Transform(new Vector3(0, 0, 10), Matrix.CreateRotationX(lightAngle2) * Matrix.CreateRotationY(lightAngle1));

            previousMouseState = Mouse.GetState();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = new DepthStencilState();

            effect.CurrentTechnique = effect.Techniques[0];

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        effect.Parameters["World"].SetValue(mesh.ParentBone.Transform);
                        effect.Parameters["View"].SetValue(view);
                        effect.Parameters["Projection"].SetValue(projection);
                        Matrix worldInverseTranspose = Matrix.Transpose(Matrix.Invert(mesh.ParentBone.Transform));
                        effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTranspose);
                        effect.Parameters["CameraPosition"].SetValue(cameraPosition);
                        effect.Parameters["LightPosition"].SetValue(lightPosition);

                        effect.Parameters["AmbientColor"].SetValue(new Vector4(1, 1, 1, 1));
                        effect.Parameters["AmbientIntensity"].SetValue(cameraPosition);
                        effect.Parameters["DiffuseColor"].SetValue(cameraPosition);
                        effect.Parameters["DiffuseIntensity"].SetValue(cameraPosition);
                        effect.Parameters["SpecularColor"].SetValue(cameraPosition);
                        effect.Parameters["SpecularIntensity"].SetValue(cameraPosition);
                        effect.Parameters["Shininess"].SetValue(cameraPosition);

                        effect.Parameters["normalMap"].SetValue(texture);

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