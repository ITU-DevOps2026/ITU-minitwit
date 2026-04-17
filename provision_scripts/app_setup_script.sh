#Script for setting up Droplets, by downloading Docker and sets enviroment variables, and changes the default folder.

set -e # Exit on error

while fuser /var/lib/apt/lists/lock >/dev/null 2>&1 ; do
echo "Waiting for other software managers to finish..."
sleep 5
done
# The following addresses an issue in DO's Ubuntu images, which still contain a lock file
sudo apt-get update

# Install docker and docker compose
sudo apt install -qq -y docker.io
sudo apt install -qq -y docker-compose-v2

sudo systemctl status docker

echo -e "\nVerifying that docker works ...\n"
docker run --rm hello-world
docker rmi hello-world

echo ". $HOME/.bashrc" >> $HOME/.bash_profile
echo -e "\nConfiguring credentials as environment variables...\n"
source $HOME/.bash_profile

echo -e "\nSelecting Minitwit Folder as default folder when you ssh into the server...\n"
echo "cd /minitwit" >> ~/.bash_profile

sed -i 's/\r$//' /minitwit/deploy.sh
chmod +x /minitwit/deploy.sh