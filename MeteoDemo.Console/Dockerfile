FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MeteoDemo/MeteoDemo.csproj", "MeteoDemo/"]
COPY ["MeteoDemo.Logic/MeteoDemo.Logic.csproj", "MeteoDemo.Logic/"]
COPY ["MeteoDemo.Database/MeteoDemo.Database.csproj", "MeteoDemo.Database/"]
RUN dotnet restore "MeteoDemo/MeteoDemo.csproj"
COPY . .
WORKDIR "/src/MeteoDemo"
RUN dotnet build "./MeteoDemo.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MeteoDemo.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MeteoDemo.dll"]
