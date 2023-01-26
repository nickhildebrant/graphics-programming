using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lab03
{
    public class Lab03 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Model model;

        Matrix world;
        Matrix view;
        Matrix projection;

        float angle, angle2;
        float distance = 1f;

        Effect effect;

        SpriteFont font;

        MouseState previousMouseState;

        public Lab03()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            effect = Content.Load<Effect>("SimpleShading");
            model = Content.Load<Model>("bunny");
            //font = Content.Load<SpriteFont>("font");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                angle += 0.1f * (Mouse.GetState().X - previousMouseState.X);
                angle2 += 0.1f * (Mouse.GetState().Y - previousMouseState.Y);
            }

            if (Mouse.GetState().RightButton == ButtonState.Pressed)
            {
                distance += 0.1f * (Mouse.GetState().Y - previousMouseState.Y);
            }

            Vector3 camera = Vector3.Transform(distance * new Vector3(0, 0, 20), Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));
            view = Matrix.CreateLookAt(camera, Vector3.Zero, Vector3.UnitY);

            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), GraphicsDevice.Viewport.AspectRatio, 0.1f, 100);

            previousMouseState = Mouse.GetState();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.DepthStencilState = new DepthStencilState();

            _spriteBatch.Begin();
            //_spriteBatch.DrawString(font, "angle:" + angle, Vector2.UnitX + Vector2.UnitY * 12, Color.White);
            _spriteBatch.End();

            effect.CurrentTechnique = effect.Techniques[0];
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {

                    effect.Parameters["World"].SetValue(mesh.ParentBone.Transform);
                    effect.Parameters["View"].SetValue(view);
                    effect.Parameters["Projection"].SetValue(projection);
                    effect.Parameters["AmbientColor"].SetValue(Color.Black.ToVector3());
                    effect.Parameters["AmbientIntensity"].SetValue(2f);
                    effect.Parameters["DiffuseColor"].SetValue(new Vector3(0.75f, 0.75f, 0.75f));
                    effect.Parameters["DiffuseLightDirection"].SetValue(Vector3.Up);
                    effect.Parameters["DiffuseIntensity"].SetValue(1f);

                    Matrix worldInverseTranspose = Matrix.Transpose(Matrix.Invert(mesh.ParentBone.Transform));
                    effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTranspose);
                    pass.Apply();

                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                        GraphicsDevice.Indices = part.IndexBuffer;

                        GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);
                    }
                }
            }

            base.Draw(gameTime);
        }
    }
}