using System;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace Bev.Instruments.P9710.Detector
{
    public class P9710Detector
    {
        private const int secretCode = 9957;    // this password was found by brute force
        private const int blockSize = 64;
        private readonly SerialPort comPort;
        private const int waitOnClose = 100;

        public P9710Detector(string portName)
        {
            DevicePort = portName.Trim();
            comPort = new SerialPort(DevicePort, 9600);
        }

        public string DevicePort { get; }

        public void WriteDetectorStatusToRam(DetectorStatus detectorStatus)
        {
            WriteIdentificationStringToRam();
            if (detectorStatus.SerialNumber is int sn)
                WriteSerialNumberToRam(sn);
            if (detectorStatus.DetectorName is string dn)
                WriteDetectorNameToRam(dn);
            if (detectorStatus.CalibrationFactor is double cf)
                WriteCalibrationFactorToRam(cf);
            if (detectorStatus.PhotometricUnit is int pu)
                WriteUnitToRam(pu);
            if (detectorStatus.CustomString is string cs)
                WriteCustomStringToRam(cs);
        }

        public void WriteIdentificationStringToRam()
        {
            WriteStringToRam("PT9610", 0);
        }

        public void WriteSerialNumberToRam(int serialNumber)
        {
            if (serialNumber < 0) return;
            if (serialNumber >= 0xFFFF) return;
            byte[] bytes = BitConverter.GetBytes(serialNumber);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            if (bytes.Length < 2) return;
            SetPointer(6);
            Query($"SC{bytes[0]}");
            Query($"SC{bytes[1]}");
        }

        public void WriteCustomStringToRam(string stringToWrite)
        {
            stringToWrite = stringToWrite.PadRight(16);
            string truncated = stringToWrite.Substring(0, 16);
            WriteStringToRam(truncated, 16); // this 16 is different from the former ones
        }

        public void WriteDetectorNameToRam(string name)
        {
            name = name.PadRight(4);
            string firstHalf = name.Substring(0, 2);
            string secondHalf = name.Substring(2, 2);
            WriteStringToRam(firstHalf, 48);
            WriteStringToRam(secondHalf, 54);
        }

        public void WriteCalibrationFactorToRam(double factor)
        {
            double exp = Math.Floor(Math.Log10(Math.Abs(factor)));
            double mantissa = factor / Math.Pow(10, exp);
            int exponent = (int)exp;
            WriteCalibrationFactorToRam(mantissa, exponent);
        }

        public void WriteUnitToRam(int unitCode)
        {
            if (unitCode < 0)
                return;
            if (unitCode > 0xFFF)
                return;
            byte flags = GetByte(53);
            // set bit0 to 1 and leave the rest unchanged
            flags |= 0b_0000_0001;
            // set the unit bits to 0 and leave the rest unchanged
            flags &= 0b_1000_0001;
            // set the unit bits and leave the rest unchanged
            byte code = (byte)(unitCode << 1);
            flags |= code;
            SetPointer(53);
            Query($"SC{flags}");
        }

        public void ClearDetectorRam()
        {
            byte[] bytes = new byte[blockSize];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = 0xFF;
            }
            WriteBytesToRam(bytes, 0);
        }

        public byte[] DumpDetectorRam()
        {
            return DumpDetectorRam(blockSize);
        }

        public string RamToString()
        {
            return RamToString(DumpDetectorRam());
        }

        // dangerous method!
        public void SaveRamToEeprom()
        {
            UnlockDevice();
            _ = Query($"SE{blockSize}");
            LockDevice();
        }

        private void WriteCalibrationFactorToRam(double mantissa, int exponent)
        {
            byte expByte = (byte)(-exponent - 3); // -10 -> 7
            bool factorIsNegative = false;

            if (mantissa < 0)
            {
                mantissa = Math.Abs(mantissa);
                factorIsNegative = true;
            }
            if (mantissa < 1.00001)
            {
                // TODO
                return;
            }
            if (mantissa > 9.9999)
            {
                // TODO
                return;
            }

            double normalizedFactor = 65535 / (mantissa * 0.999_985);
            int integerFactor = (int)Math.Round(normalizedFactor);

            byte[] bytes = BitConverter.GetBytes(integerFactor);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            byte flags = GetByte(53);
            if (factorIsNegative)
            {
                // set bit7 to 1 and leave the rest unchanged
                flags |= 0b_1000_0000;
            }
            else
            {
                // set bit7 to 0 and leave the rest unchanged
                flags &= 0b_0111_1111;
            }
            // set bit0 to 1 and leave the rest unchanged
            flags |= 0b_0000_0001;
            SetPointer(50);
            Query($"SC{bytes[0]}");
            Query($"SC{bytes[1]}");
            Query($"SC{expByte}");
            Query($"SC{flags}");
        }

        private string RamToString(byte[] ram)
        {
            if (ram == null) return string.Empty;
            if (ram.Length == 0) return string.Empty;
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < ram.Length; i++)
            {
                string asBinary = Convert.ToString(ram[i], 2).PadLeft(8, '0');
                char asChar = Convert.ToChar(ram[i]);
                if (ram[i] < 32 || ram[i] > 126)
                    asChar = ' ';
                sb.AppendLine($"{i,4} {i:X3} -> {ram[i],3} {ram[i]:X2} {asBinary} '{asChar}'");
            }
            return sb.ToString();
        }

        private byte[] DumpDetectorRam(int size)
        {
            if (size < 0) size = 0;
            if (size > 0x800) size = 0x800;
            byte[] bytes = new byte[size];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = GetByte(i);
            }
            return bytes;
        }

        private byte GetByte(int pointer)
        {
            if (pointer < 0)
                return 0;
            if (pointer > 0x800)
                return 0;
            return byte.Parse(Query($"GC{pointer}"));
        }

        private void WriteStringToRam(string stringToWrite, int pointer)
        {
            if (string.IsNullOrWhiteSpace(stringToWrite))
                return;
            byte[] bytes = Encoding.ASCII.GetBytes(stringToWrite);
            WriteBytesToRam(bytes, pointer);
        }

        private void WriteBytesToRam(byte[] bytes, int pointer)
        {
            if (bytes == null)
                return;
            if (bytes.Length == 0)
                return;
            SetPointer(pointer);
            for (int i = 0; i < bytes.Length; i++)
            {
                Query($"SC{bytes[i]}");
            }
        }

        private void UnlockDevice()
        {
            _ = Query($"RA{secretCode}");
        }

        private void LockDevice()
        {
            _ = Query("RA0000");
        }

        private void SetPointer(int pointer)
        {
            if (pointer < 0)
                return;
            if (pointer > 0x800)
                return;
            _ = Query($"SP{pointer}");
        }

        private string Query(string command)
        {
            string answer = "???";
            OpenPort();
            try
            {
                comPort.WriteLine(command);
                answer = comPort.ReadLine();
            }
            catch (Exception)
            {
                // just do nothing
            }
            ClosePort();
            Thread.Sleep(waitOnClose);
            return answer;
        }

        private void OpenPort()
        {
            try
            {
                if (!comPort.IsOpen)
                    comPort.Open();
            }
            catch (Exception)
            { }
        }

        private void ClosePort()
        {
            try
            {
                if (comPort.IsOpen)
                {
                    comPort.Close();
                    Thread.Sleep(waitOnClose);
                }
            }
            catch (Exception)
            { }
        }

    }
}
