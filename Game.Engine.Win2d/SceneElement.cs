using System;
using System.Numerics;
using Microsoft.Graphics.Canvas;

namespace Game.Engine.Win2d
{
    public interface ISceneElement
    {
        void Draw(CanvasDrawingSession ds);

        bool Intersect(ISceneElement other);
    }

    public abstract class SceneElement<T> : ISceneElement
        where T : Scene
    {
        public T Scene { get; private set; }

        protected SceneElement(T scene)
        {
            if (scene == null)
            {
                throw new ArgumentNullException(nameof(scene));
            }

            Scene = scene;
        }

        public void Draw(CanvasDrawingSession ds)
        {
            DoDraw(ds);
        }

        public bool Intersect(ISceneElement other)
        {
            if (other == null || this == other)
            {
                return false;
            }

            return DoIntersect(other);
        }

        protected abstract bool DoIntersect(ISceneElement other);

        protected abstract void DoDraw(CanvasDrawingSession ds);
    }

    public abstract class SceneElement : SceneElement<Scene>
    {
        protected SceneElement(Scene scene) : base(scene)
        {
        }
    }
}