# Build and run

Requires the .NET 8 SDK. The WPF app targets `net8.0-windows` and builds on Windows only. Core, audio, SimConnect shell, and tests build on any OS.

```
dotnet test TurbulencePro.sln
dotnet build src/TurbulencePro.App/TurbulencePro.App.csproj
```

NuGet packages:

- Microsoft.NET.Test.Sdk 17.11.1
- xunit 2.9.2
- xunit.runner.visualstudio 2.8.2
- NAudio 2.2.1 (app playback dependency; the synthesizer itself has no package)

## MSFS 2024 SDK

Install the MSFS 2024 SDK. The managed assembly is:

`$(MSFS_SDK)/SimConnect SDK/lib/managed/Microsoft.FlightSimulator.SimConnect.dll`

Copy `SimConnect.dll` beside the app. Build the live client with:

```
dotnet build src/TurbulencePro.SimConnect/TurbulencePro.SimConnect.csproj -p:SimConnectManagedDll="C:\MSFS 2024 SDK\SimConnect SDK\lib\managed\Microsoft.FlightSimulator.SimConnect.dll"
```

Without that DLL the solution still builds. `SimConnectDataSource` stays disconnected. Test mode does not need the sim.

## Test mode

Run the WPF app. The Flight page starts in test mode and runs `SimulationLoop` at 30 Hz. UI labels refresh at 10 Hz. The Test tab drives altitude, AGL, IAS, vertical speed, wind, weight, on-ground, cloud, and precipitation. Class buttons force the procedural class and label it as simulated.

## Connect to MSFS 2024

1. Start a flight.
2. Build with the managed DLL path above.
3. The live client opens SimConnect against the window handle, defines the catalog in `SimVarCatalog`, and requests the user object every third sim frame.
4. The callback only copies `FlightSample`. The estimator runs on the simulation timer.

Camera control is not implemented. `ICameraEffect` is the null implementation. Wake is the null `IWakeEstimator`.

## Recording

CSV header is `FlightRecorder.Header`. Replay loads samples and calls the same `SimulationLoop.Step`.

Profiles in `profiles/` are perceptual response only.
