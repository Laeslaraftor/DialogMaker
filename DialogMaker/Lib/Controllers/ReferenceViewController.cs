using DialogMaker.Lib.Elements;
using System.Windows.Input;

namespace DialogMaker.Lib.Controllers
{
    public class ReferenceViewController : IDisposable
    {
        public ReferenceViewController(ReferenceView view)
        {
            _view = view;
        }
        ~ReferenceViewController()
        {
            Dispose();
        }

        private readonly ReferenceView _view;

        #region Управление

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        #endregion

        #region События


        #endregion
    }
}
