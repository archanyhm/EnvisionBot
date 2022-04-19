FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["EnvisionBot/EnvisionBot.csproj", "EnvisionBot/"]
RUN dotnet restore "EnvisionBot/EnvisionBot.csproj"
COPY . .
WORKDIR "/src/EnvisionBot"
RUN dotnet build "EnvisionBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EnvisionBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EnvisionBot.dll"]
