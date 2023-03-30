namespace SingularityGroup.HotReload {
    internal static class PackageConst {
        public const string Version = "1.8.2";
        // Never higher than Version
        // Used for the download
        public const string ServerVersion = "1.8.1";
        public const string PackageName = "com.singularitygroup.hotreload";
        public const string LibraryCachePath = "Library/" + PackageName;
        public const string ConfigFileName = "hot-reload-config.json";
    }
}
