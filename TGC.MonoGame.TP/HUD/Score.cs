using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Types;
using TGC.MonoGame.TP.Types.Tanks;
using TGC.MonoGame.TP.Utils;
using TGC.MonoGame.TP.Utils.Models;

namespace TGC.MonoGame.TP.HUD;

public class Score : ScreenText
{
    private float _limit { get; set;}
    private float _score { get; set; }
    
    public Score(GraphicsDevice graphicsDevice, float limit) : base(graphicsDevice)
    {
        _limit = limit;
        LogoScale = 0.12f;
        TextScale = 1.75f;
    }

    internal override Vector2 LogoLocation() => new Vector2(10f , 10f);
    internal override Vector2 TextLocation() => new Vector2(10f + Logo.Width / 5f, 10f);

    internal override string TextToDraw() => _score.ToString("0") + "/ " + _limit.ToString("0");
    
    public override void LoadContent(ContentManager content)
    { 
        Font = content.Load<SpriteFont>($"{ContentFolder.Fonts}/Stencil16");
        Logo = content.Load<Texture2D>($"{ContentFolder.Images}/score");
    }
    
    public void Update(List<Tank> team)
    {
        _score = team.Where(tank => tank.Action.isEnemy).ToList().Sum(tank => tank.Deaths);
    }
    
    public bool HasWon()
    {
        return _score >= _limit;
    }
}