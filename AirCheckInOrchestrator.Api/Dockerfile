FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AirCheckInOrchestrator.Api/AirCheckInOrchestrator.Api.csproj", "AirCheckInOrchestrator.Api/"]
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["Baggage/Baggage.csproj", "Baggage/"]
COPY ["Registration/Registration.csproj", "Registration/"]
RUN dotnet restore "AirCheckInOrchestrator.Api/AirCheckInOrchestrator.Api.csproj"
COPY . .
WORKDIR "/src/AirCheckInOrchestrator.Api"
RUN dotnet build "./AirCheckInOrchestrator.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AirCheckInOrchestrator.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AirCheckInOrchestrator.Api.dll"]
