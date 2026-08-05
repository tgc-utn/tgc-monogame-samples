using System;
using System.Diagnostics;

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
        public SpriteFont Font;

        private bool _godMode;
        public SpriteBatch SpriteBatch;

        private int _status = ST_PRESENTACION;
        private Texture2D _texture;
        private Texture2D _textureAux;
        private float _timerLevel;
        public TunelMesh Tunel;

        /// <inheritdoc />
        public ComboRata(TGCViewer game)
            : base(game)
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
            _texture = Game.Content.Load<Texture2D>(ContentFolderTextures + "tunel/metal");
            _textureAux = Game.Content.Load<Texture2D>(ContentFolderTextures + "tunel/level2");

            // Load a shader using Content pipeline.
            Effect = Game.Content.Load<Effect>(ContentFolderEffects + "ComboRata");
            Tunel = new TunelMesh();
            Effect.Parameters["World"].SetValue(Matrix.Identity);
            var projectionMatrix =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 50, 50000);

            Game.Gizmos.UpdateViewProjection(Matrix.Identity, projectionMatrix);

            Effect.Parameters["Projection"].SetValue(projectionMatrix);
            Effect.Parameters["ModelTexture"].SetValue(_texture);
            Font = Game.Content.Load<SpriteFont>(ContentFolderSpriteFonts + "CascadiaCode/CascadiaCodePL");
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            Effect.CurrentTechnique = Effect.Techniques["ColorDrawing"];
            base.LoadContent();
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            var elapsedTime = gameTime.ElapsedGameTime.Milliseconds / 1000f;

            switch (_status)
            {
                case ST_CAMBIO_NIVEL:
                    _timerLevel -= elapsedTime;
                    if (_timerLevel < 0)
                    {
                        _status = ST_STAGE_1;
                    }

                    break;

                case ST_GAME_OVER:
                    break;

                case ST_PRESENTACION:
                    if (Game.CurrentKeyboardState.IsKeyDown(Keys.Space))
                    {
                        _status = ST_STAGE_1;
                    }

                    break;
                default:
                    {
                        var keys = Game.CurrentKeyboardState.GetPressedKeys();
                        if (keys.Length > 0)
                        {
                            if (keys[0] == Keys.G)
                            {
                                _godMode = !_godMode;
                            }
                        }

                        Tunel.Update(elapsedTime, Game.CurrentKeyboardState);
                        if (Tunel.Colision && !_godMode)
                        {
                            _status = ST_GAME_OVER;
                        }

                        var p_ant = 1 + (int)Tunel.Ant_pos;
                        var p = 1 + (int)Tunel.Pos;
                        if (p_ant != p && p % 50 == 0)
                        {
                            // paso al siguiente stage
                            _status++;
                            AdvanceStage();
                        }
                    }

                    break;
            }

            base.Update(gameTime);
        }

        private void AdvanceStage()
        {
            switch (_status)
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
                    Effect.Parameters["ModelTexture"].SetValue(_textureAux);
                    break;
                case 6:
                    // paso al siguiente nivel
                    _status = ST_CAMBIO_NIVEL;
                    Tunel.Level = 1 - Tunel.Level;
                    Tunel.FillVertices();
                    Effect.CurrentTechnique = Effect.Techniques["ColorDrawing"];
                    Effect.Parameters["ModelTexture"].SetValue(_texture);
                    _timerLevel = 2;
                    break;
                default:
                    Debug.WriteLine($"Unexpected Status value: {_status}", "ComboRata");
                    break;
            }
        }

        /// <inheritdoc />
        public override void Draw(GameTime gameTime)
        {
            Game.Background = Tunel.Colision ? Color.GreenYellow : Color.Gray;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            Tunel.Draw(GraphicsDevice, Effect);

            // Debug
            // var world = Matrix.CreateScale(Vector3.One * 40) * Matrix.CreateTranslation(Tunel.PosGamer);
            // var world = Matrix.CreateScale(Vector3.One * 10) * Matrix.CreateTranslation(Tunel.PosGamer - Tunel.ViewDir * 50);
            // var world = Matrix.CreateScale(Vector3.One * 10) * Matrix.CreateTranslation(Tunel.PosGamer + Tunel.ViewDir * 50);
            // Effect.Parameters["World"].SetValue(world);
            // Box.Draw(Effect);
            switch (_status)
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
                    DrawCenterTextY("Distancia Recorrida = " + Math.Round(Tunel.Pos, 1), 50, 1);
                    DrawCenterText("GAME OVER", 5);
                    break;
                default:
                    SpriteBatch.Begin();
                    SpriteBatch.DrawString(Font, "Distancia:" + Math.Round(Tunel.Pos, 1), new Vector2(10, 10),
                        Color.White);
                    SpriteBatch.End();
                    if (_godMode)
                    {
                        DrawRightText("GODMODE", 10, 1);
                    }

                    break;
            }

            base.Draw(gameTime);
        }

        public void DrawCenterText(string msg, float escala)
        {
            var w = GraphicsDevice.Viewport.Width;
            var h = GraphicsDevice.Viewport.Height;
            var size = Font.MeasureString(msg) * escala;
            SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null,
                Matrix.CreateScale(escala) * Matrix.CreateTranslation((w - size.X) / 2, (h - size.Y) / 2, 0));
            SpriteBatch.DrawString(Font, msg, new Vector2(0, 0), Color.YellowGreen);
            SpriteBatch.End();
        }

        public void DrawCenterTextY(string msg, float y, float escala)
        {
            var w = GraphicsDevice.Viewport.Width;
            var size = Font.MeasureString(msg) * escala;
            SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null,
                Matrix.CreateScale(escala) * Matrix.CreateTranslation((w - size.X) / 2, y, 0));
            SpriteBatch.DrawString(Font, msg, new Vector2(0, 0), Color.YellowGreen);
            SpriteBatch.End();
        }

        public void DrawRightText(string msg, float y, float escala)
        {
            var w = GraphicsDevice.Viewport.Width;
            var size = Font.MeasureString(msg) * escala;
            SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null,
                Matrix.CreateScale(escala) * Matrix.CreateTranslation(w - size.X - 20, y, 0));
            SpriteBatch.DrawString(Font, msg, new Vector2(0, 0), Color.YellowGreen);
            SpriteBatch.End();
        }
    }
}
