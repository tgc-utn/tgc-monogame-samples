﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Geometries;
using TGC.MonoGame.Samples.Viewer;

namespace TGC.MonoGame.Samples.Samples.CompleteSolutions.ComboRata
{
    /// <summary>
    ///     Combo Rata:
    ///     A mini game with some effects.
    ///     Author: Mariano Banquiero
    ///     TODO needs a refactor.
    /// </summary>
    public class ComboRata : TGCSample
    {
        public const int ST_PRESENTACION = 0;
        public const int ST_STAGE_1 = 1;
        public const int ST_STAGE_2 = 2;
        public const int ST_STAGE_3 = 3;
        public const int ST_STAGE_4 = 4;
        public const int ST_STAGE_5 = 5;
        public const int ST_GAME_OVER = 99;
        public const int ST_CAMBIO_NIVEL = 98;
        public CubePrimitive Box;
        public Effect Effect;
        public SpriteFont font;

        public bool god_mode;
        public SpriteBatch spriteBatch;

        public int status = ST_PRESENTACION;
        public Texture2D Texture, TextureAux;
        public float timer_level;
        public TunelMesh Tunel;

        /// <inheritdoc />
        public ComboRata(TGCViewer game) : base(game)
        {
            Category = TGCSampleCategory.CompleteSolutions;
            Name = "Combo Rata";
            Description = "A mini game with some effects.";
        }

        /// <inheritdoc />
        public override void Initialize()
        {
            Box = new CubePrimitive(GraphicsDevice);
            base.Initialize();
        }

        /// <inheritdoc />
        protected override void LoadContent()
        {
            Texture = Game.Content.Load<Texture2D>(ContentFolderTextures + "tunel/metal");
            TextureAux = Game.Content.Load<Texture2D>(ContentFolderTextures + "tunel/level2");
            // Load a shader using Content pipeline.
            Effect = Game.Content.Load<Effect>(ContentFolderEffects + "ComboRata");
            Tunel = new TunelMesh();
            Effect.Parameters["World"].SetValue(Matrix.Identity);
            var projectionMatrix =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 50, 50000);

            Game.Gizmos.UpdateViewProjection(Matrix.Identity, projectionMatrix);

            Effect.Parameters["Projection"].SetValue(projectionMatrix);
            Effect.Parameters["ModelTexture"].SetValue(Texture);
            font = Game.Content.Load<SpriteFont>(ContentFolderSpriteFonts + "CascadiaCode/CascadiaCodePL");
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Effect.CurrentTechnique = Effect.Techniques["ColorDrawing"];
            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            var elapsedTime = gameTime.ElapsedGameTime.Milliseconds / 1000f;

            switch (status)
            {
                case ST_CAMBIO_NIVEL:
                    timer_level -= elapsedTime;
                    if (timer_level < 0) status = ST_STAGE_1;
                    break;

                case ST_GAME_OVER:
                    break;

                case ST_PRESENTACION:
                    if (Game.CurrentKeyboardState.IsKeyDown(Keys.Space))
                        status = ST_STAGE_1;
                    break;
                default:
                {
                    var keys = Game.CurrentKeyboardState.GetPressedKeys();
                    if (keys.Length > 0)
                        if (keys[0] == Keys.G)
                            god_mode = !god_mode;

                    Tunel.Update(elapsedTime, Game.CurrentKeyboardState);
                    if (Tunel.colision && !god_mode)
                        status = ST_GAME_OVER;

                    var p_ant = 1 + (int) Tunel.ant_pos;
                    var p = 1 + (int) Tunel.pos;
                    if (p_ant != p && p % 50 == 0)
                    {
                        // paso al siguiente stage
                        status++;
                        switch (status)
                        {
                            case ST_STAGE_2:
                                Effect.CurrentTechnique = Effect.Techniques["EdgeDectect"];
                                break;
                            case ST_STAGE_3:
                                Effect.CurrentTechnique = Effect.Techniques["TexCoordsDrawing"];
                                break;
                            case ST_STAGE_4:
                                Effect.CurrentTechnique = Effect.Techniques["TextureDrawing"];
                                break;
                            case ST_STAGE_5:
                                Effect.Parameters["ModelTexture"].SetValue(TextureAux);
                                break;
                            case 6:
                                // paso al siguiente nivel
                                status = ST_CAMBIO_NIVEL;
                                Tunel.level = 1 - Tunel.level;
                                Tunel.FillVertices();
                                Effect.CurrentTechnique = Effect.Techniques["ColorDrawing"];
                                Effect.Parameters["ModelTexture"].SetValue(Texture);
                                timer_level = 2;
                                break;
                        }
                    }
                }
                    break;
            }


