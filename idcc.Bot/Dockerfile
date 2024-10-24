FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN apt-get update && apt-get install -y apt-utils libgdiplus libc6-dev
WORKDIR /app


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["idcc.Bot/idcc.Bot.csproj", "idcc.Bot/"]
COPY ["idcc.Bot/NuGet.config", "./"]
RUN dotnet restore "idcc.Bot/idcc.Bot.csproj" --configfile ./NuGet.config
COPY . .
WORKDIR "/src/idcc.Bot"
RUN dotnet build "idcc.Bot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "idcc.Bot.csproj" -c Release -o /app/publish

FROM base AS final
ENV ASPNETCORE_URLS=http://+:6000
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "idcc.Bot.dll"]
