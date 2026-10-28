FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MeteoDemo.Console/MeteoDemo.Console.csproj", "MeteoDemo.Console/"]
COPY ["MeteoDemo.Core/MeteoDemo.Core.csproj", "MeteoDemo.Core/"]
COPY ["MeteoDemo.Data/MeteoDemo.Data.csproj", "MeteoDemo.Data/"]
RUN dotnet restore "MeteoDemo.Console/MeteoDemo.Console.csproj"
COPY . .
WORKDIR "/src/MeteoDemo.Console"
RUN dotnet build "./MeteoDemo.Console.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MeteoDemo.Console.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MeteoDemo.Console.dll"]
