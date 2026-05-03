set -e # Exit on error

# Wait for any remaining locks
while fuser /var/lib/apt/lists/lock >/dev/null 2>&1 ; do
    echo "Waiting for other software managers to finish..."
    sleep 5
done

# 1. GET THE PRIVATE IP
# In DigitalOcean, eth1 is typically the private network interface
PRIVATE_IP=$(ip -4 addr show eth1 | grep -oP '(?<=inet\s)\d+(\.\d+){3}')
echo "Detected Private IP: $PRIVATE_IP"
source /etc/profile.d/db_env.sh

# 2. CONFIGURE FIREWALL
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

# Only allow your application's private IP to access the DB port
if [ ! -z "$APP_SERVER_IP" ]; then
    sudo ufw allow from "$APP_SERVER_IP" to any port 3306
    echo "Firewall: Allowed 3306 for $APP_SERVER_IP"
fi

sudo ufw reload 
sudo ufw --force enable

# 3. RUN DOCKER BOUND TO PRIVATE IP
docker run -d \
    --name minitwit_mysql \
    --restart unless-stopped \
    -p $PRIVATE_IP:3306:3306 \
    --mount type=volume,source=minitwit_db_data,target=/var/lib/mysql \
    -e MYSQL_ROOT_PASSWORD="$DB_PASSWORD" \
    $1