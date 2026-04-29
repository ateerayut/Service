# Multi-stage build for .NET 10 Minimal API
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first for better caching
COPY ["Service.slnx", "."]
COPY ["src/Service.Api/Service.Api.csproj", "src/Service.Api/"]
COPY ["src/Service.Application/Service.Application.csproj", "src/Service.Application/"]
COPY ["src/Service.Domain/Service.Domain.csproj", "src/Service.Domain/"]
COPY ["src/Service.Infrastructure/Service.Infrastructure.csproj", "src/Service.Infrastructure/"]

# Restore dependencies
RUN dotnet restore

# Copy everything else and build
COPY . .
WORKDIR "/src/src/Service.Api"
RUN dotnet build "Service.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Service.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose port 8080
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Service.Api.dll"]
