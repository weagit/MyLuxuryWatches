using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace MyApp.Service;
public partial class DeviceOrientationService
{
    SerialPort? mySerialPort;
    string portDetected = null;

    public partial void OpenPort()
    {
        if (mySerialPort != null)
        {
            try
            {
                if (mySerialPort.IsOpen) mySerialPort.Close();
                mySerialPort.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while closing the port: {ex.Message}");
            }
            finally
            {
                mySerialPort = null;
            }
        }
        else
        {
            bool deviceFound = false;
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%(COM%'");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    string nom = queryObj["Name"]?.ToString() ?? "";
                    string id = queryObj["PNPDeviceID"]?.ToString() ?? "";

                    // Vérification avec l'ID du périphérique
                    if (id.Contains("PID_A4A7"))
                    {
                        int debut = nom.LastIndexOf("COM");
                        int fin = nom.LastIndexOf(")");
                        if (debut != -1 && fin != -1)
                        {
                            portDetected = nom.Substring(debut, fin - debut);
                            deviceFound = true;
                            break; // on prend le premier port trouvé correspondant au scanner
                        }
                    }
                }

                if (!deviceFound)
                {
                    // Aucun dispositif compatible trouvé
                    Shell.Current.DisplayAlert("Device Not Detected",
                        "No compatible scanner detected. Please check your device connection.", "OK");
                    return;
                }

                if (portDetected != null)
                {
                    mySerialPort = new SerialPort
                    {
                        BaudRate = 9600,
                        PortName = portDetected,
                        Parity = Parity.None,
                        DataBits = 8,
                        StopBits = StopBits.One,
                        ReadTimeout = 10000,
                        WriteTimeout = 10000
                    };

                    mySerialPort.DataReceived += new SerialDataReceivedEventHandler(DataHandler);

                    try
                    {
                        mySerialPort.Open();
                        Console.WriteLine($"Port {portDetected} successfully opened");
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Shell.Current.DisplayAlert("Access Error",
                            $"Port {portDetected} is in use by another application or not accessible.", "OK");
                    }
                    catch (IOException)
                    {
                        Shell.Current.DisplayAlert("Port Error",
                            $"Port {portDetected} may already be open or no longer exists.", "OK");
                    }
                    catch (Exception ex)
                    {
                        Shell.Current.DisplayAlert("Connection Error",
                            $"Unable to open port {portDetected}: {ex.Message}", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while searching ports: {ex.Message}");
                Shell.Current.DisplayAlert("System Error",
                    "An error occurred while searching for devices.", "OK");
            }
        }
    }

    public partial void ClosePort()
    {
        if (mySerialPort != null && mySerialPort.IsOpen)
        {
            try
            {
                mySerialPort.Close();
                mySerialPort.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while closing the port: {ex.Message}");
            }
            finally
            {
                mySerialPort = null;
            }
        }
    }

    private void DataHandler(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            SerialPort sp = (SerialPort)sender;
            string data = sp.ReadExisting().Trim();

            if (!string.IsNullOrWhiteSpace(data))
            {
                SerialBuffer.Enqueue(data);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in DataHandler: {ex}");
        }
    }
}
