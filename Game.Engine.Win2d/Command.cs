using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Engine.Win2d
{
    public interface ICommand
    {
        ISceneElement SceneElement { get; }
        bool CanExecute();
        ICommand Execute();
    }

    public abstract class Command<T> : ICommand where T : ISceneElement
    {
        public T SceneElement { get; }

        ISceneElement ICommand.SceneElement => SceneElement;

        protected Command(T sceneElement)
        {
            SceneElement = sceneElement;
        }

        public virtual bool CanExecute()
        {
            return true;
        }

        public abstract ICommand Execute();
    }

    public abstract class Command : Command<ISceneElement>
    {
        protected Command(ISceneElement sceneElement)
            : base(sceneElement)
        {
        }
    }

    public class NullCommand : Command
    {
        public NullCommand(ISceneElement sceneElement)
            : base(sceneElement)
        {
        }

        public override bool CanExecute()
        {
            return false;
        }

        public override ICommand Execute()
        {
            return null;
        }
    }
}