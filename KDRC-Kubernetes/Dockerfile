FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["KDRC-Kubernetes/KDRC-Kubernetes.csproj", "KDRC-Kubernetes/"]
RUN dotnet restore "KDRC-Kubernetes/KDRC-Kubernetes.csproj"
COPY . .
WORKDIR "/src/KDRC-Kubernetes"
RUN dotnet build "KDRC-Kubernetes.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KDRC-Kubernetes.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KDRC-Kubernetes.dll"]
