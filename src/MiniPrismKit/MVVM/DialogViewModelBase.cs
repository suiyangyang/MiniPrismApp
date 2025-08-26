using MiniPrismKit.Services.Configuration;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Services.Dialogs;

namespace MiniPrismKit.MVVM
{
    public class DialogViewModelBase : ViewModelBase, IDialogAware
    {
        public event Action<IDialogResult> RequestClose;

        public DialogViewModelBase(IEventAggregator eventAggregator, IConfigService config, IContainerProvider container)
            : base(eventAggregator, config, container)
        {
            OkCommand = new DelegateCommand(Ok);
            CancelCommand = new DelegateCommand(Cancel);
        }

        public DelegateCommand OkCommand { get; private set; }

        public DelegateCommand CancelCommand { get; private set; }

        public string Title { get; set; }

        public virtual bool CanCloseDialog() => true;

        public virtual void OnDialogClosed()
        {
        }

        public virtual void OnDialogOpened(IDialogParameters parameters)
        {
        }

        protected virtual void Ok()
        {
            CloseDialog(ButtonResult.OK);
        }

        protected virtual void Cancel()
        {
            CloseDialog(ButtonResult.Cancel);
        }
        protected void CloseDialog(ButtonResult result = ButtonResult.None, IDialogParameters? parameters = null)
        {
            try
            {
                if (parameters != null)
                    RequestClose.Invoke(new DialogResult(result, parameters));
                else
                    RequestClose.Invoke(new DialogResult(result));
            }
            catch (InvalidOperationException)
            {
                // DialogService 未注入 RequestClose，忽略或记录日志
                Logger.Warning("DialogCloseListener 未初始化，无法关闭对话框");
            }
        }
    }
}
