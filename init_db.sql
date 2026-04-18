CREATE DATABASE minitwit CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

USE minitwit;

CREATE TABLE user (
  user_id integer primary key auto_increment,
  username varchar(100) not null,
  email varchar(200) null,
  pw_hash varchar(200) not null,
  UNIQUE INDEX (username)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE follower (
  who_id integer,
  whom_id integer,
  UNIQUE INDEX (who_id, whom_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE message (
  message_id integer primary key auto_increment,
  author_id integer not null,
  text varchar(500) not null,
  pub_date integer,
  flagged integer,
  INDEX (flagged, pub_date DESC),
  INDEX (author_id, flagged, pub_date DESC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE latest (
  id integer primary key auto_increment, 
  value integer not null
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
