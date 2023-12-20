FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

COPY ["Adramelech/Adramelech.csproj", "Adramelech/"]
RUN dotnet restore "Adramelech/Adramelech.csproj"

COPY . .
WORKDIR "/src/Adramelech"
RUN dotnet build "Adramelech.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Adramelech.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Adramelech.dll"]
