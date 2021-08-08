namespace Bev.Instruments.P9710.Detector
{
    public static class DumpDescription
    {
        private static string notUsed = "< not used >";

        public static string ForLine(int i)
        {
            switch (i)
            {
                case 0:  return "Identification string - letter 1";
                case 1:  return "Identification string - letter 2";
                case 2:  return "Identification string - letter 3";
                case 3:  return "Identification string - letter 4";
                case 4:  return "Identification string - letter 5";
                case 5:  return "Identification string - letter 6";
                case 6:  return "Serial number - LSB";
                case 7:  return "Serial number - MSB";
                case 8:  return notUsed;
                case 9:  return notUsed;
                case 10: return notUsed;
                case 11: return notUsed;
                case 12: return notUsed;
                case 13: return notUsed;
                case 14: return notUsed;
                case 15: return notUsed;
                case 16: return "Custom string - letter 1";
                case 17: return "Custom string - letter 2";
                case 18: return "Custom string - letter 3";
                case 19: return "Custom string - letter 4";
                case 20: return "Custom string - letter 5";
                case 21: return "Custom string - letter 6";
                case 22: return "Custom string - letter 7";
                case 23: return "Custom string - letter 8";
                case 24: return "Custom string - letter 9";
                case 25: return "Custom string - letter 10";
                case 26: return "Custom string - letter 11";
                case 27: return "Custom string - letter 12";
                case 28: return "Custom string - letter 13";
                case 29: return "Custom string - letter 14";
                case 30: return "Custom string - letter 15";
                case 31: return "Custom string - letter 16";
                case 32: return notUsed;
                case 33: return notUsed;
                case 34: return notUsed;
                case 35: return notUsed;
                case 36: return notUsed;
                case 37: return notUsed;
                case 38: return notUsed;
                case 39: return notUsed;
                case 40: return notUsed;
                case 41: return notUsed;
                case 42: return notUsed;
                case 43: return notUsed;
                case 44: return notUsed;
                case 45: return notUsed;
                case 46: return notUsed;
                case 47: return notUsed;
                case 48: return "Detector name - letter 1";
                case 49: return "Detector name - letter 2";
                case 50: return "Calibration factor - LSB";
                case 51: return "Calibration factor - MSB";
                case 52: return "Calibration factor - exponent";
                case 53: return "Bit pattern: flag, unit, sign of calibration factor";
                case 54: return "Detector name - letter 3";
                case 55: return "Detector name - letter 4";
                case 56: return notUsed;
                case 57: return notUsed;
                case 58: return notUsed;
                case 59: return notUsed;
                case 60: return notUsed;
                case 61: return notUsed;
                case 62: return notUsed;
                case 63: return notUsed;
                default: return string.Empty; ;
            }

        }
    }
}
