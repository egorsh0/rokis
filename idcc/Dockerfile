# ───── build ────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY idcc/cert.pfx /https/cert.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password="test"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path="https/cert.pfx"

ENV ASPNETCORE_URLS=http://+:5000;https://+:443
EXPOSE 5000
EXPOSE 443

COPY idcc/idcc.csproj idcc/
RUN dotnet restore idcc/idcc.csproj
COPY . .
RUN dotnet publish idcc/idcc.csproj -c Release -o /app/publish

# ───── runtime (только host, без SDK) ───────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        libgdiplus libc6-dev libfontconfig1 postgresql-client && \
    rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "idcc.dll"]
