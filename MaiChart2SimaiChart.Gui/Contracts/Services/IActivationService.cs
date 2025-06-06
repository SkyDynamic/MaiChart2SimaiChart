using System.Threading.Tasks;

namespace MaiChart2SimaiChart.Gui.Contracts.Services;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}
