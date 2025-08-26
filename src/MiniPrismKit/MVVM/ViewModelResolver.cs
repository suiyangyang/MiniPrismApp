﻿using MiniPrismKit.Extensions;
using MiniPrismKit.Utils;
using Prism.Ioc;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace MiniPrismKit.MVVM
{
    public class ViewModelResolver : IViewModelResolver
    {
        private readonly Func<IContainerProvider> _containerFactory;
        private Action<object, object, IContainerProvider> _configureViewAndViewModel;
        private IContainerProvider _container;

        public IContainerProvider Container => _container ?? (_container = _containerFactory());

        public ViewModelResolver(Func<IContainerProvider> containerFactory)
        {
            Guards.ThrowIfAnyNull(containerFactory);

            _containerFactory = containerFactory;
        }

        public object ResolveViewModelForView(object view, Type viewModelType)
        {
            var viewModel = Container.Resolve(viewModelType);
            _configureViewAndViewModel?.Invoke(view, viewModel, Container);

            return viewModel;
        }

        public IViewModelResolver IfInheritsFrom<TView, TViewModel>(Action<TView, TViewModel, IContainerProvider> configuration)
        {
            var previousAction = _configureViewAndViewModel;
            _configureViewAndViewModel = (view, viewModel, container) =>
            {
                previousAction?.Invoke(view, viewModel, container);
                if (view is TView tView && viewModel is TViewModel tViewModel)
                {
                    configuration?.Invoke(tView, tViewModel, container);
                }
            };
            return this;
        }

        public IViewModelResolver IfInheritsFrom<TView>(Type genericInterfaceType, Action<TView, object, IGenericInterface, IContainerProvider> configuration)
        {
            var previousAction = _configureViewAndViewModel;
            _configureViewAndViewModel = (view, viewModel, container) =>
            {
                previousAction?.Invoke(view, viewModel, container);
                var interfaceInstance = viewModel.AsGenericInterface(genericInterfaceType);
                if (view is TView tView && interfaceInstance != null)
                {
                    configuration?.Invoke(tView, viewModel, interfaceInstance, container);
                }
            };
            return this;
        }
    }

    public static class ViewModelResolverExtensions
    {
        public static IViewModelResolver UseDefaultConfigure(this IViewModelResolver @this) => @this
            .IfInheritsFrom<ViewModelBase>((view, viewModel) =>
            {
                Debug.WriteLine($"*********{viewModel.GetType()}");
                viewModel.Dispatcher = view.Dispatcher;
            })
            .IfInheritsFrom<IViewLoadedAndUnloadedAware>((view, viewModel) =>
            {
                view.Initialized += (sender, e) => viewModel.OnInitialize();
                view.Loaded += (sender, e) => viewModel.OnLoaded();
                view.Unloaded += (sender, e) => viewModel.OnUnloaded();
            })
            .IfInheritsFrom<ITabItemLoadedAndUnloadedAware>((view, viewModel) =>
            {
                view.Initialized += (sender, e) => viewModel.OnInitialize();
                view.Loaded += (sender, e) =>
                {
                    if (!viewModel.IsFirstLoaded)
                    {
                        viewModel.OnTabControlLoaded();
                        TabControl tabControl = view.GetParent<TabControl>();
                        if (tabControl != null)
                        {
                            tabControl.Loaded += (sender, e) => viewModel.OnTabControlLoaded();
                            tabControl.Unloaded += (sender, e) => viewModel.OnTabControlUnloaded();
                        }
                        viewModel.IsFirstLoaded = true;
                    }
                };
            })
            .IfInheritsFrom(typeof(IViewLoadedAndUnloadedAware<>), (view, viewModel, interfaceInstance) =>
            {
                var viewType = view.GetType();
                if (interfaceInstance.GenericArguments.Single() != viewType)
                {
                    throw new InvalidOperationException();
                }

                var onLoadedMethod = interfaceInstance.GetMethod<Action<object>>("OnLoaded", viewType);
                var onUnloadedMethod = interfaceInstance.GetMethod<Action<object>>("OnUnloaded", viewType);
                var OnInitializeMethod = interfaceInstance.GetMethod<Action<object>>("OnInitialize", viewType);
                view.Loaded += (sender, args) => onLoadedMethod(sender);
                view.Unloaded += (sender, args) => onUnloadedMethod(sender);
                view.Initialized += (sender, e) => OnInitializeMethod(sender);
            });

        public static IViewModelResolver IfInheritsFrom<TViewModel>(this IViewModelResolver @this, Action<FrameworkElement, TViewModel> configuration)
        {
            Guards.ThrowIfAnyNull(@this, configuration);

            return @this.IfInheritsFrom<FrameworkElement, TViewModel>((view, viewModel, container) => configuration(view, viewModel));
        }

        public static IViewModelResolver IfInheritsFrom<TViewModel>(this IViewModelResolver @this, Action<FrameworkElement, TViewModel, IContainerProvider> configuration)
        {
            Guards.ThrowIfAnyNull(@this, configuration);

            return @this.IfInheritsFrom(configuration);
        }

        public static IViewModelResolver IfInheritsFrom(this IViewModelResolver @this, Type genericInterfaceType, Action<FrameworkElement, object, IGenericInterface> configuration)
        {
            Guards.ThrowIfAnyNull(@this, configuration);

            return @this.IfInheritsFrom<FrameworkElement>(
                genericInterfaceType,
                (view, viewModel, interfaceInstance, container) => configuration(view, viewModel, interfaceInstance));
        }
    }
}