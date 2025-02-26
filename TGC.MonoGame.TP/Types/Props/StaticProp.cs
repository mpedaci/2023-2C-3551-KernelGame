﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using TGC.MonoGame.TP.Helpers.Collisions;
using TGC.MonoGame.TP.Types.References;
using TGC.MonoGame.TP.Types.Tanks;

namespace TGC.MonoGame.TP.Types.Props;

public abstract class StaticProp : Resource
{
    private PropReference Prop;
    
    public bool Destroyed = false;
    
    // Box Parameters
    public Vector3 Position;
    
    public Matrix OBBWorld { get; set; }
    public float Angle { get; set; } = 0f;
    public Matrix Translation { get; set; }
    public BoundingBox Box { get; set; }

    public StaticProp(PropReference modelReference)
    {
        Reference = modelReference.Prop;
        Prop = modelReference;
        Position = Prop.Position;
    }
    
    public StaticProp(PropReference modelReference, Vector3 position)
    {
        Reference = modelReference.Prop;
        Prop = modelReference;
        Position = position;
    }

    public override void Load(ContentManager content)
    {
        base.Load(content);
        
        Translation = Matrix.CreateTranslation(Position);
        World = Matrix.CreateScale(Reference.Scale) * Reference.Rotation * Matrix.CreateRotationY(Angle) * Translation;
        Model.Root.Transform = World;
        Box = BoundingVolumesExtension.CreateAABBFrom(Model);
        Box = new BoundingBox(Box.Min * Reference.BBScale.X + World.Translation * Reference.BBScale.Y,
            Box.Max * Reference.BBScale.X + World.Translation * Reference.BBScale.Y);
    }

    public void Update(ICollidable collidable)
    {
        if (Destroyed) return;
        if (collidable.VerifyCollision(Box))
            CollidedWith(collidable);
    }
    public abstract void CollidedWith(ICollidable other);

    public void Reset()
    {
        Destroyed = false;
    }
}