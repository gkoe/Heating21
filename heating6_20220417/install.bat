xcopy /y \\10.0.0.1\pi\dotnet\Heating.Net\Api\data\database.db database.db
xcopy /y \\10.0.0.1\pi\dotnet\Heating.Net\Api\appsettings.json appsettings_api.json
xcopy /y \\10.0.0.1\pi\dotnet\Heating.Net\Wasm\wwwroot\appsettings.json appsettings_wasm.json

xcopy /y /e /k /h /i *.* \\10.0.0.1\pi\dotnet\Heating.Net\
xcopy /y .\database.db \\10.0.0.1\pi\dotnet\Heating.Net\Api\data\database.db
xcopy /y .\appsettings_api.json \\10.0.0.1\pi\dotnet\Heating.Net\Api\appsettings.json
xcopy /y .\appsettings_wasm.json \\10.0.0.1\pi\dotnet\Heating.Net\Wasm\wwwroot\appsettings.json

