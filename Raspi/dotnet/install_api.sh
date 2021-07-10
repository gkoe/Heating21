pwd
docker build  -f Dockerfile_Api -t api .  --no-cache
cd /home/pi/docker/compose/api
pwd
docker-compose down
docker-compose up -d
