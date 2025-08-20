using MiniPrismKit.MVVM;
using MinitPrismKit.Services.Configuration;

namespace MinitPrismKit.MVVM
{
    public class DialogViewModelBase : ViewModelBase, IDialogAware
    {
        public DialogViewModelBase(IEventAggregator eventAggregator, IConfigService config, IContainerProvider container) : base(eventAggregator, config, container)
        {
            OkCommand = new DelegateCommand(Ok);
            CancelCommand = new DelegateCommand(Cancel);
        }

        public DialogCloseListener RequestClose { get; internal set; }

        public DelegateCommand OkCommand { get; private set; }

        public DelegateCommand CancelCommand { get; private set; }


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
                    RequestClose.Invoke(parameters, result);
                else
                    RequestClose.Invoke(result);
            }
            catch (InvalidOperationException)
            {
                // DialogService 未注入 RequestClose，忽略或记录日志
                Logger.Warning("DialogCloseListener 未初始化，无法关闭对话框");
            }
        }
    }
}
