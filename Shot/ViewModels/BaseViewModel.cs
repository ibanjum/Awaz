using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Shot.Navigation;


namespace Shot.ViewModels
{
    public abstract class BaseViewModel : NotifyPropertyChanged
    {
        protected BaseViewModel(INavigationService navService)
        {
        }

        public Task Init()
        {
            return Task.CompletedTask;
        }


        public virtual void OnNavigatedTo()
        {
            // called when view appearing
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
