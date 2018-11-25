using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace Game.Engine.Win2d
{
    public sealed class GameLoopThread
    {
        //public static readonly GameLoopThread Instance = new GameLoopThread();

        private CanvasAnimatedControl _canvasAnimatedControl;

        public void Initialize(CanvasAnimatedControl canvasAnimatedControl)
        {
            if (canvasAnimatedControl == null)
            {
                throw new ArgumentNullException(nameof(canvasAnimatedControl));
            }

            _canvasAnimatedControl = canvasAnimatedControl;
        }

        public async Task RunAsync(DispatchedHandler dispatchedHandler)
        {
            if (_canvasAnimatedControl == null)
            {
                throw new InvalidOperationException("This class GameLoopThread is not initialized");
            }

            await _canvasAnimatedControl.RunOnGameLoopThreadAsync(dispatchedHandler);
        }

        public void Clean()
        {
            _canvasAnimatedControl = null;
        }
    }
}
