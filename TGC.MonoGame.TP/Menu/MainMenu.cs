﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Types;
using TGC.MonoGame.TP.Utils;
using TGC.MonoGame.TP.Utils.Fonts;

namespace TGC.MonoGame.TP.Menu;

public class MainMenu
{
    private ButtonsGrid Buttons;
    private Texture2D _logo;
    private Texture2D _winBackground;
    private Texture2D _gameOverBackground;
    // Background
    private Map _menuMap;
    private Camera _camera;
    
    public SpriteFont Font { get; set; }
    public SpriteBatch SpriteBatch { get; set; }
    public GraphicsDevice GraphicsDevice { get; }

    public MainMenu (GraphicsDevice graphicsDevice, GameState gameState, Map menuMap)
        {
            GraphicsDevice = graphicsDevice;
            SpriteBatch = new SpriteBatch(graphicsDevice);
            _menuMap = menuMap;
            _camera = new AngularCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0f,50f,10f), _menuMap.Player.Position, (float)Math.PI/4);
            
            var buttons = new List<Button>
            {
                new ("Nuevo Juego", GameStatus.NormalGame),
                new ("Modo Dios", GameStatus.GodModeGame),
                new ("Debug", GameStatus.DebugModeGame),
                new ("Salir", GameStatus.Exit),
            };
            Buttons = new ButtonsGrid(gameState, graphicsDevice.Viewport.Width/2, graphicsDevice.Viewport.Height/2, buttons);
        }
    
        public void LoadContent(GraphicsDevice graphicsDevice,ContentManager content)
        {
            _logo = content.Load<Texture2D>(Utils.Textures.Menu.MenuImage.Path);
            _winBackground = content.Load<Texture2D>(Utils.Textures.Menu.Win.Path);
            _gameOverBackground = content.Load<Texture2D>(Utils.Textures.Menu.GameOver.Path);
            Font = content.Load<SpriteFont>($"{ContentFolder.Fonts}/Stencil72");
            _menuMap.Load(graphicsDevice, content);
            Buttons.LoadContent(content);
        }

        public void Update(GameStatus gameStatus, GameTime gameTime)
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            if (gameStatus == GameStatus.MainMenu)
            {
                _menuMap.Update(gameTime);
                Buttons.Update(Mouse.GetState());
            }
        }
        
        public void Draw(GameStatus gameStatus, RenderTarget2D ShadowMapRenderTarget, Camera TargetLightCamera, BoundingFrustum BoundingFrustum)
        {
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            try
            {
                _menuMap.Draw(_camera, ShadowMapRenderTarget, GraphicsDevice, TargetLightCamera, BoundingFrustum);
            }
            catch (Exception e) { }
            SpriteBatch.Begin();
            var destRectangle = new Rectangle((GraphicsDevice.Viewport.Width - _logo.Width*2/3)/2,
                GraphicsDevice.Viewport.Height/50, _logo.Width*2/3, _logo.Height*2/3);
            SpriteBatch.Draw(_logo, destRectangle, Color.White);
            if (gameStatus == GameStatus.DeathMenu || gameStatus == GameStatus.WinMenu)
            {
                var destRectangle2 = new Rectangle(0,
                    0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
                var text = "";
                if(gameStatus == GameStatus.WinMenu)
                {
                    SpriteBatch.Draw(_winBackground, destRectangle2, Color.White);
                    text = "Ganaste!";
                }
                if(gameStatus == GameStatus.DeathMenu)
                {
                    SpriteBatch.Draw(_gameOverBackground, destRectangle2, Color.White);
                    text = "Perdiste!";
                }
                var size = Font.MeasureString(text);
                SpriteBatch.DrawString(Font, text, new Vector2((GraphicsDevice.Viewport.Width/2f - size.X/2), 20f), Color.DarkRed, 0f, Vector2.Zero, 1f,
                    SpriteEffects.None, 0);
                text = "Pulse espacio para volver al menu principal";
                size = Font.MeasureString(text);
                SpriteBatch.DrawString(Font, text, new Vector2((GraphicsDevice.Viewport.Width/2f - size.X/2), 20f + 50f), Color.DarkRed, 0f, Vector2.Zero, 1f,
                    SpriteEffects.None, 0);
            }
            SpriteBatch.End();
            if (gameStatus == GameStatus.MainMenu)
            {
                Buttons.Draw(SpriteBatch);
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            }
        }

        public void Dispose()
        {
            SpriteBatch.Dispose();
        }
}
