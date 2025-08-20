using Serilog;
using System.Windows;

namespace MiniPrismKit.Services.Exceptions
{
    public interface IExceptionHandler
    {
        void Handle(Exception ex);
    }

    public class DefaultExceptionHandler : IExceptionHandler
    {
        public void Handle(Exception ex)
        {
            Log.Error(ex, "全局异常");

            try
            {
                MessageBox.Show(
                    ex.Message,
                    "应用程序错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            catch
            {
                // 如果UI不可用（比如服务环境），忽略MessageBox
            }
        }
    }
}
