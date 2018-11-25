using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Numerics;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace Game.Engine.Win2d
{
    public abstract class MoveController<T> where T : Scene
    {
        private CanvasAnimatedControl _canvasAnimatedControl;
        private CoreIndependentInputSource _inputSource;

        protected T Scene { get; private set; }
        
        public async Task Initialize(CanvasAnimatedControl canvasAnimatedControl, T scene)
        {
            if (canvasAnimatedControl == null)
            {
                throw new ArgumentNullException(nameof(canvasAnimatedControl));
            }

            if (scene == null)
            {
                throw new ArgumentNullException(nameof(scene));
            }

            _canvasAnimatedControl = canvasAnimatedControl;
            Scene = scene;

            await _canvasAnimatedControl.RunOnGameLoopThreadAsync(() =>
            {
                _inputSource = canvasAnimatedControl.CreateCoreIndependentInputSource(CoreInputDeviceTypes.Mouse |
                                                                                      CoreInputDeviceTypes.Pen |
                                                                                      CoreInputDeviceTypes.Touch);
                _inputSource.PointerPressed += OnPointerPressed;
                _inputSource.PointerMoved += OnPointerMoved;
                _inputSource.PointerReleased += OnPointerReleased;
            });

            DoInitialize();
        }

        protected abstract void DoInitialize();

        private void OnPointerPressed(object sender, PointerEventArgs args)
        {
            var currentPointPosition = GetCurrentPointPosition(args);
            var command = OnPointerPressed(currentPointPosition, args.CurrentPoint.PointerId);
            if (command != null)
            {
                Scene.AddCommand(command);
            }
        }
        
        private void OnPointerMoved(object sender, PointerEventArgs args)
        {
            var currentPointPosition = GetCurrentPointPosition(args);
            var command = OnPointerMoved(currentPointPosition, args.CurrentPoint.PointerId);
            if (command != null)
            {
                Scene.AddCommand(command);
            }
        }

        private void OnPointerReleased(object sender, PointerEventArgs args)
        {
            var currentPointPosition = GetCurrentPointPosition(args);
            var command = OnPointerReleased(currentPointPosition, args.CurrentPoint.PointerId);
            if (command != null)
            {
                Scene.AddCommand(command);
            }
        }

        protected abstract ICommand OnPointerPressed(Vector2 pointPosition, uint pointerId);
        protected abstract ICommand OnPointerMoved(Vector2 pointPosition, uint pointerId);
        protected abstract ICommand OnPointerReleased(Vector2 pointPosition, uint pointerId);

        private Vector2 GetCurrentPointPosition(PointerEventArgs args)
        {
            var canvasSize = _canvasAnimatedControl.Size;

            float scale;
            var origin = Scene.GetOrigin(canvasSize, out scale);

            var currentPointPosition = args.CurrentPoint.Position.ToVector2() - origin;
            currentPointPosition = currentPointPosition / scale;
            return currentPointPosition;
        }
        
        public async Task Clean()
        {
            DoClean();
            if (_inputSource != null)
            {
                await _canvasAnimatedControl.RunOnGameLoopThreadAsync(() =>
                {
                    _inputSource.PointerPressed -= OnPointerPressed;
                    _inputSource.PointerMoved -= OnPointerMoved;
                    _inputSource.PointerReleased -= OnPointerReleased;
                    _inputSource = null;
                });
            }

            _canvasAnimatedControl = null;
            Scene = null;
        }

        protected abstract void DoClean();
    }
}
