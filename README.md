# GasolineraSimulacion

Panel central para simulación de bombas de gasolinera.  
Proyecto final – Programación III / Electrónica Digital  
Universidad Mesoamericana

---

## Requisitos

- .NET 6.0 SDK (Windows)
- Visual Studio 2022 o VS Code con extensión C#
- Arduino con firmware cargado (ver sección Arduino)

---

## Estructura del proyecto

```
GasolineraSimulacion/
├── Models/
│   ├── TipoPago.cs          ← Enum: Prepago / TanqueLleno
│   ├── Abastecimiento.cs    ← Modelo de cada abastecimiento
│   └── Bomba.cs             ← Estado de cada bomba (1-4)
├── Services/
│   ├── ArduinoService.cs    ← Comunicación serial JSON con Arduino
│   ├── RegistroService.cs   ← Lectura/escritura en abastecimientos.json
│   └── EstadisticasService.cs ← Cierres de caja e informes
├── UI/
│   └── PanelCentral.cs      ← Formulario principal (Windows Forms)
├── Data/
│   ├── abastecimientos.json ← Generado automáticamente
│   └── precio.json          ← Precio por litro del día
└── Program.cs
```

---

## Compilar y ejecutar

```bash
cd GasolineraSimulacion
dotnet build
dotnet run
```

---

## Protocolo JSON con Arduino

### PC → Arduino (orden de inicio)
```json
{ "bomba": 1, "tipo": "Prepago", "litros": 0.5 }
{ "bomba": 2, "tipo": "TanqueLleno", "litros": 0 }
{ "bomba": 1, "tipo": "Detener", "litros": 0 }
```

### Arduino → PC (respuesta al terminar)
```json
{ "bomba": 1, "litrosServidos": 0.5, "completado": true }
{ "bomba": 2, "litrosServidos": 12.3, "completado": true }
```

---

## Firmware Arduino (referencia)

```cpp
#include <ArduinoJson.h>

void setup() {
  Serial.begin(9600);
}

void loop() {
  if (Serial.available()) {
    String linea = Serial.readStringUntil('\n');
    StaticJsonDocument<200> doc;
    deserializeJson(doc, linea);

    int bomba       = doc["bomba"];
    String tipo     = doc["tipo"].as<String>();
    float litros    = doc["litros"];

    // Aquí controlar electroválvula / PWM según bomba y tipo
    // ...

    // Al terminar, responder:
    StaticJsonDocument<100> resp;
    resp["bomba"]         = bomba;
    resp["litrosServidos"] = litros;  // reemplazar con lectura real del sensor
    resp["completado"]    = true;
    serializeJson(resp, Serial);
    Serial.println();
  }
}
```

---

## Notas

- Si el Arduino no está conectado, el sistema funciona igualmente; las bombas se pueden
  detener manualmente ingresando los litros servidos.
- El precio por litro se puede cambiar en cualquier momento desde el Panel Principal.
- Los archivos JSON se guardan en la carpeta `Data/` junto al ejecutable.
