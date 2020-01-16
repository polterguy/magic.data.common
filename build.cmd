
set version=%1
set key=%2

cd %~dp0
dotnet build magic.data.common/magic.data.common.csproj --configuration Release --source https://api.nuget.org/v3/index.json
dotnet nuget push magic.data.common/bin/Release/magic.data.common.%version%.nupkg -k %key% -s https://api.nuget.org/v3/index.json
