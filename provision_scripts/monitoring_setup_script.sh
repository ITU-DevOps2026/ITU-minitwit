set -e # Exit on error

while fuser /var/lib/apt/lists/lock >/dev/null 2>&1 ; do
    echo "Waiting for other software managers to finish..."
    sleep 5
done

# In DigitalOcean, eth1 is typically the private network interface
PRIVATE_IP=$(ip -4 addr show eth1 | grep -oP '(?<=inet\s)\d+(\.\d+){3}')
echo "Detected Private IP: $PRIVATE_IP"

# Make .env file that Docker Compose will be using
echo "MONITOR_AND_LOGGING_PRIVATE_IP=$PRIVATE_IP" > /deploy/.env
echo "GF_SECURITY_ADMIN_USER=$GF_SECURITY_ADMIN_USER" >> /deploy/.env
echo "GF_SECURITY_ADMIN_PASSWORD=$GF_SECURITY_ADMIN_PASSWORD" >> /deploy/.env

# CONFIGURE FIREWALL
sudo ufw default deny incoming
sudo ufw default allow outgoing
sudo ufw allow ssh

# Change configurations in the sshd_config file to limit ssh retries, the graceperiod of a non authorized connection
# and disabling password authentication to reduce brute force attacks
echo "Setting a higher security baseline for SSH"
sudo sed -i 's/#PasswordAuthentication yes/PasswordAuthentication no/' /etc/ssh/sshd_config
sudo sed -i 's/#LoginGraceTime 2m/LoginGraceTime 30/' /etc/ssh/sshd_config
sudo sed -i 's/#MaxAuthTries 6/MaxAuthTries 3/' /etc/ssh/sshd_config
sudo sed -i 's/#MaxSessions 10/MaxSessions 5/' /etc/ssh/sshd_config

source /etc/profile.d/env.sh

# Allow Grafana UI (Public) - fine to allow since grafana is locked behind a user
sudo ufw allow 3000/tcp

# Allow applications private ip to access ports containing logs and monitoring
if [[ ! -z "$APP_SERVER_IP" ]]; then
    sudo ufw allow from "$APP_SERVER_IP" to any port 9200
    sudo ufw allow from "$APP_SERVER_IP" to any port 9090
    echo "Firewall: Monitoring ports opened for $APP_SERVER_IP"
fi

sudo ufw reload 
sudo ufw --force enable

echo -e "\nSelecting deploy Folder as default folder when you ssh into the server...\n"
echo "cd /deploy" >> ~/.bash_profile

sed -i 's/\r$//' /deploy/deploy.sh
chmod +x /deploy/deploy.sh
echo -e "\nVagrant setup done ..."