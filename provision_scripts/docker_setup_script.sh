#Script for setting up Droplets, by downloading Docker and sets enviroment variables, and changes the default folder.

set -e # Exit on error

while fuser /var/lib/apt/lists/lock >/dev/null 2>&1 ; do
echo "Waiting for other software managers to finish..."
sleep 5
done

# Add Docker's official GPG key:
sudo apt-get update
sudo apt install -y ca-certificates curl
sudo install -m 0755 -d /etc/apt/keyrings
sudo curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc
sudo chmod a+r /etc/apt/keyrings/docker.asc

# Add the repository to Apt sources:
sudo tee /etc/apt/sources.list.d/docker.sources <<EOF
Types: deb
URIs: https://download.docker.com/linux/ubuntu
Suites: $(. /etc/os-release && echo "${UBUNTU_CODENAME:-$VERSION_CODENAME}")
Components: stable
Architectures: $(dpkg --print-architecture)
Signed-By: /etc/apt/keyrings/docker.asc
EOF

while fuser /var/lib/apt/lists/lock >/dev/null 2>&1 ; do
echo "Waiting for other software managers to finish..."
sleep 5
done

sudo apt-get update

# Install docker and docker compose
sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

sudo systemctl status docker

echo -e "\nVerifying that docker works ...\n"
docker run --rm hello-world
docker rmi hello-world