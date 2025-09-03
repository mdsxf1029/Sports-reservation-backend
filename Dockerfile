FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Sports-reservation-backend.csproj", "./"]
RUN dotnet restore "./Sports-reservation-backend.csproj"
COPY . .
RUN dotnet build "./Sports-reservation-backend.csproj" -c $BUILD_CONFIGURATION -o /app/build
RUN echo "Executing the BUILD stage!"

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
RUN dotnet publish "./Sports-reservation-backend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
RUN echo "Executing the PUBLISH stage!"

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN chmod 777 wwwroot/uploads
ENTRYPOINT ["dotnet", "Sports-reservation-backend.dll"]
