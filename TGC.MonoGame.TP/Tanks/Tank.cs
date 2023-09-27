using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Collisions;
using TGC.MonoGame.TP.References;

namespace TGC.MonoGame.TP.Tanks;

public class Tank : ICollidable
{
    private Model Model;
    private ModelReference Reference;
    private Effect Effect;
    private Vector3 Position;
    public Matrix World;

    private float _velocidad;
    private Matrix _rotacion;
    
    public Tank(ModelReference model, Vector3 position)
    {
        Reference = model;
        Position = position;
        World = Matrix.CreateScale(Reference.Scale) * Reference.Rotation * Matrix.CreateTranslation(position);
        
        _velocidad = 0;
        _rotacion = Matrix.Identity;
    }

    public void Load(ContentManager content, Effect effect)
    {
        Model = content.Load<Model>(Reference.Path);
        Effect = effect;
        foreach (var modelMeshPart in Model.Meshes.SelectMany(tankModelMesh => tankModelMesh.MeshParts))
        {
            modelMeshPart.Effect = Effect;
        }
    }

    public void Draw(Matrix view, Matrix projection)
    {
        Model.Root.Transform = World;

        // Para dibujar le modelo necesitamos pasarle informacion que el efecto esta esperando.
        Effect.Parameters["View"].SetValue(view);
        Effect.Parameters["Projection"].SetValue(projection);
        Effect.Parameters["DiffuseColor"].SetValue(Reference.Color.ToVector3());

        // Draw the model.
        foreach (var mesh in Model.Meshes)
        {
            Effect.Parameters["World"].SetValue(mesh.ParentBone.Transform * World);
            mesh.Draw();
        }
    }

    public void CollidedWithSmallProp()
    {
        Console.WriteLine("Chocaste con prop chico");
        // TODO frenar un poco el tanque
    }

    public void CollidedWithLargeProp()
    {
        Console.WriteLine("Chocaste con prop grande");
        // TODO frenar el tanque del todo
    }

    public void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.W))
        {
            // Avanzo
            _velocidad += 0.01f;
        }
        if (Keyboard.GetState().IsKeyDown(Keys.S))
        {
            // Retrocedo
            _velocidad -= 0.01f;
        }
        if (Keyboard.GetState().IsKeyDown(Keys.A))
        {
            // Giro izq
            _rotacion *= Matrix.CreateRotationY(0.04f);
        }
        if (Keyboard.GetState().IsKeyDown(Keys.D))
        {
            // Giro der
            _rotacion *= Matrix.CreateRotationY(-0.04f);
        }
        
        Position += Vector3.Transform(Vector3.Forward, _rotacion) * _velocidad * gameTime.ElapsedGameTime.Milliseconds;
        Move(Position,_rotacion);
        _velocidad = Math.Max(0, _velocidad-0.008f);
    }

    public void Move(Vector3 position, Matrix rotation)
    {
        World = rotation * Matrix.CreateTranslation(position) * Matrix.CreateScale(Reference.Scale);
    }
}