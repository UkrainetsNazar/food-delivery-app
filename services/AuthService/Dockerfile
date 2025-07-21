FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["AuthService/Auth.API/Auth.API.csproj", "AuthService/Auth.API/"]
COPY ["Shared/Contracts/Contracts.csproj", "Shared/Contracts/"]

RUN dotnet restore "AuthService/Auth.API/Auth.API.csproj"

COPY . .

WORKDIR "/src/AuthService/Auth.API"
RUN dotnet publish "Auth.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Auth.API.dll"]