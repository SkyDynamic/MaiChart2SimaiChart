using System.Threading.Tasks;
using MaiChart2SimaiChart.Gui.Contracts.Services;
using MaiChart2SimaiChart.Gui.ViewModels;
using Microsoft.UI.Xaml;

namespace MaiChart2SimaiChart.Gui.Activation;

public class DefaultActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
{
    private readonly INavigationService _navigationService;

    public DefaultActivationHandler(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
        return _navigationService.Frame?.Content == null;
    }

    protected async override Task HandleInternalAsync(LaunchActivatedEventArgs args)
    {
        _navigationService.NavigateTo(typeof(ExportChartViewModel).FullName!, args.Arguments);

        await Task.CompletedTask;
    }
}