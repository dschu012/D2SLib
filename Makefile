#todo... better method
nuget:
	dotnet nuget push D2SLib.1.0.2.nupkg --api-key $NUGET_APIKEY --source https://api.nuget.org/v3/index.json
