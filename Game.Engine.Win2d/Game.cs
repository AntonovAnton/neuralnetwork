using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Media.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace Game.Engine.Win2d
{
    public class Game<T, S>
        where T : MoveController<S>
        where S : Scene
    {
        private readonly CanvasAnimatedControl _canvasAnimatedControl;
        private S _scene;
        private T _moveLookController;

        public Game(CanvasAnimatedControl canvasAnimatedControl)
        {
            if (canvasAnimatedControl == null)
            {
                throw new ArgumentNullException(nameof(CanvasAnimatedControl));
            }

            _canvasAnimatedControl = canvasAnimatedControl;
        }

        public S Scene => _scene;

        public async Task Start(S scene)
        {
            _scene = scene;
            await _scene.InitializeAsync(_canvasAnimatedControl);

            _moveLookController = Activator.CreateInstance<T>();
            await _moveLookController.Initialize(_canvasAnimatedControl, _scene);

            _canvasAnimatedControl.Update += OnUpdate;
            _canvasAnimatedControl.Draw += OnDraw;
        }

        private void OnUpdate(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {
            _scene.Update();
        }

        public async Task UpdateAsync()
        {
            await _canvasAnimatedControl.RunOnGameLoopThreadAsync(() => { _scene.Update(); });
        }

        private void OnDraw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            _scene.Draw(args.DrawingSession, sender.Size);
        }

        public async Task Stop()
        {
            _canvasAnimatedControl.Update -= OnUpdate;
            _canvasAnimatedControl.Draw -= OnDraw;

            if (_moveLookController != null)
            {
                await _moveLookController.Clean();
                _moveLookController = null;
            }

            if (_scene != null)
            {
                _scene.Clean();
                _scene = null;
            }
        }
    }
}
