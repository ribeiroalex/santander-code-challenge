FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Santander.CodeChallenge.Api/Santander.CodeChallenge.Api.csproj", "Santander.CodeChallenge.Api/"]
COPY ["Santander.CodeChallenge.Application/Santander.CodeChallenge.Application.csproj", "Santander.CodeChallenge.Application/"]
COPY ["Santander.CodeChallenge.Infrastructure/Santander.CodeChallenge.Infrastructure.csproj", "Santander.CodeChallenge.Infrastructure/"]
RUN dotnet restore "Santander.CodeChallenge.Api/Santander.CodeChallenge.Api.csproj"
COPY . .
WORKDIR "/src/Santander.CodeChallenge.Api"
RUN dotnet build "Santander.CodeChallenge.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Santander.CodeChallenge.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Santander.CodeChallenge.Api.dll"]
