FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app

# Zona horaria America/Lima
RUN apt-get update \
    && apt-get install -y --no-install-recommends tzdata \
    && ln -snf /usr/share/zoneinfo/America/Lima /etc/localtime \
    && echo "America/Lima" > /etc/timezone \
    && rm -rf /var/lib/apt/lists/*
ENV TZ=America/Lima

# Escuchar en todas las interfaces (0.0.0.0), no en localhost
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copiar los archivos .csproj - rutas relativas desde la raíz del contexto
COPY ["VelsatBackendAPI/VelsatBackendAPI.csproj", "VelsatBackendAPI/"]
COPY ["VelsatBackendAPI.Data/VelsatBackendAPI.Data.csproj", "VelsatBackendAPI.Data/"]
COPY ["VelsatBackendAPI.Model/VelsatBackendAPI.Model.csproj", "VelsatBackendAPI.Model/"]

# Restaurar dependencias
RUN dotnet restore "VelsatBackendAPI/VelsatBackendAPI.csproj"

# Copiar todo el código desde la raíz
COPY . .

# Build del proyecto principal
WORKDIR "/src/VelsatBackendAPI"
RUN dotnet build "VelsatBackendAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VelsatBackendAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VelsatBackendAPI.dll"]