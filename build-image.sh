version=v1

docker build . -t wilsonnetodev/demo-echo-api:$version
docker tag wilsonnetodev/demo-echo-api:$version wilsonnetodev/demo-echo-api:latest

docker push wilsonnetodev/demo-echo-api:$version
docker push wilsonnetodev/demo-echo-api:latest