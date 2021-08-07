namespace Bev.Instruments.P9710.Detector
{
    public record DetectorStatus
    {
        public string? DetectorName { get; set; } = null;
        public int? SerialNumber { get; set; } = null;
        public double? CalibrationFactor { get; set; } = null;
        public int? PhotometricUnit { get; set; } = null;
        public string? CustomString { get; set; } = null;

        public string? PhotometricUnitSymbol => CodeToString(PhotometricUnit);

        private string CodeToString(int? photometricUnit)
        {
            if (photometricUnit == null)
                return null;
            return photometricUnit! switch
            {
                0 => "W",
                1 => "W/m2",
                2 => "W/sr",
                3 => "W/m2/sr",
                4 => "lm",
                5 => "lx",
                6 => "cd",
                7 => "cd/m2",
                8 => "MED/h",
                9 => "mol/m2/s",
                10 => "A",
                11 => "cdsr",
                12 => "lm/sr",
                13 => "lm/m2",
                14 => "pc",
                15 => "fc",
                16 => "E/m2",
                17 => "W/cm2",
                18 => "W/cm2*sr",
                19 => "lm/cm2",
                20 => "cdsr/m2",
                21 => "fL",
                22 => "sb",
                23 => "L",
                24 => "nit",
                _ => string.Empty,
            };
        }
    }
}
