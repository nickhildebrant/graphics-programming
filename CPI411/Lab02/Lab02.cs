using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lab02
{
    public class Lab02 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        float angle;
        float distance = 2f;

        Effect effect;

        Matrix world;
        Matrix view;
        Matrix projection;

        VertexPositionTexture[] vertices =
        {
            new VertexPositionTexture(new Vector3(0, 1, 0), new Vector2(0.5f, 0)),
            new VertexPositionTexture(new Vector3(1, 0, 0), new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(-1, 0, 0), new Vector2(0, 1))
        };

        public Lab02()
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

            effect = Content.Load<Effect>("SimpleTexture");
            effect.Parameters["MyTexture"].SetValue(Content.Load<Texture2D>("logo_mg"));
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if(Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                angle += 0.1f;
                Vector3 offset = new Vector3((float)System.Math.Cos(angle), (float)System.Math.Sin(angle), 0f);
                effect.Parameters["offset"].SetValue(offset);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                angle -= 0.1f;
                Vector3 offset = new Vector3((float)System.Math.Cos(angle), (float)System.Math.Sin(angle), 0f);
                effect.Parameters["offset"].SetValue(offset);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                distance += 0.05f;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                distance -= 0.05f;
            }

            Vector3 cameraPosition = distance * new Vector3((float)System.Math.Sin(angle), 0, (float)System.Math.Cos(angle));

            world = Matrix.Identity;
            view = Matrix.CreateLookAt(cameraPosition, new Vector3(), new Vector3(0, 1, 0));
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), GraphicsDevice.Viewport.AspectRatio, 0.1f, 100);

            effect.Parameters["World"].SetValue(world);
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, vertices, 0, vertices.Length / 3);
            }

            base.Draw(gameTime);
        }
    }
}