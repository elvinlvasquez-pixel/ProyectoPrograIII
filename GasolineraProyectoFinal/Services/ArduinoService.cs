using System;
using System.IO.Ports;
using System.Text.Json;
using GasolineraSimulacion.Models;

namespace GasolineraSimulacion.Services
{
    /// <summary>
    /// Maneja toda la comunicación serial con el Arduino.
    /// Toda la comunicación se hace a través de cadenas JSON.
    /// </summary>
    public class ArduinoService
    {
        private SerialPort _puerto;

        // Evento que dispara cuando Arduino envía datos de vuelta
        public event Action<RespuestaArduino> MensajeRecibido;

        // Evento para logging/debug en la UI
        public event Action<string> LogRecibido;

        public bool EstaConectado => _puerto != null && _puerto.IsOpen;

       
        public bool Conectar(string portName, int baudRate = 9600)
        {
            try
            {
                _puerto = new SerialPort(portName, baudRate)
                {
                    ReadTimeout = 5000,
                    WriteTimeout = 2000,
                    NewLine = "\n"
                };
                _puerto.DataReceived += OnDataReceived;
                _puerto.Open();
                LogRecibido?.Invoke($"✓ Conectado a {portName} @ {baudRate} baud");
                return true;
            }
            catch (Exception ex)
            {
                LogRecibido?.Invoke($"✗ Error al conectar: {ex.Message}");
                return false;
            }
        }

        
        public bool EnviarOrden(int numeroBomba, TipoPago tipo, double litros)
        {
            if (!EstaConectado)
            {
                LogRecibido?.Invoke("✗ No hay conexión con Arduino.");
                return false;
            }

            try
            {
                var orden = new OrdenArduino
                {
                    bomba = numeroBomba,
                    tipo = tipo.ToString(),
                    litros = litros
                };
                string json = JsonSerializer.Serialize(orden);
                _puerto.WriteLine(json);
                LogRecibido?.Invoke($"→ Enviado a Arduino: {json}");
                return true;
            }
            catch (Exception ex)
            {
                LogRecibido?.Invoke($"✗ Error al enviar orden: {ex.Message}");
                return false;
            }
        }

     
        public void EnviarDetener(int numeroBomba)
        {
            if (!EstaConectado) return;
            var orden = new { bomba = numeroBomba, tipo = "Detener", litros = 0.0 };
            string json = JsonSerializer.Serialize(orden);
            _puerto.WriteLine(json);
            LogRecibido?.Invoke($"→ Deteniendo bomba {numeroBomba}: {json}");
        }

       
        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string datos = _puerto.ReadLine().Trim();
                LogRecibido?.Invoke($"← Recibido de Arduino: {datos}");

                var respuesta = JsonSerializer.Deserialize<RespuestaArduino>(datos);
                if (respuesta != null)
                {
                    MensajeRecibido?.Invoke(respuesta);
                }
            }
            catch (Exception ex)
            {
                LogRecibido?.Invoke($"✗ Error al procesar respuesta: {ex.Message}");
            }
        }

        public void Desconectar()
        {
            if (_puerto != null && _puerto.IsOpen)
            {
                _puerto.Close();
                LogRecibido?.Invoke("Desconectado del Arduino.");
            }
        }

        public static string[] ObtenerPuertosDisponibles()
        {
            return SerialPort.GetPortNames();
        }
    }

    // Clases auxiliares para serialización JSON

    public class OrdenArduino
    {
        public int bomba { get; set; }
        public string tipo { get; set; }
        public double litros { get; set; }
    }

    public class RespuestaArduino
    {
        public int bomba { get; set; }
        public double litrosServidos { get; set; }
        public bool completado { get; set; }
    }
}