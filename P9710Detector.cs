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

        public void WriteSecretStringToRam(string stringToWrite)
        {
            stringToWrite=stringToWrite.PadRight(16);
            string truncated = stringToWrite.Substring(0, 16);
            WriteStringToRam(truncated, 16); // this 16 is different from the former ones
        }

        public void WriteMagicStringToRam()
        {
            WriteStringToRam("PT9610", 0);
        }

        public void WriteStringToRam(string stringToWrite, int pointer)
        {
            if (string.IsNullOrWhiteSpace(stringToWrite))
                return;
            byte[] bytes = Encoding.ASCII.GetBytes(stringToWrite);
            WriteBytesToRam(bytes, pointer);
        }

        public void WriteBytesToRam(byte[] bytes, int pointer)
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

        public void WriteDetectorNameToRam(string name)
        {
            name = name.PadRight(4);
            string firstHalf = name.Substring(0, 2);
            string secondHalf = name.Substring(2, 2);
            WriteStringToRam(firstHalf, 48);
            WriteStringToRam(secondHalf, 54);
        }

        public void WriteSerialNumberToRam(int serialNumber)
        {
            if (serialNumber < 0) return;
            if (serialNumber >= 0xFFFF) return;
            byte[] bytes = BitConverter.GetBytes(serialNumber);
            if (!BitConverter.IsLittleEndian) 
                Array.Reverse(bytes);
            SetPointer(6);
            Query($"SC{bytes[0]}");
            Query($"SC{bytes[1]}");
        }

        public void WriteCalibrationFactorToRam(double factorMantissa)
        {
            if (factorMantissa < 0)
            {
                factorMantissa = Math.Abs(factorMantissa);
                // TODO set sign flag
            }
            if (factorMantissa < 1.00001)
            {
                // TODO
                return;
            }
            double normFactor = 65535 / (factorMantissa * 0.999985);
            int integerFactor = (int)Math.Round(normFactor);

            byte[] bytes = BitConverter.GetBytes(integerFactor);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            // TODO something
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

        public byte[] GetDetectorRam()
        {
            return GetDetectorRam(blockSize);
        }

        public byte[] GetDetectorRam(int size)
        {
            if (size < 0) size = 0;
            if (size > 0x800) size = 0x800;
            byte[] bytes = new byte[size];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = byte.Parse(Query($"GC{i}"));
            }
            return bytes;
        }




        // dangerous method!
        public void LoadRamToEeprom()
        {
            UnlockDevice();
            _ = Query($"SE{blockSize}");
            LockDevice();
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
