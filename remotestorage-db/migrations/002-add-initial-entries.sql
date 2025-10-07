INSERT INTO images (name, url, publication_year)
VALUES
  ('Sample Image 1', '/1', '2022-01-01'),
  ('Sample Image 2', '/2', '2023-02-15'),
  ('Sample Image 3', '/3', '2024-03-20')
ON CONFLICT DO NOTHING;
