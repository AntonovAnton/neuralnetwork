using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace Game.Engine.Win2d
{
    public abstract class Scene
    {
        private readonly List<ISceneElement> _elements = new List<ISceneElement>();
        private readonly Dictionary<ISceneElement, ICommand> _commands = new Dictionary<ISceneElement, ICommand>();

        protected CanvasAnimatedControl _canvasAnimatedControl;
        public EventHandler<EventArgs> Updated;

        public Size Size { get; set; }
        
        public async Task InitializeAsync(CanvasAnimatedControl canvasAnimatedControl)
        {
            _canvasAnimatedControl = canvasAnimatedControl;
            await DoInitializeAsync();
        }

        public void AddCommand(ICommand command)
        {
            _commands[command.SceneElement] = command;
        }

        public void RemoveCommand(ICommand command)
        {
            _commands[command.SceneElement] = null;
        }

        public void Update()
        {
            foreach (var element in _elements)
            {
                var command = _commands[element];
                if (command != null)
                {
                    RemoveCommand(command);
                    if (command.CanExecute())
                    {
                        command = command.Execute();
                    }
                }

                if (command != null)
                {
                    AddCommand(command);
                }
            }

            DoUpdate();

            Updated?.Invoke(this, EventArgs.Empty);
        }

        public void Draw(CanvasDrawingSession ds, Size canvasSize)
        {
            float scale;
            var origin = GetOrigin(canvasSize, out scale);

            ds.Transform = Matrix3x2.CreateScale(scale) * Matrix3x2.CreateTranslation(origin);

            //drawing board
            //ds.DrawRectangle(new Rect(new Point(0, 0), new Size(canvasSize.Width - 1, canvasSize.Height - 1)),
            //    Colors.Black);

            DoDraw(ds);

            foreach (var sceneElement in _elements)
            {
                sceneElement.Draw(ds);
            }
        }

        protected abstract Task DoInitializeAsync();

        protected abstract void DoUpdate();
        
        protected abstract void DoDraw(CanvasDrawingSession canvasDrawingSession);

        public Vector2 GetOrigin(Size canvasSize, out float scale)
        {
            var ratioScene = Size.Width / Size.Height;
            var ratioCanvas = canvasSize.Width / canvasSize.Height;
            if (ratioScene > ratioCanvas)
            {
                scale = (float)(canvasSize.Width / Size.Width);
                return new Vector2(0, (float)((canvasSize.Height - Size.Height * scale) / 2f));
            }
            else
            {
                scale = (float)(canvasSize.Height / Size.Height);
                return new Vector2((float)((canvasSize.Width - Size.Width * scale) / 2f), 0);
            }
        }

        /// <summary> Check that a scene element can be intersected by another scene element
        /// </summary>
        /// <param name="sceneElement"></param>
        /// <returns>intersected scene element</returns>
        public ISceneElement CheckIntersect(ISceneElement sceneElement)
        {
            if (sceneElement == null)
            {
                return null;
            }

            foreach (var element in _elements)
            {
                if (element == sceneElement)
                {
                    continue;
                }

                if (sceneElement.Intersect(element))
                {
                    return element;
                }
            }

            return null;
        }

        public void AddElement(ISceneElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            _elements.Add(element);
            _commands[element] = null;
        }

        public void AddElements(IEnumerable<ISceneElement> elements)
        {
            if (elements == null)
            {
                throw new ArgumentNullException(nameof(elements));
            }

            foreach (var element in elements)
            {
                AddElement(element);
            }
        }

        public void RemoveElement(ISceneElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            if (_commands.ContainsKey(element))
            {
                _commands.Remove(element);
            }
            _elements.Remove(element);
        }
        
        public void Clean()
        {
            _commands.Clear();
            _elements.Clear();
        }
    }
}
