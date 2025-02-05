using System;

// This file is auto-generated. Do not modify or move this file.

namespace SuperUnityBuild.Generated
{
    public enum ReleaseType
    {
        None,
        Main,
    }

    public enum Platform
    {
        None,
        Linux,
        macOS,
        WebGL,
        PC,
    }

    public enum ScriptingBackend
    {
        None,
        Mono,
        IL2CPP,
    }

    public enum Architecture
    {
        None,
        Linux_x64,
        macOS,
        WebGL,
        Windows_x86,
    }

    public enum Distribution
    {
        None,
    }

    public static class BuildConstants
    {
        public static readonly DateTime buildDate = new DateTime(638742998354655040);
        public const string version = "1.0.0.1";
        public const ReleaseType releaseType = ReleaseType.Main;
        public const Platform platform = Platform.PC;
        public const ScriptingBackend scriptingBackend = ScriptingBackend.Mono;
        public const Architecture architecture = Architecture.Windows_x86;
        public const Distribution distribution = Distribution.None;
    }
}

