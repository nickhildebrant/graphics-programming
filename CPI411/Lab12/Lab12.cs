using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lab12
{
    public class Lab12 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        RenderTarget2D renderTarget;
        Texture2D randomNormalMap, depthAndNormalMap;
        float offset = 800f / 256f;
        float SSAORad = 0.01f;

        Model model;
        Effect effect;
        Matrix world = Matrix.Identity;
        Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, 20), new Vector3(0, 0, 0), Vector3.UnitY);
        Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 600f, 0.1f, 100f);

        VertexPositionTexture[] vertices =
        {
            new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(1, -1, 0), new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(-1, -1, 0), new Vector2(0, 1)),
            new VertexPositionTexture(new Vector3(-1, 1, 0), new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(-1, -1, 0), new Vector2(0, 1))
        };

        Vector3 cameraPosition;
        Vector3 cameraTarget;

        float cameraAngleX = -30, cameraAngleY = -30;
        float distance = 15f;

        MouseState previousMouseState;
        KeyboardState previousKeyboardState;

        public Lab12()
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

            PresentationParameters pp = GraphicsDevice.PresentationParameters;
            renderTarget = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);

            model = Content.Load<Model>("objects");
            //depthAndNormalMap = Content.Load<Texture2D>("noise");
            randomNormalMap = Content.Load<Texture2D>("noise");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

            if(Keyboard.GetState().IsKeyDown(Keys.R))
            {
                if(Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift)) { SSAORad += 0.0005f; }
                else { SSAORad -= 0.0005f; }
            }

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

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = new DepthStencilState();
            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            DrawDepthAndNormalMap();

            GraphicsDevice.SetRenderTarget(null);
            depthAndNormalMap = (Texture2D)renderTarget;
            // This block will be used later for Deferred Shading (SSAO)
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.DarkSlateBlue, 1.0f, 0);
            DrawSSAO();
            
            //using (SpriteBatch sprite = new SpriteBatch(GraphicsDevice))
            //{
            //    sprite.Begin();
            //    sprite.Draw(depthAndNormalMap, new Vector2(0, 0), null,
            //    Color.White, 0, new Vector2(0, 0), 1f, SpriteEffects.None, 0);
            //    sprite.End();
            //}

            base.Draw(gameTime);
        }

        private void DrawDepthAndNormalMap()
        {
            effect = Content.Load<Effect>("DepthAndNormal");
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
                        Matrix worldInverseTransposeMatrix = Matrix.Transpose(Matrix.Invert(mesh.ParentBone.Transform));
                        effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTransposeMatrix);

                        pass.Apply();
                        GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                        GraphicsDevice.Indices = part.IndexBuffer;
                        GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                        part.VertexOffset, part.StartIndex, part.PrimitiveCount);
                    }
                }
            }
        }

        private void DrawSSAO()
        {
            effect = Content.Load<Effect>("SSAO");

            effect.CurrentTechnique = effect.Techniques[0];
            effect.CurrentTechnique.Passes[0].Apply();
            effect.Parameters["RandomNormalTexture"].SetValue(randomNormalMap);
            effect.Parameters["DepthAndNormalTexture"].SetValue(depthAndNormalMap);
            effect.Parameters["offset"].SetValue(offset);
            effect.Parameters["rad"].SetValue(SSAORad);

            GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, vertices, 0, vertices.Length / 3);
        }
    }
}