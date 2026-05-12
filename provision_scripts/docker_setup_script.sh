#Script for setting up Droplets, by downloading Docker and sets enviroment variables, and changes the default folder.

set -e # Exit on error

# Disable potential background upgrades that might aqcuire the lock when we spin up a droplet
sudo systemctl stop apt-daily.timer apt-daily-upgrade.timer
sudo systemctl stop apt-daily.service apt-daily-upgrade.service
sudo systemctl kill --kill-who=all apt-daily.service || true

# temporarily add a known DNS server to your system - might fix connection issue we have?
# source: https://askubuntu.com/questions/91543/apt-get-update-fails-to-fetch-files-temporary-failure-resolving-error
echo "nameserver 8.8.8.8" | sudo tee /etc/resolv.conf > /dev/null

wait_for_apt() {
  sleep 3
  echo "Waiting for APT locks..."
  while sudo fuser /var/lib/dpkg/lock-frontend /var/lib/apt/lists/lock /var/lib/dpkg/lock >/dev/null 2>&1; do
    echo "APT is busy, sleeping 5s..."
    sleep 5
  done
  # A small extra buffer to let the process fully exit
  sleep 3
}

# Add Docker's official GPG key:
wait_for_apt
sudo apt-get update
wait_for_apt
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

wait_for_apt
sudo apt-get update

# Install docker and docker compose
wait_for_apt
sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

sudo systemctl status docker

echo -e "\nVerifying that docker works ...\n"
docker run --rm hello-world
docker rmi hello-world

# Restart daily update services
sudo systemctl start apt-daily.timer apt-daily-upgrade.timer