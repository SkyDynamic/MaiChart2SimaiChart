using System.Threading.Tasks;

namespace MaiChart2SimaiChart.Gui.Activation;

public interface IActivationHandler
{
    bool CanHandle(object args);

    Task HandleAsync(object args);
}