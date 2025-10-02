CREATE TABLE IF NOT EXISTS images (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    content VARCHAR(255) NOT NULL,
    publication_year INTEGER
);
