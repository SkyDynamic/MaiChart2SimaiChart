namespace MaiChart2SimaiChart.Gui.Contracts.ViewModel;

public interface INavigationAware
{
    void OnNavigatedTo(object parameter);

    void OnNavigatedFrom();
}