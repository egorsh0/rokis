FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN apt-get update && apt-get install -y apt-utils libgdiplus libc6-dev libfontconfig1
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["idcc/idcc.csproj", "idcc/"]
RUN dotnet restore "idcc/idcc.csproj"
COPY . .
WORKDIR "/src/idcc"
RUN dotnet build "idcc.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "idcc.csproj" -c Release -o /app/publish

FROM base AS final
ENV ASPNETCORE_URLS=http://+:5000
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "idcc.dll"]
