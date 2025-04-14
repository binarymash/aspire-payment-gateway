#!/bin/bash
rm -rf ./Artifacts/TestResults/*
dotnet clean ./src/AspirePaymentGateway.Api.Tests/
dotnet test ./src/AspirePaymentGateway.Api.Tests/ --collect:"XPlat Code Coverage" --settings:"./src/coverlet.runsettings" --results-directory:"./Artifacts/TestResults" -p:Threshold=95
echo dotnet test exit code: $?
dotnet reportgenerator -reports:"./Artifacts/TestResults/*/coverage.cobertura.xml" -targetdir:"./Artifacts/TestResults"

