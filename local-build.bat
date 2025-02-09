dotnet clean
dotnet build ./MapsetChecksCatch.sln
move .\bin\Debug\net8.0\MapsetChecksCatch.dll "%AppData%\Mapset Verifier Externals\checks"