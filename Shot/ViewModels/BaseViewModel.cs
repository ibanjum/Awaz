using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Shot.Navigation;
using Shot.Services;

namespace Shot.ViewModels
{
    public abstract class BaseViewModel : NotifyPropertyChanged
    {
        private readonly INavigationService _navService;

        protected BaseViewModel(INavigationService navService)
        {
        }

        public virtual Task Init()
        {
            return Task.CompletedTask;
        }


        public virtual void OnNavigatedTo()
        {

        }

        public virtual void NavigateBack()
        {
        }
    }

    public abstract class BaseViewModel<TParameter> : BaseViewModel
    {
        public BaseViewModel(INavigationService navService) : base(navService)
        {
        }

        public abstract void Init(TParameter parameter);
    }
}
