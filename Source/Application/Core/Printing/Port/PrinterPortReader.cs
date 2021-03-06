﻿using System;
using SystemInterface.IO;
using SystemInterface.Microsoft.Win32;

namespace pdfforge.PDFCreator.Core.Printing.Port
{
    public interface IPrinterPortReader
    {
        PrinterPort ReadPrinterPort(string portName);
    }

    public class PrinterPortReader : IPrinterPortReader
    {
        private const string DefaultTempFolderName = "PDFCreator";
        private const string RegistryBaseKey = @"SYSTEM\CurrentControlSet\Control\Print\Monitors\pdfcmon\Ports\";
        private readonly IPathSafe _pathSafe;

        private readonly IRegistry _registry;

        public PrinterPortReader(IRegistry registry, IPathSafe pathSafe)
        {
            _registry = registry;
            _pathSafe = pathSafe;
        }

        public PrinterPort ReadPrinterPort(string portName)
        {
            var subKey = _pathSafe.Combine(RegistryBaseKey, portName);
            var key = _registry.LocalMachine.OpenSubKey(subKey);

            if (key == null)
                return null;

            var printerPort = new PrinterPort();

            try
            {
                printerPort.Name = portName;

                printerPort.Description = key.GetValue("Description").ToString();
                printerPort.Program = key.GetValue("Program").ToString();

                var portTypeString = key.GetValue("Type", "PS").ToString();

                if (portTypeString.Equals("XPS", StringComparison.OrdinalIgnoreCase))
                {
                    printerPort.Type = PortType.Xps;
                }

                printerPort.TempFolderName = key.GetValue("TempFolderName", DefaultTempFolderName).ToString();

                var serverValue = (int?)key.GetValue("Server");

                if (serverValue == 1)
                    printerPort.IsServerPort = true;

                if (string.IsNullOrWhiteSpace(printerPort.TempFolderName))
                    printerPort.TempFolderName = DefaultTempFolderName;

                var jobCounter = (int)key.GetValue("JobCounter", 0);
                printerPort.JobCounter = jobCounter;
            }
            catch (NullReferenceException)
            {
                printerPort = null;
            }

            key.Close();

            return printerPort;
        }
    }
}
