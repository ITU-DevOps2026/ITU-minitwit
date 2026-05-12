# -*- mode: ruby -*-
# vi: set ft=ruby :
#
# Usage:
#   vagrant up                  # prompts for environment
#   DEPLOY_ENV=test vagrant up  # non-interactive
#   DEPLOY_ENV=prod vagrant up  # non-interactive

# Load environment variables from .env file if it exists
if File.exist?(".env")
  File.readlines(".env").each do |line|
    line.strip!
    next if line.empty? || line.start_with?("#")
    key, value = line.split("=", 2)
    value = value.strip.gsub(/\A["']|["']\z/, '') if value
    ENV[key] = value if key && value
  end
end

# Environment selection
if ENV['DEPLOY_ENV'].nil?
  print "Deploy to [test or prod]. Write prod to spin up production env, and test to spin up test env (default is: test): "
  input = $stdin.gets.chomp
  DEPLOY_ENV = %w[prod test].include?(input) ? input : "test"
else
  DEPLOY_ENV = ENV.fetch("DEPLOY_ENV", "test")
end

IS_PROD = DEPLOY_ENV == "prod"
puts "==> Deploying #{DEPLOY_ENV.upcase} environment"

require 'json'
require 'droplet_kit'

# Per-environment configuration
CFG = if IS_PROD
  {
    swarm_tag:          "minitwit-swarm",
    db_tag:             "Prod-DB",
    manager_count:      1,
    worker_count:       2,
    db_password:        ENV['PROD_DB_PASSWORD'],
    db_connection:      ENV['prod_db_connection'],
    manager_res_ip:     ENV['PROD_MANAGER_RES_IP'],
    db_res_ip:          ENV['PROD_DB_RES_IP'],
    monitoring_res_ip:  ENV['PROD_MONITORING_RES_IP'],
    db_image:           "mathildegk/minitwit-mysql-prod",
    # File paths for inter-droplet IP coordination (kept separate per env)
    manager_ip_file:    "./provision_scripts/prod_manager_IP",
    db_ip_file:         "./provision_scripts/prod_db_IP",
    monitoring_priv_ip_file: "./provision_scripts/prod_monitoring_private_IP",
    monitoring_pub_ip_file:  "./provision_scripts/prod_monitoring_public_IP",
    join_token_file:    "./provision_scripts/prod_join_token",
  }
else
  {
    swarm_tag:          "minitwit-swarm-test",
    db_tag:             "Test-DB",
    manager_count:      1,
    worker_count:       2,
    db_password:        ENV['TEST_DB_PASSWORD'],
    db_connection:      ENV['test_db_connection'],
    manager_res_ip:     ENV['TEST_MANAGER_RES_IP'],
    db_res_ip:          ENV['TEST_DB_RES_IP'],
    monitoring_res_ip:  ENV['TEST_MONITORING_RES_IP'],
    db_image:           "mathildegk/minitwit-mysql-test",
    manager_ip_file:    "./provision_scripts/test_manager_IP",
    db_ip_file:         "./provision_scripts/test_db_IP",
    monitoring_priv_ip_file: "./provision_scripts/test_monitoring_private_IP",
    monitoring_pub_ip_file:  "./provision_scripts/test_monitoring_public_IP",
    join_token_file:    "./provision_scripts/test_join_token",
  }
end

# SSH keys saved in the team on Digital Ocean — fetched once, shared by all droplets
token = ENV["DIGITAL_OCEAN_TOKEN"]
response = `curl -s -H "Authorization: Bearer #{token}" https://api.digitalocean.com/v2/account/keys?per_page=200`
keys_data = JSON.parse(response)
team_public_keys = keys_data["ssh_keys"]
  .map { |k| k["public_key"].strip }
  .join("\n")

# Tags — create once per run (DigitalOcean ignores duplicates)
client = DropletKit::Client.new(access_token: ENV['DIGITAL_OCEAN_TOKEN'])
client.tags.create(DropletKit::Tag.new(name: CFG[:swarm_tag]))
client.tags.create(DropletKit::Tag.new(name: CFG[:db_tag]))

# Helper: assign a DigitalOcean reserved IP to a droplet
def assign_reserved_ip(machine, reserved_ip, token)
  command = "curl -s http://169.254.169.254/metadata/v1/id"
  droplet_id = ""
  machine.communicate.execute(command) do |type, data| 
    droplet_id << data if type == :stdout
  end
  droplet_id = droplet_id.strip
  puts "Found droplet ID: #{droplet_id}"

  command = "curl -X POST -H 'Content-Type: application/json' -H 'Authorization: Bearer #{token}' -d '{\"type\":\"assign\",\"droplet_id\":#{droplet_id}}' 'https://api.digitalocean.com/v2/reserved_ips/#{reserved_ip}/actions'"
  machine.communicate.execute(command)
  puts "Successfully assigned reserved IP #{reserved_ip} to droplet #{droplet_id}"
end

# Helper: fetch a droplet's private IP via metadata
def fetch_private_ip(machine)
  command = "curl -s http://169.254.169.254/metadata/v1/interfaces/private/0/ipv4/address"
  ip = ""
  machine.communicate.execute(command) { |type, data| ip << data if type == :stdout }
  ip.strip
end

# Helper: fetch a droplet's public IP via metadata
def fetch_public_ip(machine)
  command = "curl -s http://169.254.169.254/metadata/v1/interfaces/public/0/ipv4/address"
  ip = ""
  machine.communicate.execute(command) { |type, data| ip << data if type == :stdout }
  ip.strip
end

# Helper: wait for files to exist, also takes argument for maximum wait time before error is thrown
def wait_for_files(label, *files, timeout_seconds: 300)
  deadline = Time.now + timeout_seconds
  loop do
    missing = files.reject { |f| File.exist?(f) }
    break if missing.empty?
    if Time.now > deadline
      abort "TIMEOUT after #{timeout_seconds}s — #{label} never received: #{missing.join(', ')}"
    end
    puts "#{label}: waiting for #{missing.join(', ')}..."
    sleep 5
  end
end

# Vagrant general configuration for all droplets
Vagrant.configure("2") do |config|
  config.vm.box     = 'digital_ocean'
  config.vm.box_url = "https://github.com/devopsgroup-io/vagrant-digitalocean/raw/master/box/digital_ocean.box"
  config.ssh.private_key_path = '~/.ssh/id_ed25519'
  config.vm.synced_folder ".", "/vagrant", disabled: true

  config.vm.provider :digital_ocean do |provider|
    provider.ssh_key_name = ENV["SSH_KEY_NAME"]
    provider.token        = ENV["DIGITAL_OCEAN_TOKEN"]
    provider.image        = 'ubuntu-22-04-x64'
    provider.region       = 'fra1'
    provider.size         = 's-1vcpu-1gb'
    provider.privatenetworking = true
  end

  # Manager
  (1..CFG[:manager_count]).each do |i|
    config.vm.define "minitwit-#{DEPLOY_ENV}-manager-#{i}", autostart: true, primary: true do |manager|
      manager.vm.provider :digital_ocean do |provider|
        provider.tags = [CFG[:swarm_tag]]
      end
      manager.vm.hostname = "minitwit-#{DEPLOY_ENV}-manager-#{i}"
      manager.vm.synced_folder "remote_files/#{DEPLOY_ENV}_env", "/minitwit", type: "rsync"

      # Save own private IP and assign reserved IP
      manager.trigger.before :"Vagrant::Action::Builtin::Provision", type: :action do |t|
        t.info = "Writing own IP to file and assigning reserved IP"
        t.ruby do |env, machine|
          ip = fetch_private_ip(machine)
          File.write(CFG[:manager_ip_file], ip)
          puts "Successfully saved Manager IP: #{ip}"

          assign_reserved_ip(machine, CFG[:manager_res_ip], ENV['DIGITAL_OCEAN_TOKEN'])
        end
      end

      # Wait for DB and monitoring, then inject IPs into bash_profile
      manager.trigger.before :"Vagrant::Action::Builtin::Provision", type: :action do |t|
        t.info = "Collecting database and monitoring IPs and injecting them into manager"
        t.ruby do |env, machine|
          wait_for_files("Manager", CFG[:db_ip_file], CFG[:monitoring_priv_ip_file], CFG[:monitoring_pub_ip_file])

          db_ip            = File.read(CFG[:db_ip_file]).strip
          monitor_priv_ip  = File.read(CFG[:monitoring_priv_ip_file]).strip
          monitor_pub_ip   = File.read(CFG[:monitoring_pub_ip_file]).strip
          final_db_conn    = CFG[:db_connection].gsub("<db_IP>", db_ip)
          docker_user      = ENV['DOCKER_USERNAME'] || ""

          commands = [
            "echo 'export db_connection=\"#{final_db_conn}\"' > ~/.bash_profile",
            "echo 'export MONITOR_AND_LOGGING_PRIVATE_IP=\"#{monitor_priv_ip}\"' >> ~/.bash_profile",
            "echo 'export MONITOR_AND_LOGGING_PUBLIC_IP=\"#{monitor_pub_ip}\"' >> ~/.bash_profile",
            "echo 'export DOCKER_USERNAME=\"#{docker_user}\"' >> ~/.bash_profile",
          ]
          commands.each { |cmd| machine.communicate.execute(cmd) }
          puts "Successfully injected DB and Monitoring IPs into ~/.bash_profile"
        end
      end

      manager.vm.provision "shell", path: "provision_scripts/docker_setup_script.sh", binary: false
      manager.vm.provision "shell", path: "provision_scripts/manager_setup_script.sh", binary: false
      manager.vm.provision "shell", path: "provision_scripts/configure_firewall_manager.sh", binary: false
      manager.vm.provision "shell", path: "provision_scripts/fail2ban_setup_script.sh", binary: false
      manager.vm.provision "shell", name: "inject_ssh_keys", inline: <<-SHELL
        echo "#{team_public_keys}" >> /root/.ssh/authorized_keys
        echo "SSH keys injected"
      SHELL

      # Init swarm, save join token, deploy stack
      manager.trigger.after :up do |t|
        t.info = "Initialising swarm on manager and fetching worker token"
        t.ruby do |env, machine|
          manager_ip = fetch_private_ip(machine)

          machine.communicate.execute("docker swarm init --advertise-addr #{manager_ip} || true")

          join_token = ""
          machine.communicate.execute("docker swarm join-token -q worker") do |type, data|
            join_token << data if type == :stdout
          end
          join_token.strip!
          File.write(CFG[:join_token_file], join_token)
          puts "Successfully saved join token: #{join_token}"

          machine.communicate.execute("./deploy.sh")
        end
      end
    end
  end

  # Workers
  (1..CFG[:worker_count]).each do |i|
    config.vm.define "minitwit-#{DEPLOY_ENV}-worker-#{i}", autostart: true, primary: true do |worker|
      worker.vm.provider :digital_ocean do |provider|
        provider.tags = [CFG[:swarm_tag]]
      end
      worker.vm.hostname = "minitwit-#{DEPLOY_ENV}-worker-#{i}"

      worker.vm.provision "shell", path: "provision_scripts/docker_setup_script.sh", binary: false
      worker.vm.provision "shell", path: "provision_scripts/configure_firewall_worker.sh", binary: false
      worker.vm.provision "shell", path: "provision_scripts/fail2ban_setup_script.sh", binary: false
      worker.vm.provision "shell", name: "inject_ssh_keys", inline: <<-SHELL
        echo "#{team_public_keys}" >> /root/.ssh/authorized_keys
        echo "SSH keys injected"
      SHELL

      worker.trigger.after [:up, :provision] do |t|
        t.info = "Joining swarm created by manager"
        t.ruby do |env, machine|
          wait_for_files("Worker-#{i}", CFG[:manager_ip_file], CFG[:join_token_file], timeout_seconds: 300)

          manager_ip  = File.read(CFG[:manager_ip_file]).strip
          join_token  = File.read(CFG[:join_token_file]).strip

          puts "minitwit-#{DEPLOY_ENV}-worker-#{i} joining at #{manager_ip} with token #{join_token}"
          machine.communicate.execute("sudo docker swarm join --token #{join_token} #{manager_ip}:2377") do |type, data|
            puts data if type == :stdout
          end
        end
      end
    end
  end

  # Database
  config.vm.define "minitwit-#{DEPLOY_ENV}-mysql", autostart: true, primary: true do |server|
    server.vm.provider :digital_ocean do |provider|
      provider.tags = [CFG[:db_tag]]
    end
    server.vm.hostname = "minitwit-#{DEPLOY_ENV}-mysql"

    server.vm.provision "shell", inline: <<-SHELL
      echo "export DB_PASSWORD='#{CFG[:db_password]}'" >> /etc/profile.d/db_env.sh
      chmod +x /etc/profile.d/db_env.sh
    SHELL
    server.vm.provision "shell", path: "provision_scripts/docker_setup_script.sh", binary: false
    server.vm.provision "shell", path: "provision_scripts/database_setup_script.sh", binary: false, args: CFG[:db_image]
    server.vm.provision "shell", path: "provision_scripts/fail2ban_setup_script.sh", binary: false
    server.vm.provision "shell", name: "inject_ssh_keys", inline: <<-SHELL
      echo "#{team_public_keys}" >> /root/.ssh/authorized_keys
      echo "SSH keys injected"
    SHELL

    server.trigger.before :"Vagrant::Action::Builtin::Provision", type: :action do |t|
      t.info = "Writing own IP to file, waiting for manager IP, assigning reserved IP"
      t.ruby do |env, machine|
        db_ip = fetch_private_ip(machine)
        File.write(CFG[:db_ip_file], db_ip)
        puts "Successfully saved Database IP: #{db_ip}"

        wait_for_files("DB", CFG[:manager_ip_file])

        manager_ip = File.read(CFG[:manager_ip_file]).strip
        machine.communicate.execute("echo 'export APP_SERVER_IP=\"#{manager_ip}\"' | sudo tee -a /etc/profile.d/db_env.sh")
        puts "Successfully injected Manager IP into db_env.sh"

        assign_reserved_ip(machine, CFG[:db_res_ip], ENV['DIGITAL_OCEAN_TOKEN'])
      end
    end
  end

  # Monitoring & Logging
  config.vm.define "minitwit-#{DEPLOY_ENV}-monitoring-and-logging", autostart: true, primary: true do |server|
    server.vm.hostname = "minitwit-#{DEPLOY_ENV}-monitoring-and-logging"
    server.vm.synced_folder "remote_files/monitoring_and_logging", "/deploy",     type: "rsync"
    server.vm.synced_folder "monitoring",                          "/monitoring",  type: "rsync"
    server.vm.synced_folder "logging",                             "/logging",     type: "rsync"

    server.trigger.before :"Vagrant::Action::Builtin::Provision", type: :action do |t|
      t.info = "Writing own IPs to files, waiting for manager IP, assigning reserved IP"
      t.ruby do |env, machine|
        priv_ip = fetch_private_ip(machine)
        pub_ip  = fetch_public_ip(machine)

        File.write(CFG[:monitoring_priv_ip_file], priv_ip)
        File.write(CFG[:monitoring_pub_ip_file],  pub_ip)
        puts "Saved Monitoring IPs — private: #{priv_ip}, public: #{pub_ip}"

        wait_for_files("Monitoring", CFG[:manager_ip_file])

        manager_ip = File.read(CFG[:manager_ip_file]).strip
        machine.communicate.execute("echo 'export APP_SERVER_IP=\"#{manager_ip}\"' | sudo tee /etc/profile.d/env.sh")
        puts "Successfully injected Manager IP into env.sh"

        machine.communicate.execute("echo 'export GF_SECURITY_ADMIN_USER=\"#{GF_SECURITY_ADMIN_USER}\"' | sudo tee /etc/profile.d/env.sh")
        machine.communicate.execute("echo 'export GF_SECURITY_ADMIN_PASSWORD=\"#{GF_SECURITY_ADMIN_PASSWORD}\"' | sudo tee /etc/profile.d/env.sh")
        puts "Successfully injected Grafana User credentials into env.sh"

        assign_reserved_ip(machine, CFG[:monitoring_res_ip], ENV['DIGITAL_OCEAN_TOKEN'])
      end
    end

    server.vm.provision "shell", inline: <<-SHELL
      if [ ! -f /swapfile ]; then
        echo "Creating 1GB swap file..."
        sudo fallocate -l 1G /swapfile
        sudo chmod 600 /swapfile
        sudo mkswap /swapfile
        sudo swapon /swapfile
        echo '/swapfile none swap sw 0 0' | sudo tee -a /etc/fstab
        echo "Swap created."
      else
        echo "Swap file already exists."
      fi
      sudo sysctl vm.swappiness=1
    SHELL

    server.vm.provision "shell", inline: <<-SHELL
      chmod +x /etc/profile.d/env.sh
      source /etc/profile.d/env.sh
      PROM_CONFIG="/monitoring/prometheus/prometheus.yml"
      if [ -f "$PROM_CONFIG" ]; then
        sed -i "s/APP_IP_PLACEHOLDER/$APP_SERVER_IP/g" "$PROM_CONFIG"
        echo "Injected $APP_SERVER_IP into $PROM_CONFIG"
      fi
    SHELL

    server.vm.provision "shell", path: "provision_scripts/docker_setup_script.sh", binary: false
    server.vm.provision "shell", path: "provision_scripts/monitoring_setup_script.sh",binary: false
    server.vm.provision "shell", path: "provision_scripts/fail2ban_setup_script.sh", binary: false
    server.vm.provision "shell", name: "inject_ssh_keys", inline: <<-SHELL
      echo "#{team_public_keys}" >> /root/.ssh/authorized_keys
      echo "SSH keys injected"
    SHELL

    server.trigger.after :up do |t|
      t.ruby do |env, machine|
        machine.communicate.execute("./deploy.sh")
      end
    end
  end
end