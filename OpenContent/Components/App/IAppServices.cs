using Satrabel.OpenContent.Components.Dnn;
using Satrabel.OpenContent.Components.Files;
using Satrabel.OpenContent.Components.Localization;
using System;

namespace Satrabel.OpenContent.Components
{
    public interface IAppServices
    {
        ILogAdapter Logger { get; }
        ILocalizationAdapter Localizer { get; }
        ICacheAdapter CacheAdapter { get; }
        IGlobalSettingsRepository CreateGlobalSettingsRepository(int tenantId = -1);
        IClientResourceManager ClientResourceManager { get; }
        ILogAdapter CreateLogger(Type type);
    }
}