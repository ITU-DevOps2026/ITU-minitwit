#!/bin/sh
if [ "$1" = "init" ]; then

    if [ -f "/tmp/minitwit.db" ]; then 
        echo "Database already exists."
        exit 1
    fi
    echo "Putting a database to /tmp/minitwit.db..."
    dotnet run init
elif [ "$1" = "inittmpdb" ]; then
    # Used by Vagrantfile
    if [ -f "/tmp/minitwit.db" ]; then 
        echo "Database already exists."
        exit 1
    fi
    echo "Putting a database to /tmp/minitwit.db..."
    # Vagrantfile needs to know specifically where its' dotnet is located
    # Therefore we write out the explicit path
    #export DOTNET_ROOT=/home/vagrant/.dotnet
    #sudo -u vagrant -E HOME=/home/vagrant $DOTNET_ROOT/dotnet run init
    sudo .dotnet/dotnet run init
elif [ "$1" = "startprod" ]; then
     echo "Starting minitwit with production webserver..."
     nohup dotnet run -c Release --urls "http://0.0.0.0:5035" > /tmp/out.log 2>&1 &
elif [ "$1" = "start" ]; then
    echo "Starting minitwit..."
    nohup dotnet run > /tmp/out.log 2>&1 &
elif [ "$1" = "stop" ]; then
    echo "Stopping minitwit..."
    pkill -f minitwit
elif [ "$1" = "inspectdb" ]; then
    ./flag_tool -i | less
elif [ "$1" = "flag" ]; then
    shift             # This removes "flag" from the arguments
    ./flag_tool "$@"
else
  echo "I do not know this command..."
fi
