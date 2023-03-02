using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lab08Image
{
    public class Lab08Image : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Texture2D texture;
        Texture2D filter;
        Effect effect;

        public Lab08Image()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 600;
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

            texture = Content.Load<Texture2D>("Honolulu");
            filter = Content.Load<Texture2D>("filter");
            effect = Content.Load<Effect>("post");

            effect.Parameters["modelTexture"].SetValue(texture);
            //effect.Parameters["filterTexture"].SetValue(filter);
            effect.Parameters["imageWidth"].SetValue((float)texture.Width);
            effect.Parameters["imageHeight"].SetValue((float)texture.Height);

            Matrix projection = Matrix.CreateOrthographicOffCenter(0, 800, 600, 0, 0, 1);
            Matrix halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);
            effect.Parameters["MatrixTransform"].SetValue(halfPixelOffset * projection);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(0, null, null, null, null, effect);
            _spriteBatch.Draw(texture, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}