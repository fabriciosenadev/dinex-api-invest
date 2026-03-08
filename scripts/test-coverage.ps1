dotnet test .\DinExApi.sln `
  /p:CollectCoverage=true `
  /p:CoverletOutput=.\TestResultsCoverage\ `
  /p:CoverletOutputFormat=cobertura `
  /p:Threshold=75 `
  /p:ThresholdType=line `
  /p:ThresholdStat=total

if ($LASTEXITCODE -ne 0) {
  exit $LASTEXITCODE
}
