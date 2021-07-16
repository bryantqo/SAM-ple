FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build-image

ARG FUNCTION_DIR="/build"
ARG SAM_BUILD_MODE="run"
ENV PATH="/root/.dotnet/tools:${PATH}"

RUN apt-get update && apt-get -y install zip
RUN [ "dotnet", "tool", "install", "-g", "Amazon.Lambda.Tools" ]

RUN mkdir -p $FUNCTION_DIR

WORKDIR /build
COPY . $FUNCTION_DIR/

# Build and Copy artifacts depending on build mode.
RUN mkdir -p build_artifacts

RUN find /build

RUN dotnet publish -o build_artifacts WebAPI/WebAPI.csproj

RUN find /build/build_artifacts/

FROM public.ecr.aws/lambda/dotnet:5.0

COPY --from=build-image /build/build_artifacts/ /var/task/

RUN find /var/task/
# Command can be overwritten by providing a different command in the template directly.
ENV LAMBDA_DOTNET_MAIN_ASSEMBLY=WebAPI.dll
ENV DOTNET_ENVIRONMENT=Development
ENV ASPNETCORE_ENVIRONMENT=Development
CMD [ "WebAPI::WebAPI.LambdaEntryPoint::FunctionHandlerAsync" ]
