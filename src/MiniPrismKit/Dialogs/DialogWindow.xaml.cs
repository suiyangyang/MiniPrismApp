using Prism.Services.Dialogs;
using System.Windows;

namespace MiniPrismKit.Dialogs
{
    public partial class DialogWindow : Window, IDialogWindow
    {
        public DialogWindow()
        {
            InitializeComponent();
        }

        public IDialogResult Result { get; set; }
    }
}
