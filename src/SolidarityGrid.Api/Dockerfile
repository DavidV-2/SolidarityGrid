FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["src/SolidarityGrid.Api/SolidarityGrid.Api.csproj", "SolidarityGrid.Api/"]
COPY ["src/SolidarityGrid.Application/SolidarityGrid.Application.csproj", "SolidarityGrid.Application/"]
COPY ["src/SolidarityGrid.Domain/SolidarityGrid.Domain.csproj", "SolidarityGrid.Domain/"]
COPY ["src/SolidarityGrid.Infrastructure/SolidarityGrid.Infrastructure.csproj", "SolidarityGrid.Infrastructure/"]

RUN dotnet restore "SolidarityGrid.Api/SolidarityGrid.Api.csproj"

COPY src/ .

WORKDIR /src/SolidarityGrid.Api

RUN dotnet publish -c Release -o /app/publish

FROM build AS publish
RUN dotnet publish "SolidarityGrid.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SolidarityGrid.Api.dll"]