# Migrating to another database

## Choice of database
- We will be using a SQL database as we want our data to be structed and consistent, which is a part of what SQL databases excel at, in comparison NOSQL allows us to store data in more loose fashions, which can be more scalable and better supports exposing it systems that will utilize it with algorithms, something we will not be doing. Source: https://www.integrate.io/blog/which-database/

- We are using a MySQL database, as it was one of the SQL database engines commonly recommended. We also considered Postgres, but after reading articles online it seems like Postgres has some extra advanced features which are good for complex queries and transactions, which we don't really have a use-case for in our application at the moment. Also, MySQL is supposedly easier to setup and can run a bit faster, since it is more lightweight compared to Postgres. Source: https://www.geeksforgeeks.org/mysql/difference-between-mysql-and-postgresql/

- We have decided to host our database on a seperate droplet within the same team, this means that the droplets will be running on the same virtual private cloud, allowing our application to connect to the database via private IP's, without exposing it to the public internet. We think that separating the application and the database on their own droplets will make it easier for us to get an overview of different metrics on Digital Ocean, like fx CPU usage etc., so that it is clearer which of our artifacts is the metrics relate to. 

## Trial of database change
-  We created a docker image for a MySQL database (in Dockerfile-mysql) based mainly on what was done in this repo: https://github.com/itu-devops/itu-minitwit-ci/tree/master. 

- In order to create this docker image, we needed an initialisation script for the database that creates all the tables. Since Sqlite and MySQL syntax is different (fx string is instead called varchar), we couldn't reuse the schema.sql file from the original application. We created a new file called init_db.sql which uses the correct MySQL syntax to create the tables. 

- The docker image loads this initialization script into /docker-entrypoint-initdb.d/, so the script is being run when the docker container is started. 

- We manually created a droplet on Digital Ocean (with the same configurations as our application, since those configurations seemed to be working fine for our application so far), and we then manually built and pushed the docker image for the database to Mathilde's Docker Hub repository. On the droplet, we installed Docker and pulled the database image from Mathilde's repository onto the droplet. 

- We then ran the following command on the droplet, which starts the MySQL server (Source: https://github.com/itu-devops/itu-minitwit-ci/blob/master/readme_dockerized.md): 

```bash
docker run --rm -d --name minitwit_mysql -e MYSQL_ROOT_PASSWORD=root <your_dockerhub_username>/itusqlimage
```

- We connected to the database to check that it was running correctly by using the following command: 

```bash
docker run --rm -it mysql:5.7 mysql -u root -proot -h <droplet's ip address> -D minitwit
```

- We could then verify that all our tables had been created successfully. We then seeded the database with some test data from this link: https://github.com/itu-devops/itu-minitwit-ci/blob/master/minitwit_init.sql

- In our application, we changed the database provider from Sqlite to MySQL by adding the following package to our .csproj file: 

````C#
<PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="9.0.0" />
````

- We then made changes to minitwit.cs, so that it connects to a MySQL database instead of Sqlite. DbPath is stored as an environment variable. 

```C#
builder.Services.AddDbContext<MinitwitContext>(options =>
  options.UseMySql(DbPath, ServerVersion.AutoDetect(DbPath)));
```

- We manually built and pushed a docker image of the application to Mathilde's Docker Hub repository for the test environment application, so that we could try to pull this image into the test environment droplet and connect it to the database droplet. 

- We then logged in to our droplet for the application's test environment (minitwit-test-env). We set an environment variable for the Database connection string on the droplet (Template for connection string: Server=myServerAddress;Port=1234;Database=myDataBase;Uid=myUsername;Pwd=myPassword;). The connection string can be found by running nano ~/.bash_profile. We also changed the compose.yaml file on the droplet, so that it had the correct environment variable set up. We then pulled the new docker image for the application onto the droplet and ran the deploy.sh script in order to get it up and running. 

- We could now verify that everything was up and running correctly, and that our application was connected to the new database. Although some characters in the tweets are not being processed correctly so we had tweets like this: "â€œIt will not be made out of sight over the side; more fell for that time, and that for you,â€™ said he, leaning back in Baker Street". This looks like an encoding issue, might be a problem with the data we used to seed the database or with the setup of the database itself. 

- Current next step would be to get a dump of the data in our production application and seed the database droplet with this data instead.

## The migration
- To fix the issues with text encoding, we found out that we needed to add the encoding of utf8mb4 to the database intialization script, so that the tables will be created with the correct encoding. We also found out that when running the script to seed the database, we had to write SET NAMES 'utf8mb4'; before all the insert statements, so that the rows will be inserted with the right encoding. 

- We set up a Vagrantfile to create a Digital Ocean droplet with a running MySQL database server. We then destroyed the droplet we had used for testing the database setup, and created a new droplet from this Vagrantfile instead. 

- We created a dump of our current production database in order to inspect the data and try out how complicated it would be to seed the new database with this data instead. This is the command to create a database dump: 

```
sqlite3 minitwit.db .dump > dump.sql
```

- We then used scp to copy the dump from the droplet to our local computer:

```bash
scp root@134.209.254.53:../data/dump.sql ./dump.sql
```

- We then had to do some data cleaning. Fx the dump contains so-called Byte Order Marks (invisible hex-code that some Windows text editors sometimes add to the text) which MySQL couldn't handle, so we had to delete those from the file. We also had to make sure that the dump only contains Insert-statements and not the scripts to create the tables (because these already exist in our database), so we used a command that compares the dump with a schema-file and only keeps the difference between these files. We also had to remove some lines related to Sqlite (inserting values into sqlite_sequence, which isn't a thing for MySQL). With help from Gemini, we combined all of this into the following command: 

