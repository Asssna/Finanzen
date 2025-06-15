FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Finanzen/Finanzen.csproj", "Finanzen/"]
RUN dotnet restore "Finanzen/Finanzen.csproj"

COPY . .
WORKDIR "/src/Finanzen"
RUN dotnet publish "Finanzen.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Finanzen.dll"]