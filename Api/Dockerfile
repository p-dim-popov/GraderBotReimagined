FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["GraderBotReimagined/GraderBotReimagined.csproj", "GraderBotReimagined/"]
RUN dotnet restore "GraderBotReimagined/GraderBotReimagined.csproj"
COPY . .
WORKDIR "/src/GraderBotReimagined"
RUN dotnet build "GraderBotReimagined.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GraderBotReimagined.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GraderBotReimagined.dll"]
