using Content.Shared._Sunrise.SunriseCCVars;
using Content.Sunrise.Interfaces.Client;
using Content.Sunrise.Interfaces.Shared;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Configuration;
using Robust.Shared.Player;

namespace Content.Client._Sunrise.UserProfile;

[GenerateTypedNameReferences]
public sealed partial class UserProfile : Control
{
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;
    [Dependency] private readonly IUriOpener _uri = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    private readonly IClientServiceAuthManager? _serviceAuthManager;
    private readonly ISharedSponsorsManager? _sponsorsManager;

    private string _donateUrl = string.Empty;

    public UserProfile()
    {
        IoCManager.InjectDependencies(this);
        RobustXamlLoader.Load(this);

        IoCManager.Instance!.TryResolveType(out _sponsorsManager);
        IoCManager.Instance!.TryResolveType(out _serviceAuthManager);

        _cfg.OnValueChanged(SunriseCCVars.InfoLinksDonate, OnInfoLinksDonateChanged, true);

        if (_serviceAuthManager == null)
        {
            LinkDiscordButton.Disabled = true;
            LinkTelegramButton.Disabled = true;
        }
        else
        {
            LinkTelegramButton.OnPressed += LinkTelegramPressed;
            LinkDiscordButton.OnPressed += LinkDiscordPressed;
            _serviceAuthManager.LoadedServiceLinkedServices += RefreshServiceLinkedServices;
        }

        if (_sponsorsManager == null)
        {
            BuySponsorButton.Disabled = true;
            InfoSponsorButton.Disabled = true;
        }
        else
        {
            BuySponsorButton.OnPressed += BuySponsorPressed;
            _sponsorsManager.LoadedSponsorData += RefreshSponsorData;
            InfoSponsorButton.OnPressed += InfoSponsorPressed;
        }
    }

    private void OnInfoLinksDonateChanged(string url)
    {
        BuySponsorButton.Disabled = url == "";
        _donateUrl = url;
    }

    private void RefreshSponsorData()
    {
        if (_sponsorsManager == null)
            return;

        if (_playerManager.LocalSession != null)
        {
            if (_sponsorsManager.ClientIsSponsor())
            {
                _sponsorsManager.TryGetOocTitle(_playerManager.LocalSession.UserId, out var sponsorTitle);
                SponsorTierName.Text = sponsorTitle;
            }
            else
            {
                SponsorTierName.Text = Loc.GetString("user-profile-no-sponsor");
            }
        }
    }

    private void RefreshServiceLinkedServices(List<LinkedServiceData> linkedServices)
    {
        if (_serviceAuthManager == null)
            return;

        foreach (var serviceAuthData in _serviceAuthManager.ServiceLinkedServices)
        {
            switch (serviceAuthData.ServiceType)
            {
                case ServiceType.Discord:
                    LinkDiscordButton.Visible = false;
                    LinkedDiscordName.Text = serviceAuthData.Username;
                    break;
                case ServiceType.Telegram:
                    LinkTelegramButton.Visible = false;
                    LinkedTelegramName.Text = serviceAuthData.Username;
                    break;
            }
        }
    }

    private void BuySponsorPressed(BaseButton.ButtonEventArgs obj)
    {
        _uri.OpenUri(_donateUrl);
    }

    private void LinkTelegramPressed(BaseButton.ButtonEventArgs obj)
    {
        if (_serviceAuthManager == null)
            return;

        _serviceAuthManager.ToggleWindow(ServiceType.Telegram);
    }

    private void LinkDiscordPressed(BaseButton.ButtonEventArgs obj)
    {
        if (_serviceAuthManager == null)
            return;

        _serviceAuthManager.ToggleWindow(ServiceType.Discord);
    }

    private void InfoSponsorPressed(BaseButton.ButtonEventArgs obj)
    {

    }
}
