using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using CPI411.SimpleEngine;
using System;
using System.Diagnostics;

namespace Assignment4
{
    public class Assignment4 : Game
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
        float distance = 15f;

        MouseState previousMouseState;
        KeyboardState previousKeyboardState;

        int particleTexture = 0;
        int emitterNum = 0;
        string particleName = "Fire";
        string emitterShape = "Square";
        string emitterType = "Fountain Basic";

        Random random;
        ParticleManager particleManager;
        Vector3 particlePosition = new Vector3(0, 0, 0);
        int particleNum = 10;
        int maxParticles = 10000;
        int maxAge = 4;

        float particleSpeed = 1.0f;
        float emissionSize = 1.0f;

        public Assignment4()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), GraphicsDevice.Viewport.AspectRatio, 0.1f, 1000);

            textures = new Texture2D[3];

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("font");
            model = Content.Load<Model>("Plane");
            effect = Content.Load<Effect>("ParticleShader");
            textures[0] = Content.Load<Texture2D>("fire");
            textures[1] = Content.Load<Texture2D>("smoke");
            textures[2] = Content.Load<Texture2D>("water");

            random = new System.Random();

            particleManager = new ParticleManager(GraphicsDevice, maxParticles);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.P)) { GenerateParticles(); }

            particleManager.Update(gameTime.ElapsedGameTime.Milliseconds * 0.001f);

            if (Keyboard.GetState().IsKeyDown(Keys.D1)) { particleTexture = -1; }
            if (Keyboard.GetState().IsKeyDown(Keys.D2)) { particleTexture = 0; }
            if (Keyboard.GetState().IsKeyDown(Keys.D3)) { particleTexture = 1; }
            if (Keyboard.GetState().IsKeyDown(Keys.D4)) { particleTexture = 2; }

            if (Keyboard.GetState().IsKeyDown(Keys.F1)) { emitterType = "Fountain Basic"; }
            if (Keyboard.GetState().IsKeyDown(Keys.F2)) { emitterType = "Fountain Medium"; }
            if (Keyboard.GetState().IsKeyDown(Keys.F3)) { emitterType = "Fountain Advanced"; }

            if(Keyboard.GetState().IsKeyDown(Keys.F4) && !previousKeyboardState.IsKeyDown(Keys.F4))
            {
                if(emitterShape.Equals("Square")) { emitterShape = "Curve"; }
                else if(emitterShape.Equals("Curve")) { emitterShape = "Ring"; }
                else if(emitterShape.Equals("Ring")) { emitterShape = "Square"; }
            }

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

        private void GenerateParticles()
        {
            Vector3 position = new Vector3();
            float randomAngle = (float)(Math.PI * (random.NextDouble() * 2.0 - 1.0));
            if(emitterShape == "Square") { position = new Vector3((float)(emissionSize * (random.NextDouble() - 0.5)), 0, (float)(emissionSize * (random.NextDouble() - 0.5))); }
            if(emitterShape == "Curve") { position = new Vector3(randomAngle / 3.0f * emissionSize, 0, emissionSize * (float)Math.Sin(randomAngle)); }
            if(emitterShape == "Ring") { position = new Vector3(emissionSize * (float)Math.Sin(randomAngle), 0, emissionSize * (float)Math.Cos(randomAngle)); }

            Vector3 velocity = new Vector3();
            if(emitterType == "Fountain Basic") { velocity = new Vector3(0, 2, 0); }
            if(emitterType == "Fountain Medium") { velocity = new Vector3((float)random.NextDouble() * 2 - 1, (float)random.NextDouble() * 2 - 1, (float)random.NextDouble() * 2 - 1); }
            if(emitterType == "Fountain Advanced") { velocity = new Vector3(0, 0, 0); }

            for (int i = 0; i < particleNum; i++)
            {
                float angle = (float)(Math.PI * (i * 6) / 180.0f);
                Particle particle = particleManager.getNext();
                particle.Position = position;
                particle.Velocity = particleSpeed * velocity;
                particle.Acceleration = (emitterType == "Fountain Basic") ? new Vector3(0, 0, 0) : new Vector3(0, -2f, 0);
                particle.MaxAge = maxAge;
                particle.Init();
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = new DepthStencilState();

            model.Draw(world, view, projection);

            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            if (particleTexture > -1)
            {
                effect.CurrentTechnique = effect.Techniques[0];
                effect.CurrentTechnique.Passes[0].Apply();
                effect.Parameters["World"].SetValue(world);
                effect.Parameters["View"].SetValue(view);
                effect.Parameters["Projection"].SetValue(projection);
                effect.Parameters["InverseCamera"].SetValue(Matrix.CreateRotationX(cameraAngleY) * Matrix.CreateRotationY(cameraAngleX) * Matrix.CreateTranslation(cameraTarget));

                effect.Parameters["Texture"].SetValue(textures[particleTexture]);
            }


            particleManager.Draw(GraphicsDevice);

            _spriteBatch.Begin();
            if (showInfo)
            {
                int i = 0;
                _spriteBatch.DrawString(font, "Camera Position: (" + cameraPosition.X.ToString("0.00") + ", " + cameraPosition.Y.ToString("0.00") + ", " + cameraPosition.Z.ToString("0.00") + ")", Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "Camera Angle: (" + cameraAngleX.ToString("0.00") + ", " + cameraAngleY.ToString("0.00") + ")", Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "Emitter Type: " + emitterType, Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "Emitter Shape: " + emitterShape, Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Black);
            }
            if (showHelp)
            {
                int i = 0;
                _spriteBatch.DrawString(font, "Press H to show/hide the Info Menu", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "Press ? to show/hide the Help Menu", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "Left Click + Drag Rotates the Camera", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "Right Click + Drag Zooms In/Out", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "Middle Mouse + Drag Translates Camera", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "S Key: Resets the Camera and Light", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "1: Phong Quads", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "2: Smoke Quads", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "3: Water Quads", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "4: Fire Quads", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "F1: Fountain Basic", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "F2: Fountain Medium", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "F3: Fountain Advanced", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "F4: Change Emitter Shapes", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
            }
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}