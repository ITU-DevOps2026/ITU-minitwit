PRAGMA foreign_keys=OFF;
BEGIN TRANSACTION;

CREATE TABLE user (
  user_id integer primary key autoincrement,
  username string not null,
  email string not null,
  pw_hash string not null
);
CREATE TABLE sqlite_sequence(name,seq);
CREATE TABLE follower (
  who_id integer,
  whom_id integer
);
CREATE TABLE message (
  message_id integer primary key autoincrement,
  author_id integer not null,
  text string not null,
  pub_date integer,
  flagged integer
);

DELETE FROM sqlite_sequence;
INSERT INTO sqlite_sequence VALUES('user',229);
INSERT INTO sqlite_sequence VALUES('message',11325);
COMMIT;

