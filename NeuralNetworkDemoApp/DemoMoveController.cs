using Game.Engine.Win2d;
using System.Linq;
using System.Numerics;

namespace NeuralNetworkDemoApp
{
    internal class DemoMoveController : MoveController<NeuralNetworkScene>
    {
        protected override void DoInitialize()
        {
        }

        protected override ICommand OnPointerMoved(Vector2 pointPosition, uint pointerId)
        {
            return null;
        }

        protected override ICommand OnPointerPressed(Vector2 pointPosition, uint pointerId)
        {
            var link = Scene.Links.LastOrDefault(l => l.Intersect(pointPosition));
            if (link != null)
            {
                link.ShowToolTip = true;
                link.ToolTipPosition = pointPosition;
            }

            return null;
        }

        protected override ICommand OnPointerReleased(Vector2 pointPosition, uint pointerId)
        {
            foreach (var link in Scene.Links)
            {
                link.ShowToolTip = false;
            }

            return null;
        }

        protected override void DoClean()
        {
        }
    }
}
