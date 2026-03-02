source ~/.bash_profile

cd /minitwit

docker compose -f compose.yaml pull
docker compose -f compose.yaml up -d