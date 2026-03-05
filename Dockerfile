FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src
COPY ["Backend.csproj", "."]
RUN dotnet restore "Backend.csproj"
COPY . .
RUN dotnet publish "Backend.csproj" \
    -c Release \
    -o /app/publish \
    -r linux-musl-x64 \
    --self-contained true
    #/p:PublishTrimmed=true

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-alpine AS final
WORKDIR /app
EXPOSE 80
COPY --from=build /app/publish .
ENTRYPOINT ["./Backend"]