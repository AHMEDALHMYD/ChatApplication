namespace ChatApplication.Server
{
    public class Dockerfile
    {
        FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ChatApplication.Server/ChatApplication.Server.csproj", "ChatApplication.Server/"]
RUN dotnet restore "ChatApplication.Server/ChatApplication.Server.csproj"
COPY. .
WORKDIR "/src/ChatApplication.Server"
RUN dotnet publish "ChatApplication.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from= build / app / publish.
ENTRYPOINT["dotnet", "ChatApplication.Server.dll"]

    }
}
