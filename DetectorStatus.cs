namespace Bev.Instruments.P9710.Detector
{
    public class DetectorStatus
    {
        public string DetectorName { get; set; }
        public int SerialNumber { get; set; }
        public double CalibrationFactor { get; set; }
        public int PhotometricUnit { get; set; }
        public string CustomString { get; set; }
        public string PhotometricUnitSymbol => CodeToString(PhotometricUnit);

        private string CodeToString(int photometricUnit)
        {
            switch (photometricUnit)
            {
                case 0: return "W";
                case 1: return "W/m2";
                case 2: return "W/sr";
                case 3: return "W/m2/sr";
                case 4: return "lm";
                case 5: return "lx";
                case 6: return "cd";
                case 7: return "cd/m2";
                case 8: return "MED/h";
                case 9: return "mol/m2/s";
                case 10: return "A";
                case 11: return "cdsr";
                case 12: return "lm/sr";
                case 13: return "lm/m2";
                case 14: return "pc";
                case 15: return "fc";
                case 16: return "E/m2";
                case 17: return "W/cm2";
                case 18: return "W/cm2*sr";
                case 19: return "lm/cm2";
                case 20: return "cdsr/m2";
                case 21: return "fL";
                case 22: return "sb";
                case 23: return "L";
                case 24: return "nit";
                default: return string.Empty;
            }
        }
    }
}
