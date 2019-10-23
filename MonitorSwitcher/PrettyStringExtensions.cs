using static MonitorSwitcher.WinApi.DataTypes;

namespace MonitorSwitcher
{
    public static class PrettyStringExtensions
    {
        public static string ToPretty(this DisplayConfigSourceMode mode)
        {
            return $"{mode.width}x{mode.height} @ {mode.position.ToPretty()}";
        }

        public static string ToPretty(this DisplayConfigTargetMode mode)
        {
            return $"{mode.targetVideoSignalInfo.ToPretty()}";
        }

        public static string ToPretty(this DisplayConfigVideoSignalInfo video)
        {
            return $"{video.activeSize.ToPretty()} {video.videoStandard} {video.vSyncFreq.ToPretty()}Hz";
        }

        public static string ToPretty(this DisplayConfig2DRegion region)
        {
            return $"{region.cx}x{region.cy}";
        }
        public static string ToPretty(this DisplayConfigRational ratio)
        {
            return $"{(double)ratio.numerator / (double)ratio.denominator}";
        }
        public static string ToPretty(this DisplayConfigPathTargetInfo path)
        {
            return $"{path.id} mi={path.modeInfoIdx} avail={path.targetAvailable} status={path.statusFlags} {path.outputTechnology} {(double)path.refreshRate.numerator / (double)path.refreshRate.denominator}";
        }
        public static string ToPretty(this DisplayConfigPathSourceInfo path)
        {
            return $"{path.id} mi={path.modeInfoIdx} status={path.statusFlags}";
        }

        public static string ToPretty(this PointL point)
        {
            return $"({point.x}, {point.y})";
        }


    }
}
