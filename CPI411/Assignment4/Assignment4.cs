using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using CPI411.SimpleEngine;
using System;

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
        float randomness = 5f;

        ParticleManager particleManager;
        Vector3 particlePosition = new Vector3(0, 4, 0);
        Vector3 velocityOverride = Vector3.Zero;
        int particleCount = 0;
        int particleNum = 10;
        int maxParticles = 10000;
        int maxAge = 4;

        bool usingUserVelocity = false;
        bool usingWind = false;
        bool isRandom = false;
        bool gravityAffected = false;
        int  particleTime = 0;
        float gravity = -9f;
        float particleSpeed = 1.0f;
        float emissionSize = 1.0f;

        float bounciness = 1f;
        float friction = 0.5f;

        Vector3 windForce = new Vector3(1, 1, 1);

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

            UpdateParticles(gameTime);

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

            // Control the wind
            if (Keyboard.GetState().IsKeyDown(Keys.X))
            {
                windForce.X = (!Keyboard.GetState().IsKeyDown(Keys.LeftShift) && !Keyboard.GetState().IsKeyDown(Keys.RightShift)) ? MathHelper.Clamp((float)(windForce.X - 0.005f), (float)-0.1f, (float)0.1f) : MathHelper.Clamp((float)(windForce.X + 0.005f), (float)-0.1f, (float)0.1f);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Y))
            {
                windForce.Y = (!Keyboard.GetState().IsKeyDown(Keys.LeftShift) && !Keyboard.GetState().IsKeyDown(Keys.RightShift)) ? MathHelper.Clamp((float)(windForce.Y - 0.005f), (float)-0.1f, (float)0.1f) : MathHelper.Clamp((float)(windForce.Y + 0.005f), (float)-0.1f, (float)0.1f);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Z))
            {
                windForce.Z = (!Keyboard.GetState().IsKeyDown(Keys.LeftShift) && !Keyboard.GetState().IsKeyDown(Keys.RightShift)) ? MathHelper.Clamp((float)(windForce.Z - 0.005f), (float)-0.1f, (float)0.1f) : MathHelper.Clamp((float)(windForce.Z + 0.005f), (float)-0.1f, (float)0.1f);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.F))
            {
                friction = (!Keyboard.GetState().IsKeyDown(Keys.LeftShift) && !Keyboard.GetState().IsKeyDown(Keys.RightShift)) ? MathHelper.Clamp((float)(friction - 0.005f), (float)0f, (float)1f) : MathHelper.Clamp((float)(friction + 0.005f), (float)0f, (float)1f);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.B))
            {
                bounciness = (!Keyboard.GetState().IsKeyDown(Keys.LeftShift) && !Keyboard.GetState().IsKeyDown(Keys.RightShift)) ? MathHelper.Clamp((float)(bounciness - 0.005f), (float)0f, (float)1f) : MathHelper.Clamp((float)(bounciness + 0.005f), (float)0f, (float)1f);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.G))
            {
                gravity = (!Keyboard.GetState().IsKeyDown(Keys.LeftShift) && !Keyboard.GetState().IsKeyDown(Keys.RightShift)) ? MathHelper.Clamp((float)(gravity - 0.05f), (float)-20f, (float)20f) : MathHelper.Clamp((float)(gravity + 0.05f), (float)-20f, (float)20f);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.R))
            {
                randomness = (!Keyboard.GetState().IsKeyDown(Keys.LeftShift) && !Keyboard.GetState().IsKeyDown(Keys.RightShift)) ? MathHelper.Clamp((float)(randomness - 0.05f), (float)0f, (float)20f) : MathHelper.Clamp((float)(randomness + 0.05f), (float)0f, (float)20f);
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
            Particle particle = null;
            if(emitterShape == "Square")
            {
                for (int i = 0; i < 32; i++)
                {
                    for (int j = 0; j < 32; j++)
                    {
                        if (((i == 0) || ((i == 31) || ((j == 0) && ((i > 0) && (i < 32))))) || (((j == 31) && (i > 0)) && (i < 32)))
                        {
                            particle = particleManager.getNext();
                            particle.Position = particlePosition;
                            particle.MaxAge = maxAge;
                            particle.Velocity = new Vector3((float)(i - 16), 0f, (float)(j - 16));
                            if (emitterType == "Fountain Basic")
                            {
                                particle.Velocity += (Vector3.UnitY * 5f) * (gravityAffected ? ((float)-1) : ((float)(1)));
                                particle.Acceleration = Vector3.Zero;
                            }
                            else if (emitterType == "Fountain Medium")
                            {
                                particle.Velocity += new Vector3(((((float)random.NextDouble()) - 0.5f) * 2f) * randomness, ((((float)random.NextDouble()) - 0.5f) * 2f) * randomness, ((((float)random.NextDouble()) - 0.5f) * 2f) * randomness);
                                particle.Acceleration = Vector3.UnitY * gravity;
                            }
                            else if (emitterType == "Fountain Advanced")
                            {
                                particle.Velocity += new Vector3(((((float)random.NextDouble()) - 0.5f) * 2f) * randomness, ((((float)random.NextDouble()) - 0.5f) * 2f) * randomness, ((((float)random.NextDouble()) - 0.5f) * 2f) * randomness);
                                particle.Acceleration = Vector3.UnitY * gravity;
                            }

                            if (usingUserVelocity)
                            {
                                particle.Velocity = this.velocityOverride;
                            }

                            particle.Init();
                        }
                    }
                }
            }
            else if(emitterShape == "Curve")
            {
                particle = particleManager.getNext();
                particle.Position = particlePosition;
                particle.MaxAge = maxAge;
                particle.Position += (Vector3.UnitX * 2f) * ((float)Math.Sin((double)MathHelper.ToRadians((float)(5 * particleCount))));
                particle.Velocity = Vector3.Zero;
                particleCount++;

                if (emitterType == "Fountain Basic")
                {
                    particle.Velocity += (Vector3.UnitY * 5f) * (gravityAffected ? ((float)-1) : ((float)(1)));
                    particle.Acceleration = Vector3.Zero;
                }
                else if (emitterType == "Fountain Medium")
                {
                    particle.Velocity += new Vector3(((((float)random.NextDouble()) - 0.5f) * 2f) * randomness, ((((float)random.NextDouble()) - 0.5f) * 2f) * randomness, ((((float)random.NextDouble()) - 0.5f) * 2f) * randomness);
                    particle.Acceleration = Vector3.UnitY * gravity;
                }
                else if (emitterType == "Fountain Advanced")
                {
                    particle.Velocity += new Vector3(((((float)random.NextDouble()) - 0.5f) * 2f) * randomness, ((((float)random.NextDouble()) - 0.5f) * 2f) * randomness, ((((float)random.NextDouble()) - 0.5f) * 2f) * randomness);
                    particle.Acceleration = Vector3.UnitY * gravity;
                }

                if (usingUserVelocity)
                {
                    particle.Velocity = velocityOverride;
                }
                particle.Init();
            }
            else
            {
                if(emitterShape == "Ring")
                {
                    for(int i = 0; i < 60; i++)
                    {
                        particle = particleManager.getNext();
                        particle.Position = particlePosition;
                        particle.MaxAge = maxAge;
                        particle.Velocity = new Vector3(10f * ((float)Math.Sin((double)MathHelper.ToRadians((float)(i * 6)))), 0f, 10f * ((float)Math.Cos((double)MathHelper.ToRadians((float)(i * 6)))));

                        if (emitterType == "Fountain Basic")
                        {
                            particle.Velocity += (Vector3.UnitY * 5f) * (gravityAffected ? ((float)-1) : ((float)(1)));
                            particle.Acceleration = Vector3.Zero;
                        }
                        else if (emitterType == "Fountain Medium")
                        {
                            particle.Velocity += new Vector3(((((float)random.NextDouble()) - 0.5f) * 2f) * randomness, ((((float)random.NextDouble()) - 0.5f) * 2f) * randomness, ((((float)random.NextDouble()) - 0.5f) * 2f) * randomness);
                            particle.Acceleration = Vector3.UnitY * gravity;
                        }
                        else if (emitterType == "Fountain Advanced")
                        {
                            particle.Velocity += new Vector3(((((float)random.NextDouble()) - 0.5f) * 2f) * randomness, ((((float)random.NextDouble()) - 0.5f) * 2f) * randomness, ((((float)random.NextDouble()) - 0.5f) * 2f) * randomness);
                            particle.Acceleration = Vector3.UnitY * gravity;
                        }

                        if (usingUserVelocity)
                        {
                            particle.Velocity = velocityOverride;
                        }

                        particle.Init();
                    }
                }
            }
        }

        private void UpdateParticles(GameTime gameTime)
        {
            Particle[] particles;
            Particle particle;

            particleManager.Update(gameTime.ElapsedGameTime.Milliseconds * 0.001f);
            if (emitterType != "Fountain Medium")
            {
                if (emitterType == "Advanced")
                {
                    particles = particleManager.particles;
                    for (int i = 0; i < particles.Length; i++)
                    {
                        particle = particles[i];
                        if (particle.IsActive())
                        {
                            particle.Acceleration = new Vector3(particle.Acceleration.X, this.gravity, particle.Acceleration.Z);
                            if (particle.Position.Y < 1.75f)
                            {
                                particle.Position = new Vector3(particle.Position.X, 1.75f, particle.Position.Z);
                                particle.Velocity = new Vector3(particle.Velocity.X, -particle.Velocity.Y * this.bounciness, particle.Velocity.Z);
                            }
                        }
                    }
                }
            }
            else
            {
                particles = particleManager.particles;
                for (int i = 0; i < particles.Length; i++)
                {
                    particle = particles[i];
                    if (particle.IsActive())
                    {
                        if (particle.Position.Y < 1.75f)
                        {
                            particle.Position = new Vector3(particle.Position.X, 1.75f, particle.Position.Z);
                            particle.Velocity = new Vector3(particle.Velocity.X, 0f, particle.Velocity.Z);
                            particle.Acceleration = new Vector3(particle.Acceleration.X, 0f, particle.Acceleration.Z);
                        }
                        if (particle.Position.Y == 1.75f)
                        {
                            particle.Velocity = new Vector3(particle.Velocity.X * ((float)Math.Pow((double)(1f - friction), 0.03)), 0f, particle.Velocity.Z * ((float)Math.Pow((double)(1f - friction), 0.03)));
                        }
                    }
                }
            }

            // Using Wind and randomness
            if (usingWind)
            {
                particles = this.particleManager.particles;
                for (int i = 0; i < particles.Length; i++)
                {
                    particle = particles[i];
                    if (particle.IsActive())
                    {
                        if (isRandom && ((particleTime % 60) == 0))
                        {
                            windForce = new Vector3(((((float)this.random.NextDouble()) - 0.5f) * 0.05f) * this.randomness, 0f, ((((float)this.random.NextDouble()) - 0.5f) * 0.05f) * this.randomness);
                        }
                        particle.Position += windForce;
                    }
                }
            }

            particleTime++;
            // Using Age
            //if (this.usingAge)
            //{
            //    particles = particleManager.particles;
            //    for (int i = 0; i < particles.Length; i++)
            //    {
            //        particle = particles[i];
            //        if (particle.IsActive())
            //        {
            //            particle.Age = maxAge;
            //        }
            //    }
            //}
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
            else
            {

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
                _spriteBatch.DrawString(font, "Emitter Shape: " + emitterShape, Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "Wind Force: (" + windForce.X.ToString("0.00") + ", " + windForce.Y.ToString("0.00") + ", " + windForce.Z.ToString("0.00") + ")", Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "User Velocity: (" + windForce.X.ToString("0.00") + ", " + windForce.Y.ToString("0.00") + ", " + windForce.Z.ToString("0.00") + ")", Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "b/B - Bounciness: " + bounciness, Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "f/F - Friction: " + friction, Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "r/R - Randomness: " + randomness, Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "Age of Particles: " + maxAge, Vector2.UnitX + Vector2.UnitY * 15 * (i++), Color.Black);
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
                _spriteBatch.DrawString(font, "W: Activate Wind (Advanced Only)", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
                _spriteBatch.DrawString(font, "V: User Velocity (Advanced Only)", Vector2.UnitX * 500 + Vector2.UnitY * 15 * (i++), Color.Black);
            }
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}