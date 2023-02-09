using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using CPI411.SimpleEngine;

namespace Lab05
{
    public class Lab05 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Skybox skybox;
        string[] skyboxTextures = 
            {
                "skybox/SunsetPNG2",
                "skybox/SunsetPNG1",
                "skybox/SunsetPNG4",
                "skybox/SunsetPNG3",
                "skybox/SunsetPNG6",
                "skybox/SunsetPNG5",
            };

        Vector3 cameraPosition;
        Matrix view;
        Matrix projection;

        float angle, angle2;
        float distance = 1f;

        Effect effect;

        SpriteFont font;

        MouseState previousMouseState;

        public Lab05()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            skybox = new Skybox(skyboxTextures, Content, GraphicsDevice);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

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

            cameraPosition = Vector3.Transform(new Vector3(0, 0, 20), Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle));
            view = Matrix.CreateLookAt(distance * cameraPosition, new Vector3(), Vector3.Transform(Vector3.Up, Matrix.CreateRotationX(angle2) * Matrix.CreateRotationY(angle)));
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), GraphicsDevice.Viewport.AspectRatio, 0.1f, 100);

            previousMouseState = Mouse.GetState();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rasterizerState;

            skybox.Draw(view, projection, cameraPosition);

            base.Draw(gameTime);
        }
    }
}