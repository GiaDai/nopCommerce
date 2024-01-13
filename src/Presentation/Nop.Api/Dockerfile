#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 5010
ENV ASPNETCORE_URLS=http://+:5010
ENV ASPNETCORE_ENVIRONMENT=Production

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Presentation/Nop.Api/Nop.Api.csproj", "Presentation/Nop.Api/"]
RUN dotnet restore "Presentation/Nop.Api/Nop.Api.csproj"
COPY . .
WORKDIR "/src/Presentation/Nop.Api"
RUN dotnet build "Nop.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Nop.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Nop.Api.dll"]