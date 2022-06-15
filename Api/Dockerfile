FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

ENV DEBIAN_FRONTEND noninteractive
ENV DENO_INSTALL="/root/.deno"
ENV PATH="${DENO_INSTALL}/bin:${PATH}"

FROM base as runtime
#RUN apt-get update -y && apt-get install -y nodejs npm && /bin/npm i -g n && n 16
RUN apt-get update -y && apt-get install -y firejail curl unzip && \
    curl -fsSL https://deno.land/x/install/install.sh | sh

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Api", "GraderBotReimagined"]
RUN dotnet restore "GraderBotReimagined/Api.csproj"
COPY . .
WORKDIR "/src/GraderBotReimagined"
RUN dotnet build "Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Api.csproj" -c Release -o /app/publish

FROM runtime
#COPY --from=denoland/deno:debian "/usr/bin/deno" "/usr/bin/deno"
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]
