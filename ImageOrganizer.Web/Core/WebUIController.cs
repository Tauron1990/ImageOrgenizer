using System;
using System.Threading.Tasks;
using Tauron.Application;

namespace ImageOrganizer.Web.Core
{
    public class WebUIControllerFactory : IUIControllerFactory
    {
        private class InternUISync : IUISynchronize
        {
            public Task BeginInvoke(Action action) => Task.Run(action);

            public Task<TResult> BeginInvoke<TResult>(Func<TResult> action) => Task.Run(action);

            public void Invoke(Action action) => action();

            public TReturn Invoke<TReturn>(Func<TReturn> action) => action();

            public bool CheckAccess => true;
        }

        private class WebUIController : IUIController
        {
            public IWindow MainWindow
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }
            public ShutdownMode ShutdownMode
            {
                get => ShutdownMode.OnExplicitShutdown;
                set => throw new NotSupportedException();
            }

            public void Run(IWindow window)
            {
                
            }

            public void Shutdown()
            {
            }
        }

        public IUIController CreateController() => new WebUIController();

        public void SetSynchronizationContext() => UiSynchronize.Synchronize = new InternUISync();
    }
}