FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /source

ARG TARGETARCH
ARG BUILDPLATFORM

# copy csproj and restore as distinct layers
COPY *.sln .
COPY MemoDown/*.csproj ./MemoDown/

RUN dotnet restore -a $TARGETARCH

# copy everything else and build app
COPY MemoDown/. ./MemoDown/
WORKDIR /source/MemoDown

RUN dotnet publish -a $TARGETARCH -o /memodown --no-restore

# final stage/image
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:8.0-alpine

WORKDIR /memodown
COPY --from=build /memodown ./

ENV ASPNETCORE_HTTP_PORTS 8080
ENV MemoDown__MemoDir /memo

VOLUME /memo

EXPOSE 8080

ENTRYPOINT ["dotnet", "MemoDown.dll"]