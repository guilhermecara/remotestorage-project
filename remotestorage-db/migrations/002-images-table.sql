CREATE TABLE IF NOT EXISTS images (
    id SERIAL PRIMARY KEY,
    name TEXT NOT NULL,
    url TEXT NOT NULL,
    publication_year TIMESTAMP DEFAULT NOW()
);