```bash
sed -i '' 's/\xef\xbb\xbf//g' dump.sql && \ 
{ echo "SET NAMES 'utf8mb4';"; grep -vxF -f schema.sql dump.sql; } > data.sql && \ 
sed -i '' '/sqlite_sequence/d' data.sql
```

- Then we used scp to copy the data.sql file onto the database droplet:

```bash
scp data.sql root@<database ip address>:~/data.sql
```

- And finally we could ssh into the database droplet and run the following command to seed the database:

```bash
docker run --rm -i mysql:5.7 mysql -h <database ip address> -u root -proot minitwit < data.sql
```

- From this test run, we figured out that seeding the database with our production data took a lot of time (around 10 minutes), which means we would miss a lot of requests if we only did one data-migration. So we decided that when we were going to migrate the data to a production database, then we would do it in more batches - so first an initial seed with all the data, and then some smaller seeds with the rest of the data that had come into the database while the initial seed had been running. 

- So we repeated all of the above for our production database, and as soon as the initial seed was finished, we created a new dump of the sqlite database, and then used the following command in order to compare it with the old dump and only keep the new changes, so that the new dump could be loaded much quicker into the database:

```bash
sed -i '' 's/\xef\xbb\xbf//g' dump.sql && \ 
{ echo "SET NAMES 'utf8mb4';"; grep -vxF -f schema.sql dump.sql; } > data.sql && \ 
sed -i '' '/sqlite_sequence/d' data.sql && \ 
sort old_data.sql -o old_data_sorted.sql && \ 
sort data.sql -o data_sorted.sql && \ 
{ echo "SET NAMES 'utf8mb4';"; comm -13 old_data_sorted.sql data_sorted.sql; } > data.sql && \ 
scp data.sql root@164.92.189.173:~/data.sql
```

- On the database we could then again run the command that seeds the database, this time with a new and much smaller dump of data. 

- We then pushed our code changes into main, which triggers an automatic deployment to our production droplet, and while this deployment was finishing up, we did one last database dump that we loaded into the new database, so that we were as up to date as possible. We had however forgotten to fully update the connection string stored in the environment variables on the production droplet, so we had a couple of minutes where the application was down. From Helge's dashboard, it seems like we missed around 19 register requests in our switch from Sqlite to MySQL. 

## The hacking
- Some hours after the database switch, we noticed on the dashboard that we had suddenly missed hundreds of register request, and we also couldn't see any tweets when opening our minitwit application. When we checked the contents of our database, we could only see a single message which came from some hackers who had gotten access to our database and had deleted all of our data. They asked for some bitcoins in order to retrieve our data. 

- We decided to delete the database droplets (both our test environment and production environment databases had been hacked), and with assistance from Gemini, we created Vagrantfiles that could set up more secure droplets. We realized that our databases had been accessible from the public internet without any secure credentials, so it makes sense that some hackers/bots gained root access to our databases. 

- The new Vagrantfiles require you to set a secure database password in your own environment variables before creating the droplet. It also sets up a firewall which only allows access to the droplet via ssh and from the application's IP address. This ensures that the database is no longer accessible from the public internet, and that access to the database is also guarded by a secure password. 