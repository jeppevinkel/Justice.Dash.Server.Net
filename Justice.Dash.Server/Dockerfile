FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Justice.Dash.Server/Justice.Dash.Server.csproj", "Justice.Dash.Server/"]
RUN dotnet restore "Justice.Dash.Server/Justice.Dash.Server.csproj"
COPY . .
WORKDIR "/src/Justice.Dash.Server"
RUN dotnet build "Justice.Dash.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Justice.Dash.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Justice.Dash.Server.dll"]
