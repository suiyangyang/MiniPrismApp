﻿using Prism.Ioc;

namespace MiniPrismKit.MVVM
{
    public interface IViewModelResolver
    {
        object ResolveViewModelForView(object view, Type viewModelType);

        IViewModelResolver IfInheritsFrom<TView, TViewModel>(Action<TView, TViewModel, IContainerProvider> configuration);

        IViewModelResolver IfInheritsFrom<TView>(Type genericInterfaceType, Action<TView, object, IGenericInterface, IContainerProvider> configuration);
    }
}
