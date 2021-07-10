pwd
docker build -f Dockerfile_Wasm -t wasm .  --no-cache
cd /home/pi/docker/compose/wasm
pwd
docker-compose down
docker-compose up -d
