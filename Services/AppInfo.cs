namespace ColorChanger.Services;

internal static class AppInfo
{
    internal const string Version = "v1.1.0";

    internal static string Title => $"Color Changer For Texture {Version}";

    internal static string ItemUrl => Properties.Resources.ItemURL;

    internal static string CCUnityUrl => Properties.Resources.CCUnityURL;

    internal static string GithubUrl => Properties.Resources.GithubURL;

    internal static string UpdateCheckUrl => Properties.Resources.UpdateCheckURL;
}
