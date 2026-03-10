CREATE DATABASE minitwit CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

USE minitwit;

CREATE TABLE user (
  user_id integer primary key auto_increment,
  username varchar(100) not null,
  email varchar(200) null,
  pw_hash varchar(200) not null
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE follower (
  who_id integer,
  whom_id integer,
  INDEX (who_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE message (
  message_id integer primary key auto_increment,
  author_id integer not null,
  text varchar(500) not null,
  pub_date integer,
  flagged integer,
  INDEX (author_id),
  INDEX (pub_date DESC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
