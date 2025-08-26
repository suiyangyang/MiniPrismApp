using System.Windows;
using System.Windows.Media;

namespace MiniPrismKit.Utils
{
    public static class VisualTreeHelperEx
    {
        /// <summary>
        /// 查找指定类型的子节点
        /// </summary>
        /// <typeparam name="T">查找的类型</typeparam>
        /// <param name="obj">指定的元素</param>
        /// <param name="name">子节点的名称，不传入时，找到第一个类型匹配的就会返回</param>
        /// <returns></returns>
        public static T GetChild<T>(this DependencyObject obj, string name = null) where T : FrameworkElement
        {
            DependencyObject child = null;
            T grandChild = null;
            for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(obj) - 1; i++)
            {
                child = VisualTreeHelper.GetChild(obj, i);
                if (string.IsNullOrEmpty(name) && child is T)
                {
                    return (T)child;
                }
                else if (child is T && ((T)child).Name == name)
                {
                    return (T)child;
                }
                else
                {
                    grandChild = GetChild<T>(child, name);
                    if (grandChild != null)
                        return grandChild;
                }
            }

            return null;
        }

        /// <summary>
        /// 查找指定类型的父节点
        /// </summary>
        /// <typeparam name="T">指定的类型</typeparam>
        /// <param name="obj">当前元素</param>
        /// <returns></returns>
        public static T GetParent<T>(this FrameworkElement obj) where T : FrameworkElement
        {
            FrameworkElement parent = VisualTreeHelper.GetParent(obj) as FrameworkElement;
            if (parent == null)
                return null;
            if (parent is T)
                return parent as T;
            return GetParent<T>(parent);
        }
    }
}