            base.Update(gameTime);
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Tunel.colision ? Color.GreenYellow : Color.Gray;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            //

            Tunel.Draw(GraphicsDevice, Effect);
            
            // Debug
            //var world = Matrix.CreateScale(Vector3.One * 40) * Matrix.CreateTranslation(Tunel.PosGamer);
            //var world = Matrix.CreateScale(Vector3.One * 10) * Matrix.CreateTranslation(Tunel.PosGamer - Tunel.ViewDir * 50);
            //var world = Matrix.CreateScale(Vector3.One * 10) * Matrix.CreateTranslation(Tunel.PosGamer + Tunel.ViewDir * 50);
            //Effect.Parameters["World"].SetValue(world);
            //Box.Draw(Effect);

            switch (status)
            {
                case ST_CAMBIO_NIVEL:
                    DrawCenterText("NEXT LEVEL!!!!", 5);
                    break;

                case ST_PRESENTACION:
                    DrawCenterTextY("COMBO RATA ", 100, 5);
                    DrawCenterTextY("Left y Right -> girar", 300, 1);
                    DrawCenterTextY("SpaceBar -> pausa", 400, 1);
                    DrawCenterTextY("G -> Modo God", 500, 1);
                    DrawCenterTextY("Presione SPACE para comenzar", 600, 1);
                    break;

                case ST_GAME_OVER:
                    DrawCenterTextY("Distancia Recorrida = " + Math.Round(Tunel.pos, 1), 50, 1);
                    DrawCenterText("GAME OVER", 5);
                    break;
                default:
                    spriteBatch.Begin();
                    spriteBatch.DrawString(font, "Distancia:" + Math.Round(Tunel.pos, 1), new Vector2(10, 10),
                        Color.White);
                    spriteBatch.End();
                    if (god_mode)
                        DrawRightText("GODMODE", 10, 1);

                    break;
            }

            base.Draw(gameTime);
        }

        public void DrawCenterText(string msg, float escala)
        {
            var W = GraphicsDevice.Viewport.Width;
            var H = GraphicsDevice.Viewport.Height;
            var size = font.MeasureString(msg) * escala;
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null,
                Matrix.CreateScale(escala) * Matrix.CreateTranslation((W - size.X) / 2, (H - size.Y) / 2, 0));
            spriteBatch.DrawString(font, msg, new Vector2(0, 0), Color.YellowGreen);
            spriteBatch.End();
        }

        public void DrawCenterTextY(string msg, float Y, float escala)
        {
            var W = GraphicsDevice.Viewport.Width;
            var H = GraphicsDevice.Viewport.Height;
            var size = font.MeasureString(msg) * escala;
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null,
                Matrix.CreateScale(escala) * Matrix.CreateTranslation((W - size.X) / 2, Y, 0));
            spriteBatch.DrawString(font, msg, new Vector2(0, 0), Color.YellowGreen);
            spriteBatch.End();
        }

        public void DrawRightText(string msg, float Y, float escala)
        {
            var W = GraphicsDevice.Viewport.Width;
            var H = GraphicsDevice.Viewport.Height;
            var size = font.MeasureString(msg) * escala;
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null,
                Matrix.CreateScale(escala) * Matrix.CreateTranslation(W - size.X - 20, Y, 0));
            spriteBatch.DrawString(font, msg, new Vector2(0, 0), Color.YellowGreen);
            spriteBatch.End();
        }
    }
}