using System;
using GalaSoft.MvvmLight;

namespace Venus.ViewModel
{
    public class RequestCloseViewModel : ViewModelBase, IRequestCloseViewModel
    {
        public event EventHandler RequestClose;
        protected void OnRequestClose(EventArgs e)
        {
            RequestClose?.Invoke(this, e);
        }

        public override void Cleanup()
        {
            var listeners = RequestClose?.GetInvocationList();
            if (listeners?.Length > 0)
            {
                foreach (var listener in listeners)
                {
                    RequestClose -= listener as EventHandler;
                }
            }

            RequestClose = null;
            
            base.Cleanup();
        }
    }
}