xcopy /y \\192.168.0.56\pi\dotnet\heating\Api\data\database.db database.db
xcopy /y \\192.168.0.56\pi\dotnet\heating\Api\appsettings.json appsettings_api.json
xcopy /y \\192.168.0.56\pi\dotnet\heating\Wasm\wwwroot\appsettings.json appsettings_wasm.json

xcopy /y /e /k /h /i *.* \\192.168.0.56\pi\dotnet\heating\
xcopy /y .\database.db \\192.168.0.56\pi\dotnet\heating\Api\data\database.db
xcopy /y .\appsettings_api.json \\192.168.0.56\pi\dotnet\heating\Api\appsettings.json
xcopy /y .\appsettings_wasm.json \\192.168.0.56\pi\dotnet\heating\Wasm\wwwroot\appsettings.json

