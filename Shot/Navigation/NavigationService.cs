using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Ninject;
using Xamarin.Forms;
using Shot.Models;
using Shot.ViewModels;

namespace Shot.Navigation
{
    public class NavigationService : INavigationService
    {
        private readonly INavigation navigationRoot;

        private readonly Page currentView;
        private readonly IDictionary<Type, Type> map = new Dictionary<Type, Type>();

        public NavigationService(INavigation navigation)
        {
            navigationRoot = navigation;
        }

        public bool CanGoBack
        {
            get
            {
                return navigationRoot.NavigationStack != null
                  && navigationRoot.NavigationStack.Count > 0;
            }
        }

        public async Task NavigateTo<TVM>() where TVM : BaseViewModel
        {
            await NavigateToView(typeof(TVM));

            if (navigationRoot.NavigationStack.Last().BindingContext is BaseViewModel)
            {
                await ((BaseViewModel)navigationRoot.NavigationStack.Last().BindingContext).Init();
            }
        }

        public async Task NavigateTo<TVM, TParameter>(TParameter parameter) where TVM : BaseViewModel
        {
            await NavigateToView(typeof(TVM));

            if (navigationRoot.NavigationStack
                .Last().BindingContext is BaseViewModel<TParameter>)
            {
                ((BaseViewModel<TParameter>)navigationRoot.NavigationStack.Last().BindingContext).Init(parameter);
            }
        }

        public void RegisterMapping(Type viewModel, Type view)
        {
            map.Add(viewModel, view);
        }

        public void RemoveLastView()
        {
            if (navigationRoot.NavigationStack.Any())
            {
                var lastView = navigationRoot.NavigationStack[navigationRoot.NavigationStack.Count - 2];
                navigationRoot.RemovePage(lastView);
            }
        }

        public void ClearBackStack()
        {
            if (navigationRoot.NavigationStack.Count <= 1)
            {
                return;
            }

            for (var i = 0; i < navigationRoot.NavigationStack.Count - 1; i++)
            {
                navigationRoot.RemovePage(navigationRoot.NavigationStack[i]);
            }
        }

        public async Task GoBack()
        {
            if (CanGoBack)
            {
                await navigationRoot.PopAsync(true);
            }
        }

        private async Task NavigateToView(Type viewModelType)
        {
            Type viewType;

            if (!map.TryGetValue(viewModelType, out viewType))
            {
                throw new ArgumentException("No view found in View Mapping for " + viewModelType.FullName + ".");
            }

            var constructor = viewType.GetTypeInfo()
                .DeclaredConstructors
                .FirstOrDefault(dc => dc.GetParameters().Count() <= 0);
            var view = constructor.Invoke(null) as Page;

            var vm = ((App)Application.Current).Kernel.Get(viewModelType);
            view.BindingContext = vm;

            await navigationRoot.PushAsync(view, true);
        }

        public async Task DisplayPopup(PopupModel popup)
        {
            await ((App)Application.Current).MainPage.DisplayAlert(popup.Title, popup.Text, popup.ButtonText);
        }

        public async Task<string> DisplayPrompt(PopupModel popup)
        {
            return await ((App)Application.Current).MainPage.DisplayPromptAsync(popup.Title, popup.Text, popup.ButtonText);
        }

        public async Task<string> DisplayActionSheet(ActionSheetModel actionSheetModel)
        {
            if (actionSheetModel.Buttons == null)
            {
                return await ((App)Application.Current).MainPage.DisplayActionSheet(
                    actionSheetModel.Title, actionSheetModel.Cancel, actionSheetModel.Distruction);
            }
            else
            {
                return await ((App)Application.Current).MainPage.DisplayActionSheet(
                    actionSheetModel.Title, actionSheetModel.Cancel, actionSheetModel.Distruction, actionSheetModel.Buttons);
            }
        }
    }
    public interface INavigationService
    {
        void RegisterMapping(Type viewModel, Type view);

        bool CanGoBack { get; }

        Task GoBack();

        Task DisplayPopup(PopupModel popup);

        Task<string> DisplayPrompt(PopupModel popup);

        Task NavigateTo<TVM>()
             where TVM : BaseViewModel;

        Task NavigateTo<TVM, TParameter>(TParameter parameter)
           where TVM : BaseViewModel;

        void RemoveLastView();

        void ClearBackStack();

        Task<string> DisplayActionSheet(ActionSheetModel actionSheetModel);
    }
}
