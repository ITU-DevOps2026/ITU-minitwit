#Script for setting up Fail2Ban, by installing it from the apt library, and setting up our desired configuration.

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

wait_for_apt
sudo apt-get update
wait_for_apt
sudo apt install -y fail2ban

sudo cp /etc/fail2ban/jail.conf /etc/fail2ban/jail.local

sed -i 's/bantime  = 10m/bantime  = 24h /' /etc/fail2ban/jail.local
sed -i 's/findtime  = 10m/findtime  = 24h /' /etc/fail2ban/jail.local
sed -i 's/maxretry  = 5/maxretry  = 2 /' /etc/fail2ban/jail.local

sudo systemctl enable fail2ban
sudo systemctl start fail2ban
sudo systemctl status fail2ban