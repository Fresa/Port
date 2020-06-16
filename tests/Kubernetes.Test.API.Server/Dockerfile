#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["tests/Kubernetes.Test.API.Server/Kubernetes.Test.API.Server.csproj", "tests/Kubernetes.Test.API.Server/"]
RUN dotnet restore "tests/Kubernetes.Test.API.Server/Kubernetes.Test.API.Server.csproj"
COPY . .
WORKDIR "/src/tests/Kubernetes.Test.API.Server"
RUN dotnet build "Kubernetes.Test.API.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Kubernetes.Test.API.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Kubernetes.Test.API.Server.dll"]