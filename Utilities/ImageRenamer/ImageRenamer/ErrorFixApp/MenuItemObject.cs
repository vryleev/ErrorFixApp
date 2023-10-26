using System.Windows.Input;

namespace ErrorFixApp
{
    public class MenuItemObject:BaseObservableObject
    {
        private ICommand _command;
        private string _content;

        public ICommand Command
        {
            get { return _command; }
            set
            {
                _command = value;
                OnPropertyChanged();
            }
        }

        public string Content
        {
            get { return _content; }
            set
            {
                _content = value;
                OnPropertyChanged();
            }
        }
    }
}