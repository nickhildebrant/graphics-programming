using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Final
{
    public class Final : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        SpriteFont font;
        Model model;
        Texture2D[] textures;
        Effect effect;

        bool showInfo = true, showHelp = true;

        Matrix world = Matrix.Identity;
        Matrix view, projection;

        Vector3 cameraPosition;
        Vector3 cameraTarget;

        float cameraAngleX = -30, cameraAngleY = -30;
        float distance = 15;

        MouseState previousMouseState;
        KeyboardState previousKeyboardState;

        VertexPositionColor[] vertices =
        {
            new VertexPositionColor(new Vector3(-10, 0, 10), Color.Red),       // Top left
            new VertexPositionColor(new Vector3(10, 0, 10), Color.Green),      // Top right
            new VertexPositionColor(new Vector3(10, 0, -10), Color.Blue),      // Bottom right

            new VertexPositionColor(new Vector3(10, 0, -10), Color.Blue),     // Bottom right
            new VertexPositionColor(new Vector3(-10, 0, -10), Color.Green),    // Bottom left
            new VertexPositionColor(new Vector3(-10, 0, 10), Color.Red)      // Top left
        };

        public Final()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            world = Matrix.Identity;
            view = Matrix.CreateLookAt(cameraPosition, new Vector3(), new Vector3(0, 0, 0));
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), GraphicsDevice.Viewport.AspectRatio, 0.1f, 100);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("font");
            effect = Content.Load<Effect>("SubdivisionShader");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

            // Info UI + Help UI
            if (Keyboard.GetState().IsKeyDown(Keys.H) && !previousKeyboardState.IsKeyDown(Keys.H)) { showInfo = !showInfo; }
            if (Keyboard.GetState().IsKeyDown(Keys.OemQuestion) && !previousKeyboardState.IsKeyDown(Keys.OemQuestion)) { showHelp = !showHelp; }

            // Reset the camera
            if (Keyboard.GetState().IsKeyDown(Keys.S)) { cameraAngleX = cameraAngleY = -30; distance = 15; cameraTarget = Vector3.Zero; }

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

            previousMouseState = Mouse.GetState();
            previousKeyboardState = Keyboard.GetState();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            effect.Parameters["World"].SetValue(world);
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                //GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, vertices, 0, vertices.Length / 3);
                GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, vertices, 0, vertices.Length / 3);
            }

            _spriteBatch.Begin();
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}