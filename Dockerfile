# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:11.0 AS build
WORKDIR /app

# Copy solution and project files first to cache NuGet dependencies
COPY FinanceTrackerApi.slnx ./
COPY src/FinanceTrackerApi/FinanceTrackerApi.csproj src/FinanceTrackerApi/
COPY tests/FinanceTrackerApi.Tests/FinanceTrackerApi.Tests.csproj tests/FinanceTrackerApi.Tests/

# Restore NuGet dependencies
RUN dotnet restore FinanceTrackerApi.slnx

# Copy remaining source code and publish in Release mode
COPY . ./
RUN dotnet publish src/FinanceTrackerApi/FinanceTrackerApi.csproj -c Release -o /app/publish

# Runtime Stage (lightweight runtime container)
FROM mcr.microsoft.com/dotnet/aspnet:11.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "FinanceTrackerApi.dll"]