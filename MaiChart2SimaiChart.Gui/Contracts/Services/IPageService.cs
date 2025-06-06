using System;

namespace MaiChart2SimaiChart.Gui.Contracts.Services;

public interface IPageService
{
    Type GetPageType(string key);
}