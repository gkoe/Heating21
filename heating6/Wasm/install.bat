rmdir /Q /S \\192.168.0.66\pi\dotnet\Iot.Net\Wasm
mkdir \\192.168.0.66\pi\dotnet\Iot.Net\Wasm
xcopy /y /s D:\work\CSharp\iot.net\Iot.Net\Wasm\bin\Release\net5.0\browser-wasm\publish\wwwroot \\192.168.0.66\pi\dotnet\Iot.Net\Wasm
xcopy /y \\192.168.0.66\pi\dotnet\Iot.Net\appsettings_Wasm.json \\192.168.0.66\pi\dotnet\Iot.Net\Wasm\wwwroot\appsettings.json
