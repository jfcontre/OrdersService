# Etapa de compilación: se utiliza la imagen SDK para compilar la aplicación
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia el archivo de proyecto y restaura las dependencias
COPY OrdersService.Api.csproj .
RUN dotnet restore

# Copia el resto del código y compila en modo Release
COPY . .
RUN dotnet publish OrdersService.sln -c Release -o /app/publish

# Etapa final: se utiliza la imagen de runtime para ejecutar la aplicación
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OrdersService.Api.dll"]