FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["UserService/User.API/User.API.csproj", "UserService/User.API/"]
COPY ["Shared/Contracts/Contracts.csproj", "Shared/Contracts/"]

RUN dotnet restore "UserService/User.API/User.API.csproj"

COPY . .

WORKDIR "/src/UserService/User.API"
RUN dotnet publish "User.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "User.API.dll"]