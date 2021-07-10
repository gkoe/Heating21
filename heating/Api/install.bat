rmdir /Q /S out
dotnet publish -c Release -o out
rmdir /Q /S \\192.168.0.66\pi\dotnet\Iot.Net\Api
mkdir \\192.168.0.66\pi\dotnet\Iot.Net\Api
xcopy /y /s out \\192.168.0.66\pi\dotnet\Iot.Net\Api
xcopy /y \\192.168.0.66\pi\dotnet\Iot.Net\appsettings_Api.json \\192.168.0.66\pi\dotnet\Iot.Net\Api\appsettings.json
