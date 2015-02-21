using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace wiki_down.tools.admin.ViewModels
{
    public class ShellViewModel : PropertyChangedBase
    {
        private bool _isProcessing;
        private bool _isIndeterminate;
        private string _currentOperation;
        private int _completeness;
        private bool _canCancel;

        public ShellViewModel()
        {
            IsProcessing = true;
            IsIndetermindate = false;
            CurrentOperation = "Faking it!";
            Completeness = 50;
            CanCancel = false;
            RootViewModels = new ObservableCollection<RootViewModel>();
            ConnectionsViewModel = new ConnectionsViewModel();
        }

        public ConnectionsViewModel ConnectionsViewModel { get; private set; }

        public ObservableCollection<RootViewModel> RootViewModels { get; private set; }

        public bool IsProcessing
        {
            get
            {
                return _isProcessing;
            }
            private set
            {
                _isProcessing = value;
                NotifyOfPropertyChange(()=>IsProcessing);
            }
        }

        public bool CanCancel
        {
            get { return _canCancel; }
            private set
            {
                _canCancel = value;
                NotifyOfPropertyChange(()=>CanCancel);
            }
        }

        public void Cancel()
        {
            IsProcessing = false;
        }

        public bool IsIndetermindate
        {
            get {  return _isIndeterminate;}
            private set
            {
                _isIndeterminate = value;
                NotifyOfPropertyChange(()=>IsIndetermindate);
            }
        }

        public string CurrentOperation
        {
            get { return _currentOperation; }
            private set
            {
                _currentOperation = value;
                NotifyOfPropertyChange(() => CurrentOperation);
            }
        }

        public int Completeness
        {
            get { return _completeness; }
            private set
            {
                _completeness = value;
                NotifyOfPropertyChange(() => Completeness);
            }
        }
    }

    public class ConnectionsViewModel : RootViewModel
    {
    }

    public abstract class RootViewModel : PropertyChangedBase
    {
    }
}
