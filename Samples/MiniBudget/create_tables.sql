create table if not exists purchase (
	id bigserial primary key,
	amount NUMERIC(2),
	description varchar(256),
	purchased_at timestamp
);
